using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Xml;

using Sprocket;
using Sprocket.Web;
using Sprocket.Web.CMS.Script;
using Sprocket.Data;

namespace Sprocket.Web.FileManager
{
	public class SprocketFile : IPropertyEvaluatorExpression
	{
		#region Fields

		protected long sprocketFileID = 0;
		protected long clientSpaceID = 0;
		protected byte[] fileData = null;
		protected string fileTypeExtension = "";
		protected string originalFileName = "";
		protected string contentType = "";
		protected string title = "";
		protected string description = "";
		private long dataLength = 0;
		protected DateTime uploadDate = DateTime.MinValue;

		#endregion

		#region Properties

		public long DataLength
		{
			get { return dataLength; }
		}

		///<summary>
		///Gets or sets the value for SprocketFileID
		///</summary>
		public long SprocketFileID
		{
			get { return sprocketFileID; }
			set { sprocketFileID = value; }
		}

		///<summary>
		///Gets or sets the value for ClientSpaceID
		///</summary>
		public long ClientSpaceID
		{
			get { return clientSpaceID; }
			set { clientSpaceID = value; }
		}

		///<summary>
		///Gets or sets the value for FileData
		///</summary>
		public byte[] FileData
		{
			get { return fileData; }
			set
			{
				fileData = value;
				dataLength = fileData.Length;
			}
		}

		///<summary>
		///Gets or sets the value for FileTypeExtension
		///</summary>
		public string FileTypeExtension
		{
			get { return fileTypeExtension; }
			set { fileTypeExtension = value; }
		}

		///<summary>
		///Gets or sets the value for OriginalFileName
		///</summary>
		public string OriginalFileName
		{
			get { return originalFileName; }
			set { originalFileName = value; }
		}

		///<summary>
		///Gets or sets the value for ContentType
		///</summary>
		public string ContentType
		{
			get { return contentType; }
			set { contentType = value; }
		}

		///<summary>
		///Gets or sets the value for Title
		///</summary>
		public string Title
		{
			get { return title; }
			set { title = value; }
		}

		///<summary>
		///Gets or sets the value for Description
		///</summary>
		public string Description
		{
			get { return description; }
			set { description = value; }
		}

		///<summary>
		///Gets or sets the value for UploadDate
		///</summary>
		public DateTime UploadDate
		{
			get { return uploadDate; }
			set { uploadDate = value; }
		}

		#endregion

		#region Constructors

		public SprocketFile()
		{
		}

		public SprocketFile(long sprocketFileID, long clientSpaceID, byte[] fileData, string fileTypeExtension, string originalFileName, string contentType, string title, string description, DateTime uploadDate)
		{
			this.sprocketFileID = sprocketFileID;
			this.clientSpaceID = clientSpaceID;
			this.fileData = fileData;
			this.fileTypeExtension = fileTypeExtension;
			this.originalFileName = originalFileName;
			this.contentType = contentType;
			this.title = title;
			this.description = description;
			this.uploadDate = uploadDate;
			dataLength = fileData.Length;
		}

		public SprocketFile(long clientSpaceID, HttpPostedFile fileData, string title, string description)
		{
			this.clientSpaceID = clientSpaceID;
			this.fileData = new byte[fileData.ContentLength];
			fileData.InputStream.Read(this.fileData, 0, fileData.ContentLength);
			this.originalFileName = fileData.FileName;
			string[] arr = fileData.FileName.Split('.');
			if (arr.Length > 1)
				fileTypeExtension = arr[1];
			else
				fileTypeExtension = "";
			this.contentType = fileData.ContentType;
			this.title = title;
			this.description = description;
			this.uploadDate = SprocketDate.Now;
			dataLength = fileData.ContentLength;
		}

		public SprocketFile(IDataReader reader)
		{
			if (reader["SprocketFileID"] != DBNull.Value) sprocketFileID = (long)reader["SprocketFileID"];
			if (reader["ClientSpaceID"] != DBNull.Value) clientSpaceID = (long)reader["ClientSpaceID"];
			if (reader["FileData"] != DBNull.Value) fileData = (byte[])reader["FileData"];
			if (reader["FileTypeExtension"] != DBNull.Value) fileTypeExtension = (string)reader["FileTypeExtension"];
			if (reader["OriginalFileName"] != DBNull.Value) originalFileName = (string)reader["OriginalFileName"];
			if (reader["ContentType"] != DBNull.Value) contentType = (string)reader["ContentType"];
			if (reader["Title"] != DBNull.Value) title = (string)reader["Title"];
			if (reader["Description"] != DBNull.Value) description = (string)reader["Description"];
			if (reader["UploadDate"] != DBNull.Value) uploadDate = (DateTime)reader["UploadDate"];
			if (reader["DataLength"] != DBNull.Value) dataLength = (long)reader["DataLength"];
		}

		#endregion

		#region JSON Methods

		/// <summary>
		/// Writes this entity out as a JSON formatted string
		/// </summary>
		public void WriteJSON(StringWriter writer)
		{
			writer.Write("{");
			JSON.EncodeNameValuePair(writer, "SprocketFileID", sprocketFileID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "ClientSpaceID", clientSpaceID);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "FileTypeExtension", fileTypeExtension);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "OriginalFileName", originalFileName);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "ContentType", contentType);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Title", title);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "Description", description);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "UploadDate", uploadDate);
			writer.Write(",");
			JSON.EncodeNameValuePair(writer, "DataLength", dataLength);
			writer.Write("}");
		}

		#endregion

		#region IPropertyEvaluatorExpression Members

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "sprocketfileid":
				case "clientspaceid":
				case "filedata":
				case "filetypeextension":
				case "originalfilename":
				case "contenttype":
				case "title":
				case "description":
				case "uploaddate":
					return true;
				default:
					return false;
			}
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "sprocketfileid": return SprocketFileID;
				case "clientspaceid": return ClientSpaceID;
				case "filedata": return FileData;
				case "filetypeextension": return FileTypeExtension;
				case "originalfilename": return OriginalFileName;
				case "contenttype": return ContentType;
				case "title": return Title;
				case "description": return Description;
				case "uploaddate": return UploadDate;
				default: return null;
			}
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return "[SprocketFile:" + SprocketFileID + "]";
		}

		#endregion

		public Result ValidateImageData()
		{
			Image img;
			return ValidateImageData(out img);
		}

		public Result ValidateImageData(out Image img)
		{
			try
			{
				img = Image.FromStream(new MemoryStream(fileData));
				if (img.Width == 0 && img.Height == 0)
					return new Result("Corrupt image file. The image has no dimensions.");
			}
			catch (Exception ex)
			{
				img = null;
				return new Result(ex.Message);
			}
			return new Result();
		}
	}
}
