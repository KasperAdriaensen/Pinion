using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.
using Pinion.Documentation;
using Pinion.Compiler.Internal;

namespace Pinion
{
	[APISource]
	[DocSourceDisplayName("Math")]
	public static class PinionAPIMath
	{
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

		// [APIMethod]
		// [DocMethodOperatorReplace("++")]
		// public static float Increment(float value)
		// {
		// 	return ++value;
		// }

		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int Increment(int value)
		{
			return ++value;
		}

		[APICustomCompileIdentifier]
		private static void IncrementCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there is exactly one argument of the right type.
			if (providedArguments[0].argumentSource == CompilerArgument.ArgSource.Variable)
			{
				Debug.LogError("was a variavble!");
			}

			foreach (var type in providedArguments)
			{
				Debug.Log(type.argumentSource);
			}


		}

		// [APIMethod(MethodFlags = APIMethodFlags.Internal)]
		// public static int IncrementPreVariable_Int(PinionContainer container)
		// {
		// 	ushort location = container.AdvanceToNextInstruction();
		// 	int value = container.IntRegister.ReadValue(location) + 1;
		// 	container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
		// 	return value;
		// }



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
	}
}
