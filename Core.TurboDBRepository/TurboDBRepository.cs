using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

using Dataweb.TurboDBManaged;
using Dataweb.Diagramming;
using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming {

	public class TurboDBRepository : AdoNetStore {

		public TurboDBRepository() {
			base.ProviderName = "Dataweb.TurboDBManaged";
			
			sqlGenerator.OpeningFieldQualifier = "[";
			sqlGenerator.ClosingFieldQualifier = "]";
		}


		public string DataSource {
			get { 
				TurboDBConnection connection = new TurboDBConnection();
				connection.ConnectionString = ConnectionString;
				return connection.DataSource;
			}
		}


		public override void CreateDbCommands(IStoreCache cache) {
			//GenerateShapeEntityCommandSet(entityType);
			//GenerateShapeEntityCommandSet(entityType);
			//GenerateShapeEntityCommandSet(entityType);
			//GenerateShapeEntityCommandSet(entityType);
			//GenerateShapeEntityCommandSet(entityType);
			//GenerateShapeEntityCommandSet(entityType);
						
			//base.CreateDbCommands();
		}		


		private CommandSet CreateCommandSet(SqlGenerator sqlGenerator) {
			CommandSet result = CommandSet.Empty;
			IDbCommand cmd;

			cmd = CreateCommand();
			sqlGenerator.GetCreateTableCommand(cmd);
			result.CreateTableCommand = cmd;

			cmd = CreateCommand();
			sqlGenerator.GetSelectIdsCommand(cmd);
			result.SelectIdsCommand = cmd;

			cmd = CreateCommand();
			sqlGenerator.GetSelectByIdCommand(cmd);
			result.SelectByIdCommand =  cmd;

			cmd = CreateCommand();
			sqlGenerator.GetInsertCommand(cmd);
			result.InsertCommand = cmd;

			cmd = CreateCommand();
			sqlGenerator.GetUpdateCommand(cmd);
			result.UpdateCommand = cmd;

			cmd = CreateCommand();
			sqlGenerator.GetDeleteCommand(cmd);
			result.DeleteCommand = cmd;

			return result;
		}


		private void GenerateShapeEntityCommandSets() {
			//CommandSet commandSet;
			//SqlGenerator sqlGenerator = new SqlGenerator();
			//sqlGenerator.OpeningFieldQualifier = "[";
			//sqlGenerator.ClosingFieldQualifier = "]";

			//#region *** DESIGN TABLE ***
			////Id	AutoInc
			////Name	String(40)
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "Design";
			//sqlGenerator.AddField("Id", TurboDBType.AutoInc, ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String);
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateDesignTableCommand(commandSet.CreateTableCommand);
			//SetSelectDesignIdsCommand(commandSet.SelectIdsCommand);
			//SetSelectDesignByIdCommand(commandSet.SelectByIdCommand);
			//SetInsertDesignCommand(commandSet.InsertCommand);
			//SetUpdateDesignCommand(commandSet.UpdateCommand);
			//SetDeleteDesignCommand(commandSet.DeleteCommand);
			//#endregion

			//#region *** STYLE TABLE ***
			////There is a table that stores the style id and style type along with the design id and a separate table for each style type.
			//// the base style table is buildt according to the followig schema:
			//// DesignId		Integer
			//// Id				AutoInc
			//// Type	string
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "Style";
			//sqlGenerator.AddField("DesignId", TurboDBType.Integer);
			//sqlGenerator.AddField("Id", TurboDBType.AutoInc, ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("TStyle", TurboDBType.String, ColumnProperties.NotNull);
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(commandSet.CreateTableCommand);
			//commandSet.SelectIdsCommand.CommandText = commandSet.SelectIdsCommand.CommandText.ReplaceRange(";", string.Format(" WHERE {0}DesignId{1} = :DesignId", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier));
			//commandSet.SelectIdsCommand.Parameters.Add(new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetSelectStyleIdsCommand(commandSet.SelectIdsCommand);
			//SetSelectStyleTypeByIdCommand(commandSet.SelectByIdCommand);

			////The tables for the specific style types are all built according to the following schema.
			//// Id				Integer
			////…	…	Further properties
			//#endregion

			//#region *** COLORSTYLE TABLE ***
			//IEntityType entityType = null;

			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "ColorStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("Color", TurboDBType.Integer);
			//sqlGenerator.AddField("ColorTransparency", TurboDBType.Byte);
			//commandSet = CreateCommandSet(sqlGenerator);

			//entityType = styleEntityTypes[ColorStyle.EntityTypeName];
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);

			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);

			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + ColorStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + ColorStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);

			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);

			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);

			//#endregion

			//#region *** CAPSTYLE TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "CapStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("CapShape", TurboDBType.Byte);
			//sqlGenerator.AddField("CapSize", TurboDBType.Integer);
			//sqlGenerator.AddField("ColorStyle", TurboDBType.String);
			////sqlGenerator.AddField("ColorStyle", TurboDBType.Integer, ColumnProperties.ForeignKey);
			////sqlGenerator.AddMasterTable("ColorStyle", "ColorStyle", "Id");

			//entityType = styleEntityTypes[CapStyle.EntityTypeName];
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);
			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + CapStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + CapStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);
			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);
			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);
			//#endregion

			//#region *** FILLSTYLE TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "FillStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("BaseColorStyle", TurboDBType.String);
			//sqlGenerator.AddField("AdditionalColorStyle", TurboDBType.String);
			//sqlGenerator.AddField("FillMode", TurboDBType.Byte);
			//sqlGenerator.AddField("FillPattern", TurboDBType.Byte);
			//sqlGenerator.AddField("ImageLayout", TurboDBType.Integer);
			//sqlGenerator.AddField("ImageTransparency", TurboDBType.Byte);
			//sqlGenerator.AddField("ImageGammaCorrection", TurboDBType.Float);
			//sqlGenerator.AddField("ImageCompressionQuality", TurboDBType.Byte);
			//sqlGenerator.AddField("ImageFileName", TurboDBType.String);
			//sqlGenerator.AddField("Image", TurboDBType.Blob);

			//entityType = styleEntityTypes[FillStyle.EntityTypeName];
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);
			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + FillStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + FillStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);
			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);
			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);
			//#endregion

			//#region *** CHARACTERSTYLE TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "CharacterStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("FontName", TurboDBType.String);
			//sqlGenerator.AddField("Size", TurboDBType.Float);
			//sqlGenerator.AddField("Style", TurboDBType.Byte);
			//sqlGenerator.AddField("ColorStyle", TurboDBType.String);

			//entityType = styleEntityTypes[CharacterStyle.EntityTypeName];
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);
			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + CharacterStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + CharacterStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);
			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);
			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);
			//#endregion

			//#region *** LINESTYLE TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "LineStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("LineWidth", TurboDBType.Integer);
			//sqlGenerator.AddField("DashStyle", TurboDBType.Byte);
			//sqlGenerator.AddField("DashCap", TurboDBType.Byte);
			//sqlGenerator.AddField("LineJoin", TurboDBType.Byte);
			//sqlGenerator.AddField("ColorStyle", TurboDBType.String);

			//entityType = styleEntityTypes[LineStyle.EntityTypeName];
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);
			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + LineStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + LineStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);
			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);
			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);
			//#endregion

			//#region *** SHAPESTYLE TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "ShapeStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("RoundedCorners", TurboDBType.Boolean);
			//sqlGenerator.AddField("ShowGradients", TurboDBType.Boolean);
			//sqlGenerator.AddField("ShowShadows", TurboDBType.Boolean);
			//sqlGenerator.AddField("ShadowColorStyle", TurboDBType.String);

			//entityType = styleEntityTypes[ShapeStyle.EntityTypeName];
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);
			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + ShapeStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + ShapeStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);
			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);
			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);
			//#endregion

			//#region *** PARAGRAPHSTYLE TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "ParagraphStyle";
			//sqlGenerator.AddField("Id", TurboDBType.Integer, ColumnProperties.NotNull | ColumnProperties.Unique | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Name", TurboDBType.String, ColumnProperties.Unique);
			//sqlGenerator.AddField("Caption", TurboDBType.String);
			//sqlGenerator.AddField("Alignment", TurboDBType.SmallInt);
			//sqlGenerator.AddField("Trimming", TurboDBType.Byte);
			//sqlGenerator.AddField("PaddingLeft", TurboDBType.Integer);
			//sqlGenerator.AddField("PaddingTop", TurboDBType.Integer);
			//sqlGenerator.AddField("PaddingRight", TurboDBType.Integer);
			//sqlGenerator.AddField("PaddingBottom", TurboDBType.Integer);
			//sqlGenerator.AddField("WordWrap", TurboDBType.Boolean);

			//entityType = styleEntityTypes[ParagraphStyle.EntityTypeName];
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateStyleTableCommand(entityType, commandSet.CreateTableCommand);
			//SetSelectStyleIdsCommand(entityType, commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectStyleByIdCommand(entityType, commandSet.SelectByIdCommand);
			//commandSet.InsertCommand.CommandText = "INSERT INTO [Style] ([DesignId], [TStyle]) VALUES (:DesignId, '" + ParagraphStyleTypeName + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + ParagraphStyleTypeName + "')", "CurrentRecordId('Style')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//commandSet.InsertCommand.Parameters.Insert(0, new TurboDBParameter("DesignId", TurboDBType.Integer));
			//SetInsertStyleCommand(entityType, commandSet.InsertCommand);
			//SetUpdateStyleCommand(entityType, commandSet.UpdateCommand);
			//commandSet.DeleteCommand.CommandText += "DELETE FROM Styles WHERE Id = :Id;";
			//SetDeleteStyleCommand(entityType, commandSet.DeleteCommand);
			//#endregion

			//#region *** PROJECT TABLE ***
			//GenerateEntityCommandSet(projectSettingsEntityType);
			//#endregion

			//#region *** DIAGRAM TABLE ***
			//GenerateEntityCommandSet(diagramEntityType);
			//#endregion

			//#region *** TEMPLATE TABLE ***
			//GenerateEntityCommandSet(templateEntityType);
			//#endregion
			
			//#region *** SHAPE BASE TABLE ***
			////There is a table that stores the shape id and shape type and a separate table for each shape type. 
			////The tables for the specific shape types are all built according to the following schema.
			//// *** BASE SHAPE TABLE ***
			//// Id				AutoInc
			//// Type	string
			//// *** TYPED SHAPE TABLES ***
			////DiagramId	Integer	
			////TemplateId Integer
			////Id	Integer	
			////X	Integer	
			////Y	Integer	
			////<other shape properties>
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "Shape";
			//sqlGenerator.AddField("Id", TurboDBType.AutoInc, ColumnProperties.NotNull | ColumnProperties.PrimaryKey);
			//sqlGenerator.AddField("Type", TurboDBType.String, ColumnProperties.NotNull);
			//commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateShapeTableCommand(commandSet.CreateTableCommand);
			//SetSelectShapeIdsCommand(commandSet.SelectIdsCommand);
			//SetSelectShapeTypeByIdCommand(commandSet.SelectByIdCommand);
			//#endregion
			
			//#region *** SHAPE CONNECTIONS TABLE ***
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = "ShapeConnections";
			//sqlGenerator.AddField("DiagramId", TurboDBType.Integer, ColumnProperties.NotNull);
			//sqlGenerator.AddField("ConnectorShapeId", TurboDBType.Integer, ColumnProperties.NotNull);
			//sqlGenerator.AddField("GluePointId", TurboDBType.Integer);
			//sqlGenerator.AddField("TargetShapeId", TurboDBType.Integer);
			//sqlGenerator.AddField("ConnectionPointId", TurboDBType.Integer);
			//commandSet = CreateCommandSet(sqlGenerator);

			//SetCreateShapeConnectionTableCommand(commandSet.CreateTableCommand);
			//commandSet.SelectIdsCommand.CommandText = string.Format("SELECT * FROM [{0}]", sqlGenerator.TableName);
			//SetSelectShapeConnectionIdsCommand(commandSet.SelectIdsCommand);
			//commandSet.SelectByIdCommand.CommandText = "SELECT * FROM [ShapeConnections] WHERE [DiagramId] = :DiagramId";
			//commandSet.SelectByIdCommand.Parameters.Clear();
			//commandSet.SelectByIdCommand.Parameters.Add(new TurboDBParameter("DiagramId", TurboDBType.Integer));
			//SetSelectShapeConnectionByIdCommand(commandSet.SelectByIdCommand);
			//SetInsertShapeConnectionCommand(commandSet.InsertCommand);
			//commandSet.DeleteCommand.CommandText = "DELETE FROM [ShapeConnections] WHERE [DiagramId] = :DiagramId AND [ConnectorShapeId] = :ConnectorShapeId AND [GluePointId] = :GluePointId AND [TargetShapeId] = :TargetShapeId AND [ConnectionPointId] = :ConnectionPointId;";
			//commandSet.DeleteCommand.Parameters.Add(new TurboDBParameter("DiagramId", TurboDBType.Integer));
			//commandSet.DeleteCommand.Parameters.Add(new TurboDBParameter("ConnectorShapeId", TurboDBType.Integer));
			//commandSet.DeleteCommand.Parameters.Add(new TurboDBParameter("GluepointId", TurboDBType.Integer));
			//commandSet.DeleteCommand.Parameters.Add(new TurboDBParameter("TargetShapeId", TurboDBType.Integer));
			//commandSet.DeleteCommand.Parameters.Add(new TurboDBParameter("ConnectionPointId", TurboDBType.Integer));
			//SetDeleteShapeConnectionCommand(commandSet.DeleteCommand);
			//#endregion
		}


		public void GenerateShapeEntityCommandSets(IEnumerable<IEntityType> entityTypes, IEnumerable<IEntityType> modelObjectTypes) {
			SqlGenerator sqlGenerator = new SqlGenerator();
			sqlGenerator.OpeningFieldQualifier = "[";
			sqlGenerator.ClosingFieldQualifier = "]";

			foreach (IEntityType entityType in entityTypes)
				GenerateShapeEntityCommandSet(entityType);

			// ToDo: implement generating commands for ModelObjects
		}


		private void GenerateShapeEntityCommandSet(IEntityType shapeEntityType) {
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = shapeEntityType.Name;

			//sqlGenerator.AddField("DiagramId", TurboDBType.Integer);
			//for (int i = 0; i < shapeEntityType.PropertyCount; ++i) {
			//   EntityPropertyDefinition pi = shapeEntityType.GetPropertyInfo(i);
			//   if (pi is EntityFieldDefinition) {
			//      if (pi.Name.Equals("Id"))
			//         // Create Id field as PrimaryKey
			//         sqlGenerator.AddField(pi.Name, ((EntityFieldDefinition)pi).Type, ColumnProperties.PrimaryKey | ColumnProperties.Unique);
			//      else
			//         // Create normal field
			//         sqlGenerator.AddField(pi.Name, ((EntityFieldDefinition)pi).Type);
			//   }
			//   else if (pi is InnerObjectFieldInfo) {
			//      GenerateInnerObjectsCommands((InnerObjectFieldInfo)pi);
			//   }
			//   else throw new DiagrammingException("Unexpected PropertyInfo type '{0}'", pi.GetType().Name);
			//}

			//CommandSet commandSet = CreateCommandSet(sqlGenerator);
			//SetCreateShapeTableCommand(shapeEntityType, commandSet.CreateTableCommand);

			//SetSelectShapeIdsCommand(shapeEntityType, commandSet.SelectIdsCommand);

			//TurboDBCommand cmd = (TurboDBCommand)CreateCommand();
			//cmd.CommandText = string.Format("SELECT * FROM {0}{1}{2} WHERE DiagramId = :DiagramId;", sqlGenerator.OpeningFieldQualifier, shapeEntityType.Name, sqlGenerator.ClosingFieldQualifier);
			//cmd.Parameters.Add(new TurboDBParameter("DiagramId", TurboDBType.Integer));
			//SetSelectDiagramShapesCommand(shapeEntityType, cmd);

			//commandSet.SelectByIdCommand.CommandText = commandSet.SelectByIdCommand.CommandText.ReplaceRange(string.Format("{0}DiagramId{1}, ", sqlGenerator.OpeningFieldQualifier, sqlGenerator.ClosingFieldQualifier), "");
			//SetSelectShapeByIdCommand(shapeEntityType, commandSet.SelectByIdCommand);

			//commandSet.InsertCommand.CommandText = "INSERT INTO [Shape] ([Type]) VALUES ('" + shapeEntityType.Name + "'); " + commandSet.InsertCommand.CommandText.ReplaceRange(":Id", "CurrentRecordId('Shape')");
			//commandSet.InsertCommand.CommandText = commandSet.InsertCommand.CommandText.ReplaceRange("CurrentRecordId('" + shapeEntityType.Name + "')", "CurrentRecordId('Shape')");
			//commandSet.InsertCommand.Parameters.RemoveAt(commandSet.InsertCommand.Parameters.DiagramControllerIndexOf("Id"));
			//SetInsertShapeCommand(shapeEntityType, commandSet.InsertCommand);

			//SetUpdateShapeCommand(shapeEntityType, commandSet.UpdateCommand);

			//commandSet.DeleteCommand.CommandText += "DELETE FROM Shape WHERE Id = :Id;";
			//SetDeleteShapeCommand(shapeEntityType, commandSet.DeleteCommand);
		}


		private void GenerateEntityCommandSet(IEntityType entityType) {
			//sqlGenerator.Clear();
			//sqlGenerator.TableName = entityType.Name;

			//for (int i = 0; i < entityType.PropertyCount; ++i) {
			//   EntityPropertyDefinition pi = entityType.GetPropertyInfo(i);
			//   if (pi is EntityFieldDefinition) {
			//      if (pi.Name.Equals("Id"))
			//         // Create Id field as PrimaryKey
			//         sqlGenerator.AddField(pi.Name, TurboDBType.AutoInc, ColumnProperties.PrimaryKey);
			//      else
			//         // Create normal field
			//         sqlGenerator.AddField(pi.Name, ((EntityFieldDefinition)pi).Type);
			//   }
			//   else if (pi is InnerObjectFieldInfo) {
			//      GenerateInnerObjectsCommands((InnerObjectFieldInfo)pi);
			//   }
			//   else throw new DiagrammingException("Unexpected PropertyInfo type '{0}'", pi.GetType().Name);
			//}

			//CommandSet commandSet = CreateCommandSet(sqlGenerator);
			//if (entityType == projectSettingsEntityType) {
			//   SetCreateProjectTableCommand(commandSet.CreateTableCommand);
			//   commandSet.SelectByIdCommand.CommandText = string.Format("SELECT * FROM {0}{1}{2};", sqlGenerator.OpeningFieldQualifier, entityType.Name, sqlGenerator.ClosingFieldQualifier);
			//   commandSet.SelectByIdCommand.Parameters.Clear();
			//   SetSelectProjectCommand(commandSet.SelectByIdCommand);
			//   SetInsertProjectSettingsCommand(commandSet.InsertCommand);
			//   SetUpdateProjectSettingsCommand(commandSet.UpdateCommand);
			//   SetDeleteProjectSettingsCommand(commandSet.DeleteCommand);
			//}
			//else if (entityType == diagramEntityType) {
			//   SetCreateDiagramTableCommand(commandSet.CreateTableCommand);
			//   SetSelectDiagramIdsCommand(commandSet.SelectIdsCommand);
			//   SetSelectDiagramByIdCommand(commandSet.SelectByIdCommand);
			//   SetInsertDiagramCommand(commandSet.InsertCommand);
			//   SetUpdateDiagramCommand(commandSet.UpdateCommand);
			//   SetDeleteDiagramCommand(commandSet.DeleteCommand);
			//}
			//else if (entityType == templateEntityType) {
			//   SetCreateTemplateTableCommand(commandSet.CreateTableCommand);
			//   SetSelectTemplateIdsCommand(commandSet.SelectIdsCommand);
			//   SetSelectTemplateByIdCommand(commandSet.SelectByIdCommand);
			//   SetInsertTemplateCommand(commandSet.InsertCommand);
			//   SetUpdateTemplateCommand(commandSet.UpdateCommand);
			//   SetDeleteTemplateCommand(commandSet.DeleteCommand);
			//}
			//else throw new DiagrammingException("Unexpected EntityType '{0}'.", entityType.Name);
		}


		private void GenerateInnerObjectsCommands(EntityFieldDefinition propertyInfo) {
			//SqlGenerator sqlGenerator = new SqlGenerator(propertyInfo.Name);
			//sqlGenerator.OpeningFieldQualifier = "[";
			//sqlGenerator.ClosingFieldQualifier = "]";
			//sqlGenerator.AddField("Owner", TurboDBType.Integer);
			//foreach (EntityPropertyDefinition pi in propertyInfo.FieldInfos) {
			//   if (pi is EntityFieldDefinition)
			//      sqlGenerator.AddField(pi.Name, ((EntityFieldDefinition)pi).Type);
			//   else if (pi is InnerObjectFieldInfo)
			//      GenerateInnerObjectsCommands((InnerObjectFieldInfo)pi);
			//   else throw new DiagrammingException("Unexpected PropertyInfo type '{0}'.", pi.GetType().Name);
			//}
			
			//CommandSet commandSet = CreateCommandSet(sqlGenerator);
			
			//// Set CREATE command
			//SetCreateInnerObjectTablesCommand(propertyInfo, commandSet.CreateTableCommand);

			//// Set SELECT command
			//commandSet.SelectByIdCommand.CommandText = string.Format("SELECT * FROM [{0}] WHERE [Owner] = :Owner;", propertyInfo.Name);
			//commandSet.SelectByIdCommand.Parameters.Clear();
			//commandSet.SelectByIdCommand.Parameters.Add(new TurboDBParameter("Owner", TurboDBType.Integer));
			//SetSelectInnerObjectsCommand(propertyInfo, commandSet.SelectByIdCommand);
			
			//// Set INSERT command
			//SetInsertInnerObjectsCommand(propertyInfo, commandSet.InsertCommand);

			//// Set DELETE command
			//commandSet.DeleteCommand.CommandText = commandSet.DeleteCommand.CommandText.ReplaceRange(";", "Owner = :Owner;");
			//commandSet.DeleteCommand.Parameters.Add(new TurboDBParameter("Owner", TurboDBType.Integer));
			//SetDeleteInnerObjectsCommand(propertyInfo, commandSet.DeleteCommand);
		}


		public void CreateTurboDBSchema() {
			//IDbCommand command = CreateCommand();
			//TurboDBConnection connection = new TurboDBConnection();
			//connection.ConnectionString = ConnectionString;
			//command.Connection = connection;

			//try {
			//   if (command.Connection is TurboDBConnection) {
			//      if (!File.Exists(((TurboDBConnection)command.Connection).Database))
			//         ((TurboDBConnection)command.Connection).CreateDatabase();
			//   }
			//   if (connection.State != ConnectionState.Open)
			//      connection.Open();

			//   #region *** DESIGN TABLE ***
			//   command = GetCreateDesignTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   CreateInnerObjectsSchema(designEntityType);
			//   #endregion

			//   #region *** STYLE TABLES ***
			//   command = GetCreateStyleTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[ColorStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[CapStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[CharacterStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[FillStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[LineStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[ParagraphStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }

			//   command = GetCreateStyleTableCommand(styleEntityTypes[ShapeStyle.EntityTypeName]);
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   #endregion

			//   #region *** PROJECT TABLE ***
			//   command = GetCreateProjectTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   CreateInnerObjectsSchema(projectSettingsEntityType);
			//   #endregion

			//   #region *** DIAGRAM TABLE ***
			//   command = GetCreateDiagramTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   CreateInnerObjectsSchema(diagramEntityType);
			//   #endregion

			//   #region *** TEMPLATE TABLE ***
			//   command = GetCreateTemplateTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   CreateInnerObjectsSchema(templateEntityType);
			//   #endregion

			//   #region *** SHAPE TABLE ***
			//   command = GetCreateShapeTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   #endregion

			//   #region *** SHAPECONNECTIONS TABLE
			//   command = GetCreateShapeConnectionTableCommand();
			//   if (command != null) {
			//      command.Connection = connection;
			//      command.ExecuteNonQuery();
			//   }
			//   #endregion

			//   #region *** BUSINESSOBJECT TABLES ***
			//   //command = GetCreateBusinessObjectTableCommand();
			//   //if (command != null) command.ExecuteNonQuery();
			//   #endregion
			//}
			//finally {
			//   command.Connection.Close();
			//}
		}


		public void CreateTurboDBSchema(IEnumerable<IEntityType> entityTypes, IEnumerable<IEntityType> modelObjectTypes) {
			//IDbCommand command = CreateCommand();
			//TurboDBConnection connection = new TurboDBConnection();
			//connection.ConnectionString = ConnectionString;
			//command.Connection = connection;

			//if (command.Connection is TurboDBConnection) {
			//   if (!File.Exists(((TurboDBConnection)command.Connection).Database)) {
			//      ((TurboDBConnection)command.Connection).CreateDatabase();
			//      CreateTurboDBSchema();
			//   }
			//}

			//try {
			//   if (connection.State != ConnectionState.Open)
			//      connection.Open();

			//   foreach (IEntityType entityType in entityTypes) {
			//      command.CommandText = string.Format("SELECT * FROM Sys_UserTables WHERE [Name] ='{0}'", entityType.Name);
			//      if (!command.ExecuteReader().Read()) {
			//         command = GetCreateShapeTableCommand(entityType);
			//         if (command != null) {
			//            command.Connection = connection;
			//            command.ExecuteNonQuery();
			//         }
			//         else throw new DiagrammingException(string.Format("Create table command for EntityType '{0}' not set.", entityType.Name));
			//      }
			//      CreateInnerObjectsSchema(entityType);
			//   }				

			//   //foreach (Type modelObjectType in modelObjectTypes) {
			//   //   command = GetCreateBusinessObjectTableCommand(modelObjectType.Name);
			//   //   if (command != null) command.ExecuteNonQuery();
			//   //   else throw new DiagrammingException(string.Format("Create table command for Type '{0}' not set.", modelObjectType.Name));
			//   //}
			//}
			//finally {
			//   command.Connection.Close();
			//}
		}


		private void CreateInnerObjectsSchema(IEntityType entityType) {
			//IDbCommand command = CreateCommand();
			//TurboDBConnection connection = new TurboDBConnection();
			//connection.ConnectionString = ConnectionString;
			//command.Connection = connection;
			//if (connection.State != ConnectionState.Open)
			//   connection.Open();

			//for (int i = 0; i < entityType.PropertyCount; ++i) {
			//   EntityPropertyDefinition pi = entityType.GetPropertyInfo(i);
			//   if (pi is InnerObjectFieldInfo) {
			//      command.CommandText = string.Format("SELECT * FROM Sys_UserTables WHERE [Name] ='{0}'", pi.Name);
			//      if (!command.ExecuteReader().Read()) {
			//         command = GetCreateInnerObjectsTableCommand((InnerObjectFieldInfo)pi);
			//         if (command != null) {
			//            command.Connection = connection;
			//            command.ExecuteNonQuery();
			//         }
			//         else throw new DiagrammingException(string.Format("Create table command for PropertyInfo '{0}' not set.", pi.Name));
			//      }
			//   }
			//}
		}
		
		
		#region Fields
		SqlGenerator sqlGenerator = new SqlGenerator();
		#endregion
	}


	internal enum ColumnProperties { 
		None = 0, 
		PrimaryKey = 1, 
		ForeignKey = 2, 
		NotNull = 4, 
		Unique = 8
	};


	internal class SqlGenerator {
		internal SqlGenerator() {
		}
		internal SqlGenerator(string tableName) {
			this.tableName = tableName;
		}

		private struct ColDef {
			internal string Name;
			internal TurboDBType Type;
			internal ColumnProperties Properties;
			internal string MasterTable;
			internal List<string> MasterFields;
			internal static ColDef Empty {
				get {
					ColDef cd = new ColDef();
					cd.Name = "";
					cd.Type = TurboDBType.String;
					cd.Properties = ColumnProperties.None;
					cd.MasterTable = "";
					cd.MasterFields = null;
					return cd;
				}
			}
		}


		internal void Clear() {
			for (int i = 0; i < colDefs.Count; ++i) {
				ColDef colDef = colDefs[i];
				if (colDef.MasterFields != null) {
					colDef.MasterFields.Clear();
					colDef.MasterFields = null;
				}
			}
			colDefs.Clear();
			tableName = "";
			sb.Remove(0, sb.Length);
		}


		internal string TableName {
			get { return tableName; }
			set { tableName = value; }
		}


		internal string OpeningFieldQualifier {
			get { return fieldQualifierOpening; }
			set { fieldQualifierOpening = value; }
		}


		internal string ClosingFieldQualifier {
			get { return fieldQualifierClosing; }
			set { fieldQualifierClosing = value; }
		}


		internal string ParamQualifier {
			get { return paramQualifier; }
			set { paramQualifier = value; }
		}


		internal void AddField(string name, TurboDBType turboDBType) {
			AddField(name, turboDBType, ColumnProperties.None);
		}


		internal void AddField(string name, TurboDBType turboDBType, ColumnProperties properties) {
			ColDef colDef = ColDef.Empty;
			colDef.Name = name;
			colDef.Type = turboDBType;
			colDef.Properties = properties;
			colDefs.Add(colDef);
		}


		internal void AddField(string name, Type type) {
			AddField(name, TypeToTurboDBType(type), ColumnProperties.None);
		}


		internal void AddField(string name, Type type, ColumnProperties properties) {
			AddField(name, TypeToTurboDBType(type), properties);
		}


		internal void AddMasterTable(string fieldName, string masterTableName, string masterFieldName) {			
			int fieldIndex = -1;
			for (int i = 0; i < colDefs.Count; ++i) {
				if (string.Equals(colDefs[i].Name, fieldName, StringComparison.InvariantCultureIgnoreCase)) {
					fieldIndex = i;
					break;
				}
			}

			if (fieldIndex < 0)
				throw new Exception("Field '{0}' not found in current field definitions.");
			else {
				if (string.IsNullOrEmpty(masterTableName))
					throw new Exception("Name of the master table may not be empty.");
				if (string.IsNullOrEmpty(masterFieldName))
					throw new Exception("Name of the master table's key field may not be empty.");
				if (!string.IsNullOrEmpty(colDefs[fieldIndex].MasterTable) 
					&& !string.Equals(colDefs[fieldIndex].MasterTable, masterTableName, StringComparison.InvariantCultureIgnoreCase))
					throw new Exception(string.Format("Field '{0}' is already a foreign key for table '{1}'.", colDefs[fieldIndex].Name, colDefs[fieldIndex].MasterTable));
				ColDef colDef = colDefs[fieldIndex];
				colDef.MasterTable = masterTableName;
				if (colDef.MasterFields == null)
					colDef.MasterFields = new List<string>();
				colDef.MasterFields.Add(masterFieldName);
				colDefs[fieldIndex] = colDef;
			}
		}


		internal void GetCreateTableCommand(IDbCommand command) {
			command.Parameters.Clear();

			sb.Remove(0, sb.Length);
			if (string.IsNullOrEmpty(tableName))
				throw new Exception("Tablename must not be empty.");
			// add table definition
			sb.AppendFormat("CREATE TABLE {0}{1}{2} (", fieldQualifierOpening, tableName, fieldQualifierClosing);
			// add field definitions
			for (int i = 0; i < colDefs.Count; ++i)
				sb.AppendFormat("{0}{1}{2}{3} {4}{5}", i > 0 ? ", " : "", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing, TurboDBTypeToSql(colDefs[i].Type), ((colDefs[i].Properties & ColumnProperties.NotNull) != 0) ? " NOT NULL" : "");			
			// add constraints
			AddConstraint(ColumnProperties.PrimaryKey);
			AddConstraint(ColumnProperties.Unique);
			AddReferencialIntegrityConstraints();
			sb.Append(");");

			command.CommandText = sb.ToString();
		}


		internal void GetDropTableCommand(IDbCommand command) {
			command.Parameters.Clear();
			command.CommandText = string.Format("DROP TABLE {0}{1}{2};", fieldQualifierOpening, tableName, fieldQualifierClosing);
		}


		internal void GetSelectIdsCommand(IDbCommand command) {
			command.Parameters.Clear();
			GetSelectIdsCommand(command, false);
		}


		internal void GetSelectIdsCommand(IDbCommand command, bool withParams) {
			command.Parameters.Clear();
			
			sb.Remove(0, sb.Length);
			sb.Append("SELECT ");
			bool keyColAdded = false;
			for (int i = 0; i < colDefs.Count; ++i) {
				if ((colDefs[i].Properties & ColumnProperties.PrimaryKey) != 0) {
					sb.AppendFormat("{0}{1}{2}{3}", keyColAdded ? ", " : "", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing);
					if (!keyColAdded) keyColAdded = true;
				}
			}
			sb.AppendFormat(" FROM {0}{1}{2}", fieldQualifierOpening, tableName, fieldQualifierClosing);

			if (withParams) {
				keyColAdded = false;
				for (int i = 0; i < colDefs.Count; ++i) {
					if ((colDefs[i].Properties & ColumnProperties.ForeignKey) != 0) {
						sb.AppendFormat("{0}{1}{2}{3} = {4}{2}", keyColAdded ? " AND " : " WHERE ", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing, paramQualifier);
						command.Parameters.Add(new TurboDBParameter(colDefs[i].Name, colDefs[i].Type));
						if (!keyColAdded) keyColAdded = true;
					}
				}
			}
			sb.Append(";");
			command.CommandText = sb.ToString();
		}


		internal void GetSelectByIdCommand(IDbCommand command) {
			command.Parameters.Clear();
			
			sb.Remove(0, sb.Length);
			sb.Append("SELECT ");
			for (int i = 0; i < colDefs.Count; ++i)
				sb.AppendFormat("{0}{1}{2}{3}", i > 0 ? ", " : "", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing);
			sb.AppendFormat(" FROM {0}{1}{2} WHERE ", fieldQualifierOpening, tableName, fieldQualifierClosing);
			bool keyAdded = false;
			for (int i = 0; i < colDefs.Count; ++i) {
				if ((colDefs[i].Properties & ColumnProperties.PrimaryKey) != 0 || (colDefs[i].Properties & ColumnProperties.ForeignKey) != 0) {
					sb.AppendFormat("{0}{1}{2}{3} = {4}{2}", keyAdded ? " AND " : "", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing, paramQualifier);
					command.Parameters.Add(new TurboDBParameter(colDefs[i].Name, colDefs[i].Type));
					keyAdded = true;
				}
			}
			sb.Append(";");
			command.CommandText = sb.ToString();
		}


		internal void GetInsertCommand(IDbCommand command) {
			command.Parameters.Clear();

			sb.Remove(0, sb.Length);
			sb.AppendFormat("INSERT INTO {0}{1}{2} (", fieldQualifierOpening, tableName, fieldQualifierClosing);
			bool isFirst = true;
			for (int i = 0; i < colDefs.Count; ++i) {
				if (colDefs[i].Type != TurboDBType.AutoInc) {
					sb.AppendFormat("{0}{1}{2}{3}", isFirst ? "" : ", ", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing);
					if (isFirst) isFirst = false;
				}
			}
			sb.Append(") VALUES (");
			isFirst = true;
			for (int i = 0; i < colDefs.Count; ++i) {
				if (colDefs[i].Type != TurboDBType.AutoInc) {
					sb.AppendFormat("{0}{1}{2}", isFirst ? "" : ", ", paramQualifier, colDefs[i].Name);
					command.Parameters.Add(new TurboDBParameter(colDefs[i].Name, colDefs[i].Type, !((colDefs[i].Properties & ColumnProperties.NotNull) != 0)));
					if (isFirst) isFirst = false;
				}
			}
			sb.AppendFormat("); SELECT CurrentRecordId('{0}');", tableName);
			command.CommandText = sb.ToString();
		}


		internal void GetUpdateCommand(IDbCommand command) {
			command.Parameters.Clear();

			sb.Remove(0, sb.Length);
			sb.AppendFormat("UPDATE {0}{1}{2} SET ", fieldQualifierOpening, tableName, fieldQualifierClosing);
			bool fieldAdded = false;
			for (int i = 0; i < colDefs.Count; ++i) {
				if (!((colDefs[i].Properties & ColumnProperties.PrimaryKey) != 0)) {
					sb.AppendFormat("{0}{1}{2}{3} = {4}{2}", fieldAdded ? ", " : "", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing, paramQualifier);
					command.Parameters.Add(new TurboDBParameter(colDefs[i].Name, colDefs[i].Type, !((colDefs[i].Properties & ColumnProperties.NotNull) != 0)));
					if (!fieldAdded) fieldAdded = true;
				}
			}
			bool keyColAdded = false;
			for (int i = 0; i < colDefs.Count; ++i) {
				if ((colDefs[i].Properties & ColumnProperties.PrimaryKey) != 0) {
					sb.AppendFormat("{0} {1}{2}{3} = {4}{2}", keyColAdded ? " AND" : " WHERE", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing, paramQualifier);
					if (i < command.Parameters.Count)
					    command.Parameters.Insert(i, new TurboDBParameter(colDefs[i].Name, colDefs[i].Type, false));
					else
						command.Parameters.Add(new TurboDBParameter(colDefs[i].Name, colDefs[i].Type, false));
					if (!keyColAdded) keyColAdded = true;
				}
			}
			sb.Append(";");
			command.CommandText = sb.ToString();
		}


		internal void GetDeleteCommand(IDbCommand command) {
			command.Parameters.Clear();

			sb.Remove(0, sb.Length);
			sb.AppendFormat("DELETE FROM {0}{1}{2} WHERE ", fieldQualifierOpening, tableName, fieldQualifierClosing);
			bool fieldAdded = false;
			for (int i = 0; i < colDefs.Count; ++i) {
				if ((colDefs[i].Properties & ColumnProperties.PrimaryKey) != 0) {
					sb.AppendFormat("{0}{1}{2}{3} = {4}{2}", fieldAdded ? " AND " : "", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing, paramQualifier);
					command.Parameters.Add(new TurboDBParameter(colDefs[i].Name, colDefs[i].Type, !((colDefs[i].Properties & ColumnProperties.NotNull) != 0)));
					fieldAdded = true;
				}
			}
			sb.Append(";");
			command.CommandText = sb.ToString();
		}


		private void AddConstraint(ColumnProperties property) {
			string constraintName = "";
			if (property == ColumnProperties.PrimaryKey)
				constraintName = "PRIMARY KEY";
			else if (property == ColumnProperties.Unique)
				constraintName = "UNIQUE";
			else
				throw new Exception(string.Format("Unsopported constraint type '{0}'.", property));

			bool addClosingBracket = false;
			int cnt = colDefs.Count;
			for (int i = 0; i < cnt; ++i) {
				if ((colDefs[i].Properties & property) != 0) {
					if (!addClosingBracket) {
						sb.AppendFormat(", {0}({1}{2}{3}", constraintName, fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing);
						addClosingBracket = true;
					}
					else
						sb.AppendFormat(", {0}{1}{2}", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing);
				}
			}
			if (addClosingBracket)
				sb.Append(")");
		}


		private void AddReferencialIntegrityConstraints() {
			Dictionary<string, string> masterFields = new Dictionary<string,string>();
			Dictionary<string, string> detailFields = new Dictionary<string,string>();
			for (int i = 0; i < colDefs.Count; ++i) {
				if ((colDefs[i].Properties & ColumnProperties.ForeignKey) != 0) {
					if (!masterFields.ContainsKey(colDefs[i].MasterTable)) {
						string fields = "";
						foreach (string field in colDefs[i].MasterFields)
							fields += string.Format("{0}{1}{2}{3}", fields.Length == 0 ? "" : ", ", fieldQualifierOpening, field, fieldQualifierClosing);
						masterFields.Add(colDefs[i].MasterTable, fields);
					}
					if (!detailFields.ContainsKey(colDefs[i].MasterTable))
						detailFields.Add(colDefs[i].MasterTable, string.Format("{0}{1}{2}", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing));
					else {
						detailFields[colDefs[i].MasterTable] += string.Format(", {0}{1}{2}", fieldQualifierOpening, colDefs[i].Name, fieldQualifierClosing);
					}
				}
			}

			if (masterFields.Keys.Count != detailFields.Keys.Count) 
				throw new Exception("Unexpected error.");
				
			foreach (string masterTable in detailFields.Keys) {
				sb.AppendFormat(", FOREIGN KEY ({0}) REFERENCES {1}{2}{3}({4}) ON UPDATE CASCADE ON DELETE CASCADE", detailFields[masterTable], fieldQualifierOpening, masterTable, fieldQualifierClosing, masterFields[masterTable]);
			}
		}


		private string TurboDBTypeToSql(TurboDBType turboDBType) {
			int size = 0;
			switch (turboDBType) {
				case TurboDBType.Float:
					size = 9; break;
				case TurboDBType.String:
				case TurboDBType.WideString:
					size = 255; break;
				default:
					size = 0; break;
			}
			return TurboDBTypeToSql(turboDBType, size);
		}


		private string TurboDBTypeToSql(TurboDBType turboDBType, int size) {
			string result = "";
			switch (turboDBType) {
				case TurboDBType.AutoInc: result = "AUTOINC"; break;
				case TurboDBType.Blob: result = "LONGVARBINARY"; break;
				case TurboDBType.Boolean: result = "BOOLEAN"; break;
				case TurboDBType.Byte: result = "BYTE"; break;
				case TurboDBType.Date: result = "DATE"; break;
				case TurboDBType.DateTime: result = "TIMESTAMP"; break;
				case TurboDBType.Enum: result = "ENUM"; break;
				case TurboDBType.Float: result = "DOUBLE"; break;
				case TurboDBType.Guid: result = "GUID"; break;
				case TurboDBType.Integer: result = "INTEGER"; break;
				case TurboDBType.LargeInt: result = "BIGINT"; break;
				case TurboDBType.Memo: result = "LONGVARCHAR"; break;
				case TurboDBType.SmallInt: result = "SMALLINT"; break;
				case TurboDBType.String: result = "VARCHAR"; break;
				case TurboDBType.Time: result = "TIME"; break;
				case TurboDBType.WideMemo: result = "LONGVARWCHAR"; break;
				case TurboDBType.WideString: result = "VARWCHAR"; break;
				default:
					//TurboDBType.Link;
					//TurboDBType.Relation;
					//TurboDBType.NotSupported;
					return "";
			}

			if (turboDBType == TurboDBType.String || turboDBType == TurboDBType.WideString)
				result += ("(" + size.ToString() + ")");
			else if (turboDBType == TurboDBType.Float) {
				if (result.IndexOf("PRECISION") < 0)
					result += " PRECISION";
				if (size < 10) result += (" (" + size.ToString() + ")");
				else result += (" (9)");
			}
			else if (turboDBType == TurboDBType.Enum) {
			}
			return result;
		}


		private TurboDBType DbTypeToTurboDBType(DbType dbType) {
			switch (dbType) {
				case DbType.AnsiString:
				case DbType.AnsiStringFixedLength:
					return TurboDBType.String;
				case DbType.Binary: 
				case DbType.Object:
					return TurboDBType.Blob;
				case DbType.Boolean:
					return TurboDBType.Boolean;
				case DbType.Byte:	
				case DbType.SByte:
					return TurboDBType.Byte;
				case DbType.Date:
					return TurboDBType.Date;
				case DbType.DateTime:
				case DbType.DateTime2: 
				case DbType.DateTimeOffset:
					return TurboDBType.DateTime;
				case DbType.Single:
				case DbType.Decimal:
				case DbType.Currency:
				case DbType.Double:
				case DbType.VarNumeric:
					return TurboDBType.Float;
				case DbType.Guid:
					return TurboDBType.Guid;
				case DbType.Int16:
				case DbType.UInt16:
					return TurboDBType.SmallInt;
				case DbType.Int32:
				case DbType.UInt32:
					return TurboDBType.Integer;
				case DbType.Int64:
				case DbType.UInt64:
					return TurboDBType.LargeInt;
				case DbType.String:
				case DbType.StringFixedLength:
					return TurboDBType.WideString;
				case DbType.Time:
					return TurboDBType.Time;
				case DbType.Xml:
					return TurboDBType.WideMemo;
				default:
					throw new Exception(string.Format("Unsupported DbType value '{0}'.", dbType));
			}
		}


		private TurboDBType TypeToTurboDBType(Type type) {
			TurboDBType result;
			if (type == typeof(DBNull)) result = TurboDBType.Unknown;
			else if (type == typeof(string)) result = TurboDBType.String;
			else if (type == typeof(bool)) result = TurboDBType.Boolean;
			else if (type == typeof(byte) || type == typeof(Enum)) result = TurboDBType.Byte;
			else if (type == typeof(short)) result = TurboDBType.SmallInt;
			else if (type == typeof(int) || type == typeof(Color) || type == typeof(object)) result = TurboDBType.Integer;
			else if (type == typeof(long)) result = TurboDBType.LargeInt;
			else if (type == typeof(byte[]) || type == typeof(Image)) result = TurboDBType.Blob;
			else if (type == typeof(DateTime)) result = TurboDBType.DateTime;
			else if (type == typeof(float) || type == typeof(double)) result = TurboDBType.Float;
			else if (type == typeof(Guid)) result = TurboDBType.Guid;
			else throw new Exception(String.Format("Cannot map type {0} to a TurboDB data type", type.Name));
			return result;
		}


		#region Fields
		private StringBuilder sb = new StringBuilder();
		private List<ColDef> colDefs = new List<ColDef>();
		private string tableName = "";
		private string fieldQualifierOpening = "\"";
		private string fieldQualifierClosing = "\"";
		private string paramQualifier = ":";
		#endregion
	}
}