namespace HouseOfChess.Platform.Infrastructure.Contracts.Game;

public sealed record GameMoveEntry(int Ply, string San, string Uci);
