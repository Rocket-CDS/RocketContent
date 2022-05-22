using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContent.Components;
using RocketPortal.Components;
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
        private AppThemeLimpet _appTheme;
        private AppThemeSystemLimpet _appThemeSystem;
        private AppThemeSystemLimpet _appThemeContent;
        private PortalContentLimpet _portalContent;
        private string _dataRef;
        private string _moduleRef;
        private string _rowKey;
        private PortalLimpet _portalData;
        private RemoteModule _remoteModule;
        private string _org;
        private Dictionary<string, object> _dataObjects;
        private OrganisationLimpet _orgData;

        public override Dictionary<string, object> ProcessCommand(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            var strOut = ""; // return nothing if not matching commands.
            var storeParamCmd = paramCmd;

            paramCmd = InitCmd(paramCmd, systemInfo, interfaceInfo, postInfo, paramInfo, langRequired);

            var rtnDic = new Dictionary<string, object>();

            switch (paramCmd)
            {
                case "rocketcontent_adminpanel":
                    strOut = AdminPanel();
                    break;

                case "rocketsystem_edit":
                    strOut = RocketSystem();
                    break;
                case "rocketsystem_init":
                    strOut = RocketSystemInit();
                    break;
                case "rocketsystem_delete":
                    strOut = RocketSystemDelete();
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

                    
                case "article_admincreate":
                    strOut = AdminCreateArticle();
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


                    
                case "remote_sortrows":
                    strOut = SortRows();
                    break;
                case "remote_removerow":
                    strOut = RemoveRow();
                    break;
                case "remote_addrow":
                    strOut = AddRow();
                    break;
                case "remote_addlistitem":
                    strOut = AddArticleListItem();
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
                    if (_sessionParams.Get("remoteedit") == "false")
                        strOut = AdminDetailDisplay(GetActiveArticle(_dataRef));
                    else
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
                case "remote_getappthemeversions":
                    strOut = AppThemeVersions();
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
                case "remote_publicviewbeforeheader":
                    strOut = GetPublicArticleBeforeHeader();
                    break;
                case "remote_publicseo":
                    strOut = ""; // not used for rocketcontent
                    break;

                case "invalidcommand":
                    strOut = "INVALID COMMAND: " + storeParamCmd;
                    break;

            }
            if (paramCmd == "remote_publicview" || paramCmd == "remote_publicmenu")
            {
                rtnDic.Add("remote-firstheader", GetPublicArticleBeforeHeader());
                rtnDic.Add("remote-lastheader", GetPublicArticleHeader());
            }

            if (!rtnDic.ContainsKey("remote-settingsxml")) rtnDic.Add("remote-settingsxml", _remoteModule.Record.ToXmlItem());            
            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);

            // tell remote module it can cache the resposne 
            rtnDic.Add("remote-cache", "true");

            return rtnDic;

        }
        /// <summary>
        /// We may have a wrapper system, so check both systems for the template 
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        private string GetSystemTemplate(string templateName)
        {
            var razorTempl = _appTheme.GetTemplate(templateName);
            if (razorTempl == "") razorTempl = _appThemeSystem.GetTemplate(templateName);
            if (razorTempl == "") razorTempl = _appThemeContent.GetTemplate(templateName);
            return razorTempl;
        }
        private string RocketSystemSave()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                _portalContent.Save(_postInfo);
                _portalData.Record.SetXmlProperty("genxml/systems/" + _systemData.SystemKey + "setup", "True");
                _portalData.Update();
                return RocketSystem();
            }
            return "Invalid PortalId";
        }
        private String RocketSystem()
        {
            var razorTempl = GetSystemTemplate("RocketSystem.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalContent, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private String RocketSystemInit()
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
        private String RocketSystemDelete()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalId > 0)
            {
                _portalContent = new PortalContentLimpet(portalId, _sessionParams.CultureCodeEdit);
                _portalContent.Delete();
            }
            return "";
        }
        private string AdminPanel()
        {
            var razorTempl = GetSystemTemplate("AdminPanel.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalContent, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string AdminPanelHeader()
        {
            var razorTempl = GetSystemTemplate("AdminPanelHeader.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalContent, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string ReloadPage()
        {
            // user does not have access, logoff
            UserUtils.SignOut();

            var portalAppThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = portalAppThemeSystem.GetTemplate("Reload.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string GetDashboard()
        {
            var razorTempl = GetSystemTemplate("Dashboard.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _portalContent, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string RemoteSettings()
        {
            var appThemeDataList = new AppThemeDataList(_org, _systemData.SystemKey);
            var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);
            var razorTempl = _appThemeContent.GetTemplate("RemoteSettings.cshtml"); // only find system template.
            _dataObjects.Add("articledata", articleData);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeDataList, _dataObjects, _passSettings,_sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string EditContent()
        {
            var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);

            if (articleData.AdminAppThemeFolder == "")
            {
                // should not happen, this is a data error.  Delete and Create.
                articleData.Delete();
                return AdminSelectAppThemeDisplay();
            }


            // rowKey can come from the sessionParams or paramInfo.  (Because on no rowkey on the language change)
            var articleRow = articleData.GetRow(0);
            if (_rowKey != "") articleRow = articleData.GetRow(_rowKey);
            if (articleRow == null) articleRow = articleData.GetRow(0);  // row removed and still in sessionparams
            var razorTempl = GetSystemTemplate("remotedetail.cshtml");
            _dataObjects.Remove("apptheme");
            _dataObjects.Add("apptheme", new AppThemeLimpet(_portalContent.PortalId, articleData.AdminAppThemeFolder, articleData.AdminAppThemeFolderVersion, articleData.Organisation));
            _dataObjects.Add("articlerow", articleRow);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string MessageDisplay(string msgKey)
        {
            var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);
            var razorTempl = GetSystemTemplate("MessageDisplay.cshtml");
            _passSettings.Add("msgkey", msgKey);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string SaveSettings()
        {
            if (_moduleRef != "")
            {
                var remoteModule = new RemoteModule(_portalContent.PortalId, _moduleRef);
                remoteModule.Save(_postInfo);
                // update sitekey after Save(), it replaces all XML.
                remoteModule.SiteKey = _sessionParams.SiteKey;
                remoteModule.Update();

                // Make sure we have the correct _org, if changed.
                _remoteModule = remoteModule;
                _org = _remoteModule.Organisation;
                if (_org == "") _org = _orgData.DefaultOrg();

                // add the appTheme to the DataRecord. This is so we can get AppTheme for View.
                var articleData = new ArticleLimpet(_portalData.PortalId, _dataRef, _sessionParams.CultureCodeEdit);
                articleData.AdminAppThemeFolder = remoteModule.AppThemeFolder;
                articleData.AdminAppThemeFolderVersion = remoteModule.AppThemeVersion;
                articleData.Organisation = _org;
                articleData.Update();
            }
            return RemoteSettings();
        }
        private string AppThemeVersions()
        {
            var appTheme = _postInfo.GetXmlProperty("genxml/remote/apptheme");
            if (_paramInfo.GetXmlProperty("genxml/hidden/ctrl") == "appthemeviewversion") appTheme = _postInfo.GetXmlProperty("genxml/remote/appthemeview");
            var appThemeData = new AppThemeLimpet(_portalData.PortalId, appTheme, "", _org);
            if (!appThemeData.Exists) return "Invalid AppTheme: " + appTheme;
            var razorTempl = GetSystemTemplate("RemoteAppThemeVersions.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeData, _dataObjects,  _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;
            _systemData = new SystemLimpet(systemInfo.GetXmlProperty("genxml/systemkey"));
            _appThemeSystem = new AppThemeSystemLimpet(_systemData.SystemKey);
            _appThemeContent = new AppThemeSystemLimpet("rocketcontent");
            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _userParams = new UserParams(_sessionParams.BrowserSessionId);
            _passSettings = new Dictionary<string, string>();
            _orgData = new OrganisationLimpet();
            _moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            if (_moduleRef == "") _moduleRef = _paramInfo.GetXmlProperty("genxml/remote/moduleref");
            _rowKey = _postInfo.GetXmlProperty("genxml/config/rowkey");
            if (_rowKey == "") _rowKey = _paramInfo.GetXmlProperty("genxml/hidden/rowkey");
            _sessionParams.ModuleRef = _moduleRef; // we need this on the module view template, to stop clashes in modules that use the same dataref. 

            // use a selectkey.  the selectkey is the same as the rowkey.
            // we can not duplicate ID on simplisity_click in the s-fields, when the id is on the form. 
            // The paramInfo field would contain the same as the form.  On load this may be empty.
            var selectkey = _paramInfo.GetXmlProperty("genxml/hidden/selectkey");
            if (selectkey != "") _rowKey = selectkey;

            // Assign Langauge
            DNNrocketUtils.SetCurrentCulture();
            if (_sessionParams.CultureCode == "") _sessionParams.CultureCode = DNNrocketUtils.GetCurrentCulture();
            if (_sessionParams.CultureCodeEdit == "") _sessionParams.CultureCodeEdit = DNNrocketUtils.GetEditCulture();
            DNNrocketUtils.SetCurrentCulture(_sessionParams.CultureCode);
            DNNrocketUtils.SetEditCulture(_sessionParams.CultureCodeEdit);

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid >= 0 && PortalUtils.GetPortalId() == 0)
            {
                _remoteModule = new RemoteModule(portalid, _moduleRef);
                _portalContent = new PortalContentLimpet(portalid, _sessionParams.CultureCodeEdit); // Portal 0 is admin, editing portal setup
            }
            else
            {
                _remoteModule = new RemoteModule(PortalUtils.GetPortalId(), _moduleRef);
                _portalContent = new PortalContentLimpet(PortalUtils.GetPortalId(), _sessionParams.CultureCodeEdit);
                if (!_portalContent.Active) return "";
            }
            _portalData = new PortalLimpet(_portalContent.PortalId);
            _dataRef = _remoteModule.DataRef;

            if (_dataRef == "") 
            {
                // If we are editing from the AdminPanel, we will not have a moduleRef, only a dataref.
                _dataRef = _paramInfo.GetXmlProperty("genxml/hidden/dataref");
                if (_dataRef == "") _dataRef = _paramInfo.GetXmlProperty("genxml/remote/dataref");
            }

            _org = _remoteModule.Organisation;
            if (_org == "") _org = _orgData.DefaultOrg();

            _appTheme = new AppThemeLimpet(_portalContent.PortalId, _remoteModule.AppThemeViewFolder, _remoteModule.AppThemeViewVersion, _org);

            var securityData = new SecurityLimpet(_portalContent.PortalId, _systemData.SystemKey, _rocketInterface, -1, -1);

            _dataObjects = new Dictionary<string, object>();
            _dataObjects.Add("remotemodule", _remoteModule);
            _dataObjects.Add("apptheme", _appTheme);
            _dataObjects.Add("appthemesystem", _appThemeSystem);
            _dataObjects.Add("appthemecontent", _appThemeContent);
            _dataObjects.Add("portalcontent", _portalContent);
            _dataObjects.Add("portaldata", _portalData);
            _dataObjects.Add("securitydata", securityData);
            _dataObjects.Add("systemdata", _systemData);

            if (paramCmd.StartsWith("remote_"))
            {
                var sk = _paramInfo.GetXmlProperty("genxml/remote/securitykeyedit");
                if (!UserUtils.IsEditor() && _portalData.SecurityKeyEdit != sk) paramCmd = "";                
            }
            else
            {
                paramCmd = securityData.HasSecurityAccess(paramCmd, "rocketsystem_login");
            }

            return paramCmd;
        }
    }

}
