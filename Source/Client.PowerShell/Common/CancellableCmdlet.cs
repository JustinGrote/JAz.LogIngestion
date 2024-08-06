namespace JAz.LogIngestion;

using System.Management.Automation;

public abstract class CancellablePSCmdlet : PSCmdlet, IDisposable
{
	private CancellationTokenSource? _pipelineStopTokenSource;
	protected CancellationToken PipelineStopToken => (_pipelineStopTokenSource ??= new()).Token;

	public void Dispose()
	{
		_pipelineStopTokenSource?.Dispose();
		_pipelineStopTokenSource = null;
	}

	protected override void StopProcessing()
	{
		_pipelineStopTokenSource?.Cancel();
		_pipelineStopTokenSource?.Dispose();
		_pipelineStopTokenSource = null;
	}
}