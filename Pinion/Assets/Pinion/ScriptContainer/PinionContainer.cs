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
		public const int executionTimeoutMs = 3000; // Could easily be made configurable in the future.

		// TODO This should probably not be editable.
		public List<ushort> scriptInstructions = new List<ushort>();

		public int CurrentInstructionIndex
		{
			get { return currentInstructionIndex; }
			// do not clamp here - under certain conditions settings this to -1 can be desirable (e.g. execute command index 0 as next command)
			// a number >= scriptInstructions.Length will just terminate the execution loop
			private set { currentInstructionIndex = value; }
		}

		public ContainerMemoryRegister<int> IntRegister { get { return intRegister; } }
		public ContainerMemoryRegister<float> FloatRegister { get { return floatRegister; } }
		public ContainerMemoryRegister<string> StringRegister { get { return stringRegister; } }
		public ContainerMemoryRegister<int> LabelRegister { get { return labelRegister; } }
		public int mainBlockStartIndex = 0; // TODO Don't like this being a public value. It should not be altered outside of compilation.

		protected int resumeIndex = -1;
		protected System.Action<LogType, string> logHandler = null;
		protected System.ValueTuple<string, object>[] externalVariables = null;

		private ContainerMemoryRegister<int> intRegister = new ContainerMemoryRegister<int>(64);
		private ContainerMemoryRegister<float> floatRegister = new ContainerMemoryRegister<float>(64);
		private ContainerMemoryRegister<string> stringRegister = new ContainerMemoryRegister<string>(32);
		private ContainerMemoryRegister<int> labelRegister = new ContainerMemoryRegister<int>(127);

		private Stack<object> stack = new Stack<object>();
		private Stack<int> jumpLocationStack = new Stack<int>();
		private bool initialized = false;
		private int currentInstructionIndex = 0;
		private Stopwatch executeStopwatch = new Stopwatch();

		public virtual void Run(System.Action<LogType, string> logHandler = null, params System.ValueTuple<string, object>[] externalVariables)
		{
			this.logHandler = logHandler;
			this.externalVariables = externalVariables;
			RunInternal();
		}

		protected void RunInternal()
		{
			int instructionCount = scriptInstructions.Count;
			stack.Clear();

			executeStopwatch.Reset();
			executeStopwatch.Start();

			// if running this script for the first time, we initialize all container-scope variables
			// on any subsequent runs, we skip this step so they retain their values
			currentInstructionIndex = initialized ? mainBlockStartIndex : 0;
			initialized = true;

			if (resumeIndex >= 0)
			{
				currentInstructionIndex = resumeIndex;
				resumeIndex = -1;
				OnSleepResume();
			}

			// API functions can and will alter the current instruction index through the CurrentInstructionIndex property.
			// This happens by design, so that functions can skip/goto/conditionally branch, ... or get data from the bytecode itself, e.g. in the case of reading from a register.
			// Hence the funky for loop below without initialization.

			for (; currentInstructionIndex < instructionCount; currentInstructionIndex++)
			{
				// Previous command paused the script, setting resumeIndex >= 0. Script execution will stop.
				// Script will resume from resumeIndex when it is run again. 
				if (resumeIndex >= 0)
					break;

				// Runs the actual api call.
				PinionAPI.CallAPIInstruction(scriptInstructions[currentInstructionIndex], this);

				if (executeStopwatch.ElapsedMilliseconds >= executionTimeoutMs)
				{
					LogWarning($"Script execution time exceeded timeout of {executionTimeoutMs} ms. Script was stopped.");
					break;
				}
			}

			executeStopwatch.Stop();
		}

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

		public void Push(int value)
		{
			stack.Push(value);
		}

		public void Push(string value)
		{
			stack.Push(value);
		}

		public void Push(float value)
		{
			stack.Push(value);
		}

		public void Push(bool value)
		{
			stack.Push(value);
		}

		public void Push(object value)
		{
			stack.Push(value);
		}

		public object PopFromStack()
		{
			return stack.Pop();
		}

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
				Debug.LogError($"[PinionContainer] Invalid index for next instruction! Index must be >= 0.");
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

		public virtual void Stop()
		{
			initialized = false;
		}

		public void Sleep()
		{
			resumeIndex = CurrentInstructionIndex + 1;
			OnSleep();
		}

		protected virtual void OnSleep()
		{

		}

		protected virtual void OnSleepResume()
		{

		}

		public void OnInitBegin()
		{
		}

		public void OnInitEnd()
		{
			if (externalVariables == null)
				return;

			// We don't check if the same external variable is "consumed" multiple times.
			// Compilation should already prevent the same variable name being used twice.
			intRegister.StoreExternalVariables(externalVariables);
			floatRegister.StoreExternalVariables(externalVariables);
			stringRegister.StoreExternalVariables(externalVariables);
		}



		public virtual void ParseMetaData(string metaData, System.Action<string> errorMessageReceiver)
		{

		}
	}
}