using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	public static class OperatorLookup
	{
		// Going by this (C-style order of operations) because it's documented and somebody probably thought this through very well.
		//https://en.wikipedia.org/wiki/Order_of_operations#Programming_languages
		// NOTE: This collection is (among other purposes) used to generate a RegEx. 
		// This has a downside where operators that form a subset of a longer operator (e.g. < and <=) could incorrectly detect the shorter one, depending on their order in the regex.
		// To prevent this, put the "longer" one first.
		// This does not seem very robust, but the for the time being, it is acceptable.
		private static Dictionary<string, IOperatorInfo> operatorsToInstructions = new Dictionary<string, IOperatorInfo>
		{
			{"*", new OperatorInfo("Multiply", 3, OperatorAssociativity.Left, 2)},
			{"/", new OperatorInfo("Divide", 3, OperatorAssociativity.Left, 2)},
			{"%", new OperatorInfo("Modulo", 3, OperatorAssociativity.Left, 2)},

//			{"++", new OperatorInfo("IncrementPrefixed", 2, OperatorAssociativity.Left, 1)},
			{"++", new OperatorInfoIncrement(2, OperatorAssociativity.Left, 1)},

			{"+", new OperatorInfo("Add", 4, OperatorAssociativity.Left, 2)},
			{"-", new OperatorInfo("Subtract", 4, OperatorAssociativity.Left, 2)},
			{"n", new OperatorInfo("Negate", 2, OperatorAssociativity.Right, 1, false)}, // unary "-", to distinguish it from subtraction

			{"<=", new OperatorInfo("LessThanOrEqual", 6, OperatorAssociativity.Left, 2)},
			{"<", new OperatorInfo("LessThan", 6, OperatorAssociativity.Left, 2)},
			{">=", new OperatorInfo("GreaterThanOrEqual", 6, OperatorAssociativity.Left, 2)},
			{">", new OperatorInfo("GreaterThan", 6, OperatorAssociativity.Left, 2)},

			{"==", new OperatorInfo("Equals", 7, OperatorAssociativity.Left, 2)},
			{"!=", new OperatorInfo("NotEquals", 7, OperatorAssociativity.Left, 2)},

			{"!", new OperatorInfo("Not", 2, OperatorAssociativity.Right, 1)},

			{"&&", new OperatorInfo("And", 11, OperatorAssociativity.Left, 2)},
			{"||", new OperatorInfo("Or", 11, OperatorAssociativity.Left, 2)},
		};

		public static bool IsOperator(string token)
		{
			return operatorsToInstructions.ContainsKey(token);
		}

		public static IOperatorInfo GetOperatorInfo(string token)
		{
			return operatorsToInstructions[token];
		}

		public static void AppendOperators(IList<string> operatorTokens)
		{
			foreach (KeyValuePair<string, IOperatorInfo> operatorInfo in operatorsToInstructions)
			{
				// Some operators are only used internally, post-splitting.
				// If they occur in the source code, they should be ignored.
				if (operatorInfo.Value.AffectsTokenization)
					operatorTokens.Add(operatorInfo.Key);
			}
		}

		private static int GetOperatorPrecedence(string token)
		{
			if (operatorsToInstructions.ContainsKey(token))
				return operatorsToInstructions[token].Precedence;

			return int.MaxValue;
		}
	}
}
