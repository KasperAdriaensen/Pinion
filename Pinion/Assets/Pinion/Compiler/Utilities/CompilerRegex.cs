using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Pinion.Compiler.Internal
{
	public static class CompilerRegex
	{
		// REGEX ==========================================================================
		// captures anything in between (), matching greedily, with the contents of the brackets in group 1
		public const string bracketedSubExpressionRegex = @"\((.*)\)";

		// Adapted from https://stackoverflow.com/questions/9155483/regular-expressions-balancing-group
		// This uses balancing group, a regex feature unique to .Net's regex implementation to keep a "stack" of matched opening/closing brackets.
		public const string bracketedSubExpressionRegexComplex = @"\(((?:[^()]|(?<open>\()|(?<-open>\)))+(?(open)(?!)))\)";

		// Valid ways to define an int are: any sequence of digits, with or without "-" in front.
		public const string validIntRegex = @"^-?\d+$";
		// Valid ways to define a float are: "3.2f", "3.2", ".2", ".2f" or "3f" or any version thereof with "-" in front. "3" will be interpreted as int, because it is checked first.
		public const string validFloatRegex = @"^-?\d*\.?\d+f?$";

		// Captures anything that starts with //, all the way to line end. Used to remove comment lines.
		public const string commentRegex = @"\/{2}.*";

		// Full disclosure: I can't find the source that I based this off anymore, but the gist of it is:
		// It selects for any whitespace characters (0 or more) that are not enclosed in quotation marks.
		// This includes line breaks, meaning this can only act on a per-line basis. Haven't quite figured out how to it better without creating other edge cases.
		// NOTE: when testing this in a regex parser, the first \ should be removed. It is part of the "\s" regex character.
		// Normally we'd prepend an @ (verbatim string) to avoid having to use escape characters, but that breaks further down on the other quotation marks. Try it.
		public const string whitespaceRemoveRegex = "\\s+(?=([^\"]*\"[^\"]*\")*[^\"]*$)";

		// Regex for valid label name. Current constraints: must consist of any amount of alphanumeric characters.
		// Also defines string beginning (^) and end ($), so cannot return partial matches.
		public const string validLabelNameRegex = @"^[a-zA-Z0-9]*$";

		// Regex for valid variable name. 
		// Current constraints: must be prefixed with $ (default case) or $$ for system variables, first character must be lower or upper case letter (a-z), followed by any amount of alphanumeric characters.
		// Also defines string beginning (^) and end ($), so cannot return partial matches.
		public const string validVariableNameRegex = @"^\$\$?[a-zA-Z][a-zA-Z0-9]*$";

		public const string matchQuoteText = "\"[^\"]*\"";

		// Used to isolate meta block
		public const string metaBlockRegex = "#META\r?\n(.*)\r?\n#END_META"; //\r?\n matches both Windows and UNIX style line breaks

		public const string variableWriteRegex = @"^set\((.*)\)";
		public const string variableDeclareRegex = @"^declare\((.*)\)";

		public const string LineNumberRegex = @"^#(\d+)#"; // #s surrounding an int, at start of string, capture group contains the number only
	}
}
