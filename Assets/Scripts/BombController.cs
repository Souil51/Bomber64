using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MouseEventArgs : EventArgs
{
    public bool bRedZone;
    public bool bBlueZone;
}

public class BombController : MonoBehaviour
{
    public enum BOMB_TYPE { BLUE = 0, RED = 1 }

    private static float BOMB_SPEED = 1f;

    public delegate void MouseUpEventHandler(object sender, MouseEventArgs args);
    public delegate void BombFallenEventHandler(object sender, EventArgs args);
    public delegate void BombExplodedEventHandler(object sender, EventArgs args);
    public delegate void BombDisappearedEventHandler(object sender, EventArgs args);
    public delegate void BombExplosionStartEventHandler(object sender, EventArgs args);

    public event MouseUpEventHandler MouseUpEvent;
    public event BombFallenEventHandler BombFallenEvent;
    public event BombExplodedEventHandler BombExplodedenEvent;
    public event BombDisappearedEventHandler BombDisappearedEvent;
    public event BombExplosionStartEventHandler BombExplosionStartEvent;

    [SerializeField]
    private Sprite m_sprtRED;

    [SerializeField]
    private Sprite m_sprtBLUE;

    private GameController m_gameCtrl;
    private SpriteRenderer m_sprtRenderder;
    private Rigidbody2D m_rb2d;
    private Animator m_animator;

    private readonly float m_fXForce = 0.01f;
    private readonly float m_fYForce = 0.01f;

    //Définition des zones
    private float m_xStartBlueZone;
    private float m_xEndBlueZone;
    private float m_yStartBlueZone;
    private float m_yEndBlueZone;
    private float m_xStartRedZone;
    private float m_xEndRedZone;
    private float m_yStartRedZone;
    private float m_yEndRedZone;
    private float m_yZoneBottom;
    private float m_yZoneTop;
    private float m_yMinZoneBottom;
    private float m_yMaxZoneTop;
    private float m_zoneBorderSize;

    private bool m_bDragging = false;
    private bool m_bEnabled = true;
    private float distance;
    private BOMB_TYPE m_type;
    private float m_fHalfSize;
    private float m_fSqrtSize;
    
    // Start is called before the first frame update
    void Start()
    {
        this.m_sprtRenderder = GetComponent<SpriteRenderer>();
        this.m_animator = GetComponent<Animator>();
        this.m_rb2d = GetComponent<Rigidbody2D>();

        //Bombe aléatoire
        int nType = Random.Range(0, 2);
        m_type = (BOMB_TYPE)nType;
        
        switch (m_type)
        {
            case BOMB_TYPE.BLUE: 
                m_sprtRenderder.sprite = m_sprtBLUE;
                break;
            case BOMB_TYPE.RED: 
                m_sprtRenderder.sprite = m_sprtRED; 
                break;
        }

        PlayAnimation("Idle");

        this.m_fHalfSize = m_sprtRenderder.sprite.bounds.size.x / 2;
        this.m_fSqrtSize = Mathf.Sqrt(Mathf.Pow(m_fHalfSize, 2) / 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_gameCtrl.IsGameEnded()) return;

        if(transform.position.y == m_yZoneBottom)
            transform.position += new Vector3(BOMB_SPEED * Time.deltaTime, 0, 0);
        else if(transform.position.y == m_yZoneTop)
            transform.position += new Vector3(-BOMB_SPEED * Time.deltaTime, 0, 0);

        if (m_bDragging && m_bEnabled)
        {
            UpdatePosition();
        }

        if(m_bEnabled && (transform.position.x < GameController.X_SPAWN_LEFT || transform.position.x > GameController.X_SPAWN_RIGHT) && (transform.position.y == m_yZoneTop || transform.position.y == m_yZoneBottom))
        {
            BombDisappear();
        }
    }

    public static void SetBombSpeed(float fSpeed)
    {
        BOMB_SPEED = fSpeed;
    }

    public void InitZones(float xStartBlue, float xEndBlue, float yStartBlue, float yEndBlue, float xStartRed, float xEndRed, float yStartRed, float yEndRed, float yZoneBottom, float yZoneTop, float zoneBorderSize, float yMinZoneBottom, float yMaxZoneTop)
    {
        m_xStartBlueZone = xStartBlue;
        m_xEndBlueZone = xEndBlue;
        m_yStartBlueZone = yStartBlue;
        m_yEndBlueZone = yEndBlue;

        m_xStartRedZone = xStartRed;
        m_xEndRedZone = xEndRed;
        m_yStartRedZone = yStartRed;
        m_yEndRedZone = yEndRed;

        m_yZoneBottom = yZoneBottom;
        m_yZoneTop = yZoneTop;

        m_zoneBorderSize = zoneBorderSize;

        m_yMinZoneBottom = yMinZoneBottom;
        m_yMaxZoneTop = yMaxZoneTop;
    }

    public void SetGameController(GameController gameCtrl)
    {
        m_gameCtrl = gameCtrl;
    }

    private Vector3 UpdatePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        Vector3 vRes = new Vector3(rayPoint.x, rayPoint.y, transform.position.z);
        transform.position = vRes;

        return vRes;
    }

    private void SetRigidBodyForce()
    {
        int nRand = Random.Range(0, 4);

        switch (nRand)
        {
            case 0: m_rb2d.AddForce(new Vector2(m_fXForce, m_fYForce), ForceMode2D.Force);
                break;
            case 1:
                m_rb2d.AddForce(new Vector2(-m_fXForce, m_fYForce), ForceMode2D.Force);
                break;
            case 2:
                m_rb2d.AddForce(new Vector2(m_fXForce, -m_fYForce), ForceMode2D.Force);
                break;
            case 3:
                m_rb2d.AddForce(new Vector2(-m_fXForce, -m_fYForce), ForceMode2D.Force);
                break;
        }
    }

    void OnMouseEnter()
    {
        if (m_gameCtrl.IsGameEnded()) return;

        //m_sprtRenderder.material.color = mouseOverColor;
    }

    void OnMouseExit()
    {
        if (m_gameCtrl.IsGameEnded()) return;

        //m_sprtRenderder.material.color = originalColor;
    }

    void OnMouseDown()
    {
        if (m_gameCtrl.IsGameEnded()) return;

        if (m_bEnabled)
        {
            distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            m_bDragging = true;

            UpdatePosition();

            m_animator.Rebind();
            PlayAnimation("Idle");
        }
    }

    void OnMouseUp()
    {
        if (m_gameCtrl.IsGameEnded()) return;

        if (m_bDragging && m_bEnabled)
        {
            m_bDragging = false;

            bool bIsInZone = IsInZone(out Vector3 vNewPos, out bool bRedZone, out bool bBlueZone, out bool bCenterZoneBottom, out bool bCenterZoneTop);

            transform.position = vNewPos;

            if (!bIsInZone && !bCenterZoneBottom && !bCenterZoneTop)
            {
                PlayAnimation("BombFallAnimation");

                m_gameCtrl.PlaySound(m_gameCtrl.MUSIC_FX_FALL);
            }
            else if((m_type == BOMB_TYPE.BLUE && bRedZone) || (m_type == BOMB_TYPE.RED && bBlueZone))
            {
                PlayAnimation("BombWrongZone");

                transform.position = new Vector3(transform.position.x, transform.position.y, -3);

                m_gameCtrl.PlaySound(m_gameCtrl.MUSIC_FX_WRONG_ZONE);
            }
            else if(bIsInZone)
            {
                m_bEnabled = false;

                SetRigidBodyForce();

                transform.position = new Vector3(transform.position.x, transform.position.y, -2);

                m_gameCtrl.PlaySound(m_gameCtrl.MUSIC_FX_RIGHT_ZONE);
            }
            else if(bCenterZoneBottom || bCenterZoneTop)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -2);
            }

            if (this.MouseUpEvent != null)
            {
                MouseEventArgs args = new MouseEventArgs
                {
                    bRedZone = bRedZone,
                    bBlueZone = bBlueZone
                };

                MouseUpEvent(this, args);
            }
        }
    }

    public BOMB_TYPE GetBombType()
    {
        return this.m_type;
    }

    private void PlayAnimation(string szAnimation)
    {
        string szSuffixe = m_type == BOMB_TYPE.BLUE ? "Blue" : "Red";

        m_animator.Play(szAnimation + szSuffixe);
    }

    public void Animation_BombFallen()
    {
        BombFallenEvent?.Invoke(this, EventArgs.Empty);

        Destroy(this.gameObject);
    }

    public void Animation_Disable()
    {
        m_gameCtrl.PlaySound(m_gameCtrl.MUSIC_FX_EXPLOSION);

        BombExplosionStartEvent?.Invoke(this, EventArgs.Empty);

        this.m_bEnabled = false;
    }

    public void Animation_Exploded()
    {
        BombExplodedenEvent?.Invoke(this, EventArgs.Empty);

        Destroy(this.gameObject);
    }

    public void Animation_Destroy()
    {
        Destroy(gameObject);
    }

    private void Animation_PlayTicTac()
    {
        m_gameCtrl.PlaySound(m_gameCtrl.MUSIC_FX_TIC_TAC);
    }

    public void BombDisappear()
    {
        this.m_bEnabled = false;

        m_gameCtrl.PlaySound(m_gameCtrl.MUSIC_FX_EXPLOSION);

        m_animator.Play("BombExplosion");

        BombExplosionStartEvent?.Invoke(this, EventArgs.Empty);

        BombDisappearedEvent?.Invoke(this, EventArgs.Empty);
    }

    public void UpdatePosition(Vector3 vPos)
    {
        transform.position = vPos;
    }

    /// <summary>
    /// Vérifie si la bomb est dans une des zones
    /// </summary>
    /// <param name="vCurrentPos">La position actuelle de la bombe</param>
    /// <param name="vNewBombPos">out : nouvelle position de la bombe pour éviter qu'elle soit sur la bordure d'un des zone</param>
    /// <returns></returns>
    public bool IsInZone(out Vector3 vNewBombPos, out bool bRedZone, out bool bBlueZone, out bool bCenterZoneBottom, out bool bCenterZoneTop)
    {
        bool bRes = false;
        Vector3 vCurrentPos = transform.position;
        vNewBombPos = vCurrentPos;
        bRedZone = bBlueZone = bCenterZoneBottom = bCenterZoneTop = false;

        if (vCurrentPos.y >= m_yEndBlueZone && vCurrentPos.y <= 0f)
        {
            bCenterZoneBottom = true;
            vNewBombPos.y = m_yZoneBottom;
        }
        else if (vCurrentPos.y > 0f && vCurrentPos.y <= m_yStartRedZone)
        {
            bCenterZoneTop = true;
            vNewBombPos.y = m_yZoneTop;
        }
        //Zone rouge
        else if (vCurrentPos.x >= m_xStartRedZone && vCurrentPos.x <= m_xEndRedZone
            && vCurrentPos.y >= m_yStartRedZone && vCurrentPos.y <= m_yEndRedZone)
        {
            bRes = true;
            bRedZone = true;

            //Si la bombe est sur une bordure mais à l'intérieur
            if (vCurrentPos.x < m_xStartRedZone + m_fHalfSize)
                vNewBombPos.x = m_xStartRedZone + m_fHalfSize;
            else if (vCurrentPos.x > m_xEndRedZone - m_fHalfSize)
                vNewBombPos.x = m_xEndRedZone - m_fHalfSize;

            if (vCurrentPos.y < m_yStartRedZone + m_fHalfSize)
                vNewBombPos.y = m_yStartRedZone + m_fHalfSize;
            else if (vCurrentPos.y > m_yEndRedZone - m_fHalfSize)
                vNewBombPos.y = m_yEndRedZone - m_fHalfSize;
        }
        else if (vCurrentPos.x >= m_xStartRedZone - m_fHalfSize - m_zoneBorderSize && vCurrentPos.x <= m_xEndRedZone + m_fHalfSize + m_zoneBorderSize
                    && vCurrentPos.y >= m_yStartRedZone - m_fHalfSize - m_zoneBorderSize && vCurrentPos.y <= m_yEndRedZone + m_fHalfSize + m_zoneBorderSize)
        {
            //Si la bombee est sur une bordure mais à l'extérieur
            bool bNewXLeft = vCurrentPos.x < m_xStartRedZone;
            bool bNewXRight = vCurrentPos.x > m_xEndRedZone;
            bool bNewYBottom = vCurrentPos.y < m_yStartRedZone;
            bool bNewYTop = vCurrentPos.y > m_yEndRedZone;

            //X
            float fSize = m_fHalfSize;
            if (bNewYBottom || bNewYTop)
                fSize = m_fSqrtSize;

            if (bNewXLeft)
                vNewBombPos.x = m_xStartRedZone - fSize - m_zoneBorderSize;
            else if (bNewXRight)
                vNewBombPos.x = m_xEndRedZone + fSize + m_zoneBorderSize;

            //Y
            fSize = m_fHalfSize;
            if (bNewXLeft || bNewXRight)
                fSize = m_fSqrtSize;

            if (bNewYBottom)
                vNewBombPos.y = m_yStartRedZone - fSize - m_zoneBorderSize;
            else if (bNewYTop)
                vNewBombPos.y = m_yEndRedZone + fSize + m_zoneBorderSize;
        } //Zone bleu
        else if (vCurrentPos.x >= m_xStartBlueZone && vCurrentPos.x <= m_xEndBlueZone
            && vCurrentPos.y >= m_yStartBlueZone && vCurrentPos.y <= m_yEndBlueZone)
        {
            bRes = true;
            bBlueZone = true;

            //Si la bombe est sur une bordure mais à l'intérieur
            if (vCurrentPos.x < m_xStartBlueZone + m_fHalfSize)
                vNewBombPos.x = m_xStartBlueZone + m_fHalfSize;
            else if (vCurrentPos.x > m_xEndBlueZone - m_fHalfSize)
                vNewBombPos.x = m_xEndBlueZone - m_fHalfSize;

            if (vCurrentPos.y < m_yStartBlueZone + m_fHalfSize)
                vNewBombPos.y = m_yStartBlueZone + m_fHalfSize;
            else if (vCurrentPos.y > m_yEndBlueZone - m_fHalfSize)
                vNewBombPos.y = m_yEndBlueZone - m_fHalfSize;
        }
        else if (vCurrentPos.x >= m_xStartBlueZone - m_fHalfSize - m_zoneBorderSize && vCurrentPos.x <= m_xEndBlueZone + m_fHalfSize + m_zoneBorderSize
                    && vCurrentPos.y >= m_yStartBlueZone - m_fHalfSize - m_zoneBorderSize && vCurrentPos.y <= m_yEndBlueZone + m_fHalfSize + m_zoneBorderSize)
        {
            //Si la bombe est sur une bordure mais à l'extérieur
            bool bNewXLeft = vCurrentPos.x < m_xStartBlueZone;
            bool bNewXRight = vCurrentPos.x > m_xEndBlueZone;
            bool bNewYBottom = vCurrentPos.y < m_yStartBlueZone;
            bool bNewYTop = vCurrentPos.y > m_yEndBlueZone;

            //X
            float fSize = m_fHalfSize;
            if (bNewYBottom || bNewYTop)
                fSize = m_fSqrtSize;

            if (bNewXLeft)
                vNewBombPos.x = m_xStartBlueZone - fSize - m_zoneBorderSize;
            else if (bNewXRight)
                vNewBombPos.x = m_xEndBlueZone + fSize + m_zoneBorderSize;

            //Y
            fSize = m_fHalfSize;
            if (bNewXLeft || bNewXRight)
                fSize = m_fSqrtSize;

            if (bNewYBottom)
                vNewBombPos.y = m_yStartBlueZone - fSize - m_zoneBorderSize;
            else if (bNewYTop)
                vNewBombPos.y = m_yEndBlueZone + fSize + m_zoneBorderSize;
        }

        return bRes;
    }
}
