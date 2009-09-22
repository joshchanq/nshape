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


		public string ProjectName {
			get { return projectNameTextBox.Text; }
		}


		private void OpenAdoNetRepositoryDialog_Load(object sender, EventArgs e) {
			serverNameTextBox.Text = Environment.MachineName;
			databaseNameTextBox.Text = "nShape";
		}
	}
}