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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.Advanced {

	internal static class TypeDescriptorRegistrar {

		public static void RegisterUITypeEditor(Type type, Type uiTypeEditorType) {
			if (type == null) throw new ArgumentNullException("type");
			if (uiTypeEditorType == null) throw new ArgumentNullException("typeConverterType");
			if (!IsType(uiTypeEditorType, typeof(UITypeEditor)))
				throw new ArgumentException(string.Format("{0} is not a {1}.", type.Name, typeof(UITypeEditor).Name));

			if (registeredEditors.ContainsKey(type))
				registeredEditors[type] = uiTypeEditorType;
			else registeredEditors.Add(type, uiTypeEditorType);
		}


		public static void UnregisterUITypeEditor(Type type, Type uiTypeEditorType) {
			registeredEditors.Remove(type);
		}


		public static void RegisterTypeConverter(Type type, Type typeConverterType) {
			if (type == null) throw new ArgumentNullException("type");
			if (typeConverterType == null) throw new ArgumentNullException("typeConverterType");
			if (!IsType(typeConverterType, typeof(TypeConverter)))
				throw new ArgumentException(string.Format("{0} is not a {1}.", type.Name, typeof(TypeConverter).Name));

			if (registeredConverters.ContainsKey(type))
				registeredConverters[type] = typeConverterType;
			else registeredConverters.Add(type, typeConverterType);
		}


		public static void UnregisterTypeConverter(Type type, Type typeConverterType) {
			registeredConverters.Remove(type);
		}


		public static UITypeEditor GetRegisteredUITypeEditor(Type type) {
			UITypeEditor result = null;
			Type editorType = null;
			if (registeredEditors.TryGetValue(type, out editorType))
				result = Activator.CreateInstance(editorType) as UITypeEditor;
			return result;
		}


		public static TypeConverter GetRegisteredTypeConverter(Type type) {
			TypeConverter result = null;
			Type converterType = null;
			if (registeredConverters.TryGetValue(type, out converterType))
				result = Activator.CreateInstance(converterType) as TypeConverter;
			return result;
		}


		private static bool IsType(Type sourceType, Type targetType) {
			return (sourceType == targetType
				|| sourceType.IsSubclassOf(targetType)
				|| sourceType.GetInterface(targetType.Name, true) != null);
		}


		#region Fields
		private static Dictionary<Type, Type> registeredEditors = new Dictionary<Type, Type>();
		private static Dictionary<Type, Type> registeredConverters = new Dictionary<Type, Type>();
		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class TypeDescriptionProviderDg : TypeDescriptionProvider {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.TypeDescriptionProviderDg" />
		/// </summary>
		public TypeDescriptionProviderDg()
			: base(TypeDescriptor.GetProvider(typeof(object))) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.TypeDescriptionProviderDg" />
		/// </summary>
		public TypeDescriptionProviderDg(Type type)
			: base(TypeDescriptor.GetProvider(type)) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.TypeDescriptionProviderDg" />
		/// </summary>
		public TypeDescriptionProviderDg(TypeDescriptionProvider parent)
			: base(parent) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IPropertyController PropertyController {
			set { propertyController = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
			ICustomTypeDescriptor baseTypeDescriptor = base.GetTypeDescriptor(objectType, instance);
			if (propertyController != null)
				return new TypeDescriptorDg(baseTypeDescriptor, propertyController);
			else return baseTypeDescriptor;
		}


		private static IPropertyController propertyController;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class TypeDescriptorDg : CustomTypeDescriptor {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.TypeDescriptorDg" />
		/// </summary>
		public TypeDescriptorDg(ICustomTypeDescriptor parent, IPropertyController propertyController)
			: base(parent) {
			if (propertyController == null) throw new ArgumentNullException("propertyController");
			this.propertyController = propertyController;
		}


		/// <override></override>
		public override AttributeCollection GetAttributes() {
			return base.GetAttributes();
		}


		/// <override></override>
		public override string GetClassName() {
			return base.GetClassName();
		}


		/// <override></override>
		public override string GetComponentName() {
			return base.GetComponentName();
		}


		/// <override></override>
		public override TypeConverter GetConverter() {
			return base.GetConverter();
		}


		/// <override></override>
		public override EventDescriptor GetDefaultEvent() {
			return base.GetDefaultEvent();
		}


		/// <override></override>
		public override PropertyDescriptor GetDefaultProperty() {
			PropertyDescriptor propertyDescriptor = base.GetDefaultProperty();
			if (propertyDescriptor != null && propertyController != null)
				return new PropertyDescriptorDg(propertyDescriptor, propertyController, GetPropertyOwner(propertyDescriptor));
			else return propertyDescriptor;
		}


		/// <override></override>
		public override object GetEditor(Type editorBaseType) {
			return base.GetEditor(editorBaseType);
		}


		/// <override></override>
		public override EventDescriptorCollection GetEvents() {
			return base.GetEvents();
		}


		/// <override></override>
		public override EventDescriptorCollection GetEvents(Attribute[] attributes) {
			return base.GetEvents(attributes);
		}


		/// <override></override>
		public override object GetPropertyOwner(PropertyDescriptor pd) {
			return base.GetPropertyOwner(pd);
		}


		/// <override></override>
		public override PropertyDescriptorCollection GetProperties() {
			if (propertyController != null)
				return DoGetProperties(base.GetProperties());
			else return base.GetProperties();
		}


		/// <override></override>
		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
			if (propertyController != null) 
				return DoGetProperties(base.GetProperties(attributes));
			else return base.GetProperties(attributes);
		}


		private PropertyDescriptorCollection DoGetProperties(PropertyDescriptorCollection baseProperties) {
			PropertyDescriptor[] resultProperties = new PropertyDescriptor[baseProperties.Count];
			int baseCnt = baseProperties.Count;
			for (int i = 0; i < baseCnt; ++i) {
				PropertyDescriptorDg propDesc = new PropertyDescriptorDg(baseProperties[i], propertyController, GetPropertyOwner(baseProperties[i]));
				resultProperties[i] = propDesc;
			}
			return new PropertyDescriptorCollection(resultProperties);
		}


		private IPropertyController propertyController;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class PropertyDescriptorDg : PropertyDescriptor {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.PropertyDescriptorDg" />
		/// </summary>
		public PropertyDescriptorDg(PropertyDescriptor descriptor, IPropertyController controller, object propertyOwner)
			: base(descriptor.Name, GetAttributes(controller, descriptor, propertyOwner)) {
			this.controller = controller;
			this.descriptor = descriptor;

			browsableAttr = Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
			readOnlyAttr = Attributes[typeof(ReadOnlyAttribute)] as ReadOnlyAttribute;
			descriptionAttr = Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
		}


		/// <override></override>
		public override bool CanResetValue(object component) {
			return descriptor.CanResetValue(component);
		}


		/// <override></override>
		public override Type ComponentType {
			get { return descriptor.ComponentType; }
		}


		/// <override></override>
		public override object GetValue(object component) {
			return descriptor.GetValue(component);
		}


		/// <override></override>
		public override void SetValue(object component, object value) {
			if (permissionAttr != null) {
				if (controller.Project == null) throw new InvalidOperationException("PropertyController.Project is not set.");
				if (controller.Project.SecurityManager == null) throw new InvalidOperationException("PropertyController.Project.SecurityManager is not set.");
				bool isGranted;
				if (component is Shape) {
					Shape s = component as Shape;
					if (s != null) isGranted = controller.Project.SecurityManager.IsGranted(permissionAttr.Permission, s);
					else isGranted = controller.Project.SecurityManager.IsGranted(permissionAttr.Permission);
				} else isGranted = controller.Project.SecurityManager.IsGranted(permissionAttr.Permission);
				if (!isGranted) {
					controller.CancelSetProperty();
					throw new NShapeSecurityException(permissionAttr.Permission);
				}
			}
			controller.SetPropertyValue(component, descriptor.Name, descriptor.GetValue(component), value);
		}


		/// <override></override>
		public override string Description {
			get { return (descriptionAttr != null) ? descriptionAttr.Description : base.Description; }
		}

		
		/// <override></override>
		public override bool IsReadOnly {
			get { return (readOnlyAttr != null) ? readOnlyAttr.IsReadOnly : false; }
		}


		/// <override></override>
		public override bool IsBrowsable {
			get { return (browsableAttr != null) ? browsableAttr.Browsable : base.IsBrowsable; }
		}


		/// <override></override>
		public override bool IsLocalizable {
			get {
				//return base.IsLocalizable; 
				return false;
			}
		}


		///// <override></override>
		//public override object GetEditor(Type editorBaseType) {
		//    return IsReadOnly ? null : base.GetEditor(editorBaseType);
		//}


		/// <override></override>
		public override Type PropertyType {
			get { return descriptor.PropertyType; }
		}


		/// <override></override>
		public override void ResetValue(object component) {
			descriptor.ResetValue(component);
		}


		/// <override></override>
		public override bool ShouldSerializeValue(object component) {
			return descriptor.ShouldSerializeValue(component);
		}


		private static Attribute[] GetAttributes(IPropertyController controller, PropertyDescriptor descriptor, object propertyOwner) {
			if (controller == null) throw new ArgumentNullException("controller");
			if (descriptor == null) throw new ArgumentNullException("descriptor");
			if (propertyOwner == null) throw new ArgumentNullException("propertyOwner");
			try {
				int attrCount = descriptor.Attributes.Count;
				BrowsableAttribute browsableAttr = descriptor.Attributes[typeof(BrowsableAttribute)] as BrowsableAttribute;
				ReadOnlyAttribute readOnlyAttr = descriptor.Attributes[typeof(ReadOnlyAttribute)] as ReadOnlyAttribute;
				DescriptionAttribute descriptionAttr = descriptor.Attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
				RequiredPermissionAttribute requiredPermissionAttr = descriptor.Attributes[typeof(RequiredPermissionAttribute)] as RequiredPermissionAttribute;

				if (requiredPermissionAttr != null) {
					Permission permission = requiredPermissionAttr.Permission;
					// Check if property is viewable
					if (!IsGranted(controller, permission, propertyOwner)) {
						// Hide if PropertyDisplayMode is 'Hidden' and 'View' access is not granted,
						// otherwise set peroperty to readonly 
						if (controller.PropertyDisplayMode == NonEditableDisplayMode.Hidden
							&& !IsGranted(controller, permission, propertyOwner)) {
							browsableAttr = BrowsableAttribute.No;
						} else {
							readOnlyAttr = ReadOnlyAttribute.Yes;
							descriptionAttr = GetNotGrantedDescription(permission);
						}
					}
				}

				// Now copy all attributes
				int cnt = descriptor.Attributes.Count;
				List<Attribute> result = new List<Attribute>(attrCount);
				// Add stored/modified attributes first
				if (browsableAttr != null) result.Add(browsableAttr);
				if (readOnlyAttr != null) result.Add(readOnlyAttr);
				if (descriptionAttr != null) result.Add(descriptionAttr);
				if (requiredPermissionAttr != null) result.Add(requiredPermissionAttr);
				// Copy all other attributes
				for (int i = 0; i < cnt; ++i) {
					// Skip stored/modified attributes
					if (descriptor.Attributes[i] is BrowsableAttribute) continue;
					else if (descriptor.Attributes[i] is ReadOnlyAttribute) continue;
					else if (descriptor.Attributes[i] is DescriptionAttribute) continue;
					else if (descriptor.Attributes[i] is EditorAttribute) {
						if (readOnlyAttr != null && readOnlyAttr.IsReadOnly)
							continue;
					}
					result.Add(descriptor.Attributes[i]);
				}
				return result.ToArray();
			} catch (Exception) {
				throw;
			}
		}


		private static bool IsGranted(IPropertyController controller, Permission permissions, object propertyOwner) {
			if (propertyOwner is Shape) {
				Shape s = propertyOwner as Shape;
				if (s != null) return controller.Project.SecurityManager.IsGranted(permissions, s);
			}
			return controller.Project.SecurityManager.IsGranted(permissions);
		}


		private static DescriptionAttribute GetNotGrantedDescription(Permission permission) {
			return new DescriptionAttribute(string.Format("Required permission '{0}' is not granted.", permission));
		}


		private IPropertyController controller = null;
		private PropertyDescriptor descriptor = null;
		private RequiredPermissionAttribute permissionAttr = null;
		private ReadOnlyAttribute readOnlyAttr = null;
		private BrowsableAttribute browsableAttr = null;
		private DescriptionAttribute descriptionAttr = null;
	}

}