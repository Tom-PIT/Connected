using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using System;
using System.Text;
using System.Text.Json;

using TomPIT.Connectivity;
using TomPIT.Sys.SqlDeployment;

namespace TomPIT.Sys.Configuration;

internal class DatabaseInitializer
{
	private string DatabaseConnectionString { get; set; }

	public string Token { get; set; }
	public string Name { get; set; } = "Default";

	public string UserName { get; set; } = "admin";

	public string Url { get; set; } = "http://localhost/sys";

	public string ApplicationVersion { get; set; }

	private string InsertScriptPath { get; set; }

	private string UpdateScriptPath { get; set; }

	public static void Initialize()
	{
		var initializer = new DatabaseInitializer();

		initializer.InitializeConfig();

		MsSqlSchemaDeployment.DeploySchema(initializer.DatabaseConnectionString, initializer.InsertScriptPath, initializer.UpdateScriptPath, initializer.ApplicationVersion, null);

		initializer.Deploy();
	}

	protected virtual void InitializeConfig()
	{
		Shell.Configuration.Bind("sys", this);

		if (Token is not null)
			Token = Encoding.UTF8.GetString(Convert.FromBase64String(Token));

		DatabaseConnectionString = ConnectionStringsConfiguration.Sys;

		ApplicationVersion = Shell.Version.ToString();

		InsertScriptPath = Shell.Configuration.GetValue<string>("sqlScriptPaths:create", "./create.sql");
		UpdateScriptPath = Shell.Configuration.GetValue<string>("sqlScriptPaths:update", "./update.json");
	}

	private void Deploy()
	{
		using var con = new SqlConnection(DatabaseConnectionString);

		con.Open();

		var user = GetUserId(con);

		if (user == 0)
			user = CreateUser(con); ;

		if (!HasMembership(con))
			AddMembership(con, user);

		var resourceGroup = GetResourceGroup(con);

		if (resourceGroup == 0)
			resourceGroup = CreateResourceGroup(con);

		if (!HasAuthenticationToken(con))
			CreateAuthenticationToken(con, user, resourceGroup);
	}

	private static int GetResourceGroup(SqlConnection connection)
	{
		try
		{
			return Convert.ToInt32(new SqlCommand("SELECT TOP 1 id FROM tompit.resource_group", connection).ExecuteScalar());
		}
		catch
		{
			return default;
		}
	}

	private static bool HasMembership(SqlConnection connection)
	{
		var com = new SqlCommand("SELECT count(*) FROM tompit.membership", connection);

		return Convert.ToInt32(com.ExecuteScalar()) > 0;
	}

	private static bool HasAuthenticationToken(SqlConnection connection)
	{
		var com = new SqlCommand("SELECT count(*) FROM tompit.auth_token", connection);

		return Convert.ToInt32(com.ExecuteScalar()) > 0;
	}

	private static int GetUserId(SqlConnection connection)
	{
		try
		{
			return Convert.ToInt32(new SqlCommand("SELECT TOP 1 id FROM tompit.[user]", connection).ExecuteScalar());
		}
		catch
		{
			return default;
		}
	}

	private int CreateResourceGroup(SqlConnection connection)
	{
		var c = new SqlCommand("tompit.resource_group_ins", connection)
		{
			CommandType = System.Data.CommandType.StoredProcedure
		};

		var p = c.Parameters.AddWithValue("@token", new Guid("{E14372D1-17CD-48D6-BC29-D57C397AF87C}"));

		p.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;

		c.Parameters.AddWithValue("@name", "Default");
		c.Parameters.AddWithValue("@storage_provider", new Guid("E8BC6674718241D0ADA31835BDA71E36"));
		c.Parameters.AddWithValue("@connection_string", null);

		c.ExecuteNonQuery();

		return Convert.ToInt32(new SqlCommand("SELECT TOP 1 id FROM tompit.resource_group", connection).ExecuteScalar());
	}

	private void CreateAuthenticationToken(SqlConnection connection, int user, int resourceGroup)
	{
		var c = new SqlCommand("tompit.auth_token_ins", connection)
		{
			CommandType = System.Data.CommandType.StoredProcedure
		};

		c.Parameters.AddWithValue("@token", Guid.NewGuid());
		c.Parameters.AddWithValue("@key", Token);
		c.Parameters.AddWithValue("@claims", 2147483647);
		c.Parameters.AddWithValue("@status", 1);
		c.Parameters.AddWithValue("@resource_group", resourceGroup);
		c.Parameters.AddWithValue("@user", user);
		c.Parameters.AddWithValue("@name", "Sys");

		c.ExecuteNonQuery();
	}

	private int CreateUser(SqlConnection connection)
	{
		var c = new SqlCommand("tompit.user_ins", connection)
		{
			CommandType = System.Data.CommandType.StoredProcedure
		};

		c.Parameters.AddWithValue("@token", Guid.NewGuid());
		c.Parameters.AddWithValue("@url", UserName.ToLowerInvariant());
		c.Parameters.AddWithValue("@status", 1);
		c.Parameters.AddWithValue("@notification_enabled", true);
		c.Parameters.AddWithValue("@login_name", UserName);
		c.Parameters.AddWithValue("@auth_token", Guid.NewGuid());

		c.ExecuteNonQuery();

		return Convert.ToInt32(new SqlCommand("SELECT TOP 1 id FROM tompit.[user]", connection).ExecuteScalar());
	}

	private static void AddMembership(SqlConnection connection, int user)
	{
		var c = new SqlCommand("tompit.membership_ins", connection)
		{
			CommandType = System.Data.CommandType.StoredProcedure
		};

		c.Parameters.AddWithValue("@user", user);
		c.Parameters.AddWithValue("@role", new Guid("C82BBDAD-E913-4779-8771-981349467860"));

		c.ExecuteNonQuery();
	}

	private class ConfigurationBindings
	{
		public string? Name { get; set; }
		public string? Url { get; set; }
		public string? Token { get; set; }
	}
}
