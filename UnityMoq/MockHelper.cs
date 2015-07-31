using System;
using Microsoft.Practices.Unity;
using Moq;

namespace UnityMoq
{
    public class MockHelper : IDisposable
    {
        private readonly IUnityContainer container;
        private readonly UnityMoq.MockFactory factory;

        public MockHelper()
        {
            factory = new UnityMoq.MockFactory();
            container = new UnityContainer();
            container.AddExtension(new FactoryUnityExtension(factory));
        }

        public void AddMock<T>(Mock<T> mock) where T : class
        {
            factory.AddMock(mock);
        }

        public Mock<T> Mock<T>() where T : class
        {
            return factory.ResolveMock<T>(type => container.Resolve(type));
        }

        public T Instance<T>()
        {
            return container.Resolve<T>();
        }

        public void Dispose()
        {
            container.Dispose();
            factory.Dispose();
        }
    }
}
