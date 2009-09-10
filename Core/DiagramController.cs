using System;
using System.Collections.Generic;

using Dataweb.nShape.Advanced;


namespace Dataweb.nShape.Controllers {

	public class DiagramController {

		public DiagramController(DiagramSetController owner, Diagram diagram) {
			if (owner == null) throw new ArgumentNullException("owner");
			this.owner = owner;
			this.diagram = diagram;
		}


		~DiagramController() {
		}


		#region [Public] Events

		public event EventHandler DiagramChanging;

		public event EventHandler DiagramChanged;

		#endregion


		#region [Public] Properties

		public DiagramSetController Owner {
			get { return owner; }
		}


		public Project Project {
			get { return (owner == null) ? null : owner.Project; }
		}


		public Diagram Diagram { 
			get { return diagram; }
			set {
				if (DiagramChanging != null) DiagramChanging(this, eventArgs);
				diagram = value;
				if (DiagramChanged != null) DiagramChanged(this, eventArgs);
			}
		}


		public Tool Tool {
			get { return owner.ActiveTool; }
			set { owner.ActiveTool = value; }
		}

		#endregion


		#region [Public] Methods

		public void CreateDiagram(string name) {
			diagram = new Diagram(name);
			owner.Project.Repository.InsertDiagram(diagram);
			Diagram = diagram;
		}


		public void OpenDiagram(string name) {
			Diagram = owner.Project.Repository.GetDiagram(name);
		}


		public IEnumerable<nShapeAction> GetActions(IShapeCollection selectedShapes) {
			// ToDo: No actions at the moment
			yield break;
		}

		#endregion


		#region Fields

		private Diagram diagram = null;
		private DiagramSetController owner = null;

		// Buffers
		private EventArgs eventArgs = new EventArgs();

		#endregion
	}

}