using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIAsserts
	{
		[APIMethod]
		public static void Assert(bool condition)
		{
			Debug.Assert(condition);
		}

		[APIMethod]
		public static void Assert(bool condition, string message)
		{
			Debug.Assert(condition, message);
		}

		[APIMethod]
		public static void AssertEquals(bool valueA, bool valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(valueA.Equals(valueB), $"Boolean value '{valueA}' did not equal value '{valueB}'.");
		}

		[APIMethod]
		public static void AssertEquals(int valueA, int valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(valueA.Equals(valueB), $"Int value '{valueA}' did not equal value '{valueB}'.");
		}

		[APIMethod]
		public static void AssertEquals(string valueA, string valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(valueA.Equals(valueB), $"String value '{valueA}' did not equal value '{valueB}'.");
		}

		[APIMethod]
		public static void AssertEqualsApproximate(float valueA, float valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(Mathf.Approximately(valueA, valueB), $"Float value '{valueA}' did not approximately equal value '{valueB}'.");
		}
	}

}