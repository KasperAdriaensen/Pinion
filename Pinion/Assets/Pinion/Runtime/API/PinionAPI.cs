using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion;
using ArgList = System.Collections.ObjectModel.ReadOnlyCollection<System.Type>; // This was getting lengthy.
using System.Reflection;
using Pinion.Compiler.Internal;
using System.Linq.Expressions;
using System.Linq;
using System;
using Pinion.ContainerMemory;
using System.ComponentModel;

namespace Pinion
{
	public static partial class PinionAPI
	{
		/* NOMENCLATURE =====================================
		In the code below, there's quite a bit of switching between representing the instruction as a string, ushort...
		For clarity's sake, the code below aims to use the following naming as consistently as possible:
		* ushort 			-> "instruction code" / "code"
		* string 			-> "instruction string" / "string"
		* InstructionData 	-> "instruction data" - custom class that contains all necessary info for this API call
		*/

		public const int MaxParameterCount = 16;
		private const ushort firstCustomInstructionCode = 16; // Leave some unoccupied for safety/futureproofing.
		private static bool attemptedBuild = false;
		private static bool buildSuccess = false;

		public static bool BuiltSuccessfully
		{
			get
			{
				return attemptedBuild && buildSuccess;
			}
		}

		private static InstructionData[] instructionLookUpTable = null;
		private static Dictionary<string, List<InstructionData>> instructionStringsLookup = new Dictionary<string, List<InstructionData>>();
		private static Dictionary<string, InstructionData> internalIdentifierLookup = new Dictionary<string, InstructionData>();
		private static StackValue[] instructionParametersReuse = new StackValue[MaxParameterCount];

		// We don't do too much validation - most of this will be handled by the C-Sharp compiler anyway.
		// Just checking we don't conflict with some internal keywords.
		// Note: This is not for checking actual instructions! Actual internal instructions are checked in a more sophisticated way.
		private static HashSet<string> reservedInstructionStrings = new HashSet<string>
		{
			"set",
			"declare",
			"if",
			"endif",
			"else",
			"elseif",
			"while",
			"endwhile"
		};

		public static bool BuildAPI(System.Action<string> errorMessageReceiver)
		{
			return BuildAPI(errorMessageReceiver, false, null);
		}

		public static bool BuildAPI(System.Action<string> errorMessageReceiver, bool forceRebuild)
		{
			return BuildAPI(errorMessageReceiver, forceRebuild, null);
		}

		public static bool BuildAPI(System.Action<string> errorMessageReceiver, bool forceRebuild, params Type[] extraAPISources)
		{
			if (attemptedBuild)
				return buildSuccess;

			instructionStringsLookup.Clear();
			internalIdentifierLookup.Clear();

			// GetDiscoverableAPISources returns all classes marked with [APISource].
			// extraAPISources can be used to build from extra API sources *not* marked with marked with [APISource].
			// The latter is a pretty exceptional use case for if you want certain API to only be compilable in certain contexts.
			// E.g. current only use: to make sure the API from the examples in the package isn't always automatically included.

			IEnumerable<(APIMethodAttribute, MethodInfo)> allAPIMethods;

			if (extraAPISources != null)
			{
				allAPIMethods = GetAPIMethodsFromSources(GetDiscoverableAPISources().Union(extraAPISources));
			}
			else
			{
				allAPIMethods = GetAPIMethodsFromSources(GetDiscoverableAPISources());
			}

			bool overallSuccess = true;
			ushort currentInstructionCode = firstCustomInstructionCode;
			List<InstructionData> builtInstructionData = new List<InstructionData>();

			foreach ((APIMethodAttribute, MethodInfo) methodInSource in allAPIMethods)
			{
				// If we've failed to build a previous API methods, no point continuing with the rest.
				if (overallSuccess == false)
					break;

				// NOTE: we could also adapt GetAllAPIFunctions() to only return functions marked with the APIFunction attribute.
				// But we need access to data inside that attribute anyway, so we might as well do it here.
				APIMethodAttribute methodAttribute = methodInSource.Item1;
				MethodInfo methodInfo = methodInSource.Item2;
				string instructionString = methodInfo.Name;

				InstructionData instructionData = null;

				try
				{
					// See note above reservedInstructionStrings.
					if (reservedInstructionStrings.Contains(instructionString))
						throw new PinionAPIException($"'{instructionString}' is a reserved instruction string. Cannot define an API call with this name.");

					instructionData = new InstructionData(currentInstructionCode, methodInfo, methodAttribute.HasFlag(APIMethodFlags.Internal));

					if (instructionData == null)
					{
						overallSuccess = false; // Let's not break the loop yet, so we can still display other errors, should there be any.
						continue;
					}

					// Several instructions can share the same instruction string, but with a different parameter signature (overloads). 
					// This means one string can have multiple potential matches. Here we build a Dictionary<string, InstructionData> to reflect this 1-to-n relationship.
					// NOTE: Resolving string match to the right signature *ONLY* happens at compile time. 

					if (instructionStringsLookup.ContainsKey(instructionString))
					{
						List<InstructionData> lookupList = instructionStringsLookup[instructionString];
						bool currentInstructionIsInternal = instructionData.isInternal;

						foreach (InstructionData data in lookupList)
						{
							// Silly check: we'd rather not have implementers overload internal instructions. (Not strictly a problem, but then you could also overload it with the same arguments!)
							// Of course, we don't know whether the interal or the normal function will be encountered first.
							// Therefore, we say that either all internal or all normal is fine, just not a mix of the two.
							if (data.isInternal != currentInstructionIsInternal)
							{
								overallSuccess = false;
								throw new PinionAPIException($"Instruction '{instructionString}' could not be generated. The name '{instructionString}' is reserved for an internal instruction.");
							}
						}

						lookupList.Add(instructionData);
					}
					else
					{
						// we initialize with a very low capacity - in many cases this will be enough and the capacity will expand if needed.
						List<InstructionData> overloads = new List<InstructionData>(4);
						overloads.Add(instructionData);
						instructionStringsLookup.Add(instructionString, overloads);
					}

					// Internal methods can be used for logic the script user should not have access to. Typically they are inserted into the instruction list at compile time.
					// We have no good way of knowing beforehand what instruction code they will be assigned. 
					// We could just identify/search for them by their actual method name, but that would mean rewriting the compiler if we chose to rename that function.
					// Instead, we give them a fixed identifier that can be used for compile-time lookups, indepent from the function name. Also means there's still a debuggable link between the two.
					// E.g. output.Add(PinionAPI.GetInternalByID(PinionAPI.ReadIntID).instructionCode);
					if (methodAttribute.HasFlag(APIMethodFlags.Internal))
					{
						APIInternalMethodIdentifierAttribute methodIdentifier =
							methodInfo.GetCustomAttribute(typeof(APIInternalMethodIdentifierAttribute), false) as APIInternalMethodIdentifierAttribute;

						if (methodIdentifier != null)
						{
							string identifier = methodIdentifier.Identifier;

							if (!internalIdentifierLookup.ContainsKey(identifier))
							{
								internalIdentifierLookup.Add(identifier, instructionData);
							}
							else
							{
								overallSuccess = false;
								throw new PinionAPIException($"Internal methods {internalIdentifierLookup[identifier].instructionString} and {instructionString} are assigned the same identifier ({identifier})!");
							}
						}
					}

					builtInstructionData.Add(instructionData);
					currentInstructionCode++;
				}
				catch (System.Exception exception)
				{
					overallSuccess = false;
#if UNITY_EDITOR
					// Allows double-clicking in the console window to go to the location the exception was thrown. (Only useful inside Unity editor.)
					// Beyond that, the same information is already relayed by the errorMessageReceiver;
					Debug.LogException(exception);
#endif
					if (errorMessageReceiver != null)
						errorMessageReceiver($"An exception occurred while building API function {instructionString}. Message: {exception.Message}");

				}
			}

			// Overall success - if a single instruction failed to build properly, we don't want to allow any scripts to compile.
			if (!overallSuccess)
			{
				instructionStringsLookup.Clear();
				internalIdentifierLookup.Clear();
				instructionLookUpTable = null;
				Debug.Log($"Failed to build API.");
				return false;
			}

			// Throw away any excess capacity. Currently, there is no way to alter this past this point.
			// But instructionStringsLookup does keep existing, so let's not waste the memory.
			foreach (List<InstructionData> matchList in instructionStringsLookup.Values)
			{
				matchList.TrimExcess();
			}

			instructionLookUpTable = new InstructionData[firstCustomInstructionCode + builtInstructionData.Count];

			for (int i = 0; i < builtInstructionData.Count; i++)
			{
				instructionLookUpTable[firstCustomInstructionCode + i] = builtInstructionData[i];
			}

			buildSuccess = overallSuccess;
			attemptedBuild = true;

			Debug.Log($"API built succesfully. {builtInstructionData.Count} API functions discovered.");
			return overallSuccess;
		}








		public static void CallAPIInstruction(ushort instructionCode, PinionContainer callingContainer)
		{
			// We do not check whether the passed instruction code is within the bounds of instructionLookupTable, because that would be wasted effort.
			// If it is invalid - compilation did not do it's just job correctly.
			InstructionData instructionData = instructionLookUpTable[instructionCode];

#if UNITY_EDITOR && PINION_RUNTIME_DEBUG

			string signature = string.Empty;
			for (int i = 0; i < instructionData.exposedParameterCount; i++)
			{
				signature += instructionData.GetParameterType(i).ToString();

				if (i < instructionData.exposedParameterCount - 1)
					signature += ",";
			}

			Debug.Log($"Calling: {instructionData.instructionString}({signature}).");
#endif

			if (instructionData.RequiresContainer)
			{
				instructionParametersReuse[0] = callingContainer.GetStackWrapperAs(instructionData.requiredContainerType); // the container is never on the stack

				// Arguments are on the stack in "reverse" order. First popped argument is the last one!
				// start from compileData.parameterCount -> first index is (skipped index 0) + (compileData.parameterCount-1)
				// iterate while i > 0 because index 0 is already filled in!
				for (int i = instructionData.exposedParameterCount; i > 0; i--)
				{
					instructionParametersReuse[i] = callingContainer.PopFromStack();
					//Debug.Log(instructionParametersReuse[i]);
				}
			}
			else
			{
				// Arguments are on the stack in "reverse" order. First popped argument is the last one!
				// start from compileData.parameterCount-1 -> for four arguments, start at index 3
				// iterate while i >= 0
				for (int i = instructionData.exposedParameterCount - 1; i >= 0; i--)
				{
					instructionParametersReuse[i] = callingContainer.PopFromStack();
					//Debug.Log(instructionParametersReuse[i]);
				}
			}

			// The system to create delegates requires us to work with an object[], not something something more flexible like List<object>.
			// However, since the delegate creation logic also hardcodes the number of parameters to read, we don't need to worry about passing the correct number of arguments.
			// Barring unforeseen bugs, it can only ever read the correct indices/amount. Indices beyond that are ignored.
			instructionData.Call(callingContainer, instructionParametersReuse);
		}


		private static IEnumerable<(APIMethodAttribute, MethodInfo)> GetAPIMethodsFromSources(IEnumerable<Type> allSources)
		{
			foreach (Type sourceType in allSources)
			{
				foreach ((APIMethodAttribute, MethodInfo) apiMethod in GetAPIMethodsInSource(sourceType))
				{
					yield return apiMethod;
				}
			}
		}

		private static IEnumerable<(APIMethodAttribute, MethodInfo)> GetAPIMethodsInSource(Type targetType)
		{
			BindingFlags methodBindingFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			foreach (MethodInfo methodInfo in targetType.GetMethods(methodBindingFlags))
			{
				APIMethodAttribute methodAttribute = methodInfo.GetCustomAttribute(typeof(APIMethodAttribute), false) as APIMethodAttribute;

				if (methodAttribute != null)
					yield return (methodAttribute, methodInfo);
			}
		}

		private static IEnumerable<Type> GetDiscoverableAPISources()
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				// Currently iterates over *all* assemblies, even though many couldn't possibly have our own custom attribute anyway.
				// This only needs to happen once and is pretty quick regardlessly.
				// Nonetheless, it might be a good idea to create a list of target assemblies, but serializing this is a bit involved.

				foreach (Type type in assembly.GetTypes())
				{
					// only includes classes marked as APISource
					if (type.GetCustomAttribute(typeof(APISourceAttribute), false) != null)
						yield return type;
				}
			}
		}

		public static void StoreDiscoverableAPIMethods(List<(APIMethodAttribute, MethodInfo)> store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			store.AddRange(GetAPIMethodsFromSources(GetDiscoverableAPISources()));
		}

		public static void StoreDiscoverableAPISources(List<Type> store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			store.AddRange(GetDiscoverableAPISources());
		}

		public static void StoreAPIMethodsForSource(Type source, List<(APIMethodAttribute, MethodInfo)> store)
		{
			if (store == null)
				throw new ArgumentNullException(nameof(store));

			if (source == null)
				throw new ArgumentNullException(nameof(source));

			store.AddRange(GetAPIMethodsInSource(source));
		}

		private static IEnumerable<MethodInfo> GetAPIInitializersInSource(Type targetType)
		{
			BindingFlags methodBindingFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			foreach (MethodInfo methodInfo in targetType.GetMethods(methodBindingFlags))
			{
				APIInitAttribute methodAttribute = methodInfo.GetCustomAttribute(typeof(APIInitAttribute), false) as APIInitAttribute;

				if (methodAttribute != null)
					yield return methodInfo;
			}
		}

		private static IEnumerable<MethodInfo> GetAPIResettersInSource(Type targetType)
		{
			BindingFlags methodBindingFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			foreach (MethodInfo methodInfo in targetType.GetMethods(methodBindingFlags))
			{
				APIResetAttribute methodAttribute = methodInfo.GetCustomAttribute(typeof(APIResetAttribute), false) as APIResetAttribute;

				if (methodAttribute != null)
					yield return methodInfo;
			}
		}

		public static void InitAPISources()
		{
			foreach (Type sourceType in GetDiscoverableAPISources())
			{
				foreach (MethodInfo initializer in GetAPIInitializersInSource(sourceType))
				{
					initializer.Invoke(null, null);
				}
			}
		}

		public static void ResetAPISources()
		{
			foreach (Type sourceType in GetDiscoverableAPISources())
			{
				foreach (MethodInfo resetter in GetAPIResettersInSource(sourceType))
				{
					resetter.Invoke(null, null);
				}
			}
		}



		public static void GetMatchingInstructions(string instructionString, IList<InstructionData> matchStorage, bool allowInternal = false)
		{
			if (instructionStringsLookup.ContainsKey(instructionString))
			{
				List<InstructionData> matches = instructionStringsLookup[instructionString];

				foreach (InstructionData match in matches)
				{
					if (!match.isInternal || allowInternal)
						matchStorage.Add(match);
				}
			}
		}

		public static bool IsInstructionString(string input, bool allowInternal = false)
		{
			if (instructionStringsLookup.ContainsKey(input))
			{
				List<InstructionData> matches = instructionStringsLookup[input];

				if (allowInternal && matches.Count > 0)
					return true;

				foreach (InstructionData match in matches)
				{
					if (!match.isInternal)
						return true;
				}
			}

			return false;
		}

		public static InstructionData GetInternalInstructionByID(string identifier)
		{
			if (internalIdentifierLookup.ContainsKey(identifier))
			{
				return internalIdentifierLookup[identifier];
			}
			else
			{
				throw new PinionAPIException($"No internal method linked to identifier '{identifier}'.");
			}
		}

		public static InstructionData GetInstructionData(ushort instructionCode, bool allowInternal = false)
		{
			if (instructionCode < firstCustomInstructionCode || instructionCode >= instructionLookUpTable.Length)
				return null;

			if (allowInternal)
			{
				return instructionLookUpTable[instructionCode];
			}
			else
			{
				InstructionData data = instructionLookUpTable[instructionCode];
				return data.isInternal ? null : data;
			}
		}
	}
}