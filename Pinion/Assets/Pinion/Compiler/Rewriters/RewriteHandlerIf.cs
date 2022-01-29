using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pinion.Compiler.Internal;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	public class RewriteHandlerIf : IRewriteHandler
	{
		private const string branchLabelBase = "IFJUMP";
		// Compiled flag leads to longer initialization time but faster runtime performance, especially when callecd frequently.
		private static Regex ifRegex = new Regex(@"^if\((.*)\)$", RegexOptions.Compiled); // captures "If([anything])", with [anything] is capture group 1.
		private static Regex endIfRegex = new Regex(@"^endif$", RegexOptions.Compiled);
		private static Regex elseRegex = new Regex(@"^else$", RegexOptions.Compiled); // the brackets captured only for error display
		private static Regex elseIfRegex = new Regex(@"^elseif\((.*)\)$", RegexOptions.Compiled); // NOTE: Whitespace has already been cleared at this point, so not "Else If". See below why this is checked seperately from IF and ELSE.
		private static List<(string, int)> errorsEncountered = new List<(string, int)>(10);

		private int branchUniqueIndex = 0;
		private Stack<BranchInfo> branchStack = new Stack<BranchInfo>();


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

		public bool AttemptRewrite(string inputLine, out string outputLine, int lineNumber)
		{
			outputLine = inputLine;

			// CHECK IF ==================================
			Match match = ifRegex.Match(inputLine);

			if (match.Success)
			{
				// If(expression)
				// becomes:
				// ?=>@IfEndx,expression
				// where x is incrementing index

				string condition = match.Groups[1].Value;

				if (string.IsNullOrEmpty(condition))
					errorsEncountered.Add(($"Empty If statement.'", lineNumber));

				//outputLine = $"{CompilerConstants.ConditionLabelJump}{CompilerConstants.LabelReadPrefix}{GetBranchLabel(ConditionalKeyword.If, true)},{match.Groups[1].Value}";
				outputLine = BuildIf(match.Groups[1].Value, ConditionalKeyword.@if);

				return true;
			}

			// CHECK ENDIF ==================================
			// Tiny optimization? Checked first, instead of in the regular IF-ELSE IF-ELSE-ENDIF order because, logically, ENDIF is gonna be more frequent than ELSE or ELSE IF, being non-optional.
			match = endIfRegex.Match(inputLine);

			if (match.Success)
			{
				// EndIf
				// becomes:
				// label:@IfEndx
				// where x is incrementing index

				// string label = GetBranchLabel(ConditionalKeyword.EndIf, false); // returns string.Empty if an Else already closed this conditional scope
				// outputLine = $"{CompilerConstants.LabelCreatePrefixComplete}{label}";

				outputLine = BuildEndIf(lineNumber);
				return true;
			}

			// CHECK ELSE IF ==================================
			// NOTE: ELSE IF is functionally identical to keyword ELSE followed by keyword IF. 
			// The only reason it's treated separately is because we only check "one item" at the beginning of the line and fixing that would be a much more complicated refactor.
			match = elseIfRegex.Match(inputLine);

			if (match.Success)
			{
				// Else If
				// becomes:
				// =>@IfEnd(x+1)
				// label:@IfEndx
				// ?=>@IfEnd(x+1),expression
				// where x is incrementing index

				// string closeBranch = GetBranchLabel(ConditionalKeyword.ElseIf, false);
				// string openBranch = GetBranchLabel(ConditionalKeyword.ElseIf, true);

				// outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{openBranch}\r\n{CompilerConstants.LabelCreatePrefixComplete}{closeBranch}\r\n{CompilerConstants.ConditionLabelJump}{CompilerConstants.LabelReadPrefix}{openBranch},{match.Groups[1].Value}";


				string condition = match.Groups[1].Value;

				if (string.IsNullOrEmpty(condition))
					errorsEncountered.Add(($"Empty ElseIf statement.'", lineNumber));

				string elseLogic = BuildElse(ConditionalKeyword.elseif, lineNumber);
				// Second argument: Reuse the same branch index as was generated for the ELSE above. Essentially this means that the IF is not truly "nested" in the ELSE.
				// The alternative would mean we have to close two branches when encountering the single ENDIF further on: one explicitly for the enclosing ELSE and one implicitly for the nested IF.
				// That was harder, so we go with this solution.
				string ifLogic = BuildIf(condition, ConditionalKeyword.@if, true);

				outputLine = elseLogic + System.Environment.NewLine + ifLogic;

				return true;
			}

			match = elseRegex.Match(inputLine);

			if (match.Success)
			{
				// Else
				// becomes:
				// =>@IfEnd(x+1)
				// label:@IfEndx
				// where x is incrementing index

				// string closeBranch = GetBranchLabel(ConditionalKeyword.Else, false);
				// string openBranch = GetBranchLabel(ConditionalKeyword.Else, true);

				// outputLine = $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{openBranch}\r\n{CompilerConstants.LabelCreatePrefixComplete}{closeBranch}";

				// if (!string.IsNullOrEmpty(match.Groups[1].Value))
				// {
				// 	errorsEncountered.Add(($"Invalid Else condition: '{inputLine}'", lineNumber));
				// }

				outputLine = BuildElse(ConditionalKeyword.@else, lineNumber);

				return true;
			}

			return false;
		}

		public void Reset()
		{
			branchUniqueIndex = 0;
			branchStack.Clear();
			errorsEncountered.Clear();
		}


		private string BuildIf(string condition, ConditionalKeyword keyword, bool incursEndIfDebt = false)
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

		private string BuildElse(ConditionalKeyword keyword, int lineNumber)
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

		private string BuildEndIf(int lineNumber)
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

		private BranchInfo OpenBranch(ConditionalKeyword keyword, int endIfDebt = 0)
		{
			branchStack.Push(new BranchInfo(branchUniqueIndex, keyword, endIfDebt));
			branchUniqueIndex++;
			return branchStack.Peek();
		}

		private BranchInfo CloseBranch(int lineNumber)
		{
			return branchStack.Pop();
		}

		private BranchInfo GetCurrentBranch()
		{
			return branchStack.Peek();
		}

		public void CheckValidity(Action<string, int> errorMessageHandler)
		{
			if (errorMessageHandler == null)
				throw new ArgumentNullException(nameof(errorMessageHandler));

			if (branchStack.Count > 0)
			{
				errorsEncountered.Add(("Mismatched If-EndIf detected.", -1));
			}

			for (int i = 0; i < errorsEncountered.Count; i++)
			{
				errorMessageHandler(errorsEncountered[i].Item1, errorsEncountered[i].Item2);
			}
		}
	}
}
