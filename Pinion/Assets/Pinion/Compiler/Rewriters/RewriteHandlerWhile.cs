using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pinion.Compiler.Internal
{
	public class RewriteHandlerWhile : IRewriteHandler
	{
		private int whileLoopUniqueIndex = 0;
		private Stack<int> whileLoopScope = new Stack<int>();
		private static Regex whileStartRegex = new Regex(@"^while\((.*)\)$"); // captures "While([anything])", with [anything] is capture group 1.
		private static Regex whileEndRegex = new Regex(@"^endwhile$");

		public bool AttemptRewrite(string inputLine, out string outputLine, int lineNumber)
		{
			outputLine = inputLine;

			Match match = whileStartRegex.Match(inputLine);

			if (match.Success)
			{
				// While(expression)
				// becomes:
				// label:@WhileStart0
				// ?=>@WhileEndx,expression
				// where x is incrementing index

				outputLine = $"{CompilerConstants.LabelCreatePrefixComplete}{GetWhileStartLabel(true)}\r\n{CompilerConstants.ConditionLabelJump}{CompilerConstants.LabelReadPrefix}{GetWhileEndLabel()},{match.Groups[1].Value}";

				return true;
			}

			match = whileEndRegex.Match(inputLine);

			if (match.Success)
			{

				// EndWhile
				// becomes:
				// =>@WhileStartx
				// label:@WhileEndx
				// where x is incrementing index

				outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetWhileStartLabel()}\r\n{CompilerConstants.LabelCreatePrefixComplete}{GetWhileEndLabel(true)}";

				return true;
			}

			return false;
		}

		public void Reset()
		{
			whileLoopUniqueIndex = 0;
			whileLoopScope.Clear();
		}

		private string GetWhileStartLabel(bool openScope = false)
		{
			if (openScope)
			{
				whileLoopScope.Push(whileLoopUniqueIndex);
				// NOTE/ This not equal to stack depth! It's a unique number that keeps incrementing so that each loop, regardless of nesting depth, gets a unique number.
				whileLoopUniqueIndex++;
				return $"WhileStart{whileLoopScope.Peek()}";
			}
			else
			{
				return $"WhileStart{whileLoopScope.Peek()}";
			}
		}

		private string GetWhileEndLabel(bool closeScope = false)
		{
			if (closeScope)
			{
				return $"WhileEnd{whileLoopScope.Pop()}";
			}
			else
			{
				return $"WhileEnd{whileLoopScope.Peek()}";
			}
		}

		public void CheckValidity(Action<string, int> errorMessageHandler)
		{
			if (errorMessageHandler == null)
				throw new ArgumentNullException(nameof(errorMessageHandler));

			if (whileLoopScope.Count > 0)
			{
				errorMessageHandler("Mismatched While-EndWhile detected.", -1);
			}
		}
	}

}