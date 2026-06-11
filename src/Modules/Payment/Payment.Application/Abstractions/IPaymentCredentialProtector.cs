namespace Payment.Application.Abstractions;

public interface IPaymentCredentialProtector
{
    string Protect(string value);
    string Unprotect(string protectedValue);
}
