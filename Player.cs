using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public float speed = 20f;
    public float jumpPower = 5f;
    public float drag = 10f;

    [SerializeField] private float collisionBubbleSize = 4f;
    [SerializeField] private Transform cameraFollow;
    
    private Vector3 _viewDirection;
    private Vector3 _inputDirection;
    private Vector2 _rawInputDirection;
    private Camera _camera;
    private Rigidbody _rb;
    private GravityBody _body;
    private RaycastHit _hit;
    private LayerMask _playerMask;
    private Vector3 _castOrigin;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        _camera = Camera.main;
        _rb = GetComponent<Rigidbody>();
        _body = GetComponent<GravityBody>();
        _playerMask = LayerMask.NameToLayer("Player");
        _castOrigin = new Vector3(0, -0.4f, 0);
        cameraFollow.transform.position = transform.position;
        cameraFollow.transform.rotation = transform.rotation;
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        _body.UpdateTargetVector();
        UpdateViewDir();
        PerformMove();
        PerformDrag();
        _body.UpdateOrientation();
        _body.ApplyGravityForce();
    }

    private void LateUpdate()
    {
        UpdateCameraFollow();
    }

    void UpdateViewDir()
    {
        _viewDirection = Util.ThrowOutLocalY(transform.position - _camera.transform.position, transform);
    }

    void OnMove(InputValue move)
    {
        _rawInputDirection = move.Get<Vector2>();
    }

    void OnJump(InputValue jump)
    {
        _rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    void PerformMove()
    {
        // Calculate position delta
        float inputHorizontal = _rawInputDirection.x;
        float inputVertical = _rawInputDirection.y;
        Vector3 direction = ((inputHorizontal * _camera.transform.right).normalized + (inputVertical * _viewDirection).normalized).normalized;
        Vector3 positionDelta = direction * (speed * Time.deltaTime * 0.1f);

        // Adjust movement magnitude to not clip into walls
        if (Physics.Raycast(_castOrigin + transform.position, positionDelta, out _hit,  collisionBubbleSize, _playerMask, QueryTriggerInteraction.Ignore))
        {
            positionDelta = positionDelta - Vector3.Project(positionDelta, _hit.normal);
        }
        _rb.MovePosition(transform.position + (positionDelta));
    }

    void PerformDrag()
    {
        Vector3 v = transform.InverseTransformDirection(_rb.velocity);
        v.x *= (1f / drag) * Time.deltaTime;
        v.z *= (1f / drag) * Time.deltaTime;
        _rb.velocity = transform.TransformDirection(v);
        
        Vector3 vA = transform.InverseTransformDirection(_rb.angularVelocity);
        vA.y = (1f / 1000f) * Time.deltaTime;
        _rb.angularVelocity = transform.TransformDirection(vA);
        
    }

    void UpdateCameraFollow()
    {
        cameraFollow.position = transform.position;
        //cameraFollow.rotation = Quaternion.FromToRotation()
        cameraFollow.rotation = Quaternion.SlerpUnclamped(cameraFollow.rotation, transform.rotation, Time.deltaTime * 7f);
    }
}
