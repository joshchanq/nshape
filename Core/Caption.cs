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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;


namespace Dataweb.NShape.Advanced {

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
			get { return captionText; }
			set {
				if (string.IsNullOrEmpty(value)) {
					GdiHelpers.DisposeObject(ref textPath);
					captionTextSuffix = null;
				}
				captionText = value;
				if (!string.IsNullOrEmpty(captionText)) {
					captionTextSuffix = captionText.EndsWith("\n") ? "\n" : null;
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
				textBounds.Size = TextMeasurer.MeasureText(displayService.InfoGraphics, string.IsNullOrEmpty(captionText)
						? "Ig" : captionText, ToolCache.GetFont(characterStyle), captionBounds.Size, paragraphStyle);
			} else textBounds.Size = TextMeasurer.MeasureText(string.IsNullOrEmpty(captionText)
				? "Ig" : captionText, ToolCache.GetFont(characterStyle), captionBounds.Size, paragraphStyle);

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
		/// <param name="layoutX">X coordinate of the layout rectangle</param>
		/// <param name="layoutY">Y coordinate of the layout rectangle</param>
		/// <param name="layoutW">Width of the layout rectangle</param>
		/// <param name="layoutH">Height of the layout rectangle</param>
		/// <param name="characterStyle">Character style of the caption</param>
		/// <param name="paragraphStyle">Paragraph style of the caption</param>
		/// <returns></returns>
		public bool CalculatePath(int layoutX, int layoutY, int layoutW, int layoutH, ICharacterStyle characterStyle, IParagraphStyle paragraphStyle) {
			if (characterStyle == null) throw new ArgumentNullException("charStyle");
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			if (string.IsNullOrEmpty(captionText))
				return true;
			else if (textPath != null /*&& layoutW > 0 && layoutH > 0*/) {
				// Collect bjects for calculating text layout
				Font font = ToolCache.GetFont(characterStyle);
				StringFormat formatter = ToolCache.GetStringFormat(paragraphStyle);
				Rectangle textBounds = Rectangle.Empty;
				textBounds.X = layoutX + paragraphStyle.Padding.Left;
				textBounds.Y = layoutY + paragraphStyle.Padding.Top;
				textBounds.Width = Math.Max(1, layoutW - paragraphStyle.Padding.Horizontal);
				textBounds.Height = Math.Max(1, layoutH - paragraphStyle.Padding.Vertical);
				// Create text path
				textPath.Reset();
				textPath.StartFigure();
				textPath.AddString(PathText, font.FontFamily, (int)font.Style, characterStyle.Size, textBounds, formatter);
				textPath.CloseFigure();
#if DEBUG
				if (textPath.PointCount == 0 && PathText.Trim() != string.Empty) {
					Size textSize = TextMeasurer.MeasureText(PathText, font, textBounds.Size, paragraphStyle);
					Debug.Print("Failed to create TextPath - please check if the caption bounds are too small for the text.");
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


		private string PathText {
			get {
				if (captionText == null) return captionText;
				else return captionText + captionTextSuffix;
			}
		}


		#region Fields

		// The caption's text
		private string captionText = string.Empty;
		// As a trailing line break will always be ignored when creating a graphics path or when 
		// measuring the text we add an other 'dummy' line break in case the text ends with a line break.
		private string captionTextSuffix = null;
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
	/// <remarks>RequiredPermissions set</remarks>
	/// <status>reviewed</status>
	public abstract class CaptionedShapeBase : PathBasedPlanarShape, ICaptionedShape {

		[Category("Text")]
		[Description("Text displayed inside the shape")]
		[PropertyMappingId(PropertyIdText)]
		[RequiredPermission(Permission.ModifyData)]
		[Editor("Dataweb.NShape.WinFormsUI.TextEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
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


		[Category("Text")]
		[Description("Determines the style of the shape's text.")]
		[PropertyMappingId(PropertyIdCharacterStyle)]
		[RequiredPermission(Permission.Present)]
		public ICharacterStyle CharacterStyle {
			get { return privateCharacterStyle ?? ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0); }
			set {
				Invalidate();
				privateCharacterStyle = (Template != null && value == ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0)) ? null : value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[Category("Text")]
		[Description("Determines the layout of the shape's text.")]
		[RequiredPermission(Permission.Present)]
		[PropertyMappingId(PropertyIdParagraphStyle)]
		public IParagraphStyle ParagraphStyle {
			get { return privateParagraphStyle ?? ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0); }
			set {
				Invalidate();
				privateParagraphStyle = (Template != null && value == ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0)) ? null : value;
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
				// Copy first caption
				ICharacterStyle charStyle = src.GetCaptionCharacterStyle(0);
				privateCharacterStyle = (Template != null && charStyle == ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0)) ? null : charStyle;
				IParagraphStyle paragraphStyle = src.GetCaptionParagraphStyle(0);
				privateParagraphStyle = (Template != null && paragraphStyle == ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0)) ? null : paragraphStyle;
				string txt = src.GetCaptionText(0);
				if (!string.IsNullOrEmpty(txt)) {
					if (caption == null) caption = new Caption(txt);
					else caption.Text = txt;
				} else caption = null;

				// Copy remaining captions
				int cnt = Math.Min(CaptionCount, src.CaptionCount);
				for (int i = 1; i < cnt; ++i) {
					SetCaptionCharacterStyle(i, src.GetCaptionCharacterStyle(i));
					SetCaptionParagraphStyle(i, GetCaptionParagraphStyle(i));
					SetCaptionText(i, GetCaptionText(i));
				}
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
			if (index != 0) throw new NShapeException("NotSupported label index.");
			else return Text;
		}


		public virtual ICharacterStyle GetCaptionCharacterStyle(int index) {
			if (index != 0) throw new NShapeException("NotSupported label index.");
			return CharacterStyle;
		}


		public virtual IParagraphStyle GetCaptionParagraphStyle(int index) {
			if (index != 0) throw new NShapeException("NotSupported label index.");
			return ParagraphStyle;
		}


		public virtual void SetCaptionText(int index, string text) {
			if (index != 0) throw new NShapeException("NotSupported label index.");
			else Text = text;
		}


		public virtual void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			if (index != 0) throw new NShapeException("NotSupported label index.");
			CharacterStyle = characterStyle;
		}


		public virtual void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			if (index != 0) throw new NShapeException("NotSupported label index.");
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
				case PropertyIdText: 
					Text = propertyMapping.GetString(); 
					break;
				case PropertyIdCharacterStyle: 
					// assign private stylebecause if the style matches the template's style, it would not be assigned.
					CharacterStyle = propertyMapping.GetStyle() as ICharacterStyle;
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
			bool result = false;
			Rectangle layoutRectangle = Rectangle.Empty;
			CalcCaptionBounds(0, out layoutRectangle);
			result = caption.CalculatePath(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, CharacterStyle, ParagraphStyle);
			if (maintainTextAngle && Angle > 900 && Angle <= 2700) {
				// Flip text in order to maintain its orientation
				Matrix.Reset();
				PointF rotationCenter = PointF.Empty;
				rotationCenter.X = layoutRectangle.X + (layoutRectangle.Width / 2f);
				rotationCenter.Y = layoutRectangle.Y + (layoutRectangle.Height / 2f);
				Matrix.RotateAt(180, rotationCenter, MatrixOrder.Append);
				caption.TransformPath(Matrix);
			}
			return result;
		}


		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			// transform DrawCache only if the drawCache is valid, otherwise it will be recalculated
			// at the correct position/size
			if (!drawCacheIsInvalid && caption != null) 
				caption.TransformPath(Matrix);
		}


		protected void DrawCaption(Graphics graphics) {
			if (caption != null) caption.Draw(graphics, CharacterStyle, ParagraphStyle);
		}


		#region Fields

		protected const int PropertyIdText = 4;
		protected const int PropertyIdCharacterStyle = 5;
		protected const int PropertyIdParagraphStyle = 6;
		protected bool maintainTextAngle = true;

		private Caption caption;
		// private styles
		private ICharacterStyle privateCharacterStyle = null;
		private IParagraphStyle privateParagraphStyle = null;

		#endregion
	}

	#endregion
}
