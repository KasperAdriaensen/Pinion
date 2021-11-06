using System;
using System.Collections;
using System.Collections.Generic;
using Pinion.Compiler.Internal;
using UnityEngine;

namespace Pinion.Compiler
{
	public partial class PinionCompiler
	{
		private static List<InstructionData> instructionMatchBuffer = new List<InstructionData>(32); // reusable collection for string to instruction lookup results
		private static Stack<System.Type> consumedArgumentsBuffer = new Stack<System.Type>(32); // reusable stack for arguments "consumed" by signature match checking

		private static Type ParseInstruction(PinionContainer targetContainer, string instructionString, List<ushort> output, IList<System.Type> providedArguments)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing instruction: \'{instructionString}\'");
#endif

			InstructionData matchedInstruction = null;

			instructionMatchBuffer.Clear();
			// The same string can resolve to multiple instruction signatures. These are stored in instructionMatchBuffer.
			PinionAPI.GetMatchingInstructions(instructionString, instructionMatchBuffer);

			if (instructionMatchBuffer.Count < 1)
			{
				AddCompileError($"Unknown instruction: '{instructionString}.'");
				return null;
			}

			// We iterate over all potential matches, looking for a full match.
			// We also keep track of the closest match so far, should we never end up finding a full match.
			bool fullMatch = false;
			int bestParameterMatchCount = -1; // -1 so that even 0 matches is better!

			foreach (InstructionData potentialMatch in instructionMatchBuffer)
			{
				int parameterMatchCount = 0;
				fullMatch = potentialMatch.MatchesArguments(providedArguments, out parameterMatchCount);

				if (fullMatch)
				{
					matchedInstruction = potentialMatch;
					break;
				}
				else if (parameterMatchCount > bestParameterMatchCount) // It's not perfect, but it's at least *better*.
				{
					matchedInstruction = potentialMatch;
					bestParameterMatchCount = parameterMatchCount;
				}
			}


			// If a full signature match was not found, we generate some feedback on the closest match.
			if (!fullMatch)
			{
				string errorMessage = string.Empty;
				if (matchedInstruction == null || matchedInstruction.exposedParameterCount <= 0) // If there are no parameters, we can't generate a more meaningful message.
				{
					errorMessage = $"There is no version of the instruction {instructionString} that accepts the provided arguments.";
				}
				else
				{
					// Generate a string that describes the best signature match.
					string bestMatchArgumentsString = string.Empty;

					for (int i = 0; i < matchedInstruction.exposedParameterCount; i++)
					{
						System.Type argumentType = matchedInstruction.GetParameterType(i);
						bestMatchArgumentsString += TypeNameShortHands.GetSimpleTypeName(argumentType);

						if (i < matchedInstruction.exposedParameterCount - 1)
							bestMatchArgumentsString += ", ";
					}

					// Generate a string that describes the arguments provided.
					string providedArgumentsString = string.Empty;

					for (int i = 0; i < providedArguments.Count; i++)
					{
						providedArgumentsString += TypeNameShortHands.GetSimpleTypeName(providedArguments[i]);

						if (i < providedArguments.Count - 1)
							providedArgumentsString += ", ";
					}

					errorMessage = $"There is no version of the instruction {instructionString} that accepts the provided arguments ({providedArgumentsString}). Best match was {instructionString}({bestMatchArgumentsString}).";
				}

				providedArguments.Clear();
				AddCompileError(errorMessage);
				// No chance of compiling something useful at this point. Compilation will fail.
				return null;
			}

			// Some instruction are internals only - they make the system itself function. We don't really want the player to know or care about these.
			// Normally, we shouldn't even get to this point, since the parsing by default filters out internal functions. It won't "recognize" those strings as instructions at all.
			// However, if we do end up here in some freak accident, we'll trow a compiler error just in case.
			if (matchedInstruction.internalInstruction)
			{
				providedArguments.Clear();
				AddCompileError($"[PinionCompiler] Expression {instructionString} is reserved for internal use only.");
				return null;
			}

			// The actual final instruction code list!
			output.Add(matchedInstruction.instructionCode);

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.LogFormat($"[PinionCompiler] Parsed instruction {instructionString} to instruction {matchedInstruction.instructionCode}: {matchedInstruction.instructionString}.");
#endif
			providedArguments.Clear();
			return matchedInstruction.returnType;
		}
	}
}