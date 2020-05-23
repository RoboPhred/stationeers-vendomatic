using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using HarmonyLib;

namespace Vendomatic
{
    [HarmonyPatch(typeof(VendingMachine), "CanLogicWrite")]
    class VendingMachine_CanLogicWrite
    {
        static bool Prefix(
            VendingMachine __instance,
            ref bool __result,
            LogicType logicType
        )
        {
            if (!__instance.IsCompleted && GameManager.GameState == GameState.Running)
            {
                __result = false;
                return false;
            }
            switch (logicType)
            {
                case LogicType.RequestHash:
                case LogicType.ClearMemory:
                case LogicType.Setting:
                    __result = true;
                    return false;
                case LogicType.Color:
                    return __instance.HasColorState;
                case LogicType.Activate:
                    return __instance.HasActivateState;
                case LogicType.Open:
                    return __instance.HasOpenState;
                case LogicType.Mode:
                    return __instance.HasModeState;
                case LogicType.Lock:
                    return __instance.HasLockState;
                case LogicType.On:
                    return __instance.HasOnOffState;
            }

            __result = false;
            return false;
        }
    }
}