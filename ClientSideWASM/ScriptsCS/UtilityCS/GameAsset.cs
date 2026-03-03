namespace ClientSideWASM;
using Microsoft.AspNetCore.Components;

public class GameAsset {
    public string Name { get; set; }
    private string[] _frameRefs;
    public string[] FrameRefs { 
        get => _frameRefs; 
        set {
            _frameRefs = value;
            // IMPORTANT: Initialize the Frames array so Blazor has a place to put the refs
            Frames = new ElementReference[value.Length];
        } 
    }
    public ElementReference[] Frames { get; set; }
    public int GlobalStartIndex { get; set; } // We will use this for the math fix below
}