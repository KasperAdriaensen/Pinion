namespace Pinion.Compiler.Internal
{
	public interface IRewriteHandler
	{
		bool AttemptRewrite(string inputLine, out string outputLine, int lineNumber);
		void Reset();
		void CheckValidity(System.Action<string, int> errorMessageHandler);
	}

}