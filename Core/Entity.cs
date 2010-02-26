/******************************************************************************
  Copyright 2009 dataweb GmbH
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
using System.Drawing;
using System.Collections;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Defines a type that can be read from and written into a repository.
	/// </summary>
	/// <status>reviewed</status>
	public interface IEntity {

		/// <summary>
		/// Indicates the id of the entity.
		/// </summary>
		object Id { get; }

		/// <summary>
		/// Assigns a new unique Id. May only be called if there is no Id yet.
		/// Otherwise, an exception is thrown.
		/// </summary>
		/// <param name="id">Valid id for current repository.</param>		
		void AssignId(object id);

		/// <summary>
		/// Loads the fields of the entity.
		/// </summary>
		/// <param name="reader">Repository reader to read from</param>
		/// <param name="version">Version of repository data</param>
		void LoadFields(IRepositoryReader reader, int version);

		/// <summary>
		/// Loads the inner objects of the given property.
		/// </summary>
		/// <param name="propertyName">Property of inner objects</param>
		/// <param name="reader">Repository reader to read from</param>
		/// <param name="version">Version of repository data</param>
		void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version);

		/// <summary>
		/// Saves the fields of the entity.
		/// </summary>
		/// <param name="writer">Repository writer to write to</param>
		/// <param name="version">Version of repository data to be written</param>
		void SaveFields(IRepositoryWriter writer, int version);

		/// <summary>
		/// Saves the inner objects of the given property.
		/// </summary>
		/// <param name="propertyName">Property of the inner objects</param>
		/// <param name="writer">Repository writer to write to</param>
		/// <param name="version">Version of repository data to be written</param>
		void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version);

		/// <summary>
		/// Deletes the entity from the repository.
		/// </summary>
		/// <param name="writer">Repository writer to use for the deletion</param>
		/// <param name="version">Version of repository data</param>
		void Delete(IRepositoryWriter writer, int version);
	}


	/// <summary>
	/// Describes a property of an entity.
	/// </summary>
	/// <status>reviewed</status>
	public abstract class EntityPropertyDefinition {

		protected EntityPropertyDefinition(string name) {
			this.name = name;
		}


		/// <summary>
		/// Indicates the name of the property.
		/// </summary>
		public string Name {
			get { return name; }
		}


		/// <summary>
		/// Specfies name of the property as used in the repository.
		/// </summary>
		/// <remarks>This is a convenience property for the cache. Applications 
		/// should never access it.</remarks>
		public string ElementName {
			get { return elementName; }
			internal set { elementName = value; }
		}
		

		#region Fields

		private string name;

		private string elementName;

		#endregion
	}

	
	/// <summary>
	/// Describes a single valued property.
	/// </summary>
	/// <status>reviewed</status>
	public class EntityFieldDefinition : EntityPropertyDefinition {

		public EntityFieldDefinition(string name, Type type)
			: base(name) {
			if (type == null) throw new ArgumentNullException("type");
			this.type = type;
		}


		public Type Type {
			get { return type; }
		}


		#region Fields
		private Type type;
		#endregion
	}


	/// <summary>
	/// Describes a property which holds a collection of inner objects.
	/// </summary>
	/// <status>reviewed</status>
	public class EntityInnerObjectsDefinition : EntityPropertyDefinition {

		public EntityInnerObjectsDefinition(string name, string entityTypeName, string[] innerFieldNames, Type[] innerFieldTypes)
			: base(name) {
			if (innerFieldNames.Length != innerFieldTypes.Length)
				throw new NShapeException("Numeric of field names does not match number of field types.");
			this.entityTypeName = entityTypeName;
			this.fieldInfos = new EntityFieldDefinition[innerFieldNames.Length];
			for (int i = 0; i < innerFieldNames.Length; ++i)
				fieldInfos[i] = new EntityFieldDefinition(innerFieldNames[i], innerFieldTypes[i]);
		}


		/// <summary>
		/// Retrieves the entity type name of the inner objects.
		/// </summary>
		public string EntityTypeName {
			get { return entityTypeName; }
		}


		/// <summary>
		/// Retrieves the property definitions of the inner objects.
		/// </summary>
		public IEnumerable<EntityPropertyDefinition> PropertyDefinitions {
			get { return fieldInfos; }
		}


		#region Fields

		private string entityTypeName;
		private EntityFieldDefinition[] fieldInfos;

		#endregion
	}


	/// <summary>
	/// Describes the kind of entity.
	/// </summary>
	/// <status>reviewed</status>
	public enum EntityCategory { 
		/// <summary>Project settings</summary>
		ProjectSettings, 
		/// <summary>Diagram</summary>
		Diagram, 
		/// <summary>Shape</summary>
		Shape, 
		/// <summary>Template</summary>
		Template,
		/// <summary>Model</summary>
		Model,
		/// <summary>Model object</summary>
		ModelObject,
		/// <summary>Model object</summary>
		ModelMapping,
		/// <summary>Design</summary>
		Design, 
		/// <summary>Style</summary>
		Style 
	}


	/// <summary>
	/// Describes an entity in the conceptual data model.
	/// </summary>
	/// <remarks>An entity type is used to map the object model to the conceptual 
	/// data model. Since the XML document represents that conceptual model, the 
	/// entity types define the schema of the XML document.</remarks>
	/// <status>reviewed</status>
	public interface IEntityType {

		/// <summary>
		/// Full name of the entity type including the namespace.
		/// </summary>
		string FullName { get; }

		/// <summary>
		/// Name of the entity type used in XML.
		/// </summary>
		string ElementName { get; set; }

		/// <summary>
		/// Indicates the category of the entity.
		/// </summary>
		EntityCategory Category { get; }

		/// <summary>
		/// Indicates the repository version to be used with the entity type.
		/// </summary>
		int RepositoryVersion { get; }
		
		/// <summary>
		/// Creates an empty instance of this entity for loading.
		/// </summary>
		/// <returns></returns>
		IEntity CreateInstanceForLoading();

		/// <summary>
		/// Lists all property infos of this entity type.
		/// </summary>
		IEnumerable<EntityPropertyDefinition> PropertyDefinitions { get; }

		/// <summary>
		/// Indicates whether this entity type contains inner objects.
		/// </summary>
		bool HasInnerObjects { get; }
	}


	/// <summary>
	/// Represents a method that creates an entity.
	/// </summary>
	/// <returns></returns>
	public delegate IEntity CreateInstanceDelegate();
	

	/// <summary>
	/// Represents a method that retrieves the property definitions of an entity.
	/// </summary>
	/// <param name="version">Repository version to which to refer</param>
	/// <status>reviewed</status>
	public delegate IEnumerable<EntityPropertyDefinition> GetPropertyDefinitionsDelegate(int version);


	/// <summary>
	/// Describes the entities of one sort.
	/// </summary>
	public class EntityType : IEntityType {

		/// <summary>
		/// Constructs an entity type.
		/// </summary>
		/// <param name="entityTypeName">Name of the entity type</param>
		/// <param name="category">Category of the entity type</param>
		/// <param name="version">Storage format version to use with this entity type</param>
		/// <param name="createInstanceDelegate">Method to call to create an instance of the type</param>
		/// <param name="propertyInfos">Properties of the entities</param>
		public EntityType(string entityTypeName, EntityCategory category, int version, 
			CreateInstanceDelegate createInstanceDelegate, IEnumerable<EntityPropertyDefinition> propertyDefinitions) {
			if (entityTypeName == null) throw new ArgumentNullException("entityTypeName");
			if (createInstanceDelegate == null) throw new ArgumentNullException("createInstanceDelegate");
			if (propertyDefinitions == null) throw new ArgumentNullException("propertyDefinitions");
			//
			this.name = entityTypeName;
			this.category = category;
			this.repositoryVersion = version;
			this.createInstanceDelegate = createInstanceDelegate;
			this.propertyDefinitions = new List<EntityPropertyDefinition>(propertyDefinitions);
		}

		#region IEntityType Members

		/// <override></óverride>
		public string FullName {
			get { return name; }
		}


		/// <override></override>
		public string ElementName {
			get { return elementName; }
			set { elementName = value; }
		}


		/// <override></override>
		public EntityCategory Category {
			get { return category; }
		}


		/// <override></override>
		public int RepositoryVersion { 
			get { return repositoryVersion; } 
		}


		/// <override></override>
		public IEntity CreateInstanceForLoading() {
			return createInstanceDelegate();
		}


		/// <override></override>
		public IEnumerable<EntityPropertyDefinition> PropertyDefinitions {
			get { return propertyDefinitions; }
		}


		/// <override></override>
		public bool HasInnerObjects {
			get {
				bool result = false;
				foreach (EntityPropertyDefinition pi in propertyDefinitions)
					if (pi is EntityInnerObjectsDefinition) {
						result = true;
						break;
					}
				return result;
			}
		}

		#endregion


		#region Fields

		private string name;
		private string elementName;
		private EntityCategory category;
		private int repositoryVersion;
		private CreateInstanceDelegate createInstanceDelegate;
		private List<EntityPropertyDefinition> propertyDefinitions;

		#endregion
	}


	/// <summary>
	/// Writes entities into a cache.
	/// </summary>
	/// <status>reviewed</status>
	public interface IRepositoryWriter {

		void WriteBool(bool value);

		void WriteByte(byte value);

		void WriteInt16(short value);

		void WriteInt32(int value);

		void WriteInt64(long value);

		void WriteFloat(float value);

		void WriteDouble(double value);

		void WriteChar(char value);

		void WriteString(string value);

		void WriteDate(DateTime date);

		void WriteImage(Image image);

		void WriteTemplate(Template template);

		void WriteStyle(IStyle style);

		void WriteModelObject(IModelObject modelObject);

		/// <summary>
		/// Starts writing the next set of inner objects through an additional cache 
		/// repositoryWriter.
		/// </summary>
		void BeginWriteInnerObjects();

		/// <summary>
		/// Terminates writing the current type of inner objects.
		/// </summary>
		void EndWriteInnerObjects();

		/// <summary>
		/// Starts writing an inner object to the cache.
		/// </summary>
		void BeginWriteInnerObject();

		/// <summary>
		/// Commits the current inner object to the data store and prepares the inner
		/// repositoryWriter for the next inner object.
		/// </summary>
		void EndWriteInnerObject();

		void DeleteInnerObjects();

	}


	/// <summary>
	/// Reads objects from a cache.
	/// </summary>
	/// <status>reviewed</status>
	public interface IRepositoryReader {

		// -- Methods for reading fields --

		bool ReadBool();

		byte ReadByte();

		short ReadInt16();

		int ReadInt32();

		long ReadInt64();

		float ReadFloat();

		double ReadDouble();

		char ReadChar();

		string ReadString();

		DateTime ReadDate();

		Template ReadTemplate();

		Shape ReadShape();

		IModelObject ReadModelObject();

		Image ReadImage();

		IColorStyle ReadColorStyle();

		ICapStyle ReadCapStyle();

		IFillStyle ReadFillStyle();

		ICharacterStyle ReadCharacterStyle();

		ILineStyle ReadLineStyle();

		//IShapeStyle ReadShapeStyle();

		IParagraphStyle ReadParagraphStyle();


		// -- Methods for reading inner objects --

		/// <summary>
		/// Fetches the next set of inner objects and prepares them for reading.
		/// </summary>
		/// <returns></returns>
		void BeginReadInnerObjects();

		/// <summary>
		/// Finishes reading the current set of inner objects.
		/// </summary>
		void EndReadInnerObjects();

		/// <summary>
		/// Fetches the next inner object in a set of inner object.
		/// </summary>
		/// <returns></returns>
		bool BeginReadInnerObject();

		/// <summary>
		/// Finishes reading an inner object.
		/// </summary>
		void EndReadInnerObject();
	}
}
