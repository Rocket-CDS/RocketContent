using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContent.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RocketContent.API
{
    public partial class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private SystemLimpet _systemData;
        private Dictionary<string, string> _passSettings;
        private SessionParams _sessionParams;
        private UserParams _userParams;
        private AppThemeSystemLimpet _appThemeSystem;
        private PortalContentLimpet _portalContent;
        private string _dataRef;
        private AppThemeLimpet _appTheme;
        private RemoteModule _remoteModule;
        private string _rowKey;

        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            var storeParamCmd = paramCmd;

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {
                case "rocketsystem_edit":
                    strOut = RocketSystem();
                    break;
                case "rocketsystem_init":
                    strOut = RocketSystemInit();
                    break;
                case "rocketsystem_delete":
                    strOut = RocketSystemDelete();
                    break;
                case "rocketsystem_valid":
                    strOut = RocketSystemValid();
                    break;
                case "rocketsystem_adminpanel":
                    strOut = AdminPanel();
                    break;
                case "rocketsystem_adminpanelheader":
                    strOut = AdminPanelHeader();
                    break;
                case "rocketsystem_save":
                    strOut = RocketSystemSave();
                    break;
                case "rocketsystem_login":
                    _userParams.TrackClear(_systemData.SystemKey);
                    strOut = ReloadPage();
                    break;
                


                case "dashboard_get":
                    strOut = GetDashboard();
                    break;



                case "article_adminlist":
                    strOut = AdminListDisplay();
                    break;
                case "article_selectapptheme":
                    strOut = AdminSelectAppThemeDisplay();
                    break;
                case "article_admindetail":
                    strOut = GetAdminArticle();
                    break;
                case "article_adminsave":
                    strOut = SaveArticleRow();
                    break;
                case "article_admindelete":
                    strOut = GetAdminDeleteArticle();
                    break;



                case "remote_addrow":
                    strOut = AddRow();
                    break;
                case "remote_addlink":
                    strOut = AddArticleLink();
                    break;
                case "remote_adddoc":
                    strOut = AddArticleDoc();
                    break;
                case "remote_addimage":
                    strOut = AddArticleImage();
                    break;
                case "remote_editoption":
                    strOut = "true";
                    break;
                case "remote_edit":
                    strOut = EditContent();
                    break;
                case "remote_editsave":
                    strOut = SaveArticleRow();
                    break;
                case "remote_settings":
                    strOut = RemoteSettings();
                    break;
                case "remote_settingssave":
                    strOut = SaveSettings();
                    break;


                case "remote_publiclist":
                    strOut = ""; // not used for rocketcontent
                    break;
                case "remote_publicview":
                    strOut = GetPublicArticle();
                    break;
                case "remote_publicviewheader":
                    strOut = GetPublicArticleHeader();
                    break;
                case "remote_publicseo":
                    strOut = ""; // not used for rocketcontent
                    break;

                case "invalidcommand":
                    strOut = "INVALID COMMAND: " + storeParamCmd;
                    break;

            }

            rtnDic.Add("outputhtml", strOut);
            return rtnDic;

        }

        private string RocketSystemSave()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                var portalContent = new PortalContentLimpet(portalId, _sessionParams.CultureCodeEdit);
                if (portalContent.PortalId >= 0) portalContent.Save(_postInfo);
                _portalContent = new PortalContentLimpet(portalId, _sessionParams.CultureCodeEdit); // reload portal data after save (for langauge change)
                var razorTempl = _appThemeSystem.GetTemplate("RocketSystem.cshtml");
                return RenderRazorUtils.RazorDetail(razorTempl, _portalContent, _passSettings, _sessionParams, true);
            }
            return "Invalid PortalId";
        }
        private String RocketSystem()
            {
                try
                {
                    _portalContent = new PortalContentLimpet(_portalContent.PortalId, _sessionParams.CultureCodeEdit);
                    var razorTempl = _appThemeSystem.GetTemplate("RocketSystem.cshtml");
                    return RenderRazorUtils.RazorDetail(razorTempl, _portalContent, _passSettings, _sessionParams, true);
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            private String RocketSystemInit()
            {
                try
                {
                    var newportalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/newportalid");
                    if (newportalId > 0)
                    {
                        _portalContent = new PortalContentLimpet(newportalId, _sessionParams.CultureCodeEdit);
                        _portalContent.Validate();
                        _portalContent.Update();
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            private String RocketSystemDelete()
            {
                try
                {
                    var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                    if (portalId > 0)
                    {
                        _portalContent = new PortalContentLimpet(portalId, _sessionParams.CultureCodeEdit);
                        _portalContent.Delete();
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            private String RocketSystemValid()
            {
                try
                {
                    var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
                    if (portalId > 0)
                    {
                        _portalContent = new PortalContentLimpet(portalId, _sessionParams.CultureCodeEdit);
                        if (!_portalContent.Valid)
                        {
                            var razorTempl = _appThemeSystem.GetTemplate("InvalidSystem.cshtml");
                            return RenderRazorUtils.RazorDetail(razorTempl, _portalContent, _passSettings, _sessionParams, true);
                        }
                    }
                    return "";
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        private string AdminPanel()
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("AdminPanel.cshtml");
                return RenderRazorUtils.RazorDetail(razorTempl, _portalContent, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string AdminPanelHeader()
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("AdminPanelHeader.cshtml");
                return RenderRazorUtils.RazorDetail(razorTempl, _portalContent, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string ReloadPage()
        {
            try
            {
                // user does not have access, logoff
                UserUtils.SignOut();

                var portalAppThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
                var razorTempl = portalAppThemeSystem.GetTemplate("Reload.cshtml");
                return RenderRazorUtils.RazorDetail(razorTempl, null, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string GetDashboard()
        {
            try
            {
                var razorTempl = _appThemeSystem.GetTemplate("Dashboard.cshtml");
                return RenderRazorUtils.RazorDetail(razorTempl, _portalContent, _passSettings, _sessionParams, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string RemoteSettings()
        {
            try
            {
                var appThemeDataList = new AppThemeDataList(_systemData.SystemKey);
                var razorTempl = _appThemeSystem.GetTemplate("RemoteSettings.cshtml");

                var remoteModule = new RemoteModule(_portalContent.PortalId, _dataRef);

                var nbRazor = new SimplisityRazor(appThemeDataList, _passSettings);
                nbRazor.DataObjects.Add("remotemodule", remoteModule);
                nbRazor.SessionParamsData = _sessionParams;
                nbRazor.DataRef = _dataRef;
                nbRazor.ModuleId = _paramInfo.ModuleId;
                return RenderRazorUtils.RazorDetail(razorTempl, nbRazor);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string SaveSettings()
        {
            try
            {
                if (_dataRef != "")
                {
                    var remoteModule = new RemoteModule(_portalContent.PortalId, _dataRef);
                    remoteModule.Save(_postInfo);
                }
                return RemoteSettings();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string EditContent()
        {
            try
            {
                var remoteModule = new RemoteModule(_portalContent.PortalId, _dataRef);
                if (remoteModule.AppThemeFolder == "") return LocalUtils.ResourceKey("RC.noapptheme");
                var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);
                var articleRow = articleData.GetRow(0);
                if (_rowKey != "")articleRow = articleData.GetRow(_rowKey);
                var razorTempl = _appThemeSystem.GetTemplate("remotedetail.cshtml");
                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("apptheme", _appTheme);
                dataObjects.Add("remotemodule", _remoteModule);
                dataObjects.Add("articlerow", articleRow);
                return RenderRazorUtils.RazorDetail(razorTempl, articleData, dataObjects, _sessionParams, _passSettings, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string MessageDisplay(string msgKey)
        {
            try
            {
                var remoteModule = new RemoteModule(_portalContent.PortalId, _dataRef);
                var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);
                var razorTempl = _appThemeSystem.GetTemplate("MessageDisplay.cshtml");
                var dataObjects = new Dictionary<string, object>();
                dataObjects.Add("apptheme", _appTheme);
                dataObjects.Add("remotemodule", _remoteModule);
                _passSettings.Add("msgkey", msgKey);
                return RenderRazorUtils.RazorDetail(razorTempl, articleData, dataObjects, _sessionParams, _passSettings, true);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _systemData = new SystemLimpet(systemInfo.GetXmlProperty("genxml/systemkey"));
            _appThemeSystem = new AppThemeSystemLimpet(_systemData.SystemKey);
            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _userParams = new UserParams(_sessionParams.BrowserSessionId);
            _passSettings = new Dictionary<string, string>();
            _dataRef = _paramInfo.GetXmlProperty("genxml/hidden/dataref");
            if (_dataRef == "") _dataRef = _paramInfo.GetXmlProperty("genxml/remote/dataref");
            _rowKey = _postInfo.GetXmlProperty("genxml/config/key");
            if (_rowKey == "") _rowKey = _paramInfo.GetXmlProperty("genxml/hidden/key");

            // Assign Langauge
            DNNrocketUtils.SetCurrentCulture();
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid >= 0 && PortalUtils.GetPortalId() == 0)
                _portalContent = new PortalContentLimpet(portalid, _sessionParams.CultureCodeEdit); // Portal 0 is admin, editing portal setup
            else
            {
                _portalContent = new PortalContentLimpet(PortalUtils.GetPortalId(), _sessionParams.CultureCodeEdit);
                if (!_portalContent.Active) return "";
            }
            // [TODO]: Users should only have access to their own services for setup on portal 0.
            // [TODO]: Private admin needs to allow access for managers.
            // [TODO]: Public facing API should allow access for all users.

            _remoteModule = new RemoteModule(_portalContent.PortalId, _dataRef);
            _appTheme = new AppThemeLimpet(_remoteModule.Record.GetXmlProperty("genxml/remote/apptheme"));

            var securityData = new SecurityLimpet(_portalContent.PortalId, _systemData.SystemKey, _rocketInterface, -1, -1);
            paramCmd = securityData.HasSecurityAccess(paramCmd, "rocketsystem_login");

            return paramCmd;
        }
    }

}
