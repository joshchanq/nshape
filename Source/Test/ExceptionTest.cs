using System;
using Dataweb.NShape;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Dataweb.NShape.GeneralShapes;

namespace NShapeTest {

	[TestClass]
	public class ExceptionTest {

		[TestMethod]
		public void TestExceptionSerialization() {
			const string errorMessage = "Exception Serialization Test Message";
			const string innerErrorMessage = "Inner Exception Error Message";
			
			PerformExceptionTest(new NShapeException(errorMessage, new Exception(innerErrorMessage)));
			PerformExceptionTest(new NShapeSecurityException(Permission.ModifyPermissionSet));
			PerformExceptionTest(new NShapeInternalException(errorMessage, new Exception(innerErrorMessage)));
			PerformExceptionTest(new NShapeUnsupportedValueException(this));
			//PerformExceptionTest(new NShapeInterfaceNotSupportedException());
			//PerformExceptionTest(new NShapeMappingNotSupportedException());
			PerformExceptionTest(new NShapePropertyNotSetException(this, "Property"));
	}


		private void PerformExceptionTest<TException>(TException exception) where TException : Exception {
            // Save the full ToString() value, including the exception message and stack trace.
            string exceptionToString = exception.ToString();

            // Round-trip the exception: Serialize and de-serialize with a BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream memStream = new MemoryStream()) {
                // "Save" object state
                formatter.Serialize(memStream, exception);
                // Re-use the same stream for de-serialization
                memStream.Seek(0, 0);
                // Replace the original exception with de-serialized one
                exception = (TException)formatter.Deserialize(memStream);
            }
            // Double-check that the exception message and stack trace (owned by the base Exception) are preserved
            Assert.AreEqual(exceptionToString, exception.ToString(), string.Format("{0}.ToString() failed after serialization.", typeof(TException).Name));
		}

	}
}
