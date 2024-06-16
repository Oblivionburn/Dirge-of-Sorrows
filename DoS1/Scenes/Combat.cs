using System;
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

using DoS1.Util;

namespace DoS1.Scenes
{
    public class Combat : Scene
    {
        #region Variables

        private Squad ally_squad;
        private Squad enemy_squad;

        private List<Character> targets = new List<Character>();
        private Character current_character = null;
        private string attack_type = "";
        private int ep_cost = 0;

        private int character_frame = 1;
        private int effect_frame = 1;
        private int move_speed = 8;

        private int round = 0;
        private int combat_step = 0;

        private int ally_total_damage;
        private int enemy_total_damage;

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
                        picture.Name != "Result")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                World.Draw(spriteBatch, Main.Game.Resolution, RenderingManager.Lighting.DrawColor);

                if (ally_squad != null)
                {
                    CharacterUtil.DrawSquad(spriteBatch, ally_squad, RenderingManager.Lighting.DrawColor);
                }

                if (enemy_squad != null)
                {
                    CharacterUtil.DrawSquad(spriteBatch, enemy_squad, RenderingManager.Lighting.DrawColor);
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

                Menu.GetPicture("Result").Draw(spriteBatch);

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

                            button.Opacity = 0.9f;
                            button.Selected = false;

                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        button.Opacity = 0.9f;
                        button.Selected = false;
                    }
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Result")
            {
                InputManager.Mouse.Flush();

                Army ally_army = CharacterManager.GetArmy("Ally");
                if (ally_army.Squads.Any())
                {
                    Handler.Combat = false;
                    SceneManager.ChangeScene("Localmap");
                    Main.Timer.Start();
                }
                else
                {
                    Handler.Combat = false;

                    SoundManager.StopMusic();
                    SoundManager.NeedMusic = true;

                    SceneManager.ChangeScene("GameOver");
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!TimeManager.Paused)
            {
                if (current_character != null)
                {
                    Tile origin_tile = OriginTile(current_character);
                    Tile target_tile = TargetTile(current_character);

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

                                targets = GetTargets(current_character);
                                if (!targets.Any())
                                {
                                    can_attack = false;
                                    FinishCombat();
                                }

                                if (can_attack)
                                {
                                    attack_type = CharacterUtil.AttackType(current_character);
                                    SwitchAnimation(current_character, attack_type);

                                    combat_step++;
                                }
                                break;

                            case 1:
                                if (attack_type == "Attack")
                                {
                                    if (AtTile(current_character, target_tile))
                                    {
                                        combat_step++;
                                    }
                                    else
                                    {
                                        MoveForward(current_character);
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
                                if (character_frame >= current_character.Animator.Frames * 10)
                                {
                                    Attack();
                                    combat_step++;
                                }
                                else if (character_frame % 10 == 0)
                                {
                                    CharacterUtil.Animate(current_character);

                                    if (attack_type == "Cast")
                                    {
                                        AnimateCastEffect();
                                    }

                                    character_frame++;
                                }
                                else
                                {
                                    character_frame++;
                                }
                                break;

                            case 3:
                                if (effect_frame >= 40)
                                {
                                    combat_step++;
                                }
                                else if (effect_frame % 4 == 0)
                                {
                                    //Animate hit effects
                                    AnimateDamageLabels();
                                    AnimateDamageEffects();
                                    effect_frame++;
                                }
                                else
                                {
                                    effect_frame++;
                                }
                                break;

                            case 4:
                                if (AtTile(current_character, origin_tile))
                                {
                                    FinishAttack();
                                }
                                else
                                {
                                    MoveBack(current_character);
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

        private Tile TargetTile(Character character)
        {
            Map map = World.Maps[0];
            Layer ground = map.GetLayer("Ground");

            if (character.Type == "Ally")
            {
                int x = ground.Columns % 2 == 0 ? (int)Math.Ceiling((double)ground.Columns / 2) - 1 : 
                    (int)Math.Ceiling((double)ground.Columns / 2);

                Tile tile = ground.GetTile(new Vector2(x, character.Formation.Y + 1));
                if (tile != null)
                {
                    return tile;
                }
            }
            else if (character.Type == "Enemy")
            {
                int x = ground.Columns % 2 == 0 ? (int)Math.Ceiling((double)ground.Columns / 2) - 1 : 
                    (int)Math.Ceiling((double)ground.Columns / 2) - 2;

                Tile tile = ground.GetTile(new Vector2(x, character.Formation.Y + 1));
                if (tile != null)
                {
                    return tile;
                }
            }

            return null;
        }

        private Tile OriginTile(Character character)
        {
            Map map = World.Maps[0];

            Layer ground = map.GetLayer("Ground");
            if (ground != null)
            {
                if (character.Type == "Ally")
                {
                    int x = ground.Columns % 2 == 0 ? (int)Math.Ceiling((double)ground.Columns / 2) + 1 :
                        (int)Math.Ceiling((double)ground.Columns / 2) + 2;

                    Tile tile = ground.GetTile(new Vector2(character.Formation.X + x, character.Formation.Y + 1));
                    if (tile != null)
                    {
                        return tile;
                    }
                }
                else if (character.Type == "Enemy")
                {
                    int x = ground.Columns % 2 == 0 ? (int)Math.Ceiling((double)ground.Columns / 2) - 5 :
                        (int)Math.Ceiling((double)ground.Columns / 2) - 6;

                    Tile tile = ground.GetTile(new Vector2(character.Formation.X + x, character.Formation.Y + 1));
                    if (tile != null)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        private bool AtTile(Character character, Tile tile)
        {
            if (character.Region != null &&
                tile.Region != null)
            {
                if (character.Region.X >= tile.Region.X - (move_speed / 2) &&
                    character.Region.X <= tile.Region.X + (move_speed / 2))
                {
                    character.Region = new Region(tile.Region.X, character.Region.Y, character.Region.Width, character.Region.Height);
                    return true;
                }
            }

            return false;
        }

        private void MoveForward(Character character)
        {
            if (character != null &&
                character.Region != null)
            {
                if (character.Type == "Ally")
                {
                    character.Region.X -= move_speed;
                }
                else if (character.Type == "Enemy")
                {
                    character.Region.X += move_speed;
                }
            }
        }

        private void MoveBack(Character character)
        {
            if (character != null &&
                character.Region != null)
            {
                if (character.Type == "Ally")
                {
                    character.Region.X += move_speed;
                }
                else if (character.Type == "Enemy")
                {
                    character.Region.X -= move_speed;
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
                        RenderingManager.Lighting.DrawColor * 0.8f, true);
            }
            else if (current_character.Type == "Enemy")
            {
                Menu.AddPicture(Handler.GetID(), "Cast", AssetManager.Textures["EvilCast"],
                    new Region(current_character.Region.X, current_character.Region.Y, current_character.Region.Width, current_character.Region.Height),
                        RenderingManager.Lighting.DrawColor * 0.8f, true);
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
                    DoDamage(target, weapon);
                }
            }
        }

        private void DoDamage(Character defender, Item weapon)
        {
            if (weapon != null)
            {
                string[] damage_types = { "Physical", "Fire", "Wind", "Earth", "Water" };

                for (int i = 0; i < damage_types.Length; i++)
                {
                    string type = damage_types[i];

                    if (InventoryUtil.Weapon_HasElement(weapon, type))
                    {
                        int damage = InventoryUtil.Get_TotalDamage(weapon, type);

                        Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
                        if (helm != null)
                        {
                            int defense = InventoryUtil.Get_TotalDefense(helm, type);
                            damage -= defense;
                        }

                        Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
                        if (armor != null)
                        {
                            int defense = InventoryUtil.Get_TotalDefense(armor, type);
                            damage -= defense;
                        }

                        Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
                        if (shield != null)
                        {
                            int defense = InventoryUtil.Get_TotalDefense(shield, type);
                            damage -= defense;
                        }

                        if (damage < 0)
                        {
                            damage = 0;
                        }

                        if (damage > 0)
                        {
                            defender.HealthBar.Value -= damage;
                            defender.HealthBar.Update();

                            Color damage_color = Color.White;
                            switch (type)
                            {
                                case "Fire":
                                    damage_color = Color.Red;
                                    break;

                                case "Wind":
                                    damage_color = Color.Yellow;
                                    break;

                                case "Earth":
                                    damage_color = Color.Brown;
                                    break;

                                case "Water":
                                    damage_color = Color.Blue;
                                    break;
                            }

                            AddEffect(defender, weapon, type);

                            if (defender.Type == "Ally")
                            {
                                enemy_total_damage += damage;
                            }
                            else if (defender.Type == "Enemy")
                            {
                                ally_total_damage += damage;
                            }

                            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", damage.ToString(), damage_color,
                                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);

                            if (defender.HealthBar.Value < 0)
                            {
                                defender.HealthBar.Value = 0;
                            }

                            if (defender.HealthBar.Value <= 0)
                            {
                                defender.Dead = true;
                            }
                        }
                        else
                        {
                            AssetManager.PlaySound_Random("Swing");

                            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "0", Color.Black,
                                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);
                        }
                    }
                }
            }
        }

        private void AddEffect(Character character, Item weapon, string type)
        {
            switch (type)
            {
                case "Physical":
                    if (weapon.Categories.Contains("Sword") ||
                        weapon.Categories.Contains("Axe"))
                    {
                        AssetManager.PlaySound_Random("IronSword");

                        float x = character.Region.X - (character.Region.Width / 4);
                        float y = character.Region.Y - (character.Region.Height / 8);
                        float width = character.Region.Width + ((character.Region.Width / 4) * 2);
                        float height = character.Region.Height + ((character.Region.Height / 8) * 2);

                        if (character.Type == "Ally")
                        {
                            Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Slash_Right"],
                                new Region(x, y, width, height),
                                    RenderingManager.Lighting.DrawColor, true);
                        }
                        else if (character.Type == "Enemy")
                        {
                            Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Slash_Left"],
                                new Region(x, y, width, height),
                                    RenderingManager.Lighting.DrawColor, true);
                        }
                    }
                    else if (weapon.Categories.Contains("Mace"))
                    {
                        AssetManager.PlaySound_Random("Thump");

                        Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump"],
                            new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                RenderingManager.Lighting.DrawColor, true);
                    }
                    else if (weapon.Categories.Contains("Bow"))
                    {
                        AssetManager.PlaySound_Random("Bow");

                        if (character.Type == "Ally")
                        {
                            Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Arrow_Right"],
                                new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                    RenderingManager.Lighting.DrawColor, true);
                        }
                        else if (character.Type == "Enemy")
                        {
                            Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Arrow_Left"],
                                new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                    RenderingManager.Lighting.DrawColor, true);
                        }
                    }
                    break;

                case "Fire":
                    AssetManager.PlaySound_Random("Fire");

                    Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Fire"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor * 0.8f, true);
                    break;

                case "Wind":
                    AssetManager.PlaySound_Random("Shock");

                    Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Lightning"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor * 0.8f, true);
                    break;

                case "Earth":
                    AssetManager.PlaySound_Random("Earth");

                    Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Earth"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor, true);
                    break;

                case "Water":
                    AssetManager.PlaySound_Random("Ice");

                    Menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Ice"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor, true);
                    break;
            }

            Picture effect = Get_LastDamagePicture();
            effect.Image = new Rectangle(0, 0, effect.Texture.Width / 4, effect.Texture.Height);
        }

        private List<Character> GetTargets(Character character)
        {
            List<Character> targets = new List<Character>();

            Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
            Squad target_squad = null;

            if (character.Type == "Ally")
            {
                target_squad = enemy_squad;
            }
            else if (character.Type == "Enemy")
            {
                target_squad = ally_squad;
            }

            if (target_squad != null)
            {
                if (weapon != null)
                {
                    if (InventoryUtil.Weapon_IsAoE(weapon))
                    {
                        foreach (Character target in target_squad.Characters)
                        {
                            if (!target.Dead)
                            {
                                targets.Add(target);
                            }
                        }
                    }
                    else if (weapon.Categories.Contains("Axe"))
                    {
                        Character target = GetMeleeTarget(character, target_squad);
                        if (target != null)
                        {
                            targets.Add(target);

                            Character side_target = null;

                            if (character.Formation.Y == 0 &&
                                target.Formation.Y == 0)
                            {
                                side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y + 1));
                            }
                            else if (character.Formation.Y == 1 &&
                                     target.Formation.Y == 1)
                            {
                                side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y - 1));
                                if (side_target != null &&
                                    side_target.Dead)
                                {
                                    side_target = null;
                                }

                                if (side_target == null)
                                {
                                    side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y + 1));
                                }
                            }
                            else if (character.Formation.Y == 2 &&
                                     target.Formation.Y == 2)
                            {
                                side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y - 1));
                            }

                            if (side_target != null &&
                                side_target.Dead)
                            {
                                side_target = null;
                            }

                            if (side_target != null)
                            {
                                targets.Add(side_target);
                            }
                        }
                    }
                    else if (weapon.Categories.Contains("Bow") ||
                             weapon.Categories.Contains("Grimoire"))
                    {
                        Character target = GetRangedTarget(character, target_squad);
                        if (target != null &&
                            !target.Dead)
                        {
                            targets.Add(target);
                        }
                    }
                }
                
                if (!targets.Any())
                {
                    Character target = GetMeleeTarget(character, target_squad);
                    if (target != null)
                    {
                        targets.Add(target);
                    }
                }
            }

            return targets;
        }

        private Character GetMeleeTarget(Character character, Squad target_squad)
        {
            Character target = null;

            if (character.Type == "Ally")
            {
                for (int x = 2; x >= 0; x--)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }
            else if (character.Type == "Enemy")
            {
                for (int x = 0; x < 3; x++)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }

            if (target != null &&
                target.Dead)
            {
                target = null;
            }

            return target;
        }

        private Character GetRangedTarget(Character character, Squad target_squad)
        {
            Character target = null;

            if (character.Type == "Ally")
            {
                for (int x = 0; x < 3; x++)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }
            else if (character.Type == "Enemy")
            {
                for (int x = 2; x >= 0; x--)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }

            if (target != null &&
                target.Dead)
            {
                target = null;
            }

            return target;
        }

        private void SwitchAnimation(Character character, string type)
        {
            if (character.Texture != null)
            {
                string[] parts = character.Texture.Name.Split('_');
                string direction = parts[0];
                string category = parts[1];
                string skin_tone = parts[2];

                string texture = direction + "_" + category + "_" + skin_tone + "_" + type;

                character.Texture = AssetManager.Textures[texture];
                if (character.Texture != null)
                {
                    Item item = InventoryUtil.Get_EquippedItem(character, "Armor");
                    if (item != null)
                    {
                        texture = direction + "_Armor_" + item.Categories[0] + "_" + item.Materials[0] + "_" + type;
                        item.Texture = AssetManager.Textures[texture];
                    }

                    item = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (item != null)
                    {
                        texture = direction + "_Weapon_" + item.Categories[0] + "_" + item.Materials[0] + "_" + type;
                        item.Texture = AssetManager.Textures[texture];
                    }

                    CharacterUtil.ResetAnimation(current_character);
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
                    label.Region.Y -= 2;
                    label.Opacity -= 0.1f;

                    if (label.Opacity <= 0)
                    {
                        Menu.Labels.Remove(label);
                        i--;
                    }
                }
            }
        }

        private Picture Get_LastDamagePicture()
        {
            for (int i = Menu.Pictures.Count - 1; i >= 0; i--)
            {
                Picture picture = Menu.Pictures[i];
                if (picture.Name == "Damage")
                {
                    return picture;
                }
            }

            return null;
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
                SwitchAnimation(current_character, "Idle");

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

            bool ally_defeat = true;
            if (ally_squad.Characters.Any())
            {
                ally_defeat = false;
            }
            else
            {
                CharacterManager.GetArmy("Ally").Squads.Remove(ally_squad);
            }

            bool enemy_defeat = true;
            if (enemy_squad.Characters.Any())
            {
                enemy_defeat = false;
            }
            else
            {
                CharacterManager.GetArmy("Enemy").Squads.Remove(enemy_squad);
            }

            Button button = Menu.GetButton("Result");

            if (ally_defeat)
            {
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                button.Text = ally_squad.Name + " lost!";
            }
            else if (enemy_defeat)
            {
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                button.Text = ally_squad.Name + " won!";
            }
            else if (enemy_total_damage > ally_total_damage)
            {
                ally_defeat = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Defeat"];
                button.Text = ally_squad.Name + " lost!\n\n" +
                    ally_squad.Name + " Total Damage: " + ally_total_damage + "\n" +
                    enemy_squad.Name + " Total Damage: " + enemy_total_damage;
            }
            else if (ally_total_damage > enemy_total_damage)
            {
                enemy_defeat = true;
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Victory"];
                button.Text = ally_squad.Name + " won!\n\n" +
                    ally_squad.Name + " Total Damage: " + ally_total_damage + "\n" +
                    enemy_squad.Name + " Total Damage: " + enemy_total_damage;
            }
            else
            {
                Menu.GetPicture("Result").Texture = AssetManager.Textures["Draw"];
                button.Text = "Both parties are retreating.";
            }

            button.Text += "\n\n(Clear here to continue)";

            Menu.GetPicture("Result").Visible = true;
            button.Visible = true;

            if (enemy_defeat)
            {
                //Reward
            }
        }

        public override void Load()
        {
            Menu.Clear();

            if (!string.IsNullOrEmpty(Handler.Combat_Terrain))
            {
                WorldGen.GenCombatMap();
                LoadCharacters();

                Menu.AddPicture(Handler.GetID(), "Light", AssetManager.Textures["White"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);

                if (TimeManager.Now.Hours >= 10 ||
                    TimeManager.Now.Hours <= 5)
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Sky_Night"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Grass")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Sky_Day"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Desert")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Desert"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain == "Snow" ||
                         Handler.Combat_Terrain == "Ice")
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Snow"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain.Contains("Forest"))
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Forest"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Terrain.Contains("Mountains"))
                {
                    Menu.AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Mountains"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }

                if (Handler.Combat_Ally_Base)
                {
                    Menu.AddPicture(Handler.GetID(), "Base", AssetManager.Textures["Base_Ally"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }
                else if (Handler.Combat_Enemy_Base)
                {
                    Menu.AddPicture(Handler.GetID(), "Base", AssetManager.Textures["Base_Enemy"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, true);
                }

                Menu.AddPicture(Handler.GetID(), "Result", AssetManager.Textures["Victory"], new Region(0, 0, 0, 0), RenderingManager.Lighting.DrawColor, false);

                Menu.AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    font = AssetManager.Fonts["ControlFont"],
                    name = "Result",
                    texture = AssetManager.Textures["TextFrame"],
                    texture_highlight = AssetManager.Textures["TextFrame"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White * 0.9f,
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

        private void LoadCharacters()
        {
            if (World.Maps.Any())
            {
                enemy_squad = CharacterManager.GetArmy("Enemy").GetSquad(Handler.Combat_Enemy_Squad);
                if (enemy_squad != null)
                {
                    foreach (Character character in enemy_squad.Characters)
                    {
                        Tile tile = OriginTile(character);
                        if (tile != null)
                        {
                            character.Region = new Region(tile.Region.X, tile.Region.Y - tile.Region.Height, tile.Region.Width, tile.Region.Height * 1.5f);

                            float bar_x = character.Region.X + (character.Region.Width / 8);
                            float bar_width = (character.Region.Width / 8) * 6;
                            float bar_height = character.Region.Width / 8;

                            character.HealthBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height, bar_width, bar_height);
                            character.HealthBar.Visible = true;
                            character.HealthBar.Update();

                            character.ManaBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height + bar_height, bar_width, bar_height);
                            character.ManaBar.Visible = true;
                            character.ManaBar.Update();

                            CharacterUtil.UpdateGear(character);
                        }
                    }
                }

                ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Combat_Ally_Squad);
                if (ally_squad != null)
                {
                    foreach (Character character in ally_squad.Characters)
                    {
                        Tile tile = OriginTile(character);
                        if (tile != null)
                        {
                            character.Region = new Region(tile.Region.X, tile.Region.Y - tile.Region.Height, tile.Region.Width, tile.Region.Height * 1.5f);

                            float bar_x = character.Region.X + (character.Region.Width / 8);
                            float bar_width = (character.Region.Width / 8) * 6;
                            float bar_height = character.Region.Width / 8;

                            character.HealthBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height, bar_width, bar_height);
                            character.HealthBar.Visible = true;
                            character.HealthBar.Update();

                            character.ManaBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height + bar_height, bar_width, bar_height);
                            character.ManaBar.Visible = true;
                            character.ManaBar.Update();

                            CharacterUtil.UpdateGear(character);
                        }
                    }
                }
            }
        }

        public override void Resize(Point point)
        {
            base.Resize(point);

            if (World.Maps.Any())
            {
                WorldUtil.Resize_OnCombat(World.Maps[0]);

                Menu.GetPicture("Light").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);
                Menu.GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);
                Menu.GetPicture("Result").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                Menu.GetButton("Result").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 4), 
                    Main.Game.ScreenHeight - (Main.Game.MenuSize.X * 5), Main.Game.MenuSize.X * 8, Main.Game.MenuSize.X * 3);

                Picture base_image = Menu.GetPicture("Base");
                if (base_image != null)
                {
                    base_image.Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);
                }
            }
        }

        #endregion
    }
}
