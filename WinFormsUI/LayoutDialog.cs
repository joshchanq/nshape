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
using System.Diagnostics;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Layouters;


namespace Dataweb.NShape.WinFormsUI {

	public partial class LayoutDialog : Form {

		public LayoutDialog() {
			InitializeComponent();
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(this.GetType().Assembly.Location);
		}


		public LayoutDialog(ILayouter layouter)
			: this() {
			if (layouter == null) throw new ArgumentNullException("layouter");
			
			int panelIdx = -1;
			if (layouter is ExpansionLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Expansion");
				horizontalCompressionTrackBar.Value = ((ExpansionLayouter)layouter).HorizontalCompression;
				verticalCompressionTrackBar.Value = ((ExpansionLayouter)layouter).VerticalCompression;
			} else if (layouter is GridLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Alignment");
				columnDistanceTrackBar.Value = ((GridLayouter)layouter).CoarsenessX;
				rowDistanceTrackBar.Value = ((GridLayouter)layouter).CoarsenessY;
			} else if (layouter is RepulsionLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Clusters");
				repulsionStrengthTrackBar.Value = ((RepulsionLayouter)layouter).Repulsion;
				repulsionRangeTrackBar.Value=((RepulsionLayouter)layouter).RepulsionRange;
				attractionStrengthTrackBar.Value = ((RepulsionLayouter)layouter).SpringRate;
			} else if (layouter is FlowLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Flow");
				bottomUpRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.BottomUp;
				leftToRightRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.LeftToRight;
				topDownRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.TopDown;
				rightToLeftRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.RightToLeft;
			}

			this.layouter = layouter;
			algorithmListBox.SelectedIndex = panelIdx;
		}


		public event EventHandler LayoutChanged;


		public Project Project {
			get { return project; }
			set { project = value; }
		}


		public Diagram Diagram {
			get { return diagram; }
			set { diagram = value; }
		}


		public IEnumerable<Shape> SelectedShapes {
			get { return selectedShapes; }
			set {
				selectedShapes.Clear();
				selectedShapes.AddRange(value);
			}
		}


		private void PrepareLayouter() {
			switch ((string)currentPanel.Tag) {
				case "Expansion": // Distribution
					if (layouter == null || !(layouter is ExpansionLayouter))
						layouter = new ExpansionLayouter(Project);
					ExpansionLayouter dl = (ExpansionLayouter)layouter;
					dl.HorizontalCompression = horizontalCompressionTrackBar.Value;
					dl.VerticalCompression = verticalCompressionTrackBar.Value;
					break;
				case "Alignment":
					if (layouter == null || !(layouter is GridLayouter))
						layouter = new GridLayouter(Project);
					GridLayouter gl = (GridLayouter)layouter;
					gl.CoarsenessX = columnDistanceTrackBar.Value;
					gl.CoarsenessY = rowDistanceTrackBar.Value;
					/*
					 * gl.ColumnDistance = columnDistanceTrackBar.Value;
					gl.RowDistance = rowDistanceTrackBar.Value; */
					break;
				case "Clusters":
					if (layouter == null || !(layouter is RepulsionLayouter))
						layouter = new RepulsionLayouter(Project);
					RepulsionLayouter rl = (RepulsionLayouter)layouter;
					// The default distance between connected elements should be 100 display units.
					// The default distance between unconnected elements should be 300 display units.
					rl.Friction = 0; // 300;
					rl.Repulsion = repulsionStrengthTrackBar.Value;
					rl.RepulsionRange = repulsionRangeTrackBar.Value;
					rl.SpringRate = attractionStrengthTrackBar.Value;
					// Two unconnected elements at the same position should move to their default distance 
					// within two steps
					rl.Mass = 50;
					break;
				case "Flow":
					if (layouter == null || !(layouter is FlowLayouter))
						layouter = new FlowLayouter(Project);
					FlowLayouter fl = (FlowLayouter)layouter;
					if (bottomUpRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.BottomUp;
					else if (leftToRightRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.LeftToRight;
					else if (topDownRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.TopDown;
					else if (rightToLeftRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.RightToLeft;
					break;
				default:
					Debug.Assert(false);
					break;
			}
		}


		private void SetShapes() {
			if (diagram != null) {
				layouter.AllShapes = diagram.Shapes;
				if (selectedShapes.Count == 0) layouter.Shapes = diagram.Shapes;
				else layouter.Shapes = selectedShapes;
			} 
		}


		private void Restore() {
			layouter.RestoreState();
			OnLayoutChanged();
			SwitchToOriginal();
		}


		private void PreviewFast() {
			SetShapes();
			layouter.SaveState();
			layouter.Prepare();
			layouter.Execute(10);
			layouter.Fit(0, 0, diagram.Size.Width, diagram.Size.Height);
			OnLayoutChanged();
			SwitchToPreview();
		}


		private void PreviewIfRequired() {
			if (immediateRadioButton.Checked) {
				if (isPreview) Restore();
				PrepareLayouter();
				PreviewFast();
			}
		}


		private void DisplayCurrentPanel() {
			Panel newPanel = null;
			foreach (Control c in Controls)
				if (c is Panel && (string)c.Tag == (string)algorithmListBox.SelectedItem) {
					newPanel = (Panel)c;
					break;
				}
			if (newPanel == null) {
				MessageBox.Show("This algorithm is currently not supported.");
			} else {
				newPanel.Show();
				// Hide all others
				foreach (Control c in Controls)
					if (c is Panel && c != newPanel) c.Hide();
				currentPanel = newPanel;
			}
		}


		private void OnLayoutChanged() {
			if (LayoutChanged != null) LayoutChanged(this, eventArgs);
		}


		private void FinishAnimatedPreview() {
			layoutTimer.Stop();
			SwitchToPreview();
		}


		private void SwitchToPreview() {
			isPreview = true;
			previewButton.Text = "Original";
		}


		private void SwitchToOriginal() {
			isPreview = false;
			previewButton.Text = "Preview";
		}


		#region Form Event Handlers

		private void LayoutControlForm_Load(object sender, EventArgs e) {
			if (algorithmListBox.SelectedIndex < 0) 
				algorithmListBox.SelectedIndex = 0;
		}


		private void LayoutControlForm_Shown(object sender, EventArgs e) {
			DisplayCurrentPanel();
		}


		private void applyButton_Click(object sender, EventArgs e) {
			SwitchToOriginal();
		}


		private void centerButton_Click(object sender, EventArgs e) {
			PrepareLayouter();
			layouter.Prepare();
			layouter.Fit(0, 0, diagram.Size.Width, diagram.Size.Height);
		}


		private void algorithmListBox_SelectedIndexChanged(object sender, EventArgs e) {
			DisplayCurrentPanel();
		}


		private void layoutTimer_Tick(object sender, EventArgs e) {
			bool stop = !layouter.ExecuteStep();
			OnLayoutChanged();
			if (stop) FinishAnimatedPreview();
		}


		private void columnDistanceTrackBar_ValueChanged(object sender, EventArgs e) {
			columnDistanceLabel.Text = columnDistanceTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void rowDistanceTrackBar_ValueChanged(object sender, EventArgs e) {
			rowDistanceLabel.Text = rowDistanceTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void attractionStrengthTrackBar_ValueChanged(object sender, EventArgs e) {
			attractionStrengthLabel.Text = attractionStrengthTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void repulsionStrengthTrackBar_ValueChanged(object sender, EventArgs e) {
			repulsionStrengthLabel.Text = repulsionStrengthTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void repulsionRangeTrackBar_ValueChanged(object sender, EventArgs e) {
			repulsionRangeLabel.Text = repulsionRangeTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void horizontalCompressionTrackBar_ValueChanged(object sender, EventArgs e) {
			horizontalCompressionLabel.Text = horizontalCompressionTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void verticalCompressionTrackBar_ValueChanged(object sender, EventArgs e) {
			verticalCompressionLabel.Text = verticalCompressionTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void keepAspectRationCheckBox_CheckedChanged(object sender, EventArgs e) {
			if (keepAspectRationCheckBox.Checked) {
				verticalCompressionTrackBar.Value = horizontalCompressionTrackBar.Value;
				PreviewIfRequired();
			}
		}


		private void previewButton_Click(object sender, EventArgs e) {
			if (isPreview)
				Restore();
			else if (layoutTimer.Enabled)
				FinishAnimatedPreview();
			else {
				PrepareLayouter();
				if (animatedRadioButton.Checked) {
					SetShapes();
					layouter.SaveState();
					layouter.Prepare();
					previewButton.Text = "Running";
#if DEBUG
					layoutTimer.Interval = 1000;
#else
					layoutTimer.Interval = 50;
#endif
					layoutTimer.Start();
				} else PreviewFast();
			}
		}


		private void closeButton_Click(object sender, EventArgs e) {
			if (Modal) DialogResult = DialogResult.OK;
			else Close();
		}

		#endregion


		#region Fields

		private Panel currentPanel;
		private bool isPreview;

		private Project project;
		private Diagram diagram;
		private List<Shape> selectedShapes = new List<Shape>(100);
		private ILayouter layouter;

		private EventArgs eventArgs = new EventArgs();

		#endregion
	}
}