using Dataweb.Utilities;
using System.Collections.Generic;

namespace Dataweb.NShape {
	
#if DEBUG
	public static class NShapeDiagnostics {

		public static bool ContainsCounter(string name) {
			return counters.ContainsKey(name);
		}


		public static long GetCounter(string name) {
			return counters[name];
		}
		
		
		public static void AddCounter(string name) {
			counters.Add(name, 0);
		}


		public static bool RemoveCounter(string name) {
			return counters.Remove(name);
		}


		public static void IncrementCounter(string name) {
			++counters[name];
		}


		public static void DecrementCounter(string name) {
			--counters[name];
		}


		public static void ResetCounter(string name) {
			counters[name] = 0;
		}
		
		
		private static Dictionary<string, long> counters = new Dictionary<string,long>();
	}
#endif

}
