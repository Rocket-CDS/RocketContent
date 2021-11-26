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
using System.Xml.XPath;
using System.Xml.Linq;

namespace RocketContent.Components
{
    public class ArticleLimpet
    {
        public const string _tableName = "RocketContent";
        public const string _entityTypeCode = "ART";
        private DNNrocketController _objCtrl;
        private int _articleId;

        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="dataRef"></param>
        /// <param name="langRequired"></param>
        public ArticleLimpet(int portalId, string dataRef, string langRequired)
        {
            PortalId = portalId;
            Info = new SimplisityInfo();
            Info.ItemID = -1;
            Info.TypeCode = _entityTypeCode;
            Info.ModuleId = -1;
            Info.UserId = -1;
            Info.GUIDKey = dataRef;
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

            var info = _objCtrl.GetByGuidKey(PortalId, -1, _entityTypeCode, DataRef, "", _tableName, cultureCode);
            if (info != null && info.ItemID > 0) Info = info; // check if we have a real record, or a dummy being created and not saved yet.
            Info.Lang = CultureCode;
            PortalId = Info.PortalId;
            if (DataRef == "") DataRef = GeneralUtils.GetGuidKey();

            if (GetRowList().Count  == 0) AddRow(); // create first row automatically
        }
        public void Delete()
        {
            _objCtrl.Delete(Info.ItemID, _tableName);
        }

        private SimplisityInfo ReplaceInfoFields(SimplisityInfo newInfo, SimplisityInfo postInfo, string xpathListSelect)
        {
            var textList = postInfo.XMLDoc.SelectNodes(xpathListSelect);
            if (textList != null)
            {
                foreach (XmlNode nod in textList)
                {
                    newInfo.SetXmlProperty(xpathListSelect.Replace("*", "") + nod.Name, nod.InnerText);
                }
            }
            return newInfo;
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

        #region "rows"
        public void UpdateRow(string rowKey, SimplisityInfo postInfo)
        {
            var newArticleRows = new List<SimplisityInfo>();
            var articleRows = GetRowList();
            foreach (var sInfo in articleRows)
            {                
                if (sInfo.GetXmlProperty("genxml/config/rowkey") == rowKey)
                {
                    var newInfo = ReplaceInfoFields(new SimplisityInfo(), postInfo, "genxml/textbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/textbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/checkbox/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/select/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/radio/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/config/*");
                    newInfo = ReplaceInfoFields(newInfo, postInfo, "genxml/lang/genxml/config/*");

                    var imgList = postInfo.GetList(ImageListName);
                    foreach (var img in imgList)
                    {
                        newInfo.AddListItem(ImageListName, img);
                    }
                    var docList = postInfo.GetList(DocumentListName);
                    foreach (var doc in docList)
                    {
                        newInfo.AddListItem(DocumentListName, doc);
                    }
                    var linkList = postInfo.GetList(LinkListName);
                    foreach (var link in linkList)
                    {
                        newInfo.AddListItem(LinkListName, link);
                    }

                    newArticleRows.Add(newInfo);
                }
                else
                {
                    newArticleRows.Add(sInfo);
                }
            }
            Info.RemoveList("rows");
            foreach (var sInfo in newArticleRows)
            {
                Info.AddListItem("rows", sInfo);
            }
            Update();
        }
        public string AddRow()
        {
            var newInfo = new SimplisityInfo();
            var rowKey = GeneralUtils.GetGuidKey();
            newInfo.SetXmlProperty("genxml/config/rowkey", rowKey);
            newInfo.SetXmlProperty("genxml/lang/genxml/config/rowkeylang", rowKey);
            Info.AddListItem("rows", newInfo);
            Update();
            return rowKey;
        }
        public void SortRows(string keylistcsv)
        {
            var rowSortOrder = new List<string>();
            var l = keylistcsv.Split(',');
            foreach (var rKey in l)
            {
                if (rKey != "") rowSortOrder.Add(rKey);
            }

            var rowLangList = _objCtrl.GetList(PortalId, -1, _entityTypeCode + "LANG", " and R1.ParentItemId = " + ArticleId + " ", "", "", 0, 0, 0, 0, _tableName);
            foreach (var r in rowLangList)
            {
                var rRec = new SimplisityRecord(r);
                var rowList = rRec.GetRecordList("rows");
                var rowDict = new Dictionary<string, SimplisityRecord>();
                foreach (var rowData in rowList)
                {
                    rowDict.Add(rowData.GetXmlProperty("genxml/config/rowkeylang"), rowData);
                }
                rRec.RemoveRecordList("rows");
                foreach (var k in rowSortOrder)
                {
                    if (rowDict.ContainsKey(k)) rRec.AddRecordListItem("rows", rowDict[k]);
                }
                _objCtrl.Update(rRec, _tableName);
            }
            var sRec1 = new SimplisityRecord(Info);
            var rowList1 = sRec1.GetRecordList("rows");
            var rowDict1 = new Dictionary<string, SimplisityRecord>();
            foreach (var rowData in rowList1)
            {
                rowDict1.Add(rowData.GetXmlProperty("genxml/config/rowkey"), rowData);
            }
            sRec1.RemoveRecordList("rows");
            foreach (var k in rowSortOrder)
            {
                sRec1.AddRecordListItem("rows", rowDict1[k]);
            }
            _objCtrl.Update(sRec1, _tableName);
            _objCtrl.RebuildLangIndex(PortalId, ArticleId, _tableName); // rebuild the index record [Essential to get the correct sort order] 

            Populate(CultureCode);
        }
        public void RemoveRow(string rowKey)
        {
            var rowLangList = _objCtrl.GetList(PortalId, -1, _entityTypeCode + "LANG", " and R1.ParentItemId = " + ArticleId + " ", "", "", 0, 0, 0, 0, _tableName);
            foreach (var r in rowLangList)
            {
                var rRec = new SimplisityRecord(r);
                rRec.RemoveRecordListItem("rows", "genxml/config/rowkeylang", rowKey);
                _objCtrl.Update(rRec, _tableName);
            }
            Info.RemoveListItem("rows", "genxml/config/rowkey", rowKey);
            Update();
        }
        public ArticleRowLimpet GetRow(string rowKey)
        {
            var articleRow = Info.GetListItem("rows", "genxml/config/rowkey", rowKey);
            if (articleRow == null) return null;
            return new ArticleRowLimpet(ArticleId, articleRow.XMLData);
        }
        public ArticleRowLimpet GetRow(int idx)
        {
            var articleRow = Info.GetListItem("rows", idx);
            if (articleRow == null) return null;
            return new ArticleRowLimpet(ArticleId, articleRow.XMLData);
        }
        public List<SimplisityInfo> GetRowList()
        {
            return Info.GetList("rows");
        }
        public List<ArticleRowLimpet> GetRows()
        {
            var rtn = new List<ArticleRowLimpet>();
            foreach (var i in Info.GetList("rows"))
            {
                rtn.Add(new ArticleRowLimpet(ArticleId, i.XMLData));
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
        public string DataRef { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public string GUIDKey { get { return Info.GUIDKey; } set { Info.GUIDKey = value; } }
        public int SortOrder { get { return Info.SortOrder; } set { Info.SortOrder = value; } }
        public bool DebugMode { get; set; }
        public int PortalId { get; set; }
        public bool Exists { get {if (Info.ItemID  <= 0) { return false; } else { return true; }; } }
        public string LinkListName { get { return "linklist"; } }
        public string DocumentListName { get { return "documentlist"; } }
        public string ImageListName { get { return "imagelist"; } }
        public string Name { get { return Info.GetXmlProperty("genxml/textbox/name"); } set { Info.SetXmlProperty("genxml/textbox/name", value); } }
        public string AdminAppThemeFolder { get { return Info.GetXmlProperty("genxml/textbox/adminappthemefolder"); } set { Info.SetXmlProperty("genxml/textbox/adminappthemefolder", value); } }
        public string AdminAppThemeFolderVersion { get { return Info.GetXmlProperty("genxml/textbox/adminappthemefolderversion"); } set { Info.SetXmlProperty("genxml/textbox/adminappthemefolderversion", value); } }

        #endregion

    }

}
