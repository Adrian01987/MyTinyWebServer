using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TinyWebServerLib.Attributes;
using TinyWebServerLib.Http;

namespace CustomerApi;

[ApiController]
public class CustomersController
{

    [HttpGet("customers")]
    public Task<HttpResponse> GetCustomers()
    {
        var customers = new List<Customer>
        {
            new(1, "Alice"),
            new(2,"Bob")
        };

        string json = JsonSerializer.Serialize(customers);
        HttpResponse response = new(200, [],json);

        response.Headers["Content-Type"] = "application/json";
        return Task.FromResult(response);
    }

    [HttpGet("customers/{id}")]
    public Task<HttpResponse> GetCustomerById(int id)
    {
        var customer = new Customer(id, $"Customer {id}");

        string json = JsonSerializer.Serialize(customer);
        var response = new HttpResponse(200, [], json);
        response.Headers["Content-Type"] = "application/json";
        return Task.FromResult(response);
    }

    [HttpPost("customers")]
    public Task<HttpResponse> CreateCustomer(Customer customer)
    {
        var context = new ValidationContext(customer);
        var results = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(customer, context, results, true);
        if (!isValid)
        {
            string errors = string.Join("; ", results.Select(r => r.ErrorMessage));
            HttpResponse errorResponse = new(400, [],errors);

            errorResponse.Headers["Content-Type"] = "text/plain";
            return Task.FromResult(errorResponse);
        }

        HttpResponse response = new (201, [], "Customer created");

        response.Headers["Content-Type"] = "text/plain";
        return Task.FromResult(response);
    }
}
