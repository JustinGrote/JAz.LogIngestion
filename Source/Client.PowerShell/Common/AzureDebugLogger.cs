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
			(eventData, message) => cmdlet.WriteDebug($"{cmdletName}[LogsIngestionClient]: {message}"),
			EventLevel.Verbose
		);
	}
}

/// <summary>
/// Collects debug logs from Azure SDK and has a methods to write them to the debug stream of a PSCmdlet.
/// </summary>
internal class AzureDebugLogCollector(PSCmdlet cmdlet)
{
	readonly Queue<(EventWrittenEventArgs, string)> clientLogs = [];
	string cmdletName => cmdlet.MyInvocation.MyCommand.Name;

	/// <summary>
	/// Creates an AzureEventSourceListener for logging debug messages.
	/// The listener will write messages to the debug stream of the provided PSCmdlet instance.
	/// Stops listening when disposed, recommended to use in a using block.
	/// </summary>
	/// <param name="cmdlet">The PSCmdlet instance</param>
	/// <returns>An instance of AzureEventSourceListener.</returns>
	internal AzureEventSourceListener CreateAzureDebugLogger(EventLevel verbosity = EventLevel.Verbose)
	{

		return new AzureEventSourceListener(
			(eventData, message) => clientLogs.Enqueue((eventData, message)),
			verbosity
		);
	}

	internal void WriteCollectedDebugLogs()
	{
		while (clientLogs.TryDequeue(out var log))
		{
			var (_, message) = log;
			cmdlet.WriteDebug($"{cmdletName}[LogsIngestionClient]: {message}");
		}
	}

	internal void WatchForDebugLogs(Task task, CancellationToken cancellationToken)
	{
		while (!task.Wait(500, cancellationToken))
		{
			WriteCollectedDebugLogs();
		}
	}
}