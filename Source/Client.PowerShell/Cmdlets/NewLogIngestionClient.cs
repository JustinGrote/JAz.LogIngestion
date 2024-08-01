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
using static System.Management.Automation.ErrorCategory;
using System.CodeDom;

[Cmdlet(VerbsCommunications.Connect, $"JAzLogIngestionClient", DefaultParameterSetName = "Default")]
[OutputType(typeof(LogsIngestionClient))]
public class NewLogIngestionClient : CancellablePSCmdlet
{
	[NotNull]
	[Parameter(Mandatory = true)]
	public string? Endpoint { get; set; }

	[NotNull]
	[Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
	public PSCredential? Credential { get; set; }

	[Parameter(ParameterSetName = "ClientSecret", Mandatory = true)]
	public Guid TenantId { get; set; }

	[Parameter]
	public SwitchParameter PassThru { get; set; }

	[Parameter]
	public SwitchParameter NoVerify { get; set; }

	protected override void EndProcessing()
	{
		DefaultAzureCredentialOptions options = new()
		{
			ExcludeInteractiveBrowserCredential = false,
			Diagnostics = {
				IsAccountIdentifierLoggingEnabled = true
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

		if (NoVerify.IsPresent is false)
		{
			string? selectedCredential = ParameterSetName == "ClientSecret"
				? "ClientSecretCredential"
				: null;

			using AzureEventSourceListener credentialFinder = new(
				(eventData, message) =>
				{
					Regex selectedCredentialRegex = new("DefaultAzureCredential credential selected: (?<credential>.+)");
					if (selectedCredentialRegex.IsMatch(message))
					{
						selectedCredential = selectedCredentialRegex.Match(message).Groups["credential"].Value;
					}
				},
				level: EventLevel.Informational
			);

			using var listener = this.CreateAzureDebugLogger();

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

			WriteVerbose($"Authenticated to monitor.azure.com using: {selectedCredential}");


		}

		Context.Client = new(new Uri(Endpoint), credential);

		if (PassThru.IsPresent is true)
		{
			WriteObject(Context.Client);
		}

		base.EndProcessing();
	}
}