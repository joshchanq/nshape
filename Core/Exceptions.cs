using System;
using System.Collections.Generic;
using System.Text;

using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming {

	// TODO 3: Redesign exceptions
	public class DiagrammingException : Exception {

		public DiagrammingException(string message) : base(message) { }


		public DiagrammingException(string format, params object[] args)
			: base(string.Format(format, args), null) {
		}


		public DiagrammingException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}

	}


	public class DiagrammingSecurityException : DiagrammingException {

		public DiagrammingSecurityException(Permission permission)
			: base("Required permission '{0}' is not granted.", permission) {
		}

		public DiagrammingSecurityException(ICommand command)
			: base((command is Command) ?
			string.Format("'{0}' denied: Required permission '{1}' is not granted.", command.Description, ((Command)command).RequiredPermission)
			: string.Format("'{0}' denied: Required permission is not granted.", (command != null) ? command.Description : string.Empty)) {
		}

	}


	public class DiagrammingInternalException : Exception {

		public DiagrammingInternalException(string message) : base(message) { }


		public DiagrammingInternalException(string format, params object[] args)
			: base(string.Format(format, args), null) {
		}


		public DiagrammingInternalException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}
	}


	public class DiagrammingUnsupportedValueException : DiagrammingInternalException {
		public DiagrammingUnsupportedValueException(Type type, object value)
			: base("Unsupported {0} value '{1}'.", type.Name, value) {
		}
		public DiagrammingUnsupportedValueException(object value)
			: base((value != null) ? string.Format("Unsupported {0} value '{1}'.", value.GetType().Name, value) : "Unsupported value.") {
		}
	}


	public class DiagrammingInterfaceNotSupportedException : DiagrammingInternalException {
		public DiagrammingInterfaceNotSupportedException(Type instanceType, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) { }
		public DiagrammingInterfaceNotSupportedException(string instanceTypeName, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceTypeName, neededInterface.FullName) { }
		public DiagrammingInterfaceNotSupportedException(ShapeType instanceType, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) { }
		public DiagrammingInterfaceNotSupportedException(ModelObjectType instanceType, Type neededInterface) : base("Type '{0}' does not implement interface '{1}'.", instanceType.FullName, neededInterface.FullName) { }
	}


	public class DiagrammingMappingNotSupportedException : DiagrammingInternalException {
		public DiagrammingMappingNotSupportedException(Type shapeType, Type modelType) : base("Mapping between proeprty types '{0}' and '{1}' are not supported.", modelType.Name, shapeType.Name) { }
	}


	public class DiagrammingPropertyNotSetException : DiagrammingInternalException {
		public DiagrammingPropertyNotSetException(string propertyName) : base("Property '{0}' is not set.") { }
		public DiagrammingPropertyNotSetException(object propertyOwner, string propertyName) : base("Property '{0}' of {1} is not set.", propertyName, propertyOwner.GetType().Name) { }
	}
}
