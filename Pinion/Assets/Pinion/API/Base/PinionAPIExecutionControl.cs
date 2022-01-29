using System.Collections;
using System.Collections.Generic;
using Pinion;
using Pinion.Documentation;
using UnityEngine;

namespace Pinion
{
	[APISource]
	[DocSourceDisplayName("Script Execution")]
	public static class PinionAPIExecutionControl
	{
		/// Pauses the script and makes it resume from this point in the next game frame.
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

		/// Stops script execution permanently.
		[APIMethod]
		public static void Stop(PinionContainer container)
		{
			container.Stop();
		}
	}
}
