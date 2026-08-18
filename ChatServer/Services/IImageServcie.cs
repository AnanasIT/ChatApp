using MessageDTO;
using ServiceResultModel;

namespace IImageServiceModel;
public interface IImageservice
{
    Task<ServiceResult<string>> SaveImageAsync(Stream fileStream, string fileName);
    Task<ServiceResult<bool>> DeleteImageAsync(string imageUrl);
}