namespace ImageServiceModel;

using MessageDTO;
using IImageServiceModel;
using ServiceResultModel;

public class ImageService : IImageservice
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger) {
        _env = env;
        _logger = logger;
    }

    public async Task<ServiceResult<string>> SaveImageAsync(Stream fileStream, string fileName)
    {
        try
        {
            _logger.LogInformation("📷🖼️ Запрос на отправку картинки");

            var imagesDir = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);
            
            var extensions = Path.GetExtension(fileName);
            var newFileName = $"{DateTime.Now.Ticks}_{Guid.NewGuid():N}{extensions}";
            var filePath = Path.Combine(imagesDir, newFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            var imageUrl = $"/images/{newFileName}";
            return ServiceResult<string>.Success(imageUrl);
        }

        catch(Exception ex)
        {
            _logger.LogError("Ошибка сохранения картинки");
            return ServiceResult<string>.Failure($"❌ Ошибка: {ex.Message}");
        }
    }


    public async Task<ServiceResult<bool>> DeleteImageAsync(string imageUrl)
    {
        _logger.LogInformation($"Запрос на удаление картинки {imageUrl}");
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) {
                return ServiceResult<bool>.Success(false);
            }

            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }

            return ServiceResult<bool>.Success(true);
        }

        catch (Exception ex)
        {
            _logger.LogError($"❌ Ошибка удаления: {ex.Message}");
            return ServiceResult<bool>.Failure($"Ошибка удаления: {ex.Message}");
        }
    }
}