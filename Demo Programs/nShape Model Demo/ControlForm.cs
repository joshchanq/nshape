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
using System.Windows.Forms;


namespace NShape_Model_Demo {

	public partial class ControlForm : Form {
		
		public ControlForm() {
			InitializeComponent();
		}


		public int Value {
			get { return trackBar.Value; }
		}


		public event EventHandler<EventArgs> ValueChanged;


		private void trackBar_Scroll(object sender, EventArgs e) {
			if (ValueChanged != null) ValueChanged(this, e);
		}
	}
}
