/******************************************************************************
  Copyright 2009-2011 dataweb GmbH
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

using System.Collections;
using System.Collections.Generic;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// A generic readonly collection of items providing an enumerator and the total number of items in the collection
	/// </summary>
	public interface IReadOnlyCollection<T> : IEnumerable<T>, ICollection { }


	/// <summary>
	/// A list based class implementing the IReadOnlyCollection interface
	/// </summary>
	public class ReadOnlyList<T> : List<T>, IReadOnlyCollection<T> {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ReadOnlyList`1" />.
		/// </summary>
		public ReadOnlyList()
			: base() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ReadOnlyList`1" />.
		/// </summary>
		public ReadOnlyList(int capacity)
			: base(capacity) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ReadOnlyList`1" />.
		/// </summary>
		public ReadOnlyList(IEnumerable<T> collection)
			: base(collection) {
		}

	}

}
