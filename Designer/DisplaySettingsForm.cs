using System;
using System.Windows.Forms;

using Dataweb.Diagramming.Controllers;


namespace Dataweb.Diagramming.Designer {

	public partial class DisplaySettingsForm : Form {
		public DisplaySettingsForm() {
			InitializeComponent();

			resizePointCombo.Items.Clear();
			connectionPointCombo.Items.Clear();
			foreach (ControlPointShape ptShape in Enum.GetValues(typeof(ControlPointShape))) {
				resizePointCombo.Items.Add(ptShape);
				connectionPointCombo.Items.Add(ptShape);
			}
		}


		public bool ShowGrid {
			get { return showGridCheckBox.Checked; }
			set { showGridCheckBox.Checked = value; }
		}


		public bool SnapToGrid {
			get { return snapToGridCheckBox.Checked; }
			set { snapToGridCheckBox.Checked = value; }
		}


		public int GridSize {
			get { return Convert.ToInt32(gridSizeUpDown.Value); }
			set { gridSizeUpDown.Value = value; }
		}


		public int SnapDistance {
			get { return Convert.ToInt32(snapDistanceUpDown.Value); }
			set { snapDistanceUpDown.Value = value; }
		}


		public ControlPointShape ResizePointShape {
			get { return (ControlPointShape)resizePointCombo.SelectedItem; }
			set { resizePointCombo.SelectedItem = value; }
		}


		public ControlPointShape ConnectionPointShape {
			get { return (ControlPointShape)connectionPointCombo.SelectedItem; }
			set { connectionPointCombo.SelectedItem = value; }
		}


		public int ControlPointSize {
			get { return Convert.ToInt32(pointSizeUpDown.Value); }
			set { pointSizeUpDown.Value = value; }
		}


		private void cancelButton_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.Cancel;
		}
		

		private void okButton_Click(object sender, EventArgs e) {
			this.DialogResult = DialogResult.OK;
		}
	}
}