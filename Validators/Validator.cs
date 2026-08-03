using FluentValidation;

using RegisterDTO;
using LoginDTO;
using MessageDTO;
using RoomModel;

namespace ValidatorModel;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestUser>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно!")
            .MinimumLength(3).WithMessage("Имя пользователя минимум 3 символа!")
            .MaximumLength(50).WithMessage("Имя пользователя максимум 50 символов!");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email пользователя обязателен!")
            .EmailAddress().WithMessage("Некорректный email!");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен!")
            .MinimumLength(6).WithMessage("Пароль минимум 6 символов!");
    }
}


public class LoginRequestValidator : AbstractValidator<LoginRequestUser>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязательно!");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен!");
    }
}


public class MessageValidator : AbstractValidator<string>
{
    public MessageValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Сообщение не может быть пустым!")
            .MaximumLength(2000).WithMessage("Максимум 2000 символов!");
    }
}


public class RoomValidator : AbstractValidator<string>
{
    public RoomValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Название комнаты обязательно!")
            .MinimumLength(2).WithMessage("Название комнаты минимум 2 символа!")
            .MaximumLength(50).WithMessage("Название комнаты максимум 50 символов!");
    }
}