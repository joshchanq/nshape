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
	/// Controls the behavior of a set of diagrams.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(DiagramSetController), "DiagramSetController.bmp")]
	public class DiagramSetController : Component {

		public DiagramSetController() {
		}


		public DiagramSetController(Project project)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			Project = project;
		}


		~DiagramSetController() {
		}


		#region [Public] Events

		public event EventHandler ProjectChanging;
		
		public event EventHandler ProjectChanged;

		public event EventHandler ToolChanged;

		public event EventHandler<ModelObjectsEventArgs> SelectModelObjectsRequested;

		public event EventHandler<DiagramEventArgs> DiagramAdded;

		public event EventHandler<DiagramEventArgs> DiagramRemoved;

		#endregion


		#region [Public] Properties

		[Category("NShape")]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		[Category("NShape")]
		public Project Project {
			get { return project; }
			set {
				if (ProjectChanging != null) ProjectChanging(this, new EventArgs());
				if (project != null) UnregisterProjectEvents();
				project = value;
				if (project != null) RegisterProjectEvents();
				if (ProjectChanged != null) ProjectChanged(this, new EventArgs());
			}
		}


		[Browsable(false)]
		public Tool ActiveTool {
			get { return tool; }
			set {
				tool = value;
				if (ToolChanged != null) ToolChanged(this, new EventArgs());
			}
		}


		[Browsable(false)]
		public IEnumerable<Diagram> Diagrams {
			get {
				for (int i = 0; i < diagramControllers.Count; ++i)
					yield return diagramControllers[i].Diagram;
			}
		}

		#endregion


		#region [Public] Methods

		public Diagram CreateDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertProjectIsOpen();
			// Create new diagram
			Diagram diagram = new Diagram(name);
			diagram.Width = 1000;
			diagram.Height = 1000;
			ICommand cmd = new InsertDiagramCommand(diagram);
			project.ExecuteCommand(cmd);
			return diagram;
		}


		public void CloseDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertProjectIsOpen();
			int idx = IndexOf(name);
			if (idx >= 0) {
				DiagramEventArgs eventArgs = GetDiagramEventArgs(diagramControllers[idx].Diagram);
				diagramControllers.RemoveAt(idx);
				if (DiagramRemoved != null) DiagramRemoved(this, eventArgs);
			}
		}


		public void CloseDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertProjectIsOpen();
			int idx = DiagramControllerIndexOf(diagram);
			if (idx >= 0) {
				DiagramController controller = diagramControllers[idx];
				diagramControllers.RemoveAt(idx);

				DiagramEventArgs eventArgs = GetDiagramEventArgs(controller.Diagram);
				if (DiagramRemoved != null) DiagramRemoved(this, eventArgs);
				controller.Diagram = null;
				controller = null;
			}
		}


		public void DeleteDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertProjectIsOpen();
			int idx = IndexOf(name);
			if (idx >= 0) {
				DiagramController controller = diagramControllers[idx];
				ICommand cmd = new DeleteDiagramCommand(controller.Diagram);
				project.ExecuteCommand(cmd);
			}
		}


		public void DeleteDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertProjectIsOpen();
			int idx = DiagramControllerIndexOf(diagram);
			if (idx >= 0) {
				DiagramController controller = diagramControllers[idx];
				ICommand cmd = new DeleteDiagramCommand(controller.Diagram);
				project.ExecuteCommand(cmd);
			}
		}


		public void SelectModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (SelectModelObjectsRequested != null) 
				SelectModelObjectsRequested(this, GetModelObjectsEventArgs(modelObjects));
		}

		#endregion


		#region [Public] Methods: Methods provided by actions

		public void InsertShapes(Diagram diagram, IEnumerable<Shape> shapes, LayerIds activeLayers, bool withModelObjects) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			ICommand cmd = new InsertShapeCommand(diagram, activeLayers, shapes, withModelObjects, true);
			Project.ExecuteCommand(cmd);
			// ToDo: Raise event
			//if (ShapesInserted != null) ShapesInserted(this, eventArgs);
		}


		public void DeleteShapes(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			ICommand cmd = new DeleteShapeCommand(diagram, shapes, withModelObjects);
			Project.ExecuteCommand(cmd);
			// ToDo: Raise event
			//if (ShapesRemoved != null) ShapesRemoved(this, eventArgs);
		}


		public void Copy(Diagram source, IEnumerable<Shape> shapes, bool withModelObjects) {
			Copy(source, shapes, withModelObjects, Geometry.InvalidPoint);
		}


		public void Copy(Diagram source, IEnumerable<Shape> shapes, bool withModelObjects, Point startPos) {
			if (source == null) throw new ArgumentNullException("source");
			if (shapes == null) throw new ArgumentNullException("shapes");

			editBuffer.Clear();
			editBuffer.action = EditAction.Copy;
			editBuffer.withModelObjects = withModelObjects;
			editBuffer.initialMousePos = startPos;
			editBuffer.shapes.AddRange(shapes);
			// Copy shapes:
			// Use the ShapeCollection's Clone method in order to maintain connections 
			// between shapes inside the collection
			editBuffer.shapes = editBuffer.shapes.Clone(withModelObjects);	
		}


		public void Cut(Diagram source, IEnumerable<Shape> shapes, bool withModelObjects) {
			Cut(source, shapes, withModelObjects, Geometry.InvalidPoint);
		}


		public void Cut(Diagram source, IEnumerable<Shape> shapes, bool withModelObjects, Point startPos) {
			if (source == null) throw new ArgumentNullException("source");
			if (shapes == null) throw new ArgumentNullException("shapes");

			editBuffer.Clear();
			editBuffer.action = EditAction.Cut;
			editBuffer.withModelObjects = withModelObjects;
			editBuffer.initialMousePos = startPos;
			editBuffer.shapes.AddRange(shapes);

			ICommand cmd = new DeleteShapeCommand(source, editBuffer.shapes, withModelObjects);
			project.ExecuteCommand(cmd);
		}


		public void Paste(Diagram destination, LayerIds activeLayers) {
			Paste(destination, activeLayers, 20, 20);
		}


		public void Paste(Diagram destination, LayerIds activeLayers, Point p) {
			if (!editBuffer.IsEmpty) {
				int dx, dy;
				if (!Geometry.IsValid(editBuffer.initialMousePos))
					dx = dy = 20;
				else {
					dx = p.X - editBuffer.initialMousePos.X;
					dy = p.Y - editBuffer.initialMousePos.Y;
				}
				Paste(destination, activeLayers, dx, dy);
				editBuffer.initialMousePos = p;
			}
		}


		public void Paste(Diagram destination, LayerIds activeLayers, int offsetX, int offsetY) {
			if (destination == null) throw new ArgumentNullException("destination");
			if (!editBuffer.IsEmpty) {
				++editBuffer.pasteCount;

				// create command
				ICommand cmd = new InsertShapeCommand(
					destination,
					activeLayers,
					editBuffer.shapes.BottomUp,
					editBuffer.withModelObjects,
					(editBuffer.action == EditAction.Cut),
					offsetX,
					offsetY);
				// Execute InsertCommand and select inserted shapes
				Project.ExecuteCommand(cmd);

				// Clone shapes for another paste operation
				editBuffer.shapes = editBuffer.shapes.Clone(editBuffer.withModelObjects);
				if (editBuffer.action == EditAction.Cut) editBuffer.action = EditAction.Copy;
			}
		}


		/// <summary>
		/// Aggregates the selected shapes to a group.
		/// </summary>
		public void GroupShapes(Diagram diagram, IEnumerable<Shape> shapes, LayerIds activeLayers) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			int cnt = 0;
			foreach (Shape s in shapes)
				if (++cnt > 1) break;
			if (cnt > 1) {
				ShapeType groupShapeType = Project.ShapeTypes["ShapeGroup"];
				Debug.Assert(groupShapeType != null);

				Shape groupShape = groupShapeType.CreateInstance();
				ICommand cmd = new GroupShapesCommand(diagram, activeLayers, groupShape, shapes);
				Project.ExecuteCommand(cmd);
			}
		}


		/// <summary>
		/// Aggregate selected shapes to a composite shape based on the bottom shape.
		/// </summary>
		public void AggregateCompositeShape(Diagram diagram, Shape compositeShape, IEnumerable<Shape> shapes, LayerIds activeLayers) {
			if (compositeShape == null) throw new ArgumentNullException("compositeShape");
			if (shapes == null) throw new ArgumentNullException("shapes");
			// Add shapes to buffer (TopDown)
			shapeBuffer.Clear();
			foreach (Shape shape in shapes) {
				if (shape == compositeShape) continue;
				shapeBuffer.Add(shape);
			}
			ICommand cmd = new AggregateCompositeShapeCommand(diagram, activeLayers, compositeShape, shapeBuffer);
			Project.ExecuteCommand(cmd);
			shapeBuffer.Clear();
		}


		/// <summary>
		/// Ungroups the selected shapes or the selected shape aggregation.
		/// </summary>
		public void UngroupShapes(Diagram diagram, Shape groupShape) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (groupShape == null) throw new ArgumentNullException("groupShape");
			if (!(groupShape is IShapeGroup)) throw new ArgumentException(string.Format("groupShape does not implpement interface {0}", typeof(IShapeGroup).Name));
			// Add grouped shapes to shape buffer for selecting them later
			shapeBuffer.Clear();
			shapeBuffer.AddRange(groupShape.Children);

			ICommand cmd = new UngroupShapesCommand(diagram, groupShape);
			Project.ExecuteCommand(cmd);

			shapeBuffer.Clear();
		}


		/// <summary>
		/// Splits the selected composite shape into independent shapes.
		/// </summary>
		public void SplitCompositeShape(Diagram diagram, Shape compositeShape) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (compositeShape == null) throw new ArgumentNullException("compositeShape");
			if (compositeShape == null) throw new ArgumentNullException("compositeShape");
			Debug.Assert(!(compositeShape is IShapeGroup));
			// Add grouped shapes to shape buffer for selecting them later
			shapeBuffer.Clear();
			shapeBuffer.AddRange(compositeShape.Children);
			shapeBuffer.Add(compositeShape);

			ICommand cmd = new SplitCompositeShapeCommand(diagram, diagram.GetShapeLayers(compositeShape), compositeShape);
			Project.ExecuteCommand(cmd);

			shapeBuffer.Clear();
		}


		/// <summary>
		/// Lists one shape on top or to bottom
		/// </summary>
		public void LiftShape(Diagram diagram, Shape shape, ZOrderDestination liftMode) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shape == null) throw new ArgumentNullException("shape");
			ICommand cmd = null;
			cmd = new LiftShapeCommand(diagram, shape, liftMode);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Lifts a collection of shapes on top or to bottom.
		/// </summary>
		public void LiftShapes(Diagram diagram, IEnumerable<Shape> shapes, ZOrderDestination liftMode) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			ICommand cmd = new LiftShapeCommand(diagram, shapes, liftMode);
			Project.ExecuteCommand(cmd);
		}


		public bool CanInsertShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			return (Project.SecurityManager.IsGranted(Permission.Insert)
				&& !diagram.Shapes.ContainsAny(shapes));
		}


		public bool CanDeleteShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			return Project.SecurityManager.IsGranted(Permission.Delete)
				&& diagram.Shapes.ContainsAll(shapes);
		}


		public bool CanCut(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			return CanCopy(shapes) && CanDeleteShapes(diagram, shapes);
		}


		public bool CanCopy(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			// Check if shapes is not an empty collection
			foreach (Shape s in shapes) return true;
			return false;
		}


		public bool CanPaste(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (editBuffer.IsEmpty) return false;
			else {
				if (editBuffer.action == EditAction.Copy)
					return Project.SecurityManager.IsGranted(Permission.Insert);
				else return CanInsertShapes(diagram, editBuffer.shapes);
			}
		}


		public bool CanGroupShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			int cnt= 0;
			foreach (Shape shape in shapes) {
				++cnt;
				if (cnt >= 2) return Project.SecurityManager.IsGranted(Permission.Delete);
			}
			return false;
		}


		public bool CanUngroupShape(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			foreach (Shape shape in shapes) {
				if (shape is IShapeGroup && shape.Parent == null)
					return CanInsertShapes(diagram, shape.Children);
				else return false;
			}
			return false;
		}


		public bool CanAggregateShapes(Diagram diagram, IReadOnlyShapeCollection shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			return CanDeleteShapes(diagram, shapes)
				&& shapes.Count > 1
				&& !(shapes.Bottom is IShapeGroup);
		}


		public bool CanSplitShapeAggregation(Diagram diagram, IReadOnlyShapeCollection shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (shapes.Count == 1 && !(shapes.TopMost is IShapeGroup)) {
				Shape s = shapes.TopMost;
				if (s.Children.Count > 0 && CanInsertShapes(diagram, shapes.TopMost.Children))
					return true;
			} 
			return false;
		}


		public bool CanLiftShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			return Project.SecurityManager.IsGranted(Permission.Layout)
				&& diagram.Shapes.ContainsAll(shapes);
		}
		
		#endregion


		#region [Internal] Types

		internal enum EditAction { None, Copy, Cut }


		internal class EditBuffer {

			public EditBuffer() {
				action = EditAction.None;
				initialMousePos = Geometry.InvalidPoint;
				pasteCount = 0;
				shapes = new ShapeCollection();
			}

			public bool IsEmpty {
				get { return shapes.Count == 0; }
			}
			
			public void Clear() {
				initialMousePos = Geometry.InvalidPoint;
				action = EditAction.None;
				pasteCount = 0;
				shapes.Clear();
			}

			public Point initialMousePos;

			public EditAction action;

			public int pasteCount;

			public bool withModelObjects;

			public ShapeCollection shapes;
		}

		#endregion


		#region [Internal] Properties 

		internal IReadOnlyCollection<DiagramController> DiagramControllers {
			get { return diagramControllers; }
		}

		#endregion


		#region [Public/Internal] Methods

		// ToDo: Make these methods protected internal as soon as the WinFormsUI.Display class 
		// is split into DiagramPresenter and Display:IDiagramView
		public DiagramController OpenDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			return DoAddDiagramController(diagram);
		}


		public DiagramController OpenDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertProjectIsOpen();
			// Try to find diagram with given projectName
			Diagram diagram = null;
			foreach (Diagram d in project.Repository.GetDiagrams()) {
				if (d.Name == name) {
					diagram = d;
					break;
				}
			}
			// If a suitable diagram was found, create a diagramController for it
			if (diagram == null) return null;
			else return DoAddDiagramController(diagram);
		}

		#endregion


		#region [Private] Methods: Registering event handlers

		private void RegisterProjectEvents() {
			project.Opened += project_ProjectOpen;
			project.Closed += project_ProjectClosed;
			if (project.IsOpen) RegisterRepositoryEvents();
		}


		private void UnregisterProjectEvents(){
			project.Opened -= project_ProjectOpen;
			project.Closed -= project_ProjectClosed;
		}


		private void RegisterRepositoryEvents() {
			project.Repository.DiagramInserted += Repository_DiagramInserted;
			project.Repository.DiagramDeleted += Repository_DiagramDeleted;
			
			project.Repository.DesignUpdated += Repository_DesignUpdated;

			project.Repository.TemplateShapeReplaced += Repository_TemplateShapeReplaced;

			project.Repository.ShapesInserted += Repository_ShapesInserted;
			project.Repository.ShapesDeleted += Repository_ShapesDeleted;
		}


		private void UnregisterRepositoryEvents() {
			project.Repository.DiagramInserted -= Repository_DiagramInserted;
			project.Repository.DiagramDeleted -= Repository_DiagramDeleted;
			
			project.Repository.DesignUpdated -= Repository_DesignUpdated;
			
			project.Repository.TemplateShapeReplaced -= Repository_TemplateShapeReplaced;

			project.Repository.ShapesInserted -= Repository_ShapesInserted;
			project.Repository.ShapesDeleted -= Repository_ShapesDeleted;
		}

		#endregion


		#region [Private] Methods: Event handler implementations

		private void project_ProjectClosed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
		}

		
		private void project_ProjectOpen(object sender, EventArgs e) {
			Debug.Assert(project.Repository != null);
			RegisterRepositoryEvents();
		}

	
		private void Repository_DesignUpdated(object sender, RepositoryDesignEventArgs e) {
			// nothing to do
		}


		private void Repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			CloseDiagram(e.Diagram);
		}


		private void Repository_DiagramInserted(object sender, RepositoryDiagramEventArgs e) {
			// nothing to do
		}


		private void Repository_TemplateShapeReplaced(object sender, RepositoryTemplateShapeReplacedEventArgs e) {
			// Nothing to do here... Should be done by the ReplaceShapeCommand

			//foreach (Diagram diagram in Diagrams) {
			//   foreach (Shape oldShape in diagram.Shapes) {
			//      if (oldShape.Template == e.Template) {
			//         Shape newShape = e.Template.CreateShape();
			//         newShape.CopyFrom(oldShape);
			//         diagram.Shapes.Replace(oldShape, newShape);
			//      }
			//   }
			//}
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			// Check if the deleted shapes still exists in its diagram and remove them in this case
			foreach (Shape s in e.Shapes) {
				if (s.Diagram != null) {
					Diagram d = s.Diagram;
					d.Shapes.Remove(s);
				}
			}
		}


		private void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
			// Insert shapes that are not yet part of its diagram
			foreach (Shape shape in e.Shapes) {
				Diagram d = e.GetDiagram(shape);
				if (d != null && !d.Shapes.Contains(shape))
					d.Shapes.Add(shape);
			}
		}

		#endregion


		#region [Private] Methods

		private void AssertProjectIsOpen() {
			if (project == null) throw new NShapePropertyNotSetException(this, "Project");
			if (!project.IsOpen) throw new NShapeException("Project is not open.");
		}


		private int IndexOf(string name) {
			for (int i = diagramControllers.Count - 1; i >= 0; --i) {
				if (diagramControllers[i].Diagram.Name == name)
					return i;
			}
			return -1;
		}
		
		
		private int DiagramControllerIndexOf(Diagram diagram) {
			for (int i = diagramControllers.Count - 1; i >= 0; --i) {
				if (diagramControllers[i].Diagram == diagram)
					return i;
			}
			return -1;
		}
		
		
		private DiagramController DoAddDiagramController(Diagram diagram) {
			if (DiagramControllerIndexOf(diagram) >= 0) throw new ArgumentException("The diagram was already opened.");
			DiagramController controller= new DiagramController(this, diagram);
			diagramControllers.Add(controller);
			if (DiagramAdded != null) DiagramAdded(this, GetDiagramEventArgs(controller.Diagram));
			return controller;
		}


		private DiagramEventArgs GetDiagramEventArgs(Diagram diagram) {
			diagramEventArgs.Diagram = diagram;
			return diagramEventArgs;
		}


		private DiagramShapeEventArgs GetShapeEventArgs(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			diagramShapeEventArgs.SetDiagramShapes(shape, diagram);
			return diagramShapeEventArgs;
		}


		private DiagramShapeEventArgs GetShapeEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			diagramShapeEventArgs.SetDiagramShapes(shapes, diagram);
			return diagramShapeEventArgs;
		}


		private ModelObjectsEventArgs GetModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			modelObjectEventArgs.SetModelObjects(modelObjects);
			return modelObjectEventArgs;
		}

		#endregion


		#region Fields

		private Project project = null;
		private Tool tool;
		private ReadOnlyList<DiagramController> diagramControllers = new ReadOnlyList<DiagramController>();

		// Cut'n'Paste buffers
		private EditBuffer editBuffer = new EditBuffer();		// Buffer for Copy/Cut/Paste-Actions
		private Rectangle copyCutBounds = Rectangle.Empty;
		private Point copyCutMousePos = Point.Empty;
		// Other buffers
		private List<Shape> shapeBuffer = new List<Shape>();
		private List<IModelObject> modelBuffer = new List<IModelObject>();
		// EventArgs buffers
		private DiagramEventArgs diagramEventArgs = new DiagramEventArgs();
		private DiagramShapeEventArgs diagramShapeEventArgs = new DiagramShapeEventArgs();
		private ModelObjectsEventArgs modelObjectEventArgs = new ModelObjectsEventArgs();

		#endregion
	}


	#region Enums

	public enum IndicatorDrawMode { Normal, Highlighted, Deactivated };


	/// <summary>
	/// This is the NShape representation of System.Windows.Forms.MouseButtons (Framework 2.0)
	/// </summary>
	[Flags]
	public enum MouseButtonsDg {
		/// <summary>
		/// No mouse button was pressed.
		/// </summary>
		None = 0,
		/// <summary>
		/// The left mouse button was pressed.
		/// </summary>
		Left = 0x100000,
		/// <summary>
		/// The right mouse button was pressed.
		/// </summary>
		Right = 0x200000,
		/// <summary>
		/// The middle mouse button was pressed.
		/// </summary>
		Middle = 0x400000,
		/// <summary>
		/// The first XButton was pressed.
		/// </summary>
		ExtraButton1 = 0x800000,
		/// <summary>
		/// The second XButton was pressed.
		/// </summary>
		ExtraButton2 = 0x1000000,
	}


	public enum MouseEventType { MouseDown, MouseMove, MouseUp };


	public enum KeyEventType { KeyDown, KeyPress, KeyUp, PreviewKeyDown }


	/// <summary>
	/// This is the NShape representation of System.Windows.Forms.Keys (Framework 2.0)
	/// </summary>
	[Flags]
	public enum KeysDg {
		A = 0x41,
		Add = 0x6b,
		Alt = 0x40000,
		Apps = 0x5d,
		Attn = 0xf6,
		B = 0x42,
		Back = 8,
		BrowserBack = 0xa6,
		BrowserFavorites = 0xab,
		BrowserForward = 0xa7,
		BrowserHome = 0xac,
		BrowserRefresh = 0xa8,
		BrowserSearch = 170,
		BrowserStop = 0xa9,
		C = 0x43,
		Cancel = 3,
		Capital = 20,
		CapsLock = 20,
		Clear = 12,
		Control = 0x20000,
		ControlKey = 0x11,
		Crsel = 0xf7,
		D = 0x44,
		D0 = 0x30,
		D1 = 0x31,
		D2 = 50,
		D3 = 0x33,
		D4 = 0x34,
		D5 = 0x35,
		D6 = 0x36,
		D7 = 0x37,
		D8 = 0x38,
		D9 = 0x39,
		Decimal = 110,
		Delete = 0x2e,
		Divide = 0x6f,
		Down = 40,
		E = 0x45,
		End = 0x23,
		Enter = 13,
		EraseEof = 0xf9,
		Escape = 0x1b,
		Execute = 0x2b,
		Exsel = 0xf8,
		F = 70,
		F1 = 0x70,
		F10 = 0x79,
		F11 = 0x7a,
		F12 = 0x7b,
		F13 = 0x7c,
		F14 = 0x7d,
		F15 = 0x7e,
		F16 = 0x7f,
		F17 = 0x80,
		F18 = 0x81,
		F19 = 130,
		F2 = 0x71,
		F20 = 0x83,
		F21 = 0x84,
		F22 = 0x85,
		F23 = 0x86,
		F24 = 0x87,
		F3 = 0x72,
		F4 = 0x73,
		F5 = 0x74,
		F6 = 0x75,
		F7 = 0x76,
		F8 = 0x77,
		F9 = 120,
		FinalMode = 0x18,
		G = 0x47,
		H = 0x48,
		HanguelMode = 0x15,
		HangulMode = 0x15,
		HanjaMode = 0x19,
		Help = 0x2f,
		Home = 0x24,
		I = 0x49,
		IMEAccept = 30,
		IMEAceept = 30,
		IMEConvert = 0x1c,
		IMEModeChange = 0x1f,
		IMENonconvert = 0x1d,
		Insert = 0x2d,
		J = 0x4a,
		JunjaMode = 0x17,
		K = 0x4b,
		KanaMode = 0x15,
		KanjiMode = 0x19,
		KeyCode = 0xffff,
		L = 0x4c,
		LaunchApplication1 = 0xb6,
		LaunchApplication2 = 0xb7,
		LaunchMail = 180,
		LButton = 1,
		LControlKey = 0xa2,
		Left = 0x25,
		LineFeed = 10,
		LMenu = 0xa4,
		LShiftKey = 160,
		LWin = 0x5b,
		M = 0x4d,
		MButton = 4,
		MediaNextTrack = 0xb0,
		MediaPlayPause = 0xb3,
		MediaPreviousTrack = 0xb1,
		MediaStop = 0xb2,
		Menu = 0x12,
		Modifiers = -65536,
		Multiply = 0x6a,
		N = 0x4e,
		Next = 0x22,
		NoName = 0xfc,
		None = 0,
		NumLock = 0x90,
		NumPad0 = 0x60,
		NumPad1 = 0x61,
		NumPad2 = 0x62,
		NumPad3 = 0x63,
		NumPad4 = 100,
		NumPad5 = 0x65,
		NumPad6 = 0x66,
		NumPad7 = 0x67,
		NumPad8 = 0x68,
		NumPad9 = 0x69,
		O = 0x4f,
		Oem1 = 0xba,
		Oem102 = 0xe2,
		Oem2 = 0xbf,
		Oem3 = 0xc0,
		Oem4 = 0xdb,
		Oem5 = 220,
		Oem6 = 0xdd,
		Oem7 = 0xde,
		Oem8 = 0xdf,
		OemBackslash = 0xe2,
		OemClear = 0xfe,
		OemCloseBrackets = 0xdd,
		Oemcomma = 0xbc,
		OemMinus = 0xbd,
		OemOpenBrackets = 0xdb,
		OemPeriod = 190,
		OemPipe = 220,
		Oemplus = 0xbb,
		OemQuestion = 0xbf,
		OemQuotes = 0xde,
		OemSemicolon = 0xba,
		Oemtilde = 0xc0,
		P = 80,
		Pa1 = 0xfd,
		Packet = 0xe7,
		PageDown = 0x22,
		PageUp = 0x21,
		Pause = 0x13,
		Play = 250,
		Print = 0x2a,
		PrintScreen = 0x2c,
		Prior = 0x21,
		ProcessKey = 0xe5,
		Q = 0x51,
		R = 0x52,
		RButton = 2,
		RControlKey = 0xa3,
		Return = 13,
		Right = 0x27,
		RMenu = 0xa5,
		RShiftKey = 0xa1,
		RWin = 0x5c,
		S = 0x53,
		Scroll = 0x91,
		Select = 0x29,
		SelectMedia = 0xb5,
		Separator = 0x6c,
		Shift = 0x10000,
		ShiftKey = 0x10,
		Sleep = 0x5f,
		Snapshot = 0x2c,
		Space = 0x20,
		Subtract = 0x6d,
		T = 0x54,
		Tab = 9,
		U = 0x55,
		Up = 0x26,
		V = 0x56,
		VolumeDown = 0xae,
		VolumeMute = 0xad,
		VolumeUp = 0xaf,
		W = 0x57,
		X = 0x58,
		XButton1 = 5,
		XButton2 = 6,
		Y = 0x59,
		Z = 90,
		Zoom = 0xfb
	}

	#endregion


	#region EventArgs

	public class MouseEventArgsDg : EventArgs {

		public MouseEventArgsDg(MouseEventType eventType, MouseButtonsDg buttons, int clicks, int delta, Point location, KeysDg modifiers) {
			this.buttons = buttons;
			this.clicks = clicks;
			this.wheelDelta = delta;
			this.eventType = eventType;
			this.position = location;
			this.modifiers = modifiers;
		}


		/// <summary>
		/// Contains the type of MouseEvent that was raised.
		/// </summary>
		public MouseEventType EventType {
			get { return eventType; }
		}


		/// <summary>
		/// Contains a combination of all MouseButtons that were pressed.
		/// </summary>
		public MouseButtonsDg Buttons {
			get { return buttons; }
		}


		/// <summary>
		/// Contains the number of clicks.
		/// </summary>
		public int Clicks {
			get { return clicks; }
		}


		/// <summary>
		/// Contains a (signed) count of the number of detents the mouse wheel was rotated.
		/// A detent is one notch of the mouse wheel.
		/// </summary>
		public int WheelDelta {
			get { return wheelDelta; }
		}


		/// <summary>
		/// Contains the position (in diagram coordinates) of the mouse cursor at the time the event was raised.
		/// </summary>
		public Point Position {
			get { return position; }
		}


		/// <summary>
		/// Contains the modifiers in case any modifier keys were pressed
		/// </summary>
		public KeysDg Modifiers {
			get { return modifiers; }
		}


		protected internal MouseEventArgsDg() {
			this.buttons = MouseButtonsDg.None;
			this.clicks = 0;
			this.wheelDelta = 0;
			this.eventType = MouseEventType.MouseMove;
			this.position = Point.Empty;
		}


		#region Fields

		protected MouseEventType eventType;
		protected MouseButtonsDg buttons;
		protected Point position;
		protected int wheelDelta;
		protected int clicks;
		protected KeysDg modifiers;
		
		#endregion
	}


	public class KeyEventArgsDg : EventArgs {

		public KeyEventArgsDg(KeyEventType eventType, int keyData, char keyChar, bool handled, bool suppressKeyPress) {
			this.eventType = eventType;
			this.handled = handled;
			this.keyChar = keyChar;
			this.keyData = keyData;
			this.suppressKeyPress = suppressKeyPress;
		}


		public KeyEventType EventType {
			get { return eventType; }
		}


		public char KeyChar {
			get { return keyChar; }
		}


		public int KeyData {
			get { return keyData; }
		}


		public bool Handled {
			get { return handled; }
			set { handled = value; }
		}


		public bool SuppressKeyPress {
			get { return suppressKeyPress; }
			set { suppressKeyPress = value; }
		}


		public bool Control {
			get { return (keyData & control) == control; }
		}


		public bool Shift {
			get { return (keyData & shift) == shift; }
		}


		public bool Alt {
			get { return (keyData & alt) == alt; }
		}


		public int KeyCode {
			get { return keyData & keyCode; }
		}


		public int Modifiers {
			get { return (keyData & ~keyCode); }
		}

		/// <summary>
		/// The bitmask to extract modifiers from a key value.
		/// </summary>
		protected const int modifiers = -65536;

		/// <summary>
		/// The bitmask to extract a key code from a key value.
		/// </summary>
		protected const int keyCode = 65535;

		/// <summary>
		/// The SHIFT modifier key.
		/// </summary>
		protected const int shift = 65536;

		/// <summary>
		/// The CTRL modifier key.
		/// </summary>
		protected const int control = 131072;

		/// <summary>
		/// The ALT modifier key.
		/// </summary>
		protected const int alt = 262144;

		protected KeyEventType eventType;
		protected char keyChar;
		protected int keyData;
		protected bool handled;
		protected bool suppressKeyPress;
	}


	public class DiagramEventArgs : EventArgs {
		
		public DiagramEventArgs(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
		}

		public Diagram Diagram {
			get { return diagram; }
			internal set { diagram = value; }
		}

		internal DiagramEventArgs() { }

		private Diagram diagram;

	}


	public class ShapeEventArgs : EventArgs {

		public ShapeEventArgs(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.shapes.AddRange(shapes);
		}

		public IReadOnlyCollection<Shape> Shapes {
			get { return shapes; }
		}


		internal ShapeEventArgs() {
			this.shapes.Clear();
		}

		internal void SetShapes(IEnumerable<Shape> shapes) {
			this.shapes.Clear();
			this.shapes.AddRange(shapes);
		}

		internal void SetShape(Shape shape) {
			this.shapes.Clear();
			this.shapes.Add(shape);
		}

		private ReadOnlyList<Shape> shapes = new ReadOnlyList<Shape>();
	}


	public class ModelObjectsEventArgs : EventArgs {

		public ModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			this.modelObjects.AddRange(modelObjects);
		}

		public IReadOnlyCollection<IModelObject> ModelObjects {
			get { return modelObjects; }
		}


		internal ModelObjectsEventArgs() {
			this.modelObjects.Clear();
		}

		internal void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			this.modelObjects.Clear();
			this.modelObjects.AddRange(modelObjects);
		}

		internal void SetModelObject(IModelObject modelObject) {
			this.modelObjects.Clear();
			this.modelObjects.Add(modelObject);
		}

		private ReadOnlyList<IModelObject> modelObjects = new ReadOnlyList<IModelObject>();
	}


	public class DiagramShapeEventArgs : ShapeEventArgs {
		
		public DiagramShapeEventArgs(IEnumerable<Shape> shapes, Diagram diagram)
			: base(shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
		}


		public Diagram Diagram {
			get { return diagram; }
		}


		internal DiagramShapeEventArgs()
			: base() {
		}


		internal void SetDiagram(Diagram diagram) {
			this.diagram = diagram;
		}
		
		
		internal void SetDiagramShapes(IEnumerable<Shape> shapes, Diagram diagram) {
			SetShapes(shapes);
			SetDiagram(diagram);
		}


		internal void SetDiagramShapes(Shape shape, Diagram diagram) {
			SetShape(shape);
			SetDiagram(diagram);
		}


		private Diagram diagram;
	}


	public class ShapeMouseEventArgs : MouseEventArgsDg {

		public ShapeMouseEventArgs(IEnumerable<Shape> shapes, Diagram diagram, MouseEventType eventType, MouseButtonsDg buttons, int clicks, int delta, Point location, KeysDg modifiers)
			: base(eventType, buttons, clicks, delta, location, modifiers) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.shapes.AddRange(shapes);
			this.diagram = diagram;
		}

		public IReadOnlyCollection<Shape> Shapes { get { return shapes; } }

		internal ShapeMouseEventArgs()
			: base() {
			this.shapes.Clear();
		}

		internal void SetShapes(IEnumerable<Shape> shapes) {
			this.shapes.Clear();
			this.shapes.AddRange(shapes);
		}

		internal void SetShape(Shape shape) {
			this.shapes.Clear();
			this.shapes.Add(shape);
		}

		private ReadOnlyList<Shape> shapes = new ReadOnlyList<Shape>();
		private Diagram diagram = null;
	}

	#endregion


	#region Exceptions

	public class DiagramControllerNotFoundException : NShapeException {
		public DiagramControllerNotFoundException(Diagram diagram)
			: base("No {0} found for {1} '{2}'", typeof(DiagramController).Name, typeof(Diagram).Name, (diagram != null) ? diagram.Name : string.Empty) {
		}
	}

	#endregion

}
