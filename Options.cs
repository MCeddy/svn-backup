using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnBackup
{
    class Options
    {
        [OptionArray('r', "repositories", DefaultValue = null, HelpText = "list of repos to backup", Required = false)]
        public string[] Repositories { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("SVN Backup", "0.1"),
                Copyright = new CopyrightInfo("René Schimmelpfennig", 2013),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("open source");
            help.AddPreOptionsLine("Usage: app -pSomeone");
            help.AddOptions(this);
            return help;
        }
    }
}
