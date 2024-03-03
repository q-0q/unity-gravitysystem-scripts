using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.Assertions;

public enum ZoneType
{
    Point,
    Path,
    Linear
}

[RequireComponent(typeof(Collider))]
public class GravityZone : MonoBehaviour
{
    private Collider _collider;
    [SerializeField] public ZoneType zoneType = ZoneType.Point;
    
    [SerializeField] private Vector3 point;
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private Vector3 linearNormalVector;
    [SerializeField] private bool push = false;
    [SerializeField] public int priority = 0;
    
    
    public Vector3 GetVector(Vector3 bodyPosition)
    {
        Vector3 rawVector = Vector3.zero;
        
        if (zoneType == ZoneType.Point)
            rawVector = (point + transform.position) - bodyPosition;

        if (zoneType == ZoneType.Path)
            rawVector = (pathCreator.path.GetClosestPointOnPath(bodyPosition)) - bodyPosition;

        if (zoneType == ZoneType.Linear)
        {
            rawVector = transform.rotation * linearNormalVector.normalized * 8f;
        }

        if (push)
        {
            rawVector *= -1f;
        }
        
        return rawVector * Mathf.Pow(1f / rawVector.magnitude, 4f) * (priority + 1);
    }
    
    void Start()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }
    
}
