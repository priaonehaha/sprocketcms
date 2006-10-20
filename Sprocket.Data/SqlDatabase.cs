using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Xml;
using System.Web;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace Sprocket.Data
{
	public class SqlDatabase : Database
	{
		public override IDbConnection GetConnectionObject()
		{
			return new SqlConnection();
		}

		public override IDataAdapter GetDataAdapter()
		{
			return new SqlDataAdapter();
		}

		public override IDataAdapter GetDataAdapter(IDbCommand cmd)
		{
			return new SqlDataAdapter((SqlCommand)cmd);
		}

		public override void SetDataAdapterCommands(IDataAdapter da, IDbCommand select, IDbCommand update, IDbCommand insert, IDbCommand delete)
		{
			SqlDataAdapter sda = (SqlDataAdapter)da;
			sda.SelectCommand = (SqlCommand)select;
			sda.UpdateCommand = (SqlCommand)update;
			sda.InsertCommand = (SqlCommand)insert;
			sda.DeleteCommand = (SqlCommand)delete;
		}

		public void ExecuteScript(string script)
		{
			string[] sql = Regex.Split(script, @"go\r\n", RegexOptions.Multiline);
			for (int i = 0; i < sql.Length; i++)
			{
				if (sql[i].Trim() == "")
					continue;
				IDbCommand cmd = CreateCommand(sql[i], CommandType.Text);
				cmd.ExecuteNonQuery();
			}
		}

		protected override void CheckParameter(IDataParameter prm)
		{
			if(prm.Value == DBNull.Value)
				return;

			switch (prm.DbType)
			{
				case DbType.DateTime:
					prm.Value = new SqlDateTime((DateTime)prm.Value);
					break;

				case DbType.Decimal:
					prm.Value = new SqlDecimal((decimal)prm.Value);
					break;

				case DbType.String:
					prm.Value = new SqlString((string)prm.Value);
					break;
			}
		}

		public override void SetParameterSize(IDataParameter prm, int size)
		{
			((SqlParameter)prm).Size = size;
		}
	}
}
