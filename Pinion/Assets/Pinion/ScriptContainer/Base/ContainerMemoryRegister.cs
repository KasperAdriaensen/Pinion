using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion.ContainerMemory
{
	public class ContainerMemoryRegister<T>
	{
		public readonly byte registerMax = 64;
		private T[] register = null;
		private byte registerCount = 0;
		private bool[] registerValueIsLiteral = null;
		private Dictionary<string, byte> externalVariableIndices = null; // in most cases, this won't get used, so no point allocating needlessly

		public ContainerMemoryRegister(byte maxSize)
		{
			registerMax = maxSize;
			register = new T[registerMax];
			registerCount = 0;
			registerValueIsLiteral = new bool[registerMax];
		}

		// Handy for e.g. string, to fill with string empty instead of default null.
		public ContainerMemoryRegister(byte maxSize, T defaultValue) : this(maxSize)
		{
			for (int i = 0; i < register.Length; i++)
			{
				register[i] = defaultValue;
			}
		}

		public bool RegisterValue(T startValue, out ushort index, bool isLiteral, string externalVariableName = null)
		{
			index = 0;
			if (registerCount >= registerMax)
			{
				Debug.LogWarning(string.Format("Exceeded max number of {0} memory entries: {1}.", typeof(T).ToString(), registerMax));
				return false;
			}

			// For literals, we can safely assign several "instances" of the same literal to the same address. The value will never be altered anyway.
			// This saves spaces wherever the same literal value is reused.
			if (isLiteral)
			{
				byte indexFound = 0;
				if (LiteralAlreadyPresent(startValue, out indexFound))
				{
					index = (ushort)indexFound;
					return true;
				}
			}

			register[registerCount] = startValue;
			registerValueIsLiteral[registerCount] = isLiteral;

			// "Exposed" variables save a name so they can be filled in from code outside of the script.
			if (!isLiteral && !string.IsNullOrEmpty(externalVariableName))
			{
				if (externalVariableIndices == null)
					externalVariableIndices = new Dictionary<string, byte>();

				externalVariableIndices.Add(externalVariableName, registerCount);
			}

			// Instruction code is encoded as a ushort, so we'll conveniently convert here already.
			index = (ushort)registerCount;

			registerCount++;

			return true;
		}

		public bool RegisterValue(out ushort index, bool isLiteral, string systemVariableName = null)
		{
			return RegisterValue(default(T), out index, isLiteral, systemVariableName);
		}

		private bool LiteralAlreadyPresent(T value, out byte indexFound)
		{
			indexFound = 0;
			for (byte i = 0; i < registerCount; i++)
			{
				if (registerValueIsLiteral[i] && register[i].Equals(value))
				{
					indexFound = i;
					return true;
				}
			}

			return false;
		}

		public T ReadValue(ushort index)
		{
			if (index < 0 || index >= registerMax)
			{
				Debug.LogError($"Memory index {index} for {typeof(T)} register is out of bounds. Index should be between 0 and {registerMax - 1}. Returning default value.");
				return default(T);
			}

			if (index > registerCount)
			{
				Debug.LogError($"Requesting index from {typeof(T)} register at uninitialized index {index}. Current register size is {registerCount}. Returning default value.");
			}

			return register[index];
		}

		public void WriteValue(ushort index, T newValue)
		{
			if (index < 0 || index >= registerMax)
			{
				Debug.LogErrorFormat("Memory index {0} is out of bounds. Cannot set value in register.", index);
			}

			if (index > registerCount)
			{
				Debug.LogErrorFormat("Cannot write value at index {0}, because value at that index is uninitialized. Current register size is {1}.", index, registerCount);
			}

			register[index] = newValue;
		}

		public void StoreExternalVariables(System.ValueTuple<string, object>[] externalVariables)
		{
			if (externalVariableIndices == null)
				return;

			for (int i = 0; i < externalVariables.Length; i++)
			{
				(string, object) externalVariable = externalVariables[i];

				if (externalVariableIndices.ContainsKey(externalVariable.Item1) && externalVariable.Item2.GetType() == typeof(T)) // TODO: should type-mismatch give runtime error or fail silently as "not a match"?
				{
					byte externalVariableIndex = externalVariableIndices[externalVariable.Item1];
					register[externalVariableIndex] = (T)externalVariable.Item2;
				}
			}
		}
	}
}