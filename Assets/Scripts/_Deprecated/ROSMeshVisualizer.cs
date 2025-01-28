using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Visualization;

public class ROSMeshVisualizer : MonoBehaviour {
    public string rosTopicName = "/object_markers";

    private Material meshMaterial;
    private ROSConnection rosConnection;
    private GameObject meshObject;
    private Mesh unityMesh;

    void Start() {
        // ROS Verbindung initialisieren
        rosConnection = ROSConnection.GetOrCreateInstance();
        if (rosConnection == null) {
            Debug.LogError("ROSConnection konnte nicht initialisiert werden.");
            return;
        }

        rosConnection.Subscribe<MarkerArrayMsg>(rosTopicName, OnMeshReceived);

        // Material mit Standard-Shader erstellen
        meshMaterial = new Material(Shader.Find("Unlit/VertexColor"));
        if (meshMaterial == null) {
            Debug.LogError("Standard-Shader konnte nicht gefunden werden.");
            return;
        }

        // GameObject für das Mesh erstellen
        meshObject = new GameObject("VisualizedMesh");
        meshObject.AddComponent<MeshFilter>();
        var renderer = meshObject.AddComponent<MeshRenderer>();
        renderer.material = meshMaterial;

        // Unity-Mesh initialisieren
        unityMesh = new Mesh {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Unterstützt große Indizes
        };
    }

    void OnMeshReceived(MarkerArrayMsg markerArrayMsg) {
        if (markerArrayMsg == null || markerArrayMsg.markers.Length == 0) {
            Debug.LogWarning("Empfangene MarkerArrayMsg ist leer.");
            return;
        }

        foreach (var marker in markerArrayMsg.markers) {
            if (marker == null || marker.type != MarkerMsg.TRIANGLE_LIST || marker.points == null || marker.points.Length == 0) {
                Debug.LogWarning("Marker ist ungültig oder enthält keine Punkte.");
                continue;
            }

            // Punkte und Indizes für das Mesh sammeln
            Vector3[] vertices = new Vector3[marker.points.Length];
            List<int> triangles = new List<int>();
            Color[] vertexColors = new Color[marker.points.Length]; // Farben für jeden Punkt

            for (int i = 0; i < marker.points.Length; i++) {
                // Transformieren von ROS zu Unity-Koordinaten (z-Achse invertieren)
                vertices[i] = new Vector3(
                    (float)marker.points[i].x,
                    (float)marker.points[i].z, // ROS y wird zu Unity z
                    (float)marker.points[i].y * -1f // ROS z wird invertiert zu Unity y
                );

                // Falls Marker eine Farbe haben, anwenden
                if (marker.colors != null && marker.colors.Length > i) {
                    // Die Farbe des Punktes aus der ROS-Nachricht nehmen
                    var rosColor = marker.colors[i];
                    vertexColors[i] = new Color((float)rosColor.r, (float)rosColor.g, (float)rosColor.b, (float)rosColor.a);
                    Debug.Log("Farben vorhanden " + vertexColors[i].ToString());
                } else {
                    Debug.Log("Default Farben werden gesetzt");
                    // Standardfarbe falls keine Farbe vorhanden ist
                    vertexColors[i] = Color.cyan;
                }
            }

            // Dreiecks-Indizes erstellen (jeder Punktblock von drei Punkten ist ein Dreieck)
            for (int i = 0; i < marker.points.Length; i += 3) {
                if (i + 2 < marker.points.Length) {
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + 2);
                }
            }

            // Mesh-Daten aktualisieren
            unityMesh.Clear();
            unityMesh.vertices = vertices;
            unityMesh.triangles = triangles.ToArray();
            unityMesh.colors = vertexColors; // Farben auf das Mesh anwenden
            unityMesh.RecalculateNormals(); // Normale berechnen für richtige Beleuchtung
            unityMesh.RecalculateBounds(); // Mesh-Grenzen anpassen

            // Mesh auf GameObject anwenden
            var meshFilter = meshObject.GetComponent<MeshFilter>();
            if (meshFilter != null) {
                meshFilter.mesh = unityMesh;
            } else {
                Debug.LogError("MeshFilter konnte nicht gefunden werden.");
            }

            // Nur das erste TRIANGLE_LIST verarbeiten
            break;
        }
    }
}
