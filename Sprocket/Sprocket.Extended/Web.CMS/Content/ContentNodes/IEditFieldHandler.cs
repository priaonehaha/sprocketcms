using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace Sprocket.Web.CMS.Content
{
	/// <summary>
	/// Defines a handler for editing a single editable field for a single version
	/// of a specific page. The handler knows which field it is in charge of and what
	/// type of field it is. It is nothing more than a handler and does not store
	/// or hold data for that field.
	/// </summary>
	public interface IEditFieldHandler
	{
		void Initialise(XmlElement xml);
		string TypeName { get; }
		string FieldName { get; }
		void InitialiseData(IEditFieldData data);
		object GetOutputValue(IEditFieldData data);
		string RenderAdminField(IEditFieldData data);
		Result ReadAdminField(out IEditFieldData data);
		bool IsContentDifferent(IEditFieldData a, IEditFieldData b);
	}

	public interface IEditFieldData
	{
		
	}

	public interface IEditFieldHandlerDatabaseInterface
	{
		void LoadDataList(List<EditFieldInfo> fields);
		Result SaveData(long dataID, IEditFieldData data);
	}

	public interface IEditFieldObjectCreator
	{
		string Identifier { get; }
		IEditFieldHandler CreateHandler();
		IEditFieldData CreateDataObject();
		IEditFieldHandlerDatabaseInterface CreateDatabaseInterface();
	}
}
