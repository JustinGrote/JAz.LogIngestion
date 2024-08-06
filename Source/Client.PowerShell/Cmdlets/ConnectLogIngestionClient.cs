namespace JAz.LogIngestion;
using System.Management.Automation;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using System.Diagnostics.Tracing;
using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using System.Reflection.Metadata;
using static System.Management.Automation.ErrorCategory;

[Cmdlet(VerbsCommunications.Connect, $"{Settings.Prefix}LogIngestionClient", DefaultParameterSetName = "Default")]
[OutputType(typeof(LogsIngestionClient))]
public class NewLogIngestionClient : CancellablePSCmdlet
{
	[NotNull]
	[Parameter(Mandatory = true)]
	public string? Endpoint { get; set; }

	[NotNull]
	[Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
	public PSCredential? Credential { get; set; }

	[NotNull]
	[Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
	public string? TenantId { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	public SwitchParameter Verify { get; set; }

	readonly AzureDebugLogCollector azureDebugLogCollector;

	public NewLogIngestionClient()
	{
		azureDebugLogCollector = new AzureDebugLogCollector(this);
	}

	protected override void EndProcessing()
	{
		DefaultAzureCredentialOptions options = new()
		{
			ExcludeInteractiveBrowserCredential = false,
			Diagnostics = {
				IsAccountIdentifierLoggingEnabled = true,
				IsLoggingContentEnabled = true,
			}
		};

		TokenCredential credential = ParameterSetName == "ClientSecret"
			? new ClientSecretCredential(
				TenantId.ToString(),
				Credential.UserName,
				Credential.GetNetworkCredential().Password,
				new()
				{
					Diagnostics = {
						IsAccountIdentifierLoggingEnabled = true
					}
				}
			)
			: new DefaultAzureCredential(new DefaultAzureCredentialOptions()
			{
				// We are OK with interactive logins if possible
				ExcludeInteractiveBrowserCredential = false
			});

		if (Verify.IsPresent)
		{
			string? selectedCredential = ParameterSetName == "ClientSecret"
				? "ClientSecretCredential"
				: null;

			using AzureEventSourceListener credentialFinder = new(
				(eventData, message) =>
				{
					const int DEFAULT_AZURE_CREDENTIAL_SELECTED = 13;
					if (eventData.EventSource.Name == "Azure.Identity"
						&& eventData.EventId == DEFAULT_AZURE_CREDENTIAL_SELECTED
						&& eventData.Payload?[0] is string payload
					)
					{
						selectedCredential = payload;
					}
				},
				level: EventLevel.Verbose
			);

			using var listener = azureDebugLogCollector.CreateAzureDebugLogger();

			// Test the credentials are valid
			var token = credential.GetToken(new(["https://monitor.azure.com/.default"]), PipelineStopToken);

			if (selectedCredential is null)
			{
				ThrowTerminatingError(new(
					new("Unable to authenticate to monitor.azure.com. Please check you have provided valid credentials. -Debug will provide more information"),
					"FailedToFindSelectedCredential",
					AuthenticationError,
					null
				));
			}

			azureDebugLogCollector.WriteCollectedDebugLogs();
			WriteVerbose($"Authenticated to monitor.azure.com using: {selectedCredential}");
		}

		LogsIngestionClientOptions clientOptions = new() { };

		Context.Client = new(new Uri(Endpoint), credential, clientOptions);

		if (PassThru.IsPresent is true)
		{
			WriteObject(Context.Client);
		}

		base.EndProcessing();
	}
}