using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Data;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using Sprocket.Utility;

using Sprocket.Web.CMS.SnapLayouts;

namespace Sprocket.Web.CMS
{
	public class SqlServer2005SnapLayoutsDataLayer : ISnapLayoutsDataLayer
	{
		public Type DatabaseHandlerType
		{
			get { return typeof(SqlServer2005Database); }
		}

		public Result InitialiseDatabase()
		{
			Result result;
			SqlConnection conn = null;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SqlServer2005Database db = (SqlServer2005Database)DatabaseManager.DatabaseEngine;
					result = db.ExecuteScript(conn, ResourceLoader.LoadTextResource("Sprocket.Web.CMS.SnapPanels.SqlServer2005.SnapPanels.sql"));
					if (result.Succeeded)
					{
						//result = db.ExecuteScript(conn, ResourceLoader.LoadTextResource("Sprocket.Security.SqlServer2005.procedures.sql"));
						//if (result.Succeeded)
						scope.Complete();

					}
				}
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection(conn);
			}
			return result;
		}

		public SqlParameter NewSqlParameter(string name, object value, SqlDbType dbType)
		{
			SqlParameter prm = new SqlParameter(name, value);
			prm.SqlDbType = dbType;
			return prm;
		}

		#region Members for SnapCanvas

		public Result Store(SnapCanvas snapCanvas)
		{
			SqlConnection conn = null;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SqlCommand cmd = new SqlCommand("SnapCanvas_Store", conn);
					cmd.CommandType = CommandType.StoredProcedure;
					SqlParameter prm = new SqlParameter("@SnapCanvasID", snapCanvas.SnapCanvasID);
					prm.Direction = ParameterDirection.InputOutput;
					cmd.Parameters.Add(prm);
					cmd.Parameters.Add(NewSqlParameter("@UnitWidth", snapCanvas.UnitWidth, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@UnitHeight", snapCanvas.UnitHeight, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@WidthExpandable", snapCanvas.WidthExpandable, SqlDbType.Bit));
					cmd.Parameters.Add(NewSqlParameter("@HeightExpandable", snapCanvas.HeightExpandable, SqlDbType.Bit));
					cmd.Parameters.Add(NewSqlParameter("@UnitSize", snapCanvas.UnitSize, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@GapSize", snapCanvas.GapSize, SqlDbType.SmallInt));
					cmd.ExecuteNonQuery();
					snapCanvas.SnapCanvasID = (long)prm.Value;
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result(ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection(conn);
			}
			return new Result();
		}

		public event InterruptableEventHandler<SnapCanvas> OnBeforeDeleteSnapCanvas;
		public event NotificationEventHandler<SnapCanvas> OnSnapCanvasDeleted;
		public Result Delete(SnapCanvas snapCanvas)
		{
			Result result = new Result();
			if (OnBeforeDeleteSnapCanvas != null)
				OnBeforeDeleteSnapCanvas(snapCanvas, result);
			SqlConnection conn = null;
			if (result.Succeeded)
			{
				try
				{
					using (TransactionScope scope = new TransactionScope())
					{
						conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
						SqlCommand cmd = new SqlCommand("SnapCanvas_Delete", conn);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@SnapCanvasID", snapCanvas.SnapCanvasID));
						cmd.ExecuteNonQuery();
						scope.Complete();
					}
				}
				catch (Exception ex)
				{
					return new Result(ex.Message);
				}
				finally
				{
					DatabaseManager.DatabaseEngine.ReleaseConnection(conn);
				}
				if (OnSnapCanvasDeleted != null)
					OnSnapCanvasDeleted(snapCanvas);
			}
			return result;
		}

		public SnapCanvas SelectSnapCanvas(long id)
		{
			using (SqlConnection conn = new SqlConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand("SnapCanvas_Select", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@SnapCanvasID", id));
				SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				SnapCanvas entity;
				if (!reader.Read())
					entity = null;
				else
					entity = new SnapCanvas(reader);
				reader.Close();
				return entity;
			}
		}

		#endregion

		#region Members for SnapPanel

		public Result Store(SnapPanel snapPanel)
		{
			SqlConnection conn = null;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SqlCommand cmd = new SqlCommand("SnapPanel_Store", conn);
					cmd.CommandType = CommandType.StoredProcedure;
					SqlParameter prm = new SqlParameter("@SnapPanelID", snapPanel.SnapPanelID);
					prm.Direction = ParameterDirection.InputOutput;
					cmd.Parameters.Add(prm);
					cmd.Parameters.Add(NewSqlParameter("@SnapCanvasID", snapPanel.SnapCanvasID, SqlDbType.BigInt));
					cmd.Parameters.Add(NewSqlParameter("@WidgetTypeID", snapPanel.WidgetTypeID, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewSqlParameter("@UnitWidth", snapPanel.UnitWidth, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@UnitHeight", snapPanel.UnitHeight, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@UnitX", snapPanel.UnitX, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@UnitY", snapPanel.UnitY, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@MaxUnitWidth", snapPanel.MaxUnitWidth, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@MinUnitWidth", snapPanel.MinUnitWidth, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@MaxUnitHeight", snapPanel.MaxUnitHeight, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@MinUnitHeight", snapPanel.MinUnitHeight, SqlDbType.SmallInt));
					cmd.Parameters.Add(NewSqlParameter("@LockPosition", snapPanel.LockPosition, SqlDbType.Bit));
					cmd.Parameters.Add(NewSqlParameter("@LockSize", snapPanel.LockSize, SqlDbType.Bit));
					cmd.Parameters.Add(NewSqlParameter("@AllowDelete", snapPanel.AllowDelete, SqlDbType.Bit));
					cmd.ExecuteNonQuery();
					snapPanel.SnapPanelID = (long)prm.Value;
					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				return new Result(ex.Message);
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection(conn);
			}
			return new Result();
		}

		public event InterruptableEventHandler<SnapPanel> OnBeforeDeleteSnapPanel;
		public event NotificationEventHandler<SnapPanel> OnSnapPanelDeleted;
		public Result Delete(SnapPanel snapPanel)
		{
			Result result = new Result();
			if (OnBeforeDeleteSnapPanel != null)
				OnBeforeDeleteSnapPanel(snapPanel, result);
			SqlConnection conn = null;
			if (result.Succeeded)
			{
				try
				{
					using (TransactionScope scope = new TransactionScope())
					{
						conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
						SqlCommand cmd = new SqlCommand("SnapPanel_Delete", conn);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@SnapPanelID", snapPanel.SnapPanelID));
						cmd.ExecuteNonQuery();
						scope.Complete();
					}
				}
				catch (Exception ex)
				{
					return new Result(ex.Message);
				}
				finally
				{
					DatabaseManager.DatabaseEngine.ReleaseConnection(conn);
				}
				if (OnSnapPanelDeleted != null)
					OnSnapPanelDeleted(snapPanel);
			}
			return result;
		}

		public SnapPanel SelectSnapPanel(long id)
		{
			using (SqlConnection conn = new SqlConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand("SnapPanel_Select", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@SnapPanelID", id));
				SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				SnapPanel entity;
				if (!reader.Read())
					entity = null;
				else
					entity = new SnapPanel(reader);
				reader.Close();
				return entity;
			}
		}

		#endregion

		public List<SnapPanel> ListPanelsForCanvas(long canvasID)
		{
			using (SqlConnection conn = new SqlConnection(DatabaseManager.DatabaseEngine.ConnectionString))
			{
				conn.Open();
				SqlCommand cmd = new SqlCommand("SnapCanvas_ListPanels", conn);
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@SnapCanvasID", canvasID));
				SqlDataReader reader = cmd.ExecuteReader();
				List<SnapPanel> list = new List<SnapPanel>();
				while (reader.Read())
					list.Add(new SnapPanel(reader));
				reader.Close();
				conn.Close();
				return list;
			}
		}
	}
}
