using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleGameUI : MonoBehaviour
{
	[SerializeField]
	private ExampleGameUIMessage messagePrefab = null;


	// Quick and dirty singleton implementation. Remove.
	private static ExampleGameUI instance = null;
	public static ExampleGameUI Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<ExampleGameUI>();

			return instance;
		}
	}

	private List<ExampleGameUIMessage> messagesPool = new List<ExampleGameUIMessage>();
	private Dictionary<string, ExampleGameUIMessage> messagesWithIDPool = new Dictionary<string, ExampleGameUIMessage>();

	public void DisplayMessage(string message)
	{
		DisplayMessage(message, null);
	}

	public void DisplayMessage(string message, string id)
	{
		GetMessageFromPool(id).Initialize(message);
	}

	public void RemoveMessage(string id)
	{
		if (messagesWithIDPool.ContainsKey(id))
		{
			messagesWithIDPool[id].Hide(true);
		}
	}


	private ExampleGameUIMessage GetMessageFromPool(string id = null)
	{
		ExampleGameUIMessage returnedMessage = null;
		if (!string.IsNullOrEmpty(id))
		{
			if (messagesWithIDPool.ContainsKey(id))
			{
				returnedMessage = messagesWithIDPool[id];
			}
		}
		else
		{
			foreach (ExampleGameUIMessage message in messagesPool)
			{
				if (!message.InUse)
				{
					returnedMessage = message;
					break;
				}
			}
		}

		if (returnedMessage == null)
		{
			returnedMessage = GameObject.Instantiate<ExampleGameUIMessage>(messagePrefab);
			returnedMessage.transform.SetParent(messagePrefab.transform.parent, false);

			if (!string.IsNullOrEmpty(id))
			{
				messagesWithIDPool.Add(id, returnedMessage);
			}
			else
			{
				messagesPool.Add(returnedMessage);
			}
		}

		return returnedMessage;
	}






}
