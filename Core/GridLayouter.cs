using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Dataweb.Utilities;
using Dataweb.nShape.Advanced;


namespace Dataweb.nShape.Layouters {

	/// <summary>
	/// Positions all shapes on the nodes of a rectangular grid.
	/// </summary>
	/// <remarks>Grid layouter looks for the most natural grid distances for the given
	/// shapes. These distances must be small enough to provide a different node for
	/// each shape and large enough to look tidy.</remarks>
	public class GridLayouter : LayouterBase, ILayouter {

		public GridLayouter(Project project)
			: base(project) {
		}


		public int CoarsenessX {
			get { return coarsenessX; }
			set { coarsenessX = value; }
		}


		public int CoarsenessY {
			get { return coarsenessY; }
			set { coarsenessY = value; }
		}


		public override string InvariantName {
			get { return "GridLayouter"; }
		}


		public override string Description {
			get { return Resources.GetString("GridLayouter_Description"); }
		}


		public override void Prepare() {
		}


		public override void Unprepare() {
		}


		public override bool ExecuteStep() {
			// If executing multiple times in a row, we want the layouter to restart from the
			// original situation.
			if (project.History.IsNextUndoCommand(lastCommand))
				project.History.Undo();
			else lastCommand = null;

			Rectangle boundingRectangle = CalcLayoutArea();
			// Find the optimal fit horizontal origin and spacing
			int origH = boundingRectangle.Left; int spaceH = 1000;
			FindSpacing2(ref origH, ref spaceH, (boundingRectangle.Left + boundingRectangle.Right) / 2, true);
			// Find the optimal fit vertical origin and spacing
			int origV = boundingRectangle.Top; int spaceV = 1000;
			FindSpacing2(ref origV, ref spaceV, (boundingRectangle.Top + boundingRectangle.Bottom)/2, false);
			// Arrange the shapes in the calculated grid
			ArrangeShapes(origH, origV, spaceH, spaceV);
			return false;
		}


		protected struct GridPosition {
			public int col;
			public int row;
			public int dir; // -1: unknown, 0: node, 1: eastbound edge, 2: northbound edge
			public int idx; // > 0, if there are multiple shapes for that position
		}


		protected class GridPositionComparer : IComparer, IComparer<GridPosition> {

			#region IComparer Members

			public int Compare(object x, object y) {
				return Compare((GridPosition)x, (GridPosition)y);
			}

			#endregion


			#region IComparer<GridPosition> Members

			public int Compare(GridPosition x, GridPosition y) {
				if (x.col == y.col)
					if (x.row == y.row)
						return x.idx.CompareTo(y.idx);
					else return x.row.CompareTo(y.row);
				else return x.col.CompareTo(y.col);
			}

			#endregion
		}


		protected virtual void FindSpacing2(ref int origin, ref int spacing, int center, bool horizontal) {
			int bestEnergy = int.MaxValue;
			for (int s = 1; s < 1000; ++s) {
				int o;
				int energy = CalcEnergy(out o, ref s, center, horizontal);
				if (energy < bestEnergy) {
					origin = o;
					spacing = s;
					bestEnergy = energy;
				}
			}
		}


		protected virtual int CalcEnergy(out int origin, ref int spacing, int center, bool horizontal) {
			origin = OptimizeOrigin(center, spacing, horizontal);
			return CalcDistanceSum(origin, spacing, horizontal) / selectedShapes.Count + (horizontal? CoarsenessX: CoarsenessY) / spacing;
		}


		protected virtual void FindSpacing1(ref int origin, ref int spacing, int center, bool horizontal) {
			FindNextLocalMinimum(ref origin, ref spacing, center, horizontal);
			spacing = ReduceLocalMinimum(origin, spacing, 40, horizontal);
		}


		protected virtual int FindNextLocalMinimum(ref int origin, ref int spacing, int avgOrigin, bool horizontal) {
			int newOrigin = OptimizeOrigin(avgOrigin, spacing, horizontal);
			int newSpacing = spacing;
			int distanceSum = int.MaxValue;
			int newDistanceSum = CalcDistanceSum(newOrigin, newSpacing, horizontal);
			// Follow the ascending part
			/*while (newDistanceSum >= distanceSum) {
				++spacing;
				origin = OptimizeOrigin(origin, spacing, horizontal);
				newDistanceSum = CalcDistanceSum(origin, spacing, horizontal);
			}*/
			// Follow the descending part (-1 because of rounding errors)
			while (newSpacing > 1 && newDistanceSum <= distanceSum) {
				spacing = newSpacing;
				origin = newOrigin;
				distanceSum = newDistanceSum;
				newSpacing = spacing - 1;
				newOrigin = OptimizeOrigin(avgOrigin, newSpacing, horizontal);
				newDistanceSum = CalcDistanceSum(newOrigin, newSpacing, horizontal);
			}
			// Ergebnis
			return distanceSum;
		}
		

		// Searches a quotient, by which the spacing is divided to lead to a distance
		// sum which is almost the distance sum of the current spacing divided by the quotient.
		// The spacing must be larger than the average shape size.
		protected int ReduceLocalMinimum(int origin, int spacing, int minSpacing, bool horizontal) {
			int distanceSum = CalcDistanceSum(origin, spacing, horizontal);
			int q = 2;
			while (spacing / q > minSpacing) {
				int newDistanceSum = CalcDistanceSum(origin, spacing / q, horizontal);
				if (newDistanceSum <= distanceSum / q) {
					spacing /= q;
					distanceSum = newDistanceSum;
					q = 2;
				} else ++q;
			}
			return spacing;
		}


		protected virtual int OptimizeOrigin(int origin, int spacing, bool horizontal) {
			// int result = maxOrigin;
			// First, we optimize the origin for this spacing
			int displacementSum = 0;
			foreach (Shape s in selectedShapes) {
				// Displacement is the amount of units to move the shape to the nearest grid node. > 0 right/down, < 0 to left/up.
				int rest = horizontal ? (s.X - origin) % spacing : (s.Y - origin) % spacing;
				if (rest < 0) displacementSum += 2 * rest > -spacing ? -rest : -spacing - rest;
				else displacementSum += 2 * rest < spacing ? -rest : spacing - rest;
			}
			int result = origin - displacementSum / selectedShapes.Count;
			return result;
		}


		protected virtual int CalcDistance(int position, int origin, int spacing) {
			int rest = Math.Abs((position - origin) % spacing);
			return Math.Min(rest, spacing - rest);
		}


		protected virtual int CalcDistanceSum(int origin, int spacing, bool horizontal) {
			int distanceSum = 0;
			foreach (Shape s in selectedShapes)
				distanceSum += CalcDistance(horizontal? s.X: s.Y, origin, spacing);
			return distanceSum;
		}


		// Add all seleced shapes into the grid
		protected virtual void ArrangeShapes(int ox, int oy, int sx, int sy) {
			lastCommand = new MoveShapesCommand();
			SortedList<GridPosition, Shape> positionAssignments 
				= new SortedList<GridPosition, Shape>(selectedShapes.Count, new GridPositionComparer());
			foreach (Shape s in selectedShapes) {
				GridPosition gp;
				gp.col = (s.X - ox + (s.X > ox? sx/2: -sx/2)) / sx;
				gp.row = (s.Y - oy + (s.Y > oy? sy/2: -sy/2)) / sy;
				gp.dir = -1;
				gp.idx = 0;
				int i = positionAssignments.IndexOfKey(gp);
				if (i >= 0) {
					// Search a free index for this position
					while (i < positionAssignments.Count && positionAssignments.Keys[i].col == gp.col && positionAssignments.Keys[i].row == gp.row)
						++i;
					gp.idx = i;
				}
				positionAssignments.Add(gp, s);
			}
			// Where more than one shape want to go to the same position, select the 
			// nearest one and assign the others to the edges.
			int idx = 0;
			while (idx < positionAssignments.Count) {
				int minIdx = 0;
				int minDist = int.MaxValue;
				GridPosition gp = positionAssignments.Keys[idx];
				while (idx < positionAssignments.Count && positionAssignments.Keys[idx].col == gp.col 
					&& positionAssignments.Keys[idx].row == gp.row) {
					GridPosition p = positionAssignments.Keys[idx];
					int dist = CalcDistance(positionAssignments[p].X, ox, sx) + CalcDistance(positionAssignments[p].Y, oy, sy);
					if (dist < minDist) {
						minIdx = idx;
						minDist = dist;
					}
					++idx;
				}
				// Make the minimum index the shape at the node
				Shape shape = positionAssignments.Values[minIdx];
				positionAssignments.RemoveAt(minIdx);
				--idx;
				lastCommand.AddMove(shape, ox + gp.col * sx - shape.X, oy + gp.row * sy - shape.Y);
			}
			// CopyFrom the other shapes to edges
			/*idx = 0;
			while (idx < positionAssignments.Count) {
				GridPosition gp = positionAssignments.Keys[idx];

				if (positionAssignments[gp].Y < oy + gp.row * sy) --gp.row;

			}*/
			// Move the edge shapes to their respective positions
			idx = 0;
			while (idx < positionAssignments.Count) {
				GridPosition gp = positionAssignments.Keys[idx];
				// Count the number of shapes for the same edge
				int startIdx = idx;
				while (idx < positionAssignments.Count && positionAssignments.Keys[idx].col == gp.col 
					&& positionAssignments.Keys[idx].row == gp.row && positionAssignments.Keys[idx].dir == gp.dir)
					++idx;
				int count = idx - startIdx;
				// Distribute the shapes along their respective edge
				for (int i = startIdx; i < idx; ++i) {
					if (gp.dir == 0) {
						// Position on horizontal edge
						lastCommand.AddMove(positionAssignments[gp], 
							ox + gp.col * sx + (i - startIdx + 1) * sx / (count + 1) - positionAssignments[gp].X,
							oy + gp.row * sy - positionAssignments[gp].Y);
					} else {
						// Position on vertical edge
						lastCommand.AddMove(positionAssignments[gp], 
							ox + gp.col * sx - positionAssignments[gp].X,
							oy + gp.row * sy + (i - startIdx + 1) * sy / (count + 1) - positionAssignments[gp].Y);
					}
				}
			}
			project.ExecuteCommand(lastCommand);
		}


		#region Fields

		private MoveShapesCommand lastCommand;
		private int coarsenessX;
		private int coarsenessY;

		#endregion

	}


	/*/// <summary>
	/// Positions all shapes on the nodes of rectangular grid.
	/// </summary>
	public class GridLayouter : LayouterBase, ILayouter {

		public GridLayouter() {
		}


		public int ColumnDistance {
			get { return colDistance; }
			set { colDistance = value; }
		}


		public int RowDistance {	
			get { return rowDistance; }
			set { rowDistance = value; }
		}


		#region ILayouter Members

		public override string InvariantName {
			get { return "Positioning"; }
		}


		public override string Description {
			get { return Resources.GetString("GridLayouter_Description"); }
		}


		public override void Prepare() {
			if (selectedShapes.Count <= 0) throw new nShapeException("There are no shapes.");
			// Wir mache die Gitterlinien soweit auseinander, das das durchschnittliche 
			// Shape noch einen Abstand von 0.1 durchschnittlicher Shape-Größe hat
			int sumWidths = 0;
			int sumHeights = 0;
			int n = 0;
			foreach (Shape s in selectedShapes) if (!(s is ILinearShape)) {
				Rectangle r = s.BoundingRectangle;
				sumWidths += r.Width;
				sumHeights += r.Height;
				++n;
			}
			// Grid-Koordinaten berechnen
			layoutArea = CalcLayoutArea();
			gridWidth = colDistance * sumWidths / (100 * n);
			gridHeight = rowDistance * sumWidths / (100 * n);
			//
			currentCol = 0;
			currentRow = 0;
		}


		public override void Unprepare() {
			// Nichts zu tun.
		}


		public override bool ExecuteStep() {
			bool result = true; // false, wenn die nächste Position außerhalb des Bereichs liegt
			bool done = false; // true, wenn ein Shape bewegt wurde
			while (!done && result) {
				// Wir suchen das Shape, das im Gebiet um diese Position ist. Falls es davon 
				// mehrere gibt, nehmen wir das nächste.
				int gridX = layoutArea.left + currentCol * gridWidth;
				int gridY = layoutArea.top + currentRow * gridHeight;
				Rectangle rect = Rectangle.Empty;
				rect.X = gridX - gridWidth / 2;
				rect.Y = gridY - gridHeight / 2;
				rect.Width = gridWidth;
				rect.Height = gridHeight;
				currentShapes.Clear();
				foreach (Shape s in selectedShapes) if (!(s is ILinearShape)) {
					if (Geometry.RectContainsPoint(s.X, s.Y, rect.left, rect.top, rect.Width, rect.Height))
						currentShapes.Add(s);
				}
				if (currentShapes.Count > 0) {
					Shape shape = currentShapes[0];
					int distance = (int)Geometry.DistancePointPoint(shape.X, shape.Y, gridX, gridY);
					// Minimum suchen
					foreach (Shape s in currentShapes) {
						int d = (int)Geometry.DistancePointPoint(shape.X, shape.Y, gridX, gridY);
						if (d < distance) {
							shape = s;
							distance = d;
						}
					}
					shape.MoveControlPointTo(ControlPointId.Reference, gridX, gridY, ResizeModifiers.None);
					done = true;
				}
				++currentCol;
				if (layoutArea.left + currentCol * gridWidth - gridWidth / 2 > layoutArea.right) {
					currentCol = 0;
					++currentRow;
					if (layoutArea.top + currentRow * gridHeight - gridHeight / 2 > layoutArea.bottom)
						result = false;
				}
			}
			return result;
		}

		#endregion


		// Konfiguration
		private int colDistance;
		private int rowDistance;

		// Berechnet aus Konfiguration
		private Rectangle layoutArea;
		private int gridWidth;
		private int gridHeight;

		// Zustand während Berechnung
		private int currentCol;
		private int currentRow;
		private List<Shape> currentShapes = new List<Shape>();
	}*/

}
