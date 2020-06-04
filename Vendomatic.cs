
using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Vendomatic
{
    [BepInPlugin("net.robophreddev.stationeers.Vendomatic", "Vending machine QOL Improvements", "1.0.1.0")]
    public class VendomaticPlugin : BaseUnityPlugin
    {
        public static VendomaticPlugin Instance;


        public void Log(string line)
        {
            Debug.Log("[Vendomatic]: " + line);
        }

        void Awake()
        {
            VendomaticPlugin.Instance = this;

            try
            {
                // Harmony.DEBUG = true;
                var harmony = new Harmony("net.robophreddev.stationeers.Vendomatic");
                harmony.PatchAll();
                Log("Patch succeeded");
            }
            catch (Exception e)
            {
                Log("Patch Failed");
                Log(e.ToString());
            }
        }
    }
}