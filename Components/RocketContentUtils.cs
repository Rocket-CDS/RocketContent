using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketContent.Components
{
    public static class RocketContentUtils
    {
        public const string ControlPath = "/DesktopModules/DNNrocketModules/RocketContent";
        public const string ResourcePath = "/DesktopModules/DNNrocketModules/RocketContent/App_LocalResources";
        public static string DisplayView(int portalId, string moduleRef, string rowKey, SessionParams sessionParam, string template = "view.cshtml")
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam);
            if (!dataObject.ModuleSettings.HasAppThemeAdmin) return "loadsettings";

            var razorTempl = dataObject.AppThemeView.GetTemplate(template);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, null, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string DisplayAdminView(int portalId, string moduleRef, string rowKey, SessionParams sessionParam, string template = "AdminDetail.cshtml")
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, rowKey, sessionParam);
            if (!dataObject.ModuleSettings.HasAppThemeAdmin) return "loadsettings";

            var razorTempl = dataObject.AppThemeAdmin.GetTemplate(template);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, null, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string DisplaySystemView(int portalId, string moduleRef, SessionParams sessionParam, string template)
        {
            var dataObject = new DataObjectLimpet(portalId, moduleRef, "", sessionParam);
            if (dataObject.AppThemeSystem == null) return "No System View";

            var razorTempl = dataObject.AppThemeSystem.GetTemplate(template);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, dataObject.DataObjects, null, null, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public static string ResourceKey(string resourceKey, string resourceExt = "Text", string cultureCode = "")
        {
            return DNNrocketUtils.GetResourceString(ResourcePath, resourceKey, resourceExt, cultureCode);
        }        
        public static string TokenReplacementCultureCode(string str, string CultureCode)
        {
            if (CultureCode == "") return str;
            str = str.Replace("{culturecode}", CultureCode);
            var s = CultureCode.Split('-');
            if (s.Count() == 2)
            {
                str = str.Replace("{language}", s[0]);
                str = str.Replace("{country}", s[1]);
            }
            return str;
        }
    }

}
