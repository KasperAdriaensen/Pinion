namespace Pinion.Editor
{
	using UnityEngine;
	using UnityEditor;
	using Pinion;
	using Pinion.Compiler;
	using System.Collections.Generic;
	using System;

	[CustomPropertyDrawer(typeof(PinionTextFieldAttribute))]
	public class PinionTextFieldDrawer : PropertyDrawer
	{
		private const float lineMargin = 3f;
		private const float lineHeight = 22f;
		private const float lineHeightWithMargin = lineHeight + lineMargin;
		private const float maxTextFieldHeight = 300;

		private List<string> errorMessages = new List<string>();
		private float calculatedHeight = (lineHeight * 2) + lineMargin;
		private bool compiledOnce = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			float yPosOriginal = position.y;
			float yPos = position.y;

			if (property.propertyType != SerializedPropertyType.String)
			{
				Rect messageRect = new Rect(position.x, yPos, position.width, lineHeight);
				EditorGUI.HelpBox(messageRect, "DrawPinionTextField attribute can only be used on string fields.", MessageType.Error);
				yPos += lineHeightWithMargin;
				calculatedHeight = yPos - yPosOriginal;
				return;
			}

			// property.isExpanded = EditorGUI.Foldout(new Rect(position.x, yPos, position.width, lineHeight), property.isExpanded, property.name);
			// yPos += lineHeightWithMargin;

			string propertyName = property.name;
			if (property.propertyPath.Contains("Array")) // Silly workaround, but detects correctly.
			{
				propertyName = "Array Item";
			}

			EditorGUI.LabelField(new Rect(position.x, yPos, position.width, lineHeight), propertyName, EditorStyles.boldLabel);
			yPos += lineHeightWithMargin;

			// only way to reliably get predicted size of a text area with text wrapping
			float calculatedTextFieldHeight = EditorStyles.textArea.CalcHeight(new GUIContent(property.stringValue), position.width);
			calculatedTextFieldHeight = Math.Min(maxTextFieldHeight, calculatedTextFieldHeight);
			//calculatedTextFieldHeight = maxTextFieldHeight;

			Rect textFieldRect = new Rect(position.x, yPos, position.width, calculatedTextFieldHeight);
			property.stringValue = EditorGUI.TextArea(textFieldRect, property.stringValue);
			yPos += calculatedTextFieldHeight + lineMargin;

			if (GUI.Button(new Rect(position.x, yPos, position.width, lineHeight), "Compile"))
			{
				CompileString(property.stringValue);
			}

			yPos += lineHeightWithMargin;

			for (int i = 0; i < errorMessages.Count; i++)
			{
				string errorMessage = errorMessages[i];
				float height = GetWarningHeight(errorMessage, position.width);
				Rect messageRect = new Rect(position.x, yPos, position.width, height);
				EditorGUI.HelpBox(messageRect, errorMessage, MessageType.Error);
				yPos += height + lineMargin;
			}

			if (compiledOnce && errorMessages.Count <= 0)
			{
				string message = "Compiled successfully.";
				float height = GetWarningHeight(message, position.width);
				Rect messageRect = new Rect(position.x, yPos, position.width, height);
				EditorGUI.HelpBox(messageRect, message, MessageType.Info);
				yPos += height + lineMargin;
			}

			calculatedHeight = yPos - yPosOriginal;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return calculatedHeight;
		}

		private void CompileString(string script)
		{
			PinionTextFieldAttribute textFieldAttribute = attribute as PinionTextFieldAttribute;
			Type targetType = textFieldAttribute.ContainerType;
			compiledOnce = true;
			errorMessages.Clear();
			PinionCompiler.CompileForEditor(targetType, script, HandleCompileError);
		}

		private void HandleCompileError(string errorMessage)
		{
			errorMessages.Add(errorMessage);
		}

		public static float GetWarningHeight(string text, float width)
		{
			return EditorStyles.helpBox.CalcHeight(new GUIContent(text), width);
		}
	}
}

