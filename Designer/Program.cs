using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Dataweb.Diagramming.Designer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
			[STAThread]
			static void Main() {
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
				Application.Run(new DiagramDesignerMainForm());
			}


			static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
				if (MessageBox.Show(e.Exception.Message + "\r\nDo you want to terminate the application?", "Unhandled Exception", MessageBoxButtons.YesNo) == DialogResult.Yes)
					Application.Exit();
			}
    }






}