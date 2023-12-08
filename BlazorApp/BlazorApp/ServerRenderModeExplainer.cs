using BlazorApp.Client;

namespace BlazorApp;

public class ServerRenderModeExplainer(IHttpContextAccessor accessor) : IRenderModeExplainer
{
    RenderMode IRenderModeExplainer.GetMode()
    {
        var prerendering = !accessor.HttpContext?.Response.HasStarted ?? false;
        if(prerendering)
        {
            return RenderMode.Prerender;
        } else
        {
            return RenderMode.Server;
        }

    }
}
