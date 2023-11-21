using Unity.Netcode;
using UnityEngine;

public class HelloWorldPlayer : NetworkBehaviour
{
    Vector3 moveDir = new Vector3(0, 0, 0);
    float speed = 10;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            //Move();
        }
    }

    /*public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = NewPos();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }*/

    /*[ServerRpc(RequireOwnership = false)]
    void SubmitPositionRequestServerRpc()
    {
        Position.Value = NewPos();
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
    }*/

    void Update()
    {
        if (!IsOwner) return;

        moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;


        Vector3 pos = moveDir * speed * Time.deltaTime;

        GetComponent<Rigidbody>().AddForce(pos * 20);
        //transform.Translate(pos);

        //MoveServerRpc(pos);
    }

    [ServerRpc]
    void MoveServerRpc(Vector3 pos)
    {
        GetComponent<Rigidbody>().AddForce(pos * 20);
    }

    private void OnCollisionEnter(Collision collision)
    {
           
    }


    /*private Vector3 NewPos()
    {
        if (IsLocalPlayer)
        {       // input handling for local player only
            int oldMoveX = moveX;
            int oldMoveY = moveY;

            moveX = 0;
            moveY = 0;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                moveX -= 1;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                moveX += 1;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                moveY += 1;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                moveY -= 1;
            }

            move = new Vector3(moveX, 0f, moveY);


            newPos = transform.position + move * Time.deltaTime * speed;

            return newPos;
        }
        else
        {
            return Vector3.zero;
        }
    }*/

    /*[ServerRpc]
    void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetRandomPositionOnPlane();
    }*/

    /*private void Awake()
    {
        transform.position = new Vector3(0, 1, 0);
    }*/
}