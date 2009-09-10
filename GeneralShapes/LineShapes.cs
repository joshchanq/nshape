using System.ComponentModel;
using Dataweb.nShape.Advanced;
using System;


namespace Dataweb.nShape.GeneralShapes {

	/// <summary>
	/// Line consisting of multiple line segments.
	/// </summary>
	public class Polyline : PolylineBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			Shape result = new Polyline(shapeType, template);
			return result;
		}


		public override Shape Clone() {
			Shape result = new Polyline(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		[Category("Appearance")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Defines the line cap appearance of the line's beginning.")]
		public ICapStyle StartCapStyle {
			get {
				if (StartCapStyleInternal == null && Template == null) throw new nShapeException("Property StartCapStyle is not set.");
				return StartCapStyleInternal == null ? ((Polyline)Template.Shape).StartCapStyle : StartCapStyleInternal;
			}
			set {
				StartCapStyleInternal = (Template != null && value == ((Polyline)Template.Shape).StartCapStyle) ? null : value;
			}
		}


		[Category("Appearance")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Defines the line cap appearance of the line's ending.")]
		public ICapStyle EndCapStyle {
			get {
				if (EndCapStyleInternal == null && Template == null) throw new nShapeException("Property EndCapStyle is not set.");
				return EndCapStyleInternal == null ? ((Polyline)Template.Shape).EndCapStyle : EndCapStyleInternal;
			}
			set {
				EndCapStyleInternal = (Template != null && value == ((Polyline)Template.Shape).EndCapStyle) ? null : value;
			}
		}


		protected internal Polyline(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal Polyline(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		#region Fields
		private const string persistentTypeName = "MultiSegmentLine";
		#endregion
	}


	public class CircularArc : CircularArcBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			if (shapeType == null) throw new ArgumentNullException("shapeType");
			Shape result = new CircularArc(shapeType, template);
			return result;
		}


		public override Shape Clone() {
			Shape result = new CircularArc(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		[Category("Appearance")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Defines the line cap appearance of the line's beginning.")]
		public ICapStyle StartCapStyle {
			get {
				if (StartCapStyleInternal == null && Template == null) return null;
				return StartCapStyleInternal == null ? ((CircularArc)Template.Shape).StartCapStyle : StartCapStyleInternal;
			}
			set {
				StartCapStyleInternal = (Template != null && value == ((CircularArc)Template.Shape).StartCapStyle) ? null : value;
			}
		}


		[Category("Appearance")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Defines the line cap appearance of the line's ending.")]
		public ICapStyle EndCapStyle {
			get {
				if (EndCapStyleInternal == null && Template == null) return null;
				return EndCapStyleInternal == null ? ((CircularArc)Template.Shape).EndCapStyle : EndCapStyleInternal;
			}
			set {
				EndCapStyleInternal = (Template != null && value == ((CircularArc)Template.Shape).EndCapStyle) ? null : value;
			}
		}


		protected internal CircularArc(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}
	}


	#region RectangularLine (deactivated)
	//public class RectangularLine : MultiSegmentLine {
	//   protected internal RectifiedLine(Type shapeType, Template template)
	//		: base(shapeType, template) {
	//   }


	//   protected internal RectifiedLine(RectifiedLine source)
	//      : base(source) {
	//   }


	//   public override object Clone() {
	//      return new RectifiedLine(this);
	//   }


	//   public override string GetPersistentTypeName(int version) {
	//      return entityTypeName;
	//   }


	//   public override bool IsPointNear(int x, int y, float distance, ControlPointCapabilities controlPointCapabilities) {
	//      for (int i = 0; i < shapePoints.Length - 1; ++i) {
	//         if (LineIsNear(x, y, distance, shapePoints[i].X, shapePoints[i].Y, shapePoints[i + 1].X, shapePoints[i + 1].Y))
	//            return true;
	//      }
	//      return false;
	//   }


	//   public override bool ContainsPoint(int x, int y) {
	//      for (int i = 0; i < shapePoints.Length - 1; ++i) {
	//         if (Geometry.LineContainsPoint(x, y, shapePoints[i].X, shapePoints[i].Y, shapePoints[i + 1].X, shapePoints[i + 1].Y, 2))
	//            return true;
	//      }
	//      return false;
	//   }


	//   public override bool IntersectsWith(int x, int y, int width, int height) {
	//      for (int i = 0; i < shapePoints.Length - 1; ++i) {
	//         if (Geometry.LineIntersectsWithRect(shapePoints[i].X, shapePoints[i].Y,
	//                 shapePoints[i + 1].X, shapePoints[i + 1].Y,
	//                 x, y, x + width, y + height))
	//            return true;
	//      }
	//      return false;
	//   }


	//   public override void Invalidate() {
	//      if (DisplayService != null) {
	//         int margin = LineStyle.LineWidth + 1;
	//         for (int i = 0; i < shapePoints.Length - 1; ++i) {
	//            int left = int.MaxValue;
	//            int top = int.MaxValue;
	//            int right = int.MinValue;
	//            int bottom = int.MinValue;

	//            left = shapePoints[i].X < shapePoints[i + 1].X ? shapePoints[i].X : shapePoints[i + 1].X;
	//            top = shapePoints[i].Y < shapePoints[i + 1].Y ? shapePoints[i].Y : shapePoints[i + 1].Y;
	//            right = shapePoints[i].X > shapePoints[i + 1].X ? shapePoints[i].X : shapePoints[i + 1].X;
	//            bottom = shapePoints[i].Y > shapePoints[i + 1].Y ? shapePoints[i].Y : shapePoints[i + 1].Y;

	//            DisplayService.Invalidate(left - margin, top - margin, right + margin, bottom + margin);
	//         }
	//      }
	//   }


	//   public override void Draw(Graphics graphics) {
	//      if (PointCount > 1) {
	//         if (shapePoints.Length != (PointCount * 2) - 1)
	//            Array.Resize(ref shapePoints, (PointCount * 2) - 1);
	//         int i = 0;
	//         foreach (Point p in Vertices) {
	//            if (i == 0) {
	//               shapePoints[i] = p;
	//               shapePoints[i + 1] = p;
	//               i += 2;
	//            }
	//            else {
	//               if (i < shapePoints.Length)
	//                  shapePoints[i] = p;
	//               if (i + 1 < shapePoints.Length)
	//                  shapePoints[i + 1] = p;

	//               shapePoints[i - 1].Y = p.Y;
	//               i += 2;
	//            }
	//         }

	//         // draw Caps
	//         DrawStartCap(graphics, shapePoints[0].X, shapePoints[0].Y);
	//         DrawEndCap(graphics, shapePoints[PointCount - 1].X, shapePoints[PointCount - 1].Y);

	//         // draw Line
	//         Pen pen = ToolCache.GetPen(LineStyle, StartCapStyle, EndCapStyle);
	//         graphics.DrawLines(pen, shapePoints);
	//      }
	//   }


	//   #region Fields
	//   private const string entityTypeName = "RectangularLine";

	//   private Point[] shapePoints = new Point[2];
	//   #endregion
	//}
	#endregion


	#region BezierLine (deactivated)
	//public class BezierLine : LineShape {
	//    protected internal BezierLine(Type shapeType, Template template)
	//		: base(shapeType, template) {
	//    }


	//    protected internal BezierLine(BezierLine source)
	//        : base(source) {
	//        // copy all Vertices
	//        Array.Resize(ref shapePoints, source.PointCount);
	//        int i = 0;
	//        foreach (Point p in source.Vertices) {
	//            shapePoints[i] = p;
	//            ++i;
	//        }
	//    }


	//    public override object Clone() {
	//        return new BezierLine(this);
	//    }


	//    public override string GetPersistentTypeName(int version) {
	//        return entityTypeName;
	//    }


	//    public override bool Rotate(int tenthsOfDegree, int x, int y) {
	//        Invalidate();
	//        PointF p = PointF.Empty;
	//        p.X = x;
	//        p.Y = y;
	//        Matrix.BeginFieldReading();
	//        Matrix.RotateAt(tenthsOfDegree / 10f, p);
	//        Matrix.TransformPoints(shapePoints);
	//        Invalidate();
	//        return true;
	//    }


	//    public override IEnumerable<Point> Vertices {
	//        get { return shapePoints; }
	//    }


	//    public override int PointCount {
	//        get { return shapePoints.Length; }
	//    }


	//    public override int X {
	//        get { return shapePoints[0].X; }
	//        set { MoveControlPointTo(ControlPointId.Reference, value, Y, ResizeModifiers.None); }
	//    }


	//    public override int Y {
	//        get { return shapePoints[0].Y; }
	//        set { MoveControlPointTo(ControlPointId.Reference, X, value, ResizeModifiers.None); }
	//    }


	//    protected override float StartCapAngle {
	//        get {
	//            if (float.IsNaN(startCapAngle)) {
	//                PointF p = PointF.Empty;
	//                Geometry.BezierPoint(shapePoints[shapePoints.Length - 1], shapePoints[shapePoints.Length - 2], shapePoints[1], shapePoints[0], StartCapStyle.CapSize, ref p);
	//                startCapAngle = Geometry.RadiansToDegrees(Geometry.Angle(shapePoints[0].X, shapePoints[0].Y, (int)Math.Round(p.X), (int)Math.Round(p.Y)));
	//            }
	//            return startCapAngle;
	//        }
	//    }


	//    protected override float EndCapAngle {
	//        get {
	//            if (float.IsNaN(endCapAngle)) {
	//                PointF p = PointF.Empty;
	//                Geometry.BezierPoint(shapePoints[0], shapePoints[1], shapePoints[shapePoints.Length - 2], shapePoints[shapePoints.Length - 1], StartCapStyle.CapSize, ref p);
	//                endCapAngle = Geometry.RadiansToDegrees(Geometry.Angle(shapePoints[shapePoints.Length - 1].X, shapePoints[shapePoints.Length - 1].Y, (int)Math.Round(p.X), (int)Math.Round(p.Y)));
	//            }
	//            return endCapAngle;
	//        }
	//    }


	//    public override IEnumerable<nShapeAction> GetActions(int x, int mouseY, int connectionPointId) {
	//        List<nShapeAction> commands = new List<nShapeAction>();
	//        commands.Add(new InfoAction(this));
	//        if (ContainsPoint(x, mouseY) && connectionPointId <= 0 && shapePoints.Length < 4)
	//            commands.Add(new CommandAction("Insert Point", new InsertPointCommand(null, this, x, mouseY)));
	//        if (connectionPointId > 0 && shapePoints.Length > 2)
	//            commands.Add(new CommandAction("RemoveRange Point", new RemovePointCommand(null, this, connectionPointId)));

	//        return commands;
	//    }


	//    public override bool ContainsPoint(int x, int y) {
	//        bool newPointId = false;
	//        Rectangle rectangle = Rectangle.Empty;
	//        GetApproxBounds(ref rectangle);
	//        if (rectangle.Contains(x, y)) {
	//            GraphicsPath p = new GraphicsPath();
	//            p.AddBezier(shapePoints[0], shapePoints[1], shapePoints[PointCount - 2], shapePoints[PointCount - 1]);
	//            p.Widen(ToolCache.GetPen(LineStyle, StartCapStyle, EndCapStyle));
	//            newPointId = p.IsVisible(x, y);
	//            p.Dispose();
	//            p = null;
	//        }
	//        return newPointId;
	//    }


	//    private void GetApproxBounds(ref Rectangle rectangle) {
	//        int left = int.MaxValue;
	//        int top = int.MaxValue;
	//        int right = int.MinValue;
	//        int bottom = int.MinValue;
	//        for (int i = 0; i < shapePoints.Length; ++i) {
	//            if (i == 0 || i == shapePoints.Length - 1) {
	//                if (shapePoints[i].X < left) left = shapePoints[i].X;
	//                if (shapePoints[i].X > right) right = shapePoints[i].X;
	//                if (shapePoints[i].Y < top) top = shapePoints[i].Y;
	//                if (shapePoints[i].Y > bottom) bottom = shapePoints[i].Y;
	//            }
	//            else {
	//                if (shapePoints[i].X + ((Math.Min(shapePoints[0].X, shapePoints[PointCount - 1].X) - shapePoints[i].X) / 2) < left)
	//                    left = shapePoints[i].X + ((Math.Min(shapePoints[0].X, shapePoints[PointCount - 1].X) - shapePoints[i].X) / 2);
	//                if (shapePoints[i].X - ((shapePoints[i].X - Math.Max(shapePoints[0].X, shapePoints[PointCount - 1].X)) / 2) > right)
	//                    right = shapePoints[i].X - ((shapePoints[i].X - Math.Max(shapePoints[0].X, shapePoints[PointCount - 1].X)) / 2);
	//                if (shapePoints[i].Y + ((Math.Min(shapePoints[0].Y, shapePoints[PointCount - 1].Y) - shapePoints[i].Y) / 2) < top)
	//                    top = shapePoints[i].Y + ((Math.Min(shapePoints[0].Y, shapePoints[PointCount - 1].Y) - shapePoints[i].Y) / 2);
	//                if (shapePoints[i].Y - ((shapePoints[i].Y - Math.Max(shapePoints[0].Y, shapePoints[PointCount - 1].Y)) / 2) > bottom)
	//                    bottom = shapePoints[i].Y - ((shapePoints[i].Y - Math.Max(shapePoints[0].Y, shapePoints[PointCount - 1].Y)) / 2);
	//            }
	//        }
	//        rectangle.X = left;
	//        rectangle.Y = top;
	//        rectangle.Width = right - left;
	//        rectangle.Height = bottom - top;
	//    }


	//    public override bool IntersectsWith(int x, int y, int width, int height) {
	//        Rectangle rectangle = Rectangle.Empty;
	//        rectangle.X = x;
	//        rectangle.Y = y;
	//        rectangle.Width = width;
	//        rectangle.Height = height;

	//        if (Geometry.LineIntersectsWithRect(shapePoints[0], shapePoints[shapePoints.Length - 1], rectangle))
	//            return true;

	//        int left = int.MaxValue;
	//        int top = int.MaxValue;
	//        int right = int.MinValue;
	//        int bottom = int.MinValue;
	//        for (int i = 0; i < shapePoints.Length; ++i) {
	//            if (i == 0 || i == shapePoints.Length - 1) {
	//                if (shapePoints[i].X < left) left = shapePoints[i].X;
	//                if (shapePoints[i].X > right) right = shapePoints[i].X;
	//                if (shapePoints[i].Y < top) top = shapePoints[i].Y;
	//                if (shapePoints[i].Y > bottom) bottom = shapePoints[i].Y;

	//                if (rectangle.Contains(shapePoints[i]))
	//                    return true;
	//            }
	//            else {
	//                if (shapePoints[i].X + ((Math.Min(shapePoints[0].X, shapePoints[PointCount - 1].X) - shapePoints[i].X) / 2) < left)
	//                    left = shapePoints[i].X + ((Math.Min(shapePoints[0].X, shapePoints[PointCount - 1].X) - shapePoints[i].X) / 2);
	//                if (shapePoints[i].X - ((shapePoints[i].X - Math.Max(shapePoints[0].X, shapePoints[PointCount - 1].X)) / 2) > right)
	//                    right = shapePoints[i].X - ((shapePoints[i].X - Math.Max(shapePoints[0].X, shapePoints[PointCount - 1].X)) / 2);
	//                if (shapePoints[i].Y + ((Math.Min(shapePoints[0].Y, shapePoints[PointCount - 1].Y) - shapePoints[i].Y) / 2) < top)
	//                    top = shapePoints[i].Y + ((Math.Min(shapePoints[0].Y, shapePoints[PointCount - 1].Y) - shapePoints[i].Y) / 2);
	//                if (shapePoints[i].Y - ((shapePoints[i].Y - Math.Max(shapePoints[0].Y, shapePoints[PointCount - 1].Y)) / 2) > bottom)
	//                    bottom = shapePoints[i].Y - ((shapePoints[i].Y - Math.Max(shapePoints[0].Y, shapePoints[PointCount - 1].Y)) / 2);
	//            }
	//        }
	//        Rectangle bounds = Rectangle.Empty;
	//        bounds.X = left;
	//        bounds.Y = top;
	//        bounds.Width = right - left;
	//        bounds.Height = bottom - top;
	//        if (rectangle.IntersectsWith(bounds))
	//            return true;

	//        return false;
	//    }


	//    public override bool IsPointNear(int x, int y, float distance) {
	//        int left = int.MaxValue;
	//        int top = int.MaxValue;
	//        int right = int.MinValue;
	//        int bottom = int.MinValue;
	//        for (int i = 0; i < shapePoints.Length; ++i) {
	//            if (i == 0 || i == shapePoints.Length - 1) {
	//                if (shapePoints[i].X < left) left = shapePoints[i].X;
	//                if (shapePoints[i].X > right) right = shapePoints[i].X;
	//                if (shapePoints[i].Y < top) top = shapePoints[i].Y;
	//                if (shapePoints[i].Y > bottom) bottom = shapePoints[i].Y;
	//            }
	//            if (Geometry.Distance(x, y, shapePoints[i].X, shapePoints[i].Y) <= distance)
	//                return true;
	//        }
	//        if (left <= x && x <= right && top <= y && y <= bottom)
	//            return true;
	//        return false;
	//    }


	//    public override void Fit(int x, int y, int width, int height) {
	//        //throw new Exception("The method or operation is not implemented.");
	//    }


	//    protected override int ControlPointCount {
	//        get { return shapePoints.Length; }
	//    }


	//    public override Point GetControlPointPosition(int connectionPointId) {
	//        int controlPointIndex = GetPointIndex(connectionPointId);
	//        return shapePoints[controlPointIndex];
	//    }


	//    public override bool HasControlPointCapability(ControlPointId connectionPointId, ControlPointCapabilities controlPointCapabilities) {
	//        int controlPointIndex = GetPointIndex(connectionPointId);
	//        if ((controlPointCapabilities & ControlPointCapabilities.Reference) != 0) {
	//            if (controlPointIndex == 0)
	//                return true;
	//        }
	//        if ((controlPointCapabilities & ControlPointCapabilities.Glue) != 0) {
	//            if ((controlPointIndex == 0 || controlPointIndex == PointCount - 1) && IsConnectionPointEnabled(connectionPointId))
	//                return true;
	//        }
	//        if ((controlPointCapabilities & ControlPointCapabilities.Size) != 0) {
	//            return true;
	//        }
	//        if ((controlPointCapabilities & ControlPointCapabilities.AttachGluePointToConnectionPoint) != 0) {
	//            if (controlPointIndex != 0 || controlPointIndex != PointCount - 1)
	//                return true;
	//        }
	//        return false;
	//    }


	//    protected override bool MoveControlPointBy(int connectionPointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
	//        int pointIndex = connectionPointId - 1;
	//        shapePoints[pointIndex].X += deltaX;
	//        shapePoints[pointIndex].Y += deltaY;

	//        StartCapBounds = Rectangle.Empty;
	//        EndCapBounds = Rectangle.Empty;
	//        startCapAngle = float.NaN;
	//        endCapAngle = float.NaN;

	//        return true;
	//    }


	//    protected override bool MoveShape(int deltaX, int deltaY) {
	//        for (int i = 0; i < shapePoints.Length; ++i) {
	//            shapePoints[i].X += deltaX;
	//            shapePoints[i].Y += deltaY;
	//        }

	//        return true;
	//    }


	//    public override int AddVertex(int afterPointId, int x, int y) {
	//        int newPointId = ControlPointId.NotSupported;
	//        int afterControlPointIndex = GetPointIndex(afterPointId);
	//        if (afterControlPointIndex >= 0) {
	//            Invalidate();
	//            if (shapePoints.Length < 4) {
	//                // insert Point into segment
	//                Array.Resize(ref shapePoints, shapePoints.Length + 1);

	//                if (afterControlPointIndex + 1 < shapePoints.Length - 1) {
	//                    for (int i = shapePoints.Length - 1; i > afterControlPointIndex; --i) {
	//                        shapePoints[i].X = shapePoints[i - 1].X;
	//                        shapePoints[i].Y = shapePoints[i - 1].Y;
	//                    }
	//                }
	//                shapePoints[afterControlPointIndex + 1].X = x;
	//                shapePoints[afterControlPointIndex + 1].Y = y;
	//                newPointId = GetPointId(afterControlPointIndex + 1);
	//            }
	//            else {
	//                shapePoints[afterControlPointIndex - 1].X = shapePoints[afterControlPointIndex].X;
	//                shapePoints[afterControlPointIndex - 1].Y = shapePoints[afterControlPointIndex].Y;
	//                shapePoints[afterControlPointIndex].X = x;
	//                shapePoints[afterControlPointIndex].Y = y;
	//                newPointId = GetPointId(afterControlPointIndex);
	//            }
	//            this.Invalidate();
	//        }
	//        return newPointId;
	//    }


	//    public override int InsertVertex(int x, int y) {
	//        // find segment where the new point has to be inserted
	//        return AddVertex(GetPointId(shapePoints.Length), x, y);
	//    }


	//    public override void RemoveVertex(int connectionPointId) {
	//        this.Invalidate();
	//        int controlPointIndex = GetPointIndex(connectionPointId);
	//        Array.Copy(shapePoints, controlPointIndex + 1, shapePoints, controlPointIndex, (shapePoints.Length - 1) - controlPointIndex);
	//        Array.Resize(ref shapePoints, shapePoints.Length - 1);
	//        this.Invalidate();
	//    }


	//    public override void Invalidate() {
	//        if (DisplayService != null) {
	//            base.Invalidate();

	//            Rectangle currentBounds = Rectangle.Empty;
	//            Geometry.CalcBoundingRectangle(shapePoints, ref currentBounds);
	//            DisplayService.Invalidate(currentBounds);
	//        }
	//    }


	//    public override void Draw(Graphics graphics) {
	//        if (shapePoints.Length >= 2) {
	//            DrawStartCap(graphics, shapePoints[0].X, shapePoints[0].Y);
	//            DrawEndCap(graphics, shapePoints[shapePoints.Length - 1].X, shapePoints[shapePoints.Length - 1].Y);

	//            // draw Line
	//            Pen pen = ToolCache.GetPen(LineStyle, StartCapStyle, EndCapStyle);
	//            int lastIdx = shapePoints.Length - 1;
	//            graphics.DrawBezier(pen, shapePoints[0].X, shapePoints[0].Y, shapePoints[1].X, shapePoints[1].Y, shapePoints[lastIdx - 1].X, shapePoints[lastIdx - 1].Y, shapePoints[lastIdx].X, shapePoints[lastIdx].Y);

	//            //PointF res = PointF.Empty;
	//            //Geometry.BezierPoint(shapePoints[0], shapePoints[1], shapePoints[shapePoints.Length - 2], shapePoints[shapePoints.Length - 1], StartCapStyle.CapSize, ref res);
	//            //graphics.DrawLine(Pens.Green, res.X - 3, res.Y, res.X + 3, res.Y);
	//            //graphics.DrawLine(Pens.Green, res.X, res.Y - 3, res.X, res.Y + 3);

	//            //Geometry.BezierPoint(shapePoints[shapePoints.Length - 1], shapePoints[shapePoints.Length - 2], shapePoints[1], shapePoints[0], StartCapStyle.CapSize, ref res);
	//            //graphics.DrawLine(Pens.Green, res.X - 3, res.Y, res.X + 3, res.Y);
	//            //graphics.DrawLine(Pens.Green, res.X, res.Y - 3, res.X, res.Y + 3);

	//            //float Ax, Ay, Az;
	//            //float Bx, By, Bz;
	//            //float Cx, Cy, Cz;
	//            //float Dx, Dy, Dz;

	//            //Ax = shapePoints[0].X; Ay = shapePoints[0].Y;
	//            //Bx = shapePoints[1].X; By = shapePoints[1].Y;
	//            //Cx = shapePoints[shapePoints.Length - 2].X; Cy = shapePoints[shapePoints.Length - 2].Y;
	//            //Dx = shapePoints[shapePoints.Length - 1].X; Dy = shapePoints[shapePoints.Length - 1].Y;

	//            //int cnt = 20;
	//            //PointF[] pts = new PointF[cnt+1];
	//            //float a = 1.0f;
	//            //float b = 1.0f - a;
	//            //for (int i = 0; i <= cnt; ++i) {
	//            //   // Get a point on the curve
	//            //   float X = Ax * a * a * a + Bx * 3 * a * a * b + Cx * 3 * a * b * b + Dx * b * b * b;
	//            //   float Y = Ay * a * a * a + By * 3 * a * a * b + Cy * 3 * a * b * b + Dy * b * b * b;
	//            //   //float Z = Az * a * a * a + Bz * 3 * a * a * b + Cz * 3 * a * b * b + Dz * b * b * b;

	//            //   // Draw the line from point to point (assuming OGL is set up properly)
	//            //   pts[i].X = X;
	//            //   pts[i].Y = Y;

	//            //   // Change the variable
	//            //   a -= 1f / cnt;
	//            //   b = 1.0f - a;
	//            //}
	//            //LineStyle l1 = new LineStyle(null, "line 1");
	//            //l1.ColorStyle = new ColorStyle(null, "red", Color.FromArgb(128, Color.Red));
	//            //l1.DashStyle = Dataweb.nShape.DashStyle.Solid;
	//            //l1.LineJoin = LineJoin.Round;
	//            //l1.LineWidth = 1;
	//            //Pen pen1 = ToolCache.GetPen(l1, StartCapStyle, EndCapStyle);
	//            //Pen linePt1 = new Pen(l1.ColorStyle.Color);
	//            ////graphics.DrawLines(pen1, pts);
	//            ////foreach (PointF p in pts) {
	//            ////   graphics.DrawLine(linePt1, p.X - 2, p.Y, p.X + 2, p.Y);
	//            ////}

	//            ////cnt = cnt * 10;
	//            //cnt = 200;
	//            //pts = new PointF[cnt + 1];
	//            // a = 1.0f;
	//            // b = 1.0f - a;
	//            //for (int i = 0; i <= cnt; ++i) {
	//            //   // Get a point on the curve
	//            //   float X = Ax * a * a * a + Bx * 3 * a * a * b + Cx * 3 * a * b * b + Dx * b * b * b;
	//            //   float Y = Ay * a * a * a + By * 3 * a * a * b + Cy * 3 * a * b * b + Dy * b * b * b;
	//            //   //float Z = Az * a * a * a + Bz * 3 * a * a * b + Cz * 3 * a * b * b + Dz * b * b * b;

	//            //   // Draw the line from point to point (assuming OGL is set up properly)
	//            //   pts[i].X = X;
	//            //   pts[i].Y = Y;

	//            //   // Change the variable
	//            //   a -= 1f / cnt;
	//            //   b = 1.0f - a;
	//            //}
	//            //LineStyle l2 = new LineStyle(null, "line 1");
	//            //l2.ColorStyle = new ColorStyle(null, "yellow", Color.FromArgb(128, Color.Yellow));
	//            //l2.DashStyle = Dataweb.nShape.DashStyle.Solid;
	//            //l2.LineJoin = LineJoin.Round;
	//            //l2.LineWidth = 1;
	//            //Pen pen2 = ToolCache.GetPen(l2, StartCapStyle, EndCapStyle);
	//            //Pen linePt2 = new Pen(l2.ColorStyle.Color);
	//            ////graphics.DrawLines(pen2, pts);
	//            //foreach (PointF p in pts) {
	//            //   graphics.DrawLine(linePt2, p.X - 0.2f, p.Y, p.X + 0.2f, p.Y);
	//            //   graphics.DrawLine(linePt2, p.X, p.Y - 0.2f, p.X, p.Y + 0.2f);
	//            //}
	//        }
	//    }


	//    public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
	//        Graphics g = Graphics.FromImage(image);
	//        g.SmoothingMode = SmoothingMode.HighQuality;
	//        SolidBrush brush = new SolidBrush(transparentColor);
	//        g.FillRectangle(brush, 0, 0, image.Width, image.Height);

	//        int startCapSize = 0;
	//        if (StartCapStyle.CapShape != CapShape.None)
	//            startCapSize = StartCapStyle.CapSize;
	//        int endCapSize = 0;
	//        if (EndCapStyle.CapShape != CapShape.None)
	//            endCapSize = EndCapStyle.CapSize;

	//        int height, width;
	//        height = width = (int)Math.Max(startCapSize, endCapSize) * 4;
	//        if (height == 0 || width == 0) {
	//            width = image.Width;
	//            height = image.Height;
	//        }
	//        g.ScaleTransform((float)image.Width / width, (float)image.Height / height);

	//        Point[] points = new Point[4] {
	//         new Point(margin, height / 4),
	//         new Point(width - (width / 4), height / 4),
	//         new Point(width / 4, height - (height / 4)),
	//         new Point(width - margin, height - (height / 4))
	//      };

	//        Pen pen = ToolCache.GetPen(LineStyle, StartCapStyle, EndCapStyle);
	//        g.DrawBeziers(pen, points);
	//    }


	//    #region Fields
	//    private const string entityTypeName = "BezierLine";

	//    float startCapAngle = float.NaN;
	//    float endCapAngle = float.NaN;
	//    // drawing stuff
	//    Point[] shapePoints = new Point[2];
	//    #endregion
	//}
	#endregion

}
