/******************************************************************************
  Copyright 2009 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Dataweb.NShape.Advanced;
using Dataweb.Utilities;


namespace Dataweb.NShape.GeneralShapes {

	public class Ellipse : EllipseBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Ellipse(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Ellipse(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Ellipse(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal Ellipse(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);

				Path.Reset();
				Path.StartFigure();
				Path.AddEllipse(left, top, Width, Height);
				Path.CloseFigure();
				return true;
			} else return false;
		}
	}


	public class Circle : CircleBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Circle(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Circle(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Circle(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal Circle(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);
				Path.Reset();
				Path.StartFigure();
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			} else return false;
		}
	}


	/// <summary>
	/// Rectangular shape with caption.
	/// </summary>
	public class Box : RectangleBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Box(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Box(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Box(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal Box(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();

				Rectangle shapeRect = Rectangle.Empty;
				shapeRect.Offset((int)Math.Round(-Width / 2f), (int)Math.Round(-Height / 2f));
				shapeRect.Width = Width;
				shapeRect.Height = Height;

				Path.StartFigure();
				Path.AddRectangle(shapeRect);
				Path.CloseFigure();
				return true;
			} else return false;
		}
	}


	public class Square : SquareBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Square(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Square(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Square(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal Square(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Size / 2f);
				int top = (int)Math.Round(-Size / 2f);

				shapeRect.X = left;
				shapeRect.Y = top;
				shapeRect.Width = Size;
				shapeRect.Height = Size;

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(shapeRect);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		#region Fields
		Rectangle shapeRect;
		#endregion
	}


	public class RoundedBox : RectangleBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new RoundedBox(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new RoundedBox(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			Point result = base.CalculateConnectionFoot(startX, startY);
			int radius = CalcCornerRadius();

			if (Angle == 0) {
				int left = (int)Math.Round(X - (Width / 2f));
				int top = (int)Math.Round(Y - (Height / 2f));
				int right = left + Width;
				int bottom = top + Height;
				Point p = Geometry.InvalidPoint;
				if (left <= result.X && result.X <= left + radius && top <= result.Y && result.Y <= top + radius) {
					// Check TopLeft corner
					p = Geometry.IntersectCircleWithLine(left + radius, top + radius, radius, startX, startY, X, Y, false);
					if (Geometry.IsValid(p)) result = p;
				} else if (right - radius <= result.X && result.X <= right && top <= result.Y && result.Y <= top + radius) {
					// Check TopRight corner
					p = Geometry.IntersectCircleWithLine(right - radius, top + radius, radius, startX, startY, X, Y, false);
					if (Geometry.IsValid(p)) result = p;
				} else if (left <= result.X && result.X <= left + radius && bottom - radius <= result.Y && result.Y <= bottom) {
					// Check BottomLeft corner
					p = Geometry.IntersectCircleWithLine(left + radius, bottom - radius, radius, startX, startY, X, Y, false);
					if (Geometry.IsValid(p)) result = p;
				} else if (right - radius <= result.X && result.X <= right && bottom - radius <= result.Y && result.Y <= bottom) {
					// Check BottomRight corner
					p = Geometry.IntersectCircleWithLine(right - radius, bottom - radius, radius, startX, startY, X, Y, false);
					if (Geometry.IsValid(p)) result = p;
				}
			} else {
				// Check the top and bottom side (between the rounded corners:
				// If the line intersects with any of these sides, we need not calculate the rounded corner intersection
				int cornerRadius = CalcCornerRadius();
				float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
				if (!Geometry.RectangleIntersectsWithLine(X, Y, Width - (2 * cornerRadius), Height, angleDeg, startX, startY, result.X, result.Y, true)
					&& !Geometry.RectangleIntersectsWithLine(X, Y, Width, Height - (2 * cornerRadius), angleDeg, startX, startY, result.X, result.Y, true)) {
					// If there is no intersection with any of the straight sides, check the rounded corners:

					// Calculate all center points of all corner roundings
					PointF topLeft = PointF.Empty, topRight = PointF.Empty, bottomRight = PointF.Empty, bottomLeft = PointF.Empty;
					RectangleF rect = RectangleF.Empty;
					rect.X = X - (Width / 2f);
					rect.Y = Y - (Height / 2f);
					rect.Width = Width;
					rect.Height = Height;
					rect.Inflate(-cornerRadius, -cornerRadius);
					Geometry.RotateRectangle(rect, X, Y, angleDeg, out topLeft, out topRight, out bottomRight, out bottomLeft);

					// Check corner roundings for intersection with the calculated line
					PointF p = Geometry.InvalidPointF;
					if (Geometry.CircleIntersectsWithLine(topLeft.X, topLeft.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(topLeft.X, topLeft.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					} else if (Geometry.CircleIntersectsWithLine(topRight.X, topRight.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(topRight.X, topRight.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					} else if (Geometry.CircleIntersectsWithLine(bottomRight.X, bottomRight.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(bottomRight.X, bottomRight.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					} else if (Geometry.CircleIntersectsWithLine(bottomLeft.X, bottomLeft.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(bottomLeft.X, bottomLeft.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					}
				}
			}
			return result;
		}


		protected internal RoundedBox(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal RoundedBox(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				if (Angle % 900 == 0) return base.CalculateBoundingRectangle(tight);
				else {
					Rectangle result = Geometry.InvalidRectangle;
					// Calculate the minimum bounding box
					int cornerRadius = CalcCornerRadius();
					int w = (int)Math.Round(Width / 2f) - cornerRadius;
					int h = (int)Math.Round(Height / 2f) - cornerRadius;
					Rectangle rect = Rectangle.Empty;
					rect.Offset(X - w, Y - h);
					rect.Width = Width - cornerRadius - cornerRadius;
					rect.Height = Height - cornerRadius - cornerRadius;
					Point topLeft, topRight, bottomLeft, bottomRight;
					Geometry.RotateRectangle(rect, Center, Geometry.TenthsOfDegreeToDegrees(Angle), out topLeft, out topRight, out bottomRight, out bottomLeft);

					result.X = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
					result.Y = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
					result.Width = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X)) - result.X;
					result.Height = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y)) - result.Y;
					result.Inflate(cornerRadius, cornerRadius);
					return result;
				}
			} else return base.CalculateBoundingRectangle(tight);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int r = CalcCornerRadius();

				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				Path.Reset();
				Path.StartFigure();
				Path.AddLine(left + r, top, right - r, top);
				Path.AddArc(right - r - r, top, r + r, r + r, -90, 90);
				Path.AddLine(right, top + r, right, bottom - r);
				Path.AddArc(right - r - r, bottom - r - r, r + r, r + r, 0, 90);
				Path.AddLine(right - r, bottom, left + r, bottom);
				Path.AddArc(left, bottom - r - r, r + r, r + r, 90, 90);
				Path.AddLine(left, bottom - r, left, top + r);
				Path.AddArc(left, top, r + r, r + r, 180, 90);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		private int CalcCornerRadius() {
			int result = 10;
			if (Width < 30)
				result = (int)Math.Round((float)(Width - 2) / 3);
			if (Height < 30)
				result = (int)Math.Round((float)(Height - 2) / 3);
			if (result <= 0) result = 1;
			return result;
		}
	}


	public class Diamond : DiamondBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Diamond(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Diamond(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Diamond(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal Diamond(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}

	}


	public class IsoscelesTriangle : IsoscelesTriangleBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new IsoscelesTriangle(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new IsoscelesTriangle(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal IsoscelesTriangle(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal IsoscelesTriangle(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class ThickArrow : RectangleBase {

		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			bodyHeightRatio = 1d / 3d;
			headWidth = (int)Math.Round(Width / 2f);
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is ThickArrow) {
				this.headWidth = ((ThickArrow)source).headWidth;
				this.bodyHeightRatio = ((ThickArrow)source).bodyHeightRatio;
			}
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new ThickArrow(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		public int BodyWidth {
			get { return Width - headWidth; }
		}


		[Category("Layout")]
		[Description("The height of the arrow's body.")]
		[PropertyMappingId(PropertyIdBodyHeight)]
		[RequiredPermission(Permission.Layout)]
		public int BodyHeight {
			get { return (int)Math.Round(Height * bodyHeightRatio); }
			set {
				Invalidate();
				if (value > Height) throw new ArgumentOutOfRangeException("BodyHeight");

				if (Height == 0) bodyHeightRatio = 0;
				else bodyHeightRatio = value / (float)Height;
				
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[Category("Layout")]
		[Description("The width of the arrow's tip.")]
		[PropertyMappingId(PropertyIdHeadWidth)]
		[RequiredPermission(Permission.Layout)]
		public int HeadWidth {
			get { return headWidth; }
			set {
				Invalidate();
				headWidth = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 7; }
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ArrowTipControlPoint:
				case BodyEndControlPoint:
					// ToDo: Implement GluePoint behavior for ThickArrows
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
						/*|| (controlPointCapabilities & ControlPointCapabilities.Glue) != 0*/);
				case ArrowTopControlPoint:
				case BodyTopControlPoint:
				case BodyBottomControlPoint:
				case ArrowBottomControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
								|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
								&& IsConnectionPointEnabled(controlPointId)));

				case RotateControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
								|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0
								|| (controlPointCapability & ControlPointCapabilities.Connect) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
			//return base.HasControlPointCapability(connectionPointId, controlPointCapabilities);
		}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			float headWidthRatio = this.HeadWidth / (float)Width;
			HeadWidth = (int)Math.Round(width * headWidthRatio);
			base.Fit(x, y, width, height);
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			CalcShapePoints();
			PointF rotationCenter = PointF.Empty;
			rotationCenter.X = X;
			rotationCenter.Y = Y;
			Matrix.Reset();
			Matrix.Translate(X, Y, MatrixOrder.Prepend);
			Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), rotationCenter, MatrixOrder.Append);
			Matrix.TransformPoints(shapePoints);
			Matrix.Reset();

			Point startPoint = Point.Empty;
			startPoint.X = startX;
			startPoint.Y = startY;
			Point result = Geometry.GetNearestPoint(startPoint, Geometry.IntersectPolygonLine(shapePoints, startX, startY, X, Y, true));
			if (!Geometry.IsValid(result)) result = Center;
			return result;
		}


		#region IEntity Members

		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			HeadWidth = reader.ReadInt32();
			BodyHeight = reader.ReadInt32();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(HeadWidth);
			writer.WriteInt32(BodyHeight);
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ThickArrow" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("HeadWidth", typeof(int));
			yield return new EntityFieldDefinition("BodyHeight", typeof(int));
		}

		#endregion


		protected internal ThickArrow(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ThickArrow(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new IndexOutOfRangeException();
			captionBounds = Rectangle.Empty;
			captionBounds.Width = (int)Math.Round(Width - (HeadWidth / 3f));
			captionBounds.Height = BodyHeight;
			captionBounds.X = -(int)Math.Round((Width / 2f) - (HeadWidth / 3f));
			captionBounds.Y = -(int)Math.Round(captionBounds.Height / 2f);
			if (ParagraphStyle != null) {
				captionBounds.X += ParagraphStyle.Padding.Left;
				captionBounds.Y += ParagraphStyle.Padding.Top;
				captionBounds.Width -= ParagraphStyle.Padding.Horizontal;
				captionBounds.Height -= ParagraphStyle.Padding.Vertical;
			}
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				CalcShapePoints();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = Geometry.InvalidRectangle;
			if (Width >= 0 && Height >= 0) {
				CalcShapePoints();
				Geometry.CalcBoundingRectangle(shapePoints, 0, 0, Geometry.TenthsOfDegreeToDegrees(Angle), out result);
				if (Geometry.IsValid(result)) result.Offset(X, Y);
			}
			return result;
		}


		/// <override></override>
		protected override int GetControlPointIndex(ControlPointId id) {
			switch (id) {
				case ArrowTipControlPoint: return 0;
				case ArrowTopControlPoint: return 1;
				case BodyTopControlPoint: return 2;
				case BodyEndControlPoint: return 3;
				case BodyBottomControlPoint: return 4;
				case ArrowBottomControlPoint: return 5;
				case RotateControlPoint: return 6;
				default:
					return base.GetControlPointIndex(id);
			}
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = -(int)Math.Round(Width / 2f);
			int right = left + Width;
			int top = -(int)Math.Round(Height / 2f);
			int bottom = top + Height;
			int halfBodyWidth = (int)Math.Round(BodyWidth / 2f);
			int halfBodyHeight = (int)Math.Round(BodyHeight / 2f);

			int i = 0;
			controlPoints[i].X = left;
			controlPoints[i].Y = 0;
			++i;
			controlPoints[i].X = right - BodyWidth;
			controlPoints[i].Y = top;
			++i;
			controlPoints[i].X = right - halfBodyWidth;
			controlPoints[i].Y = -halfBodyHeight;
			++i;
			controlPoints[i].X = right;
			controlPoints[i].Y = 0;
			++i;
			controlPoints[i].X = right - halfBodyWidth;
			controlPoints[i].Y = halfBodyHeight;
			++i;
			controlPoints[i].X = right - BodyWidth;
			controlPoints[i].Y = bottom;
			++i;
			controlPoints[i].X = 0;
			controlPoints[i].Y = 0;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			if (pointId == ArrowTipControlPoint || pointId == BodyEndControlPoint) {
				bool result = true;
				int dx = 0, dy = 0;
				int width = Width;
				int angle = Angle;
				Point tipPt = GetControlPointPosition(ArrowTipControlPoint);
				Point endPt = GetControlPointPosition(BodyEndControlPoint);

				if (pointId == ArrowTipControlPoint)
					result = Geometry.MoveArrowPoint(Center, tipPt, endPt, angle, headWidth, 0.5f, deltaX, deltaY, modifiers, out dx, out dy, out width, out angle);
				else
					result = Geometry.MoveArrowPoint(Center, endPt, tipPt, angle, headWidth, 0.5f, deltaX, deltaY, modifiers, out dx, out dy, out width, out angle);

				Width = width;
				Angle = angle;
				MoveByCore(dx, dy);
				ControlPointsHaveMoved();
				return result;
			} else return base.MovePointByCore(pointId, deltaX, deltaY, modifiers);
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int width = Width;
			int height = Height;
			switch (pointId) {
				case ArrowTopControlPoint:
				case ArrowBottomControlPoint:
					if (pointId == ArrowTopControlPoint) {
						if (!Geometry.MoveRectangleTop(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out height))
							result = false;
					} else {
						if (!Geometry.MoveRectangleBottom(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out height))
							result = false;
					}
					int newHeadWidth = HeadWidth + (int)Math.Round(transformedDeltaX);
					if (newHeadWidth < 0) {
						newHeadWidth = 0;
						result = false;
					} else if (newHeadWidth > Width) {
						newHeadWidth = Width;
						result = false;
					}
					HeadWidth = newHeadWidth;
					break;
				case BodyTopControlPoint:
				case BodyBottomControlPoint:
					int newBodyHeight = 0;
					if (pointId == BodyTopControlPoint)
						newBodyHeight = (int)Math.Round(BodyHeight - (transformedDeltaY * 2));
					else
						newBodyHeight = (int)Math.Round(BodyHeight + (transformedDeltaY * 2));
					if (newBodyHeight > Height) {
						newBodyHeight = Height;
						result = false;
					} else if (newBodyHeight < 0) {
						newBodyHeight = 0;
						result = false;
					}
					BodyHeight = newBodyHeight;
					break;
				default:
					return base.MovePointByCore(pointId, transformedDeltaX, transformedDeltaY, sin, cos, modifiers);
			}
			if (width < headWidth) {
				width = headWidth;
				result = false;
			}
			Width = width;
			Height = height;
			MoveByCore(dx, dy);
			ControlPointsHaveMoved();
			return result;
		}


		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdBodyHeight:
					BodyHeight = propertyMapping.GetInteger();
					break;
				case PropertyIdHeadWidth:
					HeadWidth = propertyMapping.GetInteger();
					break;
				default:
					base.ProcessExecModelPropertyChange(propertyMapping);
					break;
			}
		}
		
		
		private void CalcShapePoints() {
			int left = -(int)Math.Round(Width / 2f);
			int right = left + Width;
			int top = -(int)Math.Round(Height / 2f);
			int bottom = top + Height;
			int halfBodyHeight = (int)Math.Round(BodyHeight / 2f);

			// head tip
			shapePoints[0].X = left;
			shapePoints[0].Y = 0;

			// head side tip (top)
			shapePoints[1].X = right - BodyWidth;
			shapePoints[1].Y = top;

			// head / body connection point
			shapePoints[2].X = right - BodyWidth;
			shapePoints[2].Y = -halfBodyHeight;

			// body corner (top)
			shapePoints[3].X = right;
			shapePoints[3].Y = -halfBodyHeight;

			// body corner (bottom)
			shapePoints[4].X = right;
			shapePoints[4].Y = halfBodyHeight;

			// head / body connection point
			shapePoints[5].X = right - BodyWidth;
			shapePoints[5].Y = halfBodyHeight;

			// head side tip (bottom)
			shapePoints[6].X = right - BodyWidth;
			shapePoints[6].Y = bottom;
		}


		#region Fields

		protected const int PropertyIdBodyHeight = 9;
		protected const int PropertyIdHeadWidth = 10;

		private Point newTipPos = Point.Empty;
		private Point oldTipPos = Point.Empty;

		private const int ArrowTipControlPoint = 1;
		private const int ArrowTopControlPoint = 2;
		private const int ArrowBottomControlPoint = 3;
		private const int BodyTopControlPoint = 4;
		private const int BodyBottomControlPoint = 5;
		private const int BodyEndControlPoint = 6;
		private const int RotateControlPoint = 7;

		private Point[] shapePoints = new Point[7];
		private int headWidth;
		private double bodyHeightRatio;
		#endregion
	}


	public class Picture : PictureBase {
		
		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Picture(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Picture(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Picture(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			Construct();
		}


		protected internal Picture(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			Construct();
		}


		private void Construct() {
			//Image = new NamedImage();
		}

	}


	// FreeTriangle as base for the FreePolygon
	public class FreeTriangle : CaptionedShapeBase {

		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			shapePoints[0].X = 0;
			shapePoints[0].Y = -20;
			shapePoints[1].X = -20;
			shapePoints[1].Y = 20;
			shapePoints[2].X = 20;
			shapePoints[2].Y = 20;
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is FreeTriangle) {
				FreeTriangle src = (FreeTriangle)source;
				this.shapePoints[0] = src.shapePoints[0];
				this.shapePoints[1] = src.shapePoints[1];
				this.shapePoints[2] = src.shapePoints[2];
			}
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new FreeTriangle(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			return base.GetControlPointIds(controlPointCapability);
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPoint1:
				case ControlPoint2:
				case ControlPoint3:
					return ((controlPointCapability & ControlPointCapabilities.Resize) > 0
								|| ((controlPointCapability & ControlPointCapabilities.Connect) > 0 && IsConnectionPointEnabled(controlPointId)));
				case RotateControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Rotate) > 0
								|| (controlPointCapability & ControlPointCapabilities.Reference) > 0
								|| ((controlPointCapability & ControlPointCapabilities.Connect) > 0 && IsConnectionPointEnabled(controlPointId)));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			switch (controlPointId) {
				case ControlPoint1:
				case ControlPoint2:
				case ControlPoint3:
					int ptIdx = GetControlPointIndex(controlPointId);
					return controlPoints[ptIdx];
				case RotateControlPoint:
					return Center;
				default:
					return base.GetControlPointPosition(controlPointId);
			}
		}


		/// <override></override>
		public override RelativePosition CalculateRelativePosition(int x, int y) {
			throw new NotImplementedException();
		}


		/// <override></override>
		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			throw new NotImplementedException();
		}


		/// <override></override>
		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			ControlPointId result = base.HitTest(x, y, controlPointCapability, range);
			if (result != ControlPointId.None)
				return result;

			UpdateDrawCache();
			for (int i = 0; i < 3; ++i) {
				int j = i < 2 ? i + 1 : 0;
				int x1 = ControlPoints[i].X + X;
				int y1 = ControlPoints[i].Y + Y;
				if (Geometry.DistancePointPoint(x, y, x1, y1) <= range)
					return i + 1;
			}
			if ((controlPointCapability & ControlPointCapabilities.Rotate) > 0)
				if (Geometry.DistancePointPoint(X, Y, x, y) <= range)
					return RotateControlPoint;
			return ControlPointId.None;
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 4; }
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			DrawPath(graphics, LineStyle, FillStyle);
			DrawCaption(graphics);
			base.Draw(graphics);	// draw children
		}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			// 
		}


		protected internal FreeTriangle(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FreeTriangle(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override int DivFactorX {
			get { return 1; }
		}


		/// <override></override>
		protected override int DivFactorY {
			get { return 1; }
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result;
			Geometry.CalcBoundingRectangle(shapePoints, out result);
			result.Offset(X, Y);
			if (X < result.X) {
				result.Width = result.Right - X;
				result.X = X;
			} else if (result.Right < X)
				result.Width = X - result.X;
			if (Y < result.Y) {
				result.Height = result.Bottom - Y;
				result.Y = Y;
			} else if (result.Bottom < Y)
				result.Height = Y - result.Y;
			return result;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			// transform x|y to 0|0 before comparing with (the untransformed) shapePoints
			return Geometry.TriangleContainsPoint(shapePoints[0], shapePoints[1], shapePoints[2], x - X, y - Y);
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			// transform the rectangle 0|0 before comparing it with the (untransformed) shapePoints
			Rectangle r = Rectangle.Empty;
			r.X = x - X;
			r.Y = y - Y;
			r.Width = width;
			r.Height = height;
			return Geometry.PolygonIntersectsWithRectangle(shapePoints, r);
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			int idx = GetControlPointIndex(pointId);
			Debug.Assert(idx >= 0);

			Invalidate();
			shapePoints[idx].X += (int)Math.Round((transformedDeltaX * cos) - (transformedDeltaY * sin));
			shapePoints[idx].Y += (int)Math.Round((transformedDeltaX * sin) + (transformedDeltaY * cos));
			InvalidateDrawCache();
			Invalidate();

			return true;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			Geometry.CalcBoundingRectangle(shapePoints, out captionBounds);
			if (ParagraphStyle != null) {
				captionBounds = Rectangle.Empty;
				captionBounds.X += ParagraphStyle.Padding.Left;
				captionBounds.Y += ParagraphStyle.Padding.Top;
				captionBounds.Width -= ParagraphStyle.Padding.Horizontal;
				captionBounds.Height -= ParagraphStyle.Padding.Vertical;
			}
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int cnt = shapePoints.Length;
			for (int i = 0; i < cnt; ++i) {
				controlPoints[i].X = shapePoints[i].X;
				controlPoints[i].Y = shapePoints[i].Y;
			}
			controlPoints[ControlPointCount - 1].X = 0;
			controlPoints[ControlPointCount - 1].Y = 0;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
		}


		#region Fields
		protected const int ControlPoint1 = 1;
		protected const int ControlPoint2 = 2;
		protected const int ControlPoint3 = 3;
		protected const int RotateControlPoint = 4;

		private Point[] shapePoints = new Point[3];	// the Vertices contain the untransformed relative position to X/Y
		#endregion
	}


	public static class NShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			if (registrar == null) throw new ArgumentNullException("registrar");

			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			// Linear GeneralShapes
			registrar.RegisterShapeType(new ShapeType("Polyline", namespaceName, namespaceName,
				Polyline.CreateInstance, Polyline.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("CircularArc", namespaceName, namespaceName,
				"With only two points, it behaves like a straight line, with all three points, it behaves like a circular arc.",
				CircularArc.CreateInstance, CircularArc.GetPropertyDefinitions));
			// new Type(typeof(RectangularLine).Name, RectangularLine.CreateInstance, 
			// RectangularLine.GetPropertyDefinitions(null)).Register(registrar);
			// new Type(typeof(BezierLine).Name, BezierLine.CreateInstance, 
			// BezierLine.GetPropertyDefinitions(null)).Register(registrar);
			// Planar GeneralShapes
			registrar.RegisterShapeType(new ShapeType("Text", namespaceName, namespaceName,
				"Supports automatic sizing to its text.",
				Text.CreateInstance, Text.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Label", namespaceName, namespaceName,
				"Supports autosizing to its text and connecting to other shapes. If the label's 'pin' is connected to a shape, the label will move with its partner shape.",
				Label.CreateInstance, Label.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceQuadrangle));
			// new Type("Triangle", lib, namespaceName, Triangle.CreateInstance, 
			// Triangle.GetPropertyDefinitions(null), 
			// Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceTriangle).Register(registrar);
			registrar.RegisterShapeType(new ShapeType("IsoscelesTriangle", namespaceName, namespaceName,
				IsoscelesTriangle.CreateInstance, IsoscelesTriangle.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceTriangle));
			registrar.RegisterShapeType(new ShapeType("Ellipse", namespaceName, namespaceName,
				Ellipse.CreateInstance, Ellipse.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceCircle));
			registrar.RegisterShapeType(new ShapeType("Circle", namespaceName, namespaceName,
				Circle.CreateInstance, Circle.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceCircle));
			registrar.RegisterShapeType(new ShapeType("Box", namespaceName, namespaceName,
				Box.CreateInstance, Box.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Square", namespaceName, namespaceName,
				Square.CreateInstance, Square.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Diamond", namespaceName, namespaceName,
				Diamond.CreateInstance, Diamond.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceDiamond));
			registrar.RegisterShapeType(new ShapeType("RoundedBox", namespaceName, namespaceName,
				RoundedBox.CreateInstance, RoundedBox.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("ThickArrow", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return (Shape)new ThickArrow(shapeType, t); },
				ThickArrow.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Picture", namespaceName, namespaceName,
				Picture.CreateInstance, Picture.GetPropertyDefinitions,
				Dataweb.NShape.GeneralShapes.Properties.Resources.ShaperReferenceQuadrangle));
			// new Type("Free Triangle", lib.Name, namespaceName, 
			// delegate(Type shapeType, Template db) { return (Shape)new FreeTriangle(shapeType, db); }, 
			// FreeTriangle.GetPropertyDefinitions(null)).Register(registrar);
		}


		#region Fields

		private const string namespaceName = "GeneralShapes";

		private const int preferredRepositoryVersion = 3;

		#endregion
	}
}