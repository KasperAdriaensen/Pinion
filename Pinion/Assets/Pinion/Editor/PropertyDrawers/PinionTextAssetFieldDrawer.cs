namespace Pinion.Editor
{
	using UnityEngine;
	using UnityEditor;
	using Pinion;
	using Pinion.Compiler;
	using System.Collections.Generic;
	using System;

	[CustomPropertyDrawer(typeof(PinionTextAssetFieldAttribute))]
	public class PinionTextAssetFieldDrawer : PropertyDrawer
	{
		private const float lineMargin = 3f;
		private const float lineHeight = 22f;
		private const float lineHeightWithMargin = lineHeight + lineMargin;

		private List<string> errorMessages = new List<string>();
		private float calculatedHeight = (lineHeight * 2) + lineMargin;
		private bool compiledOnce = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			float yPosOriginal = position.y;
			float yPos = position.y;

			if (property.propertyType != SerializedPropertyType.ObjectReference)
			{
				Rect messageRect = new Rect(position.x, yPos, position.width, lineHeight);
				EditorGUI.HelpBox(messageRect, "DrawPinionTextAssetField attribute can only be used on TextAsset fields.", MessageType.Error);
				yPos += lineHeightWithMargin;
				calculatedHeight = yPos - yPosOriginal;
				return;
			}

			string propertyName = property.name;
			if (property.propertyPath.Contains("Array")) // Silly workaround, but detects correctly.
			{
				propertyName = "Array Item";
			}

			EditorGUI.ObjectField(new Rect(position.x, yPos, position.width, lineHeight), property, typeof(TextAsset), new GUIContent(propertyName));
			yPos += lineHeightWithMargin;

			bool cachedGUIEnabled = GUI.enabled;
			GUI.enabled = GUI.enabled && (property.objectReferenceValue != null);

			if (GUI.Button(new Rect(position.x, yPos, position.width, lineHeight), "Compile"))
			{
				TextAsset textAsset = property.objectReferenceValue as TextAsset;

				if (textAsset == null)
				{
					errorMessages.Add("Not a valid TextAsset.");
				}
				else
				{
					CompileString(textAsset.text);
				}
			}

			GUI.enabled = cachedGUIEnabled;

			yPos += lineHeightWithMargin;

			for (int i = 0; i < errorMessages.Count; i++)
			{
				string errorMessage = errorMessages[i];
				float height = PinionTextFieldDrawer.GetWarningHeight(errorMessage, position.width);
				Rect messageRect = new Rect(position.x, yPos, position.width, height);
				EditorGUI.HelpBox(messageRect, errorMessage, MessageType.Error);
				yPos += height + lineMargin;
			}

			if (compiledOnce && errorMessages.Count <= 0)
			{
				string message = "Compiled successfully.";
				float height = PinionTextFieldDrawer.GetWarningHeight(message, position.width);
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
			PinionTextAssetFieldAttribute textFieldAttribute = attribute as PinionTextAssetFieldAttribute;
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

