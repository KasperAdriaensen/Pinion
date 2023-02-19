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

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int IncrementPrefixed(PinionContainer container, int value)
		{
			++value;
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static float IncrementPrefixed(PinionContainer container, float value)
		{
			++value;
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int IncrementPostfixed(PinionContainer container, int value)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value + 1);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static float IncrementPostfixed(PinionContainer container, float value)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value + 1f);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static int DecrementPrefixed(PinionContainer container, int value)
		{
			--value;
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static float DecrementPrefixed(PinionContainer container, float value)
		{
			--value;
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static int DecrementPostfixed(PinionContainer container, int value)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value - 1);
			return value;
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static float DecrementPostfixed(PinionContainer container, float value)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value - 1f);
			return value;
		}

		[APICustomCompileIdentifier]
		private static void IncrementCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, System.Action<string> compileErrorHandler)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there is exactly one argument of the right type.
			CompilerArgument arg = providedArguments[0];

			// Disallow using increment/decrement operator with anything but variables. To do otherwise is pointless anyway.
			if (arg.argumentSource != CompilerArgument.ArgSource.Variable)
			{
				compileErrorHandler(messageIncrementVariableOnly);
				return;
			}

			instructionCodes.Add(arg.variablePointer.GetIndexInRegister());
		}
	}
}
