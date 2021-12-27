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

	private void GenerateDocumentation(bool includeInternal = false)
	{
		StringBuilder stringBuilder = new StringBuilder();

		List<Type> allAPISources = new List<Type>();
		PinionAPI.StoreAllAPISources(allAPISources);
		List<(APIMethodAttribute, MethodInfo)> allMethodsInAPISource = new List<(APIMethodAttribute, MethodInfo)>();

		foreach (Type source in allAPISources)
		{
			allMethodsInAPISource.Clear();
			PinionAPI.StoreAPIMethodsForSource(source, allMethodsInAPISource);

			stringBuilder.Append(source.Name);
			stringBuilder.Append(" ========================================================");
			stringBuilder.AppendLine();

			foreach ((APIMethodAttribute, MethodInfo) methodInSource in allMethodsInAPISource)
			{
				APIMethodAttribute methodAttribute = methodInSource.Item1;
				MethodInfo methodInfo = methodInSource.Item2;

				if (methodAttribute.HasFlag(APIMethodFlags.Internal))
				{
					if (!includeInternal)
						continue;
					else
						stringBuilder.Append("[INTERNAL] ");
				}

				Type returnType = methodInfo.ReturnType;

				if (returnType == typeof(void))
				{
					stringBuilder.Append("void");
				}
				else
				{
					stringBuilder.Append(TypeNameShortHands.GetSimpleTypeName(methodInfo.ReturnType));
				}

				stringBuilder.Append(" ");

				stringBuilder.Append(methodInfo.Name);
				stringBuilder.Append("(");

				ParameterInfo[] array = methodInfo.GetParameters();
				for (int i = 0; i < array.Length; i++)
				{
					ParameterInfo info = array[i];

					Type parameterType = info.ParameterType;

					if (parameterType == typeof(PinionContainer) || parameterType.IsSubclassOf(typeof(PinionContainer)))
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
			stringBuilder.AppendLine();
		}

		Debug.Log(stringBuilder.ToString());
	}
}