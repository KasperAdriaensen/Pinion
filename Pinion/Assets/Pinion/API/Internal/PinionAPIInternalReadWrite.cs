using System.Collections;
using System.Collections.Generic;
using Pinion;
using UnityEngine;

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
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadInt)]
		public static void ReadValue_Int(PinionContainer container)
		{
			int value = container.IntRegister.ReadValue(container.AdvanceToNextInstruction());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadFloat)]
		public static void ReadValue_Float(PinionContainer container)
		{
			float value = container.FloatRegister.ReadValue(container.AdvanceToNextInstruction());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadBool)]
		public static void ReadValue_Bool(PinionContainer container)
		{
			bool value = container.BoolRegister.ReadValue(container.AdvanceToNextInstruction());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadString)]
		public static void ReadValue_String(PinionContainer container)
		{
			string value = container.StringRegister.ReadValue(container.AdvanceToNextInstruction());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteInt)]
		public static void WriteValue_Int(PinionContainer container)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteFloat)]
		public static void WriteValue_Float(PinionContainer container)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<float>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteBool)]
		public static void WriteValue_Bool(PinionContainer container)
		{
			container.BoolRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<bool>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteString)]
		public static void WriteValue_String(PinionContainer container)
		{
			container.StringRegister.WriteValue(container.AdvanceToNextInstruction(), container.PopFromStack<string>());
		}

		// Array versions. Similar to above, but they take an extra argument to determine an offset to the index in the ContainerMemoryRegister.

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadIntArray)]
		public static void ReadValue_IntArray(PinionContainer container)
		{
			int value = container.IntRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadFloatArray)]
		public static void ReadValue_FloatArray(PinionContainer container)
		{
			float value = container.FloatRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadBoolArray)]
		public static void ReadValue_BoolArray(PinionContainer container)
		{
			bool value = container.BoolRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadStringArray)]
		public static void ReadValue_StringArray(PinionContainer container)
		{
			string value = container.StringRegister.ReadValueFromArray(container, container.AdvanceToNextInstruction(), container.PopFromStack<int>());
			container.PushToStack(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteIntArray)]
		public static void WriteValue_IntArray(PinionContainer container)
		{
			container.IntRegister.WriteValueToArray(container, container.PopFromStack<int>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteFloatArray)]
		public static void WriteValue_FloatArray(PinionContainer container)
		{
			container.FloatRegister.WriteValueToArray(container, container.PopFromStack<float>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteBoolArray)]
		public static void WriteValue_BoolArray(PinionContainer container)
		{
			container.BoolRegister.WriteValueToArray(container, container.PopFromStack<bool>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteStringArray)]
		public static void WriteValue_StringArray(PinionContainer container)
		{
			container.StringRegister.WriteValueToArray(container, container.PopFromStack<string>(), container.AdvanceToNextInstruction(), container.PopFromStack<int>());
		}
	}
}
