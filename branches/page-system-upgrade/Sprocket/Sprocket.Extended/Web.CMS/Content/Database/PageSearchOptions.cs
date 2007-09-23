using System;
using System.Collections.Generic;
using System.Text;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class PageSearchOptions
	{
		private bool? hidden = null, draft = null, deleted = false;
		private string categorySetName = null, categoryName = null;
		long pageNumber = 1, pageSize = 0;
		PageResultSetOrder pageOrder = PageResultSetOrder.PublishDateDescending;

		public PageResultSetOrder PageOrder
		{
			get { return pageOrder; }
			set { pageOrder = value; }
		}

		public long PageNumber
		{
			get { return pageNumber; }
			set { pageNumber = value; }
		}

		public long PageSize
		{
			get { return pageSize; }
			set { pageSize = value; }
		}

		public string CategorySetName
		{
			get { return categorySetName; }
		}

		public string CategoryName
		{
			get { return categoryName; }
		}

		public bool? Deleted
		{
			get { return deleted; }
			set { deleted = value; }
		}

		public bool? Draft
		{
			get { return draft; }
			set { draft = value; }
		}

		public bool? Hidden
		{
			get { return hidden; }
			set { hidden = value; }
		}

		public void SetCategory(string categorySetName, string categoryName)
		{
			if (categoryName == null || categorySetName == null)
				throw new NullReferenceException("categorySetName and categoryName must be non-null values");
			this.categoryName = categoryName;
			this.categorySetName = categorySetName;
		}

		public void ClearCategory()
		{
			categorySetName = null;
			categoryName = null;
		}
	}

	public class PageResultSet : IPropertyEvaluatorExpression, IListExpression
	{
		private List<Page> pages;
		private long totalResults, pageCount, pageNumber, pageSize, firstResultIndex = -1, lastResultIndex = -1;
		PageResultSetOrder pageOrder;

		public PageResultSet(List<Page> list, long total, long pageSize, long pageNumber, PageResultSetOrder pageOrder)
		{
			pages = list;
			totalResults = total;
			this.pageSize = pageSize;
			this.pageNumber = pageNumber;
			pageCount = pageSize == 0 ? 1 : total / pageSize + (total % pageSize == 0 ? 0 : 1);
			this.pageOrder = pageOrder;
			if (total > 0)
			{
				firstResultIndex = Math.Max((pageNumber - 1) * pageSize, 0);
				lastResultIndex = firstResultIndex + Math.Max(list.Count - 1, 0);
			}
		}

		public PageResultSetOrder PageOrder
		{
			get { return pageOrder; }
		}

		public long LastResultIndex
		{
			get { return lastResultIndex; }
		}

		public long FirstResultIndex
		{
			get { return firstResultIndex; }
		}

		public long PageSize
		{
			get { return pageSize; }
		}

		public long PageNumber
		{
			get { return pageNumber; }
		}

		public long PageCount
		{
			get { return pageCount; }
		}

		public List<Page> Pages
		{
			get { return pages; }
		}

		public long TotalResults
		{
			get { return totalResults; }
		}

		public void LoadContentForPages()
		{
			Dictionary<long, Dictionary<string, List<EditFieldInfo>>> editFieldsBySectionNameByRevisionID = new Dictionary<long, Dictionary<string, List<EditFieldInfo>>>();
			Dictionary<long, Page> pagesByRevisionID = new Dictionary<long, Page>();
			List<long> ids = new List<long>();
			foreach (Page page in pages)
			{
				editFieldsBySectionNameByRevisionID[page.RevisionID] = new Dictionary<string, List<EditFieldInfo>>();
				pagesByRevisionID[page.RevisionID] = page;
				ids.Add(page.RevisionID);
			}
			Dictionary<string, List<EditFieldInfo>> editFieldsByFieldType = ContentManager.Instance.DataProvider.ListPageEditFieldsByFieldType(ids);

			// build a map of field sections to the fields they contain
			foreach (KeyValuePair<string, List<EditFieldInfo>> fieldListWithCommonFieldType in editFieldsByFieldType)
			{
				// group all of the nodes (currently grouped according to node type) into field name groups
				foreach (EditFieldInfo info in fieldListWithCommonFieldType.Value)
				{
					Dictionary<string, List<EditFieldInfo>> editFieldsBySectionName = editFieldsBySectionNameByRevisionID[info.PageRevisionID];
					List<EditFieldInfo> editfields;
					if (!editFieldsBySectionName.TryGetValue(info.SectionName, out editfields))
					{
						editfields = new List<EditFieldInfo>();
						editFieldsBySectionName.Add(info.SectionName, editfields);
					}
					editfields.Add(info);
				}
				// seeing as each iteration in this loop identifies the full set of nodes for a single content node type,
				// get the database interface for that node type and load the data for all the nodes of that type. Note
				// that fieldListWithCommonFieldType, due to the program logic, will always have at least one element, so
				// as such, the first element can be used to retrieve the field type.
				IEditFieldHandlerDatabaseInterface dbi = fieldListWithCommonFieldType.Value[0].DataHandler;
				if (dbi == null) continue;
				dbi.LoadDataList(fieldListWithCommonFieldType.Value);
			}

			// now go and assign all the loaded edit field data to the pages in the list
			foreach (KeyValuePair<long, Dictionary<string, List<EditFieldInfo>>> kvp in editFieldsBySectionNameByRevisionID)
				pagesByRevisionID[kvp.Key].BuildAdminSectionList(kvp.Value);
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "pages":
				case "totalresults":
				case "pagecount":
				case "pagenumber":
				case "pagesize":
				case "firstresultindex":
				case "lastresultindex":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "pages": return pages;
				case "totalresults": return totalResults;
				case "pagecount": return pageCount;
				case "pagenumber": return pageNumber;
				case "pagesize": return pageSize;
				case "firstresultindex": return firstResultIndex;
				case "lastresultindex": return lastResultIndex;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property of PageResultSet.", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return pages;
		}

		public System.Collections.IList GetList(ExecutionState state)
		{
			return pages;
		}
	}

	public enum PageResultSetOrder
	{
		PublishDateDescending = 1,
		PublishDateAscending = 2,
		Random = 999
	}
}
