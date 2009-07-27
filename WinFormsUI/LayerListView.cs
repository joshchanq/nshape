using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Dataweb.Diagramming.Advanced;
using Dataweb.Diagramming.Controllers;


namespace Dataweb.Diagramming.WinFormsUI {
	
	public partial class LayerListView : ListView, ILayerView {

		public LayerListView() {
			DoubleBuffered = true;
			InitializeComponent();
			HeaderStyle = ColumnHeaderStyle.None;

			selectedBrush = new SolidBrush(Color.FromArgb(128, Color.Gainsboro));
			backgroundBrush = new SolidBrush(BackColor);
			textBrush = new SolidBrush(ForeColor);
			labelEditAllowed = LabelEdit;

			CreateColumns();
		}


		#region [Public] ILayerView Members

		#region Events

		public event EventHandler<LayersEventArgs> SelectedLayerChanged;

		public event EventHandler<LayerRenamedEventArgs> LayerRenamed;

		public event EventHandler<LayerZoomThresholdChangedEventArgs> LayerUpperZoomThresholdChanged;

		public event EventHandler<LayerZoomThresholdChangedEventArgs> LayerLowerZoomThresholdChanged;

		public event EventHandler<LayerMouseEventArgs> LayerViewMouseDown;

		public event EventHandler<LayerMouseEventArgs> LayerViewMouseMove;

		public event EventHandler<LayerMouseEventArgs> LayerViewMouseUp;

		#endregion


		void ILayerView.BeginUpdate() {
			SuspendLayout();
		}


		void ILayerView.EndUpdate() {
		   ResumeLayout();
		}

		
		void ILayerView.Clear() {
			ClearColumnsAndItems();
		}


		public void AddLayer(Layer layer, bool isActive, bool isVisible) {
			if (layer == null) throw new ArgumentNullException("layer");
			ListViewItem item = new ListViewItem(layer.Name);
			ListViewItem.ListViewSubItem subItem = null;
			for (int i = 0; i < Columns.Count; ++i) {
				if (i == idxColumnState)
					subItem = item.SubItems.Add(new ListViewItem.ListViewSubItem(item, layer.Name));
				else if (i == idxColumnName)
					item.SubItems.Add(new ListViewItem.ListViewSubItem(item, layer.Name));
				else if (i == idxColumnVisibility)
					item.SubItems.Add(new ListViewItem.ListViewSubItem(item, layer.Name));
				else if (i == idxColumnLowerZoomBound)
					item.SubItems.Add(new ListViewItem.ListViewSubItem(item, layer.LowerZoomThreshold.ToString()));
				else if (i == idxColumnUpperZoomBound)
					item.SubItems.Add(new ListViewItem.ListViewSubItem(item, layer.UpperZoomThreshold.ToString()));
			}
			item.Text = layer.Name;
			item.Tag = new LayerInfo(layer, isActive, isVisible);

			Items.Add(item);
			Refresh();
			//return Items.IndexOf(item);
		}


		public void RefreshLayer(Layer layer, bool isActive, bool isVisible) {
			if (layer == null) throw new ArgumentNullException("layer");
			oldName = newName = string.Empty;
			ListViewItem item = FindItem(layer);
			if (item != null) {
				item.Text = layer.Name;
				item.Tag = new LayerInfo(layer, isActive, isVisible);
				Invalidate(item.Bounds);
			}
		}


		public void RemoveLayer(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			ListViewItem item = FindItem(layer);
			if (item != null) {
				Items.Remove(item);
				Invalidate(item.Bounds);
			}
		}


		public void BeginEditLayerName(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			ListViewItem item = FindItem(layer);
			if (item != null) item.BeginEdit();
		}


		public void BeginEditLayerMinZoomBound(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			ListViewItem item = FindItem(layer);
			if (item != null) ShowUpDown(item, idxColumnLowerZoomBound);
		}


		public void BeginEditLayerMaxZoomBound(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			ListViewItem item = FindItem(layer);
			if (item != null) ShowUpDown(item, idxColumnUpperZoomBound);
		}


		public void OpenContextMenu(int x, int y, IEnumerable<DiagrammingAction> actions, Project project) {
			if (actions == null) throw new ArgumentNullException("actions");
			if (project == null) throw new ArgumentNullException("project");
			if (showDefaultContextMenu && contextMenuStrip != null) {
				contextMenuStrip.SuspendLayout();
				contextMenuStrip.Left = x;
				contextMenuStrip.Top = y;
				WinFormHelpers.BuildContextMenu(contextMenuStrip, actions, project, hideMenuItemsIfNotGranted);
				contextMenuStrip.Closing += contextMenuStrip_Closing;
				contextMenuStrip.Show(x, y);
				contextMenuStrip.ResumeLayout();
			}
		}

		
		void ILayerView.Invalidate() {
		   Invalidate();
		}

		#endregion


		#region [Protected] Overridden Methods

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (LayerViewMouseDown != null) 
				LayerViewMouseDown(this, GetMouseEventArgs(MouseEventType.MouseDown, e));
		}


		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (LayerViewMouseMove != null)
				LayerViewMouseMove(this, GetMouseEventArgs(MouseEventType.MouseMove, e));
		}


		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (LayerViewMouseUp!= null)
				LayerViewMouseUp(this, GetMouseEventArgs(MouseEventType.MouseUp, e));
		}


		protected override void OnSelectedIndexChanged(EventArgs e) {
			base.OnSelectedIndexChanged(e);
			if (SelectedLayerChanged != null) SelectedLayerChanged(this, new LayersEventArgs(GetSelectedLayers()));
		}


		protected override void OnBeforeLabelEdit(LabelEditEventArgs e) {
			base.OnBeforeLabelEdit(e);
			LayerInfo layerInfo = (LayerInfo)Items[e.Item].Tag;
			oldName = layerInfo.layer.Name;
			newName = string.Empty;
		}


		protected override void OnAfterLabelEdit(LabelEditEventArgs e) {
			base.OnAfterLabelEdit(e);
			newName = e.Label;
			if (newName != null && oldName != newName && LayerRenamed != null) {
				LayerInfo layerInfo = (LayerInfo)Items[e.Item].Tag;
				LayerRenamed(this, new LayerRenamedEventArgs(layerInfo.layer, oldName, newName));
			}
			oldName = newName = string.Empty;
			if (!labelEditAllowed) LabelEdit = false;
		}


		protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e) {
			e.DrawDefault = true;
			base.OnDrawColumnHeader(e);
		}
		
		
		protected override void OnDrawItem(DrawListViewItemEventArgs e) {
			base.OnDrawItem(e);

			Rectangle lineBounds = Rectangle.Empty;
			lineBounds.X = 0;
			lineBounds.Y = e.Bounds.Y;
			lineBounds.Width = Width;
			lineBounds.Height = e.Bounds.Height;

			if (e.ItemIndex % 2 == 0)
				backgroundBrush = Brushes.AliceBlue;
			else 
				backgroundBrush = Brushes.White;
			e.Graphics.FillRectangle(backgroundBrush, lineBounds);
			
			// This is a workaround for the disappearing subitems
			// ToDo: Find out why subitems keep disappearing and find a better solution than this
			for (int i = 0; i < e.Item.SubItems.Count; ++i)
				OnDrawSubItem(new DrawListViewSubItemEventArgs(e.Graphics, e.Bounds, e.Item, e.Item.SubItems[i], e.ItemIndex, i, null, e.State));

			if (SelectedItems.Count > 0 && e.Item.Selected)
				e.Graphics.FillRectangle(selectedBrush, e.Bounds);
		}


		protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e) {
			LayerInfo layerInfo = (LayerInfo)e.Item.Tag;
			int imgIdx;
			if (e.ColumnIndex == idxColumnState) {
				imgIdx = layerInfo.isActive ? 1 : 0;
				e.Graphics.DrawImageUnscaled(stateImageList.Images[imgIdx], e.SubItem.Bounds.Location);
			} else if (e.ColumnIndex == idxColumnVisibility) {
				imgIdx = layerInfo.isVisible ? 1 : 0;
				e.Graphics.DrawImageUnscaled(visibilityImageList.Images[imgIdx], e.SubItem.Bounds.Location);
			} else if (e.ColumnIndex == idxColumnName) {
				Rectangle bounds = e.Item.GetBounds(ItemBoundsPortion.Label);
				e.Graphics.DrawString(e.Item.Text, Font, textBrush, bounds);
			} else if (e.ColumnIndex == idxColumnLowerZoomBound) {
				string txt;
				if (layerInfo.layer.LowerZoomThreshold == int.MinValue)
					txt = float.NegativeInfinity.ToString();
				else txt = string.Format("{0:D1} %",layerInfo.layer.LowerZoomThreshold);
				e.Graphics.DrawString(txt, Font, textBrush, e.SubItem.Bounds);
			} else if (e.ColumnIndex == idxColumnUpperZoomBound) {
				string txt;
				if (layerInfo.layer.UpperZoomThreshold == int.MaxValue)
					txt = float.PositiveInfinity.ToString();
				else txt = string.Format("{0:D1} %", layerInfo.layer.UpperZoomThreshold);
				e.Graphics.DrawString(txt, Font, textBrush, e.SubItem.Bounds);
			} else
				e.DrawDefault = true;
			base.OnDrawSubItem(e);
		}

		#endregion


		#region [Private] Methods

		private IEnumerable<Layer> GetSelectedLayers() {
			for (int i = 0; i < SelectedItems.Count; ++i)
				yield return ((LayerInfo)SelectedItems[i].Tag).layer;
		}


		private LayerMouseEventArgs GetMouseEventArgs(MouseEventType eventType, MouseEventArgs eventArgs) {
			// Find clicked layer item
			Layer layer;
			LayerItem layerItem = LayerItem.None;
			// Find clicked item
			ListViewItem item = GetItemAt(eventArgs.X, eventArgs.Y);
			if (item == null)
				layer = null;
			else {
				// Set Layer
				layer = ((LayerInfo)item.Tag).layer;
				// Find matching subItem
				for (int i = Columns.Count - 1; i >= 0; --i) {
					// get subItem's bounds
					Rectangle subItemBounds = item.SubItems[i].Bounds;
					// if click was inside the subItem's bounds, determine item type
					if (Geometry.RectangleContainsPoint(subItemBounds, eventArgs.Location)) {
						if (i == idxColumnState) {
							layerItem = LayerItem.ActiveState;
							break;
						} else if (i == idxColumnVisibility) {
							layerItem = LayerItem.Visibility;
							break;
						} else if (i == idxColumnName) {
							// Check if the click was inside the layer name's text bounds
							Size size = TextRenderer.MeasureText(item.SubItems[i].Text, Font);
							if (Geometry.RectangleContainsPoint(eventArgs.X, eventArgs.Y, item.SubItems[i].Bounds.X, item.SubItems[i].Bounds.Y, size.Width, size.Height))
								layerItem = LayerItem.Name;
							break;
						} else if (i == idxColumnLowerZoomBound) {
							layerItem = LayerItem.MinZoom;
							break;
						} else if (i == idxColumnUpperZoomBound) {
							layerItem = LayerItem.MaxZoom;
							break;
						}
					}
				}
			}
			// Create EventArgs and fire event
			layerItemMouseEventArgs.SetMouseEvent(layer, layerItem, eventType, eventArgs);
			return layerItemMouseEventArgs;
		}


		private ListViewItem FindItem(Layer layer) {
			ListViewItem result = null;
			for (int i = 0; i < Items.Count; ++i) {
				if (layer  == ((LayerInfo)Items[i].Tag).layer) {
					result = Items[i];
					break;
				}
			}
			return result;
		}


		private void CreateColumns() {
			SuspendLayout();
			Items.Clear();

			// first, create Columns
			Columns.Clear();
			Columns.Add(keyColumnName, "Name", 100);
			Columns.Add(keyColumnState, "Active", 17);
			Columns.Add(keyColumnVisibility, "Visible", 17);
			Columns.Add(keyColumnLowerZoomBound, "LowerZoomBound", 50);
			Columns.Add(keyColumnUpperZoomBound, "UpperZoomBound", 50);

			idxColumnState = Columns.IndexOfKey(keyColumnState);
			idxColumnVisibility = Columns.IndexOfKey(keyColumnVisibility);
			idxColumnName = Columns.IndexOfKey(keyColumnName);
			idxColumnLowerZoomBound = Columns.IndexOfKey(keyColumnLowerZoomBound);
			idxColumnUpperZoomBound = Columns.IndexOfKey(keyColumnUpperZoomBound);

			SetNumericUpdown(Columns[keyColumnLowerZoomBound]);
			SetNumericUpdown(Columns[keyColumnUpperZoomBound]);

			ResumeLayout();
			Invalidate();
		}


		private void ClearColumnsAndItems() {
			SuspendLayout();
			// Clear items
			Items.Clear();
			// Clear columns
			Columns.Clear();
			CreateColumns();
			//
			Invalidate();
			ResumeLayout();
		}


		private void SetNumericUpdown(ColumnHeader columnHeader) {
			NumericUpDown upDown = new NumericUpDown();
			upDown.Visible = false;
			upDown.Parent = this;
			upDown.Minimum = 0;
			upDown.Maximum = int.MaxValue;
			columnHeader.Tag = upDown;
		}


		private void ShowUpDown(ListViewItem item, int columnIndex) {
			LayerInfo layerInfo = (LayerInfo)item.Tag;
			int value;
			string columnKey;
			if (columnIndex == idxColumnUpperZoomBound) {
				columnKey = keyColumnUpperZoomBound;
				value = layerInfo.layer.UpperZoomThreshold;
			} else {
				columnKey = keyColumnLowerZoomBound;
				value = layerInfo.layer.LowerZoomThreshold;
			}

			NumericUpDown upDown = (NumericUpDown)Columns[columnKey].Tag;
			Rectangle bounds = item.SubItems[columnIndex].Bounds;
			upDown.Bounds = bounds;
			upDown.Top = (int)Math.Round(bounds.Top + (bounds.Height / 2f) - (upDown.Height / 2f));
			upDown.ValueChanged += upDown_ValueChanged;
			upDown.Leave += upDown_Leave;
			upDown.Tag = layerInfo.layer;
			upDown.Value = value;
			upDown.Visible = true;
			upDown.Focus();
		}


		private void upDown_Leave(object sender, EventArgs e) {
			if (sender is NumericUpDown) {
				NumericUpDown upDown = (NumericUpDown)sender;
				Layer layer = (Layer)upDown.Tag;
				
				upDown.Tag = null;
				upDown.Leave -= upDown_Leave;
				upDown.ValueChanged -= upDown_ValueChanged;
				upDown.Visible = false;

				if (sender == Columns[keyColumnLowerZoomBound].Tag) {
					if (LayerLowerZoomThresholdChanged != null)
						LayerLowerZoomThresholdChanged(this, new LayerZoomThresholdChangedEventArgs(layer, layer.LowerZoomThreshold, (int)upDown.Value));
				} else if (sender == Columns[keyColumnUpperZoomBound].Tag) {
					if (LayerUpperZoomThresholdChanged != null)
						LayerUpperZoomThresholdChanged(this, new LayerZoomThresholdChangedEventArgs(layer, layer.UpperZoomThreshold, (int)upDown.Value));
				}

			} else { }
		}

		
		private void upDown_ValueChanged(object sender, EventArgs e) {
			//
		}


		private void contextMenuStrip_Closing(object sender, CancelEventArgs e) {
			if (showDefaultContextMenu && sender == contextMenuStrip) {
				contextMenuStrip.Closing -= contextMenuStrip_Closing;
				WinFormHelpers.CleanUpContextMenu(this.contextMenuStrip);
			}
		}

		#endregion


		#region [Private] Types

		private struct LayerInfo {

			public LayerInfo(Layer layer, bool isActive, bool isVisible) {
				this.layer = layer;
				this.isActive = isActive;
				this.isVisible = isVisible;
			}

			public Layer layer;

			public bool isActive;

			public bool isVisible;
		}


		private class LayerListViewMouseEventArgs : LayerMouseEventArgs {
			public LayerListViewMouseEventArgs(Layer layer, LayerItem item,
				MouseEventType eventType, DiagrammingMouseButtons buttons, int clickCount, int wheelDelta,
				Point position, DiagrammingKeys modifiers)
				: base(layer, item, eventType, buttons, clickCount, wheelDelta, position, modifiers) {
			}


			protected internal LayerListViewMouseEventArgs()
				: base() { }
		

			protected internal void SetMouseEvent(Layer layer, LayerItem item, MouseEventType eventType, MouseEventArgs eventArgs) {
				this.SetMouseEvent(eventType, (DiagrammingMouseButtons)eventArgs.Button, eventArgs.Clicks, eventArgs.Delta, eventArgs.Location, (DiagrammingKeys)Control.ModifierKeys);
				this.Layer = layer;
				this.Item = item;
			}


			protected internal void SetMouseEvent(Layer layer, LayerItem item, MouseEventType eventType, DiagrammingMouseEventArgs eventArgs) {
				this.SetMouseEvent(eventType, eventArgs.Buttons, eventArgs.Clicks, eventArgs.WheelDelta, eventArgs.Position, eventArgs.Modifiers);
				this.Item = item;
				this.Layer = layer;
			}
		}

		#endregion


		#region Fields

		private const string keyColumnState = "StateColumn";
		private const string keyColumnVisibility = "VisibilityColumn";
		private const string keyColumnName = "NameColumn";
		private const string keyColumnLowerZoomBound = "LowerZoomThresholdColumn";
		private const string keyColumnUpperZoomBound = "UpperZoomThresholdColumn";

		private int idxColumnState = -1;
		private int idxColumnVisibility = -1;
		private int idxColumnName = -1;
		private int idxColumnLowerZoomBound = -1;
		private int idxColumnUpperZoomBound = -1;

		private string oldName;
		private string newName;
		private bool labelEditAllowed;
		private bool showDefaultContextMenu = true;
		private bool hideMenuItemsIfNotGranted = true;

		// prawing and painting
		Brush selectedBrush = new SolidBrush(Color.FromArgb(128, Color.Gainsboro));
		Brush backgroundBrush = Brushes.White;
		Brush textBrush;

		LayerListViewMouseEventArgs layerItemMouseEventArgs = new LayerListViewMouseEventArgs();

		#endregion
	}
}
