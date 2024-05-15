using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mindscape.Raygun4Net.Storage;

/// <summary>
/// Stores a cached copy of crash reports that failed to send in Local App Data
/// Creates a directory if specified, otherwise creates a unique directory based off the location of the application
/// </summary>
public sealed class LocalApplicationDataCrashReportCache : ICrashReportCache
{
  private readonly FileSystemCrashReportCache _fileSystemErrorStorage;

  public LocalApplicationDataCrashReportCache(string directoryName = null)
  {
    if (directoryName is null)
    {
      // Try generate a unique id, from the executable location
      var uniqueId = Assembly.GetEntryAssembly()?.Location ?? throw new ApplicationException("Cannot determine unique application id");

      var uniqueIdHash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(uniqueId));
      directoryName = BitConverter.ToString(uniqueIdHash).Replace("-", "").ToLowerInvariant();
    }

    var localAppDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), directoryName);
    _fileSystemErrorStorage = new FileSystemCrashReportCache(localAppDirectory);
  }

  public Task<List<CrashReportCacheEntry>> GetAll(CancellationToken cancellationToken)
  {
    return _fileSystemErrorStorage.GetAll(cancellationToken);
  }

  public Task<CrashReportCacheEntry> Save(string crashPayload, string apiKey, CancellationToken cancellationToken)
  {
    return _fileSystemErrorStorage.Save(crashPayload, apiKey, cancellationToken);
  }

  public Task<bool> Remove(CrashReportCacheEntry cacheEntry, CancellationToken cancellationToken)
  {
    return _fileSystemErrorStorage.Remove(cacheEntry, cancellationToken);
  }
}