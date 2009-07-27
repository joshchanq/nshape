using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Dataweb.Diagramming.WinFormsUI {
	public partial class TextInputDialog : Form {
		public TextInputDialog() {
			InitializeComponent();
		}


		public TextInputDialog(string caption, string prompt) {
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