using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Sounds;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Inputs;
using OP_Engine.Tiles;
using OP_Engine.Characters;
using OP_Engine.Weathers;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_UI : Menu
    {
        #region Variables

        private int mouseClickDelay = 0;
        private bool SelectingMultiple = false;

        #endregion

        #region Constructor

        public Menu_UI(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "UI";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Handler.Combat)
            {
                Active = false;
                Visible = false;
            }

            if (Visible &&
                !TimeManager.Paused)
            {
                Scene scene = WorldUtil.GetScene();

                if (string.IsNullOrEmpty(Handler.AlertType))
                {
                    UpdateControls(scene.World);
                }
                else
                {
                    UpdateAlert(scene.World);
                }

                if (!Handler.LocalPause)
                {
                    UpdateTime();
                    UpdateGold();
                }

                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Pictures)
                {
                    if (picture.Name != "Highlight" &&
                        picture.Name != "Select")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Select")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Buttons)
                {
                    if (button.Name != "Dialogue_Option1" &&
                        button.Name != "Dialogue_Option2" &&
                        button.Name != "Dialogue_Option3")
                    {
                        button.Draw(spriteBatch);
                    }
                }

                foreach (Slider slider in Sliders)
                {
                    slider.Draw(spriteBatch, slider.DrawColor);
                }

                foreach (InputBox input in Inputs)
                {
                    input.Draw(spriteBatch);
                }

                foreach (ProgressBar progressBar in ProgressBars)
                {
                    progressBar.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    label.Draw(spriteBatch);
                }

                foreach (Button button in Buttons)
                {
                    if (button.Name == "Dialogue_Option1" ||
                        button.Name == "Dialogue_Option2" ||
                        button.Name == "Dialogue_Option3")
                    {
                        button.Draw(spriteBatch);
                    }
                }

                Picture portrait1 = GetPicture("Dialogue_Portrait1");
                if (portrait1.Visible)
                {
                    CharacterUtil.DrawCharacter_Portrait(spriteBatch, portrait1, Handler.Dialogue_Character1);
                }

                Picture portrait2 = GetPicture("Dialogue_Portrait2");
                if (portrait2.Visible)
                {
                    CharacterUtil.DrawCharacter_Portrait(spriteBatch, portrait2, Handler.Dialogue_Character2);
                }
            }
        }

        private void UpdateControls(World world)
        {
            if (!Handler.MovingGrid &&
                !SelectingMultiple)
            {
                bool hovering_button = HoveringButton();
                Handler.Hovering_Squad = null;
                bool hovering_location = false;
                bool hovering_selection = false;

                if (!hovering_button)
                {
                    Map map = WorldUtil.GetMap(world);

                    Layer pathing = null;
                    if (map != null)
                    {
                        pathing = map.GetLayer("Pathing");
                    }

                    Handler.Hovering_Squad = HoveringSquad(world);
                    if (Handler.Hovering_Squad == null)
                    {
                        if (Handler.Selected_Token == -1 &&
                            pathing != null)
                        {
                            pathing.Visible = false;
                        }

                        hovering_location = HoveringLocation(map);
                    }
                    else if (Handler.Hovering_Squad.Type != "Enemy" &&
                             (Handler.Selected_Token != -1 &&
                             Handler.Hovering_Squad.ID != Handler.Selected_Token))
                    {
                        hovering_location = HoveringLocation(map);
                    }

                    if (Handler.Selected_Token != -1)
                    {
                        hovering_selection = HoveringSelection(world, map);
                        if (hovering_selection)
                        {
                            GetPicture("Select").Visible = true;
                        }
                    }

                    if (!hovering_selection &&
                        !hovering_location &&
                        Handler.Hovering_Squad == null)
                    {
                        GetPicture("Select").Visible = false;
                    }
                }

                if (!hovering_button &&
                    !hovering_location &&
                    Handler.Hovering_Squad == null)
                {
                    GetLabel("Examine").Visible = false;

                    if (Handler.Selected_Token == -1)
                    {
                        GetPicture("Highlight").Visible = false;
                    }
                }
            }
            else
            {
                GetLabel("Examine").Visible = false;

                if (SelectingMultiple)
                {
                    bool hovering_button = HoveringButton();

                    if (!hovering_button &&
                        InputManager.Mouse_LB_Pressed)
                    {
                        SelectingMultiple = false;
                        SelectToken_FromMultiple(-1);
                    }
                }

                if (InputManager.Mouse_LB_Pressed)
                {
                    Handler.MoveGridDelay = 0;
                    Handler.MovingGrid = false;
                }
            }

            if (InputManager.Mouse_LB_Held &&
                InputManager.Mouse.Moved)
            {
                Handler.MoveGridDelay++;

                if (Handler.MoveGridDelay >= 4)
                {
                    WorldUtil.MoveGrid(world);
                }
            }

            if (InputManager.Mouse_ScrolledUp)
            {
                WorldUtil.ZoomIn();
            }
            else if (InputManager.Mouse_ScrolledDown)
            {
                WorldUtil.ZoomOut();
            }

            if (InputManager.KeyPressed("Space"))
            {
                GameUtil.Toggle_Pause(true);
            }
            else if (InputManager.KeyPressed("Esc"))
            {
                TimeManager.Paused = true;
                Active = false;
                Visible = false;
                MenuManager.ChangeMenu("Main");
            }
            else if (InputManager.KeyPressed("Debug"))
            {
                if (!Main.Game.Debugging)
                {
                    Main.Game.Debugging = true;
                    Handler.Gold = 10000;
                    GetLabel("Debug").Visible = true;
                }
                else
                {
                    Main.Game.Debugging = false;
                    GetLabel("Debug").Visible = false;
                }
            }
        }

        private bool HoveringButton()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;

                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (button.Name == "Alert")
                        {
                            if (Handler.AlertType == "Combat")
                            {
                                GetLabel("Combat_Attacker").TextColor = button.TextColor_Selected;
                                GetLabel("Combat_VS").TextColor = button.TextColor_Selected;
                                GetLabel("Combat_Defender").TextColor = button.TextColor_Selected;
                            }
                        }

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;

                            CheckClick(button);

                            if (button.Name != "Alert")
                            {
                                button.Opacity = 0.8f;
                            }

                            button.Selected = false;

                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        if (button.Name != "Alert")
                        {
                            button.Opacity = 0.8f;
                        }
                            
                        button.Selected = false;

                        if (button.Name == "Alert")
                        {
                            if (Handler.AlertType == "Combat")
                            {
                                GetLabel("Combat_Attacker").TextColor = button.TextColor;
                                GetLabel("Combat_VS").TextColor = button.TextColor;
                                GetLabel("Combat_Defender").TextColor = button.TextColor;
                            }
                        }
                    }
                }
            }

            return found;
        }

        private Squad HoveringSquad(World world)
        {
            Squad hovered_squad = null;

            if (Handler.LocalMap)
            {
                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Visible)
                        {
                            Picture highlight = GetPicture("Highlight");
                            Picture select = GetPicture("Select");

                            if (InputManager.MouseWithin(squad.Region.ToRectangle))
                            {
                                hovered_squad = squad;

                                Map map = WorldUtil.GetMap(world);

                                if (Handler.Selected_Token == -1)
                                {
                                    highlight.Region = squad.Region;
                                    highlight.Visible = true;

                                    if (squad.Type == "Ally")
                                    {
                                        highlight.DrawColor = new Color(0, 0, 255, 255);
                                    }
                                    else if (squad.Type == "Enemy")
                                    {
                                        highlight.DrawColor = new Color(255, 0, 0, 255);
                                    }

                                    GameUtil.Examine(this, squad.Name);
                                }
                                else if (Handler.Selected_Token == squad.ID)
                                {
                                    select.Region = squad.Region;
                                    select.Visible = true;
                                    select.DrawColor = new Color(0, 255, 255, 255);

                                    map.GetLayer("Pathing").Visible = false;

                                    GameUtil.Examine(this, squad.Name);
                                }
                                else if (squad.Type == "Enemy")
                                {
                                    select.Region = squad.Region;
                                    select.Visible = true;
                                    select.DrawColor = new Color(255, 0, 0, 255);

                                    GameUtil.Examine(this, squad.Name);
                                }

                                if (InputManager.Mouse_LB_Pressed)
                                {
                                    hovered_squad = null;

                                    if (Handler.Selected_Token == squad.ID)
                                    {
                                        DeselectToken(map);
                                    }
                                    else if (squad.Type == "Ally" &&
                                             Handler.Selected_Token == -1)
                                    {
                                        SelectToken(army, squad);
                                    }
                                    else if (squad.Type == "Enemy" &&
                                             Handler.Selected_Token != -1)
                                    {
                                        SelectToken_Enemy(squad);
                                    }

                                    break;
                                }
                                else if (InputManager.Mouse_RB_Pressed)
                                {
                                    Handler.Selected_Squad = squad.ID;

                                    Handler.ViewOnly_Squad = true;
                                    Handler.ViewOnly_Character = true;
                                    Handler.ViewOnly_Item = true;

                                    Layer locations = map.GetLayer("Locations");
                                    Tile location_tile = locations.GetTile(squad.Destination);
                                    if (location_tile != null &&
                                        squad.Type == "Ally")
                                    {
                                        if (location_tile.Type.Contains("Shop") ||
                                            location_tile.Type.Contains("Academy") ||
                                            location_tile.Type.Contains("Ally"))
                                        {
                                            Handler.ViewOnly_Squad = false;
                                            Handler.ViewOnly_Character = false;
                                            Handler.ViewOnly_Item = false;
                                        }
                                    }

                                    GameUtil.Toggle_Pause(false);
                                    InputManager.Mouse.Flush();
                                    MenuManager.ChangeMenu("Squad");
                                }
                                else if (Handler.Selected_Token == -1)
                                {
                                    WorldUtil.DisplayPath_Squad(this, map, squad);
                                }
                            }
                            else if (Handler.Selected_Token == squad.ID)
                            {
                                highlight.DrawColor = new Color(0, 0, 255, 255);
                            }
                        }
                    }
                }
            }

            return hovered_squad;
        }

        private bool HoveringLocation(Map map)
        {
            bool found = false;

            if (map == null)
            {
                return false;
            }

            Layer locations = map.GetLayer("Locations");
            for (int i = 0; i < locations.Tiles.Count; i++)
            {
                Tile location = locations.Tiles[i];

                if (location.Visible)
                {
                    if (InputManager.MouseWithin(location.Region.ToRectangle))
                    {
                        found = true;

                        GameUtil.Examine(this, location.Name);

                        if (Handler.Selected_Token == -1)
                        {
                            Picture highlight = GetPicture("Highlight");
                            highlight.Region = location.Region;
                            highlight.Visible = true;
                            highlight.DrawColor = new Color(255, 255, 255, 255);
                        }
                        else
                        {
                            Picture select = GetPicture("Select");
                            select.Region = location.Region;
                            select.Visible = true;
                            select.DrawColor = new Color(0, 255, 0, 255);
                        }

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            if (map.Name == "World Map")
                            {
                                found = false;
                                CheckClick_Worldmap_Location(map, location, i);
                            }
                            
                            break;
                        }
                    }
                }
            }

            return found;
        }

        private bool HoveringSelection(World world, Map map)
        {
            bool found = false;

            if (map == null)
            {
                return false;
            }

            Layer ground = map.GetLayer("Ground");
            foreach (Tile tile in ground.Tiles)
            {
                if (InputManager.MouseWithin(tile.Region.ToRectangle))
                {
                    found = true;

                    if (Handler.Hovering_Squad == null ||
                        Handler.Hovering_Squad.Type != "Enemy")
                    {
                        Picture select = GetPicture("Select");
                        select.Region = tile.Region;
                        select.DrawColor = new Color(0, 255, 0, 255);
                    }

                    if (InputManager.Mouse_LB_Pressed)
                    {
                        found = false;
                        CheckClick_Selection(world, tile);
                        break;
                    }
                    else if (InputManager.Mouse_RB_Pressed)
                    {
                        DeselectToken(map);
                    }
                    else
                    {
                        WorldUtil.DisplayPath_Temp(map, tile);
                    }
                }
            }

            return found;
        }

        private void CheckClick_Worldmap_Location(Map map, Tile tile, int location_num)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.MoveGridDelay = 0;
            Handler.Level = location_num;

            Army enemies = CharacterManager.GetArmy("Enemy");

            Layer ground = map.GetLayer("Ground");
            Tile ground_tile = ground.GetTile(new Vector2(tile.Location.X, tile.Location.Y));

            Map localmap;

            World world = SceneManager.GetScene("Localmap").World;
            if (world.Maps.Count >= Handler.Level + 1)
            {
                //Revisit map
                localmap = world.Maps[Handler.Level];
                localmap.Visible = true;
            }
            else
            {
                //Generate new map
                if (!world.Maps.Any())
                {
                    world = new World
                    {
                        ID = Handler.GetID(),
                        Visible = true,
                        DrawColor = Color.White
                    };

                    SceneManager.GetScene("Localmap").World = world;
                }

                localmap = WorldGen.GenLocalmap(world, ground_tile, Handler.Level);
                world.Maps.Add(localmap);

                //Generate enemies
                ArmyUtil.Gen_EnemySquads(enemies, Handler.Level);
            }

            //Hide other maps
            for (int i = 0; i < world.Maps.Count; i++)
            {
                Map existing = world.Maps[i];
                if (existing.ID != localmap.ID)
                {
                    existing.Visible = false;
                }
            }

            if (localmap != null)
            {
                //Set shop inventories
                if (!Handler.ShopInventories.ContainsKey(Handler.Level))
                {
                    Handler.ShopInventories.Add(Handler.Level, InventoryUtil.Gen_Shop(Handler.Level + 1));
                }
                Handler.TradingShop = Handler.ShopInventories[Handler.Level];

                //Set academy units
                if (!Handler.AcademyRecruits.ContainsKey(Handler.Level))
                {
                    Handler.AcademyRecruits.Add(Handler.Level, ArmyUtil.Gen_Academy());
                }
                Handler.TradingAcademy = Handler.AcademyRecruits[Handler.Level];

                //Set weather
                if (ground_tile.Type.Contains("Snow") ||
                    ground_tile.Type.Contains("Ice"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Snow };
                }
                else if (!ground_tile.Type.Contains("Desert"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm };
                }

                Handler.LocalMap = true;

                GetButton("PlayPause").Enabled = true;
                GetButton("Speed").Enabled = true;

                //Set "Return to Worldmap" button
                Button worldMap = GetButton("Worldmap");
                worldMap.Visible = true;

                int maxLevelUnlocked = WorldUtil.MaxLevelUnlocked();
                if (maxLevelUnlocked > Handler.Level)
                {
                    worldMap.Enabled = true;
                }
                else
                {
                    worldMap.Enabled = false;
                }

                //Set starting squad at ally base
                Army allies = CharacterManager.GetArmy("Ally");
                Squad ally_squad = allies.Squads[0];
                WorldUtil.AllyToken_Start(ally_squad, localmap);

                //Set enemies at enemy base
                foreach (Squad enemy_squad in enemies.Squads)
                {
                    WorldUtil.EnemyToken_Start(enemy_squad, localmap);
                }

                //Switch to local map
                SceneManager.ChangeScene("Localmap");
                WorldUtil.Resize_OnStart(localmap);

                SoundManager.StopMusic();
                SoundManager.NeedMusic = true;
            }
        }

        private void CheckClick_Selection(World world, Tile tile)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.MoveGridDelay = 0;

            //Select tile in local map for pathing
            Map map = world.Maps[Handler.Level];
            ArmyUtil.SetPath(this, map, tile);

            InputManager.Mouse.Flush();
            GameUtil.Toggle_Pause(false);
        }

        private void SelectToken(Army army, Squad selected_squad)
        {
            List<Squad> potential_squads = new List<Squad>
            {
                selected_squad
            };

            foreach (Squad squad in army.Squads)
            {
                if (squad.Location.X == selected_squad.Location.X &&
                    squad.Location.Y == selected_squad.Location.Y &&
                    squad.ID != selected_squad.ID)
                {
                    potential_squads.Add(squad);
                }
            }

            if (potential_squads.Count > 1)
            {
                SelectingMultiple = true;

                for (int i = 0; i < potential_squads.Count; i++)
                {
                    Squad squad = potential_squads[i];

                    AddButton(new ButtonOptions
                    {
                        id = Handler.GetID(),
                        font = AssetManager.Fonts["ControlFont"],
                        name = "Squad_" + squad.ID,
                        text = squad.Name,
                        texture = AssetManager.Textures["ButtonFrame"],
                        texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                        region = new Region(selected_squad.Region.X + selected_squad.Region.Width, selected_squad.Region.Y + (Main.Game.MenuSize.Y * i), Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y),
                        draw_color = new Color(128, 128, 128, 255),
                        draw_color_selected = Color.White,
                        text_color = Color.White,
                        text_selected_color = Color.Red,
                        enabled = true,
                        visible = true
                    });
                }

                GameUtil.Toggle_Pause(false);
            }
            else
            {
                AssetManager.PlaySound_Random("Click");

                Handler.Selected_Token = selected_squad.ID;

                GameUtil.Toggle_Pause(false);
            }

            InputManager.Mouse.Flush();
        }

        private void SelectToken_FromMultiple(long id)
        {
            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(id);
            if (squad != null)
            {
                Handler.Selected_Token = squad.ID;
            }

            for (int i = 0; i < Buttons.Count; i++)
            {
                Button button = Buttons[i];
                if (button.Name.Contains("Squad"))
                {
                    Buttons.Remove(button);
                    i--;
                }
            }
        }

        private void DeselectToken(Map map)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Token = -1;

            GetPicture("Select").Visible = false;
            GetPicture("Highlight").Visible = true;
            GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);

            if (map != null)
            {
                Layer pathing = map.GetLayer("Pathing");
                pathing.Visible = false;
            }

            GameUtil.Toggle_Pause(false);
            InputManager.Mouse.Flush();
        }

        private void SelectToken_Enemy(Squad enemy_squad)
        {
            Squad ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Selected_Token);
            if (ally_squad != null)
            {
                Character leader = ally_squad.GetLeader();
                leader.Target_ID = enemy_squad.ID;
            }
        }

        private void UpdateAlert(World world)
        {
            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (button.Name == "Alert" ||
                        button.Name == "Dialogue_Option1" ||
                        button.Name == "Dialogue_Option2" ||
                        button.Name == "Dialogue_Option3")
                    {
                        if (InputManager.MouseWithin(button.Region.ToRectangle))
                        {
                            button.Selected = true;

                            if (button.Name == "Alert" &&
                                Handler.AlertType == "Combat")
                            {
                                GetLabel("Combat_Attacker").TextColor = button.TextColor_Selected;
                                GetLabel("Combat_VS").TextColor = button.TextColor_Selected;
                                GetLabel("Combat_Defender").TextColor = button.TextColor_Selected;
                            }

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                CheckClick(button);
                                button.Selected = false;
                                break;
                            }
                        }
                        else if (InputManager.Mouse.Moved)
                        {
                            button.Selected = false;

                            if (button.Name == "Alert" &&
                                Handler.AlertType == "Combat")
                            {
                                GetLabel("Combat_Attacker").TextColor = button.TextColor;
                                GetLabel("Combat_VS").TextColor = button.TextColor;
                                GetLabel("Combat_Defender").TextColor = button.TextColor;
                            }
                        }
                    }
                }
            }

            Map map = WorldUtil.GetMap(world);
            if (map != null)
            {
                Layer pathing = map.GetLayer("Pathing");
                if (pathing != null)
                {
                    pathing.Visible = false;
                }
            }

            GetLabel("Examine").Visible = false;
            GetPicture("Select").Visible = false;
            GetPicture("Highlight").Visible = false;

            AnimateMouseClick();
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            if (button.Name == "PlayPause")
            {
                GameUtil.Toggle_Pause(true);
            }
            else if (button.Name == "Speed")
            {
                SpeedToggle();
            }
            else if (button.Name == "Worldmap")
            {
                if (Handler.Selected_Token != -1)
                {
                    GetPicture("Select").Visible = false;
                    GetPicture("Highlight").Visible = false;

                    Scene scene = WorldUtil.GetScene();
                    DeselectToken(WorldUtil.GetMap(scene.World));
                }

                GameUtil.ReturnToWorldmap();
            }
            else if (button.Name == "Alert")
            {
                if (Handler.AlertType == "Combat")
                {
                    button.Visible = false;
                    GetLabel("Combat_Attacker").Visible = false;
                    GetLabel("Combat_VS").Visible = false;
                    GetLabel("Combat_Defender").Visible = false;
                    GetPicture("MouseClick").Visible = false;

                    Active = false;
                    Visible = false;

                    SoundManager.StopMusic();
                    SoundManager.NeedMusic = true;

                    Scene combat = SceneManager.GetScene("Combat");
                    combat.Load();

                    SceneManager.ChangeScene(combat);

                    Handler.Combat = true;
                    Handler.AlertType = "";
                    SoundManager.AmbientPaused = false;
                }
            }
            else if (button.Name == "Dialogue_Option1")
            {
                if (button.Text == "Enter Town")
                {
                    EnterTown();
                }
                else if (button.Text == "Retreat")
                {
                    Undeploy();
                    CloseDialogue();
                    GameUtil.Toggle_Pause(false);
                }
                else if (button.Text == "Claim Region")
                {
                    UnlockNextLocation();
                    CloseDialogue();
                    GameUtil.Toggle_Pause(false);
                }
                else if (button.Text == "(Continue)")
                {
                    CloseDialogue();
                    GameUtil.Toggle_Pause(false);
                }
                else if (button.Text == "(Retreat)")
                {
                    CloseDialogue();
                    GameUtil.ReturnToWorldmap();
                }
            }
            else if (button.Name == "Dialogue_Option2")
            {
                if (button.Text == "Move Out")
                {
                    CloseDialogue();
                    GameUtil.Toggle_Pause(false);
                }
            }
            else if (button.Name == "Dialogue_Option3")
            {

            }
            else if (button.Name.Contains("Squad"))
            {
                string[] parts = button.Name.Split('_');
                long id = long.Parse(parts[1]);
                SelectToken_FromMultiple(id);
            }
            else
            {
                TimeManager.Paused = true;
                SoundManager.AmbientPaused = true;

                Active = false;
                Visible = false;

                MenuManager.ChangeMenu(button.Name);

                InputManager.Mouse.Flush();
                InputManager.Keyboard.Flush();
            }
        }

        private void CloseDialogue()
        {
            Handler.AlertType = "";
            Handler.Dialogue_Character1 = null;
            Handler.Dialogue_Character2 = null;

            GetLabel("Dialogue").Visible = false;
            GetButton("Dialogue_Option1").Visible = false;
            GetButton("Dialogue_Option2").Visible = false;
            GetButton("Dialogue_Option3").Visible = false;
            GetPicture("Dialogue_Portrait1").Visible = false;
            GetPicture("Dialogue_Portrait2").Visible = false;
        }

        private void EnterTown()
        {
            string type = "Town";

            Squad squad = ArmyUtil.Get_Squad(Handler.Dialogue_Character1);
            if (squad != null)
            {
                Tile location = WorldUtil.GetLocation(squad);
                if (location != null)
                {
                    if (location.Type.Contains("Shop"))
                    {
                        type = "Shop";
                    }
                    else if (location.Type.Contains("Academy"))
                    {
                        type = "Academy";
                    }
                }
            }

            CloseDialogue();

            if (type == "Shop")
            {
                MenuManager.ChangeMenu("Shop");
            }
            else if (type == "Academy")
            {
                MenuManager.ChangeMenu("Academy");
            }
            else
            {
                GameUtil.Toggle_Pause(false);
            }
        }

        private void Undeploy()
        {
            Squad squad = ArmyUtil.Get_Squad(Handler.Dialogue_Character1);
            if (squad != null)
            {
                squad.Visible = false;
                squad.Active = false;
            }
        }

        private void UnlockNextLocation()
        {
            GetButton("Worldmap").Enabled = true;

            Scene scene = SceneManager.GetScene("Worldmap");
            Map map = scene.World.Maps[0];

            Layer ground = map.GetLayer("Ground");
            Layer locations = map.GetLayer("Locations");
            Layer roads = map.GetLayer("Roads");

            Tile currentLocation = locations.Tiles[Handler.Level];
            currentLocation.Type = "Base_Ally";
            currentLocation.Texture = AssetManager.Textures["Tile_Base_Ally"];

            Tile nextLocation = locations.Tiles[Handler.Level + 1];
            nextLocation.Visible = true;

            WorldGen.GenRoad(ground, roads, currentLocation, nextLocation, Direction.Nowhere);

            foreach (Tile road in roads.Tiles)
            {
                road.Visible = true;

                Tile location = WorldUtil.Get_Tile(locations, new Vector2(road.Location.X, road.Location.Y));
                if (location != null &&
                    location.Visible)
                {
                    road.Visible = false;
                }
            }

            WorldGen.AlignRegions(map);
        }

        private void UpdateTime()
        {
            long NewHours = TimeManager.Now.Hours;
            string hours;
            string minutes;

            bool pm = false;

            if (NewHours > 12)
            {
                NewHours = NewHours - 12;
                pm = true;
            }
            else if (NewHours == 0)
            {
                NewHours = 12;
            }
            else if (NewHours == 12)
            {
                pm = true;
            }

            if (NewHours < 10)
            {
                hours = "0" + NewHours.ToString();
            }
            else
            {
                hours = NewHours.ToString();
            }

            if (TimeManager.Now.Minutes < 10)
            {
                minutes = "0" + TimeManager.Now.Minutes.ToString();
            }
            else
            {
                minutes = TimeManager.Now.Minutes.ToString();
            }

            Label time = GetLabel("Time");
            Label date = GetLabel("Date");

            if (pm == false)
            {
                time.Text = hours + ":" + minutes + " AM";
            }
            else
            {
                time.Text = hours + ":" + minutes + " PM";
            }

            date.Text = "Day " + TimeManager.Now.Days.ToString();
        }

        private void UpdateGold()
        {
            Label gold = GetLabel("Gold");
            gold.Text = "Gold: " + Handler.Gold;
        }

        private void AnimateMouseClick()
        {
            Picture mouseClick = GetPicture("MouseClick");
            if (mouseClick.Visible)
            {
                if (mouseClickDelay >= 10)
                {
                    mouseClickDelay = 0;

                    int X = mouseClick.Image.X + mouseClick.Image.Height;
                    if (X >= mouseClick.Texture.Width)
                    {
                        X = 0;
                    }

                    mouseClick.Image = new Rectangle(X, mouseClick.Image.Y, mouseClick.Image.Width, mouseClick.Image.Height);
                }
                else
                {
                    mouseClickDelay++;
                }
            }
        }

        private void SpeedToggle()
        {
            Main.TimeSpeed++;
            if (Main.TimeSpeed >= 5)
            {
                Main.TimeSpeed = 1;
            }

            Button button = GetButton("Speed");

            if (Main.TimeSpeed == 1)
            {
                button.HoverText = "Time x1";
                button.Texture = AssetManager.Textures["Button_Speed1"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
            }
            else if (Main.TimeSpeed == 2)
            {
                button.HoverText = "Time x2";
                button.Texture = AssetManager.Textures["Button_Speed2"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
            }
            else if (Main.TimeSpeed == 3)
            {
                button.HoverText = "Time x3";
                button.Texture = AssetManager.Textures["Button_Speed3"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
            }
            else if (Main.TimeSpeed == 4)
            {
                button.HoverText = "Time x4";
                button.Texture = AssetManager.Textures["Button_Speed4"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
            }

            Save.ExportINI();
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Main",
                hover_text = "System",
                texture = AssetManager.Textures["Button_Menu"],
                texture_highlight = AssetManager.Textures["Button_Menu_Hover"],
                texture_disabled = AssetManager.Textures["Button_Menu_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Army",
                hover_text = "Army",
                texture = AssetManager.Textures["Button_Army"],
                texture_highlight = AssetManager.Textures["Button_Army_Hover"],
                texture_disabled = AssetManager.Textures["Button_Army_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Inventory",
                hover_text = "Inventory",
                texture = AssetManager.Textures["Button_Inventory"],
                texture_highlight = AssetManager.Textures["Button_Inventory_Hover"],
                texture_disabled = AssetManager.Textures["Button_Inventory_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Worldmap",
                hover_text = "Return to Worldmap",
                texture = AssetManager.Textures["Button_Exit"],
                texture_highlight = AssetManager.Textures["Button_Exit_Hover"],
                texture_disabled = AssetManager.Textures["Button_Exit_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = false
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "PlayPause",
                hover_text = "Pause",
                texture = AssetManager.Textures["Button_Pause"],
                texture_highlight = AssetManager.Textures["Button_Pause_Hover"],
                texture_disabled = AssetManager.Textures["Button_Pause_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Speed",
                hover_text = "Time x1",
                texture = AssetManager.Textures["Button_Speed1"],
                texture_highlight = AssetManager.Textures["Button_Speed1_Hover"],
                texture_disabled = AssetManager.Textures["Button_Speed1_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            Button speed_button = GetButton("Speed");
            if (Main.TimeSpeed == 1)
            {
                speed_button.HoverText = "Time x1";
                speed_button.Texture = AssetManager.Textures["Button_Speed1"];
                speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
            }
            else if (Main.TimeSpeed == 2)
            {
                speed_button.HoverText = "Time x2";
                speed_button.Texture = AssetManager.Textures["Button_Speed2"];
                speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
            }
            else if (Main.TimeSpeed == 3)
            {
                speed_button.HoverText = "Time x3";
                speed_button.Texture = AssetManager.Textures["Button_Speed3"];
                speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
            }
            else if (Main.TimeSpeed == 4)
            {
                speed_button.HoverText = "Time x4";
                speed_button.Texture = AssetManager.Textures["Button_Speed4"];
                speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Debug", "Debugging", Color.White, new Region(0, 0, 0, 0), false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Date", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Date").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Time").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gold", "Gold: " + Handler.Gold, Color.Gold, AssetManager.Textures["Frame_Wide"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Gold").Opacity = 0.8f;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Alert",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["TextFrame"],
                texture_highlight = AssetManager.Textures["TextFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                draw_color_selected = Color.Red,
                text_color = Color.Red,
                text_selected_color = Color.White,
                enabled = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_Attacker", "", Color.Red, new Region(0, 0, 0, 0), false);
            GetLabel("Combat_Attacker").Alignment_Horizontal = Alignment.Center;
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_VS", "vs", Color.Red, new Region(0, 0, 0, 0), false);
            GetLabel("Combat_VS").Alignment_Horizontal = Alignment.Center;
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_Defender", "", Color.Red, new Region(0, 0, 0, 0), false);
            GetLabel("Combat_Defender").Alignment_Horizontal = Alignment.Center;
            AddPicture(Handler.GetID(), "MouseClick", AssetManager.Textures["LeftClick"], new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Dialogue", "", Color.White, AssetManager.Textures["TextFrame"],
                new Region(0, 0, 0, 0), new Color(64, 64, 64, 255), false);

            Label dialogue = GetLabel("Dialogue");
            dialogue.Alignment_Verticle = Alignment.Top;
            dialogue.Alignment_Horizontal = Alignment.Left;
            dialogue.AutoScale = false;
            dialogue.Scale = 1;

            AddPicture(Handler.GetID(), "Dialogue_Portrait1", AssetManager.Textures["Spot"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Dialogue_Portrait2", AssetManager.Textures["Spot"], new Region(0, 0, 0, 0), Color.White, false);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Dialogue_Option1",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["TextFrame"],
                texture_highlight = AssetManager.Textures["TextFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = new Color(64, 64, 64, 255),
                draw_color_selected = new Color(128, 128, 128, 255),
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Dialogue_Option2",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["TextFrame"],
                texture_highlight = AssetManager.Textures["TextFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = new Color(64, 64, 64, 255),
                draw_color_selected = new Color(128, 128, 128, 255),
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Dialogue_Option3",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["TextFrame"],
                texture_highlight = AssetManager.Textures["TextFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = new Color(64, 64, 64, 255),
                draw_color_selected = new Color(128, 128, 128, 255),
                text_color = Color.White,
                text_selected_color = Color.Red,
                enabled = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Highlight"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Select", AssetManager.Textures["Highlight"], new Region(0, 0, 0, 0), new Color(0, 255, 0, 255), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            GetButton("PlayPause").Region = new Region(Main.Game.ScreenWidth - (width * 4), 0, width, height);
            GetButton("Speed").Region = new Region(Main.Game.ScreenWidth - (width * 3), 0, width, height);

            GetLabel("Date").Region = new Region(Main.Game.ScreenWidth - (width * 2), 0, width * 2, height / 2);
            GetLabel("Time").Region = new Region(Main.Game.ScreenWidth - (width * 2), height / 2, width * 2, height / 2);
            GetLabel("Debug").Region = new Region(Main.Game.ScreenWidth - (width * 2), height, width * 2, height / 2);
            GetLabel("Gold").Region = new Region((Main.Game.Resolution.X / 2) - (width * 5), 0, width * 10, height);

            GetButton("Alert").Region = new Region((Main.Game.ScreenWidth / 2) - (width * 4), Main.Game.ScreenHeight - (height * 5), width * 8, height * 3);
            GetLabel("Dialogue").Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Main.Game.ScreenHeight - (height * 5), width * 10, height * 4);

            GetButton("Main").Region = new Region(0, 0, width, height);
            GetButton("Army").Region = new Region(width, 0, width, height);
            GetButton("Inventory").Region = new Region(width * 2, 0, width, height);
            GetButton("Worldmap").Region = new Region(width * 3, 0, width, height);

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
