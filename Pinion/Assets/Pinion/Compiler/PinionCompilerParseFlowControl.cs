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
		private static Dictionary<string, ushort> labelToIndexInLabelList = new Dictionary<string, ushort>();

		private static void ParseLabelCreation(PinionContainer targetContainer, string expression, int lineNumber)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("[PinionCompiler] Parsing label creation: " + expression);
#endif

			// We don't test again if the passed string starts with LabelCreatePrefix. 
			// There is currently no code path where this can happen. It is checked directly before the only place this is called.
			// That is no guarantee, of course, but we'll consider it ok enough for now.
			string label = expression.Substring(CompilerConstants.LabelCreatePrefixComplete.Length);

			if (!IsValidLabelName(label))
			{
				AddCompileError($"Invalid label name: {label}. Name must consist of alphanumeric characters.");
				return;
			}

			// Note that during compilation a label READ can also create this entry (but will set it to -1).
			// After all, by their nature, labels might be referenced before the instruction they refer to is encountered.
			// The actual value, however, will always be filled a label declaration.
			ushort locationInLabelRegister = GetOrCreateLabelRegisterValue(targetContainer, label);

			// If value >= 0, a previous declaration already filled this value!
			if (targetContainer.LabelRegister.ReadValue(locationInLabelRegister) >= 0)
			{
				AddCompileError($"Label: {label} is declared multiple times!");
				return;
			}

			// Store in the register the number of instructions compiled so far. This makes us jump to the first instruction after them (if present).
			int jumpLocation = targetContainer.scriptInstructions.Count;

			targetContainer.LabelRegister.WriteValue(locationInLabelRegister, jumpLocation);

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Label {label} was assigned jump index {jumpLocation}.");
#endif
		}

		private static void ParseLabelRead(PinionContainer targetContainer, string labelName, List<ushort> output)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("[PinionCompiler] Parsing label read: " + labelName);
#endif

			// We don't test again if the passed string starts with LabelReadPrefix. 
			// There is currently no code path where this can happen. It is checked directly before the only place this is called.
			// That is no guarantee, of course, but we'll consider it solid enough for now.
			string label = labelName.Substring(CompilerConstants.LabelReadPrefix.Length);

			// The user shouldn't normally see this, since labels are only created by the compiler.
			// However, there's nothing stopping them from just writing the label code out.
			if (!IsValidLabelName(label))
			{
				AddCompileError($"Invalid label name: {label}. Name must consist of alphanumeric characters.");
				return;
			}

			// A label READ is allowed to create this entry (but will set it to -1).
			// After all, by their nature, labels might be referenced before the instruction they refer to is encountered.
			// The actual value, however, will always be filled a label declaration.
			ushort locationInLabelRegister = GetOrCreateLabelRegisterValue(targetContainer, label);

			output.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDReadLabel).instructionCode);
			output.Add(locationInLabelRegister);
		}

		private static ushort GetOrCreateLabelRegisterValue(PinionContainer targetContainer, string label)
		{
			if (!labelToIndexInLabelList.ContainsKey(label))
			{
				if (targetContainer.LabelRegister.RegisterValue(-1, out ushort location, false))
				{
					labelToIndexInLabelList.Add(label, location);

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
					Debug.Log($"[PinionCompiler] Created label {label} at register index {location}.");
#endif
					return location;
				}
				else
				{
					AddCompileError($"Exceeded maximum number of jump labels ({targetContainer.LabelRegister.registerMax}). Returning default value.");
					return default(ushort);
				}
			}
			else
			{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Label {label} already existed at register index {labelToIndexInLabelList[label]}.");
#endif
				return labelToIndexInLabelList[label];
			}
		}

		private static bool IsValidLabelName(string labelName)
		{
			if (string.IsNullOrEmpty(labelName))
				return false;

			return Regex.IsMatch(labelName, CompilerRegex.validLabelNameRegex);
		}

		private static void ParseConditionalLabelJump(PinionContainer targetContainer, string expression)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("[PinionCompiler] Parsing conditional label jump: " + expression);
#endif

			expression = expression.Substring(CompilerConstants.ConditionLabelJump.Length); // remove the ConditionLabelJump marker at beginning.

			// This is not as a clean as usual. We can't split by the comma because there could be a comma in a sub-expression of the condition part.
			// Problematic? Solution?
			string labelName = expression.Substring(0, expression.IndexOf(',')); // everything before first comma is label name - commas are not allowed in label names
			string condition = expression.Remove(0, labelName.Length + 1); // length + 1 to also remove comma

			if (string.IsNullOrWhiteSpace(labelName) || string.IsNullOrWhiteSpace(condition))
			{
				AddCompileError($"Invalid conditional jump: \"{expression}\".");
				return;
			}

			// label to jump to if condition returns false
			// will be pushed to separate jump location stack
			ParseLabelRead(targetContainer, labelName, targetContainer.scriptInstructions);

			if (!compileSuccess)
				return;

			// condition (bool)
			// will be pushed to standard stack
			Type returnType = ParseExpression(targetContainer, condition);     // condition

			if (returnType == null)
			{
				// No message here - the failed compilation of the expression will be more meaningful.
				return;
			}

			// See if one has been added.
			// Not outputting the expression here because it's already been rewritten.
			if (returnType != typeof(bool)) // null or something else
			{
				if (returnType == typeof(void))
				{
					AddCompileError($"Invalid conditional statement. Expression does not return a value.");
				}
				else
				{

					AddCompileError($"Invalid conditional statement. Expression resolves to type {TypeNameShortHands.GetSimpleTypeName(returnType)} instead type {TypeNameShortHands.GetSimpleTypeName(typeof(bool))}.");
				}
				return;
			}

			// pops jump location from jump location stack and condition from standard stack
			// if condition == false, will jump to jump location; if condition == true, will just continue with next instruction
			targetContainer.scriptInstructions.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDIfFalseGoTo).instructionCode);
		}

		private static void ParseLabelJump(PinionContainer targetContainer, string expression)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("[PinionCompiler] Parsing label jump: " + expression);
#endif

			string labelName = expression.Substring(CompilerConstants.LabelJump.Length); // remove the LabelJump marker at beginning.

			if (string.IsNullOrWhiteSpace(labelName))
			{
				AddCompileError($"Invalid conditional jump: \"{expression}\".");
				return;
			}

			// label to jump to if condition returns false
			// will be pushed to separate jump location stack
			ParseLabelRead(targetContainer, labelName, targetContainer.scriptInstructions);

			// pops jump location from jump location stack
			// then jumps to jump location
			targetContainer.scriptInstructions.Add(PinionAPI.GetInternalInstructionByID(PinionAPI.InternalIDGoTo).instructionCode);
		}
	}
}
