namespace TomPIT.ComponentModel.Features
{
	public interface IFeature : IElement
	{
		string Name { get; }
		bool Enabled { get; }
	}
}
