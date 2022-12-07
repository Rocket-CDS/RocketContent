using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContent.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace RocketContent.API
{
    public partial class StartConnect
    {
        private string SelectAppThemeProject()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.ProjectName = _paramInfo.GetXmlProperty("genxml/hidden/projectname");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string SelectAppTheme()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeAdminFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string SelectAppThemeView()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeViewFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string SelectAppThemeVersion()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeAdminVersion = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolderversion");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string SelectAppThemeVersionView()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeViewVersion = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolderversion");
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string ResetAppTheme()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.ProjectName = "";
            moduleData.AppThemeAdminFolder = "";
            moduleData.AppThemeAdminVersion = "";
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string ResetAppThemeView()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.AppThemeViewFolder = "";
            moduleData.AppThemeViewVersion = "";
            _dataObject.SetDataObject("modulesettings", moduleData);
            moduleData.Update();
            return RemoteSettings();
        }
        private string RemoteSettings()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("RemoteSettings.cshtml"); // only find system template.
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

        private string SaveSettings()
        {
            var moduleData = _dataObject.ModuleSettings;
            moduleData.Save(_postInfo);
            moduleData.Update();
            _dataObject.SetDataObject("modulesettings", moduleData);
            return RemoteSettings();
        }
        private string AppThemeVersions()
        {
            var appTheme = _postInfo.GetXmlProperty("genxml/remote/apptheme");
            if (_paramInfo.GetXmlProperty("genxml/hidden/ctrl") == "appthemeviewversion") appTheme = _postInfo.GetXmlProperty("genxml/remote/appthemeview");
            var razorTempl = GetSystemTemplate("RemoteAppThemeVersions.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.AppThemeView, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }


    }
}

