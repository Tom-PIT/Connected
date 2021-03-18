using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.DataProviders.Sql.Parsing
{

	internal class CommandTextDescriptor : ICommandTextDescriptor
	{
		private List<ICommandTextParameter> _parameters = null;
		private List<ICommandTextVariable> _variables = null;
		public CommandTextType Type { get; private set; } = CommandTextType.Text;
		public OperationType Statement { get; private set; } = OperationType.Other;
		public string Name { get; private set; }
		private string Sql { get; set; }

		private List<ICommandTextParameter> CommandParameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<ICommandTextParameter>();

				return _parameters;
			}

		}

		public ImmutableArray<ICommandTextParameter> Parameters => CommandParameters.ToImmutableArray();

		public ImmutableArray<ICommandTextVariable> Variables => CommandVariables.ToImmutableArray();
		private List<ICommandTextVariable> CommandVariables
		{
			get
			{
				if (_variables == null)
					_variables = new List<ICommandTextVariable>();

				return _variables;
			}
		}

		public string CommandText => Type == CommandTextType.Procedure ? Name : Sql;

		public bool SupportsConcurrency { get; private set; }

		public void Parse(string sql)
		{
			Sql = sql;

			var result = Parser.Parse(Sql);

			if (result.Errors.Count() > 0)
			{
				var sb = new StringBuilder();

				foreach (var error in result.Errors)
					sb.AppendLine(error.Message);

				throw new RuntimeException(Name, sb.ToString(), LogCategories.Deployment);
			}

			foreach (var batch in result.Script.Batches)
			{
				foreach (var statement in batch.Statements)
					DiscoverChildren(statement);
			}

			Merge();
		}

		private void Merge()
		{
			if (Type == CommandTextType.Procedure)
				return;

			foreach (var variable in CommandVariables)
			{
				if (!variable.Bound)
					AddParameter(variable.Name, null, null);
			}
		}

		private void DiscoverChildren(SqlCodeObject sql)
		{
			if (sql is SqlCreateAlterProcedureStatementBase)
				Type = CommandTextType.Procedure;
			else if (sql is SqlCreateAlterViewStatementBase)
			{
				Type = CommandTextType.View;
				Statement = OperationType.Select;
			}

			if (sql is SqlProcedureDefinitionForCreate create)
				Name = create.Name.Sql;
			else if (sql is SqlViewDefinition view)
				Name = view.Name.Sql;

			if (Statement == OperationType.Other)
			{
				if (sql is SqlSelectStatement)
					Statement = OperationType.Query;
				else if (sql is SqlDeleteStatement)
					Statement = OperationType.Delete;
				else if (sql is SqlUpdateStatement)
					Statement = OperationType.Update;
				else if (sql is SqlInsertStatement)
					Statement = OperationType.Insert;
			}

			if (sql is SqlTopSpecification top && Statement == OperationType.Query)
			{
				if (top.Value is SqlScalarExpression scalar)
				{
					if (Types.TryConvert(scalar.Sql, out int topn) && topn == 1)
						Statement = OperationType.Select;
				}
			}

			if (sql is SqlScalarVariableRefExpression var)
				AddVariable(var.VariableName);
			else if (sql is SqlScalarVariableRefExpression scalar)
				AddVariable(scalar.VariableName);
			else
			{
				if (sql is SqlParameterDeclaration p)
					AddParameter(p.Name, p.Type.DataType, p.Value);
				else if (sql is SqlVariableDeclaration vard)
					BindVariable(vard.Name);
			}

			foreach (var child in sql.Children)
				DiscoverChildren(child);
		}

		private void BindVariable(string name)
		{
			if (CommandVariables.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0) == null)
				AddVariable(name);

			var variable = CommandVariables.First(f => string.Compare(f.Name, name, true) == 0) as CommandTextVariable;

			variable.Bound = true;
		}

		private void AddVariable(string name)
		{
			if (CommandVariables.FirstOrDefault(f => string.Compare(name, f.Name, true) == 0) != null)
				return;

			CommandVariables.Add(new CommandTextVariable
			{
				Name = name
			});
		}

		private void AddParameter(string name, SqlDataType type, SqlScalarExpression value)
		{
			if (CommandParameters.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0) != null)
				return;

			object v = null;

			if (value != null && value is SqlLiteralExpression literal)
			{
				switch (literal.Type)
				{
					case LiteralValueType.Binary:
						v = literal.Value;
						break;
					case LiteralValueType.Default:
						v = literal.Value;
						break;
					case LiteralValueType.Identifier:
						v = literal.Value;
						break;
					case LiteralValueType.Integer:
						v = literal.Value;
						break;
					case LiteralValueType.Image:
						v = literal.Value;
						break;
					case LiteralValueType.Money:
						v = literal.Value;
						break;
					case LiteralValueType.Null:
						v = DBNull.Value;
						break;
					case LiteralValueType.Numeric:
						v = literal.Value;
						break;
					case LiteralValueType.Real:
						v = literal.Value;
						break;
					case LiteralValueType.String:
						v = literal.Value;
						break;
					case LiteralValueType.UnicodeString:
						v = literal.Value;
						break;
					default:
						break;
				}
			}

			var resolvedType = ResolveDataType(type);

			CommandParameters.Add(new CommandTextParameter
			{
				Name = name,
				Type = resolvedType.Item1,
				Value = v
			});

			if (resolvedType.Item2)
				SupportsConcurrency = true;
		}

		private (DbType, bool) ResolveDataType(SqlDataType type)
		{
			if (type == null)
				return (DbType.String, false);

			Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType dbType;

			try
			{
				dbType = type.GetTypeSpec().SqlDataType;
			}
			catch
			{
				throw new RuntimeException(Name, $"{SR.ErrCannotResolveDatabaseType} ({type.ObjectIdentifier})", LogCategories.Deployment);
			}

			switch (dbType)
			{
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.None:
					return (DbType.Object, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.BigInt:
					return (DbType.Int64, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Binary:
					return (DbType.Binary, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Bit:
					return (DbType.Boolean, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Char:
					return (DbType.StringFixedLength, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Date:
					return (DbType.Date, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.DateTime:
					return (DbType.DateTime, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.DateTime2:
					return (DbType.DateTime2, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.DateTimeOffset:
					return (DbType.DateTimeOffset, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Decimal:
					return (DbType.Decimal, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Float:
					return (DbType.Single, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Geography:
					return (DbType.Object, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Geometry:
					return (DbType.Object, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.HierarchyId:
					return (DbType.Object, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Image:
					return (DbType.Binary, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Int:
					return (DbType.Int32, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Money:
					return (DbType.Decimal, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.NChar:
					return (DbType.StringFixedLength, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.NText:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Numeric:
					return (DbType.Double, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.NVarChar:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.NVarCharMax:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Real:
					return (DbType.Decimal, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.SmallDateTime:
					return (DbType.DateTime, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.SmallInt:
					return (DbType.Int16, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.SmallMoney:
					return (DbType.Decimal, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.SysName:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Text:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Time:
					return (DbType.Time, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Timestamp:
					return (DbType.Int64, true);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.TinyInt:
					return (DbType.Byte, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.UniqueIdentifier:
					return (DbType.Guid, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.VarBinary:
					return (DbType.Binary, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.VarBinaryMax:
					return (DbType.Binary, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.VarChar:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.VarCharMax:
					return (DbType.String, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Variant:
					return (DbType.Object, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.Xml:
					return (DbType.Xml, false);
				case Microsoft.SqlServer.Management.SqlParser.Metadata.SqlDataType.XmlNode:
					return (DbType.String, false);
				default:
					return (DbType.String, false);
			}
		}
	}
}
