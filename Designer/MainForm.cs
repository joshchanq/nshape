using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Dataweb.Diagramming.Advanced;
using Dataweb.Diagramming.Controllers;
using Dataweb.Diagramming.WinFormsUI;


namespace Dataweb.Diagramming.Designer {

	public partial class DiagramDesignerMainForm : Form {

		public DiagramDesignerMainForm() {
			try {
				InitializeComponent();
				runtimeModeComboBox.SelectedIndex = 0;

				configFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\dataweb\\Diagramming Designer\\";
				if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
				configFile = "Config.cfg";

				historyTrackBar.Minimum = 0;
				historyTrackBar.Maximum = 0;
				historyTrackBar.Value = 0;
				historyTrackBar.TickStyle = System.Windows.Forms.TickStyle.BottomRight;
				historyTrackBar.TickFrequency = 1;
				currHistoryPos = historyTrackBar.Value;
				project.History.CommandAdded += history_CommandAdded;
				project.History.CommandExecuted += history_CommandExecuted;
				project.History.CommandsExecuted += history_CommandsExecuted;
				project.LibrarySearchPaths.Add(Application.StartupPath);

				ReadRecentProjectsFromConfigFile();
				CreateRecentProjectsMenuItems();

				XmlStore store = new XmlStore(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Diagramming Designer\\", ".xml");
				if (!Directory.Exists(store.DirectoryName))
					Directory.CreateDirectory(store.DirectoryName);
				CreateProject(newProjectName, store);

#if DEBUG
				// Shape libraries
				project.AddLibraryByFilePath("Dataweb.Diagramming.GeneralShapes.dll");
				//project.AddLibraryByFilePath("Diagramming.SoftwareArchitectureShapes.dll");
				//project.AddLibraryByFilePath("Diagramming.FlowChartShapes.dll");
				//project.AddLibraryByFilePath("Diagramming.ElectricalShapes.dll");
				// ModelObjectTypes libraries
				//project.AddLibraryByFilePath("Dataweb.Diagramming.GeneralModelObjects.dll");
#endif
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}


		#region Properties

		public bool ShowGrid {
			get { return showGrid; }
			set {
				showGrid = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ShowGrid = value;
				}
			}
		}


		public bool SnapToGrid {
			get { return snapToGrid; }
			set { 
				snapToGrid = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.SnapToGrid = value;
				}
			}
		}


		public int GridSize {
			get { return gridSize; }
			set {
				gridSize = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.GridSize = value;
				}
			}
		}


		public int SnapDistance {
			get { return snapDistance; }
			set {
				snapDistance = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.SnapDistance = value;
				}
			}
		}


		public ControlPointShape ResizePointShape {
			get { return resizePointShape; }
			set {
				resizePointShape = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ResizeGripShape = value;
				}
			}
		}


		public ControlPointShape ConnectionPointShape {
			get { return connectionPointShape; }
			set {
				connectionPointShape = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ConnectionPointShape = value;
				}
			}
		}


		public int ControlPointSize {
			get { return controlPointSize; }
			set {
				controlPointSize = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.GripSize = value;
				}
			}
		}


		public int Zoom {
			get { return zoom; }
			set {
				zoom = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ZoomLevel = value;
				}
			}
		}


		public bool HighQuality {
			get { return highQuality; }
			set {
				highQuality = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.HighQualityBackground = value;
					d.HighQualityRendering = value;
					d.Refresh();
				}
			}
		}

		#endregion


		#region ConfigFile, project and Cache

		private XmlReader OpenCfgReader(string filePath) {
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CloseInput = true;
			return XmlReader.Create(filePath, settings);
		}
		
		
		private XmlWriter OpenCfgWriter(string filePath) {
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.CloseOutput = true;
			settings.Indent = true;
			return XmlWriter.Create(filePath, settings);
		}
		
		
		private void CreateConfigFile(string filePath) {
			cfgWriter = OpenCfgWriter(filePath);
			cfgWriter.WriteStartDocument();
			cfgWriter.WriteStartElement(projectsTag);
			cfgWriter.WriteEndElement();
			cfgWriter.Close();
		}


		private void ReadRecentProjectsFromConfigFile() {
			if (!File.Exists(configFolder + configFile))
				CreateConfigFile(configFolder + configFile);

			cfgReader = OpenCfgReader(configFolder + configFile);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(cfgReader);
			cfgReader.Close();

			recentProjects = new List<RepositoryInfo>();
			foreach (XmlNode xmlNode in xmlDoc.GetElementsByTagName(projectsTag)) {
				foreach (XmlNode childNode in xmlNode.ChildNodes) {
					RepositoryInfo repositoryInfo = RepositoryInfo.Empty;
					foreach (XmlAttribute attr in childNode.Attributes) {
						if (attr.Name.Equals(projectNameTag, StringComparison.InvariantCultureIgnoreCase)) {
							repositoryInfo.projectName = attr.Value;
						} else if (attr.Name.Equals(typeNameTag, StringComparison.InvariantCultureIgnoreCase)) {
							repositoryInfo.typeName = attr.Value;
						} else if (attr.Name.Equals(serverNameTag, StringComparison.InvariantCultureIgnoreCase)) {
							repositoryInfo.computerName = attr.Value;
						} else if (attr.Name.Equals(dataSourceTag, StringComparison.InvariantCultureIgnoreCase)) {
							repositoryInfo.location = attr.Value;
						}
					}
					if (repositoryInfo != RepositoryInfo.Empty 
						&& (repositoryInfo.typeName == xmlStoreTypeName || repositoryInfo.typeName == sqlServerStoreTypeName))
						recentProjects.Add(repositoryInfo);
				}
			}
		}


		// Appends the current project to the recent project XML file
		private void SaveRecentProjectsToConfigFile() {
			// delete existing config file
			if (File.Exists(configFolder + configFile))
				File.Delete(configFolder + configFile);

			// Create a new config file
			CreateConfigFile(configFolder + configFile);
			cfgReader = OpenCfgReader(configFolder + configFile);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(cfgReader);
			cfgReader.Close();

			// Find the "Projects" node
			XmlNode repositoriesNode = null;
			foreach (XmlNode xmlNode in xmlDoc.GetElementsByTagName(projectsTag)) {
				repositoriesNode = xmlNode;
				break;
			}
			Debug.Assert(repositoriesNode != null);

			// Save all recent projects
			foreach (RepositoryInfo projectInfo in recentProjects) {
				XmlNode currentRepositoryNode = null;
				foreach (XmlNode childNode in repositoriesNode.ChildNodes) {
					foreach (XmlAttribute attribute in childNode.Attributes) {
						if (attribute.Name.Equals(projectNameTag, StringComparison.InvariantCultureIgnoreCase)) {
							if (attribute.Value.Equals(projectInfo.projectName, StringComparison.InvariantCultureIgnoreCase)) {
								currentRepositoryNode = childNode;
								break;
							}
						}
					}
					if (currentRepositoryNode != null) break;
				}
				if (currentRepositoryNode != null)
					repositoriesNode.RemoveChild(currentRepositoryNode);

				if (repositoriesNode.ChildNodes.Count >= 15)
					repositoriesNode.RemoveChild(repositoriesNode.FirstChild);

				XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, projectTag, xmlDoc.NamespaceURI);
				newNode.Attributes.Append(xmlDoc.CreateAttribute(projectNameTag)).Value = projectInfo.projectName;
				newNode.Attributes.Append(xmlDoc.CreateAttribute(typeNameTag)).Value = projectInfo.typeName;
				newNode.Attributes.Append(xmlDoc.CreateAttribute(serverNameTag)).Value = projectInfo.computerName;
				newNode.Attributes.Append(xmlDoc.CreateAttribute(dataSourceTag)).Value = projectInfo.location;
				repositoriesNode.AppendChild(newNode);

				cfgWriter = OpenCfgWriter(configFolder + configFile);
				xmlDoc.Save(cfgWriter);
				cfgWriter.Close();
			}
		}


		private void ReplaceRepository(string projectName, Store repository) {
			UnregisterRepositoryEvents();
			project.Name = projectName;
			((CachedRepository)project.Repository).Store = repository;
			RegisterRepositoryEvents();
		}


		private void PrependRecentProjectsMenuItem(RepositoryInfo projectInfo) {
			ToolStripItem item = new ToolStripMenuItem(projectInfo.projectName);
			item.Tag = projectInfo;
			item.ToolTipText = string.Format("ProjectName: {0}\nLocation: {1} \nRepository Type: {2}", projectInfo.projectName, projectInfo.location, projectInfo.typeName);
			item.Click += openRecentProjectMenuItem_Click;
			recentProjectsMenuItem.DropDownItems.Insert(0, item);
			if (!recentProjectsMenuItem.Visible) recentProjectsMenuItem.Visible = true;
		}


		private void CreateRecentProjectsMenuItems() {
			recentProjectsMenuItem.Visible = (recentProjects.Count > 0);
			for (int i = 0; i < recentProjects.Count; ++i)
				PrependRecentProjectsMenuItem(recentProjects[i]);
		}


		private RepositoryInfo GetReposistoryInfo(Project project) {
			RepositoryInfo projectInfo = RepositoryInfo.Empty;
			projectInfo.projectName = project.Name;
			Store store = ((CachedRepository)project.Repository).Store;
			if (store is XmlStore) {
				projectInfo.typeName = xmlStoreTypeName;
				string filePath = ((XmlStore)store).ProjectFilePath;
				projectInfo.location = filePath;
				projectInfo.computerName = Environment.MachineName;
			} else if (store is SqlStore) {
				projectInfo.typeName = sqlServerStoreTypeName;
				projectInfo.location = ((SqlStore)store).ConnectionString;
				projectInfo.computerName = ((SqlStore)store).ServerName;
			} else Debug.Fail("Unexpected repository type");
#if TdbRepository
			else if (project.Repository is TurboDBRepository) {
				repositoryInfo.dataSource = ((TurboDBRepository)project.Repository).ConnectionString;
				repositoryInfo.serverName = ((TurboDBRepository)project.Repository).ServerName;
			}
#endif
			return projectInfo;
		}


		private bool AddToRecentProjects(Project project) {
			return AddToRecentProjects(GetReposistoryInfo(project));
		}


		private bool AddToRecentProjects(RepositoryInfo projectInfo) {
			// Check if the project already exists in the recent projects list
			for (int i = recentProjects.Count - 1; i >= 0; --i)
				if (recentProjects[i] == projectInfo) return false;
			// If it does not, add it and create a new menu item
			recentProjects.Add(projectInfo);
			PrependRecentProjectsMenuItem(projectInfo);
			SaveRecentProjectsToConfigFile();
			return true;
		}


		private bool RemoveFromRecentProjects(Project project) {
			return RemoveFromRecentProjects(GetReposistoryInfo(project));
		}


		private bool RemoveFromRecentProjects(RepositoryInfo projectInfo) {
			return recentProjects.Remove(projectInfo); 
		}


		private void UpdateRecentProjectsMenu() {
			recentProjectsMenuItem.DropDownItems.Clear();
			foreach (RepositoryInfo pi in recentProjects)
				PrependRecentProjectsMenuItem(pi);
		}


		private void CreateProject(string projectName, Store store) {
			ReplaceRepository(projectName, store);
			project.Create();
			DisplayDiagrams();
			// Adjust menu items
			saveMenuItem.Enabled = true;
			saveAsMenuItem.Enabled = true;
		}


		private void OpenProject(string projectName, Store repository) {
			try {
				ReplaceRepository(projectName, repository);
				project.Open();
				DisplayDiagrams();
				AddToRecentProjects(project);
				UpdateRecentProjectsMenu();
			} catch (Exception exc) {
				MessageBox.Show(exc.Message, "Error while opening Repository.");
			}
		}


		private bool CloseProject() {
			bool result = true;
			if (project.Repository.IsModified) {
				DialogResult dlgResult = MessageBox.Show("Do you want to save your project before closing it?", "Save changes", MessageBoxButtons.YesNoCancel);
				switch (dlgResult) {
					case DialogResult.Yes:
						project.Repository.SaveChanges();
						break;
					case DialogResult.No:
						// do nothing
						break;
					case DialogResult.Cancel:
						result = false;
						break;
				}
			}

			if (result) {
				if (project.Repository != null)
					project.Close();

				// clear all displays and diagramControllers
				for (int i = displayTabControl.TabPages.Count - 1; i >= 0; --i) {
					if (displayTabControl.TabPages[i].Controls[0] is Display) {
						Display display = (Display)displayTabControl.TabPages[i].Controls[0];
						displayTabControl.TabPages.RemoveAt(i);

						display.MouseUp -= display_MouseUp;
						display.MouseMove -= display_MouseMove;
						display.ShapesSelected -= display_ShapesSelected;
						display.Clear();
						display.Dispose();
						display = null;
					}
				}
			}

			return result;
		}

		#endregion


		private void DisplayMouseCoordinates(int x, int y) {
			statusLabelPosition.Text = "X: " + x.ToString() + ", Y: " + y.ToString();
		}


		private void UpdateToolBarAndMenuItems() {
			bool shapesOnly, shapesAndModels;
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null) {
				// cut
				shapesOnly = shapesAndModels = CurrentDisplay.DiagramSetController.CanCut(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes);
				cutShapeButton.Enabled = shapesOnly;
				cutShapeOnlyMenuItem.Enabled = shapesOnly;
				cutShapeAndModelMenuItem.Enabled = shapesAndModels;

				// copy
				shapesOnly =
				shapesAndModels = CurrentDisplay.DiagramSetController.CanCopy(CurrentDisplay.SelectedShapes);
				cutShapeButton.Enabled = shapesOnly;
				copyShapeOnlyMenuItem.Enabled = shapesOnly;
				copyShapeAndModelMenuItem.Enabled = shapesAndModels;

				// paste
				pasteButton.Enabled =
				pasteMenuItem.Enabled = CurrentDisplay.DiagramSetController.CanPaste(CurrentDisplay.Diagram);

				// delete
				shapesOnly =
				shapesAndModels = CurrentDisplay.DiagramSetController.CanDeleteShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes);
				deleteShapeButton.Enabled = shapesOnly;
				deleteShapeOnlyMenuItem.Enabled = shapesOnly;
				deleteShapeAndModelMenuItem.Enabled = shapesAndModels;

				// toForeGround / toBackground
				toForegroundMenuItem.Enabled =
				toBackgroundMenuItem.Enabled = CurrentDisplay.DiagramSetController.CanLiftShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes);
			} else {
				// cut
				cutShapeButton.Enabled =
				cutShapeOnlyMenuItem.Enabled =
				cutShapeAndModelMenuItem.Enabled = false;
				// copy
				shapesOnly =
				shapesAndModels =
				cutShapeButton.Enabled =
				copyShapeOnlyMenuItem.Enabled =
				copyShapeAndModelMenuItem.Enabled = false;
				// paste
				pasteButton.Enabled =
				pasteMenuItem.Enabled = false;
				// delete
				shapesOnly =
				shapesAndModels =
				deleteShapeButton.Enabled =
				deleteShapeOnlyMenuItem.Enabled =
				deleteShapeAndModelMenuItem.Enabled = false;
				// toForeGround / toBackground
				toForegroundMenuItem.Enabled =
				toBackgroundMenuItem.Enabled = false;
			}
			// undo / redo
			undoToolStripSplitButton.Enabled =
			undoMenuItem.Enabled = diagramSetController.Project.History.UndoCommandCount > 0;
			redoToolStripSplitButton.Enabled =
			redoMenuItem.Enabled = diagramSetController.Project.History.RedoCommandCount > 0;
		}


		private void CreateDisplayForDiagram(Diagram diagram) {
			if (FindDisplayTabPage(diagram) == null) {
				// Create a new ownerDisplay
				Display display = new Display();
				display.BackColor = Color.DarkGray;
				display.HighQualityRendering = HighQuality;
				display.HighQualityBackground = HighQuality;
				display.ShowGrid = ShowGrid;
				display.GridSize = GridSize;
				display.SnapToGrid = SnapToGrid;
				display.SnapDistance = SnapDistance;
				display.GripSize = ControlPointSize;
				display.ResizeGripShape = ResizePointShape;
				display.ConnectionPointShape = ConnectionPointShape;
				display.ZoomLevel = Zoom;
				display.Dock = DockStyle.Fill;
				//
				// Assign DiagramSetController and diagram
				display.DiagramSetController = diagramSetController;
				display.Diagram = diagram;
				//
				// Create and add a new tabPage for the display
				TabPage tabPage = new TabPage(diagram.Name);
				tabPage.Controls.Add(display);
				displayTabControl.TabPages.Add(tabPage);
				displayTabControl.SelectedTab = tabPage;
				displaysTabControl_SelectedIndexChanged(displayTabControl, new EventArgs());
			}
			UpdateToolBarAndMenuItems();
		}


		private void RemoveDisplayOfDiagram(Diagram diagram) {
			TabPage tabPage = FindDisplayTabPage(diagram);
			if (tabPage != null) {
				foreach (Control ctrl in tabPage.Controls) {
					if (ctrl is Display) {
						Display d = (Display)ctrl;

						displayTabControl.TabPages.Remove(tabPage);
						d.Clear();

						d.Dispose();
						tabPage.Dispose();
					}
				}
			}
			UpdateToolBarAndMenuItems();
		}


		private TabPage FindDisplayTabPage(Diagram diagram) {
			foreach (TabPage tabPage in displayTabControl.TabPages) {
				foreach (Control ctrl in tabPage.Controls) {
					if (ctrl is Display) {
						Display d = (Display)ctrl;
						if (d.Diagram == diagram) return tabPage;
					}
				}
			}
			return null;
		}


		/// <summary>
		/// Creates a ownerDisplay for each diagram in the project and a default one if there isn'db any.
		/// </summary>
		private void DisplayDiagrams() {
			// Display all diagramControllers of the project
			bool diagramAdded = false;
			foreach (Diagram diagram in project.Repository.GetDiagrams()) {
				project.Repository.GetDiagramShapes(diagram);
				CreateDisplayForDiagram(diagram);
				diagramAdded = true;
			}
			// If the project has no ownerDisplay, create the default one.
			if (!diagramAdded)
				newDiagramToolStripMenuItem_Click(this, new EventArgs());
		}


		private Display CurrentDisplay {
			get { return currentDisplay; }
			set {
				if (currentDisplay != null) {
					currentDisplay.MouseUp -= display_MouseUp;
					currentDisplay.MouseMove -= display_MouseMove;
					currentDisplay.ShapesSelected -= display_ShapesSelected;
					currentDisplay.ZoomChanged -= display_ZoomChanged;
				}
				currentDisplay = value;
				if (currentDisplay != null) {
					currentDisplay.MouseUp += display_MouseUp;
					currentDisplay.MouseMove += display_MouseMove;
					currentDisplay.ShapesSelected += display_ShapesSelected;
					currentDisplay.ZoomChanged += display_ZoomChanged;

					currentDisplay.HighQualityRendering = HighQuality;
					currentDisplay.ShowGrid = ShowGrid;
					currentDisplay.HighQualityBackground = HighQuality;
					currentDisplay.ZoomLevel = Zoom;
					currentDisplay.CurrentTool = toolBoxController.SelectedTool;

					display_ShapesSelected(currentDisplay, null);
				}
			}
		}


		// Returns false, if the save should be retried with another project projectName.
		private bool SaveProject() {
			bool result;
			try {
				project.Repository.SaveChanges();
				RepositoryInfo projectInfo = GetReposistoryInfo(project);
				RemoveFromRecentProjects(projectInfo);
				AddToRecentProjects(projectInfo);
				UpdateRecentProjectsMenu();
				result = true;
			} catch (IOException exc) {
				MessageBox.Show(exc.Message);
				result = false;
			} catch (Exception exc) {
				MessageBox.Show(exc.Message);
				result = true;
			}
			return result;
		}


		// Returns false, if the save should be retried with another project entityTypeName.
		private bool SaveProjectAs() {
			bool result;
			if (project.Repository.Exists()) {
				if (MessageBox.Show(string.Format("The repository already contains a project named '{0}'. Overwrite?", project.Name),
					"Saving Project", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
					project.Repository.Erase();
					result = SaveProject();
				} else result = false;
			} else 
				result = SaveProject();
			return result;
		}


		#region *** Register event handlers ***

		private void RegisterRepositoryEvents() {
			if (project.Repository != null) {
				project.Repository.DiagramInserted += repository_DiagramInserted;
				project.Repository.DiagramDeleted += repository_DiagramDeleted;
				project.Repository.ModelObjectsInserted += Repository_ModelObjectsInsertedOrDeleted;
				project.Repository.ModelObjectsDeleted += Repository_ModelObjectsInsertedOrDeleted;
			}
		}


		private void UnregisterRepositoryEvents() {
			if (project.Repository != null) {
				project.Repository.DiagramInserted -= repository_DiagramInserted;
				project.Repository.DiagramDeleted -= repository_DiagramDeleted;
				project.Repository.ModelObjectsInserted -= Repository_ModelObjectsInsertedOrDeleted;
				project.Repository.ModelObjectsDeleted -= Repository_ModelObjectsInsertedOrDeleted;
			}
		}

		#endregion


		#region *** Project and Cache event handler implementations ***

		private void project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			//bool showModelTree = (project.ModelObjectTypes.ModelObjectTypeCount > 0);
			//if (modelTreeView.Visible != showModelTree) {
			//   if (!modelTreeView.Visible) {
			//      modelTreeController.project = project;
			//   }
			//}
		}


		private void project_Opened(object sender, EventArgs e) {
			RegisterRepositoryEvents();
			// Set main form title
			Text = appTitle + " - " + project.Name;
			// Hide/Show ModelTreeView
			modelTreeView.SuspendLayout();
			modelTreeView.Visible = false;
			foreach (IModelObject m in project.Repository.GetModelObjects(null)) {
				modelTreeView.Visible = true;
				break;
			}
			modelTreeView.ResumeLayout();
		}


		private void project_Closed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
			Text = appTitle;
		}


		private void history_CommandAdded(object sender, CommandEventArgs e) {
			undoToolStripSplitButton.Enabled = true;
			if (CurrentDisplay != null) {
				if (project.History.UndoCommandCount + project.History.RedoCommandCount != historyTrackBar.Maximum)
					historyTrackBar.Maximum = project.History.UndoCommandCount + project.History.RedoCommandCount;
				currHistoryPos = 0;
				historyTrackBar.Value = 0;
			}
		}


		private void history_CommandExecuted(object sender, CommandEventArgs e) {
			undoToolStripSplitButton.Enabled = project.History.UndoCommandCount > 0;
			redoToolStripSplitButton.Enabled = project.History.RedoCommandCount > 0;
			try {
				historyTrackBar.ValueChanged -= historyTrackBar_ValueChanged;
				if (e.Reverted) {
					++currHistoryPos;
					++historyTrackBar.Value;
				} else {
					--currHistoryPos;
					--historyTrackBar.Value;
				}
			} finally {
				historyTrackBar.ValueChanged += historyTrackBar_ValueChanged;
			}
		}


		private void history_CommandsExecuted(object sender, CommandsEventArgs e) {
			undoToolStripSplitButton.Enabled = project.History.UndoCommandCount > 0;
			redoToolStripSplitButton.Enabled = project.History.RedoCommandCount > 0;
			try {
				historyTrackBar.ValueChanged -= historyTrackBar_ValueChanged;
				if (e.Reverted) {
					if (currHistoryPos != historyTrackBar.Value) {
						currHistoryPos += e.Commands.Count;
						historyTrackBar.Value += e.Commands.Count;
					}
				} else {
					currHistoryPos -= e.Commands.Count;
					historyTrackBar.Value -= e.Commands.Count;
				}
			} finally {
				historyTrackBar.ValueChanged += historyTrackBar_ValueChanged;
			}
		}


		private void repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			RemoveDisplayOfDiagram(e.Diagram);
		}


		private void repository_DiagramInserted(object sender, RepositoryDiagramEventArgs e) {
			CreateDisplayForDiagram(e.Diagram);
			UpdateToolBarAndMenuItems();
		}

		
		private void Repository_ModelObjectsInsertedOrDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			bool modelExists = false;
			foreach (IModelObject modelObject in project.Repository.GetModelObjects(null)) {
				modelExists = true;
				break;
			}
			if (modelTreeView.Visible != modelExists) 
				modelTreeView.Visible = modelExists;
		}

		#endregion


		#region *** Display event handler implementations ***

		private void display_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
			int x = e.X;
			int y = e.Y;
			if (CurrentDisplay != null)
				CurrentDisplay.ControlToDiagram(e.X, e.Y, out x, out y);
			DisplayMouseCoordinates(x, y);
		}


		private void display_DragOver(object sender, System.Windows.Forms.DragEventArgs e) {
			p.X = e.X; p.Y = e.Y;
			if (CurrentDisplay != null)
				p = CurrentDisplay.PointToClient(p);
			DisplayMouseCoordinates(p.X, p.Y);
		}


		private void display_ShapesSelected(object sender, EventArgs e) {
			if (sender.Equals(CurrentDisplay)) {
				int cnt = CurrentDisplay.SelectedShapes.Count;
				if (cnt > 0) statusLabelMessage.Text = string.Format("{0} shape{1} selected.", cnt, cnt > 1 ? "s" : "");
				else statusLabelMessage.Text = "";
				UpdateToolBarAndMenuItems();
				//
				if (layoutControlForm != null)
					layoutControlForm.SelectedShapes = CurrentDisplay.SelectedShapes;
			}
		}


		private void display_MouseUp(object sender, MouseEventArgs e) {
		}


		private void display_ZoomChanged(object sender, EventArgs e) {
			string txt = string.Format("{0} %", currentDisplay.ZoomLevel);
			if (txt != zoomToolStripComboBox.Text)
				zoomToolStripComboBox.Text = currentDisplay.ZoomLevel.ToString();
		}

		#endregion


		#region *** ModelTree event handler implementations ***

		private void modelTree_ModelObjectSelected(object sender, ModelObjectSelectedEventArgs eventArgs) {
			this.propertyWindowTabControl.SelectedTab = this.propertyWindowModelTab;
		}

		#endregion


		#region *** ToolBox event handler implementations ***

		private void toolBoxAdapter_ShowDesignEditorDialog(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}


		private void toolBoxAdapter_ShowTemplateEditorDialog(object sender, ShowTemplateEditorEventArgs e) {
			// Möglichkeit 1:
			//templateEditorDialog = new TemplateEditorDialog(e.Project, e.Template);
			//templateEditorDialog.Show(this);

			// Möglichkeit 2:
			templateEditorDialog = new TemplateEditorDialog();
			templateEditorDialog.Project = e.Project;
			templateEditorDialog.Template = e.Template;
			templateEditorDialog.Show(this);

			//// Möglichkeit 3:
			//TemplateEditorDialog dlg = new TemplateEditorDialog();
			//dlg.TemplateEditor = new TemplateEditor();
			//dlg.TemplateEditor.Initialize(e.DiagramSetController, e.Template);
			//dlg.Show(this);
		}


		private void toolBoxAdapter_ShowLibraryManagerDialog(object sender, EventArgs e) {
			LibraryManagementDialog dlg = new LibraryManagementDialog(project);
			dlg.Show(this);
		}


		private void toolBoxAdapter_ToolSelected(object sender, EventArgs e) {
			// ToDo:
			// If the ownerDisplay and the ToolBox are not connected directly, one can handle this event and 
			// assign the SelectedTool as the DIsplay's CurrentTool
		}


		private void toolBoxAdapter_ShowDesignEditor(object sender, System.EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}

		#endregion


		#region Event handler implementations

		private void DiagrammingDemoFrm_Load(object sender, EventArgs e) {
			UpdateToolBarAndMenuItems();
		}


		private void displaysTabControl_SelectedIndexChanged(object sender, EventArgs e) {
			TabPage tab = displayTabControl.SelectedTab;
			if (tab != null && tab.Controls.Count > 0) {
				CurrentDisplay = (Display)tab.Controls[0];

				layerPresenter.DiagramPresenter = CurrentDisplay;

				UpdateToolBarAndMenuItems();
			}
		}


		private void DiagramDesignerMainForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (!CloseProject())
				e.Cancel = true;
			else SaveRecentProjectsToConfigFile();
		}


		private void LayoutControlForm_LayoutChanged(object sender, EventArgs e) {
			// Why? Makes preview switch between original and new State.
			// currentDisplay.SaveChanges();
		}


		private void LayoutControlForm_FormClosed(object sender, FormClosedEventArgs e) {
			layoutControlForm = null;
		}

		#endregion


		#region Toolbar event handler implementations

		private void refreshButton_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null)
				CurrentDisplay.Refresh();
		}


		private void forwardButton_Click(object sender, EventArgs e) {
			if (displayTabControl.SelectedIndex < displayTabControl.TabPages.Count - 1)
				++displayTabControl.SelectedIndex;
		}


		private void backButton_Click(object sender, EventArgs e) {
			if (displayTabControl.SelectedIndex > 0)
				displayTabControl.SelectedIndex--;
		}


		private void undoToolStripSplitButton_DropDownOpening(object sender, EventArgs e) {
			undoToolStripSplitButton.DropDownItems.Clear();
			if (CurrentDisplay != null) {
				int nr = 0;
				foreach (string cmdDesc in project.History.GetUndoCommandDescriptions(historyDropDownItemCount)) {
					System.Windows.Forms.ToolStripItem item = new System.Windows.Forms.ToolStripMenuItem();
					item.Text = string.Format("{0}: {1}", ++nr, cmdDesc);
					item.Click += undoItem_Click;
					undoToolStripSplitButton.DropDownItems.Add(item);
				}
			}
		}


		private void redoToolStripSplitButton_DropDownOpening(object sender, EventArgs e) {
			redoToolStripSplitButton.DropDownItems.Clear();
			if (CurrentDisplay != null) {
				int nr = 0;
				foreach (string cmdDesc in project.History.GetRedoCommandDescriptions(historyDropDownItemCount)) {
					System.Windows.Forms.ToolStripItem item = new System.Windows.Forms.ToolStripMenuItem();
					item.Text = string.Format("{0}: {1}", ++nr, cmdDesc);
					item.Click += redoItem_Click;
					redoToolStripSplitButton.DropDownItems.Add(item);
				}
			}
		}


		private void zoomToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			int zoom;
			if (int.TryParse(zoomToolStripComboBox.Text.Replace('%', ' ').Trim(), out zoom)) {
				if (Zoom != zoom) Zoom = zoom;
			}
		}


		private void toolStripComboBox1_TextChanged(object sender, EventArgs e) {			
			int zoom;
			if (!zoomToolStripComboBox.Text.Contains("%")) {
				int.TryParse(zoomToolStripComboBox.Text.Trim(), out zoom);
				zoomToolStripComboBox.Text = string.Format("{0} %", zoom);
			}
			else {
				if (int.TryParse(zoomToolStripComboBox.Text.Replace('%', ' ').Trim(), out zoom)) {
					if (zoom > 0 && Zoom != zoom) Zoom = zoom;
				}
			}
		}


		private void runtimeModeButton_SelectedIndexChanged(object sender, EventArgs e) {
			((DefaultSecurity)project.SecurityManager).CurrentRoleName = runtimeModeComboBox.Text;
		}

		#endregion


		#region "File" menu event handler implementations

		private void newXMLRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			if (saveFileDialog.ShowDialog() == DialogResult.OK && CloseProject()) {
				XmlStore repository = new XmlStore(Path.GetDirectoryName(saveFileDialog.FileName), ".xml");
				CreateProject(Path.GetFileNameWithoutExtension(saveFileDialog.FileName), repository);
			}
		}


		private void newSQLServerRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			using (OpenAdoNetRepositoryDialog dlg = new OpenAdoNetRepositoryDialog()) {
				if (dlg.ShowDialog() == DialogResult.OK && CloseProject()) {
					SqlStore repository = new SqlStore();
					repository.ServerName = ".\\SQLEXPRESS";
					repository.DatabaseName = "TurboDiagram";
					CreateProject(dlg.ProjectName, repository);
				}
			}
		}


		private void openXMLRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.Filter = fileFilterXmlRepository;
			if (openFileDialog.ShowDialog() == DialogResult.OK && CloseProject()) {
				XmlStore repository = new XmlStore(Path.GetDirectoryName(openFileDialog.FileName), ".xml");
				OpenProject(Path.GetFileNameWithoutExtension(openFileDialog.FileName), repository);
			}
		}


		private void openSQLServerRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			using (OpenAdoNetRepositoryDialog dlg = new OpenAdoNetRepositoryDialog()) {
				if (dlg.ShowDialog() == DialogResult.OK && CloseProject()) {
					SqlStore repository = new SqlStore();
					repository.ServerName = ".\\SQLEXPRESS";
					repository.DatabaseName = "TurboDiagram";
					OpenProject(dlg.ProjectName, repository);
				}
			}
		}


		private void openRecentProjectMenuItem_Click(object sender, EventArgs e) {
			Debug.Assert(sender is ToolStripItem && ((ToolStripItem)sender).Tag is RepositoryInfo);
			RepositoryInfo repositoryInfo = (RepositoryInfo)((ToolStripItem)sender).Tag;
			if (CloseProject()) {
				Store store = null;
				if (repositoryInfo.typeName == xmlStoreTypeName) {
					store = new XmlStore(Path.GetDirectoryName(repositoryInfo.location), ".xml");
				} else if (repositoryInfo.typeName == sqlServerStoreTypeName) {
					store = new SqlStore();
					((SqlStore)store).DatabaseName = repositoryInfo.location;
					((SqlStore)store).ServerName = repositoryInfo.computerName;
				}
#if TdbRepository 
			else if (projectInfo.typeName == typeof(TurboDBRepository).Name) {
				repository = new TurboDBRepository();
				((TurboDBRepository)repository).DataSource = projectInfo.dataSource;
				((TurboDBRepository)repository).ServerName = projectInfo.serverName;
			} 
#endif
 else MessageBox.Show("Unknown repository type in recent list.");
				if (store != null) OpenProject(repositoryInfo.projectName, store);
			}
		}


		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
			if (((CachedRepository)project.Repository).Store is XmlStore) {
				XmlStore xmlStore = (XmlStore)((CachedRepository)project.Repository).Store;
				saveFileDialog.Filter = fileFilterXmlRepository;
				saveFileDialog.FileName = xmlStore.ProjectFilePath;
				bool retry = true;
				while (retry && saveFileDialog.ShowDialog() == DialogResult.OK) {
					xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
					project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
					saveMenuItem.Enabled = true;
					retry = !SaveProjectAs();
				}
			}
#if TdbRepository
			else if (project.Repository is TurboDBRepository) {
				TurboDBRepository turboDBRepository = (TurboDBRepository)project.Repository;
				saveFileDialog.Filter = fileFilterTurboDBRepository;
				saveFileDialog.FileName = xmlRepository.FileName;
				if (saveFileDialog.ShowDialog() == DialogResult.OK) {
					TurboDBRepository repository = (TurboDBRepository)project.Repository;
					repository.ConnectionString = string.Format("DataSource={0}", saveFileDialog.FileName);
					saveMenuItem.Enabled = true;
					SaveProjectAs();
				}
			}
#endif
		}


		private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
			if (project.Name == newProjectName)
				saveAsToolStripMenuItem_Click(sender, e);
			else SaveProject();
		}


		private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e) {
			CloseProject();
		}


		private void ManageShapeAndModelLibrariesMenuItem_Click(object sender, EventArgs e) {
			if (libDialog == null)
				libDialog = new LibraryManagementDialog(project);
			libDialog.ShowDialog();
			libDialog.Dispose();
			libDialog = null;
		}


		private void exportDiagramAsMenuItem_Click(object sender, EventArgs e) {

		}


		private void emfPlusFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportMetaFile(DiagrammingImageFormat.EmfPlus);
		}


		private void emfOnlyFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportMetaFile(DiagrammingImageFormat.Emf);
		}


		private void pngFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(DiagrammingImageFormat.Png);
		}


		private void jpgFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(DiagrammingImageFormat.Jpeg);
		}


		private void bmpFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(DiagrammingImageFormat.Bmp);
		}


		private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}


		private Image GetImageFromDiagram(DiagrammingImageFormat imageFormat) {
			Image result = null;
			Color backColor = Color.Transparent;
			if (CurrentDisplay.SelectedShapes.Count > 0)
				result = CurrentDisplay.Diagram.CreateImage(imageFormat, CurrentDisplay.SelectedShapes.BottomUp, CurrentDisplay.GridSize, false, backColor);
			else
				result = CurrentDisplay.Diagram.CreateImage(imageFormat, null, 0, true, backColor);
			return result;
		}
		
		
		private void ExportMetaFile(DiagrammingImageFormat imageFormat) {
			saveFileDialog.Filter = "Enhanced Meta Files|*.emf|All Files|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				using (Image image = GetImageFromDiagram(imageFormat)) {
					if (image != null) GdiHelpers.SaveImageToFile(image, saveFileDialog.FileName, imageFormat);
				}
			}
		}


		private void ExportBitmapFile(DiagrammingImageFormat imageFormat) {
			string fileFilter = null;
			switch (imageFormat) {
				case DiagrammingImageFormat.Bmp: fileFilter = "Bitmap Picture Files|*.bmp|All Files|*.*"; break;
				case DiagrammingImageFormat.Gif: fileFilter = "Graphics Interchange Format Files|*.gif|All Files|*.*"; break;
				case DiagrammingImageFormat.Jpeg: fileFilter = "Joint Photographic Experts Group (JPEG) Files|*.jpeg;*.jpg|All Files|*.*"; break;
				case DiagrammingImageFormat.Png: fileFilter = "Portable Network Graphics Files|*.png|All Files|*.*"; break;
				case DiagrammingImageFormat.Tiff: fileFilter = "Tagged Image File Format Files|*.tiff;*.tif|All Files|*.*"; break;
				default: throw new DiagrammingUnsupportedValueException(imageFormat);
			}
			saveFileDialog.Filter = fileFilter;
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				using (Image image = GetImageFromDiagram(imageFormat)) {
					if (image != null) GdiHelpers.SaveImageToFile(image, saveFileDialog.FileName, imageFormat, 100);
				}
			}
		}

		#endregion


		#region "Edit" menu event handler implementations

		private void newDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			Diagram diagram = new Diagram(string.Format("Diagram {0}", displayTabControl.TabPages.Count + 1));
			diagram.Title = diagram.Name;
			diagram.Width = 1000;
			diagram.Height = 1000;
			ICommand cmd = new InsertDiagramCommand(diagram);
			project.ExecuteCommand(cmd);
			CreateDisplayForDiagram(diagram);
			showDiagramSettingsToolStripMenuItem_Click(this, new EventArgs());
		}


		private void deleteDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			Diagram diagram = CurrentDisplay.Diagram;
			ICommand cmd = new DeleteDiagramCommand(diagram);
			project.ExecuteCommand(cmd);

			// Try to remove Display (in case the Cache-Event was not handled)
			RemoveDisplayOfDiagram(diagram);
		}


		private void copyShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.Copy(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, false);
			UpdateToolBarAndMenuItems();
		}


		private void copyShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.Copy(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, true);
			UpdateToolBarAndMenuItems();
		}


		private void cutShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.Cut(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, false);
			UpdateToolBarAndMenuItems();
		}


		private void cutShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.Cut(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, true);
			UpdateToolBarAndMenuItems();
		}


		private void pasteMenuItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.Paste(CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, CurrentDisplay.GridSize, CurrentDisplay.GridSize);
			UpdateToolBarAndMenuItems();
		}


		private void deleteShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.DeleteShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, true);
		}


		private void deleteShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DiagramSetController.DeleteShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, false);
		}


		private void historyTrackBar_ValueChanged(object sender, EventArgs e) {
			//if (CurrentDisplay != null) {
				int d = currHistoryPos - historyTrackBar.Value;
				bool commandExecuted = false;
				try {
					project.History.CommandExecuted -= history_CommandExecuted;
					project.History.CommandsExecuted -= history_CommandsExecuted;

					if (d != 0) {
						if (d < 0) project.History.Undo(d * (-1));
						else if (d > 0) project.History.Redo(d);
						commandExecuted = true;
					}
				} catch (DiagrammingSecurityException exc) {
					MessageBox.Show(this, exc.Message, "Command execution failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
					commandExecuted = false;
				} finally {
					project.History.CommandExecuted += history_CommandExecuted;
					project.History.CommandsExecuted += history_CommandsExecuted;
				}

				if (commandExecuted)
					currHistoryPos = historyTrackBar.Value;
				else if (historyTrackBar.Value != currHistoryPos)
					historyTrackBar.Value = currHistoryPos;
				UpdateToolBarAndMenuItems();
			//}
		}


		private void undoButton_Click(object sender, EventArgs e) {
			if (historyTrackBar.Value < historyTrackBar.Maximum)
				historyTrackBar.Value += 1;
			if (sender is ToolStripItem) ((ToolStripItem)sender).Invalidate();
		}


		private void redoButton_Click(object sender, EventArgs e) {
			if (historyTrackBar.Value > historyTrackBar.Minimum)
				historyTrackBar.Value -= 1;
			if (sender is ToolStripItem) ((ToolStripItem)sender).Invalidate();
		}


		private void undoItem_Click(object sender, EventArgs e) {
			System.Windows.Forms.ToolStripSplitButton button = (System.Windows.Forms.ToolStripSplitButton)((System.Windows.Forms.ToolStripItem)sender).OwnerItem;
			// Undo was executed from the main menu (DropDownList)
			if (button != null) {
				int idx = button.DropDownItems.IndexOf((System.Windows.Forms.ToolStripMenuItem)sender);
				historyTrackBar.Value += idx + 1;
			}
			else
				// Undo was executed from context menu
				historyTrackBar.Value += 1;
		}


		private void redoItem_Click(object sender, EventArgs e) {
			System.Windows.Forms.ToolStripSplitButton button = (System.Windows.Forms.ToolStripSplitButton)((System.Windows.Forms.ToolStripItem)sender).OwnerItem;
			// Redo was executed from the main menu (DropDownList)
			if (button != null) {
				int idx = button.DropDownItems.IndexOf((System.Windows.Forms.ToolStripMenuItem)sender);
				historyTrackBar.Value -= idx + 1;
			}
			else
				// Redo was executed from context menu
				historyTrackBar.Value -= 1;
		}

		#endregion


		#region "View" menu event handler implementations

		private void showGridToolStripMenuItem_Click(object sender, EventArgs e) {
			bool isChecked = false;
			if (sender is System.Windows.Forms.ToolStripMenuItem)
				isChecked = ((System.Windows.Forms.ToolStripMenuItem)sender).Checked;
			else if (sender is System.Windows.Forms.ToolStripButton)
				isChecked = ((System.Windows.Forms.ToolStripButton)sender).Checked;
			ShowGrid = isChecked;
			showGridMenuItem.Checked = isChecked;
			showGridToolbarButton.Checked = isChecked;
		}


		private void showDisplaySettingsItem_Click(object sender, EventArgs e) {
			DisplaySettingsForm dlg = new DisplaySettingsForm();
			dlg.ShowGrid = ShowGrid;
			dlg.SnapToGrid = SnapToGrid;
			dlg.GridSize = GridSize;
			dlg.SnapDistance = SnapDistance;
			dlg.ResizePointShape = ResizePointShape;
			dlg.ConnectionPointShape = ConnectionPointShape;
			dlg.ControlPointSize = ControlPointSize;
			dlg.ShowDialog(this);
			if (dlg.DialogResult == DialogResult.OK) {
				ShowGrid = dlg.ShowGrid;
				showGridMenuItem.Checked = ShowGrid;
				showGridToolbarButton.Checked = ShowGrid;

				SnapToGrid = dlg.SnapToGrid;
				GridSize = dlg.GridSize;
				SnapDistance = dlg.SnapDistance;
				ResizePointShape = dlg.ResizePointShape;
				ConnectionPointShape = dlg.ConnectionPointShape;
				ControlPointSize = dlg.ControlPointSize;
			}
			dlg.Dispose();
			dlg = null;
		}


		private void showDiagramSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
			diagramSetController.SetObject(0, CurrentDisplay.Diagram);
		}


		private void editDesignsAndStylesToolStripMenuItem_Click(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}


		private void viewShowLayoutControlToolStripMenuItem_Click(object sender, EventArgs e) {
			if (layoutControlForm == null) {
				LayoutView lcf = new LayoutView();
				lcf.Project = CurrentDisplay.Project;
				lcf.Diagram = CurrentDisplay.Diagram;
				lcf.SelectedShapes = CurrentDisplay.SelectedShapes;
				lcf.FormClosed += LayoutControlForm_FormClosed;
				lcf.LayoutChanged += LayoutControlForm_LayoutChanged;
				lcf.Show();
				layoutControlForm = lcf;
			} else {
				layoutControlForm.Activate();
			}
		}


		private void highQualityToolStripMenuItem_Click(object sender, EventArgs e) {
			HighQuality = !HighQuality;
			highQualityRenderingMenuItem.Checked = HighQuality;
		}

		#endregion


		#region "Tools" menu event handler implementations

		private void adoNetDatabaseGeneratorToolStripMenuItem_Click(object sender, EventArgs e) {
			if (project.IsOpen) {
				DialogResult result = MessageBox.Show("The project has to be closed before creating a new ADO.NET database repository.\nClose Project Now?", "", MessageBoxButtons.YesNo);
				if (result == DialogResult.Yes) CloseProject();
				else return;
			}

			SqlStore store = new SqlStore();
			store.ServerName = ".\\SQLEXPRESS";
			store.DatabaseName = "TurboDiagram";
			((CachedRepository)project.Repository).Store = store;
			project.RemoveAllLibraries();
			store.DropDbSchema();
			project.AddLibraryByFilePath("Dataweb.Diagramming.GeneralShapes");
			project.RegisterEntityTypes();
			store.CreateDbCommands((IStoreCache)project.Repository);
			store.CreateDbSchema();
			project.Close();
		}

		#endregion


		#region Fields

		private const string newProjectName = "New Diagramming Project";
		private const string xmlStoreTypeName = "XML";
		private const string sqlServerStoreTypeName = "SQL Server";
		private const string appTitle = "Diagramming Designer";

		private Point p;
		private int currHistoryPos;
		private Display currentDisplay;

		// ownerDisplay config
		private bool showGrid = true;
		private bool snapToGrid = true;
		private int gridSize = 20;
		private int snapDistance = 5;
		private ControlPointShape resizePointShape = ControlPointShape.Square;
		private ControlPointShape connectionPointShape = ControlPointShape.Circle;
		private int controlPointSize = 3;

		private string configFile;
		private string configFolder;
		private int zoom = 100;
		private bool highQuality = true;
		private int historyDropDownItemCount = 20;
		private LibraryManagementDialog libDialog;

		private LayoutView layoutControlForm;
		private TemplateEditorDialog templateEditorDialog;

		private XmlWriter cfgWriter;
		private XmlReader cfgReader;
		private List<RepositoryInfo> recentProjects;


		private struct RepositoryInfo {

			public static readonly RepositoryInfo Empty;

			public static bool operator ==(RepositoryInfo a, RepositoryInfo b) {
				return (a.location == b.location
					&& a.projectName == b.projectName
					&& a.computerName == b.computerName
					&& a.typeName == b.typeName);
			}


			public static bool operator !=(RepositoryInfo a, RepositoryInfo b) { return !(a == b); }


			public override bool Equals(object obj) {
				return (obj is RepositoryInfo && ((RepositoryInfo)obj) == this);
			}


			public override int GetHashCode() {
				int hashCode = base.GetHashCode();
				if (location != null) hashCode ^= location.GetHashCode();
				if (projectName != null) hashCode ^= projectName.GetHashCode();
				if (computerName != null) hashCode ^= computerName.GetHashCode();
				if (typeName != null) hashCode ^= typeName.GetHashCode();
				return hashCode;
			}


			public RepositoryInfo(string projectName, string typeName, string serverName, string dataSource) {
				this.projectName = projectName;
				this.typeName = typeName;
				this.computerName = serverName;
				this.location = dataSource;
			}


			public string projectName;

			public string typeName;

			/// <summary>
			/// Contains the server projectName (in case of an AdoRepository) or the computer projectName (in case of a XmlStore).
			/// </summary>
			public string computerName;

			/// <summary>
			/// Contains the database' data source (in case of an AdoRepository) or the path to an XML file (in case of a XmlStore).
			/// </summary>
			public string location;

			static RepositoryInfo() {
				Empty.location = string.Empty;
				Empty.projectName = string.Empty;
				Empty.computerName = string.Empty;
				Empty.typeName = string.Empty;
			}
		}


		private const string projectsTag = "Projects";
		private const string projectTag = "Project";
		private const string projectNameTag = "Name";
		private const string typeNameTag = "RepositoryType";
		private const string serverNameTag = "ServerName";
		private const string dataSourceTag = "DataSource";

#if TdbRepository
		private const string fileFilterAllRepositories = "Diagramming Repository Files|*.xml;*.tdbd|XML Repository Files|*.xml|TurboDB Repository Databases|*.tdbd|All Files|*.*";
		private const string fileFilterTurboDBRepository = "TurboDB Repository Databases|*.tdbd|All Files|*.*";
#endif
		private const string fileFilterXmlRepository = "XML Repository Files|*.xml|All Files|*.*";

		#endregion
	}

}