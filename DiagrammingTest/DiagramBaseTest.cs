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
using System.Data.SqlClient;
using Dataweb.Utilities;
using System.Drawing;
using System.IO;


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
			((CachedRepository)project.Repository).Store = CreateXmlStore();
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
			((CachedRepository)project.Repository).Store = CreateXmlStore();
			project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly);
			//
			foreach (ShapeType st in project.ShapeTypes) {
				Shape s = st.CreateInstance();
				if (s is TextBase) ((TextBase)s).AutoSize = false;
				//
				// -- Test Properties --
				s.Fit(0, 0, 40, 40);	// ensure that the shape has a defined size
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
				Assert.IsTrue(Geometry.RectangleContainsPoint(bounds1, oiPoint));
				//
				Assert.IsTrue(s.IntersectsWith(bounds1.X, bounds1.Y, bounds1.Width, bounds1.Height));
				//
				bounds1 = new Rectangle(500, 500, 100, 100);
				s.Fit(bounds1.X, bounds1.Y, bounds1.Width, bounds1.Height);
				bounds1.Inflate(10, 10);
				Assert.IsTrue(Geometry.RectangleContainsRectangle(bounds1, s.GetBoundingRectangle(true)));
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
			((CachedRepository)project.Repository).Store = CreateXmlStore();
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
			((CachedRepository)project.Repository).Store = CreateXmlStore();
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
			((CachedRepository)project.Repository).Store = CreateXmlStore();
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
			((CachedRepository)project.Repository).Store = CreateXmlStore();
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
			string timerName;

			// Test inserting, modifying and deleting objects from repository
			timerName = "RepositoryTest XMLStore Timer";
			TestContext.BeginTimer(timerName);
			RepositoryTestCore(CreateXmlStore(), CreateXmlStore());
			TestContext.EndTimer(timerName);

			// Test inserting large diagrams
			timerName = "LargeDiagramTest XMLStore Timer";
			TestContext.BeginTimer(timerName);
			LargeDiagramCore(CreateXmlStore());
			TestContext.EndTimer(timerName);
		}


		[TestMethod]
		public void SQLRepositoryTest() {
			string server = Environment.MachineName + databaseServerType;
			string timerName;

			try {
				SQLCreateDatabase();

				// Test inserting, modifying and deleting objects from repository
				timerName = "RepositoryTest SqlStore Timer";
				TestContext.BeginTimer(timerName);
				RepositoryTestCore(CreateSqlStore(), CreateSqlStore());
				TestContext.EndTimer(timerName);

				// Test inserting large diagrams
				timerName = "LargeDiagramTest SqlStore Timer";
				TestContext.BeginTimer(timerName);
				LargeDiagramCore(CreateSqlStore());
				TestContext.EndTimer(timerName);

			} finally {
				SQLDropDatabase();
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
				CreateDiagram(project1, diagramName, shapesPerRow, shapesPerRow, true, true, true, true);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				Comparer.Compare(project1, project2);
				project2.Close();

				// Modify (and insert) content of the repository and save it
				ModifyContent(project1);
				InsertContent(project1);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				Comparer.Compare(project1, project2);
				project2.Close();

				// Delete various data from project
				DeleteContent(project1);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				Comparer.Compare(project1, project2);
				project2.Close();
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
				CreateLargeDiagram(project, diagramName);

				project.Repository.SaveChanges();
				Trace.WriteLine("Saved!");
			} finally {
				project.Close();
			}
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
				//else if (s is PolygonBase) {
				//   s.MoveControlPointTo(1, -shapeSize / 2, -shapeSize / 2, ResizeModifiers.None);
				//   s.MoveControlPointTo(2, -shapeSize / 2, shapeSize / 2, ResizeModifiers.None);
				//   s.MoveControlPointTo(3, shapeSize / 2, shapeSize / 2, ResizeModifiers.None);
				//} 
				else if (s is PolylineBase) {
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

				Assert.IsTrue(Geometry.IsValid(tightBounds));
				Assert.IsTrue(Geometry.IsValid(looseBounds));
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


		#region Repository test helper methods

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
				numericModelMapping = new NumericModelMapping(2, 4, NumericModelMapping.MappingType.FloatInteger, 10, 0);
				formatModelMapping = new FormatModelMapping(4, 2, FormatModelMapping.MappingType.StringString, "{0}");
				// Create style model mapping
				float range = (shapesPerRow * shapesPerColumn) / 15f;
				styleModelMapping = new StyleModelMapping(1, 4, StyleModelMapping.MappingType.FloatStyle);
				for (int i = 0; i < 15; ++i) {
					IStyle style = null;
					switch (i) {
						case 0: style = project.Design.LineStyles.None; break;
						case 1: style = project.Design.LineStyles.Dotted; break;
						case 2: style = project.Design.LineStyles.Dashed; break;
						case 3: style = project.Design.LineStyles.Special1; break;
						case 4: style = project.Design.LineStyles.Special2; break;
						case 5: style = project.Design.LineStyles.Normal; break;
						case 6: style = project.Design.LineStyles.Blue; break;
						case 7: style = project.Design.LineStyles.Green; break;
						case 8: style = project.Design.LineStyles.Yellow; break;
						case 9: style = project.Design.LineStyles.Red; break;
						case 10: style = project.Design.LineStyles.HighlightDotted; break;
						case 11: style = project.Design.LineStyles.HighlightDashed; break;
						case 12: style = project.Design.LineStyles.Highlight; break;
						case 13: style = project.Design.LineStyles.HighlightThick; break;
						case 14: style = project.Design.LineStyles.Thick; break;
						default: style = null; break;
					}
					if (style != null) styleModelMapping.AddValueRange(i * range, style);
				}
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


		private SqlStore CreateSqlStore() {
			string server = Environment.MachineName + databaseServerType;
			return new SqlStore(server, databaseName);
		}


		private XmlStore CreateXmlStore() {
			return new XmlStore(Path.GetTempPath(), ".xml");
		}


		private void SQLCreateDatabase() {
			using (SqlStore sqlStore = CreateSqlStore()) {
				// Create database
				string connectionString = string.Format("server={0};Integrated Security=True", sqlStore.ServerName);
				using (SqlConnection conn = new SqlConnection(connectionString)) {
					conn.Open();
					try {
						SqlCommand command = conn.CreateCommand();
						command.CommandText = string.Format("CREATE DATABASE {0}", databaseName);
						command.ExecuteNonQuery();
					} catch (SqlException exc) {
						// Ignore "Database already exists" error
						if (exc.ErrorCode != sqlErrDatabaseExists) throw exc;
					}
				}

				// Create Repository
				CachedRepository repository = new CachedRepository();
				repository.Store = CreateSqlStore();

				// Create project
				Project project = new Project();
				project.Name = "NShape SQL Test";
				project.Repository = repository;

				// Add and register libraries
				project.RemoveAllLibraries();
				project.AddLibraryByName("Dataweb.NShape.GeneralModelObjects");
				project.AddLibrary(typeof(Circle).Assembly);
				project.RegisterEntityTypes();

				// Create schema
				sqlStore.CreateDbCommands(repository);
				sqlStore.CreateDbSchema(repository);

				// Close project
				project.Close();
			}
		}


		private void SQLDropDatabase() {
			string connectionString = string.Empty;
			using (SqlStore sqlStore = CreateSqlStore()) {
				connectionString = string.Format("server={0};Integrated Security=True", sqlStore.ServerName);
				sqlStore.DropDbSchema();
			}

			// Drop database
			if (!string.IsNullOrEmpty(connectionString)) {
				using (SqlConnection conn = new SqlConnection(connectionString)) {
					conn.Open();
					try {
						using (SqlCommand command = conn.CreateCommand()) {
							command.CommandText = string.Format("DROP DATABASE {0}", databaseName);
							command.ExecuteNonQuery();
						}
					} catch (SqlException exc) {
						if (exc.ErrorCode != sqlErrDatabaseExists) throw exc;
					}
				}
			}
		}


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
				else newStyle.BaseColorStyle = design.ColorStyles[colorStyleName];
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
		private const int sqlErrDatabaseExists = -2146232060;
		private const string databaseServerType = "\\SQLEXPRESS";
		private const string databaseName = "NShapeSQLTest";

		private const string NewNamePrefix = "Copy of";
		private const string ModifiedNamePrefix = "Modified";
	}

}
