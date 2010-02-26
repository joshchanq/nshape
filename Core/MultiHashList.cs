/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System.Collections.Generic;

namespace Dataweb.Utilities {

	/// <summary>
	/// Implements a list, whose index is a hash value and which can have multiple items
	/// per index value.
	/// </summary>
	public class MultiHashList<T>  {

		public MultiHashList(int capacity) {
			int listCapacity = capacity / order;
			list = new List<Element>(listCapacity);
			for (int i = 0; i < listCapacity; ++i) list.Add(null);
		}


		public void Add(uint key, T value) {
			Element newElement = new Element(key, value);
			if (list[(int)(key % list.Capacity)] == null)
				list[(int)(key % list.Capacity)] = newElement;
			else {
				Element e;
#if DEBUG
				int cnt = 0;
				for (e = list[(int)(key % list.Capacity)]; !e.item.Equals(value) && e.next != null; e = e.next) 
					++cnt;
				if (cnt > maxListLen) maxListLen = cnt;
				else if (cnt > 0 && cnt < minListLen) minListLen = cnt;
#else
				for (e = list[(int)(key % list.Capacity)]; !e.item.Equals(value) && e.next != null; e = e.next) ;
#endif
				// TODO 2: Optimize the second comparison.
				if (!e.item.Equals(value)) e.next = newElement;
			}
		}


		public bool Remove(uint key, T value) {
			if (list[(int)(key % list.Capacity)] == null) return false;
			Element e;
			if (list[(int)(key % list.Capacity)].item.Equals(value)) {
				list[(int)(key % list.Capacity)] = list[(int)(key % list.Capacity)].next;
				return true;
			} else {
				for (e = list[(int)(key % list.Capacity)]; 
					e.next != null && (e.next.key != key || !e.next.item.Equals(value)); 
					e = e.next) ;
				// Either e.next is null or the one we are searching for
				if (e.next == null) return false;
				e.next = e.next.next;
				return true;
			}
		}


		public void Clear() {
			// Clear list by overwriting all items with null
			for (int i = list.Count - 1; i >= 0; --i) list[i] = null;
		}


		public IEnumerable<T> this[uint key] {
			get {
				if (list[(int)(key % list.Capacity)] == null) yield break;
				for (Element e = list[(int)(key % list.Capacity)]; e != null; e = e.next)
					if (e.key == key) yield return e.item;
			}
		}


#if DEBUG
		
		/// <summary>
		/// Returns the number of entries in the longest list.
		/// </summary>
		public int MaxListLength {
			get { return maxListLen; }
		}


		/// <summary>
		/// Returns the nuber of lists that are not empty.
		/// </summary>
		public int ListCount {
			get {
				int cnt = 0;
				for (int i = list.Count - 1; i >= 0; --i ) {
					if (list[i] != null) ++cnt;
				}
				return cnt;
			}
		}

#endif
		
		
		private class Element {
			public Element(uint key, T item) {
				this.key = key;
				this.item = item;
			}
			public uint key;
			public T item;
			public Element next;
		}


		private const int order = 3;
		private List<Element> list;
#if DEBUG
		private int maxListLen = 0;
		private int minListLen = 0;
#endif
	}

}