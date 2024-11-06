using System;
using System.Collections.Generic;
using System.IO;
using CalamityBuilds;
using Terraria;
using Terraria.ModLoader;
using Microsoft.VisualBasic.FileIO;

public class TooltipAppender : GlobalItem
{
    public override bool InstancePerEntity => true;

    private static Dictionary<string, (string ModName, string ProgressionStage)> bossData = new Dictionary<string, (string, string)>();
    private static Dictionary<string, Dictionary<string, string>> itemRecommendations = new Dictionary<string, Dictionary<string, string>>();

    public override void Load()
    {
        LoadItemRecommendations();
        LoadBossData();
        Mod.Logger.Info("Load completed. Total items loaded into dictionary: " + itemRecommendations.Count);
    }

    private void LoadItemRecommendations()
    {
        string filePath = "items.csv";

        Mod.Logger.Info("Attempting to load items.csv from: " + filePath);

        if (Mod.FileExists(filePath))
        {
            Mod.Logger.Info("items.csv found. Loading recommendations...");

            using (StreamReader reader = new StreamReader(Mod.GetFileStream(filePath)))
            using (TextFieldParser parser = new TextFieldParser(reader))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                parser.ReadLine(); // Skips the header

                while (!parser.EndOfData)
                {
                    string[] parts = parser.ReadFields();

                    if (parts.Length >= 4) 
                    {
                        string itemName = parts[0].Trim();
                        string recommendation = parts[1].Trim();
                        string itemClass = parts[2].Trim();
                        string comment = parts[3].Trim();

                        var recommendationData = new Dictionary<string, string>
                        {
                            { "Recommendation", recommendation },
                            { "Class", itemClass },
                            { "Comment", comment }
                        };

                        itemRecommendations[itemName] = recommendationData;
                        Mod.Logger.Info($"Added to dictionary - Name: {itemName}, Recommendation: {recommendation}");
                    }
                }
            }
        }
        else
        {
            Mod.Logger.Warn("Could not find items.csv");
        }
    }

    private void LoadBossData()
    {
        string filePath = "bosses.csv";

        if (Mod.FileExists(filePath))
        {
            using (StreamReader reader = new StreamReader(Mod.GetFileStream(filePath)))
            using (TextFieldParser parser = new TextFieldParser(reader))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                parser.ReadLine(); // Skips the header

                while (!parser.EndOfData)
                {
                    string[] parts = parser.ReadFields();

                    if (parts.Length >= 3)
                    {
                        string bossName = parts[0].Trim();
                        string modName = parts[1].Trim();
                        string progressionStage = parts[2].Trim();

                        bossData[bossName] = (modName, progressionStage);
                    }
                }
            }
        }
    }

    private bool IsBossDefeated(string bossName)
{
    if (bossData.TryGetValue(bossName, out var bossInfo))
    {
        string modName = bossInfo.ModName;
        
        if (modName == "Terraria")
        {
            // Check vanilla bosses using NPC.downed flags
            return bossName switch
            {
                "Eye of Cthulhu" => NPC.downedBoss1,
                "Eater of Worlds" => NPC.downedBoss2,
                "Brain of Cthulhu" => NPC.downedBoss2,
                "Skeletron" => NPC.downedBoss3,
                "Wall of Flesh" => Main.hardMode,
                "Destroyer" => NPC.downedMechBoss1,
                "Twins" => NPC.downedMechBoss2,
                "Skeletron Prime" => NPC.downedMechBoss3,
                "Plantera" => NPC.downedPlantBoss,
                "Golem" => NPC.downedGolemBoss,
                "Lunatic Cultist" => NPC.downedAncientCultist,
                "Moon Lord" => NPC.downedMoonlord,
                _ => false
            };
        }
        else if (modName == "Calamity")
        {
            // Check Calamity bosses dynamically using Mod.Call
            Mod calamityMod = ModLoader.GetMod("CalamityMod");
            if (calamityMod != null)
            {
                return calamityMod.Call("GetBossDowned", bossName) as bool? == true;
            }
        }
    }
    return false;
}


    private string GetCurrentProgressionStage()
    {
        // Default to Pre-Boss if no bosses have been defeated
        string currentStage = "Pre-Boss";

        // Will debug to include individual mech progression check and exo mechs/scal check
       /* int mechanicalBossCount = 0;
        int calamityBossCount = 0;

        bool destroyerDefeated = IsBossDefeated("Destroyer");
        bool twinsDefeated = IsBossDefeated("Twins");
        bool skeletronPrimeDefeated = IsBossDefeated("Skeletron Prime");

        if (destroyerDefeated) mechanicalBossCount++;
        if (twinsDefeated) mechanicalBossCount++;
        if (skeletronPrimeDefeated) mechanicalBossCount++;

        if (mechanicalBossCount == 1)
        {
            currentStage = "Post-Mechanical Boss 1";
        }
        else if (mechanicalBossCount == 2)
        {
            currentStage = "Post-Mechanical Boss 2";
        }
        else if (mechanicalBossCount == 3)
        {
            currentStage = "Pre-Plantera";
        }

        bool exoMechsDefeated = IsBossDefeated("Exo Mechs");
        bool supremeCalamitasDefeated = IsBossDefeated("Supreme Witch, Calamitas");

        if (exoMechsDefeated) calamityBossCount++;
        if (supremeCalamitasDefeated) calamityBossCount++;

        if (calamityBossCount == 1)
        {
            currentStage = "Pre-Exo Mechs/Supreme Witch, Calamitas";
        }
        else if (calamityBossCount == 2)
        {
            currentStage = "Endgame";
        }
        */
        foreach (var boss in bossData)
        {
            string bossName = boss.Key;
            string progressionStage = boss.Value.ProgressionStage;

            if (bossName == "Destroyer" || bossName == "Twins" || bossName == "Skeletron Prime" || bossName == "Exo Mechs" || bossName == "Supreme Witch, Calamitas")
                continue;

            if (IsBossDefeated(bossName) && currentStage != "Pre-Exo Mechs/Supreme Witch, Calamitas" && currentStage != "Endgame")
            {
                currentStage = progressionStage;
            }
        }

        return currentStage;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (itemRecommendations.TryGetValue(item.Name, out var recommendationData))
        {
            string recommendation = recommendationData["Recommendation"];
            string currentProgression = GetCurrentProgressionStage();
            string itemClass = recommendationData["Class"];
            string comment = recommendationData.ContainsKey("Comment") ? recommendationData["Comment"] : "";

            string tooltipText = $"{recommendation} (Current Progression: {currentProgression}, Class: {itemClass})";
            if (!string.IsNullOrEmpty(comment))
        {
            tooltipText += $"\nNote: {comment}";
        }
            TooltipLine line = new TooltipLine(Mod, "Recommended", tooltipText);
            tooltips.Add(line);
        }
    }
}
