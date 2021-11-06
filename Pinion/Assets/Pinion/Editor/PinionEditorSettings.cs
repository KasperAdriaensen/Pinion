using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class PinionEditorSettings : EditorWindow
{
	public List<string> CurrentDefines
	{
		get
		{
			char[] separator = { ';' };
			string currentSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
			return new List<string>(currentSettings.Split(separator));
		}
	}

	private static List<string> projectDefines = new List<string>
	{
		compileDebug
	};

	private const string compileDebug = "PINION_COMPILE_DEBUG";
	private bool compileDebugActive = false;


	[MenuItem("Window/Pinion/Pinion Settings")]
	private static void ShowWindow()
	{
		var window = GetWindow<PinionEditorSettings>();
		window.titleContent = new GUIContent("Pinion Settings");
		window.Show();
	}

	private void OnEnable()
	{
		compileDebugActive = CurrentDefines.Contains(compileDebug);
	}

	private void OnGUI()
	{
		if (EditorApplication.isCompiling)
		{
			EditorGUILayout.HelpBox("Compiling...", MessageType.Info);
			return;
		}

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.HelpBox("If enabled, compilation and execution of any scripts will output highly verbose debug information. Likely to have a performance impact. Will never be active in builds.", MessageType.Info);
		compileDebugActive = EditorGUILayout.Toggle("Debug Pinion compiler", compileDebugActive);

		if (EditorGUI.EndChangeCheck())
		{
			ApplyDefines();
		}
	}

	private void ApplyDefines()
	{
		List<string> newDefines = CurrentDefines;
		// remove of all of our defines, keep the ones added by other code
		newDefines.RemoveAll((string define) => projectDefines.Contains(define));

		if (compileDebugActive)
			newDefines.Add(compileDebug);

		string concatenatedDefines = string.Join(";", newDefines.ToArray());

		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, concatenatedDefines);
	}
}
