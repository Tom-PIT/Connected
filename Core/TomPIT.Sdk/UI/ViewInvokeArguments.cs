using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TomPIT.Middleware;
using TomPIT.Models;

namespace TomPIT.UI
{
	public class ViewInvokeArguments : MicroServiceContext
	{
		private ViewDataDictionary<IRuntimeModel> _viewData = null;
		private TempDataDictionary _tempData = null;
		private ViewBagDictionary _dynamicViewDataDictionary = null;
		private dynamic _viewBag = null;
		public ViewInvokeArguments(IViewModel context) : base(context)
		{
			Model = context;
		}

		public ViewInvokeArguments(ViewDataDictionary viewData, ITempDataDictionary tempData, dynamic viewBag) : base(viewData.Model as IMicroServiceContext)
		{
			Model = viewData.Model as IViewModel;
			_viewData = viewData as ViewDataDictionary<IRuntimeModel>;
			_tempData = tempData as TempDataDictionary;
			_viewBag = viewBag;
		}

		public IViewModel Model { get; }

		public void Forbidden()
		{
			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}

		public void NotFound()
		{
			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
		}

		public void BadRequest()
		{
			Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		}

		public ViewDataDictionary<IRuntimeModel> ViewData
		{
			get
			{
				if (_viewData == null)
				{
					_viewData = new ViewDataDictionary<IRuntimeModel>(
						metadataProvider: new EmptyModelMetadataProvider(),
						modelState: new ModelStateDictionary())
					{
						Model = Model
					};

				}

				return _viewData;
			}
		}

		public TempDataDictionary TempData
		{
			get
			{
				if (_tempData == null)
					_tempData = new TempDataDictionary(Model.ActionContext.HttpContext, Model.TempData);

				return _tempData;
			}
		}

		public dynamic ViewBag
		{
			get
			{
				if (_viewBag != null)
					return _viewBag;

				if (_dynamicViewDataDictionary == null)
					_dynamicViewDataDictionary = new ViewBagDictionary(() => ViewData);

				return _dynamicViewDataDictionary;
			}

		}
	}
}