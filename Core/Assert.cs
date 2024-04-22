namespace Core;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

static class Contractual {
    [DebuggerHidden]
    [StackTraceHidden]
    public static void Assert([DoesNotReturnIf(false)] bool condition) {
        Assert(condition, "assertion failed");
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Assert([DoesNotReturnIf(false)] bool condition, string message) {
        Assert(condition, () => message);
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        Func<string> message
    ) {
        if (!condition) {
            throw new Exception(message());
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Requires([DoesNotReturnIf(false)] bool condition) {
        Requires(condition, "pre-condition failed");
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Requires([DoesNotReturnIf(false)] bool condition, string message) {
        Requires(condition, () => message);
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Requires([DoesNotReturnIf(false)] bool condition, Func<string> message) {
        if (!condition) {
            throw new Exception(message());
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Ensures([DoesNotReturnIf(false)] bool condition) {
        Ensures(condition, "post-condition failed");
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Ensures([DoesNotReturnIf(false)] bool condition, string message) {
        Ensures(condition, () => message);
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void Ensures([DoesNotReturnIf(false)] bool condition, Func<string> message) {
        if (!condition) {
            throw new Exception(message());
        }
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [DoesNotReturn]
    public static void Panic(string message) {
        throw new Exception(message);
    }

    [DebuggerHidden]
    [StackTraceHidden]
    public static void DoOnlyOnce(ref bool doneBefore) {
        Requires(!doneBefore, "should only execute once");
        doneBefore = true;
    }
}
