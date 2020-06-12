namespace TomPIT.MicroServices.Design
{
	internal static class DesignUtils
	{
		private const string DesignAssembly = "TomPIT.MicroServices.Design";
		private const string SdkAssembly = "TomPIT.Sdk";
		private const string ManagementAssembly = "TomPIT.Management";
		private const string DevelopmentAssembly = "TomPIT.Development";

		public const string ComponentApi = "Api";
		public const string ComponentModel = "Model";
		public const string ComponentEvent = "Event";
		public const string ComponentConnection = "Connection";
		public const string ComponentFeature = "Feature";
		public const string SettingFeature = "SettingFeature";
		public const string ViewFeature = "ViewFeature";
		public const string TestScenario = "Scenario";
		public const string TestCase = "TestCase";
		public const string EmbeddedAssembly = "EmbeddedAssembly";
		public const string Assembly = "Assembly";
		public const string MediaFile = "MediaFile";
		public const string Folder = "Folder";
		public const string ScriptBundle = "ScriptBundle";
		public const string Javascript = "Javascript";
		public const string ScriptUpload = "ScriptUpload";
		public const string String = "String";
		public const string Stylesheet = "Stylesheet";
		public const string Less = "Less";
		public const string Snippet = "Snippet";
		public const string Helper = "Helper";
		public const string Class = "Class";
		public const string EventBinding = "EventBinding";
		public const string IoCContainer = "IoCContainer";

		public const string ApiOperationItems = "TomPIT.MicroServices.Design.Items.OperationCollection, " + DesignAssembly;
		public const string SubscriptionEventsItems = "TomPIT.MicroServices.Design.Items.SubscriptionEventsCollection, " + DesignAssembly;
		public const string DataProviderItems = "TomPIT.MicroServices.Design.Items.DataProviderItems, " + DesignAssembly;
		public const string EventBindingsItems = "TomPIT.MicroServices.Design.Items.EventBindingsCollection, " + DesignAssembly;
		public const string FeaturesItems = "TomPIT.MicroServices.Design.Items.FeaturesCollection, " + DesignAssembly;
		public const string ScenariosItems = "TomPIT.MicroServices.Design.Items.TestScenariosCollection, " + DesignAssembly;
		public const string TestCasesItems = "TomPIT.MicroServices.Design.Items.TestCasesCollection, " + DesignAssembly;
		public const string ScriptSourceItems = "TomPIT.MicroServices.Design.Items.ScriptSourceCollection, " + DesignAssembly;
		public const string StylesheetItems = "TomPIT.MicroServices.Design.Items.StylesheetCollection, " + DesignAssembly;
		public const string ScriptItems = "TomPIT.MicroServices.Design.Items.ScriptCollection, " + DesignAssembly;
		public const string EventItems = "TomPIT.MicroServices.Design.Items.EventItems, " + DesignAssembly;
		public const string DistributedEventItems = "TomPIT.MicroServices.Design.Items.DistributedEventCollection, " + DesignAssembly;
		public const string IoCOperationItems = "TomPIT.MicroServices.Design.Items.IoCOperationCollection, " + DesignAssembly;
		public const string DependencyInjectionsItems = "TomPIT.MicroServices.Design.Items.DependencyInjectionsCollection, " + DesignAssembly;
		public const string UIDependencyInjectionsItems = "TomPIT.MicroServices.Design.Items.UIDependencyInjectionsCollection, " + DesignAssembly;
		public const string ViewsItems = "TomPIT.MicroServices.Design.Items.ViewsItems, " + DesignAssembly;
		public const string PartialsItems = "TomPIT.MicroServices.Design.Items.PartialsItems, " + DesignAssembly;
		public const string MasterItems = "TomPIT.MicroServices.Design.Items.MasterItems, " + DesignAssembly;
		public const string QueueWorkerItems = "TomPIT.MicroServices.Design.Items.QueueWorkersCollection, " + DesignAssembly;
		public const string IoCEndpointItems = "TomPIT.MicroServices.Design.Items.IoCEndpointCollection, " + DesignAssembly;
		public const string ModelOperationItems = "TomPIT.MicroServices.Design.Items.ModelOperationsCollection, " + DesignAssembly;
		public const string ApiOperationListItems = "TomPIT.MicroServices.Design.Items.ApiOperationsItems, " + DesignAssembly;
		public const string ApiElement = "TomPIT.MicroServices.Design.Dom.ApiElement, " + DesignAssembly;
		public const string ApiOperationElement = "TomPIT.MicroServices.Design.Dom.ApiOperationElement, " + DesignAssembly;
		public const string ScriptElement = "TomPIT.MicroServices.Design.Dom.ScriptElement, " + DesignAssembly;
		public const string ViewElement = "TomPIT.MicroServices.Design.Dom.ViewElement, " + DesignAssembly;
		public const string ScriptBundleElement = "TomPIT.MicroServices.Design.Dom.ScriptBundleElement, " + DesignAssembly;
		public const string SearchCatalogListItems = "TomPIT.MicroServices.Design.Items.SearchCatalogItems, " + DesignAssembly;
		public const string SubscriptionListItems = "TomPIT.MicroServices.Design.Items.SubscriptionItems, " + DesignAssembly;
		public const string SubscriptionEventListItems = "TomPIT.MicroServices.Design.Items.SubscriptionEventItems, " + DesignAssembly;
		public const string ConnectionListItems = "TomPIT.MicroServices.Design.Items.ConnectionItems, " + DesignAssembly;

		public const string ApiManifest = "TomPIT.Reflection.Manifests.Providers.ApiManifestProvider, " + SdkAssembly;
		public const string IoCManifest = "TomPIT.Reflection.Manifests.Providers.IoCManifestProvider, " + SdkAssembly;

		public const string ApiOperationCreateHandler = "TomPIT.Development.Handlers.ApiOperationCreateHandler, " + SdkAssembly;
		public const string MasterCreateHandler = "TomPIT.Handlers.MasterCreateHandler, " + DevelopmentAssembly;
		public const string ScriptCreateHandler = "TomPIT.Handlers.ScriptCreateHandler, " + DevelopmentAssembly;
		public const string ScriptBundleHandler = "TomPIT.Development.Handlers.ScriptBundleCreateHandler, " + DevelopmentAssembly;

		public const string ScheduleDesigner = "TomPIT.Management.Designers.ScheduleDesigner, " + ManagementAssembly;
		public const string AssemblyEmbeddedDesigner = "TomPIT.MicroServices.Design.Designers.AssemblyEmbeddedDesigner, " + DesignAssembly;
		public const string MediaResourceFileUploadDesigner = "TomPIT.MicroServices.Design.Designers.MediaResourceFileUploadDesigner, " + DesignAssembly;
		public const string MediaDesigner = "TomPIT.MicroServices.Design.Designers.MediaDesigner, " + DesignAssembly;
		public const string ScriptUploadDesigner = "TomPIT.MicroServices.Design.Designers.ScriptUploadDesigner, " + DesignAssembly;
		public const string StringTableDesigner = "TomPIT.MicroServices.Design.Designers.StringTable, " + DesignAssembly;

	}
}
