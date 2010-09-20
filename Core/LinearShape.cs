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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// One-dimensional shape defined by a sequence of vertices.
	/// </summary>
	public interface ILinearShape {

		/// <summary>
		/// Adds a new ControlPoint to the interior of the shape.
		/// </summary>
		/// <param name="x">X coordinate of the new Controlpoint.</param>
		/// <param name="y">Y coordinate of the new ControlPoint.</param>
		/// <returns>ControlPointId of the new ControlPoint</returns>
		ControlPointId AddVertex(int x, int y);

		/// <summary>
		/// Inserts a new ControlPoint to the shape before the ControlPoint with the 
		/// given Id. The new Point may be outside the shape, thus changing the outline of the shape.
		/// </summary>
		/// <param name="beforePointId">PointId of the ControlPoint the new point should be inserted before.</param>
		/// <param name="x">X coordinate of the new ControlPoint.</param>
		/// <param name="y">Y coordinate of the new ControlPoint.</param>
		/// <returns>ControlPointId of the new ControlPoint</returns>
		ControlPointId InsertVertex(ControlPointId beforePointId, int x, int y);

		/// <summary>
		/// Removes the point with the given ControlPointId from the line.
		/// </summary>
		/// <param name="pointId">PointId of the point to remove.</param>
		void RemoveVertex(ControlPointId pointId);

		///// <summary>
		///// All Vertices of the line, ordered by their appearance on the line from start point to end point.
		///// A Vertex is defined as a ControlPoint which defines the shape of the linear shape.
		///// </summary>
		//IEnumerable<Point> Vertices { get; }

		/// <summary>
		/// Returns the <see cref="T:Dataweb.NShape.Advanced.ControlPointId" /> of the vertex next to the vertex associated with the given <see cref="T:Dataweb.NShape.Advanced.ControlPointId" /> (Direction: <see cref="T:Dataweb.NShape.Advanced.ControlPointId.FirstVertex" /> to <see cref="T:Dataweb.NShape.Advanced.ControlPointId.LastVertex" />).
		/// </summary>
		ControlPointId GetNextVertexId(ControlPointId vertexId);

		/// <summary>
		/// Returns the <see cref="T:Dataweb.NShape.Advanced.ControlPointId" /> of the vertex next to the vertex associated with the given <see cref="T:Dataweb.NShape.Advanced.ControlPointId" /> (Direction: <see cref="T:Dataweb.NShape.Advanced.ControlPointId.LastVertex" /> to <see cref="T:Dataweb.NShape.Advanced.ControlPointId.FirstVertex" />).
		/// </summary>
		ControlPointId GetPreviousVertexId(ControlPointId vertexId);

		/// <summary>Specifies the minimum number of vertices needed for defining the linear shape</summary>
		int MinVertexCount { get; }

		/// <summary>Specifies the maximum number of vertices allowed for defining the shape.</summary>
		int MaxVertexCount { get; }

		/// <summary>Specifies the current number of vertices the linear shape consists of.</summary>
		int VertexCount { get; }

		/// <summary>Calculates the point where a connected shape should intersect with the current shape.</summary>
		Point CalculateConnectionFoot(int x1, int y1, int x2, int y2);

		/// <summary>Calculates the normal vector for the given point.</summary>
		Point CalcNormalVector(Point point);

		/// <summary>Indicates if the line has a direction, e.g. a cap on one side.</summary>
		bool IsDirected { get; }

	}


	/// <summary>
	/// A one-dimensional shape
	/// </summary>
	/// <remarks>RequiredPermissions set</remarks>
	public abstract class LineShapeBase : ShapeBase, ILinearShape {

		#region Shape Members

		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is LineShapeBase) {
				// Copy templated properties
				ICapStyle capStyle;
				capStyle = ((LineShapeBase)source).StartCapStyleInternal;
				privateStartCapStyle = (Template != null && capStyle == ((LineShapeBase)Template.Shape).StartCapStyleInternal) ? null : capStyle;

				capStyle = ((LineShapeBase)source).EndCapStyleInternal;
				privateEndCapStyle = (Template != null && capStyle == ((LineShapeBase)Template.Shape).EndCapStyleInternal) ? null : capStyle;
			}
			if (source is ILinearShape) {
				ILinearShape srcLine = ((ILinearShape)source);
				// Copy vertices
				if (srcLine.MinVertexCount <= MaxVertexCount) {
					int srcLastVertexIdx = srcLine.VertexCount - 1;
					int ownMaxVertexIdx = MaxVertexCount - 1;
					int vertexIdx = -1;
					// Try to copy all vertices of the source shape
					Point p = Point.Empty;
					ControlPointId vertexId = ControlPointId.FirstVertex;
					do {
						p = source.GetControlPointPosition(vertexId);
						++vertexIdx;
						if (vertexId == ControlPointId.FirstVertex) {
							// Move start point
							int idx = GetControlPointIndex(ControlPointId.FirstVertex);
							MovePointByCore(ControlPointId.FirstVertex, p.X - vertices[idx].X, p.Y - vertices[idx].Y, ResizeModifiers.None);
						} else if (vertexIdx == srcLastVertexIdx) {
							// Move end point
							int idx = GetControlPointIndex(ControlPointId.LastVertex);
							MovePointByCore(ControlPointId.LastVertex, p.X - vertices[idx].X, p.Y - vertices[idx].Y, ResizeModifiers.None);
						} else if (vertexIdx >= ownMaxVertexIdx) {
							// if the line's maximum vertex count is reached, 
							// skip all but the source's last vertex
							continue;
						} else if (VertexCount < srcLine.VertexCount) {
							// If the destination shape has not enough vertices, insert a new vertex
							InsertVertex(ControlPointId.LastVertex, p.X, p.Y);
							pointIds[vertexIdx] = vertexId;	// maintain ControlPointIds of source
						} else {
							// If there destination already has enough vertices, move the shape's 
							// vertex to the source's vertex position 
							vertices[vertexIdx] = p;
							pointIds[vertexIdx] = vertexId;	// maintain ControlPointIds of source
						}
						vertexId = srcLine.GetNextVertexId(vertexId);
					} while (vertexId != ControlPointId.None);
				}
			}
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			if (StartCapStyleInternal == null)
				privateStartCapStyle = styleSet.GetPreviewStyle(styleSet.CapStyles.None);
			else privateStartCapStyle = styleSet.GetPreviewStyle(StartCapStyleInternal);
			if (EndCapStyleInternal == null)
				privateEndCapStyle = styleSet.GetPreviewStyle(styleSet.CapStyles.None);
			else privateEndCapStyle = styleSet.GetPreviewStyle(EndCapStyleInternal);
		}


		/// <override></override>
		public abstract Point CalculateConnectionFoot(int x1, int y1, int x2, int y2);


		/// <override></override>
		public override void Connect(ControlPointId ownPointId, Shape otherShape, ControlPointId otherPointId) {
			if (otherShape == null) throw new ArgumentNullException("otherShape");
			if (otherShape.IsConnected(ControlPointId.Any, this) == ControlPointId.Reference)
				throw new InvalidOperationException(string.Format("The specified {0} is already connected to this {1} via Point-To-Shape connection.", otherShape.Type.Name, this.Type.Name));
			base.Connect(ownPointId, otherShape, otherPointId);
		}
		
		
		/// <override></override>
		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			return Enumerator.Create(this, controlPointCapability);
		}


		/// <override></override>
		public override bool NotifyStyleChanged(IStyle style) {
			bool result = base.NotifyStyleChanged(style);
			if (style == null || IsStyleAffected(StartCapStyleInternal, style) || IsStyleAffected(EndCapStyleInternal, style)) {
				Invalidate();
				InvalidateDrawCache();
				Invalidate();
				result = true;
			}
			return result;
		}


		/// <override></override>
		public override void Invalidate() {
			base.Invalidate();
			if (DisplayService != null) {
				if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
					DisplayService.Invalidate(StartCapBounds);
				if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
					DisplayService.Invalidate(EndCapBounds);
			}
		}


		/// <override></override>
		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			if (controlPointId == ControlPointId.None || controlPointId == ControlPointId.Any)
				throw new InvalidOperationException(string.Format("{0} is not a valid value for this operation", controlPointId));
			else if (controlPointId == ControlPointId.Reference) {
				Point p = Point.Empty;
				p.Offset(X, Y);
				return p;
			} else {
				int idx = GetControlPointIndex(controlPointId);
				return vertices[idx];
			}
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if ((controlPointCapability & ControlPointCapabilities.Connect) == ControlPointCapabilities.Connect
				&& !IsConnectionPointEnabled(controlPointId))
				return false;
			if (controlPointId == ControlPointId.Reference || controlPointId == ControlPointId.None || controlPointId == ControlPointId.Any)
				return base.HasControlPointCapability(controlPointId, controlPointCapability);
			else if (IsFirstVertex(controlPointId)) {
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0
					|| (controlPointCapability & ControlPointCapabilities.Reference) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			} else if (IsLastVertex(controlPointId)) {
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			} else {
				int pointIdx = GetControlPointIndex(controlPointId);
				if (pointIdx > 0 && pointIdx < VertexCount - 1)
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 || (controlPointCapability & ControlPointCapabilities.Resize) != 0);
				else return false;
					//throw new IndexOutOfRangeException();
			}
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(int mouseX, int mouseY, int range) {
			// return actions of base class
			IEnumerator<MenuItemDef> enumerator = GetBaseMenuItemDefs(mouseX, mouseY, range);
			while (enumerator.MoveNext()) yield return enumerator.Current;

			// return own actions
			bool isFeasible;
			string description;
			ControlPointId clickedPointId = HitTest(mouseX, mouseY, ControlPointCapabilities.All, range);

			isFeasible = ContainsPoint(mouseX, mouseY) 
				&& (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference)
				&& (VertexCount < MaxVertexCount);
			description = "You have to click on the line in order to insert new points";
			yield return new CommandMenuItemDef("Insert Point", null, description, isFeasible,
				new AddVertexCommand(this, mouseX, mouseY));

			isFeasible = false;
			if (HasControlPointCapability(clickedPointId, ControlPointCapabilities.Resize))
				if (!HasControlPointCapability(clickedPointId, ControlPointCapabilities.Glue))
					if ((clickedPointId != ControlPointId.None && IsConnected(clickedPointId, null) == ControlPointId.None))
						if (VertexCount > MinVertexCount)
							isFeasible = true;
			//isFeasible = HasControlPointCapability(clickedPointId, ControlPointCapabilities.Resize)
			//   && !HasControlPointCapability(clickedPointId, ControlPointCapabilities.Glue)
			//   && (clickedPointId != ControlPointId.None && IsConnected(clickedPointId, null) == ControlPointId.None)
			//   && (VertexCount > MinVertexCount);
			if (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference)
				description = "No control point was clicked";
			else description = "Glue control points may not be removed.";
			yield return new CommandMenuItemDef("Remove Point", null, description, isFeasible,
				new RemoveVertexCommand(this, clickedPointId));
		}


		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.LineShapeBase" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in ShapeBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("StartCapStyle", typeof(object));
			yield return new EntityFieldDefinition("EndCapStyle", typeof(object));
			yield return new EntityInnerObjectsDefinition(attrNameVertices, "Core.Point", pointAttrNames, pointAttrTypes);
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteStyle(privateStartCapStyle);
			writer.WriteStyle(privateEndCapStyle);
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			privateStartCapStyle = reader.ReadCapStyle();
			privateEndCapStyle = reader.ReadCapStyle();
		}


		/// <override></override>
		protected override void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == attrNameVertices) {
				// Save points
				writer.BeginWriteInnerObjects();
				for (int i = 0; i < pointIds.Count; ++i) {
					Point p = GetControlPointPosition(pointIds[i]);
					writer.BeginWriteInnerObject();
					writer.WriteInt32(i);
					writer.WriteInt32(pointIds[i]);
					writer.WriteInt32(p.X);
					writer.WriteInt32(p.Y);
					writer.EndWriteInnerObject();
				}
				writer.EndWriteInnerObjects();
			} else base.SaveInnerObjectsCore(propertyName, writer, version);
		}


		/// <override></override>
		protected override void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == attrNameVertices) {
				vertices.Clear();
				pointIds.Clear();
				// Load points
				int pointId, pointIdx, x, y;
				reader.BeginReadInnerObjects();
				while (reader.BeginReadInnerObject()) {
					pointIdx = reader.ReadInt32();
					pointId = reader.ReadInt32();
					x = reader.ReadInt32();
					y = reader.ReadInt32();
					Point vertex = Point.Empty;
					vertex.Offset(x, y);
					vertices.Insert(pointIdx, vertex);
					pointIds.Insert(pointIdx, pointId);
					reader.EndReadInnerObject();
				}
				reader.EndReadInnerObjects();
			} else base.LoadInnerObjectsCore(propertyName, reader, version);
		}

		#endregion


		#region ILinearShape Members

		/// <override></override>
		public abstract ControlPointId InsertVertex(ControlPointId beforePointId, int x, int y);


		/// <override></override>
		public abstract ControlPointId AddVertex(int x, int y);


		/// <override></override>
		public abstract void RemoveVertex(ControlPointId controlPointId);


		/// <override></override>
		public abstract Point CalcNormalVector(Point p);


		/// <override></override>
		public abstract int MinVertexCount { get; }


		/// <override></override>
		public abstract int MaxVertexCount { get; }


		/// <override></override>
		public virtual int VertexCount {
			get { return vertices.Count; }
		}


		/// <summary>
		/// Retrieve the id of the next neighbor point of pointId in physical order "start to end"
		/// </summary>
		public ControlPointId GetNextVertexId(ControlPointId pointId) {
			switch (pointId) {
				case ControlPointId.Any:
				case ControlPointId.None:
				case ControlPointId.Reference:
					return ControlPointId.None;
				default:
					int ptIdx = GetControlPointIndex(pointId);
					++ptIdx;
					if (ptIdx >= 0 && ptIdx < VertexCount)
						return GetControlPointId(ptIdx);
					else return ControlPointId.None;
			}
		}


		/// <summary>
		/// Retrieve the id of the previous neighbor point of pointId in physical order "start to end"
		/// </summary>
		public ControlPointId GetPreviousVertexId(ControlPointId pointId) {
			switch (pointId) {
				case ControlPointId.Any:
				case ControlPointId.None:
				case ControlPointId.Reference:
					return ControlPointId.None;
				default:
					int ptIdx = GetControlPointIndex(pointId);
					if (ptIdx > 0 && ptIdx <= VertexCount - 1)
						return GetControlPointId(ptIdx - 1);
					else return ControlPointId.None;
			}
		}


		/// <override></override>
		public bool IsDirected {
			get {
				return (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
					|| (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None);
			}
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// Internal start CapStyle of the line. May be published by a decendant through a property
		/// </summary>
		protected ICapStyle StartCapStyleInternal {
			get { return privateStartCapStyle ?? ((LineShapeBase)Template.Shape).StartCapStyleInternal; }
			set {
				Invalidate();
				if (Owner != null) Owner.NotifyChildResizing(this);
				
				privateStartCapStyle = (Template != null && value == ((LineShapeBase)Template.Shape).StartCapStyleInternal) ? null : value;
				InvalidateDrawCache();
				
				if (Owner != null) Owner.NotifyChildResized(this);
				Invalidate();
			}
		}


		/// <summary>
		/// Internal end CapStyle of the line. May be published by a decendant through a property
		/// </summary>
		protected ICapStyle EndCapStyleInternal {
			get { return privateEndCapStyle ?? ((LineShapeBase)Template.Shape).EndCapStyleInternal; }
			set {
				Invalidate();
				if (Owner != null) Owner.NotifyChildResizing(this);

				privateEndCapStyle = (Template != null && value == ((LineShapeBase)Template.Shape).EndCapStyleInternal) ? null : value;
				InvalidateDrawCache();

				if (Owner != null) Owner.NotifyChildResized(this);
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected float StartCapAngle {
			get {
				if (float.IsNaN(startCapAngle))
					startCapAngle = CalcCapAngle(ControlPointId.FirstVertex);
				return startCapAngle;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected float EndCapAngle {
			get {
				if (float.IsNaN(endCapAngle)) 
					endCapAngle = CalcCapAngle(ControlPointId.LastVertex);
				return endCapAngle;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Rectangle StartCapBounds {
			get {
				if (!Geometry.IsValid(startCapBounds)) {
					if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
						startCapBounds = ToolCache.GetCapBounds(StartCapStyleInternal, LineStyle, StartCapAngle);
					else startCapBounds = Rectangle.Empty;
					startCapBounds.Offset(vertices[GetControlPointIndex(ControlPointId.FirstVertex)]);
				}
				return startCapBounds;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Rectangle EndCapBounds {
			get {
				if (!Geometry.IsValid(endCapBounds)) {
					if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
						endCapBounds = ToolCache.GetCapBounds(EndCapStyleInternal, LineStyle, EndCapAngle);
					else endCapBounds = Rectangle.Empty;
					endCapBounds.Offset(vertices[GetControlPointIndex(ControlPointId.LastVertex)]);
				}
				return endCapBounds;
			}
		}

		#endregion


		#region [Protected] Methods (Inherited)

		/// <summary>
		/// Protetced internal constructur. Should only be called by the Type's CreateShapeDelegate
		/// </summary>
		protected internal LineShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			Construct();
		}



		/// <summary>
		/// Protetced internal constructur. Should only be called by the Type's CreateShapeDelegate
		/// </summary>
		protected internal LineShapeBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			Construct();
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			
			privateStartCapStyle = styleSet.CapStyles.None;
			privateEndCapStyle = styleSet.CapStyles.None;
			
			vertices[0] = Point.Empty;
			vertices[1] = new Point(20, 20);
		}


		/// <override></override>
		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdStartCapStyle:
					privateStartCapStyle = (propertyMapping.GetStyle() as ICapStyle);
					InvalidateDrawCache();
					Invalidate();
					break;
				case PropertyIdEndCapStyle:
					privateEndCapStyle = (propertyMapping.GetStyle() as ICapStyle);
					InvalidateDrawCache();
					Invalidate();
					break;
				default:
					base.ProcessExecModelPropertyChange(propertyMapping);
					break;
			}
		}


		/// <override></override>
		protected override bool RotateCore(int angle, int x, int y) {
			bool result = false;
			if (IsConnected(1, null) != ControlPointId.None || IsConnected(2, null) != ControlPointId.None)
				result = false;
			else {
				// Lists cannot be rotated , so copy points to an array.
				Point rotationCenter = Point.Empty;
				Point[] verts = vertices.ToArray();

				// Prepare transformation matrix and transform vertices
				rotationCenter.Offset(x, y);
				Matrix.Reset();
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(angle), rotationCenter);
				Matrix.TransformPoints(verts);

				// Copy rotated points back to the list of vertices
				for (int i = vertices.Count - 1; i >= 0; --i)
					vertices[i] = verts[i];

				InvalidateDrawCache();
				result = true;
			}
			return result;
		}


		/// <override></override>
		protected internal override int ControlPointCount {
			get { return vertices.Count; }
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// tight fitting and loose bounding rectangle are equal for lines
			Rectangle result = Rectangle.Empty;
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
				result = Geometry.UniteRectangles(result, StartCapBounds);
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
				result = Geometry.UniteRectangles(result, EndCapBounds);
			return result;
		}


		/// <override></override>
		protected override bool MoveByCore(int deltaX, int deltaY) {
			if (IsConnected(ControlPointId.FirstVertex, null) != ControlPointId.None
				|| IsConnected(ControlPointId.LastVertex, null) != ControlPointId.None)
				return false;

			base.MoveByCore(deltaX, deltaY);
			// Move vertices
			Point p = Point.Empty;
			for (int i = vertices.Count - 1; i >= 0; --i) {
				p = vertices[i];
				p.Offset(deltaX, deltaY);
				vertices[i] = p;
			}
			// Move CapBounds (if calculated)
			if (Geometry.IsValid(startCapBounds)) startCapBounds.Offset(deltaX, deltaY);
			if (Geometry.IsValid(endCapBounds)) endCapBounds.Offset(deltaX, deltaY);
			TransformDrawCache(deltaX, deltaY, 0, X, Y);
			return true;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			if (shapePoints.Length != VertexCount) { }
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
				if (StartCapContainsPoint(x, y)) return true;
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
				if (EndCapContainsPoint(x, y)) return true;
			return false;
		}


		/// <summary>
		/// Retrieve ControlPoint's index in the physical point array
		/// </summary>
		protected override int GetControlPointIndex(ControlPointId pointId) {
			if (pointId == ControlPointId.Reference) return -1;
			else if (IsFirstVertex(pointId)) return 0;
			else if (IsLastVertex(pointId)) return VertexCount - 1;
			else return pointIds.IndexOf(pointId);
		}


		/// <summary>
		/// Retrieve physical point's ControlPointId
		/// </summary>
		protected override ControlPointId GetControlPointId(int pointIdx) {
			if (pointIdx == 0)
				return ControlPointId.FirstVertex;
			else if (pointIdx == VertexCount - 1)
				return ControlPointId.LastVertex;
			else return pointIds[pointIdx];
		}


		/// <override></override>
		protected override bool IsConnectionPointEnabled(ControlPointId pointId) {
			return base.IsConnectionPointEnabled(pointId);
		}


		/// <override></override>
		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			// Do not delete shapePoints or cap buffers here for performance reasons
			startCapAngle = endCapAngle = float.NaN;
			startCapBounds = endCapBounds = Geometry.InvalidRectangle;
		}


		/// <override></override>
		protected override void UpdateDrawCache() {
			if (drawCacheIsInvalid) {
				Debug.Assert(shapePoints != null);
				// Calculate the line's shape points at the coordinate system's origin...
				RecalcDrawCache();
				// ... and transform to the current position
				TransformDrawCache(X, Y, 0, X, Y);
			}
		}


		/// <override></override>
		protected override void RecalcDrawCache() {
			int ptIdx;
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None) {
				ptIdx = GetControlPointIndex(ControlPointId.FirstVertex);
				// get untransfomed cap points and transform it to the start point (relative to origin of coordinates)
				ToolCache.GetCapPoints(StartCapStyleInternal, LineStyle, ref startCapPointsBuffer);
				TransformCapToOrigin(shapePoints[ptIdx].X, shapePoints[ptIdx].Y, StartCapAngle, ref startCapPointsBuffer);
			}
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None) {
				ptIdx = GetControlPointIndex(ControlPointId.LastVertex);
				// get untransfomed cap points and transform it to the start point (relative to origin of coordinates)
				ToolCache.GetCapPoints(EndCapStyleInternal, LineStyle, ref endCapPointsBuffer);
				TransformCapToOrigin(shapePoints[ptIdx].X, shapePoints[ptIdx].Y, EndCapAngle, ref endCapPointsBuffer);
			}
			drawCacheIsInvalid = false;
		}


		/// <summary>
		/// Transforms all objects that need to be transformed, such as Point-Arrays, GraphicsPaths or Brushes
		/// </summary>
		/// <param name="deltaX">Translation on X axis</param>
		/// <param name="deltaY">Translation on Y axis</param>
		/// <param name="deltaAngle">Rotation shapeAngle in tenths of degrees</param>
		/// <param name="rotationCenterX">X coordinate of the rotation center</param>
		/// <param name="rotationCenterY">Y coordinate of the rotation center</param>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			Matrix.Reset();
			if (!drawCacheIsInvalid) {
				if (deltaX != 0 || deltaY != 0 || deltaAngle != 0) {
					Matrix.Translate(deltaX, deltaY, MatrixOrder.Prepend);
					if (deltaAngle != 0) {
						PointF rotationCenter = PointF.Empty;
						rotationCenter.X = rotationCenterX;
						rotationCenter.Y = rotationCenterY;
						Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(deltaAngle), rotationCenter, MatrixOrder.Append);
					}
					if (shapePoints != null) Matrix.TransformPoints(shapePoints);
					if (startCapPointsBuffer != null) Matrix.TransformPoints(startCapPointsBuffer);
					if (endCapPointsBuffer != null) Matrix.TransformPoints(endCapPointsBuffer);
				}
			}
		}

		#endregion


		#region [Protected] Methods

		/// <ToBeCompleted></ToBeCompleted>
		protected bool IsFirstVertex(ControlPointId pointId) {
			return (pointId == ControlPointId.FirstVertex || pointIds.IndexOf(pointId) == 0);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected bool IsLastVertex(ControlPointId pointId) {
			return (pointId == ControlPointId.LastVertex || pointIds.IndexOf(pointId) == VertexCount - 1);
		}


		/// <summary>
		/// Performs an intersection test on the LineCap
		/// </summary>
		protected bool StartCapIntersectsWith(Rectangle rectangle) {
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None) {
				if (Geometry.RectangleIntersectsWithRectangle(startCapBounds, rectangle)) {
					if (Geometry.PolygonIntersectsWithRectangle(startCapPointsBuffer, rectangle))
						return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Performs an intersection test on the LineCap
		/// </summary>
		protected bool EndCapIntersectsWith(Rectangle rectangle) {
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None) {
				if (Geometry.RectangleIntersectsWithRectangle(endCapBounds, rectangle)) {
					if (Geometry.PolygonIntersectsWithRectangle(endCapPointsBuffer, rectangle))
						return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Performs a hit test on the LineCap
		/// </summary>
		protected bool StartCapContainsPoint(int pointX, int pointY) {
			if (Geometry.RectangleContainsPoint(startCapBounds, pointX, pointY)) {
				if (startCapPointsBuffer == null) 
					CalcCapPoints(GetControlPointIndex(ControlPointId.FirstVertex), StartCapAngle, StartCapStyleInternal, LineStyle, ref startCapBounds, ref startCapPointsBuffer);
				if (Geometry.ConvexPolygonContainsPoint(startCapPointsBuffer, pointX, pointY))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Performs a hit test on the LineCap
		/// </summary>
		protected bool EndCapContainsPoint(int pointX, int pointY) {
			if (Geometry.RectangleContainsPoint(endCapBounds, pointX, pointY)) {
				if (endCapPointsBuffer == null)
					CalcCapPoints(GetControlPointIndex(ControlPointId.LastVertex), endCapAngle, EndCapStyleInternal, LineStyle, ref endCapBounds, ref endCapPointsBuffer);
				if (Geometry.ConvexPolygonContainsPoint(endCapPointsBuffer, pointX, pointY))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Calculates the line cap angle for the given control point ControlPointId.LineStart or ControlPointId.LineEnd
		/// </summary>
		/// <returns>Line cap angle in degrees</returns>
		protected abstract float CalcCapAngle(ControlPointId pointId);


		/// <summary>
		/// Draws the line's StartCap
		/// </summary>
		protected void DrawStartCapBackground(Graphics graphics, int pointX, int pointY) {
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None) {
				Brush capBrush = ToolCache.GetBrush(StartCapStyleInternal.ColorStyle);
				// ToDo: Find a solution for round caps - perhaps transform the GraphicsPath itself?
				if (startCapPointsBuffer != null && startCapPointsBuffer.Length > 0)
					graphics.FillPolygon(capBrush, startCapPointsBuffer, System.Drawing.Drawing2D.FillMode.Alternate);
			}
		}


		/// <summary>
		/// Draws the line's EndCap
		/// </summary>
		protected void DrawEndCapBackground(Graphics graphics, int pointX, int pointY) {
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None) {
				Brush capBrush = ToolCache.GetBrush(EndCapStyleInternal.ColorStyle);
				// ToDo: Find a solution for round caps - perhaps transform the GraphicsPath itself?
				if (endCapPointsBuffer != null && endCapPointsBuffer.Length > 0)
					graphics.FillPolygon(capBrush, endCapPointsBuffer, System.Drawing.Drawing2D.FillMode.Alternate);
			}
		}


		/// <summary>
		/// Re-Calculates the position of a glue point when the control point next to the glue point moved.
		/// If the given glue point is not next to the moved point and/or if it is not connected via 
		/// Point-To-Shape connection, this method will do nothing. 
		/// Otherwise, the position of the given glue point will be re-calculated and the glue point will be moved.
		/// </summary>
		/// <param name="gluePointId">Glue point to be moved</param>
		/// <param name="movedPointId">The point id that</param>
		/// <returns></returns>
		protected bool MaintainGluePointPosition(ControlPointId gluePointId, ControlPointId movedPointId) {
			if ((GetNextVertexId(gluePointId) == movedPointId || GetPreviousVertexId(gluePointId) == movedPointId)
			&& HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue)) {
				// Recalc glue point if it is connected to an other shape via point-to-shape connection
				ShapeConnectionInfo sci = GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty && sci.OtherPointId == ControlPointId.Reference) {
					Point pos = CalcGluePoint(gluePointId, sci.OtherShape);
					if (Geometry.IsValid(pos)) {
						vertices[GetControlPointIndex(gluePointId)] = pos;
						return true;
					}
				}
			}
			return false;
		}

		#endregion


		#region [Private] Methods and Properties
		
		/// <summary>
		/// Specifies the tolerance when performing hit tests and intersection calculations
		/// </summary>
		private float ContainsPointDelta {
			get { return 2; }
		}

	
		private void Construct() {
			vertices = new List<Point>(MinVertexCount);
			pointIds = new List<ControlPointId>(MinVertexCount);
			for (int i = MinVertexCount - 1; i >= 0; --i) {
				vertices.Add(Point.Empty);
				pointIds.Insert(0, i + 1);
			}
			shapePoints = new Point[MinVertexCount];
			InvalidateDrawCache();
		}


		/// <summary>
		/// Calculate LineCap
		/// </summary>
		/// <param name="pointIndex"></param>
		/// <param name="capAngle"></param>
		/// <param name="capStyle"></param>
		/// <param name="lineStyle"></param>
		/// <param name="capBounds"></param>
		/// <param name="pointBuffer"></param>
		private void CalcCapPoints(int pointIndex, float capAngle, ICapStyle capStyle, ILineStyle lineStyle, ref Rectangle capBounds, ref PointF[] pointBuffer) {
			// get untransfomed shape points
			ToolCache.GetCapPoints(capStyle, lineStyle, ref pointBuffer);
			// translate, rotate and scale shapePoints
			TransformCapToOrigin(vertices[pointIndex].X, vertices[pointIndex].Y, capAngle, ref pointBuffer);
		}


		/// <summary>
		/// Transform CapPoints to the line end it belongs to, rotate it in the right direction and scale it according 
		/// to the LineStyle's line width (see note below)
		/// </summary>
		private void TransformCapToOrigin(int toX, int toY, float angleDeg, ref PointF[] capPoints) {
			Matrix.Reset();
			Matrix.Translate(toX, toY);
			Matrix.Rotate(angleDeg + 90);
			// Due to the fact that CustomCaps are automatically scaled with the LineWidth of a Pen
			// in GDI+, we scale down the the PointF arrays returned by the ToolCache in order to maintain
			// the cap size.
			// As a result of that, the CustomLineCap's PointF array have to be upscaled again for calculating
			// the bounds to invalidate and filling the interior
			Matrix.Scale(LineStyle.LineWidth, LineStyle.LineWidth);
			Matrix.TransformPoints(capPoints);
		}


		private IEnumerator<MenuItemDef> GetBaseMenuItemDefs(int mouseX, int mouseY, int range) {
			return base.GetMenuItemDefs(mouseX, mouseY, range).GetEnumerator();
		}
		
		#endregion


		#region ControlPointId Enumerator

		private struct Enumerator : IEnumerable<ControlPointId>, IEnumerator<ControlPointId>, IEnumerator {

			public static Enumerator Create(LineShapeBase shape, ControlPointCapabilities flags) {
				Debug.Assert(shape != null);
				Enumerator result;
				result.shape = shape;
				result.flags = flags;
				result.currentId = ControlPointId.FirstVertex;
				result.ctrlPointCnt = shape.ControlPointCount;
				return result;
			}


			#region IEnumerable<ControlPointId> Members

			public IEnumerator<ControlPointId> GetEnumerator() {
				return this;
			}

			#endregion


			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() {
				return this;
			}

			#endregion


			#region IEnumerator<ControlPointId> Members

			public bool MoveNext() {
				bool result = false;
				do {
					if (currentId == ControlPointId.None)
						return false;
					if (shape.HasControlPointCapability(currentId, flags))
						result = true;
					else currentId = shape.GetNextVertexId(currentId);
				} while (result == false);
				return result;
			}


			public void Reset() {
				ctrlPointCnt = shape.ControlPointCount;
				currentId = 1;
			}


			ControlPointId IEnumerator<ControlPointId>.Current {
				get {
					if (currentId == ControlPointId.None) throw new InvalidOperationException("ControlPointId.None is not a valid ControlPointId for iterating.");
					int result = currentId;
					currentId = shape.GetNextVertexId(currentId);
					return result;
				}
			}

			#endregion


			#region IEnumerator Members

			public object Current {
				get { return (IEnumerator<int>)this.Current; }
			}

			#endregion


			#region IDisposable Members

			public void Dispose() {
				this.flags = 0;
				this.currentId = ControlPointId.None;
				this.ctrlPointCnt = 0;
				this.shape = null;
			}

			#endregion


			#region Fields

			private LineShapeBase shape;
			private ControlPointCapabilities flags;
			private ControlPointId currentId;
			private int ctrlPointCnt;

			#endregion
		}

		#endregion
		
		
		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdStartCapStyle = 7;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdEndCapStyle = 8;
	
		private const string attrNameVertices = "Vertices";
		private static string[] pointAttrNames = new string[] { "PointIndex", "PointId", "X", "Y" };
		private static Type[] pointAttrTypes = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) };

		// List of PointIds for mapping PointIndex to PointIds
		/// <ToBeCompleted></ToBeCompleted>
		protected List<ControlPointId> pointIds;
		// List of Vertices (absolute coordinates)
		/// <ToBeCompleted></ToBeCompleted>
		protected List<Point> vertices;
		// Array of points used for drawing
		/// <ToBeCompleted></ToBeCompleted>
		protected Point[] shapePoints;

		// Styles
		private ICapStyle privateStartCapStyle = null;
		private ICapStyle privateEndCapStyle = null;

		// drawing stuff
		private float startCapAngle = float.NaN;
		private float endCapAngle = float.NaN;
		private Rectangle startCapBounds = Rectangle.Empty;		// Rectangle for invalidating the line caps
		private Rectangle endCapBounds = Rectangle.Empty;		// Rectangle for invalidating the line caps
		private PointF[] startCapPointsBuffer = null;				// buffer for the startCap - used for drawing and hit- / intersection testing
		private PointF[] endCapPointsBuffer = null;				// buffer for the startCap - used for drawing and hit- / intersection testing
		private Matrix matrix = new Matrix();

		#endregion
	}


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
			bottomRight.Offset(x+width, y+height);
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
				float segmentLength = Geometry.DistancePointPoint(vertices[idx], vertices[idx+1]);
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
				result.C = (int)Math.Round((distanceFromA / (segmentLength / 100)) * 10);
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
						}
						else if (verticalIntersection) {
							p.X += stepX;
							if (p != endCell) {
								if (!cellProcessed(p)) 
									yield return p;
							} else endCellReached = true;
						} 
						else if (horizontalIntersection) {
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
				while (i != endIdx){
					int j = i + step;
					PointF p = Geometry.IntersectCircleWithLine(vertices[capPtIdx].X, vertices[capPtIdx].Y, capInset, vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, true);
					if (Geometry.IsValid(p) && Geometry.LineContainsPoint(vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, false, p.X, p.Y))
						result = Geometry.RadiansToDegrees(Geometry.Angle(vertices[capPtIdx].X, vertices[capPtIdx].Y, p.X, p.Y));
					i = i + step;
				}
			}
			if (float.IsNaN(result))
				result= Geometry.RadiansToDegrees(Geometry.Angle(vertices[capPtIdx].X, vertices[capPtIdx].Y, vertices[otherPtIdx].X, vertices[otherPtIdx].Y));
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


	/// <summary>
	/// Abstract base class for circular arcs.
	/// </summary>
	/// <remarks>RequiredPermissions set</remarks>
	public abstract class CircularArcBase : LineShapeBase {

		#region Shape Members

		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is CircularArcBase) {
				// Vertices and CapStyles will be copied by the base class
				// so there's nothing left to do here...
			}
		}


		/// <override></override>
		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			// The relative Position of an arc is defined by...
			// A: Tenths of percentage of the distance between StartPoint and EndPoint
			// B: not used
			Point result = Geometry.InvalidPoint;
			if (IsLine) result = Geometry.VectorLinearInterpolation(StartPoint, EndPoint, relativePosition.A / 1000f);
			else {
				RecalculateArc();

				// Calculate absolute sweep angle from relative position (percentate of sweep angle)
				float angleToPt = SweepAngle * (relativePosition.A / 1000f);

				// Calculate absolute position on the arc
				float radius = Radius + (relativePosition.B / 10f);
				float x = Center.X + radius;
				float y = Center.Y;
				Geometry.RotatePoint(Center.X, Center.Y, StartAngle + angleToPt, ref x, ref y);
				result = Point.Empty;
				result.Offset((int)Math.Round(x), (int)Math.Round(y));
			}
			if (!Geometry.IsValid(result)) System.Diagnostics.Debug.Fail("Unable to calculate glue point position!");
			return result;
		}


		/// <override></override>
		public override RelativePosition CalculateRelativePosition(int x, int y) {
			// The relative Position of an arc is defined by...
			// A: Tenths of percentage of the distance between StartPoint and EndPoint
			// B: Distance from arc's outline
			RelativePosition result = RelativePosition.Empty;
			Point p = Point.Empty;
			p.Offset(x, y);
			if (IsLine) {
				float length = Geometry.DistancePointPoint(StartPoint, EndPoint);
				if (length == 0) {
					result.A = result.B = result.C = 0;
				} else {
					float distToPt = Geometry.DistancePointPoint(StartPoint, p);
					result.A = (int)Math.Round((distToPt / (length / 100)) * 10);
					result.B = (int)Math.Round(Geometry.DistancePointLineSegment2(p, StartPoint, EndPoint) * 10);
				}
			} else {
				RecalculateArc();

				// Calculate absolute angle to the given point
				float angleToPt = Geometry.RadiansToDegrees(Geometry.Angle(Center, p));
				if (angleToPt < 0) angleToPt += 360;
				if (angleToPt < StartAngle && StartAngle + SweepAngle > 360)
					angleToPt += 360;

				// Calculate sweep angle to the given point relative to the (absolute and positive) start angle
				float resultAngle = angleToPt - ((360 + StartAngle) % 360);
				// Encode angle to the given point as percentage of the current sweepangle 
				// (which is *always* from start point to end point)
				result.A = (int)Math.Round((resultAngle / (SweepAngle / 100f)) * 10);
				result.B = (int)Math.Round((Geometry.DistancePointPoint(Center, p) - Radius) * 10);
				if (result.A < 0 || result.A > 1000) {
					Debug.Print("Calculated relative position is {0}%! StartAngle = {1}°, SweepAngle = {2}°, Angle to absolute point = {3}°",
						result.A / 10f, StartAngle, SweepAngle, angleToPt);
				}
			}
			return result;
		}


		/// <override></override>
		public override Point CalcNormalVector(Point point) {
			Point result = Point.Empty;
			if (IsLine) {
				float lineAngle = Geometry.RadiansToDegrees(Geometry.Angle(StartPoint, EndPoint));
				int x = point.X + 100;
				int y = point.Y;
				Geometry.RotatePoint(point.X, point.Y, lineAngle + 90, ref x, ref y);
				result.X = x;
				result.Y = y;
			} else {
				if (drawCacheIsInvalid) UpdateDrawCache();
				float angleDeg = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, point.X, point.Y));
				result = Geometry.CalcPoint(point.X, point.Y, angleDeg, 100);
			}
			return result;
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int fromX, int fromY) {
			Point result = Geometry.InvalidPoint;
			// Calculate intersection point(s)
			PointF p = Geometry.GetNearestPoint(fromX, fromY,
				Geometry.IntersectArcLine(
					StartPoint.X, StartPoint.Y,
					RadiusPoint.X, RadiusPoint.Y,
					EndPoint.X, EndPoint.Y,
					Center.X, Center.Y,
					fromX, fromY, false));
			if (Geometry.IsValid(p)) {
				result.X = (int)Math.Round(p.X);
				result.Y = (int)Math.Round(p.Y);
			} else result = Geometry.GetNearestPoint(fromX, fromY, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y);
			return result;
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int x1, int y1, int x2, int y2) {
			Point result = Point.Empty;
			Point linePt = Geometry.GetFurthestPoint((int)Math.Round(Center.X), (int)Math.Round(Center.Y), x1, y1, x2, y2);
			Point intersectionPt = Geometry.GetNearestPoint(linePt, Geometry.IntersectArcLine(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, x1, y1, x2, y2, false));
			if (Geometry.IsValid(intersectionPt))
				return intersectionPt;
			else
				return Geometry.GetNearestPoint(linePt, Geometry.CalcArcTangentThroughPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, linePt.X, linePt.Y));
		}


		/// <override></override>
		protected internal override IEnumerable<Point> CalculateCells(int cellSize) {
			// Calculate cells occupied by children
			if (ChildrenCollection != null) {
				foreach (Shape shape in ChildrenCollection.BottomUp)
					foreach (Point cell in shape.CalculateCells(cellSize))
						yield return cell;
			}

			// The outer bounding rectangle (including the control points) is required here
			Rectangle bounds = CalculateBoundingRectangle(false);
			// This not 100% correct as cell 0 will be occupied by objects at 10/10 
			// as well as objects at -10/-10, 10/-10 and -10/10. 
			// On the other hand, integer division is >20 times faster than floored float divisions
			// and for this simple "bounding rectangle" approach, it works ok.
			int leftIdx = bounds.Left / cellSize;
			int topIdx = bounds.Top / cellSize;
			int rightIdx = bounds.Right / cellSize;
			int bottomIdx = bounds.Bottom / cellSize;
			Point p = Point.Empty;
			Rectangle cellBounds = Rectangle.Empty;
			cellBounds.Width = cellBounds.Height = cellSize;
			for (p.X = leftIdx; p.X <= rightIdx; p.X += 1) {
				for (p.Y = topIdx; p.Y <= bottomIdx; p.Y += 1) {
					cellBounds.X = p.X * cellSize;
					cellBounds.Y = p.Y * cellSize;
					if (Geometry.ArcIntersectsWithRectangle(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, cellBounds))
						yield return p;
				}
			}
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(int mouseX, int mouseY, int range) {
			// Return actions of base class. Use private method to avoid compiler warning
			IEnumerator<MenuItemDef> enumerator = GetBaseActions(mouseX, mouseY, range);
			while (enumerator.MoveNext()) yield return enumerator.Current;
			// return own actions
			ControlPointId clickedPointId = FindNearestControlPoint(mouseX, mouseY, range, ControlPointCapabilities.Resize);

			bool isFeasible;
			string description;

			isFeasible = (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference) && ContainsPoint(mouseX, mouseY) && VertexCount < 3;
			description = "You have to click on the line in order to insert new points";
			yield return new CommandMenuItemDef("Insert Point", null, description, isFeasible,
				new AddVertexCommand(this, mouseX, mouseY));

			isFeasible = !HasControlPointCapability(clickedPointId, ControlPointCapabilities.Glue) && VertexCount > 2;
			if (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference)
				description = "No control point was clicked";
			else description = "Glue control points may not be removed.";
			yield return new CommandMenuItemDef("Remove Point", null, description, isFeasible,
				new RemoveVertexCommand(this, range));
		}


		/// <override></override>
		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			if (IsLine) {
				if (Geometry.DistancePointLine(x, y, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, true) <= range) {
					if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability)
							&& !(Geometry.DistancePointPoint(x, y, vertices[0].X, vertices[0].Y) <= range)
							&& !(Geometry.DistancePointPoint(x, y, vertices[1].X, vertices[1].Y) <= range)
							&& IsConnectionPointEnabled(ControlPointId.Reference))
								return ControlPointId.Reference;
				}
			} else {
				float lineContainsDelta = (LineStyle.LineWidth / 2f) + 2;
				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, x, y, lineContainsDelta)) {
					if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability)
						&& !(Geometry.DistancePointPoint(x, y, vertices[0].X, vertices[0].Y) <= range)
						&& !(Geometry.DistancePointPoint(x, y, vertices[2].X, vertices[2].Y) <= range)
						&& IsConnectionPointEnabled(ControlPointId.Reference))
							return ControlPointId.Reference;
				}
			}
			for (int i = 0; i < VertexCount; ++i) {
				if (Geometry.DistancePointPoint(x, y, vertices[i].X, vertices[i].Y) <= range) {
					ControlPointId ptId = GetControlPointId(i);
					if (HasControlPointCapability(ptId, controlPointCapability)
						&& IsConnectionPointEnabled(ptId)) return ptId;
				}
			}
			return ControlPointId.None;
		}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			if (IsLine) {
				MoveControlPointTo(ControlPointId.FirstVertex, x, y, ResizeModifiers.None);
				MoveControlPointTo(ControlPointId.LastVertex, x + width, y + height, ResizeModifiers.None);
			} else {
				float radius = Math.Min(width / 2f, height / 2f);
				PointF c = PointF.Empty;
				c.X = x + (width / 2f);
				c.Y = y + (height / 2f);

				// Calculate new start point
				PointF s = PointF.Empty;
				s.X = c.X + radius;
				s.Y = c.Y;
				s = Geometry.RotatePoint(c, StartAngle, s);
				// Calculate new end point
				PointF e = PointF.Empty;
				e = Geometry.RotatePoint(c, StartAngle + SweepAngle, s);
				// calculate new radius point
				PointF r = PointF.Empty;
				r = Geometry.RotatePoint(c, Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, RadiusPoint.X, RadiusPoint.Y)), s);
				foreach (ControlPointId id in GetControlPointIds(ControlPointCapabilities.Resize)) {
					if (IsFirstVertex(id)) MoveControlPointTo(id, (int)Math.Round(s.X), (int)Math.Round(s.Y), ResizeModifiers.None);
					else if (IsLastVertex(id)) MoveControlPointTo(id, (int)Math.Round(e.X), (int)Math.Round(e.Y), ResizeModifiers.None);
					else MoveControlPointTo(id, (int)Math.Round(r.X), (int)Math.Round(r.Y), ResizeModifiers.None);
				}
			}
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
		public override int MinVertexCount { get { return 2; } }


		/// <override></override>
		public override int MaxVertexCount { get { return 3; } }


		/// <override></override>
		public override ControlPointId InsertVertex(ControlPointId beforePointId, int x, int y) {
			int newPointId = ControlPointId.None;
			if (IsFirstVertex(beforePointId) || beforePointId == ControlPointId.Reference || beforePointId == ControlPointId.None)
				throw new NShapeException("{0} is not a valid {1} for this operation.", beforePointId, typeof(ControlPointId).Name);
			if (VertexCount >= MaxVertexCount) throw new NShapeException("Number of maximum vertices reached.");

			Point p = Point.Empty;
			p.Offset(x, y);
			if (vertices.Count < MaxVertexCount) {
				int pointIndex = GetControlPointIndex(beforePointId);
				if (pointIndex < 0 || pointIndex > VertexCount || pointIndex > MaxVertexCount)
					throw new IndexOutOfRangeException();
				// insert Radius Point
				vertices.Insert(pointIndex, p);
				// create a new PointId
				newPointId = 3;
				pointIds.Insert(pointIndex, newPointId);
			} else {
				int pointIndex = GetControlPointIndex(GetPreviousVertexId(beforePointId));
				if (pointIndex < 0 || pointIndex > VertexCount || pointIndex > MaxVertexCount)
					throw new IndexOutOfRangeException();
				// replace Radius Point
				vertices[pointIndex] = p;
				newPointId = 3;
			}
			InvalidateDrawCache();

			return newPointId;
		}


		/// <override></override>
		public override ControlPointId AddVertex(int x, int y) {
			if (VertexCount >= MaxVertexCount) throw new InvalidOperationException("Number of maximum vertices reached.");
			return InsertVertex(ControlPointId.LastVertex, x, y);
		}


		/// <override></override>
		public override void RemoveVertex(ControlPointId controlPointId) {
			if (IsFirstVertex(controlPointId) || IsLastVertex(controlPointId))
				throw new InvalidOperationException("Start- and end pioints of linear shapes cannot be removed.");
			int controlPointIndex = GetControlPointIndex(controlPointId);
			vertices.RemoveAt(controlPointIndex);
			InvalidateDrawCache();
		}


		/// <override></override>
		public override void Invalidate() {
			if (DisplayService != null) {
				base.Invalidate();

				int margin = 1;
				if (LineStyle != null) margin = (int)Math.Ceiling(LineStyle.LineWidth / 2f) + 1;
				//DisplayService.Invalidate((int)Math.Floor(arcBounds.X - margin), (int)Math.Floor(arcBounds.Y - margin), (int) Math.Ceiling(arcBounds.Width + margin + margin), (int)Math.Ceiling(arcBounds.Height + margin + margin));

				int left, right, top, bottom;
				left = Math.Min(StartPoint.X, EndPoint.X);
				right = Math.Max(StartPoint.X, EndPoint.X);
				top = Math.Min(StartPoint.Y, EndPoint.Y);
				bottom = Math.Max(StartPoint.Y, EndPoint.Y);
				if (VertexCount > 2) {
					if (arcIsInvalid) RecalculateArc();
					if (!arcIsInvalid) {
						if (arcBounds.Left < left)
							left = (int)Math.Floor(arcBounds.Left);
						if (arcBounds.Top < top)
							top = (int)Math.Floor(arcBounds.Top);
						if (arcBounds.Right > right)
							right = (int)Math.Ceiling(arcBounds.Right);
						if (arcBounds.Bottom > bottom)
							bottom = (int)Math.Ceiling(arcBounds.Bottom);
					}
				}
				DisplayService.Invalidate(left - margin, top - margin, right - left + margin + margin, bottom - top + margin + margin);
			}
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			UpdateDrawCache();
			if (VertexCount > 1) {
				// draw caps interior
				int startPtIdx = GetControlPointIndex(ControlPointId.FirstVertex);
				DrawStartCapBackground(graphics, shapePoints[startPtIdx].X, shapePoints[startPtIdx].Y);
				int endPtIdx = GetControlPointIndex(ControlPointId.LastVertex);
				DrawEndCapBackground(graphics, shapePoints[endPtIdx].X, shapePoints[endPtIdx].Y);

				// draw line
				Pen pen = ToolCache.GetPen(LineStyle, StartCapStyleInternal, EndCapStyleInternal);
				DrawOutline(graphics, pen);
#if DEBUG

				#region Visualize arc definition
				
				// Draw bounding rectangle / line cap bounds
				//graphics.DrawRectangle(Pens.Red, GetBoundingRectangle(true));
				//if (StartCapBounds != Geometry.InvalidRectangle)
				//   graphics.DrawRectangle(Pens.Red, StartCapBounds);
				//if (EndCapBounds != Geometry.InvalidRectangle)
				//   graphics.DrawRectangle(Pens.Red, EndCapBounds);


				//if (!IsLine) {
				//   // Draw start- and sweep angle of the arc
				//   using (Brush sweepAngBrush = new SolidBrush(Color.FromArgb(128, Color.Blue)))
				//      GdiHelpers.DrawAngle(graphics, sweepAngBrush, Center, arcStartAngle, arcSweepAngle, (int)Math.Round(3 * (Radius / 4)));
				//   using (Brush startAngBrush = new SolidBrush(Color.FromArgb(128, Color.Navy)))
				//      GdiHelpers.DrawAngle(graphics, startAngBrush, Center, arcStartAngle, (int)Math.Round(Radius / 2));

				//   // Draw the arc's vertices
				//   GdiHelpers.DrawPoint(graphics, Pens.Lime, StartPoint.X, StartPoint.Y, 10);
				//   GdiHelpers.DrawPoint(graphics, Pens.Blue, RadiusPoint.X, RadiusPoint.Y, 10);
				//   GdiHelpers.DrawPoint(graphics, Pens.Green, EndPoint.X, EndPoint.Y, 10);

				//   // Draw the arc's shapePoints
				//   for (int i = 0; i < shapePoints.Length; ++i)
				//      GdiHelpers.DrawPoint(graphics, Pens.Red, shapePoints[i].X, shapePoints[i].Y, 3);
				//   GdiHelpers.DrawPoint(graphics, Pens.Red, Center.X, Center.Y, 3);
				//   graphics.DrawRectangle(Pens.Red, arcBounds.X, arcBounds.Y, arcBounds.Width, arcBounds.Height);
				//}

				#endregion

				#region Visualize absolute/relative positions

				// Draw relative positions from 0% - 100%
				//RelativePosition relPos = RelativePosition.Empty;
				//Point p = Point.Empty;
				//int size = 10;
				//relPos.B = 0;
				//relPos.A = 0;
				//int cnt = 10;
				//for (int i = 0; i <= cnt; ++i) {
				//   relPos.A = i * (1000 / cnt);
				//   p = CalculateAbsolutePosition(relPos);
				//   GdiHelpers.DrawPoint(graphics, Pens.Green, p.X, p.Y, size);
				//   relPos = CalculateRelativePosition(p.X, p.Y);
				//   GdiHelpers.DrawPoint(graphics, Pens.Red, p.X, p.Y, size);
				//}

				// Draw angles to connected glue points
				//if (!IsLine) {
				//   foreach (ShapeConnectionInfo ci in GetConnectionInfos(ControlPointId.Reference, null)) {
				//      Point pt = ci.OtherShape.GetControlPointPosition(ci.OtherPointId);
				//      float angleDeg = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, StartPoint.X, StartPoint.Y, pt.X, pt.Y));
				//      if (angleDeg < 0) angleDeg += 360;
				//      GdiHelpers.DrawAngle(graphics, Brushes.Red, Center, arcStartAngle + angleDeg, (int)(Radius / 2));
				//      GdiHelpers.DrawPoint(graphics, Pens.Blue, pt.X, pt.Y, 3);
				//   }
				//}

				//if (!IsLine) {
				//   // Draw angle to point 0 and shapeAngle to point 2
				//   float arcRadius;
				//   PointF arcCenter = Geometry.CalcArcCenterAndRadius((float)StartPoint.X, (float)StartPoint.Y, (float)RadiusPoint.X, (float)RadiusPoint.Y, (float)EndPoint.X, (float)EndPoint.Y, out arcRadius);

				//   SolidBrush startAngleBrush = new SolidBrush(Color.FromArgb(96, Color.Red));
				//   SolidBrush sweepAngleBrush = new SolidBrush(Color.FromArgb(96, Color.Blue));
				//   SolidBrush ptAngleBrush = new SolidBrush(Color.FromArgb(96, Color.Green));

				//   // Draw StartAngle
				//   float angleToStartPt = (360 + Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, StartPoint.X, StartPoint.Y))) % 360;
				//   GdiHelpers.DrawAngle(graphics, startAngleBrush, arcCenter, angleToStartPt, (int)(Radius / 2));

				//   // Draw relative Position of connected shape
				//   foreach (ShapeConnectionInfo ci in GetConnectionInfos(ControlPointId.Reference, null)) {
				//      if (ci.OwnPointId != ControlPointId.Reference) continue;
				//      Point p = ci.OtherShape.GetControlPointPosition(ci.OtherPointId);

				//      RelativePosition relativePosition = CalculateRelativePosition(p.X, p.Y);
				//      float arcLength = Radius * Geometry.DegreesToRadians(SweepAngle);
				//      float resAngleToPt = Geometry.RadiansToDegrees((arcLength * relativePosition.A / 1000f) / Radius);

				//      // Draw relative position
				//      GdiHelpers.DrawAngle(graphics, sweepAngleBrush, Center, angleToStartPt, resAngleToPt, (int)(Radius - (Radius / 4)));

				//      // Draw absolute position
				//      Point absPtPos = CalculateAbsolutePosition(relativePosition);
				//      GdiHelpers.DrawPoint(graphics, Pens.Red, absPtPos.X, absPtPos.Y, 3);
				//      break;
				//   }

				//   startAngleBrush.Dispose();
				//   sweepAngleBrush.Dispose();
				//}

				//if (!IsLine) {
				//   // Draw angle to point 0 and shapeAngle to point 2
				//   float arcRadius;
				//   PointF arcCenter = Geometry.CalcArcCenterAndRadius((float)StartPoint.X, (float)StartPoint.Y, (float)RadiusPoint.X, (float)RadiusPoint.Y, (float)EndPoint.X, (float)EndPoint.Y, out arcRadius);

				//   GdiHelpers.DrawPoint(graphics, Pens.Red, vertices[0].X, vertices[0].Y, 4);
				//   GdiHelpers.DrawPoint(graphics, Pens.Purple, vertices[1].X, vertices[1].Y, 4);
				//   GdiHelpers.DrawPoint(graphics, Pens.Blue, vertices[2].X, vertices[2].Y, 4);

				//   SolidBrush startAngleBrush = new SolidBrush(Color.FromArgb(96, Color.Red));
				//   SolidBrush endAngleBrush = new SolidBrush(Color.FromArgb(96, Color.Blue));

				//   float startAngle = Geometry.RadiansToDegrees(Geometry.Angle(arcCenter.X, arcCenter.Y, vertices[0].X, vertices[0].Y));
				//   float endAngle = Geometry.RadiansToDegrees(Geometry.Angle(arcCenter.X, arcCenter.Y, vertices[2].X, vertices[2].Y));
				//   if (!float.IsNaN(startAngle))
				//      GdiHelpers.DrawAngle(graphics, startAngleBrush, Center, startAngle, (int)(Radius / 2));
				//   if (!float.IsNaN(endAngle))
				//      GdiHelpers.DrawAngle(graphics, endAngleBrush, Center, endAngle, (int)(Radius / 2));

				//   startAngleBrush.Dispose();
				//   endAngleBrush.Dispose();
				//}

				#endregion

				#region Visualize dynamic connection point calculation

				//ControlPointId gluePointId = ControlPointId.LastVertex;
				//if (!IsLine && IsConnected(gluePointId, null) != ControlPointId.None) {
				//   ShapeConnectionInfo ci = GetConnectionInfo(gluePointId, null);
				//   Shape shape = ci.OtherShape;
				//   Point ptPos = Geometry.InvalidPoint;
				//   // Calculate desired arc: 
				//   // Start point, end point, center and radius
				//   ShapeConnectionInfo startCi = GetConnectionInfo(ControlPointId.FirstVertex, null);
				//   ShapeConnectionInfo endCi = GetConnectionInfo(ControlPointId.LastVertex, null);
				//   Point startPt = Point.Empty, endPt = Point.Empty;
				//   if (startCi.IsEmpty) startPt.Offset(StartPoint.X, StartPoint.Y);
				//   else startPt.Offset(startCi.OtherShape.X, startCi.OtherShape.Y);
				//   if (endCi.IsEmpty) endPt.Offset(EndPoint.X, EndPoint.Y);
				//   else endPt.Offset(endCi.OtherShape.X, endCi.OtherShape.Y);
				//   float r;
				//   PointF centerPt = Geometry.CalcArcCenterAndRadius(startPt, RadiusPoint, endPt, out r);
				//   //
				//   // Draw the base circle of the new arc
				//   Pen tmpPen = Pens.Yellow;
				//   GdiHelpers.DrawLine(graphics, tmpPen, centerPt, startPt);
				//   GdiHelpers.DrawLine(graphics, tmpPen, centerPt, endPt);
				//   GdiHelpers.DrawPoint(graphics, tmpPen, centerPt.X, centerPt.Y, 3);
				//   graphics.DrawEllipse(tmpPen, centerPt.X - radius, centerPt.Y - radius, radius + radius, radius + radius);

				//   //
				//   // Calculate tangent on the desired arc through the other shape's center
				//   Point tangentPt = IsFirstVertex(gluePointId) ? startPt : endPt;
				//   float a, b, c;
				//   Geometry.CalcPerpendicularLine(centerPt.X, centerPt.Y, tangentPt.X, tangentPt.Y, out a, out b, out c);
				//   int aT, bT, cT;
				//   Geometry.TranslateLine((int)a, (int)b, (int)c, tangentPt, out aT, out bT, out cT);
				//   // Draw Tangent
				//   tmpPen = Pens.Orange;
				//   GdiHelpers.DrawLine(graphics, tmpPen, aT, bT, cT);
				//   GdiHelpers.DrawPoint(graphics, tmpPen, tangentPt.X, tangentPt.Y, 3);

				//   //
				//   // Calculate intersection point of the calculated tangent and the perpendicular bisector 
				//   // of the line through startPt and endPt
				//   Geometry.CalcPerpendicularBisector(startPt.X, startPt.Y, endPt.X, endPt.Y, out a, out b, out c);
				//   PointF pT = Geometry.IntersectLines(aT, bT, cT, a, b, c);
				//   //
				//   // Draw the calculated intersection point and the perpendicular bisector
				//   tmpPen = Pens.OrangeRed;
				//   GdiHelpers.DrawLine(graphics, tmpPen, a, b, c);
				//   GdiHelpers.DrawPoint(graphics, tmpPen, (endPt.X - ((endPt.X - startPt.X) / 2)), (endPt.Y - ((endPt.Y - startPt.Y) / 2)), 3);

				//   if (pT != Geometry.InvalidPointF) {
				//      PointF pB = Geometry.VectorLinearInterpolation(startPt, endPt, 0.5);
				//      ptPos = Point.Round(Geometry.VectorLinearInterpolation(pB, pT, 0.75));

				//      graphics.DrawLine(Pens.DarkRed, ptPos, centerPt);
				//      // Check if the calculated point is on the right side
				//      bool chk = Geometry.ArcIntersectsWithLine(startPt.X, startPt.Y, RadiusPoint.X, RadiusPoint.Y, endPt.X, endPt.Y, ptPos.X, ptPos.Y, centerPt.X, centerPt.Y, true);

				//      bool intersectswith = false;
				//      List<PointF> chkPts = new List<PointF>(Geometry.IntersectArcLine(startPt, RadiusPoint, endPt, ptPos, centerPt, true));
				//      for (int i = chkPts.Count - 1; i >= 0; --i) {
				//         if (chkPts[i] != Geometry.InvalidPointF) {
				//            intersectswith = true;
				//            break;
				//         }
				//      }
				//      if (!intersectswith)
				//         ptPos = Geometry.VectorLinearInterpolation(ptPos, tangentPt, 2);

				//      //
				//      // Draw the calculated point
				//      GdiHelpers.DrawPoint(graphics, Pens.Red, ptPos.X, ptPos.Y, 3);
				//   }
				//   // If the arc only has 2 points or something went wrong while calculating the desired arc
				//   if (ptPos == Geometry.InvalidPoint)
				//      ptPos = Geometry.VectorLinearInterpolation(StartPoint, EndPoint, 0.5d);
				//   Point result = CalcGluePointFromPosition(gluePointId, shape, ptPos.X, ptPos.Y);
				//   //
				//   // Draw resulting intersection point
				//   GdiHelpers.DrawPoint(graphics, Pens.Lime, result.X, result.Y, 3);
				//}

		#endregion

				#region Visualize simultaneous dynamic calculation of two connected glue points

				//// Draw Calculating both ends simultaneously
				//if (!IsLine
				//   && IsConnected(ControlPointId.FirstVertex, null) == ControlPointId.Reference
				//   && IsConnected(ControlPointId.LastVertex, null) == ControlPointId.Reference) {
				//   // Get partner shapes and current glue point positions
				//   Shape shapeA = GetConnectionInfo(ControlPointId.FirstVertex, null).OtherShape;
				//   Shape shapeB = GetConnectionInfo(ControlPointId.LastVertex, null).OtherShape;
				//   Point currGluePtAPos = GetControlPointPosition(ControlPointId.FirstVertex);
				//   Point currGluePtBPos = GetControlPointPosition(ControlPointId.LastVertex);

				//   float sPtAngle = Geometry.RadiansToDegrees(Geometry.Angle(shapeA.X, shapeA.Y, RadiusPoint.X, RadiusPoint.Y));
				//   float ePtAngle = Geometry.RadiansToDegrees(Geometry.Angle(shapeB.X, shapeB.Y, RadiusPoint.X, RadiusPoint.Y));
				//   float dist = Geometry.DistancePointPoint(shapeA.X, shapeA.Y, shapeB.X, shapeB.Y) * 2;
				//   Point tmpPtS = Geometry.CalcPoint(shapeA.X, shapeA.Y, sPtAngle, dist);
				//   Point tmpPtE = Geometry.CalcPoint(shapeB.X, shapeB.Y, ePtAngle, dist);
				//   Point newRadiusPtPos = Geometry.IntersectLines(
				//      shapeA.X, shapeA.Y, tmpPtS.X, tmpPtS.Y,
				//      shapeB.X, shapeB.Y, tmpPtE.X, tmpPtE.Y);
				//   if (newRadiusPtPos == Geometry.InvalidPoint)
				//      newRadiusPtPos = Geometry.VectorLinearInterpolation(shapeA.X, shapeA.Y, shapeB.X, shapeB.Y, 0.5);

				//   // Calculate a common base point for connection foot calculation
				//   Point calcBasePtA = CalcGluePointCalculationBase(ControlPointId.FirstVertex, shapeA);
				//   Point calcBasePtB = CalcGluePointCalculationBase(ControlPointId.LastVertex, shapeB);
				//   Point calcBasePt = Geometry.VectorLinearInterpolation(calcBasePtA, calcBasePtB, 0.5);
				//   // Calc new glue point positions from the common calculation base
				//   Point newGluePtAPos = CalcGluePointFromPosition(ControlPointId.FirstVertex, shapeA, calcBasePt.X, calcBasePt.Y);
				//   Point newGluePtBPos = CalcGluePointFromPosition(ControlPointId.LastVertex, shapeB, calcBasePt.X, calcBasePt.Y);
				//   // Move both glue points to their final destination

				//   Pen tmpPen;
				//   tmpPen = Pens.Red;
				//   GdiHelpers.DrawLine(graphics, tmpPen, StartPoint, tmpPtS);
				//   tmpPen = Pens.Blue;
				//   GdiHelpers.DrawLine(graphics, tmpPen, EndPoint, tmpPtE);
				//   tmpPen = Pens.Lime;
				//   GdiHelpers.DrawPoint(graphics, tmpPen, newRadiusPtPos, 3);
				//   GdiHelpers.DrawPoint(graphics, tmpPen, newGluePtAPos, 3);
				//   GdiHelpers.DrawPoint(graphics, tmpPen, newGluePtBPos, 3);
				//}

				#endregion

#endif
			}
			base.Draw(graphics);
		}


		/// <override></override>
		public override void DrawOutline(Graphics graphics, Pen pen) {
			base.DrawOutline(graphics, pen);
			if (IsLine) graphics.DrawLine(pen, StartPoint, EndPoint);
			else {
				Debug.Assert(Geometry.IsValid(arcBounds));
				Debug.Assert(!float.IsNaN(StartAngle));
				Debug.Assert(!float.IsNaN(SweepAngle));
				graphics.DrawArc(pen, arcBounds, StartAngle, SweepAngle);
			}
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
				float scale = Math.Max((float)image.Width / width, (float)image.Height / height);
				g.ScaleTransform(scale, scale);

				RectangleF r = RectangleF.Empty;
				float m = Math.Max(margin, Math.Max(startCapSize, endCapSize));
				r.X = m;
				r.Y = m;
				r.Width = width - m - m;
				r.Height = height - m - m;

				Pen pen = ToolCache.GetPen(LineStyle, StartCapStyleInternal, EndCapStyleInternal);
				g.DrawArc(pen, r, 135, 270);
			}
		}

		#endregion


		#region [Protected] Methods

		/// <ToBeCompleted></ToBeCompleted>
		protected internal CircularArcBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal CircularArcBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			for (int i = vertices.Count - 1; i >= 0; --i)
				vertices[i] = Point.Empty;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// Calcualte line caps' bounds
			Rectangle capBounds = base.CalculateBoundingRectangle(tight);
			Rectangle result = Rectangle.Empty;
			float halfLineWidth = LineStyle.LineWidth / 2f;
			float delta = halfLineWidth + 0.2f;
			if (IsLine) {
				result.X = (int)Math.Floor(Math.Min(StartPoint.X, EndPoint.X) - halfLineWidth);
				result.Y = (int)Math.Floor(Math.Min(StartPoint.Y, EndPoint.Y) - halfLineWidth);
				result.Width = (int)Math.Ceiling(Math.Max(StartPoint.X, EndPoint.X) + halfLineWidth) - result.X;
				result.Height = (int)Math.Ceiling(Math.Max(StartPoint.Y, EndPoint.Y) + halfLineWidth) - result.Y;
			} else {
				if (arcIsInvalid) RecalculateArc();

				float left = Center.X - Radius - halfLineWidth;
				float top = Center.Y - Radius - halfLineWidth;
				float right = Center.X + Radius + halfLineWidth;
				float bottom = Center.Y + Radius + halfLineWidth;

				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, left, Center.Y, delta)) result.X = (int)Math.Floor(left);
				else result.X = (int)Math.Floor(Math.Min(StartPoint.X, EndPoint.X) - halfLineWidth);

				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, Center.X, top, delta)) result.Y = (int)Math.Floor(top);
				else result.Y = (int)Math.Floor(Math.Min(StartPoint.Y, EndPoint.Y) - halfLineWidth);

				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, right, Center.Y, delta)) result.Width = (int)Math.Ceiling(right) - result.X;
				else result.Width = (int)Math.Ceiling(Math.Max(StartPoint.X, EndPoint.X) + halfLineWidth) - result.X;

				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, Center.X, bottom, delta)) result.Height = (int)Math.Ceiling(bottom) - result.Y;
				else result.Height = (int)Math.Ceiling(Math.Max(StartPoint.Y, EndPoint.Y) + halfLineWidth) - result.Y;
			}
			if (!capBounds.IsEmpty) result = Geometry.UniteRectangles(result, capBounds);
			return result;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			if (base.ContainsPointCore(x, y))
				return true;
			float lineContainsDelta = (int)Math.Ceiling(LineStyle.LineWidth / 2f) + 2;
			if (IsLine) return Geometry.LineContainsPoint(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, true, x, y, lineContainsDelta);
			else {
				if (Geometry.IsValid(Center))
					return Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, x, y, lineContainsDelta);
				else return Geometry.ArcContainsPoint(
						StartPoint.X, StartPoint.Y, 
						RadiusPoint.X, RadiusPoint.Y, 
						EndPoint.X, EndPoint.Y, 
						lineContainsDelta, x, y);
			}
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.X = x;
			rectangle.Y = y;
			rectangle.Width = width;
			rectangle.Height = height;
			if (IsLine) {
				if (Geometry.RectangleIntersectsWithLine(rectangle, 
					StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, true))
					return true;
			} else {
				if (Geometry.ArcIntersectsWithRectangle(StartPoint.X, StartPoint.Y,
					RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, rectangle))
					return true;
			}
			return false;
		}


		/// <override></override>
		protected override bool MoveByCore(int deltaX, int deltaY) {
			// move cap bounds and cap points			
			if (base.MoveByCore(deltaX, deltaY)) {
				if (Geometry.IsValid(center)) {
					center.X += deltaX;
					center.Y += deltaY;
				}
				return true;
			} else {
				InvalidateDrawCache();
				return false;
			}
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			int pointIndex = GetControlPointIndex(pointId);
			
			float radiusPtAngle = 0;
			bool maintainAspect = false;
			ControlPointId otherGluePtId = ControlPointId.None;
			ControlPointId nextPtId = ControlPointId.None;
			if ((modifiers & ResizeModifiers.MaintainAspect) == ResizeModifiers.MaintainAspect
				 && (IsFirstVertex(pointId) || IsLastVertex(pointId))) {
				maintainAspect = true;
				// Get opposite glue point and the glue point next to the moved glue point
				// (may be identical to the opposite glue point)
				if (IsFirstVertex(pointId)) {
					otherGluePtId = ControlPointId.LastVertex;
					nextPtId = GetPreviousVertexId(otherGluePtId);
				} else {
					otherGluePtId = ControlPointId.FirstVertex;
					nextPtId = GetNextVertexId(otherGluePtId);
				}
				// Calculate the original angle for later use
				radiusPtAngle = Geometry.RadiansToDegrees(Geometry.Angle(
					GetControlPointPosition(pointId),
					GetControlPointPosition(otherGluePtId),
					GetControlPointPosition(nextPtId)));
			}

			// Assign new position to vertex
			Point p = vertices[pointIndex];
			p.Offset(deltaX, deltaY);
			vertices[pointIndex] = p;

			if (maintainAspect) {
				if (IsLine) {
					MaintainGluePointPosition(otherGluePtId, nextPtId);
					if (VertexCount > 2)
						vertices[1] = Geometry.VectorLinearInterpolation(StartPoint, EndPoint, 0.5f);
				} else {
					// Try to maintain angle between StartPoint and RadiusPoint
					Point movedPtPos = GetControlPointPosition(pointId);
					Point otherGluePtPos = GetControlPointPosition(otherGluePtId);
					int hX = otherGluePtPos.X;
					int hY = otherGluePtPos.Y;
					Geometry.RotatePoint(movedPtPos.X, movedPtPos.Y, radiusPtAngle, ref hX, ref hY);

					int aPb, bPb, cPb; // perpendicular bisector
					int aR, bR, cR;	// line through start point and radius point
					Geometry.CalcPerpendicularBisector(movedPtPos.X, movedPtPos.Y, otherGluePtPos.X, otherGluePtPos.Y, out aPb, out bPb, out cPb);
					Geometry.CalcLine(movedPtPos.X, movedPtPos.Y, hX, hY, out aR, out bR, out cR);

					Point newPos = Geometry.IntersectLines(aPb, bPb, cPb, aR, bR, cR);
					Debug.Assert(Geometry.IsValid(newPos));
					vertices[1] = newPos;

					// After moving the point between the glue points, we have to recalculate the glue point
					// positions again
					MaintainGluePointPosition(ControlPointId.LastVertex, GetPreviousVertexId(ControlPointId.LastVertex));
					MaintainGluePointPosition(ControlPointId.FirstVertex, GetNextVertexId(ControlPointId.FirstVertex));
				}
			} else {
				MaintainGluePointPosition(ControlPointId.FirstVertex, pointId);
				MaintainGluePointPosition(ControlPointId.LastVertex, pointId);
			}
			return true;
		}


		/// <override></override>
		protected override Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			if (IsLine) {
				ControlPointId secondPtId = (gluePointId == ControlPointId.FirstVertex) ? ControlPointId.LastVertex : ControlPointId.FirstVertex;
				// If the line only has 2 vertices and both are connected via Point-To-Shape connection...
				if (IsConnected(ControlPointId.FirstVertex, null) == ControlPointId.Reference
					&& IsConnected(ControlPointId.LastVertex, null) == ControlPointId.Reference) {
					// ... calculate new point position from the position of the second shape:
					Shape secondShape = GetConnectionInfo(secondPtId, null).OtherShape;
					return CalcGluePointFromPosition(gluePointId, shape, secondShape.X, secondShape.Y);
				} else {
					Point p = GetControlPointPosition(secondPtId);
					Point result = CalcGluePointFromPosition(gluePointId, shape, p.X, p.Y);
					return result;
				}
			} else {
				Point pos = CalcGluePointCalculationBase(gluePointId, shape);
				return CalcGluePointFromPosition(gluePointId, shape, pos.X, pos.Y);
			}
		}


		/// <override></override>
		protected override float CalcCapAngle(ControlPointId pointId) {
			if (pointId == ControlPointId.FirstVertex)
				return CalcCapAngle(GetControlPointIndex(pointId), StartCapStyleInternal.CapSize);
			else if (pointId == ControlPointId.LastVertex)
				return CalcCapAngle(GetControlPointIndex(pointId), EndCapStyleInternal.CapSize);
			else throw new NotSupportedException();
		}


		/// <override></override>
		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			arcIsInvalid = true;
			center = Geometry.InvalidPointF;
			radius = float.NaN;
			arcStartAngle = float.NaN;
			arcSweepAngle = float.NaN;
			arcBounds = Geometry.InvalidRectangleF;
		}


		/// <override></override>
		protected override void RecalcDrawCache() {
			// Reset draw cache to origin (for later transformation)
			if (shapePoints.Length != vertices.Count) Array.Resize<Point>(ref shapePoints, vertices.Count);
			for (int i = vertices.Count - 1; i >= 0; --i) {
				Point p = vertices[i];
				p.Offset(-X, -Y);
				shapePoints[i] = p;
			}
			// Recalculate arc parameters (RecalculateArc does nothing if IsLine is true)
			RecalculateArc();

			// Calculate boundingRectangle of the arc (required for drawing and invalidating)
			arcBounds.X = center.X - X - radius;
			arcBounds.Y = center.Y - Y - radius;
			arcBounds.Width = arcBounds.Height = Math.Max(0.1f, radius + radius);

			base.RecalcDrawCache();
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			Matrix.TransformPoints(shapePoints);
			if (Geometry.IsValid(arcBounds)) arcBounds.Offset(deltaX, deltaY);
		}

		#endregion


		#region [Private] Properties

		/// <summary>
		/// returns the coordinates of the arc's start point. 
		/// The start point is defined as the first point of the arc (clockwise) and the underlying point id changes depending on the position of the other points.
		/// </summary>
		private Point StartPoint {
			get { return vertices[0]; }
		}


		/// <summary>
		/// returns the coordinates of the arc's end point. 
		/// The end point is defined as the first point of the arc (clockwise) and the underlying point id changes depending on the position of the other points.
		/// </summary>
		private Point EndPoint {
			get { return vertices[vertices.Count - 1]; }
		}


		/// <summary>
		/// This point defines the radius of the arc and the start point and end points. 
		/// The radius point is located between start point and end point of the arc per definition. 
		/// Which of the points are regarded as start point or end point depends on the position of the radius point.
		/// </summary>
		private Point RadiusPoint {
			get {
				if (VertexCount == 2)
					return Geometry.VectorLinearInterpolation(StartPoint, EndPoint, 0.5f);
				else if (VertexCount == 3) return vertices[1];
				else throw new IndexOutOfRangeException();
			}
		}


		private PointF Center {
			get {
				if (IsLine) center = Geometry.VectorLinearInterpolation((PointF)StartPoint, (PointF)EndPoint, 0.5f);
				else if (arcIsInvalid) RecalculateArc();
				return center;
			}
		}


		private float StartAngle {
			get {
				if (arcIsInvalid) RecalculateArc();
				return arcStartAngle;
			}
		}


		private float SweepAngle {
			get {
				if (arcIsInvalid) RecalculateArc();
				return arcSweepAngle;
			}
		}


		private float Radius {
			get {
				if (IsLine) return float.PositiveInfinity;
				else if (arcIsInvalid) RecalculateArc();
				return radius;
			}
		}


		private bool IsLine {
			get {
				return (vertices.Count == 2
					|| Geometry.LineContainsPoint(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, false, RadiusPoint.X, RadiusPoint.Y));
			}
		}

		#endregion


		#region [Private] Methods

		private void Construct() {
			InvalidateDrawCache();
		}


		private IEnumerator<MenuItemDef> GetBaseActions(int mouseX, int mouseY, int range) {
			return base.GetMenuItemDefs(mouseX, mouseY, range).GetEnumerator();
		}


		private void RecalculateArc() {
			if (!IsLine) {
				center = Geometry.CalcArcCenterAndRadius(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, out radius);
				if (Geometry.IsValid(center)) {
					// Calculate center point and radius
					CalculateAngles(center, StartPoint, RadiusPoint, EndPoint, out arcStartAngle, out arcSweepAngle);
					// Calculate arc's bounds
					arcBounds.X = center.X - radius;
					arcBounds.Y = center.Y - radius;
					arcBounds.Width = arcBounds.Height = radius + radius;
					arcIsInvalid = false;
				}
			}
		}
		
		
		private bool CalculateAngles(PointF centerPt, Point startPt, Point radiusPt, Point endPt, out float startAngle, out float sweepAngle) {
			if (!Geometry.IsValid(centerPt)) throw new ArgumentException("centerPt");
			if (!Geometry.IsValid(startPt)) throw new ArgumentException("startPt");
			if (!Geometry.IsValid(endPt)) throw new ArgumentException("endPt");
			if (!Geometry.IsValid(radiusPt)) throw new ArgumentException("radiusPt");
			
			startAngle = sweepAngle = float.NaN;

			// Calculate vertex angles
			float p0Angle = Geometry.Angle(centerPt.X, centerPt.Y, startPt.X, startPt.Y);
			float p1Angle = Geometry.Angle(centerPt.X, centerPt.Y, radiusPt.X, radiusPt.Y);
			float p2Angle = Geometry.Angle(centerPt.X, centerPt.Y, endPt.X, endPt.Y);
			
			// Sort vertices in order to calculate start- and sweep angle
			int startPtIdx, endPtIdx;
			if (p0Angle >= 0 && p1Angle >= 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 1: All angles positive
				//
				if (p0Angle < p1Angle && p1Angle < p2Angle) {
					startPtIdx = 0;
					endPtIdx = 2;
				} else if (p0Angle >= p1Angle && p1Angle >= p2Angle) {
					startPtIdx = 2;
					endPtIdx = 0;
				} else {
					if (p0Angle < p2Angle) {
						startPtIdx = 2;
						endPtIdx = 0;
					} else {
						startPtIdx = 0;
						endPtIdx = 2;
					}
				}
			} else if (p0Angle <= 0 && p1Angle <= 0 && p2Angle <= 0) {
				//===============================================================================
				// Case 2: All angles negative
				//
				if (p0Angle < p1Angle && p1Angle < p2Angle) {
					startPtIdx = 0;
					endPtIdx = 2;
				} else if (p0Angle >= p1Angle && p1Angle >= p2Angle) {
					startPtIdx = 2;
					endPtIdx = 0;
				} else {
					if (p0Angle > p2Angle) {
						startPtIdx = 0;
						endPtIdx = 2;
					} else {
						startPtIdx = 2;
						endPtIdx = 0;
					}
				}
			} else if (p0Angle >= 0 && p1Angle >= 0 && p2Angle < 0) {
				//===============================================================================
				// Case 3: startPt's angle positive, radiusPt's angle positive, endPt's angle negative
				//
				if (p0Angle < p1Angle) {
					startPtIdx = 0;
					endPtIdx = 2;
				} else {
					startPtIdx = 2;
					endPtIdx = 0;
				}
			} else if (p0Angle >= 0 && p1Angle < 0 && p2Angle < 0) {
				//===============================================================================
				// Case 4: startPt's angle positive, radiusPt's angle negative, endPt's angle negative
				//
				if (p1Angle < p2Angle) {
					startPtIdx = 0;
					endPtIdx = 2;
				} else {
					startPtIdx = 2;
					endPtIdx = 0;
				}
			} else if (p0Angle < 0 && p1Angle < 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 5: startPt's angle negative, radiusPt's angle negative, endPt's angle positive
				//
				if (p0Angle < p1Angle) {
					startPtIdx = 0;
					endPtIdx = 2;
				} else {
					startPtIdx = 2;
					endPtIdx = 0;
				}
			} else if (p0Angle < 0 && p1Angle >= 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 6: startPt's angle negative, radiusPt's angle positive, endPt's angle positive
				//
				if (p1Angle < p2Angle) {
					startPtIdx = 0;
					endPtIdx = 2;
				} else {
					startPtIdx = 2;
					endPtIdx = 0;
				}
			} else if (p0Angle >= 0 && p1Angle < 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 7: startPt's angle positive, radiusPt's angle negative, endPt's angle positive
				//
				if (p0Angle < p2Angle) {
					startPtIdx = 2;
					endPtIdx = 0;
				} else {
					startPtIdx = 0;
					endPtIdx = 2;
				}
			} else if (p0Angle < 0 && p1Angle >= 0 && p2Angle < 0) {
				//===============================================================================
				// Case 8: startPt's angle negative, radiusPt's angle positive, endPt's angle negative
				//
				if (p0Angle < p2Angle) {
					startPtIdx = 2;
					endPtIdx = 0;
				} else {
					startPtIdx = 0;
					endPtIdx = 2;
				}
			} else if (float.IsNaN(p0Angle) && float.IsNaN(p1Angle) && float.IsNaN(p2Angle)) {
				//===============================================================================
				// Case 9: No Solution: Arc is not defined
				//
				startPtIdx = 0;
				endPtIdx = 2;
				return false;
			} else throw new NShapeInternalException("Unable to calculate drawCache.");

			// calculate angles
			sweepAngle = Geometry.RadiansToDegrees(Geometry.Angle(centerPt.X, centerPt.Y, vertices[startPtIdx].X, vertices[startPtIdx].Y, vertices[endPtIdx].X, vertices[endPtIdx].Y));
			if (sweepAngle < 0) sweepAngle = (360 + sweepAngle) % 360;
			if (startPtIdx == 0) {
				startAngle = Geometry.RadiansToDegrees(Geometry.Angle(centerPt.X, centerPt.Y, vertices[startPtIdx].X, vertices[startPtIdx].Y));
				if (startAngle < 0) startAngle = (360 + startAngle) % 360;
			} else {
				// if startPt and endPt were swapped, invert the sweepAngle - otherwise the line cap will be drawn on the wrong side.
				startAngle = Geometry.RadiansToDegrees(Geometry.Angle(centerPt.X, centerPt.Y, vertices[endPtIdx].X, vertices[endPtIdx].Y));
				sweepAngle = -sweepAngle;
			}
			return true;
		}


		private float CalcCapAngle(int pointIndex, float capSize) {
			if (Geometry.DistancePointPoint(StartPoint, EndPoint) < capSize)
				capSize = Geometry.DistancePointPoint(StartPoint, EndPoint);
			foreach (PointF p in Geometry.IntersectCircleArc((float)vertices[pointIndex].X, (float)vertices[pointIndex].Y, capSize, StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius)) {
				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, p.X, p.Y, 1)) {
					float angle = Geometry.RadiansToDegrees(Geometry.Angle(vertices[pointIndex].X, vertices[pointIndex].Y, p.X, p.Y));
					return angle;
				}
			}
			return 0;

			//float angle = float.NaN;
			//int x = vertices[pointIndex].X;
			//int y = vertices[pointIndex].Y;
			//int radPtX = RadiusPoint.X;
			//int radPtY = RadiusPoint.Y;

			//// Calculate points on the arc for calculating the intersection point of the cap with the arc point on the 
			//// correct side of the arc. 
			//// If the distance between StartPoint and EndPoint is smaller than the cap size, there are two valid 
			//// intersection points if we use the whole arc for calculation of the intersection point. In order to avoid
			//// this problem, we calculate the intersection point with only half of the arc
			//PointF endPt = PointF.Empty, radPt = PointF.Empty;
			//endPt = radPt = Center;
			//endPt.X = radPt.X = Center.X + Radius;
			//float alpha = arcSweepAngle / 4f;
			//endPt = Geometry.RotatePoint(Center, arcStartAngle + (alpha + alpha), endPt);
			//if (pointIndex == 0) radPt = Geometry.RotatePoint(Center, arcStartAngle + alpha, radPt);
			//else radPt = Geometry.RotatePoint(Center, arcStartAngle + (alpha + alpha + alpha), radPt);
			//// calculate intersection point and calculate shapeAngle
			//foreach (PointF p in Geometry.IntersectCircleArc(x, y, capSize, x, y, radPt.X, radPt.Y, endPt.X, endPt.Y, Center.X, Center.Y, Radius)) {
			//   angle = Geometry.RadiansToDegrees(Geometry.Angle(x, y, p.X, p.Y));
			//   break;
			//}
			//if (float.IsNaN(angle)) {
			//   //throw new NShapeInternalException("Angle of line cap could not be calculated.");
			//   angle = 0;
			//}
			//return angle;
		}


		private Point CalcGluePointCalculationBase(ControlPointId gluePointId, Shape shape) {
			Point result = Geometry.InvalidPoint;
			if (!IsLine) {
				// Calculate a new radius for the arc:
				// A circle through the 2 end points of the arc (or the centers of the shapes they are connected to)
				// and the arc's radius point.
				ShapeConnectionInfo startCi = GetConnectionInfo(ControlPointId.FirstVertex, null);
				ShapeConnectionInfo endCi = GetConnectionInfo(ControlPointId.LastVertex, null);
				Point startPt = Point.Empty, endPt = Point.Empty;
				if (startCi.IsEmpty) startPt.Offset(StartPoint.X, StartPoint.Y);
				else startPt.Offset(startCi.OtherShape.X, startCi.OtherShape.Y);
				if (endCi.IsEmpty) endPt.Offset(EndPoint.X, EndPoint.Y);
				else endPt.Offset(endCi.OtherShape.X, endCi.OtherShape.Y);
				float r;
				PointF centerPt = Geometry.CalcArcCenterAndRadius(startPt, RadiusPoint, endPt, out r);
				//
				// Calculate tangent on the desired arc through the other shape's center
				Point tangentPt = IsFirstVertex(gluePointId) ? startPt : endPt;
				float a, b, c;
				Geometry.CalcPerpendicularLine(centerPt.X, centerPt.Y, tangentPt.X, tangentPt.Y, out a, out b, out c);
				float aT, bT, cT;
				Geometry.TranslateLine(a, b, c, tangentPt, out aT, out bT, out cT);
				//
				// Calculate intersection point of the calculated tangent and the perpendicular bisector 
				// of the line through startPt and endPt
				Geometry.CalcPerpendicularBisector(startPt.X, startPt.Y, endPt.X, endPt.Y, out a, out b, out c);
				PointF pT = Geometry.IntersectLines(aT, bT, cT, a, b, c);
				if (Geometry.IsValid(pT)) {
					PointF pB = Geometry.VectorLinearInterpolation(startPt, endPt, 0.5f);
					result = Point.Round(Geometry.VectorLinearInterpolation(pB, pT, 0.75f));
					// Check if the calculated point is on the right side
					if (!Geometry.ArcIntersectsWithLine(startPt.X, startPt.Y, RadiusPoint.X, RadiusPoint.Y, endPt.X, endPt.Y, result.X, result.Y, centerPt.X, centerPt.Y, true))
						result = Geometry.VectorLinearInterpolation(result, tangentPt, 2);
				}
			}
			// If the arc only has 2 points or something went wrong while calculating the desired arc
			if (!Geometry.IsValid(result)) result = Geometry.VectorLinearInterpolation(StartPoint, EndPoint, 0.5f);
			return result;
		}

		#endregion


		#region Fields

		// Property buffers
		private bool arcIsInvalid = true;
		private PointF center = Geometry.InvalidPointF;
		private float radius = float.NaN;

		// Draw cache
		private float arcStartAngle = float.NaN;
		private float arcSweepAngle = float.NaN;
		private RectangleF arcBounds = Geometry.InvalidRectangleF;
		
		#endregion
	}

}
