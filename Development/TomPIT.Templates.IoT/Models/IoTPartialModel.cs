using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.ComponentModel.UI;
using TomPIT.IoT.UI;
using TomPIT.IoT.UI.Stencils;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.IoT.Models
{
	public class IoTPartialModel : ExecutionContext, IRuntimeModel
	{
		private IContextIdentity _identity = null;
		private List<string> _stencils = null;
		private List<IIoTElement> _targetStencils = null;

		public IComponent Component { get; set; }
		public IView ViewConfiguration { get; set; }

		protected Controller Controller { get; private set; }
		public ActionContext ActionContext { get; }
		public List<string> Stencils
		{
			get
			{
				if (_stencils == null)
					_stencils = new List<string>();

				return _stencils;
			}
		}

		public List<IIoTElement> TargetStencils
		{
			get
			{
				if (_targetStencils == null)
				{
					_targetStencils = new List<IIoTElement>();
					var v = View as IoTView;

					foreach (var i in Stencils)
					{
						var stencil = v.Elements.FirstOrDefault(f => string.Compare(f.Name, i, true) == 0);

						if (stencil == null)
							throw new RuntimeException(string.Format("{0} ({1})", SR.ErrIoTStencilNotFound, i));

						TargetStencils.Add(stencil);
					}
				}

				return _targetStencils;
			}
		}

		public IMicroService MicroService { get; private set; }
		public IIoTView View { get; private set; }

		public void Initialize(Controller controller)
		{
			Controller = controller;
		}

		public void Initialize(Controller controller, string microService, string view)
		{
			Controller = controller;

			MicroService = GetService<IMicroServiceService>().Select(microService);

			if (MicroService == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, microService));

			View = GetService<IComponentService>().SelectConfiguration(MicroService.Token, "View", view) as IIoTView;

			if (View == null)
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrViewNotFound, view));

			Initialize(null, null, null, MicroService.Token.ToString());
		}

		public IEnumerable<ValidationResult> Validate()
		{
			return null;
		}

		public void Bind(string authorityId, string authority, string contextId)
		{

		}

		public string Title { get; protected set; }
		public IModelNavigation Navigation => null;
	}
}
