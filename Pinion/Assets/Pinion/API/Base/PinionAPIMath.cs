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

		// This essentially a dummy method. The ReplaceInstruction value on the APICustomCompileRequired means compilatio will *always* be replaced with custom compilation logic.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementIntCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.ReplaceInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int Increment(int value)
		{
			return ++value;
		}

		[APICustomCompileIdentifier]
		private static void IncrementIntCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there is exactly one argument of the right type.
			CompilerArgument arg = providedArguments[0];

			if (arg.argumentSource == CompilerArgument.ArgSource.Variable)
			{
				instructionCodes.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.IncrementIntVariablePrefix).instructionCode);
				instructionCodes.Add(arg.variablePointer.GetIndexInRegister());
			}
			else
			{
				instructionCodes.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.IncrementIntLiteralPrefix).instructionCode);
			}
		}

		[APIInternalMethodIdentifier(PinionAPIInternalIDs.IncrementIntVariablePrefix)]
		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		public static int IncrementVariablePrefix(PinionContainer container, int value)
		{
			++value;
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		[APIInternalMethodIdentifier(PinionAPIInternalIDs.IncrementIntLiteralPrefix)]
		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		public static int IncrementLiteralPrefix(int value)
		{
			return ++value;
		}
	}
}
