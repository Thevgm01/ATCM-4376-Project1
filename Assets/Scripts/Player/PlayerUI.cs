using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject player;

    Health playerHealth;
    RawImage crosshair;
    RectTransform HPBar;
    RawImage damageHighlight;

    float flashStartAlpha = 0.5f;
    float flashDuration = 0.2f;

    // Start is called before the first frame update
    void Awake()
    {
        playerHealth = player.GetComponent<Health>();
        playerHealth.onDamaged += Damaged;
        playerHealth.onKilled += Died;

        crosshair = transform.Find("Crosshair").GetComponent<RawImage>();
        HPBar = transform.Find("HP").Find("Foreground").GetComponent<RectTransform>();
        damageHighlight = transform.Find("Damage Highlight").GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Damaged(int newHP)
    {
        HPBar.sizeDelta = new Vector2(100 * (float)newHP / playerHealth.maxHealth, 100);
        StartCoroutine("ScreenFlash", flashDuration);
    }

    void Died()
    {
        StartCoroutine("ScreenFlash", flashDuration * 3);
        crosshair.gameObject.SetActive(false);
        HPBar.transform.parent.gameObject.SetActive(false);
    }

    IEnumerator ScreenFlash(float duration)
    {
        damageHighlight.gameObject.SetActive(true);
        var col = damageHighlight.color;
        float timer = duration;
        while(timer > 0f)
        {
            damageHighlight.color = new Color(col.r, col.g, col.b, Mathf.Lerp(0f, flashStartAlpha, timer / duration));
            timer -= Time.deltaTime;
            yield return null;
        }
        damageHighlight.gameObject.SetActive(false);
    }
}
