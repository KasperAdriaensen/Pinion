using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pinion;
using Pinion.Compiler;
using System.Linq;
using System;
using System.Reflection;

namespace Pinion.Editor
{
	public class PinionQuickTest : EditorWindow
	{
		private string textInput = string.Empty;
		private List<(MessageType, string)> messages = new List<(MessageType, string)>();
		private int executionTimeoutMs = PinionContainer.executionTimeoutMsDefault;
		private PinionContainer compileResult = null;
		private bool timeCompilation = false;
		private bool timeExecution = false;
		private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch(); // System.Diagnostics namespace clashes with Unity's Debug
		private long compileTime = -1;
		private long executeTime = -1;
		private bool compilationIncludedAPIBuild = false;
		private TextAsset scriptTextAsset = null;
		private string stringToCompile = null;

		private int selectedContainerType = 0;
		// These two have to be kept in sync. Not great design, but EditorGUILayout.Popup
		// complicates things to much to do it any other way.
		private List<Type> containerTypes = new List<Type>();
		private string[] containerTypeNames = null;
		private bool uncompiledChanges = false;

		[MenuItem("Window/Pinion/Pinion Quick Test")]
		private static void ShowWindow()
		{
			var window = GetWindow<PinionQuickTest>();
			window.titleContent = new GUIContent("Pinion Quick Test");
			window.Show();
		}

		private void OnEnable()
		{
			containerTypeNames = StoreContainerTypes(containerTypes);
		}

		private void OnGUI()
		{
			selectedContainerType = EditorGUILayout.Popup("Target container type", selectedContainerType, containerTypeNames);

			GUILayout.Label("Enter Pinion code or TextAsset.");
			GUILayout.BeginHorizontal();

			EditorGUI.BeginChangeCheck();
			scriptTextAsset = (TextAsset)EditorGUILayout.ObjectField(scriptTextAsset, typeof(TextAsset), false);
			if (scriptTextAsset && GUILayout.Button("Clear", GUILayout.Width(50)))
			{
				scriptTextAsset = null;
			}
			if (EditorGUI.EndChangeCheck())
			{
				uncompiledChanges = true;
			}

			GUILayout.EndHorizontal();

			if (scriptTextAsset != null)
			{
				GUI.enabled = false;
				EditorGUILayout.TextArea(scriptTextAsset.text, GUILayout.MinHeight(300));
				stringToCompile = scriptTextAsset.text;
				GUI.enabled = true;
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				textInput = EditorGUILayout.TextArea(textInput, GUILayout.MinHeight(300));
				stringToCompile = textInput;
				if (EditorGUI.EndChangeCheck())
				{
					uncompiledChanges = true;
				}
			}

			if (uncompiledChanges)
			{
				GUILayout.Label("Uncompiled changes", EditorStyles.miniLabel);
			}
			else
			{
				// Stupid hack to make the rest of the UI not jump around when there are no more uncompiled changes.
				// string.Empty will still jump a little.
				GUILayout.Label(" ", EditorStyles.miniLabel);
			}

			GUILayout.Space(20f);
			GUILayout.BeginHorizontal();
			executionTimeoutMs = EditorGUILayout.IntField("Execution timeout (ms)", executionTimeoutMs);
			if (GUILayout.Button("Set to default"))
			{
				executionTimeoutMs = PinionContainer.executionTimeoutMsDefault;
			}
			GUILayout.EndHorizontal();

			timeCompilation = GUILayout.Toggle(timeCompilation, "Time compilation");
#if PINION_COMPILE_DEBUG
			if (timeCompilation)
			{
				EditorGUILayout.HelpBox("The compilation debugging symbol is currently defined. There will be significant overhead from logging and generating output. Timing results will not be indicative.", MessageType.Warning);
			}
#endif
			timeExecution = GUILayout.Toggle(timeExecution, "Time execution");
#if PINION_RUNTIME_DEBUG
			if (timeExecution)
			{
				EditorGUILayout.HelpBox("The runtime debugging symbol is currently defined. There will be significant overhead from logging and generating output. Timing results will not be indicative.", MessageType.Warning);
			}
#endif


			GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.BeginHorizontal();
			if (timeCompilation && compileTime >= 0)
			{
				double ms = ((double)compileTime / System.Diagnostics.Stopwatch.Frequency) * 1000;

				GUILayout.Label($"Compile time: {ms} ms.", EditorStyles.boldLabel);
			}

			if (timeExecution && executeTime >= 0)
			{
				double ms = ((double)executeTime / System.Diagnostics.Stopwatch.Frequency) * 1000;
				GUILayout.Label($"Execute time: {ms} ms.", EditorStyles.boldLabel);
			}

			GUILayout.EndHorizontal();

			if (timeCompilation && compilationIncludedAPIBuild)
			{
				EditorGUILayout.HelpBox($"Compilation time included API building. This happens the first time any Pinion container is compiled during a session (play mode) or after a recompile (edit mode). In play mode, it can also be called at a time of your choosing. Compile again or press Build API to prevent this.", MessageType.Info);
			}

			GUILayout.EndVertical();


			GUILayout.Space(20f);

			GUI.enabled = !PinionAPI.BuiltSuccessfully;
			if (GUILayout.Button("Build API"))
			{
				PinionAPI.BuildAPI(Debug.Log);
			}
			GUI.enabled = true;

			if (GUILayout.Button("Compile"))
			{
				Compile();
			}

			GUI.enabled = compileResult != null;
			if (GUILayout.Button("Run"))
			{
				Run();
			}
			GUI.enabled = true;

			if (GUILayout.Button("Compile & Run"))
			{
				Compile();
				Run();
			}


			foreach ((MessageType, string) message in messages)
			{
				EditorGUILayout.HelpBox(message.Item2, message.Item1);
			}
		}

		private void Compile()
		{
			messages.Clear();

			compileTime = -1;
			executeTime = -1; // also want to reset if we're only compiling

			// Keep track of whether compilation also included api building.
			// If it hasn't happened yet, it will take place now.
			compilationIncludedAPIBuild = !PinionAPI.BuiltSuccessfully;

			stopwatch.Reset();
			stopwatch.Start();

			//compileResult = PinionCompiler.Compile(stringToCompile, CompileErrorHandler);
			compileResult = PinionCompiler.CompileForEditor(containerTypes[selectedContainerType], stringToCompile, HandleCompileError);

			stopwatch.Stop();
			if (timeCompilation)
				compileTime = stopwatch.ElapsedTicks;

			if (!messages.Any(m => m.Item1 == MessageType.Error))
			{
				messages.Add((MessageType.Info, "Compiled successfully."));
			}

			uncompiledChanges = false;
		}

		private void Run()
		{
			if (compileResult != null)
			{
				compileResult.ExecutionTimeoutMs = executionTimeoutMs;
				Debug.Log("Running Pinion quick test...");

				executeTime = -1;
				stopwatch.Reset();
				stopwatch.Start();

				try
				{
					compileResult.Run(HandleMessage);
				}
				catch
				{
					string message = $"Unhandled exception during execution. Check console.";
					HandleMessage(LogType.Exception, message);
					throw;
				}

				stopwatch.Stop();
				if (timeExecution)
					executeTime = stopwatch.ElapsedTicks;
			}
		}

		private void HandleCompileError(string message)
		{
			HandleMessage(LogType.Error, message);
		}

		private void HandleMessage(LogType logtype, string message)
		{
			MessageType messageType = MessageType.None;
			switch (logtype)
			{
				case LogType.Log:
					messageType = MessageType.Info;
					break;
				case LogType.Warning:
					messageType = MessageType.Warning;
					break;
				default:
					messageType = MessageType.Error;
					break;
			}

			messages.Add((messageType, message));
		}

		private string[] StoreContainerTypes(List<Type> returnTypes)
		{
			if (returnTypes == null)
				throw new ArgumentNullException(nameof(returnTypes));

			returnTypes.Clear();

			Type parentType = typeof(PinionContainer);
			returnTypes.Add(parentType);

			foreach (Type type in
				Assembly.GetAssembly(parentType).GetTypes()
				.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(parentType)))
			{
				returnTypes.Add(type);
			}

			return returnTypes.Select(t => t.Name).ToArray();
		}

		// No need to assign this again because the container's Log/LogWarning/LogError already calls the Unity logger too.
		// Keeping it around in case this changes.
		// private void RunTimeErrorHandler(LogType logType, string message)
		// {
		// 	switch (logType)
		// 	{
		// 		case LogType.Error:
		// 			Debug.LogError(message);
		// 			break;

		// 		case LogType.Warning:
		// 			Debug.LogWarning(message);
		// 			break;

		// 		default:
		// 			Debug.Log(message);
		// 			break;
		// 	}
		// }
	}
}