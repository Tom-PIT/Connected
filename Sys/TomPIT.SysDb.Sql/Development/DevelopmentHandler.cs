using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development
{
	internal class DevelopmentHandler : IDevelopmentHandler
	{
		private IMicroServiceHandler _microServices = null;
		private IFeatureHandler _features = null;
		private IComponentHandler _components = null;
		private IQaHandler _qa = null;

		public IQaHandler QA
		{
			get
			{
				if (_qa == null)
					_qa = new QaHandler();

				return _qa;
			}
		}


		public IMicroServiceHandler MicroServices
		{
			get
			{
				if (_microServices == null)
					_microServices = new MicroServiceHandler();

				return _microServices;
			}
		}

		public IFeatureHandler Features
		{
			get
			{
				if (_features == null)
					_features = new FeatureHandler();

				return _features;
			}
		}

		public IComponentHandler Components
		{
			get
			{
				if (_components == null)
					_components = new ComponentHandler();

				return _components;
			}
		}
	}
}