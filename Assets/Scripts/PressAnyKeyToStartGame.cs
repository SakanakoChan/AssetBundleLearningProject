using UnityEngine;

public class PressAnyKeyToStartGame : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            gameObject.SetActive(false);
        }
    }
}
