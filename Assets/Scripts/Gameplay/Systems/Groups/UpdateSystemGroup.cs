using Unity.Entities;

namespace Gameplay.Systems.Groups
{
    [DisableAutoCreation]
    public partial class UpdateSystemGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            EnableSystemSorting = false;
        }
    }
}