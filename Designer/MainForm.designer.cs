namespace Dataweb.nShape.Designer {

	partial class DiagramDesignerMainForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagramDesignerMainForm));
			Dataweb.nShape.DefaultSecurity defaultSecurity1 = new Dataweb.nShape.DefaultSecurity();
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.statusLabelPosition = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusLabelMessage = new System.Windows.Forms.ToolStripStatusLabel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.projectInXMLFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.projectInSQLServerDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openXMLRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openSQLServerRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.recentProjectsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.ManageShapeAndModelLibrariesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exportDiagramAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.graphicsFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.emfExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.wmfExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pngExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.jpgExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.bmpExportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.quitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.insertDiagramMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showDiagramSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
			this.cutShapeOnlyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cutShapeAndModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyShapeOnlyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyShapeAndModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteShapeOnlyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteShapeAndModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.undoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.toForegroundMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toBackgroundMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showGridMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showDisplaySettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.editDesignsAndStylesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.viewShowLayoutControlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.highQualityRenderingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.adoNetDatabaseGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.defaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.toolboxPropsPanel = new System.Windows.Forms.Panel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.listView = new System.Windows.Forms.ListView();
			this.propertyWindowTabControl = new System.Windows.Forms.TabControl();
			this.propertyWindowShapeTab = new System.Windows.Forms.TabPage();
			this.viewObjectPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.propertyWindowModelTab = new System.Windows.Forms.TabPage();
			this.modelObjectPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.layersTab = new System.Windows.Forms.TabPage();
			this.layerEditorListView1 = new Dataweb.nShape.WinFormsUI.LayerListView();
			this.diagramSetController = new Dataweb.nShape.Controllers.DiagramSetController();
			this.project = new Dataweb.nShape.Project(this.components);
			this.cachedRepository = new Dataweb.nShape.Advanced.CachedRepository();
			this.layerController = new Dataweb.nShape.Controllers.LayerController();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.displayTabControl = new System.Windows.Forms.TabControl();
			this.historyTrackBar = new System.Windows.Forms.TrackBar();
			this.modelTreeView = new System.Windows.Forms.TreeView();
			this.navigationToolStrip = new System.Windows.Forms.ToolStrip();
			this.prevDiagramButton = new System.Windows.Forms.ToolStripButton();
			this.nextDiagramButton = new System.Windows.Forms.ToolStripButton();
			this.editToolStrip = new System.Windows.Forms.ToolStrip();
			this.cutShapeButton = new System.Windows.Forms.ToolStripButton();
			this.copyShapeButton = new System.Windows.Forms.ToolStripButton();
			this.pasteButton = new System.Windows.Forms.ToolStripButton();
			this.deleteShapeButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.undoToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
			this.redoToolStripSplitButton = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.settingsToolStrip = new System.Windows.Forms.ToolStrip();
			this.zoomToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
			this.refreshToolbarButton = new System.Windows.Forms.ToolStripButton();
			this.showGridToolbarButton = new System.Windows.Forms.ToolStripButton();
			this.displayToolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.runtimeModeComboBox = new System.Windows.Forms.ToolStripComboBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.toolBoxController = new Dataweb.nShape.Controllers.ToolSetController();
			this.modelTreeController = new Dataweb.nShape.Controllers.ModelController();
			this.modelTreeAdapter = new Dataweb.nShape.WinFormsUI.ModelTreeViewPresenter();
			this.propertyGridAdapter = new Dataweb.nShape.WinFormsUI.PropertyPresenter();
			this.toolBoxListViewAdapter = new Dataweb.nShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.layerPresenter = new Dataweb.nShape.Controllers.LayerPresenter();
			this.statusStrip.SuspendLayout();
			this.mainMenuStrip.SuspendLayout();
			this.toolboxPropsPanel.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.propertyWindowTabControl.SuspendLayout();
			this.propertyWindowShapeTab.SuspendLayout();
			this.propertyWindowModelTab.SuspendLayout();
			this.layersTab.SuspendLayout();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.historyTrackBar)).BeginInit();
			this.navigationToolStrip.SuspendLayout();
			this.editToolStrip.SuspendLayout();
			this.settingsToolStrip.SuspendLayout();
			this.displayToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BottomToolStripPanel
			// 
			this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// statusStrip
			// 
			this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelPosition,
            this.statusLabelMessage});
			this.statusStrip.Location = new System.Drawing.Point(0, 0);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.statusStrip.Size = new System.Drawing.Size(1016, 22);
			this.statusStrip.TabIndex = 1;
			this.statusStrip.Text = "statusStrip";
			// 
			// statusLabelPosition
			// 
			this.statusLabelPosition.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.statusLabelPosition.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.statusLabelPosition.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.statusLabelPosition.Name = "statusLabelPosition";
			this.statusLabelPosition.Size = new System.Drawing.Size(56, 17);
			this.statusLabelPosition.Text = "X: 0, Y: 0";
			this.statusLabelPosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.statusLabelPosition.ToolTipText = "Mouse Position";
			// 
			// statusLabelMessage
			// 
			this.statusLabelMessage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.statusLabelMessage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.statusLabelMessage.Name = "statusLabelMessage";
			this.statusLabelMessage.Size = new System.Drawing.Size(945, 17);
			this.statusLabelMessage.Spring = true;
			this.statusLabelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TopToolStripPanel
			// 
			this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(1016, 24);
			this.mainMenuStrip.TabIndex = 7;
			this.mainMenuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newProjectToolStripMenuItem,
            this.openProjectMenuItem,
            this.recentProjectsMenuItem,
            this.toolStripMenuItem7,
            this.saveMenuItem,
            this.saveAsMenuItem,
            this.closeProjectToolStripMenuItem,
            this.toolStripMenuItem5,
            this.ManageShapeAndModelLibrariesMenuItem,
            this.exportDiagramAsMenuItem,
            this.toolStripMenuItem1,
            this.quitMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// newProjectToolStripMenuItem
			// 
			this.newProjectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectInXMLFileToolStripMenuItem,
            this.projectInSQLServerDatabaseToolStripMenuItem});
			this.newProjectToolStripMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.NewDiagramBtn2;
			this.newProjectToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.newProjectToolStripMenuItem.Name = "newProjectToolStripMenuItem";
			this.newProjectToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.newProjectToolStripMenuItem.Text = "New Project";
			// 
			// projectInXMLFileToolStripMenuItem
			// 
			this.projectInXMLFileToolStripMenuItem.Name = "projectInXMLFileToolStripMenuItem";
			this.projectInXMLFileToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
			this.projectInXMLFileToolStripMenuItem.Text = "XML Repository...";
			this.projectInXMLFileToolStripMenuItem.Click += new System.EventHandler(this.newXMLRepositoryToolStripMenuItem_Click);
			// 
			// projectInSQLServerDatabaseToolStripMenuItem
			// 
			this.projectInSQLServerDatabaseToolStripMenuItem.Name = "projectInSQLServerDatabaseToolStripMenuItem";
			this.projectInSQLServerDatabaseToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
			this.projectInSQLServerDatabaseToolStripMenuItem.Text = "SQL Server Repository...";
			this.projectInSQLServerDatabaseToolStripMenuItem.Click += new System.EventHandler(this.newSQLServerRepositoryToolStripMenuItem_Click);
			// 
			// openProjectMenuItem
			// 
			this.openProjectMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openXMLRepositoryToolStripMenuItem,
            this.openSQLServerRepositoryToolStripMenuItem});
			this.openProjectMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.OpenBtn;
			this.openProjectMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.openProjectMenuItem.Name = "openProjectMenuItem";
			this.openProjectMenuItem.Size = new System.Drawing.Size(263, 22);
			this.openProjectMenuItem.Text = "Open Project";
			// 
			// openXMLRepositoryToolStripMenuItem
			// 
			this.openXMLRepositoryToolStripMenuItem.Name = "openXMLRepositoryToolStripMenuItem";
			this.openXMLRepositoryToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
			this.openXMLRepositoryToolStripMenuItem.Text = "XML Repository...";
			this.openXMLRepositoryToolStripMenuItem.Click += new System.EventHandler(this.openXMLRepositoryToolStripMenuItem_Click);
			// 
			// openSQLServerRepositoryToolStripMenuItem
			// 
			this.openSQLServerRepositoryToolStripMenuItem.Name = "openSQLServerRepositoryToolStripMenuItem";
			this.openSQLServerRepositoryToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
			this.openSQLServerRepositoryToolStripMenuItem.Text = "SQL Server Repository...";
			this.openSQLServerRepositoryToolStripMenuItem.Click += new System.EventHandler(this.openSQLServerRepositoryToolStripMenuItem_Click);
			// 
			// recentProjectsMenuItem
			// 
			this.recentProjectsMenuItem.Name = "recentProjectsMenuItem";
			this.recentProjectsMenuItem.Size = new System.Drawing.Size(263, 22);
			this.recentProjectsMenuItem.Text = "Recent Projects";
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Size = new System.Drawing.Size(260, 6);
			// 
			// saveMenuItem
			// 
			this.saveMenuItem.Enabled = false;
			this.saveMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveMenuItem.Image")));
			this.saveMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.saveMenuItem.Name = "saveMenuItem";
			this.saveMenuItem.Size = new System.Drawing.Size(263, 22);
			this.saveMenuItem.Text = "Save";
			this.saveMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// saveAsMenuItem
			// 
			this.saveAsMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.saveAsMenuItem.Name = "saveAsMenuItem";
			this.saveAsMenuItem.Size = new System.Drawing.Size(263, 22);
			this.saveAsMenuItem.Text = "Save as...";
			this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// closeProjectToolStripMenuItem
			// 
			this.closeProjectToolStripMenuItem.Name = "closeProjectToolStripMenuItem";
			this.closeProjectToolStripMenuItem.Size = new System.Drawing.Size(263, 22);
			this.closeProjectToolStripMenuItem.Text = "Close Project";
			this.closeProjectToolStripMenuItem.Click += new System.EventHandler(this.closeProjectToolStripMenuItem_Click);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(260, 6);
			// 
			// ManageShapeAndModelLibrariesMenuItem
			// 
			this.ManageShapeAndModelLibrariesMenuItem.Name = "ManageShapeAndModelLibrariesMenuItem";
			this.ManageShapeAndModelLibrariesMenuItem.Size = new System.Drawing.Size(263, 22);
			this.ManageShapeAndModelLibrariesMenuItem.Text = "Manage Shape and Model Libraries...";
			this.ManageShapeAndModelLibrariesMenuItem.Click += new System.EventHandler(this.ManageShapeAndModelLibrariesMenuItem_Click);
			// 
			// exportDiagramAsMenuItem
			// 
			this.exportDiagramAsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.graphicsFileToolStripMenuItem,
            this.emfExportMenuItem,
            this.wmfExportMenuItem,
            this.pngExportMenuItem,
            this.jpgExportMenuItem,
            this.bmpExportMenuItem});
			this.exportDiagramAsMenuItem.Name = "exportDiagramAsMenuItem";
			this.exportDiagramAsMenuItem.Size = new System.Drawing.Size(263, 22);
			this.exportDiagramAsMenuItem.Text = "Export diagram";
			// 
			// graphicsFileToolStripMenuItem
			// 
			this.graphicsFileToolStripMenuItem.Name = "graphicsFileToolStripMenuItem";
			this.graphicsFileToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
			this.graphicsFileToolStripMenuItem.Text = "Graphics file...";
			this.graphicsFileToolStripMenuItem.Click += new System.EventHandler(this.exportDiagramAsMenuItem_Click);
			// 
			// emfExportMenuItem
			// 
			this.emfExportMenuItem.Name = "emfExportMenuItem";
			this.emfExportMenuItem.Size = new System.Drawing.Size(155, 22);
			this.emfExportMenuItem.Text = "EMF Plus file";
			this.emfExportMenuItem.Click += new System.EventHandler(this.emfPlusFileToolStripMenuItem_Click);
			// 
			// wmfExportMenuItem
			// 
			this.wmfExportMenuItem.Name = "wmfExportMenuItem";
			this.wmfExportMenuItem.Size = new System.Drawing.Size(155, 22);
			this.wmfExportMenuItem.Text = "EMF file";
			this.wmfExportMenuItem.Click += new System.EventHandler(this.emfOnlyFileToolStripMenuItem_Click);
			// 
			// pngExportMenuItem
			// 
			this.pngExportMenuItem.Name = "pngExportMenuItem";
			this.pngExportMenuItem.Size = new System.Drawing.Size(155, 22);
			this.pngExportMenuItem.Text = "PNG file";
			this.pngExportMenuItem.Click += new System.EventHandler(this.pngFileToolStripMenuItem_Click);
			// 
			// jpgExportMenuItem
			// 
			this.jpgExportMenuItem.Name = "jpgExportMenuItem";
			this.jpgExportMenuItem.Size = new System.Drawing.Size(155, 22);
			this.jpgExportMenuItem.Text = "JPG file";
			this.jpgExportMenuItem.Click += new System.EventHandler(this.jpgFileToolStripMenuItem_Click);
			// 
			// bmpExportMenuItem
			// 
			this.bmpExportMenuItem.Name = "bmpExportMenuItem";
			this.bmpExportMenuItem.Size = new System.Drawing.Size(155, 22);
			this.bmpExportMenuItem.Text = "BMP file";
			this.bmpExportMenuItem.Click += new System.EventHandler(this.bmpFileToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(260, 6);
			// 
			// quitMenuItem
			// 
			this.quitMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.quitMenuItem.Name = "quitMenuItem";
			this.quitMenuItem.Size = new System.Drawing.Size(263, 22);
			this.quitMenuItem.Text = "Quit";
			this.quitMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertDiagramMenuItem,
            this.deleteDiagramToolStripMenuItem,
            this.showDiagramSettingsToolStripMenuItem,
            this.toolStripMenuItem8,
            this.cutShapeOnlyMenuItem,
            this.cutShapeAndModelMenuItem,
            this.copyShapeOnlyMenuItem,
            this.copyShapeAndModelMenuItem,
            this.pasteMenuItem,
            this.deleteShapeOnlyMenuItem,
            this.deleteShapeAndModelMenuItem,
            this.toolStripMenuItem2,
            this.undoMenuItem,
            this.redoMenuItem,
            this.toolStripMenuItem6,
            this.toForegroundMenuItem,
            this.toBackgroundMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// insertDiagramMenuItem
			// 
			this.insertDiagramMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.NewDiagramBtn2;
			this.insertDiagramMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.insertDiagramMenuItem.Name = "insertDiagramMenuItem";
			this.insertDiagramMenuItem.Size = new System.Drawing.Size(256, 22);
			this.insertDiagramMenuItem.Text = "Insert Diagram";
			this.insertDiagramMenuItem.Click += new System.EventHandler(this.newDiagramToolStripMenuItem_Click);
			// 
			// deleteDiagramToolStripMenuItem
			// 
			this.deleteDiagramToolStripMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.DeleteDiagramBtn2;
			this.deleteDiagramToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.deleteDiagramToolStripMenuItem.Name = "deleteDiagramToolStripMenuItem";
			this.deleteDiagramToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
			this.deleteDiagramToolStripMenuItem.Text = "Delete Diagram";
			this.deleteDiagramToolStripMenuItem.Click += new System.EventHandler(this.deleteDiagramToolStripMenuItem_Click);
			// 
			// showDiagramSettingsToolStripMenuItem
			// 
			this.showDiagramSettingsToolStripMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.DiagramPropertiesBtn3;
			this.showDiagramSettingsToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.showDiagramSettingsToolStripMenuItem.Name = "showDiagramSettingsToolStripMenuItem";
			this.showDiagramSettingsToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
			this.showDiagramSettingsToolStripMenuItem.Text = "Diagram Properties";
			this.showDiagramSettingsToolStripMenuItem.Click += new System.EventHandler(this.showDiagramSettingsToolStripMenuItem_Click);
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			this.toolStripMenuItem8.Size = new System.Drawing.Size(253, 6);
			// 
			// cutShapeOnlyMenuItem
			// 
			this.cutShapeOnlyMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.CutBtn;
			this.cutShapeOnlyMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.cutShapeOnlyMenuItem.Name = "cutShapeOnlyMenuItem";
			this.cutShapeOnlyMenuItem.Size = new System.Drawing.Size(256, 22);
			this.cutShapeOnlyMenuItem.Text = "Cut Shape";
			this.cutShapeOnlyMenuItem.Click += new System.EventHandler(this.cutShapeOnlyItem_Click);
			// 
			// cutShapeAndModelMenuItem
			// 
			this.cutShapeAndModelMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.CutBtn;
			this.cutShapeAndModelMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.cutShapeAndModelMenuItem.Name = "cutShapeAndModelMenuItem";
			this.cutShapeAndModelMenuItem.Size = new System.Drawing.Size(256, 22);
			this.cutShapeAndModelMenuItem.Text = "Cut Shape and Model";
			this.cutShapeAndModelMenuItem.Click += new System.EventHandler(this.cutShapeAndModelItem_Click);
			// 
			// copyShapeOnlyMenuItem
			// 
			this.copyShapeOnlyMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.CopyBtn;
			this.copyShapeOnlyMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.copyShapeOnlyMenuItem.Name = "copyShapeOnlyMenuItem";
			this.copyShapeOnlyMenuItem.Size = new System.Drawing.Size(256, 22);
			this.copyShapeOnlyMenuItem.Text = "Copy Shape";
			this.copyShapeOnlyMenuItem.Click += new System.EventHandler(this.copyShapeOnlyItem_Click);
			// 
			// copyShapeAndModelMenuItem
			// 
			this.copyShapeAndModelMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.CopyBtn;
			this.copyShapeAndModelMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.copyShapeAndModelMenuItem.Name = "copyShapeAndModelMenuItem";
			this.copyShapeAndModelMenuItem.Size = new System.Drawing.Size(256, 22);
			this.copyShapeAndModelMenuItem.Text = "Copy Shape and Model";
			this.copyShapeAndModelMenuItem.Visible = false;
			this.copyShapeAndModelMenuItem.Click += new System.EventHandler(this.copyShapeAndModelItem_Click);
			// 
			// pasteMenuItem
			// 
			this.pasteMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.PasteBtn;
			this.pasteMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.pasteMenuItem.Name = "pasteMenuItem";
			this.pasteMenuItem.Size = new System.Drawing.Size(256, 22);
			this.pasteMenuItem.Text = "Paste Shape";
			this.pasteMenuItem.Click += new System.EventHandler(this.pasteMenuItem_Click);
			// 
			// deleteShapeOnlyMenuItem
			// 
			this.deleteShapeOnlyMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.DeleteBtn;
			this.deleteShapeOnlyMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.deleteShapeOnlyMenuItem.Name = "deleteShapeOnlyMenuItem";
			this.deleteShapeOnlyMenuItem.ShortcutKeyDisplayString = "(Del)";
			this.deleteShapeOnlyMenuItem.Size = new System.Drawing.Size(256, 22);
			this.deleteShapeOnlyMenuItem.Text = "Delete Shape";
			this.deleteShapeOnlyMenuItem.Click += new System.EventHandler(this.deleteShapeOnlyItem_Click);
			// 
			// deleteShapeAndModelMenuItem
			// 
			this.deleteShapeAndModelMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.DeleteBtn;
			this.deleteShapeAndModelMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.deleteShapeAndModelMenuItem.Name = "deleteShapeAndModelMenuItem";
			this.deleteShapeAndModelMenuItem.ShortcutKeyDisplayString = "(Ctrl+Del)";
			this.deleteShapeAndModelMenuItem.Size = new System.Drawing.Size(256, 22);
			this.deleteShapeAndModelMenuItem.Text = "Delete Shape and Model";
			this.deleteShapeAndModelMenuItem.Visible = false;
			this.deleteShapeAndModelMenuItem.Click += new System.EventHandler(this.deleteShapeAndModelItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(253, 6);
			// 
			// undoMenuItem
			// 
			this.undoMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.UndoBtn;
			this.undoMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.undoMenuItem.Name = "undoMenuItem";
			this.undoMenuItem.Size = new System.Drawing.Size(256, 22);
			this.undoMenuItem.Text = "Undo";
			this.undoMenuItem.Click += new System.EventHandler(this.undoButton_Click);
			// 
			// redoMenuItem
			// 
			this.redoMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.RedoBtn;
			this.redoMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.redoMenuItem.Name = "redoMenuItem";
			this.redoMenuItem.Size = new System.Drawing.Size(256, 22);
			this.redoMenuItem.Text = "Redo";
			this.redoMenuItem.Click += new System.EventHandler(this.redoButton_Click);
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new System.Drawing.Size(253, 6);
			// 
			// toForegroundMenuItem
			// 
			this.toForegroundMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.ToForeground;
			this.toForegroundMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.toForegroundMenuItem.Name = "toForegroundMenuItem";
			this.toForegroundMenuItem.Size = new System.Drawing.Size(256, 22);
			this.toForegroundMenuItem.Text = "Bring to Front";
			// 
			// toBackgroundMenuItem
			// 
			this.toBackgroundMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.ToBackground;
			this.toBackgroundMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.toBackgroundMenuItem.Name = "toBackgroundMenuItem";
			this.toBackgroundMenuItem.Size = new System.Drawing.Size(256, 22);
			this.toBackgroundMenuItem.Text = "Send to Back";
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showGridMenuItem,
            this.refreshToolStripMenuItem,
            this.showDisplaySettingsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.editDesignsAndStylesToolStripMenuItem,
            this.viewShowLayoutControlToolStripMenuItem,
            this.toolStripMenuItem4,
            this.highQualityRenderingMenuItem});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
			this.viewToolStripMenuItem.Text = "View";
			// 
			// showGridMenuItem
			// 
			this.showGridMenuItem.Checked = true;
			this.showGridMenuItem.CheckOnClick = true;
			this.showGridMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showGridMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.ToggleGridBtn2;
			this.showGridMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.showGridMenuItem.Name = "showGridMenuItem";
			this.showGridMenuItem.Size = new System.Drawing.Size(235, 22);
			this.showGridMenuItem.Text = "Show Grid";
			this.showGridMenuItem.Click += new System.EventHandler(this.showGridToolStripMenuItem_Click);
			// 
			// refreshToolStripMenuItem
			// 
			this.refreshToolStripMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.RefreshBtn;
			this.refreshToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
			this.refreshToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
			this.refreshToolStripMenuItem.Text = "Refresh Display";
			this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// showDisplaySettingsToolStripMenuItem
			// 
			this.showDisplaySettingsToolStripMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.PropertiesBtn;
			this.showDisplaySettingsToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.showDisplaySettingsToolStripMenuItem.Name = "showDisplaySettingsToolStripMenuItem";
			this.showDisplaySettingsToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
			this.showDisplaySettingsToolStripMenuItem.Text = "Display Settings";
			this.showDisplaySettingsToolStripMenuItem.Click += new System.EventHandler(this.showDisplaySettingsItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(232, 6);
			// 
			// editDesignsAndStylesToolStripMenuItem
			// 
			this.editDesignsAndStylesToolStripMenuItem.Image = global::Dataweb.nShape.Designer.Properties.Resources.DesignEditorBtn;
			this.editDesignsAndStylesToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.editDesignsAndStylesToolStripMenuItem.Name = "editDesignsAndStylesToolStripMenuItem";
			this.editDesignsAndStylesToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
			this.editDesignsAndStylesToolStripMenuItem.Text = "Show Designs and Styles Editor";
			this.editDesignsAndStylesToolStripMenuItem.Click += new System.EventHandler(this.editDesignsAndStylesToolStripMenuItem_Click);
			// 
			// viewShowLayoutControlToolStripMenuItem
			// 
			this.viewShowLayoutControlToolStripMenuItem.Name = "viewShowLayoutControlToolStripMenuItem";
			this.viewShowLayoutControlToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
			this.viewShowLayoutControlToolStripMenuItem.Text = "Show Layout Control";
			this.viewShowLayoutControlToolStripMenuItem.Click += new System.EventHandler(this.viewShowLayoutControlToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(232, 6);
			// 
			// highQualityRenderingMenuItem
			// 
			this.highQualityRenderingMenuItem.Checked = true;
			this.highQualityRenderingMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.highQualityRenderingMenuItem.Name = "highQualityRenderingMenuItem";
			this.highQualityRenderingMenuItem.Size = new System.Drawing.Size(235, 22);
			this.highQualityRenderingMenuItem.Text = "High Quality Rendering";
			this.highQualityRenderingMenuItem.Click += new System.EventHandler(this.highQualityToolStripMenuItem_Click);
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.adoNetDatabaseGeneratorToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// adoNetDatabaseGeneratorToolStripMenuItem
			// 
			this.adoNetDatabaseGeneratorToolStripMenuItem.Name = "adoNetDatabaseGeneratorToolStripMenuItem";
			this.adoNetDatabaseGeneratorToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.adoNetDatabaseGeneratorToolStripMenuItem.Text = "ADO.NET Database Generator";
			this.adoNetDatabaseGeneratorToolStripMenuItem.Click += new System.EventHandler(this.adoNetDatabaseGeneratorToolStripMenuItem_Click);
			// 
			// defaultToolStripMenuItem
			// 
			this.defaultToolStripMenuItem.Name = "defaultToolStripMenuItem";
			this.defaultToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// RightToolStripPanel
			// 
			this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.RightToolStripPanel.Name = "RightToolStripPanel";
			this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// LeftToolStripPanel
			// 
			this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.LeftToolStripPanel.Name = "LeftToolStripPanel";
			this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// ContentPanel
			// 
			this.ContentPanel.AutoScroll = true;
			this.ContentPanel.Size = new System.Drawing.Size(928, 522);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(150, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 663);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// toolboxPropsPanel
			// 
			this.toolboxPropsPanel.Controls.Add(this.splitContainer1);
			this.toolboxPropsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.toolboxPropsPanel.Location = new System.Drawing.Point(788, 0);
			this.toolboxPropsPanel.Name = "toolboxPropsPanel";
			this.toolboxPropsPanel.Size = new System.Drawing.Size(228, 663);
			this.toolboxPropsPanel.TabIndex = 4;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.listView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyWindowTabControl);
			this.splitContainer1.Size = new System.Drawing.Size(228, 663);
			this.splitContainer1.SplitterDistance = 264;
			this.splitContainer1.TabIndex = 0;
			// 
			// listView
			// 
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.FullRowSelect = true;
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listView.HideSelection = false;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.MultiSelect = false;
			this.listView.Name = "listView";
			this.listView.ShowItemToolTips = true;
			this.listView.Size = new System.Drawing.Size(228, 264);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			// 
			// propertyWindowTabControl
			// 
			this.propertyWindowTabControl.Controls.Add(this.propertyWindowShapeTab);
			this.propertyWindowTabControl.Controls.Add(this.propertyWindowModelTab);
			this.propertyWindowTabControl.Controls.Add(this.layersTab);
			this.propertyWindowTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyWindowTabControl.Location = new System.Drawing.Point(0, 0);
			this.propertyWindowTabControl.Name = "propertyWindowTabControl";
			this.propertyWindowTabControl.SelectedIndex = 0;
			this.propertyWindowTabControl.Size = new System.Drawing.Size(228, 395);
			this.propertyWindowTabControl.TabIndex = 0;
			// 
			// propertyWindowShapeTab
			// 
			this.propertyWindowShapeTab.Controls.Add(this.viewObjectPropertyGrid);
			this.propertyWindowShapeTab.Location = new System.Drawing.Point(4, 22);
			this.propertyWindowShapeTab.Name = "propertyWindowShapeTab";
			this.propertyWindowShapeTab.Padding = new System.Windows.Forms.Padding(3);
			this.propertyWindowShapeTab.Size = new System.Drawing.Size(220, 369);
			this.propertyWindowShapeTab.TabIndex = 0;
			this.propertyWindowShapeTab.Text = "Shape";
			this.propertyWindowShapeTab.UseVisualStyleBackColor = true;
			// 
			// viewObjectPropertyGrid
			// 
			this.viewObjectPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.viewObjectPropertyGrid.Location = new System.Drawing.Point(3, 3);
			this.viewObjectPropertyGrid.Name = "viewObjectPropertyGrid";
			this.viewObjectPropertyGrid.Size = new System.Drawing.Size(214, 363);
			this.viewObjectPropertyGrid.TabIndex = 1;
			// 
			// propertyWindowModelTab
			// 
			this.propertyWindowModelTab.Controls.Add(this.modelObjectPropertyGrid);
			this.propertyWindowModelTab.Location = new System.Drawing.Point(4, 22);
			this.propertyWindowModelTab.Name = "propertyWindowModelTab";
			this.propertyWindowModelTab.Padding = new System.Windows.Forms.Padding(3);
			this.propertyWindowModelTab.Size = new System.Drawing.Size(220, 339);
			this.propertyWindowModelTab.TabIndex = 1;
			this.propertyWindowModelTab.Text = "Model";
			this.propertyWindowModelTab.UseVisualStyleBackColor = true;
			// 
			// modelObjectPropertyGrid
			// 
			this.modelObjectPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.modelObjectPropertyGrid.Location = new System.Drawing.Point(3, 3);
			this.modelObjectPropertyGrid.Name = "modelObjectPropertyGrid";
			this.modelObjectPropertyGrid.Size = new System.Drawing.Size(214, 333);
			this.modelObjectPropertyGrid.TabIndex = 0;
			// 
			// layersTab
			// 
			this.layersTab.Controls.Add(this.layerEditorListView1);
			this.layersTab.Location = new System.Drawing.Point(4, 22);
			this.layersTab.Name = "layersTab";
			this.layersTab.Padding = new System.Windows.Forms.Padding(3);
			this.layersTab.Size = new System.Drawing.Size(220, 339);
			this.layersTab.TabIndex = 2;
			this.layersTab.Text = "Layers";
			this.layersTab.UseVisualStyleBackColor = true;
			// 
			// layerEditorListView1
			// 
			this.layerEditorListView1.AllowColumnReorder = true;
			this.layerEditorListView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layerEditorListView1.FullRowSelect = true;
			this.layerEditorListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.layerEditorListView1.HideSelection = false;
			this.layerEditorListView1.LabelEdit = true;
			this.layerEditorListView1.LabelWrap = false;
			this.layerEditorListView1.Location = new System.Drawing.Point(3, 3);
			this.layerEditorListView1.Name = "layerEditorListView1";
			this.layerEditorListView1.OwnerDraw = true;
			this.layerEditorListView1.ShowGroups = false;
			this.layerEditorListView1.Size = new System.Drawing.Size(214, 333);
			this.layerEditorListView1.TabIndex = 0;
			this.layerEditorListView1.UseCompatibleStateImageBehavior = false;
			this.layerEditorListView1.View = System.Windows.Forms.View.Details;
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
			this.project.Name = "";
			this.project.Repository = this.cachedRepository;
			defaultSecurity1.CurrentRole = Dataweb.nShape.StandardRole.Administrator;
			defaultSecurity1.CurrentRoleName = "Administrator";
			this.project.SecurityManager = defaultSecurity1;
			this.project.LibraryLoaded += new System.EventHandler<Dataweb.nShape.LibraryLoadedEventArgs>(this.project_LibraryLoaded);
			this.project.Opened += new System.EventHandler(this.project_Opened);
			this.project.Closed += new System.EventHandler(this.project_Closed);
			// 
			// cachedRepository
			// 
			this.cachedRepository.ProjectName = "";
			this.cachedRepository.Store = null;
			this.cachedRepository.Version = 0;
			// 
			// layerController
			// 
			this.layerController.DiagramSetController = this.diagramSetController;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter2.Location = new System.Drawing.Point(785, 0);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(3, 663);
			this.splitter2.TabIndex = 5;
			this.splitter2.TabStop = false;
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip);
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.AutoScroll = true;
			this.toolStripContainer1.ContentPanel.Controls.Add(this.displayTabControl);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.historyTrackBar);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.splitter2);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.toolboxPropsPanel);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.splitter1);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.modelTreeView);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1016, 663);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(1016, 734);
			this.toolStripContainer1.TabIndex = 8;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mainMenuStrip);
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.navigationToolStrip);
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.editToolStrip);
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.settingsToolStrip);
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.displayToolStrip);
			// 
			// displayTabControl
			// 
			this.displayTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.displayTabControl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.displayTabControl.Location = new System.Drawing.Point(153, 18);
			this.displayTabControl.Name = "displayTabControl";
			this.displayTabControl.SelectedIndex = 0;
			this.displayTabControl.Size = new System.Drawing.Size(632, 645);
			this.displayTabControl.TabIndex = 9;
			this.displayTabControl.SelectedIndexChanged += new System.EventHandler(this.displaysTabControl_SelectedIndexChanged);
			// 
			// historyTrackBar
			// 
			this.historyTrackBar.AutoSize = false;
			this.historyTrackBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.historyTrackBar.LargeChange = 1;
			this.historyTrackBar.Location = new System.Drawing.Point(153, 0);
			this.historyTrackBar.Name = "historyTrackBar";
			this.historyTrackBar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.historyTrackBar.Size = new System.Drawing.Size(632, 18);
			this.historyTrackBar.TabIndex = 7;
			this.historyTrackBar.ValueChanged += new System.EventHandler(this.historyTrackBar_ValueChanged);
			// 
			// modelTreeView
			// 
			this.modelTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.modelTreeView.FullRowSelect = true;
			this.modelTreeView.ImageIndex = 0;
			this.modelTreeView.Location = new System.Drawing.Point(0, 0);
			this.modelTreeView.Name = "modelTreeView";
			this.modelTreeView.SelectedImageIndex = 0;
			this.modelTreeView.Size = new System.Drawing.Size(150, 663);
			this.modelTreeView.TabIndex = 2;
			// 
			// navigationToolStrip
			// 
			this.navigationToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.navigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.prevDiagramButton,
            this.nextDiagramButton});
			this.navigationToolStrip.Location = new System.Drawing.Point(3, 24);
			this.navigationToolStrip.Name = "navigationToolStrip";
			this.navigationToolStrip.Size = new System.Drawing.Size(58, 25);
			this.navigationToolStrip.TabIndex = 9;
			// 
			// prevDiagramButton
			// 
			this.prevDiagramButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.prevDiagramButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.BackBtn;
			this.prevDiagramButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.prevDiagramButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.prevDiagramButton.Name = "prevDiagramButton";
			this.prevDiagramButton.Size = new System.Drawing.Size(23, 22);
			this.prevDiagramButton.Text = "Previous Diagram";
			this.prevDiagramButton.ToolTipText = "Back";
			this.prevDiagramButton.Click += new System.EventHandler(this.backButton_Click);
			// 
			// nextDiagramButton
			// 
			this.nextDiagramButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.nextDiagramButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.ForwardBtn;
			this.nextDiagramButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.nextDiagramButton.Name = "nextDiagramButton";
			this.nextDiagramButton.Size = new System.Drawing.Size(23, 22);
			this.nextDiagramButton.Text = "Next Diagram";
			this.nextDiagramButton.ToolTipText = "Forward";
			this.nextDiagramButton.Click += new System.EventHandler(this.forwardButton_Click);
			// 
			// editToolStrip
			// 
			this.editToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.editToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutShapeButton,
            this.copyShapeButton,
            this.pasteButton,
            this.deleteShapeButton,
            this.toolStripSeparator2,
            this.undoToolStripSplitButton,
            this.redoToolStripSplitButton,
            this.toolStripButton2});
			this.editToolStrip.Location = new System.Drawing.Point(61, 24);
			this.editToolStrip.Name = "editToolStrip";
			this.editToolStrip.Size = new System.Drawing.Size(197, 25);
			this.editToolStrip.TabIndex = 11;
			// 
			// cutShapeButton
			// 
			this.cutShapeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cutShapeButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.CutBtn;
			this.cutShapeButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.cutShapeButton.Name = "cutShapeButton";
			this.cutShapeButton.Size = new System.Drawing.Size(23, 22);
			this.cutShapeButton.Text = "Cut Shape";
			this.cutShapeButton.Click += new System.EventHandler(this.cutShapeOnlyItem_Click);
			// 
			// copyShapeButton
			// 
			this.copyShapeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.copyShapeButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.CopyBtn;
			this.copyShapeButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.copyShapeButton.Name = "copyShapeButton";
			this.copyShapeButton.Size = new System.Drawing.Size(23, 22);
			this.copyShapeButton.Text = "Copy Shape";
			this.copyShapeButton.Click += new System.EventHandler(this.copyShapeOnlyItem_Click);
			// 
			// pasteButton
			// 
			this.pasteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.pasteButton.Enabled = false;
			this.pasteButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.PasteBtn;
			this.pasteButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.pasteButton.Name = "pasteButton";
			this.pasteButton.Size = new System.Drawing.Size(23, 22);
			this.pasteButton.Text = "Paste";
			this.pasteButton.ToolTipText = "Paste";
			this.pasteButton.Click += new System.EventHandler(this.pasteMenuItem_Click);
			// 
			// deleteShapeButton
			// 
			this.deleteShapeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteShapeButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.DeleteBtn;
			this.deleteShapeButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.deleteShapeButton.Name = "deleteShapeButton";
			this.deleteShapeButton.Size = new System.Drawing.Size(23, 22);
			this.deleteShapeButton.Text = "Delete Shape";
			this.deleteShapeButton.Click += new System.EventHandler(this.deleteShapeOnlyItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// undoToolStripSplitButton
			// 
			this.undoToolStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.undoToolStripSplitButton.Enabled = false;
			this.undoToolStripSplitButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.UndoBtn;
			this.undoToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.undoToolStripSplitButton.Name = "undoToolStripSplitButton";
			this.undoToolStripSplitButton.Size = new System.Drawing.Size(32, 22);
			this.undoToolStripSplitButton.Text = "Undo";
			this.undoToolStripSplitButton.ButtonClick += new System.EventHandler(this.undoButton_Click);
			this.undoToolStripSplitButton.DropDownOpening += new System.EventHandler(this.undoToolStripSplitButton_DropDownOpening);
			// 
			// redoToolStripSplitButton
			// 
			this.redoToolStripSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.redoToolStripSplitButton.Enabled = false;
			this.redoToolStripSplitButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.RedoBtn;
			this.redoToolStripSplitButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.redoToolStripSplitButton.Name = "redoToolStripSplitButton";
			this.redoToolStripSplitButton.Size = new System.Drawing.Size(32, 22);
			this.redoToolStripSplitButton.Text = "Redo";
			this.redoToolStripSplitButton.ButtonClick += new System.EventHandler(this.redoButton_Click);
			this.redoToolStripSplitButton.DropDownOpening += new System.EventHandler(this.redoToolStripSplitButton_DropDownOpening);
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Image = global::Dataweb.nShape.Designer.Properties.Resources.DiagramPropertiesBtn3;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton2.Text = "Diagram Properties";
			this.toolStripButton2.Click += new System.EventHandler(this.showDiagramSettingsToolStripMenuItem_Click);
			// 
			// settingsToolStrip
			// 
			this.settingsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.settingsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomToolStripComboBox,
            this.refreshToolbarButton,
            this.showGridToolbarButton});
			this.settingsToolStrip.Location = new System.Drawing.Point(258, 24);
			this.settingsToolStrip.Name = "settingsToolStrip";
			this.settingsToolStrip.Size = new System.Drawing.Size(126, 25);
			this.settingsToolStrip.TabIndex = 13;
			// 
			// zoomToolStripComboBox
			// 
			this.zoomToolStripComboBox.AutoSize = false;
			this.zoomToolStripComboBox.DropDownWidth = 66;
			this.zoomToolStripComboBox.Items.AddRange(new object[] {
            "1000 %",
            "800 %",
            "600 %",
            "400 %",
            "300 %",
            "200 %",
            "175 %",
            "150 %",
            "125 %",
            "100 %",
            "90 %",
            "80 %",
            "70 %",
            "60 %",
            "50 %",
            "40 %",
            "30 %",
            "20 %",
            "10 %"});
			this.zoomToolStripComboBox.Name = "zoomToolStripComboBox";
			this.zoomToolStripComboBox.Size = new System.Drawing.Size(66, 21);
			this.zoomToolStripComboBox.Text = "100 %";
			this.zoomToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.zoomToolStripComboBox_SelectedIndexChanged);
			this.zoomToolStripComboBox.TextChanged += new System.EventHandler(this.toolStripComboBox1_TextChanged);
			// 
			// refreshToolbarButton
			// 
			this.refreshToolbarButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.refreshToolbarButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.RefreshBtn;
			this.refreshToolbarButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.refreshToolbarButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.refreshToolbarButton.Name = "refreshToolbarButton";
			this.refreshToolbarButton.Size = new System.Drawing.Size(23, 22);
			this.refreshToolbarButton.Text = "Refresh Display";
			this.refreshToolbarButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// showGridToolbarButton
			// 
			this.showGridToolbarButton.Checked = true;
			this.showGridToolbarButton.CheckOnClick = true;
			this.showGridToolbarButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showGridToolbarButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.showGridToolbarButton.Image = global::Dataweb.nShape.Designer.Properties.Resources.ToggleGridBtn2;
			this.showGridToolbarButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.showGridToolbarButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.showGridToolbarButton.Name = "showGridToolbarButton";
			this.showGridToolbarButton.Size = new System.Drawing.Size(23, 22);
			this.showGridToolbarButton.Text = "Show/Hide Gridlines";
			this.showGridToolbarButton.ToolTipText = "Show/Hide Gridlines";
			this.showGridToolbarButton.Click += new System.EventHandler(this.showGridToolStripMenuItem_Click);
			// 
			// displayToolStrip
			// 
			this.displayToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.displayToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.runtimeModeComboBox});
			this.displayToolStrip.Location = new System.Drawing.Point(384, 24);
			this.displayToolStrip.Name = "displayToolStrip";
			this.displayToolStrip.Size = new System.Drawing.Size(168, 25);
			this.displayToolStrip.TabIndex = 12;
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = global::Dataweb.nShape.Designer.Properties.Resources.PropertiesBtn;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Display Properties...";
			this.toolStripButton1.ToolTipText = "Display Properties...";
			this.toolStripButton1.Click += new System.EventHandler(this.showDisplaySettingsItem_Click);
			// 
			// runtimeModeComboBox
			// 
			this.runtimeModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.runtimeModeComboBox.DropDownWidth = 100;
			this.runtimeModeComboBox.Items.AddRange(new object[] {
            "Administrator",
            "Super User",
            "Designer",
            "Operator",
            "Guest"});
			this.runtimeModeComboBox.Name = "runtimeModeComboBox";
			this.runtimeModeComboBox.Size = new System.Drawing.Size(100, 25);
			this.runtimeModeComboBox.ToolTipText = "User Role";
			this.runtimeModeComboBox.SelectedIndexChanged += new System.EventHandler(this.runtimeModeButton_SelectedIndexChanged);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.CreatePrompt = true;
			this.saveFileDialog.Filter = "XML Repository Files|*.xml|All Files|*.*";
			// 
			// toolBoxController
			// 
			this.toolBoxController.DiagramSetController = this.diagramSetController;
			this.toolBoxController.LibraryManagerSelected += new System.EventHandler(this.toolBoxAdapter_ShowLibraryManagerDialog);
			this.toolBoxController.DesignEditorSelected += new System.EventHandler(this.toolBoxAdapter_ShowDesignEditor);
			this.toolBoxController.TemplateEditorSelected += new Dataweb.nShape.Controllers.TemplateEditorSelectedEventHandler(this.toolBoxAdapter_ShowTemplateEditorDialog);
			// 
			// modelTreeController
			// 
			this.modelTreeController.DiagramSetController = this.diagramSetController;
			// 
			// modelTreeAdapter
			// 
			this.modelTreeAdapter.ModelTreeController = this.modelTreeController;
			this.modelTreeAdapter.TreeView = this.modelTreeView;
			// 
			// propertyGridAdapter
			// 
			this.propertyGridAdapter.Controller = this.diagramSetController;
			this.propertyGridAdapter.PrimaryPropertyGrid = this.viewObjectPropertyGrid;
			this.propertyGridAdapter.SecondaryPropertyGrid = this.modelObjectPropertyGrid;
			// 
			// toolBoxListViewAdapter
			// 
			this.toolBoxListViewAdapter.ListView = this.listView;
			this.toolBoxListViewAdapter.ToolSetController = this.toolBoxController;
			// 
			// layerPresenter
			// 
			this.layerPresenter.Controller = this.layerController;
			this.layerPresenter.DiagramPresenter = null;
			this.layerPresenter.LayerView = this.layerEditorListView1;
			// 
			// DiagramDesignerMainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1016, 734);
			this.Controls.Add(this.toolStripContainer1);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.mainMenuStrip;
			this.Name = "nShapeDesignerMainForm";
			this.Text = "nShape Designer";
			this.Load += new System.EventHandler(this.nShapeDesignerMainForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.nShapeDesignerMainForm_FormClosing);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.toolboxPropsPanel.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.propertyWindowTabControl.ResumeLayout(false);
			this.propertyWindowShapeTab.ResumeLayout(false);
			this.propertyWindowModelTab.ResumeLayout(false);
			this.layersTab.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.historyTrackBar)).EndInit();
			this.navigationToolStrip.ResumeLayout(false);
			this.navigationToolStrip.PerformLayout();
			this.editToolStrip.ResumeLayout(false);
			this.editToolStrip.PerformLayout();
			this.settingsToolStrip.ResumeLayout(false);
			this.settingsToolStrip.PerformLayout();
			this.displayToolStrip.ResumeLayout(false);
			this.displayToolStrip.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel statusLabelPosition;
		private System.Windows.Forms.ToolStripStatusLabel statusLabelMessage;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quitMenuItem;
		private System.Windows.Forms.TreeView modelTreeView;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel toolboxPropsPanel;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem undoMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem showGridMenuItem;
		private System.Windows.Forms.ToolStrip navigationToolStrip;
		private System.Windows.Forms.ToolStripButton prevDiagramButton;
		private System.Windows.Forms.ToolStripButton nextDiagramButton;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.Windows.Forms.ToolStrip editToolStrip;
		private System.Windows.Forms.ToolStripButton pasteButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSplitButton undoToolStripSplitButton;
		private System.Windows.Forms.ToolStripSplitButton redoToolStripSplitButton;
		private System.Windows.Forms.TrackBar historyTrackBar;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem defaultToolStripMenuItem;
		private Dataweb.nShape.WinFormsUI.ModelTreeViewPresenter modelTreeAdapter;
		private System.Windows.Forms.TabControl propertyWindowTabControl;
		private System.Windows.Forms.TabPage propertyWindowShapeTab;
		private System.Windows.Forms.PropertyGrid viewObjectPropertyGrid;
		private System.Windows.Forms.TabPage propertyWindowModelTab;
		private System.Windows.Forms.PropertyGrid modelObjectPropertyGrid;
		private Dataweb.nShape.WinFormsUI.PropertyPresenter propertyGridAdapter;
		private Dataweb.nShape.Controllers.ToolSetController toolBoxController;
		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ToolStrip displayToolStrip;
		private System.Windows.Forms.ToolStripMenuItem openProjectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private Dataweb.nShape.Project project;
		private System.Windows.Forms.ToolStrip settingsToolStrip;
		private System.Windows.Forms.ToolStripComboBox zoomToolStripComboBox;
		private System.Windows.Forms.TabControl displayTabControl;
		private System.Windows.Forms.ToolStripMenuItem highQualityRenderingMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exportDiagramAsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pngExportMenuItem;
		private System.Windows.Forms.ToolStripMenuItem emfExportMenuItem;
		private System.Windows.Forms.ToolStripMenuItem jpgExportMenuItem;
		private System.Windows.Forms.ToolStripMenuItem bmpExportMenuItem;
		private System.Windows.Forms.ToolStripMenuItem wmfExportMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ManageShapeAndModelLibrariesMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
		private System.Windows.Forms.ToolStripMenuItem toForegroundMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toBackgroundMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripComboBox runtimeModeComboBox;
#if TdbRepository
		private Dataweb.nShape.TurboDBRepository turboDBRepository;
#endif

		private System.Windows.Forms.ToolStripMenuItem viewShowLayoutControlToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
		private System.Windows.Forms.ToolStripMenuItem newProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem insertDiagramMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteDiagramToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
		private System.Windows.Forms.ToolStripMenuItem editDesignsAndStylesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showDisplaySettingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cutShapeOnlyMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cutShapeAndModelMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyShapeOnlyMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyShapeAndModelMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteShapeOnlyMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteShapeAndModelMenuItem;
		private System.Windows.Forms.ToolStripButton cutShapeButton;
		private System.Windows.Forms.ToolStripButton copyShapeButton;
		private System.Windows.Forms.ToolStripButton deleteShapeButton;
		private Dataweb.nShape.WinFormsUI.ToolSetListViewPresenter toolBoxListViewAdapter;
		private System.Windows.Forms.ToolStripMenuItem projectInXMLFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem projectInSQLServerDatabaseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openXMLRepositoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openSQLServerRepositoryToolStripMenuItem;
		private System.Windows.Forms.TabPage layersTab;
		private Dataweb.nShape.WinFormsUI.LayerListView layerEditorListView1;
		private Dataweb.nShape.Controllers.LayerController layerController;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem adoNetDatabaseGeneratorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem recentProjectsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showDiagramSettingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripButton refreshToolbarButton;
		private System.Windows.Forms.ToolStripButton showGridToolbarButton;
		private Dataweb.nShape.Controllers.ModelController modelTreeController;
		private Dataweb.nShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.nShape.Advanced.CachedRepository cachedRepository;
		private Dataweb.nShape.Controllers.LayerPresenter layerPresenter;
		private System.Windows.Forms.ToolStripMenuItem graphicsFileToolStripMenuItem;
	}
}

