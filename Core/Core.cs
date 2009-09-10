using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Collections;


namespace Dataweb.nShape.Advanced {

	/// <summary>
	/// Simulates a string coming from a resource.
	/// </summary>
	/// <remarks>Later versions will hold a reference to a ResourceManager and read
	/// the string from there.</remarks>
	public class ResourceString {

		static public implicit operator ResourceString(string s) {
			return new ResourceString(s);
		}


		public ResourceString(string s) {
			value = s;
		}


		public string Value {
			get { return value; }
		}


		private string value;

	}


	/// <summary>
	/// Provides services to shapes
	/// </summary>
	public interface IDisplayService {

		/// <summary>
		/// Invalidate the given rectangle.
		/// </summary>
		/// <param name="x">Left side of the rectangle</param>
		/// <param name="y">Top side of the rectangle</param>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		void Invalidate(int x, int y, int width, int height);

		/// <summary>
		/// Invalidate the given rectangle.
		/// </summary>
		/// <param name="rectangle">Rectangle to invalidate.</param>
		void Invalidate(Rectangle rectangle);

		/// <summary>
		/// Update layout according to the changed bounds.
		/// </summary>
		void NotifyBoundsChanged();	// ToDo: Find a better name...

		/// <summary>
		/// Info graphics for mearusing text, etc. Do not dispose!
		/// </summary>
		Graphics InfoGraphics { get; }

		IFillStyle HintBackgroundStyle { get; }

		ILineStyle HintForegroundStyle { get; }
	}


	/// <summary>
	/// Represents a place, where shapes and model object typea are registered.
	/// </summary>
	/// <status>reviewed</status>
	public interface IRegistrar {

		/// <summary>
		/// Registers a library for shape or model objects.
		/// </summary>
		/// <param name="version">Version of the library</param>
		void RegisterLibrary(string name, int defaultRepositoryVersion);

		/// <summary>
		/// Registers a shape type implemented in the library.
		/// </summary>
		/// <param name="shapeType"></param>
		void RegisterShapeType(ShapeType shapeType);

		/// <summary>
		/// Registers a model object type implemented in the library.
		/// </summary>
		/// <param name="modelObjectType"></param>
		void RegisterModelObjectType(ModelObjectType modelObjectType);
	}


	/// <summary>
	/// Encapsulates the configuration on project level.
	/// </summary>
	public class ProjectSettings : IEntity {

		/// <summary>
		/// Constructs a projects projectData instance.
		/// </summary>
		public ProjectSettings() {
		}


		/// <summary>
		/// Empties the projectData.
		/// </summary>
		public void Clear() {
			this.id = null;
			this.lastSaved = DateTime.MinValue;
			this.libraries.Clear();
		}


		/// <summary>
		/// Copies all properties from the given projectData.
		/// </summary>
		/// <param name="source"></param>
		public void CopyFrom(ProjectSettings source) {
			if (source == null) throw new ArgumentNullException("source");
			id = ((IEntity)source).Id;
			lastSaved = source.LastSaved;
			for (int i = 0; i < source.libraries.Count; ++i) {
				if (!libraries.Contains(source.libraries[i]))
					libraries.Add(source.libraries[i]);
			}
		}


		/// <summary>
		/// Defines the date of the last saving of the project.
		/// </summary>
		public DateTime LastSaved {
			get { return lastSaved; }
			set { lastSaved = value; }
		}


		public void AddLibrary(string name, string assemblyName, int repositoryVersion) {
			if (name == null) throw new ArgumentNullException("name");
			if (assemblyName == null) throw new ArgumentNullException("assemblyName");
			libraries.Add(new LibraryData(name, assemblyName, repositoryVersion));
		}


		/// <summary>
		/// Retrieves the cache version of the given library.
		/// </summary>
		/// <param name="libraryName"></param>
		/// <returns></returns>
		public int GetRepositoryVersion(string libraryName) {
			if (libraryName == null) throw new ArgumentNullException("libraryName");
			LibraryData ld = FindLibraryData(libraryName, true);
			return ld.RepositoryVersion;
		}


		/// <summary>
		/// Indicates the library assemblies required for the project.
		/// </summary>
		public IEnumerable<string> AssemblyNames {
			get {
				foreach (LibraryData ld in libraries)
					yield return ld.AssemblyName;
			}
		}


		#region IEntity Members

		public static string EntityTypeName {
			get { return entityTypeName; }
		}


		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("LastSavedUtc", typeof(DateTime));
			yield return new EntityInnerObjectsDefinition("Libraries", "Core.Library", librariesAttrNames, librariesAttrTypes);
		}


		object IEntity.Id {
			get { return id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this.id != null)
				throw new InvalidOperationException("Project settings have already an id.");
			this.id = id;
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteDate(DateTime.Now);
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			lastSaved = reader.ReadDate();
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			Project.AssertSupportedVersion(true, version);
			writer.BeginWriteInnerObjects();
			foreach (LibraryData ld in libraries) {
				writer.BeginWriteInnerObject();
				writer.WriteString(ld.Name);
				writer.WriteString(ld.AssemblyName);
				writer.WriteInt32(ld.RepositoryVersion);
				writer.EndWriteInnerObject();
			}
			writer.EndWriteInnerObjects();
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			Project.AssertSupportedVersion(false, version);
			reader.BeginReadInnerObjects();
			while (reader.BeginReadInnerObject()) {
				LibraryData ld = new LibraryData(null, null, 0);
				ld.Name = reader.ReadString();
				ld.AssemblyName = reader.ReadString();
				ld.RepositoryVersion = reader.ReadInt32();
				libraries.Add(ld);
				reader.EndReadInnerObject();
			}
			reader.EndReadInnerObjects();
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition) {
					writer.DeleteInnerObjects();
				}
			}
		}

		#endregion


		private class LibraryData {

			public LibraryData(string name, string assemblyName, int repositoryVersion) {
				Name = name; 
				AssemblyName = assemblyName; 
				RepositoryVersion = repositoryVersion;
			}

			// Specifies the name of the library
			public string Name;
			// Specifies the full assembly name including version and public key token.
			public string AssemblyName;
			// Specifies this library's repository version as used in the project.
			public int RepositoryVersion;
		}


		private LibraryData FindLibraryData(string libraryName, bool throwIfNotFound) {
			LibraryData result = null;
			foreach (LibraryData ld in libraries)
				if (ld.Name.Equals(libraryName, StringComparison.InvariantCultureIgnoreCase)) {
					result = ld;
					break;
				}
			if (result == null && throwIfNotFound) throw new ArgumentException(string.Format("Library '{0}' not found.", libraryName));
			return result;
		}


		#region Fields

		private static string entityTypeName = "Core.Project";
		private static string[] librariesAttrNames = new string[] { "Name", "AssemblyName", "RepositoryVersion" };
		private static Type[] librariesAttrTypes = new Type[] { typeof(string), typeof(string), typeof(int) };

		private object id;
		private DateTime lastSaved;
		private List<LibraryData> libraries = new List<LibraryData>();

		#endregion
	}


	public struct EmptyEnumerator<T> : IEnumerable, IEnumerable<T>, IEnumerator<T>, IDisposable {

		public static EmptyEnumerator<T> Create() {
			EmptyEnumerator<T> result = EmptyEnumerator<T>.Empty;
			return result;
		}

		public static readonly EmptyEnumerator<T> Empty;

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator() {
			return this;
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return this;
		}

		#endregion

		#region IEnumerator<T> Members

		public T Current {
			get { return default(T); }
		}

		#endregion

		#region IEnumerator Members

		object IEnumerator.Current {
			get { return default(T); }
		}

		public bool MoveNext() {
			return false; ;
		}

		public void Reset() {
			// nothing to do
		}

		#endregion


		#region IDisposable Members

		public void Dispose() {
			// nothing to do
		}

		#endregion

	}

}
