using System;
using System.Collections.Generic;
using System.Text;
using Dataweb.NShape.Advanced;
using System.Diagnostics;


namespace Dataweb.NShape {

	public class ShapeDuplicator {

		static ShapeDuplicator() {
			modelObjectClones = new Dictionary<IModelObject, IModelObject>();
		}


		/// <summary>
		/// Creates a clone of both, shape and model object.
		/// </summary>
		public static Shape CloneShapeAndModelObject(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			modelObjectClones.Clear();
			return DoCloneShape(shape, true);
		}


		/// <summary>
		/// Creates for each shape a clone of both, the shape and it's model object.
		/// </summary>
		public static IEnumerable<Shape> CloneShapeAndModelObject(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			modelObjectClones.Clear();
			foreach (Shape s in shapes)
				yield return DoCloneShape(s, true);
		}


		/// <summary>
		/// Creates a clone of the given shape. The clone references the source' model object.
		/// </summary>
		public static Shape CloneShapeOnly(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			modelObjectClones.Clear();
			return DoCloneShape(shape, false);
		}


		/// <summary>
		/// Creates a clone of the given shapes. The clones reference their source' model object.
		/// </summary>
		public static IEnumerable<Shape> CloneShapesOnly(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			modelObjectClones.Clear();
			foreach (Shape s in shapes)
				yield return DoCloneShape(s, false);
		}


		/// <summary>
		/// Creates a clone of the given shape's model object.
		/// </summary>
		public static void CloneModelObjectOnly(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			modelObjectClones.Clear();
			DoCloneModelObject(shape);
		}


		/// <summary>
		/// Creates a clone of the given shape's model object.
		/// </summary>
		public static void CloneModelObjectsOnly(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			modelObjectClones.Clear();
			foreach (Shape shape in shapes)
				DoCloneModelObject(shape);
		}


		private static Shape DoCloneShape(Shape shape, bool cloneModelObject) {
			Shape result = shape.Clone();
			if (cloneModelObject) DoCloneModelObject(result);
			return result;
		}
		
		
		private static void DoCloneModelObject(Shape shape) {
			if (shape.Children.Count == 0 && shape.ModelObject != null) {
#if DEBUG
				IModelObject clone = shape.ModelObject.Clone(); 
				Debug.Assert(clone.Parent == shape.ModelObject.Parent);
				shape.ModelObject = clone;
#else
				shape.ModelObject = shape.ModelObject.Clone();
#endif
			} else {
				CreateModelObjectClones(shape);
				if (modelObjectClones.Count > 0)
					AssignModelObjectClones(shape);
			}
		}


		private static void CreateModelObjectClones(Shape shape) {
			Debug.Assert(shape != null);
			if (shape.ModelObject != null) {
				modelObjectClones.Add(shape.ModelObject, shape.ModelObject.Clone());
				Debug.Assert(modelObjectClones[shape.ModelObject].Parent == shape.ModelObject.Parent);
				Debug.Assert(modelObjectClones[shape.ModelObject].Id == null);
			}
			foreach (Shape childShape in shape.Children) {
				if (childShape.ModelObject != null && !modelObjectClones.ContainsKey(childShape.ModelObject))
					CreateModelObjectClones(childShape);
			}
		}


		private static void AssignModelObjectClones(Shape shape) {
			Debug.Assert(shape != null);
			IModelObject mo = null;
			if (shape.ModelObject != null && modelObjectClones.TryGetValue(shape.ModelObject, out mo)) {
				if (shape.ModelObject.Parent != null 
					&& modelObjectClones.ContainsKey(shape.ModelObject.Parent)) {
					Debug.Print("Changing parent of '{0}' from '{1}' to '{2}'", mo.Name, shape.ModelObject.Parent.Name, modelObjectClones[shape.ModelObject.Parent].Name);
					mo.Parent = modelObjectClones[shape.ModelObject.Parent];
				}
				Debug.Print("Replacing shape's model object {0} with {1}", shape.ModelObject.Name, mo.Name);
				shape.ModelObject = mo;
			}
			foreach (Shape childShape in shape.Children)
				AssignModelObjectClones(childShape);
		}


		private static Dictionary<IModelObject, IModelObject> modelObjectClones;
	}

}
