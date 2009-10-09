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
using System.Windows.Forms;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.GeneralShapes;


namespace nShape_Model_Demo {

	public partial class MainForm : Form {
		
		public MainForm() {
			InitializeComponent();
		}

		
		private void MainForm_Load(object sender, EventArgs e) {
			// Create a new Project
			project.Name = " nShape Model Demo";
			project.AutoGenerateTemplates = false;
			project.Create();
			project.AddLibraryByName("Dataweb.nShape.GeneralShapes");
			project.AddLibrary(this.GetType().Assembly);

			CreateTemplates();

			CreateDiagram();

			controlForm.ValueChanged += new EventHandler<EventArgs>(controlForm_ValueChanged);
			controlForm.Show(this);
		}

		
		private void CreateTemplates() {
			// Battery Template
			RoundedBox batteryShape = (RoundedBox)project.ShapeTypes["RoundedBox"].CreateInstance();
			Battery batteryModel = (Battery)project.ModelObjectTypes["Battery"].CreateInstance();
			batteryShape.ModelObject = batteryModel;
			batteryShape.Width = 120;
			batteryShape.Height = 80;
			batteryShape.FillStyle = project.Design.FillStyles.White;
			batteryShape.LineStyle = project.Design.LineStyles.Thick;
			Template batteryTemplate = new Template(BatteryTemplateName, batteryShape);
			batteryTemplate.MapTerminal(Battery.OutputTerminal, ControlPointId.Reference);
			// 4 == PropertyIdText
			FormatModelMapping batteryMapping = new FormatModelMapping(4, Conductor.CurrencyPropertyId, FormatModelMappingType.IntegerString, "{0} mA");
			batteryTemplate.MapProperties(batteryMapping);
			toolSetController.CreateTemplateTool(batteryTemplate, null);

			// Wire Template
			Polyline wireShape = (Polyline)project.ShapeTypes["Polyline"].CreateInstance();
			Wire wireModel = (Wire)project.ModelObjectTypes["Wire"].CreateInstance();
			wireShape.ModelObject = wireModel;
			Template wireTemplate = new Template(WireTemplateName, wireShape);
			wireTemplate.MapTerminal(Wire.ConnectionTerminal1, ControlPointId.FirstVertex);
			wireTemplate.MapTerminal(Wire.ConnectionTerminal2, ControlPointId.LastVertex);
			// 1 = PropertyIdLineStyle
			StyleModelMapping wireMapping = new StyleModelMapping(1, Conductor.CurrencyPropertyId, StyleModelMappingType.IntegerStyle);
			wireMapping.AddValueRange(int.MinValue, project.Design.LineStyles.Normal);
			wireMapping.AddValueRange(1, project.Design.LineStyles.Green);
			wireMapping.AddValueRange(500, project.Design.LineStyles.Yellow);
			wireMapping.AddValueRange(1000, project.Design.LineStyles.Red);
			wireTemplate.MapProperties(wireMapping);
			toolSetController.CreateTemplateTool(wireTemplate, null);

			// Switch Template
			Box switchShape = (Box)project.ShapeTypes["Box"].CreateInstance();
			Switch switchModel = (Switch)project.ModelObjectTypes["Switch"].CreateInstance();
			switchShape.ModelObject = switchModel;
			switchShape.Width = switchShape.Height = 40;
			switchShape.FillStyle = project.Design.FillStyles.Red;
			Template switchTemplate = new Template(SwitchTemplateName, switchShape);
			switchTemplate.MapTerminal(Switch.ConnectionTerminal1, 4);
			switchTemplate.MapTerminal(Switch.ConnectionTerminal2, 5);
			FormatModelMapping switchStateMapping = new FormatModelMapping(4, Switch.StatePropertyId, FormatModelMappingType.IntegerString, "{0}");
			StyleModelMapping switchColorMapping = new StyleModelMapping(3, Switch.StatePropertyId, StyleModelMappingType.IntegerStyle);
			switchColorMapping.AddValueRange(int.MinValue, project.Design.FillStyles.Red);
			switchColorMapping.AddValueRange(1, project.Design.FillStyles.Green);
			StyleModelMapping switchLineMapping = new StyleModelMapping(1, Switch.StatePropertyId, StyleModelMappingType.IntegerStyle);
			switchLineMapping.AddValueRange(int.MinValue, project.Design.LineStyles.Dotted);
			switchLineMapping.AddValueRange(1, project.Design.LineStyles.Normal);
			switchTemplate.MapProperties(switchStateMapping);
			switchTemplate.MapProperties(switchLineMapping);
			switchTemplate.MapProperties(switchColorMapping);
			toolSetController.CreateTemplateTool(switchTemplate, null);

			// Consumer Template
			Circle consumerShape = (Circle)project.ShapeTypes["Circle"].CreateInstance();
			Consumer consumerModel = (Consumer)project.ModelObjectTypes["Consumer"].CreateInstance();
			consumerShape.ModelObject = consumerModel;
			consumerShape.Diameter = 80;
			consumerShape.FillStyle = project.Design.FillStyles.Black;
			Template consumerTemplate = new Template(ConsumerTemplateName, consumerShape);
			consumerTemplate.MapTerminal(Consumer.InputTerminal, 7);
			StyleModelMapping consumerColorMapping = new StyleModelMapping(3, Conductor.CurrencyPropertyId, StyleModelMappingType.IntegerStyle);
			consumerColorMapping.AddValueRange(int.MinValue, project.Design.FillStyles.Black);
			consumerColorMapping.AddValueRange(100, project.Design.FillStyles.Red);
			consumerColorMapping.AddValueRange(200, project.Design.FillStyles.Yellow);
			consumerColorMapping.AddValueRange(300, project.Design.FillStyles.White);
			consumerColorMapping.AddValueRange(1000, project.Design.FillStyles.Transparent);
			StyleModelMapping consumerLineMapping = new StyleModelMapping(1, Conductor.CurrencyPropertyId, StyleModelMappingType.IntegerStyle);
			consumerLineMapping.AddValueRange(int.MinValue, project.Design.LineStyles.Normal);
			consumerLineMapping.AddValueRange(100, project.Design.LineStyles.Red);
			consumerLineMapping.AddValueRange(200, project.Design.LineStyles.Highlight);
			consumerLineMapping.AddValueRange(300, project.Design.LineStyles.HighlightThick);
			consumerLineMapping.AddValueRange(1000, project.Design.LineStyles.Dotted);
			consumerTemplate.MapProperties(consumerLineMapping);
			consumerTemplate.MapProperties(consumerColorMapping);
			toolSetController.CreateTemplateTool(consumerTemplate, null);
		}
		
		
		private void CreateDiagram() {
			Diagram diagram = new Diagram("Model Test Diagram");
			diagram.Width = 1000;
			diagram.Height = 1000;
			project.Repository.InsertDiagram(diagram);
			display.Diagram = diagram;
		}


		private void controlForm_ValueChanged(object sender, EventArgs e) {
			foreach (Shape s in display.Diagram.Shapes) {
				if (s.ModelObject is Battery)
					((Battery)s.ModelObject).Current = controlForm.Value;
			}
		}


		private void display_ShapeClick(object sender, DiagramPresenterShapeClickEventArgs e) {
			if (e.Mouse.Buttons == MouseButtonsDg.Left) {
				if (e.Shape.ModelObject is Switch) {
					bool state = ((Switch)e.Shape.ModelObject).State;
					System.Diagnostics.Debug.Print("Switch {0} clicked. Switching from {1} to {2}", e.Shape.ModelObject.Name, state, !state);
					((Switch)e.Shape.ModelObject).State = !state;
				}
			}
		}


		private const string BatteryTemplateName = "Battery";
		private const string WireTemplateName = "Wire";
		private const string SwitchTemplateName = "Switch";
		private const string ConsumerTemplateName = "Consumer";

		ControlForm controlForm = new ControlForm();
	}
}
