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
    public class AudioTrackController : ControllerBase
    {
        private readonly DataContextDapper _dapper;

        public AudioTrackController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("GetAudioTracks/{trackId}/{userId}/{searchParam}")]
        public IEnumerable<AudioTrack> GetAudioTracks(int trackId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC dbo.spAudioTracks_Get";
            string stringParameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (trackId != 0)
            {
                stringParameters += ", @AudioTrackId = @AudioTrackIdParameter";
                sqlParameters.Add("@AudioTrackIdParameter", trackId, DbType.Int32);
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

            return _dapper.LoadDataWithParameters<AudioTrack>(sql, sqlParameters);
        }

        [HttpGet("GetMyAudioTracks")]
        public IEnumerable<AudioTrack> GetMyAudioTracks()
        {
            string sql = @"EXEC dbo.spAudioTracks_Get @UserId = @UserIdParameter";
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            return _dapper.LoadDataWithParameters<AudioTrack>(sql, sqlParameters);
        }

        [HttpPut("UpsertAudioTrack")]
        public IActionResult UpsertAudioTrack(AudioTrack audioTrackToUpsert)
        {
            string sql = @"EXEC dbo.spAudioTracks_Upsert
            @UserId = @UserIdParameter,
            @SongName = @SongNameParameter,
            @SongTip = @SongTipParameter,
            @SongKey = @SongKeyParameter,
            @SongChords = @SongChordsParameter,
            @SongInstrument = @SongInstrumentParameter,
            @SongDifficulty = @SongDifficultyParameter,
            @SongData = @SongDataParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@SongNameParameter", audioTrackToUpsert.SongName, DbType.String);
            sqlParameters.Add("@SongTipParameter", audioTrackToUpsert.SongTip, DbType.String);
            sqlParameters.Add("@SongKeyParameter", audioTrackToUpsert.SongKey, DbType.String);
            sqlParameters.Add("@SongChordsParameter", audioTrackToUpsert.SongChords, DbType.String);
            sqlParameters.Add("@SongInstrumentParameter", audioTrackToUpsert.SongInstrument, DbType.String);
            sqlParameters.Add("@SongDifficultyParameter", audioTrackToUpsert.SongDifficulty, DbType.String);
            sqlParameters.Add("@SongDataParameter", audioTrackToUpsert.SongData, DbType.Binary);

            if (request.AudioTrackId > 0)
            {
                sql += ", @AudioTrackId = @AudioTrackIdParameter";
                sqlParameters.Add("@AudioTrackIdParameter", request.AudioTrackId, DbType.Int32);
            }

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert audio track");
        }

        [HttpDelete("DeleteAudioTrack/{audioTrackId}")]
        public IActionResult DeleteAudioTrack(int audioTrackId)
        {
            string sql = @"EXEC dbo.spAudioTrack_Delete @AudioTrackId = @AudioTrackIdParameter, @UserId = @UserIdParameter";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@AudioTrackIdParameter", audioTrackId, DbType.Int32);
            sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);

            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }
            throw new Exception("Failed to delete audio track");
        }
    }
}