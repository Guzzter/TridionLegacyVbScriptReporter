using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace LegacyVbScriptReporterConsole
{
    public class List
    {
        public List<ListItem> Items { get; set; }
        public List(IEnumerable<XNode> searchResult)
        {
            Items = new List<ListItem>();

            foreach (System.Xml.Linq.XElement node in searchResult)
            {
                Items.Add(new ListItem(node));
            }

        }

        public List(System.Collections.IEnumerator nodes)
        {
            Items = new List<ListItem>();

            while (nodes.MoveNext())
            {
                Items.Add(new ListItem(nodes.Current as XElement));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ListItem i in Items)
            {
                sb.AppendLine(i.ToString());
            }
            return sb.ToString();
        }

    }
}
