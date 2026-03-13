namespace BankingApp.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string Address,
    DateTime CreatedAt);

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string Address);

public record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string Phone,
    string Address);
