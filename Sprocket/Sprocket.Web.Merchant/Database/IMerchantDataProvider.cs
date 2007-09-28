using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Data;
using Sprocket.Web.Merchant.PayPal;

namespace Sprocket.Web.Merchant.Database
{
	public interface IMerchantDataProvider
	{
		void Initialise(Result result);

		Result Store(PayPalTransactionResponse payPalTransactionResponse);
		Result Delete(PayPalTransactionResponse payPalTransactionResponse);
		PayPalTransactionResponse SelectPayPalTransactionResponse(Guid id);
	}
}
