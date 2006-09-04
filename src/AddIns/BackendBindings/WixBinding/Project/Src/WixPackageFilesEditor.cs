﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision$</version>
// </file>

using ICSharpCode.SharpDevelop.Project;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace ICSharpCode.WixBinding
{
	/// <summary>
	/// Class that is responsible for editing the installer package files in the
	/// WiX source file.
	/// </summary>
	public class WixPackageFilesEditor
	{
		IWixPackageFilesView view;
		ITextFileReader fileReader;
		IWixDocumentWriter documentWriter;
		IDirectoryReader directoryReader;
		WixDocument document;
		WixSchemaCompletionData wixSchemaCompletionData;
		bool usingRootDirectoryRef;
		
		/// <summary>
		/// Gets the files and directories in the specified path.
		/// </summary>
		class DefaultDirectoryReader : IDirectoryReader
		{
			public string[] GetFiles(string path)
			{
				return Directory.GetFiles(path);
			}
			
			public string[] GetDirectories(string path)
			{
				return Directory.GetDirectories(path);
			}
		}
		
		/// <summary>
		/// Creates a new instance of the WixPackageFilesEditor.
		/// </summary>
		/// <param name="view">The UI for the package files editor.</param>
		/// <param name="fileReader">The file reader hides the file system and the
		/// workbench from the editor so the class can be easily tested.</param>
		/// <param name="documentWriter">The file writer hides the file system and the
		/// workbench from the editor.</param>
		/// <param name="directoryReader">The directory reader hides the file system
		/// from the editor.</param>
		public WixPackageFilesEditor(IWixPackageFilesView view, ITextFileReader fileReader, IWixDocumentWriter documentWriter, IDirectoryReader directoryReader)
		{
			this.view = view;
			this.fileReader = fileReader;
			this.documentWriter = documentWriter;
			this.directoryReader = directoryReader;
		}
		
		/// <summary>
		/// Creates a new instance of the WixPackageFilesEditor which uses
		/// the default directory reader which just uses the Directory class.
		/// </summary>
		public WixPackageFilesEditor(IWixPackageFilesView view, ITextFileReader fileReader, IWixDocumentWriter documentWriter)
			: this(view, fileReader, documentWriter, new DefaultDirectoryReader())
		{
		}
		
		/// <summary>
		/// Displays the setup files for the specified WixProject.
		/// </summary>
		public void ShowFiles(WixProject project)
		{
			// Look for Wix document containing root directory.
			if (project.WixSourceFiles.Count > 0) {
				bool errors = false;
				WixDocument currentDocument = null;
				view.ClearDirectories();
				foreach (FileProjectItem item in project.WixSourceFiles) {
					try {
						currentDocument = CreateWixDocument(item.FileName);
						FindRootDirectoryResult result = FindRootDirectory(currentDocument);
						if (result == FindRootDirectoryResult.RootDirectoryRefFound) {
							break;
						}
					} catch (XmlException) {
						errors = true;
					}
				}
				if (errors) {
					view.ShowSourceFilesContainErrorsMessage();
				} else if (document == null) {
					view.ShowNoSourceFileFoundMessage(project.Name);
				} else {
					SelectedElementChanged();
				}
			} else {
				view.ShowNoSourceFileFoundMessage(project.Name);
			}
		}
		
		/// <summary>
		/// Gets the Wix document containing the package files information.
		/// </summary>
		public WixDocument Document {
			get {
				return document;
			}
		}
		
		/// <summary>
		/// Saves changes made to the document.
		/// </summary>
		public void Save()
		{
			if (view.IsDirty) {
				documentWriter.Write(document);
				view.IsDirty = false;
			}
		}
		
		/// <summary>
		/// The item has been selected in the view.
		/// </summary>
		public void SelectedElementChanged()
		{
			XmlElement element = view.SelectedElement;
			view.AllowedChildElements.Clear();
			view.Attributes.Clear();
			if (element != null) {
				view.Attributes.AddRange(WixXmlAttribute.GetAttributes(element, WixSchemaCompletionData.GetAttributes(element.Name)));
				view.AllowedChildElements.AddRange(WixSchemaCompletionData.GetChildElements(element.Name));
			} else {
				view.AllowedChildElements.Add("Directory");
			}
			view.AttributesChanged();
		}
		
		/// <summary>
		/// The attribute value has changed.
		/// </summary>
		public void AttributeChanged()
		{
			view.IsDirty = true;
			UpdateChangedAttributes(view.SelectedElement);
		}
		
		/// <summary>
		/// Adds a new element.
		/// </summary>
		public void AddElement(string name)
		{
			XmlElement parentElement = view.SelectedElement;
			XmlElement element = null;
			switch (name) {
				case WixDirectoryElement.DirectoryElementName:
					element = AddDirectory(parentElement as WixDirectoryElement, "NewDirectory");
					break;
				case WixComponentElement.ComponentElementName:
					element = AddComponent(parentElement as WixDirectoryElement, "NewComponent");
					break;
				default:
					element = AddElement(parentElement, name);
					break;
			}
			
			if (element != null) {
				AddElementToView(element);
			}
		}
	
		/// <summary>
		/// Removes the selected element.
		/// </summary>
		public void RemoveElement()
		{
			XmlElement element = view.SelectedElement;
			if (element != null) {
				XmlElement parentElement = (XmlElement)element.ParentNode;
				parentElement.RemoveChild(element);
				view.RemoveElement(element);
				view.IsDirty = true;
				SelectedElementChanged();
			}
		}
		
		/// <summary>
		/// Adds the specified files to the selected element.
		/// </summary>
		public void AddFiles(string[] fileNames)
		{
			foreach (string fileName in fileNames) {
				AddFile(fileName);
			}
		}
		
		/// <summary>
		/// Adds the specified directory to the selected element. This
		/// adds all the contained files and subdirectories recursively to the
		/// setup package.
		/// </summary>
		public void AddDirectory(string directory)
		{
			WixDirectoryElement parentElement = (WixDirectoryElement)view.SelectedElement;
			
			// Add directory.
			string directoryName = WixDirectoryElement.GetLastDirectoryName(directory);
			WixDirectoryElement directoryElement = AddDirectory(parentElement, directoryName);
			AddFiles(directoryElement, directory);
			
			// Adds directory contents recursively.
			AddDirectoryContents(directoryElement, directory);
			
			AddElementToView(directoryElement);
		}
		
		/// <summary>
		/// Adds a single file to the selected component element.
		/// </summary>
		void AddFile(string fileName)
		{
			WixComponentElement componentElement = view.SelectedElement as WixComponentElement;
			if (componentElement != null) {
				WixFileElement fileElement = AddFile(componentElement, fileName);
				view.AddElement(fileElement);
				view.IsDirty = true;
			}
		}
		
		/// <summary>
		/// Adds a file to the specified component element.
		/// </summary>
		WixFileElement AddFile(WixComponentElement componentElement, string fileName)
		{
			WixFileElement fileElement = componentElement.AddFile(fileName);
			if (!componentElement.HasDiskId) {
				componentElement.DiskId = "1";
			}
			return fileElement;
		}
		
		/// <summary>
		/// Updates the attribute values for the element.
		/// </summary>
		void UpdateChangedAttributes(XmlElement changedElement)
		{
			if (changedElement != null) {
				foreach (WixXmlAttribute attribute in view.Attributes) {
					if (String.IsNullOrEmpty(attribute.Value)) {
						changedElement.RemoveAttribute(attribute.Name);
					} else {
						changedElement.SetAttribute(attribute.Name, attribute.Value);
					}
				}
			}
		}
		
		WixDocument CreateWixDocument(string fileName)
		{
			WixDocument doc = new WixDocument();
			doc.Load(fileReader.Create(fileName));
			doc.FileName = fileName;
			return doc;
		}
		
		/// <summary>
		/// Adds a new directory with a autogenerated id.
		/// </summary>
		WixDirectoryElement AddDirectory(WixDirectoryElementBase parentElement, string name)
		{
			if (parentElement == null) {
				if (usingRootDirectoryRef) {
					parentElement = document.RootDirectoryRef;
				} else {
					parentElement = document.RootDirectory;
					if (parentElement == null) {
						parentElement = document.AddRootDirectory();
					}
				}
			}
			WixDirectoryElement directoryElement = parentElement.AddDirectory(name);
			directoryElement.ShortName = name;
			return directoryElement;
		}
		
		WixSchemaCompletionData WixSchemaCompletionData {
			get {
				if (wixSchemaCompletionData == null) {
					wixSchemaCompletionData = new WixSchemaCompletionData();
				}
				return wixSchemaCompletionData;
			}
		}
		
		/// <summary>
		/// Adds a new element to the specified parent.
		/// </summary>
		XmlElement AddElement(XmlElement parentElement, string name)
		{
			if (parentElement != null) {
				XmlElement element = document.CreateWixElement(name);
				return (XmlElement)parentElement.AppendChild(element);
			}
			return null;
		}
		
		void AddElementToView(XmlElement element)
		{
			view.AddElement(element);
			view.SelectedElement = element;
			view.IsDirty = true;
			SelectedElementChanged();
		}
		
		/// <summary>
		/// Adds a new component element to the directory element.
		/// </summary>
		/// <param name="id">The id attribute the component element will have.</param>
		WixComponentElement AddComponent(WixDirectoryElement parentElement, string id)
		{
			if (parentElement != null) {
				WixComponentElement element = parentElement.AddComponent(id);
				return element;
			}
			return null;
		}
		
		enum FindRootDirectoryResult
		{
			NoMatch = 0,
			RootDirectoryFound = 1,
			RootDirectoryRefFound = 2
		}
		
		/// <summary>
		/// Tries to find a root directory or root directory ref in the specified
		/// document.
		/// </summary>
		FindRootDirectoryResult FindRootDirectory(WixDocument currentDocument)
		{
			if (currentDocument.IsProductDocument) {
				WixDirectoryElement rootDirectory = currentDocument.RootDirectory;
				if (rootDirectory != null) {
					view.AddDirectories(rootDirectory.GetDirectories());
				}
				document = currentDocument;
				return FindRootDirectoryResult.RootDirectoryFound;
			} else {
				WixDirectoryRefElement rootDirectoryRef = currentDocument.RootDirectoryRef;
				if (rootDirectoryRef != null) {
					view.AddDirectories(rootDirectoryRef.GetDirectories());
					document = currentDocument;
					usingRootDirectoryRef = true;
					return FindRootDirectoryResult.RootDirectoryRefFound;
				}
			}
			return FindRootDirectoryResult.NoMatch;
		}
		
		/// <summary>
		/// Adds the set of files to the specified directory element. Each file
		/// gets its own parent component element.
		/// </summary>
		void AddFiles(WixDirectoryElement directoryElement, string directory)
		{
			foreach (string fileName in DirectoryReader.GetFiles(directory)) {
				string path = Path.Combine(directory, fileName);
				string id = WixComponentElement.GenerateIdFromFileName(document, path);
				WixComponentElement component = AddComponent(directoryElement, id);
				AddFile(component, path);
			}
		}
		
		IDirectoryReader DirectoryReader {
			get {
				return directoryReader;
			}
		}
		
		/// <summary>
		/// Adds any subdirectories and files to the directory element.
		/// </summary>
		/// <param name="directoryElement">The directory element to add
		/// the components and subdirectories to.</param>
		/// <param name="directory">The full path of the directory.</param>
		void AddDirectoryContents(WixDirectoryElement directoryElement, string directory)
		{
			foreach (string subDirectoryName in DirectoryReader.GetDirectories(directory)) {
				string subDirectory = Path.Combine(directory, subDirectoryName);
				WixDirectoryElement subDirectoryElement = AddDirectory(directoryElement, subDirectoryName);
				AddFiles(subDirectoryElement, subDirectory);
				
				// Add the subdirectory contents.
				AddDirectoryContents(subDirectoryElement, subDirectory);
			}
		}
	}
}
