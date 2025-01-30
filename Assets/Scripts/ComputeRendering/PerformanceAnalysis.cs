using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace ComputeRendering {
    [Serializable]
    public class PerformanceData {
        public string Timestamp;
        public List<float> TickTimes;
    }
    public class PerformanceAnalysis {
        private LinkedList<float> tickTimes = new LinkedList<float>();
        private float lastTickTime;
        private string logFilePath = "";
        private string fileDateFormat = "yyyy_MM_dd_HH_mm_ss_fff";

        public PerformanceAnalysis(string name = null) {
            if(string.IsNullOrEmpty(name)) {
                name = Path.ChangeExtension($"PerformanceAnalysis_{DateTime.Now.ToString(fileDateFormat)}", ".json");
            }
            else {
                name = Path.ChangeExtension($"PerformanceAnalysis_{DateTime.Now.ToString(fileDateFormat)}_{name}", ".json");
            }
            this.logFilePath = name;
            
            lastTickTime = Time.realtimeSinceStartup;
        }

        public void Tick() {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - this.lastTickTime;
            this.lastTickTime = currentTime;

            this.tickTimes.AddFirst(deltaTime);
        }

        public void SaveToFile() {
            try {
                var data = new PerformanceData {
                    Timestamp = DateTime.Now.ToString("o"),
                    TickTimes = new List<float>(tickTimes)
                };

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(logFilePath, json);

                Debug.Log($"[PerformanceAnalysis] Data saved to {logFilePath}");
            } catch (Exception ex) {
                Debug.LogError($"Error saving performance data: {ex.Message}");
            }
        }
    }
}
