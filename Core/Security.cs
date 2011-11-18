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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	/// <summary>
	/// Specifies the permission.
	/// </summary>
	[Flags]
	public enum Permission {
		/// <summary>No permissions are granted.</summary>
		None = 0x0000,
		/// <summary>Assign a security domain to any shape. This permission is security domain independent.</summary>
		ModifyPermissionSet = 0x0001,
		/// <summary>Modify position, size, rotation or z-order of shapes.</summary>
		Layout = 0x0002,
		/// <summary>Modify the appearance of the shape (color, line thickness etc.) and assign another design.</summary>
		Present = 0x0004,
		/// <summary>Modify data properties.</summary>
		ModifyData = 0x0008,
		/// <summary>Insert shape into diagram.</summary>
		Insert = 0x0010,
		/// <summary>Remove shape from diagram.</summary>
		Delete = 0x0020,
		/// <summary>Connect or disconnect shapes.</summary>
		Connect = 0x0040,
		/// <summary>Edit, insert and delete templates.</summary>
		Templates = 0x0080,
		/// <summary>Edit, insert and delete designs.</summary>
		Designs = 0x0100,
		/// <summary>All available permissions are granted.</summary>
		All = 0xffff
	}


	/// <summary>
	/// Specifies the set of <see cref="T:Dataweb.NShape.Permission" /> required for changing a property.
	/// </summary>
	[AttributeUsage((AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field), AllowMultiple = true, Inherited = true)]
	public class RequiredPermissionAttribute : Attribute {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.RequiredPermissionAttribute" />.
		/// </summary>
		public RequiredPermissionAttribute(Permission requiredPermission) {
			permission = requiredPermission;
		}

		/// <summary>
		/// Specifies the set of <see cref="T:Dataweb.NShape.Permission" /> required required.
		/// </summary>
		public Permission Permission {
			get { return permission; }
		}

		private Permission permission;
	}


	//public class QueryPermissionEventArgs : EventArgs {

	//   public QueryPermissionEventArgs(Permission permissionSet) {
	//      this.permissionSet = permissionSet;
	//   }

	//   public Permission PermissionSet { get { return permissionSet; } }

	//   private Permission permissionSet;
	//}


	//public delegate void QueryPermissionHandler(object sender, QueryPermissionEventArgs eventArgs);


	/// <summary>
	/// Controls the access to diagram operations.
	/// </summary>
	public interface ISecurityManager {
	
		/// <summary>
		/// Checks whether the given domain-independent  is granted by for the current role.
		/// </summary>
		bool IsGranted(Permission permission);

		/// <summary>
		/// Checks  whether then given permission is granted for the domain and the current role.
		/// </summary>
		bool IsGranted(Permission permission, char domainName);

		/// <summary>
		/// Checks whether a given permission is granted for a given shape by the current
		/// user permissions.
		/// </summary>
		bool IsGranted(Permission permission, Shape shape);

		/// <summary>
		/// Checks whether a given permission is granted for all shapes of a list by 
		/// the current user permissions.
		/// </summary>
		bool IsGranted(Permission permission, IEnumerable<Shape> shapes);

	}


	/// <summary>
	/// Defines a standard user role
	/// </summary>
	public enum StandardRole {
		/// <summary>All permissions are granted.</summary>
		Administrator,
		/// <summary>Most permissions are granted.</summary>
		SuperUser,
		/// <summary>Permissions required for designing diagrams are granted.</summary>
		Designer,
		/// <summary>Permissions needed for changing the state of objects are granted.</summary>
		Operator,
		/// <summary>Nearly no permissions are granted.</summary>
		Guest,
		/// <summary>Custom permissons are granted.</summary>
		Custom
	}
	
	
	/// <summary>
	/// SecurityManager implementation based on a fixed set of user roles.
	/// </summary>
	public class RoleBasedSecurityManager : ISecurityManager {

		/// <summary>
		/// Creates a default security object with standard roles and domains.
		/// </summary>
		public RoleBasedSecurityManager() {
			AddRole(roleNameAdministrator, "May do anything.");
			AddRole(roleNameSuperUser, "May do almost anything.");
			AddRole(roleNameDesigner, "Creates and edits diagrams.");
			AddRole(roleNameOperator, "Works with prepared diagrams.");
			AddRole(roleNameGuest, "Views diagrams.");
			currentSecManRole = roles[0];
			currentRole = StandardRole.Administrator;
			//
			char domain;
			domain = 'A';
			AddDomain(domain, "SuperUser creates diagrams, Designer only modifies designs and styles, Operator modifies layout and data.");
			AddPermissions(domain, roleNameAdministrator, Permission.All);
			AddPermissions(domain, roleNameSuperUser,
				Permission.Connect
				| Permission.Delete
				| Permission.Designs
				| Permission.Insert
				| Permission.Layout
				| Permission.ModifyData
				| Permission.Present
				| Permission.Templates);
			AddPermissions(domain, roleNameDesigner,
				Permission.Designs
				| Permission.Layout
				| Permission.ModifyData
				| Permission.Present);
			AddPermissions(domain, roleNameOperator, 
				Permission.Layout 
				| Permission.ModifyData);
			AddPermissions(domain, roleNameGuest, Permission.None);


			domain = 'B';
			AddDomain(domain, "SuperUser and Designer create diagrams, Operator may modify layout and data data.");
			AddPermissions(domain, roleNameAdministrator, Permission.All);
			AddPermissions(domain, roleNameSuperUser, 
				Permission.Connect 
				| Permission.Delete 
				| Permission.Designs 
				| Permission.Insert 
				| Permission.Layout 
				| Permission.ModifyData 
				| Permission.Present 
				| Permission.Templates);
			AddPermissions(domain, roleNameDesigner,
				Permission.Connect
				| Permission.Delete
				| Permission.Designs
				| Permission.Insert
				| Permission.Layout
				| Permission.ModifyData
				| Permission.Present);
			AddPermissions(domain, roleNameOperator,
				Permission.Layout
				| Permission.ModifyData);
			AddPermissions(domain, roleNameGuest, Permission.None);
			//
		}


		/// <summary>
		/// Defines the role of the current user.
		/// </summary>
		public string CurrentRoleName {
			get { return currentSecManRole.name; }
			set {
				switch (value) {
						case roleNameAdministrator:
							currentRole = StandardRole.Administrator;
							break;
						case roleNameSuperUser:
							currentRole = StandardRole.SuperUser;
							break;
						case roleNameDesigner:
							currentRole = StandardRole.Designer;
							break;
						case roleNameOperator:
							currentRole = StandardRole.Operator;
							break;
						case roleNameGuest:
							currentRole = StandardRole.Guest;
							break;
						default:
							currentRole = StandardRole.Custom;
							break;
				}
				currentSecManRole = GetRole(value, true);
			}
		}


		/// <summary>
		/// Defines the role of the current user.
		/// </summary>
		public StandardRole CurrentRole {
			get { return currentRole; }
			set {
				switch (value) {
					case StandardRole.Administrator:
						currentSecManRole = GetRole(roleNameAdministrator, true);
						break;
					case StandardRole.SuperUser:
						currentSecManRole = GetRole(roleNameSuperUser, true);
						break;
					case StandardRole.Designer:
						currentSecManRole = GetRole(roleNameDesigner, true);
						break;
					case StandardRole.Operator:
						currentSecManRole = GetRole(roleNameOperator, true);
						break;
					case StandardRole.Guest:
						currentSecManRole = GetRole(roleNameGuest, true);
						break;
					case StandardRole.Custom:
						throw new ArgumentException("Custom roles are set by setting the CurrentRoleName property.");
				}
				currentRole = value;
			}
		}


		/// <summary>
		/// Adds a domain to the security.
		/// </summary>
		public void AddDomain(char name, string description) {
			if (!IsValidDomainName(name)) throw new ArgumentException("This is not an allowed domain name.");
			if (description == null) throw new ArgumentNullException("description");
			if (domains[name - 'A'] != null) throw new ArgumentException("A domain with this name exists already.");
			domains[name - 'A'] = description;
		}


		/// <summary>
		/// Removes a domain from the security.
		/// </summary>
		public void RemoveDomain(char name) {
			if (!IsValidDomainName(name)) throw new ArgumentException("This is not an allowed domain name.");
			if (domains[name - 'A'] == null) throw new ArgumentException("A domain with this name does not exist.");
			domains[name - 'A'] = null;
		}


		/// <summary>
		/// Adds a role to the security.
		/// </summary>
		public void AddRole(string name, string description) {
			if (name == null) throw new ArgumentNullException("name");
			roles.Add(new SecurityManagerRole(name, description));
		}


		/// <summary>
		/// Adds a new security role by copying an existing one.
		/// </summary>
		public void AddRole(string name, string description, string sourceRoleName) {
			if (name == null) throw new ArgumentNullException("name");
			roles.Add(GetRole(sourceRoleName, true).Clone());
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		public void AddPermissions(char domain, StandardRole role, Permission permissions) {
			string roleName = GetRoleName(role);
			AddPermissions(domain, role, permissions);
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		public void AddPermissions(char domain, string roleName, Permission permissions) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).AddPermissions(domain, permissions);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		public void SetPermissions(char domain, StandardRole role, Permission permissions) {
			string roleName = GetRoleName(role);
			SetPermissions(domain, roleName, permissions);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		public void SetPermissions(char domain, string roleName, Permission permissions) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).SetPermissions(domain, permissions);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		public void RemovePermissions(char domain, StandardRole role, Permission permissions) {
			string roleName = GetRoleName(role);
			RemovePermissions(domain, roleName, permissions);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		public void RemovePermissions(char domain, string roleName, Permission permissions) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).RemovePermissions(domain, permissions);
		}


		/// <summary>
		/// Returns the name of the given role.
		/// </summary>
		public string GetRoleName(StandardRole role) {
			switch (role) {
				case StandardRole.Administrator: return roleNameAdministrator;
				case StandardRole.Designer: return roleNameDesigner;
				case StandardRole.Guest: return roleNameGuest;
				case StandardRole.Operator: return roleNameOperator;
				case StandardRole.SuperUser: return roleNameSuperUser;
				default: 
					throw new InvalidOperationException(string.Format("{0} is not a valid role for this operation.", role));
			}
		}


		#region ISecurityManager Members

		/// <override></override>
		public bool IsGranted(Permission permission) {
			return IsGranted(permission, 'A');
		}


		/// <override></override>
		public bool IsGranted(Permission permission, char domainName) {
			return currentSecManRole.IsGranted(permission, domainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return IsGranted(permission, shape.SecurityDomainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			bool grantedForAllShapes = true;
			bool shapeCollectionIsEmpty = true;
			foreach (Shape s in shapes) {
				if (shapeCollectionIsEmpty) shapeCollectionIsEmpty = false;
				if (!IsGranted(permission, s)) {
					grantedForAllShapes = false;
					break;
				}
			}
			return (!shapeCollectionIsEmpty && grantedForAllShapes);
		}

		#endregion


		private class SecurityManagerRole {

			public SecurityManagerRole(string name, string description) {
				this.name = name;
				this.description = description;
			}


			public SecurityManagerRole Clone() {
				SecurityManagerRole result = new SecurityManagerRole(name, description);
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


		private SecurityManagerRole GetRole(string name, bool throwOnNotFound) {
			SecurityManagerRole result = null;
			foreach (SecurityManagerRole r in roles) {
				if (r.name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
					result = r;
					break;
				}
			}
			if (result == null && throwOnNotFound) throw new ArgumentException(string.Format("The role '{0}' does not exist.", name));
			return result;
		}


		private bool IsValidDomainName(char name) {
			return name >= 'A' && name <= 'Z';
		}


		// Contains the descriptions for the domains. If description is null, the domain
		// is not allowed.
		private string[] domains = new string[26];

		// List of known roles.
		private List<SecurityManagerRole> roles = new List<SecurityManagerRole>(10);

		// Reference of current Role.
		private SecurityManagerRole currentSecManRole;
		private StandardRole currentRole;

		private const string roleNameAdministrator = "Administrator";
		private const string roleNameSuperUser = "Super User";
		private const string roleNameDesigner = "Designer";
		private const string roleNameOperator = "Operator";
		private const string roleNameGuest = "Guest";
	}


	/// <summary>
	/// This class was renamed to RoleBasedSecurityManager. 
	/// This wrapper maintains compatibility with previous versions.
	/// </summary>
	public class DefaultSecurity : RoleBasedSecurityManager {
	}

}
