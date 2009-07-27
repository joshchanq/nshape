using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;

using Dataweb.Diagramming.Advanced;
using System.Collections;


namespace Dataweb.Diagramming.Controllers {

	public enum TemplateControllerEditMode { CreateTemplate, EditTemplate };


	#region EventArgs

	/// <summary>
	/// Encapsulates parameters for a TemplateController event raised when the TemplateController is initializing itself.
	/// </summary>
	public class TemplateControllerInitializingEventArgs : EventArgs {

		public TemplateControllerInitializingEventArgs(TemplateControllerEditMode editMode, Template template) {
			this.editMode = editMode;
			this.template = template;
		}


		public TemplateControllerEditMode EditMode {
			get { return editMode; }
		}


		public Template Template {
			get { return template; }
		}


		private TemplateControllerEditMode editMode;
		private Template template;
	}


	/// <summary>
	/// Encapsulates parameters for a TemplateController event raised when the template's projectName is modified.
	/// </summary>
	public class TemplateControllerStringChangedEventArgs : EventArgs {

		public TemplateControllerStringChangedEventArgs(string oldString, string newString) {
			this.oldString = oldString;
			this.newString = newString;
		}


		public string OldString {
			get { return oldString; }
			internal set { oldString = value; }
		}
		

		public string NewString { 
			get { return newString; }
			internal set { newString = value; }
		}


		private string newString;
		private string oldString;
	}


	/// <summary>
	/// Encapsulates parameters for a template-related TemplateController event.
	/// </summary>
	public class TemplateControllerTemplateEventArgs : EventArgs {

		public TemplateControllerTemplateEventArgs(Template template) {
			this.template = template;
		}

		
		public Template Template {
			get { return template; }
			internal set { template = value; }
		}


		private Template template;
	}


	/// <summary>
	/// Encapsulates parameters for a TemplateController event raised when template's shape is replaced ba a shape of another Type.
	/// </summary>
	public class TemplateControllerTemplateShapeReplacedEventArgs : TemplateControllerTemplateEventArgs {

		public TemplateControllerTemplateShapeReplacedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape)
			: base(template) {
			this.oldTemplateShape = oldTemplateShape;
			this.newTemplateShape = newTemplateShape;
		}


		public Shape OldTemplateShape {
			get { return oldTemplateShape; }
			internal set { oldTemplateShape = value; }
		}


		public Shape NewTemplateShape {
			get { return newTemplateShape; }
			internal set { newTemplateShape = value; }
		}


		private Shape oldTemplateShape;
		private Shape newTemplateShape;
	}


	/// <summary>
	/// Encapsulates parameters for a TemplateController event raised when template's model object is replaced by a model object of another ModelObejctType.
	/// </summary>
	public class TemplateControllerModelObjectReplacedEventArgs : TemplateControllerTemplateEventArgs {

		public TemplateControllerModelObjectReplacedEventArgs(Template template,
			IModelObject oldModelObject, IModelObject newModelObject)
			: base(template) {
			this.oldModelObject = oldModelObject;
			this.newModelObject = newModelObject;
		}


		public IModelObject OldModelObject {
			get { return oldModelObject; }
			internal set { oldModelObject = value; }
		}


		public IModelObject NewModelObject {
			get { return newModelObject; }
			internal set { newModelObject = value; }
		}

		private IModelObject oldModelObject;
		private IModelObject newModelObject;
	}


	/// <summary>
	/// Encapsulates parameters for a TemplateController event raised when the mapping of ControlPointId to TerminalId is modified.
	/// </summary>
	public class TemplateControllerPropertyMappingChangedEventArgs : TemplateControllerTemplateEventArgs {
		public TemplateControllerPropertyMappingChangedEventArgs(Template template, IModelMapping modelMapping)
			: base(template) {
			this.propertyMapping = modelMapping;
		}

		public IModelMapping ModelMapping {
			get { return propertyMapping; }
			internal set { propertyMapping = value; }
		}

		private IModelMapping propertyMapping = null;
	}


	/// <summary>
	/// Encapsulates parameters for a TemplateController event raised when the mapping of shape's properties to modeloject's properties is modified.
	/// </summary>
	public class TemplateControllerPointMappingChangedEventArgs : TemplateControllerTemplateEventArgs {

		public TemplateControllerPointMappingChangedEventArgs(Template template, ControlPointId controlPointId, TerminalId oldTerminalId, TerminalId newTerminalId)
			: base(template) {
			this.controlPointId = controlPointId;
			this.oldTerminalId = oldTerminalId;
			this.newTerminalId = newTerminalId;
		}

		public ControlPointId ControlPointId {
			get { return controlPointId; }
			internal set { controlPointId = value; }
		}

		public TerminalId OldTerminalId {
			get { return oldTerminalId; }
			internal set { oldTerminalId = value; }
		}

		public TerminalId NewTerminalId {
			get { return newTerminalId; }
			internal set { newTerminalId = value; }
		}

		private ControlPointId controlPointId;
		private TerminalId oldTerminalId;
		private TerminalId newTerminalId;
	}

	#endregion


	/// <summary>
	/// A non-visual component for editing templates. 
	/// </summary>
	public class TemplateController : Component, IPropertyController, IDisplayService {

		/// <summary>
		/// Creates a new TemplateController instance
		/// </summary>
		public TemplateController() {
			infoGraphics = Graphics.FromHwnd(IntPtr.Zero);
		}


		/// <summary>
		/// Creates and initializes a new TemplateController instance
		/// </summary>
		/// <param name="projectInfo"></param>
		/// <param name="template"></param>
		/// <param name="newModelObject"></param>
		public TemplateController(Project project, Template template)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			Initialize(project, template);
		}


		~TemplateController() {
			PropertyController = null;
			infoGraphics.Dispose();
			infoGraphics = null;
		}


		#region IDisplayService Members

		/// <override></override>
		void IDisplayService.Invalidate(int x, int y, int width, int height) {
			// nothing to do
		}


		/// <override></override>
		void IDisplayService.Invalidate(Rectangle rectangle) {
			// nothing to do
		}


		/// <override></override>
		void IDisplayService.NotifyBoundsChanged() { 
			// nothing to do
		}


		/// <override></override>
		Graphics IDisplayService.InfoGraphics {
			get { return infoGraphics; }
		}


		IFillStyle IDisplayService.HintBackgroundStyle {
			get {
				if (Project != null && Project.IsOpen)
					return Project.Design.FillStyles.White;
				else return null;
			}
		}


		ILineStyle IDisplayService.HintForegroundStyle {
			get {
				if (Project != null && Project.IsOpen)
					return Project.Design.LineStyles.Normal;
				else return null;
			}
		}

		#endregion


		#region IPropertyController Members

		public event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		public event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		public event EventHandler<PropertyControllerEventArgs> RefreshObjects;

		public event EventHandler<PropertyControllerEventArgs> ObjectsDeleted;
		
		public event EventHandler ProjectClosing;

		public bool ReadOnly {
			get { return (propertyController != null && propertyController.ReadOnly); }
		}

		public void SetObject(int pageIndex, object selectedObject) {
			if (propertyController != null)
				propertyController.SetObject(pageIndex, selectedObject, SelectedObjectsChangedCallback);
		}

		public void SetObjects(int pageIndex, IEnumerable selectedObjects) {
			if (propertyController != null)
				propertyController.SetObjects(pageIndex, selectedObjects, SelectedObjectsChangedCallback);
		}

		public void SelectedObjectsChanged(int pageIndex, IEnumerable<object> modifiedObjects, PropertyInfo propertyInfo, object[] oldValues, object newValue) {
			if (propertyController != null)
				propertyController.SelectedObjectsChanged(pageIndex, modifiedObjects, propertyInfo, oldValues, newValue);
		}

		#endregion


		#region [Public] Events

		/// <summary>
		/// Raised when TemplateController Initializes
		/// </summary>
		public event EventHandler<TemplateControllerInitializingEventArgs> Initializing;

		/// <summary>
		/// Raised when changes are applied.
		/// </summary>
		public event EventHandler ApplyingChanges;

		/// <summary>
		/// Raised when changes are discarded.
		/// </summary>
		public event EventHandler DiscardingChanges;

		/// <summary>
		/// Raised when ever a property of the template itself or a property of the template's shape or model obejct was changed.
		/// Usually, this event is used for signalling the user interface that there was 
		/// </summary>
		public event EventHandler TemplatePropertyChanged;

		/// <summary>
		/// Raised when the template was renamed
		/// </summary>
		public event EventHandler<TemplateControllerStringChangedEventArgs> TemplateNameChanged;

		/// <summary>
		/// Raised when the template title changed
		/// </summary>
		public event EventHandler<TemplateControllerStringChangedEventArgs> TemplateTitleChanged;

		/// <summary>
		/// Raised when the template's description changed
		/// </summary>
		public event EventHandler<TemplateControllerStringChangedEventArgs> TemplateDescriptionChanged;

		/// <summary>
		/// Raised when the template's shape is replaced by another shape.
		/// </summary>
		public event EventHandler<TemplateControllerTemplateShapeReplacedEventArgs> TemplateShapeReplaced;

		/// <summary>
		/// Raised when the template's ModelObject is replaced by another ModelObject
		/// </summary>
		public event EventHandler<TemplateControllerModelObjectReplacedEventArgs> TemplateModelObjectReplaced;

		/// <summary>
		/// Raised when the property mapping between shape and ModelObject was created or changed
		/// </summary>
		public event EventHandler<TemplateControllerPropertyMappingChangedEventArgs> TemplateShapePropertyMappingSet;

		/// <summary>
		/// Raised when the property mapping between shape and ModelObject was deleted
		/// </summary>
		public event EventHandler<TemplateControllerPropertyMappingChangedEventArgs> TemplateShapePropertyMappingDeleted;

		/// <summary>
		/// Raised when ConnectionPoints were enabled/disabled or mapped to other Terminals of the underlying ModelObject
		/// </summary>
		public event EventHandler<TemplateControllerPointMappingChangedEventArgs> TemplateShapeControlPointMappingChanged;

		#endregion


		#region [Public] Properties

		/// <summary>
		/// The TemplateController's Project.
		/// </summary>
		public Project Project {
			get { return project; }
			set {
				if (project != null && propertyController != null)
					propertyController.Project = null;
				project = value;
				if (project != null) {
					// Set with proerty in order to (un)register events
					if (propertyController == null) PropertyController = new PropertyController(project);
					Initialize(project, OriginalTemplate);
				}
			}
		}


		/// <summary>
		/// Specified wether the TemplateController edits an existing or creates a new template.
		/// </summary>
		public TemplateControllerEditMode EditMode {
			get { return editMode; }
		}


		/// <summary>
		/// A list of all shapes available.
		/// </summary>
		public IReadOnlyCollection<Shape> Shapes {
			get { return shapes; }
		}


		/// <summary>
		/// A list of all model objects available.
		/// </summary>
		public IReadOnlyCollection<IModelObject> ModelObjects {
			get { return modelObjects; }
		}


		/// <summary>
		/// A clone of the original template. This template will be modified. 
		/// When applying the changes, it will be copied into the original template property-by-property .
		/// </summary>
		public Template WorkTemplate { get { return workTemplate; } }


		/// <summary>
		/// The original template. Remains unchanged until applying changes.
		/// </summary>
		public Template OriginalTemplate { get { return originalTemplate; } }


		/// <summary>
		/// Specifies wether the TemplateController is isInitialized completly
		/// </summary>
		public bool IsInitialized { get { return isInitialized; } }

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Calling this method initializes the TemplateController.
		/// </summary>
		public void Initialize(Template template) {
			if (project == null) throw new ArgumentNullException("Property 'Project' is not set.");
			Initialize(Project, template);
		}


		/// <summary>
		/// Calling this method initializes the TemplateController.
		/// </summary>
		public void Initialize(Project project, Template template) {
			if (project == null) throw new ArgumentNullException("project");
			if (this.project != project) Project = project;

			bool templateSupportingShapeTypeFound = false;
			foreach (ShapeType shapeType in project.ShapeTypes) {
				if (shapeType.SupportsAutoTemplates) {
					templateSupportingShapeTypeFound = true;
					break;
				}
			}
			if (!templateSupportingShapeTypeFound) throw new DiagrammingException("No template supporting shape types found. Load a shape library first.");

			// Disable all controls if the user has not the appropriate access rights
			if (!project.SecurityManager.IsGranted(Permission.Templates)) {
				// ToDo: implement access right restrictions
			}
			
			// Create a copy of the template
			if (template != null) {
				editMode = TemplateControllerEditMode.EditTemplate;
				originalTemplate = template;
				workTemplate = template.Clone();
				workTemplate.Shape.DisplayService = this;
			} else {
				// Create a new Template
				editMode = TemplateControllerEditMode.CreateTemplate;
				originalTemplate = null;

				// As a shape is mandatory for every template, find a shape first
				Shape shape = FindFirstShapeOfType(true);
				if (shape == null) shape = FindFirstShapeOfType(false); // if no planar shape was found, get the first one
				workTemplate = new Template("", shape);
				shape.DisplayService = this;
			}

			InitShapeList();
			InitModelObjectList();
			isInitialized = true;

			if (Initializing != null) {
				TemplateControllerInitializingEventArgs eventArgs = new TemplateControllerInitializingEventArgs(editMode, template);
				Initializing(this, eventArgs);
			}
		}


		/// <summary>
		/// Rename the current template.
		/// </summary>
		/// <param name="name"></param>
		public void SetTemplateName(string name) {
			if (workTemplate.Name != name) {
				string oldName = workTemplate.Name;
				workTemplate.Name = name;
				TemplateWasChanged = true;

				if (TemplateNameChanged != null) {
					stringChangedEventArgs.OldString = oldName;
					stringChangedEventArgs.NewString = name;
					TemplateNameChanged(this, stringChangedEventArgs);
				}
			}
		}


		/// <summary>
		/// Change the current template's title.
		/// </summary>
		public void SetTemplateTitle(string title) {
			if (workTemplate.Title != title) {
				string oldTitle = workTemplate.Title;
				workTemplate.Title = title;
				TemplateWasChanged = true;

				if (TemplateTitleChanged != null) {
					stringChangedEventArgs.OldString = oldTitle;
					stringChangedEventArgs.NewString = title;
					TemplateTitleChanged(this, stringChangedEventArgs);
				}
			}
		}


		/// <summary>
		/// Change the current template's description.
		/// </summary>
		/// <param name="projectName"></param>
		public void SetTemplateDescription(string description) {
			if (workTemplate.Description != description) {
				string oldDescription = workTemplate.Name;
				workTemplate.Description = description;
				TemplateWasChanged = true;

				if (TemplateDescriptionChanged != null) {
					stringChangedEventArgs.OldString = oldDescription;
					stringChangedEventArgs.NewString = description;
					TemplateDescriptionChanged(this, stringChangedEventArgs);
				}
			}
		}
		

		/// <summary>
		/// Set the given shape as the template's shape.
		/// </summary>
		public void SetTemplateShape(Shape newShape) {
			if (newShape == null) throw new ArgumentNullException("newShape");
			// buffer the current template shape
			Shape oldShape = workTemplate.Shape;
			if (oldShape != null)
				oldShape.Invalidate();
			
			// set the new template shape
			newShape.DisplayService = this;
			newShape.Invalidate();
			workTemplate.Shape = newShape;

			TemplateWasChanged = true;
			if (TemplateShapeReplaced != null) {
				shapeReplacedEventArgs.Template = workTemplate;
				shapeReplacedEventArgs.OldTemplateShape = oldShape;
				shapeReplacedEventArgs.NewTemplateShape = newShape;
				TemplateShapeReplaced(this, shapeReplacedEventArgs);
			}
			// edit new shape in the PropertyEditor
			propertyController.SetObject(0, newShape, SelectedObjectsChangedCallback);
		}


		/// <summary>
		/// Set the given Modelobject as the template's ModelObject
		/// </summary>
		public void SetTemplateModel(IModelObject newModelObject) {
			if (workTemplate.Shape == null) throw new DiagrammingException("The template's shape property is not set to a reference of an object.");
			IModelObject oldModelObject = workTemplate.Shape.ModelObject;
			if (oldModelObject != null) {
				// ToDo: Implement ModelObject.CopyFrom()
				//newModelObject.CopyFrom(oldModelObject);
			}
			workTemplate.UnmapAllTerminals();
			workTemplate.Shape.ModelObject = newModelObject;
			TemplateWasChanged = true;

			if (TemplateModelObjectReplaced != null) {
				modelObjectReplacedEventArgs.Template = workTemplate;
				modelObjectReplacedEventArgs.OldModelObject = oldModelObject;
				modelObjectReplacedEventArgs.NewModelObject = newModelObject;
				TemplateModelObjectReplaced(this, modelObjectReplacedEventArgs);
			}
			// edit newModelObject in the PropertyEditor
			if (newModelObject != null) {
				propertyController.SetObjects(0, newModelObject.Shapes, SelectedObjectsChangedCallback);
				propertyController.SetObject(1, newModelObject, SelectedObjectsChangedCallback);
			} else propertyController.SetObject(0, workTemplate.Shape, SelectedObjectsChangedCallback);

			if (TemplateModelObjectReplaced != null)
				TemplateModelObjectReplaced(this, null);
		}


		/// <summary>
		/// Not yet implemented
		/// </summary>
		public void SetModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			workTemplate.MapProperties(modelMapping);
			TemplateWasChanged = true;
			if (TemplateShapePropertyMappingSet != null) {
				modelMappingChangedEventArgs.Template = workTemplate;
				modelMappingChangedEventArgs.ModelMapping = modelMapping;
				TemplateShapePropertyMappingSet(this, modelMappingChangedEventArgs);
			}
		}


		/// <summary>
		/// Deletes a ModelMapping
		/// </summary>
		/// <param name="modelMapping"></param>
		public void DeleteModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			workTemplate.UnmapProperties(modelMapping);
			TemplateWasChanged = true;
			if (TemplateShapePropertyMappingDeleted != null) {
				modelMappingChangedEventArgs.Template = workTemplate;
				modelMappingChangedEventArgs.ModelMapping = modelMapping;
				TemplateShapePropertyMappingDeleted(this, modelMappingChangedEventArgs);
			}
		}


		/// <summary>
		/// If the template has no Modelobject, this method enables/disables ConnectionPoints of the shape.
		/// If the template has a ModelObject, this method assigns a ModelObject terminal to a ConnectionPoint of the shape
		/// </summary>
		/// <param name="connectionPointId">Id of the shape's ControlPoint</param>
		/// <param name="terminalId">Id of the Modelobject's Terminal. Pass -1 in order to clear the mapping.</param>
		public void SetTerminalConnectionPointMapping(ControlPointId controlPointId, TerminalId terminalId) {
			TerminalId oldTerminalId = workTemplate.GetMappedTerminalId(controlPointId);
			workTemplate.MapTerminal(terminalId, controlPointId);
			TemplateWasChanged = true;

			if (TemplateShapeControlPointMappingChanged != null) {
				controlPointMappingChangedEventArgs.ControlPointId = controlPointId;
				controlPointMappingChangedEventArgs.OldTerminalId = oldTerminalId;
				controlPointMappingChangedEventArgs.NewTerminalId = terminalId;
				TemplateShapeControlPointMappingChanged(this, controlPointMappingChangedEventArgs);
			}
		}


		/// <summary>
		/// Handle changes on objects edited by the PropertyController
		/// </summary>
		/// <param name="propertyChangedEventArgs"></param>
		public void SelectedObjectsChangedCallback(PropertyControllerPropertyChangedEventArgs propertyChangedEventArgs) {
			TemplateWasChanged = true;
		}


		/// <summary>
		/// Applies all changes made on the working template to the original template.
		/// </summary>
		public void ApplyChanges() {
			if (TemplateWasChanged) {
				ICommand cmd = null;
				switch (editMode) {
					case TemplateControllerEditMode.CreateTemplate:
						cmd = new CreateTemplateCommand(workTemplate);
						project.ExecuteCommand(cmd);
						// after inserting the template into the cache, the template becomes the new 
						// originalTemplate and a new workTemplate has to be cloned.
						// TemplateControllerEditMode is changed from Create to Edit so the user can continue editing the 
						// template until the template editor is closed
						originalTemplate = workTemplate;
						// ToDo: Set appropriate DisplayService
						originalTemplate.Shape.DisplayService = null;
						workTemplate = originalTemplate.Clone();
						editMode = TemplateControllerEditMode.EditTemplate;
						break;

					case TemplateControllerEditMode.EditTemplate:
						// set workTemplate.Shape's DisplayService to the original shape's DisplayService 
						// (typically the ToolSetController)
						workTemplate.Shape.DisplayService = originalTemplate.Shape.DisplayService;
						if (workTemplate.Shape.Type != originalTemplate.Shape.Type)
							cmd = new ExchangeTemplateShapeCommand(originalTemplate, workTemplate);
						else
							cmd = new ExchangeTemplateCommand(originalTemplate, workTemplate);
						project.ExecuteCommand(cmd);
						break;

					default: throw new DiagrammingUnsupportedValueException(typeof(TemplateControllerEditMode), editMode);
				}
				TemplateWasChanged = false;
				if (ApplyingChanges != null) ApplyingChanges(this, eventArgs);
			}
		}


		/// <summary>
		/// Discards all changes made to the working copy of the original template.
		/// </summary>
		public void DiscardChanges() {
			if (EditMode == TemplateControllerEditMode.CreateTemplate)
				Initialize(project, null);
			else
				Initialize(project, originalTemplate);
			if (DiscardingChanges != null) DiscardingChanges(this, eventArgs);
		}


		/// <summary>
		/// Clears all buffers and objects used by the TemplateController
		/// </summary>
		public void Clear() {
			if (propertyController != null)
				propertyController.SetObject(0, null, SelectedObjectsChangedCallback);

			ClearShapeList();
			ClearModelObjectList();

			workTemplate = null;
			originalTemplate = null;
		}

		#endregion


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				Clear();

				UnregisterPropertyControllerEvents();
				propertyController = null;

				infoGraphics.Dispose();
			}
			base.Dispose(disposing);
		}


		#region [Private] Properties

		private bool TemplateWasChanged {
			get { return templateWasChanged; }
			set {
				if (project.SecurityManager.IsGranted(Permission.Templates)) {
					templateWasChanged = value;
					if (TemplatePropertyChanged != null) TemplatePropertyChanged(this, eventArgs);
				}
			}
		}


		private PropertyController PropertyController {
			get { return propertyController; }
			set {
				if (propertyController != null) UnregisterPropertyControllerEvents();
				propertyController = value;
				if (propertyController != null) RegisterPropertyControllerEvents();
			}
		}

		#endregion


		#region [Private] Methods

		private void ClearShapeList() {
			//foreach (Shape shape in shapes)
			//   shape.Dispose();
			shapes.Clear();
		}


		private void ClearModelObjectList() {
			//foreach (IModelObject modelObject in modelObjects)
			//   modelObject.Dispose();
			modelObjects.Clear();
		}


		private void InitShapeList() {
			ClearShapeList();
			foreach (ShapeType shapeType in project.ShapeTypes) {
				if (!shapeType.SupportsAutoTemplates) continue;
				Shape shape = shapeType.CreateInstance();
				shape.DisplayService = this;
				shapes.Add(shape);
			}
		}


		private void InitModelObjectList() {
			ClearModelObjectList();
			foreach (ModelObjectType modelObjectType in project.ModelObjectTypes) {
				IModelObject modelObject = modelObjectType.CreateInstance();
				modelObjects.Add(modelObject);
			}
		}


		private Shape FindFirstShapeOfType(bool findPlanarShape) {
			foreach (ShapeType shapeType in project.ShapeTypes) {
				if (!shapeType.SupportsAutoTemplates) continue;
				Shape shape = shapeType.CreateInstance();
				if (findPlanarShape) {
					if (shape is IPlanarShape) return shape;
				} else return shape;
			}
			return null;
		}

		#endregion


		#region [Private] Methods: Register for events

		private void RegisterPropertyControllerEvents() {
			if (propertyController != null) {
				propertyController.ObjectsSet += propertyController_ObjectsSet;
				propertyController.PropertyChanged += propertyController_PropertyChanged;
				propertyController.RefreshObjects += propertyController_RefreshObjects;
				propertyController.ObjectsDeleted += propertyController_ObjectsDeleted;
				propertyController.ProjectClosing += propertyController_ProjectClosing;
			}
		}


		private void UnregisterPropertyControllerEvents() {
			if (propertyController != null) {
				propertyController.ObjectsSet -= propertyController_ObjectsSet;
				propertyController.PropertyChanged -= propertyController_PropertyChanged;
				propertyController.RefreshObjects -= propertyController_RefreshObjects;
				propertyController.ObjectsDeleted -= propertyController_ObjectsDeleted;
				propertyController.ProjectClosing -= propertyController_ProjectClosing;
			}
		}

		#endregion


		#region [Private] Methods: PropertyController Event Handler implementations

		private void propertyController_RefreshObjects(object sender, PropertyControllerEventArgs e) {
			if (RefreshObjects != null) RefreshObjects(this, e);
		}


		private void propertyController_PropertyChanged(object sender, PropertyControllerPropertyChangedEventArgs e) {
			if (PropertyChanged != null) PropertyChanged(this, e);
		}


		private void propertyController_ObjectsSet(object sender, PropertyControllerEventArgs e) {
			if (ObjectsSet != null) ObjectsSet(this, e);
		}


		private void propertyController_ProjectClosing(object sender, EventArgs e) {
			if (ProjectClosing != null) ProjectClosing(this, e);
		}


		private void propertyController_ObjectsDeleted(object sender, PropertyControllerEventArgs e) {
			if (ObjectsDeleted != null) ObjectsDeleted(this, e);
		}

		#endregion


		#region Fields
		// IDisplayService fields
		private Graphics infoGraphics;
		// TemplateController fields
		private Project project;
		private PropertyController propertyController;
		private TemplateControllerEditMode editMode;
		private Template originalTemplate;
		private Template workTemplate;
		private ReadOnlyList<Shape> shapes = new ReadOnlyList<Shape>();
		private ReadOnlyList<IModelObject> modelObjects = new ReadOnlyList<IModelObject>();
		private bool templateWasChanged = false;
		private bool isInitialized = false;
		// EventArgs buffers
		private EventArgs eventArgs = new EventArgs();
		private TemplateControllerStringChangedEventArgs stringChangedEventArgs 
			= new TemplateControllerStringChangedEventArgs(string.Empty, string.Empty);
		private TemplateControllerTemplateShapeReplacedEventArgs shapeReplacedEventArgs 
			= new TemplateControllerTemplateShapeReplacedEventArgs(null, null, null);
		private TemplateControllerModelObjectReplacedEventArgs modelObjectReplacedEventArgs 
			= new TemplateControllerModelObjectReplacedEventArgs(null, null, null);
		private TemplateControllerPointMappingChangedEventArgs controlPointMappingChangedEventArgs 
			= new TemplateControllerPointMappingChangedEventArgs(null, ControlPointId.None, TerminalId.Invalid, TerminalId.Invalid);
		private TemplateControllerPropertyMappingChangedEventArgs modelMappingChangedEventArgs
			= new TemplateControllerPropertyMappingChangedEventArgs(null, null);

		#endregion
	}
}