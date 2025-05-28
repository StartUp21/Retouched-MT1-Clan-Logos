using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TrainworksReloaded.Base;
using TrainworksReloaded.Core;
using UnityEngine;

namespace Retouched_MT1_Clan_Logos
{
	public static class PluginInfo
	{
		public const string PLUGIN_GUID = "RetouchedMT1ClanLogos.Plugin";

		public const string PLUGIN_NAME = "RetouchedMT1ClanLogos.Plugin";

		public const string PLUGIN_VERSION = "0.0.1";
	}
	[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		internal readonly string[] resources = { "small", "medium", "large", "silhouette" };
		private void Awake()
		{
			Dictionary<string, object> iconDict = GetIcons();
			Railend.ConfigurePostAction(c =>
			{
				if (c.GetInstance<GameDataClient>().TryGetProvider<SaveManager>(out var saveManager))
				{
					var balanceData = saveManager.GetAllGameData().GetBalanceData();
					var iconsField = AccessTools.Field(typeof(ClassData), "icons");
					var iconDictGUIDLookup = iconDict;
					foreach (var classData in balanceData.GetClassDatas())
					{
						if (iconDictGUIDLookup.TryGetValue(classData.GetID(), out var icon))
							iconsField.SetValue(classData, icon);
					}
				}
				else
				{
					Logger.LogError("Failed to get SaveManager instance");
				}
			});
			Logger.LogInfo("Plugin RetouchedMT1ClanLogos.Plugin is loaded!");
		}

		private Dictionary<string, object> GetIcons()
		{
			Dictionary<string, object> iconDict = new Dictionary<string, object>();
			var iconSetType = AccessTools.Inner(typeof(ClassData), "IconSet");
			var iconSetTypeFields = resources.ToDictionary(key => key, value => AccessTools.Field(iconSetType, value));
			var textures = LoadTextures();
			var classes = new (ClassCardStyle name, string guid)[]
			{
				(ClassCardStyle.Hellhorned, "c595c344-d323-4cf1-9ad6-41edc2aebbd0"),
				(ClassCardStyle.Awoken		, "fd119fcf-c2cf-469e-8a5a-e9b0f265560d"),
				(ClassCardStyle.Remnant   , "4fe56363-b1d9-46b7-9a09-bd2df1a5329f"),
				(ClassCardStyle.Stygian   , "9317cf9a-04ec-49da-be29-0e4ed61eb8ba"),
				(ClassCardStyle.Umbra     , "fda62ada-520e-42f3-aa88-e4a78549c4a2")
			};
			for (int c = 0; c < classes.Length; c++)
			{
				object icon = Activator.CreateInstance(iconSetType);
				iconDict.Add(classes[c].guid, icon);
				ClassCardStyle className = classes[c].name;
				foreach (var field in iconSetTypeFields)
				{
					Rect r = default;
					switch (field.Key)
					{
						case "small":
							r = new Rect(48 * c, 48, 48, 48);
							break;
						case "medium":
							r = new Rect(128 * c, 128, 128, 128);
							break;
						case "large":
							r = new Rect(92 * c, 92, 92, 92);
							break;
						case "silhouette":
							r = new Rect(73.1429f * c, 73, 73.1429f, 73);
							break;
					}
					var iconSprite = Sprite.Create(textures[field.Key], r, new Vector2(0.5f, 0.5f));
					iconSprite.name = $"Retouched {className} {field.Key}";
					field.Value.SetValue(icon, iconSprite);
				}
			}
			return iconDict;
		}

		private Dictionary<string, Texture2D> LoadTextures()
		{
			Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
			for (int i = 0; i < resources.Length; i++)
			{
				string res = resources[i];
				byte[]? data = null;
				var asm = Assembly.GetExecutingAssembly();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					asm.GetManifestResourceStream($"Retouched_MT1_Clan_Logos.Resources.{res}.png").CopyTo(memoryStream);
					data = memoryStream.ToArray();
				}
				Texture2D t = new Texture2D(4, 4, res == "medium" ? TextureFormat.DXT5 : TextureFormat.RGBA32, mipChain: false);
				t.LoadImage(data);
				t.wrapMode = TextureWrapMode.Clamp;
				t.wrapModeU = TextureWrapMode.Clamp;
				t.wrapModeV = TextureWrapMode.Clamp;
				t.wrapModeW = TextureWrapMode.Clamp;
				t.filterMode = FilterMode.Bilinear;
				t.Apply(updateMipmaps: true, makeNoLongerReadable: true);
				t.name = $"Retouched {res}";
				textures.Add(res, t);
			}
			return textures;
		}
	}
}
