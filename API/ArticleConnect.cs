﻿using DNNrocketAPI.Components;
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
        private ArticleLimpet GetActiveArticle(string dataRef)
        {
            return new ArticleLimpet(_portalContent.PortalId, dataRef, _sessionParams.CultureCodeEdit);
        }
        public int SaveArticle()
        {
            _passSettings.Add("saved", "true");
            var articleData = new ArticleLimpet(_portalContent.PortalId, _dataRef, _sessionParams.CultureCodeEdit);
            return articleData.Save(_postInfo);
        }
        public void DeleteArticle()
        {
            GetActiveArticle(_dataRef).Delete();
        }
        public string AddArticleImage()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.Save(_postInfo);
            var imgList = ImgUtils.MoveImageToFolder(_postInfo, _portalContent.ImageFolderMapPath);
            foreach (var nam in imgList)
            {
                articleData.AddImage(nam);
            }
            return GetAdminArticle();
        }
        public string AddArticleImage64()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.Save(_postInfo);

            var userfilename = UserUtils.GetCurrentUserId() + "_base64image.jpg";
            var baseFileMapPath = PortalUtils.TempDirectoryMapPath() + "\\" + userfilename;

            var base64image = _postInfo.GetXmlProperty("genxml/base64image");
            var baseArray = base64image.Split(',');
            if (baseArray.Length >= 2)
            {
                base64image = baseArray[1];
                var sInfo = new SimplisityInfo();
                sInfo.SetXmlProperty("genxml/hidden/fileuploadlist", "base64image.jpg");

                var bytes = Convert.FromBase64String(base64image);
                using (var imageFile = new FileStream(baseFileMapPath, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                }

                var imgList = ImgUtils.MoveImageToFolder(sInfo, _portalContent.ImageFolderMapPath);
                foreach (var nam in imgList)
                {
                    articleData.AddImage(nam);
                }
                return GetAdminArticle();
            }
            return "";
        }
        public string AddArticleDoc()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.Save(_postInfo);
            var docList = MoveDocumentToFolder(_postInfo, _portalContent.DocFolderMapPath);
            foreach (var nam in docList)
            {
                articleData.AddDoc(nam);
            }
            return GetAdminArticle();
        }
        private List<string> MoveDocumentToFolder(SimplisityInfo postInfo, string destinationFolder, int maxDocuments = 50)
        {
            destinationFolder = destinationFolder.TrimEnd('\\');
            var rtn = new List<string>();
            var userid = UserUtils.GetCurrentUserId(); // prefix to filename on upload.
            if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);
            var fileuploadlist = postInfo.GetXmlProperty("genxml/hidden/fileuploadlist");
            if (fileuploadlist != "")
            {
                var docCount = 1;
                foreach (var f in fileuploadlist.Split(';'))
                {
                    if (f != "")
                    {
                        var friendlyname = GeneralUtils.DeCode(f);
                        var userfilename = userid + "_" + friendlyname;
                        if (docCount <= maxDocuments)
                        {
                            var unqName = DNNrocketUtils.GetUniqueFileName(friendlyname.Replace(" ", "_"), destinationFolder);
                            var fname = destinationFolder + "\\" + unqName;
                            File.Move(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename, fname);
                            if (File.Exists(fname))
                            {
                                rtn.Add(unqName);
                                docCount += 1;
                            }
                        }
                        File.Delete(PortalUtils.TempDirectoryMapPath() + "\\" + userfilename);
                    }
                }
            }
            return rtn;
        }
        public string AddArticleLink()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.Save(_postInfo);
            articleData.AddLink();
            return GetAdminArticle();
        }
        public String GetAdminArticle()
        {
            var articleData = GetActiveArticle(_dataRef);
            return AdminDetailDisplay(articleData);
        }
        public String GetAdminSaveArticle()
        {
            var articleData = GetActiveArticle(_dataRef);
            articleData.Save(_postInfo);
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
            var razorTempl = _appThemeSystem.GetTemplate("admindetail.cshtml");
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("apptheme", _appTheme);
            dataObjects.Add("remotemodule", _remoteModule);  
            return RenderRazorUtils.RazorDetail(razorTempl, articleData, dataObjects, _sessionParams, _passSettings, true);
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
            var articleData = GetActiveArticle(_dataRef);
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

