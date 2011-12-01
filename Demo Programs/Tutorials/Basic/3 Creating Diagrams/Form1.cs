using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;


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
			project1.Name = "Circles";
			// Path to the NShape shape library assemblies
			string programFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			project1.LibrarySearchPaths.Add(Path.Combine(programFilesDir, string.Format("dataweb{0}NShape{0}bin", Path.DirectorySeparatorChar)));
			project1.Open();
			// Display a diagram
			display1.LoadDiagram("Diagram 1");
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
				RectangleBase shape;
				string url = line.Substring(0, idx1);
				if (!shapeDict.TryGetValue(url, out shape)) {
					shape = (RectangleBase)project1.ShapeTypes["Ellipse"].CreateInstance();
					shape.Width = 100;
					shape.Height = 60;
					shape.X = x + 50;
					shape.Y = y + 50;
					x += 120;
					if (x > 1200) {
						x = 10;
						y += 70;
					}
					shape.SetCaptionText(0, Path.GetFileNameWithoutExtension(url));
					shapeDict.Add(url, shape);
					diagram.Shapes.Add(shape);
				}
				url = line.Substring(idx1 + 1, idx2 - idx1 - 1);
				if (!shapeDict.TryGetValue(url, out shape)) {
					shape = (RectangleBase)project1.ShapeTypes["Ellipse"].CreateInstance();
					shape.Width = 100;
					shape.Height = 60;
					shape.X = x + 50;
					shape.Y = y + 50;
					x += 120;
					if (x > 1200) {
						x = 10;
						y += 70;
					}
					shape.SetCaptionText(0, Path.GetFileNameWithoutExtension(url));
					shapeDict.Add(url, shape);
					diagram.Shapes.Add(shape);
				}
				line = reader.ReadLine();
			}
			reader.Close();
			cachedRepository1.InsertDiagram(diagram);
			display1.Diagram = diagram;
		}

	}

}