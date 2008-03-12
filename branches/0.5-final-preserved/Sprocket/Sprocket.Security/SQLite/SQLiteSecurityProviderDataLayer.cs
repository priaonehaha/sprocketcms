using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Transactions;

using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Security.SQLite
{
	public class SQLiteSecurityProviderDataLayer : ISecurityProviderDataLayer
	{
		private SQLiteStoredProcedures procs;
		public SQLiteSecurityProviderDataLayer()
		{
			procs = new SQLiteStoredProcedures(ResourceLoader.LoadTextResource("Sprocket.Security.SQLite.procedures.sql"));
		}

		public Type DatabaseHandlerType
		{
			get { return typeof(SQLiteDatabase); }
		}

		public void InitialiseDatabase(Result result)
		{
			if (!result.Succeeded)
				return;

			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = ResourceLoader.LoadTextResource("Sprocket.Security.SQLite.schema.sql");
					cmd.ExecuteNonQuery();

					cmd.CommandText = procs["Insert First Client"];
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", SecurityProvider.ClientSpaceID));
					int n = cmd.ExecuteNonQuery();

					if (n > 0) // then a new client was inserted, so insert accompanying data
					{
						User user = new User(SecurityProvider.ClientSpaceID, "admin", "password", "System", "Administrator", "user@domain", true, true, false, 0);
						PermissionType pt1 = new PermissionType(DatabaseManager.DatabaseEngine.GetUniqueID(), PermissionType.SuperUser, "Unrestricted Access", false);
						PermissionType pt2 = new PermissionType(DatabaseManager.DatabaseEngine.GetUniqueID(), PermissionType.AdministrativeAccess, "Access Admin Area", false);
						PermissionType pt3 = new PermissionType(DatabaseManager.DatabaseEngine.GetUniqueID(), PermissionType.UserAdministrator, "Create/Modify Users", false);
						PermissionType pt4 = new PermissionType(DatabaseManager.DatabaseEngine.GetUniqueID(), PermissionType.RoleAdministrator, "Create/Modify Roles", false);
						user.UserID = DatabaseManager.DatabaseEngine.GetUniqueID();
						user.Activated = true;
						Result r = Store(user);
						if (r.Succeeded)
						{
							r = Store(pt1); if (r.Succeeded)
							{
								r = Store(pt2); if (r.Succeeded)
								{
									r = Store(pt3); if (r.Succeeded)
									{
										r = Store(pt4); if (r.Succeeded)
										{
											r = AssignPermissionToUser(user.UserID, PermissionType.SuperUser);
										}
									}
								}
							}
						}
						if (!r.Succeeded)
							result.SetFailed(r.Message);
					}
					if (result.Succeeded)
						scope.Complete();
				}
			}
			catch (Exception ex)
			{
				result.SetFailed("Unable to initialise SQLite database for SecurityProvider: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		private void CheckForNullParameters(SQLiteCommand cmd)
		{
			foreach (SQLiteParameter prm in cmd.Parameters)
				if (prm.Value == null)
					prm.Value = DBNull.Value;
		}

		public bool Authenticate(string username, string passwordHash)
		{
			return Authenticate(SecurityProvider.ClientSpaceID, username, passwordHash);
		}

		public bool Authenticate(long clientSpaceID, string username, string passwordHash)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["Authenticate"];
					cmd.Parameters.Add(new SQLiteParameter("@Username", username));
					cmd.Parameters.Add(new SQLiteParameter("@PasswordHash", passwordHash));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					CheckForNullParameters(cmd);
					SQLiteDataReader dr = cmd.ExecuteReader();
					bool result;
					if (dr.Read())
						if (dr.IsDBNull(0))
							result = false;
						else
							result = Convert.ToBoolean(dr[0]);
					else
						result = false;
					dr.Close();
					scope.Complete();
					return result;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public bool IsEmailAddressTaken(long clientSpaceID, string email)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["IsEmailAddressTaken"];
					cmd.Parameters.Add(new SQLiteParameter("@Email", email));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@ExcludeUserID", -1));
					CheckForNullParameters(cmd);
					SQLiteDataReader dr = cmd.ExecuteReader();
					bool result;
					dr.Read();
					result = Convert.ToBoolean(dr[0]);
					dr.Close();
					scope.Complete();
					return result;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public bool IsUsernameTaken(long clientSpaceID, string username)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["IsUsernameTaken"];
					cmd.Parameters.Add(new SQLiteParameter("@Username", username));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@ExcludeUserID", -1));
					CheckForNullParameters(cmd);
					SQLiteDataReader dr = cmd.ExecuteReader();
					bool result;
					dr.Read();
					result = Convert.ToBoolean(dr[0]);
					dr.Close();
					scope.Complete();
					return result;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public bool IsEmailAddressTaken(long clientSpaceID, string email, long? excludeUserID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["IsEmailAddressTaken"];
					cmd.Parameters.Add(new SQLiteParameter("@Email", email));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@ExcludeUserID", excludeUserID));
					CheckForNullParameters(cmd);
					SQLiteDataReader dr = cmd.ExecuteReader();
					bool result;
					dr.Read();
					result = Convert.ToBoolean(dr[0]);
					dr.Close();
					scope.Complete();
					return result;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public bool IsUsernameTaken(long clientSpaceID, string username, long? excludeUserID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["IsUsernameTaken"];
					cmd.Parameters.Add(new SQLiteParameter("@Username", username));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@ExcludeUserID", excludeUserID));
					CheckForNullParameters(cmd);
					SQLiteDataReader dr = cmd.ExecuteReader();
					bool result;
					dr.Read();
					result = Convert.ToBoolean(dr[0]);
					dr.Close();
					scope.Complete();
					return result;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public Result Store(ClientSpace client)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["Store ClientSpace"];
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", client.ClientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@Name", client.Name));
					cmd.Parameters.Add(new SQLiteParameter("@Enabled", client.Enabled));
					cmd.Parameters.Add(new SQLiteParameter("@PrimaryUserID", client.PrimaryUserID));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("Unable to store ClientSpace: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public Result Store(User user)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["Store User"];
					cmd.Parameters.Add(new SQLiteParameter("@UserID", user.UserID));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", user.ClientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@Username", user.Username));
					cmd.Parameters.Add(new SQLiteParameter("@PasswordHash", user.PasswordHash));
					cmd.Parameters.Add(new SQLiteParameter("@FirstName", user.FirstName));
					cmd.Parameters.Add(new SQLiteParameter("@Surname", user.Surname));
					cmd.Parameters.Add(new SQLiteParameter("@Email", user.Email));
					cmd.Parameters.Add(new SQLiteParameter("@LocalTimeOffsetHours", user.LocalTimeOffsetHours));
					cmd.Parameters.Add(new SQLiteParameter("@Enabled", user.Enabled));
					cmd.Parameters.Add(new SQLiteParameter("@Hidden", user.Hidden));
					cmd.Parameters.Add(new SQLiteParameter("@Locked", user.Locked));
					cmd.Parameters.Add(new SQLiteParameter("@Deleted", user.Deleted));
					cmd.Parameters.Add(new SQLiteParameter("@Activated", user.Activated));
					cmd.Parameters.Add(new SQLiteParameter("@ActivationReminderSent", user.ActivationReminderSent));
					cmd.Parameters.Add(new SQLiteParameter("@Created", user.Created));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("Unable to store User: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public Result Store(Role role)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["Store Role"];
					cmd.Parameters.Add(new SQLiteParameter("@RoleID", role.RoleID));
					cmd.Parameters.Add(new SQLiteParameter("@RoleCode", role.RoleCode));
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", role.ClientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@Name", role.Name));
					cmd.Parameters.Add(new SQLiteParameter("@Enabled", role.Enabled));
					cmd.Parameters.Add(new SQLiteParameter("@Locked", role.Locked));
					cmd.Parameters.Add(new SQLiteParameter("@Hidden", role.Hidden));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("Unable to store Role: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public Result Store(PermissionType permissionType)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.CommandText = procs["Store PermissionType"];
					cmd.Parameters.Add(new SQLiteParameter("@PermissionTypeID", permissionType.PermissionTypeID));
					cmd.Parameters.Add(new SQLiteParameter("@PermissionTypeCode", permissionType.PermissionTypeCode));
					cmd.Parameters.Add(new SQLiteParameter("@Description", permissionType.Description));
					cmd.Parameters.Add(new SQLiteParameter("@DefaultValue", permissionType.DefaultValue));
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result("Unable to store PermissionType: " + ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public event InterruptableEventHandler<ClientSpace> OnBeforeDeleteClientSpace;
		public event NotificationEventHandler<ClientSpace> OnClientSpaceDeleted;
		public event InterruptableEventHandler<User> OnBeforeDeleteUser;
		public event NotificationEventHandler<User> OnUserDeleted;
		public event InterruptableEventHandler<Role> OnBeforeDeleteRole;
		public event NotificationEventHandler<Role> OnRoleDeleted;
		public event InterruptableEventHandler<PermissionType> OnBeforeDeletePermissionType;
		public event NotificationEventHandler<PermissionType> OnPermissionTypeDeleted;

		public Result Delete(ClientSpace client)
		{
			Result result = new Result();
			if (OnBeforeDeleteClientSpace != null)
				OnBeforeDeleteClientSpace(client, result);
			if (result.Succeeded)
			{
				using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					connection.Open();
					using (TransactionScope scope = new TransactionScope())
					{
						try
						{
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "DELETE FROM ClientSpaces WHERE ClientSpaceID = @ClientSpaceID";
							cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", client.ClientSpaceID));
							cmd.ExecuteNonQuery();
							scope.Complete();
						}
						catch (Exception ex)
						{
							return new Result("Unable to delete ClientSpace: " + ex.Message);
						}
					}
				}
				if (OnClientSpaceDeleted != null)
					OnClientSpaceDeleted(client);
			}
			return result;
		}

		public Result Delete(User user)
		{
			Result result = new Result();
			if (OnBeforeDeleteUser != null)
				OnBeforeDeleteUser(user, result);
			if (result.Succeeded)
			{
				using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					connection.Open();
					using (TransactionScope scope = new TransactionScope())
					{
						try
						{
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "DELETE FROM Users WHERE UserID = @UserID";
							cmd.Parameters.Add(new SQLiteParameter("@UserID", user.UserID));
							cmd.ExecuteNonQuery();
							scope.Complete();
						}
						catch (Exception ex)
						{
							return new Result("Unable to delete User: " + ex.Message);
						}
					}
				}
				if (OnUserDeleted != null)
					OnUserDeleted(user);
			}
			return result;
		}

		public Result Delete(Role role)
		{
			Result result = new Result();
			if (OnBeforeDeleteRole != null)
				OnBeforeDeleteRole(role, result);
			if (result.Succeeded)
			{
				using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					connection.Open();
					using (TransactionScope scope = new TransactionScope())
					{
						try
						{
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "DELETE FROM Roles WHERE RoleID = @RoleID";
							cmd.Parameters.Add(new SQLiteParameter("@RoleID", role.RoleID));
							cmd.ExecuteNonQuery();
							scope.Complete();
						}
						catch (Exception ex)
						{
							return new Result("Unable to delete Role: " + ex.Message);
						}
					}
				}
				if (OnRoleDeleted != null)
					OnRoleDeleted(role);
			}
			return result;
		}

		public Result Delete(PermissionType permissionType)
		{
			Result result = new Result();
			if (OnBeforeDeletePermissionType != null)
				OnBeforeDeletePermissionType(permissionType, result);
			if (result.Succeeded)
			{
				using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
				{
					connection.Open();
					using (TransactionScope scope = new TransactionScope())
					{
						try
						{
							SQLiteCommand cmd = connection.CreateCommand();
							cmd.CommandText = "DELETE FROM PermissionTypes WHERE PermissionTypeID = @PermissionTypeID";
							cmd.Parameters.Add(new SQLiteParameter("@PermissionTypeID", permissionType.PermissionTypeID));
							cmd.ExecuteNonQuery();
							scope.Complete();
						}
						catch (Exception ex)
						{
							return new Result("Unable to delete PermissionType: " + ex.Message);
						}
					}
				}
				if (OnPermissionTypeDeleted != null)
					OnPermissionTypeDeleted(permissionType);
			}
			return result;
		}

		public Result InitialiseClientSpace(long clientSpaceID)
		{
			return new Result();
		}

		public Result ActivateUser(string activationCode, out long userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Result SetEmailChangeRequest(long userID, string newEmailAddress, string activationCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public EmailChangeRequest SelectEmailChangeRequest(long userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public ClientSpace SelectClientSpace(long clientSpaceID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["Select ClientSpace"];
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					SQLiteDataReader reader = cmd.ExecuteReader();
					ClientSpace obj = null;
					if (reader.Read())
						obj = new ClientSpace(reader);
					reader.Close();
					scope.Complete();
					return obj;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public User SelectUser(long clientSpaceID, string username)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["Select User By Username"];
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@Username", username));
					CheckForNullParameters(cmd);
					SQLiteDataReader reader = cmd.ExecuteReader();
					User user = null;
					if (reader.Read())
						user = new User(reader);
					reader.Close();
					scope.Complete();
					return user;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public User SelectUser(long userID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["Select User By UserID"];
					cmd.Parameters.Add(new SQLiteParameter("@UserID", userID));
					SQLiteDataReader reader = cmd.ExecuteReader();
					User user = null;
					if (reader.Read())
						user = new User(reader);
					reader.Close();
					scope.Complete();
					return user;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public Role SelectRole(long clientSpaceID, string roleCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Role SelectRole(long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public long? GetRoleIDFromRoleCode(Guid clientSpaceID, string roleCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool DoesRoleInheritRole(long thisRoleID, long doesItInheritRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void InheritRoleFrom(long thisRoleID, long inheritFromRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void DisinheritRoleFrom(long thisRoleID, long disinheritFromRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<Role> ListInheritedRoles(long thisRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<Role> ListUserRoles(long userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsUserInRole(long userID, string roleCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsUserInRole(long userID, long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void AssignRoleToUser(long userID, string roleCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void UnassignRoleFromUser(long userID, string roleCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public Result AssignPermissionToUser(long userID, string permissionTypeCode)
		{
			using (TransactionScope scope = new TransactionScope())
			{
				SQLiteConnection conn = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
				try
				{
					SQLiteCommand cmd = new SQLiteCommand(conn);
					cmd.CommandText = procs["AssignPermissionToUser"];
					cmd.CommandType = CommandType.Text;
					cmd.Parameters.Add(new SQLiteParameter("@UserID", userID));
					cmd.Parameters.Add(new SQLiteParameter("@PermissionTypeCode", permissionTypeCode));
					CheckForNullParameters(cmd);
					cmd.ExecuteNonQuery();
					scope.Complete();
				}
				catch (Exception ex)
				{
					return new Result("Error executing AssignPermissionToUser: " + ex.Message);
				}
				finally
				{
					DatabaseManager.DatabaseEngine.ReleaseConnection();
				}
			}
			return new Result();
		}

		public void AssignPermissionToRole(long roleID, string permissionTypeCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool DoesUserHavePermission(long userID, string permissionTypeCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void RemoveRolesAndPermissionsFromUser(long userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void RemoveRolesAndPermissionsFromRole(long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void SetRolesAndPermissionsForUser(long userID, List<string> roleCodes, List<string> permissionTypeCodes)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void SetRolesAndPermissionsForRole(long roleID, List<string> roleCodes, List<string> permissionTypeCodes)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<Role> ListAccessibleRoles(long userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<PermissionTypeState> ListPermissionsForRole(long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<PermissionTypeState> ListAllPermissionTypesAgainstUser(long userID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["ListPermissionsAndRolesForUser"];
					cmd.Parameters.Add(new SQLiteParameter("@UserID", userID));
					Dictionary<long, PermissionType> types = new Dictionary<long, PermissionType>();
					Dictionary<long, Role> roles = new Dictionary<long, Role>();
					Dictionary<long, PermissionTypeState> final = new Dictionary<long, PermissionTypeState>();
					Dictionary<long, List<long>> role2role = new Dictionary<long, List<long>>();
					Dictionary<long, List<long>> rolePermissions = new Dictionary<long, List<long>>();
					SQLiteDataReader reader = cmd.ExecuteReader();
					// permissions
					while (reader.Read())
					{
						PermissionType pt = new PermissionType(reader);
						types[pt.PermissionTypeID] = pt;
					}
					// roles
					reader.NextResult();
					while (reader.Read())
					{
						Role role = new Role(reader);
						roles.Add(role.RoleID, role);
					}
					// role to role
					reader.NextResult();
					while (reader.Read())
					{
						long roleID = (long)reader["RoleID"];
						long inheritsRoleID = (long)reader["InheritsRoleID"];
						if (!role2role.ContainsKey(roleID))
							role2role.Add(roleID, new List<long>());
						role2role[roleID].Add(inheritsRoleID);
					}
					// role permissions
					reader.NextResult();
					while (reader.Read())
					{
						long roleID = (long)reader["RoleID"];
						long ptid = (long)reader["PermissionTypeID"];
						if (!rolePermissions.ContainsKey(roleID))
							rolePermissions.Add(roleID, new List<long>());
						rolePermissions[roleID].Add(ptid);
					}
					// user permissions
					reader.NextResult();
					while (reader.Read())
					{
						long id = (long)reader["PermissionTypeID"];
						final.Add(id, new PermissionTypeState(types[id], PermissionState.Specified));
					}
					// user roles (the following code block simulates the T-SQL set-based solution
					reader.NextResult();
					List<List<long>> inherits = new List<List<long>>();
					List<long> r = new List<long>();
					while (reader.Read()) // first get the roles that this user is a member of
					{
						long roleID = (long)reader["RoleID"];
						r.Add(roleID);
					}
					reader.Close();
					if(r.Count > 0) // if this user was a member of any roles
					{
						inherits.Add(r); // add the roles as the first level of inheritances
						while (true) // starting at the base ancestor generation of 0
						{
							r = new List<long>(); // start a list of roles that are inherited
							foreach (long roleID in inherits[inherits.Count-1]) // for each role in the current generation
								foreach (long inheritsRoleID in role2role[roleID]) // for each role _that_ role inherits
									if (!r.Contains(inheritsRoleID)) // if we haven't already noted down that role id
										r.Add(inheritsRoleID); // add the role to the list of roles inherited by this generation
							if (r.Count == 0) // if none were inherited, we're done
								break;
							inherits.Add(r); // add the list of roles for this generation to the stack
						}
					}
					foreach (List<long> roleIDs in inherits) // for each ancestor generation of role inheritances
						foreach (long roleID in roleIDs) // for each set of roles that each ancestor inherits
							if (rolePermissions.ContainsKey(roleID)) // if the role has any specific permissions
								foreach (long ptid in rolePermissions[roleID]) // for each permission that the role has
									if (!final.ContainsKey(ptid)) // if we haven't already noted down that permission
										final.Add(ptid, new PermissionTypeState(types[ptid], PermissionState.Inherited)); // add the permission to the list

					List<PermissionTypeState> list = new List<PermissionTypeState>();
					list.AddRange(final.Values);
					scope.Complete();
					return list;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public List<PermissionTypeState> ListAllPermissionTypesAgainstRole(long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<RoleState> ListAllRolesAgainstRole(long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<RoleState> ListAllRolesAgainstUser(long userID)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.CommandText = procs["ListAllRolesAgainstUser"];
					cmd.Parameters.Add(new SQLiteParameter("@UserID", userID));
					SQLiteDataReader reader = cmd.ExecuteReader();
					List<RoleState> list = new List<RoleState>();
					while (reader.Read())
						list.Add(new RoleState(new Role(reader), Convert.ToBoolean(reader["HasRole"]),
							(bool)reader["HasRole"] ? PermissionState.Specified : PermissionState.Disabled));
					reader.Close();
					scope.Complete();
					return list;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public List<Role> ListDescendentRoles(long roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<User> FilterUsers(string partUsername, string partFirstName, string partSurname, string partEmail, int? maxResults, long? editableByUserID, bool? activated, out int totalMatches)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SQLiteConnection connection = (SQLiteConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SQLiteCommand cmd = connection.CreateCommand();
					cmd.Connection = connection;
					cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", SecurityProvider.ClientSpaceID));
					cmd.Parameters.Add(new SQLiteParameter("@Username", partUsername));
					cmd.Parameters.Add(new SQLiteParameter("@FirstName", partFirstName));
					cmd.Parameters.Add(new SQLiteParameter("@Surname", partSurname));
					cmd.Parameters.Add(new SQLiteParameter("@Email", partEmail));
					cmd.Parameters.Add(new SQLiteParameter("@Activated", activated));
					CheckForNullParameters(cmd);
					string limit = maxResults.HasValue ? "LIMIT " + maxResults : "";
					cmd.CommandText = procs["Filter Users"].Replace("[limit]", limit);
					SQLiteDataReader reader = cmd.ExecuteReader();
					List<User> list = new List<User>();
					while (reader.Read())
						list.Add(new User(reader));
					if (maxResults.HasValue)
					{
						reader.Close();
						cmd.CommandText = procs["Filter Users"].Replace("[limit]", "").Replace("*", "COUNT(*)");
						reader = cmd.ExecuteReader();
						reader.Read();
						totalMatches = Convert.ToInt32(reader[0]);
					}
					else
						totalMatches = list.Count;
					reader.Close();
					scope.Complete();
					return list;
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public User SelectUserByEmail(long clientSpaceID, string email)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
