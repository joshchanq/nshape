using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;


namespace Dataweb.Utilities {

	/* The following naming scheme should be applied to resource string names:
	 * <Entity projectName>_<string projectName>
	 * Entity projectName is a projectName that describes the entity which defines the string.
	 * String projectName is a unique projectName within this class. */

	/// <summary>
	/// Loads strings from the resource of the library.
	/// </summary>
	static class Resources {

		public static string GetString(string name) {
			if (name == null) throw new ArgumentNullException("name");
			EnsureResourceManager();
			return resourceManager.GetString(name);
		}


		public static string FormatString(string formatName, object arg0) {
			if (formatName == null) throw new ArgumentNullException("formatName");
			EnsureResourceManager();
			return string.Format(resourceManager.GetString(formatName), arg0);
		}


		public static string FormatString(string formatName, object arg0, object arg1) {
			if (formatName == null) throw new ArgumentNullException("formatName");
			EnsureResourceManager();
			return string.Format(resourceManager.GetString(formatName), arg0, arg1);
		}


		public static string FormatString(string formatName, object arg0, object arg1, object arg2) {
			if (formatName == null) throw new ArgumentNullException("formatName");
			EnsureResourceManager();
			return string.Format(resourceManager.GetString(formatName), arg0, arg1, arg2);
		}


		public static string FormatString(string formatName, params object[] args) {
			if (formatName == null) throw new ArgumentNullException("formatName");
			EnsureResourceManager();
			return string.Format(resourceManager.GetString(formatName), args);
		}


		static Resources() {
			// Nothing to do yet.
		}


		// Makes sure the resource manager is isInitialized.
		private static void EnsureResourceManager() {
			if (resourceManager == null) {
				resourceManager = new ResourceManager("Dataweb.nShape.Properties.Resources", Assembly.GetExecutingAssembly());
			}
		}


		// Holds a resource manager for the default resources of the current library
		private static ResourceManager resourceManager;

	}

}