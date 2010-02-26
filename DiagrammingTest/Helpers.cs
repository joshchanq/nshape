using System;
using System.Collections.Generic;
using System.Text;


namespace NShapeTest {

	// Enum helper class
	public static class Enum<T> where T : struct, IComparable {

		public static T Parse(string value) {
			return (T)Enum.Parse(typeof(T), value);
		}


		public static IList<T> GetValues() {
			IList<T> list = new List<T>();
			foreach (object value in Enum.GetValues(typeof(T)))
				list.Add((T)value);
			return list;
		}


		public static T GetNextValue(T currentValue) {
			T result = default(T);
			IList<T> values = Enum<T>.GetValues();
			int cnt = values.Count;
			for (int i = 0; i < cnt; ++i) {
				if (values[i].Equals(currentValue)) {
					if (i + 1 < cnt) result = values[i + 1];
					else result = values[0];
					break;
				}
			}
			return result;
		}

	}
}
