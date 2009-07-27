using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;


namespace Dataweb.Diagramming.Advanced {

	#region ShapeAggregation abstract base class

	/// <summary>
	/// Defines a set of shapes aggregated into another shape.
	/// </summary>
	public abstract class ShapeAggregation : ShapeCollection {

		protected ShapeAggregation(Shape owner)
			: base() {
			Construct(owner);
		}


		protected ShapeAggregation(Shape owner, int capacity)
			: base(capacity) {
			Construct(owner, capacity);
		}


		protected ShapeAggregation(Shape owner, IEnumerable<Shape> collection)
			: base(collection) {
			Construct(owner, shapes.Capacity);
		}


		#region ShapeAggregation Members called by the owner shape

		public Shape Owner {
			get { return owner; }
			protected set { owner = value; }
		}


		public virtual void CopyFrom(IShapeCollection source) {
			if (source == null) throw new ArgumentNullException("source");
			foreach (Shape shape in source.BottomUp) {
				Shape shapeClone = shape.Clone();
				shapeClone.ZOrder = shape.ZOrder;
				shapeClone.Parent = this.Owner;
				shapeClone.DisplayService = this.Owner.DisplayService;
				this.shapes.Add(shapeClone);
			}
			if (source is ShapeAggregation) {
				ShapeAggregation src = (ShapeAggregation)source;
				this.aggregationAngle = src.aggregationAngle;
				this.rotationCenter = src.rotationCenter;
				// Copy center points of the shapes
				this.shapePositions.Clear();
				for (int i = 0; i < shapes.Count; ++i) {
					Point shapePos = src.shapePositions[src.shapes[i]];
					this.shapePositions.Add(this.shapes[i], shapePos);
				}
			} else {
				// if the source ShapeCollection is not a ShapeAggregation, 
				// store unrotated ShapePositions nevertheless
				this.shapePositions.Clear();
				for (int i = 0; i < shapes.Count; ++i) {
					Point shapePos = Point.Empty;
					shapePos.Offset(shapes[i].X, shapes[i].Y);
					this.shapePositions.Add(shapes[i], shapePos);
				}
			}
		}


		/// <summary>
		/// Notifies the child shapes that their parent has been rotated. Rotates all children according to the parent's rotation.
		/// </summary>
		/// <returns>
		/// True if all child shapes have rotated in the desired way. 
		/// False if the child shapes cannot move as desired due to restrictions (such as connections).
		/// </returns>
		public bool NotifyParentRotated(int angle, int rotationCenterX, int rotationCenterY) {
			if (shapes.Count == 0) return false;
			else {
				bool result = true;
				try {
					SuspendUpdate();
					RevertRotation();

					// set new rotation center and shapeAngle
					aggregationAngle = (aggregationAngle + angle) % 3600;
					rotationCenter.X = rotationCenterX;
					rotationCenter.Y = rotationCenterY;

					// rotate child shapes around the parent's center
					for (int i = shapes.Count-1; i >= 0 ; --i) {
						if (!shapes[i].Rotate(aggregationAngle, rotationCenter.X, rotationCenter.Y))
							result = false;
					}
					// Reset bounding rectangles
					boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
				} finally { ResumeUpdate(); }
				return result;
			}
		}


		/// <summary>
		/// Notifies the child shapes that their parent has been moved. Moves all children according to the parent's movement.
		/// </summary>
		/// <returns>
		/// True if all child shapes have moved in the desired way. 
		/// False if the child shapes cannot move as desired due to restrictions (such as connections).
		/// </returns>
		public virtual bool NotifyParentMoved(int deltaX, int deltaY) {
			if (shapes.Count == 0) return true;
			else {
				bool result = true;
				try {
					SuspendUpdate();
					// move child shapes with parent
					for (int i = 0; i < shapes.Count; ++i) {
						// move shape
						if (!shapes[i].MoveBy(deltaX, deltaY))
							result = false;
						// Update centerPoint: 
						// Due to the fact that Point is a value Type, we have to overwrite the Point with a new Point.
						Point shapeCenter = shapePositions[shapes[i]];
						shapeCenter.Offset(deltaX, deltaY);
						shapePositions[shapes[i]] = shapeCenter;
					}
					if (boundingRectangleTight != Geometry.InvalidRectangle)
						boundingRectangleTight.Offset(deltaX, deltaY);
					if (boundingRectangleLoose != Geometry.InvalidRectangle)
						boundingRectangleLoose.Offset(deltaX, deltaY);
				} finally { ResumeUpdate(); }
				return result;
			}
		}


		/// <summary>
		/// Notifies the child shapes that their parent has been resized. The action performed depends on the implementing class.
		/// </summary>
		/// <returns>
		/// True if all child shapes have resized in the desired way. 
		/// False if the child shapes cannot move as desired due to restrictions (such as connections).
		/// </returns>
		public abstract bool NotifyParentSized(int deltaX, int deltaY);


		public void SetPreviewStyles(IStyleSet styleSet) {
			if (styleSet == null) throw new ArgumentNullException("styleSet");
			foreach (Shape shape in shapes)
				shape.MakePreview(styleSet);
		}


		public bool ContainsPoint(int x, int y) {
			// TODO 2: Could be optimized using the shapeMap.
			foreach (Shape shape in TopDown) {
				if (shape.ContainsPoint(x, y))
					return true;
			}
			return false;
		}


		public bool IntersectsWith(int x, int y, int width, int height){
			foreach (Shape shape in TopDown)
				if (shape.IntersectsWith(x, y, width, height))
					return true;
			return false;
		}


		public bool NotifyStyleChanged(IStyle style) {
			bool result = false;
			foreach (Shape shape in shapes)
				if (shape.NotifyStyleChanged(style)) result = true;
			return result;
		}


		public void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			foreach (Shape shape in BottomUp)
				shape.Draw(graphics);

//#if DEBUG
//         foreach (KeyValuePair<Shape, Point> item in shapePositions) {
//            GdiHelpers.DrawPoint(graphics, Pens.Lime, item.Key.X, item.Key.Y, 3);
//            GdiHelpers.DrawPoint(graphics, Pens.Red, item.Value.X, item.Value.Y, 3);
//         }
//         GdiHelpers.DrawAngle(graphics, new SolidBrush(Color.FromArgb(96, Color.Yellow)), rotationCenter, Geometry.TenthsOfDegreeToDegrees(aggregationAngle), 20);
//         GdiHelpers.DrawPoint(graphics, Pens.Blue, rotationCenter.X, rotationCenter.Y, 3);
//#endif
		}


		public void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			foreach (Shape shape in BottomUp) shape.DrawOutline(graphics, pen);
		}
		
	
		protected internal void Invalidate() {
			foreach (Shape shape in shapes)
				shape.Invalidate();
		}


		//public void CalcGluePointFromPosition() {
		//}

		#endregion


		#region ShapeAggregation Methods called by the child shapes

		/// <override></override>
		public override void NotifyChildMoved(Shape shape) {
			base.NotifyChildMoved(shape);
			// reset rotation of the ShapeAggregation if any of the Children are changed directly
			if (!UpdateSuspended) {
				// reset rotation info and re-assign (unrotated) shape position
				//ResetRotation();
				
				UpdateShapePosition(shape);
				boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
			}
		}


		/// <override></override>
		public override void NotifyChildResized(Shape shape) {
			base.NotifyChildResized(shape);
			// reset rotation of the ShapeAggregation if any of the Children are changed directly
			if (!UpdateSuspended) {
				// reset rotation info and re-assign (unrotated) shape positions
				//ResetRotation();

				UpdateShapePosition(shape);
				boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
			}
		}


		/// <override></override>
		public override void NotifyChildRotated(Shape shape) {
			base.NotifyChildRotated(shape);
			if (!UpdateSuspended)
				boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
		}

		#endregion


		#region Overridden methods (protected)

		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			// Store position of the (unrotated) shape and reset bounding rectangle
			AddShapePosition(shape);
			// Set shape's parent
			shape.Parent = Owner;
			shape.DisplayService = Owner.DisplayService;
			return result;
		}


		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			// RemoveRange position of the (unrotated) shape and reset bounding rectangle
			shapePositions.Remove(shape);
			// Reset shape's parent and display service
			shape.Parent = null;
			shape.DisplayService = null;
			return result;
		}


		protected override void ReplaceCore(Shape oldShape, Shape newShape) {
			base.ReplaceCore(oldShape, newShape);
			// Reset old shape's Parent and DisplayService
			oldShape.Parent = null;
			oldShape.DisplayService = null;
			// Set new shape's Parent and DisplayService
			newShape.Parent = Owner;
			newShape.DisplayService = Owner.DisplayService;
		}


		protected override void ClearCore() {
			foreach (Shape shape in shapes) {
				shape.Invalidate();
				shape.Parent = null;
				shape.DisplayService = null;
			}
			shapePositions.Clear();
			base.ClearCore();
		}

		#endregion


		protected void SuspendUpdate() {
			++suspendCounter;
		}


		protected void ResumeUpdate() {
			Debug.Assert(suspendCounter > 0);
			--suspendCounter;
		}


		protected bool UpdateSuspended { get { return suspendCounter > 0; } }


		private void Construct(Shape owner) {
			Construct(owner, -1);
		}


		private void Construct(Shape owner, int capacity) {
			if (owner == null) throw new ArgumentNullException("owner");
			this.owner = owner;
			if (capacity <= 0) 
				shapePositions = new Dictionary<Shape, Point>();
			else shapePositions = new Dictionary<Shape, Point>(capacity);
		}


		private void AddShapePosition(Shape shape) {
			// get the shape's original center point (for restoring the unrotated position)
			Point shapeCenter = Point.Empty;
			shapeCenter.X = shape.X;
			shapeCenter.Y = shape.Y;
			shapePositions.Add(shape, shapeCenter);
		}


		private void RevertRotation() {
			for (int i = shapes.Count - 1; i >= 0; --i) {
				Point unrotatedShapeCenter = shapePositions[shapes[i]];
				shapes[i].Rotate(-aggregationAngle, shapes[i].X, shapes[i].Y);
				shapes[i].MoveTo(unrotatedShapeCenter.X, unrotatedShapeCenter.Y);
			}
		}


		//private void ResetRotation() {
		//   this.rotationCenter = Point.Empty;
		//   this.aggregationAngle = 0;
		//   foreach (Shape s in shapes) {
		//      Point p = Point.Empty;
		//      p.Offset(s.X, s.Y);
		//      shapePositions[s] = p;
		//   }
		//}


		private void UpdateShapePosition(Shape shape) {
			// Calculate the new unrotated shape position and 
			// update it in the shapePositions dictionary
			Point currentPos = Point.Empty;
			currentPos.Offset(shape.X, shape.Y);
			if (aggregationAngle != 0)
				currentPos = Geometry.RotatePoint(rotationCenter,
					-Geometry.TenthsOfDegreeToDegrees(aggregationAngle), currentPos);
			shapePositions[shape] = currentPos;
		}


		#region Fields

		private Shape owner = null;
		private int suspendCounter = 0;
		// Fields for rotating shapes
		private int aggregationAngle = 0;
		private Point rotationCenter = Point.Empty;
		// a list of unrotated positions of the shapes, used for rotating shapes precisely
		private Dictionary<Shape, Point> shapePositions;

		#endregion
	}

	#endregion


	#region GroupAggregation class

	public class GroupShapeAggregation : ShapeAggregation {

		public GroupShapeAggregation(Shape owner)
			: base(owner) {
			if (owner == null) throw new ArgumentNullException("owner");
			if (!(owner is IShapeGroup)) throw new ArgumentException("owner");
		}


		/// <override></override>
		public override void CopyFrom(IShapeCollection source) {
			base.CopyFrom(source);
			if (source is GroupShapeAggregation)
			   this.center = ((GroupShapeAggregation)source).Center;
			else CalcCenter();
		}


		/// <override></override>
		public override bool NotifyParentMoved(int deltaX, int deltaY) {
			bool result = true;
			try {
				SuspendUpdate();
				base.NotifyParentMoved(deltaX, deltaY);
				center.Offset(deltaX, deltaY);
			} finally { ResumeUpdate(); }
			return result;
		}


		/// <override></override>
		public override bool NotifyParentSized(int deltaX, int deltaY) {
			try {
				SuspendUpdate();
				// Nothing to do
			} finally { ResumeUpdate(); }
			return false;
		}


		/// <override></override>
		public override void NotifyChildMoving(Shape shape) {
			base.NotifyChildMoving(shape);
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
		}


		/// <override></override>
		public override void NotifyChildMoved(Shape shape) {
			base.NotifyChildMoved(shape);
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}


		/// <override></override>
		public override void NotifyChildResizing(Shape shape) {
			base.NotifyChildResizing(shape);
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
		}


		/// <override></override>
		public override void NotifyChildResized(Shape shape) {
			base.NotifyChildResized(shape);
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}


		/// <override></override>
		public override void NotifyChildRotating(Shape shape) {
			base.NotifyChildRotating(shape);
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
		}


		/// <override></override>
		public override void NotifyChildRotated(Shape shape) {
			base.NotifyChildRotated(shape);
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}


		public Point Center {
			get { return center; }
		}


		#region Overidden protected methods

		/// <override></override>
		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
			return result;
		}


		/// <override></override>
		protected override void AddRangeCore(IEnumerable<Shape> collection) {
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
			try {
				SuspendUpdate();
				base.AddRangeCore(collection);
			} finally { ResumeUpdate(); }
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}


		/// <override></override>
		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
			return result;
		}


		/// <override></override>
		protected override bool RemoveRangeCore(IEnumerable<Shape> shapes) {
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
			bool result = false;
			try {
				SuspendUpdate();
				result = base.RemoveRangeCore(shapes);
			} finally { ResumeUpdate(); }
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
			return result;
		}


		/// <override></override>
		protected override void ReplaceCore(Shape oldShape, Shape newShape) {
			base.ReplaceCore(oldShape, newShape);
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}


		/// <override></override>
		protected override void ReplaceRangeCore(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
			try {
				SuspendUpdate();
				base.ReplaceRangeCore(oldShapes, newShapes);
			} finally { ResumeUpdate(); }
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}


		/// <override></override>
		protected override void ClearCore() {
			base.ClearCore();
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChangeded();
			}
		}

		#endregion


		protected new IShapeGroup Owner {
			get { return (IShapeGroup)base.Owner; }
		}
		
		
		private void CalcCenter() {
			Rectangle r = GetBoundingRectangle(true);
			center.X = r.X + (int)Math.Round(r.Width / 2f);
			center.Y = r.Y + (int)Math.Round(r.Height / 2f);
		}


		#region Fields
		private Point center = Point.Empty;
		#endregion
	}

	#endregion


	#region CompositeShapeAggregation class

	public class CompositeShapeAggregation : ShapeAggregation {

		public CompositeShapeAggregation(Shape owner)
			: base(owner) {
			relativePositions = new Dictionary<Shape, RelativePosition>();
		}


		public CompositeShapeAggregation(Shape owner, int capacity)
			: base(owner, capacity) {
			relativePositions = new Dictionary<Shape, RelativePosition>(capacity);
		}


		public CompositeShapeAggregation(Shape owner, IEnumerable<Shape> collection)
			: base(owner, (collection is ICollection<Shape>) ? ((ICollection<Shape>)collection).Count : -1) {
			if (collection == null) throw new ArgumentNullException("collection");
			// if collection provides an indexer, use it in order to reduce overhead
			if (collection is IList<Shape>) {
				IList<Shape> shapeList = (IList<Shape>)collection;
				int cnt = shapeList.Count;
				// Create dictionary of relative positions
				relativePositions = new Dictionary<Shape, RelativePosition>(cnt);
				// Add shapes
				for (int i = 0; i < cnt; ++i)
					Add(shapeList[i]);
			} else {
				// Create dictionary of relative positions
				if (collection is ICollection<Shape>)
					relativePositions = new Dictionary<Shape, RelativePosition>(((ICollection<Shape>)collection).Count);
				else relativePositions = new Dictionary<Shape, RelativePosition>();
				// Add shapes
				foreach (Shape shape in collection)
					Add(shape);
			}
		}


		public override void CopyFrom(IShapeCollection source) {
			base.CopyFrom(source);
			if (source is CompositeShapeAggregation) {
				CompositeShapeAggregation src = (CompositeShapeAggregation)source;
				// Copy relative positions of the children
				relativePositions.Clear();
				for (int i = 0; i < shapes.Count; ++i) {
					RelativePosition pos = src.relativePositions[src.shapes[i]];
					relativePositions.Add(shapes[i], pos);
				}
			}
		}


		public override bool NotifyParentSized(int deltaX, int deltaY) {
			bool result = true;
			try {
				SuspendUpdate();
				// move children to their new absolute position calculated from relativePosition
				for (int i = 0; i < shapes.Count; ++i) {
					Point p = Owner.CalculateAbsolutePosition(relativePositions[shapes[i]]);
					shapes[i].MoveTo(p.X, p.Y);
				}
				boundingRectangleLoose = boundingRectangleTight = Geometry.InvalidRectangle;
			} finally {
				ResumeUpdate();
			}
			return result;
		}


		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			AddRelativePosition(shape);
			return result;
		}


		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			relativePositions.Remove(shape);
			return result;
		}


		protected override void ClearCore() {
			relativePositions.Clear();
			base.ClearCore();
		}


		private void AddRelativePosition(Shape shape) {
			RelativePosition relativePos = Owner.CalculateRelativePosition(shape.X, shape.Y);
			relativePositions.Add(shape, relativePos);
		}


		#region Fields
		private Dictionary<Shape, RelativePosition> relativePositions;
		#endregion
	}

	#endregion

}