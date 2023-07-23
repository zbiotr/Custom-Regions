﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomRegions.Mod.Structs;
using System.IO;
using UnityEngine;
using CustomRegions.Mod;
using System.Text.RegularExpressions;
using System.CodeDom;

namespace CustomRegions.CustomPearls
{
    internal static class Data
    {
        public static void Refresh()
        {
            Unregister();
            FindCustomPearlData();
            Encryption.EncryptAllCustomPearls();
        }

        public static void FindCustomPearlData()
        {
            try
            {
                CustomDataPearlsList = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl>();
                string customFilePath = AssetManager.ResolveFilePath("CustomPearls.txt");
                if (!File.Exists(customFilePath)) return;
                foreach (string str in File.ReadAllLines(customFilePath))
                {
                    string[] array = Regex.Split(str, " : ");
                    string pearlName = array[0];
                    CustomRegionsMod.CustomLog("Pearl text name is " + pearlName);

                    if (ExtEnumBase.TryParse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName, false, out _))
                    { continue; }

                    DataPearl.AbstractDataPearl.DataPearlType type = RegisterPearlType(pearlName);


                    Color color = RWCustom.Custom.hexToColor(array[1]);
                    Color colorHighlight = RWCustom.Custom.hexToColor(array[2]);
                    string filePath = array[3];

                    CustomPearl pearl = new CustomPearl(type, color, colorHighlight, filePath, RegisterConversations(pearlName));
                    CustomDataPearlsList.Add(type, pearl);
                }
            }
            catch (Exception e) { CustomRegionsMod.CustomLog("Error loading custom pearls" + e, true); }
        }

        public static void Unregister()
        {
            foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> pearl in CustomDataPearlsList)
            {
                pearl.Value.type?.Unregister();
                pearl.Value.conversationID?.Unregister();
            }
            CustomDataPearlsList = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl>();
        }

        public static DataPearl.AbstractDataPearl.DataPearlType RegisterPearlType(string name)
        {
            return new DataPearl.AbstractDataPearl.DataPearlType(name, true);
        }

        public static Conversation.ID RegisterConversations(string name)
        {
            return new Conversation.ID(name, true);
        }


        public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> CustomDataPearlsList = new();


    }
}
