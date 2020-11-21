using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector2 movementDirection = new Vector2(0,0);
    public Vector2 lookDirection = new Vector2(1, 0);

    public float speed = 8;

    public Rigidbody2D body;

    public GameObject bulletPrefab;
    public Transform bulletOrigin;

    private Color selfColor;
    
    public int maxActiveBullets = 5;
    public int currentActiveBullets = 0;

    public int id = 0;
    public bool diedthisRound = false;

    public Transform turretTransform;

    [Range(0.1f, 3f)]
    [Tooltip("how many seconds it takes for player to turn 360 degrees")]
    public float secondsToTurn = 0.5f;

    [Range(0.1f, 4f)]
    [Tooltip("how many seconds it takes for player TURRET to turn 360 degrees")]
    public float secondsToAim = 1f;

    public float acceleration = 20f;

    // public MeshRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        selfColor = generateNewColor();
        updateRendererColour();

        Physics2D.queriesStartInColliders = false;
    }

    public void init(int _id) {
        id = _id;
    }

    public void onRespawn() {
        diedthisRound = false;
        gameObject.SetActive(true);
        currentActiveBullets = 0;
    }

    public void die() {
        diedthisRound = true;
        gameObject.SetActive(false);
    }

    private void updateRendererColour() {
        Renderer theRenderer = GetComponent<MeshRenderer>();
        theRenderer.material.SetColor("_Color", selfColor);
    }

    public void disableInputs() {
        GetComponent<PlayerInput>().SwitchCurrentActionMap("disabled");
    }

    public void enableInputs() {
        GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
    }

    private Color generateNewColor() {
        float r = Random.Range(0.0f, 1f);
        float g = Random.Range(0.0f, 1f);
        float b = Random.Range(0.0f, 1f);
        return new Color(r, g, b, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if(movementDirection.sqrMagnitude > 0.2*0.2) {
        float rotSpeed = 360 / secondsToTurn;
            float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
            float rotDelta = rotSpeed * Time.deltaTime;

            Quaternion prevTurretRotation = turretTransform.rotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), rotDelta);
            turretTransform.rotation = prevTurretRotation;
        }

        {
            float rotSpeed = 360 / secondsToAim;
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            float rotDelta = rotSpeed * Time.deltaTime;
            turretTransform.rotation = Quaternion.RotateTowards(turretTransform.rotation, Quaternion.Euler(0, 0, angle), rotDelta);
        }

        Vector2 velocity = movementDirection * speed;
        body.velocity = velocity;
        
        // Vector2 targetVelocity = movementDirection * speed;
        // if(movementDirection.sqrMagnitude < 0.1 * 0.1) {
        //     targetVelocity = new Vector2(0, 0);
        // }

        // Vector2 accelerationVector = (targetVelocity - body.velocity).normalized*acceleration;

        // body.velocity += accelerationVector * Time.deltaTime;

        // if((body.velocity - targetVelocity).sqrMagnitude < acceleration * Time.fixedDeltaTime * 2) {
        //     body.velocity = targetVelocity;
        // }
    }

    
    public void onOwnBulletDestroyed() {
        if(currentActiveBullets > 0)
            currentActiveBullets--;
    }


    public void OnFire(InputAction.CallbackContext context) {
        
        //raycast from player origin to just beyond spawn point of bullet
        float raycastDistance = ((Vector2)(bulletOrigin.position - transform.position)).magnitude * 1.05f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, raycastDistance, LayerMask.GetMask("wall"));
        if(hit.collider) {
            return;    
        }

        if(context.performed && currentActiveBullets < maxActiveBullets) {
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletOrigin.position, turretTransform.rotation);
            bullet.GetComponent<BulletController>().init(this);
            // bullet.GetComponent<Renderer>().material.SetColor("_Color", selfColor);
            currentActiveBullets++;
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        Vector2 inputAmount = context.ReadValue<Vector2>();
        movementDirection = inputAmount;

        // Debug.Log(movementDirection);

        // if(Mathf.Abs(movementDirection.x) > 0.25) {
        //     movementDirection.x = movementDirection.x < 0 ? -1 : 1;
        // } else {
        //     movementDirection.x = 0f;
        // }

        // if(Mathf.Abs(movementDirection.y) > 0.25) {
        //     movementDirection.y = movementDirection.y < 0 ? -1 : 1;
        // } else {
        //     movementDirection.y = 0f;
        // }
    }

    public void OnAim(InputAction.CallbackContext context) {
        Vector2 inputAmount = context.ReadValue<Vector2>();

        if(inputAmount.sqrMagnitude > 0.5 * 0.5) {
        lookDirection = inputAmount;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag.ToLower() == "bullet") {
            die();
        }
    }

    public void OnChangeColor(InputAction.CallbackContext context) {
        if(context.performed) {
            selfColor = generateNewColor();
            updateRendererColour();
        }
    }
}
