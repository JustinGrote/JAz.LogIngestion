using System.Diagnostics.Tracing;
using System.Management.Automation;

using Azure.Core.Diagnostics;
using Azure.Monitor.Ingestion;

namespace JAz.LogIngestion;

internal static class Context
{
	internal static LogsIngestionClient? Client { get; set; }
}

internal static class PSCmdletExtensions
{
	internal static AzureEventSourceListener CreateAzureDebugLogger(this PSCmdlet cmdlet)
	{
		string cmdletName = cmdlet.MyInvocation.MyCommand.Name;
		return new AzureEventSourceListener(
			(level, message) => cmdlet.WriteDebug($"{cmdletName}[LogsIngestionClient]: {message}"),
			EventLevel.Informational
		);
	}
}