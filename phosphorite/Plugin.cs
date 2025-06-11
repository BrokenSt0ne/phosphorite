using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        public bool useFreecam;

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
        public Rect individualLightWindowRect;

        // New Light Shit
        Vector2 scrollPos;

        bool haveEditorWindow;
        int lightID;

        GameLight currentLight;
        GameObject lightVisualizer;

        private string newIntensity = "1";
        private string newColor = "#ffffff";
        private string xNInput = "0", yNInput = "0", zNInput = "0";

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
            //__instance.AddComponent<FreecamManager>();
            LightingManager.instance.SetCustomDynamicLightingEnabled(true);
            LightingManager.instance.SetAmbientLightDynamic(Color.white);

            SceneManager.sceneUnloaded += OnHorrorMapUnloaded;
        }

        void OnHorrorMapUnloaded(Scene scene)
        {
            if (scene.name.ToLower().Contains("ghost"))
            {
                LightingManager.instance.SetCustomDynamicLightingEnabled(true);
                LightingManager.instance.SetAmbientLightDynamic(Color.white);
            }
        }

        public void Update()
        {
            if (Keyboard.current.vKey.wasPressedThisFrame) onGUIEnabled ^= true;
            if(onGUIEnabled == false && lightVisualizer != null)
            {
                Destroy(lightVisualizer);
            }
        }

        void OnGUI()
        {
            if (!onGUIEnabled)
            {
                useFreecam = false;
                return;
            }
            useFreecam = true;
            mainWindowRect = GUILayout.Window(0, mainWindowRect, MainEditorWindow, "Light Spawner", GUI.skin.window);
            lightWindowRect = GUILayout.Window(1, lightWindowRect, LightEditorWindow, "Light Editor", GUI.skin.window);
            if (haveEditorWindow)
            {
                individualLightWindowRect = GUILayout.Window(2, new Rect(lightWindowRect.x + lightWindowRect.width + 20, lightWindowRect.y, 300, 300), individualLightWindow, "Individual Light", GUI.skin.window);
            }
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

            if (GUILayout.Button("Set to Player Position"))
            {
                xInput = Camera.main.transform.position.x.ToString();
                yInput = Camera.main.transform.position.y.ToString();
                zInput = Camera.main.transform.position.z.ToString();
            }

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
                Destroy(lightVisualizer);
                haveEditorWindow = false;
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
                    if(GUILayout.Button("Edit Light " + i))
                    {
                        haveEditorWindow = true;
                        lightID = i;
                        currentLight = LightingManager.instance.gameLights[i];
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        void individualLightWindow(int windowID)
        {
            if (lightVisualizer == null)
            {
                lightVisualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lightVisualizer.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
                lightVisualizer.GetComponent<SphereCollider>().enabled = false;
                lightVisualizer.GetComponent<Renderer>().material.color = currentLight.light.color;
            }
            lightVisualizer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            lightVisualizer.transform.position = currentLight.gameObject.transform.position;

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            GUILayout.Label("Light " + lightID);
            GUILayout.Space(2);

            GUILayout.Label("Current intensity: " + currentLight.light.intensity);
            newIntensity = GUILayout.TextField(newIntensity);
            if(GUILayout.Button("Set Intensity"))
            {
                var __newIntensity = float.Parse(newIntensity);
                currentLight.light.intensity = __newIntensity;
            }
            GUILayout.Space(8);

            Color32 color32 = currentLight.light.color;
            GUILayout.Label("Current color: " + $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}");
            newColor = GUILayout.TextField(newColor);
            if(GUILayout.Button("Set Color"))
            {
                ColorUtility.TryParseHtmlString(newColor, out Color color);
                currentLight.light.color = color;
                lightVisualizer.GetComponent<Renderer>().material.color = color;
            }
            GUILayout.Space(8);

            GUILayout.Label("Position (XYZ)");
            xNInput = GUILayout.TextField(xNInput);
            yNInput = GUILayout.TextField(yNInput);
            zNInput = GUILayout.TextField(zNInput);

            if (GUILayout.Button("Set to Player Position"))
            {
                xNInput = Camera.main.transform.position.x.ToString();
                yNInput = Camera.main.transform.position.y.ToString();
                zNInput = Camera.main.transform.position.z.ToString();
            }

            if (GUILayout.Button("Set New Position"))
            {
                float.TryParse(xNInput, out float x);
                float.TryParse(yNInput, out float y);
                float.TryParse(zNInput, out float z);
                currentLight.transform.position = new Vector3(x, y, z);
            }
            GUILayout.Space(8);

            if (GUILayout.Button("Destroy Light"))
            {
                Destroy(lightVisualizer);
                LightingManager.instance.RemoveGameLight(currentLight);
                haveEditorWindow = false;
                if(currentLight.lightId != 0)
                {
                    currentLight = LightingManager.lightList[currentLight.lightId - 1];
                }
            }

            if (GUILayout.Button("Close Window"))
            {
                haveEditorWindow = false;
                Destroy(lightVisualizer);
            }
        }
    }
}