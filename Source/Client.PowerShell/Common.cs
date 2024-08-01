using Azure.Monitor.Ingestion;

namespace JAz.LogIngestion;

class Context
{
	internal static LogsIngestionClient? Client { get; set; }
}