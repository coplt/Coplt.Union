using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Coplt.Union.Utilities.Json;

namespace Coplt.Union.Utilities;

[Union2]
[JsonConverter(typeof(ResultConverter))]
public partial struct Result<T, E>
{
    [UnionTemplate]
    private interface Template
    {
        T Ok();
        E Err();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, E>(OkOf<T> r) => Result<T, E>.Ok(r.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T, E>(ErrOf<E> r) => Result<T, E>.Err(r.Error);
}

public readonly record struct OkOf<T>(T Value)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> ToResult<E>() => Result<T, E>.Ok(Value);
}

public readonly record struct ErrOf<E>(E Error)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, E> ToResult<T>() => Result<T, E>.Err(Error);
}

public static class Result<A>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, A> Ok<T>(T value) => Result<T, A>.Ok(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<A, E> Err<E>(E value) => Result<A, E>.Err(value);
}

public static class Result
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OkOf<T> Ok<T>(T value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrOf<E> Err<E>(E value) => new(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> Ok<T, E>(T value) => Result<T, E>.Ok(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, E> Err<T, E>(E value) => Result<T, E>.Err(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<R, E> Map<T, E, R>(this Result<T, E> result, Func<T, R> selector) =>
        result.IsOk ? Ok(selector(result.Ok)) : Err(result.Err);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<R, E> Map<T, E, R, A>(this Result<T, E> result, A arg, Func<A, T, R> selector) =>
        result.IsOk ? Ok(selector(arg, result.Ok)) : Err(result.Err);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, E, R>(this Result<T, E> result, R or, Func<T, R> selector) =>
        result.IsOk ? selector(result.Ok) : or;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, E, R, A>(this Result<T, E> result, R or, A arg, Func<A, T, R> selector) =>
        result.IsOk ? selector(arg, result.Ok) : or;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, E, R>(this Result<T, E> result, Func<R> or, Func<T, R> selector) =>
        result.IsOk ? selector(result.Ok) : or();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static R MapOr<T, E, R, A>(this Result<T, E> result, A arg, Func<A, R> or, Func<A, T, R> selector) =>
        result.IsOk ? selector(arg, result.Ok) : or(arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, R> MapErr<T, E, R>(this Result<T, E> result, Func<E, R> selector) =>
        result.IsErr ? Err(selector(result.Err)) : Ok(result.Ok);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, R> MapErr<T, E, R, A>(this Result<T, E> result, A arg, Func<A, E, R> selector) =>
        result.IsErr ? Err(selector(arg, result.Err)) : Ok(result.Ok);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T, E>(this Result<T, E> result) => result.Ok;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T, E>(this Result<T, E> result, T or) =>
        result.IsOk ? result.Ok : or;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T, E>(this Result<T, E> result, Func<T> or) =>
        result.IsOk ? result.Ok : or();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Unwrap<T, E, A>(this Result<T, E> result, A arg, Func<A, T> or) =>
        result.IsOk ? result.Ok : or(arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? UnwrapOrDefault<T, E>(this Result<T, E> result) =>
        result.IsOk ? result.Ok : default;

#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UnwrapOrThrow<T, E>(this Result<T, E> result) where E : Exception =>
        result.IsOk ? result.Ok : throw result.Err;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static E UnwrapErr<T, E>(this Result<T, E> result) => result.Err;

#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UnwrapThrow<T, E>(this Result<T, E> result) where E : Exception => throw result.Err;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<U, E> And<T, E, U>(this Result<T, E> result, Result<U, E> other) =>
        result.IsOk ? other : Err(result.Err);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<U, E> AndThen<T, E, U>(this Result<T, E> result, Func<Result<U, E>> other) =>
        result.IsOk ? other() : Err(result.Err);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<U, E> AndThen<T, E, U, A>(this Result<T, E> result, A arg, Func<A, Result<U, E>> other) =>
        result.IsOk ? other(arg) : Err(result.Err);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, F> Or<T, E, F>(this Result<T, E> result, Result<T, F> other) =>
        result.IsOk ? Ok(result.Ok) : other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, F> OrElse<T, E, F>(this Result<T, E> result, Func<Result<T, F>> other) =>
        result.IsOk ? Ok(result.Ok) : other();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, F> OrElse<T, E, F, A>(this Result<T, E> result, A arg, Func<A, Result<T, F>> other) =>
        result.IsOk ? Ok(result.Ok) : other(arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Option<T>, E> Transpose<T, E>(this Option<Result<T, E>> option)
    {
        if (option.IsNone) return Ok<Option<T>, E>(None);
        ref var result = ref option.Some;
        return result.IsErr ? Err(result.Err) : Ok(Some(result.Ok));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Result<T, E>> Transpose<T, E>(this Result<Option<T>, E> result)
    {
        if (result.IsErr) return Some(Err<T, E>(result.Err));
        ref var option = ref result.Ok;
        return option.IsNone ? None : Some(Ok<T, E>(option.Some));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<R, E> Select<T, E, R>(this Result<T, E> result, Func<T, R> selector) =>
        Map(result, selector);
}
