using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Infragistics.Win;
using Infragistics.Win.UltraWinExplorerBar;

namespace Dataweb.TurboDiagram {
	public partial class ToolBoxInfragisticsAdapter : Component, IToolBox, IDisposable {
		public ToolBoxInfragisticsAdapter() {
			InitializeComponent();
		}

		#region IDisposable Members

		void IDisposable.Dispose() {
			DisposeUltraExplorerBar();
		}


		private void DisposeUltraExplorerBar() {
			if (ultraExplorerBar != null) {
				foreach (UltraExplorerBarGroup group in ultraExplorerBar.Groups) {
					foreach (UltraExplorerBarItem item in group.Items) {
						if (item.Tag is Tool) {
							((Tool)item.Tag).ToolExecuted -= new ToolExecutedEventHandler(tool_ToolExecuted);
							((Tool)item.Tag).Dispose();
							item.Dispose();
						}
					}
				}
				ultraExplorerBar.Dispose();
			}
		}

		#endregion


		#region IShapeService Members

		public void InvalidateDiagram(int x, int y, int width, int height) { }
		public void InvalidateDiagram(Rectangle rectangle) { }
		public void InvalidateDiagram(RectangleF rectangleF) { }
		public void InvalidateDiagram(Region region) { }

		public Design Design { get { return Project.CurrentDesign; } }

		public History History { get { return Project.History; } }

		public ToolCache ToolCache { get { return Project.ToolCache; } }

		#endregion


		#region IToolBox Members

		public event ToolSelectedEventHandler ToolSelected;

	
		public IRepository Repository {
			get { return repository; }
			set {
				if (repository is IRepository) {
					repository.RepositoryOpen -= new EventHandler(repository_RepositoryOpen);
					repository.RepositoryClose -= new EventHandler(repository_RepositoryClose);
					repository.TemplateUpdated -= new TemplateUpdatedEvent(repository_TemplateUpdated);
					repository.ShapeUpdated -= new ShapeUpdatedEvent(repository_ShapeUpdated);
					Clear();
				}
				repository = value;
				if (repository is IRepository) {
					repository.RepositoryOpen += new EventHandler(repository_RepositoryOpen);
					repository.RepositoryClose += new EventHandler(repository_RepositoryClose);
					repository.TemplateUpdated += new TemplateUpdatedEvent(repository_TemplateUpdated);
					repository.ShapeUpdated += new ShapeUpdatedEvent(repository_ShapeUpdated);
				}
			}
		}


		public IPropertyWindow PropertyWindow {
			get { return propertyWindow; }
			set { propertyWindow = value; }
		}


		public void Clear() {
			menuItemNew.Enabled = false;
			foreach (ToolStripDropDownItem dropDownItem in menuItemNew.DropDownItems) {
				dropDownItem.Click -= new EventHandler(menuItemNew_Click);
			}
			menuItemNew.DropDownItems.Clear();
			menuItemNew.Enabled = false;
			menuItemEdit.Enabled = false;
			menuItemDelete.Enabled = false;

			if (ultraExplorerBar != null) {
				foreach (UltraExplorerBarGroup group in ultraExplorerBar.Groups) {
					foreach (UltraExplorerBarItem item in group.Items) {
						if (item.Tag is Tool)
							((Tool)item.Tag).Dispose();
					}
					group.Items.Clear();
				}
				ultraExplorerBar.Groups.Clear();
			}
		}


		void repository_RepositoryClose(object sender, EventArgs e) {
			Clear();
		}


		void repository_RepositoryOpen(object sender, EventArgs e) {
			Init();
		}


		void repository_TemplateUpdated(Template template) {
			if (template != null) {
				ultraExplorerBar.BeginUpdate();
				// search *all* tools and change template if used
				foreach (UltraExplorerBarGroup group in ultraExplorerBar.Groups) {
					foreach (UltraExplorerBarItem item in group.Items) {
						if (item.Tag is Tool) {
							Tool tool = (Tool)item.Tag;
							Template t = null;
							PropertyInfo[] propertyInfos = tool.GetType().GetProperties();
							if (propertyInfos != null) {
								foreach (PropertyInfo pi in propertyInfos) {
									if (pi.PropertyType == typeof(Template)) {
										t = (Template)pi.GetValue(tool, null);
										break;
									}
								}
							}
							if (template.Equals(t)) {
								tool.Refresh();
								item.Settings.AppearancesSmall.Appearance.Image = tool.SmallIcon;
								item.Settings.AppearancesLarge.Appearance.Image = tool.LargeIcon;
							}
						}
					}
				}
				ultraExplorerBar.EndUpdate();
			}
		}


		void repository_ShapeUpdated(IShape shape) {
			if (shape != null) {
				ultraExplorerBar.BeginUpdate();
				foreach (UltraExplorerBarGroup group in ultraExplorerBar.Groups) {
					foreach (UltraExplorerBarItem item in group.Items) {
						if (item.Tag is Tool) {
							Tool tool = (Tool)item.Tag;
							Template t = null;
							PropertyInfo[] propertyInfos = tool.GetType().GetProperties();
							if (propertyInfos != null) {
								foreach (PropertyInfo pi in propertyInfos) {
									if (pi.PropertyType == typeof(Template)) {
										t = (Template)pi.GetValue(tool, null);
										break;
									}
								}
							}
							if (t != null && shape.Equals(t.ShapeObject)) {
								tool.Refresh();
								item.Settings.AppearancesSmall.Appearance.Image = tool.SmallIcon;
								item.Settings.AppearancesLarge.Appearance.Image = tool.LargeIcon;
							}
						}
					}
				}
				ultraExplorerBar.EndUpdate();
			}
		}


		private void Init() {
			if (repository == null)
				throw new Exception("ToolBoxAdapter's Repository property is not set to a reference of an object.", new NullReferenceException());
			//else if (Project == null)
			//	throw new Exception("ToolBoxAdapter's Project property is not set to a reference of an object.", new NullReferenceException());
			else {
				// Add Special Tools
				DefaultTool = new PointerTool();
				AddTool(new FreeHandTool(Display.Project));
				//
				if (!repository.IsOpen)
					repository.Open();
				List<Template> templates = new List<Template>(repository.GetTemplates());
				foreach (Template template in templates) {
					Tool tool;
					if (template.ShapeObject.IsLine)
						tool = new LineTool(template);
					else
						tool = new InsertionTool(template);
					AddTool(tool);
				}

				menuItemNew.Enabled = true;
				menuItemEdit.Enabled = false;
				menuItemDelete.Enabled = false;

				foreach (Template template in repository.GetTemplates()) {
					Tool tool;
					if (template.ShapeObject.IsLine)
						tool = new LineTool(template);
					else
						tool = new InsertionTool(template);
					AddTool(tool);
				}
			}

			if (ultraExplorerBar != null) {
				if (defaultItem != null)
					defaultItem.Active = true;
			}
			else
				throw new Exception("Adapter's UltraExplorerBar property is not set to a reference of an object.", new NullReferenceException());
		}


		public void SelectTool(Tool tool) {
			foreach (UltraExplorerBarGroup group in ultraExplorerBar.Groups) {
				foreach (UltraExplorerBarItem item in group.Items) {
					if (item.Tag.Equals(tool)) {
						ultraExplorerBar.ActiveGroup = group;
						ultraExplorerBar.ActiveItem = item;
						break;
					}
				}
			}
			if (Display != null)
				Display.Tool = SelectedTool;
		}
		
		
		public Tool SelectedTool {
			get {
				Tool result = null;
				UltraExplorerBarItem selectedItem = GetSelectedItem();
				if (selectedItem != null) {
					result = (Tool)selectedItem.Tag;
				}
				return result;
			}
		}


		public Tool DefaultTool {
			get {
				Tool result = null;
				if (defaultItem != null && defaultItem.Tag != null)
					result = (Tool)defaultItem.Tag;
				return result;
			}
			set {
				bool found = false;
				if (value != null) {
					foreach (UltraExplorerBarGroup group in ultraExplorerBar.Groups) {
						foreach (UltraExplorerBarItem item in group.Items) {
							if (item.Tag == value) {
								defaultItem = item;
								found = true;
								break;
							}
						}
					}
					if (!found)
						AddTool(value);
				}
			}
		}


		public void AddTool(Template template) {
			AddTemplate(template);
			Tool tool;
			if (template.ShapeObject.IsLine)
				tool = new LineTool(template);
			else
				tool = new InsertionTool(template);
			AddTool(tool, false);
		}
		
		
		public void AddTool(Tool tool) {
			AddTool(tool, false);
		}


		public void AddTool(Tool tool, bool isDefaultTool) {
			if (ultraExplorerBar != null) {
				tool.ToolExecuted += new ToolExecutedEventHandler(tool_ToolExecuted);
				UltraExplorerBarItem item = CreateItem(tool);

				int idx = ultraExplorerBar.Groups.IndexOf(tool.Category);
				if (idx < 0) {
					ultraExplorerBar.Groups.Add(tool.Category, tool.Category);
					idx = ultraExplorerBar.Groups.IndexOf(tool.Category);
				}
				ultraExplorerBar.Groups[idx].Items.Add(item);
				if (isDefaultTool) {
					defaultItem = item;
					defaultItem.Active = true;
				}
			}
		}


		internal void CreateTemplate(IModelObject baseModelObject) {
			TemplateEditor dlg = new TemplateEditor(Display.Project, baseModelObject);
			DialogResult result = dlg.ShowDialog();
			if (result == DialogResult.OK) {
				Repository.InsertTemplate(dlg.Result);
				AddTool(dlg.Result);
			}
			dlg.Invalidate();
		}


		public void AddTemplate(Template template) {
			if (ultraExplorerBar != null) {
				// Add Template to repository
				Repository.InsertTemplate(template);
			}
			else
				throw new Exception("ToolBox's UltraExplorerBar property is not set to a reference of an object.");
		}


		public void EditTemplate() {
			if (ultraExplorerBar != null) {
				if (editItem != null && editItem.Tag is InsertionTool) {
					TemplateEditor dlg = null;
					if (editItem.Tag is InsertionTool)
						dlg = new TemplateEditor(Display.Project, ((InsertionTool)editItem.Tag).Template);
					else if (editItem.Tag is LineTool)
						dlg = new TemplateEditor(Display.Project, ((LineTool)editItem.Tag).Template);

					DialogResult result = dlg.ShowDialog();
					if (result == DialogResult.OK) {
						editItem.Tag = null;
						if (dlg.Result.ShapeObject.IsLine)
							editItem.Tag = new LineTool(dlg.Result);
						else
							editItem.Tag = new InsertionTool(dlg.Result);
						
						Repository.UpdateTemplate(dlg.Result);
					}
					else
						SelectTool(DefaultTool);
					dlg.Invalidate();
				}
			}
		}


		public void DeleteTemplate() {
			if (ultraExplorerBar != null) {
				UltraExplorerBarItem item = GetSelectedItem();
				if (item != null) {
					if (item.Tag is InsertionTool) {
						InsertionTool tool = (InsertionTool)item.Tag;
						Template template = (Template)tool.Template;

						// delete tool from ToolBox
						UltraExplorerBarGroup group = item.Group;
						group.Items.Remove(item);
						((Template)item.Tag).Dispose();
						item.Dispose();

						// delete Template from Repository
						Repository.DeleteTemplate(template);
					}
					else
						throw new Exception("Selected tool is of the wrong type.");
				}
				SelectTool(DefaultTool);
			}
		}

		#endregion


		public Display Display {
			get { return display; }
			set { display = value; }
		}


		public Project Project {
			get { return project; }
			set { project = value; }
		}


		public UltraExplorerBar UltraExplorerBar {
			get { return ultraExplorerBar; }
			set {
				if (value != null) {
					ultraExplorerBar = value;
					
					ultraExplorerBar.GroupSettings.AllowDrag = DefaultableBoolean.False;
					ultraExplorerBar.GroupSettings.AllowEdit = DefaultableBoolean.False;
					ultraExplorerBar.GroupSettings.AllowItemDrop = DefaultableBoolean.False;
					ultraExplorerBar.GroupSettings.AllowItemUncheck = DefaultableBoolean.False;

					ultraExplorerBar.ItemSettings.AllowDragCopy = ItemDragStyle.None;
					ultraExplorerBar.ItemSettings.AllowDragMove = ItemDragStyle.None;
					ultraExplorerBar.ItemSettings.AllowEdit = DefaultableBoolean.False;
					ultraExplorerBar.ItemSettings.HotTracking = DefaultableBoolean.True;

					ultraExplorerBar.ContextMenu = null;
					ultraExplorerBar.ContextMenuStrip = contextMenu;

					ultraExplorerBar.MouseDown += new MouseEventHandler(ultraExplorerBar_MouseDown);
					ultraExplorerBar.ItemClick += new ItemClickEventHandler(ultraExplorerBar_ItemClick);
					ultraExplorerBar.ItemDoubleClick += new ItemDoubleClickEventHandler(ultraExplorerBar_ItemDoubleClick);
					ultraExplorerBar.ActiveItemChanged += new ActiveItemChangedEventHandler(ultraExplorerBar_ActiveItemChanged);
					menuItemEdit.Click += new EventHandler(menuItemEditTemplate_Click);
					menuItemDelete.Click += new EventHandler(menuItemDeleteTemplate_Click);
					ultraExplorerBar.KeyDown += new KeyEventHandler(ultraExplorerBar_KeyDown);
					/*foreach (UltraExplorerBarStyle style in Enum.GetValues(typeof(UltraExplorerBarStyle))) {
						ToolStripMenuItem item = new ToolStripMenuItem(style.ToString(), null, menuItemStyle_Click);
						item.Tag = style;
						menuItemStyle.DropDownItems.Add(item);
					}*/
				}
				else {
					if (ultraExplorerBar != null) {
						menuItemDelete.Click -= new EventHandler(menuItemDeleteTemplate_Click);
						menuItemEdit.Click -= new EventHandler(menuItemEditTemplate_Click);
						/*foreach (ToolStripMenuItem item in menuItemStyle.DropDownItems)
							item.Click -= new EventHandler(menuItemStyle_Click);*/
						ultraExplorerBar.ActiveItemChanged -= new ActiveItemChangedEventHandler(ultraExplorerBar_ActiveItemChanged);
						ultraExplorerBar.ItemDoubleClick -= new ItemDoubleClickEventHandler(ultraExplorerBar_ItemDoubleClick);
						ultraExplorerBar.ItemClick -= new ItemClickEventHandler(ultraExplorerBar_ItemClick);
						ultraExplorerBar.MouseDown -= new MouseEventHandler(ultraExplorerBar_MouseDown);
						ultraExplorerBar.KeyDown -= new KeyEventHandler(ultraExplorerBar_KeyDown);
						DisposeUltraExplorerBar();
						ultraExplorerBar = value;
					}
				}
			}
		}


		void ultraExplorerBar_KeyDown(object sender, KeyEventArgs e) {
			Tool selectedTool = (Tool)GetSelectedItem().Tag;
			if (selectedTool != null && e.KeyCode == Keys.Escape)
				selectedTool.Cancel();
		}


		private UltraExplorerBarItem CreateItem(Tool tool) {
			UltraExplorerBarItem item = new UltraExplorerBarItem(tool.Name);
			item.Text = tool.Name;
			item.ToolTipText = tool.ToolTipText;
			item.Tag = tool;

			item.Settings.AppearancesSmall.Appearance.Image = tool.SmallIcon;
			item.Settings.AppearancesLarge.Appearance.Image = tool.LargeIcon;

			return item;
		}


		private UltraExplorerBarItem GetSelectedItem() {
			UltraExplorerBarItem item = null;
			if (ultraExplorerBar != null)
				item = ultraExplorerBar.ActiveItem;
			return item;
		}


		private void ultraExplorerBar_ActiveItemChanged(object sender, ItemEventArgs e) {
			menuItemDelete.Enabled = (bool)(ultraExplorerBar.ActiveItem != null);
			menuItemEdit.Enabled = (bool)(ultraExplorerBar.ActiveItem != null);
			if (ToolSelected != null) {
				Tool selectedTool = (Tool)GetSelectedItem().Tag;
				if (selectedTool != null)
					selectedTool.Cancel();

				if (Display != null)
					Display.Tool = selectedTool;

				if (e.Item != null)
					ToolSelected(this, new EventArgs());
			}
		}


		void ultraExplorerBar_MouseDown(object sender, MouseEventArgs e) {
			if ((e.Button & MouseButtons.Right) > 0) {
				editItem = ultraExplorerBar.ItemFromPoint(e.Location);
			}
		}


		private void ultraExplorerBar_ItemClick(object sender, ItemEventArgs e) {
			executeOnce = (sender != defaultItem);
			if (ToolSelected != null)
				ToolSelected(this, new EventArgs());
		}


		private void ultraExplorerBar_ItemDoubleClick(object sender, ItemEventArgs e) {
			executeOnce = false;
			if (ToolSelected != null)
				ToolSelected(this, new EventArgs());
		}


		private void tool_ToolExecuted(object sender, ToolExecutedEventArgs e) {
			if ((SelectedTool == (Tool)sender) && executeOnce) {
				defaultItem.Active = true;
				executeOnce = false;
			}
		}


		private void contextMenu_Opening(object sender, CancelEventArgs e) {
			menuItemNew.DropDownItems.Clear();
			foreach (ModelObjectType t in Display.Project.Model.ModelObjectTypes) {
				IModelObject modelObject = t.CreateObject(null);
				ToolStripMenuItem menuItem = new ToolStripMenuItem(modelObject.ModelObjectType.Name, null, menuItemNew_Click);
				menuItem.Tag = modelObject;
				menuItemNew.DropDownItems.Add(menuItem);
			}

			//if (sender is ContextMenuStrip) {
			//    ContextMenuStrip menu = (ContextMenuStrip)sender;
			//    menuItemEdit.Enabled = false;
			//    menuItemDelete.Enabled = false;
			//    foreach (ListViewItem item in listView.Items) {
			//        if (ultraExplorerBar.RectangleToScreen(item.Bounds).Contains(menu.Location)) {
			//            if (item.Tag is InsertionTool) {
			//                menuItemEdit.Enabled = true;
			//                menuItemDelete.Enabled = true;
			//                break;
			//            }
			//        }
			//    }
			//}
		}

		
		private void menuItemNew_Click(object sender, EventArgs e) {
			if (((ToolStripMenuItem)sender).Tag is IModelObject)
				CreateTemplate((IModelObject)((ToolStripMenuItem)sender).Tag);
			else
				throw new Exception("Invalid ModelObject!");
		}


		void menuItemEditTemplate_Click(object sender, EventArgs e) {
			EditTemplate();
		}


		void menuItemDeleteTemplate_Click(object sender, EventArgs e) {
			DeleteTemplate();
		}


		/*private void menuItemStyle_Click(object sender, EventArgs e) {
			UltraExplorerBarStyle style = (UltraExplorerBarStyle)((ToolStripMenuItem)sender).Tag;
			ultraExplorerBar.Style = style;
		}*/


		#region Fields
		
		private const int imageMargin = 2;
		private const int smallImageSize = 16;
		private const int largeImageSize = 32;

		private UltraExplorerBar ultraExplorerBar;
		private IPropertyWindow propertyWindow;
		private IRepository repository;
		private Display display;
		private Project project;

		private UltraExplorerBarItem defaultItem;
		private bool executeOnce;
		private UltraExplorerBarItem editItem;
		
		#endregion
	}
}
