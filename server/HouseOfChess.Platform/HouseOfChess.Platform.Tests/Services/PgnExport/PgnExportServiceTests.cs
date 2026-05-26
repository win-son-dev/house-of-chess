using HouseOfChess.Platform.Infrastructure.Contracts.PgnExport;
using HouseOfChess.Platform.Infrastructure.Options;
using HouseOfChess.Platform.Services.PgnExport;
using Microsoft.Extensions.Options;

namespace HouseOfChess.Platform.Tests.Services.PgnExport;

public class PgnExportServiceTests
{
    private static PgnExportService Build(PgnExportOptions? options = null) =>
        new(Options.Create(options ?? new PgnExportOptions()));

    private static PgnExportInputs Inputs(
        string result,
        IReadOnlyList<string> sans,
        string white = "alice",
        string black = "bob",
        string timeControl = "5+0",
        DateTime? startedAtUtc = null) =>
        new(white, black, timeControl, startedAtUtc ?? new DateTime(2026, 5, 26, 0, 0, 0, DateTimeKind.Utc), result, sans);

    [Fact]
    public void Build_ScholarsMate_ProducesAllSevenTagsPlusTimeControlAndMovetext()
    {
        // 1. e4 e5 2. Bc4 Nc6 3. Qh5 Nf6 4. Qxf7#
        var sut = Build();
        var inputs = Inputs("1-0", ["e4", "e5", "Bc4", "Nc6", "Qh5", "Nf6", "Qxf7#"]);

        var pgn = sut.Build(inputs);

        Assert.Contains("[Event \"House of Chess\"]",  pgn);
        Assert.Contains("[Site \"House of Chess\"]",   pgn);
        Assert.Contains("[Date \"2026.05.26\"]",       pgn);
        Assert.Contains("[Round \"-\"]",               pgn);
        Assert.Contains("[White \"alice\"]",           pgn);
        Assert.Contains("[Black \"bob\"]",             pgn);
        Assert.Contains("[Result \"1-0\"]",            pgn);
        Assert.Contains("[TimeControl \"300+0\"]",     pgn);
        Assert.Contains("1. e4 e5 2. Bc4 Nc6 3. Qh5 Nf6 4. Qxf7# 1-0", pgn);
    }

    [Fact]
    public void Build_FoolsMate_BlackWinsAndMovetextMatches()
    {
        // 1. f3 e5 2. g4 Qh4#
        var sut = Build();
        var inputs = Inputs("0-1", ["f3", "e5", "g4", "Qh4#"]);

        var pgn = sut.Build(inputs);

        Assert.Contains("[Result \"0-1\"]", pgn);
        Assert.Contains("1. f3 e5 2. g4 Qh4# 0-1", pgn);
    }

    [Fact]
    public void Build_Draw_UsesDrawToken()
    {
        var sut = Build();
        var inputs = Inputs("1/2-1/2", ["e4", "e5"]);

        var pgn = sut.Build(inputs);

        Assert.Contains("[Result \"1/2-1/2\"]", pgn);
        Assert.EndsWith("1/2-1/2", pgn);
    }

    [Fact]
    public void Build_NoMoves_EmitsHeadersAndResultOnly()
    {
        var sut = Build();
        var inputs = Inputs("1-0", []);

        var pgn = sut.Build(inputs);

        Assert.Contains("[White \"alice\"]", pgn);
        Assert.EndsWith("\n1-0", pgn);
    }

    [Fact]
    public void Build_TimeControl_FischerIncrement_ConvertsMinutesToSeconds()
    {
        var sut = Build();
        var inputs = Inputs("1-0", ["e4"], timeControl: "10+5");

        var pgn = sut.Build(inputs);

        Assert.Contains("[TimeControl \"600+5\"]", pgn);
    }

    [Fact]
    public void Build_TimeControl_UnparseableSpec_PassedThroughVerbatim()
    {
        var sut = Build();
        var inputs = Inputs("1-0", ["e4"], timeControl: "untimed");

        var pgn = sut.Build(inputs);

        Assert.Contains("[TimeControl \"untimed\"]", pgn);
    }

    [Fact]
    public void Build_DateTag_FormattedAsYyyyDotMmDotDd()
    {
        var sut = Build();
        var inputs = Inputs("1-0", ["e4"], startedAtUtc: new DateTime(2026, 1, 9, 14, 30, 0, DateTimeKind.Utc));

        var pgn = sut.Build(inputs);

        Assert.Contains("[Date \"2026.01.09\"]", pgn);
    }

    [Fact]
    public void Build_HeaderOverrides_HonorsOptions()
    {
        var sut = Build(new PgnExportOptions { Event = "Casual", Site = "https://hoc.example", Round = "3" });
        var inputs = Inputs("1-0", ["e4"]);

        var pgn = sut.Build(inputs);

        Assert.Contains("[Event \"Casual\"]",            pgn);
        Assert.Contains("[Site \"https://hoc.example\"]", pgn);
        Assert.Contains("[Round \"3\"]",                  pgn);
    }
}
