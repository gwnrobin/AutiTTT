using UnityEngine;

public class LocalVelocity : MonoBehaviour
{
    [SerializeField]
    private Transform target = null;

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
        //GameManager.players[target.gameObject.GetComponent<PlayerManager>().id].transform.hasChanged = false;
        //Debug.Log(GameManager.players[target.gameObject.GetComponent<PlayerManager>().id].transform.hasChanged);
        if (target.transform.hasChanged == true){
            worldVelocity = ((target.transform.position - previousVelocity)) / Time.deltaTime;
            previousVelocity = target.transform.position;

            velocity = target.transform.InverseTransformDirection(worldVelocity);
            target.transform.hasChanged = false;
        } 
    }
}

