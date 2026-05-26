using System.ComponentModel.DataAnnotations;

namespace HouseOfChess.Platform.Infrastructure.Contracts.Game;

public sealed record MoveRequest(
    [Required, RegularExpression("^[a-h][1-8][a-h][1-8][qrbn]?$")]
    string Uci);
