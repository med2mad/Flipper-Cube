using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class player : MonoBehaviour
{
    public Rigidbody rb;
    public Transform theCamera;
    public Vector3 cameraOffset = new Vector3(2f, 4f, -3.5f);
    public Transform stage;
    public Transform cube220;
    public Transform cube93;
    public ParticleSystem psItem;
    public ParticleSystem psDeath;
    public Image fade;
    public AudioSource walkSound;
    public AudioSource explosionSound;
    public AudioSource itemSound;
    public AudioSource buttonSound;
    public AudioSource teleportSound;
    public AudioSource lightonSound;
    public AudioSource lightoffSound;
    public AudioSource shieldSound;
    public AudioSource lights;
    public Text itemText;
    public Text shieldText;
    public int itemTotal = 5;
    private int itemCount = 0;
    private int shieldCount = 0;
    private int puzzleCount = 0;
    
    public Transform flipperCube;
    public Transform flipper1;
    public Transform flipper2;
    public Transform flipper3;
    public Transform flipper4;
    public Renderer shield;
    public Renderer cylinder;
    public Material blockMat;

    Vector3 arround;
    Vector3 flipper;
    Vector3 checkDirection;

    Boolean w = false;
    Boolean d = false;
    Boolean s = false;
    Boolean a = false;

    float value = 0;
    float accumulated = 0;

    Boolean finished = false;
    bool started = false;
    Boolean rolling = false;
    bool cutscene1 = false;
    bool cutscene2 = false;

    void Start()
    {
        Color color = fade.color;
        color.a = 1;
        fade.color = color;
        fade.CrossFadeAlpha(0f, 0f, true);

        if(shield != null) shield.enabled = false;
        if(cylinder != null) cylinder.enabled = false;

        itemText.text = "Items: " + itemCount + " / " + itemTotal;

        foreach (Transform child in GameObject.Find("puzzle").transform)
        {
            child.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        }
    }

    void Update()
    {
        if (finished || !started) { return; }

        if (!rolling && !cutscene1 && !cutscene2)
        {
            w = (Input.GetKey("w") || Input.GetKey("up"));
            s = (Input.GetKey("s") || Input.GetKey("down"));
            d = (Input.GetKey("d") || Input.GetKey("right"));
            a = (Input.GetKey("a") || Input.GetKey("left"));

            if (w) checkDirection = Vector3.forward;
            else if (s) checkDirection = Vector3.back;
            else if (a) checkDirection = Vector3.left;
            else if (d) checkDirection = Vector3.right;

            Ray wallRay = new Ray(transform.position, checkDirection); //check if a wall was in the direction
            if ((w || s || a || d) && !Physics.Raycast(wallRay, out RaycastHit wallHitInfo, 1f, LayerMask.GetMask("block")))
            {
                rolling = true;

                if (w)
                {
                    arround = new Vector3(1f, 0, 0);
                    flipper = flipper1.position;
                }
                else if (s)
                {
                    arround = new Vector3(-1f, 0, 0);
                    flipper = flipper4.position;
                }
                else if (a)
                {
                    arround = new Vector3(0, 0, 1f);
                    flipper = flipper3.position;
                }
                else if (d)
                {
                    arround = new Vector3(0, 0, -1f);
                    flipper = flipper2.position;
                }
            }
        }

        if (rolling)
        {
            value = 250f * Time.deltaTime;
            transform.RotateAround(flipper, arround, value);
            accumulated += value;

            if (accumulated >= 90f)
            {
                transform.RotateAround(flipper, arround, 90f - accumulated);
                accumulated = 0;
                rolling = false;

                Ray floorRay = new Ray(transform.position, Vector3.down); //check if a block was beneath (to put player above)
                if (Physics.Raycast(floorRay, out RaycastHit floorHitInfo, 1f, LayerMask.GetMask("block")))
                {   
                    Vector3 newPosition = floorHitInfo.transform.position + new Vector3(0, 1f, 0);
                    if (!finished) {
                        if(shieldCount > 0) {
                            shieldCount --;
                            shieldText.text = "Shield: " + shieldCount;
                            if(shieldCount == 0) shield.enabled = false;
                        }
                    }

                    if(floorHitInfo.collider.CompareTag("trap")){
                        hit();
                    }
                    else if(floorHitInfo.collider.CompareTag("button")){
                        foreach (Transform child in floorHitInfo.transform)
                        {
                            child.position = new Vector3(child.position.x, -1f, child.position.z);
                        }
                        floorHitInfo.collider.GetComponent<Renderer>().material = GameObject.Find("Cube (403)").GetComponent<Renderer>().material;
                        buttonSound.Play();
                    }
                    else if(floorHitInfo.collider.CompareTag("teleporter")){
                        if(floorHitInfo.collider.gameObject.name=="teleporter1")
                            newPosition = cube220.transform.position + new Vector3(0, 1f, 0);
                        else
                            newPosition = cube93.position + new Vector3(0, 1f, 0);

                        teleportSound.Play();
                    }
                    else if(floorHitInfo.collider.CompareTag("shield")){
                        shieldCount = 10;
                        shieldText.text = "Shield: " + shieldCount;
                        shield.enabled = true;
                        shieldSound.Play();
                    }
                    else if(floorHitInfo.collider.CompareTag("puzzle")){
                        Material mat = floorHitInfo.collider.gameObject.GetComponent<Renderer>().material;
                        if(mat.GetColor("_EmissionColor") == Color.black)
                        {
                            mat.SetColor("_EmissionColor", Color.yellow*16f);
                            puzzleCount ++;
                            if(puzzleCount==9){
                                Collider[] colliders = Physics.OverlapSphere(GameObject.Find("wallc").transform.position, 1f);
                                explosionSound.Play();
                                foreach(Collider hit in colliders)
                                {
                                    if(hit.gameObject.name != "walls"){
                                        hit.gameObject.GetComponent<Rigidbody>().AddExplosionForce(2000f, GameObject.Find("wallc").transform.position, 1f);
                                        hit.enabled = false;
                                    }
                                }
                                foreach (Transform child in GameObject.Find("puzzle").transform)
                                {
                                    child.gameObject.tag = "Untagged";
                                    child.gameObject.GetComponent<Renderer>().material = GameObject.Find("Cube (276)").GetComponent<Renderer>().material;
                                }
                            }
                            lightonSound.Play();
                        }
                        else{
                            foreach (Transform child in GameObject.Find("puzzle").transform)
                            {
                                child.gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
                            }
                            puzzleCount = 0;
                            lightoffSound.Play();
                        }
                    }
                    else if(floorHitInfo.collider.CompareTag("goal") && itemCount==itemTotal){
                        preFinish(1f);

                        foreach (Transform child in stage){
                            Rigidbody newComponent;
                            if(child.gameObject.GetComponent<Rigidbody>() == null)
                                newComponent = child.gameObject.AddComponent<Rigidbody>();
                            else {newComponent = child.gameObject.GetComponent<Rigidbody>();}
                            newComponent.AddExplosionForce(300f, floorHitInfo.collider.transform.position+new Vector3(0, 1f, 0), 10f);
                        }
                        GameObject.Find("shield1").SetActive(false); //if not in "stage"
                        GameObject.Find("shield2").SetActive(false); //if not in "stage"

                        GameObject.Find("goalFace").GetComponent<Renderer>().enabled = false;
                        explosionSound.Play();
                        cylinder.enabled = false;
                        cylinder.gameObject.GetComponent<Light>().enabled = false;
                        gameObject.SetActive(false);
                    }

                    if (!finished){
                        walkSound.Play();
                        transform.position = newPosition;
                        flipperCube.eulerAngles = new Vector3(0, 0, 0);
                    }
                }
                else
                {
                    // shieldCount = 0;
                    // shield.enabled = false;
                    preFinish(0);
                    rb.AddForce(0, -1000f, 0);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (!finished && !cutscene1 && !cutscene2) { 
            float x = transform.position.x + cameraOffset.x;
            float y = cameraOffset.y;
            float z = transform.position.z + cameraOffset.z;
            theCamera.position = new Vector3(x, y, z);

            theCamera.rotation = Quaternion.Euler(50f, -25f, 0);
        }
        else if(cutscene2) {
            theCamera.position = Vector3.MoveTowards(theCamera.position, new Vector3(-9.783961f, 14.90623f, -5.384568f), 0.4f);
            if (theCamera.position == new Vector3(-9.783961f, 14.90623f, -5.384568f)) {Invoke("glow", 1f);}
        }
        
        if(shield != null){
            shield.transform.position = transform.position;
            shield.material.mainTextureOffset = shield.material.mainTextureOffset - new Vector2(0, Time.deltaTime * 0.5f);
        }
        if(cylinder != null){
            cylinder.material.mainTextureOffset = cylinder.material.mainTextureOffset - new Vector2(Time.deltaTime * 0.5f, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("item")){
            other.gameObject.SetActive(false);
            psItem.transform.position = other.transform.position;
            psItem.Play();
            itemSound.Play();
            itemCount++;
            itemText.text = "Items: " + itemCount + " / " + itemTotal;
            if (itemCount == itemTotal){
                theCamera.position = new Vector3(theCamera.position.x, 14.90623f, theCamera.position.z);
                theCamera.rotation = Quaternion.Euler(60.368f, 394.997f, 0);
                cutscene1 = true;
                Invoke("cutsceneF1", 0.3f);
            }
        }
        else if (other.CompareTag("enemy")){
            hit();
        }
    }

    void glow(){
        lights.PlayOneShot(lights.clip); 
        GameObject.Find("goalFace").GetComponent<Renderer>().material = GameObject.Find("player").GetComponent<Renderer>().material;
        cylinder.enabled = true;

        foreach (Transform child in GameObject.Find("stage").transform)
        {
            if(child.GetComponent<Renderer>() != null)
            child.GetComponent<Renderer>().material = blockMat;
        }
        foreach (Transform child in GameObject.Find("puzzle").transform)
        {
            child.GetComponent<Renderer>().material = blockMat;
        }

        Invoke("cutsceneF3", 2f);
    }
    void cutsceneF1(){
        Invoke("cutsceneF2", 0.7f);     
    }
    void cutsceneF2(){
        cutscene2 = true;
    }
    void cutsceneF3(){
        cutscene1 = false;
        cutscene2 = false;
    }

    void hit(){
        if(shieldCount <= 0){
            psDeath.transform.position = transform.position;
            transform.gameObject.GetComponent<Renderer>().enabled = false;
            transform.gameObject.GetComponent<Collider>().enabled = false;
            psDeath.Play();
            explosionSound.Play();
            preFinish(0.5f);
        }
    }
    void preFinish(float seconds){
        finished = true;
        Invoke("finish", seconds);
    }
    void finish(){
        fade.CrossFadeAlpha(1f, 2f, false);
        Invoke("restart", 2f);
    }
    void restart(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}