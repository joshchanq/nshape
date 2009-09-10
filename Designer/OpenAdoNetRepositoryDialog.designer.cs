namespace Dataweb.nShape.Designer {

	partial class OpenAdoNetRepositoryDialog {
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
			this.label1 = new System.Windows.Forms.Label();
			this.serverNameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.databaseNameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.projectNameTextBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.providerNameComboBox = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server Name";
			// 
			// serverNameTextBox
			// 
			this.serverNameTextBox.Location = new System.Drawing.Point(12, 80);
			this.serverNameTextBox.Name = "serverNameTextBox";
			this.serverNameTextBox.Size = new System.Drawing.Size(306, 20);
			this.serverNameTextBox.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 115);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Database Name";
			// 
			// databaseNameTextBox
			// 
			this.databaseNameTextBox.Location = new System.Drawing.Point(12, 131);
			this.databaseNameTextBox.Name = "databaseNameTextBox";
			this.databaseNameTextBox.Size = new System.Drawing.Size(306, 20);
			this.databaseNameTextBox.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 166);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(71, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Project Name";
			// 
			// projectNameTextBox
			// 
			this.projectNameTextBox.Location = new System.Drawing.Point(12, 182);
			this.projectNameTextBox.Name = "projectNameTextBox";
			this.projectNameTextBox.Size = new System.Drawing.Size(306, 20);
			this.projectNameTextBox.TabIndex = 5;
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(243, 227);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(162, 227);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 7;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 13);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(77, 13);
			this.label4.TabIndex = 8;
			this.label4.Text = "Provider Name";
			// 
			// providerNameComboBox
			// 
			this.providerNameComboBox.FormattingEnabled = true;
			this.providerNameComboBox.Items.AddRange(new object[] {
            "SQL Server",
            "TurboDB"});
			this.providerNameComboBox.Location = new System.Drawing.Point(12, 29);
			this.providerNameComboBox.Name = "providerNameComboBox";
			this.providerNameComboBox.Size = new System.Drawing.Size(306, 21);
			this.providerNameComboBox.TabIndex = 9;
			this.providerNameComboBox.Text = "SQL Server";
			// 
			// OpenAdoNetRepositoryDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(330, 262);
			this.Controls.Add(this.providerNameComboBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.projectNameTextBox);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.databaseNameTextBox);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.serverNameTextBox);
			this.Controls.Add(this.label1);
			this.Name = "OpenAdoNetRepositoryDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Select Database Project";
			this.Load += new System.EventHandler(this.OpenAdoNetRepositoryDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox serverNameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox databaseNameTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox projectNameTextBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox providerNameComboBox;
	}
}