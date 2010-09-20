/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.WinFormsUI;
using System.ComponentModel.Design;
using System.ComponentModel;


namespace Dataweb.NShape.Designer {

	public partial class DiagramDesignerMainForm : Form {

		public DiagramDesignerMainForm() {
			InitializeComponent();
			Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			runtimeModeComboBox.SelectedIndex = 0;
#if !DEBUG
			historyTrackBar.Visible = false;
			toolStripContainer1.TopToolStripPanel.Controls.Remove(debugToolStrip);
#endif
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


		public Color GridColor {
			get { return gridColor; }
			set {
				gridColor = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.GridColor = value;
				}
			}
		}


		public bool ShowDefaultContextMenu {
			get {
				return CurrentDisplay.ShowDefaultContextMenu
					&& toolSetListViewPresenter.ShowDefaultContextMenu
					&& modelTreePresenter.ShowDefaultContextMenu
					&& layerEditorListView.ShowDefaultContextMenu;
			}
			set {
				toolSetListViewPresenter.ShowDefaultContextMenu
				= modelTreePresenter.ShowDefaultContextMenu
				= layerEditorListView.ShowDefaultContextMenu = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ShowDefaultContextMenu = value;
				}
			}
		}


		public bool HideDeniedMenuItems {
			get {
				return CurrentDisplay.HideDeniedMenuItems
					&& toolSetListViewPresenter.HideDeniedMenuItems
					&& modelTreePresenter.HideDeniedMenuItems
					&& layerEditorListView.HideDeniedMenuItems;
			}
			set {
				toolSetListViewPresenter.HideDeniedMenuItems
				= modelTreePresenter.HideDeniedMenuItems
				= layerEditorListView.HideDeniedMenuItems = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.HideDeniedMenuItems = value;
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


#if DEBUG

		public bool ShowCellOccupation {
			get { return showCellOccupation; }
			set {
				showCellOccupation = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ShowCellOccupation = showCellOccupation;
				}
			}
		}


		public bool ShowInvalidatedAreas {
			get { return showInvalidatedAreas; }
			set {
				showInvalidatedAreas = value;
				foreach (TabPage t in displayTabControl.TabPages) {
					Display d = (Display)t.Controls[0];
					d.ShowInvalidatedAreas = showInvalidatedAreas;
				}
			}
		}

#endif

		#endregion


		#region ConfigFile, Project and Store

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
			cfgWriter.WriteStartElement(projectDirectoryTag);
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

			foreach (XmlNode projectDirNode in xmlDoc.GetElementsByTagName(projectDirectoryTag)) {
				XmlAttribute attr = projectDirNode.Attributes[pathTag];
				if (attr != null) xmlStoreDirectory = attr.Value;
				break;
			}

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
					if (repositoryInfo != RepositoryInfo.Empty && !recentProjects.Contains(repositoryInfo)) 
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

			// Find the "Project Directory" node
			XmlNode projectDirectoryNode = null;
			foreach (XmlNode xmlNode in xmlDoc.GetElementsByTagName(projectDirectoryTag)) {
				projectDirectoryNode = xmlNode;
				break;
			}
			Debug.Assert(projectDirectoryNode != null);
			// Save last project directory
			projectDirectoryNode.Attributes.Append(xmlDoc.CreateAttribute(pathTag)).Value = xmlStoreDirectory;
			
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

				if (repositoriesNode.ChildNodes.Count >= RecentProjectsLimit)
					repositoriesNode.RemoveChild(repositoriesNode.FirstChild);

				XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, projectTag, xmlDoc.NamespaceURI);
				newNode.Attributes.Append(xmlDoc.CreateAttribute(projectNameTag)).Value = projectInfo.projectName;
				newNode.Attributes.Append(xmlDoc.CreateAttribute(typeNameTag)).Value = projectInfo.typeName;
				newNode.Attributes.Append(xmlDoc.CreateAttribute(serverNameTag)).Value = projectInfo.computerName;
				newNode.Attributes.Append(xmlDoc.CreateAttribute(dataSourceTag)).Value = projectInfo.location;
				repositoriesNode.AppendChild(newNode);
			}
			// Save to file
			cfgWriter = OpenCfgWriter(configFolder + configFile);
			xmlDoc.Save(cfgWriter);
			cfgWriter.Close();
		}


		private void MaintainRecentProjects() {
			bool modified = false, remove = false;
			for (int i = recentProjects.Count - 1; i >= 0; --i) {
				remove = false;
				if (recentProjects[i].typeName == RepositoryInfo.SqlServerStoreTypeName)
					continue;
				else if (recentProjects[i].typeName == RepositoryInfo.XmlStoreTypeName) {
					if (!File.Exists(recentProjects[i].location)) {
						string msgFormat = "The file or folder '{0}' cannot be opened. Do you want to remove it from the 'Recently opened projects' list?";
						remove = (MessageBox.Show(this, string.Format(msgFormat, recentProjects[i].location), "File not found", MessageBoxButtons.YesNo) == DialogResult.Yes);
					}
				}
				if (remove) {
					recentProjects.RemoveAt(i);
					modified = true;
				}
			}
			if (modified) SaveRecentProjectsToConfigFile();
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
			item.ToolTipText = string.Format("Project: {0}{3}Location: {1}{3}Repository Type: {2}", projectInfo.projectName, projectInfo.location, projectInfo.typeName, Environment.NewLine);
			item.Click += openRecentProjectMenuItem_Click;
			recentProjectsMenuItem.DropDownItems.Insert(0, item);
			if (!recentProjectsMenuItem.Visible) recentProjectsMenuItem.Visible = true;
		}


		private void CreateRecentProjectsMenuItems() {
			ClearRecentProjectsMenu();
			recentProjectsMenuItem.Visible = (recentProjects.Count > 0);
			for (int i = 0; i < recentProjects.Count; ++i)
				PrependRecentProjectsMenuItem(recentProjects[i]);
		}


		private RepositoryInfo GetReposistoryInfo(Project project) {
			RepositoryInfo projectInfo = RepositoryInfo.Empty;
			projectInfo.projectName = project.Name;
			Store store = ((CachedRepository)project.Repository).Store;
			if (store is XmlStore) {
				projectInfo.typeName = RepositoryInfo.XmlStoreTypeName;
				string filePath = ((XmlStore)store).ProjectFilePath;
				projectInfo.location = filePath;
				projectInfo.computerName = Environment.MachineName;
			} else if (store is SqlStore) {
				projectInfo.typeName = RepositoryInfo.SqlServerStoreTypeName;
				projectInfo.location = ((SqlStore)store).DatabaseName;
				projectInfo.computerName = ((SqlStore)store).ServerName;
			} else Debug.Fail("Unexpected repository type");
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
			ClearRecentProjectsMenu();
			foreach (RepositoryInfo pi in recentProjects)
				PrependRecentProjectsMenuItem(pi);
		}


		private void ClearRecentProjectsMenu() {
			for (int i = recentProjectsMenuItem.DropDownItems.Count - 1; i >= 0; --i) {
				recentProjectsMenuItem.DropDownItems[i].Click -= openRecentProjectMenuItem_Click;
				recentProjectsMenuItem.DropDownItems[i].Dispose();
			}
			recentProjectsMenuItem.DropDownItems.Clear();
		}


		private void CreateProject(string projectName, Store store, bool askUserLoadLibraries) {
			ReplaceRepository(projectName, store);
			project.Create();
			projectSaved = false;
			DisplayDiagrams();
			if (askUserLoadLibraries) 
				CheckLibrariesLoaded();
			// Adjust menu items
			saveMenuItem.Enabled = true;
			saveAsMenuItem.Enabled = true;
		}


		private void CheckLibrariesLoaded() {
			bool librariesLoaded = false;
			foreach (System.Reflection.Assembly a in project.Libraries) {
				librariesLoaded = true;
				break;
			}
			if (!librariesLoaded) {
				if (MessageBox.Show(this, "Do you want to load shape libraries now?", "Load shape libraries",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
					using (LibraryManagementDialog dlg = new LibraryManagementDialog(project))
						dlg.ShowDialog(this);
				}
			}
		}


		private void OpenProject(string projectName, Store repository) {
			Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			try {
				ReplaceRepository(projectName, repository);
				project.Open();
				DisplayDiagrams();
				projectSaved = true;
				// Move project on top of the recent projects list 
				RepositoryInfo repositoryInfo = GetReposistoryInfo(project);
				RemoveFromRecentProjects(repositoryInfo);
				AddToRecentProjects(repositoryInfo);
				UpdateRecentProjectsMenu();
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error while opening Repository.", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				Cursor = Cursors.Default;
			}
		}


		// Returns false, if the save should be retried with another project name.
		private bool SaveProject() {
			bool result = false;
			this.Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			try {
				// Save changes to xml file
				project.Repository.SaveChanges();
				projectSaved = true;

				// Add project to "Recent Projects" list
				RepositoryInfo projectInfo = GetReposistoryInfo(project);
				RemoveFromRecentProjects(projectInfo);
				AddToRecentProjects(projectInfo);
				UpdateRecentProjectsMenu();
				result = true;
			} catch (IOException exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				this.Cursor = Cursors.Default;
			}
			return result;
		}


		// Returns false, if the save should be retried with another project name.
		private bool SaveProjectAs() {
			bool result = false;
			if (((CachedRepository)project.Repository).Store is XmlStore) {
				XmlStore xmlStore = (XmlStore)((CachedRepository)project.Repository).Store;

				// Select a file name
				saveFileDialog.CreatePrompt = false;		// Do not ask wether to create the file
				saveFileDialog.CheckFileExists = false;	// Do not check wether the file does NOT exist
				saveFileDialog.CheckPathExists = true;		// Ask wether to overwrite existing file
				saveFileDialog.AutoUpgradeEnabled = (Environment.OSVersion.Version.Major >= 6);
				saveFileDialog.Filter = fileFilterXmlRepository;
				if (Directory.Exists(xmlStore.DirectoryName))
					saveFileDialog.InitialDirectory = xmlStore.DirectoryName;
				else
					saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				saveFileDialog.FileName = Path.GetFileName(xmlStore.ProjectFilePath);

				// Try to save repository to file...
				if (saveFileDialog.ShowDialog() == DialogResult.OK) {
					xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
					project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
					Text = string.Format("{0} - {1}", appTitle, project.Name);
					// Delete file if it exists, because the user was prompted wether to overwrite it before (SaveFileDialog.CheckPathExists).
					if (project.Repository.Exists()) project.Repository.Erase();
					saveMenuItem.Enabled = true;
					result = SaveProject();
				}
			} else if (((CachedRepository)project.Repository).Store is AdoNetStore) {
				// Save repository to database because the database and the project name are 
				// selected before creating the project when using AdoNet stores
				saveMenuItem.Enabled = true;
				result = SaveProject();
			} else {
				if (((CachedRepository)project.Repository).Store == null)
					MessageBox.Show(this, "There is no store component attached to the repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				else {
					string msg = string.Format("Unsupported store type: '{0}'.", ((CachedRepository)project.Repository).Store.GetType().Name);
					MessageBox.Show(this, msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			} 
			return result;
		}


		private bool CloseProject() {
			bool result = true;
			if (project.Repository.IsModified) {
				string msg = string.Format("Do you want to save the current project '{0}' before closing it?", project.Name);
				DialogResult dlgResult = MessageBox.Show(this, msg, "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				switch (dlgResult) {
					case DialogResult.Yes:
						if (project.Repository.Exists())
							SaveProject();
						else SaveProjectAs();
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
				project.Close();
				projectSaved = false;

				// clear all displays and diagramControllers
				for (int i = displayTabControl.TabPages.Count - 1; i >= 0; --i) {
					if (displayTabControl.TabPages[i].Controls[0] is Display) {
						Display display = (Display)displayTabControl.TabPages[i].Controls[0];
						displayTabControl.TabPages.RemoveAt(i);

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


		private void CheckFrameworkVersion() {
			System.Reflection.Assembly exeAssembly = this.GetType().Assembly;
			System.Reflection.Assembly coreAssembly = typeof(Project).Assembly;
			System.Reflection.Assembly uiAssembly = typeof(Display).Assembly;

			if (exeAssembly == null || coreAssembly == null || uiAssembly == null) {
				throw new Exception("Failed to retrive component's assemblies.");
			} else {
				// Check installed .NET framework version
				Version coreAssemblyVersion = new Version(coreAssembly.ImageRuntimeVersion.Replace("v", ""));
				Version uiAssemblyVersion = new Version(uiAssembly.ImageRuntimeVersion.Replace("v", ""));
				Version exeAssemblyVersion = new Version(exeAssembly.ImageRuntimeVersion.Replace("v", ""));
				if (Environment.Version < coreAssemblyVersion 
					|| Environment.Version < uiAssemblyVersion
					|| Environment.Version < exeAssemblyVersion) {
					string msg = string.Empty;
					msg += string.Format("The installed .NET framework version does not meet the requirements:{0}", Environment.NewLine);
					msg += string.Format(".NET Framework {0} is installed, version {1} is required.", Environment.Version, coreAssembly.ImageRuntimeVersion);
					throw new NShapeException(msg);
				}

				System.Reflection.AssemblyName designerAssemblyName = this.GetType().Assembly.GetName();
				System.Reflection.AssemblyName coreAssemblyName = typeof(Project).Assembly.GetName();
				System.Reflection.AssemblyName uiAssemblyName = typeof(Display).Assembly.GetName();
				// Check nShape framework library versions
				if (coreAssemblyName.Version != uiAssemblyName.Version) {
					string msg = string.Empty;
					msg += "The versions of the loaded nShape framework libraries do not match:" + Environment.NewLine;
					msg += string.Format("{0}: Version {1}{2}", coreAssemblyName.Name, coreAssemblyName.Version, Environment.NewLine);
					msg += string.Format("{0}: Version {1}{2}", uiAssemblyName.Name, uiAssemblyName.Version, Environment.NewLine);
					throw new NShapeException(msg);
				}
				// Check program against used nShape framework library versions
				if (coreAssemblyName.Version != designerAssemblyName.Version
					|| uiAssemblyName.Version != designerAssemblyName.Version) {
					string msg = string.Empty;
					msg += "The version of this program does not match the versions of the loaded nShape framework libraries:" + Environment.NewLine;
					msg += string.Format("{0}: Version {1}{2}", designerAssemblyName.Name, designerAssemblyName.Version, Environment.NewLine);
					msg += string.Format("{0}: Version {1}{2}", coreAssemblyName.Name, coreAssemblyName.Version, Environment.NewLine);
					msg += string.Format("{0}: Version {1}{2}", uiAssemblyName.Name, uiAssemblyName.Version, Environment.NewLine);
					MessageBox.Show(this, msg, "Assembly Version Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}


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
				display.Name = string.Format("Display{0}", displayTabControl.TabCount + 1);
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
#if DEBUG
				display.ShowCellOccupation = ShowCellOccupation;
				display.ShowInvalidatedAreas = ShowInvalidatedAreas;
#endif
				display.Dock = DockStyle.Fill;
				//
				// Assign DiagramSetController and diagram
				display.PropertyController = propertyController;
				display.DiagramSetController = diagramSetController;
				display.Diagram = diagram;
				display.CurrentTool = toolSetController.SelectedTool;
				//display.UserMessage += display_UserMessage;
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
			if (!diagramAdded) {
				Diagram diagram = new Diagram(string.Format("Diagram {0}", displayTabControl.TabPages.Count + 1));
				project.Repository.InsertDiagram(diagram);
				CreateDisplayForDiagram(diagram);
				showDiagramSettingsToolStripMenuItem_Click(this, new EventArgs());
			}
		}


		private Display CurrentDisplay {
			get { return currentDisplay; }
			set {
				if (currentDisplay != null) {
					currentDisplay.MouseMove -= display_MouseMove;
					currentDisplay.ShapesSelected -= display_ShapesSelected;
					currentDisplay.ZoomChanged -= display_ZoomChanged;
				}
				currentDisplay = value;
				if (currentDisplay != null) {
					currentDisplay.MouseMove += display_MouseMove;
					currentDisplay.ShapesSelected += display_ShapesSelected;
					currentDisplay.ZoomChanged += display_ZoomChanged;

					currentDisplay.HighQualityRendering = HighQuality;
					currentDisplay.ShowGrid = ShowGrid;
					currentDisplay.HighQualityBackground = HighQuality;
					currentDisplay.ZoomLevel = Zoom;
					if (currentDisplay.Diagram != null) {
						currentDisplay.CurrentTool = toolSetController.SelectedTool;
						propertyController.SetObject(0, currentDisplay.Diagram);
					}

					display_ShapesSelected(currentDisplay, null);
				}
			}
		}


		#region [Private] Register event handlers

		private void RegisterRepositoryEvents() {
			if (project.Repository != null) {
				project.Repository.DiagramInserted += repository_DiagramInserted;
				project.Repository.DiagramDeleted += repository_DiagramDeleted;
				project.Repository.ModelObjectsInserted += repository_ModelObjectsInsertedOrDeleted;
				project.Repository.ModelObjectsDeleted += repository_ModelObjectsInsertedOrDeleted;
			}
		}


		private void UnregisterRepositoryEvents() {
			if (project.Repository != null) {
				project.Repository.DiagramInserted -= repository_DiagramInserted;
				project.Repository.DiagramDeleted -= repository_DiagramDeleted;
				project.Repository.ModelObjectsInserted -= repository_ModelObjectsInsertedOrDeleted;
				project.Repository.ModelObjectsDeleted -= repository_ModelObjectsInsertedOrDeleted;
			}
		}

		#endregion


		#region [Private] Event Handler implementations - Project, History and Repository

		private void project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			// nothing to do here...
		}


		private void project_Opened(object sender, EventArgs e) {
			RegisterRepositoryEvents();
			// Set main form title
			Text = string.Format("{0} - {1}", appTitle, project.Name);
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
			
			historyTrackBar.Maximum = project.History.UndoCommandCount + project.History.RedoCommandCount;
			Text = appTitle;
			statusLabelMessage.Text =
			statusLabelPosition.Text = string.Empty;
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
					if (currHistoryPos > 0) --currHistoryPos;
					if (historyTrackBar.Value > 0) --historyTrackBar.Value;
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


		private void repository_ModelObjectsInsertedOrDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			bool modelExists = false;
			foreach (IModelObject modelObject in project.Repository.GetModelObjects(null)) {
				modelExists = true;
				break;
			}
			if (modelTreeView.Visible != modelExists)
				modelTreeView.Visible = modelExists;
		}

		#endregion


		#region [Private] Event Handler implementations - Display

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
				else statusLabelMessage.Text = string.Empty;
				UpdateToolBarAndMenuItems();
				//
				if (layoutControlForm != null)
					layoutControlForm.SelectedShapes = CurrentDisplay.SelectedShapes;
			}
		}


		private void display_ZoomChanged(object sender, EventArgs e) {
			string txt = string.Format("{0} %", currentDisplay.ZoomLevel);
			if (txt != zoomToolStripComboBox.Text)
				zoomToolStripComboBox.Text = currentDisplay.ZoomLevel.ToString();
		}


		private void display_UserMessage(object sender, UserMessageEventArgs e) {
			MessageBoxIcon icon = MessageBoxIcon.Information;
			MessageBox.Show(this, e.MessageText, "Information", MessageBoxButtons.OK, icon);
		}

		#endregion


		#region [Private] Event Handler implementations - ModelTree

		private void modelTree_ModelObjectSelected(object sender, ModelObjectSelectedEventArgs eventArgs) {
			this.propertyWindowTabControl.SelectedTab = this.propertyWindowModelTab;
		}

		#endregion


		#region [Private] Event Handler implementations - ToolBox

		private void toolBoxAdapter_ShowTemplateEditorDialog(object sender, TemplateEditorEventArgs e) {
			templateEditorDialog = new TemplateEditorDialog(e.Project, e.Template);
			templateEditorDialog.Show(this);
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


		#region [Private] Event Handler implementations - Misc

		private void DiagramDesignerMainForm_Load(object sender, EventArgs e) {
			try {
				// Read config file
				configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nShapeConfigDirectory);
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
				MaintainRecentProjects();
				CreateRecentProjectsMenuItems();

				// Get command line parameters and check if a repository should be loaded on startup
				RepositoryInfo repositoryInfo = RepositoryInfo.Empty;
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				if (commandLineArgs != null) {
					int cnt = commandLineArgs.Length;
					for (int i = 0; i < cnt; ++i) {
						string path = Path.GetFullPath(commandLineArgs[i]);
						if (string.IsNullOrEmpty(path)) continue;
						else {
							if (path == Path.GetFullPath(Application.ExecutablePath))
								continue;
							// Check if the file is an xml document
							if (File.Exists(path)) {
								TextReader reader = null;
								try {
									reader = File.OpenText(path);
									if (reader.ReadLine().Contains("xml")) {
										repositoryInfo.computerName = Environment.MachineName;
										repositoryInfo.location = path;
										repositoryInfo.projectName = Path.GetFileNameWithoutExtension(path);
										repositoryInfo.typeName = RepositoryInfo.XmlStoreTypeName;
									}
								} finally {
									if (reader != null) {
										reader.Close();
										reader.Dispose();
										reader = null;
									}
								}
							}
						}
					}
				}

				XmlStore store;
				if (repositoryInfo != RepositoryInfo.Empty) {
					store = new XmlStore(Path.GetDirectoryName(repositoryInfo.location), Path.GetExtension(repositoryInfo.location));
					OpenProject(repositoryInfo.projectName, store);
				} else {
					//store = new XmlStore(xmlStoreDirectory, ".xml");
					//CreateProject(newProjectName, store, false);
					NewXmlRepositoryProject(false);
					project.AddLibraryByName("Dataweb.NShape.GeneralShapes");
#if DEBUG
					// Shape libraries
					project.AddLibraryByName("Dataweb.NShape.SoftwareArchitectureShapes");
					//project.AddLibraryByName("Dataweb.NShape.FlowChartShapes");
					//project.AddLibraryByName("Dataweb.NShape.ElectricalShapes");
					// ModelObjectTypes libraries
					//project.AddLibraryByFilePath("Dataweb.NShape.GeneralModelObjects.dll");
#endif
				}

				UpdateToolBarAndMenuItems();
			} catch (Exception ex) {
			   MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		void DiagramDesignerMainForm_Shown(object sender, System.EventArgs e) {
			CheckFrameworkVersion();
			CheckLibrariesLoaded();
		}


		private void DiagramDesignerMainForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (!CloseProject()) e.Cancel = true;
			else SaveRecentProjectsToConfigFile();
		}


		private void displaysTabControl_SelectedIndexChanged(object sender, EventArgs e) {
			TabPage tab = displayTabControl.SelectedTab;
			if (tab != null && tab.Controls.Count > 0) {
				CurrentDisplay = (Display)tab.Controls[0];

				layerPresenter.DiagramPresenter = CurrentDisplay;

				UpdateToolBarAndMenuItems();
			}
		}


		private void LayoutControlForm_LayoutChanged(object sender, EventArgs e) {
			// Why? Makes preview switch between original and new State.
			// currentDisplay.SaveChanges();
		}


		private void LayoutControlForm_FormClosed(object sender, FormClosedEventArgs e) {
			layoutControlForm = null;
		}

		#endregion


		#region [Private] Event Handler implementations - Toolbar

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
			} else {
				if (int.TryParse(zoomToolStripComboBox.Text.Replace('%', ' ').Trim(), out zoom)) {
					if (zoom > 0 && Zoom != zoom) Zoom = zoom;
				}
			}
		}


		private void runtimeModeButton_SelectedIndexChanged(object sender, EventArgs e) {
			((RoleBasedSecurityManager)project.SecurityManager).CurrentRoleName = runtimeModeComboBox.Text;
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "File"

		private void newXMLRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			NewXmlRepositoryProject(true);
		}


		private void NewXmlRepositoryProject(bool askUserLoadLibraries) {
			if (CloseProject()) {
				if (!Directory.Exists(xmlStoreDirectory)) 
					xmlStoreDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				XmlStore store = new XmlStore(xmlStoreDirectory, ".xml");
				CreateProject(newProjectName, store, askUserLoadLibraries);
			}
		}


		private void newSQLServerRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			string projectName;
			AdoNetStore store = GetAdoNetStore(out projectName, OpenAdoNetRepositoryDialog.Mode.CreateProject);
			if (store != null) CreateProject(projectName, store, true);
		}


		private AdoNetStore GetAdoNetStore() {
			string projectName;
			return GetAdoNetStore(out projectName, OpenAdoNetRepositoryDialog.Mode.CreateSchema);
		}


		private AdoNetStore GetAdoNetStore(out string projectName, OpenAdoNetRepositoryDialog.Mode mode) {
			projectName = string.Empty;
			AdoNetStore result = null;
			try {
				Cursor = Cursors.WaitCursor;
				Application.DoEvents();
				using (OpenAdoNetRepositoryDialog dlg = new OpenAdoNetRepositoryDialog(this, defaultServerName, defaultDatabaseName, mode)) {
					if (dlg.ShowDialog() == DialogResult.OK && CloseProject()) {
						if (dlg.ProviderName == sqlServerProviderName) {
							result = new SqlStore(dlg.ServerName, dlg.DatabaseName);
							projectName = dlg.ProjectName;
						} else MessageBox.Show(this, "Unsupported database repository.", "Unsupported repository", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			} finally { Cursor = Cursors.Default; }
			return result;
		}


		private void openXMLRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.Filter = fileFilterXmlRepository;
			openFileDialog.AutoUpgradeEnabled = (Environment.OSVersion.Version.Major >= 6);
			if (Directory.Exists(xmlStoreDirectory))
				openFileDialog.InitialDirectory = xmlStoreDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK && CloseProject()) {
				xmlStoreDirectory = Path.GetDirectoryName(openFileDialog.FileName);
				XmlStore store = new XmlStore(xmlStoreDirectory, Path.GetExtension(openFileDialog.FileName));
				OpenProject(Path.GetFileNameWithoutExtension(openFileDialog.FileName), store);
			}
		}


		private void openSQLServerRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			string projectName;
			AdoNetStore store = GetAdoNetStore(out projectName, OpenAdoNetRepositoryDialog.Mode.OpenProject);
			if (store != null) OpenProject(projectName, store);
		}


		private void openRecentProjectMenuItem_Click(object sender, EventArgs e) {
			Debug.Assert(sender is ToolStripItem && ((ToolStripItem)sender).Tag is RepositoryInfo);
			RepositoryInfo repositoryInfo = (RepositoryInfo)((ToolStripItem)sender).Tag;
			if (CloseProject()) {
				Store store = RepositoryInfo.CreateStore(repositoryInfo);
				if (store != null) OpenProject(repositoryInfo.projectName, store);
				else MessageBox.Show(this, string.Format("{0} repositories are not supported by this version.", repositoryInfo.typeName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
			SaveProjectAs();
		}


		private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
			if (!projectSaved || !project.Repository.Exists())
				SaveProjectAs();
			else SaveProject();
		}


		private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e) {
			CloseProject();
		}


		private void ManageShapeAndModelLibrariesMenuItem_Click(object sender, EventArgs e) {
			using (LibraryManagementDialog dlg = new LibraryManagementDialog(project))
				dlg.ShowDialog(this);
		}


		private void exportDiagramAsMenuItem_Click(object sender, EventArgs e) {
			using (ExportDiagramDialog dlg = new ExportDiagramDialog(CurrentDisplay))
				dlg.ShowDialog(this);
		}


		private void emfPlusFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportMetaFile(ImageFileFormat.EmfPlus);
		}


		private void emfOnlyFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportMetaFile(ImageFileFormat.Emf);
		}


		private void pngFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(ImageFileFormat.Png);
		}


		private void jpgFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(ImageFileFormat.Jpeg);
		}


		private void bmpFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(ImageFileFormat.Bmp);
		}


		private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}


		private Image GetImageFromDiagram(ImageFileFormat imageFormat) {
			Image result = null;
			Color backColor = Color.Transparent;
			if (CurrentDisplay.SelectedShapes.Count > 0)
				result = CurrentDisplay.Diagram.CreateImage(imageFormat, CurrentDisplay.SelectedShapes.BottomUp, CurrentDisplay.GridSize, false, backColor);
			else
				result = CurrentDisplay.Diagram.CreateImage(imageFormat, null, 0, true, backColor);
			return result;
		}


		private void ExportMetaFile(ImageFileFormat imageFormat) {
			saveFileDialog.Filter = "Enhanced Meta Files|*.emf|All Files|*.*";
			saveFileDialog.AutoUpgradeEnabled = (Environment.OSVersion.Version.Major >= 6);
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				using (Image image = GetImageFromDiagram(imageFormat)) {
					if (image != null) GdiHelpers.SaveImageToFile(image, saveFileDialog.FileName, imageFormat);
				}
			}
		}


		private void ExportBitmapFile(ImageFileFormat imageFormat) {
			string fileFilter = null;
			switch (imageFormat) {
				case ImageFileFormat.Bmp: fileFilter = "Bitmap Picture Files|*.bmp|All Files|*.*"; break;
				case ImageFileFormat.Gif: fileFilter = "Graphics Interchange Format Files|*.gif|All Files|*.*"; break;
				case ImageFileFormat.Jpeg: fileFilter = "Joint Photographic Experts Group (JPEG) Files|*.jpeg;*.jpg|All Files|*.*"; break;
				case ImageFileFormat.Png: fileFilter = "Portable Network Graphics Files|*.png|All Files|*.*"; break;
				case ImageFileFormat.Tiff: fileFilter = "Tagged Image File Format Files|*.tiff;*.tif|All Files|*.*"; break;
				default: throw new NShapeUnsupportedValueException(imageFormat);
			}
			saveFileDialog.Filter = fileFilter;
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				using (Image image = GetImageFromDiagram(imageFormat)) {
					if (image != null) GdiHelpers.SaveImageToFile(image, saveFileDialog.FileName, imageFormat, 100);
				}
			}
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "Edit"

		private void newDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			Diagram diagram = new Diagram(string.Format("Diagram {0}", displayTabControl.TabPages.Count + 1));
			diagram.Title = diagram.Name;
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
			CurrentDisplay.Copy(false);
			UpdateToolBarAndMenuItems();
		}


		private void copyShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Copy(true);
			UpdateToolBarAndMenuItems();
		}


		private void cutShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Cut(false);
			UpdateToolBarAndMenuItems();
		}


		private void cutShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Cut(true);
			UpdateToolBarAndMenuItems();
		}


		private void pasteMenuItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Paste();
			UpdateToolBarAndMenuItems();
		}


		private void deleteShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DeleteShapes(true);
			UpdateToolBarAndMenuItems();
		}


		private void deleteShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DeleteShapes(false);
			UpdateToolBarAndMenuItems();
		}


		private void historyTrackBar_ValueChanged(object sender, EventArgs e) {
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
			} catch (NShapeSecurityException exc) {
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
			} else
				// Undo was executed from context menu
				historyTrackBar.Value += 1;
		}


		private void redoItem_Click(object sender, EventArgs e) {
			System.Windows.Forms.ToolStripSplitButton button = (System.Windows.Forms.ToolStripSplitButton)((System.Windows.Forms.ToolStripItem)sender).OwnerItem;
			// Redo was executed from the main menu (DropDownList)
			if (button != null) {
				int idx = button.DropDownItems.IndexOf((System.Windows.Forms.ToolStripMenuItem)sender);
				historyTrackBar.Value -= idx + 1;
			} else
				// Redo was executed from context menu
				historyTrackBar.Value -= 1;
		}

		#endregion


		#region [Private] Event Handler implementations - Menu Item "View"

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


		private void debugDrawOccupationToolbarButton_Click(object sender, EventArgs e) {
#if DEBUG
			ShowCellOccupation = debugDrawOccupationToolbarButton.Checked;
#endif
		}


		private void debugDrawInvalidatedAreaToolbarButton_Click(object sender, EventArgs e) {
#if DEBUG
			ShowInvalidatedAreas = debugDrawInvalidatedAreaToolbarButton.Checked;
#endif
		}


		private void showDisplaySettingsItem_Click(object sender, EventArgs e) {
			using (DisplaySettingsForm dlg = new DisplaySettingsForm(this)) {
				dlg.ShowGrid = ShowGrid;
				dlg.SnapToGrid = SnapToGrid;
				dlg.GridColor = GridColor;
				dlg.GridSize = GridSize;
				dlg.SnapDistance = SnapDistance;
				dlg.ResizePointShape = ResizePointShape;
				dlg.ConnectionPointShape = ConnectionPointShape;
				dlg.ControlPointSize = ControlPointSize;
				dlg.ShowDefaultContextMenu = ShowDefaultContextMenu;
				dlg.HideDeniedMenuItems = HideDeniedMenuItems;

				if (dlg.ShowDialog(this) == DialogResult.OK) {
					ShowGrid = dlg.ShowGrid;
					GridColor = dlg.GridColor;
					showGridMenuItem.Checked = ShowGrid;
					showGridToolbarButton.Checked = ShowGrid;
					SnapToGrid = dlg.SnapToGrid;
					GridSize = dlg.GridSize;
					SnapDistance = dlg.SnapDistance;
					ResizePointShape = dlg.ResizePointShape;
					ConnectionPointShape = dlg.ConnectionPointShape;
					ControlPointSize = dlg.ControlPointSize;
					ShowDefaultContextMenu = dlg.ShowDefaultContextMenu;
					HideDeniedMenuItems = dlg.HideDeniedMenuItems;
				}
			}
		}


		private void showDiagramSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
			propertyController.SetObject(0, CurrentDisplay.Diagram);
		}


		private void editDesignsAndStylesToolStripMenuItem_Click(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}


		private void viewShowLayoutControlToolStripMenuItem_Click(object sender, EventArgs e) {
			if (layoutControlForm == null) {
				LayoutDialog lcf = new LayoutDialog();
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


		#region [Private] Event Handler implementations - Menu item "Tools"

		private void adoNetDatabaseGeneratorToolStripMenuItem_Click(object sender, EventArgs e) {
			if (project.Repository == null)
				MessageBox.Show(this, "No repository set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else if (!(project.Repository is CachedRepository))
				MessageBox.Show(this, string.Format("Repositories of type '{0}' are not supported by the database generator.", project.Repository.GetType().Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else {
				if (project.IsOpen) {
					string msgStr = "You are about to create a new database schema for a NShape database repository." + Environment.NewLine + Environment.NewLine;
					msgStr += "If you proceed, the current project will be closed and you will be asked for a database server and for choosing a set of NShape libraries." + Environment.NewLine;
					msgStr += "You can not save projects in the database using other than the selected libraries." + Environment.NewLine + Environment.NewLine;
					msgStr += "Do you want to proceed?";
					DialogResult result = MessageBox.Show(this, msgStr, "Create ADO.NET database schema", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result != DialogResult.Yes) return;
					if (!CloseProject()) return;
				}
				AdoNetStore store = GetAdoNetStore();
				if (store != null) {
					CachedRepository cachedReporitory = (CachedRepository)project.Repository;
					cachedReporitory.Store = store;
					project.RemoveAllLibraries();
					using (LibraryManagementDialog dlg = new LibraryManagementDialog(project))
						dlg.ShowDialog(this);
					project.RegisterEntityTypes();

					Cursor = Cursors.WaitCursor;
					Application.DoEvents();
					try {
						store.DropDbSchema();
						store.CreateDbCommands(cachedReporitory);
						store.CreateDbSchema(cachedReporitory);
						project.Close();
						MessageBox.Show(this, "Database schema created successfully.", "Schema created", MessageBoxButtons.OK, MessageBoxIcon.Information);
					} catch (Exception exc) {
						MessageBox.Show(this, "An error occured while creating database schema:" + Environment.NewLine + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					} finally {
						Cursor = Cursors.Default;
					}
				}
			}
		}


		private void nShapeEventMonitorToolStripMenuItem_Click(object sender, EventArgs e) {
			if (eventMoniorForm == null) {
				eventMoniorForm = new EventMonitorForm();
				eventMoniorForm.FormClosed += eventMoniorForm_FormClosed;
				try {
					// Display
					eventMoniorForm.AddEventSource(CurrentDisplay);
					// Project + History
					eventMoniorForm.AddEventSource(project);
					eventMoniorForm.AddEventSource(project.History);
					// Repository
					eventMoniorForm.AddEventSource(cachedRepository);
					// DiagramSetController
					eventMoniorForm.AddEventSource(diagramSetController);
					// ToolSetController + ToolSetPresenter + Tools
					eventMoniorForm.AddEventSource(toolSetController);
					eventMoniorForm.AddEventSource(toolSetListViewPresenter);
					foreach (Tool tool in toolSetController.Tools)
						eventMoniorForm.AddEventSource(tool);
					// PropertyController + PropertyPresenter
					eventMoniorForm.AddEventSource(propertyController);
					eventMoniorForm.AddEventSource(propertyPresenter);

					eventMoniorForm.AddEventSource(layerController);
					eventMoniorForm.AddEventSource(layerPresenter);

					if (modelTreeController != null)
						eventMoniorForm.AddEventSource(modelTreeController);
					if (modelTreePresenter != null)
						eventMoniorForm.AddEventSource(modelTreePresenter);

					eventMoniorForm.Show();
				} catch (Exception exc) {
					MessageBox.Show(this, exc.Message, "Error while opening EventMonitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
					eventMoniorForm.Close();
				}
			} else eventMoniorForm.Close();
			nShapeEventMonitorToolStripMenuItem.Checked = (eventMoniorForm != null);
		}


		private void eventMoniorForm_FormClosed(object sender, FormClosedEventArgs e) {
			eventMoniorForm.FormClosed -= eventMoniorForm_FormClosed;
			eventMoniorForm.Dispose();
			eventMoniorForm = null;
			nShapeEventMonitorToolStripMenuItem.Checked = (eventMoniorForm != null);
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "Help"

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			using (AboutBox dlg = new AboutBox())
				dlg.ShowDialog(this);
		}

		#endregion


		#region [Private] Types

		private struct RepositoryInfo {

			public static readonly RepositoryInfo Empty;


			public const string XmlStoreTypeName = "XML";

			
			public const string SqlServerStoreTypeName = "SQL Server";


			public static bool operator ==(RepositoryInfo a, RepositoryInfo b) {
				return (a.location == b.location
					&& a.projectName == b.projectName
					&& a.computerName == b.computerName
					&& a.typeName == b.typeName);
			}


			public static bool operator !=(RepositoryInfo a, RepositoryInfo b) { return !(a == b); }


			public static Store CreateStore(RepositoryInfo repositoryInfo) {
				Store store = null;
				if (repositoryInfo.typeName == RepositoryInfo.XmlStoreTypeName) {
					store = new XmlStore(Path.GetDirectoryName(repositoryInfo.location), Path.GetExtension(repositoryInfo.location));
				} else if (repositoryInfo.typeName == RepositoryInfo.SqlServerStoreTypeName) {
					store = new SqlStore();
					((SqlStore)store).DatabaseName = repositoryInfo.location;
					((SqlStore)store).ServerName = repositoryInfo.computerName;
				} else {
					Debug.Fail(string.Format("Unsupported {0} value '{1}'", typeof(RepositoryInfo).Name, repositoryInfo));
				}
				return store;
			}
			
			
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

		#endregion


		#region [Private] Constants

		private const string newProjectName = "New NShape Project";
		private const string appTitle = "NShape Designer";
		private const string sqlServerProviderName = "SQL Server";
		private const string defaultDatabaseName = "NShape";
		private const string defaultServerName = ".\\SQLEXPRESS";
		private const string fileFilterXmlRepository = "XML Repository Files|*.xml|All Files|*.*";

		private const string projectDirectoryTag = "ProjectDirectory";
		private const string pathTag = "Path";
		private const string projectsTag = "Projects";
		private const string projectTag = "Project";
		private const string projectNameTag = "Name";
		private const string typeNameTag = "RepositoryType";
		private const string serverNameTag = "ServerName";
		private const string dataSourceTag = "DataSource";
		private const int RecentProjectsLimit = 15;

		#endregion


		#region [Private] Fields

		EventMonitorForm eventMoniorForm = null;
		
		private string nShapeConfigDirectory = Path.Combine("dataweb", "NShape");
		private string xmlStoreDirectory;
		private bool projectSaved = false;

		private Point p;
		private int currHistoryPos;
		private Display currentDisplay;

		// ownerDisplay config
		private bool showGrid = true;
		private bool snapToGrid = true;
		private int gridSize = 20;
		private Color gridColor = Color.Gainsboro;
		private int snapDistance = 5;
		private ControlPointShape resizePointShape = ControlPointShape.Square;
		private ControlPointShape connectionPointShape = ControlPointShape.Circle;
		private int controlPointSize = 3;
#if DEBUG
		private bool showCellOccupation = false;
		private bool showInvalidatedAreas = false;
#endif

		private string configFile;
		private string configFolder;
		private int zoom = 100;
		private bool highQuality = true;
		private int historyDropDownItemCount = 20;

		private LayoutDialog layoutControlForm;
		private TemplateEditorDialog templateEditorDialog;

		private XmlWriter cfgWriter;
		private XmlReader cfgReader;
		private List<RepositoryInfo> recentProjects = new List<RepositoryInfo>();

#if TdbRepository
		private const string fileFilterAllRepositories = "NShape Repository Files|*.xml;*.tdbd|XML Repository Files|*.xml|TurboDB Repository Databases|*.tdbd|All Files|*.*";
		private const string fileFilterTurboDBRepository = "TurboDB Repository Databases|*.tdbd|All Files|*.*";
#endif
		#endregion
	}

}