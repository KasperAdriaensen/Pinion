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


		private enum ConditionalKeyword
		{
			If,
			EndIf,
			Else,
			ElseIf,
			IfImplied
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
				outputLine = BuildIf(match.Groups[1].Value, ConditionalKeyword.If);

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

				string elseLogic = BuildElse(ConditionalKeyword.ElseIf, lineNumber);
				// Second argument: Reuse the same branch index as was generated for the ELSE above. Essentially this means that the IF is not truly "nested" in the ELSE.
				// The alternative would mean we have to close two branches when encountering the single ENDIF further on: one explicitly for the enclosing ELSE and one implicitly for the nested IF.
				// That was harder, so we go with this solution.
				string ifLogic = BuildIf(condition, ConditionalKeyword.If, true);

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

				outputLine = BuildElse(ConditionalKeyword.Else, lineNumber);

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

			BranchInfo previousBranch = CloseBranch();
			ConditionalKeyword previousKeyword = previousBranch.keyword;
			int endIfDepth = previousBranch.endIfDebt;

			if (previousKeyword != ConditionalKeyword.If && previousKeyword != ConditionalKeyword.ElseIf)
			{
				errorsEncountered.Add(($"The conditional keyword {keyword} must be preceded by {ConditionalKeyword.If} or {ConditionalKeyword.ElseIf}.", lineNumber));
			}

			string closeBranchLabel = $"{branchLabelBase}{previousBranch.index}";
			string openBranchLabel = $"{branchLabelBase}{OpenBranch(keyword, endIfDepth).index}";

			return $"{CompilerConstants.LabelJump}{CompilerConstants.LabelReadPrefix}{openBranchLabel}\r\n{CompilerConstants.LabelCreatePrefixComplete}{closeBranchLabel}";
		}

		private string BuildEndIf(int lineNumber)
		{
			string result = string.Empty;

			do
			{
				BranchInfo previousBranch = CloseBranch();
				ConditionalKeyword previousKeyword = previousBranch.keyword;

				if (previousKeyword == ConditionalKeyword.EndIf)
				{
					errorsEncountered.Add(($"The conditional keyword {ConditionalKeyword.EndIf} must be preceded by {ConditionalKeyword.If}, {ConditionalKeyword.Else}, or {ConditionalKeyword.ElseIf}.", lineNumber));
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
			while (branchStack.Count > 0);

			return result;
		}

		private BranchInfo OpenBranch(ConditionalKeyword keyword, int endIfDebt = 0)
		{
			branchStack.Push(new BranchInfo(branchUniqueIndex, keyword, endIfDebt));
			branchUniqueIndex++;
			return branchStack.Peek();
		}

		private BranchInfo CloseBranch()
		{
			return branchStack.Pop();
		}

		private BranchInfo GetCurrentBranch()
		{
			return branchStack.Peek();
		}

		/*

				private string GetBranchLabel(ConditionalKeyword keyword, bool openBranch)
				{
					// openBranch == true always amounts to doing the same as if keyword
					// for closing a branch, we check if we're closing the branch with a valid new keyword
					// e.g. if-elseif-else-endif is valid, but not if-else-elseif-endif or if-else-else-endif, etc.

					if (openBranch || keyword == ConditionalKeyword.If)
					{
						ifScope.Push((ifUniqueIndex, keyword));
						ifUniqueIndex++;
						return $"IfEnd{ifScope.Peek()}";
					}

					// Below, openBranch is ALWAYS false. In other words: checks below only relate to closing branches

					if (keyword == ConditionalKeyword.ElseIf || keyword == ConditionalKeyword.Else)
					{
						(int, ConditionalKeyword) scopeInfo = ifScope.Pop();
						ConditionalKeyword previousKeyword = scopeInfo.Item2;

						if (previousKeyword != ConditionalKeyword.If || previousKeyword != ConditionalKeyword.ElseIf)
						{
							errorsEncountered.Add($"The conditional keyword {keyword} must be preceded by {ConditionalKeyword.Else} or {ConditionalKeyword.ElseIf}.");
						}

						int value = scopeInfo.Item1;
						return $"IfEnd{value}";
					}

					if (keyword == ConditionalKeyword.EndIf)
					{
						(int, ConditionalKeyword) scopeInfo = ifScope.Pop();
						ConditionalKeyword previousKeyword = scopeInfo.Item2;

						if (previousKeyword == ConditionalKeyword.EndIf)
						{
							errorsEncountered.Add($"The conditional keyword {ConditionalKeyword.EndIf} must be preceded by {ConditionalKeyword.If}, {ConditionalKeyword.Else}, or {ConditionalKeyword.ElseIf}.");
						}

						int value = scopeInfo.Item1;
						return $"IfEnd{value}";
					}

					throw new System.NotSupportedException("Unexpected conditional keyword: " + keyword.ToString());
				}*/

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
