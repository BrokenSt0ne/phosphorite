using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaNetworking;
using HarmonyLib;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static GameLightingManager;

//code ENTIRELY SKIDDED from BIOLOGIAL TESTICALS THE FIFTH

namespace phosphorite.Patches;

[HarmonyPatch(typeof(GameLightingManager))]
public class GameLightingManagerPatches
{
    public static int MaxLightCount = 512;

    private static Task sortingTask;
    private static bool sortComplete = false;
    private static object sortLock = new();
    public static List<GameLight> sortedLights = new();

    private static float LightViewRadius = 25f;

    [HarmonyPatch(nameof(GameLightingManager.InitData)), HarmonyPrefix]
    public static bool InitData(GameLightingManager __instance)
    {
        instance = __instance;
        __instance.gameLights = new List<GameLight>(512);
        __instance.lightDataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 512, UnsafeUtility.SizeOf<LightData>());
        __instance.lightData = new LightData[512];
        __instance.ClearGameLights();
        __instance.SetDesaturateAndTintEnabled(enable: false, Color.black);
        __instance.SetAmbientLightDynamic(Color.black);
        __instance.SetCustomDynamicLightingEnabled(enable: false);
        return false;
    }
    
    [HarmonyPatch(nameof(GameLightingManager.CompareDistFromCamera)), HarmonyPrefix]
    public static bool CompareDistFromCamera(GameLightingManager __instance, GameLight a, GameLight b, ref int __result)
    {
        if (a?.light == null)
        {
            __result = b?.light == null ? 0 : 1;
            return false;
        }

        if (b?.light == null)
        {
            __result = -1;
            return false;
        }

        float distA = (__instance.cameraPosForSort - a.light.transform.position).sqrMagnitude;
        float distB = (__instance.cameraPosForSort - b.light.transform.position).sqrMagnitude;

        __result = distA.CompareTo(distB);
        return false;
    }

    [HarmonyPatch(nameof(GameLightingManager.RefreshLightData))]
    public static bool RefreshLightData(GameLightingManager __instance)
    {
        if (__instance.lightData == null || !__instance.customVertexLightingEnabled)
        {
            return false;
        }

        if (__instance.immediateSort)
        {
            __instance.immediateSort = false;
            __instance.skipNextSlice = true;
            __instance.SortLights();
        }

        for (int i = 0; i < 512; i++)
        {
            if (i < __instance.gameLights.Count)
            {
                __instance.GetFromLight(i, i);
            }
            else
            {
                __instance.ResetLight(i);
            }
        }

        __instance.lightDataBuffer.SetData(__instance.lightData);
        Shader.SetGlobalBuffer("_GT_GameLight_Lights", __instance.lightDataBuffer);

        return false;
    }
}