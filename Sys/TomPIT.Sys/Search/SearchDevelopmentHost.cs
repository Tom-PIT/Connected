﻿using TomPIT.Sys.Diagnostics;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Search
{
	internal class SearchDevelopmentHost : SearchHost<DevelopmentSearchMessage>
	{
		private string _searchDirectory = null;

		public override string FileName => "Development";
		protected override string SearchDirectory
		{
			get
			{
				if (_searchDirectory == null)
				{
					_searchDirectory = string.Empty;

					var setting = DataModel.Settings.Select("Development Search Path", null, null, null);

					if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
					{
						LogError(SysLogEvents.SearchDirectoryInit, SR.ErrSysDevSearchPath);

						return _searchDirectory;
					}

					_searchDirectory = setting.Value;

					if (System.IO.Directory.Exists(_searchDirectory))
					{
						LogError(SysLogEvents.SearchDirectoryInit, $"{SR.ErrSysSearchPathNotExist} ({_searchDirectory})");

						_searchDirectory = string.Empty;
					}
				}

				return _searchDirectory;
			}
		}

		protected override void OnComplete(DevelopmentSearchMessage message)
		{

		}
	}
}
