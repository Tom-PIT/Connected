using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;

namespace TomPIT.Connectivity
{
	public class ServerUrl
	{
		private List<Tuple<string, string>> _parameters = null;
		public string Controller { get; set; }
		public string Action { get; set; }

		public string Url
		{
			get
			{
				var serverUrl = ParseUrl(Controller, Action);
				var qs = QueryString();

				if (string.IsNullOrWhiteSpace(qs))
					return serverUrl;

				var kind = string.IsNullOrWhiteSpace(BaseUrl) ? UriKind.Relative : UriKind.Absolute;

				var uri = new Uri(string.Format("{0}?{1}", serverUrl, qs), kind);

				return uri.AbsoluteUri;
			}
		}

		protected virtual string BaseUrl { get; set; }
		protected virtual string ParseUrl(string controller, string action)
		{
			if (string.IsNullOrWhiteSpace(BaseUrl))
				return string.Format("/{0}/{1}", Controller, Action);
			else
				return string.Format("{0}/{1}/{2}", BaseUrl, Controller, Action);
		}

		public List<Tuple<string, string>> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<Tuple<string, string>>();

				return _parameters;
			}
		}

		public string QueryString()
		{
			var sb = new StringBuilder();

			foreach (var i in Parameters)
				sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(i.Item1), HttpUtility.UrlEncode(i.Item2));

			return sb.ToString().TrimEnd('&');
		}

		public ServerUrl AddParameterFromArray<T>(string name, params T[] value)
		{
			if (value.Length == 0)
				return this;

			var sb = new StringBuilder();

			foreach (var i in value)
			{
				var v = ConvertValue(i);

				if (string.IsNullOrWhiteSpace(v))
					continue;

				sb.AppendFormat("{0};", v);
			}

			return AddParameter(name, sb.ToString());
		}

		public ServerUrl AddParameter(string name, object value)
		{
			var v = ConvertValue(value);

			if (string.IsNullOrWhiteSpace(v))
				return this;

			Parameters.Add(new Tuple<string, string>(name, v));

			return this;
		}

		private string ConvertValue(object value)
		{
			if (value == null || value == DBNull.Value)
				return string.Empty;

			if (value is DateTime)
				value = ((DateTime)value).Ticks;

			if (value is IModelBindable)
				return ((IModelBindable)value).Serialize();
			else
			{
				var converter = TypeDescriptor.GetConverter(value.GetType());

				if (converter == null || !converter.CanConvertTo(typeof(string)))
					return value.ToString();

				return converter.ConvertToInvariantString(value);
			}
		}
		public static ServerUrl Create(string baseUrl, string controller, string action)
		{
			return new ServerUrl
			{
				Controller = controller,
				Action = action,
				BaseUrl = baseUrl
			};
		}

		public static ServerUrl Create(string controller, string action)
		{
			return Create(null, controller, action);
		}

		public static implicit operator string(ServerUrl d)
		{
			if (d == null)
				return null;

			return d.Url;
		}
	}
}