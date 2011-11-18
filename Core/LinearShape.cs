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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
		/// Inserts a new control point to the shape before the control point with the 
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
		[Obsolete]
		Point CalculateConnectionFoot(int x1, int y1, int x2, int y2);

		/// <summary>Calculates the normal vector for the given point.</summary>
		Point CalcNormalVector(Point point);

		/// <summary>Indicates if the line has a direction, e.g. a cap on one side.</summary>
		bool IsDirected { get; }

	}


	/// <summary>
	/// One-dimensional shape with optional caps on both ends defined by a sequence of vertices
	/// </summary>
	/// <remarks>RequiredPermissions set</remarks>
	public abstract class LineShapeBase : ShapeBase, ILinearShape {

		#region [Public] Shape Members

		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is LineShapeBase) {
				LineShapeBase sourceLine = (LineShapeBase)source;
				// Copy templated properties
				ICapStyle capStyle;
				capStyle = sourceLine.StartCapStyleInternal;
				privateStartCapStyle = (Template != null && capStyle == ((LineShapeBase)Template.Shape).StartCapStyleInternal) ? null : capStyle;

				capStyle = sourceLine.EndCapStyleInternal;
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
		[Obsolete]
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
				// Notify the owner that the bounds of the line could have been changed 
				// due to a changed line width (we cannot detect this kind of change, 
				// so we notify the owner in order to be on the save side)
				if (Owner != null) {
					Owner.NotifyChildResizing(this);
					Owner.NotifyChildResized(this);
				}
				Invalidate();
				result = true;
			}
			return result;
		}


		/// <override></override>
		public override void Invalidate() {
			base.Invalidate();
			if (DisplayService != null) {
				if (IsShapedLineCap(StartCapStyleInternal))
					DisplayService.Invalidate(StartCapBounds);
				if (IsShapedLineCap(EndCapStyleInternal))
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
			if (controlPointId == ControlPointId.None || controlPointId == ControlPointId.Any)
				return base.HasControlPointCapability(controlPointId, controlPointCapability);
			else if (controlPointId == ControlPointId.Reference) {
				return (((controlPointCapability & ControlPointCapabilities.Reference) > 0)
					|| (controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
			} else if (IsFirstVertex(controlPointId)) {
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0
					|| (controlPointCapability & ControlPointCapabilities.Reference) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			} else if (IsLastVertex(controlPointId)) {
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			} else {
				int pointIdx = GetControlPointIndex(controlPointId);
				if ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId))
					return true;
				if (pointIdx >= 0 && pointIdx < ControlPointCount)
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
			}
			return false;
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

			isFeasible = ContainsPoint(mouseX, mouseY) && (clickedPointId == ControlPointId.None || clickedPointId == ControlPointId.Reference);
			description = "You have to click on the line in order to insert new points";
			if (VertexCount >= MaxVertexCount) {
				isFeasible = false;
				description = "The line already has the maximum number of vertices";
			}
			yield return new CommandMenuItemDef("Insert Vertex", null, description, isFeasible,
				new AddVertexCommand(this, mouseX, mouseY));

			isFeasible = false;
			if (HasControlPointCapability(clickedPointId, ControlPointCapabilities.Resize)) {
				if (!HasControlPointCapability(clickedPointId, ControlPointCapabilities.Glue)) {
					if ((clickedPointId != ControlPointId.None && IsConnected(clickedPointId, null) == ControlPointId.None)) {
						if (VertexCount > MinVertexCount)
							isFeasible = true;
						else description = "Minimum vertex count reached";
					} else description = "Control point is connected";
				} else description = "Glue control points may not be removed";
			} else description = "No resize point was clicked";
			yield return new CommandMenuItemDef("Remove Vertex", null, description, isFeasible,
				new RemoveVertexCommand(this, clickedPointId));
		}


		#endregion


		#region [Public] IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.LineShapeBase" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			// Initialize definitions for inner objects
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
				// Save vertices
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


		#region [Public] ILinearShape Members

		/// <override></override>
		public abstract ControlPointId InsertVertex(ControlPointId beforePointId, int x, int y);


		/// <override></override>
		public abstract ControlPointId AddVertex(int x, int y);


		/// <override></override>
		public abstract void RemoveVertex(ControlPointId controlPointId);


		/// <override></override>
		public abstract Point CalcNormalVector(Point p);


		/// <override></override>
		[Browsable(false)]
		public abstract int MinVertexCount { get; }


		/// <override></override>
		[Browsable(false)]
		public abstract int MaxVertexCount { get; }


		/// <override></override>
		[Browsable(false)]
		public virtual int VertexCount {
			get { return vertices.Count; }
		}


		/// <summary>
		/// Retrieve the id of the next neighbor vertex of pointId in physical order "start to end"
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
		/// Retrieve the id of the previous neighbor vertex of pointId in physical order "end to start"
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
		[Browsable(false)]
		public bool IsDirected {
			get {
				return (IsShapedLineCap(StartCapStyleInternal))
					|| (IsShapedLineCap(EndCapStyleInternal));
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
					if (IsShapedLineCap(StartCapStyleInternal))
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
					if (IsShapedLineCap(EndCapStyleInternal))
						endCapBounds = ToolCache.GetCapBounds(EndCapStyleInternal, LineStyle, EndCapAngle);
					else endCapBounds = Rectangle.Empty;
					endCapBounds.Offset(vertices[GetControlPointIndex(ControlPointId.LastVertex)]);
				}
				return endCapBounds;
			}
		}

		#endregion


		#region [Protected Internal] Methods (Inherited)

		/// <summary>
		/// Protetced internal constructur. Should only be called by the <see cref="T:Dataweb.NShape.Advanced.ShapeType" />'s <see cref="T:Dataweb.NShape.Advanced.CreateShapeDelegate" />
		/// </summary>
		protected internal LineShapeBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			Construct();
		}



		/// <summary>
		/// Protetced internal constructor. Should only be called by the <see cref="T:Dataweb.NShape.Advanced.ShapeType" />'s <see cref="T:Dataweb.NShape.Advanced.CreateShapeDelegate" />
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



		#endregion


		#region [Protected] Methods (Inherited)

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
			if (IsShapedLineCap(StartCapStyleInternal))
				result = Geometry.UniteRectangles(result, StartCapBounds);
			if (IsShapedLineCap(EndCapStyleInternal))
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
			if (IsShapedLineCap(StartCapStyleInternal))
				if (StartCapContainsPoint(x, y)) return true;
			if (IsShapedLineCap(EndCapStyleInternal))
				if (EndCapContainsPoint(x, y)) return true;
			return false;
		}


		/// <summary>
		/// Retrieve ControlPoint's index in the list of point id's (in physical order)
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
			if (IsShapedLineCap(StartCapStyleInternal)) {
				ptIdx = GetControlPointIndex(ControlPointId.FirstVertex);
				// get untransfomed cap points and transform it to the start point (relative to origin of coordinates)
				ToolCache.GetCapPoints(StartCapStyleInternal, LineStyle, ref startCapPointsBuffer);
				TransformCapToOrigin(shapePoints[ptIdx].X, shapePoints[ptIdx].Y, StartCapAngle, ref startCapPointsBuffer);
			}
			if (IsShapedLineCap(EndCapStyleInternal)) {
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
		/// Returns true if the line cap is a shaped and sizable line cap.
		/// </summary>
		protected bool IsShapedLineCap(ICapStyle capStyle) {
			if (capStyle == null) return false;
			switch (capStyle.CapShape) {
				case CapShape.None:
					return false;
				case CapShape.ArrowClosed:
				case CapShape.ArrowOpen:
				case CapShape.CenteredCircle:
				case CapShape.CenteredHalfCircle:
				case CapShape.Circle:
				case CapShape.Diamond:
				case CapShape.Square:
				case CapShape.Triangle:
					return true;
				default:
					throw new NShapeUnsupportedValueException(capStyle.CapShape);
			}
		}


		/// <summary>
		/// Performs an intersection test on the LineCap
		/// </summary>
		protected bool StartCapIntersectsWith(Rectangle rectangle) {
			if (IsShapedLineCap(StartCapStyleInternal)) {
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
			if (IsShapedLineCap(EndCapStyleInternal)) {
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
		/// Calculates the line cap angle for the given control point.
		/// </summary>
		/// <param name="pointId"> ControlPointId.FirstVertex or ControlPointId.LastVertex</param>
		/// <remarks>
		/// GDI+ uses the intercsection point of the line cap's border with the line itself to calculate the angle of the cap.
		/// E.g: If a polyline a vertex inside its cap that changes the direction, the intersection point of this segment with 
		/// the line cap's shape is used to calculate the angle. 
		/// Therefore we cannot use normal- or tangent vectors for cap angle calculation.
		/// </remarks>
		/// <returns>Line cap angle in degrees</returns>
		protected abstract float CalcCapAngle(ControlPointId pointId);


		/// <summary>
		/// Draws the line's StartCap
		/// </summary>
		protected void DrawStartCapBackground(Graphics graphics, int pointX, int pointY) {
			if (IsShapedLineCap(StartCapStyleInternal)) {
				Brush capBrush = ToolCache.GetBrush(StartCapStyleInternal.ColorStyle, LineStyle);
				// ToDo: Find a solution for round caps - perhaps transform the GraphicsPath itself?
				if (startCapPointsBuffer != null && startCapPointsBuffer.Length > 0)
					graphics.FillPolygon(capBrush, startCapPointsBuffer, System.Drawing.Drawing2D.FillMode.Alternate);
			}
		}


		/// <summary>
		/// Draws the line's EndCap
		/// </summary>
		protected void DrawEndCapBackground(Graphics graphics, int pointX, int pointY) {
			if (IsShapedLineCap(EndCapStyleInternal)) {
				Brush capBrush = ToolCache.GetBrush(EndCapStyleInternal.ColorStyle, LineStyle);
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


		/// <summary>
		/// Constructs a new instance.
		/// </summary>
		protected virtual void Construct() {
			vertices = new List<Point>(MinVertexCount);
			pointIds = new List<ControlPointId>(MinVertexCount);
			for (int i = MinVertexCount - 1; i >= 0; --i) {
				vertices.Add(Point.Empty);
				pointIds.Insert(0, i + 1);
			}
			shapePoints = new Point[MinVertexCount];
			InvalidateDrawCache();
		}

		#endregion


		#region [Private] Methods and Properties
		
		/// <summary>
		/// Specifies the tolerance when performing hit tests and intersection calculations
		/// </summary>
		private float ContainsPointDelta {
			get { return 2; }
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

}
