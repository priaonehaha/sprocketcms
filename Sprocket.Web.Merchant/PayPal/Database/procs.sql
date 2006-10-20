IF EXISTS(SELECT id FROM sysobjects WHERE name='InsertPayPalTransactionResponse' AND type='P')
	DROP PROCEDURE InsertPayPalTransactionResponse
IF EXISTS(SELECT id FROM sysobjects WHERE name='UpdatePayPalTransactionResponse' AND type='P')
	DROP PROCEDURE UpdatePayPalTransactionResponse
IF EXISTS(SELECT id FROM sysobjects WHERE name='SelectPayPalTransactionResponse' AND type='P')
	DROP PROCEDURE SelectPayPalTransactionResponse
IF EXISTS(SELECT id FROM sysobjects WHERE name='DeletePayPalTransactionResponse' AND type='P')
	DROP PROCEDURE DeletePayPalTransactionResponse

go

CREATE PROCEDURE dbo.InsertPayPalTransactionResponse
	@txn_id nvarchar(17),
	@txn_type nvarchar(40) = null,
	@auth_amount money = null,
	@auth_id nvarchar(40) = null,
	@auth_exp datetime = null,
	@auth_status nvarchar(20) = null,
	@mc_gross_x money = null,
	@mc_handling_x money = null,
	@num_cart_items int = null,
	@parent_txn_id nvarchar(17) = null,
	@payment_date datetime = null,
	@payment_status nvarchar(40) = null,
	@payment_type nvarchar(20) = null,
	@pending_reason nvarchar(20) = null,
	@reason_code nvarchar(20) = null,
	@remaining_settle money = null,
	@transaction_entity nvarchar(20) = null,
	@invoice nvarchar(127) = null,
	@memo nvarchar(255) = null,
	@tax money = null,
	@business nvarchar(127) = null,
	@item_name nvarchar(127) = null,
	@item_number nvarchar(127) = null,
	@quantity int = null,
	@receiver_email nvarchar(127) = null,
	@receiver_id nvarchar(13) = null,
	@address_city nvarchar(40) = null,
	@address_country nvarchar(64) = null,
	@address_country_code nvarchar(2) = null,
	@address_name nvarchar(128) = null,
	@address_state nvarchar(40) = null,
	@address_status nvarchar(20) = null,
	@address_street nvarchar(200) = null,
	@address_zip nvarchar(20) = null,
	@first_name nvarchar(64) = null,
	@last_name nvarchar(64) = null,
	@payer_id nvarchar(13) = null,
	@payer_status nvarchar(20) = null,
	@residence_country nvarchar(2) = null,
	@exchange_rate money = null,
	@mc_fee money = null,
	@mc_gross money = null,
	@mc_handling# money = null,
	@mc_shipping# money = null,
	@payment_fee money = null,
	@payment_gross money = null,
	@settle_amount money = null,
	@settle_currency nvarchar(10) = null,
	@subscr_date datetime = null,
	@subscr_effective datetime = null,
	@period1 nvarchar(20) = null,
	@period2 nvarchar(64) = null,
	@period3 nvarchar(20) = null,
	@amount1 money = null,
	@amount2 money = null,
	@amount3 money = null,
	@mc_amount1 money = null,
	@mc_amount2 money = null,
	@mc_amount3 money = null,
	@mc_currency nvarchar(10) = null,
	@recurring bit = null,
	@reattempt bit = null,
	@retry_at datetime = null,
	@recur_times int = null,
	@username nvarchar(64) = null,
	@password nvarchar(127) = null,
	@subscr_id nvarchar(19) = null,
	@custom nvarchar(255) = null
AS
BEGIN
	INSERT INTO PayPalTransactionResponses
		(txn_id, txn_type, auth_amount, auth_id, auth_exp, auth_status, mc_gross_x, mc_handling_x, num_cart_items, parent_txn_id, payment_date, payment_status, payment_type, pending_reason, reason_code, remaining_settle, transaction_entity, invoice, memo, tax, business, item_name, item_number, quantity, receiver_email, receiver_id, address_city, address_country, address_country_code, address_name, address_state, address_status, address_street, address_zip, first_name, last_name, payer_id, payer_status, residence_country, exchange_rate, mc_fee, mc_gross, mc_handling#, mc_shipping#, payment_fee, payment_gross, settle_amount, settle_currency, subscr_date, subscr_effective, period1, period2, period3, amount1, amount2, amount3, mc_amount1, mc_amount2, mc_amount3, mc_currency, recurring, reattempt, retry_at, recur_times, username, password, subscr_id, custom)
	VALUES
		(@txn_id, @txn_type, @auth_amount, @auth_id, @auth_exp, @auth_status, @mc_gross_x, @mc_handling_x, @num_cart_items, @parent_txn_id, @payment_date, @payment_status, @payment_type, @pending_reason, @reason_code, @remaining_settle, @transaction_entity, @invoice, @memo, @tax, @business, @item_name, @item_number, @quantity, @receiver_email, @receiver_id, @address_city, @address_country, @address_country_code, @address_name, @address_state, @address_status, @address_street, @address_zip, @first_name, @last_name, @payer_id, @payer_status, @residence_country, @exchange_rate, @mc_fee, @mc_gross, @mc_handling#, @mc_shipping#, @payment_fee, @payment_gross, @settle_amount, @settle_currency, @subscr_date, @subscr_effective, @period1, @period2, @period3, @amount1, @amount2, @amount3, @mc_amount1, @mc_amount2, @mc_amount3, @mc_currency, @recurring, @reattempt, @retry_at, @recur_times, @username, @password, @subscr_id, @custom)
END

go

CREATE PROCEDURE dbo.UpdatePayPalTransactionResponse
	@txn_id nvarchar(17),
	@txn_type nvarchar(40) = null,
	@auth_amount money = null,
	@auth_id nvarchar(40) = null,
	@auth_exp datetime = null,
	@auth_status nvarchar(20) = null,
	@mc_gross_x money = null,
	@mc_handling_x money = null,
	@num_cart_items int = null,
	@parent_txn_id nvarchar(17) = null,
	@payment_date datetime = null,
	@payment_status nvarchar(40) = null,
	@payment_type nvarchar(20) = null,
	@pending_reason nvarchar(20) = null,
	@reason_code nvarchar(20) = null,
	@remaining_settle money = null,
	@transaction_entity nvarchar(20) = null,
	@invoice nvarchar(127) = null,
	@memo nvarchar(255) = null,
	@tax money = null,
	@business nvarchar(127) = null,
	@item_name nvarchar(127) = null,
	@item_number nvarchar(127) = null,
	@quantity int = null,
	@receiver_email nvarchar(127) = null,
	@receiver_id nvarchar(13) = null,
	@address_city nvarchar(40) = null,
	@address_country nvarchar(64) = null,
	@address_country_code nvarchar(2) = null,
	@address_name nvarchar(128) = null,
	@address_state nvarchar(40) = null,
	@address_status nvarchar(20) = null,
	@address_street nvarchar(200) = null,
	@address_zip nvarchar(20) = null,
	@first_name nvarchar(64) = null,
	@last_name nvarchar(64) = null,
	@payer_id nvarchar(13) = null,
	@payer_status nvarchar(20) = null,
	@residence_country nvarchar(2) = null,
	@exchange_rate money = null,
	@mc_fee money = null,
	@mc_gross money = null,
	@mc_handling# money = null,
	@mc_shipping# money = null,
	@payment_fee money = null,
	@payment_gross money = null,
	@settle_amount money = null,
	@settle_currency nvarchar(10) = null,
	@subscr_date datetime = null,
	@subscr_effective datetime = null,
	@period1 nvarchar(20) = null,
	@period2 nvarchar(64) = null,
	@period3 nvarchar(20) = null,
	@amount1 money = null,
	@amount2 money = null,
	@amount3 money = null,
	@mc_amount1 money = null,
	@mc_amount2 money = null,
	@mc_amount3 money = null,
	@mc_currency nvarchar(10) = null,
	@recurring bit = null,
	@reattempt bit = null,
	@retry_at datetime = null,
	@recur_times int = null,
	@username nvarchar(64) = null,
	@password nvarchar(127) = null,
	@subscr_id nvarchar(19) = null,
	@custom nvarchar(255) = null
AS
BEGIN
	UPDATE PayPalTransactionResponses SET
		txn_type = COALESCE(@txn_type, txn_type),
		auth_amount = COALESCE(@auth_amount, auth_amount),
		auth_id = COALESCE(@auth_id, auth_id),
		auth_exp = COALESCE(@auth_exp, auth_exp),
		auth_status = COALESCE(@auth_status, auth_status),
		mc_gross_x = COALESCE(@mc_gross_x, mc_gross_x),
		mc_handling_x = COALESCE(@mc_handling_x, mc_handling_x),
		num_cart_items = COALESCE(@num_cart_items, num_cart_items),
		parent_txn_id = COALESCE(@parent_txn_id, parent_txn_id),
		payment_date = COALESCE(@payment_date, payment_date),
		payment_status = COALESCE(@payment_status, payment_status),
		payment_type = COALESCE(@payment_type, payment_type),
		pending_reason = COALESCE(@pending_reason, pending_reason),
		reason_code = COALESCE(@reason_code, reason_code),
		remaining_settle = COALESCE(@remaining_settle, remaining_settle),
		transaction_entity = COALESCE(@transaction_entity, transaction_entity),
		invoice = COALESCE(@invoice, invoice),
		memo = COALESCE(@memo, memo),
		tax = COALESCE(@tax, tax),
		business = COALESCE(@business, business),
		item_name = COALESCE(@item_name, item_name),
		item_number = COALESCE(@item_number, item_number),
		quantity = COALESCE(@quantity, quantity),
		receiver_email = COALESCE(@receiver_email, receiver_email),
		receiver_id = COALESCE(@receiver_id, receiver_id),
		address_city = COALESCE(@address_city, address_city),
		address_country = COALESCE(@address_country, address_country),
		address_country_code = COALESCE(@address_country_code, address_country_code),
		address_name = COALESCE(@address_name, address_name),
		address_state = COALESCE(@address_state, address_state),
		address_status = COALESCE(@address_status, address_status),
		address_street = COALESCE(@address_street, address_street),
		address_zip = COALESCE(@address_zip, address_zip),
		first_name = COALESCE(@first_name, first_name),
		last_name = COALESCE(@last_name, last_name),
		payer_id = COALESCE(@payer_id, payer_id),
		payer_status = COALESCE(@payer_status, payer_status),
		residence_country = COALESCE(@residence_country, residence_country),
		exchange_rate = COALESCE(@exchange_rate, exchange_rate),
		mc_fee = COALESCE(@mc_fee, mc_fee),
		mc_gross = COALESCE(@mc_gross, mc_gross),
		mc_handling# = COALESCE(@mc_handling#, mc_handling#),
		mc_shipping# = COALESCE(@mc_shipping#, mc_shipping#),
		payment_fee = COALESCE(@payment_fee, payment_fee),
		payment_gross = COALESCE(@payment_gross, payment_gross),
		settle_amount = COALESCE(@settle_amount, settle_amount),
		settle_currency = COALESCE(@settle_currency, settle_currency),
		subscr_date = COALESCE(@subscr_date, subscr_date),
		subscr_effective = COALESCE(@subscr_effective, subscr_effective),
		period1 = COALESCE(@period1, period1),
		period2 = COALESCE(@period2, period2),
		period3 = COALESCE(@period3, period3),
		amount1 = COALESCE(@amount1, amount1),
		amount2 = COALESCE(@amount2, amount2),
		amount3 = COALESCE(@amount3, amount3),
		mc_amount1 = COALESCE(@mc_amount1, mc_amount1),
		mc_amount2 = COALESCE(@mc_amount2, mc_amount2),
		mc_amount3 = COALESCE(@mc_amount3, mc_amount3),
		mc_currency = COALESCE(@mc_currency, mc_currency),
		recurring = COALESCE(@recurring, recurring),
		reattempt = COALESCE(@reattempt, reattempt),
		retry_at = COALESCE(@retry_at, retry_at),
		recur_times = COALESCE(@recur_times, recur_times),
		username = COALESCE(@username, username),
		password = COALESCE(@password, password),
		subscr_id = COALESCE(@subscr_id, subscr_id),
		custom = COALESCE(@custom, custom)
	WHERE txn_id = @txn_id
END

go

CREATE PROCEDURE dbo.SelectPayPalTransactionResponse
	@txn_id	uniqueidentifier
AS
BEGIN
	SELECT *
	  FROM PayPalTransactionResponses
	 WHERE txn_id = @txn_id
END

go

CREATE PROCEDURE dbo.DeletePayPalTransactionResponse
	@txn_id nvarchar(17)
AS
BEGIN
	DELETE
	  FROM PayPalTransactionResponses
	 WHERE txn_id = @txn_id
END
