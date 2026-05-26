using HouseOfChess.Platform.Infrastructure.Contracts.PgnExport;

namespace HouseOfChess.Platform.Infrastructure.Services.PgnExport;

public interface IPgnExportService
{
    string Build(PgnExportInputs inputs);
}
