using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class RapierWorldManager : MonoBehaviour
{
    //IntPtr rapierWorldPtr;


    private void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
        RapierWorld.Create_world();

    }
    private void FixedUpdate()
    {
        RapierWorld.UpdateTransforms();

    }


    private void OnDisable()
    {
        RapierWorld.Destory_world();
    }


}
