using Infrastructure.Services.Window.Core;

namespace Infrastructure.Extensions
{
    public static class WindowServiceExtensions
    {
        public static bool IsLoadingAnyWindowIncludingParents(this IWindowService windowService)
        {
            if (windowService.IsLoadingAnyWindow || windowService.IsLoadingAnyWindowInParents())
                return true;

            return false;
        }

        public static bool IsLoadingAnyWindowInParents(this IWindowService windowService)
        {
            if (windowService.Parent == null)
                return false;

            if (windowService.Parent.IsLoadingAnyWindow)
                return true;

            return windowService.Parent.IsLoadingAnyWindowInParents();
        }

        public static IWindow GetTopWindowIncludingParents(this IWindowService windowService)
        {
            if (windowService.HasAnyWindowInParents())
                return windowService.Parent.GetTopWindowIncludingParents();

            return windowService.GetTopWindow();
        }

        public static bool TryFindIncludingParents(this IWindowService windowService, WindowID windowID, out IWindow window)
        {
            if (windowService.TryFind(windowID, out window))
                return true;

            if (windowService.Parent == null)
            {
                window = null;
                return false;
            }

            return windowService.Parent.TryFindIncludingParents(windowID, out window);
        }

        public static IWindowService GetRootWindowService(this IWindowService windowService)
        {
            if (windowService.Parent == null)
                return windowService;

            return windowService.Parent.GetRootWindowService();
        }

        public static bool HasAnyWindowInParents(this IWindowService windowService)
        {
            if (windowService.Parent == null)
                return false;

            if (windowService.Parent.GetTopWindow() != null)
                return true;

            return windowService.Parent.HasAnyWindowInParents();
        }
    }
}