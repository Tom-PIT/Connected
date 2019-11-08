using System;

namespace TomPIT.Annotations.Design
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
