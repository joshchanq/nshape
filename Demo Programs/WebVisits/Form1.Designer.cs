namespace WebVisists {
	partial class MainForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			Dataweb.NShape.DefaultSecurity defaultSecurity1 = new Dataweb.NShape.DefaultSecurity();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadWebStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.display = new Dataweb.NShape.WinFormsUI.Display();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.cachedRepository = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore = new Dataweb.NShape.XmlStore();
			this.toolBoxAdapter = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.toolbox = new System.Windows.Forms.ListView();
			this.toolSetController = new Dataweb.NShape.Controllers.ToolSetController();
			this.mainMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadWebStatisticsToolStripMenuItem,
            this.loadDiagramToolStripMenuItem,
            this.saveDiagramToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadWebStatisticsToolStripMenuItem
			// 
			this.loadWebStatisticsToolStripMenuItem.Name = "loadWebStatisticsToolStripMenuItem";
			this.loadWebStatisticsToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.loadWebStatisticsToolStripMenuItem.Text = "Load Web statistics...";
			this.loadWebStatisticsToolStripMenuItem.Click += new System.EventHandler(this.loadWebStatisticsToolStripMenuItem_Click);
			// 
			// loadDiagramToolStripMenuItem
			// 
			this.loadDiagramToolStripMenuItem.Name = "loadDiagramToolStripMenuItem";
			this.loadDiagramToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.loadDiagramToolStripMenuItem.Text = "Load Diagram";
			this.loadDiagramToolStripMenuItem.Click += new System.EventHandler(this.loadDiagramToolStripMenuItem_Click);
			// 
			// saveDiagramToolStripMenuItem
			// 
			this.saveDiagramToolStripMenuItem.Name = "saveDiagramToolStripMenuItem";
			this.saveDiagramToolStripMenuItem.Size = new System.Drawing.Size(190, 22);
			this.saveDiagramToolStripMenuItem.Text = "Save Diagram";
			this.saveDiagramToolStripMenuItem.Click += new System.EventHandler(this.saveDiagramToolStripMenuItem_Click);
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(996, 24);
			this.mainMenuStrip.TabIndex = 3;
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.selectAllToolStripMenuItem.Text = "Select All";
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "Open Web Statistics File";
			// 
			// display
			// 
			this.display.AllowDrop = true;
			this.display.AutoScroll = true;
			this.display.BackColorGradient = System.Drawing.SystemColors.Control;
			this.display.BackgroundGradientAngle = 45;
			this.display.ConnectionPointShape = Dataweb.NShape.Controllers.ControlPointShape.Circle;
			this.display.ControlPointAlpha = ((byte)(255));
			this.display.CurrentTool = null;
			this.display.DiagramSetController = this.diagramSetController;
			this.display.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display.GridAlpha = ((byte)(255));
			this.display.GridColor = System.Drawing.Color.White;
			this.display.GridSize = 20;
			this.display.GripSize = 3;
			this.display.HighQualityBackground = true;
			this.display.HighQualityRendering = true;
			this.display.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display.Location = new System.Drawing.Point(0, 24);
			this.display.MinRotateRange = 30;
			this.display.Name = "display";
			this.display.RenderingQualityHighQuality = Dataweb.NShape.Advanced.RenderingQuality.HighQuality;
			this.display.RenderingQualityLowQuality = Dataweb.NShape.Advanced.RenderingQuality.DefaultQuality;
			this.display.ResizeGripShape = Dataweb.NShape.Controllers.ControlPointShape.Square;
			this.display.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display.ShowGrid = true;
			this.display.ShowScrollBars = true;
			this.display.Size = new System.Drawing.Size(996, 684);
			this.display.SnapDistance = 5;
			this.display.SnapToGrid = true;
			this.display.TabIndex = 0;
			this.display.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			this.display.ZoomLevel = 100;
			this.display.ZoomWithMouseWheel = true;
			this.display.Load += new System.EventHandler(this.display_Load);
			// 
			// diagramSetController
			// 
			this.diagramSetController.ActiveTool = null;
			this.diagramSetController.Project = this.project;
			// 
			// project
			// 
			this.project.AutoGenerateTemplates = false;
			this.project.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project.LibrarySearchPaths")));
			this.project.Name = null;
			this.project.Repository = this.cachedRepository;
			defaultSecurity1.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			defaultSecurity1.CurrentRoleName = "Administrator";
			this.project.SecurityManager = defaultSecurity1;
			// 
			// cachedRepository
			// 
			this.cachedRepository.ProjectName = null;
			this.cachedRepository.Store = this.xmlStore;
			this.cachedRepository.Version = 0;
			// 
			// xmlStore
			// 
			this.xmlStore.DesignFileName = "";
			this.xmlStore.DirectoryName = "";
			this.xmlStore.FileExtension = ".xml";
			this.xmlStore.ProjectName = "";
			this.xmlStore.Version = 0;
			// 
			// toolBoxAdapter
			// 
			this.toolBoxAdapter.ListView = this.toolbox;
			this.toolBoxAdapter.ToolSetController = this.toolSetController;
			// 
			// toolbox
			// 
			this.toolbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.toolbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.toolbox.FullRowSelect = true;
			this.toolbox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.toolbox.HideSelection = false;
			this.toolbox.Location = new System.Drawing.Point(0, 633);
			this.toolbox.MultiSelect = false;
			this.toolbox.Name = "toolbox";
			this.toolbox.ShowItemToolTips = true;
			this.toolbox.Size = new System.Drawing.Size(145, 76);
			this.toolbox.TabIndex = 4;
			this.toolbox.UseCompatibleStateImageBehavior = false;
			this.toolbox.View = System.Windows.Forms.View.Details;
			this.toolbox.SelectedIndexChanged += new System.EventHandler(this.toolbox_SelectedIndexChanged);
			// 
			// toolSetController
			// 
			this.toolSetController.DiagramSetController = this.diagramSetController;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(996, 708);
			this.Controls.Add(this.toolbox);
			this.Controls.Add(this.display);
			this.Controls.Add(this.mainMenuStrip);
			this.Name = "MainForm";
			this.Text = "WebVisits";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadWebStatisticsToolStripMenuItem;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripMenuItem saveDiagramToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadDiagramToolStripMenuItem;
		private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolBoxAdapter;
		private System.Windows.Forms.ListView toolbox;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.XmlStore xmlStore;
		private Dataweb.NShape.Controllers.ToolSetController toolSetController;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository;
		private Dataweb.NShape.Project project;
	}
}

