using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using Sprocket.Data;
using Sprocket.Security;

namespace Sprocket.Security
{
	public interface ISecurityProviderDataLayer
	{
		Type DatabaseHandlerType { get; }
		Result InitialiseDatabase();

		bool Authenticate(string username, string passwordHash);
		bool Authenticate(Guid clientSpaceID, string username, string passwordHash);
		bool IsEmailAddressTaken(Guid clientSpaceID, string email);
		bool IsUsernameTaken(Guid clientSpaceID, string username);

		Result Store(ClientSpace client);
		Result Store(User user);
		Result Store(Role role);
		Result Store(PermissionType permissionType);

		Result Delete(ClientSpace client);
		Result Delete(User user);
		Result Delete(Role role);
		Result Delete(PermissionType permissionType);

		event InterruptableEventHandler<ClientSpace> OnBeforeDeleteClientSpace;
		event NotificationEventHandler<ClientSpace> OnClientSpaceDeleted;
		event InterruptableEventHandler<User> OnBeforeDeleteUser;
		event NotificationEventHandler<User> OnUserDeleted;
		event InterruptableEventHandler<Role> OnBeforeDeleteRole;
		event NotificationEventHandler<Role> OnRoleDeleted;
		event InterruptableEventHandler<PermissionType> OnBeforeDeletePermissionType;
		event NotificationEventHandler<PermissionType> OnPermissionTypeDeleted;

		int? GetRoleIDFromRoleCode(Guid clientSpaceID, string roleCode);

		bool DoesRoleInheritRole(int thisRoleID, int doesItInheritRoleID);
		void InheritRoleFrom(int thisRoleID, int inheritFromRoleID);
		void DisinheritRoleFrom(int thisRoleID, int disinheritFromRoleID);
		void ListInheritedRoles(int thisRoleID);

		List<Role> ListUserRoles(int userID);
		bool IsUserInRole(int userID, int roleID);
		void AssignRoleToUser(int userID, int roleID);
		void UnassignRoleFromUser(int userID, int roleID);

		void AssignPermissionToUser(int userID, int permissionTypeID);
		void AssignPermissionToRole(int roleID, int permissionTypeID);
		bool DoesUserHavePermission(int userID, int permissionTypeID);

		void RemoveRolesAndPermissionsFromUser(int userID);
		void RemoveRolesAndPermissionsFromRole(int roleID);

		List<Role> ListAccessibleRoles(int userID);
		List<PermissionTypeState> ListPermissionsForUser(int userID);
		List<PermissionTypeState> ListPermissionsForRole(int roleID);
		List<PermissionTypeState> ListAllPermissionTypesAgainstUser(int userID);
		List<PermissionTypeState> ListAllPermissionTypesAgainstRole(int roleID);
		List<RoleState> ListAllRolesAgainstRole(int roleID);
		List<Role> ListDescendentRoles(int roleID);
	}
}
