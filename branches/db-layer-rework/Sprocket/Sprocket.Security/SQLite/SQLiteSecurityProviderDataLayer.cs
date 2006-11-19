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
		public Type DatabaseHandlerType
		{
			get { return typeof(SQLiteDatabase); }
		}

		private string GetSQL(string name)
		{
			return ResourceLoader.LoadTextResource("Sprocket.Security.SQLite." + name + ".sql");
		}

		public Result InitialiseDatabase()
		{
			Result result = new Result();
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				connection.Open();
				using (TransactionScope scope = new TransactionScope())
				{
					try
					{
						SQLiteCommand cmd = connection.CreateCommand();
						cmd.CommandText = GetSQL("SQLite Tables");
						cmd.ExecuteNonQuery();

						cmd.CommandText = GetSQL("Insert First Client");
						cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", SecurityProvider.ClientSpaceID));
						int n = cmd.ExecuteNonQuery();

						if (n > 0) // then a new client was inserted, so insert accompanying data
						{
						}
						scope.Complete();
					}
					catch (Exception ex)
					{
						result.SetFailed("Unable to initialise SQLite database for SecurityProvider: " + ex.Message);
					}
				}
			}
			return result;
		}

		public bool Authenticate(string username, string passwordHash)
		{
			return Authenticate(SecurityProvider.ClientSpaceID, username, passwordHash);
		}

		public bool Authenticate(Guid clientSpaceID, string username, string passwordHash)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				SQLiteCommand cmd = connection.CreateCommand();
				cmd.CommandText = GetSQL("Authenticate");
				cmd.Parameters.Add(new SQLiteParameter("@Username", username));
				cmd.Parameters.Add(new SQLiteParameter("@PasswordHash", passwordHash));
				cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
				SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				bool result;
				if (dr.Read())
					if (dr.IsDBNull(0))
						result = false;
					else
						result = Convert.ToBoolean(dr[0]);
				else
					result = false;
				dr.Close();
				return result;
			}
		}

		public bool IsEmailAddressTaken(Guid clientSpaceID, string email)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				SQLiteCommand cmd = connection.CreateCommand();
				cmd.CommandText = GetSQL("IsEmailAddressTaken");
				cmd.Parameters.Add(new SQLiteParameter("@Email", email));
				cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
				SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				bool result;
				dr.Read();
				result = Convert.ToBoolean(dr[0]);
				dr.Close();
				return result;
			}
		}

		public bool IsUsernameTaken(Guid clientSpaceID, string username)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				SQLiteCommand cmd = connection.CreateCommand();
				cmd.CommandText = GetSQL("IsUsernameTaken");
				cmd.Parameters.Add(new SQLiteParameter("@Username", username));
				cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", clientSpaceID));
				SQLiteDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				bool result;
				dr.Read();
				result = Convert.ToBoolean(dr[0]);
				dr.Close();
				return result;
			}
		}

		public Result Store(ClientSpace client)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				connection.Open();
				using (TransactionScope scope = new TransactionScope())
				{
					try
					{
						SQLiteCommand cmd = connection.CreateCommand();
						cmd.CommandText = GetSQL("Store ClientSpace");
						cmd.Parameters.Add(new SQLiteParameter("@ClientSpaceID", client.ClientSpaceID));
						cmd.Parameters.Add(new SQLiteParameter("@Name", client.Name));
						cmd.Parameters.Add(new SQLiteParameter("@Enabled", client.Enabled));
						cmd.Parameters.Add(new SQLiteParameter("@PrimaryUserID", client.PrimaryUserID));
						cmd.Parameters.Add(new SQLiteParameter("@OwnerClientSpaceID", client.OwnerClientSpaceID));
						cmd.ExecuteNonQuery();
						scope.Complete();
					}
					catch(Exception ex)
					{
						return new Result("Unable to store ClientSpace: " + ex.Message);
					}
				}
			}
			return new Result();
		}

		public Result Store(User user)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				connection.Open();
				using (TransactionScope scope = new TransactionScope())
				{
					try
					{
						SQLiteCommand cmd = connection.CreateCommand();
						cmd.CommandText = GetSQL("Store User");
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
					catch (Exception ex)
					{
						return new Result("Unable to store User: " + ex.Message);
					}
				}
			}
			return new Result();
		}

		public Result Store(Role role)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				connection.Open();
				using (TransactionScope scope = new TransactionScope())
				{
					try
					{
						SQLiteCommand cmd = connection.CreateCommand();
						cmd.CommandText = GetSQL("Store Role");
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
					catch (Exception ex)
					{
						return new Result("Unable to store Role: " + ex.Message);
					}
				}
			}
			return new Result();
		}

		public Result Store(PermissionType permissionType)
		{
			using (SQLiteConnection connection = new SQLiteConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				connection.Open();
				using (TransactionScope scope = new TransactionScope())
				{
					try
					{
						SQLiteCommand cmd = connection.CreateCommand();
						cmd.CommandText = GetSQL("Store PermissionType");
						cmd.Parameters.Add(new SQLiteParameter("@PermissionTypeID", permissionType.PermissionTypeID));
						cmd.Parameters.Add(new SQLiteParameter("@PermissionTypeCode", permissionType.PermissionTypeCode));
						cmd.Parameters.Add(new SQLiteParameter("@Description", permissionType.Description));
						cmd.Parameters.Add(new SQLiteParameter("@DefaultValue", permissionType.DefaultValue));
						cmd.ExecuteNonQuery();
						scope.Complete();
					}
					catch (Exception ex)
					{
						return new Result("Unable to store PermissionType: " + ex.Message);
					}
				}
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

		public int? GetRoleIDFromRoleCode(Guid clientSpaceID, string roleCode)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool DoesRoleInheritRole(int thisRoleID, int doesItInheritRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void InheritRoleFrom(int thisRoleID, int inheritFromRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void DisinheritRoleFrom(int thisRoleID, int disinheritFromRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void ListInheritedRoles(int thisRoleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<Role> ListUserRoles(int userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool IsUserInRole(int userID, int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void AssignRoleToUser(int userID, int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void UnassignRoleFromUser(int userID, int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void AssignPermissionToUser(int userID, int permissionTypeID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void AssignPermissionToRole(int roleID, int permissionTypeID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public bool DoesUserHavePermission(int userID, int permissionTypeID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void RemoveRolesAndPermissionsFromUser(int userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void RemoveRolesAndPermissionsFromRole(int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<Role> ListAccessibleRoles(int userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<PermissionTypeState> ListPermissionsForUser(int userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<PermissionTypeState> ListPermissionsForRole(int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<PermissionTypeState> ListAllPermissionTypesAgainstUser(int userID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<PermissionTypeState> ListAllPermissionTypesAgainstRole(int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<RoleState> ListAllRolesAgainstRole(int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<Role> ListDescendentRoles(int roleID)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
