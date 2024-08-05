namespace JAz.LogIngestion;

using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

using Azure.Monitor.Ingestion;

[Cmdlet(VerbsCommunications.Send, $"JAzLog")]
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
	public object? InputObject { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	readonly List<object> _logs = [];

	protected override void BeginProcessing()
	{
		_logs.Clear();
	}

	protected override void ProcessRecord()
	{
		_logs.Add(InputObject);
	}

	protected override void EndProcessing()
	{
		if (Context.Client is null)
		{
			ThrowTerminatingError(
				new ErrorRecord(
					new InvalidOperationException("You must first connect with Connect-JAzLogIngestionClient"),
					"ClientNotConnected",
					ErrorCategory.InvalidOperation,
					null
				)
			);
		}

		LogsUploadOptions uploadOptions = new()
		{
			Serializer = new PowerShellJsonSerializer()
		};

		using var debugLogger = this.CreateAzureDebugLogger();
		Azure.Response response = Context.Client.Upload(RuleId, StreamName, _logs, uploadOptions, PipelineStopToken);
		if (PassThru.IsPresent)
		{
			WriteObject(response);
		}
	}
}