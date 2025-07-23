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

        [HttpGet("AudioFiles")]
        public IEnumerable<AudioFile> GetAudioFiles()
        {
            return _dapper.LoadData<AudioFile>("SELECT * FROM AudioFiles");
        }

        [HttpPut("UpsertAudioFile")]
        public IActionResult UpsertAudioFile(AudioFile audioFile)
        {
            string sql = "";
            //this needs to be built out similar to the post one from course
            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to upsert audio file");
        }
        [HttpDelete("AudioFile/{audioFileId}")]
        public IActionResult DeleteAudioFile(int postId)
        {
            string sql = @"EXEC dbo.spAudioFile_Upsert @AudioFileId = " +
                    audioFile.Id.ToString() +
                    ", @FileName = '" + audioFile.FileName +
                    "', @FileData = @FileData";
            //This SP does not currently exist

            if (_dapper.ExecuteSql(sql))
            {
                return Ok();
            }
            throw new Exception("Failed to delete audio file");
        }
    }
}