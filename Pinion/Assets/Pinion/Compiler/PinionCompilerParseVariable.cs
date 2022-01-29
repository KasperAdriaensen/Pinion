using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pinion.Compiler.Internal;
using UnityEngine;

namespace Pinion.Compiler
{
	public partial class PinionCompiler
	{
		private static Dictionary<string, IVariablePointer> variableNameToPointerMappings = new Dictionary<string, IVariablePointer>();

		private static void ParseVariableDeclaration(PinionContainer targetContainer, string argumentsString)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("[PinionCompiler] Parsing variable declaration: " + argumentsString);
#endif
			// nested method
			string ReadUntilNextChar(string input, char stopAt, out string stringRemaining)
			{
				string output = string.Empty;
				stringRemaining = input;

				if (!string.IsNullOrWhiteSpace(input))
				{
					int charLocation = input.IndexOf(stopAt);

					if (charLocation > 0)
					{
						output = input.Substring(0, charLocation);

						if (charLocation + 1 < input.Length)
							stringRemaining = input.Substring(charLocation + 1);
						else
							stringRemaining = string.Empty;
					}
					else
					{
						stringRemaining = string.Empty;
						output = input;
					}
				}
				else
				{
					stringRemaining = string.Empty;
					output = string.Empty;
				}

				return output;
			}

			string remainingArgsString = argumentsString;
			string typeIdentifier = ReadUntilNextChar(remainingArgsString, ',', out remainingArgsString);
			string variableName = ReadUntilNextChar(remainingArgsString, ',', out remainingArgsString);
			string valueExpression = remainingArgsString;

			if (string.IsNullOrEmpty(typeIdentifier) || string.IsNullOrEmpty(variableName))
			{
				AddCompileError($"Invalid variable declaration. Incorrect number of arguments. Arguments are: \"{argumentsString}\".");
				return;
			}

			// Only used to store this external value "by name". Kept with two $$, just so the external code visually matches the script code.
			// Otherwise treated identically.
			string externalVariableName = null;

			// If the second '$' was removed, this is an external variable name.
			// Beyond, external and internal variables are treated identically. $$myVar and $myVar will give a name conflict.
			if (IsExternalVariable(variableName))
			{
				externalVariableName = variableName;

				//TODO: This should be passed in a much better way.
				if (PinionCompiler.CurrentBlockContext != ScriptBlock.InitBlock)
				{
					AddCompileError($"External variables can only be declared in an {ScriptBlock.InitBlock}.");
					return;
				}
			}

			if (!IsValidVariableName(variableName))
			{
				AddCompileError($"Invalid variable name: {variableName}. Name must prefixed with $ or $$ (for system variables). Variable name must start with a letter, followed by any amount of alphanumeric characters.");
				return;
			}

			if (IsVariableDeclared(variableName))
			{
				AddCompileError($"A variable with the name \"{variableName}\" was already declared.");
				return;
			}

			ushort index = 0;

			switch (typeIdentifier)
			{

				case "int":
					if (targetContainer.IntRegister.RegisterValue(out index, false, externalVariableName))
					{
						variableNameToPointerMappings.Add(variableName, new VariablePointer<int>(index));
					}
					else
					{
						AddCompileError($"Exceeded maximum number ({targetContainer.IntRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(string))}.");
						return;
					}
					break;

				case "float":
					if (targetContainer.FloatRegister.RegisterValue(out index, false, externalVariableName))
					{
						variableNameToPointerMappings.Add(variableName, new VariablePointer<float>(index));
					}
					else
					{
						AddCompileError($"Exceeded maximum number ({targetContainer.FloatRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(float))}.");
						return;
					}

					break;

				case "bool":
					if (targetContainer.BoolRegister.RegisterValue(out index, false, externalVariableName))
					{
						variableNameToPointerMappings.Add(variableName, new VariablePointer<bool>(index));
					}
					else
					{
						AddCompileError($"Exceeded maximum number ({targetContainer.BoolRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(bool))}.");
						return;
					}
					break;

				case "string":
					// declare with default string.empty, so we don't have to care about null refs in a language where null isn't really a thing
					if (targetContainer.StringRegister.RegisterValue(string.Empty, out index, false, externalVariableName))
					{
						variableNameToPointerMappings.Add(variableName, new VariablePointer<string>(index));
					}
					else
					{
						AddCompileError($"Exceeded maximum number ({targetContainer.StringRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(string))}.");
						return;
					}

					break;

				default:
					AddCompileError(string.Format("Invalid variable declaration. Unknown type: {0}.", typeIdentifier));
					return;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.LogFormat("[PinionCompiler] Declared variable \"{0}\" in register for type \"{1}\".", variableName, typeIdentifier);
#endif

			// optional default value
			if (!string.IsNullOrEmpty(valueExpression))
				ParseVariableWrite(targetContainer, variableName, valueExpression);
		}

		private static Type ParseVariableRead(string variableName, List<ushort> output)
		{
			// The process for declaring, defining and retrieving variables is largely the same as that for literals, with the exception that the same register index is used repeatedly,
			// rather than a unique index per literal. For an in depth explanation of the process, look at literal parsing.

			if (!IsValidVariableName(variableName))
			{
				AddCompileError($"Invalid variable name: {variableName}. Name must prefixed with $ or $$ (for system variables). Variable name must start with a letter, followed by any amount of alphanumeric characters.");
				return null;
			}

			if (!IsVariableDeclared(variableName))
			{
				AddCompileError($"Undeclared variable: {variableName}. A variable must be declared before it can be referenced.");
				return null;
			}

			IVariablePointer pointer = variableNameToPointerMappings[variableName];

			output.Add(pointer.GetReadInstruction().instructionCode);
			output.Add(pointer.GetIndexInRegister());

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsed variable read: variable {variableName} of type {pointer.GetValueType().ToString()}.");
#endif

			return pointer.GetValueType();
		}

		private static void ParseVariableAssign(PinionContainer targetContainer, string argumentsString)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("[PinionCompiler] Parsing variable assign: " + argumentsString);
#endif
			// This is not as a clean as usual. We can't split by the comma because there could be a comma in a sub-expression of the assignment part.
			// Problematic? Solution?
			string variableName = argumentsString.Substring(0, argumentsString.IndexOf(',')); // everything before first , is variable name
			string otherArguments = argumentsString.Remove(0, variableName.Length + 1); // length + 1 to also remove comma

			ParseVariableWrite(targetContainer, variableName, otherArguments);
		}

		private static void ParseVariableWrite(PinionContainer targetContainer, string variableName, string valueExpression)
		{
			if (!IsValidVariableName(variableName))
			{
				AddCompileError($"Invalid variable name: {variableName}. Name must prefixed with $ or $$ (for system variables). Variable name must start with a letter, followed by any amount of alphanumeric characters.");
				return;
			}

			if (!IsVariableDeclared(variableName))
			{
				AddCompileError($"Undeclared variable: {variableName}. A variable must be declared before it can be used.");
				return;
			}

			IVariablePointer pointer = variableNameToPointerMappings[variableName];
			Type returnType = ParseExpression(targetContainer, valueExpression);

			if (!compileSuccess) // if valueExpression could not be parsed, compilation will already have failed with a (hopefully) meaningful message
				return;

			System.Type expectedType = pointer.GetValueType();
			if (returnType != expectedType) // return is null or something other than expectedType
			{
				if (returnType == typeof(void))
					AddCompileError(string.Format($"Expression in the variable assignment returns no value."));
				else
					AddCompileError(string.Format($"Cannot assign a value of type {TypeNameShortHands.GetSimpleTypeName(returnType)} to variable {variableName}, of type {TypeNameShortHands.GetSimpleTypeName(expectedType)}."));
				return;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing variable write. Writing '{valueExpression}' to variable {variableName}.");
#endif

			targetContainer.scriptInstructions.Add(pointer.GetWriteInstruction().instructionCode);
			targetContainer.scriptInstructions.Add(pointer.GetIndexInRegister());
		}

		private static bool IsVariableDeclared(string variableName)
		{
			return variableNameToPointerMappings.ContainsKey(variableName);
		}

		private static bool IsValidVariableName(string variableName)
		{
			// for details of constraints, see declaration of regex pattern
			return Regex.IsMatch(variableName, CompilerRegex.validVariableNameRegex);
		}

		private static bool IsExternalVariable(string input)
		{
			return input.StartsWith("$$");
		}
	}
}
