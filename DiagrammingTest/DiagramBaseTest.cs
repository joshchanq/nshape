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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NShapeTest {

	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class DiagramBaseTest {
		
		public DiagramBaseTest() {
			//
			// TODO: Add constructor logic here
			//
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
		public void BaseTest() {
			// -- Create a project --
			Project project = new Project();
			project.Name = "Test";
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore("C:\\Temp", ".xml");
			project.Repository.Erase();
			project.Create();
		
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			//
			Diagram diagram = new Diagram("All Shapes");
			diagram.Width = 800;
			diagram.Height = 600;
			project.Repository.InsertDiagram(diagram);
			int x = 0;
			int shapeCount = 0;
			foreach (ShapeType st in project.ShapeTypes) {
				x += 50;
				Shape s = st.CreateInstance();
				s.X = x;
				s.Y = 50;
				if (s is IPlanarShape) {
					((IPlanarShape)s).Angle = -300;
					//((IPlanarShape)s).FillStyle
					// ((ILinearShape)s).LineStyle
				}
				diagram.Shapes.Add(s, shapeCount + 1);
				project.Repository.InsertShape(s, diagram);
				++shapeCount;
			}
			//
			Diagram diagram2 = new Diagram("Connections");
			diagram2.Width = 800;
			diagram2.Height = 600;
			Circle circle = (Circle)project.ShapeTypes["Circle"].CreateInstance();
			circle.X = 50;
			circle.Y = 50;
			circle.Diameter = 10;
			diagram2.Shapes.Add(circle, diagram.Shapes.MaxZOrder + 10);
			Box box = (Box)project.ShapeTypes["Box"].CreateInstance();
			box.X = 100;
			box.Y = 50;
			box.Width = 20;
			box.Height = 10;
			box.Angle = 450;
			diagram2.Shapes.Add(box, diagram2.Shapes.MaxZOrder + 10);
			Polyline polyLine = (Polyline)project.ShapeTypes["Polyline"].CreateInstance();
			polyLine.Connect(1, box, 3);
			polyLine.Connect(2, circle, 7);
			diagram2.Shapes.Add(polyLine, diagram2.Shapes.MaxZOrder + 10);
			project.Repository.InsertDiagram(diagram2);
			//
			project.Repository.SaveChanges();
			project.Close();
			//
			// -- Reload project and modify --
			project.Open();
			diagram = project.Repository.GetDiagram("All Shapes");
			project.Repository.GetDiagramShapes(diagram);
			foreach (Shape s in diagram.Shapes) {
				s.MoveBy(300, 300);
				if (s is ICaptionedShape)
					((ICaptionedShape)s).SetCaptionText(0, s.Type.Name);
				project.Repository.UpdateShape(s);
			}
			project.Repository.SaveChanges();
			project.Close();
			//
			// -- Reload and check --
			project.Open();
			diagram = project.Repository.GetDiagram("All Shapes");
			int c = 0;
			foreach (Shape s in diagram.Shapes) {
				Assert.AreEqual(350, s.Y);
				if (s is IPlanarShape) Assert.AreEqual(3300, ((IPlanarShape)s).Angle, s.Type.Name);
				if (s is ICaptionedShape)
					Assert.AreEqual(((ICaptionedShape)s).GetCaptionText(0), s.Type.Name);
				++c;
			}
			Assert.AreEqual(shapeCount, diagram.Shapes.Count);
			Assert.AreEqual(shapeCount, c);
			project.Close();
		}


		/// <summary>
		/// Calls all Shape methods and properties for each available shape type and does 
		/// some plausibility checks.
		/// </summary>
		[TestMethod]
		public void ShapeTest() {
			Project project = new Project();
			project.Name = "Test";
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore("C:\\Temp", ".xml");
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			//
			foreach (ShapeType st in project.ShapeTypes) {
				Shape s = st.CreateInstance();
				if (s is TextBase) ((TextBase)s).AutoSize = false;
				//
				// -- Test Properties --
				s.X = 100;
				Assert.AreEqual(100, s.X);
				s.Y = -100;
				Assert.AreEqual(-100, s.Y);
				Assert.ReferenceEquals(st, s.Type);
				Assert.ReferenceEquals(null, s.Template);
				Assert.ReferenceEquals(null, s.Tag);
				s.Tag = "Hello";
				Assert.AreEqual("Hello", (string)s.Tag);
				Assert.AreEqual('A', s.SecurityDomainName);
				s.SecurityDomainName = 'K';
				Assert.AreEqual('K', s.SecurityDomainName);
				Assert.ReferenceEquals(null, s.Parent);
				s.Parent = null;
				Assert.ReferenceEquals(null, s.ModelObject);
				// s.ModelObject = project.ModelObjectTypes["Generic ModelObjectTypes Object"].CreateInstance();
				// Assert
				Assert.ReferenceEquals(project.Design.LineStyles.Normal, s.LineStyle);
				s.LineStyle = project.Design.LineStyles.HighlightDashed;
				Assert.ReferenceEquals(project.Design.LineStyles.HighlightDashed, s.LineStyle);
				Assert.ReferenceEquals(null, ((IEntity)s).Id);
				Assert.ReferenceEquals(null, s.DisplayService);
				Assert.AreEqual(0, s.Children.Count);
				Rectangle bounds1 = s.GetBoundingRectangle(true);
				//
				// -- Test Methods --
				RelativePosition relPos = s.CalculateRelativePosition(100, -100);
				Point absPos = s.CalculateAbsolutePosition(relPos);
				Assert.AreEqual(absPos.X, 100);
				Assert.AreEqual(absPos.Y, -100);
				//
				Shape clone = s.Clone();
				Assert.AreEqual(clone.Y, s.Y);
				Assert.ReferenceEquals(clone.LineStyle, s.LineStyle);
				//
				// TODO 2: Do not know, how to test the result.
				s.ContainsPoint(0, 0);
				s.ContainsPoint(105, -95);
				s.ContainsPoint(-10000, +10000);
				//
				int cpc = 0;
				foreach (ControlPointId id in s.GetControlPointIds(ControlPointCapabilities.Resize)) {
					++cpc;
					s.GetControlPointPosition(id);
					Assert.IsTrue(s.HasControlPointCapability(id, ControlPointCapabilities.Resize));
				}
				//
				s.MoveBy(-320, 1000);
				Rectangle bounds2 = s.GetBoundingRectangle(true);
				bounds2.Offset(320, -1000);
				Assert.AreEqual(bounds1, bounds2);
				s.MoveTo(100, -100);
				bounds2 = s.GetBoundingRectangle(true);
				Assert.AreEqual(bounds2, bounds1);
				//
				Point cPos = s.GetControlPointPosition(ControlPointId.Reference);
				ControlPointId cpId = s.FindNearestControlPoint(cPos.X, cPos.Y, 1, ControlPointCapabilities.All);
				Assert.IsTrue(s.HasControlPointCapability(cpId, ControlPointCapabilities.Reference));
				//
				Point oiPoint = s.CalculateConnectionFoot(1000, 1000);
				bounds1 = s.GetBoundingRectangle(true);
				bounds1.Inflate(2, 2);
				Assert.IsTrue(bounds1.Contains(oiPoint));
				//
				Assert.IsTrue(s.IntersectsWith(bounds1.X, bounds1.Y, bounds1.Width, bounds1.Height));
				//
				bounds1 = new Rectangle(500, 500, 100, 100);
				s.Fit(bounds1.X, bounds1.Y, bounds1.Width, bounds1.Height);
				bounds1.Inflate(10, 10);
				Assert.IsTrue(bounds1.Contains(s.GetBoundingRectangle(true)));
				//
				foreach (MenuItemDef a in s.GetMenuItemDefs(0, 0, 0))
					Assert.IsNotNull(a.Name);
				//
				// Connections
				// One connection from each connection point to the shape
				// One connection from each connection point to the next connection point
				foreach (ControlPointId cpi in s.GetControlPointIds(ControlPointCapabilities.Connect)) {
					Polyline polyline = (Polyline)project.ShapeTypes["Polyline"].CreateInstance();
					polyline.Connect(1, s, cpi);
					polyline.Connect(2, s, ControlPointId.Reference);
					Assert.AreNotEqual(ControlPointId.None, s.IsConnected(ControlPointId.Any, polyline));
					Assert.AreNotEqual(ControlPointId.None, s.IsConnected(cpi, polyline));
					Assert.AreNotEqual(ControlPointId.None, s.IsConnected(ControlPointId.Reference, polyline));
				}
				// Control point manipulation
					Point p = s.GetControlPointPosition(1);
					if (s.MoveControlPointBy(1, -10, -10, ResizeModifiers.None)) {
						p.Offset(-10, -10);
						Assert.AreEqual(p, s.GetControlPointPosition(1));
						s.MoveControlPointTo(1, p.X + 10, p.Y + 10, ResizeModifiers.None);
						p.Offset(+10, +10);
						Assert.AreEqual(p, s.GetControlPointPosition(1));
					}
			}
			project.Close();
		}


		[TestMethod]
		public void AggregationTest() {
			// -- Create a project --
			Project project = new Project();
			project.Name = "Test";
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore("C:\\Temp", ".xml");
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			Diagram diagram = new Diagram("Diagram A");
			// Create a group
			ShapeGroup group = (ShapeGroup)project.ShapeTypes["ShapeGroup"].CreateInstance();
			group.Children.Add(project.ShapeTypes["Circle"].CreateInstance(), 1);
			group.Children.Add(project.ShapeTypes["Square"].CreateInstance(), 2);
			group.Children.Add(project.ShapeTypes["Diamond"].CreateInstance(), 3);
			group.MoveTo(200, 200);
			diagram.Shapes.Add(group, 1);
			// Create an aggregation
			Box box = (Box)project.ShapeTypes["Box"].CreateInstance();
			box.Children.Add(project.ShapeTypes["Circle"].CreateInstance(), 1);
			box.Children.Add(project.ShapeTypes["Square"].CreateInstance(), 2);
			box.Children.Add(project.ShapeTypes["Diamond"].CreateInstance(), 3);
			box.MoveTo(400, 200);
			diagram.Shapes.Add(box, 2);
			// Save everything
			project.Repository.InsertDiagram(diagram);
			project.Repository.SaveChanges();
			project.Close();
			//
			// -- Reload and modify --
			project.Open();
			foreach (Diagram d in project.Repository.GetDiagrams())
				diagram = d;
			group = (ShapeGroup)diagram.Shapes.Bottom;
			Shape shape = null;
			foreach (Shape s in group.Children)
				shape = s;
			group.Children.Remove(shape);
			project.Repository.DeleteShape(shape);
			box = (Box)diagram.Shapes.TopMost;
			foreach (Shape s in box.Children)
				shape = s;
			box.Children.Remove(shape);
			project.Repository.DeleteShape(shape);
			project.Repository.SaveChanges();
			project.Close();
			//
			// -- Check --
			project.Open();
			foreach (Diagram d in project.Repository.GetDiagrams()) {
				project.Repository.GetDiagramShapes(d);
				foreach (Shape s in d.Shapes)
					Assert.AreEqual(2, s.Children.Count);
			}
			project.Close();
		}


		[TestMethod]
		public void TemplateTest() {
			Project project = new Project();
			project.Name = "Test";
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore("C:\\Temp", ".xml");
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			Template template = new Template("Template1", project.ShapeTypes["RoundedBox"].CreateInstance());
			((IPlanarShape)template.Shape).FillStyle = project.Design.FillStyles.Red;
			project.Repository.InsertTemplate(template);
			Diagram diagram = new Diagram("Diagram A");
			diagram.Shapes.Add(template.CreateShape(), 1);
			project.Repository.InsertDiagram(diagram);
			Assert.ReferenceEquals(((IPlanarShape)diagram.Shapes.Bottom).FillStyle, ((IPlanarShape)template.Shape).FillStyle);
			IFillStyle fillStyle = project.Design.FillStyles.Green;
			((IPlanarShape)template.Shape).FillStyle = fillStyle;
			Assert.ReferenceEquals(((IPlanarShape)diagram.Shapes.Bottom).FillStyle, ((IPlanarShape)template.Shape).FillStyle);
			project.Repository.SaveChanges();
			project.Close();
			//
			project.Open();
			template = project.Repository.GetTemplate("Template1");
			diagram = project.Repository.GetDiagram("Diagram A");
			Assert.AreEqual(((IPlanarShape)diagram.Shapes.Bottom).FillStyle.BaseColorStyle, ((IPlanarShape)template.Shape).FillStyle.BaseColorStyle);
			project.Close();
		}


		[TestMethod]
		public void CommandTest() {
			// Initialize the project
			Project project = new Project();
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore("C:\\Temp", ".xml");
			project.Name = "Test";
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			// Create a diagram with one shape
			Diagram diagram = new Diagram("Diagram A");
			project.Repository.InsertDiagram(diagram);
			Shape shape = project.ShapeTypes["Square"].CreateInstance();
			shape.X = 100;
			shape.Y = 100;
			// Execute commands
			project.ExecuteCommand(new InsertShapeCommand(diagram, LayerIds.None, shape, true, false));
			project.ExecuteCommand(new MoveShapeByCommand(shape, 200, 200));
			project.History.Undo();
			project.History.Undo();
			Assert.AreEqual(diagram.Shapes.Count, 0);
			project.History.Redo();
			Assert.AreEqual(diagram.Shapes.Count, 1);
			Assert.AreEqual(diagram.Shapes.Bottom.X, 100);
			project.History.Redo();
			Assert.AreEqual(diagram.Shapes.Bottom.X, 300);
			project.Repository.SaveChanges();
			project.Close();
		}


		[TestMethod]
		public void StylesTest() {
			Project project = new Project();
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore("C:\\Temp", ".xml");
			project.Name = "Test";
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			ColorStyle colorStyle = (ColorStyle)project.Design.ColorStyles.Blue;
			colorStyle.Color = Color.LightBlue;
			project.Repository.UpdateStyle(colorStyle);
			project.Repository.SaveChanges();
			project.Close();
			project.Open();
			colorStyle = (ColorStyle)project.Design.ColorStyles.Blue;
			Assert.AreEqual(Color.LightBlue.ToArgb(), colorStyle.Color.ToArgb());
			project.Close();
		}


		[TestMethod]
		public void BoundingRectangleTest() {
			// -- Create a project --
			Project project = new Project();
			project.Name = "BoundingRectangleTest";
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = new XmlStore(@"C:\Temp", ".xml");
			project.Repository.Erase();
			project.Create();

			// Add Libraries:
			// GeneralShapes
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			// ElectricalShapes
			project.AddLibrary(typeof(Dataweb.NShape.ElectricalShapes.AutoDisconnectorSymbol).Assembly);
			// FlowChartShapes
			project.AddLibrary(typeof(Dataweb.NShape.FlowChartShapes.ProcessSymbol).Assembly);
			// SoftwareArchitectureShapes
			project.AddLibrary(typeof(Dataweb.NShape.SoftwareArchitectureShapes.CloudSymbol).Assembly);

			//
			Diagram diagram = new Diagram("All Shapes");
			diagram.Width = 500;
			diagram.Height = 500;
			project.Repository.InsertDiagram(diagram);
			int shapeCount = 0;
			foreach (ShapeType st in project.ShapeTypes) {
				Shape s = st.CreateInstance();
				diagram.Shapes.Add(s, shapeCount + 1);
				project.Repository.InsertShape(s, diagram);
				++shapeCount;
			}
			
			BoundingRectangleTestCore(diagram.Shapes, 0, 100, 0);
			BoundingRectangleTestCore(diagram.Shapes, 200, 100, 300);

			project.Close();
		}


		[TestMethod]
		public void XMLRepositoryTest() {
			string dirName = "C:\\Temp";
			string extension = ".xml";
			RepositoryTestCore(new XmlStore(dirName, extension), new XmlStore(dirName, extension));
		}


		[TestMethod]
		public void SQLRepositoryTest() {
			string server = Environment.MachineName + "\\SQLEXPRESS";
			string database = "TurboDiagram";
			RepositoryTestCore(new SqlStore(server, database), new SqlStore(server, database));
		}


		[TestMethod]
		public void XMLLargeDiagramTest() {
			string timerName = "XMLStore Timer";
			TestContext.BeginTimer(timerName);
			LargeDiagramCore(new XmlStore("C:\\Temp", ".xml"));
			TestContext.EndTimer(timerName);
		}


		[TestMethod]
		public void SQLLargeDiagramTest() {
			string timerName = "SqlStore Timer";
			TestContext.BeginTimer(timerName);
			LargeDiagramCore(new SqlStore(Environment.MachineName + "\\SQLEXPRESS", "TurboDiagram"));
			TestContext.EndTimer(timerName);
		}

		#endregion


		#region Test core methods

		private void RepositoryTestCore(Store store1, Store store2) {
			string projectName = "Repository Test";
			
			// Create reference project and repository
			Project project1 = new Project();
			project1.Name = projectName;
			project1.Repository = new CachedRepository();
			((CachedRepository)project1.Repository).Store = store1;
			project1.Repository.Erase();
			project1.Create();
			project1.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);

			// Create test data, populate repository, save repository
			string diagramName = "Diagram";
			CreateDiagram(project1, diagramName, 10, 10, true, true, true, true);
			project1.Repository.SaveChanges();

			// Create another project and populate repository from saved store
			Project project2 = new Project();
			project2.Name = projectName;
			project2.Repository = new CachedRepository();
			((CachedRepository)project2.Repository).Store = store2;
			project2.Open();

			// Compare the saved repository with the loaded repository
			//
			// Compare Designs
			Comparer.Compare(((IStyleSetProvider)project1).StyleSet, ((IStyleSetProvider)project2).StyleSet);
			foreach (Design d in project2.Repository.GetDesigns())
				Comparer.Compare(d, project2.Repository.GetDesign(((IEntity)d).Id));
			
			// Compare Templates
			foreach (Template savedTemplate in project1.Repository.GetTemplates())
			   Comparer.Compare(savedTemplate, project2.Repository.GetTemplate(savedTemplate.Id));
			
			// Compare ModelObjects
			foreach (IModelObject modelObject in project1.Repository.GetModelObjects(null))
				Comparer.Compare(modelObject, project2.Repository.GetModelObject(modelObject.Id));
			
			// Compare Diagrams
			foreach (Diagram diagram in project1.Repository.GetDiagrams()) {
				Comparer.Compare(diagram, project2.Repository.GetDiagram(((IEntity)diagram).Id));

				// Compare ZOrder and LayerIds
				List<Shape> savedShapes = new List<Shape>(diagram.Shapes.BottomUp);
				Diagram loadedDiagram = project2.Repository.GetDiagram(((IEntity)diagram).Id);
				List<Shape> loadedShapes = new List<Shape>(loadedDiagram.Shapes.BottomUp);

				for (int i = savedShapes.Count - 1; i >= 0; --i) {
				}
			}
		}


		private void LargeDiagramCore(Store store) {
			string projectName = "Large Diagram Test";
			Project project = new Project();
			project.Name = projectName;
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = store;
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);

			string diagramName = "Large Diagram";
			CreateLargeDiagram(project, diagramName);

			project.Repository.SaveChanges();
			Trace.WriteLine("Saved!");
			project.Close();
		}


		private void BoundingRectangleTestCore(IShapeCollection shapes, int shapePos, int shapeSize, int shapeAngle) {
			foreach (Shape s in shapes) {
				// Move shape
				s.MoveTo(shapePos, shapePos);

				// Resize shape
				if (s is RectangleBase) {
					((RectangleBase)s).Width = shapeSize;
					((RectangleBase)s).Height = shapeSize;
				} else if (s is DiamondBase) {
					((DiamondBase)s).Width = shapeSize;
					((DiamondBase)s).Height = shapeSize;
				} else if (s is CircleBase)
					((CircleBase)s).Diameter = shapeSize;
				else if (s is SquareBase)
					((SquareBase)s).Size = shapeSize;
				else if (s is PolygonBase) {
					s.MoveControlPointTo(1, -shapeSize / 2, -shapeSize / 2, ResizeModifiers.None);
					s.MoveControlPointTo(2, -shapeSize / 2, shapeSize / 2, ResizeModifiers.None);
					s.MoveControlPointTo(3, shapeSize / 2, shapeSize / 2, ResizeModifiers.None);
				} else if (s is PolylineBase) {
					s.MoveControlPointTo(ControlPointId.FirstVertex, -shapeSize / 2, -shapeSize / 2, ResizeModifiers.None);
					s.MoveControlPointTo(ControlPointId.LastVertex, shapeSize / 2, shapeSize / 2, ResizeModifiers.None);
				} else if (s is CircularArcBase) {
					// ToDo: Add third point
					s.MoveControlPointTo(ControlPointId.FirstVertex, -shapeSize / 2, -shapeSize / 2, ResizeModifiers.None);
					s.MoveControlPointTo(ControlPointId.LastVertex, shapeSize / 2, shapeSize / 2, ResizeModifiers.None);
				} else if (s is ShapeGroup) {
					s.Children.Add(shapes.TopMost);
				} //else s.Fit(-shapeSize / 2, -shapeSize / 2, shapeSize, shapeSize);

				// rotate shape
				if (shapeAngle != 0 && s is IPlanarShape)
					((IPlanarShape)s).Angle = shapeAngle;

				Rectangle tightBounds = s.GetBoundingRectangle(true);
				Rectangle looseBounds = s.GetBoundingRectangle(false);

				Assert.IsTrue(tightBounds != Geometry.InvalidRectangle);
				Assert.IsTrue(looseBounds != Geometry.InvalidRectangle);
				Assert.IsTrue(!tightBounds.IsEmpty);
				Assert.IsTrue(!looseBounds.IsEmpty);

				Assert.IsTrue(tightBounds.Width >= 0);
				Assert.IsTrue(tightBounds.Height >= 0);
				Assert.IsTrue(looseBounds.Width >= 0);
				Assert.IsTrue(looseBounds.Height >= 0);

				Assert.IsTrue(tightBounds.X >= looseBounds.X);
				Assert.IsTrue(tightBounds.Y >= looseBounds.Y);
				Assert.IsTrue(tightBounds.Width <= looseBounds.Width);
				Assert.IsTrue(tightBounds.Height <= looseBounds.Height);
			}
		}

		#endregion


		#region Helper methods

		private void CreateLargeDiagram(Project project, string diagramName) {
			const int shapesPerSide = 100;
			CreateDiagram(project, diagramName, shapesPerSide, shapesPerSide, true, false, false, true);
		}


		private void CreateDiagram(Project project, string diagramName, int shapesPerRow, int shapesPerColumn, bool connectShapes, bool withModels, bool withModelMappings, bool withLayers) {
			const int shapeSize = 80;
			int lineLength = shapeSize / 2;
			//
			// Create ModelMappings
			NumericModelMapping numericModelMapping = null;
			FormatModelMapping formatModelMapping = null;
			StyleModelMapping styleModelMapping = null;
			if (withModelMappings) {
				// Create numeric- and format model mappings
				numericModelMapping = new NumericModelMapping(2, 4, NumericModelMappingType.FloatInteger, 10, 0);
				formatModelMapping = new FormatModelMapping(4, 2, FormatModelMappingType.StringString, "{0}");
				// Create style model mapping
				int range = (shapesPerRow * shapesPerColumn) / 15;
				styleModelMapping = new StyleModelMapping(1, 3, StyleModelMappingType.IntegerStyle);
				styleModelMapping.AddValueRange(0 * range, project.Design.LineStyles.None);
				styleModelMapping.AddValueRange(1 * range, project.Design.LineStyles.Dotted);
				styleModelMapping.AddValueRange(2 * range, project.Design.LineStyles.Dashed);
				styleModelMapping.AddValueRange(3 * range, project.Design.LineStyles.Special1);
				styleModelMapping.AddValueRange(4 * range, project.Design.LineStyles.Special2);
				styleModelMapping.AddValueRange(5 * range, project.Design.LineStyles.Normal);
				styleModelMapping.AddValueRange(6 * range, project.Design.LineStyles.Blue);
				styleModelMapping.AddValueRange(7 * range, project.Design.LineStyles.Green);
				styleModelMapping.AddValueRange(8 * range, project.Design.LineStyles.Yellow);
				styleModelMapping.AddValueRange(9 * range, project.Design.LineStyles.Red);
				styleModelMapping.AddValueRange(10 * range, project.Design.LineStyles.HighlightDotted);
				styleModelMapping.AddValueRange(11 * range, project.Design.LineStyles.HighlightDashed);
				styleModelMapping.AddValueRange(12 * range, project.Design.LineStyles.Highlight);
				styleModelMapping.AddValueRange(13 * range, project.Design.LineStyles.HighlightThick);
				styleModelMapping.AddValueRange(14 * range, project.Design.LineStyles.Thick);
			}
			//
			// Create model obejct for the planar shape's template
			IModelObject planarModel = null;
			if (withModels) planarModel = project.ModelObjectTypes["Core.GenericModelObject"].CreateInstance();
			//
			// Create a shape for the planar shape's template
			Circle circleShape = (Circle)project.ShapeTypes["Circle"].CreateInstance();
			circleShape.Diameter = shapeSize;
			//
			// Create a template for the planar shapes
			Template planarTemplate = new Template("PlanarShape Template", circleShape);
			if (withModels) {
				planarTemplate.Shape.ModelObject = planarModel;
				if (withModelMappings) {
					planarTemplate.MapProperties(numericModelMapping);
					planarTemplate.MapProperties(formatModelMapping);
					planarTemplate.MapProperties(styleModelMapping);
				}
			}
			// 
			// Create a template for the linear shapes
			Template linearTemplate = null;
			if (connectShapes)
				linearTemplate = new Template("LinearShape Template", project.ShapeTypes["Polyline"].CreateInstance());
			//
			// Insert the created templates into the repository
			project.Repository.InsertTemplate(planarTemplate);
			foreach (IModelMapping modelMapping in planarTemplate.GetPropertyMappings())
				project.Repository.InsertModelMapping(modelMapping, planarTemplate);
			if (connectShapes) {
				project.Repository.InsertTemplate(linearTemplate);
				foreach (IModelMapping modelMapping in linearTemplate.GetPropertyMappings())
					project.Repository.InsertModelMapping(modelMapping, linearTemplate);
			}
			//
			// Prepare the connection points
			ControlPointId leftPoint = withModels ? ControlPointId.Reference : 4;
			ControlPointId rightPoint = withModels ? ControlPointId.Reference : 5;
			ControlPointId topPoint = withModels ? ControlPointId.Reference : 2;
			ControlPointId bottomPoint = withModels ? ControlPointId.Reference : 7;
			//
			// Create the diagram
			Diagram diagram = new Diagram(diagramName);
			//
			// Create and add layers
			LayerIds planarLayer = LayerIds.None, linearLayer = LayerIds.None, oddRowLayer = LayerIds.None,
				evenRowLayer = LayerIds.None, oddColLayer = LayerIds.None, evenColLayer = LayerIds.None;
			if (withLayers) {
				const string planarLayerName = "PlanarShapesLayer";
				const string linearLayerName = "LinearShapesLayer";
				const string oddRowsLayerName = "OddRowsLayer";
				const string evenRowsLayerName = "EvenRowsLayer";
				const string oddColsLayerName = "OddColsLayer";
				const string evenColsLayerName = "EvenColsLayer";
				// Create Layers
				Layer planarShapesLayer = new Layer(planarLayerName);
				planarShapesLayer.Title = "Planar Shapes";
				planarShapesLayer.LowerZoomThreshold = 5;
				planarShapesLayer.UpperZoomThreshold = 750;
				diagram.Layers.Add(planarShapesLayer);
				Layer linearShapesLayer = new Layer(linearLayerName);
				linearShapesLayer.Title = "Linear Shapes";
				linearShapesLayer.LowerZoomThreshold = 10;
				linearShapesLayer.UpperZoomThreshold = 500;
				diagram.Layers.Add(linearShapesLayer);
				Layer oddRowsLayer = new Layer(oddRowsLayerName);
				oddRowsLayer.Title = "Odd Rows";
				oddRowsLayer.LowerZoomThreshold = 2;
				oddRowsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(oddRowsLayer);
				Layer evenRowsLayer = new Layer(evenRowsLayerName);
				evenRowsLayer.Title = "Even Rows";
				evenRowsLayer.LowerZoomThreshold = 2;
				evenRowsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(evenRowsLayer);
				Layer oddColsLayer = new Layer(oddColsLayerName);
				oddColsLayer.Title = "Odd Columns";
				oddColsLayer.LowerZoomThreshold = 2;
				oddColsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(oddColsLayer);
				Layer evenColsLayer = new Layer(evenColsLayerName);
				evenColsLayer.Title = "Even Columns";
				evenColsLayer.LowerZoomThreshold = 2;
				evenColsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(evenColsLayer);
				// Assign LayerIds
				planarLayer = diagram.Layers.FindLayer(planarLayerName).Id;
				linearLayer = diagram.Layers.FindLayer(linearLayerName).Id;
				oddRowLayer = diagram.Layers.FindLayer(oddRowsLayerName).Id;
				evenRowLayer = diagram.Layers.FindLayer(evenRowsLayerName).Id;
				oddColLayer = diagram.Layers.FindLayer(oddColsLayerName).Id;
				evenColLayer = diagram.Layers.FindLayer(evenColsLayerName).Id;
			}

			for (int rowIdx = 0; rowIdx < shapesPerRow; ++rowIdx) {
				LayerIds rowLayer = ((rowIdx + 1) % 2 == 0) ? evenRowLayer : oddRowLayer;
				for (int colIdx = 0; colIdx < shapesPerRow; ++colIdx) {
					LayerIds colLayer = ((colIdx + 1) % 2 == 0) ? evenColLayer : oddColLayer;
					int shapePosX = shapeSize + colIdx * (lineLength + shapeSize);
					int shapePosY = shapeSize + rowIdx * (lineLength + shapeSize);

					circleShape = (Circle)planarTemplate.CreateShape();
					circleShape.Text = string.Format("{0} / {1}", rowIdx + 1, colIdx + 1);
					circleShape.MoveTo(shapePosX, shapePosY);
					if (withModels) {
						project.Repository.InsertModelObject(circleShape.ModelObject);
						((GenericModelObject)circleShape.ModelObject).IntegerValue = rowIdx;
					}

					diagram.Shapes.Add(circleShape, project.Repository.ObtainNewTopZOrder(diagram));
					if (withLayers) diagram.AddShapeToLayers(circleShape, planarLayer | rowLayer | colLayer);
					if (connectShapes) {
						if (rowIdx > 0) {
							Shape lineShape = linearTemplate.CreateShape();
							lineShape.Connect(ControlPointId.FirstVertex, circleShape, topPoint);
							Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.FirstVertex, circleShape));

							Shape otherShape = diagram.Shapes.FindShape(shapePosX, shapePosY - shapeSize, ControlPointCapabilities.None, 0, null);
							lineShape.Connect(ControlPointId.LastVertex, otherShape, bottomPoint);
							diagram.Shapes.Add(lineShape, project.Repository.ObtainNewBottomZOrder(diagram));
							if (withLayers) diagram.AddShapeToLayers(lineShape, linearLayer);
							Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.LastVertex, otherShape));
						}
						if (colIdx > 0) {
							Shape lineShape = linearTemplate.CreateShape();
							lineShape.Connect(1, circleShape, leftPoint);
							Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.FirstVertex, circleShape));

							Shape otherShape = diagram.Shapes.FindShape(shapePosX - shapeSize, shapePosY, ControlPointCapabilities.None, 0, null);
							lineShape.Connect(2, otherShape, rightPoint);
							diagram.Shapes.Add(lineShape, project.Repository.ObtainNewBottomZOrder(diagram));
							if (withLayers) diagram.AddShapeToLayers(lineShape, linearLayer);
							Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.LastVertex, otherShape));
						}
					}
				}
			}
			diagram.Width = (lineLength + shapeSize) * shapesPerRow + 2 * shapeSize;
			diagram.Height = (lineLength + shapeSize) * shapesPerColumn + 2 * shapeSize;
			project.Repository.InsertDiagram(diagram);
		}

		#endregion


		private TestContext testContextInstance;
	}
}
