using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

using Dataweb.Utilities;
using System.Collections;


namespace Dataweb.Diagramming.Advanced {

	#region Class EntityBucket<TObject>

	public enum ItemState { Original, Modified, OwnerChanged, Deleted, New };

	/// <summary>
	/// Stores a reference to a loaded object together with its state.
	/// </summary>
	/// <typeparam projectName="Type">Type of the object to store</typeparam>
	// EntityBucket is a reference type, because it is entered into dictionaries.
	// Modifying a field of a value type in a dictionary is not possible during
	// an enumeration, but we have to modify at least the State.
	public class EntityBucket<TObject> {

		public EntityBucket(TObject obj, IEntity owner, ItemState state) {
			this.ObjectRef = obj;
			this.Owner = owner;
			this.State = state;
		}

		public TObject ObjectRef;
		public IEntity Owner;
		public ItemState State;
	}

	#endregion


	#region Struct ShapeConnection

	public struct ShapeConnection {

		public static bool operator ==(ShapeConnection x, ShapeConnection y) {
			return (
				x.ConnectorShape == y.ConnectorShape
				&& x.TargetShape == y.TargetShape
				&& x.GluePointId == y.GluePointId
				&& x.TargetPointId == y.TargetPointId);
		}


		public static bool operator !=(ShapeConnection x, ShapeConnection y) { return !(x == y); }


		public ShapeConnection(Diagram diagram, Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId) {
			this.ConnectorShape = connectorShape;
			this.GluePointId = gluePointId;
			this.TargetShape = targetShape;
			this.TargetPointId = targetPointId;
		}


		public override bool Equals(object obj) {
			return obj is ShapeConnection && this == (ShapeConnection)obj;
		}


		public override int GetHashCode() {
			int result = GluePointId.GetHashCode() ^ TargetPointId.GetHashCode();
			if (ConnectorShape != null) result ^= ConnectorShape.GetHashCode();
			if (TargetShape != null) result ^= TargetShape.GetHashCode();
			return result;
		}


		public static readonly ShapeConnection Empty;

		public Shape ConnectorShape;
		public ControlPointId GluePointId;
		public Shape TargetShape;
		public ControlPointId TargetPointId;


		static ShapeConnection() {
			Empty.ConnectorShape = null;
			Empty.GluePointId = ControlPointId.None;
			Empty.TargetShape = null;
			Empty.TargetPointId = ControlPointId.None;
		}

	}

	#endregion


	/// <summary>
	/// Defines a filter function for the loading methods.
	/// </summary>
	/// <typeparam projectName="TEntity"></typeparam>
	/// <param name="entity"></param>
	/// <param name="owner"></param>
	/// <returns></returns>
	public delegate bool FilterDelegate<TEntity>(TEntity entity, IEntity owner);


	/// <summary>
	/// Retrieves the entity with the given id.
	/// </summary>
	/// <param name="pid"></param>
	/// <returns></returns>
	public delegate IEntity Resolver(object pid);


	#region RepositoryReader Class

	/// <summary>
	/// Cache reader for the cached cache.
	/// </summary>
	public abstract class RepositoryReader : IRepositoryReader, IDisposable {

		protected RepositoryReader(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			this.cache = cache;
		}


		#region IRepositoryReader Members

		public abstract void BeginReadInnerObjects();

		public abstract void EndReadInnerObjects();

		public bool BeginReadInnerObject() {
			if (innerObjectsReader == null)
				return DoBeginObject();
			else return innerObjectsReader.BeginReadInnerObject();
		}


		public abstract void EndReadInnerObject();

		public bool ReadBool() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadBool();
			} else return innerObjectsReader.ReadBool();
		}


		public byte ReadByte() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadByte();
			} else return innerObjectsReader.ReadByte();
		}


		public short ReadInt16() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt16();
			} else return innerObjectsReader.ReadInt16();
		}


		public int ReadInt32() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt32();
			} else return innerObjectsReader.ReadInt32();
		}


		public long ReadInt64() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt64();
			} else return innerObjectsReader.ReadInt64();
		}


		public float ReadFloat() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadFloat();
			} else return innerObjectsReader.ReadFloat();
		}


		public double ReadDouble() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadDouble();
			} else return innerObjectsReader.ReadDouble();
		}


		public char ReadChar() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadChar();
			} else return innerObjectsReader.ReadChar();
		}


		public string ReadString() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadString();
			} else return innerObjectsReader.ReadString();
		}


		public DateTime ReadDate() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadDate();
			} else return innerObjectsReader.ReadDate();
		}


		public System.Drawing.Image ReadImage() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadImage();
			} else return innerObjectsReader.ReadImage();
		}


		public Template ReadTemplate() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetTemplate(id);
		}


		public Shape ReadShape() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetShape(id);
		}


		public IModelObject ReadModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetModelObject(id);
		}


		public Design ReadDesign() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDesign(id);
		}


		public ICapStyle ReadCapStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (ICapStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadCapStyle();
		}


		public ICharacterStyle ReadCharacterStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (ICharacterStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadCharacterStyle();
		}


		public IColorStyle ReadColorStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (IColorStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadColorStyle();
		}


		public IFillStyle ReadFillStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (IFillStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadFillStyle();
		}


		public ILineStyle ReadLineStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			IStyle style = cache.GetProjectStyle(id);
			Diagnostics.RuntimeCheck(style is ILineStyle, string.Format("Style {0} is not a line style.", id));
			return (ILineStyle)style;
			} else return innerObjectsReader.ReadLineStyle();
		}


		public IParagraphStyle ReadParagraphStyle() {
			if (innerObjectsReader == null) {
			object id = ReadId();
			if (id == null) return null;
			return (IParagraphStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadParagraphStyle();
		}


		public IShapeStyle ReadShapeStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (IShapeStyle)cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadShapeStyle();
		}

		#endregion


		#region IDisposable Members

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion


		#region Implementation

		// Resets the repositoryReader for a sequence of reads of entities of the same type.
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


		// Reads an id or null, if no id exists.
		protected internal abstract object ReadId();

		/// <summary>
		/// Indicates the current index in the list of property infos of the entity type.
		/// </summary>
		protected internal int PropertyIndex {
			get { return propertyIndex; }
			set { propertyIndex = value; }
		}


		protected IStoreCache Cache {
			get { return cache; }
		}


		protected IEnumerable<EntityPropertyDefinition> PropertyInfos {
			get { return propertyInfos; }
		}


		protected IEntity Object {
			get { return entity; }
			set { entity = value; }
		}


		protected abstract bool DoReadBool();

		protected abstract byte DoReadByte();

		protected abstract short DoReadInt16();

		protected abstract int DoReadInt32();

		protected abstract long DoReadInt64();

		protected abstract float DoReadFloat();

		protected abstract double DoReadDouble();

		protected abstract char DoReadChar();

		protected abstract string DoReadString();

		protected abstract DateTime DoReadDate();

		protected abstract System.Drawing.Image DoReadImage();


		protected Template DoReadTemplate() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetTemplate(id);
		}


		protected Shape DoReadShape() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetShape(id);
		}


		protected IModelObject DoReadModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetModelObject(id);
		}


		protected Design DoReadDesign() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDesign(id);
		}


		protected ICapStyle DoReadCapStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ICapStyle)cache.GetProjectStyle(id);
		}


		protected ICharacterStyle DoReadCharacterStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ICharacterStyle)cache.GetProjectStyle(id);
		}


		protected IColorStyle DoReadColorStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IColorStyle)cache.GetProjectStyle(id);
		}


		protected IFillStyle DoReadFillStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IFillStyle)cache.GetProjectStyle(id);
		}


		protected ILineStyle DoReadLineStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ILineStyle)cache.GetProjectStyle(id);
		}


		protected IParagraphStyle DoReadParagraphStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IParagraphStyle)cache.GetProjectStyle(id);
		}


		protected IShapeStyle DoReadShapeStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IShapeStyle)cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Checks whether the current property index refers to a valid entity field.
		/// </summary>
		protected virtual void ValidatePropertyIndex() {
			// We cannot check propertyIndex < 0 because some readers use PropertyIndex == -1 for the id.
			if (propertyIndex >= propertyInfos.Count)
				throw new DiagrammingException("An entity tries to read more properties from the repository than there are defined.");
		}


		protected virtual void Dispose(bool disposing) {
			// Nothing to do
		}

		#endregion


		#region Fields

		private IStoreCache cache;
		protected List<EntityPropertyDefinition> propertyInfos = new List<EntityPropertyDefinition>(20);
		private int propertyIndex;
		protected RepositoryReader innerObjectsReader;
		// used for loading innerObjects
		private IEntity entity;

		#endregion
	}

	#endregion


	#region RepositoryWriter

	/// <summary>
	/// Offline RepositoryWriter
	/// </summary>
	public abstract class RepositoryWriter : IRepositoryWriter {

		protected RepositoryWriter(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			this.cache = cache;
		}


		#region IRepositoryWriter Members

		public void WriteId(object id) {
			if (innerObjectsWriter == null) DoWriteId(id);
			else innerObjectsWriter.WriteId(id);
		}


		public void WriteBool(bool value) {
			if (innerObjectsWriter == null) DoWriteBool(value);
			else innerObjectsWriter.WriteBool(value);
		}


		public void WriteByte(byte value) {
			if (innerObjectsWriter == null) DoWriteByte(value);
			else innerObjectsWriter.WriteByte(value);
		}


		public void WriteInt16(short value) {
			if (innerObjectsWriter == null) DoWriteInt16(value);
			else innerObjectsWriter.WriteInt16(value);
		}


		public void WriteInt32(int value) {
			if (innerObjectsWriter == null) DoWriteInt32(value);
			else innerObjectsWriter.WriteInt32(value);
		}


		public void WriteInt64(long value) {
			if (innerObjectsWriter == null) DoWriteInt64(value);
			else innerObjectsWriter.WriteInt64(value);
		}


		public void WriteFloat(float value) {
			if (innerObjectsWriter == null) DoWriteFloat(value);
			else innerObjectsWriter.WriteFloat(value);
		}


		public void WriteDouble(double value) {
			if (innerObjectsWriter == null) DoWriteDouble(value);
			else innerObjectsWriter.WriteDouble(value);
		}


		public void WriteChar(char value) {
			if (innerObjectsWriter == null) DoWriteChar(value);
			else innerObjectsWriter.WriteChar(value);
		}


		public void WriteString(string value) {
			if (innerObjectsWriter == null) DoWriteString(value);
			else innerObjectsWriter.WriteString(value);
		}


		public void WriteDate(DateTime value) {
			if (innerObjectsWriter == null) DoWriteDate(value);
			else innerObjectsWriter.WriteDate(value);
		}


		public void WriteImage(System.Drawing.Image image) {
			if (innerObjectsWriter == null) DoWriteImage(image);
			else innerObjectsWriter.WriteImage(image);
		}


		public void WriteTemplate(Template template) {
			if (template != null && template.Id == null) throw new InvalidOperationException(
				string.Format("Template '{0}' is not registered with repository.", template.Name));
			if (innerObjectsWriter == null) {
				if (template == null) WriteId(null);
				else WriteId(template.Id);
			} else innerObjectsWriter.WriteTemplate(template);
		}


		public void WriteStyle(IStyle style) {
			if (style != null && style.Id == null) throw new InvalidOperationException(
				 string.Format("{0} '{1}' is not registered with the repository.", style.GetType().Name, style.Name));
			if (innerObjectsWriter == null) {
				if (style == null) WriteId(null);
				else WriteId(style.Id);
			} else innerObjectsWriter.WriteStyle(style);
		}


		public void WriteModelObject(IModelObject modelObject) {
			if (modelObject != null && modelObject.Id == null) throw new InvalidOperationException(
				 string.Format("{0} '{1}' is not registered with the repository.", modelObject.Type.FullName, modelObject.Name));
			if (innerObjectsWriter == null) {
				Debug.Assert(modelObject == null || modelObject.Id != null);
				if (modelObject == null) WriteId(null);
				else WriteId(modelObject.Id);
			} else innerObjectsWriter.WriteModelObject(modelObject);
		}


		public void BeginWriteInnerObjects() {
			if (innerObjectsWriter != null)
				throw new InvalidOperationException("Call EndWriteInnerObjects before a new call to BeginWriteInnerObjects.");
			DoBeginWriteInnerObjects();
		}


		public void EndWriteInnerObjects() {
			if (innerObjectsWriter == null)
				throw new InvalidOperationException("BeginWriteInnerObjects has not been called.");
			DoEndWriteInnerObjects();
			innerObjectsWriter = null;
		}


		public void BeginWriteInnerObject() {
			// Must be executed by the outer writer. Currently there is only one inner 
			// and one outer.
			DoBeginWriteInnerObject();
		}


		public void EndWriteInnerObject() {
			// Must be executed by the outer writer. Currently there is only one inner 
			// and one outer.
			DoEndWriteInnerObject();
		}


		public void DeleteInnerObjects() {
			if (innerObjectsWriter == null) {
				++PropertyIndex;
				DoDeleteInnerObjects();
			} else innerObjectsWriter.DeleteInnerObjects();
		}

		#endregion


		protected IStoreCache Cache {
			get { return cache; }
		}


		protected abstract void DoWriteId(object id);

		protected abstract void DoWriteBool(bool value);

		protected abstract void DoWriteByte(byte value);

		protected abstract void DoWriteInt16(short value);

		protected abstract void DoWriteInt32(int value);

		protected abstract void DoWriteInt64(long value);

		protected abstract void DoWriteFloat(float value);

		protected abstract void DoWriteDouble(double value);

		protected abstract void DoWriteChar(char value);

		protected abstract void DoWriteString(string value);

		protected abstract void DoWriteDate(DateTime date);

		protected abstract void DoWriteImage(System.Drawing.Image image);

		protected abstract void DoBeginWriteInnerObjects();

		protected abstract void DoEndWriteInnerObjects();

		// Must be called upon the outer cache writer.
		protected abstract void DoBeginWriteInnerObject();

		// Must be called upon the outer cache writer.
		protected abstract void DoEndWriteInnerObject();

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


		protected internal virtual void Finish() {
			// Nothing to do
		}


		protected internal int PropertyIndex {
			get { return propertyIndex; }
			set { propertyIndex = value; }
		}


		protected IEntity Entity {
			get { return entity; }
		}


		#region Fields

		private IStoreCache cache;

		// Current entity to write. Null when writing an inner object
		private IEntity entity;

		// Description of the entity type currently writting
		protected List<EntityPropertyDefinition> propertyInfos = new List<EntityPropertyDefinition>(20);

		// Index of property currently being written
		private int propertyIndex;

		// When writing inner objects, reference to the responsible writer
		protected RepositoryWriter innerObjectsWriter;

		#endregion
	}

	#endregion


	/// <summary>
	/// Caches modifications to the cache and commits them to the data store
	/// during SaveChanges.
	/// </summary>
	public class CachedRepository : Component, IRepository, IStoreCache {

		public Store Store {
			get { return this.store; }
			set {
				AssertClosed();
				this.store = value;
				if (this.store != null && !string.IsNullOrEmpty(this.projectName)) 
					this.store.ProjectName = projectName;
			}
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


		#region IRepository Members

		public int Version {
			get { return version; }
			set { version = value; }
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
				throw new DiagrammingException("The repository already contains an entity type called '{0}'.", entityType.FullName);
			foreach (KeyValuePair<string, IEntityType> item in entityTypes) {
				if (item.Value.FullName.Equals(entityType.FullName, StringComparison.InvariantCultureIgnoreCase))
					throw new DiagrammingException("The repository already contains an entity type called '{0}'.", entityType.FullName);
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


		/// <overrice></overrice>
		public bool Exists() {
			return store != null && store.Exists();
		}


		/// <override></override>
		public virtual void Create() {
			AssertClosed();
			if (string.IsNullOrEmpty(projectName)) throw new DiagrammingException("No project name defined.");
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
			if (string.IsNullOrEmpty(projectName)) throw new DiagrammingException("No project name defined.");
			if (store == null) throw new InvalidOperationException("Repository has no store attached. An in-memory repository must be created, not opened.");
			store.ProjectName = projectName;
			store.Open(this);
			// Load the project, must be exactly one.
			store.LoadProjects(this, FindEntityType(ProjectSettings.EntityTypeName, true));
			IEnumerator<EntityBucket<ProjectSettings>> projectEnumerator = projects.Values.GetEnumerator();
			if (!projectEnumerator.MoveNext())
				throw new DiagrammingException("Project '{0}' not found in repository.", projectName);
			settings = projectEnumerator.Current.ObjectRef;
			if (projectEnumerator.MoveNext())
				throw new DiagrammingException("Two projects named '{0}' found in repository.", projectName);
			// Load the design, there must be exactly one returned
			store.LoadDesigns(this, ((IEntity)settings).Id);
			IEnumerator<EntityBucket<Design>> designEnumerator = designs.Values.GetEnumerator();
			if (!designEnumerator.MoveNext())
				throw new DiagrammingException("Project styles not found.");
			projectDesign = designEnumerator.Current.ObjectRef;
			if (designEnumerator.MoveNext())
				throw new DiagrammingException("More than one project design found in repository.");
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


		#region Project
		
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


		public event EventHandler<RepositoryProjectEventArgs> ProjectUpdated;

		public event EventHandler<RepositoryProjectEventArgs> ProjectDeleted;

		#endregion


		#region Model

		/// <override></override>
		public Model GetModel() {
			AssertOpen();
			if (models.Count <= 0 && ((IEntity)settings).Id != null)
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


		public event EventHandler<RepositoryModelEventArgs> ModelInserted;

		public event EventHandler<RepositoryModelEventArgs> ModelUpdated;

		public event EventHandler<RepositoryModelEventArgs> ModelDeleted;

		#endregion


		#region Templates

		/// <override></override>
		// We assume that this is only called once to load all existing templates.
		public IEnumerable<Template> GetTemplates() {
			AssertOpen();
			if (((IEntity)settings).Id != null && templates.Count <= 0)
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
			InsertEntity<Shape>(newShapes, template.Shape, template);
			if (template.Shape.ModelObject != null)
				InsertEntity<IModelObject>(newModelObjects, template.Shape.ModelObject, template);
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public Template GetTemplate(object id) {
			if (id == null) throw new ArgumentNullException("id");
			EntityBucket<Template> result = null;
			AssertOpen();
			if (!templates.TryGetValue(id, out result)) {
				store.LoadTemplates(this, ((IEntity)settings).Id);
				if (!templates.TryGetValue(id, out result))
					throw new DiagrammingException("Template with id '{0}' not found in store.", id);
			}
			return result.ObjectRef;
		}


		/// <override></override>
		public Template GetTemplate(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertOpen();
			if (templates.Count <= 0)
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
			if (TemplateUpdated != null) TemplateUpdated(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void ReplaceTemplateShape(Template template, Shape oldShape, Shape newShape) {
			if (template == null) throw new ArgumentNullException("template");
			if (oldShape == null) throw new ArgumentNullException("oldShape");
			if (newShape == null) throw new ArgumentNullException("newShape");
			AssertOpen();
			InsertShape(newShape, template);
			UpdateEntity<Template>(templates, newTemplates, template);
			DeleteShape(oldShape);
			if (TemplateShapeReplaced != null) TemplateShapeReplaced(this, GetTemplateShapeExchangedEventArgs(template, oldShape, newShape));
		}


		/// <override></override>
		public void DeleteTemplate(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			// Delete the template's shape
			DeleteEntity<Shape>(shapes, newShapes, template.Shape);
			DeleteEntity<Template>(templates, newTemplates, template);
			if (TemplateDeleted != null) TemplateDeleted(this, GetTemplateEventArgs(template));
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

		public void InsertModelMapping(IModelMapping modelMapping, Template template) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			if (template == null) throw new ArgumentNullException("template");
			DoInsertModelMapping(modelMapping, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}

		public void InsertModelMappings(IEnumerable<IModelMapping> modelMappings, Template template) {
			if (modelMappings == null) throw new ArgumentNullException("modelMappings");
			if (template == null) throw new ArgumentNullException("template");
			foreach (IModelMapping modelMapping in modelMappings)
				DoInsertModelMapping(modelMapping, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}

		public void UpdateModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			DoUpdateModelMapping(modelMapping);
			if (ModelMappingsUpdated != null) ModelMappingsUpdated(this, 
				GetTemplateEventArgs(GetModelMappingOwner(modelMapping)));
		}

		public void UpdateModelMapping(IEnumerable<IModelMapping> modelMappings) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			Template owner = null;
			foreach (IModelMapping modelMapping in modelMappings) {
				DoUpdateModelMapping(modelMapping);
				if (owner == null) owner = GetModelMappingOwner(modelMapping);
				else if (owner != GetModelMappingOwner(modelMapping)) 
					throw new DiagrammingException("Invalid model mapping owner.");
			}
			if (ModelMappingsUpdated != null) ModelMappingsUpdated(this, GetTemplateEventArgs(owner));
		}

		public void DeleteModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			Template owner = GetModelMappingOwner(modelMapping);
			DoDeleteModelMapping(modelMapping);
			if (ModelMappingsDeleted != null) ModelMappingsDeleted(this, GetTemplateEventArgs(owner));
		}

		public void DeleteModelMappings(IEnumerable<IModelMapping> modelMappings) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			Template owner = null;
			foreach (IModelMapping modelMapping in modelMappings) {
				DoDeleteModelMapping(modelMapping);
				if (owner == null) owner = GetModelMappingOwner(modelMapping);
				else if (owner != GetModelMappingOwner(modelMapping))
					throw new DiagrammingException("Invalid model mapping owner.");
			}
			if (ModelMappingsDeleted != null) ModelMappingsDeleted(this, GetTemplateEventArgs(owner));
		}

		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsInserted;

		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsUpdated;

		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsDeleted;

		#endregion


		#region Diagrams

		/// <override></override>
		public IEnumerable<Diagram> GetDiagrams() {
			AssertOpen();
			if (diagrams.Count <= 0 && ((IEntity)settings).Id != null)
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
				store.LoadDiagrams(this, ((IEntity)settings).Id);
				if (!diagrams.TryGetValue(id, out result))
					throw new DiagrammingException("Diagram with id '{0}' not found in repository.", id);
			}
			return result.ObjectRef;
		}


		/// <override></override>
		public Diagram GetDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertOpen();
			// If there is a diagram, we assume we have already loaded them all.
			if (diagrams.Count <= 0 && ((IEntity)settings).Id != null)
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
				DeleteShape(s);
			// Now we can delete the actual diagram
			DeleteEntity<Diagram>(diagrams, newDiagrams, diagram);
			//
			if (DiagramDeleted != null) DiagramDeleted(this, GetDiagramEventArgs(diagram));
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
			store.LoadDiagramShapes(this, diagram);
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
			// --------------------------------------
			// @@Kurt: Einfügen der Kinder
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children.BottomUp)
					DoInsertShape(childShape, shape);
			}
			// --------------------------------------
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
			// --------------------------------------
			// @@Kurt: Löschen der Kindern
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					DoDeleteShape(childShape);
			}
			// --------------------------------------
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


		// === ShapeConnections ===

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
			newShapeConnections.Remove(connection);
			deletedShapeConnections.Add(connection);
			isModified = true;
		}


		#region ModelObjects

		private EntityBucket<IModelObject> GetModelObjectItem(object id) {
			if (id == null) throw new DiagrammingException("ModelObject has no identifier.");
			EntityBucket<IModelObject> item;
			if (!modelObjects.TryGetValue(id, out item))
				throw new DiagrammingException(string.Format("ModelObject {0} not found.", id));
			return item;
		}


		/// <override></override>
		// TODO 2: Should be similar to GetShape. Unify?
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
				if (model != null) store.LoadModelModelObjects(this, model.Id);
				if (modelObjects.TryGetValue(id, out moeb))
					result = moeb.ObjectRef;
			}
			if (result == null) throw new DiagrammingException("Model object with id '{0}' not found in repository.", id);
			return result;
		}


		public IEnumerable<IModelObject> GetModelObjects(IModelObject parent) {
			AssertOpen();
			if (((IEntity)settings).Id != null && modelObjects.Count == 0) {
				if (parent == null) {
					Model model = GetModel();
					if (model != null) store.LoadModelModelObjects(this, model.Id);
				} else store.LoadChildModelObjects(this, ((IEntity)parent).Id);
			}
			foreach (EntityBucket<IModelObject> mob in modelObjects) {
				if (mob.ObjectRef.Parent == parent) yield return mob.ObjectRef;
			}
			foreach (KeyValuePair<IModelObject, IEntity> item in newModelObjects)
				if (item.Key.Parent == parent) yield return item.Key;
		}


		public void InsertModelObject(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoInsertModelObject(modelObject);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		public void InsertModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			// ToDo: find a smarter way of inserting multiple objects
			foreach (IModelObject modelObject in modelObjects)
				DoInsertModelObject(modelObject);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		public void UpdateModelObjectParent(IModelObject modelObject, IModelObject parent) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			DoUpdateModelObjectParent(modelObject, parent);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, GetModelObjectsEventArgs(modelObject));
		}
		
		
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


		public void UpdateModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				DoUpdateModelObject(modelObject);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, e);
		}


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


		public void DeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				DoDeleteModelObject(modelObject);
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		public void UnloadModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			foreach (IModelObject mo in modelObjects) {
				// TODO 2: Should we allow to remove from new model objects?
				if (mo.Id == null) newModelObjects.Remove(mo);
				else this.modelObjects.Remove(mo.Id);
			}
		}


		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsInserted;

		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsUpdated;

		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		#endregion


		#region Designs

		/// <override></override>
		public IEnumerable<Design> GetDesigns() {
			AssertOpen();
			if (designs.Count <= 0)
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
				if (designs.Count <= 0 && store != null)
					store.LoadDesigns(this, null);
				if (!designs.TryGetValue(id, out designBucket))
					throw new DiagrammingException("Design with id '{0}' not found in repository.", id);
				result = designBucket.ObjectRef;
			}
			return result;
		}


		/// <override></override>
		public void InsertDesign(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			if (((IEntity)design).Id != null) throw new DiagrammingException("Can only insert new designs.");
			if (newDesigns.ContainsKey(design)) throw new DiagrammingException("Design already exists in the repository.");
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


		public event EventHandler<RepositoryDesignEventArgs> DesignInserted;

		public event EventHandler<RepositoryDesignEventArgs> DesignUpdated;

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
		public void DeleteStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			DeleteEntity<IStyle>(styles, newStyles, style);
			if (StyleDeleted != null) StyleDeleted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void UpdateStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			UpdateEntity<IStyle>(styles, newStyles, style);
			if (StyleUpdated != null) StyleUpdated(this, GetStyleEventArgs(style));
		}


		public event EventHandler<RepositoryStyleEventArgs> StyleInserted;

		public event EventHandler<RepositoryStyleEventArgs> StyleUpdated;

		public event EventHandler<RepositoryStyleEventArgs> StyleDeleted;

		#endregion

		#endregion


		#region IStoreCache Members

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
		/// Calculates an XML tag projectName for the given entity projectName.
		/// </summary>
		/// <param name="elementTag"></param>
		/// <returns></returns>
		static protected string CalcElementName(string entityName) {
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
			// Not yet, must use prefix plus projectName in order to do that
			// result = result.ReplaceRange('.', ':');
			return stringBuilder.ToString();
		}


		/// <summary>
		/// Retrieves the indicated project style, which is always loaded when the project 
		/// is open.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected IStyle GetProjectStyle(object id) {
			EntityBucket<IStyle> styleItem;
			if (!styles.TryGetValue(id, out styleItem))
				throw new DiagrammingException("Style with id '{0}' does not exist.", id);
			return styleItem.ObjectRef;
		}


		/// <summary>
		/// Inserts an entity into the internal cache and marks it as new.
		/// </summary>
		/// <typeparam projectName="TEntity"></typeparam>
		/// 
		/// <param name="newEntities"></param>
		/// <param name="entity"></param>
		/// <param name="owner"></param>
		protected void InsertEntity<TEntity>(Dictionary<TEntity, IEntity> newEntities, 
			TEntity entity, IEntity owner) where TEntity : IEntity {
			if (entity.Id != null)
				throw new ArgumentException("Entities with an id cannot be inserted into the repository.");
			newEntities.Add(entity, owner);
			isModified = true;
		}
		

		/// <summary>
		/// Updates an entity in the internal cache and marks it as modified.
		/// </summary>
		/// <typeparam projectName="TEntity"></typeparam>
		/// <param name="loadedEntities"></param>
		/// <param name="newEntities"></param>
		/// <param name="entity"></param>
		protected void UpdateEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			Dictionary<TEntity, IEntity> newEntities, TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) {
				if (!newEntities.ContainsKey(entity))
					throw new DiagrammingException(string.Format("Entity not found in Repository."));
			} else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new DiagrammingException("Entity not found in repository.");
				item.State = ItemState.Modified;
			}
			isModified = true;
		}


		/// <summary>
		/// Marks the entity for deletion from the data store. 
		/// Must be called after all children have been removed.
		/// </summary>
		/// <typeparam projectName="TEntity"></typeparam>
		/// <param name="loadedEntities"></param>
		/// <param name="newEntities"></param>
		/// <param name="entity"></param>
		protected void DeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			Dictionary<TEntity, IEntity> newEntities, TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) {
				if (!newEntities.ContainsKey(entity))
					throw new DiagrammingException(string.Format("Entity not found in repository.", entity.Id));
				newEntities.Remove(entity);
			} else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new DiagrammingException("Entity not found in repository.");
				item.State = ItemState.Deleted;
			}
			isModified = true;
		}


		protected void AcceptEntities<EntityType>(Dictionary<object, EntityBucket<EntityType>> loadedEntities,
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
				if (item.Value.State != ItemState.Original)
					loadedEntities[item.Key].State = ItemState.Original;
			}

			// Move new entities from newEntities to loadedEntities
			foreach (KeyValuePair<EntityType, IEntity> ep in newEntities) {
				// Settings do not have a parent
				loadedEntities.Add(ep.Key.Id, new EntityBucket<EntityType>(ep.Key, ep.Value, ItemState.Original));
			}
			newEntities.Clear();
		}


		protected void AcceptAll() {
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
		}


		/// <summary>
		/// Defines a dictionary for loaded entity types.
		/// </summary>
		/// <typeparam projectName="TEntity"></typeparam>
		protected class LoadedEntities<TEntity> : Dictionary<object, EntityBucket<TEntity>>, 
			ICacheCollection<TEntity> where TEntity: IEntity {

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


		protected void DoInsertDesign(Design design, ProjectSettings projectData) {
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
			//foreach (IStyle style in styleSetProvider.StyleSet.ShapeStyles)
			//   InsertEntity<IStyle>(styles, newStyles, style, design);
			foreach (IStyle style in styleSet.ParagraphStyles)
				InsertEntity<IStyle>(newStyles, style, design);
			isModified = true;
		}


		#region ProjectOwner 

		/// <summary>
		/// Serves as a parent entity for the project info.
		/// </summary>
		protected class ProjectOwner : IEntity {

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
		

		#region Implementation

		protected void ClearBuffers() {
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


		protected IEntityType FindEntityType(string entityTypeName, bool mustExist) {
			IEntityType result;
			entityTypes.TryGetValue(CalcElementName(entityTypeName), out result);
			if (mustExist && result == null)
				throw new DiagrammingException("Entity type '{0}' does not exist in the repository.", entityTypeName);
			return result;
		}


		protected IEnumerable<TEntity> GetCachedEntities<TEntity>(LoadedEntities<TEntity> loadedEntities, 
			IDictionary<TEntity, IEntity> newEntities) where TEntity: IEntity {
			foreach (EntityBucket<TEntity> eb in loadedEntities)
				yield return eb.ObjectRef;
			foreach (KeyValuePair<TEntity, IEntity> item in newEntities)
				yield return item.Key;
		}


		protected void AssertOpen() {
			if (!isOpen) throw new DiagrammingException("Repository is not open.");
			Debug.Assert(settings != null && projectDesign != null);
		}


		protected void AssertClosed() {
			if (isOpen) throw new DiagrammingException("Repository is already open.");
		}


		/// <summary>
		/// Tests the invariants of the offline cache object.
		/// </summary>
		protected void AssertValid() {
			// project projectData are defined for an open project.
			Debug.Assert(!isOpen || GetProject() != null);
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


		private Template GetModelMappingOwner(IModelMapping modelMapping) {
			Template owner = null;
			if (modelMapping.Id == null) {
				Debug.Assert(newModelMappings.ContainsKey(modelMapping));
				Debug.Assert(newModelMappings[modelMapping] is Template);
				owner = (Template)newModelMappings[modelMapping];
			} else {
				Debug.Assert(newModelMappings[modelMapping] is Template);
				owner = (Template)modelMappings[modelMapping.Id].Owner;
			}
			return owner;
		}
		
		
		private void DoInsertShape(Shape shape, IEntity parentEntity) {
			InsertEntity<Shape>(newShapes, shape, parentEntity);
		}


		private void DoUpdateShape(Shape shape) {
			UpdateEntity<Shape>(shapes, newShapes, shape);
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
			// We must delete the shape's connections first
			foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null))
				if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
					DeleteShapeConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
			DeleteEntity<Shape>(shapes, newShapes, shape);
		}


		private void DoInsertModelObject(IModelObject modelObject) {
			IEntity owner;
			if (modelObject.Parent != null) owner = modelObject.Parent;
			else owner = GetModel();
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


		#region Methods for retrieving EventArgs

		protected RepositoryProjectEventArgs GetProjectEventArgs(ProjectSettings projectData) {
			projectEventArgs.Project = projectData;
			return projectEventArgs;
		}


		protected RepositoryModelEventArgs GetModelEventArgs(Model model) {
			modelEventArgs.Model = model;
			return modelEventArgs;
		}
		
		
		protected RepositoryDesignEventArgs GetDesignEventArgs(Design design) {
			designEventArgs.Design = design;
			return designEventArgs;
		}


		protected RepositoryStyleEventArgs GetStyleEventArgs(IStyle style) {
			styleEventArgs.Style = style;
			return styleEventArgs;
		}
		
		
		protected RepositoryDiagramEventArgs GetDiagramEventArgs(Diagram diagram) {
			diagramEventArgs.Diagram = diagram;
			return diagramEventArgs;
		}


		protected RepositoryTemplateEventArgs GetTemplateEventArgs(Template template) {
			templateEventArgs.Template = template;
			return templateEventArgs;
		}


		protected RepositoryTemplateShapeReplacedEventArgs GetTemplateShapeExchangedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape) {
			templateShapeExchangedEventArgs.Template = template;
			templateShapeExchangedEventArgs.OldTemplateShape = oldTemplateShape;
			templateShapeExchangedEventArgs.NewTemplateShape = newTemplateShape;
			return templateShapeExchangedEventArgs;
		}


		protected RepositoryShapesEventArgs GetShapesEventArgs(Shape shape) {
			Diagram diagram;
			if (((IEntity)shape).Id == null)
				diagram = newShapes[shape] as Diagram;
			else diagram = shapes[((IEntity)shape).Id].Owner as Diagram;
			shapeEventArgs.SetShape(shape, diagram);
			return shapeEventArgs;
		}
		
		
		protected RepositoryShapesEventArgs GetShapesEventArgs(Shape shape, Diagram diagram) {
			shapeEventArgs.SetShape(shape, diagram);
			return shapeEventArgs;
		}


		protected RepositoryShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
			shapeEventArgs.SetShapes(shapes, diagram);
			return shapeEventArgs;
		}


		protected RepositoryShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes) {
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


		protected RepositoryModelObjectsEventArgs GetModelObjectsEventArgs(IModelObject modelObject) {
			modelObjectEventArgs.SetModelObject(modelObject);
			return modelObjectEventArgs;
		}


		protected RepositoryModelObjectsEventArgs GetModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			modelObjectEventArgs.SetModelObjects(modelObjects);
			return modelObjectEventArgs;
		}


		#endregion


		#region Fields

		// project info is an internal entity type
		protected const string projectInfoEntityTypeName = "ProjectInfo";

		// Used to calculate the element entityTypeName
		static private StringBuilder stringBuilder = new StringBuilder();

		// True, when Open was successfully called. Is identical to store.IsOpen if store 
		// is defined.
		private bool isOpen;

		/// <summary>
		/// True, when modfications have been done to any part of the projects since
		/// Open or SaveChanges. 
		/// </summary>
		protected bool isModified;

		/// <summary>
		/// Reference to the open project for easier access.
		/// </summary>
		protected ProjectSettings settings;

		/// <summary>
		/// Indicates the pseudo design used to manage the styles of the project.
		/// This design is not entered in the designs or newDesigns dictionaries.
		/// </summary>
		protected Design projectDesign;

		private int version;

		// Name of the project
		private string projectName = null;

		// Store for cache data. Is null, if no store is assigned to open
		// cache, i.e. the cache is in-memory.
		private Store store;

		// DirectoryName of registered entities
		protected Dictionary<string, IEntityType> entityTypes = new Dictionary<string, IEntityType>();

		// Containers for loaded objects
		// TODO 2: Try to make private
		protected LoadedEntities<ProjectSettings> projects = new LoadedEntities<ProjectSettings>();
		protected LoadedEntities<Model> models = new LoadedEntities<Model>();
		protected LoadedEntities<Design> designs = new LoadedEntities<Design>();
		protected LoadedEntities<IStyle> styles = new LoadedEntities<IStyle>();
		protected LoadedEntities<Diagram> diagrams = new LoadedEntities<Diagram>();
		protected LoadedEntities<Template> templates = new LoadedEntities<Template>();
		protected LoadedEntities<IModelMapping> modelMappings = new LoadedEntities<IModelMapping>();
		protected LoadedEntities<Shape> shapes = new LoadedEntities<Shape>();
		protected LoadedEntities<IModelObject> modelObjects = new LoadedEntities<IModelObject>();

		// TODO 2: Try to make private
		protected List<ShapeConnection> newShapeConnections = new List<ShapeConnection>();
		protected List<ShapeConnection> deletedShapeConnections = new List<ShapeConnection>();

		// Containers for new entities
		// Stores the new entity as the key and its parent as the value.
		// (New objects do not yet have an id and are therefore not addressable in the dictionary.)
		// TODO 2: Try to make private
		protected Dictionary<ProjectSettings, IEntity> newProjects = new Dictionary<ProjectSettings, IEntity>();
		protected Dictionary<Model, IEntity> newModels = new Dictionary<Model, IEntity>();
		protected Dictionary<Design, IEntity> newDesigns = new Dictionary<Design, IEntity>();
		protected Dictionary<IStyle, IEntity> newStyles = new Dictionary<IStyle, IEntity>();
		protected Dictionary<Diagram, IEntity> newDiagrams = new Dictionary<Diagram, IEntity>();
		protected Dictionary<Template, IEntity> newTemplates = new Dictionary<Template, IEntity>();
		protected Dictionary<IModelMapping, IEntity> newModelMappings = new Dictionary<IModelMapping, IEntity>();
		protected Dictionary<Shape, IEntity> newShapes = new Dictionary<Shape, IEntity>();
		protected Dictionary<IModelObject, IEntity> newModelObjects = new Dictionary<IModelObject, IEntity>();

		// EventArg Buffers
		protected RepositoryProjectEventArgs projectEventArgs = new RepositoryProjectEventArgs();
		protected RepositoryModelEventArgs modelEventArgs = new RepositoryModelEventArgs();
		protected RepositoryDesignEventArgs designEventArgs = new RepositoryDesignEventArgs();
		protected RepositoryStyleEventArgs styleEventArgs = new RepositoryStyleEventArgs();
		protected RepositoryDiagramEventArgs diagramEventArgs = new RepositoryDiagramEventArgs();
		protected RepositoryTemplateEventArgs templateEventArgs = new RepositoryTemplateEventArgs();
		protected RepositoryTemplateShapeReplacedEventArgs templateShapeExchangedEventArgs = new RepositoryTemplateShapeReplacedEventArgs();
		protected RepositoryShapesEventArgs shapeEventArgs = new RepositoryShapesEventArgs();
		protected RepositoryModelObjectsEventArgs modelObjectEventArgs = new RepositoryModelObjectsEventArgs();

		// project needs an owner for the newProjects dictionary
		protected ProjectOwner projectOwner = new ProjectOwner();

		#endregion
	}

}