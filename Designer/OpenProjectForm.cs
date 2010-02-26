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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace Dataweb.NShape.Designer {

	public partial class OpenProjectForm : Form {
		private const string fileFilterAllRepositories = "XML Repository Files|*.xml|TurboDB Repository Databases|*.tdbd|All Files|*.*";


		public OpenProjectForm(Form owner, IEnumerable<string> lastUsedRepositories) {
			if (lastUsedRepositories == null) throw new ArgumentNullException("lastUsedRepositories");
			InitializeComponent();
			if (owner != null) {
				Owner = owner;
				Icon = Owner.Icon;
			}

			listBox1.Items.Clear();
			foreach (string repository in lastUsedRepositories) {
				listBox1.Items.Insert(0, repository);
			}
			newProjectRadioBtn.Checked = (listBox1.Items.Count == 0);
			openRecentProjectRadioBtn.Checked = (listBox1.Items.Count > 0);
			openRecentProjectRadioBtn.Enabled = (listBox1.Items.Count > 0);
		}


		public OpenProjectForm(IEnumerable<string> lastUsedRepositories) {
		}


		public string ProjectName {
			get {
				if (newProjectRadioBtn.Checked)
					return projectNameTextBox.Text;
				else
					return "";
			}
		}


		public string FileName {
			get {
				if (newProjectRadioBtn.Checked)
					return fileNameTextBox.Text;
				else if (openProjectRadioBtn.Checked)
					return existingFileNameTextBox.Text;
				else
					return listBox1.SelectedItem.ToString();
			}
		}


		public bool CreateNewProject {
			get { return newProjectRadioBtn.Checked; }
		}


		private void listBox1_MouseClick(object sender, MouseEventArgs e) {
			openRecentProjectRadioBtn.Checked = true;
		}


		private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (listBox1.SelectedIndex >= 0)
				okBtn_Click(null, null);
		}


		private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
			openRecentProjectRadioBtn.Checked = true;
		}


		private void fileNameTextBox_TextChanged(object sender, EventArgs e) {
			newProjectRadioBtn.Checked = true;
		}


		private void browseButton_Click(object sender, EventArgs e) {
			newProjectRadioBtn.Checked = true;

			saveFileDialog.Filter = fileFilterAllRepositories;
			saveFileDialog.FileName = string.Empty;
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
				fileNameTextBox.Text = saveFileDialog.FileName;
		}


		private void existingFileNameTextBox_TextChanged(object sender, EventArgs e) {
			openProjectRadioBtn.Checked = true;
		}


		private void browseLoadButton_Click(object sender, EventArgs e) {
			openProjectRadioBtn.Checked = true;

			openFileDialog.Filter = fileFilterAllRepositories;
			openFileDialog.FileName = string.Empty;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
				existingFileNameTextBox.Text = openFileDialog.FileName;
		}


		private void cancelBtn_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
		}


		private void okBtn_Click(object sender, EventArgs e) {
			bool error = false;
			if (newProjectRadioBtn.Checked) {
				if (projectNameTextBox.Text == "") {
					MessageBox.Show("Please enter a name for the new project.");
					projectNameTextBox.Focus();
					error = true;
				}
				if (fileNameTextBox.Text == "") {
					MessageBox.Show("Please choose a file for the project's repository.");
					fileNameTextBox.Focus();
					error = true;
				}
			} else if (openProjectRadioBtn.Checked) {
				if (string.IsNullOrEmpty(existingFileNameTextBox.Text)) {
					MessageBox.Show("Please choose a file for the project's repository.");
					existingFileNameTextBox.Focus();
					error = true;
				}
			} else {
				if (listBox1.SelectedIndex < 0) {
					MessageBox.Show("Please select a repository from the list.");
					listBox1.Focus();
					error = true;
				}
			}
			if (!error)
				DialogResult = DialogResult.OK;
		}
	}
}