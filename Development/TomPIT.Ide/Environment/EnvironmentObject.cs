namespace TomPIT.Ide.Environment
{
	public class EnvironmentObject : IEnvironmentObject
	{
		public EnvironmentObject(IEnvironment environment)
		{
			Environment = environment;
		}

		public IEnvironment Environment { get; }
	}
}
