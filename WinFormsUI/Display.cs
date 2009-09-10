using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using Dataweb.nShape.Advanced;
using Dataweb.nShape.Controllers;


namespace Dataweb.nShape.WinFormsUI {

	public partial class Display : UserControl, IDiagramPresenter, IDisplayService {
		
		public Display() {
			// Enable DoubleBuffered painting
			this.SetStyle(ControlStyles.AllPaintingInWmPaint 
				//| ControlStyles.OptimizedDoubleBuffer 
				| ControlStyles.ResizeRedraw 
				| ControlStyles.ContainerControl
				| ControlStyles.SupportsTransparentBackColor
				| ControlStyles.UserPaint
				, true);
			this.UpdateStyles();
			infoGraphics = Graphics.FromHwnd(this.Handle);

			// Initialize components
			InitializeComponent();
			AllowDrop = true;
			
			// Set event handlers
			scrollBarH.Scroll += ScrollBar_Scroll;
			scrollBarV.Scroll += ScrollBar_Scroll;
			scrollBarH.MouseEnter += ScrollBars_MouseEnter;
			scrollBarV.MouseEnter += ScrollBars_MouseEnter;
			scrollBarH.VisibleChanged += scrollBar_VisibleChanged;
			scrollBarV.VisibleChanged += scrollBar_VisibleChanged;

			hScrollBarPanel.BackColor = BackColor;

			// Calculate grip shapes
			CalcControlPointShape(resizePointPath, resizePointShape, handleRadius);
			CalcControlPointShape(rotatePointPath, ControlPointShape.RotateArrow, handleRadius);
			CalcControlPointShape(connectionPointPath, connectionPointShape, handleRadius);
			//
			previewTextFormatter.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
			previewTextFormatter.Trimming = StringTrimming.EllipsisCharacter;

			//
			gridSize.Width = gridSpace;
			gridSize.Height = gridSpace;

			// used for fixed refresh rate rendering
			//displayRefreshTimer.Interval = 10;	// 100 fps
			displayRefreshTimer.Interval = 16;	// 60 fps
			//displayRefreshTimer.Interval = 33;	// 30 fps
			displayRefreshTimer.Tick += displayRefreshTimer_Tick;
			//displayRefreshTimer.Start();

			autoScrollTimer.Interval = 50;
			autoScrollTimer.Tick += autoScrollTimer_Tick;
		}


		~Display() {
			Dispose();
		}


		#region IDisplayService Members (explicit implementation)

		// Invalidate the given area (Diagram coordinates)
		void IDisplayService.Invalidate(int x, int y, int width, int height) {
			InvalidateDiagram(x, y, width, height);
		}


		void IDisplayService.Invalidate(Rectangle rectangle) {
			InvalidateDiagram(rectangle);
		}


		/// <override></override>
		void IDisplayService.NotifyBoundsChanged() {
			UpdateScrollBars();
			Invalidate();
		}
		
		
		Graphics IDisplayService.InfoGraphics {
			get { return infoGraphics; }
		}


		IFillStyle IDisplayService.HintBackgroundStyle {
			get {
				if (hintBackgroundStyle == null) {
					hintBackgroundStyle = new FillStyle("Hint Background Style");
					hintBackgroundStyle.BaseColorStyle = new ColorStyle("Hint Background Color", SelectionInteriorColor);
					hintBackgroundStyle.AdditionalColorStyle = hintBackgroundStyle.BaseColorStyle;
					hintBackgroundStyle.FillMode = FillMode.Solid;
				}
				return hintBackgroundStyle;
			}
		}


		ILineStyle IDisplayService.HintForegroundStyle {
			get {
				if (hintForegroundStyle == null) {
					hintForegroundStyle = new LineStyle("Hint Foreground Line Style");
					hintForegroundStyle.ColorStyle = new ColorStyle("Hint Foreground Color", ToolPreviewColor);
					hintForegroundStyle.DashCap = DashCap.Round;
					hintForegroundStyle.LineJoin = LineJoin.Round;
					hintForegroundStyle.LineWidth = 1;
				}
				return hintForegroundStyle;
			}
		}

		#endregion


		#region [Public] Events
		
		public event EventHandler ShapesSelected;

		public event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeClick;

		public event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeDoubleClick;

		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeInsert;

		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeRemove;

		public event EventHandler ZoomChanged;

		public event EventHandler DiagramChanging;
		
		public event EventHandler DiagramChanged;

		public event EventHandler<LayersEventArgs> LayerVisibilityChanged;

		public event EventHandler<LayersEventArgs> ActiveLayersChanged;

		#endregion


		#region [Public] Properties 

		[Browsable(false)]
		public Project Project {
			get { return (diagramSetController == null) ? null : diagramSetController.Project; }
		}


		[Browsable(false)]
		public IDisplayService DisplayService {
			get { return this; }
		}


		[Category("nShape")]
		public DiagramSetController DiagramSetController {
			get { return diagramSetController; }
			set {
				if (diagramSetController != null) UnregisterDiagramSetControllerEvents();
				// Controller has to be assigned to the property (instead of the field) in order to process delegates
				//if (value == null) Controller = null;
				//else Controller = value.OpenDiagram();
				diagramSetController = value;
				if (diagramSetController != null) RegisterDiagramSetControllerEvents();
			}
		}


		[Category("nShape")]
		public Tool CurrentTool {
			get { return (diagramController == null) ? null : diagramController.Owner.ActiveTool; }
			set { 
				if (diagramController != null) diagramController.Owner.ActiveTool = value; 
			}
		}


		[ReadOnly(true)]
		[Category("nShape")]
		public Diagram Diagram {
			get { return diagramController == null ? null : diagramController.Diagram; }
			set {
				if (diagramSetController == null) throw new ArgumentNullException("DiagramSetController");
				if (Diagram != null) 
					Controller = null;	// Close diagram and unregister events				
				if (value != null)
					Controller = diagramSetController.OpenDiagram(value);	// Register events
				
				//Debug.Assert(diagramController != null);
				//if (diagramController.Diagram != null) diagramController.Diagram.DisplayService = null;
				//diagramController.Diagram = value;
				//if (diagramController.Diagram != null) diagramController.Diagram.DisplayService = this;
			}
		}


		[Browsable(false)]
		public IShapeCollection SelectedShapes { get { return selectedShapes; } }


		/// <summary>
		/// Bounds of the display's drawing area (client area minus scroll bars) in control coordinates
		/// </summary>
		public Rectangle DrawBounds {
			get {
				if (drawBounds == Geometry.InvalidRectangle) {
					drawBounds.X = Left;
					drawBounds.Y = Top;
					if (scrollBarV.Visible) drawBounds.Width = Width - scrollBarV.Width;
					else drawBounds.Width = Width;
					if (scrollBarH.Visible) drawBounds.Height = Height - scrollBarH.Height;
					else drawBounds.Height = Height;
				}
				return drawBounds;
			}
		}


		public override ContextMenuStrip ContextMenuStrip {
			get {
				ContextMenuStrip result = base.ContextMenuStrip;
				if (result == null && ShowDefaultContextMenu)
					result = displayContextMenuStrip;
				return result;
			}
			set {
				base.ContextMenuStrip = value;
			}
		}


		/// <summary>
		/// Zoom in percentage.
		/// </summary>
		public int ZoomLevel {
			get { return zoomLevel; }
			set {
				if (value <= 0)
					throw new nShapeException("NotSupported value: Value has to be greater than 0.");
				zoomLevel = value;
				zoomfactor = value / 100f;
				UpdateScrollBars();

				UnselectShapesOfInvisibleLayers();
				Invalidate();

				if (ZoomChanged != null) ZoomChanged(this, null);
			}
		}


		public LayerIds ActiveLayers {
			get { return activeLayers; }
		}


		public LayerIds HiddenLayers {
			get { return hiddenLayers; }
		}

		#endregion


		#region [Public] Properties: Behavior and appearance

		[Category("Behaviour")]
		public bool ZoomWithMouseWheel {
			get { return zoomWithMouseWheel; }
			set { zoomWithMouseWheel = value; }
		}
		
		
		[Category("Behavior")]
		public bool ShowScrollBars {
			get { return showScrollBars; }
			set { showScrollBars = value; }
		}


		[Category("Behavior")]
		public bool SnapToGrid {
			get { return snapToGrid; }
			set { snapToGrid = value; }
		}


		[Category("Behavior")]
		public int SnapDistance {
			get { return snapDistance; }
			set { snapDistance = value; }
		}


		[Category("Behavior")]
		[DefaultValue(true)]
		public bool ShowDefaultContextMenu {
			get { return showDefaultContextMenu; }
			set { showDefaultContextMenu = value; }
		}


		[Category("Behavior")]
		[DefaultValue(true)]
		public int MinRotateRange {
			get { return minRotateDistance; }
			set { minRotateDistance = value; }
		}


		[Category("Appearance")]
		public int GridSize {
			get { return this.gridSpace; }
			set {
				if (value <= 0)
					throw new Exception("Value has to be > 0.");
				gridSpace = value;
				gridSize.Width = gridSpace;
				gridSize.Height = gridSpace;
			}
		}


		/// <summary>
		/// The pieRadius of a ControlPoint handle from the center to the outer handle bound.
		/// </summary>
		[Category("Appearance")]
		public int GripSize {
			get { return handleRadius; }
			set {
				handleRadius = value;
				invalidateDelta = handleRadius;

				CalcControlPointShape(rotatePointPath, ControlPointShape.RotateArrow, handleRadius);
				CalcControlPointShape(resizePointPath, resizePointShape, handleRadius);
				CalcControlPointShape(connectionPointPath, connectionPointShape, handleRadius);
				Invalidate();
			}
		}


		/// <summary>
		/// Provides the size (radius) of ControlPoint grips according to the current zoom in diagram coordinates.
		/// </summary>
		[Browsable(false)]
		public int ZoomedGripSize {
			get { return Math.Max(1, (int)Math.Round(handleRadius / zoomfactor)); }
		}


		[Category("Appearance")]
		public bool ShowGrid {
			get { return gridVisible; }
			set {
				gridVisible = value;
				Invalidate(this.drawBounds);
			}
		}


		[Category("Appearance")]
		public bool HighQualityRendering {
			get { return highQualityRendering; }
			set {
				highQualityRendering = value;
				currentRenderingQuality = highQualityRendering ? renderingQualityHigh : renderingQualityLow;
				if (controlBrush != null) {
					controlBrush.Dispose();
					controlBrush = null;
				}
				if (infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(infoGraphics, currentRenderingQuality);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public bool HighQualityBackground {
			get { return highQualityBackground; }
			set {
				highQualityBackground = value;
				if (Diagram != null)
					Diagram.HighQualityRendering = value;
				if (controlBrush != null) {
					controlBrush.Dispose();
					controlBrush = null;
				}
				if (infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(infoGraphics, currentRenderingQuality);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public ControlPointShape ResizeGripShape {
			get { return resizePointShape; }
			set {
				resizePointShape = value;
				CalcControlPointShape(resizePointPath, resizePointShape, handleRadius);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public ControlPointShape ConnectionPointShape {
			get { return connectionPointShape; }
			set {
				connectionPointShape = value;
				CalcControlPointShape(connectionPointPath, connectionPointShape, handleRadius);
				Invalidate();
			}
		}

		#endregion


		#region [Public] Properties: Colors

		[Category("Appearance")]
		public float BackgroundGradientAngle {
			get { return controlBrushGradientAngle; }
			set {
				controlBrushGradientAngle = value;
				controlBrushGradientSin = Math.Sin(Geometry.DegreesToRadians(controlBrushGradientAngle));
				controlBrushGradientCos = Math.Cos(Geometry.DegreesToRadians(controlBrushGradientAngle));

				if (controlBrush != null) {
					controlBrush.Dispose();
					controlBrush = null;
				}
				Invalidate();
			}
		}


		[Category("Appearance")]
		public override Color BackColor {
			get { return backColor; }
			set {
				backColor = value;
				hScrollBarPanel.BackColor = value;
				if (controlBrush != null) {
					controlBrush.Dispose();
					controlBrush = null;
				}
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color BackColorGradient {
			get { return gradientBackColor; }
			set {
				gradientBackColor = value;

				if (controlBrush != null) {
					controlBrush.Dispose();
					controlBrush = null;
				}
				Invalidate();
			}
		}


		[Category("Appearance")]
		public byte GridAlpha {
			get { return gridAlpha; }
			set {
				gridAlpha = value;
				CreatePen(gridColor, gridAlpha, ref gridPen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public byte ControlPointAlpha {
			get { return selectionAlpha; }
			set {
				selectionAlpha = value;

				CreateBrush(selectionInteriorColor, selectionAlpha, ref handleInteriorBrush);
				CreatePen(selectionInteriorColor, selectionAlpha, handleRadius, ref outlineNormalPen);
				CreatePen(selectionInteriorColor, selectionAlpha, handleRadius, ref outlineHilightPen);
				CreatePen(selectionInteriorColor, selectionAlpha, handleRadius, ref outlineInactivePen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color GridColor {
			get { return gridColor; }
			set {
				gridColor = value;

				CreatePen(gridColor, gridAlpha, ref gridPen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color SelectionInteriorColor {
			get { return selectionInteriorColor; }
			set {
				if (hintBackgroundStyle != null) {
					ToolCache.NotifyStyleChanged(hintBackgroundStyle);
					hintBackgroundStyle = null;
				}
				selectionInteriorColor = value;

				CreatePen(selectionInteriorColor, ref outlineInteriorPen);
				CreateBrush(selectionInteriorColor, selectionAlpha, ref handleInteriorBrush);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color SelectionNormalColor {
			get { return selectionNormalColor; }
			set {
				selectionNormalColor = value;

				CreatePen(selectionNormalColor, selectionAlpha, handleRadius, ref outlineNormalPen);
				CreatePen(selectionNormalColor, selectionAlpha, ref outlineNormalPen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color SelectionHilightColor {
			get { return selectionHilightColor; }
			set {
				selectionHilightColor = value;

				CreatePen(selectionHilightColor, selectionAlpha, handleRadius, ref outlineNormalPen);
				CreatePen(selectionHilightColor, selectionAlpha, ref outlineNormalPen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color SelectionInactiveColor {
			get { return selectionInactiveColor; }
			set {
				selectionInactiveColor = value;

				CreatePen(selectionInactiveColor, selectionAlpha, handleRadius, ref outlineNormalPen);
				CreatePen(selectionInactiveColor, selectionAlpha, ref outlineNormalPen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color ToolPreviewColor {
			get { return toolPreviewColor; }
			set {
				if (hintForegroundStyle != null) {
					ToolCache.NotifyStyleChanged(hintForegroundStyle);
					hintForegroundStyle = null;
				}
				toolPreviewColor = value;

				CreatePen(toolPreviewColor, toolPreviewColorAlpha, ref toolPreviewPen);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public Color ToolPreviewBackColor {
			get { return toolPreviewBackColor; }
			set {
				toolPreviewBackColor = value;

				CreateBrush(toolPreviewBackColor, toolPreviewBackColorAlpha, ref toolPreviewBackBrush);
				Invalidate();
			}
		}


		[Category("Appearance")]
		public nShapeRenderingQuality RenderingQualityHighQuality {
			get { return renderingQualityHigh; }
			set {
				renderingQualityHigh = value;
				if (highQualityRendering) {
					currentRenderingQuality = renderingQualityHigh;
					if (infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(infoGraphics, currentRenderingQuality);
				}
			}
		}


		[Category("Appearance")]
		public nShapeRenderingQuality RenderingQualityLowQuality {
			get { return renderingQualityLow; }
			set {
				renderingQualityLow = value;
				if (!highQualityRendering) {
					currentRenderingQuality = renderingQualityLow;
					if (infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(infoGraphics, currentRenderingQuality);
				}
			}
		}


		#endregion


		#region [Public] Methods: Coordinate transformation

		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(int dX, int dY, out int cX, out int cY) {
			cX = diagramPosX + (int)Math.Round((dX - scrollPosX) * zoomfactor);
			cY = diagramPosY + (int)Math.Round((dY - scrollPosY) * zoomfactor);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(Point dPt, out Point cPt) {
			cPt = Point.Empty;
			cPt.Offset(
				diagramPosX + (int)Math.Round((dPt.X - scrollPosX) * zoomfactor),
				diagramPosY + (int)Math.Round((dPt.Y - scrollPosY) * zoomfactor)
				);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(Rectangle dRect, out Rectangle cRect) {
			cRect = Rectangle.Empty;
			cRect.Offset(
				diagramPosX + (int)Math.Round((dRect.X - scrollPosX) * zoomfactor),
				diagramPosY + (int)Math.Round((dRect.Y - scrollPosY) * zoomfactor)
				);
			cRect.Width = (int)Math.Round(dRect.Width * zoomfactor);
			cRect.Height = (int)Math.Round(dRect.Height * zoomfactor);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(int dDistance, out int cDistance) {
			cDistance = (int)Math.Round(dDistance * zoomfactor);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(Size dSize, out Size cSize) {
			cSize = Size.Empty;
			cSize.Width = (int)Math.Round(dSize.Width * zoomfactor);
			cSize.Height = (int)Math.Round(dSize.Height * zoomfactor);
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(int cX, int cY, out int dX, out int dY) {
			dX = (int)Math.Round((cX - diagramPosX) / zoomfactor) + scrollPosX;
			dY = (int)Math.Round((cY - diagramPosY) / zoomfactor) + scrollPosY;
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(Point cPt, out Point dPt) {
			dPt = Point.Empty;
			dPt.X = (int)Math.Round((cPt.X - diagramPosX) / zoomfactor) + scrollPosX;
			dPt.Y = (int)Math.Round((cPt.Y - diagramPosY) / zoomfactor) + scrollPosY;
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(Rectangle cRect, out Rectangle dRect) {
			dRect = Rectangle.Empty;
			dRect.X = (int)Math.Round((cRect.X - diagramPosX) / zoomfactor) + scrollPosX;
			dRect.Y = (int)Math.Round((cRect.Y - diagramPosY) / zoomfactor) + scrollPosY;
			dRect.Width = (int)Math.Round((cRect.Width / zoomfactor));
			dRect.Height = (int)Math.Round((cRect.Height / zoomfactor));
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(Size cSize, out Size dSize) {
			dSize = Size.Empty;
			dSize.Width = (int)Math.Round((cSize.Width / zoomfactor));
			dSize.Height = (int)Math.Round((cSize.Height / zoomfactor));
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(int cDistance, out int dDistance) {
			dDistance = (int)Math.Round((cDistance / zoomfactor));
		}


		/// <summary>
		/// Transformes screen coordinates to diagram coordinates
		/// </summary>
		public void ScreenToDiagram(Point sPt, out Point dPt) {
			ControlToDiagram(PointToClient(sPt), out dPt);
		}


		/// <summary>
		/// Transformes screen coordinates to diagram coordinates
		/// </summary>
		public void ScreenToDiagram(Rectangle sRect, out Rectangle dRect) {
			ControlToDiagram(RectangleToClient(sRect), out dRect);
		}

		#endregion


		#region [Public] Methods: Drawing and Painting


		public void InvalidateDiagram(int x, int y, int width, int height) {
			rectBuffer.X = x;
			rectBuffer.Y = y;
			rectBuffer.Width = width;
			rectBuffer.Height = height;
			InvalidateDiagram(rectBuffer);
		}


		public void InvalidateDiagram(Rectangle rect) {
			if (rect == Geometry.InvalidRectangle) throw new ArgumentException("rect");
			DiagramToControl(rect, out rectBuffer);
			rectBuffer.Inflate(invalidateDelta + 1, invalidateDelta + 1);

			//// Timer based fixed-fps-rate rendering
			//if (InvalidationBuffer.IsEmpty)
			//   InvalidationBuffer = area;
			//else {
			//   InvalidationBuffer.X = Math.Min(InvalidationBuffer.X, area.X);
			//   InvalidationBuffer.Y = Math.Min(InvalidationBuffer.Y, area.Y);
			//   InvalidationBuffer.Width = Math.Max(InvalidationBuffer.right, area.right) - InvalidationBuffer.X;
			//   InvalidationBuffer.Height = Math.Max(InvalidationBuffer.bottom, area.bottom) - InvalidationBuffer.Y;
			//}
			//performInvalidation = true;

			// traditional rendering
			if (suspendInvalidateCounter > 0) invalidatedAreaBuffer = Geometry.UniteRectangles(invalidatedAreaBuffer, rect);
			else base.Invalidate(rectBuffer);

#if DEBUG
			Rectangle r = Rectangle.Empty;
			r.Width = 100;
			r.Height = 20;
			if (suspendInvalidateCounter > 0) invalidatedAreaBuffer = Geometry.UniteRectangles(invalidatedAreaBuffer, r); 
			base.Invalidate(r);
#endif
		}


		/// <summary>
		/// Invalidates the bounding rectangle around the shape and all its (suitable) control points.
		/// </summary>
		public void InvalidateGrips(Shape shape, ControlPointCapabilities controlPointCapability) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = Point.Empty;
			int transformedHandleRadius;
			ControlToDiagram(handleRadius, out transformedHandleRadius);
			++transformedHandleRadius;
			Rectangle r = Rectangle.Empty;
			foreach (ControlPointId id in shape.GetControlPointIds(controlPointCapability)) {
				p = shape.GetControlPointPosition(id);
				if (r.IsEmpty) {
					r.X = p.X;
					r.Y = p.Y;
					r.Width = r.Height = 1;
				} else r = Geometry.UniteRectangles(p.X, p.Y, p.X, p.Y, r);
			}
			r.Inflate(transformedHandleRadius, transformedHandleRadius);
			InvalidateDiagram(r);

			// This consumes twice the time of the solution above:
			//int transformedHandleSize = transformedHandleRadius+transformedHandleRadius;
			//foreach (int pointId in shape.GetControlPointIds(controlPointCapabilities)) {
			//   p = shape.GetControlPointPosition(pointId);
			//   Invalidate(p.X - transformedHandleRadius, p.Y - transformedHandleRadius, transformedHandleSize, transformedHandleSize);
			//}
		}


		/// <summary>
		/// Invalidates the bounding rectangle around all shapes and all their (suitable) control points.
		/// </summary>
		public void InvalidateGrips(IEnumerable<Shape> shapes, ControlPointCapabilities controlPointCapability) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			Point p = Point.Empty;
			int transformedHandleRadius;
			ControlToDiagram(handleRadius, out transformedHandleRadius);
			++transformedHandleRadius;
			Rectangle r = Rectangle.Empty;
			foreach (Shape shape in shapes)
				r = Geometry.UniteRectangles(shape.GetBoundingRectangle(false), r);
			r.Inflate(transformedHandleRadius, transformedHandleRadius);
			InvalidateDiagram(r);
		}


		public void InvalidateSnapIndicators(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			int transformedPointRadius, transformedGridSize;
			ControlToDiagram(GripSize, out transformedPointRadius);
			transformedGridSize = (int)Math.Round(GridSize * (ZoomLevel / 100f));

			Rectangle bounds = shape.GetBoundingRectangle(false);
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.All)) {
				Point p = Point.Empty;
				p = shape.GetControlPointPosition(id);
				bounds = Geometry.UniteRectangles(p.X, p.Y, p.X, p.Y, bounds);
			}
			bounds.Inflate(transformedPointRadius, transformedPointRadius);
			InvalidateDiagram(bounds);
		}


		void IDiagramPresenter.SuspendUpdate() {
			invalidatedAreaBuffer = Rectangle.Empty;
			++suspendInvalidateCounter;
		}


		void IDiagramPresenter.ResumeUpdate() {
			if (suspendInvalidateCounter == 0) throw new nShapeException("Missing subsequent call of method SuspendInvalidate.");
			--suspendInvalidateCounter;
			InvalidateDiagram(invalidatedAreaBuffer);
		}


		public void ResetTransformation() {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			ResetTransformation(currentGraphics);
		}


		public void RestoreTransformation() {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			RestoreTransformation(currentGraphics, diagramPosX, diagramPosY, scrollPosX, scrollPosY, zoomfactor);
		}


		public void DrawShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (!graphicsIsTransformed) throw new InvalidOperationException("RestoreTransformation has to be called before calling this method.");
			shape.Draw(currentGraphics);
		}


		public void DrawShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (!graphicsIsTransformed) throw new InvalidOperationException("RestoreTransformation has to be called before calling this method.");
			foreach (Shape shape in shapes)
				shape.Draw(currentGraphics);
		}


		public void DrawControlPoints(Shape shape) {
			DrawControlPoints(shape, ControlPointCapabilities.Resize | ControlPointCapabilities.Connect | ControlPointCapabilities.Glue);
		}


		public void DrawControlPoints(Shape shape, ControlPointCapabilities capabilities) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before calling this method.");
			Point p = Point.Empty;
			// first, draw Resize- and ConnectionPoints
			foreach (ControlPointId id in shape.GetControlPointIds(capabilities)) {
				if (id == ControlPointId.Reference) continue;

				// Get point position and transform the coordinates
				p = shape.GetControlPointPosition(id);
				if (shape.HasControlPointCapability(id, ControlPointCapabilities.Connect | ControlPointCapabilities.Glue))
					DrawConnectionPointCore(shape, id, p.X, p.Y, nShapeDrawMode.Normal);
				else if (shape.HasControlPointCapability(id, ControlPointCapabilities.Resize))
					DrawResizeGripCore(shape, p.X, p.Y, nShapeDrawMode.Normal);
			}
			// draw the roation point on top of all other points
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
				p = shape.GetControlPointPosition(id);
				DrawRotateGripCore(shape, p.X, p.Y, nShapeDrawMode.Normal);
			}
		}


		public void DrawConnectionPoints(nShapeDrawMode drawMode, Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			Point p = Point.Empty;
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Connect)) {
				p = shape.GetControlPointPosition(id);
				DrawConnectionPointCore(shape, id, p.X, p.Y, drawMode);
			}
		}


		public void DrawResizeGrip(nShapeDrawMode drawMode, Shape shape, ControlPointId pointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(pointId);
			DrawResizeGripCore(shape, p.X, p.Y, drawMode);
		}


		public void DrawRotateGrip(nShapeDrawMode drawMode, Shape shape, ControlPointId pointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(pointId);
			DrawRotateGripCore(shape, p.X, p.Y, drawMode);
		}


		public void DrawConnectionPoint(nShapeDrawMode drawMode, Shape shape, ControlPointId pointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(pointId);
			DrawConnectionPointCore(shape, pointId, p.X, p.Y, drawMode);
		}


		/// <summary>
		/// Draws the bounds of all captions of the shape
		/// </summary>
		public void DrawCaptionBounds(nShapeDrawMode drawMode, ICaptionedShape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			for (int i = shape.CaptionCount - 1; i >= 0; --i)
				DrawCaptionBounds(drawMode, shape, i);
		}


		public void DrawCaptionBounds(nShapeDrawMode drawMode, ICaptionedShape shape, int captionIndex) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			if (inplaceTextbox == null || inplaceShape != shape || inplaceCaptionIndex != captionIndex) {
				if (shape.GetCaptionTextBounds(captionIndex, out pointBuffer[0], out pointBuffer[1], out pointBuffer[2], out pointBuffer[3])) {
					DiagramToControl(pointBuffer[0], out pointBuffer[0]);
					DiagramToControl(pointBuffer[1], out pointBuffer[1]);
					DiagramToControl(pointBuffer[2], out pointBuffer[2]);
					DiagramToControl(pointBuffer[3], out pointBuffer[3]);
					Pen pen = null;
					switch (drawMode) {
						case nShapeDrawMode.Deactivated:
							pen = HandleInactivePen;
							break;
						case nShapeDrawMode.Normal:
							pen = HandleNormalPen;
							break;
						case nShapeDrawMode.Highlighted:
							pen = HandleHilightPen;
							break;
						default: throw new nShapeUnsupportedValueException(drawMode);
					}
					currentGraphics.DrawPolygon(pen, pointBuffer);
				}
			}
		}


		public void DrawShapeOutline(nShapeDrawMode drawMode, Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (!graphicsIsTransformed) throw new nShapeException("RestoreTransformation has to be called before calling this method.");
			Pen backgroundPen = null;
			Pen foregroundPen = null;
			if (shape.Parent != null) DrawParentOutline(currentGraphics, shape.Parent);
			switch (drawMode) {
				case nShapeDrawMode.Deactivated:
					backgroundPen = OutlineInactivePen;
					foregroundPen = OutlineInteriorPen;
					break;
				case nShapeDrawMode.Normal:
					backgroundPen = OutlineNormalPen;
					foregroundPen = OutlineInteriorPen;
					break;
				case nShapeDrawMode.Highlighted:
					backgroundPen = OutlineHilightPen;
					foregroundPen = OutlineInteriorPen;
					break;
				default: throw new nShapeUnsupportedValueException(typeof(nShapeDrawMode), drawMode);
			}
			// scale lineWidth 
			backgroundPen.Width = GripSize / zoomfactor;
			foregroundPen.Width = 1 / zoomfactor;

			shape.DrawOutline(currentGraphics, backgroundPen);
			shape.DrawOutline(currentGraphics, foregroundPen);
		}


		public void DrawSnapIndicators(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;
			int snapIndicatorRadius = handleRadius;

			bool graphicsWasTransformed = graphicsIsTransformed;
			if (graphicsIsTransformed) ResetTransformation();
			try {
				Rectangle shapeBounds = shape.GetBoundingRectangle(true);
				int zoomedGridSize;
				ControlToDiagram(GridSize, out zoomedGridSize);

				bool drawLeft = (shapeBounds.Left % GridSize == 0);
				bool drawTop = (shapeBounds.Top % GridSize == 0);
				bool drawRight = (shapeBounds.Right % GridSize == 0);
				bool drawBottom = (shapeBounds.Bottom % GridSize == 0);

				// transform shape bounds to control coordinates
				DiagramToControl(shapeBounds, out shapeBounds);

				// draw outlines
				if (drawLeft) currentGraphics.DrawLine(outerSnapPen, shapeBounds.Left, shapeBounds.Top - 1, shapeBounds.Left, shapeBounds.Bottom + 1);
				if (drawRight) currentGraphics.DrawLine(outerSnapPen, shapeBounds.Right, shapeBounds.Top - 1, shapeBounds.Right, shapeBounds.Bottom + 1);
				if (drawTop) currentGraphics.DrawLine(outerSnapPen, shapeBounds.Left - 1, shapeBounds.Top, shapeBounds.Right + 1, shapeBounds.Top);
				if (drawBottom) currentGraphics.DrawLine(outerSnapPen, shapeBounds.Left - 1, shapeBounds.Bottom, shapeBounds.Right + 1, shapeBounds.Bottom);
				// fill interior
				if (drawLeft) currentGraphics.DrawLine(innerSnapPen, shapeBounds.Left, shapeBounds.Top, shapeBounds.Left, shapeBounds.Bottom);
				if (drawRight) currentGraphics.DrawLine(innerSnapPen, shapeBounds.Right, shapeBounds.Top, shapeBounds.Right, shapeBounds.Bottom);
				if (drawTop) currentGraphics.DrawLine(innerSnapPen, shapeBounds.Left, shapeBounds.Top, shapeBounds.Right, shapeBounds.Top);
				if (drawBottom) currentGraphics.DrawLine(innerSnapPen, shapeBounds.Left, shapeBounds.Bottom, shapeBounds.Right, shapeBounds.Bottom);

				foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.All)) {
					Point p = Point.Empty;
					p = shape.GetControlPointPosition(id);

					// check if the point is on a gridline
					bool hGridLineContainsPoint = p.X % (GridSize * zoomfactor) == 0;
					bool vGridLineContainsPoint = p.Y % (GridSize * zoomfactor) == 0;
					// collect coordinates for bounding box
					if (p.X < left) left = p.X;
					if (p.X > right) right = p.X;
					if (p.Y < top) top = p.Y;
					if (p.Y > bottom) bottom = p.Y;

					if (hGridLineContainsPoint || vGridLineContainsPoint) {
						DiagramToControl(p, out p);
						currentGraphics.FillEllipse(HandleInteriorBrush, p.X - snapIndicatorRadius, p.Y - snapIndicatorRadius, snapIndicatorRadius * 2, snapIndicatorRadius * 2);
						currentGraphics.DrawEllipse(innerSnapPen, p.X - snapIndicatorRadius, p.Y - snapIndicatorRadius, snapIndicatorRadius * 2, snapIndicatorRadius * 2);
					}
				}
			} finally {
				if (graphicsWasTransformed) RestoreTransformation();
			}
		}


		/// <summary>
		/// Draws a selection frame.
		/// </summary>
		/// <param name="frameRect">Bounds of the selection frame in diagram coordinates.</param>
		public void DrawSelectionFrame(Rectangle frameRect) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			DiagramToControl(frameRect, out rectBuffer);
			if (HighQualityRendering) {
				currentGraphics.FillRectangle(ToolPreviewBackBrush, rectBuffer);
				currentGraphics.DrawRectangle(ToolPreviewPen, rectBuffer);
			} else {
				ControlPaint.DrawLockedFrame(currentGraphics, rectBuffer, false);
				//ControlPaint.DrawFocusRectangle(graphics, rectBuffer, Color.White, Color.Black);
			}
		}


		public void DrawAnglePreview(Point center, int radius, Point mousePos, int cursorId, int originalAngle, int newAngle) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			// Get cursor size
			Size cursorSize = registeredCursors[cursorId].Size;
			// transform diagram coordinates to control coordinates
			DiagramToControl(center, out center);
			DiagramToControl(radius, out radius);
			DiagramToControl(mousePos, out mousePos);
			// Check if the cursor has the minimum distance from the rotation point
			if (radius > minRotateDistance) {
				// Calculate angle and angle info text
				float angleDeg = Geometry.TenthsOfDegreeToDegrees(newAngle <= 1800 ? newAngle : (newAngle - 3600));
				string angleInfoText = null;
				if (SelectedShapes.Count > 1)
					angleInfoText = string.Format("{0}°", angleDeg);
				else {
					float origShapeAngle = Geometry.TenthsOfDegreeToDegrees(originalAngle - newAngle);
					float newShapeAngle = Geometry.TenthsOfDegreeToDegrees(originalAngle % 3600);
					angleInfoText = string.Format("{0}° ({1}° {2} {3}°)", newShapeAngle, origShapeAngle, angleDeg < 0 ? '-' : '+', Math.Abs(angleDeg));
				}
				// Calculate LayoutRectangle's size for the angle info text
				Rectangle layoutRect = Rectangle.Empty;
				layoutRect.Size = TextMeasurer.MeasureText(currentGraphics, angleInfoText, Font, Size.Empty, previewTextFormatter);
				layoutRect.Width = Math.Min((int)Math.Round(radius * 1.5), layoutRect.Width);
				// Calculate the circumcircle of the LayoutRectangle and the distance between mouse and rotation center...
				float circumCircleRadius = Geometry.DistancePointPoint(-cursorSize.Width / 2f, -cursorSize.Height / 2f, layoutRect.Width, layoutRect.Height) / 2f;
				float mouseDistance = Math.Max(Geometry.DistancePointPoint(center, mousePos), 0.0001f);
				float interpolationFactor = circumCircleRadius / mouseDistance;
				// ... then transform the layoutRectangle towards the mouse cursor
				PointF textCenter = Geometry.VectorLinearInterpolation((PointF)mousePos, (PointF)center, interpolationFactor);
				layoutRect.X = (int)Math.Round(textCenter.X - (layoutRect.Width / 2f));
				layoutRect.Y = (int)Math.Round(textCenter.Y - (layoutRect.Height / 2f));

				// Draw angle info pie
				int pieSize = radius + radius;
				if (HighQualityRendering) {
					currentGraphics.DrawEllipse(ToolPreviewPen, center.X - radius, center.Y - radius, pieSize, pieSize);
					currentGraphics.FillPie(ToolPreviewBackBrush, center.X - radius, center.Y - radius, pieSize, pieSize, 0, angleDeg);
					currentGraphics.DrawPie(ToolPreviewPen, center.X - radius, center.Y - radius, pieSize, pieSize, 0, angleDeg);
				} else {
					currentGraphics.DrawPie(Pens.Black, center.X - radius, center.Y - radius, pieSize, pieSize, 0, angleDeg);
					currentGraphics.DrawPie(Pens.Black, center.X - radius, center.Y - radius, pieSize, pieSize, 0, angleDeg);
				}
				currentGraphics.DrawString(angleInfoText, Font, Brushes.Black, layoutRect, previewTextFormatter);
			} else {
				// If cursor is nearer to the rotation point that the required distance,
				// Draw rotation instuction preview
				if (HighQualityRendering) {
					// draw shapeAngle preview circle
					currentGraphics.DrawEllipse(ToolPreviewPen, center.X - radius, center.Y - radius, radius + radius, radius + radius);
					currentGraphics.FillPie(ToolPreviewBackBrush, center.X - radius, center.Y - radius, radius + radius, radius + radius, 0, 45f);
					currentGraphics.DrawPie(ToolPreviewPen, center.X - radius, center.Y - radius, radius + radius, radius + radius, 0, 45f);

					// Draw rotation direction arrow line
					int bowInsetX, bowInsetY;
					bowInsetX = bowInsetY = radius / 4;
					currentGraphics.DrawArc(ToolPreviewPen, center.X - radius + bowInsetX, center.Y - radius + bowInsetY, radius + radius - bowInsetX - bowInsetX, radius + radius - bowInsetY - bowInsetY, 0, 22.5f);
					// Calculate Arrow Tip
					int arrowTipX = 0; int arrowTipY = 0;
					arrowTipX = center.X + radius - bowInsetX;
					arrowTipY = center.Y;
					Geometry.RotatePoint(center.X, center.Y, 45f, ref arrowTipX, ref arrowTipY);
					arrowShape[0].X = arrowTipX;
					arrowShape[0].Y = arrowTipY;
					//
					arrowTipX = center.X + radius - bowInsetX - GripSize - GripSize;
					arrowTipY = center.Y;
					Geometry.RotatePoint(center.X, center.Y, 22.5f, ref arrowTipX, ref arrowTipY);
					arrowShape[1].X = arrowTipX;
					arrowShape[1].Y = arrowTipY;
					//
					arrowTipX = center.X + radius - bowInsetX + GripSize + GripSize;
					arrowTipY = center.Y;
					Geometry.RotatePoint(center.X, center.Y, 22.5f, ref arrowTipX, ref arrowTipY);
					arrowShape[2].X = arrowTipX;
					arrowShape[2].Y = arrowTipY;
					// Draw arrow tip
					currentGraphics.FillPolygon(ToolPreviewBackBrush, arrowShape);
					currentGraphics.DrawPolygon(ToolPreviewPen, arrowShape);
				} else currentGraphics.DrawPie(Pens.Black, center.X - radius, center.Y - radius, radius * 2, radius * 2, 0, 45f);
			}
		}


		public void DrawLine(Point a, Point b) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			DiagramToControl(a, out a);
			DiagramToControl(b, out b);
			currentGraphics.DrawLine(outerSnapPen, a, b);
			currentGraphics.DrawLine(innerSnapPen, a, b);
		}

		#endregion


		#region [Public] Methods: (Un)Selecting shapes

		/// <summary>
		/// Clears the current selection.
		/// </summary>
		public void UnselectAll() {
			ClearSelection();
			PerformSelectionNotifications();
		}


		/// <summary>
		/// Removes the given Shape from the current selection.
		/// </summary>
		public void UnselectShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			DoUnselectShape(shape);
			PerformSelectionNotifications();
		}


		/// <summary>
		/// Selects the given shape. Current selection will be cleared.
		/// </summary>
		public void SelectShape(Shape shape) {
			SelectShape(shape, false);
		}


		/// <summary>
		/// Selects the given shape.
		/// </summary>
		/// <param name="shape">Shape to be selected.</param>
		/// <param name="addToCurrentSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		public void SelectShape(Shape shape, bool addToSelection) {
			if (shape == null) throw new ArgumentNullException("shape");
			DoSelectShape(shape, addToSelection);
			PerformSelectionNotifications();
		}


		/// <summary>
		/// Selects the given shape.
		/// </summary>
		/// <param name="shape">Shape to be selected.</param>
		/// <param name="addToCurrentSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		public void SelectShapes(IEnumerable<Shape> shapes, bool addToSelection) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (!addToSelection)
				ClearSelection();
			foreach (Shape shape in shapes)
				DoSelectShape(shape, true);
			PerformSelectionNotifications();
		}


		/// <summary>
		/// Selects all shapes within the given area.
		/// </summary>
		/// <param name="area">All shapes in the given rectangle will be selected.</param>
		/// <param name="addToCurrentSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		public void SelectShapes(Rectangle area, bool addToSelection) {
			if (Diagram != null) {
				// ensure rectangle width and height are positive
				if (area.Size.Width < 0) {
					area.Width = Math.Abs(area.Width);
					area.X = area.X - area.Width;
				}
				if (area.Size.Height < 0) {
					area.Height = Math.Abs(area.Height);
					area.Y = area.Y - area.Height;
				}
				SelectShapes(Diagram.Shapes.FindShapes(area.X, area.Y, area.Width, area.Height, true), addToSelection);
			}
		}


		/// <summary>
		/// Selects all shapes of the given shape type.
		/// </summary>
		public void SelectShapes(ShapeType shapeType, bool addToSelection) {
			if (shapeType == null) throw new ArgumentNullException("shapeType");
			if (Diagram != null) {
				// Find all shapes of the same ShapeType
				shapeBuffer.Clear();
				foreach (Shape shape in Diagram.Shapes) {
					if (shape.Type == shapeType)
						shapeBuffer.Add(shape);
				}
				SelectShapes(shapeBuffer, addToSelection);
			}
		}


		/// <summary>
		/// Selects all shapes based on the given template.
		/// </summary>
		public void SelectShapes(Template template, bool addToSelection) {
			if (template == null) throw new ArgumentNullException("template");
			if (Diagram != null) {
				// Find all shapes of the same ShapeType
				shapeBuffer.Clear();
				foreach (Shape shape in Diagram.Shapes) {
					if (shape.Template == template)
						shapeBuffer.Add(shape);
				}
				SelectShapes(shapeBuffer, addToSelection);
			}
		}


		/// <summary>
		/// Selects all shapes of the diagram.
		/// </summary>
		public void SelectAll() {
			selectedShapes.Clear();
			selectedShapes.AddRange(Diagram.Shapes);
			((IDiagramPresenter)this).SuspendUpdate();
			foreach (Shape shape in selectedShapes.BottomUp) {
				shape.Invalidate();
				InvalidateGrips(shape, ControlPointCapabilities.All);
			}
			((IDiagramPresenter)this).ResumeUpdate();
			PerformSelectionNotifications();
		}

		#endregion


		#region [Public] Methods

		public void DisplayDiagram() {
			if (diagramController.Diagram != null) {
				Diagram.DisplayService = this;
				if (Diagram is Diagram)
					Diagram.HighQualityRendering = HighQualityRendering;
			}
			UpdateScrollBars();
			Invalidate();
		}
		

		public bool LoadDiagram(string diagramName) {
			if (diagramName == null) throw new ArgumentNullException("diagramName");
			bool result = false;
			// clear ObjectRef selectedShapes and models
			if (Project.Repository == null)
				throw new nShapeException("Repository is not set to an instance of IRepository.");
			if (!Project.Repository.IsOpen)
				throw new nShapeException("Repository is not open.");

			foreach (Diagram d in Project.Repository.GetDiagrams()) {
				if (Diagram == null && diagramName.Equals(string.Empty) ||
					d.Name.Equals(diagramName, StringComparison.InvariantCultureIgnoreCase)) {
					// use Property Setter because it sets the displayService and loads all diagramShapes from Cache
					Diagram = d;
					result = true;
					break;
				}
			}

			if (Diagram != null) {
				//DiagramController.InsertShapes(project.Cache.GetDiagramShapes(this.DiagramController));
			}
			UpdateScrollBars();
			Refresh();

			return result;
		}


		public void Clear() {
			//ClearSelection();
			if (Diagram != null) Diagram = null;
			selectedShapes.Clear();
			shapeBuffer.Clear();
			editBuffer.Clear();
			Invalidate();
		}


		public IEnumerable<nShapeAction> GetActions() {
			if (Diagram != null) {
				Point mousePos = Point.Empty;
				ScreenToDiagram(Control.MousePosition, out mousePos);
				Shape shapeUnderCursor = Diagram.Shapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null);
				bool modelObjectsAssigned = ModelObjectsAssigned(selectedShapes);

				#region Context menu structure
				// Select...
				// Bring to front
				// Send to bottom
				// Group shapes
				// Ungroup shapes
				// Aggregate composite shape
				// Split composite shape
				// --------------------------
				// Cut
				// Copy
				// Paste
				// Delete
				// --------------------------
				// Diagram Properties
				// --------------------------
				// Undo
				// Redo
				//
				#endregion

				// Create a action group
				yield return new nShapeActionGroup("Select...", null, "Select or unselect Shapes", true,
					new nShapeAction[] {
						CreateSelectAllAction(),
						CreateSelectByTemplateAction(shapeUnderCursor),
						CreateSelectByTypeAction(shapeUnderCursor),
						CreateUnselectAllAction()
					}, -1);
				yield return CreateBringToFrontAction(Diagram, selectedShapes);
				yield return CreateSendToBackAction(Diagram, selectedShapes);
				yield return CreateGroupShapesAction(Diagram, selectedShapes, activeLayers);
				yield return CreateUngroupAction(Diagram, selectedShapes);
				yield return CreateAggregateAction(Diagram, selectedShapes, activeLayers);
				yield return CreateUnaggregateAction(Diagram, selectedShapes);
				yield return new SeparatorAction();
				yield return CreateCutAction(Diagram, selectedShapes, modelObjectsAssigned, mousePos);
				yield return CreateCopyAction(Diagram, selectedShapes, modelObjectsAssigned, mousePos);
				yield return CreatePasteAction(Diagram, selectedShapes, activeLayers, mousePos);
				yield return CreateDeleteAction(Diagram, selectedShapes, modelObjectsAssigned);
				yield return new SeparatorAction();
				yield return CreatePropertiesAction(Diagram, selectedShapes, mousePos);
				yield return new SeparatorAction();
				yield return CreateUndoAction();
				yield return CreateRedoAction();
			}
		}


		public void Cut(bool withModelObjects) {
			if (Diagram != null && selectedShapes.Count > 0)
				Cut(withModelObjects, Geometry.InvalidPoint);
		}


		public void Cut(bool withModelObjects, Point currentMousePos) {
			if (Diagram != null && selectedShapes.Count > 0)
				PerformCut(Diagram, selectedShapes, withModelObjects, currentMousePos);
		}


		public void Copy(bool withModelObjects) {
			Copy(withModelObjects, Geometry.InvalidPoint);
		}


		public void Copy(bool withModelObjects, Point currentMousePos) {
			if (Diagram != null && SelectedShapes.Count > 0)
				PerformCopy(Diagram, SelectedShapes, withModelObjects, currentMousePos);
		}


		public void Paste(int offsetX, int offsetY) {
			if (Diagram != null && diagramSetController.CanPaste(Diagram))
				PerformPaste(Diagram, activeLayers, Geometry.InvalidPoint);
		}


		public void Paste(Point currentMousePosition) {
			if (Diagram != null && diagramSetController.CanPaste(Diagram))
				PerformPaste(Diagram, activeLayers, currentMousePosition);
		}


		public void Delete(bool withModelObjects) {
			if (Diagram != null && SelectedShapes.Count > 0) {
				PerformDelete(Diagram, selectedShapes, withModelObjects);
			}
		}


		/// <summary>
		/// Sets a previously registered cursor.
		/// </summary>
		/// <param name="cursor"></param>
		public void SetCursor(int cursorId) {
			// If cursor was not loaded yet, load it now
			if (!registeredCursors.ContainsKey(cursorId))
				LoadRegisteredCursor(cursorId);
			Cursor = registeredCursors[cursorId] ?? Cursors.Default;
		}


		public void OpenCaptionEditor(ICaptionedShape shape, int x, int y) {
			if (shape == null) throw new ArgumentNullException("shape");
			inplaceShape = shape;
			inplaceCaptionIndex = shape.FindCaptionFromPoint(x, y);
			if(inplaceCaptionIndex >= 0)
				OpenCaptionEditor(shape, inplaceCaptionIndex, string.Empty);
		}


		public void OpenCaptionEditor(ICaptionedShape shape, int labelIndex) {
			OpenCaptionEditor(shape, labelIndex, string.Empty);
		}


		public void OpenCaptionEditor(ICaptionedShape shape, int labelIndex, string newText) {
			if (shape == null) throw new ArgumentNullException("shape");
			inplaceShape = shape;
			inplaceCaptionIndex = labelIndex;
			Debug.Assert(inplaceCaptionIndex >= 0);

			// store caption's current text
			string currentText = shape.GetCaptionText(inplaceCaptionIndex);
			// delete caption's text temporarily, otherwise the text would be drawn twice
			shape.SetCaptionText(inplaceCaptionIndex, string.Empty);

			// Create and show inplace text editor
			inplaceTextbox = new InPlaceTextBox(this, inplaceShape, inplaceCaptionIndex, currentText, newText);
			inplaceTextbox.KeyDown += inPlaceTextBox_KeyDown;
			inplaceTextbox.Leave += inPlaceTextBox_Leave;
			inplaceTextbox.ShortcutsEnabled = true;
			this.Controls.Add(inplaceTextbox);
			inplaceTextbox.Focus();
			inplaceTextbox.Invalidate();
		}


		public void EnsureVisible(int x, int y) {
			int ctrlX, ctrlY;
			DiagramToControl(x, y, out ctrlX, out ctrlY);
			if (AutoScrollAreaContainsPoint(ctrlX, ctrlY)) {
				Rectangle autoScrollArea = DrawBounds;
				autoScrollArea.Inflate(-autoScrollMargin, -autoScrollMargin);
				ControlToDiagram(autoScrollArea, out autoScrollArea);
				
				// scroll horizontally
				int deltaX = 0, deltaY = 0;
				if (scrollBarH.Visible) {
					if (x < autoScrollArea.Left)
						// Scroll left
						deltaX = x - autoScrollArea.Left;
					else if (autoScrollArea.Right < x)
						// Scroll right
						deltaX = x - autoScrollArea.Right;
				}
				if (scrollBarV.Visible) {
					if (y < autoScrollArea.Top)
						// Scroll left
						deltaY = y - autoScrollArea.Top;
					else if (autoScrollArea.Bottom < y)
						// Scroll right
						deltaY = y - autoScrollArea.Bottom;
				}
				ScrollTo(scrollPosX + deltaX, scrollPosY + deltaY);
			}
		}


		public void EnsureVisible(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			EnsureVisible(shape.GetBoundingRectangle(false));
		}


		public void EnsureVisible(Rectangle rectangle) {
			Rectangle ctrlRect;
			DiagramToControl(rectangle, out ctrlRect);
			
			// Check if the diagram has to be zoomed
			if (AutoScrollAreaContainsPoint(ctrlRect.Left, ctrlRect.Top)
				|| AutoScrollAreaContainsPoint(ctrlRect.Right, ctrlRect.Top)
				|| AutoScrollAreaContainsPoint(ctrlRect.Left, ctrlRect.Bottom)
				|| AutoScrollAreaContainsPoint(ctrlRect.Right, ctrlRect.Bottom)) {

				Rectangle autoScrollArea = DrawBounds;
				autoScrollArea.Inflate(-autoScrollMargin, -autoScrollMargin);
				ControlToDiagram(autoScrollArea, out autoScrollArea);

				// Check if Zoom has to be adjusted
				if (autoScrollArea.Width < rectangle.Width ||
					autoScrollArea.Height < rectangle.Height) {
					float scale = Geometry.CalcScaleFactor(rectangle.Width, rectangle.Height, autoScrollArea.Width, autoScrollArea.Height);
					ZoomLevel = Math.Max(1, (int)Math.Floor(scale * 10) * 10);
				}
			}

			DiagramToControl(rectangle, out ctrlRect);
			ctrlRect.Inflate(autoScrollMargin, autoScrollMargin);
			if (AutoScrollAreaContainsPoint(ctrlRect.Left, ctrlRect.Top)
				|| AutoScrollAreaContainsPoint(ctrlRect.Right, ctrlRect.Top)
				|| AutoScrollAreaContainsPoint(ctrlRect.Left, ctrlRect.Bottom)
				|| AutoScrollAreaContainsPoint(ctrlRect.Right, ctrlRect.Bottom)) {
				
				// Recalculate autoScrollArea in case a new zoom level was applied
				Rectangle autoScrollArea = DrawBounds;
				autoScrollArea.Inflate(-autoScrollMargin, -autoScrollMargin);
				ControlToDiagram(autoScrollArea, out autoScrollArea);

				// scroll horizontally
				int deltaX = 0, deltaY = 0;
				if (scrollBarH.Visible) {
					if (rectangle.Left < autoScrollArea.Left)
						// Scroll left
						deltaX = rectangle.Left - autoScrollArea.Left;
					else if (autoScrollArea.Right < rectangle.Right)
						// Scroll right
						deltaX = rectangle.Right - autoScrollArea.Right;
				}
				if (scrollBarV.Visible) {
					if (rectangle.Top < autoScrollArea.Top)
						// Scroll left
						deltaY = rectangle.Top - autoScrollArea.Top;
					else if (autoScrollArea.Bottom < rectangle.Top)
						// Scroll right
						deltaY = rectangle.Top - autoScrollArea.Bottom;
				}
				ScrollTo(scrollPosX + deltaX, scrollPosY + deltaY);
			}
		}


		public void SetLayerVisibility(LayerIds layerIds, bool visible) {
			// Hide or show layers
			if (visible) hiddenLayers ^= (hiddenLayers & layerIds);
			else hiddenLayers |= layerIds;
			// Update presenter
			Invalidate();
			// Perform notification
			if (LayerVisibilityChanged != null) LayerVisibilityChanged(this, LayerHelper.GetLayersEventArgs(LayerHelper.GetLayers(layerIds, Diagram)));
		}


		public void SetLayerActive(LayerIds layerIds, bool active) {
			// Activate or deactivate layers
			if (active) activeLayers |= layerIds;
			else activeLayers ^= (activeLayers & layerIds);
			// Update presenter
			Invalidate();
			// Perform notification
			if (ActiveLayersChanged != null) ActiveLayersChanged(this, LayerHelper.GetLayersEventArgs(LayerHelper.GetLayers(layerIds, Diagram)));
		}


		public bool IsLayerVisible(LayerIds layerId) {
			return !((hiddenLayers & layerId) != 0);
		}


		public bool IsLayerActive(LayerIds layerId) {
			return (activeLayers & layerId) != 0;
		}

		#endregion


		[Browsable(false)]
		protected DiagramController Controller {
			get { return diagramController; }
			set {
				Debug.Assert(diagramSetController != null);
				if (diagramController != null) {
					UnregisterDiagramControllerEvents();
					if (diagramController.Diagram != null) {
						diagramSetController.CloseDiagram(diagramController.Diagram);
						Clear();
					}
				}
				diagramController = value;
				if (diagramController != null) {
					RegisterDiagramControllerEvents();
					if (diagramController.Diagram != null) DisplayDiagram();
				}
			}
		}


		#region [Protected] Methods: On[Event] event processing

		protected virtual void OnShapeClick(DiagramPresenterShapeClickEventArgs eventArgs) {
			if (ShapeClick != null) ShapeClick(this, eventArgs);
		}


		protected virtual void OnShapeDoubleClick(DiagramPresenterShapeClickEventArgs eventArgs) {
			if (ShapeDoubleClick != null) ShapeDoubleClick(this, eventArgs);
		}


		protected virtual void OnShapeInsert(DiagramPresenterShapeEventArgs eventArgs) {
			if (ShapeInsert != null) ShapeInsert(this, eventArgs);
		}


		protected virtual void OnShapeRemove(DiagramPresenterShapeEventArgs eventArgs) {
			if (ShapeRemove != null) ShapeRemove(this, eventArgs);
		}


		protected override void OnMouseWheel(MouseEventArgs e) {
			base.OnMouseWheel(e);
			// ToDo: Redirect MouseWheel movement to the current tool?
			//if (CurrentTool != null)
			//   CurrentTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseDown, e));
			
			if (Diagram != null && ZoomWithMouseWheel) {
				if (e.Delta < 0 && ZoomLevel - zoomStepping > 0)
					ZoomLevel = (ZoomLevel - zoomStepping) - (ZoomLevel % zoomStepping);
				else if (e.Delta > 0)
					ZoomLevel = (ZoomLevel + zoomStepping) - (ZoomLevel % zoomStepping);
			}
		}
		
		
		protected override void OnMouseClick(MouseEventArgs e) {
			Debug.Print("OnMouseClick");
			base.OnMouseClick(e);

			// Raise ShapeClick-Events if a shape has been clicked
			// ToDo: Is this correct?
			if (Diagram != null && ShapeClick != null && !ScrollBarContainsPoint(e.Location)) {
				int mouseX, mouseY;
				ControlToDiagram(e.X, e.Y, out mouseX, out mouseY);
				foreach (Shape clickedShape in Diagram.Shapes.FindShapes(mouseX, mouseY, ControlPointCapabilities.All, GripSize))
					OnShapeClick(new DiagramPresenterShapeClickEventArgs(clickedShape, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseUp, e)));
			}
		}


		protected override void OnMouseDown(MouseEventArgs e) {
			Debug.Print("OnMouseDown");
			base.OnMouseDown(e);
			if (inplaceTextbox != null) CloseCaptionEditor(true);
			if (CurrentTool != null && !ScrollBarContainsPoint(e.Location))
				CurrentTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseDown, e));
		}


		protected override void OnMouseDoubleClick(MouseEventArgs e) {
			Debug.Print("OnMouseDoubleClick");
			base.OnMouseDoubleClick(e);
			// Raise ShapeClick-Events if a shape has been clicked
			// ToDo: Is this correct?
			if (Diagram != null && ShapeClick != null && !ScrollBarContainsPoint(e.Location)) {
				int mouseX, mouseY;
				ControlToDiagram(e.X, e.Y, out mouseX, out mouseY);
				foreach (Shape clickedShape in Diagram.Shapes.FindShapes(mouseX, mouseY, ControlPointCapabilities.All, GripSize))
					OnShapeDoubleClick(new DiagramPresenterShapeClickEventArgs(clickedShape, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseUp, e)));
			}
		}


		protected override void OnMouseEnter(EventArgs e) {
			base.OnMouseEnter(e);
			if (CurrentTool != null) CurrentTool.EnterDisplay(this);
		}


		protected override void OnMouseLeave(EventArgs e) {
			base.OnMouseLeave(e);
			if (CurrentTool != null) CurrentTool.LeaveDisplay(this);
		}


		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (universalScrollEnabled)
				PerformUniversalScroll(e.Location);
			else {
				bool eventHandled = false;
				if (CurrentTool != null && !ScrollBarContainsPoint(e.Location)) {
					eventHandled = CurrentTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseMove, e));
					if (CurrentTool.WantsAutoScroll && AutoScrollAreaContainsPoint(e.X, e.Y)) {
						int x, y;
						ControlToDiagram(e.X, e.Y, out x, out y);
						EnsureVisible(x, y);
						if (!autoScrollTimer.Enabled) autoScrollTimer.Enabled = true;
					} else if (autoScrollTimer.Enabled) 
						autoScrollTimer.Enabled = false;
				}

				if (!eventHandled) {
					// ToDo: Call OnShapeMouseOver (does not exist yet) method here?
				}
			}
			lastMousePos = e.Location;
		}


		protected override void OnMouseUp(MouseEventArgs e) {
			Debug.Print("OnMouseUp");
			base.OnMouseUp(e);

			bool eventHandled = false;
			if (CurrentTool != null && !ScrollBarContainsPoint(e.Location))
				eventHandled = CurrentTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseUp, e));

			if (!eventHandled) {
				if (e.Button == MouseButtons.Middle) {
					if (universalScrollEnabled) EndUniversalScroll();
					else StartUniversalScroll(e.Location);
				} else if (e.Button == MouseButtons.Right) {
					if (Diagram != null) {
						Point mousePos = Point.Empty;
						ControlToDiagram(e.Location, out mousePos);
						// if there is no selected shape under the cursor
						if (SelectedShapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null) == null) {
							// Check if there is a non-selected shape under the cursor 
							// and select it in this case
							Shape shape = Diagram.Shapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null);
							if (shape != null) SelectShape(shape);
							else UnselectAll();
						}
					}
					// Display context menu
					if (ContextMenuStrip != null) ContextMenuStrip.Show(PointToScreen(e.Location));
				}
			}
		}


		protected override void OnMouseCaptureChanged(EventArgs e) {
			base.OnMouseCaptureChanged(e);
		}


		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
			base.OnPreviewKeyDown(e);
			if (CurrentTool != null)
				CurrentTool.ProcessKeyEvent(WinFormHelpers.GetKeyEventArgs(e));
		}


		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			
			bool handled = false;
			if (CurrentTool != null)
				handled = CurrentTool.ProcessKeyEvent(WinFormHelpers.GetKeyEventArgs(KeyEventType.KeyDown, e));

			if (!handled) {
				switch (e.KeyCode) {
					case Keys.F2:
						if (Diagram != null) {
							if (SelectedShapes.Count == 1 && SelectedShapes.TopMost is ICaptionedShape) {
								ICaptionedShape captionedShape = (ICaptionedShape)SelectedShapes.TopMost;
								OpenCaptionEditor(captionedShape, 0);
							}
						}
						break;

					case Keys.Delete:
						if (Diagram != null) {
							if (Controller.Owner.CanDeleteShapes(Diagram, SelectedShapes))
								Controller.Owner.DeleteShapes(Diagram, SelectedShapes, true);
						}
						break;

					// Copy
					case Keys.C:
						if ((e.Modifiers & Keys.Control) == Keys.Control) {
							if (Diagram != null && SelectedShapes.Count > 0) {
								if (Controller.Owner.CanCopy(SelectedShapes))
									Controller.Owner.Copy(Diagram, SelectedShapes, true);
							}
						}
						break;

					// Paste
					case Keys.V:
						if ((e.Modifiers & Keys.Control) == Keys.Control) {
							if (Controller.Owner.CanPaste(Diagram))
								Controller.Owner.Paste(Diagram, ActiveLayers);
						}
						break;

					// Cut
					case Keys.X:
						if ((e.Modifiers & Keys.Control) == Keys.Control) {
							if (Diagram != null && SelectedShapes.Count > 0) {
								if (Controller.Owner.CanCut(Diagram, SelectedShapes))
									Controller.Owner.Cut(Diagram, SelectedShapes, true);
							}
						}
						break;

					// Undo/Redo
					case Keys.Z:
						if ((e.Modifiers & Keys.Control) != 0) {
							if (Diagram != null) {
								if ((e.Modifiers & Keys.Shift) != 0) {
									if (Controller.Owner.Project.History.RedoCommandCount > 0)
										Controller.Owner.Project.History.Redo();
								} else {
									if (Controller.Owner.Project.History.UndoCommandCount > 0)
										Controller.Owner.Project.History.Undo();
								}
							}
						}
						break;

					default:
						// do nothing
						break;
				}
			}
		}


		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			if (CurrentTool != null)
				CurrentTool.ProcessKeyEvent(WinFormHelpers.GetKeyEventArgs(KeyEventType.KeyUp, e));
		}


		protected override void OnKeyPress(KeyPressEventArgs e) {
			base.OnKeyPress(e);
			bool isHandled = false;
			nShapeKeyEventArgs eventArgs = WinFormHelpers.GetKeyEventArgs(e);
			if (CurrentTool != null)
				isHandled = CurrentTool.ProcessKeyEvent(eventArgs);

			// Show caption editor
			if (!isHandled && selectedShapes.Count == 1) {
				if (!char.IsControl(eventArgs.KeyChar)) {
					string pressedKey = eventArgs.KeyChar.ToString();
					if (selectedShapes.TopMost is ICaptionedShape) {
						ICaptionedShape labeledShape = (ICaptionedShape)selectedShapes.TopMost;
						if (labeledShape.CaptionCount > 0) OpenCaptionEditor(labeledShape, 0, pressedKey);
					}
				}
			}
		}


		protected override void OnDragEnter(DragEventArgs drgevent) {
			base.OnDragEnter(drgevent);
		}


		protected override void OnDragOver(DragEventArgs drgevent) {
			base.OnDragOver(drgevent);
			if (drgevent.Data.GetDataPresent(typeof(ModelObjectDragInfo)) && Diagram != null) {
				Point mousePosCtrl = PointToClient(MousePosition);
				Point mousePosDiagram;
				ControlToDiagram(mousePosCtrl, out mousePosDiagram);

				Shape shape = Diagram.Shapes.FindShape(mousePosDiagram.X, mousePosDiagram.Y, ControlPointCapabilities.None, 0, null);
				if (shape != null && shape.ModelObject == null)
					drgevent.Effect = DragDropEffects.Move;
				else drgevent.Effect = DragDropEffects.None;
			} else drgevent.Effect = DragDropEffects.None;
		}


		protected override void OnDragDrop(DragEventArgs drgevent) {
			base.OnDragDrop(drgevent);
			if (drgevent.Data.GetDataPresent(typeof(ModelObjectDragInfo)) && Diagram != null) {
				Point mousePosDiagram;
				ControlToDiagram(PointToClient(MousePosition), out mousePosDiagram);

				Shape shape = Diagram.Shapes.FindShape(mousePosDiagram.X, mousePosDiagram.Y, ControlPointCapabilities.None, 0, null);
				if (shape != null && shape.ModelObject == null) {
					ModelObjectDragInfo dragInfo = (ModelObjectDragInfo)drgevent.Data.GetData(typeof(ModelObjectDragInfo));
					ICommand cmd = new AssignModelObjectCommand(shape, dragInfo.ModelObject);
					Project.ExecuteCommand(cmd);
				}
			}
		}


		protected override void OnDragLeave(EventArgs e) {
			base.OnDragLeave(e);
		}
		
		
		protected override void OnContextMenuStripChanged(EventArgs e) {
			if (ContextMenuStrip != null && ContextMenuStrip != displayContextMenuStrip) {
				if (displayContextMenuStrip != null) {
					displayContextMenuStrip.Opening -= displayContextMenuStrip_Opening;
					displayContextMenuStrip.Closed -= displayContextMenuStrip_Closed;
				}
				displayContextMenuStrip = ContextMenuStrip;
				if (displayContextMenuStrip != null) {
					displayContextMenuStrip.Opening += displayContextMenuStrip_Opening;
					displayContextMenuStrip.Closed += displayContextMenuStrip_Closed;
				}
			}
			base.OnContextMenuStripChanged(e);
		}


		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
		}


		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			drawBounds = Geometry.InvalidRectangle;
			UpdateScrollBars();
			Invalidate();
		}


		protected override void OnLayout(LayoutEventArgs e) {
			base.OnLayout(e);
		}


		protected override void OnInvalidated(InvalidateEventArgs e) {
			base.OnInvalidated(e);
		}


		protected override void OnPaintBackground(PaintEventArgs e) {
			// override this method and do nothing in  order to make DoubleBuffering work
		}


		protected override void OnPaint(PaintEventArgs e) {
#if DEBUG
			stopWatch.Reset();
			stopWatch.Start();
#endif
			//UpdateScrollBars();
			CalcTransformation();

			currentGraphics = e.Graphics;
			GdiHelpers.ApplyGraphicsSettings(currentGraphics, currentRenderingQuality);
			//currentGraphics.Clear(Color.Transparent);
			if (BackColor.A < 255) {
				GraphicsState gfxState = currentGraphics.Save();
				OnPaintBackground(e);
				currentGraphics.Restore(gfxState);
			}

			// =====   DRAW CONTROL BACKGROUND   =====
			DrawControl(currentGraphics, e.ClipRectangle);

			// =====   DRAW DIAGRAM   =====
			if (Diagram != null)
				DrawDiagram(currentGraphics, e.ClipRectangle);

			// =====   DRAW UNIVERSAL SCROLL INDICATOR   =====
			currentGraphics.ResetTransform();
			if (universalScrollEnabled) {
				ResetTransformation();
				universalScrollCursor.Draw(currentGraphics, universalScrollFixPointBounds);
			}

			// =====   DRAW DEBUG INFO   =====
#if DEBUG
			stopWatch.Stop();
			//currentGraphics.FillRectangle(new SolidBrush(Color.FromArgb(96, Color.White)), e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height);
			//currentGraphics.DrawRectangle(Pens.Red, e.ClipRectangle.X+1, e.ClipRectangle.Y+1, e.ClipRectangle.Width-2, e.ClipRectangle.Height-2);
			currentGraphics.DrawString(string.Format("{0} ms", stopWatch.ElapsedMilliseconds), Font, Brushes.Red, Point.Empty);
			//currentGraphics.DrawString(string.Format("\n\rPaint Control: {0} ms", tC.TotalMilliseconds), Font, Brushes.Red, e.ClipRectangle.Location);

			//// paint invalidated area
			//if (e.ClipRectangle != DisplayAreaBounds) {
			//   Rectangle clipRectBuffer = Rectangle.Empty;
			//   //ControlToDiagram(e.ClipRectangle, ref clipRectBuffer);
			//   if (clipRectBrush == clipRectBrush1)
			//      clipRectBrush = clipRectBrush2;
			//   else
			//      clipRectBrush = clipRectBrush1;
			//   //currentGraphics.FillRectangle(clipRectBrush, clipRectBuffer);
			//   currentGraphics.FillRectangle(clipRectBrush, e.ClipRectangle);
			//   //currentGraphics.DrawRectangle(Pens.Red, e.ClipRectangle.X + 1, e.ClipRectangle.Y + 1, e.ClipRectangle.Width - 2, e.ClipRectangle.Height - 2);
			//}

#endif
			
			#region =====   RENDER BACKBUFFER   =====
			//// render backbuffer to Display and swap backbuffers
			//currentBackBuffer.Render(e.Graphics);
			//if (currentBackBuffer == backBufferGfx1) {
			//   currentBackBuffer = backBufferGfx2;
			//   CreateBackBuffer(ref backBufferContext1, ref backBufferGfx1);
			//}
			//else {
			//   currentBackBuffer = backBufferGfx1;
			//   CreateBackBuffer(ref backBufferContext2, ref backBufferGfx2);
			//}
			#endregion

			currentGraphics = null;
		}


		protected override void NotifyInvalidate(Rectangle invalidatedArea) {
			base.NotifyInvalidate(invalidatedArea);
		}

		#endregion


		#region [Protected] Methods 

		protected internal void CloseCaptionEditor(bool confirm) {
			// End editing
			if (confirm) {
				if (inplaceTextbox.Text != inplaceTextbox.OriginalText) {
					ICommand cmd = new SetCaptionTextCommand(inplaceShape, inplaceCaptionIndex, inplaceTextbox.Text);
					Project.ExecuteCommand(cmd);
				} else inplaceShape.SetCaptionText(inplaceCaptionIndex, inplaceTextbox.Text);
			} else inplaceShape.SetCaptionText(inplaceCaptionIndex, inplaceTextbox.OriginalText);
			// Clean up
			inplaceTextbox.KeyDown -= inPlaceTextBox_KeyDown;
			inplaceTextbox.Leave -= inPlaceTextBox_Leave;
			inplaceTextbox.Dispose();
			inplaceTextbox = null;
			inplaceShape = null;
			inplaceCaptionIndex = -1;
		}


		protected virtual void DrawResizeGripCore(Shape shape, int x, int y, nShapeDrawMode drawMode) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			if (HighQualityRendering) {
				Pen handlePen = null;
				Brush handleBrush = null;
				switch (drawMode) {
					case nShapeDrawMode.Normal:
						handlePen = HandleNormalPen;
						handleBrush = HandleInteriorBrush;
						break;
					case nShapeDrawMode.Deactivated:
						handlePen = HandleInactivePen;
						handleBrush = Brushes.Transparent;
						break;
					case nShapeDrawMode.Highlighted:
						handlePen = HandleHilightPen;
						handleBrush = HandleInteriorBrush;
						break;
					default: throw new nShapeUnsupportedValueException(typeof(nShapeDrawMode), drawMode);
				}
				DrawControlPointPath(resizePointPath, x, y, handlePen, handleBrush);
			} else {
				rectBuffer.X = x - handleRadius;
				rectBuffer.Y = y - handleRadius;
				rectBuffer.Width = rectBuffer.Height = handleRadius + handleRadius;
				ControlPaint.DrawContainerGrabHandle(currentGraphics, rectBuffer);
			}
		}


		protected virtual void DrawRotateGripCore(Shape shape, int x, int y, nShapeDrawMode drawMode) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			if (HighQualityRendering) {
				Pen handlePen = null;
				Brush handleBrush = null;
				switch (drawMode) {
					case nShapeDrawMode.Normal:
						handlePen = HandleNormalPen;
						handleBrush = HandleInteriorBrush;
						break;
					case nShapeDrawMode.Deactivated:
						handlePen = HandleInactivePen;
						handleBrush = Brushes.Transparent;
						break;
					case nShapeDrawMode.Highlighted:
						handlePen = HandleHilightPen;
						handleBrush = HandleInteriorBrush;
						break;
					default: throw new nShapeUnsupportedValueException(typeof(nShapeDrawMode), drawMode);
				}
				CalcControlPointShape(rotatePointPath, ControlPointShape.RotateArrow, handleRadius);
				DrawControlPointPath(rotatePointPath, x, y, handlePen, handleBrush);
			} else {
				Rectangle hdlRect = Rectangle.Empty;
				hdlRect.X = x - handleRadius;
				hdlRect.Y = y - handleRadius;
				hdlRect.Width = hdlRect.Height = handleRadius + handleRadius;
				ControlPaint.DrawGrabHandle(currentGraphics, hdlRect, false, (drawMode == nShapeDrawMode.Deactivated));
			}
		}


		protected virtual void DrawConnectionPointCore(Shape shape, ControlPointId pointId, int x, int y, nShapeDrawMode drawMode) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			if (graphicsIsTransformed) throw new nShapeException("ResetTransformation has to be called before caling this method.");
			if (HighQualityRendering) {
				int hdlRad;
				Pen handlePen = null;
				Brush handleBrush = null;
				switch (drawMode) {
					case nShapeDrawMode.Normal:
						bool isConnected = shape.IsConnected(pointId, null) != ControlPointId.None;
						if (shape.HasControlPointCapability(pointId, ControlPointCapabilities.Resize)) {
							DrawResizeGripCore(shape, x, y, nShapeDrawMode.Normal);
							handlePen = isConnected ? HandleHilightPen : HandleInactivePen;
						} else handlePen = isConnected ? HandleHilightPen : HandleInactivePen;
						handleBrush = HandleInteriorBrush;
						hdlRad = handleRadius;
						break;
					case nShapeDrawMode.Deactivated:
						handlePen = HandleInactivePen;
						handleBrush = Brushes.Transparent;
						hdlRad = handleRadius;
						break;
					case nShapeDrawMode.Highlighted:
						handlePen = HandleHilightPen;
						handleBrush = HandleInteriorBrush;
						hdlRad = handleRadius + 1;
						break;
					default: throw new nShapeUnsupportedValueException(typeof(nShapeDrawMode), drawMode);
				}
				DrawControlPointPath(connectionPointPath, x, y, handlePen, handleBrush);
			} else {
				DiagramToControl(x, y, out x, out y);
				ControlPaint.DrawGrabHandle(currentGraphics, Rectangle.FromLTRB(x - handleRadius, y - handleRadius, x + (2 * handleRadius), y + (2 * handleRadius)), true, true);
			}
		}

		#endregion


		private bool MultiSelect {
			get {
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) return true;
				else if ((Control.ModifierKeys & Keys.ShiftKey) == Keys.ShiftKey) return true;
				else if ((Control.ModifierKeys & Keys.Control) == Keys.Control) return true;
				else if ((Control.ModifierKeys & Keys.ControlKey) == Keys.ControlKey) return true;
				else return false;
			}
		}

		
		#region [Private] Properties: Pens, Brushes, Bounds, etc

		private Rectangle DiagramBounds {
			get {
				if (Diagram == null) return Rectangle.Empty;
				else return Geometry.UniteRectangles(0, 0, Diagram.Width, Diagram.Height, 
					Diagram.Shapes.GetBoundingRectangle(false));
			}
		}


		/// <summary>
		/// Provides the current graphics object for tools
		/// </summary>
		//private Graphics Graphics {
		//   get {
		//      if (currentGraphics == null) {
		//         Debug.Fail("Graphics property may only be used inside the OnPaint method.");
		//         currentGraphics = Graphics.FromHwnd(this.Handle);
		//         GdiHelpers.ApplyGraphicsSettings(currentGraphics, currentRenderingQuality);
		//      }
		//      return currentGraphics;

		//      //return currentBackBuffer.Graphics;
		//   }
		//}


		private Pen GridPen {
			get {
				if (gridPen == null) CreatePen(gridColor, gridAlpha, ref gridPen);
				return gridPen;
			}
		}


		private Pen OutlineInteriorPen {
			get {
				if (outlineInteriorPen == null) CreatePen(selectionInteriorColor, ref outlineInteriorPen);
				return outlineInteriorPen;
			}
		}


		private Pen OutlineNormalPen {
			get {
				if (outlineNormalPen == null) CreatePen(selectionNormalColor, selectionAlpha, handleRadius, ref outlineNormalPen);
				return outlineNormalPen;
			}
		}


		private Pen OutlineHilightPen {
			get {
				if (outlineHilightPen == null) CreatePen(selectionHilightColor, selectionAlpha, handleRadius, ref outlineHilightPen);
				return outlineHilightPen;
			}
		}


		private Pen OutlineInactivePen {
			get {
				if (outlineInactivePen == null) CreatePen(selectionInactiveColor, selectionAlpha, handleRadius, ref outlineInactivePen);
				return outlineInactivePen;
			}
		}


		private Pen HandleNormalPen {
			get {
				if (handleNormalPen == null) CreatePen(selectionNormalColor, ref handleNormalPen);
				return handleNormalPen;
			}
		}


		private Pen HandleHilightPen {
			get {
				if (handleHilightPen == null) CreatePen(selectionHilightColor, ref handleHilightPen);
				return handleHilightPen;
			}
		}


		private Pen HandleInactivePen {
			get {
				if (handleInactivePen == null) CreatePen(selectionInactiveColor, ref handleInactivePen);
				return handleInactivePen;
			}
		}


		private Brush HandleInteriorBrush {
			get {
				if (handleInteriorBrush == null) CreateBrush(selectionInteriorColor, selectionAlpha, ref handleInteriorBrush);
				return handleInteriorBrush;
			}
		}


		private Brush InplaceTextboxBackBrush {
			get {
				if (inplaceTextboxBackBrush == null)
					CreateBrush(SelectionInteriorColor, inlaceTextBoxBackAlpha, ref  inplaceTextboxBackBrush);
				return inplaceTextboxBackBrush;
			}
		}


		private Pen ToolPreviewPen {
			get {
				if (toolPreviewPen == null) CreatePen(toolPreviewColor, toolPreviewColorAlpha, ref toolPreviewPen);
				return toolPreviewPen;
			}
		}


		private Brush ToolPreviewBackBrush {
			get {
				if (toolPreviewBackBrush == null) CreateBrush(toolPreviewBackColor, toolPreviewBackColorAlpha, ref toolPreviewBackBrush);
				return toolPreviewBackBrush;
			}
		}

		#endregion


		#region [Private] Methods: Drawing and painting implementation

		// Inactive methods: 
		// Fixed frame rate rendering implementation
		private void displayRefreshTimer_Tick(object sender, EventArgs e) {
			if (performInvalidation) {
				Invalidate(invalidationBuffer, true);
				invalidationBuffer = Rectangle.Empty;
				performInvalidation = false;
			}
		}


		// Inactive methods: 
		// Manual DoubleBuffering implementation
		private void CreateBackBuffer(ref BufferedGraphicsContext backBufferContext, ref BufferedGraphics backBufferGfx) {
			rectBuffer.X = 0;
			rectBuffer.Y = 0;
			rectBuffer.Width = Bounds.Width;
			rectBuffer.Height = Bounds.Height;

			if (backBufferGfx != null) backBufferGfx.Dispose();
			backBufferContext.Dispose();
			backBufferGfx = backBufferContext.Allocate(this.CreateGraphics(), rectBuffer);
			GdiHelpers.ApplyGraphicsSettings(backBufferGfx.Graphics, currentRenderingQuality);
		}


		private void CalcTransformation() {
			int zoomedDiagramWidth = (int)Math.Round(DiagramBounds.Width * zoomfactor) + (2 * diagramMargin);
			int zoomedDiagramHeight = (int)Math.Round(DiagramBounds.Height * zoomfactor) + (2 * diagramMargin);

			diagramPosX = diagramMargin - (int)Math.Round(DiagramBounds.X * zoomfactor) + ((DrawBounds.X + DrawBounds.Width) / 2) - (zoomedDiagramWidth / 2);
			diagramPosY = diagramMargin - (int)Math.Round(DiagramBounds.Y * zoomfactor) + ((DrawBounds.Y + DrawBounds.Height) / 2) - (zoomedDiagramHeight / 2);
		}
		
		
		private void RestoreTransformation(Graphics graphics, int diagramOffsetX, int diagramOffsetY, int scrollOffsetX, int scrollOffsetY, float zoomFactor) {
			// transform graphics object
			graphics.TranslateTransform(diagramOffsetX / zoomFactor, diagramOffsetY / zoomFactor, MatrixOrder.Append);
			graphics.TranslateTransform(-scrollOffsetX, -scrollOffsetY, MatrixOrder.Append);
			graphics.ScaleTransform(zoomFactor, zoomFactor, MatrixOrder.Append);
			graphicsIsTransformed = true;
		}


		private void ResetTransformation(Graphics graphics) {
			graphics.ResetTransform();
			graphicsIsTransformed = false;
		}


		private void CalcControlPointShape(GraphicsPath path, ControlPointShape pointShape, int halfSize) {
			path.Reset();
			switch (pointShape) {
				case ControlPointShape.Circle:
					path.StartFigure();
					path.AddEllipse(-halfSize, -halfSize, halfSize + halfSize, halfSize + halfSize);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				case ControlPointShape.Diamond:
					path.StartFigure();
					path.AddLine(0, -halfSize, halfSize, 0);
					path.AddLine(halfSize, 0, 0, halfSize);
					path.AddLine(0, halfSize, -halfSize, 0);
					path.AddLine(-halfSize, 0, 0, -halfSize);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				case ControlPointShape.Hexagon:
					float sixthSize = (halfSize + halfSize) / 6f;
					path.StartFigure();
					path.AddLine(-sixthSize, -halfSize, sixthSize, -halfSize);
					path.AddLine(sixthSize, -halfSize, halfSize, 0);
					path.AddLine(halfSize, 0, sixthSize, halfSize);
					path.AddLine(sixthSize, halfSize, -sixthSize, halfSize);
					path.AddLine(-sixthSize, halfSize, -halfSize, 0);
					path.AddLine(-halfSize, 0, -sixthSize, -halfSize);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				case ControlPointShape.RotateArrow:
					rectBuffer.X = rectBuffer.Y = -halfSize;
					rectBuffer.Width = rectBuffer.Height = halfSize + halfSize;
					PointF? p = Geometry.IntersectCircleWithLine(0f, 0f, halfSize, 0, 0, -halfSize, -halfSize, true);
					Debug.Assert(p.HasValue);
					float quaterSize = halfSize / 2f;

					path.StartFigure();
					// arrow line
					path.AddArc(rectBuffer, -90, 315);
					path.AddLine(p.Value.X, p.Value.Y, -halfSize, -halfSize);
					path.AddLine(-halfSize, -halfSize, 0, -halfSize);
					path.CloseFigure();

					// closed arrow tip
					//path.StartFigure();
					//path.AddLine(0, -halfSize, 0, 0);
					//path.AddLine(0, 0, p.Value.X + 1, p.Value.Y + 1);
					//path.AddLine(p.Value.X + 1, p.Value.Y + 1, 0, 0);
					//path.AddLine(0, 0, 0, -halfSize);
					//path.CloseFigure();

					// open arrow tip
					path.StartFigure();
					path.AddLine(0, -halfSize, 0, 0);
					path.AddLine(0, 0, -quaterSize, -quaterSize);
					path.AddLine(-quaterSize, -quaterSize, 0, 0);
					path.AddLine(0, 0, 0, -halfSize);
					path.CloseFigure();

					path.CloseAllFigures();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
					break;
				case ControlPointShape.Square:
					rectBuffer.X = rectBuffer.Y = -halfSize;
					rectBuffer.Width = rectBuffer.Height = halfSize + halfSize;
					path.StartFigure();
					path.AddRectangle(rectBuffer);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				default: throw new nShapeUnsupportedValueException(typeof(ControlPointShape), pointShape);
			}
		}


		private void CreatePen(Color color, ref Pen pen) {
			CreatePen(color, 255, 1, ref pen);
		}


		private void CreatePen(Color color, byte alpha, ref Pen pen) {
			CreatePen(color, alpha, 1, ref pen);
		}


		private void CreatePen(Color color, float lineWidth, ref Pen pen) {
			CreatePen(color, 255, lineWidth, ref pen);
		}


		private void CreatePen(Color color, byte alpha, float lineWidth, ref Pen pen) {
			if (pen != null) pen.Dispose();
			pen = new Pen(alpha != 255 ? Color.FromArgb(alpha, color) : color, lineWidth);
			pen.LineJoin = LineJoin.Round;
			pen.StartCap = LineCap.Round;
			pen.EndCap = LineCap.Round;
		}


		private void CreateBrush(Color color, ref Brush brush) {
			if (brush != null) brush.Dispose();
			brush = new SolidBrush(color);
		}


		private void CreateBrush(Color color, byte alpha, ref Brush brush) {
			if (brush != null) brush.Dispose();
			brush = new SolidBrush(Color.FromArgb(alpha, color));
		}


		private void CreateBrush(Color gradientStartColor, Color gradientEndColor, Rectangle brushBounds, float gradientAngle, ref Brush brush) {
			if (brush != null) brush.Dispose();
			brush = new LinearGradientBrush(brushBounds, gradientStartColor, gradientEndColor, gradientAngle, true);
		}


		private void DrawParentOutline(Graphics graphics, Shape parentShape) {
			if (parentShape.Parent != null)
				DrawParentOutline(graphics, parentShape.Parent);
			parentShape.DrawOutline(graphics, OutlineInactivePen);
		}


		/// <summary>
		/// Translates and draws the given ControlPoint path at the given position (diagram coordinates).
		/// </summary>
		private void DrawControlPointPath(GraphicsPath path, int x, int y, Pen pen, Brush brush) {
			if (currentGraphics == null) throw new InvalidOperationException("Calling this method is only allowed while painting.");
			// transform the given 
			DiagramToControl(x, y, out x, out y);

			// transform ControlPoint Shape
			pointMatrix.Reset();
			pointMatrix.Translate(x, y);
			path.Transform(pointMatrix);
			// draw ConnectionPoint shape
			currentGraphics.FillPath(brush, path);
			currentGraphics.DrawPath(pen, path);
			// undo ControlPoint transformation
			pointMatrix.Reset();
			pointMatrix.Translate(-x, -y);
			path.Transform(pointMatrix);
		}


		private void UpdateScrollBars() {
			try {
				SuspendLayout();
				if (showScrollBars && Diagram != null) {
					CalcTransformation();

					Rectangle diagramScrollArea = DiagramBounds;
					Rectangle controlScrollArea;
					DiagramToControl(diagramScrollArea, out controlScrollArea);

					// Show/Hide vertical scroll bar
					if (controlScrollArea.Height < DrawBounds.Height) {
						if (scrollBarV.Visible) scrollBarV.Visible = false;
					} else if (!scrollBarV.Visible) scrollBarV.Visible = true;

					// Show/Hide horizontal scroll bar
					if (controlScrollArea.Width < DrawBounds.Width) {
						if (scrollBarH.Visible)
							hScrollBarPanel.Visible = scrollBarH.Visible = false;
					} else if (!scrollBarH.Visible)
						hScrollBarPanel.Visible = scrollBarH.Visible = true;

					int zoomedDrawHeight = (int)Math.Round(DrawBounds.Height / zoomfactor);
					int zoomedDrawWidth = (int)Math.Round(DrawBounds.Width / zoomfactor);
					int zoomedDiagramPosX = (int)Math.Round(diagramPosX / zoomfactor);
					int zoomedDiagramPosY = (int)Math.Round(diagramPosY / zoomfactor);
					int zoomedDiagramMargin = (int)Math.Round(diagramMargin / zoomfactor);
					int smallChange = Math.Max(1, GridSize / 2);

					// Set vertical scrollbar position, size and limits
					if (controlScrollArea.Height < DrawBounds.Height)
						scrollBarH.Width = DrawBounds.Width;
					else {
						// Set horizontal scrollbar's width
						scrollBarH.Width = DrawBounds.Width;
						// Set up vertical scrollbar
						scrollBarV.SmallChange = smallChange;
						scrollBarV.LargeChange = Math.Max(1, zoomedDrawHeight);
						scrollBarV.Minimum = diagramScrollArea.Y + zoomedDiagramPosY - zoomedDiagramMargin;
						scrollBarV.Maximum = (diagramScrollArea.Height - zoomedDrawHeight) 
							+ scrollBarV.LargeChange + zoomedDiagramPosY + zoomedDiagramMargin;
					}

					// Set horizontal scrollBar position, size  and limits
					if (controlScrollArea.Width < DrawBounds.Width)
						scrollBarV.Height = DrawBounds.Height;
					else {
						// Set vertical scrollbar's height
						scrollBarV.Height = DrawBounds.Height;
						// Set up horizontal scrollbar
						scrollBarH.SmallChange = smallChange;
						scrollBarH.LargeChange = Math.Max(1, zoomedDrawWidth);
						scrollBarH.Minimum = diagramScrollArea.X + zoomedDiagramPosX - zoomedDiagramMargin;
						scrollBarH.Maximum = (diagramScrollArea.Width - zoomedDrawWidth)
							+ scrollBarH.LargeChange + zoomedDiagramPosX + zoomedDiagramMargin;
					}
					// Maintain scroll position when zooming out
					ScrollTo(scrollBarH.Value, scrollBarV.Value);
				} else {
					if (scrollBarV.Visible) scrollBarV.Visible = false;
					if (scrollBarH.Visible) scrollBarH.Visible = false;
					if (hScrollBarPanel.Visible) hScrollBarPanel.Visible = false;
				}
			} finally { ResumeLayout(); }
		}


		private void UpdateControlBrush() {
			if (controlBrush == null) {
				if (highQualityBackground) {
					rectBuffer.X = 0;
					rectBuffer.Y = 0;
					rectBuffer.Width = 1000;
					rectBuffer.Height = 1000;
					if (gradientBackColor == Color.Empty) {
						// create a gradient brush based on the given BackColor (1/3 lighter in the upperLeft, 1/3 darker in the lowerRight)
						int lR = BackColor.R + ((BackColor.R / 3)); if (lR > 255) lR = 255;
						int lG = BackColor.G + ((BackColor.G / 3)); if (lG > 255) lG = 255;
						int lB = BackColor.B + ((BackColor.B / 3)); if (lB > 255) lB = 255;
						int dR = BackColor.R - ((BackColor.R / 3)); if (lR < 0) lR = 0;
						int dG = BackColor.G - ((BackColor.G / 3)); if (lG < 0) lG = 0;
						int dB = BackColor.B - ((BackColor.B / 3)); if (lB < 0) lB = 0;
						controlBrush = new LinearGradientBrush(rectBuffer, Color.FromArgb(lR, lG, lB), Color.FromArgb(dR, dG, dB), controlBrushGradientAngle);
					} else controlBrush = new LinearGradientBrush(rectBuffer, BackColorGradient, BackColor, controlBrushGradientAngle);
				} else controlBrush = new SolidBrush(BackColor);
				controlBrushSize = Size.Empty;
			}

			// apply transformation
			if (controlBrush is LinearGradientBrush && this.Size != controlBrushSize) {
				double rectWidth = Math.Abs((1000 * controlBrushGradientCos) - (1000 * controlBrushGradientSin));		// (width * cos) - (Height * sin)
				double rectHeight = Math.Abs((1000 * controlBrushGradientSin) + (1000 * controlBrushGradientCos));		// (width * sin) + (height * cos)
				double gradLen = (rectWidth + rectHeight) / 2;
				float scaleX = (float)(Width / gradLen);
				float scaleY = (float)(Height / gradLen);

				((LinearGradientBrush)controlBrush).ResetTransform();
				((LinearGradientBrush)controlBrush).ScaleTransform(scaleX, scaleY);
				((LinearGradientBrush)controlBrush).RotateTransform(controlBrushGradientAngle);
				controlBrushSize = this.Size;
			}
		}


		/// <summary>
		/// Draw the display control including the diagram's shadow (if a diagram is displayed)
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="clipRectangle"></param>
		private void DrawControl(Graphics graphics, Rectangle clipRectangle) {
			UpdateControlBrush();
			clipRectangle.Inflate(2, 2);
			if (Diagram == null) {
				// No diagram is shown... just fill the control with its background color/gradient
				graphics.FillRectangle(controlBrush, clipRectangle.X, clipRectangle.Y, clipRectangle.Width, clipRectangle.Height);
			} else {
				Rectangle diagramBounds = Rectangle.Empty;
				diagramBounds.Size = Diagram.Size;
				DiagramToControl(diagramBounds, out diagramBounds);

				// =====   DRAW CONTROL BACKGROUND   =====
				if (Diagram != null && !Diagram.BackgroundColor.IsEmpty && Diagram.BackgroundColor.A == 255 && Diagram.BackgroundGradientColor.A == 255) {
					// Above the diagram
					if (clipRectangle.Top < diagramBounds.Top)
						graphics.FillRectangle(controlBrush,
							clipRectangle.Left, clipRectangle.Top,
							clipRectangle.Width, diagramBounds.Top - clipRectangle.Top);

					// Left of the diagram
					if (clipRectangle.Left < diagramBounds.Left)
						graphics.FillRectangle(controlBrush,
							clipRectangle.Left, Math.Max(clipRectangle.Top, diagramBounds.Top - 1),
							diagramBounds.Left - clipRectangle.Left, Math.Min(clipRectangle.Height, diagramBounds.Height + 2));

					// Right of the diagram
					if (clipRectangle.Right > diagramBounds.Right)
						graphics.FillRectangle(controlBrush,
							diagramBounds.Right, Math.Max(clipRectangle.Top, diagramBounds.Top - 1),
							clipRectangle.Right - diagramBounds.Right, Math.Min(clipRectangle.Height, diagramBounds.Height + 2));

					// Below the diagram
					if (clipRectangle.Bottom > diagramBounds.Bottom)
						graphics.FillRectangle(controlBrush,
							clipRectangle.Left, diagramBounds.Bottom,
							clipRectangle.Width, clipRectangle.Bottom - diagramBounds.Bottom);
				} else graphics.FillRectangle(controlBrush, clipRectangle);
				

				// Draw diagram's shadow (if there is a diagram)
				if (clipRectangle.Right >= diagramBounds.Right
					|| clipRectangle.Bottom >= diagramBounds.Top) {
					int zoomedShadowSize = (int)Math.Round(shadowSize * zoomfactor);
					//if (Diagram.BackgroundColor.A < 255 || Diagram.BackgroundGradientColor.A < 255)
					//   graphics.FillRectangle(diagramShadowBrush, diagramBounds.X + zoomedShadowSize, diagramBounds.Y + zoomedShadowSize, diagramBounds.Width, diagramBounds.Height);
					// else 
					graphics.FillRectangle(diagramShadowBrush,
						diagramBounds.Right, diagramBounds.Y + zoomedShadowSize,
						zoomedShadowSize, diagramBounds.Height);
					graphics.FillRectangle(diagramShadowBrush,
						diagramBounds.X + zoomedShadowSize, diagramBounds.Bottom,
						diagramBounds.Width - zoomedShadowSize, zoomedShadowSize);
				}
			}
		}


		private void DrawDiagram(Graphics graphics, Rectangle clipRectangle) {
			Debug.Assert(Diagram != null);

			// Clipping transformation
			// transform clipping area from control to Diagram coordinates
			clipRectangle.Inflate(2, 2);
			ControlToDiagram(clipRectangle, out clipRectBuffer);
			
			Rectangle diagramBounds = Rectangle.Empty;
			diagramBounds.Size = Diagram.Size;
			DiagramToControl(diagramBounds, out diagramBounds);

			// Draw Background
			RestoreTransformation();
			Diagram.DrawBackground(graphics, clipRectBuffer);
			ResetTransformation(graphics);

			// Draw grid
			if (gridVisible) {
				int zoomedGridSpace = (int)Math.Round(gridSpace * zoomfactor);
				if (zoomedGridSpace > 0) {
					// Calculate grid bounds
					int left = Math.Max(clipRectangle.Left, diagramBounds.Left);
					int top = Math.Max(clipRectangle.Top, diagramBounds.Top);
					int right = Math.Min(clipRectangle.Right, diagramBounds.Right);
					int bottom = Math.Min(clipRectangle.Bottom, diagramBounds.Bottom);
					
					// Calculate grid start/end positions for the given clip rectangle
					int startX = left - ((left % zoomedGridSpace) - ((diagramPosX - (int)Math.Round(scrollPosX * zoomfactor)) % zoomedGridSpace));
					int startY = top - ((top % zoomedGridSpace) - ((diagramPosY - (int)Math.Round(scrollPosY * zoomfactor)) % zoomedGridSpace));
					int endX = Math.Min(right, diagramBounds.Right);
					int endY = Math.Min(bottom, diagramBounds.Bottom);

					// Line grid
					for (int i = startX; i <= endX; i += zoomedGridSpace)
						graphics.DrawLine(GridPen, i, top, i, bottom);	// draw vertical lines
					for (int i = startY; i <= endY; i += zoomedGridSpace)
						graphics.DrawLine(GridPen, left, i, right, i);	// draw horizontal lines
					
					// Cross grid (very slow!)
					// ToDo: Use a pen with a dash pattern
					//for (int x = startX; x <= endX; x += zoomedGridSpace) {
					//   for (int mouseY = startY; mouseY <= endY; mouseY += zoomedGridSpace) {
					//      graphics.DrawLine(gridPen, x - 1, mouseY, x + 1, mouseY);
					//      graphics.DrawLine(gridPen, x, mouseY - 1, x, mouseY + 1);
					//   }
					//}
					
					// Point grid
					//Rectangle r = Rectangle.Empty;
					//r.X = startX;
					//r.Y = startY;
					//r.Width = endX;
					//r.Height = endY;
					//Size s = Size.Empty;
					//s.Width = zoomedGridSpace;
					//s.Height = zoomedGridSpace;
					//ControlPaint.DrawGrid(graphics, r, s, Color.Transparent);
				}
			}
			// Draw diagram border
			graphics.DrawRectangle(Pens.Black, diagramBounds);
			

			// Draw shapes and their outlines (selection indicator)
			RestoreTransformation();
			// Draw Shapes
			Diagram.DrawShapes(graphics, GetVisibleLayers(), clipRectBuffer);
			// Draw selection indicator(s)
			foreach (Shape shape in selectedShapes.BottomUp) {
				if (shape.DisplayService != this) continue;
				if (shape.IntersectsWith(clipRectBuffer.X, clipRectBuffer.Y, clipRectBuffer.Width, clipRectBuffer.Height)) {
					// ToDo: * Should DrawShapeOutline(...) draw LinearShapes with LineCaps? (It doesn't at the moment)
					// ToDo:	* If the selected shape implements ILinearShape, try to get it's LineCaps
					// ToDo:	* Find a way how to obtain the CustomLineCaps from the ToolCache without knowing if a line has CapStyles properties...

					// Draw Shape's Outline
					DrawShapeOutline(nShapeDrawMode.Normal, shape);
				}
			}

			// Now draw Handles, caption bounds, etc
			ControlPointCapabilities capabilities = 0;
			if (selectedShapes.Count == 1) capabilities = ControlPointCapabilities.Rotate | ControlPointCapabilities.Glue | ControlPointCapabilities.Resize;
			else if (selectedShapes.Count > 1) capabilities = ControlPointCapabilities.Rotate;
			if (selectedShapes.Count > 0) {
				ResetTransformation();
				foreach (Shape shape in SelectedShapes.BottomUp) {
					if (shape.DisplayService != this) continue;
					if (shape.IntersectsWith(clipRectBuffer.X, clipRectBuffer.Y, clipRectBuffer.Width, clipRectBuffer.Height)) {
						// Draw ControlPoints
						DrawControlPoints(shape, capabilities);
						// Draw CaptionTextBounds / InPlaceTextBox
						if (inplaceTextbox != null) {
							graphics.FillRectangle(InplaceTextboxBackBrush, inplaceTextbox.Bounds);
							graphics.DrawRectangle(HandleInactivePen, inplaceTextbox.Bounds);
						} else if (shape is ICaptionedShape)
							DrawCaptionBounds(nShapeDrawMode.Deactivated, (ICaptionedShape)shape);
					}
				}
				RestoreTransformation();
			}

			// Draw tool preview
			if (CurrentTool != null) CurrentTool.Draw(this);

			// Draw debug infos
#if DEBUG

			#region Fill occupied cells and draw cell borders
			//((ShapeCollection)diagramController.Diagram.Shapes).DrawOccupiedCells(graphics, diagramController.Diagram.Width, diagramController.Diagram.Height);

			//// Draw cell borders
			//for (int iX = 0; iX < diagramController.Diagram.Width; iX += Diagram.CellSize)
			//   graphics.DrawLine(Pens.Green, iX, 0, iX, diagramController.Diagram.Height);
			//for (int iY = 0; iY < diagramController.Diagram.Height; iY += Diagram.CellSize)
			//   graphics.DrawLine(Pens.Green, 0, iY, diagramController.Diagram.Width, iY);
			#endregion

			#region Visualize invalidated Rectangles
			//if (clipRectBrush != clipRectBrush1)
			//   clipRectBrush = clipRectBrush1;
			//else clipRectBrush = clipRectBrush2;
			//graphics.FillRectangle(clipRectBrush, clipRectBuffer);
			#endregion

			#region Visualize AutoScroll Bounds
			//Rectangle autoScrollArea = DrawBounds;
			//autoScrollArea.Inflate(-autoScrollMargin, -autoScrollMargin);
			//ControlToDiagram(autoScrollArea, out autoScrollArea);
			//graphics.DrawRectangle(Pens.Blue, autoScrollArea);
			//graphics.DrawString(string.Format("AutoScroll Area: {0}", autoScrollArea), Font, Brushes.Blue, autoScrollArea.Location);

			//Point p = PointToClient(Control.MousePosition);
			//if (AutoScrollAreaContainsPoint(p.X, p.Y)) {
			//   ControlToDiagram(p, out p);
			//   GdiHelpers.DrawPoint(graphics, Pens.Red, p.X, p.Y, 3);
			//}
			#endregion

			// Count invalidation
			//++paintCounter;
#endif
		}

		#endregion


		#region [Private] Methods: Shape selection implementation

		/// <summary>
		/// Invalidate all selected Shapes and their ControlPoints before clearing the selection
		/// </summary>
		private void ClearSelection() {
			foreach (Shape shape in selectedShapes)
				InvalidateShape(shape);
			selectedShapes.Clear();
		}


		/// <summary>
		/// Invalidates the shape (or its parent(s)) along with all ControlPoints
		/// </summary>
		/// <param name="shape"></param>
		private void InvalidateShape(Shape shape) {
			if (shape.Parent != null)
				// parents invalidate their children themselves
				InvalidateShape(shape.Parent);
			else {
				shape.Invalidate();
				InvalidateGrips(shape, ControlPointCapabilities.All);
			}
		}


		private void UnselectShapesOfInvisibleLayers() {
			foreach (Shape selectedShape in selectedShapes.TopDown)
				if ((Diagram.GetShapeLayers(selectedShape) & HiddenLayers) != 0)
					UnselectShape(selectedShape);
		}


		/// <summary>
		/// Removes the shape from the list of selected shapes and invalidates the shape and its ControlPoints
		/// </summary>
		/// <param name="shape"></param>
		private void DoUnselectShape(Shape shape) {
			if (shape.Parent != null) {
				foreach (Shape s in shape.Parent.Children)
					RemoveShapeFromSelection(s);
			} else RemoveShapeFromSelection(shape);
		}


		private void RemoveShapeFromSelection(Shape shape) {
			//Debug.Assert(selectedShapes.Contains(shape));
			selectedShapes.Remove(shape);
			InvalidateGrips(shape, ControlPointCapabilities.All);
		}


		/// <summary>
		/// Checks if the shape has a parent and handles ShapeAggregation selection in case.
		/// </summary>
		/// <param name="shape"></param>
		private void DoSelectShape(Shape shape, bool addToSelection) {
			if (shape == null) throw new ArgumentNullException("shape");
			// Check if the selected shape is a child shape. 
			// Sub-selection of a CompositeShapes' children is not allowed
			if (shape.Parent != null) {
				if (!(shape.Parent is IShapeGroup))
					DoSelectShape(shape.Parent, addToSelection);
				else {
					if (!addToSelection
						&& selectedShapes.Count == 1
						&& (selectedShapes.Contains(shape.Parent) || selectedShapes.TopMost.Parent == shape.Parent)) {
						ClearSelection();
						AddShapeToSelection(shape);
					} else {
						if (!addToSelection)
							ClearSelection();
						if (!selectedShapes.Contains(shape.Parent))
							AddShapeToSelection(shape.Parent);
					}
				}
			} else {
				// standard selection
				if (!addToSelection)
					ClearSelection();
				AddShapeToSelection(shape);
			}
		}


		private void SelectShapeAggregation(ShapeAggregation aggregation, bool addToSelection) {
			shapeBuffer.Clear();
			shapeBuffer.AddRange(aggregation);
			bool allSelected = true;
			int cnt = shapeBuffer.Count;
			for (int i = 0; i < cnt; ++i) {
				if (!selectedShapes.Contains(shapeBuffer[i])) {
					allSelected = false;
					break;
				}
			}
			// if all Shapes of the aggregation are selected, select the shape itself
			if (allSelected) {
				// If the shape should be added to the selection, remove the aggregation's shapes
				// from the selection and add the selected shape
				if (addToSelection) {
					foreach (Shape s in aggregation)
						RemoveShapeFromSelection((Shape)s);
				} else
					ClearSelection();
				AddShapesToSelection(aggregation);
			} else {
				if (!addToSelection)
					ClearSelection();
				AddShapesToSelection(aggregation);
			}
		}


		/// <summary>
		/// Adds the shapes to the list of selected shapes and invalidates the shapes and their controlPoints
		/// </summary>
		private void AddShapesToSelection(IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes)
				AddShapeToSelection(shape);
		}


		/// <summary>
		/// Adds the shapes to the list of selected shapes and invalidates the shape and its controlPoints
		/// </summary>
		private void AddShapeToSelection(Shape shape) {
			// When selecting with frame, it can easily happen that shapes are inside 
			// the selection frame that are already selected... so skip them here
			if (!selectedShapes.Contains(shape)) {
				selectedShapes.Add(shape);
				InvalidateShape(shape);
			}
		}


		/// <summary>
		/// Enables/disables all ownerDisplay context menu items that are not suitable depending on the selected shapes. 
		/// Raises the ShapesSelected event.
		/// </summary>
		private void PerformSelectionNotifications() {
			if (diagramSetController != null) {
				diagramSetController.SetObjects(1, GetModelObjects(selectedShapes));
				diagramSetController.SetObjects(0, selectedShapes);
			}
			if (ShapesSelected != null) ShapesSelected(this, eventArgs);
		}


		private IEnumerable<IModelObject> GetModelObjects(IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes) {
				if (shape.ModelObject == null) continue;
				else yield return shape.ModelObject;
			}
		}

		#endregion


		#region [Private] Methods

		private bool SelectingChangedShapes {
			get { return collectingChangesCounter > 0; }
		}
		
		
		private void StartSelectingChangedShapes() {
			Debug.Assert(collectingChangesCounter >= 0);
			if (collectingChangesCounter == 0) {
				SuspendLayout();
				UnselectAll();
			}
			++collectingChangesCounter;
		}


		private void EndSelectingChangedShapes() {
			Debug.Assert(collectingChangesCounter > 0);
			--collectingChangesCounter;
			if (collectingChangesCounter == 0) ResumeLayout();
		}
		
		
		private void DisposeObject(IDisposable disposableObject) {
			if (disposableObject != null) disposableObject.Dispose();
			disposableObject = null;
		}


		private Cursor GetDefaultCursor() {
			if (universalScrollEnabled) return Cursors.NoMove2D;
			else return Cursors.Default;
		}
		
		
		private Cursor LoadCursorFromResource(byte[] resource, int cursorId) {
			Cursor result = null;
			if (resource != null) {
				MemoryStream stream = new MemoryStream(resource, 0, resource.Length, false);
				try {
					result = new Cursor(stream);
					result.Tag = cursorId;
				} finally {
					stream.Close();
					stream.Dispose();
				}
			}
			return result;
		}


		private void LoadRegisteredCursor(int cursorId) {
			Debug.Assert(!registeredCursors.ContainsKey(cursorId));
			Cursor cursor = LoadCursorFromResource(CursorProvider.GetResource(cursorId), cursorId);
			registeredCursors.Add(cursorId, cursor ?? Cursors.Default);
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


		private LayerIds GetVisibleLayers() {
			LayerIds result = LayerIds.None;
			if (diagramController.Diagram != null) {
				foreach (Layer layer in diagramController.Diagram.Layers) {
					if ((HiddenLayers & layer.Id) == 0 && layer.LowerZoomThreshold <= zoomLevel && layer.UpperZoomThreshold >= zoomLevel)
						result |= layer.Id;
				}
			}
			return result;
		}
		

		//private bool shapesSelected(bool withModelObjects) {
		//   bool result = false;
		//   if (selectedShapes.Count > 0) {
		//      result = true;
		//      if (withModelObjects) {
		//         foreach (Shape shape in selectedShapes) {
		//            if (shape.ModelObject == null) {
		//               result = false;
		//               break;
		//            }
		//         }
		//      }
		//   }
		//   return result;
		//}


		/// <summary>
		/// Replaces all the shapes with clones and clears their DisplayServices
		/// </summary>
		private void CreateNewClones(ShapeCollection shapes, bool withModelObjects) {
			CreateNewClones(shapes, withModelObjects, 0, 0);
		}


		/// <summary>
		/// Replaces all the shapes with clones and clears their DisplayServices
		/// </summary>
		private void CreateNewClones(ShapeCollection shapes, bool withModelObjects, int offsetX, int offsetY) {
			// clone from last to first shape in order to maintain the ZOrder
			foreach (Shape shape in shapes.BottomUp) {
				Shape clone = shape.Clone();
				if (withModelObjects) clone.ModelObject = shape.ModelObject.Clone();
				clone.MoveControlPointBy(ControlPointId.Reference, offsetX, offsetY, ResizeModifiers.None);
				shapes.Replace(shape, clone);
			}
		}


		private bool ScrollBarContainsPoint(Point p) {
			return ScrollBarContainsPoint(p.X, p.Y);
		}


		private bool ScrollBarContainsPoint(int x, int y) {
			bool result = (Geometry.RectangleContainsPoint(scrollBarH.Left, scrollBarH.Top, scrollBarH.Width, scrollBarH.Height, x, y)
				|| Geometry.RectangleContainsPoint(scrollBarV.Left, scrollBarV.Top, scrollBarV.Width, scrollBarV.Height, x, y));
			return result;
		}


		/// <summary>
		/// Returns truw if the given point (control coordinates) is inside the auto scroll area
		/// </summary>
		private bool AutoScrollAreaContainsPoint(int x, int y) {
			return (x <= DrawBounds.Left + autoScrollMargin
				|| y <= DrawBounds.Top + autoScrollMargin
				|| x > DrawBounds.Width - autoScrollMargin
				|| y > DrawBounds.Height - autoScrollMargin);
		}


		private void StartUniversalScroll(Point startPos) {
		   universalScrollEnabled = true;
		   universalScrollStartPos = startPos;
			universalScrollFixPointBounds = Rectangle.Empty;
			universalScrollFixPointBounds.Size = universalScrollCursor.Size;
			universalScrollFixPointBounds.Offset(
				universalScrollStartPos.X - (universalScrollFixPointBounds.Width / 2),
				universalScrollStartPos.Y - (universalScrollFixPointBounds.Height / 2));
			Invalidate(universalScrollFixPointBounds);
		}


		private void PerformUniversalScroll(Point currentPos) {
			if (universalScrollEnabled){
				Cursor = GetUniversalScrollCursor(currentPos);
				if (!universalScrollFixPointBounds.Contains(currentPos)) {
					Invalidate(universalScrollFixPointBounds);
					const int slowDownFactor = 4;
					int minimumX = universalScrollCursor.Size.Width / 2;
					int minimumY = universalScrollCursor.Size.Height / 2;
					int deltaX = (currentPos.X - universalScrollStartPos.X);
					if (Math.Abs(deltaX) < minimumX) deltaX = 0;
					else deltaX /= slowDownFactor;
					int deltaY = (currentPos.Y - universalScrollStartPos.Y);
					if (Math.Abs(deltaY) < minimumY) deltaY = 0;
					else deltaY /= slowDownFactor;
					ScrollBy(deltaX, deltaY);

					if (!autoScrollTimer.Enabled) autoScrollTimer.Enabled = true;
				} else autoScrollTimer.Enabled = false;
			}
		}


		private Cursor GetUniversalScrollCursor(Point currentPos) {
			Cursor result;
			if (universalScrollFixPointBounds.Contains(currentPos)) 
				result = Cursors.NoMove2D;
			else {
				float angle = (360 + Geometry.RadiansToDegrees(Geometry.Angle(universalScrollStartPos, currentPos))) % 360;
				if ((angle > 337.5f && angle <= 360) || (angle >= 0 && angle <= 22.5f))
					result = Cursors.PanEast;
				else if (angle > 22.5f && angle <= 67.5f)
					result = Cursors.PanSE;
				else if (angle > 67.5f && angle <= 112.5f)
					result = Cursors.PanSouth;
				else if (angle > 112.5f && angle <= 157.5f)
					result = Cursors.PanSW;
				else if (angle > 157.5f && angle <= 202.5f)
					result = Cursors.PanWest;
				else if (angle > 202.5f && angle <= 247.5f)
					result = Cursors.PanNW;
				else if (angle > 247.5f && angle <= 292.5f)
					result = Cursors.PanNorth;
				else if (angle > 292.5f && angle <= 337.5f)
					result = Cursors.PanNE;
				else result = Cursors.NoMove2D;
			}
			return result;
		}
		
		
		private void EndUniversalScroll() {
			Invalidate(universalScrollFixPointBounds);
			autoScrollTimer.Enabled = false;
		   universalScrollEnabled = false;
			universalScrollStartPos = Geometry.InvalidPoint;
			universalScrollFixPointBounds = Geometry.InvalidRectangle;
		}


		private void ScrollBy(int deltaX, int deltaY) {
			if (deltaX != 0 || deltaY != 0) ScrollTo(scrollPosX + deltaX, scrollPosY + deltaY);
		}


		private void ScrollTo(int x, int y) {
			if (x < scrollBarH.Minimum) x = scrollBarH.Minimum;
			else if (x > scrollBarH.Maximum - scrollBarH.LargeChange) x = scrollBarH.Maximum - scrollBarH.LargeChange;
			if (y < scrollBarV.Minimum) y = scrollBarV.Minimum;
			else if (y > scrollBarV.Maximum - scrollBarV.LargeChange) y = scrollBarV.Maximum - scrollBarV.LargeChange;

			if (x != scrollPosX || y != scrollPosY) {
				if (inplaceTextbox != null) {
					// ToDo: Scroll InPlaceTextBox with along the rest of the display content
					CloseCaptionEditor(false);
				}
				int oldX = scrollPosX;
				int oldY = scrollPosY;
				SetScrollPosX(x);
				SetScrollPosY(y);

				Invalidate();
			}
		}
		
		
		private void SetScrollPosX(int newValue) {
			if (newValue < scrollBarH.Minimum) newValue = scrollBarH.Minimum;
			else if (newValue > scrollBarH.Maximum) newValue = scrollBarH.Maximum;
			scrollBarH.Value = newValue;
			scrollPosX = newValue;
		}


		private void SetScrollPosY(int newValue) {
			if (newValue < scrollBarV.Minimum) newValue = scrollBarV.Minimum;
			else if (newValue > scrollBarV.Maximum) newValue = scrollBarV.Maximum;
			scrollBarV.Value = newValue;
			scrollPosY = newValue;
		}

		#endregion


		#region [Private] Methods: Creating Actions

		private nShapeAction CreateSelectAllAction() {
			bool isFeasible = selectedShapes.Count != Diagram.Shapes.Count;
			string description = isFeasible ? "Select all shapes of the diagram"
				: "All shapes of the diagram are selected";
			return new DelegateAction("Select all", null, description, isFeasible, Permission.None,
					(action, project) => SelectAll());
		}


		private nShapeAction CreateUnselectAllAction() {
			bool isFeasible = selectedShapes.Count > 0;
			string description = isFeasible ? "Unselect all selected shapes" : "No shapes selected";
			return new DelegateAction("Unselect all", null, description, isFeasible, Permission.None,
				(action, project) => UnselectAll());
		}


		private nShapeAction CreateSelectByTypeAction(Shape shapeUnderCursor) {
			bool isFeasible = shapeUnderCursor != null;
			string description;
			if (isFeasible)
				description = string.Format("Select all shapes of type '{0}' in the diagram", shapeUnderCursor.Type.Name);
			else description = "No shape under the cursor";
			return new DelegateAction(
				(shapeUnderCursor == null) ? "Select all shapes of a type"
				: string.Format("Select all shapes of type '{0}'", shapeUnderCursor.Type.Name), 
				null, description, isFeasible, Permission.None, 
				(action, project) => SelectShapes(shapeUnderCursor.Type, MultiSelect));
		}


		private nShapeAction CreateSelectByTemplateAction(Shape shapeUnderCursor) {
			bool isFeasible;
			string description;
			string title;
			if (shapeUnderCursor == null) {
				isFeasible = false;
				title = "Select all shapes based on a template";
				description = "No shape under cursor";
			} else if (shapeUnderCursor.Template == null) {
				isFeasible = false;
				title = "Select all shapes based on a template";
				description = "The shape under the cursor is not based on any template";
			} else {
				isFeasible = true;
				string templateTitle = string.IsNullOrEmpty(shapeUnderCursor.Template.Title) ?
					shapeUnderCursor.Template.Name : shapeUnderCursor.Template.Title;
				title = string.Format("Select all shapes based on template '{0}'", templateTitle);
				description = string.Format("Select all shapes of the diagram based on template '{0}'", templateTitle);
			}
			return new DelegateAction(title, null, description, isFeasible, Permission.None, 
				(action, project) => SelectShapes(shapeUnderCursor.Template, MultiSelect));
		}


		private nShapeAction CreateBringToFrontAction(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = diagramSetController.CanLiftShapes(diagram, shapes);
			string description;
			if (isFeasible) {
				if (shapes.Count == 1) description = string.Format("Bring '{0}' to front", shapes.TopMost.Type.Name);
				else description = string.Format("Bring {0} shapes to foreground", shapes.Count);
			} else description = noShapesSelectedText;
			return new DelegateAction("Bring To Front", Properties.Resources.ToForeground, description,
				isFeasible, Permission.Layout, (a, p) => diagramSetController.LiftShapes(diagram, shapes, ZOrderDestination.ToTop));
		}


		private nShapeAction CreateSendToBackAction(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = diagramSetController.CanLiftShapes(diagram, shapes);
			string description;
			if (isFeasible) {
				if (shapes.Count == 1) description = string.Format("Send '{0}' to background", shapes.TopMost.Type.Name);
				else description = string.Format("Send {0} shapes to background", shapes.Count);
			} else description = noShapesSelectedText;
			return new DelegateAction("Send To Back", Properties.Resources.ToBackground, description,
				isFeasible, Permission.Layout, (a, p) => diagramSetController.LiftShapes(diagram, shapes, ZOrderDestination.ToBottom));
		}


		private nShapeAction CreateGroupShapesAction(Diagram diagram, IShapeCollection shapes, LayerIds activeLayers) {
			bool isFeasible = diagramSetController.CanGroupShapes(shapes);
			string description = isFeasible ? string.Format("Group {0} shapes", shapes.Count) : notEnoughShapesSelectedText;
			return new DelegateAction("Group Shapes", Properties.Resources.GroupBtn,
				description, isFeasible, Permission.Insert | Permission.Delete,
				(a, p) => PerformGroupShapes(diagram, shapes, activeLayers));
		}


		private nShapeAction CreateUngroupAction(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = diagramSetController.CanUngroupShape(diagram, shapes);
			string description;
			if (isFeasible)
				description = string.Format("Ungroup {0} shapes", shapes.TopMost.Children.Count);
			else {
				if (shapes.TopMost is IShapeGroup && shapes.TopMost.Parent is IShapeGroup)
					description = "The selected group is member of another group.";
				else description = noGroupSelectedText;
			}
			return new DelegateAction("Ungroup Shapes", Properties.Resources.UngroupBtn,
				description, isFeasible, Permission.Insert | Permission.Delete,
				(a, p) => PerformUngroupShapes(diagram, shapes.TopMost));


		}


		private nShapeAction CreateAggregateAction(Diagram diagram, IShapeCollection shapes, LayerIds activeLayers) {
			bool isFeasible = diagramSetController.CanAggregateShapes(diagram, shapes);
			string description = isFeasible ?
				string.Format("Aggregate {0} shapes into composite shape", shapes.Count - 1)
				: string.Format(notEnoughShapesSelectedText);
			return new DelegateAction("Aggregate Shapes", Properties.Resources.AggregateShapeBtn,
				description, isFeasible, Permission.Delete,
				(a, p) => PerformAggregateCompositeShape(diagram, shapes.Bottom, shapes, activeLayers));
		}


		private nShapeAction CreateUnaggregateAction(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = diagramSetController.CanSplitShapeAggregation(diagram, shapes);
			string description;
			if (isFeasible) description = "Split shape aggregation";
			else description = shapes.Count == 1 ? "No shape aggregation selected" : "Too many shapes selected";
			return new DelegateAction("Split Shapes", Properties.Resources.SplitShapeAggregationBtn,
				description, isFeasible, Permission.Insert,
				(a, p) => PerformSplitCompositeShape(diagram, shapes.TopMost));
		}


		private nShapeAction CreateCutAction(Diagram diagram, IShapeCollection shapes, bool modelObjectsAssigned, Point position) {
			string title = "Cut";
			Bitmap icon = Properties.Resources.CutBtn; ;
			Permission permission = Permission.Delete; ;
			bool isFeasible = diagramSetController.CanCut(diagram, shapes);
			string description = isFeasible ?
				string.Format("Cut {0} shape{1}", shapes.Count, shapes.Count > 1 ? "s" : "")
				: noShapesSelectedText;
			if (!modelObjectsAssigned)
				return new DelegateAction(title, icon, description, isFeasible, permission,
					(a, p) => PerformCut(diagram, shapes, false, position));
			else
				return new nShapeActionGroup(title, icon, description, isFeasible,
					new nShapeAction[] {
						// Cut shapes only
						new DelegateAction(title, icon, description, isFeasible, permission,
							(a, p) => PerformCut(diagram, shapes, false, position)),
						// Cut shapes with models
						new DelegateAction(title + withModelsPostFix, icon, description + withModelsPostFix,
							isFeasible, permission, (a, p) => PerformCut(diagram, shapes, true, position))
					}, 1);
		}


		private nShapeAction CreateCopyAction(Diagram diagram, IShapeCollection shapes, bool modelObjectsAssigned, Point position) {
			string title = "Copy";
			Bitmap icon = Properties.Resources.CopyBtn;
			Permission permission = Permission.None;
			bool isFeasible = diagramSetController.CanCopy(shapes);
			string description = isFeasible ?
				string.Format("Copy {0} shape{1}", shapes.Count, shapes.Count > 1 ? "s" : "")
				: noShapesSelectedText;
			if (!modelObjectsAssigned)
				return new DelegateAction(title, icon, description, isFeasible, permission,
					(a, p) => PerformCopy(diagram, shapes, false, position));
			else
				return new nShapeActionGroup(title, icon, description, isFeasible,
					new nShapeAction[] {
						// Cut shapes only
						new DelegateAction(title, icon, description, isFeasible, permission,
						(a, p) => PerformCopy(diagram, shapes, false, position)),
						// Cut shapes with models
						new DelegateAction(title + withModelsPostFix, icon, description + withModelsPostFix,
							isFeasible, permission, (a, p) => PerformCopy(diagram, shapes, true, position)) 
					}, 1);
		}


		private nShapeAction CreatePasteAction(Diagram diagram, IShapeCollection shapes, LayerIds activeLayers, Point position) {
			bool isFeasible = diagramSetController.CanPaste(diagram);
			string description = isFeasible ?
				string.Format("Paste {0} shape{1}", shapes.Count, shapes.Count > 1 ? "s" : "")
				: "No shapes cut/copied yet";
			return new DelegateAction("Paste", Properties.Resources.PasteBtn, description,
				isFeasible, Permission.Insert, (a, p) => Paste(position));
		}


		private nShapeAction CreateDeleteAction(Diagram diagram, IShapeCollection shapes, bool modelObjectsAssigned) {
			string title = "Delete";
			Bitmap icon = Properties.Resources.DeleteBtn;
			Permission permission = Permission.Delete;
			bool isFeasible = diagramSetController.CanDeleteShapes(diagram, shapes);
			string description = isFeasible ?
				string.Format("Delete {0} shape{1}", shapes.Count, shapes.Count > 1 ? "s" : "")
				: noShapesSelectedText;

			if (!modelObjectsAssigned)
				return new DelegateAction(title, icon, description, isFeasible, permission,
					(a, p) => PerformDelete(diagram, shapes, false));
			else {
				bool otherShapesAssignedToModels = OtherShapesAssignedToModels(shapes);
				string deleteWithModelsDesc;
				if (otherShapesAssignedToModels)
					deleteWithModelsDesc = "There are shapes assigned to the model(s)";
				else deleteWithModelsDesc = description + withModelsPostFix;

				return new nShapeActionGroup(title, icon, description, isFeasible,
					new nShapeAction[] {
						// Cut shapes only
						new DelegateAction(title, icon, description, isFeasible, permission,
							(a, p) => PerformDelete(diagram, shapes, false)),
						new DelegateAction(title + withModelsPostFix, icon, deleteWithModelsDesc,
							!otherShapesAssignedToModels, permission, (a, p) => PerformDelete(diagram, shapes, true))
					}, 1);
			}
		}


		private nShapeAction CreatePropertiesAction(Diagram diagram, IShapeCollection shapes, Point position) {
			bool isFeasible = true;
			string descriptionFormat = "Show {0} properties";
			string description = null;
			object obj = null;
			Shape s = shapes.FindShape(position.X, position.Y, ControlPointCapabilities.None, 0, null);
			if (s != null) {
				description = string.Format(descriptionFormat, s.Type.Name);
				obj = s;
			} else {
				description = string.Format(descriptionFormat, typeof(Diagram).Name);
				obj = diagram;
			}
			return new DelegateAction("Properties", Properties.Resources.DiagramPropertiesBtn3,
				description, isFeasible, Permission.ModifyData, (a, p) => DiagramSetController.SetObject(0, obj));
		}


		private nShapeAction CreateUndoAction() {
			bool isFeasible = Project.History.UndoCommandCount > 0;
			string description = isFeasible ? Project.History.GetUndoCommandDescription() : "No undo commands left";
			return new DelegateAction("Undo", Properties.Resources.UndoBtn, description, isFeasible,
				Permission.None, (a, p) => PerformUndo());
		}


		private nShapeAction CreateRedoAction() {
			bool isFeasible = Project.History.RedoCommandCount > 0;
			string description = isFeasible ? Project.History.GetRedoCommandDescription() : "No redo commands left";
			return new DelegateAction("Redo", Properties.Resources.RedoBtn, description, isFeasible,
				Permission.None, (a, p) => PerformRedo());
		}


		private void PerformGroupShapes(Diagram diagram, IShapeCollection shapes, LayerIds activeLayers) {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				diagramSetController.GroupShapes(diagram, shapes, activeLayers);
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformUngroupShapes(Diagram diagram, Shape shape) {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				diagramSetController.UngroupShapes(diagram, shape);
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformAggregateCompositeShape(Diagram diagram, Shape shape, IShapeCollection shapes, LayerIds activeLayers) {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				diagramSetController.AggregateCompositeShape(diagram, shapes.Bottom, shapes, activeLayers);
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformSplitCompositeShape(Diagram diagram, Shape shape) {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
			diagramSetController.SplitCompositeShape(diagram, shape);
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformCut(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects, Point position) {
			if (position != Geometry.InvalidPoint)
				diagramSetController.Cut(diagram, shapes, withModelObjects, position);
			else diagramSetController.Cut(diagram, shapes, withModelObjects);
		}


		private void PerformCopy(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects, Point position) {
			if (position != Geometry.InvalidPoint)
				diagramSetController.Copy(diagram, shapes, withModelObjects, position);
			else diagramSetController.Copy(diagram, shapes, withModelObjects);
		}


		private void PerformPaste(Diagram diagram, LayerIds layerIds, Point position) {
			try {
				StartSelectingChangedShapes();
				if (position == Geometry.InvalidPoint)
					diagramSetController.Paste(diagram, layerIds, GridSize, GridSize);
				else diagramSetController.Paste(diagram, layerIds, position);
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformDelete(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects){
			diagramSetController.DeleteShapes(diagram, shapes, withModelObjects);
		}


		private void PerformUndo() {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				Project.History.Undo();
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformRedo() {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				Project.History.Redo();
			} finally { EndSelectingChangedShapes(); }
		}


		private bool ModelObjectsAssigned(IEnumerable<Shape> shapes) {
			foreach (Shape s in shapes)
				if (s.ModelObject != null) return true;
			return false;
		}


		private bool OtherShapesAssignedToModels(IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes) {
				if (shape.ModelObject != null)
					if (OtherShapeAssignedToModel(shape.ModelObject, shape))
						return true;
			}
			return false;
		}


		private bool OtherShapeAssignedToModel(IModelObject modelObject, Shape shape) {
			foreach (Shape assignedShape in modelObject.Shapes) {
				if (assignedShape != shape) return true;
				foreach (IModelObject childModelObject in Project.Repository.GetModelObjects(modelObject))
					if (OtherShapeAssignedToModel(childModelObject, shape)) return true;
			}
			return false;
		}

		#endregion


		#region [Private] Methods: (Un)Registering events

		private void RegisterDiagramSetControllerEvents() {
			diagramSetController.ProjectChanging += diagramSetController_ProjectChanging;
			diagramSetController.ProjectChanged += diagramSetController_ProjectChanged;
			diagramSetController.SelectModelObjectsRequested += diagramSetController_SelectModelObjectRequested;
			if (diagramSetController.Project != null) RegisterProjectEvents();
		}

		
		private void UnregisterDiagramSetControllerEvents() {
			if (diagramSetController.Project != null) UnregisterProjectEvents();
			diagramSetController.SelectModelObjectsRequested -= diagramSetController_SelectModelObjectRequested;
		}


		private void RegisterProjectEvents() {
			if (!projectIsRegistered) {
				Debug.Assert(Project != null);
				Project.Opened += Project_ProjectOpen;
				Project.Closing += Project_ProjectClosing;
				Project.Closed += Project_ProjectClosed;
				projectIsRegistered = true;
				if (Project.IsOpen) RegisterRepositoryEvents();
			}
		}


		private void UnregisterProjectEvents() {
			if (projectIsRegistered) {
				Debug.Assert(Project != null);
				Project.Opened -= Project_ProjectOpen;
				Project.Closing -= Project_ProjectClosing;
				Project.Closed -= Project_ProjectClosed;
				projectIsRegistered = false;
				if (Project.Repository != null) UnregisterRepositoryEvents();
			}
		}


		private void RegisterRepositoryEvents() {
			if (!repositoryIsRegistered) {
				Debug.Assert(Project.Repository != null);
				Project.Repository.DiagramUpdated += Repository_DiagramUpdated;
				Project.Repository.ShapesInserted += Repository_ShapesInserted;
				Project.Repository.ShapesUpdated += Repository_ShapesUpdated;
				Project.Repository.ShapesDeleted += Repository_ShapesDeleted;
				Project.Repository.TemplateShapeReplaced += Repository_TemplateShapeReplaced;
				repositoryIsRegistered = true;
			}
		}


		private void UnregisterRepositoryEvents() {
			if (repositoryIsRegistered) {
				Debug.Assert(Project.Repository != null);
				Project.Repository.DiagramUpdated -= Repository_DiagramUpdated;
				Project.Repository.ShapesInserted -= Repository_ShapesInserted;
				Project.Repository.ShapesUpdated -= Repository_ShapesUpdated;
				Project.Repository.ShapesDeleted -= Repository_ShapesDeleted;
				Project.Repository.TemplateShapeReplaced -= Repository_TemplateShapeReplaced;
				repositoryIsRegistered = false;
			}
		}


		private void RegisterDiagramControllerEvents() {
			diagramController.DiagramChanged += Controller_DiagramChanged;
			diagramController.DiagramChanging += Controller_DiagramChanging;
		}


		private void UnregisterDiagramControllerEvents() {
			diagramController.DiagramChanged -= Controller_DiagramChanged;
			diagramController.DiagramChanging -= Controller_DiagramChanging;
		}

		#endregion


		#region [Private] Methods: EventHandler implementations

		private void diagramSetController_SelectModelObjectRequested(object sender, ModelObjectsEventArgs e) {
			if (Diagram != null) {
				UnselectAll();
				foreach (IModelObject modelObject in e.ModelObjects) {
					foreach (Shape s in modelObject.Shapes) {
						if (Diagram.Shapes.Contains(s)) SelectShape(s, true);
					}
				}
			}
		}


		private void diagramSetController_ProjectChanged(object sender, EventArgs e) {
			if (diagramSetController.Project != null) RegisterProjectEvents();
		}


		private void diagramSetController_ProjectChanging(object sender, EventArgs e) {
			if (diagramSetController.Project != null) UnregisterProjectEvents();
		}
		
		
		private void inPlaceTextBox_Leave(object sender, EventArgs e) {
			CloseCaptionEditor(true);
		}


		private void inPlaceTextBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter) {
				CloseCaptionEditor(true);
				e.Handled = true;
			} else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Escape) {
				CloseCaptionEditor(false);
				e.Handled = true;
			}
		}


		private void Project_ProjectClosing(object sender, EventArgs e) {
			Clear();
		}


		private void Project_ProjectClosed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
		}


		private void Project_ProjectOpen(object sender, EventArgs e) {
			RegisterRepositoryEvents();
		}


		private void Repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			if (Diagram != null && e.Diagram == Diagram) {
				UpdateScrollBars();
				Invalidate();
			}
		}


		private void Repository_ShapesUpdated(object sender, RepositoryShapesEventArgs e) {
			if (Diagram != null && SelectingChangedShapes) {
				foreach (Shape s in e.Shapes) {
					if (s.Diagram != null && s.Diagram == Diagram)
						SelectShape(s, true);
				}
			}
		}


		private void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
			if (Diagram != null && SelectingChangedShapes) {
				foreach (Shape s in e.Shapes) {
					if (s.Diagram != null && s.Diagram == Diagram)
						SelectShape(s, true);
				}
			}
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			foreach (Shape shape in e.Shapes)
				selectedShapes.Remove(shape);
		}


		private void Repository_TemplateShapeReplaced(object sender, RepositoryTemplateShapeReplacedEventArgs e) {
			SuspendLayout();
			foreach (Shape selectedShape in selectedShapes) {
				if (selectedShape.Template == e.Template) {
					foreach (Shape s in Diagram.Shapes.FindShapes(selectedShape.X, selectedShape.Y, ControlPointCapabilities.None, 0)) {
						if (s == selectedShape)
							selectedShapes.Remove(selectedShape);
						else if (s.Template == e.Template)
							selectedShapes.Replace(selectedShape, s);
					}
				}
			}
			ResumeLayout();
		}


		private void ScrollBars_MouseEnter(object sender, EventArgs e) {
			if (Cursor != Cursors.Default) Cursor = Cursors.Default;
		}


		private void ScrollBar_Scroll(object sender, ScrollEventArgs e) {
			if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
				ScrollTo(e.NewValue, scrollPosY);
			else ScrollTo(scrollPosX, e.NewValue); 
			OnScroll(e);
		}


		private void scrollBar_VisibleChanged(object sender, EventArgs e) {
			drawBounds = Geometry.InvalidRectangle;
			ScrollBar scrollBar = sender as ScrollBar;
			if (scrollBar != null) {
				Invalidate(RectangleToClient(scrollBar.RectangleToScreen(scrollBar.Bounds)));
				if (scrollBar == scrollBarH) SetScrollPosX(0);
				else if (scrollBar == scrollBarV) SetScrollPosY(0);
			}
		}

		
		private void autoScrollTimer_Tick(object sender, EventArgs e) {
			Point p = PointToClient(Control.MousePosition);
			OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, p.X, p.Y, 0));
		}


		private void ToolTip_Popup(object sender, PopupEventArgs e) {
			Point p = PointToClient(Control.MousePosition);
			toolTip.ToolTipTitle = "";
			if (Diagram != null) {
				Shape shape = Diagram.Shapes.FindShape(p.X, p.Y, ControlPointCapabilities.None, handleRadius, null);
				if (shape != null) {
					if (shape.ModelObject != null)
						toolTip.ToolTipTitle = shape.ModelObject.Name + " (" + shape.ModelObject.GetType() + ")";
				}
			}
		}


		private void Controller_DiagramChanging(object sender, EventArgs e) {
			Clear();
			if (DiagramChanging != null) DiagramChanging(this, e);
		}
		
		
		private void Controller_DiagramChanged(object sender, EventArgs e) {
			DisplayDiagram();
			if (DiagramChanged != null) DiagramChanged(this, e);
		}


		private void displayContextMenuStrip_Opening(object sender, CancelEventArgs e) {
			if (showDefaultContextMenu && Project != null) {
				// Remove DummyItem
				if (ContextMenuStrip.Items.Contains(dummyItem))
					ContextMenuStrip.Items.Remove(dummyItem);
				// Collect all actions provided by the current tool
				if (CurrentTool != null)
					WinFormHelpers.BuildContextMenu(ContextMenuStrip, CurrentTool.GetActions(this), Project, hideMenuItemsIfNotGranted);
				// Collect all actions provided by the display itself
				WinFormHelpers.BuildContextMenu(ContextMenuStrip, GetActions(), Project, hideMenuItemsIfNotGranted);
			}
		}


		private void displayContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
			if (showDefaultContextMenu) {
				WinFormHelpers.CleanUpContextMenu(ContextMenuStrip);
				// Add dummy item because context menus without items will not show
				if (ContextMenuStrip.Items.Count == 0)
					ContextMenuStrip.Items.Add(dummyItem);
			}
		}

		#endregion


		#region [Private] Types

		private enum EditAction { None, Copy, Cut, CopyWithModels, CutWithModels }
		

		private class EditBuffer {
			public EditBuffer() {
				action = EditAction.None;
				initialMousePos = null;
				pasteCount = 0;
				shapes = new ShapeCollection(5);
			}
			public void Clear() {
				initialMousePos = null;
				action = EditAction.None;
				pasteCount = 0;
				shapes.Clear();
			}
			public Point? initialMousePos;
			public EditAction action;
			public int pasteCount;
			public ShapeCollection shapes;
		}

		#endregion


		#region Fields

		// String constants for action titles and descriptions
		private const string noShapesSelectedText = "No shapes selected";
		private const string notEnoughShapesSelectedText = "Not enough shapes selected";
		private const string noGroupSelectedText = "No group selected";
		private const string withModelsPostFix = " with models";

		private static Dictionary<int, Cursor> registeredCursors = new Dictionary<int, Cursor>(10);
		private bool projectIsRegistered = false;
		private bool repositoryIsRegistered = false;
		
		// Contains all active layers (new shapes are assigned to these layers)
		private LayerIds activeLayers = LayerIds.None;
		// Contains all layer hidden by the user
		private LayerIds hiddenLayers = LayerIds.None;

		// Fields for mouse related display behavior (scroll and zoom)
		private bool zoomWithMouseWheel = true;
		private Point universalScrollStartPos = Geometry.InvalidPoint;
		private Rectangle universalScrollFixPointBounds = Geometry.InvalidRectangle;
		private Cursor universalScrollCursor = Cursors.NoMove2D;
		private bool universalScrollEnabled = false;
		private Timer autoScrollTimer = new Timer();

		private const int shadowSize = 20;
		private int diagramMargin = 40;
		private int autoScrollMargin = 40;
		private bool showScrollBars = true;
		private bool hideMenuItemsIfNotGranted = true;
		private int handleRadius = 3;
		private bool snapToGrid = true;
		private int snapDistance = 5;
		private int gridSpace = 20;
		private int zoomStepping = 5;
		private Size gridSize = Size.Empty;
		private bool gridVisible = true;
		private int minRotateDistance = 30;
		private bool showDefaultContextMenu = true;
		private ControlPointShape resizePointShape = ControlPointShape.Square;
		private ControlPointShape connectionPointShape = ControlPointShape.Circle;
		
		// Graphics and Graphics Settings
		private Graphics infoGraphics;
		private Matrix pointMatrix = new Matrix();
		private Matrix matrix = new Matrix(); 
		private Graphics currentGraphics;
		//// used for fixed refresh rate rendering without manually managing DoubleBuffering
		private Timer displayRefreshTimer = new Timer();
		private bool performInvalidation;
		private Rectangle invalidationBuffer = Rectangle.Empty;
		private bool graphicsIsTransformed = false;
		private int suspendInvalidateCounter = 0;
		private int collectingChangesCounter = 0;
		private Rectangle invalidatedAreaBuffer = Rectangle.Empty;
		private FillStyle hintBackgroundStyle = null;
		private LineStyle hintForegroundStyle = null;

		private bool highQualityBackground = true;
		private bool highQualityRendering = true;
		private nShapeRenderingQuality currentRenderingQuality;
		private nShapeRenderingQuality renderingQualityHigh = nShapeRenderingQuality.HighQuality;
		private nShapeRenderingQuality renderingQualityLow = nShapeRenderingQuality.DefaultQuality;

		// Colors
		private const byte previewColorAlpha = 96;
		private const byte previewBackColorAlpha = 64;
		private byte gridAlpha = 255;
		private byte selectionAlpha = 255;
		private byte inlaceTextBoxBackAlpha = 128;
		private byte toolPreviewColorAlpha = previewColorAlpha;
		private byte toolPreviewBackColorAlpha = previewBackColorAlpha;
		private Color backColor = Color.FromKnownColor(KnownColor.Control);
		private Color gradientBackColor = Color.FromKnownColor(KnownColor.Control);
		private Color gridColor = Color.White;
		private Color selectionNormalColor = Color.DarkGreen;
		private Color selectionHilightColor = Color.Firebrick;
		private Color selectionInactiveColor = Color.Gray;
		private Color selectionInteriorColor = Color.WhiteSmoke;
		private Color toolPreviewColor = Color.FromArgb(previewColorAlpha, Color.SteelBlue);
		private Color toolPreviewBackColor = Color.FromArgb(previewBackColorAlpha, Color.LightSlateGray);

		// Pens
		private Pen gridPen;								// pen for drawing the grid
		private Pen outlineInteriorPen;				// pen for the interior of thick outlines
		private Pen outlineNormalPen;					// pen for drawing thick shape outlines (normal)
		private Pen outlineHilightPen;				// pen for drawing thick shape outlines (highlighted)
		private Pen outlineInactivePen;				// pen for drawing thick shape outlines (inactive)
		private Pen handleNormalPen;					// pen for drawing shape handles (normal)
		private Pen handleHilightPen;					// pen for drawing connection point indicators
		private Pen handleInactivePen;				// pen for drawing inactive handles
		private Pen toolPreviewPen;					// Pen for drawing tool preview infos (rotation preview, selection frame, etc)
		private Pen outerSnapPen = new Pen(Color.FromArgb(196, Color.WhiteSmoke), 2);
		private Pen innerSnapPen = new Pen(Color.FromArgb(196, Color.SteelBlue), 1);
		
		// Brushes
		private Brush controlBrush = null;				// brush for painting the ownerDisplay control's background
		private Brush handleInteriorBrush = null;		// brush for filling shape handles
		private Brush toolPreviewBackBrush = null;	// brush for filling tool preview info (rotation preview, selection frame, etc)
		private Brush inplaceTextboxBackBrush = null; // Brush for filling the background of the inplaceTextBox.
		private Brush diagramShadowBrush = new SolidBrush(Color.FromArgb(128, Color.Gray)); // Brush for a shadow underneath the diagram

		// other drawing stuff
		private StringFormat previewTextFormatter = new StringFormat();
		private float controlBrushGradientAngle = 45f;
		private double controlBrushGradientSin = Math.Sin(Geometry.DegreesToRadians(45f));
		private double controlBrushGradientCos = Math.Cos(Geometry.DegreesToRadians(45f));
		private Size controlBrushSize = Size.Empty;
		private GraphicsPath rotatePointPath = new GraphicsPath();
		private GraphicsPath connectionPointPath = new GraphicsPath();
		private GraphicsPath resizePointPath = new GraphicsPath();
		private Point[] arrowShape = new Point[3];

		private int diagramPosX;		// Position of the left side of the Diagram on the control
		private int diagramPosY;		// Position of the upper side of the Diagram on the control
		private int zoomLevel = 100;	// Zoom level in percentage
		private float zoomfactor = 1f;// zoomFactor for transforming Diagram coordinates to control coordinates (range: >0 to x, 100% == 1)
		private int scrollPosX = 0;	// horizontal position of the scrolled Diagram (== horizontal scrollbar value)
		private int scrollPosY = 0;	// vertical position of the scrolled Diagram (== vertical scrollbar value)
		private int invalidateDelta;	// handle pieRadius or selection outline lineWidth (amount of pixels the invalidated area has to be increased)

		// Components
		private DiagramSetController diagramSetController;
		private DiagramController diagramController;

		// -- In-Place Editing --
		// text box currently used for in-place text editing
		private InPlaceTextBox inplaceTextbox;
		// shape currently edited
		private ICaptionedShape inplaceShape;
		// index of caption within shape
		private int inplaceCaptionIndex;
		
		// Lists and Collections
		private ShapeCollection selectedShapes = new ShapeCollection();
		private EditBuffer editBuffer = new EditBuffer();	// Buffer for Copy/Cut/Paste-Actions
		private Rectangle copyCutBounds = Rectangle.Empty;
		private Point copyCutMousePos = Point.Empty;
		private List<Shape> shapeBuffer = new List<Shape>();
		private List<IModelObject> modelBuffer = new List<IModelObject>();

		// Buffers
		private EventArgs eventArgs = new EventArgs();
		private Rectangle rectBuffer;						// buffer for rectangles
		private Point[] pointBuffer = new Point[4];	// point array buffer
		private Rectangle clipRectBuffer;				// buffer for clipRectangle transformation
		private Rectangle drawBounds;					// drawing area of the display (ClientRectangle - scrollbars)
		//private GraphicsPath selectionPath = new GraphicsPath();	// Path used for highlighting all selected selectedShapes

		// Temporary Buffer for last Mouse position (for MouseCursor sensitive context menu actions, e.g. Paste)
		private Point lastMousePos;
		
		// debugging stuff
#if DEBUG
		private Stopwatch stopWatch = new Stopwatch();
		//private long paintCounter;
		//private Brush clipRectBrush;
		private Brush clipRectBrush1 = new SolidBrush(Color.FromArgb(32, Color.Red));
		private Brush clipRectBrush2 = new SolidBrush(Color.FromArgb(32, Color.Green));
#endif
		#endregion
	}

}