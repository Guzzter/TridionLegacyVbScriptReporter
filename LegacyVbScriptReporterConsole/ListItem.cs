using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tridion.ContentManager.CoreService.Client;
using Tridion.ContentManager;
using System.Xml.Linq;

namespace LegacyVbScriptReporterConsole
{
    public class ListItem
    {
        public string TcmId { get; set; }
        public string Title { get; set; }
        public string ItemTypeId { get; set; }
        public Tridion.ContentManager.CoreService.Client.ItemType ItemTypeName { get; set; }
        public string WebDavPath { get; set; }
        public string Icon { get; set; }
        public string ParentTcmId { get; set; }
        public string PublicationTitle { get; set; }

        //<tcm:Item ID="tcm:17-384-64" Title="Some Page Title" Type="64" OrgItemID="tcm:17-107-4" Path="\040 Web Publication\Root\030 - Work" Icon="T64L1P0" Publication="040 Web Publication"></tcm:Item>
        public ListItem(System.Xml.Linq.XElement node)
        {
            TcmId = node.Attribute("ID").SafeStringVal();
            Title = node.Attribute("Title").SafeStringVal();
            ItemTypeId = node.Attribute("Type").SafeStringVal();
            WebDavPath = node.Attribute("Path").SafeStringVal();
            Icon = node.Attribute("Icon").SafeStringVal();
            ParentTcmId = node.Attribute("OrgItemID").SafeStringVal();
            PublicationTitle = node.Attribute("Publication").SafeStringVal();
            ItemTypeName = EnumHelper<Tridion.ContentManager.CoreService.Client.ItemType>.Parse(ItemTypeId, true);
        
        }

        public override string ToString()
        {
            return string.Format("{0} with title {1} ({2})", ItemTypeName, Title, TcmId);
        }

    }

    public static class NodeHelp 
    {
        public static string SafeStringVal(this XAttribute attr, string defaultval = "")
        {
            return attr == null ? defaultval : attr.Value;
        }

    }



    
}
