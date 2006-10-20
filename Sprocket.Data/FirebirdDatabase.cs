using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Xml;
using System.Web;

namespace Sprocket.Data
{/*
	public class FirebirdDatabase : Database
	{
		public override IDbConnection GetConnectionObject()
		{
			return new FbConnection();
		}

		public override IDataAdapter GetDataAdapter()
		{
			return new FbDataAdapter();
		}

		public override IDataAdapter GetDataAdapter(IDbCommand cmd)
		{
			return new FbDataAdapter((FbCommand)cmd);
		}

		public override void SetDataAdapterCommands(IDataAdapter da, IDbCommand select, IDbCommand update, IDbCommand insert, IDbCommand delete)
		{
			FbDataAdapter fda = (FbDataAdapter)da;
			fda.SelectCommand = (FbCommand)select;
			fda.UpdateCommand = (FbCommand)update;
			fda.InsertCommand = (FbCommand)insert;
			fda.DeleteCommand = (FbCommand)delete;
		}

		protected override void CheckParameter(IDataParameter prm)
		{
			if (prm.Value is bool)
			{
				prm.Value = (bool)prm.Value ? 1 : 0;
				prm.DbType = DbType.Int16;
			}
			else if (prm.Value is Guid)
			{
				prm.DbType = DbType.String;
				prm.Value = prm.Value.ToString();
			}
			else if (prm.DbType == DbType.Guid && prm.Value == DBNull.Value)
			{
				prm.DbType = DbType.String;
				prm.Value = Guid.Empty.ToString();
			}
		}
	}
*/}
