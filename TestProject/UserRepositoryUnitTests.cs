
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Entities;
using DTO_s;


namespace Repositories.Tests
{
    public class UserRepositoryUnitTests
    {
        private readonly Mock<ShopContext> _mockContext;
        private readonly IUserRepository _repository;

        public UserRepositoryUnitTests()
        {
            // Create an in-memory database context using EF InMemory
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ShopContext(options);

            _repository = new UserRepository(dbContext);
        }

        [Fact]
        public async Task GetById_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, Email = "user1@test.com", Password = "password1" };
            await _repository.AddUser(user);

            // Act
            var result = await _repository.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("user1@test.com", result.Email);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.GetById(999); // ID that doesn't exist

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUser_ReturnsUser_WhenUserIsAdded()
        {
            // Arrange
            var user = new User { Email = "newuser@test.com", Password = "newpassword" };

            // Act
            var result = await _repository.AddUser(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("newuser@test.com", result.Email);
            Assert.Equal("newpassword", result.Password);
        }

        [Fact]
        public async Task FindUser_ReturnsUser_WhenEmailMatches()
        {
            // Arrange
            var user = new User { Email = "user1@test.com", Password = "hashedpassword" };
            await _repository.AddUser(user);

            // Act
            var result = await _repository.FindUser("user1@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1@test.com", result.Email);
        }

        [Fact]
        public async Task FindUser_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var user = new User { Email = "user1@test.com", Password = "hashedpassword" };
            await _repository.AddUser(user);

            // Act
            var result = await _repository.FindUser("unknown@test.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUser_UpdatesUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = 1, Email = "user1@test.com", Password = "password1" };
            await _repository.AddUser(user);

            user.Password = "newpassword";

            // Act
            await _repository.UpdateUser(user);
            var updatedUser = await _repository.GetById(1);

            // Assert
            Assert.NotNull(updatedUser);
            Assert.Equal("newpassword", updatedUser.Password);
        }
    }
}


