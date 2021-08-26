using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public abstract class StreamOperation : MiddlewareApiOperation, IOperation
	{
		public void Invoke()
		{
			Invoke(null);
		}
		public void Invoke(IMiddlewareContext context)
		{
			if (context != null)
				this.WithContext(context);

			try
			{
				Validate();
				OnValidating();

				if (Context.Environment.IsInteractive)
				{
					AuthorizePolicies();
					OnAuthorize();
					OnAuthorizing();
				}

				OnInvoke();
				DependencyInjections.Invoke<object>(null);

				Invoked();
			}
			catch (ValidationException)
			{
				Rollback();
				throw;
			}
			catch (Exception ex)
			{
				Rollback();

				throw TomPITException.Unwrap(this, ex);
			}
		}

		protected virtual void OnInvoke()
		{
		}

		protected virtual void OnAuthorize()
		{
		}

		protected HttpContext HttpContext => Shell.HttpContext;
		protected bool HasBeenModified(DateTime date)
		{
			if (HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

			if (!string.IsNullOrEmpty(HttpContext.Request.Headers["If-Modified-Since"]))
			{
				var provider = CultureInfo.InvariantCulture;
				var lastMod = DateTime.ParseExact(HttpContext.Request.Headers["If-Modified-Since"], "r", provider).ToLocalTime();

				if (lastMod == date)
				{
					HttpContext.Response.StatusCode = 304;

					return false;
				}
			}
			else if (string.IsNullOrEmpty(HttpContext.Request.Headers["ETag"]))
			{
				var lastMod = new DateTime(Convert.ToInt64(HttpContext.Request.Headers["If-Modified-Since"]));

				if (lastMod == date)
				{
					HttpContext.Response.StatusCode = 304;

					return false;
				}
			}
			return true;
		}

		protected void SetModified(DateTime date, int maxAge = 600)
		{
			if (HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

			HttpContext.Response.Headers["Last-Modified"] = date.ToUniversalTime().ToString("r");
			HttpContext.Response.Headers["ETag"] = date.ToUniversalTime().Ticks.ToString();
			HttpContext.Response.Headers["Cache-Control"] = $"public, max-age={maxAge}";
		}

		protected void Write(StreamOperationWriteArgs e)
		{
			if (HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			if (e.Content == null || e.Content.Length == 0)
				return;

			if (e.Modified != DateTime.MinValue && !HasBeenModified(e.Modified))
				return;

			HttpContext.Response.ContentType = e.ContentType;

			if (e.Modified != DateTime.MinValue)
				SetModified(e.Modified);

			var cd = new ContentDisposition
			{
				FileName = e.FileName,
				Inline = e.Inline
			};

			HttpContext.Response.Headers.Append("Content-Disposition", cd.ToString());
			HttpContext.Response.ContentLength = e.Content.Length;
			HttpContext.Response.Body.WriteAsync(e.Content, 0, e.Content.Length).Wait();

			HttpContext.Response.CompleteAsync().Wait();
		}
	}
}
