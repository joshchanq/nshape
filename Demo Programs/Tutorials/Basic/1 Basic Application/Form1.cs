using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

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
			display1.LoadDiagram("Diagram 1");
		}

	}

}