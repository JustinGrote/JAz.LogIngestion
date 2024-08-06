# JAz Log Ingestion Client

This is a PowerShell module wrapper for the Azure SDK Logs Ingestion Client. This module requires PowerShell 7.4 or newer.

## Features
* Pipeline-optimized to be memory efficient, nonblocking, and garbage collected
* Multithreaded, batching, and compression for high performance log ingestion
* Auto-converts all powershell objects using PowerShell `ConvertTo-Json` style serialization for log ingestion purposes.
* Can authenticate via any of the methods supported by [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet)
* Debug output to show tracing of the requests sent to the API. Content is not displayed as it is gzipped.

## Quick Start

```powershell
Install-PSResource
Connect-JAzLogIngestionClient -Endpoint 'https://your-dcr-endpoint.ingest.monitor.azure.com' #Will use Azure.Identity to discover credentials, you can also specify a Client Secret App with -Credential and -TenantId

#Ensure you have a DCR/table/etc. created. Improperly formatted logs will be silently dropped by the API
$sendParams = @{
  #Name of the StreamDeclaration that defines your log schema. The JSON representation of what you pass on the pipeline must match this schema or the logs will be silently dropped.
  StreamName = 'Custom-Test', 
  #ImmutableId of your DCR rule
  RuleId = 'dcr-ab2c0598704740618255d6b8105a1fff'
}

@{
  TimeGenerated = (Get-Date)
  Message = 'Test Message'
},@{
  TimeGenerated = (Get-Date)
  Message = 'Test Message 2'
} | Send-JazLog @sendParams

#Larger Bulk Example
1..1000 
| Foreach-Object {
  [PSCustomObject]@{ 
    TimeGenerated = (Get-Date)
    Message = "Test Message $_"
  }
}
| Send-JazLog @sendParams #Will batch and upload parallel automatically
```
