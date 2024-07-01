using System.Linq;
using System.Timers;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Rendering;
using OP_Engine.Time;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Weathers;

using DoS1.Util;

namespace DoS1.Scenes
{
    public class Combat : Scene
    {
        #region Variables

        private int mouseClickDelay = 0;

        private Squad ally_squad;
        private Squad enemy_squad;

        private List<Character> targets = new List<Character>();
        private Character current_character = null;
        private string attack_type = "";
        private bool hero_killed = false;
        private int ep_cost = 0;

        private int character_frame = 1;
        private float move_speed = 0;
        private int effect_frame = 1;
        private int animation_speed = 8;

        private int round = 0;
        private int combat_step = 0;

        private int ally_total_damage;
        private int enemy_total_damage;

        private bool won_battle;
        private int gold;

        #endregion

        #region Constructors

        public Combat(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Combat";
            Load(content);

            Handler.CombatTimer.Elapsed += Timer_Elapsed;
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (SoundManager.NeedMusic)
                {
                    SoundManager.MusicLooping = true;
                    AssetManager.PlayMusic_Random("Combat", true);
                }

                WorldUtil.AnimateTiles();

                foreach (Button button in Menu.Buttons)
                {
                    button.Update();
                }

                for (int i = 0; i < Menu.Labels.Count; i++)
                {
                    Label label = Menu.Labels[i];
                    label.Update();
                }

                UpdateControls();
                AnimateMouseClick();
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                for (int i = 0; i < Menu.Pictures.Count; i++)
                {
                    Picture picture = Menu.Pictures[i];
                    if (picture.Name != "Damage" &&
                        picture.Name != "Cast" &&
                        picture.Name != "Result" &&
                        picture.Name != "MouseClick")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                World.Draw(spriteBatch, Main.Game.Resolution, RenderingManager.Lighting.DrawColor);

                if (ally_squad != null)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Character character = ally_squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                CharacterUtil.DrawCharacter(spriteBatch, character, RenderingManager.Lighting.DrawColor);
                            }
                        }
                    }
                }

                if (enemy_squad != null)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                CharacterUtil.DrawCharacter(spriteBatch, character, RenderingManager.Lighting.DrawColor);
                            }
                        }
                    }
                }

                for (int i = 0; i < Menu.Pictures.Count; i++)
                {
                    Picture picture = Menu.Pictures[i];
                    if (picture.Name == "Damage" ||
                        picture.Name == "Cast")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                for (int i = 0; i < Menu.Labels.Count; i++)
                {
                    Menu.Labels[i].Draw(spriteBatch);
                }

                WeatherManager.Draw(spriteBatch);

                Menu.GetPicture("Result").Draw(spriteBatch);
                Menu.GetPicture("MouseClick").Draw(spriteBatch);

                foreach (Button button in Menu.Buttons)
                {
                    button.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls()
        {
            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            CheckClick(button);

                            if (button.Name != "Result")
                            {
                                button.Opacity = 0.9f;
                            }
                            
                            button.Selected = false;

                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        if (button.Name != "Result")
                        {
                            button.Opacity = 0.9f;
                        }

                        button.Selected = false;
                    }
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");
            InputManager.Mouse.Flush();

            if (button.Name == "Result")
            {
                Army ally_army = CharacterManager.GetArmy("Ally");

                if (hero_killed ||
                    !ally_army.Squads.Any())
                {
                    Handler.Combat = false;

                    SoundManager.StopMusic();
                    SoundManager.NeedMusic = true;

                    SceneManager.ChangeScene("GameOver");
                }
                else
                {
                    Leave();
                }
            }
            else if (button.Name == "Retreat")
            {
                Retreat();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!TimeManager.Paused)
            {
                if (current_character != null)
                {
                    Tile origin_tile = CombatUtil.OriginTile(World, current_character);
                    Tile target_tile = CombatUtil.TargetTile(World, current_character);

                    if (origin_tile != null &&
                        target_tile != null)
                    {
                        switch (combat_step)
                        {
                            case 0:
                                bool can_attack = true;

                                ep_cost = InventoryUtil.Get_EP_Cost(current_character);
                                if (ep_cost > 0)
                                {
                                    if (current_character.ManaBar.Value < ep_cost)
                                    {
                                        can_attack = false;
                                    }
                                }

                                targets = CombatUtil.GetTargets(current_character, ally_squad, enemy_squad);
                                if (!targets.Any())
                                {
                                    can_attack = false;
                                    FinishCombat();
                                }

                                if (can_attack)
                                {
                                    attack_type = CharacterUtil.AttackType(current_character);
                                    CombatUtil.SwitchAnimation(current_character, attack_type);

                                    combat_step++;
                                }
                                break;

                            case 1:
                                if (attack_type == "Attack")
                                {
                                    if (CombatUtil.AtTile(current_character, target_tile, move_speed))
                                    {
                                        combat_step++;
                                    }
                                    else
                                    {
                                        CombatUtil.MoveForward(current_character, move_speed);
                                    }
                                }
                                else if (attack_type == "Cast")
                                {
                                    StartCast();
                                    combat_step++;
                                }
                                else
                                {
                                    combat_step++;
                                }

                                break;

                            case 2:
                                if (character_frame % animation_speed == 0)
                                {
                                    CharacterUtil.Animate(current_character);

                                    if (attack_type == "Cast")
                                    {
                                        AnimateCastEffect();
                                    }

                                    if (character_frame == animation_speed * 3)
                                    {
                                        Attack();
                                    }
                                    else if (character_frame >= animation_speed * 4)
                                    {
                                        combat_step++;
                                    }

                                    character_frame++;
                                }
                                else
                                {
                                    character_frame++;
                                }
                                break;

                            case 3:
                                if (effect_frame >= 20)
                                {
                                    combat_step++;
                                }
                                else if (effect_frame % 4 == 0)
                                {
                                    AnimateDamageEffects();
                                    AnimateDamageLabels();
                                    effect_frame++;
                                }
                                else
                                {
                                    AnimateDamageLabels();
                                    effect_frame++;
                                }
                                break;

                            default:
                                if (CombatUtil.AtTile(current_character, origin_tile, move_speed))
                                {
                                    FinishAttack();
                                }
                                else
                                {
                                    CombatUtil.MoveBack(current_character, move_speed);
                                }
                                break;
                        }
                    }
                }
                else
                {
                    GetCurrentCharacter();

                    if (current_character == null)
                    {
                        FinishRound();
                    }
                }
            }
        }

        private void GetCurrentCharacter()
        {
            if (current_character == null)
            {
                if (ally_squad != null)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            Character character = ally_squad.GetCharacter(new Vector2(x, y));
                            if (character != null &&
                                character.CombatStep == 0 &&
                                character.ManaBar.Value > 0 &&
                                !character.Dead)
                            {
                                current_character = character;
                                break;
                            }
                        }

                        if (current_character != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (current_character == null)
            {
                if (enemy_squad != null)
                {
                    for (int x = 2; x >= 0; x--)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            Character character = enemy_squad.GetCharacter(new Vector2(x, y));
                            if (character != null &&
                                character.CombatStep == 0 &&
                                character.ManaBar.Value > 0 &&
                                !character.Dead)
                            {
                                current_character = character;
                                break;
                            }
                        }

                        if (current_character != null)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void StartCast()
        {
            AssetManager.PlaySound_Random("Cast");

            if (current_character.Type == "Ally")
            {
                Menu.AddPicture(Handler.GetID(), "Cast", AssetManager.Textures["Cast"],
                    new Region(current_character.Region.X, current_character.Region.Y, current_character.Region.Width, current_character.Region.Height),
                        RenderingManager.Lighting.DrawColor * 0.9f, true);
            }
            else if (current_character.Type == "Enemy")
            {
                Menu.AddPicture(Handler.GetID(), "Cast", AssetManager.Textures["EvilCast"],
                    new Region(current_character.Region.X, current_character.Region.Y, current_character.Region.Width, current_character.Region.Height),
                        RenderingManager.Lighting.DrawColor * 0.9f, true);
            }

            Picture cast = Menu.GetPicture("Cast");
            cast.Image = new Rectangle(0, 0, cast.Texture.Width / 4, cast.Texture.Height);
        }

        private void Attack()
        {
            if (current_character != null)
            {
                current_character.ManaBar.Value -= ep_cost;
                current_character.ManaBar.Update();

                Item weapon = InventoryUtil.Get_EquippedItem(current_character, "Weapon");

                foreach (Character target in targets)
                {
                    CombatUtil.DoDamage(Menu, target, weapon, ref ally_total_damage, ref enemy_total_damage);

                    if (target.Dead)
                    {
                        if (target.Type == "Enemy")
                        {
                            gold += 100;
                        }
                        else if (target.ID == Handler.MainCharacter_ID)
                        {
                            MainCharacterKilled();
                            break;
                        }
                    }
                }
            }
        }

        private void AnimateDamageLabels()
        {
            for (int i = 0; i < Menu.Labels.Count; i++)
            {
                Label label = Menu.Labels[i];
                if (label.Name == "Damage" &&
                    label.Region != null)
                {
                    label.Region.Y -= 1;
                    label.Opacity -= 0.05f;

                    if (label.Opacity <= 0)
                    {
                        Menu.Labels.Remove(label);
                        i--;
                    }
                }
            }
        }

        private void AnimateDamageEffects()
        {
            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Damage")
                {
                    int X = picture.Image.X + picture.Image.Height;
                    if (X >= picture.Texture.Width)
                    {
                        Menu.Pictures.Remove(picture);
                        i--;
                    }
                    else
                    {
                        picture.Image = new Rectangle(X, picture.Image.Y, picture.Image.Width, picture.Image.Height);
                    }
                }
            }
        }

        private void AnimateCastEffect()
        {
            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Cast")
                {
                    int X = picture.Image.X + picture.Image.Height;
                    if (X >= picture.Texture.Width)
                    {
                        Menu.Pictures.Remove(picture);
                        i--;
                    }
                    else
                    {
                        picture.Image = new Rectangle(X, picture.Image.Y, picture.Image.Width, picture.Image.Height);
                        picture.Opacity -= 0.2f;
                    }
                }
            }
        }

        private void AnimateMouseClick()
        {
            Picture mouseClick = Menu.GetPicture("MouseClick");
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

        private void RemoveEffects()
        {
            for (int i = 0; i < Menu.Labels.Count; i++)
            {
                Label label = Menu.Labels[i];
                if (label.Name == "Damage")
                {
                    Menu.Labels.Remove(label);
                    i--;
                }
            }

            for (int i = 0; i < Menu.Pictures.Count; i++)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Damage" ||
                    picture.Name == "Cast")
                {
                    Menu.Pictures.Remove(picture);
                    i--;
                }
            }
        }

        private void FinishAttack()
        {
            if (current_character != null)
            {
                RemoveEffects();
                CombatUtil.SwitchAnimation(current_character, "Idle");

                if (ally_squad != null)
                {
                    for (int i = 0; i < ally_squad.Characters.Count; i++)
                    {
                        Character character = ally_squad.Characters[i];
                        if (character.Dead)
                        {
                            ally_squad.Characters.Remove(character);
                            i--;
                        }
                    }

                    foreach (Character character in ally_squad.Characters)
                    {
                        if (character.ID == current_character.ID)
                        {
                            character.CombatStep = 1;
                            break;
                        }
                    }
                }

                if (enemy_squad != null)
                {
                    for (int i = 0; i < enemy_squad.Characters.Count; i++)
                    {
                        Character character = enemy_squad.Characters[i];
                        if (character.Dead)
                        {
                            enemy_squad.Characters.Remove(character);
                            i--;
                        }
                    }

                    foreach (Character character in enemy_squad.Characters)
                    {
                        if (character.ID == current_character.ID)
                        {
                            character.CombatStep = 1;
                            break;
                        }
                    }
                }

                current_character = null;
            }

            combat_step = 0;
            character_frame = 1;
            effect_frame = 1;
            attack_type = "";
            ep_cost = 0;
            targets.Clear();
        }

        private void FinishRound()
        {
            round++;

            if (ally_squad != null)
            {
                foreach (Character character in ally_squad.Characters)
                {
                    character.CombatStep = 0;
                }
            }

            if (enemy_squad != null)
            {
                foreach (Character character in enemy_squad.Characters)
                {
                    character.CombatStep = 0;
                }
            }

            GetCurrentCharacter();
        }

        private void FinishCombat()
        {
            Handler.CombatTimer.Stop();

            if (!ally_squad.Characters.Any())
            {
                CharacterManager.GetArmy("Ally").Squads.Remove(ally_squad);
            }

            if (!enemy_squad.Characters.Any())
            {
                CharacterManager.GetArmy("Enemy").Squads.Remove(enemy_squad);
            }

            Button button = Menu.GetButton("Result");

            if (!enemy_squad.Characters.Any())
            {
                won_battle = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                button.Text = ally_squad.Name + " was victorious!";
            }
            else if (!ally_squad.Characters.Any())
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                button.Text = ally_squad.Name + " was defeated!";
            }
            else if (enemy_total_damage > ally_total_damage)
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                button.Text = ally_squad.Name + " was defeated!\n\n" +
                    "Allies Total Damage: " + ally_total_damage + "\n" +
                    "Enemies Total Damage: " + enemy_total_damage;
            }
            else if (ally_total_damage > enemy_total_damage)
            {
                won_battle = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                button.Text = ally_squad.Name + " was victorious!\n\n" +
                    "Allies Total Damage: " + ally_total_damage + "\n" +
                    "Enemies Total Damage: " + enemy_total_damage;
            }
            else
            {
                won_battle = false;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Draw"];
                button.Text = "Both parties are retreating.";
            }

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetPicture("Result").Visible = true;
            Menu.GetPicture("MouseClick").Visible = true;
            button.Visible = true;

            if (won_battle)
            {
                Handler.Gold += gold;
                button.Text += "\n\n" + gold + " Gold was looted!";

                Character leader = ally_squad.GetLeader();
                if (leader == null)
                {
                    //If leader was killed, assign new leader
                    ally_squad.Leader_ID = ally_squad.Characters[0].ID;
                }
                else
                {
                    //If leader was not killed, remove enemy as target
                    leader.Target_ID = 0;
                }
            }
        }

        private void Retreat()
        {
            Handler.CombatTimer.Stop();

            won_battle = false;

            Picture battleResult = Menu.GetPicture("Result");
            battleResult.Texture = AssetManager.Textures["Defeat"];
            battleResult.Visible = true;

            Button button = Menu.GetButton("Result");
            button.Text = ally_squad.Name + " is retreating.";

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetPicture("MouseClick").Visible = true;
            button.Visible = true;

            Handler.Gold += gold;
            button.Text += "\n\n" + gold + " Gold was looted!";
        }

        private void Leave()
        {
            Scene localmap = SceneManager.GetScene("Localmap");
            Map map = localmap.World.Maps[Handler.Level];
            Layer ground = map.GetLayer("Ground");

            if (!won_battle &&
                ally_squad.Characters.Any())
            {
                //Bump squad away from enemy
                if (ally_squad.Direction == Direction.North)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X, ally_squad.Location.Y + 1, 0);
                }
                else if (ally_squad.Direction == Direction.East)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X - 1, ally_squad.Location.Y, 0);
                }
                else if (ally_squad.Direction == Direction.South)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X, ally_squad.Location.Y - 1, 0);
                }
                else if (ally_squad.Direction == Direction.West)
                {
                    ally_squad.Location = new Location(ally_squad.Location.X + 1, ally_squad.Location.Y, 0);
                }

                Tile tile = ground.GetTile(new Vector2(ally_squad.Location.X, ally_squad.Location.Y));
                ally_squad.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height);
            }

            Handler.CombatTimer.Stop();
            Handler.Combat = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.Active = true;
            ui.Visible = true;

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;

            SceneManager.ChangeScene("Localmap");

            Main.Timer.Start();
            GameUtil.Toggle_Pause(false);
        }

        private void MainCharacterKilled()
        {
            Handler.CombatTimer.Stop();
            hero_killed = true;

            Button button = Menu.GetButton("Result");
            button.Text = GameUtil.WrapText(ally_squad.Name + " has been slain!\n\nThe story cannot continue without its hero...");

            Menu.GetButton("Retreat").Visible = false;
            Menu.GetPicture("Result").Visible = true;
            Menu.GetPicture("MouseClick").Visible = true;
            button.Visible = true;
        }

        public override void Load()
        {
            Menu.Clear();

            if (!string.IsNullOrEmpty(Handler.Combat_Terrain))
            {
                WorldGen.GenCombatMap();

                ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Combat_Ally_Squad);
                enemy_squad = CharacterManager.GetArmy("Enemy").GetSquad(Handler.Combat_Enemy_Squad);

                if (Handler.Combat_Terrain == "Grass")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Grass"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Water")
                {
                    if (TimeManager.Now.Hours >= 22 ||
                        TimeManager.Now.Hours <= 5)
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Sky_Night2"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Sky_Day"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                }
                else if (Handler.Combat_Terrain == "Desert")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Desert"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Snow" ||
                         Handler.Combat_Terrain == "Ice")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain.Contains("Forest"))
                {
                    if (Handler.Combat_Terrain.Contains("Snow"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Forest_Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Forest"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                }
                else if (Handler.Combat_Terrain.Contains("Mountains"))
                {
                    if (Handler.Combat_Terrain.Contains("Snow"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains_Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else if (Handler.Combat_Terrain.Contains("Desert"))
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains_Desert"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                    else
                    {
                        Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Backdrop_Mountains"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                    }
                }

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    font = AssetManager.Fonts["ControlFont"],
                    name = "Retreat",
                    text = "Retreat",
                    texture = AssetManager.Textures["ButtonFrame"],
                    texture_highlight = AssetManager.Textures["ButtonFrame_Highlight"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    draw_color_selected = Color.White,
                    text_color = Color.White,
                    text_selected_color = Color.Red,
                    enabled = true,
                    visible = true
                });

                Menu.AddPicture(Handler.GetID(), "Result", AssetManager.Textures["Victory"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, false);

                Menu.AddPicture(Handler.GetID(), "MouseClick", AssetManager.Textures["LeftClick"], new Region(0, 0, 0, 0), Color.White, false);
                Picture mouseClick = Menu.GetPicture("MouseClick");
                mouseClick.Image = new Rectangle(0, 0, mouseClick.Texture.Width / 4, mouseClick.Texture.Height);

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    font = AssetManager.Fonts["ControlFont"],
                    name = "Result",
                    texture = AssetManager.Textures["TextFrame"],
                    texture_highlight = AssetManager.Textures["TextFrame"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    draw_color_selected = Color.Red,
                    text_color = Color.Red,
                    text_selected_color = Color.White,
                    enabled = true,
                    visible = false
                });

                Menu.Visible = true;

                Resize(Main.Game.Resolution);

                Handler.CombatTimer.Start();
            }
        }

        public override void Resize(Point point)
        {
            base.Resize(point);

            if (World.Maps.Any())
            {
                Map map = World.Maps[0];

                WorldUtil.Resize_OnCombat(World);

                int height = Main.Game.MenuSize.X;

                Layer ground = map.GetLayer("Ground");
                Tile tile = ground.Tiles[0];
                Menu.GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, tile.Region.Y);

                move_speed = tile.Region.Width / 8;

                Menu.GetPicture("Result").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                Button result = Menu.GetButton("Result");
                result.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 4), 
                    Main.Game.ScreenHeight - (Main.Game.MenuSize.X * 5), Main.Game.MenuSize.X * 8, height * 3);

                Menu.GetButton("Retreat").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 2), result.Region.Y + result.Region.Height, Main.Game.MenuSize.X * 4, height);

                Menu.GetPicture("MouseClick").Region = new Region(result.Region.X + result.Region.Width, result.Region.Y + result.Region.Height - height, height, height);

                Picture base_image = Menu.GetPicture("Base");
                if (base_image != null)
                {
                    base_image.Region = new Region(0, 0, Main.Game.Resolution.X, tile.Region.Y + (tile.Region.Height / 2));
                }
            }
        }

        #endregion
    }
}
