using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] float objectDistance = 3f;
    [SerializeField] float yHeightAngle = -45f;
    [SerializeField] PlayerInput playerInput;

    private DragAndDropObject draggingObject = null;
    private bool isDragging = false;
    private bool isPressed = false;
    private Vector2 inputMovement;
    private Vector2 inputLook;
    private Vector2 inputPosition;
    private Quaternion inputRotation;


    private readonly float screenX = Screen.width / 2;
    private readonly float screenY = Screen.height / 2;

    private void FixedUpdate()
    {
        if (!isDragging && isPressed)
        {
            var ray = Camera.main.ScreenPointToRay(inputPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (GameController.Instance.PresnetMap.TryGetValue(hit.collider.gameObject, out draggingObject))
                {
                    isDragging = true;
                    draggingObject.Rigid.isKinematic = true;
                    draggingObject.Rigid.velocity = Vector3.zero;
                    draggingObject.Rigid.angularVelocity = Vector3.zero;
                    var mousePosition = new Vector3(inputPosition.x, inputPosition.y, objectDistance);
                    var worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    draggingObject.transform.position = worldMousePosition;
                }
            }
        }

        if (isDragging)
        {
            if(draggingObject == null)
            {
                isDragging = false;
                return;
            }

            draggingObject.Rigid.isKinematic = true;
            draggingObject.Rigid.velocity = Vector3.zero;
            draggingObject.Rigid.angularVelocity = Vector3.zero;
            var mousePosition = new Vector3(inputPosition.x, inputPosition.y, objectDistance);
            var worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var unitDiff = UnitPositionByMouse(inputPosition);
            var cameraRot = Camera.main.transform.rotation;
            var angleInRadians = yHeightAngle * Mathf.Deg2Rad;
//            var x = unitDiff.x * Mathf.Cos(angleInRadians) - unitDiff.y * Mathf.Sin(angleInRadians);
            var y = unitDiff.x * Mathf.Sin(angleInRadians) + unitDiff.y * Mathf.Cos(angleInRadians);
            var rotateDiff = cameraRot * new Vector3(unitDiff.x, y, unitDiff.y);
            draggingObject.transform.position = worldMousePosition + rotateDiff;
        }

        if (isDragging && !isPressed)
        {
            draggingObject.Rigid.angularVelocity = Vector3.zero;
            draggingObject.EnablePhysics();
            draggingObject = null;
            isDragging = false;
        }
    }

    private Vector2 UnitPositionByMouse(Vector2 pos)
    {
        var transPos = pos - new Vector2(screenX, screenY);
        transPos.x = Mathf.Sign(transPos.x) * Mathf.Min(Mathf.Abs(transPos.x), screenX);
        transPos.y = Mathf.Sign(transPos.y) * Mathf.Min(Mathf.Abs(transPos.y), screenY);
        return transPos / new Vector2(screenX, screenY);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        isPressed = context.performed;
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        inputLook = context.ReadValue<Vector2>();
    }

    public void OnPosition(InputAction.CallbackContext context)
    {
        inputPosition = context.ReadValue<Vector2>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        inputRotation = context.ReadValue<Quaternion>();
    }

}