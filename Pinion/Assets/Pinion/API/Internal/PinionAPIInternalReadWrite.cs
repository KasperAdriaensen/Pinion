using Pinion.Internal;

namespace Pinion
{
	[APISource]
	public static class PinionAPIInternalReadWrite
	{
		// These are the instructions carried out to read/wrote a value (literal or variable) from/to memory.
		// The typical structure: 
		// 1) Advance to the next instruction in the instruction list. That ushort at that point in the instruction list holds the index in the respective ContainerMemoryRegister.
		// 2) 	- For read: push that value to the stack.
		//		- For write: pop a value from the stack, replace the value in ContainerMemoryRegister with that one.

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadInt)]
		public static int ReadValue_Int(PinionContainer container)
		{
			return container.IntRegister.ReadValue(container.AdvanceToNextInstruction());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadFloat)]
		public static float ReadValue_Float(PinionContainer container)
		{
			return container.FloatRegister.ReadValue(container.AdvanceToNextInstruction());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadBool)]
		public static bool ReadValue_Bool(PinionContainer container)
		{
			return container.BoolRegister.ReadValue(container.AdvanceToNextInstruction());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadString)]
		public static string ReadValue_String(PinionContainer container)
		{
			return container.StringRegister.ReadValue(container.AdvanceToNextInstruction());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteInt)]
		public static void WriteValue_Int(PinionContainer container)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteFloat)]
		public static void WriteValue_Float(PinionContainer container)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<float>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteBool)]
		public static void WriteValue_Bool(PinionContainer container)
		{
			container.BoolRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<bool>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteString)]
		public static void WriteValue_String(PinionContainer container)
		{
			container.StringRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<string>());
		}

		// Array versions. Similar to above, but they take an extra argument to determine an offset to the index in the ContainerMemoryRegister.

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadIntArray)]
		public static int ReadValue_IntArray(PinionContainer container)
		{
			return container.IntRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadFloatArray)]
		public static float ReadValue_FloatArray(PinionContainer container)
		{
			return container.FloatRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadBoolArray)]
		public static bool ReadValue_BoolArray(PinionContainer container)
		{
			return container.BoolRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ReadStringArray)]
		public static string ReadValue_StringArray(PinionContainer container)
		{
			return container.StringRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteIntArray)]
		public static void WriteValue_IntArray(PinionContainer container)
		{
			container.IntRegister.WriteValueToArray(container, container.PopFromStack<int>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteFloatArray)]
		public static void WriteValue_FloatArray(PinionContainer container)
		{
			container.FloatRegister.WriteValueToArray(container, container.PopFromStack<float>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteBoolArray)]
		public static void WriteValue_BoolArray(PinionContainer container)
		{
			container.BoolRegister.WriteValueToArray(container, container.PopFromStack<bool>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.WriteStringArray)]
		public static void WriteValue_StringArray(PinionContainer container)
		{
			container.StringRegister.WriteValueToArray(container, container.PopFromStack<string>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}
	}
}
