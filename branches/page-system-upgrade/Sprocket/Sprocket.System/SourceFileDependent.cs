using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sprocket
{
	public abstract class SourceFileDependent<T>
	{
		private FileInfo file;
		private DateTime modifiedDate = DateTime.MinValue;
		private T dataObject;

		public SourceFileDependent(FileInfo sourceFile)
		{
			if (sourceFile == null)
				throw new NullReferenceException("A reference to a source file was expected");
			file = sourceFile;
		}

		private void Load()
		{
			if (!file.Exists)
				throw new Exception("Can't load dependent file. It doesn't exist: " + file.FullName);
			modifiedDate = file.LastWriteTime;
			Initialise(file, out dataObject);
			if (OnFileChanged != null)
				OnFileChanged();
		}

		protected abstract void Initialise(FileInfo file, out T data);

		public T Data
		{
			get
			{
				if(HasFileChanged)
					Load();
				return dataObject;
			}
		}

		public bool HasFileChanged
		{
			get
			{
				file.Refresh();
				return file.LastWriteTime != modifiedDate;
			}
		}

		public event EmptyHandler OnFileChanged;
	}

	public class XmlSourceFileDependent : SourceFileDependent<XmlDocument>
	{
		public XmlSourceFileDependent(string xmlPath) : base(new FileInfo(xmlPath))
		{
		}

		protected override void Initialise(FileInfo file, out XmlDocument data)
		{
			data = new XmlDocument();
			data.Load(file.FullName);
		}
	}
}
