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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// Connects any ListView to a NShape toolbox.
	/// </summary>
	public partial class ToolSetListViewPresenter : Component {
		
		public ToolSetListViewPresenter() {
			InitializeObjects();
			InitializeComponent();
		}


		public ToolSetListViewPresenter(IContainer container) {
			container.Add(this);
			InitializeObjects();
			InitializeComponent();
		}


		[Category("NShape")]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		[Category("NShape")]
		public ToolSetController ToolSetController {
			get { return toolSetController; }
			set {
				if (toolSetController != null) {
					UnregisterToolBoxEventHandlers();
					listView.ContextMenuStrip = null;
				}
				toolSetController = value;
				if (toolSetController != null) {
					if (listView != null && listView.ContextMenuStrip == null)
						listView.ContextMenuStrip = adapterContextMenu;
					RegisterToolBoxEventHandlers();
				}
			}
		}


		[Category("NShape")]
		public ListView ListView {
			get { return listView; }
			set {
				if (listView != null) {
					UnregisterListViewEventHandlers();
					listView.ContextMenuStrip = null;
				}
				listView = value;
				if (listView != null) {
					listView.HeaderStyle = ColumnHeaderStyle.None;
					ColumnHeader header = new ColumnHeader();
					header.Name = headerName;
					header.Text = headerName;
					if (listView.View == View.Details)
						header.Width = listView.Width - SystemInformation.VerticalScrollBarWidth - (SystemInformation.Border3DSize.Width * 2) - listView.Padding.Horizontal;
					listView.Columns.Add(header);
					listView.MultiSelect = false;
					listView.FullRowSelect = true;
					listView.ShowItemToolTips = true;
					listView.ShowGroups = true;
					listView.LabelEdit = false;
					listView.HideSelection = false;
					listView.SmallImageList = smallImageList;
					listView.LargeImageList = largeImageList;
					if (listView.ContextMenu == null) listView.ContextMenuStrip = adapterContextMenu;

					RegisterListViewEventHandlers();
				}
			}
		}


		/// <summary>
		/// Specifies if MenuItemDefs that are not granted should appear as MenuItems in the dynamic context menu.
		/// </summary>
		[Category("Behavior")]
		public bool HideDeniedMenuItems {
			get { return hideMenuItemsIfNotGranted; }
			set { hideMenuItemsIfNotGranted = value; }
		}


		/// <summary>
		/// Specifies if MenuItemDefs that are not granted should appear as MenuItems in the dynamic context menu.
		/// </summary>
		[Category("Behavior")]
		public bool ShowDefaultContextMenu {
			get { return showDefaultContextMenu; }
			set { showDefaultContextMenu = value; }
		}


		/// <summary>
		/// Dynamically built standard context menu. Will be used automatically if 
		/// the assigned listView has no ContextMenuStrip of its own.
		/// </summary>
		public ContextMenuStrip ContextMenuStrip {
			get { return adapterContextMenu; }
		}


		private void RegisterToolBoxEventHandlers() {
			toolSetController.Cleared += toolBoxController_Cleared;
			toolSetController.ToolAdded += toolBoxController_ToolAdded;
			toolSetController.ToolChanged += toolBoxController_ToolChanged;
			toolSetController.ToolRemoved += toolBoxController_ToolRemoved;
			toolSetController.ToolSelected += toolBoxController_ToolSelected;
		}


		private void UnregisterToolBoxEventHandlers() {
			toolSetController.ToolSelected -= toolBoxController_ToolSelected;
			toolSetController.ToolRemoved -= toolBoxController_ToolRemoved;
			toolSetController.ToolChanged -= toolBoxController_ToolChanged;
			toolSetController.ToolAdded -= toolBoxController_ToolAdded;
			toolSetController.Cleared -= toolBoxController_Cleared;
		}


		private void RegisterListViewEventHandlers() {
			listView.MouseDown += listView_MouseDown;
			listView.MouseUp += listView_MouseUp;
			listView.MouseDoubleClick += listView_MouseDoubleClick;
			listView.SizeChanged += listView_SizeChanged;
			listView.SelectedIndexChanged += listView_SelectedIndexChanged;
			listView.KeyDown += listView_KeyDown;
			if (listView.ContextMenuStrip != null) {
				listView.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
				listView.ContextMenuStrip.Closing += ContextMenuStrip_Closing;
			}
		}


		private void UnregisterListViewEventHandlers() {
			listView.KeyDown -= listView_KeyDown;
			listView.SelectedIndexChanged -= listView_SelectedIndexChanged;
			listView.SizeChanged -= listView_SizeChanged;
			listView.MouseDown -= listView_MouseDown;
			listView.MouseUp -= listView_MouseUp;
			listView.MouseDoubleClick -= listView_MouseDoubleClick;
			if (listView.ContextMenuStrip != null) {
				listView.ContextMenuStrip.Opening -= ContextMenuStrip_Opening;
				listView.ContextMenuStrip.Closing -= ContextMenuStrip_Closing;
			}
		}

		
		private void AssertListViewAvailable() {
			if (listView == null) throw new NShapeException("Toolbox requires a ListView.");
		}


		private void InitializeObjects() {
			smallImageList = new ImageList();
			smallImageList.ColorDepth = ColorDepth.Depth32Bit;
			smallImageList.ImageSize = new Size(smallImageSize, smallImageSize);
			smallImageList.TransparentColor = transparentColor;

			largeImageList = new ImageList();
			largeImageList.ColorDepth = ColorDepth.Depth32Bit;
			largeImageList.ImageSize = new Size(largeImageSize, largeImageSize);
			largeImageList.TransparentColor = transparentColor;
		}


		private ListViewItem FindItem(Tool tool) {
			ListViewItem result = null;
			foreach (ListViewItem lvi in listView.Items)
				if (lvi.Tag == tool) {
					result = lvi;
					break;
				}
			return result;
		}


		private ListViewItem CreateItem(Tool tool) {
			ListViewItem item = new ListViewItem(tool.Title, tool.Name);
			item.ToolTipText = tool.ToolTipText;
			item.Tag = tool;

			int imgIdx = smallImageList.Images.IndexOfKey(tool.Name);
			if (imgIdx < 0) {
				smallImageList.Images.Add(tool.Name, tool.SmallIcon);
				largeImageList.Images.Add(tool.Name, tool.LargeIcon);
				imgIdx = smallImageList.Images.IndexOfKey(tool.Name);
			}
			item.ImageIndex = imgIdx;

			return item;
		}


		private ListViewItem SelectedItem {
			get {
				ListViewItem result = null;
				if (listView != null && listView.SelectedItems.Count > 0)
					result = listView.SelectedItems[0];
				return result;
			}
		}


		#region Event handler implementations

		private void toolBoxController_ToolSelected(object sender, ToolEventArgs e) {
			if (listView != null) {
				ListViewItem item = FindItem(e.Tool);
				if (item != null && listView.SelectedItems.IndexOf(item) < 0)
					item.Selected = true;
			}
		}


		private void toolBoxController_Cleared(object sender, EventArgs args) {
			if (listView != null) {
				for (int i = listView.Items.Count - 1; i >= 0; --i) {
					Tool tool = (Tool)listView.Items[i].Tag;
					tool.Dispose();
					tool = null;
				}
				listView.Items.Clear();
				listView.Groups.Clear();
				smallImageList.Images.Clear();
				largeImageList.Images.Clear();
			}
		}


		private void toolBoxController_ToolAdded(object sender, ToolEventArgs e) {
			// SaveChanges the list view: Move this to ToolSetListViewPresenter
			if (listView != null) {
				if (FindItem(e.Tool) != null)
					throw new NShapeException(string.Format("Tool {0} already exists.", e.Tool.Title));
				ListViewItem item = CreateItem(e.Tool);
				// Put the tool into the right group
				if (!string.IsNullOrEmpty(e.Tool.Category)) {
					foreach (ListViewGroup group in listView.Groups) {
						if (group.Name.Equals(e.Tool.Category, StringComparison.InvariantCultureIgnoreCase)) {
							item.Group = group;
							break;
						}
					}
					if (item.Group == null) {
						ListViewGroup group = new ListViewGroup(e.Tool.Category, e.Tool.Category);
						listView.Groups.Add(group);
						item.Group = group;
					}
				}
				// Adjust the heading column in the list view
				if (listView.Columns[headerName] != null) {
					Graphics gfx = Graphics.FromHwnd(listView.Handle);
					if (gfx != null) {
						SizeF txtSize = gfx.MeasureString(e.Tool.Title, listView.Font);
						if (listView.Columns[headerName].Width < txtSize.Width + listView.SmallImageList.ImageSize.Width)
							listView.Columns[headerName].Width = (int)Math.Ceiling(txtSize.Width + listView.SmallImageList.ImageSize.Width);
						gfx.Dispose();
						gfx = null;
					}
				}
				// Add the item and select if default
				listView.Items.Add(item);
			}
		}


		private void toolBoxController_ToolChanged(object sender, ToolEventArgs e) {
			if (listView != null && e.Tool is TemplateTool) {
				listView.BeginUpdate();
				ListViewItem item = FindItem(e.Tool);
				if (item != null) {
					TemplateTool tool = (TemplateTool)e.Tool;
					item.Text = tool.Title;
					largeImageList.Images[item.ImageIndex] = tool.LargeIcon;
					smallImageList.Images[item.ImageIndex] = tool.SmallIcon;
				}
				listView.EndUpdate();
			}
		}


		private void toolBoxController_ToolRemoved(object sender, ToolEventArgs e) {
			listView.SuspendLayout();
			ListViewItem item = FindItem(e.Tool);
			listView.Items.Remove(item);
			listView.ResumeLayout();
		}


		private void listView_KeyDown(object sender, KeyEventArgs e) {
			if (toolSetController.SelectedTool != null && e.KeyCode == Keys.Escape) {
				toolSetController.SelectedTool.Cancel();
				if (SelectedItem != null && SelectedItem.Tag != toolSetController.DefaultTool)
					SelectedItem.EnsureVisible();
			}
		}


		private void listView_SelectedIndexChanged(object sender, EventArgs e) {
			if (listView.SelectedItems.Count > 0 && !keepLastSelectedItem)
				toolSetController.SelectTool((Tool)listView.SelectedItems[0].Tag, false);
		}


		private void listView_SizeChanged(object sender, EventArgs e) {
			ColumnHeader header = listView.Columns[headerName];
			if (header != null && listView.View == View.Details)
				header.Width = listView.Width - 10;
		}


		private void listView_MouseDoubleClick(object sender, MouseEventArgs e) {
			ListViewHitTestInfo hitTestInfo = ListView.HitTest(e.Location);
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
			   if (hitTestInfo.Item != null)
			      toolSetController.SelectTool((Tool)hitTestInfo.Item.Tag, true);
			}
		}


		private void listView_MouseUp(object sender, MouseEventArgs e) {
			if (keepLastSelectedItem && lastSelectedItem != null) {
				keepLastSelectedItem = false;
				listView.SelectedIndices.Clear();
				lastSelectedItem.Selected = true;
			}
		}


		private void listView_MouseDown(object sender, MouseEventArgs e) {
			ListViewHitTestInfo hitTestInfo = ListView.HitTest(e.Location);
			//if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
				if (hitTestInfo.Item != null && !listView.SelectedItems.Contains(hitTestInfo.Item))
					toolSetController.SelectTool((Tool)hitTestInfo.Item.Tag, false);
			//} else if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
			//   keepLastSelectedItem = true;
			//   if (listView.SelectedItems.Count > 0)
			//      lastSelectedItem = FindItem(ToolSetController.DefaultTool);
			//}
		}


		private void ContextMenuStrip_Opening(object sender, CancelEventArgs e) {
			if (showDefaultContextMenu && listView != null && listView.ContextMenu == null) {
				Point mousePos = listView.PointToClient(Control.MousePosition);
				ListViewHitTestInfo hitTestInfo = ListView.HitTest(mousePos);
				Tool clickedTool = null;
				if (hitTestInfo.Item != null) clickedTool = hitTestInfo.Item.Tag as Tool;
				
				if (toolSetController == null) throw new ArgumentNullException("ToolSetController");
				WinFormHelpers.BuildContextMenu(adapterContextMenu, toolSetController.GetMenuItemDefs(clickedTool), toolSetController.Project, hideMenuItemsIfNotGranted);
			}
			e.Cancel = adapterContextMenu.Items.Count == 0;
		}


		private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
			if (sender == adapterContextMenu)
				WinFormHelpers.CleanUpContextMenu(adapterContextMenu);
			ToolSetController.SelectedTool = ToolSetController.DefaultTool;
			e.Cancel = false;
		}

		#endregion


		#region Fields

		private const string headerName = "Name";
		private const int templateDefaultSize = 20;
		private const int imageMargin = 2;
		private const int smallImageSize = 16;
		private const int largeImageSize = 32;

		private ToolSetController toolSetController;
		private PropertyController propertyController;
		private ListView listView;

		// Settings
		private Color transparentColor = Color.White;
		private bool hideMenuItemsIfNotGranted = false;
		private bool showDefaultContextMenu = true;
		
		// buffers for preventing listview to select listview items on right click
		private bool keepLastSelectedItem = false;
		private ListViewItem lastSelectedItem = null;

		// Small images for tool with same index
		private ImageList smallImageList;

		// Large images for tool with same index
		private ImageList largeImageList;

		#endregion
	}
}
