IF EXISTS(SELECT id FROM sysobjects WHERE name='Clients' AND type='U')
	DROP TABLE Clients
IF EXISTS(SELECT id FROM sysobjects WHERE name='Users' AND type='U')
	DROP TABLE Users
IF EXISTS(SELECT id FROM sysobjects WHERE name='Roles' AND type='U')
	DROP TABLE Roles
IF EXISTS(SELECT id FROM sysobjects WHERE name='UserToRole' AND type='U')
	DROP TABLE UserToRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='RoleToRole' AND type='U')
	DROP TABLE RoleToRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='PermissionTypes' AND type='U')
	DROP TABLE PermissionTypes
IF EXISTS(SELECT id FROM sysobjects WHERE name='Permissions' AND type='U')
	DROP TABLE Permissions

IF EXISTS(SELECT id FROM sysobjects WHERE name='IsValidLogin' AND type='P')
	DROP PROCEDURE IsValidLogin

IF EXISTS(SELECT id FROM sysobjects WHERE name='ListClients' AND type='P')
	DROP PROCEDURE ListClients
IF EXISTS(SELECT id FROM sysobjects WHERE name='CreateClient' AND type='P')
	DROP PROCEDURE CreateClient
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdateClient' AND type='P')
	DROP PROCEDURE UpdateClient
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeleteClient' AND type='P')
	DROP PROCEDURE DeleteClient

IF EXISTS(SELECT id FROM sysobjects WHERE name='ListUsers' AND type='P')
	DROP PROCEDURE ListUsers
IF EXISTS(SELECT id FROM sysobjects WHERE name='CreateUser' AND type='P')
	DROP PROCEDURE CreateUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeleteUser' AND type='P')
	DROP PROCEDURE DeleteUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdateUser' AND type='P')
	DROP PROCEDURE UpdateUser
IF EXISTS(SELECT id FROM sysobjects WHERE name='IsUsernameAvailable' AND type='P')
	DROP PROCEDURE IsUsernameAvailable

IF EXISTS(SELECT id FROM sysobjects WHERE name='CreateRole' AND type='P')
	DROP PROCEDURE CreateRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeleteRole' AND type='P')
	DROP PROCEDURE DeleteRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdateRole' AND type='P')
	DROP PROCEDURE UpdateRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListRoles' AND type='P')
	DROP PROCEDURE ListRoles

IF EXISTS(SELECT id FROM sysobjects WHERE name='RoleInheritsFromRole' AND type='P')
	DROP PROCEDURE RoleInheritsFromRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='InheritRoleFrom' AND type='P')
	DROP PROCEDURE InheritRoleFrom
IF EXISTS(SELECT id FROM sysobjects WHERE name='DisinheritRoleFrom' AND type='P')
	DROP PROCEDURE DisinheritRoleFrom
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListInheritedRoles' AND type='P')
	DROP PROCEDURE ListInheritedRoles

IF EXISTS(SELECT id FROM sysobjects WHERE name='ListUserRoles' AND type='P')
	DROP PROCEDURE ListUserRoles
IF EXISTS(SELECT id FROM sysobjects WHERE name='IsUserInRole' AND type='P')
	DROP PROCEDURE IsUserInRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='AssignUserToRole' AND type='P')
	DROP PROCEDURE AssignUserToRole
IF EXISTS(SELECT id FROM sysobjects WHERE name='UnassignRoleFromUser' AND type='P')
	DROP PROCEDURE UnassignRoleFromUser

IF EXISTS(SELECT id FROM sysobjects WHERE name='CreatePermissionType' AND type='P')
	DROP PROCEDURE CreatePermissionType
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeletePermissionType' AND type='P')
	DROP PROCEDURE DeletePermissionType
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListPermissionTypes' AND type='P')
	DROP PROCEDURE ListPermissionTypes
IF EXISTS(SELECT id FROM sysobjects WHERE name='AssignPermission' AND type='P')
	DROP PROCEDURE AssignPermission
IF EXISTS(SELECT id FROM sysobjects WHERE name='RemovePermission' AND type='P')
	DROP PROCEDURE RemovePermission

IF EXISTS(SELECT id FROM sysobjects WHERE name='UserHasPermission' AND type='P')
	DROP PROCEDURE UserHasPermission
IF EXISTS(SELECT id FROM sysobjects WHERE name='ListPermissionValues' AND type='P')
	DROP PROCEDURE ListPermissionValues
