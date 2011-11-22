namespace Dataweb.NShape.WinFormsUI {

	partial class LayoutDialog {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.Panel expansionPanel;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayoutDialog));
			this.keepAspectRationCheckBox = new System.Windows.Forms.CheckBox();
			this.expandDescriptionLabel = new System.Windows.Forms.Label();
			this.verticalCompressionLabel = new System.Windows.Forms.Label();
			this.horizontalCompressionLabel = new System.Windows.Forms.Label();
			this.verticalCompressionTrackBar = new System.Windows.Forms.TrackBar();
			this.horizontalCompressionTrackBar = new System.Windows.Forms.TrackBar();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.fastRadioButton = new System.Windows.Forms.RadioButton();
			this.animatedRadioButton = new System.Windows.Forms.RadioButton();
			this.previewGroupBox = new System.Windows.Forms.GroupBox();
			this.immediateRadioButton = new System.Windows.Forms.RadioButton();
			this.applyButton = new System.Windows.Forms.Button();
			this.closeButton = new System.Windows.Forms.Button();
			this.alignmentPanel = new System.Windows.Forms.Panel();
			this.gridDescriptionLabel = new System.Windows.Forms.Label();
			this.rowDistanceLabel = new System.Windows.Forms.Label();
			this.columnDistanceLabel = new System.Windows.Forms.Label();
			this.rowDistanceTrackBar = new System.Windows.Forms.TrackBar();
			this.columnDistanceTrackBar = new System.Windows.Forms.TrackBar();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.centerButton = new System.Windows.Forms.Button();
			this.layoutTimer = new System.Windows.Forms.Timer(this.components);
			this.repulsionPanel = new System.Windows.Forms.Panel();
			this.repulsionRangeTrackBar = new System.Windows.Forms.TrackBar();
			this.repulsionRangeLabel = new System.Windows.Forms.Label();
			this.repulsionLabel4 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.attractionStrengthLabel = new System.Windows.Forms.Label();
			this.repulsionStrengthLabel = new System.Windows.Forms.Label();
			this.repulsionStrengthTrackBar = new System.Windows.Forms.TrackBar();
			this.attractionStrengthTrackBar = new System.Windows.Forms.TrackBar();
			this.repulsionLabel3 = new System.Windows.Forms.Label();
			this.repulsionLabel2 = new System.Windows.Forms.Label();
			this.flowPanel = new System.Windows.Forms.Panel();
			this.flowDirectionGroupBox = new System.Windows.Forms.GroupBox();
			this.leftToRightRadioButton = new System.Windows.Forms.RadioButton();
			this.topDownRadioButton = new System.Windows.Forms.RadioButton();
			this.rightToLeftRadioButton = new System.Windows.Forms.RadioButton();
			this.bottomUpRadioButton = new System.Windows.Forms.RadioButton();
			this.flowDescriptionLabel = new System.Windows.Forms.Label();
			this.previewButton = new System.Windows.Forms.Button();
			this.algorithmListBox = new Dataweb.NShape.WinFormsUI.VerticalTabControl();
			expansionPanel = new System.Windows.Forms.Panel();
			expansionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.verticalCompressionTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.horizontalCompressionTrackBar)).BeginInit();
			this.previewGroupBox.SuspendLayout();
			this.alignmentPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rowDistanceTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.columnDistanceTrackBar)).BeginInit();
			this.repulsionPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.repulsionRangeTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.repulsionStrengthTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.attractionStrengthTrackBar)).BeginInit();
			this.flowPanel.SuspendLayout();
			this.flowDirectionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// expansionPanel
			// 
			expansionPanel.Controls.Add(this.keepAspectRationCheckBox);
			expansionPanel.Controls.Add(this.expandDescriptionLabel);
			expansionPanel.Controls.Add(this.verticalCompressionLabel);
			expansionPanel.Controls.Add(this.horizontalCompressionLabel);
			expansionPanel.Controls.Add(this.verticalCompressionTrackBar);
			expansionPanel.Controls.Add(this.horizontalCompressionTrackBar);
			expansionPanel.Controls.Add(this.label7);
			expansionPanel.Controls.Add(this.label8);
			expansionPanel.Location = new System.Drawing.Point(100, 4);
			expansionPanel.Name = "expansionPanel";
			expansionPanel.Size = new System.Drawing.Size(367, 277);
			expansionPanel.TabIndex = 10;
			expansionPanel.Tag = "Expansion";
			// 
			// keepAspectRationCheckBox
			// 
			this.keepAspectRationCheckBox.AutoSize = true;
			this.keepAspectRationCheckBox.Checked = true;
			this.keepAspectRationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.keepAspectRationCheckBox.Location = new System.Drawing.Point(4, 221);
			this.keepAspectRationCheckBox.Name = "keepAspectRationCheckBox";
			this.keepAspectRationCheckBox.Size = new System.Drawing.Size(109, 17);
			this.keepAspectRationCheckBox.TabIndex = 9;
			this.keepAspectRationCheckBox.Text = "Keep aspect ratio";
			this.keepAspectRationCheckBox.UseVisualStyleBackColor = true;
			this.keepAspectRationCheckBox.CheckedChanged += new System.EventHandler(this.keepAspectRationCheckBox_CheckedChanged);
			// 
			// expandDescriptionLabel
			// 
			this.expandDescriptionLabel.Location = new System.Drawing.Point(4, 3);
			this.expandDescriptionLabel.Name = "expandDescriptionLabel";
			this.expandDescriptionLabel.Size = new System.Drawing.Size(359, 69);
			this.expandDescriptionLabel.TabIndex = 8;
			this.expandDescriptionLabel.Text = "Track the sliders to compress or expand the selected shapes without modifying the" +
    "ir relative positions.\r\nUse this layouter to assign a larger or smaller area of " +
    "the diagram to the selected shapes.";
			// 
			// verticalCompressionLabel
			// 
			this.verticalCompressionLabel.AutoSize = true;
			this.verticalCompressionLabel.Location = new System.Drawing.Point(135, 162);
			this.verticalCompressionLabel.Name = "verticalCompressionLabel";
			this.verticalCompressionLabel.Size = new System.Drawing.Size(25, 13);
			this.verticalCompressionLabel.TabIndex = 7;
			this.verticalCompressionLabel.Text = "120";
			// 
			// horizontalCompressionLabel
			// 
			this.horizontalCompressionLabel.AutoSize = true;
			this.horizontalCompressionLabel.Location = new System.Drawing.Point(150, 98);
			this.horizontalCompressionLabel.Name = "horizontalCompressionLabel";
			this.horizontalCompressionLabel.Size = new System.Drawing.Size(25, 13);
			this.horizontalCompressionLabel.TabIndex = 6;
			this.horizontalCompressionLabel.Text = "120";
			// 
			// verticalCompressionTrackBar
			// 
			this.verticalCompressionTrackBar.LargeChange = 20;
			this.verticalCompressionTrackBar.Location = new System.Drawing.Point(3, 178);
			this.verticalCompressionTrackBar.Maximum = 500;
			this.verticalCompressionTrackBar.Minimum = 10;
			this.verticalCompressionTrackBar.Name = "verticalCompressionTrackBar";
			this.verticalCompressionTrackBar.Size = new System.Drawing.Size(344, 45);
			this.verticalCompressionTrackBar.SmallChange = 5;
			this.verticalCompressionTrackBar.TabIndex = 5;
			this.verticalCompressionTrackBar.TickFrequency = 25;
			this.verticalCompressionTrackBar.Value = 10;
			this.verticalCompressionTrackBar.ValueChanged += new System.EventHandler(this.verticalCompressionTrackBar_ValueChanged);
			// 
			// horizontalCompressionTrackBar
			// 
			this.horizontalCompressionTrackBar.AutoSize = false;
			this.horizontalCompressionTrackBar.LargeChange = 20;
			this.horizontalCompressionTrackBar.Location = new System.Drawing.Point(3, 114);
			this.horizontalCompressionTrackBar.Maximum = 500;
			this.horizontalCompressionTrackBar.Minimum = 10;
			this.horizontalCompressionTrackBar.Name = "horizontalCompressionTrackBar";
			this.horizontalCompressionTrackBar.Size = new System.Drawing.Size(344, 45);
			this.horizontalCompressionTrackBar.SmallChange = 5;
			this.horizontalCompressionTrackBar.TabIndex = 4;
			this.horizontalCompressionTrackBar.TickFrequency = 25;
			this.horizontalCompressionTrackBar.Value = 10;
			this.horizontalCompressionTrackBar.ValueChanged += new System.EventHandler(this.horizontalCompressionTrackBar_ValueChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(0, 162);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(129, 13);
			this.label7.TabIndex = 2;
			this.label7.Text = "Vertical compression in %:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(0, 98);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(141, 13);
			this.label8.TabIndex = 1;
			this.label8.Text = "Horizontal compression in %:";
			// 
			// fastRadioButton
			// 
			this.fastRadioButton.AutoSize = true;
			this.fastRadioButton.Location = new System.Drawing.Point(85, 19);
			this.fastRadioButton.Name = "fastRadioButton";
			this.fastRadioButton.Size = new System.Drawing.Size(45, 17);
			this.fastRadioButton.TabIndex = 1;
			this.fastRadioButton.Text = "Fast";
			this.fastRadioButton.UseVisualStyleBackColor = true;
			// 
			// animatedRadioButton
			// 
			this.animatedRadioButton.AutoSize = true;
			this.animatedRadioButton.Checked = true;
			this.animatedRadioButton.Location = new System.Drawing.Point(136, 19);
			this.animatedRadioButton.Name = "animatedRadioButton";
			this.animatedRadioButton.Size = new System.Drawing.Size(69, 17);
			this.animatedRadioButton.TabIndex = 2;
			this.animatedRadioButton.TabStop = true;
			this.animatedRadioButton.Text = "Animated";
			this.animatedRadioButton.UseVisualStyleBackColor = true;
			// 
			// previewGroupBox
			// 
			this.previewGroupBox.Controls.Add(this.immediateRadioButton);
			this.previewGroupBox.Controls.Add(this.fastRadioButton);
			this.previewGroupBox.Controls.Add(this.animatedRadioButton);
			this.previewGroupBox.Location = new System.Drawing.Point(106, 294);
			this.previewGroupBox.Name = "previewGroupBox";
			this.previewGroupBox.Size = new System.Drawing.Size(209, 49);
			this.previewGroupBox.TabIndex = 5;
			this.previewGroupBox.TabStop = false;
			this.previewGroupBox.Text = "Preview";
			// 
			// immediateRadioButton
			// 
			this.immediateRadioButton.AutoSize = true;
			this.immediateRadioButton.Location = new System.Drawing.Point(6, 19);
			this.immediateRadioButton.Name = "immediateRadioButton";
			this.immediateRadioButton.Size = new System.Drawing.Size(73, 17);
			this.immediateRadioButton.TabIndex = 0;
			this.immediateRadioButton.TabStop = true;
			this.immediateRadioButton.Text = "Immediate";
			this.immediateRadioButton.UseVisualStyleBackColor = true;
			// 
			// applyButton
			// 
			this.applyButton.Location = new System.Drawing.Point(181, 358);
			this.applyButton.Name = "applyButton";
			this.applyButton.Size = new System.Drawing.Size(75, 23);
			this.applyButton.TabIndex = 6;
			this.applyButton.Text = "Apply";
			this.applyButton.UseVisualStyleBackColor = true;
			this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(391, 360);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 7;
			this.closeButton.Text = "OK";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// alignmentPanel
			// 
			this.alignmentPanel.Controls.Add(this.gridDescriptionLabel);
			this.alignmentPanel.Controls.Add(this.rowDistanceLabel);
			this.alignmentPanel.Controls.Add(this.columnDistanceLabel);
			this.alignmentPanel.Controls.Add(this.rowDistanceTrackBar);
			this.alignmentPanel.Controls.Add(this.columnDistanceTrackBar);
			this.alignmentPanel.Controls.Add(this.label4);
			this.alignmentPanel.Controls.Add(this.label3);
			this.alignmentPanel.Location = new System.Drawing.Point(100, 4);
			this.alignmentPanel.Name = "alignmentPanel";
			this.alignmentPanel.Size = new System.Drawing.Size(367, 277);
			this.alignmentPanel.TabIndex = 5;
			this.alignmentPanel.Tag = "Alignment";
			// 
			// gridDescriptionLabel
			// 
			this.gridDescriptionLabel.Location = new System.Drawing.Point(12, 17);
			this.gridDescriptionLabel.Name = "gridDescriptionLabel";
			this.gridDescriptionLabel.Size = new System.Drawing.Size(340, 51);
			this.gridDescriptionLabel.TabIndex = 9;
			this.gridDescriptionLabel.Text = "Positions the shapes on grid intersections that depend on the average shape pieRa" +
    "dius and the set distances.";
			// 
			// rowDistanceLabel
			// 
			this.rowDistanceLabel.AutoSize = true;
			this.rowDistanceLabel.Location = new System.Drawing.Point(272, 149);
			this.rowDistanceLabel.Name = "rowDistanceLabel";
			this.rowDistanceLabel.Size = new System.Drawing.Size(25, 13);
			this.rowDistanceLabel.TabIndex = 7;
			this.rowDistanceLabel.Text = "120";
			// 
			// columnDistanceLabel
			// 
			this.columnDistanceLabel.AutoSize = true;
			this.columnDistanceLabel.Location = new System.Drawing.Point(280, 85);
			this.columnDistanceLabel.Name = "columnDistanceLabel";
			this.columnDistanceLabel.Size = new System.Drawing.Size(25, 13);
			this.columnDistanceLabel.TabIndex = 6;
			this.columnDistanceLabel.Text = "120";
			// 
			// rowDistanceTrackBar
			// 
			this.rowDistanceTrackBar.LargeChange = 200;
			this.rowDistanceTrackBar.Location = new System.Drawing.Point(15, 164);
			this.rowDistanceTrackBar.Maximum = 2000;
			this.rowDistanceTrackBar.Minimum = 100;
			this.rowDistanceTrackBar.Name = "rowDistanceTrackBar";
			this.rowDistanceTrackBar.Size = new System.Drawing.Size(337, 45);
			this.rowDistanceTrackBar.SmallChange = 50;
			this.rowDistanceTrackBar.TabIndex = 5;
			this.rowDistanceTrackBar.TickFrequency = 50;
			this.rowDistanceTrackBar.Value = 150;
			this.rowDistanceTrackBar.ValueChanged += new System.EventHandler(this.rowDistanceTrackBar_ValueChanged);
			// 
			// columnDistanceTrackBar
			// 
			this.columnDistanceTrackBar.AutoSize = false;
			this.columnDistanceTrackBar.LargeChange = 200;
			this.columnDistanceTrackBar.Location = new System.Drawing.Point(12, 101);
			this.columnDistanceTrackBar.Maximum = 2000;
			this.columnDistanceTrackBar.Minimum = 100;
			this.columnDistanceTrackBar.Name = "columnDistanceTrackBar";
			this.columnDistanceTrackBar.Size = new System.Drawing.Size(340, 45);
			this.columnDistanceTrackBar.SmallChange = 50;
			this.columnDistanceTrackBar.TabIndex = 4;
			this.columnDistanceTrackBar.TickFrequency = 50;
			this.columnDistanceTrackBar.Value = 120;
			this.columnDistanceTrackBar.ValueChanged += new System.EventHandler(this.columnDistanceTrackBar_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 149);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(171, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Coarseness in the vertical direction";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 85);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(182, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "Coarseness in the horizontal direction";
			// 
			// centerButton
			// 
			this.centerButton.Location = new System.Drawing.Point(100, 358);
			this.centerButton.Name = "centerButton";
			this.centerButton.Size = new System.Drawing.Size(75, 23);
			this.centerButton.TabIndex = 4;
			this.centerButton.Text = "Center";
			this.centerButton.UseVisualStyleBackColor = true;
			this.centerButton.Click += new System.EventHandler(this.centerButton_Click);
			// 
			// layoutTimer
			// 
			this.layoutTimer.Tick += new System.EventHandler(this.layoutTimer_Tick);
			// 
			// repulsionPanel
			// 
			this.repulsionPanel.Controls.Add(this.repulsionRangeTrackBar);
			this.repulsionPanel.Controls.Add(this.repulsionRangeLabel);
			this.repulsionPanel.Controls.Add(this.repulsionLabel4);
			this.repulsionPanel.Controls.Add(this.label14);
			this.repulsionPanel.Controls.Add(this.attractionStrengthLabel);
			this.repulsionPanel.Controls.Add(this.repulsionStrengthLabel);
			this.repulsionPanel.Controls.Add(this.repulsionStrengthTrackBar);
			this.repulsionPanel.Controls.Add(this.attractionStrengthTrackBar);
			this.repulsionPanel.Controls.Add(this.repulsionLabel3);
			this.repulsionPanel.Controls.Add(this.repulsionLabel2);
			this.repulsionPanel.Location = new System.Drawing.Point(100, 4);
			this.repulsionPanel.Name = "repulsionPanel";
			this.repulsionPanel.Size = new System.Drawing.Size(367, 277);
			this.repulsionPanel.TabIndex = 9;
			this.repulsionPanel.Tag = "Clusters";
			// 
			// repulsionRangeTrackBar
			// 
			this.repulsionRangeTrackBar.LargeChange = 50;
			this.repulsionRangeTrackBar.Location = new System.Drawing.Point(8, 187);
			this.repulsionRangeTrackBar.Maximum = 1000;
			this.repulsionRangeTrackBar.Minimum = 100;
			this.repulsionRangeTrackBar.Name = "repulsionRangeTrackBar";
			this.repulsionRangeTrackBar.Size = new System.Drawing.Size(344, 45);
			this.repulsionRangeTrackBar.SmallChange = 10;
			this.repulsionRangeTrackBar.TabIndex = 18;
			this.repulsionRangeTrackBar.TickFrequency = 10;
			this.repulsionRangeTrackBar.Value = 600;
			this.repulsionRangeTrackBar.ValueChanged += new System.EventHandler(this.repulsionRangeTrackBar_ValueChanged);
			// 
			// repulsionRangeLabel
			// 
			this.repulsionRangeLabel.AutoSize = true;
			this.repulsionRangeLabel.Location = new System.Drawing.Point(191, 169);
			this.repulsionRangeLabel.Name = "repulsionRangeLabel";
			this.repulsionRangeLabel.Size = new System.Drawing.Size(25, 13);
			this.repulsionRangeLabel.TabIndex = 17;
			this.repulsionRangeLabel.Text = "120";
			// 
			// repulsionLabel4
			// 
			this.repulsionLabel4.AutoSize = true;
			this.repulsionLabel4.Location = new System.Drawing.Point(5, 169);
			this.repulsionLabel4.Name = "repulsionLabel4";
			this.repulsionLabel4.Size = new System.Drawing.Size(180, 13);
			this.repulsionLabel4.TabIndex = 15;
			this.repulsionLabel4.Text = "Range of repulsion betwenn shapes:";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(3, 3);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(359, 31);
			this.label14.TabIndex = 14;
			this.label14.Text = "Moves connected shapes nearer together and thrusts unconnected shapes further apa" +
    "rt.";
			// 
			// attractionStrengthLabel
			// 
			this.attractionStrengthLabel.AutoSize = true;
			this.attractionStrengthLabel.Location = new System.Drawing.Point(254, 41);
			this.attractionStrengthLabel.Name = "attractionStrengthLabel";
			this.attractionStrengthLabel.Size = new System.Drawing.Size(25, 13);
			this.attractionStrengthLabel.TabIndex = 13;
			this.attractionStrengthLabel.Text = "120";
			// 
			// repulsionStrengthLabel
			// 
			this.repulsionStrengthLabel.AutoSize = true;
			this.repulsionStrengthLabel.Location = new System.Drawing.Point(198, 105);
			this.repulsionStrengthLabel.Name = "repulsionStrengthLabel";
			this.repulsionStrengthLabel.Size = new System.Drawing.Size(25, 13);
			this.repulsionStrengthLabel.TabIndex = 12;
			this.repulsionStrengthLabel.Text = "120";
			// 
			// repulsionStrengthTrackBar
			// 
			this.repulsionStrengthTrackBar.Location = new System.Drawing.Point(7, 121);
			this.repulsionStrengthTrackBar.Name = "repulsionStrengthTrackBar";
			this.repulsionStrengthTrackBar.Size = new System.Drawing.Size(344, 45);
			this.repulsionStrengthTrackBar.TabIndex = 11;
			this.repulsionStrengthTrackBar.Value = 1;
			this.repulsionStrengthTrackBar.ValueChanged += new System.EventHandler(this.repulsionStrengthTrackBar_ValueChanged);
			// 
			// attractionStrengthTrackBar
			// 
			this.attractionStrengthTrackBar.AutoSize = false;
			this.attractionStrengthTrackBar.Location = new System.Drawing.Point(7, 57);
			this.attractionStrengthTrackBar.Maximum = 20;
			this.attractionStrengthTrackBar.Name = "attractionStrengthTrackBar";
			this.attractionStrengthTrackBar.Size = new System.Drawing.Size(344, 45);
			this.attractionStrengthTrackBar.TabIndex = 10;
			this.attractionStrengthTrackBar.Value = 8;
			this.attractionStrengthTrackBar.ValueChanged += new System.EventHandler(this.attractionStrengthTrackBar_ValueChanged);
			// 
			// repulsionLabel3
			// 
			this.repulsionLabel3.AutoSize = true;
			this.repulsionLabel3.Location = new System.Drawing.Point(4, 105);
			this.repulsionLabel3.Name = "repulsionLabel3";
			this.repulsionLabel3.Size = new System.Drawing.Size(188, 13);
			this.repulsionLabel3.TabIndex = 9;
			this.repulsionLabel3.Text = "Strength of repulsion between shapes:";
			// 
			// repulsionLabel2
			// 
			this.repulsionLabel2.AutoSize = true;
			this.repulsionLabel2.Location = new System.Drawing.Point(4, 41);
			this.repulsionLabel2.Name = "repulsionLabel2";
			this.repulsionLabel2.Size = new System.Drawing.Size(244, 13);
			this.repulsionLabel2.TabIndex = 8;
			this.repulsionLabel2.Text = "Strength of attraction between connected shapes:";
			// 
			// flowPanel
			// 
			this.flowPanel.Controls.Add(this.flowDirectionGroupBox);
			this.flowPanel.Controls.Add(this.flowDescriptionLabel);
			this.flowPanel.Location = new System.Drawing.Point(100, 4);
			this.flowPanel.Name = "flowPanel";
			this.flowPanel.Size = new System.Drawing.Size(367, 277);
			this.flowPanel.TabIndex = 11;
			this.flowPanel.Tag = "Flow";
			// 
			// flowDirectionGroupBox
			// 
			this.flowDirectionGroupBox.Controls.Add(this.leftToRightRadioButton);
			this.flowDirectionGroupBox.Controls.Add(this.topDownRadioButton);
			this.flowDirectionGroupBox.Controls.Add(this.rightToLeftRadioButton);
			this.flowDirectionGroupBox.Controls.Add(this.bottomUpRadioButton);
			this.flowDirectionGroupBox.Location = new System.Drawing.Point(12, 82);
			this.flowDirectionGroupBox.Name = "flowDirectionGroupBox";
			this.flowDirectionGroupBox.Size = new System.Drawing.Size(340, 100);
			this.flowDirectionGroupBox.TabIndex = 6;
			this.flowDirectionGroupBox.TabStop = false;
			this.flowDirectionGroupBox.Text = "Flow Direction";
			// 
			// leftToRightRadioButton
			// 
			this.leftToRightRadioButton.AutoSize = true;
			this.leftToRightRadioButton.Location = new System.Drawing.Point(244, 42);
			this.leftToRightRadioButton.Name = "leftToRightRadioButton";
			this.leftToRightRadioButton.Size = new System.Drawing.Size(74, 17);
			this.leftToRightRadioButton.TabIndex = 6;
			this.leftToRightRadioButton.Text = "left to right";
			this.leftToRightRadioButton.UseVisualStyleBackColor = true;
			// 
			// topDownRadioButton
			// 
			this.topDownRadioButton.AutoSize = true;
			this.topDownRadioButton.Location = new System.Drawing.Point(129, 65);
			this.topDownRadioButton.Name = "topDownRadioButton";
			this.topDownRadioButton.Size = new System.Drawing.Size(69, 17);
			this.topDownRadioButton.TabIndex = 7;
			this.topDownRadioButton.Text = "top-down";
			this.topDownRadioButton.UseVisualStyleBackColor = true;
			// 
			// rightToLeftRadioButton
			// 
			this.rightToLeftRadioButton.AutoSize = true;
			this.rightToLeftRadioButton.Location = new System.Drawing.Point(19, 42);
			this.rightToLeftRadioButton.Name = "rightToLeftRadioButton";
			this.rightToLeftRadioButton.Size = new System.Drawing.Size(74, 17);
			this.rightToLeftRadioButton.TabIndex = 8;
			this.rightToLeftRadioButton.Text = "right to left";
			this.rightToLeftRadioButton.UseVisualStyleBackColor = true;
			// 
			// bottomUpRadioButton
			// 
			this.bottomUpRadioButton.AutoSize = true;
			this.bottomUpRadioButton.Location = new System.Drawing.Point(129, 19);
			this.bottomUpRadioButton.Name = "bottomUpRadioButton";
			this.bottomUpRadioButton.Size = new System.Drawing.Size(72, 17);
			this.bottomUpRadioButton.TabIndex = 5;
			this.bottomUpRadioButton.TabStop = true;
			this.bottomUpRadioButton.Text = "bottom-up";
			this.bottomUpRadioButton.UseVisualStyleBackColor = true;
			// 
			// flowDescriptionLabel
			// 
			this.flowDescriptionLabel.Location = new System.Drawing.Point(12, 17);
			this.flowDescriptionLabel.Name = "flowDescriptionLabel";
			this.flowDescriptionLabel.Size = new System.Drawing.Size(340, 52);
			this.flowDescriptionLabel.TabIndex = 0;
			this.flowDescriptionLabel.Text = resources.GetString("flowDescriptionLabel.Text");
			// 
			// previewButton
			// 
			this.previewButton.Location = new System.Drawing.Point(330, 310);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = new System.Drawing.Size(75, 23);
			this.previewButton.TabIndex = 12;
			this.previewButton.Text = "Preview";
			this.previewButton.UseVisualStyleBackColor = true;
			this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
			// 
			// algorithmListBox
			// 
			this.algorithmListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.algorithmListBox.BackColor = System.Drawing.SystemColors.Control;
			this.algorithmListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.algorithmListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.algorithmListBox.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(220)))));
			this.algorithmListBox.FocusedItemColor = System.Drawing.Color.Beige;
			this.algorithmListBox.FormattingEnabled = true;
			this.algorithmListBox.HighlightedItemColor = System.Drawing.SystemColors.ControlLightLight;
			this.algorithmListBox.InactiveItemBackgroundColor = System.Drawing.SystemColors.Control;
			this.algorithmListBox.InactiveItemBorderColor = System.Drawing.SystemColors.Window;
			this.algorithmListBox.InactiveItemTextColor = System.Drawing.SystemColors.ControlDarkDark;
			this.algorithmListBox.IntegralHeight = false;
			this.algorithmListBox.Items.AddRange(new object[] {
            "Clusters",
            "Flow",
            "Expansion",
            "Alignment"});
			this.algorithmListBox.Location = new System.Drawing.Point(0, 0);
			this.algorithmListBox.Name = "algorithmListBox";
			this.algorithmListBox.SelectedItemColor = System.Drawing.SystemColors.Window;
			this.algorithmListBox.SelectedItemTextColor = System.Drawing.SystemColors.ControlText;
			this.algorithmListBox.Size = new System.Drawing.Size(93, 397);
			this.algorithmListBox.TabIndex = 0;
			this.algorithmListBox.SelectedIndexChanged += new System.EventHandler(this.algorithmListBox_SelectedIndexChanged);
			// 
			// LayoutDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(475, 394);
			this.Controls.Add(this.repulsionPanel);
			this.Controls.Add(this.alignmentPanel);
			this.Controls.Add(expansionPanel);
			this.Controls.Add(this.flowPanel);
			this.Controls.Add(this.previewButton);
			this.Controls.Add(this.centerButton);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.applyButton);
			this.Controls.Add(this.previewGroupBox);
			this.Controls.Add(this.algorithmListBox);
			this.DoubleBuffered = true;
			this.Name = "LayoutDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Layout Control";
			this.Load += new System.EventHandler(this.LayoutControlForm_Load);
			this.Shown += new System.EventHandler(this.LayoutControlForm_Shown);
			expansionPanel.ResumeLayout(false);
			expansionPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.verticalCompressionTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.horizontalCompressionTrackBar)).EndInit();
			this.previewGroupBox.ResumeLayout(false);
			this.previewGroupBox.PerformLayout();
			this.alignmentPanel.ResumeLayout(false);
			this.alignmentPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.rowDistanceTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.columnDistanceTrackBar)).EndInit();
			this.repulsionPanel.ResumeLayout(false);
			this.repulsionPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.repulsionRangeTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.repulsionStrengthTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.attractionStrengthTrackBar)).EndInit();
			this.flowPanel.ResumeLayout(false);
			this.flowDirectionGroupBox.ResumeLayout(false);
			this.flowDirectionGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.VerticalTabControl algorithmListBox;
		private System.Windows.Forms.RadioButton fastRadioButton;
		private System.Windows.Forms.RadioButton animatedRadioButton;
		private System.Windows.Forms.GroupBox previewGroupBox;
		private System.Windows.Forms.Button applyButton;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Panel alignmentPanel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button centerButton;
		private System.Windows.Forms.Timer layoutTimer;
		private System.Windows.Forms.TrackBar rowDistanceTrackBar;
		private System.Windows.Forms.TrackBar columnDistanceTrackBar;
		private System.Windows.Forms.Label rowDistanceLabel;
		private System.Windows.Forms.Label columnDistanceLabel;
		private System.Windows.Forms.Panel repulsionPanel;
		private System.Windows.Forms.Label verticalCompressionLabel;
		private System.Windows.Forms.Label horizontalCompressionLabel;
		private System.Windows.Forms.TrackBar verticalCompressionTrackBar;
		private System.Windows.Forms.TrackBar horizontalCompressionTrackBar;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label expandDescriptionLabel;
		private System.Windows.Forms.Label gridDescriptionLabel;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label attractionStrengthLabel;
		private System.Windows.Forms.Label repulsionStrengthLabel;
		private System.Windows.Forms.TrackBar repulsionStrengthTrackBar;
		private System.Windows.Forms.TrackBar attractionStrengthTrackBar;
		private System.Windows.Forms.TrackBar repulsionRangeTrackBar;
		private System.Windows.Forms.Label repulsionLabel3;
		private System.Windows.Forms.Label repulsionLabel2;
		private System.Windows.Forms.Label repulsionRangeLabel;
		private System.Windows.Forms.Label repulsionLabel4;
		private System.Windows.Forms.CheckBox keepAspectRationCheckBox;
		private System.Windows.Forms.Panel flowPanel;
		private System.Windows.Forms.Label flowDescriptionLabel;
		private System.Windows.Forms.RadioButton immediateRadioButton;
		private System.Windows.Forms.GroupBox flowDirectionGroupBox;
		private System.Windows.Forms.RadioButton leftToRightRadioButton;
		private System.Windows.Forms.RadioButton topDownRadioButton;
		private System.Windows.Forms.RadioButton rightToLeftRadioButton;
		private System.Windows.Forms.RadioButton bottomUpRadioButton;
		private System.Windows.Forms.Button previewButton;
	}
}