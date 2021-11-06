using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIInternalFlowControl
	{
		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDReadLabel)]
		public static void ReadLabel(PinionContainer container)
		{
			int value = container.LabelRegister.ReadValue(container.AdvanceToNextInstruction());
			container.PushJumpLocation(value);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDIfFalseGoTo)]
		public static void IfFalseGoTo(PinionContainer container, bool condition)
		{
			int jumpLocation = container.PopJumpLocation();

			// if condition was false, skip over to next jump target
			// not reusing the other GoTo method because the jump location needs to be popped either way
			if (condition == false)
				container.SetNextInstructionIndex(jumpLocation);
		}

		[APIMethod(MethodFlags = APIMethodFlags.Internal)]
		[APIInternalMethodIdentifier(PinionAPI.InternalIDGoTo)]
		public static void GoTo(PinionContainer container)
		{
			int jumpLocation = container.PopJumpLocation();
			container.SetNextInstructionIndex(jumpLocation);
		}
	}
}