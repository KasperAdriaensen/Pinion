using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Pinion.Compiler.Internal;
using Pinion.Internal;
using UnityEngine;

namespace Pinion.Compiler
{
	public partial class PinionCompiler
	{
		private static CompilerArgument ParseLiteral(PinionContainer targetContainer, Token token, List<ushort> output)
		{
			// General process for a valid literal.
			// Add a bytecode instruction indicating the next instruction code is to be interpreted as a literal. The associated function will know how to interpret that.
			// For float, string, and int: the next instruction is an index in the respective register.
			// For bool, the next instruction is either 0 (false) or anything else (true).
			// Finally, push a type to the compiler stack.

			string literalString = token.text;

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing literal read: \'{literalString}\'");
#endif

			if (string.IsNullOrEmpty(literalString)) // Not sure this would be currently possible, but it can't harm to check.
			{
				AddCompileError("Literal value was empty!");
				return CompilerArgument.Invalid;
			}

			if (ParseLiteralString(literalString, out string resultString))
			{
				if (targetContainer.StringRegister.RegisterValue(resultString, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.ReadString).instructionCode);
					output.Add(index);
					return new CompilerArgument(typeof(string), CompilerArgument.ArgSource.Literal, token);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.StringRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(string))}.");
					return CompilerArgument.Invalid;
				}
			}
			else if (ParseLiteralBool(literalString, out bool resultBool))
			{
				if (targetContainer.BoolRegister.RegisterValue(resultBool, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.ReadBool).instructionCode);
					output.Add(index);
					return new CompilerArgument(typeof(bool), CompilerArgument.ArgSource.Literal, token);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.BoolRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(bool))}.");
					return CompilerArgument.Invalid;
				}
			}
			else if (ParseLiteralInt(literalString, out int resultInt))
			{
				// TODO: Possible optimization: we could store positive ints smaller than ushort.MaxValue directly in the instruction code.
				// Then again, that would just raise complexity and would be less consistent.

				if (targetContainer.IntRegister.RegisterValue(resultInt, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.ReadInt).instructionCode);
					output.Add(index);
					return new CompilerArgument(typeof(int), CompilerArgument.ArgSource.Literal, token);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.IntRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(int))}.");
					return CompilerArgument.Invalid;
				}
			}
			else if (ParseLiteralFloat(literalString, out float resultFloat))
			{
				if (targetContainer.FloatRegister.RegisterValue(resultFloat, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.ReadFloat).instructionCode);
					output.Add(index);
					return new CompilerArgument(typeof(float), CompilerArgument.ArgSource.Literal, token);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.FloatRegister.registerMax}) of items in memory (literal or variable) of type {PinionTypes.GetPinionNameFromType(typeof(float))}.");
					return CompilerArgument.Invalid;
				}
			}
			else
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat($"Parsed literal as unrecognized: '{literalString}'.");
#endif
				AddCompileError($"Unrecognized literal: {literalString}");
				return CompilerArgument.Invalid;
			}
		}

		private static bool ParseLiteralString(string literal, out string result)
		{
			result = string.Empty;

			if (!literal.StartsWith("\""))
				return false;

			// It could be a broken string, though. If there aren't any closing quotation marks, any whitespace in the string has already been removed by the parser and it's nonsense anyway.
			if (!literal.EndsWith("\""))
			{
				AddCompileError($"String {literal} has no closing quotation mark.");
				return false;
			}

			result = literal.Trim('"');

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.LogFormat($"Parsed literal as string: {literal}.");
#endif
			return true;
		}

		private static bool ParseLiteralBool(string literal, out bool result)
		{
			// Why not just use bool.Parse? Oddly C# expects True/False (capitalized) as input. Not lower case true/false.
			// https://stackoverflow.com/questions/491334/why-does-boolean-tostring-output-true-and-not-true

			if (literal == "true")
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat($"Parsed literal as bool: '{literal}'.");
#endif
				result = true;
				return true;
			}

			if (literal == "false")
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat($"Parsed literal as bool: '{literal}'.");
#endif
				result = false;
				return true;
			}

			result = false;
			return false;
		}

		private static bool ParseLiteralInt(string literal, out int result)
		{
			result = 0;

			if (!Regex.IsMatch(literal, CompilerRegex.validIntRegex)) // See pattern declaration for valid int formats.
				return false;

			if (!int.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
			{
				AddCompileError($"Could not interpret '{literal}' as an int.");
				return false;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.LogFormat($"Parsed literal as int: '{literal}'.");
#endif
			return true;
		}

		private static bool ParseLiteralFloat(string literal, out float result)
		{
			result = 0f;

			if (!Regex.IsMatch(literal, CompilerRegex.validFloatRegex)) // See pattern declaration for valid float formats.
				return false;

			literal = literal.TrimEnd('f', 'F');

			if (!float.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
			{
				AddCompileError($"Could not interpret '{literal}' as a float.");
				return false;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.LogFormat($"Parsed literal as float: '{literal}'.");
#endif
			return true;
		}
	}
}