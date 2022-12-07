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
            if (culturecode == "") culturecode = DNNrocketUtils.GetEditCulture();
            return new ArticleLimpet(_dataObject.PortalId, dataRef, culturecode);
        }
        public string AddRow()
        {
            var articleData = GetActiveArticle(_dataRef);
            _rowKey = articleData.AddRow();
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string SortRows()
        {
            var selectkeylist = _paramInfo.GetXmlProperty("genxml/hidden/selectkeylist");
            var rowSortOrder = new List<string>();
            var l = selectkeylist.Split(',');
            foreach (var rKey in l)
            {
                if (rKey != "") rowSortOrder.Add(rKey);
            }

            var sortedArticles = new List<ArticleLimpet>();

            // we need to sort ALL langauges. 
            foreach (var cultureCode in DNNrocketUtils.GetCultureCodeList(_dataObject.PortalId))
            {
                var articleData = new ArticleLimpet(_dataObject.PortalId, _dataRef, cultureCode);

                // Build new sorted list
                var rowInfoDict = new Dictionary<string, SimplisityInfo>();
                foreach (var r in articleData.GetRowList()) 
                {
                    var key = r.GetXmlProperty("genxml/config/rowkey");
                    rowInfoDict.Add(key, r);
                }

                // Remove existing row and Add sorted rows.
                articleData.Info.RemoveList("rows");
                foreach (var k in rowSortOrder)
                {
                    if (rowInfoDict.ContainsKey(k)) articleData.Info.AddListItem("rows", rowInfoDict[k]);
                }

                sortedArticles.Add(articleData);

            }

            // cannot update in loop, it will change sortorder on first record, the rest of the language will then be wrong.
            foreach (var a in sortedArticles)
            {
                a.Update();
                //a.RebuildLangIndex(); // rebuild the index record [Essential to get the correct sort order]
            }


            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            var articleData2 = GetActiveArticle(_dataRef);
            return AdminDetailDisplay(articleData2);
        }
        public string RemoveRow()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.RemoveRow(_rowKey);

            // reload so we always have 1 row.
            articleData = GetActiveArticle(_dataRef);

            _rowKey = articleData.GetRow(0).Info.GetXmlProperty("genxml/config/rowkey");
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string SaveArticleRow()
        {            
            _passSettings.Add("saved", "true");
            var articleData = GetActiveArticle(_dataRef);
            articleData.UpdateRow(_rowKey, _postInfo);
            CacheUtils.ClearAllCache("article");
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public void DeleteArticle()
        {
            CacheUtils.ClearAllCache("article");
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
                if (imgsize == 0) imgsize = _dataObject.ModuleSettings.Record.GetXmlPropertyInt("genxml/settings/imageresize");
                if (imgsize == 0) imgsize = 640;
                var imgList = ImgUtils.UploadBase64Image(filenameList, filebase64List, baseFileMapPath, _dataObject.PortalContent.ImageFolderMapPath, imgsize);
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
                var fileList = DocUtils.UploadBase64file(filenameList, filebase64List, _dataObject.PortalContent.DocFolderMapPath);
                if (fileList.Count == 0) return MessageDisplay("RC.invalidfile");
                foreach (var docFileMapPath in fileList)
                {
                    var articleRow = articleData.GetRow(_rowKey);
                    if (articleRow != null)
                    {
                        articleRow.AddDoc(Path.GetFileName(docFileMapPath));
                        articleData.UpdateRow(_rowKey, articleRow.Info);
                    }
                }
            }

            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string AddArticleListItem()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                var listName = _paramInfo.GetXmlProperty("genxml/hidden/listname");
                articleRow.Info.AddListItem(listName, new SimplisityInfo());
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
            if (_sessionParams.Get("remoteedit") == "true") return EditContent();
            return AdminDetailDisplay(articleData);
        }
        public string AddArticleLink()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.UpdateRow(_rowKey, _postInfo);

            var articleRow = articleData.GetRow(_rowKey);
            if (articleRow != null)
            {
                articleRow.AddLink();
                articleData.UpdateRow(_rowKey, articleRow.Info);
            }
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
            // rowKey can come from the sessionParams or paramInfo.  (Because on no rowkey on the language change)
            var articleRow = articleData.GetRow(0);
            if (_rowKey != "") articleRow = articleData.GetRow(_rowKey);
            if (articleRow == null) articleRow = articleData.GetRow(0);  // row removed and still in sessionparams
            var razorTempl = _dataObject.AppThemeAdmin.GetTemplate("admindetail.cshtml");
            _dataObject.SetDataObject("articlerow", articleRow);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleData, _dataObject.DataObjects, _passSettings, _sessionParams, true);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String AdminCreateArticle()
        {
            var appTheme = _postInfo.GetXmlProperty("genxml/radio/apptheme");
            if (appTheme == "") return "No AppTheme Selected";

            _moduleRef = GeneralUtils.GetGuidKey();

            return GetAdminArticle();
        }
        public String AdminListDisplay()
        {
            var articleDataList = new ArticleLimpetList(_paramInfo, _dataObject.PortalContent, _sessionParams.CultureCodeEdit, true);
            var dataObjects = new Dictionary<string, object>();
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("adminlist.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, articleDataList, _dataObject.DataObjects, _passSettings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public String AdminSelectAppThemeDisplay()
        {
            var appThemeDataList = new AppThemeDataList(_dataObject.SystemKey);
            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("SelectAppTheme.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, appThemeDataList, _dataObject.DataObjects, _passSettings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }
        public string ArticleDocumentList()
        {
            var articleid = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var docList = new List<object>();
            foreach (var i in DNNrocketUtils.GetFiles(DNNrocketUtils.MapPath(_dataObject.PortalContent.DocFolderRel)))
            {
                var sInfo = new SimplisityInfo();
                sInfo.SetXmlProperty("genxml/name", i.Name);
                sInfo.SetXmlProperty("genxml/relname", _dataObject.PortalContent.DocFolderRel + "/" + i.Name);
                sInfo.SetXmlProperty("genxml/fullname", i.FullName);
                sInfo.SetXmlProperty("genxml/extension", i.Extension);
                sInfo.SetXmlProperty("genxml/directoryname", i.DirectoryName);
                sInfo.SetXmlProperty("genxml/lastwritetime", i.LastWriteTime.ToShortDateString());
                docList.Add(sInfo);
            }

            _passSettings.Add("uploadcmd", "articleadmin_docupload");
            _passSettings.Add("deletecmd", "articleadmin_docdelete");
            _passSettings.Add("articleid", articleid.ToString());

            var razorTempl = _dataObject.AppThemeSystem.GetTemplate("DocumentSelect.cshtml");
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, docList, _dataObject.DataObjects, _passSettings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;

        }
        public void ArticleDocumentUploadToFolder()
        {
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(_dataObject.PortalContent.DocFolderMapPath)) Directory.CreateDirectory(_dataObject.PortalContent.DocFolderMapPath);
            var fileuploadlist = _paramInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        File.Copy(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, _dataObject.PortalContent.DocFolderMapPath + "\\" + friendlyname, true);
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
            return GetPublicView("View.cshtml");
        }
        public String GetPublicArticleHeader()
        {
            return GetPublicView("Viewlastheader.cshtml");
        }
        public String GetPublicArticleBeforeHeader()
        {
            return GetPublicView("Viewfirstheader.cshtml");
        }

        private string GetPublicView(string templateName)
        {
            var razorTempl = _dataObject.AppThemeView.GetTemplate(templateName, _moduleRef);
            if (razorTempl == "") return "";
            _dataObject.SetDataObject("paraminfo", _paramInfo);
            var pr = RenderRazorUtils.RazorProcessData(razorTempl, _dataObject.ArticleData, _dataObject.DataObjects, _passSettings, _sessionParams, _dataObject.PortalContent.DebugMode);
            if (pr.StatusCode != "00") return pr.ErrorMsg;
            return pr.RenderedText;
        }

    }
}

