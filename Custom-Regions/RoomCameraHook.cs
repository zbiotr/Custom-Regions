﻿using CustomRegions.Mod;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions
{
    static class RoomCameraHook
    {
        //public delegate void orig_WWW_ctor(WWW self, string url);

        public static void ApplyHooks()
        {

            // If a custom room uses vanilla textures
            On.RoomCamera.MoveCamera2 += RoomCamera_MoveCamera2;

            On.RoomCamera.PreLoadTexture += RoomCamera_PreLoadTexture;

        }

        public static void RemoveHooks()
        {
            // If a custom room uses vanilla textures
            On.RoomCamera.MoveCamera2 -= RoomCamera_MoveCamera2;
            On.RoomCamera.PreLoadTexture -= RoomCamera_PreLoadTexture;
        }

        /// <summary>
        /// Searchs the CustomResources folder for a custom palette if its name is greater than 35. 
        /// CAREFUL! If two mods use the same palette number it will pick the first one it loads.
        /// Also loads effectColor.png
        /// </summary>
        /// 
        internal static void RoomCameraUrl(ref string url)
        {
            if ( (url.Contains("effectColors") || url.Contains("palette")) && url.Contains(".png"))
            {
                //CustomWorldMod.Log("Transforming URL " + url);
                string firstPath = "file:///" + Custom.RootFolderDirectory();
                string trimmedURL = url.Substring(url.IndexOf(firstPath)+  firstPath.Length);
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + trimmedURL;
                    //CustomWorldMod.Log($"Loading effectPalette / palette [{path}]");
                    if (File.Exists(path))
                    {
                        //CustomWorldMod.Log($"Loaded effectPalette / palette [{path}]");
                        url = "file:///" + path;
                        break;
                    }
                }
            }
        }



        private static void RoomCamera_PreLoadTexture(On.RoomCamera.orig_PreLoadTexture orig, RoomCamera self, Room room, int camPos)
        {
            if (self.quenedTexture == string.Empty)
            {
                string requestedTexture = WorldLoader.FindRoomFileDirectory(room.abstractRoom.name, true) + "_" + camPos + 1 + ".png";
                string path = requestedTexture;

                string delimitator = "file:///";
                int index = path.IndexOf(delimitator) + delimitator.Length;
                path = path.Substring(index);

                //CustomWorldMod.CustomWorldLog($"Custom regions: PreloadTexture path [{path}] Exists [{File.Exists(path)}]");
                if (!File.Exists(path))
                {
                    self.quenedTexture = FindCameraTexturePath(requestedTexture);
                    self.www = new WWW(self.quenedTexture);
                }
            }

            orig(self, room, camPos);
        }

        private static void RoomCamera_MoveCamera2(On.RoomCamera.orig_MoveCamera2 orig, RoomCamera self, string requestedTexture)
        {
            string path = requestedTexture;
            string delimitator = "file:///";
            int index = path.IndexOf(delimitator) + delimitator.Length;
            path = path.Substring(index);

            if (!File.Exists(path))
            {
                requestedTexture = FindCameraTexturePath(requestedTexture);
            }
            //CustomWorldMod.CustomWorldLog($"Custom regions: MoveCamera path [{path}] Exists [{File.Exists(path)}]. Requested texture [{requestedTexture}]. Quened texture [{self.quenedTexture}]");



            orig(self, requestedTexture);
        }


        public static string FindCameraTexturePath(string requestedTexture)
        {
            string delimitator = "Regions\\";
            int index = requestedTexture.IndexOf(delimitator) + delimitator.Length;
            string roomPathWithRegion = requestedTexture.Substring(index);

            string fullRoomPathWithRegion = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + roomPathWithRegion;
            // CustomWorldMod.CustomWorldLog($"Custom regions: Searching vanilla room textures at [{fullRoomPathWithRegion}]");
            if (File.Exists(fullRoomPathWithRegion))
            {
                requestedTexture = "file:///" + fullRoomPathWithRegion;
                //CustomWorldMod.CustomWorldLog($"Custom regions: used vanilla textures for room [{requestedTexture}]");
            }

            return requestedTexture;
        }

    }
}
