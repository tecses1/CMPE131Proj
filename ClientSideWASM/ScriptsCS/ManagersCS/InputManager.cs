namespace ClientSideWASM;

using System.Numerics;
using Shared;


public static class InputManager
{
    public static ClientInputWrapper currentInput = new ClientInputWrapper();
    static Vector2 _mousePosWorld = new Vector2(0,0);
    public static Vector2 MousePosWorld
    {
        get
        {
            _mousePosWorld.X = (float)currentInput.MouseXWorld;
            _mousePosWorld.Y = (float)currentInput.MouseYWorld;
            return _mousePosWorld;
        }
    }
    static Vector2 _mousePos = new Vector2();
    public static Vector2 MousePos
    {
        get
        {
            _mousePos.X = (float)currentInput.MouseX;
            _mousePos.Y = (float)currentInput.MouseY;
            return _mousePos;
        }
    }
    static Rect _mouseRect = new Rect(0,0,2,2);

    public static Rect MouseRect
    {
        get
        {
            _mouseRect.X = (float)currentInput.MouseX;
            _mouseRect.Y = (float)currentInput.MouseY;
            return _mouseRect;
        }
    }


    static Rect _mouseRectWorld = new Rect(0,0,2,2);

    public static Rect MouseRectWorld
    {
        get
        {
            _mouseRectWorld.X = (float)currentInput.MouseXWorld;
            _mouseRectWorld.Y = (float)currentInput.MouseYWorld;
            return _mouseRectWorld;
        }
    }

    public static void Flush()
    {
        currentInput.Flush();
    }

}