using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;


namespace Dataweb.Diagramming.Advanced {

	/// <summary>
	/// A generic readonly collection of items providing an enumerator and the total number of items in the collection
	/// </summary>
	public interface IReadOnlyCollection<T> : IEnumerable<T>, ICollection { }


	/// <summary>
	/// A list based class implementing the IReadOnlyCollection interface
	/// </summary>
	/// <typeparam projectName="T"></typeparam>
	public class ReadOnlyList<T> : List<T>, IReadOnlyCollection<T> {
		/// <override></override>
		public ReadOnlyList() : base() { }

		public ReadOnlyList(int capacity) : base(capacity) { }

		public ReadOnlyList(IEnumerable<T> collection) : base(collection) { }
	}

}
