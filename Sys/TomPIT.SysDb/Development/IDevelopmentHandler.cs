﻿namespace TomPIT.SysDb.Development
{
	public interface IDevelopmentHandler
	{
		IMicroServiceHandler MicroServices { get; }
		IFolderHandler Folders { get; }
		IComponentHandler Components { get; }
		IQaHandler QA { get; }
		IVersionControlHandler VersionControl { get; }
		IDevelopmentErrorHandler Errors { get; }
		IToolsHandler Tools { get; }
	}
}
