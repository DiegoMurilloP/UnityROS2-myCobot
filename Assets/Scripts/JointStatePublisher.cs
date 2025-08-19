using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;

public class JointStatePublisher : MonoBehaviour
{
    [SerializeField] string topicName = "/joint_states";
    [SerializeField] GameObject myCobot280;
    [SerializeField] ArticulationBody[] robotJoints;

    private ROSConnection ros;
    private JointStateMsg jointStateMsg;

    void OnValidate()
    {
        if (myCobot280 != null)
        {
            robotJoints = myCobot280.GetComponentsInChildren<ArticulationBody>();
        }
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        jointStateMsg = new JointStateMsg
        {
            name = new string[robotJoints.Length],
            position = new double[robotJoints.Length]
        };

        for (int i = 0; i < robotJoints.Length; i++)
        {
            jointStateMsg.name[i] = robotJoints[i].name;
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < robotJoints.Length; i++)
        {
            if (robotJoints[i] != null && robotJoints[i].jointPosition.dofCount > 0)
            {
                jointStateMsg.position[i] = robotJoints[i].jointPosition[0]; // rad
            }
            else
            {
                jointStateMsg.position[i] = 0.0; // joint sin DOF
            }
        }

        ros.Publish(topicName, jointStateMsg);
    }
}
