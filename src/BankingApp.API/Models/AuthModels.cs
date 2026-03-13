namespace BankingApp.API.Models;

public record RegisterRequest(string Email, string Password, string Role = "Customer");
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string Role, DateTime ExpiresAt);
