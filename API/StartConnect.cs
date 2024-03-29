﻿using DNNrocketAPI.Components;
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
        private string _moduleRef;
        private string _rowKey;
        private DataObjectLimpet _dataObject;
        private int _moduleId;
        private int _tabId;

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
                case "rocketsystem_save":
                    strOut = RocketSystemSave();
                    break;
                case "rocketsystem_login":
                    strOut = ReloadPage();
                    break;
                


                case "article_admindetail":
                    strOut = AdminDetailDisplay();
                    break;
                case "article_admincreate":
                    strOut = AdminCreateArticle();
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
                case "article_addimage":
                    strOut = AddArticleImage();
                    break;
                case "article_adddoc":
                    strOut = AddArticleDoc();
                    break;
                case "article_addlink":
                    strOut = AddArticleLink();
                    break;
                case "article_addrow":
                    strOut = AddRow();
                    break;
                case "article_editrow":
                    strOut = AdminDetailDisplay();
                    break;
                case "article_removerow":
                    strOut = RemoveRow();
                    break;
                case "article_sortrows":
                    strOut = SortRows();
                    break;
                case "article_addlistitem":
                    strOut = AddArticleListItem();
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
            if (paramCmd == "remote_publicview")
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
        public string InitCmd(string paramCmd, SimplisityInfo systemInfo, SimplisityInfo interfaceInfo, SimplisityInfo postInfo, SimplisityInfo paramInfo, string langRequired = "")
        {
            _postInfo = postInfo;
            _paramInfo = paramInfo;

            var portalid = _paramInfo.GetXmlPropertyInt("genxml/hidden/portalid");
            if (portalid == 0) portalid = PortalUtils.GetCurrentPortalId();

            _rocketInterface = new RocketInterface(interfaceInfo);
            _sessionParams = new SessionParams(_paramInfo);

            _moduleRef = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
            _tabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
            _moduleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/moduleid");
            _rowKey = _postInfo.GetXmlProperty("genxml/config/rowkey");
            if (_rowKey == "") _rowKey = _paramInfo.GetXmlProperty("genxml/hidden/rowkey");
            _sessionParams.ModuleRef = _moduleRef; // we need this on the module view template, to stop clashes in modules that use the same dataref. 
            _sessionParams.TabId = _tabId;
            _sessionParams.ModuleId = _moduleId;

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

            _dataObject = new DataObjectLimpet(portalid, _sessionParams.ModuleRef, _rowKey, _sessionParams);

            if (paramCmd.StartsWith("remote_public")) return paramCmd;
            
            if (!_dataObject.ModuleSettings.HasAppThemeAdmin) // Check if we have an AppTheme
            {
                if (paramCmd.StartsWith("rocketcontent_") || paramCmd.StartsWith("rocketsystem_")) return paramCmd;
                return "rocketcontent_settings";
            }

            var securityData = new SecurityLimpet(_dataObject.PortalId, _dataObject.SystemKey, _rocketInterface, _sessionParams.TabId, _sessionParams.ModuleId);
            return securityData.HasSecurityAccess(paramCmd, "rocketsystem_login");
        }
    }

}
