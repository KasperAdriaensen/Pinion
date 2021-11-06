using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.

namespace Pinion
{
	[System.Flags]
	public enum APIMethodFlags
	{
		None = 0,
		Internal = 1
	}

	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class APISourceAttribute : System.Attribute
	{
	}

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class APIMethodAttribute : System.Attribute
	{
		//public bool Internal { get; set; }
		public APIMethodFlags MethodFlags { get; set; }

		public bool HasFlag(APIMethodFlags flag)
		{
			return (MethodFlags & flag) == flag;
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	sealed class APIInternalMethodIdentifierAttribute : System.Attribute
	{
		private readonly string identifier;

		public APIInternalMethodIdentifierAttribute(string identifier)
		{
			this.identifier = identifier;
		}

		public string Identifier
		{
			get { return identifier; }
		}
	}
}
