using BlazorApp.Client;

public class ClientRenderModeContext : IRenderModeExplainer
{
    public RenderMode GetMode() =>RenderMode.Client;
}
