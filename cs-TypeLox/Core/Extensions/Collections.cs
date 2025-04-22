namespace Core;

public static class CollectionsExtensions {
    public static V ComputeIfAbsent<K, V>(this IDictionary<K, V> self, K key, Func<K, V> compute) {
        if (self.TryGetValue(key, out var result)) {
            return result;
        } else {
            return self[key] = compute(key);
        }
    }

    public static void AddNotNull<T>(this ICollection<T> self, T? item)
    where T : notnull {
        if (item is not null) {
            self.Add(item);
        }
    }
}
