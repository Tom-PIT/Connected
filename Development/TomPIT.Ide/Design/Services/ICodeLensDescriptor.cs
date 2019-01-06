namespace TomPIT.Design.Services
{
	public interface ICodeLensDescriptor
	{
		IRange Range { get; }
		string Id { get; }
		ICodeLensCommand Command { get; }
	}
}
