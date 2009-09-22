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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Dataweb.NShape.Advanced;
using Dataweb.Utilities;


namespace Dataweb.NShape.WinFormsUI {

	#region TypeConverters

	public class nShapeTextConverter : TypeConverter {

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string))
				return true;
			if (destinationType == typeof(string[]))
				return true;
			if (destinationType == typeof(IEnumerable<string>))
				return true;
			if (destinationType == typeof(IList<string>))
				return true;
			if (destinationType == typeof(IReadOnlyCollection<string>))
				return true;
			if (destinationType == typeof(ICollection<string>))
				return true;
			return base.CanConvertTo(context, destinationType);
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string) && value != null) {
				//return ((string)value).Split(new string[] { "\n\r", "\r\n", "\n" }, StringSplitOptions.None) as IEnumerable<string>;
				string result = "";
				foreach (string line in ((IEnumerable<string>)value))
					result += line + "\r\n";
				return result;
			} else if (destinationType == typeof(string[]) && value != null)
				return value as IEnumerable<string>;
			else if (destinationType == typeof(IEnumerable<string>) && value != null)
				return value as IEnumerable<string>;
			else if (destinationType == typeof(IList<string>) && value != null)
				return value as IEnumerable<string>;
			else if (destinationType == typeof(ICollection<string>) && value != null)
				return value as IEnumerable<string>;
			else return base.ConvertTo(context, culture, value, destinationType);
		}

	}


	public class nShapeNamedImageConverter : TypeConverter {

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) return true;
			if (destinationType == typeof(Image)) return true;
			if (destinationType == typeof(Bitmap)) return true;
			if (destinationType == typeof(Metafile)) return true;
			return base.CanConvertTo(context, destinationType);
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (value != null && value is NamedImage) {
				NamedImage val = (NamedImage)value;
				if (destinationType == typeof(string))
					return val.Name;
				else if (destinationType == typeof(Bitmap))
					return (Bitmap)val.Image;
				else if (destinationType == typeof(Metafile))
					return (Metafile)val.Image;
				else if (destinationType == typeof(Image))
					return val.Image;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}


	public class nShapeStyleConverter : TypeConverter {

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) return true;
			return base.CanConvertTo(context, destinationType);
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string) && value is IStyle) return ((IStyle)value).Title;
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}


	public class nShapeFontFamilyConverter : TypeConverter {

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string))
				return true;
			return base.CanConvertTo(context, destinationType);
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string) && value != null)
				return ((FontFamily)value).Name;
			return base.ConvertTo(context, culture, value, destinationType);
		}

	}


	public class nShapeTextPaddingConverter : TypeConverter {

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string))
				return true;
			else {
				if (destinationType == typeof(Padding))
					return true;
				else return base.CanConvertTo(context, destinationType);
			}
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				string stringResult = string.Empty;
				if (value != null) {
					TextPadding val = (TextPadding)value;
					stringResult = string.Format("{0}; {1}; {2}; {3}", val.Left, val.Top, val.Right, val.Bottom);
				}
				return stringResult;
			} else if (destinationType == typeof(Padding)) {
				Padding paddingResult = Padding.Empty;
				if (value != null) {
					TextPadding val = (TextPadding)value;
					paddingResult.Left = val.Left;
					paddingResult.Top = val.Top;
					paddingResult.Right = val.Right;
					paddingResult.Bottom = val.Bottom;
				}
				return paddingResult;
			} else
				return base.ConvertTo(context, culture, value, destinationType);
		}


		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			return ((sourceType == typeof(string))
					|| (sourceType == typeof(Padding))
					|| base.CanConvertFrom(context, sourceType));
		}


		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			TextPadding result = TextPadding.Empty;
			Padding padding = Padding.Empty;
			if (value is string) {
				PaddingConverter paddingConverter = (PaddingConverter)TypeDescriptor.GetConverter(typeof(Padding));
				padding = (Padding)paddingConverter.ConvertFrom(context, culture, value);
				result.Left = padding.Left;
				result.Top = padding.Top;
				result.Right = padding.Right;
				result.Bottom = padding.Bottom;
			} else if (value is Padding) {
				padding = (Padding)value;
				result.Left = padding.Left;
				result.Top = padding.Top;
				result.Right = padding.Right;
				result.Bottom = padding.Bottom;
			} else
				return base.ConvertFrom(context, culture, value);
			return result;
		}
	}

	#endregion


	#region UITypeEditors


	/// <summary>
	/// nShape UI type editor for choosing a character style's font from a drop down list.
	/// </summary>
	public class nShapeFontFamilyEditor : UITypeEditor {
		
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null) {
					using (FontFamilyListBox listBox = new FontFamilyListBox(edSvc)) {
						listBox.BorderStyle = BorderStyle.None;
						listBox.IntegralHeight = false;
						listBox.Items.Clear();

						FontFamily[] families = FontFamily.Families;
						int cnt = families.Length;
						for (int i = 0; i < cnt; ++i) {
							listBox.Items.Add(families[i].Name);
							if (families[i] == value)
								listBox.SelectedIndex = listBox.Items.Count - 1;
						}
						edSvc.DropDownControl(listBox);
						if (listBox.SelectedItem != null)
							value = listBox.SelectedItem.ToString();
					}
				}
			}
			return value;
		}


		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.DropDown;
			return base.GetEditStyle(context);
		}


		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return false;
		}


		public override void PaintValue(PaintValueEventArgs e) {
			if (e != null && e.Value != null) {
				// store original values;
				SmoothingMode origSmoothingMode = e.Graphics.SmoothingMode;
				CompositingQuality origCompositingQuality = e.Graphics.CompositingQuality;
				InterpolationMode origInterpolationmode = e.Graphics.InterpolationMode;
				System.Drawing.Text.TextRenderingHint origTextRenderingHint = e.Graphics.TextRenderingHint;
				Matrix origTransform = e.Graphics.Transform;

				// set new GraphicsModes
				e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
				e.Graphics.CompositingQuality = CompositingQuality.HighQuality;	// CAUTION: Slows down older machines!!
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

				StringFormat formatter = new StringFormat();
				formatter.Alignment = StringAlignment.Center;
				formatter.LineAlignment = StringAlignment.Near;

				Font font = new Font(e.Value.ToString(), e.Bounds.Height, FontStyle.Regular, GraphicsUnit.Pixel);
				e.Graphics.DrawString(e.Value.ToString(), font, Brushes.Black, (RectangleF)e.Bounds, formatter);

				font.Dispose();
				formatter.Dispose();

				// restore original values
				e.Graphics.Transform = origTransform;
				e.Graphics.SmoothingMode = origSmoothingMode;
				e.Graphics.CompositingQuality = origCompositingQuality;
				e.Graphics.InterpolationMode = origInterpolationmode;
				e.Graphics.TextRenderingHint = origTextRenderingHint;

				base.PaintValue(e);
			}
		}


		public override bool IsDropDownResizable { get { return true; } }
	}


	/// <summary>
	/// nShape UI type editor for entering text.
	/// </summary>
	public class nShapeTextEditor : UITypeEditor {

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null) {
					TextEditorDialog stringEditor = null;
					try {
						if (value is string && !string.IsNullOrEmpty(value as string)) {
							if (value is string && context.Instance is ICaptionedShape) {
								ICaptionedShape labeledShape = (ICaptionedShape)context.Instance;
								for (int i = 0; i < labeledShape.CaptionCount; ++i) {
									if (labeledShape.GetCaptionText(i).Equals(value)) {
										ICharacterStyle characterStyle = labeledShape.GetCaptionCharacterStyle(i);
										stringEditor = new TextEditorDialog(value as string, characterStyle.FontName, characterStyle.SizeInPoints, characterStyle.Style);
									}
								}
							}
							if (stringEditor == null) stringEditor = new TextEditorDialog(value as string);
							if (stringEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
								value = stringEditor.ResultText;
							}
						} else if (value is IEnumerable<string> && value != null) {
							stringEditor = new TextEditorDialog((IEnumerable<string>)value);
							if (stringEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
								value = stringEditor.ResultLines;
							}
						} else {
							stringEditor = new TextEditorDialog();
							if (stringEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
								value = stringEditor.ResultText;
							}
						}
					} finally {
						if (stringEditor != null) stringEditor.Dispose();
						stringEditor = null;
					}
				}
			}
			return value;
		}


		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null) {
				return UITypeEditorEditStyle.Modal;
			}
			return base.GetEditStyle(context);
		}
	}


	/// <summary>
	/// nShape UI type editor for choosing an image and assinging a name.
	/// </summary>
	public class nShapeNamedImageEditor : UITypeEditor {

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null) {
					ImageEditor imageEditor = null;
					try {
						if (value is NamedImage && value != null)
							imageEditor = new ImageEditor((NamedImage)value);
						else imageEditor = new ImageEditor();

						if (imageEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							value = imageEditor.Result;
					} finally {
						if (imageEditor != null) imageEditor.Dispose();
						imageEditor = null;
					}
				}
			}
			return value;
		}


		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.Modal;
			return base.GetEditStyle(context);
		}


		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return true;
		}


		public override void PaintValue(PaintValueEventArgs e) {
			base.PaintValue(e);
			if (e != null && e.Value is NamedImage) {
				NamedImage img = (NamedImage)e.Value;
				if (img.Image != null) {
					Rectangle srcRect = Rectangle.Empty;
					srcRect.X = 0;
					srcRect.Y = 0;
					srcRect.Width = img.Width;
					srcRect.Height = img.Height;

					float lowestRatio = (float)Math.Round(Math.Min((double)(e.Bounds.Width - (e.Bounds.X + e.Bounds.X)) / (double)img.Width, (double)(e.Bounds.Height - (e.Bounds.Y + e.Bounds.Y)) / (double)img.Height), 6);
					Rectangle dstRect = Rectangle.Empty;
					dstRect.Width = (int)Math.Round(srcRect.Width * lowestRatio);
					dstRect.Height = (int)Math.Round(srcRect.Height * lowestRatio);
					dstRect.X = e.Bounds.X + (int)Math.Round((float)(e.Bounds.Width - dstRect.Width) / 2);
					dstRect.Y = e.Bounds.Y + (int)Math.Round((float)(e.Bounds.Height - dstRect.Height) / 2);

					e.Graphics.DrawImage(img.Image, dstRect, srcRect, GraphicsUnit.Pixel);
				}
			}
		}
	}


	/// <summary>
	/// nShape UI type editor for choosing a style from a drop down list.
	/// </summary>
	public class nShapeStyleEditor : UITypeEditor {

		public nShapeStyleEditor() {
			if (designBuffer == null) throw new nShapeInternalException("Design is not set. Set the static property nShapeStyleEditor.Design to a reference of the current design before creating the UI type editor.");
			this.design = designBuffer;
		}


		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (provider != null) {
				IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (editorService != null) {
					// fetch current Design (if changed)
					if (designBuffer != null && designBuffer != design)
						design = designBuffer;

					// fetch edited object (Shape)
					Shape shape = null;
					if (context.Instance is Shape)
						shape = (Shape)context.Instance;

					bool showItemDefaultStyle = (shape != null && shape.Template != null);
					StyleListBox styleListBox = null;
					try {
						Type styleType = null;
						if (value == null) {
							if (context.PropertyDescriptor.PropertyType == typeof(ICapStyle))
								styleType = typeof(CapStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IColorStyle))
								styleType = typeof(ColorStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IFillStyle))
								styleType = typeof(FillStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(ICharacterStyle))
								styleType = typeof(CharacterStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(ILineStyle))
								styleType = typeof(LineStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IParagraphStyle))
								styleType = typeof(ParagraphStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IShapeStyle))
								styleType = typeof(ShapeStyle);
							else
								throw new nShapeException(string.Format("Unsupported Style Type {0}.", context.PropertyDescriptor.PropertyType));

							styleListBox = new StyleListBox(editorService, design, styleType, showItemDefaultStyle);
						} else
							styleListBox = new StyleListBox(editorService, design, value as Style, showItemDefaultStyle);

						editorService.DropDownControl(styleListBox);
						if (styleListBox.SelectedItem is IStyle)
							value = styleListBox.SelectedItem;
						else value = null;
					} finally {
						if (styleListBox != null) styleListBox.Dispose();
						styleListBox = null;
					}
				}
			}
			return value;
		}


		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.DropDown;
			else return base.GetEditStyle(context);
		}


		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return true;
		}


		public override void PaintValue(PaintValueEventArgs e) {
			//base.PaintValue(e);
			if (e != null && e.Value != null) {
				// store original values;
				SmoothingMode origSmoothingMode = e.Graphics.SmoothingMode;
				CompositingQuality origCompositingQuality = e.Graphics.CompositingQuality;
				InterpolationMode origInterpolationmode = e.Graphics.InterpolationMode;
				System.Drawing.Text.TextRenderingHint origTextRenderingHint = e.Graphics.TextRenderingHint;
				Matrix origTransform = e.Graphics.Transform;

				// Set new GraphicsModes
				GdiHelpers.ApplyGraphicsSettings(e.Graphics, nShapeRenderingQuality.MaximumQuality);

				Rectangle previewRect = Rectangle.Empty;
				previewRect.X = e.Bounds.X + 1;
				previewRect.Y = e.Bounds.Y + 1;
				previewRect.Width = (e.Bounds.Right - 1) - (e.Bounds.Left + 1);
				previewRect.Height = (e.Bounds.Bottom - 1) - (e.Bounds.Top + 1);

				if (e.Value is ICapStyle) 
					DrawStyleItem(e.Graphics, previewRect, (ICapStyle)e.Value);
				else if (e.Value is ICharacterStyle)
					DrawStyleItem(e.Graphics, previewRect, (ICharacterStyle)e.Value);
				else if (e.Value is IColorStyle)
					DrawStyleItem(e.Graphics, previewRect, (IColorStyle)e.Value);
				else if (e.Value is IFillStyle)
					DrawStyleItem(e.Graphics, previewRect, (IFillStyle)e.Value);
				else if (e.Value is ILineStyle)
					DrawStyleItem(e.Graphics, previewRect, (ILineStyle)e.Value);
				else if (e.Value is IParagraphStyle)
					DrawStyleItem(e.Graphics, previewRect, (IParagraphStyle)e.Value);
				else if (e.Value is IShapeStyle) {
					// nothing to do				
				}

				// restore original values
				e.Graphics.Transform = origTransform;
				e.Graphics.SmoothingMode = origSmoothingMode;
				e.Graphics.CompositingQuality = origCompositingQuality;
				e.Graphics.InterpolationMode = origInterpolationmode;
				e.Graphics.TextRenderingHint = origTextRenderingHint;
			}
		}


		public override bool IsDropDownResizable {
			get { return true; }
		}


		public static Design Design {
			set { designBuffer = value; }
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, ICapStyle capStyle) {
			Pen capPen = ToolCache.GetPen(design.LineStyles.Normal, null, capStyle);
			float scale = 1;
			if (capStyle.CapSize + 2 >= previewBounds.Height) {
				scale = Geometry.CalcScaleFactor(capStyle.CapSize + 2, capStyle.CapSize + 2, previewBounds.Width - 2, previewBounds.Height - 2);
				gfx.ScaleTransform(scale, scale);
			}
			int startX, endX, y;
			startX = previewBounds.Left;
			if (capPen.StartCap == LineCap.Custom)
				startX += (int)Math.Round(capStyle.CapSize - capPen.CustomStartCap.BaseInset);
			startX = (int)Math.Round(startX / scale);
			endX = previewBounds.Right;
			if (capPen.EndCap == LineCap.Custom)
				endX -= (int)Math.Round(capStyle.CapSize - capPen.CustomEndCap.BaseInset);
			endX = (int)Math.Round(endX / scale);
			y = (int)Math.Round((previewBounds.Y + ((float)previewBounds.Height / 2)) / scale);
			gfx.DrawLine(capPen, startX, y, endX, y);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, ICharacterStyle charStyle) {
			Brush fontBrush = ToolCache.GetBrush(charStyle.ColorStyle);
			Font font = ToolCache.GetFont(charStyle);
			int height = Geometry.PointToPixel(charStyle.SizeInPoints, gfx.DpiY);
			float scale = Geometry.CalcScaleFactor(height, height, previewBounds.Width, previewBounds.Height);
			gfx.ScaleTransform(scale, scale);
			RectangleF layoutRect = RectangleF.Empty;
			layoutRect.X = 0;
			layoutRect.Y = 0;
			layoutRect.Width = (float)previewBounds.Width / scale;
			layoutRect.Height = (float)previewBounds.Height / scale;
			gfx.DrawString(string.Format("{0} {1} pt", charStyle.FontName, charStyle.SizeInPoints), 
				font, fontBrush, layoutRect, formatter);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, IColorStyle colorStyle) {
			Brush brush = ToolCache.GetBrush(colorStyle);
			gfx.FillRectangle(brush, previewBounds);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, IFillStyle fillStyle) {
			Brush fillBrush = ToolCache.GetBrush(fillStyle);
			// Transform
			if (fillBrush is LinearGradientBrush) {
				float srcGradLen = ((LinearGradientBrush)fillBrush).Rectangle.Width;
				float dstGradLen = previewBounds.Width / (float)Math.Cos(Geometry.DegreesToRadians(fillStyle.GradientAngle));
				float scale = dstGradLen / srcGradLen;
				((LinearGradientBrush)fillBrush).ResetTransform();
				((LinearGradientBrush)fillBrush).ScaleTransform(scale, scale);
				((LinearGradientBrush)fillBrush).RotateTransform(fillStyle.GradientAngle);
			} else if (fillBrush is TextureBrush) {
				if (fillStyle.ImageLayout == nShapeImageLayout.Stretch) {
					float scaleX = (float)previewBounds.Width / ((TextureBrush)fillBrush).Image.Width;
					float scaleY = (float)previewBounds.Height / ((TextureBrush)fillBrush).Image.Height;
					((TextureBrush)fillBrush).ScaleTransform(scaleX, scaleY);
				} else {
					float scale = Geometry.CalcScaleFactor(((TextureBrush)fillBrush).Image.Width, ((TextureBrush)fillBrush).Image.Height, previewBounds.Width, previewBounds.Height);
					((TextureBrush)fillBrush).ScaleTransform(scale, scale);
					((TextureBrush)fillBrush).TranslateTransform((((TextureBrush)fillBrush).Image.Width * scale) / 2, (((TextureBrush)fillBrush).Image.Height * scale) / 2);
				}
			}
			// Draw
			if (fillBrush != Brushes.Transparent) gfx.FillRectangle(fillBrush, previewBounds);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, ILineStyle lineStyle) {
			Pen linePen = ToolCache.GetPen(lineStyle, null, null);
			int height = lineStyle.LineWidth + 2;
			if (height > previewBounds.Height) {
				float scale = Geometry.CalcScaleFactor(height, height, previewBounds.Width, previewBounds.Height);
				linePen.ScaleTransform(scale, scale);
			}
			gfx.DrawLine(linePen, previewBounds.X, previewBounds.Y + (previewBounds.Height / 2), previewBounds.Right, previewBounds.Y + (previewBounds.Height / 2));
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, IParagraphStyle paragraphStyle) {
			StringFormat stringFormat = ToolCache.GetStringFormat(paragraphStyle);
			Rectangle r = Rectangle.Empty;
			r.X = previewBounds.X + paragraphStyle.Padding.Left;
			r.Y = previewBounds.Y + paragraphStyle.Padding.Top;
			r.Width = (previewBounds.Width * 10) - (paragraphStyle.Padding.Left + paragraphStyle.Padding.Right);
			r.Height = (previewBounds.Height * 10) - (paragraphStyle.Padding.Top + paragraphStyle.Padding.Bottom);

			// Transform
			float scale = Geometry.CalcScaleFactor(r.Width, r.Height, previewBounds.Width, previewBounds.Height);
			gfx.ScaleTransform(scale, scale);

			// Draw
			const string previewText = "This is the first line of the sample text.\r\nThis is line 2 of the text.\r\nLine 3 of the text.\r\nLine four of this text is a little longer for demonstrating the WordWrap property setting.";
			gfx.DrawString(previewText, Control.DefaultFont, Brushes.Black, r, stringFormat);
		}


		#region Fields

		private static Design designBuffer;
		private Design design;
		private StringFormat formatter = new StringFormat(StringFormatFlags.NoWrap);

		#endregion
	}

	#endregion

}
