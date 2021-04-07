namespace TomPIT.Ide.TextServices.Languages
{
	public enum MinimapPosition
	{
		Inline = 1,
		Gutter = 2
	}
	public interface IDeltaDecorationMinimap
	{
		string Color { get; }
		string DarkColor { get; }
		MinimapPosition Position { get; }
	}
}
