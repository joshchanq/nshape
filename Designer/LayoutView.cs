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
using System.Diagnostics;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Layouters;


namespace Dataweb.NShape.Designer {

	public partial class LayoutView : Form {

		public LayoutView() {
			InitializeComponent();
		}


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


		public event EventHandler LayoutChanged;


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
					// Der natürliche Abstand zweier verbundener Elemente soll 100 sein.
					// Der natürliche Abstand zweier unverbundener Element soll 300 sein.
					rl.Friction = 0; // 300;
					rl.Repulsion = repulsionStrengthTrackBar.Value;
					rl.RepulsionRange = repulsionRangeTrackBar.Value;
					rl.SpringRate = attractionStrengthTrackBar.Value;
					// Zwei aufeinanderliegende unverbundene Elemente sollen in zwei Schritten
					// den korrekten Abstand einnehmen.
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


		#region Form Event Handlers

		private void LayoutControlForm_Load(object sender, EventArgs e) {
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



		private void columnDistanceTrackBar_Scroll(object sender, EventArgs e) {
			columnDistanceLabel.Text = columnDistanceTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void rowDistanceTrackBar_Scroll(object sender, EventArgs e) {
			rowDistanceLabel.Text = rowDistanceTrackBar.Value.ToString();
			PreviewIfRequired();
		}


		private void attractionStrengthTrackBar_Scroll(object sender, EventArgs e) {
			attractionStrengthLabel.Text = attractionStrengthTrackBar.Value.ToString();
		}


		private void repulsionStrengthTrackBar_Scroll(object sender, EventArgs e) {
			repulsionStrengthLabel.Text = repulsionStrengthTrackBar.Value.ToString();
		}


		private void repulsionRangeTrackBar_Scroll(object sender, EventArgs e) {
			repulsionRangeLabel.Text = repulsionRangeTrackBar.Value.ToString();
		}


		private void horizontalCompressionTrackBar_Scroll(object sender, EventArgs e) {
			horizontalCompressionLabel.Text = horizontalCompressionTrackBar.Value.ToString();
			if (keepAspectRationCheckBox.Checked) {
				verticalCompressionTrackBar.Value = horizontalCompressionTrackBar.Value;
				verticalCompressionLabel.Text = verticalCompressionTrackBar.Value.ToString();
			}
			PreviewIfRequired();
		}


		private void verticalCompressionTrackBar_Scroll(object sender, EventArgs e) {
			verticalCompressionLabel.Text = verticalCompressionTrackBar.Value.ToString();
			if (keepAspectRationCheckBox.Checked) {
				horizontalCompressionTrackBar.Value = verticalCompressionTrackBar.Value;
				horizontalCompressionLabel.Text = horizontalCompressionTrackBar.Value.ToString();
			}
			PreviewIfRequired();
		}


		private void keepAspectRationCheckBox_CheckedChanged(object sender, EventArgs e) {
			if (keepAspectRationCheckBox.Checked)
				verticalCompressionTrackBar.Value = horizontalCompressionTrackBar.Value;
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
					layoutTimer.Interval = 1000;
					layoutTimer.Start();
				} else PreviewFast();
			}
		}

		#endregion


		private void SetShapes() {
			layouter.AllShapes = diagram.Shapes;
			if (selectedShapes.Count == 0) layouter.Shapes = diagram.Shapes;
			else layouter.Shapes = selectedShapes;
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
				if (c is Panel && (string)c.Tag == (string)algorithmListBox.SelectedItem)
					newPanel = (Panel)c;
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


		private void closeButton_Click(object sender, EventArgs e) {
			Close();
		}


		private Panel currentPanel;
		private bool isPreview;

		private Project project;
		private Diagram diagram;
		private List<Shape> selectedShapes = new List<Shape>(100);
		private ILayouter layouter;

		private EventArgs eventArgs = new EventArgs();

	}
}