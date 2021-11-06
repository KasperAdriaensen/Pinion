using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Utility class to initialize several basic things at game start - cleaner than pushing unrelated stuff into ContentManagerIgniter
public static class RuntimeInitialize
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnGameStartBeforeLoad()
	{
		// We make sure the entire thread uses invariant culture. 
		// If not, on certain systems, save files and configurations may throw errors because a comma is expected instead of a point as decimal marker.
		// This is a little cleaner than passing the CultureInfo with every float.TryParse (risking we overlook one).
		// Note: what about other threads? Do threads spawned from this one copy the CurrentCulture?
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
	}
}
