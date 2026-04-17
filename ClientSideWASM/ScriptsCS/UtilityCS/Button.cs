namespace ClientSideWASM;

using System.Drawing;
using Blazorex;
using Shared;

public class Button : DrawText
{

    public Color defaultColor;
    public Color hoverColor;

    bool _clicked = false;

    public bool clicked
    {
        get
        {
            if (_clicked)
            {
                _clicked = false; // Reset the clicked state after it's been read
                return true;
            }
            return false;
        }
        set
        {
            _clicked = value;
        }
    }


    public Button(string text, Transform t) : base(text,t)
    {
        defaultColor = Color.White;
        hoverColor = Color.Blue;
    }

    public void Update()
    {
        if (this.transform.rect.IntersectsWith(InputManager.MouseRect))
        {
            this.setTextColor(hoverColor,255);
            this.clicked = InputManager.currentInput.LeftPressed;
        }
        else
        {
            this.setTextColor(defaultColor,255);
        }

    }

}