namespace UserService.Models
{
    public class ClientModel
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientDescription { get; set; }

        public bool ToUpdateSecret { get; set; }
    }
}
