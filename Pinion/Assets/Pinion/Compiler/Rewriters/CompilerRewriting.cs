using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Linq;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.
using System;
using Pinion.Compiler.Internal;
using System.Text;

namespace Pinion.Compiler.Internal
{
	public static class CompilerRewriting
	{
		private static IRewriteHandler[] rewriteHandlersFlowControl = new IRewriteHandler[]
		{
			new RewriteHandlerIf(),
			new RewriteHandlerWhile()
		};

		public static string RemoveComments(string input)
		{
			return Regex.Replace(input, CompilerRegex.commentRegex, string.Empty);
		}

		public static string RemoveWhitespace(string input)
		{
			StringBuilder resultBuilder = new StringBuilder(input.Length); // by definition, can't be longer, we're only removing

			using (StringReader reader = new StringReader(input))
			{
				string line = string.Empty;
				do
				{
					line = reader.ReadLine();

					// Entirely blank lines (empty or whitespace characters only) can be removed at this point.
					// Source line numbers should already have been inserted, so removing these lines (or inserting others later on)
					// should no longer affect correct line number reporting.
					if (string.IsNullOrWhiteSpace(line))
						continue;

					line = Regex.Replace(line, CompilerRegex.whitespaceRemoveRegex, string.Empty);

					resultBuilder.AppendLine(line);
				}
				while (line != null);
			}

			return resultBuilder.ToString();
		}

		public static string InsertSourceLineNumbers(string input)
		{
			StringBuilder resultBuilder = new StringBuilder(input.Length * 2);

			int lineNumber = 0;

			using (StringReader reader = new StringReader(input))
			{
				string line = string.Empty;
				do
				{
					line = reader.ReadLine();
					lineNumber++; // Do this here already: line numbering typically starts at 1 and we *also* want to count blank lines.

					// True for either end of file or blank lines.
					// Blank lines don't need a line label - they could never have code to refer to anyway. They are *counted* however.
					// Lines consisting of only whitespace can also be ignored (IsNullOrWhiteSpace also returns true for empty lines).
					if (!string.IsNullOrWhiteSpace(line))
					{
						resultBuilder.AppendLine(string.Format(CompilerConstants.LineNumberInsert, lineNumber.ToString()));
						resultBuilder.AppendLine(line);
					}
					else
					{
						resultBuilder.AppendLine(line);
					}
				}
				while (line != null);
			}

			return resultBuilder.ToString();
		}

		public static string RewriteFlowAndExecutionControl(string input, System.Action<string, int> errorMessageHandler)
		{
			foreach (IRewriteHandler rewriteHandler in rewriteHandlersFlowControl)
			{
				rewriteHandler.Reset();
			}

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
					Match lineNumberMatch = Regex.Match(line, CompilerRegex.LineNumberRegex);
					if (lineNumberMatch.Success)
					{
						lineNumber = int.Parse(lineNumberMatch.Groups[1].Value); // update "line number"

						// Leave this line unaltered otherwise.
						resultBuilder.AppendLine(line);
						continue;
					}

					foreach (IRewriteHandler rewriteHandler in rewriteHandlersFlowControl)
					{
						// returns true if rewrite happened and stops checking for other flow control.
						// TODO:
						// This is based in the assumption that one line won't contain e.g. both a while AND an if.
						// That seems like a dangerous assumption, but the current logic for adding in the go-to labels
						// will break anyway in that scenario.
						// A knotty problem, which we've opted to accept for now.
						// TODO: Currently does not yet support for-loop.
						if (rewriteHandler.AttemptRewrite(line, out line, lineNumber))
							break;
					}

					resultBuilder.AppendLine(line);
				}
				while (line != null);
			}

			foreach (IRewriteHandler rewriteHandler in rewriteHandlersFlowControl)
			{
				rewriteHandler.CheckValidity(errorMessageHandler);
			}

			return resultBuilder.ToString();
		}
	}
}