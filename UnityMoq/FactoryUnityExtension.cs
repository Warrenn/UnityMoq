using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace UnityMoq
{
    public class FactoryUnityExtension : UnityContainerExtension
    {
        private readonly MockFactory factory;

        private CustomFactoryBuildStrategy strategy;

        public FactoryUnityExtension(MockFactory factory)
        {
            this.factory = factory;
        }

        protected override void Initialize()
        {
            strategy = new CustomFactoryBuildStrategy(factory);
            Context.Strategies.Add(strategy, UnityBuildStage.PreCreation);
            Context.Policies.Set(new ParentMarkerPolicy(Context.Lifetime), new NamedTypeBuildKey<ParentMarkerPolicy>());
        }
    }
}
