namespace CMPE131Proj;
using System;
using Blazorex;

public class InputWrapper
{

    //store state of events, then pass input as a whole package on frame.
    //W, A, S, D
    //add to this list for any keys desired and update it in load keys.
    public bool[] keys = {false,false,false,false};
    public MouseMoveEvent cMouseMovementInput;
    public MouseClickEvent cMouseClickInput;
    public InputWrapper()
    {
        
    }

    public void loadKeysDown(KeyboardPressEvent keysDown)
    {
        switch (keysDown.Key){
            case "w": keys[0] = true; break;
            case "a": keys[1] = true; break;
            case "s": keys[2] = true; break;
            case "d": keys[3] = true; break;
            default: break;
        }

    }

    public void loadKeysUp(KeyboardPressEvent keysUp)
    {
        switch (keysUp.Key){
            case "w": keys[0] = false; break;
            case "a": keys[1] = false; break;
            case "s": keys[2] = false; break;
            case "d": keys[3] = false; break;
            default: break;
        }
    }
    public void Clear()
    {

    }
    
}
