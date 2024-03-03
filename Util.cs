using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static Vector3 ThrowOutLocalY(Vector3 vector3, Transform transform)
    {
        Vector3 v = vector3;
        v = transform.InverseTransformDirection(v);
        v = transform.TransformDirection(v.x, 0f, v.z);
        return v;
    }
}
