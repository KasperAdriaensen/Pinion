namespace Pinion.Editor
{
	using UnityEngine;
	using UnityEditor;
	using Pinion;
	using Pinion.Compiler;
	using System.Collections.Generic;
	using System;

	[CustomPropertyDrawer(typeof(DrawPinionTextFieldAttribute))]
	public class PinionTextFieldDrawer : PropertyDrawer
	{
		private const float lineMargin = 3f;
		private const float lineHeight = 20f;
		private const float lineHeightWithMargin = lineHeight + lineMargin;

		private List<string> errorMessages = new List<string>();
		private float calculatedHeight = lineHeight;
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

			property.isExpanded = EditorGUI.Foldout(new Rect(position.x, yPos, position.width, lineHeight), property.isExpanded, property.name);
			yPos += lineHeightWithMargin;

			if (!property.isExpanded)
			{
				calculatedHeight = yPos - yPosOriginal;
				return;
			}

			Rect textFieldSize = new Rect(position.x, yPos, position.width, 200);
			property.stringValue = EditorGUI.TextArea(textFieldSize, property.stringValue);
			yPos += textFieldSize.height + lineMargin;

			if (GUI.Button(new Rect(position.x, yPos, position.width, lineHeight), "Compile"))
			{
				CompileString(property.stringValue);
			}

			yPos += lineHeightWithMargin;

			for (int i = 0; i < errorMessages.Count; i++)
			{
				string errorMessage = errorMessages[i];
				Rect messageRect = new Rect(position.x, yPos, position.width, lineHeight);
				EditorGUI.HelpBox(messageRect, errorMessage, MessageType.Error);
				yPos += lineHeightWithMargin;
			}

			if (compiledOnce && errorMessages.Count <= 0)
			{
				Rect messageRect = new Rect(position.x, yPos, position.width, lineHeight);
				EditorGUI.HelpBox(messageRect, "Compiled successfully.", MessageType.Info);
				yPos += lineHeightWithMargin;
			}

			calculatedHeight = yPos - yPosOriginal;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return calculatedHeight;
		}

		private void CompileString(string script)
		{
			DrawPinionTextFieldAttribute textFieldAttribute = attribute as DrawPinionTextFieldAttribute;
			Type targetType = textFieldAttribute.ContainerType;
			compiledOnce = true;
			errorMessages.Clear();
			PinionCompiler.CompileForEditor(targetType, script, HandleCompileError);
		}

		private void HandleCompileError(string errorMessage)
		{
			errorMessages.Add(errorMessage);
		}

	}
}

