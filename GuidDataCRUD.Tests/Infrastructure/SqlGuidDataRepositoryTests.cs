using GuidDataCRUD.Business;
using GuidDataCRUD.Business.Models;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using Microsoft.Extensions.Options;
using GuidDataCRUD.Infrastructure.Database;

namespace GuidDataCRUD.Tests.Infrastructure
{
    /// <summary>
    /// Unit tests of <see cref="SqlGuidDataRepository"/>
    /// </summary>
    public class SqlGuidDataRepositoryTests
    {
        private IOptions<AppSettings> _appSettingsMock;

        public SqlGuidDataRepositoryTests()
        {
            _appSettingsMock = Substitute.For<IOptions<AppSettings>>();
        }

        [Fact]
        public async void Can_Constructor_Throws_ArgumentNullException_When_No_AppSettings_Given()
        {
            _appSettingsMock.Value.ReturnsForAnyArgs(default(AppSettings));

            Assert.Throws<ArgumentNullException>(() => new SqlGuidDataRepository(_appSettingsMock));
        }

        [Fact]
        public async void Can_Constructor_Throws_ArgumentNullException_When_No_ConnectionString_Given()
        {
            _appSettingsMock.Value.ReturnsForAnyArgs(new AppSettings() { Sql_ConnectionString_GuidData = "" });

            Assert.Throws<ArgumentNullException>(() => new SqlGuidDataRepository(_appSettingsMock));
        }
    }
}
