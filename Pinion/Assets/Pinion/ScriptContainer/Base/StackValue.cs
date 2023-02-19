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
			// Dictionary maps T to StackValue<T>.

			// If not yet in dictionary, create it now.
			if (!valueTypesToStackValueTypes.TryGetValue(valueType, out Type stackValueType))
			{
				stackValueType = typeof(StackValue<>).MakeGenericType(valueType);
				valueTypesToStackValueTypes.Add(valueType, stackValueType);
			}

			return stackValueType;
		}

		public static ConstructorInfo GetConstructorInfo(Type valueType)
		{
			// Dictionary maps StackValue<T> to that type's Constructor

			// If not yet in dictionary, create it now.
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
			// Dictionary maps StackValue<T> to that type's Read function.

			// If not yet in dictionary, create it now.
			if (!stackValueTypesToReadMethodInfos.TryGetValue(valueType, out MethodInfo methodInfo))
			{
				Type stackValueType = GetStackValueType(valueType);
				methodInfo = stackValueType.GetMethod("Read");
				stackValueTypesToReadMethodInfos.Add(valueType, methodInfo);
			}

			return methodInfo;
		}

		public static StackValue<T> GetContainerWrapper<T>(T container) where T : PinionContainer
		{
			Type containerType = typeof(T);
			Type wrapperType = GetStackValueType(containerType);
			object wrapper = Activator.CreateInstance(wrapperType, container);
			StackValue<T> wrapperTyped = (StackValue<T>)wrapper;
			return wrapperTyped;
		}

		public static StackValue GetContainerWrapper(Type desiredContainerType, PinionContainer container)
		{
			Type wrapperType = GetStackValueType(desiredContainerType);
			StackValue wrapper = (StackValue)Activator.CreateInstance(wrapperType, container);
			return wrapper;
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