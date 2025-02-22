using System.Text.Json.Serialization;
using Coplt.Union.Utilities.Json;

namespace Coplt.Union.Utilities;

[Union]
[JsonConverter(typeof(ResultConverter))]
public readonly partial struct Result<T, E>
{
    [UnionTemplate]
    private interface Template
    {
        T Ok();
        E Err();
    }
}
