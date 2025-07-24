SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spAudioTrack_Delete]
    @AudioTrackId INT
    , @UserId INT 
AS
BEGIN
    DELETE FROM dbo.AudioTracks 
        WHERE AudioTrackId = @AudioTrackId
            AND UserId = @UserId
END
GO


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spAudioTracks_Get]
/*EXEC dbo.spAudioTracks_Get @UserId = 1003, @SearchValue='Second'*/
/*EXEC dbo.spAudioTracks_Get @AudioTrackId = 2*/
    @UserId INT = NULL
    , @SearchValue NVARCHAR(MAX) = NULL
    , @AudioTrackId INT = NULL
AS
BEGIN
    SELECT * FROM dbo.AudioTracks AS AudioTracks
        WHERE AudioTracks.UserId = ISNULL(@UserId, AudioTracks.UserId)
            AND AudioTracks.AudioTrackId = ISNULL(@AudioTrackId, AudioTracks.AudioTrackId)
            AND (@SearchValue IS NULL
                OR AudioTracks.SongName LIKE '%' + @SearchValue + '%')
END
GO


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spAudioTracks_Upsert]
    @UserId INT
    , @AudioTrackId INT = NULL
    , @SongName NVARCHAR(255)
    , @SongTip NVARCHAR(MAX)
    , @SongKey NVARCHAR(50)
    , @SongChords NVARCHAR(MAX)
    , @SongInstrument NVARCHAR(100)
    , @SongDifficulty NVARCHAR(50)
    , @SongData VARBINARY(MAX)
AS
BEGIN
    IF @AudioTrackId IS NULL OR NOT EXISTS (SELECT * FROM dbo.AudioTracks WHERE AudioTrackId = @AudioTrackId)
        BEGIN
            INSERT INTO dbo.AudioTracks(
                [UserId],
                [SongName],
                [SongTip],
                [SongKey],
                [SongChords],
                [SongInstrument],
                [SongDifficulty],
                [SongData],
                [CreatedAt],
                [UpdatedAt]
            ) VALUES (
                @UserId,
                @SongName,
                @SongTip,
                @SongKey,
                @SongChords,
                @SongInstrument,
                @SongDifficulty,
                @SongData,
                GETDATE(),
                GETDATE()
            )
        END
    ELSE
        BEGIN
            UPDATE dbo.AudioTracks 
                SET SongName = @SongName,
                    SongTip = @SongTip,
                    SongKey = @SongKey,
                    SongChords = @SongChords,
                    SongInstrument = @SongInstrument,
                    SongDifficulty = @SongDifficulty,
                    SongData = @SongData,
                    UpdatedAt = GETDATE()
                WHERE AudioTrackId = @AudioTrackId
                    AND UserId = @UserId
        END
END
GO
