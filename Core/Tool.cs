using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Timers;

using Dataweb.Diagramming.Controllers;


namespace Dataweb.Diagramming.Advanced {

	public class CursorProvider {

		/// <summary>
		/// Registers a custom cursor that can be used with SetCursor.
		/// </summary>
		/// <param name="fileName">The file name of the cursor resource.</param>
		/// <returns>Id of the cursor.</returns>
		public static int RegisterCursor(string fileName) {
			if (fileName == null) throw new ArgumentNullException("fileName");
			byte[] resource = null;
			FileStream stream = new FileStream(fileName, FileMode.Open);
			try {
				resource = new byte[stream.Length];
				stream.Read(resource, 0, resource.Length);
			} finally {
				stream.Close();
				stream.Dispose();
			}
			return RegisterCursorResource(resource);
		}


		/// <summary>
		/// Registers a custom cursor that can be used with SetCursor.
		/// </summary>
		/// <param name="resourceAssembly">Assembly containing the cursor resource.</param>
		/// <param name="resourceName">The name of the cursor resource.</param>
		/// <returns>Id of the cursor.</returns>
		public static int RegisterCursor(Assembly resourceAssembly, string resourceName) {
			if (resourceAssembly == null) throw new ArgumentNullException("resourceAssembly");
			if (resourceName == null) throw new ArgumentNullException("resourceName");
			byte[] resource = null;
			Stream stream = resourceAssembly.GetManifestResourceStream(resourceName);
			try {
				resource = new byte[stream.Length];
				stream.Read(resource, 0, resource.Length);
			} finally {
				stream.Close();
				stream.Dispose();
			}
			return RegisterCursorResource(resource);
		}


		/// <summary>
		/// Registers a custom cursor that can be used with SetCursor.
		/// </summary>
		/// <param name="fileName">The cursor resource.</param>
		/// <returns>Id of the cursor.</returns>
		public static int RegisterCursor(byte[] resource) {
			if (resource == null) throw new ArgumentNullException("resource");
			return RegisterCursorResource(resource);
		}


		/// <summary>
		/// Returns all registered cursors.
		/// CursorId 0 means the system's default cursor which is not stored as resource.
		/// </summary>
		public static IEnumerable<int> CursorIDs { 
			get{ return registeredCursors.Keys;}
		}
		
		
		/// <summary>
		/// Returns the resource associated with the given cursorID. 
		/// CursorId 0 means the system's default cursor which is not stored as resource.
		/// </summary>
		/// <param name="cursorID">ID of the cursor returned by the RegisterCursor method.</param>
		/// <returns></returns>
		public static byte[] GetResource(int cursorID){
			if (cursorID == DefaultCursorID) return null;
			return registeredCursors[cursorID];
		}


		public const int DefaultCursorID = 0;


		private static int RegisterCursorResource(byte[] resource) {
			// Check if the resource was registered
			foreach (KeyValuePair<int, byte[]> item in registeredCursors) {
				if (item.Value.Length == resource.Length) {
					bool equal = true;
					for (int i = item.Value.Length - 1; i >= 0; --i) {
						if (item.Value[i] != resource[i]) {
							equal = false;
							break;
						}
					}
					if (equal) return item.Key;
				}
			}
			// Register resource
			int cursorId = registeredCursors.Count + 1;
			registeredCursors.Add(cursorId, resource);
			return cursorId;
		}


		private static Dictionary<int, byte[]> registeredCursors = new Dictionary<int, byte[]>();
	}
	
	
	/// <summary>
	/// Specifies the outcome of a tool execution.
	/// </summary>
	/// <status>reviewed</status>
	public enum ToolResult { 
		/// <summary>Tool was successfully executed</summary>
		Executed, 
		/// <summary>Tool was canceled</summary>
		Canceled 
	}


	/// <summary>
	/// Describes how a tool was executed.
	/// </summary>
	/// <status>reviewed</status>
	public class ToolExecutedEventArgs : EventArgs {

		public ToolExecutedEventArgs(Tool tool, ToolResult eventType)
			: base() {
			if (tool == null) throw new ArgumentNullException("tool");
			this.tool = tool;
			this.eventType = eventType;
		}


		public Tool Tool {
			get { return tool; }
		}


		public ToolResult EventType {
			get { return eventType; }
		}


		private Tool tool;
		private ToolResult eventType;

	}


	/// <summary>
	/// Controls a user operation on a diagram.
	/// </summary>
	/// <status>reviewed</status>
	public abstract class Tool: IDisposable {

		#region IDisposable Members

		public virtual void Dispose() {
			if (smallIcon != null)
				smallIcon.Dispose();
			smallIcon = null;

			if (largeIcon != null)
				largeIcon.Dispose();
			largeIcon = null;
		}

		#endregion


		public string Name { 
			get { return name; } 
		}


		public string Title {
			get { return title; }
			set { title = value; }
		}


		public virtual string Description {
			// TODO 2: Remove this implementation, when all derived classes have a better one.
			get { return description; }
			set { description = value; }
		}


		public string Category {
			get { return category; }
			set { category = value; }
		}


		public string ToolTipText {
			get { return Description; }
			set { Description = value; }
		}


		public Bitmap SmallIcon {
			get { return smallIcon; }
			set { smallIcon = value; }
		}


		public Bitmap LargeIcon {
			get { return largeIcon; }
			set { largeIcon = value; }
		}


		public virtual bool WantsAutoScroll {
			get { return wantsAutoScroll; }
			protected set { wantsAutoScroll = value; }
		}


		public abstract void EnterDisplay(IDiagramPresenter diagramPresenter);


		public abstract void LeaveDisplay(IDiagramPresenter diagramPresenter);


		/// <summary>
		/// Processes a display mouse event.
		/// The base Method has to be called at the end when overriding this implementation.
		/// </summary>
		/// <param name="display">Display where the event occurred.</param>
		/// <param name="e">Description of the mouse event.</param>
		/// <returns>True if the event was handled, false if the event was not handled.</returns>
		public virtual bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, DiagrammingMouseEventArgs e) {
			if (diagramPresenter == null) throw new ArgumentNullException("display");
			currentMouseState.Buttons = e.Buttons;
			currentMouseState.Modifiers = e.Modifiers;
			diagramPresenter.ControlToDiagram(e.Position, out currentMouseState.Position);
			return false;
		}


		/// <summary>
		/// Processes a keyboard event.
		/// </summary>
		/// <param name="e">Description of the keyboard event.</param>
		/// <returns>True if the event was handled, false if the event was not handled.</returns>
		public virtual bool ProcessKeyEvent(DiagrammingKeyEventArgs e) {
			bool result = false;
			switch (e.EventType) {
				case KeyEventType.KeyDown:
					// Cancel tool
					if (e.KeyCode == (int)DiagrammingKeys.Escape) {
						Cancel();
						result = true;
					}
					break;

				case KeyEventType.KeyPress:
				case KeyEventType.PreviewKeyDown:
				case KeyEventType.KeyUp:
					// do nothing
					break;
				default: throw new DiagrammingUnsupportedValueException(e.EventType);
			}
			return result;
		}


		/// <summary>
		/// Sets protected readonly-properties to invalid values and raises the ToolExecuted event.
		/// </summary>
		public void Cancel() {
			// End the tool's action
			EndToolAction();
			
			// Reset the tool's state
			CancelCore();

			currentMouseState = MouseState.Empty;
			
			OnToolExecuted(CancelledEventArgs);
		}


		public abstract IEnumerable<DiagrammingAction> GetActions(IDiagramPresenter diagramPresenter);


		public abstract void Invalidate(IDiagramPresenter diagramPresenter);


		public abstract void Draw(IDiagramPresenter diagramPresenter);


		public abstract void RefreshIcons();


		/// <summary>
		/// Occurs when the tool was executed or canceled.
		/// </summary>
		public event EventHandler<ToolExecutedEventArgs> ToolExecuted;


		protected Tool() {
			Construct();
		}


		protected Tool(string category) {
			this.category = category;
			Construct();
		}


		~Tool() {
			Dispose();
		}


		protected abstract void CancelCore();
		
		
		protected virtual void OnToolExecuted(ToolExecutedEventArgs eventArgs) {
			if (ToolExecuted != null) ToolExecuted(this, eventArgs);
		}
		
		
		/// <summary>
		/// Finds the nearest snap point for a point.
		/// </summary>
		/// <param name="ptX">X coordinate</param>
		/// <param name="ptY">Y coordinate</param>
		/// <param name="snapDeltaX">Horizontal distance between x and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between y and the nearest snap point.</param>
		/// <returns>Distance to nearest snap point.</returns>
		/// <remarks>If snapping is disabled for the current ownerDisplay, this function does virtually nothing.</remarks>
		protected float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, int x, int y, out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");

			float distance = float.MaxValue;
			snapDeltaX = snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				// calculate position of surrounding grid lines
				int gridSize = diagramPresenter.GridSize;
				int left = x - (x % gridSize);
				int above = y - (y % gridSize);
				int right = x - (x % gridSize) + gridSize;
				int below = y - (y % gridSize) + gridSize;
				float currDistance = 0;
				int snapDistance = diagramPresenter.SnapDistance;

				// calculate distance from the given point to the surrounding grid lines
				currDistance = y - above;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaY = above - y;
				}
				currDistance = right - x;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - x;
				}
				currDistance = below - y;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaY = below - y;
				}
				currDistance = x - left;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - x;
				}

				// calculate approximate distance from the given point to the surrounding grid points
				currDistance = Geometry.DistancePointPoint(x, y, left, above);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - x;
					snapDeltaY = above - y;
				}
				currDistance = Geometry.DistancePointPoint(x, y, right, above);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - x;
					snapDeltaY = above - y;
				}
				currDistance = Geometry.DistancePointPoint(x, y, left, below);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - x;
					snapDeltaY = below - y;
				}
				currDistance = Geometry.DistancePointPoint(x, y, right, below);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - x;
					snapDeltaY = below - y;
				}
			}
			return distance;
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape's control point.
		/// </summary>
		/// <param name="shape">The shape for which the nearest snap point is searched.</param>
		/// <param name="connectionPointId">The control point of the shape.</param>
		/// <param name="moveByX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="moveByY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <returns>Distance to nearest snap point.</returns>
		protected float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId, 
			int pointOffsetX, int pointOffsetY, out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			snapDeltaX = snapDeltaY = 0;
			Point p = shape.GetControlPointPosition(controlPointId);
			return FindNearestSnapPoint(diagramPresenter, p.X + pointOffsetX, p.Y + pointOffsetY, out snapDeltaX, out snapDeltaY);
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape.
		/// </summary>
		/// <param name="shape">The shape for which the nearest snap point is searched.</param>
		/// <param name="shapeOffsetX">Declares the distance, the shape is moved on X axis 
		/// before finding snap point.</param>
		/// <param name="shapeOffsetY">Declares the distance, the shape is moved on X axis 
		/// before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <returns>Distance to the calculated snap point.</returns>
		protected float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Shape shape, int shapeOffsetX, int shapeOffsetY,
			out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			snapDeltaX = snapDeltaY = 0;
			int snapDistance = diagramPresenter.SnapDistance;
			float lowestDistance = float.MaxValue;

			Rectangle shapeBounds = shape.GetBoundingRectangle(true);
			shapeBounds.Offset(shapeOffsetX, shapeOffsetY);
			int boundsCenterX = (int)Math.Round(shapeBounds.X + shapeBounds.Width /2f);
			int boundsCenterY = (int)Math.Round(shapeBounds.Y + shapeBounds.Width / 2f);
			
			int dx, dy;
			float currDistance;
			// Calculate snap distance of center point
			currDistance = FindNearestSnapPoint(diagramPresenter, boundsCenterX, boundsCenterY, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}

			// Calculate snap distance of bounding rectangle
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Left, shapeBounds.Top, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Right, shapeBounds.Top, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Left, shapeBounds.Bottom, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Right, shapeBounds.Bottom, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			return lowestDistance;
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape.
		/// </summary>
		/// <param name="shape">The shape for which the nearest snap point is searched.</param>
		/// <param name="moveByX">Declares the distance, the shape is moved on X axis 
		/// before finding snap point.</param>
		/// <param name="moveByY">Declares the distance, the shape is moved on X axis 
		/// before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param name="controlPointCapabilities">Filter for control points taken into 
		/// account while calculating the snap distance.</param>
		/// <returns>Control point of the shape, the calculated distance refers to.</returns>
		protected ControlPointId FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Shape shape, int pointOffsetX, int pointOffsetY, 
			out int snapDeltaX, out int snapDeltaY, ControlPointCapabilities controlPointCapability) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			snapDeltaX = snapDeltaY = 0;
			ControlPointId result = ControlPointId.None;
			int snapDistance = diagramPresenter.SnapDistance;
			float lowestDistance = float.MaxValue;
			foreach (ControlPointId ptId in shape.GetControlPointIds(controlPointCapability)) {
				int dx, dy;
				float currDistance = FindNearestSnapPoint(diagramPresenter, shape, ptId, pointOffsetX, pointOffsetY, out dx, out dy);
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
		/// Finds the nearest ControlPoint in range of the given shape's ControlPoint. 
		/// If there is no ControlPoint in range, the snap distance to the nearest grid 
		/// line will be calculated.
		/// </summary>
		/// <param name="shape">The given shape.</param>
		/// <param name="connectionPointId">the given shape's ControlPoint</param>
		/// <param name="moveByX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="moveByY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param name="ownPointId">The Id of the returned shape's nearest ControlPoint.</param>
		/// <returns>The shape owning the nearest ControlPoint</returns>
		protected Shape FindNearestControlPoint(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId, 
			ControlPointCapabilities targetPointCapabilities, int pointOffsetX, int pointOffsetY, 
			out int snapDeltaX, out int snapDeltaY, out ControlPointId targetPointId) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			Shape result = null;
			snapDeltaX = snapDeltaY = 0;
			targetPointId = ControlPointId.None;

			if (diagramPresenter.Diagram != null) {
				// calculate new position of the ControlPoint
				Point ctrlPtPos = shape.GetControlPointPosition(controlPointId);
				ctrlPtPos.Offset(pointOffsetX, pointOffsetY);

				int snapDistance = diagramPresenter.SnapDistance;
				foreach (Shape targetShape in diagramPresenter.Diagram.Shapes.FindShapes(ctrlPtPos.X, ctrlPtPos.Y, ControlPointCapabilities.Connect, snapDistance)) {
					if (targetShape == shape) continue;
					float distance, lowestDistance = float.MaxValue;
					ControlPointId ptId = targetShape.FindNearestControlPoint(ctrlPtPos.X, ctrlPtPos.Y, snapDistance, targetPointCapabilities);
					if (ptId != ControlPointId.None) {
						if (ptId == ControlPointId.Reference) {
							// If the shape itself is hit, do not calculate the snap distance because snapping 
							// to "real" control point has a higher priority.
							// Set TargetPointId and result shape in order to skip snapping to gridlines
							targetPointId = ptId;
							result = targetShape;
						} else {
							Point targetPtPos = targetShape.GetControlPointPosition(ptId);
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
				}
				// calcualte distance to nearest grid point if there is no suitable control point in range
				if (targetPointId == ControlPointId.None)
					FindNearestSnapPoint(diagramPresenter, ctrlPtPos.X, ctrlPtPos.Y, out snapDeltaX, out snapDeltaY);
			}
			return result;
		}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) 
		/// in range of the given point.
		/// </summary>
		protected ShapeAtCursorInfo FindConnectionTarget(IDiagramPresenter diagramPresenter, int x, int y, bool onlyUnselected) {
			// Find non-selected shape its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
			ControlPointId pointId;
			int zOrder = int.MinValue;
			foreach (Shape shape in diagramPresenter.Diagram.Shapes.FindShapes(x, y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize)) {
				// Skip selected shapes (if not wanted)
				if (onlyUnselected && diagramPresenter.SelectedShapes.Contains(shape)) continue;
				// Skip shapes below the last matching shape
				if (shape.ZOrder < zOrder) continue;
				// Perform a HitTest on the shape
				pointId = shape.HitTest(x, y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize);
				if (pointId != ControlPointId.None) {
					if (shape.HasControlPointCapability(pointId, ControlPointCapabilities.Glue)) { continue; }
					result.Shape = shape;
					result.ControlPointId = pointId;
					zOrder = shape.ZOrder;
				}
			}
			return result;
		}


		/// <summary>
		/// Find the topmost shape that is at the given point or has a control point with the given
		/// capabilities in range of the given point. If parameter onlyUnselected is true, only 
		/// shapes that are not selected will be returned.
		/// </summary>
		protected ShapeAtCursorInfo FindShapeAtCursor(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities capabilities, int range, bool onlyUnselected) {
			// Find non-selected shape its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
			int zOrder = int.MinValue;
			foreach (Shape shape in diagramPresenter.Diagram.Shapes.FindShapes(x, y, capabilities, range)) {
				// Skip selected shapes (if not wanted)
				if (onlyUnselected && diagramPresenter.SelectedShapes.Contains(shape)) continue;

				// No need to handle Parent shapes here as Children of CompositeShapes cannot be 
				// selected and grouped shapes keep their ZOrder

				// Skip shapes below the last matching shape
				if (shape.ZOrder < zOrder) continue;
				zOrder = shape.ZOrder;
				result.Shape = shape;
				result.ControlPointId = shape.HitTest(x, y, capabilities, range);
				if (result.Shape is ICaptionedShape)
					result.CaptionIndex = ((ICaptionedShape)shape).FindCaptionFromPoint(x, y);
			}
			return result;
		}


		protected void InvalidateConnectionTargets(IDiagramPresenter diagramPresenter, int currentPosX, int currentPosY) {
			// invalidate selectedShapes in last range
			diagramPresenter.InvalidateGrips(shapesInRange, ControlPointCapabilities.Connect);

			ShapeAtCursorInfo shapeAtCursor = FindConnectionTarget(diagramPresenter, currentPosX, currentPosY, false);
			if (!shapeAtCursor.IsEmpty) shapeAtCursor.Shape.Invalidate();

			// invalidate selectedShapes in current range
			shapesInRange.Clear();
			shapesInRange.AddRange(diagramPresenter.Diagram.Shapes.FindShapes(currentPosX, currentPosY, ControlPointCapabilities.Connect, pointHighlightRange));
			if (shapesInRange.Count > 0)
				diagramPresenter.InvalidateGrips(shapesInRange, ControlPointCapabilities.Connect);
			else {
			}
		}


		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, int x, int y) {
			DrawConnectionTargets(diagramPresenter, x, y, EmptyEnumerator<Shape>.Empty);
		}


		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, int x, int y, IEnumerable<Shape> excludedShapes) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Connect)) {
				// Find connection target shape at the given position
				ShapeAtCursorInfo shapeAtCursor = FindConnectionTarget(diagramPresenter, x, y, false);
				
				// Add shapes in range to the shapebuffer and then remove all excluded shapes
				shapeBuffer.Clear();
				shapeBuffer.AddRange(shapesInRange);
				foreach (Shape excludedShape in excludedShapes) {
					shapeBuffer.Remove(excludedShape);
					if (excludedShape == shapeAtCursor.Shape)
						shapeAtCursor.Clear();
				}
				
				// If there is no ControlPoint under the Cursor and the cursor is over a shape, draw the shape's outline
				if (!shapeAtCursor.IsEmpty && shapeAtCursor.ControlPointId == ControlPointId.Reference
					&& shapeAtCursor.Shape.ContainsPoint(x, y)) {
					diagramPresenter.DrawShapeOutline(DiagrammingDrawMode.Highlighted, shapeAtCursor.Shape);
				}
				
				// Draw all connectionPoints of all shapes in range (except the excluded ones, see above)
				diagramPresenter.ResetTransformation();
				try {
					for (int i = shapeBuffer.Count-1; i >= 0; --i) {
						foreach (int ptId in shapeBuffer[i].GetControlPointIds(ControlPointCapabilities.Connect)) {
							DiagrammingDrawMode drawMode = DiagrammingDrawMode.Normal;
							if (shapeBuffer[i] == shapeAtCursor.Shape && ptId == shapeAtCursor.ControlPointId)
								drawMode = DiagrammingDrawMode.Highlighted;
							diagramPresenter.DrawConnectionPoint(drawMode, shapeBuffer[i], ptId);
						}
					}
				} finally { diagramPresenter.RestoreTransformation(); }
			}
		}


		/// <summary>
		/// Sets the start coordinates for an action as well as the display to use for the action.
		/// </summary>
		/// <param name="display">The display to use for the action.</param>
		/// <param name="startPos">The start position of the action.</param>
		public virtual void StartToolAction(IDiagramPresenter diagramPresenter, Point startPos, bool wantAutoScroll) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			actionDiagramPresenter = diagramPresenter;
			actionStartPos = startPos;
			wantsAutoScroll = wantAutoScroll;
		}


		/// <summary>
		/// Ends a tool's action. Crears the start position for the action and the display used for the action.
		/// </summary>
		public virtual void EndToolAction() {
			if (actionDiagramPresenter != null) {
				Invalidate(actionDiagramPresenter);
				actionDiagramPresenter.Capture = false;
				actionDiagramPresenter.SetCursor(CursorProvider.DefaultCursorID);
				actionDiagramPresenter = null;
			}
			wantsAutoScroll = false;
			actionStartPos = Geometry.InvalidPoint;
		}


		protected bool IsGripHit(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId, int x, int y) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(controlPointId);
			return IsGripHit(diagramPresenter, p.X, p.Y, x, y);
		}


		protected bool IsGripHit(IDiagramPresenter diagramPresenter, int controlPointX, int controlPointY, int x, int y) {
			if (diagramPresenter == null) throw new ArgumentNullException("display");
			return Geometry.DistancePointPoint(controlPointX, controlPointY, x, y) <= diagramPresenter.ZoomedGripSize;
		}


		/// <summary>
		/// Current state of the mouse (state after the last ProcessMouseEvent call).
		/// Position is in Diagram coordinates.
		/// </summary>
		protected MouseState CurrentMouseState {
			get { return currentMouseState; }
		}
		
		
		/// <summary>
		/// The display used by the current (pending) action.
		/// </summary>
		protected IDiagramPresenter ActionDiagramPresenter { 
			get {
				if (actionDiagramPresenter == null) throw new DiagrammingException("The action's current display was not set yet. Call StartToolAction method to set the action's current display.");
				return actionDiagramPresenter;
			}
		}


		/// <summary>
		/// Transformed start coordinates of the current (pending) action (diagram coordinates).
		/// Use SetActionStartPosition method to set this value and ClearActionStartPosition to clear it.
		/// </summary>
		protected Point ActionStartPos {
			get {
				if (actionStartPos == Geometry.InvalidPoint)
					throw new DiagrammingInternalException("The action's start position was not set yet. Call SetActionStartPosition method to set the start position.");
				return actionStartPos;
			}
		}


		protected ToolExecutedEventArgs ExecutedEventArgs;


		protected ToolExecutedEventArgs CancelledEventArgs;


		private void Construct() {
			smallIcon = new Bitmap(16, 16);
			largeIcon = new Bitmap(32, 32);
			name = "Tool " + this.GetHashCode().ToString();
			ExecutedEventArgs = new ToolExecutedEventArgs(this, ToolResult.Executed);
			CancelledEventArgs = new ToolExecutedEventArgs(this, ToolResult.Canceled);
		}


		internal void Assert(bool condition) {
			Assert(condition, null);
		}


		internal void Assert(bool condition, string message) {
			if (condition == false) {
				if (string.IsNullOrEmpty(message)) throw new DiagrammingInternalException("Assertion Failure.");
				else throw new DiagrammingInternalException(string.Format("Assertion Failure: {0}", message));
			}
		}


		#region Types
		
		protected struct MouseState {

			public static bool operator ==(MouseState a, MouseState b){
				return (a.Position == b.Position 
					&& a.Modifiers == b.Modifiers
					&& a.Buttons == b.Buttons);
			}
			
			public static bool operator !=(MouseState a, MouseState b) {
				return !(a == b);
			}

			public static MouseState Empty;

			public override int GetHashCode() {
				return Position.GetHashCode() ^ Buttons.GetHashCode() ^ Modifiers.GetHashCode();
			}			

			public override bool Equals(object obj) {
 				return (obj is MouseState && object.ReferenceEquals(this, obj));
			}
			
			public Point Position;

			public DiagrammingKeys Modifiers;

			public DiagrammingMouseButtons Buttons;

			public bool IsButtonDown(DiagrammingMouseButtons button) {
				return (Buttons & button) != 0;
			}

			public bool IsKeyPressed(DiagrammingKeys modifier) {
				return (Modifiers & modifier) != 0;
			}

			public bool IsEmpty {
				get { return this == Empty; }
			}

			static MouseState() {
				Empty.Position = Geometry.InvalidPoint;
				Empty.Modifiers = DiagrammingKeys.None;
				Empty.Buttons = 0;
			}
		}


		protected struct ShapeAtCursorInfo {
			
			public static bool operator ==(ShapeAtCursorInfo a, ShapeAtCursorInfo b){
				return (a.Shape == b.Shape
					&& a.ControlPointId == b.ControlPointId
					&& a.CaptionIndex == b.CaptionIndex);
			}
			
			public static bool operator !=(ShapeAtCursorInfo a, ShapeAtCursorInfo b) {
				return !(a == b);
			}

			public static ShapeAtCursorInfo Empty;

			public override int GetHashCode() {
				return Shape.GetHashCode() ^ ControlPointId.GetHashCode() ^ CaptionIndex.GetHashCode();
			}			

			public override bool Equals(object obj) {
 				return (obj is ShapeAtCursorInfo && object.ReferenceEquals(this, obj));
			}

			public void Clear() {
				this = Empty;
			}
			
			public Shape Shape;

			public ControlPointId ControlPointId;

			public int CaptionIndex;

			public bool IsCursorAtGrip {
				get {
					return (Shape != null
					&& ControlPointId != ControlPointId.None
					&& ControlPointId != ControlPointId.Reference);
				}
			}

			public bool IsCursorAtGluePoint {
				get { 
					return (Shape != null 
						&& Shape.HasControlPointCapability(ControlPointId, ControlPointCapabilities.Glue));
				}
			}

			public bool IsCursorAtCaption {
				get { return (Shape is ICaptionedShape && CaptionIndex >= 0 && !IsCursorAtGrip); }
			}

			public bool IsEmpty {
				get { return Shape == null; }
			}

			static ShapeAtCursorInfo() {
				Empty.Shape = null;
				Empty.ControlPointId = ControlPointId.None;
				Empty.CaptionIndex = -1;
			}
		}

		#endregion


		#region Fields

		// --- Description of the tool ---
		// Unique name of the tool.
		private string name;
		// Title that will be displayed in the tool box
		private string title;
		// Category title of the tool, used for grouping tools in the tool box
		private string category;
		// Hint that will be displayed when the mouse is hovering the tool
		private string description;
		// small icon of the tool
		private Bitmap smallIcon;
		// the large icon of the tool
		private Bitmap largeIcon;
		//
		private bool wantsAutoScroll = false;
		// margin and background colors of the toolbox icons "LargeIcon" and "SmallIcon"
		protected int margin = 1;
		protected Color transparentColor = Color.LightGray;
		// highlighting connection targets in range
		private int pointHighlightRange = 50;
		//
		// --- Mouse state after last mouse event ---
		// State of the mouse after the last ProcessMouseEvents call
		private MouseState currentMouseState = MouseState.Empty;
		// Shapes whose connection points will be highlighted in the next drawing
		private List<Shape> shapesInRange = new List<Shape>();

		
		// --- Definition of current action ---
		// the display that is edited with this tool
		private IDiagramPresenter actionDiagramPresenter;
		// Transformed coordinates of the mouse position when an action has started (diagram coordinates)
		private Point actionStartPos;
		// 
		// Work buffer for shapes
		private List<Shape> shapeBuffer = new List<Shape>();

		#endregion
	}
	

	/// <summary>
	/// Lets the user size, move, rotate and select shapes.
	/// </summary>
	public class PointerTool : Tool {

		public PointerTool()
			: base("Standard") {
			Construct();
		}


		public PointerTool(string category)
			: base(category) {
			Construct();
		}


		#region [Public] Tool Members

		/// <override></override>
		public override void RefreshIcons() {
			// nothing to do...
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, DiagrammingMouseEventArgs e) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			bool result = false;
			// get new mouse state
			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;
			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
				// Only process mouse action if the position of the mouse or a mouse button state changed
				if (e.EventType != MouseEventType.MouseMove || newMouseState.Position != CurrentMouseState.Position) {
					// Process the mouse event
					switch (e.EventType) {
						case MouseEventType.MouseDown:
							// Start drag action such as drawing a SelectionFrame or moving selectedShapes/shape handles
							result = ProcessMouseDown(diagramPresenter, newMouseState);
							break;

						case MouseEventType.MouseMove:
							// Set cursors depending on HotSpots or draw moving/resizing preview
							result = ProcessMouseMove(diagramPresenter, newMouseState);
							break;

						case MouseEventType.MouseUp:
							// perform selection/moving/resizing
							result = ProcessMouseUp(diagramPresenter, newMouseState);
							if (!result && e.Clicks > 1)
								// perform QuickRotate (90°) if the feature is enabled
								result = ProcessDoubleClick(diagramPresenter, newMouseState, e.Clicks);
							break;

						default: throw new DiagrammingUnsupportedValueException(e.EventType);
					}
				}
			} finally { diagramPresenter.ResumeUpdate(); }
			base.ProcessMouseEvent(diagramPresenter, e);
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(DiagrammingKeyEventArgs e) {
			bool result = base.ProcessKeyEvent(e);
			// if the keyPress was not handled by the base class, try to handle it here
			if (!result) {
				switch (e.EventType) {
					case KeyEventType.PreviewKeyDown:
					case KeyEventType.KeyDown:
						// do nothing
						break;
					case KeyEventType.KeyUp:
						// handle Function keys
						switch (e.KeyCode) {
							case (int)DiagrammingKeys.F1: break;
							case (int)DiagrammingKeys.F2:
								//ToDo: Start caption editing here?
								break;
							case (int)DiagrammingKeys.F3: break;
							case (int)DiagrammingKeys.F4: break;
							case (int)DiagrammingKeys.F5: break;
							case (int)DiagrammingKeys.F6: break;
							case (int)DiagrammingKeys.F7: break;
							case (int)DiagrammingKeys.F8: break;
							case (int)DiagrammingKeys.F9: break;
							case (int)DiagrammingKeys.F10: break;
							case (int)DiagrammingKeys.F11: break;
							case (int)DiagrammingKeys.F12: break;
							case (int)DiagrammingKeys.F13: break;
							case (int)DiagrammingKeys.F14: break;
							case (int)DiagrammingKeys.F15: break;
							case (int)DiagrammingKeys.F16: break;
							case (int)DiagrammingKeys.F17: break;
							case (int)DiagrammingKeys.F18: break;
							case (int)DiagrammingKeys.F19: break;
							case (int)DiagrammingKeys.F20: break;
							case (int)DiagrammingKeys.F21: break;
							case (int)DiagrammingKeys.F22: break;
							case (int)DiagrammingKeys.F23: break;
							case (int)DiagrammingKeys.F24: break;
						}
						// ToDo: handle ShortCuts (Ctrl + ?)
						//result = true;
						break;

					case KeyEventType.KeyPress:
						//if (!char.IsControl(e.KeyChar))
						//   ShowTextEditor(e.KeyChar.ToString());
						//result = true;
						break;
					default:
						throw new DiagrammingException(string.Format("Unexpected {0} value '{1}'.", e.EventType.GetType(), e.EventType));
				}
			}
			return result;
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			// nothing to do
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
 			// nothing to do
		}


		public override IEnumerable<DiagrammingAction> GetActions(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			int mouseX = CurrentMouseState.Position.X;
			int mouseY = CurrentMouseState.Position.Y;

			bool separatorRequired = false;
			if (!SelectedShapeAtCursor.IsEmpty) {
				// Deliver Template's actions
				if (SelectedShapeAtCursor.Shape.Template != null) {
					foreach (DiagrammingAction action in SelectedShapeAtCursor.Shape.Template.GetActions()) {
						if (!separatorRequired) separatorRequired = true;
						yield return action;
					}
				}
				foreach (DiagrammingAction action in SelectedShapeAtCursor.Shape.GetActions(mouseX, mouseY, diagramPresenter.ZoomedGripSize)) {
					if (separatorRequired) yield return new SeparatorAction();
					yield return action;
				}
				if (SelectedShapeAtCursor.Shape.ModelObject != null) {
					if (separatorRequired) yield return new SeparatorAction();
					foreach (DiagrammingAction action in SelectedShapeAtCursor.Shape.ModelObject.GetActions())
						yield return action;
				}
			} else {
				// ToDo: Find shape under the cursor and return its actions?
				// ToDo: Collect all actions provided by the diagram if no shape was right-clicked
			}
			// ToDo: Add tool-specific actions?
		}

	
		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			switch (CurrentAction) {
				case ToolAction.Select:
					// nothing to do
					break;
				
				case ToolAction.None:
				case ToolAction.EditCaption:
					// MouseOver-Highlighting of the caption under the cursor 
					// At the moment, the ownerDisplay draws the caption bounds along with the selection highlighting
					if (SelectedShapeAtCursor.IsCursorAtCaption) {
						diagramPresenter.ResetTransformation();
						try {
							diagramPresenter.DrawCaptionBounds(DiagrammingDrawMode.Highlighted, (ICaptionedShape)SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.CaptionIndex);
						} finally { diagramPresenter.RestoreTransformation(); }
					}
					break;
				
				case ToolAction.SelectWithFrame:
					diagramPresenter.ResetTransformation();
					try {
						diagramPresenter.DrawSelectionFrame(frameRect);
					} finally { diagramPresenter.RestoreTransformation(); }
					break;
				
				case ToolAction.MoveShape:
				case ToolAction.MoveHandle:
					// Draw shape previews first
					diagramPresenter.DrawShapes(Previews.Values);

					// Then draw snap-lines and -points
					if (SelectedShapeAtCursor != null && (snapPtId > 0 || snapDeltaX != 0 || snapDeltaY != 0)) {
						Shape previewAtCursor = FindPreviewOfShape(SelectedShapeAtCursor.Shape);
						diagramPresenter.DrawSnapIndicators(previewAtCursor);
					}
					// Finally, draw highlighten ConnectionPoints and/or highlighted shape outlines
					if (Previews.Count == 1 && SelectedShapeAtCursor.ControlPointId != ControlPointId.None) {
						Shape preview = null;
						foreach (KeyValuePair<Shape, Shape> item in Previews) {
							preview = item.Value;
							break;
						}
						if (preview.HasControlPointCapability(SelectedShapeAtCursor.ControlPointId, ControlPointCapabilities.Glue)) {
							// find and highlight connectionPoints in Range
							Point gluePointPos = preview.GetControlPointPosition(SelectedShapeAtCursor.ControlPointId);
							DrawConnectionTargets(ActionDiagramPresenter, gluePointPos.X, gluePointPos.Y, ActionDiagramPresenter.SelectedShapes);
						}
					}
					break;
				
				case ToolAction.Rotate:
					diagramPresenter.DrawShapes(Previews.Values);
					diagramPresenter.ResetTransformation();
					try {
						int currAngle = CalcAngle(ActionStartPos, CurrentMouseState);
						// ToDo: Determine standard cursor size
						diagramPresenter.DrawAnglePreview(rectBuffer.Location, rectBuffer.Width, CurrentMouseState.Position, cursors[ToolCursor.Rotate], 0, currAngle);
					} finally { diagramPresenter.RestoreTransformation(); }
					break;
				
				default: throw new DiagrammingUnsupportedValueException(CurrentAction);
			}
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			switch (CurrentAction) {
				case ToolAction.None:
				case ToolAction.Select:
				case ToolAction.EditCaption:
					if (!SelectedShapeAtCursor.IsEmpty) {
						SelectedShapeAtCursor.Shape.Invalidate();
						diagramPresenter.InvalidateGrips(SelectedShapeAtCursor.Shape, ControlPointCapabilities.All);
					}
					break;

				case ToolAction.SelectWithFrame:
					diagramPresenter.DisplayService.Invalidate(frameRect);
					break;

				case ToolAction.MoveHandle:
				case ToolAction.MoveShape:
					Assert(!SelectedShapeAtCursor.IsEmpty);
					if (Previews.Count > 0) {
						InvalidateShapes(diagramPresenter, Previews.Values);
						if (diagramPresenter.SnapToGrid) {
							Shape previewAtCursor = FindPreviewOfShape(SelectedShapeAtCursor.Shape);
							diagramPresenter.InvalidateSnapIndicators(previewAtCursor);
						}
						if (CurrentAction == ToolAction.MoveHandle && SelectedShapeAtCursor.IsCursorAtGluePoint)
							InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.Position.X, CurrentMouseState.Position.Y);
					}
					break;

				case ToolAction.Rotate:
					if (Previews.Count > 0) InvalidateShapes(diagramPresenter, Previews.Values);
					InvalidateAnglePreview(diagramPresenter);
					break;

				default: throw new DiagrammingUnsupportedValueException(typeof(DiagrammingAction), CurrentAction);
			}
		}


		/// <override></override>
		public override void StartToolAction(IDiagramPresenter diagramPresenter, Point startPos, bool wantAutoScroll) {
			base.StartToolAction(diagramPresenter, startPos, wantAutoScroll);
			// Empty selection frame
			frameRect.Location = startPos;
			frameRect.Size = Size.Empty;
		}
		
		
		/// <override></override>
		public override void EndToolAction() {
			base.EndToolAction();
			currentToolAction = ToolAction.None;
			ClearPreviews();
		}


		/// <override></override>
		protected override void CancelCore() {
			frameRect = Rectangle.Empty;
			rectBuffer = Rectangle.Empty;

			currentToolAction = ToolAction.None;
			SelectedShapeAtCursor.Clear();
		}


		#endregion


		#region [Private] Properties

		private ToolAction CurrentAction {
			get { return currentToolAction; }
		}


		private ShapeAtCursorInfo SelectedShapeAtCursor {
			get { return selShapeAtCursor; }
		}

		#endregion


		#region [Private] MouseEvent processing implementation

		private bool ProcessMouseDown(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;

			if (!SelectedShapeAtCursor.IsEmpty &&
				!diagramPresenter.SelectedShapes.Contains(SelectedShapeAtCursor.Shape))
				SelectedShapeAtCursor.Clear();

			// If no action is pending, try to start a new one...
			if (CurrentAction == ToolAction.None) {
				// Get suitable action (depending on the currently selected shape under the mouse cursor)
				ToolAction newAction = DetermineMouseDownAction(diagramPresenter, mouseState);
				if (newAction != ToolAction.None) {
					currentToolAction = newAction;
					bool wantAutoScroll;
					switch (newAction) {
						case ToolAction.Select:
						case ToolAction.SelectWithFrame:
						case ToolAction.MoveHandle:
						case ToolAction.MoveShape:
							wantAutoScroll = true; break;
						default: wantAutoScroll = false; break;
					}
					StartToolAction(diagramPresenter, mouseState.Position, wantAutoScroll);

					// If the action requires preview shapes, create them now...
					switch (CurrentAction) {
						case ToolAction.None:
						case ToolAction.Select:
						case ToolAction.SelectWithFrame:
						case ToolAction.EditCaption:
							break;
						case ToolAction.MoveHandle:
						case ToolAction.MoveShape:
						case ToolAction.Rotate:
							CreatePreviewShapes(diagramPresenter);
							break;
						default: throw new DiagrammingUnsupportedValueException(CurrentAction);
					}

					Invalidate(ActionDiagramPresenter);
					result = true;
				}
			} else {
				// ... otherwise cancel the action (if right mouse button was pressed)
				ToolAction newAction = DetermineMouseDownAction(diagramPresenter, mouseState);
				if (newAction == ToolAction.None) {
					Cancel();
					result = true;
				}
			}
			return result;
		}


		private bool ProcessMouseMove(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;

			if (!SelectedShapeAtCursor.IsEmpty && 
				!diagramPresenter.SelectedShapes.Contains(SelectedShapeAtCursor.Shape))
				SelectedShapeAtCursor.Clear();

			switch (CurrentAction) {
				case ToolAction.None:
					SetSelectedShapeAtCursor(diagramPresenter, mouseState.Position.X, mouseState.Position.Y, diagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
					Invalidate(diagramPresenter);
					break;

				case ToolAction.EditCaption:
					Invalidate(ActionDiagramPresenter);
					break;

				case ToolAction.Select:
					// Find unselected shape under the mouse cursor
					ShapeAtCursorInfo shapeAtCursor = ShapeAtCursorInfo.Empty;
					if (mouseState.IsButtonDown(DiagrammingMouseButtons.Left))
						shapeAtCursor = FindShapeAtCursor(ActionDiagramPresenter, ActionStartPos.X, ActionStartPos.Y, ControlPointCapabilities.None, 0, true);

					ToolAction newAction = DetermineMouseMoveAction(ActionDiagramPresenter, mouseState, shapeAtCursor);
					if (newAction != CurrentAction) {
						currentToolAction = newAction;
						if (currentToolAction == ToolAction.SelectWithFrame) {
							result = PrepareSelectionFrame(ActionDiagramPresenter, mouseState);
						} else if (currentToolAction == ToolAction.MoveShape) {
							if (SelectedShapeAtCursor.IsEmpty) {
								// Select shape at cursor before start dragging it
								PerformSelection(ActionDiagramPresenter, mouseState, shapeAtCursor);
								SetSelectedShapeAtCursor(diagramPresenter, ActionStartPos.X, ActionStartPos.Y, 0, ControlPointCapabilities.None);
								Assert(!SelectedShapeAtCursor.IsEmpty);
							}
							Assert(!SelectedShapeAtCursor.IsEmpty);
							CreatePreviewShapes(ActionDiagramPresenter);
							result = PrepareMoveShapePreview(ActionDiagramPresenter, mouseState);
						} else throw new DiagrammingInternalException("Unhandled change of CurrentAction.");
					} 
					Invalidate(ActionDiagramPresenter);
					break;

				case ToolAction.SelectWithFrame:
					Invalidate(ActionDiagramPresenter);
					result = PrepareSelectionFrame(ActionDiagramPresenter, mouseState);
					Invalidate(ActionDiagramPresenter);
					break;

				case ToolAction.MoveHandle:
					Assert(IsMoveHandleFeasible(ActionDiagramPresenter, SelectedShapeAtCursor));
					Invalidate(ActionDiagramPresenter);
					result = PrepareMoveHandlePreview(ActionDiagramPresenter, mouseState);
					Invalidate(ActionDiagramPresenter);
					break;

				case ToolAction.MoveShape:
					Assert(IsMoveShapeFeasible(ActionDiagramPresenter, SelectedShapeAtCursor));
					Invalidate(ActionDiagramPresenter);
					result = PrepareMoveShapePreview(diagramPresenter, mouseState);
					Invalidate(ActionDiagramPresenter);
					break;

				case ToolAction.Rotate:
					Assert(IsRotatatingFeasible(ActionDiagramPresenter, SelectedShapeAtCursor));
					Invalidate(ActionDiagramPresenter);
					PrepareRotatePreview(ActionDiagramPresenter, mouseState);
					Invalidate(ActionDiagramPresenter);
					break;

				default: throw new DiagrammingUnsupportedValueException(typeof(ToolAction), CurrentAction);
			}
			
			int cursorId = DetermineCursor(diagramPresenter, mouseState);
			if (CurrentAction == ToolAction.None) diagramPresenter.SetCursor(cursorId);
			else ActionDiagramPresenter.SetCursor(cursorId);

			return result;
		}


		private bool ProcessMouseUp(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;

			if (!SelectedShapeAtCursor.IsEmpty &&
				!diagramPresenter.SelectedShapes.Contains(SelectedShapeAtCursor.Shape))
				SelectedShapeAtCursor.Clear();

			switch (CurrentAction) {
				case ToolAction.None:
					// do nothing
					break;

				case ToolAction.Select:
					// Perform selection
					ShapeAtCursorInfo shapeAtCursor = ShapeAtCursorInfo.Empty;
					if (!SelectedShapeAtCursor.IsEmpty) {
						if (SelectedShapeAtCursor.Shape.ContainsPoint(mouseState.Position.X, mouseState.Position.Y)) {
							Shape shape = ActionDiagramPresenter.Diagram.Shapes.FindShape(mouseState.Position.X, mouseState.Position.Y, ControlPointCapabilities.None, 0, SelectedShapeAtCursor.Shape);
							if (shape != null) {
								shapeAtCursor.Shape = shape;
								shapeAtCursor.ControlPointId = shape.HitTest(mouseState.Position.X, mouseState.Position.Y, ControlPointCapabilities.None, 0);
								shapeAtCursor.CaptionIndex = -1;
							}
						}
					}
					else shapeAtCursor = FindShapeAtCursor(diagramPresenter, mouseState.Position.X, mouseState.Position.Y, ControlPointCapabilities.None, 0, false);
					result = PerformSelection(ActionDiagramPresenter, mouseState, shapeAtCursor);

					SetSelectedShapeAtCursor(ActionDiagramPresenter, mouseState.Position.X, mouseState.Position.Y, ActionDiagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
					EndToolAction();
					break;

				case ToolAction.SelectWithFrame:
					// select all selectedShapes within the frame
					result = PerformFrameSelection(ActionDiagramPresenter, mouseState);
					EndToolAction();
					break;

				case ToolAction.EditCaption:
					// if the user clicked a caption, display the caption editor
					Assert(SelectedShapeAtCursor.IsCursorAtCaption);
					ActionDiagramPresenter.OpenCaptionEditor((ICaptionedShape)SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.CaptionIndex);
					EndToolAction();
					result = true;
					break;

				case ToolAction.MoveHandle:
					Assert(!SelectedShapeAtCursor.IsEmpty);
					result = PerformMoveHandle(ActionDiagramPresenter, mouseState);
					EndToolAction();
					break;

				case ToolAction.MoveShape:
					Assert(!SelectedShapeAtCursor.IsEmpty);
					result = PerformMoveShape(ActionDiagramPresenter, mouseState);
					EndToolAction();
					break;

				case ToolAction.Rotate:
					Assert(!SelectedShapeAtCursor.IsEmpty);
					result = PerformRotate(ActionDiagramPresenter, mouseState);
					EndToolAction();
					break;

				default: throw new DiagrammingUnsupportedValueException(CurrentAction);
			}
			
			diagramPresenter.SetCursor(DetermineCursor(diagramPresenter, mouseState));
			OnToolExecuted(ExecutedEventArgs);
			return result;
		}


		private bool ProcessDoubleClick(IDiagramPresenter diagramPresenter, MouseState mouseState, int clickCount) {
			bool result = false;
			if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes) && enableQuickRotate) {
				if (!SelectedShapeAtCursor.IsEmpty && SelectedShapeAtCursor.IsCursorAtGrip
					&& SelectedShapeAtCursor.Shape.HasControlPointCapability(SelectedShapeAtCursor.ControlPointId, ControlPointCapabilities.Rotate)) {
					int angle = 900 * (clickCount - 1);
					if (angle % 3600 != 0) {
						PerformQuickRotate(diagramPresenter, angle);
						result = true;
						OnToolExecuted(ExecutedEventArgs);
					}
				}
			}
			return result;
		}

		#endregion


		#region [Private] Determine action depending on mouse state and event type

		/// <summary>
		/// Decide which tool action is suitable for the current mouse state.
		/// </summary>
		private ToolAction DetermineMouseDownAction(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (mouseState.IsButtonDown(DiagrammingMouseButtons.Left)) {
				if (!SelectedShapeAtCursor.IsEmpty) {
					// Check if cursor is over a control point and moving grips or rotating is feasible
					if (SelectedShapeAtCursor.IsCursorAtGrip) {
						if (IsMoveHandleFeasible(diagramPresenter, SelectedShapeAtCursor))
							return ToolAction.MoveHandle;
						else if (IsRotatatingFeasible(diagramPresenter, SelectedShapeAtCursor))
							return ToolAction.Rotate;
					}
					// Moving shapes is initiated as soon as the user starts drag action (move mouse 
					// while mouse button us pressed) 
					// If the user does not start a drag action, this will result in (un)selecting shapes.

					//// Check if cursor is inside the shape and move shape is feasible
					//if (IsMoveShapeFeasible(display, SelectedShapeAtCursor))
					//   return ToolAction.MoveShape;
				}
				// If the cursor is not over a caption of a selected shape when clicking left mouse button, 
				// we assume the user wants to select something
				// Same thing if no other action is granted.
				if (IsEditCaptionFeasible(diagramPresenter, SelectedShapeAtCursor))
					return ToolAction.EditCaption;
				else return ToolAction.Select;
			} else if (mouseState.IsButtonDown(DiagrammingMouseButtons.Right)) {
				// Abort current action when clicking right mouse button
				return ToolAction.None;
			} else {
				// Ignore other pressed mouse buttons
				return CurrentAction;
			}
		}


		/// <summary>
		/// Decide which tool action is suitable for the current mouse state.
		/// </summary>
		private ToolAction DetermineMouseMoveAction(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursor) {
			switch (CurrentAction) {
				case ToolAction.None:
				case ToolAction.EditCaption:
				case ToolAction.MoveHandle:
				case ToolAction.MoveShape:
				case ToolAction.Rotate:
				case ToolAction.SelectWithFrame:
					// Do not change the current action
					return CurrentAction;

				case ToolAction.Select:
					if (mouseState.IsButtonDown(DiagrammingMouseButtons.Left)) {
						// If there is no shape under the cursor, start a SelectWithFrame action,
						// otherwise start a MoveShape action
						if (IsMoveShapeFeasible(diagramPresenter, SelectedShapeAtCursor)
							|| IsMoveShapeFeasible(diagramPresenter, shapeAtCursor))
							return ToolAction.MoveShape;
						else return ToolAction.SelectWithFrame;
					} else return CurrentAction;
				
				default: throw new DiagrammingUnsupportedValueException(CurrentAction);
			}
		}

		#endregion


		#region [Private] Action implementations

		#region Selecting Shapes

		// (Un)Select shape unter the mouse pointer
		private bool PerformSelection(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursor) {
			bool result = false;
			bool multiSelect = mouseState.IsKeyPressed(DiagrammingKeys.Control) || mouseState.IsKeyPressed(DiagrammingKeys.Shift);

			// When selecting shapes conteolpoints should be ignored as the user does not see them 
			// until a shape is selected
			const ControlPointCapabilities capabilities = ControlPointCapabilities.None;
			const int range = 0;

			// Determine the shape that has to be selected:
			Shape shapeToSelect = null;
			if (!SelectedShapeAtCursor.IsEmpty) {
				// When in multiSelection mode, unselect the selected shape under the cursor
				if (multiSelect) shapeToSelect = SelectedShapeAtCursor.Shape;
				else {
					// First, check if the selected shape under the cursor has children that can be selected
					shapeToSelect = SelectedShapeAtCursor.Shape.Children.FindShape(mouseState.Position.X, mouseState.Position.Y, capabilities, range, null);
					// Second, check if the selected shape under the cursor has siblings that can be selected
					if (shapeToSelect == null && SelectedShapeAtCursor.Shape.Parent != null) {
						shapeToSelect = SelectedShapeAtCursor.Shape.Parent.Children.FindShape(mouseState.Position.X, mouseState.Position.Y, capabilities, range, SelectedShapeAtCursor.Shape);
						// Discard found shape if it is the selected shape at cursor
						if (shapeToSelect == SelectedShapeAtCursor.Shape) shapeToSelect = null;
						if (shapeToSelect == null) {
							foreach (Shape shape in SelectedShapeAtCursor.Shape.Parent.Children.FindShapes(mouseState.Position.X, mouseState.Position.Y, capabilities, range)) {
								if (shape == SelectedShapeAtCursor.Shape) continue;
								shapeToSelect = shape;
								break;
							}
						}
					}
					// Third, check if there are non-selected shapes below the selected shape under the cursor
					Shape startShape = SelectedShapeAtCursor.Shape;
					while (startShape.Parent != null) startShape = startShape.Parent;
					if (shapeToSelect == null && diagramPresenter.Diagram.Shapes.Contains(startShape))
						shapeToSelect = diagramPresenter.Diagram.Shapes.FindShape(mouseState.Position.X, mouseState.Position.Y, capabilities, range, startShape);
				}
			}

			// If there was a shape to select related to the selected shape under the cursor
			// (a child or a sibling of the selected shape or a shape below it),
			// try to select the first non-selected shape under the cursor
			if (shapeToSelect == null && shapeAtCursor.Shape != null 
				&& shapeAtCursor.Shape.ContainsPoint(mouseState.Position.X, mouseState.Position.Y))
				shapeToSelect = shapeAtCursor.Shape;

			// If a new shape to select was found, perform selection
			if (shapeToSelect != null) {
				// (check if multiselection mode is enabled (Shift + Click or Ctrl + Click))
				if (multiSelect) {
					// if multiSelect is enabled, add/remove to/from selected selectedShapes...
					if (diagramPresenter.SelectedShapes.Contains(shapeToSelect)) {
						// if object is selected -> remove from selection
						diagramPresenter.UnselectShape(shapeToSelect);
						RemovePreviewOf(shapeToSelect);
						result = true;
					} else {
						// If object is not selected -> add to selection
						diagramPresenter.SelectShape(shapeToSelect, true);
						result = true;
					}
				}
				else {
					// ... otherwise deselect all selectedShapes but the clicked object
					ClearPreviews();
					// check if the clicked shape is a child of an already selected shape
					Shape childShape = null;
					if (diagramPresenter.SelectedShapes.Count == 1
						&& diagramPresenter.SelectedShapes.TopMost.Children != null
						&& diagramPresenter.SelectedShapes.TopMost.Children.Count > 0) {
						childShape = diagramPresenter.SelectedShapes.TopMost.Children.FindShape(mouseState.Position.X, mouseState.Position.Y, ControlPointCapabilities.None, 0, null);
					}
					if (childShape != null) diagramPresenter.SelectShape(childShape, false);
					else diagramPresenter.SelectShape(shapeToSelect, false);
					result = true;
				}

				// validate if the desired shape or its parent was selected
				if (shapeToSelect.Parent != null) {
					if (!diagramPresenter.SelectedShapes.Contains(shapeToSelect))
						if (diagramPresenter.SelectedShapes.Contains(shapeToSelect.Parent))
							shapeToSelect = shapeToSelect.Parent;
				}
			} else if (SelectedShapeAtCursor.IsEmpty) {
				// if there was no other shape to select and none of the selected shapes is under the cursor,
				// clear selection
				if (!multiSelect) {
					if (diagramPresenter.SelectedShapes.Count > 0) {
						diagramPresenter.UnselectAll();
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


		// Calculate new selection frame
		private bool PrepareSelectionFrame(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			frameRect.X = Math.Min(ActionStartPos.X, mouseState.Position.X);
			frameRect.Y = Math.Min(ActionStartPos.Y, mouseState.Position.Y);
			frameRect.Width = Math.Max(ActionStartPos.X, mouseState.Position.X) - frameRect.X;
			frameRect.Height = Math.Max(ActionStartPos.Y, mouseState.Position.Y) - frameRect.Y;
			return true;
		}


		// Select shapes inside the selection frame
		private bool PerformFrameSelection(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool multiSelect = mouseState.IsKeyPressed(DiagrammingKeys.Control) || mouseState.IsKeyPressed(DiagrammingKeys.Shift);
			diagramPresenter.SelectShapes(frameRect, multiSelect);
			return true;
		}

		#endregion


		#region Connecting / Disconnecting GluePoints

		private bool ShapeHasGluePoint(Shape shape) {
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Glue))
				return true;
			return false;
		}


		private void DisconnectGluePoints(IDiagramPresenter diagramPresenter) {
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				foreach (ControlPointId ptId in selectedShape.GetControlPointIds(ControlPointCapabilities.Connect | ControlPointCapabilities.Glue)) {
					// disconnect GluePoints if they are moved together with their targets
					bool skip = false;
					foreach (ShapeConnectionInfo ci in selectedShape.GetConnectionInfos(ptId, null)) {
						if (ci.OwnPointId != ptId) throw new DiagrammingInternalException("Fatal error: Unexpected ShapeConnectionInfo was returned.");
						if (diagramPresenter.SelectedShapes.Contains(ci.OtherShape)) {
							skip = false;
							break;
						}
					}
					if (skip) continue;
					
					// otherwise, compare positions of the GluePoint with it's targetPoint and disconnect if they are not equal
					if (selectedShape.HasControlPointCapability(ptId, ControlPointCapabilities.Glue)) {
						Shape previewShape = FindPreviewOfShape(selectedShape);
						if (selectedShape.GetControlPointPosition(ptId) != previewShape.GetControlPointPosition(ptId)) {
							bool isConnected = false;
							foreach (ShapeConnectionInfo sci in selectedShape.GetConnectionInfos(ptId, null)) {
								if (sci.OwnPointId == ptId) {
									isConnected = true;
									break;
								} else throw new DiagrammingInternalException("Fatal error: Unexpected ShapeConnectionInfo was returned.");
							}
							if (isConnected) {
								ICommand cmd = new DisconnectCommand(selectedShape, ptId);
								diagramPresenter.Project.ExecuteCommand(cmd);
							}
						}
					}
				}
			}
		}


		private void ConnectGluePoints(IDiagramPresenter diagramPresenter) {
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				// find selectedShapes that own GluePoints
				foreach (ControlPointId gluePointId in selectedShape.GetControlPointIds(ControlPointCapabilities.Glue)) {
					Point gluePointPos = Point.Empty;
					gluePointPos = selectedShape.GetControlPointPosition(gluePointId);

					// find selectedShapes to connect to
					foreach (Shape shape in diagramPresenter.Diagram.Shapes.FindShapes(gluePointPos.X, gluePointPos.Y, ControlPointCapabilities.Connect, diagramPresenter.GripSize)) {
						if (diagramPresenter.SelectedShapes.Contains(shape)) {
							// restore connections that were disconnected before
							int targetPointId = shape.FindNearestControlPoint(gluePointPos.X, gluePointPos.Y, 0, ControlPointCapabilities.Connect);
							if (targetPointId != ControlPointId.None)
								selectedShape.Connect(gluePointId, shape, targetPointId);
						}
						else {
							ShapeAtCursorInfo shapeInfo = FindConnectionTarget(diagramPresenter, gluePointPos.X, gluePointPos.Y, true);
							if (shapeInfo.ControlPointId != ControlPointId.None) {
								ICommand cmd = new ConnectCommand(selectedShape, gluePointId, shapeInfo.Shape, shapeInfo.ControlPointId);
								diagramPresenter.Project.ExecuteCommand(cmd);
							} 
							//else if (shape.ContainsPoint(gluePointPos.X, gluePointPos.Y)) {
							//   ICommand cmd = new ConnectCommand(selectedShape, gluePointId, shape, ControlPointId.Reference);
							//   display.Project.ExecuteCommand(cmd);
							//}
						}
					}
				}
			}
		}
		
		#endregion


		#region Moving Shapes

		// prepare drawing preview of move action
		private bool PrepareMoveShapePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			Assert(diagramPresenter.SelectedShapes.Count > 0);
			Assert(!SelectedShapeAtCursor.IsEmpty);
			// calculate the movement
			int distanceX = mouseState.Position.X - ActionStartPos.X;
			int distanceY = mouseState.Position.Y - ActionStartPos.Y;
			// calculate "Snap to Grid" offset
			snapDeltaX = snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursor.Shape, distanceX, distanceY, out snapDeltaX, out snapDeltaY);
				distanceX += snapDeltaX;
				distanceY += snapDeltaY;
			}
			// move selectedShapes
			Rectangle shapeBounds = Rectangle.Empty;
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				Shape preview = FindPreviewOfShape(selectedShape);
				preview.MoveTo(selectedShape.X + distanceX, selectedShape.Y + distanceY);
			}
			return true;
		}


		// apply the move action
		private bool PerformMoveShape(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			if (SelectedShapeAtCursor.IsEmpty) {
				// Das SOLLTE nie passieren - passiert aber leider ab und zu... :-(
				return result;
			}

			if (ActionStartPos != mouseState.Position) {
				// calculate the movement
				int distanceX = mouseState.Position.X - ActionStartPos.X;
				int distanceY = mouseState.Position.Y - ActionStartPos.Y;
				snapDeltaX = snapDeltaY = 0;
				if (diagramPresenter.SnapToGrid)
					FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursor.Shape, distanceX, distanceY, out snapDeltaX, out snapDeltaY, ControlPointCapabilities.All);

				ICommand cmd = new MoveShapeByCommand(diagramPresenter.SelectedShapes, distanceX + snapDeltaX, distanceY + snapDeltaY);
				diagramPresenter.Project.ExecuteCommand(cmd);

				snapDeltaX = snapDeltaY = 0;
				snapPtId = ControlPointId.None;
				result = true;
			}
			return result;
		}
		
		#endregion


		#region Moving ControlPoints

		// prepare drawing preview of resize action 
		private bool PrepareMoveHandlePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			int distanceX = mouseState.Position.X - ActionStartPos.X;
			int distanceY = mouseState.Position.Y - ActionStartPos.Y;

			// calculate "Snap to Grid/ControlPoint" offset
			snapDeltaX = snapDeltaY = 0;
			if (SelectedShapeAtCursor.IsCursorAtGluePoint) {
				ControlPointId targetPtId;
				Shape targetShape = FindNearestControlPoint(diagramPresenter, SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.ControlPointId, ControlPointCapabilities.Connect, distanceX, distanceY, out snapDeltaX, out snapDeltaY, out targetPtId);
			}
			else
				FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.ControlPointId, distanceX, distanceY, out snapDeltaX, out snapDeltaY);
			distanceX += snapDeltaX;
			distanceY += snapDeltaY;

			// ToDo: optimize this: fewer move operations
			// (This code does not work yet)
			//Point originalPtPos = Point.Empty;
			//for (int i = 0; i < display.SelectedShapes.Count; ++i) {
			//   // reset position
			//   originalPtPos = display.SelectedShapes[i].GetControlPointPosition(SelectedPointId);
			//   // perform new movement
			//   if (Previews[i].HasControlPointCapability(SelectedPointId, ControlPointCapabilities.Resize))
			//      Previews[i].MoveControlPointTo(SelectedPointId, originalPtPos.X + distanceX, originalPtPos.Y + distanceY, CurrentModifiers);
			//}

			// move selected shapes
			Point originalPtPos = Point.Empty;
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				Shape previewShape = FindPreviewOfShape(selectedShape);
				
				// reset position
			   originalPtPos = selectedShape.GetControlPointPosition(SelectedShapeAtCursor.ControlPointId);
				// ToDo: Restore ResizeModifiers
			   previewShape.MoveControlPointTo(SelectedShapeAtCursor.ControlPointId, originalPtPos.X, originalPtPos.Y, ResizeModifiers.None);
			   previewShape.MoveControlPointTo(ControlPointId.Reference, selectedShape.X, selectedShape.Y, ResizeModifiers.None);

			   // perform new movement
			   if (previewShape.HasControlPointCapability(SelectedShapeAtCursor.ControlPointId, ControlPointCapabilities.Resize))
					// ToDo: Restore ResizeModifiers
			      previewShape.MoveControlPointBy(SelectedShapeAtCursor.ControlPointId, distanceX, distanceY, ResizeModifiers.None);
			}
			return true;
		}


		// apply the resize action
		private bool PerformMoveHandle(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			Invalidate(diagramPresenter);
			if (ActionStartPos != mouseState.Position) {
				int distanceX = mouseState.Position.X - ActionStartPos.X;
				int distanceY = mouseState.Position.Y - ActionStartPos.Y;

				// if the moved ControlPoint is a single GluePoint, snap to ConnectionPoints
				snapDeltaX = snapDeltaY = 0;
				bool isGluePoint = false;
				if (diagramPresenter.SelectedShapes.Count == 1)
					isGluePoint = SelectedShapeAtCursor.Shape.HasControlPointCapability(SelectedShapeAtCursor.ControlPointId, ControlPointCapabilities.Glue);
				
				// Snap to Grid or ControlPoint
				Shape targetShape = null;
				ControlPointId targetPointId = ControlPointId.None;
				if (isGluePoint) {
					// Find a ControlPoint to snap/connect to
					targetShape = FindNearestControlPoint(diagramPresenter, SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.ControlPointId, ControlPointCapabilities.Connect, distanceX, distanceY, out snapDeltaX, out snapDeltaY, out targetPointId);
					// If no 'real' ControlPoint was found, search shapes for a Point-To-Shape connection
					Point p = SelectedShapeAtCursor.Shape.GetControlPointPosition(SelectedShapeAtCursor.ControlPointId);
					if (targetShape == null && targetPointId == ControlPointId.None) {
						foreach (Shape shape in diagramPresenter.Diagram.Shapes.FindShapes(p.X + distanceX, p.Y + distanceY, ControlPointCapabilities.None, 0)) {
							if (diagramPresenter.SelectedShapes.Contains(shape)) continue;
							targetShape = shape;
							break;
						}
					}
				} else
					FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.ControlPointId, distanceX, distanceY, out snapDeltaX, out snapDeltaY);
				distanceX += snapDeltaX;
				distanceY += snapDeltaY;

				if (isGluePoint){
					ICommand cmd = new MoveGluePointCommand(SelectedShapeAtCursor.Shape, SelectedShapeAtCursor.ControlPointId, targetShape, distanceX, distanceY, ResizeModifiers.None);
					diagramPresenter.Project.ExecuteCommand(cmd);
				}
				else {
					// ToDo: Re-activate ResizeModifiers
					ICommand cmd = new MoveControlPointCommand(diagramPresenter.SelectedShapes, SelectedShapeAtCursor.ControlPointId, distanceX, distanceY, ResizeModifiers.None);
					diagramPresenter.Project.ExecuteCommand(cmd);
				}
				
				snapDeltaX = snapDeltaY = 0;
				snapPtId = ControlPointId.None;
				result = true;
			}
			return result;
		}

		#endregion


		#region Rotating Shapes

		private int CalcAngle(Point prevMousePos, MouseState newMouseState) {
			int deltaX = newMouseState.Position.X - prevMousePos.X;
			int deltaY = newMouseState.Position.Y - prevMousePos.Y;
			int result = (3600 + Geometry.RadiansToTenthsOfDegree((float)Math.Atan2(deltaY, deltaX))) % 3600;
			if (newMouseState.IsKeyPressed(DiagrammingKeys.Control) && newMouseState.IsKeyPressed(DiagrammingKeys.Shift)) {
				// rotate by tenths of degrees
				// do nothing 
			} else if (newMouseState.IsKeyPressed(DiagrammingKeys.Control)) {
				// rotate by full degrees
				result -= (result % 10);
			} else if (newMouseState.IsKeyPressed(DiagrammingKeys.Shift)) {
				// rotate by 5 degrees
				result -= (result % 50);
			} else {
				// default:
				// rotate by 15 degrees
				result -= (result % 150);
			}
			return result;
		}


		// prepare drawing preview of rotate action
		private bool PrepareRotatePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			int dist = (int)Math.Round(Geometry.DistancePointPoint(mouseState.Position.X, mouseState.Position.Y, ActionStartPos.X, ActionStartPos.Y));
			diagramPresenter.DiagramToControl(dist, out dist);
			if (dist > diagramPresenter.MinRotateRange) {
				// calculate new angle
				int currAngle = CalcAngle(ActionStartPos, mouseState);

				// ToDo: Implement rotation around a common rotation center
				Point rotationCenter = Point.Empty;
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
					Shape previewShape = FindPreviewOfShape(selectedShape);
					// Get ControlPointId of the first rotate control point
					ControlPointId rotatePtId = ControlPointId.None;
					foreach (ControlPointId id in previewShape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
						rotatePtId = id;
						break;
					}
					if (rotatePtId == ControlPointId.None) throw new DiagrammingInternalException("{0} has no rotate control point.");
					rotationCenter = previewShape.GetControlPointPosition(rotatePtId);

					// restore original shapeAngle
					if (previewShape is IPlanarShape)
						((IPlanarShape)previewShape).Angle = ((IPlanarShape)selectedShape).Angle;
					else {
						int lastAngle = CalcAngle(ActionStartPos, CurrentMouseState);
						previewShape.Rotate(-lastAngle, rotationCenter.X, rotationCenter.Y);
						//previewShape.MoveTo(selectedShape.X, selectedShape.Y);
					}

					// perform rotation
					if (previewShape is IPlanarShape)
						((IPlanarShape)previewShape).Angle += currAngle;
					else previewShape.Rotate(currAngle, rotationCenter.X, rotationCenter.Y);

				}
			}
			return true;
		}


		// apply rotate action
		private bool PerformRotate(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			if (ActionStartPos != mouseState.Position) {
				int dist = (int)Math.Round(Geometry.DistancePointPoint(mouseState.Position.X, mouseState.Position.Y, ActionStartPos.X, ActionStartPos.Y));
				diagramPresenter.DiagramToControl(dist, out dist);
				if (dist > diagramPresenter.MinRotateRange) {
					int angle = CalcAngle(ActionStartPos, mouseState);
					ICommand cmd = new RotateShapesCommand(diagramPresenter.SelectedShapes, angle);
					diagramPresenter.Project.ExecuteCommand(cmd);
					result = true;
				}
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


		private bool PerformQuickRotate(IDiagramPresenter diagramPresenter, int angle) {
			bool result = false;
			if (enableQuickRotate) {
				ICommand cmd = new RotateShapesCommand(diagramPresenter.SelectedShapes, angle);
				diagramPresenter.Project.ExecuteCommand(cmd);
				InvalidateAnglePreview(diagramPresenter);
				result = true;
			}
			return result;
		}


		private void InvalidateAnglePreview(IDiagramPresenter diagramPresenter) {
			// invalidate previous shapeAngle preview
			diagramPresenter.InvalidateDiagram(rectBuffer.X - rectBuffer.Width - diagramPresenter.GripSize, rectBuffer.Y - rectBuffer.Height - diagramPresenter.GripSize, rectBuffer.Width + rectBuffer.Width + diagramPresenter.GripSize + diagramPresenter.GripSize, rectBuffer.Height + rectBuffer.Height + diagramPresenter.GripSize + diagramPresenter.GripSize);

			int requiredDistance;
			diagramPresenter.ControlToDiagram(diagramPresenter.MinRotateRange, out requiredDistance);
			int length = (int)Math.Round(Geometry.DistancePointPoint(ActionStartPos.X, ActionStartPos.Y, CurrentMouseState.Position.X, CurrentMouseState.Position.Y));

			// invalidate current angle preview / instruction preview
			rectBuffer.Location = ActionStartPos;
			if (length > requiredDistance)
				rectBuffer.Width = rectBuffer.Height = length;
			else 
				rectBuffer.Width = rectBuffer.Height = requiredDistance;
			diagramPresenter.InvalidateDiagram(rectBuffer.X - rectBuffer.Width, rectBuffer.Y - rectBuffer.Height, rectBuffer.Width + rectBuffer.Width, rectBuffer.Height + rectBuffer.Height);
		}

		#endregion


		#region Title Editor

		//private void ShowTextEditor(string pressedKey) {
		//   // show TextEditor
		//   if (CurrentDisplay.SelectedShapes.Count == 1) {
		//      if (CurrentDisplay.SelectedShapes.TopMost is ICaptionedShape) {
		//         ICaptionedShape labeledShape = (ICaptionedShape)CurrentDisplay.SelectedShapes.TopMost;
		//         if (labeledShape.CaptionCount > 0) CurrentDisplay.OpenCaptionEditor(labeledShape, 0, pressedKey);
		//      }
		//   }
		//}

		#endregion

		#endregion


		#region [Private] Preview management implementation

		/// <summary>
		/// The dictionary of preview shapes: The key is the original shape, the value is the preview shape.
		/// </summary>
		private IDictionary<Shape, Shape> Previews {
			get { return previewShapes; }
		}


		private Shape FindPreviewOfShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			Assert(previewShapes.ContainsKey(shape), string.Format("No preview found for '{0}' shape.", shape.Type.Name));
			return previewShapes[shape];
		}


		private Shape FindShapeOfPreview(Shape previewShape) {
			if (previewShape == null) throw new ArgumentNullException("previewShape");
			Assert(originalShapes.ContainsKey(previewShape), string.Format("No original shape found for '{0}' preview shape.", previewShape.Type.Name));
			return originalShapes[previewShape];
		}


		private void AddPreview(Shape shape, Shape previewShape, IDisplayService displayService) {
			if (originalShapes.ContainsKey(previewShape)) return;
			if (previewShapes.ContainsKey(shape)) return;
			// Set DisplayService for the preview shape
			if (previewShape.DisplayService != displayService)
				previewShape.DisplayService = displayService;

			// Add shape and its preview to the appropriate dictionaries
			previewShapes.Add(shape, previewShape);
			originalShapes.Add(previewShape, shape);

			// Add shape's children and their previews to the appropriate dictionaries
			if (previewShape.Children.Count > 0) {
				IEnumerator<Shape> previewChildren = previewShape.Children.TopDown.GetEnumerator();
				IEnumerator<Shape> originalChildren = shape.Children.TopDown.GetEnumerator();

				previewChildren.Reset();
				originalChildren.Reset();
				bool processNext = false;
				if (previewChildren.MoveNext() && originalChildren.MoveNext())
					processNext = true;
				while (processNext) {
					AddPreview(originalChildren.Current, previewChildren.Current, displayService);
					processNext = (previewChildren.MoveNext() && originalChildren.MoveNext());
				}
			}
		}


		private void RemovePreviewOf(Shape originalShape) {
			if (previewShapes.ContainsKey(originalShape)) {
				// Invalidate Preview Shape
				Shape previewShape = Previews[originalShape];
				previewShape.Invalidate();

				// remove previews of the shape and its children from the preview's dictionary
				previewShapes.Remove(originalShape);
				if (originalShape.Children.Count > 0) {
					foreach (Shape childShape in originalShape.Children)
						previewShapes.Remove(childShape);
				}
				// remove the shape and its children from the shape's dictionary
				originalShapes.Remove(previewShape);
				if (previewShape.Children.Count > 0) {
					foreach (Shape childShape in previewShape.Children)
						originalShapes.Remove(childShape);
				}
			}
		}


		private void RemovePreview(Shape previewShape) {
			Shape origShape = null;
			if (!originalShapes.TryGetValue(previewShape, out origShape))
				throw new DiagrammingInternalException("This preview shape has no associated original shape in this tool.");
			else {
				// Invalidate Preview Shape
				previewShape.Invalidate();
				// Remove both, original- and preview shape from the appropriate dictionaries
				previewShapes.Remove(origShape);
				originalShapes.Remove(previewShape);
			}
		}


		private void ClearPreviews() {
			foreach (KeyValuePair<Shape, Shape> item in previewShapes) {
				Shape preview = item.Value;
				foreach (ControlPointId gluePointId in preview.GetControlPointIds(ControlPointCapabilities.Glue))
					preview.Disconnect(gluePointId);
				preview.Invalidate();
				preview.DisplayService = null;
				preview.Dispose();
			}
			previewShapes.Clear();
			originalShapes.Clear();
		}


		private bool IsConnectedToNonSelectedShape(IDiagramPresenter diagramPresenter, Shape shape) {
			foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty
					&& !diagramPresenter.SelectedShapes.Contains(sci.OtherShape))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Create previews of shapes connected to the given shape (and it's children) and connect them to the
		/// shape's preview (or the preview of it's child)
		/// </summary>
		/// <param name="shape">The original shape which contains all ConnectionInfo</param>
		private void ConnectPreviewOfShape(IDiagramPresenter diagramPresenter, Shape shape) {
			// process shape's children
			if (shape.Children != null && shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					ConnectPreviewOfShape(diagramPresenter, childShape);
			}

			Shape preview = FindPreviewOfShape(shape);
			foreach (ShapeConnectionInfo connectionInfo in shape.GetConnectionInfos(ControlPointId.Any, null)) {
				if (diagramPresenter.SelectedShapes.Contains(connectionInfo.OtherShape)) {
					// Do not connect previews if BOTH of the connected shapes are part of the selection because 
					// this would restrict movement of the connector shapes and decreases performance (many 
					// unnecessary FollowConnectionPointWithGluePoint() calls)
					if (shape.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue)) {
						if (IsConnectedToNonSelectedShape(diagramPresenter, shape)) {
							Shape targetPreview = FindPreviewOfShape(connectionInfo.OtherShape);
							preview.Connect(connectionInfo.OwnPointId, targetPreview, connectionInfo.OtherPointId);
						}
					}
				} else {
					// Connect preview of shape to a non-selected shape if it is a single shape 
					// that has a glue point (e.g. a Label)
					if (preview.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue)) {
						// Only connect if the control point to be connected is not the control point to be moved
						if (shape == SelectedShapeAtCursor.Shape && connectionInfo.OwnPointId != SelectedShapeAtCursor.ControlPointId)
							preview.Connect(connectionInfo.OwnPointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
					} else
						// Create a preview of the shape that is connected to the preview (recursive call)
						CreateConnectedTargetPreviewShape(diagramPresenter, preview, connectionInfo);
				}
			}
		}


		/// <summary>
		/// Creates (or finds) a preview of the connection's PassiveShape and connects it to the current preview shape
		/// </summary>
		/// <param name="previewShape">The preview shape</param>
		/// <param name="connectionInfo">ConnectionInfo of the original (non-preview) shape</param>
		private void CreateConnectedTargetPreviewShape(IDiagramPresenter diagramPresenter, Shape previewShape, ShapeConnectionInfo connectionInfo) {
			// Check if any other selected shape is connected to the same non-selected shape
			Shape previewTargetShape;
			// If the current passiveShape is already connected to another shape of the current selection,
			// connect the current preview to the other preview's passiveShape
			if (!targetShapeBuffer.TryGetValue(connectionInfo.OtherShape, out previewTargetShape)) {
				// If the current passiveShape is not connected to any other of the selected selectedShapes,
				// create a clone of the passiveShape and connect it to the corresponding preview
				// If the preview exists, abort connecting (in this case, the shape is a preview of a child shape)
				if (previewShapes.ContainsKey(connectionInfo.OtherShape)) return;
				else {
					previewTargetShape = connectionInfo.OtherShape.Type.CreatePreviewInstance(connectionInfo.OtherShape);
					AddPreview(connectionInfo.OtherShape, previewTargetShape, diagramPresenter.DisplayService);
				}
				// add passiveShape and it's clone to the passiveShape dictionary
				targetShapeBuffer.Add(connectionInfo.OtherShape, previewTargetShape);
			}
			// Connect the (new or existing) preview shapes
			// Skip connecting if the preview is already connected.
			Assert(previewTargetShape != null, "Error while creating connected preview shapes.");
			if (previewTargetShape.IsConnected(connectionInfo.OtherPointId, null) == ControlPointId.None) {
				previewTargetShape.Connect(connectionInfo.OtherPointId, previewShape, connectionInfo.OwnPointId);
				// check, if any shapes are connected to the connector (that is connected to the selected shape)
				foreach (ShapeConnectionInfo connectorCI in connectionInfo.OtherShape.GetConnectionInfos(ControlPointId.Any, null)) {
					// skip if the connector is connected to the shape with more than one glue point
					if (connectorCI.OtherShape == FindShapeOfPreview(previewShape)) continue;
					if (connectorCI.OwnPointId != connectionInfo.OtherPointId) {
						// Check if the shape on the other end is selected.
						// If it is, connect to it's preview or skip connecting if the target preview does 
						// not exist yet (it will be connected when creating the targt's preview)
						if (diagramPresenter.SelectedShapes.Contains(connectorCI.OtherShape)) {
							if (previewShapes.ContainsKey(connectorCI.OtherShape)) {
								Shape s = FindPreviewOfShape(connectorCI.OtherShape);
								if (s.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None)
									previewTargetShape.Connect(connectorCI.OwnPointId, s, connectorCI.OtherPointId);
							} else continue;
						} else if (connectorCI.OtherShape.HasControlPointCapability(connectorCI.OtherPointId, ControlPointCapabilities.Glue))
							// Connect connectors connected to the previewTargetShape
							CreateConnectedTargetPreviewShape(diagramPresenter, previewTargetShape, connectorCI);
						else if (connectorCI.OtherPointId == ControlPointId.Reference) {
							// Connect the other end of the previewTargetShape if the connection is a Point-To-Shape connection
							Assert(connectorCI.OtherShape.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None);
							Assert(previewTargetShape.IsConnected(connectorCI.OwnPointId, null) == ControlPointId.None);
							previewTargetShape.Connect(connectorCI.OwnPointId, connectorCI.OtherShape, connectorCI.OtherPointId);
						}
					}
				}
			}
		}


		#endregion


		#region [Private] Helper Methods

		private void SetSelectedShapeAtCursor(IDiagramPresenter diagramPresenter, int mouseX, int mouseY, int handleRadius, ControlPointCapabilities handleCapabilities) {
			// Find the shape under the cursor
			selShapeAtCursor.Clear();
			selShapeAtCursor.Shape = diagramPresenter.SelectedShapes.FindShape(mouseX, mouseY, handleCapabilities, handleRadius, null);
			
			// If there is a shape under the cursor, find the nearest control point and caption
			if (!selShapeAtCursor.IsEmpty) {
				// Find control point at cursor that belongs to the selected shape at cursor
				selShapeAtCursor.ControlPointId = selShapeAtCursor.Shape.FindNearestControlPoint(mouseX, mouseY, diagramPresenter.ZoomedGripSize, gripCapabilities);
				// Find caption at cursor (if the shape is a captioned shape)
				if (selShapeAtCursor.Shape is ICaptionedShape && ((ICaptionedShape)selShapeAtCursor.Shape).CaptionCount > 0)
					selShapeAtCursor.CaptionIndex = ((ICaptionedShape)selShapeAtCursor.Shape).FindCaptionFromPoint(mouseX, mouseY);
			}
		}


		private bool ShapeOrShapeRelativesContainsPoint(Shape shape, int x, int y, ControlPointCapabilities capabilities, int range) {
			if (shape.HitTest(x, y, capabilities, range) != ControlPointId.None)
				return true;
			else if (shape.Parent != null) {
				if (ShapeOrShapeRelativesContainsPoint(shape.Parent, x, y, capabilities, range))
					return true;
			}
			return false;
		}


		private int DetermineCursor(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			switch (CurrentAction) {
				case ToolAction.None:
					// If no action is pending, the folowing cursors are possible:
					// - Default (no selected shape under cursor or action not granted)
					// - Move shape cursor
					// - Move grip cursor
					// - Rotate cursor
					// - Edit caption cursor
					if (!SelectedShapeAtCursor.IsEmpty) {
						// Check if cursor is over a caption and editing caption is feasible
						if (IsEditCaptionFeasible(diagramPresenter, SelectedShapeAtCursor))
							return cursors[ToolCursor.EditCaption];
						// Check if cursor is over a control point and moving grips or rotating is feasible
						if (SelectedShapeAtCursor.IsCursorAtGrip) {
							if (IsMoveHandleFeasible(diagramPresenter, SelectedShapeAtCursor))
								return cursors[ToolCursor.MoveHandle];
							else if (IsRotatatingFeasible(diagramPresenter, SelectedShapeAtCursor))
								return cursors[ToolCursor.Rotate];
							else return cursors[ToolCursor.Default];
						}
						// Check if cursor is inside the shape and move shape is feasible
						if (IsMoveShapeFeasible(diagramPresenter, SelectedShapeAtCursor))
							return cursors[ToolCursor.MoveShape];
					}
					return cursors[ToolCursor.Default];

				case ToolAction.Select:
				case ToolAction.SelectWithFrame:
					return cursors[ToolCursor.Default];

				case ToolAction.EditCaption:
					Assert(!SelectedShapeAtCursor.IsEmpty);
					Assert(SelectedShapeAtCursor.Shape is ICaptionedShape);
					// If the cursor is outside the caption, return default cursor
					int captionIndex = ((ICaptionedShape)SelectedShapeAtCursor.Shape).FindCaptionFromPoint(mouseState.Position.X, mouseState.Position.Y);
					if (captionIndex == SelectedShapeAtCursor.CaptionIndex)
						return cursors[ToolCursor.EditCaption];
					else return cursors[ToolCursor.Default];

				case ToolAction.MoveHandle:
					Assert(!SelectedShapeAtCursor.IsEmpty);
					Assert(SelectedShapeAtCursor.IsCursorAtGrip);
					if (SelectedShapeAtCursor.IsCursorAtGluePoint) {
						Shape previewShape = FindPreviewOfShape(SelectedShapeAtCursor.Shape);
						Point ptPos = previewShape.GetControlPointPosition(SelectedShapeAtCursor.ControlPointId);
						ShapeAtCursorInfo shapeAtCursor = FindConnectionTarget(diagramPresenter, ptPos.X, ptPos.Y, true);
						if (!shapeAtCursor.IsEmpty && shapeAtCursor.IsCursorAtGrip)
							return cursors[ToolCursor.Connect];
					}
					return cursors[ToolCursor.MoveHandle];

				case ToolAction.MoveShape:
					return cursors[ToolCursor.MoveShape];

				case ToolAction.Rotate:
					return cursors[ToolCursor.Rotate];

				default: throw new DiagrammingUnsupportedValueException(CurrentAction);
			}
		}


		/// <summary>
		/// Create Previews of all shapes selected in the CurrentDisplay.
		/// These previews are connected to all the shapes the original shapes are connected to.
		/// </summary>
		private void CreatePreviewShapes(IDiagramPresenter diagramPresenter) {
			if (Previews.Count == 0 && diagramPresenter.SelectedShapes.Count > 0) {
				// first, clone all selected shapes...
				foreach (Shape shape in diagramPresenter.SelectedShapes)
					AddPreview(shape, shape.Type.CreatePreviewInstance(shape), diagramPresenter.DisplayService);
				// ...then restore connections between previews and connections between previews and non-selected shapes
				targetShapeBuffer.Clear();
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes.BottomUp) {
					// AttachGluePointToConnectionPoint the preview shape (and all it's cildren) to all the shapes the original shape was connected to
					// Additionally, create previews for all connected shapes and connect these to the appropriate target shapes
					ConnectPreviewOfShape(diagramPresenter, selectedShape);
				}
				targetShapeBuffer.Clear();
			}
		}


		private void InvalidateShapes(IDiagramPresenter diagramPresenter, IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes)
				DoInvalidateShape(diagramPresenter, shape);
		}


		private void DoInvalidateShape(IDiagramPresenter diagramPresenter, Shape shape) {
			if (shape.Parent != null)
				DoInvalidateShape(diagramPresenter, shape.Parent);
			else {
				shape.Invalidate();
				diagramPresenter.InvalidateGrips(shape, ControlPointCapabilities.All);
			}
		}


		private bool IsMoveShapeFeasible(IDiagramPresenter diagramPresenter, ShapeAtCursorInfo shapeAtCursor) {
			if (shapeAtCursor.IsEmpty)
				return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes))
				return false;
			//if (CurrentAction == ToolAction.None && (shapeAtCursor.IsCursorAtGrip || shapeAtCursor.IsCursorAtCaption))
			//   return false;

			// ToDo: If there are *many* shapes selected (e.g. 10000), this check will be extremly slow...
			if (diagramPresenter.SelectedShapes.Count < 1000) {
				// LinearShapes that own connected gluePoints may not be moved.
				foreach (Shape shape in diagramPresenter.SelectedShapes) {
					if (shape is ILinearShape) {
						foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
							ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
							if (!sci.IsEmpty) {
								// Allow movement if the connected shapes are moved together
								if (!diagramPresenter.SelectedShapes.Contains(sci.OtherShape))
									return false;
							}
						}
					}
				}
			}
			return true;
		}


		private bool IsMoveHandleFeasible(IDiagramPresenter diagramPresenter, ShapeAtCursorInfo shapeAtCursor) {
			if (shapeAtCursor.IsEmpty)
				return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes))
				return false;
			if (!shapeAtCursor.Shape.HasControlPointCapability(shapeAtCursor.ControlPointId, ControlPointCapabilities.Resize | ControlPointCapabilities.Glue))
				return false;
			if (diagramPresenter.SelectedShapes.Count > 1) {
				// GluePoints may only be moved alone
				if (shapeAtCursor.Shape.HasControlPointCapability(shapeAtCursor.ControlPointId, ControlPointCapabilities.Glue))
					return false;
				// Check if all shapes that are going to be resizes are of the same type
				Shape lastShape = null;
				foreach (Shape shape in diagramPresenter.SelectedShapes) {
					if (lastShape != null && lastShape.Type != shape.Type) 
						return false;
					lastShape = shape;
				}
			}
			return true;
		}


		private bool IsRotatatingFeasible(IDiagramPresenter diagramPresenter, ShapeAtCursorInfo shapeAtCursor) {
			if (shapeAtCursor.IsEmpty) 
				return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes))
				return false;
			if (!shapeAtCursor.Shape.HasControlPointCapability(shapeAtCursor.ControlPointId, ControlPointCapabilities.Rotate))
				return false;
			if (diagramPresenter.SelectedShapes.Count > 1) {
				// check if all selected shapes have a rotate handle
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
					bool shapeHasRotateHandle = false;
					foreach (ControlPointId ptId in selectedShape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
						shapeHasRotateHandle = true;
						break;
					}
					if (!shapeHasRotateHandle) return false;
				}
			}
			return true;
		}


		private bool IsEditCaptionFeasible(IDiagramPresenter diagramPresenter, ShapeAtCursorInfo shapeAtCursor) {
			if (shapeAtCursor.IsEmpty) 
				return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.ModifyData, shapeAtCursor.Shape))
				return false;
			if (!shapeAtCursor.IsCursorAtCaption)
				return false;
			return true;
		}

		#endregion


		#region [Private] Construction

		static PointerTool() {
			cursors = new Dictionary<ToolCursor, int>(8);
			// Register cursors
			cursors.Add(ToolCursor.Default, CursorProvider.DefaultCursorID);
			cursors.Add(ToolCursor.ActionDenied, CursorProvider.RegisterCursor(Properties.Resources.ActionDeniedCursor));
			cursors.Add(ToolCursor.EditCaption, CursorProvider.RegisterCursor(Properties.Resources.EditTextCursor));
			cursors.Add(ToolCursor.MoveShape, CursorProvider.RegisterCursor(Properties.Resources.MoveShapeCursor));
			cursors.Add(ToolCursor.MoveHandle, CursorProvider.RegisterCursor(Properties.Resources.MovePointCursor));
			cursors.Add(ToolCursor.Rotate, CursorProvider.RegisterCursor(Properties.Resources.RotateCursor));
			// ToDo: Create better Connect/Disconnect cursors
			cursors.Add(ToolCursor.Connect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			cursors.Add(ToolCursor.Disconnect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
		}


		private void Construct() {
			Title = "Pointer";
			ToolTipText = "Select one or more objects by clicking or drawing a frame.\n\rSelected objects can be moved by dragging them to the target position or resized by dragging a control point to the target position.";

			SmallIcon = global::Dataweb.Diagramming.Properties.Resources.PointerIconSmall;
			SmallIcon.MakeTransparent(Color.Fuchsia);

			LargeIcon = global::Dataweb.Diagramming.Properties.Resources.PointerIconLarge;
			LargeIcon.MakeTransparent(Color.Fuchsia);

			frameRect = Rectangle.Empty;
		}

		#endregion


		#region [Private] Types

		private enum ToolAction { None, Select, SelectWithFrame, EditCaption, MoveShape, MoveHandle, Rotate }


		private enum ToolCursor {
			Default,
			Rotate,
			MoveHandle,
			MoveShape,
			ActionDenied,
			EditCaption,
			Connect,
			Disconnect
		}


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

		#endregion


		#region Fields

		// --- Description of the tool ---
		private static Dictionary<ToolCursor, int> cursors;
		//
		private bool enableQuickRotate = false;
		private ControlPointCapabilities gripCapabilities = ControlPointCapabilities.Resize | ControlPointCapabilities.Rotate;

		// --- State after the last ProcessMouseEvent ---
		// selected shape under the mouse cursor, being highlighted in the next drawing
		private ShapeAtCursorInfo selShapeAtCursor;
		// rectangle that represents the transformed selection area in control coordinates
		private Rectangle frameRect;
		// stores the distance the SelectedShape was moved on X-axis for snapping the nearest gridpoint
		private int snapDeltaX;
		// stores the distance the SelectedShape was moved on Y-axis for snapping the nearest gridpoint
		private int snapDeltaY;
		// index of the controlPoint that snapped to grid/point/swimline
		private int snapPtId;

		// -- Definition of current action
		// indicates the current action depending on the mouseButton State, selected selectedShapes and mouse movement
		private ToolAction currentToolAction = ToolAction.None;
		// preview shapes (Key = original shape, Value = preview shape)
		private Dictionary<Shape, Shape> previewShapes = new Dictionary<Shape, Shape>();
		// original shapes (Key = preview shape, Value = original shape)
		private Dictionary<Shape, Shape> originalShapes = new Dictionary<Shape, Shape>();

		// Buffers
		// rectangle buffer 
		private Rectangle rectBuffer;		
		// used for buffering selectedShapes connected to the preview selectedShapes: key = passiveShape, values = targetShapes's clone
		private Dictionary<Shape, Shape> targetShapeBuffer = new Dictionary<Shape, Shape>();
		// buffer used for storing connections that are temporarily disconnected for moving shapes
		private List<ConnectionInfoBuffer> connectionsBuffer = new List<ConnectionInfoBuffer>();	

		#endregion
	}
	

	/// <summary>
	/// Lets the user create a templated shape.
	/// </summary>
	public abstract class TemplateTool : Tool {

		protected TemplateTool(Template template, string category)
			: base(category) {
			Construct(template);
		}


		protected TemplateTool(Template template)
			: base() {
			Category = template.Shape.Type.DefaultCategoryTitle;
			Construct(template);
		}


		public override void Dispose() {
			// Do not dispose the Template - it has to be disposed by the cache
			base.Dispose();
		}


		public Template Template {
			get { return template; }
		}


		/// <override></override>
		public override void RefreshIcons() {
			using (Shape clone = Template.Shape.Clone()) {
				clone.DrawThumbnail(base.LargeIcon, margin, transparentColor);
				base.LargeIcon.MakeTransparent(transparentColor);
			}
			using (Shape clone = Template.Shape.Clone()) {
				clone.DrawThumbnail(base.SmallIcon, margin, transparentColor);
				base.SmallIcon.MakeTransparent(transparentColor);
			}
			ClearPreview();
			Title = string.IsNullOrEmpty(Template.Title) ? Template.Name : Template.Title;
		}


		protected Shape PreviewShape {
			get { return previewShape; }
		}


		protected void CreatePreview(IDiagramPresenter diagramPresenter) {
			previewShape = Template.Shape.Type.CreatePreviewInstance(Template.Shape);
			previewShape.DisplayService = diagramPresenter.DisplayService;
			previewShape.Invalidate();
		}


		protected void ClearPreview() {
			if (previewShape != null) {
				previewShape.Invalidate();
				previewShape.Dispose();
				previewShape = null;
			}
		}
				

		private void Construct(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			this.template = template;
			Title = string.IsNullOrEmpty(template.Title) ? template.Name : template.Title;
			ToolTipText = string.IsNullOrEmpty(template.Description) ? string.Format("Insert {0}", Title)
				: string.Format("Insert {0}: {1}", Title, template.Description);
			RefreshIcons();
		}


		#region Fields

		private Template template;
		private Shape previewShape;

		#endregion
	}


	/// <summary>
	/// Lets the user create a shape based on a point sequence.
	/// </summary>
	public class LinearShapeCreationTool : TemplateTool {

		private enum ToolAction { None, DrawLine, AddPoint, MovePoint }

		public LinearShapeCreationTool(Template template)
			: base(template) {
			Construct(template);
		}


		public LinearShapeCreationTool(Template template, string category)
			: base(template, category) {
			Construct(template);
		}


		#region IDisposable Interface

		public override void Dispose() {
			base.Dispose();
		}

		#endregion


		public override IEnumerable<DiagrammingAction> GetActions(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			yield break;
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, DiagrammingMouseEventArgs e) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			bool result = false;

			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;
			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						if (CurrentMouseState.Position != newMouseState.Position) {
							ProcessMouseMove(diagramPresenter, newMouseState);
						}
						break;
					case MouseEventType.MouseDown:
						// MouseDown starts drag-based actions
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						if (e.Clicks > 1) result = ProcessDoubleClick(diagramPresenter, newMouseState);
						break;

					case MouseEventType.MouseUp:
						// MouseUp finishes drag-actions. Click-based actions are handled by the MouseClick event
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						result = ProcessMouseClick(diagramPresenter, newMouseState);
						break;

					default: throw new DiagrammingUnsupportedValueException(e.EventType);
				}
				base.ProcessMouseEvent(diagramPresenter, e);
			} finally { diagramPresenter.ResumeUpdate(); }
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(DiagrammingKeyEventArgs e) {
			// nothing to do here
			return false;
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			Invalidate(diagramPresenter);
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
			Invalidate(diagramPresenter);
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (PreviewShape != null) {
				diagramPresenter.InvalidateGrips(PreviewShape, ControlPointCapabilities.All);
				Point p = PreviewShape.GetControlPointPosition(ControlPointId.LastVertex);
				InvalidateConnectionTargets(diagramPresenter, p.X, p.Y);
			}
			else InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.Position.X, CurrentMouseState.Position.Y);
		}


		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			Point gluePtPos;
			// Draw preview shape
			if (PreviewShape != null) {
				// Get current GluePoint position
				gluePtPos = PreviewShape.GetControlPointPosition(ControlPointId.LastVertex);
				// Draw preview shape and its ControlPoints
				diagramPresenter.DrawShape(PreviewShape);
				diagramPresenter.ResetTransformation();
				try {
					foreach (ControlPointId pointId in PreviewShape.GetControlPointIds(ControlPointCapabilities.Glue | ControlPointCapabilities.Resize))
						diagramPresenter.DrawResizeGrip(DiagrammingDrawMode.Normal, PreviewShape, pointId);
				} finally { diagramPresenter.RestoreTransformation(); }
			} else gluePtPos = CurrentMouseState.Position;

			// highlight ConnectionPoints in range
			DrawConnectionTargets(diagramPresenter, gluePtPos.X, gluePtPos.Y);


			//// highlight ConnectionPoints in range
			//ShapeAtCursorInfo targetShapeAtCursor = FindConnectionTarget(display, CurrentMouseState.Position.X, CurrentMouseState.Position.Y, false);
			//if (targetShapeAtCursor.IsEmpty) {
			//   Point gluePtPos;
			//   if (PreviewShape != null)
			//      gluePtPos = PreviewShape.GetControlPointPosition(EndPointId);
			//   else gluePtPos = CurrentMouseState.Position;
			//   DrawConnectionTargets(display, gluePtPos.X, gluePtPos.Y);
			//}

			//// draw shape and ResizeHandles
			//if (PreviewShape != null) {
			//   display.DrawShape(PreviewShape);
			//   Point gluePointPos = PreviewShape.GetControlPointPosition(EndPointId);
			//   DrawConnectionTargets(display, gluePointPos.X, gluePointPos.Y);
			//}
		}


		/// <override></override>
		public override void StartToolAction(IDiagramPresenter diagramPresenter, Point startPos, bool wantAutoScroll) {
			base.StartToolAction(diagramPresenter, startPos, wantAutoScroll);
		}


		/// <override></override>
		public override void EndToolAction() {
			base.EndToolAction();
			ClearPreview();
			lastInsertedPointId = ControlPointId.None;
			action = ToolAction.None;
		}


		/// <override></override>
		protected override void CancelCore() {
			// Create the line until the last point that was created manually.
			// This feature only makes sense if an additional ControlPoint was created (other than the default points)
			ILinearShape templateShape = Template.Shape as ILinearShape;
			ILinearShape previewShape = PreviewShape as ILinearShape;
			if (PreviewShape != null && previewShape.VertexCount > templateShape.VertexCount)
				FinishLine(ActionDiagramPresenter, CurrentMouseState, true);
		}


		#region Construction

		static LinearShapeCreationTool() {
			cursors = new Dictionary<ToolCursor, int>(6);
			cursors.Add(ToolCursor.Default, CursorProvider.DefaultCursorID);
			cursors.Add(ToolCursor.Pen, CursorProvider.RegisterCursor(Properties.Resources.PenCursor));
			cursors.Add(ToolCursor.MovePoint, CursorProvider.RegisterCursor(Properties.Resources.MovePointCursor));
			cursors.Add(ToolCursor.Connect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			cursors.Add(ToolCursor.Disconnect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			cursors.Add(ToolCursor.NotAllowed, CursorProvider.RegisterCursor(Properties.Resources.ActionDeniedCursor));
			// ToDo: Create better cursors for connecting/disconnecting
		}
		
		
		private void Construct(Template template) {
			if (!(template.Shape is ILinearShape))
				throw new DiagrammingException("The template's shape does not implement {0}.", typeof(ILinearShape).Name);
			if (template.Shape is PolylineBase)
				ToolTipText += "\n\rPolylines are finished by double clicking.";
		}

		#endregion


		private bool ProcessMouseMove(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			ShapeAtCursorInfo shapeAtCursor = ShapeAtCursorInfo.Empty;
			switch (CurrentAction) {
				case ToolAction.None:
					Invalidate(diagramPresenter);
					break;

				case ToolAction.AddPoint:
						Invalidate(ActionDiagramPresenter);
						shapeAtCursor = FindConnectionTarget(diagramPresenter, mouseState.Position.X, mouseState.Position.Y, false);

						// check for connectionpoints wihtin the snapArea
						if (!shapeAtCursor.IsEmpty){
							Point p = Point.Empty;
							if (shapeAtCursor.IsCursorAtGrip)
								p = shapeAtCursor.Shape.GetControlPointPosition(shapeAtCursor.ControlPointId);
							else p = mouseState.Position;
							// ToDo: Restore ResizeModifiers
							PreviewShape.MoveControlPointTo(ControlPointId.LastVertex, p.X, p.Y, ResizeModifiers.None);
						} else {
							int snapDeltaX = 0, snapDeltaY = 0;
							if (diagramPresenter.SnapToGrid)
								FindNearestSnapPoint(diagramPresenter, mouseState.Position.X, mouseState.Position.Y, out snapDeltaX, out snapDeltaY);
							// ToDo: Restore ResizeModifiers
							PreviewShape.MoveControlPointTo(ControlPointId.LastVertex, mouseState.Position.X + snapDeltaX, mouseState.Position.Y + snapDeltaY, ResizeModifiers.None);
						}
						Invalidate(ActionDiagramPresenter);
					break;

				case ToolAction.DrawLine:
				case ToolAction.MovePoint:
					throw new NotImplementedException();
				
				default: throw new DiagrammingUnsupportedValueException(CurrentAction);
			}
			// set cursor depending on the object under the mouse cursor
			int currentCursorId = DetermineCursor(diagramPresenter, shapeAtCursor.Shape, shapeAtCursor.ControlPointId);
			if (CurrentAction == ToolAction.None)
				diagramPresenter.SetCursor(currentCursorId);
			else ActionDiagramPresenter.SetCursor(currentCursorId);
			return result;
		}


		private bool ProcessMouseDown(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			return result;
		}


		private bool ProcessMouseUp(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			return result;
		}


		private bool ProcessMouseClick(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			switch (CurrentAction) {
				case ToolAction.None:
					if (mouseState.IsButtonDown(DiagrammingMouseButtons.Left)) {
						// If no other ToolAction is in Progress (e.g. drawing a line or moving a point),
						// a normal MouseClick starts a new line in Point-By-Point mode
						if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert)) {
							action = ToolAction.AddPoint;
							StartLine(diagramPresenter, mouseState);
							Invalidate(diagramPresenter);
						}
					} else if (mouseState.IsButtonDown(DiagrammingMouseButtons.Right)) {
						Cancel();
						result = true;
					}
					break;

				case ToolAction.AddPoint:
					if (mouseState.IsButtonDown(DiagrammingMouseButtons.Left)) {
						Invalidate(ActionDiagramPresenter);
						InsertNewPoint(ActionDiagramPresenter, mouseState);
						result = true;
						Invalidate(diagramPresenter);
					} else if (mouseState.IsButtonDown(DiagrammingMouseButtons.Right)) {
						Assert(PreviewShape != null);
						if (PreviewLinearShape.VertexCount <= PreviewLinearShape.MinVertexCount)
							Cancel();
						else {
							FinishLine(ActionDiagramPresenter, mouseState, false);
							EndToolAction();
						}
						result = true;
						OnToolExecuted(ExecutedEventArgs);
					}
					break;

				case ToolAction.DrawLine:
				case ToolAction.MovePoint:
					throw new NotImplementedException();
				default: throw new DiagrammingUnsupportedValueException(CurrentAction);
			}
			return result;
		}


		private bool ProcessDoubleClick(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			Assert(PreviewShape != null);
			FinishLine(ActionDiagramPresenter, mouseState, true);
			EndToolAction();
			result = true;

			OnToolExecuted(ExecutedEventArgs);
			return result;
		}


		private ILinearShape PreviewLinearShape {
			get { return (ILinearShape)PreviewShape; }
		}


		private ToolAction CurrentAction {
			get { return action; }
		}


		/// <summary>
		/// Creates a new preview line shape
		/// </summary>
		/// <param name="shape">The shape under/near the line's StartPoint</param>
		/// <param name="pointIdToConnect">The ControlPointId under/near the line's StartPoint</param>
		private void StartLine(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			// Try to find a connection target
			ShapeAtCursorInfo targetShape = FindConnectionTarget(diagramPresenter, mouseState.Position.X, mouseState.Position.Y, false);

			int snapDeltaX = 0, snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				if (targetShape.IsEmpty || targetShape.ControlPointId == ControlPointId.Reference)
					FindNearestSnapPoint(diagramPresenter, mouseState.Position.X, mouseState.Position.Y, out snapDeltaX, out snapDeltaY);
				else {
					Point p = targetShape.Shape.GetControlPointPosition(targetShape.ControlPointId);
					snapDeltaX = p.X - mouseState.Position.X;
					snapDeltaY = p.Y - mouseState.Position.Y;
				}
			}

			// set line's start coordinates
			Point start = Point.Empty;
			if (!targetShape.IsEmpty) {
				if (targetShape.ControlPointId == ControlPointId.Reference) {
					// ToDo: Get nearest point on line
					start = mouseState.Position;
					start.Offset(snapDeltaX, snapDeltaY);
				} else 
					start = targetShape.Shape.GetControlPointPosition(targetShape.ControlPointId);
			} else {
				start = mouseState.Position;
				start.Offset(snapDeltaX, snapDeltaY);
			}
			// Start ToolAction
			StartToolAction(diagramPresenter, start, true);

			// create new preview shape
			CreatePreview(diagramPresenter);
			// ToDo: Reactivate ResizeModifiers
			PreviewShape.MoveControlPointTo(ControlPointId.FirstVertex, start.X, start.Y, ResizeModifiers.None);
			PreviewShape.MoveControlPointTo(ControlPointId.LastVertex, mouseState.Position.X, mouseState.Position.Y, ResizeModifiers.None);
			lastInsertedPointId = ControlPointId.FirstVertex;
		}


		/// <summary>
		/// Inserts a new point into the current preview line before the end point (that is sticking to the mouse cursor).
		/// </summary>
		private void InsertNewPoint(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			Assert(PreviewLinearShape != null);
			if (PreviewLinearShape.VertexCount < PreviewLinearShape.MaxVertexCount) {
				ControlPointId existingPointId = ControlPointId.None;
				Point pointPos = PreviewShape.GetControlPointPosition(ControlPointId.LastVertex);
				foreach (ControlPointId ptId in PreviewShape.GetControlPointIds(ControlPointCapabilities.All)) {
					if (ptId == ControlPointId.Reference) continue;
					if (ptId == ControlPointId.LastVertex) continue;
					Point p = PreviewShape.GetControlPointPosition(ptId);
					if (p == pointPos && ptId != ControlPointId.Reference) {
						existingPointId = ptId;
						break;
					}
				}
				if (existingPointId == ControlPointId.None)
					lastInsertedPointId = PreviewLinearShape.InsertVertex(ControlPointId.LastVertex, pointPos.X, pointPos.Y);
			} else FinishLine(diagramPresenter, mouseState, false);
		}
		
		
		/// <summary>
		/// Creates a new LinearShape and inserts it into the diagram of the CurrentDisplay by executing a Command.
		/// </summary>
		/// <param name="createWithAllPoints">If true, the line will be created as a 
		/// clone of the preview shape. If false, the line will be created until the 
		/// last point inserted. The point at the mouse cursor will be skipped.</param>
		private void FinishLine(IDiagramPresenter diagramPresenter, MouseState mouseState, bool ignorePointAtMouse) {
			Assert(PreviewShape != null);
			// Create a new shape from the template
			Shape newShape = Template.CreateShape();
			// Copy points from the PreviewShape to the new shape 
			// The current EndPoint of the preview (sticking to the mouse cursor) will be discarded
			foreach (ControlPointId pointId in PreviewShape.GetControlPointIds(ControlPointCapabilities.Resize)) {
				Point p = PreviewShape.GetControlPointPosition(pointId);
				// skip ReferencePoint and EndPoint
				switch (pointId) {
					case StartPointId:
					case EndPointId:
						// Check if there are any occurences left...
						Assert(false);
						break;
					case ControlPointId.Reference:
						continue;
					case ControlPointId.LastVertex:
						// * If the line *has no* vertex limit, the last point (sticking to the mouse cursor) will 
						//   always be discarded 
						// * If the line *has a* vertex limit, the last point will be created
						// * If the tool was cancelled, the last point will be discarded.
						// * If the line has not enough vertices to discard one, the last will be created at the 
						//	  position of the mouse
						if ((PreviewLinearShape.VertexCount == PreviewLinearShape.MaxVertexCount && !ignorePointAtMouse)
							|| PreviewLinearShape.VertexCount == PreviewLinearShape.MinVertexCount)
							newShape.MoveControlPointTo(ControlPointId.LastVertex, p.X, p.Y, ResizeModifiers.None);
						else continue;
						break;
					case ControlPointId.FirstVertex:
						newShape.MoveControlPointTo(ControlPointId.FirstVertex, p.X, p.Y, ResizeModifiers.None);
						break;
					default:
						// treat the last inserted Point as EndPoint
						if (ignorePointAtMouse && pointId == lastInsertedPointId)
							newShape.MoveControlPointTo(ControlPointId.LastVertex, p.X, p.Y, ResizeModifiers.None);
						else {
							if (pointId == lastInsertedPointId && PreviewLinearShape.VertexCount < PreviewLinearShape.MaxVertexCount)
								newShape.MoveControlPointTo(ControlPointId.LastVertex, p.X, p.Y, ResizeModifiers.None);
							else ((ILinearShape)newShape).InsertVertex(ControlPointId.LastVertex, p.X, p.Y);
						}
						break;
				}
			}

			// Create an aggregated command which performs creation of the new shape and 
			// connecting the new shapes to other shapes in one step
			AggregatedCommand aggregatedCommand = new AggregatedCommand();
			if (Template.Shape.ModelObject != null)
				aggregatedCommand.Add(new InsertShapeAndModelCommand(ActionDiagramPresenter.Diagram, ActionDiagramPresenter.ActiveLayers, newShape, false));
			else
				aggregatedCommand.Add(new InsertShapeCommand(ActionDiagramPresenter.Diagram, ActionDiagramPresenter.ActiveLayers, newShape, false));

			foreach (ControlPointId ptId in newShape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				Point p = newShape.GetControlPointPosition(ptId);
				Shape targetShape = ActionDiagramPresenter.Diagram.Shapes.FindShape(p.X, p.Y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize, null);
				if (targetShape != null) {
					ControlPointId targetPtId = targetShape.FindNearestControlPoint(p.X, p.Y, 0, ControlPointCapabilities.Connect);
					if (targetPtId != ControlPointId.None)
						aggregatedCommand.Add(new ConnectCommand(newShape, ptId, targetShape, targetPtId));
				}
			}
			// execute command and insert it into the history
			ActionDiagramPresenter.Project.ExecuteCommand(aggregatedCommand);
			// select the created ConnectorShape
			ActionDiagramPresenter.SelectShape(newShape, false);
			OnToolExecuted(ExecutedEventArgs);
		}


		/// <summary>
		/// Set the cursor for the current action
		/// </summary>
		private int DetermineCursor(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId pointId) {
			switch (CurrentAction) {
				case ToolAction.None:
				case ToolAction.AddPoint:
					if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout)) {
						if (shape != null && shape is ILinearShape && pointId != ControlPointId.None)
							return cursors[ToolCursor.MovePoint];
						else if (pointId != ControlPointId.None) {
							if (!shape.HasControlPointCapability(pointId, ControlPointCapabilities.Glue) &&
								shape.HasControlPointCapability(pointId, ControlPointCapabilities.Connect)) {
								return cursors[ToolCursor.Connect];
							}
						} else if (shape != null) {
							if (shape.HasControlPointCapability(ControlPointId.Reference, ControlPointCapabilities.Connect))
								return cursors[ToolCursor.Connect];
						}
						return cursors[ToolCursor.Pen];
					} else return cursors[ToolCursor.NotAllowed];

				case ToolAction.DrawLine:
				case ToolAction.MovePoint:
					throw new NotImplementedException();
					//if (display.Project.SecurityManager.IsGranted(Permission.Insert)) {
					//   if (shape != null && pointId > 0 &&
					//      !shape.HasControlPointCapability(pointId, ControlPointCapabilities.Glue) &&
					//      shape.HasControlPointCapability(pointId, ControlPointCapabilities.Connect)) {
					//      currentCursor = connectCursor;
					//   }
					//   else if (shape != null && shape.HasControlPointCapability(ControlPointId.Reference, ControlPointCapabilities.Connect)) 
					//      currentCursor = connectCursor;
					//   else currentCursor = penCursor;
					//}
					//else currentCursor = notAllowedCursor;
					//break;

				default: throw new DiagrammingUnsupportedValueException(action);
			}
		}


		private enum ToolCursor {
			Default,
			NotAllowed,
			MovePoint,
			Pen,
			Connect,
			Disconnect
		}


		#region Fields

		// Definition of the tool
		private static Dictionary<ToolCursor, int> cursors;
		//
		private const int StartPointId = 1;
		private const int EndPointId = 2;
		
		// Tool's state definition
		// stores the last inserted Point (and its coordinates), which will become the EndPoint when the CurrentTool is cancelled
		private ControlPointId lastInsertedPointId;
		private ToolAction action;

		#endregion
	}
	

	/// <summary>
	/// Lets the user place a new shape on the diagram.
	/// </summary>
	public class PlanarShapeCreationTool : TemplateTool {

		public PlanarShapeCreationTool(Template template)
			: base(template) {
			Construct(template);
		}


		public PlanarShapeCreationTool(Template template, string category)
			: base(template, category) {
			Construct(template);
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, DiagrammingMouseEventArgs e) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			bool result = false;

			// Return if action is not allowed
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert))
				return result;

			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;
			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						if (newMouseState.Position != CurrentMouseState.Position) {
							// If no Preview exists, create a new one by starting a new ToolAction
							if (PreviewShape == null)
								StartToolAction(diagramPresenter, newMouseState.Position, false);

							Invalidate(ActionDiagramPresenter);
							// Move preview shape to Mouse Position
							PreviewShape.MoveTo(newMouseState.Position.X, newMouseState.Position.Y);
							// Snap to grid
							if (diagramPresenter.SnapToGrid) {
								int snapDeltaX = 0, snapDeltaY = 0;
								FindNearestSnapPoint(diagramPresenter, PreviewShape, 0, 0, out snapDeltaX, out snapDeltaY);
								PreviewShape.MoveTo(newMouseState.Position.X + snapDeltaX, newMouseState.Position.Y + snapDeltaY);
							}
							Invalidate(ActionDiagramPresenter);
							result = true;
						}
						break;

					case MouseEventType.MouseUp:
						if (newMouseState.IsButtonDown(DiagrammingMouseButtons.Left)) {
							// Left mouse button was pressed: Create shape
							Invalidate(ActionDiagramPresenter);
							int x = PreviewShape.X;
							int y = PreviewShape.Y;

							ICommand cmd;
							Shape newShape = Template.CreateShape();
							newShape.ZOrder = ActionDiagramPresenter.Project.Repository.ObtainNewTopZOrder(ActionDiagramPresenter.Diagram);
							if (Template.Shape.ModelObject != null)
								cmd = new InsertShapeAndModelCommand(ActionDiagramPresenter.Diagram, ActionDiagramPresenter.ActiveLayers, newShape, true, x, y);
							else
								cmd = new InsertShapeCommand(ActionDiagramPresenter.Diagram, ActionDiagramPresenter.ActiveLayers, newShape, true, x, y);
							ActionDiagramPresenter.Project.ExecuteCommand(cmd);

							newShape = ActionDiagramPresenter.Diagram.Shapes.FindShape(x, y, ControlPointCapabilities.None, 0, null);
							if (newShape != null) diagramPresenter.SelectShape(newShape, false);
							EndToolAction();
							result = true;

							OnToolExecuted(ExecutedEventArgs);
						} else if (newMouseState.IsButtonDown(DiagrammingMouseButtons.Right)) {
							// Right mouse button was pressed: Cancel Tool
							Cancel();
							result = true;
						}
						break;

					case MouseEventType.MouseDown:
						// nothing to to yet
						// ToDo 3: Implement dragging a frame with the mouse and fit the shape into that frame when releasing the button
						break;

					default: throw new DiagrammingUnsupportedValueException(e.EventType);
				}
			} finally { diagramPresenter.ResumeUpdate(); }
			base.ProcessMouseEvent(diagramPresenter, e);
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(DiagrammingKeyEventArgs e) {
			return base.ProcessKeyEvent(e);
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			if (!CurrentMouseState.IsEmpty)
				StartToolAction(diagramPresenter, CurrentMouseState.Position, false);
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
			EndToolAction();
		}
		
		
		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (drawPreview) {
				//if (DisplayContainsMousePos(ActionDisplay, CurrentMouseState.Position)) {
					diagramPresenter.DrawShape(PreviewShape);
					if (ActionDiagramPresenter.SnapToGrid) 
						diagramPresenter.DrawSnapIndicators(PreviewShape);
				//}
			}
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (PreviewShape != null && diagramPresenter.SnapToGrid) 
				diagramPresenter.InvalidateSnapIndicators(PreviewShape);
		}


		public override IEnumerable<DiagrammingAction> GetActions(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			yield break;
		}


		/// <override></override>
		protected override void CancelCore() {
			EndToolAction();
		}


		/// <override></override>
		public override void StartToolAction(IDiagramPresenter diagramPresenter, Point startPos, bool wantAutoScroll) {
			base.StartToolAction(diagramPresenter, startPos, wantAutoScroll);
			CreatePreview(ActionDiagramPresenter);
			PreviewShape.DisplayService = diagramPresenter.DisplayService;
			PreviewShape.MoveTo(startPos.X, startPos.Y);
			drawPreview = true;
			diagramPresenter.SetCursor(CurrentCursorId);
		}

		
		/// <override></override>
		public override void EndToolAction() {
			base.EndToolAction();
			drawPreview = false;
			ClearPreview();
		}


		static PlanarShapeCreationTool() {
			crossCursorId = CursorProvider.RegisterCursor(Properties.Resources.CrossCursor);
		}
		
		
		private void Construct(Template template) {
			if (!(template.Shape is IPlanarShape))
				throw new DiagrammingException("The template's shape does not implement {0}.", typeof(IPlanarShape).Name);
			drawPreview = false;
		}


		private int CurrentCursorId {
		   get { return drawPreview ? crossCursorId : CursorProvider.DefaultCursorID; }
		}


		#region Fields

		// Definition of the tool
		private static int crossCursorId;
		private bool drawPreview;

		#endregion
	}
	
	
	/// <summary>
	/// Lets the user sketch a shape using a pen.
	/// </summary>
	public class FreeHandTool : Tool {

		public FreeHandTool(Project project)
			: base("Standard") {
			Construct(project);
		}


		public FreeHandTool(Project project, string category)
			: base(category) {
			Construct(project);
		}


		#region Tool Implementation

		/// <override></override>
		public override void RefreshIcons() {
			// nothing to do
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, DiagrammingMouseEventArgs e) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			bool result = false;
			
			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;
			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseDown:
						timer.Stop();
						break;

					case MouseEventType.MouseMove:
						if (CurrentMouseState.Position != newMouseState.Position) {
							if (newMouseState.IsButtonDown(DiagrammingMouseButtons.Left)
								&& diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert)) {
								diagramPresenter.ControlToDiagram(e.Position, out p);
								currentStroke.Add(p.X, p.Y);
							}
							diagramPresenter.SetCursor(penCursorId);
						}
						Invalidate(diagramPresenter);
						break;

					case MouseEventType.MouseUp:
						if (newMouseState.IsButtonDown(DiagrammingMouseButtons.Left)
							&& diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert)) {
							StartToolAction(diagramPresenter, newMouseState.Position, false);

							strokeSet.Add(currentStroke);
							currentStroke = new Stroke();
							timer.Start();
						}
						break;

					default: throw new DiagrammingUnsupportedValueException(e.EventType);
				}
			} finally { diagramPresenter.ResumeUpdate(); }
			base.ProcessMouseEvent(diagramPresenter, e);
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(DiagrammingKeyEventArgs e) {
			// nothing to do
			return false;
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			// nothing to do
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			// nothing to do
		}

		
		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			diagramPresenter.ResetTransformation();
			try {
				// draw stroke(s)
				foreach (Stroke stroke in strokeSet) {
					for (int i = 0; i < stroke.Count - 1; ++i)
						diagramPresenter.DrawLine(stroke[i], stroke[i + 1]);
				}
				// draw stroke(s)
				for (int i = 0; i < currentStroke.Count - 1; ++i)
					diagramPresenter.DrawLine(currentStroke[i], currentStroke[i + 1]);
			} finally { diagramPresenter.RestoreTransformation(); }
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
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
			if (diagramPresenter != null) {
				diagramPresenter.ControlToDiagram(rect, out rect);
				if (strokeSet.Count > 0 || currentStroke.Count > 0)
					diagramPresenter.InvalidateDiagram(x, y, width, height);
			}
		}


		public override IEnumerable<DiagrammingAction> GetActions(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			yield break;
		}


		///// <override></override>
		//public override Cursor Cursor {
		//   get { return penCursor; }
		//}


		/// <override></override>
		protected override void CancelCore() {
			//InvalidatePreview(currentDisplay);
			currentStroke.Clear();
			strokeSet.Clear();
		}


		#endregion


		public override void Dispose() {
			base.Dispose();

			timer.Stop();
			timer.Elapsed -= timer_Tick;
			timer.Dispose();
		}


		static FreeHandTool() {
			penCursorId = CursorProvider.RegisterCursor(Properties.Resources.PenCursor);
		}
		
		
		private void Construct(Project project) {
			if (project == null) throw new ArgumentNullException("project");

			Title = "Freehand Pen";
			ToolTipText = "Draw the symbol of the object which should be created.";

			SmallIcon = global::Dataweb.Diagramming.Properties.Resources.FreehandIconSmall;
			SmallIcon.MakeTransparent(Color.Fuchsia);
			LargeIcon = global::Dataweb.Diagramming.Properties.Resources.FreehandIconLarge;
			LargeIcon.MakeTransparent(Color.Fuchsia);

			polygone = new PathFigureShape();
			strokeSet = new StrokeSequence();
			currentStroke = new Stroke();
			shaper = new Shaper();

			timer = new Timer();
			timer.Enabled = false;
			timer.Interval = timeOut;
			timer.Elapsed += timer_Tick;

			this.project = project;
			project.LibraryLoaded += project_LibraryLoaded;
			RegisterFigures();
		}


		private void project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			RegisterFigures();
		}


		private void RegisterFigures() {
			foreach (ShapeType shapeType in project.ShapeTypes)
				if (!shaper.IsRegistered(shapeType.FullName))
					shaper.RegisterFigure(shapeType.FullName, shapeType.FreehandReferenceImage);
		}


		private void timer_Tick(object sender, EventArgs e) {
			timer.Stop();
			IdentifyFigure(ActionDiagramPresenter);
		}


		private void IdentifyFigure(IDiagramPresenter diagramPresenter) {
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

			if (diagramPresenter != null && figureNames.Count > 0) {
				if (diagramPresenter.Project.Repository == null)
					throw new NullReferenceException("Unable to access repository of current ownerDisplay.");

				matchingTemplates.Clear();
				foreach (Template t in diagramPresenter.Project.Repository.GetTemplates()) {
					foreach (string figName in figureNames) {
						if (t.Shape.Type.FullName == figName) {
							matchingTemplates.Add(t);
						}
					}
				}

				if (matchingTemplates.Count == 1) {
					CreateShape(diagramPresenter, matchingTemplates[0]);
				} else if (matchingTemplates.Count > 1) {
					
					// ToDo: Create "CreateShapeFromTemplateAction" and build the ContextMenu from actions here
					// show context menu with matching templates
					
					// ToDo: Find a solution for displaying a context menu in the display
					//contextMenu.Items.Clear();
					//foreach (Template t in matchingTemplates) {
					//   ToolStripMenuItem item = new ToolStripMenuItem(t.Name, t.CreateThumbnail(16, 2), ContextMenuItem_Click);
					//   item.Tag = t;
					//   contextMenu.Items.Add(item);
					//}

					//int x, y, width, height;
					//this.GetStrokeSetBounds(out x, out y, out width, out height);
					//contextMenu.Show(x, y);
				}
			}

			Invalidate(diagramPresenter);

			strokeSet.Clear();
			currentStroke.Clear();

			OnToolExecuted(ExecutedEventArgs);
		}


		private void ContextMenuItem_Click(object sender, EventArgs e) {
			//if (sender is ToolStripMenuItem) {
			//   Template t = (Template) ((ToolStripMenuItem)sender).Tag;
			//   CreateShape(ActionDisplay, t);
			//}
		}


		private void CreateShape(IDiagramPresenter diagramPresenter, Template template) {
			// create shape
			Shape shape = template.Shape.Clone();
			if (template.Shape.ModelObject != null)
				shape.ModelObject = template.Shape.ModelObject.Clone();

			int x, y, width, height;
			GetStrokeSetBounds(out x, out y, out width, out height);
			shape.Fit(x, y, width, height);
			
			ICommand cmd;
			if (shape.ModelObject != null)
				cmd = new InsertShapeAndModelCommand(diagramPresenter.Diagram, diagramPresenter.ActiveLayers, shape, false);
			else
				cmd = new InsertShapeCommand(diagramPresenter.Diagram, diagramPresenter.ActiveLayers, shape, false);
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

		private static int penCursorId;
		private Project project;

		private readonly Brush[] brushes = new Brush[] { Brushes.Blue, Brushes.Red, Brushes.Green, Brushes.Pink, Brushes.Plum };
		private Shaper shaper;
		private PathFigureShape polygone;
		private StrokeSequence strokeSet;
		private Stroke currentStroke;

		// ToDo: Timer ins Display verlagern
		private Timer timer;
		private const int timeOut = 1250;

		private List<Template> matchingTemplates = new List<Template>();
		private System.Drawing.Rectangle rect;

		//private ContextMenuStrip contextMenu = new ContextMenuStrip();

		// buffer for coordinate conversions
		private Point p;

		#endregion
	}
}