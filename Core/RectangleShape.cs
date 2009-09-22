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
using System.ComponentModel;
using System.Drawing;


namespace Dataweb.NShape.Advanced {

	public abstract class RectangleBase : CaptionedShapeBase {

		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			size.Width = 60;
			size.Height = 40;
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is RectangleBase) {
				size.Width = ((RectangleBase)source).Width;
				size.Height = ((RectangleBase)source).Height;
			}
		}


		#region IPersistable Members

		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(Width);
			writer.WriteInt32(Height);
		}


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in CaptionedShapeBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Width", typeof(int));
			yield return new EntityFieldDefinition("Height", typeof(int));
		}

		#endregion


		#region Properties

		[Category("Layout")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("The size of the Shape.")]
		public virtual int Width {
			get { return size.Width; }
			set {
				if (value != size.Width) {
					Invalidate();
					if (Owner != null) Owner.NotifyChildResizing(this);
					int delta = value - size.Width;
					
					size.Width = value;
					ControlPointsHaveMoved();
					InvalidateDrawCache();

					if (ChildrenCollection != null) ChildrenCollection.NotifyParentSized(delta, 0);
					if (Owner != null) Owner.NotifyChildResized(this);
					Invalidate();
				}
			}
		}


		[Category("Layout")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("The horizontal size of the Shape.")]
		public virtual int Height {
			get { return size.Height; }
			set {
				if (value != size.Height) {
					Invalidate();
					if (Owner != null) Owner.NotifyChildResizing(this);
					int delta = value - size.Height;

					size.Height = value;
					ControlPointsHaveMoved();
					InvalidateDrawCache();

					if (ChildrenCollection != null) ChildrenCollection.NotifyParentSized(0, delta);
					if (Owner != null) Owner.NotifyChildResized(this);
					Invalidate();
				}
			}
		}


		[Browsable(false)]
		protected internal override int ControlPointCount {
			get { return 9; }
		}

		#endregion


		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			// The RelativePosition of a RectangleBased shape is:
			// A = Tenths of percent of Width
			// B = Tenths of percent of Height
			Point result = Point.Empty;
			result.X = (int)Math.Round((X - (Width / 2f)) + (relativePosition.A * (Width / 1000f)));
			result.Y = (int)Math.Round((Y - (Height / 2f)) + (relativePosition.B * (Height / 1000f)));
			if (Angle != 0) result = Geometry.RotatePoint(Center, Geometry.TenthsOfDegreeToDegrees(Angle), result);
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
			result.A = (int)Math.Round((x - (X - (Width / 2f))) / (Width / 1000f));
			result.B = (int)Math.Round((y - (Y - (Height / 2f))) / (Height / 1000f));
			return result;
		}


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


		public override Point CalculateConnectionFoot(int startX, int startY) {
			// Set result to a default return value
			Point result = Point.Empty;
			result.Offset(X, Y);
			
			float currDist, dist = float.MaxValue;
			float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
			int x1, y1, x2, y2;
			Point p;

			int left = (int)Math.Round(X - (Width / 2f));
			int top = (int)Math.Round(Y - (Height / 2f));
			int right = left + Width;
			int bottom = top + Height;
			// Check top side for intersection
			x1 = left; y1 = top;
			x2 = right; y2 = top;
			Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
			Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
			p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
			if (p != Geometry.InvalidPoint) {
				currDist = Geometry.DistancePointPoint(p.X, p.Y, startX, startY);
				if (currDist < dist) {
					dist = currDist;
					result = p;
				}
			}
			// check right side for intersection
			x1 = right; y1 = top;
			x2 = right; y2 = bottom;
			Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
			Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
			p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
			if (p != Geometry.InvalidPoint) {
				currDist = Geometry.DistancePointPoint(p.X, p.Y, startX, startY);
				if (currDist < dist) {
					dist = currDist;
					result = p;
				}
			}
			// check bottom side for intersection
			x1 = right; y1 = bottom;
			x2 = left; y2 = bottom;
			Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
			Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
			p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
			if (p != Geometry.InvalidPoint) {
				currDist = Geometry.DistancePointPoint(p.X, p.Y, startX, startY);
				if (currDist < dist) {
					dist = currDist;
					result = p;
				}
			}
			// check left side for intersection
			x1 = left; y1 = bottom;
			x2 = left; y2 = top;
			Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
			Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
			p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
			if (p != Geometry.InvalidPoint) {
				currDist = Geometry.DistancePointPoint(p.X, p.Y, startX, startY);
				if (currDist < dist) {
					dist = currDist;
					result = p;
				}
			}
			return result;
		}


		public override void Fit(int x, int y, int width, int height) {
			float scale = Geometry.CalcScaleFactor(Width, Height, width, height);
			int w = (int)Math.Floor(Width * scale);
			int h = (int)Math.Floor(Height * scale);
			this.Width = w - w % 2;
			this.Height = h - h % 2;
			MoveControlPointTo(ControlPointId.Reference, x + (int)Math.Round(width / 2f), y + (int)Math.Round(height / 2f), ResizeModifiers.None);
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// Tight and loose bounding rectangles are equal
			Rectangle result = Geometry.InvalidRectangle;
			if (Width >= 0 && Height >= 0) {
				if (Angle == 0 || Angle == 1800) {
					result.X = X - (int)Math.Round(Width / 2f);
					result.Y = Y - (int)Math.Round(Height / 2f);
					result.Width = Width;
					result.Height = Height;
				} else if (Angle == 900 || Angle == 2700) {
					result.X = X - (int)Math.Round(Height / 2f);
					result.Y = Y - (int)Math.Round(Width / 2f);
					result.Width = Height;
					result.Height = Width;
				} else {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					int x1, y1, x2, y2, x3, y3, x4, y4;
					int left = (int)Math.Round(X - (Width / 2f));
					int top = (int)Math.Round(Y - (Height / 2f));
					int right = left + Width;
					int bottom = top + Height;

					x1 = left; y1 = top;
					x2 = right; y2 = top;
					x3 = right; y3 = bottom;
					x4 = left; y4 = bottom;
					Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
					Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
					Geometry.RotatePoint(X, Y, angleDeg, ref x3, ref y3);
					Geometry.RotatePoint(X, Y, angleDeg, ref x4, ref y4);

					result.X = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
					result.Y = Math.Min(Math.Min(y1, y2), Math.Min(y3, y4));
					result.Width = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4)) - result.X;
					result.Height = Math.Max(Math.Max(y1, y2), Math.Max(y3, y4)) - result.Y;
				}
			}
			return result;
		}


		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			DrawPath(graphics, LineStyle, FillStyle);
			DrawCaption(graphics);
			base.Draw(graphics);
		}


		protected internal RectangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal RectangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override int DivFactorX { get { return 2; } }


		protected override int DivFactorY { get { return 2; } }


		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.RectangleContainsPoint(X - (Width / 2), Y - (Height / 2), Width, Height, Geometry.TenthsOfDegreeToDegrees(Angle), x, y, true);
		}
		
		
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			if (Angle % 900 == 0) {
				Rectangle r = GetBoundingRectangle(true);
				if (Geometry.RectangleIntersectsWithRectangle(r, x, y, width, height)
					|| Geometry.RectangleContainsRectangle(r, x, y, width, height))
					return true;
			} else {
				if (rotatedBounds.Length != 4)
					Array.Resize<PointF>(ref rotatedBounds, 4);
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				float ptX, ptY;
				ptX = X - (Width / 2f);		// left
				ptY = Y - (Height / 2f);	// top
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[0].X = ptX;
				rotatedBounds[0].Y = ptY;

				ptX = X + (Width / 2f);		// right
				ptY = Y - (Height / 2f);	// top
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[1].X = ptX;
				rotatedBounds[1].Y = ptY;

				ptX = X + (Width / 2f);		// right
				ptY = Y + (Height / 2f);	// bottom
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[2].X = ptX;
				rotatedBounds[2].Y = ptY;

				ptX = X - (Width / 2f);		// left
				ptY = Y + (Height / 2f);	// bottom
				Geometry.RotatePoint(X, Y, angle, ref ptX, ref ptY);
				rotatedBounds[3].X = ptX;
				rotatedBounds[3].Y = ptY;

				Rectangle rectangle = Rectangle.Empty;
				rectangle.Offset(x, y);
				rectangle.Width = width;
				rectangle.Height = height;
				if (Geometry.PolygonIntersectsWithRectangle(rotatedBounds, rectangle))
					return true;
			}
			return false;
		}


		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int newWidth = Width;
			int newHeight = Height;
			switch (pointId) {
				case TopLeftControlPoint:
					if (!Geometry.MoveRectangleTopLeft(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newWidth, out newHeight))
						result = false;
					break;
				case TopCenterControlPoint:
					if (!Geometry.MoveRectangleTop(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newHeight))
						result = false;
					break;
				case TopRightControlPoint:
					if (!Geometry.MoveRectangleTopRight(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newWidth, out newHeight))
						result = false;
					break;
				case MiddleLeftControlPoint:
					if (!Geometry.MoveRectangleLeft(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newWidth))
						result = false;
					break;
				case MiddleRightControlPoint:
					if (!Geometry.MoveRectangleRight(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newWidth))
						result = false;
					break;
				case BottomLeftControlPoint:
					if (!Geometry.MoveRectangleBottomLeft(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newWidth, out newHeight))
						result = false;
					break;
				case BottomCenterControlPoint:
					if (!Geometry.MoveRectangleBottom(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newHeight))
						result = false;
					break;
				case BottomRightControlPoint:
					if (!Geometry.MoveRectangleBottomRight(newWidth, newHeight, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out newWidth, out newHeight))
						result = false;
					break;
				default: throw new IndexOutOfRangeException();
			}
			// Perform Resizing
			size.Width = newWidth;
			size.Height = newHeight;
			MoveByCore(dx, dy);
			//ControlPointsHaveMoved();
			//SignalControlPointsHaveMoved(connectionPointId, modifiers);

			return result;
		}


		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

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


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new IndexOutOfRangeException();
			captionBounds = Rectangle.Empty;
			captionBounds.X = (int)Math.Round(-Width / 2f);
			captionBounds.Y = (int)Math.Round(-Height / 2f);
			captionBounds.Width = Width;
			captionBounds.Height = Height;
			//if (ParagraphStyle != null) {
			//   captionBounds.X += ParagraphStyle.Padding.Left;
			//   captionBounds.Y += ParagraphStyle.Padding.Top;
			//   captionBounds.Width -= ParagraphStyle.Padding.Horizontal;
			//   captionBounds.Height -= ParagraphStyle.Padding.Vertical;
			//   return true;
			//} else return false;
		}


		//protected virtual PointF CalcOutLineIntersection(float startX, float startY) {
		//   PointF result = PointF.Empty;
		//   result.X = X; result.Y = Y;
		//   float currDist, dist = float.MaxValue;
		//   float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
		//   int x1, y1, x2, y2;
		//   PointF? p;

		//   x1 = left; y1 = top;
		//   x2 = right; y2 = top;
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
		//   p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
		//   if (p.HasValue) {
		//      currDist = Geometry.DistancePointPoint(p.Value.X, p.Value.Y, startX, startY);
		//      if (currDist < dist) {
		//         dist = currDist;
		//         result = p.Value;
		//      }
		//   }

		//   x1 = right; y1 = top;
		//   x2 = right; y2 = bottom;
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
		//   p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
		//   if (p.HasValue) {
		//      currDist = Geometry.DistancePointPoint(p.Value.X, p.Value.Y, startX, startY);
		//      if (currDist < dist) {
		//         dist = currDist;
		//         result = p.Value;
		//      }
		//   }

		//   x1 = right; y1 = bottom;
		//   x2 = left; y2 = bottom;
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
		//   p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
		//   if (p.HasValue) {
		//      currDist = Geometry.DistancePointPoint(p.Value.X, p.Value.Y, startX, startY);
		//      if (currDist < dist) {
		//         dist = currDist;
		//         result = p.Value;
		//      }
		//   }

		//   x1 = left; y1 = bottom;
		//   x2 = left; y2 = top;
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
		//   Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
		//   p = Geometry.IntersectLineWithLineSegment(startX, startY, X, Y, x1, y1, x2, y2);
		//   if (p.HasValue) {
		//      currDist = Geometry.DistancePointPoint(p.Value.X, p.Value.Y, startX, startY);
		//      if (currDist < dist) {
		//         dist = currDist;
		//         result = p.Value;
		//      }
		//   }

		//   return result;
		//}


		///// <summary>
		///// Calls ControlPointHasMoved for the appropriate ControlPoints, depending on the given ControlPoint and ResizeModifiers
		///// </summary>
		//protected void SignalControlPointsHaveMoved(ControlPointId pointId, ResizeModifiers modifiers) {
		//   int ctrlPtId = pointId;
		//   switch (modifiers) {
		//      #region Handle movement on Shift
		//      case ResizeModifiers.MirroredResize:
		//         switch (pointId) {
		//            case TopLeftControlPoint:
		//            case TopRightControlPoint:
		//            case BottomLeftControlPoint:
		//            case BottomRightControlPoint:
		//               // move all but BalancePointControlPoint
		//               AllControlPointsHaveMovedExcept(MiddleCenterControlPoint);
		//               break;
		//            case TopCenterControlPoint:
		//            case BottomCenterControlPoint:
		//               // move top and bottom handle row
		//               ControlPointHasMoved(TopLeftControlPoint);
		//               ControlPointHasMoved(TopCenterControlPoint);
		//               ControlPointHasMoved(TopRightControlPoint);
		//               ControlPointHasMoved(BottomLeftControlPoint);
		//               ControlPointHasMoved(BottomCenterControlPoint);
		//               ControlPointHasMoved(BottomRightControlPoint);
		//               break;
		//            case MiddleLeftControlPoint:
		//            case MiddleRightControlPoint:
		//               // move left side and right side handles
		//               ControlPointHasMoved(TopLeftControlPoint);
		//               ControlPointHasMoved(MiddleLeftControlPoint);
		//               ControlPointHasMoved(BottomLeftControlPoint);
		//               ControlPointHasMoved(TopRightControlPoint);
		//               ControlPointHasMoved(MiddleRightControlPoint);
		//               ControlPointHasMoved(BottomRightControlPoint);
		//               break;
		//         }
		//         break;
		//      #endregion

		//      #region Standard Handle Movement
		//      default:
		//         switch (pointId) {
		//            case TopLeftControlPoint:
		//               // move all but BottomRightControlPoint
		//               AllControlPointsHaveMovedExcept(BottomRightControlPoint);
		//               break;
		//            case TopCenterControlPoint:
		//               // move all but bottom row
		//               ControlPointHasMoved(TopLeftControlPoint);
		//               ControlPointHasMoved(TopCenterControlPoint);
		//               ControlPointHasMoved(TopRightControlPoint);
		//               ControlPointHasMoved(MiddleLeftControlPoint);
		//               ControlPointHasMoved(MiddleRightControlPoint);
		//               ControlPointHasMoved(MiddleCenterControlPoint);
		//               break;
		//            case TopRightControlPoint:
		//               // move all but BottomLeftControlPoint
		//               AllControlPointsHaveMovedExcept(BottomLeftControlPoint);
		//               break;
		//            case MiddleLeftControlPoint:
		//               // Move all but right row
		//               ControlPointHasMoved(TopLeftControlPoint);
		//               ControlPointHasMoved(TopCenterControlPoint);
		//               ControlPointHasMoved(MiddleLeftControlPoint);
		//               ControlPointHasMoved(BottomLeftControlPoint);
		//               ControlPointHasMoved(BottomCenterControlPoint);
		//               ControlPointHasMoved(MiddleCenterControlPoint);
		//               break;
		//            case MiddleRightControlPoint:
		//               // move all but left row
		//               ControlPointHasMoved(TopCenterControlPoint);
		//               ControlPointHasMoved(TopRightControlPoint);
		//               ControlPointHasMoved(MiddleRightControlPoint);
		//               ControlPointHasMoved(BottomCenterControlPoint);
		//               ControlPointHasMoved(BottomRightControlPoint);
		//               ControlPointHasMoved(MiddleCenterControlPoint);
		//               break;
		//            case BottomLeftControlPoint:
		//               // move all but TopRightControlPoint
		//               AllControlPointsHaveMovedExcept(TopRightControlPoint);
		//               break;
		//            case BottomCenterControlPoint:
		//               // move all but top row
		//               ControlPointHasMoved(MiddleLeftControlPoint);
		//               ControlPointHasMoved(MiddleRightControlPoint);
		//               ControlPointHasMoved(BottomLeftControlPoint);
		//               ControlPointHasMoved(BottomCenterControlPoint);
		//               ControlPointHasMoved(BottomRightControlPoint);
		//               ControlPointHasMoved(MiddleCenterControlPoint);
		//               break;
		//            case BottomRightControlPoint:
		//               // move all but TopLeftControlPoint
		//               AllControlPointsHaveMovedExcept(TopLeftControlPoint);
		//               break;
		//         }
		//         break;
		//      #endregion
		//   }
		//}


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

		private Size size = Size.Empty;
		private PointF[] rotatedBounds = new PointF[4];

		#endregion
	}


	public abstract class EllipseBase : RectangleBase {

		protected internal override int ControlPointCount { get { return 13; } }


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
			Point result = Geometry.GetNearestPoint(startX, startY, Geometry.IntersectEllipseLine(X, Y, Width, Height, Geometry.TenthsOfDegreeToDegrees(Angle), startX, startY, X, Y, false));
			if (result == Geometry.InvalidPoint) return Center;
			else return result;
		}


		protected internal EllipseBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal EllipseBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.Offset(x, y);
			rectangle.Width = width;
			rectangle.Height = height;
			Rectangle boundingRect = GetBoundingRectangle(false);
			if (rectangle.Contains(boundingRect) || boundingRect.Contains(rectangle) || boundingRect.IntersectsWith(rectangle))
				return Geometry.EllipseIntersectsWithRectangle(X, Y, Width, Height, Geometry.TenthsOfDegreeToDegrees(Angle), rectangle);
			else return false;
		}


		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.EllipseContainsPoint(X, Y, Width, Height, Geometry.TenthsOfDegreeToDegrees(Angle), x, y);
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle result = Rectangle.Empty;
				if (Width >= 0 && Height >= 0) {
					if (Angle % 1800 == 0) {
						result.X = X - (Width / 2);
						result.Y = Y - (Height / 2);
						result.Width = Width;
						result.Height = Height;
					} else if (Angle % 900 == 0) {
						result.X = X - (Height / 2);
						result.Y = Y - (Width / 2);
						result.Width = Height;
						result.Height = Width;
					} else {
						// a is the major half axis
						// b is the minor half axis
						// phi is the ratation angle of the ellipse
						// t1/t2 are the angles where to find the maxima:
						// The formulas how to calculate the maxima:
						//	   x = centerX + a * cos(t) * cos(phi) - b * sin(t) * sin(phi)  [1]
						//	   y = centerY + b * sin(t) * cos(phi) + a * cos(t) * sin(phi)  [2]
						// The formula how to calculate the angle t:
						//    tan(t) = -b * tan(phi) / a   [3]
						//    tan(t) = b * cot(phi) / a  [4]
						float a = Width / 2f;
						float b = Height / 2f;
						float phi = Geometry.TenthsOfDegreeToRadians(Angle);
						double tanPhi = Math.Tan(phi);
						double sinPhi = Math.Sin(phi);
						double cosPhi = Math.Cos(phi);
						float t1 = (float)Math.Round(Math.Atan(-b * tanPhi / a), 7, MidpointRounding.ToEven);
						float t2 = (float)Math.Round(Math.Atan(b * (1 / tanPhi) / a), 7, MidpointRounding.ToEven);
						double sinT1 = Math.Sin(t1);
						double cosT1 = Math.Cos(t1);
						double sinT2 = Math.Sin(t2);
						double cosT2 = Math.Cos(t2);

						float x1 = (float)Math.Abs(a * cosT1 * cosPhi - b * sinT1 * sinPhi);
						float x2 = (float)Math.Abs(a * cosT2 * cosPhi - b * sinT2 * sinPhi);
						float y1 = (float)Math.Abs(b * sinT1 * cosPhi + a * cosT1 * sinPhi);
						float y2 = (float)Math.Abs(b * sinT2 * cosPhi + a * cosT2 * sinPhi);

						result.X = (int)Math.Floor(X - Math.Max(x1, x2));
						result.Y = (int)Math.Floor(Y - Math.Max(y1, y2));
						result.Width = (int)Math.Ceiling(X + Math.Max(x1, x2)) - result.X;
						result.Height = (int)Math.Ceiling(Y + Math.Max(y1, y2)) - result.Y;
					}
				}
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected override void CalcControlPoints() {
			double angle = Geometry.DegreesToRadians(45);
			int dx = (int)Math.Round((Width / 2f) - ((Width / 2f) * Math.Cos(angle)));
			int dy = (int)Math.Round((Height / 2f) - ((Height / 2f) * Math.Sin(angle)));
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;
			// top left
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			// top
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			// top right
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;
			// left
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			// right
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;
			// bottom left
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			// bottom
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			// bottom right
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;

			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;

			// top left
			ControlPoints[9].X = left + dx;
			ControlPoints[9].Y = top + dy;
			// top right
			ControlPoints[10].X = right - dx;
			ControlPoints[10].Y = top + dy;
			// bottom left
			ControlPoints[11].X = left + dx;
			ControlPoints[11].Y = bottom - dy;
			// bottom right
			ControlPoints[12].X = right - dx;
			ControlPoints[12].Y = bottom - dy;
		}


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new IndexOutOfRangeException();
			captionBounds = Rectangle.Empty;
			captionBounds.X = (int)Math.Round((-Width / 2f) + (Width / 8f));
			captionBounds.Y = (int)Math.Round((-Height / 2f) + (Height / 8f));
			captionBounds.Width = (int)Math.Round(Width - (Width / 4f));
			captionBounds.Height = (int)Math.Round(Height - (Height / 4f));
		}


		#region Fields

		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopRightControlPoint = 3;
		private const int BottomLeftControlPoint = 6;
		private const int BottomRightControlPoint = 8;

		#endregion
	}


	public abstract class DiamondBase : RectangleBase {

		public override Point CalculateConnectionFoot(int startX, int startY) {
			Point result = Point.Empty;
			// Calculate current (unrotated) position of the shape's points
			int left = (int)Math.Round(X - (Width / 2f));
			int top = (int)Math.Round(Y - (Height / 2f));
			int right = left + Width;
			int bottom = top + Height;
			pointBuffer[0].X = X;
			pointBuffer[0].Y = top;
			pointBuffer[1].X = right;
			pointBuffer[1].Y = Y;
			pointBuffer[2].X = X;
			pointBuffer[2].Y = bottom;
			pointBuffer[3].X = left;
			pointBuffer[3].Y = Y;
			// Rotate shape points if necessary
			if (Angle % 1800 != 0) {
				Matrix.Reset();
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center);
				Matrix.TransformPoints(pointBuffer);
			}
			// Calculate intersection points and return the nearest (or the shape's Center if there is no intersection point)
			result = Geometry.GetNearestPoint(startX, startY, Geometry.IntersectPolygonLine(pointBuffer, startX, startY, X, Y, true));
			if (result == Geometry.InvalidPoint) return Center;
			else return result;
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


		protected internal override int ControlPointCount {
			get { return 13; }
		}


		protected internal DiamondBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DiamondBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle bounds = Rectangle.Empty;
				if (Width >= 0 && Height >= 0) {
					CalcTransformedShapePoints();
					bounds.X = Math.Min(Math.Min(pointBuffer[0].X, pointBuffer[1].X), Math.Min(pointBuffer[2].X, pointBuffer[3].X));
					bounds.Y = Math.Min(Math.Min(pointBuffer[0].Y, pointBuffer[1].Y), Math.Min(pointBuffer[2].Y, pointBuffer[3].Y));
					bounds.Width = Math.Max(Math.Max(pointBuffer[0].X, pointBuffer[1].X), Math.Max(pointBuffer[2].X, pointBuffer[3].X)) - bounds.X;
					bounds.Height = Math.Max(Math.Max(pointBuffer[0].Y, pointBuffer[1].Y), Math.Max(pointBuffer[2].Y, pointBuffer[3].Y)) - bounds.Y;
				}
				return bounds;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.X = x;
			rectangle.Y = y;
			rectangle.Width = width;
			rectangle.Height = height;

			CalcTransformedShapePoints();
			return Geometry.PolygonIntersectsWithRectangle(pointBuffer, rectangle);
		}


		protected override bool ContainsPointCore(int x, int y) {
			CalcTransformedShapePoints();
			return Geometry.QuadrangleContainsPoint(x, y, pointBuffer[0], pointBuffer[1], pointBuffer[2], pointBuffer[3]);
		}


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new IndexOutOfRangeException();
			float insetX = Width / 8f;
			float insetY = Height / 8f;
			captionBounds = Rectangle.Empty;
			captionBounds.X = (int)Math.Round((-Width / 2f) + insetX);
			captionBounds.Y = (int)Math.Round((-Height / 2f) + insetY);
			captionBounds.Width = (int)Math.Round(Width - insetX - insetX);
			captionBounds.Height = (int)Math.Round(Height - insetY - insetY);
		}


		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			// top left
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			// top
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			// top right
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;
			// left
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			// right
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;
			// bottom left
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			// bottom
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			// bottom right
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;
			// rotate
			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;

			if (ControlPoints.Length > 9) {
				int dX = (int)Math.Round(Width / 4f);
				int dY = (int)Math.Round(Height / 4f);

				// top left side
				ControlPoints[9].X = left + dX;
				ControlPoints[9].Y = top + dY;

				// top right side
				ControlPoints[10].X = right - dX;
				ControlPoints[10].Y = top + dY;

				// bottom left side
				ControlPoints[11].X = left + dX;
				ControlPoints[11].Y = bottom - dY;

				// bottom right side
				ControlPoints[12].X = right - dX;
				ControlPoints[12].Y = bottom - dY;
			}
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				pointBuffer[0].X = 0;
				pointBuffer[0].Y = top;
				pointBuffer[1].X = right;
				pointBuffer[1].Y = 0;
				pointBuffer[2].X = 0;
				pointBuffer[2].Y = bottom;
				pointBuffer[3].X = left;
				pointBuffer[3].Y = 0;

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(pointBuffer);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		private void CalcTransformedShapePoints() {
			int left = (int)Math.Round(X - (Width / 2f));
			int top = (int)Math.Round(Y - (Height / 2f));
			int right = left + Width;
			int bottom = top + Height;
			pointBuffer[0].X = X;
			pointBuffer[0].Y = top;
			pointBuffer[1].X = right;
			pointBuffer[1].Y = Y;
			pointBuffer[2].X = X;
			pointBuffer[2].Y = bottom;
			pointBuffer[3].X = left;
			pointBuffer[3].Y = Y;
			if (Angle != 0) {
				Matrix.Reset();
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center);
				Matrix.TransformPoints(pointBuffer);
			}
		}


		#region Fields

		protected Point[] pointBuffer = new Point[4];

		// ControlPoint Id Constants
		private const int TopLeftControlPoint = 1;
		private const int TopRightControlPoint = 3;
		private const int BottomLeftControlPoint = 6;
		private const int BottomRightControlPoint = 8;

		#endregion
	}


	public abstract class IsoscelesTriangleBase : RectangleBase {

		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 40;
			Height = 40;
		}


		protected internal override int ControlPointCount {
			get { return 5; }
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointId.Reference:
				case RotatePointControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
								|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0
								|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
										&& IsConnectionPointEnabled(controlPointId)));
				default:
					if (controlPointId > 0 && controlPointId < RotatePointControlPoint)
						return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
									|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
											&& IsConnectionPointEnabled(controlPointId)));
					else
						return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		public override Point CalculateConnectionFoot(int startX, int startY) {
			Rectangle boundingRect = GetBoundingRectangle(true);
			int x = boundingRect.X + (int)Math.Round(boundingRect.Width / 2f);
			int y = boundingRect.Y + (int)Math.Round(boundingRect.Height / 2f);
			Point result = Point.Empty;
			result.X = x;
			result.Y = y;

			int left = (int)Math.Round(X - (Width / 2f));
			int top = Y - (int)Math.Round(Height * CenterPosFactorY);
			int right = left + Width;
			int bottom = top + Height;

			shapePoints[0].X = X;
			shapePoints[0].Y = top;
			shapePoints[1].X = left;
			shapePoints[1].Y = bottom;
			shapePoints[2].X = right;
			shapePoints[2].Y = bottom;

			Matrix.Reset();
			Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center);
			Matrix.TransformPoints(shapePoints);

			result = Geometry.GetNearestPoint(startX, startY, Geometry.IntersectPolygonLine(shapePoints, startX, startY, x, y, true));
			if (result == Geometry.InvalidPoint) result = Center;
			return result;
		}


		public override void Fit(int x, int y, int width, int height) {
			float scale = Geometry.CalcScaleFactor(Width, Height, width, height);
			this.Width = (int)Math.Floor(Width * scale);
			this.Height = (int)Math.Floor(Height * scale);
			MoveControlPointTo(ControlPointId.Reference, x + (int)Math.Round(width * CenterPosFactorX), y + (int)Math.Round(height * CenterPosFactorY), ResizeModifiers.None);
		}


		protected internal IsoscelesTriangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal IsoscelesTriangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool ContainsPointCore(int x, int y) {
			int left = (int)Math.Round(X - (Width / 2f));
			int top = Y - (int)Math.Round(Height * CenterPosFactorY);
			int right = left + Width;
			int bottom = top + Height;
			shapePoints[0].X = X;
			shapePoints[0].Y = top;
			shapePoints[1].X = left;
			shapePoints[1].Y = bottom;
			shapePoints[2].X = right;
			shapePoints[2].Y = bottom;
			if (Angle != 0) {
				Matrix.Reset();
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center);
				Matrix.TransformPoints(shapePoints);
			}
			return Geometry.TriangleContainsPoint(x, y, shapePoints[0], shapePoints[1], shapePoints[2]);
		}


		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;

			int dx = 0, dy = 0;
			int width = Width;
			int height = Height;
			switch (pointId) {
				case TopCenterControlPoint:
					if (!Geometry.MoveRectangleTop(width, height, 0, centerPosFactorY, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out height))
						result = false;
					break;

				case BottomLeftControlPoint:
					if (!Geometry.MoveRectangleBottomLeft(width, height, 0, 0, centerPosFactorX, centerPosFactorY, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
						result = false;
					break;

				case BottomRightControlPoint:
					if (!Geometry.MoveRectangleBottomRight(width, height, 0, 0, centerPosFactorX, centerPosFactorY, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
						result = false;
					break;

				case BottomCenterControlPoint:
					if (!Geometry.MoveRectangleBottom(width, height, 0, centerPosFactorY, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out height))
						result = false;
					break;

				default:
					break;
			}
			Width = width;
			Height = height;
			MoveByCore(dx, dy);
			ControlPointsHaveMoved();

			return result;
		}


		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height * centerPosFactorY);
			int right = left + Width;
			int bottom = (int)Math.Round(Height - (Height * centerPosFactorY));

			ControlPoints[0].X = 0;
			ControlPoints[0].Y = top;

			ControlPoints[1].X = left;
			ControlPoints[1].Y = bottom;

			ControlPoints[2].X = 0;
			ControlPoints[2].Y = bottom;

			ControlPoints[3].X = right;
			ControlPoints[3].Y = bottom;

			ControlPoints[4].X = 0;
			ControlPoints[4].Y = 0;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// Tight and loose fitting bounding rectangles are equal
			Rectangle bounds = Geometry.InvalidRectangle;
			if (Width >= 0 && Height >= 0) {
				int left = (int)Math.Round(X - (Width / 2f));
				int top = Y - (int)Math.Round(Height * CenterPosFactorY);
				int right = left + Width;
				int bottom = top + Height;
				// If shape's angle is a multiple of 90°, the calculation of the bounding rectangle is easy (and fast)
				if (Angle == 0) {
					bounds.X = left;
					bounds.Y = top;
					bounds.Width = Width;
					bounds.Height = Height;
				} else if (Angle == 900) {
					bounds.X = X - (int)Math.Round(Height * (1 - CenterPosFactorY));
					bounds.Y = Y - (int)Math.Round(Width * (1 - CenterPosFactorX));
					bounds.Width = Height;
					bounds.Height = Width;
				} else if (Angle == 1800) {
					bounds.X = X - (int)Math.Round(Width * (1 - CenterPosFactorX)); ;
					bounds.Y = Y - (int)Math.Round(Height * (1 - CenterPosFactorY)); ;
					bounds.Width = Width;
					bounds.Height = Height;
				} else if (Angle == 2700) {
					bounds.X = X - (int)Math.Round(Height * CenterPosFactorY);
					bounds.Y = Y - (int)Math.Round(Width * CenterPosFactorX);
					bounds.Width = Height;
					bounds.Height = Width;
				} else {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					int x1, y1, x2, y2, x3, y3;
					x1 = X; y1 = top;
					x2 = left; y2 = bottom;
					x3 = right; y3 = bottom;
					Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
					Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
					Geometry.RotatePoint(X, Y, angleDeg, ref x3, ref y3);

					bounds.X = Math.Min(Math.Min(x1, x2), Math.Min(x1, x3));
					bounds.Y = Math.Min(Math.Min(y1, y2), Math.Min(y1, y3));
					bounds.Width = Math.Max(Math.Max(x1, x2), Math.Max(x1, x3)) - bounds.X;
					bounds.Height = Math.Max(Math.Max(y1, y2), Math.Max(y1, y3)) - bounds.Y;
				}
			}
			return bounds;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width * centerPosFactorX);
				int top = (int)Math.Round(-Height * centerPosFactorY);
				int right = left + Width;
				int bottom = top + Height;

				shapePoints[0].X = 0;
				shapePoints[0].Y = top;

				shapePoints[1].X = left;
				shapePoints[1].Y = bottom;

				shapePoints[2].X = right;
				shapePoints[2].Y = bottom;

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		protected override int DivFactorY { get { return 3; } }


		protected virtual float CenterPosFactorX { get { return centerPosFactorX; } }


		protected virtual float CenterPosFactorY { get { return centerPosFactorY; } }


		#region Fields

		protected Point[] shapePoints = new Point[3];
		private const float centerPosFactorX = 0.5f;
		private const float centerPosFactorY = 0.66666666f;

		private const int TopCenterControlPoint = 1;
		private const int BottomLeftControlPoint = 2;
		private const int BottomCenterControlPoint = 3;
		private const int BottomRightControlPoint = 4;
		private const int RotatePointControlPoint = 5;

		#endregion
	}

}
