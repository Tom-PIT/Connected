using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel.Analysis.Manifest.Entities;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Compilation;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Analysis.Manifest.Providers
{
	internal class ApiManifestProvider : ComponentManifestProvider<IApi>
	{
		protected override IComponentManifest OnCreateManifest()
		{
			var manifest = new ApiManifest();

			BindManifest(manifest);
			BindApi(manifest);
			BindOperations(manifest);

			return manifest;
		}

		private void BindApi(ApiManifest manifest)
		{
			manifest.Scope = Configuration.Scope;
		}

		private void BindOperations(ApiManifest manifest)
		{
			foreach (var operation in Configuration.Operations)
				BindOperation(manifest, operation);
		}

		private void BindOperation(ApiManifest manifest, IApiOperation operation)
		{
			var om = new ApiOperationManifest
			{
				Name = operation.Name,
				Scope = operation.Scope
			};

			manifest.Operations.Add(om);

			var operationType = Connection.GetService<ICompilerService>().ResolveType(MicroService.Token, operation, operation.Name, false);

			if (operationType == null)
			{
				om.NotImplemented();
				return;
			}

			if (operationType.FindAttribute<SupportsTransactionAttribute>() != null)
				om.SupportsTransaction = true;

			var context = Connection.GetService<ICompilerService>().CreateScriptContext(operation);

			var tree = CreateSyntaxTree(context);

			if (tree == null)
			{
				om.SyntaxTreeException();
				return;
			}

			var root = tree.GetRoot();

			BindType(root, operationType, om);
			BindExtenders(root, operationType, om);
			var returnValue = operationType.GetInterface(typeof(IOperation<>).FullName);

			if (returnValue !=null)
			{
				var genericArgs = returnValue.GetGenericArguments();

				if (genericArgs.Length > 0)
				{
					var returnType = genericArgs[0];

					om.ReturnType = new ManifestType();

					BindType(root, returnType, om.ReturnType);
				}
			}
			else if (operationType is IAsyncOperation)
				om.Async = true;
		}

		private void BindExtenders(SyntaxNode scope, Type operationType, ApiOperationManifest manifest)
		{
			var extenders = operationType.FindAttributes<ExtenderAttribute>();

			if (extenders == null)
				return;

			foreach(var extender in extenders)
			{
				var extenderType = new ManifestType();

				BindType(scope, extender.Extender, extenderType);

				manifest.Extenders.Add(extenderType);
			}
		}
	}
}
