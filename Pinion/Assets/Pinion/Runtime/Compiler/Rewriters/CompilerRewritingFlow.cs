namespace Pinion.Compiler.Internal
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	public static partial class CompilerRewriting
	{
		private enum LoopType
		{
			None,
			While,
			For
		}

		// General
		private static Stack<LoopType> loopStack = new Stack<LoopType>();

		// while
		private static int whileLoopUniqueIndex = 0;
		private static Stack<int> whileLoopScope = new Stack<int>();
		private static Regex whileStartRegex = new Regex(@"^while\((.*)\)$"); // captures "While([anything])", with [anything] is capture group 1.
		private static Regex whileEndRegex = new Regex(@"^endwhile$");

		// if
		private const string branchLabelBase = "IFJUMP";
		// Compiled flag leads to longer initialization time but faster runtime performance, especially when callecd frequently.
		private static Regex ifRegex = new Regex(@"^if\((.*)\)$", RegexOptions.Compiled); // captures "If([anything])", with [anything] is capture group 1.
		private static Regex endIfRegex = new Regex(@"^endif$", RegexOptions.Compiled);
		private static Regex elseRegex = new Regex(@"^else$", RegexOptions.Compiled); // the brackets captured only for error display
		private static Regex elseIfRegex = new Regex(@"^elseif\((.*)\)$", RegexOptions.Compiled); // NOTE: Whitespace has already been cleared at this point, so not "Else If". See below why this is checked seperately from IF and ELSE.
		private static List<(string, int)> errorsEncountered = new List<(string, int)>(10);
		private static int branchUniqueIndex = 0;
		private static Stack<BranchInfo> branchStack = new Stack<BranchInfo>();

		// for
		private static int forLoopUniqueIndex = 0;
		private static Stack<(int, string)> forLoopScope = new Stack<(int, string)>();
		private static Regex forStartRegex = new Regex(@"^for\((.*)\)$"); // captures "While([anything])", with [anything] is capture group 1.
		private static Regex forEndRegex = new Regex(@"^endfor$");

		// break/continue
		private static Regex breakRegex = new Regex(@"^break$");        // line that's just "break"
		private static Regex continueRegex = new Regex(@"^continue$");  // line that's just "continue"

		private struct BranchInfo
		{
			public int index;
			public ConditionalKeyword keyword;
			public int endIfDebt;

			public BranchInfo(int index, ConditionalKeyword keyword, int endIfDebt)
			{
				this.index = index;
				this.keyword = keyword;
				this.endIfDebt = endIfDebt;
			}
		}

		// Lower case and @ to allow direct ToString without conflicting with c# keyword
		private enum ConditionalKeyword
		{
			@if,
			@endif,
			@else,
			@elseif
		}

		// NOTE: This logic used to abstracted into seperate "IRewriteHandlers" for if, while, etc.
		// However, this severely complicated architecture once things like the break command were introduced, because the various rewrite handlers
		// couldn't access each other's internal state, e.g. to check the current innermost loop type.
		// In hindsight, separate IRewriteHandlers didn't really accomplish much: both the amount of string compares and memory used are gonna be about the same.
		// So, while it may seem less "clean", merging these rewrites in a single, more complex method actually made sense.
		// Besides, the amount of flow control techniques is pretty limited and hasn't changed much in decaces, so all in all, there's
		// not much to be gained from a "flexible" list of rewrite handlers.
		public static string RewriteFlow(string input, System.Action<string, int> errorMessageHandler)
		{
			if (errorMessageHandler == null)
				throw new ArgumentNullException(nameof(errorMessageHandler));

			Reset();

			StringBuilder resultBuilder = new StringBuilder(input.Length * 2);
			int lineNumber = 0;

			using (StringReader reader = new StringReader(input))
			{
				string line = string.Empty;

				do
				{
					line = reader.ReadLine();

					if (string.IsNullOrEmpty(line)) // Ignore blank lines or end of file.
						continue;

					// Mimicked from main Pinion compiler. Also handy to be able to reference source line number there.
					// For more info, check the main compiler logic.
					Match lineNumberMatch = Regex.Match(line, CompilerRegex.lineNumberRegex);
					if (lineNumberMatch.Success)
					{
						lineNumber = int.Parse(lineNumberMatch.Groups[1].Value); // update "line number"

						// Leave this line unaltered otherwise.
						resultBuilder.AppendLine(line);
						continue;
					}

					bool rewritten = false;

					// Methods in this loop return true if a rewrite happened.
					// We can stop checking for other potential rewrites if rewritten is true, or an error was encountered.

					// TODO:
					// This is based on the assumption that one line won't contain e.g. both a while AND an if.
					// That seems like a dangerous assumption, but the current logic for adding in the go-to labels
					// will break anyway in that scenario.
					// A knotty problem, which we've opted to accept for now.

					// No check for the first one, obviously.
					rewritten = RewriteIf(line, out line, lineNumber);

					if (ContinueCheckRewrite())
						rewritten = RewriteFor(line, out line, lineNumber);

					if (ContinueCheckRewrite())
						rewritten = RewriteBreak(line, out line, lineNumber);

					if (ContinueCheckRewrite())
						rewritten = RewriteWhile(line, out line, lineNumber);

					resultBuilder.AppendLine(line);

					// Nested for convenience.
					bool ContinueCheckRewrite()
					{
						return !rewritten && (errorsEncountered.Count <= 0);
					}
				}
				while (line != null);
			}

			//  CHECK VALIDITY

			// Don't bother showing these if some other error broke further parsing.
			// In that case, these branches will by definition be unbalanced, because we never got to the closing keyword.
			// That would be a confusing error.

			// if
			if (errorsEncountered.Count <= 0 && branchStack.Count > 0)
				errorsEncountered.Add(("Mismatched if-endif detected.", -1));

			// for
			if (errorsEncountered.Count <= 0 && forLoopScope.Count > 0)
				errorsEncountered.Add(("Mismatched for-endfor detected.", -1));

			// while
			if (errorsEncountered.Count <= 0 && whileLoopScope.Count > 0)
				errorsEncountered.Add(("Mismatched while-endwhile detected.", -1));

			for (int i = 0; i < errorsEncountered.Count; i++)
			{
				errorMessageHandler(errorsEncountered[i].Item1, errorsEncountered[i].Item2);
			}

			Reset();

			return resultBuilder.ToString();

			void Reset()
			{
				// reset if logic
				branchUniqueIndex = 0;
				branchStack.Clear();
				errorsEncountered.Clear();

				// reset for logic
				forLoopUniqueIndex = 0;
				forLoopScope.Clear();

				// reset while logic
				whileLoopUniqueIndex = 0;
				whileLoopScope.Clear();

				loopStack.Clear();
			}
		}


		#region WHILE
		public static bool RewriteWhile(string inputLine, out string outputLine, int lineNumber)
		{
			outputLine = inputLine;

			Match match = whileStartRegex.Match(inputLine);

			if (match.Success)
			{
				// while(expression)
				// becomes:
				// label:@WhileStart0
				// ?=>@WhileEndx,expression
				// where x is incrementing index

				outputLine = $"{CompilerConstants.LabelCreatePrefixComplete}{GetWhileStartLabel(lineNumber, true)}\r\n{CompilerConstants.ConditionLabelJump}{CompilerConstants.LabelReadPrefix}{GetWhileEndLabel(lineNumber)},{match.Groups[1].Value}";
				return true;
			}

			if (errorsEncountered.Count > 0)
				return false;

			match = whileEndRegex.Match(inputLine);

			if (match.Success)
			{
				// E.g. endwhile before a while was encountered.
				// Better check this here as well. Otherwise we get two identical errors because the
				// scope is checked twice within the same generated string below. Harmless, but confusing.
				if (whileLoopScope.Count <= 0)
				{
					errorsEncountered.Add(("Not currently in a while loop.", lineNumber));
					return false;
				}

				// endwhile
				// becomes:
				// =>@WhileStartx
				// label:@WhileEndx
				// where x is incrementing index

				outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetWhileStartLabel(lineNumber)}\r\n{CompilerConstants.LabelCreatePrefixComplete}{GetWhileEndLabel(lineNumber, true)}";
				return true;
			}

			return false;
		}

		private static string GetWhileStartLabel(int lineNumber, bool openScope = false)
		{
			if (openScope)
			{
				whileLoopScope.Push(whileLoopUniqueIndex);
				// NOTE: This not equal to stack depth! It's a unique number that keeps incrementing so that each loop, regardless of nesting depth, gets a unique number.
				whileLoopUniqueIndex++;
				loopStack.Push(LoopType.While); // Used for tracking the current "innermost" loop across different loop types.

			}

			if (whileLoopScope.Count <= 0)
			{
				errorsEncountered.Add(("Not currently in a while loop.", lineNumber));
				return string.Empty;
			}

			return $"WhileStart{whileLoopScope.Peek()}";
		}

		private static string GetWhileEndLabel(int lineNumber, bool closeScope = false)
		{
			if (whileLoopScope.Count <= 0)
			{
				errorsEncountered.Add(("Not currently in a while loop.", lineNumber));
				return string.Empty;
			}

			if (closeScope)
				loopStack.Pop(); // Used for tracking the current "innermost" loop across different loop types.

			return closeScope ? $"WhileEnd{whileLoopScope.Pop()}" : $"WhileEnd{whileLoopScope.Peek()}";
		}
		#endregion

		#region IF
		public static bool RewriteIf(string inputLine, out string outputLine, int lineNumber)
		{
			outputLine = inputLine;

			// CHECK IF ==================================
			Match match = ifRegex.Match(inputLine);

			if (match.Success)
			{
				// if(expression)
				// becomes:
				// ?=>@IfEndx,expression
				// where x is incrementing index

				string condition = match.Groups[1].Value;

				if (string.IsNullOrEmpty(condition))
					errorsEncountered.Add(($"Empty if-statement.'", lineNumber));

				outputLine = BuildIf(match.Groups[1].Value, ConditionalKeyword.@if);
				return true;
			}

			if (errorsEncountered.Count > 0)
				return false;

			// CHECK ENDIF ==================================
			// Tiny optimization? Checked first, instead of in the regular IF-ELSE IF-ELSE-ENDIF order because -logically- ENDIF is gonna be more frequent than ELSE or ELSE IF, being non-optional.
			match = endIfRegex.Match(inputLine);

			if (match.Success)
			{
				// endif
				// becomes:
				// label:@IfEndx
				// where x is incrementing index

				outputLine = BuildEndIf(lineNumber);
				return true;
			}

			if (errorsEncountered.Count > 0)
				return false;

			// CHECK ELSE IF ==================================
			// NOTE: ELSE IF is functionally identical to keyword ELSE followed by keyword IF. 
			// The only reason it's treated separately is because we only check "one item" at the beginning of the line and fixing that would be a much more complicated refactor.
			match = elseIfRegex.Match(inputLine);

			if (match.Success)
			{
				// elseif
				// becomes:
				// =>@IfEnd(x+1)
				// label:@IfEndx
				// ?=>@IfEnd(x+1),expression
				// where x is incrementing index

				string condition = match.Groups[1].Value;

				if (string.IsNullOrEmpty(condition))
					errorsEncountered.Add(($"Empty elseif statement.'", lineNumber));

				string elseLogic = BuildElse(ConditionalKeyword.elseif, lineNumber);
				// Second argument: Reuse the same branch index as was generated for the ELSE above. Essentially this means that the IF is not truly "nested" in the ELSE.
				// The alternative would mean we have to close two branches when encountering the single ENDIF further on: one explicitly for the enclosing ELSE and one implicitly for the nested IF.
				// That was harder, so we go with this solution.
				string ifLogic = BuildIf(condition, ConditionalKeyword.@if, true);
				outputLine = elseLogic + System.Environment.NewLine + ifLogic;
				return true;
			}

			if (errorsEncountered.Count > 0)
				return false;

			match = elseRegex.Match(inputLine);

			if (match.Success)
			{
				// else
				// becomes:
				// =>@IfEnd(x+1)
				// label:@IfEndx
				// where x is incrementing index

				outputLine = BuildElse(ConditionalKeyword.@else, lineNumber);
				return true;
			}

			return false;
		}

		private static string BuildIf(string condition, ConditionalKeyword keyword, bool incursEndIfDebt = false)
		{
			string label = null;
			int currentDebt = 0;

			if (branchStack.Count > 0)
			{
				currentDebt = GetCurrentBranch().endIfDebt;
				if (incursEndIfDebt)
					currentDebt++;
			}

			label = $"{branchLabelBase}{OpenBranch(keyword, currentDebt).index}";
			return $"{CompilerConstants.ConditionLabelJump}{CompilerConstants.LabelReadPrefix}{label},{condition}";
		}

		private static string BuildElse(ConditionalKeyword keyword, int lineNumber)
		{
			string result = string.Empty;
			BranchInfo previousBranch = CloseBranch(lineNumber);
			ConditionalKeyword previousKeyword = previousBranch.keyword;
			int endIfDepth = previousBranch.endIfDebt;

			if (previousKeyword != ConditionalKeyword.@if && previousKeyword != ConditionalKeyword.elseif)
			{
				errorsEncountered.Add(($"The conditional keyword {keyword} must be preceded by {ConditionalKeyword.@if} or {ConditionalKeyword.elseif}.", lineNumber));
			}

			string closeBranchLabel = $"{branchLabelBase}{previousBranch.index}";
			string openBranchLabel = $"{branchLabelBase}{OpenBranch(keyword, endIfDepth).index}";

			return $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{openBranchLabel}\r\n{CompilerConstants.LabelCreatePrefixComplete}{closeBranchLabel}";
		}

		private static string BuildEndIf(int lineNumber)
		{
			string result = string.Empty;

			// This can happen in degenerate cases like when the closing quotation mark is forgotten in something like if '(Add("a string", "another string)'
			// The whitespace removal logic (earlier in the compiler) preserves all spaces insides quotations (i.e. strings) but this DOES assume that all quotation marks are properly balanced.
			// Removing this assumption makes it much harder to properly strip the whitespace.
			// If the whitespace isn't removed from something like 'if (Add("a string", "another string)', the regex to match if-statements also doesn't match since it ASSUMES all whitespace is already gone.
			// We might add an optional whitespace in the regex, but then it's the only one that DOES allow for that across the board. It might also create other unexpected edge cases.
			// Since stuff won't compile anyway in the "unbalanced quotation mark scenario", we'll just make sure we don't try to pop from an empty branch stack once we encounter the endif.
			// And give a maximally informative error while we're at it.
			if (branchStack.Count == 0)
			{
				errorsEncountered.Add(($"Conditional keyword {ConditionalKeyword.endif} encountered without a matching {ConditionalKeyword.@if}, {ConditionalKeyword.@else}, or {ConditionalKeyword.elseif}. Is there a malformed conditional statement further up?", lineNumber));
				return result;
			}

			while (branchStack.Count > 0)
			{
				BranchInfo previousBranch = CloseBranch(lineNumber);
				ConditionalKeyword previousKeyword = previousBranch.keyword;

				if (previousKeyword == ConditionalKeyword.endif)
				{
					errorsEncountered.Add(($"The conditional keyword {ConditionalKeyword.endif} must be preceded by {ConditionalKeyword.@if}, {ConditionalKeyword.@else}, or {ConditionalKeyword.elseif}.", lineNumber));
				}

				string closeBranchLabel = $"{branchLabelBase}{previousBranch.index}";
				result += $"{CompilerConstants.LabelCreatePrefixComplete}{closeBranchLabel}";

				if (previousBranch.endIfDebt > 0)
				{
					result += System.Environment.NewLine;
				}
				else
				{
					break;
				}
			}

			return result;
		}

		private static BranchInfo OpenBranch(ConditionalKeyword keyword, int endIfDebt = 0)
		{
			branchStack.Push(new BranchInfo(branchUniqueIndex, keyword, endIfDebt));
			branchUniqueIndex++;
			return branchStack.Peek();
		}

		private static BranchInfo CloseBranch(int lineNumber)
		{
			return branchStack.Pop();
		}

		private static BranchInfo GetCurrentBranch()
		{
			return branchStack.Peek();
		}

		#endregion

		#region BREAK
		public static bool RewriteBreak(string inputLine, out string outputLine, int lineNumber)
		{
			outputLine = inputLine;
			Match match = breakRegex.Match(inputLine);

			if (match.Success)
			{
				if (loopStack.Count <= 0)
				{
					errorsEncountered.Add(($"The 'break' keyword can only be used inside a loop.", lineNumber));
					return false;
				}

				switch (loopStack.Peek())
				{
					case LoopType.While:
						// break
						// becomes:
						// =>@WhileEndx
						outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetWhileEndLabel(lineNumber, false)}";
						return true;

					case LoopType.For:
						// break
						// becomes:
						// =>@ForEndx
						outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetForEndLabel(lineNumber, false)}";
						return true;

					default:
						errorsEncountered.Add(($"The 'break' keyword is not valid in a loop of type: '{loopStack.Peek()}'.", lineNumber));
						break;

				}

				return false;
			}

			if (errorsEncountered.Count > 0)
				return false;

			match = continueRegex.Match(inputLine);

			if (match.Success)
			{
				if (loopStack.Count <= 0)
				{
					errorsEncountered.Add(($"The 'continue' keyword can only be used inside a loop.", lineNumber));
					return false;
				}

				switch (loopStack.Peek())
				{
					case LoopType.While:
						// continue
						// becomes:
						// =>@WhileStartx
						outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetWhileStartLabel(lineNumber, false)}";
						return true;

					case LoopType.For:
						// continue
						// becomes:
						// (update expression)
						// =>@ForStartx

						// Don't forge to append update instruction, or the continue keyword just turns a for-loop into an endless one...

						outputLine = $"{GetForLoopUpdateExpression(lineNumber)}\r\n{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetForStartLabel(lineNumber, false)}";
						return true;

					default:
						errorsEncountered.Add(($"The 'break' keyword is not valid in a loop of type: '{loopStack.Peek()}'.", lineNumber));
						break;

				}

				return false;
			}
			return false;
		}
		#endregion

		#region FOR
		public static bool RewriteFor(string inputLine, out string outputLine, int lineNumber)
		{
			outputLine = inputLine;

			Match match = forStartRegex.Match(inputLine);

			if (match.Success)
			{
				// for(ExpressionA;ExpressionB;ExpressionC)
				// becomes:
				// ExpressionA
				// label:@ForStart0
				// ?=>@ForEndx,ExpressionB
				// where x is incrementing index

				// match.Groups[1] is everything between the ()
				// Split this into (what should be) exactly three parts.
				string[] parts = match.Groups[1].Value.Split(';');

				if (parts.Length != 3)
				{
					errorsEncountered.Add(($"For loop must have three elements, separated by ';'. First element can be empty.", lineNumber));
					return false;
				}

				string declaration = parts[0];
				string test = parts[1];
				string updateExpression = parts[2];

				outputLine = $"{declaration}\r\n{CompilerConstants.LabelCreatePrefixComplete}{GetForStartLabel(lineNumber, true, updateExpression)}\r\n{CompilerConstants.ConditionLabelJump}{CompilerConstants.LabelReadPrefix}{GetForEndLabel(lineNumber)},{test}";
				return true;
			}

			if (errorsEncountered.Count > 0)
				return false;

			match = forEndRegex.Match(inputLine);

			if (match.Success)
			{
				// endfor
				// becomes:
				// ExpressionC (see above)
				// =>@ForStartx
				// label:@ForEndx
				// where x is incrementing index

				outputLine = $"{GetForLoopUpdateExpression(lineNumber)}\r\n{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{GetForStartLabel(lineNumber)}\r\n{CompilerConstants.LabelCreatePrefixComplete}{GetForEndLabel(lineNumber, true)}";
				return true;
			}

			return false;
		}

		private static string GetForStartLabel(int lineNumber, bool openScope = false, string updateExpression = null)
		{
			if (openScope)
			{
				forLoopScope.Push((forLoopUniqueIndex, updateExpression));
				// NOTE: This not equal to stack depth! It's a unique number that keeps incrementing so that each loop, regardless of nesting depth, gets a unique number.
				forLoopUniqueIndex++;
				loopStack.Push(LoopType.For); // Used for tracking the current "innermost" loop across different loop types.
			}

			if (forLoopScope.Count <= 0)
			{
				errorsEncountered.Add(("Not currently in a for loop.", lineNumber));
				return string.Empty;
			}

			return $"ForStart{forLoopScope.Peek().Item1}";
		}

		private static string GetForEndLabel(int lineNumber, bool closeScope = false)
		{
			if (forLoopScope.Count <= 0)
			{
				errorsEncountered.Add(("Not currently in a for loop.", lineNumber));
				return string.Empty;
			}

			if (closeScope)
				loopStack.Pop(); // Used for tracking the current "innermost" loop across different loop types.

			return closeScope ? $"ForEnd{forLoopScope.Pop().Item1}" : $"ForEnd{forLoopScope.Peek().Item1}";
		}

		private static string GetForLoopUpdateExpression(int lineNumber)
		{
			if (forLoopScope.Count <= 0)
			{
				errorsEncountered.Add(("Not currently in a for loop.", lineNumber));
				return string.Empty;
			}

			return forLoopScope.Peek().Item2;
		}

		#endregion
	}
}