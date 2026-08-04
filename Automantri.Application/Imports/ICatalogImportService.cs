namespace Automantri.Application.Imports;

public interface ICatalogImportService
{
    Task<CatalogImportPreviewResultDto> PreviewAsync(
        Stream workbookStream,
        string fileName,
        CatalogImportPreviewRequest request,
        CancellationToken cancellationToken);

    Task<CatalogImportCommitResultDto> CommitAsync(
        CatalogImportCommitRequest request,
        CancellationToken cancellationToken);
}
