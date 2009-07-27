namespace Dataweb.Diagramming.WinFormsUI {
	partial class LayerListView {
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerListView));
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.stateImageList = new System.Windows.Forms.ImageList(this.components);
			this.visibilityImageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Name = "contextMenuStrip1";
			this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
			// 
			// stateImageList
			// 
			this.stateImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("stateImageList.ImageStream")));
			this.stateImageList.TransparentColor = System.Drawing.Color.Fuchsia;
			this.stateImageList.Images.SetKeyName(0, "Disabled.bmp");
			this.stateImageList.Images.SetKeyName(1, "Enabled.bmp");
			// 
			// visibilityImageList
			// 
			this.visibilityImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("visibilityImageList.ImageStream")));
			this.visibilityImageList.TransparentColor = System.Drawing.Color.Fuchsia;
			this.visibilityImageList.Images.SetKeyName(0, "Invisible.bmp");
			this.visibilityImageList.Images.SetKeyName(1, "Visible.bmp");
			// 
			// LayerListView
			// 
			this.AllowColumnReorder = true;
			this.ContextMenuStrip = this.contextMenuStrip;
			this.FullRowSelect = true;
			this.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.HideSelection = false;
			this.LabelEdit = true;
			this.LabelWrap = false;
			this.OwnerDraw = true;
			this.ShowGroups = false;
			this.View = System.Windows.Forms.View.Details;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ImageList stateImageList;
		private System.Windows.Forms.ImageList visibilityImageList;
	}
}
