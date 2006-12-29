using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Sprocket.Web.CMS.SprocketScript.Parser
{
	public interface IExpression
	{
		object Evaluate(ExecutionState state);
		void Build(List<Token> tokens, ref int index, Stack<int?> precedenceStack);
	}

	public interface IFilter // e.g. using, replacing
	{
	}
	public interface IFunction
	{
	}
	public interface IFunctionExpression
	{
	}

	public class GroupedExpression
	{

	}

	public interface ICustomExpression : IExpression
	{
		string Keyword { get; }
		IExpression Argument { get; set; }
	}

	public interface IBinaryExpression : IExpression
	{
		int Precedence { get; }
		IExpression Left { get; set; }
		IExpression Right { get; }
	}
	public interface IBinaryExpressionCreator
	{
		string Keyword { get; }
		int Precedence { get; }
		IBinaryExpression Create();
	}

	public interface IFilteredExpression : IExpression
	{
	}

	#region NumericExpression
	public class NumericExpression : IExpression
	{
		private object value;

		public object Evaluate(ExecutionState state)
		{
			return value;
		}

		public void Build(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			decimal n;
			if (decimal.TryParse(tokens[index++].Value, out n))
				value = n;
			else
				value = null;
		}

		public override string ToString()
		{
			return "{NumericExpression: " + value + "}";
		}
	}


	#endregion

	#region StringExpression
	public class StringExpression : IExpression
	{
		string text;

		public void Build(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			text = tokens[index++].Value;
		}

		public object Evaluate(ExecutionState state)
		{
			return text;
		}

		public override string ToString()
		{
			return "{StringExpression}";
		}
	}
	#endregion

	public enum EvaluationFailure
	{
		LeftNaN,
		RightNaN,
		LeftNotComparable,
		RightNotComparable,
		UnknownIdentifier
	}

	public abstract class BinaryExpression : IBinaryExpression
	{
		private static Stack<IExpression> stack = new Stack<IExpression>();
		public static Stack<IExpression> Stack
		{
			get { return stack; }
		}

		private IExpression left = null;
		private IExpression right = null;

		public IExpression Right
		{
			get { return right; }
		}

		public IExpression Left
		{
			get { return left; }
			set { left = value; }
		}

		public object Evaluate(ExecutionState state)
		{
			return Evaluate(Left, right, state);
		}

		protected abstract object Evaluate(IExpression left, IExpression right, ExecutionState state);
		public abstract int Precedence { get; }
		protected Token token = null;

		public void Build(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			token = tokens[index - 1];
			right = TokenParser.BuildExpression(tokens, ref index, precedenceStack);
		}

		public static class PrecedenceValues
		{
			public const int Multiplication = 0;
			public const int Addition = 100;
			public const int Subtraction = 101;
		}

		public override string ToString()
		{
			return "{BinaryExpression: [" + left + " " + token.Value + " " + right + "] }";
		}
	}

	#region to do
	/*
	public class EqualToExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			return object.Equals(a, b);
		}
	}

	public class NotEqualToExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			return !object.Equals(a, b);
		}
	}

	public class GreaterThanExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			if (a is IComparable && b is IComparable)
				return ((IComparable)a).CompareTo(b) > 0;
			else
				return null;
		}
	}

	public class GreaterThanOrEqualToExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			if (a is IComparable && b is IComparable)
				return ((IComparable)a).CompareTo(b) >= 0;
			else
				return null;
		}
	}

	public class LessThanExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			if (a is IComparable && b is IComparable)
				return ((IComparable)a).CompareTo(b) < 0;
			else
				return null;
		}
	}

	public class LessThanOrEqualToExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			if (a is IComparable && b is IComparable)
				return ((IComparable)a).CompareTo(b) <= 0;
			else
				return null;
		}
	}
	*/
	#endregion

	public class AdditionExpression : BinaryExpression
	{
		public override int Precedence { get { return BinaryExpression.PrecedenceValues.Addition; } }

		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			decimal x, y;
			if (a is decimal)
				x = (decimal)a;
			else
			{
				TypeConverter tc = TypeDescriptor.GetConverter(a);
				if (!tc.CanConvertTo(typeof(decimal)))
					return string.Concat(a, b);
				x = (decimal)tc.ConvertTo(a, typeof(decimal));
			}
			if (b is decimal)
				y = (decimal)b;
			else
			{
				TypeConverter tc = TypeDescriptor.GetConverter(b);
				if (!tc.CanConvertTo(typeof(decimal)))
					return string.Concat(a, b);
				y = (decimal)tc.ConvertTo(b, typeof(decimal));
			}
			return x + y;
		}
	}

	public class AdditionExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "+"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.Addition; } }

		public IBinaryExpression Create()
		{
			return new AdditionExpression();
		}
	}

	public class MultiplicationExpression : BinaryExpression
	{
		public override int Precedence { get { return BinaryExpression.PrecedenceValues.Multiplication; } }

		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			decimal x, y;
			if (a is decimal)
				x = (decimal)a;
			else
			{
				TypeConverter tc = TypeDescriptor.GetConverter(a);
				if (!tc.CanConvertTo(typeof(decimal)))
					ThrowException();
				x = (decimal)tc.ConvertTo(a, typeof(decimal));
			}
			if (b is decimal)
				y = (decimal)b;
			else
			{
				TypeConverter tc = TypeDescriptor.GetConverter(b);
				if (!tc.CanConvertTo(typeof(decimal)))
					ThrowException();
				y = (decimal)tc.ConvertTo(b, typeof(decimal));
			}
			return x * y;
		}

		void ThrowException()
		{
			throw new InstructionExecutionException("Uh yeah... I can't multiply those things together now, can I?", token);
		}
	}

	public class MultiplicationExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "*"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.Multiplication; } }

		public IBinaryExpression Create()
		{
			return new MultiplicationExpression();
		}
	}

	//public class SubtractionExpression : BinaryExpression
	//{
	//    public SubtractionExpression(IExpression left, IExpression right) : base(left, right) { }

	//    protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
	//    {
	//        object b = right.Evaluate(state);
	//        object a = left.Evaluate(state);
	//        DecimalConverter dc = new DecimalConverter();
	//        if (dc.CanConvertFrom(a.GetType()) && dc.CanConvertFrom(b.GetType()))
	//            return Convert.ToDecimal(a) - Convert.ToDecimal(b);
	//        else
	//            return null;
	//    }
	//}
}
