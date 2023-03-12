namespace Pinion
{
	using Pinion.Documentation;

	[APISource]
	[DocSourceDisplayName("Equality")]
	public static class PinionAPIEquality
	{
		/// Returns whether $0 is less than $1.
		[APIMethod]
		[DocMethodOperatorReplace("<")]
		public static bool LessThan(float valueA, float valueB)
		{
			return valueA < valueB;
		}

		/// Returns whether $0 is less than or equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("<=")]
		public static bool LessThanOrEqual(float valueA, float valueB)
		{
			return valueA <= valueB;
		}

		/// Returns whether $0 is greater than $1.
		[APIMethod]
		[DocMethodOperatorReplace(">")]
		public static bool GreaterThan(float valueA, float valueB)
		{
			return valueA > valueB;
		}

		/// Returns whether $0 is greater than or equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace(">=")]
		public static bool GreaterThanOrEqual(float valueA, float valueB)
		{
			return valueA >= valueB;
		}

		/// Returns whether $0 is less than $1.
		[APIMethod]
		[DocMethodOperatorReplace("<")]
		public static bool LessThan(int valueA, int valueB)
		{
			return valueA < valueB;
		}

		/// Returns whether $0 is less than or equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("<=")]
		public static bool LessThanOrEqual(int valueA, int valueB)
		{
			return valueA <= valueB;
		}

		/// Returns whether $0 is greater than $1.
		[APIMethod]
		[DocMethodOperatorReplace(">")]
		public static bool GreaterThan(int valueA, int valueB)
		{
			return valueA > valueB;
		}

		/// Returns whether $0 is greater than or equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace(">=")]
		public static bool GreaterThanOrEqual(int valueA, int valueB)
		{
			return valueA >= valueB;
		}

		/// Returns whether $0 is equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("==")]
		public static bool Equals(int valueA, int valueB)
		{
			return valueA == valueB;
		}

		/// Returns whether $0 is equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("==")]
		public static bool Equals(bool valueA, bool valueB)
		{
			return valueA == valueB;
		}

		/// Returns whether $0 is equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("==")]
		public static bool Equals(string valueA, string valueB)
		{
			return valueA.Equals(valueB);
		}

		/// Returns whether $0 is not equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("!=")]
		public static bool NotEquals(int valueA, int valueB)
		{
			return valueA != valueB;
		}

		/// Returns whether $0 is not equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("!=")]
		public static bool NotEquals(bool valueA, bool valueB)
		{
			return valueA != valueB;
		}

		/// Returns whether $0 is not equal to $1.
		[APIMethod]
		[DocMethodOperatorReplace("!=")]
		public static bool NotEquals(string valueA, string valueB)
		{
			return valueA != valueB;
		}
	}
}