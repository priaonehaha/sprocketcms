using System;
using System.Collections.Generic;
using System.Text;
using Sprocket.Web.CMS.Script;


namespace Sprocket.Web.CMS.Content.Expressions
{
	class TemplateExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("The \"template\" expression takes a single argument specifying which template to load", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			if(o == null)
				throw new InstructionExecutionException("You can't request a template using a null value as the template name.", args[0].Token);

			Template t = ContentManager.Templates[o.ToString()];
			if (t == null)
				throw new InstructionExecutionException("There is no template registered with the name \"" + o + "\".", args[0].Token);

			return t;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("You can't use the \"template\" expression by itself as the default behaviour is to render the content of the template which would result in infinite recursion.", contextToken.Next);
		}
	}
	class TemplateExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "template"; } }
		public IExpression Create() { return new TemplateExpression(); }
	}

	class AdminTemplateExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("The \"admin_template\" expression takes a single argument specifying which template to load", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			if (o == null)
				throw new InstructionExecutionException("You can't request a template using a null value as the template name.", args[0].Token);

			Template t = Admin.AdminHandler.Instance.Templates[o.ToString()];
			if (t == null)
				throw new InstructionExecutionException("There is no template registered with the name \"" + o + "\".", args[0].Token);

			return t;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("You can't use the \"admin_template\" expression by itself as the default behaviour is to render the content of the template which would result in infinite recursion.", contextToken.Next);
		}
	}
	class AdminTemplateExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "admin_template"; } }
		public IExpression Create() { return new AdminTemplateExpression(); }
	}

	class ContentTemplateExistsExpression : IArgumentListEvaluatorExpression
	{
		public object Evaluate(Token contextToken, List<ExpressionArgument> args, ExecutionState state)
		{
			if (args.Count != 1)
				throw new InstructionExecutionException("The \"content_template_exists\" expression takes a single argument specifying which template to look for", contextToken);
			object o = args[0].Expression.Evaluate(state, args[0].Token);
			if (o == null)
				throw new InstructionExecutionException("You can't request a template using a null value as the template name.", args[0].Token);

			Template t = ContentManager.Templates[o.ToString()];
			return t != null;
		}

		public object Evaluate(ExecutionState state, Token contextToken)
		{
			throw new InstructionExecutionException("You can't use the \"content_template_exists\" expression without an argument specifying which template you want to check for the existance of.", contextToken.Next);
		}
	}
	class ContentTemplateExistsExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "content_template_exists"; } }
		public IExpression Create() { return new ContentTemplateExistsExpression(); }
	}

	class AllTemplatesExpression : IExpression
	{
		public object Evaluate(ExecutionState state, Token contextToken)
		{
			return ContentManager.Templates.GetList();
		}
	}
	class AllTemplatesExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "all_templates"; } }
		public IExpression Create() { return new AllTemplatesExpression(); }
	}
}
