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
		public bool ReadOnly { get; set; }
		public bool DirectBinding { get; set; }
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

			var className = CodeGeneratonUtils.CreateIdentifierName(Configuration.ComponentName(Connection));

			Text.AppendLine();
			Text.AppendLine(string.Format("public class {0} : JsonEntity", className));
			Text.AppendLine("{");

            Text.AppendLine("\t#region .ctors");

            Text.AppendFormat("\tpublic {0}()", className);
            Text.AppendLine();
            Text.AppendLine("\t{");
            Text.AppendLine();
            Text.AppendLine("\t}");
            Text.AppendLine();

            Text.AppendFormat("\tpublic {0}(JObject data) : base (data)", className);
			Text.AppendLine();
			Text.AppendLine("\t{");
			Text.AppendLine();
			Text.AppendLine("\t}");
			Text.AppendLine();
            Text.AppendLine("\t#endregion");
            Text.AppendLine();
            Text.AppendLine("\t#region properties");
            CreateContent();
            Text.AppendLine("\t#endregion");

            if (!DirectBinding)
				CreateDataBind();

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

			if (DirectBinding)
				CreateGetter(field, identifier, type);
			else
				Text.Append(";");

			if (ReadOnly)
				Text.Append(" private");

			Text.AppendFormat(" set");

			if (DirectBinding)
				CreateSetter(field, identifier);
			else
				Text.Append(";");

			Text.Append("}");
			Text.AppendLine();
            Text.AppendLine();
        }

		private void CreateSetter(IDataField field, string identifier)
		{
			Text.AppendFormat("{{ SetValue(\"{0}\", value);}}", identifier);
		}

		private void CreateGetter(IDataField field, string identifier, Type type)
		{
			Text.AppendFormat("{{ return GetValue<{1}>(\"{0}\");}}", identifier, type.ToFriendlyName());
		}

		private void CreateDataBind()
		{
			Text.AppendLine();
			Text.AppendLine("\tprotected override void OnDatabind()");
			Text.AppendLine("\t{");

			foreach (var i in Configuration.Fields)
			{
				var type = Types.ToType(i.DataType);
				var identifier = CodeGeneratonUtils.CreateIdentifierName(i.Name);

				Text.AppendLine(string.Format("\t\t{0} = GetValue<{2}>(\"{1}\");", identifier, i.Name, type.ToFriendlyName()));
			}

			Text.AppendLine("\t}");
		}
	}
}
