using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Web;
using System.Transactions;

using Sprocket.Data;
using Sprocket.Security;
using Sprocket.Web.FileManager;

namespace Sprocket.Web.CMS.Content
{
	public class ImageEditField : IEditFieldHandler
	{
		public class ImageEditFieldCreator : IEditFieldObjectCreator
		{
			public string Identifier { get { return "Image"; } }
			public IEditFieldHandler CreateHandler() { return new ImageEditField(); }
			public IEditFieldHandlerDatabaseInterface CreateDatabaseInterface() { return new ImageEditFieldDatabaseInterface(); }
			public IEditFieldData CreateDataObject() { return new ImageEditFieldData(); }
		}
		public class ImageEditFieldDatabaseInterface : IEditFieldHandlerDatabaseInterface
		{
			public void LoadDataList(List<EditFieldInfo> fields)
			{
				ContentManager.Instance.DataProvider.LoadDataList_Image(fields);
			}

			public Result SaveData(long dataID, IEditFieldData efdata)
			{
				ImageEditFieldData data = (ImageEditFieldData)efdata;
				try
				{
					using (TransactionScope scope = new TransactionScope())
					{
						DatabaseManager.DatabaseEngine.GetConnection();
						if (data.SprocketFile != null)
							data.SprocketFileID = data.SprocketFile.SprocketFileID = DatabaseManager.DatabaseEngine.GetUniqueID();
						Result r = ContentManager.Instance.DataProvider.StoreEditField_Image(dataID, data.SprocketFileID);
						if (r.Succeeded && data.SprocketFile != null)
							r = FileManager.FileManager.DataLayer.Store(data.SprocketFile);
						if (r.Succeeded)
							scope.Complete();
						return r;
					}
				}
				catch (Exception e)
				{
					return new Result(e.Message);
				}
				finally
				{
					DatabaseManager.DatabaseEngine.ReleaseConnection();
				}
			}
		}
		public class ImageEditFieldData : IEditFieldData
		{
			private long sprocketFileID = 0;
			public long SprocketFileID
			{
				get { return sprocketFileID; }
				set
				{
					sprocketFileID = value;
					//sprocketFile = null;
				}
			}

			private SprocketFile sprocketFile = null;
			public SprocketFile SprocketFile
			{
				get { return sprocketFile; }
				set
				{
					sprocketFile = value;
					sprocketFileID = sprocketFile == null ? 0 : sprocketFile.SprocketFileID;
				}
			}

			public override string ToString()
			{
				return "[SprocketFileID: " + sprocketFileID + "]";
			}
		}

		private string fieldName = "";
		private int maxwidth = 0, maxheight = 0;

		public void Initialise(XmlElement xml)
		{
			if (xml.HasAttribute("FieldName"))
				fieldName = xml.GetAttribute("FieldName");
			if (xml.HasAttribute("MaxWidth"))
			{
				int max;
				if (!int.TryParse(xml.GetAttribute("MaxWidth"), out max))
					max = 0;
				maxwidth = max;
			}
			if (xml.HasAttribute("MaxHeight"))
			{
				int max;
				if (!int.TryParse(xml.GetAttribute("MaxHeight"), out max))
					max = 0;
				maxheight = max;
			}
		}

		public string TypeName
		{
			get { return "Image"; }
		}

		public string FieldName
		{
			get { return fieldName; }
		}

		public void InitialiseData(IEditFieldData data)
		{
			((ImageEditFieldData)data).SprocketFileID = 0;
		}

		public object GetOutputValue(IEditFieldData data)
		{
			return ((ImageEditFieldData)data).SprocketFileID;
		}

		public string RenderAdminField(IEditFieldData data)
		{
			long id = ((ImageEditFieldData)data).SprocketFileID;
			StringBuilder sb = new StringBuilder();
			if (id > 0)
			{
				sb.AppendFormat(
					"<div class=\"imageeditfield-thumb\"><img src=\"{0}admin/pages/imgthumb/{1}/\" /></div>" +
					"<input type=\"hidden\" name=\"existingimage_{2}\" value=\"{1}\" />",
					WebUtility.BasePath, id, fieldName
				);
			}
			sb.AppendFormat("<div class=\"imageeditfield-browse\"><input type=\"file\" size=\"60\" name=\"{0}\" id=\"imageeditfield-file-{0}\" class=\"file\" /></div>", fieldName);
			if (id > 0)
			{
				sb.AppendFormat(
					"<div class=\"imageeditfield-delete\"><input type=\"checkbox\" name=\"deleteimage_{0}\" value=\"yes\" " +
					"onclick=\"$('imageeditfield-file-{0}').disabled = this.checked;\" " +
					"id=\"imageeditfield-delete-{0}\" class=\"checkbox\" /><label for=\"imageeditfield-delete-{0}\">" +
					"Delete this image (no image will be selected for this field)</label></div><div style=\"clear:both;overflow:hidden;\"></div>",
					fieldName
				);
			}
			return sb.ToString();
		}

		public Result ReadAdminField(out IEditFieldData efdata)
		{
			ImageEditFieldData data = new ImageEditFieldData();

			SprocketFile file = null;
			if (HttpContext.Current.Request.Files[fieldName] != null)
				if (HttpContext.Current.Request.Files[fieldName].ContentLength > 0)
				{
					file = new SprocketFile(SecurityProvider.ClientSpaceID, HttpContext.Current.Request.Files[fieldName], "", "");
					Image img;
					Result r = file.ValidateImageData(out img);
					if (!r.Succeeded)
					{
						efdata = new ImageEditFieldData();
						return new Result("The image selected could not be loaded. The system reported back the following error: " + r.Message);
					}
					if (maxwidth > 0 || maxheight > 0)
					{
						SizingOptions options = new SizingOptions(maxwidth, maxheight, SizingOptions.Display.Constrain, 0);
						options.Image = img;
						MemoryStream stream = new MemoryStream();
						FileManager.FileManager.Instance.ResizeImage(options, stream);
						file.FileData = stream.ToArray();
					}
				}

			bool deleted = HttpContext.Current.Request.Form["deleteimage_" + fieldName] != null;
			long existing;
			if (!long.TryParse(HttpContext.Current.Request.Form["existingimage_" + fieldName], out existing))
				existing = 0;

			if (deleted)
				data.SprocketFileID = 0;
			else if (file != null)
				data.SprocketFile = file;
			else
				data.SprocketFileID = existing;
			efdata = data;

			return new Result();
		}

		public bool IsContentDifferent(IEditFieldData a, IEditFieldData b)
		{
			ImageEditFieldData d1 = (ImageEditFieldData)a;
			ImageEditFieldData d2 = (ImageEditFieldData)b;
			return d1.SprocketFileID != d2.SprocketFileID
				|| (d1.SprocketFileID == 0 && d2.SprocketFile != null)
				|| (d2.SprocketFileID == 0 && d1.SprocketFile != null);
		}
	}
}
