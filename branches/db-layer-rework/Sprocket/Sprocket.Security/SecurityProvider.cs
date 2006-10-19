using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using Sprocket;
using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Security
{
	[ModuleDependency("DatabaseManager")]
	[ModuleDependency("EmailHandler")]
	[ModuleDescription("The default security implementation for Sprocket. Handles users, roles and permissions.")]
	public partial class SecurityProvider : ISprocketModule, IDataHandlerModule, ISecurityProvider
	{
		public static SecurityProvider Instance
		{
			get { return (SecurityProvider)Core.Instance["SecurityProvider"]; }
		}

		public class InitialClient
		{
			public Guid ClientID = Guid.Empty;
		}

		public delegate void InitialClientHandler(InitialClient client);
		public event InitialClientHandler BeforeFirstClientDataInserted;

		#region IDataHandlerModule Members

		public void ExecuteDataScripts()
		{
			SqlDatabase db = (SqlDatabase)Database.Main;
			db.ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Security.DatabaseScripts.sqlserver_tables_001.sql"));
			db.ExecuteScript(ResourceLoader.LoadTextResource("Sprocket.Security.DatabaseScripts.sqlserver_procedures_001.sql"));
			InitialClient client = new InitialClient();
			if (BeforeFirstClientDataInserted != null)
				BeforeFirstClientDataInserted(client);
			string sql = ResourceLoader.LoadTextResource("Sprocket.Security.DatabaseScripts.sqlserver_data_001.sql");
			sql = sql.Replace("{ClientID}", client.ClientID.ToString());
			sql = sql.Replace("{password-hash}", Crypto.EncryptOneWay("password").Replace("'", "''"));
			db.ExecuteScript(sql);
		}

		bool SelectDatabaseEngine(DatabaseEngine engine)
		{

		}

		#endregion

		#region ISprocketModule Members

		public void AttachEventHandlers(ModuleRegistry registry)
		{
		}

		public void Initialise(ModuleRegistry registry)
		{
		}

		public string RegistrationCode
		{
			get { return "SecurityProvider"; }
		}

		public string Title
		{
			get { return "Sprocket Security Provider"; }
		}

		#endregion

		#region ISecurityProvider Members

		public bool IsValidLogin(string username, string passwordHash)
		{
			DatabaseManager dbm = (DatabaseManager)Core.Instance["DatabaseManager"];
			switch (dbm.DatabaseEngine)
			{
				case DatabaseEngine.SqlServer:
					IDbCommand cmd = Database.Main.CreateCommand("IsValidLogin", CommandType.StoredProcedure);
					Database.Main.AddParameter(cmd, "@Username", username);
					Database.Main.AddParameter(cmd, "@PasswordHash", passwordHash);
					IDataParameter prm = Database.Main.AddOutputParameter(cmd, "@IsValid", DbType.Boolean);
					cmd.ExecuteNonQuery();
					return (bool)prm.Value;

				default:
					throw new SprocketException("The Sprocket Security Provider does not currently support the selected database engine.");
			}
		}

		#endregion

		public static class RoleCodes
		{
			public static string SuperUser { get { return "SUPERUSER"; } }
		}

		public static class PermissionTypeCodes
		{
			public static string UserAdministrator { get { return "USERADMINISTRATOR"; } }
			public static string RoleAdministrator { get { return "ROLEADMINISTRATOR"; } }
		}
	}
}
