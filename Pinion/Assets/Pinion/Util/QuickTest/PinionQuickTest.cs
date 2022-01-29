using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pinion;
using Pinion.Compiler;
using System.Linq;

public class PinionQuickTest : EditorWindow
{
	private string textInput = string.Empty;
	private List<(MessageType, string)> messages = new List<(MessageType, string)>();


	[MenuItem("Window/Pinion/Pinion Quick Test")]
	private static void ShowWindow()
	{
		var window = GetWindow<PinionQuickTest>();
		window.titleContent = new GUIContent("Pinion Quick Test");
		window.Show();
	}

	private void OnGUI()
	{
		textInput = EditorGUILayout.TextArea(textInput, GUILayout.MinHeight(300));

		if (GUILayout.Button("Compile"))
		{
			Compile();
		}

		foreach ((MessageType, string) message in messages)
		{
			EditorGUILayout.HelpBox(message.Item2, message.Item1);
		}
	}

	private void Compile()
	{
		messages.Clear();
		PinionCompiler.Compile(textInput, ErrorLogHandler);

		if (!messages.Any(m => m.Item1 == MessageType.Error))
		{
			messages.Add((MessageType.Info, "Compiled successfully."));
		}
	}

	private void ErrorLogHandler(string message)
	{
		messages.Add((MessageType.Error, message));
	}
}