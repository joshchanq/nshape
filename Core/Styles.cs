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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using Dataweb.NShape.Advanced;
using Dataweb.Utilities;


namespace Dataweb.NShape {

	/// <summary>
	/// Specifies the category of a style.
	/// </summary>
	public enum StyleCategory {
		CapStyle,
		CharacterStyle,
		ColorStyle,
		FillStyle,
		LineStyle,
		ParagraphStyle
	}


	public enum ImageLayoutMode { Original, Center, Stretch, Fit, Tile, CenterTile, FilpTile }


	public enum FillMode { Solid, Gradient, Pattern, Image }


	public enum CapShape { None, ArrowClosed, ArrowOpen, Circle, Triangle, Diamond, Square, CenteredCircle, CenteredHalfCircle }


	public enum DashType { Solid, Dash, Dot, DashDot, DashDotDot }


	[TypeConverter("Dataweb.NShape.WinFormsUI.TextPaddingConverter, Dataweb.NShape.WinFormsUI")]
	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct TextPadding {

		static TextPadding() {
			Empty.Left = 0;
			Empty.Top = 0;
			Empty.Right = 0;
			Empty.Bottom = 0;
		}


		public TextPadding(int all) {
			left = top = right = bottom = all;
		}


		public TextPadding(int left, int top, int right, int bottom) {
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}


		public int Left {
			get { return left; }
			set { left = value; }
		}


		public int Top {
			get { return top; }
			set { top = value; }
		}


		public int Right {
			get { return right; }
			set { right = value; }
		}


		public int Bottom {
			get { return bottom; }
			set { bottom = value; }
		}


		public int Horizontal { get { return left + right; } }


		public int Vertical { get { return top + bottom; } }


		public static readonly TextPadding Empty;


		private int left, top, right, bottom;
	}


	/// <summary>
	/// A type description provider that calls type converters and/or ui type editors registered with the TypeDescriptorRegistrar.
	/// </summary>
	public class StyleTypeDescriptionProvider : TypeDescriptionProvider {

		public StyleTypeDescriptionProvider()
			: base(TypeDescriptor.GetProvider(typeof(Style))) {
		}


		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
			return new StyleTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
		}

	}


	public class StyleTypeDescriptor : CustomTypeDescriptor {

		public StyleTypeDescriptor(ICustomTypeDescriptor parent)
			: base(parent) {
		}


		public override object GetEditor(Type editorBaseType) {
			return TypeDescriptorRegistrar.GetRegisteredUITypeEditor(editorBaseType) ?? base.GetEditor(editorBaseType);
		}


		public override TypeConverter GetConverter() {
			return TypeDescriptorRegistrar.GetRegisteredTypeConverter(typeof(IStyle)) ?? base.GetConverter();
		}

	}


	/// <summary>
	/// Returns the style of the same type with the same projectName if there is one in the design's style collection.
	/// </summary>
	public delegate IStyle FindStyleCallback(IStyle style);


	public delegate Style CreatePreviewStyleCallback(IStyle style);


	#region ***   Style Interfaces   ***

	[TypeConverter("Dataweb.NShape.WinFormsUI.StyleConverter, Dataweb.NShape.WinFormsUI")]
	public interface IStyle : IEntity, IDisposable {
		[Browsable(false)]
		string Name { get; }
		string Title { get; }
		string ToString();
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface ICapStyle : IStyle {
		CapShape CapShape { get; }
		short CapSize { get; }
		IColorStyle ColorStyle { get; }
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IColorStyle : IStyle {
		Color Color { get; }
		byte Transparency { get; }	// percentage (range 0 to 100)
		bool ConvertToGray { get; }
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IFillStyle : IStyle {
		IColorStyle BaseColorStyle { get; }
		IColorStyle AdditionalColorStyle { get; }
		FillMode FillMode { get; }
		HatchStyle FillPattern { get; }
		short GradientAngle { get; }
		bool ConvertToGrayScale { get; }
		NamedImage Image { get; }
		ImageLayoutMode ImageLayout { get; }
		byte ImageTransparency { get; }
		float ImageGammaCorrection { get; }
		byte ImageCompressionQuality { get; }
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface ICharacterStyle : IStyle {

		/// <summary>
		/// Name of the FontFamily
		/// </summary>
		string FontName { get; }

		/// <summary>
		/// The characterStyle's FontFamily
		/// </summary>
		FontFamily FontFamily { get; }

		/// <summary>
		/// Size in points
		/// </summary>
		float SizeInPoints { get; }

		/// <summary>
		/// Size in world coordinates
		/// </summary>
		int Size { get; }

		/// <summary>
		/// The character's style
		/// </summary>
		FontStyle Style { get; }

		/// <summary>
		/// The font's color.
		/// </summary>
		IColorStyle ColorStyle { get; }
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface ILineStyle : IStyle {
		int LineWidth { get; }
		IColorStyle ColorStyle { get; }
		DashType DashType { get; }
		DashCap DashCap { get; }
		float[] DashPattern { get; }
		LineJoin LineJoin { get; }
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IShapeStyle : IStyle {
		bool RoundedCorners { get; }
		bool ShowGradients { get; }
		bool ShowShadows { get; }
		IColorStyle ShadowColor { get; }
		//bool ThreeD { get; }
	}


	[Editor("Dataweb.NShape.WinFormsUI.StyleEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IParagraphStyle : IStyle {
		ContentAlignment Alignment { get; }
		StringTrimming Trimming { get; }
		TextPadding Padding { get; }
		bool WordWrap { get; }
		// LineNumbering Numbering { get; }
		// float LineSpacing { get; }
	}

	#endregion


	#region ***   StandardStyle Definitions   ***

	/// <summary>
	/// Base class for StandardStyleNames. 
	/// Implements all methods. Derived classes only have to define 
	/// public readonly string fields named like the standard style name.
	/// </summary>
	public abstract class StandardStyleNames {

		/// <summary>
		/// Base constructor of all derived StandardStyleNames.
		/// Initializes all public readonly string fields with their field names
		/// and creates the names string array.
		/// </summary>
		protected StandardStyleNames() {
			FieldInfo[] fieldInfos = this.GetType().GetFields();
			Array.Resize<string>(ref names, fieldInfos.Length);
			int idx = -1;
			for (int i = fieldInfos.Length - 1; i >= 0; --i) {
				if (fieldInfos[i].IsInitOnly && fieldInfos[i].IsPublic &&
					fieldInfos[i].FieldType == typeof(string)) {
					names[++idx] = fieldInfos[i].Name;
					fieldInfos[i].SetValue(this, fieldInfos[i].Name);
				}
			}
			Array.Resize<string>(ref names, idx);
		}


		public string this[int index] {
			get {
				if (index >= names.Length) throw new IndexOutOfRangeException();
				return names[index];
			}
		}


		public int Count {
			get { return names.Length; }
		}


		public bool EqualsAny(string name) {
			Debug.Assert(names != null);
			if (name == Style.EmptyStyleName) return true;
			for (int i = names.Length - 1; i >= 0; --i)
				if (names[i].Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return true;
			return false;
		}


		private string[] names;
	}


	public sealed class StandardCapStyleNames : StandardStyleNames {
		public readonly string None;
		public readonly string Arrow;
		public readonly string Special1;
		public readonly string Special2;
	}


	public sealed class StandardCharacterStyleNames : StandardStyleNames {
		public readonly string Normal;
		public readonly string Caption;
		public readonly string Subtitle;
		public readonly string Heading3;
		public readonly string Heading2;
		public readonly string Heading1;
	}


	public sealed class StandardColorStyleNames : StandardStyleNames {
		public readonly string Transparent;
		public readonly string Background;
		public readonly string Highlight;
		public readonly string Text;
		public readonly string HighlightText;
		public readonly string Black;
		public readonly string White;
		public readonly string Gray;
		public readonly string LightGray;
		public readonly string Red;
		public readonly string LightRed;
		public readonly string Blue;
		public readonly string LightBlue;
		public readonly string Green;
		public readonly string LightGreen;
		public readonly string Yellow;
		public readonly string LightYellow;
	}


	public sealed class StandardFillStyleNames : StandardStyleNames {
		public readonly string Transparent;
		public readonly string Black;
		public readonly string White;
		public readonly string Red;
		public readonly string Blue;
		public readonly string Green;
		public readonly string Yellow;
	}


	public sealed class StandardLineStyleNames : StandardStyleNames {
		public readonly string None;
		public readonly string Normal;
		public readonly string Thick;
		public readonly string Dotted;
		public readonly string Dashed;
		public readonly string Highlight;
		public readonly string HighlightThick;
		public readonly string HighlightDotted;
		public readonly string HighlightDashed;
		public readonly string Red;
		public readonly string Blue;
		public readonly string Green;
		public readonly string Yellow;
		public readonly string Special1;
		public readonly string Special2;
	}


	public sealed class StandardParagraphStyleNames : StandardStyleNames {
		public readonly string Label;
		public readonly string Text;
		public readonly string Title;
	}

	#endregion


	#region ***   Style Classes   ***

	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public abstract class Style : IStyle {

		public virtual void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style == null) throw new ArgumentNullException("style");
			if (findStyleCallback == null) throw new ArgumentNullException("findStyleCallback");
			if (this.Name != style.Name) this.Name = style.Name;
			this.Title = style.Title;
		}


		[Description("The name of the style, used for identifying this style in the style set. Has to unique inside its style set.")]
		[RequiredPermission(Permission.ModifyData)]
		public string Name {
			get { return name; }
			set {
				if (name == EmptyStyleName) throw new ArgumentException(string.Format("{0} is not a valid Style name.", EmptyStyleName));
				if (!renameable) throw new InvalidOperationException("Standard styles must not be renamed.");
				name = value;
			}
		}


		[Description("The title of the style.")]
		[RequiredPermission(Permission.ModifyData)]
		public string Title {
			get {
				if (title == null || title == string.Empty)
					return name;
				else return title;
			}
			set { title = value; }
		}


		public override string ToString() {
			return Title;
		}


		protected Style()
			: this(string.Empty) {
		}


		protected Style(string name) {
			this.renameable = !IsStandardName(name);
			this.name = name;
			this.title = string.Empty;
		}


		~Style() {
			Dispose();
		}


		protected abstract bool IsStandardName(string name);


		protected internal const string EmptyStyleName = "{Empty}";


		#region IDisposable Members

		public abstract void Dispose();

		#endregion


		#region IEntity Members

		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("Title", typeof(string));
		}


		[Browsable(false)]
		public virtual object Id {
			get { return id; }
		}


		public virtual void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null)
				throw new InvalidOperationException("Style has already an id.");
			this.id = id;
		}


		public virtual void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			name = reader.ReadString();
			title = reader.ReadString();
			renameable = !IsStandardName(name);
		}


		public virtual void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			// nothing to do
		}


		public virtual void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteString(name);
			writer.WriteString(title);
		}


		public virtual void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			// nothing to do
		}


		public virtual void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		#region Fields

		private object id = null;
		private string name = null;
		private string title = null;
		private bool renameable = true;

		#endregion
	}


	public sealed class CapStyle : Style, ICapStyle {

		public static readonly CapStyle Default;


		public static StandardCapStyleNames StandardNames;


		public CapStyle()
			: base() { }


		public CapStyle(string name)
			: base(name) { }


		#region IDisposable Members

		public override void Dispose() {
			if (colorStyle != null) {
				colorStyle.Dispose();
				colorStyle = null;
			}
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			CapShape = (CapShape)reader.ReadByte();
			CapSize = reader.ReadInt16();
			ColorStyle = (IColorStyle)reader.ReadColorStyle();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, 1);
			writer.WriteByte((byte)capShape);
			writer.WriteInt16(capSize);
			writer.WriteStyle(colorStyle);
		}


		public static string EntityTypeName { get { return "Core.CapStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("CapShape", typeof(byte));
			yield return new EntityFieldDefinition("CapSize", typeof(short));
			yield return new EntityFieldDefinition("ColorStyle", typeof(object));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is CapStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				this.CapShape = ((CapStyle)style).CapShape;
				this.CapSize = ((CapStyle)style).CapSize;
				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((CapStyle)style).ColorStyle);
				if (colorStyle != null) this.ColorStyle = colorStyle;
				else this.ColorStyle = ((CapStyle)style).ColorStyle;
			} else throw new NShapeException("Style is not of the required type.");
		}


		[Description("Shape of the line cap. If none, the cap of the line depends on the line style's LineJoin setting.")]
		[RequiredPermission(Permission.Present)]
		public CapShape CapShape {
			get { return capShape; }
			set { capShape = value; }
		}


		[Description("Size of the line cap in display units.")]
		[RequiredPermission(Permission.Present)]
		public short CapSize {
			get { return capSize; }
			set {
				if (value < 0)
					throw new NShapeException("Value has to be greater than 0.");
				capSize = value;
			}
		}


		[Description("Color of the line cap's interior.")]
		[RequiredPermission(Permission.Present)]
		public IColorStyle ColorStyle {
			get { return colorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					colorStyle = null;
				else colorStyle = value;
			}
		}


		protected override bool IsStandardName(string name) {
			return StandardNames.EqualsAny(name);
		}


		static CapStyle() {
			StandardNames = new StandardCapStyleNames();

			Default = new CapStyle(EmptyStyleName);
			Default.CapShape = CapShape.None;
			Default.CapSize = 1;
			Default.ColorStyle = Dataweb.NShape.ColorStyle.Empty;
		}


		private int GetCapShapePointCount(CapShape capShape) {
			switch (capShape) {
				case CapShape.ArrowClosed:
				case CapShape.Triangle:
					return 3;
				case CapShape.Circle:
				case CapShape.Diamond:
				case CapShape.Square:
					return 4;
				case CapShape.None:
					return 0;
				default:
					throw new NShapeUnsupportedValueException(capShape);
			}
		}


		#region Fields

		private CapShape capShape = CapShape.None;
		private short capSize = 10;
		private IColorStyle colorStyle = null;

		#endregion
	}


	public sealed class CharacterStyle : Style, ICharacterStyle {

		public static readonly CharacterStyle Default;


		public static StandardCharacterStyleNames StandardNames;


		public CharacterStyle()
			: base() {
			Construct();
		}


		public CharacterStyle(string name)
			: base(name) {
			Construct();
		}


		#region IDisposable Members

		public override void Dispose() {
			if (colorStyle != null) {
				colorStyle.Dispose();
				colorStyle = null;
			}
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			fontFamily = FindFontFamily(reader.ReadString());
			fontSizeInPoints = reader.ReadInt32() / 100f;
			fontSize = Geometry.PointToPixel(fontSizeInPoints, dpi);
			fontStyle = (FontStyle)reader.ReadByte();
			colorStyle = (IColorStyle)reader.ReadColorStyle();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteString(fontFamily.Name);
			writer.WriteInt32((int)(100 * fontSizeInPoints));
			writer.WriteByte((byte)fontStyle);
			writer.WriteStyle(colorStyle);
		}


		public static string EntityTypeName { get { return "Core.CharacterStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Dataweb.NShape.Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("FontName", typeof(string));
			yield return new EntityFieldDefinition("Size", typeof(int));
			yield return new EntityFieldDefinition("Decoration", typeof(byte));
			yield return new EntityFieldDefinition("ColorStyle", typeof(object));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is CharacterStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);

				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((CharacterStyle)style).ColorStyle);
				if (colorStyle != null) this.ColorStyle = colorStyle;
				else this.ColorStyle = ((CharacterStyle)style).ColorStyle;

				this.FontName = ((CharacterStyle)style).FontName;
				this.SizeInPoints = ((CharacterStyle)style).SizeInPoints;
				this.Style = ((CharacterStyle)style).Style;
			} else throw new NShapeException("Style is not of the required type.");
		}


		[Description("Name of the style's font.")]
		[RequiredPermission(Permission.Present)]
		[Editor("Dataweb.NShape.WinFormsUI.FontFamilyEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public string FontName {
			get { return fontFamily.Name; }
			set { fontFamily = FindFontFamily(value); }
		}


		[Browsable(false)]
		[RequiredPermission(Permission.Present)]
		public FontFamily FontFamily {
			get { return fontFamily; }
			set { fontFamily = value; }
		}


		/// <summary>
		/// Font Size in Point (1/72 Inch)
		/// </summary>
		[Description("Size of the style's font in points.")]
		[RequiredPermission(Permission.Present)]
		public float SizeInPoints {
			get { return fontSizeInPoints; }
			set {
				fontSizeInPoints = value;
				fontSize = Geometry.PointToPixel(value, dpi);
			}
		}


		[Description("Size of the style's font in display units.")]
		[RequiredPermission(Permission.Present)]
		public int Size {
			get { return fontSize; }
			set {
				fontSize = value;
				fontSizeInPoints = Geometry.PixelToPoint(value, dpi);
			}
		}


		[Description("Style of the style's font.")]
		[RequiredPermission(Permission.Present)]
		public FontStyle Style {
			get { return fontStyle; }
			set {
				if (value == FontStyle.Regular)
					fontStyle = FontStyle.Regular;
				else
					fontStyle = fontStyle ^ value;
			}
		}


		[Description("The color of the style's font.")]
		[RequiredPermission(Permission.Present)]
		public IColorStyle ColorStyle {
			get { return colorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					colorStyle = null;
				else colorStyle = value;
			}
		}


		protected override bool IsStandardName(string name) {
			return CharacterStyle.StandardNames.EqualsAny(name);
		}


		static CharacterStyle() {
			StandardNames = new StandardCharacterStyleNames();
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero))
				dpi = gfx.DpiY;

			Default = new CharacterStyle(EmptyStyleName);
			Default.ColorStyle = Dataweb.NShape.ColorStyle.Empty;
			Default.FontName = "Times New Roman";
			Default.SizeInPoints = 10;
			Default.Style = FontStyle.Regular;
		}


		private void Construct() {
			fontFamily = FindFontFamily("Arial");
			fontSize = Geometry.PointToPixel(fontSizeInPoints, dpi);
		}


		private FontFamily FindFontFamily(string fontName) {
			FontFamily[] families = FontFamily.Families;
			foreach (FontFamily ff in families) {
				if (ff.Name.Equals(fontName, StringComparison.InvariantCultureIgnoreCase))
					return ff;
			}
			throw new NShapeException(string.Format("'{0}' is not a valid font name name or font is not installed on this machine.", fontName));
		}


		#region Fields
		private static readonly float dpi;

		private float fontSizeInPoints = 8.25f;
		private int fontSize;
		private FontStyle fontStyle = 0;
		private FontFamily fontFamily = null;
		private IColorStyle colorStyle = null;
		#endregion
	}


	public sealed class ColorStyle : Style, IColorStyle {

		public static readonly IColorStyle Empty;


		public static StandardColorStyleNames StandardNames;


		public ColorStyle()
			: this(string.Empty, Color.Empty) {
		}


		public ColorStyle(string name)
			: this(name, Color.Empty) {
		}


		public ColorStyle(string name, Color color)
			: base(name) {
			Construct(color, AlphaToTransparency(color.A));
		}


		public ColorStyle(string name, Color color, byte transparency)
			: base(name) {
			Construct(color, transparency);
		}


		#region IDisposable Members

		public override void Dispose() {
			// nothing to do
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			color = Color.FromArgb(reader.ReadInt32());
			transparency = reader.ReadByte();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32(color.ToArgb());
			writer.WriteByte(transparency);
		}


		public static string EntityTypeName { get { return "Core.ColorStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Color", typeof(Color));
			yield return new EntityFieldDefinition("Transparency", typeof(byte));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is ColorStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				this.Color = ((ColorStyle)style).Color;
				this.Transparency = ((ColorStyle)style).Transparency;
				this.ConvertToGray = ((ColorStyle)style).ConvertToGray;
			} else throw new NShapeException("Style is not of the required type.");
		}


		[Description("The style's color.")]
		[RequiredPermission(Permission.Present)]
		public Color Color {
			get { return color; }
			set {
				color = value;
				transparency = AlphaToTransparency(color.A);
			}
		}


		/// <summary>
		/// Indicates the transparency in percent (0-100).
		/// </summary>
		[Description("Transparency of the style's color in percentage. 100 is 100% transparent and therefore invisible.")]
		[RequiredPermission(Permission.Present)]
		public byte Transparency {
			get { return transparency; }
			set {
				if (value < 0 || value > 100) throw new NShapeException("Value has to be between 0 and 100.");
				transparency = value;
				color = Color.FromArgb(TransparencyToAlpha(transparency), color);
			}
		}


		/// <summary>
		/// Specifies if the color should be converted into a grayscale value.
		/// </summary>
		[Browsable(false)]
		[RequiredPermission(Permission.Present)]
		public bool ConvertToGray {
			get { return convertToGray; }
			set { convertToGray = value; }
		}


		protected override bool IsStandardName(string name) {
			return StandardNames.EqualsAny(name);
		}


		static ColorStyle() {
			StandardNames = new StandardColorStyleNames();
			Empty = new ColorStyle(EmptyStyleName, Color.Empty);
		}


		private void Construct(Color color, byte transparency) {
			if (transparency < 0 || transparency > 100) throw new ArgumentOutOfRangeException("Argument 'transparency' has to be between 0 and 100.");
			this.transparency = transparency;
			this.color = Color.FromArgb(TransparencyToAlpha(transparency), color);
		}


		private byte AlphaToTransparency(byte alpha) {
			return (byte)(100 - Math.Round(alpha / 2.55f));
		}


		private byte TransparencyToAlpha(byte transparency) {
			if (transparency < 0 || transparency > 100)
				throw new ArgumentOutOfRangeException("Value has to be between 0 and 100.");
			return Convert.ToByte(255 - (transparency * 2.55f));
		}


		#region Fields
		private Color color = Color.White;
		private byte transparency = 0;
		private bool convertToGray = false;
		#endregion
	}


	public sealed class FillStyle : Style, IFillStyle {

		public static readonly FillStyle Default;


		public static StandardFillStyleNames StandardNames;


		public FillStyle()
			: this(string.Empty) {
		}


		/// <summary>
		/// Creates a new empty fill style instance.
		/// </summary>
		/// <param name="name">Name of the fill style.</param>
		public FillStyle(string name)
			: this(name, ColorStyle.Empty, ColorStyle.Empty) {
		}


		/// <summary>
		/// Creates a new color or pattern based fill style instance.
		/// </summary>
		/// <param name="name">Name of the fill style.</param>
		/// <param name="baseColorStyle">The base color of the fill style.</param>
		/// <param name="additionalColorStyle">The alternate color of the fill style (gradient color or second color of patterns).</param>
		public FillStyle(string name, IColorStyle baseColorStyle, IColorStyle additionalColorStyle)
			: base(name) {
			Construct(baseColorStyle, additionalColorStyle);
		}


		/// <summary>
		/// Creates a new texture based fill style instance.
		/// </summary>
		/// <param name="isPreviewStyle">Specifies if this fill style is used for drawing previews.</param>
		/// <param name="name">Name of the fill style.</param>
		/// <param name="image">The image that defines the texture.</param>
		public FillStyle(string name, NamedImage image)
			: base(name) {
			Construct(image);
		}


		#region IDisposable Members

		public override void Dispose() {
			if (baseColorStyle != null) {
				baseColorStyle.Dispose();
				baseColorStyle = null;
			}
			if (additionalColorStyle != null) {
				additionalColorStyle.Dispose();
				additionalColorStyle = null;
			}
			if (namedImage != null) {
				namedImage.Dispose();
				namedImage = null;
			}
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			baseColorStyle = (IColorStyle)reader.ReadColorStyle();
			additionalColorStyle = (IColorStyle)reader.ReadColorStyle();
			fillMode = (FillMode)reader.ReadByte();
			fillPattern = (HatchStyle)reader.ReadByte();
			imageLayout = (ImageLayoutMode)reader.ReadByte();
			imageTransparency = reader.ReadByte();
			imageGamma = reader.ReadFloat();
			imageCompressionQuality = reader.ReadByte();
			string imageName = reader.ReadString();
			Image image = reader.ReadImage();
			if (image != null || imageName != string.Empty)
				namedImage = new NamedImage(image, imageName);
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteStyle(baseColorStyle);
			writer.WriteStyle(additionalColorStyle);
			writer.WriteByte((byte)fillMode);
			writer.WriteByte((byte)fillPattern);
			writer.WriteByte((byte)imageLayout);
			writer.WriteByte(imageTransparency);
			writer.WriteFloat(imageGamma);
			writer.WriteByte(imageCompressionQuality);
			if (namedImage != null) {
				writer.WriteString(namedImage.Name);
				writer.WriteImage(namedImage.Image);
			} else {
				writer.WriteString(string.Empty);
				writer.WriteImage(null);
			}
		}


		public static string EntityTypeName { get { return "Core.FillStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("BaseColorStyle", typeof(object));
			yield return new EntityFieldDefinition("AdditionalColorStyle", typeof(object));
			yield return new EntityFieldDefinition("FillMode", typeof(byte));
			yield return new EntityFieldDefinition("FillPattern", typeof(byte));
			yield return new EntityFieldDefinition("ImageLayout", typeof(byte));
			yield return new EntityFieldDefinition("ImageTransparency", typeof(byte));
			yield return new EntityFieldDefinition("ImageGammaCorrection", typeof(float));
			yield return new EntityFieldDefinition("ImageCompressionQuality", typeof(byte));
			yield return new EntityFieldDefinition("ImageFileName", typeof(string));
			yield return new EntityFieldDefinition("Image", typeof(Image));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is FillStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				IColorStyle colorStyle;
				if (((FillStyle)style).AdditionalColorStyle != null) {
					colorStyle = (IColorStyle)findStyleCallback(((FillStyle)style).AdditionalColorStyle);
					if (colorStyle != null) this.AdditionalColorStyle = colorStyle;
					else this.AdditionalColorStyle = ((FillStyle)style).AdditionalColorStyle;
				}
				if (((FillStyle)style).BaseColorStyle != null) {
					colorStyle = (IColorStyle)findStyleCallback(((FillStyle)style).BaseColorStyle);
					if (colorStyle != null) this.BaseColorStyle = colorStyle;
					else this.BaseColorStyle = ((FillStyle)style).BaseColorStyle;
				}

				this.ConvertToGrayScale = ((FillStyle)style).ConvertToGrayScale;
				this.FillMode = ((FillStyle)style).FillMode;
				this.FillPattern = ((FillStyle)style).FillPattern;
				if (this.Image != null) this.Image.Dispose();
				this.Image = ((FillStyle)style).Image;
				this.ImageCompressionQuality = ((FillStyle)style).ImageCompressionQuality;
				this.ImageGammaCorrection = ((FillStyle)style).ImageGammaCorrection;
				this.ImageLayout = ((FillStyle)style).ImageLayout;
				this.ImageTransparency = ((FillStyle)style).ImageTransparency;
			} else throw new NShapeException("Style is not of the required Type.");
		}


		[Description("The base color of the style. Depends on the style's FillMode: For gradients, it is the color of the lower right edge. In case of a pattern, this is the background color. For the solid fill mode it is the fill color and otherwise it will be ignored.")]
		[RequiredPermission(Permission.Present)]
		public IColorStyle BaseColorStyle {
			get { return baseColorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					baseColorStyle = null;
				else baseColorStyle = value;
			}
		}


		[Description("The additional color of the style. Depends on the style's FillMode: For gradients, it is the color of the upper left edge. In case of a pattern, this is the foreground color. For all other fill modes it will be ignored.")]
		[RequiredPermission(Permission.Present)]
		public IColorStyle AdditionalColorStyle {
			get { return additionalColorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					additionalColorStyle = null;
				else additionalColorStyle = value;
			}
		}


		[Description("Specifies the fill mode of the style.")]
		[RequiredPermission(Permission.Present)]
		public FillMode FillMode {
			get { return fillMode; }
			set { fillMode = value; }
		}


		[Description("Specifies the fill pattern of the style. Will be ignored if the fill mode is not pattern.")]
		[RequiredPermission(Permission.Present)]
		public HatchStyle FillPattern {
			get { return fillPattern; }
			set { fillPattern = value; }
		}


		public short GradientAngle {
			get { return gradientAngle; }
		}


		[Description("The Image of the fill style. Will be ignored if the fill mode is not image. Supports both bitmap and vector images.")]
		[RequiredPermission(Permission.Present)]
		[Editor("Dataweb.NShape.WinFormsUI.NamedImageEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public NamedImage Image {
			get { return namedImage; }
			set { namedImage = value; }
		}


		[Description("Layout of the style's image. Will be ignored if the fill mode is not image.")]
		[RequiredPermission(Permission.Present)]
		public ImageLayoutMode ImageLayout {
			get { return imageLayout; }
			set { imageLayout = value; }
		}


		[Description("Transparency of the style's image. Will be ignored if the fill mode is not image.")]
		[RequiredPermission(Permission.Present)]
		public byte ImageTransparency {
			get { return imageTransparency; }
			set {
				if (value < 0 || value > 100)
					throw new NShapeException("The value has to be between 0 and 100.");
				imageTransparency = value;
			}
		}


		/// <summary>
		/// If true, the Image is shown as grayscale image
		/// </summary>
		[Description("Specifies if the style's image should be displayed as grayscale image. Will be ignored if the fill mode is not image.")]
		[RequiredPermission(Permission.Present)]
		public bool ConvertToGrayScale {
			get { return convertToGrayScale; }
			set { convertToGrayScale = value; }
		}


		[Description("Gamma correction for the style's image. Will be ignored if the fill mode is not image.")]
		[RequiredPermission(Permission.Present)]
		public float ImageGammaCorrection {
			get { return imageGamma; }
			set { imageGamma = value; }
		}


		/// <summary>
		/// Quality setting in percentage when compressing the image with a non-lossless encoder.
		/// </summary>
		[Browsable(false)]
		[Description("Specifies the compression quality in percentage when saving the style's image with a lossy compression file format. Will be ignored if the fill mode is not image.")]
		[RequiredPermission(Permission.Present)]
		public byte ImageCompressionQuality {
			get { return imageCompressionQuality; }
			set {
				if (value < 0 || value > 100)
					throw new NShapeException("Value has to be between 0 and 100.");
				imageCompressionQuality = value;
			}
		}


		protected override bool IsStandardName(string name) {
			return FillStyle.StandardNames.EqualsAny(name);
		}


		private void Construct(IColorStyle baseColorStyle, IColorStyle additionalColorStyle) {
			if (baseColorStyle == null) throw new ArgumentNullException("baseColorStyle");
			if (additionalColorStyle == null) throw new ArgumentNullException("additionalColorStyle");
			this.baseColorStyle = baseColorStyle;
			this.additionalColorStyle = additionalColorStyle;
		}


		private void Construct(NamedImage image) {
			if (image == null) throw new ArgumentNullException("image");
			this.namedImage = image;
		}


		static FillStyle() {
			StandardNames = new StandardFillStyleNames();

			Default = new FillStyle(EmptyStyleName);
			Default.AdditionalColorStyle = Dataweb.NShape.ColorStyle.Empty;
			Default.BaseColorStyle = Dataweb.NShape.ColorStyle.Empty;
			Default.ConvertToGrayScale = false;
			Default.FillMode = FillMode.Solid;
			Default.FillPattern = HatchStyle.Cross;
			Default.Image = null;
			Default.ImageLayout = ImageLayoutMode.Original;
		}


		#region Fields

		// Color and Pattern Stuff
		private IColorStyle baseColorStyle = null;
		private IColorStyle additionalColorStyle = null;
		private FillMode fillMode = FillMode.Gradient;
		private HatchStyle fillPattern = HatchStyle.BackwardDiagonal;
		private short gradientAngle = 45;
		// Image Stuff
		private NamedImage namedImage = null;
		private ImageLayoutMode imageLayout = ImageLayoutMode.CenterTile;
		private byte imageTransparency = 0;
		private float imageGamma = 1f;
		private byte imageCompressionQuality = 100;
		private bool convertToGrayScale = false;

		#endregion
	}


	public sealed class LineStyle : Style, ILineStyle {

		public static readonly LineStyle Default;


		public static StandardLineStyleNames StandardNames;


		public LineStyle(string name)
			: base(name) {
		}


		public LineStyle()
			: base() {
		}


		#region IDisposable Members

		public override void Dispose() {
			if (colorStyle != null) {
				colorStyle.Dispose();
				colorStyle = null;
			}
			if (dashPattern != null)
				dashPattern = null;
		}

		#endregion


		#region IPersistable Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			LineWidth = reader.ReadInt32();
			DashType = (DashType)reader.ReadByte();
			DashCap = (DashCap)reader.ReadByte();			// set property instead of member var in order to create DashPattern array
			LineJoin = (LineJoin)reader.ReadByte();
			ColorStyle = (IColorStyle)reader.ReadColorStyle();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32(lineWidth);
			writer.WriteByte((byte)dashStyle);
			writer.WriteByte((byte)dashCap);
			writer.WriteByte((byte)lineJoin);
			writer.WriteStyle(colorStyle);
		}


		public static string EntityTypeName { get { return "Core.LineStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("LineWidth", typeof(int));
			yield return new EntityFieldDefinition("DashType", typeof(byte));
			yield return new EntityFieldDefinition("DashCap", typeof(byte));
			yield return new EntityFieldDefinition("LineJoin", typeof(byte));
			yield return new EntityFieldDefinition("ColorStyle", typeof(object));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is LineStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((LineStyle)style).ColorStyle);
				if (colorStyle != null) this.ColorStyle = colorStyle;
				else this.ColorStyle = ((LineStyle)style).ColorStyle;

				this.DashCap = ((LineStyle)style).DashCap;
				this.DashType = ((LineStyle)style).DashType;
				this.LineJoin = ((LineStyle)style).LineJoin;
				this.LineWidth = ((LineStyle)style).LineWidth;
			} else throw new NShapeException("Style is not of the required type.");
		}


		[Description("Thickness of the line.")]
		[RequiredPermission(Permission.Present)]
		public int LineWidth {
			get { return lineWidth; }
			set {
				if (value <= 0)
					throw new NShapeException("Value has to be greater than 0.");
				lineWidth = value;
			}
		}


		[Description("Shape of the line's edges and caps.")]
		[RequiredPermission(Permission.Present)]
		public LineJoin LineJoin {
			get { return lineJoin; }
			set { lineJoin = value; }
		}


		[Description("Color of the line.")]
		[RequiredPermission(Permission.Present)]
		public IColorStyle ColorStyle {
			get { return colorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					colorStyle = null;
				else colorStyle = value;
			}
		}


		[Description("Dash type of the line.")]
		[RequiredPermission(Permission.Present)]
		public DashType DashType {
			get { return dashStyle; }
			set {
				dashStyle = value;
				switch (dashStyle) {
					case DashType.Solid:
						dashPattern = new float[0];
						break;
					case DashType.Dash:
						dashPattern = new float[2] { lineDashLen, lineDashSpace };
						break;
					case DashType.DashDot:
						dashPattern = new float[4] { lineDashLen, lineDashSpace, lineDotLen, lineDashSpace };
						break;
					case DashType.DashDotDot:
						dashPattern = new float[6] { lineDashLen, lineDashSpace, lineDotLen, lineDotSpace, lineDotLen, lineDashSpace };
						break;
					case DashType.Dot:
						dashPattern = new float[2] { lineDotLen, lineDotSpace };
						break;
					default:
						throw new NShapeUnsupportedValueException(dashStyle);
				}
			}
		}


		[Description("Caps of a dashed line's dashes.")]
		[RequiredPermission(Permission.Present)]
		public DashCap DashCap {
			get { return dashCap; }
			set { dashCap = value; }
		}


		[Browsable(false)]
		public float[] DashPattern {
			get { return dashPattern; }
		}


		protected override bool IsStandardName(string name) {
			return LineStyle.StandardNames.EqualsAny(name);
		}


		static LineStyle() {
			StandardNames = new StandardLineStyleNames();

			Default = new LineStyle(EmptyStyleName);
			Default.ColorStyle = Dataweb.NShape.ColorStyle.Empty;
			Default.DashCap = DashCap.Round;
			Default.DashType = DashType.Solid;
			Default.LineJoin = LineJoin.Round;
			Default.LineWidth = 1;
		}


		#region Fields

		private int lineWidth = 1;
		private IColorStyle colorStyle = null;
		private DashType dashStyle = DashType.Solid;
		private DashCap dashCap = DashCap.Round;
		private LineJoin lineJoin = LineJoin.Round;
		private float[] dashPattern = new float[0];
		// dashpattern defs
		private const float lineDashSpace = 2f;
		private const float lineDashLen = 5f;
		private const float lineDotSpace = 1f;
		private const float lineDotLen = 1f;

		#endregion
	}


	public sealed class ShapeStyle : Style, IShapeStyle {


		public static StandardStyleNames StandardNames;


		public ShapeStyle(string name)
			: base(name) {
		}


		//public ShapeStyle(StandardShapeStyles standardStyleEnumValue)
		//   : base(false, standardStyleEnumValue) {
		//}


		public ShapeStyle()
			: base() {
		}


		#region IDisposable Members

		public override void Dispose() {
			if (shadowColorStyle != null) {
				shadowColorStyle.Dispose();
				shadowColorStyle = null;
			}
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			roundedCorners = reader.ReadBool();
			showGradients = reader.ReadBool();
			showShadows = reader.ReadBool();
			shadowColorStyle = (IColorStyle)reader.ReadColorStyle();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteBool(roundedCorners);
			writer.WriteBool(showGradients);
			writer.WriteBool(showShadows);
			writer.WriteStyle(shadowColorStyle);
		}


		public static string EntityTypeName { get { return "Core.ShapeStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("RoundedCorners", typeof(bool));
			yield return new EntityFieldDefinition("ShowGradients", typeof(bool));
			yield return new EntityFieldDefinition("ShowShadows", typeof(bool));
			yield return new EntityFieldDefinition("ShadowColorStyleName", typeof(string));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is ShapeStyle) {
				base.Assign(style, findStyleCallback);
				this.RoundedCorners = ((ShapeStyle)style).RoundedCorners;

				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((ShapeStyle)style).ShadowColor);
				if (colorStyle != null) this.ShadowColor = colorStyle;
				else this.ShadowColor = ((ShapeStyle)style).ShadowColor;

				this.ShowGradients = ((ShapeStyle)style).ShowGradients;
				this.ShowShadows = ((ShapeStyle)style).ShowShadows;
			} else throw new NShapeException("Style is not of the required type.");
		}


		public bool RoundedCorners {
			get { return roundedCorners; }
			set { roundedCorners = value; }
		}


		public bool ShowGradients {
			get { return showGradients; }
			set { showGradients = value; }
		}


		public bool ShowShadows {
			get { return showShadows; }
			set { showShadows = value; }
		}


		public IColorStyle ShadowColor {
			get { return shadowColorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					shadowColorStyle = null;
				else shadowColorStyle = value;
			}
		}


		protected override bool IsStandardName(string name) {
			throw new NotImplementedException();
		}


		#region Fields

		private bool roundedCorners = false;
		private bool showShadows = true;
		private bool showGradients = true;
		private IColorStyle shadowColorStyle = null;

		#endregion
	}


	public sealed class ParagraphStyle : Style, IParagraphStyle {

		public static readonly ParagraphStyle Default;


		public static StandardParagraphStyleNames StandardNames;


		public ParagraphStyle(string name)
			: base(name) {
		}


		public ParagraphStyle()
			: base() {
		}


		#region IDisposable Members

		public override void Dispose() {
			// nothing to do
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			Alignment = (ContentAlignment)reader.ReadByte();
			Trimming = (StringTrimming)reader.ReadByte();
			Padding = new TextPadding(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			WordWrap = reader.ReadBool();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteByte((byte)alignment);
			writer.WriteByte((byte)trimming);
			writer.WriteInt32(padding.Left);
			writer.WriteInt32(padding.Top);
			writer.WriteInt32(padding.Right);
			writer.WriteInt32(padding.Bottom);
			writer.WriteBool(wordWrap);
		}


		public static string EntityTypeName { get { return "Core.ParagraphStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Alignment", typeof(byte));
			yield return new EntityFieldDefinition("Trimming", typeof(byte));
			yield return new EntityFieldDefinition("PaddingLeft", typeof(int));
			yield return new EntityFieldDefinition("PaddingTop", typeof(int));
			yield return new EntityFieldDefinition("PaddingRight", typeof(int));
			yield return new EntityFieldDefinition("PaddingBottom", typeof(int));
			yield return new EntityFieldDefinition("WordWrap", typeof(bool));
		}

		#endregion


		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is ParagraphStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				this.Alignment = ((ParagraphStyle)style).Alignment;
				this.Padding = ((ParagraphStyle)style).Padding;
				this.Trimming = ((ParagraphStyle)style).Trimming;
				this.WordWrap = ((ParagraphStyle)style).WordWrap;
			} else throw new NShapeException("Style is not of the required type.");
		}


		[Description("Alignment of the text.")]
		[RequiredPermission(Permission.Present)]
		public ContentAlignment Alignment {
			get { return alignment; }
			set { alignment = value; }
		}


		[Description("Trimming of the text if it requires more space than available.")]
		[RequiredPermission(Permission.Present)]
		public StringTrimming Trimming {
			get { return trimming; }
			set { trimming = value; }
		}


		[Description("Distance of the text from its layout rectangle.")]
		[RequiredPermission(Permission.Present)]
		public TextPadding Padding {
			get { return padding; }
			set { padding = value; }
		}


		[Description("Specifies if automatic line breaks should be enabled.")]
		[RequiredPermission(Permission.Present)]
		public bool WordWrap {
			get { return wordWrap; }
			set { wordWrap = value; }
		}


		// public LineNumbering Numbering;
		// public float LineSpacing;


		protected override bool IsStandardName(string name) {
			return ParagraphStyle.StandardNames.EqualsAny(name);
		}


		static ParagraphStyle() {
			StandardNames = new StandardParagraphStyleNames();

			Default = new ParagraphStyle(EmptyStyleName);
			Default.Alignment = ContentAlignment.MiddleCenter;
			Default.Padding = TextPadding.Empty;
			Default.Trimming = StringTrimming.None;
			Default.WordWrap = true;
		}


		#region Fields

		private ContentAlignment alignment = ContentAlignment.MiddleCenter;
		private StringTrimming trimming = StringTrimming.None;
		private TextPadding padding = TextPadding.Empty;
		private bool wordWrap = true;

		#endregion
	}

	#endregion


	#region ***   StyleCollection Classes   ***

	public abstract class StyleCollection<TStyle> where TStyle : class, IStyle {

		public StyleCollection() {
			Construct(-1);
		}


		public StyleCollection(int capacity) {
			Construct(capacity);
		}


		/// <summary>
		/// Indexer for direct access on styles.
		/// </summary>
		/// <param name="index">Zero-based index.</param>
		public TStyle this[int index] {
			get { return internalList[internalList.Keys[index]].Style; }
		}


		/// <summary>
		/// Indexer for direct access on styles.
		/// </summary>
		/// <param name="name">The name of the style.</param>
		public TStyle this[string name] {
			get { return internalList[name].Style; }
		}


		public TStyle GetPreviewStyle(TStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return internalList[style.Name].PreviewStyle;
		}


		public TStyle GetPreviewStyle(string styleName) {
			if (styleName == null) throw new ArgumentNullException("styleName");
			return internalList[styleName].PreviewStyle;
		}


		public void Add(TStyle style, TStyle previewStyle) {
			if (style == null) throw new ArgumentNullException("style");
			if (previewStyle == null) throw new ArgumentNullException("previewStyle");
			internalList.Add(style.Name, new StyleCollection<TStyle>.StylePair<TStyle>(style, previewStyle));
		}


		public int IndexOf(TStyle item) {
			if (item == null) throw new ArgumentNullException("item");
			for (int i = internalList.Values.Count - 1; i >= 0; --i) {
				if (internalList.Values[i].Style == item
					|| internalList.Values[i].PreviewStyle == item)
					return internalList.IndexOfKey(internalList.Values[i].Style.Name);
			}
			return -1;
		}


		public int IndexOf(string styleName) {
			if (styleName == null) throw new ArgumentNullException("styleName");
			return internalList.IndexOfKey(styleName);
		}


		public bool Remove(TStyle item) {
			if (item == null) throw new ArgumentNullException("item");
			return internalList.Remove(item.Name);
		}


		public bool Remove(string styleName) {
			return internalList.Remove(styleName);
		}


		public void RemoveAt(int index) {
			string key = internalList.Keys[index];
			internalList.Remove(key);
		}


		public void Clear() {
			foreach (KeyValuePair<string, StylePair<TStyle>> item in internalList) {
				IStyle baseStyle = item.Value.Style;
				IStyle previewStyle = item.Value.PreviewStyle;
				if (baseStyle != null) baseStyle.Dispose();
				if (previewStyle != null) previewStyle.Dispose();
				baseStyle = previewStyle = null;
			}
			internalList.Clear();
		}


		public bool Contains(TStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			for (int i = internalList.Values.Count - 1; i >= 0; --i) {
				if (internalList.Values[i].Style == style
					|| internalList.Values[i].PreviewStyle == style)
					return true;
			}
			return false;
		}


		public bool Contains(string name) {
			if (name == null) throw new ArgumentNullException("name");
			return internalList.ContainsKey(name);
		}


		public bool ContainsPreviewStyle(TStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			for (int i = internalList.Values.Count - 1; i >= 0; --i) {
				if (internalList.Values[i].Style == style)
					return (internalList.Values[i].PreviewStyle != null);
				else if (internalList.Values[i].PreviewStyle == style)
					return true;
			}
			return false;
		}


		public bool ContainsPreviewStyle(string name) {
			if (name == null) throw new ArgumentNullException("name");
			if (internalList.ContainsKey(name))
				return (internalList[name].PreviewStyle != null);
			else return false;
		}


		public int Count {
			get { return internalList.Count; }
		}


		public abstract bool IsStandardStyle(TStyle style);


		public void SetPreviewStyle(string baseStyleName, TStyle value) {
			if (baseStyleName == null) throw new ArgumentNullException("baseStyle");
			if (value == null) throw new ArgumentNullException("value");
			internalList[baseStyleName].PreviewStyle = value;
		}


		public void SetPreviewStyle(TStyle baseStyle, TStyle value) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (value == null) throw new ArgumentNullException("value");
			internalList[baseStyle.Name].PreviewStyle = value;
		}


		protected void SetStyle(TStyle style, TStyle previewStyle) {
			if (style == null) throw new ArgumentNullException("style");
			if (previewStyle == null) throw new ArgumentNullException("previewStyle");
			internalList[style.Name].Style = style;
			internalList[style.Name].PreviewStyle = previewStyle;
		}


		private void Construct(int capacity) {
			if (capacity > 0)
				internalList = new SortedList<string, StylePair<TStyle>>(capacity);
			else internalList = new SortedList<string, StylePair<TStyle>>();
		}


		protected class StylePair<T> where T : class, IStyle {

			public StylePair(T baseStyle, T previewStyle) {
				this.Style = baseStyle;
				this.PreviewStyle = previewStyle;
			}

			public T Style;

			public T PreviewStyle;

		}


		protected SortedList<string, StylePair<TStyle>> internalList = null;
	}


	public class CapStyleCollection : StyleCollection<CapStyle>, ICapStyles {

		public CapStyleCollection()
			: base(CapStyle.StandardNames.Count) {
		}


		public CapStyleCollection(int capacity)
			: base(capacity) {
		}


		public override bool IsStandardStyle(CapStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return CapStyle.StandardNames.EqualsAny(style.Name);
		}


		#region ICapStyles Members

		ICapStyle ICapStyles.this[string name] {
			get { return internalList[name].Style; }
		}


		public ICapStyle None {
			get { return internalList[CapStyle.StandardNames.None].Style; }
		}


		public ICapStyle Arrow {
			get { return internalList[CapStyle.StandardNames.Arrow].Style; }
		}


		public ICapStyle Special1 {
			get { return internalList[CapStyle.StandardNames.Special1].Style; }
		}


		public ICapStyle Special2 {
			get { return internalList[CapStyle.StandardNames.Special2].Style; }
		}

		#endregion


		#region IEnumerable<ICapStyle> Members

		IEnumerator<ICapStyle> IEnumerable<ICapStyle>.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		private struct Enumerator : IEnumerator<ICapStyle> {

			public static readonly Enumerator Empty;

			public static Enumerator Create(CapStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				Enumerator result = Enumerator.Empty;
				result.collection = collection;
				result.cnt = collection.Count;
				result.idx = -1;
				return result;
			}

			public Enumerator(CapStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this.collection = collection;
				this.cnt = collection.Count;
				this.idx = -1;
			}

			#region IEnumerator<ICapStyle> Members

			public ICapStyle Current {
				get { return collection[idx]; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				collection = null;
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current {
				get { return collection[idx]; }
			}

			public bool MoveNext() {
				return (++idx < cnt);
			}

			public void Reset() {
				idx = -1;
			}

			#endregion

			static Enumerator() {
				Empty.collection = null;
				Empty.cnt = 0;
				Empty.idx = -1;
			}

			private CapStyleCollection collection;
			private int idx;
			private int cnt;
		}
	}


	public class CharacterStyleCollection : StyleCollection<CharacterStyle>, ICharacterStyles {

		public CharacterStyleCollection()
			: base(CharacterStyle.StandardNames.Count) {
		}


		public CharacterStyleCollection(int capacity)
			: base(capacity) {
		}


		public override bool IsStandardStyle(CharacterStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return CharacterStyle.StandardNames.EqualsAny(style.Name);
		}


		#region ICharacterStyles Members

		ICharacterStyle ICharacterStyles.this[string name] {
			get { return this[name]; }
		}


		public ICharacterStyle Normal {
			get { return internalList[CharacterStyle.StandardNames.Normal].Style; }
		}


		public ICharacterStyle Caption {
			get { return internalList[CharacterStyle.StandardNames.Caption].Style; }
		}


		public ICharacterStyle Subtitle {
			get { return internalList[CharacterStyle.StandardNames.Subtitle].Style; }
		}


		public ICharacterStyle Heading3 {
			get { return internalList[CharacterStyle.StandardNames.Heading3].Style; }
		}


		public ICharacterStyle Heading2 {
			get { return internalList[CharacterStyle.StandardNames.Heading2].Style; }
		}


		public ICharacterStyle Heading1 {
			get { return internalList[CharacterStyle.StandardNames.Heading1].Style; }
		}

		#endregion


		#region IEnumerable<ICharacterStyle> Members

		IEnumerator<ICharacterStyle> IEnumerable<ICharacterStyle>.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		private struct Enumerator : IEnumerator<ICharacterStyle> {

			public static readonly Enumerator Empty;

			public static Enumerator Create(CharacterStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				Enumerator result = Enumerator.Empty;
				result.collection = collection;
				result.cnt = collection.Count;
				result.idx = -1;
				return result;
			}

			public Enumerator(CharacterStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this.collection = collection;
				this.cnt = collection.Count;
				this.idx = -1;
			}

			#region IEnumerator<ICapStyle> Members

			public ICharacterStyle Current {
				get { return collection[idx]; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				collection = null;
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current {
				get { return collection[idx]; }
			}

			public bool MoveNext() {
				return (++idx < cnt);
			}

			public void Reset() {
				idx = -1;
			}

			#endregion

			static Enumerator() {
				Empty.collection = null;
				Empty.cnt = 0;
				Empty.idx = -1;
			}

			private CharacterStyleCollection collection;
			private int idx;
			private int cnt;
		}
	}


	public class ColorStyleCollection : StyleCollection<ColorStyle>, IColorStyles {

		public ColorStyleCollection()
			: base(ColorStyle.StandardNames.Count) {
		}


		public ColorStyleCollection(int capacity)
			: base(capacity) {
		}


		public override bool IsStandardStyle(ColorStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return ColorStyle.StandardNames.EqualsAny(style.Name);
		}


		#region IColorStyles Members

		IColorStyle IColorStyles.this[string name] {
			get { return this[name]; }
		}

		public IColorStyle Transparent {
			get { return internalList[ColorStyle.StandardNames.Transparent].Style; }
		}

		public IColorStyle Background {
			get { return internalList[ColorStyle.StandardNames.Background].Style; }
		}

		public IColorStyle Highlight {
			get { return internalList[ColorStyle.StandardNames.Highlight].Style; }
		}

		public IColorStyle Text {
			get { return internalList[ColorStyle.StandardNames.Text].Style; }
		}

		public IColorStyle HighlightText {
			get { return internalList[ColorStyle.StandardNames.HighlightText].Style; }
		}

		public IColorStyle Black {
			get { return internalList[ColorStyle.StandardNames.Black].Style; }
		}

		public IColorStyle White {
			get { return internalList[ColorStyle.StandardNames.White].Style; }
		}

		public IColorStyle Gray {
			get { return internalList[ColorStyle.StandardNames.Gray].Style; }
		}

		public IColorStyle LightGray {
			get { return internalList[ColorStyle.StandardNames.LightGray].Style; }
		}

		public IColorStyle Red {
			get { return internalList[ColorStyle.StandardNames.Red].Style; }
		}

		public IColorStyle LightRed {
			get { return internalList[ColorStyle.StandardNames.LightRed].Style; }
		}

		public IColorStyle Blue {
			get { return internalList[ColorStyle.StandardNames.Blue].Style; }
		}

		public IColorStyle LightBlue {
			get { return internalList[ColorStyle.StandardNames.LightBlue].Style; }
		}

		public IColorStyle Green {
			get { return internalList[ColorStyle.StandardNames.Green].Style; }
		}

		public IColorStyle LightGreen {
			get { return internalList[ColorStyle.StandardNames.LightGreen].Style; }
		}

		public IColorStyle Yellow {
			get { return internalList[ColorStyle.StandardNames.Yellow].Style; }
		}

		public IColorStyle LightYellow {
			get { return internalList[ColorStyle.StandardNames.LightYellow].Style; }
		}

		#endregion


		#region IEnumerable<IColorStyle> Members

		public IEnumerator<IColorStyle> GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		private struct Enumerator : IEnumerator<IColorStyle> {

			public static readonly Enumerator Empty;

			public static Enumerator Create(ColorStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				Enumerator result = Enumerator.Empty;
				result.collection = collection;
				result.cnt = collection.Count;
				result.idx = -1;
				return result;
			}

			public Enumerator(ColorStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this.collection = collection;
				this.cnt = collection.Count;
				this.idx = -1;
			}


			#region IEnumerator<IColorStyle> Members

			public IColorStyle Current {
				get { return collection[idx]; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				collection = null;
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current {
				get { return collection[idx]; }
			}

			public bool MoveNext() {
				return (++idx < cnt);
			}

			public void Reset() {
				idx = -1;
			}

			#endregion

			static Enumerator() {
				Empty.collection = null;
				Empty.idx = -1;
				Empty.cnt = 0;
			}

			private ColorStyleCollection collection;
			private int idx;
			private int cnt;
		}
	}


	public class FillStyleCollection : StyleCollection<FillStyle>, IFillStyles {

		public FillStyleCollection()
			: base(FillStyle.StandardNames.Count) {
		}


		public FillStyleCollection(int capacity)
			: base(capacity) {
		}


		public override bool IsStandardStyle(FillStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return FillStyle.StandardNames.EqualsAny(style.Name);
		}


		#region IFillStyles Members

		IFillStyle IFillStyles.this[string name] {
			get { return this[name]; }
		}

		public IFillStyle Transparent {
			get { return internalList[FillStyle.StandardNames.Transparent].Style; }
		}

		public IFillStyle Black {
			get { return internalList[FillStyle.StandardNames.Black].Style; }
		}

		public IFillStyle White {
			get { return internalList[FillStyle.StandardNames.White].Style; }
		}

		public IFillStyle Red {
			get { return internalList[FillStyle.StandardNames.Red].Style; }
		}

		public IFillStyle Blue {
			get { return internalList[FillStyle.StandardNames.Blue].Style; }
		}

		public IFillStyle Green {
			get { return internalList[FillStyle.StandardNames.Green].Style; }
		}

		public IFillStyle Yellow {
			get { return internalList[FillStyle.StandardNames.Yellow].Style; }
		}

		#endregion


		#region IEnumerable<IFillStyle> Members

		IEnumerator<IFillStyle> IEnumerable<IFillStyle>.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		private struct Enumerator : IEnumerator<IFillStyle> {

			public static readonly Enumerator Empty;

			public static Enumerator Create(FillStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				Enumerator result = Enumerator.Empty;
				result.collection = collection;
				result.cnt = collection.Count;
				result.idx = -1;
				return result;
			}

			public Enumerator(FillStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this.collection = collection;
				this.cnt = collection.Count;
				this.idx = -1;
			}


			#region IEnumerator<ICapStyle> Members

			public IFillStyle Current {
				get { return collection[idx]; }
			}

			#endregion


			#region IDisposable Members

			public void Dispose() {
				collection = null;
			}

			#endregion


			#region IEnumerator Members

			object IEnumerator.Current {
				get { return collection[idx]; }
			}

			public bool MoveNext() {
				return (++idx < cnt);
			}

			public void Reset() {
				idx = -1;
			}

			#endregion


			static Enumerator() {
				Empty.collection = null;
				Empty.cnt = 0;
				Empty.idx = -1;
			}

			private FillStyleCollection collection;
			private int idx;
			private int cnt;
		}
	}


	public class LineStyleCollection : StyleCollection<LineStyle>, ILineStyles {

		public LineStyleCollection()
			: base(LineStyle.StandardNames.Count) {
		}


		public LineStyleCollection(int capacity)
			: base(capacity) {
		}


		public override bool IsStandardStyle(LineStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return LineStyle.StandardNames.EqualsAny(style.Name);
		}


		#region ILineStyles Members

		ILineStyle ILineStyles.this[string name] {
			get { return this[name]; }
		}

		public ILineStyle None {
			get { return internalList[LineStyle.StandardNames.None].Style; }
		}

		public ILineStyle Normal {
			get { return internalList[LineStyle.StandardNames.Normal].Style; }
		}

		public ILineStyle Thick {
			get { return internalList[LineStyle.StandardNames.Thick].Style; }
		}

		public ILineStyle Dotted {
			get { return internalList[LineStyle.StandardNames.Dotted].Style; }
		}

		public ILineStyle Dashed {
			get { return internalList[LineStyle.StandardNames.Dashed].Style; }
		}

		public ILineStyle Highlight {
			get { return internalList[LineStyle.StandardNames.Highlight].Style; }
		}

		public ILineStyle HighlightThick {
			get { return internalList[LineStyle.StandardNames.HighlightThick].Style; }
		}

		public ILineStyle HighlightDotted {
			get { return internalList[LineStyle.StandardNames.HighlightDotted].Style; }
		}

		public ILineStyle HighlightDashed {
			get { return internalList[LineStyle.StandardNames.HighlightDashed].Style; }
		}

		public ILineStyle Red {
			get { return internalList[LineStyle.StandardNames.Red].Style; }
		}

		public ILineStyle Blue {
			get { return internalList[LineStyle.StandardNames.Blue].Style; }
		}

		public ILineStyle Green {
			get { return internalList[LineStyle.StandardNames.Green].Style; }
		}

		public ILineStyle Yellow {
			get { return internalList[LineStyle.StandardNames.Yellow].Style; }
		}

		public ILineStyle Special1 {
			get { return internalList[LineStyle.StandardNames.Special1].Style; }
		}

		public ILineStyle Special2 {
			get { return internalList[LineStyle.StandardNames.Special2].Style; }
		}

		#endregion


		#region IEnumerable<ILineStyle> Members

		IEnumerator<ILineStyle> IEnumerable<ILineStyle>.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		private struct Enumerator : IEnumerator<ILineStyle> {

			public static readonly Enumerator Empty;

			public static Enumerator Create(LineStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				Enumerator result = Enumerator.Empty;
				result.collection = collection;
				result.cnt = collection.Count;
				result.idx = -1;
				return result;
			}

			public Enumerator(LineStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this.collection = collection;
				this.cnt = collection.Count;
				this.idx = -1;
			}

			#region IEnumerator<ICapStyle> Members

			public ILineStyle Current {
				get { return collection[idx]; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				collection = null;
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current {
				get { return collection[idx]; }
			}

			public bool MoveNext() {
				return (++idx < cnt);
			}

			public void Reset() {
				idx = -1;
			}

			#endregion

			static Enumerator() {
				Empty.collection = null;
				Empty.cnt = 0;
				Empty.idx = -1;
			}

			private LineStyleCollection collection;
			private int idx;
			private int cnt;
		}
	}


	public class ShapeStyleCollection : StyleCollection<ShapeStyle>, IEnumerable<IShapeStyle> {

		public override bool IsStandardStyle(ShapeStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return ShapeStyle.StandardNames.EqualsAny(style.Name);
		}

		#region IEnumerable<IShapeStyle> Members

		public IEnumerator<IShapeStyle> GetEnumerator() {
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}

		#endregion
	}


	public class ParagraphStyleCollection : StyleCollection<ParagraphStyle>, IParagraphStyles {

		public ParagraphStyleCollection()
			: base(ParagraphStyle.StandardNames.Count) {
		}


		public ParagraphStyleCollection(int capacity)
			: base(capacity) {
		}


		public override bool IsStandardStyle(ParagraphStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return ParagraphStyle.StandardNames.EqualsAny(style.Name);
		}


		#region IParagraphStyles Members

		IParagraphStyle IParagraphStyles.this[string name] {
			get { return this[name]; }
		}

		public IParagraphStyle Label {
			get { return internalList[ParagraphStyle.StandardNames.Label].Style; }
		}

		public IParagraphStyle Text {
			get { return internalList[ParagraphStyle.StandardNames.Text].Style; }
		}

		public IParagraphStyle Title {
			get { return internalList[ParagraphStyle.StandardNames.Title].Style; }
		}

		#endregion


		#region IEnumerable<IParagraphStyle> Members

		IEnumerator<IParagraphStyle> IEnumerable<IParagraphStyle>.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return Enumerator.Create(this);
		}

		#endregion


		private struct Enumerator : IEnumerator<IParagraphStyle> {

			public static readonly Enumerator Empty;

			public static Enumerator Create(ParagraphStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				Enumerator result = Enumerator.Empty;
				result.collection = collection;
				result.cnt = collection.Count;
				result.idx = -1;
				return result;
			}

			public Enumerator(ParagraphStyleCollection collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this.collection = collection;
				this.cnt = collection.Count;
				this.idx = -1;
			}

			#region IEnumerator<ICapStyle> Members

			public IParagraphStyle Current {
				get { return collection[idx]; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				collection = null;
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current {
				get { return collection[idx]; }
			}


			public bool MoveNext() {
				return (++idx < cnt);
			}


			public void Reset() {
				idx = -1;
			}

			#endregion

			static Enumerator() {
				Empty.collection = null;
				Empty.cnt = 0;
				Empty.idx = -1;
			}

			private ParagraphStyleCollection collection;
			private int idx;
			private int cnt;
		}
	}

	#endregion
}