using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public GameObject player;

    Health playerHealth;
    RectTransform HPBar;

    // Start is called before the first frame update
    void Awake()
    {
        playerHealth = player.GetComponent<Health>();
        playerHealth.onDamaged += UpdateHP;
        playerHealth.onKilled += Disable;

        HPBar = transform.Find("HP").Find("Foreground").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void UpdateHP(int newHP)
    {
        HPBar.sizeDelta = new Vector2(100 * (float)newHP / playerHealth.maxHealth, 100);
    }

    void Disable()
    {
        gameObject.SetActive(false);
    }
}
