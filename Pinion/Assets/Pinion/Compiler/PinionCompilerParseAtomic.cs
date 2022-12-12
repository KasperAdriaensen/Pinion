using System;
using System.Collections;
using System.Collections.Generic;
using Pinion.Compiler.Internal;
using UnityEngine;

namespace Pinion.Compiler
{
	public partial class PinionCompiler
	{
		private static CompilerArgument ParseAtomicValue(PinionContainer targetContainer, string token, List<ushort> output)
		{
#if UNITY_EDITOR && PINION_COMPILE_DEBUG
			Debug.Log($"[PinionCompiler] Parsing atomic value: \'{token}\'");
#endif
			if (token.StartsWith(CompilerConstants.VariablePrefix)) // Expression is a variable identifier.
				return ParseVariableRead(targetContainer, token, output);

			// If no instruction keyword is involved and we are not dealing with a variable, we assume we are dealing with a literal and we do some extra processing.
			// The main problem here is that since the basis of this bytecode is the ushort, we have to be able to cram WHATEVER we might want to pass as a literal
			// into a ushort format. This works well for relatively small integer numbers, but gets complicated for fractional numbers, let alone strings.
			// There could be a workaround by conventionalizing something like the n next ushorts defining a fractional number, but that's cumbersome and probably error-prone.
			// It would be even more complicated for strings. 

			// Since the memory footprint of these sort of scripts should be small at best and we're not actually gonna do any repeated allocations, it makes more sense to give each
			// script container a number of small registers for literals of each supported primitive type, e.g. a register of 64 ints, 64 floats, 32 strings literals, etc.
			// We then define a "get literal of type" instruction per type, insert that into the bytecode, followed by an index at which the literal can be found. That index 
			// will *easily* fit into a ushort.

			return ParseLiteral(targetContainer, token, output);
		}
	}
}