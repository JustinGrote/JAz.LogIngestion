namespace JAz.LogIngestion;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core.Serialization;

using static Microsoft.PowerShell.Commands.JsonObject;

class PowerShellJsonSerializer : ObjectSerializer
{
	public override object? Deserialize(Stream stream, Type returnType, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public override ValueTask<object?> DeserializeAsync(Stream stream, Type returnType, CancellationToken cancellationToken)
	{
		throw new NotSupportedException();
	}

	public override ValueTask SerializeAsync(Stream stream, object? value, Type inputType, CancellationToken cancellationToken)
	{
		using StreamWriter writer = new(stream, leaveOpen: true);
		return new(
				writer.WriteAsync(
				SerializeJson(value)
			)
		);
	}

	public override void Serialize(Stream stream, object? value, Type inputType, CancellationToken cancellationToken)
	{
		using StreamWriter writer = new(stream, leaveOpen: true);
		writer.Write(
			SerializeJson(value)
		);
	}

	string SerializeJson(object? value)
	{
		ConvertToJsonContext context = new(2, true, false, Newtonsoft.Json.StringEscapeHandling.Default, null, CancellationToken.None);
		return ConvertToJson(value, context);
	}
}