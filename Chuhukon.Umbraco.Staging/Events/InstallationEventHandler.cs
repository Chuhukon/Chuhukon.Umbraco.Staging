using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Configuration;
using System.IO;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

using System.Xml.Linq;
using System.Xml;
using System.Text;
using System.Diagnostics;
using Chuhukon.Umbraco.Staging.Core;

namespace Chuhukon.Umbraco.Staging
{
	/// <summary>
	/// Packaging helper
	/// </summary>
	public class InstallationEventHandler : ApplicationEventHandler
	{
        protected string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_staging\\");

		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

			base.ApplicationStarted(umbracoApplication, applicationContext);
            
            //experimental and out the scope of this project ContentService.Published += ExportContentOnPublish; //is only excecuted when ExportDocumentRootId is given.

            ContentTypeService.SavedContentType += ExportDocumentTypesOnSave;

            //todo: code first macro's
            //todo: Macro.AfterSave didn't work for some reason, figure out why.
            var macros = CreateMacrosXml();
            SaveToPackageXml(macros);

            var templates = CreateTemplatesXml(Settings.ImportViewAsTemplateOnStart);
			SaveToPackageXml(templates);
		}

		protected void SaveToPackageXml(XElement element)
		{
			var path = System.IO.Path.Combine(folder, "package.xml");

			XDocument doc;
			using(var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				doc = XDocument.Load(file);
			}

			using (var file = new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite, FileShare.None))
			{
				doc.Descendants(element.Name.LocalName).First().ReplaceWith(element);
				doc.Save(file);
			}
		}

		/// <summary>
		/// Create Macros package xml based on existing macro's
		/// </summary>
		protected XElement CreateMacrosXml()
		{
			var macros = umbraco.cms.businesslogic.macro.Macro.GetAll();

			var macroElement = new XElement("Macros");
			foreach (var macro in macros)
			{
				try
				{
					var doc = new XmlDocument();
					var elem = macro.ToXml(doc);
					macroElement.Add(XElement.Parse(elem.OuterXml));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(string.Format("Macro '{0}', could not be packaged, because of the next exception: '{1}'", macro.Name, ex.Message));
				}
			}

			var output = System.IO.Path.Combine(folder, "Macros.xml");
			macroElement.Save(output);

			return macroElement;
		}

		/// <summary>
		/// Create template package xml based on what's in the Views folder.
		/// </summary>
		/// <param name="importViews">When set to true for all *.cshtml views in the views root a Template is created.</param>
		protected XElement CreateTemplatesXml(bool importViews)
		{
			var templateElement = new XElement("Templates");
			var filepath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("Views"));
			foreach (var path in Directory.GetFiles(filepath, "*.cshtml"))
			{
				var viewname = Path.GetFileNameWithoutExtension(path);

				XElement elem;
				using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (var reader = new StreamReader(file))
					{
						var template = string.Format(
							@"<Template>
	<Name>{0}</Name>
	<Alias>{0}</Alias>
	<Design><![CDATA[{1}]]></Design>
</Template>",
						viewname, reader.ReadToEnd());
						elem = XElement.Parse(template, LoadOptions.PreserveWhitespace);
					}
				}

				if (elem != null)
				{
					templateElement.Add(elem);
				}
			}

			var output = System.IO.Path.Combine(folder, "Templates.xml");
			templateElement.Save(output);

			if (importViews) ApplicationContext.Current.Services.PackagingService.ImportTemplates(templateElement);

			return templateElement;
		}

		/// <summary>
		/// Create documentTypes package xml based on what's currently in the umbraco db
		/// </summary>
		protected XElement CreateDocumentTypesXml()
		{
			var contentTypes = new Queue<IContentType>(ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes());
			var exportTypes = new List<IContentType>();

			int total = contentTypes.Count();
			while (exportTypes.Count() < total) //first sort existing items, base templates need to be generated before there childs..
			{
				var type = contentTypes.Peek();
				if (type.ParentId == -1 || exportTypes.Any(t => t.Id == type.ParentId))
				{
					exportTypes.Add(type);
					contentTypes.Dequeue();
				}
				else
				{
					contentTypes.Enqueue(contentTypes.Dequeue());
				}
			}

			var docTypeElement = new XElement("DocumentTypes");
			foreach (var type in exportTypes)
			{
				try
				{
					docTypeElement.Add(ApplicationContext.Current.Services.PackagingService.Export(type));
				}
				catch (Exception ex)
				{
					Debug.WriteLine(string.Format("DocumentType '{0}', could not be packaged, because of the next exception: '{1}'", type.Name, ex.Message));
				}
			}

			var output = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_staging\\DocumentTypes.xml");
			docTypeElement.Save(output);

			return docTypeElement;
		}

        /// <summary>
		/// [[-- Experimental!! --]]
        /// Builds a document set which can be used in a package file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //protected void ExportContentOnPublish(global::Umbraco.Core.Publishing.IPublishingStrategy sender, global::Umbraco.Core.Events.PublishEventArgs<IContent> e)
        //{
            
        //    if (Settings.ExportDocumentRootId.HasValue)
        //    {
        //        int rootId = Settings.ExportDocumentRootId.Value;
        //        foreach (var item in e.PublishedEntities)
        //        {
        //            var content = ApplicationContext.Current.Services.ContentService.GetById(rootId);
        //            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("_staging\\Documents\\DocumentSet.xml"));
        //            using (var file = System.IO.File.Create(path))
        //            {
        //                var root = new XElement("DocumentSet"); //root
        //                XAttribute importMode = new XAttribute("importMode", "root");
        //                root.Add(importMode);

        //                //var writer = XmlWriter.Create(file, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Document } );
        //                BuildDocumentSet(content, root);
        //                root.Save(file);
        //            }
        //        }
        //    }   
        //}
        //
        //protected void BuildDocumentSet(IContent parent, XElement root)
        //{
        //    if (parent.Published)
        //    {
        //        root.Add(parent.ToXml());

        //        foreach (var child in parent.Children())
        //        {
        //            BuildDocumentSet(child, root);
        //        }
        //    }
        //}

		protected void ExportDocumentTypesOnSave(IContentTypeService sender, global::Umbraco.Core.Events.SaveEventArgs<global::Umbraco.Core.Models.IContentType> e)
		{
			var doctypes = CreateDocumentTypesXml();
			SaveToPackageXml(doctypes);
		}
	}
}