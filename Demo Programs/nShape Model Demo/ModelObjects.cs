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
using System.Collections.Generic;
using Dataweb.NShape.Advanced;


namespace nShape_Model_Demo {

	#region Struct ModelObjectConnection

	public struct ModelObjectConnection {

		public static bool operator ==(ModelObjectConnection x, ModelObjectConnection y) {
			return (
				x.OwnerModelObject == y.OwnerModelObject
				&& x.TargetModelObject == y.TargetModelObject
				&& x.OwnerTerminalId == y.OwnerTerminalId
				&& x.TargetTerminalId == y.TargetTerminalId);
		}

		public static bool operator !=(ModelObjectConnection x, ModelObjectConnection y) { return !(x == y); }

		public ModelObjectConnection(IModelObject owner, TerminalId ownerTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			this.OwnerModelObject = owner;
			this.OwnerTerminalId = ownerTerminalId;
			this.TargetModelObject = targetModelObject;
			this.TargetTerminalId = targetTerminalId;
		}

		public override bool Equals(object obj) {
			return obj is ModelObjectConnection && this == (ModelObjectConnection)obj;
		}

		public override int GetHashCode() {
			int result = OwnerTerminalId.GetHashCode() ^ TargetTerminalId.GetHashCode();
			if (OwnerModelObject != null) result ^= OwnerModelObject.GetHashCode();
			if (TargetModelObject != null) result ^= TargetModelObject.GetHashCode();
			return result;
		}

		public static readonly ModelObjectConnection Empty;

		public IModelObject OwnerModelObject;
		
		public TerminalId OwnerTerminalId;
		
		public IModelObject TargetModelObject;
		
		public TerminalId TargetTerminalId;

		static ModelObjectConnection() {
			Empty.OwnerModelObject = null;
			Empty.OwnerTerminalId = TerminalId.Invalid;
			Empty.TargetModelObject = null;
			Empty.TargetTerminalId = TerminalId.Invalid;
		}
	}

	#endregion


	public abstract class SampleModelBase : ModelObjectBase {
		
		public override void Connect(TerminalId ownTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			if (ownTerminalId == TerminalId.Invalid) throw new ArgumentException("ownTerminalId");
			if (targetModelObject == null) throw new ArgumentNullException("targetModelObject");
			if (targetTerminalId == TerminalId.Invalid) throw new ArgumentException("targetTerminalId");
			// Create connections list if it does not exist yet
			if (connections == null) connections = new List<ModelObjectConnection>();
			// Create connection
			ModelObjectConnection connection = ModelObjectConnection.Empty;
			connection.OwnerModelObject = this;
			connection.OwnerTerminalId = ownTerminalId;
			connection.TargetModelObject = targetModelObject;
			connection.TargetTerminalId = targetTerminalId;
			// If the connection does not exist yet, add and connect target to the model
			if (IndexOf(connection) < 0) {
				connections.Add(connection);
				targetModelObject.Connect(targetTerminalId, this, ownTerminalId);
			}
		}


		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			// Create connection
			ModelObjectConnection connection = ModelObjectConnection.Empty;
			connection.OwnerModelObject = this;
			connection.OwnerTerminalId = ownTerminalId;
			connection.TargetModelObject = targetModelObject;
			connection.TargetTerminalId = targetTerminalId;
			// Remove connection
			int idx = IndexOf(connection);
			if (idx >= 0){
				connections.RemoveAt(idx);
				targetModelObject.Disconnect(targetTerminalId, this, ownTerminalId);
			}
			// Delete connections list if this was the last connection
			if (connections != null && connections.Count == 0) connections = null;
		}


		public abstract int TerminalCount { get; }


		public abstract IEnumerable<TerminalId> GetTerminalIds();
		
		
		protected SampleModelBase(SampleModelBase source)
			: base(source) {
		}


		protected SampleModelBase(ModelObjectType type)
			: base(type) {
		}


		protected int IndexOf(ModelObjectConnection connection) {
			if (connections != null) {
				for (int i = connections.Count - 1; i >= 0; --i) {
					if (connections[i] == connection)
						return i;
				}
			}
			return -1;
		}


		#region Fields
		protected List<ModelObjectConnection> connections = null;
		#endregion
	}


	public abstract class Conductor : SampleModelBase {

		protected static readonly TerminalId Input;
		protected static readonly TerminalId Output;


		public override void Connect(TerminalId ownTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			base.Connect(ownTerminalId, targetModelObject, targetTerminalId);
			if (targetModelObject is Conductor)
				((Conductor)targetModelObject).Conduct(this, ownTerminalId, Current);
		}


		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			base.Disconnect(ownTerminalId, targetModelObject, targetTerminalId);
			if (targetModelObject is Conductor)
				((Conductor)targetModelObject).Conduct(this, ownTerminalId, 0);
		}


		public override IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		public override int GetInteger(int propertyId) {
			if (propertyId == CurrencyPropertyId) return Current;
			else return base.GetInteger(propertyId);
		}


		public override float GetFloat(int propertyId) {
			return base.GetFloat(propertyId);
		}


		public override string GetString(int propertyId) {
			return base.GetString(propertyId);
		}


		[PropertyMappingId(CurrencyPropertyId)]
		public virtual int Current {
			get { return current; }
			set { 
				current = value;
				OnPropertyChanged(CurrencyPropertyId);
			}
		}


		protected Conductor(Conductor source)
			: base(source) {
			this.current = source.current;
		}


		protected Conductor(ModelObjectType type)
			: base(type) {
		}


		public virtual void Conduct(IModelObject source, TerminalId sourceTerminal, int sourceCurrent) {
			// Pass current to connected models
			if (source != this && !(this is Battery)) Current = sourceCurrent;
			if (connections != null) {
				TerminalId outputTerminal = GetOutputTerminal();
				for (int i = connections.Count - 1; i >= 0; --i) {
					if (connections[i].TargetModelObject == source) continue;
					if (!(connections[i].TargetModelObject is Conductor)) continue;
					Conductor conductor = (Conductor)connections[i].TargetModelObject;
					TerminalId inputTerminal = conductor.GetInputTerminal();

					if (connections[i].OwnerTerminalId == outputTerminal
						&& connections[i].TargetTerminalId == inputTerminal)
						conductor.Conduct(this, outputTerminal, Current);

					//if (connections[i].OwnerTerminalId != outputTerminal) continue;
					//if (connections[i].TargetTerminalId == Output) continue;
					//if (connections[i].TargetModelObject == source) continue;
					////Conduct currency to attached models
					//((Conductor)connections[i].TargetModelObject).Conduct(
					//   this, connections[i].TargetTerminalId, Current);
				}
			}
		}


		protected abstract TerminalId GetInputTerminal();


		protected abstract TerminalId GetOutputTerminal();


		protected bool IsConnectedToBattery(IModelObject source, TerminalId terminalId) {
			if (connections != null) {
				for (int i = connections.Count - 1; i >= 0; --i) {
					if (source == null && connections[i].OwnerTerminalId != terminalId) continue;
					if (connections[i].TargetModelObject is Battery)
						return true;
					else {
						if (connections[i].TargetModelObject == source) continue;
						if (connections[i].TargetTerminalId == Input) continue;
						if (!(connections[i].TargetModelObject is Conductor)) continue;
						if (((Conductor)connections[i].TargetModelObject).IsConnectedToBattery(this, connections[i].TargetTerminalId))
							return true;
					}
				}
			}
			return false;
		}


		static Conductor() {
			Input = 1;
			Output = 2;
		}


		#region Fields
		private int current = 0;
		public const int CurrencyPropertyId = 100;
		#endregion
	}


	public class Battery : Conductor {

		public static Battery CreateInstance(ModelObjectType modelObjectType) {
			return new Battery(modelObjectType);
		}


		public static readonly TerminalId OutputTerminal;
		
		
		public override IModelObject Clone() {
			return new Battery(this);
		}


		public override int TerminalCount {
			get { return 1; }
		}


		public override IEnumerable<TerminalId> GetTerminalIds() {
			yield return Output;
		}


		public override int Current {
			get { return base.Current; }
			set {
				base.Current = value;
				Conduct(this, Output, base.Current);
			}
		}


		protected override TerminalId GetInputTerminal() {
			return TerminalId.Invalid;
		}


		protected override TerminalId GetOutputTerminal() {
			return Output;
		}


		protected Battery(ModelObjectType modelObjectType)
			: base(modelObjectType) {
		}


		protected Battery(Battery source)
			: base(source) {
		}


		static Battery() {
			OutputTerminal = Conductor.Output;
		}
	}


	public class Wire : Conductor {
		
		public static Wire CreateInstance(ModelObjectType modelObjectType) {
			return new Wire(modelObjectType);
		}


		public static readonly TerminalId ConnectionTerminal1;
		public static readonly TerminalId ConnectionTerminal2;
		
		
		public override int TerminalCount {
			get { return 2; }
		}
		
		
		public override IEnumerable<TerminalId> GetTerminalIds() {
			yield return ConnectionTerminal1;
			yield return ConnectionTerminal2;
		}

		
		public override IModelObject Clone() {
			return new Wire(this);
		}


		public override void Connect(TerminalId ownTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			inputTerminal = outputTerminal = TerminalId.Invalid;
			base.Connect(ownTerminalId, targetModelObject, targetTerminalId);
		}


		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetModelObject, TerminalId targetTerminalId) {
			base.Disconnect(ownTerminalId, targetModelObject, targetTerminalId);
			inputTerminal = outputTerminal = TerminalId.Invalid;
		}


		protected override TerminalId GetInputTerminal() {
			if (inputTerminal == TerminalId.Invalid) {
				if (IsConnectedToBattery(null, ConnectionTerminal1)) 
					inputTerminal = ConnectionTerminal1;
				else if (IsConnectedToBattery(null, ConnectionTerminal2)) 
					inputTerminal = ConnectionTerminal2;
			}
			return inputTerminal;
		}


		protected override TerminalId GetOutputTerminal() {
			if (outputTerminal == TerminalId.Invalid) {
				if (GetInputTerminal() == ConnectionTerminal1)
					outputTerminal = ConnectionTerminal2;
				else if (GetInputTerminal() == ConnectionTerminal2)
					outputTerminal = ConnectionTerminal1;
			}
			return outputTerminal;
		}


		protected Wire(Wire source)
			: base(source) {
		}


		protected Wire(ModelObjectType modelObjectType)
			: base(modelObjectType) {
		}


		static Wire() {
			ConnectionTerminal1 = 3;
			ConnectionTerminal2 = 4;
		}


		private TerminalId inputTerminal = TerminalId.Invalid;
		private TerminalId outputTerminal = TerminalId.Invalid;
	}
	
	
	public class Switch : Wire {

		public static new Switch CreateInstance(ModelObjectType modelObjectType) {
			return new Switch(modelObjectType);
		}
		
		
		public override IModelObject Clone() {
			return new Switch(this);
		}


		public override IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		public override int GetInteger(int propertyId) {
			if (propertyId == StatePropertyId) return Convert.ToInt32(State);
			else return base.GetInteger(propertyId);
		}


		[PropertyMappingId(StatePropertyId)]
		public bool State {
			get { return state; }
			set {
				if (value != state) {
					state = value;
					Conduct(this, GetOutputTerminal(), Current);
					OnPropertyChanged(StatePropertyId);
				}
			}
		}


		public override int Current {
			get { return state ? base.Current : 0; }
			set { base.Current = value; }
		}


		protected Switch(Switch source)
			: base(source) {
			this.state = source.state;
		}


		protected Switch(ModelObjectType type)
			: base(type) {
		}


		#region Fields
		private bool state = false;
		private int inputCurrent = 0;
		public const int StatePropertyId = 101;
		#endregion
	}


	public class Consumer : Conductor {

		public  static Consumer CreateInstance(ModelObjectType modelObjectType) {
			return new Consumer(modelObjectType);
		}


		public static readonly TerminalId InputTerminal;
		
		
		public override IModelObject Clone() {
			return new Consumer(this);
		}


		public override int TerminalCount {
			get { return 1; }
		}
		
		
		public override IEnumerable<TerminalId> GetTerminalIds() {
			yield return Input;
		}


		public override IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		public override int GetInteger(int propertyId) {
			if (propertyId == ConsumerValueId) return Value;
			else return base.GetInteger(propertyId);
		}


		public override void Conduct(IModelObject source, TerminalId sourceTerminal, int sourceCurrent) {
			if (sourceCurrent == 0 && connections != null) {
				int alternateCurrent = 0;
				for (int i = connections.Count - 1; i >= 0; --i) {
					if (connections[i].TargetModelObject == source) continue;
					if (connections[i].TargetModelObject is Conductor)
						alternateCurrent = Math.Max(alternateCurrent, ((Conductor)connections[i].TargetModelObject).Current);
				}
				base.Conduct(source, sourceTerminal, alternateCurrent);
			} else base.Conduct(source, sourceTerminal, sourceCurrent);
		}
		
		
		[PropertyMappingId(ConsumerValueId)]
		public int Value {
			get { return val; }
			set { 
				val = value;
				OnPropertyChanged(ConsumerValueId);
			}
		}


		protected Consumer(Consumer source)
			: base(source) {
			this.val = source.val;
		}


		protected Consumer(ModelObjectType type)
			: base(type) {
		}


		protected override TerminalId GetInputTerminal() {
			return Input;
		}


		protected override TerminalId GetOutputTerminal() {
			return TerminalId.Invalid;
		}
			
			
		static Consumer() {
			InputTerminal = Input;
		}


		#region Fields
		private int val = 0;
		public const int ConsumerValueId = 102;
		#endregion
	}


	public static class nShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			registrar.RegisterModelObjectType(new GenericModelObjectType("Wire", namespaceName, categoryTitle,
				Wire.CreateInstance, Wire.GetPropertyDefinitions, 1));
			registrar.RegisterModelObjectType(new GenericModelObjectType("Battery", namespaceName, categoryTitle,
				Battery.CreateInstance, Battery.GetPropertyDefinitions, 1));
			registrar.RegisterModelObjectType(new GenericModelObjectType("Switch", namespaceName, categoryTitle,
				Switch.CreateInstance, Switch.GetPropertyDefinitions, 1));
			registrar.RegisterModelObjectType(new GenericModelObjectType("Consumer", namespaceName, categoryTitle,
				Consumer.CreateInstance, Consumer.GetPropertyDefinitions, 1));
		}


		#region Fields

		private const string namespaceName = "SampleModel";
		private const string categoryTitle = "Electricity";
		private const int preferredRepositoryVersion = 1;

		#endregion
	}
}
