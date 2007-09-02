using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Globalization;
using System.Web;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Utility;

namespace Sprocket.Web.Merchant.PayPal
{
	public class PayPalTransactionResponse
	{
		#region Constructor, Fields, Properties, JSON Methods
		#region Fields

		protected Guid transactionID = Guid.NewGuid();
		protected string txn_id = "";
		protected string txn_type = null;
		protected decimal? auth_amount = null;
		protected string auth_id = null;
		protected DateTime? auth_exp = null;
		protected string auth_status = null;
		protected decimal? mc_gross_x = null;
		protected decimal? mc_handling_x = null;
		protected int? num_cart_items = null;
		protected string parent_txn_id = null;
		protected DateTime? payment_date = null;
		protected string payment_status = null;
		protected string payment_type = null;
		protected string pending_reason = null;
		protected string reason_code = null;
		protected decimal? remaining_settle = null;
		protected string transaction_entity = null;
		protected string invoice = null;
		protected string memo = null;
		protected decimal? tax = null;
		protected string business = null;
		protected string item_name = null;
		protected string item_number = null;
		protected int? quantity = null;
		protected string receiver_email = null;
		protected string receiver_id = null;
		protected string address_city = null;
		protected string address_country = null;
		protected string address_country_code = null;
		protected string address_name = null;
		protected string address_state = null;
		protected string address_status = null;
		protected string address_street = null;
		protected string address_zip = null;
		protected string first_name = null;
		protected string last_name = null;
		protected string payer_id = null;
		protected string payer_status = null;
		protected string residence_country = null;
		protected decimal? exchange_rate = null;
		protected decimal? mc_fee = null;
		protected decimal? mc_gross = null;
		protected decimal? mc_handlingamount = null;
		protected decimal? mc_shippingamount = null;
		protected decimal? payment_fee = null;
		protected decimal? payment_gross = null;
		protected decimal? settle_amount = null;
		protected string settle_currency = null;
		protected DateTime? subscr_date = null;
		protected DateTime? subscr_effective = null;
		protected string period1 = null;
		protected string period2 = null;
		protected string period3 = null;
		protected decimal? amount1 = null;
		protected decimal? amount2 = null;
		protected decimal? amount3 = null;
		protected decimal? mc_amount1 = null;
		protected decimal? mc_amount2 = null;
		protected decimal? mc_amount3 = null;
		protected string mc_currency = null;
		protected bool? recurring = null;
		protected bool? reattempt = null;
		protected DateTime? retry_at = null;
		protected int? recur_times = null;
		protected string username = null;
		protected string password = null;
		protected string subscr_id = null;
		protected string custom = null;

		#endregion

		#region Properties

		///<summary>
		///Gets or sets the value for TransactionID
		///</summary>
		public Guid TransactionID
		{
			get { return transactionID; }
			set { transactionID = value; }
		}

		///<summary>
		///Gets or sets the value for Txn_id
		///</summary>
		public string Txn_id
		{
			get { return txn_id; }
			set { txn_id = value; }
		}

		///<summary>
		///Gets or sets the value for Txn_type
		///</summary>
		public string Txn_type
		{
			get { return txn_type; }
			set { txn_type = value; }
		}

		///<summary>
		///Gets or sets the value for Auth_amount
		///</summary>
		public decimal? Auth_amount
		{
			get { return auth_amount; }
			set { auth_amount = value; }
		}

		///<summary>
		///Gets or sets the value for Auth_id
		///</summary>
		public string Auth_id
		{
			get { return auth_id; }
			set { auth_id = value; }
		}

		///<summary>
		///Gets or sets the value for Auth_exp
		///</summary>
		public DateTime? Auth_exp
		{
			get { return auth_exp; }
			set { auth_exp = value; }
		}

		///<summary>
		///Gets or sets the value for Auth_status
		///</summary>
		public string Auth_status
		{
			get { return auth_status; }
			set { auth_status = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_gross_x
		///</summary>
		public decimal? Mc_gross_x
		{
			get { return mc_gross_x; }
			set { mc_gross_x = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_handling_x
		///</summary>
		public decimal? Mc_handling_x
		{
			get { return mc_handling_x; }
			set { mc_handling_x = value; }
		}

		///<summary>
		///Gets or sets the value for Num_cart_items
		///</summary>
		public int? Num_cart_items
		{
			get { return num_cart_items; }
			set { num_cart_items = value; }
		}

		///<summary>
		///Gets or sets the value for Parent_txn_id
		///</summary>
		public string Parent_txn_id
		{
			get { return parent_txn_id; }
			set { parent_txn_id = value; }
		}

		///<summary>
		///Gets or sets the value for Payment_date
		///</summary>
		public DateTime? Payment_date
		{
			get { return payment_date; }
			set { payment_date = value; }
		}

		///<summary>
		///Gets or sets the value for Payment_status
		///</summary>
		public string Payment_status
		{
			get { return payment_status; }
			set { payment_status = value; }
		}

		///<summary>
		///Gets or sets the value for Payment_type
		///</summary>
		public string Payment_type
		{
			get { return payment_type; }
			set { payment_type = value; }
		}

		///<summary>
		///Gets or sets the value for Pending_reason
		///</summary>
		public string Pending_reason
		{
			get { return pending_reason; }
			set { pending_reason = value; }
		}

		///<summary>
		///Gets or sets the value for Reason_code
		///</summary>
		public string Reason_code
		{
			get { return reason_code; }
			set { reason_code = value; }
		}

		///<summary>
		///Gets or sets the value for Remaining_settle
		///</summary>
		public decimal? Remaining_settle
		{
			get { return remaining_settle; }
			set { remaining_settle = value; }
		}

		///<summary>
		///Gets or sets the value for Transaction_entity
		///</summary>
		public string Transaction_entity
		{
			get { return transaction_entity; }
			set { transaction_entity = value; }
		}

		///<summary>
		///Gets or sets the value for Invoice
		///</summary>
		public string Invoice
		{
			get { return invoice; }
			set { invoice = value; }
		}

		///<summary>
		///Gets or sets the value for Memo
		///</summary>
		public string Memo
		{
			get { return memo; }
			set { memo = value; }
		}

		///<summary>
		///Gets or sets the value for Tax
		///</summary>
		public decimal? Tax
		{
			get { return tax; }
			set { tax = value; }
		}

		///<summary>
		///Gets or sets the value for Business
		///</summary>
		public string Business
		{
			get { return business; }
			set { business = value; }
		}

		///<summary>
		///Gets or sets the value for Item_name
		///</summary>
		public string Item_name
		{
			get { return item_name; }
			set { item_name = value; }
		}

		///<summary>
		///Gets or sets the value for Item_number
		///</summary>
		public string Item_number
		{
			get { return item_number; }
			set { item_number = value; }
		}

		///<summary>
		///Gets or sets the value for Quantity
		///</summary>
		public int? Quantity
		{
			get { return quantity; }
			set { quantity = value; }
		}

		///<summary>
		///Gets or sets the value for Receiver_email
		///</summary>
		public string Receiver_email
		{
			get { return receiver_email; }
			set { receiver_email = value; }
		}

		///<summary>
		///Gets or sets the value for Receiver_id
		///</summary>
		public string Receiver_id
		{
			get { return receiver_id; }
			set { receiver_id = value; }
		}

		///<summary>
		///Gets or sets the value for Address_city
		///</summary>
		public string Address_city
		{
			get { return address_city; }
			set { address_city = value; }
		}

		///<summary>
		///Gets or sets the value for Address_country
		///</summary>
		public string Address_country
		{
			get { return address_country; }
			set { address_country = value; }
		}

		///<summary>
		///Gets or sets the value for Address_country_code
		///</summary>
		public string Address_country_code
		{
			get { return address_country_code; }
			set { address_country_code = value; }
		}

		///<summary>
		///Gets or sets the value for Address_name
		///</summary>
		public string Address_name
		{
			get { return address_name; }
			set { address_name = value; }
		}

		///<summary>
		///Gets or sets the value for Address_state
		///</summary>
		public string Address_state
		{
			get { return address_state; }
			set { address_state = value; }
		}

		///<summary>
		///Gets or sets the value for Address_status
		///</summary>
		public string Address_status
		{
			get { return address_status; }
			set { address_status = value; }
		}

		///<summary>
		///Gets or sets the value for Address_street
		///</summary>
		public string Address_street
		{
			get { return address_street; }
			set { address_street = value; }
		}

		///<summary>
		///Gets or sets the value for Address_zip
		///</summary>
		public string Address_zip
		{
			get { return address_zip; }
			set { address_zip = value; }
		}

		///<summary>
		///Gets or sets the value for First_name
		///</summary>
		public string First_name
		{
			get { return first_name; }
			set { first_name = value; }
		}

		///<summary>
		///Gets or sets the value for Last_name
		///</summary>
		public string Last_name
		{
			get { return last_name; }
			set { last_name = value; }
		}

		///<summary>
		///Gets or sets the value for Payer_id
		///</summary>
		public string Payer_id
		{
			get { return payer_id; }
			set { payer_id = value; }
		}

		///<summary>
		///Gets or sets the value for Payer_status
		///</summary>
		public string Payer_status
		{
			get { return payer_status; }
			set { payer_status = value; }
		}

		///<summary>
		///Gets or sets the value for Residence_country
		///</summary>
		public string Residence_country
		{
			get { return residence_country; }
			set { residence_country = value; }
		}

		///<summary>
		///Gets or sets the value for Exchange_rate
		///</summary>
		public decimal? Exchange_rate
		{
			get { return exchange_rate; }
			set { exchange_rate = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_fee
		///</summary>
		public decimal? Mc_fee
		{
			get { return mc_fee; }
			set { mc_fee = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_gross
		///</summary>
		public decimal? Mc_gross
		{
			get { return mc_gross; }
			set { mc_gross = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_handlingamount
		///</summary>
		public decimal? Mc_handlingamount
		{
			get { return mc_handlingamount; }
			set { mc_handlingamount = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_shippingamount
		///</summary>
		public decimal? Mc_shippingamount
		{
			get { return mc_shippingamount; }
			set { mc_shippingamount = value; }
		}

		///<summary>
		///Gets or sets the value for Payment_fee
		///</summary>
		public decimal? Payment_fee
		{
			get { return payment_fee; }
			set { payment_fee = value; }
		}

		///<summary>
		///Gets or sets the value for Payment_gross
		///</summary>
		public decimal? Payment_gross
		{
			get { return payment_gross; }
			set { payment_gross = value; }
		}

		///<summary>
		///Gets or sets the value for Settle_amount
		///</summary>
		public decimal? Settle_amount
		{
			get { return settle_amount; }
			set { settle_amount = value; }
		}

		///<summary>
		///Gets or sets the value for Settle_currency
		///</summary>
		public string Settle_currency
		{
			get { return settle_currency; }
			set { settle_currency = value; }
		}

		///<summary>
		///Gets or sets the value for Subscr_date
		///</summary>
		public DateTime? Subscr_date
		{
			get { return subscr_date; }
			set { subscr_date = value; }
		}

		///<summary>
		///Gets or sets the value for Subscr_effective
		///</summary>
		public DateTime? Subscr_effective
		{
			get { return subscr_effective; }
			set { subscr_effective = value; }
		}

		///<summary>
		///Gets or sets the value for Period1
		///</summary>
		public string Period1
		{
			get { return period1; }
			set { period1 = value; }
		}

		///<summary>
		///Gets or sets the value for Period2
		///</summary>
		public string Period2
		{
			get { return period2; }
			set { period2 = value; }
		}

		///<summary>
		///Gets or sets the value for Period3
		///</summary>
		public string Period3
		{
			get { return period3; }
			set { period3 = value; }
		}

		///<summary>
		///Gets or sets the value for Amount1
		///</summary>
		public decimal? Amount1
		{
			get { return amount1; }
			set { amount1 = value; }
		}

		///<summary>
		///Gets or sets the value for Amount2
		///</summary>
		public decimal? Amount2
		{
			get { return amount2; }
			set { amount2 = value; }
		}

		///<summary>
		///Gets or sets the value for Amount3
		///</summary>
		public decimal? Amount3
		{
			get { return amount3; }
			set { amount3 = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_amount1
		///</summary>
		public decimal? Mc_amount1
		{
			get { return mc_amount1; }
			set { mc_amount1 = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_amount2
		///</summary>
		public decimal? Mc_amount2
		{
			get { return mc_amount2; }
			set { mc_amount2 = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_amount3
		///</summary>
		public decimal? Mc_amount3
		{
			get { return mc_amount3; }
			set { mc_amount3 = value; }
		}

		///<summary>
		///Gets or sets the value for Mc_currency
		///</summary>
		public string Mc_currency
		{
			get { return mc_currency; }
			set { mc_currency = value; }
		}

		///<summary>
		///Gets or sets the value for Recurring
		///</summary>
		public bool? Recurring
		{
			get { return recurring; }
			set { recurring = value; }
		}

		///<summary>
		///Gets or sets the value for Reattempt
		///</summary>
		public bool? Reattempt
		{
			get { return reattempt; }
			set { reattempt = value; }
		}

		///<summary>
		///Gets or sets the value for Retry_at
		///</summary>
		public DateTime? Retry_at
		{
			get { return retry_at; }
			set { retry_at = value; }
		}

		///<summary>
		///Gets or sets the value for Recur_times
		///</summary>
		public int? Recur_times
		{
			get { return recur_times; }
			set { recur_times = value; }
		}

		///<summary>
		///Gets or sets the value for Username
		///</summary>
		public string Username
		{
			get { return username; }
			set { username = value; }
		}

		///<summary>
		///Gets or sets the value for Password
		///</summary>
		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		///<summary>
		///Gets or sets the value for Subscr_id
		///</summary>
		public string Subscr_id
		{
			get { return subscr_id; }
			set { subscr_id = value; }
		}

		///<summary>
		///Gets or sets the value for Custom
		///</summary>
		public string Custom
		{
			get { return custom; }
			set { custom = value; }
		}

		#endregion

		#region Constructors

		public PayPalTransactionResponse()
		{
		}

		public PayPalTransactionResponse(Guid transactionID, string txn_id, string txn_type, decimal? auth_amount, string auth_id, DateTime? auth_exp, string auth_status, decimal? mc_gross_x, decimal? mc_handling_x, int? num_cart_items, string parent_txn_id, DateTime? payment_date, string payment_status, string payment_type, string pending_reason, string reason_code, decimal? remaining_settle, string transaction_entity, string invoice, string memo, decimal? tax, string business, string item_name, string item_number, int? quantity, string receiver_email, string receiver_id, string address_city, string address_country, string address_country_code, string address_name, string address_state, string address_status, string address_street, string address_zip, string first_name, string last_name, string payer_id, string payer_status, string residence_country, decimal? exchange_rate, decimal? mc_fee, decimal? mc_gross, decimal? mc_handlingamount, decimal? mc_shippingamount, decimal? payment_fee, decimal? payment_gross, decimal? settle_amount, string settle_currency, DateTime? subscr_date, DateTime? subscr_effective, string period1, string period2, string period3, decimal? amount1, decimal? amount2, decimal? amount3, decimal? mc_amount1, decimal? mc_amount2, decimal? mc_amount3, string mc_currency, bool? recurring, bool? reattempt, DateTime? retry_at, int? recur_times, string username, string password, string subscr_id, string custom)
		{
			this.transactionID = transactionID;
			this.txn_id = txn_id;
			this.txn_type = txn_type;
			this.auth_amount = auth_amount;
			this.auth_id = auth_id;
			this.auth_exp = auth_exp;
			this.auth_status = auth_status;
			this.mc_gross_x = mc_gross_x;
			this.mc_handling_x = mc_handling_x;
			this.num_cart_items = num_cart_items;
			this.parent_txn_id = parent_txn_id;
			this.payment_date = payment_date;
			this.payment_status = payment_status;
			this.payment_type = payment_type;
			this.pending_reason = pending_reason;
			this.reason_code = reason_code;
			this.remaining_settle = remaining_settle;
			this.transaction_entity = transaction_entity;
			this.invoice = invoice;
			this.memo = memo;
			this.tax = tax;
			this.business = business;
			this.item_name = item_name;
			this.item_number = item_number;
			this.quantity = quantity;
			this.receiver_email = receiver_email;
			this.receiver_id = receiver_id;
			this.address_city = address_city;
			this.address_country = address_country;
			this.address_country_code = address_country_code;
			this.address_name = address_name;
			this.address_state = address_state;
			this.address_status = address_status;
			this.address_street = address_street;
			this.address_zip = address_zip;
			this.first_name = first_name;
			this.last_name = last_name;
			this.payer_id = payer_id;
			this.payer_status = payer_status;
			this.residence_country = residence_country;
			this.exchange_rate = exchange_rate;
			this.mc_fee = mc_fee;
			this.mc_gross = mc_gross;
			this.mc_handlingamount = mc_handlingamount;
			this.mc_shippingamount = mc_shippingamount;
			this.payment_fee = payment_fee;
			this.payment_gross = payment_gross;
			this.settle_amount = settle_amount;
			this.settle_currency = settle_currency;
			this.subscr_date = subscr_date;
			this.subscr_effective = subscr_effective;
			this.period1 = period1;
			this.period2 = period2;
			this.period3 = period3;
			this.amount1 = amount1;
			this.amount2 = amount2;
			this.amount3 = amount3;
			this.mc_amount1 = mc_amount1;
			this.mc_amount2 = mc_amount2;
			this.mc_amount3 = mc_amount3;
			this.mc_currency = mc_currency;
			this.recurring = recurring;
			this.reattempt = reattempt;
			this.retry_at = retry_at;
			this.recur_times = recur_times;
			this.username = username;
			this.password = password;
			this.subscr_id = subscr_id;
			this.custom = custom;
		}

		public PayPalTransactionResponse(IDataReader reader)
		{
			if (reader["TransactionID"] != DBNull.Value) transactionID = (Guid)reader["TransactionID"];
			if (reader["txn_id"] != DBNull.Value) txn_id = (string)reader["txn_id"];
			if (reader["txn_type"] != DBNull.Value) txn_type = (string)reader["txn_type"];
			if (reader["auth_amount"] != DBNull.Value) auth_amount = (decimal?)reader["auth_amount"];
			if (reader["auth_id"] != DBNull.Value) auth_id = (string)reader["auth_id"];
			if (reader["auth_exp"] != DBNull.Value) auth_exp = (DateTime?)reader["auth_exp"];
			if (reader["auth_status"] != DBNull.Value) auth_status = (string)reader["auth_status"];
			if (reader["mc_gross_x"] != DBNull.Value) mc_gross_x = (decimal?)reader["mc_gross_x"];
			if (reader["mc_handling_x"] != DBNull.Value) mc_handling_x = (decimal?)reader["mc_handling_x"];
			if (reader["num_cart_items"] != DBNull.Value) num_cart_items = (int?)reader["num_cart_items"];
			if (reader["parent_txn_id"] != DBNull.Value) parent_txn_id = (string)reader["parent_txn_id"];
			if (reader["payment_date"] != DBNull.Value) payment_date = (DateTime?)reader["payment_date"];
			if (reader["payment_status"] != DBNull.Value) payment_status = (string)reader["payment_status"];
			if (reader["payment_type"] != DBNull.Value) payment_type = (string)reader["payment_type"];
			if (reader["pending_reason"] != DBNull.Value) pending_reason = (string)reader["pending_reason"];
			if (reader["reason_code"] != DBNull.Value) reason_code = (string)reader["reason_code"];
			if (reader["remaining_settle"] != DBNull.Value) remaining_settle = (decimal?)reader["remaining_settle"];
			if (reader["transaction_entity"] != DBNull.Value) transaction_entity = (string)reader["transaction_entity"];
			if (reader["invoice"] != DBNull.Value) invoice = (string)reader["invoice"];
			if (reader["memo"] != DBNull.Value) memo = (string)reader["memo"];
			if (reader["tax"] != DBNull.Value) tax = (decimal?)reader["tax"];
			if (reader["business"] != DBNull.Value) business = (string)reader["business"];
			if (reader["item_name"] != DBNull.Value) item_name = (string)reader["item_name"];
			if (reader["item_number"] != DBNull.Value) item_number = (string)reader["item_number"];
			if (reader["quantity"] != DBNull.Value) quantity = (int?)reader["quantity"];
			if (reader["receiver_email"] != DBNull.Value) receiver_email = (string)reader["receiver_email"];
			if (reader["receiver_id"] != DBNull.Value) receiver_id = (string)reader["receiver_id"];
			if (reader["address_city"] != DBNull.Value) address_city = (string)reader["address_city"];
			if (reader["address_country"] != DBNull.Value) address_country = (string)reader["address_country"];
			if (reader["address_country_code"] != DBNull.Value) address_country_code = (string)reader["address_country_code"];
			if (reader["address_name"] != DBNull.Value) address_name = (string)reader["address_name"];
			if (reader["address_state"] != DBNull.Value) address_state = (string)reader["address_state"];
			if (reader["address_status"] != DBNull.Value) address_status = (string)reader["address_status"];
			if (reader["address_street"] != DBNull.Value) address_street = (string)reader["address_street"];
			if (reader["address_zip"] != DBNull.Value) address_zip = (string)reader["address_zip"];
			if (reader["first_name"] != DBNull.Value) first_name = (string)reader["first_name"];
			if (reader["last_name"] != DBNull.Value) last_name = (string)reader["last_name"];
			if (reader["payer_id"] != DBNull.Value) payer_id = (string)reader["payer_id"];
			if (reader["payer_status"] != DBNull.Value) payer_status = (string)reader["payer_status"];
			if (reader["residence_country"] != DBNull.Value) residence_country = (string)reader["residence_country"];
			if (reader["exchange_rate"] != DBNull.Value) exchange_rate = (decimal?)reader["exchange_rate"];
			if (reader["mc_fee"] != DBNull.Value) mc_fee = (decimal?)reader["mc_fee"];
			if (reader["mc_gross"] != DBNull.Value) mc_gross = (decimal?)reader["mc_gross"];
			if (reader["mc_handling#"] != DBNull.Value) mc_handlingamount = (decimal?)reader["mc_handling#"];
			if (reader["mc_shipping#"] != DBNull.Value) mc_shippingamount = (decimal?)reader["mc_shipping#"];
			if (reader["payment_fee"] != DBNull.Value) payment_fee = (decimal?)reader["payment_fee"];
			if (reader["payment_gross"] != DBNull.Value) payment_gross = (decimal?)reader["payment_gross"];
			if (reader["settle_amount"] != DBNull.Value) settle_amount = (decimal?)reader["settle_amount"];
			if (reader["settle_currency"] != DBNull.Value) settle_currency = (string)reader["settle_currency"];
			if (reader["subscr_date"] != DBNull.Value) subscr_date = (DateTime?)reader["subscr_date"];
			if (reader["subscr_effective"] != DBNull.Value) subscr_effective = (DateTime?)reader["subscr_effective"];
			if (reader["period1"] != DBNull.Value) period1 = (string)reader["period1"];
			if (reader["period2"] != DBNull.Value) period2 = (string)reader["period2"];
			if (reader["period3"] != DBNull.Value) period3 = (string)reader["period3"];
			if (reader["amount1"] != DBNull.Value) amount1 = (decimal?)reader["amount1"];
			if (reader["amount2"] != DBNull.Value) amount2 = (decimal?)reader["amount2"];
			if (reader["amount3"] != DBNull.Value) amount3 = (decimal?)reader["amount3"];
			if (reader["mc_amount1"] != DBNull.Value) mc_amount1 = (decimal?)reader["mc_amount1"];
			if (reader["mc_amount2"] != DBNull.Value) mc_amount2 = (decimal?)reader["mc_amount2"];
			if (reader["mc_amount3"] != DBNull.Value) mc_amount3 = (decimal?)reader["mc_amount3"];
			if (reader["mc_currency"] != DBNull.Value) mc_currency = (string)reader["mc_currency"];
			if (reader["recurring"] != DBNull.Value) recurring = (bool?)reader["recurring"];
			if (reader["reattempt"] != DBNull.Value) reattempt = (bool?)reader["reattempt"];
			if (reader["retry_at"] != DBNull.Value) retry_at = (DateTime?)reader["retry_at"];
			if (reader["recur_times"] != DBNull.Value) recur_times = (int?)reader["recur_times"];
			if (reader["username"] != DBNull.Value) username = (string)reader["username"];
			if (reader["password"] != DBNull.Value) password = (string)reader["password"];
			if (reader["subscr_id"] != DBNull.Value) subscr_id = (string)reader["subscr_id"];
			if (reader["custom"] != DBNull.Value) custom = (string)reader["custom"];
		}

		#endregion

		#region Clone
		public PayPalTransactionResponse Clone()
		{
			PayPalTransactionResponse copy = new PayPalTransactionResponse();
			copy.transactionID = transactionID;
			copy.txn_id = txn_id;
			copy.txn_type = txn_type;
			copy.auth_amount = auth_amount;
			copy.auth_id = auth_id;
			copy.auth_exp = auth_exp;
			copy.auth_status = auth_status;
			copy.mc_gross_x = mc_gross_x;
			copy.mc_handling_x = mc_handling_x;
			copy.num_cart_items = num_cart_items;
			copy.parent_txn_id = parent_txn_id;
			copy.payment_date = payment_date;
			copy.payment_status = payment_status;
			copy.payment_type = payment_type;
			copy.pending_reason = pending_reason;
			copy.reason_code = reason_code;
			copy.remaining_settle = remaining_settle;
			copy.transaction_entity = transaction_entity;
			copy.invoice = invoice;
			copy.memo = memo;
			copy.tax = tax;
			copy.business = business;
			copy.item_name = item_name;
			copy.item_number = item_number;
			copy.quantity = quantity;
			copy.receiver_email = receiver_email;
			copy.receiver_id = receiver_id;
			copy.address_city = address_city;
			copy.address_country = address_country;
			copy.address_country_code = address_country_code;
			copy.address_name = address_name;
			copy.address_state = address_state;
			copy.address_status = address_status;
			copy.address_street = address_street;
			copy.address_zip = address_zip;
			copy.first_name = first_name;
			copy.last_name = last_name;
			copy.payer_id = payer_id;
			copy.payer_status = payer_status;
			copy.residence_country = residence_country;
			copy.exchange_rate = exchange_rate;
			copy.mc_fee = mc_fee;
			copy.mc_gross = mc_gross;
			copy.mc_handlingamount = mc_handlingamount;
			copy.mc_shippingamount = mc_shippingamount;
			copy.payment_fee = payment_fee;
			copy.payment_gross = payment_gross;
			copy.settle_amount = settle_amount;
			copy.settle_currency = settle_currency;
			copy.subscr_date = subscr_date;
			copy.subscr_effective = subscr_effective;
			copy.period1 = period1;
			copy.period2 = period2;
			copy.period3 = period3;
			copy.amount1 = amount1;
			copy.amount2 = amount2;
			copy.amount3 = amount3;
			copy.mc_amount1 = mc_amount1;
			copy.mc_amount2 = mc_amount2;
			copy.mc_amount3 = mc_amount3;
			copy.mc_currency = mc_currency;
			copy.recurring = recurring;
			copy.reattempt = reattempt;
			copy.retry_at = retry_at;
			copy.recur_times = recur_times;
			copy.username = username;
			copy.password = password;
			copy.subscr_id = subscr_id;
			copy.custom = custom;
			return copy;
		}
		#endregion

		#region JSON Methods

		/// <summary>
		/// Writes this entity out as a JSON formatted string
		/// </summary>
		public void WriteJSON(StringWriter writer)
		{
			writer.Write("{");
			JSON.EncodeNameValuePair(writer, "TransactionID", transactionID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Txn_id", txn_id);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Txn_type", txn_type);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Auth_amount", auth_amount);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Auth_id", auth_id);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Auth_exp", auth_exp);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Auth_status", auth_status);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_gross_x", mc_gross_x);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_handling_x", mc_handling_x);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Num_cart_items", num_cart_items);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Parent_txn_id", parent_txn_id);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payment_date", payment_date);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payment_status", payment_status);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payment_type", payment_type);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Pending_reason", pending_reason);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Reason_code", reason_code);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Remaining_settle", remaining_settle);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Transaction_entity", transaction_entity);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Invoice", invoice);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Memo", memo);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Tax", tax);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Business", business);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Item_name", item_name);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Item_number", item_number);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Quantity", quantity);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Receiver_email", receiver_email);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Receiver_id", receiver_id);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_city", address_city);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_country", address_country);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_country_code", address_country_code);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_name", address_name);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_state", address_state);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_status", address_status);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_street", address_street);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Address_zip", address_zip);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "First_name", first_name);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Last_name", last_name);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payer_id", payer_id);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payer_status", payer_status);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Residence_country", residence_country);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Exchange_rate", exchange_rate);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_fee", mc_fee);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_gross", mc_gross);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_handlingamount", mc_handlingamount);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_shippingamount", mc_shippingamount);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payment_fee", payment_fee);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Payment_gross", payment_gross);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Settle_amount", settle_amount);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Settle_currency", settle_currency);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Subscr_date", subscr_date);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Subscr_effective", subscr_effective);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Period1", period1);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Period2", period2);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Period3", period3);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Amount1", amount1);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Amount2", amount2);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Amount3", amount3);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_amount1", mc_amount1);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_amount2", mc_amount2);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_amount3", mc_amount3);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Mc_currency", mc_currency);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Recurring", recurring);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Reattempt", reattempt);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Retry_at", retry_at);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Recur_times", recur_times);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Username", username);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Password", password);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Subscr_id", subscr_id);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Custom", custom);
			writer.Write("}");
		}

		public void LoadJSON(object json)
		{
			Dictionary<string, object> values = json as Dictionary<string, object>;
			if (values == null) return;
			transactionID = (Guid)values["TransactionID"];
			txn_id = (string)values["Txn_id"];
			txn_type = (string)values["Txn_type"];
			auth_amount = (decimal?)values["Auth_amount"];
			auth_id = (string)values["Auth_id"];
			auth_exp = (DateTime?)values["Auth_exp"];
			auth_status = (string)values["Auth_status"];
			mc_gross_x = (decimal?)values["Mc_gross_x"];
			mc_handling_x = (decimal?)values["Mc_handling_x"];
			num_cart_items = (int?)values["Num_cart_items"];
			parent_txn_id = (string)values["Parent_txn_id"];
			payment_date = (DateTime?)values["Payment_date"];
			payment_status = (string)values["Payment_status"];
			payment_type = (string)values["Payment_type"];
			pending_reason = (string)values["Pending_reason"];
			reason_code = (string)values["Reason_code"];
			remaining_settle = (decimal?)values["Remaining_settle"];
			transaction_entity = (string)values["Transaction_entity"];
			invoice = (string)values["Invoice"];
			memo = (string)values["Memo"];
			tax = (decimal?)values["Tax"];
			business = (string)values["Business"];
			item_name = (string)values["Item_name"];
			item_number = (string)values["Item_number"];
			quantity = (int?)values["Quantity"];
			receiver_email = (string)values["Receiver_email"];
			receiver_id = (string)values["Receiver_id"];
			address_city = (string)values["Address_city"];
			address_country = (string)values["Address_country"];
			address_country_code = (string)values["Address_country_code"];
			address_name = (string)values["Address_name"];
			address_state = (string)values["Address_state"];
			address_status = (string)values["Address_status"];
			address_street = (string)values["Address_street"];
			address_zip = (string)values["Address_zip"];
			first_name = (string)values["First_name"];
			last_name = (string)values["Last_name"];
			payer_id = (string)values["Payer_id"];
			payer_status = (string)values["Payer_status"];
			residence_country = (string)values["Residence_country"];
			exchange_rate = (decimal?)values["Exchange_rate"];
			mc_fee = (decimal?)values["Mc_fee"];
			mc_gross = (decimal?)values["Mc_gross"];
			mc_handlingamount = (decimal?)values["Mc_handlingamount"];
			mc_shippingamount = (decimal?)values["Mc_shippingamount"];
			payment_fee = (decimal?)values["Payment_fee"];
			payment_gross = (decimal?)values["Payment_gross"];
			settle_amount = (decimal?)values["Settle_amount"];
			settle_currency = (string)values["Settle_currency"];
			subscr_date = (DateTime?)values["Subscr_date"];
			subscr_effective = (DateTime?)values["Subscr_effective"];
			period1 = (string)values["Period1"];
			period2 = (string)values["Period2"];
			period3 = (string)values["Period3"];
			amount1 = (decimal?)values["Amount1"];
			amount2 = (decimal?)values["Amount2"];
			amount3 = (decimal?)values["Amount3"];
			mc_amount1 = (decimal?)values["Mc_amount1"];
			mc_amount2 = (decimal?)values["Mc_amount2"];
			mc_amount3 = (decimal?)values["Mc_amount3"];
			mc_currency = (string)values["Mc_currency"];
			recurring = (bool?)values["Recurring"];
			reattempt = (bool?)values["Reattempt"];
			retry_at = (DateTime?)values["Retry_at"];
			recur_times = (int?)values["Recur_times"];
			username = (string)values["Username"];
			password = (string)values["Password"];
			subscr_id = (string)values["Subscr_id"];
			custom = (string)values["Custom"];
		}

		#endregion

		/// <summary>
		/// Populates the class fields with values derived from strings. Use this method
		/// to take a list of string values from a fieldname/value collection and assign
		/// them to their native fields in the class.
		/// </summary>
		public void Populate(Dictionary<string, string> values)
		{
			foreach (string key in values.Keys)
				Populate(key.ToString(), values[key].ToString());
		}

		/// <summary>
		/// Populates the class fields with values derived from strings. Use this method
		/// to take a list of string values from a fieldname/value collection and assign
		/// them to their native fields in the class.
		/// </summary>
		public void Populate(NameValueCollection values)
		{
			foreach (string key in values.Keys)
				Populate(key, values[key].ToString());
		}

		/// <summary>
		/// Populates the class fields with values derived from strings. Use this method
		/// to take a list of string values from a fieldname/value collection and assign
		/// them to their native fields in the class.
		/// </summary>
		public bool Populate(string fieldName, string value)
		{
			switch (fieldName)
			{
				case "TransactionID": if (value.Length > 0) transactionID = new Guid(value); return true;
				case "txn_id": txn_id = value; return true;
				case "txn_type": txn_type = value; return true;
				case "auth_amount": if (value.Length > 0) auth_amount = Decimal.Parse(value); return true;
				case "auth_id": auth_id = value; return true;
				case "auth_exp": if (value.Length > 0) auth_exp = ParseDateTime(value); return true;
				case "auth_status": auth_status = value; return true;
				case "mc_gross_x": if (value.Length > 0) mc_gross_x = Decimal.Parse(value); return true;
				case "mc_handling_x": if (value.Length > 0) mc_handling_x = Decimal.Parse(value); return true;
				case "num_cart_items": if (value.Length > 0) num_cart_items = Int32.Parse(value); return true;
				case "parent_txn_id": parent_txn_id = value; return true;
				case "payment_date": if (value.Length > 0) payment_date = ParseDateTime(value); return true;
				case "payment_status": payment_status = value; return true;
				case "payment_type": payment_type = value; return true;
				case "pending_reason": pending_reason = value; return true;
				case "reason_code": reason_code = value; return true;
				case "remaining_settle": if (value.Length > 0) remaining_settle = Decimal.Parse(value); return true;
				case "transaction_entity": transaction_entity = value; return true;
				case "invoice": invoice = value; return true;
				case "memo": memo = value; return true;
				case "tax": if (value.Length > 0) tax = Decimal.Parse(value); return true;
				case "business": business = value; return true;
				case "item_name": item_name = value; return true;
				case "item_number": item_number = value; return true;
				case "quantity": if (value.Length > 0) quantity = Int32.Parse(value); return true;
				case "receiver_email": receiver_email = value; return true;
				case "receiver_id": receiver_id = value; return true;
				case "address_city": address_city = value; return true;
				case "address_country": address_country = value; return true;
				case "address_country_code": address_country_code = value; return true;
				case "address_name": address_name = value; return true;
				case "address_state": address_state = value; return true;
				case "address_status": address_status = value; return true;
				case "address_street": address_street = value; return true;
				case "address_zip": address_zip = value; return true;
				case "first_name": first_name = value; return true;
				case "last_name": last_name = value; return true;
				case "payer_id": payer_id = value; return true;
				case "payer_status": payer_status = value; return true;
				case "residence_country": residence_country = value; return true;
				case "exchange_rate": if (value.Length > 0) exchange_rate = Decimal.Parse(value); return true;
				case "mc_fee": if (value.Length > 0) mc_fee = Decimal.Parse(value); return true;
				case "mc_gross": if (value.Length > 0) mc_gross = Decimal.Parse(value); return true;
				case "mc_handling#": if (value.Length > 0) mc_handlingamount = Decimal.Parse(value); return true;
				case "mc_shipping#": if (value.Length > 0) mc_shippingamount = Decimal.Parse(value); return true;
				case "payment_fee": if (value.Length > 0) payment_fee = Decimal.Parse(value); return true;
				case "payment_gross": if (value.Length > 0) payment_gross = Decimal.Parse(value); return true;
				case "settle_amount": if (value.Length > 0) settle_amount = Decimal.Parse(value); return true;
				case "settle_currency": settle_currency = value; return true;
				case "subscr_date": if (value.Length > 0) subscr_date = ParseDateTime(value); return true;
				case "subscr_effective": if (value.Length > 0) subscr_effective = ParseDateTime(value); return true;
				case "period1": period1 = value; return true;
				case "period2": period2 = value; return true;
				case "period3": period3 = value; return true;
				case "amount1": if (value.Length > 0) amount1 = Decimal.Parse(value); return true;
				case "amount2": if (value.Length > 0) amount2 = Decimal.Parse(value); return true;
				case "amount3": if (value.Length > 0) amount3 = Decimal.Parse(value); return true;
				case "mc_amount1": if (value.Length > 0) mc_amount1 = Decimal.Parse(value); return true;
				case "mc_amount2": if (value.Length > 0) mc_amount2 = Decimal.Parse(value); return true;
				case "mc_amount3": if (value.Length > 0) mc_amount3 = Decimal.Parse(value); return true;
				case "mc_currency": mc_currency = value; return true;
				case "recurring": if (value.Length > 0) recurring = Boolean.Parse(value); return true;
				case "reattempt": if (value.Length > 0) reattempt = Boolean.Parse(value); return true;
				case "retry_at": if (value.Length > 0) retry_at = ParseDateTime(value); return true;
				case "recur_times": if (value.Length > 0) recur_times = Int32.Parse(value); return true;
				case "username": username = value; return true;
				case "password": password = value; return true;
				case "subscr_id": subscr_id = value; return true;
				case "custom": custom = value; return true;
				default:
					return false;
			}
		}

		private DateTime ParseDateTime(string value)
		{
			string fmt = "HH:mm:ss MMM dd, yyyy";
			if (value.Length > 4 && value[value.Length - 4] == ' ')
				fmt += value.Substring(value.Length - 4, 4);
			return DateTime.ParseExact(value, new string[] { fmt, "r", "u", "s", "U", "y" }, new CultureInfo("en-US", true), DateTimeStyles.None);
		}

		#endregion

		public void LoadFromResponseText(string text)
		{
			string[] values = text.Split('\n');
			foreach (string s in values)
			{
				if (!s.Contains("="))
					continue;
				string[] kv = s.Split('=');
				Populate(kv[0], HttpUtility.UrlDecode(kv[1]));
			}
		}
	}
}
