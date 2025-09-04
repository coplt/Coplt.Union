using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Coplt.Union.Utilities.Json;

namespace Coplt.Union.Utilities;

[Union2(GenerateEquals = false)]
[JsonConverter(typeof(OptionConverter))]
public partial struct Option<T> : IEquatable<Unit>
#if NET7_0_OR_GREATER
    , IEqualityOperators<Option<T>, Unit, bool>
#endif
{
    [UnionTemplate]
    private interface Template
    {
        void None();
        T Some();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option() => this = None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option(T value) => this = Some(value);

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsSome ? 1 : 0;
    }

    public bool HasValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsSome;
    }

    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Some;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Option<T>(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(Option<T> value) => value.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Option<T>(Unit none) => Option<T>.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Unit other) => Tag == Tags.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Option<T> other) => Tag == other.Tag && Tag switch
    {
        Tags.Some => EqualityComparer<T>.Default.Equals(Unsafe.AsRef(in this).Some, other.Some),
        _ => true,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode() => Tag switch
    {
        Tags.Some => HashCode.Combine(Tag, Unsafe.AsRef(in this).Some),
        _ => HashCode.Combine(Tag),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj) => obj switch
    {
        Option<T> other => Equals(other),
        Unit other => Equals(other),
        _ => false,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Option<T> left, Unit right) => left.Equals(right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Option<T> left, Unit right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> Cast<U>() => IsNone ? Option<U>.None : Some((U)(object)Some!);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U?> As<U>() => IsNone ? Option<U?>.None : Some(Some is U u ? u : default);
}

public static class Option
{
    public static Unit None
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T>(this Option<T> option) => option.Some;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T>(this Option<T> option, T or) => option.IsNone ? or : option.Some;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T>(this Option<T> option, Func<T> or) => option.IsNone ? or() : option.Some;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T, A>(this Option<T> option, A arg, Func<A, T> or) => option.IsNone ? or(arg) : option.Some;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? UnwrapOrDefault<T>(this Option<T> option) => option.IsNone ? default : option.Some;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Filter<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.IsNone ? None : predicate(option.Some) ? Some(option.Some) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Filter<T, A>(this Option<T> option, A arg, Func<A, T, bool> predicate) =>
        option.IsNone ? None : predicate(arg, option.Some) ? Some(option.Some) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Flatten<T>(this Option<Option<T>> option) => option.IsNone ? None :
        option.Some is { IsSome: true, Some: var some } ? Some(some) : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> Map<T, R>(this Option<T> option, Func<T, R> selector) =>
        option.IsNone ? None : Some(selector(option.Some));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> Map<T, R, A>(this Option<T> option, A arg, Func<A, T, R> selector) =>
        option.IsNone ? None : Some(selector(arg, option.Some));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, R>(this Option<T> option, R or, Func<T, R> selector) =>
        option.IsNone ? or : selector(option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, R, A>(this Option<T> option, R or, A arg, Func<A, T, R> selector) =>
        option.IsNone ? or : selector(arg, option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, R>(this Option<T> option, Func<R> or, Func<T, R> selector) =>
        option.IsNone ? or() : selector(option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, R, A>(this Option<T> option, A arg, Func<A, R> or, Func<A, T, R> selector) =>
        option.IsNone ? or(arg) : selector(arg, option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> OkOr<T, E>(this Option<T> option, E err) =>
        option.IsNone ? Err(err) : Ok(option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> OkOr<T, E>(this Option<T> option, Func<E> err) =>
        option.IsNone ? Err(err()) : Ok(option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> OkOr<T, E, A>(this Option<T> option, A arg, Func<A, E> err) =>
        option.IsNone ? Err(err(arg)) : Ok(option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> And<T, R>(this Option<T> option, Option<R> other) =>
        option.IsNone ? None : other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> AndThan<T, R>(this Option<T> option, Func<T, Option<R>> selector) =>
        option.IsNone ? None : selector(option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> AndThan<T, R, A>(this Option<T> option, A arg, Func<A, T, Option<R>> selector) =>
        option.IsNone ? None : selector(arg, option.Some);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Or<T>(this Option<T> option, Option<T> other) =>
        option.IsNone ? other : option;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OrElse<T>(this Option<T> option, Func<Option<T>> ctor) =>
        option.IsNone ? ctor() : option;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> OrElse<T, A>(this Option<T> option, A arg, Func<A, Option<T>> ctor) =>
        option.IsNone ? ctor(arg) : option;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Xor<T>(this Option<T> option, Option<T> other) =>
        option.IsSome && other.IsNone ? option :
        option.IsNone && other.IsSome ? other :
        None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Insert<T>(this ref Option<T> option, T value)
    {
        option = Some(value);
        return ref option.Some;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetOrInsert<T>(this ref Option<T> option, T value)
    {
        if (option.IsNone) option = Some(value);
        return ref option.Some;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetOrInsert<T>(this ref Option<T> option, Func<T> ctor)
    {
        if (option.IsNone) option = Some(ctor());
        return ref option.Some;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetOrInsert<T, A>(this ref Option<T> option, A arg, Func<A, T> ctor)
    {
        if (option.IsNone) option = Some(ctor(arg));
        return ref option.Some;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T? GetOrInsertDefault<T>(this ref Option<T> option)
    {
        if (option.IsNone) option = Some<T>(default!);
        return ref option.Some!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Take<T>(this ref Option<T> option)
    {
        var r = option;
        option = None;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> TakeIf<T>(this ref Option<T> option, Func<T, bool> predicate)
    {
        if (option.IsSome && !predicate(option.Some)) return None;
        var r = option;
        option = None;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> TakeIf<T, A>(this ref Option<T> option, A arg, Func<A, T, bool> predicate)
    {
        if (option.IsSome && !predicate(arg, option.Some)) return None;
        var r = option;
        option = None;
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Replace<T>(this ref Option<T> option, T value)
    {
        var r = option;
        option = Some(value);
        return r;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<(T, U)> Zip<T, U>(this Option<T> option, Option<U> other)
    {
        if (option.IsNone || other.IsNone) return None;
        return Some((option.Some, other.Some));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> Zip<T, U, R>(this Option<T> option, Option<U> other, Func<T, U, R> selector)
    {
        if (option.IsNone || other.IsNone) return None;
        return Some(selector(option.Some, other.Some));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> Zip<T, U, R, A>(this Option<T> option, Option<U> other, A arg, Func<A, T, U, R> selector)
    {
        if (option.IsNone || other.IsNone) return None;
        return Some(selector(arg, option.Some, other.Some));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (Option<T>, Option<U>) UnZip<T, U>(this Option<(T, U)> option)
    {
        if (option.IsNone) return (None, None);
        var some = option.Some;
        return (Some(some.Item1), Some(some.Item2));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Count<T>(this Option<T> option) =>
        option.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> Select<T, R>(this Option<T> option, Func<T, R> selector) =>
        Map(option, selector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<R> SelectMany<T, R>(this Option<T> option, Func<T, Option<R>> selector) =>
        AndThan(option, selector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        Filter(option, predicate);
}

public static class OptionStruct
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? TryGet<T>(this Option<T> self) where T : struct => self.IsSome ? self.Some : default;
}

public static class OptionClass
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? TryGet<T>(this Option<T> self) where T : class => self.IsSome ? self.Some : default;
}
