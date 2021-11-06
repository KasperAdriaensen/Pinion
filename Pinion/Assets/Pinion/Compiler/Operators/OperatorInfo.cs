using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	// Used in parsing to correctly sort operator precedence etc.
	public class OperatorInfo
	{
		public readonly string instructionString;
		public readonly ushort precedence;
		public readonly OperatorAssociativity associativity;
		public readonly ushort argumentCount;
		// set to false to disallow splitting the input string on this operator (e.g. for "hidden" operators like the unary "-", which is internally replaced by "n".
		public bool affectsTokenization;

		public OperatorInfo(string instructionString, ushort precedence, OperatorAssociativity associativity, ushort argumentCount, bool affectsTokenization = true)
		{
			this.instructionString = instructionString;
			this.precedence = precedence;
			this.associativity = associativity;
			this.argumentCount = argumentCount;
			this.affectsTokenization = affectsTokenization;
		}
	}

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
}