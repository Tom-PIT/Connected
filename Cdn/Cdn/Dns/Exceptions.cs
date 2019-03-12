using System;

namespace TomPIT.Cdn.Dns
{
	internal class DnsQueryException : Exception
	{
		public DnsQueryException(string msg, Exception[] exs) : base(msg) { exceptions = exs; }

		private Exception[] exceptions;
	}
}
