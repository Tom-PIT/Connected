namespace TomPIT.Design
{
	public interface ITextDiffDescriptor
	{
		TextDiffOperation Operation { get; set; }
		string Text { get; set; }
	}
}
