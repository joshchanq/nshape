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


namespace Dataweb.NShape.Layouters {

	/// <ToBeCompleted></ToBeCompleted>
	public interface ILayouter {

		/// <summary>
		/// Version and language independent name of the layouter.
		/// </summary>
		string InvariantName { get; }

		/// <summary>
		/// Describes the effect of the layouter.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Specifies all shapes in the diagram, including those that must not be moved.
		/// </summary>
		IEnumerable<Shape> AllShapes { get; set; }

		/// <summary>
		/// Specifies the shapes that are to be layouted.
		/// </summary>
		IEnumerable<Shape> Shapes { get; set; }

		/// <summary>
		/// Internally saves the state of the layouting process.
		/// </summary>
		void SaveState();

		/// <summary>
		/// Restores the state of the layouting process.
		/// </summary>
		void RestoreState();

		/// <summary>
		/// Prepares for layouting.
		/// </summary>
		void Prepare();

		/// <summary>
		/// Resets the layouter to the state before the preparation.
		/// </summary>
		void Unprepare();

		/// <summary>
		/// Executes the layouting operation.
		/// </summary>
		/// <param name="maxSeconds"></param>
		void Execute(int maxSeconds);

		/// <summary>
		/// Executes one step of the layouting operation.
		/// </summary>
		/// <returns>False, if layouting is finished.</returns>
		bool ExecuteStep();

		/// <summary>
		/// Fits all layouted shapes within the given rectangle without destroying the layout.
		/// </summary>
		void Fit(int x, int y, int w, int h);
	}


	/// <summary>
	/// Abstract base class for layouters. Implementations of the ILayouter interface
	/// can but need not derive from this class.
	/// </summary>
	public abstract class LayouterBase {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Layouters.LayouterBase" />.
		/// </summary>
		/// <param name="project"></param>
		protected LayouterBase(Project project) {
			if (project == null) throw new ArgumentNullException("project");
			this.project = project;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract string InvariantName { get; }


		/// <ToBeCompleted></ToBeCompleted>
		public abstract string Description { get; }


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<Shape> AllShapes {
			get { return allShapes; }
			set { 
				allShapes.Clear();
				allShapes.AddRange(value);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<Shape> Shapes {
			get { return selectedShapes; }
			set {
				selectedShapes.Clear();
				selectedShapes.AddRange(value);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SaveState() {
			selectedPositions.Clear();
			foreach (Shape s in selectedShapes)
				if (ExcludeFromFitting(s)) selectedPositions.Add(Point.Empty);
				else selectedPositions.Add(new Point(s.X, s.Y));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RestoreState() {
			Debug.Assert(selectedShapes.Count == selectedPositions.Count);
			for (int i = 0; i < selectedShapes.Count; ++i)
				if (!ExcludeFromFitting(selectedShapes[i]))
					selectedShapes[i].MoveControlPointTo(ControlPointId.Reference, selectedPositions[i].X, selectedPositions[i].Y, ResizeModifiers.None);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void Prepare() {
			SaveState();
			layoutArea = CalcLayoutArea();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract void Unprepare();


		/// <ToBeCompleted></ToBeCompleted>
		public void Execute(int maxSeconds) {
			if (selectedPositions.Count != selectedShapes.Count)
				throw new InvalidOperationException("You must call Prepare() before calling Execute().");

			if (maxSeconds <= 0) maxSeconds = int.MaxValue;
			DateTime start = DateTime.Now;
			while (ExecuteStep() && start.AddSeconds(maxSeconds) > DateTime.Now);
			ICommand cmd = CreateLayoutCommand();
			RestoreState();
			project.ExecuteCommand(cmd);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract bool ExecuteStep();


		/// <summary>
		/// Fits the shapes into the given rectangle.
		/// </summary>
		public void Fit(int x, int y, int w, int h) {
			// Aktuelles umgebendes Rechteck der Shapes berechnen
			int x1 = int.MaxValue;
			int y1 = int.MaxValue;
			int x2 = int.MinValue;
			int y2 = int.MinValue;
			Rectangle bounds = Rectangle.Empty;
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;
				bounds = s.GetBoundingRectangle(true);
				if (bounds.Left < x1) x1 = bounds.Left;
				if (bounds.Top < y1) y1 = bounds.Top;
				if (bounds.Right > x2) x2 = bounds.Right;
				if (bounds.Bottom > y2) y2 = bounds.Bottom;
			}

			// Wenn das umgebende Rechteck größer als der Rahmen ist,
			// verkleinern
			if (x2 - x1 > w) {
				foreach (Shape s in Shapes) {
					//if (s is ILinearShape) continue;
					if (ExcludeFromFitting(s)) continue; 
					int sx = x1 + (s.X - x1) * w / (x2 - x1);
					int sy = s.Y;
					s.MoveControlPointTo(ControlPointId.Reference, sx, sy, ResizeModifiers.None);
				}
				x2 = x1 + w;
			}
			if (y2 - y1 > h) {
				foreach (Shape s in Shapes) {
					if (ExcludeFromFitting(s)) continue;

					int sx = s.X;
					int sy = y1 + (s.Y - y1) * h / (y2 - y1);
					s.MoveControlPointTo(ControlPointId.Reference, sx, sy, ResizeModifiers.None);
				}
				y2 = y1 + h;
			}
			// Wenn das umgebende Rechteck außerhalb des Rahmens liegt,
			// verschieben
			int dx = (x1 < x || x2 > x + w) ? (w - x2 - x1 + 2 * x) / 2 : 0;
			int dy = (y1 < y || y2 > y + h) ? (h - y2 - y1 + 2 * y) / 2 : 0;
			foreach (Shape s in Shapes) {
				//if (s is ILinearShape) continue;
				if (ExcludeFromFitting(s)) continue;
				int sx = s.X + dx;
				int sy = s.Y + dy;
				s.MoveControlPointTo(ControlPointId.Reference, sx, sy, ResizeModifiers.None);
			}
		}


		/// <summary>
		/// Creates a multi-move command from the original to the final arrangement and executes it.
		/// </summary>
		/// <returns></returns>
		protected ICommand CreateLayoutCommand() {
			MoveShapesToCommand cmd = new MoveShapesToCommand();
			for (int i = 0; i < selectedShapes.Count; ++i)
				cmd.AddMoveTo(selectedShapes[i], selectedPositions[i].X, selectedPositions[i].Y, selectedShapes[i].X, selectedShapes[i].Y);
			return cmd;
		}


		/// <summary>
		/// Calculates a rectangle containing all reference points of all shapes.
		/// The size of the sapes will be ignored, which results in not idempotent layouts.
		/// </summary>
		protected Rectangle CalcLayoutArea() {
			Rectangle result = Rectangle.Empty;
			int left = selectedShapes[0].X;
			int top = selectedShapes[0].Y;
			int right = left;
			int bottom = top;
			foreach (Shape s in selectedShapes) {
				if (s.X < left) left = s.X;
				else if (s.X > right) right = s.X;
				if (s.Y < top) top = s.Y;
				else if (s.Y > bottom) bottom = s.Y;
			}
			result.X = left;
			result.Y = top;
			result.Width = right - left;
			return result;
		}
		

		// There are lines without glue points (SwitchBar) and planar shapes with glue points (Label)
		// so we better check on glue points here...
		/// <ToBeCompleted></ToBeCompleted>
		protected bool ExcludeFromFitting(Shape shape) {
			foreach (ControlPointId pointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				if (shape.IsConnected(pointId, null) != ControlPointId.None) return true;
			}
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected readonly Project project;

		// All shapes in the diagram
		/// <ToBeCompleted></ToBeCompleted>
		protected List<Shape> allShapes = new List<Shape>(1000);

		// The selected shapes being layoutet
		/// <ToBeCompleted></ToBeCompleted>
		protected List<Shape> selectedShapes = new List<Shape>(100);

		// The stored positions of the selected shapes
		/// <ToBeCompleted></ToBeCompleted>
		protected List<Point> selectedPositions = new List<Point>(100);

		/// <ToBeCompleted></ToBeCompleted>
		protected Rectangle layoutArea;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class RepulsionLayouter : LayouterBase, ILayouter {

		/// <ToBeCompleted></ToBeCompleted>
		public RepulsionLayouter(Project project)
			: base(project) {
			springRate = 8;
			repulsion = 3;
			repulsionRange = 500;
			friction = 0;
			mass = 500;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int SpringRate {
			get { return springRate; }
			set { springRate = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int Repulsion {
			get { return repulsion; }
			set { repulsion = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int RepulsionRange {
			get { return repulsionRange; }
			set { repulsionRange = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int Friction {
			get { return friction; }
			set { friction = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int Mass {
			get { return mass; }
			set { mass = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int TimeInterval {
			get { return timeInterval; }
			set { timeInterval = value; }
		}


		/// <override></override>
		public override string InvariantName {
			get { return "Clusters"; }
		}


		/// <override></override>
		public override string Description {
			get { return "Moves connected shapes nearer to each other while thrusting unconnected ones apart."; }
		}


		/// <override></override>
		public override void Prepare() {
			base.Prepare();
			// Nichts zu tun!?
		}


		/// <override></override>
		public override void Unprepare() {
			// Nichts zu tun.
		}


		// Berechnen und Ausführen der Verschiebung getrennt durchführen,
		// damit die Symmetrie einer Anordnung erhalten bleibt.
		/// <override></override>
		public override bool ExecuteStep() {
			List<Size> displacements = new List<Size>();
			// Verschiebungen berechnen
			Size z = Size.Empty;
			displacements.Clear();
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;
				CalcDisplacement(s, ref z);
				displacements.Add(z);
			}
			// Verschiebungen ausführen
			int maxDisplacement = 0;
			int i = 0;
			foreach (Shape s in Shapes) {
				if (ExcludeFromFitting(s)) continue;

				int nx = s.X + displacements[i].Width;
				int ny = s.Y + displacements[i].Height;
				s.MoveTo(nx, ny);
				int displacement = (int)Geometry.DistancePointPoint(displacements[i].Width, displacements[i].Height, 0, 0);
				if (displacement > maxDisplacement) maxDisplacement = displacement;
				++i;
			}
			return maxDisplacement > 0;
		}


		private void CalcDisplacement(Shape shape, ref Size displacement) {
			int totalForceX = 0;
			int totalForceY = 0;

			// Alle anziehenden Kräfte aufsummieren
			foreach (ShapeConnectionInfo sci1 in shape.GetConnectionInfos(ControlPointId.Any, null)) {
				foreach (ShapeConnectionInfo sci2 in sci1.OtherShape.GetConnectionInfos(ControlPointId.Any, null)) {
					if (sci2.OtherShape == shape) continue;
					// Wenn man hier nicht prüft ob das OtherShape ebenfalls verknüpft ist, dann zieht bspw. ein 
					// mit einer verknüpften Linie verknüpftes Shape unendlich lang einseitig an der Linie und 
					// es kommt letztendlich zu einem "Integer Overflow".
					if (ExcludeFromFitting(sci2.OtherShape)) continue;

					// Shape shape ist über s mit sci2.Shape verknüpft
					int distance = (int)Geometry.DistancePointPoint(sci2.OtherShape.X, sci2.OtherShape.Y, shape.X, shape.Y);
					int force = distance * springRate - friction;
					if (force > 0) {
						totalForceX += (sci2.OtherShape.X - shape.X) * force / distance;
						totalForceY += (sci2.OtherShape.Y - shape.Y) * force / distance;
					}
				}
			}
			// Alle abstoßenden Kräfte aufsummieren
			foreach (Shape s in AllShapes) {
				if (ExcludeFromFitting(s)) continue;
				if (s != shape) {
					int distance = (int)Geometry.DistancePointPoint(s.X, s.Y, shape.X, shape.Y);
					if (distance <= repulsionRange) {
						int force = (repulsionRange - distance) * repulsion - friction;
						// Die Kraft kann durch die Reibung nicht negativ werden.
						if (force < 0) force = 0;
						if (distance <= 0) {
							// Die Shapes liegen aufeinander, deshalb kann man keine Richtung 
							// berechnen. Wir nehmen einen Zufalls-Einheitsvektor, der dafür 
							// sorgen sollte, dass das andere Shape (shape) woanders hin ver-
							// schoben wird.
							int directionX = random.Next(100);
							int directionY = (int)Math.Sqrt(10000 - directionX * directionX);
							totalForceX += force * directionX / 100;
							totalForceY += force * directionY / 100;
						} else {
							totalForceX += (shape.X - s.X) * force / distance;
							totalForceY += (shape.Y - s.Y) * force / distance;
						}
					}
				}
			}
			// Bewegung berechnen
			displacement.Width = (totalForceX * TimeInterval * TimeInterval / (2 * Mass));
			displacement.Height = (totalForceY * TimeInterval * TimeInterval / (2 * Mass));
		}


		private int springRate;
		private int repulsion;
		private int repulsionRange;
		private int friction; // As constant force
		private int mass = 100; // In kg. Je höher die Masse, desto kleiner die Strecken pro Zeitschritt
		private int timeInterval = 1; // In seconds

		private Random random = new Random();

	}


	/// <ToBeCompleted></ToBeCompleted>
	public class ExpansionLayouter : LayouterBase, ILayouter {

		/// <ToBeCompleted></ToBeCompleted>
		public ExpansionLayouter(Project project)
			: base(project) {
		}


		// Faktor -10 to 10
		/// <ToBeCompleted></ToBeCompleted>
		public int HorizontalCompression {
			get { return horizontalCompression; }
			set { horizontalCompression = value; }
		}


		// Faktor -10 to 10
		/// <ToBeCompleted></ToBeCompleted>
		public int VerticalCompression {
			get { return verticalCompression; }
			set { verticalCompression = value; }
		}


		#region ILayouterBase implementation

		/// <override></override>
		public override string InvariantName {
			get { return "Expansion"; }
		}


		/// <override></override>
		public override string Description {
			get { return "Compresses or expands a set of shapes without destoying their relative position."; }
		}


		/// <override></override>
		public override void Prepare() {
			base.Prepare();
			// Nothing to do
		}


		/// <override></override>
		public override void Unprepare() {
			// Nothing to do
		}


		/// <override></override>
		public override bool ExecuteStep() {
			Rectangle boundingArea = CalcLayoutArea();
			Rectangle layoutArea = boundingArea;
			layoutArea.Width = horizontalCompression * layoutArea.Width / 100;
			layoutArea.Height = verticalCompression * layoutArea.Height / 100;
			layoutArea.X = boundingArea.X - (layoutArea.Width - boundingArea.Width)/2;
			layoutArea.Y = boundingArea.Y - (layoutArea.Height - boundingArea.Height)/2;
			//
			foreach (Shape s in selectedShapes) {
				int nx = layoutArea.X + horizontalCompression * (s.X - boundingArea.X) / 100;
				int ny = layoutArea.Y + verticalCompression * (s.Y - boundingArea.Y) / 100;
				s.MoveControlPointTo(ControlPointId.Reference, nx, ny, ResizeModifiers.None);
			}
			return false;
		}
		
		#endregion

		// compression as %
		private int horizontalCompression = 0;
		private int verticalCompression = 0;

	}


	// Ordnet die Shapes so in Schichten an, dass die Pfeile in erster Linie nach
	// unten gehen und möglichst auch nur in die nächste Schicht.
	// Dabei zählen alle Linien mit ungleichen Spitzen als Pfeile in die Richtung 
	// "Ende". Linien mit gleichen Spitzen zählen als Pfeile in beide Richtungen.
	/// <ToBeCompleted></ToBeCompleted>
	public class FlowLayouter : LayouterBase, ILayouter {

		/// <ToBeCompleted></ToBeCompleted>
		public enum FlowDirection {
			/// <ToBeCompleted></ToBeCompleted>
			BottomUp,
			/// <ToBeCompleted></ToBeCompleted>
			LeftToRight,
			/// <ToBeCompleted></ToBeCompleted>
			TopDown,
			/// <ToBeCompleted></ToBeCompleted>
			RightToLeft 
		};

		/// <ToBeCompleted></ToBeCompleted>
		public FlowLayouter(Project project)
			: base(project) {
			shapeByXComparer = new ShapeByXComparer(this);
			shapeByValueComparer = new ShapeByValueComparer();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public FlowDirection Direction {
			get { return flowDirection; }
			set { flowDirection = value; }
		}


		/// <override></override>
		public override string InvariantName {
			get { return "Flow"; }
		}


		/// <override></override>
		public override string Description {
			get { return "Orders the shapes such that the majority of the arrows points in a given direction."; }
		}


		/// <override></override>
		public override void Prepare() {
			base.Prepare();
			PrepareLayering();
			phase = Phase.OptimizeLevels;
		}


		/// <override></override>
		public override void Unprepare() {
			foreach (Shape s in selectedShapes)
				s.InternalTag = null;
		}


		/// <override></override>
		public override bool ExecuteStep() {
			bool result = true;
			switch (phase) {
				case Phase.OptimizeLevels:
					if (!LayerShapes()) {
						phase = Phase.Ordering;
						PrepareOrdering();
					}
					break;
				case Phase.Ordering:
					if (!OrderShapes()) {
						phase = Phase.Positioning;
						PreparePositioning();
					}
					break;
				case Phase.Positioning:
					result = PositionShapes();
					break;
				default: 
					Debug.Fail("Unsupported phase in FlowLayouter.ExecuteStep");
					break;
			}
			return result;
		}


		// Berechnet die Felder firstLayerPos, layerDistance, layerCount
		// und weist die Shapes den Leveln zu.
		private void PrepareLayering() {
			// Hier später mal die Layers aus den vorhandenen Shapes erschließen
			Rectangle layoutRect = CalcLayoutArea();
			switch (flowDirection) {
				case FlowDirection.TopDown:
					firstLayerPos = layoutRect.Top;
					layerDistance = 100;
					layerCount = layoutRect.Height / layerDistance;
					break;
				case FlowDirection.LeftToRight:
					firstLayerPos = layoutRect.Left;
					layerDistance = 100;
					layerCount = layoutRect.Width / layerDistance;
					break;
				case FlowDirection.BottomUp:
					firstLayerPos = -layoutRect.Bottom;
					layerDistance = 100;
					layerCount = layoutRect.Height / layerDistance;
					break;
				case FlowDirection.RightToLeft:
					firstLayerPos = -layoutRect.Right;
					layerDistance = 100;
					layerCount = layoutRect.Width / layerDistance;
					break;
				default:
					Debug.Fail("Unexpected flow direction in PrepareLayering");
					break;
			}
			// Jetzt die initialen Schichten zuweisen
			foreach (Shape s in selectedShapes) {
				s.InternalTag = new LayerInfo();
				int fy = GetFlowY(s);
				int layerIndex = (fy - firstLayerPos + layerDistance / 2) / layerDistance;
				Debug.Assert(layerIndex >= 0);
				((LayerInfo)s.InternalTag).layer = layerIndex;
				if (layerIndex >= layerCount) layerCount = layerIndex + 1;
				MoveShapeFlowY(s, firstLayerPos + ((LayerInfo)s.InternalTag).layer * layerDistance);
			}
		}

		// Benutzt ein Gradientenabstiegsverfahren, um die Shapes bestmöglich auf die 
		// Ebenen zu verteilen.
		private bool LayerShapes() {
			bool result = false;
			foreach (Shape s in selectedShapes) {
				if (ExcludeFromFitting(s)) continue;

				int downInCount, downOutCount;
				int sameInCount, sameOutCount;
				int upInCount, upOutCount;
				int nextDown, nextUp;
				CalcCharacteristics(s, 0, out downInCount, out downOutCount, out sameInCount, out sameOutCount, out upInCount, out upOutCount, out nextDown, out nextUp);
				int ov1 = CalcOptimizationValue(downInCount, downOutCount, sameInCount, sameOutCount, upInCount, upOutCount);
				if (upInCount + upOutCount + sameInCount + sameOutCount > 0) {
					if (2 * upInCount + sameInCount > 2 * upOutCount + sameOutCount) {
						// Nach unten verschieben
						CalcCharacteristics(s, nextDown, out downInCount, out downOutCount, out sameInCount, out sameOutCount, out upInCount, out upOutCount, out nextDown, out nextUp);
						if (CalcOptimizationValue(downInCount, downOutCount, sameInCount, sameOutCount, upInCount, upOutCount) < ov1) {
							MoveShapeLayerFlow(s, nextDown);
							result = true;
						}
					} else if (2 * upOutCount + sameOutCount > 2 * upInCount + sameInCount) {
						// Nach oben verschieben
						CalcCharacteristics(s, nextUp, out downInCount, out downOutCount, out sameInCount, out sameOutCount, out upInCount, out upOutCount, out nextDown, out nextUp);
						if (CalcOptimizationValue(downInCount, downOutCount, sameInCount, sameOutCount, upInCount, upOutCount) < ov1) {
							MoveShapeLayerFlow(s, nextUp);
							result = true;
						}
					}
				} else if (2 * upOutCount + sameOutCount > 2 * upInCount + sameInCount) {
					// Nach oben verschieben
					CalcCharacteristics(s, nextUp, out downInCount, out downOutCount, out sameInCount, out sameOutCount, out upInCount, out upOutCount, out nextDown, out nextUp);
					if (CalcOptimizationValue(downInCount, downOutCount, sameInCount, sameOutCount, upInCount, upOutCount) < ov1) {
						MoveShapeLayerFlow(s, nextUp);
						result = true;
					}
				}
			}
			return result;
		}


		// Legt die Datenstrukturen für das Layout auf Layer-Ebene an und sortiert 
		// schon mal die oberste Ebene.
		private void PrepareOrdering() {
			layerList.Clear();
			foreach (Shape s in selectedShapes) {
				if (ExcludeFromFitting(s)) continue;

				int li = ((LayerInfo)s.InternalTag).layer;
				while (layerList.Count <= li)
					layerList.Add(new List<Shape>());
				int i = layerList[li].BinarySearch(s, shapeByXComparer);
				if (i < 0) i = ~i;
				layerList[li].Insert(i, s);
			}
			//
			firstRowPos = 100;
			rowDistance = 60;
			phase = Phase.Ordering;
			run = 0;
			currentLayer = 1;
		}


		// Benutzt Sugiyamas Idee mit den Baryzentren, um die Shapes auf den Ebenen so
		// zu sortieren, dass die Überkreuzungen möglichst gering sind.
		private bool OrderShapes() {
			// Wenn es nur eine Ebene gibt, ist nichts zu tun.
			if (currentLayer >= layerList.Count) return false;
			int layerDelta = run % 2 == 0 ? -1 : +1;
			// Berechne die Baryzentren für die aktuelle Ebene
			// Falls ein Shape keine Verbindungung in die andere Ebene hat, wird sein
			// Baryzentrum auf den Joker-Wert -1 gesetzt
			foreach (Shape shape in layerList[currentLayer]) {
				int bc = 0;
				int n = 0;
				foreach (Shape s in GetConnectedShapesInLayer(shape, currentLayer + layerDelta)) {
					++n;
					bc += layerList[currentLayer + layerDelta].IndexOf(s) + 1;
				}
				if (n == 0) ((LayerInfo)shape.InternalTag).value = -1;
				else ((LayerInfo)shape.InternalTag).value = (float)bc / n;
			}
			// Sortiere die Shapes nach aufsteigenden Baryzentrums-Werten
			// Damit vorhandene gut positionierte Shapes nicht unnötig verschoben werden
			// Fügt dieser Algorithmus die Shapes geometrisch ein
			float mbc = 0; // Maximum barycenter
			for (int i = 0; i < layerList[currentLayer].Count; ++i) {
				Shape shape = layerList[currentLayer][i];
				if (((LayerInfo)shape.InternalTag).value < 0) {
					// Ein Joker wird auf den passenden Wert gesetzt
					((LayerInfo)shape.InternalTag).value = mbc;
				} else if (((LayerInfo)shape.InternalTag).value < mbc) {
					// Falsch einsortiert
					int index = layerList[currentLayer].BinarySearch(0, i, shape, shapeByValueComparer);
					if (index < 0) index = ~index;
					layerList[currentLayer].Remove(shape);
					layerList[currentLayer].Insert(index, shape);
					if (index == 0) MoveShapeFlowX(shape, GetFlowX(layerList[currentLayer][0]) - 10);
					else MoveShapeFlowX(shape, (GetFlowX(layerList[currentLayer][index - 1]) + GetFlowX(layerList[currentLayer][index])) / 2);
				} else {
					mbc = ((LayerInfo)shape.InternalTag).value;
				}
			}
			// Nächsten Durchlauf bestimmen
			if (layerDelta < 0) {
				++currentLayer;
				if (currentLayer >= layerList.Count) {
					++run;
					currentLayer = layerList.Count - 2;
				}
			} else {
				--currentLayer;
				if (currentLayer < 0) {
					++run;
					currentLayer = 1;
				}
			}
			// Wir machen runter - rauf - runter
			return run <= 2;
		}


		// Weist alle Shapes eine Position auf den Gitterpunkten zu, ohne die Reihen-
		// folge zu ändern.
		private void PreparePositioning() {
			for (int li = 0; li < layerList.Count; ++li) {
				for (int si = 0; si < layerList[li].Count; ++si) {
					Shape shape = layerList[li][si];
					int nsi = (GetFlowX(shape) - firstRowPos + rowDistance / 2) / rowDistance;
					// Wenn nsi < si, können wir nichts machen
					if (nsi > si) {
						// Wir schieben alle Listeneinträge ab si nach oben zu nsi 
						for (int j = si; j < nsi; ++j) layerList[li].Add(null);
						for (int j = layerList[li].Count - nsi + si - 1; j >= si; --j) {
							layerList[li][j + nsi - si] = layerList[li][j];
						}
						for (int j = nsi - 1; j >= si; --j) layerList[li][j] = null;
						si = nsi;
					} else nsi = si;
					MoveShapeFlowX(shape, firstRowPos + nsi * rowDistance);
				}
			}
			phase = Phase.Positioning;
			run = 0;
			currentLayer = 1;
		}


		// Verschiebt die Shapes im aktuellen Layer so, dass die Abstände zu den Shapes
		// im darunterliegenden bzw. darüberliegenden Layer klein sind.
		private bool PositionShapes() {
			// Wenn es nur eine Ebene gibt ist nichts zu tun.
			if (currentLayer >= layerList.Count) return false;
			int layerDelta = run % 2 == 0 ? -1 : +1;
			List<Shape> shapeList = layerList[currentLayer];
			// Wir gehen von links nach rechts durch und verschieben Shapes soweit 
			// möglich an die richtige Stelle.
			for (int si = 0; si < shapeList.Count; ++si) {
				if (shapeList[si] == null) continue;
				int m = 0;
				int n = 0;
				foreach (Shape s in GetConnectedShapesInLayer(shapeList[si], currentLayer + layerDelta)) {
					m += (GetFlowX(s) - GetFlowX(shapeList[si])) / rowDistance;
					++n;
				}
				// Nach links können wir nur soweit platz ist.
				if (n > 0 && m/n != 0) TryMoveShapeRow(shapeList, si, m / n, m/n > 0);
			}
			// Nächsten Durchlauf bestimmen
			if (layerDelta < 0) {
				++currentLayer;
				if (currentLayer >= layerList.Count) {
					++run;
					currentLayer = layerList.Count - 2;
				}
			} else {
				--currentLayer;
				if (currentLayer < 0) {
					++run;
					currentLayer = 1;
				}
			}
			// Wir machen runter - rauf
			return run <= 2;
		}


		private int GetFlowX(Shape s) {
			switch (flowDirection) {
				case FlowDirection.TopDown: return s.X;
				case FlowDirection.LeftToRight: return s.Y;
				case FlowDirection.BottomUp: return s.X;
				case FlowDirection.RightToLeft: return s.X;
				default: Debug.Fail("Unexpected flow direction"); return 0;
			}
		}


		// Liefert die Koordinate in Flussrichtung
		private int GetFlowY(Shape s) {
			switch (flowDirection) {
				case FlowDirection.TopDown: return s.Y;
				case FlowDirection.LeftToRight: return s.X;
				case FlowDirection.BottomUp: return -s.Y;
				case FlowDirection.RightToLeft: return -s.X;
				default: Debug.Fail("Unexpected flow direction"); return 0;
			}
		}


		private void MoveShapeFlowX(Shape shape, int x) {
			switch (flowDirection) {
				case FlowDirection.TopDown: shape.X = x; break;
				case FlowDirection.LeftToRight: shape.Y = x; break;
				case FlowDirection.BottomUp: shape.X = x; break;
				case FlowDirection.RightToLeft: shape.Y = x; break;
				default: Debug.Fail("Unexpected flow direction"); break;
			}
		}


		private void MoveShapeFlowY(Shape shape, int y) {
			switch (flowDirection) {
				case FlowDirection.TopDown: shape.Y = y; break;
				case FlowDirection.LeftToRight: shape.X = y; break;
				case FlowDirection.BottomUp: shape.Y = -y; break;
				case FlowDirection.RightToLeft: shape.X = -y; break;
				default: Debug.Fail("Unexpected flow direction"); break;
			}
		}


		private void MoveShapeLayerFlow(Shape shape, int layerDelta) {
			// Brauchen wir einen neuen Layer?
			if (((LayerInfo)shape.InternalTag).layer + layerDelta < 0)
				InsertLayer(0);
			else if (((LayerInfo)shape.InternalTag).layer + layerDelta >= layerCount)
				InsertLayer(layerCount);
			// Shape verschieben
			((LayerInfo)shape.InternalTag).layer += layerDelta;
			MoveShapeFlowY(shape, firstLayerPos + ((LayerInfo)shape.InternalTag).layer * layerDistance);
		}


		private class LayerInfo {
			// Index des Layers ab 0
			public int layer;
			// Aktueller Wert. Beim Sortieren das Baryzentrum
			public float value;
		}


		// Liefert alle shapes in layerIndex, die mit s verbunden sind
		private IEnumerable<Shape> GetConnectedShapesInLayer(Shape shape, int layerIndex) {
			foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Reference, null))
				if (sci.OtherShape is LineShapeBase) {
					foreach (ShapeConnectionInfo psci in sci.OtherShape.GetConnectionInfos(3 - sci.OtherPointId, null)) {
						if (psci.OtherShape != null && ((LayerInfo)psci.OtherShape.InternalTag).layer == layerIndex) 
							yield return psci.OtherShape;
					}
				}
		}


		// Versucht das shape von index um delta Gitterpositionen zu verschieben. Dabei 
		// kann es schon vorhandene Shapes "zusammenschieben". Die Funktion tut das 
		// soweit möglich und liefert die Anzahl der erreichten Verschiebungen zurück.
		private int TryMoveShapeRow(List<Shape> shapeList, int index, int delta, bool allowPushing) {
			int newIndex = index;
			while (shapeList.Count <= index + delta) shapeList.Add(null);
			// 
			int inc = delta > 0 ? +1 : -1;
			while (delta != 0 && shapeList[newIndex + inc] == null) {
				newIndex += inc;
				delta -= inc;
			}
			// Falls ein anderes Element ist im Weg ist, verschieben wir es ebenfalls
			if (delta != 0 && allowPushing) 
				newIndex += TryMoveShapeRow(shapeList, newIndex + inc, delta, true);
			// Verschiebung durchführen
			if (newIndex != index) {
				shapeList[newIndex] = shapeList[index];
				shapeList[index] = null;
				MoveShapeFlowX(shapeList[newIndex], firstRowPos + rowDistance * newIndex);
			}
			return newIndex - index;
		}


		private void InsertLayer(int layerIndex) {
			foreach (Shape s in selectedShapes) {
				if (ExcludeFromFitting(s)) continue;
				if (((LayerInfo)s.InternalTag).layer >= layerIndex) {
					++((LayerInfo)s.InternalTag).layer;
				}
			}
			// Wir nehmen erst einmal an, dass nur oben und unten layer eingefügt werden
			if (layerIndex == 0) firstLayerPos -= layerDistance;
			++layerCount;
		}


		private class ShapeByXComparer : IComparer<Shape> {

			public ShapeByXComparer(FlowLayouter flowLayouter) {
				this.flowLayouter = flowLayouter;
			}


			public int Compare(Shape s1, Shape s2) {
				int x1 = flowLayouter.GetFlowX(s1); 
				int x2 = flowLayouter.GetFlowX(s2);
				return x1 > x2? + 1: x2 > x1? -1: 0;
			}


			private FlowLayouter flowLayouter;
		}


		private class ShapeByValueComparer : IComparer<Shape> {

			public int Compare(Shape s1, Shape s2) {
				float v1 = ((LayerInfo)s1.InternalTag).value;
				float v2 = ((LayerInfo)s2.InternalTag).value;
				return v1 > v2 ? +1 : v2 > v1 ? -1 : 0;
			}

		}


		private enum Phase { OptimizeLevels, Ordering, Positioning };


		// NextDown/NextUp: Abstand zum nächsten verbundenen Layer abwärts (> 0) und aufwärts (< 0)
		private void CalcCharacteristics(Shape shape, int d, out int downInCount, out int downOutCount,
			out int sameInCount, out int sameOutCount, out int upInCount, out int upOutCount, out int nextDown, out int nextUp) 
		{
			downInCount = downOutCount = sameInCount = sameOutCount = upInCount = upOutCount = 0;
			nextDown = int.MaxValue;
			nextUp = int.MinValue;
			int thisLayerIndex = ((LayerInfo)shape.InternalTag).layer + d;
			foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Reference, null))
				if (sci.OtherShape is ILinearShape) {
					Orientation o = Orientation.Both;
					if (((ILinearShape)sci.OtherShape).IsDirected)
						o = sci.OtherPointId == 1 ? Orientation.Outgoing : Orientation.Incoming;
					
					// Original by pepo:
					//Shape otherShape = sci.PassiveShape.GetConnectionInfo(null, 3-sci.ConnectionPointId).PassiveShape;
					//
					// @@Kurt
					// ToDo: Check this call: "GetConnectionInfos(3 - sci.ConnectionPointId, null). "3 - connectionPointId" ???
					foreach (ShapeConnectionInfo psci in sci.OtherShape.GetConnectionInfos(3 - sci.OtherPointId, null)) {
						Shape otherShape = psci.OtherShape;

						if (otherShape == null) continue;
						int otherLayer = ((LayerInfo)otherShape.InternalTag).layer;
						if (otherLayer < thisLayerIndex) {
							switch (o) {
								case Orientation.Incoming: ++downInCount; break;
								case Orientation.Outgoing: ++upOutCount; break;
								case Orientation.Both: ++downInCount; ++upOutCount; break;
								default: Debug.Fail("Unexpected orientation in FlowLayouter.CalcCharacteristics"); break;
							}
							if (otherLayer > thisLayerIndex + nextUp) nextUp = otherLayer - thisLayerIndex;
						} else if (otherLayer > thisLayerIndex) {
							switch (o) {
								case Orientation.Incoming: ++upInCount; break;
								case Orientation.Outgoing: ++downOutCount; break;
								case Orientation.Both: ++upInCount; ++downOutCount; break;
								default: Debug.Fail("Unexpected orientation in FlowLayouter.CalcCharacteristics"); break;
							}
							if (otherLayer < thisLayerIndex + nextDown) nextDown = otherLayer - thisLayerIndex;
						} else {
							switch (o) {
								case Orientation.Incoming: ++sameInCount; break;
								case Orientation.Outgoing: ++sameOutCount; break;
								case Orientation.Both: ++sameInCount; ++sameOutCount; break;
								default: Debug.Fail("Unexpected orientation in FlowLayouter.CalcCharacteristics"); break;
							}
						}
					}
					if (nextDown == int.MaxValue) nextDown = +1;
					if (nextUp == int.MinValue) nextUp = -1;
				}
		}


		// Berechnet den Optimierungswert für das Shape an der Stelle x, y
		private int CalcOptimizationValue(int downInCount, int downOutCount, 
			int sameInCount, int sameOutCount, int upInCount, int upOutCount) {
			return 2 * upOutCount + 2 * upInCount + sameOutCount + sameInCount;
		}
		

		private enum Orientation { Incoming, Outgoing, Both };


		private ShapeByXComparer shapeByXComparer;
		private ShapeByValueComparer shapeByValueComparer;


		private FlowDirection flowDirection = FlowDirection.TopDown;

		// Minimaler und maximaler Abstand zweier Shapes, die in benachbarten
		// Schichten sind. Die Schichtdicke ist (maxLayerDistance - minLayerDistance)/2.
		// @@Kurt: Auskommentiert wegen Warnung
		//private int minLayerDistance = 100;
		//private int maxLayerDistance = 100;

		// Gerade durchgeführte Phase
		private Phase phase;
		// Gerade durchgeführter Durchlauf
		private int run;

		private int firstLayerPos = 100;
		private int layerDistance = 100;
		private int layerCount = 10;
		private int firstRowPos = 100;
		private int rowDistance = 30;
		private int currentLayer = 0; // Der gerade bearbeitete Layer
		//
		List<List<Shape>> layerList = new List<List<Shape>>();
		List<Shape> layerShapes = new List<Shape>();
	}

}