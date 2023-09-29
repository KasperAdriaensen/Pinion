using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Pinion.Editor
{
	public class PinionSettings : EditorWindow
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

		private static List<string> possiblePinionDefines = new List<string>
		{
			compileDebug,
			runtimeDebug,
			logCompileErrorsAlways,
			logCompileErrorsEditor,
			logCompileErrorsNever
		};

		private const string compileDebug = "PINION_COMPILE_DEBUG";
		private bool compileDebugActive = false;

		private const string runtimeDebug = "PINION_RUNTIME_DEBUG";
		private bool runtimeDebugActive = false;

		private const string logCompileErrorsAlways = "PINION_LOG_COMPILE_ERRORS_ALWAYS";
		private const string logCompileErrorsEditor = "PINION_LOG_COMPILE_ERRORS_EDITOR";
		private const string logCompileErrorsNever = "PINION_LOG_COMPILE_ERRORS_NEVER";
		private LogCompileErrorOptions logCompileErrorOption = LogCompileErrorOptions.Always;

		public enum LogCompileErrorOptions
		{
			Always = 0,
			InEditorOnly = 1,
			Never = 2
		}

		[MenuItem("Window/Pinion/Pinion Settings")]
		private static void ShowWindow()
		{
			var window = GetWindow<PinionSettings>();
			window.titleContent = new GUIContent("Pinion Settings");

			// This doesn't actually work on 2020.3 (yeah...), but the default message is decent enough.
			window.saveChangesMessage = "Certain changed settings have not been applied. Apply them now?";
			window.Show();
		}

		private void OnEnable()
		{
			compileDebugActive = CurrentDefines.Contains(compileDebug);
			runtimeDebugActive = CurrentDefines.Contains(runtimeDebug);
			logCompileErrorOption = ReadLogCompileErrorOption();
		}

		private LogCompileErrorOptions ReadLogCompileErrorOption()
		{
			if (CurrentDefines.Contains(logCompileErrorsNever))
			{
				return LogCompileErrorOptions.Never;
			}
			else if (CurrentDefines.Contains(logCompileErrorsEditor))
			{
				return LogCompileErrorOptions.InEditorOnly;
			}

			return LogCompileErrorOptions.Always;
		}

		private void OnGUI()
		{
			if (EditorApplication.isCompiling)
			{
				EditorGUILayout.HelpBox("Compiling...", MessageType.Info);
				return;
			}

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("Debug  options", EditorStyles.largeLabel);

			GUILayout.BeginVertical(EditorStyles.helpBox);

			compileDebugActive = EditorGUILayout.Toggle("Debug Pinion compiler", compileDebugActive);
			GUILayout.Label("If enabled, compilation of Pinion scripts will output highly verbose, step-by-step information. Will have performance impact on compilation. Will never be active in builds.", EditorStyles.wordWrappedMiniLabel);
			GUILayout.Space(14f);
			runtimeDebugActive = EditorGUILayout.Toggle("Debug Pinion runtime", runtimeDebugActive);
			GUILayout.Label("If enabled, execution of Pinion scripts will output highly verbose, step-by-step information. Will have a major performance impact. Will never be active in builds.", EditorStyles.wordWrappedMiniLabel);
			GUILayout.EndVertical();


			GUILayout.Space(14f);
			GUILayout.Label("Compilation options", EditorStyles.largeLabel);


			GUILayout.BeginVertical(EditorStyles.helpBox);

			EditorGUIUtility.labelWidth = 200; // default size is too short
			logCompileErrorOption = (LogCompileErrorOptions)EditorGUILayout.EnumPopup("Compile errors in Unity log", logCompileErrorOption);
			EditorGUIUtility.labelWidth = 0; // 0 = return to default size
			GUILayout.Label("Determines whether compilation errors are also outputted to the Unity console/log. The Unity engine always has some small overhead for writing to the log.", EditorStyles.wordWrappedMiniLabel);


			string compileErrorOptionMessage = string.Empty;
			if (logCompileErrorOption == LogCompileErrorOptions.Always)
			{
				compileErrorOptionMessage = "Compilation errors are always outputted to the Unity log/console. You are free to do additional error message handling when compiling.";
			}
			else if (logCompileErrorOption == LogCompileErrorOptions.InEditorOnly)
			{
				compileErrorOptionMessage = "Compilation errors are outputted to the Unity log/console in editor, but omitted in builds. You are free to do additional error message handling when compiling.";
			}
			else if (logCompileErrorOption == LogCompileErrorOptions.Never)
			{
				compileErrorOptionMessage = "Compilation errors are never outputted to the Unity log/console. You will have to provide custom error message handling to view compilation errors.";
			}

			EditorGUILayout.HelpBox(compileErrorOptionMessage, MessageType.Info);

			GUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
			{
				hasUnsavedChanges = true;
			}

			GUILayout.Space(14f);

			GUI.enabled = hasUnsavedChanges;
			if (GUILayout.Button("Apply changes"))
			{
				ApplyDefines();
			}
			GUI.enabled = true;
		}

		public override void SaveChanges()
		{
			ApplyDefines();
			base.SaveChanges();
		}

		private void ApplyDefines()
		{
			List<string> newDefines = CurrentDefines;
			// remove of all of our defines, keep the ones added by other code
			newDefines.RemoveAll(define => possiblePinionDefines.Contains(define));

			if (compileDebugActive)
				newDefines.Add(compileDebug);

			if (runtimeDebugActive)
				newDefines.Add(runtimeDebug);

			if (logCompileErrorOption == LogCompileErrorOptions.InEditorOnly)
			{
				newDefines.Add(logCompileErrorsEditor);
			}
			else if (logCompileErrorOption == LogCompileErrorOptions.Never)
			{
				newDefines.Add(logCompileErrorsNever);
			}
			else
			{
				newDefines.Add(logCompileErrorsAlways);
			}

			string concatenatedDefines = string.Join(";", newDefines.ToArray());
			PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, concatenatedDefines);

			hasUnsavedChanges = false;
		}
	}
}