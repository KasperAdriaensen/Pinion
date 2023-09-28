namespace Pinion
{
	using Pinion.Documentation;
	using UnityEngine;

	[APISource]
	[DocSourceDisplayName("Time")]
	public static class PinionAPITime
	{
		/// Returns time since start of the game in seconds.
		[APIMethod]
		public static float GetTime()
		{
			return Time.time;
		}

		/// Returns the duration of the last game frame in seconds. Use this to e.g. add to a timer every frame.
		/// #code
		/// $timer += $name()
		/// #endcode
		[APIMethod]
		public static float GetDeltaTime()
		{
			return Time.deltaTime;
		}
	}
}