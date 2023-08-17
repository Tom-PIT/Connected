using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

using Newtonsoft.Json;

using TomPIT.Sys.SqlDeployment.DTOs;

namespace TomPIT.Sys.SqlDeployment;

public class MsSqlSchemaDeployment
{
   public static void DeploySchema(string connectionString, string insertScriptPath, string updateScriptPath, string applicationVersion, ILogger? logger = null)
   {
      var deployer = new SchemaDeployer(connectionString, insertScriptPath, updateScriptPath, applicationVersion, logger);

      deployer.DeploySchema();
   }

   internal class SchemaDeployer
   {
      private readonly string _connectionString;
      private readonly string _insertScriptPath;
      private readonly string _updateScriptPath;
      private readonly string _applicationVersion;
      private readonly ILogger? _logger;
      
      private SchemaVersion? _schemaVersion;

      private SchemaVersion SchemaVersion => _schemaVersion ??= GetSchemaVersion();

      public SchemaDeployer(string connectionString, string insertScriptPath, string updateScriptPath, string applicationVersion, ILogger? logger)
      {
         _connectionString = connectionString;
         _insertScriptPath = insertScriptPath;
         _updateScriptPath = updateScriptPath;
         _applicationVersion = applicationVersion;
         _logger = logger;
      }

      public void DeploySchema()
      {
         using var con = new SqlConnection(_connectionString);

         try
         {
            con.Open();
         }
         catch (Exception ex)
         {
            _logger?.LogError(ex, "Error opening sql connection");
            throw;
         }

         if (SchemaVersion == SchemaVersion.Default)
            CreateDatabase(con);
         else
            UpdateDatabase(con);
      }

      private void UpdateDatabase(SqlConnection con)
      {
         var fileName = _updateScriptPath;

         if (!File.Exists(fileName))
            return;

         _logger?.LogInformation("Updating database schema. This can take a while...");

         var scripts = JsonConvert.DeserializeObject<List<ChangeScript>>(File.ReadAllText(fileName));

         if (scripts is null)
            return;

         var server = new Server(new ServerConnection(con));

         var latestVersion = SchemaVersion;
         try
         {
            server.ConnectionContext.BeginTransaction();

            foreach (var i in scripts)
            {
               if (!i.SchemaVersion.IsNewerThan(SchemaVersion))
                  continue;

               server.ConnectionContext.ExecuteNonQuery(i.Content);

               latestVersion = i.SchemaVersion;
            }

            server.ConnectionContext.CommitTransaction();

            UpdateSchemaVersion(latestVersion);
         }
         catch
         {
            server.ConnectionContext.RollBackTransaction();

            throw;
         }
      }

      private void CreateDatabase(SqlConnection con)
      {
         _logger?.LogInformation("Creating database schema. This can take a while...");

         var fileName = _insertScriptPath;

         if (!File.Exists(fileName))
            return;

         var content = File.ReadAllText(fileName);

			var server = new Server(new ServerConnection(con));

         server.ConnectionContext.MultipleActiveResultSets = false;

         server.ConnectionContext.ExecuteNonQuery(content);

			server.ConnectionContext.MultipleActiveResultSets = true;

			UpdateSchemaVersion(new SchemaVersion(_applicationVersion));
      }

      private void UpdateSchemaVersion(SchemaVersion version)
      {
         try
         {
            UpdateNewSchemaVersion(version);
         }
         catch
         {
            UpdateOldSchemaVersion(version);
         }
      }

      private void UpdateNewSchemaVersion(SchemaVersion version)
      {
         using var con = new SqlConnection(_connectionString);

         con.Open();

         var command = new SqlCommand($"IF EXISTS(SELECT * FROM tompit.setting WHERE name ='ProductVersion' AND type IS NULL AND primary_key IS NULL AND namespace IS NULL) BEGIN UPDATE tompit.setting SET value = '{version}' WHERE name = 'ProductVersion' AND type IS NULL AND primary_key IS NULL AND namespace IS NULL END ELSE BEGIN INSERT tompit.setting (name, value) VALUES ('ProductVersion', '{version}') END", con)
         {
            CommandType = System.Data.CommandType.Text
         };

         command.ExecuteNonQuery();

         con.Close();
      }

      private void UpdateOldSchemaVersion(SchemaVersion version)
      {
         using var con = new SqlConnection(_connectionString);

         con.Open();

         var command = new SqlCommand("tompit.setting_mdf", con)
         {
            CommandType = System.Data.CommandType.StoredProcedure
         };

         command.Parameters.AddWithValue("@name", "productVersion");
         command.Parameters.AddWithValue("@visible", true);
         command.Parameters.AddWithValue("@data_type", "1");
         command.Parameters.AddWithValue("@value", version.ToString());

         command.ExecuteNonQuery();

         con.Close();
      }

      private SchemaVersion GetSchemaVersion()
      {
         var _schemaVersion = (SchemaVersion?)null;

         if (_schemaVersion is null)
         {
            try
            {
               _schemaVersion = NewSchemaVersionSelect();
            }
            catch
            {
               try
               {
                  _schemaVersion = NewSchemaVersionSelect2();
               }
               catch
               {
                  try
                  {
                     _schemaVersion = OldSchemaVersionSelect();
                  }
                  catch
                  {
                     _schemaVersion = SchemaVersion.Default;
                  }
               }
            }
         }
         return _schemaVersion;
      }

      private SchemaVersion GetSchemaVersion(string commandString)
      {
         using var con = new SqlConnection(_connectionString);

         con.Open();

         var command = new SqlCommand(commandString, con);

         var r = command.ExecuteScalar();

         con.Close();

         if (r is null || r == DBNull.Value)
            return SchemaVersion.Default;

         return new SchemaVersion(r.ToString());
      }

      private SchemaVersion OldSchemaVersionSelect()
      {
         return GetSchemaVersion("SELECT value from tompit.setting WHERE resource_group IS NULL AND name='ProductVersion'");
      }

      private SchemaVersion NewSchemaVersionSelect()
      {
         return GetSchemaVersion( "SELECT value from tompit.setting WHERE name='ProductVersion' AND type IS NULL AND primary_key IS NULL AND namespace IS NULL;");
      }

      private SchemaVersion NewSchemaVersionSelect2()
      {
         return GetSchemaVersion( "SELECT value from tompit.setting WHERE name='ProductVersion' AND type IS NULL AND primary_key IS NULL;");
      }
   }
}
