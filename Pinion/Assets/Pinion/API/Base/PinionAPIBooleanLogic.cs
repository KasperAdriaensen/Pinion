using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIBooleanLogic
	{
		[APIMethod]
		public static bool Not(bool value)
		{
			return !value;
		}

		[APIMethod]
		public static bool And(bool valueA, bool valueB)
		{
			return valueA && valueB;
		}

		[APIMethod]
		public static bool Or(bool valueA, bool valueB)
		{
			return valueA || valueB;
		}
	}
}