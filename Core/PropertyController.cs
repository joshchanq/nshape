using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

using Dataweb.nShape.Advanced;
using System.Collections;
using System.Runtime.CompilerServices;


namespace Dataweb.nShape.Controllers {

	public interface IPropertyController {
		
		#region Events

		event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		event EventHandler<PropertyControllerEventArgs> RefreshObjects;

		event EventHandler<PropertyControllerEventArgs> ObjectsDeleted;

		event EventHandler ProjectClosing;

		#endregion


		#region Properties

		Project Project { get; }

		bool ReadOnly { get ; }

		#endregion


		#region Methods

		///// <summary>
		///// Sets the given object as the SelectedObject of the attached property grid. 
		///// When the propertygrid changes properties, the given callback will be called which is responsible for creating, executing and adding commands to the history.
		///// </summary>
		//void SetObject(int pageIndex, object selectedObject);

		///// <summary>
		///// Sets the given object as the SelectedObject of the attached property grid. 
		///// When the propertygrid changes properties, the given callback will be called which is responsible for creating, executing and adding commands to the history.
		///// </summary>
		//void SetObjects(int pageIndex, IEnumerable selectedObjects);

		/// <summary>
		/// Creates and executes all necessary commands to perform the changes against the repository.
		/// </summary>
		/// <param name="modifiedObjects">The modified objects</param>
		/// <param name="propertyInfo">PropertyInfo of the changed property</param>
		/// <param name="oldValues">The modifiedObject's original property value. This can be one common value or a value for each modifiedObject</param>
		/// <param name="newValue">The new value of the modified property</param>
		void SelectedObjectsChanged(int pageIndex, IEnumerable<object> modifiedObjects, PropertyInfo propertyInfo, object[] oldValues, object newValue);

		#endregion

	}


	#region EventArgs

	public class PropertyControllerEventArgs : EventArgs {

		public PropertyControllerEventArgs(int pageIndex, IEnumerable<object> objects)
			: this() {
			if (objects == null) throw new ArgumentNullException("objects");
			SetObjects(objects);
			this.pageIndex = pageIndex;
		}


		public IReadOnlyCollection<object> Objects {
			get { return objects; }
		}


		public object[] GetObjectArray() {
			return objects.ToArray();
		}


		public int PageIndex {
			get { return pageIndex; }
			internal set { pageIndex = value; }
		}


		/// <summary>
		/// Returns the common Type of all objects in the objects collection.
		/// </summary>
		public Type ObjectsType {
			get { return commonType ?? typeof(object); }
		}


		protected internal PropertyControllerEventArgs() { }


		protected internal void SetObjects(IEnumerable objects) {
			this.objects.Clear();
			foreach (object obj in objects)
				this.objects.Add(obj);
			SetCommonType();
		}


		protected internal void SetObjects(IEnumerable<Shape> objects) {
			this.objects.Clear();
			foreach (Shape s in objects)
				this.objects.Add(s);
			commonType = typeof(Shape);
		}


		protected internal void SetObjects(IEnumerable<IModelObject> objects) {
			this.objects.Clear();
			foreach (IModelObject m in objects)
				this.objects.Add(m);
			commonType = typeof(IModelObject);
		}


		protected internal void SetObject(object obj) {
			this.objects.Clear();
			if (obj != null) this.objects.Add(obj);
			SetCommonType();
		}


		private void SetCommonType() {
			// get Type of modifiedObjects
			commonType = null;
			for (int i = objects.Count - 1; i >= 0; --i) {
				if (objects[i] == null) continue;
				if (commonType == null) {
					if (objects[i] is Shape) commonType = typeof(Shape);
					else if (objects[i] is IModelObject) commonType = typeof(IModelObject);
					else commonType = objects[i].GetType();
				} else if (!objects[i].GetType().IsSubclassOf(commonType)
							&& objects[i].GetType().GetInterface(commonType.Name) == null) {
					commonType = null;
					break;
				}
			}
		}


		private ReadOnlyList<object> objects = new ReadOnlyList<object>();
		private int pageIndex = -1;
		private Type commonType = null;
	}
	

	public class PropertyControllerPropertyChangedEventArgs : PropertyControllerEventArgs {

		public PropertyControllerPropertyChangedEventArgs(int pageIndex, IEnumerable<object> modifiedObjects, PropertyInfo propertyInfo, object[] oldValues, object newValue)
			: base(pageIndex, modifiedObjects) {
			if (modifiedObjects == null) throw new ArgumentNullException("modifiedObjects");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			// store modifiedObjects
			this.oldValues = new object[oldValues.Length];
			Array.Copy(oldValues, this.oldValues, oldValues.Length);
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}

		public object[] OldValues { get { return oldValues; } }

		public object NewValue { get { return newValue; } }

		public PropertyInfo PropertyInfo { get { return propertyInfo; } }

		private object[] oldValues;
		private object newValue;
		private PropertyInfo propertyInfo;
	}

	#endregion


	internal delegate void SelectedObjectsChangedCallback(PropertyControllerPropertyChangedEventArgs propertyChangedEventArgs);
	
	
	internal class PropertyController : Component {

		public PropertyController(Project project) {
			if (project == null) throw new ArgumentNullException("project");
			Project = project;
		}


		~PropertyController() {
			Project = null;
		}


		#region [Public] Events

		public event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		public event EventHandler<PropertyControllerEventArgs> ObjectsDeleted;

		public event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		public event EventHandler<PropertyControllerEventArgs> RefreshObjects;

		public event EventHandler<EventArgs> ProjectClosing;

		#endregion


		public bool ReadOnly { get { return readOnly; } }


		#region [Public] Methods

		/// <summary>
		/// Sets the given object as the SelectedObject of the attached property grid. 
		/// When the propertygrid changes properties, the given callback will be called which is responsible for creating, executing and adding commands to the history.
		/// </summary>
		public void SetObject(int pageIndex, object selectedObject, SelectedObjectsChangedCallback objectChangedCallback) {
			if (objectChangedCallback == null) throw new ArgumentNullException("selectedObjectChangedCallback");
			selectedObjectsChangedCallback = objectChangedCallback;
			if (ObjectsSet != null) {
				propertyControllerEventArgs.PageIndex = pageIndex;
				propertyControllerEventArgs.SetObject(selectedObject);
				ObjectsSet(this, propertyControllerEventArgs);
			}
		}


		/// <summary>
		/// Sets the given object as the SelectedObject of the attached property grid. 
		/// When the propertygrid changes properties, the given callback will be called which is responsible for creating, executing and adding commands to the history.
		/// </summary>
		public void SetObjects(int pageIndex, IEnumerable selectedObjects, SelectedObjectsChangedCallback objectsChangedCallback) {
			if (objectsChangedCallback == null) throw new ArgumentNullException("selectedObjectChangedCallback");
			selectedObjectsChangedCallback = objectsChangedCallback;
			// ToDo: Get permissions needed for modifying the properties and set properties as readonly if needed permissions are not granted.
			readOnly = false;
			if (ObjectsSet != null) {
				propertyControllerEventArgs.PageIndex = pageIndex;
				propertyControllerEventArgs.SetObjects(selectedObjects);
				ObjectsSet(this, propertyControllerEventArgs);
			}
		}


		/// <summary>
		/// Calls the SelectedObjectsChangedCallback which creates and executes a command that will be added to the project's history.
		/// </summary>
		/// <param name="modifiedObjects">The modified objects</param>
		/// <param name="propertyInfo">PropertyInfo of the changed property</param>
		/// <param name="oldValues">The modifiedObject's original property value. This can be one common value or a value for each modifiedObject</param>
		/// <param name="newValue">The new value of the modified property</param>
		public void SelectedObjectsChanged(int pageIndex, IEnumerable<object> modifiedObjects, PropertyInfo propertyInfo, object[] oldValues, object newValue) {
			PropertyControllerPropertyChangedEventArgs propertyChangedEventArgs = new PropertyControllerPropertyChangedEventArgs(pageIndex, modifiedObjects, propertyInfo, oldValues, newValue);
			if (selectedObjectsChangedCallback != null) 
				selectedObjectsChangedCallback(propertyChangedEventArgs);
			if (PropertyChanged != null) PropertyChanged(this, propertyChangedEventArgs);
		}

		#endregion


		internal Project Project {
			get { return project; }
			set {
				if (project != null) {
					if (project.Repository != null) UnregisterRepositoryEvents();
					UnregisterProjectEvents();
				}
				project = value;
				if (project != null) {
					RegisterProjectEvents();
					if (project.IsOpen) RegisterRepositoryEvents();
				}
			}
		}


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing)
				UnregisterRepositoryEvents();
			base.Dispose(disposing);
		}


		#region [Private] Methods: Registering for events

		private void RegisterProjectEvents() {
			if (project != null) {
				project.Opened += project_ProjectOpen;
				project.Closing += project_ProjectClosing;
				project.Closed += project_ProjectClosed;
			}
		}


		private void RegisterRepositoryEvents() {
			if (project != null && project.Repository != null) {
				project.Repository.StyleUpdated += repository_StyleUpdated;
				project.Repository.DiagramUpdated += repository_DiagramUpdated;
				project.Repository.ShapesUpdated += repository_ShapesUpdated;
				project.Repository.ModelObjectsUpdated += repository_ModelObjectsUpdated;

				project.Repository.StyleDeleted += Repository_StyleDeleted;
				project.Repository.DiagramDeleted += Repository_DiagramDeleted;
				project.Repository.ShapesDeleted += Repository_ShapesDeleted;
				project.Repository.ModelObjectsDeleted += Repository_ModelObjectsDeleted;
			}
		}


		private void UnregisterProjectEvents() {
			if (project != null) {
				project.Opened -= project_ProjectOpen;
				project.Closing -= project_ProjectClosing;
				project.Closed -= project_ProjectClosed;
			}
		}
		
		
		private void UnregisterRepositoryEvents() {
			if (project != null && project.Repository != null) {
				project.Repository.StyleUpdated -= repository_StyleUpdated;
				project.Repository.DiagramUpdated -= repository_DiagramUpdated;
				project.Repository.ShapesUpdated -= repository_ShapesUpdated;
				project.Repository.ModelObjectsUpdated -= repository_ModelObjectsUpdated;

				project.Repository.StyleDeleted -= Repository_StyleDeleted;
				project.Repository.DiagramDeleted -= Repository_DiagramDeleted;
				project.Repository.ShapesDeleted -= Repository_ShapesDeleted;
				project.Repository.ModelObjectsDeleted -= Repository_ModelObjectsDeleted;
			}
		}

		#endregion


		#region [Private] Methods: Event Handler implementation

		private void project_ProjectOpen(object sender, EventArgs e) {
			RegisterRepositoryEvents();
		}


		private void project_ProjectClosing(object sender, EventArgs e) {
			if (ProjectClosing != null) ProjectClosing(this, new EventArgs());
		}


		private void project_ProjectClosed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
		}


		private void repository_StyleUpdated(object sender, RepositoryStyleEventArgs e) {
			if (RefreshObjects != null) {
				propertyControllerEventArgs.PageIndex = 0;
				RefreshObjects(this, propertyControllerEventArgs);
			}
		}


		private void repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			if (RefreshObjects != null) {
				if (selectedObjectsBuffer.Count == 1 && selectedObjectsBuffer[0] is Diagram) {
					propertyControllerEventArgs.PageIndex = 0;
					RefreshObjects(this, propertyControllerEventArgs);
				}
			}
		}


		private void repository_ModelObjectsUpdated(object sender, RepositoryModelObjectsEventArgs e) {
			if (RefreshObjects != null) {
				propertyControllerEventArgs.PageIndex = 1;
				RefreshObjects(this, propertyControllerEventArgs);
			}
		}


		private void repository_ShapesUpdated(object sender, RepositoryShapesEventArgs e) {
			if (RefreshObjects != null) {
				propertyControllerEventArgs.PageIndex = 0;
				RefreshObjects(this, propertyControllerEventArgs);
			}
		}


		private void Repository_ModelObjectsDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			if (ObjectsDeleted !=null){
				propertyControllerEventArgs.PageIndex = 1;
				propertyControllerEventArgs.SetObjects(e.ModelObjects);
				ObjectsDeleted(this, propertyControllerEventArgs);
			}
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			if (ObjectsDeleted != null) {
				propertyControllerEventArgs.PageIndex = 0;
				propertyControllerEventArgs.SetObjects(e.Shapes);
				ObjectsDeleted(this, propertyControllerEventArgs);
			}
		}


		private void Repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			if (ObjectsDeleted != null) {
				propertyControllerEventArgs.PageIndex = 0;
				propertyControllerEventArgs.SetObject(e.Diagram);
				ObjectsDeleted(this, propertyControllerEventArgs);
			}
		}


		private void Repository_StyleDeleted(object sender, RepositoryStyleEventArgs e) {
			if (ObjectsDeleted != null) {
				propertyControllerEventArgs.PageIndex = 0;
				propertyControllerEventArgs.SetObject(e.Style);
				ObjectsDeleted(this, propertyControllerEventArgs);
			}
		}

		#endregion


		#region Fields
		// property fields
		private Project project;
		private bool readOnly;
		// buffers
		private SelectedObjectsChangedCallback selectedObjectsChangedCallback = null;
		private ReadOnlyList<object> selectedObjectsBuffer = new ReadOnlyList<object>();
		private PropertyControllerEventArgs propertyControllerEventArgs = new PropertyControllerEventArgs();
		#endregion
	}
}
