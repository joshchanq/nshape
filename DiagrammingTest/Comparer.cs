using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dataweb.nShape;
using Dataweb.nShape.Advanced;
using Dataweb.nShape.GeneralShapes;


namespace nShapeTest {
	
	public class nShapeComparer {

		public static void CompareId(IEntity savedEntity, IEntity loadedEntity) {
			Assert.AreEqual<bool>(savedEntity != null, loadedEntity != null);
			if (savedEntity != null && loadedEntity != null) {
				Assert.IsNotNull(savedEntity.Id);
				Assert.IsNotNull(loadedEntity.Id);
				Assert.AreEqual(savedEntity.Id, loadedEntity.Id);
			}
		}


		#region Compare designs and styles

		public static void Compare(IEnumerable<Design> saved, IEnumerable<Design> loaded) {
			Dictionary<object, Design> loadedDesigns = new Dictionary<object, Design>();
			foreach (Design d in loaded) loadedDesigns.Add(((IEntity)d).Id, d);

			foreach (Design savedDesign in saved) {
				object id = ((IEntity)savedDesign).Id;
				Design loadedDesign = null;
				Assert.IsTrue(loadedDesigns.TryGetValue(id, out loadedDesign));
				
				Assert.AreEqual<string>(savedDesign.Name, loadedDesign.Name);
				Compare(savedDesign, loadedDesign);
			}
		}


		public static void Compare(IStyleSet savedDesign, IStyleSet loadedDesign) {
			foreach (ICapStyle savedStyle in savedDesign.CapStyles) {
				ICapStyle loadedStyle = loadedDesign.CapStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle);
			}
			foreach (ICharacterStyle savedStyle in savedDesign.CharacterStyles) {
				ICharacterStyle loadedStyle = loadedDesign.CharacterStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle);
			}
			foreach (IColorStyle savedStyle in savedDesign.ColorStyles) {
				IColorStyle loadedStyle = loadedDesign.ColorStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle);
			}
			foreach (IFillStyle savedStyle in savedDesign.FillStyles) {
				IFillStyle loadedStyle = loadedDesign.FillStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle);
			}
			foreach (ILineStyle savedStyle in savedDesign.LineStyles) {
				ILineStyle loadedStyle = loadedDesign.LineStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle);
			}
			foreach (IParagraphStyle savedStyle in savedDesign.ParagraphStyles) {
				IParagraphStyle loadedStyle = loadedDesign.ParagraphStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle);
			}
			//foreach (IShapeStyle savedStyle in savedDesign.ShapeStyles) {
			//   IShapeStyle loadedStyle = loadedDesign.ShapeStyles[savedStyle.Name];
			//   Compare(savedStyle, loadedStyle);
			//}
		}


		public static void Compare(IStyle savedStyle, IStyle loadedStyle) {
			if (savedStyle is ICapStyle && loadedStyle is ICapStyle)
				Compare((ICapStyle)savedStyle, (ICapStyle)loadedStyle);
			else if (savedStyle is ICharacterStyle && loadedStyle is ICharacterStyle)
				Compare((ICharacterStyle)savedStyle, (ICharacterStyle)loadedStyle);
			else if (savedStyle is IColorStyle && loadedStyle is IColorStyle)
				Compare((IColorStyle)savedStyle, (IColorStyle)loadedStyle);
			else if (savedStyle is IFillStyle && loadedStyle is IFillStyle)
				Compare((IFillStyle)savedStyle, (IFillStyle)loadedStyle);
			else if (savedStyle is ILineStyle && loadedStyle is ILineStyle)
				Compare((ILineStyle)savedStyle, (ILineStyle)loadedStyle);
			else if (savedStyle is IParagraphStyle && loadedStyle is IParagraphStyle)
				Compare((IParagraphStyle)savedStyle, (IParagraphStyle)loadedStyle);
			else if (savedStyle is IShapeStyle && loadedStyle is IShapeStyle)
				Compare((IShapeStyle)savedStyle, (IShapeStyle)loadedStyle);
			else Assert.Fail("Different types.");
		}
		
		
		public static void Compare(ICapStyle savedStyle, ICapStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Compare(savedStyle.ColorStyle, loadedStyle.ColorStyle);
			Assert.AreEqual<CapShape>(savedStyle.CapShape, loadedStyle.CapShape);
			Assert.AreEqual<short>(savedStyle.CapSize, loadedStyle.CapSize);
		}


		public static void Compare(ICharacterStyle savedStyle, ICharacterStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Compare(savedStyle.ColorStyle, loadedStyle.ColorStyle);
			Assert.AreEqual<FontFamily>(savedStyle.FontFamily, loadedStyle.FontFamily);
			Assert.AreEqual<string>(savedStyle.FontName, loadedStyle.FontName);
			Assert.AreEqual<int>(savedStyle.Size, loadedStyle.Size);
			Assert.AreEqual<float>(savedStyle.SizeInPoints, loadedStyle.SizeInPoints);
			Assert.AreEqual<FontStyle>(savedStyle.Style, loadedStyle.Style);
		}


		public static void Compare(IColorStyle savedStyle, IColorStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Assert.AreEqual<int>(savedStyle.Color.ToArgb(), loadedStyle.Color.ToArgb());
			Assert.AreEqual<bool>(savedStyle.ConvertToGray, loadedStyle.ConvertToGray);
			Assert.AreEqual<byte>(savedStyle.Transparency, loadedStyle.Transparency);
		}


		public static void Compare(IFillStyle savedStyle, IFillStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Compare(savedStyle.BaseColorStyle, loadedStyle.BaseColorStyle);
			Compare(savedStyle.AdditionalColorStyle, loadedStyle.AdditionalColorStyle);
			Assert.AreEqual<bool>(savedStyle.ConvertToGrayScale, loadedStyle.ConvertToGrayScale);
			Assert.AreEqual<FillMode>(savedStyle.FillMode, loadedStyle.FillMode);
			Assert.AreEqual<System.Drawing.Drawing2D.HatchStyle>(savedStyle.FillPattern, loadedStyle.FillPattern);
			Assert.AreEqual<short>(savedStyle.GradientAngle, loadedStyle.GradientAngle);
			//Assert.AreEqual<Dataweb.Utilities.NamedImage>(savedStyle.Image, loadedStyle.Image);
			Assert.AreEqual<byte>(savedStyle.ImageCompressionQuality, loadedStyle.ImageCompressionQuality);
			Assert.AreEqual<float>(savedStyle.ImageGammaCorrection, loadedStyle.ImageGammaCorrection);
			Assert.AreEqual<nShapeImageLayout>(savedStyle.ImageLayout, loadedStyle.ImageLayout);
			Assert.AreEqual<byte>(savedStyle.ImageTransparency, loadedStyle.ImageTransparency);
		}


		public static void Compare(ILineStyle savedStyle, ILineStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Compare(savedStyle.ColorStyle, loadedStyle.ColorStyle);
			Assert.AreEqual<System.Drawing.Drawing2D.DashCap>(savedStyle.DashCap, loadedStyle.DashCap);
			Assert.IsNotNull(savedStyle.DashPattern);
			Assert.IsNotNull(loadedStyle.DashPattern);
			Assert.AreEqual<int>(savedStyle.DashPattern.Length, loadedStyle.DashPattern.Length);
			for (int i = savedStyle.DashPattern.Length - 1; i >= 0; --i)
				Assert.AreEqual<float>(savedStyle.DashPattern[i], loadedStyle.DashPattern[i]);
			Assert.AreEqual<DashType>(savedStyle.DashType, loadedStyle.DashType);
			Assert.AreEqual<System.Drawing.Drawing2D.LineJoin>(savedStyle.LineJoin, loadedStyle.LineJoin);
			Assert.AreEqual<int>(savedStyle.LineWidth, loadedStyle.LineWidth);
		}


		public static void Compare(IParagraphStyle savedStyle, IParagraphStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Assert.AreEqual<ContentAlignment>(savedStyle.Alignment, loadedStyle.Alignment);
			Assert.AreEqual<TextPadding>(savedStyle.Padding, loadedStyle.Padding);
			Assert.AreEqual<StringTrimming>(savedStyle.Trimming, loadedStyle.Trimming);
			Assert.AreEqual<bool>(savedStyle.WordWrap, loadedStyle.WordWrap);
		}


		public static void Compare(IShapeStyle savedStyle, IShapeStyle loadedStyle) {
			CompareBaseStyle(savedStyle, loadedStyle);
			Compare(savedStyle.ShadowColor, loadedStyle.ShadowColor);
			Assert.AreEqual<bool>(savedStyle.RoundedCorners, loadedStyle.RoundedCorners);
			Assert.AreEqual<bool>(savedStyle.ShowGradients, loadedStyle.ShowGradients);
			Assert.AreEqual<bool>(savedStyle.ShowShadows, loadedStyle.ShowShadows);
		}

		#endregion


		#region Compare templates

		public static void Compare(Template savedTemplate, Template loadedTemplate) {
			CompareId(savedTemplate, loadedTemplate);
			CompareString(savedTemplate.Description, loadedTemplate.Description, false);
			CompareString(savedTemplate.Name, loadedTemplate.Name, false);
			CompareString(savedTemplate.Title, loadedTemplate.Title, false);
			foreach (IModelMapping savedMapping in savedTemplate.GetPropertyMappings()) {
				IModelMapping loadedMapping = loadedTemplate.GetPropertyMapping(savedMapping.ModelPropertyId);
				Compare(savedMapping, loadedMapping);
			}
			Compare(savedTemplate.Shape, loadedTemplate.Shape);
			foreach (ControlPointId ptId in savedTemplate.Shape.GetControlPointIds(ControlPointCapabilities.All)) {
				Assert.AreEqual<TerminalId>(savedTemplate.GetMappedTerminalId(ptId), loadedTemplate.GetMappedTerminalId(ptId));
				Assert.AreEqual<string>(savedTemplate.GetMappedTerminalName(ptId), loadedTemplate.GetMappedTerminalName(ptId));
			}
		}


		public static void Compare(IModelMapping savedMapping, IModelMapping loadedMapping) {
			CompareId(savedMapping, loadedMapping);
			Assert.AreEqual<int>(savedMapping.ModelPropertyId, loadedMapping.ModelPropertyId);
			Assert.AreEqual<int>(savedMapping.ShapePropertyId, loadedMapping.ShapePropertyId);
			if (savedMapping is NumericModelMapping && loadedMapping is NumericModelMapping)
				Compare((NumericModelMapping)savedMapping, (NumericModelMapping)loadedMapping);
			else if (savedMapping is FormatModelMapping && loadedMapping is FormatModelMapping)
				Compare((FormatModelMapping)savedMapping, (FormatModelMapping)loadedMapping);
			else if (savedMapping is StyleModelMapping && loadedMapping is StyleModelMapping)
				Compare((StyleModelMapping)savedMapping, (StyleModelMapping)loadedMapping);
			else Assert.Fail("saved and loaded model mapping are of different types");
		}

	
		public static void Compare(NumericModelMapping savedMapping, NumericModelMapping loadedMapping) {
			Assert.AreEqual<NumericModelMappingType>(savedMapping.MappingType, loadedMapping.MappingType);
			Assert.AreEqual<float>(savedMapping.Intercept, loadedMapping.Intercept);
			Assert.AreEqual<float>(savedMapping.Slope, loadedMapping.Slope);
		}


		public static void Compare(FormatModelMapping savedMapping, FormatModelMapping loadedMapping) {
			Assert.AreEqual<FormatModelMappingType>(savedMapping.MappingType, loadedMapping.MappingType);
			Assert.AreEqual<string>(savedMapping.Format, loadedMapping.Format);
		}


		public static void Compare(StyleModelMapping savedMapping, StyleModelMapping loadedMapping) {
			Assert.AreEqual<StyleModelMappingType>(savedMapping.MappingType, loadedMapping.MappingType);
			Assert.AreEqual<int>(savedMapping.ValueRangeCount, loadedMapping.ValueRangeCount);
			if (savedMapping.MappingType == StyleModelMappingType.IntegerStyle) {
				List<int> savedRanges = new List<int>(savedMapping.ValueRangeCount);
				foreach (object obj in savedMapping.ValueRanges) savedRanges.Add((int)obj);
				List<int> loadedRanges = new List<int>(loadedMapping.ValueRangeCount);
				foreach (object obj in loadedMapping.ValueRanges) loadedRanges.Add((int)obj);
				for (int i = savedMapping.ValueRangeCount - 1; i >= 0; --i)
					Assert.AreEqual<int>(savedRanges[i], loadedRanges[i]);
			} else if (savedMapping.MappingType == StyleModelMappingType.IntegerStyle) {
				List<float> savedRanges = new List<float>(savedMapping.ValueRangeCount);
				foreach (object obj in savedMapping.ValueRanges) savedRanges.Add((float)obj);
				List<float> loadedRanges = new List<float>(loadedMapping.ValueRangeCount);
				foreach (object obj in loadedMapping.ValueRanges) loadedRanges.Add((float)obj);
				for (int i = savedMapping.ValueRangeCount - 1; i >= 0; --i)
					Assert.AreEqual<float>(savedRanges[i], loadedRanges[i]);
			} else Assert.Fail("Unsupported mapping type");
		}

		#endregion


		#region Compare diagrams

		public static void Compare(Diagram savedDiagram, Diagram loadedDiagram) {
			Assert.AreEqual<bool>(savedDiagram != null, loadedDiagram != null);
			if (savedDiagram != null && loadedDiagram != null) {
				CompareId(savedDiagram, loadedDiagram);
				Assert.AreEqual<int>(savedDiagram.BackgroundColor.ToArgb(), loadedDiagram.BackgroundColor.ToArgb());
				Assert.AreEqual<int>(savedDiagram.BackgroundGradientColor.ToArgb(), loadedDiagram.BackgroundGradientColor.ToArgb());
				CompareString(savedDiagram.BackgroundImage.Name, loadedDiagram.BackgroundImage.Name, false);
				Assert.AreEqual<int>(savedDiagram.BackgroundImage.Width, loadedDiagram.BackgroundImage.Width);
				Assert.AreEqual<int>(savedDiagram.BackgroundImage.Height, loadedDiagram.BackgroundImage.Height);
				Assert.AreEqual<float>(savedDiagram.BackgroundImageGamma, loadedDiagram.BackgroundImageGamma);
				Assert.AreEqual<bool>(savedDiagram.BackgroundImageGrayScale, loadedDiagram.BackgroundImageGrayScale);
				Assert.AreEqual<nShapeImageLayout>(savedDiagram.BackgroundImageLayout, loadedDiagram.BackgroundImageLayout);
				Assert.AreEqual<byte>(savedDiagram.BackgroundImageTransparency, loadedDiagram.BackgroundImageTransparency);
				Assert.AreEqual<int>(savedDiagram.BackgroundImageTransparentColor.ToArgb(), loadedDiagram.BackgroundImageTransparentColor.ToArgb());
				Assert.AreEqual<IDisplayService>(savedDiagram.DisplayService, loadedDiagram.DisplayService);
				Assert.AreEqual<int>(savedDiagram.Height, loadedDiagram.Height);
				Assert.AreEqual<int>(savedDiagram.Width, loadedDiagram.Width);
				Assert.AreEqual<bool>(savedDiagram.HighQualityRendering, loadedDiagram.HighQualityRendering);
				CompareString(savedDiagram.Name, loadedDiagram.Name, false);
				CompareString(savedDiagram.Title, loadedDiagram.Title, false);
				//
				// Compare Layers
				Assert.AreEqual<int>(savedDiagram.Layers.Count, loadedDiagram.Layers.Count);
				SortedList<LayerIds, Layer> savedLayers = new SortedList<LayerIds, Layer>();
				foreach (Layer l in savedDiagram.Layers) savedLayers.Add(l.Id, l);
				SortedList<LayerIds, Layer> loadedLayers = new SortedList<LayerIds, Layer>();
				foreach (Layer l in loadedDiagram.Layers) loadedLayers.Add(l.Id, l);
				foreach (KeyValuePair<LayerIds, Layer> pair in savedLayers) {
					Layer loadedLayer = loadedLayers[pair.Key];
					// Compare layers
					Assert.AreEqual<LayerIds>(pair.Value.Id, loadedLayer.Id);
					Assert.AreEqual<int>(pair.Value.LowerZoomThreshold, loadedLayer.LowerZoomThreshold);
					Assert.AreEqual<int>(pair.Value.UpperZoomThreshold, loadedLayer.UpperZoomThreshold);
					CompareString(pair.Value.Name, loadedLayer.Name, false);
					CompareString(pair.Value.Title, loadedLayer.Title, false);
				}
				//
				// Compare Shapes
				IEnumerator<Shape> savedShapes = savedDiagram.Shapes.BottomUp.GetEnumerator();
				IEnumerator<Shape> loadedShapes = loadedDiagram.Shapes.BottomUp.GetEnumerator();
				Assert.AreEqual<int>(savedDiagram.Shapes.Count, loadedDiagram.Shapes.Count);
				for (int i = savedDiagram.Shapes.Count; i >= 0; --i) {
					Compare(savedShapes.Current, loadedShapes.Current);
					Assert.AreEqual(savedShapes.MoveNext(), loadedShapes.MoveNext());
				}
			}
		}

		#endregion


		#region Compare shapes

		public static void Compare(Shape savedShape, Shape loadedShape) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				// Compare base properties
				Assert.AreEqual<string>(savedShape.Type.FullName, loadedShape.Type.FullName);
				CompareId(savedShape, loadedShape);
				CompareId(savedShape.Diagram, loadedShape.Diagram);
				CompareId(savedShape.Template, loadedShape.Template);
				Assert.AreEqual<LayerIds>(savedShape.Layers, loadedShape.Layers);
				Assert.AreEqual<IDisplayService>(savedShape.DisplayService, loadedShape.DisplayService);
				Assert.AreEqual<Rectangle>(savedShape.GetBoundingRectangle(true), loadedShape.GetBoundingRectangle(true));
				Assert.AreEqual<Rectangle>(savedShape.GetBoundingRectangle(false), loadedShape.GetBoundingRectangle(false));
				Compare(savedShape.LineStyle, loadedShape.LineStyle);
				Compare(savedShape.ModelObject, loadedShape.ModelObject);
				Compare(savedShape.Parent, loadedShape.Parent);
				Assert.AreEqual<char>(savedShape.SecurityDomainName, loadedShape.SecurityDomainName);
				Assert.AreEqual<bool>(savedShape.Tag != null, loadedShape.Tag != null);
				CompareString(savedShape.Type.FullName, loadedShape.Type.FullName, true);
				Assert.AreEqual<int>(savedShape.X, loadedShape.X);
				Assert.AreEqual<int>(savedShape.Y, loadedShape.Y);
				// 
				// Compare ZOrder and Layers
				// ToDo: Implement this
				//
				// Compare children
				Assert.AreEqual<int>(savedShape.Children.Count, loadedShape.Children.Count);
				IEnumerator<Shape> savedChildren = savedShape.Children.GetEnumerator();
				IEnumerator<Shape> loadedChildren = loadedShape.Children.GetEnumerator();
				for (int i = savedShape.Children.Count - 1; i >= 0; --i) {
					Compare(savedChildren.Current, loadedChildren.Current);
					Assert.AreEqual<bool>(savedChildren.MoveNext(), loadedChildren.MoveNext());
				}
				//
				// Compare connections
				bool savedShapeIsConnected = (savedShape.IsConnected(ControlPointId.Any, null) != ControlPointId.None);
				bool loadedShapeIsConnected = (loadedShape.IsConnected(ControlPointId.Any, null) != ControlPointId.None);
				Assert.AreEqual<bool>(savedShapeIsConnected, loadedShapeIsConnected);
				if (savedShapeIsConnected && loadedShapeIsConnected) {
					List<ShapeConnectionInfo> savedConnections = new List<ShapeConnectionInfo>(savedShape.GetConnectionInfos(ControlPointId.Any, null));
					List<ShapeConnectionInfo> loadedConnections = new List<ShapeConnectionInfo>(loadedShape.GetConnectionInfos(ControlPointId.Any, null));
					Assert.AreEqual<int>(savedConnections.Count, loadedConnections.Count);
					for (int sIdx = savedConnections.Count - 1; sIdx >= 0; --sIdx) {
						bool connectionFound = false;
						for (int lIdx = loadedConnections.Count - 1; lIdx >= 0; --lIdx) {
							IEntity saved = (IEntity)savedConnections[sIdx].OtherShape;
							IEntity loaded = (IEntity)loadedConnections[lIdx].OtherShape;
							Assert.IsNotNull(saved);
							Assert.IsNotNull(loaded);
							Assert.IsNotNull(saved.Id);
							Assert.IsNotNull(loaded.Id);
							if (saved.Id.Equals(loaded.Id)) {
								bool ownPointIsEqual = false;
								bool otherPointIsEqual = false;
								if (savedShape.HasControlPointCapability(savedConnections[sIdx].OwnPointId, ControlPointCapabilities.Glue)) {
									Point savedPoint = savedShape.GetControlPointPosition(savedConnections[sIdx].OwnPointId);
									Point loadedPoint = loadedShape.GetControlPointPosition(loadedConnections[lIdx].OwnPointId);
									ownPointIsEqual = (savedPoint == loadedPoint);
									otherPointIsEqual = (savedConnections[sIdx].OtherPointId == loadedConnections[lIdx].OtherPointId);
								} else {
									ownPointIsEqual = (savedConnections[sIdx].OwnPointId == loadedConnections[lIdx].OwnPointId);
									Point savedPoint = savedConnections[sIdx].OtherShape.GetControlPointPosition(savedConnections[sIdx].OtherPointId);
									Point loadedPoint = loadedConnections[lIdx].OtherShape.GetControlPointPosition(loadedConnections[lIdx].OtherPointId);
									otherPointIsEqual = (savedPoint == loadedPoint);
								}
								if (ownPointIsEqual && otherPointIsEqual) {
									connectionFound = true;
									break;
								}
							}
						}
						Assert.IsTrue(connectionFound);
					}
				}
				//
				// Compare specific properties
				Assert.AreEqual<bool>(savedShape is ILinearShape, loadedShape is ILinearShape);
				if (savedShape is ILinearShape && loadedShape is ILinearShape)
					Compare((ILinearShape)savedShape, (ILinearShape)loadedShape);
				Assert.AreEqual<bool>(savedShape is IPlanarShape, loadedShape is IPlanarShape);
				if (savedShape is IPlanarShape && loadedShape is IPlanarShape)
					Compare((IPlanarShape)savedShape, (IPlanarShape)loadedShape);
				Assert.AreEqual<bool>(savedShape is ICaptionedShape, loadedShape is ICaptionedShape);
				if (savedShape is ICaptionedShape && loadedShape is ICaptionedShape)
					Compare((ICaptionedShape)savedShape, (ICaptionedShape)loadedShape);
			}
		}


		public static void Compare(ILinearShape savedShape, ILinearShape loadedShape) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				Assert.AreEqual<bool>(savedShape.IsDirected, loadedShape.IsDirected);
				Assert.AreEqual<int>(savedShape.MaxVertexCount, loadedShape.MaxVertexCount);
				Assert.AreEqual<int>(savedShape.MinVertexCount, loadedShape.MinVertexCount);
				Assert.AreEqual<int>(savedShape.VertexCount, loadedShape.VertexCount);
			}
		}


		private static void Compare(IPlanarShape savedShape, IPlanarShape loadedShape) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				Assert.AreEqual<int>(savedShape.Angle, loadedShape.Angle);
				Compare(savedShape.FillStyle, loadedShape.FillStyle);
			}
		}


		private static void Compare(ICaptionedShape savedShape, ICaptionedShape loadedShape) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				Assert.AreEqual<int>(savedShape.CaptionCount, loadedShape.CaptionCount);
				for (int i = savedShape.CaptionCount - 1; i >= 0; --i) {
					Compare(savedShape.GetCaptionCharacterStyle(i), loadedShape.GetCaptionCharacterStyle(i));
					Compare(savedShape.GetCaptionParagraphStyle(i), loadedShape.GetCaptionParagraphStyle(i));
					CompareString(savedShape.GetCaptionText(i), loadedShape.GetCaptionText(i), false);
				}
			}
		}

		#endregion


		#region Compare model objects

		public static void Compare(IModelObject savedModelObject, IModelObject loadedModelObject) {
			Assert.AreEqual<bool>(savedModelObject != null, loadedModelObject != null);
			if (savedModelObject != null && loadedModelObject != null) {
			}
		}

		#endregion


		private static void CompareBaseStyle(IStyle savedStyle, IStyle loadedStyle) {
			CompareId(savedStyle, loadedStyle);
			Assert.AreEqual<string>(savedStyle.Name, loadedStyle.Name);
			Assert.AreEqual<string>(savedStyle.Title, loadedStyle.Title);
		}


		private static void CompareString(string savedString, string loadedString, bool exact) {
			if (exact)
				Assert.AreEqual<string>(savedString, loadedString);
			else {
				if (!string.IsNullOrEmpty(savedString) && !string.IsNullOrEmpty(loadedString))
					Assert.IsTrue(savedString.Equals(loadedString, StringComparison.InvariantCultureIgnoreCase));
				else Assert.IsTrue(string.IsNullOrEmpty(savedString) == string.IsNullOrEmpty(loadedString));
			}
		}

	}
}
