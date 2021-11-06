using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.

namespace Pinion
{
	[APISource]
	public static class PinionAPIEquality
	{
		[APIMethod]
		public static bool LessThan(float valueA, float valueB)
		{
			return valueA < valueB;
		}

		[APIMethod]
		public static bool LessThanOrEqual(float valueA, float valueB)
		{
			return valueA <= valueB;
		}

		[APIMethod]
		public static bool GreaterThan(float valueA, float valueB)
		{
			return valueA > valueB;
		}

		[APIMethod]
		public static bool GreaterThanOrEqual(float valueA, float valueB)
		{
			return valueA >= valueB;
		}

		[APIMethod]
		public static bool LessThan(int valueA, int valueB)
		{
			return valueA < valueB;
		}

		[APIMethod]
		public static bool LessThanOrEqual(int valueA, int valueB)
		{
			return valueA <= valueB;
		}

		[APIMethod]
		public static bool GreaterThan(int valueA, int valueB)
		{
			return valueA > valueB;
		}

		[APIMethod]
		public static bool GreaterThanOrEqual(int valueA, int valueB)
		{
			return valueA >= valueB;
		}

		[APIMethod]
		public static bool Equals(int valueA, int valueB)
		{
			return valueA == valueB;
		}

		[APIMethod]
		public static bool Equals(bool valueA, bool valueB)
		{
			return valueA == valueB;
		}

		[APIMethod]
		public static bool Equals(string valueA, string valueB)
		{
			return valueA.Equals(valueB);
		}

		[APIMethod]
		public static bool NotEquals(int valueA, int valueB)
		{
			return valueA != valueB;
		}

		[APIMethod]
		public static bool NotEquals(bool valueA, bool valueB)
		{
			return valueA != valueB;
		}

		[APIMethod]
		public static bool NotEquals(string valueA, string valueB)
		{
			return valueA != valueB;
		}


	}
}