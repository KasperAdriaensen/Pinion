using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	// https://en.wikipedia.org/wiki/Operator_associativity
	// The gist of it: if no parenthesis are present, what is the "conventional" grouping for two chained operators? 
	// Given a hypothetic ~ operator, what would be the expected reading of "a ~ b ~ c"?
	// Left associative:    (a ~ b) ~ c    e.g. 7-2-3 => (7-2)-3
	// Right associative:   a ~ (b ~ c)    e.g. 7^2^3 => 7^(2^3)
	public enum OperatorAssociativity
	{
		Left,
		Right
	}

	public interface IOperatorInfo
	{
		public string GetInstructionString(Token currentToken, IReadOnlyList<Token> expressionTokens, IReadOnlyList<CompilerArgument> args);
		public ushort GetPrecedence(Token currentToken, IReadOnlyList<Token> expressionTokens);
		public OperatorAssociativity GetAssociativity(Token currentToken, IReadOnlyList<Token> expressionTokens);
		public ushort GetArgumentCount(Token currentToken, IReadOnlyList<Token> expressionTokens);
	}

	// Used in parsing to correctly sort operator precedence etc.
	public struct OperatorInfo : IOperatorInfo
	{
		private ushort precedence;
		private OperatorAssociativity associativity;
		private ushort argumentCount;
		private string instructionString;

		public OperatorInfo(string instructionString, ushort precedence, OperatorAssociativity associativity, ushort argumentCount)
		{
			this.precedence = precedence;
			this.associativity = associativity;
			this.argumentCount = argumentCount;
			this.instructionString = instructionString;
		}

		public string GetInstructionString(Token currentToken, IReadOnlyList<Token> expressionTokens, IReadOnlyList<CompilerArgument> args)
		{
			return instructionString;
		}

		public ushort GetPrecedence(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return precedence;
		}

		public OperatorAssociativity GetAssociativity(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return associativity;
		}

		public ushort GetArgumentCount(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return argumentCount;
		}
	}

	public struct OperatorInfoIncrement : IOperatorInfo
	{
		private string prefixedInstruction;
		private string postfixedInstruction;

		public OperatorInfoIncrement(string prefixedInstruction, string postfixedInstruction)
		{
			this.prefixedInstruction = prefixedInstruction;
			this.postfixedInstruction = postfixedInstruction;
		}

		private bool IsPostfix(Token currentToken, IReadOnlyList<Token> expressionTokens, IReadOnlyList<CompilerArgument> args)
		{
			if (args.Count < 1)
				return true;

			// I.e. does ++ come *after* the argument it is operating upon?
			return currentToken.index > args[0].token.index;
		}

		public string GetInstructionString(Token currentToken, IReadOnlyList<Token> expressionTokens, IReadOnlyList<CompilerArgument> args)
		{
			return IsPostfix(currentToken, expressionTokens, args) ? postfixedInstruction : prefixedInstruction;
		}

		public ushort GetPrecedence(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return 2; // same for both versions of operator
		}

		public OperatorAssociativity GetAssociativity(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return OperatorAssociativity.Left;
		}

		public ushort GetArgumentCount(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return 2; // same for both versions of operator
		}
	}

	public struct OperatorInfoMinusSign : IOperatorInfo
	{
		private bool IsNegativeMarker(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			if (currentToken.index <= 0)
				return true;

			Token previousToken = expressionTokens[currentToken.index - 1];

			// conditions under which "-" should be read as negate instead of subtract
			if (previousToken == PinionCompiler.ArgSeparator ||
				previousToken == PinionCompiler.ParenthesisOpen ||
				OperatorLookup.IsOperator(previousToken))
				return true;

			return false;
		}

		public ushort GetArgumentCount(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			// Can't do implicit casting with a ternary operator.
			return (ushort)(IsNegativeMarker(currentToken, expressionTokens) ? 1 : 2);
		}

		public OperatorAssociativity GetAssociativity(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			return IsNegativeMarker(currentToken, expressionTokens) ? OperatorAssociativity.Right : OperatorAssociativity.Left;
		}

		public string GetInstructionString(Token currentToken, IReadOnlyList<Token> expressionTokens, IReadOnlyList<CompilerArgument> args)
		{
			return IsNegativeMarker(currentToken, expressionTokens) ? "Negate" : "Subtract";
		}

		public ushort GetPrecedence(Token currentToken, IReadOnlyList<Token> expressionTokens)
		{
			// Can't do implicit casting with a ternary operator.
			return (ushort)(IsNegativeMarker(currentToken, expressionTokens) ? 2 : 4);
		}
	}
}