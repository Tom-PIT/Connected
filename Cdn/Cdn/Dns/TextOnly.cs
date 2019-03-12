using System;
using System.Collections.Generic;

namespace TomPIT.Cdn.Dns
{
	internal class TextOnly : IRecordData
	{
		private List<string> _text = null;

		public TextOnly() { _text = new List<string>(); }
		public TextOnly(DataBuffer buffer)
		{
			_text = new List<string>();

			while (buffer.Next > 0)
				_text.Add(buffer.ReadCharString());
		}
		public TextOnly(DataBuffer buffer, int length)
		{
			int len = length;
			int pos = buffer.Position;
			_text = new List<string>();
			byte next = buffer.Next;

			while (length > 0)
			{
				_text.Add(buffer.ReadCharString());
				length -= next + 1;

				if (length < 0)
				{
					buffer.Position = pos - len;
					throw new DnsQueryException("Buffer Over Run in TextOnly Record Data Type", null);
				}

				next = buffer.Next;
			}

			if (length > 0)
			{
				buffer.Position = pos - len;
				throw new DnsQueryException("Buffer Under Run in TextOnly Record Data Type", null);
			}
		}

		protected string Text
		{
			get
			{
				string res = String.Empty;

				foreach (string s in _text)
					res += s + "\n";

				return res;
			}
		}
		protected int Count { get { return _text.Count; } }
		protected List<string> Strings { get { return _text; } }
		public override string ToString()
		{
			return "Text: " + Text;
		}
	}
}
