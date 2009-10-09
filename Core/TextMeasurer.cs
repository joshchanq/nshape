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
using System.Drawing;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Measures the extent of strings.
	/// </summary>
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
	}
}