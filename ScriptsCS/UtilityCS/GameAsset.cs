using Microsoft.AspNetCore.Components;

public class GameAsset {
    public string Name { get; set; }
    public string Url { get; set; }
    public ElementReference Image { get; set; } // Blazor will fill this
}
