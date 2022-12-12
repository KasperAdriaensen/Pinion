using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pinion.ContainerMemory
{
	public class ContainerMemoryRegister<T>
	{
		public readonly byte registerMax = 64;
		private T[] register = null;
		private T defaultValue; // allows overwriting default(string) == null with string.Empty and similar uses cases.
		private byte registerCount = 0;
		private ValueMetadata[] valueMetadata = null;
		private Dictionary<string, byte> externalVariableIndices = null; // in most cases, this won't get used, so no point allocating needlessly

		private enum MemoryType
		{
			Unused,
			Literal,
			Variable,
			ArrayRoot
		}

		private struct ValueMetadata
		{
			public MemoryType memoryType;
			public int arrayLength;
		}

		public ContainerMemoryRegister(byte maxSize)
		{
			registerMax = maxSize;
			register = new T[registerMax];
			registerCount = 0;

			valueMetadata = new ValueMetadata[registerMax];

			for (int i = 0; i < valueMetadata.Length; i++)
			{
				ValueMetadata metadata = valueMetadata[i];
				metadata.memoryType = MemoryType.Unused;
				metadata.arrayLength = -1;
			}
		}

		// Handy for e.g. string, to fill with string empty instead of default null.
		public ContainerMemoryRegister(byte maxSize, T defaultValue) : this(maxSize)
		{
			this.defaultValue = defaultValue;
			for (int i = 0; i < register.Length; i++)
			{
				register[i] = defaultValue;
			}
		}

		public bool RegisterValue(out ushort index, bool isLiteral, string externalVariableName = null)
		{
			return RegisterValueInternal(defaultValue, out index, isLiteral ? MemoryType.Literal : MemoryType.Variable, -1, externalVariableName);
		}

		public bool RegisterValue(T startValue, out ushort index, bool isLiteral, string externalVariableName = null)
		{
			return RegisterValueInternal(startValue, out index, isLiteral ? MemoryType.Literal : MemoryType.Variable, -1, externalVariableName);
		}

		public bool RegisterArray(out ushort startIndex, T[] initializeValues)
		{
			if (initializeValues == null)
				throw new System.ArgumentNullException(nameof(initializeValues));

			startIndex = 0;

			for (int i = 0; i < initializeValues.Length; i++)
			{
				if (RegisterValueInternal(initializeValues[i], out ushort memoryIndex, MemoryType.ArrayRoot, initializeValues.Length, null))
				{
					if (i == 0)
					{
						startIndex = memoryIndex;
					}
				}
				else
				{
					return false;
				}
			}



			return true;
		}

		private bool RegisterValueInternal(T startValue, out ushort index, MemoryType memoryType, int arrayLength, string externalVariableName)
		{
			index = 0;
			if (registerCount >= registerMax)
			{
				Debug.LogWarning(string.Format("Exceeded max number of {0} memory entries: {1}.", typeof(T).ToString(), registerMax));
				return false;
			}

			// For literals, we can safely assign several "instances" of the same literal to the same address. The value will never be altered anyway.
			// This saves spaces wherever the same literal value is reused.
			if (memoryType == MemoryType.Literal)
			{
				byte indexFound = 0;
				if (LiteralAlreadyPresent(startValue, out indexFound))
				{
					index = (ushort)indexFound;
					return true;
				}
			}

			register[registerCount] = startValue;
			valueMetadata[registerCount].memoryType = memoryType;
			valueMetadata[registerCount].arrayLength = arrayLength;

			// "Exposed" variables save a name so they can be filled in from code outside of the script.
			// NOTE: Currently external arrays are not supported. We can't guarantee they would fit in the memory register since they aren't filled in at compile time.
			if (memoryType == MemoryType.Variable && !string.IsNullOrEmpty(externalVariableName))
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

		private bool LiteralAlreadyPresent(T value, out byte indexFound)
		{
			indexFound = 0;
			for (byte i = 0; i < registerCount; i++)
			{
				if (valueMetadata[i].memoryType == MemoryType.Literal && register[i].Equals(value))
				{
					indexFound = i;
					return true;
				}
			}

			return false;
		}

		public T ReadValue(int valueLocation)
		{
			if (valueLocation < 0 || valueLocation >= registerMax)
			{
				Debug.LogError($"Address {valueLocation} for {typeof(T)} register is invalid. Address should range from 0 to {registerMax - 1}. Returning default value.");
				return defaultValue;
			}

			if (valueLocation > registerCount)
			{
				Debug.LogError($"Reading from {typeof(T)} register at invalid address {valueLocation}. Current register size is {registerCount}. Returning default value.");
				return defaultValue;
			}

			return register[valueLocation];
		}

		public T ReadValueFromArray(PinionContainer container, int arrayLocation, int indexInArray)
		{
			if (arrayLocation < 0 || arrayLocation >= registerMax)
			{
				container.LogError($"Address {arrayLocation} for {typeof(T)} register is invalid. Address should range from 0 to {registerMax - 1}. Returning default value.");
				return defaultValue;
			}

			if (arrayLocation > registerCount)
			{
				container.LogError($"Reading from {typeof(T)} register at invalid address {arrayLocation}. Current register size is {registerCount}. Returning default value.");
				return defaultValue;
			}

			if (valueMetadata[arrayLocation].arrayLength <= 0)
			{
				container.LogError($"Array is empty. Returning default value.");
				return defaultValue;
			}

			if (indexInArray < 0 || indexInArray >= valueMetadata[arrayLocation].arrayLength)
			{
				container.LogError($"Requested array index {indexInArray} is out of range. Array's indices range from 0 to {valueMetadata[arrayLocation].arrayLength - 1}. Returning default value.");
				return defaultValue;
			}

			return register[arrayLocation + indexInArray];
		}

		public void WriteValue(ushort valueLocation, T writeValue)
		{
			if (valueLocation < 0 || valueLocation >= registerMax)
			{
				Debug.LogError($"Address {valueLocation} for {typeof(T)} register is invalid. Address should range from 0 to {registerMax - 1}.");
			}

			if (valueLocation > registerCount)
			{
				Debug.LogError($"Writing to {typeof(T)} register at invalid address {valueLocation}. Current register size is {registerCount}.");
			}

			register[valueLocation] = writeValue;
		}

		public void WriteValueToArray(PinionContainer container, T writeValue, int arrayLocation, int indexInArray)
		{
			if (arrayLocation < 0 || arrayLocation >= registerMax)
			{
				container.LogError($"Address {arrayLocation} for {typeof(T)} register is invalid. Address should range from 0 to {registerMax - 1}.");
				return;
			}

			if (arrayLocation > registerCount)
			{
				container.LogError($"Writing to {typeof(T)} register at invalid address {arrayLocation}. Current register size is {registerCount}.");
				return;
			}

			if (valueMetadata[arrayLocation].arrayLength <= 0)
			{
				container.LogError($"Array is empty.");
				return;
			}

			if (indexInArray < 0 || indexInArray >= valueMetadata[arrayLocation].arrayLength)
			{
				container.LogError($"Requested array index {indexInArray} is out of range. Array's indices range from 0 to {valueMetadata[arrayLocation].arrayLength - 1}.");
				return;
			}

			register[arrayLocation + indexInArray] = writeValue;
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