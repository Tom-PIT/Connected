using TomPIT.ComponentModel;

namespace TomPIT.Compilers
{
	public interface ICompilerNotification
	{
		void NotifyChanged(object sender, ScriptChangedEventArgs e);
	}
}
