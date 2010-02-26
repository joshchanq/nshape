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
using System.Drawing.Drawing2D;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Title shape.
	/// </summary>
	/// <remarks>RequiredPermissions set</remarks>
	public abstract class TextBase : RectangleBase {

		#region [Public] Properties

		/// <override></override>
		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				if (autoSize) FitShapeToText();
			}
		}


		[Category("Appearance")]
		[Description("Enables automatic resizing based on the text's size. If enabled, the WordWrap property of ParagraphStyles has no effect.")]
		[RequiredPermission(Permission.Layout)]
		public bool AutoSize {
			get { return autoSize; }
			set {
				Invalidate();
				autoSize = value;
				if (autoSize) {
					FitShapeToText();
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}

		#endregion


		#region [Public] Methods

		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is TextBase)
				this.AutoSize = ((TextBase)source).AutoSize;
		}


		public override void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			base.SetCaptionCharacterStyle(index, characterStyle);
			if (autoSize) FitShapeToText();
		}


		public override void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			base.SetCaptionParagraphStyle(index, paragraphStyle);
			if (autoSize) FitShapeToText();
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


		public override bool NotifyStyleChanged(IStyle style) {
			if (base.NotifyStyleChanged(style)) {
				if (autoSize && (style is ICharacterStyle || style is IParagraphStyle))
					FitShapeToText();
				return true;
			} else return false;
		}


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

		#endregion


		#region [Protected] Methods

		protected internal TextBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal TextBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			AutoSize = true;
			Text = "Text";
			// override standard Line- and FillStyles
			FillStyle = styleSet.FillStyles.Transparent;
			LineStyle = styleSet.LineStyles.None;
		}


		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			captionBounds = Rectangle.Empty;
			if (autoSize) {
				captionBounds.Size = TextMeasurer.MeasureText(Text, CharacterStyle, Size.Empty, ParagraphStyle);
				captionBounds.Width += ParagraphStyle.Padding.Horizontal;
				captionBounds.Height += ParagraphStyle.Padding.Vertical;
				captionBounds.X = (int)Math.Round(-captionBounds.Width / 2f);
				captionBounds.Y = (int)Math.Round(-captionBounds.Height / 2f);
			} else base.CalcCaptionBounds(index, out captionBounds);
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			if (autoSize && !string.IsNullOrEmpty(Text)) {
				RectangleF rectangle = RectangleF.Empty;
				rectangle.X = x;
				rectangle.Y = y;
				rectangle.Width = width;
				rectangle.Height = height;

				if (Angle % 900 == 0)
					return Geometry.RectangleIntersectsWithRectangle(rectangle, GetBoundingRectangle(true));
				else {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					Point tl = Point.Empty, tr = Point.Empty, br = Point.Empty, bl = Point.Empty;
					Rectangle layoutRectangle = Rectangle.Empty;
					CalcCaptionBounds(0, out layoutRectangle);
					Geometry.TransformRectangle(Center, Angle, layoutRectangle, out tl, out tr, out br, out bl);

					if (Geometry.RectangleContainsPoint(rectangle, tl) 
						|| Geometry.RectangleContainsPoint(rectangle, tr) 
						|| Geometry.RectangleContainsPoint(rectangle, bl) 
						|| Geometry.RectangleContainsPoint(rectangle, br))
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


		protected override void RecalcDrawCache() {
			base.RecalcDrawCache();
			if (autoSize) FitShapeToText();
		}


		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				Rectangle shapeBuffer = Rectangle.Empty;
				shapeBuffer.X = (int)Math.Round(-Width / 2f);
				shapeBuffer.Y = (int)Math.Round(-Height / 2f);
				shapeBuffer.Width = Width;
				shapeBuffer.Height = Height;

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(shapeBuffer);
				Path.CloseFigure();
				return true;
			}
			return false;
		}

		#endregion


		#region IEntity Members

		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("AutoSize", typeof(bool));
		}


		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			AutoSize = reader.ReadBool();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteBool(AutoSize);
		}

		#endregion


		private void FitShapeToText() {
			System.Diagnostics.Debug.Assert(CharacterStyle != null);
			System.Diagnostics.Debug.Assert(ParagraphStyle != null);
			if (!string.IsNullOrEmpty(Text) && CharacterStyle != null && ParagraphStyle != null) {
				Size textSize = TextMeasurer.MeasureText(Text, CharacterStyle, Size.Empty, ParagraphStyle);
				Width = textSize.Width + ParagraphStyle.Padding.Horizontal + ParagraphStyle.Padding.Horizontal;
				Height = textSize.Height + ParagraphStyle.Padding.Vertical + ParagraphStyle.Padding.Vertical;
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
		#endregion
	}


	public abstract class LabelBase : TextBase {

		#region [Public] Properties

		[Category("Layout")]
		[Description("If false, the label maintains its angle if it is attached to an other shape (in case the attached shape is beeing rotated).")]
		[RequiredPermission(Permission.Layout)]
		public bool MaintainOrientation {
			get { return maintainOrientation; }
			set {
				Invalidate();
				maintainOrientation = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}

		#endregion


		#region [Public] Shape Members

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


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if (controlPointId == GlueControlPoint)
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0 
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			else return base.HasControlPointCapability(controlPointId, controlPointCapability);
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


		public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			if (connectedShape == null) throw new ArgumentNullException("connectedShape");
			// Follow the connected shape
			try {
				followingConnectedShape = true;
				Point currGluePtPos = gluePointPos;
				Point newGluePtPos = Geometry.InvalidPoint;
				// If the connection is a point-to-shape connection, the shape calculates the new glue point position 
				// with the help of the connected shape. 
				if (movedPointId == ControlPointId.Reference) {
					newGluePtPos = CalcGluePoint(gluePointId, connectedShape);
				} else newGluePtPos = connectedShape.GetControlPointPosition(movedPointId);
				if (!Geometry.IsValid(newGluePtPos)) {
					System.Diagnostics.Debug.Fail("Invalid glue point position");
					newGluePtPos = currGluePtPos;
				}

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

					// Calculate new position of the GlueLabel (calculation method depends on the desired behavior)
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
//#if DEBUG
//         else {
//            // Draw Line to GluePoint
//            graphics.DrawLine(Pens.Green, gluePointPos, Center);
//            GdiHelpers.DrawPoint(graphics, Pens.Green, gluePointPos.X, gluePointPos.Y, 3);

//            ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
//            if (ci != ShapeConnectionInfo.Empty) {
//               Point absPos = ci.OtherShape.CalculateAbsolutePosition(calcInfo.RelativePosition);
//               GdiHelpers.DrawPoint(graphics, Pens.Red, absPos.X, absPos.Y, 3);
//               System.Diagnostics.Debug.Print("Relative Position: A = {0}, B = {1}", calcInfo.RelativePosition.A, calcInfo.RelativePosition.B);
//               System.Diagnostics.Debug.Print("Absolute Position: {0}", absPos);
//            }

//            ////ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
//            //float shapeAngle = 0;
//            //if (ci.OtherShape is ILinearShape) {
//            //   ILinearShape line = (ILinearShape)ci.OtherShape;
//            //   Point nv = line.CalcNormalVector(gluePointPos);
//            //   graphics.DrawLine(Pens.Red, gluePointPos, nv);
//            //   shapeAngle = Geometry.RadiansToTenthsOfDegree(Geometry.Angle(gluePointPos.X, gluePointPos.Y, nv.X, nv.Y)) - 900;
//            //} else if (ci.OtherShape is IPlanarShape) {
//            //   shapeAngle = Geometry.TenthsOfDegreeToDegrees(((IPlanarShape)ci.OtherShape).Angle);
//            //}
//            //shapeAngle = shapeAngle / 10f;
//            //GdiHelpers.DrawAngle(graphics, Brushes.Red, gluePointPos, shapeAngle, 10);
//            //GdiHelpers.DrawAngle(graphics, Brushes.Purple, gluePointPos, shapeAngle, calcInfo.Alpha, 10);

//            //string debugStr = string.Format("ShapeAngle: {0}°{1}Alpha: {2}°", shapeAngle, Environment.NewLine, calcInfo.Alpha);
//            //using (Font font = new Font("Arial", 8))
//            //   graphics.DrawString(debugStr, font, Brushes.Blue, gluePointPos);
//         }
//#endif
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

		#endregion


		#region [Protected] Methods (Inherited)

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
			maintainOrientation = true;
		}


		protected internal override int ControlPointCount {
			get { return base.ControlPointCount + 1; }
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = base.CalculateBoundingRectangle(tight);
			if (!tight && IsConnected(GlueControlPoint, null) == ControlPointId.None) {
				if (Geometry.IsValid(gluePointPos)) {
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
				if (Geometry.IsValid(gluePointPos)) {
					gluePointPos.X += deltaX;
					gluePointPos.Y += deltaY;
				}
			} else {
				// If the gluePoint is connected and the shape is not
				// following the connected shape, recalculate GluePointCalcInfo
				if (!followingConnectedShape) {
					calcInfo = GluePointCalcInfo.Empty;
					CalcGluePointCalcInfo(ci.OwnPointId, ci.OtherShape, ci.OtherPointId);
					//InvalidateDrawCache();
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
					if (Geometry.IsValid(gluePointPos)) {
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
			Debug.Assert(Geometry.IsValid(result));
			return result;
		}

		#endregion


		#region IEntity Members (Explicit Implementation)

		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			gluePointPos.X = reader.ReadInt32();
			gluePointPos.Y = reader.ReadInt32();
			if (version > 2) maintainOrientation = reader.ReadBool();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(gluePointPos.X);
			writer.WriteInt32(gluePointPos.Y);
			if (version > 2) writer.WriteBool(maintainOrientation);
		}


		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in TextBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("GluePointX", typeof(int));
			yield return new EntityFieldDefinition("GluePointY", typeof(int));
			if (version > 2) yield return new EntityFieldDefinition("MaintainOrientation", typeof(bool));
		}

		#endregion


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


		protected struct GluePointCalcInfo {

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

		protected const int GlueControlPoint = 10;
		private const int pinSize = 6;
		private const int pinAngle = -45;

		// Position of glue point (unrotated)
		protected Point gluePointPos = Geometry.InvalidPoint;
		protected GluePointCalcInfo calcInfo = GluePointCalcInfo.Empty;

		//private Rectangle shapeBuffer = Rectangle.Empty;
		private GraphicsPath pinPath;

		// Specifies if the shape maintains its orientation if the connected shape is rotated
		private bool maintainOrientation;
		// Specifis if the movement of a connected label is due to repositioning or 
		// due to a "FollowConnectionPointWithGluePoint" call
		private bool followingConnectedShape = false;
		#endregion
	}

}
