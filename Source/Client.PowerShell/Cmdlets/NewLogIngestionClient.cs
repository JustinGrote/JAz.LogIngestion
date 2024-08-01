namespace JAz.LogIngestion;
using System.Management.Automation;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using System.Diagnostics.Tracing;
using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;

[Cmdlet(VerbsCommunications.Connect, $"JAzLogIngestionClient", DefaultParameterSetName = "Default")]
[OutputType(typeof(LogsIngestionClient))]
public class NewLogIngestionClient : CancellablePSCmdlet, IDisposable
{
	[NotNull]
	[Parameter(Mandatory = true)]
	public string? Endpoint { get; set; }

	[NotNull]
	[Parameter(ParameterSetName = "SecretCredential", Mandatory = true)]
	public PSCredential? Credential { get; set; }

	[Parameter(ParameterSetName = "SecretCredential", Mandatory = true)]
	public Guid TenantId { get; set; }

	// [Parameter(Mandatory = true)]
	// public string? RuleId { get; set; }

	// [Parameter(Mandatory = true)]
	// public string? StreamName { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; }

	protected override void EndProcessing()
	{

		DefaultAzureCredentialOptions options = new()
		{
			ExcludeInteractiveBrowserCredential = false,
			Diagnostics = {
				IsAccountIdentifierLoggingEnabled = true
			}
		};

		TokenCredential credential = ParameterSetName == "SecretCredential"
			? new ClientSecretCredential(
				TenantId.ToString(),
				Credential.UserName,
				Credential.GetNetworkCredential().Password
			)
			: new DefaultAzureCredential(new DefaultAzureCredentialOptions()
			{
				ExcludeInteractiveBrowserCredential = false

			});

		string? selectedCredential = ParameterSetName == "SecretCredential"
			? "ClientSecretCredential"
			: null;

		using AzureEventSourceListener credentialFinder = new(
			(eventData, message) =>
			{
				Regex selectedCredentialRegex = new("DefaultAzureCredential credential selected: (?<credential>.+)");
				if (selectedCredentialRegex.IsMatch(message))
				{
					selectedCredential = selectedCredentialRegex.Match(message).Groups["credential"].Value;
					WriteDebug($"Found selected credential: {selectedCredential}");
				}
			},
			level: EventLevel.Informational
		);

		using AzureEventSourceListener debugLogger = new(
			(eventData, message) => WriteDebug($"New-LogIngestionClient[DefaultAzureCredential]: {message}"),
			level: EventLevel.Informational
		);

		// Test the credentials are valid
		var token = credential.GetToken(new(["https://monitor.azure.com/.default"]), PipelineStopToken);

		Context.Client = new(new Uri(Endpoint), credential);

		if (PassThru.IsPresent is true)
		{
			WriteObject(Context.Client);
		}

		base.EndProcessing();
	}
}