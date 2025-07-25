using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Music.Data;
using Music.Models;
using System.Data;

namespace Music.Controllers;
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }
    [HttpGet("TestConnection")]
    public IActionResult TestConnection()
    {
        return Ok(new
        {
            success = true,
            message = "API is running successfully!",
            timestamp = DateTime.UtcNow,
            environment = "Azure"
        });
    }

    [HttpGet("TestDatabase")]
    public DateTime TestDatabase()
    {
        return _dapper.LoadDataSingleWithParameters<DateTime>("SELECT GETDATE()", new DynamicParameters());
    }
    [HttpGet("GetUsers/{userId}/{active}")]
    public IEnumerable<User> GetUsers(int userId, bool active)
    {
        string sql = @"EXEC dbo.spUsers_Get";
        string stringParameters = "";
        DynamicParameters sqlParameters = new DynamicParameters();

        if (userId != 0)
        {
            stringParameters += ", @UserId = @UserId";
            sqlParameters.Add("@UserId", userId, DbType.Int32);
        }
        if (active)
        {
            stringParameters += ", @Active = @Active";
            sqlParameters.Add("@Active", active, DbType.Boolean);
        }
        if (stringParameters.Length > 0)
        {
            sql += stringParameters.Substring(1);
        }

        IEnumerable<User> users = _dapper.LoadDataWithParameters<User>(sql, sqlParameters);
        return users;
    }
}