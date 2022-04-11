namespace TomPIT.UI.Theming
{
	using Microsoft.Extensions.DependencyInjection;
	using TomPIT.UI.Theming.Configuration;
	using TomPIT.UI.Theming.Engine;

	public class EngineFactory
    {
        public LessConfiguration Configuration { get; set; }

        public EngineFactory(LessConfiguration configuration)
        {
            Configuration = configuration;
        }
        public EngineFactory() : this(LessConfiguration.GetDefault())
        {
        }

        public ILessEngine GetEngine()
        {
            return GetEngine(new ContainerFactory());
        }

        public ILessEngine GetEngine(ContainerFactory containerFactory)
        {
            var container = containerFactory.GetContainer(Configuration);
            return container.GetRequiredService<ILessEngine>();
        }
    }
}