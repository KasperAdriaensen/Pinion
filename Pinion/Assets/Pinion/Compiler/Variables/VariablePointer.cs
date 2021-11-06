using System;
using System.Collections.Generic;

namespace Pinion.Compiler.Internal
{
	public interface IVariablePointer
	{
		InstructionData GetReadInstruction();
		InstructionData GetWriteInstruction();
		ushort GetIndexInRegister();
		System.Type GetValueType();
	}

	public struct VariablePointer<T> : IVariablePointer
	{
		private static Dictionary<System.Type, string> readIdentifiersPerType = new Dictionary<System.Type, string>
		{
			{typeof(int), PinionAPI.InternalIDReadInt},
			{typeof(float), PinionAPI.InternalIDReadFloat},
			{typeof(bool), PinionAPI.InternalIDReadBool},
			{typeof(string), PinionAPI.InternalIDReadString},
		};

		private static Dictionary<System.Type, string> writeIdentifiersPerType = new Dictionary<System.Type, string>
		{
			{typeof(int), PinionAPI.InternalIDWriteInt},
			{typeof(float), PinionAPI.InternalIDWriteFloat},
			{typeof(bool), PinionAPI.InternalIDWriteBool},
			{typeof(string), PinionAPI.InternalIDWriteString},
		};

		public static string GetReadInstructionIdentifier<U>()
		{
			System.Type type = typeof(T);
			if (readIdentifiersPerType.ContainsKey(type))
				return readIdentifiersPerType[type];

			throw new PinionAPIException($"Unsupported read instruction type: {type}");
		}

		public static string GetWriteInstructionIdentifier<U>()
		{
			System.Type type = typeof(T);
			if (writeIdentifiersPerType.ContainsKey(type))
				return writeIdentifiersPerType[type];

			throw new PinionAPIException($"Unsupported write instruction type: {type}");
		}

		private ushort registerIndexOfVariable;

		// For static: see constructor.
		private static InstructionData readInstruction;
		private static InstructionData writeInstruction;

		public VariablePointer(ushort registerIndexOfVariable)
		{
			this.registerIndexOfVariable = registerIndexOfVariable;

			// This code automatically assigns the right read/write functions for this type.
			// It's by name convention, which is a little shaky, but adding new types should happen infrequently enough that it should be maintainable.
			// If not, this class could be refactored to explicitly accept the right read/write instructions.

			// Only need to this once per type.e.
			if (readInstruction != default(InstructionData) && writeInstruction != default(InstructionData))
				return;

			readInstruction = PinionAPI.GetInternalInstructionByID(GetReadInstructionIdentifier<T>());
			writeInstruction = PinionAPI.GetInternalInstructionByID(GetWriteInstructionIdentifier<T>());
		}

		public ushort GetIndexInRegister()
		{
			return registerIndexOfVariable;
		}

		public InstructionData GetReadInstruction()
		{
			return readInstruction;
		}

		public InstructionData GetWriteInstruction()
		{
			return writeInstruction;
		}

		public Type GetValueType()
		{
			return typeof(T);
		}
	}
}
