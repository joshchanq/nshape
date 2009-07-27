using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

using Dataweb.Diagramming.Advanced;
using Dataweb.Diagramming.Controllers;


namespace Dataweb.Diagramming.WinFormsUI {

	public partial class DesignEditorDialog : Form {


		public DesignEditorDialog() {
			SetStyle(ControlStyles.ResizeRedraw
				| ControlStyles.AllPaintingInWmPaint
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.SupportsTransparentBackColor
				, true);
			UpdateStyles();

			// Initialize Components
			InitializeComponent();
			RegisterDesignControllerEventHandlers();
			RegisterDesignPresenterEventHandlers();
		}


		public DesignEditorDialog(Project project)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			this.Project = project;
		}


		public Project Project {
			get { return designController.Project; }
			set { designController.Project = value; }
		}
		
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				// Unregister all registered event handlers
				UnregisterDesignPresenterEventHandlers();
				UnregisterDesignControllerEventHandlers();
				if (Project != null) Project = null;

				if (components != null) components.Dispose();
			}
			base.Dispose(disposing);
		}


		private void InitializeDesignList() {
			designsComboBox.Items.Clear();
			foreach (Design design in Project.Repository.GetDesigns())
				if (design != null) designsComboBox.Items.Add(design.Name);
			
		}


		private void ClearDesignList() {
			designsComboBox.Items.Clear();
		}


		private void SetButtonStates() {
			string projectDesignName = Project.Design.Name;
			designsComboBox.Enabled = false;
			for (int i = 0; i < designsComboBox.Items.Count; ++i) {
				if (!projectDesignName.Equals(designsComboBox.Items[i].ToString(), StringComparison.InvariantCultureIgnoreCase)) {
					designsComboBox.Enabled = true;
					break;
				}
			}
			activateButton.Enabled = designsComboBox.Text != projectDesignName;
			deleteDesignButton.Enabled = designController.CanDelete(designPresenter.SelectedDesign);
			deleteStyleButton.Enabled = designController.CanDelete(designPresenter.SelectedDesign, designPresenter.SelectedStyle);
		}


		#region [Private] Methods: (Un)Registering event handlers

		private void RegisterDesignControllerEventHandlers() {
			designController.Initialized += designController_Initialized;
			designController.Uninitialized += designController_Uninitialized;
			designController.DesignCreated += designController_DesignCreated;
			designController.DesignChanged += designController_DesignChanged;
			designController.DesignDeleted += designController_DesignDeleted;
			designController.StyleCreated += designController_StyleCreated;
			designController.StyleChanged += designController_StyleChanged;
			designController.StyleDeleted += designController_StyleDeleted;
		}


		private void UnregisterDesignControllerEventHandlers() {
			designController.Initialized -= designController_Initialized;
			designController.Uninitialized -= designController_Uninitialized;
			designController.DesignCreated -= designController_DesignCreated;
			designController.DesignChanged -= designController_DesignChanged;
			designController.DesignDeleted -= designController_DesignDeleted;
			designController.StyleCreated -= designController_StyleCreated;
			designController.StyleChanged -= designController_StyleChanged;
			designController.StyleDeleted -= designController_StyleDeleted;
		}


		private void RegisterDesignPresenterEventHandlers() {
			designPresenter.DesignSelected += designPresenter_DesignSelected;
			designPresenter.StyleSelected += designPresenter_StyleSelected;
		}


		private void UnregisterDesignPresenterEventHandlers() {
			designPresenter.DesignSelected -= designPresenter_DesignSelected;
			designPresenter.StyleSelected -= designPresenter_StyleSelected;
		}

		#endregion


		#region [Private] Methods: DesignController event handler implementations

		private void designController_Initialized(object sender, EventArgs e) {
			InitializeDesignList();
			designPresenter.SelectedDesign = Project.Design;
		}


		private void designController_Uninitialized(object sender, EventArgs e) {
			ClearDesignList();
		}


		private void designController_DesignCreated(object sender, DesignEventArgs e) {
			if (designsComboBox.Items != null)
				designsComboBox.SelectedIndex = designsComboBox.Items.Add(e.Design.Name);
		}


		private void designController_DesignChanged(object sender, DesignEventArgs e) {
			// nothing to do
		}


		private void designController_DesignDeleted(object sender, DesignEventArgs e) {
			if (designsComboBox.Items.Contains(e.Design.Name))
				designsComboBox.Items.Remove(e.Design.Name);
			designsComboBox.SelectedIndex = designsComboBox.Items.IndexOf(Project.Design.Name);
		}


		private void designController_StyleCreated(object sender, StyleEventArgs e) {
			// nothing to do
		}


		private void designController_StyleChanged(object sender, StyleEventArgs e) {
			// nothing to do
		}


		private void designController_StyleDeleted(object sender, StyleEventArgs e) {
			// nothing to do
		}

		#endregion


		#region [Private] Methods: DesignPresenter event handler implementations

		private void designPresenter_StyleSelected(object sender, EventArgs e) {
			SetButtonStates();
		}


		private void designPresenter_DesignSelected(object sender, EventArgs e) {
			DiagrammingStyleEditor.Design = designPresenter.SelectedDesign;
			SetButtonStates();
		}

		#endregion


		#region [Private] Methods: Event handler implementations

		private void designsComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			foreach (Design design in Project.Repository.GetDesigns()) {
				if (design.Name == designsComboBox.Text) {
					designPresenter.SelectedDesign = design;
					break;
				}
			}
			SetButtonStates();
		}


		private void activateButton_Click(object sender, EventArgs e) {
			designPresenter.ActivateDesign(designPresenter.SelectedDesign);
		}
		
		
		private void newDesignButton_Click(object sender, EventArgs e) {
			designController.CreateDesign();
		}


		private void deleteDesignButton_Click(object sender, EventArgs e) {
			if (MessageBox.Show("All styles in this design will be lost.\r\nDo you really want do to delete this design?", "Delete Design", MessageBoxButtons.YesNo) == DialogResult.Yes)
				designPresenter.DeleteSelectedDesign();
		}


		private void newStyleButton_Click(object sender, EventArgs e) {
			designPresenter.CreateStyle();
		}


		private void deleteStyleButton_Click(object sender, EventArgs e) {
			if (MessageBox.Show("Do you really want do to delete this style?", "Delete Style", MessageBoxButtons.YesNo) == DialogResult.Yes)
				designPresenter.DeleteSelectedStyle();
		}


		private void closeButton_Click(object sender, EventArgs e) {
			if (Modal) DialogResult = DialogResult.OK;
			else Close();
		}

		
		private void DesignEditorDialog_FormClosed(object sender, FormClosedEventArgs e) {
			Project = null;
		}

		#endregion

	}
}