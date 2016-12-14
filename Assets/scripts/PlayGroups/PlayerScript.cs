﻿using UnityEngine;
using System.Collections;
using Game;
using UI;
using System.Collections.Generic;

namespace PlayGroup {
    public class PlayerScript: MonoBehaviour {
        public bool isMine = false;
        //Is this controlled by the player or other players

        [HideInInspector]
        public PhysicsMove physicsMove;
        public PlayerSprites playerSprites;
        public PhotonView photonView;


        void Start() {
            GameObject searchPlayerList = GameObject.FindGameObjectWithTag("PlayerList");
            if(searchPlayerList != null) {
                transform.parent = searchPlayerList.transform;
            } else {
                Debug.LogError("Scene is missing PlayerList GameObject!!");

            }
            //add physics move component and set default movespeed
            physicsMove = gameObject.GetComponent<PhysicsMove>();

            //Add player sprite controller component

            if(photonView.isMine) { //This prefab is yours, take control of it
                PlayerManager.control.SetPlayerForControl(this.gameObject);
            }
        }

        //THIS IS ONLY USED FOR LOCAL PLAYER
        public void MovePlayer(Vector2 direction) {
            //At the moment it just checks if the input window is open and if it is false then allow move
            if(!UIManager.control.chatControl.chatInputWindow.activeSelf && isMine) {
                physicsMove.MoveInDirection(direction); //Tile based physics move
                playerSprites.FaceDirection(direction); //Handles the playersprite change on direction change

            }
        }

        public float DistanceTo(Vector3 position) {
            return (transform.position - position).magnitude;
        }
    }
}