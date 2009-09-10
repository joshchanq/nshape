using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

using Dataweb.nShape.Advanced;


namespace Dataweb.nShape {

	/// <summary>
	/// Encapsulates a command.
	/// </summary>
	public interface ICommand {

		/// <summary>
		/// Executes the command.
		/// </summary>
		void Execute();

		/// <summary>
		/// Reverts the command.
		/// </summary>
		void Revert();

		/// <summary>
		/// Tests whether the required permissions for the command are granted.
		/// </summary>
		/// <param name="security"></param>
		/// <returns></returns>
		bool IsAllowed(ISecurityManager securityManager);

		/// <summary>
		/// Specifies the cache on which the command will be executed.
		/// </summary>
		IRepository Repository { get; set; }

		/// <summary>
		/// Describes the purpose of the command.
		/// </summary>
		string Description { get; }
	}


	#region Base Classes

	/// <summary>
	/// Base class for all commands.
	/// </summary>
	public abstract class Command : ICommand {

		/// <override></override>
		protected Command() {
			// nothing to do here
		}

		/// <override></override>
		public abstract void Execute();

		/// <override></override>
		public abstract void Revert();

		/// <override></override>
		public abstract Permission RequiredPermission { get; }

		/// <override></override>
		public virtual bool IsAllowed(ISecurityManager securityManager) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			return securityManager.IsGranted(RequiredPermission);
		}


		/// <override></override>
		public IRepository Repository {
			get { return repository; }
			set { repository = value; }
		}


		/// <override></override>
		public virtual string Description {
			get { return description; }
		}
		

		#region Fields

		protected string description;
		private IRepository repository;

		#endregion
	}


	/// <summary>
	/// Base class for commands that need to disconnect connected shapes before the action may executed,
	/// e.g. a DeleteCommand has to disconnect the deleted shape before deleting it.
	/// </summary>
	public abstract class AutoDisconnectShapesCommand : Command {

		protected AutoDisconnectShapesCommand()
			: base() {
		}


		protected void Disconnect(IList<Shape> shapes) {
			for (int i = shapes.Count - 1; i >= 0; --i)
				Disconnect(shapes[i]);
		}


		protected void Disconnect(Shape shape) {
			if (!connections.ContainsKey(shape))
				connections.Add(shape, new List<ShapeConnectionInfo>(shape.GetConnectionInfos(ControlPointId.Any, null)));
			foreach (ShapeConnectionInfo sci in connections[shape]) {
				if (shape is ILinearShape) {
					shape.Disconnect(sci.OwnPointId);
					Repository.DeleteShapeConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
				} else {
					sci.OtherShape.Disconnect(sci.OtherPointId);
					Repository.DeleteShapeConnection(sci.OtherShape, sci.OtherPointId, shape, sci.OwnPointId);
				}
			}
		}


		protected void Reconnect(IList<Shape> shapes) {
			// restore connections
			int cnt = shapes.Count;
			for (int i = 0; i < cnt; ++i)
				Reconnect(shapes[i]);
		}


		protected void Reconnect(Shape shape) {
			// restore connections
			if (connections.ContainsKey(shape)) {
				foreach (ShapeConnectionInfo sci in connections[shape]) {
					if (shape is ILinearShape) {
						shape.Connect(sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
						if (Repository != null) Repository.InsertShapeConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
					} else {
						sci.OtherShape.Connect(sci.OtherPointId, shape, sci.OwnPointId);
						if (Repository != null) Repository.InsertShapeConnection(sci.OtherShape, sci.OtherPointId, shape, sci.OwnPointId);
					}
				}
			}
		}


		protected Dictionary<Shape, List<ShapeConnectionInfo>> ShapeConnections { get { return connections; } }


		private Dictionary<Shape, List<ShapeConnectionInfo>> connections = new Dictionary<Shape, List<ShapeConnectionInfo>>();
	}


	/// <summary>
	/// Base class for inserting and removing shapes to a diagram and a cache
	/// </summary>
	public abstract class InsertOrRemoveShapeCommand : AutoDisconnectShapesCommand {

		protected InsertOrRemoveShapeCommand(Diagram diagram)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
		}


		protected void InsertShapes(LayerIds activeLayers) {
			DoInsertShapes(false, activeLayers);
		}


		protected void InsertShapes() {
			DoInsertShapes(true, LayerIds.None);
		}


		protected void RemoveShapes() {
			if (Shapes.Count == 0) throw new nShapeInternalException("No shapes set. Call SetShapes() before.");

			// disconnect all selectedShapes connected to the deleted shape(s)
			Disconnect(Shapes);

			if (Shapes.Count > 1) {
				diagram.Shapes.RemoveRange(Shapes);
				if (Repository != null) Repository.DeleteShapes(Shapes);
			} else {
				diagram.Shapes.Remove(Shapes[0]);
				if (Repository != null) Repository.DeleteShape(Shapes[0]);
			}
		}


		protected void SetShapes(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (shapeLayers == null) shapeLayers = new List<LayerIds>(1);
			else this.shapeLayers.Clear();
			this.Shapes.Clear();

			if (!this.Shapes.Contains(shape)) {
				this.Shapes.Add(shape);
				this.shapeLayers.Add(shape.Layers);
			}
		}


		protected void SetShapes(IEnumerable<Shape> shapes) {
			SetShapes(shapes, true);
		}


		protected void SetShapes(IEnumerable<Shape> shapes, bool invertSortOrder) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.Shapes.Clear();
			if (shapeLayers == null) shapeLayers = new List<LayerIds>();
			else this.shapeLayers.Clear();

			foreach (Shape shape in shapes) {
				if (!this.Shapes.Contains(shape)) {
					if (invertSortOrder) {
						this.Shapes.Insert(0, shape);
						this.shapeLayers.Insert(0, shape.Layers);
					} else {
						this.Shapes.Add(shape);
						this.shapeLayers.Add(shape.Layers);
					}
				}
			}
		}


		protected List<Shape> Shapes = new List<Shape>();


		protected static string DeleteDescription = "Delete {0} shape{1}";


		protected static string CreateDescription = "Create {0} shape{1}";


		private void DoInsertShapes(bool useOriginalLayers, LayerIds activeLayers) {
			int startIdx = Shapes.Count - 1;
			if (startIdx < 0) throw new nShapeInternalException("No shapes set. Call SetShapes() before.");

			if (Repository == null) throw new ArgumentNullException("Repository"); 
			for (int i = startIdx; i >= 0; --i) {
				//if (Shapes[i].ZOrder == 0) 
					Shapes[i].ZOrder = Repository.ObtainNewTopZOrder(diagram);
				diagram.Shapes.Add(Shapes[i]);
				diagram.AddShapeToLayers(Shapes[i], useOriginalLayers ? shapeLayers[i] : activeLayers);
			}
			if (startIdx == 0)
				Repository.InsertShape(Shapes[0], diagram);
			else
				Repository.InsertShapes(Shapes, diagram);

			// connect all selectedShapes that were previously connected to the shape(s)
			Reconnect(Shapes);
		}


		private Diagram diagram = null;
		private List<LayerIds> shapeLayers;
	}


	public abstract class InsertOrRemoveModelObjectsCommand : Command {

		protected void SetModelObjects(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			Debug.Assert(this.ModelObjects.Count == 0);
			ModelObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
		}


		protected void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			Debug.Assert(this.ModelObjects.Count == 0);
			foreach (IModelObject modelObject in modelObjects)
				ModelObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
		}


		protected void InsertModelObjects(bool insertShapes) {
			int cnt = ModelObjects.Count;
			if (cnt == 0) throw new nShapeInternalException("No ModelObjects set. Call SetModelObjects() before.");
			if (Repository != null) {
				Repository.InsertModelObjects(ModelObjects.Keys);
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in ModelObjects)
					InsertAndAttachObjects(item.Key, item.Value, Repository, insertShapes);
			}
		}


		protected void RemoveModelObjects(bool deleteShapes) {
			if (ModelObjects.Count == 0) throw new nShapeInternalException("No ModelObjects set. Call SetModelObjects() before.");
			if (Repository != null) {
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in ModelObjects)
					DetachAndDeleteObjects(item.Value, Repository, deleteShapes);
				Repository.DeleteModelObjects(ModelObjects.Keys);
			}
		}


		protected Dictionary<IModelObject, AttachedObjects> ModelObjects = new Dictionary<IModelObject, AttachedObjects>();


		protected struct AttachedObjects {

			public AttachedObjects(IModelObject modelObject, IRepository repository) {
				shapes = new List<Shape>();
				children = new Dictionary<IModelObject, AttachedObjects>();
				Add(modelObject, repository);
			}


			public List<Shape> Shapes {
				get { return shapes; }
			}


			public Dictionary<IModelObject, AttachedObjects> Children {
				get { return children; }
			}


			public void Add(IModelObject modelObject, IRepository repository) {
				DoAdd(this, modelObject, repository);
			}


			private void DoAdd(AttachedObjects attachedObjects, IModelObject modelObject, IRepository repository) {
				attachedObjects.Shapes.AddRange(modelObject.Shapes);
				foreach (IModelObject child in repository.GetModelObjects(modelObject))
					attachedObjects.Children.Add(child, new AttachedObjects(child, repository));
			}


			private List<Shape> shapes;
			private Dictionary<IModelObject, AttachedObjects> children;
		}


		private void DetachAndDeleteObjects(AttachedObjects attachedObjects, IRepository repository, bool deleteShapes) {
			// Delete or detach shapes
			if (attachedObjects.Shapes.Count > 0) {
				if (deleteShapes) repository.DeleteShapes(attachedObjects.Shapes);
				else {
					for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx) {
						attachedObjects.Shapes[sIdx].ModelObject = null;
					}
					repository.UpdateShapes(attachedObjects.Shapes);
				}
			}
			// Process children
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children)
				DetachAndDeleteObjects(child.Value, repository, deleteShapes);
			// Delete model object
			repository.DeleteModelObjects(attachedObjects.Children.Keys);
		}


		private void InsertAndAttachObjects(IModelObject modelObject, AttachedObjects attachedObjects, IRepository repository, bool insertShapes) {
			// Insert model objects
			repository.InsertModelObjects(attachedObjects.Children.Keys);
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children) {
				InsertAndAttachObjects(child.Key, child.Value, repository, insertShapes);
				child.Key.Parent = modelObject;
			}
			repository.UpdateModelObjects(attachedObjects.Children.Keys);
			// insert shapes
			if (attachedObjects.Shapes.Count > 0) {
				for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx)
					attachedObjects.Shapes[sIdx].ModelObject = modelObject;
				if (insertShapes)
					throw new NotImplementedException();
				else repository.UpdateShapes(attachedObjects.Shapes);
			}
		}

	}
	
	
	/// <summary>
	/// Base class for inserting and removing shapes along with their model objects to a diagram and a cache
	/// </summary>
	public abstract class InsertOrRemoveShapeAndModelCommand : InsertOrRemoveShapeCommand {
		
		protected InsertOrRemoveShapeAndModelCommand(Diagram diagram)
			: base(diagram) {
		}


		protected void InsertShapesAndModels(LayerIds activeLayers) {
			DoInsertShapesAndModels(false, activeLayers);
		}


		protected void InsertShapesAndModels() {
			DoInsertShapesAndModels(true, LayerIds.None);
		}


		protected void RemoveShapesAndModels() {
			//if (ModelObjects.Count == 0) throw new nShapeInternalException("No ModelObjects set. Call SetModelObjects() before.");
			if (Repository != null) {
				if (modelsAndObjects == null) {
					modelsAndObjects = new Dictionary<IModelObject, AttachedObjects>();
					foreach (IModelObject modelObject in ModelObjects)
						modelsAndObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
				}
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in modelsAndObjects)
					DetachAndDeleteObjects(item.Value, Repository);
				Repository.DeleteModelObjects(modelsAndObjects.Keys);
			}
			RemoveShapes();
		}


		protected void SetModelObjects(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			Debug.Assert(this.ModelObjects.Count == 0);
			this.ModelObjects.Clear();
			if (!this.ModelObjects.Contains(modelObject))
				this.ModelObjects.Add(modelObject);
		}


		protected void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			Debug.Assert(this.ModelObjects.Count == 0);
			this.ModelObjects.Clear();
			foreach (IModelObject modelObject in modelObjects) {
				if (!this.ModelObjects.Contains(modelObject))
					this.ModelObjects.Add(modelObject);
			}
		}


		protected void SetModelObjects(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			Debug.Assert(this.ModelObjects.Count == 0);
			this.ModelObjects.Clear();
			foreach (Shape shape in shapes) {
				if (shape.ModelObject != null
					&& !this.ModelObjects.Contains(shape.ModelObject))
					this.ModelObjects.Add(shape.ModelObject);
			}
		}


		protected struct AttachedObjects {

			public AttachedObjects(IModelObject modelObject, IRepository repository) {
				shapes = new List<Shape>();
				children = new Dictionary<IModelObject, AttachedObjects>();
				if (modelObject != null) Add(modelObject, repository);
			}


			public List<Shape> Shapes {
				get { return shapes; }
			}


			public Dictionary<IModelObject, AttachedObjects> Children {
				get { return children; }
			}


			public void Add(IModelObject modelObject, IRepository repository) {
				DoAdd(this, modelObject, repository);
			}


			private void DoAdd(AttachedObjects attachedObjects, IModelObject modelObject, IRepository repository) {
				if (modelObject != null) {
					attachedObjects.Shapes.AddRange(modelObject.Shapes);
					foreach (IModelObject child in repository.GetModelObjects(modelObject))
						attachedObjects.Children.Add(child, new AttachedObjects(child, repository));
				}
			}


			private List<Shape> shapes;
			private Dictionary<IModelObject, AttachedObjects> children;
		}


		protected List<IModelObject> ModelObjects {
			get { return modelObjects; }
		}


		protected static new string DeleteDescription = "Delete {0} shape{2} with {1}model{2}";


		protected static new string CreateDescription = "Create {0} shape{2} with {1}model{2}";


		private void DoInsertShapesAndModels(bool useOriginalLayers, LayerIds activeLayers) {
			int cnt = ModelObjects.Count;
			if (cnt == 0) throw new nShapeInternalException("No ModelObjects set. Call SetModelObjects() before.");
			if (useOriginalLayers)
				InsertShapes();
			else
				InsertShapes(activeLayers);
			
			if (Repository != null) {
				//if (cnt > 1)
				//   Repository.InsertModelObjects(ModelObjects);
				//else if (ModelObjects[0] != null)
				//   Repository.InsertModelObject(ModelObjects[0]);

				if (modelsAndObjects == null) {
					modelsAndObjects = new Dictionary<IModelObject, AttachedObjects>();
					foreach (IModelObject modelObject in ModelObjects)
						modelsAndObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
				}
				Repository.InsertModelObjects(modelsAndObjects.Keys);
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in modelsAndObjects)
					InsertAndAttachObjects(item.Key, item.Value, Repository);
			}
		}


		private void DetachAndDeleteObjects(AttachedObjects attachedObjects, IRepository repository) {
			for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx) {
				attachedObjects.Shapes[sIdx].ModelObject = null;
			}
			repository.UpdateShapes(attachedObjects.Shapes);
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children)
				DetachAndDeleteObjects(child.Value, repository);
			repository.DeleteModelObjects(attachedObjects.Children.Keys);
		}


		private void InsertAndAttachObjects(IModelObject modelObject, AttachedObjects attachedObjects, IRepository repository) {
			repository.InsertModelObjects(attachedObjects.Children.Keys);
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children) {
				InsertAndAttachObjects(child.Key, child.Value, repository);
				child.Key.Parent = modelObject;
			}
			repository.UpdateModelObjects(attachedObjects.Children.Keys);
			for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx)
				attachedObjects.Shapes[sIdx].ModelObject = modelObject;
			repository.UpdateShapes(attachedObjects.Shapes);
		}


		private Dictionary<IModelObject, AttachedObjects> modelsAndObjects = null;
		private List<IModelObject> modelObjects = new List<IModelObject>();
	}


	/// <summary>
	/// Base class for (un)aggregating shapes in ShapeAggregations
	/// </summary>
	public abstract class ShapeAggregationCommand : AutoDisconnectShapesCommand {

		protected ShapeAggregationCommand(Diagram diagram, Shape aggregationShape, IEnumerable<Shape> shapes)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (aggregationShape == null) throw new ArgumentNullException("aggregationShape");
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.diagram = diagram;
			this.aggregationShape = aggregationShape;
			this.shapes = new List<Shape>(shapes);
			aggregationLayerIds = LayerIds.None;
			for (int i = 0; i < this.shapes.Count; ++i)
				aggregationLayerIds |= this.shapes[i].Layers;
		}


		protected void CreateShapeAggregation(bool maintainZOrders) {
			// Remove shapes from repository, or otherwise they would be inserted twice
			if (Repository != null) Repository.DeleteShapes(shapes);
			diagram.Shapes.RemoveRange(shapes);

			// Add aggregation shape to diagram and repository
			diagram.Shapes.Add(aggregationShape);
			diagram.AddShapeToLayers(aggregationShape, aggregationLayerIds);

			// Add Shapes to aggregation shape
			if (maintainZOrders) {
				int cnt = shapes.Count;
				for (int i = 0; i < cnt; ++i)
					aggregationShape.Children.Add(shapes[i], shapes[i].ZOrder);
			} else aggregationShape.Children.AddRange(shapes);
			// As the shapes are already aggregated, they will be inserted along with the parent shape
			if (Repository != null) Repository.InsertShape(aggregationShape, diagram);

			// ToDo: Better use UpdateShapeOwner rather than DeleteShapes/InsertShapes
			//int cnt = shapes.Count;
			//if (Repository != null) {
			//   for (int i = 0; i < cnt; ++i)
			//      Repository.UpdateShapeOwner(shapes[i], this.aggregationShape);
			//}
			//if (maintainZOrders) {
			//   for (int i = 0; i < cnt; ++i)
			//      aggregationShape.Children.Add(shapes[i], shapes[i].ZOrder);
			//} else aggregationShape.Children.AddRange(shapes);
		}


		protected void DeleteShapeAggregation() {
			aggregationShape.Children.RemoveRange(shapes);
			diagram.Shapes.Remove(aggregationShape);
			if (Repository != null) {
				Repository.DeleteShapes(shapes);
				Repository.DeleteShape(aggregationShape);
			}
			diagram.Shapes.AddRange(shapes);
			if (Repository != null) Repository.InsertShapes(shapes, diagram);

			//int cnt = shapes.Count;
			//aggregationShape.Children.RemoveRange(shapes);
			//if (Repository != null) {
			//   for (int i = cnt - 1; i >= 0; --i)
			//      Repository.UpdateShapeOwner(shapes[i], diagram);
			//}
			//diagram.Shapes.AddRange(shapes);
		}


		protected Diagram diagram;
		protected List<Shape> shapes;
		protected LayerIds aggregationLayerIds;
		protected Shape aggregationShape;
	}


	/// <summary>
	/// Base class for Connecting and disconnecting two shapes
	/// </summary>
	public abstract class ConnectionCommand : Command {

		protected ConnectionCommand(Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId)
			: base() {
			if (connectorShape == null) throw new ArgumentNullException("connectorShape");
			if (targetShape == null) throw new ArgumentNullException("targetShape");
			this.connectorShape = connectorShape;
			this.gluePointId = gluePointId;
			this.targetShape = targetShape;
			this.targetPointId = targetPointId;
		}


		protected ConnectionCommand(Shape connectorShape, ControlPointId gluePointId)
			: base() {
			if (connectorShape == null) throw new ArgumentNullException("connectorShape");
			this.connectorShape = connectorShape;
			this.gluePointId = gluePointId;
			this.targetShape = null;
			this.targetPointId = ControlPointId.None;
			
			ShapeConnectionInfo sci = connectorShape.GetConnectionInfo(gluePointId, null);
			if (!sci.IsEmpty) {
				this.targetShape = sci.OtherShape;
				this.targetPointId = sci.OtherPointId;
			} throw new nShapeException("GluePoint {0} is not connected.", gluePointId);
		}


		protected void Connect() {
			connectorShape.Connect(gluePointId, targetShape, targetPointId);
			if (Repository != null) {
				Repository.UpdateShape(connectorShape);
				Repository.UpdateShape(targetShape);
				Repository.InsertShapeConnection(connectorShape, gluePointId, targetShape, targetPointId);
			}
		}


		protected void Disconnect() {
			connectorShape.Disconnect(gluePointId);

			if (Repository != null) {
				Repository.UpdateShape(connectorShape);
				Repository.UpdateShape(targetShape);
				Repository.DeleteShapeConnection(connectorShape, gluePointId, targetShape, targetPointId);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Connect; }
		}


		protected Shape connectorShape;
		protected Shape targetShape;
		protected ControlPointId gluePointId;
		protected ControlPointId targetPointId;
	}


	public abstract class InsertOrRemoveLayerCommand : Command {
		
		protected InsertOrRemoveLayerCommand(Diagram diagram, string layerName)
			: base() {
			Construct(diagram);
			if (layerName == null) throw new ArgumentNullException("layerName");
			//this.layerName = layerName;
			layers = new List<Layer>(1);
			Layer l = this.diagram.Layers.FindLayer(layerName);
			if (l == null) l = new Layer(layerName);
			layers.Add(l);
		}


		protected InsertOrRemoveLayerCommand(Diagram diagram, Layer layer)
			: base() {
			Construct(diagram);
			layers = new List<Layer>(1);
			layers.Add(layer);
			//layerName = layer.Name;
		}


		protected InsertOrRemoveLayerCommand(Diagram diagram, IEnumerable<Layer> layers)
			: base() {
			Construct(diagram);
			this.layers = new List<Layer>(layers);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout;}
		}


		protected void AddLayers() {
			for (int i = 0; i < layers.Count; ++i) {
				diagram.Layers.Add(layers[i]);
				if (Repository != null) Repository.UpdateDiagram(diagram);
			}
		}


		protected void RemoveLayers() {
			for (int i = 0; i < layers.Count; ++i) {
				diagram.Layers.Remove(layers[i]);
				if (Repository != null) Repository.UpdateDiagram(diagram);
			}
		}


		private void Construct(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
		}


		#region Fields
		protected Diagram diagram;
		protected List<Layer> layers = null;
		#endregion
	}

	#endregion


	#region AggregatedCommand class

	/// <summary>
	/// Executes a list of commands
	/// The Label of this command is created by concatenating the labels of each command.
	/// </summary>
	public class AggregatedCommand : Command {

		public AggregatedCommand()
			: base() {
			commands = new List<ICommand>();
			description = string.Empty;
		}


		public AggregatedCommand(IEnumerable<ICommand> commands)
			: base() {
			if (commands == null) throw new ArgumentNullException("commands");
			commands = new List<ICommand>(commands);
			CreateLabelString();
		}


		public override string Description {
			get {
				if (string.IsNullOrEmpty(description))
					CreateLabelString();
				return base.Description;
			}
		}


		public int CommandCount { get { return commands.Count; } }


		public void Add(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			command.Repository = Repository;
			commands.Add(command);
			description = string.Empty;
		}


		public void Insert(int index, ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			command.Repository = Repository;
			commands.Add(command);
			description = string.Empty;
		}


		public void Remove(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			RemoveAt(commands.IndexOf(command));
			description = string.Empty;
		}


		public void RemoveAt(int index) {
			commands.RemoveAt(index);
			description = string.Empty;
		}


		public override void Execute() {
			for (int i = 0; i < commands.Count; ++i) {
				if (commands[i].Repository != Repository) 
					commands[i].Repository = Repository;
				commands[i].Execute();
			}
		}


		public override void Revert() {
			for (int i = commands.Count - 1; i >= 0; --i) {
				if (commands[i].Repository != Repository)
					commands[i].Repository = Repository;
				commands[i].Revert();
			}
		}


		public override Permission RequiredPermission {
			get { 
			   Permission requiredPermission = Permission.None;
				for (int i = 0; i < commands.Count; ++i) {
					if (commands[i] is Command) 
						requiredPermission = ((Command)commands[i]).RequiredPermission;
				}
			   return requiredPermission;
			}
		}


		public override bool IsAllowed(ISecurityManager security) {
			if (security == null) throw new ArgumentNullException("security");
			bool result = true;
			for (int i = 0; i < commands.Count; ++i) {
				if (!commands[i].IsAllowed(security)) {
					result = false;
					break;
				}
			}
			return result;
		}
		
		
		private void CreateLabelString() {
			description = string.Empty;
			if (commands.Count > 0) {
				string newLine = commands.Count > 3 ? "\n" : "";
				description = commands[0].Description;
				int lastIdx = commands.Count - 1;
				for (int i = 1; i <= lastIdx; ++i) {
					if (i < lastIdx)
						description += string.Format(", {0}{1}{2}", newLine, commands[i].Description.Substring(0, 1).ToLowerInvariant(), commands[i].Description.Substring(1));
					else
						description += string.Format(" and {0}{1}{2}", newLine, commands[i].Description.Substring(0, 1).ToLowerInvariant(), commands[i].Description.Substring(1));
				}
			}
		}


		List<ICommand> commands;
	}

	#endregion


	# region ConnectCommand class
	/// <summary>
	/// Command for connecting a shape's GluePoint to an other shape's GluePoint
	/// </summary>
	public class ConnectCommand : ConnectionCommand {
		
		public ConnectCommand(Shape connectorShape, int gluePointId, Shape targetShape, int targetPointId)
			: base(connectorShape, gluePointId, targetShape, targetPointId) {
			this.description = string.Format("Connect {0} to {1}", connectorShape.Type.Name, targetShape.Type.Name);
		}


		public override void Execute() {
			Connect();
		}


		public override void Revert() {
			Disconnect();
		}

	}
	#endregion


	#region DisconnectCommand
	public class DisconnectCommand : ConnectionCommand {
		
		public DisconnectCommand(Shape connectorShape, int gluePointId)
			: base(connectorShape, gluePointId) {
			this.description = string.Format("Disconnect {0}", connectorShape.Type.Name);
			this.connectorShape = connectorShape;
			this.gluePointId = gluePointId;

			this.connectionInfo = connectorShape.GetConnectionInfo(gluePointId, null);
			if (this.connectionInfo.IsEmpty)
				throw new nShapeInternalException(string.Format("There is no connection for Point {0} of shape {1}.", gluePointId, connectorShape));
		}


		public override void Execute() {
			connectorShape.Disconnect(gluePointId);
			Repository.UpdateShape(connectorShape);
			Repository.UpdateShape(connectionInfo.OtherShape);
			Repository.DeleteShapeConnection(connectorShape, gluePointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
		}


		public override void Revert() {
			connectorShape.Connect(gluePointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
			Repository.UpdateShape(connectorShape);
			Repository.UpdateShape(connectionInfo.OtherShape);
			Repository.InsertShapeConnection(connectorShape, gluePointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
		}


		private ShapeConnectionInfo connectionInfo;
	}
	#endregion


	#region MoveShapeByCommand class

	public class MoveShapeByCommand : Command {

		public MoveShapeByCommand(Shape shape, int dX, int dY)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			this.description = string.Format("Move {0}", shape.Type.Name);
			this.shapes = new List<Shape>(1);
			this.shapes.Add(shape);
			this.dX = dX;
			this.dY = dY;
		}


		public MoveShapeByCommand(IEnumerable<Shape> shapes, int dX, int dY)
			: base() {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.shapes = new List<Shape>();

			// sort shapes:
			// move shapes with GluePoints first, then move all other shapes
			foreach (Shape shape in shapes) {
				bool hasGluePoint = false;
				foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
					hasGluePoint = true;
					break;
				}
				if (hasGluePoint)
					this.shapes.Insert(0, shape);
				else
					this.shapes.Add(shape);
			}

			// collect connections to remove temporarily
			for (int i = 0; i < this.shapes.Count; ++i) {
				if (!IsConnectedToNonSelectedShapes(this.shapes[i])) {
					foreach (ControlPointId gluePointId in this.shapes[i].GetControlPointIds(ControlPointCapabilities.Glue)) {
						ShapeConnectionInfo gluePointConnectionInfo = this.shapes[i].GetConnectionInfo(gluePointId, null);
						if (!gluePointConnectionInfo.IsEmpty) {
							ConnectionInfoBuffer connInfoBuffer;
							connInfoBuffer.shape = this.shapes[i];
							connInfoBuffer.connectionInfo = gluePointConnectionInfo;

							if (connectionsBuffer == null)
								connectionsBuffer = new List<ConnectionInfoBuffer>();
							connectionsBuffer.Add(connInfoBuffer);
						}
					}
				}
			}

			this.dX = dX;
			this.dY = dY;

			this.description = string.Format("Move {0} shape", this.shapes.Count);
			if (this.shapes.Count > 1) this.description += "s";
		}


		private bool IsConnectedToNonSelectedShapes(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty && shapes.IndexOf(sci.OtherShape) < 0)
					return true;
			}
			return false;
		}


		public override void Execute() {
			// remove connections temporarily
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Disconnect(connectionsBuffer[i].connectionInfo.OwnPointId);
			}
			// move shapes
			int cnt = shapes.Count;
			for (int i = 0; i < cnt; ++i)
				shapes[i].MoveControlPointBy(ControlPointId.Reference, dX, dY, ResizeModifiers.None);
			// restore temporarily removed connections between selected shapes
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Connect(connectionsBuffer[i].connectionInfo.OwnPointId, connectionsBuffer[i].connectionInfo.OtherShape, connectionsBuffer[i].connectionInfo.OtherPointId);
			}
			// update cache
			if (Repository != null) Repository.UpdateShapes(shapes);
		}


		public override void Revert() {
			// remove connections temporarily
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Disconnect(connectionsBuffer[i].connectionInfo.OwnPointId);
			}

			// move shapes
			for (int i = 0; i < shapes.Count; ++i)
				shapes[i].MoveControlPointBy(ControlPointId.Reference, -dX, -dY, ResizeModifiers.None);

			// restore temporarily removed connections between selected shapes
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Connect(connectionsBuffer[i].connectionInfo.OwnPointId, connectionsBuffer[i].connectionInfo.OtherShape, connectionsBuffer[i].connectionInfo.OtherPointId);
			}

			if (Repository != null) Repository.UpdateShapes(shapes);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private int dX, dY;
		private List<Shape> shapes;
		private struct ConnectionInfoBuffer {
			internal Shape shape;
			internal ShapeConnectionInfo connectionInfo;
		}
		private List<ConnectionInfoBuffer> connectionsBuffer;	// buffer used for storing connections that are temporarily disconnected for moving shapes
	}
	#endregion


	#region MoveShapesCommand class

	/// <summary>
	/// Moves multiple shapes by individual distances.
	/// </summary>
	public class MoveShapesCommand : Command {

		public MoveShapesCommand()
			: base() {
		}


		public void AddMove(Shape shape, int dx, int dy) {
			if (shape == null) throw new ArgumentNullException("shape");
			ShapeMove sm;
			sm.shape = shape;
			sm.dx = dx;
			sm.dy = dy;
			shapeMoves.Add(sm);
		}


		#region Base Class Implementation

		public override void Execute() {
			foreach (ShapeMove sm in shapeMoves)
				sm.shape.MoveControlPointBy(ControlPointId.Reference, sm.dx, sm.dy, ResizeModifiers.None);
		}


		public override void Revert() {
			foreach (ShapeMove sm in shapeMoves)
				sm.shape.MoveControlPointBy(ControlPointId.Reference, -sm.dx, -sm.dy, ResizeModifiers.None);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		protected struct ShapeMove {
			public Shape shape;
			public int dx;
			public int dy;
		}


		private List<ShapeMove> shapeMoves = new List<ShapeMove>();

		#endregion
	}

	#endregion


	#region MoveControlPointCommand class
	public class MoveControlPointCommand : Command {
		public MoveControlPointCommand(Shape shape, int controlPointId, int dX, int dY, ResizeModifiers modifiers)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			this.description = string.Format("Move control point of {0}", shape.Type.Name);
			this.shapes = new List<Shape>(1);
			this.shapes.Add(shape);
			this.controlPointId = controlPointId;
			this.dX = dX;
			this.dY = dY;
			this.modifiers = modifiers;
		}


		public MoveControlPointCommand(IEnumerable<Shape> shapes, int controlPointId, int dX, int dY, ResizeModifiers modifiers)
			: base() {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.shapes = new List<Shape>(shapes);
			this.description = string.Format("Move {0} control point{1}", this.shapes.Count, this.shapes.Count > 1 ? "s" : "");

			this.controlPointId = controlPointId;
			this.dX = dX;
			this.dY = dY;
			this.modifiers = modifiers;
		}


		public override void Execute() {
			for (int i = 0; i < shapes.Count; ++i)
				shapes[i].MoveControlPointBy(controlPointId, dX, dY, modifiers);
			
			if (Repository != null) Repository.UpdateShapes(shapes);
		}


		public override void Revert() {
			for (int i = 0; i < shapes.Count; ++i)
				shapes[i].MoveControlPointBy(controlPointId, -dX, -dY, modifiers);

			if (Repository != null) Repository.UpdateShapes(shapes);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private int controlPointId;
		private int dX, dY;
		private ResizeModifiers modifiers;
		private List<Shape> shapes;
	}
	#endregion


	#region MoveGluePointCommand

	public class MoveGluePointCommand : Command {

		/// <summary>
		/// Constructs a glue point moving command.
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="ownPointId"></param>
		/// <param name="otherShape"></param>
		/// <param name="dX"></param>
		/// <param name="dY"></param>
		/// <param name="modifiers"></param>
		public MoveGluePointCommand(Shape shape, int gluePointId, Shape targetShape, int dX, int dY, ResizeModifiers modifiers)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			this.shape = shape;
			this.targetShape = targetShape;
			this.gluePointId = gluePointId;
			this.dX = dX;
			this.dY = dY;
			this.modifiers = modifiers;
			// store original position of gluePoint (cannot be restored with dX/dY in case of PointToShape connections)
			gluePointPosition = shape.GetControlPointPosition(gluePointId);
			// store existing connection
			existingConnection = shape.GetConnectionInfo(this.gluePointId, null);

			// create new ConnectionInfo
			int targetPointId = ControlPointId.None;
			if (targetShape != null) {
				targetPointId = targetShape.FindNearestControlPoint(gluePointPosition.X + dX, gluePointPosition.Y + dY, 0, ControlPointCapabilities.Connect);
				if (targetPointId == ControlPointId.None && targetShape.ContainsPoint(gluePointPosition.X + dX, gluePointPosition.Y + dY))
					targetPointId = ControlPointId.Reference;
			}
			if (targetShape != null && targetPointId != ControlPointId.None)
				this.newConnection = new ShapeConnectionInfo(this.gluePointId, targetShape, targetPointId);
			// set description
			if (!existingConnection.IsEmpty) {
				this.description = string.Format("Disconnect {0} from {1}", shape.Type.Name, existingConnection.OtherShape.Type.Name);
				if (!newConnection.IsEmpty)
					this.description += string.Format(" and connect to {0}", newConnection.OtherShape.Type.Name);
			} else {
				if (!newConnection.IsEmpty)
					this.description += string.Format("Connect {0} to {1}", shape.Type.Name, newConnection.OtherShape.Type.Name);
				else
					this.description = string.Format("Move glue point {0} of {1}", gluePointId, shape.Type.Name);
			}
		}


		//public MoveGluePointCommand(IEnumerable<Shape> shapes, int ownPointId, int dX, int dY, IEnumerable<Shape> targetShapes, ResizeModifiers modifiers)
		//   : base() {
		//   this.shapes = new List<Shape>(shapes);
		//   this.ownPointId = ownPointId;
		//   this.description = string.Format("Move {0} glue point{1}", this.shapes.Count, this.shapes.Count > 1 ? "s" : "");
		//   this.dX = dX;
		//   this.dY = dY;

		//   this.gluePointPositions = new List<Point>(this.shapes.Count);
		//   List<Shape> targets = new List<Shape>(targetShapes);
		//   this.existingConnections = new List<ShapeConnectionInfo>(this.shapes.Count);
		//   this.newConnections = new List<ShapeConnectionInfo>(this.shapes.Count);

		//   Shape passiveShape = null;
		//   int connectionPointId = ControlPointId.None;
		//   for (int idx = 0; idx < this.shapes.Count; ++idx) {
		//      // store original position of gluePoint (cannot be restored with dX/dY in case of PointToShape connections)
		//      gluePointPositions.Add(this.shapes[idx].GetControlPointPosition(this.ownPointId));

		//      // get existing connection
		//      foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(this.gluePointId, null)) this.existingConnections.Add(sci);

		//      // get new connection
		//      passiveShape = null;
		//      connectionPointId = ControlPointId.None;
		//      for (int i = 0; i < targets.Count; ++i) {
		//         if (targets[i] != null) {
		//            connectionPointId = targets[i].FindNearestControlPoint(gluePointPositions[idx].X + dX, gluePointPositions[idx].Y + dY, 0, ControlPointCapabilities.AttachGluePointToConnectionPoint);
		//            if (connectionPointId != ControlPointId.None) {
		//               passiveShape = targets[i];
		//               break;
		//            }
		//         }
		//      }
		//      if (passiveShape != null && connectionPointId != ControlPointId.NotSupported)
		//         this.newConnections.Add(new ShapeConnectionInfo(this.ownPointId, passiveShape, connectionPointId));
		//      else
		//         this.newConnections.Add(ShapeConnectionInfo.Empty);
		//   }
		//   this.modifiers = modifiers;
		//}


		public override void Execute() {
			// DetachGluePointFromConnectionPoint existing connection
			if (!existingConnection.IsEmpty) {
				shape.Disconnect(gluePointId);
				if (Repository != null) Repository.DeleteShapeConnection(shape, gluePointId, existingConnection.OtherShape, existingConnection.OtherPointId);
			}
			// Move point
			shape.MoveControlPointBy(gluePointId, dX, dY, modifiers);
			// Establish new connection
			if (!newConnection.IsEmpty) {
				shape.Connect(gluePointId, newConnection.OtherShape, newConnection.OtherPointId);
				if (Repository != null) Repository.InsertShapeConnection(shape, gluePointId, newConnection.OtherShape, newConnection.OtherPointId);
			}
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override void Revert() {
			// DetachGluePointFromConnectionPoint new connection
			if (!newConnection.IsEmpty) {
				shape.Disconnect(gluePointId);
				if (Repository != null) Repository.DeleteShapeConnection(shape, gluePointId, newConnection.OtherShape, newConnection.OtherPointId);
			}
			// Move point
			shape.MoveControlPointTo(gluePointId, gluePointPosition.X, gluePointPosition.Y, modifiers);
			// Restore previous connection
			if (!existingConnection.IsEmpty) {
				shape.Connect(gluePointId, existingConnection.OtherShape, existingConnection.OtherPointId);
				if (Repository != null) Repository.InsertShapeConnection(shape, gluePointId, existingConnection.OtherShape, existingConnection.OtherPointId);
			}
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout | Permission.Connect; }
		}


		#region Fields
		private Shape shape;
		private Shape targetShape;
		private Point gluePointPosition;
		private ControlPointId gluePointId;
		private int dX;
		private int dY;
		private ResizeModifiers modifiers;
		private ShapeConnectionInfo existingConnection;
		private ShapeConnectionInfo newConnection = ShapeConnectionInfo.Empty;
		#endregion
	}
	#endregion


	#region RotateShapesCommand class

	/// <summary>
	/// Rotates all members of a set of shapes by the same angle.
	/// </summary>
	public class RotateShapesCommand : Command {

		public RotateShapesCommand(IEnumerable<Shape> shapes, int tenthsOfDegree)
			: base() {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.shapes = new List<Shape>(shapes);
			this.angle = tenthsOfDegree;
			if (this.shapes.Count == 1)
				this.description = string.Format("Rotate {0} by {1}°", this.shapes[0].Type.Name, tenthsOfDegree / 10f);
			else
				this.description = string.Format("Rotate {0} shapes by {1}°", this.shapes.Count, tenthsOfDegree / 10f);
		}


		public RotateShapesCommand(IEnumerable<Shape> shapes, int tenthsOfDegree, int rotateCenterX, int rotateCenterY)
			: base() {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.shapes = new List<Shape>(shapes);
			for (int i = 0; i < this.shapes.Count; ++i) {
				if (this.shapes[i] is ILinearShape) {
					List<Point> points = new List<Point>();
					foreach (ControlPointId id in this.shapes[i].GetControlPointIds(ControlPointCapabilities.Resize)) {
						Point p = this.shapes[i].GetControlPointPosition(id);
						points.Add(p);
					}
					unrotatedLinePoints.Add((ILinearShape)this.shapes[i], points);
				}
			}
			this.angle = tenthsOfDegree;
			this.rotateCenterX = rotateCenterX;
			this.rotateCenterY = rotateCenterY;
			if (this.shapes.Count == 1)
				this.description = string.Format("Rotate {0} by {1}° at ({2}|{3}", this.shapes[0].Type.Name, tenthsOfDegree / 10f, rotateCenterX, rotateCenterY);
			else
				this.description = string.Format("Rotate {0} shapes by {1}° at ({2}|{3}", this.shapes.Count, tenthsOfDegree / 10f, rotateCenterX, rotateCenterY);
		}


		public override void Execute() {
			foreach (Shape shape in shapes) {
				if (rotateCenterX == int.MinValue && rotateCenterY == int.MinValue)
					shape.Rotate(angle, shape.X, shape.Y);
				else
					shape.Rotate(angle, rotateCenterX, rotateCenterY);
			}
			if (Repository != null) Repository.UpdateShapes(shapes);
		}


		public override void Revert() {
			foreach (Shape shape in shapes) {
				if (rotateCenterX == int.MinValue && rotateCenterY == int.MinValue)
					shape.Rotate(-angle, shape.X, shape.Y);
				else {
					if (shape is ILinearShape) {
						// restore line points
						Point p = Point.Empty;
						List<Point> unrotatedPts;
						unrotatedLinePoints.TryGetValue((ILinearShape)shape, out unrotatedPts);
						foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Resize)) {
							p = shape.GetControlPointPosition(id);
							shape.MoveControlPointTo(id, p.X, p.Y, ResizeModifiers.None);
						}
					} else
						shape.Rotate(-angle, rotateCenterX, rotateCenterY);
				}
			}
			if (Repository != null) Repository.UpdateShapes(shapes);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private int angle;
		private List<Shape> shapes;
		private int rotateCenterX = int.MinValue;
		private int rotateCenterY = int.MinValue;
		private Dictionary<ILinearShape, List<Point>> unrotatedLinePoints = new Dictionary<ILinearShape, List<Point>>();
	}

	#endregion


	#region SetCaptionTextCommand class

	public class SetCaptionTextCommand : Command {

		public SetCaptionTextCommand(ICaptionedShape shape, int captionIndex, string newValue)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			if (!(shape is Shape)) throw new nShapeException("{0} is not of type {1}.", shape.GetType().Name, typeof(Shape).Name);
			this.modifiedLabeledShapes = new List<ICaptionedShape>(1);
			this.modifiedLabeledShapes.Add(shape);
			this.labelIndex = captionIndex;
			this.oldValue = shape.GetCaptionText(captionIndex);
			this.newValue = newValue;
			this.description = string.Format("Change text of {0} from '{1}' to '{2}", ((Shape)shape).Type.Name, this.oldValue, this.newValue);
		}


		public override void Execute() {
			//int newValuesCnt = 1;
			for (int i = 0; i < modifiedLabeledShapes.Count; ++i) {
				//if (newValuesCnt > 1)
				//    propertyInfo.SetFloat(modifiedLabeledShapes[i], newValues[i], null);
				//else
				modifiedLabeledShapes[i].SetCaptionText(labelIndex, newValue);
				if (Repository != null) Repository.UpdateShape((Shape)modifiedLabeledShapes[i]);
			}
		}


		public override void Revert() {
			//int newValuesCnt = 1;
			for (int i = 0; i < modifiedLabeledShapes.Count; ++i) {
				//if (newValuesCnt > 1)
				//    propertyInfo.SetFloat(modifiedLabeledShapes[i], newValues[i], null);
				//else
				modifiedLabeledShapes[i].SetCaptionText(labelIndex, oldValue);
				if (Repository != null) Repository.UpdateShape((Shape)modifiedLabeledShapes[i]);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Present | Permission.ModifyData | Permission.Layout; }
		}


		#region Fields
		private int labelIndex;
		private string oldValue;
		private string newValue;
		private List<ICaptionedShape> modifiedLabeledShapes;
		#endregion
	}
	#endregion


	#region ShapePropertySetCommand class
	public class ShapePropertySetCommand : Command {
		public ShapePropertySetCommand(IEnumerable<Shape> modifiedShapes, PropertyInfo propertyInfo, object[] oldValues, object[] newValues)
			: base() {
			if (modifiedShapes == null) throw new ArgumentNullException("modifiedShapes");
			Construct(propertyInfo, oldValues.Length, newValues.Length);

			this.modifiedShapes = new List<Shape>(modifiedShapes);
			Array.Copy(oldValues, this.oldValues, oldValues.Length);
			Array.Copy(newValues, this.newValues, newValues.Length);
			this.description = string.Format("Change property '{0}' of {1} shapes", propertyInfo.Name, this.modifiedShapes.Count);
		}


		public ShapePropertySetCommand(IEnumerable<Shape> modifiedShapes, PropertyInfo propertyInfo, object[] oldValues, object newValue)
			: base() {
			if (modifiedShapes == null) throw new ArgumentNullException("modifiedShapes");
			Construct(propertyInfo, oldValues.Length, 1);

			this.modifiedShapes = new List<Shape>(modifiedShapes);
			Array.Copy(oldValues, this.oldValues, oldValues.Length);
			this.newValues[0] = newValue;
			if (oldValues.Length == 1)
				this.description = string.Format("Change property '{0}' of {1} shapes from '{2}' to '{3}'", propertyInfo.Name, this.modifiedShapes.Count, oldValues[0], newValue);
			else
				this.description = string.Format("Change property '{0}' of {1} shapes to '{2}'", propertyInfo.Name, this.modifiedShapes.Count, newValue);
		}


		public ShapePropertySetCommand(Shape modifiedShape, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base() {
			if (modifiedShape == null) throw new ArgumentNullException("modifiedShape");
			Construct(propertyInfo, 1, 1);

			this.modifiedShapes = new List<Shape>(1);
			this.modifiedShapes.Add(modifiedShape);
			this.oldValues[0] = oldValue;
			this.newValues[0] = newValue;
			this.description = string.Format("Change property '{0}' of {1} from '{2}' to '{3}'", propertyInfo.Name, modifiedShape.Type.Name, oldValue, newValue);
		}


		public override void Execute() {
			int newValuesCnt = newValues.Length;
			int shapeCnt = modifiedShapes.Count;
			for (int i = 0; i < shapeCnt; ++i) {
				object newValue = (newValuesCnt > 1) ? newValues[i] : newValues[0];

				// Check if the new value has been set already (e.g. by the PropertyGrid). If the new value is 
				// already set, skip setting the new value (again).
				// This check is necessary because if the value is a value that is exclusive-or'ed when set 
				// (e.g. a FontStyle), the change would be undone when setting the value again
				bool performSetValue = true;
				if (newValue != null) {
					object currentValue = propertyInfo.GetValue(modifiedShapes[i], null);
					performSetValue = !newValue.Equals(currentValue);
				}
				if (performSetValue) propertyInfo.SetValue(modifiedShapes[i], newValue, null);
			}
			if (Repository != null) {
				if (shapeCnt == 1)
					Repository.UpdateShape(modifiedShapes[0]);
				else Repository.UpdateShapes(modifiedShapes);
			}
		}


		public override void Revert() {
			int oldValuesCnt = oldValues.Length;
			int shapeCnt = modifiedShapes.Count;
			for (int i = 0; i < shapeCnt; ++i) {
				object oldValue = (oldValuesCnt > 1) ? oldValues[i] : oldValues[0];
				bool skipSettingProperty = true;
				if (oldValue != null)
					skipSettingProperty = oldValue.Equals(propertyInfo.GetValue(modifiedShapes[i], null));
				if (!skipSettingProperty) propertyInfo.SetValue(modifiedShapes[i], oldValue, null);
			}
			if (Repository != null) {
				if (shapeCnt == 1)
					Repository.UpdateShape(modifiedShapes[0]);
				else Repository.UpdateShapes(modifiedShapes);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Present | Permission.ModifyData | Permission.Layout; }
		}


		private void Construct(PropertyInfo propertyInfo, int oldValueCount, int newValueCount) {
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.propertyInfo = propertyInfo;
			this.oldValues = new object[oldValueCount];
			this.newValues = new object[newValueCount];
		}


		#region Fields
		private PropertyInfo propertyInfo;
		private object[] oldValues;
		private object[] newValues;
		private List<Shape> modifiedShapes;
		#endregion
	}
	#endregion


	#region SetShapeZOrderCommand class
	public class LiftShapeCommandCommand : Command {

		public LiftShapeCommandCommand(Diagram diagram, IEnumerable<Shape> shapes, ZOrderDestination liftMode)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.modifiedShapes = new ShapeCollection(shapes);
			Construct(diagram, liftMode);
		}


		public LiftShapeCommandCommand(Diagram diagram, Shape shape, ZOrderDestination liftMode)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shape == null) throw new ArgumentNullException("shape");
			this.modifiedShapes = new ShapeCollection(1);
			this.modifiedShapes.Add(shape);
			Construct(diagram, liftMode);
		}


		public override void Execute() {
			// store current and new ZOrders
			if (zOrderInfos == null || zOrderInfos.Count == 0)
				ObtainZOrders();

			// process topDown/bottomUp to avoid ZOrder-sorting inside the diagram's ShapeCollection
			switch (liftMode) {
				case ZOrderDestination.ToBottom:
					foreach (Shape shape in modifiedShapes.TopDown) {
						ZOrderInfo info = zOrderInfos[shape];
						PerformZOrderChange(shape, info.newZOrder, info.layerIds);
					}
					break;
				case ZOrderDestination.ToTop:
					foreach (Shape shape in modifiedShapes.BottomUp) {
						ZOrderInfo info = zOrderInfos[shape];
						PerformZOrderChange(shape, info.newZOrder, info.layerIds);
					}
					break;
				default:
					throw new nShapeUnsupportedValueException(liftMode);
			}
			if (Repository != null) Repository.UpdateShapes(modifiedShapes);
		}


		public override void Revert() {
			Debug.Assert(zOrderInfos != null);
			foreach (Shape shape in modifiedShapes.BottomUp) {
				ZOrderInfo info = zOrderInfos[shape];
				PerformZOrderChange(shape, info.origZOrder, info.layerIds);
			}
			if (Repository != null) Repository.UpdateShapes(modifiedShapes);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private void Construct(Diagram diagram, ZOrderDestination liftMode) {
			this.diagram = diagram;
			this.liftMode = liftMode;
			string formatStr = string.Empty;
			switch (liftMode) {
				case ZOrderDestination.ToTop: formatStr = "Bring {0} shape{1} on top"; break;
				case ZOrderDestination.ToBottom: formatStr = "Send {0} shape{1} to bottom"; break;
				default: throw new nShapeUnsupportedValueException(liftMode);
			}
			if (modifiedShapes.Count == 1)
				this.description = string.Format(formatStr, modifiedShapes.TopMost.Type.Name, string.Empty);
			else this.description = string.Format(formatStr, modifiedShapes.Count, "s");
		}


		private void ObtainZOrders() {
			zOrderInfos = new Dictionary<Shape, ZOrderInfo>(modifiedShapes.Count);
			switch (liftMode) {
				case ZOrderDestination.ToBottom:
					if (Repository != null) {
						foreach (Shape shape in modifiedShapes.TopDown)
							zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, Repository.ObtainNewBottomZOrder(diagram), shape.Layers));
					} else {
						foreach (Shape shape in modifiedShapes.TopDown)
							zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, diagram.Shapes.MinZOrder, shape.Layers));
					}
					break;
				case ZOrderDestination.ToTop:
					if (Repository != null) {
						foreach (Shape shape in modifiedShapes.BottomUp)
							zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, Repository.ObtainNewTopZOrder(diagram), shape.Layers));
					} else {
						foreach (Shape shape in modifiedShapes.BottomUp)
							zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, diagram.Shapes.MaxZOrder, shape.Layers));
					}
					break;
				default:
					throw new nShapeUnsupportedValueException(liftMode);
			}
		}
		
		
		private void PerformZOrderChange(Shape shape, int zOrder, LayerIds layerIds) {
			diagram.Shapes.SetZOrder(shape, zOrder);
			
			//// remove shape from Diagram
			//diagram.Shapes.Remove(shape);
			//// restore the original ZOrder value
			//shape.ZOrder = zOrder;
			//// re-insert the shape on its previous position
			//diagram.Shapes.Add(shape);
			//diagram.AddShapeToLayers(shape, layerIds);
		}


		private class ZOrderInfo {
			public ZOrderInfo(int origZOrder, int newZOrder, LayerIds layerIds) {
				this.origZOrder = origZOrder;
				this.newZOrder = newZOrder;
				this.layerIds = layerIds;
			}
			public int origZOrder;
			public int newZOrder;
			public LayerIds layerIds;
		}
		
		#region Fields
		private Diagram diagram;
		private ZOrderDestination liftMode;
		private ShapeCollection modifiedShapes;
		private Dictionary<Shape, ZOrderInfo> zOrderInfos;
		#endregion
	}
	#endregion


	#region ChangeModelObjectParentCommand class
	public class SetModelObjectParentCommand : Command {
		
		public SetModelObjectParentCommand(IModelObject modelObject, IModelObject parent)
			: base() {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			if (modelObject.Parent != null && parent == null)
				this.description = string.Format(
					"Remove {0} '{1}' from hierarchical position under {2} '{3}'.",
					modelObject.Type.Name, modelObject.Name,
					modelObject.Parent.Type.Name, modelObject.Parent.Name);
			else if (modelObject.Parent == null && parent != null)
				this.description = string.Format(
					"Move {0} '{1}' to hierarchical position under {2} '{3}'.",
					modelObject.Type.Name, modelObject.Name,
					parent.Type.Name, parent.Name);
			else if (modelObject.Parent != null && parent != null)
				this.description = string.Format(
					"Change hierarchical position of {0} '{1}' from {2} '{3}' to {4} '{5}'.",
					modelObject.Type.Name, modelObject.Name,
					modelObject.Parent.Type.Name, modelObject.Parent.Name,
					parent.Type.Name, parent.Name);
			else this.description = string.Format("Move {0} '{1}'.", modelObject.Type.Name, modelObject.Name);

			this.modelObject = modelObject;
			this.oldParent = modelObject.Parent;
			this.newParent = parent;
		}


		public override void Execute() {
			modelObject.Parent = newParent;
			if (Repository != null) Repository.UpdateModelObjectParent(modelObject, newParent);
		}


		public override void Revert() {
			modelObject.Parent = oldParent;
			if (Repository != null) Repository.UpdateModelObjectParent(modelObject, oldParent);
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData; }
		}


		#region Fields
		private IModelObject modelObject;
		private IModelObject oldParent;
		private IModelObject newParent;
		#endregion
	}
	#endregion


	#region AssignModelObjectCommand class
	public class AssignModelObjectCommand : Command {

		public AssignModelObjectCommand(Shape shape, IModelObject modelObject)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			if (modelObject == null) throw new ArgumentNullException("modelObject");

			if (shape.ModelObject == null)
				this.description = string.Format("Assign {0} '{1}' to {2}.", modelObject.Type.Name, modelObject.Name, shape.Type.Name);
			else
				this.description = string.Format("Replace {0} '{1}' of {2} with {3} '{4}'.", shape.ModelObject.Type.Name, shape.ModelObject.Name, shape.Type.Name, modelObject.Type.Name, modelObject.Name);

			this.shape = shape;
			this.oldModelObject = shape.ModelObject;
			this.newModelObject = modelObject;
		}


		public override void Execute() {
			shape.ModelObject = newModelObject;
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override void Revert() {
			shape.ModelObject = oldModelObject;
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData; }
		}


		#region Fields
		private Shape shape;
		private IModelObject oldModelObject;
		private IModelObject newModelObject;
		#endregion
	}
	#endregion


	#region ModelObjectPropertySetCommand class
	public class ModelObjectPropertySetCommand : Command {
		public ModelObjectPropertySetCommand(IEnumerable<IModelObject> modifiedModelObjects, PropertyInfo propertyInfo, object[] oldValues, object newValue)
			: base() {
			if (modifiedModelObjects == null) throw new ArgumentNullException("modifiedModelObjects");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.modifiedModelObjects = new List<IModelObject>(modifiedModelObjects);
			if (oldValues.Length == 1)
				this.description = string.Format("Change property '{0}' of {1} model object(s) from '{2}' to '{3}'", propertyInfo.Name, this.modifiedModelObjects.Count, oldValues[0], newValue);
			else
				this.description = string.Format("Change property '{0}' of {1} model object(s) to '{2}'", propertyInfo.Name, this.modifiedModelObjects.Count, newValue);
			this.oldValues = new object[oldValues.Length];
			Array.Copy(oldValues, this.oldValues, oldValues.Length);
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public ModelObjectPropertySetCommand(IModelObject modifiedModelObject, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base() {
			if (modifiedModelObject == null) throw new ArgumentNullException("modifiedModelObject");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.description = string.Format("Change property '{0}' of {1} from '{2}' to '{3}'", propertyInfo.Name, modifiedModelObject.Type.Name, oldValue, newValue);
			this.modifiedModelObjects = new List<IModelObject>(1);
			this.modifiedModelObjects.Add(modifiedModelObject);
			this.oldValues = new object[1];
			this.oldValues[0] = oldValue;
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public override void Execute() {
			foreach (IModelObject modelObject in modifiedModelObjects) {
				object currValue = propertyInfo.GetValue(modelObject, null);
				propertyInfo.SetValue(modelObject, newValue, null);
			}
			if (Repository != null) {
				if (modifiedModelObjects.Count == 1) Repository.UpdateModelObject(modifiedModelObjects[0]);
				Repository.UpdateModelObjects(modifiedModelObjects);
			}
		}


		public override void Revert() {
			int oldValuesCnt = oldValues.Length;
			for (int i = 0; i < modifiedModelObjects.Count; ++i) {
				if (oldValuesCnt > 1)
					propertyInfo.SetValue(modifiedModelObjects[i], oldValues[i], null);
				else
					propertyInfo.SetValue(modifiedModelObjects[i], oldValues[0], null);

				if (Repository != null) Repository.UpdateModelObject(modifiedModelObjects[i]);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData; }
		}


		#region Fields
		private PropertyInfo propertyInfo;
		private object[] oldValues;
		private object newValue;
		private List<IModelObject> modifiedModelObjects;
		#endregion
	}
	#endregion


	#region ExchangeTemplateCommand class
	public class ExchangeTemplateCommand : Command {
		public ExchangeTemplateCommand(Template originalTemplate, Template changedTemplate)
			: base() {
			if (originalTemplate == null) throw new ArgumentNullException("originalTemplate");
			if (changedTemplate == null) throw new ArgumentNullException("changedTemplate");
			this.description = string.Format("Change tempate '{0}'", originalTemplate.Name);
			this.originalTemplate = originalTemplate;
			this.oldTemplate = originalTemplate.Clone();
			this.newTemplate = changedTemplate;
		}


		public override void Execute() {
			// ToDo: Handle exchanging shapes of different ShapeTypes
			originalTemplate.CopyFrom(newTemplate);
			if (Repository != null) {
				// ToDo: Optimize deleting/updating/insterting property mappings
				Repository.DeleteModelMappings(oldTemplate.GetPropertyMappings());
				Repository.UpdateTemplate(originalTemplate);
				Repository.InsertModelMappings(originalTemplate.GetPropertyMappings(), originalTemplate);
			}
			InvalidateTemplateShapes();
		}


		public override void Revert() {
			// ToDo: Handle exchanging shapes of different ShapeTypes
			originalTemplate.CopyFrom(oldTemplate);
			if (Repository != null) {
				// ToDo: Optimize deleting/updating/insterting property mappings
				Repository.DeleteModelMappings(newTemplate.GetPropertyMappings());
				Repository.UpdateTemplate(originalTemplate);
				Repository.InsertModelMappings(originalTemplate.GetPropertyMappings(), originalTemplate);
			}
			InvalidateTemplateShapes();
		}


		private void InvalidateTemplateShapes() {
			// Invalidate all changed selectedShapes
			if (Repository != null) {
				foreach (Diagram diagram in Repository.GetDiagrams()) {
					foreach (Shape shape in diagram.Shapes) {
						if (originalTemplate == shape.Template)
							shape.NotifyStyleChanged(null);
					}
				}
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Templates; }
		}


		#region Fields
		private Template originalTemplate;
		private Template oldTemplate;
		private Template newTemplate;

		private List<ShapeConnectionInfo> shapeConnectionInfos = new List<ShapeConnectionInfo>();
		#endregion
	}
	#endregion


	#region ExchangeTemplateShapeCommand class
	public class ExchangeTemplateShapeCommand : Command {
		public ExchangeTemplateShapeCommand(Template originalTemplate, Template changedTemplate)
			: base() {
			if (originalTemplate == null) throw new ArgumentNullException("originalTemplate");
			if (changedTemplate == null) throw new ArgumentNullException("changedTemplate");
			this.description = string.Format("Change shape of tempate  '{0}' from '{1}' to '{2}'", originalTemplate.Name, originalTemplate.Shape.Type.Name, changedTemplate.Shape.Type.Name);
			this.originalTemplate = originalTemplate;
			this.oldTemplate = originalTemplate.Clone();
			this.newTemplate = changedTemplate;

			this.oldTemplateShape = originalTemplate.Shape;
			this.newTemplateShape = changedTemplate.Shape;
			this.newTemplateShape.DisplayService = oldTemplateShape.DisplayService;
		}


		public override void Execute() {
			if (Repository != null) GetAllShapesFromTemplate();

			// Copy all Properties of the new template to the reference of the original Template
			originalTemplate.Shape = null;
			originalTemplate.CopyFrom(newTemplate);
			originalTemplate.Shape = newTemplateShape;
			if (Repository != null) 
				Repository.ReplaceTemplateShape(originalTemplate, oldTemplateShape, newTemplateShape);

			// exchange oldShapes with newShapes
			int cnt = shapesFromTemplate.Count;
			for (int i = 0; i < cnt; ++i)
				ReplaceShapes(shapesFromTemplate[i].diagram,
									shapesFromTemplate[i].oldShape,
									shapesFromTemplate[i].newShape,
									shapesFromTemplate[i].oldConnections,
									shapesFromTemplate[i].newConnections);
		}


		public override void Revert() {
			originalTemplate.Shape = null;
			originalTemplate.CopyFrom(oldTemplate);
			originalTemplate.Shape = oldTemplateShape;
			if (Repository != null) Repository.ReplaceTemplateShape(originalTemplate, newTemplateShape, oldTemplateShape);

			// exchange old shape with new Shape
			int cnt = shapesFromTemplate.Count;
			for (int i = 0; i < cnt; ++i)
				ReplaceShapes(shapesFromTemplate[i].diagram,
									shapesFromTemplate[i].newShape,
									shapesFromTemplate[i].oldShape,
									shapesFromTemplate[i].newConnections,
									shapesFromTemplate[i].oldConnections);
		}


		private void GetAllShapesFromTemplate() {
			// ToDo: In future versions, the cache should handle this by exchanging the loaded shapes at once and the unloaded shapes next time they are loaded
			// For now, find all Shapes in the Cache created from the changed Template and change their shape's type
			if (Repository != null && shapesFromTemplate.Count == 0) {
				foreach (Diagram diagram in Repository.GetDiagrams()) {
					foreach (Shape shape in diagram.Shapes) {
						if (shape.Template == originalTemplate) {
							// copy as much properties as possible from the old shape into the new shape
							ReplaceShapesBuffer buffer = new ReplaceShapesBuffer();
							buffer.diagram = diagram;
							buffer.oldShape = shape;
							buffer.newShape = newTemplate.CreateShape();
							buffer.newShape.CopyFrom(buffer.oldShape);
							buffer.oldConnections = new List<ShapeConnectionInfo>(shape.GetConnectionInfos(ControlPointId.Any, null));
							buffer.newConnections = new List<ShapeConnectionInfo>(buffer.oldConnections.Count);
							foreach (ShapeConnectionInfo sci in buffer.oldConnections) {
								// find a matching connection point...
								ControlPointId ptId = ControlPointId.None;
								if (sci.OwnPointId == ControlPointId.Reference)
									ptId = ControlPointId.Reference;	// Point-To-Shape connections are always possible
								else {
									// try to find a connection point with the same point id
									foreach (ControlPointId id in buffer.newShape.GetControlPointIds(ControlPointCapabilities.Connect)) {
										if (id == sci.OwnPointId) {
											ptId = id;
											break;
										}
									}
								}
								// if the desired point is not a valid ConnectionPoint, create a Point-To-Shape connection
								if (ptId == ControlPointId.None)
									ptId = ControlPointId.Reference;
								// store the new connectionInfo
								buffer.newConnections.Add(new ShapeConnectionInfo(ptId, sci.OtherShape, sci.OtherPointId));
							}

							// Make the new shape about the size of the original one
							Rectangle oldBounds = buffer.oldShape.GetBoundingRectangle(true);
							Rectangle newBounds = buffer.newShape.GetBoundingRectangle(true);
							if (newBounds != oldBounds)
								buffer.newShape.Fit(oldBounds.X, oldBounds.Y, oldBounds.Width, oldBounds.Height);

							shapesFromTemplate.Add(buffer);
						}
					}
				}
			}
		}
		
		
		private void ReplaceShapes(Diagram diagram, Shape oldShape, Shape newShape, IEnumerable<ShapeConnectionInfo> oldConnections, IEnumerable<ShapeConnectionInfo> newConnections) {
			oldShape.Invalidate();
			// disconnect all connections to the old shape
			foreach (ShapeConnectionInfo sci in oldConnections) {
				if (sci.OtherShape.HasControlPointCapability(sci.OtherPointId, ControlPointCapabilities.Glue)) {
					sci.OtherShape.Disconnect(sci.OtherPointId);
					if (Repository != null) Repository.DeleteShapeConnection(sci.OtherShape, sci.OtherPointId, oldShape, sci.OwnPointId);
				} else {
					oldShape.Disconnect(sci.OwnPointId);
					if (Repository != null) Repository.DeleteShapeConnection(oldShape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
				}
			}
			//
			// exchange the shapes
			oldShape.CopyFrom(newShape);
			diagram.Shapes.Replace(oldShape, newShape);
			if (Repository != null) {
				Repository.InsertShape(newShape, diagram);
				Repository.DeleteShape(oldShape);
			}
			//
			// restore all connections to the new shape
			foreach (ShapeConnectionInfo sci in newConnections) {
				if (newShape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue)) {
					newShape.Connect(sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
					if (Repository != null) Repository.InsertShapeConnection(newShape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
					//newShape.FollowConnectionPointWithGluePoint(sci.PassiveShape, sci.ConnectionPointId, sci.GluePointId);
				} else {
					sci.OtherShape.Connect(sci.OtherPointId, newShape, sci.OwnPointId);
					if (Repository != null) Repository.InsertShapeConnection(sci.OtherShape, sci.OtherPointId, newShape, sci.OwnPointId);
					//sci.PassiveShape.FollowConnectionPointWithGluePoint(newShape, sci.GluePointId, sci.ConnectionPointId);
				}
			}
			newShape.Invalidate();
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData | Permission.Present | Permission.Connect | Permission.Templates; }
		}


		#region Fields

		private struct ReplaceShapesBuffer {
			public Diagram diagram;
			public Shape oldShape;
			public Shape newShape;
			public List<ShapeConnectionInfo> oldConnections;
			public List<ShapeConnectionInfo> newConnections;
		}

		private Template originalTemplate;	// reference on the (original) Template which has to be changed
		private Template oldTemplate;			// a clone of the original Template (needed for reverting the command)
		private Template newTemplate;			// a clone of the new Template
		private Shape oldTemplateShape;		// the original template shape
		private Shape newTemplateShape;		// the new template shape
		private List<ReplaceShapesBuffer> shapesFromTemplate = new List<ReplaceShapesBuffer>();
		#endregion
	}
	
	#endregion


	#region CreateTemplateCommand class
	
	public class CreateTemplateCommand : Command {

		public CreateTemplateCommand(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			this.description = string.Format("Create new tempate '{0}' based on '{1}'", template.Name, template.Shape.Type.Name);
			this.template = template;
		}


		public CreateTemplateCommand(string templateName, Shape baseShape)
			: base() {
			if (templateName == null) throw new ArgumentNullException("templateName");
			if (baseShape == null) throw new ArgumentNullException("baseShape");
			this.description = string.Format("Create new tempate '{0}' based on '{1}'", templateName, baseShape.Type.Name);
			this.templateName = templateName;
			this.baseShape = baseShape;
		}


		public override void Execute() {
			if (template == null) {
				Shape templateShape = baseShape.Type.CreateInstance();
				templateShape.CopyFrom(baseShape);
				template = new Template(templateName, templateShape);
			}
			if (Repository != null) {
				Repository.InsertTemplate(template);
				Repository.InsertModelMappings(template.GetPropertyMappings(), template);
			}
		}


		public override void Revert() {
			if (Repository != null) {
				Repository.DeleteModelMappings(template.GetPropertyMappings());
				Repository.DeleteTemplate(template);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData | Permission.Present; }
		}


		#region Fields
		private string templateName;
		private Shape baseShape;
		private Template template;
		#endregion
	}
	
	#endregion


	#region DeleteTemplateCommand class
	public class DeleteTemplateCommand : Command {

		public DeleteTemplateCommand(Template template)
			: base() {
			if (template == null) throw new ArgumentNullException("template");
			this.description = string.Format("Delete tempate '{0}' based on '{1}'", template.Name, template.Shape.Type.Name);
			this.template = template;
		}


		public override void Execute() {
			if (Repository != null) {
				Repository.DeleteModelMappings(template.GetPropertyMappings());
				Repository.DeleteTemplate(template);
			}
		}


		public override void Revert() {
			if (Repository != null) {
				Repository.InsertTemplate(template);
				Repository.InsertModelMappings(template.GetPropertyMappings(), template);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData | Permission.Present; }
		}


		#region Fields
		private Template template;
		#endregion
	}
	#endregion


	#region DiagramPropertySetCommand class
	public class DiagramPropertySetCommand : Command {

		public DiagramPropertySetCommand(IEnumerable<Diagram> modifiedDiagrams, PropertyInfo propertyInfo, object[] oldValues, object newValue)
			: base() {
			if (modifiedDiagrams == null) throw new ArgumentNullException("modifiedDiagrams");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			Debug.Assert(oldValues is object[]);
			this.modifiedDiagrams = new List<Diagram>(modifiedDiagrams);
			if (oldValues.Length == 1)
				this.description = string.Format("Change property '{0}' of {1} diagram(s) from '{2}' to '{3}'", propertyInfo.Name, this.modifiedDiagrams.Count, oldValues[0], newValue);
			else
				this.description = string.Format("Change property '{0}' of {1} diagram(s) to '{2}'", propertyInfo.Name, this.modifiedDiagrams.Count, newValue);
			this.oldValues = new object[oldValues.Length];
			Array.Copy(oldValues, this.oldValues, oldValues.Length);
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public DiagramPropertySetCommand(Diagram modifiedDiagram, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base() {
			if (modifiedDiagram == null) throw new ArgumentNullException("modifiedDiagram");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			Debug.Assert(!(oldValue is object[]));
			this.description = string.Format("Change property '{0}' of diagram '{1}' from '{2}' to '{3}'", propertyInfo.Name, modifiedDiagram.Name, oldValue, newValue);
			this.modifiedDiagrams = new List<Diagram>(1);
			this.modifiedDiagrams.Add(modifiedDiagram);
			this.oldValues = new object[1];
			this.oldValues[0] = oldValue;
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public override void Execute() {
			for (int i = 0; i < modifiedDiagrams.Count; ++i) {
				// this is necessary because if the Value that will be exclusive-or'ed, 
				// the change would be undone
				bool setProperty = true;
				if (newValue != null)
					setProperty = !newValue.Equals(propertyInfo.GetValue(modifiedDiagrams[i], null));

				if (setProperty)
					propertyInfo.SetValue(modifiedDiagrams[i], newValue, null);

				if (Repository != null) Repository.UpdateDiagram(modifiedDiagrams[i]);
			}
		}


		public override void Revert() {
			int oldValuesCnt = oldValues.Length;
			for (int i = 0; i < modifiedDiagrams.Count; ++i) {
				object oldValue = (oldValuesCnt > 1) ? oldValues[i] : oldValues[0];

				bool setProperty = true;
				if (oldValue != null)
					setProperty = !oldValue.Equals(propertyInfo.GetValue(modifiedDiagrams[i], null));
				if (setProperty)
					propertyInfo.SetValue(modifiedDiagrams[i], oldValue, null);

				if (Repository != null) Repository.UpdateDiagram(modifiedDiagrams[i]);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.ModifyData | Permission.Present; }
		}


		#region Fields
		private PropertyInfo propertyInfo;
		private object[] oldValues;
		private object newValue;
		private List<Diagram> modifiedDiagrams;
		#endregion
	}
	#endregion


	#region DesignPropertySetCommand class
	public class DesignPropertySetCommand : Command {
		public DesignPropertySetCommand(Design modifiedObject, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base() {
			if (modifiedObject == null) throw new ArgumentNullException("modifiedObject");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.description = string.Format("Change property '{0}' of {1} '{2}' from '{3}' to '{4}'", propertyInfo.Name, modifiedObject.GetType().Name, modifiedObject.Name, oldValue, newValue);
			this.modifiedObject = modifiedObject;
			this.oldValue = oldValue;
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public override void Execute() {
			bool setProperty = true;
			if (newValue != null)
				setProperty = !newValue.Equals(propertyInfo.GetValue(modifiedObject, null));

			if (setProperty)
				propertyInfo.SetValue(modifiedObject, newValue, null);

			if (Repository != null) Repository.UpdateDesign(modifiedObject);
		}


		public override void Revert() {
			bool setProperty = true;
			if (oldValue != null)
				setProperty = !oldValue.Equals(propertyInfo.GetValue(modifiedObject, null));

			if (setProperty)
				propertyInfo.SetValue(modifiedObject, oldValue, null);

			if (Repository != null) Repository.UpdateDesign(modifiedObject);
		}


		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		#region Fields
		private PropertyInfo propertyInfo;
		private object oldValue;
		private object newValue;
		private Design modifiedObject;
		#endregion
	}
	#endregion


	#region LayerPropertySetCommand class

	public class LayerPropertySetCommand : Command {

		public LayerPropertySetCommand(Diagram diagram, Layer modifiedLayer, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.description = string.Format("Change property '{0}' of {1} '{2}' from '{3}' to '{4}'", propertyInfo.Name, modifiedLayer.GetType().Name, modifiedLayer.Name, oldValue, newValue);
			this.diagram = diagram;
			this.modifiedObject = modifiedLayer;
			this.oldValue = oldValue;
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public override void Execute() {
			bool setProperty = true;
			if (newValue != null)
				setProperty = !newValue.Equals(propertyInfo.GetValue(modifiedObject, null));
			if (setProperty) propertyInfo.SetValue(modifiedObject, newValue, null);
			if (Repository != null) Repository.UpdateDiagram(diagram);
		}


		public override void Revert() {
			bool setProperty = true;
			if (oldValue != null)
				setProperty = !oldValue.Equals(propertyInfo.GetValue(modifiedObject, null));
			if (setProperty) propertyInfo.SetValue(modifiedObject, oldValue, null);
			if (Repository != null) Repository.UpdateDiagram(diagram);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		#region Fields
		private PropertyInfo propertyInfo;
		private object oldValue;
		private object newValue;
		private Layer modifiedObject;
		private Diagram diagram;
		#endregion
	}
	#endregion


	#region StylePropertySetCommand class
	public class StylePropertySetCommand : Command {

		public StylePropertySetCommand(Design design, Style modifiedStyle, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base() {
			if (design == null) throw new ArgumentNullException("design");
			if (modifiedStyle == null) throw new ArgumentNullException("modifiedStyle");
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.description = string.Format("Change property '{0}' of {1} '{2}' from '{3}' to '{4}'", propertyInfo.Name, modifiedStyle.GetType().Name, modifiedStyle.Title, oldValue, newValue);
			this.modifiedStyle = modifiedStyle;
			this.design = design;
			this.oldValue = oldValue;
			this.newValue = newValue;
			this.propertyInfo = propertyInfo;
		}


		public override void Execute() {
			// Check if the property was set before.
			// This is necessary because if the Value is e.g. a FontStyle, which will be 
			// exclusive-or'ed, the change would be undone.
			bool doSetProperty = true;
			if (newValue != null) 
				doSetProperty = !newValue.Equals(propertyInfo.GetValue(modifiedStyle, null));
			// Check if the style was renamed.
			if (string.Compare(propertyInfo.Name, "Name") == 0) 
				MaintainStyleName((string)oldValue, (string)newValue);

			if (doSetProperty) propertyInfo.SetValue(modifiedStyle, newValue, null);
			if (Repository != null) Repository.UpdateStyle(modifiedStyle);
			if (Repository != null) Repository.UpdateDesign(design);
		}


		public override void Revert() {
			// Check if the property value has to be set
			bool doSetProperty = true;
			if (oldValue != null) 
				doSetProperty = !oldValue.Equals(propertyInfo.GetValue(modifiedStyle, null));
			// Check if the style was renamed.
			if (string.Compare(propertyInfo.Name, "Name") == 0) 
				MaintainStyleName((string)newValue, (string)oldValue);
			
			if (doSetProperty) propertyInfo.SetValue(modifiedStyle, oldValue, null);
			if (Repository != null) {
				Repository.UpdateStyle(modifiedStyle);
				Repository.UpdateDesign(design);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		private void MaintainStyleName(string oldName, string newName) {
			Type styleType = modifiedStyle.GetType();
			IStyle style = design.FindStyleByName(oldName, styleType);
			design.RemoveStyle(oldName, styleType);
			design.AddStyle(style);
		}


		#region Fields
		private PropertyInfo propertyInfo;
		private object oldValue;
		private object newValue;
		private Style modifiedStyle;
		private Design design;
		#endregion
	}
	#endregion


	#region InsertShapeCommand class
	/// <summary>
	/// Inserts the given shape(s) into diagram and cache.
	/// </summary>
	public class InsertShapeCommand : InsertOrRemoveShapeCommand {

		public InsertShapeCommand(Diagram diagram, LayerIds layerIds, Shape shape, bool keepZOrder)
			: base(diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shape == null) throw new ArgumentNullException("shape");
			this.description = string.Format(InsertOrRemoveShapeCommand.CreateDescription, shape.Type.Name, string.Empty);
			Construct(layerIds, shape, 0, 0, keepZOrder);
		}


		public InsertShapeCommand(Diagram diagram, LayerIds layerIds, Shape shape, bool keepZOrder, int toX, int toY)
			: base(diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shape == null) throw new ArgumentNullException("shape");
			Construct(layerIds, shape, toX - shape.X, toY - shape.Y, keepZOrder);
			this.description = string.Format(InsertOrRemoveShapeCommand.CreateDescription + " at {2}", shape.Type.Name, string.Empty, new Point(toX, toY));
		}


		public InsertShapeCommand(Diagram diagram, LayerIds layerIds, IEnumerable<Shape> shapes, bool keepZOrder, int deltaX, int deltaY)
			: base(diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			Construct(layerIds, shapes, deltaX, deltaY, keepZOrder);
			this.description = string.Format(InsertOrRemoveShapeCommand.CreateDescription, Shapes.Count, 's');
		}


		public InsertShapeCommand(Diagram diagram, LayerIds layerIds, IEnumerable<Shape> shapes, bool keepZOrder)
			: base(diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			Construct(layerIds, shapes, 0, 0, keepZOrder);
			this.description = string.Format(InsertOrRemoveShapeCommand.CreateDescription, Shapes.Count, 's');
		}


		public override void Execute() {
			InsertShapes(layerIds);
		}


		public override void Revert() {
			RemoveShapes();
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}


		private void Construct(LayerIds layerIds, Shape shape, int offsetX, int offsetY, bool keepZOrder) {
			this.layerIds = layerIds;
			PrepareShape(shape, offsetX, offsetY, keepZOrder);
			SetShapes(shape);
		}


		private void Construct(LayerIds layerIds, IEnumerable<Shape> shapes, int offsetX, int offsetY, bool keepZOrder) {
			this.layerIds = layerIds;
			foreach (Shape shape in shapes)
				PrepareShape(shape, offsetX, offsetY, keepZOrder);
			SetShapes(shapes);
		}


		/// <summary>
		/// Reset shape's ZOrder to 'Unassigned' and offset shape's position if neccessary
		/// </summary>
		private void PrepareShape(Shape shape, int offsetX, int offsetY, bool keepZOrder) {
			if (!keepZOrder) shape.ZOrder = 0;
			if (offsetX != 0 || offsetY != 0)
				shape.MoveBy(offsetX, offsetY);
		}


		#region Fields
		private LayerIds layerIds = LayerIds.None;
		#endregion
	}
	#endregion


	#region InsertShapeAndModelCommand class
	/// <summary>
	/// Inserts the given shapes and their model objects into diagram and cache.
	/// </summary>
	public class InsertShapeAndModelCommand : InsertOrRemoveShapeAndModelCommand {
		public InsertShapeAndModelCommand(Diagram diagram, LayerIds layerIds, Shape shape, bool keepZOrder)
			: base(diagram) {
			Construct(layerIds, shape, 0, 0, keepZOrder);
			this.description = string.Format(CreateDescription, shape.Type.Name, shape.ModelObject.Type.Name + " ", string.Empty);
		}


		public InsertShapeAndModelCommand(Diagram diagram, LayerIds layerIds, Shape shape, bool keepZOrder, int toX, int toY)
			: base(diagram) {			
			Construct(layerIds, shape, toX - shape.X, toY - shape.Y, keepZOrder);
			this.description = string.Format(CreateDescription + " at {3}", shape.Type.Name, shape.ModelObject.Type.Name + " ", string.Empty, new Point(toX, toY));
		}


		public InsertShapeAndModelCommand(Diagram diagram, LayerIds layerIds, IEnumerable<Shape> shapes, bool keepZOrder, int deltaX, int deltaY)
			: base(diagram) {
			Construct(layerIds, shapes, deltaX, deltaY, keepZOrder);
			this.description = string.Format(CreateDescription, Shapes.Count, string.Empty, 's');
		}


		public InsertShapeAndModelCommand(Diagram diagram, LayerIds layerIds, IEnumerable<Shape> shapes, bool keepZOrder)
			: base(diagram) {
			Construct(layerIds, shapes, 0, 0, keepZOrder);
			this.description = string.Format(CreateDescription, Shapes.Count, string.Empty, 's');
		}


		public override void Execute() {
			InsertShapesAndModels(layerIds);
		}


		public override void Revert() {
			RemoveShapesAndModels();
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}


		private void Construct(LayerIds layerIds, Shape shape, int offsetX, int offsetY, bool keepZOrder) {
			if (shape == null) throw new ArgumentNullException("shape");
			this.layerIds = layerIds;
			PrepareShape(shape, offsetX, offsetY, keepZOrder);
			SetShapes(shape);
			SetModelObjects(shape.ModelObject);
		}


		private void Construct(LayerIds layerIds, IEnumerable<Shape> shapes, int offsetX, int offsetY, bool keepZOrder) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.layerIds = layerIds;
			foreach (Shape shape in shapes)
				PrepareShape(shape, offsetX, offsetY, keepZOrder);
			SetShapes(shapes);
			SetModelObjects(shapes);
		}


		/// <summary>
		/// Reset shape's ZOrder to 'Unassigned' and offset shape's position if neccessary
		/// </summary>
		private void PrepareShape(Shape shape, int offsetX, int offsetY, bool keepZOrder) {
			if (!keepZOrder) shape.ZOrder = 0;
			if (offsetX != 0 || offsetY != 0)
				shape.MoveBy(offsetX, offsetY);
		}


		private LayerIds layerIds;
	}
	#endregion


	#region DeleteShapeOnlyCommand class
	/// <summary>
	/// Removes the given shape(s) from diagram and cache.
	/// </summary>
	public class DeleteShapeOnlyCommand : InsertOrRemoveShapeCommand {
		public DeleteShapeOnlyCommand(Diagram diagram, IEnumerable<Shape> deletedShapes)
			: base(diagram) {
			SetShapes(deletedShapes);
			this.description = string.Format(InsertOrRemoveShapeCommand.DeleteDescription, Shapes.Count, 's');
		}


		public DeleteShapeOnlyCommand(Diagram diagram, Shape shape)
			: base(diagram) {
			SetShapes(shape);
			this.description = string.Format(InsertOrRemoveShapeCommand.DeleteDescription, shape.Type.Name, string.Empty);
		}


		public override void Execute() {
			RemoveShapes();
		}


		public override void Revert() {
			InsertShapes();
		}


		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}
	}
	#endregion


	#region DeleteShapeAndModelCommand class
	/// <summary>
	/// RemoveRange the given shapes and their model objects from diagram and cache.
	/// </summary>
	public class DeleteShapeAndModelCommand : InsertOrRemoveShapeAndModelCommand {
		public DeleteShapeAndModelCommand(Diagram diagram, IEnumerable<Shape> shapes)
			: base(diagram) {
			SetShapes(shapes);
			SetModelObjects(shapes);
			this.description = string.Format(DeleteDescription, Shapes.Count, string.Empty, 's');
		}


		public DeleteShapeAndModelCommand(Diagram diagram, Shape shape)
			: base(diagram) {
			SetShapes(shape);
			SetModelObjects(shape.ModelObject);
			this.description = string.Format(DeleteDescription, shape.Type.Name, shape.ModelObject.Type.Name + ' ', string.Empty);
		}


		public override void Execute() {
			RemoveShapesAndModels();
		}


		public override void Revert() {
			InsertShapesAndModels();
		}


		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}
	}
	#endregion


	#region InsertModelObjectsCommand
	public class InsertModelObjectsCommand : InsertOrRemoveModelObjectsCommand {

		public InsertModelObjectsCommand(IEnumerable<IModelObject> modelObjects)
			: base() {
			modelObjectBuffer = new List<IModelObject>(modelObjects);
		}


		public InsertModelObjectsCommand(IModelObject modelObject) {
			modelObjectBuffer = new List<IModelObject>(1);
			modelObjectBuffer[0] = modelObject;
		}


		public override void Execute() {
			if (ModelObjects.Count == 0) SetModelObjects(modelObjectBuffer);
			InsertModelObjects(true);
		}


		public override void Revert() {
			RemoveModelObjects(true);
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}


		// ToDO: Remove this buffer as soon as the ModelObject gets a Children Property...
		private List<IModelObject> modelObjectBuffer;
	}
	#endregion


	#region DeleteModelObjectsCommand
	public class DeleteModelObjectsCommand : InsertOrRemoveModelObjectsCommand {

	   public DeleteModelObjectsCommand(IEnumerable<IModelObject> modelObjects)
	      : base() {
			modelObjectBuffer = new List<IModelObject>(modelObjects);
	   }


		public DeleteModelObjectsCommand(IModelObject modelObject) {
			modelObjectBuffer = new List<IModelObject>(1);
			modelObjectBuffer[0] = modelObject;
		}


		public override void Execute() {
			if (ModelObjects.Count == 0) SetModelObjects(modelObjectBuffer);
			RemoveModelObjects(true);
		}


		public override void Revert() {
			InsertModelObjects(true);
		}


		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}


		// ToDO: Remove this buffer as soon as the ModelObject gets a Children Property...
		private List<IModelObject> modelObjectBuffer;
	}
	#endregion


	#region InsertVertexCommand class

	public class InsertVertexCommand : Command {

		public InsertVertexCommand(Shape shape, int x, int y)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			this.description = string.Format("Add Point to {0} at {1}|{2}", shape, x, y);
			this.shape = shape;
			this.x = x;
			this.y = y;
		}


		public override void Execute() {
			insertedPointId = ((ILinearShape)shape).AddVertex(x, y);
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override void Revert() {
			((ILinearShape)shape).RemoveVertex(insertedPointId);
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		#region Fields
		private Shape shape;
		private int x;
		private int y;
		private ControlPointId insertedPointId = ControlPointId.None;
		#endregion
	}
	
	#endregion


	#region RemoveVertexCommand class

	public class RemoveVertexCommand : Command {

		public RemoveVertexCommand(Shape shape, ControlPointId vertexId)
			: base() {
			if (shape == null) throw new ArgumentNullException("shape");
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			this.shape = shape;
			this.removedPointId = vertexId;
			this.nextPointId = ((ILinearShape)shape).GetNextVertexId(vertexId); ;

			// do not find point position here because if controlPointId is not valid, an exception would be thrown
			this.description = string.Format("Remove point at {0}|{1} from {2} ", p.X, p.Y, shape);
		}


		public override void Execute() {
			// store point position if not done yet
			if (p == Geometry.InvalidPoint) 
				p = shape.GetControlPointPosition(removedPointId);
			shape.Invalidate();
			((ILinearShape)shape).RemoveVertex(removedPointId);
			shape.Invalidate();
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override void Revert() {
			shape.Invalidate();
			((ILinearShape)shape).InsertVertex(nextPointId, p.X, p.Y);
			shape.Invalidate();
			if (Repository != null) Repository.UpdateShape(shape);
		}


		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		#region Fields
		private Shape shape;
		private Point p = Geometry.InvalidPoint;
		private ControlPointId removedPointId = ControlPointId.None;
		private ControlPointId nextPointId = ControlPointId.None;
		#endregion
	}
	#endregion


	#region CreateDesignCommand

	public class CreateDesignCommand : Command {
		public CreateDesignCommand(Design design)
			: base() {
			if (design == null) throw new ArgumentNullException("design");
			description = string.Format("Create {0} '{1}'", design.GetType().Name, design.Name);
			this.design = design;
		}


		public override void Execute() {
			if (Repository != null) Repository.InsertDesign(design);
		}


		public override void Revert() {
			if (Repository != null) Repository.DeleteDesign(design);
		}


		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		Design design;
	}

	#endregion


	#region DeleteDesignCommand

	public class DeleteDesignCommand : Command {
		public DeleteDesignCommand(Design design)
			: base() {
			if (design == null) throw new ArgumentNullException("design");
			description = string.Format("Delete {0} '{1}'", design.GetType().Name, design.Name);
			this.design = design;
		}


		public override void Execute() {
			if (Repository != null) Repository.DeleteDesign(design);
		}


		public override void Revert() {
			if (Repository != null) Repository.InsertDesign(design);
		}


		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		Design design;
	}

	#endregion


	#region CreateStyleCommand

	public class CreateStyleCommand : Command {
		public CreateStyleCommand(Design design, Style style)
			: base() {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			description = string.Format("Create {0} '{1}'", style.GetType().Name, style.Title);
			this.design = design;
			this.style = style;
		}


		public override void Execute() {
			Design d = design ?? Repository.GetDesign(null);
			d.AddStyle(style);
			if (Repository != null) {
				Repository.InsertStyle(d, style);
				Repository.UpdateDesign(d);
			}
		}


		public override void Revert() {
			Design d = design ?? Repository.GetDesign(null);
			d.RemoveStyle(style);
			if (Repository != null) {
				Repository.DeleteStyle(style);
				Repository.UpdateDesign(d);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		Design design;
		Style style;
	}

	#endregion


	#region DeleteStyleCommand

	public class DeleteStyleCommand : Command {
		public DeleteStyleCommand(Design design, Style style)
			: base() {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			description = string.Format("Delete {0} '{1}'", style.GetType().Name, style.Title);			
			this.design = design;
			this.style = style;
		}


		public override void Execute() {
			Design d = design ?? Repository.GetDesign(null);
			d.RemoveStyle(style);
			if (Repository != null) {
				Repository.DeleteStyle(style);
				Repository.UpdateDesign(d);
			}
		}


		public override void Revert() {
			Design d = design ?? Repository.GetDesign(null);
			d.AddStyle(style);
			if (Repository != null) {
				Repository.InsertStyle(d, style);
				Repository.UpdateDesign(d);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		Design design;
		Style style;
	}

	#endregion


	#region AddLayerCommand

	public class AddLayerCommand : InsertOrRemoveLayerCommand {
		
		public AddLayerCommand(Diagram diagram, string layerName)
			: base(diagram, layerName) {
			this.description = string.Format("Add layer '{0}'", layerName);
		}


		public override void Execute() {
			AddLayers();
		}

		public override void Revert() {
			RemoveLayers();
		}
	}

	#endregion


	#region RemoveLayerCommand

	public class RemoveLayerCommand : InsertOrRemoveLayerCommand {
		public RemoveLayerCommand(Diagram diagram, Layer layer)
			: base(diagram, layer) {
			Construct();
			this.description = string.Format("RemoveRange layer '{0}' from diagram '{1}'", layer.Title, diagram.Title);
		}


		public RemoveLayerCommand(Diagram diagram, IEnumerable<Layer> layers)
			: base(diagram, layers) {
			Construct();
			this.description = string.Format("RemoveRange {0} layer from diagram '{1}'", this.layers.Count, diagram.Title);
		}


		public override void Execute() {
			diagram.RemoveShapesFromLayers(affectedShapes, layerIds);
			RemoveLayers();
		}


		public override void Revert() {
			AddLayers();
			RestoreShapeLayers();
		}


		private void Construct() {
			layerIds = LayerIds.None;
			for (int i = 0; i < layers.Count; ++i)
				layerIds |= layers[i].Id;

			affectedShapes = new List<Shape>();
			originalLayers = new List<LayerIds>();
			foreach (Shape shape in diagram.Shapes) {
				if ((shape.Layers & layerIds) != 0) {
					affectedShapes.Add(shape);
					originalLayers.Add(shape.Layers);
				}
			}
		}


		private void RestoreShapeLayers() {
			int cnt = affectedShapes.Count;
			for (int i = 0; i < cnt; ++i)
				diagram.AddShapeToLayers(affectedShapes[i], originalLayers[i]);
			if (Repository != null) Repository.UpdateDiagram(diagram);
		}


		private struct OriginalShapeLayer {
			static OriginalShapeLayer() {
				Empty.layerIds = LayerIds.None;
				Empty.shape = null;
			}
			public static readonly OriginalShapeLayer Empty;
			public Shape shape;
			public LayerIds layerIds;
		}

		private LayerIds layerIds;
		private List<Shape> affectedShapes;
		private List<LayerIds> originalLayers;
	}

	#endregion


	#region GroupShapesCommand class

	/// <summary>
	/// Create clones of the given shapes and insert them into diagram and cache
	/// </summary>
	public class GroupShapesCommand : ShapeAggregationCommand {

		public GroupShapesCommand(Diagram diagram, LayerIds layerIds, Shape shapeGroup, IEnumerable<Shape> childShapes)
			: base(diagram, shapeGroup, childShapes) {
			if (!(aggregationShape is IShapeGroup)) throw new ArgumentException("Shape must be a shape group.");
			this.description = string.Format("Aggregate {0} shapes to a '{1}'", base.shapes.Count, ((Shape)base.aggregationShape).Type.Name);

			// Calculate boundingRectangle of the children and
			// move aggregationShape to children's center
			if (aggregationShape.X == 0 && aggregationShape.Y == 0) {
				Rectangle r = Rectangle.Empty;
				foreach (Shape shape in childShapes) {
					if (r.IsEmpty) r = shape.GetBoundingRectangle(true);
					else r = Geometry.UniteRectangles(r, shape.GetBoundingRectangle(true));
				}
				aggregationShape.MoveTo(r.X + (r.Width / 2), r.Y + (r.Height / 2));
			}
		}


		public override void Execute() {
			// insert aggregationShape into diagram (and Cache)
			if (Repository != null) aggregationShape.ZOrder = Repository.ObtainNewTopZOrder(diagram);
			CreateShapeAggregation(false);
		}


		public override void Revert() {
			DeleteShapeAggregation();
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}
	}
	#endregion


	#region UngroupShapesCommand class

	/// <summary>
	/// Create clones of the given shapes and insert them into diagram and cache
	/// </summary>
	public class UngroupShapesCommand : ShapeAggregationCommand {

		public UngroupShapesCommand(Diagram diagram, Shape shapeGroup)
			: base(diagram, shapeGroup, (shapeGroup != null) ? shapeGroup.Children : null) {
			if (!(shapeGroup is IShapeGroup)) throw new ArgumentException("Shape must support IShapeGroup.");
			this.description = string.Format("Release {0} shapes from {1}'s aggregation", base.shapes.Count, base.aggregationShape.Type.Name);
		}


		public override void Execute() {
			DeleteShapeAggregation();
		}


		public override void Revert() {
			CreateShapeAggregation(false);
		}


		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}
	}
	#endregion


	#region AggregateCompositeShapeCommand class
	public class AggregateCompositeShapeCommand : ShapeAggregationCommand {

		public AggregateCompositeShapeCommand(Diagram diagram, LayerIds layerIds, Shape parentShape, IEnumerable<Shape> childShapes)
			: base(diagram, parentShape, childShapes) {
			this.description = string.Format("Aggregate {0} shapes to a composite shape", base.shapes.Count);
		}


		public override void Execute() {
			CreateShapeAggregation(true);
		}


		public override void Revert() {
			DeleteShapeAggregation();
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}
	}
	#endregion


	#region SplitCompositeShapeCommand class
	public class SplitCompositeShapeCommand : ShapeAggregationCommand {

		public SplitCompositeShapeCommand(Diagram diagram, LayerIds layerIds, Shape parentShape)
			: base(diagram, parentShape, (parentShape != null) ? parentShape.Children : null) {
			this.description = string.Format("Split composite shape into {0} single shapes", base.shapes.Count);
		}


		public override void Execute() {
			DeleteShapeAggregation();
		}


		public override void Revert() {
			CreateShapeAggregation(true);
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}
	}
	#endregion


	#region InsertDiagramCommand 
	public class InsertDiagramCommand : Command {

		public InsertDiagramCommand(Diagram diagram)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
			description = string.Format("Create diagram '{0}'.", diagram.Title);
		}


		public override void Execute() {
			if (Repository != null) Repository.InsertDiagram(diagram);
		}


		public override void Revert() {
			if (Repository != null) Repository.DeleteDiagram(diagram);
		}


		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}


		private Diagram diagram = null;
	}
	#endregion


	#region DeleteDiagramCommand
	public class DeleteDiagramCommand : Command {

		public DeleteDiagramCommand(Diagram diagram)
			: base() {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.diagram = diagram;
			this.shapes = new ShapeCollection(diagram.Shapes);
		}


		public override void Execute() {
			if (Repository != null) Repository.DeleteDiagram(diagram);
		}


		public override void Revert() {
			if (Repository != null) {
				if (diagram.Shapes.Count == 0)
					diagram.Shapes.AddRange(shapes);
				Repository.InsertDiagram(diagram);
			}
		}


		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}


		private Diagram diagram = null;
		private ShapeCollection shapes;
	}
	#endregion
}