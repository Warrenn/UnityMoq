# UnityMoq
Library that eases the use of dependency injection for unit tests. If a dependency is required it will create a mock object automatically through the use of Unity an MOQ.

Suppose you have a controller with more than one dependancy that needs to be injected but for the sake of you unit test you are only intrested in mocking one service. You can use a MockHelper class to configure just the service you are intrested in and let any other services get resolved through Unity and Moq. 

        public interface IService2
        {
            int TestResult2();
        }

        public interface IService
        {
            int TestResult(int input);
        }
        
        public class Controller
        {
            private readonly IService service;
            private readonly IService2 service2;

            public Controller(IService service, IService2 service2)
            {
                this.service = service;
                this.service2 = service2;
            }

            public virtual int TestMethod(int input)
            {
                return service.TestResult(input);
            }

            public virtual int Service3Test()
            {
                return service2.TestResult2();
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            var helper = new MockHelper();
            var serviceMock = helper.Mock<IService>();
            serviceMock
                .Setup(service => service.TestResult(It.Is<int>(v => v == 8)))
                .Returns(234)
                .Verifiable();
          
            //Any dependencies are automatically resolved
            var sut = helper.Instance<Controller>();

            //Act
            var result = sut.TestMethod(8);

            //Assert
            serviceMock.VerifyAll();
            Assert.AreEqual(234, result);
        }
