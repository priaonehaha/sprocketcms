using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Sprocket.Data;
using Sprocket.Utility;

using Sprocket;
using Sprocket;

namespace Sprocket.Security
{
	public partial class SecurityProvider
	{
		public event StateChangingEventHandler<Client> OnBeforeClientSaved;
		public event SavedNotificationEventHandler<Client> OnClientSaved;
		
		public event StateChangingEventHandler<User> OnBeforeUserSaved;
		public event SavedNotificationEventHandler<User> OnUserSaved;
		public event StateChangingEventHandler<User> OnBeforeUserDeleted;
		public event NotificationEventHandler<User> OnUserDeleted;

		public event StateChangingEventHandler<Role> OnBeforeRoleSaved;
		public event SavedNotificationEventHandler<Role> OnRoleSaved;
		public event StateChangingEventHandler<Role> OnBeforeRoleDeleted;
		public event NotificationEventHandler<Role> OnRoleDeleted;

		public Result SaveClient(Client client)
		{
			if (OnBeforeClientSaved != null)
			{
				StateChangingEventArgs args = new StateChangingEventArgs();
				OnBeforeClientSaved(client, args);
				if (args.CancelStateChange)
					return new Result(args.ReasonForCancellation);
			}

			IDbCommand cmd;
			cmd = Database.Main.CreateCommand(client.IsNew ? "CreateClient" : "UpdateClient", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@ClientID", client.ClientID);
			Database.Main.AddParameter(cmd, "@Name", client.Name);
			Database.Main.AddParameter(cmd, "@Enabled", client.Enabled);
			Database.Main.AddParameter(cmd, "@PrimaryUserID", client.PrimaryUserID);
			Database.Main.AddParameter(cmd, "@OwnerClientID", client.OwnerClientID);
			cmd.ExecuteNonQuery();
			bool isNew = client.IsNew;
			client.SetSaved();

			if (OnClientSaved != null)
				OnClientSaved(client, isNew);

			return new Result();
		}

		public Result SaveUser(User user)
		{
			if (OnBeforeUserSaved != null)
			{
				StateChangingEventArgs args = new StateChangingEventArgs();
				OnBeforeUserSaved(user, args);
				if (args.CancelStateChange)
					return new Result(args.ReasonForCancellation);
			}

			IDbCommand cmd = Database.Main.CreateCommand(user.IsNew ? "CreateUser" : "UpdateUser", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@UserID", user.UserID);
			Database.Main.AddParameter(cmd, "@ClientID", user.ClientID);
			Database.Main.AddParameter(cmd, "@Username", user.Username);
			Database.Main.AddParameter(cmd, "@PasswordHash", user.PasswordHash);
			Database.Main.AddParameter(cmd, "@FirstName", user.FirstName);
			Database.Main.AddParameter(cmd, "@Surname", user.Surname);
			Database.Main.AddParameter(cmd, "@Email", user.Email);
			Database.Main.AddParameter(cmd, "@Enabled", user.Enabled);
			Database.Main.AddParameter(cmd, "@Hidden", user.Hidden);
			Database.Main.AddParameter(cmd, "@Locked", user.Locked);
			cmd.ExecuteNonQuery();
			bool isNew = user.IsNew;
			user.SetSaved();

			if (OnUserSaved != null)
				OnUserSaved(user, isNew);

			return new Result();
		}

		public Result SaveRole(Role role)
		{
			if (OnBeforeRoleSaved != null)
			{
				StateChangingEventArgs args = new StateChangingEventArgs();
				OnBeforeRoleSaved(role, args);
				if (args.CancelStateChange)
					return new Result(args.ReasonForCancellation);
			}

			IDbCommand cmd = Database.Main.CreateCommand(role.IsNew ? "CreateRole" : "UpdateRole", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@RoleID", role.RoleID);
			Database.Main.AddParameter(cmd, "@RoleCode", role.RoleCode);
			Database.Main.AddParameter(cmd, "@ClientID", role.ClientID);
			Database.Main.AddParameter(cmd, "@Name", role.Name);
			Database.Main.AddParameter(cmd, "@Enabled", role.Enabled);
			Database.Main.AddParameter(cmd, "@Locked", role.Locked);
			Database.Main.AddParameter(cmd, "@Hidden", role.Hidden);
			cmd.ExecuteNonQuery();
			bool isNew = role.IsNew;
			role.SetSaved();

			if (OnRoleSaved != null)
				OnRoleSaved(role, isNew);

			return new Result();
		}

		public Result DeleteRole(Role role)
		{
			if (OnBeforeRoleDeleted != null)
			{
				StateChangingEventArgs args = new StateChangingEventArgs();
				OnBeforeRoleDeleted(role, args);
				if (args.CancelStateChange)
					return new Result(args.ReasonForCancellation);
			}

			IDbCommand cmd = Database.Main.CreateCommand("DeleteRole", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@RoleID", role.RoleID);
			cmd.ExecuteNonQuery();

			if (OnRoleDeleted != null)
				OnRoleDeleted(role);

			return new Result();
		}

		public Result DeleteRole(Guid roleID)
		{
			Role role = Role.Load(roleID);
			if(role == null)
				return new Result("Role not found.");
			return DeleteRole(role);
		}

		public Result DeleteUser(User user)
		{
			if (OnBeforeUserDeleted != null)
			{
				StateChangingEventArgs args = new StateChangingEventArgs();
				OnBeforeUserDeleted(user, args);
				if (args.CancelStateChange)
					return new Result(args.ReasonForCancellation);
			}

			IDbCommand cmd = Database.Main.CreateCommand("DeleteUser", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@UserID", user.UserID);
			Database.Main.AddParameter(cmd, "@DeletePermanently", true);
			cmd.ExecuteNonQuery();

			if (OnUserDeleted != null)
				OnUserDeleted(user);

			return new Result();
		}

		public Result DeleteUser(Guid userID)
		{
			User user = User.Load(userID);
			if (user == null)
				return new Result("User not found.");
			return DeleteUser(user);
		}
	}
}
