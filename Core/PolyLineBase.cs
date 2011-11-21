/******************************************************************************
  Copyright 2009-2011 dataweb GmbH
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
using System.Diagnostics;
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	/// <summary>
	/// Abstract base class for polylines.
	/// </summary>
	/// <remarks>RequiredPermissions set</remarks>
	public abstract class PolylineBase : LineShapeBase {

		#region Shape Members

		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is PolylineBase) {
				// Vertices and CapStyles will be copied by the base class
				// so there's nothing left to do here...
			}
		}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			Rectangle bounds = GetBoundingRectangle(true);
			// First, scale to the desired size
			//float scale;
			//scale = Geometry.CalcScaleFactor(bounds.Width, bounds.Height, width, height);

			// Second, move to the desired location
			Point topLeft = Point.Empty;
			topLeft.Offset(x, y);
			Point bottomRight = Point.Empty;
			bottomRight.Offset(x + width, y + height);
			MoveControlPointTo(ControlPointId.FirstVertex, topLeft.X, topLeft.Y, ResizeModifiers.None);
			MoveControlPointTo(ControlPointId.LastVertex, bottomRight.X, bottomRight.Y, ResizeModifiers.None);

			int ptNr = 0;
			foreach (ControlPointId ptId in GetControlPointIds(ControlPointCapabilities.Resize)) {
				if (IsFirstVertex(ptId) || IsLastVertex(ptId)) continue;
				ptNr = 1;
				Point pos = GetControlPointPosition(ptId);
				pos = Geometry.VectorLinearInterpolation(topLeft, bottomRight, ptNr / (float)(VertexCount - 1));
				MoveControlPointTo(ptId, pos.X, pos.Y, ResizeModifiers.None);

				//MoveControlPointTo(ptId, (int)Math.Round(pos.X * scale), (int)Math.Round(pos.Y * scale), ResizeModifiers.None);
			}
			InvalidateDrawCache();
		}


		/// <override></override>
		public override int X {
			get { return vertices[0].X; }
			set {
				int origValue = vertices[0].X;
				if (!MoveTo(value, Y)) MoveTo(origValue, Y);
			}
		}


		/// <override></override>
		public override int Y {
			get { return vertices[0].Y; }
			set {
				int origValue = vertices[0].Y;
				if (!MoveTo(X, value)) MoveTo(X, origValue);
			}
		}


		/// <override></override>
		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			// The RelativePosition of a PolyLine is defined as
			// A = ControlPointId of the first vertex of the nearest line segment (FirstVertex -> LastVertex)
			// B = Angle between the line segment (A / next vertex of A) and the point
			// C = Distance of the point from A in percentage of the line segment's length
			Point result = Point.Empty;
			int idx = GetControlPointIndex(relativePosition.A);
			float angle = Geometry.TenthsOfDegreeToDegrees(relativePosition.B);
			float distance = relativePosition.C / 1000f;
			if (distance != 0) {
				float segmentLength = Geometry.DistancePointPoint(vertices[idx], vertices[idx + 1]);
				Point p = Geometry.VectorLinearInterpolation(vertices[idx], vertices[idx + 1], (distance));
				result = Geometry.RotatePoint(vertices[idx], angle, p);
				Debug.Assert(Geometry.IsValid(result));
			} else result = vertices[idx];
			return result;
		}


		/// <override></override>
		public override RelativePosition CalculateRelativePosition(int x, int y) {
			// The RelativePosition of a PolyLine is defined as
			// A = ControlPointId of the first vertex of the nearest line segment (FirstVertex -> LastVertex)
			// B = Angle between the line segment (A / next vertex of A) and the point
			// C = Distance of the point from A in percentage of the line segment's length
			RelativePosition result = RelativePosition.Empty;
			Point p = Point.Empty;
			p.Offset(x, y);

			// Find the nearest line segment
			int pointIdx = -1;
			float angleFromA = float.NaN;
			float distanceFromA = float.NaN;
			float lowestAbsDistance = float.MaxValue;
			float lineWidth = LineStyle.LineWidth / 2f + 2;
			int cnt = vertices.Count - 1;
			for (int i = 0; i < cnt; ++i) {
				float dist = Geometry.DistancePointLine(p, vertices[i], vertices[i + 1], true);
				if (dist < lowestAbsDistance) {
					lowestAbsDistance = dist;
					pointIdx = i;
					if (dist > lineWidth)
						angleFromA = ((360 + Geometry.RadiansToDegrees(Geometry.Angle(vertices[i], vertices[i + 1], p))) % 360);
					else angleFromA = 0;
					distanceFromA = Geometry.DistancePointPoint(vertices[i], p);
				}
			}

			if (pointIdx >= 0) {
				result.A = GetControlPointId(pointIdx);
				result.B = Geometry.DegreesToTenthsOfDegree(angleFromA);
				float segmentLength = Geometry.DistancePointPoint(vertices[pointIdx], vertices[pointIdx + 1]);
				if (segmentLength == 0) result.C = 0;
				else result.C = (int)Math.Round((distanceFromA / (segmentLength / 100)) * 10);
				Debug.Assert(result.B >= 0 && result.B <= 3600, "Calculated angle is out of range.");
			}
			if (result == RelativePosition.Empty) result.A = result.B = result.C = 0;
			return result;
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int fromX, int fromY) {
			// Tries to calculate a perpendicular intersection point
			Point result = Geometry.InvalidPoint;
			// find nearest line segment
			Point fromP = Point.Empty;
			fromP.Offset(fromX, fromY);
			int pt1Idx = -1, pt2Idx = -1;
			float lowestDistance = float.MaxValue;
			for (int i = vertices.Count - 2; i >= 0; --i) {
				float distance = Geometry.DistancePointLine(fromP, vertices[i], vertices[i + 1], true);
				if (distance < lowestDistance) {
					lowestDistance = distance;
					pt1Idx = i;
					pt2Idx = i + 1;
				}
			}
			// If a segment was found, calculate the intersection point of the segment with the perpendicular 
			// line through point fromX/fromY
			if (pt1Idx >= 0 && pt2Idx >= 0) {
				Point p1 = GetControlPointPosition(GetControlPointId(pt1Idx));
				Point p2 = GetControlPointPosition(GetControlPointId(pt2Idx));
				float aSeg, bSeg, cSeg;
				// Calculate line equation for the line segment
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out aSeg, out bSeg, out cSeg);
				// Translate the line to point fromX/fromY:
				float cFrom = -((aSeg * fromP.X) + (bSeg * fromP.Y));
				// Calculate perpendicular line through fromX/fromY
				float aPer, bPer, cPer;
				Geometry.CalcPerpendicularLine(fromX, fromY, aSeg, bSeg, cFrom, out aPer, out bPer, out cPer);
				// intersect perpendicular line with line segment

				float resX, resY;
				if (Geometry.IntersectLineWithLineSegment(aPer, bPer, cPer, p1, p2, out resX, out resY)) {
					result.X = (int)Math.Round(resX);
					result.Y = (int)Math.Round(resY);
				} else {
					// if the lines do not intersect, return the nearest point of the line segment
					result = Geometry.GetNearestPoint(fromP, p1, p2);
				}
			}
			return result;
		}


		/// <override></override>
		[Obsolete]
		public override Point CalculateConnectionFoot(int x1, int y1, int x2, int y2) {
			Point result = Geometry.InvalidPoint;
			float distance;
			float lowestIntersectionDistance = float.MaxValue;
			float lowestPointDistance = float.MaxValue;
			// find (nearest) intersection point between lines
			for (int i = VertexCount - 2; i >= 0; --i) {
				Point p = Geometry.IntersectLineWithLineSegment(x1, y1, x2, y2, vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y);
				if (Geometry.IsValid(p)) {
					distance = Math.Max(Geometry.DistancePointPoint(p.X, p.Y, x1, y1), Geometry.DistancePointPoint(p.X, p.Y, x2, y2));
					if (distance < lowestIntersectionDistance) {
						result = p;
						lowestIntersectionDistance = distance;
					}
				}
			}
			// if there is no intersection with any of the line's segments, return coordinates of the nearest point
			if (!Geometry.IsValid(result)) {
				for (int i = VertexCount - 2; i >= 0; --i) {
					distance = Geometry.DistancePointLine(vertices[i].X, vertices[i].Y, x1, y1, x2, y2, true);
					if (distance < lowestPointDistance) {
						result = vertices[i];
						lowestPointDistance = distance;
					}
					distance = Geometry.DistancePointLine(vertices[i + 1].X, vertices[i + 1].Y, x1, y1, x2, y2, true);
					if (distance < lowestPointDistance) {
						result = vertices[i + 1];
						lowestPointDistance = distance;
					}
				}
				if (!Geometry.IsValid(result))
					result = vertices[0]; // this should never happen
			}
			Debug.Assert(Geometry.IsValid(result));
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if (controlPointCapability == ControlPointCapabilities.Connect
				&& !IsConnectionPointEnabled(controlPointId))
				return false;
			if (base.HasControlPointCapability(controlPointId, controlPointCapability))
				return true;
			//// Check special values (done by base class)
			//if (controlPointId == ControlPointId.Reference)
			//   return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
			//      || (controlPointCapability & ControlPointCapabilities.Resize) != 0
			//      || (controlPointCapability & ControlPointCapabilities.Connect) != 0);
			//if (controlPointId == ControlPointId.None || controlPointId == ControlPointId.Any)
			//   return base.HasControlPointCapability(controlPointId, controlPointCapability);

			// Check 'real' points
			int controlPointIndex = GetControlPointIndex(controlPointId);
			if (controlPointIndex == 0)
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0
					|| (controlPointCapability & ControlPointCapabilities.Reference) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			else if (controlPointIndex == VertexCount - 1)
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			else
				return ((controlPointCapability & ControlPointCapabilities.Connect) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
		}


		/// <override></override>
		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			// We count from start to end here because ControlPointId.FirstVertex and ControlPointId.Reference
			// refer to the same physical point. If we would process the vertices from last to first, the 
			// hit test 
			int j, maxIdx;
			maxIdx = VertexCount - 1;
			for (int i = maxIdx; i > 0; --i) {
				j = i - 1;
				if (Geometry.DistancePointPoint(x, y, vertices[i].X, vertices[i].Y) <= range) {
					ControlPointId ptId = GetControlPointId(i);
					if (HasControlPointCapability(ptId, controlPointCapability)) return ptId;
				}
				if (Geometry.DistancePointPoint(x, y, vertices[j].X, vertices[j].Y) <= range) {
					ControlPointId ptId = GetControlPointId(j);
					if (HasControlPointCapability(ptId, controlPointCapability)) return ptId;
				}
				float d = Geometry.DistancePointLine(x, y, vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, true);
				if (d <= (LineStyle.LineWidth / 2f) + range) {
					if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability)
						&& !(Geometry.DistancePointPoint(x, y, vertices[0].X, vertices[0].Y) <= range)
						&& !(Geometry.DistancePointPoint(x, y, vertices[maxIdx].X, vertices[maxIdx].Y) <= range))
						return ControlPointId.Reference;
				}
			}
			return ControlPointId.None;
		}


		/// <override></override>
		public override Point CalcNormalVector(Point point) {
			Point result = Geometry.InvalidPoint;
			for (int i = VertexCount - 1; i > 0; --i) {
				if (Geometry.LineContainsPoint(vertices[i].X, vertices[i].Y, vertices[i - 1].X, vertices[i - 1].Y, true, point.X, point.Y, LineStyle.LineWidth + 2)) {
					float lineAngle = Geometry.RadiansToDegrees(Geometry.Angle(vertices[i - 1], vertices[i]));
					int x = point.X + 100;
					int y = point.Y;
					Geometry.RotatePoint(point.X, point.Y, lineAngle + 90, ref x, ref y);
					result.X = x;
					result.Y = y;
					return result;
				}
			}
			if (!Geometry.IsValid(result)) throw new NShapeException("The given Point is not part of the line shape.");
			return result;

			//Point result = Geometry.InvalidPoint;
			//float delta = LineStyle.LineWidth + 2;
			//float dist, lowestDist = float.MaxValue;
			//int lowestIdx = -1, resultIdx = -1;
			//for (int i = VertexCount - 1; i > 0; --i) {
			//   dist = Geometry.DistancePointLine(point.X, point.Y, vertices[i].X, vertices[i].Y, vertices[i - 1].X, vertices[i - 1].Y, true);
			//   if (dist < lowestDist) {
			//      lowestDist = dist;
			//      lowestIdx = i;
			//      if (dist <= delta) resultIdx = i;
			//   }
			//}
			//if (resultIdx < 0) resultIdx = lowestIdx;
			//float lineAngle = Geometry.RadiansToDegrees(Geometry.Angle(vertices[resultIdx - 1], vertices[resultIdx]));
			//int x = point.X + 100;
			//int y = point.Y;
			//Geometry.RotatePoint(point.X, point.Y, lineAngle + 90, ref x, ref y);
			//result.X = x;
			//result.Y = y;
			//if (!Geometry.IsValid(result)) throw new NShapeException("The given Point is not part of the line shape.");
			//return result;
		}


		/// <override></override>
		public override void Invalidate() {
			base.Invalidate();
			if (DisplayService != null) {
				int j;
				for (int i = VertexCount - 2; i >= 0; --i) {
					j = i + 1;
					InvalidateSegment(vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y);
				}
			}
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			UpdateDrawCache();
			int lastIdx = shapePoints.Length - 1;
			if (lastIdx > 0) {
				// draw Caps interior
				DrawStartCapBackground(graphics, shapePoints[0].X, shapePoints[0].Y);
				DrawEndCapBackground(graphics, shapePoints[lastIdx].X, shapePoints[lastIdx].Y);

				// draw Line
				Pen pen = ToolCache.GetPen(LineStyle, StartCapStyleInternal, EndCapStyleInternal);
				graphics.DrawLines(pen, shapePoints);

				// ToDo: If the line is connected to another line, draw a connection indicator (ein Bommel oder so)
				// ToDo: Add a property for enabling/disabling this feature
			}
			base.Draw(graphics);
		}


		/// <override></override>
		public override void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			base.DrawOutline(graphics, pen);
			graphics.DrawLines(pen, shapePoints);
		}


		/// <override></override>
		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			if (image == null) throw new ArgumentNullException("image");
			using (Graphics g = Graphics.FromImage(image)) {
				GdiHelpers.ApplyGraphicsSettings(g, RenderingQuality.MaximumQuality);
				g.Clear(transparentColor);

				int startCapSize = 0;
				if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
					startCapSize = StartCapStyleInternal.CapSize;
				int endCapSize = 0;
				if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
					endCapSize = EndCapStyleInternal.CapSize;

				int height, width;
				height = width = (int)Math.Max(startCapSize, endCapSize) * 4;
				if (height == 0 || width == 0) {
					width = image.Width;
					height = image.Height;
				}
				g.ScaleTransform((float)image.Width / width, (float)image.Height / height);

				Point[] points = new Point[4] {
					new Point(margin, height / 4),
					new Point(width / 2, height / 4),
					new Point(width / 2, height - (height / 4)),
					new Point(width - margin, height - (height / 4))
				};

				Pen pen = ToolCache.GetPen(LineStyle, StartCapStyleInternal, EndCapStyleInternal);
				g.DrawLines(pen, points);
			}
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
		}


		/// <override></override>
		protected internal override IEnumerable<Point> CalculateCells(int cellSize) {
			// Calculate cells occupied by children
			if (ChildrenCollection != null) {
				foreach (Shape shape in ChildrenCollection.BottomUp)
					foreach (Point cell in shape.CalculateCells(cellSize))
						yield return cell;
			}

			// Calculate cells occupied by the line caps
			Rectangle startCapCells = Geometry.InvalidRectangle;
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None) {
				startCapCells.X = StartCapBounds.Left / cellSize;
				startCapCells.Y = StartCapBounds.Top / cellSize;
				startCapCells.Width = (StartCapBounds.Right / cellSize) - startCapCells.X;
				startCapCells.Height = (StartCapBounds.Bottom / cellSize) - startCapCells.Y;
				Point p = Point.Empty;
				for (p.X = startCapCells.Left; p.X <= startCapCells.Right; p.X += 1)
					for (p.Y = startCapCells.Top; p.Y <= startCapCells.Bottom; p.Y += 1)
						yield return p;
			}
			Rectangle endCapCells = Geometry.InvalidRectangle;
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None) {
				endCapCells.X = EndCapBounds.Left / cellSize;
				endCapCells.Y = EndCapBounds.Top / cellSize;
				endCapCells.Width = (EndCapBounds.Right / cellSize) - endCapCells.X;
				endCapCells.Height = (EndCapBounds.Bottom / cellSize) - endCapCells.Y;
				Point p = Point.Empty;
				for (p.X = endCapCells.Left; p.X <= endCapCells.Right; p.X += 1) {
					for (p.Y = endCapCells.Top; p.Y <= endCapCells.Bottom; p.Y += 1) {
						// Skip all cells occupied by the startCap's bounds
						if (Geometry.IsValid(startCapCells) && Geometry.RectangleContainsPoint(startCapCells, p))
							continue;
						yield return p;
					}
				}
			}
			// Instantiate the delegate using an anonymous method
			CellProcessedDelegate cellProcessed = delegate(Point cell) {
				return (Geometry.IsValid(startCapCells) && Geometry.RectangleContainsPoint(startCapCells, cell)
					|| Geometry.IsValid(endCapCells) && Geometry.RectangleContainsPoint(endCapCells, cell));
			};

			Point startCell = Point.Empty;
			Point endCell = Point.Empty;
			int j;
			for (int i = VertexCount - 1; i > 0; --i) {
				j = i - 1;
				// Calculate start- and end-cell
				//
				// Optimization:
				// Use integer division for values >= 0 (>20 times faster than floored float divisions)
				// Use floored float division for values < 0 (otherwise calculating intersection with cell 
				// bounds will not work collectly)
				if (vertices[i].X >= 0) startCell.X = vertices[i].X / cellSize;
				else startCell.X = (int)Math.Floor(vertices[i].X / (float)cellSize);
				if (vertices[i].Y >= 0) startCell.Y = vertices[i].Y / cellSize;
				else startCell.Y = (int)Math.Floor(vertices[i].Y / (float)cellSize);
				if (!cellProcessed(startCell))
					yield return startCell;
				if (vertices[j].X >= 0) endCell.X = vertices[j].X / cellSize;
				else endCell.X = (int)Math.Floor(vertices[j].X / (float)cellSize);
				if (vertices[j].Y >= 0) endCell.Y = vertices[j].Y / cellSize;
				else endCell.Y = (int)Math.Floor(vertices[j].Y / (float)cellSize);

				// If the segment end is in the same cell, continue with the next segment...
				if (startCell == endCell) continue;
				// ...otherwise return the end cell
				else if (!cellProcessed(endCell))
					yield return endCell;

				Point p = Point.Empty;
				if (startCell.X == endCell.X || startCell.Y == endCell.Y) {
					// Intersection test is not necessary
					p.Offset(Math.Min(startCell.X, endCell.X), Math.Min(startCell.Y, endCell.Y));
					int endX = Math.Max(startCell.X, endCell.X);
					int endY = Math.Max(startCell.Y, endCell.Y);
					if (startCell.Y == endCell.Y)
						while (++p.X < endX) {
							// Check if the cell has already been processed
							if (cellProcessed(p)) continue;
							yield return p;
						} else while (++p.Y < endY) {
							if (cellProcessed(p)) continue;
							yield return p;
						}
				} else {
					// calculate processing direction
					int stepX = (startCell.X < endCell.X) ? 1 : -1;
					int stepY = (startCell.Y < endCell.Y) ? 1 : -1;

					p = startCell;
					int xFrom, yFrom, xTo, yTo;
					bool endCellReached = false;
					do {
						// calculate cell bounds
						if (stepX > 0) {
							xFrom = p.X * cellSize;
							xTo = xFrom + cellSize;
						} else {
							xTo = p.X * cellSize;
							xFrom = xTo + cellSize;
						}
						if (stepY > 0) {
							yFrom = p.Y * cellSize;
							yTo = yFrom + cellSize;
						} else {
							yTo = p.Y * cellSize;
							yFrom = yTo + cellSize;
						}
						// Check vertical and horizontal intersection
						bool verticalIntersection = Geometry.LineSegmentIntersectsWithLineSegment(xTo, yFrom, xTo, yTo,
								vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y);
						bool horizontalIntersection = Geometry.LineSegmentIntersectsWithLineSegment(xFrom, yTo, xTo, yTo,
								vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y);
						if (verticalIntersection && horizontalIntersection) {
							// return nearby cells
							Point nextHCell = p, nextVCell = p;
							// Return nearby horiziontal cell
							nextHCell.X += stepX;
							if (nextHCell != endCell) {
								if (!cellProcessed(nextHCell))
									yield return nextHCell;
							} else endCellReached = true;
							// Return nearby vertical cell
							nextVCell.Y += stepY;
							if (nextVCell != endCell) {
								if (!cellProcessed(nextVCell))
									yield return nextVCell;
							} else endCellReached = true;
							// Return next diagonal cell
							p.Offset(stepX, stepY);
							if (p != endCell) {
								if (!cellProcessed(p))
									yield return p;
							} else endCellReached = true;
						} else if (verticalIntersection) {
							p.X += stepX;
							if (p != endCell) {
								if (!cellProcessed(p))
									yield return p;
							} else endCellReached = true;
						} else if (horizontalIntersection) {
							p.Y += stepY;
							if (p != endCell) {
								if (!cellProcessed(p))
									yield return p;
							} else endCellReached = true;
						} else {
							// This should never happen!
							Debug.Fail("Error while calculating cells: Line does not intersect expected cell borders!");
							endCellReached = true;	// Prevent endless loop
						}
					} while (!endCellReached);
				}
			}
		}

		#endregion


		#region ILinearShape Members

		/// <override></override>
		public override int MinVertexCount { get { return 2; } }


		/// <override></override>
		public override int MaxVertexCount { get { return int.MaxValue; } }


		/// <override></override>
		public override ControlPointId InsertVertex(ControlPointId beforePointId, int x, int y) {
			Invalidate();
			ControlPointId newPointId = ControlPointId.None;
			if (IsFirstVertex(beforePointId) || beforePointId == ControlPointId.Reference || beforePointId == ControlPointId.None)
				throw new ArgumentException(string.Format("{0} is not a valid control point id for this operation.", beforePointId));

			// find position where to insert the new point
			int pointIndex = GetControlPointIndex(beforePointId);
			if (pointIndex < 0 || pointIndex > VertexCount || pointIndex > MaxVertexCount)
				throw new IndexOutOfRangeException();

			// Create new vertex
			Point p = Point.Empty;
			p.Offset(x, y);
			vertices.Insert(pointIndex, p);

			// create a new PointId
			newPointId = GetNewControlPointId();
			pointIds.Insert(pointIndex, newPointId);

			InvalidateDrawCache();
			Invalidate();
			return newPointId;
		}


		/// <override></override>
		public override ControlPointId AddVertex(int x, int y) {
			Invalidate();
			// find segment where the new point has to be inserted
			ControlPointId ptId = ControlPointId.None;
			for (int i = 0; i < this.ControlPointCount - 1; ++i) {
				int startX, startY, endX, endY;
				GetSegmentCoordinates(i, out startX, out startY, out endX, out endY);

				if (Geometry.LineContainsPoint(startX, startY, endX, endY, true, x, y, LineStyle.LineWidth + 2)) {
					// ToDo: Falls die Distanz des Punktes x|y > 0 ist: Ausrechnen wo der Punkt sein muss (entlang der Lotrechten durch den Punkt verschieben)
					ControlPointId id = GetControlPointId(i + 1);
					ptId = InsertVertex(id, x, y);
					break;
				}
			}
			if (ptId == ControlPointId.None) throw new NShapeException("Cannot add vertex {0}.", new Point(x, y));
			Invalidate();
			return ptId;
		}


		/// <override></override>
		public override void RemoveVertex(ControlPointId controlPointId) {
			Invalidate();
			if (controlPointId == ControlPointId.Any || controlPointId == ControlPointId.Reference || controlPointId == ControlPointId.None)
				throw new ArgumentException(string.Format("{0} is not a valid ControlPointId for this operation.", controlPointId));
			if (IsFirstVertex(controlPointId) || IsLastVertex(controlPointId))
				throw new InvalidOperationException(string.Format("ControlPoint {0} is a GluePoint and therefore must not be removed.", controlPointId));

			// remove shape point
			int controlPointIndex = GetControlPointIndex(controlPointId);
			vertices.RemoveAt(controlPointIndex);
			// remove connectionPointId
			pointIds.Remove(controlPointId);

			InvalidateDrawCache();
			Invalidate();
		}

		#endregion


		#region [Protected] Methods

		/// <ToBeCompleted></ToBeCompleted>
		protected internal PolylineBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			// nothing to do
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal PolylineBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			// nothing to do
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.X = x;
			rectangle.Y = y;
			rectangle.Width = width;
			rectangle.Height = height;

			if (StartCapIntersectsWith(rectangle))
				return true;
			if (EndCapIntersectsWith(rectangle))
				return true;
			int cnt = VertexCount - 1;
			for (int i = 0; i < cnt; ++i) {
				if (Geometry.RectangleIntersectsWithLine(rectangle, vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y, true))
					return true;
			}
			return false;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			if (base.ContainsPointCore(x, y))
				return true;
			int cnt = VertexCount - 1;
			for (int i = 0; i < cnt; ++i) {
				if (Geometry.LineContainsPoint(vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y, true, x, y, (LineStyle.LineWidth / 2f) + 2))
					return true;
			}
			return false;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			if (deltaX == 0 && deltaY == 0) return true;

			Rectangle boundsBefore = Rectangle.Empty;
			if ((modifiers & ResizeModifiers.MaintainAspect) == ResizeModifiers.MaintainAspect)
				boundsBefore = GetBoundingRectangle(true);

			int vertexIdx = GetControlPointIndex(pointId);
			Point p = vertices[vertexIdx];
			p.Offset(deltaX, deltaY);
			vertices[vertexIdx] = p;

			// Scale line if MaintainAspect flag is set and start- or endpoint was moved
			if ((modifiers & ResizeModifiers.MaintainAspect) == ResizeModifiers.MaintainAspect
				&& (IsFirstVertex(pointId) || IsLastVertex(pointId))) {
				// ToDo: Improve maintaining aspect of polylines
				if (deltaX != 0 || deltaY != 0) {
					int dx = (int)Math.Round(deltaX / (float)(VertexCount - 1));
					int dy = (int)Math.Round(deltaY / (float)(VertexCount - 1));
					// The first and the last points are glue points, so move only the points between
					for (int i = VertexCount - 2; i > 0; --i) {
						p = vertices[i];
						p.Offset(dx, dy);
						vertices[i] = p;
					}
					// After moving the vertices between the first and the last vertex, 
					// we have to maintain the glue point positions again
					MaintainGluePointPosition(ControlPointId.LastVertex, GetPreviousVertexId(ControlPointId.LastVertex));
					MaintainGluePointPosition(ControlPointId.FirstVertex, GetNextVertexId(ControlPointId.FirstVertex));
				}
			} else {
				// Maintain glue point positions (if connected via "Point-To-Shape"
				MaintainGluePointPosition(ControlPointId.FirstVertex, pointId);
				MaintainGluePointPosition(ControlPointId.LastVertex, pointId);
			}
			return true;
		}


		/// <override></override>
		protected override Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			// Get the second point of the line segment that should intersect with the passive shape's outline
			ControlPointId secondPtId = gluePointId;
			if (IsFirstVertex(gluePointId))
				secondPtId = GetNextVertexId(gluePointId);
			else if (IsLastVertex(gluePointId))
				secondPtId = GetPreviousVertexId(gluePointId);
			//
			Point pointPosition = Geometry.InvalidPoint;
			// If the line only has 2 vertices and both are connected via Point-To-Shape connection...
			if (VertexCount == 2
				&& IsConnected(ControlPointId.FirstVertex, null) == ControlPointId.Reference
				&& IsConnected(ControlPointId.LastVertex, null) == ControlPointId.Reference) {
				// ... calculate new point position from the position of the second shape:
				Shape secondShape = GetConnectionInfo(secondPtId, null).OtherShape;
				if (secondShape is IPlanarShape) {
					pointPosition.X = secondShape.X;
					pointPosition.Y = secondShape.Y;
				}
			}
			if (!Geometry.IsValid(pointPosition))
				// Calculate new glue point position of the moved GluePoint by calculating the intersection point
				// of the passive shape's outline with the line segment from GluePoint to NextPoint/PrevPoint of GluePoint
				pointPosition = GetControlPointPosition(secondPtId);

			return CalcGluePointFromPosition(gluePointId, shape, pointPosition.X, pointPosition.Y);
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// Calculate line caps' bounds
			Rectangle capBounds = base.CalculateBoundingRectangle(tight);
			Rectangle result = Rectangle.Empty;
			Geometry.CalcBoundingRectangle(vertices, out result);
			result.Width = Math.Max(result.Width, LineStyle.LineWidth);
			result.Height = Math.Max(result.Height, LineStyle.LineWidth);
			if (!capBounds.IsEmpty) result = Geometry.UniteRectangles(result, capBounds);
			return result;
		}


		/// <override></override>
		protected override float CalcCapAngle(ControlPointId pointId) {
			float result = float.NaN;
			ControlPointId otherPointId;
			float capInset;
			int step, endIdx;

			// Get required infos
			Pen pen = ToolCache.GetPen(LineStyle, StartCapStyleInternal, EndCapStyleInternal);
			if (IsFirstVertex(pointId)) {
				otherPointId = GetNextVertexId(ControlPointId.FirstVertex);
				capInset = pen.CustomStartCap.BaseInset;
				step = 1;
				endIdx = VertexCount - 1;
			} else if (IsLastVertex(pointId)) {
				otherPointId = GetPreviousVertexId(ControlPointId.LastVertex);
				capInset = pen.CustomEndCap.BaseInset;
				step = -1;
				endIdx = 0;
			} else throw new NotSupportedException();
			int capPtIdx = GetControlPointIndex(pointId);
			int otherPtIdx = GetControlPointIndex(otherPointId);

			// Calculate cap angle
			if (Geometry.DistancePointPoint(vertices[capPtIdx], vertices[otherPtIdx]) < capInset) {
				// if the second point of the line is inside the cap, calculate the intersection point of the 
				// cap shape with the line (as GDI+ does automatically)
				int i = capPtIdx;
				endIdx = endIdx - step;
				while (i != endIdx) {
					int j = i + step;
					PointF p = Geometry.IntersectCircleWithLine(vertices[capPtIdx].X, vertices[capPtIdx].Y, capInset, vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, true);
					if (Geometry.IsValid(p) && Geometry.LineContainsPoint(vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, false, p.X, p.Y))
						result = Geometry.RadiansToDegrees(Geometry.Angle(vertices[capPtIdx].X, vertices[capPtIdx].Y, p.X, p.Y));
					i = i + step;
				}
			}
			if (float.IsNaN(result))
				result = Geometry.RadiansToDegrees(Geometry.Angle(vertices[capPtIdx].X, vertices[capPtIdx].Y, vertices[otherPtIdx].X, vertices[otherPtIdx].Y));
			Debug.Assert(!float.IsNaN(result));
			return result;
		}


		/// <override></override>
		protected override void RecalcDrawCache() {
			// Calculate shape points (relative to origin of coordinates)
			int vertexCount = vertices.Count;
			if (shapePoints.Length != vertexCount)
				Array.Resize(ref shapePoints, vertexCount);
			for (int i = 0; i < vertexCount; ++i) {
				shapePoints[i].X = vertices[i].X - X;
				shapePoints[i].Y = vertices[i].Y - Y;
			}
			// Calculate line caps and set drawCacheIsInvalid flag to false;
			base.RecalcDrawCache();
		}


		/// <summary>
		/// Retrieve a new PointId for a new point
		/// </summary>
		/// <returns></returns>
		protected virtual ControlPointId GetNewControlPointId() {
			for (int id = 1; id <= VertexCount; ++id) {
				if (pointIds.IndexOf(id) < 0)
					return id;
			}
			return VertexCount + 1;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void GetSegmentCoordinates(int segmentIndex, out int startPointX, out int startPointY, out int endPointX, out int endPointY) {
			startPointX = startPointY = endPointX = endPointY = -1;
			if (segmentIndex < 0 || segmentIndex >= VertexCount - 1) throw new IndexOutOfRangeException();
			startPointX = vertices[segmentIndex].X;
			startPointY = vertices[segmentIndex].Y;
			endPointX = vertices[segmentIndex + 1].X;
			endPointY = vertices[segmentIndex + 1].Y;
		}

		#endregion


		#region [Private] Methods

		private void InvalidateSegment(int segmentIndex) {
			int startX, startY, endX, endY;
			if (segmentIndex < VertexCount - 1) {
				GetSegmentCoordinates(segmentIndex, out startX, out startY, out endX, out endY);
				InvalidateSegment(startX, startY, endX, endY);
			}
		}


		private void InvalidatePointSegments(ControlPointId controlPointId) {
			if (IsFirstVertex(controlPointId)) {
				InvalidateSegment(vertices[0].X, vertices[0].Y, vertices[1].X, vertices[1].Y);
			} else if (IsLastVertex(controlPointId)) {
				InvalidateSegment(vertices[VertexCount - 1].X, vertices[VertexCount - 1].Y, vertices[VertexCount - 2].X, vertices[VertexCount - 2].Y);
			} else {
				int pointIndex = GetControlPointIndex(controlPointId);
				InvalidateSegment(vertices[pointIndex - 1].X, vertices[pointIndex - 1].Y, vertices[pointIndex].X, vertices[pointIndex].Y);
				InvalidateSegment(vertices[pointIndex + 1].X, vertices[pointIndex + 1].Y, vertices[pointIndex].X, vertices[pointIndex].Y);
			}
		}


		private void InvalidateSegment(int x1, int y1, int x2, int y2) {
			if (DisplayService != null) {
				int xMin, xMax, yMin, yMax;
				xMin = Math.Min(x1, x2);
				yMin = Math.Min(y1, y2);
				xMax = Math.Max(x1, x2);
				yMax = Math.Max(y1, y2);
				int margin = 1;
				if (LineStyle != null) margin = LineStyle.LineWidth + 1;
				DisplayService.Invalidate(xMin - margin, yMin - margin, (xMax - xMin) + (margin + margin), (yMax - yMin) + (margin + margin));
			}
		}


		// Delegate for checking wether CalculateCells has processed a certain cell or not.
		private delegate bool CellProcessedDelegate(Point p);

		#endregion

	}

}
