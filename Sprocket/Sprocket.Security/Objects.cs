using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

using Sprocket;
using Sprocket.Data;
using Sprocket.Mail;
using Sprocket.Utility;

namespace Sprocket.Security
{
	public class ClientSpace : IEntity
	{
		private Guid clientSpaceID;
		private Guid? primaryUserID, ownerClientSpaceID;
		private bool enabled;
		private string name;

		#region Class Properties

		public Guid ClientSpaceID
		{
			get { return clientSpaceID; }
			set { clientSpaceID = value; }
		}

		public Guid? PrimaryUserID
		{
			get { return primaryUserID; }
			set { primaryUserID = value; }
		}

		public Guid? OwnerClientSpaceID
		{
			get { return ownerClientSpaceID; }
			set { ownerClientSpaceID = value; }
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

	public class User : IEntity
	{
		#region Fields
		protected Guid userID = Guid.Empty, clientSpaceID = Guid.Empty;
		protected string username = "", passwordHash = "";
		protected string firstName = "", surname = "", email = "";
		protected bool enabled = true, locked = false, activated = false, deleted = false;
		protected bool hidden = false; // for special users used internally only
		protected int localTimeOffsetHours = 0;
		protected DateTime? activationReminderSent = null, created = null;

		private Dictionary<string, PermissionState> permissions = null;
		private Dictionary<string, PermissionState> roles = null;
		#endregion

		#region Constructor
		public User() { }

		public User(Guid clientSpaceID, string username,
			string password, string firstName, string surname, string email,
			bool enabled, bool locked, bool hidden, int localTimeOffsetHours)
		{
			this.userID = Guid.NewGuid();
			this.clientSpaceID = clientSpaceID;
			this.username = username;
			this.passwordHash = Crypto.EncryptOneWay(password);
			this.firstName = firstName;
			this.surname = surname;
			this.email = email;
			this.enabled = enabled;
			this.locked = locked;
			this.deleted = false;
			this.hidden = hidden;
			this.activated = false;
			this.created = null;
			this.activationReminderSent = null;
			this.localTimeOffsetHours = localTimeOffsetHours;
		}

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

		public Guid ClientSpaceID
		{
			get { return clientSpaceID; }
			set { clientSpaceID = value; }
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

		public bool Deleted
		{
			get { return deleted; }
			set { deleted = value; }
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

	public class Role : IEntity
	{
		private Guid roleID = Guid.Empty, clientSpaceID = Guid.Empty;
		private string roleCode = "", name = "";
		private bool enabled = true, hidden = false, locked = false;

		#region Constructor
		public Role()
		{
			roleID = Guid.NewGuid();
			roleCode = roleID.ToString();
		}

		#endregion

		#region Class Properties
		public Guid RoleID
		{
			get { return roleID; }
			set { roleID = value; }
		}

		public Guid ClientSpaceID
		{
			get { return clientSpaceID; }
			set { clientSpaceID = value; }
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

	public class PermissionType : IEntity
	{
		int permissionTypeID;
		string permissionTypeCode, description;
		bool defaultValue;

		#region Properties
		public int PermissionTypeID
		{
			get { return permissionTypeID; }
			set { permissionTypeID = value; }
		}

		public string PermissionTypeCode
		{
			get { return permissionTypeCode; }
			set { permissionTypeCode = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		public bool DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}
		#endregion
	}

	public enum PermissionState
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

	public class PermissionTypeState
	{
		private PermissionState permissionState;
		private PermissionType permissionType;

		public PermissionType PermissionType
		{
			get { return permissionType; }
		}

		public PermissionState PermissionState
		{
			get { return permissionState; }
		}

		public PermissionTypeState(PermissionType type, PermissionState state)
		{
			permissionType = type;
			permissionState = state;
		}
	}

	public class RoleState
	{
		private Role role;
		private bool isAccessible;

		public Role Role
		{
			get { return role; }
		}

		public bool IsAccessible
		{
			get { return isAccessible; }
		}

		public RoleState(Role role, bool accessible)
		{
			this.role = role;
			this.isAccessible = accessible;
		}
	}
}
