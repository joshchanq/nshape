using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Dataweb.Diagramming.Advanced;
using Dataweb.Diagramming.Controllers;
using System.Runtime.CompilerServices;
using Dataweb.Utilities;
using System.Collections;


namespace Dataweb.Diagramming.WinFormsUI {

	/// <summary>
	/// Uses a Windows Property Grid to edit properties of shapes, diagramControllers and model objects.
	/// </summary>
	public class PropertyPresenter : Component {

		public PropertyPresenter() {
		}


		public PropertyPresenter(IPropertyController propertyController)
			: this() {
			if (propertyController == null) throw new ArgumentNullException("propertyController");
			this.Controller = propertyController;
		}


		public IPropertyController Controller {
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
				if (value == null && primaryPropertyGrid != null) {
					primaryPropertyGrid.PropertyValueChanged -= propertyGrid_PropertyValueChanged;
					primaryPropertyGrid.SelectedGridItemChanged -= propertyGrid_SelectedGridItemChanged;
				}
				primaryPropertyGrid = value;
				if (primaryPropertyGrid != null) {
					primaryPropertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
					primaryPropertyGrid.SelectedGridItemChanged += propertyGrid_SelectedGridItemChanged;
				}
			}
		}


		public PropertyGrid SecondaryPropertyGrid {
			get { return secondaryPropertyGrid; }
			set {
				if (value == null && secondaryPropertyGrid != null) {
					secondaryPropertyGrid.PropertyValueChanged -= propertyGrid_PropertyValueChanged;
					secondaryPropertyGrid.SelectedGridItemChanged -= propertyGrid_SelectedGridItemChanged;
				}
				secondaryPropertyGrid = value;
				if (secondaryPropertyGrid != null) {
					secondaryPropertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
					secondaryPropertyGrid.SelectedGridItemChanged += propertyGrid_SelectedGridItemChanged;
				}
			}
		}


		private void GetPropertyGrid(int pageIndex, out PropertyGrid propertyGrid, out Hashtable selectedObjectsList) {
			propertyGrid = null;
			selectedObjectsList = null;
			switch (pageIndex) {
				case 0:
					propertyGrid = primaryPropertyGrid;
					selectedObjectsList = selectedPrimaryObjectsList;
					break;
				case 1:
					propertyGrid = secondaryPropertyGrid;
					selectedObjectsList = selectedSecondaryObjectsList;
					break;
				default: Debug.Fail("PageIndex out of range."); break;
			}
			if (propertyGrid == null) throw new IndexOutOfRangeException(string.Format("Property page {0} does not exist.", pageIndex));
		}


		private void GetPropertyGrid(int pageIndex, out PropertyGrid propertyGrid) {
			Hashtable list = null;
			GetPropertyGrid(pageIndex, out propertyGrid, out list);
		}
		
		
		private void RegisterPropertyControllerEvents() {
			if (propertyController != null) {
				propertyController.RefreshObjects += propertyController_RefreshObjects;
				propertyController.ObjectsSet += propertyController_ObjectsSet;
				propertyController.ObjectsDeleted += propertyController_ObjectsDeleted;
				propertyController.ProjectClosing += propertyController_ProjectClosing;
			}
		}


		private void UnregisterPropertyControllerEvents() {
			if (propertyController != null) {
				propertyController.RefreshObjects -= propertyController_RefreshObjects;
				propertyController.ObjectsSet -= propertyController_ObjectsSet;
				propertyController.ObjectsDeleted -= propertyController_ObjectsDeleted;
				propertyController.ProjectClosing -= propertyController_ProjectClosing;
			}
		}


		private void propertyController_ObjectsSet(object sender, PropertyControllerEventArgs e) {
			if (propertyController.Project != null && propertyController.Project.IsOpen)
				DiagrammingStyleEditor.Design = propertyController.Project.Design;

			PropertyGrid grid = null;
			Hashtable list;
			GetPropertyGrid(e.PageIndex, out grid, out list);

			// Do not assign to grid.SelectedObjects because the PropertyGrid's indexer 
			// clones the object before returning it.
			// Btw: It seems that the PropertyGrid class was not designed for displaying 
			// large numbers of objects... :-/
			object[] selectedObjectsBuffer = e.GetObjectArray();
			list.Clear();
			for (int i = selectedObjectsBuffer.Length - 1; i >= 0; --i) {
				if (!list.Contains(selectedObjectsBuffer[i]))
					list.Add(selectedObjectsBuffer[i], null);
			}

			grid.SelectedObjects = selectedObjectsBuffer;
			grid.Visible = true;
		}


		private void propertyController_RefreshObjects(object sender, PropertyControllerEventArgs e) {
			DiagrammingStyleEditor.Design = propertyController.Project.Design;
			PropertyGrid grid = null;
			switch (e.PageIndex) {
				case 0: grid = primaryPropertyGrid; break;
				case 1: grid = secondaryPropertyGrid; break;
				default: Debug.Fail("PageIndex out of range."); break;
			}
			if (grid == null) throw new IndexOutOfRangeException(string.Format("Property page {0} does not exist.", e.PageIndex));
			grid.SuspendLayout();
			grid.Refresh();
			grid.ResumeLayout();
		}


		private void propertyController_ProjectClosing(object sender, EventArgs e) {
			if (primaryPropertyGrid != null) primaryPropertyGrid.SelectedObject = null;
			if (secondaryPropertyGrid != null) secondaryPropertyGrid.SelectedObject = null;
		}


		private void propertyController_ObjectsDeleted(object sender, PropertyControllerEventArgs e) {
			PropertyGrid grid = null;
			Hashtable list = null;
			GetPropertyGrid(e.PageIndex, out grid, out list);

			if (grid.SelectedObjects != null && grid.SelectedObjects.Length > 1) {
				foreach (object obj in e.Objects) {
					if (list.ContainsKey(obj))
						list.Remove(obj);
				}
				list.Keys.CopyTo(grid.SelectedObjects, 0);
			} else {
				foreach (object obj in e.Objects) {
					if (grid.SelectedObject == obj) {
						grid.SelectedObject = null;
						break;
					}
				}
			}
		}


		/// <summary>
		/// Store the original values of the selected objects before any changes are made. This ensures that all changes can be undone later.
		/// </summary>
		private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e) {
			PropertyGrid propertyGrid = (PropertyGrid)sender;
			int cnt = propertyGrid.SelectedObjects.Length;
			// Copy selected objects into a new object array because the indexer of
			// PropertyGrid's SelectedObject property clones the whole object array on each 'get' access.
			object[] selectedObjectsBuffer = propertyGrid.SelectedObjects;
			// clear the buffer for original values
			if (originalValues.Length > 0) Array.Resize(ref originalValues, 0);
			

			// handle the case that the edited property is an expendable property, e.g. of type 'Font'. 
			// In this case, the property that has to be changed is not the edited item itself but it's parent item.
			if (e.NewSelection.Parent.PropertyDescriptor != null) {
				// the current selectedItem is a ChildItem of the edited object's property
				Type selectedObjectsType = propertyGrid.SelectedObject.GetType();
				PropertyInfo propertyInfo = selectedObjectsType.GetProperty(e.NewSelection.Parent.PropertyDescriptor.Name);

				// if the original values are different, store them all...
				if (cnt > 1 && e.NewSelection.Value == null) {
					originalValues = new object[cnt];
					for (int i = 0; i < cnt; ++i)
						originalValues[i] = propertyInfo.GetValue(selectedObjectsBuffer[i], null);
				}

				// ... otherwise store the common value
				else {
					originalValues = new object[1];
					originalValues[0] = propertyInfo.GetValue(propertyGrid.SelectedObject, null);
				}
			//} else if (e.NewSelection.Value == null && e.NewSelection.Parent.PropertyDescriptor != null) {
			//   originalValues = new object[cnt];
			//   for (int i = 0; i < cnt; ++i) {
			//      Type selectedObjectsType = selectedObjectsBuffer[i].GetType();
			//      PropertyInfo propertyInfo = selectedObjectsType.GetProperty(e.NewSelection.PropertyDescriptor.Name);
				//      originalValues[i] = propertyInfo.GetValue(selectedObjectsBuffer[i], null);
			//   }
			} else if (e.NewSelection.PropertyDescriptor != null) {
				originalValues = new object[cnt];
				for (int i = 0; i < cnt; ++i) {
					Type selectedObjectsType = selectedObjectsBuffer[i].GetType();
					PropertyInfo propertyInfo = selectedObjectsType.GetProperty(e.NewSelection.PropertyDescriptor.Name);
					originalValues[i] = propertyInfo.GetValue(selectedObjectsBuffer[i], null);
				}
			}
		}


		/// <summary>
		/// Retrieve the changed value and notify the IPropertyWindow about the changes
		/// </summary>
		private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e) {
			if (propertyController == null) throw new DiagrammingException("Property 'PropertyController' is not set.");
			if (!(sender is PropertyGrid)) throw new ArgumentException("Argument 'sender' is not a PropertyGrid.");
			
			int pageIdx = -1;
			if (sender == primaryPropertyGrid) pageIdx = 0;
			else if (sender == secondaryPropertyGrid) pageIdx = 1;
			if (pageIdx < 0 || pageIdx > 2) throw new ArgumentException(string.Format("{0} is not assigned to {0}", sender.GetType().Name, this.GetType().Name));

			PropertyGrid propertyGrid = (PropertyGrid)sender;
			PropertyInfo propertyInfo = null;
			object[] oldValues = new object[1];
			object newValue = null;

			// handle properties that can be unfolded so that the changed item is not the selected item 
			// (e.g. if Font property is unfolded and Font.Size is changed)
			if (e.ChangedItem.Parent != null && e.ChangedItem.Parent.PropertyDescriptor != null) {
				Type selectedObjectsType = propertyGrid.SelectedObject.GetType();
				propertyInfo = selectedObjectsType.GetProperty(e.ChangedItem.Parent.PropertyDescriptor.Name);

				Debug.Assert(originalValues.Length > 0);
				oldValues[0] = new object[originalValues.Length];
				Array.Copy(originalValues, oldValues, originalValues.Length);
				newValue = e.ChangedItem.Parent.Value;
			}
			else {
				Type modifiedObjectsType = ((PropertyGrid)sender).SelectedObject.GetType();
				propertyInfo = modifiedObjectsType.GetProperty(e.ChangedItem.PropertyDescriptor.Name);

				// e.OldValue is null if more than one objects are selected and the modified 
				// properties did not have the same value
				if (propertyGrid.SelectedObjects.Length > 1 && e.OldValue == null && originalValues.Length > 0) {
					oldValues = new object[originalValues.Length];
					Array.Copy(originalValues, oldValues, originalValues.Length);
				}
				else
					oldValues[0] = e.OldValue;
				newValue = e.ChangedItem.Value;
			}
			propertyController.SelectedObjectsChanged(pageIdx, propertyGrid.SelectedObjects, propertyInfo, oldValues, e.ChangedItem.Value);
		}


		#region Fields

		private PropertyGrid primaryPropertyGrid;
		private PropertyGrid secondaryPropertyGrid;
		private IPropertyController propertyController;
		// buffer for original values:
		// If >1 shapes are selected, each original property value will be stored in this buffer when 
		// selecting a property. This ensures that changes of n properties to a common value can be undone.
		private object[] originalValues = new object[0];

		// Lists for fast comparing
		private Hashtable selectedPrimaryObjectsList = new Hashtable();
		private Hashtable selectedSecondaryObjectsList = new Hashtable();
		private List<object> bufferList = new List<object>();
		#endregion
	}
}
