﻿using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	private const string typeName = "LifeSpark";
	private const string gameName = "LifeSparkRoomOne";
	private HostData[] hostList;
	public GameObject playerPrefab;
    public GameObject bossPrefab;


    private int playerCount = 0;
    private bool isRefreshingHostList = false;

	// Use this for initialization
	void Start () {
        //MasterServer.ipAddress = "127.0.0.1";
	}

	private void StartServer () {
		Network.InitializeServer(5, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
	}

	private void RefreshHostList () {
        if (!isRefreshingHostList) {
            isRefreshingHostList = true;
            MasterServer.RequestHostList(typeName);
        }
	}

	private void JoinServer (HostData hostData) {
		Network.Connect (hostData);
	}

	void OnConnectedToServer () {
		Debug.Log ("Server Joined");

		SpawnPlayer ();
        //SpawnBoss();
	}

	void OnMasterServerEvent (MasterServerEvent msEvent) {
		if (msEvent == MasterServerEvent.HostListReceived) {
			hostList = MasterServer.PollHostList();
		}
	}

	void OnServerInitialized () {
		Debug.Log ("Server Initialized");
		SpawnPlayer ();
	}

	private void SpawnPlayer () {
		Network.Instantiate (playerPrefab, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
	}

    public void SpawnBoss() {
        Network.Instantiate(bossPrefab, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
    }

	public void DestroyNetworkObject (GameObject gameObject) {
		Network.Destroy(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI () {
		if (!Network.isClient && !Network.isServer) {
			if (GUI.Button (new Rect (100, 100, 250, 100), "Start Server")) {
				StartServer();
			}

			if (GUI.Button(new Rect(100, 250, 250, 100), "Refresh Hosts"))
				RefreshHostList();
			
			if (hostList != null)
			{
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
                        if (isRefreshingHostList && MasterServer.PollHostList().Length > 0) {
                            isRefreshingHostList = false;
                            hostList = MasterServer.PollHostList();

                            Random.seed = (int)Time.time;
                            int roomIdx = (int)Random.Range(0, hostList.Length - 1);
                            JoinServer(hostList[roomIdx]);
                        }
				}
			}
		}
	}

    [RPC]
    void setPlayerCount(int count) {
        playerCount = count;
    }
}
