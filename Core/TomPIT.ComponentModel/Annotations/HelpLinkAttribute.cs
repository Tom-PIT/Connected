using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Property)]
	public class HelpLinkAttribute : Attribute
	{
		public string Url { get; private set; }

		public HelpLinkAttribute()
		{

		}

		public HelpLinkAttribute(string url)
		{
			Url = url;
		}
	}
}
