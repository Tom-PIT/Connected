namespace TomPIT.Design.Tools
{
	public interface IAutoFixProvider
	{
		string Name { get; }
		bool CanFix(object sender, AutoFixArgs e);

		void Fix(object sender, AutoFixArgs e);
	}
}
