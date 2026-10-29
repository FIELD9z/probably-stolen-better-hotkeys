using System;
using System.Globalization;
using System.Linq;
using System.Text;
namespace BetterHotkeys
{
    internal sealed class StorageRule
    {
        internal string Name { get; set; } = "";
        internal string[] Tags { get; set; } = Array.Empty<string>();
        internal bool All { get; set; } = true;
        internal int Priority { get; set; }
        internal string[] AllowedItems { get; set; } = Array.Empty<string>();
        internal string[] DeniedItems { get; set; } = Array.Empty<string>();
        internal bool ItemsOnly { get; set; }
    }

    internal static class StorageRuleCodec
    {
        private static string Encode(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
        internal static string Write(StorageRule rule) => Encode(rule.Name ?? "") + "|" + (rule.All ? "1" : "0") + "|" +
            rule.Priority.ToString(CultureInfo.InvariantCulture) + "|" + string.Join(",", rule.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.Ordinal).Select(Encode)) + "|" + (rule.ItemsOnly ? "1" : "0") + "|" + Pack(rule.AllowedItems) + "|" + Pack(rule.DeniedItems);
        private static string Pack(string[] values) => string.Join(",", values.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).Select(Encode));
        private static string[] Unpack(string value) => value.Length == 0 ? Array.Empty<string>() : value.Split(',').Select(Decode).ToArray();
        internal static StorageRule? Read(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            try
            {
                var fields = raw.Split('|');
                if ((fields.Length != 4 && fields.Length != 7) || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var priority)) return null;
                if (fields[1] != "0" && fields[1] != "1") return null;
                if (fields.Length == 7 && fields[4] != "0" && fields[4] != "1") return null;
                return new StorageRule { Name = Decode(fields[0]), All = fields[1] == "1", Priority = priority,
                    Tags = Unpack(fields[3]), ItemsOnly = fields.Length == 7 && fields[4] == "1",
                    AllowedItems = fields.Length == 7 ? Unpack(fields[5]) : Array.Empty<string>(),
                    DeniedItems = fields.Length == 7 ? Unpack(fields[6]) : Array.Empty<string>() };
            }
            catch (FormatException) { return null; }
        }
    }
}
