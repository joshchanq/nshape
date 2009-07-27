using System.Windows.Forms;

using Dataweb.Diagramming.Controllers;


namespace Dataweb.Diagramming.WinFormsUI {

	public partial class LayerEditor : UserControl {

		public LayerEditor() {
			InitializeComponent();
		}


		public IDiagramPresenter DiagramPresenter {
			get { return presenter.DiagramPresenter; }
			set { presenter.DiagramPresenter = value; }
		}


		public DiagramSetController DiagramSetController {
			get { return controller.DiagramSetController; }
			set { controller.DiagramSetController = value; }
		}


		#region Fields
		#endregion
	}
}
