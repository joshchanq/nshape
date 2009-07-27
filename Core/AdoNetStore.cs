using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

using Dataweb.Diagramming.Advanced;
using System.Globalization;


namespace Dataweb.Diagramming {

	/// <summary>
	/// Defines the operation on an entity.
	/// </summary>
	public enum RepositoryCommandType {
		/// <summary>Inserts one entity of the given type.</summary>
		Insert,
		/// <summary>Inserts a shape of a diagram.</summary>
		InsertDiagramShape,
		/// <summary>Inserts the shape of a template.</summary>
		InsertTemplateShape,
		/// <summary>Inserts a child shape of a parent shape.</summary>
		InsertChildShape,
		/// <summary>Inserts the model object of a template.</summary>
		InsertTemplateModelObject,
		/// <summary>Inserts a model object or a child model object of a parent model object.</summary>
		InsertModelModelObject,
		/// <summary>Inserts a model object or a child model object of a parent model object.</summary>
		InsertChildModelObject,
		/// <summary>Updates an entity of the given type.</summary>
		Update,
		/// <summary>Sets the owner of a shape to the given diagram.</summary>
		UpdateOwnerDiagram,
		/// <summary>Sets the owner of a shape to the given parent shape.</summary>
		UpdateOwnerShape,
		/// <summary>Sets the owner of a model object to the given model.</summary>
		UpdateOwnerModel,
		/// <summary>Sets the owner of a model object to the given model object.</summary>
		UpdateOwnerModelObject,
		/// <summary>Deletes an entitiy of the given type identified by its id.</summary>
		Delete,
		/// <summary>Selects all entities of a given type.</summary>
		SelectAll,
		/// <summary>Selects the entity of a given type with the indicated id.</summary>
		SelectById,
		/// <summary>?</summary>
		SelectByName,
		/// <summary>Selects all entities of a given type that have the indicated owner.</summary>
		SelectByOwnerId,
		/// <summary>Selects shapes of a given diagram.</summary>
		SelectDiagramShapes,
		/// <summary>Selects the shape for a given template.</summary>
		SelectTemplateShapes,
		/// <summary>Selects shapes with a given parent shape.</summary>
		SelectChildShapes,
		/// <summary>Selects the model objects for a given template.</summary>
		SelectTemplateModelObjects,
		/// <summary>Selects model objects that have no parent model object.</summary>
		SelectModelModelObjects,
		/// <summary>Selects model objects with a given parent model obejct.</summary>
		SelectChildModelObjects
	}


	public class AdoNetStoreException : DiagrammingException {

		internal protected AdoNetStoreException(string message) : base(message) { }

		internal protected AdoNetStoreException(string format, params object[] args) : base(format, args) { }

		internal protected AdoNetStoreException(string format, Exception innerException, params object[] args) : base(format, innerException, args) { }
	}


	public class MissingCommandException : AdoNetStoreException {

		internal protected MissingCommandException(string entityTypeName)
			: base("Not all required commands exist for loading and/or saving entities of type '{0}'.", entityTypeName) {
		}


		internal protected MissingCommandException(RepositoryCommandType commandType, string entityTypeName)
			: base("Command for {0} entities of type '{1}' does not exist.",
				commandType == RepositoryCommandType.Delete ? "deleting" :
				commandType == RepositoryCommandType.Insert ? "inserting" :
				commandType == RepositoryCommandType.SelectById ? "loading single" :
				//commandType == RepositoryCommandType.SelectByName ? "loading named single " :
				commandType == RepositoryCommandType.SelectAll ? "loading multiple" :
				commandType == RepositoryCommandType.Update ? "updating" : "loading and/or saving", entityTypeName) {
		}


		internal protected MissingCommandException(string entityTypeName, string filterEntityTypeName)
			: base("Command for loading entities of type '{1}' filtered by Id of '{1}' does not exist.", entityTypeName, filterEntityTypeName) {
		}
	}


	/// <summary>
	/// Stores Diagramming projects in any ADO.NET enabled database management system.
	/// </summary>
	public abstract class AdoNetStore : Store {

		#region Store Implementation

		/// <override></override>
		public override int Version {
			get { return version; }
			set { version = value; }
		}


		/// <override></override>
		public override string ProjectName {
			get { return ProjectName; }
			set {
				if (value == null) throw new ArgumentNullException("ProjectName");
				projectName = value;
			}
		}


		/// <override></override>
		public override bool Exists() {
			bool result;
			EnsureDataSourceOpen();
			Connection.Open();
			try {
				LoadSysCommands();
				IDbCommand cmd = GetCommand(ProjectSettings.EntityTypeName, RepositoryCommandType.SelectByName);
				((IDataParameter)cmd.Parameters[0]).Value = projectName;
				using (IDataReader reader = cmd.ExecuteReader())
					result = reader.Read();
			} finally {
				Connection.Close();
				EnsureDataSourceClosed();
			}
			return result;
		}


		/// <override></override>
		public override void Create(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			// TODO 2: Perhaps check, whether the project name already exists?
			// TODO 2: Perhaps check, that base version is compatible with database schema?
			OpenCore(cache, true);
		}


		/// <override></override>
		public override void Open(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			// We just check whether the cache is reachable.
			OpenCore(cache, false);
		}


		/// <override></override>
		public override void Close(IStoreCache storeCache) {
			if (storeCache == null) throw new ArgumentNullException("storeCache");
			if (connection != null) connection.Dispose();
			connection = null;
			base.Close(storeCache);
		}


		/// <override></override>
		public override void Erase() {
			AssertClosed();
			EnsureDataSourceOpen();
			try {
				LoadSysCommands();
				// If all constraints are in place, deleting the project info is sufficient.
				IDbCommand cmd = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.Delete);
				cmd.Transaction = transaction;
				((DbParameter)cmd.Parameters[0]). Value = projectName;
				cmd.ExecuteNonQuery();
			} finally {
				commands.Clear();	// Unload all commands
				EnsureDataSourceClosed();
			}
		}


		// This is the actual reading function and will go into a separate data access 
		// layer in later versions.
		/// <override></override>
		public IEnumerable<EntityBucket<TEntity>> LoadEntities<TEntity>(IStoreCache cache,
			IEntityType entityType, IdFilter idFilter, Resolver parentResolver, RepositoryCommandType cmdType, 
			params object[] parameters) where TEntity: IEntity {
			//
			// We must store all loaded entities for loading of inner objects
			// TODO 2: Now with MARS, we could reunite the two phases.
			List<EntityBucket<TEntity>> newEntities = new List<EntityBucket<TEntity>>(1000);
			//
			bool connectionOpened = EnsureDataSourceOpen();
			DbParameterReader repositoryReader = null;
			int version = entityType.RepositoryVersion;
			try {
				IDbCommand cmd = GetCommand(entityType.FullName, cmdType);
				for (int i = 0; i < parameters.Length; ++i)
					((IDbDataParameter)cmd.Parameters[i]).Value = parameters[i];
				//
				IDataReader dataReader = cmd.ExecuteReader();
				try {
					repositoryReader = new DbParameterReader(this, cache);
					repositoryReader.ResetFieldReading(entityType.PropertyDefinitions, dataReader);
					while (repositoryReader.DoBeginObject()) {
						object id = repositoryReader.ReadId();
						if (idFilter(id)) {
							// Read the fields
							object parentId = repositoryReader.ReadId();
							TEntity entity = (TEntity)entityType.CreateInstanceForLoading();
							entity.AssignId(id);
							entity.LoadFields(repositoryReader, version);
							int pix = 0;
							// Read the composite inner objects
							foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
								if (pi is EntityInnerObjectsDefinition && IsComposition(pi)) {
									// property index -1 is id. LoadInnerObjects will increment the PropertyIndex.
									repositoryReader.PropertyIndex = pix - 1;
									entity.LoadInnerObjects(pi.Name, repositoryReader, version);
								}
								++pix;
							}
							newEntities.Add(new EntityBucket<TEntity>(entity, parentResolver(parentId), ItemState.Original));
						}
					}
				} finally {
					dataReader.Close();
				}
				// Read the associated inner objects
				if (entityType.HasInnerObjects) {
					repositoryReader.ResetInnerObjectsReading();
					foreach (EntityBucket<TEntity> eb in newEntities) {
						repositoryReader.PrepareInnerObjectsReading(eb.ObjectRef);
						int pix = 0;
						foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
							if (pi is EntityInnerObjectsDefinition && !IsComposition(pi)) {
								repositoryReader.PropertyIndex = pix - 1;
								eb.ObjectRef.LoadInnerObjects(pi.Name, repositoryReader, version);
							}
							++pix;
						}
					}
				}
			} finally {
				if (repositoryReader != null) repositoryReader.Dispose();
				if (connectionOpened) EnsureDataSourceClosed();
			}
			foreach (EntityBucket<TEntity> eb in newEntities)
				yield return eb;
		}


		public override void SaveChanges(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			AssertOpen();
			AssertValid();
			const bool transactional = true;
			Connection.Open();
			Debug.Assert(transaction == null);
			if (transactional) transaction = Connection.BeginTransaction();
			try {
				//// -- Zeroth Step: Insert or update the project --
				IDbCommand projectCommand;
				if (cache.ProjectId == null) {
				  // project is a new one
					projectCommand = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.Insert);
					((DbParameter)projectCommand.Parameters[0]).Value = cache.ProjectName;
					((DbParameter)projectCommand.Parameters[1]).Value = version;
					projectCommand.Transaction = transaction;
					// Cast to int makes sure the command returns an id.
					cache.SetProjectOwnerId((int)projectCommand.ExecuteScalar());
				} else {
				  // We update the project projectName in case it has been modified.
					projectCommand = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.Update);
				  ((DbParameter)projectCommand.Parameters[0]).Value = cache.ProjectId;
					((DbParameter)projectCommand.Parameters[1]).Value = cache.ProjectName;
					((DbParameter)projectCommand.Parameters[2]).Value = version;
					projectCommand.Transaction = transaction;
					projectCommand.ExecuteNonQuery();
				}
				// -- First Step: Delete --
				// Children first, owners afterwards
				/*FlushDeletedStyles();
				FlushDeletedDesigns();*/
				DeleteShapeConnections(cache);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						DeleteEntities<Shape>(cache, et, cache.LoadedShapes);
				DeleteEntities<Diagram>(cache, cache.FindEntityTypeByName(Diagram.EntityTypeName), cache.LoadedDiagrams);
				DeleteEntities<ProjectSettings>(cache, cache.FindEntityTypeByName(ProjectSettings.EntityTypeName), cache.LoadedProjects);
				// FlushDeletedTemplates();
				//
				// -- Second Step: Updated --
				// Owners first, children afterwards
				UpdateEntities<ProjectSettings>(cache, cache.FindEntityTypeByName(ProjectSettings.EntityTypeName), cache.LoadedProjects, null);
				UpdateEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName), cache.LoadedDesigns, null);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is ColorStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is CapStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is LineStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is FillStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is CharacterStyle);
				UpdateEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), cache.LoadedStyles, (s, o) => s is ParagraphStyle);
				UpdateEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName), cache.LoadedTemplates, null);
				UpdateEntities<IModelMapping>(cache, cache.FindEntityTypeByName(NumericModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is NumericModelMapping);
				UpdateEntities<IModelMapping>(cache, cache.FindEntityTypeByName(FormatModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is FormatModelMapping);
				UpdateEntities<IModelMapping>(cache, cache.FindEntityTypeByName(StyleModelMapping.EntityTypeName), cache.LoadedModelMappings, (s, o) => s is StyleModelMapping);
				UpdateEntities<Diagram>(cache, cache.FindEntityTypeByName(Diagram.EntityTypeName), cache.LoadedDiagrams, null);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						UpdateEntities<Shape>(cache, et, cache.LoadedShapes, delegate(Shape s, IEntity o) { return s.Type.FullName == et.FullName; });
				//
				// -- Third Step: Insert --
				// Owners first, children afterwards
				InsertEntities<ProjectSettings>(cache, cache.FindEntityTypeByName(ProjectSettings.EntityTypeName), cache.NewProjects, null);
				InsertEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName), cache.NewDesigns, null);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), cache.NewStyles, (s, o) => s is ColorStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), cache.NewStyles, (s, o) => s is CapStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), cache.NewStyles, (s, o) => s is LineStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), cache.NewStyles, (s, o) => s is FillStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), cache.NewStyles, (s, o) => s is CharacterStyle);
				InsertEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), cache.NewStyles, (s, o) => s is ParagraphStyle);
				// Flush templates and their modelObjects, shapes and model mappings
				InsertEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName), cache.NewTemplates, null);
				// Flush model objects owned by templates
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.ModelObject) {
						// Flush parents first, then children
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetModelObjectCommand(et.FullName, RepositoryCommandType.InsertTemplateModelObject), 
							(m, o) => (m.Parent == null && m.Type.FullName == et.FullName && o is Template));
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetModelObjectCommand(et.FullName, RepositoryCommandType.InsertTemplateModelObject),
							(m, o) => (m.Parent != null && m.Type.FullName == et.FullName && o is Template));
					}
				// Flush shapes owned by templates
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						InsertEntities<Shape>(cache, et, cache.NewShapes, GetShapeCommand(et.FullName, RepositoryCommandType.InsertTemplateShape),
							(s, o) => o is Template && s.Type.FullName == et.FullName);
				// Flush model mappings
				InsertEntities<IModelMapping>(cache, cache.FindEntityTypeByName(NumericModelMapping.EntityTypeName), cache.NewModelMappings, (s, o) => s is NumericModelMapping);
				InsertEntities<IModelMapping>(cache, cache.FindEntityTypeByName(FormatModelMapping.EntityTypeName), cache.NewModelMappings, (s, o) => s is FormatModelMapping);
				InsertEntities<IModelMapping>(cache, cache.FindEntityTypeByName(StyleModelMapping.EntityTypeName), cache.NewModelMappings, (s, o) => s is StyleModelMapping);

				// Flush model
				InsertEntities<Model>(cache, cache.FindEntityTypeByName(Model.EntityTypeName), cache.NewModels, null);
				// Flush model objects
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.ModelObject) {
						// Flush parents first, then children
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetModelObjectCommand(et.FullName, RepositoryCommandType.InsertModelModelObject),
							(m, o) => (m.Parent == null && m.Type.FullName == et.FullName && !(o is Template)));
						InsertEntities<IModelObject>(cache, et, cache.NewModelObjects, GetModelObjectCommand(et.FullName, RepositoryCommandType.InsertModelModelObject),
							(m, o) => (m.Parent != null && m.Type.FullName == et.FullName && !(o is Template)));
					}

				// Flush diagrams and their shapes
				InsertEntities<Diagram>(cache, cache.FindEntityTypeByName(Diagram.EntityTypeName), cache.NewDiagrams, null);
				foreach (EntityType et in cache.EntityTypes)
					if (et.Category == EntityCategory.Shape)
						InsertEntities<Shape>(cache, et, cache.NewShapes, GetShapeCommand(et.FullName, RepositoryCommandType.InsertDiagramShape),
							(s, o) => o is Diagram && s.Type.FullName == et.FullName);
				// RUNTIME CHECK: At this point we only must have new shapes left that are template shapes or child shapes
				foreach (KeyValuePair<Shape, IEntity> p in cache.NewShapes) 
					Debug.Assert(((IEntity)p.Key).Id != null || p.Value is Template || p.Value is Shape);
				// Flush child shapes level by level
				// In each cycle we insert all those shapes, whose parent has already been inserted.
				bool allInserted;
				do {
					allInserted = true;
					foreach (EntityType et in cache.EntityTypes)
						if (et.Category == EntityCategory.Shape)
							InsertEntities<Shape>(cache, et, cache.NewShapes, GetShapeCommand(et.FullName, RepositoryCommandType.InsertChildShape),
								delegate(Shape s, IEntity o) { if (((IEntity)s).Id != null) return false; else { allInserted = false; return s.Type.FullName == et.FullName && ((IEntity)s.Parent).Id != null; }});
				} while (!allInserted);
				InsertShapeConnections(cache);
				UpdateShapeOwners(cache);
				//
				if (transactional) transaction.Commit();
			} catch (Exception exc) {
				Debug.Print(exc.Message);
				if (transactional) transaction.Rollback();
				throw;
			} finally {
				transaction = null;
				Connection.Close();
			}
		}


		/// <override></override>
		public override void LoadTemplates(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			EnsureDataSourceOpen();
			try {
				// Load all templates
				foreach (EntityBucket<Template> tb in LoadEntities<Template>(cache, cache.FindEntityTypeByName(Template.EntityTypeName),
					id => !cache.LoadedTemplates.Contains(id), id => cache.LoadedProjects.GetEntity(id),
					RepositoryCommandType.SelectByOwnerId, projectId)) {
					cache.LoadedTemplates.Add(tb);
				}
				// Load all template model objects. The template shapes will assign them in their load method
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						foreach (EntityBucket<IModelObject> sb in LoadEntities<IModelObject>(cache, et, id => true, id => cache.LoadedTemplates.GetEntity(id),
							RepositoryCommandType.SelectTemplateModelObjects, cache.ProjectId)) {
							cache.LoadedModelObjects.Add(sb);
						}
					}
				}
				// Load all template shapes and assign them to their templates
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.Shape) {
						foreach (EntityBucket<Shape> sb in LoadEntities<Shape>(cache, et, id => true, id => cache.LoadedTemplates.GetEntity(id),
							RepositoryCommandType.SelectTemplateShapes, cache.ProjectId)) {
							Template t = (Template)sb.Owner;
							if (t.Shape != null) throw new AdoNetStoreException("Template {0} has more than one shape.", t.Id);
							((Template)sb.Owner).Shape = sb.ObjectRef;
						}
					}
				}
				// Load and assign all ModelMappings of the template.
				// Model mappings have to be assigned after shape and model object
				foreach (EntityBucket<Template> tb in cache.LoadedTemplates) {
					if (tb.ObjectRef.Shape == null) throw new AdoNetStoreException("Template {0} has no shape.", tb.ObjectRef.Id);
					foreach (EntityType et in cache.EntityTypes) {
						if (et.Category == EntityCategory.ModelMapping) {
							foreach (EntityBucket<IModelMapping> eb in LoadEntities<IModelMapping>(cache, et,
								id => true, id => tb.ObjectRef, RepositoryCommandType.SelectByOwnerId, tb.ObjectRef.Id)) {
								cache.LoadedModelMappings.Add(eb);
								((Template)tb.ObjectRef).MapProperties(eb.ObjectRef);
							}
						}
					}
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadProjects(IStoreCache cache, IEntityType entityType, params object[] parameters) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (entityType == null) throw new ArgumentNullException("entityType");
			foreach (EntityBucket<ProjectSettings> pb in LoadEntities<ProjectSettings>(cache, entityType, id => true, id => null,
				RepositoryCommandType.SelectByName, cache.ProjectName))
				cache.LoadedProjects.Add(pb);
		}


		/// <override></override>
		public override void LoadModel(IStoreCache cache, object modelId) {
			if (cache == null) throw new ArgumentNullException("cache");
			EnsureDataSourceOpen();
			try {
				// Load model
				foreach (EntityBucket<Model> mb in LoadEntities<Model>(cache,
					cache.FindEntityTypeByName(Model.EntityTypeName), id => true,
					id => cache.Project, RepositoryCommandType.SelectByOwnerId, cache.ProjectId))
					cache.LoadedModels.Add(mb);
			} finally {
				EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadDesigns(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			Debug.Assert(((IEntity)cache.Project).Id.Equals(projectId));
			EnsureDataSourceOpen();
			try {
				foreach (EntityBucket<Design> pb in LoadEntities<Design>(cache, cache.FindEntityTypeByName(Design.EntityTypeName),
					id => true, id => cache.Project, RepositoryCommandType.SelectByOwnerId, projectId)) {
					cache.LoadedDesigns.Add(pb);
					// Load the styles of the design (They reference each other so put them into the collections immediately.)
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
					foreach (EntityBucket<IStyle> sb in LoadEntities<IStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName),
						id => true, pid => pb.Owner, RepositoryCommandType.SelectByOwnerId, ((IEntity)pb.ObjectRef).Id)) {
						pb.ObjectRef.AddStyle(sb.ObjectRef);
						cache.LoadedStyles.Add(sb);
					}
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadDiagramShapes(IStoreCache cache, Diagram diagram) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (diagram == null) throw new ArgumentNullException("diagram");
			EnsureDataSourceOpen();
			try {
				// Load all shapes of diagram
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.Shape) {
						foreach (EntityBucket<Shape> sb
							in LoadEntities<Shape>(cache, et, id => !cache.LoadedShapes.Contains(id), pid => diagram,
							RepositoryCommandType.SelectDiagramShapes, ((IEntity)diagram).Id)) {
							Debug.Assert(!diagram.Shapes.Contains(sb.ObjectRef));
							diagram.Shapes.Add(sb.ObjectRef, sb.ObjectRef.ZOrder);
							diagram.AddShapeToLayers(sb.ObjectRef, sb.ObjectRef.Layers);	// not really necessary
							LoadChildShapes(cache, sb.ObjectRef);
							cache.LoadedShapes.Add(sb);
						}
					}
				}
				// Load all shape connections of diagram
				LoadShapeConnections(cache, diagram);
			} finally {
				EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadTemplateShapes(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void LoadDiagrams(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			foreach (EntityBucket<Diagram> db in LoadEntities<Diagram>(cache,
				cache.FindEntityTypeByName(Diagram.EntityTypeName), id => true,
				id => cache.Project, RepositoryCommandType.SelectByOwnerId, cache.ProjectId)) {
				cache.LoadedDiagrams.Add(db);
				LoadDiagramShapes(cache, db.ObjectRef);
			}
		}


		/// <override></override>
		public override void LoadChildShapes(IStoreCache cache, object parentShapeId) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (parentShapeId == null) throw new ArgumentNullException("parentShapeId");
			EnsureDataSourceOpen();
			try {
				Shape parentShape = cache.GetShape(parentShapeId);
				// Load all shapes of diagram
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.Shape) {
						foreach (EntityBucket<Shape> sb
							in LoadEntities<Shape>(cache, et, id => !cache.LoadedShapes.Contains(id), pid => parentShape,
							RepositoryCommandType.SelectDiagramShapes, parentShapeId)) {
							Debug.Assert(!parentShape.Children.Contains(sb.ObjectRef));
							parentShape.Children.Add(sb.ObjectRef);
							cache.LoadedShapes.Add(sb);
						}
					}
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadTemplateModelObjects(IStoreCache cache, object templateId) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (templateId == null) throw new ArgumentNullException("templateId");
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void LoadModelModelObjects(IStoreCache cache, object modelId) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (modelId == null) throw new ArgumentNullException("modelId");
			EnsureDataSourceOpen();
			try {
				// Load all root model objects of the model
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						foreach (EntityBucket<IModelObject> mb
							in LoadEntities<IModelObject>(cache, et, id => !cache.LoadedModelObjects.Contains(id), 
							id => cache.GetModel(), RepositoryCommandType.SelectModelModelObjects, modelId)) {
							cache.LoadedModelObjects.Add(mb);
							LoadChildModelObjects(cache, mb.ObjectRef.Id);
						}
					}
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}


		/// <override></override>
		public override void LoadChildModelObjects(IStoreCache cache, object parentModelObjectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (parentModelObjectId == null) throw new ArgumentNullException("parentModelObjectId");
			EnsureDataSourceOpen();
			try {
				// Load all root model objects of the model
				foreach (EntityType et in cache.EntityTypes) {
					if (et.Category == EntityCategory.ModelObject) {
						foreach (EntityBucket<IModelObject> mb
							in LoadEntities<IModelObject>(cache, et, id => !cache.LoadedModelObjects.Contains(id),
							id => cache.GetModelObject(parentModelObjectId), RepositoryCommandType.SelectChildModelObjects, parentModelObjectId)) {
							cache.LoadedModelObjects.Add(mb);
						}
					}
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}

		#endregion


		protected virtual void LoadSysCommands() {
			commands.Clear();
			IDbCommand cmdCmd = GetSelectSysCommandsCommand();
			IDbCommand paramCmd = GetSelectSysParameterCommand();
			CommandKey ck;
			using (IDataReader reader = cmdCmd.ExecuteReader()) {
				while (reader.Read()) {
					ck.Kind = (RepositoryCommandType)Enum.Parse(typeof(RepositoryCommandType), reader.GetString(1));
					ck.EntityTypeName = reader.GetString(2);
					IDbCommand command = CreateCommand(reader.GetString(3));
					// Read parameters
					((IDataParameter)paramCmd.Parameters[0]).Value = reader.GetInt32(0);
					using (IDataReader paramReader = paramCmd.ExecuteReader()) {
						while (paramReader.Read()) {
							command.Parameters.Add(CreateParameter(paramReader.GetString(0),
								(DbType)Enum.Parse(typeof(DbType), paramReader.GetString(1))));
						}
					}
					commands.Add(ck, command);
				}
			}
		}


		protected virtual IDbCommand GetInsertSysCommandCommand() {
			IDbCommand result = CreateCommand(
				"INSERT INTO SysCommand (Kind, EntityType, Text) VALUES (@Kind, @EntityType, @Text); "
				+ "SELECT CAST(IDENT_CURRENT('SysCommand') AS INT)",
				CreateParameter("Kind", DbType.String),
				CreateParameter("EntityType", DbType.String),
				CreateParameter("Text", DbType.String));
			result.Connection = Connection;
			return result;
		}


		protected virtual IDbCommand GetInsertSysParameterCommand() {
			IDbCommand result = CreateCommand("INSERT INTO SysParameter (Command, Name, Type) VALUES (@Command, @Name, @Type)",
				CreateParameter("Command", DbType.Int32),
				CreateParameter("Name", DbType.String),
				CreateParameter("Type", DbType.String));
			result.Connection = Connection;
			return result;
		}


		protected IDbCommand GetSelectSysCommandsCommand() {
			IDbCommand result = CreateCommand("SELECT * FROM SysCommand");
			result.Connection = Connection;
			return result;
		}


		protected IDbCommand GetSelectSysParameterCommand() {
			IDbCommand result = CreateCommand("SELECT Name, Type FROM SysParameter WHERE Command = @Command",
				CreateParameter("Command", DbType.Int32));
			result.Connection = Connection;
			return result;
		}


		/// <override></override>
		protected void LoadShapeConnections(IStoreCache cache, Diagram diagram) {
			// If shape is new, there cannot be any connections in the database.
			if (((IEntity)diagram).Id == null) return;
			//
			bool connectionOpened = EnsureDataSourceOpen();
			try {
				IDbCommand cmd = GetCommand("Core.ShapeConnection", RepositoryCommandType.SelectByOwnerId);
				((IDbDataParameter)cmd.Parameters[0]).Value = ((IEntity)diagram).Id;
				using (IDataReader dataReader = cmd.ExecuteReader()) {
					ShapeConnection sc;
					while (dataReader.Read()) {
						sc.ConnectorShape = cache.GetShape(dataReader.GetInt32(0));
						sc.GluePointId = dataReader.GetInt32(1);
						sc.TargetShape = cache.GetShape(dataReader.GetInt32(2)); // TODO 3: Error handling?
						sc.TargetPointId = dataReader.GetInt32(3);
						sc.ConnectorShape.Connect(sc.GluePointId, sc.TargetShape, sc.TargetPointId);
					}
				}
			} finally {
				if (connectionOpened) EnsureDataSourceClosed();
			}
		}


		private void LoadChildShapes(IStoreCache cache, Shape s) {
			foreach (EntityType et in cache.EntityTypes) {
				if (et.Category == EntityCategory.Shape) {
					foreach (EntityBucket<Shape> sb
						in LoadEntities<Shape>(cache, et, id => true, pid => s, RepositoryCommandType.SelectChildShapes, ((IEntity)s).Id))
						s.Children.Add(sb.ObjectRef);
				}
			}
		}


		#region Command Obtaining Functions

		// Command setting functions must specify the entity by the entity projectName and 
		// not by the entity type, because it must be possible to set commands, before 
		// libraries are loaded. Therefore the entities are usually registered later
		// than the command.
		// Another reason is that external clients have no access to the entity type.
		// They should not even know the interface IEntityType or the type EntityType.

		protected IDbCommand CreateCommand() {
			if (factory == null)
				throw new DiagrammingException("No valid ADO.NET provider specified.");
			return factory.CreateCommand();
		}


		protected IDbCommand CreateCommand(string cmdText, params IDataParameter[] parameters) {
			IDbCommand result = CreateCommand();
			result.CommandText = cmdText;
			foreach (IDataParameter p in parameters) result.Parameters.Add(p);
			return result;
		}


		protected IDataParameter CreateParameter(string name, DbType dbType) {
			IDataParameter result = factory.CreateParameter();
			result.DbType = dbType;
			result.ParameterName = name;
			return result;
		}


		public IDbCommand GetCreateTablesCommand() {
			if (createTablesCommand == null) throw new DiagrammingException("Command for creating the schema is not defined.");
			if (createTablesCommand.Connection != Connection) createTablesCommand.Connection = Connection;
			return createTablesCommand;
		}


		public void SetCreateTablesCommand(IDbCommand command) {
			if (command == null) throw new ArgumentNullException("command");
			createTablesCommand = command;
		}


		public IDbCommand GetCommand(string entityTypeName, RepositoryCommandType cmdType) {
			if (entityTypeName == null) throw new ArgumentNullException("entityTypeName");
			if (entityTypeName == string.Empty) throw new ArgumentException("entityTypeName");
			IDbCommand result;
			CommandKey commandKey;
			commandKey.Kind = cmdType;
			commandKey.EntityTypeName = entityTypeName;
			if (!commands.TryGetValue(commandKey, out result))
				throw new InvalidOperationException(string.Format("ADO.NET command '{0}' for entity type '{1}' has not been defined.", cmdType, entityTypeName));
			if (result.Connection != Connection) result.Connection = Connection;
			return result;
		}


		public void SetCommand(string entityTypeName, RepositoryCommandType cmdType, IDbCommand command) {
			if (entityTypeName == null) throw new ArgumentNullException("entityTypeName");
			if (entityTypeName == string.Empty) throw new ArgumentException("entityTypeName");
			if (command == null) throw new ArgumentNullException("command");
			CommandKey commandKey;
			commandKey.Kind = cmdType;
			commandKey.EntityTypeName = entityTypeName;
			if (commands.ContainsKey(commandKey))
				commands[commandKey] = command;
			else commands.Add(commandKey, command);
		}


		#region Get/Set commands for Styles

		public IDbCommand GetSelectStyleByIdCommand(string entityTypeName) {
			return GetCommand(entityTypeName, RepositoryCommandType.SelectById);
		}


		public void SetSelectStyleByIdCommand(string entityTypeName, IDbCommand command) {
			SetCommand(entityTypeName, RepositoryCommandType.SelectById, command);
		}


		/// <summary>
		/// Retrieves a command that selects all styles for the given design for the current
		/// project.
		/// </summary>
		public IDbCommand GetSelectStylesByDesignIdCommand(string entityTypeName) {
			return GetCommand(entityTypeName, RepositoryCommandType.SelectByOwnerId);
		}


		public void SetSelectStylesByDesignIdCommand(string entityTypeName, IDbCommand command) {
			SetCommand(entityTypeName, RepositoryCommandType.SelectByOwnerId, command);
		}


		public IDbCommand GetInsertStyleCommand(string entityTypeName) {
			return GetCommand(entityTypeName, RepositoryCommandType.Insert);
		}


		public void SetInsertStyleCommand(string entityTypeName, IDbCommand command) {
			SetCommand(entityTypeName, RepositoryCommandType.Insert, command);
		}


		public IDbCommand GetUpdateStyleCommand(string entityTypeName) {
			return GetCommand(entityTypeName, RepositoryCommandType.Update);
		}


		public void SetUpdateStyleCommand(string entityTypeName, IDbCommand command) {
			SetCommand(entityTypeName, RepositoryCommandType.Update, command);
		}


		public IDbCommand GetDeleteStyleCommand(string entityTypeName) {
			return GetCommand(entityTypeName, RepositoryCommandType.Delete);
		}


		public void SetDeleteStyleCommand(string entityTypeName, IDbCommand command) {
			SetCommand(entityTypeName, RepositoryCommandType.Delete, command);
		}

		#endregion


		#region Get/Set commands for Diagrams

		public IDbCommand GetSelectDiagramIdsCommand() {
			// ToDo3: Use Diagram.EntityTypeName here?
			return GetCommand("Core.Diagram", RepositoryCommandType.SelectAll);
		}


		public void SetSelectAllDiagramsCommand(IDbCommand command) {
			// ToDo3: Use Diagram.EntityTypeName here?
			SetCommand("Core.Diagram", RepositoryCommandType.SelectAll, command);
		}


		public IDbCommand GetSelectDiagramByIdCommand() {
			// ToDo3: Use Diagram.EntityTypeName here?
			return GetCommand("Core.Diagram", RepositoryCommandType.SelectById);
		}


		public void SetSelectDiagramByIdCommand(IDbCommand command) {
			// ToDo3: Use Diagram.EntityTypeName here?
			SetCommand("Core.Diagram", RepositoryCommandType.SelectById, command);
		}


		public IDbCommand GetInsertDiagramCommand() {
			// ToDo3: Use Diagram.EntityTypeName here?
			return GetCommand("Core.Diagram", RepositoryCommandType.Insert);
		}


		public void SetInsertDiagramCommand(IDbCommand command) {
			// ToDo3: Use Diagram.EntityTypeName here?
			SetCommand("Core.Diagram", RepositoryCommandType.Insert, command);
		}


		public IDbCommand GetUpdateDiagramCommand() {
			// ToDo3: Use Diagram.EntityTypeName here?
			return GetCommand("Core.Diagram", RepositoryCommandType.Update);
		}


		public void SetUpdateDiagramCommand(IDbCommand command) {
			// ToDo3: Use Diagram.EntityTypeName here?
			SetCommand("Core.Diagram", RepositoryCommandType.Update, command);
		}


		public IDbCommand GetDeleteDiagramCommand() {
			// ToDo3: Use Diagram.EntityTypeName here?
			return GetCommand("Core.Diagram", RepositoryCommandType.Delete);
		}


		public void SetDeleteDiagramCommand(IDbCommand command) {
			// ToDo3: Use Diagram.EntityTypeName here?
			SetCommand("Core.Diagram", RepositoryCommandType.Delete, command);
		}

		#endregion


		#region Get/Set commands for Templates

		protected virtual void SetTemplateCommand(RepositoryCommandType cmdType, IDbCommand command) {
			SetCommand(Template.EntityTypeName, cmdType, command);
		}


		protected virtual IDbCommand GetTemplateCommand(RepositoryCommandType cmdType) {
			return GetCommand(Template.EntityTypeName, cmdType);
		}

		#endregion


		#region Get/Set commands for Shapes

		public virtual IDbCommand GetSelectShapeIdsCommand(string entityTypeName) {
			return GetShapeCommand(entityTypeName, RepositoryCommandType.SelectAll);
		}


		public virtual void SetSelectShapeIdsCommand(string entityTypeName, IDbCommand command) {
			SetShapeCommand(entityTypeName, RepositoryCommandType.SelectAll, command);
		}


		public virtual IDbCommand GetSelectShapeByIdCommand(string entityTypeName) {
			return GetShapeCommand(entityTypeName, RepositoryCommandType.SelectById);
		}


		public virtual void SetSelectShapeByIdCommand(string entityTypeName, IDbCommand command) {
			SetShapeCommand(entityTypeName, RepositoryCommandType.SelectById, command);
		}


		public virtual IDbCommand GetSelectShapesByDiagramId(string entityTypeName) {
			// We use the "SelectByName" command type for this task
			return GetShapeCommand(entityTypeName, RepositoryCommandType.SelectByOwnerId);
		}


		public virtual void SetSelectShapesByDiagramId(string entityTypeName, IDbCommand command) {
			// We use the "SelectByName" command type for this task
			SetShapeCommand(entityTypeName, RepositoryCommandType.SelectByOwnerId, command);
		}


		protected virtual IDbCommand GetInsertShapeCommand(string entityTypeName) {
			return GetShapeCommand(entityTypeName, RepositoryCommandType.Insert);
		}


		protected virtual void SetInsertShapeCommand(string entityTypeName, IDbCommand command) {
			SetShapeCommand(entityTypeName, RepositoryCommandType.Insert, command);
		}


		protected virtual IDbCommand GetUpdateShapeCommand(string entityTypeName) {
			return GetShapeCommand(entityTypeName, RepositoryCommandType.Update);
		}


		protected virtual void SetUpdateShapeCommand(string entityTypeName, IDbCommand command) {
			SetShapeCommand(entityTypeName, RepositoryCommandType.Update, command);
		}


		protected virtual IDbCommand GetDeleteShapeCommand(string entityTypeName) {
			return GetShapeCommand(entityTypeName, RepositoryCommandType.Delete);
		}


		public void SetDeleteShapeCommand(string entityTypeName, IDbCommand command) {
			SetShapeCommand(entityTypeName, RepositoryCommandType.Delete, command);
		}


		protected IDbCommand GetShapeCommand(string entityTypeName, RepositoryCommandType cmdType) {
			return GetCommand(entityTypeName, cmdType);
		}


		protected void SetShapeCommand(string entityTypeName, RepositoryCommandType cmdType, IDbCommand command) {
			SetCommand(entityTypeName, cmdType, command);
		}

		#endregion


		#region Get/Set commands for ShapeConnections

		/// <summary>
		/// Retrieves a SQL command for selecting the connections of a given shape.
		/// </summary>
		/// <returns></returns>
		/// <remarks>The command selects the connections for which the given shape 
		/// is on the active side.</remarks>
		public IDbCommand GetSelectShapeConnectionByShapeIdCommand() {
			// ToDo3: Use a constant here?
			return GetCommand("Core.ShapeConnection", RepositoryCommandType.SelectByOwnerId);
		}


		/// <summary>
		/// Defines a SQL command for selecting the connections of a given shape.
		/// </summary>
		/// <returns></returns>
		/// <remarks>The command selects the connections for which the given shape 
		/// is on the active side.</remarks>
		public void SetSelectShapeConnectionByShapeIdCommand(IDbCommand command) {
			// ToDo3: Use a constant here?
			SetCommand("Core.ShapeConnection", RepositoryCommandType.SelectByOwnerId, command);
		}


		public virtual IDbCommand GetInsertShapeConnectionCommand() {
			// ToDo3: Use a constant here?
			return GetCommand("Core.ShapeConnection", RepositoryCommandType.Insert);
		}


		public virtual void SetInsertShapeConnectionCommand(IDbCommand command) {
			// ToDo3: Use a constant here?
			SetCommand("Core.ShapeConnection", RepositoryCommandType.Insert, command);
		}


		public virtual IDbCommand GetDeleteShapeConnectionCommand() {
			// ToDo3: Use a constant here?
			return GetCommand("Core.ShapeConnection", RepositoryCommandType.Delete);
		}


		public virtual void SetDeleteShapeConnectionCommand(IDbCommand command) {
			// ToDo3: Use a constant here?
			SetCommand("Core.ShapeConnection", RepositoryCommandType.Delete, command);
		}

		#endregion


		#region Get/Set commands for ModelObjects

		protected IDbCommand GetModelObjectCommand(string entityTypeName, RepositoryCommandType cmdType) {
			return GetCommand(entityTypeName, cmdType);
		}


		protected void SetModelObjectCommand(string entityTypeName, RepositoryCommandType cmdType, IDbCommand command) {
			SetCommand(entityTypeName, cmdType, command);
		}


		//public virtual IDbCommand GetSelectModelObjectIdsCommand(string entityTypeName) {
		//   return GetModelObjectCommand(entityTypeName, RepositoryCommandType.SelectAll);
		//}


		//public virtual void SetSelectModelObjectIdsCommand(string entityTypeName, IDbCommand command) {
		//   SetModelObjectCommand(entityTypeName, RepositoryCommandType.SelectAll, command);
		//}


		//public virtual IDbCommand GetSelectModelObjectByIdCommand(string entityTypeName) {
		//   return GetModelObjectCommand(entityTypeName, RepositoryCommandType.SelectById);
		//}


		//public virtual void SetSelectModelObjectByIdCommand(string entityTypeName, IDbCommand command) {
		//   SetModelObjectCommand(entityTypeName, RepositoryCommandType.SelectById, command);
		//}


		//public virtual IDbCommand GetSelectModelObjectsByDiagramId(string entityTypeName) {
		//   // We use the "SelectByName" command type for this task
		//   return GetModelObjectCommand(entityTypeName, RepositoryCommandType.SelectByOwnerId);
		//}


		//public virtual void SetSelectModelObjectsByDiagramId(string entityTypeName, IDbCommand command) {
		//   // We use the "SelectByName" command type for this task
		//   SetModelObjectCommand(entityTypeName, RepositoryCommandType.SelectByOwnerId, command);
		//}


		//protected virtual IDbCommand GetInsertModelObjectCommand(string entityTypeName) {
		//   return GetModelObjectCommand(entityTypeName, RepositoryCommandType.Insert);
		//}


		//protected virtual void SetInsertModelObjectCommand(string entityTypeName, IDbCommand command) {
		//   SetModelObjectCommand(entityTypeName, RepositoryCommandType.Insert, command);
		//}


		//protected virtual IDbCommand GetUpdateModelObjectCommand(string entityTypeName) {
		//   return GetModelObjectCommand(entityTypeName, RepositoryCommandType.Update);
		//}


		//protected virtual void SetUpdateModelObjectCommand(string entityTypeName, IDbCommand command) {
		//   SetModelObjectCommand(entityTypeName, RepositoryCommandType.Update, command);
		//}


		//protected virtual IDbCommand GetDeleteModelObjectCommand(string entityTypeName) {
		//   return GetModelObjectCommand(entityTypeName, RepositoryCommandType.Delete);
		//}


		//public void SetDeleteModelObjectCommand(string entityTypeName, IDbCommand command) {
		//   SetModelObjectCommand(entityTypeName, RepositoryCommandType.Delete, command);
		//}

		#endregion


		#region Get/Set commands for Inner Objects

		public virtual IDbCommand GetSelectInnerObjectsCommand(string entityTypeName) {
			return GetInnerObjectsCommand(entityTypeName, RepositoryCommandType.SelectById);
		}


		public virtual void SetSelectInnerObjectsCommand(string entityTypeName, IDbCommand command) {
			SetInnerObjectsCommand(entityTypeName, RepositoryCommandType.SelectById, command);
		}


		public virtual IDbCommand GetInsertInnerObjectsCommand(string entityTypeName) {
			return GetInnerObjectsCommand(entityTypeName, RepositoryCommandType.Insert);
		}
		
		
		public virtual void SetInsertInnerObjectsCommand(string entityTypeName, IDbCommand command) {
			SetInnerObjectsCommand(entityTypeName, RepositoryCommandType.Insert, command);
		}


		public virtual IDbCommand GetDeleteInnerObjectsCommand(string entityTypeName) {
			return GetInnerObjectsCommand(entityTypeName, RepositoryCommandType.Delete);
		}
		
		
		public virtual void SetDeleteInnerObjectsCommand(string entityTypeName, IDbCommand command) {
			SetInnerObjectsCommand(entityTypeName, RepositoryCommandType.Delete, command);
		}


		protected IDbCommand GetInnerObjectsCommand(string entityTypeName, RepositoryCommandType cmdType) {
			return GetCommand(entityTypeName, cmdType);
		}


		protected void SetInnerObjectsCommand(string entityTypeName, RepositoryCommandType cmdType, IDbCommand command) {
			SetCommand(entityTypeName, cmdType, command);
		}
		
		#endregion

		#endregion


		internal IDbTransaction CurrentTransaction {
			get { return transaction; }
		}


		#region DbParameterReader / DbParameterWriter

		protected class DbParameterReader : RepositoryReader, IDisposable {

			public DbParameterReader(AdoNetStore store, IStoreCache cache)
				: base(cache) {
				if (store == null) throw new ArgumentNullException("store");
				this.store = store;
			}


			~DbParameterReader() {
				Dispose();
			}


			// Initialisiert den Reader für das Lesen von Feldern einer Entity.
			// Der DataReader wird von außen verwaltet.
			internal void ResetFieldReading(IEnumerable<EntityPropertyDefinition> propertyInfos, IDataReader dataReader) {
				base.ResetFieldReading(propertyInfos);
				this.dataReader = dataReader;
			}


			// Initialisiert den Reader für das Lesen von inneren Objekten einer Entity.
			internal void ResetInnerObjectsReading() {
				// base.Reset(propertyInfos);
			}


			// Prepares the cache reader for reading of fields. The first fields to 
			// read internally are the id and the parent id. Therefore we start at index -2.
			protected internal override bool DoBeginObject() {
				Debug.Assert(dataReader != null);
				if (dataReader.Read()) PropertyIndex = -internalPropertyCount - 1;
				else PropertyIndex = int.MinValue;
				return PropertyIndex > int.MinValue;
			}


			protected internal override void DoEndObject() {
				// Nothing to do
			}


			public void PrepareInnerObjectsReading(IEntity persistableObject) {
				if (persistableObject == null) throw new ArgumentNullException("persistableObject");
				this.Object = persistableObject;
				PropertyIndex = 0;
				foreach (EntityPropertyDefinition pi in PropertyInfos) {
					if (pi is EntityInnerObjectsDefinition) break;
					++PropertyIndex;
				}
			}


			internal IDataReader DataReader {
				get { return dataReader; }
				set { dataReader = value; }
			}


			internal protected override object ReadId() {
				++PropertyIndex;
				ValidateFieldIndex();
				object result = dataReader.GetValue(PropertyIndex + internalPropertyCount);
				return Convert.IsDBNull(result) ? null : result;
			}


			#region RepositoryReader Members

			protected override bool DoReadBool() {
				return dataReader.GetBoolean(PropertyIndex + internalPropertyCount);
			}


			protected override byte DoReadByte() {
				return dataReader.GetByte(PropertyIndex + internalPropertyCount);
			}


			protected override short DoReadInt16() {
				return dataReader.GetInt16(PropertyIndex + internalPropertyCount);
			}


			protected override int DoReadInt32() {
				return dataReader.GetInt32(PropertyIndex + internalPropertyCount);
			}


			protected override long DoReadInt64() {
				return dataReader.GetInt64(PropertyIndex + internalPropertyCount);
			}


			protected override float DoReadFloat() {
				return dataReader.GetFloat(PropertyIndex + internalPropertyCount);
			}


			protected override double DoReadDouble() {
				return dataReader.GetDouble(PropertyIndex + internalPropertyCount);
			}


			protected override char DoReadChar() {
				// SqlDataReader.GetChar is not implemented and _always_ throws a NotSupportedException()
				//return dataReader.GetChar(PropertyIndex + internalPropertyCount);
				return Char.Parse(dataReader.GetString(PropertyIndex + internalPropertyCount));
			}


			protected override string DoReadString() {
				string result = "";
				if (!dataReader.IsDBNull(PropertyIndex + internalPropertyCount))
					result = dataReader.GetString(PropertyIndex + internalPropertyCount);
				return result;
			}


			protected override DateTime DoReadDate() {
				DateTime result = dataReader.GetDateTime(PropertyIndex + internalPropertyCount).ToLocalTime();
				return result;
			}


			protected override Image DoReadImage() {
				Image result = null;
				if (!dataReader.IsDBNull(PropertyIndex + internalPropertyCount)) {
					byte[] buffer = new Byte[dataReader.GetBytes(PropertyIndex + internalPropertyCount, 0, null, 0, 0)];
					dataReader.GetBytes(PropertyIndex + internalPropertyCount, 0, buffer, 0, buffer.Length);
					if (buffer.Length > 0) {
						MemoryStream stream = new MemoryStream(buffer, false);
						result = Image.FromStream(stream);
					}
				}
				return result;
			}


			// This method is called by the persistable object to fetch the next set of 
			// inner objects.
			public override void BeginReadInnerObjects() {
				Debug.Assert(innerObjectsReader == null);
				++PropertyIndex;
				EntityInnerObjectsDefinition innerInfo = (EntityInnerObjectsDefinition)propertyInfos[PropertyIndex];
				// TODO 3: Replace by generic mechanism once the string reader is established
				if (AdoNetStore.IsComposition(innerInfo)) {
					innerObjectsReader = new StringReader(Cache);
					((StringReader)innerObjectsReader).ResetFieldReading(innerInfo.PropertyDefinitions, dataReader.GetString(PropertyIndex + internalPropertyCount));
				} else {
					IDbCommand cmd = store.GetSelectInnerObjectsCommand(innerInfo.EntityTypeName);
					cmd.Transaction = store.CurrentTransaction;
					((IDataParameter)cmd.Parameters[0]).Value = Object.Id;
					innerObjectsDataReader = cmd.ExecuteReader();
					cmd.Dispose(); // Geht das?
					innerObjectsReader = new DbParameterReader(store, Cache);
					InnerObjectsReader.ResetFieldReading(innerInfo.PropertyDefinitions, innerObjectsDataReader);
				}
			}


			public override void EndReadInnerObjects() {
				Debug.Assert(innerObjectsReader != null);
				if (innerObjectsDataReader != null) {
					innerObjectsDataReader.Dispose();
					innerObjectsDataReader = null;
				}
				innerObjectsReader.Dispose();
				innerObjectsReader = null;
			}


			public override void EndReadInnerObject() {
				innerObjectsReader. DoEndObject();
			}


			protected override void Dispose(bool disposing) {
				if (innerObjectsDataReader != null) {
					innerObjectsDataReader.Dispose();
					innerObjectsDataReader = null;
				}
			}

			#endregion


			private void ValidateFieldIndex() {
				if (PropertyIndex >= dataReader.FieldCount)
					throw new InvalidOperationException("An object tries to load more properties than the entity is defined to have.");
			}


			private DbParameterReader InnerObjectsReader {
				get { return (DbParameterReader)innerObjectsReader; }
			}


			#region Fields

			// Id and parent id are always inside the cache.
			private const int internalPropertyCount = 2;

			private AdoNetStore store;

			// Reference to an outside-created data repositoryReader for fields reading
			private IDataReader dataReader;

			// Owned data repositoryReader for the fields of inner objects read by additional cache repositoryReader.
			private IDataReader innerObjectsDataReader;

			#endregion

		}


		protected class DbParameterWriter : RepositoryWriter {

			internal DbParameterWriter(AdoNetStore store, IStoreCache cache)
				: base(cache) {
				this.store = store;
			}


			internal IDbCommand Command {
				get { return command; }
				set { command = value; }
			}


			// Resets the repositoryWriter for handling another set of fields
			internal protected override void Reset(IEnumerable<EntityPropertyDefinition> propertyInfos) {
				base.Reset(propertyInfos);
			}


			/// <summary>
			/// This method has to be called before passing the repositoryWriter to the IPeristable object for saving.
			/// </summary>
			internal protected override void Prepare(IEntity entity) {
				if (entity == null) throw new ArgumentNullException("entity");
				base.Prepare(entity);
				PropertyIndex = -2;
			}


			protected internal override void Finish() {
				Flush();
				base.Finish();
			}


			/// <summary>
			/// Commits the changes to the database.
			/// </summary>
			internal protected void Flush() {
				if (Entity == null) {
					// Writing an inner object
					command.ExecuteNonQuery();
				} else if (Entity.Id == null) {
					// Inserting an entity
					Entity.AssignId(command.ExecuteScalar());
				} else {
					// Updating an entity
					command.ExecuteNonQuery();
				}
			}


			/// <summary>
			/// Prepares the cache repositoryWriter for writing the fields of another entity.
			/// </summary>
			internal protected void PrepareFields() {
				// The first parameter is the parent id, which is automatically inserted and does
				// therefore not correspond to to a real property. We handle it as property index -1.
				PropertyIndex = -2;
			}


			#region RepositoryWriter Members

			protected override void DoWriteId(object value) {
				if (value == null) value = DBNull.Value;
				DoWriteValue(value);
			}


			protected override void DoWriteBool(bool value) {
				DoWriteValue(value);
			}


			protected override void DoWriteByte(byte value) {
				DoWriteValue(value);
			}


			protected override void DoWriteInt16(short value) {
				DoWriteValue(value);
			}


			protected override void DoWriteInt32(int value) {
				DoWriteValue(value);
			}


			protected override void DoWriteInt64(long value) {
				DoWriteValue(value);
			}


			protected override void DoWriteFloat(float value) {
				DoWriteValue(value);
			}


			protected override void DoWriteDouble(double value) {
				DoWriteValue(value);
			}


			protected override void DoWriteChar(char value) {
				DoWriteValue(value);
			}


			protected override void DoWriteString(string value) {
				if (value == null) DoWriteValue(DBNull.Value);
				else DoWriteValue(value);
			}


			protected override void DoWriteDate(DateTime value) {
				DoWriteValue(value);
			}


			protected override void DoWriteImage(Image value) {
				if (value == null) DoWriteValue(DBNull.Value);
				else {
					MemoryStream stream = new MemoryStream();
					try {
						if (value != null) value.Save(stream, value.RawFormat);
						DoWriteValue(stream.GetBuffer());
					} finally {
						stream.Close();
						stream.Dispose();
						stream = null;
					}
				}
			}


			// Advance to the next set of inner objects, erase the previous content, 
			// get the command and prepare for writing
			protected override void DoBeginWriteInnerObjects() {
				++PropertyIndex;
				ValidateInnerObjectsIndex();
				if (!(propertyInfos[PropertyIndex] is EntityInnerObjectsDefinition)) throw new DiagrammingException("Property is not an inner objects property.");
				//
				EntityInnerObjectsDefinition innerInfo = (EntityInnerObjectsDefinition)propertyInfos[PropertyIndex];
				if (AdoNetStore.IsComposition(innerInfo)) {
					innerObjectsWriter = new StringWriter(Cache);
					((StringWriter)innerObjectsWriter).Reset(innerInfo.PropertyDefinitions);
				} else {
					DoDeleteInnerObjects();
					innerObjectsWriter = new DbParameterWriter(store, Cache);
					InnerObjectsWriter.Reset(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).PropertyDefinitions);
					InnerObjectsWriter.Command = store.GetInsertInnerObjectsCommand(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).EntityTypeName);
					InnerObjectsWriter.Command.Transaction = store.transaction;
				}
			}


			protected override void DoEndWriteInnerObjects() {
				if (innerObjectsWriter is StringWriter) {
					((IDataParameter)command.Parameters[PropertyIndex + 1]).Value = ((StringWriter)innerObjectsWriter).StringData;
				} else {
					// Nothing to do
				}
			}


			protected override void DoBeginWriteInnerObject() {
				// Advance to next object
				innerObjectsWriter.Prepare(Entity);
				// An inner object has no id of its own but stores its parent id in the first property.
				if (innerObjectsWriter is DbParameterWriter)
					innerObjectsWriter.WriteId(Entity.Id);
			}


			protected override void DoEndWriteInnerObject() {
				// Commit inner object to data store
				innerObjectsWriter.Finish();
			}


			protected override void DoDeleteInnerObjects() {
				ValidateInnerObjectsIndex();
				if (!(propertyInfos[PropertyIndex] is EntityInnerObjectsDefinition)) throw new DiagrammingException("Property is not an inner objects property.");
				//
				// Delete all existing inner objects of the ObjectRef persistable object			
				IDbCommand command = store.GetDeleteInnerObjectsCommand(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).EntityTypeName);
				command.Transaction = store.CurrentTransaction;
				((IDataParameter)command.Parameters[0]).Value = Entity.Id;
				int count = command.ExecuteNonQuery();
			}

			#endregion


			// Cannot check against the property count, because tables can contain additional 
			// columns, e.g for the parent id and the id.
			private void ValidateFieldIndex() {
				if (PropertyIndex >= command.Parameters.Count)
					throw new DiagrammingException("Field '{0}' of entity cannot be written to the repository because the mapping contains less items. Check whether the SQL commands for this entity are correct.", PropertyIndex);
			}


			// Can check against the property count here, becuause there are no hidden inner objects.
			private void ValidateInnerObjectsIndex() {
				if (PropertyIndex >= propertyInfos.Count)
					throw new DiagrammingException("Inner objects '{0}' of entity cannot be written to the data store because the entity defines less properties.", PropertyIndex);
			}


			private void DoWriteValue(object value) {
				++PropertyIndex;
				ValidateFieldIndex();
				((IDataParameter)command.Parameters[PropertyIndex + 1]).Value = value;
			}


			private DbParameterWriter InnerObjectsWriter {
				get { return (DbParameterWriter)innerObjectsWriter; }
			}


			#region Fields

			private AdoNetStore store;

			private IDbCommand command;

			#endregion
		}
		
		#endregion DbParameterReader/Writer


		#region StringReader / StringWriter

		protected class StringReader : RepositoryReader {

			public StringReader(IStoreCache store)
				: base(store) {
			}


			// Starts reading fields for one type of object
			public void ResetFieldReading(IEnumerable<EntityPropertyDefinition> fieldInfos, string data) {
				base.ResetFieldReading(fieldInfos);
				if (data == null) throw new ArgumentNullException("data");
				str = data;
				p = 0;
			}


			#region RepositoryReader Implementation

			public override void BeginReadInnerObjects() {
				throw new NotSupportedException();
			}


			public override void EndReadInnerObjects() {
				throw new NotSupportedException();
			}


			protected internal override bool DoBeginObject() {
				// Current position is either a valid field start or end of string
				if (p < 0 || p > str.Length) throw new InvalidOperationException("NotSupported string position");
				return p < str.Length;
			}


			protected internal override void DoEndObject() {
				if (str[p] != ';') throw new DiagrammingException("NotSupported string. ';' expected.");
				++p;
			}


			public override void EndReadInnerObject() {
				throw new NotSupportedException();
			}


			protected internal override object ReadId() {
				object result = null;
				// read id type
				if (p < str.Length && str[p] == '(') {
					string typeDesc = string.Empty;
					++p; // skip '('
					while (p < str.Length && str[p] != ')') {
						typeDesc += str[p];
						++p;
					}
					if (p < str.Length && str[p] == ')') ++p;

					if (typeDesc != string.Empty){
						if (typeDesc == typeof(int).Name)
							result = DoReadInt32();
						else if (typeDesc == typeof(long).Name)
							result = DoReadInt64();
						else throw new NotSupportedException();
					}
					if (p < str.Length && str[p] == ',') ++p;
				}
				return result;				
			}


			protected override bool DoReadBool() {
				return (DoReadInteger() != 0);
			}


			protected override byte DoReadByte() {
				long value = DoReadInteger();
				if (value < byte.MinValue || byte.MaxValue < value)
					throw new DiagrammingException("Invalid repository format");
				return (byte)value;
			}


			protected override short DoReadInt16() {
				long value = DoReadInteger();
				if (value < short.MinValue || short.MaxValue < value)
					throw new DiagrammingException("Invalid repository format");
				return (short)value;
			}


			protected override int DoReadInt32() {
				long value = DoReadInteger();
				if (value < int.MinValue || int.MaxValue < value)
					throw new DiagrammingException("Invalid repository format");
				return (int)value;
			}


			protected override long DoReadInt64() {
				return DoReadInteger();
			}


			protected override float DoReadFloat() {
				double value = DoReadDouble();
				if (value < float.MinValue || float.MaxValue < value)
					throw new DiagrammingException("Invalid repository format");
				return (float)value;
			}


			protected override double DoReadDouble() {
				double result = DoReadInteger();
				if (p < str.Length && str[p] == '.') {
					++p;
					int frac = 1;
					while (p < str.Length && str[p] >= '0' && str[p] <= '9') {
						frac *= 10;
						result = result + ((double)str[p] - (double)'0') / frac;
						++p;
					}
					if (p < str.Length && str[p] == ',') ++p;
				}
				return result;
			}


			protected override char DoReadChar() {
				throw new NotSupportedException();
			}


			protected override string DoReadString() {
				throw new NotSupportedException();
			}


			protected override DateTime DoReadDate() {
				throw new NotSupportedException();
			}


			protected override Image DoReadImage() {
				throw new NotSupportedException();
			}

			#endregion


			private long DoReadInteger() {
				long result = 0;
				bool negative;
				if (p < str.Length && str[p] == '-') {
					negative = true;
					++p;
				} else {
					negative = false;
					if (p < str.Length && str[p] == '+') ++p;
				}

				while (p < str.Length && str[p] >= '0' && str[p] <= '9') {
					result = (int)10 * result + (int)str[p] - (int)'0';
					++p;
				}

				if (p < str.Length && str[p] == ',') ++p;
				if (negative) return -result;
				else return result;
			}


			// String data
			private string str;
			// Current position within string data
			private int p;
		}


		protected class StringWriter : RepositoryWriter {

			public StringWriter(IStoreCache store)
				: base(store) {
			}


			protected internal override void Reset(IEnumerable<EntityPropertyDefinition> fieldInfos) {
				base.Reset(fieldInfos);
			}


			protected internal override void Prepare(IEntity entity) {
				base.Prepare(entity);
			}


			protected internal override void Finish() {
				str.Append(';');
				base.Finish();
			}
				
			
			public string StringData {
				get { return str.ToString(); }
			}


			protected override void DoWriteId(object id) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(string.Format("({0}){1}", id.GetType().Name, id));
			}


			protected override void DoWriteBool(bool value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value ? '1' : '0');
			}


			protected override void DoWriteByte(byte value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value.ToString());
			}


			protected override void DoWriteInt16(short value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value.ToString());
			}


			protected override void DoWriteInt32(int value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value.ToString());
			}


			protected override void DoWriteInt64(long value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value.ToString());
			}


			protected override void DoWriteFloat(float value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value.ToString(CultureInfo.InvariantCulture));
			}


			protected override void DoWriteDouble(double value) {
				++PropertyIndex;
				if (PropertyIndex >= 0) str.Append(',');
				str.Append(value.ToString(CultureInfo.InvariantCulture));
			}


			protected override void DoWriteChar(char value) {
				throw new NotSupportedException();
			}


			protected override void DoWriteString(string value) {
				throw new NotSupportedException();
			}


			protected override void DoWriteDate(DateTime date) {
				throw new NotSupportedException();
			}


			protected override void DoWriteImage(Image image) {
				throw new NotSupportedException();
			}


			protected override void DoBeginWriteInnerObjects() {
				throw new NotSupportedException();
			}


			protected override void DoEndWriteInnerObjects() {
				throw new NotSupportedException();
			}


			protected override void DoBeginWriteInnerObject() {
				throw new NotSupportedException();
			}


			protected override void DoEndWriteInnerObject() {
				throw new NotSupportedException();
			}


			protected override void DoDeleteInnerObjects() {
				throw new NotSupportedException();
			}


			private StringBuilder str = new StringBuilder();
		}

		#endregion


		static protected bool IsComposition(EntityPropertyDefinition propertyInfo) {
			return propertyInfo.Name == "ConnectionPointMappings"
				|| propertyInfo.Name == "ValueRanges"
				|| propertyInfo.Name == "Vertices";
		}


		#region Save objects to database

		protected void InsertEntities<TEntity>(IStoreCache cache, IEntityType entityType,
			IEnumerable<KeyValuePair<TEntity, IEntity>> newEntities, 
			FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			InsertEntities<TEntity>(cache, entityType, newEntities, 
				GetCommand(entityType.FullName, RepositoryCommandType.Insert), filterDelegate);
		}


		protected virtual void InsertEntities<TEntity>(IStoreCache cache, 
			IEntityType entityType, IEnumerable<KeyValuePair<TEntity, IEntity>> newEntities,
			IDbCommand dbCommand, FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			// We must remove the new entities from the new dictionary and enter them to 
			// the loaded dictionary afterwards.
			DbParameterWriter repositoryWriter = null;
			foreach (KeyValuePair<TEntity, IEntity> e in newEntities) {
				// Filter out the entities we do not need
				if (filterDelegate != null && !filterDelegate(e.Key, e.Value)) continue;
				//
				// Create repositoryWriter if this is the first new object
				if (repositoryWriter == null) {
					repositoryWriter = new DbParameterWriter(this, cache);
					repositoryWriter.Reset(entityType.PropertyDefinitions);
					repositoryWriter.Command = dbCommand;
					repositoryWriter.Command.Transaction = transaction;
				}
				repositoryWriter.Prepare(e.Key);
				// Write parent id (only ProjectInfo and Design have none)
				if (e.Value != null) repositoryWriter.WriteId(e.Value.Id);
				e.Key.SaveFields(repositoryWriter, entityType.RepositoryVersion);
				int pix = 0;
				// Save all the composite inner objects.
				foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
					if (pi is EntityInnerObjectsDefinition && IsComposition(pi)) {
						repositoryWriter.PropertyIndex = pix - 1;
						e.Key.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
					}
					++pix;
				}
				repositoryWriter.Flush();
				// Now save all the non-composite inner objects.
				pix = 0;
				foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
					if (pi is EntityInnerObjectsDefinition && !IsComposition(pi)) {
						repositoryWriter.PropertyIndex = pix - 1;
						e.Key.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
					}
					++pix;
				}
			}
			repositoryWriter = null;
		}


		/// <summary>
		/// Updates the modified entities against the ADO.NET data provider.
		/// </summary>
		/// <typeparam projectName="TEntity"></typeparam>
		/// <param name="entityType"></param>
		/// <param name="loadedEntities"></param>
		/// <param name="filterDelegate"></param>
		protected virtual void UpdateEntities<TEntity>(IStoreCache cache, 
			IEntityType entityType, IEnumerable<EntityBucket<TEntity>> loadedEntities, 
			FilterDelegate<TEntity> filterDelegate) where TEntity : IEntity {
			List<object> modifiedIds = new List<object>(100);
			DbParameterWriter repositoryWriter = null;
			foreach (EntityBucket<TEntity> ei in loadedEntities) {
				// Filter out the entities we do not need
				if (filterDelegate != null && !filterDelegate(ei.ObjectRef, null)) continue;
				switch (ei.State) {
					// For OwnerChanged, we assume it was modified as well.
					case ItemState.OwnerChanged:
					case ItemState.Modified:
						// Create repositoryWriter if this is the first modified entity
						if (repositoryWriter == null) {
							repositoryWriter = new DbParameterWriter(this, cache);
							repositoryWriter.Command = GetCommand(entityType.FullName, RepositoryCommandType.Update);
							repositoryWriter.Command.Transaction = transaction;
							repositoryWriter.Reset(entityType.PropertyDefinitions);
						}
						((DbParameter)repositoryWriter.Command.Parameters[0]).Value = ei.ObjectRef.Id;
						repositoryWriter.Prepare(ei.ObjectRef);
						repositoryWriter.WriteId(ei.Owner == null? null: ei.Owner.Id);
						ei.ObjectRef.SaveFields(repositoryWriter, version);
						// Save all the composite inner objects.
						foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition && IsComposition(pi))
								ei.ObjectRef.SaveInnerObjects(pi.Name, repositoryWriter, version);
						repositoryWriter.Flush();
						// Now save all the non-composite inner objects.
						foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition && !IsComposition(pi))
								ei.ObjectRef.SaveInnerObjects(pi.Name, repositoryWriter, version);
						modifiedIds.Add(ei.ObjectRef.Id);
						break;
					case ItemState.Deleted:
					case ItemState.Original:
						continue;	// nothing to do
					default:
						throw new DiagrammingUnsupportedValueException(ei.State.GetType(), ei.State);
				}
			}
		}


		/// <summary>
		/// Erases deleted entities of type TEntity from the data store.
		/// </summary>
		/// <typeparam projectName="TEntity"></typeparam>
		/// <param name="entityType"></param>
		/// <param name="loadedEntities"></param>
		protected virtual void DeleteEntities<TEntity>(IStoreCache cache, 
			IEntityType entityType, IEnumerable<EntityBucket<TEntity>> loadedEntities) where TEntity : IEntity {
			// store id's of deleted shapes in this list and remove them after iterating with the IEnumerable enumerator
			List<object> deletedIds = new List<object>(100);
			//
			DbParameterWriter repositoryWriter = null;
			foreach (EntityBucket<TEntity> ei in loadedEntities) {
				if (ei.ObjectRef is TEntity) {
					switch (ei.State) {
						case ItemState.Deleted:
							if (repositoryWriter == null) {
								repositoryWriter = new DbParameterWriter(this, cache);
								repositoryWriter.Reset(entityType.PropertyDefinitions);
								repositoryWriter.Command = GetCommand(entityType.FullName, RepositoryCommandType.Delete);
								repositoryWriter.Command.Transaction = transaction;
							}
							repositoryWriter.Prepare(ei.ObjectRef);
							repositoryWriter.WriteId(ei.ObjectRef.Id);
							ei.ObjectRef.Delete(repositoryWriter, 1);
							repositoryWriter.Flush();
							// add to list of deleted ids
							deletedIds.Add(ei.ObjectRef.Id);
							break;
						case ItemState.OwnerChanged:
						case ItemState.Modified:
						case ItemState.Original:
							continue;
						default:
							throw new DiagrammingUnsupportedValueException(ei.State.GetType(), ei.State);
					}
				}
			}
			deletedIds.Clear();
			deletedIds = null;
		}


		protected void UpdateShapeOwners(IStoreCache cache) {
			foreach (EntityBucket<Shape> eb in cache.LoadedShapes) {
				if (eb.State == ItemState.OwnerChanged) {
					IDbCommand updateOwnerCmd;
					// For a new owner we assume parent shape. 
					if (eb.Owner == null || eb.Owner is Shape)
						updateOwnerCmd = GetCommand("Core.Shape", RepositoryCommandType.UpdateOwnerShape);
					else if (eb.Owner is Diagram)
						updateOwnerCmd = GetCommand("Core.Shape", RepositoryCommandType.UpdateOwnerDiagram);
					else {
						Debug.Fail("Unexpected owner in AdoNetRepository.Update.");
						updateOwnerCmd = null;
					}
					updateOwnerCmd.Transaction = transaction;
					((IDataParameter)updateOwnerCmd.Parameters[0]).Value = ((IEntity)eb.ObjectRef).Id;
					((IDataParameter)updateOwnerCmd.Parameters[1]).Value = ((IEntity)eb.Owner).Id;
					updateOwnerCmd.ExecuteNonQuery();
				}
			}
		}


		protected virtual void InsertShapeConnections(IStoreCache storeCache) {
			IDbCommand command = GetInsertShapeConnectionCommand();
			command.Transaction = transaction;
			foreach (ShapeConnection sc in storeCache.NewShapeConnections) {
				((IDataParameter)command.Parameters[0]).Value = ((IEntity)sc.ConnectorShape).Id;
				((IDataParameter)command.Parameters[1]).Value = sc.GluePointId;
				((IDataParameter)command.Parameters[2]).Value = ((IEntity)sc.TargetShape).Id;
				((IDataParameter)command.Parameters[3]).Value = sc.TargetPointId;
				command.ExecuteNonQuery();
			}
		}


		/// <summary>
		/// Deletes the deleted connections from the database.
		/// </summary>
		protected virtual void DeleteShapeConnections(IStoreCache cache) {
			// Delete command need not be defined if no deleted shape connections exist.
			IDbCommand command = GetDeleteShapeConnectionCommand();
			command.Transaction = transaction;
			foreach (ShapeConnection sc in cache.DeletedShapeConnections) {
				((IDataParameter)command.Parameters[0]).Value = ((IEntity)sc.ConnectorShape).Id;
				command.ExecuteNonQuery();
			}
		}

		#endregion


		protected void OpenCore(IStoreCache cache, bool create) {
			// Check if store is already open
			if (commands.Count > 0) throw new InvalidOperationException(string.Format("{0} is already open.", GetType().Name));
			EnsureDataSourceOpen();
			try {
				LoadSysCommands();
				if (!create) {
					// Read the project's repository version
					IDbCommand cmd = GetCommand(projectInfoEntityTypeName, RepositoryCommandType.SelectByName);
					((IDataParameter)cmd.Parameters[0]).Value = cache.ProjectName;
					using (IDataReader reader = cmd.ExecuteReader()) {
						if (!reader.Read()) throw new InvalidOperationException(string.Format("Project '{0}' not found in ADO.NET repository.", projectName));
						cache.SetRepositoryBaseVersion(reader.GetInt32(1));
					}
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}


		protected bool EnsureDataSourceOpen() {
			bool result;
			if (Connection.State != ConnectionState.Open) {
				Connection.Open();
				result = true;
			} else result = false;
			return result;
		}


		protected struct CommandKey {
			public RepositoryCommandType Kind;
			public string EntityTypeName;
			public override string ToString() {
				return EntityTypeName + "." + Kind.ToString();
			}
		}


		protected struct CommandSet {

			static public CommandSet Empty {
				get {
					CommandSet result;
					result.CreateTableCommand = null;
					result.DeleteCommand = null;
					result.InsertCommand = null;
					result.SelectByIdCommand = null;
					result.SelectByNameCommand = null;
					result.SelectIdsCommand = null;
					result.SelectByOwnerIdCommand = null;
					result.UpdateCommand = null;
					return result;
				}
			}


			public void SetCommand(RepositoryCommandType type, IDbCommand command) {
				if (command == null) throw new ArgumentNullException("command");
				switch (type) {
					case RepositoryCommandType.SelectAll:
						SelectIdsCommand = command; break;
					case RepositoryCommandType.SelectById:
						SelectByIdCommand = command; break;
					case RepositoryCommandType.SelectByName:
						SelectByNameCommand = command; break;
					case RepositoryCommandType.SelectByOwnerId:
						SelectByOwnerIdCommand = command; break;
					case RepositoryCommandType.Update:
						UpdateCommand = command; break;
					case RepositoryCommandType.Delete:
						DeleteCommand = command; break;
					case RepositoryCommandType.Insert:
						InsertCommand = command; break;
					default:
						Debug.Fail("NotSupported command type");
						break;
				}
			}


			public IDbCommand GetCommand(RepositoryCommandType commandType) {
				switch (commandType) {
					case RepositoryCommandType.Delete:
						return DeleteCommand;
					case RepositoryCommandType.Insert:
						return InsertCommand;
					case RepositoryCommandType.SelectAll:
						return SelectIdsCommand;
					case RepositoryCommandType.SelectById:
						return SelectByIdCommand;
					case RepositoryCommandType.SelectByOwnerId:
						return SelectByOwnerIdCommand;
					case RepositoryCommandType.SelectByName:
						return SelectByNameCommand;
					case RepositoryCommandType.Update:
						return UpdateCommand;
					default:
						throw new DiagrammingUnsupportedValueException(typeof(RepositoryCommandType), commandType);
				}
			}


			public IDbCommand CreateTableCommand;
			public IDbCommand SelectIdsCommand;
			public IDbCommand SelectByIdCommand;
			public IDbCommand SelectByNameCommand;
			public IDbCommand SelectByOwnerIdCommand;
			public IDbCommand InsertCommand;
			public IDbCommand UpdateCommand;
			public IDbCommand DeleteCommand;

		}


		/// <summary>
		/// Specifies the projectName of the ADO.NET provider used as listed in the System 
		/// Configuration.
		/// </summary>
		public string ProviderName {
			get { return providerName; }
			set { 
				providerName = value;
				factory = string.IsNullOrEmpty(providerName)? null: DbProviderFactories.GetFactory(providerName);
			}
		}


		/// <summary>
		/// Specifies the connection string for the database store.
		/// </summary>
		public string ConnectionString {
			get { return connectionString; }
			set { connectionString = value; }
		}


		/// <summary>
		/// Override this method to create the actual SQL commands for your database.
		/// </summary>
		public virtual void CreateDbCommands(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			commands.Clear();
		}


		/// <summary>
		/// Creates a schema for the database based on the current DB commands.
		/// </summary>
		/// <remarks>This function has to be regarded more as a testing feature. Real life 
		/// application will usually provide their specialized database schemas and generation
		/// scripts.</remarks>
		public virtual void CreateDbSchema() {
			AssertClosed();
			EnsureDataSourceOpen();
			try {
				// Create the actual schema
				GetCreateTablesCommand().ExecuteNonQuery();
				// Insert the SQL statements into the SysCommand table.
				IDbCommand cmdCmd = GetInsertSysCommandCommand();
				IDbCommand paramCmd = GetInsertSysParameterCommand();
				try {
					cmdCmd.Prepare();
					paramCmd.Prepare();
					foreach (KeyValuePair<CommandKey, IDbCommand> item in commands) {
						((IDbDataParameter)cmdCmd.Parameters[0]).Value = item.Key.Kind;
						((IDbDataParameter)cmdCmd.Parameters[1]).Value = item.Key.EntityTypeName;
						((IDbDataParameter)cmdCmd.Parameters[2]).Value = item.Value.CommandText;
						int cmdId = (int)cmdCmd.ExecuteScalar();
						foreach (IDataParameter p in item.Value.Parameters) {
							((IDbDataParameter)paramCmd.Parameters[0]).Value = cmdId;
							((IDbDataParameter)paramCmd.Parameters[1]).Value = p.ParameterName;
							((IDbDataParameter)paramCmd.Parameters[2]).Value = p.DbType.ToString();
							paramCmd.ExecuteNonQuery();
						}
					}
				} finally {
					paramCmd.Dispose();
					cmdCmd.Dispose();
				}
			} finally {
				EnsureDataSourceClosed();
			}
		}


		public virtual void DropDbSchema() {
			AssertClosed();
			EnsureDataSourceOpen();
			try {
				try {
					LoadSysCommands();
				} catch (DbException exc) {
					Debug.Print(exc.Message);
					// Assumption: No SysCommand table available, i.e. no diagramming tables present
					return;
				}
				IDbCommand dropCommand = GetCommand("All", RepositoryCommandType.Delete);
				dropCommand.Connection = connection;
				dropCommand.ExecuteNonQuery();
			} finally {
				EnsureDataSourceClosed();
			}
		}


		protected void EnsureDataSourceClosed() {
			Connection.Close();
		}


		private IDbConnection Connection {
			get {
				if (connection == null) {
					if (string.IsNullOrEmpty(ProviderName))
						throw new DiagrammingException("ProviderName is not set.");
					if (string.IsNullOrEmpty(ConnectionString))
						throw new DiagrammingException("ConnectionString is not set.");
					connection = factory.CreateConnection();
					connection.ConnectionString = ConnectionString;
				}
				return connection;
			}
		}


		private void AssertOpen()
		{
			// TODO 3: Implement
		}


		private void AssertValid() {
			// TODO 3: Implement
		}


		private void AssertClosed() {
		}


		#region Fields

		protected const string projectInfoEntityTypeName = "AdoNetRepository.ProjectInfo";

		private string projectName;
		private string providerName;
		private string connectionString;
		private DbProviderFactory factory;
		private IDbConnection connection;
		private int version;
		// Currently active transaction
		private IDbTransaction transaction;
		private IDbCommand createTablesCommand;
		// Commands used to load and save against the data store
		private Dictionary<CommandKey, IDbCommand> commands = new Dictionary<CommandKey, IDbCommand>(1000);

		#endregion
	}
}
