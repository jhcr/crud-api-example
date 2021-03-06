﻿using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Contracts;
using GuidDataCRUD.Business.Models;
using GuidDataCRUD.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Options;

namespace GuidDataCRUD.Tests.Business
{
    /// <summary>
    /// Unit tests of <see cref="GuidDataService"/> 
    /// </summary>
    public class GuidDataServiceTests
    {
        private readonly ILogger<GuidDataService> _loggerProvMock;
        private readonly ILogger<SqlGuidDataRepository> _loggerRepoMock;
        private IGuidDataRepository _repoMock;
        private IGuidDataService _provider;
        private IOptions<AppSettings> _appSettings;

        public GuidDataServiceTests()
        {
            var _testHostBuilder = GuidDataCRUD.Web.Program.CreateWebHostBuilder(new string[] { });

            _loggerProvMock = Substitute.For<ILogger<GuidDataService>>();
            _loggerRepoMock = Substitute.For<ILogger<SqlGuidDataRepository>>();
            _repoMock = Substitute.For<IGuidDataRepository>();
            _appSettings = Options.Create(new AppSettings() { DefaultExpireDays = 30});

            _provider = new GuidDataService(_repoMock, _appSettings);

        }

        #region Get
        [Fact]
        public async Task Can_Get_Returns_GuidData_When_Guid_Exist()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new { user = "test user", expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.GetGuidData(Arg.Is<Guid>(x => x == guid)).Returns((new GuidData() { Guid = guid, User = raw.user, Expire = long.Parse(raw.expire) }));

            var actual = await _provider.GetGuidData(guidString);

            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(raw.user, actual.User);
            Assert.Equal(raw.expire, actual.Expire);

            await _repoMock.Received(1).GetGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Get_Returns_Null_When_Guid_Not_Exist()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();

            _repoMock.GetGuidData(Arg.Is<Guid>(x => x == guid)).Returns(default(GuidData));

            var actual = await _provider.GetGuidData(guidString);

            Assert.Null(actual);

            await _repoMock.Received(1).GetGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Get_Throws_ArgumentNullException_When_No_Guid_Given()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.GetGuidData(null));

            await _repoMock.Received(0).GetGuidData(Arg.Any<Guid>());
        }
        #endregion Get

        #region Create
        [Fact]
        public async Task Can_Create_Returns_GuidData_When_No_Guid_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString()};

            _repoMock.CreateGuidData(Arg.Any<GuidData>()).Returns(new GuidData() { Guid = guid, User = request.User, Expire = long.Parse(request.Expire) });

            var actual = await _provider.CreateGuidData(request);

            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(request.User, actual.User);
            Assert.Equal(request.Expire, actual.Expire);

            await _repoMock.Received(1).CreateGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Create_Returns_GuidData_Using_30_Days_Expire_When_No_Expire_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = null };

            long autoGeneratedExpire = 0;

            await _repoMock.CreateGuidData(Arg.Do<GuidData>(x => autoGeneratedExpire = x.Expire));

            var actual = await _provider.CreateGuidData(request);

            Assert.True(autoGeneratedExpire > 0);

            await _repoMock.Received(1).CreateGuidData(Arg.Any<GuidData>());
        }
        #endregion Create

        #region Upsert
        [Fact]
        public async Task Can_Upsert_Returns_GuidData_Using_30_Days_Expire_When_No_Expire_Given()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = null };

            long autoGeneratedExpire = 0;

            await _repoMock.UpsertGuidData(Arg.Do<GuidData>(x => autoGeneratedExpire = x.Expire));

            var actual = await _provider.UpsertGuidData(guidString, request);

            Assert.True(autoGeneratedExpire > 0);

            await _repoMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_Update_Flag_True_When_It_Does_A_Update()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.UpsertGuidData(Arg.Any<GuidData>()).Returns((new GuidData() { Guid = guid, User = request.User, Expire = long.Parse(request.Expire) }, true));

            var (actual,isUpdate) = await _provider.UpsertGuidData(guidString, request);

            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(request.User, actual.User);
            Assert.Equal(request.Expire, actual.Expire);
            Assert.True(isUpdate);

            await _repoMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Returns_Update_Flag_False_When_It_Does_A_Create()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.UpsertGuidData(Arg.Any<GuidData>()).Returns((new GuidData() { Guid = guid, User = request.User, Expire = long.Parse(request.Expire) }, false));

            var (actual, isUpdate) = await _provider.UpsertGuidData(guidString, request);

            Assert.NotNull(actual);
            Assert.Equal(guidString, actual.Guid);
            Assert.Equal(request.User, actual.User);
            Assert.Equal(request.Expire, actual.Expire);
            Assert.False(isUpdate);

            await _repoMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Throws_ArgumentNullException_When_No_Guid_Given()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.UpsertGuidData(null, new GuidDataRequest() { }));

            await _repoMock.Received(0).UpsertGuidData(Arg.Any<GuidData>());
        }

        [Fact]
        public async Task Can_Upsert_Throws_ArgumentNullException_When_No_GuidDataRequest_Given()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.UpsertGuidData(null, new GuidDataRequest() { }));

            await _repoMock.Received(0).UpsertGuidData(Arg.Any<GuidData>());
        }

        #endregion Upsert

        #region Delete
        [Fact]
        public async Task Can_Delete_Returns_True_When_Operation_Succeeds()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.DeleteGuidData(Arg.Any<Guid>()).Returns(true);

            var actual = await _provider.DeleteGuidData(guidString);

            Assert.True(actual);

            await _repoMock.Received(1).DeleteGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Delete_Returns_False_When_Guid_Not_Found()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var request = new GuidDataRequest() { User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds().ToString() };

            _repoMock.DeleteGuidData(Arg.Any<Guid>()).Returns(false);

            var actual = await _provider.DeleteGuidData(guidString);

            Assert.False(actual);

            await _repoMock.Received(1).DeleteGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async Task Can_Create_Throws_ArgumentNullException_When_No_GuidDataRequest_Given()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.CreateGuidData(null));

            await _repoMock.Received(0).CreateGuidData(Arg.Any<GuidData>());
        }
       
        [Fact]
        public async Task Can_Delete_Throws_ArgumentNullException_When_No_Guid_Given()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _provider.DeleteGuidData(null));

            await _repoMock.Received(0).DeleteGuidData(Arg.Any<Guid>());
        }
        #endregion Delete
    }
}
