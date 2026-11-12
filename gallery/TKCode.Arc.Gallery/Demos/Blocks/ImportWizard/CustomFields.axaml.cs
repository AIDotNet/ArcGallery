using System.Globalization;
using Avalonia.Controls;

namespace TKCode.Arc.Gallery.Demos;

public partial class ImportWizardCustomFieldsDemo : UserControl
{
    public ImportWizardCustomFieldsDemo()
    {
        InitializeComponent();

        Wizard.Targets =
        [
            new ImportTarget
            {
                Key = "sku",
                Label = "SKU",
                IsRequired = true,
                IsKey = true,
                Headers = ["item code", "product code"],
                Synonyms = ["code", "article"],
                Validate = value => value.Trim().Length == 0 ? "Add a SKU"
                    : value != value.Trim().ToUpperInvariant() ? "Use capitals, no spaces" : null,
                Fix = value => value.Trim().Length > 0 ? value.Trim().ToUpperInvariant() : null,
            },
            new ImportTarget
            {
                Key = "title",
                Label = "Title",
                IsRequired = true,
                Synonyms = ["name", "product"],
                Validate = value => string.IsNullOrWhiteSpace(value) ? "Add a title" : null,
            },
            new ImportTarget
            {
                Key = "price",
                Label = "Price",
                Synonyms = ["cost", "amount"],
                Validate = value => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _) ? null : "Not a number",
                Fix = value => decimal.TryParse(value.Replace("$", string.Empty).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
                    ? price.ToString("0.00", CultureInfo.InvariantCulture) : null,
            },
            new ImportTarget
            {
                Key = "stock",
                Label = "Stock",
                Synonyms = ["quantity", "qty", "inventory"],
                Validate = value => int.TryParse(value, out var count) && count >= 0 ? null : "Expected a whole number",
            },
        ];
        Wizard.ExistingKeys = new HashSet<string> { "LMP-001", "DSK-210" };
        Wizard.SampleCsv = """
            Item code,Name,Cost,Qty
            LMP-001,Brass desk lamp,48.00,12
            lmp-002 ,Linen lamp shade,$19.50,30
            DSK-210,Oak standing desk,620.00,4
            CHR-114,,145.00,9
            CHR-115,Felt seat pad,12,many
            SHL-300,Wall shelf,85.00,16
            """;
    }
}
