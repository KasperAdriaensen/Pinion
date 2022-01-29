using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.
using Pinion.Documentation;

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
	}
}
