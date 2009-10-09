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
using Dataweb.NShape.Advanced;
using System.Data.SqlClient;


namespace Dataweb.NShape.Designer {

	public partial class OpenAdoNetRepositoryDialog : Form {

		public OpenAdoNetRepositoryDialog() {
			InitializeComponent();
		}


		public OpenAdoNetRepositoryDialog(string serverName, string databaseName, bool enableProjectName)
			: this() {
			serverNameTextBox.Text = serverName;
			databaseNameTextBox.Text = databaseName;
			projectNameComboBox.Visible =
				projectNameLabel.Visible = enableProjectName;
		}


		public string ProviderName {
			get { return providerNameComboBox.Text;}
		}
		
		
		public string ServerName {
			get { return serverNameTextBox.Text; }
		}


		public string DatabaseName {
			get { return databaseNameTextBox.Text; }
		}
		
		
		public string ProjectName {
			get { return projectNameComboBox.Text; }
		}


		private void OpenAdoNetRepositoryDialog_Load(object sender, EventArgs e) {
			FillProjectNameComboBox();
		}


		private void serverNameTextBox_TextChanged(object sender, EventArgs e) {
			ClearProjectNames();
		}


		private void databaseNameTextBox_TextChanged(object sender, EventArgs e) {
			ClearProjectNames();
		}


		private void providerNameComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			ClearProjectNames();
		}
		
		
		private void projectNameComboBox_DropDown(object sender, EventArgs e) {
			if (projectNameComboBox.Items.Count == 0)
				FillProjectNameComboBox();
		}


		private void ClearProjectNames() {
			if (projectNameComboBox.Visible) {
				projectNameComboBox.Items.Clear();
				projectNameComboBox.Text = string.Empty;
			}
		}


		private void FillProjectNameComboBox() {
			if (projectNameComboBox.Visible 
				&& !string.IsNullOrEmpty(ProviderName)
				&& !string.IsNullOrEmpty(ServerName)
				&& !string.IsNullOrEmpty(DatabaseName)) {
				string connectionString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True;MultipleActiveResultSets=True;Pooling=False", ServerName, DatabaseName);
				SqlConnection connection = new SqlConnection(connectionString);
				connection.Open();
				SqlCommand command = connection.CreateCommand();
				command.CommandText = "SELECT ProjectInfo.Name FROM Project INNER JOIN ProjectInfo ON Project.Id = ProjectInfo.Id ORDER BY LastSavedUtc DESC";
				try {
					using (IDataReader reader = command.ExecuteReader()) {
						projectNameComboBox.Items.Clear();
						while (reader.Read()) {
							string projectname = reader.GetString(reader.GetOrdinal("Name"));
							if (!string.IsNullOrEmpty(projectname)) {
								projectNameComboBox.Items.Add(projectname);
								if (string.IsNullOrEmpty(projectNameComboBox.Text))
									projectNameComboBox.Text = projectname;
							}
						}
					}
				} catch (SqlException) {
					// If the database server does not exist, the database does not exist 
					// or the database scheme does not exist, we will end up here.
				}
			}
		}
	}
}