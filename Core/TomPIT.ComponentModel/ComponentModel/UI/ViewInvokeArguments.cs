using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Models;
using TomPIT.Services;

namespace TomPIT.ComponentModel.UI
{
	public class ViewInvokeArguments : DataModelContext
	{
		private ViewDataDictionary<IRuntimeModel> _viewData = null;
		private TempDataDictionary _tempData = null;
		private ViewBagDictionary _dynamicViewDataDictionary = null;
		public ViewInvokeArguments(IRuntimeModel context, ITempDataProvider tempDataProvider) : base(context)
		{
			Model = context;
			TempDataProvider = tempDataProvider;
		}

		public IRuntimeModel Model { get; }
		private ITempDataProvider TempDataProvider { get; }

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
					_tempData = new TempDataDictionary(Model.ActionContext.HttpContext, TempDataProvider);

				return _tempData;
			}
		}

		public dynamic ViewBag
		{
			get
			{
				if (_dynamicViewDataDictionary == null)
				{
					_dynamicViewDataDictionary = new ViewBagDictionary(() => ViewData);
				}

				return _dynamicViewDataDictionary;
			}

		}
	}
}