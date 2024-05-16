namespace Core;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

static class Contractual {
    public sealed class AssertionException(string reason) : Exception(reason);

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(
        [DoesNotReturnIf(false)]
        bool condition
    ) {
        if (!condition) {
            throw new AssertionException("assertion failed");
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(
        [DoesNotReturnIf(false)]
        bool condition,
        string message
    ) {
        if (!condition) {
            throw new AssertionException(message);
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(
        [DoesNotReturnIf(false)]
        bool condition,
        Func<string> message
    ) {
        if (!condition) {
            throw new AssertionException(message());
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Requires(
        [DoesNotReturnIf(false)]
        bool condition
    ) {
        if (!condition) {
            throw new AssertionException("pre-condition not met");
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Requires(
        [DoesNotReturnIf(false)]
        bool condition,
        string message
    ) {
        if (!condition) {
            throw new AssertionException(message);
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Requires(
        [DoesNotReturnIf(false)]
        bool condition,
        Func<string> message
    ) {
        if (!condition) {
            throw new AssertionException(message());
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensures(
        [DoesNotReturnIf(false)]
        bool condition
    ) {
        if (!condition) {
            throw new AssertionException("post-condition not met");
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensures(
        [DoesNotReturnIf(false)]
        bool condition,
        string message
    ) {
        if (!condition) {
            throw new AssertionException(message);
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensures(
        [DoesNotReturnIf(false)]
        bool condition,
        Func<string> message
    ) {
        if (!condition) {
            throw new AssertionException(message());
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DoesNotReturn]
    public static void Panic(string message) {
        throw new Exception(message);
    }
}
