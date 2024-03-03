using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    [SerializeField] private bool manualUpdate = false;
    [SerializeField] private float gravityStrength = 100f;
    [SerializeField] private float orientationDamping = 100f;
    private Rigidbody _rb;
    private List<GravityZone> _currentZones;
    private Vector3 _targetVector;
    private Vector3 _interpolatedTargetVector;
    private bool _isStickyAttracted;
    private float _invOrientationDamping;

    private void FixedUpdate()
    {
        if (manualUpdate) { return; }
        
        UpdateTargetVector();
        UpdateOrientation();
        ApplyGravityForce();
    }

    private void Start()
    {
        _targetVector = Vector3.zero;
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _currentZones = new List<GravityZone>();
        _isStickyAttracted = false;
        _invOrientationDamping = 1f / orientationDamping;
        
        UpdateTargetVector();
        _interpolatedTargetVector = _targetVector;
    }
    
    public void UpdateOrientation()
    {
        if (_currentZones.Count == 0) { return; }

        var pos = transform.position;
        
        _interpolatedTargetVector = Vector3.Lerp(_interpolatedTargetVector, _targetVector, Time.deltaTime * _invOrientationDamping);
        
        Quaternion dest = Quaternion.FromToRotation(transform.up,
            -1f * _interpolatedTargetVector.normalized) * transform.rotation;
        transform.rotation = dest;
        
        Debug.DrawRay(transform.position, _interpolatedTargetVector, Color.magenta);

    }

    public void ApplyGravityForce()
    {
        if (_currentZones.Count == 0) { return; }
        
        Vector3 v = _targetVector.normalized * gravityStrength;
        Vector3 positionDelta = v * (Time.deltaTime * 100f);
        
        _rb.AddForce(positionDelta, ForceMode.Force);
    }

    public void UpdateTargetVector()
    {
        if (_currentZones.Count == 0) { return; }

        _targetVector = Vector3.zero;
        foreach (var zone in _currentZones)
        {
            _targetVector += zone.GetVector(transform.position);
        }
        
        Debug.DrawRay(transform.position, _targetVector, Color.green);
    }
    
    
    // OnTriggerEnter() and OnTriggerExit() maintain _currentZones, _currentHighestPriority, _primaryZoneType
    private void OnTriggerEnter(Collider col)
    {
        // Confirm collider is a GravityZone
        var gravityZone = col.gameObject.GetComponent<GravityZone>();
        if (gravityZone == null) { return; }
        
        // Exit sticky mode
        if (_isStickyAttracted)
        {
            _isStickyAttracted = false;
            _currentZones.Clear();
        }
        
        // Update state
        _currentZones.Add(gravityZone);
    }

    private void OnTriggerExit(Collider col)
    {
        
        // Confirm collider is a GravityZone
        var gravityZone = col.gameObject.GetComponent<GravityZone>();
        if (gravityZone == null) { return; }
        
        // Enter sticky mode if removing this zone would leave _currentZones empty
        if (_currentZones.Count == 1)
        {
            _isStickyAttracted = true;
            return;
        } 
        
        // Otherwise, update state
        _currentZones.Remove(gravityZone);
    }
    
}
