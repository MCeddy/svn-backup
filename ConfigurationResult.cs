using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnBackup
{
    class ConfigurationResult
    {
        public bool IsValid { get; set; }
        public DirectoryInfo BackupDirectory { get; set; }
        public DirectoryInfo SvnDirectory { get; set; }
        public FileInfo SvnAdminFile { get; set; }
        public FileInfo ZipAppFile { get; set; }
        public DirectoryInfo RepositoryDirectory { get; set; }
        public string DropBoxApiKey { get; set; }
        public string DropBoxAppSecret { get; set; }
        public string DropBoxUserToken { get; set; }
        public string DropBoxUserSecret { get; set; }
    }
}
