using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Sounds;
using OP_Engine.Utility;
using OP_Engine.Inputs;
using OP_Engine.Tiles;
using OP_Engine.Characters;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Alerts : Menu
    {
        #region Variables

        private int mouseClickDelay = 0;

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
                UpdateAlerts();

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

        private void UpdateControls()
        {
            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
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
        }

        private void UpdateAlerts()
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

            AnimateMouseClick();
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Alert")
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
                    Close();
                    GameUtil.Toggle_Pause(false);
                }
                else if (button.Text == "Claim Region")
                {
                    UnlockNextLocation();
                    Close();
                    GameUtil.Toggle_Pause(false);
                }
                else if (button.Text == "(Continue)")
                {
                    if (Handler.AlertType == "Tutorial")
                    {
                        ContinueTutorials();
                    }
                    else
                    {
                        GameUtil.Toggle_Pause(false);
                    }

                    Close();
                }
                else if (button.Text == "(Retreat)")
                {
                    Close();
                    GameUtil.ReturnToWorldmap();
                }
            }
            else if (button.Name == "Dialogue_Option2")
            {
                if (button.Text == "Move Out")
                {
                    Close();
                    GameUtil.Toggle_Pause(false);
                }
            }
            else if (button.Name == "Dialogue_Option3")
            {

            }
        }

        public override void Close()
        {
            Visible = false;

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

            Close();

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

        private void ContinueTutorials()
        {
            if (Handler.TutorialType == "Worldmap")
            {
                if (Handler.TutorialStep < 1)
                {
                    Handler.TutorialStep++;
                }
                else
                {
                    Handler.TutorialStep = 0;
                    Handler.Tutorial_Worldmap = true;
                    GameUtil.Toggle_Pause(false);
                }
            }
            else if (Handler.TutorialType == "Localmap")
            {
                if (Handler.TutorialStep < 5)
                {
                    Handler.TutorialStep++;
                }
                else
                {
                    Handler.TutorialStep = 0;
                    Handler.Tutorial_Localmap = true;
                    GameUtil.Toggle_Pause(false);
                }
            }
            else if (Handler.TutorialType == "Army")
            {
                if (Handler.TutorialStep < 2)
                {
                    Handler.TutorialStep++;
                }
                else
                {
                    Handler.TutorialStep = 0;
                    Handler.Tutorial_Army = true;
                }
            }
            else if (Handler.TutorialType == "Squad")
            {
                if (Handler.TutorialStep < 5)
                {
                    Handler.TutorialStep++;
                }
                else
                {
                    Handler.TutorialStep = 0;
                    Handler.Tutorial_Squad = true;
                }
            }
            else if (Handler.TutorialType == "Character")
            {
                if (Handler.TutorialStep < 2)
                {
                    Handler.TutorialStep++;
                }
                else
                {
                    Handler.TutorialStep = 0;
                    Handler.Tutorial_Character = true;
                }
            }
            else if (Handler.TutorialType == "Item")
            {
                if (Handler.TutorialStep < 1)
                {
                    Handler.TutorialStep++;
                }
                else
                {
                    Handler.TutorialStep = 0;
                    Handler.Tutorial_Item = true;
                }
            }
            else if (Handler.TutorialType == "Shop")
            {
                Handler.Tutorial_Shop = true;
            }
            else if (Handler.TutorialType == "Academy")
            {
                Handler.Tutorial_Academy = true;
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

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

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            GetButton("Alert").Region = new Region((Main.Game.ScreenWidth / 2) - (width * 4), Main.Game.ScreenHeight - (height * 5), width * 8, height * 3);
            GetLabel("Dialogue").Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Main.Game.ScreenHeight - (height * 5), width * 10, height * 4);
        }

        #endregion
    }
}
