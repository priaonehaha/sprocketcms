using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Sprocket.Data;
using Sprocket.Utility;
using Sprocket.SystemBase;
using System.Net;
using System.Net.Mail;
using Sprocket.Mail;
using System.Text.RegularExpressions;

namespace Sprocket.Security
{
	public partial class SecurityProvider
	{
		public class Client : DataEntity
		{
			private Guid clientID;
			private Guid? primaryUserID, ownerClientID;
			private bool enabled;
			private string name;

			#region Constructor
			public Client(string name, bool enabled, Guid? ownerClientID, Guid? primaryUserID)
			{
				this.name = name;
				this.enabled = enabled;
				this.ownerClientID = ownerClientID;
				this.primaryUserID = primaryUserID;
				this.clientID = Guid.NewGuid();
			}

			public Client(DataRow row)
			{
				Load(row);
			}

			public Client(Guid clientID)
			{
				Database.Main.RememberOpenState();
				IDbCommand cmd = Database.Main.CreateCommand("ListClients", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@ClientID", clientID);
				DataSet ds = Database.Main.GetDataSet(cmd);
				Database.Main.CloseIfWasntOpen();
				if (ds.Tables[0].Rows.Count == 0)
					throw new SprocketException("The request client (ID " + clientID + ") doesn't exist in the database.");
				Load(ds.Tables[0].Rows[0]);
			}

			private void Load(DataRow row)
			{
				if (row["clientID"] != DBNull.Value) clientID = (Guid)row["ClientID"];
				primaryUserID = row["PrimaryUserID"] == DBNull.Value ? null : (Guid?)row["PrimaryUserID"];
				ownerClientID = row["OwnerClientID"] == DBNull.Value ? null : (Guid?)row["OwnerClientID"];
				if (row["enabled"] != DBNull.Value) enabled = (bool)row["Enabled"];
				if (row["name"] != DBNull.Value) name = (string)row["Name"];
				IsNew = false;
				WasNew = false;
			}
			#endregion

			#region Class Properties

			public Guid ClientID
			{
				get { return clientID; }
				set { clientID = value; }
			}

			public Guid? PrimaryUserID
			{
				get { return primaryUserID; }
				set { primaryUserID = value; }
			}

			public Guid? OwnerClientID
			{
				get { return ownerClientID; }
				set { ownerClientID = value; }
			}

			public bool Enabled
			{
				get { return enabled; }
				set { enabled = value; }
			}

			public string Name
			{
				get { return name; }
				set { name = value; }
			}

			#endregion
		}

		public class User : DataEntity
		{
			#region Fields
			protected Guid userID = Guid.Empty, clientID = Guid.Empty;
			protected string username = "", passwordHash = "";
			protected string firstName = "", surname = "", email = "";
			protected bool enabled = true, hidden = false, locked = false, activated = false;
			protected int localTimeOffsetHours = 0;
			protected DateTime? activationReminderSent = null, created = null;

			private Dictionary<string, Permission> permissions = null;
			private Dictionary<string, Permission> roles = null;
			#endregion

			#region Constructor
			public User() { }

			public User(Guid clientID, string username,
				string password, string firstName, string surname, string email,
				bool enabled, bool locked, bool hidden)
			{
				this.userID = Guid.NewGuid();
				this.clientID = clientID;
				this.username = username;
				this.passwordHash = Crypto.EncryptOneWay(password);
				this.firstName = firstName;
				this.surname = surname;
				this.email = email;
				this.enabled = enabled;
				this.locked = locked;
				this.hidden = hidden;
				this.activated = false;
				this.created = null;
				this.activationReminderSent = null;
			}

			public User(DataRow row)
			{
				if (row["UserID"] != DBNull.Value) userID = (Guid)row["UserID"];
				if (row["ClientID"] != DBNull.Value) clientID = (Guid)row["ClientID"];
				if (row["Username"] != DBNull.Value) username = (string)row["Username"];
				if (row["PasswordHash"] != DBNull.Value) passwordHash = (string)row["PasswordHash"];
				if (row["FirstName"] != DBNull.Value) firstName = (string)row["FirstName"];
				if (row["Surname"] != DBNull.Value) surname = (string)row["Surname"];
				if (row["Email"] != DBNull.Value) email = (string)row["Email"];
				if (row["Enabled"] != DBNull.Value) enabled = (bool)row["Enabled"];
				if (row["Hidden"] != DBNull.Value) hidden = (bool)row["Hidden"];
				if (row["Locked"] != DBNull.Value) locked = (bool)row["Locked"];
				if (row["Activated"] != DBNull.Value) activated = (bool)row["Activated"];
				if (row["ActivationReminderSent"] != DBNull.Value) activationReminderSent = (DateTime)row["ActivationReminderSent"];
				if (row["Created"] != DBNull.Value) created = (DateTime)row["Created"];
				IsNew = false;
				WasNew = false;
			}
			#endregion

			#region Static Methods

			public static User Load(Guid userID)
			{
				IDbCommand cmd = Database.Main.CreateCommand("ListUsers", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				DataSet ds = Database.Main.GetDataSet(cmd);
				if (ds.Tables[0].Rows.Count == 0)
					return null;
				User user = new User(ds.Tables[0].Rows[0]);
				return user;
			}

			public static User Load(Guid clientID, string username)
			{
				IDbCommand cmd = Database.Main.CreateCommand("ListUsers", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@ClientID", clientID);
				Database.Main.AddParameter(cmd, "@Username", username);
				Database.Main.AddParameter(cmd, "@ExactMatches", true);
				DataSet ds = Database.Main.GetDataSet(cmd);
				if (ds.Tables[0].Rows.Count == 0)
					return null;
				User user = new User(ds.Tables[0].Rows[0]);
				return user;
			}

			public static User LoadByEmail(Guid clientID, string emailAddress)
			{
				IDbCommand cmd = Database.Main.CreateCommand("ListUsers", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@ClientID", clientID);
				Database.Main.AddParameter(cmd, "@Email", emailAddress);
				Database.Main.AddParameter(cmd, "@ExactMatches", true);
				DataSet ds = Database.Main.GetDataSet(cmd);
				if (ds.Tables[0].Rows.Count == 0)
					return null;
				User user = new User(ds.Tables[0].Rows[0]);
				return user;
			}

			public static User[] BasicSearch(string partUsername, string partFirstName, string partSurname, string partEmail, int? maxResults, Guid? editableByUserID, out int totalMatches, bool? activated)
			{
				if (maxResults == 0) maxResults = null;
				if (partUsername == "") partUsername = null;
				if (partFirstName == "") partFirstName = null;
				if (partSurname == "") partSurname = null;
				IDbCommand cmd = Database.Main.CreateCommand("ListUsers", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@Username", partUsername);
				Database.Main.AddParameter(cmd, "@FirstName", partFirstName);
				Database.Main.AddParameter(cmd, "@Surname", partSurname);
				Database.Main.AddParameter(cmd, "@Email", partEmail);
				Database.Main.AddParameter(cmd, "@Activated", activated);
				Database.Main.AddParameter(cmd, "@EditableByUserID", editableByUserID);
				Database.Main.AddParameter(cmd, "@MaxRecords", maxResults);
				IDataParameter prm = Database.Main.AddOutputParameter(cmd, "@TotalMatches", DbType.Int32);
				DataSet ds = Database.Main.GetDataSet(cmd);
				if (prm.Value != DBNull.Value)
					totalMatches = (int)prm.Value;
				else
					totalMatches = ds.Tables[0].Rows.Count;
				User[] users = new User[ds.Tables[0].Rows.Count];
				for(int i=0; i<ds.Tables[0].Rows.Count; i++)
					users[i] = new User(ds.Tables[0].Rows[i]);
				return users;
			}

			public static bool HasPermission(Guid userID, string permissionTypeCode)
			{
				IDbCommand cmd = Database.Main.CreateCommand("UserHasPermission", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				Database.Main.AddParameter(cmd, "@PermissionTypeCode", permissionTypeCode);
				IDataParameter prm = Database.Main.AddOutputParameter(cmd, "@HasPermission", DbType.Boolean);
				cmd.ExecuteNonQuery();
				return (bool)prm.Value;
			}

			public static bool HasRole(Guid userID, string roleCode)
			{
				IDbCommand cmd = Database.Main.CreateCommand("IsUserInRole", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				Database.Main.AddParameter(cmd, "@RoleCode", roleCode);
				IDataParameter prm = Database.Main.AddOutputParameter(cmd, "@HasRole", DbType.Boolean);
				cmd.ExecuteNonQuery();
				if ((bool)prm.Value) return true;
				cmd = Database.Main.CreateCommand("IsUserInRole", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				Database.Main.AddParameter(cmd, "@RoleCode", "SUPERUSER");
				prm = Database.Main.AddOutputParameter(cmd, "@HasRole", DbType.Boolean);
				cmd.ExecuteNonQuery();
				return (bool)prm.Value;
			}

			public static bool IsUsernameAvailable(Guid clientID, Guid? excludeUserID, string username)
			{
				Database.Main.RememberOpenState();
				IDbCommand cmd = Database.Main.CreateCommand("IsUsernameAvailable", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@ClientID", clientID);
				Database.Main.AddParameter(cmd, "@Username", username);
				Database.Main.AddParameter(cmd, "@ExcludeUserID", excludeUserID);
				IDataParameter prm = Database.Main.AddOutputParameter(cmd, "@Available", DbType.Boolean);
				cmd.ExecuteNonQuery();
				Database.Main.CloseIfWasntOpen();
				return (bool)prm.Value;
			}

			public static bool IsEmailAddressAvailable(Guid clientID, Guid? excludeUserID, string email)
			{
				Database.Main.RememberOpenState();
				IDbCommand cmd = Database.Main.CreateCommand("IsEmailAddressAvailable", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@ClientID", clientID);
				Database.Main.AddParameter(cmd, "@Email", email);
				Database.Main.AddParameter(cmd, "@ExcludeUserID", excludeUserID);
				IDataParameter prm = Database.Main.AddOutputParameter(cmd, "@Available", DbType.Boolean);
				cmd.ExecuteNonQuery();
				Database.Main.CloseIfWasntOpen();
				return (bool)prm.Value;
			}

			#endregion

			#region Instance Methods

			public bool HasPermission(string permissionTypeCode)
			{
				//if (HasRole("SUPERUSER")) return true;
				if (permissions == null) LoadPermissions();
				if (permissions.ContainsKey("SUPERUSER")) return true;
				return permissions.ContainsKey(permissionTypeCode);
			}

			public Dictionary<string, Permission> Permissions
			{
				get
				{
					if (permissions == null) LoadPermissions();
					return permissions;
				}
			}

			private void LoadPermissions()
			{
				permissions = new Dictionary<string, Permission>();
				IDbCommand cmd = Database.Main.CreateCommand("ListPermissionValues", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				DataSet ds = Database.Main.GetDataSet(cmd);
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					if ((bool)row["HasPermission"])
						permissions.Add(row["PermissionTypeCode"].ToString(),
							(bool)row["Inherited"] ? Permission.Inherited : Permission.Specified);
				}
			}

			public bool HasRole(string roleCode)
			{
				if (HasPermission("SUPERUSER")) return true;
				if (roles == null) LoadRoles();
				//if (roles.ContainsKey("SUPERUSER")) return true;
				return roles.ContainsKey(roleCode);
			}

			public Dictionary<string, Permission> Roles
			{
				get
				{
					if (roles == null) LoadRoles();
					return roles;
				}
			}

			private void LoadRoles()
			{
				roles = new Dictionary<string,Permission>();
				IDbCommand cmd = Database.Main.CreateCommand("ListAccessibleRoles", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				DataSet ds = Database.Main.GetDataSet(cmd);
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					roles.Add(row["RoleCode"].ToString(),
						(bool)row["Inherited"] ? Permission.Inherited : Permission.Specified );
				}
			}

			private DateTime ToLocalTime(DateTime utcTime)
			{
				return utcTime.Add(new TimeSpan(localTimeOffsetHours, 0, 0));
			}

			public void RevokeRolesAndPermissions()
			{
				Database.Main.RememberOpenState();
				IDbCommand cmd = Database.Main.CreateCommand("RemoveRolesAndPermissionsFromUser", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				cmd.ExecuteNonQuery();
				Database.Main.CloseIfWasntOpen();
			}

			public bool VerifyRoleAccess(params string[] restrictToAnyOfTheseRoleCodes)
			{
				if (!enabled) return false;
				foreach (string role in restrictToAnyOfTheseRoleCodes)
					if (HasRole(role))
						return true;
				return false;
			}

			public bool VerifyPermission(params string[] restrictToAnyOfThesePermissions)
			{
				if (!enabled) return false;
				foreach (string p in restrictToAnyOfThesePermissions)
					if (HasPermission(p))
						return true;
				return false;
			}

			public string GenerateNewPassword()
			{
				string pw = Utilities.GenerateRandomString(12);
				Password = pw;
				return pw;
			}

			public bool CanModifyUser(User targetUser)
			{
				foreach (KeyValuePair<string, Permission> p in targetUser.Permissions)
					if (!HasPermission(p.Key))
						return false;
				foreach (KeyValuePair<string, Permission> r in targetUser.Roles)
					if (!HasRole(r.Key))
						return false;
				return true;
			}

			public void SendPasswordReminder(string template)
			{
				string pw = GenerateNewPassword();
				Password = pw;
				Save();
				
				template = Regex.Replace(template, @"\{Username\}", Username, RegexOptions.IgnoreCase);
				template = Regex.Replace(template, @"\{Password\}", pw, RegexOptions.IgnoreCase);

				MailAddress from = EmailHandler.NullEmailAddress;
				MailAddress to = new MailAddress(Email, FullName);
				MailMessage msg = new MailMessage(from, to);
				msg.Subject = "Login Details";
				msg.Body = template;

				EmailHandler.SendAsync(msg);
			}

			public Result Save()
			{
				SecurityProvider sec = (SecurityProvider)SystemCore.Instance["SecurityProvider"];
				
				if (sec.OnBeforeUserSaved != null)
				{
					StateChangingEventArgs args = new StateChangingEventArgs();
					sec.OnBeforeUserSaved(this, args);
					if (args.CancelStateChange)
						return new Result(args.ReasonForCancellation);
				}

				Database.Main.RememberOpenState();

				bool isTrans = Database.Main.IsTransactionActive;
				if (!isTrans)
					Database.Main.BeginTransaction();

				try
				{
					IDbCommand cmd = Database.Main.CreateCommand(IsNew ? "CreateUser" : "UpdateUser", CommandType.StoredProcedure);
					AddParameters(cmd);
					cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					if (isTrans)
						Database.Main.RollbackTransaction();
					Database.Main.CloseIfWasntOpen();
					throw ex;
				}
				if (!isTrans)
					Database.Main.CommitTransaction();

				Database.Main.CloseIfWasntOpen();

				bool isNew = IsNew;
				SetSaved();
				if (sec.OnUserSaved != null)
					sec.OnUserSaved(this, isNew);

				return new Result();
			}

			protected virtual void AddParameters(IDbCommand cmd)
			{
				Database.Main.AddParameter(cmd, "@UserID", UserID);
				Database.Main.AddParameter(cmd, "@ClientID", ClientID);
				Database.Main.AddParameter(cmd, "@Username", Username);
				Database.Main.AddParameter(cmd, "@PasswordHash", PasswordHash);
				Database.Main.AddParameter(cmd, "@FirstName", FirstName);
				Database.Main.AddParameter(cmd, "@Surname", Surname);
				Database.Main.AddParameter(cmd, "@Email", Email);
				Database.Main.AddParameter(cmd, "@Enabled", Enabled);
				Database.Main.AddParameter(cmd, "@Hidden", Hidden);
				Database.Main.AddParameter(cmd, "@Locked", Locked);
				Database.Main.AddParameter(cmd, "@Activated", Activated);
				Database.Main.AddParameter(cmd, "@ActivationReminderSent", ActivationReminderSent);
			}

			#endregion

			#region Roles
			/*
			public bool HasAnyRole(params string[] roleNames)
			{
				if (roles == null) LoadRoles();
				return false;

			}

			private Role[] roles = null;
			public Role[] Roles
			{
				get
				{
					if (roles == null) LoadRoles();
					return roles;
				}
			}

			private void LoadRoles(Database db)
			{
				throw new NotImplementedException("Roles code not complete yet.");
				if (db == null)
				{
					roles = new Role[0];
					return;
				}
				//IDbCommand cmd = Database.Main.CreateCommand("ListRoles"
			}
			*/
			#endregion

			#region Class Properties

			public bool Activated
			{
				get { return activated; }
				set { activated = value; }
			}

			public DateTime? ActivationReminderSent
			{
				get { return activationReminderSent; }
				set { activationReminderSent = value; }
			}

			public DateTime? Created
			{
				get { return created; }
				set { created = value; }
			}

			public int LocalTimeOffsetHours
			{
				get { return localTimeOffsetHours; }
				set { localTimeOffsetHours = value; }
			}

			public Guid UserID
			{
				get { return userID; }
				set { userID = value; }
			}

			public Guid ClientID
			{
				get { return clientID; }
				set { clientID = value; }
			}

			public string Username
			{
				get { return username; }
				set { username = value; }
			}

			public string PasswordHash
			{
				get { return passwordHash; }
			}

			public string Password
			{
				set { passwordHash = Crypto.EncryptOneWay(value); }
			}

			public string FirstName
			{
				get { return firstName; }
				set { firstName = value; }
			}

			public string Surname
			{
				get { return surname; }
				set { surname = value; }
			}

			public string FullName
			{
				get { return (FirstName + " " + Surname).Trim(); }
			}

			public string Email
			{
				get { return email; }
				set { email = value; }
			}

			public bool Enabled
			{
				get { return enabled; }
				set { enabled = value; }
			}

			public bool Hidden
			{
				get { return hidden; }
				set { hidden = value; }
			}

			public bool Locked
			{
				get { return locked; }
				set { locked = value; }
			}

			#endregion
		}

		public class Role : DataEntity
		{
			private Guid roleID = Guid.Empty, clientID = Guid.Empty;
			private string roleCode = "", name = "";
			private bool enabled = true, hidden = false, locked = false;

			#region Constructor
			public Role()
			{
				roleID = Guid.NewGuid();
				roleCode = roleID.ToString();
			}

			public Role(DataRow row)
			{
				if (row["RoleID"] != DBNull.Value) RoleID = (Guid)row["RoleID"];
				if (row["RoleCode"] != DBNull.Value) RoleCode = (string)row["RoleCode"];
				if (row["ClientID"] != DBNull.Value) ClientID = (Guid)row["ClientID"];
				if (row["Name"] != DBNull.Value) Name = (string)row["Name"];
				if (row["Enabled"] != DBNull.Value) Enabled = (bool)row["Enabled"];
				if (row["Locked"] != DBNull.Value) Locked = (bool)row["Locked"];
				if (row["Hidden"] != DBNull.Value) Hidden = (bool)row["Hidden"];
				IsNew = false;
				WasNew = false;
			}
			#endregion

			public void RevokeRolesAndPermissions()
			{
				Database.Main.RememberOpenState();
				IDbCommand cmd = Database.Main.CreateCommand("RemoveRolesAndPermissionsFromRole", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@RoleID", roleID);
				cmd.ExecuteNonQuery();
				Database.Main.CloseIfWasntOpen();
			}

			#region Load
			public static Role Load(Guid clientID, string roleCode)
			{
				IDbCommand cmd = Database.Main.CreateCommand("ListRoles", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@ClientID", clientID);
				Database.Main.AddParameter(cmd, "@RoleCode", roleCode);
				DataSet ds = Database.Main.GetDataSet(cmd);
				if (ds.Tables[0].Rows.Count == 0)
					return null;
				return new Role(ds.Tables[0].Rows[0]);
			}

			public static Role Load(Guid roleID)
			{
				IDbCommand cmd = Database.Main.CreateCommand("ListRoles", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@RoleID", roleID);
				DataSet ds = Database.Main.GetDataSet(cmd);
				if (ds.Tables[0].Rows.Count == 0)
					return null;
				return new Role(ds.Tables[0].Rows[0]);
			}
			#endregion

			#region Class Properties
			public Guid RoleID
			{
				get { return roleID; }
				set { roleID = value; }
			}

			public Guid ClientID
			{
				get { return clientID; }
				set { clientID = value; }
			}

			public string RoleCode
			{
				get { return roleCode; }
				set { roleCode = value; }
			}

			public string Name
			{
				get { return name; }
				set { name = value; }
			}

			public bool Enabled
			{
				get { return enabled; }
				set { enabled = value; }
			}

			public bool Hidden
			{
				get { return hidden; }
				set { hidden = value; }
			}

			public bool Locked
			{
				get { return locked; }
				set { locked = value; }
			}
			#endregion
		}
	}

	public enum Permission
	{
		/// <summary>
		/// The permission is not accessible by the target
		/// </summary>
		Disabled,
		/// <summary>
		/// The permission has been explicitly enabled for the target
		/// </summary>
		Specified,
		/// <summary>
		/// The permission accessible to the target because of a role or inherited role
		/// </summary>
		Inherited
	}
}
