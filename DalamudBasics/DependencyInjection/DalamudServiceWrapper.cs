using Dalamud.IoC;
using Dalamud.Plugin;

namespace DalamudBasics.DependencyInjection
{
    internal class DalamudServiceWrapper<T>
    {
        [PluginService]
        public T Service { get; private set; } = default!;

        internal DalamudServiceWrapper(IDalamudPluginInterface pi)
        {
            pi.Inject(this);
        }
    }
}
