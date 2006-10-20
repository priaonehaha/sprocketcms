using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Utility;

namespace Sprocket.Web.CMS.Pages.Widgets
{
	public class NewsletterHandler : IPlaceHolderRenderer
	{
		public string Render(PlaceHolder placeHolder, PageEntry pageEntry, System.Xml.XmlDocument content, Stack<string> placeHolderStack, out bool containsCacheableContent)
		{
			string output;
			switch (placeHolder.Expression.ToLower())
			{
				case "subscribe":
					output = ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Pages.Widgets.Newsletter.newsletter-subscribe.htm")
						.Replace("[blurb]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/SubscribeBlurb").FirstChild.Value)
						.Replace("[label]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/EmailFieldLabel").FirstChild.Value)
						.Replace("[button]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/SubscribeButtonText").FirstChild.Value)
						.Replace("[pleasewait]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/SubscribingBlurb").FirstChild.Value)
						.Replace("[subscribed]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/SubscribedBlurb").FirstChild.Value)
						;
					break;

				case "unsubscribe":
					output = ResourceLoader.LoadTextResource("Sprocket.Web.CMS.Pages.Widgets.Newsletter.newsletter-unsubscribe.htm")
						.Replace("[blurb]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/UnsubscribeBlurb").FirstChild.Value)
						.Replace("[label]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/EmailFieldLabel").FirstChild.Value)
						.Replace("[button]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/UnsubscribeButtonText").FirstChild.Value)
						.Replace("[pleasewait]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/UnsubscribingBlurb").FirstChild.Value)
						.Replace("[unsubscribed]", GeneralRegistry.XmlDoc.SelectSingleNode("/General/Newsletter/UnsubscribedBlurb").FirstChild.Value)
						;
					break;

				default:
					containsCacheableContent = false;
					return "[Newsletter renderer expects expression of \"Subscribe\" or \"Unsubscribe\"]";
			}

			containsCacheableContent = true;
			return output;
		}
	}
}
