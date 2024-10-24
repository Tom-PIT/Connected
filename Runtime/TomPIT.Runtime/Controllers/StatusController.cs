﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Environment;
using TomPIT.Models;
using TomPIT.Runtime;

namespace TomPIT.Controllers
{
	[AllowAnonymous]
	public class StatusController : ServerController
	{
		public IActionResult Index()
		{
			bool supportsUI = !Request.IsAjaxRequest()
				&& Shell.GetService<IRuntimeService>().SupportsUI;

			if (!RouteData.Values.ContainsKey("code"))
				return NotFound();

			var code = RouteData.Values["code"] as string;

			if (string.Compare(code, "400", true) == 0)
			{
				if (!supportsUI)
					return BadRequest();
				else
				{
					var m = CreateModel(SR.StatusBadRequest, SR.StatusBadRequestMessage, null);

					m.TryAgainEnabled = false;

					return View("~/Views/Shell/Status.cshtml", m);
				}
			}
			else if (string.Compare(code, "401", true) == 0)
			{
				if (!supportsUI)
					return Unauthorized();
				else
					return CreateLoginResult();
			}
			else if (string.Compare(code, "403", true) == 0)
			{
				if (!supportsUI)
					return Forbid();
				else
				{
					var m = CreateModel(SR.StatusForbidden, SR.StatusForbiddenMessage, SR.StatusForbiddenDescription);

					m.TryAgainEnabled = false;

					return View("~/Views/Shell/Status.cshtml", m);
				}
			}
			else if (string.Compare(code, "404", true) == 0)
			{
				if (!supportsUI)
					return NotFound();
				else
				{
					var m = CreateModel(SR.StatusNotFound, SR.StatusNotFoundMessage, string.Empty);

					m.TryAgainEnabled = false;

					return View("~/Views/Shell/Status.cshtml", m);
				}
			}
			else if (string.Compare(code, "400", true) == 0)
			{
				if (!supportsUI)
					return BadRequest();
				else
				{
					var m = CreateModel(SR.StatusBadRequest, SR.StatusBadRequestMessage, string.Empty);

					m.TryAgainEnabled = false;

					return View("~/Views/Shell/Status.cshtml", m);
				}
			}

			return Ok();
		}

		private StatusModel CreateModel(string title, string message, string description)
		{
			var r = new StatusModel
			{
				StatusTitle = title,
				Message = message,
				Description = description
			};

			r.Initialize(this, null);
			r.Databind();

			return r;
		}

		protected virtual IActionResult CreateLoginResult()
		{
			var returnUrl = string.Empty;

			var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

			if (statusCodeReExecuteFeature != null)
				returnUrl = $"returnUrl={statusCodeReExecuteFeature.OriginalPathBase}{statusCodeReExecuteFeature.OriginalPath}{statusCodeReExecuteFeature.OriginalQueryString}";

			var loginUrl = "~/login";

			if (!string.IsNullOrWhiteSpace(returnUrl))
				loginUrl += $"?{returnUrl}";

			if (Shell.GetService<IRuntimeService>().Features.HasFlag(InstanceFeatures.Application))
				return new RedirectResult(loginUrl);

			var model = new LoginModel();

			model.Initialize(this, null);
			model.Databind();

			return View("~/Views/Shell/Login.cshtml", model);
		}
	}
}
