using System;
using System.IO;
using System.Xml;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using OP_Engine.Sounds;
using OP_Engine.Inventories;
using OP_Engine.Utility;
using OP_Engine.Characters;
using OP_Engine.Tiles;
using OP_Engine.Scenes;
using FMOD;
using OP_Engine.Time;

namespace DoS1.Util
{
    public static class Load
    {
        #region Variables



        #endregion

        #region Constructors



        #endregion

        #region Methods

        #region Parse INI for Options

        public static void ParseINI(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Game":
                                VisitINI(reader);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game.CrashHandler(e);
                }
            }
        }

        private static void VisitINI(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Game" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Options":
                        VisitOptions(reader);
                        break;

                    case "Speed":
                        VisitSpeed(reader);
                        break;
                }
            }
        }

        private static void VisitOptions(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Fullscreen":
                        Main.Game.GraphicsManager.IsFullScreen = reader.Value == "True";
                        Main.Game.GraphicsManager.ApplyChanges();
                        break;

                    case "Resolution":
                        var parts = reader.Value.Split(',');
                        int X = int.Parse(parts[0]);
                        int Y = int.Parse(parts[1]);

                        if (Main.Game.GraphicsManager.IsFullScreen)
                        {
                            Main.Game.GraphicsManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                            Main.Game.GraphicsManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                        }
                        else
                        {
                            Main.Game.Form.Width = X;
                            Main.Game.Form.Height = Y;
                            Main.Game.GraphicsManager.PreferredBackBufferWidth = Main.Game.Form.Width;
                            Main.Game.GraphicsManager.PreferredBackBufferHeight = Main.Game.Form.Height;
                        }

                        Main.Game.ScreenWidth = X;
                        Main.Game.ScreenHeight = Y;

                        Main.Game.ResolutionChange();

                        Main.Game.GraphicsManager.ApplyChanges();
                        break;

                    case "MusicEnabled":
                        SoundManager.MusicEnabled = reader.Value == "True";
                        break;

                    case "MusicVolume":
                        SoundManager.MusicVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "AmbientEnabled":
                        SoundManager.AmbientEnabled = reader.Value == "True";
                        break;

                    case "AmbientVolume":
                        SoundManager.AmbientVolume = float.Parse(reader.Value) / 10;
                        break;

                    case "SoundEnabled":
                        SoundManager.SoundEnabled = reader.Value == "True";
                        break;

                    case "SoundVolume":
                        SoundManager.SoundVolume = float.Parse(reader.Value) / 10;
                        break;
                }
            }
        }

        private static void VisitSpeed(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "TimeSpeed":
                        Main.TimeSpeed = int.Parse(reader.Value);
                        break;

                    case "CombatSpeed":
                        Main.CombatSpeed = int.Parse(reader.Value);
                        break;
                }
            }
        }

        #endregion

        #region Load Game

        public static void LoadGame()
        {
            string saveDir = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save);

            string gameDat = Path.Combine(saveDir, "game.dat");
            ParseGame(gameDat);

            string armiesDat = Path.Combine(saveDir, "armies.dat");
            ParseArmies(armiesDat);

            string inventoryDat = Path.Combine(saveDir, "inventory.dat");
            ParseInventory(inventoryDat);

            string worldDat = Path.Combine(saveDir, "worlds.dat");
            ParseWorlds(worldDat);
        }

        #region Game

        private static void ParseGame(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Game":
                                VisitGame(reader);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game.CrashHandler(e);
                }
            }
        }

        private static void VisitGame(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Game" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "GameProperties":
                        VisitGameProperties(reader);
                        break;

                    case "Shops":
                        VisitShops(reader);

                        if (Handler.LocalMap)
                        {
                            Handler.TradingShop = Handler.ShopInventories[Handler.Level];
                        }
                        break;

                    case "Academies":
                        VisitAcademies(reader);

                        if (Handler.LocalMap)
                        {
                            Handler.TradingAcademy = Handler.AcademyRecruits[Handler.Level];
                        }
                        break;
                }
            }
        }

        private static void VisitGameProperties(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        Handler.ID = long.Parse(reader.Value);
                        break;

                    case "Gold":
                        Handler.Gold = int.Parse(reader.Value);
                        break;

                    case "MainCharacter_ID":
                        Handler.MainCharacter_ID = long.Parse(reader.Value);
                        break;

                    case "LocalMap":
                        Handler.LocalMap = reader.Value == "True";
                        break;

                    case "Level":
                        Handler.Level = int.Parse(reader.Value);
                        break;

                    case "ManualPause":
                        Handler.ManualPause = reader.Value == "True";
                        break;

                    case "LocalPause":
                        Handler.LocalPause = reader.Value == "True";
                        break;

                    case "ItemFilter":
                        Handler.ItemFilter = reader.Value;
                        break;

                    case "Time_TotalYears":
                        TimeManager.Now.TotalYears = long.Parse(reader.Value);
                        break;

                    case "Time_Years":
                        TimeManager.Now.Years = long.Parse(reader.Value);
                        break;

                    case "Time_TotalMonths":
                        TimeManager.Now.TotalMonths = long.Parse(reader.Value);
                        break;

                    case "Time_Months":
                        TimeManager.Now.Months = long.Parse(reader.Value);
                        break;

                    case "Time_TotalDays":
                        TimeManager.Now.TotalDays = long.Parse(reader.Value);
                        break;

                    case "Time_Days":
                        TimeManager.Now.Days = long.Parse(reader.Value);
                        break;

                    case "Time_TotalHours":
                        TimeManager.Now.TotalHours = long.Parse(reader.Value);
                        break;

                    case "Time_Hours":
                        TimeManager.Now.Hours = long.Parse(reader.Value);
                        TimeManager.Reset(TimeRate.Second, (int)TimeManager.Now.Years, (int)TimeManager.Now.Months, 
                            (int)TimeManager.Now.Days, (int)TimeManager.Now.Hours);
                        break;

                    case "Time_TotalMinutes":
                        TimeManager.Now.TotalMinutes = long.Parse(reader.Value);
                        break;

                    case "Time_Minutes":
                        TimeManager.Now.Minutes = long.Parse(reader.Value);
                        break;

                    case "Time_TotalSeconds":
                        TimeManager.Now.TotalSeconds = long.Parse(reader.Value);
                        break;

                    case "Time_Seconds":
                        TimeManager.Now.Seconds = long.Parse(reader.Value);
                        break;

                    case "Time_TotalMilliseconds":
                        TimeManager.Now.TotalMilliseconds = long.Parse(reader.Value);
                        break;

                    case "Time_Milliseconds":
                        TimeManager.Now.Milliseconds = long.Parse(reader.Value);
                        break;
                }
            }
        }

        private static void VisitShops(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Shops" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "ShopProperties":
                        VisitShop(reader);
                        break;

                    case "Items":
                        Inventory inventory = new Inventory();
                        VisitItems(reader, inventory);
                        Handler.ShopInventories[Handler.ShopInventories.Count - 1] = inventory;
                        break;
                }
            }
        }

        private static void VisitShop(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Level":
                        Handler.ShopInventories.Add(int.Parse(reader.Value), new Inventory());
                        break;
                }
            }
        }

        private static void VisitItems(XmlTextReader reader, Inventory inventory)
        {
            while (reader.Read())
            {
                if (reader.Name == "Items" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Item":
                        VisitItem(reader, inventory);
                        break;
                }
            }
        }

        private static void VisitItem(XmlTextReader reader, Inventory inventory)
        {
            Item item = null;

            while (reader.Read())
            {
                if (reader.Name == "Item" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "ItemProperties":
                        item = new Item();
                        VisitItemProperties(reader, item);
                        inventory.Items.Add(item);
                        break;

                    case "Properties":
                        VisitProperties(reader, item);
                        break;

                    case "Attachments":
                        VisitItemAttachments(reader, item);
                        break;
                }
            }
        }

        private static void VisitItemProperties(XmlTextReader reader, Item item)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        item.ID = int.Parse(reader.Value);
                        break;

                    case "Name":
                        item.Name = reader.Value;
                        break;

                    case "Description":
                        item.Description = reader.Value;
                        break;

                    case "Type":
                        item.Type = reader.Value;
                        break;

                    case "Amount":
                        item.Amount = int.Parse(reader.Value);
                        break;

                    case "Buy_Price":
                        item.Buy_Price = float.Parse(reader.Value);
                        break;

                    case "Equipped":
                        item.Equipped = reader.Value == "True";
                        break;

                    case "Texture":
                        item.Texture = AssetManager.Textures[reader.Value];
                        item.Image = new Rectangle(0, 0, item.Texture.Width / 4, item.Texture.Height);
                        break;

                    case "DrawColor":
                        string[] color_parts = reader.Value.Split(',');

                        int R = int.Parse(color_parts[0]);
                        int G = int.Parse(color_parts[1]);
                        int B = int.Parse(color_parts[2]);

                        item.DrawColor = new Color(R, G, B, 255);
                        break;

                    case "Visible":
                        item.Visible = reader.Value == "True";
                        break;

                    case "Region":
                        string[] region_parts = reader.Value.Split(',');
                        item.Region = new Region(float.Parse(region_parts[0]), float.Parse(region_parts[1]), float.Parse(region_parts[2]), 
                            float.Parse(region_parts[3]));
                        break;

                    case "Icon":
                        item.Icon = AssetManager.Textures[reader.Value];
                        item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
                        item.Icon_DrawColor = Color.White;
                        break;

                    case "Icon_Visible":
                        item.Icon_Visible = reader.Value == "True";
                        break;

                    case "Icon_Region":
                        string[] icon_region_parts = reader.Value.Split(',');
                        item.Icon_Region = new Region(float.Parse(icon_region_parts[0]), float.Parse(icon_region_parts[1]), float.Parse(icon_region_parts[2]),
                            float.Parse(icon_region_parts[3]));
                        break;

                    case "Location":
                        string[] location_parts = reader.Value.Split(',');
                        item.Location = new Location(float.Parse(location_parts[0]), float.Parse(location_parts[1]), 0);
                        break;

                    case "Material":
                        item.Materials.Add(reader.Value);
                        break;

                    case "Category":
                        item.Categories.Add(reader.Value);
                        break;
                }
            }
        }

        private static void VisitProperties(XmlTextReader reader, Item item)
        {
            while (reader.Read())
            {
                if (reader.Name == "Properties" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Property":
                        Something property = new Something();
                        VisitProperty(reader, property);
                        item.Properties.Add(property);
                        break;
                }
            }
        }

        private static void VisitProperty(XmlTextReader reader, Something property)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        property.ID = int.Parse(reader.Value);
                        break;

                    case "Name":
                        property.Name = reader.Value;
                        break;

                    case "Type":
                        property.Type = reader.Value;
                        break;

                    case "Assignment":
                        property.Assignment = reader.Value;
                        break;

                    case "Value":
                        property.Value = int.Parse(reader.Value);
                        break;

                    case "Max_Value":
                        property.Max_Value = int.Parse(reader.Value);
                        break;
                }
            }
        }

        private static void VisitItemAttachments(XmlTextReader reader, Item item)
        {
            while (reader.Read())
            {
                if (reader.Name == "Attachments" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Attachment":
                        VisitItemAttachment(reader, item);
                        break;
                }
            }
        }

        private static void VisitItemAttachment(XmlTextReader reader, Item item)
        {
            Item attachment = null;

            while (reader.Read())
            {
                if (reader.Name == "Attachment" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "AttachmentProperties":
                        attachment = new Item();
                        VisitItemProperties(reader, attachment);
                        item.Attachments.Add(attachment);
                        break;

                    case "Properties":
                        VisitProperties(reader, attachment);
                        break;
                }
            }
        }

        private static void VisitAcademies(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Academies" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "AcademyProperties":
                        VisitAcademy(reader);
                        break;

                    case "Characters":
                        VisitCharacters(reader, Handler.AcademyRecruits[Handler.AcademyRecruits.Count - 1]);
                        break;
                }
            }
        }

        private static void VisitAcademy(XmlTextReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Level":
                        Handler.AcademyRecruits.Add(int.Parse(reader.Value), new Squad());
                        break;
                }
            }
        }

        private static void VisitCharacters(XmlTextReader reader, Squad squad)
        {
            Character character = null;

            while (reader.Read())
            {
                if (reader.Name == "Characters" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "CharacterProperties":
                        character = new Character();

                        int xp = 10;
                        for (int i = 2; i <= 100; i++)
                        {
                            character.XP_Needed_ForLevels.Add(i, xp);
                            xp += 2;
                        }

                        VisitCharacter(reader, character);

                        squad.Characters.Add(character);
                        break;

                    case "Stats":
                        VisitStats(reader, character);
                        break;

                    case "StatusEffects":
                        VisitStatusEffects(reader, character);
                        break;

                    case "Inventory":
                        VisitInventory(reader, character);
                        break;
                }
            }
        }

        private static void VisitCharacter(XmlTextReader reader, Character character)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        character.ID = int.Parse(reader.Value);
                        break;

                    case "Name":
                        character.Name = reader.Value;
                        break;

                    case "Type":
                        character.Type = reader.Value;
                        break;

                    case "Level":
                        character.Level = int.Parse(reader.Value);
                        break;

                    case "Texture":
                        character.Texture = AssetManager.Textures[reader.Value];
                        character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);
                        character.DrawColor = Color.White;
                        break;

                    case "Formation":
                        string[] formation_parts = reader.Value.Split(',');
                        character.Formation = new Vector2(float.Parse(formation_parts[0]), float.Parse(formation_parts[1]));
                        break;

                    case "Region":
                        string[] region_parts = reader.Value.Split(',');
                        character.Region = new Region(float.Parse(region_parts[0]), float.Parse(region_parts[1]), float.Parse(region_parts[2]),
                            float.Parse(region_parts[3]));
                        break;

                    case "Direction":
                        character.Direction = (Direction)Enum.Parse(typeof(Direction), reader.Value);
                        break;

                    case "HP_Value":
                        character.HealthBar.Base_Texture = AssetManager.Textures["ProgressBase"];
                        character.HealthBar.Bar_Texture = AssetManager.Textures["ProgressBar"];
                        character.HealthBar.Bar_Image = new Rectangle(0, 0, 0, character.HealthBar.Base_Texture.Height);
                        character.HealthBar.DrawColor = Color.Red;
                        character.HealthBar.Max_Value = 100;
                        character.HealthBar.Value = float.Parse(reader.Value);
                        character.HealthBar.Update();
                        break;

                    case "EP_Value":
                        character.ManaBar.Base_Texture = AssetManager.Textures["ProgressBase"];
                        character.ManaBar.Bar_Texture = AssetManager.Textures["ProgressBar"];
                        character.ManaBar.Bar_Image = new Rectangle(0, 0, 0, character.HealthBar.Base_Texture.Height);
                        character.ManaBar.DrawColor = Color.Blue;
                        character.ManaBar.Max_Value = 100;
                        character.ManaBar.Value = float.Parse(reader.Value);
                        character.ManaBar.Update();
                        break;
                }
            }
        }

        private static void VisitStats(XmlTextReader reader, Character character)
        {
            while (reader.Read())
            {
                if (reader.Name == "Stats" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Stat":
                        Something stat = new Something();
                        VisitStat(reader, stat);
                        character.Stats.Add(stat);
                        break;
                }
            }
        }

        private static void VisitStat(XmlTextReader reader, Something stat)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        stat.Name = reader.Value;
                        break;

                    case "Value":
                        stat.Value = int.Parse(reader.Value);
                        stat.Max_Value = 100;
                        break;
                }
            }
        }

        private static void VisitStatusEffects(XmlTextReader reader, Character character)
        {
            while (reader.Read())
            {
                if (reader.Name == "StatusEffects" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "StatusEffect":
                        Something statusEffect = new Something();
                        VisitStatusEffect(reader, statusEffect);
                        character.StatusEffects.Add(statusEffect);
                        break;
                }
            }
        }

        private static void VisitStatusEffect(XmlTextReader reader, Something statusEffect)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "Name":
                        statusEffect.Name = reader.Value;
                        break;

                    case "Amount":
                        statusEffect.Amount = int.Parse(reader.Value);
                        break;

                    case "Value":
                        statusEffect.Value = float.Parse(reader.Value);
                        break;
                }
            }
        }

        private static void VisitInventory(XmlTextReader reader, Character character)
        {
            while (reader.Read())
            {
                if (reader.Name == "Inventory" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "InventoryProperties":
                        VisitInventoryProperties(reader, character.Inventory);
                        break;

                    case "Items":
                        VisitItems(reader, character.Inventory);
                        break;
                }
            }
        }

        private static void VisitInventoryProperties(XmlTextReader reader, Inventory inventory)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        inventory.ID = long.Parse(reader.Value);
                        break;

                    case "Name":
                        inventory.Name = reader.Value;
                        break;
                }
            }
        }

        #endregion

        #region Armies

        private static void ParseArmies(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Armies":
                                VisitArmies(reader);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game.CrashHandler(e);
                }
            }
        }

        private static void VisitArmies(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Armies" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Army":
                        Army army = new Army();
                        VisitArmy(reader, army);
                        CharacterManager.Armies.Add(army);
                        break;
                }
            }
        }

        private static void VisitArmy(XmlTextReader reader, Army army)
        {
            while (reader.Read())
            {
                if (reader.Name == "Army" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "ArmyProperties":
                        VisitArmyProperties(reader, army);
                        break;

                    case "Squads":
                        VisitSquads(reader, army);
                        break;
                }
            }
        }

        private static void VisitArmyProperties(XmlTextReader reader, Army army)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        army.ID = int.Parse(reader.Value);
                        break;

                    case "Name":
                        army.Name = reader.Value;
                        break;
                }
            }
        }

        private static void VisitSquads(XmlTextReader reader, Army army)
        {
            while (reader.Read())
            {
                if (reader.Name == "Squads" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Squad":
                        Squad squad = new Squad();
                        VisitSquad(reader, squad);
                        army.Squads.Add(squad);
                        break;
                }
            }
        }

        private static void VisitSquad(XmlTextReader reader, Squad squad)
        {
            while (reader.Read())
            {
                if (reader.Name == "Squad" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "SquadProperties":
                        VisitSquadProperties(reader, squad);
                        break;

                    case "Path":
                        VisitPath(reader, squad);
                        break;

                    case "Characters":
                        VisitCharacters(reader, squad);
                        break;
                }
            }
        }

        private static void VisitSquadProperties(XmlTextReader reader, Squad squad)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        squad.ID = int.Parse(reader.Value);
                        break;

                    case "Leader_ID":
                        squad.Leader_ID = int.Parse(reader.Value);
                        break;

                    case "Name":
                        squad.Name = reader.Value;
                        break;

                    case "Type":
                        squad.Type = reader.Value;
                        break;

                    case "Texture":
                        squad.Texture = AssetManager.Textures[reader.Value];
                        squad.Image = new Rectangle(0, 0, squad.Texture.Width, squad.Texture.Height);
                        break;

                    case "Region":
                        string[] region_parts = reader.Value.Split(',');
                        squad.Region = new Region(float.Parse(region_parts[0]), float.Parse(region_parts[1]), float.Parse(region_parts[2]),
                            float.Parse(region_parts[3]));
                        break;

                    case "Active":
                        squad.Active = reader.Value == "True";
                        break;

                    case "Assignment":
                        squad.Assignment = reader.Value;
                        break;

                    case "Location":
                        string[] location_parts = reader.Value.Split(',');
                        squad.Location = new Location(float.Parse(location_parts[0]), float.Parse(location_parts[1]), 0);
                        break;

                    case "Destination":
                        string[] destination_parts = reader.Value.Split(',');
                        squad.Destination = new Vector3(float.Parse(destination_parts[0]), float.Parse(destination_parts[1]), 0);
                        break;

                    case "Coordinates":
                        string[] coordinates_parts = reader.Value.Split(',');
                        squad.Coordinates = new Location(float.Parse(coordinates_parts[0]), float.Parse(coordinates_parts[1]), 0);
                        break;

                    case "Moving":
                        squad.Moving = reader.Value == "True";
                        break;

                    case "Moved":
                        squad.Moved = float.Parse(reader.Value);
                        break;

                    case "Move_TotalDistance":
                        squad.Move_TotalDistance = float.Parse(reader.Value);
                        break;

                    case "Speed":
                        squad.Speed = float.Parse(reader.Value);
                        break;

                    case "Visible":
                        squad.Visible = reader.Value == "True";
                        break;
                }
            }
        }

        private static void VisitPath(XmlTextReader reader, Squad squad)
        {
            while (reader.Read())
            {
                if (reader.Name == "Path" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "ALocation":
                        ALocation alocation = new ALocation();
                        VisitALocation(reader, alocation);
                        squad.Path.Add(alocation);
                        break;
                }
            }
        }

        private static void VisitALocation(XmlTextReader reader, ALocation alocation)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "X":
                        alocation.X = int.Parse(reader.Value);
                        break;

                    case "Y":
                        alocation.Y = int.Parse(reader.Value);
                        break;
                }
            }
        }

        #endregion

        #region Inventory

        private static void ParseInventory(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Inventory":
                                VisitAllyInventory(reader);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game.CrashHandler(e);
                }
            }
        }

        private static void VisitAllyInventory(XmlTextReader reader)
        {
            Inventory inventory = null;

            while (reader.Read())
            {
                if (reader.Name == "Inventory" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "InventoryProperties":
                        inventory = new Inventory();
                        VisitInventoryProperties(reader, inventory);

                        bool exists = false;
                        foreach (Inventory existing in InventoryManager.Inventories)
                        {
                            if (existing.Name == inventory.Name)
                            {
                                exists = true;
                                inventory = existing;
                                inventory.Items.Clear();
                                break;
                            }
                        }

                        if (!exists)
                        {
                            InventoryManager.Inventories.Add(inventory);
                        }
                        
                        break;

                    case "Items":
                        VisitItems(reader, inventory);
                        break;
                }
            }
        }

        #endregion

        #region World

        private static void ParseWorlds(string file)
        {
            using (XmlTextReader reader = new XmlTextReader(File.OpenRead(file)))
            {
                try
                {
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            case "Worlds":
                                VisitWorlds(reader);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Main.Game.CrashHandler(e);
                }
            }
        }

        private static void VisitWorlds(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.Name == "Worlds" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Worldmap":
                        World worldmap = new World();
                        VisitWorldmap(reader, worldmap);
                        SceneManager.GetScene("Worldmap").World = worldmap;
                        break;

                    case "Localmap":
                        World localmap = new World();
                        VisitLocalmap(reader, localmap);
                        SceneManager.GetScene("Localmap").World = localmap;
                        break;
                }
            }
        }

        private static void VisitWorldmap(XmlTextReader reader, World world)
        {
            while (reader.Read())
            {
                if (reader.Name == "Worldmap" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "WorldProperties":
                        VisitWorldProperties(reader, world);
                        break;

                    case "Maps":
                        VisitMaps(reader, world);
                        break;
                }
            }
        }

        private static void VisitLocalmap(XmlTextReader reader, World world)
        {
            while (reader.Read())
            {
                if (reader.Name == "Localmap" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "WorldProperties":
                        VisitWorldProperties(reader, world);
                        break;

                    case "Maps":
                        VisitMaps(reader, world);
                        break;
                }
            }
        }

        private static void VisitWorldProperties(XmlTextReader reader, World world)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        world.ID = long.Parse(reader.Value);
                        break;

                    case "Visible":
                        world.Visible = reader.Value == "True";
                        break;
                }
            }
        }

        private static void VisitMaps(XmlTextReader reader, World world)
        {
            while (reader.Read())
            {
                if (reader.Name == "Maps" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Map":
                        Map map = new Map();
                        VisitMap(reader, map);
                        world.Maps.Add(map);

                        WorldGen.AlignRegions(map);
                        break;
                }
            }
        }

        private static void VisitMap(XmlTextReader reader, Map map)
        {
            while (reader.Read())
            {
                if (reader.Name == "Map" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "MapProperties":
                        VisitMapProperties(reader, map);
                        break;

                    case "Layers":
                        VisitLayers(reader, map);
                        break;
                }
            }
        }

        private static void VisitMapProperties(XmlTextReader reader, Map map)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        map.ID = long.Parse(reader.Value);
                        break;

                    case "Name":
                        map.Name = reader.Value;
                        break;

                    case "Type":
                        map.Type = reader.Value;
                        break;

                    case "WorldID":
                        map.WorldID = long.Parse(reader.Value);
                        break;

                    case "Visible":
                        map.Visible = reader.Value == "True";
                        break;
                }
            }
        }

        private static void VisitLayers(XmlTextReader reader, Map map)
        {
            while (reader.Read())
            {
                if (reader.Name == "Layers" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "Layer":
                        Layer layer = new Layer();
                        VisitLayer(reader, layer);
                        map.Layers.Add(layer);
                        break;
                }
            }
        }

        private static void VisitLayer(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "Layer" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "LayerProperties":
                        VisitLayerProperties(reader, layer);
                        break;

                    case "Tiles":
                        VisitTiles(reader, layer);
                        break;
                }
            }
        }

        private static void VisitLayerProperties(XmlTextReader reader, Layer layer)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        layer.ID = long.Parse(reader.Value);
                        break;

                    case "Name":
                        layer.Name = reader.Value;
                        break;

                    case "WorldID":
                        layer.WorldID = long.Parse(reader.Value);
                        break;

                    case "MapID":
                        layer.MapID = long.Parse(reader.Value);
                        break;

                    case "Rows":
                        layer.Rows = int.Parse(reader.Value);
                        break;

                    case "Columns":
                        layer.Columns = int.Parse(reader.Value);
                        break;

                    case "Visible":
                        layer.Visible = reader.Value == "True";
                        break;
                }
            }
        }

        private static void VisitTiles(XmlTextReader reader, Layer layer)
        {
            while (reader.Read())
            {
                if (reader.Name == "Tiles" && reader.NodeType == XmlNodeType.EndElement)
                    break;

                switch (reader.Name)
                {
                    case "TileProperties":
                        Tile tile = new Tile();
                        VisitTileProperties(reader, tile);
                        layer.Tiles.Add(tile);
                        break;
                }
            }
        }

        private static void VisitTileProperties(XmlTextReader reader, Tile tile)
        {
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "ID":
                        tile.ID = long.Parse(reader.Value);
                        break;

                    case "Name":
                        tile.Name = reader.Value;
                        break;

                    case "WorldID":
                        tile.WorldID = long.Parse(reader.Value);
                        break;

                    case "MapID":
                        tile.MapID = long.Parse(reader.Value);
                        break;

                    case "LayerID":
                        tile.LayerID = long.Parse(reader.Value);
                        break;

                    case "Type":
                        tile.Type = reader.Value;
                        break;

                    case "Location":
                        string[] location_parts = reader.Value.Split(',');
                        tile.Location = new Location(float.Parse(location_parts[0]), float.Parse(location_parts[1]), 0);
                        break;

                    case "Texture":
                        tile.Texture = AssetManager.Textures[reader.Value];

                        if (tile.Type == "Water")
                        {
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width / 4, tile.Texture.Height);
                        }
                        else
                        {
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        }
                        break;

                    case "Region":
                        string[] region_parts = reader.Value.Split(',');
                        tile.Region = new Region(float.Parse(region_parts[0]), float.Parse(region_parts[1]), float.Parse(region_parts[2]),
                            float.Parse(region_parts[3]));
                        break;

                    case "Visible":
                        tile.Visible = reader.Value == "True";
                        break;
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
