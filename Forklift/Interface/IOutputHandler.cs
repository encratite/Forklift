namespace Forklift
{
	interface IOutputHandler
	{
		void WriteLine(string message, params object[] arguments);
	}
}
