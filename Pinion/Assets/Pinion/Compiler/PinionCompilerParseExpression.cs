using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Pinion.Compiler.Internal;

namespace Pinion.Compiler
{
	public partial class PinionCompiler
	{
		private static string TokenSplitRegex = null; // built by BuildTokenSplitRegex
		private const string ParenthesisOpen = "(";
		private const string ParenthesisClose = ")";
		private const string ArgSeparator = ",";

		private static Stack<string> operatorStack = new Stack<string>(64);     // typical operator stack as in shunting yard algoritm
		private static Stack<System.Type> argumentStack = new Stack<Type>(64);  // stack of types for compile type checking (essentially like a runtime stack, only with types instead of actual values)
		private static Stack<int> argumentCountStack = new Stack<int>(64);      // stack used to track how many arguments a function or operator should consume within the current () scope
		private static bool interpretNextSubtractAsNegate = false;              // stupid edge case => indicates whether the next "-" is the binary subtract operator or the unary "negate" operator 
		private static List<Type> reuseArgsList = new List<Type>();             // recyclable list of arguments passed to functions to verify signature matches

		// For this parsing logic, see: shunting yard algorithm (with quite a few modifications)
		// https://en.wikipedia.org/wiki/Shunting-yard_algorithm
		private static Type ParseExpression(PinionContainer container, string expression)
		{
			if (string.IsNullOrEmpty(expression))
			{
				AddCompileError("Empty expression.");
				return null;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing expression: {expression}.");
#endif

			string[] expressionTokens = Regex.Split(expression, TokenSplitRegex);

			List<ushort> output = container.scriptInstructions; // one way or another, we need to expose this, which is annoying... FIXME?

			operatorStack.Clear();
			argumentStack.Clear();
			argumentCountStack.Clear();
			reuseArgsList.Clear();
			interpretNextSubtractAsNegate = true;

			// Begin the first scope with 0 arguments.
			argumentCountStack.Push(0);

			for (int i = 0; i < expressionTokens.Length; i++)
			{
				if (!compileSuccess) // If previous token broke everything, we can stop here.
					break;

				string currentToken = expressionTokens[i];

				// Repeated same split character eg. "))" will insert an empty value in array (artifact of splitting logic). Can safely be ignored.
				if (string.IsNullOrEmpty(currentToken))
					continue;

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Parsing token: {currentToken}");
#endif
				if (currentToken == ArgSeparator) // a comma can be ignored
				{

					bool foundOpeningParenthesis = false;
					string operatorOnStack = null;

					// Keep popping the operator stack until we encounter the opening parenthesis.
					while (operatorStack.Count > 0 && !foundOpeningParenthesis)
					{
						operatorOnStack = operatorStack.Peek();

						if (operatorOnStack == ParenthesisOpen)
						{
							foundOpeningParenthesis = true;
						}
						else
						{
							operatorOnStack = operatorStack.Pop();
							if (!Returns(ParseOperatorOrInstruction(container, operatorOnStack, output)))
								break;
						}
					}

					if (!compileSuccess)
						break;

					if (!foundOpeningParenthesis)
					{
						AddCompileError("Invalid argument separator ','.'");
						break;
					}

					interpretNextSubtractAsNegate = true;
					continue;
				}

				if (PinionAPI.IsInstructionString(currentToken)) // token represents an instruction
				{
					operatorStack.Push(currentToken);
					interpretNextSubtractAsNegate = false;
					continue;
				}

				if (OperatorLookup.IsOperator(currentToken)) // token represents operator (will later be converted to function)
				{
					// See comment next to interpretNextSubtractAsNegate declaration.
					if (interpretNextSubtractAsNegate && currentToken == "-")
						currentToken = "n";

					OperatorInfo currentTokenOperator = OperatorLookup.GetOperatorInfo(currentToken);

					while (ShouldResolveTopOfStackFirst(currentTokenOperator))
					{
						if (!Returns(ParseOperatorOrInstruction(container, operatorStack.Pop(), output)))
							break;
					}

					operatorStack.Push(currentToken);
					interpretNextSubtractAsNegate = true;
					continue;
				}

				if (currentToken == ParenthesisOpen)
				{
					operatorStack.Push(currentToken);
					argumentCountStack.Push(0);
					interpretNextSubtractAsNegate = true;
					continue;
				}

				if (currentToken == ParenthesisClose)
				{
					bool foundMatchingParenthesis = false;
					string poppedOperator = null;

					// Keep popping the operator stack until we encounter the opening parenthesis.
					while (operatorStack.Count > 0 && !foundMatchingParenthesis)
					{
						poppedOperator = operatorStack.Pop();

						if (poppedOperator == ParenthesisOpen)
						{
							foundMatchingParenthesis = true;
						}
						else
						{
							if (!Returns(ParseOperatorOrInstruction(container, poppedOperator, output)))
								break;
						}
					}

					if (!compileSuccess)
						break;

					// No opening parenthesis found: error.
					if (!foundMatchingParenthesis)
					{
						AddCompileError("Mismatched parentheses.", true);
						break;
					}

					// Opening parenthesis found.
					// If there's something else left on the operator stack...
					if (operatorStack.Count > 0)
					{
						string topOfStack = operatorStack.Peek();

						if (PinionAPI.IsInstructionString(topOfStack)) // ... and that something represents an instruction...
						{
							if (!Returns(ParseOperatorOrInstruction(container, operatorStack.Pop(), output)))
								break;
						}
					}

					int resultingArgCount = argumentCountStack.Pop();
					AlterScopeArgCount(resultingArgCount);

					interpretNextSubtractAsNegate = false;
					continue;
				}

				if (Returns(ParseAtomicValue(container, currentToken, output)))
				{
					interpretNextSubtractAsNegate = false;
				}
				else
				{
					break;
				}
			}

			if (!compileSuccess)
				return null;

			while (operatorStack.Count > 0)
			{
				string operatorOnStack = operatorStack.Pop();

				// This catches the weird typo of having an unmatched OPENING parenthesis, e.g. If((Equals($a,$b))
				// Is this correct, though? It's kind of baseless.
				if (operatorOnStack == ParenthesisOpen)
				{
					AddCompileError("Mismatched parentheses.", true);
					break;
				}

				if (!Returns(ParseOperatorOrInstruction(container, operatorOnStack, output)))
					break;
			}

			// If the expression compiled correctly, it should at most resolve to zero or one return value.
			// If there's more left on the stack, something went wrong.
			if (argumentStack.Count > 1)
			{
				AddCompileError($"Expression returned invalid: {expression}.");
				return null;
			}

			// If it compiled correctly, either return the return value or void.
			if (argumentStack.Count == 1)
				return argumentStack.Pop();
			else
				return typeof(void);
		}

		private static Type ParseOperatorOrInstruction(PinionContainer container, string token, List<ushort> output)
		{
			int argCount = 0;

			if (OperatorLookup.IsOperator(token)) // if operator - convert this function to associated instruction string
			{
				OperatorInfo operatorInfo = OperatorLookup.GetOperatorInfo(token);
				token = operatorInfo.instructionString;
				argCount = operatorInfo.argumentCount; // i.e. 2 = binary operator, 1 = unary operator 

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Converted operator \'{token}\' to instruction string \'{operatorInfo.instructionString}\'.");
#endif
			}
			else
			{
				argCount = argumentCountStack.Peek(); // i.e. ALL arguments within this scope
			}

			ConsumeArguments(argCount);

			return ParseInstruction(container, token, output, reuseArgsList);
		}

		private static void ConsumeArguments(int count)
		{
			reuseArgsList.Clear();
			int consumedArgs = 0;
			for (int i = 0; i < count; i++)
			{
				// we don't throw errors here yet if there aren't enough arguments
				// if we simply pass too few arguments to the instruction parsing, we'll get more meaningful error messages
				if (argumentStack.Count <= 0)
					break;

				reuseArgsList.Add(argumentStack.Pop());
				consumedArgs++;
			}

			reuseArgsList.Reverse();

			AlterScopeArgCount(-consumedArgs);
		}

		private static void AlterScopeArgCount(int amount)
		{
			int argCountInCurrentScope = argumentCountStack.Pop();
			argCountInCurrentScope += amount;
			argumentCountStack.Push(argCountInCurrentScope);
		}

		private static bool Returns(Type returnedType)
		{
			if (returnedType == null) // something failed - we return false so we can stop compilation
				return false;

			// There was a return value - push it onto argumentStack to continue checking if everything resolves correctly.
			if (returnedType != typeof(void))
			{
				argumentStack.Push(returnedType);
				AlterScopeArgCount(1);
			}

			return true;
		}

		private static bool ShouldResolveTopOfStackFirst(OperatorInfo currentToken)
		{
			// NOTE: We're using the terminology PRECEDENCE, literally: "going before something else".
			// This means that the value should be interpreted "chronologically", i.e. the LOWEST value is the one that should be resolved FIRST.
			// This is different from thinking of it as "priority", which is more ambiguous with regard to "high priority": high number goes first or low number goes first?

			if (operatorStack.Count <= 0)
				return false;

			string topOfStackToken = operatorStack.Peek();

			if (topOfStackToken == ParenthesisOpen)
				return false;

			OperatorInfo topOfStackOperator = OperatorLookup.GetOperatorInfo(topOfStackToken);

			// I THOUGHT this was correct, but now it doesn't feel like it should be.
			// Keeping it around for reference. I think it mostly comes down to the same thing.
			// if (currentToken.associativity == OperatorAssociativity.Left && currentToken.precedence <= topOfStackPrecedence)
			// 	return true;

			// if (currentToken.associativity == OperatorAssociativity.Right && currentToken.precedence < topOfStackPrecedence)
			// 	return true;

			// Previous operator has precedence? Resolve that one first.
			if (topOfStackOperator.precedence < currentToken.precedence)
			{
				return true;
			}
			// Previous operator has same precedence as the current one? Resolve it first if the current one "needs" the result.
			else if (topOfStackOperator.precedence == currentToken.precedence)
			{
				if (currentToken.associativity == OperatorAssociativity.Left)
					return true;
			}

			return false;
		}

		private static void BuildTokenSplitRegex()
		{
			StringBuilder builder = new StringBuilder();

			// Surround regex with a capture group so the split string *also* includes the delimiters themselves.
			// This is necessary because parentheses affect parser state.
			builder.Append("(");

			// Ignore any splitting for text enclosed in double quotes.
			// This string is already correctly escaped (was necessary for C# anyway), so don't escape it below or it'll break the group constructs included.
			builder.Append(CompilerRegex.matchQuoteText);
			builder.Append("|");

			List<string> unEscapedSplitElements = new List<string>(32);
			unEscapedSplitElements.Add(ParenthesisOpen);
			unEscapedSplitElements.Add(ParenthesisClose);
			unEscapedSplitElements.Add(ArgSeparator);

			// Appends any non-"invisible" operator tokens.
			OperatorLookup.AppendOperators(unEscapedSplitElements);

			for (int i = 0; i < unEscapedSplitElements.Count; i++)
			{
				string element = unEscapedSplitElements[i];

				if (i > 0)
					builder.Append("|"); // OR

				builder.Append(Regex.Escape(element)); // Note, this only escapes characters that could be ambiguous - it won't escape e.g. %, which is meaningless in RegEx
			}

			builder.Append(")");

			TokenSplitRegex = builder.ToString(); // surround with a capture group so the split string *also* includes the delimiters themselves
		}

	}
}
