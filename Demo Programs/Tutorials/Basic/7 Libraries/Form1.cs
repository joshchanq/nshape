using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.Layouters;


namespace BasicTutorial {

	public partial class Form1 : Form {

		[DllImport("shfolder.dll", CharSet = CharSet.Auto)]
		private static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);


		private static string GetSharedDocumentsPath() {
			StringBuilder sb = new StringBuilder(260);
			SHGetFolderPath(IntPtr.Zero, 46, IntPtr.Zero, 0, sb);
			return sb.ToString();
		}


		public Form1() {
			InitializeComponent();
		}


		private void Form1_Load(object sender, EventArgs e) {
			// Open the NShape project
			string sharedDocumentsDir = GetSharedDocumentsPath();
			sharedDocumentsDir = sharedDocumentsDir.Trim();
			// Path to the NShape sample diagrams
			xmlStore1.DirectoryName = Path.Combine(sharedDocumentsDir, Path.Combine("NShape", "Demo Projects"));
			project1.Name = "WebVisits Project";
			// Path to the NShape shape library assemblies
			string programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			project1.LibrarySearchPaths.Add(Path.Combine(programFilesDir, string.Format("dataweb{0}NShape{0}bin", Path.DirectorySeparatorChar)));
			project1.Create();

			project1.AddLibrary(typeof(Ellipse).Assembly);
			((CapStyle)project1.Design.CapStyles.Arrow).CapSize = 20;
			((CapStyle)project1.Design.CapStyles.Arrow).CapShape = CapShape.ArrowClosed;
			((CapStyle)project1.Design.CapStyles.Arrow).ColorStyle = project1.Design.ColorStyles.Green;
		}


		private void fileSaveToolStripMenuItem_Click(object sender, EventArgs e) {
			project1.Repository.SaveChanges();
		}


		private void fileLoadStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
			string appDir = Path.GetDirectoryName(Path.GetDirectoryName(Application.StartupPath));
			string statisticsFilePath = Path.GetFullPath(Path.Combine(appDir, @"Demo Programs\Tutorials\Basic\Sample Data\Small.txt"));

			Dictionary<string, RectangleBase> shapeDict = new Dictionary<string, RectangleBase>(1000);
			Diagram diagram = new Diagram("D1");

			int x = 10;
			int y = 500;
			TextReader reader = new StreamReader(statisticsFilePath);
			string line = reader.ReadLine();
			while (line != null) {
				int idx1 = line.IndexOf(';');
				int idx2 = line.IndexOf(';', idx1 + 1);
				string url = line.Substring(0, idx1);
				RectangleBase referringShape;
				if (!shapeDict.TryGetValue(url, out referringShape)) {
					referringShape = (RectangleBase)project1.ShapeTypes["Ellipse"].CreateInstance();
					referringShape.Width = 100;
					referringShape.Height = 60;
					referringShape.X = x + 50;
					referringShape.Y = y + 50;
					x += 120;
					if (x > 1200) {
						x = 10;
						y += 70;
					}
					referringShape.SetCaptionText(0, Path.GetFileNameWithoutExtension(url));
					shapeDict.Add(url, referringShape);
					diagram.Shapes.Add(referringShape);
				}
				url = line.Substring(idx1 + 1, idx2 - idx1 - 1);
				RectangleBase referredShape;
				if (!shapeDict.TryGetValue(url, out referredShape)) {
					referredShape = (RectangleBase)project1.ShapeTypes["Ellipse"].CreateInstance();
					referredShape.Width = 100;
					referredShape.Height = 60;
					referredShape.X = x + 50;
					referredShape.Y = y + 50;
					x += 120;
					if (x > 1200) {
						x = 10;
						y += 70;
					}
					referredShape.SetCaptionText(0, Path.GetFileNameWithoutExtension(url));
					shapeDict.Add(url, referredShape);
					diagram.Shapes.Add(referredShape);
				}
				// Add the connection
				Polyline arrow = (Polyline)project1.ShapeTypes["Polyline"].CreateInstance();
				diagram.Shapes.Add(arrow);
				arrow.EndCapStyle = project1.Design.CapStyles.Arrow; 
				arrow.Connect(ControlPointId.FirstVertex, referringShape, ControlPointId.Reference);
				arrow.Connect(ControlPointId.LastVertex, referredShape, ControlPointId.Reference);
				//
				// Next line
				line = reader.ReadLine();
			}
			reader.Close();
			cachedRepository1.InsertDiagram(diagram);
			display1.Diagram = diagram;
		}


		private void fileLayoutToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (Shape s in display1.Diagram.Shapes) {
				s.X = 100;
				s.Y = 100;
			}
			RepulsionLayouter layouter = new RepulsionLayouter(project1);
			layouter.SpringRate = 8;
			layouter.Repulsion = 3;
			layouter.RepulsionRange = 500;
			layouter.Friction = 0;
			layouter.Mass = 50;
			layouter.AllShapes = display1.Diagram.Shapes;
			layouter.Shapes = display1.Diagram.Shapes;
			//
			layouter.Prepare();
			layouter.Execute(10);
			layouter.Fit(50, 50, display1.Diagram.Width - 100, display1.Diagram.Height - 100);
		}

	}

}