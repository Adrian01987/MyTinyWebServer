using System.ComponentModel.DataAnnotations;

namespace CustomerApi
{
    /// <summary>
    /// Represents a customer entity with basic identification and name information.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Parameterless constructor required for JSON deserialization.
        /// </summary>
        public Customer() { }

        /// <summary>
        /// Creates a new customer with the specified id and name.
        /// </summary>
        /// <param name="id">The unique identifier for the customer.</param>
        /// <param name="name">The customer's name.</param>
        public Customer(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the customer.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the customer's name.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}
