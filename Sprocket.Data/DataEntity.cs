using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Data
{
	public abstract class DataEntity
	{
		private bool isNew = true;
		private bool wasNew = true;

		/// <summary>
		/// Gets or sets whether or not this entity already exists as a record in a database.
		/// This information is useful for knowing whether to perform an INSERT or UPDATE
		/// database operation.
		/// </summary>
		public bool IsNew
		{
			get { return isNew; }
			protected set { isNew = value; }
		}

		/// <summary>
		/// Gets or sets whether or not this entity was constructed as a new entity or from
		/// data that was retrieved from the database. This is useful if we've saved the data,
		/// hence setting IsNew to false, but still need to know if this was a new object so
		/// that we know that we should perform INSERT operations on dependent data, rather than
		/// UPDATE operations.
		/// </summary>
		public bool WasNew
		{
			get { return wasNew; }
			protected set { wasNew = value; }
		}

		public void SetSaved()
		{
			isNew = false;
		}
	}
}
