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
using System.Windows.Forms;

namespace Dataweb.NShape.WinFormsUI {

	public partial class StringInputDialog : Form {

		public StringInputDialog() {
			InitializeComponent();
		}


		public StringInputDialog(string caption, string prompt) {
			InitializeComponent();
			Caption = caption;
			Prompt = prompt;
		}


		public string Prompt {
			get { return promptLabel.Text; }
			set { promptLabel.Text = value; }
		}


		public string Caption {
			get { return this.Text; }
			set { this.Text = value; }
		}


		public HorizontalAlignment TextAlignment {
			get { return resultTextBox.TextAlign; }
			set { resultTextBox.TextAlign = value; }
		}


		public String ResultString {
			get { return resultTextBox.Text; }
			set { resultTextBox.Text = value; }
		}

		
		private void okButton_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.OK;
		}

		
		private void nextButton_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.Cancel;
		}


		private void resultTextBox_KeyUp(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return)
				this.DialogResult = DialogResult.OK;
		}
	}
}