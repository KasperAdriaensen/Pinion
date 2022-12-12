using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public sealed class APIInternalMethodIdentifierAttribute : System.Attribute
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

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public sealed class APICustomCompileRequiredAttribute : System.Attribute
	{
		public enum HandlerTypes
		{
			None,
			BeforeInstruction,
			AfterInstruction,
			ReplaceInstruction
		}

		private readonly HandlerTypes handlerType = HandlerTypes.None;
		private readonly string identifier = string.Empty;

		public APICustomCompileRequiredAttribute(string identifier, HandlerTypes handlerType)
		{
			this.identifier = identifier;
			this.handlerType = handlerType;
		}

		public HandlerTypes HandlerType
		{
			get { return handlerType; }
		}

		public string Identifier
		{
			get { return identifier; }
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public sealed class APICustomCompileIdentifierAttribute : System.Attribute
	{
		public APICustomCompileIdentifierAttribute()
		{
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class APIResetAttribute : System.Attribute
	{
	}

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class APIInitAttribute : System.Attribute
	{
	}
}
