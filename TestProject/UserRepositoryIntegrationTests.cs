using DTO_s;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Threading.Tasks;
using Xunit;

namespace Repositories.Tests
{
    public class UserRepositoryIntegrationTests
    {
        private ShopContext _context;
        private UserRepository _repository;

        // SetUp: Initialize the in-memory database and repository before each test
        public UserRepositoryIntegrationTests()
        {
            // Create a new in-memory database for each test
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // Unique name for each test run
                .Options;

            _context = new ShopContext(options);  // Create a fresh context
            _repository = new UserRepository(_context);  // Inject it into the repository
        }

        // TearDown: Clean up the in-memory database after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();  // Deletes the database after each test
            _context.Dispose();  // Disposes the context
        }

        // Test for AddUser method
        [Fact]
        public async Task AddUser_ShouldAddUser()
        {
            // Arrange
            var user = new User { Email = "testuser@test.com", Password = "password123" };

            // Act
            var addedUser = await _repository.AddUser(user);

            // Assert
            Assert.NotNull(addedUser);
            Assert.Equal("testuser@test.com", addedUser.Email);
            Assert.Equal("password123", addedUser.Password);

            // Verify user is added to the database
            var dbUser = await _context.Users.FindAsync(addedUser.Id);
            Assert.NotNull(dbUser);
            Assert.Equal("testuser@test.com", dbUser.Email);
        }

        // Test for GetById method
        [Fact]
        public async Task GetById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User { Email = "user1@test.com", Password = "password1" };
            await _repository.AddUser(user); // Add user to DB

            // Act
            var result = await _repository.GetById(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
        }

        // Test for GetById method when user does not exist
        [Fact]
        public async Task GetById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.GetById(999); // ID that does not exist

            // Assert
            Assert.Null(result);
        }

        // Test for FindUser method (successful login)
        [Fact]
        public async Task FindUser_ShouldReturnUser_WhenEmailMatches()
        {
            // Arrange
            var user = new User { Email = "user1@test.com", Password = "hashedpassword" };
            await _repository.AddUser(user); // Add user to DB

            // Act
            var result = await _repository.FindUser("user1@test.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1@test.com", result.Email);
        }

        // Test for FindUser method (unsuccessful login)
        [Fact]
        public async Task FindUser_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            // Arrange
            var user = new User { Email = "user1@test.com", Password = "hashedpassword" };
            await _repository.AddUser(user); // Add user to DB

            // Act
            var result = await _repository.FindUser("unknown@test.com");

            // Assert
            Assert.Null(result);
        }

        // Test for UpdateUser method
        [Fact]
        public async Task UpdateUser_ShouldUpdateUser()
        {
            // Arrange
            var user = new User { Email = "user2@test.com", Password = "oldpassword" };
            await _repository.AddUser(user); // Add user to DB

            user.Password = "newpassword"; // Change password

            // Act
            await _repository.UpdateUser(user);

            // Assert
            var updatedUser = await _repository.GetById(user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal("newpassword", updatedUser.Password);
        }
    }
}
