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
using System.Diagnostics;
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

	/// <summary>
	/// Non-visual component implementing the functionality of a tool set presenter.
	/// Provides methods and properties for connecting an <see cref="T:Dataweb.NShape.COntrollers.IToolSetView" /> user interface widget with a <see cref="T:Dataweb.NShape.COntrollers.ToolSetController" />
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap("ToolSetPresenter.bmp")]
	public partial class ToolSetPresenter : Component {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter" />.
		/// </summary>
		public ToolSetPresenter() {
		}


		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[Category("NShape")]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// The controller of this presenter.
		/// </summary>
		[Category("NShape")]
		public ToolSetController ToolSetController {
			get { return toolSetController; }
			set {
				if (toolSetController != null) {
					ClearToolSetView();
					UnregisterControllerEventHandlers();
				}
				toolSetController = value;
				if (toolSetController != null) {
					RegisterControllerEventHandlers();
					FillToolSetView();
				}
			}
		}


		/// <summary>
		/// Specifies a ListView used as user interface for this presenter.
		/// </summary>
		[Category("NShape")]
		public IToolSetView ToolSetView {
			get { return toolSetView; }
			set {
				if (toolSetView != null) {
					ClearToolSetView();
					UnregisterViewEventHandlers();
				}
				toolSetView = value;
				if (toolSetView != null) {
					RegisterViewEventHandlers();
					FillToolSetView();
				}
			}
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[Category("NShape")]
		public Project Project {
			get { return (toolSetController == null) ? null : toolSetController.Project; }
		}


		/// <summary>
		/// Provides access to the currently selected <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		public Tool SelectedTool {
			get { return selectedTool; }
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


		#region [Private] Methods: (Un)Registering for events

		private void RegisterControllerEventHandlers() {
			toolSetController.Cleared += toolBoxController_Cleared;
			toolSetController.ToolAdded += toolBoxController_ToolAdded;
			toolSetController.ToolChanged += toolBoxController_ToolChanged;
			toolSetController.ToolRemoved += toolBoxController_ToolRemoved;
			toolSetController.ToolSelected += toolBoxController_ToolSelected;
		}


		private void UnregisterControllerEventHandlers() {
			toolSetController.ToolSelected -= toolBoxController_ToolSelected;
			toolSetController.ToolRemoved -= toolBoxController_ToolRemoved;
			toolSetController.ToolChanged -= toolBoxController_ToolChanged;
			toolSetController.ToolAdded -= toolBoxController_ToolAdded;
			toolSetController.Cleared -= toolBoxController_Cleared;
		}


		private void RegisterViewEventHandlers() {
			toolSetView.SelectedToolChanged += toolSetView_SelectedToolChanged;
			toolSetView.ToolSetViewMouseDown += toolSetView_ToolSetViewMouseDown;
			toolSetView.ToolSetViewMouseMove += toolSetView_ToolSetViewMouseMove;
			toolSetView.ToolSetViewMouseUp += toolSetView_ToolSetViewMouseUp;
			toolSetView.ToolSetViewPreviewKeyDown += toolSetView_ToolSetViewPreviewKeyDown;
			toolSetView.ToolSetViewKeyDown += toolSetView_ToolSetViewKeyDown;
			toolSetView.ToolSetViewKeyPress += toolSetView_ToolSetViewKeyPress;
			toolSetView.ToolSetViewKeyUp += toolSetView_ToolSetViewKeyUp;
		}


		private void UnregisterViewEventHandlers() {
			toolSetView.SelectedToolChanged -= toolSetView_SelectedToolChanged;
			toolSetView.ToolSetViewMouseDown -= toolSetView_ToolSetViewMouseDown;
			toolSetView.ToolSetViewMouseMove -= toolSetView_ToolSetViewMouseMove;
			toolSetView.ToolSetViewMouseUp -= toolSetView_ToolSetViewMouseUp;
			toolSetView.ToolSetViewPreviewKeyDown -= toolSetView_ToolSetViewPreviewKeyDown;
			toolSetView.ToolSetViewKeyDown -= toolSetView_ToolSetViewKeyDown;
			toolSetView.ToolSetViewKeyPress -= toolSetView_ToolSetViewKeyPress;
			toolSetView.ToolSetViewKeyUp -= toolSetView_ToolSetViewKeyUp;
		}

		#endregion


		#region [Private] Methods

		private void AssertToolSetViewAvailable() {
			if (toolSetView == null) throw new NShapeException("ToolSetController requires a ToolSetView.");
		}


		private void FillToolSetView() {
			if (toolSetView != null && toolSetController != null) {
				foreach (Tool tool in toolSetController.Tools)
					toolSetView.AddToolItem(tool, (tool == toolSetController.DefaultTool));
			}
		}


		private void ClearToolSetView() {
			if (toolSetView != null) toolSetView.Clear();
		}

		#endregion


		#region [Private] Event handler implementations

		private void toolBoxController_ToolSelected(object sender, ToolEventArgs e) {
			if (toolSetView != null) toolSetView.SelectToolItem(e.Tool);
		}


		private void toolBoxController_Cleared(object sender, EventArgs args) {
			ClearToolSetView();
		}


		private void toolBoxController_ToolAdded(object sender, ToolEventArgs e) {
			if (toolSetView != null) {
				toolSetView.BeginUpdate();
				toolSetView.AddToolItem(e.Tool, false);
				toolSetView.EndUpdate();
			}
		}


		private void toolBoxController_ToolChanged(object sender, ToolEventArgs e) {
			if (toolSetView != null) {
				toolSetView.BeginUpdate();
				ToolSetView.RefreshToolItem(e.Tool);
				toolSetView.EndUpdate();
			}
		}


		private void toolBoxController_ToolRemoved(object sender, ToolEventArgs e) {
			if (toolSetView != null) {
				toolSetView.BeginUpdate();
				toolSetView.RemoveToolItem(e.Tool);
				toolSetView.EndUpdate();
			}
		}


		private void toolSetView_ToolSetViewKeyUp(object sender, ToolSetViewKeyEventArgs e) {
			// Nothing to do here
		}


		private void toolSetView_ToolSetViewKeyPress(object sender, ToolSetViewKeyEventArgs e) {
			if (toolSetController.SelectedTool != null && e.KeyCode == (int)KeysDg.Escape) {
			   toolSetController.SelectedTool.Cancel();
			   if (SelectedTool != null && SelectedTool != toolSetController.DefaultTool)
			      toolSetView.EnsureVisibility(SelectedTool);
			}
		}


		private void toolSetView_ToolSetViewKeyDown(object sender, ToolSetViewKeyEventArgs e) {
			// Nothing to do here
		}


		private void toolSetView_ToolSetViewPreviewKeyDown(object sender, ToolSetViewKeyEventArgs e) {
			// Nothing to do here
		}


		private void toolSetView_ToolSetViewMouseUp(object sender, ToolSetViewMouseEventArgs e) {
			Debug.Assert(e.EventType == MouseEventType.MouseUp);
			if (toolSetView != null) {
				// Use a switch command here because we are not interested in button combinations
				switch (e.Buttons) {
					case MouseButtonsDg.Left:
						// Nothing to do here - tool selection is handled by SelectedToolChanged
						break;

					case MouseButtonsDg.Right:
						IEnumerable<MenuItemDef> menuItemDefs = toolSetController.GetMenuItemDefs(e.Tool);
						toolSetView.OpenContextMenu(e.Position.X, e.Position.Y, menuItemDefs, Project);
						break;

					default:
						// Nothing to do here
						break;
				}
			}
		}


		private void toolSetView_ToolSetViewMouseMove(object sender, ToolSetViewMouseEventArgs e) {
			// Nothing to do here
		}


		private void toolSetView_ToolSetViewMouseDown(object sender, ToolSetViewMouseEventArgs e) {
			// Nothing to do here
		}


		private void toolSetView_SelectedToolChanged(object sender, SelectedToolChangedEventArgs e) {
			toolSetController.SelectTool(e.Tool, e.MultipleUse);
		}

		#endregion


		#region Fields

		private const string headerName = "Name";
		private const int templateDefaultSize = 20;
		private const int imageMargin = 2;
		private const int smallImageSize = 16;
		private const int largeImageSize = 32;

		private ToolSetController toolSetController;
		private IToolSetView toolSetView;
		private Tool selectedTool;

		// Settings
		private Color transparentColor = Color.White;
		private bool hideMenuItemsIfNotGranted = false;
		private bool showDefaultContextMenu = true;

		#endregion
	}


	/// <summary>
	/// Interface for a <see cref="T:Dataweb.NShape.Controllers.ToolSetPresenter" />'s user interface implementation.
	/// </summary>
	public interface IToolSetView {

		#region Events

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<SelectedToolChangedEventArgs> SelectedToolChanged;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewMouseEventArgs> ToolSetViewMouseDown;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewMouseEventArgs> ToolSetViewMouseMove;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewMouseEventArgs> ToolSetViewMouseUp;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewPreviewKeyDown;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewKeyDown;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewKeyPress;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<ToolSetViewKeyEventArgs> ToolSetViewKeyUp;

		#endregion


		#region Methods

		/// <summary>
		/// Clears the contents of the layer view.
		/// </summary>
		void Clear();

		/// <summary>
		/// Signals that the user interface is going to be updated.
		/// </summary>
		void BeginUpdate();

		/// <summary>
		/// Signals that the user interface was updated.
		/// </summary>
		void EndUpdate();

		/// <summary>
		/// Adds a new item to the user interface representing the given tool.
		/// </summary>
		void AddToolItem(Tool tool, bool isDefaultTool);

		/// <summary>
		/// Removes the item representing the given tool from the user interface.
		/// </summary>
		void RemoveToolItem(Tool tool);

		/// <summary>
		/// Refreshes the contents of the item representing the given layer.
		/// </summary>
		void RefreshToolItem(Tool tool);

		/// <summary>
		/// Selects the user interface item representing the given tool;
		/// </summary>
		void SelectToolItem(Tool tool);

		/// <summary>
		/// Signals that the user interface should be redrawn.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Signals the user interface that visibility of the given tool's user interface item has to be ensured.
		/// </summary>
		void EnsureVisibility(Tool tool);

		/// <summary>
		/// Signals that a context menu buildt from the given <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />s should be opened at the given coordinates.
		/// </summary>
		void OpenContextMenu(int x, int y, IEnumerable<MenuItemDef> contextMenuItemDefs, Project project);

		#endregion

	}


	/// <summary>
	/// Event args for tool set view inplementations.
	/// </summary>
	public class SelectedToolChangedEventArgs : ToolEventArgs {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.SelectedToolChangedEventArgs" />.
		/// </summary>
		public SelectedToolChangedEventArgs(Tool tool, bool multipleUse)
			: base(tool) {
			this.multipleUse = multipleUse;
		}


		/// <summary>
		/// Specifies wether a tool should be selected for multiple use or deselected after execution.
		/// </summary>
		public bool MultipleUse {
			get { return multipleUse; }
			protected internal set { multipleUse = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal SelectedToolChangedEventArgs()
			: base() {
		}


		private bool multipleUse = false;
	}


	/// <summary>
	/// Mouse event args for tool set view inplementations.
	/// </summary>
	public class ToolSetViewMouseEventArgs : MouseEventArgsDg {

		/// <ToBeCompleted></ToBeCompleted>
		public ToolSetViewMouseEventArgs(Tool tool, 
			MouseEventType eventType, MouseButtonsDg buttons, int clickCount, int wheelDelta, 
			Point position, KeysDg modifiers)
			: base(eventType, buttons, clickCount, wheelDelta, position, modifiers) {
			this.tool = tool;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ToolSetViewMouseEventArgs(Tool tool, MouseEventArgsDg mouseEventArgs)
			: this(tool, mouseEventArgs.EventType, mouseEventArgs.Buttons, mouseEventArgs.Clicks, mouseEventArgs.WheelDelta, mouseEventArgs.Position, mouseEventArgs.Modifiers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Tool Tool {
			get { return tool; }
			protected internal set { tool = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal ToolSetViewMouseEventArgs()
			: base() {
			tool = null;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetMouseEvent(MouseEventType eventType, MouseButtonsDg buttons, 
			int clickCount, int wheelDelta, Point position, KeysDg modifiers){
			this.eventType = eventType;
			this.buttons = buttons;
			this.clicks = clickCount;
			this.modifiers = modifiers;
			this.position = position;
			this.wheelDelta = wheelDelta;
		}


		private Tool tool;
	}


	/// <summary>
	/// Key event args for tool set view inplementations.
	/// </summary>
	public class ToolSetViewKeyEventArgs : KeyEventArgsDg {

		/// <ToBeCompleted></ToBeCompleted>
		public ToolSetViewKeyEventArgs(Tool tool, KeyEventType eventType, int keyData, char keyChar, bool handled, bool suppressKeyPress)
			: base(eventType, keyData, keyChar, handled, suppressKeyPress) {
			this.tool = tool;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ToolSetViewKeyEventArgs(Tool tool, KeyEventArgsDg keyEventArgs)
			: this(tool, keyEventArgs.EventType, keyEventArgs.KeyData, keyEventArgs.KeyChar, keyEventArgs.Handled, keyEventArgs.SuppressKeyPress) {
			if (tool == null) throw new ArgumentNullException("tool");
			this.tool = tool;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Tool Tool {
			get { return tool; }
			protected internal set { tool = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal ToolSetViewKeyEventArgs()
			: base() {
			tool = null;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetKeyEvent(KeyEventType eventType, 
			int keyData, char keyChar, bool handled, bool suppressKeyPress) {
			this.eventType = eventType;
			this.handled = handled;
			this.keyChar = keyChar;
			this.keyData = keyData;
			this.suppressKeyPress = suppressKeyPress;
		}


		private Tool tool;
	}

}
