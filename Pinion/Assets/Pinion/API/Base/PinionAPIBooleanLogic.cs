using System.Collections;
using System.Collections.Generic;
using Pinion.Documentation;
using UnityEngine;

namespace Pinion
{
	[APISource]
	[DocSourceDisplayName("Boolean Logic")]
	public static class PinionAPIBooleanLogic
	{
		/// Logical NOT. Returns opposite of $0.
		[APIMethod]
		[DocMethodOperatorReplace("!")]
		public static bool Not(bool value)
		{
			return !value;
		}

		/// Logical AND. Returns true if both $0 and $1 are true.
		[APIMethod]
		[DocMethodOperatorReplace("&&")]
		public static bool And(bool valueA, bool valueB)
		{
			return valueA && valueB;
		}

		/// Logical OR. Returns true if either $0 or $1 is true, or both.
		[APIMethod]
		[DocMethodOperatorReplace("||")]
		public static bool Or(bool valueA, bool valueB)
		{
			return valueA || valueB;
		}
	}
}