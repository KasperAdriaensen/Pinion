using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class EditorGUIExtensions
{
	private const string serializedPropertyPrefix = "prop_";
	// NOTE: Make sure to include BindingFlags.DeclaredOnly so we only find fields declared explicitly.
	// We don't want to mess with internal Unity stuff higher in the inheritance chain.
	private const BindingFlags autoAssignPropertyFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

	public static void AssignAllSerializedProperties<T>(T editor) where T : UnityEditor.Editor
	{
		if (editor == null)
		{
			throw new NullReferenceException("Editor object was null. No serialized properties can be filled.");
		}

		if (editor.serializedObject == null)
		{
			throw new NullReferenceException("Editor object's serializedObject was null. No serialized properties can be filled.");
		}

		SerializedObject serializedObject = editor.serializedObject;
		AssignAllSerializedPropertiesInternal<T>(editor, serializedObject);
	}

	public static void AssignAllSerializedProperties<T>(T fillInTarget, SerializedObject fillInTargetSerialized) where T : UnityEditor.EditorWindow
	{
		if (fillInTarget == null)
		{
			throw new NullReferenceException("EditorWindow object was null. No serialized properties can be filled.");
		}

		if (fillInTargetSerialized == null)
		{
			throw new NullReferenceException("Passed SerializedObject was null. No serialized properties can be filled.");
		}

		AssignAllSerializedPropertiesInternal<T>(fillInTarget, fillInTargetSerialized);
	}

	private static void AssignAllSerializedPropertiesInternal<T>(T fillInTarget, SerializedObject fillInTargetSerialized)
	{
		System.Type editorType = typeof(T);
		System.Type serializedPropertyType = typeof(SerializedProperty);

		// select all serializedProperties
		FieldInfo[] allSerializedProperties = editorType.GetFields(autoAssignPropertyFlags).Where((FieldInfo f) => f.FieldType == serializedPropertyType).ToArray();

		foreach (FieldInfo fieldInfo in allSerializedProperties)
		{
			string fieldName = fieldInfo.Name;

			if (!fieldName.StartsWith(serializedPropertyPrefix))
			{
				Debug.LogWarningFormat("Serialized property {0} did not follow naming convention and will be ignored for automatic assignment.", fieldName);
				continue;
			}

			fieldName = fieldName.Remove(0, serializedPropertyPrefix.Length);
			SerializedProperty serializedProperty = fillInTargetSerialized.FindProperty(fieldName);

			if (serializedProperty == null)
			{
				Debug.LogWarningFormat("Could not find a serialized property named {0}. Ignoring this property for automatic assignment.", fieldName);
				continue;
			}

			fieldInfo.SetValue(fillInTarget, serializedProperty);
		}
	}
}
