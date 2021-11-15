using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Unity
{
	[APISource]
	public static class PinionAPIExecutionControlLooping
	{
		[APIMethod]
		public static void Sleep(PinionContainerLooping container, float seconds)
		{
			container.SleepForTime(seconds);
		}
	}
}
