using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;
public class PlayerManager : MonoBehaviour
{
    public GameObject spawnZone;
    
    private Vector2 spawnZoneBottomLeft;
    private Vector2 spawnZoneTopRight;

    [SerializeField]
    public InputAction resetSceneAction;

    public Text countdownDisplay;

    bool isStartingRound = false;

    public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    Coroutine countdownRoutine;
    // List<GameObject> players = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        countdownDisplay = GameObject.Find("countdown").GetComponent<Text>();
        countdownDisplay.gameObject.SetActive(false);
        spawnZone = GameObject.FindGameObjectWithTag("spawn_zone");    
        spawnZoneBottomLeft = spawnZone.GetComponent<BoxCollider2D>().bounds.min;
        spawnZoneTopRight = spawnZone.GetComponent<BoxCollider2D>().bounds.max;
        resetSceneAction.performed += onResetScene;
        resetSceneAction.Enable();
    }

    private void OnDisable() {
        resetSceneAction.Disable();
    }

    private void OnEnable() {
        resetSceneAction.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //if all players are dead then start a new round
        bool allPlayersDead = true;

        foreach(GameObject player in players.Values) {
            allPlayersDead = allPlayersDead && player.GetComponent<PlayerController>().diedthisRound;
        }

        if(players.Count > 0 && allPlayersDead && !isStartingRound) {
            isStartingRound = true;
            StartCoroutine("startNewRound");
        }
    }

    void beginCountdown() {

        //already counting down, kill it and restart
        if(countdownDisplay.gameObject.activeInHierarchy) {
            stopCountdown();
        }

        countdownDisplay.gameObject.SetActive(true);
        StartCoroutine("countdownToStart");
    }

    void stopCountdown() {
        if(!countdownDisplay.gameObject.activeInHierarchy) {
            return;
        }
        StopCoroutine("countdownToStart");
    }

    IEnumerator countdownToStart() {
        countdownDisplay.color = Color.red;
        countdownDisplay.text = "3";
        yield return new WaitForSeconds(1);
        countdownDisplay.text = "2";
        yield return new WaitForSeconds(1);
        countdownDisplay.text = "1";
        yield return new WaitForSeconds(1);
        countdownDisplay.color = Color.green;
        countdownDisplay.text = "GO!";
        yield return new WaitForSeconds(3);
        yield return null;
    }
    
    IEnumerator startNewRound() {
        isStartingRound = true;
        Debug.Log("STARTING NEW ROUND");

        foreach(GameObject player in players.Values) {
            PlayerController controller = player.GetComponent<PlayerController>();

            controller.transform.position = getRandomSpawnPoint();
            controller.onRespawn();
            controller.disableInputs();
        }

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("bullet");

        foreach(GameObject bullet in bullets) {
            GameObject.Destroy(bullet);
        }

        countdownDisplay.color = Color.red;
        countdownDisplay.text = "3";
        countdownDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        countdownDisplay.text = "2";
        yield return new WaitForSeconds(1);
        countdownDisplay.text = "1";
        yield return new WaitForSeconds(1);
        countdownDisplay.color = Color.green;
        countdownDisplay.text = "GO!";
        foreach(GameObject player in players.Values) {
            PlayerController controller = player.GetComponent<PlayerController>();

            controller.enableInputs();
        }
        yield return new WaitForSeconds(1);
        countdownDisplay.gameObject.SetActive(false);
        isStartingRound = false;
        yield return null;
    }

    public Vector3 getRandomSpawnPoint() {
        float x = Random.Range(spawnZoneBottomLeft.x, spawnZoneTopRight.x);
        float y = Random.Range(spawnZoneBottomLeft.y, spawnZoneTopRight.y);

        return new Vector3(x, y, 0);
    }

    public void onPlayerJoin(PlayerInput input) {


        if(players.ContainsKey(input.playerIndex)) {
    //player gameobject still gets created via the inputmanager  so be sure to destroy the new player since the existing one is just disabled
            if(players[input.playerIndex] == input.gameObject) {
                Debug.Log("SAME");
            } else {
                Destroy(input.gameObject);

            }
            return;
        }

        float x = Random.Range(spawnZoneBottomLeft.x, spawnZoneTopRight.x);
        float y = Random.Range(spawnZoneBottomLeft.y, spawnZoneTopRight.y);
        GameObject player = input.gameObject;
        player.transform.position = new Vector3(x, y, 0);

        player.GetComponent<PlayerController>().init(input.playerIndex);
        players[input.playerIndex] = player;
        // players[newPlayerId] = player;
        
    }

    public void onResetScene(InputAction.CallbackContext context) {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
