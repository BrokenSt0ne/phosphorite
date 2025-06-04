using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.InputSystem;

//credits
//biotest: helping with patch shit
//graze: helping my dumbass
//brokenstone: me making mod idk

namespace phosphorite
{
    [BepInPlugin("com.brokenstone.gorillatag.phosphorite", "phosphorite", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public bool onGUIEnabled;

        public GameObject __instance;

        public List<GameLight> lightList = new List<GameLight>();

        private Vector3 lightPosition = Vector3.zero;
        private float lightIntensity = 1f;
        private Color lightColor = Color.white;

        private string xInput = "0", yInput = "0", zInput = "0";
        private string intensityInput = "1";
        private string colorInput = "#ffffff";

        private string inputAmbientColor = "#ffffff";

        public Rect mainWindowRect = new Rect(10, 60, 300, 470);
        public Rect lightWindowRect = new Rect(320, 60, 300, 500);

        Vector2 scrollPos;
        
        Plugin()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        public void Awake()
        {
            GorillaTagger.OnPlayerSpawned(Initialize);
        }

        public void Initialize()
        {
            __instance = new GameObject("LightingManager");
            __instance.AddComponent<LightingManager>();
            LightingManager.instance.SetCustomDynamicLightingEnabled(true);
            LightingManager.instance.SetAmbientLightDynamic(Color.white);
        }

        public void Update()
        {
            if (Keyboard.current.vKey.wasPressedThisFrame) onGUIEnabled ^= true;
        }

        

        void OnGUI()
        {
            if (!onGUIEnabled)
            {
                return;
            }
            mainWindowRect = GUILayout.Window(0, mainWindowRect, MainEditorWindow, "Light Spawner", GUI.skin.window);
            lightWindowRect = GUILayout.Window(1, lightWindowRect, LightEditorWindow, "Light Editor", GUI.skin.window);
        }

        void MainEditorWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.BeginVertical();
            GUILayout.Label("Ambient Color");
            inputAmbientColor = GUILayout.TextField(inputAmbientColor);
            if (GUILayout.Button("Apply Ambient Color"))
            {
                ColorUtility.TryParseHtmlString(inputAmbientColor, out Color amColor);
                LightingManager.instance.SetAmbientLightDynamic(amColor);
            }

            GUILayout.Label("Position (XYZ)");
            xInput = GUILayout.TextField(xInput);
            yInput = GUILayout.TextField(yInput);
            zInput = GUILayout.TextField(zInput);

            GUILayout.Label("Intensity");
            intensityInput = GUILayout.TextField(intensityInput);

            GUILayout.Label("Color");
            colorInput = GUILayout.TextField(colorInput);

            if (GUILayout.Button("Add Light"))
            {
                if (float.TryParse(xInput, out float x) &&
                    float.TryParse(yInput, out float y) &&
                    float.TryParse(zInput, out float z) &&
                    float.TryParse(intensityInput, out float intensity) &&
                    ColorUtility.TryParseHtmlString(colorInput, out Color color))
                {
                    LightingManager.AddLight(new Vector3(x, y, z), intensity, color);
                }
                else
                {
                    Debug.LogWarning("Invalid input for position/intensity/color");
                }
            }

            if (GUILayout.Button("Remove Last Light"))
            {
                int lastGameLight = lightList.Count - 1;
                LightingManager.instance.RemoveGameLight(lightList.ToArray()[lastGameLight]);
                lightList.RemoveAt(lastGameLight);
            }

            if (GUILayout.Button("Set Position"))
            {
                xInput = Camera.main.transform.position.x.ToString();
                yInput = Camera.main.transform.position.y.ToString();
                zInput = Camera.main.transform.position.z.ToString();
            }

            GUILayout.Space(16);

            if (GUILayout.Button("Save Lights to JSON"))
            {
                StartCoroutine(LightingManager.SaveLights());
            }

            if (GUILayout.Button("Load Lights from JSON"))
            {
                LightingManager.instance.ClearGameLights();
                if (File.Exists(Path.Combine(LightingManager.PluginDirectory, "data.json")))
                {
                    string jsonText = File.ReadAllText(Path.Combine(LightingManager.PluginDirectory, "data.json"));

                    Debug.Log("loading a V2 json");
                    // V2 json
                    LightingManager.LightSettings? lightSettings = JsonConvert.DeserializeObject<LightingManager.LightSettings?>(jsonText);
                    LightingManager.instance.SetAmbientLightDynamic(lightSettings.ambientColor);

                    if (lightSettings.lights != null)
                        foreach (LightingManager.LightDataCustom gameLight in lightSettings.lights)
                            LightingManager.AddLight(gameLight.pos, gameLight.intensity, gameLight.color);
                }
            }

            if (GUILayout.Button("Clear All Lights"))
            {
                LightingManager.instance.ClearGameLights();
                LightingManager.lightData.Clear();
                LightingManager.instance.SetAmbientLightDynamic(Color.white);
            }

            GUILayout.EndVertical();
        }

        void LightEditorWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            if (LightingManager.instance.gameLights != null)
            {
                for(int i = 0; i < LightingManager.instance.gameLights.Count; i++)
                {
                    GUILayout.Button("Edit " + i);
                }
            }

            GUILayout.EndScrollView();
        }
    }
}