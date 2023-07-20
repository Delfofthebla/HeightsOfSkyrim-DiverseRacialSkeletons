using System.Diagnostics;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;

namespace HeightsOfSkyrimDiverseRacialSkeletons;

public static class Program
{
    private static Lazy<Settings> _settings = new();
    private static Settings Settings => _settings.Value;
        
    public static async Task<int> Main(string[] args)
        => await SynthesisPipeline.Instance
            .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
            .SetAutogeneratedSettings("Settings", "settings.json", out _settings)
            .SetTypicalOpen(GameRelease.SkyrimSE, "HeightOfSkyrimPatch.esp")
            .AddRunnabilityCheck(state =>
            {
                Debug.Assert(state.LoadOrder.ContainsKey(ModKey.FromNameAndExtension("Heights_of_Skyrim.esp")),
                    "\n\nYour Heights_of_Skyrim.esp is not in load order or above Synthesis.esp in LO\n\n");
                    
                Debug.Assert(state.LoadOrder.ContainsKey(ModKey.FromNameAndExtension("FK's Diverse Racial Skeletons.esp")),
                    "\n\nYour FK's Diverse Racial Skeletons.esp is not in load order or above Synthesis.esp in LO\n\n");
            })
            .Run(args);

    private static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
    {
        ModKey heightsModKey = ModKey.FromNameAndExtension("Heights_of_Skyrim.esp");
        state.LoadOrder.TryGetIfEnabledAndExists(heightsModKey, out var heightsMod);
        if (heightsMod is null)
            throw new Exception("Your Heights_of_Skyrim.esp is not activated, present, or located above Synthesis.esp in LO");
        
        ModKey fksModKey = ModKey.FromNameAndExtension("FK's Diverse Racial Skeletons.esp");
        state.LoadOrder.TryGetIfEnabledAndExists(fksModKey, out var fksMod);
        if (fksMod is null)
            throw new Exception("Your FK's Diverse Racial Skeletons.esp is not activated, present, or located above Synthesis.esp in LO");

        var modifiedRaceHeights = ReApplyRacialHeightChanges(state, fksMod);
        AdjustNpcHeightsInLineWithRacialChanges(state, heightsMod, modifiedRaceHeights);
    }

    private static Dictionary<(FormKey, bool), float> ReApplyRacialHeightChanges(
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        ISkyrimModGetter fksMod)
    {
        List<FormKey> fksRaceKeys = fksMod.Races.Select(x => x.FormKey).ToList();
        List<IRaceGetter> raceWinningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<IRaceGetter>()
            .Where(x => fksRaceKeys.Contains(x.FormKey))
            .ToList();

        var modifiedRaceHeights = new Dictionary<(FormKey, bool), float>();
        foreach (IRaceGetter fksRace in fksMod.Races)
            UpdateRaceIfNecessary(state, raceWinningOverrides, fksRace, modifiedRaceHeights);

        return modifiedRaceHeights;
    }

    private static void UpdateRaceIfNecessary(
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        IEnumerable<IRaceGetter> raceWinningOverrides,
        IRaceGetter fksRace,
        IDictionary<(FormKey, bool), float> modifiedRaceHeights)
    {
        IRaceGetter winningOverride = raceWinningOverrides.First(x => x.FormKey == fksRace.FormKey);
        Race raceToPatch = state.PatchMod.Races.GetOrAddAsOverride(winningOverride);
        Race resolvedFksRace = fksRace.DeepCopy(); // There's goooootta be a better way to do this, but I am too unfamiliar with the API to know for sure.

        var maleHeightIsUnchanged = Math.Abs(winningOverride.Height.Male - fksRace.Height.Male) < 0.00001;
        var femaleHeightIsUnchanged = Math.Abs(winningOverride.Height.Female - fksRace.Height.Female) < 0.00001;
        var raceToPatchMaleSkeleton = raceToPatch.SkeletalModel?.Male;
        var raceToPatchFemaleSkeleton = raceToPatch.SkeletalModel?.Female;
        var fksMaleSkeleton = resolvedFksRace.SkeletalModel?.Male;
        var fksFemaleSkeleton = resolvedFksRace.SkeletalModel?.Female;

        var maleSkeletonIsUnchanged = string.Equals(fksMaleSkeleton?.File.RawPath?.Trim(),
            raceToPatchMaleSkeleton?.File.RawPath.Trim(),
            StringComparison.InvariantCultureIgnoreCase);
        var femaleSkeletonIsUnchanged = string.Equals(fksFemaleSkeleton?.File.RawPath.Trim(),
            raceToPatchFemaleSkeleton?.File.RawPath.Trim(),
            StringComparison.InvariantCultureIgnoreCase);
        
        if (maleHeightIsUnchanged && femaleHeightIsUnchanged && maleSkeletonIsUnchanged && femaleSkeletonIsUnchanged)
        {
            Console.WriteLine("All values for race '" + resolvedFksRace.Name + "' are correct. Skipping.");
            return;
        }

        if (!maleHeightIsUnchanged || !femaleHeightIsUnchanged)
            Console.WriteLine("Updated at least one gendered height for race: " + resolvedFksRace.Name);

        if (!maleHeightIsUnchanged)
        {
            raceToPatch.Height.Male = fksRace.Height.Male;
            modifiedRaceHeights[(raceToPatch.FormKey, false)] = fksRace.Height.Male;
            Console.WriteLine("\tMale Height Updated: " + raceToPatch.Height.Male);
        }

        if (!femaleHeightIsUnchanged)
        {
            raceToPatch.Height.Female = fksRace.Height.Female;
            modifiedRaceHeights[(raceToPatch.FormKey, true)] = fksRace.Height.Female;
            Console.WriteLine("\tFemale Height Updated: " + raceToPatch.Height.Female);
        }

        if (!maleSkeletonIsUnchanged || !femaleHeightIsUnchanged)
        {
            raceToPatch.SkeletalModel = resolvedFksRace.SkeletalModel;
            Console.WriteLine("Updated Racial Skeleton path for race: " + raceToPatch.Name);
            Console.WriteLine("\tMale Path Updated: " + resolvedFksRace?.SkeletalModel?.Male?.File.RawPath);
            Console.WriteLine("\tFemale Path Updated: " + resolvedFksRace?.SkeletalModel?.Female?.File.RawPath);
        }
    }

    private static void AdjustNpcHeightsInLineWithRacialChanges(
        IPatcherState<ISkyrimMod, ISkyrimModGetter> state,
        ISkyrimModGetter heightsMod,
        Dictionary<(FormKey, bool), float> modifiedRaceHeights)
    {
        List<FormKey> heightsNpcKeys = heightsMod.Npcs.Select(x => x.FormKey).ToList();
        List<INpcGetter> npcWinningOverrides = state.LoadOrder.PriorityOrder.WinningOverrides<INpcGetter>()
            .Where(x => heightsNpcKeys.Contains(x.FormKey))
            .ToList();

        foreach (INpcGetter heightsNpc in heightsMod.Npcs)
        {
            INpcGetter winningOverride = npcWinningOverrides.First(x => x.FormKey == heightsNpc.FormKey);

            INpc npcToPatch = state.PatchMod.Npcs.GetOrAddAsOverride(winningOverride);
            (FormKey FormKey, bool) raceKey = (npcToPatch.Race.FormKey, (npcToPatch.Configuration.Flags & NpcConfiguration.Flag.Female) != 0);

            var raceWasModified = modifiedRaceHeights.ContainsKey(raceKey);
            if (!raceWasModified)
                HandleNpcWithUnmodifiedRace(heightsNpc, npcToPatch);
            else
                HandleNpcWithModifiedRace(heightsNpc, npcToPatch);
        }
    }

    private static void HandleNpcWithModifiedRace(INpcGetter heightsNpc, INpc npcToPatch)
    {
        // If someone is being cheeky and setting non 1.0 heights on an NPC this won't be super accurate.
        // If I was more familiar with the API I'd read it from the master plugin where it was originally defined.
        var heightDiff = 1.0f - heightsNpc.Height;
        var multipliedHeight = npcToPatch.Height - (float) (heightDiff * Settings.HeightChangeMultiplier);

        Console.WriteLine(
            "Npc with name: " + npcToPatch.Name +
            " had a height of " + npcToPatch.Height +
            ", changing it to " + multipliedHeight
        );
        
        npcToPatch.Height = multipliedHeight;
    }
    
    private static void HandleNpcWithUnmodifiedRace(INpcGetter heightsNpc, INpc npcToPatch)
    {
        if (Math.Abs(npcToPatch.Height - heightsNpc.Height) < 0.00001)
        {
            Console.WriteLine(
                "Npc with name: " + npcToPatch.Name + ", " +
                "contains a race that was not touched by FK's. " +
                "It's height is also already correct. Skipping NPC."
            );
        }
        else
        {
            npcToPatch.Height = heightsNpc.Height;
            Console.WriteLine(
                "Npc with name: " + npcToPatch.Name + ", " +
                "contains a race that was not touched by FK's. " +
                "Height from Heights of Skyrim will be applied directly."
            );
        }
    }
}
