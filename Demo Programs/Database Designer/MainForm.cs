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
using System.Drawing;
using System.Windows.Forms;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.SoftwareArchitectureShapes;
using Dataweb.Utilities;
using Dataweb.NShape.WinFormsUI;
using System.IO;


namespace Database_Designer {

	internal enum DockType { 
		None = 0,
		Left = 1, 
		Top = 2, 
		Right = 4, 
		Bottom = 8
	}


	public partial class DatabaseDesignerForm : Form {
		ToolBoxWindow toolBoxWindow;
		PropertyWindow propertyWindow;
		Rectangle contentBounds = Rectangle.Empty;
		const int margin = 10;
		const int zoomStep = 25;
		const string fileFilter = "NShape XML Repository Files|*.xml|All Files|*.*";
		const string defaultProjectName = "Project 1";


		private DockType toolBoxWindowDockType = 0;
		private DockType propertyWindowDockType = 0;


		public DatabaseDesignerForm() {
			InitializeComponent();
			toolBoxWindow = new ToolBoxWindow();
			toolBoxWindow.Owner = this;
			toolBoxWindowDockType = DockType.Top ^ DockType.Right ^ DockType.Bottom;

			propertyWindow = new PropertyWindow();
			propertyWindow.Owner = this;
			propertyWindowDockType = DockType.Top ^ DockType.Right ^ DockType.Bottom;
		}


		private Rectangle ContentBounds {
			get {
				contentBounds.X = Bounds.X + SystemInformation.Border3DSize.Width;
				contentBounds.Y = Bounds.Y 
					+ SystemInformation.CaptionHeight 
					+ SystemInformation.Border3DSize.Height 
					+ SystemInformation.Border3DSize.Height 
					+ SystemInformation.FixedFrameBorderSize.Height
					+ SystemInformation.FixedFrameBorderSize.Height;
				if (toolStripContainer.TopToolStripPanelVisible)
				    contentBounds.Y += toolStripContainer.TopToolStripPanel.Height;
				contentBounds.Width = display.DrawBounds.Width;
				contentBounds.Height = display.DrawBounds.Height;
				return contentBounds; 
			}
		}


		protected override void OnMove(EventArgs e) {
			base.OnMove(e);
			if (toolBoxWindow.Visible)
				DockToolWindow();
			if (propertyWindow.Visible)
				DockPropertyWindow();
		}


		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			if (toolBoxWindow != null && toolBoxWindow.Visible)
				DockToolWindow();
			if (toolBoxWindow != null && propertyWindow.Visible)
				DockPropertyWindow();
		}


		private void DockToolWindow() {
			Rectangle contentBounds = Rectangle.Empty;
			contentBounds = ContentBounds;

			try {
				toolBoxWindow.SuspendLayout();
				int toolBoxBottom;
				if (propertyWindow.Visible)
					toolBoxBottom = contentBounds.Top + (contentBounds.Height / 2);
				else
					toolBoxBottom = contentBounds.Bottom;

				if ((toolBoxWindowDockType & DockType.Left) > 0)
					toolBoxWindow.Left = contentBounds.Left;

				if ((toolBoxWindowDockType & DockType.Top) > 0)
					toolBoxWindow.Top = contentBounds.Top + margin;

				if ((toolBoxWindowDockType & DockType.Right) > 0)
					toolBoxWindow.Left = contentBounds.Right - toolBoxWindow.Width - margin;

				if ((toolBoxWindowDockType & DockType.Bottom) > 0)
					toolBoxWindow.Height = toolBoxBottom - toolBoxWindow.Top - margin;
			} finally {
				toolBoxWindow.ResumeLayout();
			}
		}


		private void DockPropertyWindow() {
			Rectangle contentBounds = Rectangle.Empty;
			contentBounds = ContentBounds;

			try {
				propertyWindow.SuspendLayout();
				int propertyWindowTop;
				if (toolBoxWindow.Visible)
					propertyWindowTop = contentBounds.Bottom - (contentBounds.Height / 2);
				else
					propertyWindowTop = contentBounds.Top;

				if ((propertyWindowDockType & DockType.Left) > 0)
					propertyWindow.Left = contentBounds.Left;
				if ((propertyWindowDockType & DockType.Top) > 0)
					propertyWindow.Top = propertyWindowTop + 10;
				if ((propertyWindowDockType & DockType.Right) > 0)
					propertyWindow.Left = contentBounds.Right - propertyWindow.Width - margin;
				if ((propertyWindowDockType & DockType.Bottom) > 0)
					propertyWindow.Height = contentBounds.Bottom - propertyWindow.Top - margin;
			} finally {
				propertyWindow.ResumeLayout();
			}
		}

		
		private void MainForm_Load(object sender, EventArgs e) {
			// connect TurboDiagram components
			propertyPresenter.PrimaryPropertyGrid = propertyWindow.PropertyGrid;
			toolSetPresenter.ListView = toolBoxWindow.ListView;

			ResetProject();
		}

		
		private void DatabaseDesignerForm_Shown(object sender, EventArgs e) {
			toolBoxWindow.Show(this);
			DockToolWindow();
			Activate();
		}


		private void CreateTools() {
			toolSetPresenter.ToolSetController.Clear();
			toolSetPresenter.ToolSetController.AddTool(new PointerTool(), true);
			Template template;

			string category = "Database Entities";
			DatabaseSymbol databaseShape = (DatabaseSymbol)project.ShapeTypes["Database"].CreateInstance();
			databaseShape.Width = 120;
			databaseShape.Height = 120;
			databaseShape.FillStyle = project.Design.FillStyles.Yellow;
			databaseShape.CharacterStyle = project.Design.CharacterStyles.Heading3;
			databaseShape.Text = "Database";
			CreateTemplateAndTool("Database", category, databaseShape);

			EntitySymbol tableShape = (EntitySymbol)project.ShapeTypes["Entity"].CreateInstance();
			tableShape.Width = 100;
			tableShape.Height = 160;
			tableShape.FillStyle = project.Design.FillStyles.Red;
			tableShape.CharacterStyle = project.Design.CharacterStyles.Heading3;
			tableShape.ParagraphStyle = project.Design.ParagraphStyles.Title;
			tableShape.ColumnCharacterStyle = project.Design.CharacterStyles.Caption;
			tableShape.ColumnParagraphStyle = project.Design.ParagraphStyles.Label;
			CreateTemplateAndTool("Entity", category, tableShape);

			Polyline line;
			ShapeType polylineType = project.ShapeTypes["Polyline"];
			line = (Polyline)polylineType.CreateInstance();
			line.LineStyle = project.Design.LineStyles.Thick;
			CreateTemplateAndTool("Relationship", category, line);

			line = (Polyline)polylineType.CreateInstance();
			line.LineStyle = project.Design.LineStyles.Thick;
			line.EndCapStyle = project.Design.CapStyles.Arrow;
			CreateTemplateAndTool("1:n Relationship", category, line);

			line = (Polyline)polylineType.CreateInstance();
			line.LineStyle = project.Design.LineStyles.Thick;
			line.StartCapStyle = project.Design.CapStyles.Arrow;
			line.EndCapStyle = project.Design.CapStyles.Arrow;
			CreateTemplateAndTool("n:m Relationship", category, line);

			CloudSymbol cloudShape = (CloudSymbol)project.ShapeTypes["Cloud"].CreateInstance();
			cloudShape.FillStyle = project.Design.FillStyles.Blue;
			cloudShape.CharacterStyle = project.Design.CharacterStyles.Heading1;
			cloudShape.Width = 300;
			cloudShape.Height = 160;
			cloudShape.Text = "WAN / LAN";
			CreateTemplateAndTool("Cloud", category, cloudShape);

			category = "Description";
			Text text = (Text)project.ShapeTypes["Text"].CreateInstance();
			text.CharacterStyle = project.Design.CharacterStyles.Normal;
			text.Width = 100;
			CreateTemplateAndTool("Text", category, text);

			AnnotationSymbol annotationShape = (AnnotationSymbol)project.ShapeTypes["Annotation"].CreateInstance();
			annotationShape.FillStyle = project.Design.FillStyles.White;
			annotationShape.CharacterStyle = project.Design.CharacterStyles.Caption;
			annotationShape.ParagraphStyle = project.Design.ParagraphStyles.Text;
			annotationShape.Width = 120;
			annotationShape.Height = 120;
			CreateTemplateAndTool("Annotation", category, annotationShape);

			category = "Miscellaneous";
			RoundedBox roundedRectangle = (RoundedBox)project.ShapeTypes["RoundedBox"].CreateInstance();
			roundedRectangle.FillStyle = project.Design.FillStyles.Green;
			roundedRectangle.Width = 120;
			roundedRectangle.Height = 80;
			CreateTemplateAndTool("Box", category, roundedRectangle);

			Ellipse ellipse = (Ellipse)project.ShapeTypes["Ellipse"].CreateInstance();
			ellipse.FillStyle = project.Design.FillStyles.Yellow;
			ellipse.Width = 120;
			ellipse.Height = 80;
			CreateTemplateAndTool("Ellipse", category, ellipse);

			Picture picture = (Picture)project.ShapeTypes["Picture"].CreateInstance();
			picture.FillStyle = project.Design.FillStyles.Transparent;
			picture.Width = 120;
			picture.Height = 120;
			CreateTemplateAndTool("Picture", category, picture);

			ShapeType circularArcType = project.ShapeTypes["CircularArc"];
			CircularArc arc;
			arc = (CircularArc)circularArcType.CreateInstance();
			arc.LineStyle = project.Design.LineStyles.Thick;
			CreateTemplateAndTool("Arc", category, arc);

			arc = (CircularArc)circularArcType.CreateInstance();
			arc.LineStyle = project.Design.LineStyles.Thick;
			arc.EndCapStyle = project.Design.CapStyles.Arrow;
			CreateTemplateAndTool("Bowed Arrow", category, arc);

			arc = (CircularArc)circularArcType.CreateInstance();
			arc.LineStyle = project.Design.LineStyles.Thick;
			arc.StartCapStyle = project.Design.CapStyles.Arrow;
			arc.EndCapStyle = project.Design.CapStyles.Arrow;
			CreateTemplateAndTool("Bowed Double Arrow", category, arc);
		}


		private void CreateTemplateAndTool(string name, string category, Shape shape) {
			Template template = new Template(name, shape);
			toolSetPresenter.ToolSetController.CreateTemplateTool(template, category);
			project.Repository.InsertTemplate(template);
		}


		private void AddNewDiagram() {
			// Count diagrams
			int diagramCnt = 0;
			foreach (Diagram d in project.Repository.GetDiagrams())
				diagramCnt++;
			
			Diagram diagram = new Diagram(string.Format("Diagram {0}", diagramCnt + 1));
			diagram.Width = 1600;
			diagram.Height = 1200;
			diagram.BackgroundGradientColor = Color.WhiteSmoke;
			if (diagramCnt % 2 == 0)
				diagram.BackgroundColor = Color.DarkRed;
			else diagram.BackgroundColor = Color.DarkBlue;
			diagram.BackgroundImage = new NamedImage(Database_Designer.Properties.Resources.NY028_3, "BackgroundImage");
			diagram.BackgroundImageLayout = ImageLayoutMode.Fit;

			project.Repository.InsertDiagram(diagram);
			display.Diagram = diagram;

			ToolStripMenuItem item = new ToolStripMenuItem();
			item.Text = diagram.Name;
			item.Tag = diagram;
			item.Click += new EventHandler(diagramDropDownItem_Click);
			
			diagramsDropDownButton.DropDown.Items.Add(item);
			diagramsDropDownButton.Text = diagram.Name;
		}


		private void AddLoadedDiagram(Diagram diagram) {
			display.Diagram = diagram;

			ToolStripMenuItem item = new ToolStripMenuItem();
			item.Text = diagram.Name;
			item.Tag = diagram;
			item.Click += new EventHandler(diagramDropDownItem_Click);

			diagramsDropDownButton.DropDown.Items.Add(item);
			diagramsDropDownButton.Text = diagram.Name;
		}


		private void DeleteDiagram(Diagram diagram) {			
			for (int i = diagramsDropDownButton.DropDown.Items.Count - 1; i >= 0; --i) {
				if (diagramsDropDownButton.DropDown.Items[i].Tag == diagram) {
					diagramsDropDownButton.DropDown.Items[i].Click -= new EventHandler(diagramDropDownItem_Click);
					diagramsDropDownButton.DropDown.Items.RemoveAt(i);

					diagram.Clear();
					project.Repository.DeleteDiagram(diagram);
					diagram = null;
				}
			}

			if (display.Diagram == null && diagramsDropDownButton.DropDown.Items.Count > 0)
				display.Diagram = diagramsDropDownButton.DropDown.Items[0].Tag as Diagram;
		}


		private void ResetProject(){
			if (project.IsOpen) {
				display.UnselectAll();
				display.Diagram = null;
				List<Diagram> diagrams = new List<Diagram>(project.Repository.GetDiagrams());
				for (int i = diagrams.Count - 1; i >= 0; i--)
					DeleteDiagram(diagrams[i]);
				project.Close();
			}

			xmlStore.DirectoryName = xmlStore.FileExtension = string.Empty;
			project.Name = defaultProjectName;
			project.Create();
			project.AddLibraryByName("Dataweb.nShape.GeneralShapes");
			project.AddLibraryByName("Dataweb.nShape.SoftwareArchitectureShapes");

			CreateTools();

			AddNewDiagram();
			SetButtonStates();
		}


		private void LoadRepository() {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = fileFilter;
			if (dlg.ShowDialog() == DialogResult.OK) {
				if (!string.IsNullOrEmpty(dlg.FileName) && File.Exists(dlg.FileName)) {
					project.Close();

					xmlStore.FileExtension = Path.GetExtension(dlg.FileName);
					xmlStore.DirectoryName = Path.GetDirectoryName(dlg.FileName);
					project.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
					project.Open();

					toolSetController.Clear();
					toolSetController.AddTool(new PointerTool(), true);
					foreach (Template template in project.Repository.GetTemplates()) {
						string category = "Miscellaneous";
						if (template.Shape is DatabaseSymbol
							|| template.Shape is EntitySymbol
									|| template.Shape is Polyline
									|| template.Shape is CloudSymbol) {
							category = "Database Entities";
						} else if (template.Shape is Text
									|| template.Shape is AnnotationSymbol) {
							category = "Description";
						} else category = "Miscellaneous";

						toolSetController.CreateTemplateTool(template, category);
					}

					// load diagrams
					if (project.Repository != null) {
						foreach (Diagram diagram in project.Repository.GetDiagrams())
							AddLoadedDiagram(diagram);
					}
				} else MessageBox.Show(string.Format("File '{0}' does not exist.", dlg.FileName));
			}
		}


		private void SaveRepository() {
			using (SaveFileDialog dlg = new SaveFileDialog()) {
				dlg.CreatePrompt = false;
				dlg.Filter = fileFilter;
				if (!string.IsNullOrEmpty(xmlStore.DirectoryName))
					dlg.InitialDirectory = xmlStore.DirectoryName;
				dlg.FileName = project.Name + (string.IsNullOrEmpty(xmlStore.FileExtension) ? ".xml" : xmlStore.FileExtension);
				if (dlg.ShowDialog(this) == DialogResult.OK) {
					project.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
					xmlStore.DirectoryName = Path.GetDirectoryName(dlg.FileName);
					xmlStore.FileExtension = Path.GetExtension(dlg.FileName);
				}
			}
			if (!string.IsNullOrEmpty(project.Name)
				&& !string.IsNullOrEmpty(xmlStore.DirectoryName))
				project.Repository.SaveChanges();
		}


		private void SetButtonStates() {
			//cutButton.Enabled = display.CanCut(false);
			//copyButton.Enabled = display.CanCopy(false);
			//pasteButton.Enabled = display.CanPaste();
			//deleteButton.Enabled = display.CanDelete(false);
		}


		private void display_ShapesSelected(object sender, EventArgs e) {
			SetButtonStates();
		}


		private void toolBoxButton_Click(object sender, EventArgs e) {
			if (toolBoxButton.Checked) {
				toolBoxWindow.Hide();
			}
			else {
				toolBoxWindow.Show();
				this.Focus();
			}
			DockPropertyWindow();
			DockToolWindow();
			toolBoxButton.Checked = toolBoxWindow.Visible;
		}


		private void propertyWindowButton_Click(object sender, EventArgs e) {
			if (propertyWindowButton.Checked) {
				propertyWindow.Hide();
			}
			else {
				propertyWindow.Show();
				this.Focus();
			}
			DockToolWindow();
			DockPropertyWindow();
			propertyWindowButton.Checked = propertyWindow.Visible;
		}


		private void designButton_Click(object sender, EventArgs e) {
			toolSetController.ShowDesignEditor();
		}


		private void diagramButton_Click(object sender, EventArgs e) {
			if (!propertyWindow.Visible) propertyWindowButton_Click(this, null);
			propertyController.SetObject(0, display.Diagram);
		}


		private void gridButton_Click(object sender, EventArgs e) {
			display.ShowGrid = gridButton.Checked;
		}

		
		private void zoomInButton_Click(object sender, EventArgs e) {
			if (display.ZoomLevel < 10000)
				display.ZoomLevel += zoomStep;
		}


		private void zoomOutButton_Click(object sender, EventArgs e) {
			if (display.ZoomLevel > zoomStep)
				display.ZoomLevel -= zoomStep;
		}

		
		private void cutButton_Click(object sender, EventArgs e) {
			display.Cut(false);
		}


		private void copyButton_Click(object sender, EventArgs e) {
			display.Copy(false);
		}


		private void pasteButton_Click(object sender, EventArgs e) {
			display.Paste(display.GridSize, display.GridSize);
		}


		private void deleteButton_Click(object sender, EventArgs e) {
			display.Delete(false);
		}

		
		private void deleteDiagramButton_Click(object sender, EventArgs e) {
			DeleteDiagram(display.Diagram);
		}
		
		
		private void diagramDropDownItem_Click(object sender, EventArgs e) {
			display.UnselectAll();
			display.Diagram = ((ToolStripMenuItem)sender).Tag as Diagram;
			diagramsDropDownButton.Text = display.Diagram.Name;
		}


		private void addDiagramButton_Click(object sender, EventArgs e) {
			AddNewDiagram();
		}


		private void clearButton_Click(object sender, EventArgs e) {
			ResetProject();
		}

		
		private void saveButton_Click(object sender, EventArgs e) {
			SaveRepository();
		}


		private void openButton_Click(object sender, EventArgs e) {
			LoadRepository();
		}


		#region nShape component's event handler implementations

		private void display_ZoomChanged(object sender, EventArgs e) {
			zoomLabel.Text = string.Format("{0} %", display.ZoomLevel);
		}


		private void toolSetController_TemplateEditorSelected(object sender, Dataweb.NShape.Controllers.ShowTemplateEditorEventArgs e) {
			TemplateEditorDialog dlg = new TemplateEditorDialog(e.Project, e.Template);
			dlg.Show(this);
		}


		private void toolSetController_DesignEditorSelected(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}

		#endregion
	}
}
