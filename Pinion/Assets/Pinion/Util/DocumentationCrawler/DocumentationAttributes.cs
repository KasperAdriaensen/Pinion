using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Documentation
{
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public class DocSourceDisplayNameAttribute : System.Attribute
	{
		readonly string displayName;

		public DocSourceDisplayNameAttribute(string displayName)
		{
			this.displayName = displayName;
		}

		public string DisplayName
		{
			get { return displayName; }
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class DocMethodHideAttribute : System.Attribute
	{
		public DocMethodHideAttribute()
		{

		}
	}

	[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
	public class DocMethodOperatorReplaceAttribute : System.Attribute
	{
		readonly string operatorString;

		public DocMethodOperatorReplaceAttribute(string operatorString)
		{
			this.operatorString = operatorString;
		}

		public string OperatorString
		{
			get { return operatorString; }
		}
	}
}