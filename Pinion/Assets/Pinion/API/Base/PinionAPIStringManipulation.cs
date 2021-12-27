using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;


namespace Pinion
{
	[APISource]
	public static class PinionAPIStringManipulation
	{
		[APIMethod]
		public static string ToString(bool value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[APIMethod]
		public static string ToString(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[APIMethod]
		public static string ToString(float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		// NOTE: Obviously, concatenating strings is not actually a mathematical operation.
		// In every other way, it's just easier to do it that way, so we can effortless link it to the "+" operator.
		// That's why string concatenation is an overload of the mathematical Add, but implemented here.
		[APIMethod]
		public static string Add(string stringA, string stringB)
		{
			return stringA + stringB;
		}

		[APIMethod]
		public static bool IsEmptyString(string value)
		{
			return string.IsNullOrEmpty(value);
		}

		[APIMethod]
		public static int ToInt(string value, int failValue)
		{
			int result = 0;
			if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
				return result;

			return failValue;
		}

		[APIMethod]
		public static float ToFloat(string value, float failValue)
		{
			float result = 0;
			if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				return result;

			return failValue;
		}

		[APIMethod]
		public static bool CanConvertToInt(string value)
		{
			int result = 0;
			return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}

		[APIMethod]
		public static bool CanConvertToFloat(string value)
		{
			float result = 0;
			return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}
	}
}
