using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

using Dataweb.Diagramming.Core;
using Dataweb.Diagramming.Controllers;


namespace Dataweb.Diagramming.WinFormsUI {

	#region Tool Class

	public abstract class Tool : ITool {

		public string Name { 
			get { return name; } 
		}


		public string Title {
			get { return title; }
			set { title = value; }
		}


		public string Category {
			get { return category; }
			set { category = value; }
		}


		public virtual Cursor Cursor {
			get { return Cursors.Default; }
		}


		public string ToolTipText {
			get { return toolTipText; }
			set { toolTipText = value; }
		}


		public Bitmap SmallIcon {
			get { return smallIcon; }
			set { smallIcon = value; }
		}


		public Bitmap LargeIcon {
			get { return largeIcon; }
			set { largeIcon = value; }
		}


		public event ToolExecutedEventHandler ToolExecuted;

		protected Tool() {
			BaseConstruct();
		}


		protected Tool(string category) {
			this.category = category;
			BaseConstruct();
		}


		private void BaseConstruct(){
			smallIcon = new Bitmap(16, 16);
			largeIcon = new Bitmap(32, 32);
			name = "Tool " + this.GetHashCode().ToString();
			ExecutedEventArgs = new ToolExecutedEventArgs(this, ToolExecutedEventType.Execute);
			CancelledEventArgs = new ToolExecutedEventArgs(this, ToolExecutedEventType.Cancel);
		}


		#region IDisposable Members

		public virtual void Dispose() {
			ClearPreviews();

			if (smallIcon != null) 
				smallIcon.Dispose();
			smallIcon = null;

			if (largeIcon != null) 
				largeIcon.Dispose();
			largeIcon = null;

			outerSnapPen.Dispose();
			innerSnapPen.Dispose();
			shapeContextMenuItem.Dispose();
			modelObjectContextMenuItem.Dispose();
			contextMenuSeparator.Dispose();
		}

		#endregion


		public abstract void Refresh();
		
		
		public virtual bool ExecuteMouseAction(IDisplay display, DiagrammingMouseEventArgs e) {			
			// ToDo: Do not simply cast IDisplay to ownerDisplay. We have to find a way to access all the internal stuff of the ownerDisplay within the Tools without such a 'dirty' typecast from IDisplay to Display...
			currentDisplay = (Display)display;
			lastMousePos = currMousePos;
			currMousePos = e.Position;
			zoomedHandleSizeReal = currentDisplay.HandleSize / (CurrentDisplay.ZoomLevel / 100f);
			zoomedHandleSizeDiagram = Math.Max(1, (int)Math.Round(zoomedHandleSizeReal));

			// get Modifier Keys
			currentModifiers = Modifiers.None;
			if ((Control.ModifierKeys & Keys.Control) != 0)
				currentModifiers |= Modifiers.MaintainAspect;
			if ((Control.ModifierKeys & Keys.Shift) != 0)
				currentModifiers |= Modifiers.MirroredResize;

			if (e.Buttons == DiagrammingMouseButtons.Right && CurrentDisplay.ContextMenuStrip != null) {
				CurrentDisplay.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
				CurrentDisplay.ContextMenuStrip.Closed += ContextMenuStrip_Closed;
			}

			return (CurrentDisplay != null && CurrentDisplay.Diagram != null);
		}


		//public virtual bool ExecuteKeyAction(IDisplay ownerDisplay, KeyEventType eventType, KeyEventArgs e, char keyChar) {
		public virtual bool ExecuteKeyAction(IDisplay display, DiagrammingKeyEventArgs e) {
			CurrentDisplay = (Display)display;
			lastMousePos = currMousePos;

			// get Modifier Keys
			currentModifiers = Modifiers.None;
			if (e.Control)
				currentModifiers &= Modifiers.MaintainAspect;
			if (e.Shift)
				currentModifiers &= Modifiers.MaintainAspect;
			if (e.Alt)
				currentModifiers &= Modifiers.MaintainAspect;

			if (e.KeyCode == (int)Keys.Escape) {
				Cancel();
			}

			return (CurrentDisplay != null && CurrentDisplay.Diagram != null);
		}


		/// <summary>
		/// Sets protected readonly-properties to invalid values and raises the ToolExecuted event.
		/// </summary>
		public virtual void Cancel() {
			if (currentDisplay != null) {
				currentDisplay.Capture = false;
				currentDisplay.Cursor = Cursors.Default;
			}
			currentDisplay = null;

			currMousePos = Point.Empty;
			lastMousePos = Point.Empty;
			currentModifiers = Modifiers.None;
			zoomedHandleSizeReal = 0;

			OnToolExecuted(CancelledEventArgs);
		}


		public abstract void InvalidatePreview();

		
		public abstract void DrawPreview();


		protected Cursor LoadCursorFromResource(byte[] resource) {
			Cursor result = null;
			MemoryStream stream = new MemoryStream(resource, 0, resource.Length, false);
			try {
				result = new Cursor(stream);
			}
			finally {
				stream.Close();
				stream.Dispose();
			}
			return result;
		}


		protected virtual void OnToolExecuted(ToolExecutedEventArgs eventArgs) {
			if (ToolExecuted != null) ToolExecuted(this, eventArgs);
		}
		
		
		protected IDictionary<Shape, Shape> Previews {
			get { return previewShapes; }
		}


		protected Shape FindPreviewShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (!previewShapes.ContainsKey(shape))
				throw new DiagrammingInternalException("This tool has no preview shape for the given {0}.", shape.ShapeType.Name);
			return previewShapes[shape];
		}


		protected Shape FindShape(Shape previewShape) {
			return originalShapes[previewShape];
		}


		protected virtual void AddPreview(Shape shape, Shape previewShape) {
			// Set CurrentDisplay as DisplayService for the preview shapes
			if (previewShape.DisplayService != CurrentDisplay)
				previewShape.DisplayService = CurrentDisplay;
			
			// Add shape and its preview to the appropriate dictionaries
			previewShapes.Add(shape, previewShape);
			originalShapes.Add(previewShape, shape);

			// Add shape's and preview's children to the appropriate dictionaries
			if (previewShape.Children != null) {
				IEnumerator<Shape> previewChildren = previewShape.Children.TopDown.GetEnumerator();
				IEnumerator<Shape> originalChildren = shape.Children.TopDown.GetEnumerator();

				previewChildren.Reset();
				originalChildren.Reset();
				bool processNext = false;
				if (previewChildren.MoveNext() && originalChildren.MoveNext())
					processNext = true;
				while (processNext) {
					AddPreview(originalChildren.Current, previewChildren.Current);
					processNext = (previewChildren.MoveNext() && originalChildren.MoveNext());
				}
			}
		}


		protected void RemovePreviewOf(Shape originalShape) {
			if (previewShapes.ContainsKey(originalShape)) {
				// Invalidate Preview Shape
				Shape previewShape = Previews[originalShape];
				
				if (previewShape.DisplayService != null)
					previewShape.Invalidate();

				// remove previews of the shape and its children from the preview's dictionary
				previewShapes.Remove(originalShape);
				if (originalShape.Children != null) {
					foreach (Shape childShape in originalShape.Children)
						previewShapes.Remove(childShape);
				}
				// remove the shape and its children from the shape's dictionary
				originalShapes.Remove(previewShape);
				if (previewShape.Children != null) {
					foreach (Shape childShape in previewShape.Children)
						originalShapes.Remove(childShape);
				}
			}
		}


		protected void RemovePreview(Shape previewShape) {
			if (!previewShapes.ContainsValue(previewShape))
				throw new DiagrammingInternalException("This preview shape was not created by this tool");
			else {
				// Invalidate Preview Shape
				if (previewShape.DisplayService != null)
					previewShape.Invalidate();

				Shape origShape = originalShapes[previewShape];
				previewShapes.Remove(origShape);
				originalShapes.Remove(previewShape);
			}
		}


		protected virtual void ClearPreviews() {
			foreach (Shape preview in previewShapes.Values) {
				foreach (int gluePointId in preview.GetControlPointIds(ControlPointCapability.Glue))
					preview.Disconnect(gluePointId);

				if (preview.DisplayService != null) {
					preview.Invalidate();
					preview.DisplayService = null;
				}
			}
			previewShapes.Clear();
			originalShapes.Clear();
		}


		protected void ResetZoom() {
			if (displayTransform == null) {
				displayTransform = CurrentDisplay.Graphics.Transform;

				unzoomedTransform.Reset();
				//unzoomedTransform.Translate(displayTransform.OffsetX, displayTransform.OffsetY);
				CurrentDisplay.Graphics.Transform = unzoomedTransform;
			}
			else
				throw new InvalidOperationException();
		}


		protected void RestoreZoom() {
			if (displayTransform != null) {
				CurrentDisplay.Graphics.Transform = displayTransform;
				displayTransform = null;
			}
			else
				throw new InvalidOperationException();
		}


		/// <summary>
		/// Finds the nearest grid SnapPoint in range. If Snapping is disabled for the current ownerDisplay, this function does virtually nothing.
		/// </summary>
		/// <param projectName="ptX">X coordinate</param>
		/// <param projectName="ptY">Y coordinate</param>
		/// <param projectName="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param projectName="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <returns>Bee-line distance to nearest snap point.</returns>
		protected float GetDistanceToNearestSnapPoint(int ptX, int ptY, out int snapDeltaX, out int snapDeltaY) {
			float distance = float.MaxValue;
			snapDeltaX = snapDeltaY = 0;
			if (CurrentDisplay.SnapToGrid) {
				// calculate position of surrounding grid lines
				int gridSize = CurrentDisplay.GridSize;
				int left = ptX - (ptX % gridSize);
				int above = ptY - (ptY % gridSize);
				int right = ptX - (ptX % gridSize) + gridSize;
				int below = ptY - (ptY % gridSize) + gridSize;
				float currDistance = 0;
				int snapDistance = CurrentDisplay.SnapDistance;

				// calculate distance from the given point to the surrounding grid lines
				currDistance = ptY - above;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaY = above - ptY;
				}
				currDistance = right - ptX;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - ptX;
				}
				currDistance = below - ptY;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaY = below - ptY;
				}
				//currDistance = Geometry.CalcDistanceFromLine(ptX, ptY, left, above, left, below);
				currDistance = ptX - left;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - ptX;
				}

				// calculate approximate distance from the given point to the surrounding grid points
				currDistance = Geometry.DistancePointPointFast(ptX, ptY, left, above);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - ptX;
					snapDeltaY = above - ptY;
				}
				currDistance = Geometry.DistancePointPointFast(ptX, ptY, right, above);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - ptX;
					snapDeltaY = above - ptY;
				}
				currDistance = Geometry.DistancePointPointFast(ptX, ptY, left, below);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - ptX;
					snapDeltaY = below - ptY;
				}
				currDistance = Geometry.DistancePointPointFast(ptX, ptY, right, below);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - ptX;
					snapDeltaY = below - ptY;
				}
			}
			return distance;
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape's control point.
		/// </summary>
		/// <param projectName="shape">The shape for which the nearest snap point is searched.</param>
		/// <param projectName="connectionPointId">The control point of the shape.</param>
		/// <param projectName="moveByX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param projectName="moveByY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param projectName="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param projectName="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <returns>Bee-line distance to nearest snap point.</returns>
		protected float GetDistanceToNearestSnapPoint(Shape shape, int controlPointId, int pointOffsetX, int pointOffsetY, out int snapDeltaX, out int snapDeltaY) {
			snapDeltaX = snapDeltaY = 0;
			Point p = Point.Empty;
			p = shape.GetControlPointPosition(controlPointId);
			return GetDistanceToNearestSnapPoint(p.X + pointOffsetX, p.Y + pointOffsetY, out snapDeltaX, out snapDeltaY);
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape. ControlPoints of the shape are 
		/// </summary>
		/// <param projectName="shape">The shape for which the nearest snap point is searched.</param>
		/// <param projectName="moveByX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param projectName="moveByY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param projectName="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param projectName="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param projectName="controlPointCapability">Filter for ControlPoints taken into account while calculating the snap distance.</param>
		/// <returns>Control point of the shape, the calculated distance refers to.</returns>
		protected int GetDistanceToNearestSnapPoint(Shape shape, int pointOffsetX, int pointOffsetY, out int snapDeltaX, out int snapDeltaY, ControlPointCapability controlPointCapability) {
			snapDeltaX = snapDeltaY = 0;
			int result = -1;
			int snapDistance = CurrentDisplay.SnapDistance;
			float lowestDistance = float.MaxValue;
			foreach (int ptId in shape.GetControlPointIds(controlPointCapability)) {
				int dx, dy;
				float currDistance = GetDistanceToNearestSnapPoint(shape, ptId, pointOffsetX, pointOffsetY, out dx, out dy);
				if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
					lowestDistance = currDistance;
					result = ptId;
					snapDeltaX = dx;
					snapDeltaY = dy;
				}
			}
			return result;
		}


		/// <summary>
		/// Finds the nearest ControlPoint in range of the given shape's ControlPoint. If there is no ControlPoint in range, the snap distance to the nearest grid line will be calculated.
		/// </summary>
		/// <param projectName="shape">The given shape.</param>
		/// <param projectName="connectionPointId">the given shape's ControlPoint</param>
		/// <param projectName="moveByX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param projectName="moveByY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param projectName="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param projectName="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param projectName="gluePointId">The Id of the returned shape's nearest ControlPoint.</param>
		/// <returns>The shape owning the nearest ControlPoint</returns>
		protected Shape GetDistanceToNearestControlPoint(Shape shape, int controlPointId, int pointOffsetX, int pointOffsetY, out int snapDeltaX, out int snapDeltaY, out int targetPointId, ControlPointCapability targetPointCapabilities) {
			Shape result = null;
			snapDeltaX = snapDeltaY = 0;
			targetPointId = ControlPointId.None;

			if (CurrentDisplay != null && CurrentDisplay.Diagram != null) {
				// calculate new position of the ControlPoint
				Point ctrlPtPos = Point.Empty;
				ctrlPtPos = shape.GetControlPointPosition(controlPointId);
				ctrlPtPos.Offset(pointOffsetX, pointOffsetY);

				Point targetPtPos = Point.Empty;
				int snapDistance = CurrentDisplay.SnapDistance;
				foreach (Shape targetShape in CurrentDisplay.Diagram.Shapes.FindShapesFromPosition(ctrlPtPos.X, ctrlPtPos.Y, snapDistance, ControlPointCapability.Connect)) {
					if (targetShape == shape) continue;
					float distance, lowestDistance = float.MaxValue;
					int ptId = targetShape.GetNearestControlPoint(ctrlPtPos.X, ctrlPtPos.Y, snapDistance, targetPointCapabilities);
					if (ptId != ControlPointId.None && ptId != ControlPointId.Reference) {
						targetPtPos = targetShape.GetControlPointPosition(ptId);
						distance = Geometry.DistancePointPoint(ctrlPtPos.X, ctrlPtPos.Y, targetPtPos.X, targetPtPos.Y);
						if (distance <= snapDistance && distance < lowestDistance) {
							lowestDistance = distance;
							snapDeltaX = targetPtPos.X - ctrlPtPos.X;
							snapDeltaY = targetPtPos.Y - ctrlPtPos.Y;
							targetPointId = ptId;
							result = targetShape;
						}
					}
				}
				// calcualte distance to nearest grid point if there is no suitable control point in range
				if (targetPointId == ControlPointId.None)
					GetDistanceToNearestSnapPoint(ctrlPtPos.X, ctrlPtPos.Y, out snapDeltaX, out snapDeltaY);
			}
			return result;
		}
		
		
		protected void InvalidateSnapIndicators(Shape preview) {
			int transformedPointRadius, transformedGridSize;
			CurrentDisplay.ControlToDiagram(CurrentDisplay.HandleSize, out transformedPointRadius);
			transformedGridSize = (int)Math.Round(CurrentDisplay.GridSize * (CurrentDisplay.ZoomLevel / 100f));

			Rectangle bounds = preview.BoundingRectangle;
			foreach (int id in preview.GetControlPointIds(ControlPointCapability.All)) {
				Point p = Point.Empty;
				p = preview.GetControlPointPosition(id);
				bounds = Geometry.MergeRectangles(p.X, p.Y, p.X, p.Y, bounds);
			}
			bounds.Inflate(transformedPointRadius, transformedPointRadius);
			CurrentDisplay.InvalidateDiagram(bounds);
		}
		
		
		protected void DrawSnapIndicators(Shape preview) {
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;
			
			float zoom = CurrentDisplay.ZoomLevel / 100f;
			snapIndicatorRadius = CurrentDisplay.HandleSize;

			ResetZoom();
			foreach (int id in preview.GetControlPointIds(ControlPointCapability.All)) {
				Point p = Point.Empty;
				p = preview.GetControlPointPosition(id);				

				// check if the point is on a gridline
				bool drawX = p.X % (CurrentDisplay.GridSize * zoom) == 0;
				bool drawY = p.Y % (CurrentDisplay.GridSize * zoom) == 0;
				// collect coordinates for bounding box
				if (p.X < left) left = p.X;
				if (p.X > right) right = p.X;
				if (p.Y < top) top = p.Y;
				if (p.Y > bottom) bottom = p.Y;

				if (drawX || drawY) {
					CurrentDisplay.DiagramToControl(p, out p);
					CurrentDisplay.Graphics.FillEllipse(CurrentDisplay.HandleInteriorBrush, p.X - snapIndicatorRadius, p.Y - snapIndicatorRadius, snapIndicatorRadius * 2, snapIndicatorRadius * 2);
					CurrentDisplay.Graphics.DrawEllipse(innerSnapPen, p.X - snapIndicatorRadius, p.Y - snapIndicatorRadius, snapIndicatorRadius * 2, snapIndicatorRadius * 2);
				}
			}
			if (preview is IPlanarShape) {
				bool drawLeft = left % (CurrentDisplay.GridSize * zoom) == 0;
				bool drawTop = top % (CurrentDisplay.GridSize * zoom) == 0;
				bool drawRight = right % (CurrentDisplay.GridSize * zoom) == 0;
				bool drawBottom = bottom % (CurrentDisplay.GridSize * zoom) == 0;
				
				// transform coordinates
				CurrentDisplay.DiagramToControl(left, top, out left, out top);
				CurrentDisplay.DiagramToControl(right, bottom, out right, out bottom);
				
				// draw outlines
				if (drawLeft) CurrentDisplay.Graphics.DrawLine(outerSnapPen, left, top - 1, left, bottom + 1);
				if (drawRight) CurrentDisplay.Graphics.DrawLine(outerSnapPen, right, top - 1, right, bottom + 1);
				if (drawTop) CurrentDisplay.Graphics.DrawLine(outerSnapPen, left - 1, top, right + 1, top);
				if (drawBottom) CurrentDisplay.Graphics.DrawLine(outerSnapPen, left - 1, bottom, right + 1, bottom);
				// fill interior
				if (drawLeft) CurrentDisplay.Graphics.DrawLine(innerSnapPen, left, top, left, bottom);
				if (drawRight) CurrentDisplay.Graphics.DrawLine(innerSnapPen, right, top, right, bottom);
				if (drawTop) CurrentDisplay.Graphics.DrawLine(innerSnapPen, left, top, right, top);
				if (drawBottom) CurrentDisplay.Graphics.DrawLine(innerSnapPen, left, bottom, right, bottom);
			}
			RestoreZoom();
		}


		protected void InvalidateConnectionTargets(int prevPosX, int prevPosY, int currentPosX, int currentPosY) {
			// invalidate selectedShapes in last range
			int cnt = shapesInRange.Count;
			for (int i = 0; i < cnt; ++i) {
				if (shapesInRange[i].ContainsPoint(prevPosX, prevPosY))
					shapesInRange[i].Invalidate();
				CurrentDisplay.InvalidateControlPoints(shapesInRange[i], ControlPointCapability.Connect);
			}
			//// invalidate selectedShapes in last range
			//foreach (Shape shape in CurrentDisplay.DiagramController.FindShapesFromPosition(prevPosX, prevPosY, pointHighlightRange)) {
			//   if (shape.ContainsPoint(prevPosX, prevPosY))
			//      shape.Invalidate();
			//   CurrentDisplay.InvalidateControlPoints(shape, ControlPointCapability.AttachGluePointToConnectionPoint);
			//}

			// invalidate selectedShapes in current range
			shapesInRange.Clear();
			foreach (Shape shape in CurrentDisplay.Diagram.Shapes.FindShapesFromPosition(currentPosX, currentPosY, pointHighlightRange, ControlPointCapability.Connect)) {
				if (shape.ContainsPoint(currentPosX, currentPosY))
					shape.Invalidate();
				CurrentDisplay.InvalidateControlPoints(shape, ControlPointCapability.Connect);
				shapesInRange.Add(shape);
			}
		}


		protected void DrawConnectionTargets(int x, int y, IEnumerable<Shape> excludedShapes) {
			if (CurrentDisplay != null && CurrentDisplay.Project.Security.IsGranted(Permission.Connect)) {
				Graphics gfx = CurrentDisplay.Graphics;
				int snapDistance = CurrentDisplay.SnapDistance;
				
				// Get the shape under the cursor (potential connection target)
				int pointUnderCursor = ControlPointId.None;
				Shape shapeUnderCursor = CurrentDisplay.Diagram.Shapes.FindShapeFromPosition(x, y, snapDistance, ControlPointCapability.All, null);
				if (shapeUnderCursor != null && previewShapes.ContainsKey(shapeUnderCursor)) shapeUnderCursor = null;
				if (shapeUnderCursor != null) {
					pointUnderCursor = shapeUnderCursor.GetNearestControlPoint(x, y, snapDistance, ControlPointCapability.Connect);
					// If there is no ControlPoint under the Cursor and the cursor is over the shape, draw the shape's outline
					if (pointUnderCursor == ControlPointId.Reference && shapeUnderCursor.ContainsPoint(x, y))
						CurrentDisplay.DrawShapeOutline(gfx, shapeUnderCursor, DiagrammingDrawMode.Highlighted);
				}

				// Add shapes in range to the shapebuffer and then remove all excluded shapes
				shapeBuffer.Clear();
				shapeBuffer.AddRange(shapesInRange);
				if (excludedShapes != null) {
					foreach (Shape excludedShape in excludedShapes)
						shapeBuffer.Remove(excludedShape);
				}
				// Draw all connectionPoints off all shapes in range (except the excluded ones, see above)
				ResetZoom();
				int cnt = shapeBuffer.Count;
				for (int i = 0; i < cnt; ++i) {
					foreach (int ptId in shapeBuffer[i].GetControlPointIds(ControlPointCapability.Connect)) {
						DiagrammingDrawMode drawMode = DiagrammingDrawMode.Normal;
						if (shapeBuffer[i] == shapeUnderCursor && ptId == pointUnderCursor)
							drawMode = DiagrammingDrawMode.Highlighted;

						Point p = Point.Empty;
						p = shapeBuffer[i].GetControlPointPosition(ptId);
						CurrentDisplay.DiagramToControl(p, out p);
						CurrentDisplay.DrawConnectionPoint(gfx, shapeBuffer[i], ptId, p.X, p.Y, drawMode);
					}
				}
				RestoreZoom();
			}
		}


		protected static void SaveCursorToFile(Cursor cursor, string fileName) {
			using (Image img = new Bitmap(256, 256)) {
				using (Graphics g = Graphics.FromImage(img)) {
					g.Clear(Color.Fuchsia);
					cursor.Draw(g, Rectangle.FromLTRB(0, 0, cursor.Size.Width, cursor.Size.Height));
				}
				img.Save(fileName);
			}
		}


		protected Display CurrentDisplay { 
			get { return currentDisplay; }
			private set { currentDisplay = value;}
		}


		protected float ZoomedHandleSizeReal {
			get { return zoomedHandleSizeReal; }
			private set { zoomedHandleSizeReal = value; }
		}


		protected int ZoomedHandleSizeDiagram {
			get { return zoomedHandleSizeDiagram; }
			private set { zoomedHandleSizeDiagram = value; }
		}


		/// <summary>
		/// Untransformed coordinates of the current MouseEvent (control coordinates)
		/// </summary>
		protected Point CurrentMousePos {
			get { return currMousePos; }
			private set { currMousePos = value; }
		}


		/// <summary>
		/// Untransformed coordinates of the last MouseEvent (control coordinates)
		/// </summary>
		protected Point LastMousePos {
			get { return lastMousePos; }
			private set { lastMousePos = value; }
		}


		protected Modifiers CurrentModifiers {
			get { return currentModifiers; }
			private set { currentModifiers = value; }
		}


		protected ToolExecutedEventArgs ExecutedEventArgs;


		protected ToolExecutedEventArgs CancelledEventArgs;


		private void ContextMenuStrip_Opening(object sender, CancelEventArgs e) {
			if (CurrentDisplay != null) {
				List<IDiagrammingAction> shapeCommands = new List<IDiagrammingAction>();
				List<IDiagrammingAction> modelObjectCommands = new List<IDiagrammingAction>();
				List<IDiagrammingAction> newCommands = new List<IDiagrammingAction>();

				Point p = CurrentDisplay.PointToClient(Control.MousePosition);
				int mouseX; int mouseY;
				CurrentDisplay.ControlToDiagram(p.X, p.Y, out mouseX, out mouseY);
				if (CurrentDisplay.Diagram != null) {
					Shape nearestShape = CurrentDisplay.Diagram.Shapes.FindShapeFromPosition(mouseX, mouseY, CurrentDisplay.HandleSize, ControlPointCapability.All, null);
					int controlPointId = ControlPointId.None;
					if (nearestShape != null) controlPointId = nearestShape.GetNearestControlPoint(mouseX, mouseY, ZoomedHandleSizeDiagram, ControlPointCapability.All);

					foreach (Shape shape in CurrentDisplay.SelectedShapes) {
						int pointId = shape.GetNearestControlPoint(mouseX, mouseY, ZoomedHandleSizeDiagram, ControlPointCapability.All);
						
						// build MenuCommand list for shape commands						
						if (shapeCommands.Count == 0)
							shapeCommands.AddRange(shape.GetAllowedActions(CurrentDisplay.Project.Security, mouseX, mouseY, pointId));
						else {
							newCommands.Clear();
							newCommands.AddRange(shape.GetAllowedActions(CurrentDisplay.Project.Security, mouseX, mouseY, pointId));
							foreach (IDiagrammingAction cmd in shapeCommands) {
								if (newCommands.Contains(cmd))
									shapeCommands.Remove(cmd);
							}
						}

						// build menuCommandlist for ModelObject commands
						if (shape.ModelObject != null) {
							if (modelObjectCommands.Count == 0)
								modelObjectCommands.AddRange(shape.ModelObject.GetAllowedActions(CurrentDisplay.Project.Security));
							else {
								newCommands.Clear();
								newCommands.AddRange(shape.ModelObject.GetAllowedActions(CurrentDisplay.Project.Security));
								foreach (IDiagrammingAction cmd in modelObjectCommands) {
									if (newCommands.Contains(cmd))
										modelObjectCommands.Remove(cmd);
								}
							}
						}
						else {
							modelObjectCommands.Clear();
							break;
						}
					}

					// if MenuCommands were found for Shapes, add them to the context menu (if not already done)
					if (shapeCommands.Count > 0 && !CurrentDisplay.ContextMenuStrip.Items.Contains(shapeContextMenuItem)) {
						foreach (IDiagrammingAction action in shapeCommands) {
							if (action is CommandAction) {
								CommandAction commandAction = (CommandAction)action;
								Debug.Assert(commandAction.Command != null);

								// check if the command is allowed
								if (commandAction.Command.IsAllowed(CurrentDisplay.Project.Security)) {
									//commandAction.History = CurrentDisplay.project.History;
									commandAction.Project = CurrentDisplay.Project;
									//commandAction.Command.Cache = CurrentDisplay.project.Cache;
								} else continue;
							}
							shapeContextMenuItem.DropDownItems.Add(new ToolStripMenuItem(action.Text, action.Image, action.EventHandler));
						}

						if (CurrentDisplay.SelectedShapes.Count == 1)
							shapeContextMenuItem.Text = "Shape";
						else
							shapeContextMenuItem.Text = "Shapes";

						// add MenuItem (and separator) to ContextMenuStrip
						if (shapeContextMenuItem.DropDownItems.Count > 0) {
							if (!CurrentDisplay.ContextMenuStrip.Items.Contains(contextMenuSeparator))
								CurrentDisplay.ContextMenuStrip.Items.Insert(0, contextMenuSeparator);
							CurrentDisplay.ContextMenuStrip.Items.Insert(0, shapeContextMenuItem);
						}
					}

					// if MenuCommands were found for ModelObjects, add them to the context menu
					if (modelObjectCommands.Count > 0 && !CurrentDisplay.ContextMenuStrip.Items.Contains(modelObjectContextMenuItem)) {
						foreach (IDiagrammingAction cmd in modelObjectCommands)
							modelObjectContextMenuItem.DropDownItems.Add(new ToolStripMenuItem(cmd.Text, cmd.Image, cmd.EventHandler));

						if (CurrentDisplay.SelectedShapes.Count == 1)
							modelObjectContextMenuItem.Text = "ModelObject";
						else
							modelObjectContextMenuItem.Text = "ModelObjects";

						// add menuItem (and separator) to ContextMenuStrip
						if (modelObjectContextMenuItem.DropDownItems.Count > 0) {
							if (!CurrentDisplay.ContextMenuStrip.Items.Contains(contextMenuSeparator))
								CurrentDisplay.ContextMenuStrip.Items.Insert(0, contextMenuSeparator);
							CurrentDisplay.ContextMenuStrip.Items.Insert(0, modelObjectContextMenuItem);
						}
					}
				}
				e.Cancel = false;
			}
			else
				e.Cancel = true;

			currentDisplay.ContextMenuStrip.Opening -= ContextMenuStrip_Opening;
		}


		private void ContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
			// Dispose and clear all ToolStripItems for shape commands
			for (int i = shapeContextMenuItem.DropDownItems.Count - 1; i >= 0; --i)
				shapeContextMenuItem.DropDownItems[i].Dispose();
			shapeContextMenuItem.DropDownItems.Clear();
			// Dispose and clear all ToolStripItems for model commands
			for (int i = modelObjectContextMenuItem.DropDownItems.Count - 1; i >= 0; --i)
				modelObjectContextMenuItem.DropDownItems[i].Dispose();
			modelObjectContextMenuItem.DropDownItems.Clear();

			if (CurrentDisplay != null) {
				// Remove the shape submenu item
				if (CurrentDisplay.ContextMenuStrip.Items.Contains(shapeContextMenuItem))
					CurrentDisplay.ContextMenuStrip.Items.Remove(shapeContextMenuItem);
				// Remove the ModelObject submenu item
				if (CurrentDisplay.ContextMenuStrip.Items.Contains(modelObjectContextMenuItem))
					CurrentDisplay.ContextMenuStrip.Items.Remove(modelObjectContextMenuItem);
				// Remove the submenu item separator
				if (CurrentDisplay.ContextMenuStrip.Items.Contains(contextMenuSeparator))
					CurrentDisplay.ContextMenuStrip.Items.Remove(contextMenuSeparator);
				// unregister event handler
				CurrentDisplay.ContextMenuStrip.Closed -= ContextMenuStrip_Closed;
			}
		}


		#region Fields
		private string name; // the (unique) projectName of the tool.
		private string title; // the title that will be displayed in the ToolBar
		private string category; // the category of the tool, used for grouping tools
		private string toolTipText; // the hint that will be displayed when the mouse is hovering above the tool
		private Bitmap smallIcon; // the small icon of the tool
		private Bitmap largeIcon; // the large icon of the tool
		private Display currentDisplay;	// the ownerDisplay that is edited with this tool

		private Modifiers currentModifiers;		// indicates pressed modifier keys such as Shift or Control
		private Point currMousePos;				// untransformed coordinates of the ObjectRef MouseEvent (control coordinates)
		private Point lastMousePos;				// untransformed coordinates of the last MouseEvent (control coordinates)

		// transparency of the preview shape(s)
		//private const int transparency = 50;

		private Matrix unzoomedTransform = new Matrix();
		private Matrix displayTransform = null;

		// margin and background colors of the toolbox icons "LargeIcon" and "SmallIcon"
		protected int margin = 2;
		protected Color transparentColor = Color.LightGray;

		// drawing snapIndicators
		protected Pen outerSnapPen = new Pen(Color.FromArgb(196, Color.WhiteSmoke), 2);
		protected Pen innerSnapPen = new Pen(Color.FromArgb(196, Color.SteelBlue), 1);
		private int snapIndicatorRadius = 10;

		// highlighting connection targets in range
		private int pointHighlightRange = 50;
		private List<Shape> shapesInRange = new List<Shape>();
		private List<Shape> shapeBuffer = new List<Shape>();

		// zoomed handle pieRadius
		private float zoomedHandleSizeReal = 0;
		private int zoomedHandleSizeDiagram = 0;

		// Context Menu
		private ToolStripMenuItem shapeContextMenuItem = new ToolStripMenuItem();
		private ToolStripMenuItem modelObjectContextMenuItem = new ToolStripMenuItem();
		private ToolStripSeparator contextMenuSeparator = new ToolStripSeparator();
		
		private Dictionary<Shape, Shape> previewShapes = new Dictionary<Shape, Shape>();
		private Dictionary<Shape, Shape> originalShapes = new Dictionary<Shape, Shape>();
		#endregion
	}
	
	#endregion


	#region TemplateTool
	public abstract class TemplateTool : Tool, ITemplateTool {

		protected TemplateTool(Template template, IDisplayService displayService, string category)
			: base(category) {
			BaseConstruct(template, displayService);
		}


		protected TemplateTool(Template template, IDisplayService displayService)
			: base() {
			Category = template.Shape.ShapeType.CategoryTitle;
			BaseConstruct(template, displayService);
		}


		private void BaseConstruct(Template template, IDisplayService displayService) {
			this.template = template;
			this.template.Shape.DisplayService = displayService;
			if (string.IsNullOrEmpty(template.Name))
				Title = template.Shape.ShapeType.Name;
			else
				Title = template.Name;
			Refresh();
		}


		public override void Dispose() {
			// Do not dispose the Template - it has to be disposed by the cache
			base.Dispose();
		}


		public Template Template {
			get { return template; }
		}


		#region Fields
		private Template template;
		#endregion
	}
	#endregion


	#region PointerTool

	public class PointerTool : Tool {

		private enum ToolAction { None, Select, SelectWithFrame, EditCaption, MoveShape, MoveHandle, Rotate }


		public PointerTool()
			: base("Standard") {
			BaseConstruct();
		}


		public PointerTool(string category)
			: base(category) {
			BaseConstruct();
		}


		#region ITool Interface

		public new void Dispose() {
			base.Dispose();
			
			//if (rotateCursor != null) rotateCursor.Dispose();
			//if (moveHandleCursor != null) moveHandleCursor.Dispose();
			//if (moveShapeCursor != null) moveShapeCursor.Dispose();
			//if (actionDeniedCursor != null) actionDeniedCursor.Dispose();
			//if (editTextCursor != null) editTextCursor.Dispose();
			//if (connectCursor != null) connectCursor.Dispose();
			//if (disconnectCursor != null) disconnectCursor.Dispose();

			formatter.Dispose();
			frameBrush.Dispose();
			framePen.Dispose();
		}


		public override Cursor Cursor {
			get { return currentCursor ?? base.Cursor; }
		}
		
		
		public override void Refresh() {
			// nothing to do...
		}


		public override bool ExecuteMouseAction(IDisplay display, DiagrammingMouseEventArgs e) {
			bool result = false;
			if (base.ExecuteMouseAction(display, e)) {
				leftMouseButtonDown = (e.Buttons & DiagrammingMouseButtons.Left) != 0;
				int mouseX, mouseY;
				CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);
				ControlPointCapability handleCapabilities = ControlPointCapability.Resize | ControlPointCapability.Rotate;
				int handleRadius = (int)Math.Ceiling(ZoomedHandleSizeReal);

				Debug.Assert(CurrentDisplay.Diagram != null);
				// As soon as the mouseButton is pressed, we do not change the selectedShapeAtCursor
				if (!leftMouseButtonDown) SetSelectedShapeAtCursor(mouseX, mouseY, handleRadius, handleCapabilities);
				
				diagramShapeAtCursor = null;
				diagramControlPointAtCursor = ControlPointId.None;
				foreach (Shape shape in CurrentDisplay.Diagram.Shapes.FindShapesFromPosition(mouseX, mouseY, handleRadius, handleCapabilities)) {
					if (CurrentDisplay.SelectedShapes.Contains(shape)) continue;
					diagramShapeAtCursor = shape;
					diagramControlPointAtCursor = diagramShapeAtCursor.GetNearestControlPoint(mouseX, mouseY, ZoomedHandleSizeDiagram, ControlPointCapability.All);
					if (diagramShapeAtCursor != null) break;
				}
				action = DeterminePointerAction(e.EventType);

				// Handle the mouse event
				switch (e.EventType) {

					#region handle MouseDown event
					// Start drag action such as drawing a SelectionFrame or moving selectedShapes/shape handles
					case MouseEventType.MouseDown:
						if (leftMouseButtonDown) {
							CurrentDisplay.Capture = true;

							// init moving/resizing
							startMousePos = CurrentMousePos;
							if (SelectedShapeAtCursor != null) {
								// delete selection frame
								frameRect.X = mouseX;
								frameRect.Y = mouseY;
								frameRect.Size = Size.Empty;
							}
							result = true;
						}
						break;
					#endregion

					#region handle MouseMove event

					// Set cursors depending on HotSpots or draw moving/resizing preview
					case MouseEventType.MouseMove:
						// perform action
						switch (action) {
							case ToolAction.None:
								// nothing to do
								Debug.Print("MouseMove event, action = None");
								break;

							case ToolAction.Select:
							case ToolAction.SelectWithFrame:
							case ToolAction.EditCaption:
								// prepare drawing selection frame
								InvalidatePreview();
								result = true;
								break;

							case ToolAction.MoveHandle:
								// draw resize preview
								if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes)) {
									InvalidatePreview();
									result = true;
								}
								break;

							case ToolAction.MoveShape:
								// if clickedShape is not selected yet, select it
								if (SelectedShapeAtCursor == null && DiagramShapeAtCursor != null && !CurrentDisplay.SelectedShapes.Contains(DiagramShapeAtCursor)) {
									PerformSelection();
									SelectedShapeAtCursor = DiagramShapeAtCursor;
								}

								if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes)) {
									// invalidate move preview
									InvalidatePreview();
									result = true;
								}
								break;

							case ToolAction.Rotate:
								if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes)) {
									// if clickedShape is not selected yet, add it to the selection
									//if (SelectedShapeAtCursor != null && CurrentDisplay.SelectedShapes.Contains(SelectedShapeAtCursor) == false)
									//   PerformSelection(SelectedShapeAtCursor);
									Debug.Assert(SelectedShapeAtCursor != null && SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Rotate));

									// invalidate Previews
									InvalidatePreview();
									result = true;
								}
								break;

							default: throw new DiagrammingUnexpectedValueException(typeof(ToolAction), action);
						}
						break;
					#endregion

					#region handle MouseUp event
					// perform selection/moving/resizing
					case MouseEventType.MouseUp:
						if (leftMouseButtonDown) {
							CurrentDisplay.Capture = false;
							leftMouseButtonDown = false;
							switch (action) {
								case ToolAction.None:
									// do nothing
									break;

								case ToolAction.Select:
									// apply selection
									result = PerformSelection();
									break;

								case ToolAction.SelectWithFrame:
									// select all selectedShapes within the frame
									result = PerformFrameSelection();
									break;

								case ToolAction.EditCaption:
									// if the user clicked a caption, ownerDisplay the text editor
									if (SelectedShapeAtCursor != null && SelectedShapeAtCursor is ICaptionedShape && SelectedCaptionIndexAtCursor >= 0)
										CurrentDisplay.OpenCaptionEditor((ICaptionedShape)SelectedShapeAtCursor, SelectedCaptionIndexAtCursor);
									break;

								case ToolAction.MoveHandle:
									// apply resizeing
									if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes))
										result = PerformMoveHandle();
									break;

								case ToolAction.MoveShape:
									// apply moving
									if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes))
										result = PerformMoveShape();
									break;

								case ToolAction.Rotate:
									if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes))
										result = PerformRotate();
									currAngle = 0;
									break;

								default:
									throw new Exception("Unexpected PointerAction value.", new InvalidOperationException());
							}

							if (!CurrentDisplay.SelectedShapes.Contains(selectedShapeAtCursor))
							   selectedShapeAtCursor = null;
							action = ToolAction.None;
							ClearPreviews();
							OnToolExecuted(ExecutedEventArgs);

							if (CurrentDisplay != null && SelectedShapeAtCursor != null && SelectedShapeAtCursor.ContainsPoint(mouseX, mouseY))
								CurrentDisplay.OnShapeClick(new ShapeClickEventArgs(SelectedShapeAtCursor, e, CurrentModifiers));
						}
						break;
					#endregion

					#region handle MouseDoubleClick event
					case MouseEventType.MouseDoubleClick:
						if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes) && enableQuickRotate) {
							CurrentDisplay.Capture = false;
							leftMouseButtonDown = false;

							if (SelectedShapeAtCursor != null && SelectedControlPointAtCursor > 0 && SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Reference) && e.Clicks > 1) {
								int angle = 900 * (e.Clicks - 1);
								PerformQuickRotate(angle);
								selectedShapeAtCursor = null;
								action = ToolAction.None;
								ClearPreviews();
								OnToolExecuted(ExecutedEventArgs);
							}
						}
						break;
					#endregion

					#region unused MouseEvents
					case MouseEventType.MouseClick:
						if (CurrentDisplay != null && SelectedShapeAtCursor != null && SelectedShapeAtCursor.ContainsPoint(mouseX, mouseY))
							CurrentDisplay.OnShapeClick(new ShapeClickEventArgs(SelectedShapeAtCursor, e, CurrentModifiers));
						break;
					case MouseEventType.MouseEnter:
					case MouseEventType.MouseLeave:
						// nothing to do
						break;
					#endregion

					default: throw new DiagrammingUnexpectedValueException(e.EventType);
				}
				SetCursor();
				CurrentDisplay.Cursor = Cursor;
				result = true;
			}
			return result;
		}


		public override bool ExecuteKeyAction(IDisplay display, DiagrammingKeyEventArgs e) {
			bool result = false;
			if (base.ExecuteKeyAction(display, e)) {

				switch (e.EventType) {
					case KeyEventType.PreviewKeyDown:
					case KeyEventType.KeyDown:
						// do nothing
						break;
					case KeyEventType.KeyUp:
						// handle Function keys
						switch (e.KeyCode) {
							case (int)Keys.F1: break;
							case (int)Keys.F2: ShowTextEditor(""); break;
							case (int)Keys.F3: break;
							case (int)Keys.F4: break;
							case (int)Keys.F5: break;
							case (int)Keys.F6: break;
							case (int)Keys.F7: break;
							case (int)Keys.F8: break;
							case (int)Keys.F9: break;
							case (int)Keys.F10: break;
							case (int)Keys.F11: break;
							case (int)Keys.F12: break;
							case (int)Keys.F13: break;
							case (int)Keys.F14: break;
							case (int)Keys.F15: break;
							case (int)Keys.F16: break;
							case (int)Keys.F17: break;
							case (int)Keys.F18: break;
							case (int)Keys.F19: break;
							case (int)Keys.F20: break;
							case (int)Keys.F21: break;
							case (int)Keys.F22: break;
							case (int)Keys.F23: break;
							case (int)Keys.F24: break;
						}
							// ToDo: handle ShortCuts (Ctrl + ?)
						result = true;
						break;

					case KeyEventType.KeyPress:
						if (!char.IsControl(e.KeyChar))
							ShowTextEditor(e.KeyChar.ToString());
						result = true;
						break;
					default:
						throw new DiagrammingException(string.Format("Unexpected {0} value '{1}'.", e.EventType.GetType(), e.EventType));
				}
			}
			return result;
		}
		
		
		public override void Cancel() {
			// invalidate drawn previews
			foreach (Shape preview in Previews.Values)
				preview.Invalidate();
			ClearPreviews();

			if (CurrentDisplay != null) {
				CurrentDisplay.Capture = false;
				InvalidateSelectionFrame();
				InvalidateAnglePreview();
			}
			frameRect = Rectangle.Empty;
			rectBuffer = Rectangle.Empty;

			action = ToolAction.None;
			leftMouseButtonDown = false;
			selectedShapeAtCursor = null;

			base.Cancel();
		}


		public override void DrawPreview() {
			switch (action) {
				case ToolAction.None:
				case ToolAction.Select:
					// nothing to do
					break;

				case ToolAction.EditCaption:
					#region Draw highlighted caption bounds
					// MouseOver-Highlighting of the caption under the cursor 
					// At the moment, the ownerDisplay draws the caption bounds along with the selection highlighting
					Debug.Assert(CurrentDisplay != null);
					if (SelectedShapeAtCursor is ICaptionedShape && SelectedCaptionIndexAtCursor >= 0) {
						ResetZoom();
						int mouseX, mouseY;
						CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);
						CurrentDisplay.DrawCaptionBounds(CurrentDisplay.Graphics, (ICaptionedShape)SelectedShapeAtCursor, SelectedCaptionIndexAtCursor, DiagrammingDrawMode.Highlighted);
						RestoreZoom();
					}
					#endregion
					break;

				case ToolAction.SelectWithFrame:
					#region Draw selection frame
					ResetZoom();
					CurrentDisplay.DiagramToControl(frameRect, out rectBuffer);
					if (CurrentDisplay.HighQualityRendering) {
						CurrentDisplay.Graphics.FillRectangle(frameBrush, rectBuffer);
						CurrentDisplay.Graphics.DrawRectangle(framePen, rectBuffer);
					}
					else {
						ControlPaint.DrawLockedFrame(CurrentDisplay.Graphics, rectBuffer, false);
						//ControlPaint.DrawFocusRectangle(CurrentDisplay.Graphics, area, Color.White, Color.Black);
					}
					RestoreZoom();
					#endregion
					break;
				
				case ToolAction.MoveShape:
				case ToolAction.MoveHandle:
					#region Draw shape preview
					// Draw shape previews first
					foreach (Shape preview in Previews.Values)
						preview.Draw(CurrentDisplay.Graphics);

					// Then draw snap-lines and -points
					if (SelectedShapeAtCursor != null && (snapPtId > 0 || snapDeltaX != 0 || snapDeltaY != 0)) {
						Shape previewAtCursor = this.FindPreviewShape(SelectedShapeAtCursor);
						DrawSnapIndicators(previewAtCursor);
					}

					// Finally, draw highlighten ConnectionPoints and/or highlighted shape outlines
					if (Previews.Count == 1 && SelectedControlPointAtCursor != ControlPointId.None) {
						Shape preview = null;
						foreach (Shape shape in Previews.Values) {
							preview = shape;
							break;
						}
						if (preview.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Glue)) {
							// find and highlight connectionPoints in Range
							Point gluePointPos = preview.GetControlPointPosition(SelectedControlPointAtCursor);
							DrawConnectionTargets(gluePointPos.X, gluePointPos.Y, CurrentDisplay.SelectedShapes);
						}
					}
					#endregion
					break;
				
				case ToolAction.Rotate:
					#region Draw shape previews
					Graphics gfx = CurrentDisplay.Graphics;

					// draw shape previews
					foreach (Shape preview in Previews.Values)
						preview.Draw(gfx);
					#endregion

					#region Draw shapeAngle preview

					// draw shapeAngle preview
					IPlanarShape planarPreview = null;
					foreach (Shape shape in Previews.Values) {
						if (shape is IPlanarShape) planarPreview = (IPlanarShape)shape;
						break;
					}
					if (planarPreview != null) {
						if (rectBuffer.Width > 0 && rectBuffer.Height > 0) {
							ResetZoom();

							// draw (unzoomed) shapeAngle preview
							Point pieCenter = Point.Empty;
							int pieRadius = 0;
							Rectangle layoutRect = Rectangle.Empty;
							CurrentDisplay.DiagramToControl(rectBuffer.Location, out pieCenter);
							CurrentDisplay.DiagramToControl(rectBuffer.Width, out pieRadius);

							// Check if the cursor has the minimum distance from the rotation point
							if (pieRadius > minRotateDistance) {
								// Calculate angle and angle info text
								float angleDeg = Geometry.TenthsOfDegreeToDegrees(currAngle <= 1800 ? currAngle : (currAngle - 3600));
								string angleInfoText = null;
								if (Previews.Count > 1)
									angleInfoText = string.Format("{0}°", angleDeg);
								else {
									float origShapeAngle = Geometry.TenthsOfDegreeToDegrees(planarPreview.Angle - currAngle);
									float newShapeAngle = Geometry.TenthsOfDegreeToDegrees(planarPreview.Angle % 3600);
									angleInfoText = string.Format("{0}° ({1}° {2} {3}°)", newShapeAngle, origShapeAngle, angleDeg < 0 ? '-' : '+', Math.Abs(angleDeg));
								}
								// Calculate LayoutRectangle's size for the angle info text
								layoutRect.Size = TextMeasurer.MeasureText(gfx, angleInfoText, CurrentDisplay.Font, Size.Empty, formatter);
								layoutRect.Width = Math.Min((int)Math.Round(pieRadius * 1.5), layoutRect.Width);
								// Calculate the circumcircle of the LayoutRectangle and the distance between mouse and rotation center...
								float circumCircleRadius = Geometry.DistancePointPoint(-rotateCursor.Size.Width / 2f, -rotateCursor.Size.Height / 2f, layoutRect.Width, layoutRect.Height) / 2f;
								float mouseDistance = Geometry.DistancePointPoint(pieCenter, CurrentMousePos);
								float interpolationFactor = circumCircleRadius / mouseDistance;
								// ... then transform the layoutRectangle towards the mouse cursor
								PointF textCenter = Geometry.VectorLinearInterpolation((PointF)CurrentMousePos, (PointF)pieCenter, interpolationFactor);
								layoutRect.X = (int)Math.Round(textCenter.X - (layoutRect.Width / 2f));
								layoutRect.Y = (int)Math.Round(textCenter.Y - (layoutRect.Height / 2f));

								// Draw angle info pie
								int pieSize = pieRadius + pieRadius;
								if (CurrentDisplay.HighQualityRendering)  {
									gfx.DrawEllipse(framePen, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieSize, pieSize);
									gfx.FillPie(frameBrush, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieSize, pieSize, 0, angleDeg);
									gfx.DrawPie(framePen, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieSize, pieSize, 0, angleDeg);
								}
								else {
									gfx.DrawPie(Pens.Black, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieSize, pieSize, 0, angleDeg);
									gfx.DrawPie(Pens.Black, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieSize, pieSize, 0, angleDeg);
								}
								gfx.DrawString(angleInfoText, CurrentDisplay.Font, Brushes.Black, layoutRect, formatter);
							}
							else {
								// If cursor is nearer to the rotation point that the required distance,
								// Draw rotation instuction preview
								if (CurrentDisplay.HighQualityRendering) {
									// draw shapeAngle preview circle
									gfx.DrawEllipse(framePen, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieRadius + pieRadius, pieRadius + pieRadius);
									gfx.FillPie(frameBrush, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieRadius + pieRadius, pieRadius + pieRadius, 0, 45f);
									gfx.DrawPie(framePen, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieRadius + pieRadius, pieRadius + pieRadius, 0, 45f);

									// Draw rotation direction arrow line
									int bowInsetX, bowInsetY;
									bowInsetX = bowInsetY = pieRadius / 4;
									gfx.DrawArc(framePen, pieCenter.X - pieRadius + bowInsetX, pieCenter.Y - pieRadius + bowInsetY, pieRadius + pieRadius - bowInsetX - bowInsetX, pieRadius + pieRadius - bowInsetY - bowInsetY, 0, 22.5f);
									// Calculate Arrow Tip
									int arrowTipX = 0; int arrowTipY = 0;
									arrowTipX = pieCenter.X + pieRadius - bowInsetX;
									arrowTipY = pieCenter.Y;
									Geometry.RotatePoint(pieCenter.X, pieCenter.Y, 45f, ref arrowTipX, ref arrowTipY);
									arrowShape[0].X = arrowTipX;
									arrowShape[0].Y = arrowTipY;
									//
									arrowTipX = pieCenter.X + pieRadius - bowInsetX - CurrentDisplay.HandleSize - CurrentDisplay.HandleSize;
									arrowTipY = pieCenter.Y;
									Geometry.RotatePoint(pieCenter.X, pieCenter.Y, 22.5f, ref arrowTipX, ref arrowTipY);
									arrowShape[1].X = arrowTipX;
									arrowShape[1].Y = arrowTipY;
									//
									arrowTipX = pieCenter.X + pieRadius - bowInsetX + CurrentDisplay.HandleSize + CurrentDisplay.HandleSize;
									arrowTipY = pieCenter.Y;
									Geometry.RotatePoint(pieCenter.X, pieCenter.Y, 22.5f, ref arrowTipX, ref arrowTipY);
									arrowShape[2].X = arrowTipX;
									arrowShape[2].Y = arrowTipY;
									// Draw arrow tip
									gfx.FillPolygon(frameBrush, arrowShape);
									gfx.DrawPolygon(framePen, arrowShape);
								}
								else gfx.DrawPie(Pens.Black, pieCenter.X - pieRadius, pieCenter.Y - pieRadius, pieRadius * 2, pieRadius * 2, 0, 45f);
							}
						}
						RestoreZoom();
					}
					#endregion
					break;
				
				default:
					throw new Exception(string.Format("Unexpected PointerAction value {0}.", action));
			}
		}


		public override void InvalidatePreview() {
			switch (action) {
				case ToolAction.None:
					break;

				case ToolAction.Select:
				case ToolAction.EditCaption:
					if (SelectedShapeAtCursor != null) {
						SelectedShapeAtCursor.Invalidate();
						CurrentDisplay.InvalidateControlPoints(SelectedShapeAtCursor, ControlPointCapability.All);
					}
					break;

				case ToolAction.SelectWithFrame:
					InvalidateSelectionFrame();
					break;

				case ToolAction.MoveHandle:
					PrepareMoveHandlePreview();
					break;

				case ToolAction.MoveShape:
					PrepareMoveShapePreview();
					break;

				case ToolAction.Rotate:
					InvalidateAnglePreview();
					PrepareRotatePreview();
					break;

				default:
					throw new DiagrammingUnexpectedValueException(typeof(DiagrammingAction), action);
			}
		}
		
		#endregion


		#region Selecting Shapes

		// (de)select shape unter the mouse pointer
		private bool PerformSelection() {
			bool result = false;
			bool multiSelect = ((Control.ModifierKeys & Keys.Control) == Keys.Control) || 
								((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
			
			int mouseX, mouseY;
			CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);
			int distance = (int)Math.Ceiling(ZoomedHandleSizeReal);
			ControlPointCapability capabilities = ControlPointCapability.Resize | ControlPointCapability.Rotate;

			// Determine the shape that has to be selected:
			Shape shapeUnderCursor = null;
			if (SelectedShapeAtCursor != null) {
				// When in multiSelection mode, unselect the selected shape under the cursor
				if (multiSelect) shapeUnderCursor = SelectedShapeAtCursor;
				else {
					// First, check if the selected shape under the cursor has children that can be selected
					shapeUnderCursor = SelectedShapeAtCursor.Children.FindShapeFromPosition(mouseX, mouseY, distance, capabilities, null);
					// Second, check if the selected shape under the cursor has siblings that can be selected
					if (shapeUnderCursor == null && SelectedShapeAtCursor.Parent != null) {
						shapeUnderCursor = SelectedShapeAtCursor.Parent.Children.FindShapeFromPosition(mouseX, mouseY, distance, capabilities, SelectedShapeAtCursor);
						if (shapeUnderCursor == null) {
							foreach (Shape shape in SelectedShapeAtCursor.Parent.Children.FindShapesFromPosition(mouseX, mouseY, distance, capabilities)) {
								if (shape == SelectedShapeAtCursor) continue;
								else {
									shapeUnderCursor = shape;
									break;
								}
							}
						}
					}
					// Third, check if there are non-selected shapes below the selected shape under the cursor
					if (shapeUnderCursor == null && CurrentDisplay.Diagram.Shapes.Contains(SelectedShapeAtCursor))
						shapeUnderCursor = CurrentDisplay.Diagram.Shapes.FindShapeFromPosition(mouseX, mouseY, distance, capabilities, SelectedShapeAtCursor);
				}
			}
			// If there was shape to select related to the selected shape under the cursor
			// (a child or a sibling of the selected shape or a shape below it),
			// try to select the first non.-selected shape under the cursor
			if (shapeUnderCursor == null)
				shapeUnderCursor = DiagramShapeAtCursor;

			// if a new shape to select was found, perform selection
			if (shapeUnderCursor != null) {
				// (check if multiselection mode is enabled (Shift + Click or Ctrl + Click))
				if (multiSelect) {
					// if multiSelect is enabled, add/remove to/from selected selectedShapes...
					if (CurrentDisplay.SelectedShapes.Contains(shapeUnderCursor)) {
						// if object is selected -> remove from selection
						CurrentDisplay.UnselectShape(shapeUnderCursor);
						RemovePreviewOf(shapeUnderCursor);
						result = true;
					} else {
						// If object is not selected -> add to selection
						CurrentDisplay.SelectShape(shapeUnderCursor, true);
						result = true;
					}
				}
				else {
					// ... otherwise deselect all selectedShapes but the clicked object
					ClearPreviews();
					// check if the clicked shape is a child of an already selected shape
					Shape childShape = null;
					if (CurrentDisplay.SelectedShapes.Count == 1
						&& CurrentDisplay.SelectedShapes.TopMost.Children != null
						&& CurrentDisplay.SelectedShapes.TopMost.Children.Count > 0) {
						Point p = CurrentDisplay.PointToClient(Control.MousePosition);
						CurrentDisplay.ControlToDiagram(p, out p);
						childShape = CurrentDisplay.SelectedShapes.TopMost.Children.FindShapeFromPosition(p.X, p.Y, 0, ControlPointCapability.None, null);
					}
					if (childShape != null) CurrentDisplay.SelectShape(childShape, false);
					else CurrentDisplay.SelectShape(shapeUnderCursor, false);
					result = true;
				}

				// validate if the desired shape or its parent was selected
				if (shapeUnderCursor.Parent != null) {
					if (!CurrentDisplay.SelectedShapes.Contains(shapeUnderCursor))
						if (CurrentDisplay.SelectedShapes.Contains(shapeUnderCursor.Parent))
							shapeUnderCursor = shapeUnderCursor.Parent;
				}
			} else if (SelectedShapeAtCursor == null) {
				// if there was no other shape to select and none of the selected shapes is under the cursor,
				// clear selection
				if (!multiSelect) {
					if (CurrentDisplay.SelectedShapes.Count > 0) {
						CurrentDisplay.UnselectAll();
						ClearPreviews();
					}
					result = true;
				}
			} else {
				// if there was no other shape to select and a selected shape is under the cursor,
				// do nothing
			}
			return result;
		}


		// draw selection frame
		private bool InvalidateSelectionFrame() {
			bool result = false;
			if (leftMouseButtonDown /*&& LastMousePos != CurrentMousePos*/) {
				// invalidate area of the old frame
				CurrentDisplay.InvalidateDiagram(frameRect);

				int startMouseX, startMouseY, currMouseX, currMouseY;
				CurrentDisplay.ControlToDiagram(StartMousePos.X, StartMousePos.Y, out startMouseX, out startMouseY);
				CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out currMouseX, out currMouseY);

				frameRect.X = Math.Min(startMouseX, currMouseX);
				frameRect.Y = Math.Min(startMouseY, currMouseY);
				frameRect.Width = Math.Max(startMouseX, currMouseX) - frameRect.X;
				frameRect.Height = Math.Max(startMouseY, currMouseY) - frameRect.Y;

				// invalidate new selectionframe area
				CurrentDisplay.InvalidateDiagram(frameRect);
				result = true;
			}
			return result;
		}


		private bool PerformFrameSelection() {
			bool result = false;
			bool multiSelect = ((Control.ModifierKeys & Keys.Control) == Keys.Control) || ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);

			CurrentDisplay.SelectShapes(frameRect, multiSelect);
			CurrentDisplay.InvalidateDiagram(frameRect);
			frameRect.Size = Size.Empty;
			result = true;

			return result;
		}

		#endregion


		#region Connecting / Disconnecting GluePoints

		private bool IsConnectedToNonSelectedShapes(Shape shape) {
			foreach (int gluePointId in shape.GetControlPointIds(ControlPointCapability.Glue)) {
				foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(null, gluePointId)) {
					if (!CurrentDisplay.SelectedShapes.Contains(sci.PassiveShape))
						return true;
				}
			}
			return false;
		}


		private bool ShapeHasGlueConnectionPoint(Shape shape) {
			foreach (int id in shape.GetControlPointIds(ControlPointCapability.Glue))
				return true;
			return false;
		}


		private void DisconnectGluePoints() {
			foreach (Shape selectedShape in CurrentDisplay.SelectedShapes) {
				foreach (int ptId in selectedShape.GetControlPointIds(ControlPointCapability.Connect | ControlPointCapability.Glue)) {
					// disconnect GluePoints if they are moved together with their targets
					bool skip = false;
					foreach (ShapeConnectionInfo ci in selectedShape.GetConnectionInfos(null, ptId)) {
						if (ci.GluePointId != ptId) throw new DiagrammingInternalException("Fatal error: Unexpected ShapeConnectionInfo was returned.");
						if (CurrentDisplay.SelectedShapes.Contains(ci.PassiveShape)) {
							skip = false;
							break;
						}
					}
					if (skip) continue;
					
					// otherwise, compare positions of the GluePoint with it's targetPoint and disconnect if they are not equal
					if (selectedShape.HasControlPointCapability(ptId, ControlPointCapability.Glue)) {
						Shape previewShape = FindPreviewShape(selectedShape);
						if (selectedShape.GetControlPointPosition(ptId) != previewShape.GetControlPointPosition(ptId)) {
							bool isConnected = false;
							foreach(ShapeConnectionInfo sci in selectedShape.GetConnectionInfos(null, ptId)) {
								if (sci.GluePointId == ptId) {
									isConnected = true;
									break;
								} else throw new DiagrammingInternalException("Fatal error: Unexpected ShapeConnectionInfo was returned.");
							}
							if (isConnected) {
								ICommand cmd = new DisconnectCommand(selectedShape, ptId);
								CurrentDisplay.Project.ExecuteCommand(cmd);
							}
						}
					}
				}
			}
		}


		private void ConnectGluePoints() {
			foreach (Shape selectedShape in CurrentDisplay.SelectedShapes) {
				// find selectedShapes that own GluePoints
				foreach (int gluePointId in selectedShape.GetControlPointIds(ControlPointCapability.Glue)) {
					Point gluePointPos = Point.Empty;
					gluePointPos = selectedShape.GetControlPointPosition(gluePointId);

					// find selectedShapes to connect to
					foreach (Shape shape in CurrentDisplay.Diagram.Shapes.FindShapesFromPosition(gluePointPos.X, gluePointPos.Y, CurrentDisplay.HandleSize, ControlPointCapability.Connect)) {
						if (!CurrentDisplay.SelectedShapes.Contains(shape)) {
							int targetPointId = shape.GetNearestControlPoint(gluePointPos.X, gluePointPos.Y, 0, ControlPointCapability.Connect);
							if (targetPointId != ControlPointId.None) {
								ICommand cmd = new ConnectCommand(selectedShape, gluePointId, shape, targetPointId);
								CurrentDisplay.Project.ExecuteCommand(cmd);
							}
							else if (shape.ContainsPoint(gluePointPos.X, gluePointPos.Y)) {
								ICommand cmd = new ConnectCommand(selectedShape, gluePointId, shape, ControlPointId.Reference);
								CurrentDisplay.Project.ExecuteCommand(cmd);
							}							
						}
						else {
							// restore connections that were disconnected before
							int targetPointId = shape.GetNearestControlPoint(gluePointPos.X, gluePointPos.Y, 0, ControlPointCapability.Connect);
							if (targetPointId != ControlPointId.None) 
								selectedShape.Connect(gluePointId, shape, targetPointId);
						}
					}
				}
			}
		}
		
		#endregion


		#region Moving Shapes

		// prepare drawing preview of move action
		private bool PrepareMoveShapePreview() {
			bool result = true;
			CreatePreviewShapes();

			// invalidate snap indicators at their current position
			InvalidateSnapIndicators(PreviewAtCursor);

			// calculate the movement
			Point startPos = Point.Empty;
			Point currPos = Point.Empty;
			Point lastPos = Point.Empty;
			CurrentDisplay.ControlToDiagram(LastMousePos, out lastPos);
			CurrentDisplay.ControlToDiagram(CurrentMousePos, out currPos);
			CurrentDisplay.ControlToDiagram(StartMousePos, out startPos);
			int distanceX = currPos.X - startPos.X;
			int distanceY = currPos.Y - startPos.Y;

			// calculate "Snap to Grid" offset
			snapDeltaX = snapDeltaY = 0;
			if (CurrentDisplay.SnapToGrid) {
				InvalidateSnapIndicators(PreviewAtCursor);
				snapPtId = GetDistanceToNearestSnapPoint(SelectedShapeAtCursor, distanceX, distanceY, out snapDeltaX, out snapDeltaY, ControlPointCapability.All);
				distanceX += snapDeltaX;
				distanceY += snapDeltaY;
			}

			// move selectedShapes
			foreach (Shape selectedShape in CurrentDisplay.SelectedShapes) {
				Shape preview = FindPreviewShape(selectedShape);
				preview.MoveControlPointTo(ControlPointId.Reference, selectedShape.X + distanceX, selectedShape.Y + distanceY, Modifiers.None);
			}

			if (CurrentDisplay.SnapToGrid)
				InvalidateSnapIndicators(PreviewAtCursor);
			return result;
		}


		// apply the move action
		private bool PerformMoveShape() {
			bool result = false;
			if (StartMousePos!= CurrentMousePos) {
				// invalidate original shapes including oparents and control points at their current position
				InvalidateShapes(CurrentDisplay.SelectedShapes);
				InvalidateSnapIndicators(PreviewAtCursor);

				// calculate the movement
				Point startPos = Point.Empty;
				Point currPos = Point.Empty;
				CurrentDisplay.ControlToDiagram(CurrentMousePos, out currPos);
				CurrentDisplay.ControlToDiagram(StartMousePos, out startPos);
				int distanceX = currPos.X - startPos.X;
				int distanceY = currPos.Y - startPos.Y;
				snapDeltaX = snapDeltaY = 0;
				if (CurrentDisplay.SnapToGrid)
					GetDistanceToNearestSnapPoint(SelectedShapeAtCursor, distanceX, distanceY, out snapDeltaX, out snapDeltaY, ControlPointCapability.All);

				ICommand cmd = new MoveShapeByCommand(CurrentDisplay.SelectedShapes, distanceX + snapDeltaX, distanceY + snapDeltaY, CurrentDisplay.Project.Repository);
				CurrentDisplay.Project.ExecuteCommand(cmd);

				snapDeltaX = snapDeltaY = 0;
				snapPtId = ControlPointId.None;
				result = true;
			}

			return result;
		}
		
		#endregion


		#region Moving ControlPoints

		// prepare drawing preview of resize action 
		private bool PrepareMoveHandlePreview() {
			bool result = true;
			CreatePreviewShapes();

			// invalidate snap indicators
			InvalidateSnapIndicators(PreviewAtCursor);

			Point currPos = Point.Empty;
			Point startPos = Point.Empty;
			CurrentDisplay.ControlToDiagram(CurrentMousePos, out currPos);
			CurrentDisplay.ControlToDiagram(StartMousePos, out startPos);

			int distanceX, distanceY;
			distanceX = currPos.X - startPos.X;
			distanceY = currPos.Y - startPos.Y;

			// calculate "Snap to Grid/ControlPoint" offset
			snapDeltaX = snapDeltaY = 0;
			bool isGluePoint = SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Glue);
			if (isGluePoint) {
				int targetPtId;
				Shape targetShape = GetDistanceToNearestControlPoint(SelectedShapeAtCursor, SelectedControlPointAtCursor, distanceX, distanceY, out snapDeltaX, out snapDeltaY, out targetPtId, ControlPointCapability.Connect);
				if (targetShape != null) targetShape.Invalidate();
			}
			else 
				GetDistanceToNearestSnapPoint(SelectedShapeAtCursor, SelectedControlPointAtCursor, distanceX, distanceY, out snapDeltaX, out snapDeltaY);
			distanceX += snapDeltaX;
			distanceY += snapDeltaY;

			// ToDo: optimize this: fewer move operations
			// (This code does not work yet)
			//Point originalPtPos = Point.Empty;
			//for (int i = 0; i < CurrentDisplay.SelectedShapes.Count; ++i) {
			//   // reset position
			//   originalPtPos = CurrentDisplay.SelectedShapes[i].GetControlPointPosition(SelectedPointId);
			//   // perform new movement
			//   if (Previews[i].HasControlPointCapability(SelectedPointId, ControlPointCapability.Resize))
			//      Previews[i].MoveControlPointTo(SelectedPointId, originalPtPos.X + distanceX, originalPtPos.Y + distanceY, CurrentModifiers);
			//}

			// move selected shapes
			Point originalPtPos = Point.Empty;
			foreach (Shape selectedShape in CurrentDisplay.SelectedShapes) {
				Shape previewShape = FindPreviewShape(selectedShape);
				
				// reset position
			   originalPtPos = selectedShape.GetControlPointPosition(SelectedControlPointAtCursor);
			   previewShape.MoveControlPointTo(SelectedControlPointAtCursor, originalPtPos.X, originalPtPos.Y, CurrentModifiers);
			   previewShape.MoveControlPointTo(ControlPointId.Reference, selectedShape.X, selectedShape.Y, Modifiers.None);

			   // perform new movement
			   if (previewShape.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Resize))
			      previewShape.MoveControlPointBy(SelectedControlPointAtCursor, distanceX, distanceY, CurrentModifiers);
			}

			// if a single GluePoint is dragged
			if (isGluePoint) {
				// highlight target points
				// find and highlight connectionPoints in Range
				Point lastPos = Point.Empty;
				CurrentDisplay.ControlToDiagram(LastMousePos, out lastPos);
				InvalidateConnectionTargets(lastPos.X, lastPos.Y, currPos.X, currPos.Y);

				// snap to connectionPoints in range
				//GetDistanceToNearestControlPoint(SelectedShape, SelectedPointId, distanceX, distanceY, out snapDeltaX, out snapDeltaY, out snapPtId, ControlPointCapability.AttachGluePointToConnectionPoint);
				//Previews[0].MoveControlPointBy(SelectedPointId, snapDeltaX, snapDeltaY, Modifiers.None);
			}
			if (CurrentDisplay.SnapToGrid)
				InvalidateSnapIndicators(PreviewAtCursor);

			return result;
		}


		// apply the resize action
		private bool PerformMoveHandle() {
			bool result = false;
			if (StartMousePos != CurrentMousePos) {
				// invalidate original shapes including oparents and control points at their current position
				InvalidateShapes(CurrentDisplay.SelectedShapes);
				InvalidateSnapIndicators(PreviewAtCursor);

				Point startPos = Point.Empty;
				Point currPos = Point.Empty;
				CurrentDisplay.ControlToDiagram(StartMousePos, out startPos);
				CurrentDisplay.ControlToDiagram(CurrentMousePos, out currPos);
				int distanceX, distanceY;
				distanceX = currPos.X - startPos.X;
				distanceY = currPos.Y - startPos.Y;

				// if the moved ControlPoint is a single GluePoint, snap to ConnectionPoints
				snapDeltaX = snapDeltaY = 0;
				bool isGluePoint = false;
				if (CurrentDisplay.SelectedShapes.Count == 1) 
					isGluePoint = SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Glue);
				
				// Snap to Grid or ControlPoint
				Shape targetShape = null;
				int targetPointId = ControlPointId.None;
				if (isGluePoint) {
					// Find a ControlPoint to snap/connect to
					targetShape = GetDistanceToNearestControlPoint(SelectedShapeAtCursor, SelectedControlPointAtCursor, distanceX, distanceY, out snapDeltaX, out snapDeltaY, out targetPointId, ControlPointCapability.Connect);
					// If no 'real' ControlPoint was found, search shapes for a Point-To-Shape connection
					Point p = SelectedShapeAtCursor.GetControlPointPosition(SelectedControlPointAtCursor);
					if (targetShape == null && targetPointId == ControlPointId.None) {
						foreach (Shape shape in CurrentDisplay.Diagram.Shapes.FindShapesFromPosition(p.X + distanceX, p.Y + distanceY, 0, ControlPointCapability.All)) {
							if (CurrentDisplay.SelectedShapes.Contains(shape)) continue;
							targetShape = shape;
							break;
						}
					}
				} else
					GetDistanceToNearestSnapPoint(SelectedShapeAtCursor, SelectedControlPointAtCursor, distanceX, distanceY, out snapDeltaX, out snapDeltaY);
				distanceX += snapDeltaX;
				distanceY += snapDeltaY;

				if (isGluePoint){
					ICommand cmd = new MoveGluePointCommand(SelectedShapeAtCursor, SelectedControlPointAtCursor, targetShape, distanceX, distanceY, CurrentModifiers);
					CurrentDisplay.Project.ExecuteCommand(cmd);

					Point lastPos = Point.Empty;
					CurrentDisplay.ControlToDiagram(LastMousePos, out lastPos);
					InvalidateConnectionTargets(lastPos.X, lastPos.Y, currPos.X, currPos.Y);
				}
				else {
					ICommand cmd = new MoveControlPointCommand(CurrentDisplay.SelectedShapes, SelectedControlPointAtCursor, distanceX, distanceY, CurrentModifiers);
					CurrentDisplay.Project.ExecuteCommand(cmd);
				}
				
				snapDeltaX = snapDeltaY = 0;
				snapPtId = ControlPointId.None;
				result = true;
			}
			return result;
		}

		#endregion


		#region Rotating Shapes

		private int CalcAngle(int deltaX, int deltaY) {
			int result = (3600 + Geometry.RadiansToTenthsOfDegree((float)Math.Atan2(deltaY, deltaX))) % 3600;
			if ((CurrentModifiers & Modifiers.MaintainAspect) != 0 && (CurrentModifiers & Modifiers.MirroredResize) != 0) {
				// rotate by tenths of degrees
				// do nothing 
			}
			else if ((CurrentModifiers & Modifiers.MaintainAspect) != 0) {
				// rotate by full degrees
				result -= (result % 10);
			}
			else if ((CurrentModifiers & Modifiers.MirroredResize) != 0) {
				// rotate by 5 degrees
				result -= (result % 50);
			}
			else {
				// default:
				// rotate by 15 degrees
				result -= (result % 150);
			}
			return result;
		}


		// prepare drawing preview of rotate action
		private bool PrepareRotatePreview() {
			bool result = true;
			CreatePreviewShapes();

			int distanceX, distanceY;
			CurrentDisplay.ControlToDiagram(CurrentMousePos.X - StartMousePos.X, out distanceX);
			CurrentDisplay.ControlToDiagram(CurrentMousePos.Y - StartMousePos.Y, out distanceY);

			currAngle = CalcAngle(distanceX, distanceY);

			// ToDo: Implement rotation around a common rotation center
			Point rotationCenter = Point.Empty;
			foreach (Shape selectedShape in CurrentDisplay.SelectedShapes) {
				Shape previewShape = FindPreviewShape(selectedShape);
				rotationCenter.X = previewShape.X;
				rotationCenter.Y = previewShape.Y;

				// restore original shapeAngle
				if (previewShape is IPlanarShape)
				   ((IPlanarShape)previewShape).Angle = ((IPlanarShape)selectedShape).Angle;
				else  previewShape.Rotate(-lastAngle, previewShape.X, previewShape.Y);

				// perform rotation 
				if (Geometry.DistancePointPoint(CurrentMousePos.X, CurrentMousePos.Y, StartMousePos.X, startMousePos.Y) > minRotateDistance) {
					if (previewShape is IPlanarShape)
						((IPlanarShape)previewShape).Angle += currAngle;
					else previewShape.Rotate(currAngle, rotationCenter.X, rotationCenter.Y);
				}
			}
			lastAngle = currAngle;
			return result;
		}


		// apply rotate action
		private bool PerformRotate() {
			bool result = false;
			if (StartMousePos != CurrentMousePos) {
				if (Geometry.DistancePointPoint(CurrentMousePos.X, CurrentMousePos.Y, StartMousePos.X, StartMousePos.Y) > minRotateDistance) {
					int distanceX, distanceY;
					CurrentDisplay.ControlToDiagram(CurrentMousePos.X - StartMousePos.X, out distanceX);
					CurrentDisplay.ControlToDiagram(CurrentMousePos.Y - StartMousePos.Y, out distanceY);

					DisconnectGluePoints();

					int angle = CalcAngle(distanceX, distanceY);
					ICommand cmd = new RotateShapesCommand(CurrentDisplay.SelectedShapes, angle);
					CurrentDisplay.Project.ExecuteCommand(cmd);
				}
				InvalidateAnglePreview();
				result = true;
			}
			return result;
		}


		/// <summary>
		/// Specifies if a double click on the rotation handle will rotate the shape by 90°
		/// </summary>
		public bool EnableQuickRotate {
			get { return enableQuickRotate; }
			set { enableQuickRotate = value; }
		}


		private bool PerformQuickRotate(int angle) {
			bool result = false;
			if (enableQuickRotate) {
				ICommand cmd = new RotateShapesCommand(CurrentDisplay.SelectedShapes, angle);
				CurrentDisplay.Project.ExecuteCommand(cmd);
				InvalidateAnglePreview();
				result = true;
			}
			return result;
		}

		
		private void InvalidateAnglePreview() {
			// invalidate previous shapeAngle preview
			CurrentDisplay.InvalidateDiagram(rectBuffer.X - rectBuffer.Width - CurrentDisplay.HandleSize, rectBuffer.Y - rectBuffer.Height - CurrentDisplay.HandleSize, rectBuffer.Width + rectBuffer.Width + CurrentDisplay.HandleSize + CurrentDisplay.HandleSize, rectBuffer.Height + rectBuffer.Height + CurrentDisplay.HandleSize + CurrentDisplay.HandleSize);

			int length = (int)Math.Round(Geometry.DistancePointPoint(StartMousePos.X, StartMousePos.Y, CurrentMousePos.X, CurrentMousePos.Y));
			if (length > minRotateDistance) {
				// invalidate current shapeAngle preview
				rectBuffer.X = StartMousePos.X;
				rectBuffer.Y = StartMousePos.Y;
				rectBuffer.Width = length;
				rectBuffer.Height = length;
				CurrentDisplay.ControlToDiagram(rectBuffer, out rectBuffer);
				CurrentDisplay.InvalidateDiagram(rectBuffer.X - rectBuffer.Width, rectBuffer.Y - rectBuffer.Height, rectBuffer.Width + rectBuffer.Width, rectBuffer.Height + rectBuffer.Height);
			}
			else {
				// invalidate instruction preview
				rectBuffer.X = StartMousePos.X;
				rectBuffer.Y = StartMousePos.Y;
				rectBuffer.Width = minRotateDistance;
				rectBuffer.Height = minRotateDistance;
				CurrentDisplay.ControlToDiagram(rectBuffer, out rectBuffer);
				CurrentDisplay.InvalidateDiagram(rectBuffer.X - rectBuffer.Width, rectBuffer.Y - rectBuffer.Height, rectBuffer.Width + rectBuffer.Width, rectBuffer.Height + rectBuffer.Height);
			}
		}

		#endregion


		#region Text Editor

		private void ShowTextEditor(string pressedKey) {
			// show TextEditor
			if (CurrentDisplay.SelectedShapes.Count == 1) {
				if (CurrentDisplay.SelectedShapes.TopMost is ICaptionedShape) {
					ICaptionedShape labeledShape = (ICaptionedShape)CurrentDisplay.SelectedShapes.TopMost;
					if (labeledShape.CaptionCount > 0) CurrentDisplay.OpenCaptionEditor(labeledShape, 0, pressedKey);
				}
			}
		}

		#endregion


		#region Private Properties

		/// <summary>
		/// Untransformed start coordinates of the mouse action.
		/// </summary>
		private Point StartMousePos {
		   get { return startMousePos; }
		}


		/// <summary>
		/// The preview shape of the selected shape under the mouse cursor, the one that is manipulated directly
		/// </summary>
		protected Shape PreviewAtCursor {
			get { return previewUnderCursor; }
		}


		protected Shape SelectedShapeAtCursor {
			get { return selectedShapeAtCursor; }
			private set {
				if (selectedShapeAtCursor != value) {
					if (selectedShapeAtCursor != null) selectedShapeAtCursor.Invalidate();
					selectedShapeAtCursor = value;
					if (selectedShapeAtCursor != null) selectedShapeAtCursor.Invalidate();
				}
			}
		}


		protected int SelectedControlPointAtCursor {
			get {return selectedControlPointAtCursor;}
			private set { selectedControlPointAtCursor = value; }

		}


		protected int SelectedCaptionIndexAtCursor {
			get { return selectedCaptionIndexAtCursor; }
			private set {
				if (selectedCaptionIndexAtCursor != value) {
					selectedCaptionIndexAtCursor = value;
					if (selectedShapeAtCursor != null) selectedShapeAtCursor.Invalidate();
				}
			}

		}


		protected Shape DiagramShapeAtCursor {
			get { return diagramShapeAtCursor; }
		}


		protected int DiagramControlPointAtCursor {
			get { return diagramControlPointAtCursor; }
		}

		#endregion


		#region Helper Methods

		private void SetSelectedShapeAtCursor(int mouseX, int mouseY, int handleRadius, ControlPointCapability handleCapabilities) {
			// check if the selected shape under the previous cursor position is still under the cursor
			if (SelectedShapeAtCursor != null 
				&& !ShapeOrShapeRelativesContainsPoint(SelectedShapeAtCursor, mouseX, mouseY, handleRadius, handleCapabilities))
				SelectedShapeAtCursor = null;

			// find the shape under the cursor
			if (SelectedShapeAtCursor == null)
				SelectedShapeAtCursor = CurrentDisplay.SelectedShapes.FindShapeFromPosition(mouseX, mouseY, handleRadius, handleCapabilities, null);
			if (SelectedShapeAtCursor != null) {
				// if the found shape is not in the Collection of selectedShapes, get its parent shape
				if (!CurrentDisplay.SelectedShapes.Contains(SelectedShapeAtCursor))
					SelectedShapeAtCursor = SelectedShapeAtCursor.Parent ?? SelectedShapeAtCursor;

				// Find the controlpoint under the mouse cursor that belongs to selected shape under the cursor
				SelectedControlPointAtCursor = SelectedShapeAtCursor.GetNearestControlPoint(mouseX, mouseY, ZoomedHandleSizeDiagram, ControlPointCapability.All);
				if (SelectedShapeAtCursor is ICaptionedShape && ((ICaptionedShape)SelectedShapeAtCursor).CaptionCount > 0)
					SelectedCaptionIndexAtCursor = ((ICaptionedShape)SelectedShapeAtCursor).FindCaptionFromPoint(mouseX, mouseY);
			} else {
				SelectedControlPointAtCursor = ControlPointId.None;
				SelectedCaptionIndexAtCursor = -1;
			}
		}


		private bool ShapeOrShapeRelativesContainsPoint(Shape shape, int x, int y, int distance, ControlPointCapability capabilities) {
			if (shape.ContainsPoint(x, y, distance, capabilities))
				return true;
			else if (shape.Parent != null) {
				if (ShapeOrShapeRelativesContainsPoint(shape.Parent, x, y, distance, capabilities))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Create Previews of all shapes selected in the CurrentDisplay.
		/// These previews are connected to all the shapes the original shapes are connected to.
		/// </summary>
		private void CreatePreviewShapes() {
			if (Previews.Count == 0) {
				// first, clone all selected shapes...
				foreach (Shape shape in CurrentDisplay.SelectedShapes)
					AddPreview(shape, shape.ShapeType.CreatePreviewInstance(shape));
				// ...then restore connections between previews and connections between previews and non-selected shapes
				targetShapeBuffer.Clear();
				foreach (Shape selectedShape in CurrentDisplay.SelectedShapes.BottomUp) {
					// AttachGluePointToConnectionPoint the preview shape (and all it's cildren) to all the shapes the original shape was connected to
					// Additionally, create previews for all connected shapes and connect these to the appropriate target shapes
					ConnectPreviewShape(selectedShape);
				}
				targetShapeBuffer.Clear();
				previewUnderCursor = FindPreviewShape(SelectedShapeAtCursor);
			}
		}


		private void InvalidateShapes(IEnumerable<Shape> shapes) {
			bool invalidatePoints = (CurrentDisplay != null);
			foreach (Shape shape in shapes) {
				DoInvalidateShape(shape, invalidatePoints);
			}
		}


		private void DoInvalidateShape(Shape shape, bool invalidateControlPoints) {
			if (shape.Parent != null)
				DoInvalidateShape(shape.Parent, invalidateControlPoints);
			else {
				shape.Invalidate();
				if (invalidateControlPoints)
					CurrentDisplay.InvalidateControlPoints(shape, ControlPointCapability.All);
			}
		}


		/// <summary>
		/// Create previews of shapes connected to the given shape (and it's children) and connect them to the
		/// shape's preview (or the preview of it's child)
		/// </summary>
		/// <param projectName="shape">The original shape which contains all ConnectionInfo</param>
		private void ConnectPreviewShape(Shape shape) {
			// process shape's children
			if (shape.Children != null && shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					ConnectPreviewShape(childShape);
			}

			Shape preview = FindPreviewShape(shape);
			foreach (ShapeConnectionInfo connectionInfo in shape.GetConnectionInfos(null, ControlPointId.Any)) {
				if (CurrentDisplay.SelectedShapes.Contains(connectionInfo.PassiveShape)) {
					// Do not connect previews if BOTH of the connected shapes are part of the selection because 
					// this would restrict movement of the connector shapes and decreases performance (many 
					// unnecessary FollowConnectionPointWithGluePoint() calls)
					if (shape.HasControlPointCapability(connectionInfo.GluePointId, ControlPointCapability.Glue)) {
						if (IsConnectedToNonSelectedShapes(shape)) {
							Shape targetPreview = FindPreviewShape(connectionInfo.PassiveShape);
							preview.Connect(connectionInfo.GluePointId, targetPreview, connectionInfo.ConnectionPointId);
						}
					}
				} else {
					if (preview.HasControlPointCapability(connectionInfo.GluePointId, ControlPointCapability.Glue)) {
						// connect preview to non-selected shape
						if (shape != SelectedShapeAtCursor && connectionInfo.GluePointId != SelectedControlPointAtCursor)
							preview.Connect(connectionInfo.GluePointId, connectionInfo.PassiveShape, connectionInfo.ConnectionPointId);
					} else {
						// create a preview of the shape that is connected to the preview (recursive call)
						CreateConnectedTargetPreviewShape(preview, connectionInfo);
					}
				} // End of block 'Check if a preview of the connection's target shape was created'
			}	// End of block 'Process all connections'
		}


		/// <summary>
		/// Creates (or finds) a preview of the connection's PassiveShape and connects it to the current preview shape
		/// </summary>
		/// <param projectName="previewShape">The preview shape</param>
		/// <param projectName="connectionInfo">ConnectionInfo of the preview's original shape</param>
		private void CreateConnectedTargetPreviewShape(Shape previewShape, ShapeConnectionInfo connectionInfo) {
			// check if any other selected shape is connected to the same non-selected shape
			Shape previewTargetShape = null;
			if (targetShapeBuffer.ContainsKey(connectionInfo.PassiveShape)) {
				// if the current passiveShape is already connected to another shape of the current selection,
				// connect the ObjectRef preview to the other preview's passiveShape
				previewTargetShape = targetShapeBuffer[connectionInfo.PassiveShape];
			} else {
				// if the current passiveShape is not connected to any other of the selected selectedShapes,
				// clone the passiveShape and connect it to the appropriate preview
				previewTargetShape = connectionInfo.PassiveShape.ShapeType.CreatePreviewInstance(connectionInfo.PassiveShape);
				AddPreview(connectionInfo.PassiveShape, previewTargetShape);
				// add passiveShape and it's clone to the passiveShape dictionary
				targetShapeBuffer.Add(connectionInfo.PassiveShape, previewTargetShape);
			}
			// connect the (new or existing) preview shapes
			if (previewTargetShape != null) {
				previewTargetShape.Connect(connectionInfo.ConnectionPointId, previewShape, connectionInfo.GluePointId);

				// check, if any shapes are connected to the connector (that is connected to the selected shape)
				foreach (ShapeConnectionInfo connectorCI in connectionInfo.PassiveShape.GetConnectionInfos(null, ControlPointId.Any)) {
					if (connectorCI.GluePointId != connectionInfo.ConnectionPointId) {
						// connect connectors connected to the previewTargetShape
						if (connectorCI.PassiveShape.HasControlPointCapability(connectorCI.ConnectionPointId, ControlPointCapability.Glue))
							CreateConnectedTargetPreviewShape(previewTargetShape, connectorCI);
						// connect the other end of the previewTargetShape if the connection is a Point-To-Shape connection
						else if (connectorCI.ConnectionPointId == ControlPointId.Reference)
							previewTargetShape.Connect(connectorCI.GluePointId, connectorCI.PassiveShape, connectorCI.ConnectionPointId);
					}
				}
			} else throw new DiagrammingException("Error while creating connected preview shapes.");
		}


		protected override void ClearPreviews() {
			base.ClearPreviews();
			previewUnderCursor = null;
		}


		private bool IsMoveHandleAllowed() {
			if (!CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes))
				return false;
			if (CurrentDisplay.SelectedShapes.Count > 1) {
				Shape lastShape = null;
				foreach (Shape shape in CurrentDisplay.SelectedShapes) {
					if (lastShape != null) {
						if (shape.ShapeType != lastShape.ShapeType)
							// if the shapes are not of the same shapetype, do not allow to move the resize handle
							return false;
					} else if (shape is ILinearShape && SelectedControlPointAtCursor != 1 && SelectedControlPointAtCursor != 2)
						// if the selected shapes are lines, do not allow move handle if other control points 
						// than the start- or endpoint should be moved
						return false;
					lastShape = shape;
				}
			}
			return true;
		}


		private bool IsRotatatingAllowed() {
			if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes)) {
				if (CurrentDisplay.SelectedShapes.Count > 1) {
					// check if all selected shapes have a rotate handle
					foreach (Shape selectedShape in CurrentDisplay.SelectedShapes) {
						bool shapeHasRotateHandle = false;
						foreach (int ptId in selectedShape.GetControlPointIds(ControlPointCapability.Rotate)) {
							shapeHasRotateHandle = true;
							break;
						}
						if (!shapeHasRotateHandle) return false;
					}
				}
				return true;
			}
			return false;
		}


		/// <summary>
		/// Decide which tool action is suitable
		/// </summary>
		/// <param projectName="eventType"></param>
		/// <returns></returns>
		private ToolAction DeterminePointerAction(MouseEventType eventType) {
			ToolAction result = action;
			switch (eventType) {
				case MouseEventType.MouseDown:
					if (SelectedShapeAtCursor != null && SelectedCaptionIndexAtCursor >= 0) {
						// if the cursor is not over a caption of a selected shape assume the user wants
						// to select something
						result = ToolAction.EditCaption;
					} else
						result = ToolAction.Select;
					break;
				case MouseEventType.MouseMove:
					if (leftMouseButtonDown) {
						if (SelectedShapeAtCursor != null) {
							// decide what to do with the selected shape
							if (SelectedControlPointAtCursor != ControlPointId.None
								&& SelectedControlPointAtCursor != ControlPointId.Reference
								&& action != ToolAction.MoveShape) {
								// if a valid control point is under the cursor, check the controlPoint's capabilities
								if (SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Rotate)) {
									// check if rotating is possible
									if (IsRotatatingAllowed()) result = ToolAction.Rotate;
								} else {
									// check if resizing is possible
									if (IsMoveHandleAllowed()) result = ToolAction.MoveHandle;
								}
							} else {
								if (SelectedCaptionIndexAtCursor >= 0)
									result = ToolAction.EditCaption;
								else result = ToolAction.MoveShape;
							}
						} else if (DiagramShapeAtCursor != null && (action == ToolAction.None || action == ToolAction.Select)) {
							// if there's a non-selected shape under the mouse cursor, move it (it will be selected automatically
							result = ToolAction.MoveShape;
						} else if (action == ToolAction.Select) {
							// if the mouse button is pressed but there is no selected shape under the cursor, 
							// assume that this is a frame selection
							result = ToolAction.SelectWithFrame;
						}
					} else
						// if left MouseButton is not pressed:
						if (SelectedShapeAtCursor != null && SelectedCaptionIndexAtCursor >= 0)
							// if thre's a Selected shape and a caption under the cursor, assume that the user wants to edit the caption
							result = ToolAction.EditCaption;
						else
							// if there is no selected shape under the cursor, assume that the user wants to select something
							result = ToolAction.Select;
					break;
				default:
					// no change
					break;
			}
			return result;
		}


		// apply different mouse cursors depending on the HotSpot under the mouse
		private void SetCursor() {
			currentCursor = null;
			// if no selected shape is under the mouse cursor, there is nothing to manipulate
			if (SelectedShapeAtCursor == null) {
				currentCursor = defaultCursor;
			} else if (SelectedShapeAtCursor != null && CurrentDisplay.SelectedShapes.Contains(SelectedShapeAtCursor)) {
				// transform mouse coordinates (given in control coordinates) to diagram coordinates
				int mouseX, mouseY;
				CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);
				switch (SelectedControlPointAtCursor) {
					case ControlPointId.None:
					case ControlPointId.Reference:
						// If the nearest ControlPoint is NotSupported (or the Reference-Point, which is not a 'real' control point)
						// the following actions are possible:
						// - Edit a caption (if mouse cursor is over a caption)
						// - Move the shape (if mouse cursor is over the shape)
						// - Do nothing (if the mouse cursor is not over the shape)
						if (SelectedShapeAtCursor is ICaptionedShape
							&& ((ICaptionedShape)SelectedShapeAtCursor).CaptionCount > 0
							&& ((ICaptionedShape)SelectedShapeAtCursor).FindCaptionFromPoint(mouseX, mouseY) >= 0) {
							if (CurrentDisplay.Project.Security.IsGranted(Permission.ModifyData, SelectedShapeAtCursor))
								currentCursor = editTextCursor;
							else currentCursor = defaultCursor;
						} else if (SelectedShapeAtCursor.ContainsPoint(mouseX, mouseY)) {
							if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes))
								currentCursor = moveShapeCursor;
							else currentCursor = defaultCursor;
						} else currentCursor = defaultCursor;
						break;
					default:
						if (leftMouseButtonDown && PreviewAtCursor != null) {
							// If left MouseButton is down and a Preview for the SelectedShape exists, the following actions are 
							// possible:
							// - Rotating the shape 
							// - Resizing the shape 
							// - Connecting shapes
							// - Moving shapes
							if (SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Rotate)) {
								// ControlPoint is a rotation handle
								currentCursor = rotateCursor;
							} else if (SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Glue)) {
								// ControlPoint is a GluePoint - check if any shapes to connect to are under the mouse cursor
								if (DiagramShapeAtCursor != null && DiagramControlPointAtCursor != ControlPointId.None
									&& (action == ToolAction.MoveHandle || DiagramShapeAtCursor.GetConnectionInfo(null, DiagramControlPointAtCursor) != ShapeConnectionInfo.Empty))
									currentCursor = connectCursor;
								else currentCursor = moveHandleCursor;
							} else if (SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Resize)) {
								// ControlPoint is a ResizeHandle
								currentCursor = moveHandleCursor;
							} else if (SelectedShapeAtCursor.ContainsPoint(mouseX, mouseY)) {
								currentCursor = moveShapeCursor;
							} else
								currentCursor = defaultCursor;
						} else {
							// If the nearest control point is a valid ('real') control point, the following actions are 
							// possible:
							// - Rotate the shape (if mouse cursor is over a rotation control point)
							// - Resize the shape / connect shapes (if mouse cursor is over a resize handle / GluePoint)
							// - Move the shape
							// - Do nothing
							pointbuffer = SelectedShapeAtCursor.GetControlPointPosition(SelectedControlPointAtCursor);
							float distance = Geometry.DistancePointPoint(pointbuffer.X, pointbuffer.Y, mouseX, mouseY);
							// Check if the mouse corsor is in range of the ControlPoint and check ControlPoint's Capabilities:
							if (distance <= ZoomedHandleSizeReal && SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Rotate)) {
								// ControlPoint is a rotation handle
								if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes)) {
									if (IsRotatatingAllowed()) currentCursor = rotateCursor;
									else currentCursor = actionDeniedCursor;
								} else currentCursor = defaultCursor;
							} else if (distance <= ZoomedHandleSizeReal && SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Glue)) {
								// ControlPoint is a GluePoint
								if (IsMoveHandleAllowed()) {
									if (DiagramShapeAtCursor != null && DiagramControlPointAtCursor != ControlPointId.None
										&& (action == ToolAction.MoveHandle || DiagramShapeAtCursor.GetConnectionInfo(null, DiagramControlPointAtCursor) != ShapeConnectionInfo.Empty))
										currentCursor = connectCursor;
									else currentCursor = moveHandleCursor;
								} else currentCursor = defaultCursor;
							} else if (distance <= ZoomedHandleSizeReal && SelectedShapeAtCursor.HasControlPointCapability(SelectedControlPointAtCursor, ControlPointCapability.Resize)) {
								// ControlPoint is a ResizeHandle
								if (IsMoveHandleAllowed())
									currentCursor = moveHandleCursor;
								else currentCursor = defaultCursor;
							} else if (SelectedShapeAtCursor.ContainsPoint(mouseX, mouseY)) {
								if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout, CurrentDisplay.SelectedShapes))
									currentCursor = moveShapeCursor;
								else currentCursor = defaultCursor;
							}
						}
						break;
				}
			}
		}

		#endregion


		#region Construction

		private void BaseConstruct() {
			Title = "Pointer";
			ToolTipText = "Select one or more objects by clicking or drawing a frame.\n\rSelected objects can be moved by dragging them to the target position or resized by dragging a control point to the target position.";

			SmallIcon = global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.PointerIconSmall;
			SmallIcon.MakeTransparent(Color.Fuchsia);

			LargeIcon = global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.PointerIconLarge;
			LargeIcon.MakeTransparent(Color.Fuchsia);

			moveShapeCursor = LoadCursorFromResource(global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.MoveShapeCursor);
			moveHandleCursor = LoadCursorFromResource(global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.MovePointCursor);
			rotateCursor = LoadCursorFromResource(global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.RotateCursor);
			defaultCursor  = Cursors.Default;
			actionDeniedCursor  = Cursors.No;
			editTextCursor = Cursors.IBeam;
			connectCursor = disconnectCursor = Cursors.Hand;

			formatter.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
			formatter.Trimming = StringTrimming.EllipsisCharacter;

			frameRect = new Rectangle();
			frameBrush = new SolidBrush(backColor);
			framePen = new Pen(frameColor, frameLineWidth);
		}

		#endregion


		#region Fields
		// general stuff
		private bool leftMouseButtonDown;		// indicates if the mouseButton is still pressed (true) or if it was released meanwhile (false)
		private ToolAction action;					// indicates the ObjectRef action depending on the mouseButton State, selected selectedShapes and mouse movement
		private Point startMousePos;				// untransformed start coordinates of the drag action (control coordinates)

		private int snapDeltaX;						// stores the distance the SelectedShape was moved on X-axis for snapping the nearest gridpoint
		private int snapDeltaY;						// stores the distance the SelectedShape was moved on Y-axis for snapping the nearest gridpoint
		private int snapPtId;						// index of the controlPoint that snapped to grid/point/swimline
		
		private Shape selectedShapeAtCursor;			// selected shape under the mouse cursor (the one that is manipulated directly)
		private int selectedControlPointAtCursor;		// ControlPointId of selected shape's ControlPoint under the mouse cursor
		private int selectedCaptionIndexAtCursor;		// Index of selected shape's caption under the mouse cursor

		private Shape diagramShapeAtCursor;			// non-selected shape under the mouse cursor
		private int diagramControlPointAtCursor;		// ControlPointId of non-selected shape's ControlPoint under the mouse cursor

		// Buffers
		private Rectangle rectBuffer;		// rectangle buffer 
		private Point pointbuffer;			// buffer used for transforming coordinates

		// Cursors
		private Cursor currentCursor;
		private Cursor rotateCursor;
		private Cursor moveHandleCursor;
		private Cursor moveShapeCursor;
		private Cursor defaultCursor;
		private Cursor actionDeniedCursor;
		private Cursor editTextCursor;
		private Cursor connectCursor;
		private Cursor disconnectCursor;

		// selection frame stuff
		private Rectangle frameRect;		// rectangle that represents the transformed selection area in control coordinates
		private int frameLineWidth = 1;
		private Color frameColor = Color.FromArgb(96, Color.SteelBlue);
		private Color backColor = Color.FromArgb(64, Color.LightSlateGray);
		private SolidBrush frameBrush;
		private Pen framePen;
		private Point[] captionBoundsBuffer = new Point[4];

		// rotating stuff
		private bool enableQuickRotate = false;
		private int lastAngle = 0;
		private int currAngle = 0;
		private const int minRotateDistance = 30;
		private StringFormat formatter = new StringFormat();
		private Point[] arrowShape = new Point[3];

		// preview stuff
		private Shape previewUnderCursor = null;		// the preview clone of the Shape under the mouse cursor (used for snapping to grid and points)
		private Dictionary<Shape, Shape> targetShapeBuffer = new Dictionary<Shape, Shape>();	// used for buffering selectedShapes connected to the preview selectedShapes: key = passiveShape, values = targetShapes's clone

		// connection handling stuff
		private struct ConnectionInfoBuffer {

			public static readonly ConnectionInfoBuffer Empty;
			
			public static bool operator ==(ConnectionInfoBuffer x, ConnectionInfoBuffer y) { return (x.connectionInfo == y.connectionInfo && x.shape == y.shape); }
			
			public static bool operator !=(ConnectionInfoBuffer x, ConnectionInfoBuffer y) { return !(x == y); }			
			
			public Shape shape;
			
			public ShapeConnectionInfo connectionInfo;
			
			public override bool Equals(object obj) { return obj is ConnectionInfoBuffer && this == (ConnectionInfoBuffer)obj; }
			
			public override int GetHashCode() { return base.GetHashCode(); }
			
			static ConnectionInfoBuffer() {
				Empty.shape = null;
				Empty.connectionInfo = ShapeConnectionInfo.Empty;
			}

		}
		private List<ConnectionInfoBuffer> connectionsBuffer = new List<ConnectionInfoBuffer>();	// buffer used for storing connections that are temporarily disconnected for moving shapes
		#endregion
	}
	
	#endregion

	
	#region LinearShapeCreationTool

	public class LinearShapeCreationTool : TemplateTool {

		private enum ToolAction { None, DrawLine, AddPoint, MovePoint }

		public LinearShapeCreationTool(Template template, IDisplayService displayService)
			: base(template, displayService) {
			BaseConstruct(template);
		}


		public LinearShapeCreationTool(Template template, IDisplayService displayService, string category)
			: base(template, displayService, category) {
			BaseConstruct(template);
		}


		#region IDisposable Interface
		
		public override void Dispose() {
			base.Dispose();

			//if (defaultCursor != null) defaultCursor.Dispose();
			//if (notAllowedCursor != null) notAllowedCursor.Dispose();
			//if (movePointCursor != null) movePointCursor.Dispose();
			//if (penCursor != null) penCursor.Dispose();
			//if (connectCursor != null) connectCursor.Dispose();
			//if (disconnectCursor != null) disconnectCursor.Dispose();
			//if (currentCursor != null) currentCursor.Dispose();
		}
		
		#endregion


		#region ITool Interface

		public override void Refresh() {
			Template.Shape.DrawThumbnail(base.SmallIcon, margin, transparentColor);
			base.SmallIcon.MakeTransparent(transparentColor);
			Template.Shape.DrawThumbnail(base.LargeIcon, margin, transparentColor);
			base.LargeIcon.MakeTransparent(transparentColor);
		}


		public override Cursor Cursor {
			get { return currentCursor; }
		}


		public override void Cancel() {
			// Create the line until the last point that was created manually.
			// This feature only makes sense if an additional ControlPoint was created (other than the default points)
			if (PreviewShape != null && PreviewShape.ControlPointCount > Template.Shape.ControlPointCount)
				FinishLine(true);
			ResetToolState();
			if (CurrentDisplay != null)
				CurrentDisplay.Cursor = defaultCursor;			

			base.Cancel();
		}


		public override bool ExecuteMouseAction(IDisplay display, DiagrammingMouseEventArgs e) {
			bool result = false;
			if (base.ExecuteMouseAction(display, e)) {
				shapeAtCursor = null;
				pointAtCursor = ControlPointId.None;
				shapeAtCursor = FindNearestShape(out pointAtCursor);
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						highlightConnectionTargets = true;
						if (LastMousePos != CurrentMousePos) {
							if (PreviewShape != null) {
								// check for connectionpoints wihtin the snapArea
								if (shapeAtCursor != null && pointAtCursor > 0) {
									Point p = Point.Empty;
									p = shapeAtCursor.GetControlPointPosition(pointAtCursor);
									PreviewShape.MoveControlPointTo(EndPointId, p.X, p.Y, CurrentModifiers);
								}
								else {
									int mouseX, mouseY;
									CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);
									if (CurrentDisplay.SnapToGrid) {
										int snapDeltaX, snapDeltaY;
										GetDistanceToNearestSnapPoint(mouseX, mouseY, out snapDeltaX, out snapDeltaY);
										mouseX += snapDeltaX;
										mouseY += snapDeltaY;
									}
									PreviewShape.MoveControlPointTo(EndPointId, mouseX, mouseY, CurrentModifiers);
								}
							}
							InvalidatePreview();
						}
						break;
					case MouseEventType.MouseDown:
						// MouseDown starts drag-based actions
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						break;
					case MouseEventType.MouseUp:
						// MouseUp finishes drag-actions. Click-based actions are handled by the MouseClick event
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						break;
					case MouseEventType.MouseClick:
						// If no other ToolAction is in Progress (e.g. drawing a line or moving a point),
						// a normal MouseClick starts a new line in Point-By-Point mode
						if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
							if ((e.Buttons & DiagrammingMouseButtons.Left) != 0) {
								if (CurrentDisplay.Capture)
									CurrentDisplay.Capture = false;

								// Determine ToolAction
								if (action == ToolAction.None) {
									startMousePos = e.Position;
									StartLine(shapeAtCursor, pointAtCursor);
									action = ToolAction.AddPoint;
								}
								else if (action == ToolAction.AddPoint)
									InsertNewPoint();
								InvalidatePreview();
							}
						}
						break;
					case MouseEventType.MouseDoubleClick:
						// A DoubleClick finishes the current Point-By-Point line
						if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert))
							FinishLine(false);
						break;
					case MouseEventType.MouseLeave:
						highlightConnectionTargets = false;
						InvalidatePreview();
						break;
					case MouseEventType.MouseEnter:
						highlightConnectionTargets = true;
						break;
					default:
						throw new DiagrammingUnexpectedValueException(e.EventType);
				}
				// set cursor depending on the object under the mouse cursor
				SetCursor(shapeAtCursor, pointAtCursor);
				result = true;
			}
			return result;
		}


		public override void InvalidatePreview() {
			if (CurrentDisplay != null) {
				int lastMousePosX, lastMousePosY, currMousePosX, currMousePosY;
				CurrentDisplay.ControlToDiagram(LastMousePos.X, LastMousePos.Y, out lastMousePosX, out lastMousePosY);
				CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out currMousePosX, out currMousePosY);
				InvalidateConnectionTargets(lastMousePosX, lastMousePosY, currMousePosX, currMousePosY);
				if (PreviewShape != null) PreviewShape.Invalidate();
			}
		}


		public override void DrawPreview() {
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null) {
				// highlight ConnectionPoints in range
				if (highlightConnectionTargets) {
					int mouseX, mouseY;
					CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);
					DrawConnectionTargets(mouseX, mouseY, Previews.Values);
				}

				// draw shape and ResizeHandles
				if (PreviewShape != null) {
					PreviewShape.Draw(CurrentDisplay.Graphics);
					base.ResetZoom();
					foreach (int id in PreviewShape.GetControlPointIds(ControlPointCapability.Resize)) {
						if (id == EndPointId && shapeAtCursor != null) {
							Point gluePtPos = PreviewShape.GetControlPointPosition(EndPointId);
							bool highlightGluePt = false;
							if (pointAtCursor != ControlPointId.None && pointAtCursor != ControlPointId.Reference) {
								Point ptAtCursorPos = shapeAtCursor.GetControlPointPosition(pointAtCursor);
								highlightGluePt = ptAtCursorPos == gluePtPos;
							}
							else
								highlightGluePt = shapeAtCursor.ContainsPoint(gluePtPos.X, gluePtPos.Y);

							if (highlightGluePt) {
								CurrentDisplay.DiagramToControl(gluePtPos, out gluePtPos);
								CurrentDisplay.DrawConnectionPoint(CurrentDisplay.Graphics, PreviewShape, id, gluePtPos.X, gluePtPos.Y, DiagrammingDrawMode.Highlighted);
								continue;
							}
						}
						Point p = Point.Empty;
						p = PreviewShape.GetControlPointPosition(id);
						CurrentDisplay.DiagramToControl(p, out p);
						CurrentDisplay.DrawResizeControlPoint(CurrentDisplay.Graphics, PreviewShape, id, p.X, p.Y, DiagrammingDrawMode.Normal);
					}
					base.RestoreZoom();
				}
			}
		}

		#endregion


		private void BaseConstruct(Template template) {
			if (!(template.Shape is ILinearShape))
				throw new DiagrammingException("The template's shape does not implement {0}.", typeof(ILinearShape).Name);

			if (string.IsNullOrEmpty(template.Name))
				Title = template.Shape.ShapeType.Name;
			else Title = template.Name;
			ToolTipText = string.Format("Insert {0}.", Title);
			if (template.Shape is PolylineBase)
				ToolTipText += "\n\rPolylines are finished by double clicking.";

			defaultCursor = Cursors.Default;
			penCursor = LoadCursorFromResource(global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.PenCursor);
			movePointCursor = LoadCursorFromResource(global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.MovePointCursor);
			disconnectCursor = Cursors.Hand;
			connectCursor = Cursors.Hand;
			notAllowedCursor = Cursors.No;
			// ToDo: Create better cursors for connecting/disconnecting
			//connectCursor = new Cursor(GetType(), "Resources.ConnectCursor.cur");
			//disconnectCursor = new Cursor(GetType(), "Resources.DisconnectCursor.cur");
		}


		private Shape FindNearestShape(out int pointId) {
			Shape result = null;
			pointId = ControlPointId.None;
			// move snapArea to current mouse position
			int mouseX, mouseY;
			CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);

			float zoomedSnapDistance = CurrentDisplay.SnapDistance / (CurrentDisplay.ZoomLevel / 100f);
			foreach (Shape shape in CurrentDisplay.Diagram.Shapes.FindShapesFromPosition(mouseX, mouseY, CurrentDisplay.SnapDistance, ControlPointCapability.Connect)) {
				result = shape;
				break;
			}
			if (result != null) {
				Point p = Point.Empty;
				foreach (int ptId in result.GetControlPointIds(ControlPointCapability.Connect)) {
					p = result.GetControlPointPosition(ptId);
					if (Geometry.DistancePointPoint(p.X, p.Y, mouseX, mouseY) <= ZoomedHandleSizeReal) {
						pointId = ptId;
						break;
					}
				}
				if (pointId == ControlPointId.None && result.ContainsPoint(mouseX, mouseY))
					pointId = ControlPointId.Reference;
			}
			return result;
		}


		/// <summary>
		/// Creates a new preview line shape
		/// </summary>
		/// <param projectName="shape">The shape under/near the line's StartPoint</param>
		/// <param projectName="pointIdToConnect">The ControlPointId under/near the line's StartPoint</param>
		private void StartLine(Shape shape, int pointIdToConnect) {
			int mouseX, mouseY;
			CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);

			// create new preview shape
			ClearPreviews();
			AddPreview(Template.Shape, Template.Shape.ShapeType.CreatePreviewInstance(Template.Shape));

			// set line's start coordinates
			Point start = Point.Empty;
			if (shape != null && pointIdToConnect != ControlPointId.None) {
				if (pointIdToConnect == ControlPointId.Reference) {
					CurrentDisplay.ControlToDiagram(startMousePos, out start);
					// ToDo: Get nearest Intersection Point with line
				}
				else start = shape.GetControlPointPosition(pointIdToConnect);
			}
			else {
				CurrentDisplay.ControlToDiagram(startMousePos, out start);
				if (CurrentDisplay.SnapToGrid) {
					int snapDeltaX, snapDeltaY;
					GetDistanceToNearestSnapPoint(start.X, start.Y, out snapDeltaX, out snapDeltaY);
					start.X += snapDeltaX;
					start.Y += snapDeltaY;
				}
			}
			PreviewShape.MoveControlPointTo(StartPointId, start.X, start.Y, CurrentModifiers);
			PreviewShape.MoveControlPointTo(EndPointId, mouseX, mouseY, CurrentModifiers);
			lastInsertedPointId = StartPointId;
		}


		/// <summary>
		/// Inserts a new point into the current preview line before the end point (that is sticking to the mouse cursor).
		/// </summary>
		private void InsertNewPoint() {
			if (PreviewLinearShape != null) {
				if (PreviewLinearShape.VertexCount < PreviewLinearShape.MaxVertexCount) {
					Point pointPos = PreviewShape.GetControlPointPosition(EndPointId);
					lastInsertedPointId = PreviewLinearShape.InsertVertex(EndPointId, pointPos.X, pointPos.Y);
				} else FinishLine(false);
			}
		}
		
		
		/// <summary>
		/// Creates a new LinearShape and inserts it into the diagram of the CurrentDisplay by executing a Command.
		/// </summary>
		/// <param projectName="createWithAllPoints">If true, the line will be created as a 
		/// clone of the preview shape. If false, the line will be created until the 
		/// last point inserted. The point at the mouse cursor will be skipped.</param>
		private void FinishLine(bool cancelled) {
			Debug.Assert(PreviewShape != null);
			// Create a new shape from the template
			Shape newShape = Template.CreateShape();
			// Copy points from the PreviewShape to the new shape 
			// The current EndPoint of the preview (sticking to the mouse cursor) will be discarded
			foreach (int pointId in PreviewShape.GetControlPointIds(ControlPointCapability.Resize)) {
				Point p = PreviewShape.GetControlPointPosition(pointId);
				// skip ReferencePoint and EndPoint
				switch (pointId) {
					case ControlPointId.Reference:
						continue;
					case EndPointId:
						// If the line *has no* vertex limit, the last point (sticking to the mouse cursor) will 
						// always be discarded 
						// If the line *has a* vertex limit, the last point will be created
						// If the tool was cancelled, the ast point always will be discarded.
						if (PreviewLinearShape.VertexCount == PreviewLinearShape.MaxVertexCount && !cancelled) 
							newShape.MoveControlPointTo(EndPointId, p.X, p.Y, Modifiers.None);
						else continue;
						break;
					case StartPointId:
						newShape.MoveControlPointTo(StartPointId, p.X, p.Y, Modifiers.None);
						break;
					default:
						// treat the last inserted Point as EndPoint
						if (cancelled && pointId == lastInsertedPointId)
							newShape.MoveControlPointTo(EndPointId, p.X, p.Y, Modifiers.None);
						else {
							if (pointId == lastInsertedPointId && PreviewLinearShape.VertexCount != PreviewLinearShape.MaxVertexCount)
								newShape.MoveControlPointTo(EndPointId, p.X, p.Y, Modifiers.None);
							else ((ILinearShape)newShape).InsertVertex(EndPointId, p.X, p.Y);
						}
						break;
				}
			}

			// Create an aggregated command which performs creation of the new shape and 
			// connecting the new shapes to other shapes in one step
			AggregatedCommand aggregatedCommand = new AggregatedCommand();
			if (Template.Shape.ModelObject != null)
				aggregatedCommand.Add(new InsertShapeAndModelCommand(CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, newShape, false));
			else
				aggregatedCommand.Add(new InsertShapeCommand(newShape, CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, false, CurrentDisplay.Project.Repository));

			foreach (int ptId in newShape.GetControlPointIds(ControlPointCapability.Glue)) {
				Point p = Point.Empty;
				p = newShape.GetControlPointPosition(ptId);
				Shape targetShape = CurrentDisplay.Diagram.Shapes.FindShapeFromPosition(p.X, p.Y, (int)Math.Ceiling(ZoomedHandleSizeReal), ControlPointCapability.Connect, null);
				if (targetShape != null) {
					int targetPtId = targetShape.GetNearestControlPoint(p.X, p.Y, 0, ControlPointCapability.Connect);
					if (targetPtId > 0)
						aggregatedCommand.Add(new ConnectCommand(newShape, ptId, targetShape, targetPtId));
					else
						aggregatedCommand.Add(new ConnectCommand(newShape, ptId, targetShape, ControlPointId.Reference));
				}
			}
			// execute command and insert it into the history
			CurrentDisplay.Project.ExecuteCommand(aggregatedCommand);
			// select the created ConnectorShape
			CurrentDisplay.SelectShape(newShape, false);
			ResetToolState();
			OnToolExecuted(ExecutedEventArgs);
		}


		private void ResetToolState() {
			shapeAtCursor = null;
			pointAtCursor = ControlPointId.None;
			lastInsertedPointId = ControlPointId.None;
			action = ToolAction.None;
			ClearPreviews();
			// invalidate former highlighted ConnectionPoints
			highlightConnectionTargets = false;
			InvalidatePreview();
			if (CurrentDisplay != null) CurrentDisplay.Capture = false;
		}
		
		
		/// <summary>
		/// Set the cursor for the current action
		/// </summary>
		private void SetCursor(Shape shape, int pointId) {
			switch (action) {
				case ToolAction.None:
				case ToolAction.MovePoint:
					if (CurrentDisplay.Project.Security.IsGranted(Permission.Layout)) {
						if (shape != null && shape is ILinearShape && pointId > 0)
							currentCursor = movePointCursor;
						else if (pointId > 0) {
							if (!shape.HasControlPointCapability(pointId, ControlPointCapability.Glue) &&
								shape.HasControlPointCapability(pointId, ControlPointCapability.Connect)) {
								currentCursor = connectCursor;
							}
							else currentCursor = penCursor;
						}
						else if (shape != null) {
							if (shape.HasControlPointCapability(ControlPointId.Reference, ControlPointCapability.Connect))
								currentCursor = Cursors.Hand;							
							else currentCursor = penCursor;
						}
						else currentCursor = penCursor;
					}
					else currentCursor = notAllowedCursor;
					break;
				case ToolAction.DrawLine:
				case ToolAction.AddPoint:
					if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
						if (shape != null && pointId > 0 &&
							!shape.HasControlPointCapability(pointId, ControlPointCapability.Glue) &&
							shape.HasControlPointCapability(pointId, ControlPointCapability.Connect)) {
							currentCursor = connectCursor;
						}
						else if (shape != null && shape.HasControlPointCapability(ControlPointId.Reference, ControlPointCapability.Connect)) 
							currentCursor = connectCursor;
						else currentCursor = penCursor;
					}
					else currentCursor = notAllowedCursor;
					break;

				default:
					throw new Exception(string.Format("Unexpected ToolAction '{0}'", action));
			}
			CurrentDisplay.Cursor = currentCursor;
		}


		private Shape PreviewShape {
			get {
				Shape result = null;
				foreach (Shape shape in Previews.Values) {
					result = shape;
					break;
				}
				return result;
			}
		}


		private ILinearShape PreviewLinearShape {
			get {
				ILinearShape result = null;
				foreach (Shape shape in Previews.Values) {
					if (shape is ILinearShape) result = (ILinearShape)shape;
					break;
				}
				return result;
			}
		}
		

		#region Fields

		private const int StartPointId = 1;
		private const int EndPointId = 2;
		
		// stores the last inserted Point (and its coordinates), which will become the EndPoint when the CurrentTool is cancelled
		private int lastInsertedPointId;

		// stores the shape and the ControlPoint near the mouse cursor
		Shape shapeAtCursor = null;
		int pointAtCursor = ControlPointId.None;
		
		// the different Cursors of this tool
		private Cursor defaultCursor;
		private Cursor notAllowedCursor;
		private Cursor movePointCursor;
		private Cursor penCursor;
		private Cursor connectCursor;
		private Cursor disconnectCursor;
		private Cursor currentCursor;		// the current cursor, depending on the current action and the contents under the mouse cursor
		// 
		private bool highlightConnectionTargets = false;
		private ToolAction action;
		private Point startMousePos;		// transformed coordinates of the first mouse click

		#endregion
	}
	
	#endregion


	#region PlanarShapeCreationTool

	public class PlanarShapeCreationTool : TemplateTool {

		public PlanarShapeCreationTool(Template template, IDisplayService displayService)
			: base(template, displayService) {
			BaseConstruct(template);
		}


		public PlanarShapeCreationTool(Template template, IDisplayService displayService, string category)
			: base(template, displayService, category) {
			BaseConstruct(template);
		}


		public override void Dispose() {
			base.Dispose();
		}


		public override void Refresh() {
			Template.Shape.DrawThumbnail(base.LargeIcon, margin, transparentColor);
			base.LargeIcon.MakeTransparent(transparentColor);
			Template.Shape.DrawThumbnail(base.SmallIcon, margin, transparentColor);
			base.SmallIcon.MakeTransparent(transparentColor);
			ClearPreviews();
		}


		public override bool ExecuteMouseAction(IDisplay display, DiagrammingMouseEventArgs e) {
			bool result = false;
			if (base.ExecuteMouseAction(display, e)) {
				if (CurrentDisplay.Cursor != this.Cursor)
					CurrentDisplay.Cursor = this.Cursor;

				if (Previews.Count == 0)
					AddPreview(Template.Shape, Template.Shape.ShapeType.CreatePreviewInstance(Template.Shape));

				int mouseX, mouseY;
				CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out mouseX, out mouseY);

				switch (e.EventType) {

					#region handle MouseMove event
					case MouseEventType.MouseMove:
						if (CurrentMousePos != LastMousePos) {
							if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
								drawPreview = true;

								// move preview to mouse cursor
								InvalidatePreview();
								Preview.MoveControlPointTo(ControlPointId.Reference, mouseX, mouseY, Modifiers.None);
								// snap to grid
								if (CurrentDisplay.SnapToGrid) {
									int snapDeltaX, snapDeltaY;
									int snapPtId = GetDistanceToNearestSnapPoint(Preview, 0, 0, out snapDeltaX, out snapDeltaY, ControlPointCapability.All);
									mouseX += snapDeltaX;
									mouseY += snapDeltaY;
									Preview.MoveControlPointTo(ControlPointId.Reference, mouseX, mouseY, Modifiers.None);
								}
								InvalidatePreview();
							}
						}
						break;
					#endregion

					#region handle MouseUp event
					case MouseEventType.MouseUp:
						if (e.Buttons == DiagrammingMouseButtons.Left) {
							if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
								if (Preview != null && CurrentDisplay != null && DisplayContainsMousePos(CurrentMousePos)) {
									// calculate position of the new shape
									int x, y;
									CurrentDisplay.ControlToDiagram(CurrentMousePos.X, CurrentMousePos.Y, out x, out y);
									Preview.MoveControlPointTo(ControlPointId.Reference, x, y, Modifiers.None);
									// snap to grid
									if (CurrentDisplay.SnapToGrid) {
										int snapDeltaX, snapDeltaY;
										int snapPtId = GetDistanceToNearestSnapPoint(Preview, 0, 0, out snapDeltaX, out snapDeltaY, ControlPointCapability.All);
										x += snapDeltaX;
										y += snapDeltaY;
										Preview.MoveControlPointTo(ControlPointId.Reference, x, y, Modifiers.None);
									}

									ICommand cmd;
									Shape newShape = Template.CreateShape();
									if (Template.Shape.ModelObject != null)
										cmd = new InsertShapeAndModelCommand(CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, newShape, x, y, false);
									else
										cmd = new InsertShapeCommand(CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, newShape, x, y, false);
									CurrentDisplay.Project.ExecuteCommand(cmd);

									drawPreview = false;
									CurrentDisplay.SelectShape(newShape, false);
									OnToolExecuted(ExecutedEventArgs);
								}
							}
						}
						break;
					#endregion

					#region handle MouseEnter and MouseLeave event
					case MouseEventType.MouseEnter:
						if (Preview != null) {
							Preview.DisplayService = CurrentDisplay;
							Preview.MoveTo(mouseX, mouseY);
						}
						drawPreview = true;
						break;
					
					case MouseEventType.MouseLeave:
						Preview.Invalidate();
						Preview.DisplayService = null;
						drawPreview = false;
						break;
					#endregion

					#region unused MouseEvents
					case MouseEventType.MouseClick:
					case MouseEventType.MouseDoubleClick:
					case MouseEventType.MouseDown:
						// nothing to to
						break;
					#endregion

					default: throw new DiagrammingUnexpectedValueException(e.EventType);
				}
				result = true;
			}
			return result;
		}


		public override void Cancel() {
			// clear and invalidate Previews
			drawPreview = false;
			ClearPreviews();
			base.Cancel();
		}


		public override void DrawPreview() {
			if (drawPreview) {
				if (CurrentDisplay.DisplayAreaBounds.Contains(CurrentMousePos)) {
					Preview.Draw(CurrentDisplay.Graphics);
					if (CurrentDisplay.SnapToGrid)
						DrawSnapIndicators(Preview);
				}
			}
		}


		public override void InvalidatePreview() {
			if (CurrentDisplay.SnapToGrid)
				InvalidateSnapIndicators(Preview);
		}


		public new Cursor Cursor {
			get {
				// [pepo] An dieser Stelle die Rechte abfragen ist nicht korrekt. Das muss der Aufrufer wissen,
				// der die ganz Aktion veranlasst. Zum Beispiel wird dies hier auch beim Ändern eines Templates
				// aufgerufen. Warum?
				//if (CurrentDisplay != null && CurrentDisplay.project.Security.IsGranted(Permission.Insert))
				//   return Cursors.Cross;
				//else
				//   return Cursors.No;
				if (CurrentDisplay != null && drawPreview)
					return Cursors.Cross;
				else
					return Cursors.Default;
			}
		}


		private void BaseConstruct(Template template) {
			if (!(template.Shape is IPlanarShape))
				throw new DiagrammingException("The template's shape does not implement {0}.", typeof(IPlanarShape).Name);

			drawPreview = false;
			if (string.IsNullOrEmpty(template.Name))
				Title = template.Shape.ShapeType.Name;
			else Title = template.Name;
			ToolTipText = string.Format("Insert {0}.", Title);
		}


		private bool DisplayContainsMousePos(Point mousePos) {
			return (mousePos.X >= 0 && mousePos.X <= CurrentDisplay.DisplayAreaBounds.Width && mousePos.X <= CurrentDisplay.Diagram.Width &&
					mousePos.Y >= 0 && mousePos.Y <= CurrentDisplay.DisplayAreaBounds.Height && mousePos.Y <= CurrentDisplay.Diagram.Height);
		}


		private Shape Preview {
			get {
				Shape result = null;
				foreach (Shape shape in Previews.Values) {
					result = shape;
					break;
				}
				return result;
			}
		}


		#region Fields
		private bool drawPreview;
		#endregion
	}
	#endregion


	#region FreehandTool
	public class FreeHandTool : Tool {
		public FreeHandTool(Project project)
			: base("Standard") {
			BaseConstruct(project);
		}


		public FreeHandTool(Project project, string category)
			: base(category) {
			BaseConstruct(project);
		}


		public new void Dispose() {
			base.Dispose();

			timer.Stop();
			timer.Tick -= timer_Tick;
			timer.Dispose();
		}


		public override void Refresh() {
			// nothing to do
		}


		public override bool ExecuteMouseAction(IDisplay display, DiagrammingMouseEventArgs e) {
			bool result = false;
			if (base.ExecuteMouseAction(display, e)) {
				switch (e.EventType) {

					#region handle MouseDown event
					case MouseEventType.MouseDown:
						timer.Stop();
						if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
							leftMouseDown = true;
						}
						break;
					#endregion

					#region handle MouseMove event
					case MouseEventType.MouseMove:
						if (LastMousePos != CurrentMousePos) {
							if (leftMouseDown && CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
								CurrentDisplay.ControlToDiagram(e.Position, out p);
								currentStroke.Add(p.X, p.Y);
							}

							if (CurrentDisplay.Cursor != this.Cursor)
								CurrentDisplay.Cursor = this.Cursor;
						}
						InvalidatePreview();
						break;
					#endregion

					#region handle MouseUp event
					case MouseEventType.MouseUp:
						leftMouseDown = false;
						if (CurrentDisplay.Project.Security.IsGranted(Permission.Insert)) {
							timer.Start();

							strokeSet.Add(currentStroke);
							currentStroke = new Stroke();
							timer.Start();

							OnToolExecuted(CancelledEventArgs);
						}
						break;
					#endregion

					#region unused MouseEvents
					case MouseEventType.MouseClick:
					case MouseEventType.MouseDoubleClick:
					case MouseEventType.MouseEnter:
					case MouseEventType.MouseLeave:
						// nothing to do
						break;
					#endregion

					default: throw new DiagrammingUnexpectedValueException(e.EventType);
				}
				result = true;
			}
			return result;
		}


		public override void Cancel() {
			leftMouseDown = false;
			//InvalidatePreview(currentDisplay);
			currentStroke.Clear();
			strokeSet.Clear();
			base.Cancel();
		}


		public override void DrawPreview() {
			// draw stroke(s)
			foreach (Stroke stroke in strokeSet) {
				for (int i = 0; i < stroke.Count - 1; ++i) {
					CurrentDisplay.Graphics.DrawLine(Pens.DarkBlue, stroke[i], stroke[i + 1]);
				}
			}

			// draw stroke(s)
			for (int i = 0; i < currentStroke.Count - 1; ++i) {
				CurrentDisplay.Graphics.DrawLine(Pens.DarkBlue, currentStroke[i], currentStroke[i + 1]);
			}
		}


		public override void InvalidatePreview() {
			int x = int.MaxValue;
			int y = int.MaxValue;
			int width = int.MinValue;
			int height = int.MinValue;
			if (strokeSet.Count > 0)
				GetStrokeSetBounds(out x, out y, out width, out height);

			// invalidate Stroke(s)
			foreach (Point p in currentStroke) {
				if (p.X < x) x = p.X;
				if (p.Y < y) y = p.Y;
				if (p.X > x + width) width = p.X - x;
				if (p.Y > y + height) height = p.Y - y;
			}
			if (CurrentDisplay != null) {
				CurrentDisplay.ControlToDiagram(rect, out rect);
				if (strokeSet.Count > 0 || currentStroke.Count > 0)
					CurrentDisplay.InvalidateDiagram(x, y, width, height);
			}
		}


		public override Cursor Cursor {
			get { return penCursor; }
		}


		private void BaseConstruct(Project project) {
			Title = "Freehand Pen";
			ToolTipText = "Draw the symbol of the object which should be created.";

			SmallIcon = global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.FreehandIconSmall;
			SmallIcon.MakeTransparent(Color.Fuchsia);
			LargeIcon = global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.FreehandIconLarge;
			LargeIcon.MakeTransparent(Color.Fuchsia);

			penCursor = LoadCursorFromResource(global::Dataweb.Diagramming.WinFormsUI.Properties.Resources.PenCursor);

			polygone = new PathFigureShape();
			strokeSet = new StrokeSequence();
			currentStroke = new Stroke();
			shaper = new Shaper();

			timer = new Timer();
			timer.Enabled = false;
			timer.Interval = timeOut;
			timer.Tick += timer_Tick;

			this.project = project;
			project.LibraryLoaded += project_LibraryLoaded;
			RegisterFigures();
		}


		private void project_LibraryLoaded(LibraryLoadedEventArgs e) {
			RegisterFigures();
		}


		private void RegisterFigures() {
			foreach (ShapeType shapeType in project.ShapeTypes)
				if (!shaper.IsRegistered(shapeType.FullName))
					shaper.RegisterFigure(shapeType.FullName, shapeType.GetFreehandReferenceImage());
		}


		private void timer_Tick(object sender, EventArgs e) {
			timer.Stop();
			IdentifyFigure();
		}


		private void IdentifyFigure() {
			// Das ShapeSet berechnen
			Figure shapeSet = shaper.IdentifyShapes(strokeSet);

			// FeedBack
			foreach (FigureShape s in shapeSet.Shapes) {
				if (s == null)
					Console.WriteLine("NotSupported");
				else
					Console.WriteLine(s.Description);
			}
			Console.Write("=> ");

			Figure figure = shaper.FindFigure(shapeSet);
			List<string> figureNames = new List<string>();
			if (figure != null) {
				figureNames.AddRange(shaper.GetFigureNames(figure));
				Console.WriteLine(figureNames.ToString());
			}
			else
				Console.Write("No idea\n\r");

			if (CurrentDisplay != null && figureNames.Count > 0) {
				if (CurrentDisplay.Project.Repository == null)
					throw new NullReferenceException("Unable to access repository of current ownerDisplay.");

				matchingTemplates.Clear();
				foreach (Template t in CurrentDisplay.Project.Repository.GetTemplates()) {
					foreach (string figName in figureNames) {
						if (t.Shape.ShapeType.FullName == figName) {
							matchingTemplates.Add(t);
						}
					}
				}

				if (matchingTemplates.Count == 1) {
					CreateShape(matchingTemplates[0]);
				}
				else if (matchingTemplates.Count > 1) {
					// show context menu with matching templates
					contextMenu.Items.Clear();
					foreach (Template t in matchingTemplates) {
						ToolStripMenuItem item = new ToolStripMenuItem(t.Name, t.CreateThumbnail(16, 2), ContextMenuItem_Click);
						item.Tag = t;
						contextMenu.Items.Add(item);
					}
					
					int x, y, width, height;
					this.GetStrokeSetBounds(out x, out y, out width, out height);
					contextMenu.Show(x, y);
				}
			}

			InvalidatePreview();

			strokeSet.Clear();
			currentStroke.Clear();

			OnToolExecuted(ExecutedEventArgs);
		}


		private void ContextMenuItem_Click(object sender, EventArgs e) {
			if (sender is ToolStripMenuItem) {
				Template t = (Template) ((ToolStripMenuItem)sender).Tag;
				CreateShape(t);
			}
		}
		
		
		private void CreateShape(Template template) {
			// create shape
			Shape shape = (Shape)template.Shape.Clone();
			if (template.Shape.ModelObject != null)
				shape.ModelObject = (IModelObject)template.Shape.ModelObject.Clone();

			int x, y, width, height;
			GetStrokeSetBounds(out x, out y, out width, out height);
			shape.Fit(x, y, width, height);
			
			ICommand cmd;
			if (shape.ModelObject != null)
				cmd = new InsertShapeAndModelCommand(CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, shape, false);
			else
				cmd = new InsertShapeCommand(shape, CurrentDisplay.Diagram, CurrentDisplay.ActiveLayers, false, CurrentDisplay.Project.Repository);
			project.ExecuteCommand(cmd);
		}


		private void GetStrokeSetBounds(out int x, out int y, out int width, out int height) {
			x = y = 0;
			width = height = 1;
			if (strokeSet.Count > 0) {
				rect.X = int.MaxValue;
				rect.Y = int.MaxValue;
				rect.Width = int.MinValue;
				rect.Height = int.MinValue;
				foreach (Stroke stroke in strokeSet) {
					foreach (Point p in stroke) {
						if (p.X < rect.Left) rect.X = p.X;
						if (p.Y < rect.Top) rect.Y = p.Y;
						if (p.X > rect.Right) rect.Width = rect.Width + (p.X - rect.Right);
						if (p.Y > rect.Bottom) rect.Height = rect.Height + (p.Y - rect.Bottom);
					}
				}
				x = rect.X;
				y = rect.Y;
				width = rect.Width;
				height = rect.Height;
			}
		}


		#region Fields
		private Project project;
		private Cursor penCursor;

		private readonly Brush[] brushes = new Brush[] { Brushes.Blue, Brushes.Red, Brushes.Green, Brushes.Pink, Brushes.Plum };
		private Shaper shaper;
		private PathFigureShape polygone;
		private StrokeSequence strokeSet;
		private Stroke currentStroke;

		private Timer timer;
		private const int timeOut = 1250;

		private Point p;						// buffer for coordinate conversions
		private bool leftMouseDown;
		private List<Template> matchingTemplates = new List<Template>();
		private System.Drawing.Rectangle rect;

		private ContextMenuStrip contextMenu = new ContextMenuStrip();
		#endregion
	}
	#endregion
}