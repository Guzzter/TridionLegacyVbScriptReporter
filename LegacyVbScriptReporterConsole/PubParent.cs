using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegacyVbScriptReporterConsole
{
    public class PubParent
    {
        public string TcmId { get; set; }
        public string Title { get; set; }

        public string ParentTcmId { get; set; }
        public string ParentTitle { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(ParentTitle))
            {
                return string.Format("{0} ({1}) has NO parent", Title, TcmId, ParentTitle, ParentTcmId);
            }
            else
            {
                return string.Format("{0} ({1}) has parent {2} ({3})", Title, TcmId, ParentTitle, ParentTcmId);
            }
        }

    }
}
