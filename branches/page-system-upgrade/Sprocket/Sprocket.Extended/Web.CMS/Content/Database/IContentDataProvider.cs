using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Data;

namespace Sprocket.Web.CMS.Content
{
	public interface IContentDataProvider : IModuleDataProvider
	{
		Result Store(RevisionInformation revisionInformation);
		Result Store(Page page);
		Result DeleteDraftRevisions(long pageID);

		RevisionInformation SelectRevisionInformation(long revisionID);
		Page SelectPage(long pageID);
		Page SelectPageBySprocketPath(string sprocketPath);
		Page SelectPageByPageCode(string pageCode);
		List<Page> ListPages();

		Dictionary<string, List<string>> ListPageCategories(long pageRevisionID);
		Result StorePageCategories(long pageRevisionID, Dictionary<string, List<string>> categories);

		Dictionary<string, List<EditFieldInfo>> ListPageEditFieldsByFieldType(long pageRevisionID);
		Dictionary<string, List<EditFieldInfo>> ListPageEditFieldsByFieldType(List<long> pageRevisionIDs);
		Result StoreEditFieldInfo(long pageRevisionID, EditFieldInfo info);

		Result StoreEditField_TextBox(long dataID, string text);
		void LoadDataList_TextBox(List<EditFieldInfo> fields);

		Result StoreEditField_Image(long dataID, long sprocketFileID);
		void LoadDataList_Image(List<EditFieldInfo> fields);
	}
}
