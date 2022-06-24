namespace TomPIT.UI.Theming
{
	using System.Collections.Generic;
	using Cache;
	using Input;
	using Loggers;
	using Microsoft.Extensions.DependencyInjection;
	using Parameters;
	using Stylizers;
	using TomPIT.UI.Theming.Configuration;
	using TomPIT.UI.Theming.Engine;
	using TomPIT.UI.Theming.Importers;
	using TomPIT.UI.Theming.Plugins;

	public class ContainerFactory
    {
        protected IServiceCollection Container { get; set; }

        public System.IServiceProvider GetContainer(LessConfiguration configuration)
        {
            var builder = GetServices(configuration);

            return builder.BuildServiceProvider();
        }

        public ServiceCollection GetServices(LessConfiguration configuration)
        {
            var services = new ServiceCollection();
            RegisterServices(services, configuration);

            return services;
        }

        protected virtual void RegisterServices(IServiceCollection services, LessConfiguration configuration)
        {            
            if (!configuration.Web)
                RegisterLocalServices(services);

            RegisterCoreServices(services, configuration);

            OverrideServices(services, configuration);
        }

        protected virtual void OverrideServices(IServiceCollection services, LessConfiguration configuration)
        {
            if (configuration.Logger != null)
                services.AddSingleton(typeof(ILogger), configuration.Logger);

            if (configuration.LoggerInstance != null)
                services.AddSingleton(typeof(ILogger), configuration.LoggerInstance);
        }

        protected virtual void RegisterLocalServices(IServiceCollection services)
        {
            services.AddSingleton<ICache, InMemoryCache>();
            services.AddSingleton<IParameterSource, ConsoleArgumentParameterSource>();
            services.AddSingleton<ILogger, ConsoleLogger>();
            services.AddSingleton<IPathResolver, RelativePathResolver>();
        }

        protected virtual void RegisterCoreServices(IServiceCollection services, LessConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<IStylizer, PlainStylizer>();

            services.AddTransient<IImporter, Importer>();
            services.AddTransient<Parser.LessParser>();          

            services.AddTransient<ILessEngine, LessEngine>();           

            if (configuration.CacheEnabled)
                services.Decorate<ILessEngine, CacheDecorator>();

            if (!configuration.DisableParameters)
                services.Decorate<ILessEngine, ParameterDecorator>();

            services.AddSingleton<IEnumerable<IPluginConfigurator>>(configuration.Plugins);
            services.AddSingleton(typeof(IFileReader), configuration.LessSource);
        }
    }
}
