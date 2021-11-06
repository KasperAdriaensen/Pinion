using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


namespace Pinion
{
	[APISource]
	public static class PinionAPIStringManipulation
	{
		[APIMethod]
		public static string ToString(bool value)
		{
			return value.ToString();
		}

		[APIMethod]
		public static string ToString(int value)
		{
			return value.ToString();
		}

		[APIMethod]
		public static string ToString(float value)
		{
			return value.ToString();
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
	}
}
