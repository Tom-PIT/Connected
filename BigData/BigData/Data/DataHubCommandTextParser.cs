using System;
using System.Collections.Generic;
using System.Text;

namespace Amt.DataHub.Data
{
	internal class DataHubCommandTextParser
	{
		private Dictionary<string, Tuple<string, string>> _set = null;

		public DataHubCommandTextParser(string commandText)
		{
			ParseCommand(commandText);
		}

		public Guid Partition { get; private set; }
		public string Select { get; private set; }
		public Dictionary<string, Tuple<string, string>> Set
		{
			get
			{
				if (_set == null)
					_set = new Dictionary<string, Tuple<string, string>>();

				return _set;
			}
		}
		public string Where { get; private set; }

		private void ParseCommand(string commandText)
		{
			var lines = commandText.Split('\n');

			if (lines.Length > 1)
			{
				if (string.Compare(lines[0].Trim(), "partition", true) == 0)
					Partition = new Guid(lines[1].Trim());
			}

			if (lines.Length > 3)
			{
				var line = lines[2].Trim();

				if (string.Compare(line, "set", true) == 0)
					ParseSet(lines);
				else if (string.Compare(line, "select", true) == 0)
					ParseSelect(lines, 3);
			}
		}

		private void ParseSet(string[] lines)
		{
			for (int i = 3; i < lines.Length; i++)
			{
				var line = lines[i].Trim();

				if (string.Compare(line, "select", true) == 0)
				{
					ParseSelect(lines, i + 1);

					break;
				}
				else
				{
					var parameters = line.Split(',');

					foreach (var item in parameters)
					{
						var keyValue = item.Split('=');

						if (keyValue.Length == 2)
						{
							var minMax = keyValue[1].Split(';');

							if (minMax.Length == 2)
								Set.Add(keyValue[0], new Tuple<string, string>(minMax[0], minMax[1]));
							else if (minMax.Length == 1)
								Set.Add(keyValue[0], new Tuple<string, string>(minMax[0], minMax[0]));
						}
					}
				}
			}
		}

		private void ParseSelect(string[] lines, int startIndex)
		{
			var sb = new StringBuilder();

			for (int i = startIndex; i < lines.Length; i++)
			{
				var line = lines[i].Trim();

				if (string.Compare(line, "where", true) == 0)
				{
					ParseWhere(lines, i + 1);
					Select = sb.ToString();
					break;
				}
				else
					sb.Append(line);
			}

			if (string.IsNullOrWhiteSpace(Select))
				Select = sb.ToString();

			if (string.IsNullOrWhiteSpace(Where))
				Where = string.Empty;
		}

		private void ParseWhere(string[] lines, int startIndex)
		{
			var sb = new StringBuilder();

			for (int i = startIndex; i < lines.Length; i++)
				sb.Append(lines[i]);

			Where = sb.ToString();
		}
	}
}