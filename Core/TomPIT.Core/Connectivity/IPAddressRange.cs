using System.Net;
using System.Net.Sockets;

namespace TomPIT.Connectivity
{
	public class IPAddressRange
	{
		private readonly AddressFamily _addressFamily;
		private readonly byte[] _lowerBytes;
		private readonly byte[] _upperBytes;

		public IPAddressRange(IPAddress lower, IPAddress upper)
		{
			_addressFamily = lower.AddressFamily;
			_lowerBytes = lower.GetAddressBytes();
			_upperBytes = upper.GetAddressBytes();
		}

		public bool IsInRange(IPAddress address)
		{
			if (address.AddressFamily != _addressFamily)
				return false;

			var addressBytes = address.GetAddressBytes();
			var lowerBoundary = true;
			var upperBoundary = true;

			for (var i = 0; i < _lowerBytes.Length && (lowerBoundary || upperBoundary); i++)
			{
				if ((lowerBoundary && addressBytes[i] < _lowerBytes[i]) || (upperBoundary && addressBytes[i] > _upperBytes[i]))
					return false;

				lowerBoundary &= (addressBytes[i] == _lowerBytes[i]);
				upperBoundary &= (addressBytes[i] == _upperBytes[i]);
			}

			return true;
		}

		public static bool Check(IPAddress address, string values)
		{
			if (string.IsNullOrWhiteSpace(values))
				return true;

			var tokens = values.Split(',');

			foreach (var i in tokens)
			{
				if (i.Contains('-'))
				{
					var range = i.Split('-');

					if (range.Length != 2)
						continue;

					if (IPAddress.TryParse(range[0], out IPAddress low) && IPAddress.TryParse(range[1], out IPAddress high))
					{
						var r = new IPAddressRange(low, high);

						if (r.IsInRange(address))
							return true;
					}
				}
				else
				{
					if (IPAddress.TryParse(i, out IPAddress ip))
					{
						if (ip.Equals(address))
							return true;
					}
				}
			}

			return false;
		}
	}
}
