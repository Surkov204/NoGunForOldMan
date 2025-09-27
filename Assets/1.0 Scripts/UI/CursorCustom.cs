using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorCTexture: MonoBehaviour
{
    [SerializeField] private Texture2D cursorImage;
    private Vector2 cursorHotPot;

    private void Awake()
    {
        cursorHotPot = new Vector2(cursorImage.width/2, cursorImage.height/2); ;
        Cursor.SetCursor(cursorImage, cursorHotPot, CursorMode.Auto);
    }
}
