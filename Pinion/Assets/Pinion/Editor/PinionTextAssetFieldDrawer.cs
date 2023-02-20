namespace Pinion.Editor
{
	using UnityEngine;
	using UnityEditor;
	using Pinion;
	using Pinion.Compiler;
	using System.Collections.Generic;
	using System;

	[CustomPropertyDrawer(typeof(DrawPinionTextAssetFieldAttribute))]
	public class PinionTextAssetFieldDrawer : PropertyDrawer
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

			if (property.propertyType != SerializedPropertyType.ObjectReference)
			{
				Rect messageRect = new Rect(position.x, yPos, position.width, lineHeight);
				EditorGUI.HelpBox(messageRect, "DrawPinionTextAssetField attribute can only be used on TextAsset fields.", MessageType.Error);
				yPos += lineHeightWithMargin;
				calculatedHeight = yPos - yPosOriginal;
				return;
			}

			EditorGUI.ObjectField(new Rect(position.x, yPos, position.width, lineHeight), property);
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
			DrawPinionTextAssetFieldAttribute textFieldAttribute = attribute as DrawPinionTextAssetFieldAttribute;
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

