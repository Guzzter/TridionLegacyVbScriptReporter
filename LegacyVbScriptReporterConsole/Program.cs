using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.CoreService.Client;
using System.Xml;
using LegacyVbScriptReporterConsole.CoreService;
using System.IO;
using System.Xml.Linq;
using Tridion.ContentManager;

namespace LegacyVbScriptReporterConsole
{
    class Program
    {
        private const string configFilename = "CoreServiceHandler.config";
        private const string reportFile = "vbscriptreport.txt";
        private static CoreServiceHandler coreServiceHandler;
        private static CoreServiceClient core;

        private static Tridion.ContentManager.CoreService.Client.ReadOptions readOpts = new Tridion.ContentManager.CoreService.Client.ReadOptions();
        private static Dictionary<string, string> pages = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            coreServiceHandler = new CoreServiceHandler(configFilename);
            core = coreServiceHandler.GetNewNetTcpClient();
            if (core == null)
            {
                Console.WriteLine("Could not login, please check config for credentials");
                return;
            }
            else
            {
                Console.WriteLine("Start scan with Tridion for VBScript legacy code");
            }

            //https://code.google.com/p/tridion-practice/wiki/LINQQueries
            
            var ppList = GetPublicationAndTheirParents();
            StringBuilder sbReport = new StringBuilder();
            bool backupVbScriptCodeToFile = coreServiceHandler.GetAppSetting("saveTemplateCode").Equals("true", StringComparison.OrdinalIgnoreCase);
            int ptAllCount = 0;
            int ctAllCount = 0;

            foreach (var publication in ppList)
            {
                int ptCount = 0;
                int ctCount = 0;
                Console.WriteLine(Environment.NewLine + "Scanning publication " + publication.Title);
                sbReport.Append(Environment.NewLine + "Scanning publication " + publication.Title + Environment.NewLine);

                var listOfPt = GetListOfType(publication.TcmId, ItemType.PageTemplate);
                var pubID = GetPublicationId(publication.TcmId);
                foreach (var ptitem in listOfPt.Items)
                {
                    var ptd = core.Read(ptitem.TcmId, readOpts) as PageTemplateData;
                    if (ptd != null && ptd.TemplateType != "CompoundTemplate")
                    {
                        ptCount++;
                        var templateId = GetPublicationId(ptd.Id);

                        // only report local templates
                        if (pubID == templateId)
                        {
                            int pageCount = 0;
                            string pages = GetPagesUsedByAPageTemplate(ptd.Id, out pageCount);
                            if (pageCount > 0) {
                                sbReport.AppendFormat("{0} ({1}) is {2} used in {3} pages:", ptd.LocationInfo.WebDavUrl, ptd.Id, ptd.TemplateType, pageCount).Append(Environment.NewLine);
                                sbReport.AppendLine(pages);
                                sbReport.AppendFormat("----------------------------").Append(Environment.NewLine);
                            }else {
                                sbReport.AppendFormat("{0} ({1}) is {2} is not used in pages", ptd.LocationInfo.WebDavUrl, ptd.Id, ptd.TemplateType).Append(Environment.NewLine);
                            }
                            if (backupVbScriptCodeToFile)
                            {
                                File.WriteAllText(ptd.Title + ".tpts.txt", ptd.Content);
                            }
                        }

                        
                    }
                    Console.Write(".");
                }

                var listOfCt = GetListOfType(publication.TcmId, ItemType.ComponentTemplate);
                foreach (var ctitem in listOfCt.Items)
                {
                    var ctd = core.Read(ctitem.TcmId, readOpts) as ComponentTemplateData;

                    if (ctd != null && ctd.TemplateType != "CompoundTemplate")
                    {
                        ctCount++;
                        var templateId = GetPublicationId(ctd.Id);
                        int pageCount = 0;
                        string pages = GetPagesUsedByAPageTemplate(ctd.Id, out pageCount);

                        // only report local templates
                        if (pubID == templateId)
                        {
                            if (pageCount > 0)
                            {
                                sbReport.AppendFormat("{0} ({1}) is {2} used in {3} pages:", ctd.LocationInfo.WebDavUrl, ctd.Id, ctd.TemplateType, pageCount).Append(Environment.NewLine);
                                sbReport.AppendLine(pages);
                                sbReport.AppendFormat("----------------------------").Append(Environment.NewLine);
                            }
                            else
                            {
                                sbReport.AppendFormat("{0} ({1}) is {2} is not used in pages", ctd.LocationInfo.WebDavUrl, ctd.Id, ctd.TemplateType).Append(Environment.NewLine);
                            }
                        }
                        if (backupVbScriptCodeToFile)
                        {
                            File.WriteAllText(ctd.Title + ".tcts.txt", ctd.Content);
                        }

                    }
                    Console.Write(".");
                }

                ctAllCount += ctCount;
                ptAllCount += ptCount;

                sbReport.AppendFormat("Found {0} PT's and {1} CT's with VBScript in publication '{2}'" + Environment.NewLine, ptCount, ctCount, publication.Title);

            }

            sbReport.AppendFormat(Environment.NewLine + "Found {0} PT's and {1} CT's with VBScript in all publications" + Environment.NewLine, ptAllCount, ctAllCount);

            Console.WriteLine("");
            Console.WriteLine(sbReport.ToString());
            Console.WriteLine("--------------------------");

            
            File.WriteAllText(reportFile, sbReport.ToString());
            
            Console.WriteLine("Saved report to " + reportFile);
            Console.WriteLine("Done..");
            Console.ReadLine();
        }

        private static int GetPublicationId(string tcmId)
        {
            int pubId = 0;
            string nr = tcmId.Split(new[] { '-', ':' })[1];
            if (nr == "0")
                nr = tcmId.Split(new[] { '-', ':' })[2];
            if (int.TryParse(nr, out pubId))
            {
                return pubId;
            }
            return -1;
        }

        private static string GetPagesUsedByAPageTemplate(string pageTemplateTcmId, out int pagesFound)
        {
            XElement pages = core.GetListXml(pageTemplateTcmId, new UsingItemsFilterData { ItemTypes = new[] { ItemType.Page } });

            StringBuilder sbPages = new StringBuilder();
            foreach (var page in pages.Elements())
            {
                sbPages.AppendFormat("- Page '{0}' with ID {1}", page.Attribute("Title").Value, page.Attribute("ID").Value).Append(Environment.NewLine);
            }
            pagesFound = pages.Elements().Count();
            return sbPages.ToString();
        }

        //http://codedweapon.com/2012/12/getting-using-and-used-items-with-core-services/
        private static List GetListOfType(string publicationTcmId, Tridion.ContentManager.CoreService.Client.ItemType searchForType)
        {
            RepositoryItemsFilterData filter = new RepositoryItemsFilterData
            {
                ItemTypes = new[] { searchForType },
                Recursive = true
            };

            IEnumerable<XNode> searchResult = core.GetListXml(publicationTcmId, filter).Nodes();
            var list = new List(searchResult);

            return list;
        }


        private static IEnumerable<PubParent> GetPublicationAndTheirParents()
        {
            IEnumerable<PubParent> ppList = core.GetSystemWideListXml(new PublicationsFilterData())
                .Elements()
                .Select(elm => (PublicationData)core.Read(elm.Attribute("ID").Value, null))
                .Select(pub => new PubParent
                {
                    Title = pub.Title,
                    TcmId = pub.Id,
                    ParentTitle = pub.Parents.Select(parent => parent.Title).FirstOrDefault(),
                    ParentTcmId = pub.Parents.Select(parent => parent.IdRef.ToString()).FirstOrDefault()
                });

            return ppList;

        }
        
        private static void GetRelatedPages(string componentUri, int level)
        {
            //Only collect pages that have publishdate before 15 april 2015
            DateTime dt = new DateTime(2015, 4, 15);

            UsingItemsFilterData filter = new UsingItemsFilterData
            {
                ItemTypes = new[] { Tridion.ContentManager.CoreService.Client.ItemType.Component, Tridion.ContentManager.CoreService.Client.ItemType.Page },
                IncludedVersions = VersionCondition.OnlyLatestVersions
            };
            foreach (System.Xml.Linq.XElement node in core.GetListXml(componentUri, filter).Nodes())
            {
                string nodeId = node.Attribute("ID").Value;
                string path = node.Attribute("Path").Value;
                int type = int.Parse(node.Attribute("Type").Value);

                if (type == 64)
                {
                    //Is a page
                    if (!pages.Keys.Contains(nodeId))
                    {
                        //Get publish info of last publish action
                        var pubInfo = core.GetListPublishInfo(nodeId).LastOrDefault();
                        if (pubInfo != null && pubInfo.PublishedAt < dt)
                        {
                            var page = core.Read(nodeId, readOpts) as PageData;
                            var locInfo = ((PublishLocationInfo)page.LocationInfo);
                            string url = locInfo.PublishLocationUrl;
                            pages.Add(nodeId, url);
                            //publish
                            var liveTarget = new[] { "tcm:0-141-65538" };
                            //core.Publish(new[] { nodeId }, new PublishInstructionData { ResolveInstruction = new ResolveInstructionData() { IncludeChildPublications = true }, RenderInstruction = new RenderInstructionData() }, liveTarget, PublishPriority.Low, null);

                        }
                        //ignore when page is never been published
                    }

                }
                else if (level < 3)
                {
                    GetRelatedPages(nodeId, level + 1);
                }

            }
        }

        private static void WriteToHtmlFile(string prefixUrl)
        {
            var listurls = pages.OrderBy(row => row.Value).Select(row => row.Value);
            
            StringBuilder sb = new StringBuilder("<html><body><ol>");
            foreach (var page in listurls)
            {
                var url = prefixUrl + page;
                sb.AppendFormat("<li><a href=\"{0}\" target=\"_blank\">{1}</a></li>\r\n", url, url);
            }
            sb.AppendLine("</ol></body></html>");

            File.WriteAllText("pages_with_videos.html", sb.ToString(), Encoding.UTF8);
        }

    }


}
