using System.Collections;
using System.Collections.Generic;
using Pinion;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIExecutionControl
	{
		[APIMethod]
		public static void Sleep(PinionContainer container)
		{
			container.Sleep();
		}

		[APIMethod]
		public static void Stop(PinionContainer container)
		{
			container.Stop();
		}
	}
}
