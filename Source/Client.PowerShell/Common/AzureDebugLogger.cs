using System.Diagnostics.Tracing;
using System.Management.Automation;

using Azure.Core.Diagnostics;
using Azure.Monitor.Ingestion;

namespace JAz.LogIngestion;

internal static partial class Context
{
	internal static LogsIngestionClient? Client { get; set; }
}

internal static class PSCmdletExtensions
{
	/// <summary>
	/// Creates an AzureEventSourceListener for logging debug messages.
	/// The listener will write messages to the debug stream of the provided PSCmdlet instance.
	/// Stops listening when disposed, recommended to use in a using block.
	/// </summary>
	/// <param name="cmdlet">The PSCmdlet instance</param>
	/// <returns>An instance of AzureEventSourceListener.</returns>
	internal static AzureEventSourceListener CreateAzureDebugLogger(this PSCmdlet cmdlet)
	{
		string cmdletName = cmdlet.MyInvocation.MyCommand.Name;
		return new AzureEventSourceListener(
			(level, message) => cmdlet.WriteDebug($"{cmdletName}[LogsIngestionClient]: {message}"),
			EventLevel.Informational
		);
	}
}