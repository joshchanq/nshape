/******************************************************************************
  Copyright 2009-2011 dataweb GmbH
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
using System.Drawing;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NShapeTest {
	
	public class RepositoryComparer {

		public static void CompareId(IEntity savedEntity, IEntity loadedEntity) {
			Assert.AreEqual<bool>(savedEntity != null, loadedEntity != null);
			if (savedEntity != null && loadedEntity != null) {
				Assert.IsNotNull(savedEntity.Id);
				Assert.IsNotNull(loadedEntity.Id);
				Assert.AreEqual(savedEntity.Id, loadedEntity.Id);
			}
		}


		public static void Compare(Project savedProject, Project loadedProject) {
			IRepository savedRepository = savedProject.Repository;
			IRepository loadedRepository = loadedProject.Repository;
			
			// Compare versions
			Assert.AreEqual<int>(savedRepository.Version, loadedRepository.Version);
			int version = savedRepository.Version;
			
			// Compare Designs
			RepositoryComparer.Compare(((IStyleSetProvider)savedProject).StyleSet, ((IStyleSetProvider)loadedProject).StyleSet, version);
			CompareObjectCount(savedRepository.GetDesigns(), loadedRepository.GetDesigns());
			RepositoryComparer.Compare(savedProject.Design, loadedProject.Design, version);
			foreach (Design savedDesign in savedRepository.GetDesigns()) {
				if (savedDesign == savedProject.Design) continue;
				// ToDo: Clarify if the loaded projectDesign should be returned when calling GetDesigns()
				RepositoryComparer.Compare(savedDesign, loadedRepository.GetDesign(((IEntity)savedDesign).Id), version);
			}

			// Compare Templates including TerminalMappings and ModelMappings
			CompareObjectCount(savedRepository.GetTemplates(), loadedRepository.GetTemplates());
			foreach (Template savedTemplate in savedRepository.GetTemplates())
				RepositoryComparer.Compare(savedTemplate, loadedRepository.GetTemplate(savedTemplate.Id), version);

			// Compare ModelObjects
			CompareModelObjects(savedRepository, null, loadedRepository, null, version);

			// Compare diagrams including layers and shapes
			CompareObjectCount(savedRepository.GetDiagrams(), loadedRepository.GetDiagrams());
			foreach (Diagram savedDiagram in savedRepository.GetDiagrams())
				RepositoryComparer.Compare(savedDiagram, loadedRepository.GetDiagram(((IEntity)savedDiagram).Id), version);
		}


		#region Compare designs and styles

		public static void Compare(IEnumerable<Design> saved, IEnumerable<Design> loaded, int version) {
			Dictionary<object, Design> loadedDesigns = new Dictionary<object, Design>();
			foreach (Design d in loaded) loadedDesigns.Add(((IEntity)d).Id, d);

			foreach (Design savedDesign in saved) {
				object id = ((IEntity)savedDesign).Id;
				Design loadedDesign = null;
				Assert.IsTrue(loadedDesigns.TryGetValue(id, out loadedDesign));
				
				Assert.AreEqual<string>(savedDesign.Name, loadedDesign.Name);
				Compare(savedDesign, loadedDesign, version);
				Assert.AreEqual(savedDesign.CapStyles.Count, loadedDesign.CapStyles.Count);
				Assert.AreEqual(savedDesign.CharacterStyles.Count, loadedDesign.CharacterStyles.Count);
				Assert.AreEqual(savedDesign.ColorStyles.Count, loadedDesign.ColorStyles.Count);
				Assert.AreEqual(savedDesign.FillStyles.Count, loadedDesign.FillStyles.Count);
				Assert.AreEqual(savedDesign.LineStyles.Count, loadedDesign.LineStyles.Count);
				Assert.AreEqual(savedDesign.ParagraphStyles.Count, loadedDesign.ParagraphStyles.Count);
				CompareObjectCount(loadedDesign.Styles, savedDesign.Styles);
			}
		}


		public static void Compare(IStyleSet savedDesign, IStyleSet loadedDesign, int version) {
			foreach (ICapStyle savedStyle in savedDesign.CapStyles) {
				ICapStyle loadedStyle = loadedDesign.CapStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle, version);
			}
			foreach (ICharacterStyle savedStyle in savedDesign.CharacterStyles) {
				ICharacterStyle loadedStyle = loadedDesign.CharacterStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle, version);
			}
			foreach (IColorStyle savedStyle in savedDesign.ColorStyles) {
				IColorStyle loadedStyle = loadedDesign.ColorStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle, version);
			}
			foreach (IFillStyle savedStyle in savedDesign.FillStyles) {
				IFillStyle loadedStyle = loadedDesign.FillStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle, version);
			}
			foreach (ILineStyle savedStyle in savedDesign.LineStyles) {
				ILineStyle loadedStyle = loadedDesign.LineStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle, version);
			}
			foreach (IParagraphStyle savedStyle in savedDesign.ParagraphStyles) {
				IParagraphStyle loadedStyle = loadedDesign.ParagraphStyles[savedStyle.Name];
				Compare(savedStyle, loadedStyle, version);
			}
		}


		public static void Compare(IStyle savedStyle, IStyle loadedStyle, int version) {
			if (savedStyle is ICapStyle && loadedStyle is ICapStyle)
				Compare((ICapStyle)savedStyle, (ICapStyle)loadedStyle, version);
			else if (savedStyle is ICharacterStyle && loadedStyle is ICharacterStyle)
				Compare((ICharacterStyle)savedStyle, (ICharacterStyle)loadedStyle, version);
			else if (savedStyle is IColorStyle && loadedStyle is IColorStyle)
				Compare((IColorStyle)savedStyle, (IColorStyle)loadedStyle, version);
			else if (savedStyle is IFillStyle && loadedStyle is IFillStyle)
				Compare((IFillStyle)savedStyle, (IFillStyle)loadedStyle, version);
			else if (savedStyle is ILineStyle && loadedStyle is ILineStyle)
				Compare((ILineStyle)savedStyle, (ILineStyle)loadedStyle, version);
			else if (savedStyle is IParagraphStyle && loadedStyle is IParagraphStyle)
				Compare((IParagraphStyle)savedStyle, (IParagraphStyle)loadedStyle, version);
			else Assert.Fail("Different style types.");
		}


		public static void Compare(ICapStyle savedStyle, ICapStyle loadedStyle, int version) {
			CompareBaseStyle(savedStyle, loadedStyle, version);
			Compare(savedStyle.ColorStyle, loadedStyle.ColorStyle, version);
			Assert.AreEqual<CapShape>(savedStyle.CapShape, loadedStyle.CapShape);
			Assert.AreEqual<short>(savedStyle.CapSize, loadedStyle.CapSize);
		}


		public static void Compare(ICharacterStyle savedStyle, ICharacterStyle loadedStyle, int version) {
			CompareBaseStyle(savedStyle, loadedStyle, version);
			Compare(savedStyle.ColorStyle, loadedStyle.ColorStyle, version);
			Assert.AreEqual<FontFamily>(savedStyle.FontFamily, loadedStyle.FontFamily);
			Assert.AreEqual<string>(savedStyle.FontName, loadedStyle.FontName);
			Assert.AreEqual<int>(savedStyle.Size, loadedStyle.Size);
			Assert.AreEqual<float>(savedStyle.SizeInPoints, loadedStyle.SizeInPoints);
			Assert.AreEqual<FontStyle>(savedStyle.Style, loadedStyle.Style);
		}


		public static void Compare(IColorStyle savedStyle, IColorStyle loadedStyle, int version) {
			if (savedStyle == ColorStyle.Empty && loadedStyle == ColorStyle.Empty) return;
			CompareBaseStyle(savedStyle, loadedStyle, version);
			Assert.AreEqual<int>(savedStyle.Color.ToArgb(), loadedStyle.Color.ToArgb());
			if (version > 2) Assert.AreEqual<bool>(savedStyle.ConvertToGray, loadedStyle.ConvertToGray);
			Assert.AreEqual<byte>(savedStyle.Transparency, loadedStyle.Transparency);
		}


		public static void Compare(IFillStyle savedStyle, IFillStyle loadedStyle, int version) {
			CompareBaseStyle(savedStyle, loadedStyle, version);
			Compare(savedStyle.BaseColorStyle, loadedStyle.BaseColorStyle, version);
			Compare(savedStyle.AdditionalColorStyle, loadedStyle.AdditionalColorStyle, version);
			if (version > 2) Assert.AreEqual<bool>(savedStyle.ConvertToGrayScale, loadedStyle.ConvertToGrayScale);
			Assert.AreEqual<FillMode>(savedStyle.FillMode, loadedStyle.FillMode);
			Assert.AreEqual<System.Drawing.Drawing2D.HatchStyle>(savedStyle.FillPattern, loadedStyle.FillPattern);
			Assert.AreEqual<short>(savedStyle.GradientAngle, loadedStyle.GradientAngle);
			CompareNamedImage(savedStyle.Image, loadedStyle.Image, version);
			Assert.AreEqual<float>(savedStyle.ImageGammaCorrection, loadedStyle.ImageGammaCorrection);
			Assert.AreEqual<ImageLayoutMode>(savedStyle.ImageLayout, loadedStyle.ImageLayout);
			Assert.AreEqual<byte>(savedStyle.ImageTransparency, loadedStyle.ImageTransparency);
		}


		public static void Compare(ILineStyle savedStyle, ILineStyle loadedStyle, int version) {
			CompareBaseStyle(savedStyle, loadedStyle, version);
			Compare(savedStyle.ColorStyle, loadedStyle.ColorStyle, version);
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


		public static void Compare(IParagraphStyle savedStyle, IParagraphStyle loadedStyle, int version) {
			CompareBaseStyle(savedStyle, loadedStyle, version);
			Assert.AreEqual<ContentAlignment>(savedStyle.Alignment, loadedStyle.Alignment);
			Assert.AreEqual<TextPadding>(savedStyle.Padding, loadedStyle.Padding);
			Assert.AreEqual<StringTrimming>(savedStyle.Trimming, loadedStyle.Trimming);
			Assert.AreEqual<bool>(savedStyle.WordWrap, loadedStyle.WordWrap);
		}

		#endregion


		#region Compare templates

		public static void Compare(Template savedTemplate, Template loadedTemplate, int version) {
			CompareId(savedTemplate, loadedTemplate);
			CompareString(savedTemplate.Description, loadedTemplate.Description, false);
			CompareString(savedTemplate.Name, loadedTemplate.Name, false);
			CompareString(savedTemplate.Title, loadedTemplate.Title, false);
			CompareObjectCount(savedTemplate.GetPropertyMappings(), loadedTemplate.GetPropertyMappings());
			foreach (IModelMapping savedMapping in savedTemplate.GetPropertyMappings()) {
				IModelMapping loadedMapping = loadedTemplate.GetPropertyMapping(savedMapping.ModelPropertyId);
				Compare(savedMapping, loadedMapping, version);
			}
			Compare(savedTemplate.Shape, loadedTemplate.Shape, version);
			foreach (ControlPointId ptId in savedTemplate.Shape.GetControlPointIds(ControlPointCapabilities.All)) {
				Assert.AreEqual<TerminalId>(savedTemplate.GetMappedTerminalId(ptId), loadedTemplate.GetMappedTerminalId(ptId));
				Assert.AreEqual<string>(savedTemplate.GetMappedTerminalName(ptId), loadedTemplate.GetMappedTerminalName(ptId));
			}
		}


		public static void Compare(IModelMapping savedMapping, IModelMapping loadedMapping, int version) {
			CompareId(savedMapping, loadedMapping);
			Assert.AreEqual<int>(savedMapping.ModelPropertyId, loadedMapping.ModelPropertyId);
			Assert.AreEqual<int>(savedMapping.ShapePropertyId, loadedMapping.ShapePropertyId);
			if (savedMapping is NumericModelMapping && loadedMapping is NumericModelMapping)
				Compare((NumericModelMapping)savedMapping, (NumericModelMapping)loadedMapping, version);
			else if (savedMapping is FormatModelMapping && loadedMapping is FormatModelMapping)
				Compare((FormatModelMapping)savedMapping, (FormatModelMapping)loadedMapping, version);
			else if (savedMapping is StyleModelMapping && loadedMapping is StyleModelMapping)
				Compare((StyleModelMapping)savedMapping, (StyleModelMapping)loadedMapping, version);
			else Assert.Fail("saved and loaded model mapping are of different types");
		}


		public static void Compare(NumericModelMapping savedMapping, NumericModelMapping loadedMapping, int version) {
			Assert.AreEqual<NumericModelMapping.MappingType>(savedMapping.Type, loadedMapping.Type);
			Assert.IsTrue(Math.Abs(savedMapping.Intercept - loadedMapping.Intercept) < floatEqualityDelta);
			Assert.IsTrue(Math.Abs(savedMapping.Slope - loadedMapping.Slope) < floatEqualityDelta);
		}


		public static void Compare(FormatModelMapping savedMapping, FormatModelMapping loadedMapping, int version) {
			Assert.AreEqual<FormatModelMapping.MappingType>(savedMapping.Type, loadedMapping.Type);
			Assert.AreEqual<string>(savedMapping.Format, loadedMapping.Format);
		}


		public static void Compare(StyleModelMapping savedMapping, StyleModelMapping loadedMapping, int version) {
			Assert.AreEqual<StyleModelMapping.MappingType>(savedMapping.Type, loadedMapping.Type);
			Assert.AreEqual<int>(savedMapping.ValueRangeCount, loadedMapping.ValueRangeCount);
			switch (savedMapping.Type) {
				case StyleModelMapping.MappingType.IntegerStyle:
					List<int> savedIntRanges = new List<int>(savedMapping.ValueRangeCount);
					foreach (object obj in savedMapping.ValueRanges) savedIntRanges.Add((int)obj);
					List<int> loadedIntRanges = new List<int>(loadedMapping.ValueRangeCount);
					foreach (object obj in loadedMapping.ValueRanges) loadedIntRanges.Add((int)obj);
					for (int i = savedMapping.ValueRangeCount - 1; i >= 0; --i)
						Assert.AreEqual<int>(savedIntRanges[i], loadedIntRanges[i]);
					break;

				case StyleModelMapping.MappingType.FloatStyle:
					List<float> savedFloatRanges = new List<float>(savedMapping.ValueRangeCount);
					foreach (object obj in savedMapping.ValueRanges) savedFloatRanges.Add((float)obj);
					List<float> loadedFloatRanges = new List<float>(loadedMapping.ValueRangeCount);
					foreach (object obj in loadedMapping.ValueRanges) loadedFloatRanges.Add((float)obj);
					for (int i = savedMapping.ValueRangeCount - 1; i >= 0; --i)
						CompareFloat(savedFloatRanges[i], loadedFloatRanges[i]);
					break;

				default: Assert.Fail("Unsupported mapping type"); break;
			}
		}

		#endregion


		#region Compare diagrams

		public static void Compare(Diagram savedDiagram, Diagram loadedDiagram, int version) {
			Assert.AreEqual<bool>(savedDiagram != null, loadedDiagram != null);
			if (savedDiagram != null && loadedDiagram != null) {
				CompareId(savedDiagram, loadedDiagram);
				Assert.AreEqual<int>(savedDiagram.BackgroundColor.ToArgb(), loadedDiagram.BackgroundColor.ToArgb());
				Assert.AreEqual<int>(savedDiagram.BackgroundGradientColor.ToArgb(), loadedDiagram.BackgroundGradientColor.ToArgb());
				if (savedDiagram.BackgroundImage != null && loadedDiagram.BackgroundImage != null) {
					CompareString(savedDiagram.BackgroundImage.Name, loadedDiagram.BackgroundImage.Name, false);
					Assert.AreEqual<int>(savedDiagram.BackgroundImage.Width, loadedDiagram.BackgroundImage.Width);
					Assert.AreEqual<int>(savedDiagram.BackgroundImage.Height, loadedDiagram.BackgroundImage.Height);
				}
				Assert.AreEqual<float>(savedDiagram.BackgroundImageGamma, loadedDiagram.BackgroundImageGamma);
				Assert.AreEqual<bool>(savedDiagram.BackgroundImageGrayscale, loadedDiagram.BackgroundImageGrayscale);
				Assert.AreEqual<ImageLayoutMode>(savedDiagram.BackgroundImageLayout, loadedDiagram.BackgroundImageLayout);
				Assert.AreEqual<byte>(savedDiagram.BackgroundImageTransparency, loadedDiagram.BackgroundImageTransparency);
				Assert.AreEqual<int>(savedDiagram.BackgroundImageTransparentColor.ToArgb(), loadedDiagram.BackgroundImageTransparentColor.ToArgb());
				Assert.AreEqual<IDisplayService>(savedDiagram.DisplayService, loadedDiagram.DisplayService);
				Assert.AreEqual<int>(savedDiagram.Height, loadedDiagram.Height);
				Assert.AreEqual<int>(savedDiagram.Width, loadedDiagram.Width);
				Assert.AreEqual<bool>(savedDiagram.HighQualityRendering, loadedDiagram.HighQualityRendering);
				CompareString(savedDiagram.Name, loadedDiagram.Name, false);
				if (version > 2) CompareString(savedDiagram.Title, loadedDiagram.Title, false);
				//
				// Compare Layers
				Assert.AreEqual<int>(savedDiagram.Layers.Count, loadedDiagram.Layers.Count);
				SortedList<LayerIds, Layer> savedLayers = new SortedList<LayerIds, Layer>();
				foreach (Layer l in savedDiagram.Layers) savedLayers.Add(l.Id, l);
				SortedList<LayerIds, Layer> loadedLayers = new SortedList<LayerIds, Layer>();
				foreach (Layer l in loadedDiagram.Layers) loadedLayers.Add(l.Id, l);
				foreach (KeyValuePair<LayerIds, Layer> pair in savedLayers) {
					Layer loadedLayer = loadedLayers[pair.Key];
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
					Compare(savedShapes.Current, loadedShapes.Current, version);
					Assert.AreEqual(savedShapes.MoveNext(), loadedShapes.MoveNext());
				}
			}
		}

		#endregion


		#region Compare shapes

		public static void Compare(Shape savedShape, Shape loadedShape, int version) {
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
				Compare(savedShape.LineStyle, loadedShape.LineStyle, version);
				Compare(savedShape.ModelObject, loadedShape.ModelObject, version);
				Compare(savedShape.Parent, loadedShape.Parent, version);
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
					Compare(savedChildren.Current, loadedChildren.Current, version);
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
					Compare((ILinearShape)savedShape, (ILinearShape)loadedShape, version);
				Assert.AreEqual<bool>(savedShape is IPlanarShape, loadedShape is IPlanarShape);
				if (savedShape is IPlanarShape && loadedShape is IPlanarShape)
					Compare((IPlanarShape)savedShape, (IPlanarShape)loadedShape, version);
				Assert.AreEqual<bool>(savedShape is ICaptionedShape, loadedShape is ICaptionedShape);
				if (savedShape is ICaptionedShape && loadedShape is ICaptionedShape)
					Compare((ICaptionedShape)savedShape, (ICaptionedShape)loadedShape, version);
			}
		}


		private static void Compare(ILinearShape savedShape, ILinearShape loadedShape, int version) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				Assert.AreEqual<bool>(savedShape.IsDirected, loadedShape.IsDirected);
				Assert.AreEqual<int>(savedShape.MaxVertexCount, loadedShape.MaxVertexCount);
				Assert.AreEqual<int>(savedShape.MinVertexCount, loadedShape.MinVertexCount);
				Assert.AreEqual<int>(savedShape.VertexCount, loadedShape.VertexCount);
			}
		}


		private static void Compare(IPlanarShape savedShape, IPlanarShape loadedShape, int version) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				Assert.AreEqual<int>(savedShape.Angle, loadedShape.Angle);
				Compare(savedShape.FillStyle, loadedShape.FillStyle, version);
			}
		}


		private static void Compare(ICaptionedShape savedShape, ICaptionedShape loadedShape, int version) {
			Assert.AreEqual<bool>(savedShape != null, loadedShape != null);
			if (savedShape != null && loadedShape != null) {
				Assert.AreEqual<int>(savedShape.CaptionCount, loadedShape.CaptionCount);
				for (int i = savedShape.CaptionCount - 1; i >= 0; --i) {
					Compare(savedShape.GetCaptionCharacterStyle(i), loadedShape.GetCaptionCharacterStyle(i), version);
					Compare(savedShape.GetCaptionParagraphStyle(i), loadedShape.GetCaptionParagraphStyle(i), version);
					CompareString(savedShape.GetCaptionText(i), loadedShape.GetCaptionText(i), false);
				}
			}
		}

		#endregion


		#region Compare model objects

		public static void Compare(IModelObject savedModelObject, IModelObject loadedModelObject, int version) {
			Assert.AreEqual<bool>(savedModelObject != null, loadedModelObject != null);
			if (savedModelObject != null && loadedModelObject != null) {
				Assert.AreEqual<string>(savedModelObject.Type.FullName, loadedModelObject.Type.FullName);
				CompareId((IEntity)savedModelObject, (IEntity)loadedModelObject);
				Assert.AreEqual(savedModelObject.Name, loadedModelObject.Name);
				
				// Currently, there are orphaned template shapes that will be referenced by the savedModelObject, so this 
				// assertion will always fail. 
				// ToDo: Check how the template shapes became orphans and reactivate this check.
				//Assert.AreEqual(Count(savedModelObject.Shapes), Count(loadedModelObject.Shapes));
				// ToDo: Compare Id's of shapes

				Compare(savedModelObject.Parent, loadedModelObject.Parent, version);
				// Compare specific model object types
				if (savedModelObject is GenericModelObject) {
					Assert.AreEqual<bool>(savedModelObject is GenericModelObject, loadedModelObject is GenericModelObject);
					Compare((GenericModelObject)savedModelObject, (GenericModelObject)loadedModelObject, version);
				}
			}
		}


		public static void Compare(GenericModelObject savedModelObject, GenericModelObject loadedModelObject, int version) {
			Assert.AreEqual<int>(savedModelObject.IntegerValue, loadedModelObject.IntegerValue);
			CompareFloat(savedModelObject.FloatValue, loadedModelObject.FloatValue);
			CompareString(savedModelObject.StringValue, loadedModelObject.StringValue, false);
		}


		public static void CompareModelObjects(IRepository savedRepository, IModelObject savedParent, IRepository loadedRepository, IModelObject loadedParent, int version) {
			IEnumerable<IModelObject> savedModelObjs = savedRepository.GetModelObjects(savedParent);
			IEnumerable<IModelObject> loadedModelObjs = loadedRepository.GetModelObjects(loadedParent);
			CompareObjectCount(savedModelObjs, loadedModelObjs);
			foreach (IModelObject savedModelObj in savedModelObjs) {
				IModelObject loadedModelObj = loadedRepository.GetModelObject(savedModelObj.Id);
				Compare(savedModelObj, loadedModelObj, version);
				CompareModelObjects(savedRepository, savedModelObj, loadedRepository, loadedModelObj, version);
			}
		}


		#endregion


		private static void CompareBaseStyle(IStyle savedStyle, IStyle loadedStyle, int version) {
			CompareId(savedStyle, loadedStyle);
			Assert.AreEqual<string>(savedStyle.Name, loadedStyle.Name);
			Assert.AreEqual<string>(savedStyle.Title, loadedStyle.Title);
		}


		private static void CompareString(string savedString, string loadedString) {
			CompareString(savedString, loadedString, false);
		}


		private static void CompareString(string savedString, string loadedString, bool exact) {
			if (exact) Assert.AreEqual<string>(savedString, loadedString);
			else {
				if (!string.IsNullOrEmpty(savedString) && !string.IsNullOrEmpty(loadedString))
					Assert.IsTrue(savedString.Equals(loadedString, StringComparison.InvariantCultureIgnoreCase));
				else Assert.IsTrue(string.IsNullOrEmpty(savedString) == string.IsNullOrEmpty(loadedString));
			}
		}


		private static void CompareFloat(float savedValue, float loadedValue) {
			if (savedValue == 0 || loadedValue == 0)
				Assert.AreEqual(savedValue, loadedValue);
			else {
				// Calculate the number of significant digits
				float savedValueDelta = floatEqualityDelta * (savedValue / (savedValue / 10));
				float loadedValueDelta = floatEqualityDelta * (loadedValue / (loadedValue / 10));
				Assert.AreEqual(savedValueDelta, loadedValueDelta);
				// Compare the significant digits
				Assert.IsTrue(Math.Abs(savedValue - loadedValue) < savedValueDelta);
			}
		}


		private static void CompareNamedImage(NamedImage savedImage, NamedImage loadedImage, int version) {
			if (savedImage == null && loadedImage == null) return;
			CompareString(savedImage.Name, loadedImage.Name, true);
			if (savedImage.Image == null && loadedImage.Image == null) return;
			Assert.AreEqual<Size>(savedImage.Image.Size, loadedImage.Image.Size);
			CompareFloat(savedImage.Image.HorizontalResolution, loadedImage.Image.HorizontalResolution);
			CompareFloat(savedImage.Image.VerticalResolution, loadedImage.Image.VerticalResolution);
			Assert.AreEqual<System.Drawing.Imaging.PixelFormat>(savedImage.Image.PixelFormat, loadedImage.Image.PixelFormat);
			Assert.AreEqual<System.Drawing.Imaging.ImageFormat>(savedImage.Image.RawFormat, loadedImage.Image.RawFormat);
		}


		private static void CompareObjectCount<T>(IEnumerable<T> savedObjects, IEnumerable<T> loadedObjects) {
			if (Count(loadedObjects) != Count(savedObjects)) {
				int loaded = Count(loadedObjects);
				int saved = Count(savedObjects);
			}
			Assert.AreEqual<int>(Count(loadedObjects), Count(savedObjects));
		}


		private static int Count<T>(IEnumerable<T> items) {
			int result = 0;
			if (items is ICollection<T>) 
				result = ((ICollection<T>)items).Count;
			else {
				IEnumerator<T> enumerator = items.GetEnumerator();
				while (enumerator.MoveNext()) ++result;
			}
			return result;
		}


		private const float floatEqualityDelta = 0.000001f;
	}

}
