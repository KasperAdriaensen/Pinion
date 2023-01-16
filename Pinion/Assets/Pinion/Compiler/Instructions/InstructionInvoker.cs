using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Pinion.ContainerMemory;
using UnityEngine;

namespace Pinion.Compiler.Internal
{
	// Many API instructions need to be called with arguments. Since we don't know the method signatures, we can only call them with a "dynamic" invoke, 
	// which accepts a collection of objects as arguments. This means the invoke code still has to do a bunch of checks and type casting to ensure 
	// that the provided arguments are valid. That's pretty heavy, especially in tight loops.
	// More importantly, all that effort is wasted, because the bytecode compiler should already have ensured the arguments are valid.

	// This class instead creates a "wrapper" lambda that takes as its input 1) a delegate with a known signature, and 2) an object array. 
	// The code inside the wrapper casts the objects to the respective argument types and then calls the inner delegate with the correct types.
	// We can bypass any compatibility checking because, again, that was already handled at compile time.

	// e.g. Given a method with signature Action<int, bool, string>, the InstructionInvoker will create
	// a Func<Action<int,bool,string> innerDelegate, object[] args>, which internally calls
	// innerdelegate((int)args[0], (bool)args[1], (string)args[2])

	// To build the converter methods at runtime, this class uses Expression Trees, which allow you to dynamically create and compile expressions at runtime.

	// The system was inspired by some of these examples, though still differs quite a bit in the end.
	// https://nkohari.wordpress.com/2009/03/06/fast-late-bound-invocation-with-expression-trees/
	// https://github.com/tdupont750/tact.net/blob/master/framework/src/Tact/Reflection/EfficientInvoker.cs
	// https://www.automatetheplanet.com/optimize-csharp-reflection-using-delegates/

	public sealed class InstructionInvoker
	{
		private static Dictionary<string, Type> innerDelegateTypeCache = new Dictionary<string, Type>();
		private static readonly Dictionary<Type, Func<Delegate, StackValue[], StackValue>> innerDelegateToWrapperMap = new Dictionary<Type, Func<Delegate, StackValue[], StackValue>>();
		private static StringBuilder signatureStringBuilder = new StringBuilder(1000);

		private readonly Func<Delegate, StackValue[], StackValue> wrapper = null; // a takes a delegate and a StackValue[] and returns a StackValue.
		private readonly Delegate innerDelegate = null;

		private InstructionInvoker(Delegate innerDelegate, Func<Delegate, StackValue[], StackValue> wrapper)
		{
			this.innerDelegate = innerDelegate;
			this.wrapper = wrapper;
		}

		public StackValue Invoke(params StackValue[] args)
		{
			string argString = string.Empty;
			foreach (StackValue v in args)
			{
				if (v == null)
					break;

				argString += $"{v.GetValueType().ToString()}, ";

			}

			Debug.Log(argString);

			return wrapper(innerDelegate, args);
		}

		// We could generate parameterTypes from methodInfo, but we already have this array elsewhere for other purposes, so no point doing that again.
		public static InstructionInvoker Create(MethodInfo methodInfo, Type[] parameterTypes)
		{
			if (methodInfo == null)
				throw new ArgumentNullException(nameof(methodInfo));

			if (parameterTypes == null)
				throw new ArgumentNullException(nameof(parameterTypes));

			// Get type representing method signature.
			Type innerDelegateType = GetOrCreateDelegateType(methodInfo.ReturnType, parameterTypes);

			// Create new delegate of this type. This *should* always match the signature of method info.
			// C# will test compatibility anyway - no way to avoid this?
			Delegate innerDelegate = Delegate.CreateDelegate(innerDelegateType, methodInfo);

			return new InstructionInvoker(innerDelegate, GetOrCreateMethodWrapper(innerDelegateType, innerDelegate, parameterTypes));
		}

		private static Type GetOrCreateDelegateType(Type returnType, params Type[] arguments)
		{
			// There's a unique delegate type for every combination of arguments + return type
			// For easy retrieval, we combine this into a deterministic string key per combination.
			string signatureKey = GetSignatureString(returnType, arguments);

			// If delegate type was previous generated, get it from dictionary.
			if (innerDelegateTypeCache.ContainsKey(signatureKey))
				return innerDelegateTypeCache[signatureKey];

			// If delegate signature not previous encountered...
			// https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression.getdelegatetype?view=net-5.0
			// Last element is return type. Return type can be void.

			Type[] typesIncludingReturn = new Type[arguments.Length + 1];

			Array.Copy(arguments, typesIncludingReturn, arguments.Length);
			typesIncludingReturn[arguments.Length] = returnType;

			// Generate new delegate type with expressions.
			Type delegateType = Expression.GetDelegateType(typesIncludingReturn);

			// Cache it.
			innerDelegateTypeCache.Add(signatureKey, delegateType);

			return delegateType;
		}

		private static string GetSignatureString(Type returnType, params Type[] parameters)
		{
			signatureStringBuilder.Clear();

			if (parameters != null)
			{
				for (int i = 0; i < parameters.Length; i++)
				{
					signatureStringBuilder.Append(parameters[i].ToString());

					if (i < parameters.Length - 1)
						signatureStringBuilder.Append(",");
				}
			}

			if (returnType != null && returnType != typeof(void))
			{
				signatureStringBuilder.Append("=>");
				signatureStringBuilder.Append(returnType.ToString());
			}

			return signatureStringBuilder.ToString();
		}

		private static Func<Delegate, StackValue[], StackValue> GetOrCreateMethodWrapper(Type innerDelegateType, Delegate innerDelegateInstance, Type[] parameterTypes)
		{
			// If already generated, return it from the dictionary.
			if (innerDelegateToWrapperMap.ContainsKey(innerDelegateType))
				return innerDelegateToWrapperMap[innerDelegateType];

			// Writes logic for converting untyped object[] to invidual, typed arguments.
			CreateParamsExpressions(parameterTypes, out ParameterExpression paramsExpUntyped, out Expression[] paramsExpTyped);

			// Inner delegate is passed in as an abstract "Delegate", but we need to be able to call with a fully specified signature.
			ParameterExpression targetDelegateUntyped = Expression.Parameter(typeof(Delegate), "innerDelegate");
			Expression targetDelegateTyped = Expression.Convert(targetDelegateUntyped, innerDelegateType);

			// This is the expression that calls the inner delegate with the untyped object[] correctly converted. (See CreateParamsExpressions above.)
			InvocationExpression innerDelegateInvoke = Expression.Invoke(targetDelegateTyped, paramsExpTyped);

			LambdaExpression lambdaExpression;

			// We want to be able to use the same signature for all wrappers, regardless of whether they have a return value or not. This means it always has to return something.
			// Wrappers for API methods that return void will simply return null. At execution time, the system knows whether there a return value
			// is expected or not. If not, it will simply ignore the null.

			// If inner delegate has a return value...
			if (innerDelegateInstance.GetMethodInfo().ReturnType != typeof(void))
			{
				// Convert return value of inner delegate to object.
				//	UnaryExpression bodyReturningValue = Expression.Convert(innerDelegateInvoke, typeof(object));

				ConstructorInfo constructorInfo = StackValue.GetConstructorInfo(innerDelegateInstance.GetMethodInfo().ReturnType);
				NewExpression bodyReturningValue = Expression.New(constructorInfo, innerDelegateInvoke);

				// Create lambda, with bodyReturningValue as body and targetDelegateUntyped and paramsExpUntyped as two arguments
				lambdaExpression = Expression.Lambda(bodyReturningValue, targetDelegateUntyped, paramsExpUntyped);
			}
			// If inner delegate does not have a return value...
			else
			{

				// Make a dummy return value of null. (Will simply be ignored.)
				ConstantExpression returnNull = Expression.Constant(null, typeof(StackValue));
				// Append that to the void body as a block expression. Block expression is just a sequence of expressions.
				BlockExpression bodyVoidPlusReturnNull = Expression.Block(innerDelegateInvoke, returnNull);

				// Create lambda, with bodyVoidPlusReturnNull as body and targetDelegateUntyped and paramsExpUntyped as two arguments
				lambdaExpression = Expression.Lambda(bodyVoidPlusReturnNull, targetDelegateUntyped, paramsExpUntyped);
			}

			// Lambda needs to be compiled! This does not come for free, but should only happen once, ideally during some loading stage.
			return (Func<Delegate, StackValue[], StackValue>)lambdaExpression.Compile();
		}

		private static void CreateParamsExpressions(Type[] parameterTypes, out ParameterExpression paramsExpUntyped, out Expression[] paramsExpTyped)
		{
			paramsExpUntyped = Expression.Parameter(typeof(StackValue[]), "untypedArgs"); // these are the untyped arguments fed to the wrapping lambda
			paramsExpTyped = new Expression[parameterTypes.Length]; // these are the typed arguments fed to inner delegate

			// These basically write the code for "(paramsExpTyped[i]) args[i]"
			for (int i = 0; i < parameterTypes.Length; i++)
			{
				// ConstantExpression constExp = Expression.Constant(i, typeof(int)); // create a "literal" int with value i
				// BinaryExpression untypedArgument = Expression.ArrayIndex(paramsExpUntyped, constExp); // retrieve array element in object[] at index constExp
				// paramsExpTyped[i] = Expression.Convert(untypedArgument, parameterTypes[i]); // convert object to correct type

				ConstantExpression constExp = Expression.Constant(i, typeof(int)); // create a "literal" int with value i
				BinaryExpression untypedArgument = Expression.ArrayIndex(paramsExpUntyped, constExp); // retrieve array element in object[] at index constExp
				UnaryExpression typedArgument = Expression.Convert(untypedArgument, StackValue.GetStackValueType(parameterTypes[i]));

				paramsExpTyped[i] = Expression.Call(typedArgument, StackValue.GetReadMethodInfo(parameterTypes[i]));
			}
		}
	}
}