using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    [SerializeField]
    private Sprite m_sprtLife_1;

    [SerializeField]
    private Sprite m_sprtLife_2;

    [SerializeField]
    private Sprite m_sprtLife_3;

    [SerializeField]
    private Sprite m_sprtLifeEmpty_1;

    [SerializeField]
    private Sprite m_sprtLifeEmpty_2;

    [SerializeField]
    private Sprite m_sprtLifeEmpty_3;

    [SerializeField]
    private Sprite m_sprt_0;
    [SerializeField]
    private Sprite m_sprt_1;
    [SerializeField]
    private Sprite m_sprt_2;
    [SerializeField]
    private Sprite m_sprt_3;
    [SerializeField]
    private Sprite m_sprt_4;
    [SerializeField]
    private Sprite m_sprt_5;
    [SerializeField]
    private Sprite m_sprt_6;
    [SerializeField]
    private Sprite m_sprt_7;
    [SerializeField]
    private Sprite m_sprt_8;
    [SerializeField]
    private Sprite m_sprt_9;

    private Animator m_animator;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_animator.enabled = false;
    }

    private void EnablePoints(bool bValue)
    {
        transform.Find("points").gameObject.SetActive(bValue);
    }

    private void EnableLife(bool bValue)
    {
        transform.Find("pnlLife").gameObject.SetActive(bValue);
    }

    public void EnableBackground(bool bValue)
    {
        transform.Find("background").gameObject.SetActive(bValue);
    }

    public void EnableStartScreen(bool bValue)
    {
        transform.Find("MenuScreen").gameObject.SetActive(bValue);
    }

    public void EnableEndScreen(bool bValue)
    {
        transform.Find("EndScreen").gameObject.SetActive(bValue);
    }

    public void EnableGameScreen(bool bValue)
    {
        EnableLife(bValue);
        EnablePoints(bValue);
    }

    public void ShowEndEscreen()
    {
        m_animator.enabled = false;
        m_animator.Rebind();
        transform.Find("endObject").gameObject.SetActive(false);

        EnableEndScreen(true);
        EnableBackground(true);
        EnableStartScreen(false);
        EnableGameScreen(false);
    }

    public void EndExplosionAnimate()
    {
        transform.Find("endObject").gameObject.SetActive(true);

        m_animator.enabled = true;
    }

    public void UpdatePoints(int nbPoints)
    {
        int nCentaines = (nbPoints / 100) % 10;
        int nDizaines = (nbPoints / 10) % 10;
        int nUnites = nbPoints % 10;

        SetUnitImageSprite(transform.Find("points").Find("score_100").GetComponent<Image>(), nCentaines);
        SetUnitImageSprite(transform.Find("points").Find("score_10").GetComponent<Image>(), nDizaines);
        SetUnitImageSprite(transform.Find("points").Find("score_1").GetComponent<Image>(), nUnites);
    }

    public void UpdateEndScreenScore(int nScore)
    {
        int nCentaines = (nScore / 100) % 10;
        int nDizaines = (nScore / 10) % 10;
        int nUnites = nScore % 10;

        SetUnitImageSprite(transform.Find("EndScreen").Find("score_100").GetComponent<Image>(), nCentaines);
        SetUnitImageSprite(transform.Find("EndScreen").Find("score_10").GetComponent<Image>(), nDizaines);
        SetUnitImageSprite(transform.Find("EndScreen").Find("score_1").GetComponent<Image>(), nUnites);
    }

    public void SetUnitImageSprite(Image img, int nUnit)
    {
        switch (nUnit)
        {
            case 0: 
                img.sprite = m_sprt_0;
                break;
            case 1:
                img.sprite = m_sprt_1;
                break;
            case 2:
                img.sprite = m_sprt_2;
                break;
            case 3:
                img.sprite = m_sprt_3;
                break;
            case 4:
                img.sprite = m_sprt_4;
                break;
            case 5:
                img.sprite = m_sprt_5;
                break;
            case 6:
                img.sprite = m_sprt_6;
                break;
            case 7:
                img.sprite = m_sprt_7;
                break;
            case 8:
                img.sprite = m_sprt_8;
                break;
            case 9:
                img.sprite = m_sprt_9;
                break;
        }
    }

    public void SetLife(int nLife)
    {
        Transform tLife1 = transform.Find("pnlLife").Find("image_1");
        Transform tLife2 = transform.Find("pnlLife").Find("image_2");
        Transform tLife3 = transform.Find("pnlLife").Find("image_3");

        tLife1.GetComponent<Image>().sprite = nLife > 0 ? m_sprtLife_1 : m_sprtLifeEmpty_1;
        tLife2.GetComponent<Image>().sprite = nLife > 1 ? m_sprtLife_2 : m_sprtLifeEmpty_2;
        tLife3.GetComponent<Image>().sprite = nLife > 2 ? m_sprtLife_3 : m_sprtLifeEmpty_3;
    }
}
