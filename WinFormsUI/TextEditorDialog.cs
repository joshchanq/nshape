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
using System.Drawing;
using System.Windows.Forms;
using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.WinFormsUI {

	public partial class TextEditorDialog : Form {

		public TextEditorDialog() {
			Construct();
		}


		public TextEditorDialog(string text) {
			Construct();
			textBox.Text = text;
			wantReturn = false;
		}


		public TextEditorDialog(IEnumerable<string> lines) {
			if (lines == null) throw new ArgumentNullException("lines");
			Construct();
			foreach (string line in lines)
				textBox.Text += line + "\r\n";
			wantReturn = true;
		}


		public TextEditorDialog(string text, ICharacterStyle characterStyle)
			: this(text) {
			if (characterStyle == null) throw new ArgumentNullException("characterStyle");
			Font font = ToolCache.GetFont(characterStyle);
			textBox.Font = (Font)font.Clone();
			font = null;
		}


		public TextEditorDialog(string text, string fontFamilyName, float fontSizeInPts, System.Drawing.FontStyle fontStyle)
			: this(text) {
			if (fontFamilyName == null) throw new ArgumentNullException("fontFamilyName");
			textBox.Font = new Font(fontFamilyName, fontSizeInPts, fontStyle, GraphicsUnit.Point);
		}


		public TextEditorDialog(IEnumerable<string> lines, CharacterStyle characterStyle)
			: this(lines) {
			if (characterStyle == null) throw new ArgumentNullException("characterStyle");
			Font font = ToolCache.GetFont(characterStyle);
			textBox.Font = (Font)font.Clone();
			font = null;
		}


		public TextEditorDialog(IEnumerable<string> lines, string fontFamilyName, float fontSizeInPts, System.Drawing.FontStyle fontStyle)
			: this(lines) {
			if (fontFamilyName == null) throw new ArgumentNullException("fontFamilyName");
			textBox.Font = new Font(fontFamilyName, fontSizeInPts, fontStyle, GraphicsUnit.Point);
		}


		public bool WordWrap {
			get { return textBox.WordWrap; }
			set { textBox.WordWrap = value; }
		}


		public HorizontalAlignment TextAlignment {
			get { return textBox.SelectionAlignment; }
			set {
				textBox.SuspendLayout();
				int s = textBox.SelectionStart;
				int l = textBox.SelectionLength;
				textBox.SelectAll();
				textBox.SelectionAlignment = value;
				if (l == 0)
					textBox.Select(textBox.TextLength, l);
				else
					textBox.Select(s, l);
				textBox.ResumeLayout();
			}
		}


		public string ResultText {
			get { return textBox.Text; }
		}


		public string[] ResultLines {
			get { return textBox.Lines; }
		}


		private void Construct() {
			SetStyle(ControlStyles.ContainerControl
						| ControlStyles.OptimizedDoubleBuffer
						| ControlStyles.ResizeRedraw
						| ControlStyles.SupportsTransparentBackColor
						, true);
			UpdateStyles();
			InitializeComponent();
			textBox.PreviewKeyDown += textBox_PreviewKeyDown;
		}


		private void okButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
		}


		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
		}


		private void TextEditor_FormClosed(object sender, FormClosedEventArgs e) {
			textBox.PreviewKeyDown -= textBox_PreviewKeyDown;
		}


		private void textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Return:
					if (wantReturn) {
						if ((e.Modifiers & Keys.Control) != 0)
							okButton_Click(this, null);
					}
					else {
						if (!((e.Modifiers & Keys.Control) != 0))
							okButton_Click(this, null);
					}
					break;

				case Keys.Escape:
					cancelButton_Click(this, null);
					break;

				default:
					// do nothing
					break;
			}
		}


		public bool WantTab {
			get { return !textBox.TabStop; }
			set { textBox.TabStop = !value; }
		}


		public bool WantReturn {
			get { return wantReturn; }
			set { wantReturn = value; }
		}


		#region Fields
		private bool wantReturn = false;
		#endregion
	}


	[ToolboxItem(false)]
	internal class TextEditorTextBox : RichTextBox {
		
		public TextEditorTextBox() {
			SetControlStyles();
		}


		public TextEditorTextBox(string text, Font font) {
			if (font == null) throw new ArgumentNullException("font");
			SetControlStyles();
			Font = font;
			Text = text;
		}


		public TextEditorTextBox(IEnumerable<string> lines, Font font) {
			if (lines == null) throw new ArgumentNullException("lines");
			if (font == null) throw new ArgumentNullException("font");
			SetControlStyles();
			Font = font;
			List<string> textLines = new List<string>(lines);
			textLines.CopyTo(Lines);
		}


		public HorizontalAlignment TextAlignment {
			get { return SelectionAlignment; }
			set {
				SuspendLayout();
				int s = SelectionStart;
				int l = SelectionLength;
				SelectAll();
				SelectionAlignment = value;
				if (l == 0)
					Select(TextLength, l);
				else
					Select(s, l);
				ResumeLayout();
			}
		}


		private void SetControlStyles() {
			SetStyle(
				//ControlStyles.AllPaintingInWmPaint
				//| ControlStyles.UserPaint
				//| ControlStyles.FixedHeight
				//| ControlStyles.FixedWidth
				//| 
				ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.ResizeRedraw
				| ControlStyles.Selectable
				| ControlStyles.SupportsTransparentBackColor
			, true);
			UpdateStyles();
		}
	}
}