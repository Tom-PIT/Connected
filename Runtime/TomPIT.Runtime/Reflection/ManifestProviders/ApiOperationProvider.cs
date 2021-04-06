using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Annotations;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Middleware;
using TomPIT.Middleware.Interop;
using TomPIT.Reflection.CodeAnalysis;

namespace TomPIT.Reflection.ManifestProviders
{
	internal class ApiOperationProvider : IScriptManifestProvider
	{
		public IScriptManifestType CreateTypeInstance(ITypeSymbolDescriptor descriptor)
		{
			if (descriptor.Symbol.TypeKind != Microsoft.CodeAnalysis.TypeKind.Class)
				return null;

			if (descriptor.Symbol.LookupBaseType(descriptor.Model, typeof(IDistributedOperation).FullTypeName()) is not null)
			{
				return new DistributedOperationType
				{
					IsDistributed = true
				};
			}
			else if (descriptor.Symbol.LookupBaseType(descriptor.Model, typeof(IOperation<>).FullTypeName()) is not null)
			{
				var returnType = descriptor.Symbol.LookupBaseType(descriptor.Model, typeof(IOperation<>).FullTypeName());

				if (returnType != null)
				{
					if (returnType is Microsoft.CodeAnalysis.INamedTypeSymbol namedType && !namedType.TypeArguments.IsDefaultOrEmpty)
					{
						return new GenericOperationType
						{
							ReturnType = namedType.TypeArguments.First().ToDisplayString()
						};
					}
				}
			}
			else if (descriptor.Symbol.LookupBaseType(descriptor.Model, typeof(IOperation).FullTypeName()) is not null)
			{
				var returnType = descriptor.Symbol.LookupBaseType(descriptor.Model, typeof(IOperation).FullTypeName());

				if (returnType != null)
				{
					return new OperationType
					{
						
					};
				}
			}

			return null;
		}

		public void ProcessManifestType(ITypeSymbolDescriptor descriptor, IScriptManifestType type)
		{
			if (type is not OperationType operation)
				return;

			BindVerbs(descriptor, operation);

			if (operation is GenericOperationType generic)
				BindExtenders(descriptor, generic);
		}

		private static void BindVerbs(ITypeSymbolDescriptor descriptor, OperationType type)
		{
			if (descriptor.Node is not ClassDeclarationSyntax classDeclaration)
				return;

			var list = classDeclaration.AttributeLists;

			if (list.ContainsAttribute<HttpGetAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Get;
			else if (list.ContainsAttribute<HttpPostAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Post;
			else if (list.ContainsAttribute<HttpDeleteAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Delete;
			else if (list.ContainsAttribute<HttpHeadAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Head;
			else if (list.ContainsAttribute<HttpOptionsAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Options;
			else if (list.ContainsAttribute<HttpPatchAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Patch;
			else if (list.ContainsAttribute<HttpPutAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Put;
			else if (list.ContainsAttribute<HttpTraceAttribute>(descriptor.Model))
				type.Verbs |= HttpVerbs.Trace;
		}
		private static void BindExtenders(ITypeSymbolDescriptor descriptor, GenericOperationType type)
		{
			if (descriptor.Node is not ClassDeclarationSyntax classDeclaration)
				return;

			var list = classDeclaration.AttributeLists;

			foreach (var attributeList in list)
			{
				foreach (var attribute in attributeList.Attributes)
				{
					var typeInfo = descriptor.Model.GetTypeInfo(attribute);

					if (typeInfo.Type?.IsInheritedFrom(typeof(ExtenderAttribute).FullTypeName()) == true)
						type.Extenders.Add(typeInfo.Type.ToDisplayString());
				}
			}
		}
	}
}