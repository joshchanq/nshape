using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;


namespace Dataweb.nShape.Advanced {

	public enum nShapeRenderingQuality {
		/// <summary>
		/// Best rendering Quality, lowest rendering speed.
		/// </summary>
		MaximumQuality,
		/// <summary>
		/// High quality rendering with acceptable performance loss.
		/// </summary>
		HighQuality,
		/// <summary>
		/// Rendering with system default settings.
		/// </summary>
		DefaultQuality,
		/// <summary>
		/// Low Quality, high rendering speed.
		/// </summary>
		LowQuality
	}


	public static class GdiHelpers {

		#region Methods for drawing geometric primitives (debug mode obly)

#if DEBUG
		/// <summary>
		/// Visualizing points - used for debugging purposes
		/// </summary>
		public static void DrawPoint(Graphics gfx, Pen pen, PointF point, int radius) {
			DrawPoint(gfx, pen, point.X, point.Y, radius);
		}


		/// <summary>
		/// Visualizing points - used for debugging purposes
		/// </summary>
		public static void DrawPoint(Graphics gfx, Pen pen, float x, float y, int radius) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			gfx.DrawLine(pen, x - radius, y, x + radius, y);
			gfx.DrawLine(pen, x, y - radius, x, y + radius);
		}


		/// <summary>
		/// Visualizing angles - used for debugging purposes
		/// </summary>
		public static void DrawAngle(Graphics gfx, Brush brush, PointF center, float angle, int radius) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (brush == null) throw new ArgumentNullException("brush");
			Rectangle rect = Rectangle.Empty;
			rect.X = (int)Math.Round(center.X - radius);
			rect.Y = (int)Math.Round(center.Y - radius);
			rect.Width = rect.Height = radius + radius;
			gfx.FillPie(brush, rect, 0, angle);
			gfx.DrawPie(Pens.White, rect, 0, angle);
		}


		public static void DrawLine(Graphics graphics, Pen pen, PointF p1, PointF p2) {
			float a, b, c;
			Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
			DrawLine(graphics, pen, a, b, c);
		}


		public static void DrawLine(Graphics graphics, Pen pen, float a, float b, float c) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			// Gleichung nach y auflösen, 2 Werte für X einsetzen und dann die zugehörigen y ausrechnen
			float x1 = 0, x2 = 0, y1 = 0, y2 = 0;
			if (b != 0) {
				x1 = -10000;
				x2 = 10000;
				y1 = (-(a * x1) - c) / b;
				y2 = (-(a * x2) - c) / b;
			}
			else if (a != 0) {
				y1 = -10000;
				y2 = 10000;
				x1 = (-(b * y1) - c) / a;
				x2 = (-(b * y2) - c) / a;
			}
			if ((a != 0 || b != 0) 
				&& !(float.IsNaN(x1) || float.IsNaN(x2) || float.IsNaN(y1) || float.IsNaN(y2))
				&& !(float.IsInfinity(x1) || float.IsInfinity(x2) || float.IsInfinity(y1) || float.IsInfinity(y2)))
				graphics.DrawLine(pen, x1, y1, x2, y2);
		}


		/// <summary>
		/// Visualizing angles - used for debugging purposes
		/// </summary>
		public static void DrawAngle(Graphics gfx, Brush brush, PointF center, float startAngle, float sweepAngle, int radius) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (brush == null) throw new ArgumentNullException("brush");
			Rectangle rect = Rectangle.Empty;
			rect.X = (int)Math.Round(center.X - radius);
			rect.Y = (int)Math.Round(center.Y - radius);
			rect.Width = rect.Height = radius + radius;
			gfx.FillPie(brush, rect, startAngle, sweepAngle);
			gfx.DrawPie(Pens.White, rect, startAngle, sweepAngle);
		}


		/// <summary>
		/// Visualizing rotated rectangles - used for debugging purposes
		/// </summary>
		public static void DrawRotatedRectangle(Graphics gfx, Pen pen, RectangleF rect, float angleDeg) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			PointF[] pts = new PointF[5] {
				new PointF(rect.Left, rect.Top),
				new PointF(rect.Right, rect.Top),
				new PointF(rect.Right, rect.Bottom),
				new PointF(rect.Left, rect.Bottom),
				new PointF(rect.Left, rect.Top) };
			PointF center = PointF.Empty;
			center.X = rect.Left + (rect.Width / 2f);
			center.Y = rect.Top + (rect.Height / 2f);
			matrix.Reset();
			matrix.RotateAt(angleDeg, center);
			matrix.TransformPoints(pts);
			gfx.DrawLines(pen, pts);
		}


		/// <summary>
		/// Visualizing rotated ellipses - used for debugging purposes
		/// </summary>
		public static void DrawRotatedEllipse(Graphics gfx, Pen pen, float centerX, float centerY, float width, float height, float angleDeg) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			using (GraphicsPath path = new GraphicsPath()) {
				path.StartFigure();
				path.AddEllipse(centerX - width / 2f, centerY - height / 2f, width, height);

				PointF center = new PointF(centerX, centerY);
				matrix.Reset();
				matrix.RotateAt(angleDeg, center);
				path.Transform(matrix);
				gfx.DrawPath(pen, path);
			}
		}
#endif

		#endregion


		#region Graphics settings

		/// <summary>
		/// Sets all parameters that affect rendering quality / rendering speed
		/// </summary>
		public static void ApplyGraphicsSettings(Graphics graphics, nShapeRenderingQuality renderingQuality) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			switch (renderingQuality) {
				case nShapeRenderingQuality.MaximumQuality:
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					//graphics.TextRenderingHint = System.Drawing.Title.TextRenderingHint.AntiAliasGridFit;	// smoothed but blurry fonts
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;	// sharp but slightly chunky fonts
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					//graphics.CompositingMode = CompositingMode.SourceOver;				// Transparency too high
					//graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;			// produces quite blurry results
					break;

				case nShapeRenderingQuality.HighQuality:
					// antialiasing and nice font rendering
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					//graphics.TextRenderingHint = System.Drawing.Title.TextRenderingHint.AntiAliasGridFit;	// smoothed but blurry fonts
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;	// sharp but slightly chunky fonts
					graphics.InterpolationMode = InterpolationMode.High;
					break;

				case nShapeRenderingQuality.LowQuality:
					graphics.SmoothingMode = SmoothingMode.HighSpeed;
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
					graphics.InterpolationMode = InterpolationMode.Low;
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
					break;

				case nShapeRenderingQuality.DefaultQuality:
				default:
					graphics.SmoothingMode = SmoothingMode.Default;
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
					graphics.InterpolationMode = InterpolationMode.Default;
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.CompositingQuality = CompositingQuality.Default;
					graphics.PixelOffsetMode = PixelOffsetMode.Default;
					break;
			}
		}


		/// <summary>
		///  Copy all parameters that affect rendering quality / rendering speed from the given infoGraphics.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="renderingQuality"></param>
		public static void ApplyGraphicsSettings(Graphics graphics, Graphics infoGraphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (infoGraphics == null) throw new ArgumentNullException("infoGraphics");
			graphics.SmoothingMode = infoGraphics.SmoothingMode;
			graphics.TextRenderingHint = infoGraphics.TextRenderingHint;
			graphics.InterpolationMode = infoGraphics.InterpolationMode;
			graphics.CompositingMode = infoGraphics.CompositingMode;
			graphics.CompositingQuality = infoGraphics.CompositingQuality;
			graphics.PixelOffsetMode = infoGraphics.PixelOffsetMode;
		}

		#endregion


		#region Drawing images

		/// <summary>
		/// Get a suitable GDI+ WrapMode out of the given nShapeImageLayout.
		/// </summary>
		public static WrapMode GetWrapMode(nShapeImageLayout imageLayout) {
			switch (imageLayout) {
				case nShapeImageLayout.Center:
				case nShapeImageLayout.Fit:
				case nShapeImageLayout.Original:
				case nShapeImageLayout.Stretch:
					return WrapMode.Clamp;
				case nShapeImageLayout.CenterTile:
				case nShapeImageLayout.Tile:
					return WrapMode.Tile;
				case nShapeImageLayout.FilpTile:
					return WrapMode.TileFlipXY;
				default: throw new nShapeUnsupportedValueException(imageLayout);
			}
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(IFillStyle fillStyle) {
			return GetImageAttributes(fillStyle.ImageLayout, fillStyle.ImageGammaCorrection, fillStyle.ImageTransparency, fillStyle.ConvertToGrayScale, false, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(nShapeImageLayout imageLayout) {
			return GetImageAttributes(imageLayout, -1f, 0, false, false, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(nShapeImageLayout imageLayout, Color transparentColor) {
			return GetImageAttributes(imageLayout, -1f, 0, false, false, transparentColor);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(nShapeImageLayout imageLayout, float gamma, byte transparency, bool grayScale) {
			return GetImageAttributes(imageLayout, gamma, transparency, grayScale, false, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(nShapeImageLayout imageLayout, float gamma, byte transparency, bool grayScale, bool forPreview) {
			return GetImageAttributes(imageLayout, gamma, transparency, grayScale, forPreview, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(nShapeImageLayout imageLayout, float gamma, byte transparency, bool grayScale, bool forPreview, Color transparentColor) {
			if (transparency < 0 || transparency > 100) throw new ArgumentOutOfRangeException("transparency");
			ImageAttributes imageAttribs = new ImageAttributes();

			// Set WrapMode
			imageAttribs.SetWrapMode(GetWrapMode(imageLayout));

			// Set Gamma correction
			if (gamma > 0) imageAttribs.SetGamma(gamma);

			// Set / Reset conversion to grayScale
			if (grayScale || (forPreview && Design.PreviewsAsGrayScale)) {
				// set luminance corrected color shear matrix
				// Row 0, Element 1 to 3
				colorMatrix.Matrix00 = luminanceFactorRed;
				colorMatrix.Matrix01 = luminanceFactorRed;
				colorMatrix.Matrix02 = luminanceFactorRed;
				// Row 1, Element 1 to 3
				colorMatrix.Matrix10 = luminanceFactorGreen;
				colorMatrix.Matrix11 = luminanceFactorGreen;
				colorMatrix.Matrix12 = luminanceFactorGreen;
				// Row 2, Element 1 to 3
				colorMatrix.Matrix20 = luminanceFactorBlue;
				colorMatrix.Matrix21 = luminanceFactorBlue;
				colorMatrix.Matrix22 = luminanceFactorBlue;
			} else {
				// reset color shear matrix
				colorMatrix.Matrix00 = colorMatrix.Matrix11 = colorMatrix.Matrix22 = 1;
				colorMatrix.Matrix01 = colorMatrix.Matrix02 = 
				colorMatrix.Matrix10 = colorMatrix.Matrix12 = 
				colorMatrix.Matrix20 = colorMatrix.Matrix21 = 0;
			}
			// set transparency
			if (forPreview) 
				colorMatrix.Matrix33 = (float)(100 - Design.GetPreviewTransparency(transparency)) / 100;
			else 
				colorMatrix.Matrix33 = (float)(100 - transparency) / 100;
			imageAttribs.SetColorMatrix(colorMatrix);

			if (transparentColor != Color.Empty) {
				colorMaps[0].OldColor = transparentColor;
				colorMaps[0].NewColor = Color.Transparent;
				imageAttribs.SetRemapTable(colorMaps);
			}
			return imageAttribs;
		}


		/// <summary>
		/// Draw an image into the specified bounds
		/// </summary>
		public static void DrawImage(Graphics gfx, Image image, ImageAttributes imageAttribs, nShapeImageLayout imageLayout, Rectangle dstBounds, Rectangle srcBounds) {
			DrawImage(gfx, image, imageAttribs, imageLayout, dstBounds, srcBounds, 0);
		}


		/// <summary>
		/// Draw an image into the specified bounds
		/// </summary>
		public static void DrawImage(Graphics gfx, Image image, ImageAttributes imageAttribs, nShapeImageLayout imageLayout, Rectangle dstBounds, Rectangle srcBounds, float angle) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (image == null) throw new ArgumentNullException("image");
			// ToDo: Optimize this (draw only the drawArea part of the image, optimize calculations and variable use, draw bitmaps by using a TextureBrush)

			float scaleX, scaleY, aspectRatio;
			aspectRatio = CalcImageScaleAndAspect(out scaleX, out scaleY, dstBounds.Width, dstBounds.Height, image, imageLayout);

			Rectangle destinationBounds = Rectangle.Empty;
			// transform image bounds
			switch (imageLayout) {
				case nShapeImageLayout.Tile:
				case nShapeImageLayout.FilpTile:
					destinationBounds = dstBounds;
					break;
				case nShapeImageLayout.Original:
					destinationBounds.X = dstBounds.X;
					destinationBounds.Y = dstBounds.Y;
					destinationBounds.Width = Math.Min(image.Width, dstBounds.Width);
					destinationBounds.Height = Math.Min(image.Height, dstBounds.Height);
					break;
				case nShapeImageLayout.CenterTile:
				case nShapeImageLayout.Center:
					destinationBounds.X = dstBounds.X + (int)Math.Round((dstBounds.Width - image.Width) / 2f);
					destinationBounds.Y = dstBounds.Y + (int)Math.Round((dstBounds.Height - image.Height) / 2f);
					destinationBounds.Width = image.Width;
					destinationBounds.Height = image.Height;
					break;
				case nShapeImageLayout.Stretch:
				case nShapeImageLayout.Fit:
					destinationBounds.X = dstBounds.X;
					destinationBounds.Y = dstBounds.Y;
					if (imageLayout == nShapeImageLayout.Fit) {
						destinationBounds.X += (int)Math.Round((dstBounds.Width - (image.Width * scaleX)) / 2f);
						destinationBounds.Y += (int)Math.Round((dstBounds.Height - (image.Height * scaleY)) / 2f);
					}
					destinationBounds.Width = (int)Math.Round(image.Width * scaleX);
					destinationBounds.Height = (int)Math.Round(image.Height * scaleY);
					break;
				default:
					throw new nShapeException(string.Format("Unexpected {0} '{1}'.", imageLayout.GetType(), imageLayout));
			}

			PointF center = PointF.Empty;
			center.X = dstBounds.X + (dstBounds.Width / 2f);
			center.Y = dstBounds.Y + (dstBounds.Height / 2f);
			if (angle != 0) {
				gfx.TranslateTransform(center.X, center.Y);
				gfx.RotateTransform(angle);
				gfx.TranslateTransform(-center.X, -center.Y);
			}

			GraphicsUnit gfxUnit = GraphicsUnit.Display;
			RectangleF imageBounds = image.GetBounds(ref gfxUnit);
			int srcX, srcY, srcWidth, srcHeight;
			switch (imageLayout) {
				case nShapeImageLayout.CenterTile:
				case nShapeImageLayout.FilpTile:
				case nShapeImageLayout.Tile:
					int startX, startY, endX, endY;
					if (imageLayout == nShapeImageLayout.CenterTile) {
						int nX = (int)Math.Ceiling(dstBounds.Width / (float)image.Width);
						int nY = (int)Math.Ceiling(dstBounds.Height / (float)image.Height);
						if (nX == 1) startX = destinationBounds.X;
						else startX = (int)Math.Round(destinationBounds.X - ((image.Width * nX) / 2f));
						if (nY == 1) startY = destinationBounds.Y;
						else startY = (int)Math.Round(destinationBounds.Y - ((image.Height * nY) / 2f));
					} else {
						startX = dstBounds.X;
						startY = dstBounds.Y;
					}
					endX = dstBounds.Right;
					endY = dstBounds.Bottom;

					Rectangle r = Rectangle.Empty;
					r.Width = image.Width;
					r.Height = image.Height;
					for (int x = startX; x < endX; x += image.Width) {
						for (int y = startY; y < endY; y += image.Height) {
							// Set source bounds location
							srcX = (x < 0) ? -startX : 0;
							srcY = (y < 0) ? -startY : 0;
							// Set destination bounds location
							r.X = Math.Max(x, dstBounds.X);
							r.Y = Math.Max(y, dstBounds.Y);
							// set source and destination bounds' size
							if (x + image.Width > endX)
								srcWidth = r.Width = endX - r.X;
							else
								srcWidth = r.Width = image.Width - srcX;

							if (y + image.Height > endY)
								srcHeight = r.Height = endY - r.Y;
							else
								srcHeight = r.Height = image.Height - srcY;

							if (imageLayout == nShapeImageLayout.FilpTile) {
								int modX = (x / image.Width) % 2;
								int modY = (y / image.Height) % 2;
								if (modX != 0) {
									srcX = image.Width - srcX;
									srcWidth = -srcWidth;
								}
								if (modY != 0) {
									srcY = image.Height - srcY;
									srcHeight = -srcHeight;
								}
							}
							gfx.DrawImage(image, r, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel, imageAttribs);
						}
					}
					break;
				case nShapeImageLayout.Original:
				case nShapeImageLayout.Center:
					if (image.Width > dstBounds.Width || image.Height > dstBounds.Height) {
						srcWidth = destinationBounds.Width = Math.Min(image.Width, dstBounds.Width);
						srcHeight = destinationBounds.Height = Math.Min(image.Height, dstBounds.Height);
						destinationBounds.X = dstBounds.X;
						destinationBounds.Y = dstBounds.Y;
					} else {
						srcWidth = image.Width;
						srcHeight = image.Height;
					}
					if (imageLayout == nShapeImageLayout.Center) {
						srcX = image.Width - srcWidth;
						srcY = image.Height - srcHeight;
					} else {
						srcX = (int)imageBounds.X;
						srcY = (int)imageBounds.Y;
					}
					gfx.DrawImage(image, destinationBounds, srcX, srcY, srcWidth, srcHeight, GraphicsUnit.Pixel, imageAttribs);
					break;
				case nShapeImageLayout.Fit:
				case nShapeImageLayout.Stretch:
					gfx.DrawImage(image, destinationBounds, imageBounds.X, imageBounds.Y, imageBounds.Width, imageBounds.Height, gfxUnit, imageAttribs);
					break;
				default: throw new nShapeUnsupportedValueException(imageLayout);
			}
			if (angle != 0) {
				gfx.TranslateTransform(center.X, center.Y);
				gfx.RotateTransform(-angle);
				gfx.TranslateTransform(-center.X, -center.Y);
			}
		}

		#endregion


		#region Creating and transforming brushes

		public static TextureBrush CreateTextureBrush(Image image, nShapeImageLayout imageLayout, float gamma, byte transparency, bool grayScale) {
			if (image == null) throw new ArgumentNullException("image");
			return CreateTextureBrush(image, GetImageAttributes(imageLayout, gamma, transparency, grayScale));
		}


		public static TextureBrush CreateTextureBrush(Image image, ImageAttributes imageAttribs) {
			if (image == null) throw new ArgumentNullException("image");
			Rectangle brushBounds = Rectangle.Empty;
			brushBounds.X = 0;
			brushBounds.Y = 0;
			brushBounds.Width = image.Width;
			brushBounds.Height = image.Height;
			return new TextureBrush(image, brushBounds, imageAttribs);
		}
		
		
		public static void TransformLinearGradientBrush(LinearGradientBrush brush, float gradientAngleDeg, Rectangle unrotatedBounds, Point center, float angleDeg) {
			if (brush == null) throw new ArgumentNullException("brush");
			// move brush higher than needed  and make the brush larger than needed
			// (ensure that there are no false color pixels from antialiasing inside the gradient)
			float gradientSize = (int)Math.Ceiling(CalculateGradientSize(angleDeg, gradientAngleDeg, center.X - unrotatedBounds.Left, Math.Max(center.Y - unrotatedBounds.Top, unrotatedBounds.Bottom - center.Y))) + 2;
			float scaleFactor = ((float)Math.Sqrt((gradientSize * gradientSize) * 2) + 1f) / ((LinearGradientBrush)brush).Rectangle.Width;

			brush.ResetTransform();
			brush.TranslateTransform(center.X, center.Y - gradientSize);
			brush.RotateTransform(gradientAngleDeg);
			brush.ScaleTransform(scaleFactor, scaleFactor);
		}


		public static void TransformPathGradientBrush(PathGradientBrush brush, Rectangle unrotatedBounds, Point center, float angleDeg) {
			throw new NotImplementedException();
		}


		public static void TransformTextureBrush(TextureBrush brush, nShapeImageLayout imageLayout, Rectangle unrotatedBounds, Point center, float angleDeg) {
			if (brush == null) throw new ArgumentNullException("brush");
			// scale image
			float scaleX, scaleY;
			GdiHelpers.CalcImageScaleAndAspect(out scaleX, out scaleY, unrotatedBounds.Width, unrotatedBounds.Height, brush.Image, imageLayout);
			// rotate at center point
			((TextureBrush)brush).ResetTransform();
			((TextureBrush)brush).TranslateTransform(center.X, center.Y);
			((TextureBrush)brush).RotateTransform(angleDeg);
			((TextureBrush)brush).TranslateTransform(-unrotatedBounds.Width / 2f, -unrotatedBounds.Height / 2f);
			// scale image
			switch (imageLayout) {
				case nShapeImageLayout.Tile:
				case nShapeImageLayout.FilpTile:
					// nothing to do
					break;
				case nShapeImageLayout.Center:
				case nShapeImageLayout.CenterTile:
				case nShapeImageLayout.Original:
					((TextureBrush)brush).TranslateTransform((unrotatedBounds.Width - brush.Image.Width) / 2f, (unrotatedBounds.Height - brush.Image.Height) / 2f);
					break;
				case nShapeImageLayout.Stretch:
				case nShapeImageLayout.Fit:
					if (imageLayout == nShapeImageLayout.Fit)
						((TextureBrush)brush).TranslateTransform((unrotatedBounds.Width - (brush.Image.Width * scaleX)) / 2f, (unrotatedBounds.Height - (brush.Image.Height * scaleY)) / 2f);
					((TextureBrush)brush).ScaleTransform(scaleX, scaleY);
					break;
				default: throw new nShapeUnsupportedValueException(imageLayout);
			}
		}


		/// <summary>
		/// Calculates scale factors and Aspect ratio depending on the given ImageLayout.
		/// </summary>
		/// <param name="scaleFactorX">Scaling factor on X axis.</param>
		/// <param name="scaleFactorY">Scaling factor on Y axis.</param>
		/// <param name="dstWidth">The desired width of the image.</param>
		/// <param name="dstHeight">The desired height of the image.</param>
		/// <param name="image">the untransformed Image object.</param>
		/// <param name="imageLayout">ImageLayout enumeration. Deines the scaling behaviour.</param>
		/// <returns>Aspect ratio of the image.</returns>
		public static float CalcImageScaleAndAspect(out float scaleFactorX, out float scaleFactorY, int dstWidth, int dstHeight, Image image, nShapeImageLayout imageLayout) {
			float aspectRatio = 1;
			scaleFactorX = 1f;
			scaleFactorY = 1f;
			if (image != null) {
				aspectRatio = (float)image.Width / image.Height;
				switch (imageLayout) {
					case nShapeImageLayout.Fit:
						double lowestRatio = Math.Min((double)dstWidth / (double)image.Width, (double)dstHeight / (double)image.Height);
						scaleFactorX = (float)Math.Round(lowestRatio, 6);
						scaleFactorY = (float)Math.Round(lowestRatio, 6);
						break;
					case nShapeImageLayout.Stretch:
						scaleFactorX = (float)dstWidth / image.Width;
						scaleFactorY = (float)dstHeight / image.Height;
						break;
					case nShapeImageLayout.Original:
					case nShapeImageLayout.Center:
						// nothing to do
						break;
					case nShapeImageLayout.CenterTile:
					case nShapeImageLayout.Tile:
					case nShapeImageLayout.FilpTile:
						// nothing to do
						break;
					default:
						throw new nShapeException(string.Format("Unexpected {0} '{1}'", imageLayout.GetType(), imageLayout));
				}
			}
			return aspectRatio;
		}


		// calculate gradient size dependent of the shape's rotation
		private static float CalculateGradientSize(float angle, float gradientAngle, int leftToCenter, int topToCenter) {
			float result = 0f;

			double a = angle;
			int r = leftToCenter;
			int o = topToCenter;

			if (a < 0) a = 360 + a;
			a = a % 180;
			if (a / 90 >= 1 && a / 90 < 2) {
				r = topToCenter;
				o = leftToCenter;
				a = a % 90;
			}
			if (a > 45)
				a = (a - ((a % 45) * 2)) * (Math.PI / 180);
			else
				a = a * (Math.PI / 180);
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			// calculate offset 
			double dR = (r * sin) + (r * cos);
			double dO = Math.Abs((o * cos) - (o * sin));
			result = (float)Math.Round(dR + dO, 6);

			return result;
		}

		#endregion


		#region Exporting Images

		public static void SaveImageToFile(Image image, string filePath, nShapeImageFormat imageFormat) {
			SaveImageToFile(image, filePath, imageFormat, 75);
		}


		public static void SaveImageToFile(Image image, string filePath, nShapeImageFormat imageFormat, int compressionQuality) {
			if (image == null) throw new ArgumentNullException("image");
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			if (image is Metafile) {
				EmfHelper.SaveEnhMetaFile(filePath, (Metafile)image.Clone());

				#region Old version: Works but replaces all metafile records with a single DrawImage record
				//// Create a new metafile
				//Graphics gfx = Graphics.FromHwnd(IntPtr.Zero);
				//IntPtr hdc = gfx.GetHdc();
				//Metafile metaFile = new Metafile(filePath, 
				//   hdc, 
				//   Rectangle.FromLTRB(0, 0, image.Width, image.Height), 
				//   MetafileFrameUnit.Pixel, 
				//   (imageFormat == nShapeImageFormat.EmfPlus) ? EmfType.EmfPlusDual : EmfType.EmfOnly);
				//gfx.ReleaseHdc(hdc);
				//gfx.Dispose();

				//// Create graphics context for drawing
				//gfx = Graphics.FromImage(metaFile);
				//GdiHelpers.ApplyGraphicsSettings(gfx, nShapeRenderingQuality.MaximumQuality);
				//// Draw image
				//gfx.DrawImage(image, Point.Empty);
				
				//gfx.Dispose();
				//metaFile.Dispose();
				#endregion

			} else if (image is Bitmap) {
				ImageCodecInfo encoder = GetEncoderInfo(imageFormat);
				EncoderParameters encoderParams = new EncoderParameters(2);
				encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, compressionQuality);
				encoderParams.Param[1] = new EncoderParameter(Encoder.Compression, 100);	// seems to have no effect
				image.Save(filePath, encoder, encoderParams);
			} else image.Save(filePath, GetGdiImageFormat(imageFormat));
		}


		public static void CreateMetafile(string filePath, nShapeImageFormat imageFormat, int width, int height, DrawCallback callback) {
			if (callback == null) throw new ArgumentNullException("callback");
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			Metafile metaFile = null;
			// Create a new metafile
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				IntPtr hdc = gfx.GetHdc();
				try {
					metaFile= new Metafile(filePath,
						hdc,
						Rectangle.FromLTRB(0, 0, width, height),
						MetafileFrameUnit.Pixel,
						(imageFormat == nShapeImageFormat.EmfPlus) ? EmfType.EmfPlusDual : EmfType.EmfOnly);
				} finally { gfx.ReleaseHdc(hdc); }
			}
			// Create graphics context for drawing
			if (metaFile != null) {
				using (Graphics gfx = Graphics.FromImage(metaFile)) {
					GdiHelpers.ApplyGraphicsSettings(gfx, nShapeRenderingQuality.MaximumQuality);
					// Draw image
					Rectangle bounds = Rectangle.Empty;
					bounds.Width = width;
					bounds.Height = height;
					callback(gfx, bounds);
				}
				metaFile.Dispose();
				metaFile = null;
			}
		}


		public static ImageFormat GetGdiImageFormat(nShapeImageFormat imageFormat) {
			ImageFormat result;
			switch (imageFormat) {
				case nShapeImageFormat.Bmp: result = ImageFormat.Bmp; break;
				case nShapeImageFormat.Emf:
				case nShapeImageFormat.EmfPlus: result = ImageFormat.Emf; break;
				case nShapeImageFormat.Gif: result = ImageFormat.Gif; break;
				case nShapeImageFormat.Jpeg: result = ImageFormat.Jpeg; break;
				case nShapeImageFormat.Png: result = ImageFormat.Png; break;
				case nShapeImageFormat.Tiff: result = ImageFormat.Tiff; break;
				default: return result = ImageFormat.Bmp;
			}
			return result;
		}
		
		
		public static ImageCodecInfo GetEncoderInfo(nShapeImageFormat imageFormat) {
			ImageFormat format = GetGdiImageFormat(imageFormat);
			ImageCodecInfo result = null;
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo ici in encoders)
				if (ici.FormatID.Equals(format.Guid)) {
					result = ici;
					break;
				}
			return result;
		}


		public delegate void DrawCallback(Graphics graphics, Rectangle bounds);


		#endregion


		#region Fields

		private static ImageAttributes imageAttribs = new ImageAttributes();
		private static ColorMap[] colorMaps = new ColorMap[] { new ColorMap() };
		private static ColorMatrix colorMatrix = new ColorMatrix();
		private static Matrix matrix = new Matrix();

		// constants for the color-to-greyscale conversion
		// luminance correction factor (the human eye has preferences regarding colors)
		private const float luminanceFactorRed = 0.3f;
		private const float luminanceFactorGreen = 0.59f;
		private const float luminanceFactorBlue = 0.11f;
		// Alternative values
		//private const float luminanceFactorRed = 0.3f;
		//private const float luminanceFactorGreen = 0.5f;
		//private const float luminanceFactorBlue = 0.3f;

		#endregion
	}

}
