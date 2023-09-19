using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using TomPIT.App.Models;
using TomPIT.Controllers;
using TomPIT.Diagnostics;
using TomPIT.Diagnostics.Tracing;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.UI;

namespace TomPIT.App.Controllers
{
	[AllowAnonymous]
	public class ApiController : ServerController
	{
		private IViewEngine _viewEngine = null;
		private readonly ITraceService _traceService;

		public ApiController(IViewEngine viewEngine)
		{
			this._viewEngine = viewEngine;
			this._traceService = MiddlewareDescriptor.Current.Tenant.GetService<ITraceService>();

			RegisterTraceEndpoints(_traceService);
		}

		[HttpPost]
		public IActionResult Invoke()
		{
			var sw = Stopwatch.StartNew();

			var m = CreateModel();

			HttpContext.Response.RegisterForDispose(m);

			_traceService?.Trace(new TraceMessage(InvokeEndpoint, $"[{m.QualifierName}]"));

			var response = m.Interop.Invoke<object, JObject>(m.QualifierName, m.Body);

			if (sw.ElapsedMilliseconds > 2000)
				_traceService?.Trace(new TraceMessage(LongLastingInvokeEndpoint, $"[{m.QualifierName}]"));

			return Json(Serializer.Serialize(response));
		}

		[HttpPost]
		public IActionResult UIInjection()
		{
			var model = CreateUIInjectionModel();

			HttpContext.Response.RegisterForDispose(model);

			return PartialView("~/Views/UIInjectionView.cshtml", model);
		}

		[HttpPost]
		public async Task<IActionResult> Partial()
		{
			HttpContext.Request.EnableBuffering();

			var context = CreatePartialModel();

			HttpContext.Response.RegisterForDispose(context);

			_viewEngine.Context = HttpContext;
			var content = await _viewEngine.RenderPartialToStringAsync(context, context.QualifierName);

			return Content(content, "text/html");
		}

		[HttpPost]
		public IActionResult Search()
		{
			var model = CreateSearchModel();

			HttpContext.Response.RegisterForDispose(model);

			return Json(model.Search());
		}

		[HttpPost]
		public IActionResult SetUserData()
		{
			var model = CreateUserDataModel();

			HttpContext.Response.RegisterForDispose(model);
			model.SetData();

			return new EmptyResult();
		}

		[HttpPost]
		public IActionResult GetUserData()
		{
			var model = CreateUserDataModel();

			HttpContext.Response.RegisterForDispose(model);

			return Json(model.GetData());
		}

		[HttpPost]
		public IActionResult QueryUserData()
		{
			var model = CreateUserDataModel();

			HttpContext.Response.RegisterForDispose(model);

			return Json(model.QueryData());
		}

		private UserDataModel CreateUserDataModel()
		{
			var r = new UserDataModel
			{
				Body = FromBody()
			};

			r.Databind();
			r.Initialize(this, null);

			return r;
		}

		private ApiModel CreateModel()
		{
			ApiModel r;
			var apiHeader = Request.Headers["X-TP-API"];
			var componentHeader = Request.Headers["X-TP-COMPONENT"];

			if (!string.IsNullOrWhiteSpace(apiHeader))
				r = new ApiModel(apiHeader, componentHeader);
			else
				r = new ApiModel { Body = FromBody() };

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}

		private PartialModel CreatePartialModel()
		{
			var r = new PartialModel
			{
				Body = FromBody()
			};

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}

		private UIInjectionModel CreateUIInjectionModel()
		{
			var r = new UIInjectionModel
			{
				Body = FromBody()
			};

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}

		private SearchModel CreateSearchModel()
		{
			var r = new SearchModel { Body = FromBody() };

			r.Databind();
			r.Initialize(this, r.MicroService);

			return r;
		}

		private void RegisterTraceEndpoints(ITraceService service)
		{
			if (service is null)
				return;

			service.AddEndpoint(InvokeEndpoint);
			service.AddEndpoint(LongLastingInvokeEndpoint);
		}

		#region TraceEndpoints
		private ITraceEndpoint InvokeEndpoint => new TraceEndpoint(nameof(ApiController), nameof(Invoke));
		private ITraceEndpoint LongLastingInvokeEndpoint => new TraceEndpoint(nameof(ApiController), "LongLastingInvoke");
		#endregion
	}
}
