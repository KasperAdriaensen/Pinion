using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Pinion.ContainerMemory;

namespace Pinion
{
	public class PinionContainer
	{
		#region Fields
		// We don't define a "ran to completion" flag because what that  means exactly could depend a lot
		// on the exact implementation. Better to implement it there, as needed and as fits the purpose.
		public enum InternalState
		{
			None = 0,
			Initialized = 1,
			Executing = 2,
			Sleeping = 4,
		}

		public const int executionTimeoutMsDefault = 3000; // Could easily be made configurable in the future.

		public int ExecutionTimeoutMs
		{
			get { return executionTimeoutMsCustom > 0 ? executionTimeoutMsCustom : executionTimeoutMsDefault; }
			set { executionTimeoutMsCustom = value; }
		}

		private int CurrentInstructionIndex
		{
			get { return currentInstructionIndex; }
			// do not clamp here - under certain conditions settings this to -1 can be desirable 
			// (e.g. execute command index 0 as next command)
			// a number >= scriptInstructions.Length will just terminate the execution loop
			set { currentInstructionIndex = value; }
		}

		public InternalState StateFlags
		{
			get { return stateFlags; }
		}

		public int mainBlockStartIndex = 0; // TODO Don't like this being a public value. It should not be altered outside of compilation.
		public List<ushort> scriptInstructions = new List<ushort>();    // TODO This should not be editable but hard to avoid.
		public ContainerMemoryRegister<int> IntRegister { get { return intRegister; } }
		public ContainerMemoryRegister<float> FloatRegister { get { return floatRegister; } }
		public ContainerMemoryRegister<string> StringRegister { get { return stringRegister; } }
		public ContainerMemoryRegister<bool> BoolRegister { get { return boolRegister; } }
		public ContainerMemoryRegister<int> LabelRegister { get { return labelRegister; } }

		protected System.Action<LogType, string> logHandler = null;
		protected System.ValueTuple<string, object>[] externalVariables = null;
		protected int resumeIndex = -1;

		private const int stackInitSize = 16;
		private const int jumpLocationStackInitSize = 32;
		private ContainerMemoryRegister<int> intRegister = new ContainerMemoryRegister<int>(64);
		private ContainerMemoryRegister<float> floatRegister = new ContainerMemoryRegister<float>(64);
		private ContainerMemoryRegister<string> stringRegister = new ContainerMemoryRegister<string>(64);
		private ContainerMemoryRegister<bool> boolRegister = new ContainerMemoryRegister<bool>(32);
		private ContainerMemoryRegister<int> labelRegister = new ContainerMemoryRegister<int>(127);
		private Stack<StackValue> stack = new Stack<StackValue>(stackInitSize);
		private Stack<int> jumpLocationStack = new Stack<int>(jumpLocationStackInitSize);
		private int currentInstructionIndex = 0;
		private Stopwatch executeStopwatch = new Stopwatch();
		private InternalState stateFlags = InternalState.None;
		private bool forceStop = false;
		private Dictionary<System.Type, StackValue> stackWrappers = new Dictionary<System.Type, StackValue>(4);
		private int executionTimeoutMsCustom = -1;
		#endregion
		#region Run
		public virtual void Run(System.Action<LogType, string> logHandler = null, params System.ValueTuple<string, object>[] externalVariables)
		{
			this.logHandler = logHandler;
			this.externalVariables = externalVariables;
			RunInternal();
		}

		protected void RunInternal()
		{
			forceStop = false;
			int instructionCount = scriptInstructions.Count;
			stack.Clear();

			executeStopwatch.Reset();
			executeStopwatch.Start();

			// if running this script for the first time, we initialize all container-scope variables
			// on any subsequent runs, we skip this step so they retain their values
			currentInstructionIndex = HasStateFlag(InternalState.Initialized) ? mainBlockStartIndex : 0;

			SetStateFlag(InternalState.Initialized);
			SetStateFlag(InternalState.Executing);
			RemoveStateFlag(InternalState.Sleeping);

			if (resumeIndex >= 0)
			{
				currentInstructionIndex = resumeIndex;
				resumeIndex = -1;
				OnSleepResume();
			}

			// API functions can and will alter the current instruction index through the CurrentInstructionIndex property.
			// This happens by design, so that functions can skip/goto/conditionally branch, ... or get data from the bytecode itself, e.g. in the case of reading from a register.
			// Hence the funky for-loop below without initialization.

			for (; currentInstructionIndex < instructionCount; currentInstructionIndex++)
			{
				// Previous command paused the script, setting resumeIndex >= 0. Script execution will stop.
				// Script will resume from resumeIndex when it is run again. 
				if (resumeIndex >= 0 || forceStop)
					break;

				// Runs the actual api call.
				PinionAPI.CallAPIInstruction(scriptInstructions[currentInstructionIndex], this);

				if (executeStopwatch.ElapsedMilliseconds >= ExecutionTimeoutMs)
				{
					LogWarning($"Script execution time exceeded timeout of {ExecutionTimeoutMs} ms. Script was stopped.");
					break;
				}

				if (forceStop)
					break;
			}

			executeStopwatch.Stop();

			// If we reach the end of the instruction list, we have stopped executing,
			// unless we just got here because Sleep() was encountered.
			if (!HasStateFlag(InternalState.Sleeping))
				RemoveStateFlag(InternalState.Executing);
		}

		public bool HasStateFlag(InternalState state)
		{
			return (stateFlags & state) == state;
		}

		protected void SetStateFlag(InternalState state)
		{
			stateFlags |= state;
		}

		protected void RemoveStateFlag(InternalState state)
		{
			stateFlags &= ~state;
		}
		#endregion
		#region Stack
		public void PushToStack(StackValue value)
		{
			stack.Push(value);
		}

		// kept around while experimenting while experiments with minimizing boxing are ongoing.
		// public void PushToStack<T>(T value)
		// {
		// 	stack.Push(new StackValue<T>(value));
		// }

		// public void Push(int value)
		// {
		// 	stack.Push(new StackValue<int>(value));
		// }

		// public void Push(string value)
		// {
		// 	stack.Push(value);
		// }

		// public void Push(float value)
		// {
		// 	stack.Push(value);
		// }

		// public void Push(bool value)
		// {
		// 	stack.Push(value);
		// }

		// public void Push(object value)
		// {
		// 	stack.Push(value);
		// }

		// public StackValue PopFromStack()
		// {
		// 	return stack.Pop();
		// }

		public T PopFromStack<T>()
		{
			return (stack.Pop() as StackValue<T>).Read();
		}

		public StackValue PopFromStack()
		{
			return stack.Pop();
		}
		#endregion
		#region Flow
		private ushort GetInstructionCodeAtIndex(int index)
		{
			if (index < 0 || index >= scriptInstructions.Count)
			{
				Debug.LogError($"[PinionContainer]Could not find instruction at index {index}. Returning 0.");
				return 0;
			}

			return scriptInstructions[index];
		}

		public ushort AdvanceToNextInstruction()
		{
			return GetInstructionCodeAtIndex(++CurrentInstructionIndex);
		}

		public void SetNextInstructionIndex(int index)
		{
			if (index < 0)
			{
				Debug.LogError($"[PinionContainer] Invalid index ({index}) for next instruction! Index must be >= 0.");
				return;
			}

			// This comparison is correct! 
			// Setting next instruction to scriptInstructions.Count is a valid and common scenario if the script ends on a loop.
			// In this case, the execution loop will simply terminate.
			// However, any higher number should not be able to happen.
			if (index > scriptInstructions.Count)
			{
				Debug.LogError($"[PinionContainer] Invalid index for next instruction! Index must be smaller than or equal to the total instruction count ({scriptInstructions.Count})).");
			}

			// NOTE: Instruction index is always incremented at the end of the instruction loop.
			// To carry out index 17 next, we now need to set CurrentInstructionIndex to 16.
			// This means we can temporarily set it to -1 to start from 0!
			CurrentInstructionIndex = index - 1;
		}

		public void PushJumpLocation(int index)
		{
			jumpLocationStack.Push(index);
		}

		public int PopJumpLocation()
		{
			return jumpLocationStack.Pop();
		}
		#endregion
		#region ExecutionControl
		public void Stop()
		{
			forceStop = true;
			// This would have been inconsistent - reaching the end of the instruction list does not
			// reset Initialized, but calling Stop() does.
			// RemoveStateFlag(InternalState.Initialized); 
			OnStop();
		}

		protected virtual void OnStop()
		{
		}

		public void Sleep()
		{
			SetStateFlag(InternalState.Sleeping);
			resumeIndex = CurrentInstructionIndex + 1;
			OnSleep();
		}

		protected virtual void OnSleep()
		{
		}

		protected virtual void OnSleepResume()
		{
		}
		#endregion
		#region Blocks
		public void BeginInit()
		{
			if (externalVariables == null)
				return;

			// We don't check if the same external variable is "consumed" multiple times.
			// Compilation should already prevent the same variable name being used twice.
			intRegister.StoreExternalVariables(externalVariables);
			floatRegister.StoreExternalVariables(externalVariables);
			stringRegister.StoreExternalVariables(externalVariables);
			boolRegister.StoreExternalVariables(externalVariables);
		}

		public void EndInit()
		{
		}

		public virtual void ParseMetaData(string metaData, System.Action<string> errorMessageReceiver)
		{

		}
		#endregion
		#region Logging
		public void Log(string message)
		{
			if (logHandler != null)
			{
				logHandler(LogType.Log, message);
			}

			Debug.Log(message);
		}

		public void LogWarning(string message)
		{
			if (logHandler != null)
			{
				logHandler(LogType.Warning, message);
			}

			Debug.LogWarning(message);
		}

		public void LogError(string message)
		{
			if (logHandler != null)
			{
				logHandler(LogType.Error, message);
			}

			Debug.LogError(message);
		}
		#endregion
		#region StackWrappers
		public StackValue GetStackWrapperAs(System.Type containerType)
		{
			return stackWrappers[containerType];
		}

		public void GenerateStackWrappers()
		{
			stackWrappers.Clear();
			System.Type currentType = this.GetType();

			while (currentType != null && typeof(PinionContainer).IsAssignableFrom(currentType))
			{
				StackValue stackWrapper = StackValue.GetContainerWrapper(currentType, this);
				stackWrappers.Add(currentType, stackWrapper);
				currentType = currentType.BaseType;
			}
		}
		#endregion
	}
}