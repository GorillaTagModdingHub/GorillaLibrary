using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using GorillaLibrary.Wardrobe.Attributes;
using GorillaLibrary.Wardrobe.Behaviours;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: ModdedWardrobeSection("Outfits", typeof(OutfitSection_Load), typeof(OutfitSection_Clone))]

namespace GorillaLibrary.Wardrobe;

[BepInPlugin("dev.gorillalibrary.wardrobe", "GorillaLibrary.Wardrobe", "1.0.0")]
[BepInDependency("dev.gorillalibrary")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static List<ModdedWardrobeSectionAttribute> Sections;

    private GameObject sharedObject;

    public void Awake()
    {
        Logger = base.Logger;

        Events.Core.OnGameInitialized += OnGameInitialized;
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    public void OnGameInitialized()
    {
        Sections = [];

        foreach (var (guid, pluginInfo) in Chainloader.PluginInfos)
        {
            var assembly = pluginInfo?.Instance?.GetType().Assembly;
            assembly?.GetCustomAttributes().ForEach(attribute =>
            {
                if (attribute is ModdedWardrobeSectionAttribute category)
                {
                    Sections.Add(category);
                }
            });
        }

        sharedObject = new GameObject($"{Info.Metadata.Name} {Info.Metadata.Version}");
        DontDestroyOnLoad(sharedObject);

        Sections.ForEach(category =>
        {
            var types = category.SectionTypes?.Where(type => typeof(WardrobeCategory).IsAssignableFrom(type));
            var list = new List<WardrobeCategory>();
            types.ForEach(type => list.Add((WardrobeCategory)sharedObject.AddComponent(type)));
            category.categories = list;
        });
    }
}
