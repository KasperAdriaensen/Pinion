using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPITime
	{
		[APIMethod]
		public static float GetTime()
		{
			return Time.time;
		}

		[APIMethod]
		public static float GetLastFrameDuration()
		{
			return Time.deltaTime;
		}
	}
}