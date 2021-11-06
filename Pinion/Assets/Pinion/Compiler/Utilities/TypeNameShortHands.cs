using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	public static class TypeNameShortHands
	{
		private static Dictionary<System.Type, string> simpleTypeNames = new Dictionary<System.Type, string>
		{
			{typeof(float), "float"},
			{typeof(int), "int"},
			{typeof(bool), "bool"},
			{typeof(string), "string"},
		};

		public static string GetSimpleTypeName(System.Type type)
		{
			if (simpleTypeNames.ContainsKey(type))
				return simpleTypeNames[type];
			else
				return type.ToString();
		}
	}
}