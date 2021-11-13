using DNNrocketAPI;
using DNNrocketAPI.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RocketContent.Components
{
    public class PortalContentLimpet
    {
        private const string _tableName = "rocketcontent";
        private const string _systemkey = "rocketcontent";
        private DNNrocketController _objCtrl;
        private string _cacheKey;

        public PortalContentLimpet(int portalId, string cultureCode)
        {
            Record = new SimplisityRecord();
            Record.PortalId = portalId;

            if (cultureCode == "") cultureCode = DNNrocketUtils.GetEditCulture();

            _objCtrl = new DNNrocketController();

            _cacheKey = EntityTypeCode + portalId + "*" + cultureCode;

            Record = (SimplisityRecord)CacheUtils.GetCache(_cacheKey);
            if (Record == null)
            {
                Record = _objCtrl.GetRecordByType(portalId, -1, EntityTypeCode, "", "", _tableName);
                if (Record == null || Record.ItemID <= 0)
                {
                    Record = new SimplisityInfo();
                    Record.PortalId = portalId;
                    Record.ModuleId = -1;
                    Record.TypeCode = EntityTypeCode;
                    Record.Lang = cultureCode;
                }

                if (PortalUtils.PortalExists(portalId)) // check we have a portal, could be deleted
                {
                    // create folder on first load.
                    PortalUtils.CreateRocketDirectories(PortalId);

                    if (!Directory.Exists(PortalUtils.HomeDNNrocketDirectoryMapPath(PortalId))) Directory.CreateDirectory(PortalUtils.HomeDNNrocketDirectoryMapPath(PortalId));
                    ContentFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/rocketcontent";
                    ContentFolderMapPath = DNNrocketUtils.MapPath(ContentFolderRel);
                    if (!Directory.Exists(ContentFolderMapPath)) Directory.CreateDirectory(ContentFolderMapPath);

                    ImageFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/rocketcontent/images";
                    ImageFolderMapPath = DNNrocketUtils.MapPath(ImageFolderRel);
                    if (!Directory.Exists(ImageFolderMapPath)) Directory.CreateDirectory(ImageFolderMapPath);

                    DocFolderRel = PortalUtils.HomeDNNrocketDirectoryRel(PortalId).TrimEnd('/') + "/rocketcontent/docs";
                    DocFolderMapPath = DNNrocketUtils.MapPath(DocFolderRel);
                    if (!Directory.Exists(DocFolderMapPath)) Directory.CreateDirectory(DocFolderMapPath);

                }

            }
        }

        #region "Data Methods"
        public void Save(SimplisityInfo info)
        {
            Record.XMLData = info.XMLData;
            Update();
        }
        public void Update()
        {
            Record = _objCtrl.SaveRecord(Record, _tableName); // you must cache what comes back.  that is the copy of the DB.
            CacheUtils.SetCache(_cacheKey, Record);
        }
        public void Validate()
        {
            // check for existing page on portal for this system
            var tabid = PagesUtils.CreatePage(PortalId, _systemkey);
            PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Manager);
            PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.Editor);
            PagesUtils.AddPagePermissions(PortalId, tabid, DNNrocketRoles.ClientEditor);
            PagesUtils.AddPageSkin(PortalId, tabid, "rocketportal", "rocketadmin.ascx");
        }
        public void Delete()
        {
            _objCtrl.Delete(Record.ItemID, _tableName);

            // remove all portal records.
            var l = _objCtrl.GetList(PortalId, -1, "", "", "", "", 0, 0, 0, 0, _tableName);
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID, _tableName);
            }
            CacheUtils.RemoveCache(_cacheKey);
        }
        #endregion

        #region "Properties"
        public SimplisityInfo Info { get { return new SimplisityInfo(Record); } }
        public SimplisityRecord Record { get; set; }
        public int PortalId { get { return Record.PortalId; } }
        public string ContentFolderRel { get; set; }
        public string ContentFolderMapPath { get; set; }
        public string ImageFolderRel { get; set; }
        public string ImageFolderMapPath { get; set; }
        public string DocFolderRel { get; set; }
        public string DocFolderMapPath { get; set; }
        public bool Active { get { return Record.GetXmlPropertyBool("genxml/active"); } set { Record.SetXmlProperty("genxml/active", value.ToString()); } }
        public bool Valid { get { if (Record.GetXmlProperty("genxml/active") != "") return true; else return false; } }
        public string SystemKey { get { return "rocketcontent"; } }
        public string SecurityKey { get { return Record.GetXmlProperty("genxml/securitykey"); } }
        public string EntityTypeCode { get { return "PortalContent"; } }
        public bool DebugMode { get { return Record.GetXmlPropertyBool("genxml/debugmode"); } }
        public bool EmailOn { get { return Record.GetXmlPropertyBool("genxml/emailon"); } }

        #endregion

    }
}
