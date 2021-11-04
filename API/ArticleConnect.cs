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
        private ArticleLimpet GetActiveArticle(string dataRef, string culturecode = "")
        {
            if (culturecode == "") culturecode = _sessionParams.CultureCodeEdit;
            return new ArticleLimpet(_portalContent.PortalId, dataRef, culturecode);
        }
        public string AddRow()
        {
            _passSettings.Add("saved", "true");
            var articleData = new ArticleLimpet(_portalContent.PortalId, _dataRef, _sessionParams.CultureCodeEdit);
            articleData.AddRow();
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string SaveArticleRow()
        {
            
            _passSettings.Add("saved", "true");
            var articleData = new ArticleLimpet(_portalContent.PortalId, _dataRef, _sessionParams.CultureCodeEdit);
            articleData.UpdateRow(_rowKey, _postInfo);
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public void DeleteArticle()
        {
            GetActiveArticle(_dataRef).Delete();
        }
        public string AddArticleImage()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.UpdateRow(_rowKey, _postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var baseFileMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + GeneralUtils.GetGuidKey();
                var imgsize = _postInfo.GetXmlPropertyInt("genxml/hidden/imageresize");
                if (imgsize == 0) imgsize = 640;
                var imgList = ImgUtils.UploadBase64Image(filenameList, filebase64List, baseFileMapPath, _portalContent.ImageFolderMapPath, imgsize);
                foreach (var imgFileMapPath in imgList)
                {
                    var articleRow = articleData.GetRow(_rowKey);
                    if (articleRow != null)
                    {
                        articleRow.AddImage(Path.GetFileName(imgFileMapPath));
                        articleData.UpdateRow(_rowKey, articleRow.Info);
                    }
                }
            }

            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string AddArticleDoc()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.UpdateRow(_rowKey, _postInfo);

            // Add new image if found in postInfo
            var fileuploadlist = _postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            var fileuploadbase64 = _postInfo.GetXmlProperty("genxml/hidden/fileuploadbase64");
            if (fileuploadbase64 != "")
            {
                var filenameList = fileuploadlist.Split('*');
                var filebase64List = fileuploadbase64.Split('*');
                var fileList = DocUtils.UploadBase64file(filenameList, filebase64List, _portalContent.DocFolderMapPath);
                if (fileList.Count == 0) return MessageDisplay("RC.invalidfile");
                foreach (var imgFileMapPath in fileList)
                {
                    var articleRow = articleData.GetRow(_rowKey);
                    if (articleRow != null) articleRow.AddDoc(Path.GetFileName(imgFileMapPath));
                }
            }

            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string AddArticleLink()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null) articleRow.AddLink();
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public String GetAdminArticle()
        {
            var articleData = GetActiveArticle(_dataRef);
            return AdminDetailDisplay(articleData);
        }
        public String GetAdminDeleteArticle()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.Delete();
            return AdminListDisplay();
        }
        public String AdminDetailDisplay(ArticleLimpet articleData)
        {
            if (_remoteModule.AppThemeFolder == "") return LocalUtils.ResourceKey("RC.noapptheme");
            var articleRow = articleData.GetRow(0);
            if (_rowKey != "") articleRow = articleData.GetRow(_rowKey);
            var razorTempl = _appThemeSystem.GetTemplate("admindetail.cshtml");
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("apptheme", _appTheme);
            dataObjects.Add("remotemodule", _remoteModule);
            dataObjects.Add("articlerow", articleRow);
            return RenderRazorUtils.RazorDetail(razorTempl, articleData, dataObjects, _sessionParams, _passSettings, true);
        }
        public String AdminCreateArticle()
        {
            var appTheme = _postInfo.GetXmlProperty("genxml/radio/apptheme");
            if (appTheme == "") return "No AppTheme Selected";

            _dataRef = GeneralUtils.GetGuidKey();

            _remoteModule = new RemoteModule(_portalContent.PortalId, _dataRef);
            _remoteModule.Record.SetXmlProperty("genxml/remote/apptheme", appTheme);
            _remoteModule.Update();
            _appTheme = new AppThemeLimpet(_remoteModule.Record.GetXmlProperty("genxml/remote/apptheme"));

            return GetAdminArticle();
        }
        public String AdminListDisplay()
        {
            var articleDataList = new ArticleLimpetList(_paramInfo, _portalContent, _sessionParams.CultureCodeEdit, true);
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("apptheme", _appTheme);
            dataObjects.Add("remotemodule", _remoteModule);
            var razorTempl = _appThemeSystem.GetTemplate("adminlist.cshtml");
            return RenderRazorUtils.RazorDetail(razorTempl, articleDataList, dataObjects, _sessionParams, _passSettings, true);
        }
        public String AdminSelectAppThemeDisplay()
        {
            var appThemeDataList = new AppThemeDataList(_systemData.SystemKey);
            var razorTempl = _appThemeSystem.GetTemplate("SelectAppTheme.cshtml");
            var strOut = RenderRazorUtils.RazorDetail(razorTempl, appThemeDataList, _passSettings, _sessionParams, true);
            return strOut;
        }
        public string ArticleDocumentList()
        {
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var docList = new List<object>();
            foreach (var i in DNNrocketUtils.GetFiles(DNNrocketUtils.MapPath(_portalContent.DocFolderRel)))
            {
                var sInfo = new SimplisityInfo();
                sInfo.SetXmlProperty("genxml/name", i.Name);
                sInfo.SetXmlProperty("genxml/relname", _portalContent.DocFolderRel + "/" + i.Name);
                sInfo.SetXmlProperty("genxml/fullname", i.FullName);
                sInfo.SetXmlProperty("genxml/extension", i.Extension);
                sInfo.SetXmlProperty("genxml/directoryname", i.DirectoryName);
                sInfo.SetXmlProperty("genxml/lastwritetime", i.LastWriteTime.ToShortDateString());
                docList.Add(sInfo);
            }

            _passSettings.Add("uploadcmd", "articleadmin_docupload");
            _passSettings.Add("deletecmd", "articleadmin_docdelete");
            _passSettings.Add("articleid", articleid.ToString());

            var razorTempl = _appThemeSystem.GetTemplate("DocumentSelect.cshtml");
            return RenderRazorUtils.RazorList(razorTempl, docList, _passSettings, _sessionParams, true);
        }
        public void ArticleDocumentUploadToFolder()
        {
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(_portalContent.DocFolderMapPath)) Directory.CreateDirectory(_portalContent.DocFolderMapPath);
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        File.Copy(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, _portalContent.DocFolderMapPath + "\\" + friendlyname, true);
                        File.Delete(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }

            }
        }
        public void ArticleDeleteDocument()
        {
            var docfolder = _postInfo.GetXmlProperty("genxml/hidden/documentfolder");
            if (docfolder == "") docfolder = "docs";
            var docDirectory = PortalUtils.HomeDNNrocketDirectoryMapPath() + "\\" + docfolder;
            var docList = _postInfo.GetXmlProperty("genxml/hidden/dnnrocket-documentlist").Split(';');
            foreach (var i in docList)
            {
                if (i != "")
                {
                    var documentname = GeneralUtils.DeCode(i);
                    var docFile = docDirectory + "\\" + documentname;
                    if (File.Exists(docFile))
                    {
                        File.Delete(docFile);
                    }
                }
            }

        }        
        public String GetAppThemeList()
        {
            return "GetAppThemeList()";
        }
        public String GetPublicArticle()
        {
            var articleData = GetActiveArticle(_dataRef, _sessionParams.CultureCode);
            var razorTempl = _appTheme.GetTemplate("View.cshtml");
            if (razorTempl == "") return "";
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("paraminfo", _paramInfo);
            dataObjects.Add("portalcontent", _portalContent);
            return RenderRazorUtils.RazorObjectRender(razorTempl, articleData, dataObjects, _passSettings, _sessionParams, _portalContent.DebugMode);
        }
        public String GetPublicArticleHeader()
        {
            var articleData = GetActiveArticle(_dataRef);
            var razorTempl = _appTheme.GetTemplate("ViewHeader.cshtml");
            if (razorTempl == "") return "";
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("paraminfo", _paramInfo); // we need this so we can check if a detail key has been passed.  if so, we need to do the SEO for the detail.            
            return RenderRazorUtils.RazorObjectRender(razorTempl, _portalContent, dataObjects, _passSettings, _sessionParams, _portalContent.DebugMode);
        }

    }
}

