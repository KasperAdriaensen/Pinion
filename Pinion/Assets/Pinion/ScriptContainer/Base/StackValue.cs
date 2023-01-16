using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pinion.ContainerMemory
{
	public abstract class StackValue
	{
		private static Dictionary<Type, Type> valueTypesToStackValueTypes = new Dictionary<Type, Type>();
		private static Dictionary<Type, ConstructorInfo> stackValueTypesToConstructors = new Dictionary<Type, ConstructorInfo>();
		private static Dictionary<Type, MethodInfo> stackValueTypesToReadMethodInfos = new Dictionary<Type, MethodInfo>();

		public static Type GetStackValueType(Type valueType)
		{
			if (!valueTypesToStackValueTypes.TryGetValue(valueType, out Type stackValueType))
			{
				stackValueType = typeof(StackValue<>).MakeGenericType(valueType);
				valueTypesToStackValueTypes.Add(valueType, stackValueType);
			}

			return stackValueType;
		}

		public static ConstructorInfo GetConstructorInfo(Type valueType)
		{
			if (!stackValueTypesToConstructors.TryGetValue(valueType, out ConstructorInfo constructorInfo))
			{
				Type stackValueType = GetStackValueType(valueType);
				constructorInfo = stackValueType.GetConstructor(new Type[] { valueType });
				stackValueTypesToConstructors.Add(valueType, constructorInfo);
			}
			return constructorInfo;
		}

		public static MethodInfo GetReadMethodInfo(Type valueType)
		{
			if (!stackValueTypesToReadMethodInfos.TryGetValue(valueType, out MethodInfo methodInfo))
			{
				Type stackValueType = GetStackValueType(valueType);
				methodInfo = stackValueType.GetMethod("Read");
				stackValueTypesToReadMethodInfos.Add(valueType, methodInfo);
			}

			return methodInfo;
		}

		public abstract Type GetValueType();
	}

	public class StackValue<T> : StackValue
	{
		private T value;

		public StackValue(T value)
		{
			this.value = value;
		}

		public override Type GetValueType()
		{
			return typeof(T);
		}

		public T Read()
		{
			return value;
		}
	}
}