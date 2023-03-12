namespace Pinion
{
	using System;
	using UnityEngine;

	[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public sealed class DrawPinionTextFieldAttribute : PropertyAttribute
	{
		public Type ContainerType
		{
			get { return containerType; }
		}

		private readonly Type containerType = null;

		public DrawPinionTextFieldAttribute() : this(typeof(PinionContainer))
		{
		}

		public DrawPinionTextFieldAttribute(Type containerType)
		{
			this.containerType = containerType;

			if (!(typeof(PinionContainer).IsAssignableFrom(containerType)))
			{
				throw new ArgumentException("Type specified in DrawPinionTextField attribute must be or inherit type PinionContainer.");
			}
		}
	}
}