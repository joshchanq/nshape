using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

using Dataweb.Utilities;


namespace Dataweb.nShape.Advanced {

	/// <summary>
	/// Abstract base class for shapes that draw themselves using a bitmap or
	/// meta file.
	/// </summary>
	public class ImageBasedShape : ShapeBase, IPlanarShape, ICaptionedShape {

		protected internal ImageBasedShape(ShapeType shapeType, Template template,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, template) {
			Construct(resourceBaseName, resourceAssembly);
		}


		protected internal ImageBasedShape(ShapeType shapeType, IStyleSet styleSet,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, styleSet) {
			Construct(resourceBaseName, resourceAssembly);
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			w = image.Width;
			h = image.Height;
			Fit(0, 0, 100, 100);
			charStyle = styleSet.CharacterStyles.Normal;
			paragraphStyle = styleSet.ParagraphStyles.Title;
			fillStyle = styleSet.FillStyles.Transparent;
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);

			if (source is IPlanarShape) {
				IPlanarShape src = (IPlanarShape)source;
				// Copy regular properties
				this.angle = src.Angle;
				// Copy templated properties
				this.fillStyle = (Template != null && src.FillStyle == ((ImageBasedShape)Template.Shape).FillStyle) ? null : src.FillStyle;
			}
			if (source is ICaptionedShape) {
				// Copy as many captions as possible. Leave the rest untouched.
				int ownCaptionCnt = CaptionCount;
				int srcCaptionCnt = ((ICaptionedShape)source).CaptionCount;
				int cnt = Math.Min(ownCaptionCnt, srcCaptionCnt);
				for (int i = 0; i < cnt; ++i) {
					this.SetCaptionText(i, ((ICaptionedShape)source).GetCaptionText(i));
					this.SetCaptionCharacterStyle(i, ((ICaptionedShape)source).GetCaptionCharacterStyle(i));
					this.SetCaptionParagraphStyle(i, ((ICaptionedShape)source).GetCaptionParagraphStyle(i));
				}
			}
			if (source is ImageBasedShape) {
				w = ((ImageBasedShape)source).w;
				h = ((ImageBasedShape)source).h;
				if (((ImageBasedShape)source).image != null)
					image = (Image)((ImageBasedShape)source).image.Clone();
			} else {
				Rectangle r = source.GetBoundingRectangle(true);
				Fit(r.X, r.Y, r.Width, r.Height);
			}
		}


		public override Shape Clone() {
			Shape result = new ImageBasedShape(Type, (Template)null, resourceName, resourceAssembly);
			result.CopyFrom(this);
			return result;
		}


		[Category("Data")]
		public string Text {
			get { return caption.Text; }
			set { caption.Text = value; }
		}


		[Category("Appearance")]
		public ICharacterStyle CharacterStyle {
			get { return charStyle ?? ((ImageBasedShape)Template.Shape).CharacterStyle; }
			set {
				charStyle = (Template != null && value == ((ImageBasedShape)Template.Shape).CharacterStyle) ? null : value;
				caption.InvalidatePath();
				Invalidate();
			}
		}


		[Category("Appearance")]
		public IParagraphStyle ParagraphStyle {
			get { return paragraphStyle ?? ((ImageBasedShape)Template.Shape).ParagraphStyle; }
			set {
				paragraphStyle = (Template != null && value == ((ImageBasedShape)Template.Shape).ParagraphStyle) ? null : value;
				caption.InvalidatePath();
				Invalidate();
			}
		}


		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			charStyle = styleSet.GetPreviewStyle(CharacterStyle);
			paragraphStyle = styleSet.GetPreviewStyle(ParagraphStyle);
			fillStyle = styleSet.GetPreviewStyle(FillStyle);
		}


		public override bool NotifyStyleChanged(IStyle style) {
			bool result = base.NotifyStyleChanged(style);
			if (style == null || IsStyleAffected(CharacterStyle, style) || IsStyleAffected(ParagraphStyle, style)) {
				Invalidate();
				UpdateDrawCache();
				Invalidate();
				result = true;
			}
			return result;
		}


		public override RelativePosition CalculateRelativePosition(int x, int y) {
			RelativePosition result = RelativePosition.Empty;
			result.A = 0;
			result.B = 0;
			return result;
		}


		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			Point result = Point.Empty;
			result.X = x;
			result.Y = y;
			return result;
		}


		public override Point CalculateNormalVector(int x, int y) {
			if (!ContainsPoint(x, y)) throw new nShapeException("Coordinates {0} are outside {1}.", new Point(x, y), Type.FullName);
			return Geometry.CalcNormalVectorOfRectangle(x - (w / 2), y - (h / 2), x + (w / 2), y + (h / 2), x, y, 100);
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// All controlPoints of the imagebased shape are inside the shpe's bounds
			// so the tight fitting bounding rectangle equals the loose bounding rectangle
			Rectangle result = Rectangle.Empty;
			result.X = x - w / 2;
			result.Y = y - h / 2;
			result.Width = w;
			result.Height = h;
			return result;
		}


		public override int X {
			get { return x; }
			set {
				int origValue = x;
				if (!MoveTo(value, Y)) {
					MoveTo(origValue, Y);
					throw new InvalidOperationException(string.Format("Shape cannot move to {0}", new Point(value, Y)));
				}
			}
		}


		public override int Y {
			get { return y; }
			set {
				int origValue = y;
				if (!MoveTo(X, value)) {
					MoveTo(X, origValue);
					throw new InvalidOperationException(string.Format("Shape cannot move to {0}", new Point(X, value)));
				}
			}
		}


		public override void Fit(int x, int y, int width, int height) {
			if (height - ch < minH) ch = 0;
			if (width * image.Height <= (height - ch) * image.Width) {
				// Höhe ist verhältnismäßig größer
				this.h = height;
				if (this.h < ch + minH) this.w = this.h * image.Width / image.Height;
				else this.w = (this.h - ch) * image.Width / image.Height;
			} else {
				this.w = width;
				this.h = (this.w * image.Height) / image.Width + ch;
			}
			this.x = (x + width) / 2;
			this.y = (y + height) / 2;
			captionUpdated = false;
			InvalidateDrawCache();
		}


		public override Point CalculateConnectionFoot(int fromX, int fromY) {
			// Zwei identische Punkte definieren keine Gerade
			if (fromX == x && fromY == y) fromY -= 10;
			Nullable<Point> p = Geometry.IntersectLineWithRectangle(fromX, fromY, x, y, x - w / 2, y - h / 2, x - w / 2 + w, y - h / 2 + h);
			if (!p.HasValue) p = Point.Empty;
			// Jede Gerade durch den Mittelpunkt schneidet den Umriss.
			return p.Value;
		}


		protected internal override int ControlPointCount {
			get { return 4; }
		}


		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			Point result = Point.Empty;
			switch (controlPointId) {
				case 1:
					// Links oben
					result.X = x - w / 2;
					result.Y = y - h / 2;
					break;
				case ControlPointId.Reference:
				case 2:
					// Referenzpunkt
					result.X = x;
					result.Y = y;
					break;
				case 3:
					result.X = x;
					result.Y = y - h / 2 + h;
					break;
				case 8:
					result.X = x;
					result.Y = y - h / 2 + h - ch;
					break;
				default:
					Debug.Fail("NotSupported control point id");
					break;
			}
			return result;
		}


		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			bool result;
			switch (controlPointId) {
				case 1:
					result = (controlPointCapability & ControlPointCapabilities.Resize) != 0;
					break;
				case ControlPointId.Reference:
				case 2:
					result = (controlPointCapability & (ControlPointCapabilities.Reference | ControlPointCapabilities.Connect)) != 0;
					break;
				case 3:
					result = (controlPointCapability & ControlPointCapabilities.Connect) != 0;
					break;
				case 8:
					result = (controlPointCapability & ControlPointCapabilities.Resize) != 0;
					break;
				default:
					result = base.HasControlPointCapability(controlPointId, controlPointCapability);
					break;
			}
			return result;
		}


		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			UpdateDrawCache();
			if (h >= ch + minH) {
				graphics.DrawImage(image, x - w / 2, Y - w / 2, w, h - ch);
				caption.Draw(graphics, CharacterStyle, ParagraphStyle);
			} else
				graphics.DrawImage(image, x - w / 2, Y - w / 2, w, h);
			base.Draw(graphics);
		}


		public override void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			UpdateDrawCache();
			base.DrawOutline(graphics, pen);
			graphics.DrawRectangle(pen, x - w / 2, y - h / 2, w, h);
		}


		public override void Invalidate() {
			if (DisplayService != null)
				DisplayService.Invalidate(x - w / 2, y - h / 2, w, h);
			base.Invalidate();
		}


		#region IPlanarShape Members

		public int Angle {
			get { return angle; }
			set { angle = value; }
		}


		[Category("Appearance")]
		public IFillStyle FillStyle {
			get { return fillStyle ?? ((ImageBasedShape)Template.Shape).FillStyle; }
			set {
				fillStyle = (Template != null && value == ((ImageBasedShape)Template.Shape).FillStyle) ? null : value;
				caption.InvalidatePath();
				Invalidate();
			}
		}

		#endregion


		#region ICaptionedShape Members

		public int CaptionCount {
			get { return 1; }
		}


		public bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new IndexOutOfRangeException("index");
			Point location = Point.Empty;
			location.Offset(X, Y);
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.X = -w / 2;
			captionBounds.Y = -h / 2 + h - ch;
			captionBounds.Width = w;
			captionBounds.Height = ch;
			captionBounds = caption.CalculateTextBounds(captionBounds, CharacterStyle, ParagraphStyle, DisplayService);
			Geometry.TransformRectangle(location, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
			return true;
		}


		public bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new IndexOutOfRangeException("index");
			Point location = Point.Empty;
			location.Offset(X, Y);
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.X = -w / 2;
			captionBounds.Y = -h / 2 + h - ch;
			captionBounds.Width = w;
			captionBounds.Height = ch;
			Geometry.TransformRectangle(location, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
			return true;
		}


		public Rectangle GetCaptionTextBounds(int index) {
			if (index != 0) throw new IndexOutOfRangeException("index");
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.X = -w / 2;
			captionBounds.Y = -h / 2 + h - ch;
			captionBounds.Width = w;
			captionBounds.Height = ch;
			return caption.CalculateTextBounds(captionBounds, CharacterStyle, ParagraphStyle, DisplayService);
		}


		public Rectangle GetCaptionBounds(int index) {
			Rectangle result = Rectangle.Empty;
			result.X = -w / 2;
			result.Y = -h / 2 + h - ch;
			result.Width = w;
			result.Height = ch;
			return result;
		}


		public string GetCaptionText(int index) {
			return caption.Text;
		}


		public ICharacterStyle GetCaptionCharacterStyle(int index) {
			return CharacterStyle;
		}


		public IParagraphStyle GetCaptionParagraphStyle(int index) {
			return ParagraphStyle;
		}


		public void SetCaptionText(int index, string text) {
			caption.Text = text;
			this.captionUpdated = false;
		}


		public void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			CharacterStyle = characterStyle;
		}


		public void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			ParagraphStyle = paragraphStyle;
		}


		public int FindCaptionFromPoint(int x, int y) {
			return Geometry.RectangleContainsPoint(x, y, this.x - this.w / 2, this.y - this.h / 2 + this.h - this.ch, this.w, this.ch) ? 0 : -1;
		}

		#endregion


		#region IEntity Members

		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition epd in ShapeBase.GetPropertyDefinitions(version))
				yield return epd;
			yield return new EntityFieldDefinition("FillStyle", typeof(object));
			yield return new EntityFieldDefinition("CharacterStyle", typeof(object));
			yield return new EntityFieldDefinition("ParagraphStyle", typeof(object));
			yield return new EntityFieldDefinition("Text", typeof(string));
			yield return new EntityFieldDefinition("Width", typeof(int));
			yield return new EntityFieldDefinition("Height", typeof(int));
		}


		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			fillStyle = reader.ReadFillStyle();
			charStyle = reader.ReadCharacterStyle();
			paragraphStyle = reader.ReadParagraphStyle();
			
			string txt = reader.ReadString();
			if (caption == null) caption = new Caption(txt);
			else caption.Text = txt;
			w = reader.ReadInt32();
			h = reader.ReadInt32();
		}


		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteStyle(fillStyle);
			writer.WriteStyle(charStyle);
			writer.WriteStyle(ParagraphStyle);

			writer.WriteString(Text);
			writer.WriteInt32(w);
			writer.WriteInt32(h);
		}


		protected override void DeleteCore(IRepositoryWriter writer, int version) {
			// Nothing to do
		}

		#endregion


		protected override ControlPointId GetControlPointId(int index) {
			switch (index) {
				case 0: return 1;
				case 1: return 2;
				case 2: return 3;
				case 3: return 8;
				default: throw new nShapeException("NotSupported control point index.");
			}
		}


		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.RectangleContainsPoint(x, y, this.x - this.w / 2, this.y - this.h / 2, this.w, this.h);
		}


		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			bool result = Geometry.RectangleIntersectsWithRectangle(this.x - this.w / 2, this.y - this.h / 2, this.w, this.h, x, y, width, height);
			return result;
		}


		protected override bool MoveByCore(int deltaX, int deltaY) {
			base.MoveByCore(deltaX, deltaY);
			this.x += deltaX;
			this.y += deltaY;
			transformation.Reset();
			transformation.Translate(deltaX, deltaY);
			caption.TransformPath(transformation);
			return true;
		}


		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			bool result;
			if (pointId == ControlPointId.Reference || pointId == 2) result = MoveByCore(deltaX, deltaY);
			else {
				int oldX = x, oldY = y, oldW = w, oldH = h;
				int newX, newY, newW, newH;
				switch (pointId) {
					case 1:
						newX = x;
						newY = y;
						newW = oldW - deltaX;
						newH = oldH - deltaY;
						if (newW <= newH) newH = int.MaxValue;
						else newW = int.MaxValue;

						if (newW < minW) { newW = minW; newH = int.MaxValue; } else if (newH < minH) { newH = minH; newW = int.MaxValue; }
						if (newW == int.MaxValue) newW = newH * image.Width / image.Height;
						if (newH == int.MaxValue) newH = newW * image.Height / image.Width + ch;

						result = false;
						break;
					case 8:
						newH = oldH + deltaY;
						if (newH < minH) newH = minH;
						newW = (newH - ch) * image.Width / image.Height;
						if (newW < minW) {
							newW = minW;
							newH = newW * image.Height / image.Width + ch;
						}
						newX = oldX;
						newY = oldY - oldH / 2 + newH / 2;
						result = false;
						break;
					default:
						newW = w; newH = h; newX = x; newY = y;
						result = true;
						break;
				}
				x = newX; y = newY; w = newW; h = newH;
				captionUpdated = false;
			}
			return result;
		}


		protected override bool RotateCore(int angle, int x, int y) {
			if (x != X || y != Y) {
				int toX = X;
				int toY = Y;
				Geometry.RotatePoint(x, y, angle, ref toX, ref toY);
				MoveTo(toX, toY);
			}
			return false;
		}


		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			caption.InvalidatePath();
		}


		protected override void RecalcDrawCache() {
			UpdateDrawCache();
		}


		protected override void UpdateDrawCache() {
			if (drawCacheIsInvalid) {
				ch = CharacterStyle.Size + 4;
				caption.CalculatePath(-w / 2, -h / 2 + -ch, w, ch, CharacterStyle, ParagraphStyle);
				TransformDrawCache(x, y + h, 0, x, y);
				captionUpdated = true;
			}
		}


		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			transformation.Reset();
			transformation.Translate(deltaX, deltaY);
			caption.TransformPath(transformation);
		}


		private void Construct(string resourceBaseName, Assembly resourceAssembly) {
			if (resourceBaseName == null) throw new ArgumentNullException("resourceBaseName");
			System.IO.Stream stream = resourceAssembly.GetManifestResourceStream(resourceBaseName);
			image = Image.FromStream(stream);
			this.resourceName = resourceBaseName;
			this.resourceAssembly = resourceAssembly;
		}


		protected const int minW = 10;
		protected const int minH = 10;

		protected int x, y; // Position of reference point in the center
		protected int w, h; // Size of shape
		protected int angle;
		protected Image image;
		private ICharacterStyle charStyle;
		private IParagraphStyle paragraphStyle;
		private IFillStyle fillStyle;

		// Caption fields
		protected bool captionUpdated = false;
		protected int ch; // Height of the caption
		protected Caption caption = new Caption("Test");

		private Matrix transformation = new Matrix();

		// these are needed for calling the constructor when cloning
		protected string resourceName;
		protected Assembly resourceAssembly;
	}


	/// <summary>
	/// Provides a metafile whose color and line color can be customized.
	/// </summary>
	public class CustomizableMetaFile : ImageBasedShape, IPlanarShape {

		protected internal CustomizableMetaFile(ShapeType shapeType, Template template,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, template, resourceBaseName, resourceAssembly) {
			Construct();
		}


		protected internal CustomizableMetaFile(ShapeType shapeType, IStyleSet styleSet,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, styleSet, resourceBaseName, resourceAssembly) {
			Construct();
		}


		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Debug.Assert(image is Metafile);
			metafileDataSize = 20;
			metafileData = new byte[metafileDataSize];

			LineStyle = styleSet.LineStyles.Normal;
			FillStyle = styleSet.FillStyles.Red;

			imageAttribs = GdiHelpers.GetImageAttributes(nShapeImageLayout.Original);
		}


		public override Shape Clone() {
			Shape result = new CustomizableMetaFile(Type, (Template)null, resourceName, resourceAssembly);
			result.CopyFrom(this);
			return result;
		}


		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is CustomizableMetaFile) {
			}
		}


		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (!captionUpdated) UpdateDrawCache();
			brushReplaced = false;
			Rectangle dstBounds = Rectangle.Empty;
			dstBounds.Offset((x - w / 2), (y - h / 2));
			dstBounds.Width = w;
			dstBounds.Height = h - ch;

			//Matrix origTransform = graphics.Transform;
			//float scaleX, scaleY;
			//GdiHelpers.CalcImageScaleAndAspect(out scaleX, out scaleY, dstBounds.Width, dstBounds.Height, image, nShapeImageLayout.Stretch);
			//int offsetX, offsetY;
			//MetafileHeader header = ((Metafile)image).GetMetafileHeader();
			//offsetX = dstBounds.X - header.Bounds.X;
			//offsetY = dstBounds.Y - header.Bounds.Y;
			//graphics.ResetTransform();
			//graphics.TranslateTransform(-header.Bounds.X, -header.Bounds.Y, MatrixOrder.Append);
			//graphics.ScaleTransform(scaleX, scaleY, MatrixOrder.Append);
			//graphics.TranslateTransform(dstBounds.X, dstBounds.Y, MatrixOrder.Append);
			//graphics.EnumerateMetafile((Metafile)image, Point.Empty, metafileDelegate, IntPtr.Zero, imageAttribs);
			//graphics.Transform = origTransform;

			// Workaround: Create a buffer image to draw
			if (bufferImage == null) bufferImage = CreateImage();
			MetafileHeader header = bufferImage.GetMetafileHeader();
			graphics.DrawImage(bufferImage, dstBounds, header.Bounds.X, header.Bounds.Y, header.Bounds.Width, header.Bounds.Height, GraphicsUnit.Pixel, imageAttribs);

#if DEBUG
			//graphics.DrawRectangle(Pens.Red, dstBounds);
#endif
			if (h >= ch + 2 * minH) caption.Draw(graphics, CharacterStyle, ParagraphStyle);

			//using (Image img = CreateImage())
			//   GdiHelpers.SaveImageToFile(img, "S:\\CustomizableMetafile " + Type.Name + " Export Test.emf", nShapeImageFormat.EmfPlus);
		}


		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			imageAttribs = GdiHelpers.GetImageAttributes(FillStyle);
		}


		public override bool NotifyStyleChanged(IStyle style) {
			bool result = base.NotifyStyleChanged(style);
			if (IsStyleAffected(LineStyle, style) || IsStyleAffected(FillStyle, style)) {
				Invalidate();
				result = true;
			}
			return result;
		}


		private void Construct() {
			metafileDelegate = new Graphics.EnumerateMetafileProc(MetafileProc);
		}


		private bool MetafileProc(EmfPlusRecordType recordType, int flags, int dataSize,
						IntPtr data, PlayRecordCallback callbackData) {
			if (data != IntPtr.Zero) {
			   if (dataSize > metafileDataSize) {
			      metafileDataSize = dataSize;
			      metafileData = new byte[metafileDataSize];
			   }
			   // Copy the unmanaged record to a managed byte buffer that can be used by PlayRecord.
			   Marshal.Copy(data, metafileData, 0, dataSize);
			   // Adjust the color
				switch (recordType) {
					case EmfPlusRecordType.EmfSetWindowExtEx:
						// An Position 0 steht die Größe in Bild-Einheiten
						int boundsWidth = Buffers.GetInt32(metafileData, 0);
						if (boundsWidth > 0) scaling = boundsWidth / w;
						break;
					case EmfPlusRecordType.EmfSetViewportExtEx:
						// An Position 0 steht die Größe als SIZEL
						// Buffers.SetInt32(metafileData, 0, w);
						// Buffers.SetInt32(metafileData, 4, h - caption.Bounds.Height);
						break;
					case EmfPlusRecordType.EmfCreateBrushIndirect:
						// This type of record only appears in WMF and 'classic' EMF files, not in EMF+ files.
						if (!brushReplaced) {
							Buffers.SetByte(metafileData, 8, FillStyle.BaseColorStyle.Color.R);
							Buffers.SetByte(metafileData, 9, FillStyle.BaseColorStyle.Color.G);
							Buffers.SetByte(metafileData, 10, FillStyle.BaseColorStyle.Color.B);
							Buffers.SetByte(metafileData, 11, FillStyle.BaseColorStyle.Color.A);
							brushReplaced = true;
						}
						break;
					case EmfPlusRecordType.EmfCreatePen:
						// This type of record only appears in WMF and 'classic' EMF files, not in EMF+ files.
						Buffers.SetInt32(metafileData, 4, (int)LineStyle.DashType);
						Buffers.SetInt32(metafileData, 8, (int)(LineStyle.LineWidth * scaling));
						// y-Komponente wird angeblich nicht benutzt.
						Buffers.SetInt32(metafileData, 12, LineStyle.LineWidth);
						Buffers.SetByte(metafileData, 16, LineStyle.ColorStyle.Color.R);
						Buffers.SetByte(metafileData, 17, LineStyle.ColorStyle.Color.G);
						Buffers.SetByte(metafileData, 18, LineStyle.ColorStyle.Color.B);
						Buffers.SetByte(metafileData, 19, LineStyle.ColorStyle.Color.A);
						break;
					case EmfPlusRecordType.FillPath:
						// This type of record only appears in EMF+ files, not in WMF and 'classic' EMF files.
						// Content of metafileData:
						// Status FillPath(
						//		const Brush *brush,
						//		const GraphicsPath *path);
						//
						// ToDo: Find a way how to replace the pointer to the brush with a pointer to fillstyle's brush
						break;
					default:
						// nothing to do
						break;
				}
			}
			((Metafile)image).PlayRecord(recordType, flags, dataSize, metafileData);
			return true;
		}


		private Metafile CreateImage() {
			// Create MetaFile and graphics context
			Metafile metaFile = null;
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				IntPtr hdc = gfx.GetHdc();
				try {
					MetafileHeader header = ((Metafile)image).GetMetafileHeader();
					EmfType emfType;
					switch(header.Type) {
						case MetafileType.EmfPlusDual: emfType = EmfType.EmfPlusDual; break;
						case MetafileType.EmfPlusOnly: emfType = EmfType.EmfPlusOnly; break;
						default: emfType = EmfType.EmfOnly; break;
					}
					metaFile = new Metafile(stream, hdc, header.Bounds, MetafileFrameUnit.Pixel,
						emfType, "CustomizableMetafile Buffer Image");
				} finally {
					gfx.ReleaseHdc(hdc);
				}
			}
			using (Graphics gfx = Graphics.FromImage(metaFile)) {
				GdiHelpers.ApplyGraphicsSettings(gfx, nShapeRenderingQuality.MaximumQuality);
				MetafileHeader header = ((Metafile)image).GetMetafileHeader();
				gfx.EnumerateMetafile((Metafile)image, header.Bounds, header.Bounds,
					GraphicsUnit.Pixel, metafileDelegate, IntPtr.Zero, imageAttribs);
			}
			if (stream != null) {
				stream.Dispose();
				stream = null;
			}
			return metaFile;
		}


		private Graphics.EnumerateMetafileProc metafileDelegate;
		private byte[] metafileData = null; // Date for meta file drawing
		private int metafileDataSize = 0; // Allocated size for meta file data
		private bool brushReplaced;
		private float scaling;

		private ImageAttributes imageAttribs;
		private Metafile bufferImage;
	}

}