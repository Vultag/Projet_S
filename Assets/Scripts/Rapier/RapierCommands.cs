using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

//struct CreateBodyCommand
//{
//    byte type;
//    float mass;
//    Vector2 pos;
//    float rot;
//    float linDamp;
//    float angDamp;

//    public CreateBodyCommand(byte type, float mass,Vector2 pos, float rot,float linDamp, float angDamp)
//    {
//        this.type = type;
//        this.mass = mass;
//        this.pos = pos;
//        this.rot = rot;
//        this.linDamp = linDamp;
//        this.angDamp = angDamp;
//    }

//    public void Execute(IntPtr world)
//    {
//        RapierWorld.body_create(
//            world, type,mass, pos.x, pos.y, rot,
//            linDamp,
//            angDamp,
//            true
//        );
//    }
//}
struct DestroyBodyCommand
{
    ulong handle;

    public DestroyBodyCommand(ulong handle)
    {
        this.handle = handle;
    }

    public void Execute(IntPtr world)
    {
        RapierWorld.body_destroy(
            world,
            handle
        );
        ////RapierToUnityDatabase.
        ////RapierWorld.unityToRapierEntityMap.Remove(handle);

    }
}
struct CreateShapeCommand
{
    ulong handle;
    /// 0 circle ; 1 box ; 2 polygone;
    byte type;
    /// CIRCLE AND BOX
    float widthOrRadius;
    float height;
    /// COMPOSITE
    Vector2[] vertices;
    uint[] indices;

    public CreateShapeCommand(ulong handle,byte type, float widthOrRadius, float height, CompositeCollider2D composite)
    {
        this.handle = handle;
        this.type = type;

        /// CIRCLE AND BOX

        this.widthOrRadius = widthOrRadius/2f;
        this.height = height/2f;   

        /// COMPOSITE

        var mesh = composite.CreateMesh(false, false);
        Vector3[] meshVertices = mesh.vertices;
        /// could be avoided by sending the vec3 to rust and ignore z there but shouldn't matter
        /// too much as collider building should only happen on startup
        vertices = new Vector2[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i++)
        {
            vertices[i] = new Vector2(meshVertices[i].x, meshVertices[i].y);
        }
        ///could reinterpret with nativearray
        int[] indicesS = mesh.triangles;
        indices = new uint[indicesS.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = (uint)indicesS[i];
        }

    }

    public void Execute(IntPtr world)
    {
        //switch (type)
        //{
        //    case 0:
        //        RapierWorld.body_add_circle_collider(
        //            world,handle,widthOrRadius,
        //            0,
        //            0
        //            );
        //        break;
        //    case 1:
        //        RapierWorld.body_add_box_collider(
        //            world,handle,widthOrRadius,height,
        //            0,
        //            0
        //            );
        //        break;
        //    case 2:
        //        //RapierWorld.body_add_polyline_collider(
        //        //    world,handle, vertices, (UIntPtr)vertices.Length, indices, (UIntPtr)(indices.Length/3)
        //        //);
        //        break;
        //}

    }
}
struct ApplyForceCommand
{
    ulong handle;
    Vector2 linearForce;
    float angularForce;

    public ApplyForceCommand(ulong handle,Vector2 linear, float angular)
    {
        this.handle = handle;
        linearForce = linear;
        angularForce = angular;
    }

    public void Execute(IntPtr world)
    {
        RapierWorld.body_add_force(
            world,
            handle,
            linearForce.x,
            linearForce.y,
            angularForce
        );
    }
}