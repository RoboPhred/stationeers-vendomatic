
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
            // replaces base.OnCustomImportFinished(), since we can't call a base class from outside
            __instance.FinishedImporting = true;

            if (!GameManager.IsServer)
                return;

            //pattern matching also does a null check
            if (__instance.ImportingThing is Stackable importingStackable)
            {
                foreach (Slot slot in __instance.Slots)
                {
                    //find an internal slot with the same item type that has room
                    if (!slot.IsInteractable
                        && slot.Occupant is Stackable occupant
                        && occupant.PrefabHash == importingStackable.PrefabHash
                        && occupant.MaxQuantity > occupant.NetworkQuantity)
                    {
                        OnServer.Merge(occupant, importingStackable);
                    }
                    else
                    {
                        continue;
                    }
                    //fast exit if the import is now empty
                    if (importingStackable.NetworkQuantity == 0) break;
                }

                // It is possible for us to not be empty, in which case a new slot should be started.
                if (importingStackable.NetworkQuantity > 0)
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
    }
}