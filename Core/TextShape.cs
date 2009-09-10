using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Dataweb.nShape.Advanced {

	/// <summary>
	/// Title shape.
	/// </summary>
	public abstract class TextBase : RectangleBase {

		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			AutoSize = true;
			Text = "Text";
			// override standard Line- and FillStyles
			FillStyle = styleSet.FillStyles.Transparent;
			LineStyle = styleSet.LineStyles.None;
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is TextBase) {
				if (((TextBase)source).AutoSize) {
					autoSize = ((TextBase)source).AutoSize;
					CalcNewShapeSize(Text);
				}
			}
		}


		#region IPersistable Members

		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			AutoSize = reader.ReadBool();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteBool(AutoSize);
		}


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("AutoSize", typeof(bool));
		}

		#endregion


		public override string Text {
			get { return base.Text; }
			set {
				if (autoSize) CalcNewShapeSize(value);
				base.Text = value;
			}
		}


		public override int Width {
			get {
				if (autoSize && !(Text == string.Empty)) {
					if (textWidth == 0)
						CalcNewShapeSize(Text);
					if (textWidth > 0)
						return textWidth;
					else return base.Width;
				} else
					return base.Width;
			}
			set { base.Width = value; }
		}


		public override int Height {
			get {
				if (autoSize && !(Text == string.Empty)) {
					if (textHeight == 0)
						CalcNewShapeSize(Text);
					if (textHeight > 0)
						return textHeight;
					else return base.Height;
				}
				return base.Height;
			}
			set { base.Height = value; }
		}


		[Category("Appearance")]
		[Description("Enables automatic resizing based on the text's size. If enabled, the WordWrap property of ParagraphStyles has no effect.")]
		public bool AutoSize {
			get { return autoSize; }
			set {
				Invalidate();
				autoSize = value;
				CalcNewShapeSize(Text);
				InvalidateDrawCache();
				Invalidate();
			}
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
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 && !AutoSize);
				case ControlPointId.Reference:
				case MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Connect) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			return base.GetControlPointIds(controlPointCapability);
		}


		//protected override Rectangle CalculateBoundingRectangle(bool tight) {
		//   if (!string.IsNullOrEmpty(Text) && autoSize) {
		//      Rectangle result = Rectangle.Empty;
		//      if (Angle % 1800 == 0) {
		//         result.Width = Width;
		//         result.Height = Height;
		//         result.X = X - (int)Math.Round(result.Width / 2f);
		//         result.Y = Y - (int)Math.Round(result.Height / 2f);
		//      } else if (Angle % 1800 == 0) {
		//         result.Width = Height;
		//         result.Height = Width;
		//         result.X = X - (int)Math.Round(result.Width / 2f);
		//         result.Y = Y - (int)Math.Round(result.Height / 2f);
		//      } else {
		//         result.X = (int)Math.Round(-Width / 2f);
		//         result.Y = (int)Math.Round(-Height / 2f);
		//         result.Width = Width;
		//         result.Height = Height;

		//         Point tl = Point.Empty, tr = Point.Empty, br = Point.Empty, bl = Point.Empty;
		//         tl.Offset(result.Left, result.Top);
		//         tr.Offset(result.Right, result.Top);
		//         bl.Offset(result.Left, result.Bottom);
		//         br.Offset(result.Right, result.Bottom);
		//         Geometry.TransformRectangle(Center, Angle, result, out tl, out tr, out br, out bl);

		//         result.X = Math.Min(Math.Min(tl.X, tr.X), Math.Min(bl.X, br.X));
		//         result.Y = Math.Min(Math.Min(tl.Y, tr.Y), Math.Min(bl.Y, br.Y));
		//         result.Width = Math.Max(Math.Max(tl.X, tr.X), Math.Max(bl.X, br.X)) - result.X;
		//         result.Height = Math.Max(Math.Max(tl.Y, tr.Y), Math.Max(bl.Y, br.Y)) - result.Y;
		//      }
		//      return result;
		//   } else return base.CalculateBoundingRectangle(tight);
		//}


		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			DrawPath(graphics, LineStyle, FillStyle);
			DrawCaption(graphics);
		}


		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			AutoSize = false;
			Text = "abc";

			Size textSize = Size.Empty;
			// Set proposed Size
			textSize.Width = Width;
			textSize.Height = Height;

			textSize = TextMeasurer.MeasureText(Text, ToolCache.GetFont(CharacterStyle), textSize, ParagraphStyle);
			Width = textSize.Width + (textSize.Width % 2);
			Height = textSize.Height + (textSize.Height % 2);

			base.DrawThumbnail(image, margin, transparentColor);
		}


		//protected override void CalcCaptionBounds(int index, ref Rectangle captionBounds) {
		//   if (index != 0) throw new IndexOutOfRangeException();
		//   if (CharacterStyle != null && ParagraphStyle != null && autoSize) {
		//      Size textSize = Size.Empty;
		//      textSize.Height = textSize.Width = int.MaxValue;
		//      TextFormatFlags textFormatFlags = ToolCache.GetTextFormatFlags(ParagraphStyle);
		//      textFormatFlags |= TextFormatFlags.SingleLine;
		//      textSize = TextRenderer.MeasureText(Title, ToolCache.GetFont(CharacterStyle), textSize, textFormatFlags);
		//      else textSize = TextRenderer.MeasureText(null, Title, ToolCache.GetFont(CharacterStyle), textSize, textFormatFlags);
		//      captionBounds.X = (int)Math.Round(-textSize.Width / 2f);
		//      captionBounds.Y = (int)Math.Round(-textSize.Height / 2f);
		//      captionBounds.Width = textSize.Width;
		//      captionBounds.Height = textSize.Height;
		//   }
		//   else 
		//   base.CalcCaptionBounds(index, ref captionBounds);
		//}


		protected internal TextBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal TextBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			if (autoSize && !string.IsNullOrEmpty(Text)) {
				RectangleF rectangle = RectangleF.Empty;
				rectangle.X = x;
				rectangle.Y = y;
				rectangle.Width = width;
				rectangle.Height = height;

				if (Angle % 900 == 0)
					return rectangle.IntersectsWith(GetBoundingRectangle(true));
				else {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					Point tl = Point.Empty, tr = Point.Empty, br = Point.Empty, bl = Point.Empty;
					Rectangle layoutRectangle = Rectangle.Empty;
					CalcCaptionBounds(0, out layoutRectangle);
					Geometry.TransformRectangle(Center, Angle, layoutRectangle, out tl, out tr, out br, out bl);

					if (rectangle.Contains(tl) || rectangle.Contains(tr) || rectangle.Contains(bl) || rectangle.Contains(br))
						return true;
					else {
						if (Geometry.RectangleIntersectsWithLine(rectangle, tl.X, tl.Y, tr.X, tr.Y, true))
							return true;
						if (Geometry.RectangleIntersectsWithLine(rectangle, tr.X, tr.Y, br.X, br.Y, true))
							return true;
						if (Geometry.RectangleIntersectsWithLine(rectangle, br.X, br.Y, bl.X, bl.Y, true))
							return true;
						if (Geometry.RectangleIntersectsWithLine(rectangle, bl.X, bl.Y, tl.X, tl.Y, true))
							return true;
					}
					return false;
				}
			} else
				return base.IntersectsWithCore(x, y, width, height);
		}


		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			textWidth = 0;
			textHeight = 0;
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int width, height;
				//if (autoSize) {
				//   Size size = Size.Empty;
				//   Graphics gfx = DisplayService.GetInfoGraphics();
				//   if (gfx != null) {
				//      size = TextRenderer.MeasureText(gfx, Title, ToolCache.GetFont(CharacterStyle), layoutRectangle.Size, ToolCache.GetTextFormatFlags(ParagraphStyle));
				//      gfx.Dispose();
				//   }
				//   else TextRenderer.MeasureText(Title, ToolCache.GetFont(CharacterStyle), layoutRectangle.Size, ToolCache.GetTextFormatFlags(ParagraphStyle));
				//   width = size.Width;
				//   height = size.Height;
				//}
				//else {
				width = Width;
				height = Height;
				//}

				Path.Reset();
				Rectangle shapeBuffer = Rectangle.Empty;
				shapeBuffer.X = (int)Math.Round(-width / 2f);
				shapeBuffer.Y = (int)Math.Round(-height / 2f); ;
				shapeBuffer.Width = width;
				shapeBuffer.Height = height;

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(shapeBuffer);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private void CalcNewShapeSize(string txt) {
			textHeight = textWidth = 0;
			if (CharacterStyle != null && ParagraphStyle != null) {
				//formatFlags |= TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding;
				Size textSize = TextMeasurer.MeasureText(txt, ToolCache.GetFont(CharacterStyle), Size.Empty, ParagraphStyle);
				textWidth = textSize.Width + ParagraphStyle.Padding.Horizontal + ParagraphStyle.Padding.Horizontal;
				textHeight = textSize.Height + ParagraphStyle.Padding.Vertical + ParagraphStyle.Padding.Vertical;
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
		
		private bool autoSize = true;
		private int textWidth;
		private int textHeight;
		
		#endregion
	}


	public abstract class LabelBase : TextBase {

		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is LabelBase) {
				LabelBase src = (LabelBase)source;
				this.maintainOrientation = src.maintainOrientation;
				this.gluePointPos = src.gluePointPos;
				this.calcInfo = src.calcInfo;
			}
		}


		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
		}


		public bool MaintainOrientation {
			get { return maintainOrientation; }
			set {
				Invalidate();
				maintainOrientation = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if (controlPointId == GlueControlPoint)
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0 
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			else return base.HasControlPointCapability(controlPointId, controlPointCapability);
		}


		protected internal override int ControlPointCount {
			get { return base.ControlPointCount + 1; }
		}


		public override void Connect(ControlPointId ownPointId, Shape otherShape, ControlPointId otherPointId) {
			if (otherShape == null) throw new ArgumentNullException("otherShape");
			// Calculate the relative position of the gluePoint on the other shape
			CalcGluePointCalcInfo(ownPointId, otherShape, otherPointId);
			InvalidateDrawCache();
			base.Connect(ownPointId, otherShape, otherPointId);
		}


		public override void Disconnect(ControlPointId gluePointId) {
			base.Disconnect(gluePointId);
			calcInfo = GluePointCalcInfo.Empty;
		}


		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			if (controlPointId == GlueControlPoint)
				return gluePointPos;
			else
				return base.GetControlPointPosition(controlPointId);
		}


		public override void Invalidate() {
			base.Invalidate();
			if (DisplayService != null) {
				int left = Math.Min(gluePointPos.X - 3, X);
				int right = Math.Max(gluePointPos.X + 3, X);
				int top = Math.Min(gluePointPos.Y - 6, Y);
				int bottom = Math.Max(gluePointPos.Y, Y);
				DisplayService.Invalidate(left, top, right - left, bottom - top);
			}
		}


		public override void Draw(Graphics graphics) {
			base.Draw(graphics);
			if (IsConnected(GlueControlPoint, null) == ControlPointId.None) {
				//Pen pen = null;
				// If the current line style is completely transparent, try to get 
				// an other pen for drawing the gluepoint-pin.
				//if (LineStyle.ColorStyle.Transparency == 100) {
				//   if (pinPreviewLineStyle != null) pen = ToolCache.GetPen(pinPreviewLineStyle, null, null);
				//   else if (DisplayService != null) pen = ToolCache.GetPen(DisplayService.HintLineStyle, null, null);
				//} else pen = ToolCache.GetPen(LineStyle, null, null);
				//// If getting another pen failed, we weed not draw the pin at all
				//if (pen != null) graphics.DrawPath(pen, pinPath);
				
				if (DisplayService != null) {
					Pen forePen = ToolCache.GetPen(DisplayService.HintForegroundStyle, null, null);
					Brush backbrush = ToolCache.GetBrush(DisplayService.HintBackgroundStyle);
					graphics.FillPath(backbrush, pinPath);
					graphics.DrawPath(forePen, pinPath);
				}
			}
			//else {
			//   // Draw Line to GluePoint
			//   graphics.DrawLine(Pens.Green, gluePointPos, Center);
			//   GdiHelpers.DrawPoint(graphics, Pens.Green, gluePointPos.X, gluePointPos.Y, 3);

			//   ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
			//   if (ci != ShapeConnectionInfo.Empty) {
			//      Point absPos = ci.OtherShape.CalculateAbsolutePosition(calcInfo.RelativePosition);
			//      GdiHelpers.DrawPoint(graphics, Pens.Red, absPos.X, absPos.Y, 3);
			//      System.Diagnostics.Debug.Print("Relative Position: A = {0}, B = {1}", calcInfo.RelativePosition.A, calcInfo.RelativePosition.B);
			//      System.Diagnostics.Debug.Print("Absolute Position: {0}", absPos);
			//   }


			//   //ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
			//   //float shapeAngle = 0;
			//   //if (ci.OtherShape is ILinearShape) {
			//   //   ILinearShape line = (ILinearShape)ci.OtherShape;
			//   //   Point nv = line.CalcNormalVector(gluePointPos);
			//   //   graphics.DrawLine(Pens.Red, gluePointPos, nv);
			//   //   shapeAngle = Geometry.RadiansToTenthsOfDegree(Geometry.Angle(gluePointPos.X, gluePointPos.Y, nv.X, nv.Y)) - 900;
			//   //} else if (ci.OtherShape is IPlanarShape) {
			//   //   shapeAngle = Geometry.TenthsOfDegreeToDegrees(((IPlanarShape)ci.OtherShape).Angle);
			//   //}
			//   //shapeAngle = shapeAngle / 10f;
			//   //GdiHelpers.DrawAngle(graphics, Brushes.Red, gluePointPos, shapeAngle, 10);
			//   //GdiHelpers.DrawAngle(graphics, Brushes.Purple, gluePointPos, shapeAngle, calcInfo.Alpha, 10);

			//   //Font font = new Font("Arial", 8);
			//   //string debugStr = string.Format("ShapeAngle: {0}°\nAlpha: {1}°", shapeAngle, calcInfo.Alpha);
			//   //graphics.DrawString(debugStr, font, Brushes.Blue, gluePointPos);
			//   //font.Dispose();
			//}
		}


		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			gluePointPos.X = X - Width / 2;
			gluePointPos.Y = Y - Height / 2;
			InvalidateDrawCache();
			UpdateDrawCache();
			base.DrawThumbnail(image, margin, transparentColor);
		}


		public override void DrawOutline(Graphics graphics, Pen pen) {
			base.DrawOutline(graphics, pen);
			Point p = CalculateConnectionFoot(gluePointPos.X, gluePointPos.Y);
			graphics.DrawLine(pen, gluePointPos, p);
		}


		protected internal LabelBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal LabelBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			gluePointPos.X = (int)Math.Round(X - (Width / 2f)) - 10;
			gluePointPos.Y = (int)Math.Round(Y - (Height / 2f)) - 10;
			calcInfo = GluePointCalcInfo.Empty;
		}


		#region IPersistable Members

		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			gluePointPos.X = reader.ReadInt32();
			gluePointPos.Y = reader.ReadInt32();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(gluePointPos.X);
			writer.WriteInt32(gluePointPos.Y);
		}


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in TextBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("GluePointX", typeof(int));
			yield return new EntityFieldDefinition("GluePointY", typeof(int));
		}

		#endregion


		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapabilities, int range) {
			if ((controlPointCapabilities & ControlPointCapabilities.Glue) != 0
				|| (controlPointCapabilities & ControlPointCapabilities.Resize) != 0) {
				if (Geometry.DistancePointPoint(x, y, gluePointPos.X, gluePointPos.Y) <= range)
					return GlueControlPoint;
				if (IsConnected(GlueControlPoint, null) == ControlPointId.None
					&& pinPath.IsVisible(x, y)) return GlueControlPoint;
			}
			return base.HitTest(x, y, controlPointCapabilities, range);
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = base.CalculateBoundingRectangle(tight);
			if (!tight && IsConnected(GlueControlPoint, null) == ControlPointId.None) {
				if (gluePointPos != Geometry.InvalidPoint) {
					Point tl = Point.Empty, tr = Point.Empty, bl = Point.Empty, br = Point.Empty;
					tl.X = bl.X = gluePointPos.X - pinSize / 2;
					tr.X = br.X = gluePointPos.X + pinSize / 2;
					tl.Y = tr.Y = gluePointPos.Y - pinSize;
					bl.Y = br.Y = gluePointPos.Y;
					tl = Geometry.RotatePoint(gluePointPos, pinAngle, tl);
					tr = Geometry.RotatePoint(gluePointPos, pinAngle, tr);
					bl = Geometry.RotatePoint(gluePointPos, pinAngle, bl);
					br = Geometry.RotatePoint(gluePointPos, pinAngle, br);
					Rectangle pinBounds;
					Geometry.CalcBoundingRectangle(tl, tr, br, bl, out pinBounds);
					result = Geometry.UniteRectangles(result, pinBounds);
				}
			}
			return result;
		}
		
		
		protected override bool ContainsPointCore(int x, int y) {
			bool result = base.ContainsPointCore(x, y);
			if (!result) {
				if (pinPath != null) result = pinPath.IsVisible(x, y);
				else return (Geometry.DistancePointPoint(gluePointPos.X, gluePointPos.Y, x, y) <= (pinSize/2f));
			}
			return result;
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			bool result = base.IntersectsWithCore(x, y, width, height);
			if (!result) {
				int boundsX = gluePointPos.X - (pinSize / 2);
				int boundsY = gluePointPos.Y - pinSize;
				result = Geometry.RectangleIntersectsWithRectangle(x, y, width, height, boundsX, boundsY, pinSize, pinSize)
					|| Geometry.RectangleContainsRectangle(x, y, width, height, boundsX, boundsY, pinSize, pinSize);
			}
			return result;
		}


		protected override bool MoveByCore(int deltaX, int deltaY) {
			bool result = base.MoveByCore(deltaX, deltaY);
			// If the glue point is not connected, move it with the shape
			ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
			if (ci.IsEmpty) {
				gluePointPos.X += deltaX;
				gluePointPos.Y += deltaY;
			} else {
				// If the gluePoint is connected and the shape is not
				// following the connected shape, recalculate GluePointCalcInfo
				if (!followingConnectedShape) {
					calcInfo = GluePointCalcInfo.Empty;
					CalcGluePointCalcInfo(ci.OwnPointId, ci.OtherShape, ci.OtherPointId);
					InvalidateDrawCache();
				}
			}
			return result;
		}


		protected override bool MovePointByCore(ControlPointId pointId, int origDeltaX, int origDeltaY, ResizeModifiers modifiers) {
			if (pointId == GlueControlPoint) {
				bool result = false;
				// If the glue ponit is connected, recalculate glue point calculation info
				ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
				if (ci.IsEmpty) {
					// If the glue point is not connected, move the glue point to the desired position
					if (gluePointPos != Geometry.InvalidPoint) {
						gluePointPos.X += origDeltaX;
						gluePointPos.Y += origDeltaY;
						InvalidateDrawCache();
						result = true;
					}
				} else {
					if (ci.OtherPointId != ControlPointId.Reference)
						CalcGluePoint(GlueControlPoint, ci.OtherShape);
				}
				return result;
			} else return base.MovePointByCore(pointId, origDeltaX, origDeltaY, modifiers);
		}


		protected override bool RotateCore(int deltaAngle, int x, int y) {
			bool result = base.RotateCore(deltaAngle, x, y);
			ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
			if (!ci.IsEmpty) {
				// If the gluePoint is connected, recalculate GluePointCalcInfo
				calcInfo = GluePointCalcInfo.Empty;
				CalcGluePointCalcInfo(ci.OwnPointId, ci.OtherShape, ci.OtherPointId);
				InvalidateDrawCache();
			}
			return result;
		}


		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			if (pinPath != null) {
				if (IsConnected(GlueControlPoint, null) != ControlPointId.None) {
					pinPath.Dispose();
					pinPath = null;
				} else pinPath.Reset();
			}
		}
		

		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);

			Matrix.Reset();
			Matrix.Translate(deltaX, deltaY);
			pinPath.Transform(Matrix);
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				// The pin path 
				if (pinPath == null) pinPath = new GraphicsPath();
				if (pinPath.PointCount <= 0) {
					int gpX = gluePointPos.X - X;
					int gpY = gluePointPos.Y - Y;

					int halfPinSize = pinSize / 2;
					pinPath.Reset();
					pinPath.StartFigure();
					pinPath.AddEllipse(gpX - halfPinSize, gpY - pinSize, pinSize, halfPinSize);
					pinPath.AddLine(gpX, gpY - halfPinSize, gpX, gpY);
					pinPath.CloseFigure();

					Point p = Point.Empty;
					p.X = gpX;
					p.Y = gpY;
					Matrix.Reset();
					Matrix.RotateAt(pinAngle, p);
					pinPath.Transform(Matrix);
					Matrix.Reset();
				}
				return true;
			} else return false;
		}


		protected override void CalcControlPoints() {
			base.CalcControlPoints();
			ControlPoints[ControlPointCount - 1] = gluePointPos;
		}


		protected override Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			// Get current position of the GluePoint and the new position of the GluePoint
			Point result = Geometry.InvalidPoint;
			ControlPointId pointId = IsConnected(gluePointId, shape);
			if (pointId == ControlPointId.Reference)
				result = shape.CalculateAbsolutePosition(calcInfo.RelativePosition);
			else result = shape.GetControlPointPosition(pointId);
			if (result == Geometry.InvalidPoint) System.Diagnostics.Debug.Fail("Unable to calculate glue point position!");
			return result;
		}


		public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			if (connectedShape == null) throw new ArgumentNullException("connectedShape");
			// Follow the connected shape
			try {
				followingConnectedShape = true;

				Point currGluePtPos = gluePointPos;
				Point newGluePtPos = Geometry.InvalidPoint;
				// If the connection is a point-to-shape connection, the shape calculates the new glue point position 
				// with the help of the connected shape. 
				if (movedPointId == ControlPointId.Reference)
					newGluePtPos = CalcGluePoint(gluePointId, connectedShape);
				else newGluePtPos = connectedShape.GetControlPointPosition(movedPointId);
				if (newGluePtPos == Geometry.InvalidPoint) newGluePtPos = currGluePtPos;

				// Move the glue point to the new position
				int dx, dy;
				dx = newGluePtPos.X - currGluePtPos.X;
				dy = newGluePtPos.Y - currGluePtPos.Y;
				if (dx != 0 || dy != 0) {
					// Calculate new target outline intersection point along with old and new anchor point position
					int shapeAngle;
					if (connectedShape is ILinearShape) {
						Point normalVector = ((ILinearShape)connectedShape).CalcNormalVector(newGluePtPos);
						shapeAngle = Geometry.RadiansToTenthsOfDegree(Geometry.Angle(newGluePtPos.X, newGluePtPos.Y, normalVector.X, normalVector.Y)) - 900;
					} else if (connectedShape is IPlanarShape) {
						shapeAngle = ((IPlanarShape)connectedShape).Angle;
					} else shapeAngle = 0; // There is no way to get an angle from a generic shape
					
					// Calculate new position of the GlueLabel (calculation method depands on the desired behavior)
					Point newCenter = Point.Round(Geometry.CalcPoint(newGluePtPos.X, newGluePtPos.Y, calcInfo.Alpha + Geometry.TenthsOfDegreeToDegrees(shapeAngle), calcInfo.Distance));
					// move GlueLabel
					MoveTo(newCenter.X, newCenter.Y);
					gluePointPos = newGluePtPos;
					if (!maintainOrientation) {
						int newAngle = calcInfo.LabelAngle + shapeAngle;
						if (Angle != newAngle) Angle = newAngle;
					}
				}
			} finally { followingConnectedShape = false; }
		}
		
		
		private void CalcGluePointCalcInfo(ControlPointId gluePointId, Shape otherShape, ControlPointId otherPointId) {
			// calculate GluePoint position and AnchorPoint position
			Point gluePtPos = GetControlPointPosition(gluePointId);
			Point labelPos = Point.Empty;
			labelPos.Offset(X, Y);
			int labelAngle;

			//calculate target shape's outline intersection point and the relative position of the gluePoint in/on the target shape
			float alpha = float.NaN, beta = float.NaN;
			if (otherShape is ILinearShape) {
				// ToDo: Check if the point is on the line, if not, calculate an intersection point
				Point normalVector = ((ILinearShape)otherShape).CalcNormalVector(gluePtPos);
				float shapeAngleDeg = Geometry.RadiansToDegrees(Geometry.Angle(gluePtPos.X, gluePtPos.Y, normalVector.X, normalVector.Y)) - 90;
				alpha = 360 - shapeAngleDeg + Geometry.RadiansToDegrees(Geometry.Angle(gluePtPos, labelPos));
				beta = Geometry.RadiansToDegrees(Geometry.Angle(labelPos, gluePtPos));
				labelAngle = Angle - Geometry.DegreesToTenthsOfDegree(shapeAngleDeg);
			} else if (otherShape is IPlanarShape) {
				float shapeAngleDeg = Geometry.TenthsOfDegreeToDegrees(((IPlanarShape)otherShape).Angle);
				alpha = 360 - shapeAngleDeg + Geometry.RadiansToDegrees(Geometry.Angle(gluePtPos, labelPos));
				beta = Geometry.RadiansToDegrees(Geometry.Angle(labelPos, gluePtPos));
				labelAngle = Angle - ((IPlanarShape)otherShape).Angle;
			} else {
				alpha = 360 - Geometry.RadiansToDegrees(Geometry.Angle(gluePtPos, labelPos));
				beta = Geometry.RadiansToDegrees(Geometry.Angle(labelPos, gluePtPos));
				labelAngle = Angle;
			}
			RelativePosition relativePos = otherShape.CalculateRelativePosition(gluePtPos.X, gluePtPos.Y);
			float distance = Geometry.DistancePointPoint(gluePtPos, labelPos);

			// store all calculated values in the GluePointCalcInfo structure
			calcInfo.Alpha = alpha % 360;
			calcInfo.Beta = beta % 360;
			calcInfo.Distance = distance;
			calcInfo.RelativePosition = relativePos;
			calcInfo.LabelAngle = labelAngle;
		}


		private struct GluePointCalcInfo {

			static GluePointCalcInfo() {
				Empty.Alpha = float.NaN;
				Empty.Beta = float.NaN;
				Empty.RelativePosition = RelativePosition.Empty;
				Empty.Distance = float.NaN;
				Empty.LabelAngle = 0;
			}

			public static readonly GluePointCalcInfo Empty;

			public static bool operator !=(GluePointCalcInfo x, GluePointCalcInfo y) { return !(x == y); }

			public static bool operator ==(GluePointCalcInfo x, GluePointCalcInfo y) {
				return (
					((float.IsNaN(x.Alpha) == float.IsNaN(y.Alpha) && float.IsNaN(x.Alpha))
					 || (x.Alpha == y.Alpha && !float.IsNaN(x.Alpha)))
					&& ((float.IsNaN(x.Beta) == float.IsNaN(y.Beta) && float.IsNaN(x.Beta))
					 || (x.Beta == y.Beta && !float.IsNaN(x.Beta)))
					&& x.RelativePosition == y.RelativePosition
					&& x.Distance == y.Distance);
			}

			/// <summary>
			/// Distance between the target shape's outline and the anchor point.
			/// </summary>
			public float Distance;

			/// <summary>
			/// The relative position of the GluePoint inside the target shape. The target shape can calculate the new absolute position of the gluepoint using the RelativePosition.
			/// </summary>
			public RelativePosition RelativePosition;

			/// <summary>
			/// Angle between the normal vector of the target shape's outline intersection and the line through GluePoint and X|Y
			/// </summary>
			public float Alpha;

			/// <summary>
			/// Angle between the normal vector of the GlueLabel's outline intersection and the line through GluePoint and X|Y
			/// </summary>
			public float Beta;

			/// <summary>
			/// The own rotation of the label
			/// </summary>
			public int LabelAngle;

			public override bool Equals(object obj) { return obj is GluePointCalcInfo && this == (GluePointCalcInfo)obj; }

			public override int GetHashCode() {
				return (Distance.GetHashCode()
					^ RelativePosition.GetHashCode()
					^ Alpha.GetHashCode()
					^ Beta.GetHashCode()
					^ LabelAngle.GetHashCode());
			}
		}


		#region Fields

		private const int GlueControlPoint = 10;
		private const int pinSize = 6;
		private const int pinAngle = -45;

		private Rectangle shapeBuffer = Rectangle.Empty;
		private GraphicsPath pinPath;

		// Specifies if the shape maintains its orientation if the connected shape is rotated
		private bool maintainOrientation = true;
		// Specifis if the movement of a connected label is due to repositioning or 
		// due to a "FollowConnectionPointWithGluePoint" call
		private bool followingConnectedShape = false;
		// Position of glue point (unrotated)
		private Point gluePointPos = Geometry.InvalidPoint;
		private GluePointCalcInfo calcInfo = GluePointCalcInfo.Empty;

		#endregion
	}

}
