namespace Day16.Models
{
    public class CreateHostCommand
    {
        public CreateHostCommand(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; }
        public string LastName { get; }
    }
}