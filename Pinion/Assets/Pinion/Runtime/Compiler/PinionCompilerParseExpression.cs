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
		public const string ParenthesisOpen = "(";
		public const string ParenthesisClose = ")";
		public const string ArgSeparator = ",";

		private enum TokenType
		{
			None,
			ArgSeparator,
			ParenthesisOpen,
			ParenthesisClose,
			Instruction,
			Operator,
			Atomic,
		}

		private static Dictionary<TokenType, string> tokenTypeToReadable = new Dictionary<TokenType, string>
		{
			{TokenType.None, "none"},
			{TokenType.ArgSeparator, ArgSeparator},
			{TokenType.ParenthesisOpen, ParenthesisOpen},
			{TokenType.ParenthesisClose, ParenthesisClose},
			{TokenType.Instruction, "instruction"},
			{TokenType.Operator, "operator"},
			{TokenType.Atomic, "literal/variable"}
		};

		private static IReadOnlyList<Token> ExpressionToTokenList(string expression)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			string debugTokens = "Expression tokens: ";
#endif
			string[] splitExpression = Regex.Split(expression, TokenSplitRegex);
			// Length will be <= splitExpression.Length.
			// Some empty strings might get skipped but ballpark guess will be right.
			List<Token> tokens = new List<Token>(splitExpression.Length);
			int index = 0;

			for (int i = 0; i < splitExpression.Length; i++)
			{
				string expressionPart = splitExpression[i];

				// Repeated same split character eg. "))" will insert an empty value in array (artifact of splitting logic). Can safely be ignored.
				if (string.IsNullOrEmpty(expressionPart))
					continue;

				tokens.Add(new Token(expressionPart, index));
				index++;

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				debugTokens += expressionPart + "  ";
#endif
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log(debugTokens);
#endif

			return tokens;
		}

		// For this parsing logic, see: shunting yard algorithm (with quite a few modifications)
		// https://en.wikipedia.org/wiki/Shunting-yard_algorithm
		private static CompilerArgument ParseExpression(PinionContainer container, string expression)
		{
			if (string.IsNullOrEmpty(expression))
			{
				AddCompileError("Empty expression.");
				return CompilerArgument.Invalid;
			}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing expression: {expression}.");
#endif

			// Originally these were all static members that were reused.
			// BUT that didn't worked as soon as we did recursive parsing of expressions that do not share a stack 
			// e.g. Log(myArray[$variable + 1]), where "Log(...)" and "$variable + 1" are expression thats (currently) need to be resolve independently.
			// There's probably a (more efficient) way around this by treating indexers as a separate type of parenthesis that needs to be closed correctly.
			// But currently that's overcomplicating matters.

			Stack<Token> operatorStack = new Stack<Token>(64);    // typical operator stack as in shunting yard algoritm
			Stack<CompilerArgument> argsStack = new Stack<CompilerArgument>(64); // stack of types for compile type checking (essentially like a runtime stack, only with types instead of actual values)
			Stack<int> argsCountStack = new Stack<int>(64);     // stack used to track how many arguments a function or operator should consume within the current () scope
			List<CompilerArgument> argsBuffer = new List<CompilerArgument>();            // recyclable list of arguments passed to functions to verify signature matches
																						 //bool interpretNextSubtractAsNegate = false;             // stupid edge case => indicates whether the next "-" is the binary subtract operator or the unary "negate" operator 

			IReadOnlyList<Token> tokens = ExpressionToTokenList(expression);
			List<ushort> output = container.scriptInstructions; // one way or another, we need to expose this, which is annoying... FIXME?

			// Begin the first scope with 0 arguments.
			argsCountStack.Push(0);

			Token previousToken = Token.Invalid;
			Token currentToken = Token.Invalid;
			TokenType previousTokenType = TokenType.None;
			HashSet<TokenType> expectedAfterPreviousToken = new HashSet<TokenType>();

			for (int i = 0; i < tokens.Count; i++)
			{
				if (!compileSuccess) // If previous token broke everything, we can stop here.
					break;

				previousToken = currentToken;
				currentToken = tokens[i];

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Parsing token: {currentToken}");
#endif

				if (expectedAfterPreviousToken.Count > 0 && previousTokenType != TokenType.None && !expectedAfterPreviousToken.Contains(previousTokenType))
				{
					string expectedTokenString = string.Empty;
					int hashSetCounter = 0;
					foreach (TokenType expectedTokenItem in expectedAfterPreviousToken)
					{
						expectedTokenString += $"'{tokenTypeToReadable[expectedTokenItem]}'";

						if (hashSetCounter < expectedAfterPreviousToken.Count - 1)
							expectedTokenString += " or ";

						hashSetCounter++;
					}

					AddCompileError($"Unexpected token: {previousToken}. Expected {expectedTokenString}.");
				}

				expectedAfterPreviousToken.Clear();

				switch (previousTokenType)
				{
					case TokenType.ArgSeparator:
						expectedAfterPreviousToken.Add(TokenType.Atomic);
						expectedAfterPreviousToken.Add(TokenType.Instruction);
						expectedAfterPreviousToken.Add(TokenType.Operator);
						break;
					case TokenType.ParenthesisOpen:
						expectedAfterPreviousToken.Add(TokenType.Atomic);
						expectedAfterPreviousToken.Add(TokenType.Instruction);
						expectedAfterPreviousToken.Add(TokenType.Operator);
						expectedAfterPreviousToken.Add(TokenType.ParenthesisOpen);
						expectedAfterPreviousToken.Add(TokenType.ParenthesisClose);
						break;
					case TokenType.ParenthesisClose:
						expectedAfterPreviousToken.Add(TokenType.ArgSeparator);
						expectedAfterPreviousToken.Add(TokenType.Operator);
						expectedAfterPreviousToken.Add(TokenType.ParenthesisClose);
						break;
					case TokenType.Instruction:
						expectedAfterPreviousToken.Add(TokenType.ParenthesisOpen);
						break;
					case TokenType.Operator:
						expectedAfterPreviousToken.Add(TokenType.ArgSeparator);
						expectedAfterPreviousToken.Add(TokenType.Atomic);
						expectedAfterPreviousToken.Add(TokenType.Instruction);
						expectedAfterPreviousToken.Add(TokenType.Operator);
						expectedAfterPreviousToken.Add(TokenType.ParenthesisOpen);
						expectedAfterPreviousToken.Add(TokenType.ParenthesisClose);
						break;
					case TokenType.Atomic:
						expectedAfterPreviousToken.Add(TokenType.ArgSeparator);
						expectedAfterPreviousToken.Add(TokenType.Operator);
						expectedAfterPreviousToken.Add(TokenType.ParenthesisClose);
						break;
					default:
						break;
				}

				previousTokenType = TokenType.None;

				if (currentToken == ArgSeparator)
				{
					previousTokenType = TokenType.ArgSeparator;
					bool foundOpeningParenthesis = false;
					Token operatorOnStack = Token.Invalid;

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
							if (!Returns(ParseOperatorOrInstruction(container, operatorOnStack, output, argsCountStack, argsStack, argsBuffer, tokens), argsCountStack, argsStack))
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

					//interpretNextSubtractAsNegate = true;
					continue;
				}

				if (PinionAPI.IsInstructionString(currentToken)) // token represents an instruction
				{
					previousTokenType = TokenType.Instruction;
					operatorStack.Push(currentToken);
					//	interpretNextSubtractAsNegate = false;
					continue;
				}

				if (OperatorLookup.IsOperator(currentToken)) // token represents operator (will later be converted to function)
				{
					previousTokenType = TokenType.Operator;
					// See comment next to interpretNextSubtractAsNegate declaration.
					// if (interpretNextSubtractAsNegate && currentToken == "-")
					// {
					// 	//currentToken = "n";
					// 	currentToken = new Token("n", currentToken.index);
					// }

					while (ShouldResolveTopOfStackFirst(currentToken, operatorStack, tokens))
					{
						if (!Returns(ParseOperatorOrInstruction(container, operatorStack.Pop(), output, argsCountStack, argsStack, argsBuffer, tokens), argsCountStack, argsStack))
							break;
					}

					operatorStack.Push(currentToken);
					//	interpretNextSubtractAsNegate = true;
					continue;
				}

				if (currentToken == ParenthesisOpen)
				{
					previousTokenType = TokenType.ParenthesisOpen;
					operatorStack.Push(currentToken);
					argsCountStack.Push(0);
					//	interpretNextSubtractAsNegate = true;
					continue;
				}

				if (currentToken == ParenthesisClose)
				{
					previousTokenType = TokenType.ParenthesisClose;

					bool foundMatchingParenthesis = false;
					Token poppedOperator = Token.Invalid;

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
							if (!Returns(ParseOperatorOrInstruction(container, poppedOperator, output, argsCountStack, argsStack, argsBuffer, tokens), argsCountStack, argsStack))
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
						Token topOfStack = operatorStack.Peek();

						if (PinionAPI.IsInstructionString(topOfStack)) // ... and that something represents an instruction...
						{
							if (!Returns(ParseOperatorOrInstruction(container, operatorStack.Pop(), output, argsCountStack, argsStack, argsBuffer, tokens), argsCountStack, argsStack))
								break;
						}
					}

					int resultingArgCount = argsCountStack.Pop();
					AlterScopeArgCount(resultingArgCount, argsCountStack);

					//	interpretNextSubtractAsNegate = false;
					continue;
				}

				if (Returns(ParseAtomicValue(container, currentToken, output), argsCountStack, argsStack))
				{
					previousTokenType = TokenType.Atomic;
					//	interpretNextSubtractAsNegate = false;
				}
				else
				{
					break;
				}
			}

			if (!compileSuccess)
				return CompilerArgument.Invalid;

			while (operatorStack.Count > 0)
			{
				Token operatorOnStack = operatorStack.Pop();

				// This catches the weird typo of having an unmatched OPENING parenthesis, e.g. If((Equals($a,$b))
				// Is this correct, though? It's kind of baseless.
				if (operatorOnStack == ParenthesisOpen)
				{
					AddCompileError("Mismatched parentheses.", true);
					break;
				}

				if (!Returns(ParseOperatorOrInstruction(container, operatorOnStack, output, argsCountStack, argsStack, argsBuffer, tokens), argsCountStack, argsStack))
					break;
			}

			// If the expression compiled correctly, it should at most resolve to zero or one return value.
			// If there's more left on the stack, something went wrong.
			if (argsStack.Count > 1)
			{
				AddCompileError($"Expression could not parsed correctly: {expression}.");
				return CompilerArgument.Invalid;
			}

			// If it compiled correctly, either return the return value or void.
			if (argsStack.Count == 1)
				return argsStack.Pop();
			else
				return new CompilerArgument(typeof(void), CompilerArgument.ArgSource.Complex, Token.Invalid);
		}

		private static CompilerArgument ParseOperatorOrInstruction(
			PinionContainer container,
			Token token,
			List<ushort> output,
			Stack<int> argumentCountStack,
			Stack<CompilerArgument> argumentStack,
			List<CompilerArgument> storeArgsBuffer,
			IReadOnlyList<Token> expressionTokens
			)
		{
			int argCount = 0;

			if (OperatorLookup.IsOperator(token)) // if operator - convert this operator to its associated instruction string
			{
				IOperatorInfo operatorInfo = OperatorLookup.GetOperatorInfo(token);
				argCount = operatorInfo.GetArgumentCount(token, expressionTokens); // i.e. 2 = binary operator, 1 = unary operator 
				ConsumeArguments(argCount, argumentStack, argumentCountStack, storeArgsBuffer);
				// Most operators just return a fixed string, but some (e.g. increment) need to 
				// decide between different versions (e.g; prefix vs postfix increment) based on argument tokens.
				string matchedInstructionString = operatorInfo.GetInstructionString(token, expressionTokens, storeArgsBuffer);

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
				Debug.Log($"[PinionCompiler] Converted operator \'{token}\' to instruction string \'{matchedInstructionString}\'.");
#endif
				// "Replace" token down the line.
				token = new Token(matchedInstructionString, token.index);
			}
			else
			{
				argCount = argumentCountStack.Peek(); // i.e. ALL arguments within this scope
				ConsumeArguments(argCount, argumentStack, argumentCountStack, storeArgsBuffer);
			}

			return ParseInstruction(container, token, output, storeArgsBuffer);
		}

		private static void ConsumeArguments(int count, Stack<CompilerArgument> argumentStack, Stack<int> argumentCountStack, List<CompilerArgument> storeArgsBuffer)
		{
			storeArgsBuffer.Clear();
			int consumedArgs = 0;
			for (int i = 0; i < count; i++)
			{
				// we don't throw errors here yet if there aren't enough arguments
				// if we simply pass too few arguments to the instruction parsing, we'll get more meaningful error messages
				if (argumentStack.Count <= 0)
					break;

				storeArgsBuffer.Add(argumentStack.Pop());
				consumedArgs++;
			}

			storeArgsBuffer.Reverse();

			AlterScopeArgCount(-consumedArgs, argumentCountStack);
		}

		private static void AlterScopeArgCount(int amount, Stack<int> argumentCountStack)
		{
			int argCountInCurrentScope = argumentCountStack.Pop();
			argCountInCurrentScope += amount;
			argumentCountStack.Push(argCountInCurrentScope);
		}

		private static bool Returns(CompilerArgument returnValue, Stack<int> argumentCountStack, Stack<CompilerArgument> argumentStack)
		{
			if (!returnValue.Valid) // something failed - we return false so we can stop compilation
				return false;

			// There was a return value - push it onto argumentStack to continue checking if everything resolves correctly.
			if (!returnValue.IsArgumentTypeVoid)
			{
				argumentStack.Push(returnValue);
				AlterScopeArgCount(1, argumentCountStack);
			}

			return true;
		}

		private static bool ShouldResolveTopOfStackFirst(Token currentToken, Stack<Token> operatorStack, IReadOnlyList<Token> expressionTokens)
		{
			// NOTE: We're using the terminology PRECEDENCE, literally: "going before something else".
			// This means that the value should be interpreted "chronologically", i.e. the LOWEST value is the one that should be resolved FIRST.
			// This is different from thinking of it as "priority", which is more ambiguous with regard to "high priority": high number goes first or low number goes first?

			if (operatorStack.Count <= 0)
				return false;

			Token topOfStackToken = operatorStack.Peek();

			if (topOfStackToken.text == ParenthesisOpen)
				return false;

			IOperatorInfo currentOperator = OperatorLookup.GetOperatorInfo(currentToken);
			IOperatorInfo topOfStackOperator = OperatorLookup.GetOperatorInfo(topOfStackToken);

			// I THOUGHT this was correct, but now it doesn't feel like it should be.
			// Keeping it around for reference. I think it mostly comes down to the same thing.
			// if (currentToken.associativity == OperatorAssociativity.Left && currentToken.precedence <= topOfStackPrecedence)
			// 	return true;

			// if (currentToken.associativity == OperatorAssociativity.Right && currentToken.precedence < topOfStackPrecedence)
			// 	return true;

			ushort topOfStackPrecedence = topOfStackOperator.GetPrecedence(topOfStackToken, expressionTokens);
			ushort currentPrecedence = currentOperator.GetPrecedence(currentToken, expressionTokens);
			// Previous operator has precedence? Resolve that one first.
			if (topOfStackPrecedence < currentPrecedence)
			{
				return true;
			}
			// Previous operator has same precedence as the current one? Resolve it first if the current one "needs" the result.
			else if (topOfStackPrecedence == currentPrecedence)
			{
				if (currentOperator.GetAssociativity(currentToken, expressionTokens) == OperatorAssociativity.Left)
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
			builder.Append(CompilerRegex.splitPreserveTextInQuotes);
			builder.Append("|");
			builder.Append(CompilerRegex.splitPreserveVariableWithIndexer);
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
