using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Diagnostics;


namespace Dataweb.NShape.WinFormsUI {

	public class DisplayDesigner : ScrollableControlDesigner {

		public DisplayDesigner() {
		}


		// Methods
		private void DrawBorder(Graphics graphics) {
			Color color;
			Control control = this.Control;
			Rectangle clientRectangle = control.ClientRectangle;
			if (control.BackColor.GetBrightness() < 0.5) {
				color = ControlPaint.Light(control.BackColor);
			} else {
				color = ControlPaint.Dark(control.BackColor);
			}
			Pen pen = new Pen(color);
			pen.DashStyle = DashStyle.Dash;
			clientRectangle.Width--;
			clientRectangle.Height--;
			graphics.DrawRectangle(pen, clientRectangle);
			pen.Dispose();
		}


		protected override void OnPaintAdornments(PaintEventArgs p) {
			Debug.Assert(base.Component is Display);
			Display component = base.Component as Display;
			if (component != null && component.BorderStyle == BorderStyle.None)
				this.DrawBorder(p.Graphics);
			base.OnPaintAdornments(p);
		}

	}

}
