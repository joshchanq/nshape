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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	[ToolboxItem(false)]
	public partial class InPlaceTextBox : RichTextBox {

		/// <summary>
		/// Creates a new InPlaceTextBox instance
		/// </summary>
		/// <param name="owner">The display that owns the nwe instance.</param>
		/// <param name="text">The original text.</param>
		/// <param name="newText">The text that was Types by the user. If empty, the whole text will be preselected, otherwise the new text will be displayed and the cursor will be placed to the end of the text.</param>
		/// <param name="maxTextArea">The maximum text area. If the text does not fit into this area, scrollbars will be displayed.</param>
		/// <param name="characterStyle">The character style of the text.</param>
		/// <param name="paragraphStyle">The paragraph style of the text.</param>
		public InPlaceTextBox(IDiagramPresenter owner, ICaptionedShape shape, int captionIndex, string currentText)
			: this(owner, shape, captionIndex, currentText, null) {
		}


		public InPlaceTextBox(IDiagramPresenter owner, ICaptionedShape shape, int captionIndex, string currentText, string newText) {
			Construct(owner, shape, captionIndex, currentText, newText);
			// Set Text
			originalText = currentText;
			if (string.IsNullOrEmpty(newText)) {
				// Preselect the whole text if the user has not started typing yet
				base.Text = currentText;
				SelectAll();
			} else {
				// Set the types text and place the cursor at the end of the text
				base.Text = newText;
				SelectionStart = Text.Length;
			}
		}


		/// <summary>
		/// The maximum Size of the Control (in Control coordinates), equivivalent to the text's layout area.
		/// </summary>
		public Rectangle BoundsLimit {
			get { return boundsLimit; }
			set { boundsLimit = value; }
		}
		
		
		public ContentAlignment TextAlignment {
			get { return ConvertToContentAlignment(SelectionAlignment); }
			set {
				contentAlignment = value;
				this.SuspendLayout();
				SelectAll();
				SelectionAlignment = ConvertToHorizontalAlignment(value);
				DeselectAll();
				SelectionStart = Text.Length;
				this.ResumeLayout();
			}
		}


		public string OriginalText {
			get { return originalText; }
			set {
				originalText = value;
				if (Text == string.Empty)
					Text = value;
			}
		}
		
		
		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				if (originalText == string.Empty)
					originalText = value;
				InvalidateEx();
			}
		}


		protected void InvalidateEx() {
			if (Parent != null) {
				Rectangle rect = Rectangle.Empty;
				rect = Bounds;
				rect.Inflate(1, 1);
				owner.ControlToDiagram(rect, out rect);
				Parent.Invalidate(rect, true);
			}
			else Invalidate();
		}


		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
				return cp;
			}
		}


		protected override void OnTextChanged(EventArgs e) {
			// measure the current Size of the text
			sizeBuffer = TextMeasurer.MeasureText(Text, Font, layoutArea.Size, paragraphStyle);

			owner.DiagramToControl(sizeBuffer, out sizeBuffer);
			// check if the (layouted) text fits into the TextEditor
			if (Height < sizeBuffer.Height) {
				// grow textEditor
				if (sizeBuffer.Height > boundsLimit.Height)
					Height = boundsLimit.Height;
				else 
					Height = sizeBuffer.Height;
			}
			else if (Height > sizeBuffer.Height) {
				// shrink TextEditor
				if (sizeBuffer.Height < lineHeight)
					Height = lineHeight;
				else
					Height = sizeBuffer.Height;
			}
			base.OnTextChanged(e);
			InvalidateEx();
		}


		protected override void OnResize(EventArgs eventargs) {
			InvalidateEx();
			if (paragraphStyle != null) {
				// move textEditor depending on the vertical text alignment
				switch (paragraphStyle.Alignment) {
					case ContentAlignment.BottomCenter:
					case ContentAlignment.BottomLeft:
					case ContentAlignment.BottomRight:
						Top = boundsLimit.Bottom - Height;
						break;
					case ContentAlignment.MiddleCenter:
					case ContentAlignment.MiddleLeft:
					case ContentAlignment.MiddleRight:
						Top = boundsLimit.Top + (int)Math.Round((boundsLimit.Height / 2f) - (Height / 2f));
						break;
					case ContentAlignment.TopCenter:
					case ContentAlignment.TopLeft:
					case ContentAlignment.TopRight:
						Top = boundsLimit.Top;
						break;
				}
			} else Debug.Print("ParagraphStyle not found!");
			base.OnResize(eventargs);
			InvalidateEx();
		}


		protected override void NotifyInvalidate(Rectangle invalidatedArea) {
			base.NotifyInvalidate(invalidatedArea);
		}


		protected override void OnInvalidated(InvalidateEventArgs e) {
			base.OnInvalidated(e);
		}


		protected override void OnSelectionChanged(EventArgs e) {
			base.OnSelectionChanged(e);
			InvalidateEx();
		}


		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
			InvalidateEx();
		}


		protected override void OnMove(EventArgs e) {
			Invalidate();
			base.OnMove(e);
			InvalidateEx();
		}


		protected override void OnHScroll(EventArgs e) {
			base.OnHScroll(e);
			InvalidateEx();
		}


		protected override void OnVScroll(EventArgs e) {
			base.OnVScroll(e);
			InvalidateEx();
		}


		// The Paint-Methods are not called until the "UserPaint" style is set with the 
		// SetStyle() / UpdateStyles() methods in the constructer
		protected override void OnPaintBackground(PaintEventArgs pevent) {
			Parent.Update();
			//base.OnPaintBackground(pevent);
		}


		// The Paint-Methods are not called until the "UserPaint" style is set with the 
		// SetStyle() / UpdateStyles() methods in the constructor
		protected override void OnPaint(PaintEventArgs e) {
			//e.Graphics.SmoothingMode = infoGfx.SmoothingMode;
			//e.Graphics.TextRenderingHint = infoGfx.TextRenderingHint;
			//e.Graphics.CompositingQuality = infoGfx.CompositingQuality;
			
			Font font = ToolCache.GetFont(characterStyle);
			StringFormat formatter = ToolCache.GetStringFormat(paragraphStyle);
			float fontSize = (characterStyle.SizeInPoints / 72f) * e.Graphics.DpiY;
			System.Drawing.Drawing2D.GraphicsPath textPath = new System.Drawing.Drawing2D.GraphicsPath();

			textPath.Reset();
			textPath.StartFigure();
			textPath.AddString(Text, font.FontFamily, (int)font.Style, fontSize, ClientRectangle, formatter);
			textPath.CloseAllFigures();

			e.Graphics.FillPath(Brushes.Black, textPath);
		}


		private void Construct(IDiagramPresenter owner, ICaptionedShape shape, int captionIndex, string currentText, string newText) {
			if (owner == null) throw new ArgumentNullException("owner");
			if (shape == null) throw new ArgumentNullException("shape");
			if (captionIndex < 0 || captionIndex >= shape.CaptionCount) throw new ArgumentOutOfRangeException("captionIndex");
			// Set Caontrol Styles
			SetStyle(ControlStyles.ResizeRedraw
				| ControlStyles.ResizeRedraw
				| ControlStyles.SupportsTransparentBackColor
				//| ControlStyles.OptimizedDoubleBuffer
				//| ControlStyles.AllPaintingInWmPaint
				//| ControlStyles.UserPaint			// uncomment this line to activateConnectionPoints the OnPaint-Method (see below)
				, true);
			UpdateStyles();

			this.owner = owner;
			this.shape = shape;
			this.captionIndex = captionIndex;
			
			// Set Styles here because the ParagraphStyle is needed for resizing
			characterStyle = shape.GetCaptionCharacterStyle(captionIndex);
			paragraphStyle = shape.GetCaptionParagraphStyle(captionIndex);
			Point tl = Point.Empty, tr = Point.Empty, bl = Point.Empty, br = Point.Empty;
			shape.GetCaptionBounds(captionIndex, out tl, out tr, out br, out bl);

			// calculate unrotated captionBounds
			float angle;
			Point center = Geometry.VectorLinearInterpolation(tl, br, 0.5d);
			angle = Geometry.RadiansToDegrees(Geometry.Angle(tl.X, tl.Y, tr.X, tr.Y));
			tl = Geometry.RotatePoint(center, -angle, tl);
			br = Geometry.RotatePoint(center, -angle, br);
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.Location = tl;
			captionBounds.Width = br.X - tl.X;
			captionBounds.Height = br.Y - tl.Y;

			// Set members
			this.layoutArea = captionBounds;
			owner.DiagramToControl(captionBounds, out boundsLimit);

			// set base' members
			SuspendLayout();
			Font = ToolCache.GetFont(characterStyle);
			this.MaximumSize = boundsLimit.Size;
			this.Bounds = boundsLimit;
			this.BackColor = Color.Transparent;
			this.BorderStyle = BorderStyle.None;
			this.WordWrap = paragraphStyle.WordWrap;
			this.Font = ToolCache.GetFont(characterStyle);
			this.ScrollBars = RichTextBoxScrollBars.None;
			this.ZoomFactor = owner.ZoomLevel / 100f;

			// get line height
			sizeBuffer = TextRenderer.MeasureText(((IDisplayService)owner).InfoGraphics, "Ig", Font);
			owner.DiagramToControl(sizeBuffer, out sizeBuffer);
			lineHeight = sizeBuffer.Height;

			SelectAll();
			SelectionAlignment = ConvertToHorizontalAlignment(paragraphStyle.Alignment);
			DeselectAll();
			ResumeLayout();
		}


		/// <summary>
		/// Returns TextformatFlags used for measuring Text with TextRenderer class.
		/// </summary>
		private TextFormatFlags GetTextFormatFlags(IParagraphStyle paragraphStyle) {
			Debug.Assert(paragraphStyle != null);

			// The flag 'TextBoxControl' ensures that WordBreak behaves exactly like the StringFormat class' WordWrap
			TextFormatFlags result = TextFormatFlags.TextBoxControl;
			//result |= TextFormatFlags.NoPadding;
			//result |= TextFormatFlags.LeftAndRightPadding;

			// set Alignment
			switch (paragraphStyle.Alignment) {
				case ContentAlignment.BottomCenter:
					result |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.BottomLeft:
					result |= TextFormatFlags.Bottom | TextFormatFlags.Left;
					break;
				case ContentAlignment.BottomRight:
					result |= TextFormatFlags.Bottom | TextFormatFlags.Right;
					break;
				case ContentAlignment.MiddleCenter:
					result |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.MiddleLeft:
					result |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
					break;
				case ContentAlignment.MiddleRight:
					result |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
					break;
				case ContentAlignment.TopCenter:
					result |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.TopLeft:
					result |= TextFormatFlags.Top | TextFormatFlags.Left;
					break;
				case ContentAlignment.TopRight:
					result |= TextFormatFlags.Top | TextFormatFlags.Right;
					break;
				default: throw new nShapeUnsupportedValueException(paragraphStyle.Alignment);
			}
			// set WordWrap
			if (paragraphStyle.WordWrap)
				result |= TextFormatFlags.WordBreak;
			// set Trimming
			switch (paragraphStyle.Trimming) {
				case StringTrimming.Character:
					result |= TextFormatFlags.EndEllipsis;		// ToDo: Find out what's suitable here
					break;
				case StringTrimming.EllipsisCharacter:
					result |= TextFormatFlags.EndEllipsis;
					break;
				case StringTrimming.EllipsisPath:
					result |= TextFormatFlags.PathEllipsis;
					break;
				case StringTrimming.EllipsisWord:
					result |= TextFormatFlags.WordEllipsis;
					break;
				case StringTrimming.None:
					result |= TextFormatFlags.NoClipping;
					break;
				case StringTrimming.Word:
					result |= TextFormatFlags.WordEllipsis;		// ToDo: Find out what's suitable here
					break;
			}
			return result;
		}

	
		private HorizontalAlignment ConvertToHorizontalAlignment(ContentAlignment contentAlignment) {
			switch (contentAlignment) {
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					return HorizontalAlignment.Center;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					return HorizontalAlignment.Left;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					return HorizontalAlignment.Right;
				default: throw new nShapeUnsupportedValueException(typeof(ContentAlignment), contentAlignment);
			}
		}


		private ContentAlignment ConvertToContentAlignment(HorizontalAlignment horizontalAlignment) {
			switch (horizontalAlignment) {
				case HorizontalAlignment.Center:
					switch(contentAlignment) {
						case ContentAlignment.BottomCenter:
							return ContentAlignment.BottomCenter;
						case ContentAlignment.MiddleCenter:
							return ContentAlignment.MiddleCenter;
						case ContentAlignment.TopCenter:
							return ContentAlignment.TopCenter;
						default: throw new nShapeUnsupportedValueException(typeof(ContentAlignment), contentAlignment);
					}
				case HorizontalAlignment.Left:
					switch(contentAlignment) {
						case ContentAlignment.BottomLeft:
							return ContentAlignment.BottomLeft;
						case ContentAlignment.MiddleLeft:
							return ContentAlignment.MiddleLeft;
						case ContentAlignment.TopLeft:
							return ContentAlignment.TopLeft;
						default: throw new nShapeUnsupportedValueException(typeof(ContentAlignment), contentAlignment);
					}
				case HorizontalAlignment.Right:
					switch(contentAlignment) {
						case ContentAlignment.BottomRight:
							return ContentAlignment.BottomRight;
						case ContentAlignment.MiddleRight:
							return ContentAlignment.MiddleRight;
						case ContentAlignment.TopRight:
							return ContentAlignment.TopRight;
						default: throw new nShapeUnsupportedValueException(typeof(ContentAlignment), contentAlignment);
					}
				default: throw new nShapeUnsupportedValueException(typeof(HorizontalAlignment), horizontalAlignment);
			}
		}


		#region Fields
		private IDiagramPresenter owner;
		private ICaptionedShape shape;
		private int captionIndex;
		
		private string originalText = string.Empty;
		private IParagraphStyle paragraphStyle;
		private ICharacterStyle characterStyle;

		private Rectangle boundsLimit = Rectangle.Empty;
		private Rectangle layoutArea = Rectangle.Empty;
		private int lineHeight;
		
		private Size sizeBuffer = Size.Empty;
		private ContentAlignment contentAlignment;
		#endregion
	}
}
