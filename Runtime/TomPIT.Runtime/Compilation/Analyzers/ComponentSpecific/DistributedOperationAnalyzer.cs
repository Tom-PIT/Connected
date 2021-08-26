using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TomPIT.Connectivity;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Middleware.Interop;

namespace TomPIT.Compilation.Analyzers.ComponentSpecific
{
    internal class DistributedOperationAnalyzer : AnalyzerBase
    {
        private List<DiagnosticDescriptor> _descriptors = null;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.ToImmutableArray();

        public DistributedOperationAnalyzer(ITenant tenant, Guid microService, Guid component, Guid script) : base(tenant, microService, component, script)
        {
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CompilationUnit);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (!IsScriptFile(context))
                return;

            //Find base class declaration
            var distributedOperation = FindBaseDistributedOperation(context);

            if (distributedOperation is null)
                return;

            var onInvokingNode = FindClassMethod(distributedOperation, "OnBeginInvoke");
            if (ContainsEventTriggerCall(onInvokingNode, context, out var d1))
            {
                if (d1 is not null)
                    context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[0], d1.GetLocation()));
                return;
            }

            var onCommitNode = FindClassMethod(distributedOperation, "OnCommit");
            if (ContainsEventTriggerCall(onCommitNode, context, out var d2))
            {
                if (d2 is not null)
                    context.ReportDiagnostic(Microsoft.CodeAnalysis.Diagnostic.Create(Descriptors[0], d2.GetLocation()));
                return;
            }
        }

        private ClassDeclarationSyntax FindBaseDistributedOperation(SyntaxNodeAnalysisContext context)
        {
            var root = context.Node;

            if (root is null)
                return null;

            if (root.FindClass(System.IO.Path.GetFileNameWithoutExtension(this.Text.FileName)) is not ClassDeclarationSyntax declaration)
                return null;

            if (declaration.LookupBaseType(context.SemanticModel, typeof(DistributedOperation).AssemblyQualifiedName) is not null)
                return declaration;

            return null;
        }

        private MethodDeclarationSyntax FindClassMethod(ClassDeclarationSyntax classNode, string memberName)
        {
            return classNode?.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(e => string.Compare(e.Identifier.Text, memberName, false) == 0);
        }

        private bool ContainsEventTriggerCall(MethodDeclarationSyntax methodNode, SyntaxNodeAnalysisContext context, out SyntaxNode node)
        {
            node = null;

            if (methodNode is null)
                return false;

            var eventInvoke = FindFirstEventTriggerInvoke(methodNode, context);

            if (eventInvoke is null)
                return false;

            if (eventInvoke.ArgumentList.Arguments.Count == 3)
                return true;

            node = eventInvoke;
            return true;
        }


        private InvocationExpressionSyntax FindFirstEventTriggerInvoke(MethodDeclarationSyntax baseNode, SyntaxNodeAnalysisContext context)
        {
            if (baseNode is null)
                return null;

            foreach (var statement in baseNode.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expressionNode)
                    if (expressionNode.Expression is InvocationExpressionSyntax invocationNode)
                    {
                        if (IsTriggerEventExpression(invocationNode))
                            return invocationNode;
                        else
                        {
                            var method = FindClassMethod(FindBaseDistributedOperation(context), GetIdentiferName(invocationNode).Identifier.Text);
                            if (FindFirstEventTriggerInvoke(method, context) is InvocationExpressionSyntax eventInvocation)
                                return eventInvocation;
                        }
                    }
            }

            return null;
        }


        private bool IsTriggerEventExpression(InvocationExpressionSyntax invocationNode)
        {
            if (invocationNode.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var name = memberAccess.Name.Identifier.Text;
                return string.Compare(name, nameof(Middleware.Services.IMiddlewareEvents.TriggerEvent)) == 0;
            }

            return false;
        }


        private List<DiagnosticDescriptor> Descriptors
        {
            get
            {
                if (_descriptors == null)
                {
                    _descriptors = new List<DiagnosticDescriptor>
                    {
                          new DiagnosticDescriptor("TP1101", "Callback expected", "The first event triggered in a DistributedOperation should define the Callback argument", "Arguments", DiagnosticSeverity.Warning, true),
                    };
                }

                return _descriptors;
            }
        }

        private static IdentifierNameSyntax GetIdentiferName(InvocationExpressionSyntax syntax)
        {
            if (syntax == null)
                return null;

            var current = syntax.Expression;

            while (current != null)
            {
                if (current is IdentifierNameSyntax idn)
                    return idn;

                if (current is MemberAccessExpressionSyntax ma)
                    current = ma.Expression;
                else
                    break;
            }

            return null;
        }
    }
}
