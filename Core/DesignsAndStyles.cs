using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

using Dataweb.Utilities;
using Dataweb.Diagramming.Core;


namespace Dataweb.Diagramming {

	/// <summary>
	/// Specifies the categoryTitle of a style.
	/// </summary>
	public enum StyleCategory { 
		CapStyle, 
		CharacterStyle, 
		ColorStyle, 
		FillStyle, 
		LineStyle, 
		ParagraphStyle 
	}

	
	/// <summary>
	/// Specifies standard line cap styles that exists in every design.
	/// </summary>
	public enum StandardCapStyle { 
		None = 0, 
		Arrow = 1, 
		Special1 = 2, 
		Special2 = 3
	}


	/// <summary>
	/// Specifies standard character styles that exists in every design.
	/// </summary>
	public enum StandardCharacterStyle { 
		Normal = 0, 
		Caption = 1, 
		Subtitle = 2, 
		Heading3 = 3, 
		Heading2 = 4, 
		Heading1 = 5 
	}
	
	
	/// <summary>
	/// Specifies standard color styles that exist in every design.
	/// </summary>
	public enum StandardColorStyle { 
		Transparent = 0, 
		Background = 1, 
		Highlight = 2, 
		Text = 3, 
		HighlightText = 4, 
		Black = 5, 
		White = 6, 
		Gray = 7, 
		LightGray = 8, 
		Red = 9, 
		LightRed = 10, 
		Blue = 11, 
		LightBlue = 12, 
		Green = 13, 
		LightGreen = 14, 
		Yellow = 15, 
		LightYellow = 16
	}
	
	
	/// <summary>
	/// Specifies standard styles that exist in every design.
	/// </summary>
	public enum StandardFillStyle { 
		Transparent = 0,
		Black = 1, 
		White = 2, 
		Red = 3, 
		Blue = 4, 
		Green = 5, 
		Yellow = 6 
	}
	
	
	public enum StandardLineStyle {
		None = 0,
		Normal = 1,
		Thick = 2, 
		Dotted = 3, 
		Dashed = 4, 
		Highlight = 5, 
		HighlightThick = 6, 
		HighlightDotted = 7, 
		HighlightDashed = 8, 
		Red = 9, 
		Blue = 10, 
		Green = 11, 
		Yellow = 12, 
		Special1 = 13, 
		Special2 = 14
	}
	
	
	public enum StandardParagraphStyle { 
		Label = 0, 
		Text = 1,
		Title = 2
	}
	

	public enum FillMode { Solid, Gradient, Pattern, Image }	
	
	
	public enum CapShape { None, ArrowClosed, ArrowOpen, Circle, Triangle, Diamond, Square, CenteredCircle, CenteredHalfCircle }
	
	
	public enum DashType { Solid, Dash, Dot, DashDot, DashDotDot }

	[TypeConverter("Dataweb.Diagramming.WinFormsUI.DiagrammingTextPaddingConverter, Dataweb.Diagramming.WinFormsUI")]
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
	/// Returns the style of the same type with the same projectName if there is one in the design's style collection.
	/// </summary>
	public delegate IStyle FindStyleCallback(IStyle style);


	public delegate Style CreatePreviewStyleCallback(IStyle style);


	public interface IStyleSet {

		ICapStyle GetCapStyle(StandardCapStyle standardColorStyle);
		ICharacterStyle GetCharacterStyle(StandardCharacterStyle standardColorStyle);
		IColorStyle GetColorStyle(StandardColorStyle standardColorStyle);
		IFillStyle GetFillStyle(StandardFillStyle standardColorStyle);
		ILineStyle GetLineStyle(StandardLineStyle standardColorStyle);
		IParagraphStyle GetParagraphStyle(StandardParagraphStyle standardColorStyle);

		ICapStyle GetCapStyle(string name);
		ICharacterStyle GetCharacterStyle(string name);
		IColorStyle GetColorStyle(string name);
		IFillStyle GetFillStyle(string name);
		ILineStyle GetLineStyle(string name);
		IParagraphStyle GetParagraphStyle(string name);

		ICapStyle GetPreviewStyle(ICapStyle colorStyle);
		ICharacterStyle GetPreviewStyle(ICharacterStyle colorStyle);
		IColorStyle GetPreviewStyle(IColorStyle colorStyle);
		IFillStyle GetPreviewStyle(IFillStyle fillStyle);
		ILineStyle GetPreviewStyle(ILineStyle lineStyle);
		IParagraphStyle GetPreviewStyle(IParagraphStyle colorStyle);

		IEnumerable<ICapStyle> CapStyles { get; }
		IEnumerable<ICharacterStyle>CharacterStyles { get; }
		IEnumerable<IColorStyle> ColorStyles { get; }
		IEnumerable<IFillStyle> FillStyles { get; }
		IEnumerable<ILineStyle> LineStyles { get; }
		IEnumerable<IParagraphStyle> ParagraphStyles { get; }
	}


	#region ***   Style Interfaces   ***

	[TypeConverter("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleConverter, Dataweb.Diagramming.WinFormsUI")]
	public interface IStyle : IEntity, IDisposable {
		[Browsable(false)]
		string Name { get; }
		string Title { get; }
		bool IsPreviewStyle { get; }
		string ToString();
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
	public interface ICapStyle : IStyle {
		CapShape CapShape { get; }
		short CapSize { get; }
		IColorStyle ColorStyle { get; }
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
	public interface IColorStyle : IStyle {
		Color Color { get; }
		byte Transparency { get; }	// percentage (range 0 to 100)
		bool ConvertToGray { get; }
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
	public interface IFillStyle : IStyle {
		IColorStyle BaseColorStyle { get; }
		IColorStyle AdditionalColorStyle { get; }
		FillMode FillMode { get; }
		HatchStyle FillPattern { get; }
		short GradientAngle { get; }
		bool ConvertToGrayScale { get; }
		NamedImage Image { get; }
		DiagrammingImageLayout ImageLayout { get; }
		byte ImageTransparency { get; }
		float ImageGammaCorrection { get; }
		byte ImageCompressionQuality { get; }
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
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
		float Size { get; }
		
		/// <summary>
		/// The character's style
		/// </summary>
		FontStyle Style { get; }
		
		// The character's color
		IColorStyle ColorStyle { get; }
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
	public interface ILineStyle : IStyle {
		int LineWidth { get; }
		IColorStyle ColorStyle { get; }
		DashType DashType { get; }
		DashCap DashCap { get; }
		float[] DashPattern { get; }
		LineJoin LineJoin { get; }
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
	public interface IShapeStyle : IStyle {
		bool RoundedCorners { get; }
		bool ShowGradients { get; }
		bool ShowShadows { get; }
		IColorStyle ShadowColor { get; }
		//bool ThreeD { get; }
	}


	[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingStyleEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
	public interface IParagraphStyle : IStyle {
		ContentAlignment Alignment { get; }
		StringTrimming Trimming { get; }
		TextPadding Padding { get; }
		bool WordWrap { get; }
		// LineNumbering Numbering { get; }
		// float LineSpacing { get; }
	}

	#endregion


	#region *** EditableStyle Interfaces ***

	// Editable style interfaces take the editing capabilities away from consumers
	// of the regular interfaces.

	public interface IEditableStyle {
		string Name { set; }
		string Title { set; }
	}


	public interface IEditableCapStyle : IStyle {
		CapShape CapShape { set; }
		short CapSize { set; }
		IColorStyle ColorStyle { set; }
	}


	public interface IEditableColorStyle : IStyle {
		Color Color { set; }
		byte Transparency { set; }	// percentage (range 0 to 100)
		bool ConvertToGray { set; }
	}


	public interface IEditableFillStyle : IStyle {
		IColorStyle BaseColorStyle { set; }
		IColorStyle AdditionalColorStyle { set; }
		FillMode FillMode { set; }
		HatchStyle FillPattern { set; }
		//short GradientAngle { set; }
		bool ConvertToGrayScale { set; }
		NamedImage Image { set; }
		DiagrammingImageLayout ImageLayout { set; }
		byte ImageTransparency { set; }
		float ImageGammaCorrection { set; }
		byte ImageCompressionQuality { set; }
	}


	public interface IEditableCharacterStyle : IStyle {
		string FontName { set; }
		FontFamily FontFamily { set; }
		float Size { set; }
		FontStyle Style { set; }
		IColorStyle ColorStyle { set; }
	}


	public interface IEditableLineStyle : IStyle {
		int LineWidth { set; }
		IColorStyle ColorStyle { set; }
		DashType DashType { set; }
		DashCap DashCap { set; }
		LineJoin LineJoin { set; }
	}


	public interface IEditableShapeStyle : IStyle {
		bool RoundedCorners { set; }
		bool ShowGradients { set; }
		bool ShowShadows { set; }
		IColorStyle ShadowColor { set; }
		//bool ThreeD { set; }
	}


	public interface IEditableParagraphStyle : IStyle {
		ContentAlignment Alignment { set; }
		StringTrimming Trimming { set; }
		TextPadding Padding { set; }
		bool WordWrap { set; }
		// LineNumbering Numbering { set; }
		// float LineSpacing { set; }
	}

	#endregion


	# region ***   Style Classes   ***

	public abstract class Style : IStyle, IEditableStyle {

		protected Style(bool isPreviewStyle) {
			this.isPreviewStyle = isPreviewStyle;
		}


		protected Style(bool isPreviewStyle, string name) {
			this.name = name;
			this.title = string.Empty;
			this.isPreviewStyle = isPreviewStyle;
		}


		protected Style(bool isPreviewStyle, Enum value) {
			this.name = value.ToString();
			this.title = value.ToString();
			this.isStandardStyle = true;
			this.isPreviewStyle = isPreviewStyle;
		}


		protected Style() : this(false) { }


		protected Style(string name) : this(false, name) { }


		protected Style(Enum value) : this(false, value) { }


		#region IDisposable Members

		public abstract void Dispose();

		#endregion


		#region IPersistable Members

		[Browsable(false)]
		public object Id { get { return id; } }


		public void AssignId(object id) {
			this.id = id;
		}


		public virtual void LoadFields(IRepositoryReader reader, int version) {
			name = reader.ReadString();
		}


		public virtual void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		public virtual void SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteString(name);
		}


		public virtual void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		public virtual void Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyInfos()) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}


		public static IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			yield return new EntityFieldDefinition("Name", typeof(string));
		}

		#endregion


		public virtual void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (this.Name != style.Name)
				this.Name = style.Name;
			this.Title = style.Title;
		}


		public string Name {
			get { return name; }
			set {
				if (isStandardStyle) throw new DiagrammingException("This style is a default style and must not be renamed.");
				name = value;
			}
		}


		public string Title {
			get {
				if (title == null || title == string.Empty)
					return name;
				else return title;
			}
			set { title = value; }
		}


		[Browsable(false)]
		public bool IsStandardStyle { get { return isStandardStyle; } }


		[Browsable(false)]
		public bool IsPreviewStyle { get { return isPreviewStyle; } }


		public override string ToString() {
			return title;
		}


		#region Fields
		private object id = null;
		private string name = string.Empty;
		private string title = string.Empty;
		private bool isStandardStyle = false;
		private bool isPreviewStyle = false;
		#endregion
	}


	public class CapStyle : Style, ICapStyle, IEditableCapStyle {

		public CapStyle(string name) : this(false, name) { }


		public CapStyle(StandardCapStyle standardStyleEnumValue) : this(false, standardStyleEnumValue) { }


		public CapStyle() : this(false) { }


		public CapStyle(bool isPreviewStyle, string name) : base(isPreviewStyle, name) { }


		public CapStyle(bool isPreviewStyle, StandardCapStyle standardStyleEnumValue) : base(isPreviewStyle, standardStyleEnumValue) { }


		public CapStyle(bool isPreviewStyle) : base(isPreviewStyle) { }


		#region IDisposable Members

		public override void Dispose() {
			colorStyle.Dispose();
			colorStyle = null;
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


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyInfos())
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
			}
			else throw new DiagrammingException("Style is not of the required type.");
		}


		public CapShape CapShape {
			get { return capShape; }
			set { capShape = value; }
		}


		public short CapSize {
			get { return capSize; }
			set {
				if (value < 0)
					throw new DiagrammingException("Value has to be greater than 0.");
				capSize = value;
			}
		}


		public IColorStyle ColorStyle {
			get { return colorStyle; }
			set { colorStyle = value; }
		}


		protected virtual int GetCapShapePointCount(CapShape capShape) {
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
					throw new DiagrammingException(string.Format("NotSupported CapShape {0}.", capShape));
			}
		}


		#region Fields

		private CapShape capShape = CapShape.None;
		private short capSize = 10;
		private IColorStyle colorStyle = Dataweb.Diagramming.ColorStyle.Empty;

		#endregion
	}


	public class CharacterStyle : Style, ICharacterStyle, IEditableCharacterStyle {

		public CharacterStyle(bool isPreviewStyle, string name)
			: base(isPreviewStyle, name) {
			fontFamily = FindFontFamily("Arial");
		}


		public CharacterStyle(bool isPreviewStyle, StandardCharacterStyle standardStyleEnumValue)
			: base(isPreviewStyle, standardStyleEnumValue) {
			fontFamily = FindFontFamily("Arial");
		}


		public CharacterStyle(bool isPreviewStyle)
			: base(isPreviewStyle) {
			fontFamily = FindFontFamily("Arial");
		}


		public CharacterStyle(string name) : this(false, name) { }


		public CharacterStyle(StandardCharacterStyle standardStyleEnumValue) : this(false, standardStyleEnumValue) { }


		public CharacterStyle() : this(false) { }


		#region IDisposable Members

		public override void Dispose() {
			colorStyle.Dispose();
			colorStyle = null;
		}

		#endregion


		#region IEntity Members

		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			fontFamily = FindFontFamily(reader.ReadString());
			fontSize = reader.ReadInt32() / 100;
			fontStyle = (FontStyle)reader.ReadByte();
			colorStyle = (IColorStyle)reader.ReadColorStyle();
		}


		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteString(fontFamily.Name);
			writer.WriteInt32((int)(100 * fontSize));
			writer.WriteByte((byte)fontStyle);
			writer.WriteStyle(colorStyle);
		}


		public static string EntityTypeName { get { return "Core.CharacterStyle"; } }


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Dataweb.Diagramming.Style.GetPropertyInfos())
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
				this.Size = ((CharacterStyle)style).Size;
				this.Style = ((CharacterStyle)style).Style;
			}
			else throw new DiagrammingException("Style is not of the required type.");
		}


		[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingFontFamilyEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
		public string FontName {
			get { return fontFamily.Name; }
			set { fontFamily = FindFontFamily(value); }
		}


		[Browsable(false)]
		public FontFamily FontFamily {
			get { return fontFamily; }
			set { fontFamily = value; }
		}


		/// <summary>
		/// Font Size in Vertices (1/72 Inch)
		/// </summary>
		public float Size {
			get { return fontSize; }
			set { fontSize = value; }
		}


		public FontStyle Style {
			get { return fontStyle; }
			set {
				if (value == FontStyle.Regular)
					fontStyle = FontStyle.Regular;
				else
					fontStyle = fontStyle ^ value;
			}
		}


		public IColorStyle ColorStyle {
			get { return colorStyle; }
			set { colorStyle = value; }
		}


		private FontFamily FindFontFamily(string fontName) {
			FontFamily[] families = FontFamily.Families;
			foreach (FontFamily ff in families) {
				if (ff.Name.Equals(fontName, StringComparison.InvariantCultureIgnoreCase)) 
					return ff;
			}
			throw new DiagrammingException(string.Format("'{0}' is not a valid font name name or font is not installed on this machine.", fontName));
		}
		
		 
		/*private FontFamily FindFontFamily(string fontName) {
			int cnt = FontFamily.Families.Length;
			for (int i = 0; i < cnt; ++i) {
				if (FontFamily.Families[i].Name.Equals(fontName, StringComparison.InvariantCultureIgnoreCase)) {
					return FontFamily.Families[i];
				}
			}
			throw new DiagrammingException(string.Format("'{0}' is not a valid font projectName projectName or font is not installed on this machine.", fontName));
		}*/


		#region Fields
		private float fontSize = 8.25f;
		private FontStyle fontStyle = 0;
		private FontFamily fontFamily = null;
		private IColorStyle colorStyle = Dataweb.Diagramming.ColorStyle.Empty;
		#endregion
	}


	public class ColorStyle : Style, IColorStyle, IEditableColorStyle {

		static ColorStyle() {
			Empty = new ColorStyle("Empty", Color.Empty);
		}


		public static readonly IColorStyle Empty;


		public ColorStyle(bool isPreviewStyle, string name, string title, Color color, byte transparency)
			: base(isPreviewStyle, name) {
			this.Title = title;
			this.transparency = transparency;
			this.color = Color.FromArgb(TransparencyToAlpha(transparency), color);
		}


		public ColorStyle(bool isPreviewStyle, StandardColorStyle standardStyleEnumValue, string title, Color color, byte transparency)
			: base(isPreviewStyle, standardStyleEnumValue) {
			this.Title = title;
			this.transparency = transparency;
			this.color = Color.FromArgb(TransparencyToAlpha(transparency), color);
		}


		public ColorStyle(bool isPreviewStyle, string name, Color color, byte transparency)
			: base(isPreviewStyle, name) {
			this.transparency = transparency;
			this.color = Color.FromArgb(TransparencyToAlpha(transparency), color);
		}


		public ColorStyle(bool isPreviewStyle, StandardColorStyle standardStyleEnumValue, Color color, byte transparency)
			: base(isPreviewStyle, standardStyleEnumValue) {
			this.transparency = transparency;
			this.color = Color.FromArgb(TransparencyToAlpha(transparency), color);
		}


		public ColorStyle(bool isPreviewStyle, string name, Color color)
			: base(isPreviewStyle, name) {
			this.color = color;
			this.transparency = AlphaToTransparency(color.A);
		}


		public ColorStyle(bool isPreviewStyle, StandardColorStyle standardStyleEnumValue, Color color)
			: base(isPreviewStyle, standardStyleEnumValue) {
			this.color = color;
			this.transparency = AlphaToTransparency(color.A);
		}


		public ColorStyle(bool isPreviewStyle, string name)
			: base(isPreviewStyle, name) {
		}


		public ColorStyle(bool isPreviewStyle, StandardColorStyle standardStyleEnumValue)
			: base(isPreviewStyle, standardStyleEnumValue) {
		}


		public ColorStyle(bool isPreviewStyle)
			: base(isPreviewStyle) {
		}


		public ColorStyle(string name, string title, Color color, byte transparency)
			: this(false, name, title, color, transparency) {
		}


		public ColorStyle(StandardColorStyle standardStyleEnumValue, string title, Color color, byte transparency)
			: this(false, standardStyleEnumValue, title, color, transparency) {
		}


		public ColorStyle(string name, Color color, byte transparency)
			: this(false, name, color, transparency) {
		}


		public ColorStyle(StandardColorStyle standardStyleEnumValue, Color color, byte transparency)
			: this(false, standardStyleEnumValue, color, transparency) {
		}


		public ColorStyle(string name, Color color)
			: this(false, name, color) {
		}


		public ColorStyle(StandardColorStyle standardStyleEnumValue, Color color)
			: this(false, standardStyleEnumValue, color) {
		}


		public ColorStyle(string name)
			: this(false, name) {
		}


		public ColorStyle(StandardColorStyle standardStyleEnumValue)
			: this(false, standardStyleEnumValue) {
		}


		public ColorStyle()
			: this(false) {
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


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyInfos())
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
			}
			else throw new DiagrammingException("Style is not of the required type.");
		}


		private byte AlphaToTransparency(byte alpha) {
			return (byte)(100 - Math.Round(alpha / 2.55f));
		}


		private byte TransparencyToAlpha(byte transparency) {
			if (transparency < 0 || transparency > 100)
				throw new DiagrammingException("Value has to be between 0 and 100.");
			return Convert.ToByte(255 - (transparency * 2.55f));
		}


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
		public byte Transparency {
			get { return transparency; }
			set {
				if (value < 0 || value > 100) throw new DiagrammingException("Value has to be between 0 and 100.");
				transparency = value;
				color = Color.FromArgb(TransparencyToAlpha(transparency), color);
			}
		}


		[Browsable(false)]
		public bool ConvertToGray {
			get { return convertToGray; }
			set { convertToGray = value; }
		}


		#region Fields
		private Color color = Color.White;
		private byte transparency = 0;
		private bool convertToGray = false;
		#endregion
	}


	public class FillStyle : Style, IFillStyle, IEditableFillStyle {

		public FillStyle(bool isPreviewStyle, string name)
			: base(isPreviewStyle, name) {
		}


		public FillStyle(bool isPreviewStyle, StandardFillStyle standardStyleEnumValue)
			: base(isPreviewStyle, standardStyleEnumValue) {
		}


		public FillStyle(bool isPreviewStyle)
			: base(isPreviewStyle) {
		}


		public FillStyle(string name)
			: this(false, name) {
		}


		public FillStyle(StandardFillStyle standardStyleEnumValue)
			: this(false, standardStyleEnumValue) {
		}


		public FillStyle()
			: this(false) {
		}


		#region IDisposable Members

		public override void Dispose() {
			baseColorStyle.Dispose();
			additionalColorStyle.Dispose();
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
			imageLayout = (DiagrammingImageLayout)reader.ReadByte();
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


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyInfos())
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
			}
			else throw new DiagrammingException("Style is not of the required Type");
		}


		public IColorStyle BaseColorStyle {
			get { return baseColorStyle; }
			set { baseColorStyle = value; }
		}


		public IColorStyle AdditionalColorStyle {
			get { return additionalColorStyle; }
			set { additionalColorStyle = value; }
		}


		public FillMode FillMode {
			get { return fillMode; }
			set { fillMode = value; }
		}


		public HatchStyle FillPattern {
			get { return fillPattern; }
			set { fillPattern = value; }
		}


		public short GradientAngle {
			get { return gradientAngle; }
		}


		[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingNamedImageEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
		public NamedImage Image {
			get { return namedImage; }
			set { namedImage = value; }
		}


		public DiagrammingImageLayout ImageLayout {
			get { return imageLayout; }
			set { imageLayout = value; }
		}


		public byte ImageTransparency {
			get { return imageTransparency; }
			set {
				if (value < 0 || value > 100)
					throw new DiagrammingException("The value has to be between 0 and 100.");
				imageTransparency = value;
			}
		}


		/// <summary>
		/// If true, the Image is shown as grayscale image
		/// </summary>
		[Browsable(false)]
		public bool ConvertToGrayScale {
			get { return convertToGrayScale; }
			set { convertToGrayScale = value; }
		}


		public float ImageGammaCorrection {
			get { return imageGamma; }
			set { imageGamma = value; }
		}


		/// <summary>
		/// Quality setting in percentage when compressing the image with a non-lossless encoder.
		/// </summary>
		public byte ImageCompressionQuality {
			get { return imageCompressionQuality; }
			set {
				if (value < 0 || value > 100)
					throw new DiagrammingException("Value has to be between 0 and 100.");
				imageCompressionQuality = value;
			}
		}


		#region Fields

		// Color and Pattern Stuff
		private IColorStyle baseColorStyle = Dataweb.Diagramming.ColorStyle.Empty;
		private IColorStyle additionalColorStyle = Dataweb.Diagramming.ColorStyle.Empty;
		private FillMode fillMode = FillMode.Gradient;
		private HatchStyle fillPattern = HatchStyle.BackwardDiagonal;
		private short gradientAngle = 45;
		// Image Stuff
		private NamedImage namedImage = null;
		private DiagrammingImageLayout imageLayout = DiagrammingImageLayout.CenterTile;
		private byte imageTransparency = 0;
		private float imageGamma = 1f;
		private byte imageCompressionQuality = 100;
		private bool convertToGrayScale = false;

		#endregion
	}


	public class LineStyle : Style, ILineStyle, IEditableLineStyle {

		public LineStyle(bool isPreviewStyle, string name)
			: base(isPreviewStyle, name) {
		}


		public LineStyle(bool isPreviewStyle, StandardLineStyle standardStyleEnumValue)
			: base(isPreviewStyle, standardStyleEnumValue) {
		}


		public LineStyle(bool isPreviewStyle)
			: base(isPreviewStyle) {
		}


		public LineStyle(string name)
			: this(false, name) {
		}


		public LineStyle(StandardLineStyle standardStyleEnumValue)
			: this(false, standardStyleEnumValue) {
		}


		public LineStyle()
			: this(false) {
		}


		#region IDisposable Members

		public override void Dispose() {
			colorStyle.Dispose();
			colorStyle = null;
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


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyInfos())
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
			}
			else throw new DiagrammingException("Style is not of the required type.");
		}


		public int LineWidth {
			get { return lineWidth; }
			set {
				if (lineWidth <= 0)
					throw new DiagrammingException("Value has to be greater than 0.");
				lineWidth = value;
			}
		}


		public LineJoin LineJoin {
			get { return lineJoin; }
			set { lineJoin = value; }
		}


		public IColorStyle ColorStyle {
			get { return colorStyle; }
			set { colorStyle = value; }
		}


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
						throw new DiagrammingException(string.Format("Unexpected DashStyle value '{0}'", dashStyle));
				}
			}
		}


		public DashCap DashCap {
			get { return dashCap; }
			set { dashCap = value; }
		}


		[Browsable(false)]
		public float[] DashPattern {
			get { return dashPattern; }
		}


		#region Fields

		private int lineWidth = 1;
		private IColorStyle colorStyle = Dataweb.Diagramming.ColorStyle.Empty;
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


	public class ShapeStyle : Style, IShapeStyle, IEditableShapeStyle {

		public ShapeStyle(string name)
			: base(false, name) {
		}


		//public ShapeStyle(StandardShapeStyles standardStyleEnumValue)
		//   : base(false, standardStyleEnumValue) {
		//}


		public ShapeStyle()
			: base(false) {
		}


		#region IDisposable Members

		public override void Dispose() {
			// nothing to do
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


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyInfos())
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
			}
			else throw new DiagrammingException("Style is not of the required type.");
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
			get { return shadowColorStyle; }
			set { shadowColorStyle = value; }
		}


		#region Fields

		private bool roundedCorners = false;
		private bool showShadows = true;
		private bool showGradients = true;
		private IColorStyle shadowColorStyle = Dataweb.Diagramming.ColorStyle.Empty;

		#endregion
	}


	public class ParagraphStyle : Style, IParagraphStyle, IEditableParagraphStyle {

		public ParagraphStyle(bool isPreviewStyle, string name)
			: base(isPreviewStyle, name) {
		}


		public ParagraphStyle(bool isPreviewStyle, StandardParagraphStyle standardStyleEnumValue)
			: base(isPreviewStyle, standardStyleEnumValue) {
		}


		public ParagraphStyle(bool isPreviewStyle)
			: base(isPreviewStyle) {
		}


		public ParagraphStyle(string name)
			: this(false, name) {
		}


		public ParagraphStyle(StandardParagraphStyle standardStyleEnumValue)
			: this(false, standardStyleEnumValue) {
		}


		public ParagraphStyle()
			: this(false) {
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


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyInfos())
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
			}
			else throw new DiagrammingException("Style is not of the required type.");
		}


		public ContentAlignment Alignment {
			get { return alignment; }
			set { alignment = value; }
		}


		public StringTrimming Trimming {
			get { return trimming; }
			set { trimming = value; }
		}


		public TextPadding Padding {
			get { return padding; }
			set { padding = value; }
		}


		public bool WordWrap {
			get { return wordWrap; }
			set { wordWrap = value; }
		}


		// public LineNumbering Numbering;
		// public float LineSpacing;

		#region Fields

		private ContentAlignment alignment = ContentAlignment.MiddleCenter;
		private StringTrimming trimming = StringTrimming.None;
		private TextPadding padding = TextPadding.Empty;
		private bool wordWrap = true;

		#endregion
	}

	#endregion


	public class IColorStyleEnumerator : IEnumerator<IColorStyle> {

		public IColorStyleEnumerator(ColorStyleCollection collection) {
			this.collection = collection;
			this.cnt = collection.Count;
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

		private ColorStyleCollection collection;
		private int idx = -1;
		private int cnt = 0;
	}


	public class ICapStyleEnumerator : IEnumerator<ICapStyle> {
		
		public ICapStyleEnumerator(CapStyleCollection collection) {
			this.collection = collection;
			this.cnt = collection.Count;
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

		private CapStyleCollection collection;
		private int idx = -1;
		private int cnt = 0;
	}


	public class ICharacterStyleEnumerator : IEnumerator<ICharacterStyle> {
		
		public ICharacterStyleEnumerator(CharacterStyleCollection collection) {
			this.collection = collection;
			this.cnt = collection.Count;
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

		private CharacterStyleCollection collection;
		private int idx = -1;
		private int cnt = 0;
	}


	public class IFillStyleEnumerator : IEnumerator<IFillStyle> {
		
		public IFillStyleEnumerator(FillStyleCollection collection) {
			this.collection = collection;
			this.cnt = collection.Count;
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

		private FillStyleCollection collection;
		private int idx = -1;
		private int cnt = 0;
	}


	public class ILineStyleEnumerator : IEnumerator<ILineStyle> {
		
		public ILineStyleEnumerator(LineStyleCollection collection) {
			this.collection = collection;
			this.cnt = collection.Count;
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

		private LineStyleCollection collection;
		private int idx = -1;
		private int cnt = 0;
	}


	public class IParagraphStyleEnumerator : IEnumerator<IParagraphStyle> {

		public IParagraphStyleEnumerator(ParagraphStyleCollection collection) {
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

		private ParagraphStyleCollection collection;
		private int idx;
		private int cnt;
	}


	public class StyleCollection<TStyle> where TStyle : class, IStyle {

		public StyleCollection() {
			styles = new List<TStyle>();
			previewStyles = new List<TStyle>();
		}


		public StyleCollection(int capacity) {
			styles = new List<TStyle>(capacity);
			previewStyles = new List<TStyle>(capacity);
		}

		
		public TStyle this[int index] {
			get { return styles[index]; }
			protected set { styles[index] = value; }
		}


		public TStyle this[string name] {
			get {
				int cnt = styles.Count;
				for (int i = 0; i<cnt; ++i)
					if (styles[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
						return styles[i];
				return styles[0];
			}
			//protected set {
			//   int cnt = styles.Count;
			//   int i = -1;
			//   for (int i = 0; i < cnt; ++i) {
			//      if (styles[i].Name == projectName) {
			//         i = i;
			//         break;
			//      }
			//   }
			//   if (i < 0)
			//      styles.Add(value);
			//   else styles[i] = value;
			//}
		}


		public TStyle GetPreviewStyle(TStyle style) {
			int idx = styles.IndexOf(style);
			if (idx < 0) {
				// if the base style was not found, check if the style is a PreviewStyle
				// This happens if a Preview of a Preview is created, e.g. when creating previews of lines connected to each other
				if (previewStyles.Contains(style)) return style;
				else throw new DiagrammingException("Style '{0}' is not part of the current design.", style.Name);
			}
			// If there is no PreviewStyle for the given style, return the style itself
			if (previewStyles[idx] == null) return styles[idx];
			else return previewStyles[idx];
		}


		public TStyle GetPreviewStyle(string styleName) {
			return GetPreviewStyle(this[styleName]);
		}


		public TStyle GetPreviewStyle(int styleIndex) {
			if (previewStyles[styleIndex] == null)
				return styles[styleIndex];
			else
				return previewStyles[styleIndex];
		}


		public void SetPreviewStyle(int index, TStyle value) {
			if (index < 0 || index > Count) throw new ArgumentOutOfRangeException("index");
			if (previewStyles[index] != null) previewStyles[index].Dispose();
			previewStyles[index] = value;
		}


		public void SetPreviewStyle(TStyle baseStyle, TStyle value) {
			int index = styles.IndexOf(baseStyle);
			SetPreviewStyle(index, value);
		}


		public void SetPreviewStyle(string name, TStyle value) {
			SetPreviewStyle(IndexOf(name), value);
		}


		public void SetStyle(int index, TStyle style, TStyle previewStyle) {
			while (styles.Count <= index) styles.Add(null);
			while (previewStyles.Count <= index) previewStyles.Add(null);
			styles[index] = style;
			previewStyles[index] = previewStyle;
		}


		public void Add(TStyle style, TStyle previewStyle) {
			styles.Add(style);
			previewStyles.Add(previewStyle);
		}


		public void Insert(int index, TStyle style, TStyle previewStyle) {
			styles.Insert(index, style);
			previewStyles.Insert(index, previewStyle);
		}


		public void Remove(TStyle item) {
			int idx = styles.IndexOf(item);
			if (idx < 0)
				throw new DiagrammingException(string.Format("Style '{0}' does not exist in the collection.", item.Name));
			styles.RemoveAt(idx);
			previewStyles.RemoveAt(idx);
		}


		public void RemoveAt(int index) {
			styles.RemoveAt(index);
			previewStyles.RemoveAt(index);
		}


		public void Clear() {
			for (int i = 0; i < styles.Count; ++i)
				styles[i].Dispose();
			styles.Clear();
			for (int i = 0; i < previewStyles.Count; ++i)
				previewStyles[i].Dispose();
			previewStyles.Clear();
		}


		public bool Contains(TStyle style) {
			if (style.IsPreviewStyle)
				return previewStyles.Contains(style);
			else return styles.Contains(style);
		}


		public bool Contains(string name) {
			int cnt = styles.Count;
			for (int i = 0; i < cnt; ++i)
				if (styles[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return true;
			return false;
		}


		//public bool ContainsPreview(TStyle previewStyle) {
		//   return previewStyles.Contains(previewStyle);
		//}


		public int IndexOf(TStyle style) {
			return styles.IndexOf(style);
		}


		public int IndexOfPreview(TStyle previewStyle) {
			return previewStyles.IndexOf(previewStyle);
		}


		public int IndexOf(string name) {
			int cnt = styles.Count;
			for (int i = 0; i < cnt; ++i)
				if (styles[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return i;
			return -1;
		}


		public int Count {
			get { return styles.Count; }
		}


		protected void SetStandardStyle(int index, TStyle style, TStyle previewStyle) {
			if (index < Count && this[index] != null)
				throw new DiagrammingException("A standard style for '{0}' already exists.", style.Name);
			while (Count <= index) styles.Add(null);
			SetStyle(index, style, previewStyle);
		}
		

		#region Fields
		private List<TStyle> styles;
		private List<TStyle> previewStyles;
		#endregion
	}


	public class ColorStyleCollection : StyleCollection<ColorStyle>, IEnumerable<IColorStyle> {

		public ColorStyleCollection()
			: base() {
		}


		public ColorStyle this[StandardColorStyle enumValue] {
			get { return this[(int)enumValue]; }
		}


		public bool IsStandardStyle(ColorStyle colorStyle) {
			return (IndexOf(colorStyle) < Enum.GetValues(typeof(StandardColorStyle)).Length);
		}


		public IColorStyle GetPreviewStyle(IColorStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (baseStyle is ColorStyle)
				return base.GetPreviewStyle((ColorStyle)baseStyle);
			else
				return base.GetPreviewStyle(baseStyle.Name);
		}


		#region IEnumerable<IColorStyle> Members

		public IEnumerator<IColorStyle> GetEnumerator() {
			return new IColorStyleEnumerator(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new IColorStyleEnumerator(this);
		}

		#endregion


		internal void SetDefaultColorStyle(StandardColorStyle enumValue, ColorStyle baseStyle, ColorStyle previewStyle) {
			SetStandardStyle((int)enumValue, baseStyle, previewStyle);
		}
	}


	public class CapStyleCollection : StyleCollection<CapStyle>, IEnumerable<ICapStyle> {

		public CapStyleCollection()
			: base() {
		}


		public CapStyle this[StandardCapStyle enumValue] {
			get { return this[(int)enumValue]; }
		}

		
		public bool IsStandardStyle(CapStyle capStyle) {
			return (IndexOf((CapStyle)capStyle) < Enum.GetValues(typeof(StandardCapStyle)).Length);
		}


		public ICapStyle GetPreviewStyle(ICapStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (baseStyle is CapStyle)
				return base.GetPreviewStyle((CapStyle)baseStyle);
			else
				return base.GetPreviewStyle(baseStyle.Name);
		}


		#region IEnumerable<ICapStyle> Members

		IEnumerator<ICapStyle> IEnumerable<ICapStyle>.GetEnumerator() {
			return new ICapStyleEnumerator(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new ICapStyleEnumerator(this);
		}

		#endregion


		internal void SetDefaultCapStyle(StandardCapStyle enumValue, CapStyle baseStyle, CapStyle previewStyle) {
			SetStandardStyle((int)enumValue, baseStyle, previewStyle);
		}
	}


	public class CharacterStyleCollection : StyleCollection<CharacterStyle>, IEnumerable<ICharacterStyle> {

		public CharacterStyleCollection()
			: base() {
		}

		public CharacterStyle this[StandardCharacterStyle enumValue] {
			get { return this[(int)enumValue]; }
		}

		
		public bool IsStandardStyle(CharacterStyle CharacterStyle) {
			return (IndexOf(CharacterStyle) < Enum.GetValues(typeof(StandardCharacterStyle)).Length);
		}


		public ICharacterStyle GetPreviewStyle(ICharacterStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (baseStyle is CharacterStyle)
				return base.GetPreviewStyle((CharacterStyle)baseStyle);
			else
				return base.GetPreviewStyle(baseStyle.Name);
		}


		#region IEnumerable<ICharacterStyle> Members

		IEnumerator<ICharacterStyle> IEnumerable<ICharacterStyle>.GetEnumerator() {
			return new ICharacterStyleEnumerator(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new ICharacterStyleEnumerator(this);
		}

		#endregion


		internal void SetDefaultCharacterStyle(StandardCharacterStyle enumValue, CharacterStyle baseStyle, CharacterStyle previewStyle) {
			SetStandardStyle((int)enumValue, baseStyle, previewStyle);
		}
	}


	public class FillStyleCollection : StyleCollection<FillStyle>, IEnumerable<IFillStyle> {

		public FillStyleCollection()
			: base() {
		}


		public FillStyle this[StandardFillStyle enumValue] {
			get { return this[(int)enumValue]; }
		}


		public bool IsStandardStyle(FillStyle FillStyle) {
			return (IndexOf(FillStyle) < Enum.GetValues(typeof(StandardFillStyle)).Length);
		}


		public IFillStyle GetPreviewStyle(IFillStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (baseStyle is FillStyle)
				return base.GetPreviewStyle((FillStyle)baseStyle);
			else
				return base.GetPreviewStyle(baseStyle.Name);
		}


		#region IEnumerable<IFillStyle> Members

		IEnumerator<IFillStyle> IEnumerable<IFillStyle>.GetEnumerator() {
			return new IFillStyleEnumerator(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new IFillStyleEnumerator(this);
		}

		#endregion


		internal void SetDefaultFillStyle(StandardFillStyle enumValue, FillStyle baseStyle, FillStyle previewStyle) {
			SetStandardStyle((int)enumValue, baseStyle, previewStyle);
		}
	}


	public class LineStyleCollection : StyleCollection<LineStyle>, IEnumerable<ILineStyle> {

		public LineStyleCollection()
			: base() {
		}


		public LineStyle this[StandardLineStyle enumValue] {
			get { return this[(int)enumValue]; }
		}


		public bool IsStandardStyle(LineStyle LineStyle) {
			return (IndexOf(LineStyle) < Enum.GetValues(typeof(StandardLineStyle)).Length);
		}
		

		public ILineStyle GetPreviewStyle(ILineStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (baseStyle is LineStyle)
				return base.GetPreviewStyle((LineStyle)baseStyle);
			else
				return base.GetPreviewStyle(baseStyle.Name);
		}


		#region IEnumerable<ILineStyle> Members

		IEnumerator<ILineStyle> IEnumerable<ILineStyle>.GetEnumerator() {
			return new ILineStyleEnumerator(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new ILineStyleEnumerator(this);
		}

		#endregion


		internal void SetDefaultLineStyle(StandardLineStyle enumValue, LineStyle baseStyle, LineStyle previewStyle) {
			SetStandardStyle((int)enumValue, baseStyle, previewStyle);
		}
	}


	public class ParagraphStyleCollection : StyleCollection<ParagraphStyle>, IEnumerable<IParagraphStyle> {

		public ParagraphStyleCollection()
			: base() {
		}


		public ParagraphStyle this[StandardParagraphStyle enumValue] {
			get { return this[(int)enumValue]; }
		}


		public bool IsStandardStyle(ParagraphStyle ParagraphStyle) {
			return (IndexOf(ParagraphStyle) < Enum.GetValues(typeof(StandardParagraphStyle)).Length);
		}


		public IParagraphStyle GetPreviewStyle(IParagraphStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (baseStyle is ParagraphStyle)
				return base.GetPreviewStyle((ParagraphStyle)baseStyle);
			else
				return base.GetPreviewStyle(baseStyle.Name);
		}


		#region IEnumerable<IParagraphStyle> Members

		IEnumerator<IParagraphStyle> IEnumerable<IParagraphStyle>.GetEnumerator() {
			return new IParagraphStyleEnumerator(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new IParagraphStyleEnumerator(this);
		}

		#endregion


		internal void SetDefaultParagraphStyle(StandardParagraphStyle enumValue, ParagraphStyle baseStyle, ParagraphStyle previewStyle) {
			SetStandardStyle((int)enumValue, baseStyle, previewStyle);
		}
	}


	/// <summary>
	/// Defines a set of styles for shapes.
	/// </summary>
	public class Design : IStyleSet, IEntity {

		/// <summary>
		/// Creates an empty design for subsequent loading from the cache.
		/// </summary>
		public Design() {
			// Nothing to do
		}

		
		/// <summary>
		/// Creates a value design ready for use. It already includes the standard styles.
		/// </summary>
		/// <param projectName="projectName"></param>
		public Design(string name) {
			this.name = name;
			CreateStandardStyles();
		}


		#region IPersistable Members

		public object Id { get { return id; } }


		public void AssignId(object id) {
			if (id == null)
				throw new DiagrammingException("NotSupported argument 'Id'.");
			else if (this.id != null)
				throw new DiagrammingException("Id is already assigned.");
			else
				this.id = id;
		}


		public void LoadFields(IRepositoryReader reader, int version) {
			name = reader.ReadString();
			description = reader.ReadString();
		}


		public void SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteString(name);
			writer.WriteString(description);
		}


		public void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		public void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		public void Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyInfos()) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}


		public static string EntityTypeName { get { return "Core.Design"; } }


		public static IEnumerable<EntityPropertyDefinition> GetPropertyInfos() {
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("Description", typeof(string));
		}

		#endregion


		#region IStyleSet Members

		public ICapStyle GetCapStyle(StandardCapStyle standardCapStyle) {
			return capStyles[standardCapStyle];
		}


		public ICharacterStyle GetCharacterStyle(StandardCharacterStyle standardCharacterStyle) {
			return characterStyles[standardCharacterStyle];
		}


		public IColorStyle GetColorStyle(StandardColorStyle standardColorStyle) {
			return colorStyles[standardColorStyle];
		}


		public IFillStyle GetFillStyle(StandardFillStyle standardFillStyle) {
			return fillStyles[standardFillStyle];
		}


		public ILineStyle GetLineStyle(StandardLineStyle standardLineStyle) {
			return lineStyles[standardLineStyle];
		}


		public IParagraphStyle GetParagraphStyle(StandardParagraphStyle standardParagraphStyle) {
			return paragraphStyles[standardParagraphStyle];
		}


		public ICapStyle GetCapStyle(string name) {
			return capStyles[name];
		}


		public ICharacterStyle GetCharacterStyle(string name) {
			return characterStyles[name];
		}


		public IColorStyle GetColorStyle(string name) {
			return colorStyles[name];
		}


		public IFillStyle GetFillStyle(string name) {
			return fillStyles[name];
		}


		public ILineStyle GetLineStyle(string name) {
			return lineStyles[name];
		}


		public IParagraphStyle GetParagraphStyle(string name) {
			return paragraphStyles[name];
		}


		public ICapStyle GetPreviewStyle(ICapStyle capStyle) {
			return capStyles.GetPreviewStyle(capStyle);
		}


		public ICharacterStyle GetPreviewStyle(ICharacterStyle characterStyle) {
			return characterStyles.GetPreviewStyle(characterStyle);
		}


		public IColorStyle GetPreviewStyle(IColorStyle colorStyle) {
			return colorStyles.GetPreviewStyle(colorStyle);
		}


		public IFillStyle GetPreviewStyle(IFillStyle fillStyle) {
			return fillStyles.GetPreviewStyle(fillStyle);
		}


		public ILineStyle GetPreviewStyle(ILineStyle lineStyle) {
			return lineStyles.GetPreviewStyle(lineStyle);
		}


		public IParagraphStyle GetPreviewStyle(IParagraphStyle paragraphStyle) {
			return paragraphStyles.GetPreviewStyle(paragraphStyle);
		}


		IEnumerable<ICapStyle> IStyleSet.CapStyles {
			get { return capStyles; }
		}


		IEnumerable<ICharacterStyle> IStyleSet.CharacterStyles {
			get { return characterStyles; }
		}


		IEnumerable<IColorStyle> IStyleSet.ColorStyles {
			get { return colorStyles; }
		}


		IEnumerable<IFillStyle> IStyleSet.FillStyles {
			get { return fillStyles; }
		}


		IEnumerable<ILineStyle> IStyleSet.LineStyles {
			get { return lineStyles; }
		}


		IEnumerable<IParagraphStyle> IStyleSet.ParagraphStyles {
			get { return paragraphStyles; }
		}

		#endregion

	
		public string Name {
			get { return name; }
			set { name = value; }
		}


		/// <summary>
		/// Returns all Styles of the design regardless of the style categoryTitle.
		/// </summary>
		public IEnumerable<IStyle> Styles {
			get {
				foreach (IStyle s in colorStyles) yield return s;
				foreach (IStyle s in capStyles) yield return s;
				foreach (IStyle s in lineStyles) yield return s;
				foreach (IStyle s in fillStyles) yield return s;
				foreach (IStyle s in characterStyles) yield return s;
				foreach (IStyle s in paragraphStyles) yield return s;
			}
		}


		/// <summary>
		/// Clears all style collections of the design
		/// </summary>
		public void Clear() {
			ParagraphStyles.Clear();
			ShapeStyles.Clear();
			LineStyles.Clear();
			CharacterStyles.Clear();
			FillStyles.Clear();
			CapStyles.Clear();
			ColorStyles.Clear();
		}


		public bool ContainsStyle(IStyle style) {
			if (style is CapStyle)
				return capStyles.Contains((CapStyle)style);
			else if (style is CharacterStyle)
				return characterStyles.Contains((CharacterStyle)style);
			else if (style is ColorStyle)
				return colorStyles.Contains((ColorStyle)style);
			else if (style is FillStyle)
				return fillStyles.Contains((FillStyle)style);
			else if (style is LineStyle)
				return lineStyles.Contains((LineStyle)style);
			else if (style is ParagraphStyle)
				return paragraphStyles.Contains((ParagraphStyle)style);
			else if (style is ShapeStyle)
				return shapeStyles.Contains((ShapeStyle)style);
			else
				throw new DiagrammingInternalException(string.Format("Unexpected style type '{0}'.", style.GetType().Name));
		}


		/// <summary>
		/// Returns the style of the same type with the same projectName if there is one in the design's style collection.
		/// </summary>
		public IStyle FindMatchingStyle(IStyle style) {
			if (style is ColorStyle) {
				if (ColorStyles.Contains(style.Name))
					return ColorStyles[style.Name];
				else return null;
			}
			else if (style is CapStyle) {
				if (CapStyles.Contains(style.Name))
					return CapStyles[style.Name];
				else return null;
			}
			else if (style is FillStyle) {
				if (FillStyles.Contains(style.Name))
					return FillStyles[style.Name];
				else return null;
			}
			else if (style is CharacterStyle) {
				if (CharacterStyles.Contains(style.Name))
					return CharacterStyles[style.Name];
				else return null;
			}
			else if (style is LineStyle) {
				if (LineStyles.Contains(style.Name))
					return LineStyles[style.Name];
				else return null;
			}
			else if (style is ShapeStyle) {
				if (ShapeStyles.Contains(style.Name))
					return ShapeStyles[style.Name];
				else return null;
			}
			else if (style is ParagraphStyle) {
				if (ParagraphStyles.Contains(style.Name))
					return ParagraphStyles[style.Name];
				else return null;
			}
			else throw new DiagrammingInternalException(string.Format("Unsupported style type {0}", style.GetType().Name));
		}


		public IStyle FindStyleByName(string name, Type styleType) {
			return DoFindStyleByName(name, styleType);
		}
		
		
		public CapStyleCollection CapStyles { get { return capStyles; } }


		public CharacterStyleCollection CharacterStyles { get { return characterStyles; } }


		public ColorStyleCollection ColorStyles { get { return colorStyles; } }


		public FillStyleCollection FillStyles { get { return fillStyles; } }


		public LineStyleCollection LineStyles { get { return lineStyles; } }


		public StyleCollection<ShapeStyle> ShapeStyles { get { return shapeStyles; } }
		
		
		public ParagraphStyleCollection ParagraphStyles { get { return paragraphStyles; } }


		public void AddStyle(IStyle style) {
			if (style is CapStyle)
				capStyles.Add((CapStyle)style, CreatePreviewStyle((ICapStyle)style));
			else if (style is CharacterStyle)
				characterStyles.Add((CharacterStyle)style, CreatePreviewStyle((ICharacterStyle)style));
			else if (style is ColorStyle)
				colorStyles.Add((ColorStyle)style, CreatePreviewStyle((IColorStyle)style));
			else if (style is FillStyle)
				fillStyles.Add((FillStyle)style, CreatePreviewStyle((IFillStyle)style));
			else if (style is LineStyle)
				lineStyles.Add((LineStyle)style, CreatePreviewStyle((ILineStyle)style));
			else if (style is ParagraphStyle)
				paragraphStyles.Add((ParagraphStyle)style, CreatePreviewStyle((IParagraphStyle)style));
			else if (style is ShapeStyle) {
				shapeStyles.Add((ShapeStyle)style, CreatePreviewStyle((IShapeStyle)style));
			}
			else
				throw new DiagrammingException("Unsupported Style type '{0}'.", style.GetType().Name);
		}


		public void RemoveStyle(IStyle style) {
			if (style is CapStyle)
				capStyles.Remove((CapStyle)style);
			else if (style is CharacterStyle)
				characterStyles.Remove((CharacterStyle)style);
			else if (style is ColorStyle)
				colorStyles.Remove((ColorStyle)style);
			else if (style is FillStyle)
				fillStyles.Remove((FillStyle)style);
			else if (style is LineStyle)
				lineStyles.Remove((LineStyle)style);
			else if (style is ParagraphStyle)
				paragraphStyles.Remove((ParagraphStyle)style);
			else if (style is ShapeStyle)
				shapeStyles.Remove((ShapeStyle)style);
			else
				throw new DiagrammingException("Unsupported Style type '{0}'.", style.GetType().Name);
		}


		/// <summary>
		/// Assigns the given style to the existing style with the same projectName. 
		/// If there is not style with such a projectName, a new style is created.
		/// This method also takes care about preview styles.
		/// </summary>
		/// <param projectName="style">The style that should be assigned to an existing style.</param>
		/// <returns>Returns true if an existring style was assigned and false if there was no matching style.</returns>
		public bool AssignStyle(IStyle style) {
			bool styleFound = ContainsStyle(style);
			if (styleFound) {
				Type styleType = style.GetType();
				Style existingStyle = DoFindStyleByName(name, styleType);
				existingStyle.Assign(style, this.FindMatchingStyle);
				CreateAndSetPreviewStyle(existingStyle);
			} else {
				Style newStyle = new CapStyle(style.Name);
				newStyle.Assign(style, this.FindMatchingStyle);
				AddStyle(newStyle);
			}
			return styleFound;
		}


		#region CreatePreviewStyle methods

		public CapStyle CreatePreviewStyle(ICapStyle baseStyle) {
			CapStyle result = new CapStyle(true, baseStyle.Name + previewNameSuffix);
			result.CapShape = baseStyle.CapShape;
			result.CapSize = baseStyle.CapSize;
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			return result;
		}


		public ColorStyle CreatePreviewStyle(IColorStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			ColorStyle result = new ColorStyle(true, baseStyle.Name + previewNameSuffix);
			result.Color = baseStyle.Color;
			result.Transparency = GetPreviewTransparency(baseStyle.Transparency);
			result.ConvertToGray = true;
			return result;
		}


		public FillStyle CreatePreviewStyle(IFillStyle baseStyle) {
			FillStyle result = new FillStyle(true, baseStyle.Name + previewNameSuffix);
			if (baseStyle.AdditionalColorStyle != null)
				result.AdditionalColorStyle = CreatePreviewStyle(baseStyle.AdditionalColorStyle);
			if (baseStyle.BaseColorStyle != null)
				result.BaseColorStyle = CreatePreviewStyle(baseStyle.BaseColorStyle);
			result.ConvertToGrayScale = true;
			result.FillMode = baseStyle.FillMode;
			result.FillPattern = baseStyle.FillPattern;
			
			if (baseStyle.Image != null && (baseStyle.Image.Width > 500 || baseStyle.Image.Height > 500)) {
				float scale = Geometry.CalcScaleFactor(
					baseStyle.Image.Width, 
					baseStyle.Image.Height, 
					baseStyle.Image.Width / Math.Max(1, (baseStyle.Image.Width / 500)), 
					baseStyle.Image.Height / Math.Max(1, (baseStyle.Image.Height / 500)));
				int width = (int)Math.Round(baseStyle.Image.Width * scale);
				int height = (int)Math.Round(baseStyle.Image.Height * scale);
				NamedImage namedImg = new NamedImage();
				namedImg.Image = baseStyle.Image.Image.GetThumbnailImage(width, height, null, IntPtr.Zero);
				namedImg.Name = baseStyle.Image.Name;
				result.Image = namedImg;
			}
			else result.Image = baseStyle.Image;

			result.ImageGammaCorrection = baseStyle.ImageGammaCorrection;
			result.ImageLayout = baseStyle.ImageLayout;
			result.ImageTransparency = GetPreviewTransparency(baseStyle.ImageTransparency);
			return result;
		}


		public CharacterStyle CreatePreviewStyle(ICharacterStyle baseStyle) {
			CharacterStyle result = new CharacterStyle(true, baseStyle.Name + previewNameSuffix);
			if (baseStyle.ColorStyle != null) 
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			result.FontName = baseStyle.FontName;
			result.Size = baseStyle.Size;
			result.Style = baseStyle.Style;
			return result;
		}


		public LineStyle CreatePreviewStyle(ILineStyle baseStyle) {
			LineStyle result = new LineStyle(true, baseStyle.Name + previewNameSuffix);
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			result.DashCap = baseStyle.DashCap;
			result.DashType = baseStyle.DashType;
			result.LineJoin = baseStyle.LineJoin;
			result.LineWidth = baseStyle.LineWidth;
			return result;
		}


		public ShapeStyle CreatePreviewStyle(IShapeStyle baseStyle) {
			ShapeStyle result = new ShapeStyle(baseStyle.Name + previewNameSuffix);
			result.RoundedCorners = baseStyle.RoundedCorners;
			if (baseStyle.ShadowColor != null)
				result.ShadowColor = CreatePreviewStyle(baseStyle.ShadowColor);
			result.ShowGradients = baseStyle.ShowGradients;
			result.ShowShadows = baseStyle.ShowShadows;
			return result;
		}


		public ParagraphStyle CreatePreviewStyle(IParagraphStyle baseStyle) {
			ParagraphStyle result = new ParagraphStyle(true, baseStyle.Name + previewNameSuffix);
			result.Alignment = baseStyle.Alignment;
			result.Padding = baseStyle.Padding;
			result.Trimming = baseStyle.Trimming;
			return result;
		}

		#endregion


		private Style DoFindStyleByName(string name, Type styleType) {
			if (styleType == typeof(CapStyle))
				return capStyles[name];
			else if (styleType == typeof(CharacterStyle))
				return CharacterStyles[name];
			else if (styleType == typeof(ColorStyle))
				return colorStyles[name];
			else if (styleType == typeof(FillStyle))
				return fillStyles[name];
			else if (styleType == typeof(LineStyle))
				return lineStyles[name];
			else if (styleType == typeof(ParagraphStyle))
				return paragraphStyles[name];
			else if (styleType == typeof(ShapeStyle))
				return shapeStyles[name];
			else throw new DiagrammingException("Unexpected style type '{0}'.", styleType.Name);
		}


		private void CreateAndSetPreviewStyle(Style baseStyle) {
			if (baseStyle is CapStyle) {
				CapStyle style = (CapStyle)baseStyle;
				CapStyle previewStyle = CreatePreviewStyle(style);
				capStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is CharacterStyle) {
				CharacterStyle style = (CharacterStyle)baseStyle;
				CharacterStyle previewStyle = CreatePreviewStyle(style);
				characterStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is ColorStyle) {
				ColorStyle style = (ColorStyle)baseStyle;
				ColorStyle previewStyle = CreatePreviewStyle(style);
				colorStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is FillStyle) {
				FillStyle style = (FillStyle)baseStyle;
				FillStyle previewStyle = CreatePreviewStyle(style);
				fillStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is LineStyle) {
				LineStyle style = (LineStyle)baseStyle;
				LineStyle previewStyle = CreatePreviewStyle(style);
				lineStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is ParagraphStyle) {
				ParagraphStyle style = (ParagraphStyle)baseStyle;
				ParagraphStyle previewStyle = CreatePreviewStyle(style);
				paragraphStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is CapStyle) {
				// ToDo: Implement ShapeStyles
				//ShapeStyle style = (ShapeStyle)baseStyle;
				//ShapeStyle previewStyle = CreatePreviewStyle(style);
				//shapeStyles.SetPreviewStyle(style, previewStyle);
			} else throw new DiagrammingException("Unexpected style type '{0}'.", baseStyle.GetType().Name);
		}


		private Byte GetPreviewTransparency(byte baseTransparency) {
			int result = baseTransparency + ((100 - baseTransparency) / 2);
			if (result > 100) result = 100;
			return Convert.ToByte(result);
		}


		#region Creating Standard Styles

		private void CreateStandardStyles() {
			CreateStandardColorStyles();
			CreateStandardCapStyles();
			CreateStandardCharacterStyles();
			CreateStandardFillStyles();
			CreateStandardLineStyles();
			CreateStandardParagraphStyles();
		}


		private void CreateStandardColorStyles() {
			ColorStyle colorStyle;

			colorStyle = new ColorStyle(StandardColorStyle.Background, Color.Silver);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Background, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Black, Color.Black);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Black, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Blue, Color.SteelBlue);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Blue, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Gray, Color.Gray);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Gray, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Green, Color.SeaGreen);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Green, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Highlight, Color.DarkOrange);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Highlight, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.HighlightText, Color.Navy);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.HighlightText, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.LightBlue, Color.LightSteelBlue);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.LightBlue, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.LightGray, Color.LightGray);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.LightGray, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.LightGreen, Color.DarkSeaGreen);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.LightGreen, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.LightRed, Color.LightSalmon);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.LightRed, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.LightYellow, Color.LightGoldenrodYellow);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.LightYellow, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Red, Color.Firebrick);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Red, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Text, Color.Black);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Text, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Transparent, Color.Transparent);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Transparent, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.White, Color.White);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.White, colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(StandardColorStyle.Yellow, Color.Gold);
			ColorStyles.SetDefaultColorStyle(StandardColorStyle.Yellow, colorStyle, CreatePreviewStyle(colorStyle));
		}


		private void CreateStandardCapStyles() {
			CapStyle capStyle;

			capStyle = new CapStyle(StandardCapStyle.Arrow);
			capStyle.CapShape = CapShape.ArrowClosed;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = ColorStyles[StandardColorStyle.White];
			capStyles.SetDefaultCapStyle(StandardCapStyle.Arrow, capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(StandardCapStyle.None);
			capStyle.CapShape = CapShape.None;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = ColorStyles[StandardColorStyle.White];
			capStyles.SetDefaultCapStyle(StandardCapStyle.None, capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(StandardCapStyle.Special1);
			capStyle.CapShape = CapShape.Circle;
			capStyle.CapSize = 6;
			capStyle.ColorStyle = ColorStyles[StandardColorStyle.White];
			capStyles.SetDefaultCapStyle(StandardCapStyle.Special1, capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(StandardCapStyle.Special2);
			capStyle.CapShape = CapShape.Diamond;
			capStyle.CapSize = 6;
			capStyle.ColorStyle = ColorStyles[StandardColorStyle.White];
			capStyles.SetDefaultCapStyle(StandardCapStyle.Special2, capStyle, CreatePreviewStyle(capStyle));
		}


		private void CreateStandardCharacterStyles() {
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				CharacterStyle charStyle;
				charStyle = new CharacterStyle(StandardCharacterStyle.Caption);
				charStyle.ColorStyle = ColorStyles[StandardColorStyle.Text];
				charStyle.FontName = "Tahoma";
				charStyle.Size = 10;
				//charStyle.Size = Geometry.PointToPixel(10, gfx.DpiY);
				charStyle.Style = FontStyle.Regular;
				characterStyles.SetDefaultCharacterStyle(StandardCharacterStyle.Caption, charStyle, CreatePreviewStyle(charStyle));

				charStyle = new CharacterStyle(StandardCharacterStyle.Heading1);
				charStyle.ColorStyle = ColorStyles[StandardColorStyle.Text];
				charStyle.FontName = "Tahoma";
				charStyle.Size = 36;
				//charStyle.Size = Geometry.PointToPixel(36, gfx.DpiY);
				charStyle.Style = FontStyle.Bold;
				characterStyles.SetDefaultCharacterStyle(StandardCharacterStyle.Heading1, charStyle, CreatePreviewStyle(charStyle));

				charStyle = new CharacterStyle(StandardCharacterStyle.Heading2);
				charStyle.ColorStyle = ColorStyles[StandardColorStyle.Text];
				charStyle.FontName = "Tahoma";
				charStyle.Size = 24;
				//charStyle.Size = Geometry.PointToPixel(24, gfx.DpiY);
				charStyle.Style = FontStyle.Bold;
				characterStyles.SetDefaultCharacterStyle(StandardCharacterStyle.Heading2, charStyle, CreatePreviewStyle(charStyle));

				charStyle = new CharacterStyle(StandardCharacterStyle.Heading3);
				charStyle.ColorStyle = ColorStyles[StandardColorStyle.Text];
				charStyle.FontName = "Tahoma";
				charStyle.Size = 16;
				//charStyle.Size = Geometry.PointToPixel(16, gfx.DpiY);
				charStyle.Style = FontStyle.Bold;
				characterStyles.SetDefaultCharacterStyle(StandardCharacterStyle.Heading3, charStyle, CreatePreviewStyle(charStyle));

				charStyle = new CharacterStyle(StandardCharacterStyle.Normal);
				charStyle.ColorStyle = ColorStyles[StandardColorStyle.Text];
				charStyle.FontName = "Tahoma";
				charStyle.Size = 12;
				//charStyle.Size = Geometry.PointToPixel(12, gfx.DpiY);
				charStyle.Style = FontStyle.Regular;
				characterStyles.SetDefaultCharacterStyle(StandardCharacterStyle.Normal, charStyle, CreatePreviewStyle(charStyle));

				charStyle = new CharacterStyle(StandardCharacterStyle.Subtitle);
				charStyle.ColorStyle = ColorStyles[StandardColorStyle.Text];
				charStyle.FontName = "Tahoma";
				charStyle.Size = 12;
				//charStyle.Size = Geometry.PointToPixel(12, gfx.DpiY);
				charStyle.Style = FontStyle.Bold;
				characterStyles.SetDefaultCharacterStyle(StandardCharacterStyle.Subtitle, charStyle, CreatePreviewStyle(charStyle));
			}
		}


		private void CreateStandardFillStyles() {
			FillStyle fillStyle;

			fillStyle = new FillStyle(StandardFillStyle.Black);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.Black];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.Black, fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(StandardFillStyle.Blue);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.Blue];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.Blue, fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(StandardFillStyle.Green);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.Green];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.Green, fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(StandardFillStyle.Red);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.Red];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.Red, fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(StandardFillStyle.Transparent);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.Transparent];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.Transparent];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.Transparent, fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(StandardFillStyle.White);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.White, fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(StandardFillStyle.Yellow);
			fillStyle.AdditionalColorStyle = colorStyles[StandardColorStyle.White];
			fillStyle.BaseColorStyle = colorStyles[StandardColorStyle.Yellow];
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.SetDefaultFillStyle(StandardFillStyle.Yellow, fillStyle, CreatePreviewStyle(fillStyle));
		}


		private void CreateStandardLineStyles() {
			LineStyle lineStyle;

			lineStyle = new LineStyle(StandardLineStyle.Blue);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Blue];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Blue, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Dashed);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Black];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dash;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Dashed, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Dotted);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Black];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Dotted, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Green);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Green];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Green, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Highlight);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Highlight];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Highlight, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.HighlightDashed);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Highlight];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dash;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.HighlightDashed, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.HighlightDotted);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Highlight];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.HighlightDotted, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.HighlightThick);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Highlight];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 3;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.HighlightThick, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.None);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Transparent];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.None, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Normal);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Black];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Normal, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Red);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Red];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Red, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Special1);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Black];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.DashDot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Special1, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Special2);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Black];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.DashDotDot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Special2, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Thick);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Black];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 3;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Thick, lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(StandardLineStyle.Yellow);
			lineStyle.ColorStyle = colorStyles[StandardColorStyle.Yellow];
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.SetDefaultLineStyle(StandardLineStyle.Yellow, lineStyle, CreatePreviewStyle(lineStyle));
		}
		

		private void CreateStandardParagraphStyles() {
			ParagraphStyle paragraphStyle;

			paragraphStyle = new ParagraphStyle(StandardParagraphStyle.Label);
			paragraphStyle.Alignment = ContentAlignment.MiddleLeft;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = false;
			paragraphStyles.SetDefaultParagraphStyle(StandardParagraphStyle.Label, paragraphStyle, CreatePreviewStyle(paragraphStyle));

			paragraphStyle = new ParagraphStyle(StandardParagraphStyle.Text);
			paragraphStyle.Alignment = ContentAlignment.TopLeft;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = true;
			paragraphStyles.SetDefaultParagraphStyle(StandardParagraphStyle.Text, paragraphStyle, CreatePreviewStyle(paragraphStyle));

			paragraphStyle = new ParagraphStyle(StandardParagraphStyle.Title);
			paragraphStyle.Alignment = ContentAlignment.MiddleCenter;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = true;
			paragraphStyles.SetDefaultParagraphStyle(StandardParagraphStyle.Title, paragraphStyle, CreatePreviewStyle(paragraphStyle));

		}

		#endregion


		#region Fields

		private object id = null;
		private string name = "";
		private string description = "";
		
		// Style Collections
		private CapStyleCollection capStyles = new CapStyleCollection();
		private CharacterStyleCollection characterStyles = new CharacterStyleCollection();
		private ColorStyleCollection colorStyles = new ColorStyleCollection();
		private FillStyleCollection fillStyles = new FillStyleCollection();
		private LineStyleCollection lineStyles = new LineStyleCollection();
		private ParagraphStyleCollection paragraphStyles = new ParagraphStyleCollection();
		private StyleCollection<ShapeStyle> shapeStyles = new StyleCollection<ShapeStyle>();
		private const string previewNameSuffix = " Preview Style";

		#endregion
	}
}
