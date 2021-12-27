using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Pinion.Compiler.Internal;
using UnityEngine;

namespace Pinion.Compiler
{
	public partial class PinionCompiler
	{
		private static Type ParseLiteral(PinionContainer targetContainer, string literal, List<ushort> output)
		{
			// General process for a valid literal.
			// Add a bytecode instruction indicating the next instruction code is to be interpreted as a literal. The associated function will know how to interpret that.
			// For float, string, and int: the next instruction is an index in the respective register.
			// For bool, the next instruction is either 0 (false) or anything else (true).
			// Finally, push a type to the compiler stack.

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing literal: \'{literal}\'");
#endif

			if (string.IsNullOrEmpty(literal)) // Not sure this would be currently possible, but it can't harm to check.
			{
				AddCompileError("Literal value was empty!");
				return null;
			}

			// First off: figure out what type of literal we're looking at.
			// There is, as far as I'm aware, no nice and clean way to figure out what type a string represents. That makes sense.
			// So instead, we just put the picture together as well as we can, successively testing different literal types, trying to be as efficient as possible.

			// First one's easy. If the expression start with a quotation mark, it has to be a string. At best, it won't parse as anything else anyway.
			if (literal.StartsWith("\""))
			{
				// It could be a broken string, though. If there aren't any closing quotation marks, any whitespace in the string has already been removed by the parser and it's nonsense anyway.
				if (!literal.EndsWith("\""))
				{
					AddCompileError($"String {literal} has no closing quotation mark.");
					return null;
				}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat("[PinionCompiler] Parsed literal as string: {0}", literal);
#endif

				literal = literal.Trim('"');

				if (targetContainer.StringRegister.RegisterValue(literal, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDReadString).instructionCode);
					output.Add(index);
					return typeof(string);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.StringRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(string))}.");
					return null;
				}
			}
			else if (literal == "false")
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat("[PinionCompiler] Parsed literal as bool (false): {0}", literal);
#endif
				output.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDReadBool).instructionCode);
				output.Add(0);
				return typeof(bool);
			}
			else if (literal == "true")
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat("[PinionCompiler] Parsed literal as bool (true): {0}", literal);
#endif
				output.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDReadBool).instructionCode);
				output.Add(1);
				return typeof(bool);
			}
			else if (Regex.IsMatch(literal, CompilerRegex.validIntRegex))
			{
				int result = 0;

				if (!int.TryParse(literal, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
				{
					AddCompileError($"Could not interpret \"{literal}\" as an int.");
					return null;
				}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat("[PinionCompiler] Parsed literal as int: {0}", literal);
#endif

				// TODO: Possible optimization: we could store positive ints smaller than ushort.MaxValue directly in the instruction code.
				// Then again, that would just raise complexity and would be less consistent.

				if (targetContainer.IntRegister.RegisterValue(result, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDReadInt).instructionCode);
					output.Add(index);
					return typeof(int);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.IntRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(int))}.");
					return null;
				}
			}
			else if (Regex.IsMatch(literal, CompilerRegex.validFloatRegex)) // See pattern declaration for valid float formats.
			{
				literal = literal.Trim('f');
				float result = 0f;

				if (!float.TryParse(literal, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				{
					AddCompileError($"Could not interpret \"{literal}\" as a float.");
					return null;
				}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat("[PinionCompiler] Parsed literal as float: {0}", literal);
#endif

				if (targetContainer.FloatRegister.RegisterValue(result, out ushort index, true))
				{
					output.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDReadFloat).instructionCode);
					output.Add(index);
					return typeof(float);
				}
				else
				{
					AddCompileError($"Exceeded maximum number ({targetContainer.FloatRegister.registerMax}) of items in memory (literal or variable) of type {TypeNameShortHands.GetSimpleTypeName(typeof(float))}.");
					return null;
				}
			}
			else
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.LogFormat("[PinionCompiler] Parsed literal as unrecognized: {0}", literal);
#endif
				AddCompileError($"Unrecognized literal: \"{literal}\".");
				return null;
			}
		}
	}
}