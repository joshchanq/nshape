using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;


namespace Dataweb.Diagramming.Advanced {

	#region IStoreCache Interface

	/// <summary>
	/// Provides access to diagramming entities of one type for stores.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public interface ICacheCollection<TEntity>: IEnumerable<EntityBucket<TEntity>> where TEntity: IEntity {

		bool Contains(object id);

		TEntity GetEntity(object id);

		EntityBucket<TEntity> this[object id] { get; }

		void Add(EntityBucket<TEntity> bucket);

	}


	/// <summary>
	/// Provides access to diagramming entities for stores.
	/// </summary>
	public interface IStoreCache {

		object ProjectId { get; }

		string ProjectName { get; }

		/// <summary>
		/// Indicates the repository version of the core libraries.
		/// </summary>
		int RepositoryBaseVersion { get; }

		/// <summary>
		/// Sets the repository version of the core libraries from a loading project.
		/// </summary>
		/// <param name="version"></param>
		void SetRepositoryBaseVersion(int version);

		ProjectSettings Project { get; }

		void SetProjectOwnerId(object id);

		Design ProjectDesign { get; }

		//---------

		IEnumerable<IEntityType> EntityTypes { get; }

		IEntityType FindEntityTypeByName(string entityTypeName);

		IEntityType FindEntityTypeByElementName(string elementName);

		string CalculateElementName(string entityTypeName);

		//---------

		IStyle GetProjectStyle(object id);

		Model GetModel();
		
		Template GetTemplate(object id);

		Diagram GetDiagram(object id);

		Shape GetShape(object id);

		IModelObject GetModelObject(object id);

		Design GetDesign(object id);

		//---------

		ICacheCollection<Diagram> LoadedDiagrams { get; }

		IEnumerable<KeyValuePair<Diagram, IEntity>> NewDiagrams { get; }

		ICacheCollection<Shape> LoadedShapes { get; }

		IEnumerable<KeyValuePair<Shape, IEntity>> NewShapes { get; }

		ICacheCollection<ProjectSettings> LoadedProjects { get; }

		IEnumerable<KeyValuePair<ProjectSettings, IEntity>> NewProjects { get; }

		ICacheCollection<Model> LoadedModels { get; }

		IEnumerable<KeyValuePair<Model, IEntity>> NewModels { get; }

		ICacheCollection<Design> LoadedDesigns { get; }

		IEnumerable<KeyValuePair<Design, IEntity>> NewDesigns { get; }

		ICacheCollection<IStyle> LoadedStyles { get; }

		IEnumerable<KeyValuePair<IStyle, IEntity>> NewStyles { get; }

		ICacheCollection<Template> LoadedTemplates { get; }

		ICacheCollection<IModelMapping> LoadedModelMappings { get; }

		IEnumerable<KeyValuePair<Template, IEntity>> NewTemplates { get; }

		IEnumerable<KeyValuePair<IModelMapping, IEntity>> NewModelMappings { get; }

		IEnumerable<ShapeConnection> NewShapeConnections { get; }

		IEnumerable<ShapeConnection> DeletedShapeConnections { get; }

		ICacheCollection<IModelObject> LoadedModelObjects { get; }

		IEnumerable<KeyValuePair<IModelObject, IEntity>> NewModelObjects { get; }

	}
	#endregion


	#region Store Class

	public delegate bool IdFilter(object id);


	/// <summary>
	/// Stores cache data persistently in a data source.
	/// </summary>
	public abstract class Store: Component {

		/// <summary>
		/// Specifies the main version of the storage format.
		/// </summary>
		public abstract int Version { get; set; }

		/// <summary>
		/// Specifies the name of the project.
		/// </summary>
		public abstract string ProjectName { get; set; }

		/// <summary>
		/// Tests whether the project already exists in the data source.
		/// </summary>
		/// <returns></returns>
		public abstract bool Exists();

		/// <summary>
		/// Creates a project store in the data source.
		/// </summary>
		/// <param name="storeCache"></param>
		public abstract void Create(IStoreCache storeCache);

		/// <summary>
		/// Opens a project store in the data source.
		/// </summary>
		/// <param name="storeCache"></param>
		public abstract void Open(IStoreCache storeCache);

		/// <summary>
		/// Closes the project store.
		/// </summary>
		/// <param name="storeCache"></param>
		public virtual void Close(IStoreCache storeCache) {
			if (storeCache == null) throw new ArgumentNullException("storeCache");
			// Nothing to do yet.
		}

		/// <summary>
		/// Deletes the project store in the data source.
		/// </summary>
		public abstract void Erase();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="entityType"></param>
		/// <param name="parameters"></param>
		public abstract void LoadProjects(IStoreCache cache, IEntityType entityType, params object[] parameters);

		public abstract void LoadModel(IStoreCache cache, object projectId);		
		
		/// <summary>
		/// Loads general designs or a project design.
		/// </summary>
		/// <param name="cache">Store cache to load to.</param>
		/// <param name="projectId">Project id for project design, null for general designs.</param>
		public abstract void LoadDesigns(IStoreCache cache, object projectId);

		public abstract void LoadTemplates(IStoreCache cache, object projectId);

		public abstract void LoadDiagrams(IStoreCache cache, object projectId);

		public abstract void LoadDiagramShapes(IStoreCache cache, Diagram diagram);

		public abstract void LoadTemplateShapes(IStoreCache cache, object templateId);

		public abstract void LoadChildShapes(IStoreCache cache, object parentShapeId);

		public abstract void LoadTemplateModelObjects(IStoreCache cache, object templateId);
		
		public abstract void LoadModelModelObjects(IStoreCache cache, object modelId);
		
		public abstract void LoadChildModelObjects(IStoreCache cache, object parentModelObjectId);

		/// <summary>
		/// Commits all modifications in the cache to the data store.
		/// </summary>
		public abstract void SaveChanges(IStoreCache storeCache);

	}

	#endregion

}
