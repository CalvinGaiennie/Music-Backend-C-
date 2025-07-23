using System.Data;
using Dapper;
using Music.Data;
using Music.Models;

namespace Music.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }
        public bool UpsertUser(User user)
        {
            string sql = @"EXEC dbo.spUser_Upsert
            @FirstName = @FirstNameParameter, 
            @LastName = @LastNameParameter,
            @Email = @EmailParameter,
            @Active = @ActiveParameter,
            @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();

            sqlParameters.Add("@FirstNameParameter", user.FirstName, DbType.String);
            sqlParameters.Add("@LastNameParameter", user.LastName, DbType.String);
            sqlParameters.Add("@EmailParameter", user.Email, DbType.String);
            sqlParameters.Add("@ActiveParameter", user.Active, DbType.Boolean);
            sqlParameters.Add("@UserIdParameter", user.UserId, DbType.Int32);

            return _dapper.ExecuteSqlWithParameters(sql, sqlParameters);
        }
    }
}