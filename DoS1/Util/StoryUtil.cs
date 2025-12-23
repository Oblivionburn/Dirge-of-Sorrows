using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Enums;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class StoryUtil
    {
        public static string HisHer(string gender)
        {
            if (gender == "Male")
            {
                return "his";
            }

            return "her";
        }

        public static string HimHer(string gender)
        {
            if (gender == "Male")
            {
                return "him";
            }

            return "her";
        }

        public static string HeShe(string gender)
        {
            if (gender == "Male")
            {
                return "he";
            }

            return "she";
        }

        public static void Alert_Story(Menu menu)
        {
            Handler.AlertType = "Story";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;

            Tutorial_Worldmap(dialogue, dialogue_name);
            Intro(menu, alerts, dialogue, dialogue_name);
            Tutorial_Formation(dialogue, dialogue_name);
            Tutorial_Movement(dialogue, dialogue_name);
            Tutorial_Market(alerts, dialogue, dialogue_name);
            Tutorial_Equipment(dialogue, dialogue_name);
            Intro_Part2(alerts, dialogue, dialogue_name);
            Tutorial_Runes(dialogue, dialogue_name);
            Intro_Part3(alerts, dialogue, dialogue_name);
            Tutorial_Combat(alerts, dialogue, dialogue_name);
            Intro_Part4(alerts, dialogue, dialogue_name);
            Intro_Part5(alerts, dialogue, dialogue_name);
            Tutorial_ClaimRegion(alerts, dialogue, dialogue_name);
            Intro_Part6(alerts, dialogue, dialogue_name);
            Tutorial_Squads(alerts, dialogue, dialogue_name);
            TheEnd(alerts, dialogue, dialogue_name);
        }

        public static void Alert_Story(Squad squad, Character king)
        {
            Handler.AlertType = "Story";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;

            TheKing_Part1(alerts, dialogue, dialogue_name, squad, king);
            TheKing_Part2(alerts, dialogue, dialogue_name, king);
        }

        public static void Tutorial_Worldmap(Label dialogue, Label dialogue_name)
        {
            string message = "";

            if (Handler.StoryStep == 0)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Left-click the red castle on the Worldmap to enter the Local Map of that location." +
                    "\n\nIf the red castle is not visible, left-click and drag the map until you can see it.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Intro(Menu menu, Menu alerts, Label dialogue, Label dialogue_name)
        {
            Army ally = CharacterManager.GetArmy("Ally");
            Squad squad = ally.Squads[0];
            Character hero = squad.GetLeader();

            Character friend = Handler.GetFriend();

            Army special = CharacterManager.GetArmy("Special");
            Character spouse = special.Squads[0].Characters[0];

            string message = "";

            if (Handler.StoryStep == 1)
            {
                Scene scene = WorldUtil.GetScene();
                Map map = scene.World.Maps[Handler.Level];
                Layer ground = map.GetLayer("Ground");
                Tile tile = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));

                WorldUtil.CameraToTile(menu, map, ground, tile);
                GameUtil.LocalPause();

                dialogue_name.Text = "Narrator";

                if (hero.Gender == "Male")
                {
                    message = "\"Your wife approaches as you're tilling the soil of your farm's grain field...\"";
                }
                else
                {
                    message = "\"Your husband takes a break from tilling the soil of your farm's grain field and comes into the house for some water...\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 2)
            {
                GameUtil.LocalPause();

                Handler.Dialogue_Character2 = spouse;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = spouse.Name;

                if (hero.Gender == "Male")
                {
                    message = "\"I think we're short on grain seed this season. You should take a break from the field and head to the market to buy some more.\"";
                }
                else
                {
                    message = "\"I think we're short on grain seed this season. Could you head to the market to buy some more?\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 3 &&
                     friend != null)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "Narrator";

                if (hero.Gender == "Male")
                {
                    message = "\"You follow your wife back to the house to clean yourself up before heading to the market. Just as you're finishing, you hear a knock" +
                        " on the door and can tell from the friendly greeting that it's your best friend, " + friend.Name + ".\"";
                }
                else
                {
                    message = "\"You start to clean yourself up for heading to the market. Just as you're finishing, you hear someone enter the house and peek out to" +
                        " see it's your best friend, " + friend.Name + ".\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 4 &&
                     friend != null)
            {
                GameUtil.LocalPause();

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;

                if (hero.Gender == "Male")
                {
                    message = "\"Your wife tells me you're heading to the market for some grain. I need to get a few things from the market as well... mind if I tag along?\"";
                }
                else
                {
                    message = "\"Your husband told me you're heading to the market for some grain. I need to get a few things from the market as well... mind if I tag along?\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Formation(Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            string message = "";

            if (Handler.StoryStep == 5)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Your starting squad, represented by a blue token on the map, will always be deployed at your Base when entering a Local Map. You can right-click" +
                    " any squad positioned at a Town or your Base to edit them.\n\nRight-click your starting squad now to edit it.";
            }
            else if (Handler.StoryStep == 6 &&
                     friend != null)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Left-click and drag " + friend.Name + " from your Reserves (on the right) to a position in your squad's Formation (on the left).";
            }
            else if (Handler.StoryStep == 7)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Squad Menu.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Movement(Label dialogue, Label dialogue_name)
        {
            Menu alerts = MenuManager.GetMenu("Alerts");

            Army ally = CharacterManager.GetArmy("Ally");
            Squad squad = ally.Squads[0];

            Character friend = Handler.GetFriend();

            string message = "";

            if (Handler.StoryStep == 8 &&
                friend != null)
            {
                GameUtil.LocalPause();

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;

                Tile market = WorldUtil.GetMarket();
                Direction direction = WorldUtil.Get_Direction(new Vector2(squad.Location.X, squad.Location.Y), new Vector2(market.Location.X, market.Location.Y));

                message = "\"The nearest market is at " + market.Name + ", which is " + direction.ToString() + " of here. I'm ready when you are!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 9)
            {
                Tile market = WorldUtil.GetMarket();

                dialogue_name.Text = "System";
                message = "Any Town on the map with a gold outline will possess a market. Left-click your squad to begin moving it, and then left-click " + market.Name +
                    " to set that as your destination.\n\nYou can zoom in/out the map with the Scroll Wheel.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Market(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            string message = "";

            if (Handler.StoryStep == 10 &&
                friend != null)
            {
                dialogue_name.Text = "Narrator";
                message = "\"Upon entering the market, you quickly found the seeds you were needing. You and " + friend.Name + " continued looking around to see" +
                    " what else was being sold...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 11 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Did you ever get that Cloth Helm you were wanting for protecting your face from mosquitoes out in the field? Maybe you should buy" +
                    " one while we're here.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 12)
            {
                dialogue_name.Text = "System";
                message = "Items in the market (on the right) can be right-clicked to purchase them. Items in your inventory (on the left) can be right-clicked" +
                    " to sell them.\n\nPurchase a Cloth Helm from the market.";
            }
            else if (Handler.StoryStep == 13)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Exit Market button in the bottom-center to leave the Market Menu.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Equipment(Label dialogue, Label dialogue_name)
        {
            Character hero = Handler.GetHero();

            string message = "";

            if (Handler.StoryStep == 14)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click your squad while they're at a Town so you can edit it and equip your new helm.";
            }
            else if (Handler.StoryStep == 15)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click " + hero.Name + " in the formation to change their equipment.";
            }
            else if (Handler.StoryStep == 16)
            {
                dialogue_name.Text = "System";
                message = "Left-click and drag the Cloth Helm from your inventory to the Helm Slot in order to equip it.";
            }
            else if (Handler.StoryStep == 17)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Character Menu.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Intro_Part2(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Army ally = CharacterManager.GetArmy("Ally");
            Squad squad = ally.Squads[0];
            Character hero = squad.GetLeader();

            Character friend = Handler.GetFriend();

            Army special = CharacterManager.GetArmy("Special");
            Character spouse = special.Squads[0].Characters[0];

            string message = "";

            if (Handler.StoryStep == 19 &&
                friend != null)
            {
                GameUtil.LocalPause();

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                Scene scene = WorldUtil.GetScene();
                Map map = WorldUtil.GetMap(scene.World);
                Tile ally_base = WorldUtil.Get_Base(map, "Ally");

                dialogue_name.Text = friend.Name;
                message = "\"Let's head back to " + ally_base.Name + " to show " + spouse.Name + " your new helm... I can't wait to see the look on " + HisHer(spouse.Gender) +
                    " face. I'm sure " + HeShe(spouse.Gender) + " will love it!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 22)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "Narrator";
                message = "\"As you approach your fields, you see smoke rising in the distance down the road.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 23 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Where's all that smoke coming from? Oh no... your house is on fire! " + spouse.Name + "!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 24 &&
                     friend != null)
            {
                dialogue_name.Text = "Narrator";
                message = "\"You and " + friend.Name + " run down the road to find " + spouse.Name + ", but as you approach your burning home you can see " + HimHer(spouse.Gender) +
                    " laying dead on the road in a pool of blood.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 25)
            {
                dialogue_name.Text = "Narrator";
                message = "\"With tears streaming down your face, you bend down to " + HisHer(spouse.Gender) + " body and see many long gashes across " + HisHer(spouse.Gender) +
                    " torso. Someone had slashed " + HimHer(spouse.Gender) + " repeatedly with a sword.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 26 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "(heavy sigh) \"" + hero.Name + "... I'm so sorry, but we need to arm ourselves while we can and find who did this. " + spouse.Name +
                    " deserves justice.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 27 &&
                     friend != null)
            {
                dialogue_name.Text = "Narrator";
                message = "\"You and " + friend.Name + " enter a nearby shed to retrieve some weapons.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Runes(Label dialogue, Label dialogue_name)
        {
            string message = "";

            if (Handler.StoryStep == 28)
            {
                dialogue_name.Text = "System";
                message = "Right-click your squad to equip your characters with a weapon.";
            }
            else if (Handler.StoryStep == 29)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click a character to equip them with a weapon.";
            }
            else if (Handler.StoryStep == 30)
            {
                int width = Main.Game.MenuSize.X;
                int height = Main.Game.MenuSize.X;
                int Y = Main.Game.ScreenHeight - (height * 6);

                dialogue.Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Y, width * 10, height * 4);

                int name_height = (int)(dialogue.Region.Height / 6);
                dialogue_name.Region = new Region(dialogue.Region.X + (width * 3), dialogue.Region.Y - name_height, dialogue.Region.Width - (width * 6), name_height);

                dialogue_name.Text = "System";
                message = "Above your inventory on the right are buttons to filter which items are visible in the grid. Click the Weapons filter button to view" +
                    " your weapons.";
            }
            else if (Handler.StoryStep == 31)
            {
                dialogue_name.Text = "System";
                message = "Left-click and drag a weapon to the character's Weapon Slot.";
            }
            else if (Handler.StoryStep == 32)
            {
                dialogue_name.Text = "System";
                message = "Right-click the weapon you just equipped to attach a rune to it for extra damage.";
            }
            else if (Handler.StoryStep == 33)
            {
                dialogue_name.Text = "System";
                message = "The grid on the right displays all runes in your inventory. Left-click and drag a rune to a Rune Slot under your weapon to attach it.";
            }
            else if (Handler.StoryStep == 34)
            {
                int width = Main.Game.MenuSize.X;
                int height = Main.Game.MenuSize.X;
                int Y = Main.Game.ScreenHeight - (height * 7);

                dialogue.Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Y, width * 10, height * 4);

                int name_height = (int)(dialogue.Region.Height / 6);
                dialogue_name.Region = new Region(dialogue.Region.X + (width * 3), dialogue.Region.Y - name_height, dialogue.Region.Width - (width * 6), name_height);

                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Item Menu.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Intro_Part3(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            Army special = CharacterManager.GetArmy("Special");
            Character spouse = special.Squads[0].Characters[0];

            string message = "";

            if (Handler.StoryStep == 35)
            {
                dialogue_name.Text = "System";
                message = "Equip both characters with a weapon before leaving the Squad Menu.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 36)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "Narrator";
                message = "\"As you exit the shed, a heavily armed person appears from the back of your house carrying bags of items they must've stolen after" +
                    " murdering " + spouse.Name + ".\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 37 &&
                     friend != null)
            {
                dialogue_name.Text = friend.Name;
                message = "(yells angrily) \"There they are! Get them!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 38)
            {
                Squad enemy_squad = special.Squads[1];
                Character enemy = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = enemy;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = enemy.Name;
                message = "\"I see you found my warning. Don't worry, " + HeShe(spouse.Gender) + " didn't scream... much.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 39)
            {
                Squad enemy_squad = special.Squads[1];
                Character enemy = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = enemy;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = enemy.Name;
                message = "(laughing) \"Perhaps you should've paid your taxes on time?\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 40 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"We've had enough of your warnings! Every month you ask for more taxes than the last! It's thievery and extortion!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 41)
            {
                Squad special_squad = special.Squads[1];
                Character enemy = special_squad.Characters[0];

                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = enemy;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = enemy.Name;
                message = "\"Go cry about it to " + local_lord.Name + "... I'm just following " + HisHer(local_lord.Gender) + " orders.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 42 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"This ends here!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Combat(Menu alerts, Label dialogue, Label dialogue_name)
        {
            string message = "";

            if (Handler.StoryStep == 43)
            {
                dialogue_name.Text = "System";
                message = "Combat is automatic and death is permanent. Prepare your equipment prior to combat for the best chance of success.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 44)
            {
                dialogue_name.Text = "System";
                message = "Excluding story events, if a character's death is imminent you can click the Retreat button to end combat early and live to fight another day.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Intro_Part4(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            Army special = CharacterManager.GetArmy("Special");
            Character spouse = special.Squads[0].Characters[0];

            string message = "";

            if (Handler.StoryStep == 45 &&
                friend != null)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "Narrator";
                message = "\"With the murder of " + spouse.Name + " now avenged, " + friend.Name + " helped you bury " + HisHer(spouse.Gender) + " body.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 46 &&
                     friend != null)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"We must put a stop to " + local_lord.Name + "... " + HeShe(local_lord.Gender) + "'s taken too much from us, and I fear this madness will" +
                    " never end until we stop " + HimHer(local_lord.Gender) + " for good.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 47 &&
                     friend != null)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Many friends and neighbors in the area have also suffered under " + HisHer(local_lord.Gender) + " tyranny as you have." +
                    " I'm sure they would aid us in the coming battle if we went to the Academy and plead our case to bolster our Reserves. We could also go to the" +
                    " Market for more equipment. Whichever you decide, I'll follow your lead.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 48)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                dialogue_name.Text = "System";
                message = "You're now free to do as you please, and your objective is clear: kill " + local_lord.Name + ".\n\n- If your HP/EP is low, you can park your" +
                    " squad at a Town/Base to recover 1 HP/EP per minute.\n- You will now gain 1 Gold per Town you control at the start of every day.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Intro_Part5(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            string message = "";

            if (Handler.StoryStep == 50)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = local_lord;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = local_lord.Name;
                message = "\"I don't recall having any appointments today... what do you want?\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 51 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"We're here to put an end to your tyranny and extortion!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 52)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = local_lord;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = local_lord.Name;
                message = "\"My... extortion? You simple fool. It's not I extorting you, it's your King! I just gather the gold he requests, by whatever means necessary," +
                    " else it's my own head going in the gutter and I can't abide that... you see, I'm rather attached to it.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 53 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Then you're as guilty as the King for being complicit... and we'll tolerate no more of this! We will cut you all down like the snakes you" +
                    " are, until the very head of your King is rolling at our feet! You will know our suffering, until there is none left to fear us!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 54)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = local_lord;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = local_lord.Name;
                message = "\"I see. Well, I guess you leave me no choice... and I'm not going down without a fight!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_ClaimRegion(Menu alerts, Label dialogue, Label dialogue_name)
        {
            string message = "";

            if (Handler.StoryStep == 55)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Well done! To finish this tutorial map, and every subsequent map, just park your squad at the enemy's Base to capture it and enable the" +
                    " \"Return to Worldmap\" button in the upper-left corner of the screen which will let you proceed to the next map.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Intro_Part6(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            string message = "";

            if (Handler.StoryStep == 57 &&
                friend != null)
            {
                GameUtil.LocalPause();

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"I've been thinking... our desire for justice may be pure, but we won't survive taking this fight all the way to the King by ourselves." +
                    " Eventually we'll run out of energy if this war consists of just you and I.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 58 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Our best chance of winning this fight is to start building an army of our own. I know some people in this area that might be willing" +
                    " to join us... wait here for a bit while I go talk to them...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 60)
            {
                GameUtil.LocalPause();

                Handler.TimeSpeed = Handler.TimeSpeed_Temp;
                GameUtil.UpdateSpeed();

                Army reserves = CharacterManager.GetArmy("Reserves");
                Squad squad = reserves.Squads[0];

                CryptoRandom random = new CryptoRandom();
                Handler.recruit1 = CharacterUtil.NewCharacter_Random(new Vector2(0, 0), false, random.Next(0, 2));
                squad.AddCharacter(Handler.recruit1);

                Item armor1 = InventoryUtil.AddItem(Handler.recruit1.Inventory, "Cloth", "Cloth", "Armor");
                InventoryUtil.EquipItem(Handler.recruit1, armor1);

                random = new CryptoRandom();
                Handler.recruit2 = CharacterUtil.NewCharacter_Random(new Vector2(0, 0), false, random.Next(0, 2));
                squad.AddCharacter(Handler.recruit2);

                Item armor2 = InventoryUtil.AddItem(Handler.recruit2.Inventory, "Cloth", "Cloth", "Armor");
                InventoryUtil.EquipItem(Handler.recruit2, armor2);

                Handler.StoryStep++;
            }
            else if (Handler.StoryStep == 61 &&
                     friend != null)
            {
                dialogue_name.Text = "Narrator";
                message = "\"" + friend.Name + " returns some time later with two people following close behind " + HimHer(friend.Gender) + ".\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 62 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"We're by no means an army, yet, but this is at least a start.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 63)
            {
                GameUtil.LocalPause();

                dialogue_name.Text = "System";
                message = "Left-click the Army button in the upper-left corner.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void Tutorial_Squads(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character friend = Handler.GetFriend();

            string message = "";

            if (Handler.StoryStep == 64)
            {
                dialogue_name.Text = "System";
                message = "Left-click the Add Squad button in the upper-left corner to create a new squad.";
            }
            else if (Handler.StoryStep == 65)
            {
                dialogue_name.Text = "System";
                message = "Right-click your new squad to edit it.";
            }
            else if (Handler.StoryStep == 66)
            {
                dialogue_name.Text = "System";
                message = "Left-click and drag " + Handler.recruit1.Name + " and " + Handler.recruit2.Name + " from your Reserves (on the right) to a position in your squad's Formation (on the left).";
            }
            else if (Handler.StoryStep == 67)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Squad Menu.";
            }
            else if (Handler.StoryStep == 68)
            {
                dialogue_name.Text = "System";
                message = "Left-click your new squad to select it.";
            }
            else if (Handler.StoryStep == 69)
            {
                dialogue_name.Text = "System";
                message = "Left-click the Deploy Squad button in the upper-left corner to add your new squad on the Local Map at your Base.";
            }
            else if (Handler.StoryStep == 70)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Army Menu.";
            }
            else if (Handler.StoryStep == 71 &&
                     friend != null)
            {
                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Our new recruits are unarmed, but they brought 500 gold with them... we just need to send them to the nearest market to get some" +
                    " equipment. Would be best if they avoided combat in the meantime, so as not to get them killed before they become useful.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void TheKing_Part1(Menu alerts, Label dialogue, Label dialogue_name, Squad squad, Character king)
        {
            Character leader = squad.GetCharacter(squad.Leader_ID);

            string message = "";

            if (Handler.StoryStep == 73)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The King stands tall and defiant before you, his guards tense at his side.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 74 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"... and here you are... ready to strike me down as you've done to so many others...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 75)
            {
                Handler.Dialogue_Character2 = leader;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = leader.Name;
                message = "\"You are a greedy tyrant that must be stopped! You have abused and extorted everyone in your kingdom for too long... " +
                    "and we are here to end it!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 76 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"You think my deeds are merely driven by greed? How naive... I took what was needed for the good of the kingdom.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 77 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"Without gold, we can buy no weapons, and without weapons we are defenseless against the other realms. I did what needed to be done " +
                    "for the safety of all.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 78)
            {
                Handler.Dialogue_Character2 = leader;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = leader.Name;
                message = "\"Safe? People are being murdered so a few more coins can be added to your coffers, yet you stand here preaching about keeping us safe?\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 79 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"If some sacrifices were made to secure the future of our kingdom, then they were necessary.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 80)
            {
                Handler.Dialogue_Character2 = leader;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = leader.Name;
                message = "\"If the price of your future must be paid in blood, then that is too high a cost for anyone to bare. Your future ends here!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 81 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"If you will not share my future, then in death you will become my past!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                Handler.CombatFinishing = true;
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void TheKing_Part2(Menu alerts, Label dialogue, Label dialogue_name, Character king)
        {
            Character hero = Handler.GetHero();

            string message = "";

            if (Handler.StoryStep == 83)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The King falls to his knees, coughing blood as he clasps at his wounds...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 84 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"You think you've won? (cough) You think this will stop anything? Even I am merely a pawn in a bigger game.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 85 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"(cough) We're all just... puppets (cough)... of The Merchant's Guild. They make mountains of gold (cough) from turning us " +
                    "against each other... whispering 'War' in our ears, so they can sell more weapons to everyone.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 86 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"We needed their weapons to keep our kingdom safe. You wouldn't have made it this far if they hadn't sold them to you as well. " +
                    "They control all the markets... they could have refused you at any point.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 87 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "\"You're as much a pawn in their game as the rest of us. Enjoy the throne... for as long as they let you hold it. It's only a " +
                    "matter of time before they turn the other kingdoms against you.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 88)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The King gasps and coughs, struggling to squeeze out his final words...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 89 &&
                     king != null)
            {
                Handler.Dialogue_Character1 = king;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = king.Name;
                message = "whispers \"I tried... I tried to keep us safe. But we were never safe... nobody is safe...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 90)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The King lets out a long drawn sigh as his eyes turn lifeless and the final vestiges of life leave his body.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 91)
            {
                dialogue_name.Text = "Narrator";

                if (hero.Gender == "Male")
                {
                    message = "\"The King is dead. Long live the new King, " + hero.Name + ".\"";
                }
                else
                {
                    message = "\"The King is dead. Long live the new Queen, " + hero.Name + ".\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                Handler.CombatFinishing = true;
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }

        public static void TheEnd(Menu alerts, Label dialogue, Label dialogue_name)
        {
            Character hero = Handler.GetHero();

            string message = "";

            if (Handler.StoryStep == 93)
            {
                dialogue_name.Text = "Narrator";
                message = "\"With the King now dead, " + hero.Name + " ascended the throne and the kingdom once again knew peace.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 94)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The contract with The Merchant's Guild for the sale of new weapons in perpetuity was immediately torn up, as the kingdom " +
                    "had more than enough to safeguard itself, and this enabled the taxes to be reduced to an amount that the citizens of the kingdom " +
                    "found reasonable.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 95)
            {
                dialogue_name.Text = "Narrator";
                message = "\"No more innocent blood would be shed out of greed and threats of war, but the previous King's dying words haunted " + hero.Name + " as "
                    + HeShe(hero.Gender) + " lay in bed at night...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 96)
            {
                dialogue_name.Text = "Narrator";
                message = "\"Would The Merchant's Guild really turn the other kingdoms against " + HimHer(hero.Gender) + " in retaliation for no longer " +
                    "purchasing their weapons? How long would this peace really last?\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 97)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The Merchant's Guild were a bigger threat on the horizon, and it was only a matter of time before they would need to be " +
                    "dealt with, but that's a tale for another time...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 98)
            {
                dialogue_name.Text = "Narrator";
                message = "\"The End.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            if (!string.IsNullOrEmpty(message))
            {
                dialogue.Text = GameUtil.WrapText_Dialogue(message);
            }
        }
    }
}
