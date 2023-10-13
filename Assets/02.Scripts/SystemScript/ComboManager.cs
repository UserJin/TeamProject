using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComboManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static ComboManager instance = null;

    public int combo;
    public int maxCombo;
    public TMP_Text cur_Combo;

    private bool isCombo;

    IEnumerator comboCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        initCombo();
        isCombo = false;
    }

    public void initCombo()
    {
        combo = 0;
        maxCombo = 0;
        comboCount = ComboCount();
    }

    public void AddCombo()
    {
        combo += 1;
        cur_Combo.text = $"COMBO: {combo}";
        if (combo > maxCombo) maxCombo = combo;
        if(isCombo)
        {
            StopCoroutine(comboCount);
            comboCount = ComboCount();
        }
        isCombo = true;
        StartCoroutine(comboCount);
    }

    public void StopCombo()
    {
        StopCoroutine(comboCount);
        GameManager.instance.scoreManager.CalCombo(combo);
        combo = 0;
        cur_Combo.text = $"COMBO: {combo}";
        comboCount = ComboCount();
    }

    IEnumerator ComboCount()
    {
        yield return new WaitForSecondsRealtime(5.0f);
        GameManager.instance.scoreManager.CalCombo(combo);
        combo = 0;
        cur_Combo.text = $"COMBO: {combo}";
        isCombo = false;
        comboCount = ComboCount();
    }
}
