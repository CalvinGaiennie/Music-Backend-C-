using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;

namespace Music.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class AudioFileController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public AudioFileController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("AudioFiles/{fileId}/{userId}/{searchParam}")]
        public IEnumerable<AudioFile> GetAudioFiles(int fileId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC dbo.spAudioFile_Get";
            string stringParameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (fileId != 0)
            {
                stringParameters += ", @AudioFileId = @AudioFileIdParameter";
                sqlParameters.Add("@AudioFileIdParameter", fileId, DbType.Int32);
            }

            if (userId != 0)
            {
                stringParameters += ", @UserId = @UserIdParameter";
                sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);
            }

            if (searchParam != "None" && searchParam.ToLower() != "none")
            {
                stringParameters += ", @SearchValue = @SearchValueParameter";
                sqlParameters.Add("@SearchValueParameter", searchParam, DbType.String);
            }

            if (stringParameters.Length > 0)
            {
                sql += stringParameters.Substring(1);
            }

            return _dapper.LoadDataWithParameters<AudioFile>(sql, sqlParameters);
        }

        [HttpPut("UpsertAudioFile")]
        public IActionResult UpsertAudioFile(AudioFile audioFileToUpsert)
        {
            string sql = @"EXEC dbo.spAudioFile_Upsert
            @UserId = UserIdParameter,
            @AudioFileName = @AudioFileNameParameter,
            @AudioFileContent = @AudioFileContentParameter
            @AudioFileId = @AudioFileIdParameter";
            //this needs to be built out similar to the post one from course
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@AudioFileNameParameter", audioFileToUpsert.FileName, DbType.String);
            sqlParameters.Add("@AudioFileContentParameter", audioFileToUpsert.FileData, DbType.Binary);

            if (audioFileToUpsert.AudioFileId > 0)
            {
                sql += ", @AudioFileId = @AudioFileIdParameter";
                sqlParameters.Add("@AudioFileIdParameter", audioFileToUpsert.AudioFileId, DbType.Int32);
            }

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert audio file");
        }


        [HttpDelete("AudioFile/{audioFileId}")]
        public IActionResult DeleteAudioFile(int audioFileId)
        {
            string sql = @"EXEC dbo.spAudioFile_Delete @AudioFileId = " + audioFileId.ToString() + ", @UserId = " + this.User.FindFirst("userId")?.Value;

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to delete audio file");
        }
    }
}