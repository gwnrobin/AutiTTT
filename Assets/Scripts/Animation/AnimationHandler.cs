using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager = null;

    private Animator animator;
    private LocalVelocity localVelocity;

    private int velocityXHash;
    private int velocityYHash;
    private int runningHash;

    private int verticalRotationHash;

    private float x;
    private float y;

    private void Start()
    {
        animator = GetComponent<Animator>();
        localVelocity = GetComponent<LocalVelocity>();

        velocityXHash = Animator.StringToHash("Velocity X");
        velocityYHash = Animator.StringToHash("Velocity Y");
        runningHash = Animator.StringToHash("Running");

        verticalRotationHash = Animator.StringToHash("Horizontal Aim");
    }

    private void Update()
    {
        Vector3 velocity = localVelocity.GetVelocity;

        x = Mathf.Lerp(x, velocity.x, .25f);
        y = Mathf.Lerp(y, velocity.z, .25f);

        animator.SetFloat(velocityXHash, x);
        animator.SetFloat(velocityYHash, y);

        //animator.SetBool(runningHash, localVelocity.GetVelocity.magnitude >= 6f);
        
        animator.SetFloat(verticalRotationHash, playerManager.verticalRotation);
    }
}

