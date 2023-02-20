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
		private const string messageIncrementVariableOnly = "Increment and decrement operator can only be used with variables.";

		#region AssignCompileHandler
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
		#endregion

		#region Assign =
		/// Assigns $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, int variable, int newValue)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		/// Assigns $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, float variable, float newValue)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		/// Assigns $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, string variable, string newValue)
		{
			container.StringRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}

		/// Assigns $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("=")]
		public static void AssignVariable(PinionContainer container, bool variable, bool newValue)
		{
			container.BoolRegister.WriteValue(container.AdvanceToNextInstruction(), newValue);
		}
		#endregion

		#region AddAssign +=
		/// Adds $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("+=")]
		public static void AddAssignVariable(PinionContainer container, int variable, int addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue + addValue);
		}

		/// Adds $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("+=")]
		public static void AddAssignVariable(PinionContainer container, float variable, float addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue + addValue);
		}

		/// Concatenates $2 to variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("+=")]
		public static void AddAssignVariable(PinionContainer container, string variable, string addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			string oldValue = container.StringRegister.ReadValue(variableLocation);
			container.StringRegister.WriteValue(variableLocation, oldValue + addValue);
		}
		#endregion

		#region SubtractAssign -=

		/// Subtracts $2 from variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("-=")]
		public static void SubtractAssign(PinionContainer container, int variable, int addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue - addValue);
		}

		/// Subtracts $2 from variable $1.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("-=")]
		public static void SubtractAssign(PinionContainer container, float variable, float addValue)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue - addValue);
		}
		#endregion

		#region MultiplyAssign *=

		/// Multiplies variable $1 by $2.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void MultiplyAssignVariable(PinionContainer container, int variable, int multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue * multiplier);
		}

		/// Multiplies variable $1 by $2.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void MultiplyAssignVariable(PinionContainer container, float variable, float multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue * multiplier);
		}
		#endregion

		#region DivideAssign /=

		/// Divides variable $1 by $2.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void DivideAssignVariable(PinionContainer container, int variable, int multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			int oldValue = container.IntRegister.ReadValue(variableLocation);
			container.IntRegister.WriteValue(variableLocation, oldValue / multiplier);
		}

		/// Multiplies variable $1 by $2.
		[APIMethod]
		[APICustomCompileRequired(nameof(AssignVariableCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("*=")]
		public static void DivideAssignVariable(PinionContainer container, float variable, float multiplier)
		{
			// The 'variable' argument technically doesn't get used here, but doing it this way significantly simplifies compilation.
			ushort variableLocation = container.AdvanceToNextInstruction();
			float oldValue = container.FloatRegister.ReadValue(variableLocation);
			container.FloatRegister.WriteValue(variableLocation, oldValue / multiplier);
		}
		#endregion

		#region IncrementCompileHandler
		[APICustomCompileIdentifier]
		private static void IncrementCompileHandler(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, System.Action<string> compileErrorHandler)
		{
			// Compiler should already have ensured signature match at this point. 
			// We can be reasonably sure there is exactly one argument of the right type.
			CompilerArgument arg = providedArguments[0];

			// Disallow using increment/decrement operator with anything but variables. To do otherwise is pointless anyway.
			if (arg.argumentSource != CompilerArgument.ArgSource.Variable)
			{
				compileErrorHandler(messageIncrementVariableOnly);
				return;
			}

			instructionCodes.Add(arg.variablePointer.GetIndexInRegister());
		}
		#endregion

		#region Increment ++

		/// Increments (increases by one) variable $1. Increments value before returning.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int IncrementPrefixed(PinionContainer container, int value)
		{
			++value;
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		/// Increments (increases by one) variable $1. Increments value before returning.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static float IncrementPrefixed(PinionContainer container, float value)
		{
			++value;
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		/// Increments (increases by one) variable $1. Returns original value before incrementing.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static int IncrementPostfixed(PinionContainer container, int value)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value + 1);
			return value;
		}

		/// Increments (increases by one) variable $1. Returns original value before incrementing.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("++")]
		public static float IncrementPostfixed(PinionContainer container, float value)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value + 1f);
			return value;
		}
		#endregion

		#region Decrement --

		/// Decrements (decreases by one) variable $1. Decrements value before returning.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static int DecrementPrefixed(PinionContainer container, int value)
		{
			--value;
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		/// Decrements (decreases by one) variable $1. Decrements value before returning.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static float DecrementPrefixed(PinionContainer container, float value)
		{
			--value;
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value);
			return value;
		}

		/// Decrements (decreases by one) variable $1. Returns original value before decrementing.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static int DecrementPostfixed(PinionContainer container, int value)
		{
			container.IntRegister.WriteValue(container.AdvanceToNextInstruction(), value - 1);
			return value;
		}

		/// Decrements (decreases by one) variable $1. Returns original value before decrementing.
		[APIMethod]
		[APICustomCompileRequired(nameof(IncrementCompileHandler), APICustomCompileRequiredAttribute.HandlerTypes.AfterInstruction)]
		[DocMethodOperatorReplace("--")]
		public static float DecrementPostfixed(PinionContainer container, float value)
		{
			container.FloatRegister.WriteValue(container.AdvanceToNextInstruction(), value - 1f);
			return value;
		}
		#endregion
	}
}