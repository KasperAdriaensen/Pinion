namespace Pinion
{
	using Pinion.Documentation;
	using UnityEngine;

	[APISource]
	[DocSourceDisplayName("Asserts")]
	public static class PinionAPIAsserts
	{
		/// Logs an error message to the game log file if $0 is false.
		/// Use with caution, can impact performance.
		[APIMethod]
		public static void Assert(bool condition)
		{
			Debug.Assert(condition);
		}

		/// Logs $1 to the game log file if $0 is false.
		/// Use with caution, can impact performance.
		[APIMethod]
		public static void Assert(bool condition, string message)
		{
			Debug.Assert(condition, message);
		}

		/// Logs an error message to the game log file if $0 does not equal $1.
		/// Use with caution, can impact performance.
		[APIMethod]
		public static void AssertEquals(bool valueA, bool valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(valueA.Equals(valueB), $"Boolean value '{valueA}' did not equal value '{valueB}'.");
		}

		/// Logs an error message to the game log file if $0 does not equal $1.
		/// Use with caution, can impact performance.
		[APIMethod]
		public static void AssertEquals(int valueA, int valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(valueA.Equals(valueB), $"Int value '{valueA}' did not equal value '{valueB}'.");
		}

		/// Logs an error message to the game log file if $0 does not equal $1.
		/// Use with caution, can impact performance.
		[APIMethod]
		public static void AssertEquals(string valueA, string valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(valueA.Equals(valueB), $"String value '{valueA}' did not equal value '{valueB}'.");
		}

		/// Logs an error message to the game log file if $0 does not approximately equal $1.
		/// Use with caution, can impact performance.
		[APIMethod]
		public static void AssertEqualsApproximate(float valueA, float valueB)
		{
			if (valueA != valueB) // Silly, but if we don't't do this when valueA == valueN, we still incur the string allocation below anyway.
				Assert(Mathf.Approximately(valueA, valueB), $"Float value '{valueA}' did not approximately equal value '{valueB}'.");
		}
	}

}