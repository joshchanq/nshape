using System;
using System.Collections.Generic;
using System.Text;

using Dataweb.Diagramming.Advanced;


namespace Dataweb.Diagramming {

	[Flags()]
	public enum Permission {
		None = 0,
		/// <summary>
		/// CopyFrom another permission set
		/// </summary>
		ModifyPermissionSet = 1,
		/// <summary>
		/// Modify position, size, rotation or z-order of shapes
		/// </summary>
		Layout = 2,
		/// <summary>
		/// Modify the appearance of the shape (color, line thickness etc.) and assign 
		/// another design
		/// </summary>
		Present = 4,
		/// <summary>
		/// Modify data properties
		/// </summary>
		ModifyData = 8,
		/// <summary>
		/// Insert shape into diagram
		/// </summary>
		Insert = 16,
		/// <summary>
		/// RemoveRange shape from diagram
		/// </summary>
		Delete = 32,
		/// <summary>
		/// Connect or disconnect shapes
		/// </summary>
		Connect = 64,
		/// <summary>
		/// Edit, insert and delete templates
		/// </summary>
		Templates = 128,
		/// <summary>
		/// Edit, insert and delete designs
		/// </summary>
		Designs = 256,
		/// <summary>Everything</summary>
		All = 0xffff
	}


	public class QueryPermissionEventArgs : EventArgs {

		public QueryPermissionEventArgs(Permission permissionSet) {
			this.permissionSet = permissionSet;
		}

		public Permission PermissionSet { get { return permissionSet; } }

		private Permission permissionSet;
	}


	public delegate void QueryPermissionHandler(object sender, QueryPermissionEventArgs eventArgs);


	/// <summary>
	/// Controls the access to diagram operations.
	/// </summary>
	public interface ISecurityManager {

		/// <summary>
		/// Checks whether the given domain-independent permission is granted by for the current role.
		/// </summary>
		bool IsGranted(Permission permission);

		/// <summary>
		/// Checks  whether then given permission is granted for the domain and the current role.
		/// </summary>
		bool IsGranted(Permission permission, char domainName);

		/// <summary>
		/// Checks whether a given permission is granted for a given shape by the ObjectRef 
		/// user permissions.
		/// </summary>
		bool IsGranted(Permission permission, Shape shape);

		/// <summary>
		/// Checks whether a given permission is granted for all shapes of a list by 
		/// the ObjectRef user permissions.
		/// </summary>
		bool IsGranted(Permission permission, IEnumerable<Shape> shapes);
	}


	#region DefaultSecurity


	public enum StandardRole {
		Administrator,
		SuperUser,
		Designer,
		Operator,
		Guest,
		Custom
	}


	/// <summary>
	/// SecurityManager implementation based on a fixed set of user roles.
	/// </summary>
	public class DefaultSecurity : ISecurityManager {

		/// <summary>
		/// Creates a default security object with standard roles and domains.
		/// </summary>
		public DefaultSecurity() {
			AddRole(roleNameAdministrator, "May do anything.");
			AddRole(roleNameSuperUser, "May do almost anything.");
			AddRole(roleNameDesigner, "Creates and edits diagrams.");
			AddRole(roleNameOperator, "Works with prepared diagrams.");
			AddRole(roleNameGuest, "Views diagrams.");
			currentRole = roles[0];
			currentStdRole = StandardRole.Administrator;
			//
			AddDomain('A', "Everybody may do everything.");
			AddPermissions('A', roleNameAdministrator, Permission.All);
			AddPermissions('A', roleNameSuperUser, Permission.All);
			AddPermissions('A', roleNameDesigner, Permission.All);
			AddPermissions('A', roleNameOperator, Permission.All);
			AddPermissions('A', roleNameGuest, Permission.None);
			//
			AddDomain('B', "Even operators can move shapes.");
			AddPermissions('A', roleNameAdministrator, Permission.All);
			AddPermissions('A', roleNameSuperUser, Permission.All);
			AddPermissions('A', roleNameDesigner, Permission.Connect | Permission.Delete | Permission.Insert | Permission.Layout | Permission.ModifyData | Permission.Present);
			AddPermissions('A', roleNameOperator, Permission.Layout | Permission.ModifyData);
			AddPermissions('A', roleNameGuest, Permission.None);
		}


		/// <summary>
		/// Adds a domain to the security.
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="description"></param>
		public void AddDomain(char name, string description) {
			if (!IsValidDomainName(name)) throw new ArgumentException("This is not an allowed domain name.");
			if (description == null) throw new ArgumentNullException("description");
			if (domains[name - 'A'] != null) throw new ArgumentException("A domain with this name exists already.");
			domains[name - 'A'] = description;
		}


		/// <summary>
		/// Removes a domain from the security.
		/// </summary>
		/// <param name="projectName"></param>
		public void RemoveDomain(char name) {
			if (!IsValidDomainName(name)) throw new ArgumentException("This is not an allowed domain name.");
			if (domains[name - 'A'] == null) throw new ArgumentException("A domain with this name does not exist.");
			domains[name - 'A'] = null;
		}


		/// <summary>
		/// Adds a role to the security.
		/// </summary>
		/// <param name="projectName"></param>
		/// <param name="description"></param>
		public void AddRole(string name, string description) {
			if (name == null) throw new ArgumentNullException("name");
			roles.Add(new Role(name, description));
		}


		/// <summary>
		/// Adds a new security role by copying an existing one.
		/// </summary>
		/// <param name="projectName">Name of the new role</param>
		/// <param name="description">Description for the new role</param>
		/// <param name="sourceRole">Name of the role to copy</param>
		public void AddRole(string name, string description, string sourceRoleName) {
			if (name == null) throw new ArgumentNullException("name");
			roles.Add(GetRole(sourceRoleName, true).Clone());
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="role"></param>
		/// <param name="permissions"></param>
		public void AddPermissions(char domain, string role, Permission permissions) {
			if (role == null) throw new ArgumentNullException("role");
			GetRole(role, true).AddPermissions(domain, permissions);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="role"></param>
		/// <param name="permissions"></param>
		public void SetPermissions(char domain, string role, Permission permissions) {
			if (role == null) throw new ArgumentNullException("role");
			GetRole(role, true).SetPermissions(domain, permissions);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="role"></param>
		/// <param name="permissions"></param>
		public void RemovePermissions(char domain, string role, Permission permissions) {
			if (role == null) throw new ArgumentNullException("role");
			GetRole(role, true).RemovePermissions(domain, permissions);
		}


		/// <summary>
		/// Defines the role of the current user.
		/// </summary>
		public string CurrentRoleName {
			get { return currentRole.name; }
			set {
				switch (value) {
						case roleNameAdministrator:
							currentStdRole = StandardRole.Administrator;
							break;
						case roleNameSuperUser:
							currentStdRole = StandardRole.SuperUser;
							break;
						case roleNameDesigner:
							currentStdRole = StandardRole.Designer;
							break;
						case roleNameOperator:
							currentStdRole = StandardRole.Operator;
							break;
						case roleNameGuest:
							currentStdRole = StandardRole.Guest;
							break;
						default:
							currentStdRole = StandardRole.Custom;
							break;
				}
				currentRole = GetRole(value, true);
			}
		}



		/// <summary>
		/// Defines the role of the current user.
		/// </summary>
		public StandardRole CurrentRole {
			get { return currentStdRole; }
			set {
				switch (value) {
					case StandardRole.Administrator:
						currentRole = GetRole(roleNameAdministrator, true);
						break;
					case StandardRole.SuperUser:
						currentRole = GetRole(roleNameSuperUser, true);
						break;
					case StandardRole.Designer:
						currentRole = GetRole(roleNameDesigner, true);
						break;
					case StandardRole.Operator:
						currentRole = GetRole(roleNameOperator, true);
						break;
					case StandardRole.Guest:
						currentRole = GetRole(roleNameGuest, true);
						break;
					case StandardRole.Custom:
						throw new ArgumentException("Custom roles are set by setting the CurrentRoleName property.");
				}
				currentStdRole = value;
			}
		}


		#region ISecurityManager Members

		/// <override></override>
		public bool IsGranted(Permission permission) {
			return IsGranted(permission, 'A');
		}


		/// <override></override>
		public bool IsGranted(Permission permission, char domainName) {
			return currentRole.IsGranted(permission, domainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return IsGranted(permission, shape.SecurityDomainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			bool result = true;
			foreach (Shape s in shapes)
				if (!IsGranted(permission, s)) {
					result = false;
					break;
				}
			return result;
		}

		#endregion


		private class Role {

			public Role(string name, string description) {
				this.name = name;
				this.description = description;
			}


			public Role Clone() {
				Role result = new Role(name, description);
				permissions.CopyTo(result.permissions, 0);
				return result;
			}


			public void AddPermissions(char domain, Permission permissions) {
				this.permissions[domain - 'A'] |= permissions;
			}


			public void SetPermissions(char domain, Permission permissions) {
				this.permissions[domain - 'A'] = permissions;
			}


			public void RemovePermissions(char domain, Permission permissions) {
				this.permissions[domain - 'A'] &= ~permissions;
			}


			public bool IsGranted(Permission permissions, char domainName) {
				return (this.permissions[domainName - 'A'] & permissions) == permissions;
			}


			public string name;
			public string description;
			public Permission[] permissions = new Permission[26];
		}


		private Role GetRole(string name, bool throwOnNotFound) {
			Role result = null;
			foreach (Role r in roles) {
				if (r.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
					result = r;
					break;
				}
			}
			if (result == null && throwOnNotFound) throw new ArgumentException("A role with this name does not exist.");
			return result;
		}


		private bool IsValidDomainName(char name) {
			return name >= 'A' && name <= 'Z';
		}


		// Contains the descriptions for the domains. If description is null, the domain
		// is not allowed.
		private string[] domains = new string[26];

		// List of known roles.
		private List<Role> roles = new List<Role>(10);

		// Reference of current Role.
		private Role currentRole;
		private StandardRole currentStdRole;

		private const string roleNameAdministrator = "Administrator";
		private const string roleNameSuperUser = "Super User";
		private const string roleNameDesigner = "Designer";
		private const string roleNameOperator = "Operator";
		private const string roleNameGuest = "Guest";
	}

	#endregion

}
