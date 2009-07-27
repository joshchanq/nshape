using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;


namespace Dataweb.Diagramming.WinFormsUI {

	[ToolboxItem(false)]
	public partial class VerticalTabControl : ListBox {
		public VerticalTabControl() {
			InitializeComponent();
			SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.OptimizedDoubleBuffer |
						ControlStyles.ResizeRedraw,
						true);
			UpdateStyles();

			this.formatterFlags = 0 | StringFormatFlags.NoWrap;
			this.formatter = new StringFormat(formatterFlags);
			this.formatter.Trimming = StringTrimming.EllipsisCharacter;
			this.formatter.Alignment = StringAlignment.Center;
			this.formatter.LineAlignment = StringAlignment.Center;
		}


		#region Colors

		public Color InactiveItemBackgroundColor {
			get { return BackColor; }
			set {
				if (backgroundBrush != null) {
					backgroundBrush.Dispose();
					backgroundBrush = null;
				}
				BackColor = value;
			}
		}


		public Color HighlightedItemColor {
			get { return highlightedItemColor; }
			set {
				if (highlightedItemBrush != null) {
					highlightedItemBrush.Dispose();
					highlightedItemBrush = null;
				}
				highlightedItemColor = value;
			}
		}


		public Color SelectedItemColor {
			get { return selectedItemColor; }
			set {
				if (selectedItemBrush != null) {
					selectedItemBrush.Dispose();
					selectedItemBrush = null;
				}
				selectedItemColor = value;
			}
		}


		public Color InactiveItemBorderColor {
			get { return itemBorderColor; }
			set {
				if (itemBorderPen != null) {
					itemBorderPen.Dispose();
					itemBorderPen = null;
				}
				itemBorderColor = value;
			}
		}


		public Color FocusedItemColor {
			get { return focusBackgroundColor; }
			set {
				if (focusBackgroundBrush != null) {
					focusBackgroundBrush.Dispose();
					focusBackgroundBrush = null;
				}
				focusBackgroundColor = value;
			}
		}


		public Color FocusBorderColor {
			get { return focusBorderColor; }
			set {
				if (selectedBorderPen != null) {
					selectedBorderPen.Dispose();
					selectedBorderPen = null;
				}
				focusBorderColor = value;
			}
		}


		public Color SelectedItemTextColor {
			get { return selectedTextColor; }
			set {
				if (selectedTextBrush != null) {
					selectedTextBrush.Dispose();
					selectedTextBrush = null;
				}
				selectedTextColor = value;
			}
		}


		public Color InactiveItemTextColor {
			get { return itemTextColor; }
			set {
				if (itemTextBrush != null) {
					itemTextBrush.Dispose();
					itemTextBrush = null;
				}
				itemTextColor = value;
			}
		}

		#endregion


		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			base.OnMeasureItem(e);
			e.ItemWidth = Width;
			e.ItemHeight = 50;
		}


		protected override void OnDrawItem(DrawItemEventArgs e) {
			base.OnDrawItem(e);

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			Rectangle itemBounds = Rectangle.Empty;
			itemBounds.X = e.Bounds.X + 3;
			itemBounds.Y = e.Bounds.Y + 1;
			itemBounds.Width = e.Bounds.Width - itemBounds.X;
			itemBounds.Height = e.Bounds.Height - 2;

			e.Graphics.FillRectangle(BackgroundBrush, e.Bounds);
			if ((e.State & DrawItemState.Selected) != 0) {
				e.Graphics.FillRectangle(SelectedItemBrush, itemBounds.Left, itemBounds.Top, e.Bounds.Width, itemBounds.Height);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, e.Bounds.Top, itemBounds.Right - 1, itemBounds.Top);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, itemBounds.Bottom, itemBounds.Right - 1, e.Bounds.Bottom);

				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Left, itemBounds.Top, itemBounds.Left, itemBounds.Bottom);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Left, itemBounds.Top, itemBounds.Right, itemBounds.Top);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Left, itemBounds.Bottom, itemBounds.Right, itemBounds.Bottom);
				e.Graphics.DrawString(Items[e.Index].ToString(), Font, SelectedTextBrush, itemBounds, formatter);

				e.Graphics.FillRectangle(BackgroundBrush, 0, Items.Count * e.Bounds.Height, itemBounds.Right, Height);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, Items.Count * e.Bounds.Height, itemBounds.Right - 1, Height);
			}
			else {
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, e.Bounds.Top, itemBounds.Right - 1, e.Bounds.Bottom);
				if (e.Index >= 0 && Items.Count > 0)
					e.Graphics.DrawString(Items[e.Index].ToString(), Font, ItemTextBrush, itemBounds, formatter);
			}
		}


		protected override void OnPaintBackground(PaintEventArgs pevent) {
			base.OnPaintBackground(pevent);
		}


		protected override void OnPaint(PaintEventArgs e) {
			//e.Graphics.FillRectangle(BackgroundBrush, Bounds);
			base.OnPaint(e);
		}


		#region Pens and Brushes

		private Brush BackgroundBrush {
			get {
				if (backgroundBrush == null)
					backgroundBrush = new SolidBrush(InactiveItemBackgroundColor);
				return backgroundBrush;
			}
		}


		private Brush HighlightedItemBrush {
			get {
				if (highlightedItemBrush == null)
					highlightedItemBrush = new SolidBrush(highlightedItemColor);
				return highlightedItemBrush;
			}
		}


		private Brush SelectedItemBrush {
			get {
				if (selectedItemBrush == null)
					selectedItemBrush = new SolidBrush(selectedItemColor);
				return selectedItemBrush;
			}
		}


		private Brush ItemTextBrush {
			get {
				if (itemTextBrush == null)
					itemTextBrush = new SolidBrush(itemTextColor);
				return itemTextBrush;
			}
		}


		private Brush SelectedTextBrush {
			get {
				if (selectedTextBrush == null)
					selectedTextBrush = new SolidBrush(selectedTextColor);
				return selectedTextBrush;
			}
		}


		private Brush FocusBackgroundBrush {
			get {
				if (focusBackgroundBrush == null)
					focusBackgroundBrush = new SolidBrush(focusBackgroundColor);
				return focusBackgroundBrush;
			}
		}


		private Pen ItemBorderPen {
			get {
				if (itemBorderPen == null)
					itemBorderPen = new Pen(itemBorderColor);
				return itemBorderPen;
			}
		}


		private Pen FocusBorderPen {
			get {
				if (selectedBorderPen == null)
					selectedBorderPen = new Pen(selectedItemBorderColor);
				return selectedBorderPen;
			}
		}

		#endregion


		#region Fields
		private StringFormat formatter;
		private StringFormatFlags formatterFlags;
		// Colors
		private Color backgroundColor = Color.FromKnownColor(KnownColor.Control);
		private Color highlightedItemColor = Color.FromKnownColor(KnownColor.ControlLightLight);
		private Color selectedItemColor = Color.FromKnownColor(KnownColor.Window);
		private Color selectedItemBorderColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
		private Color itemBorderColor = Color.FromKnownColor(KnownColor.Window);
		private Color focusBackgroundColor = Color.Beige;
		private Color focusBorderColor = Color.FromArgb(128, Color.Beige);
		private Color itemTextColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
		private Color selectedTextColor = Color.FromKnownColor(KnownColor.ControlText);
		// Pens and Brushes
		private Brush backgroundBrush;
		private Brush highlightedItemBrush;
		private Brush selectedItemBrush;
		private Brush itemTextBrush;
		private Brush selectedTextBrush;
		private Brush focusBackgroundBrush;
		private Pen itemBorderPen;
		private Pen selectedBorderPen;
		#endregion
	}
}
