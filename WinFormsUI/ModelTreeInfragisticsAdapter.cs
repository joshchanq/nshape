using System;
using Infragistics.Win.UltraWinTree;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Drawing;


namespace Dataweb.TurboDiagram {

	#region ModelTreeInfragisticsEnumerator

	internal class ModelTreeInfragisticsEnumerator : IEnumerator<IModelObject> {
		public ModelTreeInfragisticsEnumerator(UltraTree treeView) {
			this.treeView = treeView;
			index = -1;
		}

		#region IEnumerator<IModelObject> Members

		public bool MoveNext() { return (bool)(++index < treeView.SelectedNodes.Count); }
		
		public void Reset() { index = -1; }
		
		IModelObject IEnumerator<IModelObject>.Current { get { return (IModelObject)treeView.SelectedNodes[index].Tag; } }
		
		#endregion

		#region IDisposable Members
		public void Dispose() { }
		#endregion

		#region IEnumerator Members

		object IEnumerator.Current { get { return (IModelObject)treeView.SelectedNodes[index].Tag; } }

		#endregion

		#region Fields
		private UltraTree treeView;
		private int index;
		#endregion
	}
	#endregion


	#region ModelTreeInfragisticsSelectedObjects

	internal class ModelTreeInfragisticsSelectedObjects : ICollection<IModelObject> {

		public ModelTreeInfragisticsSelectedObjects(UltraTree treeView) {
			this.treeView = treeView;
		}


		#region ICollection<IModelObject> Members


		public void Add(IModelObject item) {
			/*foreach (UltraTreeNode n in treeView.Nodes) {
				if (n.Tag == item) {
					//treeView.SelectedNodes.Add(n);
					break;
				}
			}*/
			throw new Exception("The method or operation is not supported by this control.");
		}

		public void Clear() {
			treeView.SelectedNodes.Clear();
		}

		public bool Contains(IModelObject item) {
			bool result = false;
			foreach (UltraTreeNode n in treeView.SelectedNodes) {
				if (n.Tag == item) {
					result = true;
					break;
				}
			}
			return result;
		}

		public void CopyTo(IModelObject[] array, int arrayIndex) {
			throw new Exception("The method or operation is not implemented.");
		}

		public int Count {
			get { return treeView.SelectedNodes.Count; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool Remove(IModelObject item) {
			throw new Exception("The method or operation is not supported by this control.");
		}

		#endregion

		#region IEnumerable<IModelObject> Members

		public IEnumerator<IModelObject> GetEnumerator() {
			return new ModelTreeInfragisticsEnumerator(treeView);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return new ModelTreeInfragisticsEnumerator(treeView);
		}

		#endregion

		#region Fields
		private UltraTree treeView;
		#endregion
	}
	#endregion


	[ToolboxItem(true)]
	public class ModelTreeInfragisticsAdapter : Component, IModelTree {

		public event ModelObjectSelectedEventHandler ModelObjectSelected;


		public ModelTreeInfragisticsAdapter() {
			imageList = new ImageList();
			imageList.TransparentColor = Color.White;
			imageList.ImageSize = new Size(imageSize, imageSize);
		}


		public IRepository Repository {
			get { return repository; }
			set {
				if (repository is IRepository) {
					repository.RepositoryOpen -= new EventHandler(repository_RepositoryOpen);
					repository.RepositoryClose -= new EventHandler(repository_RepositoryClose);
					repository.TemplateUpdated -= new TemplateUpdatedEvent(repository_TemplateUpdated);
					repository.ModelObjectInserted -= new ModelObjectInsertedEvent(repository_ModelObjectInserted);
					repository.ModelObjectUpdated -= new ModelObjectUpdatedEvent(repository_ModelObjectUpdated);
					repository.ModelObjectDeleted -= new ModelObjectDeletedEvent(repository_ModelObjectDeleted);
				}
				repository = value;
				if (repository is IRepository) {
					repository.RepositoryOpen += new EventHandler(repository_RepositoryOpen);
					repository.RepositoryClose += new EventHandler(repository_RepositoryClose);
					repository.TemplateUpdated += new TemplateUpdatedEvent(repository_TemplateUpdated);
					repository.ModelObjectInserted += new ModelObjectInsertedEvent(repository_ModelObjectInserted);
					repository.ModelObjectUpdated += new ModelObjectUpdatedEvent(repository_ModelObjectUpdated);
					repository.ModelObjectDeleted += new ModelObjectDeletedEvent(repository_ModelObjectDeleted);
				}
			}
		}


		public Display Display {
			get { return display; }
			set {
				if (display != null)
					display.ShapeObjectsSelected -= new ShapeObjectsSelectedEventHandler(display_ShapeObjectsSelected);
				display = value;
				if (display != null)
					display.ShapeObjectsSelected += new ShapeObjectsSelectedEventHandler(display_ShapeObjectsSelected);
			}
		}


		[Browsable(false)]
		public ICollection<IModelObject> SelectedModelObjects {
			get { return modelTreeSelectedObjects; }
		}


		public UltraTree UltraTree {
			get { return treeView; }
			set {
				if (value != null) {
					treeView = value;
					treeView.ImageList = imageList;
					treeView.AfterCollapse += new AfterNodeChangedEventHandler(treeView_AfterCollapse);
					treeView.AfterSelect += new AfterNodeSelectEventHandler(treeView_AfterSelect);
					treeView.BeforeExpand += new BeforeNodeChangedEventHandler(treeView_BeforeExpand);
					treeView.BeforeSelect += new BeforeNodeSelectEventHandler(treeView_BeforeSelect);
					treeView.SelectionDragStart += new EventHandler(treeView_SelectionDragStart);

					modelTreeSelectedObjects = new ModelTreeInfragisticsSelectedObjects(treeView);
				}
				else {
					if (treeView != null) {
					treeView.AfterCollapse -= new AfterNodeChangedEventHandler(treeView_AfterCollapse);
					treeView.AfterSelect -= new AfterNodeSelectEventHandler(treeView_AfterSelect);
					treeView.BeforeExpand -= new BeforeNodeChangedEventHandler(treeView_BeforeExpand);
					treeView.BeforeSelect -= new BeforeNodeSelectEventHandler(treeView_BeforeSelect);
					treeView.ImageList = null;
					}
					treeView = value;
				}
			}
		}


		void display_ShapeObjectsSelected(object sender, ShapeObjectsSelectedEventArgs e) {
			// ToDo:
			// Select ModelObjects?
		}


		void repository_RepositoryClose(object sender, EventArgs e) {
			Clear();
		}


		void repository_RepositoryOpen(object sender, EventArgs e) {
			Init();
		}


		void repository_TemplateUpdated(Template template) {
			treeView.SuspendLayout();
			if (template != null) {
				int idx = imageList.Images.IndexOfKey(template.ModelObject.ModelObjectType.Name);
				if (idx >= 0) {
					imageList.Images[idx] = template.GetIcon(imageList.ImageSize.Width, imgMargin);
					UltraTree.Refresh();
				}
			}
			treeView.ResumeLayout();
		}


		void repository_ModelObjectDeleted(IModelObject modelObject) {
			ModelObjectDeleted(modelObject);
		}


		void repository_ModelObjectUpdated(IModelObject modelObject) {
			ModelObjectChanged(modelObject);
		}


		void repository_ModelObjectInserted(IModelObject modelObject) {
			ModelObjectAdded(modelObject);
		}


		private void Init() {
			if (treeView != null) {
				if (repository != null) {
					List<IModelObject> modelObjectList = new List<IModelObject>(repository.GetChildModelObjects(null));
					foreach (IModelObject modelObject in modelObjectList) {
						UltraTreeNode node = CreateNode(modelObject);
						treeView.Nodes.Add(node);
					}
				}
				else
					throw new Exception("Adapter's Repository property is not set to a reference of an object.", new NullReferenceException());
			}
			else
				throw new Exception("Adapter's UltraTree property is not set to a reference of an object.", new NullReferenceException());
		}

		
		public void Clear() {
			if (treeView != null)
				treeView.Nodes.Clear();
		}


		public Template FindTemplate(string modelObjectTypeName) {
			Template result = null;
			if (!repository.IsOpen)
				repository.Open();
			List<Template> templates = new List<Template>(repository.GetTemplates());
			foreach (Template t in templates) {
				if (t.ModelObject.ModelObjectType.Name == modelObjectTypeName) {
					result = t;
					break;
				}
			}
			return result;
		}

		
		public void ModelObjectAdded(IModelObject modelObject) {
			if (treeView != null) {
				if (modelObject.Parent != null) {
					UltraTreeNode node = FindModelObjectNode(treeView.Nodes, modelObject.Parent);
					if (node != null && (node.Expanded || node.Nodes.Count == 0))
						node.Nodes.Add(CreateNode(modelObject));
				}
				else
					treeView.Nodes.Add(CreateNode(modelObject));
			}
		}


		public void ModelObjectsAdded(IEnumerable<IModelObject> modelObjects) {
			treeView.BeginUpdate();
			foreach (IModelObject modelObject in modelObjects)
				ModelObjectAdded(modelObject);
			treeView.EndUpdate();
		}


		public void ModelObjectDeleted(IModelObject modelObject) {			
			if (treeView != null) {
				UltraTreeNode node = FindModelObjectNode(treeView.Nodes, modelObject);
				if (node != null) {
					UltraTreeNode parentNode = node.Parent;
					if (parentNode == null)
						treeView.Nodes.Remove(node);
					else
						parentNode.Nodes.Remove(node);
				}
			}
		}


		public void ModelObjectsDeleted(IEnumerable<IModelObject> modelObjects) {
			if (treeView != null)
				treeView.BeginUpdate();
			foreach (IModelObject modelObject in modelObjects)
				ModelObjectDeleted(modelObject);
			if (treeView != null)
				treeView.EndUpdate();
		}


		public void ModelObjectChanged(IModelObject modelObject) {
			if (treeView != null) {
				UltraTreeNode node = FindModelObjectNode(treeView.Nodes, modelObject);
				if (node != null) {
					UltraTreeNode parentNode = node.Parent;

					UltraTreeNode newNode = CreateNode(modelObject);
					if (parentNode == null) {
						int idx = treeView.Nodes.IndexOf(node);
						treeView.Nodes.RemoveAt(idx);
						treeView.Nodes.Insert(idx, newNode);
					}
					else {
						int idx = parentNode.Nodes.IndexOf(node);
						parentNode.Nodes.RemoveAt(idx);
						parentNode.Nodes.Insert(idx, newNode);
					}
				}
			}
		}


		public int CreateNodeImage(IModelObject modelObject) {
			int idx = imageList.Images.IndexOfKey(modelObject.ModelObjectType.Name);
			if (idx < 0) {
				Template template = null;
				template = FindTemplate(modelObject.ModelObjectType.Name);

				Image img = null;
				if (template != null) {
					img = template.GetIcon(imageList.ImageSize.Width, imgMargin);
				}
				else {
					img = new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height);
					Graphics g = Graphics.FromImage(img);
					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
					g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
					g.FillRectangle(new SolidBrush(imageList.TransparentColor), new System.Drawing.Rectangle(0, 0, imageList.ImageSize.Width, imageList.ImageSize.Height));
					g.DrawEllipse(Pens.Red, new System.Drawing.Rectangle(imgMargin, imgMargin, imageSize - (2 * imgMargin), imageSize - (2 * imgMargin)));
					g.DrawLine(Pens.Red, new Point(0 + (2 * imgMargin), imageSize - (2 * imgMargin)), new Point(imageSize - (2 * imgMargin), (2 * imgMargin)));
				}

				imageList.Images.Add(modelObject.ModelObjectType.Name, img);
				idx = imageList.Images.IndexOfKey(modelObject.ModelObjectType.Name);
			}
			return idx;
		}


		private UltraTreeNode CreateNode(IModelObject modelObject) {
			UltraTreeNode result = new UltraTreeNode(modelObject.Name);
			result.Key = modelObject.Name;
			result.Text = modelObject.Name;
			result.Tag = modelObject;
			result.LeftImages.Add(imageList.Images[CreateNodeImage(modelObject)]);
			result.Nodes.Add(result.Key + " DummyNode", "");
			return result;
		}


		private UltraTreeNode FindModelObjectNode(TreeNodesCollection nodesCollection, IModelObject modelObject) {
			UltraTreeNode result = null;
			foreach (UltraTreeNode node in nodesCollection) {
				if (node.Tag == modelObject)
					result = node;
				else if (node.Nodes.Count > 0)
					result = FindModelObjectNode(node.Nodes, modelObject);
				
				if (result != null)
					break;
			}
			return result;
		} 

		
		private void treeView_AfterCollapse(object sender, NodeEventArgs e) {
			foreach (UltraTreeNode node in e.TreeNode.Nodes)
				e.TreeNode.Nodes.Remove(node);
			e.TreeNode.Nodes.Add(e.TreeNode.Key + " DummyNode", "");
		}


		private void treeView_AfterSelect(object sender, SelectEventArgs e) {
			if (ModelObjectSelected != null)
				ModelObjectSelected(this, new ModelObjectSelectedEventArgs(SelectedModelObjects, false));
		}


		private void treeView_BeforeExpand(object sender, CancelableNodeEventArgs e) {
			if (e.TreeNode.Nodes.Count > 0) {
				e.TreeNode.Nodes.Clear();
				/*foreach (UltraTreeNode node in e.TreeNode.Nodes)
					e.TreeNode.Nodes.Remove(node);*/
			}
			List<IModelObject> childs = new List<IModelObject>(Repository.GetChildModelObjects((IModelObject)e.TreeNode.Tag));
			foreach (IModelObject child in childs)
				e.TreeNode.Nodes.Add(CreateNode(child));

			if (e.TreeNode.Nodes.Count == 0)
				e.Cancel = true;
		}


		private void treeView_BeforeSelect(object sender, BeforeSelectEventArgs e) {
			// if node had no children and the + sign disappeared: 
			// add a DummyNode again
			foreach (UltraTreeNode node in treeView.Nodes)
				if (!node.HasExpansionIndicator)
					node.Nodes.Add(node.Key + " DummyNode", "");
		}


		void treeView_SelectionDragStart(object sender, EventArgs e) {
			if (SelectedModelObjects.Count > 0) {
				List<IShape> presentationObjects = new List<IShape>();
				foreach (ShapeType t in Display.Project.Presentation.ShapeTypes)
					presentationObjects.Add(t.CreateInstance(null, null));				
				foreach (IModelObject modelObject in SelectedModelObjects) {
					Template template = FindTemplate(modelObject.ModelObjectType.Name);
					if (template != null) {
						IShape ShapeObject = (IShape)template.ShapeObject.Clone();
						ShapeObject.ModelObject = modelObject;
						presentationObjects.Add(ShapeObject);
					}
				}
				DragInsertInfo dii = new DragInsertInfo(presentationObjects, 0, 0);
				treeView.DoDragDrop(dii, DragDropEffects.Move);
			}
		}


		#region Fields
		private const int imageSize = 16;
		private const int imgMargin = 2;

		ModelTreeInfragisticsSelectedObjects modelTreeSelectedObjects;

		private IRepository repository;
		private Display display;

		private ImageList imageList;
		private UltraTree treeView;
		#endregion

	}
}
