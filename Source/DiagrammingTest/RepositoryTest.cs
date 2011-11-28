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
using System.Diagnostics;
using System.Drawing;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;


namespace NShapeTest {

	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class RepositoryTest {

		public RepositoryTest() {
			// TODO: Add constructor logic here
		}


		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get { return testContextInstance; }
			set { testContextInstance = value; }
		}


		#region Additional test attributes

		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext) { }

		// Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup() { }

		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize() {
			TestContext.BeginTimer(TestContext.TestName + " Timer");
		}

		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup() {
			TestContext.EndTimer(TestContext.TestName + " Timer");
		}

		#endregion


		#region Test methods

		[TestMethod]
		public void XMLRepositoryTest() {
			string timerName;

			// Test inserting, modifying and deleting objects from repository
			timerName = "RepositoryTest XMLStore Timer";
			TestContext.BeginTimer(timerName);
			RepositoryTestCore(RepositoryHelper.CreateXmlStore(), RepositoryHelper.CreateXmlStore());
			TestContext.EndTimer(timerName);

			// Test inserting large diagrams
			timerName = "LargeDiagramTest XMLStore Timer";
			TestContext.BeginTimer(timerName);
			LargeDiagramCore(RepositoryHelper.CreateXmlStore());
			TestContext.EndTimer(timerName);
		}


		[TestMethod]
		public void SQLRepositoryTest() {
			string timerName;
			try {
				RepositoryHelper.SQLCreateDatabase();

				// Test inserting, modifying and deleting objects from repository
				timerName = "RepositoryTest SqlStore Timer";
				TestContext.BeginTimer(timerName);
				RepositoryTestCore(RepositoryHelper.CreateSqlStore(), RepositoryHelper.CreateSqlStore());
				TestContext.EndTimer(timerName);

				// Test inserting large diagrams
				timerName = "LargeDiagramTest SqlStore Timer";
				TestContext.BeginTimer(timerName);
				LargeDiagramCore(RepositoryHelper.CreateSqlStore());
				TestContext.EndTimer(timerName);

			} finally {
				RepositoryHelper.SQLDropDatabase();
			}
		}

		#endregion


		#region Repository test core methods

		private void RepositoryTestCore(Store store1, Store store2) {
			const string projectName = "Repository Test";
			const int shapesPerRow = 10;

			Project project1 = new Project();
			Project project2 = new Project();
			try {
				// Create two projects and repositories, one for modifying and 
				// saving, one for loading and comparing the result
				project1.Name =
				project2.Name = projectName;
				project1.Repository = new CachedRepository();
				project2.Repository = new CachedRepository();
				((CachedRepository)project1.Repository).Store = store1;
				((CachedRepository)project2.Repository).Store = store2;

				// Delete the current project (if it exists)
				project1.Repository.Erase();
				project1.Create();
				project1.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);

				// Create test data, populate repository and save repository
				string diagramName = "Diagram";
				DiagramHelper.CreateDiagram(project1, diagramName, shapesPerRow, shapesPerRow, true, true, true, true);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
				project2.Close();

				// Modify (and insert) content of the repository and save it
				ModifyContent(project1);
				InsertContent(project1);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
				project2.Close();

				// Delete various data from project
				DeleteContent(project1);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
				project2.Close();

				
				if (store1 is XmlStore) {
					// Save project again and check if backup files were created
					project1.Repository.SaveChanges();
					XmlStore xmlStore =(XmlStore)store1;
					Assert.IsTrue(File.Exists(Path.Combine(xmlStore.DirectoryName, project1.Name + ".bak")));
					Assert.IsTrue(File.Exists(Path.Combine(xmlStore.DirectoryName, project1.Name + xmlStore.FileExtension)));
					if (Directory.Exists(Path.Combine(xmlStore.DirectoryName, project1.Name + " Images")))
						Assert.IsTrue(Directory.Exists(Path.Combine(xmlStore.DirectoryName, project1.Name + " Images.bak")));
				}
			} finally {
				project1.Close();
				project2.Close();
			}
		}


		private void LargeDiagramCore(Store store) {
			string projectName = "Large Diagram Test";
			Project project = new Project();
			try {
				project.Name = projectName;
				project.Repository = new CachedRepository();
				((CachedRepository)project.Repository).Store = store;
				project.Repository.Erase();
				project.Create();
				project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);

				string diagramName = "Large Diagram";
				DiagramHelper.CreateLargeDiagram(project, diagramName);

				project.Repository.SaveChanges();
				Trace.WriteLine("Saved!");
			} finally {
				project.Close();
			}
		}

		#endregion


		#region Repository test helper methods

		#region Insert methods

		private void InsertContent(Project project) {
			IRepository repository = project.Repository;

			// Insert styles into the project's design
			// Insert new color styles at least in order to prevent other styles from using them (they will be deleted later)
			Design design = project.Design;
			List<ICapStyle> capStyles = new List<ICapStyle>(design.CapStyles);
			foreach (ICapStyle style in capStyles)
				InsertStyle(style, design, repository);
			List<ICharacterStyle> characterStyles = new List<ICharacterStyle>(design.CharacterStyles);
			foreach (ICharacterStyle style in characterStyles)
				InsertStyle(style, design, repository);
			List<IFillStyle> fillStyles = new List<IFillStyle>(design.FillStyles);
			foreach (IFillStyle style in fillStyles)
				InsertStyle(style, design, repository);
			List<ILineStyle> lineStyles = new List<ILineStyle>(design.LineStyles);
			foreach (ILineStyle style in lineStyles)
				InsertStyle(style, design, repository);
			List<IParagraphStyle> paragraphStyles = new List<IParagraphStyle>(design.ParagraphStyles);
			foreach (IParagraphStyle style in paragraphStyles)
				InsertStyle(style, design, repository);
			List<IColorStyle> colorStyles = new List<IColorStyle>(design.ColorStyles);
			foreach (IColorStyle style in colorStyles)
				InsertStyle(style, design, repository);

			// ToDo: Currently, the XML repository does not support more than one design. Insert additionsal designs later.
			//List<Design> designs = new List<Design>(repository.GetDesigns());
			//foreach (Design design in designs)
			//   InsertContent(design, repository);

			// Insert templates
			List<Template> templates = new List<Template>(repository.GetTemplates());
			foreach (Template template in templates)
				InsertTemplate(template, repository);

			// Insert model objects
			List<IModelObject> modelObjects = new List<IModelObject>(repository.GetModelObjects(null));
			foreach (IModelObject modelObject in modelObjects)
				InsertModelObject(modelObject, repository);

			// Modify diagrams and shapes
			List<Diagram> diagrams = new List<Diagram>(repository.GetDiagrams());
			foreach (Diagram diagram in diagrams)
				InsertDiagram(diagram, repository);
		}


		private void InsertDesign(Design sourceDesign, IRepository repository) {
			Design newDesign = new Design(GetName(sourceDesign.Name, EditContentMode.Insert));
			repository.InsertDesign(newDesign);
			foreach (IColorStyle style in sourceDesign.ColorStyles)
				InsertStyle(style, newDesign, repository);
			foreach (ICapStyle style in sourceDesign.CapStyles)
				InsertStyle(style, newDesign, repository);
			foreach (ICharacterStyle style in sourceDesign.CharacterStyles)
				InsertStyle(style, newDesign, repository);
			foreach (IFillStyle style in sourceDesign.FillStyles)
				InsertStyle(style, newDesign, repository);
			foreach (ILineStyle style in sourceDesign.LineStyles)
				InsertStyle(style, newDesign, repository);
			foreach (IParagraphStyle style in sourceDesign.ParagraphStyles)
				InsertStyle(style, newDesign, repository);
		}


		private void InsertStyle(ICapStyle sourceStyle, Design design, IRepository repository) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design, EditContentMode.Insert);
			CapStyle newStyle = new CapStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert);
			newStyle.CapShape = sourceStyle.CapShape;
			newStyle.CapSize = sourceStyle.CapSize;
			if (sourceStyle.ColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.ColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					InsertStyle(sourceStyle.ColorStyle, design, repository);
				newStyle.ColorStyle = design.ColorStyles[colorStyleName];
			}
			design.AddStyle(newStyle);
			repository.InsertStyle(design, newStyle);
		}


		private void InsertStyle(IColorStyle sourceStyle, Design design, IRepository repository) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design, EditContentMode.Insert);
			ColorStyle newStyle = new ColorStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert);
			newStyle.Color = sourceStyle.Color;
			newStyle.Transparency = sourceStyle.Transparency;
			newStyle.ConvertToGray = sourceStyle.ConvertToGray;
			design.AddStyle(newStyle);
			repository.InsertStyle(design, newStyle);
		}


		private void InsertStyle(IFillStyle sourceStyle, Design design, IRepository repository) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design, EditContentMode.Insert);
			FillStyle newStyle = new FillStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert);
			if (sourceStyle.AdditionalColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.AdditionalColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					InsertStyle(sourceStyle.AdditionalColorStyle, design, repository);
				newStyle.AdditionalColorStyle = design.ColorStyles[colorStyleName];
			}
			if (sourceStyle.BaseColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.BaseColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					InsertStyle(sourceStyle.BaseColorStyle, design, repository);
				newStyle.BaseColorStyle = design.ColorStyles[colorStyleName];
			}
			newStyle.ConvertToGrayScale = sourceStyle.ConvertToGrayScale;
			newStyle.FillMode = sourceStyle.FillMode;
			newStyle.FillPattern = sourceStyle.FillPattern;
			if (sourceStyle.Image != null) {
				NamedImage namedImg = new NamedImage((Image)sourceStyle.Image.Image.Clone(),
					GetName(sourceStyle.Image.Name, EditContentMode.Insert));
				newStyle.Image = namedImg;
			} else newStyle.Image = sourceStyle.Image;
			newStyle.ImageGammaCorrection = sourceStyle.ImageGammaCorrection;
			newStyle.ImageLayout = sourceStyle.ImageLayout;
			newStyle.ImageTransparency = sourceStyle.ImageTransparency;
			design.AddStyle(newStyle);
			repository.InsertStyle(design, newStyle);
		}


		private void InsertStyle(ICharacterStyle sourceStyle, Design design, IRepository repository) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design, EditContentMode.Insert);
			CharacterStyle newStyle = new CharacterStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert);
			if (sourceStyle.ColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.ColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					InsertStyle(sourceStyle.ColorStyle, design, repository);
				newStyle.ColorStyle = design.ColorStyles[colorStyleName];
			}
			newStyle.FontName = sourceStyle.FontName;
			newStyle.SizeInPoints = sourceStyle.SizeInPoints;
			newStyle.Style = sourceStyle.Style;
			design.AddStyle(newStyle);
			repository.InsertStyle(design, newStyle);
		}


		private void InsertStyle(ILineStyle sourceStyle, Design design, IRepository repository) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design, EditContentMode.Insert);
			LineStyle newStyle = new LineStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert);
			if (sourceStyle.ColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.ColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					InsertStyle(sourceStyle.ColorStyle, design, repository);
				newStyle.ColorStyle = design.ColorStyles[colorStyleName];
			}
			newStyle.DashCap = sourceStyle.DashCap;
			newStyle.DashType = sourceStyle.DashType;
			newStyle.LineJoin = sourceStyle.LineJoin;
			newStyle.LineWidth = sourceStyle.LineWidth;
			design.AddStyle(newStyle);
			repository.InsertStyle(design, newStyle);
		}


		private void InsertStyle(IParagraphStyle sourceStyle, Design design, IRepository repository) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design, EditContentMode.Insert);
			ParagraphStyle newStyle = new ParagraphStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert);
			newStyle.Alignment = sourceStyle.Alignment;
			newStyle.Padding = sourceStyle.Padding;
			newStyle.Trimming = sourceStyle.Trimming;
			design.AddStyle(newStyle);
			repository.InsertStyle(design, newStyle);
		}


		private void InsertTemplate(Template sourceTemplate, IRepository repository) {
			Template newTemplate = sourceTemplate.Clone();
			newTemplate.Description = GetName(sourceTemplate.Description, EditContentMode.Insert);
			newTemplate.Name = GetName(sourceTemplate.Name, EditContentMode.Insert);
			// Clone ModelObject and insert terminal mappings
			if (sourceTemplate.Shape.ModelObject != null) {
				foreach (ControlPointId pointId in sourceTemplate.Shape.GetControlPointIds(ControlPointCapabilities.All)) {
					if (newTemplate.Shape.HasControlPointCapability(pointId, ControlPointCapabilities.Connect)) {
						TerminalId terminalId = sourceTemplate.GetMappedTerminalId(pointId);
						if (terminalId != TerminalId.Invalid) newTemplate.MapTerminal(terminalId, pointId);
					}
				}
			}
			repository.InsertTemplate(newTemplate);

			// Insert property mappings
			foreach (IModelMapping sourceModelMapping in sourceTemplate.GetPropertyMappings())
				InsertModelMapping(sourceModelMapping, newTemplate, repository);
		}


		private void InsertModelMapping(IModelMapping sourceModelMapping, Template template, IRepository repository) {
			IModelMapping newModelMapping = null;
			if (sourceModelMapping is NumericModelMapping) {
				NumericModelMapping source = (NumericModelMapping)sourceModelMapping;
				NumericModelMapping numericMapping = new NumericModelMapping(
					source.ShapePropertyId,
					source.ModelPropertyId,
					source.Type);
				numericMapping.Intercept = source.Intercept;
				numericMapping.Slope = source.Slope;
				newModelMapping = numericMapping;
			} else if (sourceModelMapping is FormatModelMapping) {
				FormatModelMapping source = (FormatModelMapping)sourceModelMapping;
				FormatModelMapping formatMapping = new FormatModelMapping(
					source.ShapePropertyId,
					source.ModelPropertyId,
					source.Type);
				formatMapping.Format = source.Format;
				newModelMapping = formatMapping;
			} else if (sourceModelMapping is StyleModelMapping) {
				StyleModelMapping source = (StyleModelMapping)sourceModelMapping;
				StyleModelMapping styleMapping = new StyleModelMapping(
					source.ShapePropertyId,
					source.ModelPropertyId,
					source.Type);
				foreach (object range in styleMapping.ValueRanges) {
					if (range is float)
						styleMapping.AddValueRange((float)range, styleMapping[(float)range]);
					else if (range is int)
						styleMapping.AddValueRange((int)range, styleMapping[(int)range]);
				}
				newModelMapping = styleMapping;
			}
			if (newModelMapping != null) {
				template.MapProperties(newModelMapping);
				repository.InsertModelMapping(newModelMapping, template);
			}
		}


		private void InsertDiagram(Diagram sourceDiagram, IRepository repository) {
			Diagram newDiagram = new Diagram(GetName(sourceDiagram.Name, EditContentMode.Insert));
			// Insert properties
			newDiagram.BackgroundColor = sourceDiagram.BackgroundColor;
			newDiagram.BackgroundGradientColor = sourceDiagram.BackgroundGradientColor;
			newDiagram.BackgroundImageGamma = sourceDiagram.BackgroundImageGamma;
			newDiagram.BackgroundImageGrayscale = sourceDiagram.BackgroundImageGrayscale;
			newDiagram.BackgroundImageLayout = sourceDiagram.BackgroundImageLayout;
			newDiagram.BackgroundImageTransparency = sourceDiagram.BackgroundImageTransparency;
			newDiagram.BackgroundImageTransparentColor = sourceDiagram.BackgroundImageTransparentColor;
			newDiagram.Height = sourceDiagram.Height;
			newDiagram.Width = sourceDiagram.Width;
			newDiagram.Title = GetName(sourceDiagram.Title, EditContentMode.Insert);

			// Insert layers
			foreach (Layer sourceLayer in sourceDiagram.Layers) {
				Layer newLayer = new Layer(GetName(sourceLayer.Name, EditContentMode.Insert));
				newLayer.Title = GetName(sourceLayer.Title, EditContentMode.Insert);
				newLayer.UpperZoomThreshold = sourceLayer.UpperZoomThreshold;
				newLayer.LowerZoomThreshold = sourceLayer.LowerZoomThreshold;
				newDiagram.Layers.Add(newLayer);
			}
			repository.InsertDiagram(newDiagram);

			// Insert shapes
			foreach (Shape sourceShape in sourceDiagram.Shapes.BottomUp)
				InsertShape(sourceShape, newDiagram, repository);
		}


		private void InsertShape(Shape sourceShape, Diagram diagram, IRepository repository) {
			Shape newShape = sourceShape.Clone();
			diagram.Shapes.Add(newShape);
			diagram.AddShapeToLayers(newShape, sourceShape.Layers);
			repository.InsertShape(newShape, diagram);
		}


		private void InsertModelObject(IModelObject sourceModelObject, IRepository repository) {
			IModelObject newModelObject = sourceModelObject.Clone();
			newModelObject.Name = GetName(sourceModelObject.Name, EditContentMode.Insert);
			repository.InsertModelObject(newModelObject);
		}

		#endregion


		#region Modify methods

		private void ModifyContent(Project project) {
			IRepository repository = project.Repository;

			// Modify designs and styles
			foreach (Design design in repository.GetDesigns())
				ModifyDesign(design, repository);

			// Modify templates
			foreach (Template template in repository.GetTemplates())
				ModifyTemplate(template, repository);

			// Modify model objects
			foreach (IModelObject modelObject in repository.GetModelObjects(null))
				ModifyModelObject(modelObject, repository);

			// Modify diagrams and shapes
			foreach (Diagram diagram in repository.GetDiagrams())
				ModifyDiagram(diagram, repository);
		}


		private void ModifyDesign(Design design, IRepository repository) {
			foreach (CapStyle style in design.CapStyles) ModifyStyle(style, design, repository);
			foreach (CharacterStyle style in design.CharacterStyles) ModifyStyle(style, design, repository);
			foreach (FillStyle style in design.FillStyles) ModifyStyle(style, design, repository);
			foreach (LineStyle style in design.LineStyles) ModifyStyle(style, design, repository);
			foreach (ParagraphStyle style in design.ParagraphStyles) ModifyStyle(style, design, repository);
			foreach (ColorStyle style in design.ColorStyles) ModifyStyle(style, design, repository);
		}


		private void ModifyStyle(Style style, IRepository repository) {
			if (!repository.GetDesign(null).IsStandardStyle(style))
				style.Name = GetName(style.Name, EditContentMode.Modify);
			style.Title = GetName(style.Title, EditContentMode.Modify);
			repository.UpdateStyle(style);
		}


		private void ModifyStyle(CapStyle style, Design design, IRepository repository) {
			style.CapShape = Enum<CapShape>.GetNextValue(style.CapShape);
			style.CapSize += 1;
			style.ColorStyle = GetNextValue(design.ColorStyles, style.ColorStyle);
			ModifyStyle(style, repository);
		}


		private void ModifyStyle(CharacterStyle style, Design design, IRepository repository) {
			style.ColorStyle = GetNextValue(design.ColorStyles, style.ColorStyle);
			style.FontFamily = GetNextValue(FontFamily.Families, style.FontFamily);
			style.Size += 1;
			style.Style = Enum<FontStyle>.GetNextValue(style.Style);
			ModifyStyle(style, repository);
		}


		private void ModifyStyle(ColorStyle style, Design design, IRepository repository) {
			int r = style.Color.R;
			int g = style.Color.G;
			int b = style.Color.B;
			style.Color = Color.FromArgb(g, b, r);
			style.ConvertToGray = !style.ConvertToGray;
			style.Transparency = (style.Transparency <= 50) ? (byte)75 : (byte)25;
			ModifyStyle(style, repository);
		}


		private void ModifyStyle(FillStyle style, Design design, IRepository repository) {
			style.AdditionalColorStyle = GetNextValue(design.ColorStyles, style.AdditionalColorStyle);
			style.BaseColorStyle = GetNextValue(design.ColorStyles, style.BaseColorStyle);
			style.ConvertToGrayScale = !style.ConvertToGrayScale;
			style.FillMode = Enum<FillMode>.GetNextValue(style.FillMode);
			style.FillPattern = Enum<System.Drawing.Drawing2D.HatchStyle>.GetNextValue(style.FillPattern);
			style.ImageGammaCorrection += 0.1f;
			style.ImageLayout = Enum<ImageLayoutMode>.GetNextValue(style.ImageLayout);
			style.ImageTransparency = (style.ImageTransparency < 100) ?
				(byte)(style.ImageTransparency + 1) : (byte)(style.ImageTransparency - 1);
			ModifyStyle(style, repository);
		}


		private void ModifyStyle(LineStyle style, Design design, IRepository repository) {
			style.ColorStyle = GetNextValue(design.ColorStyles, style.ColorStyle);
			style.DashCap = Enum<System.Drawing.Drawing2D.DashCap>.GetNextValue(style.DashCap);
			style.DashType = Enum<DashType>.GetNextValue(style.DashType);
			style.LineJoin = Enum<System.Drawing.Drawing2D.LineJoin>.GetNextValue(style.LineJoin);
			style.LineWidth += 1;
			ModifyStyle(style, repository);
		}


		private void ModifyStyle(ParagraphStyle style, Design design, IRepository repository) {
			style.Alignment = Enum<ContentAlignment>.GetNextValue(style.Alignment);
			style.Padding = new TextPadding(style.Padding.Left + 1, style.Padding.Top + 1, style.Padding.Right + 1, style.Padding.Bottom + 1);
			style.Trimming = Enum<StringTrimming>.GetNextValue(style.Trimming);
			style.WordWrap = !style.WordWrap;
			ModifyStyle(style, repository);
		}


		private void ModifyTemplate(Template template, IRepository repository) {
			template.Description = GetName(template.Description, EditContentMode.Modify);
			template.Name = GetName(template.Name, EditContentMode.Modify);

			// Assign a new child shape with a new child modelObject
			Shape newShape = template.Shape.Clone();
			if (newShape.ModelObject != null) newShape.ModelObject = null;

			template.Shape.Children.Add(newShape);
			repository.InsertShape(newShape, template.Shape);
			if (template.Shape.ModelObject != null) {
				// ToDo: ModelObjects of child shapes and child model objects are not supported in the current version
				//newShape.ModelObject = template.Shape.ModelObject.Clone();
				//newShape.ModelObject.Parent = template.Shape.ModelObject;
				//repository.InsertModelObject(newShape.ModelObject);
			}

			// Modify property mappings
			foreach (IModelMapping modelMapping in template.GetPropertyMappings())
				ModifyModelMapping(modelMapping, repository);
			repository.UpdateModelMappings(template.GetPropertyMappings());

			// Modify terminal mappings
			if (template.Shape.ModelObject != null) {
				// Get all mapped point- and terminal ids
				List<ControlPointId> pointIds = new List<ControlPointId>();
				List<TerminalId> terminalIds = new List<TerminalId>();
				foreach (ControlPointId pointId in template.Shape.GetControlPointIds(ControlPointCapabilities.All)) {
					TerminalId terminalId = template.GetMappedTerminalId(pointId);
					if (terminalId != TerminalId.Invalid) {
						pointIds.Add(pointId);
						terminalIds.Add(terminalId);
					}
				}
				// Now reverse the mappings
				Assert.AreEqual(pointIds.Count, terminalIds.Count);
				for (int i = 0; i < terminalIds.Count; ++i)
					template.MapTerminal(terminalIds[i], pointIds[pointIds.Count - i]);
			}
			repository.UpdateTemplate(template);
		}


		private void ModifyModelMapping(IModelMapping modelMapping, IRepository repository) {
			if (modelMapping is NumericModelMapping) {
				NumericModelMapping numericMapping = (NumericModelMapping)modelMapping;
				numericMapping.Intercept += 10;
				numericMapping.Slope += 1;
			} else if (modelMapping is FormatModelMapping) {
				FormatModelMapping formatMapping = (FormatModelMapping)modelMapping;
				formatMapping.Format = GetName(formatMapping.Format, EditContentMode.Modify);
			} else if (modelMapping is StyleModelMapping) {
				StyleModelMapping styleMapping = (StyleModelMapping)modelMapping;
				List<object> ranges = new List<object>(styleMapping.ValueRanges);
				Design design = repository.GetDesign(null);
				foreach (object range in ranges) {
					IStyle currentStyle = null;
					if (range is float)
						currentStyle = styleMapping[(float)range];
					else if (range is int)
						currentStyle = styleMapping[(int)range];

					IStyle newStyle = null;
					if (currentStyle is CapStyle)
						newStyle = GetNextValue(design.CapStyles, (CapStyle)currentStyle);
					else if (currentStyle is CharacterStyle)
						newStyle = GetNextValue(design.CharacterStyles, (CharacterStyle)currentStyle);
					else if (currentStyle is ColorStyle)
						newStyle = GetNextValue(design.ColorStyles, (ColorStyle)currentStyle);
					else if (currentStyle is FillStyle)
						newStyle = GetNextValue(design.FillStyles, (FillStyle)currentStyle);
					else if (currentStyle is LineStyle)
						newStyle = GetNextValue(design.LineStyles, (LineStyle)currentStyle);
					else if (currentStyle is ParagraphStyle)
						newStyle = GetNextValue(design.ParagraphStyles, (ParagraphStyle)currentStyle);

					if (range is float) {
						styleMapping.RemoveValueRange((float)range);
						styleMapping.AddValueRange((float)range, newStyle);
					} else if (range is int) {
						styleMapping.RemoveValueRange((int)range);
						styleMapping.AddValueRange((int)range, newStyle);
					}
				}
			}
		}


		private void ModifyDiagram(Diagram diagram, IRepository repository) {
			// Modify "base" properties
			Color backColor = diagram.BackgroundColor;
			Color gradientColor = diagram.BackgroundGradientColor;
			diagram.BackgroundGradientColor = backColor;
			diagram.BackgroundColor = gradientColor;
			diagram.BackgroundImageGamma += 0.1f;
			diagram.BackgroundImageGrayscale = !diagram.BackgroundImageGrayscale;
			diagram.BackgroundImageLayout = Enum<ImageLayoutMode>.GetNextValue(diagram.BackgroundImageLayout);
			diagram.BackgroundImageTransparency = (diagram.BackgroundImageTransparency <= 50) ? (byte)75 : (byte)25;
			diagram.BackgroundImageTransparentColor = Color.FromArgb(
				diagram.BackgroundImageTransparentColor.G,
				diagram.BackgroundImageTransparentColor.B,
				diagram.BackgroundImageTransparentColor.R);
			diagram.Height += 200;
			diagram.Width += 200;
			diagram.Title = GetName(diagram.Title, EditContentMode.Modify);
			diagram.Name = GetName(diagram.Name, EditContentMode.Modify);

			// Modify layers
			foreach (Layer layer in diagram.Layers) {
				layer.LowerZoomThreshold += 1;
				layer.Title = GetName(layer.Title, EditContentMode.Modify);
				layer.UpperZoomThreshold += 1;
			}
			repository.UpdateDiagram(diagram);

			// Modify shapes
			foreach (Shape shape in diagram.Shapes)
				ModifyShape(shape, repository);
		}


		private void ModifyModelObject(IModelObject modelObject, IRepository repository) {
			IModelObject child = modelObject.Clone();
			child.Parent = modelObject;
			if (modelObject is GenericModelObject)
				ModifyModelObject((GenericModelObject)modelObject, repository);
			repository.UpdateModelObject(modelObject);
		}


		private void ModifyModelObject(GenericModelObject modelObject, IRepository repository) {
			modelObject.FloatValue += 1.1f;
			modelObject.IntegerValue += 1;
			modelObject.StringValue += " Modified";
		}


		private void ModifyShape(Shape shape, IRepository repository) {
			shape.Children.Add(shape.Clone());
			shape.LineStyle = GetNextValue(repository.GetDesign(null).LineStyles, shape.LineStyle);
			shape.MoveBy(100, 100);
			shape.SecurityDomainName = (char)(((int)shape.SecurityDomainName) + 1);
			if (shape is ILinearShape) ModifyShape((ILinearShape)shape, repository);
			else if (shape is IPlanarShape) ModifyShape((IPlanarShape)shape, repository);
			else if (shape is ICaptionedShape) ModifyShape((ICaptionedShape)shape, repository);
			repository.UpdateShape(shape);
		}


		private void ModifyShape(ILinearShape shape, IRepository repository) {
			// ToDo: Modify lines
		}


		private void ModifyShape(IPlanarShape shape, IRepository repository) {
			shape.Angle += 10;
			shape.FillStyle = GetNextValue(repository.GetDesign(null).FillStyles, shape.FillStyle);
		}


		private void ModifyShape(ICaptionedShape shape, IRepository repository) {
			Design design = repository.GetDesign(null);
			int cnt = shape.CaptionCount;
			for (int i = 0; i < cnt; ++i) {
				string txt = shape.GetCaptionText(i);
				shape.SetCaptionText(i, txt + "Modified");
				ICharacterStyle characterStyle = shape.GetCaptionCharacterStyle(i);
				shape.SetCaptionCharacterStyle(i, GetNextValue(design.CharacterStyles, characterStyle));
				IParagraphStyle paragraphStyle = shape.GetCaptionParagraphStyle(i);
				shape.SetCaptionParagraphStyle(i, GetNextValue(design.ParagraphStyles, paragraphStyle));
			}
		}

		#endregion


		#region Delete methods

		private void DeleteContent(Project project) {
			IRepository repository = project.Repository;

			// Delete all additional designs
			DeleteDesigns(repository);

			// Delete all non-standard styles
			DeleteStyles(project.Design, repository);

			// Delete templates
			DeleteTemplates(repository);

			// Modify diagrams and shapes
			DeleteDiagrams(repository);

			// ToDo: Delete the whole model
			//DeleteModel(repository);
		}


		private void DeleteDesigns(IRepository repository) {
			Design projectDesign = repository.GetDesign(null);
			List<Design> designs = new List<Design>(repository.GetDesigns());
			foreach (Design d in designs) {
				if (d == projectDesign) continue;
				repository.DeleteDesign(d);
			}
		}


		private void DeleteStyles(Design design, IRepository repository) {
			DeleteStyles(design, design.CapStyles, repository);
			DeleteStyles(design, design.CharacterStyles, repository);
			DeleteStyles(design, design.FillStyles, repository);
			DeleteStyles(design, design.LineStyles, repository);
			DeleteStyles(design, design.ParagraphStyles, repository);
			DeleteStyles(design, design.ColorStyles, repository);
		}


		private void DeleteStyles<TStyle>(Design design, IEnumerable<TStyle> styles, IRepository repository) where TStyle : IStyle {
			List<TStyle> styleList = new List<TStyle>(styles);
			foreach (TStyle style in styleList) {
				if (!design.IsStandardStyle(style)) {
					design.RemoveStyle(style);
					repository.DeleteStyle(style);
				}
			}
		}


		private void DeleteTemplates(IRepository repository) {
			List<Template> templates = new List<Template>(repository.GetTemplates());
			foreach (Template t in templates) {
				if (IsNameOf(EditContentMode.Insert, t.Name)) {
					// If the template was inserted, delete it
					repository.DeleteTemplate(t);
				} else if (IsNameOf(EditContentMode.Modify, t.Name)) {
					// If the template was modified, delete some of it's content 
					//
					// Delete child model objects
					if (t.Shape.ModelObject != null) {
						DeleteChildModelObjects(t.Shape.ModelObject, repository);
						t.UnmapAllTerminals();

						// Delete ModelMappings
						List<IModelMapping> modelMappings = new List<IModelMapping>(t.GetPropertyMappings());
						t.UnmapAllProperties();
						repository.DeleteModelMappings(modelMappings);
					}

					// Delete child shapes
					if (t.Shape.Children.Count > 0) {
						List<Shape> children = new List<Shape>(t.Shape.Children);
						t.Shape.Children.Clear();
						repository.DeleteShapes(children);
					}
					repository.UpdateTemplate(t);
				}
			}
		}


		private void DeleteChildModelObjects(IModelObject parent, IRepository repository) {
			List<IModelObject> children = new List<IModelObject>(repository.GetModelObjects(parent));
			for (int i = children.Count - 1; i >= 0; --i)
				DeleteChildModelObjects(children[i], repository);
			repository.DeleteModelObjects(children);
		}


		private void DeleteDiagrams(IRepository repository) {
			List<Diagram> diagrams = new List<Diagram>(repository.GetDiagrams());
			for (int i = diagrams.Count - 1; i >= 0; --i) {
				if (IsNameOf(EditContentMode.Insert, diagrams[i].Name)) {
					repository.DeleteDiagram(diagrams[i]);
				} else if (IsNameOf(EditContentMode.Modify, diagrams[i].Name)) {
					// Delete every second shapes
					DeleteShapes(diagrams[i].Shapes.TopDown, repository);

					// Delete layers
					List<Layer> layers = new List<Layer>();
					for (int j = layers.Count - 1; j >= 0; --j) {
						if (j % 2 == 0) diagrams[j].Layers.Remove(layers[j]);
					}
				}
			}
		}


		private void DeleteShapes(IEnumerable<Shape> shapes, IRepository repository) {
			List<Shape> shapeList = new List<Shape>(shapes);
			for (int i = shapeList.Count - 1; i >= 0; --i) {
				if (i % 2 == 0) {
					DoDeleteShape(shapeList[i], repository);
					if (shapeList[i].Diagram != null)
						shapeList[i].Diagram.Shapes.Remove(shapeList[i]);
					else if (shapeList[i].Parent != null)
						shapeList[i].Parent.Children.Remove(shapeList[i]);
				}
			}
		}


		private void DoDeleteShape(Shape shape, IRepository repository) {
			// Delete shape from repository (connections will be deleted automatically)
			repository.DeleteShape(shape);
			// Disconnect shape
			if (shape.IsConnected(ControlPointId.Any, null) != ControlPointId.None) {
				List<ShapeConnectionInfo> connections = new List<ShapeConnectionInfo>(shape.GetConnectionInfos(ControlPointId.Any, null));
				foreach (ShapeConnectionInfo ci in connections) {
					Assert.IsFalse(ci == ShapeConnectionInfo.Empty);
					shape.Disconnect(ci.OwnPointId);
				}
			}
		}

		#endregion


		private T GetNextValue<T>(IEnumerable<T> collection, T currentValue)
			where T : class {
			T result = null;
			IEnumerator<T> enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext()) {
				if (result == null) result = enumerator.Current;
				if (enumerator.Current == currentValue) {
					if (enumerator.MoveNext()) {
						result = enumerator.Current;
						break;
					}
				}
			}
			return result;
		}


		private string GetNewStyleName<TStyle>(TStyle style, Design design, EditContentMode mode)
			where TStyle : IStyle {
			string result = GetName(style.Name, mode);
			if (design.ColorStyles.Contains(result)) {
				result = result + " ({0})";
				int i = 1;
				while (design.ColorStyles.Contains(string.Format(result, i))) ++i;
				result = string.Format(result, i);
			}
			return result;
		}


		private string GetName(string name, EditContentMode mode) {
			string result;
			switch (mode) {
				case EditContentMode.Insert:
					result = NewNamePrefix + " " + name; break;
				case EditContentMode.Modify:
					result = ModifiedNamePrefix + " " + name; break;
				default:
					Debug.Fail(string.Format("Unexpected {0} value '{1}'", typeof(EditContentMode).Name, mode));
					result = name; break;
			}
			return result;
		}


		private bool IsNameOf(EditContentMode mode, string name) {
			bool result;
			switch (mode) {
				case EditContentMode.Insert:
					result = name.StartsWith(NewNamePrefix); break;
				case EditContentMode.Modify:
					result = name.StartsWith(ModifiedNamePrefix);
					break;
				default:
					Debug.Fail(string.Format("Unexpected {0} value '{1}'", typeof(EditContentMode).Name, mode));
					result = false; break;
			}
			return result;
		}

		#endregion


		private TestContext testContextInstance;

		private enum EditContentMode { Insert, Modify };

		private const string NewNamePrefix = "Copy of";
		private const string ModifiedNamePrefix = "Modified";
	}

}
