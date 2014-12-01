using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

namespace DapperSimple.Data

{
    public class DataConfig
    {

        public static string GetPrimaryConnectionString()
        {
            var connectionStringKey = "DefaultConnection";

            if (ConfigurationManager.ConnectionStrings[connectionStringKey] == null)
                throw new ConfigurationErrorsException(
                    "Missing connectionstring for key '" + connectionStringKey + "', add one to the web.config");
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;

            return connectionString;
        }

        public static string GetPrimaryConnectionStringKey()
        {
            return "DefaultConnection";
        }



        public static void EnsureDatabaseIsAvailable(string connectionStringKey, string applicationName, bool createLocalDbIfMissing, MigrationsAssembly migrationsAssembly = null)
        {

            // Check if ConnectionString exists
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringKey];
            if (connectionStringSetting == null)
            {
                var configurationError =
                    string.Format("Missing connectionstring '{0}', add one to the app.config for the test project\r\n" +
                                  "E.g.: <add name=\"{0}\" connectionString=\"Data Source=(LocalDb)\\v11.0;Initial Catalog={1};Integrated Security=SSPI;" +
                                  "AttachDBFilename=|DataDirectory|\\{1}.mdf\" providerName=\"System.Data.SqlClient\" />",
                                  connectionStringKey, applicationName);
                throw new Exception(configurationError);
            }
            var connectionString = connectionStringSetting.ConnectionString;

            try
            {
                var databaseVerified = VerifyDatabase(connectionString, createLocalDbIfMissing, migrationsAssembly);
                if (!databaseVerified)
                    throw new Exception(
                        string.Format("Failed to migrate the database"));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to migrate the database"), ex);
            }
        }

        /// <summary>
        /// Verifies that the database exists and creates it if it doesn't
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="createLocalDbIfMissing">True if a LocalDB should be created if an existing cannot be found</param>
        /// <param name="migrationsAssembly">A reference to the assemby containing the database migrations (e.g. MigrationsAssembly.FromType&lt;</param>
        /// <returns></returns>
        private static bool VerifyDatabase(string connectionString, bool createLocalDbIfMissing, MigrationsAssembly migrationsAssembly = null)
        {
            if (AppDomain.CurrentDomain.GetData("DataDirectory") == null)
            {
                AppDomain.CurrentDomain.SetData(
"DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ""));
            }

            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var isLocalDb = sqlConnectionStringBuilder.DataSource.StartsWith("(LocalDb)");
            if (isLocalDb)
            {
                try
                {
                    var dbName = sqlConnectionStringBuilder.InitialCatalog;
                    var dbFileName = sqlConnectionStringBuilder.AttachDBFilename;
                    if (dbFileName.Contains("|DataDirectory|"))
                    {
                        // Replace it with the resolved path
                        var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                        dbFileName = dbFileName.Replace("|DataDirectory|", dataDirectory);
                        sqlConnectionStringBuilder.AttachDBFilename = dbFileName;
                    }

                    var outputFolder = Path.GetDirectoryName(dbFileName) ?? "";
                    var resolvedConnectionString = sqlConnectionStringBuilder.ToString();

                    // Create Data Directory If It Doesn't Already Exist.
                    if (!Directory.Exists(outputFolder) && createLocalDbIfMissing)
                    {
                        Directory.CreateDirectory(outputFolder);
                    }

                    // If the database does not already exist, create it.
                    if (!File.Exists(dbFileName))
                    {
                        if (createLocalDbIfMissing)
                            if (!CreateDatabase(resolvedConnectionString)) return false;
                    }

                    if (migrationsAssembly != null)
                        RunMigrations(resolvedConnectionString, migrationsAssembly);

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to verify the database", ex);
                }
            }
            else
            {
                var masterConnectionString = GetMasterConnectionString(connectionString);

                using (var connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = string.Format("SELECT count(*) FROM sys.databases WHERE Name = '{0}'",
                        sqlConnectionStringBuilder.InitialCatalog);
                    if ((int) cmd.ExecuteScalar() == 0)
                    {
                        cmd = connection.CreateCommand();
                        cmd.CommandText = String.Format("CREATE DATABASE [{0}]", sqlConnectionStringBuilder.InitialCatalog);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
          

            try
            {
                if (migrationsAssembly != null)
                    RunMigrations(connectionString, migrationsAssembly);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to run migrations on the database", ex);
            }

            return false;
        }

        private static string GetMasterConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "master";
            return builder.ConnectionString;
        }

        /// <summary>
        /// Creates the localDd database in the file specified by the connectionstring
        /// </summary>
        /// <param name="connectionString">Connectionstring for a localDb including Initial Catalog and AttachDBFilename</param>
        /// <returns></returns>
        private static bool CreateDatabase(string connectionString)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var dbFileName = sqlConnectionStringBuilder.AttachDBFilename;
            var dbName = sqlConnectionStringBuilder.InitialCatalog;
            var localDbConnectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");

            try
            {
                using (var connection = new SqlConnection(localDbConnectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
   

                    Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
                    DetachDatabase(connectionString);

                    cmd.CommandText = String.Format("CREATE DATABASE [{0}] ON (NAME = N'{0}', FILENAME = '{1}')", dbName, dbFileName);
                    cmd.ExecuteNonQuery();
                }

                return File.Exists(dbFileName);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to create database {1} in file {0}", dbFileName, dbName);
                throw new Exception(message, ex);
            }
        }

        /// <summary>
        /// Detatches a localDb database file specified by the AttachDBFilename in the connectionstring
        /// </summary>
        /// <param name="connectionString">Connectionstring for a localDb including Initial Catalog and AttachDBFilename</param>
        /// <returns></returns>
        private static bool DetachDatabase(string connectionString)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var dbFileName = sqlConnectionStringBuilder.AttachDBFilename;
            var dbName = sqlConnectionStringBuilder.InitialCatalog;
            var localDbConnectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");

            try
            {
                using (var connection = new SqlConnection(localDbConnectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = String.Format("exec sp_detach_db '{0}'", dbName);
                    cmd.ExecuteNonQuery();


                    return true;
                }


            }
            catch (Exception ex)
            {
                //throw new Exception(string.Format("Failed to detach the database {1} in file {0}", dbFileName, dbName), ex);
                return false;
            }
        }

        /// <summary>
        /// Migrates the database specified by the connectionstring to the latest version of migrations
        /// </summary>
        /// <param name="connectionString">Connectionstring for a localDb including Initial Catalog and AttachDBFilename</param>
        /// <returns></returns>
        private static bool MigrateDatabase(string connectionString)
        {
            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var dbFileName = sqlConnectionStringBuilder.AttachDBFilename;
            var dbName = sqlConnectionStringBuilder.InitialCatalog;
            var localDbConnectionString = String.Format(@"Data Source=(LocalDB)\v11.0;Initial Catalog=master;Integrated Security=True");

            try
            {
                using (var connection = new SqlConnection(localDbConnectionString))
                {
                    connection.Open();
                    var cmd = connection.CreateCommand();

                    DetachDatabase(connectionString);

                    cmd.CommandText = String.Format("CREATE DATABASE [{0}] ON (NAME = N'{0}', FILENAME = '{1}')", dbName, dbFileName);
                    cmd.ExecuteNonQuery();
                }

                return File.Exists(dbFileName);
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to migrate {1} in file {0}", dbFileName, dbName);
                throw new Exception(message, ex);
            }
        }

        // Runs all migrations in the assembly of specified type
        public static void RunMigrations(string connectionString, MigrationsAssembly migrationsAssembly, MigrationProcessorFactory factory = null)
        {
            try
            {
               
                var announcer = new NullAnnouncer();
                //var announcer = new TextWriterAnnouncer(s => System.Diagnostics.Debug.WriteLine(s));
                var assembly = migrationsAssembly.Assembly;

                var migrationContext = new RunnerContext(announcer);

                var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
            
                var processor = (factory ?? new SqlServerProcessorFactory()).Create(connectionString, announcer, options);
                var runner = new MigrationRunner(assembly, migrationContext, processor);
                runner.MigrateUp(useAutomaticTransactionManagement: true);

                processor.Dispose();
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to run migrations for {0}", connectionString);
                throw new Exception(message, ex);
            }
        }
    }


    /// <summary>
    /// Class to hold options for the MigrationRunner
    /// </summary>
    public class MigrationOptions : IMigrationProcessorOptions
    {
        public bool PreviewOnly { get; set; }
        public int Timeout { get; set; }
        public string ProviderSwitches { get; private set; }
    }

    /// <summary>
    /// Class to describe which assembly to load the Migrations from
    /// </summary>
    public class MigrationsAssembly
    {
        public static MigrationsAssembly FromAssemblyName(string assemblyName)
        {
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            return new MigrationsAssembly() { Assembly = assembly };
        }

        public static MigrationsAssembly FromClass<T>()
        {
            return new MigrationsAssembly() { Assembly = typeof(T).Assembly };
        }

        public Assembly Assembly { get; set; }
    }
}