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
		public readonly IVariablePointer variablePointer;
		public readonly Token token;

		public CompilerArgument(Type argumentType, ArgSource argumentSource, Token token)
		{
			this.argumentType = argumentType;
			this.argumentSource = argumentSource;
			this.variablePointer = null;
			this.token = token;
		}

		public CompilerArgument(IVariablePointer variablePointer, Token token) : this(variablePointer.GetValueType(), ArgSource.Variable, token)
		{
			this.variablePointer = variablePointer;
		}

		public static CompilerArgument Invalid
		{
			get { return new CompilerArgument(null, ArgSource.Literal, Token.Invalid); }
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