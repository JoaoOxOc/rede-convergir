namespace UserService.Models.Users
{
    public class AssociationTypeModel
    {
        public int? AssociationTypeId { get; set; }
        public string? Code { get; set; }

        public string? Label { get; set; }
        public string? Description { get; set; }
        public bool? VatRequired { get; set; }
    }
}
