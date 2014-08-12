using DropNet;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using SvnBackup.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var options = new Options();
            var stopWatch = new Stopwatch();

            logger.Info("start");

            stopWatch.Start();

            if (CommandLine.Parser.Default.ParseArguments(args, options)) //parse command line
            {
                var configService = new ConfigurationService(logger);
                var config = configService.GetConfiguration();

                if (!config.IsValid)
                {
                    Environment.Exit(0);
                }

                var backupDirectoryFiles = config.BackupDirectory.GetFiles();
                if (backupDirectoryFiles.Any())
                {
                    //archive old files
                    DirectoryInfo archiveDirectory = new DirectoryInfo(Path.Combine(config.BackupDirectory.FullName, "archive"));
                    if (!archiveDirectory.Exists)
                    {
                        archiveDirectory.Create();
                    }

                    foreach (var file in backupDirectoryFiles)
                    {
                        file.MoveTo(Path.Combine(archiveDirectory.FullName, file.Name));
                    }
                }

                foreach (var repository in config.RepositoryDirectory.GetDirectories())
                {
                    logger.Info("found repository '{0}'", repository.Name);

                    //create repository dump
                    logger.Info("create dump..");

                    string backupFileName = String.Format("svn_backup_{0:yyyyMMdd_HHmmss}_{1}.dump", DateTime.Now, repository.Name);
                    FileInfo backupFile = new FileInfo(Path.Combine(config.BackupDirectory.FullName, backupFileName));
                    string svnAdminArgs = String.Format("dump {0}", repository.FullName);

                    using (Process svnAdminProcess = new Process())
                    {
                        svnAdminProcess.StartInfo = new ProcessStartInfo()
                        {
                            FileName = config.SvnAdminFile.FullName,
                            Arguments = svnAdminArgs,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false
                        };
                        svnAdminProcess.Start();
                        svnAdminProcess.StandardOutput.BaseStream.ToFile(backupFile.FullName);
                    }

                    //compress
                    logger.Info("compress file..");

                    FileInfo zipFile = new FileInfo(Path.ChangeExtension(backupFile.FullName, "zip"));

                    using (var fs = zipFile.Create())
                    {
                        using (var zipStream = new ZipOutputStream(fs))
                        {
                            zipStream.SetLevel(9); //highest compression

                            ZipEntry zipEntry = new ZipEntry(ZipEntry.CleanName(backupFile.Name));
                            zipEntry.DateTime = backupFile.CreationTime;

                            zipStream.PutNextEntry(zipEntry);

                            using (var sourceFileStream = backupFile.OpenRead())
                            {
                                StreamUtils.Copy(sourceFileStream, zipStream, new byte[4096]);
                            }

                            zipStream.CloseEntry();
                        }
                    }

                    //remove uncompressed file
                    logger.Debug("remove backup file '{0}'", backupFile.Name);
                    backupFile.Delete();
                }
                
                if (!String.IsNullOrEmpty(config.DropBoxApiKey))
                {
                    //DropBox upload
                    var dropBoxClient = new DropNetClient(config.DropBoxApiKey, config.DropBoxAppSecret, config.DropBoxUserToken, config.DropBoxUserSecret);
                    dropBoxClient.UseSandbox = true;

                    logger.Info("clear files from dropbox app folder");

                    var metadata = dropBoxClient.GetMetaData();

                    if ((metadata != null) && (metadata.Contents != null))
                    {
                        foreach (var content in metadata.Contents)
                        {
                            logger.Debug("delete '{0}' from DropBox", content.Path);
                            dropBoxClient.Delete(content.Path);
                        }
                    }

                    //DropBox upload all backups

                    logger.Info("upload backups to DropBox");

                    foreach (var backupFile in config.BackupDirectory.GetFiles("*.zip"))
                    {
                        logger.Debug("upload '{0}' to DropBox", backupFile.Name);
                        dropBoxClient.UploadFile("", backupFile.Name, backupFile.OpenRead());
                    }
                }
            }

            stopWatch.Stop();

            logger.Info("finished in {0:hh\\:mm\\:ss}", stopWatch.Elapsed);
        }
    }
}
