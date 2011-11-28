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
using System.Diagnostics;
using System.Drawing;


namespace Dataweb.NShape.Advanced {

	#region Property Mappings

	/// <summary>
	/// Indicates that a property can be used for model-to-shape property mappings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=true, Inherited=true)]
	public class PropertyMappingIdAttribute : Attribute {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.PropertyMappingIdAttribute" />.
		/// </summary>
		public PropertyMappingIdAttribute(int id) {
			this.id = id;
		}

		/// <summary>
		/// The id value used for identifying the property.
		/// </summary>
		public int Id { get { return id; } }

		private int id;
	}


	/// <summary>
	/// Defines the mapping of shape properties to model properties.
	/// </summary>
	public interface IModelMapping: IEntity {

		/// <summary>
		/// The property of a <see cref="T:Dataweb.NShape.Advanced.Shape" /> with the specified <see cref="T:Dataweb.NShape.Advanced.PropertyMappingIdAttribute" /> value.
		/// </summary>
		int ShapePropertyId { get; }

		/// <summary>
		/// The property of a <see cref="T:Dataweb.NShape.Advanced.IModelObject" /> with the specified <see cref="T:Dataweb.NShape.Advanced.PropertyMappingIdAttribute" /> value.
		/// </summary>
		int ModelPropertyId { get; }

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> accepts <see cref="T:System.Int32" /> values from the associated <see cref="T:Dataweb.NShape.Advanced.IModelObject" />.
		/// </summary>
		bool CanSetInteger { get; }

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> accepts <see cref="T:System.Single" /> values from the associated <see cref="T:Dataweb.NShape.Advanced.IModelObject" />.
		/// </summary>
		bool CanSetFloat { get; }

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> accepts <see cref="T:System.String" /> values from the associated <see cref="T:Dataweb.NShape.Advanced.IModelObject" />.
		/// </summary>
		bool CanSetString { get; }

		/// <summary>
		/// Sets a <see cref="T:System.Int32" /> value provided by the <see cref="T:Dataweb.NShape.Advanced.IModelObject" />.
		/// </summary>
		void SetInteger(int value);

		/// <summary>
		/// Sets a <see cref="T:System.Single" /> value provided by the <see cref="T:Dataweb.NShape.Advanced.IModelObject" />.
		/// </summary>
		void SetFloat(float value);

		/// <summary>
		/// Sets a <see cref="T:System.String" /> value provided by the <see cref="T:Dataweb.NShape.Advanced.IModelObject" />.
		/// </summary>
		void SetString(string value);

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> can provide <see cref="T:System.Int32" /> values for the associated <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		bool CanGetInteger { get; }

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> can provide <see cref="T:System.Single" /> values for the associated <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		bool CanGetFloat { get; }

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> can provide <see cref="T:System.String" /> values for the associated <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		bool CanGetString { get; }

		/// <summary>
		/// Specifies wether the <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> can provide <see cref="T:Dataweb.NShape.IStyle" /> values for the associated <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		bool CanGetStyle { get; }

		/// <summary>
		/// Retrieves a converted <see cref="T:System.Int32" /> value for the <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		int GetInteger();

		/// <summary>
		/// Retrieves a converted <see cref="T:System.Single" /> value for the <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		float GetFloat();

		/// <summary>
		/// Retrieves a converted <see cref="T:System.String" /> value for the <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		string GetString();

		/// <summary>
		/// Retrieves a converted <see cref="T:Dataweb.NShape.IStyle" /> value for the <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		IStyle GetStyle();

	}


	/// <summary>
	/// Provides a base class for <see cref="T:Dataweb.NShape.Advanced.ModelMappingBase" /> implementations.
	/// </summary>
	public abstract class ModelMappingBase : IModelMapping {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ModelMappingBase" />.
		/// </summary>
		protected ModelMappingBase(int modelPropertyId, int shapePropertyId) {
			this.shapePropertyId = shapePropertyId;
			this.modelPropertyId = modelPropertyId;
		}


		/// <summary>
		/// Creates an uninitialized instance of <see cref="T:Dataweb.NShape.Advanced.ModelMappingBase" /> used for loading from an <see cref="T:Dataweb.NShape.Advanced.IRepository" />.
		/// </summary>
		protected ModelMappingBase() {
		}


		#region IModelMapping Members

		/// <override></override>
		public int ShapePropertyId {
			get { return shapePropertyId; }
		}


		/// <override></override>
		public int ModelPropertyId {
			get { return modelPropertyId; }
		}


		/// <override></override>
		public abstract void SetInteger(int value);

		/// <override></override>
		public abstract void SetFloat(float value);

		/// <override></override>
		public abstract void SetString(string value);

		/// <override></override>
		public abstract int GetInteger();

		/// <override></override>
		public abstract float GetFloat();

		/// <override></override>
		public abstract string GetString();

		/// <override></override>
		public abstract IStyle GetStyle();

		/// <override></override>
		public abstract bool CanSetInteger { get; }

		/// <override></override>
		public abstract bool CanSetFloat { get; }

		/// <override></override>
		public abstract bool CanSetString { get; }

		/// <override></override>
		public abstract bool CanGetInteger { get; }

		/// <override></override>
		public abstract bool CanGetFloat { get; }

		/// <override></override>
		public abstract bool CanGetString { get; }

		/// <override></override>
		public abstract bool CanGetStyle { get; }

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ModelMappingBase" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("ShapePropertyId", typeof(int));
			yield return new EntityFieldDefinition("ModelPropertyId", typeof(int));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public object Id {
			get { return id; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) throw new InvalidOperationException(string.Format("{0} has already a id.", GetType().Name));
			this.id = id;

		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			shapePropertyId = reader.ReadInt32();
			modelPropertyId = reader.ReadInt32();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			//nothing to do
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteInt32(shapePropertyId);
			writer.WriteInt32(modelPropertyId);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			// nothing to do
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		// Fields
		private object id = null;
		private int shapePropertyId;
		private int modelPropertyId;
	}


	/// <summary>
	/// A <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> for mapping a numeric property of a <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> to a numeric property of a <see cref="T:Dataweb.NShape.Advanced.Shape" />.
	/// </summary>
	public class NumericModelMapping : ModelMappingBase {

		/// <summary>
		/// Specifies the mapping capabilities of the <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" />.
		/// </summary>
		public enum MappingType { 
			/// <summary>Specifies a mapping from a <see cref="T:System.Int32" /> property to a <see cref="T:System.Int32" />.</summary>
			IntegerInteger,
			/// <summary>Specifies a mapping from a <see cref="T:System.Int32" /> property to a <see cref="T:System.Single" />.</summary>
			IntegerFloat,
			/// <summary>Specifies a mapping from a <see cref="T:System.Single" /> property to a <see cref="T:System.Int32" />.</summary>
			FloatInteger,
			/// <summary>Specifies a mapping from a <see cref="T:System.Single" /> property to a <see cref="T:System.Single" />.</summary>
			FloatFloat 
		};


		/// <summary>
		/// Constructs a new <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" /> instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerFloat e.g. means model's integer property to shapes float property.
		/// </param>
		public NumericModelMapping(int shapePropertyId, int modelPropertyId, MappingType mappingType)
			: this(shapePropertyId, modelPropertyId, mappingType, 0, 1) {
		}


		/// <summary>
		/// Constructs a new <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" /> instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerFloat e.g. means model's integer property to shapes float property.
		/// </param>
		/// <param name="intercept">Defines an offset for the mapped value.</param>
		/// <param name="slope">Defines a factor for the mapped value.</param>
		public NumericModelMapping(int shapePropertyId, int modelPropertyId, MappingType mappingType, float intercept, float slope)
			: base(modelPropertyId, shapePropertyId) {
			this.mappingType = mappingType;
			this.intercept = intercept;
			this.slope = slope;
		}


		#region IModelMappping Members

		/// <override></override>
		public override bool CanGetInteger {
			get {
				return (mappingType == MappingType.FloatInteger
					|| mappingType == MappingType.IntegerInteger);
			}
		}


		/// <override></override>
		public override bool CanSetInteger {
			get {
				return (mappingType == MappingType.IntegerFloat
					|| mappingType == MappingType.IntegerInteger);
			}
		}


		/// <override></override>
		public override bool CanGetFloat {
			get {
				return (mappingType == MappingType.IntegerFloat
					|| mappingType == MappingType.FloatFloat);
			}
		}


		/// <override></override>
		public override bool CanSetFloat {
			get {
				return (mappingType == MappingType.FloatInteger
					|| mappingType == MappingType.FloatFloat);
			}
		}


		/// <override></override>
		public override bool CanGetString {
			get { return false; }
		}


		/// <override></override>
		public override bool CanSetString {
			get { return false; }
		}


		/// <override></override>
		public override bool CanGetStyle {
			get { return false; }
		}


		/// <override></override>
		public override int GetInteger() {
				if (CanGetInteger)
					checked { return (int)Math.Round(Intercept + (value * Slope)); }
				else throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetInteger(int value) {
			if (CanSetInteger) checked { this.value = value; }
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override float GetFloat() {
			if (CanGetFloat)
				checked { return (float)Math.Round(Intercept + (value * Slope), 6); }
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetFloat(float value) {
			if (CanSetFloat) checked { this.value = value; }
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override string GetString() {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetString(string value) {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override IStyle GetStyle() {
			throw new NotSupportedException();
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" />.
		/// </summary>
		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition propDef in ModelMappingBase.GetPropertyDefinitions(version))
				yield return propDef;

			yield return new EntityFieldDefinition("MappingType", typeof(int));
			yield return new EntityFieldDefinition("Intercept", typeof(float));
			yield return new EntityFieldDefinition("Slope", typeof(float));

			yield return new EntityInnerObjectsDefinition("Layers", "Core.Layer",
				new string[] { "Id", "Name", "Title", "LowerVisibilityThreshold", "UpperVisibilityThreshold" },
				new Type[] { typeof(int), typeof(string), typeof(string), typeof(int), typeof(int) });
		}


		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			mappingType = (MappingType)reader.ReadInt32();
			intercept = reader.ReadFloat();
			slope = reader.ReadFloat();
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32((int)mappingType);
			writer.WriteFloat(intercept);
			writer.WriteFloat(slope);
		}

		#endregion


		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping.MappingType" /> specifying the mapping capabilities of this <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" />.
		/// </summary>
		public MappingType Type {
			get { return mappingType; }
		}
		
		
		/// <summary>
		/// Defines a factor for the mapping.
		/// </summary>
		public float Slope {
			get { return slope; }
			set { slope = value; }
		}


		/// <summary>
		/// Defines an offset for the mapping.
		/// </summary>
		public float Intercept {
			get { return intercept; }
			set { intercept = value; }
		}


		/// <summary>
		/// Constructor for IEntity CreateInstanceDelegate: Creates an empty instance for loading from Repository
		/// </summary>
		protected internal NumericModelMapping()
			: base() {
		}


		#region Fields

		private const string entityTypeName = "NumericModelMapping";
		
		private MappingType mappingType;
		private double value = 0;
		private float slope = 1;
		private float intercept = 0;

		#endregion
	}


	/// <summary>
	/// A <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> for mapping a numeric or textual property of a <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> to a textual property of a <see cref="T:Dataweb.NShape.Advanced.Shape" />.
	/// </summary>
	public class FormatModelMapping : ModelMappingBase {

		/// <summary>
		/// Specifies the mapping capabilities of the <see cref="T:Dataweb.NShape.Advanced.FormatModelMapping" />.
		/// </summary>
		public enum MappingType {
			/// <summary>Specifies a mapping from a <see cref="T:System.Int32" /> property to a <see cref="T:System.String" /> property.</summary>
			IntegerString,
			/// <summary>Specifies a mapping from a <see cref="T:System.Float" /> property to a <see cref="T:System.String" /> property.</summary>
			FloatString,
			/// <summary>Specifies a mapping from a <see cref="T:System.String" /> property to a <see cref="T:System.String" /> property.</summary>
			StringString
		};


		/// <summary>
		/// Constructs a new FormatModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerString e.g. means model's integer property to shapes string property.
		/// </param>
		public FormatModelMapping(int shapePropertyId, int modelPropertyId, MappingType mappingType)
			: this(shapePropertyId, modelPropertyId, mappingType, "{0}") {
		}


		/// <summary>
		/// Constructs a new FormatModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerString e.g. means model's integer property to shapes string property.
		/// </param>
		/// <param name="format">The format for the mapped value.</param>
		public FormatModelMapping(int shapePropertyId, int modelPropertyId, MappingType mappingType, string format)
			: base(modelPropertyId, shapePropertyId) {
			this.format = format;
			this.mappingType = mappingType;
		}


		#region IModelMapping Members

		/// <override></override>
		public override bool CanGetInteger {
			get { return false; }
		}


		/// <override></override>
		public override bool CanSetInteger {
			get { return (mappingType == MappingType.IntegerString); }
		}


		/// <override></override>
		public override bool CanGetFloat {
			get { return false; }
		}


		/// <override></override>
		public override bool CanSetFloat {
			get { return (mappingType == MappingType.FloatString); }
		}


		/// <override></override>
		public override bool CanGetString {
			get { return true; }
		}


		/// <override></override>
		public override bool CanSetString {
			get { return (mappingType == MappingType.StringString); }
		}


		/// <override></override>
		public override bool CanGetStyle {
			get { return false; }
		}


		/// <override></override>
		public override int GetInteger() {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetInteger(int value) {
			if (CanSetInteger) intValue = value;
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override float GetFloat() {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetFloat(float value) {
			if (CanSetFloat) floatValue = value;
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override string GetString() {
			try {
				switch (mappingType) {
					case MappingType.FloatString:
						return string.Format(Format, floatValue);
					case MappingType.IntegerString:
						return string.Format(Format, intValue);
					case MappingType.StringString:
						return string.Format(Format, stringValue);
					default: throw new NShapeUnsupportedValueException(mappingType);
				}
			} catch (FormatException exc) {
				return exc.Message + " " + Format;
			}
		}


		/// <override></override>
		public override void SetString(string value) {
			if (CanSetString) stringValue = value;
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override IStyle GetStyle() {
			throw new NotSupportedException();
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.Advanced.FormatModelMapping" />.
		/// </summary>
		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.FormatModelMapping" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition propDef in ModelMappingBase.GetPropertyDefinitions(version))
				yield return propDef;

			yield return new EntityFieldDefinition("MappingType", typeof(int));
			yield return new EntityFieldDefinition("format", typeof(string));
		}


		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			mappingType = (MappingType)reader.ReadInt32();
			format = reader.ReadString();
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32((int)mappingType);
			writer.WriteString(format);
		}

		#endregion


		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Advanced.FormatModelMapping.MappingType" /> specifying the mapping capabilities of this <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" />.
		/// </summary>
		public MappingType Type {
			get { return mappingType; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string Format {
			get { return format; }
			set {
				if (value == null) throw new ArgumentNullException("Format");
				if (value == string.Empty) throw new ArgumentException("Format");
				format = value;
			}
		}


		/// <summary>
		/// Constructor for IEntity CreateInstanceDelegate: Creates an empty instance for loading from Repository
		/// </summary>
		protected internal FormatModelMapping()
			: base() {
		}


		#region Fields

		private const string entityTypeName = "FormatModelMapping";

		private MappingType mappingType;
		private string format;
		private int intValue;
		private float floatValue;
		private string stringValue;

		#endregion
	}


	/// <summary>
	/// A <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> for mapping a numeric property of a <see cref="T:Dataweb.NShape.Advanced.IModelMapping" /> to a <see cref="T:Dataweb.NShape.IStyle" /> property of a <see cref="T:Dataweb.NShape.Advanced.Shape" />.
	/// </summary>
	public class StyleModelMapping : ModelMappingBase {

		/// <summary>
		/// Specifies the mapping capabilities of the <see cref="T:Dataweb.NShape.Advanced.FormatModelMapping" />.
		/// </summary>
		public enum MappingType {
			/// <summary>Specifies a mapping from a <see cref="T:System.Int32" /> property to a <see cref="T:Dataweb.NShape.IStyle" /> property.</summary>
			IntegerStyle,
			/// <summary>Specifies a mapping from a <see cref="T:System.Single" /> property to a <see cref="T:Dataweb.NShape.IStyle" /> property.</summary>
			FloatStyle
		};


		/// <summary>
		/// Constructs a new StyleModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerStyle e.g. means model's integer property to shapes style property.
		/// </param>
		public StyleModelMapping(int shapePropertyId, int modelPropertyId, MappingType mappingType)
			: base(modelPropertyId, shapePropertyId) {
			this.mappingType = mappingType;
			if (mappingType == MappingType.IntegerStyle)
				intRanges = new SortedList<int, IStyle>();
			else floatRanges = new SortedList<float, IStyle>();
		}


		/// <summary>
		/// Constructs a new StyleModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerStyle e.g. means model's integer property to shapes style property.
		/// </param>
		/// <param name="style">Specifies the style that is used for all values outside the user defined ranges.</param>
		public StyleModelMapping(int shapePropertyId, int modelPropertyId, MappingType mappingType, IStyle style)
			: this(shapePropertyId, modelPropertyId, mappingType) {
			defaultStyle = style;
		}


		/// <summary>
		/// Constructor for IEntity CreateInstanceDelegate: Creates an empty instance for loading from Repository
		/// </summary>
		internal StyleModelMapping()
			: base() {
		}


		#region IModelMapping Members

		/// <override></override>
		public override bool CanGetInteger {
			get { return false; }
		}


		/// <override></override>
		public override bool CanSetInteger {
			get { return (mappingType == MappingType.IntegerStyle); }
		}


		/// <override></override>
		public override bool CanGetFloat {
			get { return false; }
		}


		/// <override></override>
		public override bool CanSetFloat {
			get { return (mappingType == MappingType.FloatStyle); }
		}


		/// <override></override>
		public override bool CanGetString {
			get { return false; }
		}


		/// <override></override>
		public override bool CanSetString {
			get { return false; }
		}


		/// <override></override>
		public override bool CanGetStyle {
			get { return true; }
		}


		/// <override></override>
		public override int GetInteger() {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetInteger(int value) {
			if (CanSetInteger) intValue = value;
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override float GetFloat() {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetFloat(float value) {
			if (CanSetFloat) floatValue = value;
			else throw new NotSupportedException();
		}


		/// <override></override>
		public override string GetString() {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override void SetString(string value) {
			throw new NotSupportedException();
		}


		/// <override></override>
		public override IStyle GetStyle() {
			IStyle result;
			if (mappingType == MappingType.IntegerStyle) {
				result = defaultStyle;
				int fromValue = int.MinValue;
				foreach (KeyValuePair<int, IStyle> rangeItem in intRanges) {
					if (fromValue == int.MinValue && intValue < rangeItem.Key)
						break;
					else if (fromValue <= intValue && intValue < rangeItem.Key) {
						result = intRanges[fromValue];
						break;
					} else result = rangeItem.Value;
					fromValue = rangeItem.Key;
				}
			} else if (mappingType == MappingType.FloatStyle) {
				result = defaultStyle;
				if (float.IsNaN(floatValue)) 
					return result;
				//if (float.IsNegativeInfinity(floatValue))
				//   return floatRanges.ContainsKey(floatValue) ? floatRanges[floatValue] : result;
				float fromValue = float.NegativeInfinity;
				foreach (KeyValuePair<float, IStyle> rangeItem in floatRanges) {
					if (fromValue == float.NegativeInfinity && floatValue < rangeItem.Key)
						break;
					else if (fromValue <= floatValue && floatValue < rangeItem.Key) {
						result = floatRanges[fromValue];
						break;
					} else result = rangeItem.Value;
					fromValue = rangeItem.Key;
				}
			} else throw new NotSupportedException();
			return result;
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.StyleModelMapping" />.
		/// </summary>
		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.StyleModelMapping" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition propDef in ModelMappingBase.GetPropertyDefinitions(version))
				yield return propDef;

			yield return new EntityFieldDefinition("MappingType", typeof(int));
			yield return new EntityFieldDefinition("DefaultStyleType", typeof(int));
			yield return new EntityFieldDefinition("DefaultStyle", typeof(object));
			
			yield return new EntityInnerObjectsDefinition("ValueRanges", "Core.Range",
				new string[] { "Value", "StyleType", "Style" },
				new Type[] { typeof(float), typeof(int), typeof(object) });
		}


		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			mappingType = (MappingType)reader.ReadInt32();
			if (mappingType == MappingType.IntegerStyle)
				intRanges = new SortedList<int, IStyle>();
			else floatRanges = new SortedList<float, IStyle>();
			defaultStyle = ReadStyle(reader);
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32((int)mappingType);
			WriteStyle(writer, defaultStyle);
		}


		/// <override></override>
		public override void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			base.LoadInnerObjects(propertyName, reader, version);
			Debug.Assert(propertyName == "ValueRanges");
			Debug.Assert((intRanges != null && intRanges.Count == 0)
				|| (floatRanges != null && floatRanges.Count == 0));
			reader.BeginReadInnerObjects();
			while (reader.BeginReadInnerObject()) {
				IStyle style = null;
				switch (mappingType) {
					case MappingType.IntegerStyle:
						int intValue = (int)reader.ReadFloat();
						style = ReadStyle(reader);
						intRanges.Add(intValue, style);
						break;
					case MappingType.FloatStyle:
						float floatValue = reader.ReadFloat();
						style = ReadStyle(reader);
						floatRanges.Add(floatValue, style);
						break;
					default: throw new NShapeUnsupportedValueException(mappingType);
				}
				reader.EndReadInnerObject();
			}
			reader.EndReadInnerObjects();
		}


		/// <override></override>
		public override void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			base.SaveInnerObjects(propertyName, writer, version);
			Debug.Assert(propertyName == "ValueRanges");
			writer.BeginWriteInnerObjects();
			switch (mappingType) {
				case MappingType.IntegerStyle:
					foreach (KeyValuePair<int, IStyle> range in intRanges) {
						writer.BeginWriteInnerObject();
						writer.WriteFloat(range.Key);
						WriteStyle(writer, range.Value);
						writer.EndWriteInnerObject();
					}
					break;
				case MappingType.FloatStyle:
					foreach (KeyValuePair<float, IStyle> range in floatRanges) {
						writer.BeginWriteInnerObject();
						writer.WriteFloat(range.Key);
						WriteStyle(writer, range.Value);
						writer.EndWriteInnerObject();
					}
					break;
				default: throw new NShapeUnsupportedValueException(mappingType);
			}
			writer.EndWriteInnerObjects();
		}

		#endregion


		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Advanced.StyleModelMapping.MappingType" /> specifying the mapping capabilities of this <see cref="T:Dataweb.NShape.Advanced.NumericModelMapping" />.
		/// </summary>
		public MappingType Type {
			get { return mappingType; }
		}

	
		/// <summary>
		/// Get the number of range definitions.
		/// </summary>
		public int ValueRangeCount {
			get {
				if (mappingType == MappingType.IntegerStyle)
					return intRanges.Count;
				else if (mappingType == MappingType.FloatStyle)
					return floatRanges.Count;
				else throw new NotSupportedException();
			}
		}


		/// <summary>
		/// Get the defined value ranges.
		/// </summary>
		public IEnumerable<object> ValueRanges {
			get {
				if (mappingType == MappingType.IntegerStyle) {
					foreach (KeyValuePair<int, IStyle> range in intRanges)
						yield return range.Key;
				} else if (mappingType == MappingType.FloatStyle) {
					foreach (KeyValuePair<float, IStyle> range in floatRanges)
						yield return range.Key;
				} else throw new NotSupportedException();
			}
		}


		/// <summary>
		/// Gets the <see cref="T:Dataweb.NShape.IStyle" /> associated with the given range key.
		/// </summary>
		public IStyle this[int key] {
			get {
				if (mappingType == MappingType.IntegerStyle)
					return intRanges[key];
				else throw new NotSupportedException();
			}
		}


		/// <summary>
		/// Gets the <see cref="T:Dataweb.NShape.IStyle" /> associated with the given range key.
		/// </summary>
		public IStyle this[float key] {
			get {
				if (mappingType == MappingType.FloatStyle)
					return floatRanges[key];
				else throw new NotSupportedException();
			}
		}


		/// <summary>
		/// Deletes the current value ranges.
		/// </summary>
		public void ClearValueRanges() {
			if (mappingType == MappingType.IntegerStyle) {
				intRanges.Clear();
			} else if (mappingType == MappingType.FloatStyle) {
				floatRanges.Clear();
			} else throw new NotSupportedException();
		}


		/// <summary>
		/// Adds a new value range.
		/// </summary>
		/// <param name="lowerValue">The lower bound of the value range.</param>
		/// <param name="style">The <see cref="T:Dataweb.NShape.IStyle" /> representing the range of values.</param>
		public void AddValueRange(int lowerValue, IStyle style) {
			if (mappingType == MappingType.IntegerStyle)
				intRanges.Add(lowerValue, style);
			else throw new NotSupportedException();
		}


		/// <summary>
		/// Adds a new value range.
		/// </summary>
		/// <param name="lowerValue">The upper bound of the value range.</param>
		/// <param name="style">The <see cref="T:Dataweb.NShape.IStyle" /> representing the range of values.</param>
		public void AddValueRange(float lowerValue, IStyle style) {
			if (mappingType == MappingType.FloatStyle)
				floatRanges.Add(lowerValue, style);
			else throw new NotSupportedException();
		}


		/// <summary>
		/// Removes the value range assaciated with the given key.
		/// </summary>
		public bool RemoveValueRange(int value) {
			if (mappingType == MappingType.IntegerStyle) 
				return intRanges.Remove(value);
			else throw new NotSupportedException();
		}


		/// <summary>
		/// Removes the value range assaciated with the given key.
		/// </summary>
		public bool RemoveValueRange(float value) {
			if (mappingType == MappingType.FloatStyle)
				return floatRanges.Remove(value);
			else throw new NotSupportedException();
		}


		private IStyle ReadStyle(IRepositoryReader reader) {
			IStyle result;
			MappedStyleType mappedStyleType = (MappedStyleType)reader.ReadInt32();
			switch (mappedStyleType) {
				case MappedStyleType.CapStyle:
					result = reader.ReadCapStyle(); break;
				case MappedStyleType.CharacterStyle:
					result = reader.ReadCharacterStyle(); break;
				case MappedStyleType.ColorStyle:
					result = reader.ReadColorStyle(); break;
				case MappedStyleType.FillStyle:
					result = reader.ReadFillStyle(); break;
				case MappedStyleType.LineStyle:
					result = reader.ReadLineStyle(); break;
				case MappedStyleType.ParagraphStyle:
					result = reader.ReadParagraphStyle(); break;
				case MappedStyleType.Unassigned:
					// Skip value - it does not matter what we read here
					reader.ReadColorStyle();	// ToDo: Find a better solution for skipping an object id
					result = null;
					break;
				default: throw new NShapeUnsupportedValueException(mappedStyleType);
			}
			return result;
		}


		private void WriteStyle(IRepositoryWriter writer, IStyle style) {
			writer.WriteInt32((int)GetMappedStyleType(style));
			writer.WriteStyle(style);
		}
		
		
		private MappedStyleType GetMappedStyleType(IStyle style) {
			if (style == null) return MappedStyleType.Unassigned;
			if (style is ICapStyle) return MappedStyleType.CapStyle;
			else if (style is ICharacterStyle) return MappedStyleType.CharacterStyle;
			else if (style is IColorStyle) return MappedStyleType.ColorStyle;
			else if (style is IFillStyle) return MappedStyleType.FillStyle;
			else if (style is ILineStyle) return MappedStyleType.LineStyle;
			else if (style is IParagraphStyle) return MappedStyleType.ParagraphStyle;
			else throw new NShapeUnsupportedValueException(style);
		}
		
		
		private enum MappedStyleType { 
			Unassigned, 
			CapStyle, 
			CharacterStyle, 
			ColorStyle, 
			FillStyle, 
			LineStyle, 
			ParagraphStyle
		}


		#region Fields

		private const string entityTypeName = "StyleModelMapping";

		private MappingType mappingType;
		private int intValue;
		private float floatValue;
		private IStyle defaultStyle = null;
		private SortedList<int, IStyle> intRanges = null;
		private SortedList<float, IStyle> floatRanges = null;

		#endregion
	}

	#endregion


	/// <summary>
	/// Combines a shape and a model object to form a sample for shape creation.
	/// </summary>
	public class Template : IEntity {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Template" />.
		/// </summary>
		public Template(string name, Shape shape) {
			if (name == null) throw new ArgumentNullException("name");
			if (shape == null) throw new ArgumentNullException("shape");
			this.name = name;
			this.shape = shape;
		}


		/// <override></override>
		public override string ToString() {
			return Title;
		}


		/// <summary>
		/// Creates a new <see cref="T:Dataweb.NShape.Advanced.Template" /> that is a copy of the current instance.
		/// </summary>
		public Template Clone() {
			Template result = new Template();
			result.CopyFrom(this);
			return result;
		}


		/// <summary>
		/// Copies all properties and fields from the source template to this template.
		/// The shape and the model obejcts will be cloned.
		/// </summary>
		public void CopyFrom(Template source) {
			if (source == null) throw new ArgumentNullException("source");

			this.name = source.name;
			this.title = source.title;
			this.description = source.description;
			
			// Clone or copy shape
			if (this.shape == null)	this.shape = ShapeDuplicator.CloneShapeAndModelObject(source.shape);
			else this.shape.CopyFrom(source.shape);	// Template will be copied although this is not desirable

			// copy connection point mapping
			this.connectionPointMappings.Clear();
			foreach (KeyValuePair<ControlPointId, TerminalId> item in source.connectionPointMappings)
				this.connectionPointMappings.Add(item.Key, item.Value);

			// copy property mapping
			this.propertyMappings.Clear();
			foreach (KeyValuePair<int, IModelMapping>item in source.propertyMappings)
				this.propertyMappings.Add(item.Key, item.Value);
		}


		/// <summary>
		/// Gets or sets an object that provides additional data.
		/// </summary>
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}
		
		
		/// <summary>
		/// Specifies the culture independent name.
		/// </summary>
		public string Name {
			get { return name; }
			set { name = value; }
		}


		/// <summary>
		/// Specifies the culture dependent display name.
		/// </summary>
		public string Title {
			get { return string.IsNullOrEmpty(title) ? name : title; }
			set {
				if (value == name || string.IsNullOrEmpty(value))
					title = null;
				else title = value;
			}
		}


		/// <summary>
		/// A descriptive text for the template.
		/// </summary>
		public string Description {
			get { return description; }
			set { description = value; }
		}


		/// <summary>
		/// Defines the shape for this template. If the template contains a ModelObject, it will also become the shape's ModelObject.
		/// </summary>
		/// <remarks>Replacing the shape of a template with templated shapes results in 
		/// errors, if the templated shapes are not updated accordingly.</remarks>
		public Shape Shape {
			get { return shape; }
			set {
				if (shape != null && shape.ModelObject != null
					&& value != null && value.ModelObject != null) {
					// if both shapes have ModelObejct instances assigned, 
					// try to keep as many mappings as possible
					// ToDo: try to copy property mappings
					CopyTerminalMappings(shape.ModelObject, value.ModelObject);
				} else {
					// delete all mappings to restore default behavior
					UnmapAllProperties();
					UnmapAllTerminals();
				}
				shape = value;
			}
		}


		/// <summary>
		/// Creates a new shape from this template.
		/// </summary>
		/// <returns></returns>
		public Shape CreateShape() {
			Shape result = shape.Type.CreateInstance(this);
			if (shape.ModelObject != null)
				ShapeDuplicator.CloneModelObjectOnly(result);
			return result;
		}


		/// <summary>
		/// Creates a thumbnail of the template shape.
		/// </summary>
		/// <param name="size">Size of tumbnail in pixels</param>
		/// <param name="margin">Size of margin around shape in pixels</param>
		public Image CreateThumbnail(int size, int margin) {
			return CreateThumbnail(size, margin, Color.White);
		}


		/// <summary>
		/// Creates a thumbnail of the template shape.
		/// </summary>
		/// <param name="size">Size of tumbnail in pixels</param>
		/// <param name="margin">Size of margin around shape in pixels</param>
		/// <param name="transparentColor">Specifies a color that will be rendered transparent.</param>
		public Image CreateThumbnail(int size, int margin, Color transparentColor) {
			Image bmp = new Bitmap(size, size);
			using (Shape shapeClone = Shape.Clone())
				shapeClone.DrawThumbnail(bmp, margin, transparentColor);
			return bmp;
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		#region Visualization Mapping

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<IModelMapping> GetPropertyMappings() {
			return propertyMappings.Values;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IModelMapping GetPropertyMapping(int modelPropertyId) {
			IModelMapping result = null;
			propertyMappings.TryGetValue(modelPropertyId, out result);
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void MapProperties(IModelMapping propertyMapping) {
			if (propertyMapping == null) throw new ArgumentNullException("propertyMapping");
			if (propertyMappings.ContainsKey(propertyMapping.ModelPropertyId))
				propertyMappings[propertyMapping.ModelPropertyId] = propertyMapping;
			else
				propertyMappings.Add(propertyMapping.ModelPropertyId, propertyMapping);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void UnmapProperties(IModelMapping propertyMapping) {
			if (propertyMapping == null) throw new ArgumentNullException("propertyMapping");
			propertyMappings.Remove(propertyMapping.ModelPropertyId);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void UnmapAllProperties() {
			propertyMappings.Clear();
		}

		#endregion


		#region Terminal Mapping

		/// <ToBeCompleted></ToBeCompleted>
		public TerminalId GetMappedTerminalId(ControlPointId connectionPointId) {
			// If there is a mapping, return it.
			TerminalId result;
			if (connectionPointMappings.TryGetValue(connectionPointId, out result))
				return result;
			else {
				// if there is no mapping, return default values:
				if (shape != null) {
					// - If the given point is no connectionPoint
					if (!shape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Connect | ControlPointCapabilities.Glue))
						return TerminalId.Invalid;
					// - If a shape is set but no ModelObject, all connectionPoints are activated by default
					else if (shape.ModelObject == null) return TerminalId.Generic;
					else return TerminalId.Invalid;
				} else return TerminalId.Invalid;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string GetMappedTerminalName(ControlPointId connectionPointId) {
			TerminalId terminalId = GetMappedTerminalId(connectionPointId);
			if (terminalId == TerminalId.Invalid)
				return null;
			else {
				if (shape.ModelObject != null)
					return shape.ModelObject.Type.GetTerminalName(terminalId);
				else return activatedTag;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void MapTerminal(TerminalId terminalId, ControlPointId connectionPointId) {
			// check if terminalId and connectionPointId are valid values
			if (shape == null)
				throw new InvalidOperationException("Template has no shape.");
			if (!shape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Glue | ControlPointCapabilities.Connect))
				throw new NShapeException("Control point {0} is not a valid connection point.", connectionPointId);
			//
			if (connectionPointMappings.ContainsKey(connectionPointId))
				connectionPointMappings[connectionPointId] = terminalId;
			else
				connectionPointMappings.Add(connectionPointId, terminalId);
		}


		/// <summary>
		/// Clears all mappings between the shape's connection points and the model's terminals.
		/// </summary>
		public void UnmapAllTerminals() {
			connectionPointMappings.Clear();
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.Advanced.Template" />.
		/// </summary>
		public static string EntityTypeName { get { return entityTypeName; } }


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.Template" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			if (version > 2) yield return new EntityFieldDefinition("Title", typeof(string));
			yield return new EntityFieldDefinition("Description", typeof(string));
			yield return new EntityInnerObjectsDefinition(connectionPtMappingName + "s", connectionPtMappingName, connectionPtMappingAttrNames, connectionPtMappingAttrTypes);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public object Id {
			get { return id; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) throw new InvalidOperationException("Template has already an id.");
			this.id = id;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteString(name);
			if (version > 2) writer.WriteString(title);
			writer.WriteString(description);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			name = reader.ReadString();
			if (version > 2) title = reader.ReadString();
			description = reader.ReadString();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			if (propertyName == "ConnectionPointMappings") {
				// Save ConnectionPoint mappings
				writer.BeginWriteInnerObjects();
				foreach (ControlPointId pointId in Shape.GetControlPointIds(ControlPointCapabilities.Connect)) {
					TerminalId terminalId = GetMappedTerminalId(pointId);
					writer.BeginWriteInnerObject();
					writer.WriteInt32(pointId);
					writer.WriteInt32((int)terminalId);
					writer.EndWriteInnerObject();
				}
				writer.EndWriteInnerObjects();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			if (propertyName == "ConnectionPointMappings") {
				// load ConnectionPoint mappings			
				reader.BeginReadInnerObjects();
				while (reader.BeginReadInnerObject()) {
					ControlPointId connectionPointId = reader.ReadInt32();
					TerminalId terminalId = reader.ReadInt32();
					// The following is the essence of MapTerminal without the checks.
					if (connectionPointMappings.ContainsKey(connectionPointId))
						connectionPointMappings[connectionPointId] = terminalId;
					else
						connectionPointMappings.Add(connectionPointId, terminalId);
					reader.EndReadInnerObject();
				}
				reader.EndReadInnerObjects();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		// Used to create templates for loading.
		internal Template() {
		}


		private int CountControlPoints(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			int result = 0;
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.All))
				++result;
			return result;
		}


		/// <summary>
		/// Checks if the mappings between ConnectionPoints and Terminals can be reused
		/// </summary>
		private void CopyTerminalMappings(IModelObject oldModelObject, IModelObject newModelObject) {
			if (oldModelObject == null) throw new ArgumentNullException("oldModelObject");
			if (newModelObject == null) throw new ArgumentNullException("newModelObject");
			foreach (KeyValuePair<ControlPointId, TerminalId> item in connectionPointMappings) {
				string oldTerminalName = oldModelObject.Type.GetTerminalName(item.Value);
				string newTerminalName = newModelObject.Type.GetTerminalName(item.Value);
				if (oldTerminalName != newTerminalName)
					connectionPointMappings[item.Key] = TerminalId.Invalid;
			}
		}


		#region Fields

		private static string entityTypeName = "Core.Template";
		private static string connectionPtMappingName = "ConnectionPointMapping";
		
		private static string[] connectionPtMappingAttrNames = new string[] { "PointId", "TerminalId" };
		private static Type[] connectionPtMappingAttrTypes = new Type[] { typeof(int), typeof(int) };		

		private const string deactivatedTag = "Deactivated";
		private const string activatedTag = "Activated";

		private object id = null;
		private string name;
		private string title;
		private string description;
		private Shape shape;
		private object tag;
		
		private Dictionary<ControlPointId, TerminalId> connectionPointMappings = new Dictionary<ControlPointId, TerminalId>();
		private SortedList<int, IModelMapping> propertyMappings = new SortedList<int, IModelMapping>();
		
		#endregion
	}
}