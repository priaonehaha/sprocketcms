using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;

using Sprocket.Utility;

namespace Sprocket.Web.CMS.Content.Expressions
{
	class PagesInCategoryExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			string catset = (args[0].Expression.Evaluate(state, args[0].Token) ?? "").ToString();
			string catname = (args[1].Expression.Evaluate(state, args[1].Token) ?? "").ToString();
			int pageSize = Convert.ToInt32(TokenParser.VerifyUnderlyingType(args[2].Expression.Evaluate(state, args[2].Token)) ?? 0);
			int pageNumber = Convert.ToInt32(TokenParser.VerifyUnderlyingType(args[3].Expression.Evaluate(state, args[3].Token)) ?? 0);
			string sort = (args[4].Expression.Evaluate(state, args[4].Token) ?? "").ToString();
			PageResultSetOrder pageOrder;
			switch (sort)
			{
				case "random": pageOrder = PageResultSetOrder.Random; break;
				case "publishdate ascending": pageOrder = PageResultSetOrder.PublishDateAscending; break;
				default: pageOrder = PageResultSetOrder.PublishDateDescending; break;
			}
			PageSearchOptions options = new PageSearchOptions();
			options.Draft = false;
			options.Deleted = false;
			options.Hidden = false;
			options.PageSize = pageSize;
			options.PageNumber = pageNumber;
			options.PageOrder = pageOrder;
			options.SetCategory(catset, catname);
			PageResultSet pages = ContentManager.Instance.DataProvider.ListPages(options);
			pages.LoadContentForPages();
			return pages;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("\"pages_in_category\" requires arguments specifying the category set name and the category name in that set.", contextToken);
		}
	}

	class PagesInCategoryExpressionCreator : IExpressionCreator
	{
		public string Keyword
		{
			get { return "pages_in_category"; }
		}

		public IExpression Create()
		{
			return new PagesInCategoryExpression();
		}
	}
}
