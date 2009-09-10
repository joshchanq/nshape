using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;


namespace Dataweb.nShape.Advanced {

	public class TypeDescriptorRegistrar {
		
		public static void RegisterUITypeEditor(Type type, UITypeEditor editor) {
			if (registeredEditors.ContainsKey(type))
				registeredEditors[type] = editor;
			else registeredEditors.Add(type, editor);
		}


		public static void RegisterTypeConverter(Type type, TypeConverter converter) {
			if (registeredConverters.ContainsKey(type))
				registeredConverters[type] = converter;
			else registeredConverters.Add(type, converter);
		}


		public static bool TypeHasRegisteredUITypeEditor(Type type) {
			return registeredEditors.ContainsKey(type);
		}


		public static bool TypeHasRegisteredTypeConverter(Type type) {
			return registeredConverters.ContainsKey(type);
		}


		public static UITypeEditor GetRegisteredUITypeEditor(Type type) {
			UITypeEditor result;
			registeredEditors.TryGetValue(type, out result);
			return result;
		}


		public static TypeConverter GetRegisteredTypeConverter(Type type) {
			TypeConverter result;
			registeredConverters.TryGetValue(type, out result);
			return result;
		}


		#region Fields
		private static Dictionary<Type, UITypeEditor> registeredEditors = new Dictionary<Type, UITypeEditor>();
		private static Dictionary<Type, TypeConverter> registeredConverters = new Dictionary<Type, TypeConverter>();
		#endregion
	}
	

	public class nShapeStyleTypeDescriptionProvider : TypeDescriptionProvider {
		public nShapeStyleTypeDescriptionProvider()
			: base(TypeDescriptor.GetProvider(typeof(Style))) {
		}


		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
			return new nShapeStyleTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
		}
	}


	public class nShapeStyleTypeDescriptor : CustomTypeDescriptor {
		public nShapeStyleTypeDescriptor(ICustomTypeDescriptor parent)
			: base(parent) {
		}

		public override object GetEditor(Type editorBaseType) {
			UITypeEditor editor = TypeDescriptorRegistrar.GetRegisteredUITypeEditor(editorBaseType);
			if (editor != null) return editor;
			else return base.GetEditor(editorBaseType);
		}


		public override TypeConverter GetConverter() {
			return TypeDescriptorRegistrar.GetRegisteredTypeConverter(typeof(Style)) ?? base.GetConverter();
		}
	}


	#region TypeDescriptionProvider Sample 2

	[TypeDescriptionProvider(typeof(MyTypeDescriptionProvider))]
	public class MyClass {
	}


	public sealed class MyTypeDescriptionProvider : TypeDescriptionProvider {
		public MyTypeDescriptionProvider()
			: base(TypeDescriptor.GetProvider(typeof(MyClass))) { }

		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
			return new MyTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
		}
	}

	public sealed class MyTypeDescriptor : CustomTypeDescriptor {
		public MyTypeDescriptor(ICustomTypeDescriptor parent)
			: base(parent) {
		}

		public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
			PropertyDescriptorCollection result = base.GetProperties(attributes);
			// hier wäre eine gute stelle, an den props rumzufummeln
			return result;
		}
	}

	#endregion


	#region TypeDescriptonProvider Sample
	//[TypeDescriptionProvider(typeof(MyObjTypeDescriptionProvider))]
	//public class MyObj : INotifyPropertyChanged {
	//   #region Private variables
	//   private int m_i4Sum1;
	//   private int m_i4Sum2;
	//   #endregion

	//   #region Constructor
		
	//   public MyObj() {
	//   }

	//   public MyObj(int i4Sum1, int i4Sum2) {
	//      m_i4Sum1 = i4Sum1;
	//      m_i4Sum2 = i4Sum2;
	//   }

	//   #endregion

	//   #region Protected virtual methods

	//   protected virtual void OnPropertyChanged(string xsPropertyName) {
	//      PropertyChangedEventArgs e = new PropertyChangedEventArgs(xsPropertyName);
	//      if (PropertyChanged != null)
	//         PropertyChanged(this, e);
	//   }

	//   #endregion

	//   #region Public properties

	//   public int pi4Sum1 {
	//      get { return m_i4Sum1; }
	//      set {
	//         m_i4Sum1 = value;
	//         OnPropertyChanged("pi4Sum1");
	//      }
	//   }

	//   public int pi4Sum2 {
	//      get { return m_i4Sum2; }
	//      set {
	//         m_i4Sum2 = value;
	//         OnPropertyChanged("pi4Sum2");
	//      }
	//   }

	//   // pi4Result is provided by the MyObjTypeDescriptionProvider

	//   #endregion

	//   #region INotifyPropertyChanged Members

	//   public event PropertyChangedEventHandler PropertyChanged;

	//   #endregion
	//}


	//public class MyObjTypeDescriptionProvider : TypeDescriptionProvider {
	//   private TypeDescriptionProvider m_BaseProvider;
	//   internal PropertyDescriptorCollection m_PropertyCache;

	//   public MyObjTypeDescriptionProvider() {
	//   }

	//   public MyObjTypeDescriptionProvider(Type db) {
	//      m_BaseProvider = TypeDescriptor.GetProvider(db);
	//   }


	//   public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
	//      //return base.GetTypeDescriptor(objectType, instance);
	//      return new MyObjCustomTypeDescriptor(objectType, this, null, (MyObj)instance);
	//   }
	//}

	//public class MyObjCustomTypeDescriptor : CustomTypeDescriptor {
	//   Type m_ObjType;
	//   MyObjTypeDescriptionProvider m_Provider;
	//   MyObj m_MyObj;

	//   public MyObjCustomTypeDescriptor() {
	//   }

	//   public MyObjCustomTypeDescriptor(Type objType, MyObjTypeDescriptionProvider provider, ICustomTypeDescriptor parentDescriptor, MyObj myObj)
	//      : base(parentDescriptor) {
	//      m_ObjType = objType;
	//      m_Provider = provider;
	//      m_MyObj = myObj;
	//   }

	//   public override EventDescriptorCollection GetEvents() {
	//      return base.GetEvents();
	//   }

	//   public override EventDescriptorCollection GetEvents(Attribute[] attributes) {
	//      return base.GetEvents(attributes);
	//   }

	//   public override PropertyDescriptorCollection GetProperties() {
	//      return GetProperties(null);
	//   }

	//   public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
	//      if (m_Provider.m_PropertyCache != null) {
	//         // Return the cached property descriptors
	//         return m_Provider.m_PropertyCache;
	//      }
	//      else {
	//         // Create the property descriptors
	//         m_Provider.m_PropertyCache = new PropertyDescriptorCollection(null);

	//         foreach (PropertyInfo info in typeof(MyObj).GetProperties()) {
	//            // Hier wirst du dir die bereits bestehenden PropertyDescriptoren holen oder bauen müssen.
	//         }

	//         // Add my custom pi4Result property
	//         ResultPropertyDescriptor propDesc = new ResultPropertyDescriptor("pi4Result", null, m_MyObj);
	//         m_Provider.m_PropertyCache.Add(propDesc);
	//      }

	//      return m_Provider.m_PropertyCache;
	//   }

	//   public override object GetPropertyOwner(PropertyDescriptor pd) {
	//      return base.GetPropertyOwner(pd);
	//   }
	//}

	//// This PropertyDescriptor provides the pi4Result property which is added to MyObj
	//public class ResultPropertyDescriptor : PropertyDescriptor {
	//   private MyObj m_MyObj;


	//   public ResultPropertyDescriptor(string projectName, Attribute[] attributes, MyObj myObj)
	//      : base(projectName, attributes) {
	//      m_MyObj = myObj;
	//   }

	//   public override bool CanResetValue(object component) {
	//      return false;
	//   }

	//   public override Type ComponentType {
	//      get { return typeof(MyObj); }
	//   }

	//   public override object GetValue(object component) {
	//      MyObj comp = (MyObj)component;
	//      return comp.pi4Sum1 + comp.pi4Sum2;
	//   }

	//   public override bool IsReadOnly {
	//      get { return true; }
	//   }

	//   public override Type MappingPropertyType {
	//      get { return typeof(int); }
	//   }

	//   public override void ResetValue(object component) {

	//   }

	//   public override void SetFloat(object component, object value) {

	//   }

	//   public override bool ShouldSerializeValue(object component) {
	//      return false;
	//   }
	//}
	#endregion
}