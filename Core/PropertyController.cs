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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

	public interface IPropertyController {
		
		#region Events

		event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		event EventHandler<PropertyControllerEventArgs> ObjectsModified;

		event EventHandler ProjectClosing;

		#endregion


		#region Properties

		Project Project { get; }

		#endregion


		#region Methods

		void SetPropertyValue(object obj, string propertyName, object oldValue, object newValue);

		void CancelSetProperty();

		void CommitSetProperty();

		///// <summary>
		///// Creates and executes all necessary commands to perform the changes against the repository.
		///// </summary>
		///// <param name="modifiedObjects">The modified objects</param>
		///// <param name="propertyInfo">PropertyInfo of the changed property</param>
		///// <param name="oldValues">The modifiedObject's original property value. This can be one common value or a value for each modifiedObject</param>
		///// <param name="newValue">The new value of the modified property</param>
		//void SelectedObjectsChanged(int pageIndex, IEnumerable<object> modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue);

		#endregion

	}


	#region EventArgs

	public class PropertyControllerEventArgs : EventArgs {

		public PropertyControllerEventArgs(int pageIndex, IEnumerable objects) {
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

		public PropertyControllerPropertyChangedEventArgs(int pageIndex, IEnumerable modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(pageIndex, modifiedObjects) {
			if (modifiedObjects == null) throw new ArgumentNullException("modifiedObjects");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			// store modifiedObjects
			this.oldValues = new List<object>(oldValues);
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}

		public IEnumerable<object> OldValues { get { return oldValues; } }

		public object NewValue { get { return newValue; } }

		public PropertyInfo PropertyInfo { get { return propertyInfo; } }

		private List<object> oldValues;
		private object newValue;
		private PropertyInfo propertyInfo;
	}

	#endregion


	public delegate void SelectedObjectsChangedCallback(PropertyControllerPropertyChangedEventArgs propertyChangedEventArgs);


	public class PropertyController : Component, IPropertyController {

		public PropertyController() {
		}
		
		
		public PropertyController(Project project) {
			if (project == null) throw new ArgumentNullException("project");
			Project = project;
		}


		~PropertyController() {
			Project = null;
		}


		#region [Public] IPropertyController Events

		public event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		public event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		public event EventHandler<PropertyControllerEventArgs> ObjectsModified;

		public event EventHandler ProjectClosing;

		#endregion


		#region [Public] IPropertyController Properties

		[Category("NShape")]
		public Project Project {
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

		#endregion


		#region [Public] IPropertyController Methods

		public void SetPropertyValue(object obj, string propertyName, object oldValue, object newValue) {
			AssertProjectExists();
			if (propertySetBuffer != null 
				&& propertySetBuffer.Objects.Count > 0 
				&& !IsOfType(obj.GetType(), propertySetBuffer.ObjectType)) 
				throw new InvalidOperationException("Another transaction is pending.");
			if (propertySetBuffer == null) {
				PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
				if (obj is Shape)
					propertySetBuffer = new PropertySetInfo<Shape>(propertyInfo, newValue);
				else if (obj is IModelObject)
					propertySetBuffer = new PropertySetInfo<IModelObject>(propertyInfo, newValue);
				else if (obj is Diagram)
					propertySetBuffer = new PropertySetInfo<Diagram>(propertyInfo, newValue);
				else if (obj is Layer)
					propertySetBuffer = new PropertySetInfo<Layer>(propertyInfo, newValue);
				else if (obj is Design)
					propertySetBuffer = new PropertySetInfo<Design>(propertyInfo, newValue);
				else if (obj is Style)
					propertySetBuffer = new PropertySetInfo<Style>(propertyInfo, newValue);
				else throw new NotSupportedException();
			}
			propertySetBuffer.Objects.Add(obj);
			propertySetBuffer.OldValues.Add(oldValue);
		}


		public void CancelSetProperty() {
			AssertProjectExists();
			// Discard buffer
			propertySetBuffer = null;
		}


		public void CommitSetProperty() {
			AssertProjectExists();
			if (propertySetBuffer != null){
				ICommand command = null;
				if (propertySetBuffer.Objects.Count != 0) {
					if (IsOfType(propertySetBuffer.ObjectType, typeof(Shape))) {
						command = new ShapePropertySetCommand(
							ConvertEnumerator<Shape>.Create(propertySetBuffer.Objects),
							propertySetBuffer.PropertyInfo,
							propertySetBuffer.OldValues,
							propertySetBuffer.NewValue);
					} else if (IsOfType(propertySetBuffer.ObjectType, typeof(IModelObject))) {
						command = new ModelObjectPropertySetCommand(
							ConvertEnumerator<IModelObject>.Create(propertySetBuffer.Objects),
							propertySetBuffer.PropertyInfo,
							propertySetBuffer.OldValues,
							propertySetBuffer.NewValue);
					} else if (IsOfType(propertySetBuffer.ObjectType, typeof(Diagram))) {
						command = new DiagramPropertySetCommand(
							ConvertEnumerator<Diagram>.Create(propertySetBuffer.Objects),
							propertySetBuffer.PropertyInfo,
							propertySetBuffer.OldValues,
							propertySetBuffer.NewValue);
					} else if (IsOfType(propertySetBuffer.ObjectType, typeof(Layer))) {
						Diagram diagram = null;
						foreach (Diagram d in project.Repository.GetDiagrams()) {
							foreach (Layer l in ConvertEnumerator<Layer>.Create(propertySetBuffer.Objects)) {
								if (d.Layers.Contains(l)) {
									diagram = d;
									break;
								}
								if (diagram != null) break;
							}
						}
						command = new LayerPropertySetCommand(
							diagram,
							ConvertEnumerator<Layer>.Create(propertySetBuffer.Objects),
							propertySetBuffer.PropertyInfo,
							propertySetBuffer.OldValues,
							propertySetBuffer.NewValue);
					} else if (IsOfType(propertySetBuffer.ObjectType, typeof(Design))) {
						command = new DesignPropertySetCommand(
							ConvertEnumerator<Design>.Create(propertySetBuffer.Objects),
							propertySetBuffer.PropertyInfo,
							propertySetBuffer.OldValues,
							propertySetBuffer.NewValue);
					} else if (IsOfType(propertySetBuffer.ObjectType, typeof(Style))) {
						Design design = null;
						if (project.Repository != null) {
							foreach (Design d in project.Repository.GetDesigns()) {
								foreach (Style s in ConvertEnumerator<Style>.Create(propertySetBuffer.Objects)) {
									if (d.ContainsStyle(s)) {
										design = d;
										break;
									}
								}
								if (design != null) break;
							}
						} else design = project.Design;
						command = new StylePropertySetCommand(
							design,
							ConvertEnumerator<Style>.Create(propertySetBuffer.Objects),
							propertySetBuffer.PropertyInfo,
							propertySetBuffer.OldValues,
							propertySetBuffer.NewValue);
					} else throw new NotSupportedException();
				}

				if (command != null) {
					if (!command.IsAllowed(Project.SecurityManager)) throw new NShapeSecurityException(command);
					if (updateRepository) Project.ExecuteCommand(command);
					else command.Execute();
					if (PropertyChanged != null) {
						int pageIndex;
						Hashtable selectedObjectsList;
						GetSelectedObjectList(propertySetBuffer.Objects, out pageIndex, out selectedObjectsList);
						if (pageIndex >= 0) {
							PropertyControllerPropertyChangedEventArgs e = new PropertyControllerPropertyChangedEventArgs(
								pageIndex,
								propertySetBuffer.Objects,
								propertySetBuffer.PropertyInfo,
								propertySetBuffer.OldValues,
								propertySetBuffer.NewValue);
							PropertyChanged(this, e);
						}
					}
				}
			}
			propertySetBuffer = null;
		}

		#endregion


		#region [Public] Methods

		public void SetObject(int pageIndex, object selectedObject) {
			AssertProjectExists();
			SetObject(pageIndex, selectedObject, true);
		}


		public void SetObject(int pageIndex, object selectedObject, bool isPersistent) {
			AssertProjectExists();

			Hashtable selectedObjectsList;
			GetSelectedObjectList(pageIndex, out selectedObjectsList);
			selectedObjectsList.Clear();
			updateRepository = isPersistent;
			if (selectedObject != null) selectedObjectsList.Add(selectedObject, null);

			if (ObjectsSet != null) {
				propertyControllerEventArgs.PageIndex = pageIndex;
				propertyControllerEventArgs.SetObject(selectedObject);
				ObjectsSet(this, propertyControllerEventArgs);
			}
		}


		public void SetObjects(int pageIndex, IEnumerable selectedObjects) {
			AssertProjectExists();
			SetObjects(pageIndex, selectedObjects, true);
		}


		public void SetObjects(int pageIndex, IEnumerable selectedObjects, bool arePersistent) {
			if (selectedObjects == null) throw new ArgumentNullException("selectedObjects");
			AssertProjectExists();

			Hashtable selectedObjectsList;
			GetSelectedObjectList(pageIndex, out selectedObjectsList);
			selectedObjectsList.Clear();

			updateRepository = arePersistent;
			foreach (object o in selectedObjects)
				selectedObjectsList.Add(o, null);

			if (ObjectsSet != null) {
				propertyControllerEventArgs.PageIndex = pageIndex;
				propertyControllerEventArgs.SetObjects(selectedObjects);
				ObjectsSet(this, propertyControllerEventArgs);
			}
		}


		public object GetSelectedObject(int pageIndex) {
			object result = null;
			Hashtable selectedObjects;
			GetSelectedObjectList(pageIndex, out selectedObjects);
			if (selectedObjects.Count > 0) {
				foreach (DictionaryEntry item in selectedObjects) {
					result = item.Key;
					break;
				}
			}
			return result;
		}


		public IEnumerable<object> GetSelectedObjects(int pageIndex) {
			Hashtable selectedObjects;
			GetSelectedObjectList(pageIndex, out selectedObjects);
			foreach (DictionaryEntry item in selectedObjects)
				yield return item.Key;
		}


		public int GetSelectedObjectsCount(int pageIndex) {
			Hashtable selectedObjects;
			GetSelectedObjectList(pageIndex, out selectedObjects);
			return selectedObjects.Count;
		}

		#endregion


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
			OnObjectModified(e.Style);
		}


		private void repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			OnObjectModified(e.Diagram);
		}


		private void repository_ModelObjectsUpdated(object sender, RepositoryModelObjectsEventArgs e) {
			OnObjectsModified(e.ModelObjects);
		}


		private void repository_ShapesUpdated(object sender, RepositoryShapesEventArgs e) {
			OnObjectsModified(e.Shapes);
		}


		private void Repository_ModelObjectsDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			OnObjectsDeleted(e.ModelObjects);
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			OnObjectsDeleted(e.Shapes);
		}


		private void Repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			OnObjectDeleted(e.Diagram);
		}


		private void Repository_StyleDeleted(object sender, RepositoryStyleEventArgs e) {
			OnObjectDeleted(e.Style);
		}

		#endregion


		private void AssertProjectExists() {
			if (Project == null) throw new InvalidOperationException("Property Project is not set.");
		}
		
		
		private bool IsOfType(Type sourceType, Type targetType) {
			return (sourceType == targetType
				|| sourceType.IsSubclassOf(targetType)
				|| sourceType.GetInterface(targetType.Name, true) != null);
		}


		private void GetSelectedObjectList(int pageIndex, out Hashtable selectedObjectsList) {
			selectedObjectsList = null;
			switch (pageIndex) {
				case 0: selectedObjectsList = selectedPrimaryObjects; break;
				case 1: selectedObjectsList = selectedSecondaryObjects; break;
				default: throw new IndexOutOfRangeException();
			}
		}


		private void GetSelectedObjectList(IEnumerable objs, out int pageIndex, out Hashtable selectedObjectsList) {
			pageIndex = -1;
			selectedObjectsList = null;
			foreach (object obj in objs) {
				if (selectedPrimaryObjects.ContainsKey(obj)) {
					pageIndex = 0;
					selectedObjectsList = selectedPrimaryObjects;
				} else if (selectedSecondaryObjects.ContainsKey(obj)) {
					pageIndex = 1;
					selectedObjectsList = selectedSecondaryObjects;
				}
				if (selectedObjectsList != null) break;
			}
		}


		private void GetSelectedObjectList(object obj, out int pageIndex, out Hashtable selectedObjectsList) {
			pageIndex = -1;
			selectedObjectsList = null;
			if (selectedPrimaryObjects.ContainsKey(obj)) {
				pageIndex = 0;
				selectedObjectsList = selectedPrimaryObjects;
			} else if (selectedSecondaryObjects.ContainsKey(obj)) {
				pageIndex = 1;
				selectedObjectsList = selectedSecondaryObjects;
			}
		}


		private void OnObjectsModified(IEnumerable objs) {
			if (ObjectsModified != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(objs, out pageIndex, out selectedObjectsList);
				if (pageIndex >= 0) {
					propertyControllerEventArgs.PageIndex = pageIndex;
					propertyControllerEventArgs.SetObjects(selectedObjectsList.Keys);
					ObjectsModified(this, propertyControllerEventArgs);
				}
			}
		}


		private void OnObjectModified(object obj) {
			if (ObjectsModified != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(obj, out pageIndex, out selectedObjectsList);
				if (pageIndex >= 0) {
					propertyControllerEventArgs.PageIndex = pageIndex;
					propertyControllerEventArgs.SetObjects(selectedObjectsList.Keys);
					ObjectsModified(this, propertyControllerEventArgs);
				}
			}
		}


		private void OnObjectsDeleted(IEnumerable objs) {
			if (ObjectsSet != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(objs, out pageIndex, out selectedObjectsList);
				if (pageIndex >= 0) {
					// Remove all deleted objects and set the rest as new selection
					foreach (object o in objs) {
						if (selectedObjectsList.Contains(o))
							selectedObjectsList.Remove(o);
					}
					propertyControllerEventArgs.PageIndex = pageIndex;
					if (selectedObjectsList.Count > 0)
						propertyControllerEventArgs.SetObjects(selectedObjectsList);
					else propertyControllerEventArgs.SetObject(null);

					ObjectsSet(this, propertyControllerEventArgs);
				}
			}
		}


		private void OnObjectDeleted(object obj) {
			if (this.ObjectsSet != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(obj, out pageIndex, out selectedObjectsList);
				if (pageIndex >= 0 && selectedObjectsList.Contains(obj)) {
					selectedObjectsList.Remove(obj);
					propertyControllerEventArgs.PageIndex = pageIndex;
					if (selectedObjectsList.Count > 0)
						propertyControllerEventArgs.SetObjects(selectedObjectsList);
					else propertyControllerEventArgs.SetObject(null);

					ObjectsSet(this, propertyControllerEventArgs);
				}
			}
		}

		
		private abstract class PropertySetInfo {

			public PropertySetInfo(object newValue) {
				this.newValue = newValue;
			}

			public abstract IList Objects { get; }


			public List<object> OldValues {
				get { return oldValues; }
			}


			public object NewValue {
				get { return newValue; }
			}


			public abstract Type ObjectType { get; }


			public abstract PropertyInfo PropertyInfo { get; }
			

			private List<object> oldValues = new List<object>();
			private object newValue;
		}


		private class PropertySetInfo<TObject> : PropertySetInfo {

			public PropertySetInfo(PropertyInfo propertyInfo, object newValue)
				: base(newValue) {
				this.objects = new List<TObject>();
				this.propertyInfo = propertyInfo;
			}


			public override IList Objects {
				get { return objects; }
			}


			public override Type ObjectType {
				get { return typeof(TObject); }
			}


			public override PropertyInfo PropertyInfo {
				get { return propertyInfo; }
			}


			private List<TObject> objects;
			private PropertyInfo propertyInfo;
		}


		#region Fields
		// property fields
		private Project project;
		private bool updateRepository;

		private Hashtable selectedPrimaryObjects = new Hashtable();
		private Hashtable selectedSecondaryObjects = new Hashtable();
		private PropertyControllerEventArgs propertyControllerEventArgs = new PropertyControllerEventArgs();
		private PropertySetInfo propertySetBuffer;
		#endregion
	}
}
