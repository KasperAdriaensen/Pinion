namespace Pinion
{
	using System.Collections.Generic;
	using Pinion.Compiler.Internal;
	using Pinion.Documentation;
	using Pinion.Internal;

	[APISource]
	[DocSourceDisplayName("Arrays")]
	public static class PinionAPIArrays
	{
		private const string messageAssignVariableOnly = "Can only get the length of an array variable.";


		#region GetLengthCompileHandler
		[APICustomCompileIdentifier]
		private static void GetLengthCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, System.Action<string> compileErrorHandler)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there is exactly one argument of the right type.
			CompilerArgument arg = providedArguments[0];

			// Disallow using GetLength with anything but array variables.
			if (arg.argumentSource != CompilerArgument.ArgSource.Variable || !arg.variablePointer.IsArray)
			{
				compileErrorHandler(messageAssignVariableOnly);
				return;
			}

			instructionCodes.Add(arg.variablePointer.GetIndexInRegister());
		}
		#endregion

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPIInternalIDs.ArrayZeroOffset)]
		public static int ArrayZeroOffset()
		{
			return 0;
		}

		/// Returns the length of passed array.
		[APIMethod]
		[APICustomCompileRequired(nameof(GetLengthCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		public static int GetLength(PinionContainer container, bool array)
		{
			// The array value goes unused
			return container.BoolRegister.GetArrayLength(container, container.AdvanceToNextInstruction());
		}

		/// Returns the length of passed array.
		[APIMethod]
		[APICustomCompileRequired(nameof(GetLengthCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		public static int GetLength(PinionContainer container, int array)
		{
			// The array value goes unused
			return container.IntRegister.GetArrayLength(container, container.AdvanceToNextInstruction());
		}

		/// Returns the length of passed array.
		[APIMethod]
		[APICustomCompileRequired(nameof(GetLengthCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		public static int GetLength(PinionContainer container, float array)
		{
			// The array value goes unused
			return container.FloatRegister.GetArrayLength(container, container.AdvanceToNextInstruction());
		}

		/// Returns the length of passed array.
		[APIMethod]
		[APICustomCompileRequired(nameof(GetLengthCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		public static int GetLength(PinionContainer container, string array)
		{
			// The array value goes unused
			return container.StringRegister.GetArrayLength(container, container.AdvanceToNextInstruction());
		}
	}
}