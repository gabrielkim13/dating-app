using System.Threading.Tasks;
using api.Helpers;
using api.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace api.Services
{
  public class PhotoService : IPhotoService
  {
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
      var account = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);

      _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
      var uploadResult = new ImageUploadResult();

      if (file.Length > 0)
      {
        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
          File = new FileDescription(file.FileName, stream),
          Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
        };

        uploadResult = await _cloudinary.UploadAsync(uploadParams);
      }

      return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
      var deleteParams = new DeletionParams(publicId);

      var result = await _cloudinary.DestroyAsync(deleteParams);

      return result;
    }
  }
}
