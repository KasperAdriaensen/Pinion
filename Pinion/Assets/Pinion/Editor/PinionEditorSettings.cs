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
		compileDebug,
		runtimeDebug
	};

	private const string compileDebug = "PINION_COMPILE_DEBUG";
	private bool compileDebugActive = false;

	private const string runtimeDebug = "PINION_RUNTIME_DEBUG";
	private bool runtimeDebugActive = false;


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
		runtimeDebugActive = CurrentDefines.Contains(runtimeDebug);
	}

	private void OnGUI()
	{
		if (EditorApplication.isCompiling)
		{
			EditorGUILayout.HelpBox("Compiling...", MessageType.Info);
			return;
		}

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.HelpBox("If enabled, compilation of Pinion scripts will output highly verbose, step-by-step information. Will have performance impact on compilation. Will never be active in builds.", MessageType.Info);
		compileDebugActive = EditorGUILayout.Toggle("Debug Pinion compiler", compileDebugActive);

		EditorGUILayout.HelpBox("If enabled, execution of Pinion scripts will output highly verbose, step-by-step information. Will have a major performance impact. Will never be active in builds.", MessageType.Info);
		runtimeDebugActive = EditorGUILayout.Toggle("Debug Pinion runtime", runtimeDebugActive);


		if (EditorGUI.EndChangeCheck())
		{
			ApplyDefines();
		}
	}

	private void ApplyDefines()
	{
		List<string> newDefines = CurrentDefines;
		// remove of all of our defines, keep the ones added by other code
		newDefines.RemoveAll(define => projectDefines.Contains(define));

		if (compileDebugActive)
			newDefines.Add(compileDebug);

		if (runtimeDebugActive)
			newDefines.Add(runtimeDebug);

		string concatenatedDefines = string.Join(";", newDefines.ToArray());

		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, concatenatedDefines);
	}
}
