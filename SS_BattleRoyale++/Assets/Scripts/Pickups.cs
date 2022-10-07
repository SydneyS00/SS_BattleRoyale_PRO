using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum PickupType
{
    Health, 
    Ammo
}
public class Pickups : MonoBehaviourPun
{
    public PickupType type;
    public int value;

    void OnTriggerEnter(Collider other)
    {

        //Debug.Log
        Debug.Log("Pickup triggered.");

        //we want host to look at this 
        if (!PhotonNetwork.IsMasterClient)
            return;

        //check if we hit a player
        if(other.CompareTag("Player"))
        {
            //get the player
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            //give the player the values
            if (type == PickupType.Health)
                player.photonView.RPC("Heal", player.photonPlayer, value);
            else if (type == PickupType.Ammo)
                player.photonView.RPC("GiveAmmo", player.photonPlayer, value);

            //destroy the object - Prof Slease fixed code 
            photonView.RPC("DestroyPickup", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void DestroyPickup()
    {
        Destroy(gameObject);
    }
}
