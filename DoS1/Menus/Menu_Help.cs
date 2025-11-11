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
            else if (button.Name == "Movement")
            {
                GetInfo_Movement();
            }
            else if (button.Name == "Combat")
            {
                GetInfo_Combat();
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

        private void GetInfo_Movement()
        {
            Label text = GetLabel("Text");
            text.Text = "";

            string newText = "";
            newText += "If you left-click one of your squads, the game will automatically pause and allow you to select a destination for that squad with a preview of " +
                "the path it will take to get there. A destination can be a location on the map which will be highlighted with a green box, or an enemy squad which " +
                "will be highlighted with a red circle. Once a destination is selected, the game will unpause and your squad will move along its designated path until " +
                "it reaches its destination. If the destination is an enemy squad, then the path of your squad will continuously update to follow the enemy.\n\n";
            newText += "When a squad is selected for movement you can right-click anywhere, or left-click the same squad again, to cancel moving it. Cancelling movement " +
                "will not remove a squad's current path/destination. When no squad is selected for movement, you can hover the mouse over any squad to preview their " +
                "current path and destination, including enemy squads.\n\n";
            newText += "When one of your squads collides with an enemy squad, or vice-versa, both squads will be repositioned to their current tile location and then " +
                "combat will begin.\n\n";

            text.Text = GameUtil.WrapText_Help(newText, 80);

            int width = Main.Game.MenuSize_X * 20;
            int height = Main.Game.MenuSize_Y * 12;

            int X = (Main.Game.Resolution.X / 2) - (width / 2);
            int Y = (Main.Game.Resolution.Y / 2) - (height / 2);

            int buttonWidth = Main.Game.MenuSize_X * 3;

            text.Region = new Region(X + buttonWidth, Y, width - (Main.Game.MenuSize_X * 2), height);
        }

        private void GetInfo_Combat()
        {
            Label text = GetLabel("Text");
            text.Text = "";

            string newText = "";
            newText += "When combat begins, you will be taken to a separate Combat screen with your squad on the right-side of the field and the enemy on the left. " +
                "Combat plays out automatically until one side has been completely eliminated, or until both sides have run out of Energy Points (EP). Every action in " +
                "combat costs a certain number of EP, based on the equipment a character is wearing. Higher tier armor/weapons/runes will cost more EP to wield. In the " +
                "event that both sides have run out EP, the winner will be determined by whichever side caused the most damage.\n\n";
            newText += "The order of combat is your whole squad goes first, then the entire enemy squad takes their turn, etc. Character order within a squad is " +
                "determined based on their formation position, with the upper-left corner starting first and then proceeding left-to-right/top-to-bottom\n\n";
            newText += "Target selection is based on the weapon a character is wielding:\n";
            newText += "- Characters wielding a melee weapon will always target whoever is directly in front of them on the same row. If no target can be found in the " +
                "same row, then they will search for a target in other rows, starting from the Front and working their way to the Back. If the character is in the top " +
                "row, then the next row searched will be the middle and last will be the bottom row. If the character is in the middle row, then the next row searched " +
                "will be the top and last will be the bottom row. If the character is in the bottom row, then the next row searched will be the middle and last will be " +
                "the top row.\n";
            newText += "- Characters wielding a ranged weapon (bow or grimoire with offensive runes) will always target whoever is furthest back on the same row. If no " +
                "target can be found in the same row, then they will search the other rows in the same order as a character wielding a melee weapon, starting from the " +
                "Back and working their way to the Front.\n";
            newText += "- Characters wielding any weapon with restorative runes (e.g. Health or Energy), can simultaneously target their own squad members for " +
                "restoration of HP/EP. Restoration targeting will always be whoever has the least amount of the value being restored. If the restorative rune is paired " +
                "with an Area Rune, then the target will be the entire squad when the Area effect is applied.";

            text.Text = GameUtil.WrapText_Help(newText, 100);

            int width = Main.Game.MenuSize_X * 20;
            int height = Main.Game.MenuSize_Y * 12;

            int X = (Main.Game.Resolution.X / 2) - (width / 2);
            int Y = (Main.Game.Resolution.Y / 2) - (height / 2);

            int buttonWidth = Main.Game.MenuSize_X * 3;

            text.Region = new Region(X + buttonWidth, Y, width, height + Main.Game.MenuSize_Y);
        }

        private void GetInfo_Formation()
        {
            Label text = GetLabel("Text");
            text.Text = "";

            string newText = "";
            newText += "The left column of your squad's formation is the Front, the middle column is the Middle, and the right column is the Back. For enemy squads, the " +
                "Front of their formation is on the right since they face the opposite direction.\n\n";
            newText += "Characters in the Front do normal damage with melee weapons, as well as receive normal damage from melee attacks. Characters in the Middle will " +
                "do and receive 3/4 of melee damage. The Back will do and receive 1/2 of melee damage.\n\n";
            newText += "Characters in the Back will do normal damage with bow weapons, as well as receive normal damage from bow attacks. Characters in the Middle will " +
                "do and receive 3/4 of bow damage. The Front will do and receive 1/2 of bow damage.\n\n";
            newText += "Characters wielding grimoires do no extra damage from formation position.\n\n";

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

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Frame_Text_Full"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "ButtonsColumn", AssetManager.Textures["Frame_Large"], new Region(0, 0, 0, 0), Color.White, false);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Movement",
                text = "Movement",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text_color = Color.Black,
                text_selected_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Combat",
                text = "Combat",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text_color = Color.Black,
                text_selected_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Formation",
                text = "Formation",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text_color = Color.Black,
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
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text_color = Color.Black,
                text_selected_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                font = AssetManager.Fonts["ControlFont"],
                name = "Back",
                text = "Close",
                texture = AssetManager.Textures["ButtonFrame"],
                texture_highlight = AssetManager.Textures["ButtonFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                draw_color_selected = Color.White,
                text_color = Color.Black,
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
                text_color = new Color(99, 82, 71),
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
            int buttonHeight = Main.Game.MenuSize_Y / 2;

            Picture background = GetPicture("Background");
            background.Region = new Region(X + buttonWidth, Y, width - buttonWidth, height);

            GetPicture("ButtonsColumn").Region = new Region(X, Y, buttonWidth, height);

            GetButton("Movement").Region = new Region(X, Y, buttonWidth, buttonHeight);
            GetButton("Combat").Region = new Region(X, Y + buttonHeight, buttonWidth, buttonHeight);
            GetButton("Formation").Region = new Region(X, Y + (buttonHeight * 2), buttonWidth, buttonHeight);
            GetButton("StatusEffects").Region = new Region(X, Y + (buttonHeight * 3), buttonWidth, buttonHeight);

            GetButton("Back").Region = new Region(X, background.Region.Y + background.Region.Height - (buttonHeight * 2), buttonWidth, buttonHeight * 2);
            GetLabel("Text").Region = new Region(X + buttonWidth, Y, width - buttonWidth, height);
        }

        #endregion
    }
}
