using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Squad : Menu
    {
        #region Variables

        Squad squad = null;

        int Top = 0;

        bool examining;
        bool moving;
        Character moving_character = null;

        List<Picture> GridList = new List<Picture>();
        List<Character> ReserveList = new List<Character>();

        static int width = Main.Game.MenuSize.X * 3;
        static int height = Main.Game.MenuSize.Y * 3;
        int starting_Y = Main.Game.MenuSize.Y + (height / 2) + height;
        int starting_X = (Main.Game.ScreenWidth / 2) - (width / 2) - width;

        static int grid_width = Main.Game.MenuSize.X;
        static int grid_height = Main.Game.MenuSize.Y;
        int starting_grid_Y = (Main.Game.ScreenHeight / 2) - (grid_height * 5);
        int starting_grid_X = (Main.Game.ScreenWidth / 2) + (grid_width * 2);

        Vector2 starting_pos;
        Rectangle starting_region;

        Vector2 new_pos;

        #endregion

        #region Constructors

        public Menu_Squad(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Squad";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (moving)
                {
                    MoveCharacter();
                }
                else
                {
                    UpdateControls();
                }

                Army ally_army = CharacterManager.GetArmy("Ally");
                if (ally_army != null)
                {
                    foreach (Squad ally_squad in ally_army.Squads)
                    {
                        if (ally_squad.ID == Handler.Selected_Squad)
                        {
                            foreach (Character character in ally_squad.Characters)
                            {
                                if (character.Visible)
                                {
                                    CharacterUtil.UpdateGear(character);
                                }
                            }

                            break;
                        }
                    }
                }

                Army enemy_army = CharacterManager.GetArmy("Enemy");
                if (enemy_army != null)
                {
                    foreach (Squad enemy_squad in enemy_army.Squads)
                    {
                        if (enemy_squad.ID == Handler.Selected_Squad)
                        {
                            foreach (Character character in enemy_squad.Characters)
                            {
                                if (character.Visible)
                                {
                                    CharacterUtil.UpdateGear(character);
                                }
                            }

                            break;
                        }
                    }
                }

                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null)
                {
                    foreach (Squad reserve_squad in reserve_army.Squads)
                    {
                        foreach (Character character in reserve_squad.Characters)
                        {
                            if (character.Visible)
                            {
                                CharacterUtil.UpdateGear(character);
                            }
                        }
                    }
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
                    if (picture.Name == "Background")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name != "Window_Top" &&
                        picture.Name != "Highlight" &&
                        picture.Name != "Background")
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

                Army ally_army = CharacterManager.GetArmy("Ally");
                if (ally_army != null)
                {
                    foreach (Squad ally_squad in ally_army.Squads)
                    {
                        if (ally_squad.ID == Handler.Selected_Squad)
                        {
                            CharacterUtil.DrawSquad(spriteBatch, ally_squad, Color.White);
                        }
                    }
                }

                Army enemy_army = CharacterManager.GetArmy("Enemy");
                if (enemy_army != null)
                {
                    foreach (Squad enemy_squad in enemy_army.Squads)
                    {
                        if (enemy_squad.ID == Handler.Selected_Squad)
                        {
                            CharacterUtil.DrawSquad(spriteBatch, enemy_squad, Color.White);
                        }
                    }
                }

                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null)
                {
                    foreach (Squad reserve_squad in reserve_army.Squads)
                    {
                        foreach (Character character in reserve_squad.Characters)
                        {
                            if (character.Visible)
                            {
                                CharacterUtil.DrawCharacter(spriteBatch, character, Color.White);
                            }
                        }
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Window_Top")
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
            examining = false;

            bool found_button = HoveringButton();
            bool found_squad = HoveringSquad();
            bool found_grid = HoveringGrid();

            if (!examining)
            {
                GetLabel("Examine").Visible = false;
            }

            if (!found_squad &&
                !found_grid)
            {
                GetPicture("Highlight").Visible = false;
            }

            if (!Handler.ViewOnly_Squad)
            {
                if (InputManager.Mouse_ScrolledDown)
                {
                    Army army = CharacterManager.GetArmy("Reserves");
                    if (army != null)
                    {
                        Character last_character = null;
                        if (ReserveList.Count > 0)
                        {
                            last_character = ReserveList[ReserveList.Count - 1];
                        }

                        if (last_character != null)
                        {
                            Top++;

                            if (Top > last_character.Formation.Y)
                            {
                                Top = (int)last_character.Formation.Y;
                            }
                        }

                        ResizeGrid();
                    }
                }
                else if (InputManager.Mouse_ScrolledUp)
                {
                    Top--;
                    if (Top <= 0)
                    {
                        Top = 0;
                    }

                    ResizeGrid();
                }
            }

            if (!found_button &&
                !found_squad &&
                !found_grid &&
                InputManager.Mouse_RB_Pressed)
            {
                Back();
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
                        examining = true;

                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            examining = false;

                            CheckClick(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;

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

            return found;
        }

        private bool HoveringSquad()
        {
            bool found = false;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Picture tile = GetPicture("squad_x:" + x.ToString() + ",squad_y:" + y.ToString());
                    if (tile != null)
                    {
                        if (InputManager.MouseWithin(tile.Region.ToRectangle))
                        {
                            found = true;

                            if (!moving)
                            {
                                Character character = squad.GetCharacter(new Vector2(x, y));
                                if (character != null)
                                {
                                    examining = true;

                                    GameUtil.Examine(this, character.Name);

                                    if (InputManager.Mouse_LB_Held &&
                                        InputManager.Mouse.Moved &&
                                        !Handler.ViewOnly_Squad)
                                    {
                                        found = false;
                                        examining = false;
                                        moving = true;

                                        starting_pos = new Vector2(character.Formation.X, character.Formation.Y);
                                        starting_region = character.Region.ToRectangle;
                                        moving_character = character;
                                        character.HealthBar.Visible = false;
                                        character.ManaBar.Visible = false;

                                        break;
                                    }
                                    else if (InputManager.Mouse_RB_Pressed)
                                    {
                                        found = false;
                                        examining = false;

                                        SelectCharacter(character.ID);
                                    }
                                }
                            }
                            else
                            {
                                new_pos = new Vector2(x, y);
                            }

                            Picture highlight = GetPicture("Highlight");
                            highlight.Region = tile.Region;
                            highlight.Visible = true;

                            break;
                        }
                    }
                }

                if (found)
                {
                    break;
                }
            }

            return found;
        }

        private bool HoveringGrid()
        {
            bool found = false;

            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null)
            {
                Squad squad = army.Squads[0];
                if (squad != null)
                {
                    for (int y = 0; y < 10; y++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            Picture tile = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                            if (tile != null)
                            {
                                if (InputManager.MouseWithin(tile.Region.ToRectangle))
                                {
                                    found = true;

                                    if (!moving)
                                    {
                                        Character character = squad.GetCharacter(new Vector2(x, y));
                                        if (character != null)
                                        {
                                            examining = true;

                                            GameUtil.Examine(this, character.Name);

                                            if (InputManager.Mouse_LB_Held &&
                                                InputManager.Mouse.Moved)
                                            {
                                                found = false;
                                                examining = false;
                                                moving = true;

                                                starting_pos = new Vector2(character.Formation.X, character.Formation.Y);
                                                starting_region = character.Region.ToRectangle;
                                                moving_character = character;
                                                break;
                                            }
                                            else if (InputManager.Mouse_RB_Pressed)
                                            {
                                                found = false;
                                                SelectCharacter(character.ID);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        new_pos = new Vector2(x, y);
                                    }

                                    Picture highlight = GetPicture("Highlight");
                                    highlight.Region = tile.Region;
                                    highlight.Visible = true;

                                    break;
                                }
                            }
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                }
            }

            return found;
        }

        private void MoveCharacter()
        {
            bool found_squad = HoveringSquad();
            bool found_grid = HoveringGrid();

            if (!found_squad &&
                !found_grid)
            {
                GetPicture("Highlight").Visible = false;
            }

            if (InputManager.Mouse_LB_Held)
            {
                moving_character.Region = new Region(InputManager.Mouse.X - (grid_width / 2), InputManager.Mouse.Y - (grid_height / 3) - grid_height, grid_width, grid_height + (grid_height / 2));
            }
            else
            {
                moving = false;

                if (!found_squad &&
                    !found_grid)
                {
                    moving_character.Formation = new Vector2(starting_pos.X, starting_pos.Y);
                    moving_character.Region = new Region(starting_region.X, starting_region.Y, starting_region.Width, starting_region.Height);
                }
                else if (found_squad)
                {
                    AddToSquad();
                }
                else if (moving_character.ID == Handler.MainCharacter_ID)
                {
                    //Prevent removing hero from first squad
                    moving_character.Formation = new Vector2(starting_pos.X, starting_pos.Y);
                    moving_character.Region = new Region(starting_region.X, starting_region.Y, starting_region.Width, starting_region.Height);
                }
                else if (found_grid)
                {
                    AddToReserves();
                }

                ResizeSquad();
                ResizeGrid();
            }
        }

        private void AddToSquad()
        {
            AssetManager.PlaySound_Random("Equip");

            Squad ally_squad = null;
            Squad reserves = null;

            Army army = CharacterManager.GetArmy("Ally");
            if (army != null)
            {
                foreach (Squad squad in army.Squads)
                {
                    if (squad.ID == Handler.Selected_Squad)
                    {
                        ally_squad = squad;
                        break;
                    }
                }
            }

            Army reserve_army = CharacterManager.GetArmy("Reserves");
            if (reserve_army != null)
            {
                reserves = reserve_army.Squads[0];
            }

            if (ally_squad != null)
            {
                bool inSquad = false;
                foreach (Character existing in ally_squad.Characters)
                {
                    if (existing.ID == moving_character.ID)
                    {
                        inSquad = true;
                        break;
                    }
                }

                bool found_other = false;

                foreach (Character existing in ally_squad.Characters)
                {
                    if (existing.Formation.X == new_pos.X &&
                        existing.Formation.Y == new_pos.Y &&
                        existing.ID != moving_character.ID)
                    {
                        found_other = true;

                        if (inSquad)
                        {
                            SwapCharacters(ally_squad, moving_character, ally_squad, existing);
                        }
                        else
                        {
                            SwapCharacters(reserves, moving_character, ally_squad, existing);

                            ReserveList.Add(existing);
                            existing.HealthBar.Visible = false;
                            existing.ManaBar.Visible = false;

                            ReserveList.Remove(moving_character);
                            CharacterUtil.ResizeBars(moving_character);
                        }
                        
                        break;
                    }
                }

                if (!found_other)
                {
                    moving_character.Formation = new Vector2(new_pos.X, new_pos.Y);

                    if (!inSquad)
                    {
                        reserves.Characters.Remove(moving_character);

                        ReserveList.Remove(moving_character);
                        CharacterUtil.ResizeBars(moving_character);

                        ally_squad.Characters.Add(moving_character);
                        if (ally_squad.Characters.Count == 1)
                        {
                            ally_squad.Name = moving_character.Name;
                            ally_squad.Leader_ID = moving_character.ID;
                        }
                    }
                }
            }
        }

        private void AddToReserves()
        {
            Squad ally_squad = null;
            Squad reserves = null;

            Army army = CharacterManager.GetArmy("Ally");
            if (army != null)
            {
                foreach (Squad squad in army.Squads)
                {
                    if (squad.ID == Handler.Selected_Squad)
                    {
                        ally_squad = squad;
                        break;
                    }
                }
            }

            Army reserve_army = CharacterManager.GetArmy("Reserves");
            if (reserve_army != null)
            {
                reserves = reserve_army.Squads[0];
            }

            if (reserves != null)
            {
                bool inReserves = false;
                foreach (Character existing in reserves.Characters)
                {
                    if (existing.ID == moving_character.ID)
                    {
                        inReserves = true;
                        break;
                    }
                }

                bool found_other = false;

                foreach (Character existing in reserves.Characters)
                {
                    if (existing.Formation.X == new_pos.X &&
                        existing.Formation.Y == new_pos.Y &&
                        existing.ID != moving_character.ID)
                    {
                        found_other = true;

                        if (inReserves)
                        {
                            SwapCharacters(reserves, moving_character, reserves, existing);
                        }
                        else
                        {
                            SwapCharacters(ally_squad, moving_character, reserves, existing);

                            ReserveList.Add(moving_character);
                            moving_character.HealthBar.Visible = false;
                            moving_character.ManaBar.Visible = false;

                            ReserveList.Remove(existing);
                            CharacterUtil.ResizeBars(existing);
                        }
                        
                        break;
                    }
                }

                if (!found_other)
                {
                    moving_character.Formation = new Vector2(new_pos.X, new_pos.Y);

                    if (!inReserves)
                    {
                        reserves.Characters.Add(moving_character);

                        ReserveList.Add(moving_character);
                        moving_character.HealthBar.Visible = false;
                        moving_character.ManaBar.Visible = false;

                        ally_squad.Characters.Remove(moving_character);

                        if (ally_squad.Characters.Count == 0)
                        {
                            ally_squad.Name = "";
                        }
                        else if (ally_squad.Characters.Count > 0)
                        {
                            ally_squad.Name = ally_squad.Characters[0].Name;
                        }
                    }
                }
            }
        }

        private void SwapCharacters(Squad old_squad, Character moving_char, Squad new_squad, Character existing_char)
        {
            new_squad.Characters.Remove(existing_char);
            old_squad.Characters.Add(existing_char);

            new_squad.Characters.Add(moving_char);
            old_squad.Characters.Remove(moving_char);

            moving_char.Formation = new Vector2(new_pos.X, new_pos.Y);
            moving_char.Region = new Region(existing_char.Region.X, existing_char.Region.Y, existing_char.Region.Width, existing_char.Region.Height);

            existing_char.Formation = new Vector2(starting_pos.X, starting_pos.Y);
            existing_char.Region = new Region(starting_region.X, starting_region.Y, starting_region.Width, starting_region.Height);
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                Back();
            }
        }

        private void Back()
        {
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            if (MenuManager.PreviousMenus.Count < 3)
            {
                GameUtil.Toggle_Pause(false);
            }

            MenuManager.ChangeMenu_Previous();
        }

        private void SelectCharacter(long id)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Character = id;

            InputManager.Mouse.Flush();
            MenuManager.ChangeMenu("Character");
        }

        private void ResetPos()
        {
            width = Main.Game.MenuSize.X * 3;
            height = Main.Game.MenuSize.Y * 3;
            starting_Y = (Main.Game.ScreenHeight / 2) + (grid_height * 5) - (height * 3);

            if (!Handler.ViewOnly_Squad)
            {
                starting_X = (Main.Game.ScreenWidth / 2) - (width / 2) - (width * 3);
            }
            else
            {
                starting_X = (Main.Game.ScreenWidth / 2) - (width / 2) - width;
            }
        }

        private void ResetGridPos()
        {
            grid_width = Main.Game.MenuSize.X;
            grid_height = Main.Game.MenuSize.Y;
            starting_grid_Y = (Main.Game.ScreenHeight / 2) - (grid_height * 5);
            starting_grid_X = (Main.Game.ScreenWidth / 2) + (grid_width * 2);
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddPicture(Handler.GetID(), "Arrow_Up", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Arrow_Down", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, false);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Back",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                texture_disabled = AssetManager.Textures["Button_Back_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name_Squad", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name_Reserves", "Reserves", Color.White, new Region(0, 0, 0, 0), false);

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        private void DisplayArrows()
        {
            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null)
            {
                Picture arrow_down = GetPicture("Arrow_Down");

                bool down_visible = true;
                int bottom_row = 9 + Top;

                Character last_character = null;
                if (ReserveList.Count > 0)
                {
                    last_character = ReserveList[ReserveList.Count - 1];
                }

                if (last_character != null)
                {
                    if (last_character.Visible &&
                       (last_character.Region.Y <= arrow_down.Region.Y))
                    {
                        down_visible = false;
                    }
                    else if (!last_character.Visible &&
                              last_character.Formation.Y <= bottom_row)
                    {
                        down_visible = false;
                    }
                }
                else
                {
                    down_visible = false;
                }

                arrow_down.Visible = down_visible;
            }

            Picture arrow_up = GetPicture("Arrow_Up");
            if (Top == 0)
            {
                arrow_up.Visible = false;
            }
            else
            {
                arrow_up.Visible = true;
            }
        }

        private void ResizeSquad()
        {
            ResetPos();

            if (squad != null)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        Picture tile = GetPicture("squad_x:" + x.ToString() + ",squad_y:" + y.ToString());
                        if (tile != null)
                        {
                            tile.Region = new Region(starting_X + (width * x), starting_Y + (height * y), width, height);
                            tile.Location = new Location(x, y, 0);
                        }

                        Character character = squad.GetCharacter(new Vector2(x, y));
                        if (character != null)
                        {
                            character.Region = new Region(starting_X + (width * x), starting_Y + (height * y) - height, width, height + (height / 2));
                            character.Visible = true;
                            CharacterUtil.ResizeBars(character);
                        }
                    }
                }
            }
        }

        private void ClearSquad()
        {
            if (squad != null)
            {
                GetLabel("Name_Squad").Text = "";

                for (int i = 0; i < Pictures.Count; i++)
                {
                    bool found = false;

                    Picture picture = Pictures[i];
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Character character = squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                character.Visible = false;
                            }

                            if (picture.Name == "squad_x:" + x.ToString() + ",squad_y:" + y.ToString())
                            {
                                found = true;
                                Pictures.Remove(picture);
                                i--;
                                break;
                            }
                        }

                        if (found)
                        {
                            break;
                        }
                    }
                }

                squad = null;
            }
        }

        private void LoadSquad()
        {
            ClearSquad();
            ResetPos();

            Army ally_army = CharacterManager.GetArmy("Ally");
            if (ally_army != null)
            {
                foreach (Squad ally_squad in ally_army.Squads)
                {
                    if (ally_squad.ID == Handler.Selected_Squad)
                    {
                        squad = ally_squad;
                        break;
                    }
                }
            }

            if (squad == null)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                if (enemy_army != null)
                {
                    foreach (Squad enemy_squad in enemy_army.Squads)
                    {
                        if (enemy_squad.ID == Handler.Selected_Squad)
                        {
                            squad = enemy_squad;
                            break;
                        }
                    }
                }
            }

            if (squad != null)
            {
                Label name_squad = GetLabel("Name_Squad");
                name_squad.Region = new Region(starting_X, starting_Y - (height / 2), width * 3, (height / 2));
                name_squad.Text = squad.Name + " Squad";

                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        long id = Handler.GetID();

                        Character character = squad.GetCharacter(new Vector2(x, y));
                        if (character != null)
                        {
                            id = character.ID;
                            character.Region = new Region(starting_X + (width * x), starting_Y + (height * y) - height, width, height + (height / 2));
                            character.Visible = true;
                            CharacterUtil.ResizeBars(character);
                        }

                        AddPicture(id, "squad_x:" + x.ToString() + ",squad_y:" + y.ToString(), AssetManager.Textures["Grid"],
                            new Region(starting_X + (width * x), starting_Y + (height * y), width, height), Color.White, true);
                    }
                }
            }
        }

        private void ResizeGrid()
        {
            ResetGridPos();

            Army reserve_army = CharacterManager.GetArmy("Reserves");
            if (reserve_army != null)
            {
                Squad reserve_squad = reserve_army.Squads[0];
                if (reserve_squad != null)
                {
                    foreach (Character character in reserve_squad.Characters)
                    {
                        character.Visible = false;
                    }

                    for (int y = 0; y < 10; y++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            Picture picture = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                            if (picture != null)
                            {
                                picture.Region = new Region(starting_grid_X + (grid_width * x), starting_grid_Y + (grid_height * y), grid_width, grid_height);
                                picture.Location = new Location(x, y + Top, 0);
                            }
                        }
                    }

                    foreach (Character reserve in ReserveList)
                    {
                        foreach (Picture grid in GridList)
                        {
                            if (reserve.Formation.X == grid.Location.X &&
                                reserve.Formation.Y == grid.Location.Y)
                            {
                                reserve.Region = new Region(grid.Region.X, grid.Region.Y - grid_height, grid.Region.Width, grid_height + (grid_height / 2));
                                reserve.Visible = true;
                                break;
                            }
                        }
                    }
                }
            }

            GetPicture("Arrow_Up").Region = new Region(starting_grid_X - grid_width, starting_grid_Y, grid_width, grid_height);
            GetPicture("Arrow_Down").Region = new Region(starting_grid_X - grid_width, starting_grid_Y + (grid_height * 9), grid_width, grid_height);

            DisplayArrows();
        }

        private void ClearGrid()
        {
            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null)
            {
                Squad squad = army.Squads[0];
                if (squad != null)
                {
                    foreach (Character existing in squad.Characters)
                    {
                        existing.Visible = false;
                    }
                }
            }

            GetLabel("Name_Reserves").Visible = false;
            ReserveList.Clear();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture existing = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                    if (existing != null)
                    {
                        Pictures.Remove(existing);

                        foreach (Picture grid in GridList)
                        {
                            if (grid.ID == existing.ID)
                            {
                                GridList.Remove(grid);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void LoadGrid()
        {
            Top = 0;

            ClearGrid();
            ResetGridPos();

            Label reserves = GetLabel("Name_Reserves");
            reserves.Region = new Region(starting_grid_X, starting_grid_Y - grid_height, grid_width * 10, grid_height);
            reserves.Visible = true;

            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null)
            {
                Squad squad = army.Squads[0];
                if (squad != null)
                {
                    foreach (Character existing in squad.Characters)
                    {
                        ReserveList.Add(existing);
                    }

                    for (int y = 0; y < 10; y++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            long id = Handler.GetID();

                            Character character = squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                id = character.ID;
                                character.Region = new Region(starting_grid_X + (grid_width * x), starting_grid_Y + (grid_height * y) - grid_height, grid_width, grid_height + (grid_height / 2));
                                character.Visible = true;
                            }

                            AddPicture(id, "x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                                new Region(starting_grid_X + (grid_width * x), starting_grid_Y + (grid_height * y), grid_width, grid_height), Color.White, true);

                            Picture grid = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                            if (grid != null)
                            {
                                grid.Location = new Location(x, y, 0);
                                GridList.Add(grid);
                            }
                        }
                    }
                }
            }

            GetPicture("Arrow_Up").Region = new Region(starting_grid_X - grid_width, starting_grid_Y, grid_width, grid_height);
            GetPicture("Arrow_Down").Region = new Region(starting_grid_X - grid_width, starting_grid_Y + (grid_height * 9), grid_width, grid_height);
        }

        public override void Load()
        {
            LoadSquad();

            if (Handler.ViewOnly_Squad)
            {
                ClearGrid();
            }
            else
            {
                LoadGrid();
            }
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            int X = 0;
            int Y = 0;

            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            GetButton("Back").Region = new Region(X, Y, width, height);

            GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            if (Visible)
            {
                ResizeSquad();
                ResizeGrid();

                GetLabel("Name_Squad").Region = new Region(starting_X, starting_Y - (height / 2), width * 3, height / 2);
                GetLabel("Name_Reserves").Region = new Region(starting_grid_X, starting_grid_Y - grid_height, grid_width * 10, grid_height);
            }
        }

        #endregion
    }
}
