using System.IO;
using Catel;

namespace CLP.Entities.Demo.Demo.DomainModel.Utility.Extentions
{
    public static class DirectoryInfoExtensions
    {
        public static void Copy(this DirectoryInfo directoryInfo, string destinationFolderPath, bool copySubDirectories = true)
        {
            Argument.IsNotNull("directoryInfo", directoryInfo);
            Argument.IsNotNullOrWhitespace("destinationFolderPath", destinationFolderPath);
            Argument.IsNotNull("copySubDirectories", copySubDirectories);

            if (!directoryInfo.Exists)
            {
                return;
            }

            var folderName = directoryInfo.Name;
            var destinationPath = Path.Combine(destinationFolderPath, folderName);
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                var newFilePath = Path.Combine(destinationPath, fileInfo.Name);
                fileInfo.CopyTo(newFilePath, true);
            }

            if (!copySubDirectories)
            {
                return;
            }

            foreach (var subDirectoryInfos in directoryInfo.GetDirectories())
            {
                subDirectoryInfos.Copy(destinationPath);
            }
        }
    }
}