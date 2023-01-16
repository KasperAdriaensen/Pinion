using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.
using Pinion.Documentation;
using Pinion.Compiler.Internal;
using Pinion.Internal;

namespace Pinion
{
	[APISource]
	[DocSourceDisplayName("Math")]
	public static class PinionAPIMath
	{
		private const string messageIncrementVariableOnly = "Increment and decrement operator can only be used with variables.";

		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static float Add(float valueA, float valueB)
		{
			return valueA + valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static int Add(int valueA, int valueB)
		{
			return valueA + valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static float Add(float valueA, int valueB)
		{
			return valueA + valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static float Add(int valueA, float valueB)
		{
			return valueA + valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static float Subtract(float valueA, float valueB)
		{
			return valueA - valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static int Subtract(int valueA, int valueB)
		{
			return valueA - valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static float Subtract(float valueA, int valueB)
		{
			return valueA - valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static float Subtract(int valueA, float valueB)
		{
			return valueA - valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static float Multiply(float valueA, float valueB)
		{
			return valueA * valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static int Multiply(int valueA, int valueB)
		{
			return valueA * valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static float Multiply(float valueA, int valueB)
		{
			return valueA * valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static float Multiply(int valueA, float valueB)
		{
			return valueA * valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static float Divide(float valueA, float valueB)
		{
			return valueA / valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static int Divide(int valueA, int valueB)
		{
			return valueA / valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static float Divide(int valueA, float valueB)
		{
			return valueA / valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static float Divide(float valueA, int valueB)
		{
			return valueA / valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static float Modulo(float valueA, float valueB)
		{
			return valueA % valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static int Modulo(int valueA, int valueB)
		{
			return valueA % valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static float Modulo(int valueA, float valueB)
		{
			return valueA % valueB;
		}

		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static float Modulo(float valueA, int valueB)
		{
			return valueA % valueB;
		}

		[APIMethod]
		[DocMethodHide()]
		public static float Negate(float number)
		{
			return -number;
		}

		[APIMethod]
		[DocMethodHide()]
		public static int Negate(int number)
		{
			return -number;
		}

		[APIMethod]
		public static float Absolute(float number)
		{
			return Mathf.Abs(number);
		}

		[APIMethod]
		public static int Absolute(int number)
		{
			return Mathf.Abs(number);
		}

		// // This essentially a dummy method. 
		// // The ReplaceInstruction value on the APICustomCompileRequired means standard compilation behavior will *always* be replaced with custom compilation logic.
		// [APIMethod]
		// [APICustomCompileRequired(nameof(IncrementIntCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.ReplaceInstruction)]
		// [DocMethodOperatorReplace("++")]
		// public static int Increment(int value)
		// {
		// 	// int a = 1;
		// 	// int b = 2;
		// 	// int test = a++ b;


		// 	// Debug.Log(test);
		// 	return ++value;
		// }

		// [APICustomCompileIdentifier]
		// private static void IncrementIntCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, System.Action<string> compileErrorHandler)
		// {
		// 	// Compiler should already have ensured signature match at this point. 
		// 	// We can be reasonably sure there is exactly one argument of the right type.
		// 	CompilerArgument arg = providedArguments[0];

		// 	// Disallow using increment decrement operator with anything but variables. To do otherwise is pointless anyway, and saves complexity.
		// 	if (arg.argumentSource != CompilerArgument.ArgSource.Variable)
		// 	{
		// 		compileErrorHandler(messageIncrementVariableOnly);
		// 		return;
		// 	}

		// 	// Compilation of the Increment operator breaks down into prefixed and suffixed.
		// 	// Current behavior is always 'prefixed'.

		// 	instructionCodes.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.IncrementIntVariablePrefix).instructionCode);
		// 	instructionCodes.Add(arg.variablePointer.GetIndexInRegister());
		// }

		// [APIInternalMethodIdentifier(PinionAPIInternalIDs.IncrementIntVariablePrefix)]
		// [APIMethod(MethodFlags = APIMethodFlags.Internal)]
		// public static int IncrementVariablePrefix(PinionContainer container, int value)
		// {
		// 	++value;
		// 	container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
		// 	return value;
		// }

		// This essentially a dummy method. 
		// The ReplaceInstruction value on the APICustomCompileRequired means standard compilation behavior will *always* be replaced with custom compilation logic.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementIntCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int IncrementPrefixed(PinionContainer container, int value)
		{
			++value;
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		[APICustomCompileIdentifier]
		private static void IncrementIntCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, System.Action<string> compileErrorHandler)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there is exactly one argument of the right type.
			CompilerArgument arg = providedArguments[0];

			// Disallow using increment decrement operator with anything but variables. To do otherwise is pointless anyway, and saves complexity.
			if (arg.argumentSource != CompilerArgument.ArgSource.Variable)
			{
				compileErrorHandler(messageIncrementVariableOnly);
				return;
			}

			instructionCodes.Add(arg.variablePointer.GetIndexInRegister());
		}
	}
}
