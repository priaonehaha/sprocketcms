using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Web.Merchant.PayPal;
using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.Merchant.PayPal
{
	public class PaypalExpression : IPropertyEvaluatorExpression
	{
		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "action":
				case "subscribetest":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "action":
					return PayPal.PayPalPostURL;

				case "subscribetest":

					PaypalSubscription pps = new PaypalSubscription();
					pps.CustomValue = Guid.NewGuid().ToString();
					pps.ItemName = "Test Subscription";
					pps.ItemNumber = 400;
					pps.NotifyURL = "http://snowdevil78.dyndns.org/prospector/public/paypal-ipn-process/";
					pps.SubscriptionPeriodSize = 3;
					pps.SubscriptionPeriodUnit = PayPalSubscriptionPeriodUnit.Day;
					pps.SubscriptionPrice = 10;
					pps.TrialPeriodSize = 0;
					pps.EditMode = PayPalSubscriptionEditMode.ModifyOnly;
					return pps.GetFormFields();

				default:
					throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property of this object", token);
			}
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("The \"paypal\" expression cannot be evaluated by itself. Please specify one of its properties.", contextToken);
		}

		public class PayPalExpressionCreator : IExpressionCreator
		{
			public string Keyword { get { return "paypal"; } }
			public IExpression Create() { return new PaypalExpression(); }
		}
	}
}
