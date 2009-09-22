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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	// TODO 3: Redesign exceptions
	public class nShapeException : Exception {

		public nShapeException(string message) : base(message) { }


		public nShapeException(string format, params object[] args)
			: base(string.Format(format, args), null) {
		}


		public nShapeException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}

	}


	public class nShapeSecurityException : nShapeException {

		public nShapeSecurityException(Permission permission)
			: base("Required permission '{0}' is not granted.", permission) {
		}

		public nShapeSecurityException(ICommand command)
			: base((command is Command) ?
			string.Format("'{0}' denied: Required permission '{1}' is not granted.", command.Description, ((Command)command).RequiredPermission)
			: string.Format("'{0}' denied: Required permission is not granted.", (command != null) ? command.Description : string.Empty)) {
		}

	}


	public class nShapeInternalException : Exception {

		public nShapeInternalException(string message) : base(message) { }


		public nShapeInternalException(string format, params object[] args)
			: base(string.Format(format, args), null) {
		}


		public nShapeInternalException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}
	}


	public class nShapeUnsupportedValueException : nShapeInternalException {
		public nShapeUnsupportedValueException(Type type, object value)
			: base("Unsupported {0} value '{1}'.", type.Name, value) {
		}
		public nShapeUnsupportedValueException(object value)
			: base((value != null) ? string.Format("Unsupported {0} value '{1}'.", value.GetType().Name, value) : "Unsupported value.") {
		}
	}


	public class nShapeInterfaceNotSupportedException : nShapeInternalException {
		public nShapeInterfaceNotSupportedException(Type instanceType, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) { }
		public nShapeInterfaceNotSupportedException(string instanceTypeName, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceTypeName, neededInterface.FullName) { }
		public nShapeInterfaceNotSupportedException(ShapeType instanceType, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) { }
		public nShapeInterfaceNotSupportedException(ModelObjectType instanceType, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) { }
	}


	public class nShapeMappingNotSupportedException : nShapeInternalException {
		public nShapeMappingNotSupportedException(Type shapeType, Type modelType) : base("Mapping between proeprty types '{0}' and '{1}' are not supported.", modelType.Name, shapeType.Name) { }
	}


	public class nShapePropertyNotSetException : nShapeInternalException {
		public nShapePropertyNotSetException(string propertyName) : base("Property '{0}' is not set.") { }
		public nShapePropertyNotSetException(object propertyOwner, string propertyName) : base("Property '{0}' of {1} is not set.", propertyName, propertyOwner.GetType().Name) { }
	}
}
