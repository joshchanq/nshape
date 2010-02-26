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


namespace Dataweb.NShape {

	public interface ICapStyles : IEnumerable<ICapStyle> {

		ICapStyle this[string name] { get; }

		ICapStyle None { get; }

		ICapStyle Arrow { get; }

		ICapStyle Special1 { get; }

		ICapStyle Special2 { get; }

	}


	public interface ICharacterStyles : IEnumerable<ICharacterStyle> {

		ICharacterStyle this[string name] { get; }

		ICharacterStyle Caption { get; }

		ICharacterStyle Heading1 { get; }

		ICharacterStyle Heading2 { get; }

		ICharacterStyle Heading3 { get; }

		ICharacterStyle Normal { get; }

		ICharacterStyle Subtitle { get; }

	}


	public interface IColorStyles : IEnumerable<IColorStyle> {

		IColorStyle this[string name] { get; }

		IColorStyle Background { get; }

		IColorStyle Black { get; }

		IColorStyle Blue { get; }

		IColorStyle Gray { get; }

		IColorStyle Green { get; }

		IColorStyle Highlight { get; }

		IColorStyle HighlightText { get; }

		IColorStyle LightBlue { get; }

		IColorStyle LightGray { get; }

		IColorStyle LightGreen { get; }

		IColorStyle LightRed { get; }

		IColorStyle LightYellow { get; }

		IColorStyle Red { get; }

		IColorStyle Text { get; }

		IColorStyle Transparent { get; }

		IColorStyle White { get; }

		IColorStyle Yellow { get; }

	}


	public interface IFillStyles : IEnumerable<IFillStyle> {

		IFillStyle this[string name] { get; }

		IFillStyle Black { get; }

		IFillStyle Blue { get; }

		IFillStyle Green { get; }

		IFillStyle Red { get; }

		IFillStyle Transparent { get; }

		IFillStyle White { get; }

		IFillStyle Yellow { get; }

	}


	public interface ILineStyles : IEnumerable<ILineStyle> {

		ILineStyle this[string name] { get; }

		ILineStyle Blue { get; }

		ILineStyle Dashed { get; }

		ILineStyle Dotted { get; }

		ILineStyle Green { get; }

		ILineStyle Highlight { get; }

		ILineStyle HighlightDashed { get; }

		ILineStyle HighlightDotted { get; }

		ILineStyle HighlightThick { get; }

		ILineStyle None { get; }

		ILineStyle Normal { get; }

		ILineStyle Red { get; }

		ILineStyle Special1 { get; }

		ILineStyle Special2 { get; }

		ILineStyle Thick { get; }

		ILineStyle Yellow { get; }

	}


	public interface IParagraphStyles : IEnumerable<IParagraphStyle> {

		IParagraphStyle this[string name] { get; }

		// Standard Style Properties
		IParagraphStyle Label { get; }

		IParagraphStyle Text { get; }

		IParagraphStyle Title { get; }

	}


	/// <summary>
	/// Defines a set of styles.
	/// </summary>
	public interface IStyleSet {

		ICapStyles CapStyles { get; }

		ICharacterStyles CharacterStyles { get; }

		IColorStyles ColorStyles { get; }

		IFillStyles FillStyles { get; }

		ILineStyles LineStyles { get; }

		IParagraphStyles ParagraphStyles { get; }

		ICapStyle GetPreviewStyle(ICapStyle colorStyle);

		ICharacterStyle GetPreviewStyle(ICharacterStyle colorStyle);

		IColorStyle GetPreviewStyle(IColorStyle colorStyle);

		IFillStyle GetPreviewStyle(IFillStyle fillStyle);

		ILineStyle GetPreviewStyle(ILineStyle lineStyle);

		IParagraphStyle GetPreviewStyle(IParagraphStyle colorStyle);

	}


	/// <summary>
	/// Defines a set of styles for shapes.
	/// </summary>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public class Design : IStyleSet, IEntity {

		/// <summary>
		/// Creates an empty design for subsequent loading from the cache.
		/// </summary>
		internal Design() {
		}


		/// <summary>
		/// Creates a value design ready for use. It already includes the standard styles.
		/// </summary>
		/// <param name="projectName"></param>
		public Design(string name)
			: this() {
			if (name == null) throw new ArgumentNullException("name");
			this.name = name;
			// Create standard styles
			CreateStandardColorStyles();
			CreateStandardCapStyles();
			CreateStandardCharacterStyles();
			CreateStandardFillStyles();
			CreateStandardLineStyles();
			CreateStandardParagraphStyles();
		}


		#region IEntity Members

		public static string EntityTypeName {
			get { return "Core.Design"; }
		}


		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("Description", typeof(string));
		}


		object IEntity.Id {
			get { return id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null)
				throw new ArgumentNullException("id");
			if (this.id != null)
				throw new InvalidOperationException("Design has already an id.");
			this.id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			name = reader.ReadString();
			description = reader.ReadString();
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteString(name);
			writer.WriteString(description);
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		#region IStyleSet Members

		public ICapStyle GetPreviewStyle(ICapStyle capStyle) {
			if (capStyle == null) throw new ArgumentNullException("capStyle");
			return capStyles.GetPreviewStyle(capStyle.Name);
		}


		public ICharacterStyle GetPreviewStyle(ICharacterStyle characterStyle) {
			if (characterStyle == null) throw new ArgumentNullException("characterStyle");
			return characterStyles.GetPreviewStyle(characterStyle.Name);
		}


		public IColorStyle GetPreviewStyle(IColorStyle colorStyle) {
			if (colorStyle == null) throw new ArgumentNullException("colorStyle");
			return colorStyles.GetPreviewStyle(colorStyle.Name);
		}


		public IFillStyle GetPreviewStyle(IFillStyle fillStyle) {
			if (fillStyle == null) throw new ArgumentNullException("fillStyle");
			return fillStyles.GetPreviewStyle(fillStyle.Name);
		}


		public ILineStyle GetPreviewStyle(ILineStyle lineStyle) {
			if (lineStyle == null) throw new ArgumentNullException("lineStyle");
			return lineStyles.GetPreviewStyle(lineStyle.Name);
		}


		public IParagraphStyle GetPreviewStyle(IParagraphStyle paragraphStyle) {
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			return paragraphStyles.GetPreviewStyle(paragraphStyle.Name);
		}


		ICapStyles IStyleSet.CapStyles {
			get { return capStyles; }
		}


		ICharacterStyles IStyleSet.CharacterStyles {
			get { return characterStyles; }
		}


		IColorStyles IStyleSet.ColorStyles {
			get { return colorStyles; }
		}


		IFillStyles IStyleSet.FillStyles {
			get { return fillStyles; }
		}


		ILineStyles IStyleSet.LineStyles {
			get { return lineStyles; }
		}


		IParagraphStyles IStyleSet.ParagraphStyles {
			get { return paragraphStyles; }
		}

		#endregion


		#region [Public] Properties

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


		public CapStyleCollection CapStyles {
			get { return capStyles; }
		}


		public CharacterStyleCollection CharacterStyles {
			get { return characterStyles; }
		}


		public ColorStyleCollection ColorStyles {
			get { return colorStyles; }
		}


		public FillStyleCollection FillStyles {
			get { return fillStyles; }
		}


		public LineStyleCollection LineStyles {
			get { return lineStyles; }
		}


		public ParagraphStyleCollection ParagraphStyles {
			get { return paragraphStyles; }
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Clears all style collections of the design
		/// </summary>
		public void Clear() {
			// Clear user defined styles
			//shapeStyles.Clear();
			paragraphStyles.Clear();
			lineStyles.Clear();
			characterStyles.Clear();
			fillStyles.Clear();
			capStyles.Clear();
			colorStyles.Clear();
		}


		public bool ContainsStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
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
				throw new NShapeInternalException(string.Format("Unexpected style type '{0}'.", style.GetType().Name));
		}


		public bool IsStandardStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is CapStyle)
				return capStyles.IsStandardStyle((CapStyle)style);
			else if (style is CharacterStyle)
				return characterStyles.IsStandardStyle((CharacterStyle)style);
			else if (style is ColorStyle)
				return colorStyles.IsStandardStyle((ColorStyle)style);
			else if (style is FillStyle)
				return fillStyles.IsStandardStyle((FillStyle)style);
			else if (style is LineStyle)
				return lineStyles.IsStandardStyle((LineStyle)style);
			else if (style is ShapeStyle)
				return shapeStyles.IsStandardStyle((ShapeStyle)style);
			else if (style is ParagraphStyle)
				return paragraphStyles.IsStandardStyle((ParagraphStyle)style);
			else throw new NShapeUnsupportedValueException(style);
		}


		/// <summary>
		/// Returns the style of the same type with the same projectName if there is one in the design's style collection.
		/// </summary>
		public IStyle FindMatchingStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is ColorStyle) {
				if (colorStyles.Contains(style.Name))
					return colorStyles[style.Name];
				else return null;
			} else if (style is CapStyle) {
				if (capStyles.Contains(style.Name))
					return capStyles[style.Name];
				else return null;
			} else if (style is FillStyle) {
				if (fillStyles.Contains(style.Name))
					return fillStyles[style.Name];
				else return null;
			} else if (style is CharacterStyle) {
				if (characterStyles.Contains(style.Name))
					return characterStyles[style.Name];
				else return null;
			} else if (style is LineStyle) {
				if (lineStyles.Contains(style.Name))
					return lineStyles[style.Name];
				else return null;
			//} else if (style is ShapeStyle) {
			//   if (ShapeStyles.Contains(style.Name))
			//      return ShapeStyles[style.Name];
			//   else return null;
			} else if (style is ParagraphStyle) {
				if (paragraphStyles.Contains(style.Name))
					return paragraphStyles[style.Name];
				else return null;
			} else throw new NShapeUnsupportedValueException(style);
		}


		public IStyle FindStyleByName(string name, Type styleType) {
			if (name == null) throw new ArgumentNullException("name");
			if (styleType == null) throw new ArgumentNullException("styleType");
			return DoFindStyleByName(name, styleType);
		}


		public void AddStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is CapStyle) {
				capStyles.Add((CapStyle)style, CreatePreviewStyle((ICapStyle)style));
			} else if (style is CharacterStyle) {
				characterStyles.Add((CharacterStyle)style, CreatePreviewStyle((ICharacterStyle)style));
			} else if (style is ColorStyle) {
				colorStyles.Add((ColorStyle)style, CreatePreviewStyle((IColorStyle)style));
			} else if (style is FillStyle) {
				fillStyles.Add((FillStyle)style, CreatePreviewStyle((IFillStyle)style));
			} else if (style is LineStyle) {
				lineStyles.Add((LineStyle)style, CreatePreviewStyle((ILineStyle)style));
			} else if (style is ParagraphStyle) {
				paragraphStyles.Add((ParagraphStyle)style, CreatePreviewStyle((IParagraphStyle)style));
			} else if (style is ShapeStyle) {
				shapeStyles.Add((ShapeStyle)style, CreatePreviewStyle((IShapeStyle)style));
			} else throw new NShapeUnsupportedValueException(style);
		}


		public void RemoveStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
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
			else throw new NShapeUnsupportedValueException(style);
		}


		public void RemoveStyle(string name, Type styleType) {
			if (name == null) throw new ArgumentNullException("name");
			if (styleType == null) throw new ArgumentNullException("styleType");
			if (styleType == typeof(CapStyle))
				capStyles.Remove(name);
			else if (styleType == typeof(CharacterStyle))
				characterStyles.Remove(name);
			else if (styleType == typeof(ColorStyle))
				colorStyles.Remove(name);
			else if (styleType == typeof(FillStyle))
				fillStyles.Remove(name);
			else if (styleType == typeof(LineStyle))
				lineStyles.Remove(name);
			else if (styleType == typeof(ParagraphStyle))
				paragraphStyles.Remove(name);
			else if (styleType == typeof(ShapeStyle))
				shapeStyles.Remove(name);
			else throw new NShapeUnsupportedValueException(styleType);
		}


		/// <summary>
		/// Assigns the given style to the existing style with the same projectName. 
		/// If there is not style with such a projectName, a new style is created.
		/// This method also takes care about preview styles.
		/// </summary>
		/// <param name="style">The style that should be assigned to an existing style.</param>
		/// <returns>Returns true if an existring style was assigned and false if there was no matching style.</returns>
		public bool AssignStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
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


		public CapStyle CreatePreviewStyle(ICapStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			CapStyle result = new CapStyle(baseStyle.Name + previewNameSuffix);
			result.CapShape = baseStyle.CapShape;
			result.CapSize = baseStyle.CapSize;
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			return result;
		}


		public ColorStyle CreatePreviewStyle(IColorStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			ColorStyle result = new ColorStyle(baseStyle.Name + previewNameSuffix);
			result.Color = baseStyle.Color;
			result.Transparency = GetPreviewTransparency(baseStyle.Transparency);
			result.ConvertToGray = previewAsGrayScale;
			return result;
		}


		public FillStyle CreatePreviewStyle(IFillStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			FillStyle result = new FillStyle(baseStyle.Name + previewNameSuffix, ColorStyle.Empty, ColorStyle.Empty);
			if (baseStyle.AdditionalColorStyle != null)
				result.AdditionalColorStyle = CreatePreviewStyle(baseStyle.AdditionalColorStyle);
			if (baseStyle.BaseColorStyle != null)
				result.BaseColorStyle = CreatePreviewStyle(baseStyle.BaseColorStyle);
			result.ConvertToGrayScale = previewAsGrayScale;
			result.FillMode = baseStyle.FillMode;
			result.FillPattern = baseStyle.FillPattern;

			int newSize = 512;
			if (baseStyle.Image != null && (baseStyle.Image.Width > 2 * newSize || baseStyle.Image.Height > newSize)) {
				float scale = Geometry.CalcScaleFactor(
					baseStyle.Image.Width,
					baseStyle.Image.Height,
					baseStyle.Image.Width / Math.Max(1, (baseStyle.Image.Width / newSize)),
					baseStyle.Image.Height / Math.Max(1, (baseStyle.Image.Height / newSize)));
				int width = (int)Math.Round(baseStyle.Image.Width * scale);
				int height = (int)Math.Round(baseStyle.Image.Height * scale);
				NamedImage namedImg = new NamedImage();
				namedImg.Image = baseStyle.Image.Image.GetThumbnailImage(width, height, null, IntPtr.Zero);
				namedImg.Name = baseStyle.Image.Name;
				result.Image = namedImg;
			} else result.Image = baseStyle.Image;

			result.ImageGammaCorrection = baseStyle.ImageGammaCorrection;
			result.ImageLayout = baseStyle.ImageLayout;
			result.ImageTransparency = GetPreviewTransparency(baseStyle.ImageTransparency);
			return result;
		}


		public CharacterStyle CreatePreviewStyle(ICharacterStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			CharacterStyle result = new CharacterStyle(baseStyle.Name + previewNameSuffix);
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			result.FontName = baseStyle.FontName;
			result.SizeInPoints = baseStyle.SizeInPoints;
			result.Style = baseStyle.Style;
			return result;
		}


		public LineStyle CreatePreviewStyle(ILineStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			LineStyle result = new LineStyle(baseStyle.Name + previewNameSuffix);
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			result.DashCap = baseStyle.DashCap;
			result.DashType = baseStyle.DashType;
			result.LineJoin = baseStyle.LineJoin;
			result.LineWidth = baseStyle.LineWidth;
			return result;
		}


		public ShapeStyle CreatePreviewStyle(IShapeStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			ShapeStyle result = new ShapeStyle(baseStyle.Name + previewNameSuffix);
			result.RoundedCorners = baseStyle.RoundedCorners;
			if (baseStyle.ShadowColor != null)
				result.ShadowColor = CreatePreviewStyle(baseStyle.ShadowColor);
			result.ShowGradients = baseStyle.ShowGradients;
			result.ShowShadows = baseStyle.ShowShadows;
			return result;
		}


		public ParagraphStyle CreatePreviewStyle(IParagraphStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			ParagraphStyle result = new ParagraphStyle(baseStyle.Name + previewNameSuffix);
			result.Alignment = baseStyle.Alignment;
			result.Padding = baseStyle.Padding;
			result.Trimming = baseStyle.Trimming;
			return result;
		}

		#endregion


		internal static bool PreviewsAsGrayScale {
			get { return previewAsGrayScale; }
		}


		internal static Byte GetPreviewTransparency(byte baseTransparency) {
			int result = baseTransparency + (int)Math.Round((100 - baseTransparency) * previewTransparencyFactor);
			if (result < 0) result = 0;
			else if (result > 100) result = 100;
			return Convert.ToByte(result);
		}


		private Style DoFindStyleByName(string name, Type styleType) {
			if (IsOfType(styleType, typeof(ICapStyle)))
				return capStyles.Contains(name) ? capStyles[name] : null;
			else if (IsOfType(styleType, typeof(ICharacterStyle)))
				return characterStyles.Contains(name) ? characterStyles[name] : null;
			else if (IsOfType(styleType, typeof(IColorStyle)))
				return colorStyles.Contains(name) ? colorStyles[name] : null;
			else if (IsOfType(styleType, typeof(IFillStyle)))
				return fillStyles.Contains(name) ? fillStyles[name] : null;
			else if (IsOfType(styleType, typeof(ILineStyle)))
				return lineStyles.Contains(name) ? lineStyles[name] : null;
			else if (styleType == typeof(ParagraphStyle))
				return paragraphStyles.Contains(name) ? paragraphStyles[name] : null;
			else if (styleType == typeof(ShapeStyle))
				return shapeStyles.Contains(name) ? shapeStyles[name] : null;
			else throw new NShapeException("Unexpected style type '{0}'.", styleType.Name);
		}


		private bool IsOfType(Type objectType, Type targetType) {
			return objectType == targetType
				|| objectType.IsSubclassOf(targetType)
				|| objectType.GetInterface(targetType.Name, true) != null;
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
			} else if (baseStyle is ShapeStyle) {
				// ToDo: Implement ShapeStyles
				//ShapeStyle style = (ShapeStyle)baseStyle;
				//ShapeStyle previewStyle = CreatePreviewStyle(style);
				//shapeStyles.SetPreviewStyle(style, previewStyle);
			} else throw new NShapeException("Unexpected style type '{0}'.", baseStyle.GetType().Name);
		}


		#region Creating Standard Styles

		private void CreateStandardColorStyles() {
			ColorStyle colorStyle;

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Background, Color.Silver);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Black, Color.Black);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Blue, Color.SteelBlue);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Gray, Color.Gray);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Green, Color.SeaGreen);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Highlight, Color.DarkOrange);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.HighlightText, Color.Navy);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightBlue, Color.LightSteelBlue);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightGray, Color.LightGray);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightGreen, Color.DarkSeaGreen);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightRed, Color.LightSalmon);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightYellow, Color.LightGoldenrodYellow);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Red, Color.Firebrick);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Text, Color.Black);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Transparent, Color.Transparent);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.White, Color.White);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Yellow, Color.Gold);
			colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));
		}


		private void CreateStandardCapStyles() {
			CapStyle capStyle;

			capStyle = new CapStyle(CapStyle.StandardNames.Arrow);
			capStyle.CapShape = CapShape.ArrowClosed;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = colorStyles.White;
			capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.None);
			capStyle.CapShape = CapShape.None;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = colorStyles.White;
			capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.Special1);
			capStyle.CapShape = CapShape.Circle;
			capStyle.CapSize = 6;
			capStyle.ColorStyle = colorStyles.White;
			capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.Special2);
			capStyle.CapShape = CapShape.Diamond;
			capStyle.CapSize = 6;
			capStyle.ColorStyle = colorStyles.White;
			capStyles.Add(capStyle, CreatePreviewStyle(capStyle));
		}


		private void CreateStandardCharacterStyles() {
			CharacterStyle charStyle;

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Caption);
			charStyle.ColorStyle = colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 10;
			charStyle.Style = FontStyle.Regular;
			characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Heading1);
			charStyle.ColorStyle = colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 36;
			charStyle.Style = FontStyle.Bold;
			characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Heading2);
			charStyle.ColorStyle = colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 24;
			charStyle.Style = FontStyle.Bold;
			characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Heading3);
			charStyle.ColorStyle = colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 16;
			charStyle.Style = FontStyle.Bold;
			characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Normal);
			charStyle.ColorStyle = colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 12;
			charStyle.Style = FontStyle.Regular;
			characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Subtitle);
			charStyle.ColorStyle = colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 12;
			charStyle.Style = FontStyle.Bold;
			characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));
		}


		private void CreateStandardFillStyles() {
			FillStyle fillStyle;

			fillStyle = new FillStyle(FillStyle.StandardNames.Black, colorStyles.Black, colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Blue, colorStyles.Blue, colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Green, colorStyles.Green, colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Red, colorStyles.Red, colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Transparent, colorStyles.Transparent, colorStyles.Transparent);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.White, colorStyles.White, colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Yellow, colorStyles.Yellow, colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));
		}


		private void CreateStandardLineStyles() {
			LineStyle lineStyle;

			lineStyle = new LineStyle(LineStyle.StandardNames.Blue);
			lineStyle.ColorStyle = colorStyles.Blue;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Dashed);
			lineStyle.ColorStyle = colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dash;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Dotted);
			lineStyle.ColorStyle = colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Green);
			lineStyle.ColorStyle = colorStyles.Green;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Highlight);
			lineStyle.ColorStyle = colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.HighlightDashed);
			lineStyle.ColorStyle = colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dash;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.HighlightDotted);
			lineStyle.ColorStyle = colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.HighlightThick);
			lineStyle.ColorStyle = colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 3;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.None);
			lineStyle.ColorStyle = colorStyles.Transparent;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Normal);
			lineStyle.ColorStyle = colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Red);
			lineStyle.ColorStyle = colorStyles.Red;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Special1);
			lineStyle.ColorStyle = colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.DashDot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Special2);
			lineStyle.ColorStyle = colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.DashDotDot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Thick);
			lineStyle.ColorStyle = colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 3;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Yellow);
			lineStyle.ColorStyle = colorStyles.Yellow;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));
		}


		private void CreateStandardParagraphStyles() {
			ParagraphStyle paragraphStyle;

			paragraphStyle = new ParagraphStyle(ParagraphStyle.StandardNames.Label);
			paragraphStyle.Alignment = ContentAlignment.MiddleLeft;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = false;
			paragraphStyles.Add(paragraphStyle, CreatePreviewStyle(paragraphStyle));

			paragraphStyle = new ParagraphStyle(ParagraphStyle.StandardNames.Text);
			paragraphStyle.Alignment = ContentAlignment.TopLeft;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = true;
			paragraphStyles.Add(paragraphStyle, CreatePreviewStyle(paragraphStyle));

			paragraphStyle = new ParagraphStyle(ParagraphStyle.StandardNames.Title);
			paragraphStyle.Alignment = ContentAlignment.MiddleCenter;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = true;
			paragraphStyles.Add(paragraphStyle, CreatePreviewStyle(paragraphStyle));

		}

		#endregion


		#region Fields

		// parameters for preview style creation
		private static bool previewAsGrayScale = true;
		private static float previewTransparencyFactor = 0.66f;

		private object id = null;
		private string name = string.Empty;
		private string description = string.Empty;

		// Style collections
		private CapStyleCollection capStyles = new CapStyleCollection();
		private CharacterStyleCollection characterStyles = new CharacterStyleCollection();
		private ColorStyleCollection colorStyles = new ColorStyleCollection();
		private FillStyleCollection fillStyles = new FillStyleCollection();
		private LineStyleCollection lineStyles = new LineStyleCollection();
		private ParagraphStyleCollection paragraphStyles = new ParagraphStyleCollection();
		private ShapeStyleCollection shapeStyles = new ShapeStyleCollection();
		private const string previewNameSuffix = "";

		#endregion
	}

}