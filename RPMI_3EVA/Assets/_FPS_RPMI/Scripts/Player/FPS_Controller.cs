using UnityEngine;
using UnityEngine.InputSystem;

public class FPS_Controller : MonoBehaviour
{
    #region General Variables
    [Header("Movimiento y mirar")]

    [SerializeField] GameObject Cam_Holder; //Ref al objeto que tiene como j¡hijo la cámara (rota por la cámara)
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float maxForce = 1f; //Fuerza máxima de aceleración
    [SerializeField] float sensitivity = 0.1f; //Sensibilidad para el input de look

    [Header("Jump & GroundCheck")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isGrounded;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Player State Bools")]
    [SerializeField] bool isSprinting;
    [SerializeField] bool isCrouching;

    #endregion


    //Variables de referencias privadas
    Rigidbody RB; //Ref al rigidbod del player
    Animator anim; //Ref al animator del player

    //Variables para el input
    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;

    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Lock del cursor del ratón
        Cursor.lockState = CursorLockMode.Locked; //Mueve el cursor al centro 
        Cursor.visible = false; //Oculta el cursor de la vista
    }

    // Update is called once per frame
    void Update()
    {
        //GroundCheck
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        //Dibujar rayo para identificar la orienrtación de la cámara
        Debug.DrawRay(Cam_Holder.transform.position, Cam_Holder.transform.forward * 100f, Color.red);

    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void LateUpdate()
    {
        CameraLook();
    }

    void CameraLook() 
    {
        //Rotación horizontal del cuerpo del personaje
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
        //Rotacion vertical de cámara
        lookRotation += (-lookInput.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation, -90,90);
        Cam_Holder.transform.localEulerAngles = new Vector3(lookRotation, 0f, 0f);
    }

    void Movement() 
    { 
     Vector3 currentvelocity = RB.linearVelocity; //Calcular velocidad del rb constantemente
        Vector3 targetVelocity = new Vector3(moveInput.x,0, moveInput.y);
        targetVelocity *= isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : speed);

        //Convertir dirección local en global
        targetVelocity = transform.TransformDirection(targetVelocity);

        //Calcular el cambio de velocidad
        Vector3 velocityChange = (targetVelocity - currentvelocity);
        velocityChange = new Vector3 (velocityChange.x, 0f, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        //  Aplicar fuerza de movimiento
        RB.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump() 
    { 
     if (isGrounded) RB.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    #region Input Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) Jump();  
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed) 
        {
            isCrouching = !isCrouching;
            anim.SetBool("IsCrouching", isCrouching);
        }
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && !isCrouching) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }
    #endregion
}
