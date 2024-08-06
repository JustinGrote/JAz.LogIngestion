namespace JAz.LogIngestion;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Azure.Core.Serialization;

using Newtonsoft.Json;

using static Microsoft.PowerShell.Commands.JsonObject;

/// <summary>
/// Serializes Objects using the PowerShell engine, which is useful for Extended Type System (ETS) objects.
/// </summary>
///
class PowerShellJsonSerializer : ObjectSerializer
{
	ConvertToJsonContext context;
	/// <summary>
	/// Creates a new instance of the <see cref="PowerShellJsonSerializer"/> class.
	/// </summary>
	/// <param name="context">Customize the serialization process. Note that StringsAsEnums is on by default.</param>
	public PowerShellJsonSerializer(ConvertToJsonContext? context)
	{
		this.context = context ?? new(
			default,
			true,
			default,
			StringEscapeHandling.Default,
			null,
			CancellationToken.None
		);
	}

	public PowerShellJsonSerializer(CancellationToken token) : this(new ConvertToJsonContext(
		default,
		true,
		default,
		StringEscapeHandling.Default,
		null,
		token
	))
	{ }

	public override object? Deserialize(Stream stream, Type returnType, CancellationToken cancellationToken) =>
		throw new NotSupportedException();

	public override ValueTask<object?> DeserializeAsync(Stream stream, Type returnType, CancellationToken cancellationToken) =>
		throw new NotSupportedException();

	public override ValueTask SerializeAsync(Stream stream, object? value, Type inputType, CancellationToken cancellationToken)
	{
		using StreamWriter writer = new(stream, leaveOpen: true);
		return new(
				writer.WriteAsync(
				SerializeJson(value, cancellationToken)
			)
		);
	}

	public override void Serialize(Stream stream, object? value, Type inputType, CancellationToken cancellationToken)
	{
		using StreamWriter writer = new(stream, leaveOpen: true);
		writer.Write(
			SerializeJson(value, cancellationToken)
		);
	}

	string SerializeJson(object? value, CancellationToken cancellationToken) =>
		ConvertToJson(value, AddCancelToken(context, cancellationToken));

	/// <summary>
	/// Creates a new context with an appropriate cancel token. Used for thread safety rather than mutating the original context.
	/// </summary>
	static ConvertToJsonContext AddCancelToken(ConvertToJsonContext context, CancellationToken cancellationToken) =>
		new(
			context.MaxDepth,
			context.EnumsAsStrings,
			context.CompressOutput,
			context.StringEscapeHandling,
			context.Cmdlet,
			cancellationToken
		);
}