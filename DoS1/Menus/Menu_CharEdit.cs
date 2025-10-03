using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Inventories;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_CharEdit : Menu
    {
        #region Variables

        bool ControlsLoaded;

        Character character;

        int warning = 0;
        bool Shift = false;

        int Head;
        int Skin;
        int HairStyle;
        string[] HairColors = new string[] { "Brown", "Red", "Blonde", "Black", "Gray", "White", "Purple", "Blue", "Cyan", "Green", "Pink" };
        int HairColor;
        string[] EyeColors = new string[] { "Green", "Brown", "Blue", "Cyan", "Purple", "Gold", "Red", "Black", "Gray" };
        int EyeColor;

        #endregion

        #region Constructors

        public Menu_CharEdit(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "CharEdit";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible)
            {
                UpdateControls();

                if (warning > 0)
                {
                    GetLabel("Warning").Visible = true;
                    InputBox input = GetInput("Name");

                    if (warning % 4 == 0)
                    {
                        if (input.DrawColor == Color.White)
                        {
                            input.DrawColor = Color.Red;
                        }
                        else
                        {
                            input.DrawColor = Color.White;
                        }
                    }

                    warning--;

                    if (warning == 0)
                    {
                        GetLabel("Warning").Visible = false;
                        input.Opacity = 0.8f;
                        input.DrawColor = Color.White;
                    }
                }

                base.Update(gameRef, content);

                foreach (InputBox input in Inputs)
                {
                    if (input.Active)
                    {
                        if (!string.IsNullOrEmpty(input.Text) &&
                            !string.IsNullOrEmpty(input.Caret))
                        {
                            input.Caret = "";
                        }
                    }
                }
            }
        }

        private void UpdateControls()
        {
            bool found = false;
            bool examine = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;
                        examine = true;

                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            examine = false;

                            CheckClick(button);
                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            foreach (InputBox input in Inputs)
            {
                if (input.Active)
                {
                    input.Text = character.Name;
                }
                else if (InputManager.MouseWithin(input.Region.ToRectangle))
                {
                    found = true;
                    input.Opacity = 1;

                    if (InputManager.Mouse_LB_Pressed)
                    {
                        found = false;
                        input.Active = true;

                        InputManager.Mouse.Flush();

                        InputManager.Keyboard.OnKeyStateChange += Keyboard_OnKeyStateChange;
                        Main.Game.Window.TextInput += Window_TextInput;
                        break;
                    }
                }
                else if (InputManager.Mouse.Moved &&
                         !input.Active)
                {
                    input.Opacity = 0.8f;
                }
            }

            if (!examine)
            {
                GetLabel("Examine").Visible = false;
            }

            if ((!found && InputManager.Mouse_LB_Pressed) ||
                (GetInput("Name").Active && InputManager.KeyPressed("Enter")))
            {
                Main.Game.Window.TextInput -= Window_TextInput;
                InputManager.Keyboard.OnKeyStateChange -= Keyboard_OnKeyStateChange;

                InputBox nameBox = GetInput("Name");
                nameBox.Active = false;
                nameBox.Opacity = 0.8f;
            }
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (e.Key == Keys.Back)
            {
                if (!string.IsNullOrEmpty(character.Name))
                {
                    character.Name = character.Name.Remove(character.Name.Length - 1, 1);
                }
            }
            else if (InputManager.Keyboard.KeysMapped.ContainsValue(e.Key) &&
                     e.Key != Keys.LeftShift &&
                     e.Key != Keys.RightShift &&
                     e.Key != Keys.Escape &&
                     e.Key.ToString().Length == 1)
            {
                if (Shift)
                {
                    character.Name += e.Key.ToString().ToUpper();
                }
                else
                {
                    character.Name += e.Key.ToString().ToLower();
                }
            }
        }

        private void Keyboard_OnKeyStateChange(object sender, KeyEventArgs e)
        {
            foreach (var key in e.KeysPressed)
            {
                if (key == Keys.LeftShift ||
                    key == Keys.RightShift)
                {
                    Shift = false;
                }
            }

            foreach (var key in e.KeysDown)
            {
                if (key == Keys.LeftShift ||
                    key == Keys.RightShift)
                {
                    Shift = true;
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                if (!string.IsNullOrEmpty(character.Name))
                {
                    Back();
                }
                else
                {
                    warning = 60;
                }
            }
            else if (button.Name == "HairStyle_Minus")
            {
                GetButton("HairStyle_Plus").Enabled = true;

                if (HairStyle > 0)
                {
                    HairStyle--;
                }

                if (HairStyle == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Hair").Visible = true;
                GetPicture("Hair").Texture = AssetManager.Textures["Left_" + Handler.HairStyles[HairStyle]];
            }
            else if (button.Name == "HairStyle_Plus")
            {
                GetButton("HairStyle_Minus").Enabled = true;

                if (HairStyle < Handler.HairStyles.Length - 1)
                {
                    HairStyle++;
                }

                if (HairStyle == Handler.HairStyles.Length - 1)
                {
                    button.Enabled = false;
                    GetPicture("Hair").Visible = false;
                }
                else
                {
                    GetPicture("Hair").Visible = true;
                    GetPicture("Hair").Texture = AssetManager.Textures["Left_" + Handler.HairStyles[HairStyle]];
                }
            }
            else if (button.Name == "HairColor_Minus")
            {
                GetButton("HairColor_Plus").Enabled = true;

                if (HairColor > 0)
                {
                    HairColor--;
                }

                if (HairColor == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Hair").DrawColor = Handler.HairColors[HairColors[HairColor]];
            }
            else if (button.Name == "HairColor_Plus")
            {
                GetButton("HairColor_Minus").Enabled = true;

                if (HairColor < HairColors.Length - 1)
                {
                    HairColor++;
                }

                if (HairColor == HairColors.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Hair").DrawColor = Handler.HairColors[HairColors[HairColor]];
            }
            else if (button.Name == "Head_Minus")
            {
                GetButton("Head_Plus").Enabled = true;

                if (Head > 0)
                {
                    Head--;
                }

                if (Head == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            }
            else if (button.Name == "Head_Plus")
            {
                GetButton("Head_Minus").Enabled = true;

                if (Head < Handler.HeadStyles.Length - 1)
                {
                    Head++;
                }

                if (Head == Handler.HeadStyles.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            }
            else if (button.Name == "EyeColor_Minus")
            {
                GetButton("EyeColor_Plus").Enabled = true;

                if (EyeColor > 0)
                {
                    EyeColor--;
                }

                if (EyeColor == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Eyes").DrawColor = Handler.EyeColors[EyeColors[EyeColor]];
            }
            else if (button.Name == "EyeColor_Plus")
            {
                GetButton("EyeColor_Minus").Enabled = true;

                if (EyeColor < EyeColors.Length - 1)
                {
                    EyeColor++;
                }

                if (EyeColor == EyeColors.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Eyes").DrawColor = Handler.EyeColors[EyeColors[EyeColor]];
            }
            else if (button.Name == "SkinColor_Minus")
            {
                GetButton("SkinColor_Plus").Enabled = true;

                if (Skin > 0)
                {
                    Skin--;
                }

                if (Skin == 0)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];

                Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (armor == null)
                {
                    GetPicture("Body").Texture = AssetManager.Textures[character.Direction.ToString() + "_Body_" + Handler.SkinTones[Skin] + "_Idle"];
                }
            }
            else if (button.Name == "SkinColor_Plus")
            {
                GetButton("SkinColor_Minus").Enabled = true;

                if (Skin < Handler.SkinTones.Length - 1)
                {
                    Skin++;
                }

                if (Skin == Handler.SkinTones.Length - 1)
                {
                    button.Enabled = false;
                }

                GetPicture("Head").Texture = AssetManager.Textures["Left_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];

                Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (armor == null)
                {
                    GetPicture("Body").Texture = AssetManager.Textures[character.Direction.ToString() + "_Body_" + Handler.SkinTones[Skin] + "_Idle"];
                }
            }
        }

        private void Back()
        {
            AssetManager.PlaySound_Random("Click");

            GetInput("Name").Active = false;

            Squad squad = ArmyUtil.Get_Squad(character.ID);
            if (squad != null)
            {
                Character leader = squad.GetLeader();
                if (leader != null &&
                    leader.ID == character.ID)
                {
                    squad.Name = character.Name;
                }
            }

            character.Texture = AssetManager.Textures[character.Direction.ToString() + "_Body_" + Handler.SkinTones[Skin] + "_Idle"];
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);

            Item head = character.Inventory.GetItem("Head");
            head.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + Handler.SkinTones[Skin] + "_" + Handler.HeadStyles[Head]];
            head.Image = character.Image;

            character.Inventory.GetItem("Eyes").DrawColor = Handler.EyeColors[EyeColors[EyeColor]];

            Item hair = character.Inventory.GetItem("Hair");
            if (hair != null)
            {
                hair.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + Handler.HairStyles[HairStyle]];
                hair.Image = character.Image;
                hair.DrawColor = Handler.HairColors[HairColors[HairColor]];
            }

            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            MenuManager.ChangeMenu_Previous();
        }

        private void LoadControls()
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name", "Name:", Color.White, new Region(0, 0, 0, 0), true);
            AddInput(AssetManager.Fonts["ControlFont"], Handler.GetID(), 100, "Name", "", Color.DarkGray, AssetManager.Textures["TextFrame"],
                new Region(0, 0, 0, 0), false, true);
            GetInput("Name").Alignment_Horizontal = Alignment.Left;
            GetInput("Name").Alignment_Verticle = Alignment.Center;
            GetInput("Name").TextColor_Selected = Color.White;

            AddPicture(Handler.GetID(), "Body", AssetManager.Textures["Left_Armor_Cloth_Cloth_Idle"], new Region(0, 0, 0, 0),
                new Color(255, 255, 255, 255), true);
            AddPicture(Handler.GetID(), "Head", AssetManager.Textures["Left_Light_Head1"], new Region(0, 0, 0, 0),
                new Color(255, 255, 255, 255), true);
            AddPicture(Handler.GetID(), "Eyes", AssetManager.Textures["Left_Eye"], new Region(0, 0, 0, 0),
                Handler.EyeColors["Green"], true);
            AddPicture(Handler.GetID(), "Hair", AssetManager.Textures["Left_Style1"], new Region(0, 0, 0, 0),
                Handler.HairColors["Brown"], true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairStyle", "Hair Style:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("HairStyle").Alignment_Horizontal = Alignment.Right;
            GetLabel("HairStyle").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairStyle_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairStyle_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "HairColor", "Hair Color:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("HairColor").Alignment_Horizontal = Alignment.Right;
            GetLabel("HairColor").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairColor_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "HairColor_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Head", "Head:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("Head").Alignment_Horizontal = Alignment.Right;
            GetLabel("Head").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Head_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Head_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "EyeColor", "Eye Color:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("EyeColor").Alignment_Horizontal = Alignment.Right;
            GetLabel("EyeColor").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "EyeColor_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "EyeColor_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "SkinColor", "Skin Color:", Color.White, new Region(0, 0, 0, 0), true);
            GetLabel("SkinColor").Alignment_Horizontal = Alignment.Right;
            GetLabel("SkinColor").Alignment_Verticle = Alignment.Center;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "SkinColor_Minus",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = false,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "SkinColor_Plus",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Back",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Warning", "Name required!", Color.Red, new Region(0, 0, 0, 0), false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            ControlsLoaded = true;
        }

        public override void Load()
        {
            LoadControls();
            character = null;

            Army ally_army = CharacterManager.GetArmy("Ally");
            if (ally_army != null)
            {
                foreach (Squad squad in ally_army.Squads)
                {
                    character = squad.GetCharacter(Handler.Selected_Character);
                    if (character != null)
                    {
                        break;
                    }
                }
            }

            if (character == null)
            {
                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null)
                {
                    foreach (Squad squad in reserve_army.Squads)
                    {
                        character = squad.GetCharacter(Handler.Selected_Character);
                        if (character != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (character != null)
            {
                GetInput("Name").Text = character.Name;

                Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (armor != null)
                {
                    GetPicture("Body").Texture = armor.Texture;
                }
                else
                {
                    GetPicture("Body").Texture = character.Texture;
                }

                Item head = character.Inventory.GetItem("Head");
                GetPicture("Head").Texture = head.Texture;

                string[] headParts = head.Texture.Name.Split('_');

                string skinTone = headParts[1];
                for (int i = 0; i < Handler.SkinTones.Length; i++)
                {
                    string tone = Handler.SkinTones[i];
                    if (tone == skinTone)
                    {
                        Skin = i;

                        if (Skin == 0)
                        {
                            GetButton("SkinColor_Minus").Enabled = false;
                        }
                        else
                        {
                            GetButton("SkinColor_Minus").Enabled = true;
                        }

                        if (Skin == Handler.SkinTones.Length - 1)
                        {
                            GetButton("SkinColor_Plus").Enabled = false;
                        }

                        break;
                    }
                }

                string headStyle = headParts[2];
                for (int i = 0; i < Handler.HeadStyles.Length; i++)
                {
                    string style = Handler.HeadStyles[i];
                    if (style == headStyle)
                    {
                        Head = i;

                        if (Head == 0)
                        {
                            GetButton("Head_Minus").Enabled = false;
                        }
                        else
                        {
                            GetButton("Head_Minus").Enabled = true;
                        }

                        if (Head == Handler.HeadStyles.Length - 1)
                        {
                            GetButton("Head_Plus").Enabled = false;
                        }

                        break;
                    }
                }

                Item eyes = character.Inventory.GetItem("Eyes");
                GetPicture("Eyes").DrawColor = eyes.DrawColor;

                int eyeColorNum = 0;
                foreach (var color in Handler.EyeColors)
                {
                    if (color.Value.R == eyes.DrawColor.R &&
                        color.Value.G == eyes.DrawColor.G &&
                        color.Value.B == eyes.DrawColor.B)
                    {
                        EyeColor = eyeColorNum;
                        if (EyeColor == 0)
                        {
                            GetButton("EyeColor_Minus").Enabled = false;
                        }
                        else
                        {
                            GetButton("EyeColor_Minus").Enabled = true;
                        }

                        if (EyeColor == Handler.EyeColors.Count - 1)
                        {
                            GetButton("EyeColor_Plus").Enabled = false;
                        }

                        break;
                    }

                    eyeColorNum++;
                }

                Item hair = character.Inventory.GetItem("Hair");
                if (hair != null)
                {
                    GetPicture("Hair").Texture = hair.Texture;
                    GetPicture("Hair").DrawColor = hair.DrawColor;
                    GetPicture("Hair").Visible = true;

                    string[] hairParts = hair.Texture.Name.Split('_');

                    string hairStyle = hairParts[1];
                    for (int i = 0; i < Handler.HairStyles.Length; i++)
                    {
                        string style = Handler.HairStyles[i];
                        if (style == hairStyle)
                        {
                            HairStyle = i;

                            if (HairStyle == 0)
                            {
                                GetButton("HairStyle_Minus").Enabled = false;
                            }
                            else
                            {
                                GetButton("HairStyle_Minus").Enabled = true;
                            }

                            if (HairStyle == Handler.HairStyles.Length - 1)
                            {
                                GetButton("HairStyle_Plus").Enabled = false;
                            }

                            break;
                        }
                    }

                    int hairColorNum = 0;
                    foreach (var color in Handler.HairColors)
                    {
                        if (color.Value.R == hair.DrawColor.R &&
                            color.Value.G == hair.DrawColor.G &&
                            color.Value.B == hair.DrawColor.B)
                        {
                            HairColor = hairColorNum;
                            if (HairColor == 0)
                            {
                                GetButton("HairColor_Minus").Enabled = false;
                            }
                            else
                            {
                                GetButton("HairColor_Minus").Enabled = true;
                            }
                            
                            if (HairColor == Handler.HairColors.Count - 1)
                            {
                                GetButton("HairColor_Plus").Enabled = false;
                            }

                            break;
                        }

                        hairColorNum++;
                    }
                }
                else
                {
                    GetButton("HairStyle_Plus").Enabled = false;
                    GetPicture("Hair").Visible = false;
                    HairStyle = Handler.HairStyles.Length - 1;
                }
            }

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.Y;
            float center_x = Main.Game.ScreenWidth / 2;

            if (ControlsLoaded)
            {
                GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                int Y = (Main.Game.ScreenHeight / 2) - (height * 6);
                GetLabel("Name").Region = new Region(center_x - (width * 6), Y, width * 2, height);
                GetInput("Name").Region = new Region(center_x - (width * 4), Y, width * 8, height);
                GetLabel("Warning").Region = new Region(center_x + (width * 4), Y, width * 4, height);

                Y += (height * 2);
                Picture body = GetPicture("Body");
                body.Region = new Region(center_x - (width * 3), Y, width * 6, height * 8);
                body.Image = new Rectangle(0, 0, body.Texture.Width / 4, body.Texture.Height);

                GetPicture("Head").Region = body.Region;
                GetPicture("Head").Image = body.Image;

                GetPicture("Eyes").Region = body.Region;
                GetPicture("Eyes").Image = body.Image;

                GetPicture("Hair").Region = body.Region;
                GetPicture("Hair").Image = body.Image;

                GetLabel("HairStyle").Region = new Region(body.Region.X - (width * 5), Y, width * 4, height);
                GetButton("HairStyle_Minus").Region = new Region(body.Region.X - width, Y, width, height);
                GetButton("HairStyle_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

                Y += height;
                GetLabel("HairColor").Region = new Region(body.Region.X - (width * 5), Y, width * 4, height);
                GetButton("HairColor_Minus").Region = new Region(body.Region.X - width, Y, width, height);
                GetButton("HairColor_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

                Y += height;
                GetLabel("Head").Region = new Region(body.Region.X - (width * 4), Y, width * 3, height);
                GetButton("Head_Minus").Region = new Region(body.Region.X - width, Y, width, height);
                GetButton("Head_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

                Y += height;
                GetLabel("EyeColor").Region = new Region(body.Region.X - (width * 4), Y, width * 3, height);
                GetButton("EyeColor_Minus").Region = new Region(body.Region.X - width, Y, width, height);
                GetButton("EyeColor_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

                Y += height;
                GetLabel("SkinColor").Region = new Region(body.Region.X - (width * 4), Y, width * 3, height);
                GetButton("SkinColor_Minus").Region = new Region(body.Region.X - width, Y, width, height);
                GetButton("SkinColor_Plus").Region = new Region(body.Region.X + body.Region.Width, Y, width, height);

                Y = (int)body.Region.Y + (int)body.Region.Height;
                GetButton("Back").Region = new Region(body.Region.X + (body.Region.Width / 2) - (width / 2), Y, width, height);

                GetLabel("Examine").Region = new Region(0, 0, 0, 0);
            }
        }

        #endregion
    }
}
