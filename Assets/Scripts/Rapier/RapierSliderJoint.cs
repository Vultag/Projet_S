using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class RapierSliderJoint : MonoBehaviour
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
    private float axisX;
    [SerializeField]
    private float axisY;

    [SerializeField]
    private bool useLimits;
    [SerializeField]
    private float minLimit;
    [SerializeField]
    private float maxLimit;


    [SerializeField]
    private bool motorEnabled;
    [SerializeField]
    private float targetPosition;
    [SerializeField]
    private float stiffness;
    [SerializeField]
    private float damping;

    //[SerializeField]
    //private float velocityTarget;
    //[SerializeField]
    //private float velocityGain = 1f;

    [SerializeField]
    private float maxForce = 1000f;


    private void Start()
    {

        if (!TryGetComponent<RapierBody>(out var body)) Debug.Log("NO BODY ON JOINT");
        bool connectedToWorld = connectedBody == null;

        if (axisX == 0 && axisY == 0) axisY = 1;

        handle = RapierWorld.joint_add_prismatic(
            RapierWorld.world,
            body.entityHandle,
            connectedToWorld ? 0 : connectedBody.entityHandle,
            connectedToWorld,
            enableCollision,
            anchor.x,
            anchor.y,
            connectedAnchor.x,
            connectedAnchor.y,

            axisX, axisY,

            useLimits,
            minLimit,
            maxLimit,

            motorEnabled,
            targetPosition,
            stiffness,
            damping,
            //velocityTarget,
            //velocityGain,
            maxForce
        );
    }

    //public void remplaceJoint(
    //    float axisX,
    //    float axisY,
    //    float limitMin,
    //    float limitMax)
    //{
    //    handle = RapierWorld.joint_change_prismatic(RapierWorld.world, handle, axisX, axisY, limitMin, limitMax);
    //}


}
#region EDITOR
#if UNITY_EDITOR

[CustomEditor(typeof(RapierSliderJoint))]
public class RapierSliderJointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var useLimits = serializedObject.FindProperty("useLimits");
        var minLimit = serializedObject.FindProperty("minLimit");
        var maxLimit = serializedObject.FindProperty("maxLimit");

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "useLimits",
            "minLimit",
            "maxLimit",
            "motorEnabled",
            "targetPosition",
            "stiffness",
            "damping",
            //"velocityTarget",
            //"velocityGain",
            "maxForce"
        );

        EditorGUILayout.PropertyField(useLimits);

        if (useLimits.boolValue)
        {
            EditorGUILayout.PropertyField(minLimit);
            EditorGUILayout.PropertyField(maxLimit);
        }

        EditorGUILayout.Space();

        var motorEnabled = serializedObject.FindProperty("motorEnabled");
        EditorGUILayout.PropertyField(motorEnabled);

        if (motorEnabled.boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stiffness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damping"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("velocityTarget"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("velocityGain"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxForce"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion