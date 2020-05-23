using Assets.Scripts;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using static Assets.Scripts.Objects.Thing;

namespace Vendomatic
{
    [HarmonyPatch(typeof(VendingMachine), "InteractWith")]
    class VendingMachine_InteractWith
    {
        static bool Prefix(
            VendingMachine __instance,
            ref DelayedActionInstance __result,
            Interactable interactable,
            Interaction interaction,
            bool doAction = true
        )
        {
            if (interactable.Action == InteractableType.Activate)
            {
                // Take over the activate command.
                VendomaticPlugin.Instance.Log("Vending machine activation.  Hyjacking execution");
                VendingMachine_InteractWith.VendingMachineActivate(__instance, ref __result, interactable, interaction, doAction);
                return false;
            }
            return true;
        }

        static void VendingMachineActivate(
            VendingMachine __instance,
            ref DelayedActionInstance __result,
            Interactable interactable,
            Interaction interaction,
            bool doAction
        )
        {
            // General usage checks
            DelayedActionInstance delayedActionInstance = new DelayedActionInstance
            {
                Duration = 0f,
                ActionMessage = interactable.ContextualName
            };

            if (__instance.IsLocked)
            {
                __result = delayedActionInstance.Fail(HelpTextDevice.DeviceLocked);
                return;
            }
            if (!__instance.IsAuthorized(interaction.SourceThing))
            {
                __result = delayedActionInstance.Fail(Localization.ParseTooltip("Unable to interact as you do not have the required {SLOT:AccessCard}"));
                return;
            }


            // Activate usage check
            if (!__instance.OnOff)
            {
                __result = delayedActionInstance.Fail(HelpTextDevice.DeviceNotOn);
                return;
            }
            if (!__instance.Powered)
            {
                __result = delayedActionInstance.Fail(HelpTextDevice.DeviceNoPower);
                return;
            }
            if (__instance.Exporting != 0)
            {
                __result = delayedActionInstance.Fail(HelpTextDevice.DeviceLocked);
                return;
            }
            if (!__instance.CurrentSlot.Occupant || !__instance.HasSomething)
            {
                __result = delayedActionInstance.Fail("Nothing selected to dispense");
                return;
            }
            if (!doAction)
            {
                __result = delayedActionInstance.Succeed();
                return;
            }

            if (!GameManager.IsServer)
            {
                __result = delayedActionInstance.Succeed();
                return;
            }

            // Activation

            var amount = (int)__instance.GetLogicValue(LogicType.Setting);
            var stackable = __instance.CurrentSlot.Occupant as Stackable;

            VendomaticPlugin.Instance.Log("Vending machine activation passed checks.  Vending " + amount);

            if (amount == 0 || !stackable || stackable.Quantity <= amount)
            {
                // Not stackable, or we want the entire stack.  move the whole thing to the export slot.
                VendomaticPlugin.Instance.Log("Vending entire stack");
                OnServer.MoveToSlot(__instance.CurrentSlot.Occupant, __instance.ExportSlot);
                OnServer.Interact(__instance.InteractExport, 1);
            }
            else
            {
                VendomaticPlugin.Instance.Log("Vending partial stack");

                // Stackable and splitting.  Create the new stack.
                var newStack = OnServer.Create(stackable.Prefab as DynamicThing, __instance.ExportSlot) as Stackable;
                // Set the custom color
                if (stackable.CustomColor.IsSet)
                    OnServer.SetCustomColor(newStack, stackable.CustomColor.Index);
                // Sync over stack properties such as reagents
                stackable.OnSplitStack(newStack);
                // Modify the amounts in both stacks.
                stackable.NetworkQuantity -= amount;
                newStack.NetworkQuantity = amount;

                // Trigger the export slot
                OnServer.Interact(__instance.InteractExport, 1);
            }
            __result = delayedActionInstance.Succeed();
        }
    }
}