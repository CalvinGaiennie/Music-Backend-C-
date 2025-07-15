using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Music.Data;
using DotnetApi.Models;
using System.Data;

namespace DotnetApi.Controllers;
public class UserController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }
    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingleWithParameters<DateTime>("SELECT GETDATE()", new DynamicParameters());
    }
    [HttpGet("GetUsers/{userId}")]
    public IEnumerable<User> GetUsers(int userId, bool active)
    {
        string sql = "";
        string stringParameters = "";
        DynamicParameters sqlParameters = new DynamicParameters();

        if (userId != 0)
        {
            stringParameters += ", @UserId = UserIdParameter";
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
        }
        if (active)
        {
            stringParameters += ", @Active = @ActiveParameter";
            sqlParameters.Add("@ActiveParameter", active, DbType.Boolean);
        }
        if (stringParameters.Length > 0)
        {
            sql += stringParameters.Substring(1);
        }

        IEnumerable<User> users = _dapper.LoadDataWithParameters<User>(sql, sqlParameters);
        return users;
    }
}