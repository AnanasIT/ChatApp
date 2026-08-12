namespace ServiceResultModel;

public class ServiceResult<T>
{
    public string? Error {get; set;} = string.Empty;
    public bool IsSucces {get; set;}
    public T? Data {get; set;}

    public static ServiceResult<T> Success(T data) =>
        new() {IsSucces = true, Data = data};
    
    public static ServiceResult<T> Failure(string Error) =>
        new() {IsSucces = false, Error = Error};
}