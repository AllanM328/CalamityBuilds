using System;
using System.Collections.Generic;
using System.IO;
using CalamityBuilds;
using Terraria;
using Terraria.ModLoader;

public class TooltipAppender : GlobalItem
{
    // Ensure each instance of TooltipAppender has its own data by setting InstancePerEntity to true
    public override bool InstancePerEntity => true;

    // Dictionary to hold item recommendations data by item name
    private static Dictionary<string, Dictionary<string, string>> itemRecommendations = new Dictionary<string, Dictionary<string, string>>();

    // Load CSV data when the mod initializes
    public override void Load()
    {
        LoadItemRecommendations();
        Mod.Logger.Info("Load completed. Total items loaded into dictionary: " + itemRecommendations.Count);
    }

    // Method to load recommendations from the CSV file
    private void LoadItemRecommendations()
{
    // Get the path to the CSV file
    string filePath = "items.csv";

    // Confirm the file path
    Mod.Logger.Info("Attempting to load items.csv from: " + filePath);

    if (Mod.FileExists(filePath))
    {
        Mod.Logger.Info("items.csv found. Loading recommendations...");

        using (StreamReader reader = new StreamReader(Mod.GetFileStream(filePath)))
        {
            reader.ReadLine(); // Skip the header

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Mod.Logger.Info("Reading line: " + line);
                string[] parts = line.Split(',');

                if (parts.Length == 4)
                {
                    string itemName = parts[0].Trim();
                    string recommendation = parts[1].Trim();
                    string progressionStage = parts[2].Trim();
                    string itemClass = parts[3].Trim();

                    var recommendationData = new Dictionary<string, string>
                    {
                        { "Recommendation", recommendation },
                        { "ProgressionStage", progressionStage },
                        { "Class", itemClass }
                    };

                    itemRecommendations[itemName] = recommendationData;
                    Mod.Logger.Info($"Added to dictionary - Name: {itemName}, Recommendation: {recommendation}");
                }
                else
                {
                    Mod.Logger.Warn($"Skipping malformed line: {line}");
                }
            }
        }
    }
    else
    {
        Mod.Logger.Warn("Could not find items.csv");
    }
    // Log final dictionary contents
    Mod.Logger.Info("Final dictionary contents:");
    foreach (var key in itemRecommendations.Keys)
    {
        Mod.Logger.Info($"Dictionary contains item: {key}");
    }
}




    // Modify tooltips based on CSV data
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        Mod.Logger.Info("ModifyTooltips called. Current dictionary count: " + itemRecommendations.Count);

        // Use item.Name for dictionary lookup
        string itemName = item.Name;
        Mod.Logger.Info($"Checking item: {itemName}");

        if (itemRecommendations.TryGetValue(itemName, out var recommendationData))
        {
            Mod.Logger.Info($"Found matching entry in dictionary for item: {itemName}");

            string recommendation = recommendationData["Recommendation"];
            string progression = recommendationData["ProgressionStage"];
            string itemClass = recommendationData["Class"];

            string tooltipText = $"{recommendation} (Progression: {progression}, Class: {itemClass})";
            TooltipLine line = new TooltipLine(Mod, "Recommended", tooltipText);
            tooltips.Add(line);
            Mod.Logger.Info($"Tooltip added for item: {itemName}");
        }
        else
        {
            Mod.Logger.Warn($"No tooltip found for item: {itemName}");
        }
    }






}


