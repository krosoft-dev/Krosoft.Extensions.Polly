using System.Net;
using Krosoft.Extensions.Resilience.Extensions;
using Krosoft.Extensions.Resilience.Tests.Core;
using Krosoft.Extensions.Testing;
using Krosoft.Extensions.Testing.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq.Protected;

namespace Krosoft.Extensions.Resilience.Tests.Extensions;

[TestClass]
public class HttpClientBuilderExtensionsTests : BaseTest
{
    private TestHttpService _testHttpService = null!;

    protected override void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<TestHttpService>();
        services.AddHttpClient("sitemap")
                .AddPolicyHandlers(configuration, services)
                .AddHttpMessageHandler(() => new MockHttpMessageHandler());
    }

    [TestInitialize]
    public void SetUp()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            // Setup the PROTECTED method to mock
            .Setup<Task<HttpResponseMessage>>(
                                              "SendAsync",
                                              ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>()
                                             )
            // prepare the expected response of the mocked http call
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[{'id':1,'value':'1'}]")
            })
            .Verifiable();

        // use real http client with mocked handler here
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://test.com/")
        };

        //Arrange
        var mockFactory = new Mock<IHttpClientFactory>();

        mockFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var serviceProvider = CreateServiceCollection(services =>
        {
            //var mockDateTimeService = new Mock<IDateTimeService>();
            //mockDateTimeService.Setup(x => x.Now)
            //                   .Returns(new DateTime(2012, 1, 3));
            services.SwapTransient(_ => mockFactory);
        });

        _testHttpService = serviceProvider.GetRequiredService<TestHttpService>();
    }

    [TestMethod]
    public async Task VerifyOkTest()
    {
        var result = await _testHttpService.GetAsync(CancellationToken.None);

//            //Assert.Equal(HttpStatusCode.OK, result.StatusCode);

//            _mock.Verify(x => x.Send(It.IsAny<PaiementsQuery>(), CancellationToken.None), Times.Once());

//            _mock.Setup(x => x.Send(It.IsAny<PaiementsQuery>(), CancellationToken.None))
//                 .ReturnsAsync(() => new List<PaiementDto>
//                 {
//                     new PaiementDto { Id = new Guid("e89e75f8-cc74-4968-b609-f7ceb7dc53f7") },
//                     new PaiementDto { Id = new Guid("51401e09-4bf6-4918-b554-c406e16daebf") }
//                 });

//            var result = await _controller.GetPaiementsAsync(new PaiementsQuery());

//            _mock.Verify(x => x.Send(It.IsAny<PaiementsQuery>(), CancellationToken.None), Times.Once());

        Check.That(result).IsNotNull();
//        }
//    }
//}using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using NFluent;
//using Positive.Platform.Paiements.Api.Controllers;
//using Positive.Platform.Paiements.Api.Models.Dto;
//using Positive.Platform.Paiements.Api.Models.Queries;

//namespace Positive.Platform.Paiements.Api.Tests.Unit.Controllers
//{
//    [TestClass]
//    public class PaiementsControllerTests
//    {
//        private PaiementsController _controller;
//        private Mock<IMediator> _mock;

//        [TestInitialize]
//        public void SetUp()
//        {
//            _mock = new Mock<IMediator>();
//            var serviceProviderMock = new Mock<IServiceProvider>();
//            serviceProviderMock
//                .Setup(_ => _.GetService(typeof(IMediator)))
//                .Returns(_mock.Object);

//            _controller = new PaiementsController
//            {
//                ControllerContext = new ControllerContext
//                {
//                    HttpContext = new DefaultHttpContext { RequestServices = serviceProviderMock.Object }
//                }
//            };
//        }

//        [TestMethod]
//        public async Task GetPaiementsAsyncTest()
//        {
//            _mock.Setup(x => x.Send(It.IsAny<PaiementsQuery>(), CancellationToken.None))
//                 .ReturnsAsync(() => new List<PaiementDto>
//                 {
//                     new PaiementDto { Id = new Guid("e89e75f8-cc74-4968-b609-f7ceb7dc53f7") },
//                     new PaiementDto { Id = new Guid("51401e09-4bf6-4918-b554-c406e16daebf") }
//                 });

//            var result = await _controller.GetPaiementsAsync(new PaiementsQuery());

//            _mock.Verify(x => x.Send(It.IsAny<PaiementsQuery>(), CancellationToken.None), Times.Once());
//            Check.That(result).IsNotNull();
//            Check.That(result).HasSize(2);
//            Check.That(result.Select(r => r.Id.ToString())).ContainsExactly("e89e75f8-cc74-4968-b609-f7ceb7dc53f7", "51401e09-4bf6-4918-b554-c406e16daebf");
//        }
//    }
//}
    }
}