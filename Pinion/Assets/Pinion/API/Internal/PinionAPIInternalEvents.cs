using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIInternalEvents
	{
		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDInitBegin)]
		public static void InitBegin(PinionContainer container)
		{
			container.OnInitBegin();
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDInitEnd)]
		public static void InitEnd(PinionContainer container)
		{
			container.OnInitEnd();
		}

		// Currently not used, easily restored.
		// 	[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		// [APIInternalMethodIdentifier(PinionAPI.InternalIDMainBegin)]
		// public static void MainBegin(PinionContainer container)
		// {

		// }

		// 	[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		// [APIInternalMethodIdentifier(PinionAPI.InternalIDMainEnd)]
		// public static void MainEnd(PinionContainer container)
		// {

		// }
	}
}