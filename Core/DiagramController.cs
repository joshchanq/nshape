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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

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