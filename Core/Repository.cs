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
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	#region IRepository Interface

	/// <summary>
	/// Defines the contract for storing NShape projects.
	/// </summary>
	public interface IRepository {

		/// <summary>
		/// Specifies the base storage format version.
		/// </summary>
		int Version { get; set; }

		/// <summary>
		/// Specifies the project for this cache. Null if not set.
		/// </summary>
		string ProjectName { get; set; }

		/// <summary>
		/// Registers an entity with the cache.
		/// </summary>
		/// <param name="entityType">Entity type names must be unique ignoring their casing.</param>
		void AddEntityType(IEntityType entityType);

		/// <summary>
		/// Unregisters an entity with the cache.
		/// </summary>
		/// <param name="entityType">Name of entity type to remove with correct casing</param>
		void RemoveEntityType(string entityTypeName);

		/// <summary>
		/// Removes all registered entity types.
		/// </summary>
		/// <remarks>This method must be called before different libraries are loaded
		/// and their entities re-registered.</remarks>
		// TODO 2: Unnecessary; remove them when closed.
		void RemoveAllEntityTypes();

		/// <summary>
		/// Indicates whether the project exists in the persistent store of the cache.
		/// </summary>
		/// <returns>True, wenn the cache is connected to an existing persistent store.</returns>
		bool Exists();

		/// <summary>
		/// Creates and opens a new project in the cache.
		/// </summary>
		/// <remarks>Create does not actually create the cache. We want to give the client 
		// a chance to not flush it and thereby not having performed any durable action.</remarks>
		void Create();

		/// <summary>
		/// Opens an existing project in the cache.
		/// </summary>
		void Open();

		/// <summary>
		/// Closes the cache.
		/// </summary>
		void Close();

		/// <summary>
		/// Deletes the persistent store of the project from the cache.
		/// </summary>
		void Erase();

		/// <summary>
		/// Indicates, whether the reposistory is open.
		/// </summary>
		bool IsOpen { get; }

		/// <summary>
		/// Indicates, whether modifications have been performed since the last call
		/// to Open or SaveChanges.
		/// </summary>
		bool IsModified { get; }

		/// <summary>
		/// Gets a bottom z-order value for the given diagram.
		/// </summary>
		/// <param name="diagram"></param>
		/// <returns></returns>
		int ObtainNewBottomZOrder(Diagram diagram);

		/// <summary>
		/// Gets a top z-order value for the given diagram.
		/// </summary>
		/// <param name="diagram"></param>
		/// <returns></returns>
		int ObtainNewTopZOrder(Diagram diagram);

		/// <summary>
		/// Submits all modifications in the cache to the data store.
		/// </summary>
		void SaveChanges();


		#region Project

		/// <summary>
		/// Retrieves the current project.
		/// </summary>
		/// <returns></returns>
		ProjectSettings GetProject();

		/// <summary>
		/// Updates the current project.
		/// </summary>
		void UpdateProject();

		/// <summary>
		/// Deletes the current project.
		/// </summary>
		void DeleteProject();

		event EventHandler<RepositoryProjectEventArgs> ProjectUpdated;

		#endregion


		#region Model

		/// <summary>
		/// Retrieves the current model.
		/// </summary>
		/// <returns></returns>
		Model GetModel();

		/// <summary>
		/// Inserts a new model.
		/// </summary>
		void InsertModel(Model model);

		/// <summary>
		/// Updates the current model.
		/// </summary>
		void UpdateModel(Model model);

		/// <summary>
		/// Deletes the current model.
		/// </summary>
		void DeleteModel(Model model);

		/// <summary>
		/// Undeletes the current model.
		/// </summary>
		void UndeleteModel(Model model);

		event EventHandler<RepositoryModelEventArgs> ModelInserted;

		event EventHandler<RepositoryModelEventArgs> ModelUpdated;

		event EventHandler<RepositoryModelEventArgs> ModelDeleted;

		#endregion


		#region Designs

		/// <summary>
		/// Retrieves all known designs.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Design> GetDesigns();

		/// <summary>
		/// Fetches a single design object from the cache.
		/// </summary>
		/// <param name="id">Id of design to fetch. Null to indicate the project design.</param>
		/// <returns>Reference to object</returns>
		Design GetDesign(object id);

		/// <summary>
		/// Inserts a new design into the cache.
		/// </summary>
		/// <param name="design"></param>
		void InsertDesign(Design design);

		void UpdateDesign(Design design);

		void DeleteDesign(Design design);

		void UndeleteDesign(Design design);

		event EventHandler<RepositoryDesignEventArgs> DesignInserted;

		event EventHandler<RepositoryDesignEventArgs> DesignUpdated;

		event EventHandler<RepositoryDesignEventArgs> DesignDeleted;

		#endregion


		#region Styles

		void InsertStyle(Design design, IStyle style);

		void UpdateStyle(IStyle style);

		void DeleteStyle(IStyle style);

		void UndeleteStyle(Design design, IStyle style);

		event EventHandler<RepositoryStyleEventArgs> StyleInserted;

		event EventHandler<RepositoryStyleEventArgs> StyleUpdated;

		event EventHandler<RepositoryStyleEventArgs> StyleDeleted;

		#endregion


		#region Diagrams

		/// <summary>
		/// Fetches a single diagram object from the cache.
		/// </summary>
		/// <param name="id">Id of object to fetch</param>
		/// <returns>Reference to object or null, if object was not found.</returns>
		Diagram GetDiagram(object id);

		/// <summary>
		/// Fetches a single diagram identified by its projectName.
		/// </summary>
		/// <param name="projectName"></param>
		/// <returns></returns>
		Diagram GetDiagram(string name);

		/// <summary>
		/// Liefert die Liste aller Diagramme in diesem Projekt
		/// </summary>
		/// <returns></returns>
		IEnumerable<Diagram> GetDiagrams();

		/// <summary>
		/// Fügt dem Projekt ein neues Diagramm hinzu.
		/// </summary>
		/// <param name="image"></param>
		void InsertDiagram(Diagram diagram);

		void UpdateDiagram(Diagram diagram);

		void DeleteDiagram(Diagram diagram);

		void UndeleteDiagram(Diagram diagram);

		event EventHandler<RepositoryDiagramEventArgs> DiagramInserted;

		event EventHandler<RepositoryDiagramEventArgs> DiagramUpdated;

		event EventHandler<RepositoryDiagramEventArgs> DiagramDeleted;

		#endregion


		# region Templates

		/// <summary>
		/// Fetches a single object from the cache.
		/// </summary>
		/// <param name="id">Id of object to fetch</param>
		/// <returns>Reference to object or null, if object was not found.</returns>
		Template GetTemplate(object id);
		
		/// <summary>
		/// Fetches a single template given its projectName.
		/// </summary>
		/// <param name="projectName"></param>
		/// <returns></returns>
		Template GetTemplate(string name);

		/// <summary>
		/// Fetches all Templates in the project from the cache.
		/// </summary>
		/// <returns>Iterator to step through the Templates list.</returns>
		IEnumerable<Template> GetTemplates();

		void InsertTemplate(Template template);

		void UpdateTemplate(Template template);

		/// <summary>
		/// Replaces the shape of the template.
		/// </summary>
		/// <param name="template"></param>
		/// <param name="oldShape"></param>
		/// <param name="newShape"></param>
		void ReplaceTemplateShape(Template template, Shape oldShape, Shape newShape);

		void DeleteTemplate(Template template);

		void UndeleteTemplate(Template template);

		event EventHandler<RepositoryTemplateEventArgs> TemplateInserted;

		event EventHandler<RepositoryTemplateEventArgs> TemplateUpdated;

		event EventHandler<RepositoryTemplateEventArgs> TemplateDeleted;

		event EventHandler<RepositoryTemplateShapeReplacedEventArgs> TemplateShapeReplaced;

		#endregion


		#region ModelMappings

		void InsertModelMapping(IModelMapping modelMapping, Template template);

		void InsertModelMappings(IEnumerable<IModelMapping> modelMappings, Template template);

		void UpdateModelMapping(IModelMapping modelMapping);

		void UpdateModelMapping(IEnumerable<IModelMapping> modelMappings);

		void DeleteModelMapping(IModelMapping modelMapping);

		void DeleteModelMappings(IEnumerable<IModelMapping> modelMappings);

		void UndeleteModelMappings(IModelMapping modelMapping, Template template);

		void UndeleteModelMappings(IEnumerable<IModelMapping> modelMappings, Template template);

		event EventHandler<RepositoryTemplateEventArgs> ModelMappingsInserted;

		event EventHandler<RepositoryTemplateEventArgs> ModelMappingsUpdated;

		event EventHandler<RepositoryTemplateEventArgs> ModelMappingsDeleted;

		#endregion


		#region Shapes

		/// <summary>
		/// Makes sure that the shapes for the given diagram that intersect with one 
		/// of the given rectangles are loaded.
		/// </summary>
		void GetDiagramShapes(Diagram diagram, params Rectangle[] rectangles);

		void InsertShape(Shape shape, Diagram diagram);

		void InsertShape(Shape shape, Shape parentShape);

		void InsertShape(Shape shape, Template owningTemplate);

		void InsertShapes(IEnumerable<Shape> shapes, Diagram diagram);

		void InsertShapes(IEnumerable<Shape> shapes, Shape parentShape);

		void UpdateShape(Shape shape);

		/// <summary>
		/// SaveChanges the shape's parent, which is now a diagram.
		/// </summary>
		/// <param name="shape">Shape whose parent has changed</param>
		/// <param name="diagram">New parent of the shape</param>
		void UpdateShapeOwner(Shape shape, Diagram diagram);

		/// <summary>
		/// SaveChanges the shape's parent, which is now a shape.
		/// </summary>
		/// <param name="shape">Shape whose parent has changed</param>
		/// <param name="parent">New parent of the shape</param>
		void UpdateShapeOwner(Shape shape, Shape parent);

		void UpdateShapes(IEnumerable<Shape> shapes);

		void DeleteShape(Shape shape);

		void DeleteShapes(IEnumerable<Shape> shapes);

		void UndeleteShape(Shape shape, Diagram diagram);

		void UndeleteShape(Shape shape, Shape parent);

		void UndeleteShapes(IEnumerable<Shape> shapes, Diagram diagram);

		void UndeleteShapes(IEnumerable<Shape> shapes, Shape parent);

		/// <summary>
		/// Removes all shapes of the diagram from the cache cache.
		/// </summary>
		/// <param name="diagram"></param>
		void UnloadDiagramShapes(Diagram diagram);

		/// <summary>
		/// Inserts a new shape connection into the cache.
		/// </summary>
		void InsertShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId);

		/// <summary>
		/// Deletes a shape connection from the cache.
		/// </summary>
		void DeleteShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId);

		event EventHandler<RepositoryShapesEventArgs> ShapesInserted;

		event EventHandler<RepositoryShapesEventArgs> ShapesUpdated;

		event EventHandler<RepositoryShapesEventArgs> ShapesDeleted;

		#endregion


		#region ModelObjects

		/// <summary>
		/// Fetches a single object from the cache.
		/// </summary>
		/// <param name="id">Id of object to fetch</param>
		/// <returns>Reference to object or null, if object was not found.</returns>
		IModelObject GetModelObject(object id);

		IEnumerable<IModelObject> GetModelObjects(IModelObject parent);

		void InsertModelObject(IModelObject modelObject);

		void InsertModelObjects(IEnumerable<IModelObject> modelObjects);

		void UpdateModelObject(IModelObject modelObject);

		void UpdateModelObjects(IEnumerable<IModelObject> modelObjects);

		void UpdateModelObjectParent(IModelObject modelObject, IModelObject parent);

		void DeleteModelObject(IModelObject modelObject);

		void DeleteModelObjects(IEnumerable<IModelObject> modelObjects);

		void UndeleteModelObject(IModelObject modelObject);

		void UndeleteModelObjects(IEnumerable<IModelObject> modelObjects);

		/// <summary>
		/// Removes the given model objects from the repository cache.
		/// </summary>
		/// <param name="modelObjects"></param>
		void UnloadModelObjects(IEnumerable<IModelObject> modelObjects);

		event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsInserted;

		event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsUpdated;

		event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		#endregion

	}

	#endregion


	#region Repository events' EventArgs

	/// <summary>
	/// Encapsulates parameters for a project-related cache event.
	/// </summary>
	public class RepositoryProjectEventArgs : EventArgs {

		public RepositoryProjectEventArgs(ProjectSettings projectSettings) {
			if (projectSettings == null) throw new ArgumentNullException("projectSettings");
			this.projectSettings = projectSettings;
		}

		public ProjectSettings Project {
			get { return projectSettings; }
			internal set { projectSettings = value; }
		}

		internal RepositoryProjectEventArgs() { }

		private ProjectSettings projectSettings = null;
	}


	/// <summary>
	/// Encapsulates parameters for a project-related cache event.
	/// </summary>
	public class RepositoryModelEventArgs : EventArgs {

		public RepositoryModelEventArgs(Model model) {
			if (model == null) throw new ArgumentNullException("model");
			this.model = model;
		}

		public Model Model {
			get { return model; }
			internal set { model = value; }
		}

		internal RepositoryModelEventArgs() { }

		private Model model = null;
	}


	/// <summary>
	/// Encapsulates parameters for a design-related cache event.
	/// </summary>
	public class RepositoryDesignEventArgs : EventArgs {

		public RepositoryDesignEventArgs(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			this.design = design;
		}

		public Design Design {
			get { return design; }
			internal set { design = value; }
		}

		internal RepositoryDesignEventArgs() { }

		private Design design = null;
	}


	/// <summary>
	/// Encapsulates parameters for a project-related cache event.
	/// </summary>
	public class RepositoryStyleEventArgs : EventArgs {

		public RepositoryStyleEventArgs(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			this.style = style;
		}

		public IStyle Style {
			get { return style; }
			internal set { style = value; }
		}

		internal RepositoryStyleEventArgs() { }

		private IStyle style = null;
	}


	/// <summary>
	/// Encapsulates parameters for a diagram-related cache event.
	/// </summary>
	public class RepositoryDiagramEventArgs : EventArgs {

		public RepositoryDiagramEventArgs(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
		}

		public Diagram Diagram {
			get { return diagram; }
			internal set { diagram = value; }
		}

		internal RepositoryDiagramEventArgs() { }

		private Diagram diagram = null;
	}


	/// <summary>
	/// Encapsulates parameters for a template-related cache event.
	/// </summary>
	public class RepositoryTemplateEventArgs : EventArgs {

		public RepositoryTemplateEventArgs(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			this.template = template;
		}

		public Template Template {
			get { return template; }
			internal set { template = value; }
		}

		internal RepositoryTemplateEventArgs() { }

		private Template template = null;
	}


	/// <summary>
	/// Encapsulates parameters for a cache events raised when template shapes are exchanged.
	/// </summary>
	public class RepositoryTemplateShapeReplacedEventArgs : RepositoryTemplateEventArgs {

		public RepositoryTemplateShapeReplacedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape)
			: base(template) {
			if (oldTemplateShape == null) throw new ArgumentNullException("oldTemplateShape");
			if (newTemplateShape == null) throw new ArgumentNullException("newTemplateShape");
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


		internal RepositoryTemplateShapeReplacedEventArgs() : base() { }

		private Shape oldTemplateShape = null;
		private Shape newTemplateShape = null;
	}


	/// <summary>
	/// Encapsulates parameters for a shape-related cache event.
	/// </summary>
	public class RepositoryShapeEventArgs : EventArgs {

		public RepositoryShapeEventArgs(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			this.shape = shape;
			this.diagram = diagram;
		}

		public Shape Shape {
			get { return shape; }
			internal set { shape = value; }
		}


		public Diagram Diagram {
			get { return Diagram; }
		}


		internal RepositoryShapeEventArgs() { }


		private Shape shape = null;
		private Diagram diagram = null;
	}


	/// <summary>
	/// Encapsulates parameters for a shape-related cache event.
	/// </summary>
	public class RepositoryShapesEventArgs : EventArgs {

		public RepositoryShapesEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			SetShapes(shapes, diagram);
		}


		public RepositoryShapesEventArgs(IEnumerable<KeyValuePair<Shape, Diagram>> shapesWithDiagrams) {
			if (shapesWithDiagrams == null) throw new ArgumentNullException("shapesWithDiagrams");
			shapes.Clear();
			foreach (KeyValuePair<Shape, Diagram> item in shapesWithDiagrams)
				shapes.Add(item.Key, item.Value);
		}


		public IEnumerable<Shape> Shapes {
			get { return shapes.Keys; }
		}


		public Diagram GetDiagram(Shape s) {
			Diagram d;
			if (shapes.TryGetValue(s, out d)) return d;
			else return null;
		}


		public int Count {
			get { return shapes.Count; }
		}


		internal RepositoryShapesEventArgs() {
			this.shapes.Clear();
		}


		internal void Clear() {
			shapes.Clear();
		}
		
		
		internal void AddShape(Shape shape, Diagram diagram) {
			shapes.Add(shape, diagram);
		}


		internal void SetShapes(IEnumerable<Shape> shapes, Diagram diagram) {
			this.shapes.Clear();
			foreach (Shape s in shapes) this.shapes.Add(s, diagram);
		}


		internal void SetShape(Shape shape, Diagram diagram) {
			this.shapes.Clear();
			this.shapes.Add(shape, diagram);
		}


		private Dictionary<Shape, Diagram> shapes = new Dictionary<Shape,Diagram>();
	}


	/// <summary>
	/// Encapsulates parameters for a modelobject-related cache event.
	/// </summary>
	public class RepositoryModelObjectEventArgs : EventArgs {

		public RepositoryModelObjectEventArgs(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			this.modelObject = modelObject;
		}

		public IModelObject ModelObject {
			get { return modelObject; }
			internal set { modelObject = value; }
		}

		internal RepositoryModelObjectEventArgs() { }

		private IModelObject modelObject = null;
	}


	/// <summary>
	/// Encapsulates parameters for a modelobject-related cache event.
	/// </summary>
	public class RepositoryModelObjectsEventArgs : EventArgs {

		public RepositoryModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			this.modelObjects.AddRange(modelObjects);
		}


		public IEnumerable<IModelObject> ModelObjects { get { return modelObjects; } }


		public int Count { get { return modelObjects.Count; } }


		internal RepositoryModelObjectsEventArgs() {
			modelObjects.Clear();
		}


		internal void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			this.modelObjects.Clear();
			this.modelObjects.AddRange(modelObjects);
		}


		internal void SetModelObject(IModelObject modelObject) {
			modelObjects.Clear();
			modelObjects.Add(modelObject);
		}


		private List<IModelObject> modelObjects = new List<IModelObject>();
	}

	#endregion

}
