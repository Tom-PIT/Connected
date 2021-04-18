namespace TomPIT.Design
{
	public interface IDiffDescriptor
	{
		string Original { get; }
		string Modified { get; }
		string Syntax { get; }
	}
}
