using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using Pinion;
using System.Reflection;
using System;
using Pinion.Compiler.Internal;
using System.IO;
using Pinion.Documentation;
using System.Text.RegularExpressions;

namespace Pinion.Documentation
{
	public class DocumentationCrawler : EditorWindow
	{
		[SerializeField]
		private TextAsset[] prependTexts = null;

		[SerializeField]
		private string skipList = string.Empty;


		private const string typeColor = "#c4c8e5";
		private const string parameterNameEmphasis = "*";
		// See BuildFriendlyTypeDictionary
		private Dictionary<Type, string> friendlyTypeDictionary = null;

		private SerializedObject serializedObject = null;
		private SerializedProperty prop_prependTexts = null;
		private SerializedProperty prop_skipList = null;


		[MenuItem("Window/Pinion/Documentation Generator")]
		private static void ShowWindow()
		{
			var window = GetWindow<DocumentationCrawler>();
			window.titleContent = new GUIContent("Documentation Generator");
			window.Show();
		}

		private void OnEnable()
		{
			serializedObject = new SerializedObject(this);
			EditorGUIExtensions.AssignAllSerializedProperties<DocumentationCrawler>(this, serializedObject);
		}

		private void OnGUI()
		{
			serializedObject.Update();
			EditorGUILayout.HelpBox("Compiles a Markdown-formatted overview of all API instructions in this project. Experimental. Results may vary.", MessageType.Info);

			GUILayout.Space(20f);

			GUILayout.Label("Other text (Markdown-formatted) to add in front of auto-generated documentation.");
			EditorGUILayout.PropertyField(prop_prependTexts);
			GUILayout.Space(20f);

			GUILayout.Label("Classes to skip. Comma-separated.");
			if (GUILayout.Button("Fill with default skip list"))
			{
				EnterDefaultSkipList();
			}
			EditorGUILayout.PropertyField(prop_skipList);

			GUILayout.Space(50f);

			if (GUILayout.Button("Generate Documentation"))
			{
				GenerateDocumentation();
			}
			serializedObject.ApplyModifiedProperties();
		}

		private void EnterDefaultSkipList()
		{
			skipList = "PinionAPIAsserts";
		}

		private void ErrorDisplay(string message)
		{

		}

		private void GenerateDocumentation(bool includeInternal = false)
		{
			StringBuilder stringBuilder = new StringBuilder();


			for (int i = 0; i < prop_prependTexts.arraySize; i++)
			{
				TextAsset textAsset = (TextAsset)prop_prependTexts.GetArrayElementAtIndex(i).objectReferenceValue;

				if (textAsset != null)
				{
					stringBuilder.Append(textAsset.text);
				}
			}


			List<Type> allAPISources = new List<Type>();
			PinionAPI.StoreAllAPISources(allAPISources);
			List<(APIMethodAttribute, MethodInfo)> allMethodsInAPISource = new List<(APIMethodAttribute, MethodInfo)>();

			stringBuilder.AppendLine();
			stringBuilder.Append(" # Scripting Documentation");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();

			HashSet<string> skipClasses = new HashSet<string>(skipList.Split(','));

			foreach (Type source in allAPISources)
			{
				allMethodsInAPISource.Clear();
				PinionAPI.StoreAPIMethodsForSource(source, allMethodsInAPISource);

				if (allMethodsInAPISource.Count <= 0)
					continue;

				string sourceName = source.Name;

				if (skipClasses.Contains(sourceName))
					continue;

				string sourceFileText = FindSourceTextBasedOnClassName(sourceName);

				DocSourceDisplayNameAttribute nameAttribute = source.GetCustomAttribute<DocSourceDisplayNameAttribute>();

				if (nameAttribute != null)
					sourceName = nameAttribute.DisplayName;

				allMethodsInAPISource.RemoveAll(m => m.Item2.GetCustomAttribute<DocMethodHideAttribute>() != null);

				if (!includeInternal)
					allMethodsInAPISource.RemoveAll(m => m.Item1.HasFlag(APIMethodFlags.Internal));

				if (allMethodsInAPISource.Count <= 0)
					continue;

				stringBuilder.Append(" ## ");
				stringBuilder.Append(sourceName);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();

				foreach ((APIMethodAttribute, MethodInfo) methodInSource in allMethodsInAPISource)
				{
					APIMethodAttribute methodAttribute = methodInSource.Item1;
					MethodInfo methodInfo = methodInSource.Item2;

					if (methodAttribute.HasFlag(APIMethodFlags.Internal))
					{
						stringBuilder.Append("[INTERNAL] ");
					}

					string instructionName = methodInfo.Name;

					DocMethodOperatorReplaceAttribute operatorReplace = methodInfo.GetCustomAttribute<DocMethodOperatorReplaceAttribute>();
					bool treatAsOperator = false;

					if (operatorReplace != null)
					{
						string operatorString = operatorReplace.OperatorString;
						if (OperatorLookup.IsOperator(operatorString))
						{
							treatAsOperator = true;
							instructionName = operatorString;
						}
						else
						{
							Debug.LogWarning($"Trying to replace instruction {instructionName} with operator {operatorString}. {operatorString} is not a valid operator. Treating it as a regular instruction instead.");
						}
					}

					if (treatAsOperator)
						AppendOperator(stringBuilder, instructionName, methodInfo);
					else
						AppendInstruction(stringBuilder, instructionName, methodInfo);


					Type returnType = methodInfo.ReturnType;

					if (returnType != typeof(void))
					{
						stringBuilder.Append(" returns ");
						stringBuilder.Append(FormatTypeName(TypeNameShortHands.GetSimpleTypeName(methodInfo.ReturnType)));

					}

					stringBuilder.Append("  "); // Markdown line break
					stringBuilder.AppendLine();

					if (!string.IsNullOrEmpty(sourceFileText))
					{
						string documentation = ReadInFileDocumentation(sourceFileText, methodInfo);

						if (!string.IsNullOrEmpty(documentation))
						{
							stringBuilder.Append(documentation);
						}
					}

					stringBuilder.AppendLine();
				}

				stringBuilder.AppendLine();
			}

			string fileContents = stringBuilder.ToString();

			File.WriteAllText(UnityEngine.Application.dataPath + Path.DirectorySeparatorChar + "APIDocumentation.md", fileContents);
		}

		private void AppendInstruction(StringBuilder stringBuilder, string instructionName, MethodInfo methodInfo)
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(FormatInstructionName(instructionName));
			stringBuilder.Append("(");

			ParameterInfo[] parameters = methodInfo.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameter = parameters[i];
				Type parameterType = parameter.ParameterType;

				if (parameterType == typeof(PinionContainer) || parameterType.IsSubclassOf(typeof(PinionContainer)))
					continue;

				stringBuilder.Append(FormatTypeName(TypeNameShortHands.GetSimpleTypeName(parameter.ParameterType)));
				stringBuilder.Append(" ");

				stringBuilder.Append(FormatParameterName(parameter.Name));

				if (i < parameters.Length - 1)
					stringBuilder.Append(", ");
			}

			stringBuilder.Append(")");
		}


		private void AppendOperator(StringBuilder stringBuilder, string operatorString, MethodInfo methodInfo)
		{
			stringBuilder.Append(" ");

			OperatorInfo operatorInfo = OperatorLookup.GetOperatorInfo(operatorString);

			operatorString = "\\" + operatorString; // \ for Markdown escape

			int argumentCount = operatorInfo.argumentCount;
			int parameterCounter = 0;

			stringBuilder.Append("operator ");
			stringBuilder.Append(FormatInstructionName(operatorString));
			stringBuilder.Append(" ");

			ParameterInfo[] parameters = methodInfo.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameter = parameters[i];
				Type parameterType = parameter.ParameterType;

				if (parameterType == typeof(PinionContainer) || parameterType.IsSubclassOf(typeof(PinionContainer)))
					continue;

				if (parameterCounter >= (argumentCount - 1))
					stringBuilder.Append(", ");

				// if (parameterCounter >= (argumentCount - 1))
				// {
				// 	stringBuilder.Append("**");
				// 	stringBuilder.Append(operatorString);
				// 	stringBuilder.Append("**");
				// 	stringBuilder.Append(" ");
				// }

				stringBuilder.Append(FormatTypeName(TypeNameShortHands.GetSimpleTypeName(parameter.ParameterType)));
				stringBuilder.Append(" ");

				stringBuilder.Append(FormatParameterName(parameter.Name));
				stringBuilder.Append(" ");

				parameterCounter++;
			}
		}

		private const string regexClassDeclaration = @"class\s+";

		private string FindSourceTextBasedOnClassName(string className)
		{
			string projectRootDirectory = new DirectoryInfo(UnityEngine.Application.dataPath).Parent.ToString() + Path.DirectorySeparatorChar;

			string[] matchingGUIDs = AssetDatabase.FindAssets($"{className} t:Script");

			if (matchingGUIDs == null || matchingGUIDs.Length < 1)
				return string.Empty;

			foreach (string guid in matchingGUIDs)
			{
				string filePath = projectRootDirectory + AssetDatabase.GUIDToAssetPath(guid);
				string fileText = File.ReadAllText(filePath);

				if (!Regex.IsMatch(fileText, regexClassDeclaration + className))
					continue;

				return fileText;
			}

			Debug.LogWarning($"Failed to find any script file that matches class name {className}. Cannot read in-file documentation.");

			return string.Empty;
		}

		private string ReadInFileDocumentation(string fileText, MethodInfo methodInfo)
		{
			if (friendlyTypeDictionary == null)
				BuildFriendlyTypeDictionary();

			string methodRegex = $"{GetTypeString(methodInfo.ReturnType)}\\s+{methodInfo.Name}\\(";
			ParameterInfo[] parameters = methodInfo.GetParameters();

			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameter = parameters[i];
				methodRegex += $"\\s*{GetTypeString(parameter.ParameterType)}\\s+{parameter.Name}\\s*";

				if (i < parameters.Length - 1)
					methodRegex += ",";
			}

			methodRegex += "\\)";

			Match match = Regex.Match(fileText, methodRegex);

			if (!match.Success)
			{
				Debug.LogWarning($"Failed to locate method {methodInfo.Name} in its file with regex pattern {methodRegex}.");
				return string.Empty;
			}

			// This is wonky, convoluted and likely error-prone. This is by no means guaranteed to be watertight.
			// Long story short: 
			// FIRST a line starting with non-newlines, then exactly three ///, some stuff, and a new line; all this 1-n times. (No documentation is also valid.)
			// THEN a bunch of whatever. E.g. there could be Atttributes in between the documentation comment and method declaration. 
			// The bunch of whatever must NOT be { or }, so that we can't detect a match starting from the comment above a *previous* function higher up.
			// FINALLY the method declaration as already tested higher up.

			string documentationRegex = @"((?:^[^\n]*\/{3}.*\n)+)(?:[^\{\}])*" + methodRegex;
			match = Regex.Match(fileText, documentationRegex, RegexOptions.Multiline);
			if (!match.Success)
			{
				//Debug.LogWarning($"Failed to locate documentation in file with regex pattern {documentationRegex}");
				return string.Empty;
			}

			StringBuilder processedDocumentationBuilder = new StringBuilder();
			Regex commentStartStripRegex = new Regex(@"\s*\/{3} *");
			bool codeBlock = false;
			using (StringReader reader = new StringReader(match.Groups[1].Value)) // Capture group 1 is actual comment text
			{
				string line = string.Empty;
				do
				{
					line = reader.ReadLine();

					if (string.IsNullOrEmpty(line)) // Ignore blank lines or end of file.
						continue;

					line = commentStartStripRegex.Replace(line, string.Empty); // strip /// and leading whitespace

					if (line == "#code")
					{
						codeBlock = true;
						processedDocumentationBuilder.AppendLine();
						continue;
					}
					else if (line == "#endcode")
					{
						codeBlock = false;
						processedDocumentationBuilder.AppendLine();
						continue;
					}

					if (codeBlock)
						processedDocumentationBuilder.Append("\t"); // Markdown for code block

					processedDocumentationBuilder.Append(line);
					// We treat any linebreak in the comment as just a continuation of the same paragraph.
					// We insert one space to not splice sentences together in this case.
					processedDocumentationBuilder.AppendLine(" ");


					//processedDocumentationBuilder.AppendLine("  "); // Markdown linebreak

				}
				while (line != null);
			}

			string processDocumentation = processedDocumentationBuilder.ToString();

			// Replace $0, $1, $2, $n with matching parameter names.
			for (int i = 0; i < parameters.Length; i++)
			{
				Type parameterType = parameters[i].ParameterType;
				string searchPattern = $"\\${i}";
				if (parameterType == typeof(PinionContainer) || parameterType.IsSubclassOf(typeof(PinionContainer)))
				{
					if (Regex.IsMatch(processDocumentation, searchPattern))
					{
						Debug.LogError($"Method: {methodInfo.Name}. Replacing parameter index {i} with a parameter type inheriting from PinionContainer. Parameters of this type are not exposed. Including them in documentation is likely a mistake?");
					}
				}

				processDocumentation = Regex.Replace(processDocumentation, searchPattern, $"{FormatParameterName(parameters[i].Name)}");
			}

			// Replace $return with return type
			processDocumentation = Regex.Replace(processDocumentation, "\\$return", $"{FormatTypeName(GetTypeString(methodInfo.ReturnType))}");

			// Replace $return with instructionName
			processDocumentation = Regex.Replace(processDocumentation, "\\$name", $"{methodInfo.Name}");

			return processDocumentation;

			// Nested function for convenience
			string GetTypeString(Type type)
			{
				return friendlyTypeDictionary.ContainsKey(type) ? friendlyTypeDictionary[type] : type.Name.ToString();
			}
		}

		private string AddColorTags(string baseString, string colorValue)
		{
			return $"<span style=\"color:{colorValue}\">{baseString}</span>";
		}

		private string FormatParameterName(string baseString)
		{
			return $"`{baseString}`";
		}

		private string FormatTypeName(string baseString)
		{
			return AddColorTags(baseString, typeColor);
		}

		private string FormatInstructionName(string baseString)
		{
			return $"<span id=\"{baseString}\">**{baseString}**</span>"; // adds id for easy linking in an html page as an anchor link
		}


		// A quick and dirty thing for converting actual types to "C# friendly" version. Based on:
		// https://stackoverflow.com/questions/16984005/convert-c-friendly-type-name-to-actual-type-int-typeofint
		private void BuildFriendlyTypeDictionary()
		{
			if (friendlyTypeDictionary != null)
				friendlyTypeDictionary.Clear();
			else
				friendlyTypeDictionary = new Dictionary<Type, string>();

			var mscorlib = Assembly.GetAssembly(typeof(int));

			using (var provider = new Microsoft.CSharp.CSharpCodeProvider())
			{
				foreach (var type in mscorlib.DefinedTypes)
				{
					if (string.Equals(type.Namespace, "System"))
					{
						var typeRef = new System.CodeDom.CodeTypeReference(type);
						var csTypeName = provider.GetTypeOutput(typeRef);

						// Ignore qualified types.
						if (csTypeName.IndexOf('.') == -1)
						{
							friendlyTypeDictionary.Add(type, csTypeName);
						}
					}
				}
			}
		}
	}
}