using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace UnityMoq
{
    public class CustomFactoryBuildStrategy : BuilderStrategy
    {
        private readonly MockFactory factory;


        public CustomFactoryBuildStrategy(MockFactory factory)
        {
            this.factory = factory;
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            if (context.Existing != null)
            {
                return;
            }

            var key = context.OriginalBuildKey;

            if (!factory.HasMock(key.Type) && !MockFactory.IsMockeable(key.Type))
            {
                return;
            }

            context.Existing = factory.ResolveInstance(key.Type, t =>
            {
                var buildKey = new NamedTypeBuildKey(t, null);
                return context.NewBuildUp(buildKey);
            });

            var ltm = new ContainerControlledLifetimeManager();
            ltm.SetValue(context.Existing);

            // Find the container to add this to
            IPolicyList parentPolicies;
            var parentMarker = context.Policies.Get<ParentMarkerPolicy>(
                new NamedTypeBuildKey<ParentMarkerPolicy>(), out parentPolicies);

            // Add lifetime manager to container
            parentPolicies.Set<ILifetimePolicy>(ltm, new NamedTypeBuildKey(key.Type));
            // And add to LifetimeContainer so it gets disposed
            parentMarker.AddToLifetime(ltm);

            // Short circuit the rest of the chain, object's already created
            context.BuildComplete = true;
        }
    }
}
