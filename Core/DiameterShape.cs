using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;


namespace Dataweb.nShape.Advanced {

	public abstract class DiameterShapeBase : CaptionedShapeBase {

		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			internalDiameter = 40;
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is DiameterShapeBase)
				internalDiameter = ((DiameterShapeBase)source).DiameterInternal;
		}


		#region IPersistable Members

		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in CaptionedShapeBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Diameter", typeof(Int32));
		}


		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			internalDiameter = reader.ReadInt32();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(internalDiameter);
		}

		#endregion


		#region public Properties

		[Browsable(false)]
		protected internal override int ControlPointCount {
			get { return 9; }
		}

		#endregion


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopLeftControlPoint:
				case TopCenterControlPoint:
				case TopRightControlPoint:
				case MiddleLeftControlPoint:
				case MiddleRightControlPoint:
				case BottomLeftControlPoint:
				case BottomCenterControlPoint:
				case BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) > 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) > 0
								&& IsConnectionPointEnabled(controlPointId)));
				case ControlPointId.Reference:
				case MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) > 0
							|| (controlPointCapability & ControlPointCapabilities.Rotate) > 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) > 0 && IsConnectionPointEnabled(controlPointId)));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			// The RelativePosition of a RectangleBased shape is:
			// A = Tenths of percent of Width
			// B = Tenths of percent of Height
			Point result = Point.Empty;
			result.X = (int)Math.Round((X - DiameterInternal / 2f) + (DiameterInternal * (relativePosition.A / 1000f)));
			result.Y = (int)Math.Round((Y - DiameterInternal / 2f) + (DiameterInternal * (relativePosition.B / 1000f)));
			result = Geometry.RotatePoint(Center, Geometry.TenthsOfDegreeToDegrees(Angle), result);
			return result;
		}


		public override RelativePosition CalculateRelativePosition(int x, int y) {
			// The RelativePosition of a RectangleBased shape is:
			// A = Tenths of percent of Width
			// B = Tenths of percent of Height
			RelativePosition result = RelativePosition.Empty;
			if (Angle != 0) {
				float ptX = x;
				float ptY = y;
				Geometry.RotatePoint(X, Y, Geometry.TenthsOfDegreeToDegrees(-Angle), ref x, ref y);
			}
			result.A = (int)Math.Round((x - (X - DiameterInternal / 2f)) / (this.DiameterInternal / 1000f));
			result.B = (int)Math.Round((y - (Y - DiameterInternal / 2f)) / (this.DiameterInternal / 1000f));
			return result;
		}


		public override void Fit(int x, int y, int width, int height) {
			float scale = Geometry.CalcScaleFactor(DiameterInternal, DiameterInternal, width, height);
			DiameterInternal = (int)Math.Floor(DiameterInternal * scale);
			MoveControlPointTo(ControlPointId.Reference, x + (int)Math.Round(DiameterInternal / 2f), y + (int)Math.Round(DiameterInternal / 2f), ResizeModifiers.None);
		}


		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			UpdateDrawCache();
			DrawPath(graphics, LineStyle, FillStyle);
			DrawCaption(graphics);
			base.Draw(graphics);
		}


		protected internal DiameterShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DiameterShapeBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override int DivFactorX { get { return 2; } }


		protected override int DivFactorY { get { return 2; } }


		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int size = DiameterInternal;
			int hSize = size, vSize = size;
			switch (pointId) {
				#region TopLeft
				case TopLeftControlPoint:
					if (!Geometry.MoveRectangleTopLeft(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers | ResizeModifiers.MaintainAspect, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				#endregion

				#region TopCenter
				case TopCenterControlPoint:
					if (!Geometry.MoveRectangleTop(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out vSize))
						result = false;
					size = vSize;
					break;
				#endregion

				#region TopRight
				case TopRightControlPoint:
					if (!Geometry.MoveRectangleTopRight(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers | ResizeModifiers.MaintainAspect, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				#endregion

				#region Middle left
				case MiddleLeftControlPoint:
					if (!Geometry.MoveRectangleLeft(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize))
						result = false;
					size = hSize;
					break;
				#endregion

				#region Middle right
				case MiddleRightControlPoint:
					if (!Geometry.MoveRectangleRight(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out hSize))
						result = false;
					size = hSize;
					break;
				#endregion

				#region bottom left
				case BottomLeftControlPoint:
					if (!Geometry.MoveRectangleBottomLeft(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers | ResizeModifiers.MaintainAspect, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				#endregion

				#region bottom Center
				case BottomCenterControlPoint:
					if (!Geometry.MoveRectangleBottom(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out vSize))
						result = false;
					size = vSize;
					break;
				#endregion

				#region bottom right
				case BottomRightControlPoint:
					if (!Geometry.MoveRectangleBottomRight(size, size, transformedDeltaX, transformedDeltaY, cos, sin, modifiers | ResizeModifiers.MaintainAspect, out dx, out dy, out hSize, out vSize))
						result = false;
					size = Math.Min(hSize, vSize);
					break;
				#endregion
			}
			// Perform Resizing
			DiameterInternal = size;
			MoveByCore(dx, dy);
			ControlPointsHaveMoved();

			return result;
		}


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new IndexOutOfRangeException();
			captionBounds = Rectangle.Empty;
			captionBounds.X = captionBounds.Y = (int)Math.Round(-DiameterInternal / 2f);
			captionBounds.Width = captionBounds.Height = DiameterInternal;
		}


		protected int DiameterInternal {
			get { return internalDiameter; }
			set {
				if (value > 0) {
					Invalidate();
					if (Owner != null) Owner.NotifyChildResizing(this);
					int delta = value - internalDiameter;

					internalDiameter = value;
					ControlPointsHaveMoved();
					InvalidateDrawCache();

					if (ChildrenCollection != null) ChildrenCollection.NotifyParentSized(delta, delta);
					if (Owner != null) Owner.NotifyChildResized(this);
					Invalidate();
				}
			}
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

		private int internalDiameter = 0;
		#endregion
	}


	public abstract class SquareBase : DiameterShapeBase {

		public int Size {
			get { return base.DiameterInternal; }
			set { base.DiameterInternal = value; }
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = Geometry.InvalidRectangle;
			if (Size >= 0) {
				result.X = X - (int)Math.Round(Size / 2f);
				result.Y = Y - (int)Math.Round(Size / 2f);
				result.Width = result.Height = Size;
				if (Angle % 900 != 0) {
					Point tl, tr, bl, br;
					Geometry.RotateRectangle(result, Center, Geometry.TenthsOfDegreeToDegrees(Angle), out tl, out tr, out br, out bl);
					Geometry.CalcBoundingRectangle(tl, tr, bl, br, out result);
				}
			}
			return result;
		}


		protected internal SquareBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal SquareBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Size / 2f);
			int top = (int)Math.Round(-Size / 2f);
			int right = left + Size;
			int bottom = top + Size;

			// top row (left to right)			
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;

			// middle row (left to right)
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;

			// bottom row (left to right)
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;

			// rotate handle
			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;
		}


		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.RectangleContainsPoint(X - DiameterInternal / 2, Y - DiameterInternal / 2, DiameterInternal, DiameterInternal, Geometry.TenthsOfDegreeToDegrees(Angle), x, y, true);
		}
		
		
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.X = x;
			rectangle.Y = y;
			rectangle.Width = width;
			rectangle.Height = height;

			if (Angle % 900 == 0) {
				Rectangle bounds = Rectangle.Empty;
				bounds.X = X - (Size / 2);
				bounds.Y = Y - (Size / 2);
				bounds.Width = bounds.Height = Size;
				return rectangle.IntersectsWith(bounds);
			} else {
				if (rotatedBounds.Length != 4)
					Array.Resize<PointF>(ref rotatedBounds, 4);
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				float ptX, ptY;
				float halfSize = Size / 2f;
				ptX = X - halfSize;		// left
				ptY = Y - halfSize;	// top
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[0].X = ptX;
				rotatedBounds[0].Y = ptY;

				ptX = X + halfSize;		// right
				ptY = Y - halfSize;		// top
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[1].X = ptX;
				rotatedBounds[1].Y = ptY;

				ptX = X + halfSize;		// right
				ptY = Y + halfSize;	// bottom
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[2].X = ptX;
				rotatedBounds[2].Y = ptY;

				ptX = X - halfSize;		// left
				ptY = Y + halfSize;	// bottom
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[3].X = ptX;
				rotatedBounds[3].Y = ptY;

				return Geometry.PolygonIntersectsWithRectangle(rotatedBounds, rectangle);
			}
		}


		protected internal override int ControlPointCount {
			get { return 9; }
		}


		#region Fields
		private PointF[] rotatedBounds = new PointF[4];
		#endregion
	}


	public abstract class CircleBase : DiameterShapeBase {

		public int Diameter {
			get { return base.DiameterInternal; }
			set { base.DiameterInternal = value; }
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle result = Geometry.InvalidRectangle;
				if (DiameterInternal >= 0) {
					// No need to rotate the tight bounding rectangle of a circle
					result.X = X - (int)Math.Round(Diameter / 2f);
					result.Y = Y - (int)Math.Round(Diameter / 2f);
					result.Width = result.Height = Diameter;
				}
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal override int ControlPointCount { 
			get { return 13; } 
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case TopLeftControlPoint:
				case TopRightControlPoint:
				case BottomLeftControlPoint:
				case BottomRightControlPoint:
					return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
				case 10:
				case 11:
				case 12:
				case 13:
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		public override Point CalculateConnectionFoot(int startX, int startY) {
			Point p = Geometry.IntersectCircleWithLine(X, Y, (int)Math.Round(Diameter / 2f), startX, startY, X, Y, true);
			if (p != Geometry.InvalidPoint) return p;
			else return Center;
		}


		protected internal CircleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal CircleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.CircleContainsPoint(X, Y, Diameter / 2f, 0, x, y);
		}
		
		
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle r = Rectangle.Empty;
			r.X = x;
			r.Y = y;
			r.Width = width;
			r.Height = height;
			return Geometry.CircleIntersectsWithRectangle(r, Center, Diameter / 2f);
		}


		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Diameter / 2f);
			int top = (int)Math.Round(-Diameter / 2f);
			int right = left + Diameter;
			int bottom = top + Diameter;

			// Top left
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			// Top
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			// Top right
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;
			// Left
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			// Right
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;
			// Bottom left
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			// Bottom
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			// Bottom right
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;
			// Center
			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;

			if (ControlPointCount > 9) {
				double angle = Geometry.DegreesToRadians(45);
				int dx = (int)Math.Round((Diameter / 2f) - ((Diameter / 2f) * Math.Cos(angle)));
				int dy = (int)Math.Round((Diameter / 2f) - ((Diameter / 2f) * Math.Sin(angle)));
				// Top left
				ControlPoints[9].X = left + dx;
				ControlPoints[9].Y = top + dy;
				// Top right
				ControlPoints[10].X = right - dx;
				ControlPoints[10].Y = top + dy;
				// Bottom left
				ControlPoints[11].X = left + dx;
				ControlPoints[11].Y = bottom - dy;
				// Bottom right
				ControlPoints[12].X = right - dx;
				ControlPoints[12].Y = bottom - dy;
			}
		}


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new IndexOutOfRangeException();
			captionBounds = Rectangle.Empty;
			captionBounds.X = (int)Math.Round((-Diameter / 2f) + (Diameter / 8f));
			captionBounds.Y = (int)Math.Round((-Diameter / 2f) + (Diameter / 8f));
			captionBounds.Width = (int)Math.Round(Diameter - (Diameter / 4f));
			captionBounds.Height = (int)Math.Round(Diameter - (Diameter / 4f));
		}


		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopRightControlPoint = 3;
		private const int BottomLeftControlPoint = 6;
		private const int BottomRightControlPoint = 8;
	}

}