using System;

namespace Pinion.Compiler.Internal
{
	public struct CompilerArgument
	{
		public enum ArgSource
		{
			Literal,
			Variable,
			Complex
		}

		public readonly Type argumentType;
		public readonly ArgSource argumentSource;

		public CompilerArgument(Type argumentType, ArgSource argumentSource)
		{
			this.argumentType = argumentType;
			this.argumentSource = argumentSource;
		}

		public static CompilerArgument Invalid
		{
			get { return new CompilerArgument(null, ArgSource.Literal); }
		}

		public bool Valid
		{
			get { return argumentType != null; }
		}

		public bool IsArgumentTypeVoid
		{
			get { return argumentType == typeof(void); }
		}
	}
}