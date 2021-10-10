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
        private ArticleLimpet GetActiveArticle(int articleid)
        {
            return new ArticleLimpet(_portalContent.PortalId, articleid, _sessionParams.CultureCodeEdit);
        }

        public int SaveArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            _passSettings.Add("saved", "true");
            var articleData = new ArticleLimpet(_portalContent.PortalId, articleId, _sessionParams.CultureCodeEdit);
            return articleData.Save(_postInfo);
        }
        public void DeleteArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            GetActiveArticle(articleId).Delete();
        }
        public void CopyArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.ArticleId = -1;
            var newarticleId = articleData.Copy();
            // create all languages
            var l = DNNrocketUtils.GetCultureCodeList();
            foreach (var c in l)
            {
                articleData = new ArticleLimpet(_portalContent.PortalId, articleId, c);
                var newarticleData = new ArticleLimpet(_portalContent.PortalId, newarticleId, c);
                newarticleData.Save(articleData.Info);
                newarticleData.Name += " - " + LocalUtils.ResourceKey("RC.copy", "Text", c);
                newarticleData.Update();
            }
        }
        public string AddArticleImage()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Save(_postInfo);
            var imgList = ImgUtils.MoveImageToFolder(_postInfo, _portalContent.ImageFolderMapPath);
            foreach (var nam in imgList)
            {
                articleData.AddImage(nam);
            }
            return GetAdminArticle(articleData.ArticleId);
        }
        public string AddArticleImage64()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
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
                    return GetAdminArticle(articleData.ArticleId);
                }
            }
            return "ERROR: Invalid ItemId or base64 string";
        }
        public string AddArticleDoc()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Save(_postInfo);
            var docList = MoveDocumentToFolder(_postInfo, _portalContent.DocFolderMapPath);
            foreach (var nam in docList)
            {
                articleData.AddDoc(nam);
            }
            return GetAdminArticle(articleData.ArticleId);
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
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            if (articleId > 0)
            {
                var articleData = GetActiveArticle(articleId);
                articleData.Save(_postInfo);
                articleData.AddLink();
                return GetAdminArticle(articleData.ArticleId);
            }
            return "ERROR: Invalid ItemId";
        }

        public String AddArticle()
        {
            return GetAdminArticle(-1);
        }
        public String GetAdminArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            return GetAdminArticle(articleData);
        }
        public String GetAdminArticle(int articleId)
        {
            var articleData = GetActiveArticle(articleId);
            return GetAdminArticle(articleData);
        }
        public String GetAdminCreateArticle()
        {
            var appthemeFolder = _postInfo.GetXmlProperty("genxml/radio/apptheme");
            var articleData = GetActiveArticle(-1);
            articleData.AppThemeFolder = appthemeFolder;
            articleData.Update();
            return GetAdminArticle(articleData);
        }
        public String GetAdminSaveArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Save(_postInfo);
            return GetAdminArticle(articleData);
        }
        public String GetAdminDeleteArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            articleData.Delete();
            return GetAdminArticleList();
        }
        public String GetAdminArticle(ArticleLimpet articleData)
        {
            var razorTempl = _appThemeSystem.GetTemplate("admindetail.cshtml");
            return RenderRazorUtils.RazorDetail(razorTempl, articleData, _passSettings, _sessionParams, true);
        }
        public String GetAdminArticleList()
        {
            var articleDataList = new ArticleLimpetList(_paramInfo, _portalContent, _sessionParams.CultureCodeEdit, true);

            var razorTempl = _appThemeSystem.GetTemplate("adminlist.cshtml");
            var strOut = RenderRazorUtils.RazorDetail(razorTempl, articleDataList, _passSettings, _sessionParams, true);
            return strOut;
        }
        public String GetAdminSelectAppTheme()
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
        public String GetPublicArticle()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var razorTempl = articleData.AppTheme.GetTemplate("View.cshtml");
            if (razorTempl == "") return "";
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("paraminfo", _paramInfo); // we need this so we can check if a detail key has been passed.  if so, we need to do the SEO for the detail.            
            return RenderRazorUtils.RazorObjectRender(razorTempl, _portalContent, dataObjects, _passSettings, _sessionParams, _portalContent.DebugMode);
        }
        public String GetPublicArticleHeader()
        {
            var articleId = _paramInfo.GetXmlPropertyInt("genxml/hidden/articleid");
            var articleData = GetActiveArticle(articleId);
            var razorTempl = articleData.AppTheme.GetTemplate("ViewHeader.cshtml");
            if (razorTempl == "") return "";
            var dataObjects = new Dictionary<string, object>();
            dataObjects.Add("paraminfo", _paramInfo); // we need this so we can check if a detail key has been passed.  if so, we need to do the SEO for the detail.            
            return RenderRazorUtils.RazorObjectRender(razorTempl, _portalContent, dataObjects, _passSettings, _sessionParams, _portalContent.DebugMode);
        }

    }
}

