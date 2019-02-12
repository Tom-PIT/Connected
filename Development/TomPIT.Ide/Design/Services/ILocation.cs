namespace TomPIT.Design.Services
{
	public interface ILocation
	{
		IRange Range { get; }
		string Uri { get; }
	}
}
