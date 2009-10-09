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
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	public abstract class PolygonBase : CaptionedShapeBase {

		protected PolygonBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			throw new NotImplementedException();
		}


		protected PolygonBase(ShapeType shapeType, Template template) : base(shapeType, template) {
			throw new NotImplementedException();
		}


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			throw new NotImplementedException();
		}

		protected override int DivFactorX {
			get { throw new NotImplementedException(); }
		}

		protected override int DivFactorY {
			get { throw new NotImplementedException(); }
		}

		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			throw new NotImplementedException();
		}

		protected override void CalcControlPoints() {
			throw new NotImplementedException();
		}

		protected internal override int ControlPointCount {
			get { throw new NotImplementedException(); }
		}

		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			throw new NotImplementedException();
		}

		public override Shape Clone() {
			throw new NotImplementedException();
		}

		public override void Fit(int x, int y, int width, int height) {
			throw new NotImplementedException();
		}

		public override RelativePosition CalculateRelativePosition(int x, int y) {
			throw new NotImplementedException();
		}

		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			throw new NotImplementedException();
		}
	}


		//public override void CopyFrom(Shape source) {
		//   base.CopyFrom(source);
		//   if (source is PolygonBase) {
		//      shapePoints = new Point[((PolygonBase)source).PointCount];
		//      for (int i = 0; i < shapePoints.Length; ++i) {
		//         shapePoints[i].X = ((PolygonBase)source).shapePoints[i].X;
		//         shapePoints[i].Y = ((PolygonBase)source).shapePoints[i].Y;
		//      }
		//      center = Geometry.CalcPolygonBalancePoint(shapePoints);
		//   }
		//}


		//#region IEntity Members

		//protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
		//   base.LoadFieldsCore(reader, version);
		//   int pointCount = reader.ReadInt32();
		//   if (pointCount != shapePoints.Length)
		//      Array.Resize(ref shapePoints, pointCount);
		//}


		//protected override void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
		//   switch (propertyName) {
		//      case "Vertices":
		//         // load Vertices
		//         reader.BeginReadInnerObjects();
		//         while (reader.BeginReadInnerObject()) {
		//            int ptIdx = reader.ReadInt32();
		//            int ptId = reader.ReadInt32();
		//            int x = reader.ReadInt32();
		//            int y = reader.ReadInt32();
		//            MoveControlPointTo(ptId, x, y, ResizeModifiers.None);
		//            reader.EndReadInnerObject();
		//         }
		//         reader.EndReadInnerObjects();
		//         break;
		//      default:
		//         base.LoadInnerObjectsCore(propertyName, reader, version);
		//         break;
		//   }
		//}


		//protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
		//   base.SaveFieldsCore(writer, version);
		//}


		//protected override void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
		//   switch (propertyName) {
		//      case "Vertices":
		//         // save Vertices
		//         writer.BeginWriteInnerObjects();
		//         foreach (ControlPointId pointId in GetControlPointIds(ControlPointCapabilities.All)) {
		//            Point p = GetControlPointPosition(pointId);
		//            writer.BeginWriteInnerObject();
		//            writer.WriteInt32(pointId - 1);
		//            writer.WriteInt32(pointId);
		//            writer.WriteInt32(p.X);
		//            writer.WriteInt32(p.Y);
		//            writer.EndWriteInnerObject();
		//         }
		//         writer.EndWriteInnerObjects();
		//         break;
		//      default:
		//         base.SaveInnerObjectsCore(propertyName, writer, version);
		//         break;
		//   }
		//}


		//public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
		//   foreach (EntityPropertyDefinition pi in CaptionedShapeBase.GetPropertyDefinitions(version))
		//      yield return pi;
		//   yield return new EntityFieldDefinition("PointCount", typeof(int));
		//   yield return new EntityInnerObjectsDefinition("Vertices", pointTypeName, pointAttrNames, pointAttrTypes);
		//}

		//#endregion


		//public virtual int PointCount {
		//   get { return shapePoints.Length; }
		//}


		//protected internal override int ControlPointCount {
		//   get { return shapePoints.Length + 1; }
		//}


		//public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
		//   if (controlPointId == 1)
		//      return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0 || (controlPointCapability & ControlPointCapabilities.Reference) != 0);
		//   else if (controlPointId > 1 && controlPointId <= ControlPointCount)
		//      return ((controlPointCapability & ControlPointCapabilities.Resize) != 0) || ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
		//   else return base.HasControlPointCapability(controlPointId, controlPointCapability);
		//}


		//public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
		//   throw new NotImplementedException();
		//}


		//public override RelativePosition CalculateRelativePosition(int x, int y) {
		//   throw new NotImplementedException();
		//}


		//public override void Draw(Graphics graphics) {
		//   if (graphics == null) throw new ArgumentNullException("graphics");
		//   DrawPath(graphics, LineStyle, FillStyle);
		//   DrawCaption(graphics);

		//   //graphics.DrawLine(Pens.Red, X - 3, Y, X + 3, Y);
		//   //graphics.DrawLine(Pens.Red, X, Y - 3, X, Y + 3);


		//   //Point[] pts = new Point[shapePoints.Length];
		//   //int width = (int)Math.Round(0.7f * Bounds.Width);
		//   //int height = (int)Math.Round(0.7f * Bounds.Height);
		//   //int x = (int)Math.Round(0.7f * Bounds.X);
		//   //int y = (int)Math.Round(0.7f * Bounds.Y);
		//   //UpdateGraphicalObjects();
		//   //float scaleFactor = Geometry.CalcScaleFactor(Bounds.Width, Bounds.Height, width, height);
		//   //for (int i = 0; i < shapePoints.Length; ++i) {
		//   //   float deltaX = (shapePoints[i].X - Bounds.left) * scaleFactor;
		//   //   float deltaY = (shapePoints[i].Y - Bounds.top) * scaleFactor;
		//   //   pts[i].X = (int)Math.Round(x + deltaX);
		//   //   pts[i].Y = (int)Math.Round(y + deltaY);
		//   //}
		//   //Point c = Geometry.CalcPolygonBalancePoint(pts);

		//   //graphics.DrawRectangle(Pens.Red, Bounds);

		//   //graphics.DrawPolygon(Pens.Green, pts);
		//   //graphics.DrawLine(Pens.Red, c.X - 3, c.Y, c.X + 3, c.Y);
		//   //graphics.DrawLine(Pens.Red, c.X, c.Y - 3, c.X, c.Y + 3);

		//   base.Draw(graphics);
		//}


		//protected internal override void InitializeToDefault(IStyleSet styleSet) {
		//   base.InitializeToDefault(styleSet);
		//   ControlPoints = new Point[ControlPointCount];
		//   shapePoints[0].X = 0;
		//   shapePoints[0].Y = -20;
		//   shapePoints[1].X = -20;
		//   shapePoints[1].Y = 20;
		//   shapePoints[2].X = 20;
		//   shapePoints[2].Y = 20;
		//   Center = Geometry.CalcPolygonBalancePoint(shapePoints);
		//}


		//protected internal PolygonBase(ShapeType shapeType, Template template)
		//   : base(shapeType, template) {
		//}


		//protected internal PolygonBase(ShapeType shapeType, IStyleSet styleSet)
		//   : base(shapeType, styleSet) {
		//}


		//protected Point[] ShapePoints {
		//   get { return shapePoints; }
		//   set { shapePoints = value; }
		//}


		//protected override bool ContainsPointCore(int x, int y) {
		//   if (Path.PointCount == 0)
		//      return Geometry.ConvexPolygonContainsPoint(shapePoints, x, y);
		//   else
		//      return Geometry.ConvexPolygonContainsPoint(Path.PathPoints, x, y);
		//}


		//protected override bool IntersectsWithCore(int x, int y, int width, int height) {
		//   Rectangle rectangle = Rectangle.Empty;
		//   rectangle.X = x;
		//   rectangle.Y = y;
		//   rectangle.Width = width;
		//   rectangle.Height = height;
		//   if (Path.PointCount == 0) {
		//      if (Geometry.PolygonIntersectsWithRectangle(shapePoints, rectangle))
		//         return true;
		//   } else {
		//      if (Geometry.PolygonIntersectsWithRectangle(Path.PathPoints, rectangle))
		//         return true;
		//   }
		//   return false;
		//}


		//protected override bool MoveByCore(int deltaX, int deltaY) {
		//   return base.MoveByCore(deltaX, deltaY);
		//}


		//protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
		//   bool result = true;
		//   switch (pointId) {
		//      case 1:
		//         result = false;
		//         break;
		//      default:
		//         shapePoints[pointId - 2].X += (int)Math.Round(transformedDeltaX);
		//         shapePoints[pointId - 2].Y += (int)Math.Round(transformedDeltaY);

		//         Center = Geometry.CalcPolygonBalancePoint(shapePoints);
		//         InvalidateDrawCache();

		//         ControlPointsHaveMoved();
		//         break;
		//   }
		//   return result;
		//}


		//protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
		//   if (index != 0) throw new IndexOutOfRangeException();
		//   Geometry.CalcBoundingRectangle(shapePoints, out captionBounds);
		//}


		//protected override void CalcControlPoints() {
		//   // rotation handle
		//   Center = Geometry.CalcPolygonBalancePoint(shapePoints);

		//   ControlPoints[0].X = Center.X;
		//   ControlPoints[0].Y = Center.Y;
		//   // resize handles
		//   for (int i = 0; i < shapePoints.Length; ++i) {
		//      ControlPoints[i + 1].X = shapePoints[i].X;
		//      ControlPoints[i + 1].Y = shapePoints[i].Y;
		//   }
		//}


		//protected override bool CalculatePath() {
		//   if (base.CalculatePath()) {
		//      Path.StartFigure();
		//      Path.AddPolygon(shapePoints);
		//      Path.CloseFigure();
		//      return true;
		//   } else return false;
		//}


		//#region Fields

		//private static string pointTypeName = "Point";
		//private static string[] pointAttrNames = new string[] { "PointIndex", "PointId", "X", "Y" };
		//private static Type[] pointAttrTypes = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) };

		//private Point[] shapePoints = new Point[3];
		//private Point center = Point.Empty;

		//#endregion
}