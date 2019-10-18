using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Models;
using GuidDataCRUD.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Threading;

namespace GuidDataCRUD.Tests.Infrastructure
{
    /// <summary>
    /// Unit tests of <see cref="RedisGuidDataDecorator"/>
    /// </summary>
    public class RedisGuidDataDecoratorTests
    {
        private IDistributedCache _cacheMock;
        private IGuidDataRepository _dbMock;
        private IGuidDataRepository _repo;

        public RedisGuidDataDecoratorTests()
        {
            _cacheMock = Substitute.For<IDistributedCache>();
            _dbMock = Substitute.For<IGuidDataRepository>();

            _repo = new RedisGuidDataDecorator(_dbMock, _cacheMock);
        }

        
        [Fact]
        public async void Can_Get_From_Cache_After_Cached()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var raw = new GuidData{ Guid = guid, User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds() };
            var cached = JsonConvert.SerializeObject(raw);

            _cacheMock.GetAsync(Arg.Is<string>(x=>x== "GuidData_" + guidString)).Returns(Encoding.ASCII.GetBytes(cached));

            var actual = await _repo.GetGuidData(guid);

            Assert.NotNull(actual);
            Assert.Equal(guid, actual.Guid);

            await _dbMock.DidNotReceive().GetGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async void Can_Get_From_DB_Then_Set_To_Cache_Before_Cached()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var expected = new GuidData() { Guid = guid, User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds() };

            _cacheMock.GetAsync(Arg.Is<string>(x => x == "GuidData_" + guidString)).Returns(default(byte[]));
            _dbMock.GetGuidData(Arg.Is<Guid>(x => x == guid)).Returns(expected);

            var actual = await _repo.GetGuidData(guid);

            Assert.NotNull(actual);
            Assert.Equal(expected.Guid, actual.Guid);
            Assert.Equal(expected.User, actual.User);
            Assert.Equal(expected.Expire, actual.Expire);

            await _cacheMock.Received(1).GetAsync(Arg.Any<string>());
            await _cacheMock.Received(1).SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
            await _dbMock.Received(1).GetGuidData(Arg.Any<Guid>());
        }

        [Fact]
        public async void Can_Set_To_Cache_When_Created()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var expected = new GuidData() { Guid = guid, User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds() };

            _dbMock.CreateGuidData(Arg.Is<GuidData>(x => x.Guid == expected.Guid)).Returns(expected);

            var actual = await _repo.CreateGuidData(expected);

            Assert.NotNull(actual);
            Assert.Equal(expected.Guid, actual.Guid);
            Assert.Equal(expected.User, actual.User);
            Assert.Equal(expected.Expire, actual.Expire);

            await _dbMock.Received(1).CreateGuidData(Arg.Any<GuidData>());
            await _cacheMock.Received(1).SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
        }

        [Fact]
        public async void Can_Set_To_Cache_When_Upserted()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var expected = new GuidData() { Guid = guid, User = "test user", Expire = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds() };

            _dbMock.UpsertGuidData(Arg.Is<GuidData>(x => x.Guid == expected.Guid)).Returns((expected, true));

            var (actual,isUpdated) = await _repo.UpsertGuidData(expected);

            Assert.NotNull(actual);
            Assert.Equal(expected.Guid, actual.Guid);
            Assert.Equal(expected.User, actual.User);
            Assert.Equal(expected.Expire, actual.Expire);

            await _dbMock.Received(1).UpsertGuidData(Arg.Any<GuidData>());
            await _cacheMock.Received(1).SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
        }

        [Fact]
        public async void Can_Remove_From_Cache_When_Deleted()
        {
            var guid = Guid.NewGuid();
            var guidString = guid.ToString("N").ToUpper();
            var expected = true;

            _dbMock.DeleteGuidData(Arg.Is<Guid>(x => x == guid)).Returns(expected);

            var actual = await _repo.DeleteGuidData(guid);

            Assert.Equal(expected, actual);

            await _dbMock.Received(1).DeleteGuidData(Arg.Any<Guid>());
            await _cacheMock.Received(1).RemoveAsync(Arg.Any<string>());
        }

    }
}
