namespace Dataweb.nShape.Designer {

	partial class OpenProjectForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenProjectForm));
			this.okBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.newProjectRadioBtn = new System.Windows.Forms.RadioButton();
			this.openRecentProjectRadioBtn = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.projectNameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.fileNameTextBox = new System.Windows.Forms.TextBox();
			this.browseSaveButton = new System.Windows.Forms.Button();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.openProjectRadioBtn = new System.Windows.Forms.RadioButton();
			this.browseLoadButton = new System.Windows.Forms.Button();
			this.existingFileNameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okBtn.Location = new System.Drawing.Point(282, 313);
			this.okBtn.Name = "okBtn";
			this.okBtn.Size = new System.Drawing.Size(75, 23);
			this.okBtn.TabIndex = 12;
			this.okBtn.Text = "OK";
			this.okBtn.UseVisualStyleBackColor = true;
			// 
			// cancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.Location = new System.Drawing.Point(363, 313);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new System.Drawing.Size(75, 23);
			this.cancelBtn.TabIndex = 13;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(32, 159);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(406, 147);
			this.listBox1.TabIndex = 11;
			// 
			// newProjectRadioBtn
			// 
			this.newProjectRadioBtn.AutoSize = true;
			this.newProjectRadioBtn.Checked = true;
			this.newProjectRadioBtn.Location = new System.Drawing.Point(12, 12);
			this.newProjectRadioBtn.Name = "newProjectRadioBtn";
			this.newProjectRadioBtn.Size = new System.Drawing.Size(127, 17);
			this.newProjectRadioBtn.TabIndex = 0;
			this.newProjectRadioBtn.TabStop = true;
			this.newProjectRadioBtn.Text = "Create a new Project:";
			this.newProjectRadioBtn.UseVisualStyleBackColor = true;
			// 
			// openRecentProjectRadioBtn
			// 
			this.openRecentProjectRadioBtn.AutoSize = true;
			this.openRecentProjectRadioBtn.Location = new System.Drawing.Point(11, 136);
			this.openRecentProjectRadioBtn.Name = "openRecentProjectRadioBtn";
			this.openRecentProjectRadioBtn.Size = new System.Drawing.Size(156, 17);
			this.openRecentProjectRadioBtn.TabIndex = 10;
			this.openRecentProjectRadioBtn.Text = "Open recently used Project:";
			this.openRecentProjectRadioBtn.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(29, 38);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Project name:";
			// 
			// projectNameTextBox
			// 
			this.projectNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.projectNameTextBox.Location = new System.Drawing.Point(107, 35);
			this.projectNameTextBox.Name = "projectNameTextBox";
			this.projectNameTextBox.Size = new System.Drawing.Size(331, 20);
			this.projectNameTextBox.TabIndex = 2;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(29, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(79, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Repository File:";
			// 
			// fileNameTextBox
			// 
			this.fileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.fileNameTextBox.Location = new System.Drawing.Point(107, 61);
			this.fileNameTextBox.Name = "fileNameTextBox";
			this.fileNameTextBox.Size = new System.Drawing.Size(300, 20);
			this.fileNameTextBox.TabIndex = 4;
			// 
			// browseSaveButton
			// 
			this.browseSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseSaveButton.Location = new System.Drawing.Point(413, 59);
			this.browseSaveButton.Name = "browseSaveButton";
			this.browseSaveButton.Size = new System.Drawing.Size(25, 23);
			this.browseSaveButton.TabIndex = 5;
			this.browseSaveButton.Text = "...";
			this.browseSaveButton.UseVisualStyleBackColor = true;
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog1";
			// 
			// openProjectRadioBtn
			// 
			this.openProjectRadioBtn.AutoSize = true;
			this.openProjectRadioBtn.Location = new System.Drawing.Point(11, 90);
			this.openProjectRadioBtn.Name = "openProjectRadioBtn";
			this.openProjectRadioBtn.Size = new System.Drawing.Size(147, 17);
			this.openProjectRadioBtn.TabIndex = 6;
			this.openProjectRadioBtn.TabStop = true;
			this.openProjectRadioBtn.Text = "Open this existing Project:";
			this.openProjectRadioBtn.UseVisualStyleBackColor = true;
			// 
			// browseLoadButton
			// 
			this.browseLoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseLoadButton.Location = new System.Drawing.Point(413, 108);
			this.browseLoadButton.Name = "browseLoadButton";
			this.browseLoadButton.Size = new System.Drawing.Size(25, 23);
			this.browseLoadButton.TabIndex = 9;
			this.browseLoadButton.Text = "...";
			this.browseLoadButton.UseVisualStyleBackColor = true;
			// 
			// existingFileNameTextBox
			// 
			this.existingFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.existingFileNameTextBox.Location = new System.Drawing.Point(107, 110);
			this.existingFileNameTextBox.Name = "existingFileNameTextBox";
			this.existingFileNameTextBox.Size = new System.Drawing.Size(300, 20);
			this.existingFileNameTextBox.TabIndex = 8;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(29, 113);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(79, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Repository File:";
			// 
			// OpenProjectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(450, 348);
			this.Controls.Add(this.browseLoadButton);
			this.Controls.Add(this.existingFileNameTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.openProjectRadioBtn);
			this.Controls.Add(this.browseSaveButton);
			this.Controls.Add(this.fileNameTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.projectNameTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.openRecentProjectRadioBtn);
			this.Controls.Add(this.newProjectRadioBtn);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.okBtn);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "OpenProjectForm";
			this.Text = "OpenProjectForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.RadioButton newProjectRadioBtn;
		private System.Windows.Forms.RadioButton openRecentProjectRadioBtn;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox projectNameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox fileNameTextBox;
		private System.Windows.Forms.Button browseSaveButton;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.RadioButton openProjectRadioBtn;
		private System.Windows.Forms.Button browseLoadButton;
		private System.Windows.Forms.TextBox existingFileNameTextBox;
		private System.Windows.Forms.Label label3;
	}
}