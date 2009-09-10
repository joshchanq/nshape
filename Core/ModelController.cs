using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Dataweb.nShape.Advanced;
using System.Reflection;
using System.Collections;
using System.Drawing;


namespace Dataweb.nShape.Controllers {

	public class ModelController : Component, IPropertyController {

		public ModelController() {
		}


		public ModelController(DiagramSetController diagramSetController) {
			if (diagramSetController == null) throw new ArgumentNullException("diagramSetController");
			DiagramSetController = diagramSetController;
		}


		public ModelController(Project project)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			Project = project;
		}


		#region IPropertyController Members

		public event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		public event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		public event EventHandler<PropertyControllerEventArgs> RefreshObjects;

		public event EventHandler<PropertyControllerEventArgs> ObjectsDeleted;

		public event EventHandler ProjectClosing;


		[Browsable(false)]
		public bool ReadOnly {
			get {
				if (diagramSetController != null) return diagramSetController.ReadOnly;
				else return (PropertyController != null) ? PropertyController.ReadOnly : true; 
			}
		}

		
		public void SetObject(int pageIndex, object selectedObject) {
			if (Project == null) throw new InvalidOperationException("Project property is not set.");
			if (diagramSetController != null) diagramSetController.SetObject(pageIndex, selectedObject);
			else PropertyController.SetObject(pageIndex, selectedObject, selectedObjectsChangedCallback);
		}


		public void SetObjects(int pageIndex, IEnumerable selectedObjects) {
			if (Project == null) throw new InvalidOperationException("Project property is not set.");
			if (diagramSetController != null) diagramSetController.SetObjects(pageIndex, selectedObjects);
			else PropertyController.SetObjects(pageIndex, selectedObjects, selectedObjectsChangedCallback);
		}


		public void SelectedObjectsChanged(int pageIndex, IEnumerable<object> modifiedObjects, PropertyInfo propertyInfo, object[] oldValues, object newValue) {
			if (Project == null) throw new InvalidOperationException("Project property is not set.");
			if (diagramSetController != null) diagramSetController.SelectedObjectsChanged(pageIndex, modifiedObjects, propertyInfo, oldValues, newValue);
			else PropertyController.SelectedObjectsChanged(pageIndex, modifiedObjects, propertyInfo, oldValues, newValue);
		}

		#endregion

	
		#region [Public] Events

		public event EventHandler Initialized;

		public event EventHandler Uninitialized;

		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsCreated;

		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsChanged;

		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		/// <summary>
		/// The Changed event will be raised whenever an object somehow related to a model object has changed.
		/// </summary>
		public event EventHandler Changed;

		#endregion


		#region [Public] Properties

		[ReadOnly(true)]
		public Project Project {
			get { return (diagramSetController == null) ? project : diagramSetController.Project; }
			set {
				if (diagramSetController != null && diagramSetController.Project != value) {
					string errMsg = string.Format("A {0} is already assigned. Its project will be used.", diagramSetController.GetType().Name);
					throw new InvalidOperationException(errMsg);
				}
				if (Project != value) {
					DetachProject();
					project = value;
					AttachProject();
				}
			}
		}


		public DiagramSetController DiagramSetController {
			get { return diagramSetController; }
			set {
				if (Project != null) DetachProject();
				if (diagramSetController != null) UnregisterDiagramSetControllerEvents();
				diagramSetController = value;
				if (diagramSetController != null) {
					RegisterDiagramSetControllerEvents();
					AttachProject();
				}
			}
		}

		#endregion


		#region [Public] Methods

		public void CreateModelObject() {
			throw new NotImplementedException();
		}


		public void RenameModelObject(IModelObject modelObject, string newName) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			throw new NotImplementedException();
		}


		/// <summary>
		/// Deletes the given model obejcts and their attached shapes
		/// </summary>
		/// <param name="modelObjects"></param>
		public void DeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			ICommand cmd = new DeleteModelObjectsCommand (modelObjects);
			Project.ExecuteCommand(cmd);
		}


		public void SetModelObjectParent(IModelObject modelObject, IModelObject parent) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			ICommand cmd = new SetModelObjectParentCommand(modelObject, parent);
			Project.ExecuteCommand(cmd);
		}


		public void Copy(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			copyPasteBuffer.Clear();
			copyPasteBuffer.Add(modelObject.Clone());
		}


		public void Copy(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			copyPasteBuffer.Clear();
			foreach (IModelObject mo in modelObjects)
				copyPasteBuffer.Add(mo.Clone());
		}


		public void Paste(IModelObject parent) {
			// Set parent
			for (int i = copyPasteBuffer.Count - 1; i >= 0; --i)
				copyPasteBuffer[i].Parent = parent;
			// Execute command
			ICommand command = new InsertModelObjectsCommand(copyPasteBuffer);
			Project.ExecuteCommand(command);
			// Copy for next paste action
			for (int i = copyPasteBuffer.Count - 1; i >= 0; --i)
				copyPasteBuffer[i] = copyPasteBuffer[i].Clone();
		}


		public IEnumerable<IModelObject> GetChildModelObjects(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			return Project.Repository.GetModelObjects(modelObject);
		}


		public void FindShapes(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			if (diagramSetController == null) throw new InvalidOperationException("DiagramSetController is not set");
			diagramSetController.SelectModelObjects(modelObjects);
		}


		public IEnumerable<nShapeAction> GetActions(IReadOnlyCollection<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			
			// New...
			// Rename
			yield return CreateDeleteModelObjectsAction(modelObjects);
			
			yield return new SeparatorAction();

			yield return CreateCopyModelObjectsAction(modelObjects);
			// Cut
			yield return CreatePasteModelObjectsAction(modelObjects);

			yield return new SeparatorAction();
			
			// Find model object...
			yield return CreateFindShapesAction(modelObjects);
		}

		#endregion


		private PropertyController PropertyController {
			get {
				if (diagramSetController != null) return null;
				else {
					if (PropertyController == null && project != null)
						propertyController = new PropertyController(project);
					return propertyController;
				}
			}
		}


		private void selectedObjectsChangedCallback(PropertyControllerPropertyChangedEventArgs propertyChangedEventArgs) {
			throw new NotImplementedException();
		}


		#region [Private] Methods: (Un)Registering event handlers

		private void DetachProject() {
			if (Project != null) {
				// Unregister and delete property controller
				UnregisterPropertyControllerEvents();
				propertyController = null;
				// Unregister current project
				UnregisterProjectEvents();
				project = null;
			}
		}


		private void AttachProject() {
			if (Project != null) {
				// Register current project
				RegisterProjectEvents();
				// Register property controller events
				RegisterPropertyControllerEvents();
			}
		}


		private void RegisterDiagramSetControllerEvents() {
			Debug.Assert(diagramSetController != null);
			diagramSetController.ProjectChanged += diagramSetController_ProjectChanged;
			diagramSetController.ProjectChanging += diagramSetController_ProjectChanging;
		}


		private void UnregisterDiagramSetControllerEvents() {
			Debug.Assert(diagramSetController != null);
			diagramSetController.ProjectChanged -= diagramSetController_ProjectChanged;
			diagramSetController.ProjectChanging -= diagramSetController_ProjectChanging;
		}


		private void RegisterProjectEvents() {
			Debug.Assert(Project != null);

			// Register project events
			Project.Opened += project_ProjectOpen;
			Project.Closing += project_ProjectClosing;
			Project.Closed += project_ProjectClosed;
			
			// Register repository events
			if (Project.IsOpen) project_ProjectOpen(this, null);
		}


		private void UnregisterProjectEvents() {
			Debug.Assert(Project != null);

			// Unregister repository events
			if (Project.Repository != null) UnregisterRepositoryEvents();
			
			// Unregister project events
			Project.Opened -= project_ProjectOpen;
			Project.Closing -= project_ProjectClosing;
			Project.Closed -= project_ProjectClosed;
		}


		private void RegisterRepositoryEvents() {
			Debug.Assert(Project != null && Project.Repository != null);
			Project.Repository.ModelObjectsInserted += repository_ModelObjectsInserted;
			Project.Repository.ModelObjectsUpdated += repository_ModelObjectsUpdated;
			Project.Repository.ModelObjectsDeleted += repository_ModelObjectsDeleted;
			Project.Repository.TemplateInserted += repository_TemplateInserted;
			Project.Repository.TemplateUpdated += repository_TemplateUpdated;
			Project.Repository.TemplateDeleted += repository_TemplateDeleted;
			Project.Repository.TemplateShapeReplaced += repository_TemplateShapeReplaced;
		}


		private void UnregisterRepositoryEvents() {
			Debug.Assert(Project != null && Project.Repository != null);
			Project.Repository.ModelObjectsInserted -= repository_ModelObjectsInserted;
			Project.Repository.ModelObjectsUpdated -= repository_ModelObjectsUpdated;
			Project.Repository.ModelObjectsDeleted -= repository_ModelObjectsDeleted;
			Project.Repository.TemplateInserted -= repository_TemplateInserted;
			Project.Repository.TemplateUpdated -= repository_TemplateUpdated;
			Project.Repository.TemplateDeleted -= repository_TemplateDeleted;
			Project.Repository.TemplateShapeReplaced -= repository_TemplateShapeReplaced;
		}


		private void RegisterPropertyControllerEvents() {
			if (diagramSetController != null) {
				diagramSetController.ObjectsDeleted += propertyController_ObjectsDeleted;
				diagramSetController.ObjectsSet += propertyController_ObjectsSet;
				diagramSetController.ProjectClosing += propertyController_ProjectClosing;
				diagramSetController.PropertyChanged += propertyController_PropertyChanged;
				diagramSetController.RefreshObjects += propertyController_RefreshObjects;
			} else {
				Debug.Assert(Project != null);
				PropertyController.ObjectsDeleted += propertyController_ObjectsDeleted;
				PropertyController.ObjectsSet += propertyController_ObjectsSet;
				PropertyController.ProjectClosing += propertyController_ProjectClosing;
				PropertyController.PropertyChanged += propertyController_PropertyChanged;
				PropertyController.RefreshObjects += propertyController_RefreshObjects;
			}
		}


		private void UnregisterPropertyControllerEvents() {
			if (diagramSetController != null) {
				diagramSetController.ObjectsDeleted += propertyController_ObjectsDeleted;
				diagramSetController.ObjectsSet += propertyController_ObjectsSet;
				diagramSetController.ProjectClosing += propertyController_ProjectClosing;
				diagramSetController.PropertyChanged += propertyController_PropertyChanged;
				diagramSetController.RefreshObjects += propertyController_RefreshObjects;
			} else {
				Debug.Assert(Project != null);
				PropertyController.ObjectsDeleted -= propertyController_ObjectsDeleted;
				PropertyController.ObjectsSet -= propertyController_ObjectsSet;
				PropertyController.ProjectClosing -= propertyController_ProjectClosing;
				PropertyController.PropertyChanged -= propertyController_PropertyChanged;
				PropertyController.RefreshObjects -= propertyController_RefreshObjects;
			}
		}

		#endregion


		#region [Private] Methods: DiagramSetController event handler implementations

		private void diagramSetController_ProjectChanged(object sender, EventArgs e) {
			if (diagramSetController.Project != null) AttachProject();
		}


		private void diagramSetController_ProjectChanging(object sender, EventArgs e) {
			if (diagramSetController.Project != null) DetachProject();
		}

		#endregion


		#region [Private] Methods: Project event handler implementations

		private void project_ProjectOpen(object sender, EventArgs e) {
			RegisterRepositoryEvents();
			if (Initialized != null) Initialized(this, new EventArgs());
		}


		private void project_ProjectClosing(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
			if (Uninitialized != null) Uninitialized(this, new EventArgs());
		}


		private void project_ProjectClosed(object sender, EventArgs e) {
			// nothing to do here
		}

		#endregion


		#region [Private] Methods: Repository event handler implementations

		private void repository_ModelObjectsInserted(object sender, RepositoryModelObjectsEventArgs e) {
			if (ModelObjectsCreated != null) ModelObjectsCreated(this, e);
		}


		private void repository_ModelObjectsUpdated(object sender, RepositoryModelObjectsEventArgs e) {
			if (ModelObjectsChanged != null) ModelObjectsChanged(this, e);
		}


		private void repository_ModelObjectsDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		private void repository_TemplateShapeReplaced(object sender, RepositoryTemplateShapeReplacedEventArgs e) {
			//ToDo: Refresh TreeView icon
			if (Changed != null) Changed(this, new EventArgs());
		}


		private void repository_TemplateInserted(object sender, RepositoryTemplateEventArgs e) {
			// nothing to do
			if (Changed != null) Changed(this, new EventArgs());
		}


		private void repository_TemplateUpdated(object sender, RepositoryTemplateEventArgs e) {
			//ToDo: Refresh TreeView icon
			if (Changed != null) Changed(this, new EventArgs());
		}


		private void repository_TemplateDeleted(object sender, RepositoryTemplateEventArgs e) {
			//ToDo: Refresh TreeView icon
		}

		#endregion


		#region [Private] Methods: PropertyController event handler implementations

		private void propertyController_RefreshObjects(object sender, PropertyControllerEventArgs e) {
			if (RefreshObjects != null) RefreshObjects(this, e);
		}


		private void propertyController_PropertyChanged(object sender, PropertyControllerPropertyChangedEventArgs e) {
			if (PropertyChanged != null) PropertyChanged(this, e);
		}


		private void propertyController_ProjectClosing(object sender, EventArgs e) {
			if (ProjectClosing != null) ProjectClosing(this, e);
		}


		private void propertyController_ObjectsSet(object sender, PropertyControllerEventArgs e) {
			if (ObjectsSet != null) ObjectsSet(this, e);
		}


		private void propertyController_ObjectsDeleted(object sender, PropertyControllerEventArgs e) {
			if (ObjectsDeleted != null) ObjectsDeleted(sender, e);
		}

		#endregion


		#region [Private] Methods: Create actions

		private nShapeAction CreateDeleteModelObjectsAction(IReadOnlyCollection<IModelObject> modelObjects) {
			string description;
			bool isFeasible;
			if (modelObjects != null && modelObjects.Count > 0) {
				isFeasible = true;
				description = string.Format("Delete {0} model object{1}.", modelObjects.Count, (modelObjects.Count > 0) ? "s" : string.Empty);
				foreach (IModelObject modelObject in modelObjects)
					foreach (IModelObject mo in Project.Repository.GetModelObjects(modelObject))
						foreach (Shape s in mo.Shapes) {
							isFeasible = false;
							description = "One or more child model objects are attached to shapes.";
							break;
						}
			} else {
				isFeasible = false;
				description = "No model objects selected";
			}

			return new DelegateAction("Delete", null, Color.Empty, "DeleteModelObjectsAction",
				description, false, isFeasible, Permission.None,
				(a, p) => DeleteModelObjects(modelObjects));
		}


		private nShapeAction CreateCopyModelObjectsAction(IReadOnlyCollection<IModelObject> modelObjects) {
			bool isFeasible = (modelObjects != null && modelObjects.Count > 0);
			string description;
			if (isFeasible)
				description = string.Format("Copy {0} model object{1}.", modelObjects.Count, (modelObjects.Count > 1) ? "s" : string.Empty);
			else description = "No model objects selected";
			return new DelegateAction("Copy", null, Color.Empty, "CopyModelObjectsAction",
				description, false, isFeasible, Permission.None,
				(a, p) => Copy(modelObjects));
		}


		private nShapeAction CreatePasteModelObjectsAction(IReadOnlyCollection<IModelObject> modelObjects) {
			bool isFeasible = (copyPasteBuffer.Count > 0 && modelObjects.Count <= 1);
			string description;
			if (isFeasible)
				description = string.Format("Paste {0} model object{1}.", copyPasteBuffer.Count, (copyPasteBuffer.Count > 1) ? "s" : string.Empty);
			else description = "No model objects copied.";
			
			IModelObject parent = null;
			foreach (IModelObject mo in modelObjects) {
				parent = mo;
				break;
			}
			return new DelegateAction("Paste", null, Color.Empty, "DeleteModelObjectsAction",
				description, false, isFeasible, Permission.None,
				(a, p) => Paste(parent));
		}


		private nShapeAction CreateFindShapesAction(IReadOnlyCollection<IModelObject> modelObjects) {
			bool isFeasible = (diagramSetController != null);
			string description = "Find and select all assigned shapes.";
			return new DelegateAction("Find assigned shapes", null, Color.Empty, "FindShapesAction",
				description, false, isFeasible, Permission.None,
				(a, p) => FindShapes(modelObjects));
		}

		#endregion


		#region Fields

		private DiagramSetController diagramSetController;
		private Project project;
		private PropertyController propertyController;

		private List<IModelObject> copyPasteBuffer = new List<IModelObject>();

		#endregion
	}


	public class ModelObjectSelectedEventArgs : EventArgs {

		public ModelObjectSelectedEventArgs(IEnumerable<IModelObject> selectedModelObjects, bool ensureVisibility) {
			if (selectedModelObjects == null) throw new ArgumentNullException("selectedModelObjects");
			this.modelObjects = new List<IModelObject>(selectedModelObjects);
			this.ensureVisibility = ensureVisibility;
		}

		public IEnumerable<IModelObject> SelectedModelObjects {
			get { return modelObjects; }
			internal set {
				modelObjects.Clear();
				if (value != null) modelObjects.AddRange(value);
			}
		}

		public bool EnsureVisibility {
			get { return ensureVisibility; }
			internal set { ensureVisibility = value; }
		}

		internal ModelObjectSelectedEventArgs() {
			modelObjects = new List<IModelObject>();
			ensureVisibility = false;
		}
		
		private List<IModelObject> modelObjects;
		private bool ensureVisibility;
	}


	#region ModelTree Actions

	// ToDo: Define Actions for FindShape, DeleteModel, AddModel, RenameModel
	
	#endregion
}