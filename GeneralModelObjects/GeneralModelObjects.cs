using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using Dataweb.nShape.Advanced;

namespace Dataweb.nShape.GeneralModelObjects {

	public enum StateEnum { On, Off, Blocked, Defect, Unknown };


	public class ValueDevice : ModelObjectBase {

		public static ValueDevice CreateInstance(ModelObjectType modelObjectType) {
			return new ValueDevice(modelObjectType);
		}
		
		
		protected internal ValueDevice(ModelObjectType modelObjectType)
			: base(modelObjectType) {
		}


		protected internal ValueDevice(ValueDevice source)
			: base(source) {
			this.MaxValue = source.MaxValue;
			this.MinValue = source.MinValue;
		}


		public override IModelObject Clone() {
			return new ValueDevice(this);
		}


		public override IEnumerable<nShapeAction> GetActions() {
			throw new NotImplementedException();
		}


		public override void Connect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException();
		}


		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException();
		}

	
		[Description("The state of the device. This value is represented by the assigned Shape.")]
		public StateEnum State {
			get { return state; }
			set { state = value; }
		}

	
		//[Description("The current value of the Device.")]
		//public double Value {
		//   get { return this.value; }
		//   set { this.value = value; }
		//}


		[Description("The minimum value of the Device.")]
		public double MinValue {
			get { return minValue; }
			set { minValue = value; }
		}


		[Description("The maximum value of the Device.")]
		public double MaxValue {
			get { return maxValue; }
			set { maxValue = value; }
		}

		//public override IEnumerable<nShapeAction> GetActions() {
		//   // "Set State", "Set Min Value", "Set Max Value", "SetFloat";
		//}


		//private double value;
		private StateEnum state;
		private double minValue;
		private double maxValue;
	}


	public static class nShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			registrar.RegisterModelObjectType(new GenericModelObjectType("ValueDevice", namespaceName, categoryTitle, 
				ValueDevice.CreateInstance, ValueDevice.GetPropertyDefinitions, 2));
		}


		#region Fields

		private const string namespaceName = "GeneralModelObjects";
		private const string categoryTitle = "General";
		private const int preferredRepositoryVersion = 2;

		#endregion
	}

}