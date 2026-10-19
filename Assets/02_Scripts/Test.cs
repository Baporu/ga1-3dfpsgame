using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
#if UNITY_EDITOR
        Debug.Log("Hello World!");
#endif
    }

    private void Update()
    {
    }
}