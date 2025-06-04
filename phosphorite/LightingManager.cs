using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using BepInEx;
using System.Reflection;

namespace phosphorite
{
    public class LightingManager : MonoBehaviour
    {
        private List<GameLight> lightList = new List<GameLight>();
        private List<GameObject> lightObjectList = new List<GameObject>();
        public static List<LightDataCustom> lightData = new List<LightDataCustom>();

        public static GameLightingManager instance;
        public static LightSettings lightSettings;

        public static bool saved;
        public static string PluginDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

        void Awake()
        {
            instance = GameObject.FindObjectOfType<GameLightingManager>();
        }

        public static IEnumerator SaveLights()
        {
            lightSettings = new LightSettings();
            lightSettings.ambientColor = Shader.GetGlobalColor("_GT_GameLight_Ambient_Color");
            lightSettings.lights = lightData;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(lightSettings, Formatting.Indented);

            File.WriteAllText(Path.Combine(PluginDirectory, "data.json"), json);

            yield return null;
            saved = true;
        }

        public static void AddLight(Vector3 position, float intensity, Color color)
        {
            GameObject lightObj = new GameObject("GameLight");
            lightObj.transform.position = position;
            //lightObj.transform.rotation = Quaternion.identity;

            Light unityLight = lightObj.AddComponent<Light>();
            unityLight.type = LightType.Point;
            unityLight.intensity = intensity;
            unityLight.color = color;

            GameLight gameLight = lightObj.AddComponent<GameLight>();
            gameLight.light = unityLight;

            if (intensity <= 0)
                gameLight.negativeLight = true;

            int id = instance.AddGameLight(gameLight);
            if (id >= 0)
            {
                Debug.Log($"Added GameLight with ID: {id}");
                lightData.Add(new LightDataCustom(position, intensity, color));
            }
            else
                Debug.LogWarning("Failed to add GameLight.");
        }
    }
}
