namespace TomPIT.Ide
{
	public abstract class EnvironmentPanel
	{
		protected EnvironmentPanel(IEnvironment environment)
		{
			Environment = environment;
		}

		public IEnvironment Environment { get; private set; }
	}
}
