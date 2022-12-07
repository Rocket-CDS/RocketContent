using DNNrocketAPI.Components;
using Rocket.AppThemes.Components;
using RocketContent.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketContent.API
{
    public partial class StartConnect : DNNrocketAPI.APInterface
    {
        private SimplisityInfo _postInfo;
        private SimplisityInfo _paramInfo;
        private RocketInterface _rocketInterface;
        private Dictionary<string, string> _passSettings;
        private SessionParams _sessionParams;
        private string _dataRef;
        private string _moduleRef;
        private string _rowKey;
        private DataObjectLimpet _dataObject;

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


                case "remote_selectappthemeproject":
                    strOut = SelectAppThemeProject();
                    break;
                case "remote_selectapptheme":
                    strOut = SelectAppTheme();
                    break;
                case "remote_selectappthemeversion":
                    strOut = SelectAppThemeVersion();
                    break;
                case "remote_resetapptheme":
                    strOut = ResetAppTheme();
                    break;                    


                case "remote_publiclist":
                    strOut = ""; // not used for rocketcontent
                    break;
                case "remote_publicview":
                    strOut = GetPublicArticle();
                    break;
                case "remote_publicviewlastheader":
                    strOut = GetPublicArticleHeader();
                    break;
                case "remote_publicviewfirstheader":
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

            if (!rtnDic.ContainsKey("remote-settingsxml")) rtnDic.Add("remote-settingsxml", _dataObject.RemoteModule.Record.ToXmlItem());            
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
            return _dataObject.AppThemeSystem.GetTemplate(templateName);
        }
        private string RocketSystemSave()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid"); // we may have passed selection
            if (portalId >= 0)
            {
                _dataObject.PortalContent.Save(_postInfo);
                _dataObject.PortalData.Record.SetXmlProperty("genxml/systems/" + _dataObject.SystemKey + "setup", "True");
                _dataObject.PortalData.Update();
                return RocketSystem();
            }
            return "Invalid PortalId";
        }
        private String RocketSystem()
        {
            var razorTempl = GetSystemTemplate("RocketSystem.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private String RocketSystemInit()
        {
            var newportalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/newportalid");
            if (newportalId > 0)
            {
                var portalContent = new PortalContentLimpet(newportalId, _sessionParams.CultureCodeEdit);
                portalContent.Validate();
                portalContent.Update();
                _dataObject.SetDataObject("portalcontent", portalContent);
            }
            return "";
        }
        private String RocketSystemDelete()
        {
            var portalId = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalId > 0)
            {
                _dataObject.PortalContent.Delete();
            }
            return "";
        }
        private string AdminPanel()
        {
            var razorTempl = GetSystemTemplate("AdminPanel.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string AdminPanelHeader()
        {
            var razorTempl = GetSystemTemplate("AdminPanelHeader.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string ReloadPage()
        {
            // user does not have access, logoff
            UserUtils.SignOut();

            var portalAppThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = portalAppThemeSystem.GetTemplate("Reload.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string GetDashboard()
        {
            var razorTempl = GetSystemTemplate("Dashboard.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.PortalContent, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string SelectAppThemeProject()
        {
            var articleData = _dataObject.ArticleData;
            articleData.ProjectName = _paramInfo.GetXmlProperty("genxml/hidden/projectname");
            articleData.Update();
            _dataObject.SetDataObject("articledata", articleData);
            return RemoteSettings();
        }
        private string SelectAppTheme()
        {
            var articleData = _dataObject.ArticleData;
            articleData.AppThemeFolder = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolder");
            articleData.Update();
            _dataObject.SetDataObject("articledata", articleData);
            return RemoteSettings();
        }
        private string SelectAppThemeVersion()
        {
            var articleData = _dataObject.ArticleData;
            articleData.AppThemeFolderVersion = _paramInfo.GetXmlProperty("genxml/hidden/appthemefolderversion");
            _dataObject.SetDataObject("articledata", articleData);
            articleData.Update();
            return RemoteSettings();
        }
        private string ResetAppTheme()
        {
            var articleData = _dataObject.ArticleData;
            articleData.ProjectName = "";
            articleData.AppThemeFolder = "";
            articleData.AppThemeFolderVersion = "";
            _dataObject.SetDataObject("articledata", articleData);
            articleData.Update();
            return RemoteSettings();
        }
        private string RemoteSettings()
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("RemoteSettings.cshtml"); // only find system template.
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.DataObjects, _passSettings,_sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string EditContent()
        {
            var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);

            if (articleData.AppThemeFolder == "")
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
            _dataObject.DataObjects.Remove("apptheme");
            _dataObject.DataObjects.Add("apptheme", new AppThemeLimpet(_dataObject.PortalContent.PortalId, articleData.AppThemeFolder, articleData.AppThemeFolderVersion, articleData.ProjectName));
            _dataObject.DataObjects.Add("articlerow", articleRow);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string MessageDisplay(string msgKey)
        {
            var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCodeEdit);
            var razorTempl = GetSystemTemplate("MessageDisplay.cshtml");
            _passSettings.Add("msgkey", msgKey);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string SaveSettings()
        {
            if (_moduleRef != "")
            {
                var remoteModule = new RemoteModule(_dataObject.PortalContent.PortalId, _moduleRef);
                remoteModule.Save(_postInfo);
                // update sitekey after Save(), it replaces all XML.
                remoteModule.SiteKey = _sessionParams.SiteKey;
                remoteModule.Update();

                // Make sure we have the correct _org, if changed.
                _dataObject.SetDataObject("remotemodule", remoteModule);

                // add the appTheme to the DataRecord. This is so we can get AppTheme for View.
                var articleData = new ArticleLimpet(_dataObject.PortalId, _dataRef, _sessionParams.CultureCodeEdit);
                articleData.AppThemeFolder = remoteModule.AppThemeFolder;
                articleData.AppThemeFolderVersion = remoteModule.AppThemeVersion;
                articleData.ProjectName = _dataObject.AppThemeProjectName;
                articleData.Update();
            }
            return RemoteSettings();
        }
        private string AppThemeVersions()
        {
            var appTheme = _postInfo.GetXmlProperty("genxml/remote/apptheme");
            if (_paramInfo.GetXmlProperty("genxml/hidden/ctrl") == "appthemeviewversion") appTheme = _postInfo.GetXmlProperty("genxml/remote/appthemeview");
            var razorTempl = GetSystemTemplate("RemoteAppThemeVersions.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.AppThemeView, _dataObject.DataObjects,  _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid == 0) portalid = PortalUtils.GetCurrentPortalId();

            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);
            _passSettings = new Dictionary<string, string>();

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

            _dataObject = new DataObjectLimpet(portalid, _sessionParams.ModuleRef, _sessionParams.CultureCodeEdit);
            // Check if we have an AppTheme
            if (_dataObject.AppThemeView == null)
            {
                if (paramCmd == "remote_selectappthemeproject" || paramCmd == "remote_selectapptheme") return paramCmd; // Check if we are updating the AppTheme.
                return "remote_settings";
            }
            _dataObject.AppThemeView = new AppThemeLimpet(portalid, _dataObject.RemoteModule.AppThemeViewFolder, _dataObject.RemoteModule.AppThemeViewVersion, _dataObject.AppThemeProjectName); ;
            _dataObject.AppThemeAdmin = new AppThemeLimpet(portalid, _dataObject.RemoteModule.AppThemeFolder, _dataObject.RemoteModule.AppThemeFolder, _dataObject.AppThemeProjectName);

            if (_dataObject.PortalContent.PortalId != 0 && !_dataObject.PortalContent.Active) return "";

            _dataRef = _dataObject.RemoteModule.DataRef;

            if (_dataRef == "") 
            {
                // If we are editing from the AdminPanel, we will not have a moduleRef, only a dataref.
                _dataRef = _paramInfo.GetXmlProperty("genxml/hidden/dataref");
                if (_dataRef == "") _dataRef = _paramInfo.GetXmlProperty("genxml/remote/dataref");
            }

            var securityData = new SecurityLimpet(_dataObject.PortalId, _dataObject.SystemKey, _rocketInterface, -1, -1);

            if (!paramCmd.StartsWith("remote_public"))
            {
                if (paramCmd.StartsWith("remote_"))
                {
                    var sk = _paramInfo.GetXmlProperty("genxml/remote/securitykeyedit");
                    if (!UserUtils.IsEditor() && _dataObject.PortalData.SecurityKeyEdit != sk) paramCmd = "";
                }
                else
                {
                    paramCmd = securityData.HasSecurityAccess(paramCmd, "rocketsystem_login");
                }
            }

            return paramCmd;
        }
    }

}
