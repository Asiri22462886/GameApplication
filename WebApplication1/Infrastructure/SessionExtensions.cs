using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WordGame.Infrastructure
{
    public static class SessionExtensions
    {
        // Stores a complex object in session by converting it to JSON first.
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Reads a JSON value from session and converts it back into the original object type.
        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json);
        }
    }
}
