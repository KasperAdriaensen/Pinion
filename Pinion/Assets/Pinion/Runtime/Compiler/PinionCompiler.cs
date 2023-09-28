using UnityEngine;
using System.Text.RegularExpressions;
using System;
using Pinion.Compiler.Internal;
using System.IO;
using Pinion.Internal;

namespace Pinion.Compiler
{
	public static partial class PinionCompiler
	{
		private static bool apiBuiltSuccesfully = true;

		private enum ScriptBlock
		{
			None = 0,
			InitBlock = 1,
			MainBlock = 2,
			FunctionBlock = 3,
			MetaBlock = 4
		}

		private static System.Action<string> compileErrorCallback = null;
		private static bool compileSuccess = true;
		private static int currentLineNumber = -1; // 0 while not compiling - actual line numbers start at 1
		private static Regex variableDeclareRegex = new Regex(CompilerRegex.variableDeclareRegex, RegexOptions.Compiled);
		private static Regex variableWriteRegex = new Regex(CompilerRegex.variableWriteRegex, RegexOptions.Compiled);
		private static ScriptBlock currentBlockContext = ScriptBlock.None;

		static PinionCompiler()
		{
			// BuildAPI internally builds the lookup table (instruction ushort -> API call).
			// This one does send any errors directly to Unity console - this is before UI/frontend/whatever can pass an error handler anyway
			apiBuiltSuccesfully = PinionAPI.BuildAPI(Debug.LogError);
			BuildTokenSplitRegex();
		}

		public static PinionContainer Compile(string script, System.Action<string> errorMessageReceiver = null)
		{
			return CompileInternal<PinionContainer>(script, errorMessageReceiver);
		}

		public static PinionContainer CompileForEditor(Type containerType, string script, System.Action<string> errorMessageReceiver = null)
		{
			if (Application.isPlaying)
			{
				throw new NotSupportedException("Creating a PinionContainer with type parameter is not permitted at runtime. Use the generic method overload instead.");
			}

			if (!(typeof(PinionContainer)).IsAssignableFrom(containerType))
			{
				throw new ArgumentException("Container type must inherit from PinionContainer.", nameof(containerType));
			}

			PinionContainer container = (PinionContainer)Activator.CreateInstance(containerType);
			return CompileInternal<PinionContainer>(script, errorMessageReceiver, container);
		}

		public static T Compile<T>(string script, System.Action<string> errorMessageReceiver = null) where T : PinionContainer, new()
		{
			return CompileInternal<T>(script, errorMessageReceiver);
		}

		private static T CompileInternal<T>(string script, System.Action<string> errorMessageReceiver = null, T targetContainer = null) where T : PinionContainer, new()
		{
			compileErrorCallback = errorMessageReceiver;

			variableNameToPointerMappings.Clear();
			labelToIndexInLabelList.Clear();
			compileSuccess = true;

			if (!apiBuiltSuccesfully) // show message when API failed to compile - break immediately 
			{
				AddCompileError("Cannot compile due to internal failure to compile Pinion API. Check error log for details.", false);
				return null;
			}

			// Meta block contains free-form information. If present, it is passed to the script container of type T.
			// Type T is free to decide how to use this data (e.g. parse it for initialization).
			string metaBlock = ReadMetaBlock(script);

			if (!compileSuccess) // Parsing meta block could spawn an error. Don't bother beyond this point.
				return null;

			T newContainer = (targetContainer != null) ? targetContainer : new T();

			if (metaBlock != null) // A meta-block was present, whatever its contents may be.
				newContainer.ParseMetaData(metaBlock, AddCompileError);

			try
			{
				ParseScript(newContainer, script);
			}
			catch (Exception exception)
			{
				AddCompileException(exception);
			}

			compileErrorCallback = null;
			currentBlockContext = ScriptBlock.None;
			newContainer.GenerateStackWrappers();

			if (compileSuccess)
			{
				return newContainer;
			}
			else
			{
				return null;
			}
		}

		private static string ReadMetaBlock(string scriptIn)
		{
			string metaBlock = null;
			MatchCollection metaBlockMatches = Regex.Matches(scriptIn, CompilerRegex.metaBlockRegex, RegexOptions.Singleline); // single line flag makes . match newlines

			if (metaBlockMatches != null && metaBlockMatches.Count > 0)
			{
				if (metaBlockMatches.Count > 1)
				{
					AddCompileError("Multiple meta blocks detected. This is not allowed", false);
					return null;
				}

				Match metaBlockMatch = metaBlockMatches[0];
				metaBlock = metaBlockMatch.Groups[1].Value;
			}

			return metaBlock;
		}

		private static void ParseScript(PinionContainer targetContainer, string script)
		{
			currentLineNumber = 0;

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("Script original state: " + System.Environment.NewLine + script);
#endif

			// Removes comments.
			script = CompilerRewriting.RemoveComments(script);

			// Inserts line number tags that allow us to refer to correct line numbers during 
			// compilation, despite any other rewrites that might add/remove lines.
			// Needs to happen in a separate pass, before other rewrites!
			// Better way to handle this?
			script = CompilerRewriting.InsertSourceLineNumbers(script);


#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("Script after removing comments and inserting line numbers: " + System.Environment.NewLine + script);
#endif

			// Removes any whitespace.
			// This significantly simplifies any other pattern matching further along.
			script = CompilerRewriting.RemoveWhitespace(script);

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("Script after removing whitespace: " + System.Environment.NewLine + script);
#endif

			// Converts typical flow control keywords to goto label structure.
			script = CompilerRewriting.RewriteFlow(script, AddCompileError);

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log("Script after rewriting flow control: " + System.Environment.NewLine + script);
#endif

			if (!compileSuccess) // malformed flow control above could be enough to break compilation
				return;

			currentBlockContext = ScriptBlock.None;
			bool initBlockPresent = false;
			bool mainBlockPresent = false;
			bool metaBlockPresent = false;
			bool blocksPresent = false;
			bool implicitMainPresent = false;
			//bool functionsPresent = false;


			using (StringReader reader = new StringReader(script))
			{
				string line = string.Empty;

				do
				{
					line = reader.ReadLine();

					if (string.IsNullOrEmpty(line)) // Ignore end of file and blank lines, if they still exist
						continue;

					if (LineNumberUpdate(line, out line))
					{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
						Debug.LogFormat($"[PinionCompiler] ENTERED LINE {currentLineNumber} -------------------------------------------------");
#endif
						continue;
					}

#if UNITY_EDITOR && PINION_COMPILE_DEBUG
					Debug.Log($"[PinionCompiler] LINE: {line}");
#endif

					if (line == CompilerConstants.MetaBeginMarker)
					{
						if (currentBlockContext != ScriptBlock.None)
							AddCompileError($"Cannot use {CompilerConstants.MetaBeginMarker} inside {currentBlockContext}.");

						if (metaBlockPresent)
							AddCompileError($"Cannot have more than one {ScriptBlock.MetaBlock}.");
						else if (targetContainer.scriptInstructions.Count > 0 || blocksPresent)
							AddCompileError($"{CompilerConstants.MetaBeginMarker} must be at the start of the script. It cannot occur in any other context.");

						metaBlockPresent = true;
						currentBlockContext = ScriptBlock.MetaBlock;
						continue;
					}
					else if (line == CompilerConstants.MetaEndMarker)
					{
						if (currentBlockContext != ScriptBlock.MetaBlock)
							AddCompileError($"Cannot use {CompilerConstants.MetaEndMarker} inside {currentBlockContext}.");

						currentBlockContext = ScriptBlock.None;
						continue;
					}

					if (line == CompilerConstants.InitBeginMarker)
					{
						if (currentBlockContext != ScriptBlock.None)
							AddCompileError($"Cannot use {CompilerConstants.InitBeginMarker} inside {currentBlockContext}.");

						if (initBlockPresent)
							AddCompileError($"Cannot have more than one {ScriptBlock.InitBlock}.");
						else if (targetContainer.scriptInstructions.Count > 0 || blocksPresent)
							AddCompileError($"{CompilerConstants.InitBeginMarker} must be at the start of the script, below the meta block (if present). It cannot occur in any other context.");

						if (implicitMainPresent)
							AddCompileError($"Cannot start an {ScriptBlock.InitBlock} when there is an implicit {ScriptBlock.MainBlock} (code outside any block).");

						initBlockPresent = true;
						currentBlockContext = ScriptBlock.InitBlock;

						// Event that enables calling custom logic on the script container on init start.
						targetContainer.scriptInstructions.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.InitBegin).instructionCode);
						continue;
					}
					else if (line == CompilerConstants.InitEndMarker)
					{
						if (currentBlockContext != ScriptBlock.InitBlock)
							AddCompileError($"Cannot use {CompilerConstants.InitEndMarker} inside {currentBlockContext}.");

						currentBlockContext = ScriptBlock.None;

						// Event that enables calling custom logic on the script container on init end.
						targetContainer.scriptInstructions.Add(PinionAPI.GetInternalInstructionByID(PinionAPIInternalIDs.InitEnd).instructionCode);
						continue;
					}

					if (line == CompilerConstants.MainBeginMarker)
					{
						if (currentBlockContext != ScriptBlock.None)
							AddCompileError($"Cannot use {CompilerConstants.MainBeginMarker} inside {currentBlockContext}.");

						if (mainBlockPresent)
							AddCompileError($"Cannot have more than one {ScriptBlock.MainBlock}.");

						if (implicitMainPresent)
							AddCompileError($"Cannot start a {ScriptBlock.MainBlock} when there is an implicit {ScriptBlock.MainBlock} (code outside any block).");

						mainBlockPresent = true;
						currentBlockContext = ScriptBlock.MainBlock;
						targetContainer.mainBlockStartIndex = targetContainer.scriptInstructions.Count;
						continue;
					}
					else if (line == CompilerConstants.MainEndMarker)
					{
						if (currentBlockContext != ScriptBlock.MainBlock)
							AddCompileError($"Cannot use {CompilerConstants.InitEndMarker} inside {currentBlockContext}.");

						currentBlockContext = ScriptBlock.None;
						continue;
					}

					// TODO
					// if (line == CompilerConstants.FunctionBeginMarker)
					// {
					// 	if (compilerState != CompilerState.None)
					// 		AddCompileError($"Cannot use {CompilerConstants.FunctionBeginMarker} inside {compilerState}.");

					// 	functionsPresent = true;
					// 	compilerState = CompilerState.FunctionBlock;
					// 	continue;
					// }
					// else if (line == CompilerConstants.FunctionEndMarker)
					// {
					// 	if (compilerState != CompilerState.FunctionBlock)
					// 		AddCompileError($"Cannot use {CompilerConstants.FunctionEndMarker} inside {compilerState}.");

					// 	compilerState = CompilerState.None;
					// 	continue;
					// }


					// NOTE: meta block is not counted for this: it's "invisible" to itself and other blocks.
					blocksPresent = initBlockPresent || mainBlockPresent;

					switch (currentBlockContext)
					{
						case ScriptBlock.MainBlock:
							ParseLineForMain(targetContainer, line);
							break;

						case ScriptBlock.InitBlock:
							ParseLineForInit(targetContainer, line);
							break;

						case ScriptBlock.None:
							if (blocksPresent)
							{
								AddCompileError("Cannot make an implicit main block when other blocks are present.");
								break;
							}
							else
							{
								implicitMainPresent = true;
								ParseLineForMain(targetContainer, line);
							}
							break;

							// NOTE: meta block is not parsed at all here, it's been interpreted before compilation even began
							// In fact, we could remove the metablock entirely before this point, but that would create complications (e.g. for line number references etc.)

							// TODO
							// case CompilerState.FunctionBlock:
							// 	ParseLineForMain(targetContainer, line); // Same logic as main, differences are on a different level.
							// 	break;
					}
				}
				while (line != null && compileSuccess); // break compilation prematurely if error was thrown
			}

			if (compileSuccess) // Some final checks if everything else succeeded.
			{
				if (currentBlockContext != ScriptBlock.None)
					AddCompileError($"Block {currentBlockContext} was opened, but never closed.");

				if (initBlockPresent && !mainBlockPresent)
					AddCompileError($"A main block was not defined, but other blocks were. An implicit main block is only allowed if there are no other blocks, except for a meta block.");
			}


#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			OutputFullInstructionList(targetContainer);
#endif
		}

		private static void ParseLineForInit(PinionContainer targetContainer, string line)
		{
			// Match match = variableDeclareRegex.Match(line);
			// if (match.Success)
			// {
			// 	ParseVariableDeclaration(targetContainer, match.Groups[1].Value);
			// }
			// else
			// {
			// 	AddCompileError($"Only variable declarations are allowed in {ScriptBlock.InitBlock}. Invalid content: {line}");
			// }

			// FIXME This is untested and likely dangerous!
			ParseLineForMain(targetContainer, line);
		}

		private static void ParseLineForMain(PinionContainer targetContainer, string line)
		{
			Match match;

			match = variableDeclareRegex.Match(line);
			if (match.Success)
			{
				ParseVariableDeclaration(targetContainer, match.Groups[1].Value);
				return;
			}

			match = variableWriteRegex.Match(line);
			if (match.Success)
			{
				ParseVariableAssign(targetContainer, match.Groups[1].Value);
				return;
			}

			if (line.StartsWith(CompilerConstants.LabelCreatePrefixComplete))
			{
				ParseLabelCreation(targetContainer, line, currentLineNumber);
			}
			else if (line.StartsWith(CompilerConstants.ConditionLabelJump))
			{
				ParseConditionalLabelJump(targetContainer, line);
			}
			else if (line.StartsWith(CompilerConstants.LabelJump))
			{
				ParseLabelJump(targetContainer, line);
			}
			else
			{
				ParseExpression(targetContainer, line);
			}
		}

		private static bool LineNumberUpdate(string line, out string resultLine)
		{
			// See note above InsertSourceLineNumbers above.
			Match lineNumberMatch = Regex.Match(line, CompilerRegex.lineNumberRegex);
			if (lineNumberMatch.Success)
			{
				currentLineNumber = int.Parse(lineNumberMatch.Groups[1].Value); // update "line number"
				resultLine = line.Substring(lineNumberMatch.Value.Length); // remove line number tag from line
				return true;
			}
			else
			{
				resultLine = line;
				return false;
			}
		}

		private static void AddCompileException(Exception exception)
		{
			AddCompileError($"Internal compiler exception. Check log file for details.");
			Debug.LogException(exception);
		}

		private static void AddCompileError(string message) // just here because the default argument makes use as a delegate hard
		{
			AddCompileError(message, true);
		}


		private static void AddCompileError(string message, bool includeLine = true)
		{
			if (includeLine)
			{
				AddCompileError(message, currentLineNumber);
			}
			else
			{
				AddCompileErrorNoLineNumber(message);
			}
		}

		private static void AddCompileError(string message, int customLineNumber)
		{
			message = $"[Line {customLineNumber}] {message}";
			AddCompileErrorNoLineNumber(message);
		}

		private static void AddCompileErrorNoLineNumber(string message)
		{
			// Defined from PinionSettings

#if PINION_LOG_COMPILE_ERRORS_ALWAYS || (PINION_LOG_COMPILE_ERRORS_EDITOR && UNITY_EDITOR)
			Debug.LogError($"[PinionCompiler] Compile error: {message}");
#endif

			compileSuccess = false;

			if (compileErrorCallback != null)
			{
				compileErrorCallback(message);
			}
		}
	}
}
