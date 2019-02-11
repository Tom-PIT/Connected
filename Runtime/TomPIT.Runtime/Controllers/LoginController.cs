using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Models;

namespace TomPIT.Controllers
{
	[AllowAnonymous]
	public class LoginController : ServerController
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View(LoginView, CreateModel(this));
		}

		[HttpGet]
		public IActionResult Logoff()
		{
			var key = SecurityUtils.AuthenticationCookieName;

			if (Request.Cookies.ContainsKey(key))
				Response.Cookies.Delete(key);

			return View(LoginView, CreateModel(this));
		}

		protected virtual string LoginView { get { return "~/Views/Shell/Login.cshtml"; } }
		protected virtual string LoginFormView { get { return "~/Views/Shell/LoginForm.cshtml"; } }
		protected virtual string ChangePasswordView { get { return "~/Views/Shell/ChangePassword.cshtml"; } }

		[HttpPost]
		public IActionResult Authenticate()
		{
			var m = CreateModel(this);

			var body = FromBody();

			m.MapAuthenticate(body);
			m.Validate(this, body);

			if (ModelState.IsValid)
			{
				try
				{
					var ar = m.Authenticate();

					if (ar.Success)
						CreateCookie(m, ar.Token);
					else
					{
						if (ar.MustChangePassword())
							return View(ChangePasswordView, m);
						else
							ModelState.AddModelError("val", ar.GetDescription());
					}

				}
				catch (Exception ex)
				{
					ModelState.AddModelError("val", ex.Message);
				}

				if (ModelState.IsValid)
				{
					var returnUrl = Request.Query["returnUrl"];

					if (string.IsNullOrWhiteSpace(returnUrl))
					{
						var loc = body.Optional("location", string.Empty);

						if (string.IsNullOrWhiteSpace(loc))
							returnUrl = m.RootUrl();
						else
						{
							var relative = Request.HttpContext.RelativePath(loc);

							if (relative.Trim('/').StartsWith("sys/"))
								returnUrl = m.RootUrl();
							else
								returnUrl = loc;
						}
					}

					return AjaxRedirect(returnUrl);
				}
			}

			return View(LoginFormView, m);
		}

		[HttpPost]
		public IActionResult ChangePassword()
		{
			var m = CreateModel(this);

			var body = FromBody();

			m.MapChangePassword(body);
			m.Validate(this, body);

			if (ModelState.IsValid)
			{
				try
				{
					m.ChangePassword();
					var ar = m.Authenticate();

					if (ar.Success)
						CreateCookie(m, ar.Token);
					else
					{
						if (ar.MustChangePassword())
							return View(ChangePasswordView, m);
						else
							ModelState.AddModelError("val", ar.GetDescription());
					}

				}
				catch (Exception ex)
				{
					ModelState.AddModelError("val", ex.Message);
				}

				if (ModelState.IsValid)
				{
					var returnUrl = Request.Query["returnUrl"];

					if (string.IsNullOrWhiteSpace(returnUrl))
						returnUrl = m.RootUrl();

					return AjaxRedirect(returnUrl);
				}
			}

			return View(ChangePasswordView, m);
		}

		protected virtual LoginModel CreateModel(Controller controller)
		{
			var r = new LoginModel();

			r.Initialize(controller, null);
			r.Databind();

			return r;
		}

		private void CreateCookie(LoginModel model, string token)
		{
			var key = SecurityUtils.AuthenticationCookieName;

			Response.Cookies.Delete(key);

			var expiration = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(20);

			var content = new JObject
			{
				{ "jwt",token },
				{ "endpoint",model.Endpoint   },
				{ "expiration",expiration.Ticks.AsString()   }
			};

			Response.Cookies.Append(key, Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content))), new CookieOptions
			{
				HttpOnly = true,
				Expires = expiration
			}
						);
		}
	}
}