namespace nShape_Model_Demo {
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
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.cachedRepository = new Dataweb.NShape.Advanced.CachedRepository();
			this.display = new Dataweb.NShape.WinFormsUI.Display();
			this.toolSetController = new Dataweb.NShape.Controllers.ToolSetController();
			this.toolSetListViewPresenter = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.listView1 = new System.Windows.Forms.ListView();
			this.splitContainer = new System.Windows.Forms.SplitContainer();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// diagramSetController
			// 
			this.diagramSetController.ActiveTool = null;
			this.diagramSetController.Project = this.project;
			// 
			// project
			// 
			this.project.AutoGenerateTemplates = true;
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
			this.cachedRepository.Store = null;
			this.cachedRepository.Version = 0;
			// 
			// display
			// 
			this.display.AllowDrop = true;
			this.display.AutoScroll = true;
			this.display.BackColorGradient = System.Drawing.SystemColors.Control;
			this.display.BackgroundGradientAngle = 45;
			this.display.ConnectionPointShape = Dataweb.NShape.Controllers.ControlPointShape.Circle;
			this.display.ControlPointAlpha = ((byte)(255));
			this.display.DiagramSetController = this.diagramSetController;
			this.display.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display.GridAlpha = ((byte)(255));
			this.display.GridColor = System.Drawing.Color.White;
			this.display.GridSize = 20;
			this.display.GripSize = 3;
			this.display.HideDeniedMenuItems = false;
			this.display.HighQualityBackground = true;
			this.display.HighQualityRendering = true;
			this.display.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display.Location = new System.Drawing.Point(0, 0);
			this.display.MinRotateRange = 30;
			this.display.Name = "display";
			this.display.PropertyController = null;
			this.display.RenderingQualityHighQuality = Dataweb.NShape.Advanced.RenderingQuality.HighQuality;
			this.display.RenderingQualityLowQuality = Dataweb.NShape.Advanced.RenderingQuality.DefaultQuality;
			this.display.ResizeGripShape = Dataweb.NShape.Controllers.ControlPointShape.Square;
			this.display.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display.ShowGrid = true;
			this.display.ShowScrollBars = true;
			this.display.Size = new System.Drawing.Size(759, 572);
			this.display.SnapDistance = 5;
			this.display.SnapToGrid = true;
			this.display.TabIndex = 0;
			this.display.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			this.display.ZoomLevel = 100;
			this.display.ZoomWithMouseWheel = true;
			this.display.ShapeClick += new System.EventHandler<Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs>(this.display_ShapeClick);
			// 
			// toolSetController
			// 
			this.toolSetController.DiagramSetController = this.diagramSetController;
			// 
			// toolSetListViewPresenter
			// 
			this.toolSetListViewPresenter.ListView = this.listView1;
			this.toolSetListViewPresenter.ToolSetController = this.toolSetController;
			// 
			// listView1
			// 
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.ShowItemToolTips = true;
			this.listView1.Size = new System.Drawing.Size(146, 572);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.display);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.listView1);
			this.splitContainer.Size = new System.Drawing.Size(909, 572);
			this.splitContainer.SplitterDistance = 759;
			this.splitContainer.TabIndex = 1;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(909, 572);
			this.Controls.Add(this.splitContainer);
			this.Name = "MainForm";
			this.Text = "nShape Model Demo";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.Project project;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository;
		private Dataweb.NShape.Controllers.ToolSetController toolSetController;
		private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolSetListViewPresenter;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.SplitContainer splitContainer;
	}
}

