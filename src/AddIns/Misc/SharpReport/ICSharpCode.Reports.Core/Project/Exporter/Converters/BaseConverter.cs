﻿/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Peter
 * Datum: 28.12.2008
 * Zeit: 17:30
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Drawing;
using ICSharpCode.Reports.Core.Events;
using ICSharpCode.Reports.Core.Interfaces;

namespace ICSharpCode.Reports.Core.Exporter
{

	/// <summary>
	/// Description of BaseConverter.
	/// </summary>
	
	public class BaseConverter:IBaseConverter
	{
		public event EventHandler <NewPageEventArgs> PageFull;
		
		private BaseReportItem parentItem;
		private IDataNavigator dataNavigator;
		private ExporterPage singlePage;
		private SectionBounds sectionBounds;
		private Rectangle parentRectangle;
		private IExportItemsConverter exportItemsConverter;
		private ILayouter layouter;
	
		
		public BaseConverter(IDataNavigator dataNavigator,ExporterPage singlePage,IExportItemsConverter exportItemsConverter,ILayouter layouter)
		{
			if (dataNavigator == null) {
				throw new ArgumentNullException("dataNavigator");
			}
			if (singlePage == null) {
				throw new ArgumentNullException("singlePage");
			}
			if (exportItemsConverter == null) {
				throw new ArgumentNullException("exportItemsConverter");
			}
			if (layouter == null) {
				throw new ArgumentNullException("layouter");
			}
			this.singlePage = singlePage;
			this.dataNavigator = dataNavigator;
			this.sectionBounds = this.singlePage.SectionBounds;
			this.exportItemsConverter = exportItemsConverter;
			this.layouter = layouter;
		}
		
		
		protected bool IsPageFull (Rectangle rectangle)
		{
			if (rectangle.Bottom > SectionBounds.PageFooterRectangle.Top) {
				return true;
			}
			return false;
		}
		
		
		protected void FirePageFull (ExporterCollection items)
		{
			EventHelper.Raise<NewPageEventArgs>(PageFull,this,new NewPageEventArgs(items));
		}
		
			
		
		protected  ExporterCollection ConvertItems (BaseReportItem parent,
		                                            IContainerItem row,Point offset)
		                                
		{
			this.exportItemsConverter.Offset = offset.Y;
			IExportColumnBuilder exportLineBuilder = row as IExportColumnBuilder;

			if (exportLineBuilder != null) {

				this.dataNavigator.Fill(row.Items);
			
				ExportContainer lineItem = this.exportItemsConverter.ConvertToContainer(row);
				
				BaseReportItem baseReportItem = row as BaseReportItem;
				
				this.exportItemsConverter.ParentLocation = baseReportItem.Location;
				
				if (baseReportItem.BackColor != GlobalValues.DefaultBackColor) {
					foreach (BaseReportItem i in row.Items) {
						i.BackColor = baseReportItem.BackColor;
					}
				}
				
				ExporterCollection list = row.Items.ConvertAll <BaseExportColumn> (this.exportItemsConverter.ConvertToLineItem);
				
				lineItem.Items.AddRange(list);
				
				ExporterCollection containerList = new ExporterCollection();
				containerList.Add (lineItem);
				return containerList;
			}
			return null;
		}
		
		#region IBaseConverter
		
		public virtual ExporterCollection Convert(BaseReportItem parent, BaseReportItem item)
		{
			this.parentItem = parent;
			this.parentRectangle = new Rectangle(parent.Location,parent.Size);
			return new ExporterCollection();;
		}
		
		
		public Rectangle ParentRectangle {
			get { return parentRectangle; }
		}
		
		
		public ExporterPage SinglePage {
			get { return singlePage; }
		}
		
		public SectionBounds SectionBounds {
			get { return sectionBounds; }
		}
		
		public IDataNavigator DataNavigator {
			get { return dataNavigator; }
		}
		
		public IExportItemsConverter ExportItemsConverter {
			get { return exportItemsConverter; }
		}
		
		public ILayouter Layouter {
			get { return layouter; }
		}
		
		public Graphics Graphics {get;set;}
		#endregion
	}
}
