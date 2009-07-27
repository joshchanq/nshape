using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Dataweb.Diagramming.Advanced;
using Dataweb.Diagramming.GeneralShapes;


namespace Dataweb.Diagramming.ElectricalShapes {

	public abstract class ElectricalRectangleBase : RectangleBase {
		
		protected override int ControlPointCount { get { return 9; } }


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopCenterControlPoint:
				case MiddleLeftControlPoint:
				case MiddleRightControlPoint:
				case BottomCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 || (controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				case TopLeftControlPoint:
				case TopRightControlPoint:
				case BottomLeftControlPoint:
				case BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0 || (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalRectangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalRectangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopCenterControlPoint = 2;
		private const int TopRightControlPoint = 3;
		private const int MiddleLeftControlPoint = 4;
		private const int MiddleRightControlPoint = 5;
		private const int BottomLeftControlPoint = 6;
		private const int BottomCenterControlPoint = 7;
		private const int BottomRightControlPoint = 8;
		private const int MiddleCenterControlPoint = 9;
	}


	public abstract class ElectricalSquareBase : SquareBase {
		
		protected override int ControlPointCount { get { return 9; } }


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopCenterControlPoint:
				case MiddleLeftControlPoint:
				case MiddleRightControlPoint:
				case BottomCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 || ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalSquareBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalSquareBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		// ControlPoint Id Constants
		private const int TopCenterControlPoint = 2;
		private const int MiddleLeftControlPoint = 4;
		private const int MiddleRightControlPoint = 5;
		private const int BottomCenterControlPoint = 7;
	}


	public abstract class ElectricalEllipseBase : EllipseBase {
		
		protected override int ControlPointCount { get { return 9; } }


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopCenterControlPoint:
				case MiddleLeftControlPoint:
				case MiddleRightControlPoint:
				case BottomCenterControlPoint:
					return (controlPointCapability & ControlPointCapabilities.Resize) != 0 && (IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalEllipseBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalEllipseBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		// ControlPoint Id Constants
		private const int TopCenterControlPoint = 2;
		private const int MiddleLeftControlPoint = 4;
		private const int MiddleRightControlPoint = 5;
		private const int BottomCenterControlPoint = 7;
	}


	public abstract class ElectricalCircleBase : CircleBase {

		protected override int ControlPointCount { get { return 9; } }


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopLeftControlPoint:
				case TopRightControlPoint:
				case BottomLeftControlPoint:
				case BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalCircleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalCircleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopRightControlPoint = 3;
		private const int BottomLeftControlPoint = 6;
		private const int BottomRightControlPoint = 8;
	}


	public abstract class ElectricalTriangleBase : IsoscelesTriangleBase {

		protected override int ControlPointCount { get { return 5; } }


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointId.Reference:
				case MiddleCenterControlPoint:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
				case BottomCenterControlPoint:
				case TopCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 || (controlPointCapability & ControlPointCapabilities.Connect) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
					//return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
			}
		}

		
		protected internal ElectricalTriangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalTriangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		// ControlPoint Id Constants
		private const int TopCenterControlPoint = 1;
		private const int BottomLeftControlPoint = 2;
		private const int BottomCenterControlPoint = 3;
		private const int BottomRightControlPoint = 4;
		private const int MiddleCenterControlPoint = 5;
	}


	public class DisconnectorSymbol : ElectricalCircleBase {

		public override Shape Clone() {
			Shape result = new DisconnectorSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()){
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);

				Path.StartFigure();
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		public DisconnectorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		public DisconnectorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class AutoDisconnectorSymbol : ElectricalCircleBase {

		public override Shape Clone() {
			Shape result = new AutoDisconnectorSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);
				int right = left + Diameter;

				Path.StartFigure();
				Path.AddLine(left, top, right, top);
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle result = Rectangle.Empty;
				result.X = result.Y = -(int)Math.Round(Diameter / 2f);
				result.Width = result.Height = Diameter;
				// Rotate bottom line
				if (Angle != 0) {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					Point tl = Point.Empty;
					tl.Offset(result.Left, result.Top);
					tl = Geometry.RotatePoint(Point.Empty, angleDeg, tl);
					Point tr = Point.Empty;
					tr.Offset(result.Right, result.Top);
					tr = Geometry.RotatePoint(Point.Empty, angleDeg, tr);
					Geometry.UniteRectangles(tl.X, tl.Y, tr.X, tr.Y, result);
					result.Offset(X, Y);
				}
				return result;
			} return base.CalculateBoundingRectangle(tight);
		}


		public AutoDisconnectorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		public AutoDisconnectorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class AutoSwitchSymbol : ElectricalSquareBase {

		public override Shape Clone() {
			Shape result = new AutoSwitchSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()){
			Path.Reset();
				int left = (int)Math.Round(-Size / 2f);
				int top = (int)Math.Round(-Size / 2f);
				int bottom = top + Size;

				shapeSmallRect.Width = (Size / 6) * 3;
				shapeSmallRect.Height = Size / 6;
				shapeSmallRect.X = left + ((Size - shapeSmallRect.Width) / 2);
				shapeSmallRect.Y = bottom - shapeSmallRect.Height;

				shapeBigRect.Width = Size - (shapeSmallRect.Width / 4);
				shapeBigRect.Height = Size - shapeSmallRect.Height;
				shapeBigRect.X = left + ((Size - shapeBigRect.Width) / 2);
				shapeBigRect.Y = top;

				Path.StartFigure();
				Path.AddRectangle(shapeSmallRect);
				Path.AddRectangle(shapeBigRect);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		public AutoSwitchSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		public AutoSwitchSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		private Rectangle shapeSmallRect;
		private Rectangle shapeBigRect;
	}


	public class SwitchSymbol : ElectricalSquareBase {

		public override Shape Clone() {
			Shape result = new SwitchSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Size / 2f);
				int top = (int)Math.Round(-Size / 2f);

				shapeRect.X = left;
				shapeRect.Y = top;
				shapeRect.Width = Size;
				shapeRect.Height = Size;

				Path.StartFigure();
				Path.AddRectangle(shapeRect);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		public SwitchSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		public SwitchSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		#region Fields
		System.Drawing.Rectangle shapeRect;
		#endregion
	}


	public class BusBarSymbol : Polyline {

		public override Shape Clone() {
			Shape result = new BusBarSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if ((controlPointCapability & ControlPointCapabilities.Connect) != 0)
				return true;
			if ((controlPointCapability & ControlPointCapabilities.Glue) != 0) {
				// always false
			}
			if ((controlPointCapability & ControlPointCapabilities.Reference) != 0) {
				if (controlPointId == ControlPointId.Reference || controlPointId == 1) return true;
			}
			if ((controlPointCapability & ControlPointCapabilities.Rotate) != 0) {
				// always false
			}
			if ((controlPointCapability & ControlPointCapabilities.Resize) != 0)
				return true;
			return false;
		}


		protected internal BusBarSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal BusBarSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}

	
	public class TransformerSymbol : ElectricalRectangleBase {

		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			FillStyle = styleSet.FillStyles.Transparent;
			Width = 40;
			Height = 70;
		}


		public override Shape Clone() {
			Shape result = new TransformerSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if ((controlPointCapability & ControlPointCapabilities.Connect) != 0) {
				if (controlPointId == TopCenterControlPoint || controlPointId == BottomCenterControlPoint)
					return true;
			}
			if ((controlPointCapability & ControlPointCapabilities.Glue) != 0) {
				// always false
			}
			if ((controlPointCapability & ControlPointCapabilities.Reference) != 0) {
				if (controlPointId == ControlPointId.Reference || controlPointId == MiddleCenterControlPoint) 
					return true;
			}
			if ((controlPointCapability & ControlPointCapabilities.Rotate) != 0) {
				if (controlPointId == MiddleCenterControlPoint)
					return true;
			}
			if ((controlPointCapability & ControlPointCapabilities.Resize) != 0)
				if (controlPointId != -1 && controlPointId != MiddleCenterControlPoint)
					return true;
			return false;
		}


		protected internal TransformerSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal TransformerSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool CalculatePath() {
			Path.Reset();

			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int bottom = top + Height;

			d = (Width / 8);
			ringWidth = Width - LineStyle.LineWidth - LineStyle.LineWidth;
			ringHeight = (int)Math.Round(Height / 2f) + d;

			Path.StartFigure();
			circleShape.X = left + LineStyle.LineWidth;
			circleShape.Y = top + LineStyle.LineWidth;
			circleShape.Width = ringWidth;
			circleShape.Height = ringHeight;
			Path.AddEllipse(circleShape);
			Path.CloseFigure();

			Path.StartFigure();
			circleShape.Inflate(-d, -d);
			Path.AddEllipse(circleShape);
			Path.CloseFigure();

			Path.StartFigure();
			circleShape.X = left + LineStyle.LineWidth;
			circleShape.Y = bottom - ringHeight;
			circleShape.Width = ringWidth;
			circleShape.Height = ringHeight;
			Path.AddEllipse(circleShape);
			Path.CloseFigure();

			Path.StartFigure();
			circleShape.Inflate(-d, -d);
			Path.AddEllipse(circleShape);
			Path.CloseFigure();
			Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
			return true;
		}


		#region Fields

		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopCenterControlPoint = 2;
		private const int TopRightControlPoint = 3;
		private const int MiddleLeftControlPoint = 4;
		private const int MiddleRightControlPoint = 5;
		private const int BottomLeftControlPoint = 6;
		private const int BottomCenterControlPoint = 7;
		private const int BottomRightControlPoint = 8;
		private const int MiddleCenterControlPoint = 9;
		
		int d;
		int ringWidth;
		int ringHeight;
		Rectangle circleShape;

		#endregion
	}


	public class EarthSymbol : ElectricalRectangleBase {

		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 40;
			Height = 40;
		}


		public override Shape Clone() {
			Shape result = new EarthSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 
								&& IsConnectionPointEnabled(controlPointId)));
				case TopLeftControlPoint:
				case TopRightControlPoint:
				case MiddleLeftControlPoint:
				case MiddleRightControlPoint:
				case BottomLeftControlPoint:
				case BottomCenterControlPoint:
				case BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
							|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
					//return false;
			}
		}

		
		protected internal EarthSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal EarthSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;


				int lineWidth = LineStyle.LineWidth;
				int lineX = -lineWidth;
				int largeAntennaTop = -lineWidth;
				int largeAntennaBottom = lineWidth;
				int mediumAntennaTop = bottom - (int)Math.Round(Height / 4f) - lineWidth;
				int mediumAntennaBottom = bottom - (int)Math.Round(Height / 4f) + lineWidth;
				int smallAntennaTop = bottom - lineWidth - lineWidth;

				Path.StartFigure();

				// downward line from top to large 'antenna', left side
				Path.AddLine(lineX, top, lineX, largeAntennaTop);
				// large 'antenna', left side
				Path.AddLine(lineX, largeAntennaTop, left, largeAntennaTop);
				Path.AddLine(left, largeAntennaTop, left, largeAntennaBottom);
				Path.AddLine(left, largeAntennaBottom, lineX, largeAntennaBottom);
				// downward line from large 'antenna' to medium 'antenna', left side
				Path.AddLine(lineX, largeAntennaBottom, lineX, mediumAntennaTop);
				// medium 'antenna', left side
				int antennaLeft = left + (int)Math.Round(Width / 6f);
				Path.AddLine(lineX, mediumAntennaTop, antennaLeft, mediumAntennaTop);
				Path.AddLine(antennaLeft, mediumAntennaTop, antennaLeft, mediumAntennaBottom);
				Path.AddLine(antennaLeft, mediumAntennaBottom, lineX, mediumAntennaBottom);
				// downward line from medium 'antenna' to small 'antenna', left side				
				Path.AddLine(lineX, mediumAntennaBottom, lineX, smallAntennaTop);
				// small 'antenna', complete
				antennaLeft = left + (int)Math.Round(Width / 3f);
				int antennaRight = right - (int)Math.Round(Width / 3f);
				Path.AddLine(lineX, smallAntennaTop, antennaLeft, smallAntennaTop);
				Path.AddLine(antennaLeft, smallAntennaTop, antennaLeft, bottom);
				Path.AddLine(antennaLeft, bottom, antennaRight, bottom);
				lineX = lineWidth;
				Path.AddLine(antennaRight, bottom, antennaRight, smallAntennaTop);
				Path.AddLine(antennaRight, smallAntennaTop, lineX, smallAntennaTop);
				// upward line from small 'antenna' to medium 'antenna', right side
				Path.AddLine(lineX, smallAntennaTop, lineX, mediumAntennaBottom);
				// medium 'antenna', right side
				antennaRight = right - (int)Math.Round(Width / 6f);
				Path.AddLine(lineX, mediumAntennaBottom, antennaRight, mediumAntennaBottom);
				Path.AddLine(antennaRight, mediumAntennaBottom, antennaRight, mediumAntennaTop);
				Path.AddLine(antennaRight, mediumAntennaTop, lineX, mediumAntennaTop);
				// upward line from medium 'antenna' to large 'antenna', right side
				Path.AddLine(lineX, mediumAntennaTop, lineX, largeAntennaBottom);
				// large 'antenna', right side
				Path.AddLine(lineX, largeAntennaBottom, right, largeAntennaBottom);
				Path.AddLine(right, largeAntennaBottom, right, largeAntennaTop);
				Path.AddLine(right, largeAntennaTop, lineX, largeAntennaTop);
				// upward line from large 'antenna' to top, right side
				Path.AddLine(lineX, largeAntennaTop, lineX, top);
				Path.AddLine(lineX, top, -lineWidth, top);

				Path.CloseFigure();
				return true;
			}
			return false;
		}
		
	
		#region Fields

		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopCenterControlPoint = 2;
		private const int TopRightControlPoint = 3;
		private const int MiddleLeftControlPoint = 4;
		private const int MiddleRightControlPoint = 5;
		private const int BottomLeftControlPoint = 6;
		private const int BottomCenterControlPoint = 7;
		private const int BottomRightControlPoint = 8;
		private const int MiddleCenterControlPoint = 9;

		private Rectangle shapeBuffer = Rectangle.Empty;

		#endregion
	}


	public class FeederSymbol : ElectricalTriangleBase {

		public override Shape Clone() {
			Shape result = new FeederSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal FeederSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FeederSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class RectifierSymbol : ElectricalTriangleBase {

		public override Shape Clone() {
			Shape result = new RectifierSymbol(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width * CenterPosFactorX);
				int top = (int)Math.Round(-Height * CenterPosFactorY);
				int right = left + Width;
				int bottom = top + Height;

				Path.StartFigure();
				Path.AddLine(left, top, right, top);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		protected internal RectifierSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal RectifierSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class DisconnectingPoint : ElectricalRectangleBase {

		public override Shape Clone() {
			Shape result = new DisconnectingPoint(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {				
				case MiddleLeftControlPoint:
				case MiddleRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 
								&& IsConnectionPointEnabled(controlPointId)));
				case TopLeftControlPoint:
				case TopCenterControlPoint:
				case TopRightControlPoint:
				case BottomLeftControlPoint:
				case BottomCenterControlPoint:
				case BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
							|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
					//return false;
			}
		}


		protected internal DisconnectingPoint(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DisconnectingPoint(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				int offsetX = (int)Math.Round(Width / 6f);
				int offsetY = (int)Math.Round(Height / 3f);

				Path.StartFigure();
				Path.AddLine(left, top, left, bottom);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(left, top, left, bottom);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(left + offsetX, top + offsetY, left + offsetX, bottom - offsetY);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(left + offsetX + offsetX, 0, right - offsetX - offsetX, 0);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(right - offsetX, top + offsetY, right - offsetX, bottom - offsetY);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(right, top, right, bottom);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		#region Fields

		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopCenterControlPoint = 2;
		private const int TopRightControlPoint = 3;
		private const int MiddleLeftControlPoint = 4;
		private const int MiddleRightControlPoint = 5;
		private const int BottomLeftControlPoint = 6;
		private const int BottomCenterControlPoint = 7;
		private const int BottomRightControlPoint = 8;
		private const int MiddleCenterControlPoint = 9;

		private Rectangle shapeBuffer= Rectangle.Empty;
		#endregion
	}


	public static class DiagrammingLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			registrar.RegisterLibrary(libraryName, preferredRepositoryVersion);
			registrar.RegisterShapeType(new ShapeType("BusBar", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new BusBarSymbol(shapeType, t); }, 
				BusBarSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceHorizontalBar));
			registrar.RegisterShapeType(new ShapeType("Disconnector", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new DisconnectorSymbol(shapeType, t); }, 
				DisconnectorSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceCircleWithBar));
			registrar.RegisterShapeType(new ShapeType("AutoDisconnector", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new AutoDisconnectorSymbol(shapeType, t); }, 
				AutoDisconnectorSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceCircleWithBar));
			registrar.RegisterShapeType(new ShapeType("AutoSwitch", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new AutoSwitchSymbol(shapeType, t); }, 
				AutoSwitchSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Switch", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new SwitchSymbol(shapeType, t); }, 
				SwitchSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Transformer", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new TransformerSymbol(shapeType, t); }, 
				TransformerSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceDoubleCircle));
			registrar.RegisterShapeType(new ShapeType("Earth", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new EarthSymbol(shapeType, t); }, 
				EarthSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
			registrar.RegisterShapeType(new ShapeType("Feeder", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new FeederSymbol(shapeType, t); }, 
				FeederSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
			registrar.RegisterShapeType(new ShapeType("Rectifier", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new RectifierSymbol(shapeType, t); }, 
				RectifierSymbol.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
			registrar.RegisterShapeType(new ShapeType("DisconnectingPoint", libraryName, libraryName, 
				delegate(ShapeType shapeType, Template t) { return new DisconnectingPoint(shapeType, t); }, 
				DisconnectingPoint.GetPropertyDefinitions, Dataweb.Diagramming.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
		}


		private const string libraryName = "ElectricalShapes";
		private const int preferredRepositoryVersion = 3;
	}
}
