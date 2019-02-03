using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Design
{
	public class MicroServiceDescriptor
	{
		private List<IApi> _apis = null;
		private IServiceReferences _refs = null;
		private List<MicroServiceDescriptor> _items = null;

		internal MicroServiceDescriptor(IExecutionContext context, DiscoveryModel model, IMicroService service)
		{
			Context = context;
			MicroService = service;
			Model = model;

			BindApis();

			Authorize();
		}

		private void Authorize()
		{
			var e = new AuthorizationArgs(Context.GetAuthenticatedUserToken(), Claims.ImplementMicroservice, MicroService.Token.ToString());

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			Authorized = Context.Connection().GetService<IAuthorizationService>().Authorize(Context, e).Success;
		}

		private void BindApis()
		{
			foreach (var i in Model.Apis.Where(f => f.MicroService(Context.Connection()) == MicroService.Token))
				Apis.Add(i);
		}

		public IMicroService MicroService { get; }
		private IExecutionContext Context { get; }
		private DiscoveryModel Model { get; }

		public int ReferencedCount { get; set; }
		public bool Authorized { get; private set; }

		public List<IApi> Apis
		{
			get
			{
				if (_apis == null)
					_apis = new List<IApi>();

				return _apis;
			}
		}

		public IServiceReferences References
		{
			get
			{
				if (_refs == null)
				{
					var component = Context.Connection().GetService<IComponentService>().SelectComponent(MicroService.Token, "Reference", "References");

					if (component == null)
						return null;

					_refs = Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) as IServiceReferences;
				}

				return _refs;
			}
		}

		public int ReferenceCount { get { return References == null ? 0 : References.MicroServices.Count; } }

		public string ApiName(IApi api)
		{
			return api.ComponentName(Context);
		}

		public List<MicroServiceDescriptor> Items
		{
			get
			{
				if (_items == null && ReferenceCount > 0)
				{
					_items = new List<MicroServiceDescriptor>();

					foreach (var i in References.MicroServices)
					{
						if (i.Validation.Validate(Context))
						{
							var ms = Context.Connection().GetService<IMicroServiceService>().Select(i.MicroService);

							if (ms != null)
								_items.Add(new MicroServiceDescriptor(Context, Model, ms));
						}
					}
				}

				return _items;
			}
		}

		public int OperationCount
		{
			get
			{
				var r = 0;

				foreach (var i in Apis)
					r += i.Operations.Count;

				return r;
			}
		}
	}
}
