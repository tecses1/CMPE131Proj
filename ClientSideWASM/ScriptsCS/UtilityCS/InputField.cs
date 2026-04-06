using ClientSideWASM;
using Shared;
using System.Drawing;
public class InputField : DrawText
{
    public string placeholder;
    public DateTime lastTick = DateTime.Now;
    public bool stroke = false;
    
    public InputField(string placeholder, Transform t) : base(placeholder, t)
    {
        this.placeholder = placeholder;
        setTextColor(Color.DarkGray,255);
        setBorderColor(Color.LightGray,255);
        setFillColor(Color.White,255);
    }
    //returns true if enter is pressed, to signal we're done editing.
    public bool Update(ClientInputWrapper ci)
    {
        // If the text is currently the placeholder, and it's been at least 500ms since the last tick, toggle the visibility of the placeholder text
        if ((DateTime.Now - lastTick).TotalMilliseconds >= 500)
        {
            if (stroke)
            {
                this.text = placeholder + " "; // Show the placeholder
            }
            else
            {
                this.text = placeholder + "|"; // Hide the placeholder
            }
            stroke = !stroke; // Toggle the stroke state
            lastTick = DateTime.Now; // Reset the tick timer
        }
        foreach (var key in ci.keysPressed)
        {
            
            if (ci.CKeyPressed(key.Key)) // Check if this key was just pressed
            {
            
            
              if (key.Key == "Backspace")
                {
                    if (this.placeholder.Length > 0)
                    {
                        this.placeholder = this.placeholder.Substring(0, this.placeholder.Length - 1);
                    }
                }
                else if (key.Key == "Enter")
                {
                    // You can handle the enter key here if needed
                    this.text = placeholder; // Ensure the text is updated to the final input
                    return true;
                }
                else 
                {
                    if (key.Key.Length == 1) // Only consider single-character keys
                    {
                        if (Char.IsLetterOrDigit(key.Key[0]) || Char.IsWhiteSpace(key.Key[0]) || Char.IsPunctuation(key.Key[0]))
                        {
                            this.placeholder += key.Key; // Append the pressed key to the placeholder
                        }
                    }
                }
            } 
            
        }
        return false;
    }

}