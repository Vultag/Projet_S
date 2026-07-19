using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class RapierHingeJoint : MonoBehaviour
{

    public ulong handle { get; private set; }


    [SerializeField]
    private RapierBody connectedBody;
    [SerializeField]
    private bool enableCollision;

    [SerializeField]
    private float2 anchor;
    [SerializeField]
    private float2 connectedAnchor;

    [SerializeField]
    private bool useLimits;
    [SerializeField]
    private float minAngle;
    [SerializeField]
    private float maxAngle;


    [SerializeField]
    private bool motorEnabled;
    [SerializeField]
    private float targetAngle;
    [SerializeField]
    private float stiffness;
    [SerializeField]
    private float damping;

    [SerializeField]
    private float velocityTarget;
    [SerializeField]
    private float velocityGain = 1f;

    [SerializeField]
    private float maxTorque = 1000f;


    void Start()
    {
        if (!TryGetComponent<RapierBody>(out var body)) Debug.Log("NO BODY ON JOINT");
        bool connectedToWorld = connectedBody == null;

        handle = RapierWorld.joint_add_revolute(
                RapierWorld.world,
                body.entityHandle,
                connectedToWorld?0:connectedBody.entityHandle,
                connectedToWorld,
                enableCollision,
                anchor.x,
                anchor.y,
                connectedAnchor.x,
                connectedAnchor.y,

                useLimits,
                minAngle * Mathf.Deg2Rad,
                maxAngle * Mathf.Deg2Rad,

                motorEnabled,
                targetAngle,
                stiffness,
                damping,
                velocityTarget,
                velocityGain,
                maxTorque
            );

    }
}

#region EDITOR
#if UNITY_EDITOR
[CustomEditor(typeof(RapierHingeJoint))]
public class RapierHingeJointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var useLimits = serializedObject.FindProperty("useLimits");
        var minAngle = serializedObject.FindProperty("minAngle");
        var maxAngle = serializedObject.FindProperty("maxAngle");

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "useLimits",
            "minAngle",
            "maxAngle",
            "motorEnabled",
            "targetAngle",
            "stiffness",
            "damping",
            "velocityTarget",
            "velocityGain",
            "maxTorque"
        );

        EditorGUILayout.PropertyField(useLimits);

        if (useLimits.boolValue)
        {
            EditorGUILayout.PropertyField(minAngle);
            EditorGUILayout.PropertyField(maxAngle);
        }

        EditorGUILayout.Space();

        var motorEnabled = serializedObject.FindProperty("motorEnabled");
        EditorGUILayout.PropertyField(motorEnabled);

        if (motorEnabled.boolValue)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("targetAngle")
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("stiffness")
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("damping")
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("velocityTarget")
            );

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("velocityGain")
            );

        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion