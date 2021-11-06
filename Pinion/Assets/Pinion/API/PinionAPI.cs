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

namespace Pinion
{
	public static partial class PinionAPI
	{
		/* NOMENCLATURE =====================================
		In the code below, there's quite a bit of switching between representing the instruction as an enum value, string, ushort...
		For clarity's sake, the code below aims to use this naming as consistently as possible:
		* enum value 	-> "instruction"
		* ushort 		-> "instruction code" / "code"
		* string 		-> "instruction string" / "string"
		*/

		public const int MaxParameterCount = 16;
		private const ushort firstCustomInstructionCode = 16; // Leave some unoccupied for safety/futureproofing.
		private static bool alreadyBuilt = false;
		private static bool buildSuccess = false;

		private static InstructionData[] instructionLookUpTable = null;
		private static Dictionary<string, List<InstructionData>> instructionStringsLookup = new Dictionary<string, List<InstructionData>>();
		private static Dictionary<string, InstructionData> internalIdentifierLookup = new Dictionary<string, InstructionData>();
		private static object[] instructionParametersReuse = new object[MaxParameterCount];


		public const string InternalIDReadInt = "InternalReadInt";
		public const string InternalIDReadFloat = "InternalReadFloat";
		public const string InternalIDReadBool = "InternalReadBool";
		public const string InternalIDReadString = "InternalReadString";
		public const string InternalIDWriteInt = "InternalWriteInt";
		public const string InternalIDWriteFloat = "InternalWriteFloat";
		public const string InternalIDWriteBool = "InternalWriteBool";
		public const string InternalIDWriteString = "InternalWriteString";
		public const string InternalIDReadLabel = "InternalReadLabel";
		public const string InternalIDIfFalseGoTo = "InternalIfFalseGoTo";
		public const string InternalIDGoTo = "InternalGoTo";
		public const string InternalIDInitBegin = "InternalIDInitBegin";
		public const string InternalIDInitEnd = "InternalIDInitEnd";
		// Currently not used, easily restored.
		// public const string InternalIDMainBegin = "InternalIDMainBegin";
		// public const string InternalIDMainEnd = "InternalIDMainEnd";

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

		private static HashSet<System.Type> supportedTypes = new HashSet<System.Type>
		{
			typeof(int),
			typeof(float),
			typeof(bool),
			typeof(string),
		};

		private static IEnumerable<MethodInfo> GetAllAPISourceMethods()
		{
			BindingFlags methodBindingFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				// This could be expanded or made less restrictive if need be.
				// For our current purposes however, this is perfectly fine. Saves iterating over a whole bunch of types that couldn't possibly have our own custom attribute anyway.
				if (assembly.GetName().Name != "Assembly-CSharp")
					continue;

				foreach (Type type in assembly.GetTypes())
				{
					// only includes classes marked as APISource
					if (type.GetCustomAttribute(typeof(APISourceAttribute), false) == null)
						continue;

					foreach (MethodInfo methodInfo in type.GetMethods(methodBindingFlags))
					{
						yield return methodInfo;
					}
				}
			}
		}

		public static bool BuildAPI(System.Action<string> errorMessageReceiver)
		{
			if (alreadyBuilt)
				return buildSuccess;

			instructionStringsLookup.Clear();
			internalIdentifierLookup.Clear();

			// Returns all methods from functions marked with the APISource attribute.
			IEnumerable<MethodInfo> allMethodsInAPISources = GetAllAPISourceMethods();

			bool overallSuccess = true;
			ushort currentInstructionCode = firstCustomInstructionCode;
			List<InstructionData> builtInstructions = new List<InstructionData>();

			foreach (MethodInfo methodInSource in allMethodsInAPISources)
			{
				// if we've failed to build an instruction, no point continuing with the rest
				if (overallSuccess == false)
					break;

				// NOTE: we could also adapt GetAllAPIFunctions() to only return functions marked with the APIFunction attribute.
				// But we need access to data inside that attribute anyway, so we might as well do it here.
				APIMethodAttribute methodAttribute = methodInSource.GetCustomAttribute(typeof(APIMethodAttribute), false) as APIMethodAttribute;

				if (methodAttribute == null) // not an API method
					continue;

				string instructionString = methodInSource.Name;
				InstructionData instructionData = null;

				try
				{
					// See note above reservedInstructionStrings.
					if (reservedInstructionStrings.Contains(instructionString))
						throw new PinionAPIException($"'{instructionString}' is a reserved instruction string. Cannot define an API call with this name.");

					instructionData = new InstructionData(currentInstructionCode, methodInSource, methodAttribute.HasFlag(APIMethodFlags.Internal));

					if (instructionData == null)
					{
						overallSuccess = false; // Let's not break the loop yet, so we can still display other errors, should there be any.
						continue;
					}

					// Several instructions can share the same instruction string, but with a different parameter signature (overloads). 
					// This means one string can have multiple potential matches. Here we build a Dictionary<string, InstructionaData> to reflect this 1-to-n relationship.
					// NOTE: Resolving string match to the right signature *ONLY* happens at compile time. 

					if (instructionStringsLookup.ContainsKey(instructionString))
					{
						List<InstructionData> lookupList = instructionStringsLookup[instructionString];
						bool currentInstructionInternal = instructionData.internalInstruction;

						foreach (InstructionData data in lookupList)
						{
							// Silly check: we'd rather not have implementers overload internal instructions. (Not strictly a problem, but then you could also overload it with the same arguments!)
							// Of course, we don't know whether the interal or the normal function will be encountered first.
							// Therefore, we say that either all internal or all normal is fine, just not a mix of the two.
							if (data.internalInstruction != currentInstructionInternal)
							{
								overallSuccess = false;
								throw new PinionAPIException($"Instruction '{methodInSource.Name}' could not be generated. The name '{methodInSource.Name}' is reserved for an internal instruction.");
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
							methodInSource.GetCustomAttribute(typeof(APIInternalMethodIdentifierAttribute), false) as APIInternalMethodIdentifierAttribute;

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
								throw new PinionAPIException($"Internal methods {internalIdentifierLookup[identifier].instructionString} and {methodInSource.Name} are assigned the same identifier ({identifier})!");
							}
						}
					}

					builtInstructions.Add(instructionData);
					currentInstructionCode++;
				}
				catch (System.Exception exception)
				{
#if UNITY_EDITOR
					Debug.LogException(exception);
#else
					if (errorMessageReceiver != null)
						errorMessageReceiver($"Exception occurred while building API function {instructionType}. Message: {exception.Message}");
#endif
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

			instructionLookUpTable = new InstructionData[firstCustomInstructionCode + builtInstructions.Count];

			for (int i = 0; i < builtInstructions.Count; i++)
			{
				instructionLookUpTable[firstCustomInstructionCode + i] = builtInstructions[i];
			}

			buildSuccess = overallSuccess;

			Debug.Log($"API built succesfully. {builtInstructions.Count} API functions discovered.");
			return overallSuccess;
		}

		public static void CallAPIInstruction(ushort instructionCode, PinionContainer callingContainer)
		{
			// We do not check whether the passed instruction code is within the bounds of instructionLookupTable, because that would be wasted effort.
			// If it is invalid - compilation did not do it's just job correctly.
			InstructionData compileData = instructionLookUpTable[instructionCode];

#if UNITY_EDITOR && PINION_COMPILE_DEBUG

			string signature = string.Empty;
			for (int i = 0; i < compileData.exposedParameterCount; i++)
			{
				signature += compileData.GetParameterType(i).ToString();

				if (i < compileData.exposedParameterCount - 1)
					signature += ",";
			}

			Debug.Log($"Calling: {compileData.instructionString}({signature}).");
#endif

			if (compileData.requiresContainer)
			{
				instructionParametersReuse[0] = callingContainer; // the container is never on the stack

				// Arguments are on the stack in "reverse" order. First popped argument is the last one!
				// start from compileData.parameterCount -> first index is (skipped index 0) + (compileData.parameterCount-1)
				// iterate while i > 0 because index 0 is already filled in!
				for (int i = compileData.exposedParameterCount; i > 0; i--)
				{
					instructionParametersReuse[i] = callingContainer.PopFromStack();
				}
			}
			else
			{
				// Arguments are on the stack in "reverse" order. First popped argument is the last one!
				// start from compileData.parameterCount-1 -> for four arguments, start at index 3
				// iterate while i >= 0
				for (int i = compileData.exposedParameterCount - 1; i >= 0; i--)
				{
					instructionParametersReuse[i] = callingContainer.PopFromStack();
				}
			}

			// The system to create delegates requires us to work with an object[], not something something more flexible like List<object>.
			// However, since the delegate creation logic also hardcodes the number of parameters to read, we don't need to worry about passing the correct number of arguments.
			// Barring unforeseen bugs, it can only ever read the correct indices/amount. Indices beyond that are ignored.
			compileData.Call(callingContainer, instructionParametersReuse);
		}

		public static void GetMatchingInstructions(string instructionString, IList<InstructionData> matchStorage, bool allowInternal = false)
		{
			if (instructionStringsLookup.ContainsKey(instructionString))
			{
				List<InstructionData> matches = instructionStringsLookup[instructionString];

				foreach (InstructionData match in matches)
				{
					if (!match.internalInstruction || allowInternal)
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
					if (!match.internalInstruction)
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
				return data.internalInstruction ? null : data;
			}
		}

		// To be used at script runtime, when compilation should already have ensured any referenced instructions exist and that 
		// any internal instructions are only being called by the system itself.
		// Any safety checks are therefore wasted performance.
		public static InstructionData GetInstructionFast(ushort instructionCode)
		{
			return instructionLookUpTable[instructionCode];
		}

		public static bool IsSupportedType(Type type)
		{
			return supportedTypes.Contains(type);
		}
	}
}