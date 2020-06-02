using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Development.Models;

namespace TomPIT.Development.Controllers
{
	public class VersionControlController : DevelopmentController
	{
		public IActionResult Index()
		{
			return View("~/Views/VersionControl/VersionControl.cshtml", new VersionControlModel());
		}

		public IActionResult Designer()
		{
			var body = FromBody();
			var designer = $"~/Views/VersionControl/{body.Required<string>("name")}.cshtml";

			return PartialView(designer, new VersionControlDesignerModel(body));
		}
		public IActionResult Changes()
		{
			return Json(new VersionControlChangesModel(FromBody()).GetChanges());
		}

		public IActionResult Diff()
		{
			return Json(new VersionControlChangesModel(FromBody()).GetDiff());
		}

		public IActionResult Commit()
		{
			new VersionControlChangesModel(FromBody()).Commit();

			return new EmptyResult();
		}

		public IActionResult Undo()
		{
			new VersionControlChangesModel(FromBody()).Undo();

			return new EmptyResult();
		}

		public IActionResult QueryActiveBindings()
		{
			return Json(new VersionControlChangesModel(FromBody()).QueryActiveBindings().OrderBy(f => f.ServiceName));
		}

		public IActionResult DesignerAction()
		{
			new VersionControlDesignerModel(FromBody()).DesignerAction();

			return new EmptyResult();
		}
	}
}
