﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Catel;
using CLP.Entities;
using Ionic.Zip;

namespace Classroom_Learning_Partner
{
    public static class ZipExtensions
    {
        #region Extensions

        #region ZipFile

        /// <summary>Retrieves entry in the given internal zip directory by entry path.</summary>
        /// <param name="zip">The already opened zip file to act upon.</param>
        /// <param name="entryPath">Expected to be a full directory path and entry name of the ZipEntry, with extension.</param>
        public static ZipEntry GetEntry(this ZipFile zip, string entryPath)
        {
            Argument.IsNotNull("zip", zip);
            Argument.IsNotNull("entryPath", entryPath);

            return zip[entryPath];
        }

        /// <summary>Searches for the specific entry in the given internal zip directory.</summary>
        /// <param name="zip">The already opened zip file to act upon.</param>
        /// <param name="entryDirectory">Expected to be a full directory path, ending with a forward slash.</param>
        /// <param name="entryName">Includes file extension.</param>
        public static ZipEntry GetEntryByNameInDirectory(this ZipFile zip, string entryDirectory, string entryName)
        {
            Argument.IsNotNull("zip", zip);
            Argument.IsNotNull("entryName", entryName);

            var entryPath = CombineEntryDirectoryAndName(entryDirectory, entryName);
            return zip.GetEntry(entryPath);
        }

        /// <summary>Searches for the specific entry in the given internal zip directory.</summary>
        /// <param name="zip">The already opened zip file to act upon.</param>
        /// <param name="entryDirectory">Expected to be a full directory path, ending with a forward slash.</param>
        public static List<ZipEntry> GetEntriesInDirectory(this ZipFile zip, string entryDirectory, bool isRecursive = false)
        {
            Argument.IsNotNull("zip", zip);
            Argument.IsNotNull("entryDirectory", entryDirectory);

            if (isRecursive)
            {
                return zip.Entries.Where(e => e.FileName.StartsWith(entryDirectory)).ToList();
            }

            var directoryEndingIndex = entryDirectory.LastIndexOf('/');
            return zip.Entries.Where(e => e.FileName.StartsWith(entryDirectory) && e.FileName.LastIndexOf('/') == directoryEndingIndex && !e.FileName.EndsWith("/")).ToList();
        }

        /// <summary>Renames an entry based on full internal file paths.</summary>
        /// <param name="zip">The already opened zip file to act upon.</param>
        /// <param name="oldInternalFilePath">Expected to be a full internal file name, with extension.</param>
        /// <param name="newInternalFilePath">Expected to be a full internal file name, with extension.</param>
        public static void RenameEntry(this ZipFile zip, string oldInternalFilePath, string newInternalFilePath)
        {
            Argument.IsNotNull("zip", zip);
            Argument.IsNotNullOrWhitespace("oldInternalFilePath", oldInternalFilePath);
            Argument.IsNotNullOrWhitespace("newInternalFilePath", newInternalFilePath);

            var zipEntry = zip.Entries.FirstOrDefault(e => e.FileName == oldInternalFilePath);
            if (zipEntry == null)
            {
                return;
            }

            zipEntry.FileName = newInternalFilePath;
        }

        #endregion // ZipFile

        #region ZipEntry

        /// <summary>Extracts a zip entry into the Xml string it contains.</summary>
        /// <param name="entry">Entry to extract.</param>
        public static string ExtractXmlString(this ZipEntry entry)
        {
            Argument.IsNotNull("entry", entry);

            using (var memoryStream = new MemoryStream())
            {
                entry.Extract(memoryStream);

                var xmlString = Encoding.ASCII.GetString(memoryStream.ToArray());
                return xmlString;
            }
        }

        /// <summary>Extracts a xml zip entry into an Entity.</summary>
        /// <param name="entry">Entry to extract.</param>
        public static T ExtractXmlEntity<T>(this ZipEntry entry) where T : ASerializableBase
        {
            Argument.IsNotNull("entry", entry);

            var xmlString = entry.ExtractXmlString();
            var entity = ASerializableBase.FromXmlString<T>(xmlString);

            return entity;
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

            // TODO: If moving the last entry from a directory, the directory is erased, include toggle to preserve empty directory.
        }

        public static string GetEntryNameWithoutExtension(this ZipEntry entry)
        {
            Argument.IsNotNull("entry", entry);

            var lastSlashIndex = entry.FileName.LastIndexOf('/');
            var entryNameWithExtension = lastSlashIndex < 0 ? entry.FileName : entry.FileName.Substring(lastSlashIndex + 1);
            var lastDotIndex = entryNameWithExtension.LastIndexOf('.');
            var entryName = lastDotIndex < 0 ? entryNameWithExtension : entryNameWithExtension.Substring(0, lastDotIndex);
            return entryName;
        }

        public static string GetEntryNameWithExtension(this ZipEntry entry)
        {
            Argument.IsNotNull("entry", entry);

            var lastSlashIndex = entry.FileName.LastIndexOf('/');
            var entryNameWithExtension = lastSlashIndex < 0 ? entry.FileName : entry.FileName.Substring(lastSlashIndex + 1);
            return entryNameWithExtension;
        }

        public static string GetEntryParentDirectory(this ZipEntry entry)
        {
            Argument.IsNotNull("entry", entry);

            var lastSlashIndex = entry.FileName.LastIndexOf('/');
            var entryParentDirectory = entry.FileName.Substring(0, lastSlashIndex + 1);
            return entryParentDirectory;
        }

        #endregion // ZipEntry

        #endregion // Extensions

        #region Static Helpers

        /// <summary>Combines an internal zip directory and an entry name into the full internal path of the entry.</summary>
        /// <param name="entryDirectory">Expected to be a full directory path, can end with a forward slash or not.</param>
        /// <param name="entryName">Includes file extension.</param>
        public static string CombineEntryDirectoryAndName(string entryDirectory, string entryName)
        {
            if (!string.IsNullOrEmpty(entryDirectory) &&
                entryDirectory.LastIndexOf('/') != entryDirectory.Length - 1)
            {
                entryDirectory = $"{entryDirectory}/";
            }

            return string.IsNullOrWhiteSpace(entryDirectory) ? entryName : $"{entryDirectory}{entryName}";
        }

        #endregion // Static Helpers
    }
}