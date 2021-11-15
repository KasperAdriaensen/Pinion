using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	public class InstructionData
	{
		public readonly ushort instructionCode = 0;
		public readonly string instructionString = string.Empty;
		public readonly bool internalInstruction = false;
		public readonly Type returnType = null;
		public readonly bool requiresContainer = false;
		public readonly int exposedParameterCount = 0;
		public readonly int fullParameterCount = 0;

		private readonly InstructionInvoker invoker = null;
		private readonly Type[] parameterTypes = null;

		public InstructionData(ushort instructionCode, MethodInfo methodInfo, bool internalInstruction = false)
		{
			this.instructionCode = instructionCode;
			this.instructionString = methodInfo.Name;
			this.internalInstruction = internalInstruction;
			this.returnType = methodInfo.ReturnType;

			this.parameterTypes = BuildAndVerifyParameters(methodInfo, out this.requiresContainer, out this.exposedParameterCount);
			this.fullParameterCount = this.parameterTypes.Length;

			invoker = InstructionInvoker.Create(methodInfo, this.parameterTypes);
		}

		private Type[] BuildAndVerifyParameters(MethodInfo methodInfo, out bool firstIsContainer, out int exposedParamsAmount)
		{
			firstIsContainer = false;

			Type[] foundParameters = methodInfo.GetParameters().Select((ParameterInfo p) => p.ParameterType).ToArray();

			if (foundParameters.Length >= PinionAPI.MaxParameterCount)
				throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: method has {foundParameters.Length} parameters. Maximum is ({PinionAPI.MaxParameterCount}).");

			for (int i = 0; i < foundParameters.Length; i++)
			{
				Type parameterType = foundParameters[i];

				if (PinionAPI.IsSupportedPublicType(parameterType)) // No further checks required.
					continue;

				if (IsTypeOrSubtype(parameterType, typeof(PinionContainer)))
				{
					if (i > 0)
						throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: parameter deriving from {typeof(PinionContainer)} can only be first parameter. Encountered it at parameter index {i}.");

					// No longer a requirement. First PinionContainer parameter should now be "hidden" everywhere.
					// if (internalInstruction == false)
					// 	throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: parameter of type {typeof(PinionContainer)} can only be used in internal API methods.");

					firstIsContainer = true;
				}
				else
				{
					throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: unsupported parameter type at parameter index {i}.");
				}
			}

			exposedParamsAmount = firstIsContainer ? foundParameters.Length - 1 : foundParameters.Length;

			if (returnType != typeof(void) && !PinionAPI.IsSupportedPublicType(returnType))
				throw new PinionAPIException($"Method {methodInfo.Name} in {methodInfo.DeclaringType}: API methods must return a supported type. Method returns {returnType}");

			return foundParameters;
		}

		public void Call(PinionContainer container, object[] parameters)
		{
			if (returnType != typeof(void))
			{
				container.Push(invoker.Invoke(parameters));
			}
			else
			{
				invoker.Invoke(parameters);
			}
		}
		public Type GetParameterType(int parameterIndex)
		{
			if (requiresContainer)
				return parameterTypes[parameterIndex + 1];
			else
				return parameterTypes[parameterIndex];
		}

		public bool IsValidForCallingContainer(PinionContainer callingContainer)
		{
			if (callingContainer == null)
				throw new ArgumentNullException(nameof(callingContainer));

			if (requiresContainer && parameterTypes != null && parameterTypes.Length > 0)
				return IsTypeOrSubtype(callingContainer.GetType(), parameterTypes[0]);

			return true;
		}

		public Type GetExpectedContainerType()
		{
			return requiresContainer ? parameterTypes[0] : null;
		}

		private static bool IsTypeOrSubtype(Type inputType, Type compareType)
		{
			return inputType == compareType || inputType.IsSubclassOf(compareType);
		}

		public bool MatchesArguments(IList<Type> providedArgumentTypes, out int matchCount)
		{
			matchCount = 0;

			for (int i = 0; i < exposedParameterCount; i++)
			{
				if (i >= providedArgumentTypes.Count) // instruction expects more arguments than were provided
					return false;

				if (GetParameterType(i) == providedArgumentTypes[i]) // NOTE: internally increments i by one if requiresContainer == true
					matchCount++;
				else
					return false;
			}

			if (providedArgumentTypes.Count > exposedParameterCount) // more arguments were provided than instruction expects
				return false;


			return true;
		}
	}
}