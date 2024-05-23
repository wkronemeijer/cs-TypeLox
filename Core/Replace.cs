namespace Core;

using System.Runtime.CompilerServices;

public static class Replacement {
    /// <summary>
    /// Assigns the value to the reference, and then returns the <b>old</b> value.
    /// </summary>
    public static T Replace<T>(ref T current, T newValue) {
        var oldValue = current;
        current = newValue;
        return oldValue;
    }

    public readonly ref struct OldValueContainer<T> {
        readonly ref T reference;
        readonly T oldValue;

        /// <summary>
        /// Restores the reference to the given value when Dispose() is called.
        /// </summary>
        public OldValueContainer(ref T reference, T oldValue) {
            this.reference = ref reference;
            this.oldValue = oldValue;
        }

        public void Dispose() {
            Requires(!Unsafe.IsNullRef(in reference));
            reference = oldValue;
        }
    }

    public static OldValueContainer<T> ReplaceUsing<T>(
        ref T variable,
        T newValue
    ) {
        var oldValue = variable;
        variable = newValue;
        return new(ref variable, oldValue);
    }
}
