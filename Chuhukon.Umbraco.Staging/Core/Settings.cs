using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Chuhukon.Umbraco.Staging.Core
{
	/// <summary>
	/// Settings from the web.config Macaw.Umbraco.Foundation.* keys
	/// </summary>
	public static class Settings
	{
		public static bool ImportViewAsTemplateOnStart
		{
			get
			{
                var ret = WebConfigurationManager.AppSettings["Chuhukon.Umbraco.Staging.ImportViewsAsTemplatesOnStart"];
				return string.IsNullOrWhiteSpace(ret) ? false : Convert.ToBoolean(ret);
			}
		}

        //public static int? ExportDocumentRootId
        //{
        //    get
        //    {
        //        int rootId = -1;
        //        if (int.TryParse(Convert.ToString(WebConfigurationManager.AppSettings["Chuhukon.Umbraco.Staging.ExportDocumentRootId"]), out rootId))
        //            return rootId;

        //        return null;
        //    }
        //}
	}
}
