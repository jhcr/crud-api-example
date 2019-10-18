# GuidDataAPI
A Web API for saving guid and metadata to database and cache

## Getting Started

### Prerequisites
1) **Redis** need be installed:
```
C:\> choco install redis-64
```
and runing at:
```
localhost:6379
```
2) **Sql server database** should be depoyed first.

[GuidDataDB](https://github.com/jhcr/GuidDataDB) is the database project for publising tables and stored procedures to following database:
```
Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GuidDataDatabase
```
Please clone and deploy/publish it first.

### Runing the Web API

Runing [GuidDataAPI](https://github.com/jhcr/GuidDataAPI) project by following command:
```
C:\YouPathTo\GuidDataAPI\GuidDataCRUD.Web>dotnet run
```
After that, the web api should be runing at https://localhost:5001/swagger and http://localhost:5000/swagger
