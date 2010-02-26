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

using System.ComponentModel;
using System.Windows.Forms;

using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	public partial class LayerEditor : UserControl {

		public LayerEditor() {
			InitializeComponent();
		}


		[Category("NShape")]
		[Browsable(true)]
		public new string ProductVersion {
			get { return base.ProductVersion; }
		}


		[Category("NShape")]
		public IDiagramPresenter DiagramPresenter {
			get { return presenter.DiagramPresenter; }
			set { presenter.DiagramPresenter = value; }
		}


		[Category("NShape")]
		public DiagramSetController DiagramSetController {
			get { return controller.DiagramSetController; }
			set { controller.DiagramSetController = value; }
		}


		#region Fields
		#endregion
	}
}
