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
using System.Drawing;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	public static class WinFormHelpers {

		#region Convert EventArgs

		/// <summary>
		/// Extracts and returns nShapeMouseEventArgs from Windows.Forms.MouseEventArgs.
		/// </summary>
		public static nShapeMouseEventArgs GetMouseEventArgs(MouseEventType eventType, MouseEventArgs e) {
			return GetMouseEventArgs(eventType, e.Button, e.Clicks, e.X, e.Y, e.Delta);
		}


		/// <summary>
		/// Returns nShapeMouseEventArgs extracted from information provided by the Control class.
		/// </summary>
		public static nShapeMouseEventArgs GetMouseEventArgs(Control control, MouseEventType eventType, int clicks, int delta) {
			Point mousePos = control.PointToClient(Control.MousePosition);
			return GetMouseEventArgs(eventType, Control.MouseButtons, clicks, mousePos.X, mousePos.Y, delta);
		}


		/// <summary>
		/// Returns nShapeMouseEventArgs buildt with the provided parameters
		/// </summary>
		public static nShapeMouseEventArgs GetMouseEventArgs(MouseEventType eventType, MouseButtons mouseButtons, int clicks, int x, int y, int delta) {
			mouseEventArgs.SetButtons(mouseButtons);
			mouseEventArgs.SetClicks(clicks);
			mouseEventArgs.SetEventType(eventType);
			mouseEventArgs.SetPosition(x, y);
			mouseEventArgs.SetWheelDelta(delta);
			mouseEventArgs.SetModifiers(GetModifiers());
			return mouseEventArgs;
		}


		private static nShapeKeys GetModifiers() {
			// get Modifier Keys
			nShapeKeys result = nShapeKeys.None;
			if ((Control.ModifierKeys & Keys.Control) != 0)
				result |= nShapeKeys.Control;
			if ((Control.ModifierKeys & Keys.Shift) != 0)
				result |= nShapeKeys.Shift;
			if ((Control.ModifierKeys & Keys.Alt) != 0)
				result |= nShapeKeys.Alt;
			return result;
		}


		/// <summary>
		/// Extracts and returns nShapeKeyEventArgs from Windows.Forms.KeyEventArgs
		/// </summary>
		public static nShapeKeyEventArgs GetKeyEventArgs(KeyEventType eventType, KeyEventArgs e) {
			return GetKeyEventArgs(eventType, '\0', (int)e.KeyData, e.Handled, e.SuppressKeyPress);
		}


		/// <summary>
		/// Extracts and returns nShapeKeyEventArgs from Windows.Forms.PreviewKeyDownEventArgs
		/// </summary>
		public static nShapeKeyEventArgs GetKeyEventArgs(PreviewKeyDownEventArgs e) {
			return GetKeyEventArgs(KeyEventType.PreviewKeyDown, '\0', (int)e.KeyData, false, false);
		}


		/// <summary>
		/// Extracts and returns nShapeKeyEventArgs from Windows.Forms.KeyPressEventArgs
		/// </summary>
		public static nShapeKeyEventArgs GetKeyEventArgs(KeyPressEventArgs e) {
			int keyData = (int)char.ToUpper(e.KeyChar) | (int)Control.ModifierKeys;
			return GetKeyEventArgs(KeyEventType.KeyPress, e.KeyChar, keyData, e.Handled, false);
		}


		/// <summary>
		/// Returns nShapeKeyEventArgs built with the provided parameters
		/// </summary>
		public static nShapeKeyEventArgs GetKeyEventArgs(KeyEventType eventType, char keyChar, int keyData, bool handled, bool suppressKeyPress) {
			keyEventArgs.SetEventType(eventType);
			keyEventArgs.SetKeyChar(keyChar);
			keyEventArgs.SetKeyData(keyData);
			keyEventArgs.Handled = handled;
			keyEventArgs.SuppressKeyPress = suppressKeyPress;
			return keyEventArgs;
		}

		#endregion


		#region Build ContextMenus from actions

		public static IEnumerable<ToolStripItem> GetContextMenuItemsFromAllowedActions(IEnumerable<nShapeAction> actions, Project project) {
			if (actions == null) throw new ArgumentNullException("actions");
			if (project == null) throw new ArgumentNullException("project");
			// Attention!!
			// We have to iterate manually instead of unsing foreach here because otherwise always the least 
			// processed action's Execute method will be called.
			IEnumerator<nShapeAction> enumerator = actions.GetEnumerator();
			while (enumerator.MoveNext()) {
				// Skip actions that are not allowed
				if (!enumerator.Current.IsGranted(project.SecurityManager)) continue;
				// Build and return menu item
				yield return CreateMenuItemFromAction(enumerator.Current, project);
			}
		}


		public static IEnumerable<ToolStripItem> GetContextMenuItemsFromActions(IEnumerable<nShapeAction> actions, Project project) {
			if (actions == null) throw new ArgumentNullException("actions");
			if (project == null) throw new ArgumentNullException("project");
			// Attention!!
			// We have to iterate manually instead of unsing foreach here because otherwise always the least 
			// processed action's Execute method will be called.
			IEnumerator<nShapeAction> enumerator = actions.GetEnumerator();
			while (enumerator.MoveNext()) {
				// Build and return menu item
				yield return CreateMenuItemFromAction(enumerator.Current, project);
			}
		}


		public static void BuildContextMenu(ContextMenuStrip contextMenuStrip, IEnumerable<nShapeAction> actions, Project project, bool skipIfNotGranted) {
			if (contextMenuStrip == null) throw new ArgumentNullException("contextMenuStrip");
			if (actions == null) throw new ArgumentNullException("actions");
			if (project == null) throw new ArgumentNullException("project");
			int itemCnt = contextMenuStrip.Items.Count;
			// Add a ToolStripSeparator if the last item is not a ToolStripSeparator
			if (itemCnt > 0 && !(contextMenuStrip.Items[itemCnt - 1] is ToolStripSeparator))
				contextMenuStrip.Items.Add(CreateMenuItemSeparator());
			IEnumerable<ToolStripItem> items = 
				skipIfNotGranted ? GetContextMenuItemsFromAllowedActions(actions, project) : GetContextMenuItemsFromAllowedActions(actions, project);
			foreach (ToolStripItem item in items) contextMenuStrip.Items.Add(item);
		}


		public static void BuildContextMenu(ContextMenuStrip contextMenu, IEnumerable<ToolStripItem> menuItems) {
			if (contextMenu == null) throw new ArgumentNullException("contextMenu");
			if (menuItems == null) throw new ArgumentNullException("menuItems");
			int itemCnt = contextMenu.Items.Count;
			if (itemCnt > 0 && !(contextMenu.Items[itemCnt - 1] is ToolStripSeparator))
				contextMenu.Items.Add(CreateMenuItemSeparator());
			foreach (ToolStripItem item in menuItems)
				contextMenu.Items.Add(item);
		}


		public static void CleanUpContextMenu(ContextMenuStrip contextMenuStrip) {
			if (contextMenuStrip == null) throw new ArgumentNullException("contextMenuStrip");
			// Do not dispose the items here because the execute method of the attached action will be called later!
			contextMenuStrip.Items.Clear();
		}


		private static ToolStripItem CreateMenuItemFromAction(nShapeAction action, Project project) {
			if (action is SeparatorAction) return CreateMenuItemSeparator();
			else {
				// Build ContextMenu item
				ToolStripMenuItem menuItem = new ToolStripMenuItem(action.Title, null, (s, e) => action.Execute(action, project));
				menuItem.Tag = action;
				menuItem.Name = action.Name;
				menuItem.Text = action.Title;
				menuItem.Checked = action.Checked;
				//menuItem.CheckOnClick = false;
				menuItem.Enabled = (action.IsFeasible && action.IsGranted(project.SecurityManager));
				if (action.IsGranted(project.SecurityManager))
					menuItem.ToolTipText = action.Description;
				else menuItem.ToolTipText = "Action is not granted.";
				menuItem.Image = action.Image;
				menuItem.ImageTransparentColor = action.ImageTransparentColor;

				menuItem.DropDownItems.Clear();
				// Add sub menu items (do not skip any sub items: if parent is granted, subitems are granted too)
				if (action.SubItems != null) {
					for (int i = 0; i < action.SubItems.Length; ++i)
						menuItem.DropDownItems.Add(CreateMenuItemFromAction(action.SubItems[i], project));
				}
				return menuItem;
			}
		}


		private static ToolStripSeparator CreateMenuItemSeparator() {
			ToolStripSeparator result = new ToolStripSeparator();
			result.Tag = new SeparatorAction();
			return result;
		}

		#endregion


		#region Types

		private class HelperMouseEventArgs : nShapeMouseEventArgs {

			public HelperMouseEventArgs()
				: base(MouseEventType.MouseMove, nShapeMouseButtons.None, 0, 0, Point.Empty, nShapeKeys.None) {
			}


			public void SetButtons(MouseButtons mouseButtons) {
				nShapeMouseButtons btns = nShapeMouseButtons.None;
				if ((mouseButtons & MouseButtons.Left) > 0) btns |= nShapeMouseButtons.Left;
				if ((mouseButtons & MouseButtons.Middle) > 0) btns |= nShapeMouseButtons.Middle;
				if ((mouseButtons & MouseButtons.Right) > 0) btns |= nShapeMouseButtons.Right;
				if ((mouseButtons & MouseButtons.XButton1) > 0) btns |= nShapeMouseButtons.ExtraButton1;
				if ((mouseButtons & MouseButtons.XButton2) > 0) btns |= nShapeMouseButtons.ExtraButton2;
				this.buttons = btns;
			}


			public void SetClicks(int clicks) {
				this.clicks = clicks;
			}


			public void SetEventType(MouseEventType eventType) {
				this.eventType = eventType;
			}


			public void SetPosition(int x, int y) {
				this.position.X = x;
				this.position.Y = y;
			}


			public void SetWheelDelta(int delta) {
				this.wheelDelta = delta;
			}


			public void SetModifiers(nShapeKeys modifiers) {
				this.modifiers = modifiers;
			}
		}


		private class HelperKeyEventArgs : nShapeKeyEventArgs {
			
			public HelperKeyEventArgs()
				: base(KeyEventType.PreviewKeyDown, 0, '\0', false, false) {
			}


			public void SetKeyData(int keyData) {
				this.keyData = keyData;
			}


			public void SetKeyChar(char keyChar) {
				this.keyChar = keyChar;
			}


			public void SetEventType(KeyEventType eventType) {
				this.eventType = eventType;
			}
		}

		#endregion


		#region Fields

		private static HelperMouseEventArgs mouseEventArgs = new HelperMouseEventArgs();
		private static HelperKeyEventArgs keyEventArgs = new HelperKeyEventArgs();
		
		#endregion
	}
}
