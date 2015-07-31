using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;

namespace UnityMoq
{
    public class MockFactory : IDisposable
    {
        private readonly IDictionary<Guid, MethodInfo> genericMethods = new SortedDictionary<Guid, MethodInfo>();
        private static readonly MethodInfo ResolveMethodInfo = typeof(MockFactory).GetMethod("ResolveMock");
        private readonly IDictionary<Guid, Mock> mockCache = new SortedDictionary<Guid, Mock>();


        public static bool IsMockeable(Type typeToMock)
        {
            if (typeToMock.IsInterface ||
                typeToMock.IsAbstract ||
                typeToMock.IsSubclassOf(typeof (Delegate)))
            {
                return true;
            }
            if (typeToMock.IsClass)
            {
                return !typeToMock.IsSealed;
            }
            return false;
        }

        private object FactoryMethod(Type type)
        {
            if (IsMockeable(type))
            {
                return ResolveInstance(type);
            }
            var parameters = Parameters(type);
            return (parameters == null) ? Activator.CreateInstance(type) : Activator.CreateInstance(type, parameters);
        }

        public void AddMock<T>(Mock<T> mock) where T : class
        {
            var type = typeof (T);
            mockCache[type.GUID] = mock;
        }
        
        public bool HasMock<T>()
        {
            var type = typeof(T);
            return HasMock(type);
        }
        
        public bool HasMock(Type type)
        {
            return mockCache.ContainsKey(type.GUID);
        }

        public object ResolveInstance(Type type, Func<Type, object> factory = null)
        {
            var typeGuid = type.GUID;
            var genericMethod = (genericMethods.ContainsKey(typeGuid))
                ? genericMethods[typeGuid]
                : ResolveMethodInfo.MakeGenericMethod(type);
            genericMethods[typeGuid] = genericMethod;

            var mock = (Mock) genericMethod.Invoke(this, new object[] {factory});
            return mock.Object;
        }

        public Mock<T> ResolveMock<T>(Func<Type, object> factory = null) where T : class
        {
            var type = typeof (T);
            var typeGuid = type.GUID;

            if (mockCache.ContainsKey(typeGuid))
            {
                return (Mock<T>)mockCache[typeGuid];
            }

            var parameters = Parameters(type, factory);

            var mock = (parameters == null)
                ? new Mock<T>(MockBehavior.Loose)
                : new Mock<T>(MockBehavior.Loose, parameters) {CallBase = true};


            mockCache.Add(typeGuid, mock);
            return mock;
        }

        private object[] Parameters(Type type, Func<Type, object> factory = null)
        {
            var ctors =
                type.GetConstructors(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.OptionalParamBinding |
                    BindingFlags.Instance);

            if (ctors.Count(info => info.GetParameters().Length > 0) <= 0) return null;

            factory = factory ?? FactoryMethod;
            var constuctorInfo = ctors[0];
            return constuctorInfo
                .GetParameters()
                .Select(parameterInfo => factory(parameterInfo.ParameterType))
                .ToArray();
        }

        public void Dispose()
        {
            mockCache.Clear();
            genericMethods.Clear();
        }
    }
}
