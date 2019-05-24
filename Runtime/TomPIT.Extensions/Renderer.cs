using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public class Renderer
	{
		private AttributesHelper _attributes = null;
		private IdeHelper _ide = null;
		private SnippetsHelper _snippets = null;
		private PartialHelper _partial = null;
		private UrlHelper _url = null;
		private JavascriptHelper _js = null;
		private SharedHelper _shared = null;
		private SystemHelper _sys = null;
		private ReportHelper _reports = null;

		internal Renderer(IHtmlHelper helper)
		{
			Html = helper;
		}

		private IHtmlHelper Html { get; }

		public JavascriptHelper JavaScript
		{
			get
			{
				if (_js == null)
					_js = new JavascriptHelper(Html);

				return _js;
			}
		}

		public AttributesHelper Attributes
		{
			get
			{
				if (_attributes == null)
					_attributes = new AttributesHelper(Html);

				return _attributes;
			}
		}

		public IdeHelper Ide
		{
			get
			{
				if (_ide == null)
					_ide = new IdeHelper(Html);

				return _ide;
			}
		}

		public SnippetsHelper Snippets
		{
			get
			{
				if (_snippets == null)
					_snippets = new SnippetsHelper(Html);

				return _snippets;
			}
		}

		public SystemHelper Sys
		{
			get
			{
				if (_sys == null)
					_sys = new SystemHelper(Html);

				return _sys;
			}
		}

		public PartialHelper Partial
		{
			get
			{
				if (_partial == null)
					_partial = new PartialHelper(Html);

				return _partial;
			}
		}

		public ReportHelper Reporting
		{
			get
			{
				if (_reports == null)
					_reports = new ReportHelper(Html);

				return _reports;
			}
		}

		public UrlHelper Url
		{
			get
			{
				if (_url == null)
					_url = new UrlHelper(Html);

				return _url;
			}
		}

		public SharedHelper Shared
		{
			get
			{
				if (_shared == null)
					_shared = new SharedHelper(Html);

				return _shared;
			}
		}
	}
}
