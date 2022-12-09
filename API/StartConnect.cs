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
                    strOut = RenderSystemTemplate("AdminPanel.cshtml");
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
                    strOut = RenderSystemTemplate("AdminPanelHeader.cshtml.cshtml");
                    break;
                case "rocketsystem_save":
                    strOut = RocketSystemSave();
                    break;
                case "rocketsystem_login":
                    strOut = ReloadPage();
                    break;
                


                case "dashboard_get":
                    strOut = RenderSystemTemplate("Dashboard.cshtml");
                    break;


                case "article_admindetail":
                    strOut = AdminDetailDisplay();
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
                        strOut = AdminDetailDisplay();
                    else
                        strOut = EditContent();
                    break;
                case "remote_editsave":
                    strOut = SaveArticleRow();
                    break;


                case "rocketcontent_settings":
                    strOut = DisplaySettings();
                    break;
                case "rocketcontent_savesettings":
                    strOut = SaveSettings();
                    break;
                case "rocketcontent_selectappthemeproject":
                    strOut = SelectAppThemeProject();
                    break;
                case "rocketcontent_selectapptheme":
                    strOut = SelectAppTheme("");
                    break;
                case "rocketcontent_selectappthemeview":
                    strOut = SelectAppTheme("view");
                    break;
                case "rocketcontent_selectappthemeversion":
                    strOut = SelectAppThemeVersion("");
                    break;
                case "rocketcontent_selectappthemeversionview":
                    strOut = SelectAppThemeVersion("view");
                    break;
                case "rocketcontent_resetapptheme":
                    strOut = ResetAppTheme();
                    break;
                case "rocketcontent_resetappthemeview":
                    strOut = ResetAppThemeView();
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

            if (!rtnDic.ContainsKey("remote-settingsxml")) rtnDic.Add("remote-settingsxml", _dataObject.ModuleSettings.Record.ToXmlItem());            
            if (!rtnDic.ContainsKey("outputjson")) rtnDic.Add("outputhtml", strOut);

            // tell remote module it can cache the resposne 
            rtnDic.Add("remote-cache", "true");

            return rtnDic;

        }
        private string RenderSystemTemplate(string templateName)
        {
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate(templateName);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
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
            return RenderSystemTemplate("RocketSystem.cshtml");
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
        private string ReloadPage()
        {
            // user does not have access, logoff
            UserUtils.SignOut();

            var portalAppThemeSystem = new AppThemeDNNrocketLimpet("rocketportal");
            var razorTempl = portalAppThemeSystem.GetTemplate("Reload.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, null, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string EditContent()
        {
            // rowKey can come from the sessionParams or paramInfo.  (Because on no rowkey on the language change)
            var articleRow = _dataObject.ArticleData.GetRow(0);
            if (_rowKey != "") articleRow = _dataObject.ArticleData.GetRow(_rowKey);
            if (articleRow == null) articleRow = _dataObject.ArticleData.GetRow(0);  // row removed and still in sessionparams
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("remotedetail.cshtml");
            _dataObject.SetDataObject("articlerow", articleRow);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ArticleData, _dataObject.DataObjects, _dataObject.Settings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        private string MessageDisplay(string msgKey)
        {
            _dataObject.SetSetting("msgkey", msgKey);
            return RenderSystemTemplate("MessageDisplay.cshtml");
        }
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid == 0) portalid = PortalUtils.GetCurrentPortalId();

            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);

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

            _dataObject = new DataObjectLimpet(portalid, _sessionParams.ModuleRef, _rowKey, _sessionParams.CultureCodeEdit);

            if (_dataObject.PortalContent.PortalId != 0 && !_dataObject.PortalContent.Active) return "";

            if (paramCmd.StartsWith("article"))
            {
                if (!UserUtils.IsEditor()) return "";
            }
            else
            {
                if (!_dataObject.ModuleSettings.HasAppThemeView) // setup module.
                {
                    if (paramCmd == "rocketcontent_selectappthemeproject"
                        || paramCmd == "rocketcontent_selectapptheme"
                        || paramCmd == "rocketcontent_selectappthemeview"
                        || paramCmd == "rocketcontent_selectappthemeversion"
                        || paramCmd == "rocketcontent_selectappthemeversionview"
                        || paramCmd == "rocketcontent_resetapptheme"
                        || paramCmd == "rocketcontent_resetappthemeview"
                        ) return paramCmd; // Check if we are updating the AppTheme.
                    return "rocketcontent_settings";
                }
            }

            _dataRef = _dataObject.ModuleSettings.DataRef;

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
