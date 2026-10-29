using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BetterHotkeys
{
    internal static class StorageItemDisplay
    {
        private const string Prefix = "storage-display-v1:";
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private static string Encode(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? ""));
        private static string Decode(string text) => Encoding.UTF8.GetString(Convert.FromBase64String(text));
        internal static void Observe(GameItem item)
        {
            // Called on the existing selected item, never on a fabricated preview instance.
            if (PlayerStore.Instance == null || string.IsNullOrEmpty(item.identifier)) return;
            var data = string.Join("|", Encode(item.spriteAtlasPath), Encode(item.spritePath), Encode(item.GetDisplayName(false)), item.useSpriteFromMod ? "1" : "0");
            ModHelper.SetModData(Settings.Id, Prefix + item.identifier, data);
            sprites.Remove(item.identifier);
        }
        private static string[]? Read(string id)
        {
            if (PlayerStore.Instance == null) return null;
            var text = ModHelper.GetModData(Settings.Id, Prefix + id, "");
            var values = text.Split('|');
            if (values.Length != 4) return null;
            try { return new[] { Decode(values[0]), Decode(values[1]), Decode(values[2]), values[3] }; }
            catch (FormatException) { return null; }
        }
        internal static string Name(string id) => Read(id)?[2] ?? id;
        internal static Sprite? Sprite(string id)
        {
            var data = Read(id);
            if (data == null) return null;
            var resourceKey = Encode(data[0]) + "|" + Encode(data[1]) + "|" + data[3];
            if (sprites.TryGetValue(resourceKey, out var cached) && cached != null) return cached;
            Sprite? sprite = null;
            if (data[3] == "1")
            {
                // Native mod atlas identity is resolved by the item renderer; no cross-mod guess here.
                return null;
            }
            if (data[0].Length != 0 && data[1].Length != 0) sprite = RenderHandler.LoadFromAtlas(data[0], data[1]);
            if (sprite != null) sprites[resourceKey] = sprite;
            return sprite;
        }
        internal static void Reset() => sprites.Clear();
    }
}
