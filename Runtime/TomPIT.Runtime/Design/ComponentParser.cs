using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Development;
using TomPIT.Reflection;
using TomPIT.Storage;

namespace TomPIT.Design
{
	internal class ComponentParser : TenantObject
	{
		private ChangeComponent _result = null;
		private IConfiguration _configuration = null;
		private IComponentImage _image = null;
		private ICommit _commit = null;
		private IComponentHistory _componentHistory = null;
		public ComponentParser(ITenant tenant, IComponent component, ChangeQueryMode mode, Guid commit) : base(tenant)
		{
			Component = component;
			Mode = mode;
			CommitToken = commit;
		}

		private Guid CommitToken { get; }
		private ChangeQueryMode Mode { get; }
		private IComponent Component { get; }

		private ICommit Commit
		{
			get
			{
				if (_commit == null && CommitToken != Guid.Empty)
					_commit = Tenant.GetService<IDesignService>().VersionControl.SelectCommit(CommitToken);

				return _commit;
			}
		}

		private IComponentHistory ComponentHistory
		{
			get
			{
				if(_componentHistory == null && Commit != null)
					_componentHistory = Tenant.GetService<IDesignService>().VersionControl.SelectCommitDetail(Commit.Token, Component.Token);

				return _componentHistory;
			}
		}
		private IComponentImage Image
		{
			get
			{
				if(_image == null)
				{
					if (ComponentHistory != null)
						_image = Tenant.GetService<IDesignService>().Components.SelectComponentImage(ComponentHistory.Blob);
				}

				return _image;
			}
		}
		private ChangeComponent Result
		{
			get
			{
				if (_result == null)
					_result = new ChangeComponent();

				return _result;
			}
		}

		private IConfiguration Configuration
		{
			get
			{
				if (_configuration == null && string.IsNullOrWhiteSpace(Result.Error))
				{
					try
					{
						_configuration = Tenant.GetService<IComponentService>().SelectConfiguration(Component.Token);
					}
					catch (Exception ex)
					{
						Result.Error = ex.Message;
					}
				}

				return _configuration;
			}
		}


		public ChangeComponent Parse()
		{
			Result.Folder = Component.Folder;
			Result.Id = Component.Token;
			Result.Microservice = Component.MicroService;
			Result.Name = Component.Name;
			Result.HasChanged = HasChanged(Component.Token, Guid.Empty);
			Result.Category = Component.Category;

			switch (Component.LockVerb)
			{
				case LockVerb.Add:
					Result.Verb = ComponentVerb.Add;
					break;
				case LockVerb.Edit:
					Result.Verb = ComponentVerb.Edit;
					break;
				case LockVerb.Delete:
					Result.Verb = ComponentVerb.Delete;
					break;
			}

			if (Result.Verb == ComponentVerb.Delete)
				return Result;

			ParseConfiguration();
			ParseElements();

			return Result;
		}

		private void ParseConfiguration()
		{
			if (Configuration == null)
				return;

			if (Configuration is IText text)
			{
				Result.Blob.Token = text.TextBlob;
				Result.Blob.Syntax = ResolveSyntax(text);
				Result.Blob.FileName = ResolveFileName(text);

				if (text.TextBlob != Guid.Empty)
					Result.Blob.HasChanged = HasChanged(Configuration.Component, text.TextBlob);

				if (Mode == ChangeQueryMode.Full)
				{
					var content = Tenant.GetService<IComponentService>().SelectText(Result.Microservice, text);

					if (!string.IsNullOrWhiteSpace(content))
					{
						var blob = Tenant.GetService<IStorageService>().Select(text.TextBlob);

						Result.Blob.Content = Encoding.UTF8.GetBytes(content);
						Result.Blob.ContentType = blob.ContentType;
					}
				}
			}

			if (Mode == ChangeQueryMode.Full)
			{
				var configuration = Tenant.GetService<IStorageService>().Download(Component.Token);

				if (configuration != null)
					Result.Configuration = configuration.Content;

				if (IncludeRuntimeConfiguration && Component.RuntimeConfiguration != Guid.Empty)
				{
					var runtimeConfiguration = Tenant.GetService<IStorageService>().Download(Component.RuntimeConfiguration);

					if (runtimeConfiguration != null)
						Result.RuntimeConfiguration = runtimeConfiguration.Content;
				}
			}
		}

		private bool HasChanged(Guid component, Guid blob)
		{
			var diff = Tenant.GetService<IDesignService>().VersionControl.GetDiff(component, blob, CommitToken);

			if (diff == null)
				return false;

			return string.Compare(diff.Original, diff.Modified, StringComparison.Ordinal) != 0;
		}

		[Obsolete]
		private bool IncludeRuntimeConfiguration => string.Compare(Component.Category, ComponentCategories.StringTable, true) == 0;

		private string ResolveSyntax(object text)
		{
			if (text == null)
				return "?";

			var syntax = text.GetType().FindAttribute<SyntaxAttribute>();

			return syntax == null ? SyntaxAttribute.CSharp : syntax.Syntax;
		}

		private string ResolveFileName(object text)
		{
			if (text == null)
				return text.ToString();

			var syntax = text.GetType().FindAttribute<SyntaxAttribute>();

			if (syntax == null)
				return text.ToString();

			if (string.Compare(syntax.Syntax, SyntaxAttribute.CSharp, true) == 0)
				return $"{text.ToString()}.csx";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Css, true) == 0)
				return $"{text.ToString()}.css";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Javascript, true) == 0)
				return $"{text.ToString()}.jsm";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Json, true) == 0)
				return $"{text.ToString()}.json";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Less, true) == 0)
				return $"{text.ToString()}.less";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Razor, true) == 0)
				return $"{text.ToString()}.cshtml";
			else
				return text.ToString();
		}

		private void ParseElements()
		{
			if (Configuration == null)
				return;

			var texts = Tenant.GetService<IDiscoveryService>().Configuration.Query<IText>(Configuration);

			foreach (var txt in texts)
			{
				if (txt == Configuration)
					continue;

				CreateTextChain(Result, txt);
			}
		}

		private void CreateTextChain(IChangeElement element, IText text)
		{
			var chain = new Stack<IElement>();
			var current = text.Parent;

			while (current != null)
			{
				if (current is IConfiguration)
					break;

				chain.Push(current);

				current = current.Parent;
			}

			var currentDescriptor = element;

			while (chain.Count > 0)
			{
				var chainElement = chain.Pop();
				var chainDescriptor = currentDescriptor.Elements.FirstOrDefault(f => f.Id == chainElement.Id);

				if (chainDescriptor == null)
				{
					chainDescriptor = new ChangeElement
					{
						Id = chainElement.Id,
						Name = ResolveName(chainElement)
					};

					currentDescriptor.Elements.Add(chainDescriptor);
				}

				currentDescriptor = chainDescriptor;
			}

			var child = new ChangeElement
			{
				Id = text.Id,
				Name = text.ToString()
			};

			currentDescriptor.Elements.Add(child);

			child.Blob.Syntax = ResolveSyntax(text);
			child.Blob.FileName = ResolveFileName(text);
			child.Blob.Token = text.TextBlob;
			child.HasChanged = HasChanged(text.Configuration().Component, text.TextBlob);
			child.Blob.HasChanged = child.HasChanged;

			if (Mode == ChangeQueryMode.Full)
			{
				var childBlob = Tenant.GetService<IStorageService>().Select(text.TextBlob);

				if (childBlob != null)
				{
					var childContent = Tenant.GetService<IStorageService>().Download(childBlob.Token);

					if (childContent != null)
					{
						child.Blob.Content = childContent.Content;
						child.Blob.ContentType = childBlob.ContentType;
					}
				}
			}
		}

		private string ResolveName(IElement element)
		{
			if (element.Parent == null || element.Parent.GetType().IsCollection())
				return element.ToString();

			var props = element.Parent.GetType().GetProperties();

			foreach (var property in props)
			{
				if (property.GetType().IsCollection())
					continue;

				var value = property.GetValue(element.Parent);

				if (value == element)
					return property.Name;
			}

			return element.ToString();
		}
	}
}
