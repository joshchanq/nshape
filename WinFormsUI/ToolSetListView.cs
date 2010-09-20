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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// Connects any ListView to a NShape toolbox.
	/// </summary>
	public partial class ToolSetListView : ListView, IToolSetView {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ToolSetListView" />.
		/// </summary>
		public ToolSetListView() {
			InitializeImageLists();
			InitializeComponent();

			HeaderStyle = ColumnHeaderStyle.None;
			ColumnHeader header = new ColumnHeader();
			header.Name = headerName;
			header.Text = headerName;
			if (View == View.Details)
				header.Width = Width - SystemInformation.VerticalScrollBarWidth - (SystemInformation.Border3DSize.Width * 2) - Padding.Horizontal;
			Columns.Add(header);
			MultiSelect = false;
			FullRowSelect = true;
			ShowItemToolTips = true;
			ShowGroups = true;
			LabelEdit = false;
			HideSelection = false;
			SmallImageList = smallImageList;
			LargeImageList = largeImageList;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ToolSetListView" />.
		/// </summary>
		public ToolSetListView(IContainer container)
			: this() {
			container.Add(this);
		}


		#region IToolSetView Members

		/// <override></override>
		public event EventHandler<SelectedToolChangedEventArgs> SelectedToolChanged;

		/// <override></override>
		public event EventHandler<ToolSetViewMouseEventArgs> ToolSetViewMouseDown;

		/// <override></override>
		public event EventHandler<ToolSetViewMouseEventArgs> ToolSetViewMouseMove;

		/// <override></override>
		public event EventHandler<ToolSetViewMouseEventArgs> ToolSetViewMouseUp;

		/// <override></override>
		public event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewPreviewKeyDown;

		/// <override></override>
		public event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewKeyDown;

		/// <override></override>
		public event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewKeyPress;

		/// <override></override>
		public event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewKeyUp;


		/// <override></override>
		public void AddToolItem(Tool tool, bool isDefaultTool) {
			if (tool == null) throw new ArgumentNullException("tool");
			BeginUpdate();
			ListViewItem toolItem = CreateItem(tool);
			Debug.Assert(toolItem != null);
			Items.Add(toolItem);
			if (isDefaultTool) defaultToolItem = toolItem;
			EndUpdate();
		}


		/// <override></override>
		public void RemoveToolItem(Tool tool) {
			BeginUpdate();
			if (tool == null) throw new ArgumentNullException("tool");
			int itemIdx = FindItemIndex(tool);
			if (Items[itemIdx] == defaultToolItem)
				defaultToolItem = null;
			Items.RemoveAt(itemIdx);
			EndUpdate();
		}


		/// <override></override>
		public void RefreshToolItem(Tool tool) {
			if (tool == null) throw new ArgumentNullException("tool");
			BeginUpdate();
			ListViewItem item = FindItem(tool);
			if (item != null) {
				item.Text = tool.Title;
				largeImageList.Images[item.ImageIndex] = tool.LargeIcon;
				smallImageList.Images[item.ImageIndex] = tool.SmallIcon;
			}
			EndUpdate();
		}


		/// <override></override>
		public void SelectToolItem(Tool tool) {
			if (tool == null) throw new ArgumentNullException("tool");
			SelectedIndices.Clear();
			int itemIdx = FindItemIndex(tool);
			if (itemIdx < 0) SelectedIndices.Add(itemIdx);
		}


		/// <override></override>
		public void EnsureVisibility(Tool tool) {
			if (tool == null) throw new ArgumentNullException("tool");
			ListViewItem item = FindItem(tool);
			if (item != null) item.EnsureVisible();
		}


		/// <override></override>
		public void OpenContextMenu(int x, int y, System.Collections.Generic.IEnumerable<MenuItemDef> contextMenuItemDefs, Project project) {
			if (!Geometry.IsValid(x, y)) throw new ArgumentException("x, y");
			if (contextMenuItemDefs == null) throw new ArgumentNullException("contextMenuItemDefs");
			if (project == null) throw new ArgumentNullException("project");
			if (showDefaultContextMenu) {
				defaultContextMenu.SuspendLayout();
				defaultContextMenu.Left = x;
				defaultContextMenu.Top = y;
				WinFormHelpers.BuildContextMenu(defaultContextMenu, contextMenuItemDefs, project, hideMenuItemsIfNotGranted);
				defaultContextMenu.Closing += ContextMenuStrip_Closing;
				defaultContextMenu.Show(x, y);
				defaultContextMenu.ResumeLayout();
			}
		}

		#endregion

		
		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[Category("NShape")]
		public new string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
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
		public override ContextMenuStrip ContextMenuStrip {
			get { return base.ContextMenuStrip ?? defaultContextMenu; }
			set { base.ContextMenuStrip = value; }
		}


		/// <override></override>
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if (ToolSetViewKeyDown != null)
				ToolSetViewKeyDown(this,
					new ToolSetViewKeyEventArgs(SelectedTool, WinFormHelpers.GetKeyEventArgs(KeyEventType.KeyDown, e)));
		}


		/// <override></override>
		protected override void OnSelectedIndexChanged(EventArgs e) {
			base.OnSelectedIndexChanged(e);
			if (SelectedToolChanged != null) {
				if (SelectedItems.Count > 0 && !keepLastSelectedItem)
					SelectedToolChanged(this, new SelectedToolChangedEventArgs(SelectedTool, false));
			}
		}


		/// <override></override>
		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			if (View == View.Details) {
				ColumnHeader header = Columns[headerName];
				if (header != null) header.Width = Width - 10;
			}
		}


		/// <override></override>
		protected override void OnMouseDoubleClick(MouseEventArgs e) {
			base.OnMouseDoubleClick(e);
			if (SelectedToolChanged != null && e.Button == MouseButtons.Left) {
				ListViewHitTestInfo hitTestInfo = HitTest(e.Location);
				if (hitTestInfo.Item != null)
					SelectedToolChanged(this, new SelectedToolChangedEventArgs((Tool)hitTestInfo.Item.Tag, true));
			}
		}


		/// <override></override>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (keepLastSelectedItem && lastSelectedItem != null) {
				keepLastSelectedItem = false;
				SelectedIndices.Clear();
				lastSelectedItem.Selected = true;
			}
		}


		/// <override></override>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			ListViewHitTestInfo hitTestInfo = HitTest(e.Location);
			if (e.Button == MouseButtons.Left) {
				if (hitTestInfo.Item != null && !SelectedItems.Contains(hitTestInfo.Item))
				if (SelectedToolChanged != null) 
					SelectedToolChanged(this, new SelectedToolChangedEventArgs((Tool)hitTestInfo.Item.Tag, false));
			} else if (e.Button == MouseButtons.Right) {
				keepLastSelectedItem = true;
				if (SelectedItems.Count > 0)
					lastSelectedItem = DefaultToolItem;
			}
		}


		#region [Private] Properties
		
		private ListViewItem SelectedItem {
			get {
				ListViewItem result = null;
				if (SelectedItems.Count > 0)
					result = SelectedItems[0];
				return result;
			}
		}


		private Tool SelectedTool {
			get { return (SelectedItem != null) ? SelectedItem.Tag as Tool : null; }
		}


		private ListViewItem DefaultToolItem {
			get { return defaultToolItem ?? ((Items.Count > 0) ? Items[0] : null); }
		}


		private Tool DefaultTool {
			get { return (DefaultToolItem == null) ? null : (Tool)DefaultToolItem.Tag; }
		}

		#endregion


		#region [Private] Methods

		private void InitializeImageLists() {
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
			int idx = FindItemIndex(tool);
			return (idx >= 0) ? Items[idx] : null;
		}


		private int FindItemIndex(Tool tool) {
			for (int i = Items.Count - 1; i >= 0; --i)
				if (Items[i].Tag == tool) return i;
			return -1;
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

		#endregion


		#region [Private] Event handler implementations

		private void toolBoxController_ToolSelected(object sender, ToolEventArgs e) {
			ListViewItem item = FindItem(e.Tool);
			if (item != null && SelectedItems.IndexOf(item) < 0)
				item.Selected = true;
		}


		private void toolBoxController_Cleared(object sender, EventArgs args) {
			for (int i = Items.Count - 1; i >= 0; --i) {
				Tool tool = (Tool)Items[i].Tag;
				tool.Dispose();
				tool = null;
			}
			Items.Clear();
			Groups.Clear();
			smallImageList.Images.Clear();
			largeImageList.Images.Clear();
		}


		private void toolBoxController_ToolAdded(object sender, ToolEventArgs e) {
			// SaveChanges the list view: Move this to ToolSetListView
			if (FindItem(e.Tool) != null)
				throw new NShapeException(string.Format("Tool {0} already exists.", e.Tool.Title));
			ListViewItem item = CreateItem(e.Tool);
			// Put the tool into the right group
			if (!string.IsNullOrEmpty(e.Tool.Category)) {
				foreach (ListViewGroup group in Groups) {
					if (group.Name.Equals(e.Tool.Category, StringComparison.InvariantCultureIgnoreCase)) {
						item.Group = group;
						break;
					}
				}
				if (item.Group == null) {
					ListViewGroup group = new ListViewGroup(e.Tool.Category, e.Tool.Category);
					Groups.Add(group);
					item.Group = group;
				}
			}
			// Adjust the heading column in the list view
			if (Columns[headerName] != null) {
				using (Graphics gfx = Graphics.FromHwnd(Handle)) {
					SizeF txtSize = gfx.MeasureString(e.Tool.Title, Font);
					if (Columns[headerName].Width < txtSize.Width + SmallImageList.ImageSize.Width)
						Columns[headerName].Width = (int)Math.Ceiling(txtSize.Width + SmallImageList.ImageSize.Width);
				}
			}
			// Add the item and select if default
			Items.Add(item);
		}


		private void toolBoxController_ToolChanged(object sender, ToolEventArgs e) {
			if (e.Tool is TemplateTool) {
				BeginUpdate();
				ListViewItem item = FindItem(e.Tool);
				if (item != null) {
					TemplateTool tool = (TemplateTool)e.Tool;
					item.Text = tool.Title;
					largeImageList.Images[item.ImageIndex] = tool.LargeIcon;
					smallImageList.Images[item.ImageIndex] = tool.SmallIcon;
				}
				EndUpdate();
			}
		}


		private void toolBoxController_ToolRemoved(object sender, ToolEventArgs e) {
			BeginUpdate();
			ListViewItem item = FindItem(e.Tool);
			Items.Remove(item);
			EndUpdate();
		}


		private void ContextMenuStrip_Opening(object sender, CancelEventArgs e) {
			if (showDefaultContextMenu)
				e.Cancel = defaultContextMenu.Items.Count == 0;
		}


		private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
			if (sender == defaultContextMenu)
				WinFormHelpers.CleanUpContextMenu(defaultContextMenu);
			if (SelectedToolChanged != null) 
				SelectedToolChanged(this, new SelectedToolChangedEventArgs(DefaultTool, true));
			e.Cancel = false;
		}

		#endregion


		#region Fields

		private const string headerName = "Name";
		private const int templateDefaultSize = 20;
		private const int imageMargin = 2;
		private const int smallImageSize = 16;
		private const int largeImageSize = 32;

		// Settings
		private Color transparentColor = Color.White;
		private bool hideMenuItemsIfNotGranted = false;
		private bool showDefaultContextMenu = true;
		
		// buffers for preventing listview to select listview items on right click
		private bool keepLastSelectedItem = false;
		private ListViewItem lastSelectedItem = null;
		private ListViewItem defaultToolItem = null;

		// Small images for tool with same index
		private ImageList smallImageList;
		// Large images for tool with same index
		private ImageList largeImageList;
		#endregion
	}

}
