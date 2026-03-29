using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPos.Application.Interfaces;

public interface IDataSyncService
{
    Task<string> ExportOrdersToCsvAsync(string filePath);
    Task<string> ExportBasketDataToCsvAsync(string filePath);
    Task SyncAllDataToMlServiceAsync();
}
