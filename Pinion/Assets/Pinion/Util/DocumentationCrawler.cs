using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using Pinion;
using System.Reflection;
using System;
using Pinion.Compiler.Internal;

public class DocumentationCrawler : EditorWindow
{

	[MenuItem("ByteCodeTest/DocumentationCrawler")]
	private static void ShowWindow()
	{
		var window = GetWindow<DocumentationCrawler>();
		window.titleContent = new GUIContent("DocumentationCrawler");
		window.Show();
	}

	private void OnGUI()
	{
		if (GUILayout.Button("Generate Documentation"))
		{
			GenerateDocumentation();
		}
	}

	private void ErrorDisplay(string message)
	{

	}

	private void GenerateDocumentation()
	{
		StringBuilder stringBuilder = new StringBuilder();

		IEnumerable<MethodInfo> allMethodsInAPISources = GetAllAPISourceMethods();

		foreach (MethodInfo methodInSource in allMethodsInAPISources)
		{
			APIMethodAttribute methodAttribute = methodInSource.GetCustomAttribute(typeof(APIMethodAttribute), false) as APIMethodAttribute;

			if (methodAttribute == null) // not an API method
				continue;

			if (methodAttribute.HasFlag(APIMethodFlags.Internal))
				stringBuilder.Append("[INTERNAL] ");


			Type returnType = methodInSource.ReturnType;

			if (returnType == typeof(void))
			{
				stringBuilder.Append("void");
			}
			else
			{
				stringBuilder.Append(TypeNameShortHands.GetSimpleTypeName(methodInSource.ReturnType));
			}

			stringBuilder.Append(" ");

			stringBuilder.Append(methodInSource.Name);
			stringBuilder.Append("(");

			ParameterInfo[] array = methodInSource.GetParameters();
			for (int i = 0; i < array.Length; i++)
			{
				ParameterInfo info = array[i];

				if (info.ParameterType == typeof(PinionContainer))
					continue;

				stringBuilder.Append(TypeNameShortHands.GetSimpleTypeName(info.ParameterType));
				stringBuilder.Append(" ");
				stringBuilder.Append(info.Name);

				if (i < array.Length - 1)
					stringBuilder.Append(", ");
			}

			stringBuilder.Append(")");

			stringBuilder.AppendLine();
		}

		Debug.Log(stringBuilder.ToString());
	}

	private static IEnumerable<MethodInfo> GetAllAPISourceMethods()
	{
		BindingFlags methodBindingFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			// This could be expanded or made less restrictive if need be.
			// For our current purposes however, this is perfectly fine. Saves iterating over a whole bunch of types that couldn't possibly have our own custom attribute anyway.
			if (assembly.GetName().Name != "Assembly-CSharp")
				continue;

			foreach (Type type in assembly.GetTypes())
			{
				// only includes classes marked as APISource
				if (type.GetCustomAttribute(typeof(APISourceAttribute), false) == null)
					continue;

				foreach (MethodInfo methodInfo in type.GetMethods(methodBindingFlags))
				{
					yield return methodInfo;
				}
			}
		}
	}
}