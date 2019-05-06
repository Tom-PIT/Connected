using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.Ide;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	public abstract class IdeControllerBase : ServerController
	{
		public IActionResult Index()
		{
			return View("~/Views/Ide/Ide.cshtml", CreateModel());
		}

		[HttpPost]
		public ActionResult Dom()
		{
			return View("~/Views/Ide/Dom.cshtml", CreateModel());
		}

		[HttpPost]
		public ActionResult Section()
		{
			var m = CreateModel();

			var s = m.RequestBody.Optional("section", string.Empty);

			if (string.IsNullOrWhiteSpace(s))
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerSection, "section");

			if (!Enum.TryParse(s, true, out EnvironmentSection section))
				throw IdeException.InvalidSection(this, IdeEvents.DesignerSection);

			Response.Headers.Add("section", section.ToString());

			switch (section)
			{
				case EnvironmentSection.None:
					return Ok();
				case EnvironmentSection.Explorer:
					return View("~/Views/Ide/Dom.cshtml", m);
				case EnvironmentSection.Designer:
					if (string.IsNullOrWhiteSpace(m.RequestBody.Optional("property", string.Empty)))
						return View("~/Views/Ide/Designers/Designer.cshtml", m);
					else
						return View("~/Views/Ide/Designers/PropertyDesigner.cshtml", m);
				case EnvironmentSection.Selection:
					return View("~/Views/Ide/Selection/Selection.cshtml", m);
				case EnvironmentSection.Properties:
					return View("~/Views/Ide/Selection/Properties.cshtml", m);
				case EnvironmentSection.Events:
					return View("~/Views/Ide/Selection/Events.cshtml", m);
				case EnvironmentSection.Toolbox:
					return View("~/Views/Ide/Selection/Toolbox.cshtml", m);
				case EnvironmentSection.Property:
					return View("~/Views/Ide/Selection/Property.cshtml", m);
				case EnvironmentSection.ErrorList:
					return View("~/Views/Ide/Selection/ErrorList.cshtml", m);
				case EnvironmentSection.All:
					return View("~/Views/Ide/Console.cshtml", m);
				default:
					throw new NotSupportedException();
			}

		}

		[HttpPost]
		public ActionResult Save()
		{
			var m = CreateModel();

			if (string.IsNullOrWhiteSpace(m.RequestBody.Optional("path", string.Empty)))
				throw IdeException.PathNotSet(this, IdeEvents.SaveProperty);

			m.Path = m.RequestBody.Optional("path", string.Empty);

			if (m.Selection.Element == null)
				throw IdeException.InvalidPath(this, IdeEvents.SaveProperty);

			if (m.Selection.Transaction == null)
				throw IdeException.NoTransactionHandler(this, IdeEvents.SaveProperty);

			var property = m.RequestBody.Optional("property", string.Empty);
			var value = m.RequestBody.Optional("value", string.Empty);
			var att = m.RequestBody.Optional("attribute", string.Empty);

			if (string.IsNullOrWhiteSpace(property))
				throw IdeException.ExpectedParameter(this, IdeEvents.SaveProperty, "property");

			var r = m.Selection.Transaction.Execute(property, att, value);

			if (r.Success)
			{
				if (!m.Commit(r.Component, property, att))
					throw IdeException.TransactionNotCommited(this, IdeEvents.SaveProperty);

				Response.Headers.Add("invalidate", r.Invalidate.ToString());

				if (m.Selection.Designer != null)
					Response.Headers.Add("designerPath", m.Selection.Designer.Path);

				Response.Headers.Add("path", m.Selection.Path);
				Response.Headers.Add("result", "1");
			}

			if (r.Data != null)
				return Json(r.Data);

			return Ok();
		}

		[HttpPost]
		public ActionResult Upload()
		{
			var m = CreateModel();

			var qs = Request.Query["path"];

			if (string.IsNullOrWhiteSpace(qs))
				throw IdeException.PathNotSet(this, IdeEvents.DesignerAction);

			var args = new JObject
			{
				{ "path", System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(qs))},
				{ "action", Request.Query["action"].ToString()}
			};

			m.RequestBody = args;
			m.Path = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(qs));

			if (m.Selection.Designer == null)
				return View("~/Views/Ide/Designers/Empty.cshtml", m);
			else
				return ResolveDesignerResult(m, m.Selection.Designer.Action(m.RequestBody));
		}

		[HttpPost]
		public ActionResult Action()
		{
			var m = CreateModel();

			if (string.IsNullOrWhiteSpace(m.RequestBody.Optional("path", string.Empty)))
				throw IdeException.PathNotSet(this, IdeEvents.DesignerAction);

			m.Path = m.RequestBody.Optional("path", string.Empty);

			if (m.Selection.Designer == null)
				return View("~/Views/Ide/Designers/Empty.cshtml", m);
			else
				return ResolveDesignerResult(m, m.Selection.Designer.Action(m.RequestBody));
		}

		[HttpPost]
		public ActionResult IdeAction()
		{
			var m = CreateModel();

			if (string.IsNullOrWhiteSpace(m.RequestBody.Optional("path", string.Empty)))
				throw IdeException.PathNotSet(this, IdeEvents.IdeAction);

			m.Path = m.RequestBody.Optional("path", string.Empty);

			return ResolveDesignerResult(m, m.Action(m.RequestBody));
		}

		private ActionResult ResolveDesignerResult(IEnvironment model, IDesignerActionResult result)
		{
			foreach (var i in result.ResponseHeaders)
				Response.Headers.Add(i.Key, i.Value);

			if (result.MessageKind != InformationKind.None)
			{
				Response.Headers.Add("messageKind", result.MessageKind.ToString());

				if (!string.IsNullOrWhiteSpace(result.Message))
					Response.Headers.Add("message", result.Message);

				if (!string.IsNullOrWhiteSpace(result.Title))
					Response.Headers.Add("title", result.Title);

				if (!string.IsNullOrWhiteSpace(result.ExplorerPath))
					Response.Headers.Add("explorerPath", result.ExplorerPath);
			}

			if (result is IDesignerActionResultJson)
			{
				Response.Headers.Add("designerResult", "json");

				return new Microsoft.AspNetCore.Mvc.JsonResult(JsonConvert.SerializeObject(((IDesignerActionResultJson)result).Data));
			}
			else if (result is IDesignerActionResultView)
			{
				var vr = result as IDesignerActionResultView;

				Response.Headers.Add("designerResult", "partial");

				return PartialView(vr.View, vr.Model);
			}
			else if (result is IDesignerActionResultSection)
			{
				var vr = result as IDesignerActionResultSection;

				Response.Headers.Add("designerResult", "section");
				Response.Headers.Add("invalidate", vr.Sections.ToString());

				if (!string.IsNullOrWhiteSpace(result.ExplorerPath) && !Response.Headers.ContainsKey("explorerPath"))
					Response.Headers.Add("explorerPath", result.ExplorerPath);

				if (model.Selection != null)
				{
					if (model.Selection.Designer != null)
						Response.Headers.Add("designerPath", model.Selection.Designer.Path);
					else
						Response.Headers.Add("designerPath", model.Selection.Path);

					Response.Headers.Add("path", model.Selection.Path);
				}

				if (vr.Data == null)
					return Ok();
				else
					return Json(vr.Data);
			}
			else
				return Ok();
		}


		protected abstract IdeModelBase CreateModel();
	}
}