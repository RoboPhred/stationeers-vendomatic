
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
			// replaces base.OnCustomImportFinished()
			__instance.FinishedImporting = true;

			if (GameManager.IsServer
				&& __instance.ImportingThing is Stackable importingStackable)//this also does a null check
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
					// We're done merging
					if (importingStackable.NetworkQuantity == 0) break;
				}
				// Skip adding the now-empty item if we are empty.
				// The empty stack was already deleted when its quantity was set to zero.
				if (importingStackable.NetworkQuantity > 0)
				{
					foreach (Slot slot in __instance.Slots)
					{
						if (!slot.IsInteractable && slot.Occupant == null)
						{
							OnServer.MoveToSlot(importingStackable, slot);
							break;
						}
					}
				}

				OnServer.Interact(__instance.InteractImport, 0, false);
				if (GameManager.GameState == GameState.Running && __instance.CurrentSlot.Occupant == null)
				{
					__instance.PlanForward();
				}
			}
		}
	}
}