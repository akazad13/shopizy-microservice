using ErrorOr;

namespace Shopizy.Catelog.API.ExternalServices.MediaUploader;

public interface IMediaUploader
{
    Task<ErrorOr<PhotoUploadResult>> UploadPhotoAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    );
    Task<ErrorOr<Success>> DeletePhotoAsync(string publicId);
}
