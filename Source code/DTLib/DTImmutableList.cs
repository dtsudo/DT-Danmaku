
namespace DTLib
{
	using System.Collections.Generic;

	public class DTImmutableList<T> where T : class
	{
		private List<T> list;

		public DTImmutableList(List<T> list)
		{
			this.list = new List<T>();
			foreach (T item in list)
			{
				this.list.Add(item);
			}
		}

		public T this[int index]
		{
			get
			{
				return this.list[index];
			}
		}

		public int Count
		{
			get
			{
				return this.list.Count;
			}
		}
	}
}
