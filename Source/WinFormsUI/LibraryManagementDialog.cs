/******************************************************************************
  Copyright 2009-2011 dataweb GmbH
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
using System.Reflection;
using System.Windows.Forms;


namespace Dataweb.NShape.WinFormsUI {
	
	/// <summary>
	/// Dialog used for loading NShape libraries into the current NShape project.
	/// </summary>
	public partial class LibraryManagementDialog : Form {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.LibraryManagementDialog" />.
		/// </summary>
		/// <param name="project"></param>
		public LibraryManagementDialog(Project project) {
			InitializeComponent();
			if (project == null) throw new ArgumentNullException("project");
			this.project = project;
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}

		
		private void LibraryManagementDialog_Load(object sender, EventArgs e) {
			project.LibraryLoaded += Project_LibraryLoaded;

			//libraryListView.Columns[0].Width = (int)Math.Round((libraryListView.Width / 3f) * 2) - 2;
			//libraryListView.Columns[1].Width = libraryListView.Width - libraryListView.Columns[0].Width - 2;

			RefreshList();
		}


		private void LibraryManagementDialog_Shown(object sender, EventArgs e) {
			bool librariesLoaded = false;
			foreach (Assembly a in project.Libraries) {
				librariesLoaded = true; 
				break;
			}
			if (!librariesLoaded) addLibraryButton_Click(this, null);
		}

	
		private void LibraryManagementDialog_FormClosed(object sender, FormClosedEventArgs e) {
			project.LibraryLoaded -= Project_LibraryLoaded;
		}


		private void addLibraryButton_Click(object sender, EventArgs e) {
			openFileDialog.Filter = "Assembly Files|*.dll|All Files|*.*";
			openFileDialog.FileName = "";
			openFileDialog.Multiselect = true;
			if (string.IsNullOrEmpty(openFileDialog.InitialDirectory))
				openFileDialog.InitialDirectory = Application.StartupPath;
			
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				// Repaint windows under the file dialog before starting with adding libraries
				Application.DoEvents();

				List<string> fileNames = new List<string>(openFileDialog.FileNames);
				fileNames.Sort();
				for (int i = 0; i < fileNames.Count; ++i) {
					try {
						if (!project.IsValidLibrary(fileNames[i])) {
							MessageBox.Show(this, string.Format(InvalidLibraryMessage, fileNames[i]), "Invalid file type", MessageBoxButtons.OK, MessageBoxIcon.Error);
						} else project.AddLibraryByFilePath(fileNames[i]);
					} catch (Exception ex) {
						RefreshList();
						MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}


		private void closeButton_Click(object sender, EventArgs e) {
			this.Close();
		}


		private void Project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			RefreshList();
		}


		private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = !project.IsValidLibrary(openFileDialog.FileName);
			string msg = string.Format(InvalidLibraryMessage, openFileDialog.FileName);
			if (e.Cancel) MessageBox.Show(this, msg, "Invalid file type", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}


		private void RefreshList() {
			libraryListView.SuspendLayout();
			
			libraryListView.Items.Clear();
			foreach (Assembly assembly in project.Libraries) {
				// Get assembly file path
				UriBuilder uriBuilder = new UriBuilder(assembly.CodeBase);
				string assemblyPath = Uri.UnescapeDataString(uriBuilder.Path);
				// Get assembly name (and version)
				AssemblyName assemblyName = assembly.GetName();

				ListViewItem item = new ListViewItem(assemblyName.Name);
				item.SubItems.Add(assemblyName.Version.ToString());
				item.SubItems.Add(assemblyPath);
				libraryListView.Items.Add(item);
			}

			libraryListView.ResumeLayout();
		}


		private Project project;
		private const string InvalidLibraryMessage = "'{0}' is not a valid NShape library.";
	}
}