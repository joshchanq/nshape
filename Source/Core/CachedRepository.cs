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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Caches modifications to the cache and commits them to the data store
	/// during SaveChanges.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(CachedRepository), "CachedRepository.bmp")]
	public class CachedRepository : Component, IRepository, IStoreCache {

		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Advanced.Store" /> containing the persistent data for this <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />
		/// </summary>
		public Store Store {
			get { return store; }
			set {
				AssertClosed();
				if (store != null) store.ProjectName = string.Empty;
				store = value;
				if (store != null) store.ProjectName = projectName;
			}
		}


		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[Category("NShape")]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Returns the XML representation of the data stored in the cache.
		/// </summary>
		/// <returns></returns>
		public string GetXml() {
			throw new NotImplementedException();
		}


		/// <summary>
		/// Reads XML data into the cache using the specified System.IO.Stream.
		/// </summary>
		/// <param name="stream"></param>
		public void ReadXml(Stream stream) {
			if (stream == null) throw new ArgumentNullException("stream");
			throw new NotImplementedException();
		}


		/// <summary>
		/// Writes the current cache content as XML data.
		/// </summary>
		/// <param name="stream"></param>
		public void WriteXml(Stream stream) {
			if (stream == null) throw new ArgumentNullException("stream");
			throw new NotImplementedException();
		}


#if DEBUG_DIAGNOSTICS
		/// <summary>
		/// Finds the owner of the given shape. For debugging purposes only!
		/// </summary>
		public IEntity FindOwner(Shape shape) {
			foreach (Diagram d in GetCachedEntities<Diagram>(diagrams, newDiagrams))
				if (d.Shapes.Contains(shape)) return d;
			foreach (Template t in GetCachedEntities<Template>(templates, newTemplates))
				if (t.Shape == shape) return t;
			foreach (Shape s in GetCachedEntities<Shape>(shapes, newShapes))
				if (s.Children.Contains(shape)) return s;
			return null;
		}
#endif


		#region IRepository Members

		
		/// <override></override>
		public int Version {
			get { return version; }
			set {
				// ToDo: Check on first/last supported save/load version
				version = value;
				if (store != null) store.Version = version;
			}
		}


		/// <override></override>
		public string ProjectName {
			get { return projectName; }
			set {
				projectName = value;
				if (store != null) store.ProjectName = projectName;
			}
		}


		/// <override></override>
		public bool IsModified {
			get { return isModified; }
		}


		/// <override></override>
		public void AddEntityType(IEntityType entityType) {
			if (entityType == null) throw new ArgumentNullException("entityType");
			if (entityTypes.ContainsKey(CalcElementName(entityType.FullName)))
				throw new NShapeException("The repository already contains an entity type called '{0}'.", entityType.FullName);
			foreach (KeyValuePair<string, IEntityType> item in entityTypes) {
				if (item.Value.FullName.Equals(entityType.FullName, StringComparison.InvariantCultureIgnoreCase))
					throw new NShapeException("The repository already contains an entity type called '{0}'.", entityType.FullName);
			}
			// Calculate the XML element names for all entity identifiers
			entityType.ElementName = CalcElementName(entityType.FullName);
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
				pi.ElementName = CalcElementName(pi.Name);
				if (pi is EntityInnerObjectsDefinition) {
					foreach (EntityPropertyDefinition fi in ((EntityInnerObjectsDefinition)pi).PropertyDefinitions)
						fi.ElementName = CalcElementName(fi.Name);
				}
			}
			entityTypes.Add(entityType.ElementName, entityType);
		}


		/// <override></override>
		public void RemoveEntityType(string entityTypeName) {
			if (entityTypeName == null) throw new ArgumentNullException("entityTypeName");
			if (entityTypeName == string.Empty) throw new ArgumentException("Invalid entity type name.");
			entityTypes.Remove(CalcElementName(entityTypeName));
		}


		/// <override></override>
		public void RemoveAllEntityTypes() {
			entityTypes.Clear();
		}


		/// <override></override>
		public void ReadVersion() {
			if (store == null) throw new Exception("Property Store is not set.");
			else if (!store.Exists()) throw new Exception("Store does not exist.");
			else store.ReadVersion(this);
		}
		
		
		/// <override></override>
		public bool Exists() {
			return store != null && store.Exists();
		}


		/// <override></override>
		public virtual void Create() {
			AssertClosed();
			if (string.IsNullOrEmpty(projectName)) throw new NShapeException("No project name defined.");
			settings = new ProjectSettings();
			newProjects.Add(settings, projectOwner);
			projectDesign = new Design("Project");
			DoInsertDesign(projectDesign, settings);
			if (store != null) {
				store.Version = version;
				store.Create(this);
			}
			isOpen = true;
		}


		/// <override></override>
		public void Open() {
			AssertClosed();
			if (string.IsNullOrEmpty(projectName)) throw new NShapeException("No project name defined.");
			if (store == null) throw new InvalidOperationException("Repository has no store attached. An in-memory repository must be created, not opened.");
			store.ProjectName = projectName;
			store.Open(this);
			// Load the project, must be exactly one.
			store.LoadProjects(this, FindEntityType(ProjectSettings.EntityTypeName, true));
			IEnumerator<EntityBucket<ProjectSettings>> projectEnumerator = projects.Values.GetEnumerator();
			if (!projectEnumerator.MoveNext())
				throw new NShapeException("Project '{0}' not found in repository.", projectName);
			settings = projectEnumerator.Current.ObjectRef;
			if (projectEnumerator.MoveNext())
				throw new NShapeException("Two projects named '{0}' found in repository.", projectName);
			// Load the design, there must be exactly one returned
			store.LoadDesigns(this, ((IEntity)settings).Id);
			IEnumerator<EntityBucket<Design>> designEnumerator = designs.Values.GetEnumerator();
			if (!designEnumerator.MoveNext())
				throw new NShapeException("Project styles not found.");
			projectDesign = designEnumerator.Current.ObjectRef;
			if (designEnumerator.MoveNext()) {
				//throw new NShapeException("More than one project design found in repository.");
				// ToDo: Load addinional designs
			}
			isOpen = true;
			isModified = false;
		}


		/// <override></override>
		public virtual void Close() {
			if (store != null) store.Close(this);
			isOpen = false;
			ClearBuffers();
			isModified = false;
		}


		/// <override></override>
		public void Erase() {
			if (store == null) throw new InvalidOperationException("Repository has no store attached.");
			store.Erase();
		}


		/// <override></override>
		public void SaveChanges() {
			if (store == null) throw new Exception("Repository has no store attached.");
			store.SaveChanges(this);
			AcceptAll();
		}


		/// <override></override>
		[Browsable(false)]
		public bool IsOpen {
			get { return isOpen; }
		}


		/// <override></override>
		public int ObtainNewBottomZOrder(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			return diagram.Shapes.MinZOrder - 10;
		}


		/// <override></override>
		public int ObtainNewTopZOrder(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			return diagram.Shapes.MaxZOrder + 10;
		}


		#region [Public] Project

		/// <override></override>
		public ProjectSettings GetProject() {
			AssertOpen();
			return this.settings;
		}


		/// <override></override>
		public void UpdateProject() {
			AssertOpen();
			UpdateEntity<ProjectSettings>(this.projects, newProjects, this.settings);
			if (ProjectUpdated != null) ProjectUpdated(this, GetProjectEventArgs(this.settings));
		}


		/// <override></override>
		public void DeleteProject() {
			AssertOpen();
			DeleteEntity<ProjectSettings>(this.projects, newProjects, this.settings);
			if (ProjectDeleted != null) ProjectDeleted(this, GetProjectEventArgs(settings));
		}


		/// <override></override>
		public event EventHandler<RepositoryProjectEventArgs> ProjectUpdated;

		/// <override></override>
		public event EventHandler<RepositoryProjectEventArgs> ProjectDeleted;

		#endregion


		#region [Public] Model

		/// <override></override>
		public Model GetModel() {
			AssertOpen();
			if (store != null && models.Count <= 0 && ((IEntity)settings).Id != null)
				store.LoadModel(this, ((IEntity)settings).Id);
			foreach (Model m in GetCachedEntities(models, newModels))
				return m;
			throw new ArgumentException("A model does not exist in the repository.");
		}


		/// <override></override>
		public void InsertModel(Model model) {
			if (model == null) throw new ArgumentNullException("model");
			AssertOpen();
			InsertEntity<Model>(newModels, model, GetProject());
			if (ModelInserted != null) ModelInserted(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public void UpdateModel(Model model) {
			AssertOpen();
			UpdateEntity<Model>(this.models, this.newModels, model);
			if (ModelUpdated != null) ModelUpdated(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public void DeleteModel(Model model) {
			AssertOpen();
			DeleteEntity<Model>(this.models, this.newModels, model);
			if (ModelDeleted != null) ModelDeleted(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public void UndeleteModel(Model model) {
			AssertOpen();
			UndeleteEntity<Model>(models, model);
			if (ModelInserted != null) ModelInserted(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public event EventHandler<RepositoryModelEventArgs> ModelInserted;

		/// <override></override>
		public event EventHandler<RepositoryModelEventArgs> ModelUpdated;

		/// <override></override>
		public event EventHandler<RepositoryModelEventArgs> ModelDeleted;

		#endregion


		#region Templates

		// We assume that this is only called once to load all existing templates.
		/// <override></override>
		public IEnumerable<Template> GetTemplates() {
			AssertOpen();
			if (store != null && ((IEntity)settings).Id != null && templates.Count <= 0)
				store.LoadTemplates(this, ((IEntity)settings).Id);
			foreach (EntityBucket<Template> tb in templates)
				yield return tb.ObjectRef;
			foreach (KeyValuePair<Template, IEntity> item in newTemplates)
				yield return item.Key;
		}


		/// <override></override>
		public void InsertTemplate(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			InsertEntity<Template>(newTemplates, template, GetProject());
			DoInsertShape(template.Shape, template);	// insert with children
			if (template.Shape.ModelObject != null)
				DoInsertModelObject(template.Shape.ModelObject, template);
			
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public Template GetTemplate(object id) {
			if (id == null) throw new ArgumentNullException("id");
			EntityBucket<Template> result = null;
			AssertOpen();
			if (!templates.TryGetValue(id, out result)) {
				AssertStoreExists();
				store.LoadTemplates(this, ((IEntity)settings).Id);
				if (!templates.TryGetValue(id, out result))
					throw new NShapeException("Template with id '{0}' not found in store.", id);
			}
			return result.ObjectRef;
		}


		/// <override></override>
		public Template GetTemplate(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertOpen();
			if (store != null && templates.Count <= 0)
				store.LoadTemplates(this, ((IEntity)settings).Id);
			foreach (Template t in GetCachedEntities<Template>(templates, newTemplates))
				if (t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return t;
			throw new ArgumentException(string.Format("A template named '{0}' does not exist in the repository.", name));
		}


		/// <override></override>
		public void UpdateTemplate(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			UpdateEntity<Template>(templates, newTemplates, template);
			
			// Insert / undelete / update shape
			DoUpdateTemplateShape(template);
			// Insert / undelete / update model object
			DoUpdateTemplateModelObject(template);
			
			if (TemplateUpdated != null) TemplateUpdated(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void ReplaceTemplateShape(Template template, Shape oldShape, Shape newShape) {
			if (template == null) throw new ArgumentNullException("template");
			if (oldShape == null) throw new ArgumentNullException("oldShape");
			if (newShape == null) throw new ArgumentNullException("newShape");
			AssertOpen();
			//DoInsertShape(newShape, template);
			UpdateEntity<Template>(templates, newTemplates, template);
			// Insert/Undelete new shape
			DoUpdateTemplateShape(template);
			DoUpdateTemplateModelObject(template);
			// Delete old shape
			DoDeleteShape(oldShape);

			if (TemplateShapeReplaced != null) TemplateShapeReplaced(this, GetTemplateShapeExchangedEventArgs(template, oldShape, newShape));
		}


		// Insert / undelete / update shape
		private void DoUpdateTemplateShape(Template template) {
			IEntity shapeEntity = template.Shape;
			if (shapeEntity.Id == null && !newShapes.ContainsKey(template.Shape))
				DoInsertShape((Shape)shapeEntity, template);
			else {
				if (CanUndelete<Shape>(template.Shape, shapes))
					DoUndeleteShape(template.Shape, template);
				DoUpdateShape(template.Shape);
			}
		}


		// Insert / undelete / update model object
		private void DoUpdateTemplateModelObject(Template template) {
			// Insert / undelete / update model object
			if (template.Shape.ModelObject != null) {
				IModelObject modelObject = template.Shape.ModelObject;
				if (modelObject.Id == null && !newModelObjects.ContainsKey(modelObject))
					DoInsertModelObject(template.Shape.ModelObject, template);
				else {
					if (CanUndelete<IModelObject>(modelObject, modelObjects))
						DoUndeleteModelObject(modelObject);
					DoUpdateModelObject(template.Shape.ModelObject);
				}
			}
		}


		/// <override></override>
		public void DeleteTemplate(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			// Delete the template's shape
			DeleteEntity<Template>(templates, newTemplates, template);
			DoDeleteShape(template.Shape);
			if (template.Shape.ModelObject != null)
				DoDeleteModelObject(template.Shape.ModelObject);
			if (TemplateDeleted != null) TemplateDeleted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void UndeleteTemplate(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			// Delete the template's shape
			UndeleteEntity<Template>(templates, template);
			DoUndeleteShape(template.Shape, template);
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> TemplateInserted;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> TemplateUpdated;

		/// <override></override>
		public event EventHandler<RepositoryTemplateShapeReplacedEventArgs> TemplateShapeReplaced;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> TemplateDeleted;

		#endregion


		#region ModelMappings

		/// <override></override>
		public void InsertModelMapping(IModelMapping modelMapping, Template template) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			if (template == null) throw new ArgumentNullException("template");
			DoInsertModelMapping(modelMapping, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void InsertModelMappings(IEnumerable<IModelMapping> modelMappings, Template template) {
			if (modelMappings == null) throw new ArgumentNullException("modelMappings");
			if (template == null) throw new ArgumentNullException("template");
			foreach (IModelMapping modelMapping in modelMappings)
				DoInsertModelMapping(modelMapping, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void UpdateModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			DoUpdateModelMapping(modelMapping);
			if (ModelMappingsUpdated != null) ModelMappingsUpdated(this,
				GetTemplateEventArgs(GetModelMappingOwner(modelMapping)));
		}


		/// <override></override>
		public void UpdateModelMappings(IEnumerable<IModelMapping> modelMappings) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			Template owner = null;
			foreach (IModelMapping modelMapping in modelMappings) {
				DoUpdateModelMapping(modelMapping);
				if (owner == null) owner = GetModelMappingOwner(modelMapping);
				else if (owner != GetModelMappingOwner(modelMapping))
					throw new NShapeException("Invalid model mapping owner.");
			}
			if (ModelMappingsUpdated != null) ModelMappingsUpdated(this, GetTemplateEventArgs(owner));
		}


		/// <override></override>
		public void DeleteModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			Template owner = GetModelMappingOwner(modelMapping);
			DoDeleteModelMapping(modelMapping);
			if (ModelMappingsDeleted != null) ModelMappingsDeleted(this, GetTemplateEventArgs(owner));
		}


		/// <override></override>
		public void DeleteModelMappings(IEnumerable<IModelMapping> modelMappings) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			Template owner = null;
			foreach (IModelMapping modelMapping in modelMappings) {
				if (owner == null) owner = GetModelMappingOwner(modelMapping);
				else if (owner != GetModelMappingOwner(modelMapping))
					throw new NShapeException("Invalid model mapping owner.");
				DoDeleteModelMapping(modelMapping);
			}
			if (ModelMappingsDeleted != null) ModelMappingsDeleted(this, GetTemplateEventArgs(owner));
		}


		/// <override></override>
		public void UndeleteModelMapping(IModelMapping modelMapping, Template template) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			DoUndeleteModelMapping(modelMapping, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void UndeleteModelMappings(IEnumerable<IModelMapping> modelMappings, Template template) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			foreach (IModelMapping modelMapping in modelMappings)
				DoUndeleteModelMapping(modelMapping, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsInserted;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsUpdated;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsDeleted;

		#endregion


		#region Diagrams

		/// <override></override>
		public IEnumerable<Diagram> GetDiagrams() {
			AssertOpen();
			if (store != null && diagrams.Count <= 0 && ((IEntity)settings).Id != null)
				store.LoadDiagrams(this, ((IEntity)settings).Id);
			foreach (EntityBucket<Diagram> db in diagrams)
				yield return db.ObjectRef;
			foreach (KeyValuePair<Diagram, IEntity> item in newDiagrams)
				yield return item.Key;
		}


		/// <override></override>
		public Diagram GetDiagram(object id) {
			if (id == null) throw new ArgumentNullException("id");
			EntityBucket<Diagram> result = null;
			AssertOpen();
			if (!diagrams.TryGetValue(id, out result)) {
				AssertStoreExists();
				store.LoadDiagrams(this, ((IEntity)settings).Id);
				if (!diagrams.TryGetValue(id, out result))
					throw new NShapeException("Diagram with id '{0}' not found in repository.", id);
			}
			return result.ObjectRef;
		}


		/// <override></override>
		public Diagram GetDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertOpen();
			// If there is a diagram, we assume we have already loaded them all.
			if (store != null && diagrams.Count <= 0 && ((IEntity)settings).Id != null)
				store.LoadDiagrams(this, ((IEntity)settings).Id);
			foreach (Diagram d in GetCachedEntities(diagrams, newDiagrams))
				if (d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return d;
			throw new ArgumentException(string.Format("A diagram named '{0}' does not exist in the repository.", name));
		}


		/// <override></override>
		public void InsertDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			InsertEntity<Diagram>(newDiagrams, diagram, GetProject());
			foreach (Shape s in diagram.Shapes) {
				InsertEntity<Shape>(newShapes, s, diagram);
				foreach (ShapeConnectionInfo sci in s.GetConnectionInfos(ControlPointId.Any, null))
					if (s.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
						InsertShapeConnection(s, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
			}
			if (DiagramInserted != null) DiagramInserted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void UpdateDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			UpdateEntity<Diagram>(diagrams, newDiagrams, diagram);
			if (DiagramUpdated != null) DiagramUpdated(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void DeleteDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			// First, delete all shapes with their connections
			foreach (Shape s in diagram.Shapes)
				DoDeleteShape(s);
			if (ShapesDeleted != null) ShapesDeleted(this, GetShapesEventArgs(diagram.Shapes, diagram));
			// Now we can delete the actual diagram
			DeleteEntity<Diagram>(diagrams, newDiagrams, diagram);
			//
			if (DiagramDeleted != null) DiagramDeleted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void UndeleteDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			UndeleteEntity<Diagram>(diagrams, diagram);
			// First, delete all shapes with their connections
			foreach (Shape s in diagram.Shapes) {
				DoUndeleteShape(s, diagram);
				// What to do with shape connections?
			}
			if (DiagramInserted != null) DiagramInserted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public event EventHandler<RepositoryDiagramEventArgs> DiagramInserted;

		/// <override></override>
		public event EventHandler<RepositoryDiagramEventArgs> DiagramUpdated;

		/// <override></override>
		public event EventHandler<RepositoryDiagramEventArgs> DiagramDeleted;

		#endregion


		#region Shapes

		/// <override></override>
		public void GetDiagramShapes(Diagram diagram, params Rectangle[] rectangles) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			// For the time being a diagram is either loaded or not. No partial loading yet.
			if (diagram.Shapes.Count > 0) return;
			// Load missing shapes
			if (store != null) store.LoadDiagramShapes(this, diagram);
		}


		/// <override></override>
		public void InsertShape(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			if (((IEntity)diagram).Id != null && !diagrams.ContainsKey(((IEntity)diagram).Id)
				|| ((IEntity)diagram).Id == null && !newDiagrams.ContainsKey(diagram))
				throw new InvalidOperationException("Diagram not found in repository.");
			DoInsertShape(shape, diagram);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, diagram));
		}


		/// <override></override>
		public void InsertShape(Shape shape, Shape parentShape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parentShape == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			if (((IEntity)parentShape).Id != null && !shapes.ContainsKey(((IEntity)parentShape).Id)
				|| ((IEntity)parentShape).Id == null && !newShapes.ContainsKey(parentShape))
				throw new InvalidOperationException("Parent shape not found in repository.");
			DoInsertShape(shape, parentShape);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void InsertShape(Shape shape, Template owningTemplate) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (owningTemplate == null) throw new ArgumentNullException("owningTemplate");
			AssertOpen();
			if (owningTemplate.Id != null && !templates.ContainsKey(owningTemplate.Id)
				|| owningTemplate.Id == null && !newTemplates.ContainsKey(owningTemplate))
				throw new InvalidOperationException("Owning template not found in repository.");
			DoInsertShape(shape, owningTemplate);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void InsertShapes(IEnumerable<Shape> shapes, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			// ToDo: find a smarter way of inserting multiple objects
			foreach (Shape shape in shapes) DoInsertShape(shape, diagram);
			if (ShapesInserted != null) {
				ShapesInserted(this, GetShapesEventArgs(shapes, diagram));
			}
		}


		/// <override></override>
		public void InsertShapes(IEnumerable<Shape> shapes, Shape parentShape) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (parentShape == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			foreach (Shape shape in shapes) DoInsertShape(shape, parentShape);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, null));
		}


		/// <override></override>
		public void UpdateShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoUpdateShape(shape);
			if (ShapesUpdated != null) {
				RepositoryShapesEventArgs e = GetShapesEventArgs(shape);
				ShapesUpdated(this, e);
			}
		}


		/// <override></override>
		public void UpdateShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			// ToDo: find a smarter way of inserting multiple objects
			foreach (Shape shape in shapes) DoUpdateShape(shape);
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void UpdateShapeOwner(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoUpdateShapeOwner(shape, diagram);
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shape, diagram));
		}


		/// <override></override>
		public void UpdateShapeOwner(Shape shape, Shape parent) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parent == null) throw new ArgumentNullException("parent");
			AssertOpen();
			DoUpdateShapeOwner(shape, parent);
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void DeleteShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			RepositoryShapesEventArgs e = GetShapesEventArgs(shape);
			DoDeleteShape(shape);
			if (ShapesDeleted != null) ShapesDeleted(this, e);
		}


		/// <override></override>
		public void DeleteShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			// ToDo: find a smarter way of inserting multiple objects
			foreach (Shape shape in shapes) DoDeleteShape(shape);
			if (ShapesDeleted != null) ShapesDeleted(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void UndeleteShape(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoUndeleteShape(shape, diagram);
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					DoUndeleteShape(childShape, shape);
			}
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape));
		}


		/// <override></override>
		private void UndeleteShape(Shape shape, Template template) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoUndeleteShape(shape, template);
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					DoUndeleteShape(childShape, shape);
			}
		}


		/// <override></override>
		public void UndeleteShape(Shape shape, Shape parent) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parent == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			if (((IEntity)parent).Id != null && !shapes.ContainsKey(((IEntity)parent).Id)
				|| ((IEntity)parent).Id == null && !newShapes.ContainsKey(parent))
				throw new InvalidOperationException("Parent shape not found in repository.");
			DoUndeleteShape(shape, parent);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void UndeleteShapes(IEnumerable<Shape> shapes, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			// ToDo: find a smarter way of inserting multiple objects
			foreach (Shape shape in shapes)
				DoUndeleteShape(shape, diagram);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, diagram));
		}


		/// <override></override>
		public void UndeleteShapes(IEnumerable<Shape> shapes, Shape parent) {
			if (parent == null) throw new ArgumentNullException("parent");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			// ToDo: find a smarter way of inserting multiple objects
			foreach (Shape shape in shapes)
				DoUndeleteShape(shape, parent);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void UnloadDiagramShapes(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			DoUnloadShapes(diagram.Shapes);
		}


		/// <override></override>
		public event EventHandler<RepositoryShapesEventArgs> ShapesInserted;

		/// <override></override>
		public event EventHandler<RepositoryShapesEventArgs> ShapesUpdated;

		/// <override></override>
		public event EventHandler<RepositoryShapesEventArgs> ShapesDeleted;

		#endregion


		#region ShapeConnections

		/// <override></override>
		public void InsertShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			if (activeShape == null) throw new ArgumentNullException("activeShape");
			if (passiveShape == null) throw new ArgumentNullException("passiveShape");
			AssertOpen();
			ShapeConnection connection;
			connection.ConnectorShape = activeShape;
			connection.GluePointId = gluePointId;
			connection.TargetShape = passiveShape;
			connection.TargetPointId = connectionPointId;
			newShapeConnections.Add(connection);
			isModified = true;

			if (ShapeConnectionInserted != null) ShapeConnectionInserted(this, GetShapeConnectionEventArgs(connection));
		}


		/// <override></override>
		public void DeleteShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			if (activeShape == null) throw new ArgumentNullException("activeShape");
			if (passiveShape == null) throw new ArgumentNullException("passiveShape");
			AssertOpen();
			ShapeConnection connection;
			connection.ConnectorShape = activeShape;
			connection.GluePointId = gluePointId;
			connection.TargetShape = passiveShape;
			connection.TargetPointId = connectionPointId;
			// If the deleted connection is not a new connection, add it to the list of deleted connections
			if (!newShapeConnections.Remove(connection))
				deletedShapeConnections.Add(connection);
			isModified = true;

			if (ShapeConnectionDeleted != null) ShapeConnectionDeleted(this, GetShapeConnectionEventArgs(connection));
		}


		/// <override></override>
		public event EventHandler<RepositoryShapeConnectionEventArgs> ShapeConnectionInserted;

		/// <override></override>
		public event EventHandler<RepositoryShapeConnectionEventArgs> ShapeConnectionDeleted;

		#endregion


		#region ModelObjects

		private EntityBucket<IModelObject> GetModelObjectItem(object id) {
			if (id == null) throw new NShapeException("ModelObject has no identifier.");
			EntityBucket<IModelObject> item;
			if (!modelObjects.TryGetValue(id, out item))
				throw new NShapeException(string.Format("ModelObject {0} not found.", id));
			return item;
		}


		// TODO 2: Should be similar to GetShape. Unify?
		/// <override></override>
		public IModelObject GetModelObject(object id) {
			if (id == null) throw new ArgumentNullException("id");
			AssertOpen();
			IModelObject result = null;
			EntityBucket<IModelObject> moeb;
			if (modelObjects.TryGetValue(id, out moeb))
				result = moeb.ObjectRef;
			else {
				// Load ModelObjects and try again
				Model model = GetModel();
				if (store != null && model != null) store.LoadModelModelObjects(this, model.Id);
				if (modelObjects.TryGetValue(id, out moeb))
					result = moeb.ObjectRef;
			}
			if (result == null) throw new NShapeException("Model object with id '{0}' not found in repository.", id);
			return result;
		}


		/// <override></override>
		public IEnumerable<IModelObject> GetModelObjects(IModelObject parent) {
			AssertOpen();
			if (store != null && ((IEntity)settings).Id != null) {
				if (parent == null) {
					if (models.Count == 0)
						store.LoadModelModelObjects(this, GetModel().Id);
				} else if (parent.Id != null)
					store.LoadChildModelObjects(this, ((IEntity)parent).Id);
			}
			foreach (EntityBucket<IModelObject> mob in modelObjects) {
				if (mob.ObjectRef.Parent == parent) {
					if ((parent != null) || (parent == null && mob.Owner is Model))
						yield return mob.ObjectRef;
				}
			}
			foreach (KeyValuePair<IModelObject, IEntity> item in newModelObjects) {
				if (item.Key.Parent == parent) {
					if ((parent != null) || (parent == null && item.Value is Model))
						yield return item.Key;
				}
			}
		}


		/// <override></override>
		public void InsertModelObject(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoInsertModelObject(modelObject);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void InsertModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			// ToDo: find a smarter way of inserting multiple objects
			foreach (IModelObject modelObject in modelObjects)
				DoInsertModelObject(modelObject);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void UpdateModelObjectParent(IModelObject modelObject, IModelObject parent) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			DoUpdateModelObjectParent(modelObject, parent);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, GetModelObjectsEventArgs(modelObject));
		}


		/// <override></override>
		public void UpdateModelObject(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoUpdateModelObject(modelObject);
			//EntityBucket<IModelObject> item = GetModelObjectItem(newModelObject.Id);
			//item.State = ItemState.Modified;
			//isModified = true;
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, e);
		}


		/// <override></override>
		public void UpdateModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				DoUpdateModelObject(modelObject);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, e);
		}


		/// <override></override>
		public void DeleteModelObject(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoDeleteModelObject(modelObject);
			//EntityBucket<IModelObject> item = GetModelObjectItem(newModelObject.Id);
			//item.State = ItemState.Deleted;
			//isModified = true;
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		/// <override></override>
		public void DeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				DoDeleteModelObject(modelObject);
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		/// <override></override>
		public void UndeleteModelObject(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoUndeleteModelObject(modelObject);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void UndeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				DoUndeleteModelObject(modelObject);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void UnloadModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			foreach (IModelObject mo in modelObjects) {
				// TODO 2: Should we allow to remove from new model objects?
				if (mo.Id == null) newModelObjects.Remove(mo);
				else this.modelObjects.Remove(mo.Id);
			}
		}


		/// <override></override>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsInserted;

		/// <override></override>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsUpdated;

		/// <override></override>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		#endregion


		#region Designs

		/// <override></override>
		public IEnumerable<Design> GetDesigns() {
			AssertOpen();
			if (store != null && ((IEntity)settings).Id != null && designs.Count <= 0)
				store.LoadDesigns(this, null);
			return GetCachedEntities<Design>(designs, newDesigns);
		}


		/// <override></override>
		public Design GetDesign(object id) {
			Design result = null;
			AssertOpen();
			if (id == null) {
				// Return the project design
				result = projectDesign;
			} else {
				EntityBucket<Design> designBucket;
				if (store != null && designs.Count <= 0)
					store.LoadDesigns(this, null);
				if (!designs.TryGetValue(id, out designBucket))
					throw new NShapeException("Design with id '{0}' not found in repository.", id);
				result = designBucket.ObjectRef;
			}
			return result;
		}


		/// <override></override>
		public void InsertDesign(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			if (((IEntity)design).Id != null) throw new NShapeException("Can only insert new designs.");
			if (newDesigns.ContainsKey(design)) throw new NShapeException("Design already exists in the repository.");
			//
			DoInsertDesign(design, settings);
			if (DesignInserted != null) DesignInserted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void UpdateDesign(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			UpdateEntity<Design>(designs, newDesigns, design);
			if (DesignUpdated != null) DesignUpdated(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void DeleteDesign(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			// First, delete all styles
			foreach (IStyle s in design.Styles)
				DeleteEntity<IStyle>(styles, newStyles, s);
			DeleteEntity<Design>(designs, newDesigns, design);
			if (DesignDeleted != null) DesignDeleted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void UndeleteDesign(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			// First, delete all styles
			UndeleteEntity<Design>(designs, design);
			foreach (IStyle s in design.Styles)
				UndeleteEntity<IStyle>(styles, s);
			if (DesignInserted != null) DesignInserted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public event EventHandler<RepositoryDesignEventArgs> DesignInserted;

		/// <override></override>
		public event EventHandler<RepositoryDesignEventArgs> DesignUpdated;

		/// <override></override>
		public event EventHandler<RepositoryDesignEventArgs> DesignDeleted;

		#endregion


		#region Styles

		/// <override></override>
		public void InsertStyle(Design design, IStyle style) {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			InsertEntity<IStyle>(newStyles, style, design);
			if (StyleInserted != null) StyleInserted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void UpdateStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			UpdateEntity<IStyle>(styles, newStyles, style);
			if (StyleUpdated != null) StyleUpdated(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void DeleteStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			DeleteEntity<IStyle>(styles, newStyles, style);
			if (StyleDeleted != null) StyleDeleted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void UndeleteStyle(Design design, IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			UndeleteEntity<IStyle>(styles, style, design);
			if (StyleInserted != null) StyleInserted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public event EventHandler<RepositoryStyleEventArgs> StyleInserted;

		/// <override></override>
		public event EventHandler<RepositoryStyleEventArgs> StyleUpdated;

		/// <override></override>
		public event EventHandler<RepositoryStyleEventArgs> StyleDeleted;

		#endregion

		#endregion


		#region [Explicit] IStoreCache Members Implementation

		
		ProjectSettings IStoreCache.Project {
			get { return settings; }
		}


		void IStoreCache.SetRepositoryBaseVersion(int version) {
			this.version = version;
		}


		void IStoreCache.SetProjectOwnerId(object id) {
			projectOwner.Id = id;
		}


		object IStoreCache.ProjectId {
			get { return ((IEntity)settings).Id; }
		}


		string IStoreCache.ProjectName {
			get { return projectName; }
		}


		int IStoreCache.RepositoryBaseVersion {
			get { return version; }
		}


		Design IStoreCache.ProjectDesign {
			get { return projectDesign; }
		}


		IEnumerable<IEntityType> IStoreCache.EntityTypes {
			get { return entityTypes.Values; }
		}


		IEntityType IStoreCache.FindEntityTypeByElementName(string elementName) {
			if (!entityTypes.ContainsKey(elementName)) throw new ArgumentException(string.Format("An entity type with element name '{0}' is not registered.", elementName));
			return entityTypes[elementName];
		}


		IEntityType IStoreCache.FindEntityTypeByName(string name) {
			return FindEntityType(name, true);
		}


		string IStoreCache.CalculateElementName(string entityTypeName) {
			return CachedRepository.CalcElementName(entityTypeName);

		}


		ICacheCollection<ProjectSettings> IStoreCache.LoadedProjects {
			get { return projects; }
		}


		IEnumerable<KeyValuePair<ProjectSettings, IEntity>> IStoreCache.NewProjects {
			get { return newProjects; }
		}


		ICacheCollection<Model> IStoreCache.LoadedModels {
			get { return models; }
		}


		IEnumerable<KeyValuePair<Model, IEntity>> IStoreCache.NewModels {
			get { return newModels; }
		}


		ICacheCollection<Design> IStoreCache.LoadedDesigns {
			get { return designs; }
		}


		IEnumerable<KeyValuePair<Design, IEntity>> IStoreCache.NewDesigns {
			get { return newDesigns; }
		}


		ICacheCollection<Diagram> IStoreCache.LoadedDiagrams {
			get { return diagrams; }
		}


		IEnumerable<KeyValuePair<Diagram, IEntity>> IStoreCache.NewDiagrams {
			get { return newDiagrams; }
		}


		ICacheCollection<Shape> IStoreCache.LoadedShapes {
			get { return shapes; }
		}


		IEnumerable<KeyValuePair<Shape, IEntity>> IStoreCache.NewShapes {
			get { return newShapes; }
		}


		ICacheCollection<IStyle> IStoreCache.LoadedStyles {
			get { return styles; }
		}


		IEnumerable<KeyValuePair<IStyle, IEntity>> IStoreCache.NewStyles {
			get { return newStyles; }
		}


		ICacheCollection<Template> IStoreCache.LoadedTemplates {
			get { return templates; }
		}


		IEnumerable<KeyValuePair<Template, IEntity>> IStoreCache.NewTemplates {
			get { return newTemplates; }
		}


		ICacheCollection<IModelMapping> IStoreCache.LoadedModelMappings {
			get { return modelMappings; }
		}


		IEnumerable<KeyValuePair<IModelMapping, IEntity>> IStoreCache.NewModelMappings {
			get { return newModelMappings; }
		}


		ICacheCollection<IModelObject> IStoreCache.LoadedModelObjects {
			get { return modelObjects; }
		}


		IEnumerable<KeyValuePair<IModelObject, IEntity>> IStoreCache.NewModelObjects {
			get { return newModelObjects; }
		}


		IEnumerable<ShapeConnection> IStoreCache.NewShapeConnections {
			get { return newShapeConnections; }
		}


		IEnumerable<ShapeConnection> IStoreCache.DeletedShapeConnections {
			get { return deletedShapeConnections; }
		}


		IStyle IStoreCache.GetProjectStyle(object id) {
			return styles[id].ObjectRef;
		}


		Template IStoreCache.GetTemplate(object id) {
			return templates[id].ObjectRef;
		}


		Diagram IStoreCache.GetDiagram(object id) {
			return diagrams[id].ObjectRef;
		}


		Shape IStoreCache.GetShape(object id) {
			return shapes[id].ObjectRef;
		}


		IModelObject IStoreCache.GetModelObject(object id) {
			return modelObjects[id].ObjectRef;
		}


		Design IStoreCache.GetDesign(object id) {
			return designs[id].ObjectRef;
		}

		#endregion


		/// <summary>
		/// Calculates an XML tag name for the given entity name.
		/// </summary>
		static private string CalcElementName(string entityName) {
			string result;
			// We remove the prefixes Core. and GeneralShapes.
			if (entityName.StartsWith("Core.")) result = entityName.Substring(5);
			else if (entityName.StartsWith("GeneralShapes.")) result = entityName.Substring(14);
			else result = entityName;
			// ReplaceRange invalid characters
			result = result.Replace(' ', '_');
			result = result.Replace('/', '_');
			result = result.Replace('<', '_');
			result = result.Replace('>', '_');
			// We replace Camel casing with underscores
			stringBuilder.Length = 0;
			for (int i = 0; i < result.Length; ++i) {
				if (char.IsUpper(result[i])) {
					// Avoid multiple subsequent underscores
					if (i > 0 && stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '_')
						stringBuilder.Append('_');
					stringBuilder.Append(char.ToLowerInvariant(result[i]));
				} else stringBuilder.Append(result[i]);
			}
			// We use namespace prefixes for the library names
			// Not yet, must use prefix plus name in order to do that
			// result = result.ReplaceRange('.', ':');
			return stringBuilder.ToString();
		}


		/// <summary>
		/// Retrieves the indicated project style, which is always loaded when the project is open.
		/// </summary>
		private IStyle GetProjectStyle(object id) {
			EntityBucket<IStyle> styleItem;
			if (!styles.TryGetValue(id, out styleItem))
				throw new NShapeException("Style with id '{0}' does not exist.", id);
			return styleItem.ObjectRef;
		}


		/// <summary>
		/// Inserts an entity into the internal cache and marks it as new.
		/// </summary>
		private void InsertEntity<TEntity>(Dictionary<TEntity, IEntity> newEntities,
			TEntity entity, IEntity owner) where TEntity : IEntity {
			if (entity.Id != null)
				throw new ArgumentException("Entities with an id cannot be inserted into the repository.");
			newEntities.Add(entity, owner);
			isModified = true;
		}


		/// <summary>
		/// Updates an entity in the internal cache and marks it as modified.
		/// </summary>
		private void UpdateEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			Dictionary<TEntity, IEntity> newEntities, TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) {
				if (!newEntities.ContainsKey(entity))
					throw new NShapeException(string.Format("Entity not found in repository."));
			} else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new NShapeException("Entity not found in repository.");
				if (item.State == ItemState.Deleted) throw new NShapeException("Entity was deleted before. Undelete the entity before modifying it.");
				item.State = ItemState.Modified;
			}
			isModified = true;
		}


		/// <summary>
		/// Marks the entity for deletion from the data store. 
		/// Must be called after all children have been removed.
		/// </summary>
		private void DeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			Dictionary<TEntity, IEntity> newEntities, TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) {
				if (!newEntities.ContainsKey(entity))
					throw new NShapeException(string.Format("Entity not found in repository.", entity.Id));
				newEntities.Remove(entity);
			} else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new NShapeException("Entity not found in repository.");
				if (item.State == ItemState.Deleted) throw new NShapeException("Entity is already deleted.");
				item.State = ItemState.Deleted;
			}
			isModified = true;
		}


		private void UndeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) throw new NShapeException(string.Format("An entity without id cannot be undeleted.", entity.Id));
			else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new NShapeException("Entity not found in repository.");
				if (item.State != ItemState.Deleted) throw new NShapeException("Entity was not deleted before. Onlydeleted entities can be undeleted.");
				item.State = ItemState.Modified;
			}
			isModified = true;
		}


		private void UndeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			TEntity entity, IEntity owner) where TEntity : IEntity {
			if (entity.Id == null) throw new NShapeException(string.Format("An entity without id cannot be undeleted.", entity.Id));
			else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					loadedEntities.Add(entity.Id, new EntityBucket<TEntity>(entity, owner, ItemState.New));
				else {
					if (item.State != ItemState.Deleted) throw new NShapeException("Entity was not deleted before. Onlydeleted entities can be undeleted.");
					item.State = ItemState.Modified;
					Debug.Assert(item.Owner == owner);
				}
			}
			isModified = true;
		}


		private void AcceptEntities<EntityType>(Dictionary<object, EntityBucket<EntityType>> loadedEntities,
			Dictionary<EntityType, IEntity> newEntities) where EntityType : IEntity {
			// Remove deleted entities from loaded Entities
			List<object> deletedEntities = new List<object>(100);
			foreach (KeyValuePair<object, EntityBucket<EntityType>> ep in loadedEntities)
				if (ep.Value.State == ItemState.Deleted)
					deletedEntities.Add(ep.Key);
			foreach (object id in deletedEntities)
				loadedEntities.Remove(id);
			deletedEntities.Clear();
			deletedEntities = null;

			// Mark modified entities as original
			foreach (KeyValuePair<object, EntityBucket<EntityType>> item in loadedEntities) {
				if (item.Value.State != ItemState.Original) {
					Debug.Assert(loadedEntities[item.Key].State != ItemState.Deleted);
					loadedEntities[item.Key].State = ItemState.Original;
				}
			}

			// Move new entities from newEntities to loadedEntities
			foreach (KeyValuePair<EntityType, IEntity> ep in newEntities) {
				// Settings do not have a parent
				loadedEntities.Add(ep.Key.Id, new EntityBucket<EntityType>(ep.Key, ep.Value, ItemState.Original));
			}
			newEntities.Clear();
		}


		private void AcceptAll() {
			AcceptEntities<ProjectSettings>(projects, newProjects);
			AcceptEntities<Design>(designs, newDesigns);
			AcceptEntities<IStyle>(styles, newStyles);
			AcceptEntities<Template>(templates, newTemplates);
			AcceptEntities<IModelMapping>(modelMappings, newModelMappings);
			AcceptEntities<IModelObject>(modelObjects, newModelObjects);
			AcceptEntities<Model>(models, newModels);
			AcceptEntities<Diagram>(diagrams, newDiagrams);
			AcceptEntities<Shape>(shapes, newShapes);
			newShapeConnections.Clear();
			deletedShapeConnections.Clear();
			this.isModified = false;
		}


		/// <summary>
		/// Defines a dictionary for loaded entity types.
		/// </summary>
		private class LoadedEntities<TEntity> : Dictionary<object, EntityBucket<TEntity>>,
			ICacheCollection<TEntity> where TEntity : IEntity {

			#region ICacheCollection<TEntity> Members

			public bool Contains(object id) {
				return ContainsKey(id);
			}


			public TEntity GetEntity(object id) {
				return this[id].ObjectRef;
			}


			public void Add(EntityBucket<TEntity> bucket) {
				Add(((IEntity)bucket.ObjectRef).Id, bucket);
			}

			#endregion


			#region IEnumerable<TEntity> Members

			// ToDo 3: Find a way to avoid the new keyword
			public new IEnumerator<EntityBucket<TEntity>> GetEnumerator() {
				foreach (EntityBucket<TEntity> eb in Values)
					yield return eb;
			}

			#endregion

		}


		private void DoInsertDesign(Design design, ProjectSettings projectData) {
			InsertEntity<Design>(newDesigns, design, projectData);
			IStyleSet styleSet = (IStyleSet)design;
			foreach (IStyle style in styleSet.ColorStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			foreach (IStyle style in styleSet.CapStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			foreach (IStyle style in styleSet.FillStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			foreach (IStyle style in styleSet.CharacterStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			foreach (IStyle style in styleSet.LineStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			foreach (IStyle style in styleSet.ParagraphStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			isModified = true;
		}


		#region [Protected] ProjectOwner

		/// <summary>
		/// Serves as a parent entity for the project info.
		/// </summary>
		protected class ProjectOwner : IEntity {

			/// <override></override>
			public object Id;

			#region IEntity Members

			object IEntity.Id {
				get { return Id; }
			}

			void IEntity.AssignId(object id) {
				throw new NotImplementedException();
			}

			void IEntity.LoadFields(IRepositoryReader reader, int version) {
				throw new NotImplementedException();
			}

			void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
				throw new NotImplementedException();
			}

			void IEntity.SaveFields(IRepositoryWriter writer, int version) {
				throw new NotImplementedException();
			}

			void IEntity.SaveInnerObjects(string PropertyName, IRepositoryWriter writer, int version) {
				throw new NotImplementedException();
			}

			void IEntity.Delete(IRepositoryWriter writer, int version) {
				throw new NotImplementedException();
			}

			#endregion

		}

		#endregion


		#region [Private] Implementation

		private void ClearBuffers() {
			projectDesign = null;

			newProjects.Clear();
			newDesigns.Clear();
			newStyles.Clear();
			newDiagrams.Clear();
			newTemplates.Clear();
			newShapes.Clear();
			newModels.Clear();
			newModelObjects.Clear();
			newModelMappings.Clear();

			projects.Clear();
			styles.Clear();
			designs.Clear();
			diagrams.Clear();
			templates.Clear();
			shapes.Clear();
			models.Clear();
			modelObjects.Clear();
			modelMappings.Clear();
		}


		private IEntityType FindEntityType(string entityTypeName, bool mustExist) {
			IEntityType result;
			entityTypes.TryGetValue(CalcElementName(entityTypeName), out result);
			if (mustExist && result == null)
				throw new NShapeException("Entity type '{0}' does not exist in the repository.", entityTypeName);
			return result;
		}


		private IEnumerable<TEntity> GetCachedEntities<TEntity>(LoadedEntities<TEntity> loadedEntities,
			IDictionary<TEntity, IEntity> newEntities) where TEntity : IEntity {
			foreach (EntityBucket<TEntity> eb in loadedEntities)
				yield return eb.ObjectRef;
			foreach (KeyValuePair<TEntity, IEntity> item in newEntities)
				yield return item.Key;
		}


		private void AssertOpen() {
			if (!isOpen) throw new NShapeException("Repository is not open.");
			Debug.Assert(settings != null && projectDesign != null);
		}


		private void AssertClosed() {
			if (isOpen) throw new NShapeException("Repository is already open.");
		}


		private void AssertStoreExists() {
			if (store == null) throw new NShapeException("There is no store component connected to the repository.");
		}


		/// <summary>
		/// Tests the invariants of the offline cache object.
		/// </summary>
		private void AssertValid() {
			// project projectData are defined for an open project.
			Debug.Assert(!isOpen || GetProject() != null);
		}


		private bool CanUndelete<TEntity>(TEntity entity, LoadedEntities<TEntity> loadedEntities) where TEntity : IEntity {
			if (entity == null) throw new ArgumentNullException("entity");
			if (loadedEntities == null) throw new ArgumentNullException("loadedEntities");
			EntityBucket<TEntity> item = null;
			if (entity.Id != null && loadedEntities.TryGetValue(entity.Id, out item)) {
				return (item.State == ItemState.Deleted);
			} else return false;
		}


		private void DoInsertModelMapping(IModelMapping modelMapping, Template template) {
			InsertEntity<IModelMapping>(newModelMappings, modelMapping, template);
		}


		private void DoUpdateModelMapping(IModelMapping modelMapping) {
			UpdateEntity<IModelMapping>(modelMappings, newModelMappings, modelMapping);
		}


		private void DoDeleteModelMapping(IModelMapping modelMapping) {
			DeleteEntity<IModelMapping>(modelMappings, newModelMappings, modelMapping);
		}


		private void DoUndeleteModelMapping(IModelMapping modelMapping, Template owner) {
			UndeleteEntity<IModelMapping>(modelMappings, modelMapping, owner);
		}


		private Template GetModelMappingOwner(IModelMapping modelMapping) {
			Template owner = null;
			if (modelMapping.Id == null) {
				Debug.Assert(newModelMappings.ContainsKey(modelMapping));
				Debug.Assert(newModelMappings[modelMapping] is Template);
				owner = (Template)newModelMappings[modelMapping];
			} else {
				Debug.Assert(modelMappings[modelMapping.Id].Owner is Template);
				owner = (Template)modelMappings[modelMapping.Id].Owner;
			}
			return owner;
		}


		private void DoInsertShape(Shape shape, IEntity parentEntity) {
			InsertEntity<Shape>(newShapes, shape, parentEntity);
			foreach (Shape childShape in shape.Children.BottomUp)
				DoInsertShape(childShape, shape);
		}


		private void DoUpdateShape(Shape shape) {
			UpdateEntity<Shape>(shapes, newShapes, shape);
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children) {
					if ((((IEntity)childShape).Id != null && shapes.Contains(((IEntity)childShape).Id)) 
						|| newShapes.ContainsKey(childShape))
						DoUpdateShape(childShape);
					else DoInsertShape(childShape, shape);
				}
			}
		}


		private void DoUpdateShapeOwner(Shape shape, IEntity owner) {
			if (((IEntity)shape).Id == null) {
				newShapes.Remove(shape);
				newShapes.Add(shape, owner);
			} else {
				shapes[((IEntity)shape).Id].Owner = owner;
				shapes[((IEntity)shape).Id].State = ItemState.OwnerChanged;
			}
		}


		private void DoDeleteShape(Shape shape) {
			// Delete the shape's children
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					DoDeleteShape(childShape);
			}
			// Delete the shape's connections
			foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null)) {
				if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
					DeleteShapeConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
				else DeleteShapeConnection(sci.OtherShape, sci.OtherPointId, shape, sci.OwnPointId);
			}
			// Detach if a model object is assigned
			if (shape.ModelObject != null) {
				shape.ModelObject = null;
			}
			// Delete the shape itself
			DeleteEntity<Shape>(shapes, newShapes, shape);
		}


		private void DoUndeleteShape(Shape shape, IEntity parentEntity) {
			UndeleteEntity<Shape>(shapes, shape);
		}


		private void DoInsertModelObject(IModelObject modelObject) {
			IEntity owner;
			if (modelObject.Parent != null) owner = modelObject.Parent;
			else owner = GetModel();
			DoInsertModelObject(modelObject, owner);
		}


		private void InsertModelObject(IModelObject modelObject, Template template) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoInsertModelObject(modelObject, template);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		private void DoInsertModelObject(IModelObject modelObject, IEntity owner) {
			InsertEntity<IModelObject>(newModelObjects, modelObject, owner);
		}


		private void DoUpdateModelObjectParent(IModelObject modelObject, IModelObject parent) {
			if (((IEntity)modelObject).Id == null) {
				newModelObjects.Remove(modelObject);
				if (parent == null)
					newModelObjects.Add(modelObject, GetModel());
				else newModelObjects.Add(modelObject, parent);
			} else {
				if (parent == null)
					modelObjects[((IEntity)modelObject).Id].Owner = GetModel();
				else
					modelObjects[((IEntity)modelObject).Id].Owner = parent;
				modelObjects[((IEntity)modelObject).Id].State = ItemState.OwnerChanged;
			}
		}


		private void DoUpdateModelObject(IModelObject modelObject) {
			UpdateEntity<IModelObject>(modelObjects, newModelObjects, modelObject);
		}


		private void DoDeleteModelObject(IModelObject modelObject) {
			// ToDo: We must delete the newModelObject's connections first
			DeleteEntity<IModelObject>(modelObjects, newModelObjects, modelObject);
		}


		private void DoUndeleteModelObject(IModelObject modelObject) {
			// ToDo: We must delete the newModelObject's connections first
			UndeleteEntity<IModelObject>(modelObjects, modelObject);
		}


		private void DoUnloadShapes(IEnumerable<Shape> shapes) {
			// First unload the children then the parent.
			// TODO 2: Should we allow to remove from new shapes?
			foreach (Shape s in shapes) {
				DoUnloadShapes(s.Children);
				if (((IEntity)s).Id == null) newShapes.Remove(s);
				else this.shapes.Remove(((IEntity)s).Id);
			}
		}

		#endregion


		#region [Private] Methods for retrieving EventArgs

		private RepositoryProjectEventArgs GetProjectEventArgs(ProjectSettings projectData) {
			projectEventArgs.Project = projectData;
			return projectEventArgs;
		}


		private RepositoryModelEventArgs GetModelEventArgs(Model model) {
			modelEventArgs.Model = model;
			return modelEventArgs;
		}


		private RepositoryDesignEventArgs GetDesignEventArgs(Design design) {
			designEventArgs.Design = design;
			return designEventArgs;
		}


		private RepositoryStyleEventArgs GetStyleEventArgs(IStyle style) {
			styleEventArgs.Style = style;
			return styleEventArgs;
		}


		private RepositoryDiagramEventArgs GetDiagramEventArgs(Diagram diagram) {
			diagramEventArgs.Diagram = diagram;
			return diagramEventArgs;
		}


		private RepositoryTemplateEventArgs GetTemplateEventArgs(Template template) {
			templateEventArgs.Template = template;
			return templateEventArgs;
		}


		private RepositoryTemplateShapeReplacedEventArgs GetTemplateShapeExchangedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape) {
			templateShapeExchangedEventArgs.Template = template;
			templateShapeExchangedEventArgs.OldTemplateShape = oldTemplateShape;
			templateShapeExchangedEventArgs.NewTemplateShape = newTemplateShape;
			return templateShapeExchangedEventArgs;
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(Shape shape) {
			Diagram diagram;
			if (((IEntity)shape).Id == null)
				diagram = newShapes[shape] as Diagram;
			else diagram = shapes[((IEntity)shape).Id].Owner as Diagram;
			shapeEventArgs.SetShape(shape, diagram);
			return shapeEventArgs;
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(Shape shape, Diagram diagram) {
			shapeEventArgs.SetShape(shape, diagram);
			return shapeEventArgs;
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
			shapeEventArgs.SetShapes(shapes, diagram);
			return shapeEventArgs;
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes) {
			shapeEventArgs.Clear();
			foreach (Shape shape in shapes) {
				Diagram diagram = null;
				if (((IEntity)shape).Id == null) {
					if (newShapes.ContainsKey(shape))
						diagram = newShapes[shape] as Diagram;
				} else if (this.shapes.ContainsKey(((IEntity)shape).Id))
					diagram = this.shapes[((IEntity)shape).Id].Owner as Diagram;
				shapeEventArgs.AddShape(shape, diagram);
			}
			return shapeEventArgs;
		}


		private RepositoryModelObjectsEventArgs GetModelObjectsEventArgs(IModelObject modelObject) {
			modelObjectEventArgs.SetModelObject(modelObject);
			return modelObjectEventArgs;
		}


		private RepositoryModelObjectsEventArgs GetModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			modelObjectEventArgs.SetModelObjects(modelObjects);
			return modelObjectEventArgs;
		}


		private RepositoryShapeConnectionEventArgs GetShapeConnectionEventArgs(ShapeConnection connection) {
			shapeConnectionEventArgs.SetShapeConnection(connection);
			return shapeConnectionEventArgs;
		}

		#endregion


		#region Fields

		// Used to calculate the element entityTypeName
		static private StringBuilder stringBuilder = new StringBuilder();

		// project info is an internal entity type
		private const string projectInfoEntityTypeName = "ProjectInfo";

		/// <summary>
		/// True, when modfications have been done to any part of the projects since
		/// Open or SaveChanges. 
		/// </summary>
		private bool isModified;

		// True, when Open was successfully called. Is identical to store.IsOpen if store 
		// is defined.
		private bool isOpen;

		/// <summary>
		/// Reference to the open project for easier access.
		/// </summary>
		private ProjectSettings settings;

		/// <summary>
		/// Indicates the pseudo design used to manage the styles of the project.
		/// This design is not entered in the designs or newDesigns dictionaries.
		/// </summary>
		private Design projectDesign;

		private int version;

		// Name of the project
		private string projectName = string.Empty;

		// Store for cache data. Is null, if no store is assigned to open
		// cache, i.e. the cache is in-memory.
		private Store store;

		// project needs an owner for the newProjects dictionary
		private ProjectOwner projectOwner = new ProjectOwner();

		// DirectoryName of registered entities
		private Dictionary<string, IEntityType> entityTypes = new Dictionary<string, IEntityType>();

		// Containers for loaded objects
		private LoadedEntities<ProjectSettings> projects = new LoadedEntities<ProjectSettings>();
		private LoadedEntities<Model> models = new LoadedEntities<Model>();
		private LoadedEntities<Design> designs = new LoadedEntities<Design>();
		private LoadedEntities<IStyle> styles = new LoadedEntities<IStyle>();
		private LoadedEntities<Diagram> diagrams = new LoadedEntities<Diagram>();
		private LoadedEntities<Template> templates = new LoadedEntities<Template>();
		private LoadedEntities<IModelMapping> modelMappings = new LoadedEntities<IModelMapping>();
		private LoadedEntities<Shape> shapes = new LoadedEntities<Shape>();
		private LoadedEntities<IModelObject> modelObjects = new LoadedEntities<IModelObject>();
		private List<ShapeConnection> newShapeConnections = new List<ShapeConnection>();
		private List<ShapeConnection> deletedShapeConnections = new List<ShapeConnection>();

		// Containers for new entities
		// Stores the new entity as the key and its parent as the value.
		// (New objects do not yet have an id and are therefore not addressable in the dictionary.)
		private Dictionary<ProjectSettings, IEntity> newProjects = new Dictionary<ProjectSettings, IEntity>();
		private Dictionary<Model, IEntity> newModels = new Dictionary<Model, IEntity>();
		private Dictionary<Design, IEntity> newDesigns = new Dictionary<Design, IEntity>();
		private Dictionary<IStyle, IEntity> newStyles = new Dictionary<IStyle, IEntity>();
		private Dictionary<Diagram, IEntity> newDiagrams = new Dictionary<Diagram, IEntity>();
		private Dictionary<Template, IEntity> newTemplates = new Dictionary<Template, IEntity>();
		private Dictionary<IModelMapping, IEntity> newModelMappings = new Dictionary<IModelMapping, IEntity>();
		private Dictionary<Shape, IEntity> newShapes = new Dictionary<Shape, IEntity>();
		private Dictionary<IModelObject, IEntity> newModelObjects = new Dictionary<IModelObject, IEntity>();

		// EventArg Buffers
		private RepositoryProjectEventArgs projectEventArgs = new RepositoryProjectEventArgs();
		private RepositoryModelEventArgs modelEventArgs = new RepositoryModelEventArgs();
		private RepositoryDesignEventArgs designEventArgs = new RepositoryDesignEventArgs();
		private RepositoryStyleEventArgs styleEventArgs = new RepositoryStyleEventArgs();
		private RepositoryDiagramEventArgs diagramEventArgs = new RepositoryDiagramEventArgs();
		private RepositoryTemplateEventArgs templateEventArgs = new RepositoryTemplateEventArgs();
		private RepositoryTemplateShapeReplacedEventArgs templateShapeExchangedEventArgs = new RepositoryTemplateShapeReplacedEventArgs();
		private RepositoryShapesEventArgs shapeEventArgs = new RepositoryShapesEventArgs();
		private RepositoryShapeConnectionEventArgs shapeConnectionEventArgs = new RepositoryShapeConnectionEventArgs();
		private RepositoryModelObjectsEventArgs modelObjectEventArgs = new RepositoryModelObjectsEventArgs();

		#endregion
	}


	#region EntityBucket<TObject> Class

	/// <summary>
	/// Specifies the state of a persistent entity stored in a <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />
	/// </summary>
	public enum ItemState { 
		/// <summary>The entity was not modified.</summary>
		Original, 
		/// <summary>The entity was modified and not yet saved.</summary>
		Modified, 
		/// <summary>The owner of the entity changed.</summary>
		OwnerChanged, 
		/// <summary>The entity was deleted from the repository but not yet saved.</summary>
		Deleted, 
		/// <summary>The entity is new.</summary>
		New 
	};


	// EntityBucket is a reference type, because it is entered into dictionaries.
	// Modifying a field of a value type in a dictionary is not possible during
	// an enumeration, but we have to modify at least the State.
	/// <summary>
	/// Stores a reference to a loaded object together with its state.
	/// </summary>
	public class EntityBucket<TObject> {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.EntityBucket`1" />
		/// </summary>
		public EntityBucket(TObject obj, IEntity owner, ItemState state) {
			this.ObjectRef = obj;
			this.Owner = owner;
			this.State = state;
		}

		/// <summary>
		/// Gets the object stored in the <see cref="T:Dataweb.NShape.Advanced.EntityBucket`1" />
		/// </summary>
		public TObject ObjectRef;
		
		/// <summary>
		/// Gets the owner <see cref="T:Dataweb.NShape.Advanced.IEntity" />.
		/// </summary>
		public IEntity Owner;

		/// <summary>
		/// Gets the <see cref="T:Dataweb.NShape.Advanced.ItemState" />.
		/// </summary>
		public ItemState State;
	}

	#endregion


	#region ShapeConnection Struct

	/// <ToBeCompleted></ToBeCompleted>
	public struct ShapeConnection {

		/// <ToBeCompleted></ToBeCompleted>
		public static bool operator ==(ShapeConnection x, ShapeConnection y) {
			return (
				x.ConnectorShape == y.ConnectorShape
				&& x.TargetShape == y.TargetShape
				&& x.GluePointId == y.GluePointId
				&& x.TargetPointId == y.TargetPointId);
		}

		/// <ToBeCompleted></ToBeCompleted>
		public static bool operator !=(ShapeConnection x, ShapeConnection y) { return !(x == y); }

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeConnection" />.
		/// </summary>
		public ShapeConnection(Diagram diagram, Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId) {
			this.ConnectorShape = connectorShape;
			this.GluePointId = gluePointId;
			this.TargetShape = targetShape;
			this.TargetPointId = targetPointId;
		}

		/// <override></override>
		public override bool Equals(object obj) {
			return obj is ShapeConnection && this == (ShapeConnection)obj;
		}

		/// <override></override>
		public override int GetHashCode() {
			int result = GluePointId.GetHashCode() ^ TargetPointId.GetHashCode();
			if (ConnectorShape != null) result ^= ConnectorShape.GetHashCode();
			if (TargetShape != null) result ^= TargetShape.GetHashCode();
			return result;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public static readonly ShapeConnection Empty;

		/// <ToBeCompleted></ToBeCompleted>
		public Shape ConnectorShape;
		
		/// <ToBeCompleted></ToBeCompleted>
		public ControlPointId GluePointId;
		
		/// <ToBeCompleted></ToBeCompleted>
		public Shape TargetShape;
		
		/// <ToBeCompleted></ToBeCompleted>
		public ControlPointId TargetPointId;


		static ShapeConnection() {
			Empty.ConnectorShape = null;
			Empty.GluePointId = ControlPointId.None;
			Empty.TargetShape = null;
			Empty.TargetPointId = ControlPointId.None;
		}
	}

	#endregion


	#region Delegates

	/// <summary>
	/// Defines a filter function for the loading methods.
	/// </summary>
	public delegate bool FilterDelegate<TEntity>(TEntity entity, IEntity owner);


	/// <summary>
	/// Retrieves the entity with the given id.
	/// </summary>
	/// <param name="pid"></param>
	/// <returns></returns>
	public delegate IEntity Resolver(object pid);

	#endregion


	#region RepositoryReader Class

	/// <summary>
	/// Cache reader for the cached cache.
	/// </summary>
	public abstract class RepositoryReader : IRepositoryReader, IDisposable {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.RepositoryReader" />.
		/// </summary>
		protected RepositoryReader(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			this.cache = cache;
		}


		#region [Public] IRepositoryReader Members

		/// <summary>
		/// Fetches the next set of inner objects and prepares them for reading.
		/// </summary>
		public abstract void BeginReadInnerObjects();


		/// <summary>
		/// Finishes reading the current set of inner objects.
		/// </summary>
		public abstract void EndReadInnerObjects();


		/// <summary>
		/// Fetches the next inner object in a set of inner object.
		/// </summary>
		public bool BeginReadInnerObject() {
			if (innerObjectsReader == null)
				return DoBeginObject();
			else return innerObjectsReader.BeginReadInnerObject();
		}


		/// <summary>
		/// Finishes reading an inner object.
		/// </summary>
		public abstract void EndReadInnerObject();


		/// <summary>
		/// Reads a boolean value from the data source.
		/// </summary>
		public bool ReadBool() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadBool();
			} else return innerObjectsReader.ReadBool();
		}


		/// <summary>
		/// Reads a byte value from the data source.
		/// </summary>
		public byte ReadByte() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadByte();
			} else return innerObjectsReader.ReadByte();
		}


		/// <summary>
		/// Reads a 16 bit integer value from the data source.
		/// </summary>
		/// <returns></returns>
		public short ReadInt16() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt16();
			} else return innerObjectsReader.ReadInt16();
		}


		/// <summary>
		/// Reads a 32 bit integer value from the data source.
		/// </summary>
		/// <returns></returns>
		public int ReadInt32() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt32();
			} else return innerObjectsReader.ReadInt32();
		}


		/// <summary>
		/// Reads a 64 bit integer value from the data source.
		/// </summary>
		/// <returns></returns>
		public long ReadInt64() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt64();
			} else return innerObjectsReader.ReadInt64();
		}


		/// <summary>
		/// Reads a single precision floating point number from the data source.
		/// </summary>
		/// <returns></returns>
		public float ReadFloat() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadFloat();
			} else return innerObjectsReader.ReadFloat();
		}


		/// <summary>
		/// Reads a double precision floating point number from the data source.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadDouble();
			} else return innerObjectsReader.ReadDouble();
		}


		/// <summary>
		/// Reads a character value.
		/// </summary>
		public char ReadChar() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadChar();
			} else return innerObjectsReader.ReadChar();
		}


		/// <summary>
		/// Reads a string value from the data source.
		/// </summary>
		public string ReadString() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadString();
			} else return innerObjectsReader.ReadString();
		}


		/// <summary>
		/// Reads a date and time value from the data source.
		/// </summary>
		public DateTime ReadDate() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadDate();
			} else return innerObjectsReader.ReadDate();
		}


		/// <summary>
		/// Reads an image value from the data source.
		/// </summary>
		public System.Drawing.Image ReadImage() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadImage();
			} else return innerObjectsReader.ReadImage();
		}


		/// <summary>
		/// Reads a template from the data source.
		/// </summary>
		public Template ReadTemplate() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetTemplate(id);
		}


		/// <summary>
		/// Reads a shape from the data source.
		/// </summary>
		/// <returns></returns>
		public Shape ReadShape() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetShape(id);
		}


		/// <summary>
		/// Reads a model object from the data source.
		/// </summary>
		public IModelObject ReadModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetModelObject(id);
		}


		/// <summary>
		/// Reads a design from the data source.
		/// </summary>
		public Design ReadDesign() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDesign(id);
		}


		/// <summary>
		/// Reads a cap style from the data source.
		/// </summary>
		public ICapStyle ReadCapStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (ICapStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadCapStyle();
		}


		/// <summary>
		/// Reads a character style from the data source.
		/// </summary>
		public ICharacterStyle ReadCharacterStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (ICharacterStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadCharacterStyle();
		}


		/// <summary>
		/// Reads a color style from the data source.
		/// </summary>
		public IColorStyle ReadColorStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (IColorStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadColorStyle();
		}


		/// <summary>
		/// Reads a fill style from the data source.
		/// </summary>
		public IFillStyle ReadFillStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (IFillStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadFillStyle();
		}


		/// <summary>
		/// Reads a line style from the data source.
		/// </summary>
		public ILineStyle ReadLineStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			IStyle style = cache.GetProjectStyle(id);
			Debug.Assert(style is ILineStyle, string.Format("Style {0} is not a line style.", id));
			return (ILineStyle)style;
			} else return innerObjectsReader.ReadLineStyle();
		}


		/// <summary>
		/// Reads a paragraph stylefrom the data source.
		/// </summary>
		public IParagraphStyle ReadParagraphStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (IParagraphStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadParagraphStyle();
		}

		#endregion


		#region [Public] IDisposable Members

		/// <summary>
		/// Releases all allocated unmanaged or persistent resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// Indicates the current index in the list of property info of the entity type.
		/// </summary>
		protected internal int PropertyIndex {
			get { return propertyIndex; }
			set { propertyIndex = value; }
		}


		/// <summary>
		/// The IStoreCache that contains the data to read.
		/// </summary>
		protected IStoreCache Cache {
			get { return cache; }
		}


		/// <summary>
		/// A read only collection of property info of the entity type to read.
		/// </summary>
		protected IEnumerable<EntityPropertyDefinition> PropertyInfos {
			get { return propertyInfos; }
		}


		/// <summary>
		/// When reading inner objects, this property stores the owner entity of the inner objects. Otherwise, this property is null/Nothing.
		/// </summary>
		protected IEntity Object {
			get { return entity; }
			set { entity = value; }
		}

		#endregion


		#region [Protected] Methods: Implementation

		/// <summary>
		/// Implementation of reading an id value. Reads an id or null, if no id exists.
		/// </summary>
		protected internal abstract object ReadId();


		/// <summary>
		/// Resets the repositoryReader for a sequence of reads of entities of the same type.
		/// </summary>
		internal virtual void ResetFieldReading(IEnumerable<EntityPropertyDefinition> propertyInfos) {
			if (propertyInfos == null) throw new ArgumentNullException("propertyInfos");
			this.propertyInfos.Clear();
			this.propertyInfos.AddRange(propertyInfos);
			propertyIndex = int.MinValue;
		}


		/// <summary>
		/// Advances to the next object and prepares reading it.
		/// </summary>
		protected internal abstract bool DoBeginObject();


		/// <summary>
		/// Finishes reading an object.
		/// </summary>
		protected internal abstract void DoEndObject();


		/// <summary>
		/// Implementation of reading a boolean value.
		/// </summary>
		protected abstract bool DoReadBool();


		/// <summary>
		/// Implementation of reading a byte value.
		/// </summary>
		protected abstract byte DoReadByte();


		/// <summary>
		/// Implementation of reading a 16 bit integer number.
		/// </summary>
		protected abstract short DoReadInt16();


		/// <summary>
		/// Implementation of reading a 32 bit integer number.
		/// </summary>
		protected abstract int DoReadInt32();


		/// <summary>
		/// Implementation of reading a 64 bit integer number.
		/// </summary>
		protected abstract long DoReadInt64();


		/// <summary>
		/// Implementation of reading a single precision floating point number.
		/// </summary>
		protected abstract float DoReadFloat();


		/// <summary>
		/// Implementation of reading a double precision floating point number.
		/// </summary>
		protected abstract double DoReadDouble();


		/// <summary>
		/// Implementation of reading a character value.
		/// </summary>
		protected abstract char DoReadChar();


		/// <summary>
		/// Implementation of reading a string value.
		/// </summary>
		/// <returns></returns>
		protected abstract string DoReadString();


		/// <summary>
		/// Implementation of reading a date and time value.
		/// </summary>
		protected abstract DateTime DoReadDate();


		/// <summary>
		/// Implementation of reading an image.
		/// </summary>
		protected abstract System.Drawing.Image DoReadImage();


		/// <summary>
		/// Implementation of reading a template.
		/// </summary>
		protected Template DoReadTemplate() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetTemplate(id);
		}


		/// <summary>
		/// Implementation of reading a shape.
		/// </summary>
		protected Shape DoReadShape() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetShape(id);
		}


		/// <summary>
		/// Implementation of reading a model object.
		/// </summary>
		protected IModelObject DoReadModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetModelObject(id);
		}


		/// <summary>
		/// Implementation of reading a design.
		/// </summary>
		protected Design DoReadDesign() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDesign(id);
		}


		/// <summary>
		/// Implementation of reading a cap style.
		/// </summary>
		protected ICapStyle DoReadCapStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ICapStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a character style.
		/// </summary>
		protected ICharacterStyle DoReadCharacterStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ICharacterStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a color style.
		/// </summary>
		protected IColorStyle DoReadColorStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IColorStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a fill style.
		/// </summary>
		protected IFillStyle DoReadFillStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IFillStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a line style.
		/// </summary>
		protected ILineStyle DoReadLineStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ILineStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a paragraph style.
		/// </summary>
		protected IParagraphStyle DoReadParagraphStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IParagraphStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Checks whether the current property index refers to a valid entity field.
		/// </summary>
		protected virtual void ValidatePropertyIndex() {
			// We cannot check propertyIndex < 0 because some readers use PropertyIndex == -1 for the id.
			if (propertyIndex >= propertyInfos.Count)
				throw new NShapeException("An entity tries to read more properties from the repository than there are defined.");
		}


		/// <override></override>
		protected virtual void Dispose(bool disposing) {
			// Nothing to do
		}

		#endregion


		#region Fields

		/// <summary>
		/// A list of property info of the entity type to read.
		/// </summary>
		protected List<EntityPropertyDefinition> propertyInfos = new List<EntityPropertyDefinition>(20);
		
		/// <summary>
		/// When reading inner objects, this field holds the reader used for reading these inner objects.
		/// </summary>
		protected RepositoryReader innerObjectsReader;

		private IStoreCache cache;
		private int propertyIndex;
		// used for loading innerObjects
		private IEntity entity;

		#endregion
	}

	#endregion


	#region RepositoryWriter Class

	/// <summary>
	/// Offline RepositoryWriter
	/// </summary>
	public abstract class RepositoryWriter : IRepositoryWriter {

		/// <summary>
		/// Initializes a new Iinstance of RepositoryWriter
		/// </summary>
		protected RepositoryWriter(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			this.cache = cache;
		}


		#region [Public] IRepositoryWriter Members

		/// <summary>
		/// Fetches the next inner object in a set of inner object.
		/// </summary>
		public void BeginWriteInnerObject() {
			// Must be executed by the outer writer. Currently there is only one inner 
			// and one outer.
			DoBeginWriteInnerObject();
		}


		/// <summary>
		/// Fetches the next set of inner objects and prepares them for writing.
		/// </summary>
		public void BeginWriteInnerObjects() {
			if (innerObjectsWriter != null)
				throw new InvalidOperationException("Call EndWriteInnerObjects before a new call to BeginWriteInnerObjects.");
			DoBeginWriteInnerObjects();
		}


		/// <summary>
		/// Finishes writing an inner object.
		/// </summary>
		public void EndWriteInnerObject() {
			// Must be executed by the outer writer. Currently there is only one inner 
			// and one outer.
			DoEndWriteInnerObject();
		}


		/// <summary>
		/// Finishes writing the current set of inner objects.
		/// </summary>
		public void EndWriteInnerObjects() {
			if (innerObjectsWriter == null)
				throw new InvalidOperationException("BeginWriteInnerObjects has not been called.");
			DoEndWriteInnerObjects();
			innerObjectsWriter = null;
		}


		/// <summary>
		/// Deletes the current set of inner objects.
		/// </summary>
		public void DeleteInnerObjects() {
			BeginWriteInnerObjects();
			EndWriteInnerObjects();
		}

		
		/// <summary>
		/// Writes an IEntity.Id value.
		/// </summary>
		/// <param name="id"></param>
		public void WriteId(object id) {
			if (innerObjectsWriter == null) DoWriteId(id);
			else innerObjectsWriter.WriteId(id);
		}


		/// <summary>
		/// Writes a boolean value.
		/// </summary>
		public void WriteBool(bool value) {
			if (innerObjectsWriter == null) DoWriteBool(value);
			else innerObjectsWriter.WriteBool(value);
		}


		/// <summary>
		/// Writes a byte value.
		/// </summary>
		public void WriteByte(byte value) {
			if (innerObjectsWriter == null) DoWriteByte(value);
			else innerObjectsWriter.WriteByte(value);
		}


		/// <summary>
		/// Writes a 16 bit integer value.
		/// </summary>
		public void WriteInt16(short value) {
			if (innerObjectsWriter == null) DoWriteInt16(value);
			else innerObjectsWriter.WriteInt16(value);
		}


		/// <summary>
		/// Writes a 32 bit integer value.
		/// </summary>
		public void WriteInt32(int value) {
			if (innerObjectsWriter == null) DoWriteInt32(value);
			else innerObjectsWriter.WriteInt32(value);
		}


		/// <summary>
		/// Writes a 64 bit integer value.
		/// </summary>
		public void WriteInt64(long value) {
			if (innerObjectsWriter == null) DoWriteInt64(value);
			else innerObjectsWriter.WriteInt64(value);
		}


		/// <summary>
		/// Writes a single precision floating point number.
		/// </summary>
		public void WriteFloat(float value) {
			if (innerObjectsWriter == null) DoWriteFloat(value);
			else innerObjectsWriter.WriteFloat(value);
		}


		/// <summary>
		/// Writes a double precision floating point number.
		/// </summary>
		public void WriteDouble(double value) {
			if (innerObjectsWriter == null) DoWriteDouble(value);
			else innerObjectsWriter.WriteDouble(value);
		}


		/// <summary>
		/// Writes a character value.
		/// </summary>
		public void WriteChar(char value) {
			if (innerObjectsWriter == null) DoWriteChar(value);
			else innerObjectsWriter.WriteChar(value);
		}


		/// <summary>
		/// Writes a string value.
		/// </summary>
		public void WriteString(string value) {
			if (innerObjectsWriter == null) DoWriteString(value);
			else innerObjectsWriter.WriteString(value);
		}


		/// <summary>
		/// Writes a date and time value.
		/// </summary>
		public void WriteDate(DateTime value) {
			if (innerObjectsWriter == null) DoWriteDate(value);
			else innerObjectsWriter.WriteDate(value);
		}


		/// <summary>
		/// Writes an image value.
		/// </summary>
		public void WriteImage(System.Drawing.Image image) {
			if (innerObjectsWriter == null) DoWriteImage(image);
			else innerObjectsWriter.WriteImage(image);
		}


		/// <summary>
		/// Writes a template.
		/// </summary>
		public void WriteTemplate(Template template) {
			if (template != null && template.Id == null) throw new InvalidOperationException(
				string.Format("Template '{0}' is not registered with repository.", template.Name));
			if (innerObjectsWriter == null) {
				if (template == null) WriteId(null);
				else WriteId(template.Id);
			} else innerObjectsWriter.WriteTemplate(template);
		}


		/// <summary>
		/// Writes a style.
		/// </summary>
		public void WriteStyle(IStyle style) {
			if (style != null && style.Id == null) throw new InvalidOperationException(
				 string.Format("{0} '{1}' is not registered with the repository.", style.GetType().Name, style.Name));
			if (innerObjectsWriter == null) {
				if (style == null) WriteId(null);
				else WriteId(style.Id);
			} else innerObjectsWriter.WriteStyle(style);
		}


		/// <summary>
		/// Writes a model object.
		/// </summary>
		public void WriteModelObject(IModelObject modelObject) {
			if (modelObject != null && modelObject.Id == null) 
				throw new InvalidOperationException(
				 string.Format("{0} '{1}' is not registered with the repository.", 
				 modelObject.Type.FullName, modelObject.Name));
			if (innerObjectsWriter == null) {
				Debug.Assert(modelObject == null || modelObject.Id != null);
				if (modelObject == null) WriteId(null);
				else WriteId(modelObject.Id);
			} else innerObjectsWriter.WriteModelObject(modelObject);
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// Indicates the current index in the list of property info of the entity type.
		/// </summary>
		protected internal int PropertyIndex {
			get { return propertyIndex; }
			set { propertyIndex = value; }
		}


		/// <summary>
		/// When reading inner objects, this property stores the owner entity of the inner objects. Otherwise, this property is null/Nothing.
		/// </summary>
		protected IEntity Entity {
			get { return entity; }
		}


		/// <summary>
		/// The IStoreCache that contains the data to read.
		/// </summary>
		protected IStoreCache Cache {
			get { return cache; }
		}

		#endregion


		#region [Protected] Methods: Implementation

		/// <summary>
		/// Implementation of writing an IEntity.Id value.
		/// </summary>
		protected abstract void DoWriteId(object id);

		/// <summary>
		/// Implementation of writing a boolean value.
		/// </summary>
		protected abstract void DoWriteBool(bool value);

		/// <summary>
		/// Implementation of writing a byte value.
		/// </summary>
		protected abstract void DoWriteByte(byte value);

		/// <summary>
		/// Implementation of writing a 16 bit integer number.
		/// </summary>
		protected abstract void DoWriteInt16(short value);

		/// <summary>
		/// Implementation of writing a 32 bit integer number.
		/// </summary>
		protected abstract void DoWriteInt32(int value);

		/// <summary>
		/// Implementation of writing a 64 bit integer number.
		/// </summary>
		protected abstract void DoWriteInt64(long value);

		/// <summary>
		/// Implementation of writing a single precision floating point number.
		/// </summary>
		protected abstract void DoWriteFloat(float value);

		/// <summary>
		/// Implementation of writing a double precision floating point number.
		/// </summary>
		protected abstract void DoWriteDouble(double value);

		/// <summary>
		/// Implementation of writing a character value.
		/// </summary>
		protected abstract void DoWriteChar(char value);

		/// <summary>
		/// Implementation of writing a string value.
		/// </summary>
		protected abstract void DoWriteString(string value);

		/// <summary>
		/// Implementation of writing a date value.
		/// </summary>
		protected abstract void DoWriteDate(DateTime date);

		/// <summary>
		/// Implementation of writing an image.
		/// </summary>
		protected abstract void DoWriteImage(System.Drawing.Image image);

		/// <summary>
		/// Implementation of BeginWriteInnerObjects.
		/// </summary>
		protected abstract void DoBeginWriteInnerObjects();

		/// <summary>
		/// Implementation of EndWriteInnerObjects.
		/// </summary>
		protected abstract void DoEndWriteInnerObjects();

		// Must be called upon the outer cache writer.
		/// <summary>
		/// Implementation of BeginWriteInnerObject.
		/// </summary>
		protected abstract void DoBeginWriteInnerObject();

		// Must be called upon the outer cache writer.
		/// <summary>
		/// Implementation of EndWriteInnerObject.
		/// </summary>
		protected abstract void DoEndWriteInnerObject();

		/// <summary>
		/// Implementation of DeleteInnerObjects.
		/// </summary>
		protected abstract void DoDeleteInnerObjects();


		/// <summary>
		/// Reinitializes the writer to work with given property infos.
		/// </summary>
		protected internal virtual void Reset(IEnumerable<EntityPropertyDefinition> propertyInfos) {
			if (propertyInfos == null) throw new ArgumentNullException("propertyInfos");
			this.propertyInfos.Clear();
			this.propertyInfos.AddRange(propertyInfos);
		}


		/// <summary>
		/// Specifies the entity to write next. Is null when going to write an inner object.
		/// </summary>
		/// <param name="entity"></param>
		protected internal virtual void Prepare(IEntity entity) {
			this.entity = entity;
			// The first property is the internally written id.
			PropertyIndex = -2;
		}


		/// <summary>
		/// Commits inner object data to the data store.
		/// </summary>
		protected internal virtual void Finish() {
			// Nothing to do
		}

		#endregion


		#region Fields

		// When writing inner objects, reference to the responsible writer
		/// <summary>
		/// When reading inner objects, this field holds the reader used for reading these inner objects.
		/// </summary>
		protected RepositoryWriter innerObjectsWriter;

		// Description of the entity type currently writting
		/// <summary>
		/// A list of <see cref="T:Dataweb.NShape.Advanced.EntityPropertyDefinition" /> for the entity type.
		/// </summary>
		protected List<EntityPropertyDefinition> propertyInfos = new List<EntityPropertyDefinition>(20);

		private IStoreCache cache;
		// Current entity to write. Null when writing an inner object
		private IEntity entity;
		// Index of property currently being written
		private int propertyIndex;
		#endregion
	}

	#endregion

}