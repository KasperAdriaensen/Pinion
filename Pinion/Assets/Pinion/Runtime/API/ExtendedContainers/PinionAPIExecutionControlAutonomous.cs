using System.Collections;
using System.Collections.Generic;
using Pinion.Documentation;
using UnityEngine;

namespace Pinion.ExtendedContainers
{
	[APISource]
	[DocSourceDisplayName("Script Execution")]
	public static class PinionAPIExecutionControlAutonomous
	{
		/// Pauses the script for $1 seconds and makes it resume from this point afterwards.
		/// #code
		/// // show a message
		/// Sleep(2f)
		/// // show another message
		/// #endcode
		[APIMethod]
		public static void Sleep(PinionContainerAutonomous container, float seconds)
		{
			container.SleepForTime(seconds);
		}
	}
}
