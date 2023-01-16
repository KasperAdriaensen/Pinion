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
		public ushort Precedence { get; }
		public OperatorAssociativity Associativity { get; }
		public ushort ArgumentCount { get; }
		// set to false to disallow splitting the input string on this operator (e.g. for "hidden" operators like the unary "-", which is internally replaced by "n".
		public bool AffectsTokenization { get; }
		public string GetInstructionString(Token operatorToken, IReadOnlyList<CompilerArgument> args);
	}

	// Used in parsing to correctly sort operator precedence etc.
	public struct OperatorInfo : IOperatorInfo
	{
		public ushort Precedence { get; private set; }
		public OperatorAssociativity Associativity { get; private set; }
		public ushort ArgumentCount { get; private set; }
		public bool AffectsTokenization { get; private set; }
		private string instructionString;

		public OperatorInfo(string instructionString, ushort precedence, OperatorAssociativity associativity, ushort argumentCount, bool affectsTokenization = true)
		{
			this.Precedence = precedence;
			this.Associativity = associativity;
			this.ArgumentCount = argumentCount;
			this.AffectsTokenization = affectsTokenization;
			this.instructionString = instructionString;
		}

		public string GetInstructionString(Token operatorToken, IReadOnlyList<CompilerArgument> args)
		{
			return instructionString;
		}
	}

	public struct OperatorInfoIncrement : IOperatorInfo
	{
		public ushort Precedence { get; private set; }
		public OperatorAssociativity Associativity { get; private set; }
		public ushort ArgumentCount { get; private set; }
		public bool AffectsTokenization { get; private set; }

		public OperatorInfoIncrement(ushort precedence, OperatorAssociativity associativity, ushort argumentCount, bool affectsTokenization = true)
		{
			this.Precedence = precedence;
			this.Associativity = associativity;
			this.ArgumentCount = argumentCount;
			this.AffectsTokenization = affectsTokenization;
		}

		public string GetInstructionString(Token operatorToken, IReadOnlyList<CompilerArgument> args)
		{
			CompilerArgument arg = args[0];

			if (operatorToken.index > arg.token.index)
			{
				return "IncrementPostfixed";
			}
			else
			{
				return "IncrementPrefixed";
			}
		}
	}

}