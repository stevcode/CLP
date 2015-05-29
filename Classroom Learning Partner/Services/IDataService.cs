using System.Collections.Generic;

namespace Classroom_Learning_Partner.Services
{
    public interface IDataService {
        string CurrentCachesFolderPath { get; set; }
        List<Cache> AvailableCaches { get; }
        Cache CurrentCache { get; set; }
        bool CreateNewCache(string cacheName);
        bool CreateNewCache(string cacheName, string cachesFolderPath);
    }
}