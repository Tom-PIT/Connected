﻿using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{
	public interface ICompilerNotification
	{
		void NotifyChanged(object sender, ScriptChangedEventArgs e);
	}
}