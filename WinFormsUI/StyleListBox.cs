using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming.WinFormsUI {
	
	public delegate void StyleSelectedEventHandler(object sender, EventArgs e);

		
	[ToolboxItem(false)]
	internal partial class StyleListBox : ListBox {

		public StyleListBox() {
			Construct();
		}


		public StyleListBox(IWindowsFormsEditorService editorService, Design design, Style style, bool showItemDefaultStyle) {
			if (editorService == null) throw new ArgumentNullException("editorService");
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");			
			Construct();
			Initialize(editorService, design, showItemDefaultStyle, style.GetType(), style);
		}


		public StyleListBox(IWindowsFormsEditorService editorService, Design design, Type selectedStyleType, bool showItemDefaultStyle) {
			if (editorService == null) throw new ArgumentNullException("editorService");
			if (design == null) throw new ArgumentNullException("design");
			if (selectedStyleType == null) throw new ArgumentNullException("selectedStyleType");
			Construct();
			Initialize(editorService, design, showItemDefaultStyle, selectedStyleType, null);
		}


		public StyleListBox(Design design, Style style, bool showItemDefaultStyle) {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			Construct();
			Initialize(null, design, showItemDefaultStyle, style.GetType(), style);
		}


		public StyleListBox(IWindowsFormsEditorService editorService) {
			if (editorService == null) throw new ArgumentNullException("editorService");
			Construct();
			this.editorService = editorService;
		}


		#region [Public] Properties and Events

		public Style SelectedStyle {
			get {
				if (base.SelectedItem == null || string.Empty.Equals(base.SelectedItem)) return null;
				else return (Style)base.SelectedItem; 
			}
			set { base.SelectedItem = value; }
		}


		public StyleCategory StyleCategory {
			get { return styleCategory; }
			set {
				bool clearItems = styleCategory != value;
				if (clearItems) {
					SuspendLayout();
					Items.Clear();
				}
				
				styleCategory = value;
				if (Items.Count == 0)
					CreateListBoxItems();
				
				if (clearItems) ResumeLayout();
			}
		}


		[ReadOnly(true)]
		[Browsable(false)]
		public Design Design {
			get { return design; }
			set {
				if (design != value) {
					design = value;
					Items.Clear();
				}
			}
		}


		public bool HighlightItems {
			get { return highlightItems; }
			set { highlightItems = value; }
		}


		public Color ItemBackgroundColor {
			get { return itemBackgroundColor; }
			set {
				if (itemBackgroundBrush != null) {
					itemBackgroundBrush.Dispose();
					itemBackgroundBrush = null;
				}
				itemBackgroundColor = value;
			}
		}


		public Color ItemHighlightedColor {
			get { return itemHighlightedColor; }
			set {
				if (itemHighlightedBrush != null) {
					itemHighlightedBrush.Dispose();
					itemHighlightedBrush = null;
				}
				itemHighlightedColor = value;				
			}
		}


		public Color ItemSelectedColor {
			get { return itemSelectedColor; }
			set {
				if (itemSelectedBrush != null) {
					itemSelectedBrush.Dispose();
					itemSelectedBrush = null;
				}
				itemSelectedColor = value;
			}
		}


		public Color ItemFocusedColor {
			get { return itemFocusedColor; }
			set {
				if (itemFocusedBrush != null) {
					itemFocusedBrush.Dispose();
					itemFocusedBrush = null;
				}
				itemFocusedColor = value;
			}
		}
		
		
		public Color FocusBorderColor {
			get { return focusBorderColor; }
			set {
				if (focusBorderPen != null) {
					focusBorderPen.Dispose();
					focusBorderPen = null;
				}
				focusBorderColor = value; 
			}
		}


		public Color ItemBorderColor {
			get { return itemBorderColor; }
			set {
				if (itemBorderPen != null) {
					itemBorderPen.Dispose();
					itemBorderPen = null;
				}
				itemBorderColor = value; 
			}
		}


		public Color TextColor {
			get { return textColor; }
			set {
				if (textBrush != null) {
					textBrush.Dispose();
					textBrush = null;
				}
				textColor = value; 
			}
		}


		#endregion


		#region [Public] Methods
		
		public void Clear() {
			Items.Clear();
			SelectedItem = null;
		}

		
		public void UpdateDesign() {
			CreateListBoxItems();
		}

		#endregion


		#region [Protected] Methods (overridden)

		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			
			if (e.KeyData == Keys.Return || e.KeyData == Keys.Space) {
				ExecuteSelection();
			}
		}


		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left) {
				for (int i = 0; i < Items.Count; ++i) {
					if (GetItemRectangle(i).Contains(e.Location)) {
						ExecuteSelection();
						break;
					}
				}
			}
		}


		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
		}


		protected override void OnPaintBackground(PaintEventArgs e) {
		    //base.OnPaintBackground(e);
		    // do nothing because of DoubleBuffering
		}


		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaintBackground(e);
			base.OnPaint(e);
		}


		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			if (Items.Count > 0) {
				e.ItemWidth = Width;
				if (!object.Equals(Items[e.Index], string.Empty)) {
					switch (styleCategory) {
						// Line Cap Styles
						case StyleCategory.CapStyle:
							e.ItemHeight = ((CapStyle)Items[e.Index]).CapSize;
							break;

						case StyleCategory.ColorStyle:
						case StyleCategory.FillStyle:
							//case StyleCategory.ShapeStyle:
							//case StyleCategory.NoStyle:
							e.ItemHeight = stdItemHeight;
							break;

						case StyleCategory.CharacterStyle:
							CharacterStyle characterStyle = (CharacterStyle)Items[e.Index];
							Font font = ToolCache.GetFont(characterStyle);
							e.ItemHeight = (int)Math.Ceiling(font.GetHeight(e.Graphics));

							//e.ItemHeight = (int)Math.Round((characterStyle.Size / 72) * e.Graphics.DpiY);
							//Size s = TextRenderer.MeasureText(e.Graphics, "Üp", font);
							//e.ItemHeight = s.Height;
							break;

						case StyleCategory.LineStyle:
							e.ItemHeight = ((LineStyle)Items[e.Index]).LineWidth + 4;
							break;

						case StyleCategory.ParagraphStyle:
							e.ItemHeight = dblItemHeight + dblItemHeight;
							break;

						default:
							throw new DiagrammingException(string.Format("Unexpected enum value '{0}'.", styleCategory));
					}
				}
				else
					base.OnMeasureItem(e);
				
				// correct calculated Height by the Height of the label's font
				float fontSizeInPixels = Font.GetHeight(e.Graphics);
				if (fontSizeInPixels > e.ItemHeight)
					e.ItemHeight = (int)Math.Round(fontSizeInPixels);
				e.ItemHeight += 4;
				if (e.ItemHeight < stdItemHeight)
					e.ItemHeight = 20;
			}
		}


		protected override void OnDrawItem(DrawItemEventArgs e) {
			itemBounds.X = e.Bounds.X + 3;
			itemBounds.Y = e.Bounds.Y + 1;
			itemBounds.Width = (e.Bounds.Right - 3) - (e.Bounds.X + 3);
			itemBounds.Height = (e.Bounds.Bottom - 1) - (e.Bounds.Y + 1);

			previewRect.X = itemBounds.X + margin;
			previewRect.Y = itemBounds.Y + margin;
			previewRect.Width = (itemBounds.Width / 2) - (2 * margin);
			previewRect.Height = (itemBounds.Bottom - margin) - (itemBounds.Y + margin);

			lableLayoutRect.X = previewRect.Right + 4;
			lableLayoutRect.Y = previewRect.Y;
			lableLayoutRect.Width = (itemBounds.Width - 4) - (previewRect.Right + 4);
			lableLayoutRect.Height = previewRect.Height;

			// Draw Item Background and Border
			e.Graphics.FillRectangle(ItemBackgroundBrush, itemBounds);
			if (itemBorderColor != Color.Transparent)
				e.Graphics.DrawRectangle(ItemBorderPen, itemBounds);

			// Draw Selection and/or Focus markers
			if ((e.State & DrawItemState.Selected) != 0)
				e.Graphics.FillRectangle(ItemSelectedBrush, itemBounds);
			if ((e.State & DrawItemState.Focus) != 0) {
				if (itemFocusedColor != Color.Transparent)
					e.Graphics.FillRectangle(ItemFocusedBrush, itemBounds);
				if (FocusBorderColor != Color.Transparent)
					e.Graphics.DrawRectangle(FocusBorderPen, itemBounds);
			}
			else if (HighlightItems && (e.State & DrawItemState.HotLight) != 0)
				if (ItemHighlightedColor != Color.Transparent)
					e.Graphics.FillRectangle(ItemHighlightedBrush, itemBounds);

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			if (Items.Count > 0 && e.Index >= 0) {
				if (object.Equals(Items[e.Index], string.Empty)) {
					e.Graphics.DrawString("Default Style", e.Font, TextBrush, lableLayoutRect, formatter);
				}
				else {
					switch (StyleCategory) {
						case StyleCategory.CapStyle:
							DrawCapStyleItem((CapStyle)Items[e.Index], e);
							break;
						case StyleCategory.ColorStyle:
							ColorStyle colorStyle = (ColorStyle)Items[e.Index];
							Brush colorBrush = ToolCache.GetBrush(colorStyle);
							e.Graphics.FillRectangle(colorBrush, previewRect);
							e.Graphics.DrawRectangle(ItemBorderPen, previewRect);
							e.Graphics.DrawRectangle(Pens.Black, previewRect);
							e.Graphics.DrawString(colorStyle.Title, e.Font, TextBrush, lableLayoutRect, formatter);
							break;
						case StyleCategory.FillStyle:
							DrawFillStyleItem((FillStyle)Items[e.Index], e);
							break;
						case StyleCategory.CharacterStyle:
							CharacterStyle charStyle = (CharacterStyle)Items[e.Index];
							Font font = ToolCache.GetFont(charStyle);
							Brush fontBrush = ToolCache.GetBrush(charStyle.ColorStyle);
							e.Graphics.DrawString(string.Format("{0} {1} pt", font.FontFamily.Name, font.SizeInPoints), font, fontBrush, previewRect, formatter);
							e.Graphics.DrawString(charStyle.Title, e.Font, TextBrush, lableLayoutRect, formatter);
							break;
						case StyleCategory.LineStyle:
							LineStyle lineStyle = (LineStyle)Items[e.Index];
							Pen linePen = ToolCache.GetPen(lineStyle, null, null);
							e.Graphics.DrawLine(linePen, previewRect.X, previewRect.Y + (previewRect.Height / 2), previewRect.Right, previewRect.Y + (previewRect.Height / 2));
							e.Graphics.DrawString(lineStyle.Title, e.Font, TextBrush, lableLayoutRect, formatter);
							break;
						case StyleCategory.ParagraphStyle:
							ParagraphStyle paragraphStyle = (ParagraphStyle)Items[e.Index];
							StringFormat stringFormat = ToolCache.GetStringFormat(paragraphStyle);
							Rectangle r = Rectangle.Empty;
							r.X = previewRect.Left + paragraphStyle.Padding.Left;
							r.Y = previewRect.Top + paragraphStyle.Padding.Top;
							r.Width = previewRect.Width - (paragraphStyle.Padding.Left + paragraphStyle.Padding.Right);
							r.Height = previewRect.Height - (paragraphStyle.Padding.Top + paragraphStyle.Padding.Bottom);
							e.Graphics.DrawString(previewText, e.Font, TextBrush, r, stringFormat);
							e.Graphics.DrawRectangle(Pens.Black, previewRect);
							e.Graphics.DrawString(paragraphStyle.Title, e.Font, TextBrush, lableLayoutRect, formatter);
							break;
						default:
							throw new DiagrammingException(string.Format("Unexpected enum value '{0}'.", styleCategory));
					}
				}
			}
		}

		#endregion


		#region [Private] Methods

		private void Construct() {
			// Initialize Components
			InitializeComponent();
			DoubleBuffered = true;

			this.matrix = new Matrix();
			this.formatterFlags = 0 | StringFormatFlags.NoWrap;
			this.formatter = new StringFormat(formatterFlags);
			this.formatter.Trimming = StringTrimming.Character;
			this.formatter.Alignment = StringAlignment.Near;
			this.formatter.LineAlignment = StringAlignment.Center;
		}


		private void Initialize(IWindowsFormsEditorService editorService, Design design, 
			bool showItemDefaultStyle, Type styleType, IStyle style) {
			
			this.editorService = editorService;
			this.design = design;
			this.showItemDefaultStyle = showItemDefaultStyle;

			StyleCategoryFromType(styleType);
			if (style != null) SelectedItem = style;
		}


		private void StyleCategoryFromType(Type styleType) {
			if (styleType == typeof(CapStyle))
				StyleCategory = StyleCategory.CapStyle;
			else if (styleType == typeof(CharacterStyle))
				StyleCategory = StyleCategory.CharacterStyle;
			else if (styleType == typeof(ColorStyle))
				StyleCategory = StyleCategory.ColorStyle;
			else if (styleType == typeof(FillStyle))
				StyleCategory = StyleCategory.FillStyle;
			else if (styleType == typeof(LineStyle))
				StyleCategory = StyleCategory.LineStyle;
			else if (styleType == typeof(ParagraphStyle))
				StyleCategory = StyleCategory.ParagraphStyle;
			//else if (styleType == typeof(ShapeStyle))
			//   StyleCategory = StyleCategory.ShapeStyle;
			else
				throw new DiagrammingException("Type StyleListBox does not support Type {0}.", styleType.Name);
		}


		private void CreateListBoxItems() {
			SuspendLayout();
			if (Items.Count > 0)
				Items.Clear();

			if (design != null) {
				if (showItemDefaultStyle)
					Items.Add(string.Empty);
				int cnt;
				switch (styleCategory) {
					// Line Cap Styles
					case StyleCategory.CapStyle:
						cnt = design.CapStyles.Count;
						for (int i = 0; i < cnt; ++i)
							Items.Add(design.CapStyles[i]);
						break;

					// Font Styles
					case StyleCategory.CharacterStyle:
						cnt = design.CharacterStyles.Count;
						for (int i = 0; i < cnt; ++i)
							Items.Add(design.CharacterStyles[i]);
						break;

					// Color Styles
					case StyleCategory.ColorStyle:
						cnt = design.ColorStyles.Count;
						for (int i = 0; i < cnt; ++i)
							Items.Add(design.ColorStyles[i]);
						break;

					// Fill Styles
					case StyleCategory.FillStyle:
						cnt = design.FillStyles.Count;
						for (int i = 0; i < cnt; ++i)
							Items.Add(design.FillStyles[i]);
						break;

					// Line Styles
					case StyleCategory.LineStyle:
						cnt = design.LineStyles.Count;
						for (int i = 0; i < cnt; ++i)
							Items.Add(design.LineStyles[i]);
						break;

					// Title Styles
					case StyleCategory.ParagraphStyle:
						cnt = design.ParagraphStyles.Count;
						for (int i = 0; i < cnt; ++i)
							Items.Add(design.ParagraphStyles[i]);
						break;

					//// Shape Styles
					//case StyleCategory.ShapeStyle:
					//   cnt = design.ShapeStyles.Count;
					//   for (int i = 0; i < cnt; ++i)
					//      Items.Add(design.ShapeStyles[i]);
					//   break;

					//case StyleCategory.NoStyle:
					//   // nothing to do
					//   break;

					default:
						throw new DiagrammingException(string.Format("Unexpected enum value '{0}'.", StyleCategory));
				}
			}
			ResumeLayout();
			Invalidate();
		}
		
		
		private void ExecuteSelection() {
			if (editorService != null)
				editorService.CloseDropDown();
			if (SelectedStyle is CapStyle || SelectedStyle is LineStyle)
				ToolCache.NotifyStyleChanged(SelectedStyle);
		}

		#endregion


		#region [Private] Properties: Pens and Brushes

		private Brush ItemBackgroundBrush {
			get {
				if (itemBackgroundBrush == null)
					itemBackgroundBrush = new SolidBrush(ItemBackgroundColor);
				return itemBackgroundBrush;
			}
		}


		private Brush ItemHighlightedBrush {
			get {
				if (itemHighlightedBrush == null)
					itemHighlightedBrush = new SolidBrush(itemHighlightedColor);
				return itemHighlightedBrush;
			}
		}


		private Brush ItemSelectedBrush {
			get {
				if (itemSelectedBrush == null)
					itemSelectedBrush = new SolidBrush(itemSelectedColor);
				return itemSelectedBrush;
			}
		}


		private Brush TextBrush {
			get {
				if (textBrush == null)
					textBrush = new SolidBrush(textColor);
				return textBrush;
			}
		}


		private Brush ItemFocusedBrush {
			get {
				if (itemFocusedBrush == null)
					itemFocusedBrush = new SolidBrush(ItemFocusedColor);
				return itemFocusedBrush;
			}
		}
		
		
		private Pen FocusBorderPen {
			get {
				if (focusBorderPen == null) {
					focusBorderPen = new Pen(focusBorderColor);
					focusBorderPen.Alignment = PenAlignment.Inset;
				}
				return focusBorderPen;
			}
		}


		private Pen ItemBorderPen {
			get {
				if (itemBorderPen == null) {
					itemBorderPen = new Pen(itemBorderColor);
					itemBorderPen.Alignment = PenAlignment.Inset;
				}
				return itemBorderPen;
			}
		}
		
		#endregion


		#region [Private] Methods: Draw FillStyle and CapStyle items

		private void DrawFillStyleItem(IFillStyle fillStyle, DrawItemEventArgs e) {
			Brush fillBrush = ToolCache.GetBrush(fillStyle);
			// Transform
			if (fillBrush is LinearGradientBrush) {
				float srcGradLen = ((LinearGradientBrush)fillBrush).Rectangle.Width;
				//float dstGradLen = previewRect.Width / (float)Math.Cos(Geometry.DegreesToRadians(fillStyle.GradientAngle));
				float dstGradLen = (float)Math.Sqrt((previewRect.Width * previewRect.Width) + (previewRect.Height * previewRect.Height));
				float scale = dstGradLen / srcGradLen;
				((LinearGradientBrush)fillBrush).ResetTransform();
				((LinearGradientBrush)fillBrush).TranslateTransform(previewRect.X, previewRect.Y);
				((LinearGradientBrush)fillBrush).ScaleTransform(scale, scale);
				((LinearGradientBrush)fillBrush).RotateTransform(fillStyle.GradientAngle);
			} else if (fillBrush is TextureBrush) {
				float scaleX = (float)previewRect.Width / ((TextureBrush)fillBrush).Image.Width;
				float scaleY = (float)previewRect.Height / ((TextureBrush)fillBrush).Image.Height;
				((TextureBrush)fillBrush).ResetTransform();
				((TextureBrush)fillBrush).TranslateTransform(previewRect.X, previewRect.Y);
				((TextureBrush)fillBrush).ScaleTransform(scaleX, scaleY);
			}
			// Draw
			if (fillBrush != Brushes.Transparent)
				e.Graphics.FillRectangle(fillBrush, previewRect);
			e.Graphics.DrawRectangle(ItemBorderPen, previewRect);
			e.Graphics.DrawRectangle(Pens.Black, previewRect);
			e.Graphics.DrawString(fillStyle.Title, e.Font, TextBrush, lableLayoutRect, formatter);
		}


		private void DrawCapStyleItem(ICapStyle capStyle, DrawItemEventArgs e) {
			Pen capPen = ToolCache.GetPen(design.LineStyles.Normal, capStyle, capStyle);
			Brush capBrush = null;
			PointF[] capPoints = null;

			int left = previewRect.Left;
			int right = previewRect.Right;
			if (capPen.StartCap == LineCap.Custom) {
				if (capPen.CustomStartCap.BaseInset > 0) {
					left += (int)Math.Round(capStyle.CapSize - capPen.CustomStartCap.BaseInset);
					right -= (int)Math.Round(capStyle.CapSize - capPen.CustomEndCap.BaseInset);
				}
			}
			int y = previewRect.Y + (previewRect.Height / 2);
			// Start Cap
			if (capStyle.CapShape != CapShape.None) {
				capBrush = ToolCache.GetBrush(capStyle.ColorStyle);
				ToolCache.GetCapPoints(capStyle, design.LineStyles.Normal, ref capPoints);
				float angle = Geometry.RadiansToDegrees(Geometry.Angle(left, y, right, y));
				matrix.Reset();
				matrix.Translate(left, y);
				matrix.Rotate(angle + 90);
				matrix.TransformPoints(capPoints);
				e.Graphics.FillPolygon(capBrush, capPoints, System.Drawing.Drawing2D.FillMode.Alternate);
			}
			// End Cap
			if (capStyle.CapShape != CapShape.None) {
				capBrush = ToolCache.GetBrush(capStyle.ColorStyle);
				ToolCache.GetCapPoints(capStyle, design.LineStyles.Normal, ref capPoints);
				float angle = Geometry.RadiansToDegrees(Geometry.Angle(right, y, left, y));
				matrix.Reset();
				matrix.Translate(right, y);
				matrix.Rotate(angle + 90);
				matrix.TransformPoints(capPoints);
				e.Graphics.FillPolygon(capBrush, capPoints, System.Drawing.Drawing2D.FillMode.Alternate);
			}
			// Draw
			e.Graphics.DrawLine(capPen, left, y, right, y);
			e.Graphics.DrawString(capStyle.Title, e.Font, TextBrush, lableLayoutRect, formatter);
		}

		#endregion


		#region Fields
		private StyleCategory styleCategory;
		private Design design;
		private IWindowsFormsEditorService editorService;
		private bool showItemDefaultStyle = false;

		// Graphical stuff
		private bool highlightItems = false;
		// Colors
		private Color itemBackgroundColor = Color.FromKnownColor(KnownColor.Window);
		private Color itemHighlightedColor = Color.FromKnownColor(KnownColor.HighlightText);
		private Color itemSelectedColor = Color.FromKnownColor(KnownColor.MenuHighlight);
		private Color textColor = Color.FromKnownColor(KnownColor.WindowText);
		private Color itemFocusedColor = Color.Transparent;
		private Color focusBorderColor = Color.Transparent;
		private Color itemBorderColor = Color.Transparent;
		// Pens and Brushes
		private Brush itemBackgroundBrush;
		private Brush itemHighlightedBrush;
		private Brush itemSelectedBrush;
		private Brush itemFocusedBrush;
		private Brush textBrush;
		private Pen itemBorderPen;
		private Pen focusBorderPen;
		// Buffers
		private Matrix matrix;
		private StringFormat formatter;
		private StringFormatFlags formatterFlags;
		private Rectangle itemBounds = Rectangle.Empty;
		private Rectangle previewRect = Rectangle.Empty;
		private Rectangle lableLayoutRect = Rectangle.Empty;

		const string previewText = "This is the first line of the sample text.\r\nThis is line 2 of the text.\r\nLine 3 of the text.";
		const int margin = 2;
		const int stdItemHeight = 20;
		const int dblItemHeight = 40;
		#endregion
	}
}
