namespace Dataweb.NShape.WinFormsUI {
	partial class LibraryManagementDialog {
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
			this.addLibraryButton = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.closeButton = new System.Windows.Forms.Button();
			this.libraryListView = new System.Windows.Forms.ListView();
			this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// addLibraryButton
			// 
			this.addLibraryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addLibraryButton.Location = new System.Drawing.Point(12, 216);
			this.addLibraryButton.Name = "addLibraryButton";
			this.addLibraryButton.Size = new System.Drawing.Size(115, 23);
			this.addLibraryButton.TabIndex = 4;
			this.addLibraryButton.Text = "Add Library...";
			this.addLibraryButton.UseVisualStyleBackColor = true;
			this.addLibraryButton.Click += new System.EventHandler(this.addLibraryButton_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog";
			this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = new System.Drawing.Point(477, 216);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(75, 23);
			this.closeButton.TabIndex = 5;
			this.closeButton.Text = "OK";
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// libraryListView
			// 
			this.libraryListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.libraryListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderVersion,
            this.columnHeaderPath});
			this.libraryListView.Location = new System.Drawing.Point(3, 2);
			this.libraryListView.Name = "libraryListView";
			this.libraryListView.Size = new System.Drawing.Size(557, 200);
			this.libraryListView.TabIndex = 6;
			this.libraryListView.UseCompatibleStateImageBehavior = false;
			this.libraryListView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "Library Name";
			this.columnHeaderName.Width = 244;
			// 
			// columnHeaderVersion
			// 
			this.columnHeaderVersion.Text = "Version";
			this.columnHeaderVersion.Width = 54;
			// 
			// columnHeaderPath
			// 
			this.columnHeaderPath.Text = "Library Path";
			this.columnHeaderPath.Width = 254;
			// 
			// LibraryManagementDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.closeButton;
			this.ClientSize = new System.Drawing.Size(564, 249);
			this.Controls.Add(this.libraryListView);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.addLibraryButton);
			this.Name = "LibraryManagementDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Loaded Libraries";
			this.Load += new System.EventHandler(this.LibraryManagementDialog_Load);
			this.Shown += new System.EventHandler(this.LibraryManagementDialog_Shown);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button addLibraryButton;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.ListView libraryListView;
		private System.Windows.Forms.ColumnHeader columnHeaderName;
		private System.Windows.Forms.ColumnHeader columnHeaderVersion;
		private System.Windows.Forms.ColumnHeader columnHeaderPath;
	}
}