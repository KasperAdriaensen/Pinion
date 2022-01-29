using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;
using Pinion.Documentation;

namespace Pinion
{
	[APISource]
	[DocSourceDisplayName("Text")]
	public static class PinionAPIStringManipulation
	{
		// NOTE: Obviously, concatenating strings is not actually a mathematical operation.
		// In every other way, it's just easier to do it that way, so we can effortless link it to the "+" operator.
		// That's why string concatenation is an overload of the mathematical Add, but implemented here.

		/// Combine $0 and $1 into a single string.
		[APIMethod]
		[DocMethodOperatorReplace("+")]
		public static string Add(string stringA, string stringB)
		{
			return stringA + stringB;
		}

		/// Converts $0 to a string.
		[APIMethod]
		public static string ToString(bool value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		/// Converts $0 to a string.
		[APIMethod]
		public static string ToString(int value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		/// Converts $0 to a string.
		[APIMethod]
		public static string ToString(float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		/// Returns whether $0 is an empty string.
		[APIMethod]
		public static bool IsEmptyString(string value)
		{
			return string.IsNullOrEmpty(value);
		}

		/// Returns $0 with all characters in lower case.
		[APIMethod]
		public static string ToLower(string value)
		{
			return value.ToLower();
		}

		/// Returns $0 with all characters in upper case.
		[APIMethod]
		public static string ToUpper(string value)
		{
			return value.ToUpper();
		}

		/// Tries to interpret string $0 as an int. If $0 is improperly formatted to allow this, returns $1 instead. See CanConvertToInt.
		[APIMethod]
		public static int ToInt(string value, int failValue)
		{
			int result = 0;
			if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
				return result;

			return failValue;
		}

		/// Tries to interpret string $0 as a float. If $0 is improperly formatted to allow this, returns $1 instead. See CanConvertToFloat.
		[APIMethod]
		public static float ToFloat(string value, float failValue)
		{
			float result = 0;
			if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				return result;

			return failValue;
		}

		/// Returns whether $0 is formatted in a way that allow conversion to an int.
		[APIMethod]
		public static bool CanConvertToInt(string value)
		{
			int result = 0;
			return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}

		/// Returns whether $0 is formatted in a way that allow conversion to a float.
		[APIMethod]
		public static bool CanConvertToFloat(string value)
		{
			float result = 0;
			return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
		}
	}
}
