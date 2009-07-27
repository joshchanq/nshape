using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace Dataweb.Diagramming.Advanced {

	// Based on code from http://www.dotnet247.com/247reference/msgs/23/118514.aspx
	public static class EmfHelper {
		[DllImport("user32.dll")]
		static extern bool OpenClipboard(IntPtr hWndNewOwner);

		[DllImport("user32.dll")]
		static extern bool EmptyClipboard();

		[DllImport("user32.dll")]
		static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

		[DllImport("user32.dll")]
		static extern bool CloseClipboard();

		[DllImport("gdi32.dll")]
		static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string fileName);

		[DllImport("gdi32.dll")]
		static extern bool DeleteEnhMetaFile(IntPtr hemf);

		// Metafile mf is set to an invalid State inside this function
		static public bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile metafile) {
			if (metafile == null) throw new ArgumentNullException("metafile");
			bool bResult = false;
			IntPtr hEMF, hEMF2;
			hEMF = metafile.GetHenhmetafile(); // invalidates mf
			if (!hEMF.Equals(IntPtr.Zero)) {
				hEMF2 = CopyEnhMetaFile(hEMF, null);
				if (!hEMF2.Equals(IntPtr.Zero)) {
					if (OpenClipboard(hWnd)) {
						if (EmptyClipboard()) {
							IntPtr hRes = SetClipboardData(14 /*CF_ENHMETAFILE*/, hEMF2);
							bResult = hRes.Equals(hEMF2);
							CloseClipboard();
						}
					}
				}
				DeleteEnhMetaFile(hEMF);
			}
			return bResult;
		}


		// Metafile mf is set to an invalid State inside this function
		static public bool SaveEnhMetaFile(string fileName, Metafile metafile){
			if (metafile == null) throw new ArgumentNullException("metafile");
			bool result = false;
			IntPtr hEmf = metafile.GetHenhmetafile();
			if (hEmf != IntPtr.Zero) {
				IntPtr resHEnh = CopyEnhMetaFile(hEmf, fileName);
				if (resHEnh != IntPtr.Zero) {
					DeleteEnhMetaFile(resHEnh);
					result = true;
				}
				DeleteEnhMetaFile(hEmf);
				metafile.Dispose();
			}
			return result;
		}
	}
}
