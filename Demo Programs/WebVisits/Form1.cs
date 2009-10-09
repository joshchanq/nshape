using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Dataweb;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.Layouters;
using Dataweb.Xml;
using System.IO;


namespace WebVisists {

	public partial class MainForm : Form {

		public MainForm() {
			InitializeComponent();
		}


		private void loadWebStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.InitialDirectory = @"..\..\Sample Web Statistics";
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				// Neues Diagramm erzeugen
				ShapeType boxType = project.ShapeTypes["Ellipse"];
				ShapeType multiLineType = project.ShapeTypes["Polyline"];
				
				Dictionary<int, RectangleBase> boxes = new Dictionary<int, RectangleBase>();
				List<Polyline> lines = new List<Polyline>();
				//
				// Die Web-Seiten eintragen und verbinden
				XmlScanner scanner = new XmlScanner(openFileDialog.FileName);
				scanner.ReadElement();
				scanner.ReadElement("WebVisits");
				scanner.ReadChild("Pages");
				if (scanner.ReadChild("Page")) do {
					scanner.ReadAttribute(); // id attribute
					RectangleBase box = (RectangleBase)boxType.CreateInstance(pageTemplate);
					box.Width = 140;
					boxes.Add(scanner.IntValue, box);
					scanner.ReadAttribute();
					box.Text = scanner.StringValue;
				} while (scanner.ReadElement());
				scanner.ReadParent();
				if (scanner.ReadChild("Referral"))
					do {
						scanner.ReadAttribute(); // id1
						int id1 = scanner.IntValue;
						Shape shape1 = boxes[id1];
						scanner.ReadAttribute(); // id2
						int id2 = scanner.IntValue;
						Shape shape2 = boxes[id2];
						scanner.ReadAttribute(); // count
						int count = scanner.IntValue;
						Polyline line = (Polyline)multiLineType.CreateInstance();
						line.EndCapStyle = project.Design.CapStyles.Arrow;
						line.LineStyle = GetLineStyle(count);
						line.Connect(ControlPointId.FirstVertex, shape1, ControlPointId.Reference);
						line.Connect(ControlPointId.LastVertex, shape2, ControlPointId.Reference);
						lines.Add(line);
					} while (scanner.ReadElement());
				scanner.ReadParent();
				scanner.Close();
				//
				// Alle Shapes ins Diagramm eintragen
				int cnt = 0;
				foreach (Diagram d in project.Repository.GetDiagrams())
					++cnt;
				Diagram diagram = new Diagram(string.Format("WebVisits Diagram {0}", cnt));
				diagram.Width = 1000;
				diagram.Height = 1000;
				diagram.BackgroundImageLayout = Dataweb.NShape.ImageLayoutMode.Fit;
				foreach (RectangleBase b in boxes.Values)
					diagram.Shapes.Add(b, project.Repository.ObtainNewTopZOrder(diagram));
				foreach (Polyline l in lines)
					diagram.Shapes.Add(l, project.Repository.ObtainNewBottomZOrder(diagram));
				
				boxes.Clear();
				lines.Clear();
				//
				// Alle ins Repository eintragen
				project.Repository.InsertDiagram(diagram);
				//
				// Layouten
				RepulsionLayouter layouter = new RepulsionLayouter(project);
				layouter.SpringRate = 8;
				layouter.Repulsion = 3;
				layouter.RepulsionRange = 500;
				layouter.Friction = 0;
				layouter.Mass = 50;
				//
				layouter.AllShapes = diagram.Shapes;
				layouter.Shapes = diagram.Shapes;
				layouter.Execute(10);
				layouter.Fit(50, 50, diagram.Width- 100, diagram.Height-100);
				//
				// Anzeigen
				display.Diagram = diagram;
			}
		}


		private void saveDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			using (SaveFileDialog dlg = new SaveFileDialog()) {
				dlg.InitialDirectory = xmlStore.DirectoryName;
				if (dlg.ShowDialog() == DialogResult.OK) {
					xmlStore.DirectoryName = Path.GetDirectoryName(dlg.FileName);
					project.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
					project.Repository.SaveChanges();
				}
			}
		}


		private void Form1_Load(object sender, EventArgs e) {
			// Set User Acceess Rights
			DefaultSecurity sec = new DefaultSecurity();
			sec.CurrentRole = StandardRole.SuperUser;
			project.SecurityManager = sec;

			xmlStore.DirectoryName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"dataweb\WebVisits");
			if (!Directory.Exists(xmlStore.DirectoryName)) Directory.CreateDirectory(xmlStore.DirectoryName);

			project.Name = "Webvisits Project";
			project.Create();
			project.LibrarySearchPaths.Add(Application.StartupPath);
			project.AddLibraryByName("Dataweb.nShape.GeneralShapes");
			project.AddLibraryByName("Dataweb.nShape.SoftwareArchitectureShapes");

			CreateLineStyles();

			// Delete default tools
			toolBoxAdapter.ToolSetController.Clear();
			toolBoxAdapter.ListView.ShowGroups = false;
			
			// Add desired tools
			toolBoxAdapter.ToolSetController.AddTool(new PointerTool());
			
			Template t = new Template("Web Page", project.ShapeTypes["Ellipse"].CreateInstance());
			((RectangleBase)t.Shape).FillStyle = project.Design.FillStyles["Green"];
			project.Repository.InsertTemplate(t);
			pageTemplate = t;
			
			t = new Template("Label", project.ShapeTypes["Label"].CreateInstance());
			((RectangleBase)t.Shape).CharacterStyle = project.Design.CharacterStyles.Normal;
			project.Repository.InsertTemplate(t);
			
			t = new Template("Annotation", project.ShapeTypes["Annotation"].CreateInstance());
			((RectangleBase)t.Shape).FillStyle = project.Design.FillStyles.Yellow;
			project.Repository.InsertTemplate(t);
		}


		private void loadDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (Diagram d in project.Repository.GetDiagrams()) {
				display.Diagram = d;
				break;
			}
			display.Refresh();
		}


		private void toolbox_SelectedIndexChanged(object sender, EventArgs e) {

		}


		private void toolBoxAdapter_ToolSelected(object sender, EventArgs e) {
		}

		
		private void display_Load(object sender, EventArgs e) {

		}


		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
			display.SelectAll();
		}


		private void CreateLineStyles() {
			// Create Line Styles
			for (int j = 1; j <= 5; ++j) {
				for (int i = 1; i <= 10; ++i) {
					string name = string.Empty;
					IColorStyle colorStyle = null;
					switch (j) {
						case 1:
							name = "Very poor";
							colorStyle = project.Design.ColorStyles.Gray;
							break;
						case 2:
							name = "Poor";
							colorStyle = project.Design.ColorStyles.Black;
							break;
						case 3:
							name = "Normal";
							colorStyle = project.Design.ColorStyles.Green;
							break;
						case 4:
							name = "Good";
							colorStyle = project.Design.ColorStyles.Blue;
							break;
						case 5:
							name = "Excellent";
							colorStyle = project.Design.ColorStyles.Highlight;
							break;
						default: Debug.Fail(""); break;
					}
					LineStyle lineStyle = new LineStyle(string.Format("{0} {1}0%", name, i));
					lineStyle.LineWidth = i;
					lineStyle.ColorStyle = colorStyle;
					project.Design.AddStyle(lineStyle);
					project.Repository.InsertStyle(project.Design, lineStyle);
				}
			}
		}


		private ILineStyle GetLineStyle(int count) {
			int factor = 1 + (int)Math.Round(count / 100f);
			string name;
			switch (factor){
				case 1: name = "Very poor"; break;
				case 2: name = "Poor"; break;
				case 3: name = "Normal"; break;
				case 4: name = "Good"; break;
				default: name = "Excellent"; break;
			}
			if (factor < 10) {
				int rating = Math.Max(1, (int)Math.Round(count / (factor * 10f))) * 10;
				string styleName = string.Format("{0} {1}%", name, rating);
				return project.Design.LineStyles[styleName];
			} else return project.Design.LineStyles["Excellent 100%"];
		}


		private Template pageTemplate;

	}
}