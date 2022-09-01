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
	private int executionTimeoutMs = PinionContainer.executionTimeoutMsDefault;
	private PinionContainer compileResult = null;
	private bool timeCompilation = false;
	private bool timeExecution = false;
	private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch(); // System.Diagnostics namespace clashes with Unity's Debug
	private long compileTime = -1;
	private long executeTime = -1;
	private bool compilationIncludedAPIBuild = false;

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

		GUILayout.Space(20f);
		GUILayout.BeginHorizontal();
		executionTimeoutMs = EditorGUILayout.IntField("Execution timeout (ms)", executionTimeoutMs);
		if (GUILayout.Button("Set to default"))
		{
			executionTimeoutMs = PinionContainer.executionTimeoutMsDefault;
		}
		GUILayout.EndHorizontal();

		timeCompilation = GUILayout.Toggle(timeCompilation, "Time compilation");
		timeExecution = GUILayout.Toggle(timeExecution, "Time execution");

#if PINION_COMPILE_DEBUG
		if (timeCompilation || timeExecution)
		{
			EditorGUILayout.HelpBox("The debugging symbol is currently defined. There will be significant overhead from logging and generating output. Timing results will not be indicative.", MessageType.Warning);
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
			EditorGUILayout.HelpBox($"Compilation time included API building. This is called the first time any Pinion container is compiled during the session, but can also be called at an earlier time for optimization purposes.", MessageType.Info);
		}

		GUILayout.EndVertical();

		GUILayout.Space(20f);

		if (GUILayout.Button("Compile"))
		{
			Compile();
		}

		if (GUILayout.Button("Compile & Execute"))
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
		compilationIncludedAPIBuild = !PinionAPI.BuiltSuccessfully;

		stopwatch.Reset();
		stopwatch.Start();

		compileResult = PinionCompiler.Compile(textInput, CompileErrorHandler);

		stopwatch.Stop();
		if (timeCompilation)
			compileTime = stopwatch.ElapsedTicks;

		if (!messages.Any(m => m.Item1 == MessageType.Error))
		{
			messages.Add((MessageType.Info, "Compiled successfully."));
		}
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

			compileResult.Run(null);

			stopwatch.Stop();
			if (timeExecution)
				executeTime = stopwatch.ElapsedTicks;
		}
	}

	private void CompileErrorHandler(string message)
	{
		messages.Add((MessageType.Error, message));
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