using Microsoft.AspNetCore.Components;

namespace Client8.Components;

public partial class MyImage
{
    [Parameter]
    public string? Base64Image { get; set; }

    [Parameter]
    public string? ImageName { get; set; }

    public bool IsDefined => Base64Image != null & ImageName != null;
}
