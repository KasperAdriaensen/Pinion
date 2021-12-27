using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	public static class CompilerConstants
	{
		// Labels
		public const string LabelReadPrefix = "@";
		public const string LabelCreatePrefix = "label:";
		public const string LabelCreatePrefixComplete = LabelCreatePrefix + LabelReadPrefix;

		// If false, go to label x. Styled as this string because then the actual method can stay an internal one that users can't manually write.
		// Also, it's not a valid C# method name, so it can't accidentally match some API method...
		public const string ConditionLabelJump = "?=>";
		// Always go to label. Otherwise same as above.
		public const string LabelJump = "=>";

		public const string VariablePrefix = "$";

		public const string VariableDeclare = "Declare(";
		public const string VariableSet = "Set(";

		public const string InitBeginMarker = "#INIT";
		public const string InitEndMarker = "#END_INIT";

		public const string MainBeginMarker = "#MAIN";
		public const string MainEndMarker = "#END_MAIN";

		public const string MetaBeginMarker = "#META";
		public const string MetaEndMarker = "#END_META";

		public const string FunctionBeginMarker = "#FUNCTION";
		public const string FunctionEndMarker = "#END_FUNCTION";

		public const string LineNumberInsert = "#{0}#"; // Must match LineNumberRegex with {0} being a line number int
	}
}
