using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace HeightsOfSkyrimDiverseRacialSkeletons;

public class Settings
{
    [MaintainOrder]

    [SettingName("Height Change Multiplier")]
    [Tooltip(
        "The multiplier that will be applied to the height changes made by Heights of Skyrim. " +
        "This will only be applied if FK's racial changes would have impacted the specific NPC's height.\r\n." + 
        "If the race height was not modified, this value will not be used for that NPC."
    )]
    public double HeightChangeMultiplier { get; set; } = 0.5;
}