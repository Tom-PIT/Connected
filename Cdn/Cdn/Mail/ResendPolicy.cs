using System;
using System.Linq;

namespace TomPIT.Cdn.Mail
{
	internal class ResendPolicy
	{
		private const int Day = 1440;

		private int _delay = 5;

		public ResendPolicy(Exception ex)
		{
			if (!(ex is SmtpException smtp))
				return;

			if (string.IsNullOrWhiteSpace(ex.Message))
				return;

			var msg = ex.Message.Trim();
			var tokens = ex.Message.Split(' ');

			if (tokens == null || tokens.Length == 0)
				return;

			var mins = true;

			if (tokens.FirstOrDefault(f => string.Compare(f.Trim(), "seconds", true) == 0) != null)
				mins = false;

			for (var i = 1; i < tokens.Length; i++)
			{
				var token = tokens[i];

				if (string.Compare("4.7.1", token.Trim(), true) == 0)
					continue;

				if (int.TryParse(token, out int val))
				{
					_delay = val;

					if (!mins)
						_delay = _delay / 60;

					return;
				}

				if (TimeSpan.TryParse(token, out TimeSpan ts))
				{
					_delay = Convert.ToInt32(ts.TotalMinutes);

					if (!mins)
						_delay = _delay / 60;

					return;
				}
			}
		}

		public int Delay { get { return _delay > Day ? Day : _delay; } }
	}
}