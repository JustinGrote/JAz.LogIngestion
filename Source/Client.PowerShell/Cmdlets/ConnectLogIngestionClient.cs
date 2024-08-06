namespace JAz.LogIngestion;
using System.Management.Automation;
using Azure.Identity;
using Azure.Monitor.Ingestion;
using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using System.Reflection.Metadata;

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

		LogsIngestionClientOptions clientOptions = new();
		Context.Client = new(new Uri(Endpoint), credential, clientOptions);

		if (PassThru.IsPresent is true)
		{
			WriteObject(Context.Client);
		}

		base.EndProcessing();
	}
}
