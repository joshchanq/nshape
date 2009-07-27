using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming.WinFormsUI {
	
	public partial class LibraryManagementDialog : Form {

		public LibraryManagementDialog(Project project) {
			InitializeComponent();
			if (project == null) throw new ArgumentNullException("project");
			this.project = project;
		}

		
		private void LibraryManagementDialog_Load(object sender, EventArgs e) {
			project.LibraryLoaded += Project_LibraryLoaded;
			RefreshList();
		}


		private void LibraryManagementDialog_FormClosed(object sender, FormClosedEventArgs e) {
			project.LibraryLoaded -= Project_LibraryLoaded;
		}


		private void addLibraryButton_Click(object sender, EventArgs e) {
			openFileDialog.Filter = "Assembly Files|*.dll|All Files|*.*";
			openFileDialog.Multiselect = true;
			if (string.IsNullOrEmpty(openFileDialog.InitialDirectory))
				openFileDialog.InitialDirectory = Application.StartupPath;
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				foreach (string assemblyName in openFileDialog.FileNames) {
					try {
						project.AddLibraryByFilePath(assemblyName);
					}
					catch (Exception ex) {
						RefreshList();
						MessageBox.Show(ex.Message);
					}
				}
			}
		}


		private void closeButton_Click(object sender, EventArgs e) {
			this.Close();
		}


		void Project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			RefreshList();
		}


		private void RefreshList() {
			listBox1.SuspendLayout();
			listBox1.Items.Clear();
			foreach (Assembly assembly in project.Libraries)
				listBox1.Items.Add(assembly);
			listBox1.ResumeLayout();
		}


		private Project project;
	}
}