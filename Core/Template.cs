using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

using Dataweb.nShape.Advanced;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;


namespace Dataweb.nShape.Advanced {

	#region Property Mappings

	public class PropertyMappingIdAttribute : Attribute {

		public PropertyMappingIdAttribute(int id) {
			this.id = id;
		}

		public int Id { get { return id; } }

		private int id;
	}


	/// <summary>
	/// Defines the mapping of shape properties to model properties.
	/// </summary>
	public interface IModelMapping: IEntity {

		int ShapePropertyId { get; }

		int ModelPropertyId { get; }

		bool CanSetInteger { get; }

		bool CanSetFloat { get; }

		bool CanSetString { get; }

		void SetInteger(int value);

		void SetFloat(float value);

		void SetString(string value);

		bool CanGetInteger { get; }

		bool CanGetFloat { get; }

		bool CanGetString { get; }

		bool CanGetStyle { get; }

		int GetInteger();

		float GetFloat();

		string GetString();

		IStyle GetStyle();

	}


	public abstract class ModelMappingBase : IModelMapping {

		protected ModelMappingBase(int modelPropertyId, int shapePropertyId) {
			this.shapePropertyId = shapePropertyId;
			this.modelPropertyId = modelPropertyId;
		}


		/// <summary>
		/// Constructor for IEntity CreateInstanceDelegate: Creates an empty instance for loading from Repository
		/// </summary>
		protected ModelMappingBase() {
		}


		#region IModelMapping Members

		public int ShapePropertyId { get { return shapePropertyId; } }


		public int ModelPropertyId { get { return modelPropertyId; } }


		public abstract void SetInteger(int value);

		public abstract void SetFloat(float value);

		public abstract void SetString(string value);

		public abstract int GetInteger();

		public abstract float GetFloat();

		public abstract string GetString();

		public abstract IStyle GetStyle();

		public abstract bool CanSetInteger { get; }

		public abstract bool CanSetFloat { get; }

		public abstract bool CanSetString { get; }

		public abstract bool CanGetInteger { get; }

		public abstract bool CanGetFloat { get; }

		public abstract bool CanGetString { get; }

		public abstract bool CanGetStyle { get; }

		#endregion


		#region IEntity Members

		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("ShapePropertyId", typeof(int));
			yield return new EntityFieldDefinition("ModelPropertyId", typeof(int));
		}


		public object Id {
			get { return id; }
		}


		public void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) throw new InvalidOperationException(string.Format("{0} has already a id.", GetType().Name));
			this.id = id;

		}

		public virtual void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			shapePropertyId = reader.ReadInt32();
			modelPropertyId = reader.ReadInt32();
		}


		public virtual void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			//nothing to do
		}


		public virtual void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteInt32(shapePropertyId);
			writer.WriteInt32(modelPropertyId);
		}


		public virtual void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			// nothing to do
		}


		public void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		//internal bool IsIntegerType(Type type) {
		//   return (type == typeof(Byte) || type == typeof(Int16)	|| type == typeof(Int32) 
		//      || type == typeof(Int64) || type == typeof(SByte) || type == typeof(UInt16)
		//      || type == typeof(UInt32) || type == typeof(UInt64) || type == typeof(Enum));
		//}


		//internal bool IsFloatType(Type type) {
		//   return (type == typeof(Single)
		//      || type == typeof(Double)
		//      || type == typeof(Decimal));
		//}


		//// Check if the given type is based on targetType
		//internal bool IsOfType(Type type, Type targetType) {
		//   return (type.IsSubclassOf(targetType) || type.GetInterface(targetType.Name, true) != null);
		//}


		// Fields
		private object id = null;
		private int shapePropertyId;
		private int modelPropertyId;
	}


	public enum NumericModelMappingType { IntegerInteger, IntegerFloat, FloatInteger, FloatFloat };


	public class NumericModelMapping : ModelMappingBase {

		/// <summary>
		/// Constructs a new NumericModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerFloat e.g. means model's integer property to shapes float property.
		/// </param>
		public NumericModelMapping(int shapePropertyId, int modelPropertyId, NumericModelMappingType mappingType)
			: this(shapePropertyId, modelPropertyId, mappingType, 0, 1) {
		}


		/// <summary>
		/// Constructs a new NumericModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerFloat e.g. means model's integer property to shapes float property.
		/// </param>
		/// <param name="intercept">Defines an offset for the mapped value.</param>
		/// <param name="slope">Defines a factor for the mapped value.</param>
		public NumericModelMapping(int shapePropertyId, int modelPropertyId, NumericModelMappingType mappingType, float intercept, float slope)
			: base(modelPropertyId, shapePropertyId) {
			this.mappingType = mappingType;
			this.intercept = intercept;
			this.slope = slope;
		}


		/// <summary>
		/// Constructor for IEntity CreateInstanceDelegate: Creates an empty instance for loading from Repository
		/// </summary>
		internal NumericModelMapping()
			: base() {
		}


		#region IModelMappping Members

		public override bool CanGetInteger {
			get {
				return (mappingType == NumericModelMappingType.FloatInteger
					|| mappingType == NumericModelMappingType.IntegerInteger);
			}
		}


		public override bool CanSetInteger {
			get {
				return (mappingType == NumericModelMappingType.IntegerFloat
					|| mappingType == NumericModelMappingType.IntegerInteger);
			}
		}


		public override bool CanGetFloat {
			get {
				return (mappingType == NumericModelMappingType.IntegerFloat
					|| mappingType == NumericModelMappingType.FloatFloat);
			}
		}


		public override bool CanSetFloat {
			get {
				return (mappingType == NumericModelMappingType.FloatInteger
					|| mappingType == NumericModelMappingType.FloatFloat);
			}
		}


		public override bool CanGetString {
			get { return false; }
		}


		public override bool CanSetString {
			get { return false; }
		}


		public override bool CanGetStyle {
			get { return false; }
		}


		public override int GetInteger() {
			if (CanGetInteger) return (int)Math.Round(Intercept + (intValue * Slope));
			else throw new NotSupportedException();
		}


		public override void SetInteger(int value) {
			if (CanSetInteger) intValue = value;
			else throw new NotSupportedException();
		}


		public override float GetFloat() {
			if (CanGetFloat) return Intercept + (floatValue * Slope);
			else throw new NotSupportedException();
		}


		public override void SetFloat(float value) {
			if (CanSetFloat) floatValue = value;
			else throw new NotSupportedException();
		}


		public override string GetString() {
			throw new NotSupportedException();
		}


		public override void SetString(string value) {
			throw new NotSupportedException();
		}


		public override IStyle GetStyle() {
			throw new NotSupportedException();
		}

		#endregion


		#region IEntity Members

		public static string EntityTypeName {
			get { return entityTypeName; }
		}


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


		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			mappingType = (NumericModelMappingType)reader.ReadInt32();
			intercept = reader.ReadFloat();
			slope = reader.ReadFloat();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32((int)mappingType);
			writer.WriteFloat(intercept);
			writer.WriteFloat(slope);
		}

		#endregion


		public NumericModelMappingType MappingType {
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


		#region Fields

		private const string entityTypeName = "NumericModelMapping";
		
		private NumericModelMappingType mappingType;
		private int intValue = 0;
		private float floatValue = 0;
		private float slope = 1;
		private float intercept = 0;

		#endregion
	}


	public enum FormatModelMappingType { IntegerString, FloatString, StringString };


	public class FormatModelMapping : ModelMappingBase {

		/// <summary>
		/// Constructs a new FormatModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerString e.g. means model's integer property to shapes string property.
		/// </param>
		public FormatModelMapping(int shapePropertyId, int modelPropertyId, FormatModelMappingType mappingType)
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
		public FormatModelMapping(int shapePropertyId, int modelPropertyId, FormatModelMappingType mappingType, string format)
			: base(modelPropertyId, shapePropertyId) {
			this.format = format;
			this.mappingType = mappingType;
		}


		/// <summary>
		/// Constructor for IEntity CreateInstanceDelegate: Creates an empty instance for loading from Repository
		/// </summary>
		internal FormatModelMapping()
			: base() {
		}


		#region IModelMapping Members

		public override bool CanGetInteger {
			get { return false; }
		}


		public override bool CanSetInteger {
			get { return (mappingType == FormatModelMappingType.IntegerString); }
		}


		public override bool CanGetFloat {
			get { return false; }
		}


		public override bool CanSetFloat {
			get { return (mappingType == FormatModelMappingType.FloatString); }
		}


		public override bool CanGetString {
			get { return true; }
		}


		public override bool CanSetString {
			get { return (mappingType == FormatModelMappingType.StringString); }
		}


		public override bool CanGetStyle {
			get { return false; }
		}


		public override int GetInteger() {
			throw new NotSupportedException();
		}


		public override void SetInteger(int value) {
			if (CanSetInteger) intValue = value;
			else throw new NotSupportedException();
		}


		public override float GetFloat() {
			throw new NotSupportedException();
		}


		public override void SetFloat(float value) {
			if (CanSetFloat) floatValue = value;
			else throw new NotSupportedException();
		}


		public override string GetString() {
			switch (mappingType) {
				case FormatModelMappingType.FloatString:
					return string.Format(Format, floatValue);
				case FormatModelMappingType.IntegerString:
					return string.Format(Format, intValue);
				case FormatModelMappingType.StringString:
					return string.Format(Format, stringValue);
				default: throw new nShapeUnsupportedValueException(mappingType);
			}
		}


		public override void SetString(string value) {
			if (CanSetString) stringValue = value;
			else throw new NotSupportedException();
		}


		public override IStyle GetStyle() {
			throw new NotSupportedException();
		}

		#endregion


		#region IEntity Members

		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition propDef in ModelMappingBase.GetPropertyDefinitions(version))
				yield return propDef;

			yield return new EntityFieldDefinition("MappingType", typeof(int));
			yield return new EntityFieldDefinition("format", typeof(string));
		}


		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			mappingType = (FormatModelMappingType)reader.ReadInt32();
			format = reader.ReadString();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32((int)mappingType);
			writer.WriteString(format);
		}

		#endregion


		public FormatModelMappingType MappingType {
			get { return mappingType; }
		}

		
		public string Format {
			get { return format; }
			set {
				if (value == null) throw new ArgumentNullException("Format");
				if (value == string.Empty) throw new ArgumentException("Format");
				format = value;
			}
		}


		#region Fields

		private const string entityTypeName = "FormatModelMapping";

		private FormatModelMappingType mappingType;
		private string format;
		private int intValue;
		private float floatValue;
		private string stringValue;

		#endregion
	}


	public enum StyleModelMappingType { IntegerStyle, FloatStyle };


	public class StyleModelMapping : ModelMappingBase {

		/// <summary>
		/// Constructs a new StyleModelMapping instance.
		/// </summary>
		/// <param name="shapePropertyId">PropertyId of the shape's property.</param>
		/// <param name="modelPropertyId">PropertyId of the model's property.</param>
		/// <param name="mappingType">
		/// Type of the mapping:
		/// IntegerStyle e.g. means model's integer property to shapes style property.
		/// </param>
		public StyleModelMapping(int shapePropertyId, int modelPropertyId, StyleModelMappingType mappingType)
			: base(modelPropertyId, shapePropertyId) {
			this.mappingType = mappingType;
			if (mappingType == StyleModelMappingType.IntegerStyle)
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
		public StyleModelMapping(int shapePropertyId, int modelPropertyId, StyleModelMappingType mappingType, IStyle style)
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

		public override bool CanGetInteger {
			get { return false; }
		}


		public override bool CanSetInteger {
			get { return (mappingType == StyleModelMappingType.IntegerStyle); }
		}
		

		public override bool CanGetFloat {
			get { return false; }
		}


		public override bool CanSetFloat {
			get { return (mappingType == StyleModelMappingType.FloatStyle); }
		}


		public override bool CanGetString {
			get { return false; }
		}


		public override bool CanSetString {
			get { return false; }
		}


		public override bool CanGetStyle {
			get { return true; }
		}


		public override int GetInteger() {
			throw new NotSupportedException();
		}


		public override void SetInteger(int value) {
			if (CanSetInteger) intValue = value;
			else throw new NotSupportedException();
		}


		public override float GetFloat() {
			throw new NotSupportedException();
		}


		public override void SetFloat(float value) {
			if (CanSetInteger) floatValue = value;
			else throw new NotSupportedException();
		}


		public override string GetString() {
			throw new NotSupportedException();
		}


		public override void SetString(string value) {
			throw new NotSupportedException();
		}


		public override IStyle GetStyle() {
			IStyle result;
			if (mappingType == StyleModelMappingType.IntegerStyle) {
				int fromValue = int.MinValue;
				result = defaultStyle;
				foreach (KeyValuePair<int, IStyle> range in intRanges) {
					if (fromValue == int.MinValue && intValue < range.Key)
						break;
					else if (fromValue <= intValue && intValue < range.Key) {
						result = intRanges[fromValue];
						break;
					} else result = range.Value;
					fromValue = range.Key;
				}
			} else if (mappingType == StyleModelMappingType.FloatStyle) {
				float fromValue = float.MinValue;
				result = defaultStyle;
				foreach (KeyValuePair<float, IStyle> range in floatRanges) {
					if (fromValue == float.MinValue && floatValue < range.Key)
						break;
					else if (fromValue <= floatValue && floatValue < range.Key) {
						result = floatRanges[fromValue];
						break;
					} else result = range.Value;
					fromValue = range.Key;
				}
			} else throw new NotSupportedException();
			return result;
		}

		#endregion


		#region IEntity Members

		public static string EntityTypeName {
			get { return entityTypeName; }
		}
		
		
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


		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			mappingType = (StyleModelMappingType)reader.ReadInt32();
			if (mappingType == StyleModelMappingType.IntegerStyle)
				intRanges = new SortedList<int, IStyle>();
			else floatRanges = new SortedList<float, IStyle>();
			defaultStyle = ReadStyle(reader);
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32((int)mappingType);
			WriteStyle(writer, defaultStyle);
		}


		public override void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			base.LoadInnerObjects(propertyName, reader, version);
			Debug.Assert(propertyName == "ValueRanges");
			Debug.Assert(intRanges.Count == 0);
			reader.BeginReadInnerObjects();
			while (reader.BeginReadInnerObject()) {
				IStyle style = null;
				switch (mappingType) {
					case StyleModelMappingType.IntegerStyle:
						int intValue = (int)reader.ReadFloat();
						style = ReadStyle(reader);
						intRanges.Add(intValue, style);
						break;
					case StyleModelMappingType.FloatStyle:
						float floatValue = reader.ReadFloat();
						style = ReadStyle(reader);
						floatRanges.Add(floatValue, style);
						break;
					default: throw new nShapeUnsupportedValueException(mappingType);
				}
				reader.EndReadInnerObject();
			}
			reader.EndReadInnerObjects();
		}


		public override void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			base.SaveInnerObjects(propertyName, writer, version);
			Debug.Assert(propertyName == "ValueRanges");
			writer.BeginWriteInnerObjects();
			switch (mappingType) {
				case StyleModelMappingType.IntegerStyle:
					foreach (KeyValuePair<int, IStyle> range in intRanges) {
						writer.BeginWriteInnerObject();
						writer.WriteFloat(range.Key);
						WriteStyle(writer, range.Value);
						writer.EndWriteInnerObject();
					}
					break;
				case StyleModelMappingType.FloatStyle:
					foreach (KeyValuePair<float, IStyle> range in floatRanges) {
						writer.BeginWriteInnerObject();
						writer.WriteFloat(range.Key);
						WriteStyle(writer, range.Value);
						writer.EndWriteInnerObject();
					}
					break;
				default: throw new nShapeUnsupportedValueException(mappingType);
			}
			writer.EndWriteInnerObjects();
		}

		#endregion


		public StyleModelMappingType MappingType {
			get { return mappingType; }
		}

	
		public int ValueRangeCount {
			get {
				if (mappingType == StyleModelMappingType.IntegerStyle)
					return intRanges.Count;
				else if (mappingType == StyleModelMappingType.FloatStyle)
					return floatRanges.Count;
				else throw new NotSupportedException();
			}
		}


		public IEnumerable<object> ValueRanges {
			get {
				if (mappingType == StyleModelMappingType.IntegerStyle) {
					foreach (KeyValuePair<int, IStyle> range in intRanges)
						yield return range.Key;
				} else if (mappingType == StyleModelMappingType.FloatStyle) {
					foreach (KeyValuePair<float, IStyle> range in floatRanges)
						yield return range.Key;
				} else throw new NotSupportedException();
			}
		}


		public IStyle this[int key] {
			get {
				if (mappingType == StyleModelMappingType.IntegerStyle)
					return intRanges[key];
				else throw new NotSupportedException();
			}
		}


		public IStyle this[float key] {
			get {
				if (mappingType == StyleModelMappingType.FloatStyle)
					return floatRanges[key];
				else throw new NotSupportedException();
			}
		}


		public void ClearValueRanges() {
			if (mappingType == StyleModelMappingType.IntegerStyle) {
				intRanges.Clear();
			} else if (mappingType == StyleModelMappingType.FloatStyle) {
				floatRanges.Clear();
			} else throw new NotSupportedException();
		}


		public void AddValueRange(int value, IStyle style) {
			if (mappingType == StyleModelMappingType.IntegerStyle)
				intRanges.Add(value, style);
			else throw new NotSupportedException();
		}


		public void AddValueRange(float value, IStyle style) {
			if (mappingType == StyleModelMappingType.FloatStyle)
				floatRanges.Add(value, style);
			else throw new NotSupportedException();
		}


		public bool RemoveValueRange(int value) {
			if (mappingType == StyleModelMappingType.IntegerStyle) 
				return intRanges.Remove(value);
			else throw new NotSupportedException();
		}


		public bool RemoveValueRange(float value) {
			if (mappingType == StyleModelMappingType.FloatStyle)
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
				case MappedStyleType.ShapeStyle:
					result = reader.ReadShapeStyle(); break;
				case MappedStyleType.Unassigned:
					// Skip value - it does not matter what we read here
					reader.ReadColorStyle();	// ToDo: Find a better solution for skipping an object id
					result = null;
					break;
				default: throw new nShapeUnsupportedValueException(mappedStyleType);
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
			else if (style is IShapeStyle) return MappedStyleType.ShapeStyle;
			else throw new nShapeUnsupportedValueException(style);
		}
		
		
		private enum MappedStyleType { 
			Unassigned, 
			CapStyle, 
			CharacterStyle, 
			ColorStyle, 
			FillStyle, 
			LineStyle, 
			ParagraphStyle, 
			ShapeStyle 
		}


		#region Fields

		private const string entityTypeName = "StyleModelMapping";

		private StyleModelMappingType mappingType;
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

		public Template(string name, Shape shape) {
			if (name == null) throw new ArgumentNullException("name");
			if (shape == null) throw new ArgumentNullException("shape");
			this.name = name;
			this.shape = shape;
		}


		public Template Clone() {
			Template result = new Template();
			result.CopyFrom(this);
			return result;
		}


		public void CopyFrom(Template source) {
			if (source == null) throw new ArgumentNullException("source");

			this.name = source.name;
			this.title = source.title;
			this.description = source.description;
			
			// Clone or copy shape
			if (this.shape == null)	this.shape = (Shape)source.shape.Clone();
			else this.shape.CopyFrom(source.shape);

			// copy connection point mapping
			this.connectionPointMappings.Clear();
			foreach (KeyValuePair<ControlPointId, TerminalId> item in source.connectionPointMappings)
				this.connectionPointMappings.Add(item.Key, item.Value);

			// copy property mapping
			this.propertyMappings.Clear();
			foreach (KeyValuePair<int, IModelMapping>item in source.propertyMappings)
				this.propertyMappings.Add(item.Key, item.Value);
		}


		public string Name {
			get { return name; }
			set { name = value; }
		}


		public string Title {
			get { return title; }
			set { title = value; }
		}


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
				result.ModelObject = shape.ModelObject.Clone();
			return result;
		}


		/// <summary>
		/// Creates a thumbnail of the template shape.
		/// </summary>
		/// <param name="size">Size of tumbnail in pixels</param>
		/// <param name="margin">Size of margin around shape in pixels</param>
		/// <returns></returns>
		public Image CreateThumbnail(int size, int margin) {
			return CreateThumbnail(size, margin, Color.White);
		}


		public Image CreateThumbnail(int size, int margin, Color transparentColor) {
			Image bmp = new Bitmap(size, size);
			using (Shape shapeClone = Shape.Clone())
				shapeClone.DrawThumbnail(bmp, margin, transparentColor);
			return bmp;
		}


		public IEnumerable<nShapeAction> GetActions() {
			yield break;
		}


		#region Visualization Mapping

		public IEnumerable<IModelMapping> GetPropertyMappings() {
			return propertyMappings.Values;
		}


		public IModelMapping GetPropertyMapping(int modelPropertyId) {
			IModelMapping result = null;
			propertyMappings.TryGetValue(modelPropertyId, out result);
			return result;
		}


		public void MapProperties(IModelMapping propertyMapping) {
			if (propertyMapping == null) throw new ArgumentNullException("propertyMapping");
			if (propertyMappings.ContainsKey(propertyMapping.ModelPropertyId))
				propertyMappings[propertyMapping.ModelPropertyId] = propertyMapping;
			else
				propertyMappings.Add(propertyMapping.ModelPropertyId, propertyMapping);
		}


		public void UnmapProperties(IModelMapping propertyMapping) {
			if (propertyMapping == null) throw new ArgumentNullException("propertyMapping");
			propertyMappings.Remove(propertyMapping.ModelPropertyId);
		}


		public void UnmapAllProperties() {
			propertyMappings.Clear();
		}

		#endregion


		#region Terminal Mapping

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


		public void MapTerminal(TerminalId terminalId, ControlPointId connectionPointId) {
			// check if terminalId and connectionPointId are valid values
			if (shape == null)
				throw new InvalidOperationException("Template has no shape.");
			if (!shape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Glue | ControlPointCapabilities.Connect))
				throw new nShapeException("Control point {0} is not a valid connection point.", connectionPointId);
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

		public static string EntityTypeName { get { return entityTypeName; } }


		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("Title", typeof(string));
			yield return new EntityFieldDefinition("Description", typeof(string));
			yield return new EntityInnerObjectsDefinition(connectionPtMappingName + "s", connectionPtMappingName, connectionPtMappingAttrNames, connectionPtMappingAttrTypes);
		}


		public object Id {
			get { return id; }
		}


		public void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) throw new InvalidOperationException("Template has already an id.");
			this.id = id;
		}


		public void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteString(name);
			writer.WriteString(title);
			writer.WriteString(description);
		}


		public void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			name = reader.ReadString();
			title = reader.ReadString();
			description = reader.ReadString();
		}


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
		
		private Dictionary<ControlPointId, TerminalId> connectionPointMappings = new Dictionary<ControlPointId, TerminalId>();
		private SortedList<int, IModelMapping> propertyMappings = new SortedList<int, IModelMapping>();
		
		#endregion
	}
}