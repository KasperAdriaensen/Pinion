namespace Pinion
{
	using System.Collections.Generic;
	using UnityEngine;
	using Pinion.Documentation;
	using Pinion.Compiler.Internal;

	[APISource]
	[DocSourceDisplayName("Math")]
	public static class PinionAPIMath
	{
		/// Returns sum of $0 and $1.
		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static float Add(float valueA, float valueB)
		{
			return valueA + valueB;
		}

		/// Returns sum of $0 and $1.
		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static int Add(int valueA, int valueB)
		{
			return valueA + valueB;
		}

		/// Returns sum of $0 and $1. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static float Add(float valueA, int valueB)
		{
			return valueA + valueB;
		}

		/// Returns sum of $0 and $1. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static float Add(int valueA, float valueB)
		{
			return valueA + valueB;
		}

		/// Returns result of subtracting $1 from $0.
		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static float Subtract(float valueA, float valueB)
		{
			return valueA - valueB;
		}

		/// Returns result of subtracting $1 from $0.
		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static int Subtract(int valueA, int valueB)
		{
			return valueA - valueB;
		}

		/// Returns result of subtracting $1 from $0. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static float Subtract(float valueA, int valueB)
		{
			return valueA - valueB;
		}

		/// Returns result of subtracting $1 from $0. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("-")]
		public static float Subtract(int valueA, float valueB)
		{
			return valueA - valueB;
		}

		/// Returns the product of $0 and $1.
		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static float Multiply(float valueA, float valueB)
		{
			return valueA * valueB;
		}

		/// Returns the product of $0 and $1.
		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static int Multiply(int valueA, int valueB)
		{
			return valueA * valueB;
		}

		/// Returns the product of $0 and $1. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static float Multiply(float valueA, int valueB)
		{
			return valueA * valueB;
		}

		/// Returns the product of $0 and $1. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("*")]
		public static float Multiply(int valueA, float valueB)
		{
			return valueA * valueB;
		}

		/// Returns the result of dividing $0 by $1.
		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static float Divide(float valueA, float valueB)
		{
			return valueA / valueB;
		}

		/// Returns the result of dividing $0 by $1. Result is rounded down.
		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static int Divide(int valueA, int valueB)
		{
			return valueA / valueB;
		}

		/// Returns the result of dividing $0 by $1. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static float Divide(int valueA, float valueB)
		{
			return valueA / valueB;
		}

		/// Returns the result of dividing $0 by $1. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("/")]
		public static float Divide(float valueA, int valueB)
		{
			return valueA / valueB;
		}

		/// Returns the remainder of dividing $0 by $1. Sign matches $0.
		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static float Remainder(float valueA, float valueB)
		{
			return valueA % valueB;
		}

		/// Returns the remainder of dividing $0 by $1. Sign matches $0.
		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static int Remainder(int valueA, int valueB)
		{
			return valueA % valueB;
		}

		/// Returns the remainder of dividing $0 by $1. Sign matches $0. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static float Remainder(int valueA, float valueB)
		{
			return valueA % valueB;
		}

		/// Returns the remainder of dividing $0 by $1. Sign matches $0. Result is of type float.
		[APIMethod]
		[DocMethodOperatorReplace("%")]
		public static float Remainder(float valueA, int valueB)
		{
			return valueA % valueB;
		}

		/// Returns the negation of $0.
		[APIMethod]
		[DocMethodHide()]
		public static float Negate(float number)
		{
			return -number;
		}

		/// Returns the negation of $0.
		[APIMethod]
		[DocMethodHide()]
		public static int Negate(int number)
		{
			return -number;
		}

		/// Returns the absolute value of $0.
		[APIMethod]
		public static float Absolute(float number)
		{
			return Mathf.Abs(number);
		}

		/// Returns the absolute value of $0.
		[APIMethod]
		public static int Absolute(int number)
		{
			return Mathf.Abs(number);
		}
	}
}
