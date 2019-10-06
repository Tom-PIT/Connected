using System;

namespace TomPIT.Annotations.Design
{
	[Obsolete]
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class CodeAnalysisProviderAttribute : Attribute
	{
		private const string DesignAssembly = "TomPIT.Design";
		private const string DevelopmentAssembly = "TomPIT.Development";

		//public const string NavigationUrlProvider = "TomPIT.Design.CodeAnalysis.Providers.NavigationUrlProvider, " + DesignAssembly;
		//public const string NavigationViewUrlProvider = "TomPIT.Design.CodeAnalysis.Providers.NavigationViewUrlProvider, " + DesignAssembly;


		public const string MicroservicesProvider = "TomPIT.Development.CodeAnalysis.Providers.MicroServicesProvider, " + DevelopmentAssembly;
		public const string SubscriptionProvider = "TomPIT.Design.CodeAnalysis.Providers.SubscriptionProvider, " + DesignAssembly;
		public const string SubscriptionEventProvider = "TomPIT.Design.CodeAnalysis.Providers.SubscriptionEventProvider, " + DesignAssembly;

		public const string ApiParameterProvider = "TomPIT.Design.CodeAnalysis.Providers.ApiParameterProvider, " + DesignAssembly;
		public const string MailTemplateProvider = "TomPIT.Design.CodeAnalysis.Providers.MailTemplateProvider, " + DesignAssembly;
		public const string MediaProvider = "TomPIT.Design.CodeAnalysis.Providers.MediaProvider, " + DesignAssembly;
		public const string IoTHubProvider = "TomPIT.Design.CodeAnalysis.Providers.IoTHubProvider, " + DesignAssembly;
		public const string IoTHubFieldProvider = "TomPIT.Design.CodeAnalysis.Providers.IoTHubFieldProvider, " + DesignAssembly;
		public const string QueueWorkerProvider = "TomPIT.Design.CodeAnalysis.Providers.QueueWorkerProvider, " + DesignAssembly;
		public const string PartialProvider = "TomPIT.Design.CodeAnalysis.Providers.PartialProvider, " + DesignAssembly;
		public const string BigDataPartitionProvider = "TomPIT.Design.CodeAnalysis.Providers.BigDataPartitionProvider, " + DesignAssembly;

		public CodeAnalysisProviderAttribute() { }

		public CodeAnalysisProviderAttribute(string type)
		{
			TypeName = type;
		}
		public CodeAnalysisProviderAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}