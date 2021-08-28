using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //Spawn
    public static readonly float X_SPAWN_LEFT = -5.5f;
    public static readonly float X_SPAWN_RIGHT = 5.5f;

    [SerializeField]
    private Texture2D m_txtIdle;

    [SerializeField]
    private Texture2D m_txtDragging;

    private CanvasController m_canvasCtrl;

    //Audio Clip
    public AudioClip MUSIC_FX_EXPLOSION;
    public AudioClip MUSIC_FX_FALL;
    public AudioClip MUSIC_FX_WRONG_ZONE;
    public AudioClip MUSIC_FX_RIGHT_ZONE;
    public AudioClip MUSIC_FX_EXPLOSION_MEDIUM;
    public AudioClip MUSIC_FX_TIC_TAC;
    //Définition des zones
    private readonly float m_xStartBlueZone = -1.89f;
    private readonly float m_xEndBlueZone = 1.875f;
    private readonly float m_yStartBlueZone = -4.446f;
    private readonly float m_yEndBlueZone = -1.061f;
    private readonly float m_xStartRedZone = -1.888f;
    private readonly float m_xEndRedZone = 1.877f;
    private readonly float m_yStartRedZone = 1.038f;
    private readonly float m_yEndRedZone = 4.463f;
    private readonly float m_yZoneBottom = -0.429f;
    private readonly float m_yMinZoneBottom = -0.923f;
    private readonly float m_yZoneTop = 0.439f;
    private readonly float m_yMaxZoneTop = 0.929f;
    private readonly float m_zoneBorderSize = 0.155f;
    //Shake
    private readonly float m_fShakeAmount = 0.1f;
    private readonly float m_fDecreaseFactor = 1.0f;
    //Equilibrage
    private readonly float m_fTimeEnableZoneTop = 20;//Temps à partir duquel la zoneTop est activée
    private readonly float m_fReduceSpawnRate = 0.005f;//A chaque spawn, le spawn rate diminue de cette valeur;
    private readonly float m_fAugmentSpeedRate = 20;//La vitesse augmente en fonction du temps total / 20

    //Shake
    private bool m_bIsShaking = false;
    private float m_fShakeDuration = 0f;
    private Vector3 m_vOriginalPos;
    //Score et vie
    private int m_nScore = 0;
    private int m_nLife = 3;
    //Menu
    private bool m_bIsInMenuScreen = true;
    private bool m_bIsInEndScreen = false;
    private bool m_bGameEnded = false;
    //Equilibrage
    private float m_fTimeTotal = 0;//Temps écoulé
    private float m_fTimeLastSpawn = 0;//Temps depuis le dernier spawn
    private float m_fSpawnRateTime = 2;//Temps entre chaque spawn

    // Start is called before the first frame update
    void Start()
    {
        m_canvasCtrl = GameObject.FindGameObjectWithTag("Canvas").GetComponent<CanvasController>();

        m_vOriginalPos = Camera.main.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
            Cursor.SetCursor(m_txtDragging, new Vector2(31, 25), CursorMode.ForceSoftware);

        if (Input.GetMouseButtonUp(0))
            Cursor.SetCursor(m_txtIdle, new Vector2(30, 25), CursorMode.ForceSoftware);

        if (m_bIsInMenuScreen || m_bIsInEndScreen)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
            else if(Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
        }
        else
        {
            if (m_bIsShaking)
            {
                if (m_fShakeDuration > 0)
                {
                    Camera.main.transform.localPosition = m_vOriginalPos + Random.insideUnitSphere * m_fShakeAmount;

                    m_fShakeDuration -= Time.deltaTime * m_fDecreaseFactor;
                }
                else
                {
                    m_fShakeDuration = 0f;
                    Camera.main.transform.localPosition = m_vOriginalPos;

                    m_bIsShaking = false;
                }
            }

            m_fTimeTotal += Time.deltaTime;
            m_fTimeLastSpawn += Time.deltaTime;

            if (m_fTimeLastSpawn >= m_fSpawnRateTime)
            {
                InstantiateBombZoneBottom();

                if (m_fTimeTotal > m_fTimeEnableZoneTop)
                    InstantiateBombZoneTop();

                m_fTimeLastSpawn = 0;

                m_fSpawnRateTime -= m_fReduceSpawnRate;
            }

            BombController.SetBombSpeed(1 + m_fTimeTotal / m_fAugmentSpeedRate);
        }
    }

    private void StartGame()
    {
        m_nLife = 3;
        m_nScore = 0;
        m_bGameEnded = false;
        m_fTimeTotal = 0;
        m_fTimeLastSpawn = 0;
        m_fSpawnRateTime = 2;

        m_bIsInMenuScreen = m_bIsInEndScreen = false;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Bomb"))
        {
            Destroy(go);
        }

        m_canvasCtrl.EnableEndScreen(false);
        m_canvasCtrl.EnableStartScreen(false);
        m_canvasCtrl.EnableBackground(false);
        m_canvasCtrl.EnableGameScreen(true);

        m_canvasCtrl.UpdatePoints(0);
        m_canvasCtrl.SetLife(m_nLife);

        Cursor.SetCursor(m_txtIdle, new Vector2(30, 25), CursorMode.ForceSoftware);
    }

    private void ShowEndScreen()
    {
        PlaySound(MUSIC_FX_EXPLOSION_MEDIUM);

        m_canvasCtrl.EndExplosionAnimate();
        m_canvasCtrl.UpdateEndScreenScore(m_nScore);

        m_bIsInEndScreen = true;
    }

    private void InstantiateBomb(Vector3 vPos)
    {
        GameObject go = (GameObject)Instantiate(Resources.Load("Bomb"));

        go.transform.position = vPos;

        BombController bombCtrl = go.GetComponent<BombController>();
        bombCtrl.SetGameController(this);
        bombCtrl.InitZones(m_xStartBlueZone, m_xEndBlueZone, m_yStartBlueZone, m_yEndBlueZone, m_xStartRedZone, m_xEndRedZone, m_yStartRedZone, m_yEndRedZone, m_yZoneBottom, m_yZoneTop, m_zoneBorderSize, m_yMinZoneBottom, m_yMaxZoneTop);

        bombCtrl.MouseUpEvent += BombCtrl_MouseUpEvent;
        bombCtrl.BombFallenEvent += BombCtrl_BombFallenEvent;
        bombCtrl.BombExplodedenEvent += BombCtrl_BombExplodedenEvent;
        bombCtrl.BombDisappearedEvent += BombCtrl_BombDisappearedEvent;
        bombCtrl.BombExplosionStartEvent += BombCtrl_BombExplosionStartEvent;
    }

    private void InstantiateBombZoneBottom()
    {
        InstantiateBomb(new Vector3(X_SPAWN_LEFT, m_yZoneBottom, -2));
    }

    private void InstantiateBombZoneTop()
    {
        InstantiateBomb(new Vector3(X_SPAWN_RIGHT, m_yZoneTop, -2));
    }

    public bool IsGameEnded()
    {
        return m_bGameEnded;
    }

    private void StartShakeScreen()
    {
        m_bIsShaking = true;
        m_fShakeDuration = 0.5f;
    }

    public void PlaySound(AudioClip audioClip)
    {
        AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
    }

    private void BombCtrl_BombDisappearedEvent(object sender, System.EventArgs args)
    {
        RemoveLife();
    }

    private void BombCtrl_BombExplodedenEvent(object sender, System.EventArgs args)
    {
        RemoveLife();
    }

    private void BombCtrl_BombFallenEvent(object sender, System.EventArgs args)
    {
        RemoveLife();
    }

    private void BombCtrl_BombExplosionStartEvent(object sender, System.EventArgs args)
    {
        StartShakeScreen();
    }

    private void RemoveLife()
    {
        m_nLife--;

        if (m_nLife == 0)
        {
            this.m_bGameEnded = true;
            this.m_fSpawnRateTime = 9999;

            StartCoroutine(WaitAndShowEndScreen());
        }

        m_canvasCtrl.SetLife(m_nLife);
    }

    private void BombCtrl_MouseUpEvent(object sender, MouseEventArgs args)
    {
        BombController bombCtrl = (BombController)sender;

        if ((bombCtrl.GetBombType() == BombController.BOMB_TYPE.RED && args.bRedZone)
            || (bombCtrl.GetBombType() == BombController.BOMB_TYPE.BLUE && args.bBlueZone))
        {
            m_nScore++;
            m_canvasCtrl.UpdatePoints(m_nScore);
        }
    }

    private IEnumerator WaitAndShowEndScreen()
    {
        yield return new WaitForSeconds(0.2f);

        ShowEndScreen();
    }
}
