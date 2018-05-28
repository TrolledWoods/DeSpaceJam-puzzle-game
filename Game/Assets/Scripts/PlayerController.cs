using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour {

    public enum MovementState
    {
        Walking, Running, Idle, JumpingUp, Falling
    }

    public enum Direction
    {
        Left, Right
    }

    public Direction moving_towards = Direction.Right; // If the direction is Right, you're in the light dimension
    public MovementState current_state = MovementState.Idle;

    public GameObject interactable_selector;
    public float max_interaction_dist = 1;
    public float speed = 3f;
    public float air_acceleration = 0.01f;
    public float ground_acceleration = 0.01f;
    public float jump_strength = 10f;
    public bool is_grounded = false;

    float distToGround;

    public Rigidbody2D rigid_body;
    public Interactable interested_in;

    public Camera life_camera;
    public Camera death_camera;

    // Use this for initialization
    void Start () {
        rigid_body = GetComponent<Rigidbody2D>();

        distToGround = GetComponent<Collider2D>().bounds.extents.y;
        interactable_selector.SetActive(false);
    }
	
	// Update is called once per frame
	void FixedUpdate() {
        // Find the closest interactable
        Interactable p_interested_in = interested_in;
        interested_in = FindClosestInteractable(max_interaction_dist);

        if (interested_in != p_interested_in)
        {
            if (interested_in != null)
            {
                interactable_selector.SetActive(true);
                interactable_selector.transform.position = interested_in.transform.position + Vector3.up * 0.5f;
            }
            else
            {
                interactable_selector.SetActive(false);
            }
        }

        // Allow for level restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            LevelLoader.instance.LoadLevel(LevelLoader.instance.current_level);
            return;
        }else if(Input.GetKeyDown(KeyCode.E) && interested_in != null)
        {
            interested_in.Interact(this);
        }

        // See if the player is grounded
        is_grounded = IsGrounded();

        DoVerticalMovement();
        DoHorizontalMovement();
    }

    Interactable FindClosestInteractable(float max_dist)
    {
        if (Interactable.interactables == null)
            return null;

        float record = max_dist;
        Interactable interested = null;

        for(int i = 0; i < Interactable.interactables.Count; i++)
        {
            Interactable obj = Interactable.interactables[i];
            float sqr_dist = (obj.transform.position - transform.position).sqrMagnitude;

            if(sqr_dist <= max_dist)
            {
                interested = obj;
            }
        }

        return interested;
    }

    bool IsGrounded()
    {
        return Physics2D.Raycast(
            new Vector2(transform.position.x, transform.position.y) - Vector2.up * (distToGround + 0.1f),
            -Vector2.up, 0.1f);
    }

    void DoVerticalMovement()
    {
        if (is_grounded && Input.GetAxis("Vertical") > 0.1f && 
            (current_state == MovementState.Idle || current_state == MovementState.Walking))
        {
            rigid_body.AddForce(Vector2.up * jump_strength, ForceMode2D.Impulse);
            current_state = MovementState.JumpingUp;
        }

        if(current_state == MovementState.JumpingUp && rigid_body.velocity.y < 0.01f)
        {
            current_state = MovementState.Falling;
        }else if(current_state == MovementState.Falling && rigid_body.velocity.y >= -0.01f)
        {
            current_state = MovementState.Idle;
        }
    }

    void DoHorizontalMovement()
    {
        float desired_speed = 0;
        float acceleration = is_grounded ? ground_acceleration : air_acceleration;

        if(current_state == MovementState.Walking && Mathf.Abs(rigid_body.velocity.x) < 0.1f)
        {
            current_state = MovementState.Idle;
        }

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f)
        {
            desired_speed = (moving_towards == Direction.Left ? -1 : 1) * Input.GetAxis("Horizontal") * speed;

            if (current_state == MovementState.Idle) {
                current_state = MovementState.Walking;
            }
        }

        if ((desired_speed > 0 && moving_towards == Direction.Left) ||
           (desired_speed < 0 && moving_towards == Direction.Right))
        {
            desired_speed = 0;
        }

        float error = (desired_speed - rigid_body.velocity.x) * acceleration;
        rigid_body.AddForce(Vector2.right * error * Time.deltaTime, ForceMode2D.Impulse);
    }

    public void SetDirection(Direction d)
    {
        transform.localScale = d == Direction.Right ? Vector3.one : new Vector3(-1, 1, 1);
        moving_towards = d;

        if(d == Direction.Right)
        {
            life_camera.gameObject.SetActive(true);
            death_camera.gameObject.SetActive(false);
        }else
        {
            life_camera.gameObject.SetActive(false);
            death_camera.gameObject.SetActive(true);
        }
    }
}
