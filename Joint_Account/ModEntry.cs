using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace Joint_Account
{
    public class ModEntry : Mod
    {
        private ModConfig config;
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += GameLoopOnDayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoopOnOneSecondUpdateTicked;
            this.config = this.Helper.ReadConfig<ModConfig>();
            
        }

        private Random rand = new Random();
        private void GameLoopOnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            //Every Second, Draw a random number and if it hits, the spouse has spent a bit of money.
            if (!Context.IsWorldReady)
            {
                return;
            }

            
            int hit = 250; //When the spouse makes a purchase
            if (rand.Next(0, 500) == hit)
            {
                var Spouse = GetSpouse();
                int AmountSpent = rand.Next(Spouse.minSpend, Spouse.maxSpend);
                string item = Spouse.thingsBought[rand.Next(0, Spouse.thingsBought.Count)];
                ShowNotification(Spouse.Name + " has spent " + AmountSpent + " on " + item , 1);
            }
        }

        private void GameLoopOnDayStarted(object sender, DayStartedEventArgs e)
        {
            //Player can regularly gets money from Spouses Job - If they are married.
            //How much Money depends on the Spouse.

            if (!Context.IsWorldReady)
            {
                return;
            }

            if (!Game1.player.isMarried())
            {
                
                Monitor.Log("Player is not Married yet.", LogLevel.Info);
                return;
            }

            var Spouse = GetSpouse();
            if (Game1.player.daysMarried == 1)
            {
                //Grant the player the money and create a notification bottom left
                Game1.player._money += Spouse.MarriageBonus;
                ShowNotification("Your Bank Account has been Merged with " + Spouse.Name +"'s.", 1);
            }


            config.PendingMoney += Spouse.Income;

            int DOM = (int)Game1.player.dayOfMonthForSaveGame;
            if (DOM == 6 | DOM == 14 | DOM == 21 | DOM == 28) //Fridays
            {
                Game1.player._money += config.PendingMoney;
                ShowNotification( Spouse.Name + "'s PayCheque has arrived!", 1);
                config.PendingMoney = 0;
            }
            this.Helper.WriteConfig(config);
        }

        public void ShowNotification(string msg, int WhatType)
        {
            var message = new HUDMessage(message: msg, whatType: WhatType);
            Game1.addHUDMessage(message);
        }
        
        private SpouseStats GetSpouse()
        {
            SpouseStats stats = new SpouseStats();
            stats.Name = Game1.player.spouse;
            stats.Income = rand.Next(0, 200); //Base Income, Government Grant?
            switch (Game1.player.spouse)
            {
                case "Alex":
                    stats.Income += 50;
                    stats.MarriageBonus = 1000;
                    //Unknown Job?
                    break;
                case "Elliot":
                    stats.Income += 50;
                    stats.MarriageBonus = 1000;
                    //Unknown Job?
                    break;
                case "Harvey":
                    stats.Income += 400;
                    stats.MarriageBonus = 8000;
                    //Doctor to the Townsfolk, would make a lot of money in addition to the base income.
                    break;
                case "Sam":
                    stats.Income += 200;
                    stats.MarriageBonus = 1500;
                    //In a Band, and works Part Time at Joja-Mart Y1
                    //If possible, lower income if player chooses against Joja?
                    break;
                case "Sebastian":
                    //Son of Wealthy Carpenter
                    stats.Income += 150;
                    stats.MarriageBonus = 4000;
                    break;
                case "Shane":
                    //Works full time at Joja
                    stats.Income += 200;
                    stats.MarriageBonus = 5000;
                    break;
                case "Abigail":
                    //In Band
                    stats.Income += 50;
                    stats.MarriageBonus = 2000;
                    break;
                case "Emily":
                    //Works at Stardrop Saloon
                    stats.Income += 100 + rand.Next(0, 50);
                    stats.MarriageBonus = 3000;
                    break;
                case "Haley":
                    //Has Rich, World Travelling Parents
                    stats.Income += 300;
                    stats.MarriageBonus = 5000;
                    break;
                case "Leah":
                    //Artist? Sculpts.
                    stats.Income += 200;
                    stats.MarriageBonus = 3500;
                    break;
                case "Maru":
                    //Works for Harvey at the Clinic
                    stats.Income += 200;
                    stats.MarriageBonus = 4000;
                    break;
                case "Penny":
                    //Teacher and lives with Pam.
                    stats.Income += 150;
                    stats.MarriageBonus = 5000;
                    break;
                default:
                    stats.Income += 100;
                    stats.MarriageBonus += 5000;
                    break;
            }
            stats.thingsBought.Add("Food");
            stats.thingsBought.Add("Clothing");
            stats.thingsBought.Add("Jewellery");
            if (Game1.player.getNumberOfChildren() > 0)
            {
                stats.thingsBought.Add("Childrens Clothing");
                stats.thingsBought.Add("Toys");
            }
            
            return stats;
        }

        class SpouseStats
        {
            public string Name;
            public int Income;
            public int MarriageBonus;
            public int minSpend = 0;
            public int maxSpend = 500;
            public List<string> thingsBought = new List<string>();
        }

        class ModConfig
        {
            public int PendingMoney = 0;
        }
    }
}