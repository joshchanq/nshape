using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Dataweb.Diagramming.Advanced {

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
		/// Removes the point with the given PointId from the line.
		/// </summary>
		/// <param name="pointId">PointId of the point to remove.</param>
		void RemoveVertex(ControlPointId pointId);

		/// <summary>
		/// All Vertices of the line, ordered by their appearance on the line from start point to end point.
		/// A Vertex is defined as a ControlPoint which defines the shape of the linear shape.
		/// </summary>
		//IEnumerable<Point> Vertices { get; }

		ControlPointId GetNextVertexId(ControlPointId vertexId);

		ControlPointId GetPreviousVertexId(ControlPointId vertexId);

		int MinVertexCount { get; }

		int MaxVertexCount { get; }

		int VertexCount { get; }

		Point CalculateConnectionFoot(int x1, int y1, int x2, int y2);

		Point CalcNormalVector(Point point);

		bool IsDirected { get; }
	}


	///<summary>
	///A one-dimensional shape
	///</summary>
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
			if (StartCapStyleInternal != null)
				privateStartCapStyle = styleSet.GetPreviewStyle(styleSet.CapStyles.None);
			else privateStartCapStyle = styleSet.GetPreviewStyle(StartCapStyleInternal);
			if (EndCapStyleInternal == null)
				privateEndCapStyle = styleSet.GetPreviewStyle(styleSet.CapStyles.None);
			else privateEndCapStyle = styleSet.GetPreviewStyle(EndCapStyleInternal);
		}


		/// <override></override>
		public abstract Point CalculateConnectionFoot(int x1, int y1, int x2, int y2);


		/// <override></override>
		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			return Enumerator.Create(this, controlPointCapability);
		}


		///// <override></override>
		//public override ControlPointId IsConnected(ControlPointId ownPointId, Shape otherShape) {
		//   if (connectionInfos != null) {
		//      for (int i = connectionInfos.Count - 1; i >= 0; --i) {
		//         if ((otherShape == null || connectionInfos[i].OtherShape == otherShape)
		//            && (ownPointId == ControlPointId.None || GetControlPointIndex(ownPointId) == GetControlPointIndex(connectionInfos[i].OwnPointId)))
		//            return connectionInfos[i].OtherPointId;
		//      }
		//   }
		//   return ControlPointId.None;
		//}


		/// <override></override>
		public override bool NotifyStyleChanged(IStyle style) {
			bool result = base.NotifyStyleChanged(style);
			if (style == null || IsStyleAffected(StartCapStyleInternal, style) || IsStyleAffected(EndCapStyleInternal, style)) {
				Invalidate();
				InvalidateDrawCache();
				//UpdateDrawCache();
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
				else throw new IndexOutOfRangeException();
			}
		}


		/// <override></override>
		public override IEnumerable<DiagrammingAction> GetActions(int mouseX, int mouseY, int range) {
			// return actions of base class
			IEnumerator<DiagrammingAction> enumerator = GetBaseActions(mouseX, mouseY, range);
			while (enumerator.MoveNext()) yield return enumerator.Current;

			ControlPointId clickedPointId = HitTest(mouseX, mouseY, ControlPointCapabilities.All, range);

			// return own actions
			bool isFeasible;
			string description;

			isFeasible = ContainsPoint(mouseX, mouseY) 
				&& (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference)
				&& (VertexCount < MaxVertexCount);
			description = "You have to click on the line in order to insert new points";
			yield return new CommandAction("Insert Point", null, description, isFeasible,
				new InsertVertexCommand(this, mouseX, mouseY));

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
			yield return new CommandAction("Remove Point", null, description, isFeasible,
				new RemoveVertexCommand(this, clickedPointId));
		}


		#endregion


		#region IEntity Members

		/// <override></override>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in ShapeBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("StartCapStyle", typeof(object));
			yield return new EntityFieldDefinition("EndCapStyle", typeof(object));
			yield return new EntityInnerObjectsDefinition("Vertices", "Core.Point", pointAttrNames, pointAttrTypes);
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
			if (propertyName == "Vertices") {
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
			if (propertyName == "Vertices") {
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


		/// <summary>
		/// Protetced internal constructur. Should only be called by the Type's CreateShapeDelegate
		/// </summary>
		/// <param name="shapeType"></param>
		/// <param name="template"></param>
		protected internal LineShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			Construct();
		}



		/// <summary>
		/// Protetced internal constructur. Should only be called by the Type's CreateShapeDelegate
		/// </summary>
		/// <param name="shapeType"></param>
		/// <param name="styleSetProvider"></param>
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
			if (propertyMapping.ShapePropertyId == PropertyIdLineStyle)
				LineStyle = propertyMapping.GetStyle() as ILineStyle;
			else base.ProcessExecModelPropertyChange(propertyMapping);
		}


		/// <override></override>
		protected override bool RotateCore(int angle, int x, int y) {
			bool result = false;
			if (IsConnected(1, null) != ControlPointId.None || IsConnected(2, null) != ControlPointId.None)
				result = false;
			else {
				TransformDrawCache(0, 0, angle, x, y);
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
			if (startCapBounds != Geometry.InvalidRectangle)
				startCapBounds.Offset(deltaX, deltaY);
			if (endCapBounds != Geometry.InvalidRectangle)
				endCapBounds.Offset(deltaX, deltaY);
			TransformDrawCache(deltaX, deltaY, 0, X, Y);
			return true;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
				if (StartCapContainsPoint(x, y)) return true;
			if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
				if (EndCapContainsPoint(x, y)) return true;
			return false;
		}


		/// <summary>
		/// Specifies the tolerance when performing hit tests and intersection calculations
		/// </summary>
		private float ContainsPointDelta {
			get { return 2; }
		}


		protected bool IsFirstVertex(ControlPointId pointId) {
			return (pointId == ControlPointId.FirstVertex || pointIds.IndexOf(pointId) == 0);
		}


		protected bool IsLastVertex(ControlPointId pointId) {
			return (pointId == ControlPointId.LastVertex || pointIds.IndexOf(pointId) == VertexCount - 1);
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
			return true;
		}


		/// <summary>
		/// Internal start CapStyle of the line. May be published by a decendant through a property
		/// </summary>
		protected ICapStyle StartCapStyleInternal {
			get { return privateStartCapStyle ?? ((LineShapeBase)Template.Shape).StartCapStyleInternal; }
			set {
				Invalidate();
				privateStartCapStyle = (Template != null && value == ((LineShapeBase)Template.Shape).StartCapStyleInternal) ? null : value;
				InvalidateDrawCache();
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
				privateEndCapStyle = (Template != null && value == ((LineShapeBase)Template.Shape).EndCapStyleInternal) ? null : value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		#region Line cap stuff

		protected float StartCapAngle {
			get {
				if (float.IsNaN(startCapAngle))
					startCapAngle = CalcCapAngle(ControlPointId.FirstVertex);
				return startCapAngle;
			}
		}


		protected float EndCapAngle {
			get {
				if (float.IsNaN(endCapAngle)) 
					endCapAngle = CalcCapAngle(ControlPointId.LastVertex);
				return endCapAngle;
			}
		}


		protected Rectangle StartCapBounds {
			get {
				if (startCapBounds == Geometry.InvalidRectangle) {
					if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None)
						startCapBounds = ToolCache.GetCapBounds(StartCapStyleInternal, LineStyle, StartCapAngle);
					else startCapBounds = Rectangle.Empty;
					startCapBounds.Offset(vertices[GetControlPointIndex(ControlPointId.FirstVertex)]);
				}
				return startCapBounds;
			}
		}


		protected Rectangle EndCapBounds {
			get {
				if (endCapBounds == Geometry.InvalidRectangle) {
					if (EndCapStyleInternal != null && EndCapStyleInternal.CapShape != CapShape.None)
						endCapBounds = ToolCache.GetCapBounds(EndCapStyleInternal, LineStyle, EndCapAngle);
					else endCapBounds = Rectangle.Empty;
					endCapBounds.Offset(vertices[GetControlPointIndex(ControlPointId.LastVertex)]);
				}
				return endCapBounds;
			}
		}


		/// <summary>
		/// Performs an intersection test on the LineCap
		/// </summary>
		protected bool StartCapIntersectsWith(Rectangle rectangle) {
			if (StartCapStyleInternal != null && StartCapStyleInternal.CapShape != CapShape.None) {
				if (startCapBounds.IntersectsWith(rectangle)) {
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
				if (endCapBounds.IntersectsWith(rectangle)) {
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
			if (startCapBounds.Contains(pointX, pointY)) {
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
			if (endCapBounds.Contains(pointX, pointY)) {
				if (endCapPointsBuffer == null)
					CalcCapPoints(GetControlPointIndex(ControlPointId.LastVertex), endCapAngle, EndCapStyleInternal, LineStyle, ref endCapBounds, ref endCapPointsBuffer);
				if (Geometry.ConvexPolygonContainsPoint(endCapPointsBuffer, pointX, pointY))
					return true;
			}
			return false;
		}

		#endregion

	
		#region Drawing and shape calculation

		/// <summary>
		/// Calculates the line cap angle for the given control point ControlPointId.LineStart or ControlPointId.LineEnd
		/// </summary>
		/// <returns>Line cap angle in degrees</returns>
		protected abstract float CalcCapAngle(ControlPointId pointId);


		/// <override></override>
		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			// Do not delete shapePoints or cap buffers here for performance reasons
			startCapAngle = endCapAngle = float.NaN;
			startCapBounds = endCapBounds = Geometry.InvalidRectangle;
		}


		/// <override></override>
		protected override void UpdateDrawCache() {
			if (drawCacheIsInvalid && shapePoints != null) {
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
		/// <param name="deltaAngleDeg">Rotation shapeAngle in tenths of degrees</param>
		/// <param name="shapePosition">X coordinate of the rotation center</param>
		/// <param name="shapeRotationCenter">Y coordinate of the rotation center</param>
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

		#endregion


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
			TransformCapToOrigin(shapePoints[pointIndex].X, shapePoints[pointIndex].Y, capAngle, ref pointBuffer);
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


		private IEnumerator<DiagrammingAction> GetBaseActions(int mouseX, int mouseY, int range) {
			return base.GetActions(mouseX, mouseY, range).GetEnumerator();
		}


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

		private static string[] pointAttrNames = new string[] { "PointIndex", "PointId", "X", "Y" };
		private static Type[] pointAttrTypes = new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) };

		// List of PointIds for mapping PointIndex to PointIds
		protected List<ControlPointId> pointIds;
		// List of Vertices (absolute coordinates)
		protected List<Point> vertices;
		// Array of points used for drawing
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
	public abstract class PolylineBase : LineShapeBase {

		#region Shape Members

		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);

			//Point p = Point.Empty;
			//p.Offset(source.X, source.Y);
			//vertices[0] = p;

			if (source is PolylineBase) {
				// Vertices and CapStyles will be copied by the base class
				// so there's nothing left to do here...
			}
		}


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


		public override IEnumerable<Point> CalculateCells(int cellSize) {
			// The outer bounding rectangle (including the control points) is required here
			Point startCell = Point.Empty;
			Point endCell = Point.Empty;
			int j;
			for (int i = VertexCount - 1; i > 0; --i) {
				j = i - 1;
				// Calculate start and end cell
				//
				// Optimization:
				// Use integer division for values >= 0 (>20 times faster than floored float divisions)
				// Use floored float division for values < 0 (otherwise calculating intersection with cell 
				// bounds will not work collectly)
				if (vertices[i].X >= 0) startCell.X = vertices[i].X / cellSize;
				else startCell.X = (int)Math.Floor(vertices[i].X / (float)cellSize);
				if (vertices[i].Y >= 0) startCell.Y = vertices[i].Y / cellSize;
				else startCell.Y = (int)Math.Floor(vertices[i].Y / (float)cellSize);
				yield return startCell;
				if (vertices[i].X >= 0) endCell.X = vertices[j].X / cellSize;
				else endCell.X = (int)Math.Floor(vertices[j].X / (float)cellSize);
				if (vertices[i].Y >= 0) endCell.Y = vertices[j].Y / cellSize;
				else endCell.Y = (int)Math.Floor(vertices[j].Y / (float)cellSize);
				if (startCell == endCell) {
					// if the segment end is in the same cell, continue with the next segment...
					continue;
				} else {
					// ... otherwise return the end cell
					yield return endCell;
				}

				Point p = Point.Empty;
				if (startCell.X == endCell.X || startCell.Y == endCell.Y) {
					// Intersection test is not necessary
					p.Offset(Math.Min(startCell.X, endCell.X), Math.Min(startCell.Y, endCell.Y));
					int endX = Math.Max(startCell.X, endCell.X);
					int endY = Math.Max(startCell.Y, endCell.Y);
					if (startCell.Y == endCell.Y)
						while (++p.X < endX) yield return p;
					else while (++p.Y < endY) yield return p;
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
							//xTo = (p.X >= 0) ? xFrom + cellSize : xFrom - cellSize;
						} else {
							xTo = p.X * cellSize;
							xFrom = xTo + cellSize;
							//xFrom = (p.X >= 0) ? xTo + cellSize : xTo - cellSize;
						}
						if (stepY > 0) {
							yFrom = p.Y * cellSize;
							yTo = yFrom + cellSize;
							//yTo = (p.Y >= 0) ? yFrom + cellSize : yFrom - cellSize;
						} else {
							yTo = p.Y * cellSize;
							yFrom = yTo + cellSize;
							//yFrom = (p.Y >= 0) ? yTo + cellSize : yTo - cellSize;
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
							if (nextHCell != endCell) yield return nextHCell;
							else endCellReached = true;
							// Return nearby vertical cell
							nextVCell.Y += stepY;
							if (nextVCell != endCell) yield return nextVCell;
							else endCellReached = true;
							// Return next diagonal cell
							p.Offset(stepX, stepY);
							if (p != endCell) yield return p;
							else endCellReached = true;
						}
						else if (verticalIntersection) {
							p.X += stepX;
							if (p != endCell) yield return p;
							else endCellReached = true;
						} 
						else if (horizontalIntersection) {
							p.Y += stepY;
							if (p != endCell) yield return p;
							else endCellReached = true;
						} else {
							// This should never happen!
							Debug.Fail("Error while calculating cells: Line does not intersect expected cell borders!");
							endCellReached = true;	// Prevent endless loop
						}
					} while (!endCellReached);
				}
			}
		}


		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			Point result = Point.Empty;
			int idx = GetControlPointIndex(relativePosition.A);
			if (idx < 0) result = vertices[0];
			else result = Geometry.VectorLinearInterpolation(vertices[idx], vertices[idx + 1], relativePosition.B / 1000f);
			// ToDo: Handle rotated lines
			return result;
		}


		public override RelativePosition CalculateRelativePosition(int x, int y) {
			// The RelativePosition of a PolyLine is defined as
			// A = PointId to start from
			// B = Tenths of percentage of the line segment fromA to the next point
			RelativePosition result = RelativePosition.Empty;
			int cnt = vertices.Count - 1;
			for (int i = 0; i < cnt; ++i) {
				if (Geometry.LineContainsPoint(x, y, vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y, LineStyle.LineWidth + 2, true)) {
					result.A = GetControlPointId(i);
					float segmentLength = Geometry.DistancePointPoint(vertices[i], vertices[i + 1]);
					float distToPt = Geometry.DistancePointPoint(vertices[i].X, vertices[i].Y, x, y);
					result.B = (int)Math.Round((distToPt / (segmentLength / 100)) * 10);
					// ToDo: Handle rotated lines
					Debug.Assert(result.B >= 0 || result.B <= 1000, "Calculated percentage out of range.");
					break;
				}
			}
			if (result == RelativePosition.Empty) result.A = result.B = 0;
			return result;
		}


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


		public override Point CalculateConnectionFoot(int x1, int y1, int x2, int y2) {
			Point result = Geometry.InvalidPoint;
			float distance;
			float lowestIntersectionDistance = float.MaxValue;
			float lowestPointDistance = float.MaxValue;
			// find (nearest) intersection point between lines
			for (int i = VertexCount - 2; i >= 0; --i) {
				Point p = Geometry.IntersectLineWithLineSegment(x1, y1, x2, y2, vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y);
				if (p != Geometry.InvalidPoint) {
					distance = Math.Max(Geometry.DistancePointPoint(p.X, p.Y, x1, y1), Geometry.DistancePointPoint(p.X, p.Y, x2, y2));
					if (distance < lowestIntersectionDistance) {
						result = p;
						lowestIntersectionDistance = distance;
					}
				}
			}
			// if there is no intersection with any of the line's segments, return coordinates of the nearest point
			if (result == Geometry.InvalidPoint) {
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
				if (result == Geometry.InvalidPoint) {
					// this should never happen
					result = vertices[0];
				}
			}
			Debug.Assert(result != Geometry.InvalidPoint);
			return result;
		}


		public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			// If both gluepoints of a single segment line are connected via Point-To-Shape-Connections...
			if (this.VertexCount == 2 
				&& IsConnected(ControlPointId.FirstVertex, null) == ControlPointId.Reference
				&& IsConnected(ControlPointId.LastVertex, null) == ControlPointId.Reference) {
				Invalidate();

				// ... calculate both points at once:
				Shape shapeA = GetConnectionInfo(ControlPointId.FirstVertex, null).OtherShape;
				Shape shapeB = GetConnectionInfo(ControlPointId.LastVertex, null).OtherShape;

				Point currGluePtAPos = GetControlPointPosition(ControlPointId.FirstVertex);
				Point newGluePtAPos = shapeA.CalculateConnectionFoot(shapeB.X, shapeB.Y);

				Point currGluePtBPos = GetControlPointPosition(ControlPointId.LastVertex);
				Point newGluePtBPos = shapeB.CalculateConnectionFoot(shapeA.X, shapeA.Y);

				MoveConnectedGluePoint(ControlPointId.FirstVertex, newGluePtAPos.X - currGluePtAPos.X, newGluePtAPos.Y - currGluePtAPos.Y, ResizeModifiers.MaintainAspect);
				MoveConnectedGluePoint(ControlPointId.LastVertex, newGluePtBPos.X - currGluePtBPos.X, newGluePtBPos.Y - currGluePtBPos.Y, ResizeModifiers.MaintainAspect);

				ControlPointsHaveMoved();
				InvalidateDrawCache();
				Invalidate();

			} else base.FollowConnectionPointWithGluePoint(gluePointId, connectedShape, movedPointId);
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
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


		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			int j;
			for (int i = VertexCount - 1; i > 0; --i) {
				j = i - 1;
				if (Geometry.DistancePointPoint(x, y, vertices[i].X, vertices[i].Y) <= range) {
					ControlPointId ptId = GetControlPointId(i);
					if (HasControlPointCapability(ptId, controlPointCapability)) return ptId;
				} 
				if (Geometry.DistancePointPoint(x, y, vertices[j].X, vertices[j].Y) <= range) {
					ControlPointId ptId = GetControlPointId(j);
					if (HasControlPointCapability(ptId, controlPointCapability)) {
						if (HasControlPointCapability(ptId, controlPointCapability)) return ptId;
					}
				} 
				float d = Geometry.DistancePointLine(x, y, vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, true);
				if (d <= (LineStyle.LineWidth / 2f) + range) {
					if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability))
					return ControlPointId.Reference;
				}
			}
			return ControlPointId.None;
		}


		public override Point CalcNormalVector(Point point) {
			Point result = Point.Empty;
			int cnt = VertexCount - 1;
			for (int i = 0; i < cnt; ++i) {
				if (Geometry.LineContainsPoint(point.X, point.Y, vertices[i].X + X, vertices[i].Y + Y, vertices[i + 1].X + X, vertices[i + 1].Y + Y, LineStyle.LineWidth + 2, true)) {
					float lineAngle = Geometry.RadiansToDegrees(Geometry.Angle(vertices[i], vertices[i + 1]));
					int x = point.X + 100;
					int y = point.Y;
					Geometry.RotatePoint(point.X, point.Y, lineAngle + 90, ref x, ref y);
					result.X = x;
					result.Y = y;
					return result;
				}
			}
			if (result == Point.Empty) throw new DiagrammingException("The given Point is not part of the line shape.");
			return result;
		}


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


		public override void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			base.DrawOutline(graphics, pen);
			graphics.DrawLines(pen, shapePoints);
		}


		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			if (image == null) throw new ArgumentNullException("image");
			using (Graphics g = Graphics.FromImage(image)) {
				GdiHelpers.ApplyGraphicsSettings(g, DiagrammingRenderingQuality.MaximumQuality);
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

		#endregion


		#region ILinearShape Members

		public override int MinVertexCount { get { return 2; } }


		public override int MaxVertexCount { get { return int.MaxValue; } }


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


		public override ControlPointId AddVertex(int x, int y) {
			Invalidate();
			// find segment where the new point has to be inserted
			ControlPointId ptId = ControlPointId.None;
			for (int i = 0; i < this.ControlPointCount - 1; ++i) {
				int startX, startY, endX, endY;
				GetSegmentCoordinates(i, out startX, out startY, out endX, out endY);

				if (Geometry.LineContainsPoint(x, y, startX, startY, endX, endY, LineStyle.LineWidth + 2, true)) {					
					// ToDo: Falls die Distanz des Punktes x|y > 0 ist: Ausrechnen wo der Punkt sein muss (entlang der Lotrechten durch den Punkt verschieben)
					ControlPointId id = GetControlPointId(i + 1);
					ptId = InsertVertex(id, x, y);
					break;
				}
			}
			if (ptId == ControlPointId.None) throw new DiagrammingException("Cannot add vertex {0}.", new Point(x, y));
			Invalidate();
			return ptId;
		}


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


		protected internal PolylineBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			// nothing to do
		}


		protected internal PolylineBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			// nothing to do
		}


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


		protected override bool ContainsPointCore(int x, int y) {
			if (base.ContainsPointCore(x, y))
				return true;
			int cnt = VertexCount - 1;
			for (int i = 0; i < cnt; ++i) {
				if (Geometry.LineContainsPoint(x, y, vertices[i].X, vertices[i].Y, vertices[i + 1].X, vertices[i + 1].Y, (LineStyle.LineWidth / 2f) + 2, true))
					return true;
			}
			return false;
		}


		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			int vertexIdx = GetControlPointIndex(pointId);
			Point p = vertices[vertexIdx];
			p.Offset(deltaX, deltaY);
			vertices[vertexIdx] = p;

			// ToDo: Scale line if MainTainAspect flag is set and start- or endpoint was moved
			return true;

			////Rectangle boundsBefore = GetBoundingRectangle(true);
			//int pointIndex = GetControlPointIndex(pointId);
			//Point p = vertices[pointIndex];
			//p.Offset(deltaX, deltaY);
			//vertices[pointIndex] = p;
			//base.MovePointByCore(connectionPointId, deltaX, deltaY, modifiers);
			//Rectangle boundsAfter = GetBoundingRectangle(true);

			//if ((modifiers & ResizeModifiers.MaintainAspect) == ResizeModifiers.MaintainAspect) {
			//   int maxIdx;
			//   if (IsStartPoint(pointId)) {
			//      // neuer Ansatz:
			//      //maxIdx = vertices.Count - 1;
			//      //float scale = Geometry.CalcScaleFactor(boundsBefore.Width, boundsBefore.Height, boundsAfter.Width, boundsAfter.Height);
			//      //for (int i = 0; i < maxIdx; ++i ) {
			//      //   vertices[i + 1] = Geometry.VectorLinearInterpolation(vertices[i], vertices[i + 1], scale);
			//      //}



			//      //for (int i = 1; i < shapePoints.Length - 1; ++i) {
			//      //   float ratioX = shapePoints[i].X / (float)boundsBefore.right;
			//      //   float ratioY = shapePoints[i].Y / (float)boundsBefore.bottom;
			//      //   float x = (boundsAfter.X + (boundsAfter.Width * ratioX)) - (boundsBefore.X + (boundsBefore.Width * ratioX));
			//      //   float y = (boundsAfter.Y + (boundsAfter.Height * ratioY)) - (boundsBefore.Y + (boundsBefore.Height * ratioY));
			//      //   shapePoints[i].X += (int)Math.Round(x);
			//      //   shapePoints[i].Y += (int)Math.Round(y);
			//      //}

			//      //// Die einfache Lösung:
			//      //if (deltaX != 0 || deltaY != 0) {
			//      //   double dx = deltaX / (double)(VertexCount - 1);
			//      //   double dy = deltaY / (double)(VertexCount - 1);
			//      //   // the first and the last points are glue points, so move only the points between
			//      //   for (int i = 1; i < shapePoints.Length - 1; ++i) {
			//      //      shapePoints[i].X = (int)Math.Round(shapePoints[i].X + dx);
			//      //      shapePoints[i].Y = (int)Math.Round(shapePoints[i].Y + dy);
			//      //   }
			//      //   InvalidateDrawCache();
			//      //}
			//   } else if (IsEndPoint(pointId)) {
			//   }
			//   ControlPointsHaveMoved();
			//} else {
			//   if (pointIndex < 2) {
			//      InvalidateDrawCache();
			//   }
			//   if (pointIndex > VertexCount - 3) {
			//      InvalidateDrawCache();
			//   }
			//   //if (pointIndex != 0 && pointIndex != shapePoints.Length - 1)
			//   //   ControlPointHasMoved(connectionPointId);
			//   //ControlPointHasMoved(ControlPointId.Reference);
			//}

			//// ToDo: Hier etwas schlaueres überlegen: base.MoveControlPointBy ruft immer Invalidate auf, invalidiert also alle segmente obwohl man eigentlich nur die an den Punkt angrenzenden Segmente invalidieren müsste
			////InvalidatePointSegments(connectionPointId);
			//return base.MovePointByCore(pointId, deltaX, deltaY, modifiers);
		}


		//public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
		//   bool handled = false;
		//   if (VertexCount == MinVertexCount) {
		//      ShapeConnectionInfo startPtCi = ShapeConnectionInfo.Empty;
		//      ShapeConnectionInfo endPtCi = ShapeConnectionInfo.Empty;
		//      foreach (ShapeConnectionInfo sci in GetConnectionInfos(ControlPointId.Any, null)) {
		//         if (sci.OtherPointId == ControlPointId.Reference) {
		//            if (IsFirstVertex(sci.OwnPointId)) startPtCi = sci;
		//            else if (IsLastVertex(sci.OwnPointId)) endPtCi = sci;
		//         }
		//      }
		//      if (!startPtCi.IsEmpty && !endPtCi.IsEmpty) {
		//         Point oldSPos = GetControlPointPosition(ControlPointId.FirstVertex);
		//         Point oldEPos = GetControlPointPosition(ControlPointId.FirstVertex);
		//         Point newSPos = endPtCi.OtherShape.CalculateConnectionFoot(startPtCi.OtherShape.X, startPtCi.OtherShape.Y);
		//         Point newEPos = startPtCi.OtherShape.CalculateConnectionFoot(endPtCi.OtherShape.X, endPtCi.OtherShape.Y);
		//         MovePointByCore(ControlPointId.FirstVertex, newSPos.X - oldSPos.X, newSPos.Y - oldSPos.Y, ResizeModifiers.None);
		//         MovePointByCore(ControlPointId.LastVertex, newEPos.X - oldEPos.X, newEPos.Y - oldEPos.Y, ResizeModifiers.None);
		//         InvalidateDrawCache();
		//         Invalidate();
		//         handled = true;
		//      }
		//   }
		//   if (!handled) base.FollowConnectionPointWithGluePoint(gluePointId, connectedShape, movedPointId);
		//}
		
		
		protected override Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			// Get the second point of the line segment that should intersect with the passive shape's outline
			ControlPointId secondPtId = gluePointId;
			if (IsFirstVertex(gluePointId))
				secondPtId = GetNextVertexId(gluePointId);
			if (IsLastVertex(gluePointId))
				secondPtId = GetPreviousVertexId(gluePointId);
			// calculate new GluePoiontPosition of the moved GluePoint by calculating the intersection point
			// of the passive shape's outline with the line segment from GluePoint to NextPoint/PrevPoint of GluePoint
			Point secondPtPos = GetControlPointPosition(secondPtId);
			return CalcGluePointFromPosition(gluePointId, shape, secondPtPos.X, secondPtPos.Y);
		}


		protected void GetSegmentCoordinates(int segmentIndex, out int startPointX, out int startPointY, out int endPointX, out int endPointY) {
			startPointX = startPointY = endPointX = endPointY = -1;
			if (segmentIndex < 0 || segmentIndex >= VertexCount - 1) throw new IndexOutOfRangeException();
			startPointX = vertices[segmentIndex].X;
			startPointY = vertices[segmentIndex].Y;
			endPointX = vertices[segmentIndex + 1].X;
			endPointY = vertices[segmentIndex + 1].Y;
		}


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
					if (p != Geometry.InvalidPointF && Geometry.LineContainsPoint(p.X, p.Y, vertices[i].X, vertices[i].Y, vertices[j].X, vertices[j].Y, false))
						result = Geometry.RadiansToDegrees(Geometry.Angle(vertices[capPtIdx].X, vertices[capPtIdx].Y, p.X, p.Y));
					i = i + step;
				}
			}
			if (float.IsNaN(result))
				result= Geometry.RadiansToDegrees(Geometry.Angle(vertices[capPtIdx].X, vertices[capPtIdx].Y, vertices[otherPtIdx].X, vertices[otherPtIdx].Y));
			Debug.Assert(!float.IsNaN(result));
			return result;
		}
		
		
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


		/// <summary>
		/// Retrieve a new PointId for a new point
		/// </summary>
		/// <returns></returns>
		private ControlPointId GetNewControlPointId() {
			for (int id = 1; id <= VertexCount; ++id) {
				if (pointIds.IndexOf(id) < 0)
					return id;
			}
			return VertexCount + 1;
		}

	}


	/// <summary>
	/// Abstract base class for circular arcs.
	/// </summary>
	public abstract class CircularArcBase : LineShapeBase {

		#region Shape Members

		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is CircularArcBase) {
				// Vertices and CapStyles will be copied by the base class
				// so there's nothing left to do here...
			}
		}


		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			Point result = Point.Empty;
			if (IsLine) result = Geometry.VectorLinearInterpolation(StartPoint, EndPoint, relativePosition.A / 1000f);
			else {
				float arcLength = Radius * Geometry.DegreesToRadians(SweepAngle);
				float angleToPt = Geometry.RadiansToDegrees((arcLength * relativePosition.A / 1000f) / Radius);
				result = Point.Round(Geometry.CalcPoint(Center.X, Center.Y, StartAngle + angleToPt, Radius));
			}
			return result;
		}


		public override RelativePosition CalculateRelativePosition(int x, int y) {
			// The relative Position of an arc is defined by...
			// A: Tenths of percentage of the distance between StartPoint and EndPoint
			// B: not used
			RelativePosition result = RelativePosition.Empty;
			if (IsLine) {
				float length = Geometry.DistancePointPoint(StartPoint, EndPoint);
				float distToPt = Geometry.DistancePointPoint(StartPoint.X, StartPoint.Y, x, y);
				result.A = (int)Math.Round((distToPt / (length / 100)) * 10);
				result.B = 0;	// not used
			} else {
				float angleToPt = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, StartPoint.X, StartPoint.Y, x, y));
				if (angleToPt < 0) angleToPt += 360;
				if (SweepAngle < 0) angleToPt += SweepAngle;
				result.A = (int)Math.Round((angleToPt / (SweepAngle / 100f)) * 10);
				result.B = 0; // not used
			}
			return result;
		}


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
				float angleDeg = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, point.X, point.Y));
				result = Geometry.CalcPoint(point.X, point.Y, angleDeg, 100);
			}
			return result;
		}


		public override Point CalculateConnectionFoot(int fromX, int fromY) {
			// TODO 2: Implement a better approach:
			Point result = Point.Empty;
			result.X = X;
			result.Y = Y;
			return result;
		}


		public override Point CalculateConnectionFoot(int x1, int y1, int x2, int y2) {
			Point result = Point.Empty;
			Point linePt = Geometry.GetFurthestPoint((int)Math.Round(Center.X), (int)Math.Round(Center.Y), x1, y1, x2, y2);
			Point intersectionPt = Geometry.GetNearestPoint(linePt, Geometry.IntersectArcLine(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, x1, y1, x2, y2, false));
			if (intersectionPt != Geometry.InvalidPoint)
				return intersectionPt;
			else
				return Geometry.GetNearestPoint(linePt, Geometry.CalcArcTangentThroughPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, linePt.X, linePt.Y));
		}


		public override IEnumerable<Point> CalculateCells(int cellSize) {
			return base.CalculateCells(cellSize);
		}


		public override IEnumerable<DiagrammingAction> GetActions(int mouseX, int mouseY, int range) {
			// return actions of base class
			IEnumerator<DiagrammingAction> enumerator = GetBaseActions(mouseX, mouseY, range);
			while (enumerator.MoveNext()) yield return enumerator.Current;
			// return own actions
			ControlPointId clickedPointId = FindNearestControlPoint(mouseX, mouseY, range, ControlPointCapabilities.Resize);

			bool isFeasible;
			string description;

			isFeasible = (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference) && ContainsPoint(mouseX, mouseY) && VertexCount < 3;
			description = "You have to click on the line in order to insert new points";
			yield return new CommandAction("Insert Point", null, description, isFeasible,
				new InsertVertexCommand(this, mouseX, mouseY));

			isFeasible = !HasControlPointCapability(clickedPointId, ControlPointCapabilities.Glue) && VertexCount > 2;
			if (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference)
				description = "No control point was clicked";
			else description = "Glue control points may not be removed.";
			yield return new CommandAction("RemoveRange Point", null, description, isFeasible,
				new RemoveVertexCommand(this, range));
		}


		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			if (IsLine) {
				if (Geometry.DistancePointLine(x, y, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, true) <= range) {
					if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability))
						return ControlPointId.Reference;
				}
			} else {
				float lineContainsDelta = (LineStyle.LineWidth / 2f) + 2;
				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, lineContainsDelta, x, y)) {
					if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability))
						return ControlPointId.Reference;
				}
			}
			for (int i = 0; i < VertexCount; ++i) {
				if (Geometry.DistancePointPoint(x, y, vertices[i].X, vertices[i].Y) <= range) {
					ControlPointId ptId = GetControlPointId(i);
					if (HasControlPointCapability(ptId, controlPointCapability)) return ptId;
				}
			}
			return ControlPointId.None;
		}


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


		public override int X {
			get { return vertices[0].X; }
			set {
				int origValue = vertices[0].X;
				if (!MoveTo(value, Y)) MoveTo(origValue, Y);
			}
		}


		public override int Y {
			get { return vertices[0].Y; }
			set {
				int origValue = vertices[0].Y;
				if (!MoveTo(X, value)) MoveTo(X, origValue);
			}
		}


		public override int MinVertexCount { get { return 2; } }


		public override int MaxVertexCount { get { return 3; } }


		public override ControlPointId InsertVertex(ControlPointId beforePointId, int x, int y) {
			int newPointId = ControlPointId.None;
			if (IsFirstVertex(beforePointId) || beforePointId == ControlPointId.Reference || beforePointId == ControlPointId.None)
				throw new DiagrammingException("{0} is not a valid {1} for this operation.", beforePointId, typeof(ControlPointId).Name);
			if (VertexCount >= MaxVertexCount) throw new DiagrammingException("Numeric of maximum vertices reached.");

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


		public override ControlPointId AddVertex(int x, int y) {
			if (VertexCount >= MaxVertexCount) throw new InvalidOperationException("Number of maximum vertices reached.");
			return InsertVertex(ControlPointId.LastVertex, x, y);
		}


		public override void RemoveVertex(ControlPointId controlPointId) {
			if (IsFirstVertex(controlPointId) || IsLastVertex(controlPointId))
				throw new InvalidOperationException("Start- and end pioints of linear shapes cannot be removed.");
			int controlPointIndex = GetControlPointIndex(controlPointId);
			vertices.RemoveAt(controlPointIndex);
			InvalidateDrawCache();
		}


		public override void Invalidate() {
			if (DisplayService != null) {
				base.Invalidate();

				int margin = 1;
				if (LineStyle != null) margin = (int)Math.Ceiling(LineStyle.LineWidth / 2f) + 1;
				//DisplayService.Invalidate((int)Math.Floor(arcBounds.X - margin), (int)Math.Floor(arcBounds.Y - margin), (int) Math.Ceiling(arcBounds.Width + margin + margin), (int)Math.Ceiling(arcBounds.Height + margin + margin));

				int left, right, top, bottom;
				if (VertexCount == 2) {
					left = Math.Min(StartPoint.X, EndPoint.X);
					right = Math.Max(StartPoint.X, EndPoint.X);
					top = Math.Min(StartPoint.Y, EndPoint.Y);
					bottom = Math.Max(StartPoint.Y, EndPoint.Y);
				} else {
					left = Math.Min(StartPoint.X, EndPoint.X);
					right = Math.Max(StartPoint.X, EndPoint.X);
					top = Math.Min(StartPoint.Y, EndPoint.Y);
					bottom = Math.Max(StartPoint.Y, EndPoint.Y);
					if (!float.IsNaN(arcBounds.Left))
						left = (int)Math.Floor(arcBounds.Left);
					if (!float.IsNaN(arcBounds.Top))
						top = (int)Math.Floor(arcBounds.Top);
					if (!float.IsNaN(arcBounds.Right))
						right = (int)Math.Ceiling(arcBounds.Right);
					if (!float.IsNaN(arcBounds.Bottom))
						bottom = (int)Math.Ceiling(arcBounds.Bottom);
				}

				DisplayService.Invalidate(left - margin, top - margin, right - left + margin + margin, bottom - top + margin + margin);
				DisplayService.Invalidate(StartPoint.X - 100, StartPoint.Y - 100, 200, 200);
				DisplayService.Invalidate(EndPoint.X - 100, EndPoint.Y - 100, 200, 200);
			}
		}


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

				#region Draw debug infos
#if DEBUG
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

				//// Draw line cap bounds
				//if (StartCapBounds != Geometry.InvalidRectangle)
				//   graphics.DrawRectangle(Pens.Red, StartCapBounds);
				//if (EndCapBounds != Geometry.InvalidRectangle)
				//   graphics.DrawRectangle(Pens.Red, EndCapBounds);

				#region Visualize absolute/relative positions

				//foreach (ShapeConnectionInfo ci in GetConnectionInfos(ControlPointId.Reference, null)) {
				//   Point pt = ci.OtherShape.GetControlPointPosition(ci.OtherPointId);
				//   float angleDeg = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, StartPoint.X, StartPoint.Y, pt.X, pt.Y));
				//   GdiHelpers.DrawAngle(graphics, Brushes.Red, Center, arcStartAngle + angleDeg, (int)(Radius / 2));
				//}

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

				//// Draw angle to point 0 and shapeAngle to point 2
				//if (!IsLine) {
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
#endif
				#endregion
			}
			base.Draw(graphics);
		}


		public override void DrawOutline(Graphics graphics, Pen pen) {
			base.DrawOutline(graphics, pen);
			if (IsLine) graphics.DrawLine(pen, StartPoint, EndPoint);
			else {
				Debug.Assert(arcBounds != Geometry.InvalidRectangleF);
				Debug.Assert(!float.IsNaN(StartAngle));
				Debug.Assert(!float.IsNaN(SweepAngle));
				graphics.DrawArc(pen, arcBounds, StartAngle, SweepAngle);
			}
		}


		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			if (image == null) throw new ArgumentNullException("image");
			using (Graphics g = Graphics.FromImage(image)) {
				GdiHelpers.ApplyGraphicsSettings(g, DiagrammingRenderingQuality.MaximumQuality);
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

		protected internal CircularArcBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal CircularArcBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			for (int i = vertices.Count - 1; i >= 0; --i)
				vertices[i] = Point.Empty;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// Calcualte line caps' bounds
			Rectangle capBounds = base.CalculateBoundingRectangle(tight);
			Rectangle result = Rectangle.Empty;
			float halfLineWidth = LineStyle.LineWidth / 2f;
			if (IsLine) {
				result.X = (int)Math.Floor(Math.Min(StartPoint.X, EndPoint.X) - halfLineWidth);
				result.Y = (int)Math.Floor(Math.Min(StartPoint.Y, EndPoint.Y) - halfLineWidth);
				result.Width = (int)Math.Ceiling(Math.Max(StartPoint.X, EndPoint.X) + halfLineWidth) - result.X;
				result.Height = (int)Math.Ceiling(Math.Max(StartPoint.Y, EndPoint.Y) + halfLineWidth) - result.Y;
			} else {
				result.X = (int)Math.Floor(Center.X - Radius - halfLineWidth);
				result.Y = (int)Math.Floor(Center.Y - Radius - halfLineWidth);
				result.Width = (int)Math.Ceiling(Radius + Radius + halfLineWidth);
				result.Height = (int)Math.Ceiling(Radius + Radius + halfLineWidth);
			}
			if (!capBounds.IsEmpty) result = Geometry.UniteRectangles(result, capBounds);
			return result;
		}
		
		
		protected override bool ContainsPointCore(int x, int y) {
			if (base.ContainsPointCore(x, y))
				return true;
			float lineContainsDelta = (int)Math.Ceiling(LineStyle.LineWidth / 2f) + 2;
			if (IsLine) return Geometry.LineContainsPoint(x, y, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, lineContainsDelta, true);
			else {
				if (Center != Geometry.InvalidPointF)
					return Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, lineContainsDelta, x, y);
				else return Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, lineContainsDelta, x, y);
			}
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			Rectangle rectangle = Rectangle.Empty;
			rectangle.X = x;
			rectangle.Y = y;
			rectangle.Width = width;
			rectangle.Height = height;
			if (IsLine) {
				if (Geometry.RectangleIntersectsWithLine(rectangle, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, true))
					return true;
			} else {
				if (Geometry.ArcIntersectsWithRectangle(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, rectangle))
					return true;
			}
			return false;
		}


		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			int pointIndex = GetControlPointIndex(pointId);

			// calc shapeAngle between StartPoint and RadiusPoint
			float angle = 0;
			if (!IsLine && (IsFirstVertex(pointId) || IsLastVertex(pointId)))
				angle = Geometry.RadiansToDegrees(Geometry.Angle(StartPoint, EndPoint, RadiusPoint));

			Point p = vertices[pointIndex];
			p.Offset(deltaX, deltaY);
			vertices[pointIndex] = p;

			if (!IsLine && (pointIndex == 0 || pointIndex == 2)) {
				//vertices[1].X = (int)Math.Round(vertices[1].X + (deltaX / 2f));
				//vertices[1].Y = (int)Math.Round(vertices[1].Y + (deltaY / 2f));

				if ((modifiers & ResizeModifiers.MaintainAspect) == ResizeModifiers.MaintainAspect) {
					int hX = EndPoint.X;
					int hY = EndPoint.Y;
					Geometry.RotatePoint(StartPoint.X, StartPoint.Y, angle, ref hX, ref hY);

					int aPb, bPb, cPb; // perpendicular bisector
					int aR, bR, cR;	// line through start point and radius point
					Geometry.CalcPerpendicularBisector(StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, out aPb, out bPb, out cPb);
					Geometry.CalcLine(StartPoint.X, StartPoint.Y, hX, hY, out aR, out bR, out cR);
					//int x, y;
					//Geometry.SolveLinear22System(aPb, bPb, aR, bR, cPb, cR, out x, out y);
					//vertices[1].X = x;
					//vertices[1].Y = y;
					Point newPos = Geometry.IntersectLines(aPb, bPb, cPb, aR, bR, cR);
					Debug.Assert(newPos != Geometry.InvalidPoint);
					vertices[1] = newPos;
				}
			}
			return true;
		}


		protected override bool MoveByCore(int deltaX, int deltaY) {
			// move cap bounds and cap points			
			if (base.MoveByCore(deltaX, deltaY)) {
				if (center != Geometry.InvalidPointF) {
					center.X += deltaX;
					center.Y += deltaY;
				}
				return true;
			} else {
				InvalidateDrawCache();
				return false;
			}
		}


		protected override Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			Point ptPos = Geometry.InvalidPoint;
			if (!IsLine) {
				// Calculate desired arc: 
				// Start point, end point, center and radius
				ShapeConnectionInfo startCi = GetConnectionInfo(ControlPointId.FirstVertex, null);
				ShapeConnectionInfo endCi = GetConnectionInfo(ControlPointId.FirstVertex, null);
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
				int aT, bT, cT;
				Geometry.TranslateLine((int)Math.Round(a), (int)Math.Round(b), (int)Math.Round(c), tangentPt, out aT, out bT, out cT);
				//
				// Calculate intersection point of the calculated tangent and the perpendicular bisector 
				// of the line through startPt and endPt
				Geometry.CalcPerpendicularBisector(startPt.X, startPt.Y, endPt.X, endPt.Y, out a, out b, out c);
				PointF pT = Geometry.IntersectLines(aT, bT, cT, a, b, c);
				if (pT != Geometry.InvalidPointF) {
					PointF pB = Geometry.VectorLinearInterpolation(startPt, endPt, 0.5);
					ptPos = Point.Round(Geometry.VectorLinearInterpolation(pB, pT, 0.75));
				}
			}
			// If the arc only has 2 points or something went wrong while calculating the desired arc
			if (ptPos == Geometry.InvalidPoint) 
				ptPos = Geometry.VectorLinearInterpolation(StartPoint, EndPoint, 0.5d);
			return CalcGluePointFromPosition(gluePointId, shape, ptPos.X, ptPos.Y);
		}


		protected override float CalcCapAngle(ControlPointId pointId) {
			if (pointId == ControlPointId.FirstVertex)
				return CalcCapAngle(GetControlPointIndex(pointId), StartCapStyleInternal.CapSize);
			else if (pointId == ControlPointId.LastVertex)
				return CalcCapAngle(GetControlPointIndex(pointId), EndCapStyleInternal.CapSize);
			else throw new NotSupportedException();
		}


		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			center = Geometry.InvalidPointF;
			radius = float.NaN;
			arcStartAngle = float.NaN;
			arcSweepAngle = float.NaN;
			arcBounds = Geometry.InvalidRectangleF;
		}


		protected override void RecalcDrawCache() {
			// calculate the position of the bow's circle center point
			if (shapePoints.Length != vertices.Count) Array.Resize<Point>(ref shapePoints, vertices.Count);
			for (int i = vertices.Count - 1; i >= 0; --i) {
				Point p = vertices[i];
				p.Offset(-X, -Y);
				shapePoints[i] = p;
			}
			if (!IsLine) {
				// Calculate center point and radius
				CalculateAngles(out arcStartAngle, out arcSweepAngle);
				// Calculate boundingRectangle of the arc (required for drawing and invalidating)
				arcBounds.X = Center.X - X - Radius;
				arcBounds.Y = Center.Y - Y - Radius;
				arcBounds.Width = arcBounds.Height = Math.Max(0.1f, Radius + Radius);
			}
			base.RecalcDrawCache();
		}


		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			Matrix.TransformPoints(shapePoints);
			if (arcBounds != Geometry.InvalidRectangleF)
				arcBounds.Offset(deltaX, deltaY);
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
					return Geometry.VectorLinearInterpolation(StartPoint, EndPoint, 0.5);
				else if (VertexCount == 3) return vertices[1];
				else throw new IndexOutOfRangeException();
			}
		}


		private PointF Center {
			get {
				if (center == Geometry.InvalidPointF) {
					if (IsLine)
						center = Geometry.VectorLinearInterpolation((PointF)StartPoint, (PointF)EndPoint, 0.5);
					else
						center = Geometry.CalcArcCenterAndRadius(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, out radius);
				}
				return center;
			}
		}


		private float StartAngle {
			get {
				if (float.IsNaN(arcStartAngle)) CalculateAngles(out arcStartAngle, out arcSweepAngle);
				return arcStartAngle;
			}
		}


		private float SweepAngle {
			get {
				if (float.IsNaN(arcSweepAngle)) CalculateAngles(out arcStartAngle, out arcSweepAngle);
				return arcSweepAngle;
			}
		}


		private float Radius {
			get {
				if (float.IsNaN(radius)) {
					if (IsLine) radius = float.PositiveInfinity;
					else
						center = Geometry.CalcArcCenterAndRadius(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, out radius);
				}
				return radius;
			}
		}


		private bool IsLine {
			get {
				return (vertices.Count == 2
					|| Geometry.LineContainsPoint(RadiusPoint.X, RadiusPoint.Y, StartPoint.X, StartPoint.Y, EndPoint.X, EndPoint.Y, false));
			}
		}

		#endregion


		#region [Private] Methods

		private void Construct() {
			InvalidateDrawCache();
		}


		private IEnumerator<DiagrammingAction> GetBaseActions(int mouseX, int mouseY, int range) {
			return base.GetActions(mouseX, mouseY, range).GetEnumerator();
		}


		private bool CalculateAngles(out float startAngle, out float sweepAngle) {
			startAngle = sweepAngle = float.NaN;

			// Calculate vertex angles
			float p0Angle = Geometry.Angle(Center.X, Center.Y, StartPoint.X, StartPoint.Y);
			float p1Angle = Geometry.Angle(Center.X, Center.Y, RadiusPoint.X, RadiusPoint.Y);
			float p2Angle = Geometry.Angle(Center.X, Center.Y, EndPoint.X, EndPoint.Y);
			
			// Sort vertices in order to calculate start- and sweep angle
			int startPointIdx, endPointIdx;
			if (p0Angle >= 0 && p1Angle >= 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 1: All angles positive
				//
				if (p0Angle < p1Angle && p1Angle < p2Angle) {
					startPointIdx = 0;
					endPointIdx = 2;
				} else if (p0Angle >= p1Angle && p1Angle >= p2Angle) {
					startPointIdx = 2;
					endPointIdx = 0;
				} else {
					if (p0Angle < p2Angle) {
						startPointIdx = 2;
						endPointIdx = 0;
					} else {
						startPointIdx = 0;
						endPointIdx = 2;
					}
				}
			} else if (p0Angle <= 0 && p1Angle <= 0 && p2Angle <= 0) {
				//===============================================================================
				// Case 2: All angles negative
				//
				if (p0Angle < p1Angle && p1Angle < p2Angle) {
					startPointIdx = 0;
					endPointIdx = 2;
				} else if (p0Angle >= p1Angle && p1Angle >= p2Angle) {
					startPointIdx = 2;
					endPointIdx = 0;
				} else {
					if (p0Angle > p2Angle) {
						startPointIdx = 0;
						endPointIdx = 2;
					} else {
						startPointIdx = 2;
						endPointIdx = 0;
					}
				}
			} else if (p0Angle >= 0 && p1Angle >= 0 && p2Angle < 0) {
				//===============================================================================
				// Case 3: StartPoint's angle positive, RadiusPoint's angle positive, EndPoint's angle negative
				//
				if (p0Angle < p1Angle) {
					startPointIdx = 0;
					endPointIdx = 2;
				} else {
					startPointIdx = 2;
					endPointIdx = 0;
				}
			} else if (p0Angle >= 0 && p1Angle < 0 && p2Angle < 0) {
				//===============================================================================
				// Case 4: StartPoint's angle positive, RadiusPoint's angle negative, EndPoint's angle negative
				//
				if (p1Angle < p2Angle) {
					startPointIdx = 0;
					endPointIdx = 2;
				} else {
					startPointIdx = 2;
					endPointIdx = 0;
				}
			} else if (p0Angle < 0 && p1Angle < 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 5: StartPoint's angle negative, RadiusPoint's angle negative, EndPoint's angle positive
				//
				if (p0Angle < p1Angle) {
					startPointIdx = 0;
					endPointIdx = 2;
				} else {
					startPointIdx = 2;
					endPointIdx = 0;
				}
			} else if (p0Angle < 0 && p1Angle >= 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 6: StartPoint's angle negative, RadiusPoint's angle positive, EndPoint's angle positive
				//
				if (p1Angle < p2Angle) {
					startPointIdx = 0;
					endPointIdx = 2;
				} else {
					startPointIdx = 2;
					endPointIdx = 0;
				}
			} else if (p0Angle >= 0 && p1Angle < 0 && p2Angle >= 0) {
				//===============================================================================
				// Case 7: StartPoint's angle positive, RadiusPoint's angle negative, EndPoint's angle positive
				//
				if (p0Angle < p2Angle) {
					startPointIdx = 2;
					endPointIdx = 0;
				} else {
					startPointIdx = 0;
					endPointIdx = 2;
				}
			} else if (p0Angle < 0 && p1Angle >= 0 && p2Angle < 0) {
				//===============================================================================
				// Case 8: StartPoint's angle negative, RadiusPoint's angle positive, EndPoint's angle negative
				//
				if (p0Angle < p2Angle) {
					startPointIdx = 2;
					endPointIdx = 0;
				} else {
					startPointIdx = 0;
					endPointIdx = 2;
				}
			} else if (float.IsNaN(p0Angle) && float.IsNaN(p1Angle) && float.IsNaN(p2Angle)) {
				//===============================================================================
				// Case 9: No Solution: Arc is not defined
				//
				startPointIdx = 0;
				endPointIdx = 2;
				return false;
			} else throw new DiagrammingInternalException("Unable to calculate drawCache.");

			// calculate angles
			sweepAngle = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, vertices[startPointIdx].X, vertices[startPointIdx].Y, vertices[endPointIdx].X, vertices[endPointIdx].Y));
			if (sweepAngle < 0) sweepAngle = (360 + sweepAngle) % 360;
			if (startPointIdx == 0) {
				startAngle = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, vertices[startPointIdx].X, vertices[startPointIdx].Y));
				if (startAngle < 0) startAngle = (360 + startAngle) % 360;
			} else {
				// if StartPoint and EndPoint were swapped, invert the sweepAngle - otherwise the line cap will be drawn on the wrong side.
				startAngle = Geometry.RadiansToDegrees(Geometry.Angle(Center.X, Center.Y, vertices[endPointIdx].X, vertices[endPointIdx].Y));
				sweepAngle = -sweepAngle;
			}
			return true;
		}


		private float CalcCapAngle(int pointIndex, float capSize) {
			if (Geometry.DistancePointPoint(StartPoint, EndPoint) < capSize)
				capSize = Geometry.DistancePointPoint(StartPoint, EndPoint);
			foreach (PointF p in Geometry.IntersectCircleArc((float)vertices[pointIndex].X, (float)vertices[pointIndex].Y, capSize, StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius)) {
				if (Geometry.ArcContainsPoint(StartPoint.X, StartPoint.Y, RadiusPoint.X, RadiusPoint.Y, EndPoint.X, EndPoint.Y, Center.X, Center.Y, Radius, 1, p.X, p.Y)) {
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
			//   //throw new DiagrammingInternalException("Angle of line cap could not be calculated.");
			//   angle = 0;
			//}
			//return angle;
		}

		#endregion


		#region Fields

		// Property buffers
		private PointF center = Geometry.InvalidPointF;
		private float radius = float.NaN;

		// Draw cache
		private float arcStartAngle = float.NaN;
		private float arcSweepAngle = float.NaN;
		private RectangleF arcBounds = Geometry.InvalidRectangleF;
		
		#endregion
	}

}
