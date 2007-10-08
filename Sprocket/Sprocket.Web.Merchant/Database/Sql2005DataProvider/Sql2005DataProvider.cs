using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

using Sprocket;
using Sprocket.Web;
using Sprocket.Data;
using Sprocket.Utility;
using Sprocket.Web.Merchant.PayPal;

namespace Sprocket.Web.Merchant.Database
{
	class Sql2005DataProvider : IMerchantDataProvider
	{
		public void Initialise(Result result)
		{
			if (!result.Succeeded)
				return;
			SqlConnection conn = null;
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SqlServer2005Database db = (SqlServer2005Database)DatabaseManager.DatabaseEngine;
					string[] scripts = new string[]{
						"Sprocket.Web.Merchant.Database.Sql2005DataProvider.paypal-tables.sql",
						"Sprocket.Web.Merchant.Database.Sql2005DataProvider.paypal-procs.sql"
					};
					foreach (string sql in scripts)
					{
						Result r = db.ExecuteScript(conn, ResourceLoader.LoadTextResource(sql));
						if (!r.Succeeded)
						{
							result.SetFailed(sql + ": " + r.Message);
							return;
						}
					}

					scope.Complete();
				}
			}
			catch (Exception ex)
			{
				result.SetFailed(ex.Message);
				return;
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		public SqlParameter NewParameter(string name, object value, SqlDbType dbType)
		{
			SqlParameter prm = new SqlParameter(name, value);
			prm.SqlDbType = dbType;
			return prm;
		}

		public Type DatabaseHandlerType
		{
			get { return typeof(SqlServer2005Database); }
		}

		#region Members for PayPalTransactionResponse

		public Result Store(PayPalTransactionResponse payPalTransactionResponse)
		{
			try
			{
				using (TransactionScope scope = new TransactionScope())
				{
					SqlConnection conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SqlCommand cmd = new SqlCommand("PayPalTransactionResponse_Store", conn);
					cmd.CommandType = CommandType.StoredProcedure;
					SqlParameter prm = new SqlParameter("@TransactionID", payPalTransactionResponse.TransactionID);
					prm.Direction = ParameterDirection.InputOutput;
					cmd.Parameters.Add(prm);
					cmd.Parameters.Add(NewParameter("@txn_id", payPalTransactionResponse.Txn_id, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@txn_type", payPalTransactionResponse.Txn_type, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@auth_amount", payPalTransactionResponse.Auth_amount, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@auth_id", payPalTransactionResponse.Auth_id, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@auth_exp", payPalTransactionResponse.Auth_exp, SqlDbType.DateTime));
					cmd.Parameters.Add(NewParameter("@auth_status", payPalTransactionResponse.Auth_status, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@mc_gross_x", payPalTransactionResponse.Mc_gross_x, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_handling_x", payPalTransactionResponse.Mc_handling_x, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@num_cart_items", payPalTransactionResponse.Num_cart_items, SqlDbType.Int));
					cmd.Parameters.Add(NewParameter("@parent_txn_id", payPalTransactionResponse.Parent_txn_id, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@payment_date", payPalTransactionResponse.Payment_date, SqlDbType.DateTime));
					cmd.Parameters.Add(NewParameter("@payment_status", payPalTransactionResponse.Payment_status, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@payment_type", payPalTransactionResponse.Payment_type, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@pending_reason", payPalTransactionResponse.Pending_reason, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@reason_code", payPalTransactionResponse.Reason_code, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@remaining_settle", payPalTransactionResponse.Remaining_settle, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@transaction_entity", payPalTransactionResponse.Transaction_entity, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@invoice", payPalTransactionResponse.Invoice, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@memo", payPalTransactionResponse.Memo, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@tax", payPalTransactionResponse.Tax, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@business", payPalTransactionResponse.Business, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@item_name", payPalTransactionResponse.Item_name, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@item_number", payPalTransactionResponse.Item_number, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@quantity", payPalTransactionResponse.Quantity, SqlDbType.Int));
					cmd.Parameters.Add(NewParameter("@receiver_email", payPalTransactionResponse.Receiver_email, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@receiver_id", payPalTransactionResponse.Receiver_id, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_city", payPalTransactionResponse.Address_city, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_country", payPalTransactionResponse.Address_country, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_country_code", payPalTransactionResponse.Address_country_code, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_name", payPalTransactionResponse.Address_name, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_state", payPalTransactionResponse.Address_state, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_status", payPalTransactionResponse.Address_status, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_street", payPalTransactionResponse.Address_street, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@address_zip", payPalTransactionResponse.Address_zip, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@first_name", payPalTransactionResponse.First_name, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@last_name", payPalTransactionResponse.Last_name, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@payer_id", payPalTransactionResponse.Payer_id, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@payer_status", payPalTransactionResponse.Payer_status, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@residence_country", payPalTransactionResponse.Residence_country, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@exchange_rate", payPalTransactionResponse.Exchange_rate, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_fee", payPalTransactionResponse.Mc_fee, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_gross", payPalTransactionResponse.Mc_gross, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_handling#", payPalTransactionResponse.Mc_handlingamount, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_shipping#", payPalTransactionResponse.Mc_shippingamount, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@payment_fee", payPalTransactionResponse.Payment_fee, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@payment_gross", payPalTransactionResponse.Payment_gross, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@settle_amount", payPalTransactionResponse.Settle_amount, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@settle_currency", payPalTransactionResponse.Settle_currency, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@subscr_date", payPalTransactionResponse.Subscr_date, SqlDbType.DateTime));
					cmd.Parameters.Add(NewParameter("@subscr_effective", payPalTransactionResponse.Subscr_effective, SqlDbType.DateTime));
					cmd.Parameters.Add(NewParameter("@period1", payPalTransactionResponse.Period1, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@period2", payPalTransactionResponse.Period2, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@period3", payPalTransactionResponse.Period3, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@amount1", payPalTransactionResponse.Amount1, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@amount2", payPalTransactionResponse.Amount2, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@amount3", payPalTransactionResponse.Amount3, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_amount1", payPalTransactionResponse.Mc_amount1, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_amount2", payPalTransactionResponse.Mc_amount2, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_amount3", payPalTransactionResponse.Mc_amount3, SqlDbType.Decimal));
					cmd.Parameters.Add(NewParameter("@mc_currency", payPalTransactionResponse.Mc_currency, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@recurring", payPalTransactionResponse.Recurring, SqlDbType.Bit));
					cmd.Parameters.Add(NewParameter("@reattempt", payPalTransactionResponse.Reattempt, SqlDbType.Bit));
					cmd.Parameters.Add(NewParameter("@retry_at", payPalTransactionResponse.Retry_at, SqlDbType.DateTime));
					cmd.Parameters.Add(NewParameter("@recur_times", payPalTransactionResponse.Recur_times, SqlDbType.Int));
					cmd.Parameters.Add(NewParameter("@username", payPalTransactionResponse.Username, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@password", payPalTransactionResponse.Password, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@subscr_id", payPalTransactionResponse.Subscr_id, SqlDbType.NVarChar));
					cmd.Parameters.Add(NewParameter("@custom", payPalTransactionResponse.Custom, SqlDbType.NVarChar));
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
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
			return new Result();
		}

		public event InterruptableEventHandler<PayPalTransactionResponse> OnBeforeDeletePayPalTransactionResponse;
		public event NotificationEventHandler<PayPalTransactionResponse> OnPayPalTransactionResponseDeleted;
		public Result Delete(PayPalTransactionResponse payPalTransactionResponse)
		{
			Result result = new Result();
			if (OnBeforeDeletePayPalTransactionResponse != null)
				OnBeforeDeletePayPalTransactionResponse(payPalTransactionResponse, result);
			if (result.Succeeded)
			{
				try
				{
					using (TransactionScope scope = new TransactionScope())
					{
						SqlConnection conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
						SqlCommand cmd = new SqlCommand("PayPalTransactionResponse_Delete", conn);
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.Add(new SqlParameter("@TransactionID", payPalTransactionResponse.TransactionID));
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
					DatabaseManager.DatabaseEngine.ReleaseConnection();
				}
				if (OnPayPalTransactionResponseDeleted != null)
					OnPayPalTransactionResponseDeleted(payPalTransactionResponse);
			}
			return result;
		}

		public PayPalTransactionResponse SelectPayPalTransactionResponse(Guid id)
		{
			try
			{
				PayPalTransactionResponse entity;
				using (TransactionScope scope = new TransactionScope())
				{
					SqlConnection conn = (SqlConnection)DatabaseManager.DatabaseEngine.GetConnection();
					SqlCommand cmd = new SqlCommand("PayPalTransactionResponse_Select", conn);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.Add(new SqlParameter("@TransactionID", id));

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (!reader.Read())
							entity = null;
						else
							entity = new PayPalTransactionResponse(reader);
						reader.Close();
					}
					scope.Complete();
				}
				return entity;
			}
			finally
			{
				DatabaseManager.DatabaseEngine.ReleaseConnection();
			}
		}

		#endregion
	}
}
