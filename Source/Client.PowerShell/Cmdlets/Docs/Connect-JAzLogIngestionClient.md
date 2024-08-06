---
external help file: Jaz.LogIngestion.dll-Help.xml
Module Name: JAz.LogIngestion
online version:
schema: 2.0.0
---

# Connect-JAzLogIngestionClient

## SYNOPSIS
Creates a Log Ingestion Client instance, and optionally verifies that the token can connect to the correct scope. Note that the authentication provided will not be tested until the first Send-JAzLog is performed.

## SYNTAX

### Default (Default)
```
Connect-JAzLogIngestionClient -Endpoint <String> [-PassThru] [-NoVerify] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ClientSecret
```
Connect-JAzLogIngestionClient -Endpoint <String> -Credential <PSCredential> -TenantId <Guid> [-PassThru]
 [-NoVerify] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Configures the module to submit logs to a log ingestion workspace.

## EXAMPLES

### Example 1
```powershell
PS> Connect-JAzLogIngestionClient -Endpoint 'https://your-dcr-endpoint.ingest.monitor.azure.com'
```

Prepares a default log ingestion client for the specified Log Ingestion Endpoint. Will cycle through the available DefaultAzureCredential authentication methods including interactive authentication if none other are found.

### Example 2
```powershell
PS> Connect-JAzLogIngestionClient -Endpoint 'https://your-dcr-endpoint.ingest.monitor.azure.com' -Credential (Get-Credential) -TenantId "yourtenantID"
```

Use an Application to connect, providing the client ID, client secret, and tenant ID (in either guid or domain form)

## PARAMETERS

### -Credential
For authenticating using an application, provide the Client ID as the username and the Client Secret as the password

```yaml
Type: PSCredential
Parameter Sets: ClientSecret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Endpoint
The URL for the ingestion address of the DCR, either via a DCE or via direct ingestion.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Provide the Client Object so that it can be passed to other commands. Generally not necssary as it will be saved as a default credential.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TenantId
Specify the tenant ID of the application you wish to authenticate with.

```yaml
Type: Guid
Parameter Sets: ClientSecret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### Azure.Monitor.Ingestion.LogsIngestionClient

## NOTES

## RELATED LINKS
