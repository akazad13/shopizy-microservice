using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ErrorOr;

namespace Shopizy.Catelog.API.ExternalServices.MediaUploader.CloudinaryService;

public class CloudinaryMediaUploader(ICloudinary cloudinary) : IMediaUploader
{
    private readonly ICloudinary _cloudinary = cloudinary;

    public async Task<ErrorOr<PhotoUploadResult>> UploadPhotoAsync(
        IFormFile file,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (file.Length > 0)
            {
                using Stream stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.Name, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill"),
                };
                ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

                return uploadResult.Error switch
                {
                    null => new PhotoUploadResult(
                        uploadResult.Url.ToString(),
                        uploadResult.PublicId
                    ),
                    _ => ErrorOr.Error.Failure(description: uploadResult.Error.Message),
                };
            }
            return ErrorOr.Error.Failure(description: "File not found!");
        }
        catch (Exception ex)
        {
            return ErrorOr.Error.Failure(description: ex.Message);
        }
    }

    public async Task<ErrorOr<Success>> DeletePhotoAsync(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId);
            DeletionResult result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result switch
            {
                "ok" => Result.Success,
                _ => ErrorOr.Error.Failure(description: result.Error.Message),
            };
        }
        catch (Exception ex)
        {
            return ErrorOr.Error.Failure(description: ex.Message);
        }
    }
}
