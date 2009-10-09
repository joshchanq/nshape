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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

	public enum ControlPointShape { Circle, Square, Diamond, Hexagon, RotateArrow };

	#region EventArgs

	public class DiagramPresenterShapeClickEventArgs : EventArgs {

		public DiagramPresenterShapeClickEventArgs(Shape shape, MouseEventArgsDg mouseEventArgs) {
			this.shape = shape;
			this.mouseEventArgs = mouseEventArgs;
		}

		public Shape Shape { get { return shape; } }

		public MouseEventArgsDg Mouse { get { return mouseEventArgs; } }

		private Shape shape;
		private MouseEventArgsDg mouseEventArgs;
	}


	public class DiagramPresenterShapeEventArgs : EventArgs {

		public DiagramPresenterShapeEventArgs(Shape shape) {
			this.shape = shape;
		}

		public Shape Shape { get { return shape; } }

		private Shape shape;
	}

	#endregion


	/// <summary>
	/// Defines the interface between the tool and the diagram presenter.
	/// </summary>
	/// <status>reviewed</status>
	// IDiagramPresenter has to be a descendant of IDisplayService because Tools set their IDiagramPresenter as
	// the preview shape's DislpayService
	public interface IDiagramPresenter {

		#region Events

		event EventHandler ShapesSelected;

		event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeClick;

		event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeDoubleClick;

		event EventHandler<DiagramPresenterShapeEventArgs> ShapeInsert;

		event EventHandler<DiagramPresenterShapeEventArgs> ShapeRemove;

		event EventHandler DiagramChanged;

		event EventHandler<LayersEventArgs> LayerVisibilityChanged;

		event EventHandler<LayersEventArgs> ActiveLayersChanged;

		#endregion


		#region Properties

		DiagramSetController DiagramSetController { get; set; }

		Project Project { get; }

		Diagram Diagram { get; }

		IDisplayService DisplayService { get; }

		[Browsable(false)]
		IShapeCollection SelectedShapes { get; }

		LayerIds ActiveLayers { get; }

		LayerIds HiddenLayers { get; }

		#endregion


		#region Properties: Visuals

		/// <summary>
		/// Font used for hints in the diagram presentation (e.g. current angle during rotation)
		/// </summary>
		Font Font { get; }


		/// <summary>
		/// True, if high quality rendering is switched on.
		/// </summary>
		bool HighQualityRendering { get; }

		#endregion


		#region Properties: Behavior

		[Category("Behavior")]
		int SnapDistance { get; set; }

		[Category("Behavior")]
		bool SnapToGrid { get; set; }

		[Category("Appearance")]
		int GridSize { get; set; }

		[Category("Appearance")]
		bool ShowGrid { get; set; }

		[Category("Appearance")]
		ControlPointShape ResizeGripShape { get; set; }

		[Category("Appearance")]
		ControlPointShape ConnectionPointShape { get; set; }

		/// <summary>
		/// The size of a ControlPoint handle from the center to the outer handle bound.
		/// </summary>
		[Category("Appearance")]
		int GripSize { get; set; }

		[Browsable(false)]
		int ZoomedGripSize { get; }

		/// <summary>
		/// Zoom in percentage.
		/// </summary>
		int ZoomLevel { get; set; }

		bool Capture { get; set; }

		[Browsable(false)]
		int MinRotateRange { get; }

		#endregion


		#region Methods: (Un)Selecting shapes

		/// <summary>
		/// Clears the current selection.
		/// </summary>
		void UnselectAll();

		/// <summary>
		/// Removes the given Shape from the current selection.
		/// </summary>
		void UnselectShape(Shape shape);

		/// <summary>
		/// Selects the given shape. Current selection will be cleared.
		/// </summary>
		void SelectShape(Shape shape);

		/// <summary>
		/// Selects the given shape.
		/// </summary>
		/// <param name="shape">Shape to be selected.</param>
		/// <param name="addToCurrentSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		void SelectShape(Shape shape, bool addToSelection);

		/// <summary>
		/// Selects the given shape.
		/// </summary>
		/// <param name="shape">Shape to be selected.</param>
		/// <param name="addToCurrentSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		void SelectShapes(IEnumerable<Shape> shapes, bool addToSelection);

		/// <summary>
		/// Selects all shapes within the given area.
		/// </summary>
		/// <param name="area">All shapes in the given rectangle will be selected.</param>
		/// <param name="addToCurrentSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		void SelectShapes(Rectangle area, bool addToSelection);

		/// <summary>
		/// Selectes all shapes of the diagram
		/// </summary>
		void SelectAll();

		#endregion


		#region Methods: Coordinate transformation routines

		/// <summary>
		/// Calculate contol coordinates from diagram coordinates
		/// </summary>
		/// <param name="dX">X value in diagram coordinates</param>
		/// <param name="dY">Y value in diagram coordinates</param>
		/// <param name="cX">X value in control coordinates</param>
		/// <param name="cY">Y value in control coordinates</param>
		void DiagramToControl(int dX, int dY, out int cX, out int cY);

		/// <summary>
		/// Calculate contol coordinates from diagram coordinates
		/// </summary>
		/// <param name="dPt">Point in diagram coordinates</param>
		/// <param name="cPt">Point in control Coordinates</param>
		void DiagramToControl(Point dPt, out Point cPt);

		/// <summary>
		/// Calculate contol coordinates from diagram coordinates
		/// </summary>
		/// <param name="dRect">Rectangle in diagram coordinates</param>
		/// <param name="cRect">Rectangle in control coordinates</param>
		void DiagramToControl(Rectangle dRect, out Rectangle cRect);

		/// <summary>
		/// Calculate contol coordinates from diagram coordinates
		/// </summary>
		/// <param name="dDistance">Distance in diagram coordinates</param>
		/// <param name="cDistance">Distance in control coordinates</param>
		void DiagramToControl(int dDistance, out int cDistance);

		/// <summary>
		/// Calculate contol coordinates from diagram coordinates
		/// </summary>
		/// <param name="dSize">Size in diagram coordinates</param>
		/// <param name="cSize">Size in control coordinates</param>
		void DiagramToControl(Size dSize, out Size cSize);

		/// <summary>
		/// Calculate diagram coordinates from control coordinates
		/// </summary>
		/// <param name="cX">X value in control coordinates</param>
		/// <param name="cY">Y value in control coordinates</param>
		/// <param name="dX">X value in diagram coordinates</param>
		/// <param name="dY">Y value in diagram coordinates</param>
		void ControlToDiagram(int cX, int cY, out int dX, out int dY);

		/// <summary>
		/// Calculate diagram coordinates from control coordinates
		/// </summary>
		/// <param name="cPt">Point in control coordinates</param>
		/// <param name="dPt">Point in diagram coordinates</param>
		void ControlToDiagram(Point cPt, out Point dPt);

		/// <summary>
		/// Calculate diagram coordinates from control coordinates
		/// </summary>
		/// <param name="cRect">Rectangle in control coordinates</param>
		/// <param name="dRect">Rectangle in diagram coordinates</param>
		void ControlToDiagram(Rectangle cRect, out Rectangle dRect);

		/// <summary>
		/// Calculate diagram coordinates from control coordinates
		/// </summary>
		/// <param name="cSize">Size in control coordinates</param>
		/// <param name="dSize">Size in diagram coordinates</param>
		void ControlToDiagram(Size cSize, out Size dSize);

		/// <summary>
		/// Calculate diagram coordinates from control coordinates
		/// </summary>
		/// <param name="cDistance">Distance in control coordinates</param>
		/// <param name="dDistance">Distance in diagram coordinates</param>
		void ControlToDiagram(int cDistance, out int dDistance);

		/// <summary>
		/// Calculate diagram coorinates from screen coordinates
		/// </summary>
		/// <param name="sPt">Point in Screen coordinates</param>
		/// <param name="iPt">Point in diagram coordinates</param>
		void ScreenToDiagram(Point sPt, out Point dPt);

		/// <summary>
		/// Calculate diagram coorinates from screen coordinates
		/// </summary>
		/// <param name="sRect">Rectangle in screen coordinates</param>
		/// <param name="dRect">Rectangle in diagram coordinates</param>
		void ScreenToDiagram(Rectangle sRect, out Rectangle dRect);

		#endregion


		#region Methods: Drawing and Invalidating

		void ResetTransformation();

		void RestoreTransformation();

		void DrawShape(Shape shape);

		void DrawShapes(IEnumerable<Shape> shapes);

		void DrawShapeOutline(IndicatorDrawMode drawMode, Shape shape);

		void DrawConnectionPoint(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId);

		void DrawRotateGrip(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId);

		void DrawResizeGrip(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId);

		void DrawCaptionBounds(IndicatorDrawMode drawMode, ICaptionedShape shape, int captionIndex);

		void DrawSnapIndicators(Shape shape);

		void DrawSelectionFrame(Rectangle frameRect);

		void DrawAnglePreview(Point center, int radius, Point mousePos, int cursorId, int originalAngle, int newAngle);

		void DrawLine(Point a, Point b);

		void InvalidateDiagram(Rectangle rect);

		void InvalidateDiagram(int right, int top, int width, int height);

		void InvalidateGrips(Shape shape, ControlPointCapabilities controlPointCapability);

		void InvalidateGrips(IEnumerable<Shape> shapes, ControlPointCapabilities controlPointCapability);

		void InvalidateSnapIndicators(Shape preview);

		void SuspendUpdate();

		void ResumeUpdate();

		#endregion


		#region Methods

		void OpenCaptionEditor(ICaptionedShape shape, int x, int y);

		void OpenCaptionEditor(ICaptionedShape shape, int labelIndex);

		void OpenCaptionEditor(ICaptionedShape shape, int labelIndex, string newText);

		/// <summary>
		/// Ensures that the given point is visible. 
		/// If the given point is outside the displayed area, the diagram will be scrolled.
		/// </summary>
		void EnsureVisible(int x, int y);

		/// <summary>
		/// Ensures that the given shape is completely visible. 
		/// If the given shape is outside the displayed area, the diagram will be scrolled and/or zoomed.
		/// </summary>
		void EnsureVisible(Shape shape);

		/// <summary>
		/// Ensures that the given area is visible. 
		/// If the given area is outside the displayed area, the diagram will be scrolled and/or zoomed.
		/// </summary>
		void EnsureVisible(Rectangle area);
		
		/// <summary>
		/// Sets a previously registered cursor.
		/// </summary>
		/// <param name="cursor"></param>
		void SetCursor(int cursorId);

		void SetLayerVisibility(LayerIds layerIds, bool visible);

		void SetLayerActive(LayerIds layerIds, bool active);

		bool IsLayerVisible(LayerIds layerId);

		bool IsLayerActive(LayerIds layerId);

		#endregion

	}

}