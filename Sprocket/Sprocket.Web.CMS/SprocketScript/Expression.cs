using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Sprocket.Web.CMS.SprocketScript.Parser
{
	public interface IExpression
	{
		object Evaluate(ExecutionState state);
		void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack);
	}

	public interface IExpressionCreator
	{
		string Keyword { get; }
		IExpression Create();
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

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
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

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
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

	#region NotExpression
	public class NotExpression : IExpression
	{
		private IExpression expression = null;

		public object Evaluate(ExecutionState state)
		{
			return new NotValue(expression.Evaluate(state));
		}

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			expression = TokenParser.BuildExpression(tokens, ref index, precedenceStack);
		}

		public class NotValue
		{
			object value;
			public NotValue(object value)
			{
				this.value = value;
			}
			public override bool Equals(object obj)
			{
				return !object.Equals(obj, value);
			}
			public override string ToString()
			{
				return "{not " + value + "}";
			}
		}
	}
	public class NotExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "not"; } }
		public IExpression Create() { return new NotExpression(); }
	}
	#endregion

	#region BooleanExpression
	public class BooleanExpression : IExpression
	{
		IExpression expr = null;
		object o = null;
		public object Evaluate(ExecutionState state)
		{
			if(expr != null)
				o = expr.Evaluate(state);
			return new SoftBoolean(!(o.Equals(0m) || o.Equals(null) || o.Equals("") || o.Equals(false)));
		}

		public class SoftBoolean
		{
			bool value;
			public SoftBoolean(bool value)
			{
				this.value = value;
			}

			public override bool Equals(object o)
			{
				if (o == null) return !value;
				bool oValue = !(o.Equals(0m) || o.Equals(null) || o.Equals("") || o.Equals(false));
				return value == oValue;
			}
		}

		public void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			Token token = tokens[index];
			if(token.Value == "true")
				o = true;
			else
				o = false;
			index++;
			//expr = TokenParser.BuildExpression(tokens, ref index, precedenceStack);
		}

		public BooleanExpression() { }
		public BooleanExpression(IExpression expr)
		{
			this.expr = expr;
		}
	}

	public class BooleanExpressionCreator : IExpressionCreator
	{
		public string Keyword { get { return "true"; } }
		public IExpression Create() { return new BooleanExpression(); }
	}

	public class BooleanExpressionCreator2 : IExpressionCreator
	{
		public string Keyword { get { return "false"; } }
		public IExpression Create() { return new BooleanExpression(); }
	}
	#endregion

	#region BinaryExpression (abstract)
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
			protected set { right = value; }
		}

		public IExpression Left
		{
			get { return left; }
			set { left = value; }
		}

		public object Evaluate(ExecutionState state)
		{
			return Evaluate(left, right, state);
		}

		protected abstract object Evaluate(IExpression left, IExpression right, ExecutionState state);
		public abstract int Precedence { get; }
		protected Token token = null;

		public virtual void BuildExpression(List<Token> tokens, ref int index, Stack<int?> precedenceStack)
		{
			token = tokens[index - 1];
			right = TokenParser.BuildExpression(tokens, ref index, precedenceStack);
		}

		public static class PrecedenceValues
		{
			public const int Multiplication = -100;
			public const int Division = -100;
			public const int Addition = -50;
			public const int Subtraction = -50;
			public const int BooleanOperation = -25;
			public const int EqualTo = 0;
			public const int NotEqualTo = 0;
		}

		public override string ToString()
		{
			return "{BinaryExpression: [" + left + " " + token.Value + " " + right + "] }";
		}
	}
	#endregion

	#region AndExpression
	public class AndExpression : BinaryExpression
	{
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			if (!(left is BooleanExpression))
				left = new BooleanExpression(left);
			if (!(right is BooleanExpression))
				right = new BooleanExpression(right);
			return left.Evaluate(state).Equals(true) && right.Evaluate(state).Equals(true);
		}

		public override int Precedence { get { return BinaryExpression.PrecedenceValues.BooleanOperation; } }
	}

	public class AndExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "and"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.BooleanOperation; } }
		public IBinaryExpression Create() { return new AndExpression(); }
	}

	public class AndExpressionCreator2 : IBinaryExpressionCreator
	{
		public string Keyword { get { return "&&"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.BooleanOperation; } }
		public IBinaryExpression Create() { return new AndExpression(); }
	}
	#endregion

	#region EqualToExpression
	public class EqualToExpression : BinaryExpression
	{
		public override int Precedence { get { return BinaryExpression.PrecedenceValues.EqualTo; } }
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			return a.Equals(b) || b.Equals(a); // we check both sides in case one side has overridden the Equals method
		}
	}
	public class EqualToExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "="; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.EqualTo; } }
		public IBinaryExpression Create() { return new EqualToExpression(); }
	}
	public class EqualToExpressionCreator2 : IBinaryExpressionCreator
	{
		public string Keyword { get { return "is"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.EqualTo; } }
		public IBinaryExpression Create() { return new EqualToExpression(); }
	}
	#endregion

	#region NotEqualToExpression
	public class NotEqualToExpression : BinaryExpression
	{
		public override int Precedence { get { return BinaryExpression.PrecedenceValues.NotEqualTo; } }
		protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
		{
			object b = right.Evaluate(state);
			object a = left.Evaluate(state);
			return !(a.Equals(b) || b.Equals(a)); // we check both sides in case one side has overridden the Equals method
		}
	}
	public class NotEqualToExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "!="; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.NotEqualTo; } }
		public IBinaryExpression Create() { return new NotEqualToExpression(); }
	}
	public class NotEqualToExpressionCreator2 : IBinaryExpressionCreator
	{
		public string Keyword { get { return "isnt"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.NotEqualTo; } }
		public IBinaryExpression Create() { return new NotEqualToExpression(); }
	}
	public class NotEqualToExpressionCreator3 : IBinaryExpressionCreator
	{
		public string Keyword { get { return "isn't"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.NotEqualTo; } }
		public IBinaryExpression Create() { return new NotEqualToExpression(); }
	}
	public class NotEqualToExpressionCreator4 : IBinaryExpressionCreator
	{
		public string Keyword { get { return "<>"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.NotEqualTo; } }
		public IBinaryExpression Create() { return new NotEqualToExpression(); }
	}
	#endregion

	//public class GreaterThanExpression : BinaryExpression
	//{
	//    protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
	//    {
	//        object b = right.Evaluate(state);
	//        object a = left.Evaluate(state);
	//        if (a is IComparable && b is IComparable)
	//            return ((IComparable)a).CompareTo(b) > 0;
	//        else
	//            return null;
	//    }
	//}

	//public class GreaterThanOrEqualToExpression : BinaryExpression
	//{
	//    protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
	//    {
	//        object b = right.Evaluate(state);
	//        object a = left.Evaluate(state);
	//        if (a is IComparable && b is IComparable)
	//            return ((IComparable)a).CompareTo(b) >= 0;
	//        else
	//            return null;
	//    }
	//}

	//public class LessThanExpression : BinaryExpression
	//{
	//    protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
	//    {
	//        object b = right.Evaluate(state);
	//        object a = left.Evaluate(state);
	//        if (a is IComparable && b is IComparable)
	//            return ((IComparable)a).CompareTo(b) < 0;
	//        else
	//            return null;
	//    }
	//}

	//public class LessThanOrEqualToExpression : BinaryExpression
	//{
	//    protected override object Evaluate(IExpression left, IExpression right, ExecutionState state)
	//    {
	//        object b = right.Evaluate(state);
	//        object a = left.Evaluate(state);
	//        if (a is IComparable && b is IComparable)
	//            return ((IComparable)a).CompareTo(b) <= 0;
	//        else
	//            return null;
	//    }
	//}

	#region AdditionExpression
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
	#endregion

	#region MultiplicationExpression
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
	#endregion

	#region SubtractionExpression
	public class SubtractionExpression : BinaryExpression
	{
		public override int Precedence { get { return BinaryExpression.PrecedenceValues.Subtraction; } }

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
			return x - y;
		}

		void ThrowException()
		{
			throw new InstructionExecutionException("I can't subtract the first thing from the second because at least one of them isn't a number.", token);
		}
	}

	public class SubtractionExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "-"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.Subtraction; } }

		public IBinaryExpression Create()
		{
			return new SubtractionExpression();
		}
	}
	#endregion

	#region DivisionExpression
	public class DivisionExpression : BinaryExpression
	{
		public override int Precedence { get { return BinaryExpression.PrecedenceValues.Division; } }

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
			if (y == 0)
				throw new InstructionExecutionException("I can't divide the first number by the second because the second is zero. i.e. Noone can divide by zero. Not even me.", token);
			return x / y;
		}

		void ThrowException()
		{
			throw new InstructionExecutionException("I can't divide the first thing by the second because at least one of them isn't a number.", token);
		}
	}

	public class DivisionExpressionCreator : IBinaryExpressionCreator
	{
		public string Keyword { get { return "/"; } }
		public int Precedence { get { return BinaryExpression.PrecedenceValues.Division; } }

		public IBinaryExpression Create()
		{
			return new DivisionExpression();
		}
	}
	#endregion
}
