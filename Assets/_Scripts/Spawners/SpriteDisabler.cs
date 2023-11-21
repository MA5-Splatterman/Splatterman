using UnityEngine;

public class SpriteDisabler : MonoBehaviour
{
    void Awake()
    {
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer)) {
			renderer.enabled = false;
		}
    }

}
