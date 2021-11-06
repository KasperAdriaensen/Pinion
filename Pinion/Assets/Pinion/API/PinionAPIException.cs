using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PinionAPIException : System.Exception
{
	public PinionAPIException() { }
	public PinionAPIException(string message) : base(message) { }
	public PinionAPIException(string message, System.Exception inner) : base(message, inner) { }
	protected PinionAPIException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}