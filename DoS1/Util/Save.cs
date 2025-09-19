using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class Save
    {
        #region Variables

        private static Stream SaveStream;
        private static XmlWriter Writer;

        #endregion

        #region Constructors



        #endregion

        #region Methods

        #region XML Methods

        private static void OpenStream(string path)
        {
            SaveStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "\t";
            Writer = XmlWriter.Create(SaveStream, xmlWriterSettings);
            Writer.WriteStartDocument();
        }

        private static void EnterNode(string elementName)
        {
            Writer.WriteStartElement(elementName);
        }

        private static void ExitNode()
        {
            Writer.WriteEndElement();
        }

        private static void CloseStream()
        {
            Writer.WriteEndDocument();
            Writer.Close();
            SaveStream.Close();
        }

        #endregion

        #region Export INI

        public static void ExportINI()
        {
            try
            {
                string file = AssetManager.Files["Config"];
                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                }

                OpenStream(file);
                SaveINI();
            }
            catch (Exception e)
            {
                Main.Game.CrashHandler(e);
            }
            finally
            {
                CloseStream();
            }
            GC.Collect();
        }

        private static void SaveINI()
        {
            EnterNode("Game");

            #region Options

            EnterNode("Options");
            Writer.WriteAttributeString("Fullscreen", Main.Game.GraphicsManager.IsFullScreen.ToString());
            Writer.WriteAttributeString("Tutorials", Handler.Tutorials.ToString());
            Writer.WriteAttributeString("MusicEnabled", SoundManager.MusicEnabled.ToString());
            Writer.WriteAttributeString("MusicVolume", (SoundManager.MusicVolume * 10).ToString());
            Writer.WriteAttributeString("AmbientEnabled", SoundManager.AmbientEnabled.ToString());
            Writer.WriteAttributeString("AmbientVolume", (SoundManager.AmbientVolume * 10).ToString());
            Writer.WriteAttributeString("SoundEnabled", SoundManager.SoundEnabled.ToString());
            Writer.WriteAttributeString("SoundVolume", (SoundManager.SoundVolume * 10).ToString());
            ExitNode();

            #endregion

            #region Speed

            EnterNode("Speed");
            Writer.WriteAttributeString("TimeSpeed", Main.TimeSpeed.ToString());
            Writer.WriteAttributeString("CombatSpeed", Main.CombatSpeed.ToString());
            ExitNode();

            #endregion

            ExitNode();
        }

        #endregion

        #region Save Game

        public static void SaveGame()
        {
            string saveDir = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save);
            
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
                Handler.Saves.Add(Handler.Selected_Save);
            }
            else
            {
                //Dump current save to re-export data
                Directory.Delete(saveDir, true);
                Directory.CreateDirectory(saveDir);
            }

            ExportGame();
            ExportArmies();
            ExportInventory();
            ExportWorld();
            ExportPortrait(saveDir);
        }

        private static void ExportGame()
        {
            try
            {
                string file = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save, "game.dat");
                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                }

                OpenStream(file);
                SaveMetaData();
            }
            catch (Exception e)
            {
                Main.Game.CrashHandler(e);
            }
            finally
            {
                CloseStream();
            }
            GC.Collect();
        }

        private static void SaveMetaData()
        {
            EnterNode("Game");

            EnterNode("GameProperties");
            Writer.WriteAttributeString("ID", Handler.ID.ToString());
            Writer.WriteAttributeString("Gold", Handler.Gold.ToString());
            Writer.WriteAttributeString("MainCharacter_ID", Handler.MainCharacter_ID.ToString());

            Writer.WriteAttributeString("Tutorial_Worldmap", Handler.Tutorial_Worldmap.ToString());
            Writer.WriteAttributeString("Tutorial_Localmap", Handler.Tutorial_Localmap.ToString());
            Writer.WriteAttributeString("Tutorial_Shop", Handler.Tutorial_Shop.ToString());
            Writer.WriteAttributeString("Tutorial_Academy", Handler.Tutorial_Academy.ToString());
            Writer.WriteAttributeString("Tutorial_Army", Handler.Tutorial_Army.ToString());
            Writer.WriteAttributeString("Tutorial_Squad", Handler.Tutorial_Squad.ToString());
            Writer.WriteAttributeString("Tutorial_Character", Handler.Tutorial_Character.ToString());
            Writer.WriteAttributeString("Tutorial_Item", Handler.Tutorial_Item.ToString());

            Writer.WriteAttributeString("LocalMap", Handler.LocalMap.ToString());
            Writer.WriteAttributeString("Level", Handler.Level.ToString());

            Writer.WriteAttributeString("ManualPause", Handler.ManualPause.ToString());
            Writer.WriteAttributeString("LocalPause", Handler.LocalPause.ToString());

            Writer.WriteAttributeString("ItemFilter", Handler.ItemFilter);

            Writer.WriteAttributeString("Time_TotalYears", TimeManager.Now.TotalYears.ToString());
            Writer.WriteAttributeString("Time_Years", TimeManager.Now.Years.ToString());
            Writer.WriteAttributeString("Time_TotalMonths", TimeManager.Now.TotalMonths.ToString());
            Writer.WriteAttributeString("Time_Months", TimeManager.Now.Months.ToString());
            Writer.WriteAttributeString("Time_TotalDays", TimeManager.Now.TotalDays.ToString());
            Writer.WriteAttributeString("Time_Days", TimeManager.Now.Days.ToString());
            Writer.WriteAttributeString("Time_TotalHours", TimeManager.Now.TotalHours.ToString());
            Writer.WriteAttributeString("Time_Hours", TimeManager.Now.Hours.ToString());
            Writer.WriteAttributeString("Time_TotalMinutes", TimeManager.Now.TotalMinutes.ToString());
            Writer.WriteAttributeString("Time_Minutes", TimeManager.Now.Minutes.ToString());
            Writer.WriteAttributeString("Time_TotalSeconds", TimeManager.Now.TotalSeconds.ToString());
            Writer.WriteAttributeString("Time_Seconds", TimeManager.Now.Seconds.ToString());
            Writer.WriteAttributeString("Time_TotalMilliseconds", TimeManager.Now.TotalMilliseconds.ToString());
            Writer.WriteAttributeString("Time_Milliseconds", TimeManager.Now.Milliseconds.ToString());
            ExitNode();

            if (Handler.ShopInventories.Any())
            {
                EnterNode("Shops");
                foreach (KeyValuePair<int, Inventory> shop in Handler.ShopInventories)
                {
                    EnterNode("ShopProperties");
                    Writer.WriteAttributeString("Level", shop.Key.ToString());
                    ExitNode();

                    SaveItems(shop.Value);
                }
                ExitNode();
            }

            if (Handler.AcademyRecruits.Any())
            {
                EnterNode("Academies");
                foreach (KeyValuePair<int, Squad> academy in Handler.AcademyRecruits)
                {
                    EnterNode("AcademyProperties");
                    Writer.WriteAttributeString("Level", academy.Key.ToString());
                    ExitNode();

                    if (academy.Value.Characters.Any())
                    {
                        SaveCharacters(academy.Value);
                    }
                }
                ExitNode();
            }

            //Exit Game Node
            ExitNode();
        }

        private static void SaveItems(Inventory inventory)
        {
            EnterNode("Items");
            foreach (Item item in inventory.Items)
            {
                EnterNode("Item");

                EnterNode("ItemProperties");
                Writer.WriteAttributeString("ID", item.ID.ToString());
                Writer.WriteAttributeString("Name", item.Name);
                Writer.WriteAttributeString("Description", item.Description);
                Writer.WriteAttributeString("Type", item.Type);
                Writer.WriteAttributeString("Amount", item.Amount.ToString());
                Writer.WriteAttributeString("Buy_Price", item.Buy_Price.ToString());
                Writer.WriteAttributeString("Equipped", item.Equipped.ToString());

                if (item.Texture != null)
                {
                    Writer.WriteAttributeString("Texture", item.Texture.Name);
                }

                if (item.DrawColor != default)
                {
                    Writer.WriteAttributeString("DrawColor", item.DrawColor.R.ToString() + "," + item.DrawColor.G.ToString() + "," +
                        item.DrawColor.B.ToString());
                }
                else
                {
                    Writer.WriteAttributeString("DrawColor", "255,255,255");
                }

                Writer.WriteAttributeString("Visible", item.Visible.ToString());
                Writer.WriteAttributeString("Region", item.Region.X.ToString() + "," + item.Region.Y.ToString() + "," +
                    item.Region.Width.ToString() + "," + item.Region.Height.ToString());

                if (item.Icon != null)
                {
                    Writer.WriteAttributeString("Icon", item.Icon.Name);
                }

                Writer.WriteAttributeString("Icon_Visible", item.Icon_Visible.ToString());

                if (item.Icon_Region != null)
                {
                    Writer.WriteAttributeString("Icon_Region", item.Icon_Region.X.ToString() + "," + item.Icon_Region.Y.ToString() + "," +
                        item.Icon_Region.Width.ToString() + "," + item.Icon_Region.Height.ToString());
                }

                if (item.Location != null)
                {
                    Writer.WriteAttributeString("Location", item.Location.X.ToString() + "," + item.Location.Y.ToString());
                }

                if (item.Materials.Any())
                {
                    Writer.WriteAttributeString("Material", item.Materials[0].ToString());
                }

                if (item.Categories.Any())
                {
                    Writer.WriteAttributeString("Category", item.Categories[0].ToString());
                }
                ExitNode();

                SaveItemProperties(item);
                SaveAttachments(item);

                //Exit Item Node
                ExitNode();
            }
            ExitNode();
        }

        private static void SaveItemProperties(Item item)
        {
            if (item.Properties.Any())
            {
                EnterNode("Properties");
                foreach (Something property in item.Properties)
                {
                    EnterNode("Property");
                    Writer.WriteAttributeString("ID", property.ID.ToString());
                    Writer.WriteAttributeString("Name", property.Name);
                    Writer.WriteAttributeString("Type", property.Type);
                    Writer.WriteAttributeString("Assignment", property.Assignment);
                    Writer.WriteAttributeString("Value", property.Value.ToString());
                    Writer.WriteAttributeString("Max_Value", property.Max_Value.ToString());
                    ExitNode();
                }
                ExitNode();
            }
        }

        private static void SaveAttachments(Item item)
        {
            if (item.Attachments != null &&
                item.Attachments.Any())
            {
                EnterNode("Attachments");
                foreach (Item attachment in item.Attachments)
                {
                    EnterNode("Attachment");

                    EnterNode("AttachmentProperties");
                    Writer.WriteAttributeString("ID", attachment.ID.ToString());
                    Writer.WriteAttributeString("Name", attachment.Name);
                    Writer.WriteAttributeString("Description", attachment.Description);
                    Writer.WriteAttributeString("Type", attachment.Type);
                    Writer.WriteAttributeString("Amount", attachment.Amount.ToString());
                    Writer.WriteAttributeString("Buy_Price", attachment.Buy_Price.ToString());
                    Writer.WriteAttributeString("Equipped", attachment.Equipped.ToString());

                    if (attachment.Texture != null)
                    {
                        Writer.WriteAttributeString("Texture", attachment.Texture.Name);
                    }

                    Writer.WriteAttributeString("Visible", attachment.Visible.ToString());
                    Writer.WriteAttributeString("Region", attachment.Region.X.ToString() + "," + attachment.Region.Y.ToString() + "," +
                        attachment.Region.Width.ToString() + "," + attachment.Region.Height.ToString());

                    if (attachment.Icon != null)
                    {
                        Writer.WriteAttributeString("Icon", attachment.Icon.Name);
                    }

                    Writer.WriteAttributeString("Icon_Visible", attachment.Icon_Visible.ToString());

                    if (attachment.Icon_Region != null)
                    {
                        Writer.WriteAttributeString("Icon_Region", attachment.Icon_Region.X.ToString() + "," + attachment.Icon_Region.Y.ToString() + "," +
                            attachment.Icon_Region.Width.ToString() + "," + attachment.Icon_Region.Height.ToString());
                    }

                    if (attachment.Location != null)
                    {
                        Writer.WriteAttributeString("Location", attachment.Location.X.ToString() + "," + attachment.Location.Y.ToString());
                    }

                    if (attachment.Materials.Any())
                    {
                        Writer.WriteAttributeString("Material", attachment.Materials[0].ToString());
                    }

                    if (attachment.Categories.Any())
                    {
                        Writer.WriteAttributeString("Category", attachment.Categories[0].ToString());
                    }
                    ExitNode();

                    SaveItemProperties(attachment);

                    //Exit Attachment Node
                    ExitNode();
                }
                ExitNode();
            }
        }

        private static void ExportArmies()
        {
            try
            {
                string file = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save, "armies.dat");
                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                }

                OpenStream(file);
                SaveArmies();
            }
            catch (Exception e)
            {
                Main.Game.CrashHandler(e);
            }
            finally
            {
                CloseStream();
            }
            GC.Collect();
        }

        private static void SaveArmies()
        {
            EnterNode("Armies");

            foreach (Army army in CharacterManager.Armies)
            {
                EnterNode("Army");

                EnterNode("ArmyProperties");
                Writer.WriteAttributeString("ID", army.ID.ToString());
                Writer.WriteAttributeString("Name", army.Name);
                ExitNode();

                if (army.Squads.Any())
                {
                    SaveSquads(army);
                }

                //Exit Node Army
                ExitNode();
            }

            ExitNode();
        }

        private static void SaveSquads(Army army)
        {
            EnterNode("Squads");
            foreach (Squad squad in army.Squads)
            {
                EnterNode("Squad");

                EnterNode("SquadProperties");
                Writer.WriteAttributeString("ID", squad.ID.ToString());
                Writer.WriteAttributeString("Leader_ID", squad.Leader_ID.ToString());
                Writer.WriteAttributeString("Name", squad.Name);
                Writer.WriteAttributeString("Type", squad.Type);

                if (squad.Texture != null)
                {
                    Writer.WriteAttributeString("Texture", squad.Texture.Name);
                }

                if (squad.Region != null)
                {
                    Writer.WriteAttributeString("Region", squad.Region.X.ToString() + "," + squad.Region.Y.ToString() + "," +
                        squad.Region.Width.ToString() + "," + squad.Region.Height.ToString());
                }

                Writer.WriteAttributeString("Active", squad.Active.ToString());
                Writer.WriteAttributeString("Assignment", squad.Assignment);

                if (squad.Location != null)
                {
                    Writer.WriteAttributeString("Location", squad.Location.X.ToString() + "," + squad.Location.Y.ToString());
                }

                Writer.WriteAttributeString("Destination", squad.Destination.X.ToString() + "," + squad.Destination.Y.ToString());

                if (squad.Coordinates != null)
                {
                    Writer.WriteAttributeString("Coordinates", squad.Coordinates.X.ToString() + "," + squad.Coordinates.Y.ToString());
                }

                Writer.WriteAttributeString("Moving", squad.Moving.ToString());
                Writer.WriteAttributeString("Moved", squad.Moved.ToString());
                Writer.WriteAttributeString("Move_TotalDistance", squad.Move_TotalDistance.ToString());
                Writer.WriteAttributeString("Speed", squad.Speed.ToString());
                Writer.WriteAttributeString("Visible", squad.Visible.ToString());
                ExitNode();

                if (squad.Path.Any())
                {
                    EnterNode("Path");
                    foreach (ALocation location in squad.Path)
                    {
                        EnterNode("ALocation");
                        Writer.WriteAttributeString("X", location.X.ToString());
                        Writer.WriteAttributeString("Y", location.Y.ToString());
                        ExitNode();
                    }
                    ExitNode();
                }

                if (squad.Characters.Any())
                {
                    SaveCharacters(squad);
                }

                //Exit Squad Node
                ExitNode();
            }
            ExitNode();
        }

        private static void SaveCharacters(Squad squad)
        {
            EnterNode("Characters");
            foreach (Character character in squad.Characters)
            {
                EnterNode("Character");

                EnterNode("CharacterProperties");
                Writer.WriteAttributeString("ID", character.ID.ToString());
                Writer.WriteAttributeString("Name", character.Name);
                Writer.WriteAttributeString("Type", character.Type);
                Writer.WriteAttributeString("Level", character.Level.ToString());
                Writer.WriteAttributeString("Texture", character.Texture.Name);
                Writer.WriteAttributeString("Formation", character.Formation.X.ToString() + "," + character.Formation.Y.ToString());
                Writer.WriteAttributeString("Region", character.Region.X.ToString() + "," + character.Region.Y.ToString() + "," +
                    character.Region.Width.ToString() + "," + character.Region.Height.ToString());
                Writer.WriteAttributeString("Direction", character.Direction.ToString());
                Writer.WriteAttributeString("HP_Value", character.HealthBar.Value.ToString());
                Writer.WriteAttributeString("EP_Value", character.ManaBar.Value.ToString());
                ExitNode();

                EnterNode("Stats");
                foreach (Something stat in character.Stats)
                {
                    EnterNode("Stat");
                    Writer.WriteAttributeString("Name", stat.Name);
                    Writer.WriteAttributeString("Value", stat.Value.ToString());
                    ExitNode();
                }
                ExitNode();

                if (character.StatusEffects.Any())
                {
                    EnterNode("StatusEffects");
                    foreach (Something statusEffect in character.StatusEffects)
                    {
                        EnterNode("StatusEffect");
                        Writer.WriteAttributeString("Name", statusEffect.Name);
                        Writer.WriteAttributeString("Amount", statusEffect.Amount.ToString());
                        Writer.WriteAttributeString("Value", statusEffect.Value.ToString());
                        ExitNode();
                    }
                    ExitNode();
                }

                Inventory inventory = character.Inventory;
                EnterNode("Inventory");

                EnterNode("InventoryProperties");
                Writer.WriteAttributeString("ID", inventory.ID.ToString());
                Writer.WriteAttributeString("Name", inventory.Name.ToString());
                ExitNode();

                SaveItems(inventory);

                //Exit Inventory Node
                ExitNode();

                //Exit Character Node
                ExitNode();
            }
            ExitNode();
        }

        private static void ExportInventory()
        {
            try
            {
                string file = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save, "inventory.dat");
                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                }

                OpenStream(file);
                SaveInventory();
            }
            catch (Exception e)
            {
                Main.Game.CrashHandler(e);
            }
            finally
            {
                CloseStream();
            }
            GC.Collect();
        }

        private static void SaveInventory()
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");

            EnterNode("Inventory");

            EnterNode("InventoryProperties");
            Writer.WriteAttributeString("ID", inventory.ID.ToString());
            Writer.WriteAttributeString("Name", inventory.Name.ToString());
            ExitNode();

            SaveItems(inventory);

            //Exit Inventory Node
            ExitNode();
        }

        private static void ExportWorld()
        {
            try
            {
                string file = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save, "worlds.dat");
                if (!File.Exists(file))
                {
                    File.Create(file).Close();
                }

                OpenStream(file);
                SaveWorlds();
            }
            catch (Exception e)
            {
                Main.Game.CrashHandler(e);
            }
            finally
            {
                CloseStream();
            }
            GC.Collect();
        }

        private static void SaveWorlds()
        {
            EnterNode("Worlds");

            SaveWorldmap();
            SaveLocalmap();

            //Exit Worlds Node
            ExitNode();
        }

        private static void SaveWorldmap()
        {
            World world = SceneManager.GetScene("Worldmap").World;

            EnterNode("Worldmap");

            EnterNode("WorldProperties");
            Writer.WriteAttributeString("ID", world.ID.ToString());
            Writer.WriteAttributeString("Visible", world.Visible.ToString());
            ExitNode();

            SaveMaps(world);

            //Exit WorldMap Node
            ExitNode();
        }

        private static void SaveLocalmap()
        {
            World world = SceneManager.GetScene("Localmap").World;

            EnterNode("Localmap");

            EnterNode("WorldProperties");
            Writer.WriteAttributeString("ID", world.ID.ToString());
            Writer.WriteAttributeString("Visible", world.Visible.ToString());
            ExitNode();

            SaveMaps(world);

            //Exit WorldMap Node
            ExitNode();
        }

        private static void SaveMaps(World world)
        {
            EnterNode("Maps");
            foreach (Map map in world.Maps)
            {
                EnterNode("Map");

                EnterNode("MapProperties");
                Writer.WriteAttributeString("ID", map.ID.ToString());
                Writer.WriteAttributeString("Name", map.Name);
                Writer.WriteAttributeString("Type", map.Type);
                Writer.WriteAttributeString("WorldID", map.WorldID.ToString());
                Writer.WriteAttributeString("Visible", map.Visible.ToString());
                ExitNode();

                if (map.Layers.Any())
                {
                    EnterNode("Layers");
                    foreach (Layer layer in map.Layers)
                    {
                        EnterNode("Layer");

                        EnterNode("LayerProperties");
                        Writer.WriteAttributeString("ID", layer.ID.ToString());
                        Writer.WriteAttributeString("Name", layer.Name);
                        Writer.WriteAttributeString("WorldID", layer.WorldID.ToString());
                        Writer.WriteAttributeString("MapID", layer.MapID.ToString());
                        Writer.WriteAttributeString("Rows", layer.Rows.ToString());
                        Writer.WriteAttributeString("Columns", layer.Columns.ToString());
                        Writer.WriteAttributeString("Visible", layer.Visible.ToString());
                        ExitNode();

                        if (layer.Tiles.Any())
                        {
                            EnterNode("Tiles");
                            foreach (Tile tile in layer.Tiles)
                            {
                                EnterNode("TileProperties");
                                Writer.WriteAttributeString("ID", tile.ID.ToString());
                                Writer.WriteAttributeString("Name", tile.Name);
                                Writer.WriteAttributeString("WorldID", tile.WorldID.ToString());
                                Writer.WriteAttributeString("MapID", tile.MapID.ToString());
                                Writer.WriteAttributeString("LayerID", tile.LayerID.ToString());
                                Writer.WriteAttributeString("Type", tile.Type);
                                Writer.WriteAttributeString("Location", tile.Location.X.ToString() + "," + tile.Location.Y.ToString());
                                Writer.WriteAttributeString("Texture", tile.Texture.Name);
                                Writer.WriteAttributeString("Region", tile.Region.X.ToString() + "," + tile.Region.Y.ToString() + "," +
                                    tile.Region.Width.ToString() + "," + tile.Region.Height.ToString());
                                Writer.WriteAttributeString("Visible", tile.Visible.ToString());
                                ExitNode();
                            }
                            ExitNode();
                        }

                        //Exit Layer Node
                        ExitNode();
                    }
                    ExitNode();
                }

                //Exit Map Node
                ExitNode();
            }
            ExitNode();
        }

        private static void ExportPortrait(string saveDir)
        {
            string portraitFile = Path.Combine(saveDir, "portrait.png");

            //Set render target
            RenderTarget2D portrait = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y * 3);
            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(portrait);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Black);

            //Get lead character
            Character leader = CharacterManager.GetArmy("Ally").Squads[0].Characters[0];

            //Create dummy portrait box
            Picture portraitBox = new Picture
            {
                Texture = AssetManager.Textures["Spot"],
                Region = new Region(0, 0, Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y * 3)
            };

            //Render portrait to target
            Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            CharacterUtil.DrawCharacter_Portrait(Main.Game.SpriteBatch, portraitBox, leader);
            Main.Game.SpriteBatch.End();

            //Save file
            using (FileStream stream = File.OpenWrite(portraitFile))
            {
                portrait.SaveAsPng(stream, Main.Game.MenuSize.X * 2, Main.Game.MenuSize.Y * 2);
            }
        }

        #endregion

        #endregion
    }
}
