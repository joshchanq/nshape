using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dataweb.Diagramming.Advanced;
using System.Diagnostics;


namespace Dataweb.Diagramming.Controllers {

	public class DesignEventArgs : EventArgs {

		public DesignEventArgs(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			this.design = design;
		}

		public Design Design {
			get { return design; }
			internal set { design = value; }
		}

		internal DesignEventArgs() { }

		private Design design;
	}


	public class StyleEventArgs : DesignEventArgs {

		public StyleEventArgs(Design design, IStyle style)
			: base(design) {
			if (style == null) throw new ArgumentNullException("style");
			this.style = style;
		}

		public IStyle Style {
			get { return style; }
			internal set { style = value; }
		}

		internal StyleEventArgs() { }

		private IStyle style;
	}


	public partial class DesignController : Component {

		public DesignController() { }


		public DesignController(Project project)
			: this() {
			// Set property in order to register event handlers
			if (project == null) throw new ArgumentNullException("project");
			this.Project = project;
		}


		#region [Public] Events

		public event EventHandler Initialized;

		public event EventHandler Uninitialized;

		/// <summary>
		/// Raised when a new design was created.
		/// </summary>
		public event EventHandler<DesignEventArgs> DesignCreated;

		/// <summary>
		/// Raised when a design has changed
		/// </summary>
		public event EventHandler<DesignEventArgs> DesignChanged;

		/// <summary>
		/// Raised when a design has been deleted.
		/// </summary>
		public event EventHandler<DesignEventArgs> DesignDeleted;

		/// <summary>
		/// Raised when a new style has been created
		/// </summary>
		public event EventHandler<StyleEventArgs> StyleCreated;

		/// <summary>
		/// Raised when a style has been modified
		/// </summary>
		public event EventHandler<StyleEventArgs> StyleChanged;

		/// <summary>
		/// Raisd when a style has been deleted.
		/// </summary>
		public event EventHandler<StyleEventArgs> StyleDeleted;

		#endregion


		#region [Public] Properties

		public Project Project {
			get { return project; }
			set {
				if (project != null) UnregisterProjectEventHandlers();
				project = value;
				if (project != null) RegisterProjectEventHandlers();
			}
		}


		public IEnumerable<Design> Designs {
			get {
				if (project == null) return EmptyEnumerator<Design>.Empty;
				else return project.Repository.GetDesigns();
			}
		}

		#endregion


		#region [Public] Methods

		public bool CanDelete(Design design) {
			return (design != null && design != project.Design);
		}


		public bool CanDelete(Design selectedDesign, Style style) {
			if (style == null) return false;
			// if the style is owned by an other design (should not happen)
			Design design = GetOwnerDesign(style);
			if (design != selectedDesign) return false;
			// check if style is a standard style
			if (design.IsStandardStyle(style))
				return false;
			// check if the deleted style is used by other styles 
			// only ColorStyles are used by other styles
			foreach (IStyle s in GetOwnerStyles(selectedDesign, style))
				return false;
			return true;
		}


		public IEnumerable<IStyle> GetOwnerStyles(Design design, IStyle style) {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			// check all Styles of the parent design if the style is used by (not yet deleted) styles
			if (style is ColorStyle) {
				foreach (IStyle s in design.Styles) {
					if (s is ICapStyle && ((ICapStyle)s).ColorStyle == style)
						yield return s;
					else if (s is ICharacterStyle && ((ICharacterStyle)s).ColorStyle == style)
						yield return s;
					else if (s is IFillStyle && (((IFillStyle)s).AdditionalColorStyle == style || ((IFillStyle)s).BaseColorStyle == style))
						yield return s;
					else if (s is ILineStyle && ((ILineStyle)s).ColorStyle == style)
						yield return s;
					else if (s is IShapeStyle && ((IShapeStyle)s).ShadowColor == style)
						yield return s;
				}
			}
		}


		public void CreateDesign() {
			int designCnt = 1;
			foreach (Design d in Designs)
				++designCnt;
			//
			Design design = new Design(string.Format("Design {0}", designCnt));
			ICommand cmd = new CreateDesignCommand(design);
			project.ExecuteCommand(cmd);
		}


		public void CreateStyle(Design design, StyleCategory category) {
			if (design == null) throw new ArgumentNullException("design");
			Style style;
			switch (category) {
				case StyleCategory.CapStyle: style = new CapStyle(); break;
				case StyleCategory.CharacterStyle: style = new CharacterStyle(); break;
				case StyleCategory.ColorStyle: style = new ColorStyle(); break;
				case StyleCategory.FillStyle: style = new FillStyle(); break;
				case StyleCategory.LineStyle: style = new LineStyle(); break;
				case StyleCategory.ParagraphStyle: style = new ParagraphStyle(); break;
				//case StyleCategory.ShapeStyle: style = new ShapeStyle(); break;
				default: throw new DiagrammingUnsupportedValueException(typeof(StyleCategory), category);
			}
			ICommand cmd = new CreateStyleCommand(design, style);
			project.ExecuteCommand(cmd);
		}


		public void ReplaceStyle(Design design, Style style, string propertyName, object oldValue, object newValue) {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			PropertyInfo propertyInfo = style.GetType().GetProperty(propertyName);
			if (propertyInfo == null) throw new DiagrammingException("Property {0} not found in Type {1}.", propertyName, style.GetType().Name);

			ICommand cmd = new StylePropertySetCommand(design, style, propertyInfo, oldValue, newValue);
			project.ExecuteCommand(cmd);

			if (StyleChanged != null) StyleChanged(this, GetStyleEventArgs(design, style));
		}


		public void DeleteDesign(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			ICommand cmd = new DeleteDesignCommand(design);
			project.ExecuteCommand(cmd);
		}


		public void DeleteStyle(Design design, Style style) {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			ICommand cmd = new DeleteStyleCommand(design, style);
			project.ExecuteCommand(cmd);
		}

		#endregion


		#region [Private] Methods

		private void Initialize() {
			RegisterRepositoryEventHandlers();
			if (Initialized != null) Initialized(this, new EventArgs());
		}


		private void Uninitialize() {
			UnregisterRepositoryEventHandlers();
			if (Uninitialized != null) Uninitialized(this, new EventArgs());
		}


		private DesignEventArgs GetDesignEventArgs(Design design) {
			designEventArgs.Design = design;
			return designEventArgs;
		}


		private StyleEventArgs GetStyleEventArgs(Design design, IStyle style) {
			styleEventArgs.Design = design;
			styleEventArgs.Style = style;
			return styleEventArgs;
		}


		private Design GetOwnerDesign(IStyle style) {
			if (project.Design.ContainsStyle(style))
				return project.Design;
			foreach (Design design in project.Repository.GetDesigns()) {
				if (design.ContainsStyle(style))
					return design;
			}
			return null;
		}

		#endregion


		#region [Private] Methods: (Un)Registering events

		private void RegisterProjectEventHandlers() {
			Debug.Assert(project != null);
			project.Opened += project_Opened;
			project.Closing += project_Closing;
			if (project.IsOpen) Initialize();
		}


		private void UnregisterProjectEventHandlers() {
			Uninitialize();
			if (project != null) {
				project.Opened -= project_Opened;
				project.Closing -= project_Closing;
			}
		}


		private void RegisterRepositoryEventHandlers() {
			Debug.Assert(project != null && project.Repository != null);
			if (project != null && project.Repository != null) {
				project.Repository.DesignInserted += Repository_DesignInserted;
				project.Repository.DesignUpdated += Repository_DesignUpdated;
				project.Repository.DesignDeleted += Repository_DesignDeleted;
				project.Repository.StyleInserted += Repository_StyleInserted;
				project.Repository.StyleUpdated += Repository_StyleUpdated;
				project.Repository.StyleDeleted += Repository_StyleDeleted;
			}
		}


		private void UnregisterRepositoryEventHandlers() {
			Debug.Assert(project != null && project.Repository != null);
			if (project != null && project.Repository != null) {
				project.Repository.DesignInserted -= Repository_DesignInserted;
				project.Repository.DesignUpdated -= Repository_DesignUpdated;
				project.Repository.DesignDeleted -= Repository_DesignDeleted;
				project.Repository.StyleInserted -= Repository_StyleInserted;
				project.Repository.StyleUpdated -= Repository_StyleUpdated;
				project.Repository.StyleDeleted -= Repository_StyleDeleted;
			}
		}

		#endregion


		#region [Private] Methods: Event handler implementations

		private void project_Opened(object sender, EventArgs e) {
			Initialize();
		}


		private void project_Closing(object sender, EventArgs e) {
			Uninitialize();
		}


		private void Repository_DesignInserted(object sender, RepositoryDesignEventArgs e) {
			if (DesignCreated != null) DesignCreated(this, GetDesignEventArgs(e.Design));
		}


		private void Repository_DesignUpdated(object sender, RepositoryDesignEventArgs e) {
			if (DesignChanged != null) DesignChanged(this, GetDesignEventArgs(e.Design));
		}


		private void Repository_DesignDeleted(object sender, RepositoryDesignEventArgs e) {
			if (DesignDeleted != null) DesignDeleted(this, GetDesignEventArgs(e.Design));
		}


		private void Repository_StyleInserted(object sender, RepositoryStyleEventArgs e) {
			if (StyleCreated != null) StyleCreated(this, GetStyleEventArgs(GetOwnerDesign(e.Style), e.Style));
		}


		private void Repository_StyleDeleted(object sender, RepositoryStyleEventArgs e) {
			if (StyleDeleted != null) StyleDeleted(this, GetStyleEventArgs(GetOwnerDesign(e.Style), e.Style));
		}


		private void Repository_StyleUpdated(object sender, RepositoryStyleEventArgs e) {
			if (StyleChanged != null) StyleChanged(this, GetStyleEventArgs(GetOwnerDesign(e.Style), e.Style));
		}

		#endregion


		#region Fields

		private Project project;
		private DesignEventArgs designEventArgs = new DesignEventArgs();
		private StyleEventArgs styleEventArgs = new StyleEventArgs();

		#endregion
	}
}
