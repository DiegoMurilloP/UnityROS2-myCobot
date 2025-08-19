using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections.Generic;

public class SourceDestinationPublisher_280 : MonoBehaviour
{
    [SerializeField] string m_TopicName = "/joint_states";
    [SerializeField] GameObject m_MyCobot280;
    [SerializeField] ArticulationBody[] m_RobotJoints;

    private ROSConnection ros;
    private Queue<JointStateMsg> messageQueue = new Queue<JointStateMsg>();
    private Dictionary<string, ArticulationBody> jointMap = new Dictionary<string, ArticulationBody>();

    private Dictionary<string, string> rosToUnityJointNames = new Dictionary<string, string>()
    {
        { "joint2_to_joint1", "joint2" },
        { "joint3_to_joint2", "joint3" },
        { "joint4_to_joint3", "joint4" },
        { "joint5_to_joint4", "joint5" },
        { "joint6_to_joint5", "joint6" },
        { "joint6output_to_joint6", "joint6_flange" }
    };

    void OnValidate()
    {
        if (m_MyCobot280 != null)
        {
            m_RobotJoints = m_MyCobot280.GetComponentsInChildren<ArticulationBody>();
        }
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<JointStateMsg>(m_TopicName, GetMessageJointState);

        var jointList = new List<ArticulationBody>();
        foreach (var ab in m_RobotJoints)
        {
            if (rosToUnityJointNames.ContainsValue(ab.name))
            {
                jointList.Add(ab);
                jointMap[ab.name] = ab;

                var drive = ab.xDrive;
                drive.stiffness = 1000f;
                drive.damping = 100f;
                drive.forceLimit = 1000f;
                ab.xDrive = drive;

                Debug.Log($"[Unity] Joint encontrado: {ab.name}");
            }
        }
        m_RobotJoints = jointList.ToArray();
    }

    void GetMessageJointState(JointStateMsg msg)
    {
        lock (messageQueue)
        {
            messageQueue.Enqueue(msg);
        }
    }

    void FixedUpdate()
    {
        while (messageQueue.Count > 0)
        {
            JointStateMsg msg;
            lock (messageQueue)
            {
                msg = messageQueue.Dequeue();
            }
            ApplyJointState(msg);
        }
    }

    void ApplyJointState(JointStateMsg jointStateMsg)
    {
        for (int i = 0; i < jointStateMsg.name.Length; i++)
        {
            string rosName = jointStateMsg.name[i];
            if (rosToUnityJointNames.TryGetValue(rosName, out string unityName))
            {
                if (jointMap.TryGetValue(unityName, out var ab))
                {
                    float deg = (float)jointStateMsg.position[i] * Mathf.Rad2Deg;

                    // Actualizar xDrive
                    var drive = ab.xDrive;
                    drive.stiffness = 1000f;
                    drive.damping = 100f;
                    drive.forceLimit = 1000f;
                    drive.target = deg;
                    ab.xDrive = drive;

#if UNITY_2021_1_OR_NEWER
                    // Forzar la actualización inmediata del joint
                    ab.TeleportRoot(ab.transform.position, ab.transform.rotation);
#endif
                }
            }
        }
    }
}
