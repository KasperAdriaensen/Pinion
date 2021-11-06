using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion;

[APISource]
public static class DroneAPI
{
	[APIMethod]
	public static float GetDroneHeightAbsolute()
	{
		GameObject drone = GameObject.Find("POCDrone");

		if (drone != null)
			return drone.transform.position.y;

		return 0f;
	}

	[APIMethod]
	public static void DisplayMessage(string message, string id)
	{
		ExampleGameUI.Instance.DisplayMessage(message, id);
	}

	[APIMethod]
	public static void RemoveMessage(string id)
	{
		ExampleGameUI.Instance.RemoveMessage(id);
	}
}
