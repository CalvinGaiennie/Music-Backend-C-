using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Music.Data;
using Music.Models;
using Music.Dtos;
using Music.Services;

namespace Music.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AudioTrackController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IBlobStorageService _blobStorageService;

        public AudioTrackController(IConfiguration config, IBlobStorageService blobStorageService)
        {
            _dapper = new DataContextDapper(config);
            _blobStorageService = blobStorageService;
        }

        [HttpGet("GetAudioTracks/{trackId}/{userId}/{searchParam}")]
        public IEnumerable<AudioTrack> GetAudioTracks(int trackId = 0, int userId = 0, string searchParam = "None")
        {
            Console.WriteLine($"[GET] GetAudioTracks called with trackId={trackId}, userId={userId}, searchParam='{searchParam}'");

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

            Console.WriteLine($"[GET] Executing SQL: {sql}");
            var results = _dapper.LoadDataWithParameters<AudioTrack>(sql, sqlParameters);

            Console.WriteLine($"[GET] Found {results.Count()} tracks");
            foreach (var track in results)
            {
                Console.WriteLine($"[GET] Track {track.AudioTrackId}: '{track.SongName}' - BlobUrl: '{track.SongBlobUrl}'");
            }

            return results;
        }

        [HttpGet("GetMyAudioTracks")]
        public IEnumerable<AudioTrack> GetMyAudioTracks()
        {
            var userId = this.User.FindFirst("userId")?.Value;
            Console.WriteLine($"[GET] GetMyAudioTracks called for userId: {userId}");

            string sql = @"EXEC dbo.spAudioTracks_Get @UserId = @UserIdParameter";
            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameter", userId, DbType.Int32);

            Console.WriteLine($"[GET] Executing SQL: {sql}");
            var results = _dapper.LoadDataWithParameters<AudioTrack>(sql, sqlParameters);

            Console.WriteLine($"[GET] Found {results.Count()} tracks for user {userId}");
            foreach (var track in results)
            {
                Console.WriteLine($"[GET] Track {track.AudioTrackId}: '{track.SongName}' - BlobUrl: '{track.SongBlobUrl}'");
            }

            return results;
        }

        [HttpPut("UpsertAudioTrack")]
        public async Task<IActionResult> UpsertAudioTrack(AudioTrackUpsertRequest request)
        {
            try
            {
                string blobUrl = string.Empty;
                Console.WriteLine($"[UPSERT] Starting upsert for song: {request.SongName}");
                Console.WriteLine($"[UPSERT] SongData length: {request.SongData?.Length ?? 0}");

                // Upload audio file to blob storage if provided
                if (request.SongData != null && request.SongData.Length > 0)
                {
                    var fileName = $"{request.SongName}_{DateTime.UtcNow:yyyyMMddHHmmss}.mp3";
                    Console.WriteLine($"[UPSERT] Uploading to blob storage with filename: {fileName}");
                    blobUrl = await _blobStorageService.UploadAudioFileAsync(fileName, request.SongData);
                    Console.WriteLine($"[UPSERT] Blob URL generated: {blobUrl}");
                }
                else
                {
                    Console.WriteLine($"[UPSERT] No SongData provided, blobUrl will be empty");
                }

                string sql = @"EXEC dbo.spAudioTracks_Upsert
                @UserId = @UserIdParameter,
                @SongName = @SongNameParameter,
                @SongTip = @SongTipParameter,
                @SongKey = @SongKeyParameter,
                @SongChords = @SongChordsParameter,
                @SongInstrument = @SongInstrumentParameter,
                @SongDifficulty = @SongDifficultyParameter,
                @SongBlobUrl = @SongBlobUrlParameter";

                DynamicParameters sqlParameters = new DynamicParameters();
                sqlParameters.Add("@UserIdParameter", this.User.FindFirst("userId")?.Value, DbType.Int32);
                sqlParameters.Add("@SongNameParameter", request.SongName, DbType.String);
                sqlParameters.Add("@SongTipParameter", request.SongTip, DbType.String);
                sqlParameters.Add("@SongKeyParameter", request.SongKey, DbType.String);
                sqlParameters.Add("@SongChordsParameter", request.SongChords, DbType.String);
                sqlParameters.Add("@SongInstrumentParameter", request.SongInstrument, DbType.String);
                sqlParameters.Add("@SongDifficultyParameter", request.SongDifficulty, DbType.String);
                sqlParameters.Add("@SongBlobUrlParameter", blobUrl, DbType.String);

                Console.WriteLine($"[UPSERT] About to save to DB with blobUrl: '{blobUrl}'");

                if (request.AudioTrackId > 0)
                {
                    sql += ", @AudioTrackId = @AudioTrackIdParameter";
                    sqlParameters.Add("@AudioTrackIdParameter", request.AudioTrackId, DbType.Int32);
                    Console.WriteLine($"[UPSERT] Updating existing track ID: {request.AudioTrackId}");
                }
                else
                {
                    Console.WriteLine($"[UPSERT] Creating new track");
                }

                if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
                {
                    Console.WriteLine($"[UPSERT] SUCCESS: Track saved with blobUrl: '{blobUrl}'");
                    return Ok(new { message = "Audio track saved successfully", blobUrl });
                }

                Console.WriteLine($"[UPSERT] FAILED: Database operation returned false");
                throw new Exception("Failed to upsert audio track");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UPSERT] EXCEPTION: {ex.Message}");
                return BadRequest(new { message = "Failed to save audio track", error = ex.Message });
            }
        }

        [HttpDelete("DeleteAudioTrack/{audioTrackId}")]
        public async Task<IActionResult> DeleteAudioTrack(int audioTrackId)
        {
            var currentUserId = this.User.FindFirst("userId")?.Value;
            Console.WriteLine($"[DELETE] Attempting to delete AudioTrack {audioTrackId} by User {currentUserId}");

            if (string.IsNullOrEmpty(currentUserId))
            {
                Console.WriteLine($"[DELETE] ERROR: No userId found in token for AudioTrack {audioTrackId}");
                return Unauthorized("User not authenticated");
            }

            try
            {
                // First, get the blob URL from the database
                string getSql = @"EXEC dbo.spAudioTracks_Get @AudioTrackId = @AudioTrackIdParameter";
                DynamicParameters getParameters = new DynamicParameters();
                getParameters.Add("@AudioTrackIdParameter", audioTrackId, DbType.Int32);

                var audioTrack = _dapper.LoadDataWithParameters<AudioTrack>(getSql, getParameters).FirstOrDefault();

                // Delete from database
                string deleteSql = @"EXEC dbo.spAudioTracks_Delete @AudioTrackId = @AudioTrackIdParameter, @UserId = @UserIdParameter";
                DynamicParameters deleteParameters = new DynamicParameters();
                deleteParameters.Add("@AudioTrackIdParameter", audioTrackId, DbType.Int32);
                deleteParameters.Add("@UserIdParameter", currentUserId, DbType.Int32);

                Console.WriteLine($"[DELETE] Executing SQL: {deleteSql} with AudioTrackId={audioTrackId}, UserId={currentUserId}");

                if (_dapper.ExecuteSqlWithParameters(deleteSql, deleteParameters))
                {
                    // Delete from blob storage if URL exists
                    if (audioTrack != null && !string.IsNullOrEmpty(audioTrack.SongBlobUrl))
                    {
                        await _blobStorageService.DeleteAudioFileAsync(audioTrack.SongBlobUrl);
                        Console.WriteLine($"[DELETE] Blob deleted: {audioTrack.SongBlobUrl}");
                    }

                    Console.WriteLine($"[DELETE] SUCCESS: AudioTrack {audioTrackId} deleted by User {currentUserId}");
                    return Ok(new { message = "Audio track deleted successfully" });
                }

                Console.WriteLine($"[DELETE] FAILED: AudioTrack {audioTrackId} not deleted by User {currentUserId}");
                Console.WriteLine($"[DELETE] Possible reasons: Track doesn't exist, User doesn't own track, or database error");
                return BadRequest(new { message = "Failed to delete audio track" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELETE] EXCEPTION: {ex.Message}");
                return BadRequest(new { message = "Error deleting audio track", error = ex.Message });
            }
        }
    }
}