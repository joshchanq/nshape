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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;


namespace Dataweb.NShape.Advanced {

	public class Model : IEntity {

		public Model() { }
		
		
		#region IEntity Members

		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield break;
		}


		public object Id {
			get { return id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) 
				throw new InvalidOperationException(string.Format("{0} has already an id.", GetType().Name));
			this.id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			// nothing to do
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			// nothing to do
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			// nothing to do
		}

		#endregion


		private const string entityTypeName = "Core.Model";
		private object id = null;
	}
	
	
	/// <summary>
	/// Defines a connection port for a model object.
	/// </summary>
	/// <status>reviewed</status>
	public struct TerminalId {

		/// <summary>Specifies the invalid connection port.</summary>
		public static readonly TerminalId Invalid;

		/// <summary>Specifies a port for the model object as a whole.</summary>
		public static readonly TerminalId Generic;


		public static implicit operator int(TerminalId tid) {
			return tid.id;
		}


		/// <summary>Converts an integer to a terminal id.</summary>
		public static implicit operator TerminalId(int value) {
			TerminalId result = Invalid;
			result.id = value;
			return result;
		}


		public static bool operator ==(TerminalId tid1, TerminalId tid2) {
			return tid1.id == tid2.id;
		}


		public static bool operator !=(TerminalId tid1, TerminalId tid2) {
			return tid1.id != tid2.id;
		}


		public override bool Equals(object obj) {
			return obj is TerminalId && (TerminalId)obj == this;
		}


		public override int GetHashCode() {
			return id.GetHashCode();
		}


		public override string ToString() {
			return id.ToString();
		}
		

		static TerminalId() {
			Invalid.id = int.MinValue;
			Generic.id = 0;
		}


		private int id;
	}


	/// <summary>
	/// Defines the interface between the nShape framework and the model objects.
	/// </summary>
	public interface IModelObject : IEntity {

		/// <summary>
		/// Name of the model object. Is unique with siblings.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Type of this model object
		/// </summary>
		ModelObjectType Type { get; }

		/// <summary>
		/// Owning model object, can be null if this object is a root object.
		/// </summary>
		IModelObject Parent { get; set; }

		/// <summary>
		/// Creates a copy of this model object.
		/// </summary>
		/// <remarks>Composite objects are also cloned, references to aggregated objects are just copied.</remarks>
		IModelObject Clone();

		/// <summary>
		/// Connects the model object with another one.
		/// </summary>
		/// <param name="ownTerminalIndex"></param>
		/// <param name="targetConnector"></param>
		/// <param name="targetTerminalIndex"></param>
		void Connect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId);

		/// <summary>
		/// Disconnects the model object from another one.
		/// </summary>
		/// <param name="ownTerminalIndex"></param>
		/// <param name="targetConnector"></param>
		/// <param name="targetTerminalIndex"></param>
		void Disconnect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId);

		/// <summary>
		/// Retrieves the attached shapes.
		/// </summary>
		IEnumerable<Shape> Shapes { get; }

		/// <summary>
		/// Attaches an observing shape.
		/// </summary>
		/// <param name="shape"></param>
		void AttachShape(Shape shape);

		/// <summary>
		/// Detaches an observing shape.
		/// </summary>
		/// <param name="shape"></param>
		void DetachShape(Shape shape);

		/// <summary>
		/// Retrieves the integer value of a field.
		/// </summary>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		int GetInteger(int propertyIndex);

		/// <summary>
		/// Retrieves the float value of a field.
		/// </summary>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		float GetFloat(int propertyIndex);

		/// <summary>
		/// Retrieves the string value of a field.
		/// </summary>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		string GetString(int propertyIndex);

		/// <summary>
		/// Gets the possible actions for this model object.
		/// </summary>
		/// <returns></returns>
		IEnumerable<nShapeAction> GetActions();

	}


	/// <summary>
	/// Represents the method that is called to create a model object.
	/// </summary>
	/// <param name="modelObjectType"></param>
	/// <returns></returns>
	/// <status>reviewed</status>
	public delegate IModelObject CreateModelObjectDelegate(ModelObjectType modelObjectType);


	/// <summary>
	/// Represents a model object type.
	/// </summary>
	// Libraries register their model object types via:
	public abstract class ModelObjectType {

		/// <summary>
		/// Constructs a model object type.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="namespaceName"></param>
		/// <param name="createModelObjectDelegate"></param>
		/// <param name="propertyInfos"></param>
		public ModelObjectType(string name, string libraryName, string categoryTitle, CreateModelObjectDelegate createModelObjectDelegate, 
			GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate) {
			if (name == null) throw new ArgumentNullException("name");
			if (!Project.IsValidName(name)) 
				throw new ArgumentException(string.Format("'{0}' is not a valid model object type name.", name));
			if (libraryName == null) throw new ArgumentNullException("libraryName");
			if (!Project.IsValidName(libraryName))
				throw new ArgumentException(string.Format("'{0}' is not a valid library name.", libraryName));
			if (createModelObjectDelegate == null) throw new ArgumentNullException("createModelObjectDelegate");
			if (getPropertyDefinitionsDelegate == null) throw new ArgumentNullException("getPropertyDefinitionsDelegate");
			//
			this.name = name;
			this.libraryName = libraryName;
			this.categoryTitle = categoryTitle;
			this.createModelObjectDelegate = createModelObjectDelegate;
			this.getPropertyDefinitionsDelegate = getPropertyDefinitionsDelegate;
		}


		/// <summary>
		/// Specifies the language invariant name of the model object type.
		/// </summary>
		public string Name { 
			get { return name; } 
		}


		/// <summary>
		/// Indicates the name of the library where the model object type is implemented.
		/// </summary>
		public string LibraryName {
			get { return libraryName; }
		}


		/// <summary>
		/// Specifies the full language invariant name of the model object type.
		/// </summary>
		public string FullName { 
			get { return string.Format("{0}.{1}", libraryName, name); } 
		}


		/// <summary>
		/// Specifies the culture depending description of the model type.
		/// </summary>
		public string Description {
			get { return description; }
			set { description = value; }
		}


		/// <summary>
		/// Indicates the default for the culture depending category name.
		/// </summary>
		public string DefaultCategoryTitle {
			get { return categoryTitle; }
		}


		/// <summary>
		/// Creates a model object instance of this type.
		/// </summary>
		public IModelObject CreateInstance() {
			return createModelObjectDelegate(this);
		}
		

		/// <summary>
		/// Indicates the propertiese of the model object type.
		/// </summary>
		public IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) { 
			return getPropertyDefinitionsDelegate(version);
		}


		/// <summary>
		/// Indicates largest available terminal id for this type.
		/// </summary>
		public abstract TerminalId MaxTerminalId { get; }


		/// <summary>
		/// Retreives the name of a terminal.
		/// </summary>
		/// <param name="terminalIndex"></param>
		/// <returns></returns>
		public abstract string GetTerminalName(TerminalId terminalId);

		/// <summary>
		/// Retrieves the id of a terminal.
		/// </summary>
		/// <param name="terminalName"></param>
		/// <returns></returns>
		public abstract TerminalId FindTerminalId(string terminalName);


		internal string GetDefaultName() {
			return string.Format("{0} {1}", name, ++nameCounter);
		}


		#region Fields

		private string name;
		private string libraryName;
		private string description;
		private string categoryTitle = string.Empty;
		private CreateModelObjectDelegate createModelObjectDelegate;
		private GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate;

		private int nameCounter = 0;

		#endregion
	}


	public class GenericModelObjectType : ModelObjectType {

		public GenericModelObjectType(string name, string namespaceName, string categoryTitle, CreateModelObjectDelegate createModelObjectDelegate,
			GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate, TerminalId maxTerminalId)
			: base(name, namespaceName, categoryTitle, createModelObjectDelegate, getPropertyDefinitionsDelegate) {
			this.maxTerminalId = maxTerminalId;
		}


		public override TerminalId MaxTerminalId {
			get { return maxTerminalId; }
		}


		public override string GetTerminalName(TerminalId terminalId) {
			return "Terminal " + Convert.ToString(terminalId);
		}


		public override TerminalId FindTerminalId(string terminalName) {
			if (string.IsNullOrEmpty(terminalName)) return TerminalId.Invalid;
			foreach (KeyValuePair<TerminalId, string> item in terminals) {
				if (item.Value.Equals(terminalName, StringComparison.InvariantCultureIgnoreCase))
					return item.Key;
			}
			return TerminalId.Invalid;
		}

		#region Fields

		TerminalId maxTerminalId;
		private Dictionary<TerminalId, string> terminals = new Dictionary<TerminalId, string>();

		#endregion

	}


	/// <summary>
	/// Defines a read-only collection of model object types.
	/// </summary>
	public interface IReadOnlyModelObjectTypeCollection: IReadOnlyCollection<ModelObjectType> {

		ModelObjectType this[string modelObjectTypeName] { get; }

	}


	/// <summary>
	/// Manages a list of model object types.
	/// </summary>
	public class ModelObjectTypeCollection : IReadOnlyModelObjectTypeCollection {

		internal ModelObjectTypeCollection() {
		}


		/// <summary>
		/// Adds a model object type to the collection.
		/// </summary>
		/// <param name="modelObjectType"></param>
		public void Add(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("modelObjectType");
			modelObjectTypes.Add(modelObjectType.FullName, modelObjectType);
		}


		/// <summary>
		/// Retrieves the model object type with the given projectName.
		/// </summary>
		/// <param name="typeName">Either a full (i.e. including the namespace) or partial model object type projectName</param>
		/// <returns>ModelObjectTypes object type with given projectName.</returns>
		public ModelObjectType GetModelObjectType(string typeName) {
			if (typeName == null) throw new ArgumentNullException("typeName");
			ModelObjectType result = null;
			if (!modelObjectTypes.TryGetValue(typeName, out result)) {
				foreach (KeyValuePair<string, ModelObjectType> item in modelObjectTypes) {
					// if no matching type projectName was found, check if the given type projectName was a type projectName without namespace
					if (item.Value.Name == typeName) {
						if (result == null) result = item.Value;
						else throw new ArgumentException("The model object type '{0}' is ambiguous. Please specify the library name.", typeName);
					}
				}
			}
			if (result == null)
				throw new ArgumentException(string.Format("Model object type '{0}' was not registered.", typeName));
			return result;
		}


		public ModelObjectType this[string modelObjectTypeName] {
			get { return GetModelObjectType(modelObjectTypeName); }
		}


		public int Count {
			get { return modelObjectTypes.Count; }
		}


		internal bool IsModelObjectTypeRegistered(ModelObjectType modelObjectType) {
			return modelObjectTypes.ContainsKey(modelObjectType.FullName);
		}


		internal void Clear() {
			modelObjectTypes.Clear();
		}


		#region IEnumerable<Type> Members

		public IEnumerator<ModelObjectType> GetEnumerator() {
			return modelObjectTypes.Values.GetEnumerator();
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return modelObjectTypes.Values.GetEnumerator();
		}

		#endregion


		#region ICollection Members

		public void CopyTo(Array array, int index) {
			if (array == null) throw new ArgumentNullException("array");
			modelObjectTypes.Values.CopyTo((ModelObjectType[])array, index);
		}


		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { throw new NotSupportedException(); }
		}

		#endregion


		#region Fields

		// Key = ModelObjectType.FullName, Value = ModelObjectType
		private Dictionary<string, ModelObjectType> modelObjectTypes = new Dictionary<string, ModelObjectType>();

		#endregion

	}


	/// <summary>
	/// Base class for model objects implementing naming, model hierarchy and shape management.
	/// </summary>
	/// <remarks>ModelObjectTypes objects can inherit from this class but need not.</remarks>
	public abstract class ModelObjectBase : IModelObject {

		protected internal ModelObjectBase(ModelObjectBase source) {
			id = null;
			modelObjectType = source.Type;
			name = modelObjectType.GetDefaultName();

			// do not clone these properties:
			//Parent = source.Parent;
		}


		#region IModelObject Members

		[Description("Indicates the name used to identify the Device.")]
		public string Name {
			get { return name; }
			set { name = value; }
		}


		[Description("The type of the ModelObject.")]
		public ModelObjectType Type {
			get { return modelObjectType; }
		}


		/// <summary>
		/// Parent of the model objects. Only the root object has no parent. Sometimes
		/// temporary objects have no parent and are therefore orphaned. E.g. when cloning
		/// model objects the clones do not have parents.
		/// </summary>
		public virtual IModelObject Parent {
			get { return parent; }
			set { parent = value; }
		}


		public abstract IModelObject Clone();


		public virtual int GetInteger(int propertyId) {
			throw new nShapeException("No integer property with PropertyId {0} found.", propertyId);
		}


		public virtual float GetFloat(int propertyId) {
			throw new nShapeException("No float property with PropertyId {0} found.", propertyId);
		}


		public virtual string GetString(int propertyId) {
			throw new nShapeException("No string property with PropertyId {0} found.", propertyId);
		}


		public abstract IEnumerable<nShapeAction> GetActions();


		public abstract void Connect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId);


		public abstract void Disconnect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId);


		/// <override></override>
		public IEnumerable<Shape> Shapes {
			get { return shapes.Keys; }
		}


		/// <override></override>
		public void AttachShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (shapes.ContainsKey(shape)) throw new nShapeException("{0} '{1}' is already attached to this shape.", Type.Name, Name);
			shapes.Add(shape, null);
		}


		/// <override></override>
		public void DetachShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (!shapes.ContainsKey(shape)) throw new nShapeException("{0} '{1}' is not attached to this shape.", Type.Name, Name);
			shapes.Remove(shape);
		}


		#endregion


		#region IEntity Members

		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
		}


		// unique id of object, does never change
		public object Id { 
			get { return id; } 
		}


		public void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) throw new InvalidOperationException("Model object has already an id.");
			this.id = id;
		}


		public virtual void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			name = reader.ReadString();
		}


		public virtual void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			// nothing to do
		}


		public virtual void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteString(name);
		}


		public virtual void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			// nothing to do
		}


		public virtual void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition) {
					writer.DeleteInnerObjects();
				}
			}
		}

		#endregion


		protected internal ModelObjectBase(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("ModelObjectType");
			this.modelObjectType = modelObjectType;
			this.name = modelObjectType.GetDefaultName();
		}


		protected void OnPropertyChanged(int propertyId) {
			foreach (Shape shape in Shapes) shape.NotifyModelChanged(propertyId);
		}


		#region Fields
		private const string persistentTypeName = "ModelObject";

		protected int terminalCount;

		private object id = null;
		private ModelObjectType modelObjectType = null;
		private string name = string.Empty;
		private IModelObject parent = null;
		private Dictionary<Shape, object> shapes = new Dictionary<Shape, object>(1);

		#endregion
	}


	/// <summary>
	/// ModelObjectTypes object with configurable number and type of properties.
	/// </summary>
	public class GenericModelObject : ModelObjectBase, IEntity {

		public static GenericModelObject CreateInstance(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("modelObjectType");
			return new GenericModelObject(modelObjectType);
		}


		protected internal GenericModelObject(ModelObjectType modelObjectType)
			: base(modelObjectType) {
		}


		protected internal GenericModelObject(GenericModelObject source)
			: base(source) {
		}


		public override IModelObject Clone() {
			return new GenericModelObject(this);
		}


		public override int GetInteger(int propertyId) {
			if (propertyId == PropertyIdIntegerValue) return IntegerValue;
			else return base.GetInteger(propertyId);
		}


		public override float GetFloat(int propertyId) {
			if (propertyId == PropertyIdFloatValue) return FloatValue;
			else return base.GetFloat(propertyId);
		}


		public override string GetString(int propertyId) {
			if (propertyId == PropertyIdStringValue) return StringValue;
			else return base.GetString(propertyId);
		}
				
		
		[PropertyMappingId(PropertyIdIntegerValue)]
		[Description("The value of the device. This value is represented by the assigned Shape.")]
		public int IntegerValue {
			get { return integerValue; }
			set {
				integerValue = value;
				OnPropertyChanged(PropertyIdIntegerValue);
			}
		}


		[PropertyMappingId(PropertyIdFloatValue)]
		[Description("The value of the device. This value is represented by the assigned Shape.")]
		public float FloatValue {
			get { return floatValue; }
			set {
				floatValue = value;
				OnPropertyChanged(PropertyIdFloatValue);
			}
		}


		[PropertyMappingId(PropertyIdStringValue)]
		[Description("The value of the device. This value is represented by the assigned Shape.")]
		public string StringValue {
			get { return stringValue; }
			set {
				stringValue = value;
				OnPropertyChanged(PropertyIdStringValue);
			}
		}


		public override IEnumerable<nShapeAction> GetActions() {
			//yield return new NotImplementedAction("Set State");
			yield break;
		}


		public override void Connect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException("Not yet implemented");
		}


		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException("Not yet implemented");
		}


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			integerValue = reader.ReadInt32();
			floatValue = reader.ReadFloat();
			stringValue = reader.ReadString();
		}


		public override void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			base.LoadInnerObjects(propertyName, reader, version);
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32(integerValue);
			writer.WriteFloat(floatValue);
			writer.WriteString(stringValue);
		}


		public override void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			base.SaveInnerObjects(propertyName, writer, version);
		}


		public override void Delete(IRepositoryWriter writer, int version) {
			base.Delete(writer, version);
		}


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in ModelObjectBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("IntegerValue", typeof(int));
			yield return new EntityFieldDefinition("FloatValue", typeof(float));
			yield return new EntityFieldDefinition("StringValue", typeof(string));
		}

		#endregion


		#region Fields

		protected const int PropertyIdGenericValue = 1;
		protected const int PropertyIdStringValue = 2;
		protected const int PropertyIdIntegerValue = 3;
		protected const int PropertyIdFloatValue = 4;

		private int integerValue;
		private float floatValue;
		private string stringValue;

		#endregion
	}
}