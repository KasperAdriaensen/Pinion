namespace Pinion
{
	using Pinion.Documentation;

	[APISource]
	[DocSourceDisplayName("Script Execution")]
	public static class PinionAPIExecutionControl
	{
		/// Pauses the script and makes it resume from this point the next time the calling container executes.
		/// See documentation of specific container typed for their implementation of the Sleep instruction.
		/// #code
		/// while ($keepWaiting)
		/// 	$name()
		/// endwhile
		/// #endcode
		[APIMethod]
		public static void Sleep(PinionContainer container)
		{
			container.Sleep();
		}

		/// Stops container execution.
		[APIMethod]
		public static void Stop(PinionContainer container)
		{
			container.Stop();
		}
	}
}
