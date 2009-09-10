using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Text;

using Dataweb.nShape.Advanced;
using System.ComponentModel;


namespace Dataweb.nShape {

	/// <summary>
	/// Uses an XML file as the data store.
	/// </summary>
	/// <remarks>XML capability should go to CachedRepository or even higher. So we 
	/// can create the XML document from any cache. Only responsibilities left 
	/// here, is how ids are generated.</remarks>
	public class XmlStore : Store {

		public XmlStore() {
			this.DirectoryName = "";
			this.FileExtension = ".xml";
		}


		public XmlStore(string directoryName, string fileExtension) {
			if (directoryName == null) throw new ArgumentNullException("directoryName");
			if (fileExtension == null) throw new ArgumentNullException("fileExtension");
			this.directory = directoryName;
			this.fileExtension = fileExtension;
		}


		/// <summary>
		/// Defines the directory, where the nShape project is stored.
		/// </summary>
		public string DirectoryName {
			get { return directory; }
			set {
				if (value == null) throw new ArgumentNullException("DirectoryName");
				directory = value; 
			}
		}


		/// <summary>
		/// Specifies the desired extension for the project file.
		/// </summary>
		public string FileExtension {
			get { return fileExtension; }
			set {
				if (value == null) throw new ArgumentNullException("FileExtension");
				fileExtension = value; 
			}
		}


		/// <summary>
		/// Defines the file projectName without extension, where the nShape designs are stored.
		/// </summary>
		public string DesignFileName {
			get { return designFileName; }
			set { designFileName = value; }
		}


		/// <summary>
		/// Retrieves the file path of the project xml file.
		/// </summary>
		[Browsable(false)]
		public string ProjectFilePath {
			get {
				if (string.IsNullOrEmpty(directory)) 
					throw new InvalidOperationException("Directory for XML repository not set.");
				if (string.IsNullOrEmpty(projectName)) 
					throw new InvalidOperationException("Project name for XML repository not set.");
				string result = Path.Combine(directory, projectName);
				if (!string.IsNullOrEmpty(fileExtension)) result += fileExtension;
				return result;
			}
		}


		/// <summary>
		/// Retrieves the file path of the design xml file.
		/// </summary>
		[Browsable(false)]
		public string DesignFilePath {
			get {
				string result;
				if (string.IsNullOrEmpty(directory)) {
					result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					result += Path.DirectorySeparatorChar + "Dataweb" + Path.DirectorySeparatorChar + "nShape";
				} else result = directory;
				if (string.IsNullOrEmpty(DesignFileName)) throw new InvalidOperationException("Project name for XML repository not set.");
				result += Path.DirectorySeparatorChar;
				result += DesignFileName;
				return result;
			}
		}


		#region Store Implementation

		public override int Version {
			get { return version; }
			set { version = value; }
		}


		public override string ProjectName {
			get { return projectName; }
			set {
				// if (value == null) throw new ArgumentNullException("ProjectName");
				if (value == null) projectName = string.Empty;
				else projectName = value;
			}
		}


		public override bool Exists() {
			return File.Exists(ProjectFilePath);
		}


		public override void Create(IStoreCache cache) {
			DoOpen(cache, true);
		}


		public override void Open(IStoreCache cache) {
			DoOpen(cache, false);
		}


		public override void Close(IStoreCache storeCache) {
			isOpen = false;
			isOpenComplete = false;
			CloseFile();
			base.Close(storeCache);
		}


		public override void Erase() {
			File.Delete(ProjectFilePath);
			// The if prevents exceptions during debugging. The catch concurrency problems.
			if (Directory.Exists(ImageDirectory)) {
				try {
					Directory.Delete(ImageDirectory, true);
				} catch (DirectoryNotFoundException) {
					// It's ok if the directory does not exi
				}
			}
		}


		public override void LoadProjects(IStoreCache cache, IEntityType entityType, params object[] parameters) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (entityType == null) throw new ArgumentNullException("entityType");
			// Do nothing. OpenComplete must be called after the libraries have been loaded.
		}


		/// <override></override>
		public override void LoadDesigns(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			// Do nothing. OpenComplete must be called after the libraries have been loaded.
		}


		/// <override></override>
		public override void LoadModel(IStoreCache cache, object projectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadTemplates(IStoreCache cache, object projectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadDiagrams(IStoreCache cache, object projectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadDiagramShapes(IStoreCache cache, Diagram diagram) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadTemplateShapes(IStoreCache cache, object templateId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadChildShapes(IStoreCache cache, object parentShapeId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadTemplateModelObjects(IStoreCache cache, object templateId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadModelModelObjects(IStoreCache cache, object modelId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadChildModelObjects(IStoreCache cache, object parentModelObjectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void SaveChanges(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (!isOpen) throw new nShapeException("Store is not open.");
			if (string.IsNullOrEmpty(ProjectFilePath)) throw new nShapeException("File name was not specified.");
			// If it is a new project, we must create the file. Otherwise it is already open.
			if (cache.ProjectId == null) {
				CreateFile(cache, ProjectFilePath, false);
			} else if (cache.LoadedProjects[cache.ProjectId].State == ItemState.Deleted) {
				// First delete the file, so the image directory still exists, when the file cannot be deleted.
				if (File.Exists(ProjectFilePath)) File.Delete(ProjectFilePath);
				if (Directory.Exists(ImageDirectory)) Directory.Delete(ImageDirectory, true);
			} else {
				OpenComplete(cache);
				// TODO 2: We should keep the file open and clear it here instead of re-creating it.
				CloseFile();
				CreateFile(cache, ProjectFilePath, true);
			}
			WriteProject(cache);
			// Close and reopen to update Windows directory and keep file ownership
			CloseFile();
			// File has been written successfully
			OpenFile(cache, ProjectFilePath);
		}

		#endregion


		#region Implementation

		/// <override></override>
		protected void LoadShapeConnections(IStoreCache cache, Diagram diagram) {
			Debug.Assert(isOpen);
			OpenComplete(cache);
		}


		protected void DoOpen(IStoreCache cache, bool create) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (isOpen) throw new InvalidOperationException(string.Format("{0} is already open.", GetType().Name));
			if (create) {
				// Nothing to do. Data is kept in memory until SaveChanges is called.
				isOpenComplete = true;
			} else {
				OpenFile(cache, ProjectFilePath);
				try {
					xmlReader.MoveToContent();
					if (xmlReader.Name != rootTag || !xmlReader.HasAttributes)
						throw new nShapeException("XML file '{0}' is not a valid nShape project file.", ProjectFilePath);
					version = int.Parse(xmlReader.GetAttribute(0));
					if (version < 1 || version > currentVersion)
						throw new nShapeException("XML file has an invalid version or is newer than this version of nShape.");
					cache.SetRepositoryBaseVersion(version);
					// Reading functions check whether cache is open.
					// isOpen = true;
					XmlSkipStartElement(rootTag);
					// We only read the designs and the project here. This gives the application
					// a chance to load required libraries. Templates and diagramControllers are then loaded
					// in OpenComplete.
					ReadProjectSettings(cache, xmlReader);
				} catch (Exception exc) {
					CloseFile();
					throw exc;
				}
			}
			isOpen = true;
		}

		#endregion


		#region Read from XML file

		private void ReadProjectSettings(IStoreCache cache, XmlReader xmlReader) {
			if (!XmlSkipToElement(projectTag)) throw new nShapeException("Invalid XML file. Project tag not found.");
			// Load project data
			IEntityType projectSettingsEntityType = cache.FindEntityTypeByName(ProjectSettings.EntityTypeName);
			ProjectSettings project = (ProjectSettings)projectSettingsEntityType.CreateInstanceForLoading();
			repositoryReader.ResetFieldReading(projectSettingsEntityType.PropertyDefinitions);
			XmlSkipStartElement(projectTag);
			repositoryReader.DoBeginObject();
			repositoryReader.ReadId();
			((IEntity)project).AssignId(DBNull.Value);
			((IEntity)project).LoadFields(repositoryReader, cache.RepositoryBaseVersion);
			xmlReader.Read(); // read out of attributes
			foreach (EntityPropertyDefinition pi in projectSettingsEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)project).LoadInnerObjects(pi.Name, repositoryReader, cache.RepositoryBaseVersion);
			cache.LoadedProjects.Add(new EntityBucket<ProjectSettings>(project, null, ItemState.Original));
			// Load the project styles
			Design projectDesign = new Design();
			// project design GUID only needed for runtime.
			((IEntity)projectDesign).AssignId(Guid.NewGuid());
			ReadAllStyles(cache, projectDesign);
			cache.LoadedDesigns.Add(new EntityBucket<Design>(projectDesign, project, ItemState.Original));
		}


		private void ReadDesigns(IStoreCache cache, XmlReader xmlReader) {
			IEntityType designEntityType = cache.FindEntityTypeByName(Design.EntityTypeName);
			string designCollectionTag = GetElementCollectionTag(designEntityType);
			XmlSkipToElement(designCollectionTag);
			if (XmlSkipStartElement(designCollectionTag)) {
				do {
					if (XmlSkipStartElement(designEntityType.ElementName)) {
						Design design = (Design)designEntityType.CreateInstanceForLoading();
						repositoryReader.ResetFieldReading(designEntityType.PropertyDefinitions);
						repositoryReader.DoBeginObject();
						((IEntity)design).LoadFields(repositoryReader, designEntityType.RepositoryVersion);
						xmlReader.Read(); // read out of attributes
						foreach (EntityPropertyDefinition pi in designEntityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition) 
								((IEntity)design).LoadInnerObjects(pi.Name, repositoryReader, designEntityType.RepositoryVersion);
						// Global designs are stored with parent id DBNull
						cache.LoadedDesigns.Add(new EntityBucket<Design>(design, null, ItemState.Original));
						design.Clear();
						ReadAllStyles(cache, design);
					}
				} while (xmlReader.ReadToNextSibling(designEntityType.ElementName));
				XmlSkipEndElement(designCollectionTag);
			}
		}


		private void OpenComplete(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (!isOpenComplete) {
				// The position is on the model
				ReadModel(cache, xmlReader);
				ReadTemplates(cache, xmlReader);
				ReadDiagrams(cache, xmlReader);
				XmlSkipEndElement(projectTag);
				XmlSkipEndElement(rootTag);
				isOpenComplete = true;
			}
		}


		private void ReadAllStyles(IStoreCache cache, Design design) {
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(ColorStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(CapStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(FillStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(LineStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(ShapeStyle.EntityTypeName));
		}


		private void ReadStyles(IStoreCache cache, XmlReader xmlReader, Design design, IEntityType styleEntityType) {
			if (!xmlReader.IsStartElement(GetElementCollectionTag(styleEntityType)))
				throw new nShapeException("Element '{0}' expected but not found.", GetElementCollectionTag(styleEntityType));
			xmlReader.Read(); // Read over the collection tag
			repositoryReader.ResetFieldReading(styleEntityType.PropertyDefinitions);
			while (xmlReader.Name == GetElementTag(styleEntityType)) {
				Style style = (Style)styleEntityType.CreateInstanceForLoading();
				repositoryReader.DoBeginObject();
				style.AssignId(repositoryReader.ReadId());
				style.LoadFields(repositoryReader, styleEntityType.RepositoryVersion);
				xmlReader.Read(); // read out of attributes
				foreach (EntityPropertyDefinition pi in styleEntityType.PropertyDefinitions)
					if (pi is EntityInnerObjectsDefinition)
						style.LoadInnerObjects(pi.Name, repositoryReader, styleEntityType.RepositoryVersion);
				cache.LoadedStyles.Add(new EntityBucket<IStyle>(style, design, ItemState.Original));
				design.AddStyle(style);
				// Reads the end tag of the specific style if present
				XmlReadEndElement(GetElementTag(styleEntityType));
			}
			XmlReadEndElement(GetElementCollectionTag(styleEntityType));
		}


		private void ReadModel(IStoreCache cache, XmlReader xmlReader) {
			IEntityType modelEntityType = cache.FindEntityTypeByName(Model.EntityTypeName);
			if (XmlSkipStartElement(modelEntityType.ElementName)) {
				if (xmlReader.NodeType != XmlNodeType.EndElement) {
					Model model = (Model)modelEntityType.CreateInstanceForLoading();
					repositoryReader.ResetFieldReading(modelEntityType.PropertyDefinitions);
					repositoryReader.DoBeginObject();
					((IEntity)model).AssignId(repositoryReader.ReadId());
					((IEntity)model).LoadFields(repositoryReader, modelEntityType.RepositoryVersion);
					xmlReader.Read(); // read out of attributes
					foreach (EntityPropertyDefinition pi in modelEntityType.PropertyDefinitions)
						if (pi is EntityInnerObjectsDefinition)
							((IEntity)model).LoadInnerObjects(pi.Name, repositoryReader, modelEntityType.RepositoryVersion);
					// Global models are stored with parent id DBNull
					cache.LoadedModels.Add(new EntityBucket<Model>(model, null, ItemState.Original));

					ReadModelObjects(cache, xmlReader, model);
				}
			}
			XmlSkipEndElement(modelEntityType.ElementName);
		}


		private void ReadModelObjects(IStoreCache cache, XmlReader xmlReader, IEntity owner) {
			if (XmlSkipStartElement(modelObjectsTag)) {
				while (xmlReader.NodeType != XmlNodeType.EndElement)
					ReadModelObject(cache, repositoryReader, owner);
				XmlSkipEndElement(modelObjectsTag);
			}
		}


		private void ReadTemplates(IStoreCache cache, XmlReader xmlReader) {
			IEntityType templateEntityType = cache.FindEntityTypeByName(Template.EntityTypeName);
			string templateCollectionTag = GetElementCollectionTag(templateEntityType);
			string templateTag = GetElementTag(templateEntityType);
			if (!xmlReader.IsStartElement(templateCollectionTag))
				throw new nShapeException("Element '{0}' expected but not found.", templateCollectionTag);
			xmlReader.Read(); // Read over the collection tag
			repositoryReader.ResetFieldReading(templateEntityType.PropertyDefinitions);
			XmlStoreReader innerReader = new XmlStoreReader(xmlReader, this, cache);
			while (xmlReader.IsStartElement(templateTag)) {
				// Read the template
				Template template = (Template)templateEntityType.CreateInstanceForLoading();
				repositoryReader.DoBeginObject();
				template.AssignId(repositoryReader.ReadId());
				template.LoadFields(repositoryReader, templateEntityType.RepositoryVersion);
				xmlReader.Read(); // read out of attributes
				// Read the model object
				IModelObject modelObject = null;
				if (XmlSkipStartElement("no_model")) XmlReadEndElement("no_model");
				else modelObject = ReadModelObject(cache, innerReader, template);
				// Read the shape
				template.Shape = ReadShape(cache, innerReader, null);
				template.Shape.ModelObject = modelObject;
				cache.LoadedTemplates.Add(new EntityBucket<Template>(template, cache.Project, ItemState.Original));
				// Read the template's inner objects
				foreach (EntityPropertyDefinition pi in templateEntityType.PropertyDefinitions)
					if (pi is EntityInnerObjectsDefinition)
						template.LoadInnerObjects(pi.Name, repositoryReader, templateEntityType.RepositoryVersion);

				XmlSkipAttributes();
				XmlStoreReader reader = new XmlStoreReader(xmlReader, this, cache);
				ReadModelMappings(cache, reader, template);
				
				// Read the template end tag
				XmlReadEndElement(GetElementTag(templateEntityType));
			}
			XmlReadEndElement(GetElementCollectionTag(templateEntityType));
		}


		private void ReadModelMappings(IStoreCache cache, XmlStoreReader reader, Template template) {
			if (XmlSkipStartElement(modelmappingsTag)) {
				while (xmlReader.NodeType != XmlNodeType.EndElement) {
					IModelMapping modelMapping = ReadModelMapping(cache, reader, template);
					template.MapProperties(modelMapping);
					//XmlSkipAttributes();
				}
				XmlSkipEndElement(modelmappingsTag);
			}
		}


		private IModelMapping ReadModelMapping(IStoreCache cache, XmlStoreReader reader, IEntity owner) {
			Debug.Assert(xmlReader.NodeType == XmlNodeType.Element);
			string modelMappingTag = xmlReader.Name;
			IEntityType entityType = cache.FindEntityTypeByElementName(modelMappingTag);
			if (entityType == null)
				throw new nShapeException("No shape type found for tag '{0}'.", modelMappingTag);
			XmlSkipStartElement(modelMappingTag);
			IModelMapping modelMapping = (IModelMapping)entityType.CreateInstanceForLoading();
			reader.ResetFieldReading(entityType.PropertyDefinitions);
			reader.DoBeginObject();
			((IEntity)modelMapping).AssignId(reader.ReadId());
			((IEntity)modelMapping).LoadFields(reader, entityType.RepositoryVersion);
			xmlReader.Read(); // Reads out of attributes
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)modelMapping).LoadInnerObjects(pi.Name, reader, entityType.RepositoryVersion);
			// Reads the end element
			XmlReadEndElement(modelMappingTag);
			// Insert shape into cache
			cache.LoadedModelMappings.Add(new EntityBucket<IModelMapping>(modelMapping, owner, ItemState.Original));
			return modelMapping;
		}


		private void ReadDiagrams(IStoreCache cache, XmlReader xmlReader) {
			IEntityType diagramEntityType = cache.FindEntityTypeByName(Diagram.EntityTypeName);
			string diagramCollectionTag = GetElementCollectionTag(diagramEntityType);
			string diagramTag = GetElementTag(diagramEntityType);
			XmlSkipToElement(diagramCollectionTag);
			if (XmlSkipStartElement(diagramCollectionTag)) {
				repositoryReader.ResetFieldReading(diagramEntityType.PropertyDefinitions);
				do {
					if (XmlSkipStartElement(diagramTag)) {
						Diagram diagram = (Diagram)diagramEntityType.CreateInstanceForLoading();
						repositoryReader.DoBeginObject();
						((IEntity)diagram).AssignId(repositoryReader.ReadId());
						((IEntity)diagram).LoadFields(repositoryReader, diagramEntityType.RepositoryVersion);
						xmlReader.Read(); // read out of attributes
						foreach (EntityPropertyDefinition pi in diagramEntityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition)
								((IEntity)diagram).LoadInnerObjects(pi.Name, repositoryReader, diagramEntityType.RepositoryVersion);
						cache.LoadedDiagrams.Add(new EntityBucket<Diagram>(diagram, cache.Project, ItemState.Original));
						XmlSkipAttributes();
						XmlStoreReader reader = new XmlStoreReader(xmlReader, this, cache);
						ReadDiagramShapes(cache, reader, diagram);
						ReadDiagramShapeConnections(cache, reader);
					}
				} while (xmlReader.ReadToNextSibling(diagramTag));
				XmlSkipEndElement(diagramCollectionTag);
			}
		}


		private void ReadDiagramShapes(IStoreCache cache, XmlStoreReader reader, Diagram diagram) {
			if (XmlSkipStartElement(shapesTag)) {
				while (xmlReader.NodeType != XmlNodeType.EndElement) {
					Shape shape = ReadShape(cache, reader, diagram);
					diagram.Shapes.Add(shape);
					diagram.AddShapeToLayers(shape, shape.Layers);	// not really necessary
					//XmlSkipAttributes();
				}
				XmlSkipEndElement(shapesTag);
			}
		}


		private Shape ReadShape(IStoreCache cache, XmlStoreReader reader, IEntity owner) {
			Debug.Assert(xmlReader.NodeType == XmlNodeType.Element);
			string shapeTag = xmlReader.Name;
			IEntityType shapeEntityType = cache.FindEntityTypeByElementName(shapeTag);
			if (shapeEntityType == null)
				throw new nShapeException("No shape type found for tag '{0}'.", shapeTag);
			XmlSkipStartElement(shapeTag);
			Shape shape = (Shape)shapeEntityType.CreateInstanceForLoading();
			reader.ResetFieldReading(shapeEntityType.PropertyDefinitions);
			reader.DoBeginObject();
			((IEntity)shape).AssignId(reader.ReadId());
			((IEntity)shape).LoadFields(reader, shapeEntityType.RepositoryVersion);
			xmlReader.Read(); // Reads out of attributes
			foreach (EntityPropertyDefinition pi in shapeEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)shape).LoadInnerObjects(pi.Name, reader, shapeEntityType.RepositoryVersion);
			// Read the child shapes
			if (XmlReadStartElement(childrenTag)) {
				do {
					Shape s = ReadShape(cache, reader, shape);
					shape.Children.Add(s);
				} while (xmlReader.Name != childrenTag && xmlReader.NodeType != XmlNodeType.EndElement);
				if (xmlReader.Name != childrenTag) throw new nShapeException("Shape children are invalid in XML document.");
				XmlReadEndElement(childrenTag);
			}
			// Reads the shape's end element
			XmlReadEndElement(shapeTag);
			// Insert shape into cache
			cache.LoadedShapes.Add(new EntityBucket<Shape>(shape, owner, ItemState.Original));
			return shape;
		}


		private void ReadDiagramShapeConnections(IStoreCache cache, XmlStoreReader reader) {
			if (XmlSkipStartElement(connectionsTag)) {
#if DEBUG
				int connectionCnt = 0;
#endif
				while (xmlReader.Name == connectionTag) {
					xmlReader.MoveToFirstAttribute();
					object connectorId = new Guid(xmlReader.GetAttribute(0));
					xmlReader.MoveToNextAttribute();
					int gluePointId = int.Parse(xmlReader.GetAttribute(1));
					xmlReader.MoveToNextAttribute();
					object targetShapeId = new Guid(xmlReader.GetAttribute(2));
					xmlReader.MoveToNextAttribute();
					int targetPointId = int.Parse(xmlReader.GetAttribute(3));
					xmlReader.MoveToNextAttribute();
					Shape connector = cache.GetShape(connectorId);
					Shape targetShape = cache.GetShape(targetShapeId);
					connector.Connect(gluePointId, targetShape, targetPointId);
#if DEBUG
					++connectionCnt;
#endif
					XmlSkipEndElement(connectionTag);
				}
				XmlSkipEndElement(connectionsTag);
#if DEBUG
				Debug.Print("{0} connections restored.", connectionCnt);
#endif
			}
		}


		// TODO 2: This is more or less identical to ReadShape. Unify?
		// That would require a IEntityWithChildren interface.
		private IModelObject ReadModelObject(IStoreCache cache, XmlStoreReader reader, IEntity owner) {
			Debug.Assert(xmlReader.NodeType == XmlNodeType.Element);
			string modelObjectTag = xmlReader.Name;
			IEntityType entityType = cache.FindEntityTypeByElementName(modelObjectTag);
			if (entityType == null)
				throw new nShapeException("No model object type found for tag '{0}'.", modelObjectTag);
			XmlSkipStartElement(modelObjectTag);
			IModelObject modelObject = (IModelObject)entityType.CreateInstanceForLoading();
			reader.ResetFieldReading(entityType.PropertyDefinitions);
			reader.DoBeginObject();
			modelObject.AssignId(reader.ReadId());
			modelObject.LoadFields(reader, entityType.RepositoryVersion);
			xmlReader.Read(); // Reads out of attributes
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					modelObject.LoadInnerObjects(pi.Name, reader, entityType.RepositoryVersion);
			// Read the child shapes
			if (XmlReadStartElement(childrenTag)) {
				do {
					IModelObject m = ReadModelObject(cache, reader, modelObject);
				} while (xmlReader.Name != childrenTag && xmlReader.NodeType != XmlNodeType.EndElement);
				if (xmlReader.Name != childrenTag) throw new nShapeException("Shape children are invalid in XML document.");
				XmlReadEndElement(childrenTag);
			}
			// Reads the shape's end element
			XmlReadEndElement(modelObjectTag);
			// Insert entity into cache
			cache.LoadedModelObjects.Add(new EntityBucket<IModelObject>(modelObject, owner, ItemState.Original));
			return modelObject;
		}

		#endregion


		#region Write to XML file

		private void CreateFile(IStoreCache cache, string pathName, bool overwrite) {
			Debug.Assert(repositoryWriter == null);
			string imageDirectoryName = CalcImageDirectoryName();
			if (!overwrite) {
				if (File.Exists(pathName))
					throw new IOException(string.Format("File {0} already exists.", pathName));
				if (Directory.Exists(imageDirectoryName))
					throw new IOException(string.Format("Image directory {0} already exists.", imageDirectoryName));
			}
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			xmlWriter = XmlWriter.Create(pathName, settings);
			repositoryWriter = new XmlStoreWriter(xmlWriter, this, cache);
			// Image directory will be created on demand.
		}


		private void OpenFile(IStoreCache cache, string fileName) {
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CloseInput = true;
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = true;
			settings.IgnoreProcessingInstructions = true;
			xmlReader = XmlReader.Create(fileName, settings);
			xmlReader.Read();
			repositoryReader = new XmlStoreReader(xmlReader, this, cache);
		}


		private void CloseFile() {
			repositoryWriter = null;
			repositoryReader = null;
			if (xmlWriter != null) {
				xmlWriter.Close();
				xmlWriter = null;
			}
			if (xmlReader != null) {
				xmlReader.Close();
				xmlReader = null;
			}
		}


		private void WriteProject(IStoreCache cache) {
			xmlWriter.WriteStartDocument();
			XmlOpenElement(rootTag);
			xmlWriter.WriteAttributeString("version", version.ToString());
			// Currently there is no other design than the project design
			// WriteDesigns();
			WriteProjectSettings(cache);
			WriteModel(cache);
			WriteTemplates(cache);
			WriteDiagrams(cache);
			XmlCloseElement(); // project tag
			XmlCloseElement(); // nShape tag
			xmlWriter.WriteEndDocument();
		}


		private void WriteProjectSettings(IStoreCache cache) {
			IEntityType projectSettingsEntityType = cache.FindEntityTypeByName(ProjectSettings.EntityTypeName);
			ProjectSettings ps = cache.Project;
			XmlOpenElement(projectTag);
			repositoryWriter.Reset(projectSettingsEntityType.PropertyDefinitions);
			repositoryWriter.Prepare(ps);
			if (((IEntity)ps).Id == null) ((IEntity)ps).AssignId(DBNull.Value);
			repositoryWriter.WriteId(null);
			((IEntity)ps).SaveFields(repositoryWriter, projectSettingsEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in projectSettingsEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)ps).SaveInnerObjects(pi.Name, repositoryWriter, projectSettingsEntityType.RepositoryVersion);
			// Save the pseudo design
			if (((IEntity)cache.ProjectDesign).Id == null) ((IEntity)cache.ProjectDesign).AssignId(Guid.NewGuid());
			WriteAllStyles(cache, cache.ProjectDesign);
		}


		private void WriteDesigns(IStoreCache cache) {
			IEntityType designEntityType = cache.FindEntityTypeByName(Design.EntityTypeName);
			string designCollectionTag = GetElementCollectionTag(designEntityType);
			string designTag = GetElementTag(designEntityType);
			//
			// Save designs and styles
			XmlOpenElement(designCollectionTag);
			repositoryWriter.Reset(designEntityType.PropertyDefinitions);
			// Save loaded Designs
			foreach (EntityBucket<Design> designItem in cache.LoadedDesigns) {
				switch (designItem.State) {
					case ItemState.Deleted:
						// Do nothing here
						break;
					case ItemState.Modified:
					case ItemState.Original:
						WriteDesign(cache, designItem.ObjectRef);
						break;
					default:
						throw new nShapeUnsupportedValueException(designItem.State.GetType(), designItem.State);
				}
			}
			// Save new designs
			foreach (KeyValuePair<Design, IEntity> d in cache.NewDesigns) {
				((IEntity)d.Key).AssignId(Guid.NewGuid());
				WriteDesign(cache, d.Key);
				// Alte Version (warum?):
				// newDesigns[newDesignsList[i].Name].AssignId(Guid.NewGuid());
				// WriteDesign(newDesigns[newDesignsList[i].Name], ref deletedStyleIds);
			}
			XmlCloseElement();
		}


		private void WriteDesign(IStoreCache cache, Design design) {
			IEntityType designEntityType = cache.FindEntityTypeByName(Design.EntityTypeName);
			string designTag = GetElementTag(designEntityType);
			XmlOpenElement(designTag);
			repositoryWriter.Prepare(design);
			repositoryWriter.WriteId(((IEntity)design).Id);
			((IEntity)design).SaveFields(repositoryWriter, designEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in designEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)design).SaveInnerObjects(pi.Name, repositoryWriter, designEntityType.RepositoryVersion);
			WriteAllStyles(cache, design);
			XmlCloseElement();
		}


		private void WriteAllStyles(IStoreCache cache, Design design) {
			WriteStyles<ColorStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), design);
			WriteStyles<CapStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), design);
			WriteStyles<CharacterStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), design);
			WriteStyles<FillStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), design);
			WriteStyles<LineStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), design);
			WriteStyles<ParagraphStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), design);
			WriteStyles<ShapeStyle>(cache, cache.FindEntityTypeByName(ShapeStyle.EntityTypeName), design);
		}


		private void WriteStyles<TStyle>(IStoreCache cache, IEntityType styleEntityType, Design design) {
			XmlOpenElement(GetElementCollectionTag(styleEntityType));
			repositoryWriter.Reset(styleEntityType.PropertyDefinitions);
			// Save Styles
			foreach (EntityBucket<IStyle> styleItem in cache.LoadedStyles) {
				if (styleItem.Owner == design && styleItem.ObjectRef is TStyle)
					if (styleItem.State != ItemState.Deleted)
						WriteStyle(styleEntityType, styleItem.ObjectRef);
			}
			foreach (KeyValuePair<IStyle, IEntity> s in cache.NewStyles)
				if (s.Key is TStyle) {
					Debug.Assert(s.Key.Id == null);
					s.Key.AssignId(Guid.NewGuid());
					WriteStyle(styleEntityType, s.Key);
				}
			XmlCloseElement();
		}


		private void WriteStyle(IEntityType styleEntityType, IStyle style) {
			XmlOpenElement(GetElementTag(styleEntityType));
			repositoryWriter.Prepare(style);
			repositoryWriter.WriteId(style.Id);
			style.SaveFields(repositoryWriter, styleEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in styleEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					style.SaveInnerObjects(pi.Name, repositoryWriter, styleEntityType.RepositoryVersion);
			XmlCloseElement();
		}


		private void WriteTemplates(IStoreCache cache) {
			// Save Templates, Shape and ModelObject
			IEntityType entityType = cache.FindEntityTypeByName(Template.EntityTypeName);
			XmlOpenElement(GetElementCollectionTag(entityType));
			repositoryWriter.Reset(entityType.PropertyDefinitions);
			// Save loaded templates
			foreach (EntityBucket<Template> templateItem in cache.LoadedTemplates) {
				if (templateItem.State != ItemState.Deleted)
					WriteTemplate(cache, entityType, templateItem.ObjectRef);
			}
			// Save new templates, shapes and model objects
			foreach (KeyValuePair<Template, IEntity> t in cache.NewTemplates)
				WriteTemplate(cache, entityType, t.Key);
			XmlCloseElement();
		}


		private void WriteTemplate(IStoreCache cache, IEntityType entityType, Template template) {
			XmlOpenElement(templateTag);
			
			// Save template definition
			repositoryWriter.Reset(entityType.PropertyDefinitions);
			repositoryWriter.Prepare(template);
			if (template.Id == null) template.AssignId(Guid.NewGuid());
			repositoryWriter.WriteId(template.Id);
			template.SaveFields(repositoryWriter, entityType.RepositoryVersion);
			XmlStoreWriter innerObjectsWriter = new XmlStoreWriter(xmlWriter, this, cache);
			if (template.Shape.ModelObject == null) {
				XmlOpenElement("no_model");
				XmlCloseElement();
			} else
				WriteModelObject(cache, template.Shape.ModelObject, innerObjectsWriter);
			WriteShape(cache, template.Shape, innerObjectsWriter);
			innerObjectsWriter = null;
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					template.SaveInnerObjects(pi.Name, repositoryWriter, version);
			repositoryWriter.Finish();

			// Save template's model mappings
			WriteModelMappings(cache, template);

			XmlCloseElement(); // template tag
		}


		private void WriteModelMappings(IStoreCache cache, Template template) {
			XmlOpenElement(modelmappingsTag);
			foreach (EntityBucket<IModelMapping> eb in cache.LoadedModelMappings) {
				// Template shapes have a null Owner
				if (eb.Owner != null && eb.Owner == template && eb.State != ItemState.Deleted)
					WriteModelMapping(cache, eb.ObjectRef, repositoryWriter);
			}
			foreach (KeyValuePair<IModelMapping, IEntity> sp in cache.NewModelMappings) {
				if (sp.Value == template)
					WriteModelMapping(cache, sp.Key, repositoryWriter);
			}
			XmlCloseElement();
		}


		private void WriteModelMapping(IStoreCache cache, IModelMapping modelMapping, XmlStoreWriter writer) {
			//IEntityType entityType = cache.FindEntityTypeByName(modelMapping.EntityTypeName);
			string entityTypeName;
			if (modelMapping is NumericModelMapping) entityTypeName = NumericModelMapping.EntityTypeName;
			else if (modelMapping is FormatModelMapping) entityTypeName = FormatModelMapping.EntityTypeName;
			else if (modelMapping is StyleModelMapping) entityTypeName = StyleModelMapping.EntityTypeName;
			else throw new nShapeUnsupportedValueException(modelMapping);
			
			IEntityType entityType = cache.FindEntityTypeByName(entityTypeName);
			// write Shape-Tag with EntityType
			XmlOpenElement(entityType.ElementName);
			writer.Reset(entityType.PropertyDefinitions);
			writer.Prepare(modelMapping);
			if (((IEntity)modelMapping).Id == null) ((IEntity)modelMapping).AssignId(Guid.NewGuid());
			writer.WriteId(((IEntity)modelMapping).Id);
			((IEntity)modelMapping).SaveFields(writer, entityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)modelMapping).SaveInnerObjects(pi.Name, writer, entityType.RepositoryVersion);
			XmlCloseElement();
		}


		private void WriteModel(IStoreCache cache) {
			IEntityType modelEntityType = cache.FindEntityTypeByName(Model.EntityTypeName);
			string modelTag = GetElementTag(modelEntityType);
			XmlOpenElement(modelTag);
			
			// Write model
			Model model = cache.GetModel();
			Debug.Assert(model != null);
			repositoryWriter.Prepare(model);
			if (model.Id == null) ((IEntity)model).AssignId(Guid.NewGuid());
			repositoryWriter.WriteId(((IEntity)model).Id);
			((IEntity)model).SaveFields(repositoryWriter, modelEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in modelEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)model).SaveInnerObjects(pi.Name, repositoryWriter, modelEntityType.RepositoryVersion);
			
			// Write all model objects
			WriteModelObjects(cache, model);
			XmlCloseElement();
		}
			
			
		private void WriteModelObjects(IStoreCache cache, Model model) {
			XmlOpenElement(modelObjectsTag);
			// We do not want to write template model objects here.
			foreach (EntityBucket<IModelObject> mob in cache.LoadedModelObjects)
				if (mob.Owner == model || mob.Owner is IModelObject)
					WriteModelObject(cache, mob.ObjectRef, repositoryWriter);
			foreach (KeyValuePair<IModelObject, IEntity> mokvp in cache.NewModelObjects)
				if (mokvp.Value == model || mokvp.Value is IModelObject)
					WriteModelObject(cache, mokvp.Key, repositoryWriter);
			XmlCloseElement();
		}


		private void WriteModelObject(IStoreCache cache, IModelObject modelObject, XmlStoreWriter writer) {
			IEntityType modelObjectEntityType = cache.FindEntityTypeByName(modelObject.Type.Name);
			string modelObjectTag = GetElementTag(modelObjectEntityType);
			XmlOpenElement(modelObjectTag);
			writer.Reset(modelObjectEntityType.PropertyDefinitions);
			writer.Prepare(modelObject);
			if (modelObject.Id == null) modelObject.AssignId(Guid.NewGuid());
			writer.WriteId(modelObject.Id);
			modelObject.SaveFields(writer, modelObjectEntityType.RepositoryVersion);
			writer.Finish();
			XmlCloseElement();
		}


		private void WriteDiagrams(IStoreCache cache) {
			IEntityType diagramEntityType = cache.FindEntityTypeByName(Diagram.EntityTypeName);
			string diagramCollectionTag = GetElementCollectionTag(diagramEntityType);
			XmlOpenElement(diagramCollectionTag);
			// Save loaded diagramControllers
			foreach (EntityBucket<Diagram> diagramItem in cache.LoadedDiagrams) {
				if (diagramItem.State != ItemState.Deleted)
					WriteDiagram(cache, diagramEntityType, diagramItem.ObjectRef);
			}
			// Save new diagrams
			foreach (KeyValuePair<Diagram, IEntity> d in cache.NewDiagrams) {
				Debug.Assert(((IEntity)d.Key).Id == null);
				((IEntity)d.Key).AssignId(Guid.NewGuid());
				WriteDiagram(cache, diagramEntityType, d.Key);
			}
			XmlCloseElement();
		}


		private void WriteDiagram(IStoreCache cache, IEntityType diagramEntityType, Diagram diagram) {
			string diagramTag = GetElementTag(diagramEntityType);
			XmlOpenElement(diagramTag);
			repositoryWriter.Reset(diagramEntityType.PropertyDefinitions);
			repositoryWriter.Prepare(diagram);
			repositoryWriter.WriteId(((IEntity)diagram).Id);
			((IEntity)diagram).SaveFields(repositoryWriter, diagramEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in diagramEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)diagram).SaveInnerObjects(pi.Name, repositoryWriter, diagramEntityType.RepositoryVersion);
			WriteDiagramShapes(cache, diagram);
			WriteDiagramShapeConnections(diagram);
			XmlCloseElement();
		}


		private void WriteDiagramShapes(IStoreCache cache, Diagram diagram) {
			XmlOpenElement(shapesTag);
			foreach (EntityBucket<Shape> eb in cache.LoadedShapes) {
				// Template shapes have a null Owner
				if (eb.Owner != null && eb.Owner == diagram && eb.State != ItemState.Deleted)
					WriteShape(cache, eb.ObjectRef, repositoryWriter);
			}
			foreach (KeyValuePair<Shape, IEntity> sp in cache.NewShapes) {
				if (sp.Value == diagram)
					WriteShape(cache, sp.Key, repositoryWriter);
			}
			XmlCloseElement();
		}


		private void WriteShape(IStoreCache cache, Shape shape, XmlStoreWriter writer) {
			IEntityType shapeEntityType = cache.FindEntityTypeByName(shape.Type.FullName);
			// write Shape-Tag with EntityType
			XmlOpenElement(shapeEntityType.ElementName);
			writer.Reset(shapeEntityType.PropertyDefinitions);
			writer.Prepare(shape);
			if (((IEntity)shape).Id == null) ((IEntity)shape).AssignId(Guid.NewGuid());
			writer.WriteId(((IEntity)shape).Id);
			((IEntity)shape).SaveFields(writer, shapeEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in shapeEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)shape).SaveInnerObjects(pi.Name, writer, shapeEntityType.RepositoryVersion);
			// Write children
			if (shape.Children.Count > 0) {
				XmlOpenElement("children");
				foreach (Shape s in shape.Children)
					WriteShape(cache, s, writer);
				XmlCloseElement();
			}
			XmlCloseElement();
		}


		//// This is correct but slow...
		//private void WriteDiagramShapeConnections(IStoreCache cache, Diagram diagram) {
		//   XmlOpenElement(connectionTag + "s");
		//   foreach (ShapeConnection c in cache.NewShapeConnections) {
		//      if (diagram.Shapes.Contains(c.ConnectorShape) && diagram.Shapes.Contains(c.TargetShape)) {
		//         XmlOpenElement(connectionTag);
		//         xmlWriter.WriteAttributeString(activeShapeTag, ((IEntity)c.ConnectorShape).Id.ToString());
		//         xmlWriter.WriteAttributeString(gluePointIdTag, c.GluePointId.ToString());
		//         xmlWriter.WriteAttributeString(passiveShapeTag, ((IEntity)c.TargetShape).Id.ToString());
		//         xmlWriter.WriteAttributeString(connectionPointIdTag, c.TargetPointId.ToString());
		//         XmlCloseElement();
		//      }
		//   }
		//   XmlCloseElement();
		//}


		private void WriteDiagramShapeConnections(Diagram diagram) {
			XmlOpenElement(connectionTag + "s");
			foreach (Shape shape in diagram.Shapes) {
				// find all gluePoints of the shape
				foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
					// get connection for each glue point
					ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
					if (!sci.IsEmpty) {
						XmlOpenElement(connectionTag);
						xmlWriter.WriteAttributeString(activeShapeTag, ((IEntity)shape).Id.ToString());
						xmlWriter.WriteAttributeString(gluePointIdTag, gluePointId.ToString());
						xmlWriter.WriteAttributeString(passiveShapeTag, ((IEntity)sci.OtherShape).Id.ToString());
						xmlWriter.WriteAttributeString(connectionPointIdTag, sci.OtherPointId.ToString());
						XmlCloseElement();
					}
				}
			}
			XmlCloseElement();
		}

		#endregion


		//-------------------------------------------------------------------------
		/// <summary>
		/// Writes fields and inner objects to XML.
		/// </summary>
		protected class XmlStoreWriter : RepositoryWriter {

			public XmlStoreWriter(XmlWriter xmlWriter, XmlStore store, IStoreCache cache)
				: base(cache) {
				if (xmlWriter == null) throw new ArgumentNullException("xmlWriter");
				if (store == null) throw new ArgumentNullException("store");
				this.store = store;
				this.xmlWriter = xmlWriter;
			}


			internal protected override void Prepare(IEntity entity) {
				base.Prepare(entity);
			}


			private string GetXmlAttributeName(int propertyIndex) {
				/* Not required for inner objects
				 * if (Entity == null) 
					throw new nShapeException("Persistable object to store is not set. Please assign an IEntity object to the property Object before calling a save method.");*/
				if (propertyInfos == null)
					throw new nShapeException("EntityType is not set. Please assign an EntityType to the property EntityType before calling a save method.");
				return propertyIndex == -1 ? "id" : propertyInfos[propertyIndex].ElementName;
			}


			#region RepositoryWriter Members

			protected override void DoWriteId(object id) {
				++PropertyIndex;
				string fieldName = GetXmlAttributeName(PropertyIndex);
				if (id == null)
					XmlAddAttributeString(fieldName, Guid.Empty.ToString());
				else
					XmlAddAttributeString(fieldName, id.ToString());
			}


			protected override void DoWriteBool(bool value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteByte(byte value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteInt16(short value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteInt32(int value) {
				++PropertyIndex;
				string fieldName = GetXmlAttributeName(PropertyIndex);
				XmlAddAttributeString(fieldName, value.ToString());
			}


			protected override void DoWriteInt64(long value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteFloat(float value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteDouble(double value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteChar(char value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			protected override void DoWriteString(string value) {
				if (string.IsNullOrEmpty(value)) value = "";
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value);
			}


			protected override void DoWriteDate(DateTime value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToUniversalTime().ToString(datetimeFormat));
			}


			protected override void DoWriteImage(Image image) {
				++PropertyIndex;
				if (image == null) {
					XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), "");
				} else {
					string filePath = store.ImageDirectory;
					if (!System.IO.Directory.Exists(filePath))
						System.IO.Directory.CreateDirectory(filePath);
					filePath = Path.Combine(filePath, Entity.Id.ToString() + " (" + (++imageCount).ToString() + ")");
					if (image.RawFormat.Guid == ImageFormat.Bmp.Guid) filePath += ".bmp";
					else if (image.RawFormat.Guid == ImageFormat.Emf.Guid) filePath += ".emf";
					else if (image.RawFormat.Guid == ImageFormat.Exif.Guid) filePath += ".exif";
					else if (image.RawFormat.Guid == ImageFormat.Gif.Guid) filePath += ".gif";
					else if (image.RawFormat.Guid == ImageFormat.Icon.Guid) filePath += ".ico";
					else if (image.RawFormat.Guid == ImageFormat.Jpeg.Guid) filePath += ".jpeg";
					else if (image.RawFormat.Guid == ImageFormat.MemoryBmp.Guid) filePath += ".bmp";
					else if (image.RawFormat.Guid == ImageFormat.Png.Guid) filePath += ".png";
					else if (image.RawFormat.Guid == ImageFormat.Tiff.Guid) filePath += ".tiff";
					else if (image.RawFormat.Guid == ImageFormat.Wmf.Guid) filePath += ".wmf";
					else Debug.Fail("Unsupported image format.");

					if (image is Metafile) {
						using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
							IntPtr hdc = gfx.GetHdc();
							Metafile metaFile = new Metafile(filePath, hdc);
							gfx.ReleaseHdc(hdc);
							using (Graphics metaFileGfx = Graphics.FromImage(metaFile)) {
								Rectangle bounds = Rectangle.Empty;
								bounds.Width = image.Width;
								bounds.Height = image.Height;
								ImageAttributes imgAttribs = GdiHelpers.GetImageAttributes(nShapeImageLayout.Original);
								GdiHelpers.DrawImage(metaFileGfx, image, imgAttribs, nShapeImageLayout.Original, bounds, bounds);
							}
						}
					} else image.Save(filePath, image.RawFormat);

					string currentDirName = Path.GetDirectoryName(store.ProjectFilePath);
					if (!string.IsNullOrEmpty(currentDirName))
						filePath = filePath.Replace(currentDirName, ".");
					else
						filePath = filePath.Replace(Path.GetDirectoryName(Path.GetFullPath(store.ProjectFilePath)), ".");
					XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), filePath);
				}
			}


			protected override void DoBeginWriteInnerObjects() {
				// Sanity checks
				if (propertyInfos == null) 
					throw new InvalidOperationException("EntityType is not set.");
				if (Entity == null) 
					throw new InvalidOperationException("InnerObject's parent object is not set to an instance of an object.");
				if (!(propertyInfos[PropertyIndex + 1] is EntityInnerObjectsDefinition)) 
					throw new InvalidOperationException(string.Format("The current property info for '{0}' does not refer to inner objects. Check whether the writing methods fit the PropertyInfos property.", propertyInfos[PropertyIndex + 1]));
				// Advance to next inner objects property
				++PropertyIndex;
				innerObjectsWriter = new XmlStoreWriter(xmlWriter, store, Cache);
				innerObjectsWriter.Reset(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).PropertyDefinitions);
				xmlWriter.WriteStartElement(Cache.CalculateElementName(propertyInfos[PropertyIndex].Name));
			}


			protected override void DoEndWriteInnerObjects() {
				xmlWriter.WriteEndElement();
			}


			protected override void DoBeginWriteInnerObject() {
				Debug.Assert(Entity != null && innerObjectsWriter != null);
				xmlWriter.WriteStartElement(Cache.CalculateElementName(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).EntityTypeName));
				innerObjectsWriter.Prepare(null);
				// Skip the property index for the id since inner objects do not have one.
				++InnerObjectsWriter.PropertyIndex;
			}


			protected override void DoEndWriteInnerObject() {
				Debug.Assert(Entity != null && innerObjectsWriter != null);
				xmlWriter.WriteEndElement();
			}


			protected override void DoDeleteInnerObjects() {
				throw new NotImplementedException();
			}

			#endregion


			private void XmlAddAttributeString(string name, string value) {
				xmlWriter.WriteAttributeString(name, value);
			}


			private XmlStoreWriter InnerObjectsWriter {
				get { return (XmlStoreWriter)innerObjectsWriter; }
			}


			#region Fields

			private XmlStore store;

			private XmlWriter xmlWriter;

			private int imageCount = 0;

			#endregion
		}

		
		/// <summary>
		/// Implements a cache repositoryReader for XML.
		/// </summary>
		protected class XmlStoreReader : RepositoryReader {

			public XmlStoreReader(XmlReader xmlReader, XmlStore store, IStoreCache cache)
				: base(cache) {
				if (xmlReader == null) throw new ArgumentNullException("xmlReader");
				if (store == null) throw new ArgumentNullException("store");
				this.store = store;
				this.xmlReader = xmlReader;
			}


			#region Implementation
			
			internal protected override object ReadId() {
				++PropertyIndex;
				ValidatePropertyIndex();
				object result = null;
				string resultString = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				if (!string.IsNullOrEmpty(resultString)) {
					result = new Guid(resultString);
					if (result.Equals(Guid.Empty))
						result = null;
				}
				xmlReader.MoveToNextAttribute();
				return result;
			}


			internal override void ResetFieldReading(IEnumerable<EntityPropertyDefinition> propertyInfos) {
				base.ResetFieldReading(propertyInfos);
			}


			// Assumes that the current tag is of the expected type or the end element of 
			// the enclosing element. Moves to the first attribute for subsequent reading.
			protected internal override bool DoBeginObject() {
				// If the enclosing element is empty, it is still the current tag and indicates 
				// an empty inner objects list.
				if (xmlReader.NodeType == XmlNodeType.EndElement || (xmlReader.IsEmptyElement && !xmlReader.HasAttributes))
					return false;
				else {
					xmlReader.MoveToFirstAttribute();
					PropertyIndex = -2;
					return true;
				}
			}


			protected internal override void DoEndObject() {
				// Read over the end tag of the object.
			}

			#endregion


			#region RepositoryReader Implementation

			public override void BeginReadInnerObjects() {
				if (propertyInfos == null) throw new nShapeException("Property EntityType is not set.");
				if (innerObjectsReader != null) throw new InvalidOperationException("EndReadInnerObjects was not called.");
				++PropertyIndex;
				string elementName = Cache.CalculateElementName(propertyInfos[PropertyIndex].Name);
				if (!xmlReader.IsStartElement(elementName)) throw new InvalidOperationException(string.Format("Element '{0}' expected.", elementName));
				if (!xmlReader.IsEmptyElement) xmlReader.Read();
				innerObjectsReader = new XmlStoreReader(xmlReader, store, Cache);
				// Set a marker to detect wrong call sequence
				InnerObjectsReader.PropertyIndex = int.MinValue;
				InnerObjectsReader.ResetFieldReading(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).PropertyDefinitions);
			}


			public override void EndReadInnerObjects() {
				if (innerObjectsReader == null) throw new InvalidOperationException("BeginReadInnerObjects was not called.");
				Debug.Assert(xmlReader.IsEmptyElement || xmlReader.NodeType == XmlNodeType.EndElement);
				xmlReader.Read(); // read end tag of collection
				innerObjectsReader = null;
			}


			public override void EndReadInnerObject() {
				xmlReader.Read(); // Read out of the attributes
				// Previous version: XmlSkipEndElement(store.CalculateElementName(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).Name));
				InnerObjectsReader.PropertyIndex = int.MinValue;
			}


			protected override bool DoReadBool() {
				bool result = bool.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override byte DoReadByte() {
				byte result = byte.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override short DoReadInt16() {
				short result = short.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override int DoReadInt32() {
				int result = int.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override long DoReadInt64() {
				long result = long.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override float DoReadFloat() {
				float result = float.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override double DoReadDouble() {
				double result = double.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override char DoReadChar() {
				char result = char.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override string DoReadString() {
				string result = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				xmlReader.MoveToNextAttribute();
				return result;
			}


			protected override DateTime DoReadDate() {
				System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();
				string attrValue = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				DateTime dateTime;
				if (!DateTime.TryParseExact(attrValue, datetimeFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal, out dateTime))
					dateTime = Convert.ToDateTime(attrValue);	// ToDo: This is for compatibility with older file versions - Remove later
				return dateTime.ToLocalTime();
				
				//DateTime result = Convert.ToDateTime(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset)).ToLocalTime();
				//xmlReader.MoveToNextAttribute();
				//return result;
			}


			protected override Image DoReadImage() {
				Image result = null;
				string filePath = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				xmlReader.MoveToNextAttribute();
				if (!string.IsNullOrEmpty(filePath)) {
					string fileName = Path.Combine(store.ImageDirectory, Path.GetFileName(filePath));
					FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
					byte[] buffer = new byte[fileStream.Length];
					fileStream.Read(buffer, 0, buffer.Length);
					fileStream.Close();
					fileStream.Dispose();

					MemoryStream memStream = new MemoryStream(buffer);
					result = Image.FromStream(memStream);
				}
				return result;
			}

			#endregion


			protected override void ValidatePropertyIndex() {
				base.ValidatePropertyIndex();
				if (PropertyIndex >= xmlReader.AttributeCount)
					throw new nShapeException("Element {0} of the entity could not be loaded because the repository is invalid. Check whether it is a valid and/or up-to-date repository.", PropertyIndex);
			}


			private XmlStoreReader InnerObjectsReader {
				get { return (XmlStoreReader)innerObjectsReader; }
			}


			#region Fields

			// There is always one more XML attribute (the id) than there are properties 
			// in the entity type.
			private const int xmlAttributeOffset = 1;

			private XmlStore store;

			private XmlReader xmlReader;

			#endregion
		}


		#region Obtain object tags and field structure

		// TODO 2: Replace this access in place.
		private string GetElementTag(IEntityType entityType) {
			return entityType.ElementName;
		}


		private string GetElementCollectionTag(IEntityType entityType) {
			return GetElementTag(entityType) + "s";
		}


		internal string ImageDirectory {
			get {
				if (string.IsNullOrEmpty(imageDirectory))
					imageDirectory = CalcImageDirectoryName();
				return imageDirectory;
			}
		}

		#endregion


		/// <summary>
		/// Calculates the directory for the images given the complete file path.
		/// </summary>
		/// <param name="pathName"></param>
		/// <returns></returns>
		protected string CalcImageDirectoryName() {
			string result = Path.GetDirectoryName(ProjectFilePath);
			if (string.IsNullOrEmpty(result)) throw new ArgumentException("XML repository file name must be a complete path.");
			result = Path.Combine(result, ProjectName + " Images");
			return result;
		}


		#region XML helper functions

		private void XmlOpenElement(string name) {
			xmlWriter.WriteStartElement(name);
		}


		private void XmlCloseElement() {
			xmlWriter.WriteFullEndElement();
		}


		// If the current element is a start element with the given, the function reads
		// it and returns true. If it is not, the function does nothing and returns false.
		private bool XmlReadStartElement(string name) {
			if (xmlReader.IsStartElement(name)) {
				xmlReader.Read();
				return true;
			} else return false;
		}


		// The current element is either <x a1="1"... /x> or </x>
		private void XmlReadEndElement(string name) {
			if (xmlReader.Name == name && xmlReader.NodeType == XmlNodeType.EndElement)
				xmlReader.ReadEndElement();
		}


		private bool XmlSkipToElement(string nodeName) {
			if (xmlReader.Name == nodeName)
				return true;
			else
				return xmlReader.ReadToFollowing(nodeName);
		}


		// Tests whether we are currently at the beginning of an element with the
		// given projectName. If so, read into it and return true. Otherwise false.
		private bool XmlSkipStartElement(string nodeName) {
			// In case we are at an attribute
			if (xmlReader.NodeType != XmlNodeType.Element && xmlReader.NodeType != XmlNodeType.EndElement) {
				xmlReader.Read();
				xmlReader.MoveToContent();
			}
			if (xmlReader.EOF || xmlReader.Name != nodeName)
				return false;
			if (xmlReader.IsEmptyElement && !xmlReader.HasAttributes) {
				xmlReader.ReadStartElement(nodeName);
				return false;
			}
			if (!xmlReader.IsEmptyElement && !xmlReader.HasAttributes)
				xmlReader.ReadStartElement(nodeName);
			return true;
		}


		private void XmlSkipEndElement(string nodeName) {
			XmlSkipAttributes();
			if (xmlReader.Name == nodeName) {
				// skip end element
				if (xmlReader.NodeType == XmlNodeType.EndElement)
					xmlReader.ReadEndElement();
				// skip empty element
				else if (xmlReader.NodeType == XmlNodeType.Element && !xmlReader.HasAttributes) {
					xmlReader.Read();
					xmlReader.MoveToContent();
				}
			}
		}


		private void XmlSkipAttributes() {
			if (xmlReader.NodeType == XmlNodeType.Attribute) {
				xmlReader.Read();
				xmlReader.MoveToContent();
			}
		}

		#endregion


		#region Fields

		protected const string ProjectFileExtension = ".xml";

		// Predefined XML Element Tags
		private const string projectTag = "project";
		private const string shapesTag = "shapes";
		private const string modelObjectsTag = "model_objects";
		private const string rootTag = "dataweb_nshape";
		private const string templateTag = "template";
		private const string modelmappingsTag = "model_mappings";
		private const string connectionsTag = "shape_connections";
		private const string connectionTag = "shape_connection";
		private const string activeShapeTag = "active_shape";
		private const string gluePointIdTag = "glue_point";
		private const string passiveShapeTag = "passive_shape";
		private const string connectionPointIdTag = "connection_point";
		private const string childrenTag = "children";
		// Format string for DateTimes
		private const string datetimeFormat = "yyyy-MM-dd HH:mm:ss";
		// Indicates the highest supported cache version of the built-in entities.
		private const int currentVersion = 100;

		// Directory projectName of project file. Always != null
		private string directory = "";
		// Name of the project. Always != null
		private string projectName = "";
		// File extension of project file. Maybe null.
		private string fileExtension = ".xml";

		private bool isOpen;

		// Repository version of the built-in entities.
		private int version;

		// File projectName of design cache file. Always != null
		private string designFileName = "";

		/// <summary>Indicates that the whole file is in memory.</summary>
		private bool isOpenComplete = false;

		// element attributes
		private Dictionary<string, string[]> attributeFields = new Dictionary<string, string[]>();

		string imageDirectory;

		//private MemoryStream memoryStream = null;
		private XmlReader xmlReader;
		private XmlWriter xmlWriter;
		private XmlStoreWriter repositoryWriter = null;
		private XmlStoreReader repositoryReader = null;

		#endregion
	}
}