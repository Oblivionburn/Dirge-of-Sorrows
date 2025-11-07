using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Time;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Help : Menu
    {
        #region Variables



        #endregion

        #region Constructor

        public Menu_Help(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Help";
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
                    if (picture.Name == "Background")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "ButtonsColumn")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls()
        {
            bool found_button = HoveringButton();

            if (!found_button)
            {
                GetLabel("Examine").Visible = false;
            }

            if (InputManager.KeyPressed("Esc"))
            {
                Back();
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

                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
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

            return found;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Formation")
            {
                GetInfo_Formation();
            }
            else if (button.Name == "StatusEffects")
            {
                GetInfo_StatusEffects();
            }
        }

        private void GetInfo_Formation()
        {
            Label text = GetLabel("Text");
            text.Text = "";

            string newText = "";
            newText += "- The first/left column of your squad is the 'front' of its formation, and the last/right column is the 'back'. For enemy squads the 'front' of " +
                "their formation is on the right, since they face the opposite direction.\n\n";
            newText += "- Characters in the front do normal damage with melee weapons, as well as receive normal damage from melee attacks. Characters in the middle will " +
                "do and receive 3/4 of melee damage. The back will do and receive 1/2 of melee damage.\n\n";
            newText += "- Characters in the back will do normal damage with bow weapons, as well as receive normal damage from bow attacks. Characters in the middle will " +
                "do and receive 3/4 of bow damage. The front will do and receive 1/2 of bow damage.\n\n";
            newText += "- Characters wielding magic grimoires do no extra damage from formation position.\n\n";

            text.Text = GameUtil.WrapText_Help(newText, 80);

            int width = Main.Game.MenuSize_X * 20;
            int height = Main.Game.MenuSize_Y * 12;

            int X = (Main.Game.Resolution.X / 2) - (width / 2);
            int Y = (Main.Game.Resolution.Y / 2) - (height / 2);

            int buttonWidth = Main.Game.MenuSize_X * 3;

            text.Region = new Region(X + buttonWidth, Y, width - (Main.Game.MenuSize_X * 2), height);
        }

        private void GetInfo_StatusEffects()
        {
            Label text = GetLabel("Text");
            text.Text = "";

            string newText = "";
            newText += "- Weak = melee/bow weapons do half damage. Duration: 1 turn, until healed, or end of combat. Max stack: 3 turns.\n\n";
            newText += "- Cursed = 10% chance to instantly die at start of turn. Duration: until healed.\n\n";
            newText += "- Melting = 10% chance to lose random piece of equipment. Duration: 1 turn, until healed, or end of combat. Max stack: 3 turns.\n\n";
            newText += "- Poisoned = 2 damage at start of turn. Duration: until healed. Max stack: infinite.\n\n";
            newText += "- Petrified = skip turn. Duration: until healed.\n\n";
            newText += "- Burning = 4 damage at start of turn. Duration: 2 turns, until healed, or end of combat. Max stack: infinite.\n\n";
            newText += "- Regenerating = 20 HP restored at start of turn. Duration: 5 turns or end of combat. Max stack: 5 turns.\n\n";
            newText += "- Charging = 20 EP restored at start of turn. Duration: 5 turns or end of combat. Max stack: 5 turns.\n\n";
            newText += "- Stunned = skip turn. Duration: 1 turn or end of combat. Max stack: infinite.\n\n";
            newText += "- Slow = melee/bow weapons do 0 damage. Duration: 1 turn, until healed, or end of combat. Max stack: 2 turns.\n\n";
            newText += "- Frozen = skip turn and take 5 damage at start of turn. Duration: 4 turns, until healed, or end of combat. Max stack: 4 turns.\n\n";
            newText += "- Shocked = skip turn and take 10 damage at start of turn. Duration: 2 turns, until healed, or end of combat. Max stack: 2 turns.";

            text.Text = GameUtil.WrapText_Help(newText, 107);

            int width = Main.Game.MenuSize_X * 20;
            int height = Main.Game.MenuSize_Y * 12;

            int X = (Main.Game.Resolution.X / 2) - (width / 2);
            int Y = (Main.Game.Resolution.Y / 2) - (height / 2);

            int buttonWidth = Main.Game.MenuSize_X * 3;

            text.Region = new Region(X + buttonWidth, Y, width, height + Main.Game.MenuSize_Y);
        }

        private void Back()
        {
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            MenuManager.ChangeMenu_Previous();

            TimeManager.Paused = false;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Frame_Full"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "ButtonsColumn", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, true);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Close",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                texture_disabled = AssetManager.Textures["Button_Back_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Formation",
                text = "Formation",
                texture = AssetManager.Textures["ButtonFrame_Large"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Large"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                draw_color_selected = Color.White,
                text_color = Color.White * 0.8f,
                text_selected_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "StatusEffects",
                text = "Status Effects",
                texture = AssetManager.Textures["ButtonFrame_Large"],
                texture_highlight = AssetManager.Textures["ButtonFrame_Large"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                draw_color_selected = Color.White,
                text_color = Color.White * 0.8f,
                text_selected_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(new LabelOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Text",
                text = "",
                text_color = Color.White,
                alignment_verticle = Alignment.Top,
                alignment_horizontal = Alignment.Left,
                region = new Region(0, 0, 0, 0),
                visible = true
            });
            GetLabel("Text").Margin = 16;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize_X * 20;
            int height = Main.Game.MenuSize_Y * 12;

            int X = (Main.Game.Resolution.X / 2) - (width / 2);
            int Y = (Main.Game.Resolution.Y / 2) - (height / 2);

            int buttonWidth = Main.Game.MenuSize_X * 3;
            int buttonHeight = Main.Game.MenuSize_Y;

            Picture background = GetPicture("Background");
            background.Region = new Region(X, Y, width, height);

            GetPicture("ButtonsColumn").Region = new Region(X, Y, buttonWidth, height);

            GetButton("Formation").Region = new Region(X, Y, buttonWidth, buttonHeight);
            GetButton("StatusEffects").Region = new Region(X, Y + buttonHeight, buttonWidth, buttonHeight);

            GetLabel("Text").Region = new Region(X + buttonWidth, Y, width - buttonWidth, height);
            GetButton("Back").Region = new Region(X, Y + background.Region.Height, Main.Game.MenuSize_X, Main.Game.MenuSize_Y);
        }

        #endregion
    }
}
