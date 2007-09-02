using System;
using System.Collections.Generic;
using System.Text;

namespace Sprocket.Web
{
	public interface IRankable
	{
		ObjectRank Rank { get; set; }
	}

	public static class RankedObject
	{
		public static int SortByRank(IRankable a, IRankable b)
		{
			if (a == null)
				return b == null ? 0 : -1;
			if (b == null)
				return 1;

			if (a.Rank > b.Rank)
				return 1;
			else if (a.Rank < b.Rank)
				return -1;
			if (a is IComparable && b is IComparable && a.GetType() == b.GetType())
				return ((IComparable)a).CompareTo((IComparable)b);
			return 0;
		}
	}

	public enum ObjectRank
	{
		First = 1,
		Earlier = 2,
		Early = 3,
		Normal = 5,
		Late = 7,
		Later = 8,
		Last = 9
	}
}
