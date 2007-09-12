using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sprocket.Utility;

using Sprocket.Web.CMS.Script;

namespace Sprocket.Web.CMS.Content
{
	public class PageAdminSectionDefinition : IPropertyEvaluatorExpression
	{
		private string name = String.Empty, label = String.Empty, hint = String.Empty, inSection = null;
		private IContentNodeType defaultContentNode = null;
		private bool allowDeleteDefaultNode = false;
		private int? maxAdditionalContentNodes = null;
		private List<IContentNodeType> allowableNodeTypes = new List<IContentNodeType>();

		/// <summary>
		/// If this list is empty, any kind of node can be added to the page.
		/// If MaxAdditionalContentNodes > 0 or is null and this list has one or more items, only items of those types can be added.
		/// If MaxAdditionalContentNodes = 0 OR (AllowableContentNodes is omitted and there is a default node type specified),
		///		new content nodes can't be added to this field on the page.
		/// </summary>
		public List<IContentNodeType> AllowableNodeTypes
		{
			get { return allowableNodeTypes; }
		}

		public int? MaxAdditionalContentNodes
		{
			get { return maxAdditionalContentNodes; }
			internal set { maxAdditionalContentNodes = value; }
		}

		public bool AllowDeleteDefaultNode
		{
			get { return allowDeleteDefaultNode; }
			internal set { allowDeleteDefaultNode = value; }
		}

		public IContentNodeType DefaultContentNode
		{
			get { return defaultContentNode; }
			internal set { defaultContentNode = value; }
		}

		public string FieldName
		{
			get { return name; }
			internal set { name = value; }
		}

		public string Label
		{
			get { return label; }
			internal set { label = value; }
		}

		public string Hint
		{
			get { return hint; }
			internal set { hint = value; }
		}

		public string InSection
		{
			get { return inSection; }
			internal set { inSection = value; }
		}

		public bool IsValidPropertyName(string propertyName)
		{
			switch (propertyName)
			{
				case "label":
				case "hint":
					return true;
			}
			return false;
		}

		public object EvaluateProperty(string propertyName, Token token, ExecutionState state)
		{
			switch (propertyName)
			{
				case "label":
					return Label;
				case "hint":
					return Hint;
			}
			throw new InstructionExecutionException("\"" + propertyName + "\" is not a valid property for this object", token);
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return "PageAdminSection: " + label;
		}

		private string RenderUI()
		{
			return "ui";
		}
	}
}
