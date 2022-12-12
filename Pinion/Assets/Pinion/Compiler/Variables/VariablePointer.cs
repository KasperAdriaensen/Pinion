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
		bool IsArray { get; }
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

		private static Dictionary<System.Type, string> readIdentifiersArrayPerType = new Dictionary<System.Type, string>
		{
			{typeof(int), PinionAPI.InternalIDReadIntArray},
			{typeof(float), PinionAPI.InternalIDReadFloatArray},
			{typeof(bool), PinionAPI.InternalIDReadBoolArray},
			{typeof(string), PinionAPI.InternalIDReadStringArray},
		};

		private static Dictionary<System.Type, string> writeIdentifiersArrayPerType = new Dictionary<System.Type, string>
		{
			{typeof(int), PinionAPI.InternalIDWriteIntArray},
			{typeof(float), PinionAPI.InternalIDWriteFloatArray},
			{typeof(bool), PinionAPI.InternalIDWriteBoolArray},
			{typeof(string), PinionAPI.InternalIDWriteStringArray},
		};


		private static string GetReadInstructionIdentifier<U>()
		{
			System.Type type = typeof(T);
			if (readIdentifiersPerType.ContainsKey(type))
				return readIdentifiersPerType[type];

			throw new PinionAPIException($"Unsupported read instruction type: {type}");
		}

		private static string GetWriteInstructionIdentifier<U>()
		{
			System.Type type = typeof(T);
			if (writeIdentifiersPerType.ContainsKey(type))
				return writeIdentifiersPerType[type];

			throw new PinionAPIException($"Unsupported write instruction type: {type}");
		}

		private static string GetReadArrayInstructionIdentifier<U>()
		{
			System.Type type = typeof(T);
			if (readIdentifiersArrayPerType.ContainsKey(type))
				return readIdentifiersArrayPerType[type];

			throw new PinionAPIException($"Unsupported array read instruction type: {type}");
		}

		private static string GetWriteArrayInstructionIdentifier<U>()
		{
			System.Type type = typeof(T);
			if (writeIdentifiersArrayPerType.ContainsKey(type))
				return writeIdentifiersArrayPerType[type];

			throw new PinionAPIException($"Unsupported array write instruction type: {type}");
		}

		public bool IsArray { get; private set; }

		// For static: see constructor.
		private static InstructionData readInstruction = null;
		private static InstructionData writeInstruction = null;
		private static InstructionData readInstructionArray = null;
		private static InstructionData writeInstructionArray = null;
		private static bool foundReadWriteInstructions = false;

		private ushort registerIndexOfVariable;

		public VariablePointer(ushort registerIndexOfVariable) : this(registerIndexOfVariable, -1)
		{
		}

		public VariablePointer(ushort registerIndexOfVariable, int arrayLength)
		{
			this.registerIndexOfVariable = registerIndexOfVariable;
			this.IsArray = arrayLength >= 0;

			// This code automatically assigns the right read/write functions for this type.

			// Only need to this once per type.
			if (!foundReadWriteInstructions)
			{
				readInstruction = PinionAPI.GetInternalInstructionByID(GetReadInstructionIdentifier<T>());
				writeInstruction = PinionAPI.GetInternalInstructionByID(GetWriteInstructionIdentifier<T>());
				readInstructionArray = PinionAPI.GetInternalInstructionByID(GetReadArrayInstructionIdentifier<T>());
				writeInstructionArray = PinionAPI.GetInternalInstructionByID(GetWriteArrayInstructionIdentifier<T>());

				foundReadWriteInstructions = true;
			}
		}

		public ushort GetIndexInRegister()
		{
			return registerIndexOfVariable;
		}

		public InstructionData GetReadInstruction()
		{
			return IsArray ? readInstructionArray : readInstruction;
		}

		public InstructionData GetWriteInstruction()
		{
			return IsArray ? writeInstructionArray : writeInstruction;
		}

		public Type GetValueType()
		{
			return typeof(T);
		}
	}
}
