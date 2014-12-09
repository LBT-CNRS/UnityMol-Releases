using System;
using System.Collections;

namespace BenTools.Data
{
	/// <summary>
	/// Summary description for Hashset.
	/// </summary>
	public class HashSet : IEnumerable, ICollection
	{
		Hashtable H = new Hashtable();
		object Dummy = new object();
		public HashSet(){}
		public void Add(object O)
		{
			H[O] = Dummy;
		}
		public void AddRange(IEnumerable List)
		{
			foreach(object O in List)
				Add(O);
		}
		public void Remove(object O)
		{
			H.Remove(O);
		}
		public bool Contains(object O)
		{
			return H.ContainsKey(O);
		}
		public void Clear()
		{
			H.Clear();
		}
		public IEnumerator GetEnumerator()
		{
			return H.Keys.GetEnumerator();
		}
		public int Count
		{
			get
			{
				return H.Count;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return H.IsSynchronized;
			}
		}

		public void CopyTo(Array array, int index)
		{
			H.Keys.CopyTo(array,index);
		}

		public object SyncRoot
		{
			get
			{
				return H.SyncRoot;
			}
		}
	}
}
