namespace JAz.LogIngestion;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

using Azure;
using Azure.Core.Diagnostics;
using Azure.Monitor.Ingestion;

[Cmdlet(VerbsCommunications.Send, $"{Settings.Prefix}Log")]
public class SendLog : CancellablePSCmdlet
{
	[NotNull]
	[Parameter(Mandatory = true)]
	public string? RuleId { get; set; }

	[NotNull]
	[Parameter(Mandatory = true)]
	public string? StreamName { get; set; }

	[NotNull]
	[Parameter(Mandatory = true, ValueFromPipeline = true)]
	public PSObject? InputObject { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	public LogsIngestionClient? Client { get; set; } = Context.Client;

	readonly BlockingCollection<object> logs = new(1000);
	readonly AzureDebugLogCollector azureDebugLogCollector;

	Task<Response>? logTask { get; set; }

	public SendLog()
	{
		azureDebugLogCollector = new AzureDebugLogCollector(this);
	}

	protected override void BeginProcessing()
	{
		if (Client is null)
		{
			ThrowTerminatingError(
				new ErrorRecord(
					new InvalidOperationException("You must first connect with Connect-JAzLogIngestionClient or provide a client via -Client parameter"),
					"ClientNotConnected",
					ErrorCategory.InvalidOperation,
					null
				)
			);
		}

		LogsUploadOptions uploadOptions = new()
		{
			Serializer = new PowerShellJsonSerializer(PipelineStopToken)
		};

		// BUG: https://github.com/Azure/azure-sdk-for-net/issues/45361
		// UseConsumingEnumerable can minimize memory usage by making logs streaming via the cmdlet, however there is
		// a bug in the Azure SDK that causes the first item to be dropped with an assert, leading a single item submission
		// to cause a NullReferenceException.
		// We work around this by sending a dummy item for the logs to consume.
		// If the above is fixed, this should be removed. It won't hurt anything as this dummy submitted log will just be silently dropped.
		logs.Add("This item will be consumed by the AssertNotNullOrEmpty in UploadAsync. If you see it, it is a bug");

		//This ensures the upload occurs on a separate thread so as not to block our cmdlet ProcessRecord ingestion
		logTask = Task.Run(
			async () =>
			{
				// This reports the ingestion client logs so that we can marshall them to the PowerShell debug stream
				using AzureEventSourceListener clientLogger = azureDebugLogCollector.CreateAzureDebugLogger();

				return await Client.UploadAsync(
					RuleId,
					StreamName,
					logs.GetConsumingEnumerable(),
					uploadOptions,
					PipelineStopToken
				).ConfigureAwait(false);
			}
		);
	}

	protected override void ProcessRecord()
	{
		// Feeds the ingestion client and should keep the operation streaming
		// And the memory usage low but still allow batching to occur.
		WriteDebug("Adding log to queue");
		logs.Add(InputObject);

		azureDebugLogCollector.WriteCollectedDebugLogs();
	}

	protected override void EndProcessing()
	{
		WriteDebug("Finished processing Logs");
		logs.CompleteAdding();

		// This should not be null as it should have been started in BeginProcessing
		ArgumentNullException.ThrowIfNull(logTask);

		azureDebugLogCollector.WriteCollectedDebugLogs();
		Response? response = logTask?.GetAwaiter().GetResult();
		if (response is not null && PassThru.IsPresent)
		{
			WriteObject(response);
		}

		azureDebugLogCollector.WriteCollectedDebugLogs();
	}

	protected override void StopProcessing()
	{
		logs.CompleteAdding();
		logs.Dispose();
		azureDebugLogCollector.WriteCollectedDebugLogs();
	}
}