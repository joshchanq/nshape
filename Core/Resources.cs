/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the nShape framework.
  nShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  nShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  nShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Reflection;
using System.Resources;


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