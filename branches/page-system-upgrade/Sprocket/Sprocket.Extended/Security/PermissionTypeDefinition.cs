using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Security
{
#warning this needs to be end up in the database via SecurityProvider.RegisterPermissionType()
	public class PermissionTypeDefinition
	{
		private string name, description;
		private Enum code;

		public Enum Code
		{
			get { return code; }
		}

		public string Name
		{
			get { return name; }
		}

		public string Description
		{
			get { return description; }
		}

		public PermissionTypeDefinition(Enum code, string name, string description)
		{
			this.code = code;
			this.name = name;
			this.description = description;
		}
	}
}
