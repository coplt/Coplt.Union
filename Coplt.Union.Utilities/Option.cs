using System.Text.Json.Serialization;
using Coplt.Union.Utilities.Json;

namespace Coplt.Union.Utilities;

[Union]
[JsonConverter(typeof(OptionConverter))]
public readonly partial struct Option<T>
{
    [UnionTemplate]
    private interface Template
    {
        void None();
        T Some();
    }

    public Option() => this = MakeNone();

    public Option(T value) => this = MakeSome(value);

    public bool HasValue => IsSome;

    public T Value => Some;

    public static implicit operator Option<T>(T value) => new(value);

    public static explicit operator T(Option<T> value) => value.Value;
}
