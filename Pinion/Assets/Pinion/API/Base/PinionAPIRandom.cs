using System.Collections;
using System.Collections.Generic;
using Pinion.Documentation;
using UnityEngine;

namespace Pinion
{
	[APISource]
	[DocSourceDisplayName("Randomness")]
	public static class PinionAPIRandom
	{
		/// Returns a random value between 0 (inclusive) and 1 (inclusive).
		[APIMethod]
		public static float RandomValue()
		{
			return Random.value;
		}

		/// Returns a random value between $0 (inclusive) and $1 (inclusive).
		[APIMethod]
		public static float RandomBetween(float min, float max)
		{
			return Random.Range(min, max);
		}

		/// Returns a random value between $0 (inclusive) and $1 (exclusive).
		[APIMethod]
		public static int RandomBetween(int min, int max)
		{
			return Random.Range(min, max);
		}
	}
}