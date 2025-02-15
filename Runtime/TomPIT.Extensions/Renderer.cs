﻿using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Middleware;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT
{
	public class Renderer
	{
		private AttributesHelper _attributes = null;
		private IdeHelper _ide = null;
		private PartialHelper _partial = null;
		private UrlHelper _url = null;
		private JavascriptHelper _js = null;
		private SharedHelper _shared = null;
		private SystemHelper _sys = null;
		private ReportHelper _reports = null;
		private SearchResultsHelper _searchResults = null;
		private IoCHelper _ioc = null;
		private StringProcessorHelper _stringProcessor = null;

		internal Renderer(IHtmlHelper helper)
		{
			Html = helper;
		}

		private IHtmlHelper Html { get; }

		public SearchResultsHelper Search
		{
			get
			{
				if (_searchResults == null)
					_searchResults = new SearchResultsHelper(Html);

				return _searchResults;
			}
		}

		public IoCHelper IoC
		{
			get
			{
				if (_ioc == null)
					_ioc = new IoCHelper(Html);

				return _ioc;
			}
		}

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

		public StringProcessorHelper Strings 
		{
			get 
			{
				if (_stringProcessor == null) 
					_stringProcessor = new StringProcessorHelper(Html);

				return _stringProcessor;
			}
		}

		[Obsolete("Please use Api Property instead.")]
		public IHtmlContent Property([CIP(CIP.ApiOperationProvider)] string api, [CIP(CIP.ApiOperationParameterProvider)] string property)
		{
			return ApiProperty(api, property);
		}

		public IHtmlContent ApiProperty([CIP(CIP.ApiOperationProvider)] string api, [CIP(CIP.ApiOperationParameterProvider)] string property)
		{
			var result = new ApiPropertyRenderer(Html.ViewData.Model as IMiddlewareContext, api, property).Result;

			if (result == null)
				return null;

			return Html.Raw(result);
		}

		public IHtmlContent SettingProperty([CIP(CIP.SettingMiddlewareProvider)] string middleware, [CIP(CIP.SettingMiddlewareParameterProvider)] string property)
		{
			var result = new SettingPropertyRenderer(Html.ViewData.Model as IMiddlewareContext, middleware, property).Result;

			if (result == null)
				return null;

			return Html.Raw(result);
		}
	}
}
