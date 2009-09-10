using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dataweb.nShape.Advanced;

namespace Dataweb.nShape.Designer {

	public partial class DiagramSettingsForm : Form {

		public DiagramSettingsForm(Diagram diagram) {
			InitializeComponent();
			propertyGrid.SelectedObject = diagram;
		}


		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
		}


		private void okButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
		}
	}
}