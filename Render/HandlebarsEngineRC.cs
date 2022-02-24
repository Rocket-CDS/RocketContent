using DNNrocketAPI.Components;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace RocketContent.Components
{
    public class HandlebarsEngineRC : HandlebarsEngine
    {
        public string ExecuteRC(string source, object model)
        {
            try
            {
                var hbs = HandlebarsDotNet.Handlebars.Create();
                RegisterHelpers(hbs);
                RegisterRCHelpers(hbs);
                return CompileTemplate(hbs, source, model);
            }
            catch (Exception ex)
            {
                LogUtils.LogException(ex);
                throw new TemplateException("Failed to render Handlebar template : " + ex.Message, ex, model, source);
            }
        }

        public static void RegisterRCHelpers(IHandlebars hbs)
        {
            RegisterArticle(hbs);
            RegisterRow(hbs);
            RegisterModuleRef(hbs);
            RegisterImage(hbs);
        }
        /// <summary>
        /// Get moduleRef
        /// {{moduleref @root}}
        /// </summary>
        /// <param name="hbs"></param>
        private static void RegisterModuleRef(IHandlebars hbs)
        {
            hbs.RegisterHelper("moduleref", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];
                    dataValue = (string)o.SelectToken("genxml.data.genxml.column.guidkey") ?? "";
                }
                writer.WriteSafeString(dataValue);
            });
        }
        /// <summary>
        /// Get Image Data
        /// 
        /// "alt" - {{image @root "alt" @../index @index}}
        /// 
        /// "count" - {{image @root "count" @../index }}
        /// "thumburl" - Usually used within 2 loops, row and image.
        /// {{image @root "thumburl" @../index @index 640 200}}
        /// </summary>
        /// <param name="hbs">image @root cmd rowidx imageidx</param>
        private static void RegisterImage(IHandlebars hbs)
        {
            hbs.RegisterHelper("image", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length >= 2)
                {
                    var o = (JObject)arguments[0];
                    var moduleref = (string)o.SelectToken("genxml.data.genxml.column.guidkey") ?? "";
                    var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";
                    var enginrUrl = (string)o.SelectToken("genxml.sessionparams.r.engineurl") ?? "";

                    var cacheKey = moduleref + "*" + cultureCode;
                    var articleData = (ArticleLimpet)CacheUtils.GetCache(cacheKey, "article");
                    if (articleData == null)
                    {
                        articleData = new ArticleLimpet(-1, moduleref, cultureCode);
                        CacheUtils.SetCache(cacheKey, articleData, "article");
                    }

                    var rowidx = 0;
                    if (arguments.Length >= 3) rowidx = (int)arguments[2];
                    var imgidx = 0;
                    if (arguments.Length >= 4) imgidx = (int)arguments[3];
                    var img = articleData.GetRow(rowidx).GetImage(imgidx);

                    switch (arguments[1].ToString())
                    {
                        case "alt":
                            dataValue = img.Alt;
                            break;
                        case "relpath":
                            dataValue = img.RelPath;
                            break;
                        case "height":
                            dataValue = img.Height.ToString();
                            break;
                        case "width":
                            dataValue = img.Width.ToString();
                            break;
                        case "count":
                            dataValue = articleData.GetRow(rowidx).GetImageList().Count.ToString();
                            break;
                        case "summary":
                            dataValue = img.Summary;
                            break;
                        case "url":
                            dataValue = img.Url;
                            break;
                        case "urltext":
                            dataValue = img.UrlText;
                            break;
                        case "thumburl":
                            var width = Convert.ToInt32(arguments[4]);
                            var height = Convert.ToInt32(arguments[5]);
                            dataValue = enginrUrl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + img.RelPath + "&w=" + width + "&h=" + height;
                            break;

                    }
               }

                writer.WriteSafeString(dataValue);
            });
        }

        private static void RegisterRow(IHandlebars hbs)
        {
            hbs.RegisterHelper("articlerow", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments.Length == 4)
                {
                    var o = (JObject)arguments[0];
                    var moduleref = (string)o.SelectToken("genxml.data.genxml.column.guidkey") ?? "";
                    var cultureCode = (string)o.SelectToken("genxml.sessionparams.r.culturecode") ?? "";

                    var cacheKey = moduleref + "*" + cultureCode;
                    var articleData = (ArticleLimpet)CacheUtils.GetCache(cacheKey, "article");
                    if (articleData == null)
                    {
                        articleData = new ArticleLimpet(-1, moduleref, cultureCode);
                        CacheUtils.SetCache(cacheKey, articleData, "article");
                    }

                    var rowKey = (string)arguments[2];
                    var imgidx = (int)arguments[3];
                    var img = articleData.GetRow(rowKey).GetImage(imgidx);

                    switch (arguments[1].ToString())
                    {
                        case "alt":
                            dataValue = img.Alt;
                            break;
                        case "imagealt":
                            dataValue = img.Alt;
                            break;

                    }

                    dataValue = img.RelPath;
                    

                }

                writer.WriteSafeString(dataValue);
            });
        }


        private static void RegisterArticle(IHandlebars hbs)
        {
            hbs.RegisterHelper("article", (writer, context, arguments) =>
            {
                var dataValue = "";
                if (arguments[0] != null)
                {
                    var o = (JObject)arguments[0];

                    switch (arguments[1].ToString())
                    {
                        case "ref":
                            dataValue = (string)o.SelectToken("genxml.data.genxml.textbox.articleref") ?? "";
                            break;
                        case "name":
                            dataValue = (string)o.SelectToken("genxml.data.genxml.lang.genxml.textbox.articlename") ?? "";
                            break;
                        case "summary":
                            dataValue = System.Web.HttpUtility.HtmlEncode((string)o.SelectToken("genxml.data.genxml.lang.genxml.textbox.articlesummary"));
                            if (dataValue == null) dataValue = "";
                            dataValue = dataValue.Replace(Environment.NewLine, "<br/>");
                            dataValue = dataValue.Replace("\n", "<br/>");
                            dataValue = dataValue.Replace("\t", "&nbsp;&nbsp;&nbsp;");
                            dataValue = dataValue.Replace("'", "&apos;");
                            break;
                        case "imagealt":
                            var fieldname2 = "imagealt";
                            if (arguments.Length >= 4 && arguments[3] != null) fieldname2 = arguments[3].ToString();
                            if (!fieldname2.Contains(".")) fieldname2 = "textbox." + fieldname2;
                            var nod = o.SelectToken("genxml.data.genxml.lang.genxml.imagelist.genxml.[" + arguments[2].ToString() + "]." + fieldname2);
                            if (nod != null)
                            {
                                dataValue = nod.ToString();
                            }
                            break;
                        case "imageurl":
                            var fieldname1 = "imagepatharticleimage";
                            if (arguments.Length >= 6 && arguments[5] != null) fieldname1 = arguments[5].ToString();
                            if (!fieldname1.Contains(".")) fieldname1 = "hidden." + fieldname1;
                            var nod1 = o.SelectToken("genxml.data.genxml.imagelist.genxml.[" + arguments[2].ToString() + "]." + fieldname1);
                            if (nod1 != null)
                            {
                                var w = arguments[3].ToString();
                                var h = arguments[4].ToString();
                                var iurl = nod1.ToString();
                                var eurl = o.SelectToken("genxml.sessionparams.r.engineurl").ToString();
                                if (eurl != null) dataValue = eurl.TrimEnd('/') + "/DesktopModules/DNNrocket/API/DNNrocketThumb.ashx?src=" + iurl + "&w=" + w + "&h=" + h;
                            }
                            break;
                        case "linkurl":
                            var fieldname = "externallinkarticlelink";
                            if (arguments.Length >= 4 && arguments[3] != null) fieldname = arguments[3].ToString();
                            if (!fieldname.Contains(".")) fieldname = "textbox." + fieldname;
                            var nodLinkUrl = o.SelectToken("genxml.data.genxml.lang.genxml.linklist.genxml.[" + arguments[2].ToString() + "]." + fieldname);
                            if (nodLinkUrl != null && nodLinkUrl.ToString() != "")
                            {
                                dataValue = nodLinkUrl.ToString();

                                var anchorName = "anchorarticlelink";
                                if (arguments.Length >= 5 && arguments[4] != null) anchorName = arguments[4].ToString();
                                if (anchorName != "")
                                {
                                    if (!anchorName.Contains(".")) anchorName = "textbox." + anchorName;
                                    var nodLinkAnchor = o.SelectToken("genxml.data.genxml.lang.genxml.linklist.genxml.[" + arguments[2].ToString() + "]." + anchorName);
                                    if (nodLinkAnchor != null)
                                    {
                                        var aurl = nodLinkAnchor.ToString();
                                        if (aurl != "") dataValue += "#" + aurl;
                                    }
                                }
                            }
                            break;
                        case "linkname":
                            var fieldname3 = "namearticlelink";
                            if (arguments.Length >= 4 && arguments[3] != null) fieldname3 = arguments[3].ToString();
                            if (!fieldname3.Contains(".")) fieldname3 = "textbox." + fieldname3;
                            var nodLinkName = o.SelectToken("genxml.data.genxml.lang.genxml.linklist.genxml.[" + arguments[2].ToString() + "]." + fieldname3);
                            if (nodLinkName != null)
                            {
                                dataValue = nodLinkName.ToString();
                            }
                            break;
                        case "docurl":
                            var docpath = "documentpatharticledoc";
                            if (arguments.Length >= 4 && arguments[3] != null) docpath = arguments[3].ToString();
                            if (!docpath.Contains(".")) docpath = "hidden." + docpath;
                            var nodDocUrl = o.SelectToken("genxml.data.genxml.documentlist.genxml.[" + arguments[2].ToString() + "]." + docpath);
                            if (nodDocUrl != null && nodDocUrl.ToString() != "")
                            {
                                var eurl = o.SelectToken("genxml.sessionparams.r.engineurl").ToString();
                                if (eurl != null) dataValue = eurl.TrimEnd('/') + "/" + nodDocUrl.ToString();
                            }
                            break;
                        case "docname":
                            var docname = "documentnamearticledoc";
                            if (arguments.Length >= 4 && arguments[3] != null) docname = arguments[3].ToString();
                            if (!docname.Contains(".")) docname = "textbox." + docname;
                            var noddocname = o.SelectToken("genxml.data.genxml.documentlist.genxml.[" + arguments[2].ToString() + "]." + docname);
                            if (noddocname != null)
                            {
                                dataValue = noddocname.ToString();
                            }
                            break;
                    }
                }

                writer.WriteSafeString(dataValue);
            });
        }

    }
}
