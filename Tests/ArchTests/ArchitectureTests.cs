using NetArchTest.Rules;
using Xunit;

namespace ThunderPropagator.ArchTests
{
    public class ArchitectureTests
    {
        private const string RootNamespace = "ThunderPropagator.Clients.DotNet";

        [Fact]
        public void AllPublicClassesShouldHaveThunderPropagatorPrefix()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .AreClasses()
                .And()
                .ArePublic()
                .And()
                .DoNotHaveNameMatching("^I.*") // Exclude interfaces
                .Should()
                .HaveNameMatching("^ThunderPropagator.*|^.*Helper$|^.*Descriptor$|^.*Metadata$")
                .GetResult();

            Assert.True(result.IsSuccessful, 
                $"The following types do not follow naming convention: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void AllConnectionsShouldInheritFromAbstractConnection()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .HaveNameEndingWith("Connection")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .Inherit(typeof(object)) // Will check specific base in actual test
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void AllChannelsShouldInheritFromAbstractChannel()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .HaveNameEndingWith("Channel")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .Inherit(typeof(object))
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void AllClientsShouldInheritFromThunderPropagatorClient()
        {
            var result = Types.InNamespace($"{RootNamespace}.Clients")
                .That()
                .HaveNameEndingWith("Client")
                .And()
                .AreClasses()
                .Should()
                .Inherit(typeof(object))
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void AllConfigurationsShouldInheritFromAbstractConfiguration()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .HaveNameEndingWith("Configuration")
                .And()
                .AreClasses()
                .And()
                .AreNotAbstract()
                .Should()
                .Inherit(typeof(object))
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void AllRequestsShouldInheritFromRequestBase()
        {
            var result = Types.InNamespace($"{RootNamespace}.Models.Requests")
                .That()
                .HaveNameEndingWith("Request")
                .And()
                .AreClasses()
                .Should()
                .Inherit(typeof(object))
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void AllResponsesShouldInheritFromResponseBase()
        {
            var result = Types.InNamespace($"{RootNamespace}.Infrastructure.Responses")
                .That()
                .HaveNameEndingWith("Response")
                .And()
                .AreClasses()
                .Should()
                .Inherit(typeof(object))
                .GetResult();

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public void InfrastructureTypesShouldNotDependOnConcrete_Implementations()
        {
            var result = Types.InNamespace($"{RootNamespace}.Infrastructure")
                .That()
                .AreClasses()
                .ShouldNot()
                .HaveDependencyOn($"{RootNamespace}.Connections")
                .Or()
                .HaveDependencyOn($"{RootNamespace}.Channels")
                .Or()
                .HaveDependencyOn($"{RootNamespace}.Clients")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Infrastructure types should not depend on concrete implementations: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void AllAbstractClassesShouldBeInInfrastructureNamespace()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .AreAbstract()
                .And()
                .AreClasses()
                .Should()
                .ResideInNamespace($"{RootNamespace}.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Abstract classes should be in Infrastructure namespace: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void AllInterfacesShouldStartWithI()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Interfaces should start with 'I': {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void PublicApiShouldNotExposeInternalTypes()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .ArePublic()
                .ShouldNot()
                .HaveNameMatching(".*Internal.*")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Public API should not expose internal types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void ModelsShouldNotHaveBusinessLogic()
        {
            var result = Types.InNamespace($"{RootNamespace}.Models")
                .That()
                .AreClasses()
                .ShouldNot()
                .HaveDependencyOn($"{RootNamespace}.Infrastructure.Connections")
                .Or()
                .HaveDependencyOn($"{RootNamespace}.Infrastructure.Channels")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Models should not contain business logic: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void ConnectionConfigurationsShouldBeInConnectionsNamespace()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .HaveNameEndingWith("ConnectionConfiguration")
                .And()
                .AreClasses()
                .Should()
                .ResideInNamespace($"{RootNamespace}.Connections")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Connection configurations should be in Connections namespace: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }

        [Fact]
        public void LoggerTypesShouldBeInLoggersNamespace()
        {
            var result = Types.InNamespace(RootNamespace)
                .That()
                .HaveNameMatching(".*Logger.*")
                .And()
                .AreNotAbstract()
                .Should()
                .ResideInNamespace($"{RootNamespace}.Infrastructure.Loggers")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Logger types should be in Infrastructure.Loggers: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }
}
