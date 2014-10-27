﻿using System.Collections.Generic;
using CLP.Entities;

namespace Classroom_Learning_Partner.Services
{
    public interface INotebookService
    {
        List<string> AvailableLocalCacheNames { get; }
        string CurrentLocalCacheDirectory { get; set; }
        string CurrentClassCacheDirectory { get; }
        string CurrentImageCacheDirectory { get; }
        string CurrentNotebookCacheDirectory { get; }
        List<string> AvailableLocalNotebookNames { get; }
        List<Notebook> OpeNotebooks { get; }
        Notebook CurrentNotebook { get; set; }
        ClassPeriod CurrentClassPeriod { get; set; }

        bool InitializeNewLocalCache(string cacheName);
        bool InitializeNewLocalCache(string cacheName, string cacheDirectoryPath);
        void ArchiveNotebookCache(string notebookCacheDirectory);
    }
}