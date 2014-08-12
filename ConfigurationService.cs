using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnBackup
{
    class ConfigurationService
    {
        private readonly Logger _logger;
        private readonly string _svnPath;
        private readonly string _repositoryPath;

        public ConfigurationService(Logger logger)
        {
            _logger = logger;
            _svnPath = ConfigurationManager.AppSettings["SvnPath"];
            _repositoryPath = ConfigurationManager.AppSettings["RepositoryPath"];
        }

        public ConfigurationResult GetConfiguration()
        {
            ConfigurationResult result = new ConfigurationResult();

            #region BackupDirectory

            result.BackupDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "backups"));

            if (!result.BackupDirectory.Exists)
            {
                _logger.Debug("Create backup folder '{0}'", result.BackupDirectory.FullName);
                result.BackupDirectory.Create();
            }

            #endregion

            #region SvnPath

            if (String.IsNullOrEmpty(_svnPath))
            {
                _logger.Error("'SvnPath' AppSetting isn't configured.");
                return result;
            }

            result.SvnDirectory = new DirectoryInfo(_svnPath);

            if (!result.SvnDirectory.Exists)
            {
                _logger.Error("Can't find SVN folder '{0}'", _svnPath);
                return result;
            }
            
            #endregion

            #region SvnAdminFile

            result.SvnAdminFile = new FileInfo(Path.Combine(result.SvnDirectory.FullName, "svnadmin.exe"));

            if (!result.SvnAdminFile.Exists)
            {
                _logger.Error("Can't find 'svnadmin.exe' in SvnPath '{0}'", _svnPath);
                return result;
            }

            #endregion

            #region RepositoryDirectory

            if (String.IsNullOrEmpty(_repositoryPath))
            {
                _logger.Error("'RepositoryPath' AppSetting isn't configured.");
                return result;
            }

            result.RepositoryDirectory = new DirectoryInfo(_repositoryPath);

            if (!result.RepositoryDirectory.Exists)
            {
                _logger.Error("Can't find repository folder '{0}'", result.RepositoryDirectory);
                return result;
            }

            #endregion

            #region DropBox

            result.DropBoxApiKey = ConfigurationManager.AppSettings["DropBox.ApiKey"];
            result.DropBoxAppSecret = ConfigurationManager.AppSettings["DropBox.AppSecret"];
            result.DropBoxUserToken = ConfigurationManager.AppSettings["DropBox.UserToken"];
            result.DropBoxUserSecret = ConfigurationManager.AppSettings["DropBox.UserSecret"];

            #endregion

            result.IsValid = true;

            return result;
        }
    }
}
