using DNNrocketAPI;
using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DNNrocketAPI.Components;
using System.Globalization;
using System.Text.RegularExpressions;
using RocketContent.Components;

namespace RocketContent.Components
{
    public class ArticleLimpet
    {
        public const string _tableName = "RocketContent";
        public const string _entityTypeCode = "ART";
        private DNNrocketController _objCtrl;
        private int _articleId;

        public ArticleLimpet()
        {
            Info = new SimplisityInfo();
        }
        /// <summary>
        /// Read an existing article, if it does not exist the "Exists" property will be false. 
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="langRequired"></param>
        public ArticleLimpet(int articleId, string langRequired)
        {
            Info = new SimplisityInfo();
            _articleId = articleId;
            Populate(langRequired);
        }
        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="articleId"></param>
        /// <param name="langRequired"></param>
        public ArticleLimpet(int portalId, int articleId, string langRequired)
        {
            if (articleId <= 0) articleId = -1;  // create new record.
            _articleId = articleId;
            PortalId = portalId;
            Info = new SimplisityInfo();
            Info.ItemID = articleId;
            Info.TypeCode = _entityTypeCode;
            Info.ModuleId = -1;
            Info.UserId = -1;
            Info.PortalId = PortalId;

            Populate(langRequired);
        }
        /// <summary>
        /// When we populate with a child article row.
        /// </summary>
        /// <param name="articleData"></param>
        public ArticleLimpet(ArticleLimpet articleData)
        {
            Info = articleData.Info;
            _articleId = articleData.ArticleId;
            CultureCode = articleData.CultureCode;
            PortalId = Info.PortalId;
        }
        private void Populate(string cultureCode)
        {
            _objCtrl = new DNNrocketController();
            CultureCode = cultureCode;

            var info = _objCtrl.GetData(_entityTypeCode, _articleId, CultureCode, ModuleId, _tableName); // get existing record.
            if (info != null && info.ItemID > 0) Info = info; // check if we have a real record, or a dummy being created and not saved yet.
            Info.Lang = CultureCode;
            PortalId = Info.PortalId;
            if (Ref == "") Ref = GeneralUtils.GetGuidKey();
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        private void ReplaceInfoFields(SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = Info.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.RemoveXmlNode(xpathListSelect.Replace("*","") + nod.Name);
                }
            }
            textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    Info.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                }
            }
        }
        public int Save(SimplisityInfo postInfo)
        {
            ReplaceInfoFields(postInfo, "genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/lang/genxml/textbox/*");
            ReplaceInfoFields(postInfo, "genxml/checkbox/*");
            ReplaceInfoFields(postInfo, "genxml/select/*");
            ReplaceInfoFields(postInfo, "genxml/radio/*");

            // Always use the same AppTheme. (It can be changed, but can lead to unexpected results)
            //ReplaceInfoFields(postInfo, "genxml/config/*");

            UpdateImages(postInfo.GetList("imagelist"));
            UpdateDocs(postInfo.GetList("documentlist"));
            UpdateLinks(postInfo.GetList("linklist"));

            return ValidateAndUpdate();
        }
        public int Update()
        {
            Info = _objCtrl.SaveData(Info, _tableName);
            if (Info.GUIDKey == "")
            {
                Info.GUIDKey = GeneralUtils.GetGuidKey();
                Info = _objCtrl.SaveData(Info, _tableName);
            }
            return Info.ItemID;
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public int Copy()
        {
            Info.ItemID = -1;
            Info.GUIDKey = GeneralUtils.GetGuidKey();
            return Update();
        }

        public void AddListItem(string listname)
        {
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Info.AddListItem(listname);         
            Update();
        }
        public void Validate()
        {
        }

        #region "images"

        public string ImageListName { get { return "imagelist";  } }
        public void UpdateImages(List<SimplisityInfo> imageList)
        {
            Info.RemoveList(ImageListName);
            foreach (var sInfo in imageList)
            {
                var imgData = new ArticleImage(sInfo, "articleimage");
                UpdateImage(imgData);
            }
        }
        public List<SimplisityInfo> GetImageList()
        {
            return Info.GetList(ImageListName);
        }
        public ArticleImage AddImage(string uniqueName)
        {
            var articleImage = new ArticleImage(new SimplisityInfo(), "articleimage");
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            var portalContent = new PortalContentLimpet(PortalId, CultureCode);
            articleImage.RelPath = portalContent.ImageFolderRel.TrimEnd('/') + "/" + uniqueName;
            Info.AddListItem(ImageListName, articleImage.Info);
            Update();
            return articleImage;
        }
        public void UpdateImage(ArticleImage articleImage)
        {
            Info.RemoveListItem(ImageListName, "genxml/hidden/imagekey", articleImage.ImageKey);
            Info.AddListItem(ImageListName, articleImage.Info);
        }
        public ArticleImage GetImage(int idx)
        {
            return new ArticleImage(Info.GetListItem(ImageListName, idx), "articleimage");
        }
        public List<ArticleImage> GetImages()
        {
            var rtn = new List<ArticleImage>();
            foreach (var i in Info.GetList(ImageListName))
            {
                rtn.Add(new ArticleImage(i, "articleimage"));
            }
            return rtn;
        }
        #endregion

        #region "docs"
        public string DocumentListName { get { return "documentlist"; } }
        public void UpdateDocs(List<SimplisityInfo> docList)
        {
            Info.RemoveList(DocumentListName);
            foreach (var sInfo in docList)
            {
                var docData = new ArticleDoc(sInfo, "articledoc");
                UpdateDoc(docData);
            }
        }
        public List<SimplisityInfo> GetDocList()
        {
            return Info.GetList(DocumentListName);
        }
        public ArticleDoc AddDoc(string uniqueName)
        {
            var articleDoc = new ArticleDoc(new SimplisityInfo(), "articledoc");
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            var portalContent = new PortalContentLimpet(PortalId, CultureCode);
            articleDoc.RelPath = portalContent.DocFolderRel.TrimEnd('/') + "/" + uniqueName;
            articleDoc.Name = uniqueName;
            Info.AddListItem(DocumentListName, articleDoc.Info);
            Update();
            return articleDoc;
        }
        public void UpdateDoc(ArticleDoc articleDoc)
        {
            Info.RemoveListItem(DocumentListName, "genxml/hidden/dockey", articleDoc.DocKey);
            Info.AddListItem(DocumentListName, articleDoc.Info);
        }
        public ArticleDoc GetDoc(int idx)
        {
            return new ArticleDoc(Info.GetListItem(DocumentListName, idx), "articledoc");
        }
        public List<ArticleDoc> GetDocs()
        {
            var rtn = new List<ArticleDoc>();
            foreach (var i in Info.GetList(DocumentListName))
            {
                rtn.Add(new ArticleDoc(i, "articledoc"));
            }
            return rtn;
        }
        #endregion

        #region "links"
        public string LinkListName { get { return "linklist"; } }
        public void UpdateLinks(List<SimplisityInfo> linkList)
        {
            Info.RemoveList(LinkListName);
            foreach (var sInfo in linkList)
            {
                var linkData = new ArticleLink(sInfo, "articlelink");
                UpdateLink(linkData);
            }
        }
        public List<SimplisityInfo> GetLinkList()
        {
            return Info.GetList(LinkListName);
        }
        public ArticleLink AddLink()
        {
            var articleLink = new ArticleLink(new SimplisityInfo(), "articlelink");
            if (Info.ItemID < 0) Update(); // blank record, not on DB.  Create now.
            Info.AddListItem(LinkListName, articleLink.Info);
            Update();
            return articleLink;
        }
        public void UpdateLink(ArticleLink articleLink)
        {
            Info.RemoveListItem(LinkListName, "genxml/hidden/linkkey", articleLink.LinkKey);
            Info.AddListItem(LinkListName, articleLink.Info);
        }
        public ArticleLink Getlink(int idx)
        {
            return new ArticleLink(Info.GetListItem(LinkListName, idx), "articlelink");
        }
        public List<ArticleLink> Getlinks()
        {
            var rtn = new List<ArticleLink>();
            foreach (var i in Info.GetList(LinkListName))
            {
                rtn.Add(new ArticleLink(i, "articlelink"));
            }
            return rtn;
        }
        #endregion

        #region "properties"

        public string CultureCode { get; private set; }
        public string EntityTypeCode { get { return _entityTypeCode; } }
        public SimplisityInfo Info { get; set; }
        public int ModuleId { get { return Info.ModuleId; } set { Info.ModuleId = value; } }
        public int XrefItemId { get { return Info.XrefItemId; } set { Info.XrefItemId = value; } }
        public int ParentItemId { get { return Info.ParentItemId; } set { Info.ParentItemId = value; } }
        public int ArticleId { get { return Info.ItemID; } set { Info.ItemID = value; } }
        public string Ref { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public string GUIDKey { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public int SortOrder { get { return Info.SortOrder; } set { Info.SortOrder = value; } }
        public bool DebugMode { get; set; }
        public int PortalId { get; set; }
        public bool Exists { get {if (Info.ItemID  <= 0) { return false; } else { return true; }; } }
        public string Name { get { return Info.GetXmlProperty(NameXPath); } set { Info.SetXmlProperty(NameXPath, value); } }
        public string NameXPath { get { return "genxml/lang/genxml/textbox/articlename"; } }
        public bool IsHidden { get { return Info.GetXmlPropertyBool("genxml/checkbox/hidden"); } }
        public string AppThemeFolder { get { return Info.GetXmlProperty("genxml/config/appthemefolder"); } set { Info.SetXmlProperty("genxml/config/appthemefolder", value); } }
        public string AppThemeVersion { get { return Info.GetXmlProperty("genxml/config/appthemeversion"); } set { Info.SetXmlProperty("genxml/config/appthemeversion", value); } }
        public AppThemeLimpet AppTheme { get { return new AppThemeLimpet(AppThemeFolder, AppThemeVersion); } }

        #endregion

    }

}
