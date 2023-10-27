using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TomPIT.MicroServices.Resources;

internal static class ResXWriter
{
	public static async Task<string> Write(IAssemblyResourceConfiguration configuration)
	{
		var ms = Tenant.GetService<IMicroServiceService>().Select(configuration.MicroService());
		var text = Tenant.GetService<IComponentService>().SelectText(ms.Token, configuration);
		var identifier = configuration.ComponentName();
		var members = CreateMembers(ms.Name, identifier, text);
		var usings = CreateUsings();

		var ns = NamespaceDeclaration(ParseName(ms.Name)).AddUsings(usings).AddMembers(members);

		ns = ns.NormalizeWhitespace();

		await using var stream = new MemoryStream();
		await using var writer = new StreamWriter(stream);

		ns.WriteTo(writer);

		writer.Flush();

		stream.Seek(0, SeekOrigin.Begin);

		return Encoding.UTF8.GetString(stream.ToArray());
	}

	private static UsingDirectiveSyntax[] CreateUsings()
	{
		return new List<UsingDirectiveSyntax>
		{
			UsingDirective(ParseName("System"))
		}.ToArray();
	}

	private static MemberDeclarationSyntax CreateMembers(string ns, string identifier, string text)
	{
		var attributes1 = AttributeList(SeparatedList(new List<AttributeSyntax> { GeneratedCodeAttribute() }));
		var attributes2 = AttributeList(SeparatedList(new List<AttributeSyntax> { DebuggerNonUserCodeAttribute() }));
		var attributes3 = AttributeList(SeparatedList(new List<AttributeSyntax> { CompilerGeneratedAttribute() }));

		var result = ClassDeclaration(identifier);

		result = result.AddAttributeLists(attributes1, attributes2, attributes3);
		result = result.AddModifiers(Token(SyntaxKind.InternalKeyword));

		var resourceMan = ParseMemberDeclaration("private static global::System.Resources.ResourceManager resourceMan;");
		var resourceCulture = ParseMemberDeclaration("private static global::System.Globalization.CultureInfo resourceCulture;");

		var stringProperties = CreateStringProperties(text);
		result = result.AddMembers(resourceMan, resourceCulture, CreateConstructor("SR"), CreateResourceManagerProperty(ns, identifier), CreateCultureProperty());

		if (stringProperties.Any())
			result = result.AddMembers(stringProperties.ToArray());

		return result;
	}

	private static PropertyDeclarationSyntax CreateResourceManagerProperty(string ns, string identifier)
	{
		var result = PropertyDeclaration(ParseTypeName("global::System.Resources.ResourceManager"), "ResourceManager");

		result = result.AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword));

		var get = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
		var attributes = AttributeList(SeparatedList(new List<AttributeSyntax> { EditorBrowsableAttribute() }));

		get = get.AddAttributeLists(attributes);

		get = get.AddBodyStatements(IfStatement(ParseExpression("object.ReferenceEquals(resourceMan, null)"), ParseStatement($"global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager(\"{ns}.{identifier}\", typeof({identifier}).Assembly);\r\nresourceMan = temp;\r\nreturn resourceMan;")));

		result = result.AddAccessorListAccessors(get);

		return result;
	}

	private static PropertyDeclarationSyntax CreateCultureProperty()
	{
		var result = PropertyDeclaration(ParseTypeName("global::System.Globalization.CultureInfo"), "Culture");

		result = result.AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword));

		var get = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
		var attributes = AttributeList(SeparatedList(new List<AttributeSyntax> { EditorBrowsableAttribute() }));

		result = result.AddAttributeLists(attributes);

		get = get.AddBodyStatements(ParseStatement("return resourceCulture;"));

		result = result.AddAccessorListAccessors(get);

		var set = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

		set = set.AddBodyStatements(ParseStatement("resourceCulture = value;"));

		result = result.AddAccessorListAccessors(set);

		return result;
	}

	private static ConstructorDeclarationSyntax CreateConstructor(string identifier)
	{
		var ctor = ConstructorDeclaration(identifier);

		ctor = ctor.AddModifiers(Token(SyntaxKind.InternalKeyword));
		ctor = ctor.WithBody(Block(ParseToken("{"), new SyntaxList<StatementSyntax>(), ParseToken("}")));

		var attributes = AttributeList(SeparatedList(new List<AttributeSyntax> { SuppressMessageAttribute() }));

		ctor = ctor.AddAttributeLists(attributes);

		return ctor;
	}

	private static AttributeSyntax GeneratedCodeAttribute()
	{
		var result = Attribute(ParseName("global::System.CodeDom.Compiler.GeneratedCodeAttribute"));

		var arg1 = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("System.Resources.Tools.StronglyTypedResourceBuilder"));
		var arg2 = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("17.0.0.0"));

		result = result.AddArgumentListArguments(AttributeArgument(arg1), AttributeArgument(arg2));

		return result;
	}

	private static AttributeSyntax SuppressMessageAttribute()
	{
		var result = Attribute(ParseName("global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute"));

		var arg1 = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Microsoft.Performance"));
		var arg2 = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("CA1811:AvoidUncalledPrivateCode"));

		result = result.AddArgumentListArguments(AttributeArgument(arg1), AttributeArgument(arg2));

		return result;
	}

	private static AttributeSyntax DebuggerNonUserCodeAttribute()
	{
		return Attribute(ParseName("global::System.Diagnostics.DebuggerNonUserCodeAttribute"));
	}

	private static AttributeSyntax CompilerGeneratedAttribute()
	{
		return Attribute(ParseName("global::System.Runtime.CompilerServices.CompilerGeneratedAttribute"));
	}

	private static AttributeSyntax EditorBrowsableAttribute()
	{
		var result = Attribute(ParseName("global::System.ComponentModel.EditorBrowsableAttribute"));

		var arg1 = ParseTypeName("global::System.ComponentModel.EditorBrowsableState.Advanced");

		result = result.AddArgumentListArguments(AttributeArgument(arg1));

		return result;
	}

	private static List<PropertyDeclarationSyntax> CreateStringProperties(string text)
	{
		using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
		var keys = new List<string>();
		var doc = XDocument.Load(ms);
		var result = new List<PropertyDeclarationSyntax>();

		var strings = from i in doc.Root.Elements("data")
						  select i.Attribute("name");

		foreach (var key in strings)
			result.Add(CreateStringProperty(key.Value));

		return result;
	}

	private static PropertyDeclarationSyntax CreateStringProperty(string key)
	{
		var result = PropertyDeclaration(ParseTypeName("string"), key);

		result = result.AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword));

		var get = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);

		get = get.AddBodyStatements(ParseStatement($"return ResourceManager.GetString(\"{key}\", resourceCulture);"));

		result = result.AddAccessorListAccessors(get);

		return result;
	}
}