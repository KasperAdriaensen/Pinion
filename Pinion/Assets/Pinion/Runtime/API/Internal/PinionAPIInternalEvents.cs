namespace Pinion.Internal
{
	[APISource]
	public static class PinionAPIInternalEvents
	{
		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.InitBegin)]
		public static void InitBegin(PinionContainer container)
		{
			container.OnInitBegin();
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.InitEnd)]
		public static void InitEnd(PinionContainer container)
		{
			container.OnInitEnd();
		}

		// Currently not used, easily restored.
		// 	[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		// [APIInternalMethodIdentifier(PinionAPIInternalIDs.InternalIDMainBegin)]
		// public static void MainBegin(PinionContainer container)
		// {

		// }

		// 	[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		// [APIInternalMethodIdentifier(PinionAPIInternalIDs.InternalIDMainEnd)]
		// public static void MainEnd(PinionContainer container)
		// {

		// }
	}
}