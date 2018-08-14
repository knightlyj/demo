using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerInfoPanel : MonoBehaviour
{
    [SerializeField]
    Text txtName = null;
    [SerializeField]
    Outline olName = null;
    [SerializeField]
    Bar hpBar = null;
    
    [HideInInspector]
    public Player player = null;
    int playerId = -1;
    
    // Use this for initialization
    void Start()
    {
        if (player != null)
        {
            playerId = player.id;
            EventManager.AddListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
            txtName.text = player.nameInGame;
        }
        else
        {
            Debug.LogError("PlayerInfoPane.Start >> player doesn't exist");
            Destroy(gameObject);
        }
    }

    float hideDistance = 15f;
    float alpha = 0f;
    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            float ratio = player.healthPoint / Player.maxHealth;
            if (ratio < 0)
                ratio = 0f;
            else if (ratio > 1)
                ratio = 1f;
            hpBar.SetRatio(ratio);
            // 设置位置
            Vector3 worldPos = player.transform.position + Vector3.up * 3f;
            Vector3 viewPortPos = Camera.main.WorldToViewportPoint(worldPos);

            if (viewPortPos.z > Camera.main.nearClipPlane)
            {
                txtName.gameObject.SetActive(true);
                hpBar.gameObject.SetActive(true);

                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
                RectTransform rt = transform as RectTransform;
                rt.anchorMin = new Vector2(screenPos.x / Screen.width - 0.1f, screenPos.y / Screen.height);
                rt.anchorMax = new Vector2(screenPos.x / Screen.width + 0.1f, screenPos.y / Screen.height);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                txtName.gameObject.SetActive(false);
                hpBar.gameObject.SetActive(false);
            }

            Vector3 toCamera = Camera.main.transform.position - player.transform.position;
            if (toCamera.magnitude > hideDistance)
            {
                alpha -= Time.deltaTime;
                if (alpha < 0f)
                    alpha = 0f;
            }
            else
            {
                alpha += Time.deltaTime;
                if (alpha > 1f)
                    alpha = 1f;
            }
            SetAlpha();
        }
    }

    void OnDestroy()
    {
        EventManager.RemoveListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
    }

    void SetAlpha()
    {
        hpBar.SetAlpha(alpha);

        Color c = olName.effectColor;
        c.a = alpha;
        olName.effectColor = c;

        c = txtName.color;
        c.a = alpha;
        txtName.color = c;
    }

    void OnPlayerDestroy(object sender, object eventArg)
    {
        EventManager.RemoveListener(EventId.RemovePlayer, playerId, this.OnPlayerDestroy);
        Destroy(gameObject);
    }
}
