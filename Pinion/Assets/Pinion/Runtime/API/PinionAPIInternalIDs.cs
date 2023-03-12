namespace Pinion.Internal
{
	public static class PinionAPIInternalIDs
	{
		public const string ReadInt = "InternalReadInt";
		public const string ReadFloat = "InternalReadFloat";
		public const string ReadBool = "InternalReadBool";
		public const string ReadString = "InternalReadString";
		public const string WriteInt = "InternalWriteInt";
		public const string WriteFloat = "InternalWriteFloat";
		public const string WriteBool = "InternalWriteBool";
		public const string WriteString = "InternalWriteString";

		public const string ReadIntArray = "InternalReadIntArray";
		public const string ReadFloatArray = "InternalReadFloatArray";
		public const string ReadBoolArray = "InternalReadBoolArray";
		public const string ReadStringArray = "InternalReadStringArray";
		public const string WriteIntArray = "InternalWriteIntArray";
		public const string WriteFloatArray = "InternalWriteFloatArray";
		public const string WriteBoolArray = "InternalWriteBoolArray";
		public const string WriteStringArray = "InternalWriteStringArray";

		public const string ReadLabel = "InternalReadLabel";
		public const string IfFalseGoTo = "InternalIfFalseGoTo";
		public const string InternalIDGoTo = "InternalGoTo";
		public const string InitBegin = "InternalIDInitBegin";
		public const string InitEnd = "InternalIDInitEnd";
		// Currently not used, easily restored.
		// public const string MainBegin = "InternalIDMainBegin";
		// public const string MainEnd = "InternalIDMainEnd";

		public const string IncrementIntVariablePrefix = "InternalIncrementIntVariablePrefix";
		public const string IncrementFloatVariablePrefix = "InternalIncrementFloatVariablePrefix";
		public const string IncrementIntVariablePostfix = "InternalIncrementIntVariablePostfix";
		public const string IncrementFloatVariablePostfix = "InternalIncrementFloatVariablePostfix";
	}
}
