using System;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using UnityEngine;

public class Ros2Subscriber : MonoBehaviour {
    // Instanz der ROS Connector-Klasse
    private ROSConnection ros;

    // Topic-Name, der abonniert wird
    public string topicName = "chatter";

    void Start()
    {
        // Verbindung zur ROS-Instanz herstellen
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<StringMsg>(topicName, MessageReceived);
    }

    // Methode, die aufgerufen wird, wenn eine Nachricht empfangen wird
    private void MessageReceived(StringMsg message) {
        // Nachricht in der Unity-Konsole ausgeben
        Debug.Log("Empfangene Nachricht: " + message.data);
    }
}