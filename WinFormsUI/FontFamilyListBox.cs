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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;


namespace Dataweb.NShape.WinFormsUI {

	[ToolboxItem(false)]
	public partial class FontFamilyListBox : ListBox {
		
		public FontFamilyListBox(IWindowsFormsEditorService editorService) {
			InitializeComponent();
			Construct(editorService);
		}

		
		public FontFamilyListBox(IWindowsFormsEditorService editorService, IContainer container) {
			container.Add(this);
			InitializeComponent();
			Construct(editorService);
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


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				// dispose drawing stuff
				formatter.Dispose();
				int cnt = fonts.Count;
				for (int i = 0; i < cnt; ++i)
					fonts[i].Dispose();
				fonts.Clear();

				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

	
		//protected override void OnSelectedIndexChanged(EventArgs e) {
		//   base.OnSelectedIndexChanged(e);
		//   ExecuteSelection();
		//}


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


		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			if (e.Index >= 0) {
				e.ItemHeight = (int)Math.Ceiling(fonts[e.Index].GetHeight(e.Graphics));
				e.ItemWidth = Width;
			}
			else base.OnMeasureItem(e);
		}


		protected override void OnDrawItem(DrawItemEventArgs e) {
			itemBounds = e.Bounds;
			itemBounds.Inflate(-3, -1);

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

			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			if (e.Index >= 0) {
				Font font = fonts[e.Index];
				e.Graphics.DrawString(font.FontFamily.Name, font, Brushes.Black, itemBounds, formatter);
			}
			else base.OnDrawItem(e);
		}


		private void Construct(IWindowsFormsEditorService editorService) {
			if (editorService == null) throw new ArgumentNullException("editorService");
			this.editorService = editorService;

			this.IntegralHeight = false;
			this.DrawMode = DrawMode.OwnerDrawVariable;
			this.SelectionMode = SelectionMode.One;
			this.DoubleBuffered = true;

			formatter.Alignment = StringAlignment.Near;
			formatter.LineAlignment = StringAlignment.Near;
			int fontSize = 10;
			int cnt = FontFamily.Families.Length;
			for (int i = 0; i < cnt; ++i) {
				Font font = null;
				if (FontFamily.Families[i].IsStyleAvailable(FontStyle.Regular))
					font = new Font(FontFamily.Families[i].Name, fontSize, FontStyle.Regular);
				else if (FontFamily.Families[i].IsStyleAvailable(FontStyle.Italic))
					font = new Font(FontFamily.Families[i].Name, fontSize, FontStyle.Italic);
				else if (FontFamily.Families[i].IsStyleAvailable(FontStyle.Bold))
					font = new Font(FontFamily.Families[i].Name, fontSize, FontStyle.Bold);
				else if (FontFamily.Families[i].IsStyleAvailable(FontStyle.Strikeout))
					font = new Font(FontFamily.Families[i].Name, fontSize, FontStyle.Strikeout);
				else if (FontFamily.Families[i].IsStyleAvailable(FontStyle.Underline))
					font = new Font(FontFamily.Families[i].Name, fontSize, FontStyle.Underline);
				else
					font = Font;
				fonts.Add(font);
			}
		}


		private void ExecuteSelection() {
			if (editorService != null) editorService.CloseDropDown();
		}


		#region Pens and Brushes

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


		#region Fields
		private IWindowsFormsEditorService editorService;
		private bool highlightItems = true;
		
		// drawing stuff
		List<Font> fonts = new List<Font>(FontFamily.Families.Length);
		StringFormat formatter = new StringFormat();
		
		const int margin = 2;
		Rectangle itemBounds = Rectangle.Empty;

		private Color itemBackgroundColor = Color.FromKnownColor(KnownColor.Window);
		private Color itemHighlightedColor = Color.FromKnownColor(KnownColor.HighlightText);
		private Color itemSelectedColor = Color.FromKnownColor(KnownColor.MenuHighlight);
		private Color textColor = Color.FromKnownColor(KnownColor.WindowText);
		private Color itemFocusedColor = Color.Transparent;
		private Color focusBorderColor = Color.Transparent;
		private Color itemBorderColor = Color.Transparent;

		private Brush itemBackgroundBrush;
		private Brush itemHighlightedBrush;
		private Brush itemSelectedBrush;
		private Brush itemFocusedBrush;
		private Brush textBrush;
		private Pen itemBorderPen;
		private Pen focusBorderPen;
		#endregion
	}
}
