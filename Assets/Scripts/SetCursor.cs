using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCursor : MonoBehaviour
{
    public Texture2D normalCursor;
    public Texture2D clickCursor;

    public Vector2 hotspot = Vector2.zero;

    public static SetCursor Instance;

    void Awake()
    {
        Cursor.SetCursor(normalCursor, hotspot, CursorMode.Auto);
        // If another instance already exists, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Otherwise, set this as the instance
        Instance = this;

        // Make it persist between scenes
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click pressed
        {
            Cursor.SetCursor(clickCursor, hotspot, CursorMode.Auto);
        }

        if (Input.GetMouseButtonUp(0)) // Left click released
        {
            Cursor.SetCursor(normalCursor, hotspot, CursorMode.Auto);
        }
    }
}
