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

	/// <summary>
	/// Marker interface for groups.
	/// </summary>
	public interface IShapeGroup {

		void NotifyChildLayoutChanging();

		void NotifyChildLayoutChangeded();

	}


	/// <summary>
	/// Groups a set of shapes together.
	/// </summary>
	public class ShapeGroup : Shape, IShapeGroup {

		internal static ShapeGroup CreateInstance(ShapeType shapeType, Template template) {
			return new ShapeGroup(shapeType, template);
		}


		#region Shape Members

		public override void CopyFrom(Shape source) {
			if (source == null) throw new ArgumentNullException("source");

			this.permissionSetName = source.SecurityDomainName;
			this.tag = source.Tag;
			this.displayService = source.DisplayService;
			//this.Parent = source.Parent;
			
			//this.location.X = source.X;
			//this.location.Y = source.Y;

			// Do not recalculate Center after adding the children because in case the group is rotated, 
			// the rotation center would not be the same.
			if (source.Children != null)
				children.CopyFrom(source.Children);

			// Do not assign to the property but to the field because the children are already on their (rotated) positions
			if (source is IPlanarShape)
				this.angle = ((IPlanarShape)source).Angle;
			if (source is ShapeGroup)
				this.angle = ((ShapeGroup)source).Angle;
		}


		public override ShapeType Type {
			get { return shapeType; }
		}


		public override IModelObject ModelObject {
			get { return modelObject; }
			set { modelObject = value; }
		}


		public override Template Template {
			get { return template; }
		}


		[Browsable(false)]
		public override Diagram Diagram {
			get {
				if (owner is DiagramShapeCollection)
					return ((DiagramShapeCollection)owner).Owner;
				else return null;
			}
			set {
				if (owner != null && owner.Contains(this)) {
					owner.Remove(this);
					owner = null;
				}
				if (value != null) {
					if (value.Shapes is ShapeCollection)
						owner = (ShapeCollection)value.Shapes;
					else throw new ArgumentException(string.Format("{0}'s Shapes property must be a {1}", value.GetType().Name, typeof(ShapeCollection).Name));
				} else owner = null;
			}
		}


		[Browsable(false)]
		public override Shape Parent {
			get {
				if (owner is ShapeAggregation) return ((ShapeAggregation)owner).Owner;
				else return null;
			}
			set {
				if (value != null) {
					if (owner != null && owner != value.Children) {
						owner.Remove(this);
						owner = null;
					}
					if (value is ShapeGroup)
						owner = ((ShapeGroup)value).children;
					else if (value.Children is ShapeAggregation)
						owner = (ShapeAggregation)value.Children;
					else throw new ArgumentException(string.Format("{0}'s Children property must be a {1}", value.GetType().Name, typeof(ShapeAggregation).Name));
				} else owner = null;
			}
		}


		public override IShapeCollection Children {
			get { return children; }
		}


		public override object Tag {
			get { return tag; }
			set { tag = value; }
		}


		public override char SecurityDomainName {
			get { return permissionSetName; }
			set { permissionSetName = value; }
		}


		public override IEnumerable<nShapeAction> GetActions(int mouseX, int mouseY, int range) {
			// no actions for the moment...
			if (template != null) {
				foreach (nShapeAction action in template.GetActions())
					yield return action;
			}
			if (modelObject != null) {
				foreach (nShapeAction action in modelObject.GetActions())
					yield return action;
			}
		}


		public override void NotifyModelChanged(int modelPropertyId) {
			for (int i = children.Count - 1; i >= 0; --i)
				NotifyModelChanged(modelPropertyId);
		}


		public override void Connect(ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId) {
			// nothing to do
		}


		public override void Disconnect(ControlPointId gluePointId) {
			// nothing to do
		}


		public override IEnumerable<ShapeConnectionInfo> GetConnectionInfos(ControlPointId ownPointId, Shape otherShape) {
			yield break;
		}


		public override ShapeConnectionInfo GetConnectionInfo(ControlPointId gluePointId, Shape otherShape) {
			return ShapeConnectionInfo.Empty;
		}


		public override ControlPointId IsConnected(ControlPointId ownPointId, Shape otherShape) {
			return ControlPointId.None;
		}


		public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			// nothing to do
		}


		public override RelativePosition CalculateRelativePosition(int x, int y) {
			RelativePosition result;
			result.A = x - X;
			result.B = y - Y;
			return result;
		}


		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			Point result = Point.Empty;
			result.X = relativePosition.A + X;
			result.Y = relativePosition.A + Y;
			return result;
		}


		public override Point CalculateNormalVector(int x, int y) {
			return Geometry.CalcNormalVectorOfRectangle(GetBoundingRectangle(true), x, y, 100);
		}


		public override Point CalculateConnectionFoot(int fromX, int fromY) {
			Point result = Point.Empty;
			result.Offset(X, Y);
			float distance, lowestDistance = float.MaxValue;
			// Use the nearest intersection point not contained by an other shape of the group
			foreach (Shape shape in children) {
				// ToDo3: Improve this implementation by using InterSectOutlineWithLineSegment(fromX, fromY, X, Y)
				Point p = shape.CalculateConnectionFoot(fromX, fromY);
				if (p == Point.Empty) {
					ControlPointId nearestCtrlPtId = shape.FindNearestControlPoint(fromX, fromY, int.MaxValue, ControlPointCapabilities.All);
					if (nearestCtrlPtId != ControlPointId.None)
						p = GetControlPointPosition(nearestCtrlPtId);
					else {
						p.X = int.MaxValue;
						p.Y = int.MaxValue;
					}
				}
				// calculate distance to point and set result if a new nearest point is found
				distance = Geometry.DistancePointPoint(p.X, p.Y, fromX, fromY);
				if (distance < lowestDistance) {
					// If the new nearest point is contained by an other shape:
					// Skip it, as we do not want lines that cross, intersect or end within shapes of the group
					bool otherShapeContainsPoint = false;
					foreach (Shape s in children.BottomUp) {
						if (s == shape) continue;
						if (s.ContainsPoint(p.X, p.Y)) {
							otherShapeContainsPoint = true;
							break;
						}
					}
					if (!otherShapeContainsPoint) {
						lowestDistance = distance;
						result = p;
					}
				}
			}
			return result;
		}


		public override bool IntersectsWith(int x, int y, int width, int height) {
			Rectangle rect = Rectangle.Empty;
			rect.X = x;
			rect.Y = y;
			rect.Width = width;
			rect.Height = height;
			if (children.Count <= 0) return rect.Contains(X, Y);
			else if (Geometry.RectangleIntersectsWithRectangle(rect, children.GetBoundingRectangle(false))) {
				foreach (Shape shape in children)
					if (shape.IntersectsWith(x, y, width, height))
						return true;
			}
			return false;
		}


		public override IEnumerable<Point> IntersectOutlineWithLineSegment(int x1, int y1, int x2, int y2) {
			// Use the nearest intersection point not contained by an other shape of the group
			foreach (Shape shape in children) {
				foreach (Point p in shape.IntersectOutlineWithLineSegment(x1, y1, x2, y2))
					if (!ContainsPoint(p.X, p.Y)) yield return p;
			}
		}


		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			//if ((controlPointCapability & ControlPointCapabilities.Reference) > 0)
			//   if (Geometry.DistancePointPoint(X, Y, x, y) <= distance)
			//      return true;
			if ((controlPointCapability & ControlPointCapabilities.Rotate) > 0) {
				if (Geometry.DistancePointPoint(RotatePoint.X, RotatePoint.Y, x, y) <= range)
					return RotatePointId;
				controlPointCapability ^= ControlPointCapabilities.Rotate;
			}
			foreach (Shape shape in children) {
				ControlPointId pointId = shape.HitTest(x, y, controlPointCapability, range);
				if (pointId != ControlPointId.None) return pointId;
			}
			return ControlPointId.None;
		}


		public override bool ContainsPoint(int x, int y) {
			if (X == x && Y == y)
				return true;
			// TODO 2: Can be optimized using the shape map.
			foreach (Shape shape in children)
				if (shape.ContainsPoint(x, y))
					return true;
			return false;
		}


		public override Rectangle GetBoundingRectangle(bool tight) {
			Rectangle result = Rectangle.Empty;
			if (children.Count <= 0) result.Offset(X, Y);
			else {
				result = children.GetBoundingRectangle(tight);
				if (!result.Contains(X, Y))
					result = Geometry.UniteRectangles(X, Y, X, Y, result);
			}
			return result;
		}


		public override IEnumerable<Point> CalculateCells(int cellSize) {
			foreach (Shape s in children)
				foreach (Point p in s.CalculateCells(cellSize))
					yield return p;
		}


		//public override int X {
		//   get { return location.X; }
		//   set {
		//      int origValue = location.X;
		//      if (!MoveTo(value, location.Y)) {
		//         MoveTo(origValue, location.Y);
		//         throw new InvalidOperationException(string.Format("Shape cannot move to {0}.", new Point(value, location.Y)));
		//      }
		//   }
		//}


		//public override int Y {
		//   get { return location.Y; }
		//   set {
		//      int origValue = location.Y;
		//      if (!MoveTo(location.X, value)) {
		//         MoveTo(location.X, origValue);
		//         throw new InvalidOperationException(string.Format("Shape cannot move to {0}.", new Point(location.X, value)));
		//      }
		//   }
		//}


		public override int X {
			get { return children.Center.X; }
			set {
				int origValue = children.Center.X;
				if (!MoveTo(value, children.Center.Y)) {
					MoveTo(origValue, children.Center.Y);
					throw new InvalidOperationException(string.Format("Shape cannot move to {0}.", new Point(value, children.Center.Y)));
				}
			}
		}


		public override int Y {
			get { return children.Center.Y; }
			set {
				int origValue = children.Center.Y;
				if (!MoveTo(children.Center.X, value)) {
					MoveTo(children.Center.X, origValue);
					throw new InvalidOperationException(string.Format("Shape cannot move to {0}.", new Point(children.Center.X, value)));
				}
			}
		}


		public override void Fit(int x, int y, int width, int height) {
			MoveTo(x + width / 2, y + height / 2);
		}


		public override bool MoveBy(int deltaX, int deltaY) {
			bool result = false;
			Invalidate();
			if (Owner != null) Owner.NotifyChildMoving(this);

			//location.Offset(deltaX, deltaY);
			result = children.NotifyParentMoved(deltaX, deltaY);

			if (Owner != null) Owner.NotifyChildMoved(this);
			Invalidate();
			return result;
		}


		public override bool MoveControlPointBy(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			bool result = false;
			if (pointId == ControlPointId.Reference)
				result = MoveBy(deltaX, deltaY);
			else {
				if (Owner != null) Owner.NotifyChildResizing(this);
				result = false;
				if (Owner != null) Owner.NotifyChildResized(this);
			}
			return result;
		}


		public override bool Rotate(int deltaAngle, int x, int y) {
			bool result = true;
			// Notify Owner
			if (Owner != null) Owner.NotifyChildRotating(this);

			// first, perform rotation around the center point...
			angle = (3600 + angle + deltaAngle) % 3600;
			// ...then, rotate the shape's center around the given rotation center and 
			// move the shape (including its children) to this point
			if (x != X || y != Y) {
				int toX = X;
				int toY = Y;
				Geometry.RotatePoint(x, y, Geometry.TenthsOfDegreeToDegrees(deltaAngle), ref toX, ref toY);
				if (!MoveTo(toX, toY)) result = false;
			}

			// Notify children and owner
			if (!children.NotifyParentRotated(deltaAngle, X, Y)) result = false;
			if (Owner != null) Owner.NotifyChildRotated(this);
			return result;
		}


		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			if (controlPointId == ControlPointId.Reference)
				//return location;
				return children.Center;
			else if (controlPointId == RotatePointId)
				return RotatePoint;
			else if (controlPointId == ControlPointId.None)
				throw new ArgumentException(string.Format("{0} is not a valid {1} for this operation.", controlPointId, typeof(ControlPointId).Name));
			return Point.Empty;
		}


		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			if ((controlPointCapability & ControlPointCapabilities.Reference) > 0
				|| (controlPointCapability & ControlPointCapabilities.Rotate) > 0)
				yield return RotatePointId;
		}


		public override ControlPointId FindNearestControlPoint(int x, int y, int distance, ControlPointCapabilities controlPointCapability) {
			if ((controlPointCapability & ControlPointCapabilities.Reference) > 0) {
				if (Geometry.DistancePointPoint(x, y, X, Y) <= distance)
					return ControlPointId.Reference;
			} else if ((controlPointCapability & ControlPointCapabilities.Rotate) > 0) {
				if (Geometry.DistancePointPoint(x, y, RotatePoint.X, RotatePoint.Y) <= distance)
					return RotatePointId;
			}
			return ControlPointId.None;
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if (controlPointId == RotatePointId || controlPointId == ControlPointId.Reference)
				return ((controlPointCapability & ControlPointCapabilities.Rotate) > 0
					|| (controlPointCapability & ControlPointCapabilities.Reference) > 0);
			return false;
		}


		public override IDisplayService DisplayService {
			get { return displayService; }
			set {
				if (displayService != value) {
					displayService = value;
					if (children != null && children.Count > 0)
						children.SetDisplayService(displayService);
				}
			}
		}


		public override void MakePreview(IStyleSet styleSet) {
			foreach (Shape shape in children)
				shape.MakePreview(styleSet);
		}


		public override ILineStyle LineStyle {
			// Shape groups do not have a line style. They return null and ignore the setting.
			get {
				return null;
			}
			set {
				// Nothing to do
			}
		}


		public override bool NotifyStyleChanged(IStyle style) {
			bool result = false;
			foreach (Shape shape in children)
				if (shape.NotifyStyleChanged(style)) result = true;
			return result;
		}


		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			children.Draw(graphics);
		}


		public override void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			children.DrawOutline(graphics, pen);
		}


		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			if (image == null) throw new ArgumentNullException("image");
			using (Graphics gfx = Graphics.FromImage(image)) {
				GdiHelpers.ApplyGraphicsSettings(gfx, nShapeRenderingQuality.MaximumQuality);
				gfx.Clear(transparentColor);

				using (Font font = new Font(FontFamily.GenericSansSerif, 9)) {
					Rectangle layoutRect = Rectangle.Empty;
					layoutRect.Size = image.Size;
					layoutRect.Inflate(-margin, -margin);
					gfx.DrawString("Icon", font, Brushes.Black, layoutRect);
					font.Dispose();
				}
			}
		}


		public override void Invalidate() {
			if (displayService != null) 
				displayService.Invalidate(GetBoundingRectangle(false));
		}

		#endregion


		public int Angle {
			get { return angle; }
			set {
				Invalidate();
				int deltaAngle = value - angle;
				Rotate(deltaAngle, X, Y);
				Invalidate();
			}
		}


		#region IShapeGroup Members

		public void NotifyChildLayoutChanging() {
			if (Owner != null) Owner.NotifyChildResizing(this);
		}


		public void NotifyChildLayoutChangeded() {
			if (Owner != null) Owner.NotifyChildResized(this);
		}

		#endregion


		#region IEntity Members

		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Template", typeof(object));
			yield return new EntityFieldDefinition("X", typeof(int));
			yield return new EntityFieldDefinition("Y", typeof(int));
			yield return new EntityFieldDefinition("ZOrder", typeof(int));
			yield return new EntityFieldDefinition("Layers", typeof(int));
			yield return new EntityFieldDefinition("Angle", typeof(int));
			// yield return new EntityInnerObjectsDefinition(childPosPropertyName, "Core.Point", pointAttrNames, pointAttrTypes);
		}


		protected override sealed object IdCore { get { return id; } }


		protected override sealed void AssignIdCore(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null) throw new InvalidOperationException("Shape group has already an id.");
			this.id = id;
		}


		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			template = reader.ReadTemplate();
			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			MoveTo(x, y);
			ZOrder = reader.ReadInt32();
			Layers = (LayerIds)reader.ReadInt32();
			angle = reader.ReadInt32();
		}


		protected override void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			// Do the actual writing
			writer.WriteTemplate(template);
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(ZOrder);
			writer.WriteInt32((int)Layers);
			writer.WriteInt32(angle);
		}


		protected override void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		protected override void DeleteCore(IRepositoryWriter writer, int version) {
			/*foreach (EntityPropertyDefinition pi in Type.PropertyInfos) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}*/
		}

		#endregion


		#region ICloneable Members

		public override Shape Clone() {
			Shape result = new ShapeGroup(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}

		#endregion


		#region IDisposable Members

		public override void Dispose() {
			if (ModelObject != null) ModelObject = null;
			foreach (Shape child in children) child.Dispose();
		}

		#endregion


		protected internal ShapeGroup(ShapeType shapeType, Template template)
			: base() {
			if (shapeType == null) throw new ArgumentNullException("shapeType");
			this.shapeType = shapeType;
			this.template = template;
			children = new GroupShapeAggregation(this);
		}


		protected internal ShapeGroup(ShapeType shapeType, IStyleSet styleSet)
			: base() {
			if (shapeType == null) throw new ArgumentNullException("shapeType");
			this.shapeType = shapeType;
			this.template = null;
			children = new GroupShapeAggregation(this);
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			foreach (Shape shape in children)
				shape.InitializeToDefault(styleSet);
		}


		protected internal override void AttachGluePointToConnectionPoint(ControlPointId ownPointId, Shape otherShape, ControlPointId gluePointId) {
			if (ownPointId != ControlPointId.Reference && !HasControlPointCapability(ownPointId, ControlPointCapabilities.Connect))
				throw new nShapeException(string.Format("{0}'s point {1} has to be a connection point.", Type.Name, ownPointId));
			if (!otherShape.HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue))
				throw new nShapeException(string.Format("{0}'s point {1} has to be a glue point.", otherShape.Type.Name, gluePointId));
			throw new NotSupportedException();
		}


		protected internal override void DetachGluePointFromConnectionPoint(ControlPointId ownPointId, Shape targetShape, ControlPointId targetPointId) {
			throw new NotSupportedException();
		}


		protected ShapeCollection Owner {
			get { return owner; }
		}


		private Point RotatePoint {
			get {
				if (children != null && children.Count > 0)
					return children.Center;
				else
					//return location;
					return Point.Empty;
			}
		}


		#region Fields

		// Shape fields
		private Template template = null;
		private IModelObject modelObject = null;
		private ShapeType shapeType = null;
		private IDisplayService displayService;
		private char permissionSetName = 'A';
		private ShapeCollection owner = null;
		private GroupShapeAggregation children;
		private object id = null;
		private object tag = null;
		//private Point location;
		
		// ShapeGroup fields
		private const int RotatePointId = 1;
		private int angle;
		#endregion
	}

}
