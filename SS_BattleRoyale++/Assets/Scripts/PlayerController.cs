using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class PlayerController : MonoBehaviourPun
{
    [Header("Info")]
    public int id;
    private int curAttackerId;

    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public int curHP;
    public int maxHP;
    public int kills;
    public bool dead;

   
    private bool flashingDamage; 

    [Header("Components")]
    public Rigidbody rig;
    public Player photonPlayer;
    public MeshRenderer mr;
    public PlayerWeapon weapon;


    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        if(!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }

    }

    void Update()
    {
        //if this is not our view or we are dead - return 
        if(!photonView.IsMine || dead) 
            return;
         
        Move();

        //when space is pressed jump
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
        //when mouse button is pressed - shoot
        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
        
    }

    void Move()
    {
        //get the input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //calculate a direction relative to where we're facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        //set that as our velocity
        rig.velocity = dir;
    }

    void TryJump()
    {
        //create a ray facing down to check if we are on the ground
        Ray ray = new Ray(transform.position, Vector3.down);

        //shoot the raycast
        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    //Function to take damage
    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        //if we are dead 
        if (dead)
            return;

        curHP -= damage;
        curAttackerId = attackerId;

        //flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        //update the health bar UI
        GameUI.instance.UpdateHealthBar();

        //die if no health left
        if (curHP <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        //this function can be paused - pausing between the damage and regular color of the player
        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            //This pauses the function and then will continue the function
            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }
    
    [PunRPC]
    void Die()
    {
        curHP = 0;
        dead = true;

        GameManager.instance.alivePlayers--;


        //host will check the win condition
        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        //is this our local player
        if(photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            //set the camera in spectator mode
            GetComponentInChildren<CameraController>().SetAsSpectator();

            //disable the physics and hide the player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }


    [PunRPC]
    public void AddKill()
    {
        kills++;

        //update the UI
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHP = Mathf.Clamp(curHP + amountToHeal, 0, maxHP);

        //update health bar UI
        GameUI.instance.UpdateHealthBar();
    }
}
