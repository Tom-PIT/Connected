namespace TomPIT.Ide.TextServices.Languages
{
	public enum DeltaDecorationRulerPosition
	{
		Left = 1,
		Center = 2,
		Right = 4,
		Full = 7
	}
	public interface IDeltaDecorationOverviewRuler
	{
		string Color { get; }
		string DarkColor { get; }
		DeltaDecorationRulerPosition Position { get; }
	}
}
