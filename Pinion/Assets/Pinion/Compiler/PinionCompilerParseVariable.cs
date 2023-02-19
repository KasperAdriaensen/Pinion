using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			bool isArray = false;
			int arraySize = 0;
			string arrayInitializerString = null;

			if (typeIdentifier.EndsWith(CompilerConstants.ArrayIndexerEmpty))
			{
				typeIdentifier = typeIdentifier.Substring(0, typeIdentifier.Length - CompilerConstants.ArrayIndexerEmpty.Length);
				isArray = true;

				if (string.IsNullOrEmpty(valueExpression))
				{
					AddCompileError("Array declaration requires a third argument: either an array size (e.g. 'set(string[], $myArray, 2)') or an initializer (e.g. 'set(string[], $myArray, {\"hello\", \"world\"})').");
					return;
				}

				// matches pattern "{x,y,z,...}"
				Match initializerMatch = Regex.Match(valueExpression, CompilerRegex.arrayInitializerRegex);

				if (initializerMatch.Success)
				{
					arrayInitializerString = initializerMatch.Groups[1].Value; // remove curly braces
				}
				else if (int.TryParse(valueExpression, out int result))
				{
					arraySize = result;
					if (arraySize < 1)
					{
						AddCompileError("Array size must greater than 0.");
						return;
					}
				}
				else
				{
					AddCompileError("Could not interpret third argument for array declaration. Must be either an array size (e.g. 'set(string[], $myArray, 2)') or an initializer (e.g. 'set(string[], $myArray, {\"hello\", \"world\"})').");
					return;
				}
			}

			// Don't do this earlier. We want to remove the array indexer first, if present.
			Type variableType = PinionTypes.GetTypeFromPinionName(typeIdentifier);

			if (variableType == null)
			{
				AddCompileError($"Invalid variable declaration. Unknown type: {typeIdentifier}. Variable declarion is structured: declare(type, $variableName, [default value]).");
				return;
			}

			ushort index = 0;

			switch (typeIdentifier) // can't currently do a switch on types yet
			{
				case "int":
					if (isArray)
					{
						int[] initValues = null;

						if (!string.IsNullOrEmpty(arrayInitializerString))
						{
							initValues = InitializerToArray<int>(arrayInitializerString, ParseLiteralInt);
						}
						else if (arraySize > 0)
						{
							initValues = Enumerable.Repeat<int>(default(int), arraySize).ToArray();
						}

						if (targetContainer.IntRegister.RegisterArray(out index, initValues))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<int>(index, initValues.Length));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.IntRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(int))}.");
							return;
						}
					}
					else
					{
						if (targetContainer.IntRegister.RegisterValue(out index, false, externalVariableName))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<int>(index));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.IntRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(int))}.");
							return;
						}
					}
					break;

				case "float":
					if (isArray)
					{
						float[] initValues = null;

						if (!string.IsNullOrEmpty(arrayInitializerString))
						{
							initValues = InitializerToArray<float>(arrayInitializerString, ParseLiteralFloat);
						}
						else if (arraySize > 0)
						{
							initValues = Enumerable.Repeat<float>(default(float), arraySize).ToArray();
						}

						if (targetContainer.FloatRegister.RegisterArray(out index, initValues))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<float>(index, initValues.Length));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.FloatRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(float))}.");
							return;
						}
					}
					else
					{
						if (targetContainer.FloatRegister.RegisterValue(out index, false, externalVariableName))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<float>(index));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.FloatRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(float))}.");
							return;
						}
					}

					break;

				case "bool":
					if (isArray)
					{
						bool[] initValues = null;

						if (!string.IsNullOrEmpty(arrayInitializerString))
						{
							initValues = InitializerToArray<bool>(arrayInitializerString, ParseLiteralBool);
						}
						else if (arraySize > 0)
						{
							initValues = Enumerable.Repeat<bool>(default(bool), arraySize).ToArray();
						}

						if (targetContainer.BoolRegister.RegisterArray(out index, initValues))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<bool>(index, initValues.Length));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.BoolRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(bool))}.");
							return;
						}
					}
					else
					{
						if (targetContainer.BoolRegister.RegisterValue(out index, false, externalVariableName))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<bool>(index));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.BoolRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(bool))}.");
							return;
						}
					}
					break;

				case "string":
					if (isArray)
					{
						string[] initValues = null;

						if (!string.IsNullOrEmpty(arrayInitializerString))
						{
							initValues = InitializerToArray<string>(arrayInitializerString, ParseLiteralString);
						}
						else if (arraySize > 0)
						{
							// declare with default string.empty, so we don't have to care about null refs in a language where null isn't really a thing
							initValues = Enumerable.Repeat<string>(string.Empty, arraySize).ToArray();
						}

						if (targetContainer.StringRegister.RegisterArray(out index, initValues))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<string>(index, initValues.Length));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.StringRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(string))}.");
							return;
						}
					}
					else
					{
						// declare with default string.empty, so we don't have to care about null refs in a language where null isn't really a thing
						if (targetContainer.StringRegister.RegisterValue(string.Empty, out index, false, externalVariableName))
						{
							variableNameToPointerMappings.Add(variableName, new VariablePointer<string>(index));
						}
						else
						{
							AddCompileError($"Exceeded maximum number ({targetContainer.StringRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(string))}.");
							return;
						}
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
			if (!isArray && !string.IsNullOrEmpty(valueExpression))
				ParseVariableWrite(targetContainer, variableName, valueExpression);
		}

		private static CompilerArgument ParseVariableRead(PinionContainer targetContainer, Token token, List<ushort> output)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing variable read: \'{token}\'");
#endif
			string variableString = token.text;

			// Test: is array name followed by "[int]"?.
			Match arrayIndexerMatch = Regex.Match(variableString, CompilerRegex.arrayIndexerRegex);
			bool accessingArray = false;
			//	int arrayIndex = 0;

			if (arrayIndexerMatch.Success)
			{
				string indexExpression = arrayIndexerMatch.Groups[1].Value; // everything between [ and ]

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Parsing array index expression: \'{indexExpression}\'");
#endif

				CompilerArgument indexerReturnValue = ParseExpression(targetContainer, indexExpression);

				if (!compileSuccess) // If indexExpression could not be parsed, compilation will already have failed with a (hopefully) meaningful message.
					return CompilerArgument.Invalid;

				if (indexerReturnValue.argumentType != typeof(int)) // return is null or something other than expectedType
				{
					if (indexerReturnValue.IsArgumentTypeVoid)
					{
						AddCompileError(string.Format($"Expression used as array index returns no value."));
					}
					else
					{
						AddCompileError(string.Format($"Cannot use {PinionTypes.GetPinionNameFromType(indexerReturnValue.argumentType)} as array index. Array index must be of type {PinionTypes.GetPinionNameFromType(typeof(int))}."));
					}

					return CompilerArgument.Invalid;
				}

				accessingArray = true;
				variableString = variableString.Substring(0, variableString.Length - arrayIndexerMatch.Length); // remove indexer
			}

			// The process for declaring, defining and retrieving variables is largely the same as that for literals, with the exception that the same register index is used repeatedly,
			// rather than a unique index per literal. For an in depth explanation of the process, look at literal parsing.

			if (!IsValidVariableName(variableString))
			{
				AddCompileError($"Invalid variable name: {variableString}. Name must prefixed with $ or $$ (for system variables). Variable name must start with a letter, followed by any amount of alphanumeric characters.");
				return CompilerArgument.Invalid;
			}

			if (!IsVariableDeclared(variableString))
			{
				AddCompileError($"Undeclared variable: {variableString}. A variable must be declared before it can be referenced.");
				return CompilerArgument.Invalid;
			}

			IVariablePointer pointer = variableNameToPointerMappings[variableString];

			if (accessingArray)
			{
				if (!pointer.IsArray)
				{
					AddCompileError($"Variable {variableString} is not an array.");
					return CompilerArgument.Invalid;
				}
			}

			output.Add(pointer.GetReadInstruction().instructionCode);
			output.Add(pointer.GetIndexInRegister());

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsed variable read: variable {variableString} of type {pointer.GetValueType().ToString()}.");
#endif

			return new CompilerArgument(pointer, token);
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

		private static void ParseVariableWrite(PinionContainer targetContainer, string variableToken, string valueExpression)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing variable read: \'{variableToken}\'");
#endif
			// Test: is array name followed by "[int]"?.
			Match arrayIndexerMatch = Regex.Match(variableToken, CompilerRegex.arrayIndexerRegex);
			bool accessingArray = false;
			//	int arrayIndex = 0;

			if (arrayIndexerMatch.Success)
			{
				string indexExpression = arrayIndexerMatch.Groups[1].Value; // everything between [ and ]

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Parsing array index expression: \'{indexExpression}\'");
#endif

				CompilerArgument indexerReturnValue = ParseExpression(targetContainer, indexExpression);

				if (!compileSuccess) // If indexExpression could not be parsed, compilation will already have failed with a (hopefully) meaningful message.
					return;

				if (indexerReturnValue.argumentType != typeof(int)) // return is null or something other than expectedType
				{
					if (indexerReturnValue.IsArgumentTypeVoid)
					{
						AddCompileError(string.Format($"Expression used as array index returns no value."));
					}
					else
					{
						AddCompileError(string.Format($"Cannot use {PinionTypes.GetPinionNameFromType(indexerReturnValue.argumentType)} as array index. Array index must be of type {PinionTypes.GetPinionNameFromType(typeof(int))}."));
					}

					return;
				}

				accessingArray = true;
				variableToken = variableToken.Substring(0, variableToken.Length - arrayIndexerMatch.Length); // remove indexer
			}

			if (!IsValidVariableName(variableToken))
			{
				AddCompileError($"Invalid variable name: {variableToken}. Name must prefixed with $ or $$ (for system variables). Variable name must start with a letter, followed by any amount of alphanumeric characters.");
				return;
			}

			if (!IsVariableDeclared(variableToken))
			{
				AddCompileError($"Undeclared variable: {variableToken}. A variable must be declared before it can be used.");
				return;
			}

			IVariablePointer pointer = variableNameToPointerMappings[variableToken];
			CompilerArgument returnValue = ParseExpression(targetContainer, valueExpression);

			if (!compileSuccess) // if valueExpression could not be parsed, compilation will already have failed with a (hopefully) meaningful message
				return;

			System.Type expectedType = pointer.GetValueType();
			if (returnValue.argumentType != expectedType) // return is null or something other than expectedType
			{
				if (returnValue.IsArgumentTypeVoid)
				{
					AddCompileError(string.Format($"Expression in the variable assignment returns no value."));
				}
				else
				{
					AddCompileError(string.Format($"Cannot assign a value of type {PinionTypes.GetPinionNameFromType(returnValue.argumentType)} to variable {variableToken}, of type {PinionTypes.GetPinionNameFromType(expectedType)}."));
				}

				return;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing variable write. Writing '{valueExpression}' to variable {variableToken}.");
#endif
			if (accessingArray)
			{
				if (!pointer.IsArray)
				{
					AddCompileError($"Variable {variableToken} is not an array.");
					return;
				}
			}

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

		private delegate bool InitializerParser<T>(string token, out T returnValue);

		private static T[] InitializerToArray<T>(string initializerString, InitializerParser<T> parser)
		{
			if (string.IsNullOrEmpty(initializerString))
			{
				return null;
			}

			List<T> values = new List<T>();

			// split either on a quoted string or on a comma
			string[] split = Regex.Split(initializerString, $"({CompilerRegex.arrayInitializerSplitRegex2})"); // Surround regex with a capture group so the split string *also* includes the delimiters themselves.
			StringBuilder builder = new StringBuilder();

			// bool inString = false;
			// string completedString = null;

			// for (int i = 0; i < initializerString.Length; i++)
			// {
			// 	char currentChar = initializerString[i];
			// 	completedString = null;

			// 	// Final argument
			// 	if (i == initializerString.Length - 1)
			// 	{
			// 		if (currentChar == ',')
			// 		{
			// 			AddCompileError($"Missing final item in initializer.");
			// 			break;
			// 		}

			// 		builder.Append(currentChar);
			// 		completedString = builder.ToString();
			// 	}
			// 	else if (currentChar == ',' && !inString)
			// 	{
			// 		completedString = builder.ToString();
			// 		builder.Clear();
			// 	}
			// 	else if (currentChar == '"')
			// 	{
			// 		inString = !inString;
			// 		completedString = builder.ToString();
			// 		builder.Clear();
			// 		builder.Append(currentChar);
			// 	}
			// 	else
			// 	{
			// 		builder.Append(currentChar);
			// 	}

			// 	if (completedString != null)
			// 		AddItem(completedString);

			// 	if (!compileSuccess)
			// 		break;
			// }

			for (int i = 0; i < split.Length; i++)
			{
				string currentString = split[i];

				if (string.IsNullOrEmpty(currentString))
					continue;

				if (currentString == PinionCompiler.ArgSeparator)
				{
					if (i == initializerString.Length - 1)
					{
						AddCompileError($"Missing final item in initializer.");
						break;
					}

					continue;
				}

				AddItem(currentString);

				if (!compileSuccess)
					break;
			}

			return values.ToArray();

			// Nested function
			void AddItem(string input)
			{
				if (parser(input, out T resultValue))
				{
					values.Add(resultValue);
				}
				else
				{
					AddCompileError($"Could not parse '{input}' as {PinionTypes.GetPinionNameFromType(typeof(T))}. Array initializer can contain literal values only.");
				}
			}
		}
	}
}
