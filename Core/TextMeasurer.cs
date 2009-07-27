using System;
using System.Drawing;
using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming.Advanced {

	public class TextMeasurer {

		/// <summary>
		/// Measures the given text and returns its size.
		/// </summary>
		/// <param name="text">The text to measure</param>
		/// <param name="font">The text's font</param>
		/// <param name="proposedSize">The layout area of the text. Size.Empty means no fitting in area.</param>
		/// <param name="paragraphStyle">The paragraph layout of the text</param>
		/// <returns></returns>
		public static Size MeasureText(string text, Font font, Size proposedSize, IParagraphStyle paragraphStyle) {
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
				return MeasureText(graphics, text, font, proposedSize, ToolCache.GetStringFormat(paragraphStyle));
		}


		/// <summary>
		/// Measures the given text and returns its size.
		/// </summary>
		/// <param name="text">The text to measure</param>
		/// <param name="font">The text's font</param>
		/// <param name="proposedSize">The layout area of the text. Size.Empty means no fitting in area.</param>
		/// <param name="format">StringFormat object defining the layout of the text</param>
		/// <returns></returns>
		public static Size MeasureText(string text, Font font, Size proposedSize, StringFormat format) {
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
				return MeasureText(graphics, text, font, proposedSize, format);
		}


		/// <summary>
		/// Measures the given text and returns its size.
		/// </summary>
		/// <param name="graphics">Graphics context used for the measuring</param>
		/// <param name="text">The text to measure</param>
		/// <param name="font">The text's font</param>
		/// <param name="proposedSize">The layout area of the text. Size.Empty means no fitting in area.</param>
		/// <param name="paragraphStyle">The paragraph layout of the text</param>
		/// <returns></returns>
		public static Size MeasureText(Graphics graphics, string text, Font font, Size proposedSize, IParagraphStyle paragraphStyle) {
			return MeasureText(graphics, text, font, proposedSize, ToolCache.GetStringFormat(paragraphStyle));
		}


		/// <summary>
		/// Measures the given text and returns its size.
		/// </summary>
		/// <param name="graphics">Graphics context used for the measuring</param>
		/// <param name="text">The text to measure</param>
		/// <param name="font">The text's font</param>
		/// <param name="proposedSize">The layout area of the text. Size.Empty means no fitting in area.</param>
		/// <param name="format">StringFormat object defining the layout of the text</param>
		/// <returns></returns>
		public static Size MeasureText(Graphics graphics, string text, Font font, Size proposedSize, StringFormat format) {
			if (text == null) throw new ArgumentNullException("text");
			if (font == null) throw new ArgumentNullException("font");
			if (format == null) throw new ArgumentNullException("format");
			// Add Flag "Measure Trailing Spaces" if not set
			bool flagAdded = false;
			if ((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) == 0) {
				format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
				flagAdded = true;
			}

			Size result = Size.Empty;
			if (proposedSize == Size.Empty)
				result = Size.Ceiling(graphics.MeasureString(text, font, PointF.Empty, format));
			else result = Size.Ceiling(graphics.MeasureString(text, font, proposedSize, format));

			// remove flag if added before
			if (flagAdded) format.FormatFlags ^= StringFormatFlags.MeasureTrailingSpaces;

			return result;
		}


		//public static Size MeasureText(Graphics graphics, string text, Font font, Size proposedSize, IParagraphStyle paragraphStyle) {
		//   Size result = Size.Empty;

		//   CharacterRange[] characterRanges = { new CharacterRange(0, text.Length) };
		//   RectangleF layoutRect = new RectangleF(0, 0, proposedSize.Width, proposedSize.Height);
		//   if (layoutRect == RectangleF.Empty) {
		//      layoutRect.Width = int.MaxValue;
		//      layoutRect.Height = int.MaxValue;
		//   }

		//   // Set string format.
		//   StringFormat stringFormat = ParagraphStyleToStringFormat(paragraphStyle);
		//   stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		//   stringFormat.SetMeasurableCharacterRanges(characterRanges);

		//   Region[] stringRegions = graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat);
		//   float resultWidth = 0;
		//   float resultHeight = 0;
		//   for (int i = 0; i < stringRegions.Length; ++i) {
		//      RectangleF bounds = stringRegions[i].GetBounds(graphics);
		//      resultWidth += bounds.Width;
		//      resultHeight += bounds.Height;
		//   }
		//   result.Width = (int)Math.Ceiling(resultWidth);
		//   result.Height = (int)Math.Ceiling(resultHeight);

		//   return result;
		//}
	}

}
