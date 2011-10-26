﻿using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Zyan.Communication;
using Zyan.Communication.Toolbox;
using Zyan.Communication.Toolbox.Compression;
using Zyan.Communication.ChannelSinks.Compression;

namespace Zyan.Tests
{
	#region Unit testing platform abstraction layer
#if NUNIT
	using NUnit.Framework;
	using TestClass = NUnit.Framework.TestFixtureAttribute;
	using TestMethod = NUnit.Framework.TestAttribute;
	using ClassInitializeNonStatic = NUnit.Framework.TestFixtureSetUpAttribute;
	using ClassInitialize = DummyAttribute;
	using ClassCleanupNonStatic = NUnit.Framework.TestFixtureTearDownAttribute;
	using ClassCleanup = DummyAttribute;
	using TestContext = System.Object;
#else
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using ClassCleanupNonStatic = DummyAttribute;
	using ClassInitializeNonStatic = DummyAttribute;
#endif
	#endregion

	/// <summary>
	/// Test class for type helper.
	///</summary>
	[TestClass]
	public class TypeHelperTests
	{
		#region Dynamically-compiled secret assembly

		private const string SecretAssemblyName = "SecretAssembly";

		private const string SecretClassName = "SecretClass";

		private static Lazy<Assembly> SecretAssembly = new Lazy<Assembly>(() =>
		{
			var sourceCode = "internal class " + SecretClassName + " { }";
			var compiler = new CSharpCodeProvider();
			var options = new CompilerParameters
			{
				GenerateExecutable = false,
				OutputAssembly = SecretAssemblyName
			};

			var result = compiler.CompileAssemblyFromSource(options, sourceCode);
			return result.CompiledAssembly;
		});

		private static Lazy<string> SecretAssemblyFullName = new Lazy<string>(() => SecretAssembly.Value.GetName().FullName);

		private static Lazy<Type> SecretClass = new Lazy<Type>(() => SecretAssembly.Value.GetType(SecretClassName));

		private static Lazy<string> SecretClassFullName = new Lazy<string>(() => SecretClass.Value.AssemblyQualifiedName);

		[ClassInitialize]
		public static void ValidateSecretAssembly(TestContext dummy)
		{
			Assert.IsNotNull(SecretAssembly.Value);
			Assert.IsNotNull(SecretClass.Value);

			Assert.IsFalse(string.IsNullOrWhiteSpace(SecretAssemblyFullName.Value));
			Assert.IsFalse(string.IsNullOrWhiteSpace(SecretClassFullName.Value));

			Assert.IsTrue(SecretAssemblyFullName.Value.StartsWith(SecretAssemblyName));
			Assert.IsTrue(SecretClassFullName.Value.StartsWith(SecretClassName));
		}

		[ClassInitializeNonStatic]
		public void ValidateSecretAssemblyNonStatic()
		{
			ValidateSecretAssembly(null);
		}

		#endregion

		[TestMethod]
		public void TypeHelper_ReturnsStaticallyLinkedType()
		{
			var typeName = typeof(ZyanDispatcher).AssemblyQualifiedName;
			var type1 = Type.GetType(typeName);
			var type2 = TypeHelper.GetType(typeName);

			Assert.IsNotNull(type1);
			Assert.IsNotNull(type2);

			Assert.AreSame(type1, typeof(ZyanDispatcher));
			Assert.AreSame(type2, typeof(ZyanDispatcher));
		}

		[TestMethod]
		public void TypeHelper_ReturnsTypeFromDynamicAssemblyUsingFullAssemblyName()
		{
			var typeName = SecretClass.Value.AssemblyQualifiedName;
			var type = TypeHelper.GetType(typeName);

			Assert.IsNotNull(type);
			Assert.AreSame(type, SecretClass.Value);
		}

		[TestMethod]
		public void TypeHelper_ReturnsTypeFromDynamicAssemblyUsingPartialAssemblyName()
		{
			var typeName = SecretClassName + ", " + SecretAssemblyName;
			var type = TypeHelper.GetType(typeName);

			Assert.IsNotNull(type);
			Assert.AreSame(type, SecretClass.Value);
		}

		[TestMethod]
		public void TypeHelper_ReturnsTypeFromDynamicAssemblyUsingFullAssemblyNameWithBadCasing()
		{
			// convert assembly name to the upper case
			var typeName = SecretClass.Value.AssemblyQualifiedName;
			var indexOfComma = typeName.IndexOf(",");
			var asmName = typeName.Substring(indexOfComma + 1);
			typeName = typeName.Substring(0, indexOfComma + 1) + asmName.ToUpper();

			var type = TypeHelper.GetType(typeName);
			Assert.IsNotNull(type);
			Assert.AreSame(type, SecretClass.Value);
		}

		[TestMethod]
		public void TypeHelper_ReturnsTypeFromDynamicAssemblyUsingPartialAssemblyNameWithBadCasing()
		{
			var typeName = SecretClassName + ", " + SecretAssemblyName.ToUpper();
			var type = TypeHelper.GetType(typeName);

			Assert.IsNotNull(type);
			Assert.AreSame(type, SecretClass.Value);
		}
	}
}
