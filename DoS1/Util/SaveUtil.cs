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
using OP_Engine.Weathers;

namespace DoS1.Util
{
    public static class SaveUtil
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
            Writer.WriteAttributeString("AutoSave", Handler.AutoSave.ToString());
            Writer.WriteAttributeString("Fullscreen", Main.Game.GraphicsManager.IsFullScreen.ToString());
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
            Writer.WriteAttributeString("TimeSpeed", Handler.TimeSpeed.ToString());
            Writer.WriteAttributeString("CombatSpeed", Handler.CombatSpeed.ToString());
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
            Writer.WriteAttributeString("FriendCharacter_ID", Handler.FriendCharacter_ID.ToString());

            Writer.WriteAttributeString("StoryStep", Handler.StoryStep.ToString());

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

            Writer.WriteAttributeString("CurrentWeather", WeatherManager.CurrentWeather.ToString());
            Writer.WriteAttributeString("Transitioning", WeatherManager.Transitioning.ToString());
            Writer.WriteAttributeString("TransitionType", WeatherManager.TransitionType.ToString());
            Writer.WriteAttributeString("Lightning", WeatherManager.Lightning.ToString());

            if (WeatherManager.Transitioning)
            {
                Weather transitionWeather = WeatherManager.GetWeather_TransitioningTo();
                if (transitionWeather != null)
                {
                    Writer.WriteAttributeString("TransitionTime", transitionWeather.TransitionTime.ToString());
                }
            }
            else
            {
                Weather weather = WeatherManager.GetWeather(WeatherManager.CurrentWeather);
                Writer.WriteAttributeString("TransitionTime", weather.TransitionTime.ToString()); 
            }
            ExitNode();

            if (Handler.MarketInventories.Any())
            {
                EnterNode("Markets");
                foreach (KeyValuePair<int, Inventory> market in Handler.MarketInventories)
                {
                    EnterNode("MarketProperties");
                    Writer.WriteAttributeString("Level", market.Key.ToString());
                    ExitNode();

                    SaveItems(market.Value);
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

            int itemCount = inventory.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                Item item = inventory.Items[i];
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
                    if (item.Name.Contains("Eye"))
                    {
                        Color eyeColor = CharacterUtil.Get_EyeColor(item);
                        Writer.WriteAttributeString("DrawColor", eyeColor.R.ToString() + "," + eyeColor.G.ToString() + "," +
                            eyeColor.B.ToString());
                    }
                    else if (item.Name.Contains("Hair"))
                    {
                        Color hairColor = CharacterUtil.Get_HairColor(item);
                        Writer.WriteAttributeString("DrawColor", hairColor.R.ToString() + "," + hairColor.G.ToString() + "," +
                            hairColor.B.ToString());
                    }
                    else
                    {
                        Writer.WriteAttributeString("DrawColor", item.DrawColor.R.ToString() + "," + item.DrawColor.G.ToString() + "," +
                            item.DrawColor.B.ToString());
                    }
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

                int count = item.Properties.Count;
                for (int i = 0; i < count; i++)
                {
                    Something property = item.Properties[i];

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

                int count = item.Attachments.Count;
                for (int i = 0; i < count; i++)
                {
                    Item attachment = item.Attachments[i];
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

            int count = CharacterManager.Armies.Count;
            for (int i = 0; i < count; i++)
            {
                Army army = CharacterManager.Armies[i];
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

            int squadCount = army.Squads.Count;
            for (int s = 0; s < squadCount; s++)
            {
                Squad squad = army.Squads[s];
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

                    int pathCount = squad.Path.Count;
                    for (int p = 0; p < pathCount; p++)
                    {
                        ALocation location = squad.Path[p];

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

            int characterCount = squad.Characters.Count;
            for (int c = 0; c < characterCount; c++)
            {
                Character character = squad.Characters[c];

                EnterNode("Character");

                EnterNode("CharacterProperties");
                Writer.WriteAttributeString("ID", character.ID.ToString());
                Writer.WriteAttributeString("Name", character.Name);
                Writer.WriteAttributeString("Gender", character.Gender);
                Writer.WriteAttributeString("Type", character.Type);
                Writer.WriteAttributeString("Level", character.Level.ToString());
                Writer.WriteAttributeString("XP", character.XP.ToString());
                Writer.WriteAttributeString("Texture", character.Texture.Name);
                Writer.WriteAttributeString("Formation", character.Formation.X.ToString() + "," + character.Formation.Y.ToString());
                Writer.WriteAttributeString("Region", character.Region.X.ToString() + "," + character.Region.Y.ToString() + "," +
                    character.Region.Width.ToString() + "," + character.Region.Height.ToString());
                Writer.WriteAttributeString("Direction", character.Direction.ToString());
                Writer.WriteAttributeString("HP_Value", character.HealthBar.Value.ToString());
                Writer.WriteAttributeString("HP_Max_Value", character.HealthBar.Max_Value.ToString());
                Writer.WriteAttributeString("EP_Value", character.ManaBar.Value.ToString());
                Writer.WriteAttributeString("EP_Max_Value", character.ManaBar.Max_Value.ToString());
                ExitNode();

                EnterNode("Stats");

                int statCount = character.Stats.Count;
                for (int s = 0;  s < statCount; s++)
                {
                    Something stat = character.Stats[s];

                    EnterNode("Stat");
                    Writer.WriteAttributeString("Name", stat.Name);
                    Writer.WriteAttributeString("Value", stat.Value.ToString());
                    ExitNode();
                }
                ExitNode();

                if (character.StatusEffects.Any())
                {
                    EnterNode("StatusEffects");

                    int statusEffectCount = character.StatusEffects.Count;
                    for (int e = 0;  e < statusEffectCount; e++)
                    {
                        Something statusEffect = character.StatusEffects[e];

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

            int worldCount = world.Maps.Count;
            for (int m = 0; m < worldCount; m++)
            {
                Map map = world.Maps[m];
                EnterNode("Map");

                EnterNode("MapProperties");
                Writer.WriteAttributeString("ID", map.ID.ToString());
                Writer.WriteAttributeString("Name", map.Name);
                Writer.WriteAttributeString("Type", map.Type);
                Writer.WriteAttributeString("Visible", map.Visible.ToString());
                ExitNode();

                if (map.Layers.Any())
                {
                    EnterNode("Layers");

                    int layerCount = map.Layers.Count;
                    for (int l  = 0; l < layerCount; l++)
                    {
                        Layer layer = map.Layers[l];
                        EnterNode("Layer");

                        EnterNode("LayerProperties");
                        Writer.WriteAttributeString("ID", layer.ID.ToString());
                        Writer.WriteAttributeString("Name", layer.Name);
                        Writer.WriteAttributeString("Rows", layer.Rows.ToString());
                        Writer.WriteAttributeString("Columns", layer.Columns.ToString());
                        Writer.WriteAttributeString("Visible", layer.Visible.ToString());
                        ExitNode();

                        if (layer.Tiles.Any())
                        {
                            EnterNode("Tiles");

                            int tileCount = layer.Tiles.Count;
                            for (int t  = 0; t < tileCount; t++)
                            {
                                Tile tile = layer.Tiles[t];
                                EnterNode("TileProperties");
                                Writer.WriteAttributeString("ID", tile.ID.ToString());
                                Writer.WriteAttributeString("Name", tile.Name);
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

        public static void ExportPortrait()
        {
            Main.Portrait = new RenderTarget2D(Main.Game.GraphicsManager.GraphicsDevice, Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y * 3);

            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(Main.Portrait);
            Main.Game.GraphicsManager.GraphicsDevice.Clear(Color.Black);

            Character leader = CharacterManager.GetArmy("Ally").Squads[0].Characters[0];

            Picture portraitBox = new Picture
            {
                Texture = AssetManager.Textures["Spot"],
                Region = new Region(0, 0, Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y * 3)
            };

            Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            CharacterUtil.DrawCharacter_Portrait(Main.Game.SpriteBatch, portraitBox, leader);
            Main.Game.SpriteBatch.End();

            Main.Game.GraphicsManager.GraphicsDevice.SetRenderTarget(null);
        }

        #endregion

        #endregion
    }
}
