﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using ICSharpCode.PythonBinding;
using ICSharpCode.SharpDevelop.Dom;
using NUnit.Framework;
using PythonBinding.Tests.Utils;

namespace PythonBinding.Tests.Resolver
{
	[TestFixture]
	public class ResolveSystemImportedAsMySystemTestFixture : ResolveTestsBase
	{
		protected override ExpressionResult GetExpressionResult()
		{
			List<ICompletionEntry> namespaceItems = new List<ICompletionEntry>();
			DefaultClass consoleClass = new DefaultClass(compilationUnit, "System.Console");
			namespaceItems.Add(consoleClass);
			projectContent.AddExistingNamespaceContents("System", namespaceItems);
			
			return new ExpressionResult("MySystem", ExpressionContext.Default);
		}
		
		protected override string GetPythonScript()
		{
			return 
				"import System as MySystem\r\n" +
				"MySystem.\r\n" +
				"\r\n";
		}
				
		[Test]
		public void ResolveResultContainsConsoleClass()
		{
			List<ICompletionEntry> items = GetCompletionItems();
			IClass consoleClass = PythonCompletionItemsHelper.FindClassFromCollection("Console", items);
			Assert.IsNotNull(consoleClass);
		}
		
		List<ICompletionEntry> GetCompletionItems()
		{
			return resolveResult.GetCompletionData(projectContent);
		}
		
		[Test]
		public void NamespaceResolveResultNameIsSystem()
		{
			NamespaceResolveResult namespaceResolveResult = resolveResult as NamespaceResolveResult;
			Assert.AreEqual("System", namespaceResolveResult.Name);
		}
		
		[Test]
		public void MockProjectContentSystemNamespaceContentsIncludesConsoleClass()
		{
			List<ICompletionEntry> items = projectContent.GetNamespaceContents("System");
			IClass consoleClass = PythonCompletionItemsHelper.FindClassFromCollection("Console", items);
			Assert.IsNotNull(consoleClass);
		}
		
		[Test]
		public void MockProjectContentNamespaceExistsReturnsTrueForSystem()
		{
			Assert.IsTrue(projectContent.NamespaceExists("System"));
		}
	}
}
