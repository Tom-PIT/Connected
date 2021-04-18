using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Reflection;

namespace TomPIT.Compilation.Analyzers
{
	[DiagnosticAnalyzer("csharp")]
	internal abstract class AnalyzerBase : DiagnosticAnalyzer, IDiagnosticAnalyzer
	{
		private IComponent _component;
		private IMicroService _ms;
		private IConfiguration _configuration;
		private IText _script;
		protected AnalyzerBase(ITenant tenant, Guid microService, Guid component, Guid script)
		{
			Tenant = tenant;
			Script = script;
			MicroServiceToken = microService;
			ComponentToken = component;
		}

		private Guid Script { get; }
		private Guid MicroServiceToken { get; }
		private Guid ComponentToken { get; }
		public ITenant Tenant { get; }

		public IComponent Component => _component ??= Tenant.GetService<IComponentService>().SelectComponent(ComponentToken);
		public IMicroService MicroService => _ms ??= Component is null ? null : Tenant.GetService<IMicroServiceService>().Select(Component.MicroService);
		public IConfiguration Configuration => _configuration ??= Component is null ? null : Tenant.GetService<IComponentService>().SelectConfiguration(Component.Token);
		public IText Text => _script ??= Configuration is null ? null : Tenant.GetService<IDiscoveryService>().Configuration.Find(Configuration, Script) as IText;

		protected static bool IsScriptFile(SyntaxNodeAnalysisContext context)
		{
			var path = context.Node.GetLocation()?.GetLineSpan().Path;

			var proposedResult = !string.IsNullOrWhiteSpace(path) && !path.Contains("/");

			if (proposedResult)
				return !IsInPlatformClass(context);

			return false;
		}

		public DiagnosticDescriptor GetDescriptor(string id)
		{
			return OnGetDescriptor(id);
		}

		protected virtual DiagnosticDescriptor OnGetDescriptor(string id)
		{
			return SupportedDiagnostics.First(f => string.Compare(f.Id, id, true) == 0);
		}

		protected string ResolveExpectedClassName()
		{
			if (Text is null)
				return null;

			var att = Text.GetType().FindAttribute<ClassRequiredAttribute>();

			if (att is null)
				return null;

			if (Component.Token == Script)
				return Component.Name;

			if (string.IsNullOrWhiteSpace(att.ClassNameProperty))
				return Text.ToString();

			var property = Text.GetType().GetProperty(att.ClassNameProperty);

			if (property is null)
				return null;

			return Types.Convert<string>(property.GetValue(Text));
		}

		private static bool IsInPlatformClass(SyntaxNodeAnalysisContext context)
		{
			var current = context.Node;

			while (current is not null && current is not ICompilationUnitSyntax)
			{
				if (current is ClassDeclarationSyntax declaration && ClassExtensions.IsPlatformClass(declaration.ClassName(context.SemanticModel)))
					return true;

				current = current.Parent;
			}

			return false;
		}
	}
}
