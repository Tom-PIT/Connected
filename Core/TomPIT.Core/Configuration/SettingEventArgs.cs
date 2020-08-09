using System;

namespace TomPIT.Configuration
{
	public class SettingEventArgs : EventArgs
	{
		public SettingEventArgs()
		{

		}
		public SettingEventArgs(string name, string nameSpace, string type, string primaryKey)
		{
			Name = name;
			Type = type;
			PrimaryKey = primaryKey;
			NameSpace = nameSpace;
		}

		public string Name { get; set; }
		public string Type { get; set; }
		public string PrimaryKey { get; set; }
		public string NameSpace { get; set; }
	}
}
