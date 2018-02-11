using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FadingSpriteComponent))]
public class TachiComponent : MonoBehaviour {

    private const float StandardTimeMult = 0.6f;
    private const float FastTimeMult = 0.2f;

    private CharaData chara;
    private ScenePlayer player;

    public void Awake() {
        player = FindObjectOfType<ScenePlayer>();
    }

    public void SetChara(string charaTag) {
        SetChara(Global.Instance().Database.Charas.GetData(charaTag));
    }

    public void SetChara(CharaData chara) {
        this.chara = chara;
        GetComponent<SpriteRenderer>().sprite = chara.portrait;
    }

    public bool ContainsChara(CharaData chara) {
        if (this.chara == null || GetComponent<TransitionComponent>().IsTransitioning()) {
            return false;
        } else {
            return this.chara.tag.Equals(chara.tag);
        }
    }

    public IEnumerator FadeCharaIn(string charaTag, FadeData fade) {
        gameObject.SetActive(true);
        float timeMult = player.ShouldUseFastMode() ? FastTimeMult : StandardTimeMult;
        SetChara(charaTag);
        yield return StartCoroutine(GetComponent<TransitionComponent>().FadeRoutine(fade, true, timeMult));
    }

    public IEnumerator FadeOut(FadeData fade) {
        if (!gameObject.activeInHierarchy) {
            yield break;
        }
        float timeMult = player.ShouldUseFastMode() ? FastTimeMult : StandardTimeMult;
        yield return StartCoroutine(GetComponent<TransitionComponent>().FadeRoutine(fade, false, timeMult));
        gameObject.SetActive(false);
    }

    public TachiMemory ToMemory() {
        TachiMemory memory = new TachiMemory();
        if (gameObject.activeSelf && chara != null) {
            memory.charaTag = chara.tag;
            memory.enabled = true;
        } else {
            memory.charaTag = null;
            memory.enabled = false;
        }
        return memory;
    }

    public void PopulateFromMemory(TachiMemory memory) {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (memory.enabled) {
            SetChara(Global.Instance().Database.Charas.GetData(memory.charaTag));
            gameObject.SetActive(true);
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1.0f);
        } else {
            gameObject.SetActive(false);
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.0f);
        }
    }
}
