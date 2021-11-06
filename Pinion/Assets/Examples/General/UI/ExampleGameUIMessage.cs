using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleGameUIMessage : MonoBehaviour
{
	public float stayTime = 4f;
	public float fadeOutTime = 0.5f;

	[SerializeField]
	private Text messageText = null;
	[SerializeField]
	private CanvasGroup canvasGroup = null;

	private float lifeTimer = 0f;

	public bool InUse
	{
		get
		{
			return this.gameObject.activeSelf;
		}
	}

	public void Initialize(string text)
	{
		lifeTimer = 0f;

		if (messageText.text != text)
			messageText.text = text;

		canvasGroup.alpha = 1f;
		this.gameObject.SetActive(true);
	}

	public void Hide(bool instant = false)
	{
		if (InUse)
		{
			if (instant)
			{
				Remove();           // remove immediately
			}
			else
			{
				lifeTimer = stayTime; // begin fade immediately
			}
		}
	}

	private void Update()
	{
		lifeTimer += Time.deltaTime;

		if (lifeTimer >= stayTime + fadeOutTime)
		{
			Remove();
		}
		else if (lifeTimer >= stayTime)
		{
			canvasGroup.alpha = 1f - Mathf.Clamp01((lifeTimer - stayTime) / fadeOutTime);
		}
	}

	private void Remove()
	{
		this.gameObject.SetActive(false);
	}



}
