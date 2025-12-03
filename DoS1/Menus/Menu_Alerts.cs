using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Inventories;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Alerts : Menu
    {
        #region Variables

        

        #endregion

        #region Constructor

        public Menu_Alerts(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Alerts";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                UpdateControls();
                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Pictures)
                {
                    picture.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name == "Dialogue")
                    {
                        label.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Label label in Labels)
                {
                    if (label.Name != "Dialogue")
                    {
                        label.Draw(spriteBatch);
                    }
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
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

        private void UpdateControls()
        {
            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        button.Selected = true;

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
                    }
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            if (button.Text == "[Enter Town]")
            {
                EnterTown();
            }
            else if (button.Text == "[Retreat]")
            {
                ArmyUtil.Undeploy();
                Close();
                GameUtil.Toggle_Pause(false);
            }
            else if (button.Text == "[Claim Region]")
            {
                UnlockNextLocation();
                Close();
                GameUtil.Toggle_Pause(false);
            }
            else if (button.Text == "[Yes - Start Tutorial]")
            {
                Handler.StoryStep = 0;
                Close();
                GameUtil.Toggle_Pause(false);
            }
            else if (button.Text == "[No - Skip Tutorial]")
            {
                Handler.StoryStep = 56;
                InventoryUtil.BeginningInventory();
                Close();
                GameUtil.Toggle_Pause(false);
            }
            else if (button.Text == "[Continue]" ||
                     button.Text == "[Click here to continue]")
            {
                if (Handler.AlertType == "Story")
                {
                    ContinueStory();

                    if (Handler.StoryStep != 38)
                    {
                        Close();
                    }
                }
                else
                {
                    GameUtil.Toggle_Pause(false);
                    Close();
                }
            }
            else if (button.Text == "[Retreat to Worldmap]")
            {
                Close();
                GameUtil.RetreatToWorldmap();
            }
            else if (button.Text == "[Hold Position]")
            {
                GameUtil.Toggle_Pause(false);
                Close();
            }
            else if (button.Text == "[Continue Moving]")
            {
                Squad squad = ArmyUtil.Get_Squad(Handler.Dialogue_Character2);
                Handler.Selected_Token = squad.ID;

                Scene localmap = SceneManager.GetScene("Localmap");
                Picture highlight = localmap.Menu.GetPicture("Highlight");
                highlight.Region = squad.Region;
                highlight.Visible = true;
                highlight.DrawColor = new Color(0, 0, 255, 255);
                highlight.Texture = AssetManager.Textures["Highlight_Circle"];

                Close();
            }
            else if (button.Text == "Fight!")
            {
                SoundManager.StopMusic();
                SoundManager.NeedMusic = true;

                Scene combat = SceneManager.GetScene("Combat");
                combat.Load();

                if (Handler.StoryStep == 38 ||
                    Handler.StoryStep == 50)
                {
                    combat.Menu.GetButton("Retreat").Visible = false;
                    GameUtil.Toggle_Pause_Combat(false);
                }

                SceneManager.ChangeScene(combat);

                Handler.Combat = true;
                Close();
            }
        }

        public override void Close()
        {
            Visible = false;

            Handler.AlertType = "";
            Handler.Dialogue_Character1 = null;
            Handler.Dialogue_Character2 = null;

            GetLabel("Combat_Attacker").Visible = false;
            GetLabel("Combat_VS").Visible = false;
            GetLabel("Combat_Defender").Visible = false;
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

            Squad squad = ArmyUtil.Get_Squad(Handler.Dialogue_Character2);
            if (squad != null)
            {
                Tile location = WorldUtil.GetLocation(squad);
                if (location != null)
                {
                    if (location.Type.Contains("Market"))
                    {
                        type = "Market";
                    }
                    else if (location.Type.Contains("Academy"))
                    {
                        type = "Academy";
                    }
                }
            }

            Close();

            if (type == "Town")
            {
                GameUtil.Toggle_Pause(false);
            }
            else
            {
                WorldUtil.EnterTown(type);
            }
        }

        private void UnlockNextLocation()
        {
            Menu ui = MenuManager.GetMenu("UI");
            ui.GetButton("Worldmap").Enabled = true;

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

        private void ContinueStory()
        {
            if (Handler.StoryStep <= 4 ||
                Handler.StoryStep == 8 ||
                Handler.StoryStep == 19 ||
                Handler.StoryStep == 48 |
                Handler.StoryStep == 55)
            {
                Handler.StoryStep++;
                GameUtil.Toggle_Pause(false);
            }
            else if (Handler.StoryStep == 10 ||
                     Handler.StoryStep == 11 ||
                     (Handler.StoryStep >= 22 && Handler.StoryStep <= 26) ||
                     Handler.StoryStep == 36 ||
                     (Handler.StoryStep >= 38 && Handler.StoryStep <= 43) ||
                     (Handler.StoryStep >= 45 && Handler.StoryStep <= 46) ||
                     Handler.StoryStep == 47 ||
                     (Handler.StoryStep >= 50 && Handler.StoryStep <= 53))
            {
                Handler.StoryStep++;
            }
            else if (Handler.StoryStep == 44 ||
                     Handler.StoryStep == 54)
            {
                Handler.StoryStep++;
                GameUtil.Toggle_Pause_Combat(false);
            }
            else if (Handler.StoryStep == 27)
            {
                InventoryUtil.BeginningInventory();
                Handler.StoryStep++;
            }
            else if (Handler.StoryStep == 37)
            {
                Handler.StoryStep++;
                Close();

                Army special = CharacterManager.GetArmy("Special");
                Scene scene = WorldUtil.GetScene();
                Map map = WorldUtil.GetMap(scene.World);
                Layer ground = map.GetLayer("Ground");
                Tile ally_base = WorldUtil.Get_Base(map, "Ally");

                Squad squad = ArmyUtil.Get_Squad(Handler.GetHero());
                if (squad != null)
                {
                    Squad enemy_squad = ArmyUtil.NewSquad("Enemy");
                    special.Squads.Add(enemy_squad);

                    ArmyUtil.Gen_EnemySquad(special, enemy_squad, 1, 1, 0, 1, 1);
                    foreach (Character character in enemy_squad.Characters)
                    {
                        foreach (Item item in character.Inventory.Items)
                        {
                            item.Attachments.Clear();
                        }
                    }
                    
                    CombatUtil.StartCombat(map, ground, ally_base, squad, enemy_squad);
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddLabel(new LabelOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Combat_Attacker",
                text = "",
                text_color = new Color(59, 42, 51),
                alignment_horizontal = Alignment.Center,
                region = new Region(0, 0, 0, 0),
                visible = false
            });

            AddLabel(new LabelOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Combat_VS",
                text = "vs",
                text_color = new Color(59, 42, 51),
                alignment_horizontal = Alignment.Center,
                region = new Region(0, 0, 0, 0),
                visible = false
            });

            AddLabel(new LabelOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Combat_Defender",
                text = "",
                text_color = new Color(59, 42, 51),
                alignment_horizontal = Alignment.Center,
                region = new Region(0, 0, 0, 0),
                visible = false
            });

            AddLabel(new LabelOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Dialogue",
                text = "",
                text_color = new Color(99, 82, 71),
                alignment_verticle = Alignment.Top,
                alignment_horizontal = Alignment.Left,
                texture = AssetManager.Textures["Frame_Text"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                visible = false
            });
            Label dialogue = GetLabel("Dialogue");
            dialogue.AutoScale = false;
            dialogue.Margin = 16;
            dialogue.Scale = 1;

            AddLabel(new LabelOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Dialogue_Name",
                text = "",
                text_color = new Color(99, 82, 71),
                alignment_verticle = Alignment.Top,
                alignment_horizontal = Alignment.Center,
                texture = AssetManager.Textures["Frame_Text_Wide"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                visible = false
            });
            Label dialogue_name = GetLabel("Dialogue_Name");
            dialogue_name.AutoScale = false;
            dialogue_name.Margin = 4;
            dialogue_name.Scale = 1;

            AddPicture(Handler.GetID(), "Dialogue_Portrait1", AssetManager.Textures["Frame_Portrait"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Dialogue_Portrait2", AssetManager.Textures["Frame_Portrait"], new Region(0, 0, 0, 0), Color.White, false);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Dialogue_Option1",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["ButtonFrame_Wide"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Wide"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.9f,
                draw_color_selected = Color.White,
                text_color = Color.Black,
                text_selected_color = Color.White,
                enabled = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Dialogue_Option2",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["ButtonFrame_Wide"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Wide"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.9f,
                draw_color_selected = Color.White,
                text_color = Color.Black,
                text_selected_color = Color.White,
                enabled = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Dialogue_Option3",
                font = AssetManager.Fonts["ControlFont"],
                texture = AssetManager.Textures["ButtonFrame_Wide"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Wide"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.9f,
                draw_color_selected = Color.White,
                text_color = Color.Black,
                text_selected_color = Color.White,
                enabled = true
            });

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            int Y = Main.Game.ScreenHeight - (height * 7);

            Label dialogue = GetLabel("Dialogue");
            dialogue.Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Y, width * 10, height * 4);

            int name_height = (int)(dialogue.Region.Height / 6);
            GetLabel("Dialogue_Name").Region = new Region(dialogue.Region.X + (width * 3), dialogue.Region.Y - name_height, dialogue.Region.Width - (width * 6), name_height);

            GetPicture("Dialogue_Portrait1").Region = new Region(dialogue.Region.X - width, dialogue.Region.Y - (height * 3), width * 3, height * 3);
            GetPicture("Dialogue_Portrait2").Region = new Region(dialogue.Region.X + dialogue.Region.Width - (width * 2), dialogue.Region.Y - (height * 3), width * 3, height * 3);

            Button option1 = GetButton("Dialogue_Option1");
            option1.Region = new Region(dialogue.Region.X, dialogue.Region.Y + dialogue.Region.Height, dialogue.Region.Width, name_height);

            Button option2 = GetButton("Dialogue_Option2");
            option2.Region = new Region(dialogue.Region.X, option1.Region.Y + option1.Region.Height, dialogue.Region.Width, name_height);

            Button option3 = GetButton("Dialogue_Option3");
            option3.Region = new Region(dialogue.Region.X, option2.Region.Y + option2.Region.Height, dialogue.Region.Width, name_height);
        }

        #endregion
    }
}
