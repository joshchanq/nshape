/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the nShape framework.
  nShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  nShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  nShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;


namespace nShapeViewer {

	public partial class FrameForm : Form {
		
		public FrameForm() {
			InitializeComponent();

			((DefaultSecurity)project.SecurityManager).CurrentRole = StandardRole.Guest;
		}

		
		private void project_Opened(object sender, EventArgs e) {
			foreach (IModelObject modelObject in project.Repository.GetModelObjects(null)) {
				modelTreeView.Visible = true;
				break;
			}
			
			Debug.Assert(displayChildForms.Count == 0);
			foreach (Diagram diagram in project.Repository.GetDiagrams()) {
				ShowDisplayChildForm(diagram);				
				
				ToolStripMenuItem item = new ToolStripMenuItem(diagram.Name);
				item.Click += openDiagramChildWindow;
				item.Enabled = !displayChildForms[diagram.Name].Visible;
				item.Tag = diagram;
				diagramsToolStripMenuItem.DropDownItems.Add(item);
			}

			openProjectToolStripMenuItem.Enabled = false;
			diagramsToolStripMenuItem.Enabled = true;
			closeProjectToolStripMenuItem.Enabled = true;
		}


		private void project_Closing(object sender, EventArgs e) {
			// Clear diagram menu items
			diagramsToolStripMenuItem.DropDownItems.Clear();

			// Close all open windows
			List<DisplayChildForm> childForms = new List<DisplayChildForm>(displayChildForms.Values);
			for (int i = childForms.Count - 1; i >= 0; --i) {
				if (childForms[i].Visible) childForms[i].Close();
				childForms[i].Dispose();
			}
			displayChildForms.Clear();
		}
		
		
		private void project_Closed(object sender, EventArgs e) {
			// Hide model tree
			if (modelTreeView.Visible) modelTreeView.Visible = false;

			openProjectToolStripMenuItem.Enabled = true;
			diagramsToolStripMenuItem.Enabled = true;
			closeProjectToolStripMenuItem.Enabled = false;
		}


		private void ShowDisplayChildForm(Diagram diagram) {
			DisplayChildForm childForm = new DisplayChildForm();
			RegisterChildFormEvents(childForm);
			childForm.MdiParent = this;
			childForm.Display.DiagramSetController = diagramSetController;
			childForm.Display.Diagram = diagram;
			childForm.Show();

			displayChildForms.Add(diagram.Name, childForm);
		}
		
		
		private void RegisterChildFormEvents(DisplayChildForm childForm) {
			childForm.FormClosing += childForm_FormClosing;
			childForm.FormClosed += childForm_FormClosed;
			childForm.Disposed += childForm_Disposed;
		}


		private void UnregisterChildFormEvents(DisplayChildForm childForm) {
			childForm.FormClosing -= childForm_FormClosing;
			childForm.FormClosed -= childForm_FormClosed;
			childForm.Disposed -= childForm_Disposed;
		}


		private void childForm_FormClosing(object sender, FormClosingEventArgs e) {
			// ToDo 
			// Ask: Are you sure?
		}


		private void childForm_FormClosed(object sender, FormClosedEventArgs e) {
			if (sender is DisplayChildForm) {
				DisplayChildForm childForm = (DisplayChildForm)sender;
				Diagram diagram = childForm.Display.Diagram;
				Debug.Assert(diagram != null);

				// Clean up
				UnregisterChildFormEvents(childForm);
				childForm.Display.Diagram = null;
				childForm.Display.DiagramSetController = null;
				displayChildForms.Remove(diagram.Name);

				// Endable menu item
				foreach (ToolStripMenuItem item in diagramsToolStripMenuItem.DropDownItems) {
					if (item.Tag == diagram) {
						item.Enabled = true;
						break;
					}
				}
			}
		}


		private void childForm_Disposed(object sender, EventArgs e) {
			if (sender is DisplayChildForm){
				Diagram diagram = ((DisplayChildForm)sender).Display.Diagram;
				if (diagram != null) displayChildForms.Remove(diagram.Name);
			}
		}


		#region Menu event handler implementations

		private void openMenuItem_Click(object sender, EventArgs e) {
			if (project.IsOpen) project.Close();
			openFileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\Demo Projects"));
			if (openFileDialog.ShowDialog(this) == DialogResult.OK) {
				xmlStore.DirectoryName = Path.GetDirectoryName(openFileDialog.FileName);
				project.Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
				if (project.LibrarySearchPaths.Count == 0)
					project.LibrarySearchPaths.Add(Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\Libraries")));
				project.Open();
			}
		}


		private void closeMenuItem_Click(object sender, EventArgs e) {
			if (project.IsOpen) project.Close();
		}


		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}


		private void openDiagramChildWindow(object sender, EventArgs e) {
			if (sender is ToolStripMenuItem) {
				ToolStripMenuItem item = (ToolStripMenuItem)sender;
				if (item.Tag is Diagram) {
					ShowDisplayChildForm((Diagram)item.Tag);
					item.Enabled = false;
				}
			}
		}
		
		
		private void horizontalToolStripMenuItem_Click(object sender, EventArgs e) {
			LayoutMdi(MdiLayout.TileHorizontal);
		}


		private void verticalToolStripMenuItem_Click(object sender, EventArgs e) {
			LayoutMdi(MdiLayout.TileVertical);
		}


		private void cascadeToolStripMenuItem_Click(object sender, EventArgs e) {
			LayoutMdi(MdiLayout.Cascade);
		}


		private void aboutDiagramViewerToolStripMenuItem_Click(object sender, EventArgs e) {
			using (AboutBox dlg = new AboutBox())
				dlg.ShowDialog(this);
		}

		#endregion


		private Dictionary<string, DisplayChildForm> displayChildForms = new Dictionary<string, DisplayChildForm>();
	}
}
