using DNNrocketAPI.Components;
using Newtonsoft.Json.Linq;
using Rocket.AppThemes.Components;
using RocketPortal.Components;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace RocketContent.Components
{
    public class DataObjectLimpet
    {
        private Dictionary<string, object> _dataObjects;
        public DataObjectLimpet(int portalid, string moduleRef, string cultureCode)
        {
            _dataObjects = new Dictionary<string, object>();
            SetDataObject("remotemodule", new RemoteModule(portalid, moduleRef));
            SetDataObject("appthemesystem", new AppThemeSystemLimpet(portalid, SystemKey));
            SetDataObject("portalcontent", new PortalContentLimpet(portalid, cultureCode));
            SetDataObject("portaldata", new PortalLimpet(portalid));
            SetDataObject("systemdata", new SystemLimpet(SystemKey));
            SetDataObject("appthemeprojects", new AppThemeProjectLimpet());
            SetDataObject("articledata", new ArticleLimpet(portalid, moduleRef, cultureCode));
        }
        public void SetDataObject(String key, object value)
        {
            if (_dataObjects.ContainsKey(key)) _dataObjects.Remove(key);
            _dataObjects.Add(key, value);
            if (key == "articledata")
            {
                // reload appthemedatalist
                var ad = (ArticleLimpet)value;
                SetDataObject("appthemedatalist", new AppThemeDataList(ad.ProjectName, SystemKey));
            }
        }
        public object GetDataObject(String key)
        {
            if (_dataObjects != null && _dataObjects.ContainsKey(key)) return _dataObjects[key];
            return null;
        }
        public List<SimplisityRecord> GetAppThemeProjects()
        {
            return AppThemeProjects.List;
        }
        public string SystemKey { get { return "rocketcontent"; } }
        public int PortalId { get { return PortalData.PortalId; } }
        public string AppThemeProjectName { get { return ArticleData.ProjectName; } }
        public Dictionary<string, object> DataObjects { get { return _dataObjects; } }
        public RemoteModule RemoteModule { get { return (RemoteModule)GetDataObject("remotemodule"); } }
        public AppThemeSystemLimpet AppThemeSystem { get { return (AppThemeSystemLimpet)GetDataObject("appthemesystem"); } }
        public PortalContentLimpet PortalContent { get { return (PortalContentLimpet)GetDataObject("portalcontent"); } }
        public AppThemeLimpet AppThemeView { get { return (AppThemeLimpet)GetDataObject("apptheme"); } set { _dataObjects.Add("apptheme", value); } }
        public AppThemeLimpet AppThemeAdmin { get { return (AppThemeLimpet)GetDataObject("appthemeadmin"); } set { _dataObjects.Add("appthemeadmin", value); } }
        public PortalLimpet PortalData { get { return (PortalLimpet)GetDataObject("portaldata"); } }
        public SystemLimpet SystemData { get { return (SystemLimpet)GetDataObject("systemdata"); } }
        public AppThemeProjectLimpet AppThemeProjects { get { return (AppThemeProjectLimpet)GetDataObject("appthemeprojects"); } }
        public ArticleLimpet ArticleData { get { return (ArticleLimpet)GetDataObject("articledata"); } }

    }
}
