namespace Pinion
{
	using UnityEngine;
	using Pinion.Documentation;
	using Pinion.Documentation.Internal;

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
		[DocMethodHide]
		public static float Negate(float number)
		{
			return -number;
		}

		/// Returns the negation of $0.
		[APIMethod]
		[DocMethodHide]
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

		/// Rounds $0 to the nearest integer. Returns it as a float.
		[APIMethod]
		public static float Round(float number)
		{
			return Mathf.Round(number);
		}

		/// Rounds $0 to the nearest integer. Returns it as an int.
		[APIMethod]
		public static int RoundToInt(float number)
		{
			return Mathf.RoundToInt(number);
		}

		/// Returns the least integer greater than or equal to $0.
		[APIMethod]
		public static float Ceil(float number)
		{
			return Mathf.Ceil(number);
		}

		/// Returns the least integer greater than or equal to $0. Returns it as an int.
		[APIMethod]
		public static int CeilToInt(float number)
		{
			return Mathf.CeilToInt(number);
		}

		/// Returns the largest integer smaller than or equal to $0.
		[APIMethod]
		public static float Floor(float number)
		{
			return Mathf.Floor(number);
		}

		/// Returns the largest integer smaller than or equal to $0. Returns it as an int.
		[APIMethod]
		public static int FloorToInt(float number)
		{
			return Mathf.FloorToInt(number);
		}

		/// Returns a value linearly interpolated between $0 and $1 with $2 as (0-1) interpolation value.
		[APIMethod]
		public static float Lerp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, t);
		}

		/// Returns a value linearly interpolated between $0 and $1 with $2 as interpolation value. $2 can be outside the 0-1 range to extrapolate.
		[APIMethod]
		public static float LerpUnclamped(float a, float b, float t)
		{
			return Mathf.LerpUnclamped(a, b, t);
		}

		/// Returns $0, clamped between minimum $1 and maximum $2.
		[APIMethod]
		public static int Clamp(int value, int min, int max)
		{
			return Mathf.Clamp(value, min, max);
		}

		/// Returns $0, clamped between minimum $1 and maximum $2.
		[APIMethod]
		public static float Clamp(float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		/// Returns the largest of either $0 or $1
		[APIMethod]
		public static float Max(float a, float b)
		{
			return Mathf.Max(a, b);
		}

		/// Returns the largest of either $0 or $1
		[APIMethod]
		public static int Max(int a, int b)
		{
			return Mathf.Max(a, b);
		}

		/// Returns the smallest of either $0 or $1
		[APIMethod]
		public static float Min(float a, float b)
		{
			return Mathf.Min(a, b);
		}

		/// Returns the smallest of either $0 or $1
		[APIMethod]
		public static int Min(int a, int b)
		{
			return Mathf.Min(a, b);
		}

		/// Returns Everyone's favorite irrational number. 3.14 something-something.
		[APIMethod]
		public static float Pi()
		{
			return Mathf.PI;
		}

		/// Returns $0 raised to the power $1.
		[APIMethod]
		public static float Pow(float number, float power)
		{
			return Mathf.Pow(number, power);
		}

		/// Returns the square root of $0.
		[APIMethod]
		public static float Sqrt(float number)
		{
			return Mathf.Sqrt(number);
		}

		/// Returns the sine of $0.
		[APIMethod]
		public static float Sin(float angleInRadians)
		{
			return Mathf.Sin(angleInRadians);
		}

		/// Returns the cosine of $0.
		[APIMethod]
		public static float Cos(float angleInRadians)
		{
			return Mathf.Cos(angleInRadians);
		}

		/// Returns the tangent of $0.
		[APIMethod]
		public static float Tan(float angleInRadians)
		{
			return Mathf.Tan(angleInRadians);
		}

		/// Returns $0 moved towards $1 in a step no bigger than $2. 
		[APIMethod]
		public static float MoveTowards(float current, float target, float maxStep)
		{
			return Mathf.MoveTowards(current, target, maxStep);
		}
	}
}
