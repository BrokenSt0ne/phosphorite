using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using GorillaLocomotion;
using UnityEngine;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using GorillaNetworking;

//credits
//biotest: helping with patch shit
//graze: helping my dumbass
//brokenstone: me making mod idk

namespace phosphorite
{
    [BepInPlugin("com.brokenstone.gorillatag.phosphorite", "phosphorite", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public class LightDataCustom
        {
            public LightDataCustom(Vector3 pos, float intensity, Color color)
            {
                this.pos = pos;
                this.intensity = intensity;
                this.color = color;
            }

            public Vector3 pos;
            public float intensity;
            public Color color;
        }

        public class LightSettings
        {
            public Color ambientColor;
            public List<LightDataCustom> lights;
        }

        public static GameLightingManager lightingManager;

        public bool onGUIEnabled;

        public List<GameLight> lightList = new List<GameLight>();

        private Vector3 lightPosition = Vector3.zero;
        private float lightIntensity = 1f;
        private Color lightColor = Color.white;

        private string xInput = "0", yInput = "0", zInput = "0";
        private string intensityInput = "1";
        private string colorInput = "#ffffff";

        private string inputAmbientColor = "#ffffff";

        private bool saved;

        private string PluginDirectory => Path.Combine(Paths.BepInExRootPath, "phosphorite");
        private List<LightDataCustom> lightData = new List<LightDataCustom>();
        public LightSettings lightSettings;

        Plugin()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        public void Awake()
        {
            GorillaTagger.OnPlayerSpawned(Initialize);
        }

        IEnumerator SaveLights()
        {
            lightSettings = new LightSettings();
            lightSettings.ambientColor = Shader.GetGlobalColor("_GT_GameLight_Ambient_Color");
            lightSettings.lights = lightData;
            
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(lightSettings, Formatting.Indented);

            Directory.CreateDirectory(PluginDirectory);
            File.WriteAllText(Path.Combine(PluginDirectory, "data.json"), json);

            yield return null;
            saved = true;
        }

        public void Initialize()
        {
            lightingManager = GameObject.Find("Miscellaneous Scripts/GameLightManager").GetComponent<GameLightingManager>();

            lightingManager.SetCustomDynamicLightingEnabled(true);
            lightingManager.SetAmbientLightDynamic(new Color(1f, 1f, 1f));

            //enables experimental auto light maker
            //SceneManager.sceneLoaded += SceneLoaded;
            //SceneManager.sceneUnloaded += delegate { SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Additive); };
        }

        //experimental auto light maker
        /*private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            lightingManager.ClearGameLights();
            foreach (var bakeryLight in FindObjectsOfType<BakeryPointLight>(includeInactive: true))
            {
                if (bakeryLight.GetComponent<Light>() != null)
                {
                    //Debug.Log("Found light " + lightFlag.name);
                    bakeryLight.gameObject.SetActive(true);
                    bakeryLight.GetComponent<Light>().enabled = true;
                    AddDebugLight(bakeryLight.transform.position, bakeryLight.intensity * 2, bakeryLight.color);
                }
            }
        }*/

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
            GUILayout.BeginArea(new Rect(10, 60, 300, 450), "Light Spawner", GUI.skin.window);

            GUILayout.Label("Ambient Color");
            inputAmbientColor = GUILayout.TextField(inputAmbientColor);
            if(GUILayout.Button("Apply Ambient Color"))
            {
                ColorUtility.TryParseHtmlString(inputAmbientColor, out Color amColor);
                lightingManager.SetAmbientLightDynamic(amColor);
            }

            GUILayout.Label("Position (X, Y, Z)");
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
                    AddDebugLight(new Vector3(x, y, z), intensity, color);
                }
                else
                {
                    Debug.LogWarning("Invalid input for position/intensity/color");
                }
            }

            if (GUILayout.Button("Remove Last Light"))
            {
                int lastGameLight = lightList.Count - 1;
                lightingManager.RemoveGameLight(lightList.ToArray()[lastGameLight]);
                lightList.RemoveAt(lastGameLight);
            }

            if (GUILayout.Button("Go To Player Pos"))
            {
                xInput = Camera.main.transform.position.x.ToString();
                yInput = Camera.main.transform.position.y.ToString();
                zInput = Camera.main.transform.position.z.ToString();
            }

            if(GUILayout.Button("Save Lights to JSON"))
            {
                StartCoroutine(SaveLights());
            }

            if(GUILayout.Button("Load Lights from JSON"))
            {
                lightingManager.ClearGameLights();
                if (File.Exists(Path.Combine(PluginDirectory, "data.json")))
                {
                    string jsonText = File.ReadAllText(Path.Combine(PluginDirectory, "data.json"));

                    Debug.Log("loading a V2 json");
                    // V2 json (new thing: AMBIENT COLOR!!!! ik its nothing much but i hate running the function everytime i load a v1 json)
                    LightSettings? lightSettings = JsonConvert.DeserializeObject<LightSettings?>(jsonText);
                    lightingManager.SetAmbientLightDynamic(lightSettings.ambientColor);
                        
                    if (lightSettings.lights != null) 
                        foreach (LightDataCustom gameLight in lightSettings.lights) 
                            AddDebugLight(gameLight.pos, gameLight.intensity, gameLight.color);
                }
            }

            if (GUILayout.Button("Clear All Lights"))
            {
                lightingManager.ClearGameLights();
                lightData.Clear();
            }

            /*if (GUILayout.Button("Fill Lights"))
            {
                foreach(var lightFlag in Resources.FindObjectsOfTypeAll<FlagForBaking>())
                {
                    if(lightFlag.GetComponent<Light>() != null)
                    {
                        Debug.Log("Found light " + lightFlag.name);
                        lightFlag.gameObject.SetActive(true);
                        lightFlag.GetComponent<Light>().enabled = true;
                        AddDebugLight(lightFlag.transform.position, lightFlag.GetComponent<Light>().intensity * 2, lightFlag.GetComponent<Light>().color);
                    }
                }
            }*/

            GUILayout.EndArea();
        }

        //called debug light because i was messing with something else, im not strikergpt zawg
        void AddDebugLight(Vector3 position, float intensity, Color color)
        {
            GameObject lightObj = new GameObject("DebugLight");
            lightObj.transform.position = position;
            lightObj.transform.rotation = Quaternion.identity;

            Light unityLight = lightObj.AddComponent<Light>();
            unityLight.type = LightType.Point;
            unityLight.intensity = intensity;
            unityLight.color = color;

            GameLight gameLight = lightObj.AddComponent<GameLight>();
            gameLight.light = unityLight;

            if (intensity <= 0)
                gameLight.negativeLight = true;
            
            lightList.Add(gameLight);
            int id = lightingManager.AddGameLight(gameLight);
            if (id >= 0)
            {
                Debug.Log($"Added GameLight with ID: {id}");
                lightData.Add(new LightDataCustom(position, intensity, color));
            }
            else
                Debug.LogWarning("Failed to add GameLight (skill issue)");
        }
    }
}