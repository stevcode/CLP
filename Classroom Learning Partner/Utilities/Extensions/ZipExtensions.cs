using System.Collections.Generic;
using System.Linq;
using Catel;
using Ionic.Zip;

namespace Classroom_Learning_Partner
{
    public static class ZipExtensions
    {
        #region Extensions

        /// <summary>Searches for the specific entry in the given internal zip directory.</summary>
        /// <param name="zip">The already opened zip file to act upon.</param>
        /// <param name="entryDirectory">Expected to be a full directory path, ending with a forward slash.</param>
        /// <param name="entryName">Includes file extension.</param>
        public static ZipEntry GetEntryByNameInDirectory(this ZipFile zip, string entryDirectory, string entryName)
        {
            Argument.IsNotNull("zip", zip);
            Argument.IsNotNull("entryName", entryName);

            var entryPath = CombineEntryDirectoryAndName(entryDirectory, entryName);
            return zip.Entries.FirstOrDefault(e => e.FileName == entryPath);
        }

        /// <summary>Searches for the specific entry in the given internal zip directory.</summary>
        /// <param name="zip">The already opened zip file to act upon.</param>
        /// <param name="entryDirectory">Expected to be a full directory path, ending with a forward slash.</param>
        public static List<ZipEntry> GetEntriesInDirectory(this ZipFile zip, string entryDirectory)
        {
            Argument.IsNotNull("zip", zip);
            Argument.IsNotNull("entryDirectory", entryDirectory);

            return zip.Entries.Where(e => e.FileName.StartsWith(entryDirectory)).ToList();
        }

        /// <summary>Renames the ZipEntry while keeping the current internal zip directory.</summary>
        /// <param name="entry">Entry to rename.</param>
        /// <param name="newEntryName">Includes file extension.</param>
        public static void RenameEntry(this ZipEntry entry, string newEntryName)
        {
            Argument.IsNotNull("entry", entry);
            Argument.IsNotNullOrWhitespace("newEntryName", newEntryName);

            var lastSlashIndex = entry.FileName.LastIndexOf('/');
            if (lastSlashIndex < 0)
            {
                entry.FileName = newEntryName;
                return;
            }

            var newPath = entry.FileName.Substring(0, lastSlashIndex + 1) + newEntryName;
            entry.FileName = newPath;
        }

        /// <summary>Moves a ZipEntry from one internal zip directory to another internal zip directory.</summary>
        /// <param name="entry">The already opened zip file to act upon.</param>
        /// <param name="newDirectory">Expected to be a full directory path, ending with a forward slash. Null is root directory.</param>
        public static void MoveEntry(this ZipEntry entry, string newDirectory)
        {
            Argument.IsNotNull("entry", entry);

            if (string.IsNullOrWhiteSpace(newDirectory))
            {
                newDirectory = string.Empty;
            }

            var lastSlashIndex = entry.FileName.LastIndexOf('/');
            var entryName = lastSlashIndex < 0 ? entry.FileName : entry.FileName.Substring(lastSlashIndex + 1);
            var newPath = $"{newDirectory}{entryName}";
            entry.FileName = newPath;

            // TODO: If moving the last entry from a directory, the directory is erased, include toggle to preserver empty directory.
        }

        #endregion // Extensions

        #region Static Helpers

        /// <summary>Combines an internal zip directory and an entry name into the full internal path of the entry.</summary>
        /// <param name="entryDirectory">Expected to be a full directory path, ending with a forward slash.</param>
        /// <param name="entryName">Includes file extension.</param>
        public static string CombineEntryDirectoryAndName(string entryDirectory, string entryName)
        {
            return string.IsNullOrWhiteSpace(entryDirectory) ? entryName : $"{entryDirectory}{entryName}";
        }

        #endregion // Static Helpers
    }
}