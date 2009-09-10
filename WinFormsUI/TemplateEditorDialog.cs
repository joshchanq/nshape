using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

using Dataweb.nShape.Advanced;
using Dataweb.nShape.Controllers;


namespace Dataweb.nShape.WinFormsUI {

	public partial class TemplateEditorDialog : Form {

		public TemplateEditorDialog() {
			DoubleBuffered = true;
			InitializeComponent();
		}


		public TemplateEditorDialog(Project project, Template template)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			templateController.Initialize(project, template);
		}


		public TemplateEditorDialog(Project project)
			: this(project, null) { }


		public Project Project {
			get { return templateController.Project; }
			set { templateController.Project = value; }
		}


		public Template Template {
			get { return template; }
			set { 
				template = value;
				if (template != null && Project != null)
					templateController.Initialize(Project, template);
			}
		}
		
		
		public TemplateController Controller {
			get { return templateController; }
		}


		private void EnableButtons() {
			if (templateController.Project != null) {
				okButton.Enabled =
				applyButton.Enabled = templatePresenter.TemplateWasModified && Project.SecurityManager.IsGranted(Permission.Templates);
			} else {
				okButton.Enabled =
				applyButton.Enabled = false;
			}
			cancelButton.Enabled = true;
		}


		#region [Private] Methods: TemplateController event handler imlpementations

		private void templateController_TemplateDescriptionChanged(object sender, TemplateControllerStringChangedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateModelObjectReplaced(object sender, TemplateControllerModelObjectReplacedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateNameChanged(object sender, TemplateControllerStringChangedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplatePropertyChanged(object sender, EventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateShapeControlPointMappingChanged(object sender, TemplateControllerPointMappingChangedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateShapePropertyMappingDeleted(object sender, TemplateControllerPropertyMappingChangedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateShapePropertyMappingSet(object sender, TemplateControllerPropertyMappingChangedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateShapeReplaced(object sender, TemplateControllerTemplateShapeReplacedEventArgs e) {
			EnableButtons();
		}


		private void templateController_TemplateTitleChanged(object sender, TemplateControllerStringChangedEventArgs e) {
			EnableButtons();
		}

		#endregion


		#region [Private] Methods: Form and control event handler implementations

		private void TemplateEditorDialog_Shown(object sender, EventArgs e) {
			EnableButtons();
		}


		private void TemplateEditorDialog_FormClosed(object sender, FormClosedEventArgs e) {
			templateController.Clear();
			templateController.Project = null;
			template = null;
		}


		private void applyButton_Click(object sender, EventArgs e) {
			templatePresenter.ApplyChanges();
			EnableButtons();
		}


		private void okButton_Click(object sender, EventArgs e) {
			if (templatePresenter.TemplateWasModified)
				templatePresenter.ApplyChanges();
			if (Modal) DialogResult = DialogResult.OK;
			else Close();
		}


		private void cancelButton_Click(object sender, EventArgs e) {
			templatePresenter.DiscardChanges();
			if (Modal) DialogResult = DialogResult.Cancel;
			else Close();
		}

		#endregion


		private Template template = null;
	}
}