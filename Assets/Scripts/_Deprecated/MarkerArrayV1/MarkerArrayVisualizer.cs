using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Visualization;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class MarkerArrayVisualizer : MonoBehaviour
{
   
    ROSConnection ros;
    public string topicName = "/object_markers";

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<MarkerArrayMsg>(topicName, DisplayMarkers);
    }

    void DisplayMarkers(MarkerArrayMsg markerArray)
    {
        foreach (var marker in markerArray.markers)
        {
            GameObject markerObject;

            switch (marker.type) {
                case MarkerMsg.CUBE:
                    markerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case MarkerMsg.SPHERE:
                    markerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                // Weitere Formtypen können hier hinzugefügt werden
                default:
                    continue;
            }
            
            // ToDo Hier manchmal infinite Exceptions, ka iwie abfangen
            markerObject.transform.position = FLU.ConvertFromRUF(marker.pose.position.From<FLU>());
            markerObject.transform.rotation = FLU.ConvertFromRUF(marker.pose.orientation.From<FLU>());
            if (marker.scale.x >= float.MaxValue || 
                marker.scale.y >= float.MaxValue ||
                marker.scale.z >= float.MaxValue) {
                Destroy(markerObject);
                Debug.LogWarning("Marker entfernt! Skalierung war zu groß");
            } else {
                markerObject.transform.localScale = new Vector3((float)marker.scale.x, (float)marker.scale.y, (float)marker.scale.z);
            
                var renderer = markerObject.GetComponent<Renderer>();
                renderer.material.color = new Color(marker.color.r, marker.color.g, marker.color.b, marker.color.a);
                Destroy(markerObject, 1);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
