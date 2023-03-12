using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Linq;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.
using System;
using Pinion.Compiler.Internal;

// This file contains some utilities for debugging the compiler itself. They should never be used outside of the editor.
namespace Pinion.Compiler
{
#if UNITY_EDITOR
	public partial class PinionCompiler
	{
		// Outputs a list of every instruction compiled in the passed PinionContainer.
		// It attempts to make the instructions somewhat "human readable."
		private static void OutputFullInstructionList(PinionContainer targetContainer)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.AppendLine("Outputting full instruction list.");
			stringBuilder.AppendLine("----------------------------------------------------------");
			stringBuilder.AppendLine("Pattern: [index] ushortValue: Instruction string");
			stringBuilder.AppendLine("----------------------------------------------------------");

			string previousInstructionString = null;

			for (int i = 0; i < targetContainer.scriptInstructions.Count; i++)
			{
				ushort currentInstructionCode = targetContainer.scriptInstructions[i];

				if (string.IsNullOrEmpty(previousInstructionString))
				{
					InstructionData currentInstructionData = PinionAPI.GetInstructionData(currentInstructionCode, true);

					if (currentInstructionData != null)
					{
						stringBuilder.AppendLine($"[{i}] {currentInstructionCode}: {currentInstructionData.instructionString}");
						previousInstructionString = currentInstructionData.instructionString;
					}

					continue;
				}

				if (previousInstructionString.StartsWith("ReadValue_") || previousInstructionString.StartsWith("ReadLabel")) // either a literal or a declared variable
				{
					stringBuilder.AppendLine(string.Format("[{0}] Previous command's index to read from = {1}", i, (ushort)currentInstructionCode));
					previousInstructionString = null;
				}
				else if (previousInstructionString.StartsWith("WriteValue_"))
				{
					stringBuilder.AppendLine(string.Format("[{0}] Previous command's index to write to = {1}", i, (ushort)currentInstructionCode));
					previousInstructionString = null;
				}
				else if (previousInstructionString == "If" || previousInstructionString == "Else" || previousInstructionString == "ElseIf")
				{
					stringBuilder.AppendLine(string.Format("[{0}] Jump target index: {1}", i, (ushort)currentInstructionCode));
					previousInstructionString = null;
				}
				else if (previousInstructionString == "SetArgumentCount")
				{
					stringBuilder.AppendLine(string.Format("[{0}] Previous command's argument count = {1}", i, (ushort)currentInstructionCode));
					previousInstructionString = null;
				}
				else
				{
					InstructionData currentInstructionData = PinionAPI.GetInstructionData(currentInstructionCode, true);

					if (currentInstructionData != null)
					{
						stringBuilder.AppendLine($"[{i}] {currentInstructionCode}: {currentInstructionData.instructionString}");
						previousInstructionString = currentInstructionData.instructionString;
					}
				}
			}

			Debug.Log(stringBuilder.ToString());
		}
	}
#endif
}
