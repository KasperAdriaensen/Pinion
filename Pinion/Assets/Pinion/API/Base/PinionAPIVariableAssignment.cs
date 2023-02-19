namespace Pinion
{
	using System.Collections.Generic;
	using Pinion.Compiler.Internal;
	using Pinion.Documentation;

	[APISource]
	[DocSourceDisplayName("Variable assignment")]
	public class PinionAPIVariableAssignment
	{
		private const string messageAssignVariableOnly = "Can only assign to a variable.";

		[APICustomCompileIdentifier]
		private static void AssignVariableCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, System.Action<string> compileErrorHandler)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there are exactly two arguments of the right type.
			CompilerArgument targetVariable = providedArguments[0];

			// Disallow using increment/decrement operator with anything but variables. To do otherwise is pointless anyway.
			if (targetVariable.argumentSource != CompilerArgument.ArgSource.Variable)
			{
				compileErrorHandler(messageAssignVariableOnly);
				return;
			}

			instructionCodes.Add(targetVariable.variablePointer.GetIndexInRegister());
		}

		// ASSIGN =====================================================================================
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, int variable, int newValue)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, float variable, float newValue)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, string variable, string newValue)
		{
			container.StringRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, bool variable, bool newValue)
		{
			container.BoolRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		// ADD ASSIGN    =====================================================================================

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("+=")]
		public static void AssignAddVariable(PinionContainer container, int variable, int addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue + addValue);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("+=")]
		public static void AssignAddVariable(PinionContainer container, float variable, float addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue + addValue);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("+=")]
		public static void AssignAddVariable(PinionContainer container, string variable, string addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			string oldValue = container.StringRegister.ReadValue(variableLocation);
			container.StringRegister.WriteValue(variableLocation, oldValue + addValue);
		}

		// SUBTRACT ASSIGN    =====================================================================================

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("-=")]
		public static void AssignSubtractVariable(PinionContainer container, int variable, int addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue - addValue);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("-=")]
		public static void AssignSubtractVariable(PinionContainer container, float variable, float addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue - addValue);
		}

		// MULTIPLY ASSIGN    =====================================================================================

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void AssignMultiplyVariable(PinionContainer container, int variable, int multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue * multiplier);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void AssignMultiplyVariable(PinionContainer container, float variable, float multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue * multiplier);
		}

		// DIVIDE ASSIGN    =====================================================================================

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void AssignDivideVariable(PinionContainer container, int variable, int multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue / multiplier);
		}

		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void AssignDivideVariable(PinionContainer container, float variable, float multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue / multiplier);
		}
	}
}