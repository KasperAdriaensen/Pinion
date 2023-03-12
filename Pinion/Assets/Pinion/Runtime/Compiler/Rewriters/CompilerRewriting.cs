using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace Pinion.Compiler.Internal
{
	public static partial class CompilerRewriting
	{
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
	}
}