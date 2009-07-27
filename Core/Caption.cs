using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;


namespace Dataweb.Diagramming.Advanced {

	/// <summary>
	/// Displays a text within a shape.
	/// </summary>
	/// <status>reviewed</status>
	public class Caption {

		public Caption() {
		}


		public Caption(string text) {
			this.Text = text;
		}


		/// <summary>
		/// Specifies the text of the caption.
		/// </summary>
		public string Text {
			get { return text; }
			set {
				if (string.IsNullOrEmpty(value)) {
					if (textPath != null) {
						textPath.Dispose();
						textPath = null;
					}
				}
				text = value;
				if (!string.IsNullOrEmpty(text)) {
					if (textPath == null) textPath = new GraphicsPath();
				}
			}
		}


		/// <summary>
		/// Calculates the current text's area within the given caption bounds.
		/// </summary>
		public Rectangle CalculateTextBounds(Rectangle captionBounds, ICharacterStyle characterStyle, 
			IParagraphStyle paragraphStyle, IDisplayService displayService) {
			Rectangle textBounds = Rectangle.Empty;
			Debug.Assert(characterStyle != null);
			Debug.Assert(paragraphStyle != null);

			// measure the text size
			//if (float.IsNaN(dpiY)) dpiY = gfx.DpiY;
			if (displayService != null) {
				textBounds.Size = TextMeasurer.MeasureText(displayService.InfoGraphics, string.IsNullOrEmpty(Text)
						? "Ig" : Text, ToolCache.GetFont(characterStyle), captionBounds.Size, paragraphStyle);
			} else textBounds.Size = TextMeasurer.MeasureText(string.IsNullOrEmpty(Text)
				? "Ig" : Text, ToolCache.GetFont(characterStyle), captionBounds.Size, paragraphStyle);

			// clip text bounds if too large
			if (textBounds.Width > captionBounds.Width)
				textBounds.Width = captionBounds.Width;
			if (textBounds.Height > captionBounds.Height)
				textBounds.Height = captionBounds.Height;

			// set horizontal alignment
			switch (paragraphStyle.Alignment) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					textBounds.X = captionBounds.X;
					break;
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					textBounds.X = captionBounds.X + (int)Math.Round((captionBounds.Width - textBounds.Width) / 2f);
					break;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					textBounds.X = captionBounds.Right - textBounds.Width;
					break;
				default: Debug.Assert(false); break;
			}
			// set vertical alignment
			switch (paragraphStyle.Alignment) {
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomRight:
					textBounds.Y = captionBounds.Bottom - textBounds.Height;
					break;
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleRight:
					textBounds.Y = captionBounds.Top + (int)Math.Round((captionBounds.Height - textBounds.Height) / 2f);
					break;
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopRight:
					textBounds.Y = captionBounds.Top;
					break;
				default: Debug.Assert(false); break;
			}
			return textBounds;
		}


		/// <summary>
		/// Resets the caption path.
		/// </summary>
		public void InvalidatePath() {
			if (textPath != null) textPath.Reset();
		}


		/// <summary>
		/// Calculates the caption path in the untransformed state.
		/// </summary>
		/// <param name="x">X coordinate of the layout rectangle</param>
		/// <param name="y">Y coordinate of the layout rectangle</param>
		/// <param name="width">Width of the layout rectangle</param>
		/// <param name="height">Height of the layout rectangle</param>
		/// <param name="characterStyle">Character style of the caption</param>
		/// <param name="paragraphStyle">Paragraph style of the caption</param>
		/// <returns></returns>
		public bool CalculatePath(int layoutX, int layoutY, int layoutW, int layoutH, 
			ICharacterStyle characterStyle, IParagraphStyle paragraphStyle) {
			if (characterStyle == null) throw new ArgumentNullException("charStyle");
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			if (string.IsNullOrEmpty(text))
				return true;
			else if (textPath != null && layoutW > 0 && layoutH > 0) {
				Font font = ToolCache.GetFont(characterStyle);
				StringFormat formatter = ToolCache.GetStringFormat(paragraphStyle);
				Rectangle textBounds = Rectangle.Empty;
				textBounds.X = layoutX;
				textBounds.Y = layoutY;
				textBounds.Width = layoutW;
				textBounds.Height = layoutH;
				textPath.Reset();
				textPath.StartFigure();
				textPath.AddString(text, font.FontFamily, (int)font.Style, characterStyle.Size, textBounds, formatter);
				textPath.CloseFigure();
#if DEBUG
				if (textPath.PointCount == 0) {
					Size textSize = TextMeasurer.MeasureText(text, font, textBounds.Size, paragraphStyle);
					Debug.Fail("Failed to create TextPath - please check if the caption bounds are too small for the text.");
				}
#endif
				return true;
			}
			return false;
		}


		public void TransformPath(Matrix matrix) {
			if (matrix == null) throw new ArgumentNullException("matrix");
			if (textPath != null) textPath.Transform(matrix);
		}


		public void Draw(Graphics graphics, ICharacterStyle characterStyle, IParagraphStyle paragraphStyle) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (characterStyle == null) throw new ArgumentNullException("charStyle");
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			if (textPath != null && textPath.PointCount > 0) {
				Brush brush = ToolCache.GetBrush(characterStyle.ColorStyle);
				graphics.FillPath(brush, textPath);
			}
		}


		#region Fields

		// Caption text
		private string text = string.Empty;
		// Graphics path of the text
		private GraphicsPath textPath;

		#endregion
	}


	#region CaptionedShape interface

	/// <summary>
	/// Represents a shape with one or more captions in it.
	/// </summary>
	/// <status>reviewed</status>
	public interface ICaptionedShape {

		/// <summary>
		/// The number of captions the shape currently contains.
		/// </summary>
		int CaptionCount { get; }

		/// <summary>
		/// Retrieves the text of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		string GetCaptionText(int index);

		/// <summary>
		/// Sets the text of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="text"></param>
		void SetCaptionText(int index, string text);

		/// <summary>
		/// Retrieves the character style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		ICharacterStyle GetCaptionCharacterStyle(int index);

		/// <summary>
		/// Sets the character style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="characterStyle"></param>
		void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle);

		/// <summary>
		/// Retrieves the paragraph style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		IParagraphStyle GetCaptionParagraphStyle(int index);

		/// <summary>
		/// Sets the paragraph style of the indicated caption.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="paragraphStyle"></param>
		void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle);

		/// <summary>
		/// Retrieves the transformed bounds of the caption in diagram coordinates. These 
		/// bounds define the maximum area the caption text can occupy.
		/// </summary>
		/// <param name="index">Index of the caption</param>
		/// <param name="topLeft">The top left corner of the transformed rectangle defining 
		/// the bounds of the caption</param>
		/// <param name="topRight">The top right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomRight">The bottom right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomLeft">The top bottom left of the transformed rectangle 
		/// defining the bounds of the caption</param>
		bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, 
			out Point bottomLeft);

		/// <summary>
		/// Retrieves the transformed bounds of the caption's text in diagram coordinates. 
		/// These bounds define the current area of the text in the caption.
		/// </summary>
		/// <param name="index">Index of the caption</param>
		/// <param name="topLeft">The top left corner of the transformed rectangle defining 
		/// the bounds of the caption</param>
		/// <param name="topRight">The top right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomRight">The bottom right corner of the transformed rectangle 
		/// defining the bounds of the caption</param>
		/// <param name="bottomLeft">The top bottom left of the transformed rectangle 
		/// defining the bounds of the caption</param>
		bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, 
			out Point bottomRight, out Point bottomLeft);

		/// <summary>
		/// Finds a caption which contains the given point.
		/// </summary>
		/// <returns>Caption index of -1 if none found.</returns>
		int FindCaptionFromPoint(int x, int y);
	}

	#endregion


	#region CaptionedShapeBase Class
	
	/// <summary>
	/// Shape having one caption.
	/// </summary>
	/// <status>reviewed</status>
	public abstract class CaptionedShapeBase : PathBasedPlanarShape, ICaptionedShape {

		[PropertyMappingId(PropertyIdText)]
		[Category("Text Layout")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Text displayed inside the shape")]
		[Editor("Dataweb.Diagramming.WinFormsUI.DiagrammingTextEditor, Dataweb.Diagramming.WinFormsUI", typeof(UITypeEditor))]
		public virtual string Text {
			get {
				if (caption == null) return string.Empty;
				else return caption.Text;
			}
			set {
				Invalidate();
				if (caption == null) caption = new Caption(value);
				else caption.Text = value;
				if (string.IsNullOrEmpty(caption.Text)) caption = null;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[PropertyMappingId(PropertyIdCharacterStyle)]
		[Category("Text Appearance")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Determines the style of the shape's text.")]
		public ICharacterStyle CharacterStyle {
			get { return privateCharacterStyle ?? ((CaptionedShapeBase)Template.Shape).CharacterStyle; }
			set {
				Invalidate();
				privateCharacterStyle = (Template != null && value == ((CaptionedShapeBase)Template.Shape).CharacterStyle) ? null : value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[PropertyMappingId(PropertyIdParagraphStyle)]
		[Category("Text Appearance")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Determines the layout of the shape's text.")]
		public IParagraphStyle ParagraphStyle {
			get { return privateParagraphStyle ?? ((CaptionedShapeBase)Template.Shape).ParagraphStyle; }
			set {
				Invalidate();
				privateParagraphStyle = (Template != null && value == ((CaptionedShapeBase)Template.Shape).ParagraphStyle) ? null : value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			CharacterStyle = styleSet.CharacterStyles.Normal;
			ParagraphStyle = styleSet.ParagraphStyles.Title;
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is ICaptionedShape) {
				ICaptionedShape src = (ICaptionedShape)source;
				// Copy the first caption of the source
				ICharacterStyle charStyle = src.GetCaptionCharacterStyle(0);
				privateCharacterStyle = (Template != null && charStyle == ((CaptionedShapeBase)Template.Shape).CharacterStyle) ? null : charStyle;
				
				IParagraphStyle paragraphStyle = src.GetCaptionParagraphStyle(0);
				privateParagraphStyle = (Template != null && paragraphStyle == ((CaptionedShapeBase)Template.Shape).ParagraphStyle) ? null : paragraphStyle;
				
				string txt = src.GetCaptionText(0);
				if (!string.IsNullOrEmpty(txt)) {
					if (caption == null) caption = new Caption(txt);
					else caption.Text = txt;
				} else caption = null;
			}
		}


		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			privateCharacterStyle = styleSet.GetPreviewStyle(CharacterStyle);
			privateParagraphStyle = styleSet.GetPreviewStyle(ParagraphStyle);
		}


		public override bool NotifyStyleChanged(IStyle style) {
			bool result = base.NotifyStyleChanged(style);
			if (style == null || IsStyleAffected(ParagraphStyle, style) || IsStyleAffected(CharacterStyle, style)) {
				Invalidate();
				InvalidateDrawCache();
				Invalidate();
				result = true;
			}
			return result;
		}


		#region ICaptionedShape Members

		public virtual int CaptionCount {
			get { return 1; }
		}


		public virtual bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new IndexOutOfRangeException("index");
			//if (caption == null) {
			//   topLeft = topRight = bottomLeft = bottomRight = Center;
			//   return false;
			//} else {
				// calc transformed layout caption bounds
				Rectangle captionBounds = Rectangle.Empty;
				CalcCaptionBounds(index, out captionBounds);
				Geometry.TransformRectangle(Center, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
				return true;
			//}
		}


		public virtual bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new IndexOutOfRangeException("index");
			if (caption == null) {
				topLeft = topRight = bottomLeft = bottomRight = Center;
				return false;
			} else {
				Rectangle captionBounds = Rectangle.Empty;
				CalcCaptionBounds(index, out captionBounds);
				Rectangle textBounds = caption.CalculateTextBounds(captionBounds, CharacterStyle, ParagraphStyle, DisplayService);
				Geometry.TransformRectangle(Center, Angle, textBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
				return true;
			}
		}


		public virtual string GetCaptionText(int index) {
			if (index != 0) throw new DiagrammingException("NotSupported label index.");
			else return Text;
		}


		public virtual ICharacterStyle GetCaptionCharacterStyle(int index) {
			if (index != 0) throw new DiagrammingException("NotSupported label index.");
			return CharacterStyle;
		}


		public virtual IParagraphStyle GetCaptionParagraphStyle(int index) {
			if (index != 0) throw new DiagrammingException("NotSupported label index.");
			return ParagraphStyle;
		}


		public virtual void SetCaptionText(int index, string text) {
			if (index != 0) throw new DiagrammingException("NotSupported label index.");
			else Text = text;
		}


		public virtual void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			if (index != 0) throw new DiagrammingException("NotSupported label index.");
			CharacterStyle = characterStyle;
		}


		public virtual void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			if (index != 0) throw new DiagrammingException("NotSupported label index.");
			ParagraphStyle = paragraphStyle;
		}


		public virtual int FindCaptionFromPoint(int x, int y) {
			for (int i = 0; i < CaptionCount; ++i) {
				Point tl = Point.Empty, tr = Point.Empty, br = Point.Empty, bl = Point.Empty;
				if (GetCaptionTextBounds(i, out tl, out tr, out br, out bl)) {
					if (Geometry.QuadrangleContainsPoint(x, y, tl, tr, br, bl))
						return i;
				}
			}
			return -1;
		}

		#endregion


		#region IEntity Members

		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, 1);

			// ILabel members
			this.privateCharacterStyle = reader.ReadCharacterStyle();
			this.privateParagraphStyle = reader.ReadParagraphStyle();

			string txt = reader.ReadString();
			if (caption == null) caption = new Caption(txt);
			else caption.Text = txt;
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			// ILabel members
			writer.WriteStyle(privateCharacterStyle);
			writer.WriteStyle(privateParagraphStyle);
			writer.WriteString(Text);
		}


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in PathBasedPlanarShape.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("CharacterStyle", typeof(object));
			yield return new EntityFieldDefinition("ParagraphStyle", typeof(object));
			yield return new EntityFieldDefinition("Text", typeof(string));
		}

		#endregion


		protected internal CaptionedShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal CaptionedShapeBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdText: Text = propertyMapping.GetString(); break;
				case PropertyIdCharacterStyle: 
					// assign private stylebecause if the style matches the template's style, it would not be assigned.
					privateCharacterStyle = propertyMapping.GetStyle() as ICharacterStyle;
					Invalidate();
					break;
				case PropertyIdParagraphStyle:
					// assign private stylebecause if the style matches the template's style, it would not be assigned.
					ParagraphStyle = propertyMapping.GetStyle() as IParagraphStyle;
					Invalidate();
					break;
				default: base.ProcessExecModelPropertyChange(propertyMapping); break;
			}
		}


		/// <summary>
		/// Calculates the untransformed area in which the caption's text is layouted.
		/// </summary>
		/// <remarks> The caller has to rotate and offset the rectangle around/by X|Y before using it.</remarks>
		protected abstract void CalcCaptionBounds(int index, out Rectangle captionBounds);


		protected override bool CalculatePath() {
			if (caption == null) return true;
			Rectangle layoutRectangle = Rectangle.Empty;
			CalcCaptionBounds(0, out layoutRectangle);
			return caption.CalculatePath(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, CharacterStyle, ParagraphStyle);
		}


		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			// transform DrawCache only if the drawCache is valid, otherwise it will be recalculated
			// at the correct position/size
			if (!drawCacheIsInvalid && caption != null) caption.TransformPath(Matrix);
		}


		protected void DrawCaption(Graphics graphics) {
			if (caption != null) caption.Draw(graphics, CharacterStyle, ParagraphStyle);
		}


		#region Fields

		protected const int PropertyIdText = 4;
		protected const int PropertyIdCharacterStyle = 5;
		protected const int PropertyIdParagraphStyle = 6;

		private Caption caption;

		// private styles
		private ICharacterStyle privateCharacterStyle = null;
		private IParagraphStyle privateParagraphStyle = null;

		#endregion
	}

	#endregion
}
