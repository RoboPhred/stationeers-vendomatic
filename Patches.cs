
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using HarmonyLib;

namespace Vendomatic
{

    [HarmonyPatch(typeof(VendingMachine), "OnCustomImportFinished")]
    class VendingMachineOnCustomImportFinishedPatch
    {
        static bool Prefix(VendingMachine __instance)
        {
            VendingMachineOnCustomImportFinishedPatch.PatchedOnCustomImportFinished(__instance);
            return false;
        }

        static void PatchedOnCustomImportFinished(VendingMachine __instance)
        {
            VendingMachineOnCustomImportFinishedPatch.CallBaseCustomImportFinished(__instance);

            if (!GameManager.IsServer)
                return;

            if (__instance.ImportingThing)
            {
                var importing = __instance.ImportingThing;
                var skipNewSlot = false;

                var importingStackable = __instance.ImportingThing as Stackable;
                if (importingStackable)
                {
                    foreach (Slot slot in __instance.Slots)
                    {
                        if (slot.IsInteractable)
                        {
                            continue;
                        }

                        var occupant = slot.Occupant as Stackable;
                        if (!occupant)
                        {
                            continue;
                        }

                        if (occupant.PrefabName != importingStackable.PrefabName)
                        {
                            continue;
                        }

                        OnServer.Merge(occupant, importingStackable);
                        if (importingStackable.NetworkQuantity == 0)
                        {
                            // Skip adding the now-empty item if we are empty.
                            // The empty stack was already deleted when its quantity was set to zero.
                            skipNewSlot = true;
                        }
                        else
                        {
                            // It is possible for us to not be empty, in which case a new slot should be started.
                        }

                        break;
                    }
                }

                if (!skipNewSlot)
                {
                    foreach (Slot slot in __instance.Slots)
                    {
                        if (!slot.IsInteractable && !slot.Occupant)
                        {
                            OnServer.MoveToSlot(__instance.ImportingThing, slot);
                            break;
                        }
                    }
                }
            }

            OnServer.Interact(__instance.InteractImport, 0, false);

            if (GameManager.GameState == GameState.Running && !__instance.CurrentSlot.Occupant)
            {
                __instance.PlanForward();
            }
        }

        static void CallBaseCustomImportFinished(VendingMachine __instance)
        {
            // Cant find a way to do this from harmony, so reimplement it.
            __instance.FinishedImporting = true;
        }
    }
}