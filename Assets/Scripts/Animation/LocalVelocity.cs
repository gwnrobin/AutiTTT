using UnityEngine;

public class LocalVelocity : MonoBehaviour
{
    private Vector3 worldVelocity;
    private Vector3 previousVelocity;

    private Vector3 velocity;

    public Vector3 GetVelocity
    {
        get { return velocity; }
    }

    private void Update()
    {
        CalculateVelocity();
    }

    private void CalculateVelocity()
    {
        worldVelocity = ((transform.position - previousVelocity)) / Time.deltaTime;
        previousVelocity = transform.position;

        velocity = transform.InverseTransformDirection(worldVelocity);
    }
}

