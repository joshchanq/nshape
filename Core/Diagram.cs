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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Dataweb.NShape.Advanced;
using Dataweb.Utilities;


namespace Dataweb.NShape {

	/// <summary>
	/// Describes export image formats.
	/// </summary>
	public enum nShapeImageFormat {
		Bmp,
		Emf,
		Gif,
		Jpeg,
		Png,
		Tiff,
		EmfPlus,
		Svg
	}


	#region Layers

	/// <summary>
	/// Describes the layers a shape is part of.
	/// </summary>
	[Flags]
	public enum LayerIds {
		None = 0x0,
		Layer01 = 0x1,
		Layer02 = 0x2,
		Layer03 = 0x4,
		Layer04 = 0x8,
		Layer05 = 0x10,
		Layer06 = 0x20,
		Layer07 = 0x40,
		Layer08 = 0x80,
		Layer09 = 0x100,
		Layer10 = 0x200,
		Layer11 = 0x400,
		Layer12 = 0x800,
		Layer13 = 0x1000,
		Layer14 = 0x2000,
		Layer15 = 0x4000,
		Layer16 = 0x8000,
		Layer17 = 0x10000,
		Layer18 = 0x20000,
		Layer19 = 0x40000,
		Layer20 = 0x80000,
		Layer21 = 0x100000,
		Layer22 = 0x200000,
		Layer23 = 0x400000,
		Layer24 = 0x800000,
		Layer25 = 0x1000000,
		Layer26 = 0x2000000,
		Layer27 = 0x4000000,
		Layer28 = 0x8000000,
		Layer29 = 0x10000000,
		Layer30 = 0x20000000,
		Layer31 = 0x40000000,
		All = int.MinValue
	}


	/// <summary>
	/// Groups shapes.
	/// </summary>
	/// <status>reviewed</status>
	public class Layer {

		public Layer(string name) {
			this.name = name;
		}


		public LayerIds Id {
			get { return id; }
			internal set { id = value; }
		}


		public string Name {
			get { return name; }
			internal set { name = value; }
		}


		public string Title {
			get {
				if (title == string.Empty) return name;
				else return title; 
			}
			set {
				if (value == null) throw new ArgumentNullException("Title");
				if (title == name) title = string.Empty;
				else title = value;
			}
		}


		public int LowerZoomThreshold {
			get { return lowerZoomThreshold; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("LowerZoomThreshold");
				lowerZoomThreshold = value;
			}
		}


		public int UpperZoomThreshold {
			get { return upperZoomThreshold; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("UpperZoomThreshold");
				upperZoomThreshold = value;
			}
		}


		#region Fields

		private LayerIds id = LayerIds.None;
		private string name = "";
		private string title = "";
		private int lowerZoomThreshold = 0;
		private int upperZoomThreshold = 5000;

		#endregion
	}


	/// <summary>
	/// Editable collection of layers
	/// </summary>
	/// <status>reviewed</status>
	public interface ILayerCollection : ICollection<Layer> {

		Layer GetLayer(LayerIds layerId);

		IEnumerable<Layer> GetLayers(LayerIds layerIds);		

		Layer FindLayer(string name);

		bool RenameLayer(string previousName, string newName);
	}


	/// <summary>
	/// Holds a list of layers.
	/// </summary>
	internal class LayerCollection : ILayerCollection {

		internal LayerCollection(Diagram diagram) {
			this.diagram = diagram;
			// create an entry for each layer so that the layer can be addressed directly
			foreach (LayerIds layerId in Enum.GetValues(typeof(LayerIds))) {
				if (layerId == LayerIds.None || layerId == LayerIds.All) continue;
				layers.Add(null);
			}
		}


		#region ILayerCollection Members

		public Layer FindLayer(string name) {
			if (name == null) throw new ArgumentNullException("name");
			int cnt = layers.Count;
			for (int i = 0; i < cnt; ++i) {
				if (layers[i] != null && layers[i].Name == name)
					return layers[i];
			}
			return null;
		}


		public Layer GetLayer(LayerIds layerId) {
			int layerBit = GetLowestLayerBit(layerId);
			if (layerBit == -1)
				throw new nShapeException("{0} is not a valid {1} to find.", layerId, typeof(LayerIds));
			return layers[layerBit];
		}


		public IEnumerable<Layer> GetLayers(LayerIds layerId) {
			foreach (int layerBit in GetLayerBits(layerId)) {
				if (layerBit == -1) continue;
				Debug.Assert(layers[layerBit] != null);
				yield return layers[layerBit];
			}
		}


		public bool RenameLayer(string previousName, string newName) {
			if (previousName == null) throw new ArgumentNullException("previousName");
			if (newName == null) throw new ArgumentNullException("newName");
			Layer layer = FindLayer(newName);
			if (layer != null) return false;
			else {
				layer.Name = newName;
				return true;
			}
		}

		#endregion


		#region ICollection<Layer> Members

		public void Add(Layer item) {
			if (item == null) throw new ArgumentNullException("item");
			for (int i = 0; i < layers.Count; ++i) {
				if (layers[i] == null) {
					item.Id = (LayerIds)Math.Pow(2, i);
					layers[i] = item;
					++layerCount;
					break;
				}
			}
			Debug.Assert(item.Id != LayerIds.None);
		}

		public void Clear() {
			for (int i = 0; i < layers.Count; ++i)
				layers[i] = null;
			layerCount = 0;
		}

		public bool Contains(Layer item) {
			return layers.Contains(item);
		}

		public void CopyTo(Layer[] array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException("array");
			layers.CopyTo(array, arrayIndex);
		}

		public int Count {
			get { return layerCount; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool Remove(Layer item) {
			if (item == null) throw new ArgumentNullException("item");
			int layerBit = GetLowestLayerBit(item.Id);
			if (layerBit == -1) return false;
			layers[layerBit] = null;
			return true;
		}

		#endregion


		#region IEnumerable<Layer> Members

		public IEnumerator<Layer> GetEnumerator() {
			return Enumerator.Create(layers);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return layers.GetEnumerator();
		}

		#endregion


		#region Methods (private)

		private int GetLowestLayerBit(LayerIds layerId) {
			const int errResult = -1;
			if (layerId == LayerIds.None) return errResult;
			int bitNo = 0;
			foreach (LayerIds id in Enum.GetValues(typeof(LayerIds))) {
				if ((layerId & id) == layerId)
					return bitNo;
				else if ((layerId & id) != 0)
					throw new ArgumentException("Argument 'layerId' must not be a combination of {1}.", typeof(LayerIds).FullName);
				++bitNo;
			}
			return errResult;
		}


		private IEnumerable<int> GetLayerBits(LayerIds layerIds) {
			if (layerIds == LayerIds.None) yield break;
			int bitNo = 0;
			foreach (LayerIds id in Enum.GetValues(typeof(LayerIds))) {
				if (id == LayerIds.None) continue;
				if ((layerIds & id) != 0) yield return bitNo;
				++bitNo;
			}
		}

		#endregion


		private struct Enumerator : IEnumerator<Layer>, IEnumerator {

			public static Enumerator Create(List<Layer> layerList) {
			if (layerList == null) throw new ArgumentNullException("layerList");
				Enumerator result = Enumerator.Empty;
				result.layerList = layerList;
				result.layerCount = layerList.Count;
				result.currentIdx = -1;
				result.currentLayer = null;
				return result;
			}


			public static readonly Enumerator Empty;


			public Enumerator(List<Layer> layerList) {
				if (layerList == null) throw new ArgumentNullException("layerList");
				this.layerList = layerList;
				this.layerCount = layerList.Count;
				this.currentIdx = -1;
				this.currentLayer = null;
			}


			#region IEnumerator<Layer> Members

			public Layer Current { get { return currentLayer; } }

			#endregion


			#region IDisposable Members

			public void Dispose() {
				// nothing to do
			}

			#endregion


			#region IEnumerator Members

			object IEnumerator.Current { get { return currentLayer; } }

			public bool MoveNext() {
				bool result = false;
				currentLayer = null;
				while (currentIdx < layerCount - 1 && !result) {
					currentLayer = layerList[++currentIdx];
					if (currentLayer != null) result = true;
				}
				return result;
			}

			public void Reset() {
				currentIdx = -1;
				currentLayer = null;
			}

			#endregion


			static Enumerator() {
				Empty.layerList = null;
				Empty.layerCount = 0;
				Empty.currentIdx = -1;
				Empty.currentLayer = null;
			}

			#region Fields
			private List<Layer> layerList;
			private int layerCount;
			private int currentIdx;
			private Layer currentLayer;
			#endregion
		}


		#region Fields
		private List<Layer> layers = new List<Layer>(31);
		private int layerCount = 0;
		private Diagram diagram = null;
		#endregion
	}

	#endregion


	/// <summary>
	/// Read/write shape collection owned by a diagram.
	/// </summary>
	internal class DiagramShapeCollection : ShapeCollection {

		public override void NotifyChildMoving(Shape shape) {
			base.NotifyChildMoving(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			++shapeCounter;
		}


		public override void NotifyChildMoved(Shape shape) {
			base.NotifyChildMoved(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			--shapeCounter;
			if (shapeCounter == 0) DoUpdateOwnerBounds();
		}


		public override void NotifyChildResizing(Shape shape) {
			base.NotifyChildResizing(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			++shapeCounter;
		}
		
		
		public override void NotifyChildResized(Shape shape) {
			base.NotifyChildResized(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			--shapeCounter;
			if (shapeCounter == 0) DoUpdateOwnerBounds();
		}


		public override void NotifyChildRotating(Shape shape) {
			base.NotifyChildRotating(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			++shapeCounter;
		}


		public override void NotifyChildRotated(Shape shape) {
			base.NotifyChildRotated(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			--shapeCounter;
			if (shapeCounter == 0) DoUpdateOwnerBounds();
		}


		internal DiagramShapeCollection(Diagram owner)
			: this(owner, 1000) {
		}


		internal DiagramShapeCollection(Diagram owner, int capacity)
			: base(capacity) {
			if (owner == null) throw new ArgumentNullException("owner");
			this.owner = owner;
		}


		internal DiagramShapeCollection(Diagram owner, IEnumerable<Shape> collection)
			: this(owner, (collection is ICollection) ? ((ICollection)collection).Count : 0) {
			AddRangeCore(collection);
		}


		internal Diagram Owner {
			get { return owner; }
		}


		protected override void AddRangeCore(IEnumerable<Shape> collection) {
			if (collection is ICollection) shapeCounter = ((ICollection)collection).Count;
			else foreach (Shape s in collection) ++shapeCounter;
			base.AddRangeCore(collection);
		}


		protected override bool RemoveRangeCore(IEnumerable<Shape> collection) {
			if (collection is ICollection) shapeCounter = ((ICollection)collection).Count;
			else foreach (Shape s in collection) ++shapeCounter;
			return base.RemoveRangeCore(collection);
		}


		protected override void ReplaceRangeCore(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			if (oldShapes is ICollection) shapeCounter = ((ICollection)oldShapes).Count;
			else foreach (Shape s in oldShapes) ++shapeCounter;
			base.ReplaceRangeCore(oldShapes, newShapes);
		}
		
		
		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			shape.Diagram = owner;
			shape.DisplayService = owner.DisplayService;
			shape.Invalidate();

			CheckOwnerboundsUpdateNeeded(shape);
			if (shapeCounter > 0) --shapeCounter;
			if (shapeCounter == 0) DoUpdateOwnerBounds();

			return result;
		}


		protected override void ReplaceCore(Shape oldShape, Shape newShape) {
			base.ReplaceCore(oldShape, newShape);
			oldShape.Diagram = null;
			oldShape.Invalidate();
			oldShape.DisplayService = null;
			newShape.Diagram = owner;
			newShape.DisplayService = owner.DisplayService;
			newShape.Invalidate();

			CheckOwnerboundsUpdateNeeded(oldShape);
			CheckOwnerboundsUpdateNeeded(newShape);
			if (shapeCounter > 0) --shapeCounter;
			if (shapeCounter == 0) DoUpdateOwnerBounds();
		}


		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			shape.Invalidate();
			shape.DisplayService = null;
			shape.Diagram = null;

			CheckOwnerboundsUpdateNeeded(shape);
			if (shapeCounter > 0) --shapeCounter;
			if (shapeCounter == 0) DoUpdateOwnerBounds();

			return result;
		}


		protected override void ClearCore() {
			for (int i = shapes.Count - 1; i >= 0; --i) {
				CheckOwnerboundsUpdateNeeded(shapes[i]);
				shapes[i].Invalidate();
				shapes[i].DisplayService = null;
			}
			base.ClearCore();
			DoUpdateOwnerBounds();
		}


		private void CheckOwnerboundsUpdateNeeded(Shape shape) {
			if (!ownerBoundsUpdateNeeded) {
				Rectangle shapeBounds = shape.GetBoundingRectangle(true);
				if (shapeBounds.Left < 0 || owner.Width < shapeBounds.Right
					|| shapeBounds.Top < 0 || owner.Height < shapeBounds.Bottom)
					ownerBoundsUpdateNeeded = true;
			}
		}


		private void DoUpdateOwnerBounds() {
			Debug.Assert(shapeCounter == 0);
			if (ownerBoundsUpdateNeeded) {
				if (owner != null) owner.NotifyBoundsChanged();
				ownerBoundsUpdateNeeded = false;
			}
		}


		#region Fields
		private Diagram owner = null;
		private int shapeCounter = 0;
		private bool ownerBoundsUpdateNeeded;
		#endregion
	}


	/// <summary>
	/// Displays shapes in layers.
	/// </summary>
	public sealed class Diagram : IEntity {

		public Diagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			this.name = name;
			diagramShapes = new DiagramShapeCollection(this, expectedShapes);
			layers = new LayerCollection(this);
			Width = 800;
			Height = 600;
			// A new diagram has no layers.
		}


		#region [Public] Properties

		/// <summary>
		/// Culture invariant name.
		/// </summary>
		[Category("Identification"),
		Description("The name of the diagram.")]
		public string Name {
			get { return name; }
			set { name = value ?? string.Empty; }
		}


		/// <summary>
		/// Culture depending title.
		/// </summary>
		[Category("Identification"),
		Description("The displayed text of the diagram.")]
		public string Title {
			get { return title; }
			set { title = value; }
		}


		/// <summary>
		/// Width of diagram in pixels.
		/// </summary>
		[Category("Layout"),
		Description("The width of the diagram.")]
		public int Width {
			get { return size.Width; }
			set {
				if (displayService != null)
					displayService.Invalidate(0, 0, Width, Height);
				if (value <= 0) size.Width = 1;
				else size.Width = value;
				if (displayService != null) 
					displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Height of diagram in pixels.
		/// </summary>
		[Category("Layout"),
		Description("The height of the diagram.")]
		public int Height {
			get { return size.Height; }
			set {
				if (displayService != null)
					displayService.Invalidate(0, 0, Width, Height);
				if (value <= 0) size.Height = 1;
				else size.Height = value;
				if (displayService != null) 
					displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Size of diagram in pixels.
		/// </summary>
		[Browsable(false)]
		public Size Size {
			get { return size; }
			set {
				if (displayService != null)
					displayService.Invalidate(0, 0, Width, Height);
				size = value;
				if (displayService != null)
					displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Background color of the diagram.
		/// </summary>
		[Category("Appearance"),
		Description("The background color of the diagram.")]
		public Color BackgroundColor {
			get { return backColor; }
			set { 
				backColor = value;
				if (colorBrush != null) {
					colorBrush.Dispose();
					colorBrush = null;
				}
				if (displayService != null)
					displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Second color of background gradient.
		/// </summary>
		[Category("Appearance")]
		[Description("The second color of the diagram's color gradient.")]
		public Color BackgroundGradientColor {
			get { return targetColor; }
			set { 
				targetColor = value;
				if (colorBrush != null) {
					colorBrush.Dispose();
					colorBrush = null;
				}
				if (displayService != null)
					displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Background image of diagram.
		/// </summary>
		[Category("Appearance")]
		[Description("The background image of the diagram.")]
		[Editor("Dataweb.nShape.WinFormsUI.nShapeNamedImageEditor, Dataweb.nShape.WinFormsUI", typeof(UITypeEditor))]
		public NamedImage BackgroundImage {
			get { return backImage; }
			set {
				backImage = value;
				InvalidateDrawCache();
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Image layout of background image.
		/// </summary>
		[Category("Appearance"),
		Description("The display mode of the diagram's background image.")]
		public nShapeImageLayout BackgroundImageLayout {
			get { return imageLayout; }
			set { 
				imageLayout = value;
				InvalidateDrawCache();
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		[Category("Appearance"),
		Description("Gamma correction for the diagram's background image.")]
		public float BackgroundImageGamma {
			get { return imageGamma; }
			set {
				if (value <= 0) throw new ArgumentOutOfRangeException("Value has to be greater 0.");
				imageGamma = value;
				InvalidateDrawCache();
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		[Category("Appearance"),
		Description("Specifies if the diagram's background image is drawn as gray scale image instead.")]
		public bool BackgroundImageGrayScale {
			get { return imageGrayScale; }
			set {
				imageGrayScale = value;
				InvalidateDrawCache();
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		[Category("Appearance"),
		Description("Specifies the transparency in percentage for the diagram's background image.")]
		public byte BackgroundImageTransparency {
			get { return imageTransparency; }
			set {
				if (value < 0 || value > 100) throw new ArgumentOutOfRangeException("The value has to be between 0 and 100.");
				imageTransparency = value;
				InvalidateDrawCache();
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		[Category("Appearance"),
		Description("Specifies the transparent color for the diagram's background image.")]
		public Color BackgroundImageTransparentColor {
			get { return imageTransparentColor; }
			set {
				imageTransparentColor = value;
				InvalidateDrawCache();
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Specifies the display service to use for this diagram.
		/// </summary>
		[Browsable(false)]
		public IDisplayService DisplayService {
			get { return displayService; }
			set {
				if (displayService != value) {
					displayService = value;
					diagramShapes.SetDisplayService(displayService);
				}
				if (displayService != null) displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Specifies whether the diagram is to render in high quality.
		/// </summary>
		public bool HighQualityRendering {
			get { return highQualityRendering; }
			set {
				highQualityRendering = value;
				if (colorBrush != null) {
					colorBrush.Dispose();
					colorBrush = null;
				}
			}
		}
		
		
		/// <summary>
		/// Provides access to the diagram layers.
		/// </summary>
		public ILayerCollection Layers {
		   get { return layers; }
		}


		/// <summary>
		/// Provides access to the diagram shapes.
		/// </summary>
		public IShapeCollection Shapes {
			get { return diagramShapes; }
		}

		#endregion


		#region [Public] Methods: Layer management

		public LayerIds GetShapeLayers(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return shape.Layers;
		}
		

		public void AddShapeToLayers(Shape shape, LayerIds layerIds) {
			if (shape == null) throw new ArgumentNullException("shape");
			shape.Layers |= layerIds;
		}


		public void AddShapesToLayers(IEnumerable<Shape> shapes, LayerIds layerIds) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			foreach (Shape shape in shapes)
				shape.Layers |= layerIds;
		}


		public void RemoveShapeFromLayers(Shape shape, LayerIds layerIds) {
			if (shape == null) throw new ArgumentNullException("shape");
			shape.Layers ^= (shape.Layers & layerIds);
			shape.Invalidate();
		}


		public void RemoveShapesFromLayers(IEnumerable<Shape> shapes, LayerIds layerIds) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			foreach (Shape shape in shapes) {
				shape.Layers ^= (shape.Layers & layerIds);
				shape.Invalidate();
			}
		}


		public void Clear() {
			diagramShapes.Clear();
			layers.Clear();
		}

		#endregion


		#region [Public] Methods: Drawing and painting

		/// <summary>
		/// Exports the contents of the diagram to an image of the given format.
		/// </summary>
		public Image CreateImage(nShapeImageFormat imageFormat) {
			return CreateImage(imageFormat, null, 0, false, Color.White, (int)DisplayService.InfoGraphics.DpiY);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to export the whole diagram area.
		/// </summary>
		public Image CreateImage(nShapeImageFormat imageFormat, IEnumerable<Shape> shapes) {
			return CreateImage(imageFormat, shapes, 0, false, Color.White, (int)DisplayService.InfoGraphics.DpiY);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		public Image CreateImage(nShapeImageFormat imageFormat, IEnumerable<Shape> shapes, bool withBackground) {
			return CreateImage(imageFormat, shapes, 0, withBackground, Color.White, (int)DisplayService.InfoGraphics.DpiY);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		public Image CreateImage(nShapeImageFormat imageFormat, IEnumerable<Shape> shapes, int margin) {
			return CreateImage(imageFormat, shapes, margin, false, Color.White, (int)DisplayService.InfoGraphics.DpiY);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes (plus margin on each side) to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		public Image CreateImage(nShapeImageFormat imageFormat, IEnumerable<Shape> shapes, int margin, bool withBackground, Color backgroundColor) {
			return CreateImage(imageFormat, shapes, margin, withBackground, backgroundColor, (int)DisplayService.InfoGraphics.DpiY);
		}
		
		
		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes (plus margin on each side) to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		public Image CreateImage(nShapeImageFormat imageFormat, IEnumerable<Shape> shapes, int margin, bool withBackground, Color backgroundColor, int dpi) {
			Image result = null;
			
			// get bounding rectangle around the given shapes
			Rectangle imageBounds = Rectangle.Empty;
			if (shapes == null) {
				imageBounds.X = imageBounds.Y = 0;
				imageBounds.Width = Width;
				imageBounds.Height = Height;
			} else {
				if (margin == 0) margin = 1;
				int left, top, right, bottom;
				left = top = int.MaxValue;
				right = bottom = int.MinValue;
				// Calculate the bounding rectangle of the given shapes
				Rectangle boundingRect = Rectangle.Empty;
				foreach (Shape shape in shapes) {
					boundingRect = shape.GetBoundingRectangle(true);
					if (boundingRect.Left < left) left = boundingRect.Left;
					if (boundingRect.Top < top) top = boundingRect.Top;
					if (boundingRect.Right > right) right = boundingRect.Right;
					if (boundingRect.Bottom > bottom) bottom = boundingRect.Bottom;
				}
				imageBounds = Rectangle.FromLTRB(left, top, right, bottom);
				imageBounds.Inflate(margin, margin);
			}

			bool originalQualitySetting = this.HighQualityRendering;
			HighQualityRendering = true;
			UpdateBrushes();

			float scaleX = 1, scaleY = 1;
			switch (imageFormat) {
				case nShapeImageFormat.Svg:
					throw new NotImplementedException("Not yet implemented.");

				case nShapeImageFormat.Emf:
				case nShapeImageFormat.EmfPlus:
					// Create MetaFile and graphics context
					IntPtr hdc = DisplayService.InfoGraphics.GetHdc();
					try {
						Rectangle bounds = Rectangle.Empty;
						bounds.Size = imageBounds.Size;
						result = new Metafile(hdc, bounds, MetafileFrameUnit.Pixel,
							imageFormat == nShapeImageFormat.Emf ? EmfType.EmfOnly : EmfType.EmfPlusDual,
							Name);
					} finally {
						DisplayService.InfoGraphics.ReleaseHdc(hdc);
					}
					break;

				case nShapeImageFormat.Bmp:
				case nShapeImageFormat.Gif:
				case nShapeImageFormat.Jpeg:
				case nShapeImageFormat.Png:
				case nShapeImageFormat.Tiff:
					if (dpi > 0 && dpi != DisplayService.InfoGraphics.DpiX && dpi != DisplayService.InfoGraphics.DpiY) {
						scaleX = dpi / DisplayService.InfoGraphics.DpiX;
						scaleY = dpi / DisplayService.InfoGraphics.DpiY;
						//imageBounds.X = (int)Math.Round(scaleX * imageBounds.X);
						//imageBounds.Y = (int)Math.Round(scaleY * imageBounds.Y);
						//imageBounds.Width = (int)Math.Round(scaleX * imageBounds.Width);
						//imageBounds.Height = (int)Math.Round(scaleY * imageBounds.Height);
						//result = new Bitmap(imageBounds.Width, imageBounds.Height);
						result = new Bitmap((int)Math.Round(scaleX * imageBounds.Width), 
							(int)Math.Round(scaleY * imageBounds.Height));
						((Bitmap)result).SetResolution(dpi, dpi);
					} else result = new Bitmap(imageBounds.Width, imageBounds.Height);
					break;

				default:
					throw new nShapeUnsupportedValueException(typeof(nShapeImageFormat), imageFormat);
			}

			// Draw diagram
			using (Graphics gfx = Graphics.FromImage(result)) {
				GdiHelpers.ApplyGraphicsSettings(gfx, nShapeRenderingQuality.MaximumQuality);

				// Fill background with background color
				if (backgroundColor.A < 255) {
					// For image formats that do not support transparency, fill background with the RGB part of 
					// the given backgropund color
					if (imageFormat == nShapeImageFormat.Bmp || imageFormat == nShapeImageFormat.Jpeg)
						gfx.Clear(Color.FromArgb(255, backgroundColor));
					else if (backgroundColor.A > 0) gfx.Clear(backgroundColor);
					// Skip filling background for meta files if transparency is 100%: 
					// Filling Background with Color.Transparent causes graphical glitches with many applications
				} else gfx.Clear(backgroundColor);

				// Transform graphics (if necessary)
				gfx.TranslateTransform(-imageBounds.X, -imageBounds.Y, MatrixOrder.Prepend);
				if (scaleX != 1 || scaleY != 1) gfx.ScaleTransform(scaleX, scaleY, MatrixOrder.Append);

				// Draw diagram background
				if (withBackground) DrawBackground(gfx, imageBounds);
				// Draw diagram shapes
				if (shapes == null) {
					foreach (Shape shape in diagramShapes.BottomUp) shape.Draw(gfx);
				} else {
					ShapeCollection shapeCollection = new ShapeCollection(shapes);
					foreach (Shape shape in shapeCollection.BottomUp) shape.Draw(gfx);
					shapeCollection.Clear();
				}
				// Reset transformation
				gfx.ResetTransform();
			}
			
			HighQualityRendering = originalQualitySetting;
			UpdateBrushes();

			return result;
		}


		/// <summary>
		/// Draws the diagram background.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="clipRectangle"></param>
		public void DrawBackground(Graphics graphics, Rectangle clipRectangle) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			Rectangle bounds = Rectangle.Empty;
			bounds.X = Math.Max(0, clipRectangle.X);
			bounds.Y = Math.Max(0, clipRectangle.Y);
			bounds.Width = Math.Min(clipRectangle.Right, Width) - bounds.X;
			bounds.Height = Math.Min(clipRectangle.Bottom, Height) - bounds.Y;

			//if (clipRectangle.Width > Width)
			//   clipRectangle.Width -= (clipRectangle.Width - Width);
			//if (clipRectangle.Height > Height)
			//   clipRectangle.Height -= (clipRectangle.Height - Height);

			// draw diagram background color
			UpdateBrushes();
			//graphics.FillRectangle(colorBrush, clipRectangle);
			graphics.FillRectangle(colorBrush, bounds);

			// draw diagram background image
			if (backImage != null && backImage.Image != null) {
				Rectangle diagramBounds = Rectangle.Empty;
				diagramBounds.Width = Width;
				diagramBounds.Height = Height;
				if (imageAttribs == null) imageAttribs = GdiHelpers.GetImageAttributes(imageLayout, imageGamma, imageTransparency, imageGrayScale, false, imageTransparentColor);
				if (backImage.Image is Metafile)
					GdiHelpers.DrawImage(graphics, backImage.Image, imageAttribs, imageLayout, diagramBounds, bounds);
				else {
					if (imageBrush == null) imageBrush = GdiHelpers.CreateTextureBrush(backImage.Image, imageAttribs);
					Point center = Point.Empty;
					center.Offset(diagramBounds.Width / 2, diagramBounds.Height / 2);
					GdiHelpers.TransformTextureBrush(imageBrush, imageLayout, diagramBounds, center, 0);
					graphics.FillRectangle(imageBrush, bounds);
				}
			}
		}


		/// <summary>
		/// Draws the diagram shapes.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="layers"></param>
		/// <param name="clipRectangle"></param>
		public void DrawShapes(Graphics graphics, LayerIds layers, Rectangle clipRectangle) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			int x = clipRectangle.X;
			int y = clipRectangle.Y;
			int width = clipRectangle.Width;
			int height = clipRectangle.Height;
			
			foreach (Shape shape in diagramShapes.BottomUp) {
				// paint shape if it intersects with the clipping area
				if ((shape.Layers == LayerIds.None || (shape.Layers & layers) > 0)
					&& Geometry.RectangleIntersectsWithRectangle(shape.GetBoundingRectangle(false), x, y, width, height)) {
					shape.Draw(graphics);
					
					//               foreach (ShapeConnectionInfo ci in shape.GetConnectionInfos(null, ControlPointId.NotSupported)) {
					//                  if (ci.ConnectionPointId == ControlPointId.Reference && ci.PassiveShape is ILinearShape) {
					//                     Point p = Point.Empty;
					//                     int ptSize = 2;
					//                     p = shape.GetControlPointPosition(ci.GluePointId);
					//                     graphics.FillEllipse(Brushes.Black, p.X - ptSize, p.Y - ptSize, ptSize + ptSize, ptSize + ptSize);
					//                  }
					//#if DEBUG
					//                  else if (ci.GluePointId != ControlPointId.Reference) {
					//                     Point p = Point.Empty;
					//                     int ptSize = 2;
					//                     p = shape.GetControlPointPosition(ci.GluePointId);
					//                     graphics.FillEllipse(Brushes.White, p.X - ptSize, p.Y - ptSize, ptSize + ptSize, ptSize + ptSize);
					//                     graphics.DrawEllipse(Pens.Black, p.X - ptSize, p.Y - ptSize, ptSize + ptSize, ptSize + ptSize);
					//                  }
					//#endif
					//               }
				} //else graphics.DrawRectangle(Pens.Red, shape.GetBoundingRectangle(false));
			}
		}
		
		#endregion


		internal void NotifyBoundsChanged() {
			if (displayService != null) displayService.NotifyBoundsChanged();
		}
		
		
		#region IEntity Members

		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("Width", typeof(int));
			yield return new EntityFieldDefinition("Height", typeof(int));
			yield return new EntityFieldDefinition("BackgroundColor", typeof(Color));
			yield return new EntityFieldDefinition("BackgroundGradientEndColor", typeof(Color));
			yield return new EntityFieldDefinition("BackgroundImageFileName", typeof(string));
			yield return new EntityFieldDefinition("BackgroundImage", typeof(Image));
			yield return new EntityFieldDefinition("ImageLayout", typeof(byte));
			yield return new EntityFieldDefinition("ImageGamma", typeof(float));
			yield return new EntityFieldDefinition("ImageTransparency", typeof(byte));
			yield return new EntityFieldDefinition("ImageGrayScale", typeof(bool));
			yield return new EntityFieldDefinition("ImageTransparentColor", typeof(int));

			yield return new EntityInnerObjectsDefinition("Layers", "Core.Layer",
				new string[] { "Id", "Name", "Title", "LowerVisibilityThreshold", "UpperVisibilityThreshold" },
				new Type[] { typeof(int), typeof(string), typeof(string), typeof(int), typeof(int) });
		}


		[Category("Identification")]
		object IEntity.Id {
			get { return id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null)
				throw new ArgumentNullException("id");
			if (this.id != null)
				throw new InvalidOperationException(string.Format("{0} has already a id.", GetType().Name));
			this.id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			name = reader.ReadString();
			size.Width = reader.ReadInt32();
			size.Height = reader.ReadInt32();
			backColor = Color.FromArgb(reader.ReadInt32());
			targetColor = Color.FromArgb(reader.ReadInt32());
			backImage.Name = reader.ReadString();
			backImage.Image = reader.ReadImage();
			imageLayout = (nShapeImageLayout)reader.ReadByte();
			imageGamma = reader.ReadFloat();
			imageTransparency = reader.ReadByte();
			imageGrayScale = reader.ReadBool();
			imageTransparentColor = Color.FromArgb(reader.ReadInt32());
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			Debug.Assert(propertyName == "Layers");
			Debug.Assert(layers.Count == 0);
			reader.BeginReadInnerObjects();
			while (reader.BeginReadInnerObject()) {
				int id = reader.ReadInt32();
				string name = reader.ReadString();
				Layer l = new Layer(name);
				l.Id = (LayerIds)id;
				l.Title = reader.ReadString();
				l.LowerZoomThreshold = reader.ReadInt32();
				l.UpperZoomThreshold = reader.ReadInt32();
				reader.EndReadInnerObject();
				layers.Add(l);
			}
			reader.EndReadInnerObjects();
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteString(name);
			writer.WriteInt32(size.Width);
			writer.WriteInt32(size.Height);
			writer.WriteInt32(BackgroundColor.ToArgb());
			writer.WriteInt32(BackgroundGradientColor.ToArgb());
			writer.WriteString(backImage.Name);
			writer.WriteImage(backImage.Image);
			writer.WriteByte((byte)imageLayout);
			writer.WriteFloat(imageGamma);
			writer.WriteByte(imageTransparency);
			writer.WriteBool(imageGrayScale);
			writer.WriteInt32(imageTransparentColor.ToArgb());
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			Debug.Assert(propertyName == "Layers");
			writer.BeginWriteInnerObjects();
			foreach (Layer l in layers) {
				writer.BeginWriteInnerObject();
				writer.WriteInt32((int)l.Id);
				writer.WriteString(l.Name);
				writer.WriteString(l.Title);
				writer.WriteInt32(l.LowerZoomThreshold);
				writer.WriteInt32(l.UpperZoomThreshold);
				writer.EndWriteInnerObject();
			}
			writer.EndWriteInnerObjects();
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		#region [Private] Methods

		private void InvalidateDrawCache() {
			if (imageAttribs != null) {
				imageAttribs.Dispose();
				imageAttribs = null;
			}
			if (imageBrush != null) {
				imageBrush.Dispose();
				imageBrush = null;
			}
		}

		
		private void UpdateBrushes() {
			if (colorBrush == null) {
				if (BackgroundGradientColor != BackgroundColor && highQualityRendering) {
					colorBrushBounds.Width = 100;
					colorBrushBounds.Height = 100;
					colorBrush = new LinearGradientBrush(colorBrushBounds, BackgroundGradientColor, BackgroundColor, 45);
				} else colorBrush = new SolidBrush(BackgroundColor);
			}
			if (colorBrush is LinearGradientBrush && Size != colorBrushBounds.Size) {
				LinearGradientBrush gradientBrush = (LinearGradientBrush)colorBrush;
				float widthSq = gradientBrush.Rectangle.Width * gradientBrush.Rectangle.Width;
				float heightSq = gradientBrush.Rectangle.Height * gradientBrush.Rectangle.Height;

				float dX; float dY;
				dX = (float)(Width / (Math.Sqrt(widthSq + widthSq) / 2));
				dY = (float)(Height / (Math.Sqrt(heightSq + heightSq) / 2));
				gradientBrush.ResetTransform();
				gradientBrush.ScaleTransform(dX, dY);
				gradientBrush.RotateTransform(45f);
				colorBrushBounds.Width = Width;
				colorBrushBounds.Height = Height;
			}
		}

		#endregion


		#region Fields

		public const int CellSize = 100;

		private const string entityTypeName = "Core.Diagram";
		private const int expectedShapes = 10000;

		private object id;
		private string title;
		private string name;
		private IDisplayService displayService;
		private LayerCollection layers = null;
		private DiagramShapeCollection diagramShapes = null;
		private Size size = new Size(1, 1);
		// Rendering stuff
		private Color backColor = Color.Silver;
		private Color targetColor = Color.WhiteSmoke;
		private bool highQualityRendering = true;
		// Background image stuff
		private NamedImage backImage = new NamedImage();
		private nShapeImageLayout imageLayout;
		private float imageGamma = 1.0f;
		private byte imageTransparency = 0;
		private bool imageGrayScale = false;
		private Color imageTransparentColor = Color.Empty;
		// Drawing and Painting stuff
		private Brush colorBrush = null;					// Brush for painting the diagram's background
		private ImageAttributes imageAttribs = null; // ImageAttributes for drawing the background image
		private TextureBrush imageBrush = null;		// Brush for painting the diagram's background image
		private Rectangle colorBrushBounds = Rectangle.Empty;
		
		// Buffers
		private List<Shape> shapeBuffer = new List<Shape>();

		#endregion
	}
}