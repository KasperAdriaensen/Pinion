using System.Collections.Generic;

namespace Pinion.Compiler.Internal
{
	public static class PinionTypes
	{
		private static Dictionary<System.Type, string> typesToPinionName = new Dictionary<System.Type, string>
		{
			{typeof(float), "float"},
			{typeof(int), "int"},
			{typeof(bool), "bool"},
			{typeof(string), "string"},
		};

		private static Dictionary<string, System.Type> pinionNamesToType = new Dictionary<string, System.Type>
		{
			{"float", typeof(float)},
			{ "int", typeof(int)},
			{"bool", typeof(bool)},
			{"string", typeof(string)},
		};

		private static HashSet<System.Type> supportedTypes = new HashSet<System.Type>
		{
			typeof(int),
			typeof(float),
			typeof(bool),
			typeof(string),
		};

		public static string GetPinionNameFromType(System.Type type)
		{
			if (typesToPinionName.ContainsKey(type))
				return typesToPinionName[type];
			else
				return type.ToString();
		}

		public static System.Type GetTypeFromPinionName(string pinionName)
		{
			if (pinionNamesToType.ContainsKey(pinionName))
				return pinionNamesToType[pinionName];
			else
				return null;
		}

		public static bool IsSupportedPublicType(System.Type type)
		{
			return supportedTypes.Contains(type);
		}
	}
}