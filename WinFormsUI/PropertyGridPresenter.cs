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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// Uses a Windows Property Grid to edit properties of shapes, diagramControllers and model objects.
	/// </summary>
	public class PropertyPresenter : Component {

		public PropertyPresenter() {
		}


		public PropertyPresenter(IPropertyController propertyController)
			: this() {
			if (propertyController == null) throw new ArgumentNullException("propertyController");
			this.PropertyController = propertyController;
		}


		[Category("NShape")]
		public IPropertyController PropertyController {
			get { return propertyController; }
			set {
				UnregisterPropertyControllerEvents();
				propertyController = value;
				RegisterPropertyControllerEvents();
			}
		}


		public int PageCount {
			get {
				int result = 0;
				if (primaryPropertyGrid != null) ++result;
				if (secondaryPropertyGrid != null) ++result;
				return result;
			}
		}
		
		
		public PropertyGrid PrimaryPropertyGrid {
			get { return primaryPropertyGrid; }
			set {
				UnregisterPropertyGridEvents(primaryPropertyGrid);
				primaryPropertyGrid = value;
				RegisterPropertyGridEvents(primaryPropertyGrid);
			}
		}


		public PropertyGrid SecondaryPropertyGrid {
			get { return secondaryPropertyGrid; }
			set {
				UnregisterPropertyGridEvents(secondaryPropertyGrid);
				secondaryPropertyGrid = value;
				RegisterPropertyGridEvents(secondaryPropertyGrid);
			}
		}


		private void GetPropertyGrid(int pageIndex, out PropertyGrid propertyGrid) {
			propertyGrid = null;
			switch (pageIndex) {
				case 0:
					propertyGrid = primaryPropertyGrid;
					break;
				case 1:
					propertyGrid = secondaryPropertyGrid;
					break;
				default: throw new IndexOutOfRangeException();
			}
		}


		//private void GetPropertyGrid(int pageIndex, out PropertyGrid propertyGrid, out Hashtable selectedObjectsList) {
		//   propertyGrid = null;
		//   selectedObjectsList = null;
		//   switch (pageIndex) {
		//      case 0:
		//         propertyGrid = primaryPropertyGrid;
		//         selectedObjectsList = selectedPrimaryObjectsList;
		//         break;
		//      case 1:
		//         propertyGrid = secondaryPropertyGrid;
		//         selectedObjectsList = selectedSecondaryObjectsList;
		//         break;
		//      default: Debug.Fail("PageIndex out of range."); break;
		//   }
		//   //if (propertyGrid == null) throw new IndexOutOfRangeException(string.Format("Property presenter has no PropertyGrid assigned for page {0}.", pageIndex));
		//}


		//private void GetPropertyGrid(int pageIndex, out PropertyGrid propertyGrid) {
		//   Hashtable list = null;
		//   GetPropertyGrid(pageIndex, out propertyGrid, out list);
		//}


		private void AssertControllerExists() {
			if (PropertyController == null) throw new InvalidOperationException("Property PropertyController is not set.");
		}
		
		
		private void RegisterPropertyControllerEvents() {
			if (propertyController != null) {
				propertyController.ObjectsModified += propertyController_RefreshObjects;
				propertyController.ObjectsSet += propertyController_ObjectsSet;
				propertyController.ProjectClosing += propertyController_ProjectClosing;
			}
		}


		private void UnregisterPropertyControllerEvents() {
			if (propertyController != null) {
				TypeDescriptionProviderDg.PropertyController = null;
				propertyController.ObjectsModified -= propertyController_RefreshObjects;
				propertyController.ObjectsSet -= propertyController_ObjectsSet;
				propertyController.ProjectClosing -= propertyController_ProjectClosing;
			}
		}


		private void RegisterPropertyGridEvents(PropertyGrid propertyGrid) {
			if (propertyGrid != null) {
				propertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
				propertyGrid.SelectedGridItemChanged += propertyGrid_SelectedGridItemChanged;
				propertyGrid.KeyDown += propertyGrid_KeyDown;
			}
		}


		private void UnregisterPropertyGridEvents(PropertyGrid propertyGrid) {
			if (propertyGrid != null) {
				propertyGrid.Enter -= propertyGrid_Enter;
				propertyGrid.Leave -= propertyGrid_Leave;
				propertyGrid.PropertyValueChanged -= propertyGrid_PropertyValueChanged;
				propertyGrid.SelectedGridItemChanged -= propertyGrid_SelectedGridItemChanged;
				propertyGrid.KeyDown -= propertyGrid_KeyDown;
			}
		}


		private void propertyGrid_Leave(object sender, EventArgs e) {
			TypeDescriptionProviderDg.PropertyController = null;
		}


		private void propertyGrid_Enter(object sender, EventArgs e) {
			TypeDescriptionProviderDg.PropertyController = propertyController;
		}


		private PropertyInfo GetPropertyInfo(PropertyGrid propertyGrid, GridItem item) {
			if (propertyGrid == null) throw new ArgumentNullException("propertyGrid");
			if (item == null) throw new ArgumentNullException("item");
			PropertyInfo result = null;
			int cnt = propertyGrid.SelectedObjects.Length;
			Type selectedObjectsType = propertyGrid.SelectedObject.GetType();
			// handle the case that the edited property is an expendable property, e.g. of type 'Font'. 
			// In this case, the property that has to be changed is not the edited item itself but it's parent item.
			if (item.Parent.PropertyDescriptor != null) {
				// the current selectedItem is a ChildItem of the edited object's property
				result = selectedObjectsType.GetProperty(item.Parent.PropertyDescriptor.Name);
			} else if (item.PropertyDescriptor != null)
				result = selectedObjectsType.GetProperty(item.PropertyDescriptor.Name);
			return result;
		}


		private void propertyController_ObjectsSet(object sender, PropertyControllerEventArgs e) {
			AssertControllerExists();

			propertyController.CancelSetProperty();
			if (propertyController.Project != null && propertyController.Project.IsOpen)
				StyleEditor.Design = propertyController.Project.Design;

			PropertyGrid grid = null;
			GetPropertyGrid(e.PageIndex, out grid);
			if (grid != null) {
				TypeDescriptionProviderDg.PropertyController = propertyController;
				grid.SelectedObjects = e.GetObjectArray();
				grid.Visible = true;
			}
		}


		private void propertyController_RefreshObjects(object sender, PropertyControllerEventArgs e) {
			AssertControllerExists();
			
			StyleEditor.Design = propertyController.Project.Design;
			PropertyGrid grid = null;
			GetPropertyGrid(e.PageIndex, out grid);
			if (grid == null) throw new IndexOutOfRangeException(string.Format("Property page {0} does not exist.", e.PageIndex));
			grid.SuspendLayout();
			grid.Refresh();
			grid.ResumeLayout();
		}


		private void propertyController_ProjectClosing(object sender, EventArgs e) {
			AssertControllerExists();
			
			propertyController.CancelSetProperty();
			if (primaryPropertyGrid != null) primaryPropertyGrid.SelectedObject = null;
			if (secondaryPropertyGrid != null) secondaryPropertyGrid.SelectedObject = null;
		}


		private void propertyGrid_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape && PropertyController != null) 
				propertyController.CancelSetProperty();
		}


		private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e) {
			AssertControllerExists();
			propertyController.CommitSetProperty();
		}


		private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e) {
			if (propertyController != null
				&& e.NewSelection != null
				&& e.OldSelection != null
				&& e.NewSelection != e.OldSelection) {
				propertyController.CancelSetProperty();
				TypeDescriptionProviderDg.PropertyController = propertyController;
			}
		}


		#region Fields

		private PropertyGrid primaryPropertyGrid;
		private PropertyGrid secondaryPropertyGrid;
		private IPropertyController propertyController;

		#endregion
	}
}
