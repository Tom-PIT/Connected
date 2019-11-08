using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Development.Models;

namespace TomPIT.Development.Controllers
{
	public class HomeController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/Index.cshtml", CreateModel());
		}

		private HomeModel CreateModel()
		{
			var r = new HomeModel();

			r.Initialize(this, null);
			r.Databind();

			return r;
		}

		public IActionResult Tool()
		{
			var model = CreateModel();

			return View($"~/Views/Tools/{RouteData.Values["tool"].ToString()}.cshtml", model);
		}

		public IActionResult Action()
		{
			var model = CreateModel();

			model.RunTool(RouteData.Values["tool"].ToString());

			return new EmptyResult();
		}

		public IActionResult Data()
		{
			var model = CreateModel();
			var body = FromBody();

			return Json(model.GetData(RouteData.Values["tool"].ToString(), body));
		}

		public IActionResult AutoFix()
		{
			var model = CreateModel();
			var body = FromBody();

			model.AutoFix(body.Required<string>("provider"), body.Required<Guid>("error"));

			return new EmptyResult();
		}
	}
}