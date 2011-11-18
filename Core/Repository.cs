/******************************************************************************
  Copyright 2009-2011 dataweb GmbH
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
		/// Specifies the project for this repository. Null if not set.
		/// </summary>
		string ProjectName { get; set; }

		/// <summary>
		/// Registers an entity with the repository.
		/// </summary>
		/// <param name="entityType">Entity type names must be unique ignoring their casing.</param>
		void AddEntityType(IEntityType entityType);

		/// <summary>
		/// Unregisters an entity with the repository.
		/// </summary>
		void RemoveEntityType(string entityTypeName);

		// TODO 2: Unnecessary; remove them when closed.
		/// <summary>
		/// Removes all registered entity types.
		/// </summary>
		/// <remarks>This method must be called before different libraries are loaded
		/// and their entities re-registered.</remarks>
		void RemoveAllEntityTypes();

		/// <summary>
		/// Indicates whether the project exists in the persistent store of the repository.
		/// </summary>
		/// <returns>True, wenn the repository is connected to an existing persistent store.</returns>
		bool Exists();

		/// <summary>
		/// Reads the version number of the project from the persistent store.
		/// </summary>
		void ReadVersion();
		
		/// <summary>
		/// Creates and opens a new project in the repository.
		/// </summary>
		/// <remarks>Create does not actually create the repository. We want to give the client 
		/// a chance to not flush it and thereby not having performed any durable action.</remarks>
		void Create();

		/// <summary>
		/// Opens an existing project in the repository.
		/// </summary>
		void Open();

		/// <summary>
		/// Closes the repository.
		/// </summary>
		void Close();

		/// <summary>
		/// Deletes the persistent store of the project from the repository.
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
		/// Submits all modifications in the repository to the data store.
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

		/// <ToBeCompleted></ToBeCompleted>
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

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryModelEventArgs> ModelInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryModelEventArgs> ModelUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryModelEventArgs> ModelDeleted;

		#endregion


		#region Designs

		/// <summary>
		/// Retrieves all known designs.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Design> GetDesigns();

		/// <summary>
		/// Fetches a single design object from the repository.
		/// </summary>
		/// <param name="id">Id of design to fetch. Null to indicate the project design.</param>
		/// <returns>Reference to object</returns>
		Design GetDesign(object id);

		/// <summary>
		/// Inserts a new design into the repository.
		/// </summary>
		void InsertDesign(Design design);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateDesign(Design design);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteDesign(Design design);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteDesign(Design design);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryDesignEventArgs> DesignInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryDesignEventArgs> DesignUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryDesignEventArgs> DesignDeleted;

		#endregion


		#region Styles

		/// <ToBeCompleted></ToBeCompleted>
		void InsertStyle(Design design, IStyle style);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateStyle(IStyle style);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteStyle(IStyle style);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteStyle(Design design, IStyle style);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryStyleEventArgs> StyleInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryStyleEventArgs> StyleUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryStyleEventArgs> StyleDeleted;

		#endregion


		#region Diagrams

		/// <summary>
		/// Fetches a single diagram object from the repository.
		/// </summary>
		/// <param name="id">Id of object to fetch</param>
		/// <returns>Reference to object or null, if object was not found.</returns>
		Diagram GetDiagram(object id);

		/// <summary>
		/// Fetches a single diagram identified by its name.
		/// </summary>
		Diagram GetDiagram(string name);

		/// <summary>
		/// Liefert die Liste aller Diagramme in diesem Projekt
		/// </summary>
		/// <returns></returns>
		IEnumerable<Diagram> GetDiagrams();

		/// <summary>
		/// Fügt dem Projekt ein neues Diagramm hinzu.
		/// </summary>
		void InsertDiagram(Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateDiagram(Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteDiagram(Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteDiagram(Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryDiagramEventArgs> DiagramInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryDiagramEventArgs> DiagramUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryDiagramEventArgs> DiagramDeleted;

		#endregion


		# region Templates

		/// <summary>
		/// Fetches a single object from the repository.
		/// </summary>
		/// <param name="id">Id of object to fetch</param>
		/// <returns>Reference to object or null, if object was not found.</returns>
		Template GetTemplate(object id);
		
		/// <summary>
		/// Fetches a single template given its name.
		/// </summary>
		Template GetTemplate(string name);

		/// <summary>
		/// Fetches all <see cref="T:Dataweb.NShape.Advanced.Template" /> in the project from the repository.
		/// </summary>
		/// <returns>Iterator to step through the <see cref="T:Dataweb.NShape.Advanced.Template" /> list.</returns>
		IEnumerable<Template> GetTemplates();

		/// <ToBeCompleted></ToBeCompleted>
		void InsertTemplate(Template template);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateTemplate(Template template);

		/// <summary>
		/// Replaces the shape of the template.
		/// </summary>
		void ReplaceTemplateShape(Template template, Shape oldShape, Shape newShape);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteTemplate(Template template);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteTemplate(Template template);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateEventArgs> TemplateInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateEventArgs> TemplateUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateEventArgs> TemplateDeleted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateShapeReplacedEventArgs> TemplateShapeReplaced;

		#endregion


		#region ModelMappings

		/// <ToBeCompleted></ToBeCompleted>
		void InsertModelMapping(IModelMapping modelMapping, Template template);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertModelMappings(IEnumerable<IModelMapping> modelMappings, Template template);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateModelMapping(IModelMapping modelMapping);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateModelMappings(IEnumerable<IModelMapping> modelMappings);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteModelMapping(IModelMapping modelMapping);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteModelMappings(IEnumerable<IModelMapping> modelMappings);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteModelMapping(IModelMapping modelMapping, Template template);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteModelMappings(IEnumerable<IModelMapping> modelMappings, Template template);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateEventArgs> ModelMappingsInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateEventArgs> ModelMappingsUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryTemplateEventArgs> ModelMappingsDeleted;

		#endregion


		#region Shapes

		/// <summary>
		/// Makes sure that the shapes for the given diagram that intersect with one 
		/// of the given rectangles are loaded.
		/// </summary>
		void GetDiagramShapes(Diagram diagram, params Rectangle[] rectangles);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertShape(Shape shape, Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertShape(Shape shape, Shape parentShape);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertShape(Shape shape, Template owningTemplate);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertShapes(IEnumerable<Shape> shapes, Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertShapes(IEnumerable<Shape> shapes, Shape parentShape);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateShape(Shape shape);

		/// <summary>
		/// Updates the shape's parent, which is now a <see cref="T:Dataweb.NShape.Diagram" />.
		/// </summary>
		/// <param name="shape"><see cref="T:Dataweb.NShape.Advanced.Shape" /> whose parent has changed</param>
		/// <param name="diagram">New parent of the <see cref="T:Dataweb.NShape.Advanced.Shape" /></param>
		void UpdateShapeOwner(Shape shape, Diagram diagram);

		/// <summary>
		/// Updates the shape's parent, which is now a <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		/// <param name="shape"><see cref="T:Dataweb.NShape.Advanced.Shape" /> whose parent has changed</param>
		/// <param name="parent">New parent of the <see cref="T:Dataweb.NShape.Advanced.Shape" /></param>
		void UpdateShapeOwner(Shape shape, Shape parent);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateShapes(IEnumerable<Shape> shapes);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteShape(Shape shape);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteShapes(IEnumerable<Shape> shapes);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteShape(Shape shape, Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteShape(Shape shape, Shape parent);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteShapes(IEnumerable<Shape> shapes, Diagram diagram);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteShapes(IEnumerable<Shape> shapes, Shape parent);

		/// <summary>
		/// Removes all shapes of the diagram from the repository.
		/// </summary>
		/// <param name="diagram"></param>
		void UnloadDiagramShapes(Diagram diagram);

		/// <summary>
		/// Inserts a new shape connection into the repository.
		/// </summary>
		void InsertShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId);

		/// <summary>
		/// Deletes a shape connection from the repository.
		/// </summary>
		void DeleteShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryShapesEventArgs> ShapesInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryShapesEventArgs> ShapesUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryShapesEventArgs> ShapesDeleted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryShapeConnectionEventArgs> ShapeConnectionInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryShapeConnectionEventArgs> ShapeConnectionDeleted;

		#endregion


		#region ModelObjects

		/// <summary>
		/// Fetches a single object from the repository.
		/// </summary>
		/// <param name="id">Id of object to fetch</param>
		/// <returns>Reference to object or null, if object was not found.</returns>
		IModelObject GetModelObject(object id);

		/// <ToBeCompleted></ToBeCompleted>
		IEnumerable<IModelObject> GetModelObjects(IModelObject parent);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertModelObject(IModelObject modelObject);

		/// <ToBeCompleted></ToBeCompleted>
		void InsertModelObjects(IEnumerable<IModelObject> modelObjects);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateModelObject(IModelObject modelObject);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateModelObjects(IEnumerable<IModelObject> modelObjects);

		/// <ToBeCompleted></ToBeCompleted>
		void UpdateModelObjectParent(IModelObject modelObject, IModelObject parent);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteModelObject(IModelObject modelObject);

		/// <ToBeCompleted></ToBeCompleted>
		void DeleteModelObjects(IEnumerable<IModelObject> modelObjects);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteModelObject(IModelObject modelObject);

		/// <ToBeCompleted></ToBeCompleted>
		void UndeleteModelObjects(IEnumerable<IModelObject> modelObjects);

		/// <summary>
		/// Removes the given model objects from the repository repository.
		/// </summary>
		/// <param name="modelObjects"></param>
		void UnloadModelObjects(IEnumerable<IModelObject> modelObjects);

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsInserted;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsUpdated;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		#endregion

	}

	#endregion


	#region Repository events' EventArgs

	/// <summary>
	/// Encapsulates parameters for a project-related respository event.
	/// </summary>
	public class RepositoryProjectEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryProjectEventArgs(ProjectSettings projectSettings) {
			if (projectSettings == null) throw new ArgumentNullException("projectSettings");
			this.projectSettings = projectSettings;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public ProjectSettings Project {
			get { return projectSettings; }
			internal set { projectSettings = value; }
		}

		internal RepositoryProjectEventArgs() { }

		private ProjectSettings projectSettings = null;
	}


	/// <summary>
	/// Encapsulates parameters for a project-related respository event.
	/// </summary>
	public class RepositoryModelEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryModelEventArgs(Model model) {
			if (model == null) throw new ArgumentNullException("model");
			this.model = model;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Model Model {
			get { return model; }
			internal set { model = value; }
		}

		internal RepositoryModelEventArgs() { }

		private Model model = null;
	}


	/// <summary>
	/// Encapsulates parameters for a design-related respository event.
	/// </summary>
	public class RepositoryDesignEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryDesignEventArgs(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			this.design = design;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Design Design {
			get { return design; }
			internal set { design = value; }
		}

		internal RepositoryDesignEventArgs() { }

		private Design design = null;
	}


	/// <summary>
	/// Encapsulates parameters for a project-related respository event.
	/// </summary>
	public class RepositoryStyleEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryStyleEventArgs(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			this.style = style;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IStyle Style {
			get { return style; }
			internal set { style = value; }
		}

		internal RepositoryStyleEventArgs() { }

		private IStyle style = null;
	}


	/// <summary>
	/// Encapsulates parameters for a diagram-related respository event.
	/// </summary>
	public class RepositoryDiagramEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryDiagramEventArgs(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Diagram Diagram {
			get { return diagram; }
			internal set { diagram = value; }
		}

		internal RepositoryDiagramEventArgs() { }

		private Diagram diagram = null;
	}


	/// <summary>
	/// Encapsulates parameters for a template-related respository event.
	/// </summary>
	public class RepositoryTemplateEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryTemplateEventArgs(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			this.template = template;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Template Template {
			get { return template; }
			internal set { template = value; }
		}

		internal RepositoryTemplateEventArgs() { }

		private Template template = null;
	}


	/// <summary>
	/// Encapsulates parameters for respository events raised when template shapes are exchanged.
	/// </summary>
	public class RepositoryTemplateShapeReplacedEventArgs : RepositoryTemplateEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryTemplateShapeReplacedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape)
			: base(template) {
			if (oldTemplateShape == null) throw new ArgumentNullException("oldTemplateShape");
			if (newTemplateShape == null) throw new ArgumentNullException("newTemplateShape");
			this.oldTemplateShape = oldTemplateShape;
			this.newTemplateShape = newTemplateShape;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Shape OldTemplateShape {
			get { return oldTemplateShape; }
			internal set { oldTemplateShape = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Shape NewTemplateShape {
			get { return newTemplateShape; }
			internal set { newTemplateShape = value; }
		}


		internal RepositoryTemplateShapeReplacedEventArgs() : base() { }

		private Shape oldTemplateShape = null;
		private Shape newTemplateShape = null;
	}


	/// <summary>
	/// Encapsulates parameters for a shape-related respository event.
	/// </summary>
	public class RepositoryShapeEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryShapeEventArgs(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			this.shape = shape;
			this.diagram = diagram;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Shape Shape {
			get { return shape; }
			internal set { shape = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Diagram Diagram {
			get { return Diagram; }
		}


		internal RepositoryShapeEventArgs() { }


		private Shape shape = null;
		private Diagram diagram = null;
	}


	/// <summary>
	/// Encapsulates parameters for a shape-related respository event.
	/// </summary>
	public class RepositoryShapesEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryShapesEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			SetShapes(shapes, diagram);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryShapesEventArgs(IEnumerable<KeyValuePair<Shape, Diagram>> shapesWithDiagrams) {
			if (shapesWithDiagrams == null) throw new ArgumentNullException("shapesWithDiagrams");
			shapes.Clear();
			foreach (KeyValuePair<Shape, Diagram> item in shapesWithDiagrams)
				shapes.Add(item.Key, item.Value);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<Shape> Shapes {
			get { return shapes.Keys; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Diagram GetDiagram(Shape s) {
			Diagram d;
			if (shapes.TryGetValue(s, out d)) return d;
			else return null;
		}


		/// <ToBeCompleted></ToBeCompleted>
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
	/// Encapsulates parameters for a modelobject-related respository event.
	/// </summary>
	public class RepositoryModelObjectEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryModelObjectEventArgs(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			this.modelObject = modelObject;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IModelObject ModelObject {
			get { return modelObject; }
			internal set { modelObject = value; }
		}

		internal RepositoryModelObjectEventArgs() { }

		private IModelObject modelObject = null;
	}


	/// <summary>
	/// Encapsulates parameters for a modelobject-related respository event.
	/// </summary>
	public class RepositoryModelObjectsEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			this.modelObjects.AddRange(modelObjects);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<IModelObject> ModelObjects {
			get { return modelObjects; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int Count {
			get { return modelObjects.Count; }
		}


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


	/// <summary>
	/// Encapsulates parameters for a shape connection related respository event.
	/// </summary>
	public class RepositoryShapeConnectionEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public RepositoryShapeConnectionEventArgs(Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId)
			: this() {
			if (connectorShape == null) throw new ArgumentNullException("connectorShape");
			if (targetShape == null) throw new ArgumentNullException("targetShape");
			if (gluePointId == ControlPointId.Any || gluePointId == ControlPointId.None)
				throw new ArgumentException("gluePointId");
			if (!connectorShape.HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue))
				throw new ArgumentException(string.Format("{0} is not a glue point of {1}.", gluePointId, connectorShape.Type.FullName));
			if (targetPointId == ControlPointId.Any || targetPointId == ControlPointId.None)
				throw new ArgumentException("targetPointId");
			if (!targetShape.HasControlPointCapability(targetPointId, ControlPointCapabilities.Connect))
				throw new ArgumentException(string.Format("{0} is not a connection point of {1}.", targetPointId, targetShape.Type.FullName));

			this.connection.ConnectorShape = connectorShape;
			this.connection.GluePointId = gluePointId;
			this.connection.TargetShape = targetShape;
			this.connection.TargetPointId = targetPointId;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal RepositoryShapeConnectionEventArgs(ShapeConnection shapeConnection) 
			: this(shapeConnection.ConnectorShape, shapeConnection.GluePointId, shapeConnection.TargetShape, shapeConnection.TargetPointId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal RepositoryShapeConnectionEventArgs() {
			this.connection = ShapeConnection.Empty;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Shape ConnectorShape {
			get { return connection.ConnectorShape; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ControlPointId GluePointId {
			get { return connection.GluePointId; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Shape TargetShape {
			get { return connection.TargetShape; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ControlPointId TargetPointId {
			get { return connection.TargetPointId; }
		}


		internal void Clear() {
			connection = ShapeConnection.Empty;
		}
		
		
		internal void SetShapeConnection(ShapeConnection connection) {
			System.Diagnostics.Debug.Assert(connection != ShapeConnection.Empty);
			this.connection = connection;
		}


		private ShapeConnection connection;
	}

	#endregion

}
