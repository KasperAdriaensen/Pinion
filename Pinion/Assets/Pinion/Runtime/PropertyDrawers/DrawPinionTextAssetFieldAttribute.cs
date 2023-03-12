namespace Pinion
{
	using System;
	using UnityEngine;

	[System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = false)]
	public sealed class DrawPinionTextAssetFieldAttribute : PropertyAttribute
	{
		public Type ContainerType
		{
			get { return containerType; }
		}

		private readonly Type containerType = null;

		public DrawPinionTextAssetFieldAttribute() : this(typeof(PinionContainer))
		{
		}

		public DrawPinionTextAssetFieldAttribute(Type containerType)
		{
			this.containerType = containerType;

			if (!(typeof(PinionContainer).IsAssignableFrom(containerType)))
			{
				throw new ArgumentException("Type specified in DrawPinionTextField attribute must be or inherit type PinionContainer.");
			}
		}
	}
}