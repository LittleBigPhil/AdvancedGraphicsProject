using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    public float Speed = .2f;
    public float RunSpeedMultiplier = 2f;
    public float Gravity = 10f;
    public float MouseSensitivityX = 3f;
    public float MouseSensitivityY = 3f;


    public bool RemoteDesktop = true;
    public bool IsController = false;
    public bool MouseLocked = true;

    public int Collected = 0;
    public int Threshold = 2;
    public int ActiveScene = 0;
    public List<string> Scenes;

    Transform CameraObject;
    private void Awake()
    {
        CameraObject = transform.GetChild(0);
    }
    private void FixedUpdate()
    {
        var controller = GetComponent<CharacterController>();
        var v = Input.GetAxis("Vertical");
        var h = Input.GetAxis("Horizontal");
        var walking = Speed * (transform.forward * v + transform.right * h).normalized;
        var movement = walking * (Input.GetKey(KeyCode.LeftShift) ? RunSpeedMultiplier : 1f);
        controller.Move(Time.fixedDeltaTime * (movement + Vector3.down * Gravity));


    }

    private Vector2 LastAxisMouse = Vector2.zero;
    private Vector2 GetMouseInput()
    {
        var toReturn = Vector2.zero;
        if (RemoteDesktop || !MouseLocked)
        {

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Vector3 axis = new Vector2(-(LastAxisMouse.x - Input.mousePosition.x) * 0.1f, -(LastAxisMouse.y - Input.mousePosition.y) * 0.1f);
            LastAxisMouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (!Input.GetButton("Fire2"))
            {
                toReturn.x = axis.x;
                toReturn.y = axis.y;
            }
        }
        else if (IsController)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            toReturn.x = Input.GetAxis("X");
            if (toReturn.x < .7f && toReturn.x > -.7f)
            {
                toReturn.x = 0f;
            }
            
            toReturn.y = Input.GetAxis("Y");
            if (toReturn.y < .7f && toReturn.y > -.7f)
            {
                toReturn.y = 0f;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false; //The documentation lies to you, this is needed, but only for built projects.
            toReturn.x = Input.GetAxisRaw("X");
            toReturn.y = Input.GetAxisRaw("Y");
        }

        return toReturn;
    }

    private float VerticalAngleTemp = 0f;
    private void Update()
    {
        RotateCamera();
        CollectObject();
        CycleScene();
    }
    private void RotateCamera()
    {
        var mouseInput = GetMouseInput();
        var xAmount = mouseInput.x * MouseSensitivityX;// * Time.deltaTime;
        var rotation = Quaternion.AngleAxis(xAmount, transform.up);

        //var velocityWorldPre = transform.TransformVector(PlayersPhysics.Velocity);
        transform.rotation = rotation * transform.rotation;
        //PlayersPhysics.Velocity = transform.InverseTransformVector(velocityWorldPre);

        
        var yAmount = -mouseInput.y * MouseSensitivityY;// * Time.deltaTime;
        var oldAngleTemp = VerticalAngleTemp;
        VerticalAngleTemp = Mathf.Clamp(VerticalAngleTemp + yAmount, -90, 90);
        yAmount = VerticalAngleTemp - oldAngleTemp;
        CameraObject.Rotate(Vector3.right, yAmount);
    }
    private void CollectObject()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit raycastHitInfo;
            var raycastHit = Physics.Raycast(transform.position, CameraObject.forward, out raycastHitInfo);
            if (raycastHit && raycastHitInfo.collider.gameObject.layer == 9)
            {
                Collected++;
                GameObject.Destroy(raycastHitInfo.collider.gameObject);
            }
        }
    }
    private void CycleScene()
    {
        if (Collected == Threshold)
        {
            Collected = 0;
            SceneManager.UnloadSceneAsync(Scenes[ActiveScene]);
            ActiveScene++;
            ActiveScene = ActiveScene % Scenes.Count;
            SceneManager.LoadSceneAsync(Scenes[ActiveScene], LoadSceneMode.Additive);
        }
    }
}
