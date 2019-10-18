using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Xunit;
using NSubstitute;
using Newtonsoft.Json;
using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Models;
using GuidDataCRUD.Business.Contracts;
using GuidDataCRUD.Infrastructure.Database;
using GuidDataCRUD.Business.Exceptions;
using GuidDataCRUD.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GuidDataCRUD.Tests.Web
{
    /// <summary>
    /// Unit Tests of <see cref="GuidDataController"/> 
    /// </summary>
    public class GuidDataControllerTest
    {
        private TestServer _server;
        private HttpClient _client;
        private readonly string _baseAddress = "http://unittestserver/";

        private ILogger<GuidDataService> _loggerProvMock;
        private ILogger<SqlGuidDataRepository> _loggerRepoMock;
        private IGuidDataRepository _repoMock;

        public GuidDataControllerTest()
        {
            var _testHostBuilder = GuidDataCRUD.Web.Program.CreateWebHostBuilder(new string[] { });

            _loggerProvMock = Substitute.For<ILogger<GuidDataService>>();
            _loggerRepoMock = Substitute.For<ILogger<SqlGuidDataRepository>>();
            _repoMock = Substitute.For<IGuidDataRepository>();

            _testHostBuilder.ConfigureTestServices(collection =>
            {
                collection.AddScoped(c => _loggerProvMock);
                collection.AddScoped(c => _loggerRepoMock);
                collection.AddScoped(c => _repoMock);
                collection.Configure<AppSettings>((settings)=> { settings.DefaultExpireDays = 30; });
            });

            _server = new TestServer(_testHostBuilder);
            _client = _server.CreateClient();
        }

        #region Get
        [Fact]
        public async Task Can_Get_Returns_200_OK_When_A_Valid_Guid_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.GetGuidData(Arg.Is<Guid>(x => x == guid)).Returns((new GuidData() { Guid = guid, User = raw.user, Expire = long.Parse(raw.expire) }));

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Get);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(raw.expire, actual.Expire);

            await _repoMock.Received(1).GetGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Get_Returns_400_BadRequest_When_No_Guid_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            var req = GetHttpRequestMessage(raw, $"guid/", HttpMethod.Get);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.Null(actual);

            await _repoMock.DidNotReceive().GetGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Get_Returns_400_BadRequest_When_Guid_Is_Not_Uppercased()
        {
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            var req = GetHttpRequestMessage(raw, $"guid/{Guid.NewGuid().ToString()}", HttpMethod.Get);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.True(actual.Errors.Count(o => o.Key == "guid") > 0);

            await _repoMock.DidNotReceive().GetGuidData(Arg.Any<Guid>());
        }
        #endregion Get

        #region Create
        [Fact]
        public async Task Can_Create_Returns_201_Creaeted_When_No_Guid_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.CreateGuidData(Arg.Any<GuidData>()).Returns(new GuidData() { Guid = guid, User = raw.user, Expire = long.Parse(raw.expire) });

            var req = GetHttpRequestMessage(raw, $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(raw.expire, actual.Expire);

            await _repoMock.Received(1).CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_201_Created_When_No_Expire_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user" };
            var defaultExpire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();

            _repoMock.CreateGuidData(Arg.Any<GuidData>()).Returns(new GuidData() { Guid = guid, User = raw.user, Expire = defaultExpire });

            var req = GetHttpRequestMessage(raw, $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(defaultExpire.ToString(), actual.Expire);

            await _repoMock.Received(1).CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_500_InternerServerError_When_Guid_Already_Exists()
        {
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.CreateGuidData(Arg.Any<GuidData>()).Returns<GuidData>(x=> { throw new ConflictResourceException(); });

            var req = GetHttpRequestMessage(raw, $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.InternalServerError, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);

            await _repoMock.Received(1).CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_400_BadRequest_When_Expire_Is_Not_A_Number()
        {
            var raw = new { user = "test user", expire = "uehfiuhw" };

            var req = GetHttpRequestMessage(raw, $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.NotNull(actual.Detail);
            Assert.True(actual.Errors.Count(o => o.Key == "expire" && o.Value[0].StartsWith("must be a number")) > 0);

            await _repoMock.DidNotReceive().CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_400_BadRequest_When_Expire_Exceeds_Epoc_Max()
        {
            var raw = new { user = "test user", expire = DateTimeOffset.MaxValue.ToUnixTimeSeconds() + 1 };

            var req = GetHttpRequestMessage(raw, $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.NotNull(actual.Detail);
            Assert.True(actual.Errors.Count(o => o.Key == "expire" && o.Value[0].StartsWith("must not exceed")) > 0);

            await _repoMock.DidNotReceive().CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_400_BadRequest_When_User_Length_Exceeds_255()
        {
            var raw = new { user = new string('*', 256), expire = DateTimeOffset.MaxValue.ToUnixTimeSeconds() };

            var req = GetHttpRequestMessage(raw, $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.NotNull(actual.Detail);
            Assert.True(actual.Errors.Count(o => o.Key == "user" && o.Value[0].StartsWith("must not exceed")) > 0);

            await _repoMock.DidNotReceive().CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_400_BadRequest_When_Request_Json_Is_Malformed()
        {
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            var req = GetMalformedRequestMessage("malformed test", $"guid", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            Assert.NotNull(await resp.Content.ReadAsStringAsync());

            await _repoMock.DidNotReceive().CreateGuidData(Arg.Any<GuidData>());
        }
        #endregion Create

        #region Upsert
        [Fact]
        public async Task Can_Upsert_When_No_Expire_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user" };
            var defaultExpire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();

            _repoMock.UpsertGuidData(Arg.Any<GuidData>()).Returns((new GuidData() { Guid = guid, User = raw.user, Expire = defaultExpire }, false));

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(defaultExpire.ToString(), actual.Expire);

            await _repoMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_201_Created_When_It_Does_A_Create()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user11", expire = DateTimeOffset.UtcNow.AddDays(12).ToUnixTimeSeconds().ToString() };

            _repoMock.UpsertGuidData(Arg.Any<GuidData>()).Returns((new GuidData() { Guid = guid, User = raw.user, Expire = long.Parse(raw.expire) }, false));

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Post);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(raw.expire, actual.Expire);

            await _repoMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_200_OK_When_It_Does_A_Update()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user11", expire = DateTimeOffset.UtcNow.AddDays(12).ToUnixTimeSeconds().ToString() };

            _repoMock.UpsertGuidData(Arg.Any<GuidData>()).Returns((new GuidData() { Guid = guid, User = raw.user, Expire = long.Parse(raw.expire) }, true));

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Post);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(raw.expire, actual.Expire);

            await _repoMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_400_BadRequest_When_Expire_Exceeds_Epoc_Max()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.MaxValue.ToUnixTimeSeconds() + 1 };

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.NotNull(actual.Detail);
            Assert.True(actual.Errors.Count(o => o.Key == "expire" && o.Value[0].StartsWith("must not exceed")) > 0);

            await _repoMock.DidNotReceive().UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_400_BadRequest_When_User_Length_Exceeds_255()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = new string('*', 256), expire = DateTimeOffset.MaxValue.ToUnixTimeSeconds() };

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.NotNull(actual.Detail);
            Assert.True(actual.Errors.Count(o => o.Key == "user" && o.Value[0].StartsWith("must not exceed")) > 0);

            await _repoMock.DidNotReceive().UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_400_BadRequest_When_Expire_Is_Not_A_Number()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = "uehfiuhw" };

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.NotNull(actual.Detail);
            Assert.True(actual.Errors.Count(o => o.Key == "expire" && o.Value[0].StartsWith("must be a number")) > 0);

            await _repoMock.DidNotReceive().UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_400_BadRequest_When_Request_Json_Is_Malformed()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            var req = GetMalformedRequestMessage("malformed test", $"guid/{guidString}", HttpMethod.Post);
            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            Assert.NotNull(await resp.Content.ReadAsStringAsync());

            await _repoMock.DidNotReceive().UpsertGuidData(Arg.Any<GuidData>());
        }
        #endregion Upsert

        #region Delete
        [Fact]
        public async Task Can_Delete_Returns_204_NoContent_When_Delete_Succeeds()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.DeleteGuidData(Arg.Is<Guid>(x => x == guid)).Returns(true);

            var req = GetHttpRequestMessage(raw, $"guid/{guidString}", HttpMethod.Delete);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<GuidDataResponse>(await resp.Content.ReadAsStringAsync());
            Assert.Empty(await resp.Content.ReadAsStringAsync());

            await _repoMock.Received(1).DeleteGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Delete_Returns_400_BadRequest_When_Guid_Format_Is_Invalid()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();

            var req = GetHttpRequestMessage("", $"guid/treett", HttpMethod.Delete);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var actual = JsonConvert.DeserializeObject<ValidationProblemDetails>(await resp.Content.ReadAsStringAsync());
            Assert.NotNull(actual);
            Assert.True(actual.Errors.Count(o=>o.Key == "guid" && o.Value[0].StartsWith("must be 32-bit hexadecimal in uppercase")) > 0);

            await _repoMock.DidNotReceive().DeleteGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Delete_Returns_404_NotFoound_When_Guid_Not_Exist()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();

            var req = GetHttpRequestMessage("", $"guid/{guidString}", HttpMethod.Delete);

            _repoMock.DeleteGuidData(Arg.Is<Guid>(x => x == guid)).Returns(false);

            var resp = await _client.SendAsync(req);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

            await _repoMock.Received(1).DeleteGuidData(Arg.Any<Guid>());
        }
        #endregion Delete

        private HttpRequestMessage GetHttpRequestMessage<TRequest>(TRequest request, string actionName, HttpMethod method = null)
        {
            method = method ?? HttpMethod.Post;
            var requestMessage = new HttpRequestMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_baseAddress + actionName),
                Method = method
            };

            return requestMessage;
        }

        private HttpRequestMessage GetMalformedRequestMessage(string malformedJson, string actionName, HttpMethod method = null)
        {
            method = method ?? HttpMethod.Post;
            var requestMessage = new HttpRequestMessage
            {
                Content = new StringContent(malformedJson, Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_baseAddress + actionName),
                Method = method
            };

            return requestMessage;
        }


    }
}
