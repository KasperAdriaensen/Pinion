using System.Collections;
using System.Collections.Generic;
using Pinion;
using UnityEngine;
using UnityEngine.UI;
using Pinion.Compiler;
using Pinion.ExtendedContainers;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PinionTest : MonoBehaviour
{
	public TextAsset testText = null;
	[SerializeField]
	private Text errorDisplay = null;

	private List<string> errorMessages = new List<string>();

	private PinionContainer currentScriptContainer = null;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
#if UNITY_EDITOR
			// always update asset before import
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(testText));
#endif

			if (currentScriptContainer != null)
			{
				currentScriptContainer.Stop();
				currentScriptContainer = null;
			}

			errorMessages.Clear();

			currentScriptContainer = PinionCompiler.Compile<PinionContainerLooping>(testText.text, AddCompileError);

			errorDisplay.text = ("Compiling..." + System.Environment.NewLine + System.Environment.NewLine);

			if (currentScriptContainer != null)
			{
				Output(LogType.Log, "Compiled succesfully!");
				Output(LogType.Log, "Running script...");

				currentScriptContainer.Run(Output, ("$$lowerLimit", 7f));
			}
			else
			{
				Output(LogType.Log, "Compilation failed with the following errors:");

				foreach (string message in errorMessages)
				{
					Output(LogType.Error, $"- {message}");
				}
			}
		}
	}

	private void AddCompileError(string message)
	{
		errorMessages.Add(message);
	}

	private void Output(LogType logType, string message)
	{
		switch (logType)
		{
			case LogType.Error:
				errorDisplay.text += $"<color=red>{message}</color>{System.Environment.NewLine}";
				break;
			case LogType.Warning:
				errorDisplay.text += $"<color=yellow>{message}</color>{System.Environment.NewLine}";
				break;
			default:
				errorDisplay.text += $"{message}{System.Environment.NewLine}";
				break;
		}


	}

}
