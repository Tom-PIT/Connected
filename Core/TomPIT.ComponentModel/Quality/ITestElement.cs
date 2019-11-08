namespace TomPIT.ComponentModel.Quality
{
	public interface ITestElement : IElement
	{
		string Name { get; }
		bool Enabled { get; }

		TestErrorBehavior ErrorBehavior { get; }
	}
}
