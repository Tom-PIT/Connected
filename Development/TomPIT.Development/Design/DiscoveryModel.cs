using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Environment;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Design
{
	public class DiscoveryModel
	{
		private List<MicroServiceDescriptor> _services = null;
		private List<IResourceGroup> _resourceGroups = null;
		private List<IApi> _apis = null;

		public DiscoveryModel(IExecutionContext context)
		{
			Context = context;
		}

		private IExecutionContext Context { get; }

		private List<MicroServiceDescriptor> Services
		{
			get
			{
				if (_services == null)
				{
					_services = new List<MicroServiceDescriptor>();
					var ms = Context.Connection().GetService<IMicroServiceService>().Query();

					foreach (var i in ms)
						_services.Add(new MicroServiceDescriptor(Context, this, i));

					foreach (var i in ms)
					{
						var refs = Context.Connection().GetService<IDiscoveryService>().References(i.Token);

						if (refs == null || refs.MicroServices.Count == 0)
							continue;

						foreach (var j in refs.MicroServices)
						{
							if (!j.IsValid)
								continue;

							var target = _services.FirstOrDefault(f => string.Compare(f.MicroService.Name, j.MicroService, true) == 0);

							if (target == null)
								continue;

							target.ReferencedCount++;
						}
					}

					if (_services.Count > 0)
						_services = _services.OrderBy(f => f.MicroService.Name).ThenBy(f => f.ReferenceCount).ToList();
				}

				return _services;
			}
		}

		public List<MicroServiceDescriptor> Root { get { return Services.Where(f => f.ReferencedCount == 0).ToList(); } }

		private List<IResourceGroup> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = Context.Connection().GetService<IResourceGroupService>().Query();

				return _resourceGroups;
			}
		}

		internal List<IApi> Apis
		{
			get
			{
				if (_apis == null)
				{
					var apis = Context.Connection().GetService<IComponentService>().QueryConfigurations(ResourceGroups.Select(f => f.Name).ToList(), "Api");

					_apis = new List<IApi>();

					foreach (var i in apis)
					{
						if (!(i is IApi api))
							continue;

						_apis.Add(api);
					}
				}

				return _apis;
			}
		}
	}
}
