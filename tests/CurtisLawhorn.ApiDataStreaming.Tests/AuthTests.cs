using System.Diagnostics.CodeAnalysis;
using CurtisLawhorn.Auth.Configuration;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CurtisLawhorn.Auth.Tests
{
    [ExcludeFromCodeCoverage]
    public class AuthTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly CognitoConfiguration _config;
        
        public AuthTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _config = new CognitoConfiguration { UserPoolId = "TestPool" };
        }
        
        [Fact]
        public void Constructor_SetsCognitoConfigurationUserPoolId()
        {
            // Arrange
            var authTests = new AuthTests();

            // Act
            var userPoolId = authTests._config.UserPoolId;

            // Assert
            Assert.Equal("TestPool", userPoolId);
        }

    }
}