using System;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;

namespace TomPIT.Designers.CodeGeneration
{
	internal class StronglyType
	{
		private IDataSource _configuration = null;
		private StringBuilder _text = null;

		public StronglyType(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }
		public string Name { get; set; }
		public bool ReadOnly { get; set; }
		public string SourceCode { get; set; }
		public Guid DataSource { get; set; }

		private IDataSource Configuration
		{
			get
			{
				if (_configuration == null && DataSource != Guid.Empty)
					_configuration = Connection.GetService<IComponentService>().SelectConfiguration(DataSource) as IDataSource;

				return _configuration;
			}
		}

		private StringBuilder Text
		{
			get
			{
				if (_text == null)
					_text = new StringBuilder();

				return _text;
			}
		}

		public string Generate()
		{
			Text.Clear();

			if (Configuration == null)
				return Text.ToString();

			var className = string.IsNullOrWhiteSpace(Name) ? CodeGeneratonUtils.CreateIdentifierName(Configuration.ComponentName(Connection)) : Name;

			Text.AppendLine();
			Text.AppendLine(string.Format("public class {0} : DataEntity", className));
			Text.AppendLine("{");

			Text.AppendLine();
			CreateContent();
			Text.AppendLine("}");

			return Text.ToString();
		}

		private void CreateContent()
		{
			foreach (var i in Configuration.Fields)
				CreateProperty(i);
		}

		private void CreateProperty(IDataField field)
		{
			Text.AppendLine($"\t[JsonProperty(\"{field.Name[0].ToString().ToLowerInvariant()}{field.Name.Substring(1)}\")]");
			var type = Types.ToType(field.DataType);
			var identifier = CodeGeneratonUtils.CreateIdentifierName(field.Name);

			Text.AppendFormat("\tpublic {0} {1} {{ get", type.ToFriendlyName(), identifier);

			Text.Append(";");

			if (ReadOnly)
				Text.Append(" private");

			Text.AppendFormat(" set");
			Text.Append(";");

			Text.Append("}");
			Text.AppendLine();
			Text.AppendLine();
		}
	}
}
