using System.Collections;
using System.Collections.Generic;
using Pinion;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIInternalReadWrite
	{
		// These are the instructions carried to push a script literal onto the stack.
		// The typical structure: 
		// 1) Advance to the next instruction in the instruction list. 
		// 2) That instruction ushort refers to the index in the respective LiteralRegister where that literal is stored. Retrieve that value (typesafe).
		// 3) Push that value onto the stack. It is pushed onto the stack as a ScriptStackValue.

		// NOTE: Functions that pop the pushed argument off the stack again will receive it as an object. At runtime, nothing enforces that they cast it to the correct type.
		// However, this will be enforced at compile time! Compilation should fail if the list of APIPopArguments does not match the current compilation stack.
		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadInt)]
		public static void ReadValue_Int(PinionContainer container)
		{
			int value = container.IntRegister.ReadValue(container.AdvanceToNextInstruction());
			container.Push(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadFloat)]
		public static void ReadValue_Float(PinionContainer container)
		{
			float value = container.FloatRegister.ReadValue(container.AdvanceToNextInstruction());
			container.Push(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadBool)]
		public static void ReadValue_Bool(PinionContainer container)
		{
			container.Push(container.AdvanceToNextInstruction() != 0); // 0 = false, everything else = true
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadString)]
		public static void ReadValue_String(PinionContainer container)
		{
			string value = container.StringRegister.ReadValue(container.AdvanceToNextInstruction());
			container.Push(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteInt)]
		public static void WriteValue_Int(PinionContainer container)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), (int)container.PopFromStack());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteFloat)]
		public static void WriteValue_Float(PinionContainer container)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), (float)container.PopFromStack());
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteBool)]
		public static void WriteValue_Bool(PinionContainer container)
		{
			throw new System.NotImplementedException();
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDWriteString)]
		public static void WriteValue_String(PinionContainer container)
		{
			container.StringRegister.WriteValue(container.AdvanceToNextInstruction(), (string)container.PopFromStack());
		}
	}
}
