using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming.GeneralShapes {

	public class Text: TextBase {

		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Text(shapeType, template);
		}


		public override Shape Clone() {
			Shape result = new Text(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Text(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}
	}


	public class Label : LabelBase {

		internal static Label CreateInstance(ShapeType shapeType, Template template) {
			return new Label(shapeType, template);
		}


		public override Shape Clone() {
			Shape result = new Label(Type, (Template)null);
			result.CopyFrom(this);
			return result;
		}


		protected internal Label(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}
	}

}
