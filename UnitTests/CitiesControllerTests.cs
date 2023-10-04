using Eoranica.Controllers;
using Eoranica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Eoranica.UnitTests
{
    public class CitiesControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResultWithListOfCities()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var mockCities = new List<City>
            {
                new City { Id = 1, Name = "City 1" },
                new City { Id = 2, Name = "City 2" },
                new City { Id = 3, Name = "City 3" }
            };

            var mockDbSet = new Mock<DbSet<City>>();
            mockDbSet.As<IQueryable<City>>().Setup(m => m.Provider).Returns(mockCities.AsQueryable().Provider);
            mockDbSet.As<IQueryable<City>>().Setup(m => m.Expression).Returns(mockCities.AsQueryable().Expression);
            mockDbSet.As<IQueryable<City>>().Setup(m => m.ElementType).Returns(mockCities.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<City>>().Setup(m => m.GetEnumerator()).Returns(mockCities.AsQueryable().GetEnumerator());
            mockDbSet.As<IAsyncEnumerable<City>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<City>(mockCities.GetEnumerator()));

            mockContext.Setup(c => c.Cities).Returns(mockDbSet.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<City>>(viewResult.ViewData.Model);
            Assert.Equal(mockCities.Count, model.Count);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewResultWithCity()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var cityId = 1;
            var mockCity = new City { Id = cityId, Name = "City 1" };

            var mockDbSet = new Mock<DbSet<City>>();
            mockDbSet.Setup(m => m.FindAsync(cityId)).ReturnsAsync(mockCity);

            mockContext.Setup(c => c.Cities).Returns(mockDbSet.Object);

            // Act
            var result = await controller.Details(cityId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<City>(viewResult.ViewData.Model);
            Assert.Equal(mockCity, model);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<EoranicaContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var context = new EoranicaContext(options))
            {
                var controller = new CitiesController(context);

                var cityId = 1;

                var mockDbSet = new Mock<DbSet<City>>();
                mockDbSet.Setup(m => m.FindAsync(cityId)).ReturnsAsync((City)null);

                context.Cities = mockDbSet.Object;

                // Act
                var result = await controller.Details(cityId);

                // Assert
                Assert.IsType<NotFoundResult>(result);
            }
        }














        [Fact]
        public async Task Create_ReturnsViewResult()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var city = new City { Name = "Test City" }; // Create a sample City object

            // Act
            var result = await controller.Create(city);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CitiesController.Index), redirectResult.ActionName);
        }


        [Fact]
        public async Task Create_ValidCity_RedirectsToIndex()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var city = new City { Id = 1, Name = "City 1" };

            // Act
            var result = await controller.Create(city);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CitiesController.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_ValidId_ReturnsViewResultWithCity()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var cityId = 1;
            var mockCity = new City { Id = cityId, Name = "City 1", CountryId = 1 };

            var mockDbSet = new Mock<DbSet<City>>();
            mockDbSet.Setup(m => m.FindAsync(cityId)).ReturnsAsync(mockCity);

            mockContext.Setup(c => c.Cities).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.Countries).Returns(Mock.Of<DbSet<Country>>());

            // Act
            var result = await controller.Edit(cityId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<City>(viewResult.ViewData.Model);
            Assert.Equal(mockCity, model);
            Assert.NotNull(viewResult.ViewData["CountryId"]);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var cityId = 1;

            var mockDbSet = new Mock<DbSet<City>>();
            mockDbSet.Setup(m => m.FindAsync(cityId)).ReturnsAsync((City)null);

            mockContext.Setup(c => c.Cities).Returns(mockDbSet.Object);

            // Act
            var result = await controller.Edit(cityId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ValidId_RedirectsToIndex()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var cityId = 1;
            var mockCity = new City { Id = cityId, Name = "City 1" };

            var mockDbSet = new Mock<DbSet<City>>();
            mockDbSet.Setup(m => m.FindAsync(cityId)).ReturnsAsync(mockCity);

            mockContext.Setup(c => c.Cities).Returns(mockDbSet.Object);

            // Act
            var result = await controller.DeleteConfirmed(cityId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(CitiesController.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var mockOptions = new DbContextOptions<EoranicaContext>();
            var mockContext = new Mock<EoranicaContext>(mockOptions);
            var controller = new CitiesController(mockContext.Object);

            var cityId = 1;

            var mockDbSet = new Mock<DbSet<City>>();
            mockDbSet.Setup(m => m.FindAsync(cityId)).ReturnsAsync((City)null);

            mockContext.Setup(c => c.Cities).Returns(mockDbSet.Object);

            // Act
            var result = await controller.DeleteConfirmed(cityId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public TestAsyncEnumerator(IEnumerator<T> enumerator)
        {
            this.enumerator = enumerator;
        }

        public T Current => enumerator.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            enumerator.Dispose();
            return new ValueTask();
        }
    }
}