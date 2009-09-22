/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the nShape framework.
  nShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  nShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  nShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

	#region EventArgs

	public class LayerEventArgs : EventArgs {

		public LayerEventArgs(Layer layer) {
			this.layer = layer;
		}

		public Layer Layer {
			get { return layer; }
			internal set { layer = value; }
		}

		internal LayerEventArgs() { }

		private Layer layer = null;
	}


	public class LayersEventArgs : EventArgs {

		public LayersEventArgs(IEnumerable<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			this.layers = new ReadOnlyList<Layer>(layers);
		}

		
		public IReadOnlyCollection<Layer> Layers { get { return layers; } }

		
		protected internal LayersEventArgs() {
			layers = new ReadOnlyList<Layer>();
		}


		protected internal void SetLayers(ReadOnlyList<Layer> layers) {
			this.layers.Clear();
			this.layers.AddRange(layers);
		}


		protected internal void SetLayers(IEnumerable<Layer> layers) {
			this.layers.Clear();
			this.layers.AddRange(layers);
		}


		protected internal void SetLayers(Layer layer) {
			this.layers.Clear();
			this.layers.Add(layer);
		}


		private ReadOnlyList<Layer> layers = null;
	}


	public class LayerRenamedEventArgs : LayerEventArgs {

		public LayerRenamedEventArgs(Layer layer, string oldName, string newName)
			: base(layer) {

			this.oldName = oldName;
			this.newName = newName;
		}


		public string OldName {
			get { return oldName; }
			internal set { oldName = value; }
		}


		public string NewName {
			get { return newName; }
			internal set { newName = value; }
		}


		protected internal LayerRenamedEventArgs() {
		}


		private string oldName;
		private string newName;
	}


	public class LayerZoomThresholdChangedEventArgs : LayerEventArgs {

		public LayerZoomThresholdChangedEventArgs(Layer layer, int oldZoomThreshold, int newZoomThreshold)
			: base(layer) {
			this.oldZoomThreshold = oldZoomThreshold;
			this.newZoomThreshold = newZoomThreshold;
		}


		public int OldZoomThreshold {
			get { return oldZoomThreshold; }
			internal set { oldZoomThreshold = value; }
		}


		public int NewZoomThreshold {
			get { return newZoomThreshold; }
			internal set { newZoomThreshold = value; }
		}


		protected internal LayerZoomThresholdChangedEventArgs() {
		}


		private int oldZoomThreshold;
		private int newZoomThreshold;
	}

	#endregion


	public class LayerController : Component {

		public LayerController() { }


		public LayerController(DiagramSetController diagramSetController)
			: this() {
			if (diagramSetController == null) throw new ArgumentNullException("diagramSetController");
			this.DiagramSetController = diagramSetController;
		}


		#region [Public] Events

		public event EventHandler DiagramChanging;

		public event EventHandler DiagramChanged;

		public event EventHandler<LayersEventArgs> LayersAdded;

		public event EventHandler<LayersEventArgs> LayersRemoved;

		public event EventHandler<LayersEventArgs> LayerModified;

		#endregion


		#region [Public] Properties

		public DiagramSetController DiagramSetController {
			get { return diagramSetController; }
			set {
				if (diagramSetController != null) UnregisterDiagramSetControllerEvents();
				diagramSetController = value;
				if (diagramSetController != null) RegisterDiagramSetControllerEvents();
			}
		}


		/// <summary>
		/// Returns the Owner's Project
		/// </summary>
		public Project Project {
			get {
				if (diagramSetController == null) return null;
				else return diagramSetController.Project;
			}
		}

		#endregion


		#region [Public] Methods

		public void AddLayer(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertDiagramSetControllerIsSet();
			string newLayerName = GetNewLayerName(diagram);
			AddLayer(diagram, newLayerName);
		}


		public void AddLayer(Diagram diagram, string layerName) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layerName == null) throw new ArgumentNullException("layerName");
			AssertDiagramSetControllerIsSet();
			if (diagram.Layers.FindLayer(layerName) != null) 
				throw new nShapeException("Layer name '{0}' already exists.", layerName);
			Command cmd = new AddLayerCommand(diagram, layerName);
			Project.ExecuteCommand(cmd);
			if (LayersAdded != null) LayersAdded(this, LayerHelper.GetLayersEventArgs(LayerHelper.GetLayers(layerName, diagram)));
		}


		public void RemoveLayers(Diagram diagram, IEnumerable<Layer> layers) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layers == null) throw new ArgumentNullException("layers");
			AssertDiagramSetControllerIsSet();
			Command cmd = new RemoveLayerCommand(diagram, layers);
			Project.ExecuteCommand(cmd);
			if (LayersRemoved != null) LayersRemoved(this, LayerHelper.GetLayersEventArgs(layers));
		}


		public void RemoveLayer(Diagram diagram, string layerName) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layerName == null) throw new ArgumentNullException("layerName");
			AssertDiagramSetControllerIsSet();
			Layer layer = diagram.Layers.FindLayer(layerName);
			if (layer == null) throw new nShapeException("Layer '{0}' does not exist.", layerName);
			Command cmd = new RemoveLayerCommand(diagram, layer);
			Project.ExecuteCommand(cmd);
			if (LayersRemoved != null) LayersRemoved(this, LayerHelper.GetLayersEventArgs(LayerHelper.GetLayers(layerName, diagram)));
		}


		public void RenameLayer(Diagram diagram, Layer layer, string oldName, string newName) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layer == null) throw new ArgumentNullException("layer");
			if (oldName == null) throw new ArgumentNullException("oldName");
			if (newName == null) throw new ArgumentNullException("newName");
			AssertDiagramSetControllerIsSet();
			PropertyInfo pi = typeof(Layer).GetProperty("Name");
			ICommand cmd = new LayerPropertySetCommand(diagram, layer, pi, oldName, newName);
			Project.ExecuteCommand(cmd);
			if (LayerModified != null) LayerModified(this, LayerHelper.GetLayersEventArgs(LayerHelper.GetLayers(layer.Id, diagram)));
		}


		public void SetLayerZoomBounds(Diagram diagram, Layer layer, int lowerZoomBounds, int upperZoomBounds) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layer == null) throw new ArgumentNullException("layer");
			AssertDiagramSetControllerIsSet();
			ICommand cmdMinZoom = null;
			ICommand cmdMaxZoom = null;
			if (layer.LowerZoomThreshold != lowerZoomBounds) {
				PropertyInfo piMinZoom = typeof(Layer).GetProperty("LowerZoomThreshold");
				cmdMinZoom = new LayerPropertySetCommand(diagram, layer, piMinZoom, layer.LowerZoomThreshold, lowerZoomBounds);
			}
			if (layer.UpperZoomThreshold != upperZoomBounds) {
				PropertyInfo piMaxZoom = typeof(Layer).GetProperty("UpperZoomThreshold");
				cmdMaxZoom = new LayerPropertySetCommand(diagram, layer, piMaxZoom, layer.UpperZoomThreshold, upperZoomBounds);
			}
			
			ICommand cmd;
			if (cmdMinZoom != null && cmdMaxZoom != null) {
				cmd = new AggregatedCommand();
				((AggregatedCommand)cmd).Add(cmdMinZoom);
				((AggregatedCommand)cmd).Add(cmdMaxZoom);
			} else if (cmdMinZoom != null && cmdMaxZoom == null)
				cmd = cmdMinZoom;
			else if (cmdMaxZoom != null && cmdMinZoom == null)
				cmd = cmdMaxZoom;
			else cmd = null;
			
			if (cmd != null) {
				Project.ExecuteCommand(cmd);
				if (LayerModified != null) LayerModified(this, LayerHelper.GetLayersEventArgs(LayerHelper.GetLayers(layer.Id, diagram)));
			}
		}


		public IEnumerable<nShapeAction> GetActions(Diagram diagram, IReadOnlyCollection<Layer> selectedLayers) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (selectedLayers == null) throw new ArgumentNullException("selectedLayers");
			bool isFeasible;
			string description;
			
			isFeasible = diagram.Layers.Count < Enum.GetValues(typeof(LayerIds)).Length;
			description = isFeasible ? "Add a new layer to the diagram" : "Maximum number of layers reached";
			yield return new DelegateAction("Add Layer", 
				Properties.Resources.NewLayer, description, isFeasible, Permission.ModifyData,
				(a, p) => AddLayer(diagram));

			isFeasible = selectedLayers.Count > 0;
			description = isFeasible ? string.Format("Delete {0} Layers", selectedLayers.Count) : "No layers selected";
			yield return new DelegateAction(string.Format("Delete Layer{0}", selectedLayers.Count > 1 ? "s" : string.Empty),
				Properties.Resources.DeleteBtn, description, isFeasible, Permission.ModifyData, 
				(a, p) => this.RemoveLayers(diagram, selectedLayers));
		}

		#endregion


		#region [Private] Methods

		private void RegisterDiagramSetControllerEvents() {
			diagramSetController.ProjectChanged += diagramSetController_ProjectChanged;
			diagramSetController.ProjectChanging += diagramSetController_ProjectChanging;
			if (diagramSetController.Project != null) RegisterProjectEvents();
		}


		private void UnregisterDiagramSetControllerEvents() {
			if (diagramSetController.Project != null) UnregisterProjectEvents();
			diagramSetController.ProjectChanging -= diagramSetController_ProjectChanging;
			diagramSetController.ProjectChanged -= diagramSetController_ProjectChanged;
		}


		private void RegisterProjectEvents() {
			AssertProjectIsSet();
			Project.Opened += project_ProjectOpen;
			Project.Closing += project_ProjectClosing;
			Project.Closed += project_ProjectClosed;
			if (Project.IsOpen) project_ProjectOpen(this, null);
		}

		
		private void UnregisterProjectEvents() {
			AssertProjectIsSet();
			if (Project.Repository != null)
				UnregisterRepositoryEvents();
			Project.Opened -= project_ProjectOpen;
			Project.Closing -= project_ProjectClosing;
			Project.Closed -= project_ProjectClosed;
		}


		private void RegisterRepositoryEvents() {
			AssertRepositoryIsSet();
			if (!repositoryEventsRegistered) {
				repositoryEventsRegistered = true;
			}
		}


		private void UnregisterRepositoryEvents() {
			AssertRepositoryIsSet();
			if (repositoryEventsRegistered) {
				repositoryEventsRegistered = false;
			}
		}
		

		private void AssertProjectIsSet() {
			if (Project == null) throw new nShapeException("{0}'s property 'Project' is not set.", typeof(DiagramSetController).FullName);
		}


		private void AssertRepositoryIsSet() {
			AssertProjectIsSet();
			if (Project.Repository == null) throw new nShapeException("Project's 'Repository' property is not set.");
		}

		
		private void AssertDiagramSetControllerIsSet() {
			if (diagramSetController == null) throw new nShapeException("Property 'DiagramController' is not set.");
		}


		private string GetNewLayerName(Diagram diagram) {
			string result = string.Empty;
			// get all used Layerids
			LayerIds usedLayerIds = LayerIds.None;
			foreach (Layer l in diagram.Layers)
				usedLayerIds |= l.Id;
			// find the first Id available
			foreach (LayerIds value in Enum.GetValues(typeof(LayerIds))) {
				if (value == LayerIds.None) continue;
				if ((usedLayerIds & value) == 0) {
					int bitNo = (int)Math.Log((int)value, 2);
					result = string.Format("Layer {0:D2}", bitNo + 1);
					break;
				}
			}
			return result;
		}

		#endregion


		#region [Private] Methods: EventHandler implementations

		private void diagramSetController_ProjectChanging(object sender, EventArgs e) {
			if (diagramSetController.Project != null) UnregisterProjectEvents();
		}


		private void diagramSetController_ProjectChanged(object sender, EventArgs e) {
			if (diagramSetController.Project != null) RegisterProjectEvents();
		}


		private void project_ProjectOpen(object sender, EventArgs e) {
			AssertRepositoryIsSet();
			RegisterRepositoryEvents();
		}


		private void project_ProjectClosing(object sender, EventArgs e) {
			// nothing to do...
		}


		private void project_ProjectClosed(object sender, EventArgs e) {
			AssertRepositoryIsSet();
			UnregisterRepositoryEvents();
		}


		private void diagrammController_ActiveLayersChanged(object sender, LayersEventArgs e) {
			if (LayerModified != null) LayerModified(this, e);
		}


		private void diagramController_LayerVisibilityChanged(object sender, LayersEventArgs e) {
			if (LayerModified != null) LayerModified(this, e);
		}


		private void diagramController_DiagramChanging(object sender, EventArgs e) {
			if (DiagramChanging != null) DiagramChanging(this, e);
		}


		private void diagramController_DiagramChanged(object sender, EventArgs e) {
			if (DiagramChanged != null) DiagramChanged(this, e);
		}

		#endregion
	

		#region Fields
		public const int MaxLayerCount = 31;

		private DiagramSetController diagramSetController = null;
		private bool repositoryEventsRegistered = false;

		private LayersEventArgs layersEventArgs = new LayersEventArgs();
		private LayerEventArgs layerEventArgs = new LayerEventArgs();
		private LayerRenamedEventArgs layerRenamedEventArgs = new LayerRenamedEventArgs();
		private EventArgs eventArgs = new EventArgs();
		#endregion
	}


	public class LayerHelper {
		
		public static IEnumerable<Layer> GetLayers(LayerIds layerId, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			return diagram.Layers.GetLayers(layerId);
		}


		public static IEnumerable<Layer> Getlayers(Layer layer) {
			yield return layer;
		}


		public static IEnumerable<Layer> GetLayers(string layerName, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			yield return diagram.Layers.FindLayer(layerName);
		}


		public static LayerEventArgs GetLayerEventArgs(string layerName, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			Layer layer = diagram.Layers.FindLayer(layerName);
			Debug.Assert(layer != null);
			layerEventArgs.Layer = layer;
			return layerEventArgs;
		}


		public static LayerEventArgs GetLayerEventArgs(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			layerEventArgs.Layer = layer;
			return layerEventArgs;
		}


		public static LayersEventArgs GetLayersEventArgs(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			layersEventArgs.SetLayers(layer);
			return layersEventArgs;
		}


		public static LayersEventArgs GetLayersEventArgs(ReadOnlyList<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			layersEventArgs.SetLayers(layers);
			return layersEventArgs;
		}


		public static LayersEventArgs GetLayersEventArgs(IEnumerable<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			layersEventArgs.SetLayers(layers);
			return layersEventArgs;
		}


		private static LayerEventArgs layerEventArgs = new LayerEventArgs();
		private static LayersEventArgs layersEventArgs = new LayersEventArgs();
	}

}
