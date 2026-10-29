using System;
using System.Globalization;
using System.Linq;
using System.Text;
namespace StarterMod
{
    internal sealed class StorageRule
    {
        internal string Name { get; set; } = "";
        internal string[] Tags { get; set; } = Array.Empty<string>();
        internal bool All { get; set; } = true;
        internal int Priority { get; set; }
    }

    internal static class StorageRuleCodec
    {
        private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
        internal static string Write(StorageRule rule) => Encode(rule.Name ?? "") + "|" + (rule.All ? "1" : "0") + "|" +
            rule.Priority.ToString(CultureInfo.InvariantCulture) + "|" + string.Join(",", rule.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.Ordinal).Select(Encode));
        internal static StorageRule? Read(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            try
            {
                var fields = raw.Split('|');
                if (fields.Length != 4 || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var priority)) return null;
                if (fields[1] != "0" && fields[1] != "1") return null;
                return new StorageRule { Name = Decode(fields[0]), All = fields[1] == "1", Priority = priority,
                    Tags = fields[3].Length == 0 ? Array.Empty<string>() : fields[3].Split(',').Select(Decode).ToArray() };
            }
            catch (FormatException) { return null; }
        }
    }
}
