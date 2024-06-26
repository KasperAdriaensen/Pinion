using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pinion.ContainerMemory;
using CustomCompileHandler = System.Tuple<Pinion.APICustomCompileRequiredAttribute.HandlerTypes, Pinion.Compiler.Internal.InstructionData.CustomCompileDelegate>;

namespace Pinion.Compiler.Internal
{
	public class InstructionData
	{
		public bool RequiresContainer => (requiredContainerType != null);

		public readonly ushort instructionCode = 0;
		public readonly string instructionString = string.Empty;
		public readonly bool isInternal = false;
		public readonly Type returnType = null;
		public readonly int fullParameterCount = 0;
		public readonly Type requiredContainerType = null;
		public readonly int exposedParameterCount = 0;

		private const BindingFlags customCompileHandlerBindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		public delegate void CustomCompileDelegate(IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, Action<string> compileErrorHandler);

		private readonly Type[] parameterTypes = null;
		private readonly InstructionInvoker invoker = null;
		private readonly List<CustomCompileHandler> customCompileHandlers = null; // Only allocated if needed. (Usually not.)

		public InstructionData(ushort instructionCode, MethodInfo methodInfo, bool isInternal = false)
		{
			this.instructionCode = instructionCode;
			this.instructionString = methodInfo.Name;
			this.isInternal = isInternal;
			this.returnType = methodInfo.ReturnType;
			customCompileHandlers = LinkCustomCompileHandlers(methodInfo);

			this.parameterTypes = BuildAndVerifyParameters(methodInfo, out this.requiredContainerType, out this.exposedParameterCount);
			this.fullParameterCount = this.parameterTypes.Length;

			invoker = InstructionInvoker.Create(methodInfo, this.parameterTypes);
		}

		private List<CustomCompileHandler> LinkCustomCompileHandlers(MethodInfo methodInfo)
		{
			if (!Attribute.IsDefined(methodInfo, typeof(APICustomCompileRequiredAttribute)))
				return null;

			Type declaringType = methodInfo.DeclaringType;
			IEnumerable<APICustomCompileRequiredAttribute> customCompileReferences = methodInfo.GetCustomAttributes<APICustomCompileRequiredAttribute>();
			IEnumerable<MethodInfo> customCompileMethodsInType = declaringType.GetMethods(customCompileHandlerBindingFlags).Where(m => Attribute.IsDefined(m, typeof(APICustomCompileIdentifierAttribute)));
			List<CustomCompileHandler> results = null;

			foreach (APICustomCompileRequiredAttribute customCompilerReference in customCompileReferences)
			{
				string identifier = customCompilerReference.Identifier;
				IEnumerable<MethodInfo> matchingMethods = customCompileMethodsInType.Where(m => m.Name == identifier);

				if (matchingMethods.Count() == 1)
				{
					MethodInfo matchedMethod = matchingMethods.First();
					try
					{
						// CreateDelegate's throwOnBindFailure means an exception will be thrown if binding to type CustomCompileDelegate failed.
						// Ergo, if no exceptions were thrown, casting to CustomCompileDelegate should never return null.
						CustomCompileDelegate customCompile = Delegate.CreateDelegate(typeof(CustomCompileDelegate), matchedMethod, true) as CustomCompileDelegate;

						// We don't allocate a list of handlers unless we actually need it.
						if (results == null)
							results = new List<CustomCompileHandler>();

						results.Add(new CustomCompileHandler(customCompilerReference.HandlerType, customCompile));
					}
					catch (System.Exception e)
					{
						throw new PinionAPIException($"Failed to convert {declaringType.FullName}.{matchedMethod.Name} to custom compile handler. Method signature should match delegate type {nameof(CustomCompileDelegate)}.", e);
					}
				}
				else if (matchingMethods.Count() < 1)
				{
					throw new PinionAPIException(
					$@"API method {declaringType.FullName}.{methodInfo.Name} has an attribute linking it to custom compile handler '{identifier}'.
No method by that name could be found. Method must be declared within the same type and be static.");
				}
				else
				{
					throw new PinionAPIException(
					$@"API method {declaringType.FullName}.{methodInfo.Name} has an attribute linking it to custom compile handler '{identifier}'.
Multiple candidates found. Any static methods named '{identifier}', declared within type {declaringType.FullName}, are considered.");
				}
			}

			return results;
		}

		private Type[] BuildAndVerifyParameters(MethodInfo methodInfo, out Type requiredContainerType, out int exposedParamsAmount)
		{
			requiredContainerType = null;

			Type[] foundParameters = methodInfo.GetParameters().Select((ParameterInfo p) => p.ParameterType).ToArray();

			if (foundParameters.Length >= PinionAPI.MaxParameterCount)
				throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: method has {foundParameters.Length} parameters. Maximum is ({PinionAPI.MaxParameterCount}).");

			for (int i = 0; i < foundParameters.Length; i++)
			{
				Type parameterType = foundParameters[i];

				if (PinionTypes.IsSupportedPublicType(parameterType)) // No further checks required.
					continue;

				// PinionContainer can be passed, but only as first argument.
				// It will not be shown in the user-facing instruction signature.
				if (IsTypeOrSubtype(parameterType, typeof(PinionContainer)))
				{
					if (i > 0)
						throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: parameter deriving from {typeof(PinionContainer)} can only be first parameter. Encountered it at parameter index {i}.");

					requiredContainerType = parameterType;
				}
				else
				{
					throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: unsupported parameter type ({parameterType}) at parameter index {i}.");
				}
			}

			exposedParamsAmount = (requiredContainerType != null) ? foundParameters.Length - 1 : foundParameters.Length;

			if (returnType != typeof(void) && !PinionTypes.IsSupportedPublicType(returnType))
				throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: API methods must return a supported type. Method returns {returnType}");

			return foundParameters;
		}

		public void Call(PinionContainer container, StackValue[] parameters)
		{
			if (returnType != typeof(void))
			{
				container.PushToStack(invoker.Invoke(parameters));
			}
			else
			{
				invoker.Invoke(parameters);
			}
		}

		public Type GetParameterType(int parameterIndex)
		{
			if (RequiresContainer)
				return parameterTypes[parameterIndex + 1];
			else
				return parameterTypes[parameterIndex];
		}

		public bool IsValidForCallingContainer(PinionContainer callingContainer)
		{
			if (callingContainer == null)
				throw new ArgumentNullException(nameof(callingContainer));

			if (RequiresContainer && parameterTypes != null && parameterTypes.Length > 0)
				return IsTypeOrSubtype(callingContainer.GetType(), parameterTypes[0]);

			return true;
		}

		public Type GetExpectedContainerType()
		{
			return RequiresContainer ? parameterTypes[0] : null;
		}

		private static bool IsTypeOrSubtype(Type inputType, Type compareType)
		{
			return inputType == compareType || inputType.IsSubclassOf(compareType);
		}

		public bool MatchesArguments(IList<CompilerArgument> providedArgumentTypes, out int matchCount)
		{
			matchCount = 0;

			for (int i = 0; i < exposedParameterCount; i++)
			{
				if (i >= providedArgumentTypes.Count) // instruction expects more arguments than were provided
					return false;

				if (GetParameterType(i) == providedArgumentTypes[i].argumentType) // NOTE: internally increments i by one if requiresContainer == true
					matchCount++;
				else
					return false;
			}

			if (providedArgumentTypes.Count > exposedParameterCount) // more arguments were provided than instruction expects
				return false;


			return true;
		}

		public bool RunCustomCompileHandlers(APICustomCompileRequiredAttribute.HandlerTypes handlerType, IList<CompilerArgument> providedArguments, IList<ushort> instructionCodes, Action<string> compileErrorHandler)
		{
			// By design this can and often will be true.
			if (customCompileHandlers == null)
				return false;

			bool foundAtleastOne = false;

			foreach (CustomCompileHandler compileHandler in customCompileHandlers)
			{
				if (compileHandler.Item1 == handlerType)
				{
					foundAtleastOne = true;
					compileHandler.Item2.Invoke(providedArguments, instructionCodes, compileErrorHandler);
				}
			}

			return foundAtleastOne;
		}
	}
}